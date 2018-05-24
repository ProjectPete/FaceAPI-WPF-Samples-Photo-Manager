using Microsoft.ProjectOxford.Face;
using Photo_Detect_Catalogue_Search_WPF_App.Data;
using Photo_Detect_Catalogue_Search_WPF_App.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace Photo_Detect_Catalogue_Search_WPF_App.Controls
{
    /// <summary>
    /// Interaction logic for ShowPersonMatchedFilesControl.xaml
    /// </summary>
    public partial class ShowPersonMatchedFilesControl : UserControl, INotifyPropertyChanged
    {
        PersonExtended _person;
        SqlDataProvider db = new SqlDataProvider();

        private ObservableCollection<PicturePerson> _matchedFiles = new ObservableCollection<PicturePerson>();

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<PicturePerson> MatchedFiles
        {
            get
            {
                return _matchedFiles;
            }
            set
            {
                _matchedFiles = value;
                PropertyChanged(this, new PropertyChangedEventArgs("MatchedFiles"));
            }
        }


        public ShowPersonMatchedFilesControl(PersonExtended person)
        {
            _person = person;
            InitializeComponent();
            txtTitle.Text = $"Listing matched files for {person.Person.Name}";
            Loaded += ShowPersonMatchedFilesControl_Loaded;
        }

        private void ShowPersonMatchedFilesControl_Loaded(object sender, RoutedEventArgs e)
        {
            var people = db.GetFilesForPersonId(_person.Person.PersonId);
            foreach(var person in people)
            {
                // add value
                MatchedFiles.Add(person);
            }
        }

        private void btnOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            var person = lstFiles.SelectedItem as PicturePerson;
            var directory = System.IO.Path.GetDirectoryName(person.PictureFile.FilePath);

            var runExplorer = new System.Diagnostics.ProcessStartInfo();
            runExplorer.FileName = "explorer.exe";
            runExplorer.Arguments = "/select,\"" + person.PictureFile.FilePath + "\"";
            System.Diagnostics.Process.Start(runExplorer);
        }
    }
}
