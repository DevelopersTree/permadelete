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
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Permadelete.Updater;
using Newtonsoft.Json;

namespace Permadelete.UpdateFactory
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _appFolderPath = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Choose the folder that contains the update files";
            dialog.UseDescriptionForTitle = true;

            var result = dialog.ShowDialog();
            if (result == null | !(bool)result) return;

            _appFolderPath = dialog.SelectedPath;

            DataContext = UpdateConfigManger.Load(_appFolderPath);
        }

        private void publishButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext == null) return;

            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Where to save the files";
            dialog.UseDescriptionForTitle = true;

            var result = dialog.ShowDialog();
            if (result == null | !(bool)result) return;

            var path = dialog.SelectedPath;

            var viewModel = (MainWindowVM)DataContext;
            var updateInfo = UpdateConfigManger.GetUpdateInfo(viewModel.Path, viewModel.Type, viewModel.Link, viewModel.Version);
            UpdateConfigManger.Write(path, viewModel.Files, viewModel.Indented ? Formatting.Indented : Formatting.None, updateInfo);

            Process.Start(path);
        }
    }
}
