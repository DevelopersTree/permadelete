using Permadelete.Controls;
using Permadelete.Helpers;
using System.Windows;

namespace Permadelete.Views
{
    /// <summary>
    /// Interaction logic for ConfirmDialog.xaml
    /// </summary>
    public partial class ConfirmDialog : FlatWindow
    {
        public ConfirmDialog(string message)
        {
            InitializeComponent();
            txtMessage.Text = message;
            Loaded += ConfirmDialog_Loaded;
        }

        private void ConfirmDialog_Loaded(object sender, RoutedEventArgs e)
        {
            shredButton.Focus();
            var settings = SettingsHelper.GetSettings();
            passesCombobox.SelectedItem = Passes = settings.DefaultOverwritePasses;
        }

        private void shredButton_Click(object sender, RoutedEventArgs e)
        {
            Passes = (int)passesCombobox.SelectedItem;
            DialogResult = true;
            Close();
        }

        public int Passes { get; set; }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
