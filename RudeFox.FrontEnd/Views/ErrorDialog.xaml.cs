using RudeFox.Controls;
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
    /// Interaction logic for ErrorDialog.xaml
    /// </summary>
    public partial class ErrorDialog : FlatWindow
    {
        public ErrorDialog()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            okButton.Focus();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

    }
}
