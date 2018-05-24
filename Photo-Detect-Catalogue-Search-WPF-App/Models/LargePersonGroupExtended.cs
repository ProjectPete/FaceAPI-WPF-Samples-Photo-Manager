using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Photo_Detect_Catalogue_Search_WPF_App.Models
{
    public class LargePersonGroupExtended
    {
        private ObservableCollection<PersonExtended> _groupPersons = new ObservableCollection<PersonExtended>();

        public LargePersonGroup Group { get; set; }

        public ObservableCollection<PersonExtended>GroupPersons
        {
            get
            {
                return _groupPersons;
            }
            set
            {
                _groupPersons = value;
            }
        }

        public PersonExtended SelectedPerson { get; set; }
    }
}
