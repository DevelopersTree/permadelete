using RudeFox.ViewModels;
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

namespace RudeFox.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstOperations.SelectedItem == null)
                (DataContext as MainWindowVM).DeleteFilesCommand.Execute(null);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Width < 650 || Height < 300)
                sidebarGrid.Visibility = Visibility.Collapsed;
            else
                sidebarGrid.Visibility = Visibility.Visible;
        }
    }
}
