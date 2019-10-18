using Permadelete.Controls;
using Permadelete.ViewModels;

namespace Permadelete.Views
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class SettingsDialog : FlatWindow
    {
        private readonly SettingsVM _viewModel;
        public SettingsDialog()
        {
            InitializeComponent();

            DataContext = _viewModel = new SettingsVM();
            Loaded += SettingsDialog_Loaded;
        }

        private void SettingsDialog_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.Load();
        }

    }
}
