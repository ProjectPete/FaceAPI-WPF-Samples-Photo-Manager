using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ClientLibrary.Controls
{
    /// <summary>
    /// Interaction logic for ManageGroupsControl.xaml
    /// </summary>
    public partial class ManageGroupsControl : UserControl, INotifyPropertyChanged
    {
        /// <summary>
        /// The number of faces to pull
        /// </summary>
        private const int NumberOfFacesToPull = 5;

        /// <summary>
        /// The face service client
        /// </summary>
        private FaceServiceClient faceServiceClient;

        /// <summary>
        /// The main window
        /// </summary>
        private MainWindow mainWindow;

        /// <summary>
        /// The database provider layer
        /// </summary>
        private Data.SqlDataProvider db = new Data.SqlDataProvider();

        /// <summary>
        /// max concurrent process number for client query.
        /// </summary>
        private int _maxConcurrentProcesses;

        private ObservableCollection<LargePersonGroupExtended> _faceGroups = new ObservableCollection<LargePersonGroupExtended>();

        private ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face> _selectedFaces 
            = new ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face>();

        LargePersonGroupExtended _selectedGroup;

        public ObservableCollection<LargePersonGroupExtended> FaceGroups
        {
            get
            {
                return _faceGroups;
            }
        }

        public LargePersonGroupExtended SelectedGroup
        {
            get
            {
                return _selectedGroup;
            }

            set
            {
                _selectedGroup = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedGroup"));
                    GetPeopleForSelectedGroup();
                }
            }
        }

        private void CheckPeopleInDatabase()
        {
            foreach(var person in SelectedGroup.GroupPersons)
            {
                var dbPerson = db.GetPerson(person.Person.PersonId);
                if (dbPerson == null)
                {
                    db.AddPerson(person.Person.PersonId, person.Person.Name, person.Person.UserData);
                }
            }
        }

        private async Task GetPeopleForSelectedGroup()
        {
            if (SelectedGroup == null)
            {
                return;
            }

            MainWindow.Log("Loading group persons...");
            SelectedGroup.GroupPersons.Clear();

            Microsoft.ProjectOxford.Face.Contract.Person[] peops = null;
            
            while(true)
            {
                try
                {
                    // ListPersonsInLargePersonGroupAsync also has skip/take overrides
                    peops = await faceServiceClient.ListPersonsInLargePersonGroupAsync(SelectedGroup.Group.LargePersonGroupId);
                    break;
                }
                catch (Exception e)
                {
                    MainWindow.Log($"API rate limit exceeded, retrying");
                    await Task.Delay(1000);
                }
            }

            if (peops == null)
            {
                MainWindow.Log($"failed to get Persons in group");
                return;
            }

            foreach (var p in peops)
            {
                var person = new Controls.PersonExtended { Person = p };
                person.PersonFilesDbCount = db.GetFileCountForPersonId(p.PersonId);
                this.Dispatcher.Invoke(() =>
                {
                    SelectedGroup.GroupPersons.Add(person);
                });

                // Initially loading just one, to save on API calls
                var guidList = new ConcurrentBag<Guid>(person.Person.PersistedFaceIds.Take(1)); 
                
                await GetFacesFromServerAsync(person, guidList);
            }

            MainWindow.Log("Finished loading group persons");
            CheckPeopleInDatabase();
        }

        /// <summary>
        /// Gets trained faces from the API.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="guidList">The unique identifier list.</param>
        /// <returns></returns>
        private async Task GetFacesFromServerAsync(PersonExtended person, ConcurrentBag<Guid> guidList)
        {
            var tasks = new List<Task>();
            Guid guid;
            while (guidList.TryTake(out guid))
            {
                tasks.Add(Task.Factory.StartNew((object inParams) =>
                {
                    var prm = (Tuple<PersonExtended, Guid>)inParams;
                    try
                    {
                        var face = faceServiceClient.GetPersonFaceInLargePersonGroupAsync(SelectedGroup.Group.LargePersonGroupId, prm.Item1.Person.PersonId, prm.Item2).Result;

                        this.Dispatcher.Invoke(
                            new Action<ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face>, string, PersistedFace>(UIHelper.UpdateFace),
                            prm.Item1.Faces,
                            face.UserData,
                            face);
                    }
                    catch (FaceAPIException e)
                    {
                        // if operation conflict, retry.
                        if (e.ErrorCode.Equals("ConcurrentOperationConflict"))
                        {
                            guidList.Add(guid);
                        }
                    }
                    catch (Exception ex)
                    {
                        guidList.Add(guid);
                        this.Dispatcher.Invoke(() =>
                        {
                            MainWindow.Log($"Rate limit exceeded, Re-queuing in 1 second");
                            Task.Delay(1000).Wait();
                        });
                    }
                }, new Tuple<PersonExtended, Guid>(person, guid)));

                if (tasks.Count >= _maxConcurrentProcesses || guidList.IsEmpty)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }
            }

            await Task.WhenAll(tasks);
            tasks.Clear();

            return;
        }

        public ManageGroupsControl()
        {
            InitializeComponent();
            Loaded += ManageGroupsControl_Loaded;
            _maxConcurrentProcesses = 4;
        }

        private async void ManageGroupsControl_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindow = Window.GetWindow(this) as MainWindow;
            string subscriptionKey = mainWindow._scenariosControl.SubscriptionKey;
            string endpoint = mainWindow._scenariosControl.SubscriptionEndpoint;

            faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);

            await LoadGroups();
        }

        private async Task LoadGroups()
        {
            var tries = 30;
            FaceGroups.Clear();

            while (tries-- > 0)
            {
                try
                {
                    var groups = await faceServiceClient.ListLargePersonGroupsAsync();
                    foreach (var grp in groups)
                    {
                        FaceGroups.Add(new LargePersonGroupExtended { Group = grp });
                    }
                    MainWindow.Log("Found {0} groups.", groups.Length);
                    break;
                }
                catch (Exception exc)
                {
                    MainWindow.Log($"Error loading groups: {exc.Message}. Retry in 1 second.");
                    await Task.Delay(1000);
                }
            }
            if (tries == 0)
            {
                MainWindow.Log($"Failed to load the groups after 30 tries.");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void btnFolderScan_Click(object sender, RoutedEventArgs e)
        {
            var person = SelectedGroup.SelectedPerson;
            var ctrl = new ScanFolderControl(_selectedGroup, mainWindow);
            var win = new PopupWindow(ctrl, $"Scan folders for matches with {_selectedGroup.Group.Name}");
            win.Show();

            //page.ContentGrid.Content = new ScanFolderControl(_selectedGroup);
        }

        private async void btnAddGroup_Click(object sender, RoutedEventArgs e)
        {
            var groupId = Guid.NewGuid().ToString();
            await faceServiceClient.CreateLargePersonGroupAsync(groupId, groupId);
            await LoadGroups();
            SelectedGroup = FaceGroups.Where(a => a.Group.LargePersonGroupId == groupId).SingleOrDefault();
        }

        private async void btnUpdateGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await faceServiceClient.UpdateLargePersonGroupAsync(SelectedGroup.Group.LargePersonGroupId, SelectedGroup.Group.Name, SelectedGroup.Group.UserData);
                MainWindow.Log($"Changes to the selected group were saved successfully");
            }
            catch (Exception ex)
            {
                MainWindow.Log($"Error updating group: {ex.Message}");
            }
        }

        private async void btnDeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure you want to delete this group and database matches?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    while(true)
                    {
                        try
                        {
                            await faceServiceClient.DeleteLargePersonGroupAsync(SelectedGroup.Group.LargePersonGroupId);
                            break;
                        }
                        catch (Exception ex)
                        {
                            MainWindow.Log($"Error deleting group: {ex.Message}, retrying");
                            await Task.Delay(1000);
                        }
                    }
                    db.RemovePersonsForGroup(SelectedGroup.Group.LargePersonGroupId);
                    //Dispatcher.Invoke(() => 
                    //{
                        FaceGroups.Remove(SelectedGroup);
                        SelectedGroup = null;
                        MainWindow.Log($"Selected group deleted successfully");
                  //  });
                }
                catch (Exception ex)
                {
                    MainWindow.Log($"Error deleting group: {ex.Message}");
                }
            }
        }

        private void btnShowFiles_Click(object sender, RoutedEventArgs e)
        {
            var person = SelectedGroup.SelectedPerson;
            var ctrl = new ShowPersonMatchedFilesControl(person);
            var win = new PopupWindow(ctrl, $"Matched files for {person.Person.Name}");
            win.Show();
        }

        private async void btnShowMore_Click(object sender, RoutedEventArgs e)
        {
            var person = SelectedGroup.SelectedPerson;
            var alreadyTaken = person.Faces.Count;
            var maxAvailable = person.Person.PersistedFaceIds.Count();
            if (alreadyTaken < maxAvailable)
            {
                var guidList = new ConcurrentBag<Guid>(person.Person.PersistedFaceIds.Skip(alreadyTaken).Take(NumberOfFacesToPull));
                await GetFacesFromServerAsync(person, guidList);
            }
            else
            {
                MainWindow.Log($"No more images stored in the service for {person.Person.Name}");
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach(var p in SelectedGroup.GroupPersons)
            {
                p.IsSelected = false;
            }
            var grid = sender as Grid;
            var person = grid.DataContext as PersonExtended;
            SelectedGroup.SelectedPerson = person;
            person.IsSelected = true;
        }
    }

    public class NotNullVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HasItemsVisibilityConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return ((int)value) > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

