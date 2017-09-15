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
        private static System.Text.RegularExpressions.Regex _pattern = new System.Text.RegularExpressions.Regex(@"[\\/]{2,}|[\\]");
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
            var viewModel = new MainWindowVM();
            var entries = System.IO.Directory.EnumerateFiles(_appFolderPath, "*.*", System.IO.SearchOption.AllDirectories);
            var files = new ObservableCollection<FileVM>();
            viewModel.Files = files;

            viewModel.Version = "1.0.0.0";
            viewModel.Link = "http://developerstree.com/";
            viewModel.Path = "data";
            viewModel.Type = UpdateType.Normal;

            foreach (var item in entries)
            {
                if (item.EndsWith(".pdb") || item.Contains("vshost"))
                    continue;
                var file = new FileVM();
                file.FileInfo = new System.IO.FileInfo(item);
                viewModel.Length += file.FileInfo.Length;
                file.Name = file.FileInfo.Name;
                var dirName = System.IO.Path.GetDirectoryName(item);
                file.Folder = dirName.Replace(_appFolderPath, string.Empty).Replace(@"\", @"/");
                var version = FileVersionInfo.GetVersionInfo(item).FileVersion;
                if (version != null)
                {
                    var count = version.Split('.').Count();
                    if (count != 4)
                    {
                        for (; count < 4; count++)
                        {
                            version += ".0";
                        }
                    }

                    file.Version = version;
                }
                else
                    file.Version = "1.0.0.0";
                file.Length = file.FileInfo.Length;
                file.IsIncluded = !file.Name.EndsWith(".xml");
                file.Overwrite = item.EndsWith(".exe") || item.EndsWith(".dll") ? false : true;
                files.Add(file);
            }

            DataContext = viewModel;
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
            var updateInfo = new UpdateInfo();
            var includedFiles = viewModel.Files.Where(f => f.IsIncluded);

            updateInfo.Version = new Version(viewModel.Version);
            updateInfo.Path = viewModel.Path;
            updateInfo.Type = viewModel.Type;
            updateInfo.Length = includedFiles.Sum(f => f.Delete ? 0 : f.FileInfo.Length);
            updateInfo.ChangeListLink = viewModel.Link;
            updateInfo.NewFiles = includedFiles.Where(f => f.Delete == false).Select(f => f.File).ToList();
            updateInfo.ObsoleteFiles = includedFiles.Where(f => f.Delete).Select(f => GetUniformPath(f.Folder, f.Name)).ToList();
            var format = viewModel.Indented ? Formatting.Indented : Formatting.None;
            var json = JsonConvert.SerializeObject(updateInfo, format);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);
            System.IO.File.WriteAllText(GetUniformPath(path, "info.json"), json);
            foreach (var file in includedFiles)
            {
                if (file.Delete)
                    continue;
                var folderName = GetUniformPath(path, updateInfo.Path, file.Folder);
                if (!System.IO.Directory.Exists(folderName))
                    System.IO.Directory.CreateDirectory(folderName);
                file.FileInfo.CopyTo(GetUniformPath(folderName, file.Name), true);
            }

            Process.Start(path);
        }

        private static string GetUniformPath(params string[] segments)
        {
            string fullPath = string.Join("/", segments);
            return _pattern.Replace(fullPath, "/");
        }

        private static string GetRelativePath(string parentFolder, string fullPath)
        {
            var result = fullPath.Replace(parentFolder, string.Empty);
            if (result.Any() && result.First() == '\\' || result.First() == '/')
                result = result.Substring(1);
            return GetUniformPath(result);
        }
    }
}
