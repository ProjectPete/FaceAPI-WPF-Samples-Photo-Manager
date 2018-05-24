

namespace Photo_Detect_Catalogue_Search_WPF_App.Models
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public class PersonExtended : INotifyPropertyChanged
    {
        private ObservableCollection<Face> _faces 
            = new ObservableCollection<Face>();

        private bool _isSelected;

        public ObservableCollection<Face> Faces
        {
            get
            {
                return _faces;
            }
            set
            {
                _faces = value;
            }
        }

        public Microsoft.ProjectOxford.Face.Contract.Person Person { get; set; }

        public int PersonFilesDbCount { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }

            set
            {
                _isSelected = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
                }
            }
        }
    }
}
