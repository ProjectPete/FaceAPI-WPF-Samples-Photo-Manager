using ClientLibrary.Controls;
using System;
using System.Collections.Generic;
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

namespace Microsoft.ProjectOxford.Face.Controls
{
    /// <summary>
    /// Interaction logic for SortMyPhotosPage.xaml
    /// </summary>
    public partial class SortMyPhotosPage : Page
    {
        public SortMyPhotosPage()
        {
            InitializeComponent();
        }

        private void Groups_Click(object sender, RoutedEventArgs e)
        {
            TheContent.Content = new ManageGroupsControl();
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
