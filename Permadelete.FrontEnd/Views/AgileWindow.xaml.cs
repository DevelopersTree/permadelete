using Permadelete.ApplicationManagement;
using Permadelete.Controls;
using Permadelete.ViewModels;
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
using System.Windows.Shapes;

namespace Permadelete.Views
{
    /// <summary>
    /// Interaction logic for AgileWindow.xaml
    /// </summary>
    public partial class AgileWindow : FlatWindow
    {
        public AgileWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            (DataContext as AgileWindowVM).CloseCommand.Execute(null);
        }
    }
}
