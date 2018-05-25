using ClientLibrary.Data;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face.Controls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Drawing;
using System.IO;
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

namespace ClientLibrary.Controls
{
    /// <summary>
    /// Interaction logic for ScanFolderPage.xaml
    /// </summary>
    public partial class ScanFolderControl : UserControl, INotifyPropertyChanged
    {
        private string _selectedFolder;
        private int _filesCount;
        private Queue<string> _files;
        private bool _canScan;
        private LargePersonGroupExtended _scanGroup;
        private ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face> _detectedFaces = new ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face>();
        private ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face> _resultCollection = new ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face>();
        private ImageSource _selectedFile;
        private string _selectedFilePath;
        private bool _isDragging;
        private Rectangle _selectRectangle;
        private Point _selectRectangleStartPoint;
        private SqlDataProvider db = new SqlDataProvider();
        private FaceServiceClient faceServiceClient;
        private MainWindow _mainWindow;

        public Rectangle SelectRectangle
        {
            get
            {
                return _selectRectangle;
            }

            set
            {
                _selectRectangle = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectRectangle"));
                    CanScan = true;
                }
            }
        }

        public int FilesCount
        {
            get
            {
                return _filesCount;
            }

            set
            {
                _filesCount = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("FilesCount"));
                    CanScan = true;
                }
            }
        }

        public Queue<string> Files
        {
            get
            {
                return _files;
            }

            set
            {
                _files = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Files"));
                    CanScan = true;
                }
            }
        }

        public string SelecetedFolder
        {
            get
            {
                return _selectedFolder;
            }

            set
            {
                _selectedFolder = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelecetedFolder"));
                    CanScan = true;
                }
            }
        }

        public bool CanScan
        {
            get
            {
                return _canScan;
            }

            set
            {
                _canScan = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CanScan"));
                }
            }
        }
        public LargePersonGroupExtended ScanGroup
        {
            get
            {
                return _scanGroup;
            }

            set
            {
                _scanGroup = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ScanGroup"));
                }
            }
        }

        public ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face> DetectedFaces
        {
            get
            {
                return _detectedFaces;
            }
        }

        public ScanFolderControl(LargePersonGroupExtended group, MainWindow mainWindow)
        {
            _scanGroup = group;
            _mainWindow = mainWindow;
            InitializeComponent();
            Loaded += ScanFolderControl_Loaded;
        }

        private void ScanFolderControl_Loaded(object sender, RoutedEventArgs e)
        {
            string subscriptionKey = _mainWindow._scenariosControl.SubscriptionKey;
            string endpoint = _mainWindow._scenariosControl.SubscriptionEndpoint;

            faceServiceClient = new FaceServiceClient(subscriptionKey, endpoint);
        }

        public ImageSource SelectedFile
        {
            get
            {
                return _selectedFile;
            }

            set
            {
                _selectedFile = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedFile"));
                }
            }
        }

        public ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face> ResultCollection
        {
            get
            {
                return _resultCollection;
            }
        }

        public int MaxImageSize
        {
            get
            {
                return 300;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void BtnFolderSelect_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = SelecetedFolder;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (dialog.SelectedPath == null)
                {
                    return;
                }

                SelecetedFolder = dialog.SelectedPath;
                Files = new Queue<string>(Directory.GetFiles(_selectedFolder));
                FilesCount = Files.Count;
            }
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            GetNextFile();
        }

        private void GetNextFile()
        {
            DetectedFaces.Clear();
            btnNext.IsEnabled = false;

            while (Files.Count > 0)
            {
                var file = Files.Dequeue();
                var dbFile = db.GetFile(file);
                if (dbFile == null)
                {
                    ProcessFile(file);
                    break;
                }
            }
            MainWindow.Log("No more files in this folder to process");
        }

        private async void ProcessFile(string filePath)
        {
            _selectedFilePath = filePath;
            
            using (var fStream = File.OpenRead(filePath))
            {
                try
                {
                    Microsoft.ProjectOxford.Face.Contract.Face[] faces;

                    while (true)
                    {
                        try
                        {
                            faces = await faceServiceClient.DetectAsync(fStream, false, true, new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Glasses, FaceAttributeType.HeadPose, FaceAttributeType.FacialHair, FaceAttributeType.Emotion, FaceAttributeType.Hair, FaceAttributeType.Makeup, FaceAttributeType.Occlusion, FaceAttributeType.Accessories, FaceAttributeType.Noise, FaceAttributeType.Exposure, FaceAttributeType.Blur });
                            break;
                        }
                        catch (Exception exc)
                        {
                            MainWindow.Log($"Error: {exc.Message}");
                            await Task.Delay(1000);
                            // retry
                        }
                    }
                    MainWindow.Log("Response: Success. Detected {0} face(s) in {1}", faces.Length, filePath);
                    
                    if (faces.Length == 0)
                    {
                        btnNext.IsEnabled = true;
                        return;
                    }

                    var renderingImage = UIHelper.LoadImageAppliedOrientation(filePath);
                    var imageInfo = UIHelper.GetImageInfoForRendering(renderingImage);
                    SelectedFile = renderingImage;

                    foreach (var face in faces)
                    {
                        DetectedFaces.Add(new Microsoft.ProjectOxford.Face.Controls.Face()
                        {
                            ImageFile = renderingImage,
                            Left = face.FaceRectangle.Left,
                            Top = face.FaceRectangle.Top,
                            Width = face.FaceRectangle.Width,
                            Height = face.FaceRectangle.Height,
                            FaceRectangle = new FaceRectangle { Height = face.FaceRectangle.Height, Width = face.FaceRectangle.Width, Left = face.FaceRectangle.Left, Top = face.FaceRectangle.Top },
                            FaceId = face.FaceId.ToString(),
                            Age = string.Format("{0:#} years old", face.FaceAttributes.Age),
                            Gender = face.FaceAttributes.Gender,
                            HeadPose = string.Format("Pitch: {0}, Roll: {1}, Yaw: {2}", Math.Round(face.FaceAttributes.HeadPose.Pitch, 2), Math.Round(face.FaceAttributes.HeadPose.Roll, 2), Math.Round(face.FaceAttributes.HeadPose.Yaw, 2)),
                            FacialHair = string.Format("FacialHair: {0}", face.FaceAttributes.FacialHair.Moustache + face.FaceAttributes.FacialHair.Beard + face.FaceAttributes.FacialHair.Sideburns > 0 ? "Yes" : "No"),
                            Glasses = string.Format("GlassesType: {0}", face.FaceAttributes.Glasses.ToString()),
                            Emotion = $"{GetEmotion(face.FaceAttributes.Emotion)}",
                            Hair = string.Format("Hair: {0}", GetHair(face.FaceAttributes.Hair)),
                            Makeup = string.Format("Makeup: {0}", ((face.FaceAttributes.Makeup.EyeMakeup || face.FaceAttributes.Makeup.LipMakeup) ? "Yes" : "No")),
                            EyeOcclusion = string.Format("EyeOccluded: {0}", ((face.FaceAttributes.Occlusion.EyeOccluded) ? "Yes" : "No")),
                            ForeheadOcclusion = string.Format("ForeheadOccluded: {0}", (face.FaceAttributes.Occlusion.ForeheadOccluded ? "Yes" : "No")),
                            MouthOcclusion = string.Format("MouthOccluded: {0}", (face.FaceAttributes.Occlusion.MouthOccluded ? "Yes" : "No")),
                            Accessories = $"{GetAccessories(face.FaceAttributes.Accessories)}",
                            Blur = string.Format("Blur: {0}", face.FaceAttributes.Blur.BlurLevel.ToString()),
                            Exposure = string.Format("{0}", face.FaceAttributes.Exposure.ExposureLevel.ToString()),
                            Noise = string.Format("Noise: {0}", face.FaceAttributes.Noise.NoiseLevel.ToString()),
                        });
                    }

                    // Convert detection result into UI binding object for rendering
                    foreach (var face in UIHelper.CalculateFaceRectangleForRendering(faces, MaxImageSize, imageInfo))
                    {
                        ResultCollection.Add(face);
                    }

                    // Start train large person group

                    while (true)
                    {
                        await Task.Delay(1000);

                        try // Temporary
                        {
                            MainWindow.Log("Request: Training group \"{0}\"", _scanGroup.Group.LargePersonGroupId);
                            await faceServiceClient.TrainLargePersonGroupAsync(_scanGroup.Group.LargePersonGroupId);
                            break;
                        }
                        catch (Exception exc)
                        {
                            MainWindow.Log($"Error: {exc.Message}");
                            // retry
                        }
                    }

                    // Wait until train completed
                    while (true)
                    {
                        await Task.Delay(1000);

                        try // Temporary
                        {
                            var status = await faceServiceClient.GetLargePersonGroupTrainingStatusAsync(_scanGroup.Group.LargePersonGroupId);
                            MainWindow.Log("Response: {0}. Group \"{1}\" training process is {2}", "Success", _scanGroup.Group.LargePersonGroupId, status.Status);
                            if (status.Status != Microsoft.ProjectOxford.Face.Contract.Status.Running)
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            MainWindow.Log($"Error: {ex.Message}");
                            // retry
                        }
                    }

                    await GoGetMatches(faceServiceClient);

                }
                catch (FaceAPIException ex)
                {
                    MainWindow.Log("Response: {0}. {1}", ex.ErrorCode, ex.ErrorMessage);
                    GC.Collect();
                    return;
                }
                GC.Collect();
            }

            btnNext.IsEnabled = true;
        }

        private async Task GoGetMatches(FaceServiceClient faceServiceClient)
        {
            // Identify each face
            // Call identify REST API, the result contains identified person information
            var identifyResult = await faceServiceClient.IdentifyAsync(_detectedFaces.Select(ff => new Guid(ff.FaceId)).ToArray(), largePersonGroupId: this._scanGroup.Group.LargePersonGroupId);
            for (int idx = 0; idx < _detectedFaces.Count; idx++)
            {
                // Update identification result for rendering
                var face = DetectedFaces[idx];
                var res = identifyResult[idx];
                if (res.Candidates.Length > 0 && _scanGroup.GroupPersons.Any(p => p.Person.PersonId == res.Candidates[0].PersonId))
                {
                    var pers = _scanGroup.GroupPersons.Where(p => p.Person.PersonId == res.Candidates[0].PersonId).First().Person;
                    face.PersonName = pers.Name;
                    face.PersonId = pers.PersonId;
                    face.PersonSourcePath = pers.UserData;
                }
                else
                {
                    face.PersonName = "Unknown";
                }
            }

            var outString = new StringBuilder();
            foreach (var face in DetectedFaces)
            {
                outString.AppendFormat("Face {0} is identified as {1}. ", face.FaceId, face.PersonName);
            }

            MainWindow.Log("Response: Success. {0}", outString);

        }

        private string GetHair(Hair hair)
        {
            if (hair.HairColor.Length == 0)
            {
                if (hair.Invisible)
                    return "Invisible";
                else
                    return "Bald";
            }
            else
            {
                HairColorType returnColor = HairColorType.Unknown;
                double maxConfidence = 0.0f;

                for (int i = 0; i < hair.HairColor.Length; ++i)
                {
                    if (hair.HairColor[i].Confidence > maxConfidence)
                    {
                        maxConfidence = hair.HairColor[i].Confidence;
                        returnColor = hair.HairColor[i].Color;
                    }
                }

                return returnColor.ToString();
            }
        }

        private string GetAccessories(Accessory[] accessories)
        {
            if (accessories.Length == 0)
            {
                return "NoAccessories";
            }

            string[] accessoryArray = new string[accessories.Length];

            for (int i = 0; i < accessories.Length; ++i)
            {
                accessoryArray[i] = accessories[i].Type.ToString();
            }

            return "Accessories: " + String.Join(",", accessoryArray);
        }

        private string GetEmotion(Microsoft.ProjectOxford.Common.Contract.EmotionScores emotion)
        {
            string emotionType = string.Empty;
            double emotionValue = 0.0;
            if (emotion.Anger > emotionValue)
            {
                emotionValue = emotion.Anger;
                emotionType = "Anger";
            }
            if (emotion.Contempt > emotionValue)
            {
                emotionValue = emotion.Contempt;
                emotionType = "Contempt";
            }
            if (emotion.Disgust > emotionValue)
            {
                emotionValue = emotion.Disgust;
                emotionType = "Disgust";
            }
            if (emotion.Fear > emotionValue)
            {
                emotionValue = emotion.Fear;
                emotionType = "Fear";
            }
            if (emotion.Happiness > emotionValue)
            {
                emotionValue = emotion.Happiness;
                emotionType = "Happiness";
            }
            if (emotion.Neutral > emotionValue)
            {
                emotionValue = emotion.Neutral;
                emotionType = "Neutral";
            }
            if (emotion.Sadness > emotionValue)
            {
                emotionValue = emotion.Sadness;
                emotionType = "Sadness";
            }
            if (emotion.Surprise > emotionValue)
            {
                emotionValue = emotion.Surprise;
                emotionType = "Surprise";
            }
            return $"{emotionType}";
        }

        private void imgCurrent_MouseDown(object sender, MouseButtonEventArgs e)
        {
            canvDrag.Children.Clear();
            _isDragging = true;

            _selectRectangleStartPoint = e.GetPosition((IInputElement)sender);
            SelectRectangle = new Rectangle() { IsHitTestVisible = false, Width = 1, Height = 1, Stroke = new SolidColorBrush(Colors.Black), StrokeThickness = 1 };
            SelectRectangle.SetValue(Canvas.LeftProperty, _selectRectangleStartPoint.X);
            SelectRectangle.SetValue(Canvas.TopProperty, _selectRectangleStartPoint.Y);

            canvDrag.Children.Add(SelectRectangle);
        }

        private void imgCurrent_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                var point = e.GetPosition((IInputElement)sender);
                var width = point.X - _selectRectangleStartPoint.X;
                var height = point.Y - _selectRectangleStartPoint.Y;

                if (width == 0 || height == 0)
                {
                    return;
                }
                //width = width == 0 ? 1 : width;
                //height = height == 0 ? 1 : height;

                if (width > 0)
                {
                    SelectRectangle.Width = width;
                }
                else
                {
                    _selectRectangleStartPoint.X += width;
                    SelectRectangle.SetValue(Canvas.LeftProperty, _selectRectangleStartPoint.X);
                    SelectRectangle.Width -= width;
                }

                if (height > 0)
                {
                    SelectRectangle.Height = height;
                }
                else
                {
                    _selectRectangleStartPoint.Y += height;
                    SelectRectangle.SetValue(Canvas.TopProperty, _selectRectangleStartPoint.Y);
                    SelectRectangle.Height -= height;
                }
            }
        }

        private void imgCurrent_MouseLeave(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            canvDrag.Children.Clear();
        }

        private void imgCurrent_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var scale = ((1 / imgCurrent.Source.Width) * imgCurrent.ActualWidth);
            var face = new Microsoft.ProjectOxford.Face.Controls.Face { Height = (int)(SelectRectangle.Height / scale), Width = (int)(SelectRectangle.Width / scale), Left = (int)(_selectRectangleStartPoint.X / scale), Top = (int)(_selectRectangleStartPoint.Y / scale), ImageFile = SelectedFile };
            DetectedFaces.Add(face);
            _isDragging = false;
            canvDrag.Children.Clear();
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var file = new PictureFile { DateAdded = DateTime.Now, FilePath = _selectedFilePath, IsConfirmed = true };
            db.AddFile(file, _scanGroup.Group.LargePersonGroupId);

            for (var ix = 0; ix < DetectedFaces.Count; ix++)
            {
                var face = DetectedFaces[ix];
                var person = new PicturePerson
                {
                    DateAdded = DateTime.Now,
                    PersonId = face.PersonId,
                    PictureFileId = file.Id,
                    LargePersonGroupId = _scanGroup.Group.LargePersonGroupId,
                    FaceJSON = Newtonsoft.Json.JsonConvert.SerializeObject(face),
                    IsConfirmed = true
                };
                db.AddPerson(person);

                if (face.AddToGroup)
                {
                    var contentControl = face.ContextBinder.Parent as ContentControl;
                    var parentGrid = contentControl.Parent as Grid;
                    var croppedImage = parentGrid.Children[1] as Image;

                    var filePath = face.ImageFile.ToString().Replace("file:///", "");
                    filePath = filePath.Replace('\\', '/');

                    var fileName = $"DbId-{person.Id}_" + System.IO.Path.GetFileName(filePath); // unique file name, into training folder
                    var newFilePath = System.IO.Path.Combine(face.PersonSourcePath, fileName);
                    
                    CropToSquare(filePath, newFilePath, face.Left, face.Top, face.Width, face.Height);
                    
                    await AddFaceToLargePersonGroup(_scanGroup.Group.LargePersonGroupId, newFilePath, face.PersonId);
                }
            }

            GetNextFile();
        }

        private void CropToSquare(string oldFilename, string fileName, int left, int top, int width, int height)
        {
            // Create a new image at the cropped size
            System.Drawing.Bitmap cropped = new System.Drawing.Bitmap(width, height);

            //Load image from file
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(oldFilename))
            {
                // Create a Graphics object to do the drawing, *with the new bitmap as the target*
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(cropped))
                {
                    // Draw the desired area of the original into the graphics object
                    g.DrawImage(image, new System.Drawing.Rectangle(0, 0, width, height), new System.Drawing.Rectangle(left, top, width, height), System.Drawing.GraphicsUnit.Pixel);
                    // Save the result
                    cropped.Save(fileName);
                }
                cropped.Dispose();
            }
        }

        private async Task AddFaceToLargePersonGroup(string largePersonGroupId, string imgPath, Guid PersonId)
        {
            var imageList = new ConcurrentBag<string>(new [] { imgPath });

            string img;
            while (imageList.TryTake(out img))
            {
                using (var fStream = File.OpenRead(img))
                {
                    try
                    {
                        //face.image.Save(m, image.RawFormat);
                        // Update person faces on server side
                        var persistFace = await faceServiceClient.AddPersonFaceInLargePersonGroupAsync(largePersonGroupId, PersonId, fStream, img);
                        return;
                    }
                    catch (FaceAPIException ex)
                    {
                        // if operation conflict, retry.
                        if (ex.ErrorCode.Equals("ConcurrentOperationConflict"))
                        {
                            MainWindow.Log("Concurrent Operation Conflict. Re-queuing");
                            imageList.Add(img);
                            continue;
                        }
                        // if operation cause rate limit exceed, retry.
                        else if (ex.ErrorCode.Equals("RateLimitExceeded"))
                        {
                            imageList.Add(img);
                            MainWindow.Log("Rate Limit Exceeded. Re-queuing in 1 second");
                            await Task.Delay(1000);
                            continue;
                        }

                        MainWindow.Log($"Error: {ex.Message}");

                        // Here we simply ignore all detection failure in this sample
                        // You may handle these exceptions by check the Error.Error.Code and Error.Message property for ClientException object
                        return;
                    }
                }
            }
        }

        private void btnRemove_Click_1(object sender, RoutedEventArgs e)
        {
            var ctrl = sender as Button;

            var f = ctrl.DataContext as Microsoft.ProjectOxford.Face.Controls.Face;
            DetectedFaces.Remove(f);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            GetNextFile();
        }
    }
}
