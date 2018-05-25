using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Controls
{
    public class PersonExtended : INotifyPropertyChanged
    {
        private ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face> _faces 
            = new ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face>();

        private bool _isSelected;

        public ObservableCollection<Microsoft.ProjectOxford.Face.Controls.Face> Faces
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
