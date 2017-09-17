using Newtonsoft.Json;
using Permadelete.Updater;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete.UpdateFactory
{
    public class UpdateConfigManger
    {
        private static System.Text.RegularExpressions.Regex _pattern = new System.Text.RegularExpressions.Regex(@"[\\/]{2,}|[\\]");

        public static void Write(string outputPath, IEnumerable<FileVM> files, Formatting format, UpdateInfo updateInfo)
        {
            outputPath = GetUniformPath(outputPath);

            var includedFiles = files.Where(f => f.IsIncluded).ToList();

            updateInfo.Length = includedFiles.Sum(f => f.Delete ? 0 : f.FileInfo.Length);
            updateInfo.NewFiles = includedFiles.Where(f => f.Delete == false).Select(f => f.File).ToList();
            updateInfo.ObsoleteFiles = includedFiles.Where(f => f.Delete).Select(f => GetUniformPath(f.Folder, f.Name)).ToList();

            var json = JsonConvert.SerializeObject(updateInfo, format);

            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);
            System.IO.File.WriteAllText(GetUniformPath(outputPath, "info.json"), json);
            foreach (var file in includedFiles)
            {
                if (file.Delete)
                    continue;
                var folderName = GetUniformPath(outputPath, updateInfo.Path, file.Folder);
                if (!System.IO.Directory.Exists(folderName))
                    System.IO.Directory.CreateDirectory(folderName);
                file.FileInfo.CopyTo(GetUniformPath(folderName, file.Name), true);
            }
        }

        public static UpdateInfo GetUpdateInfo(string path, UpdateType type, string changeListLink, string version)
        {
            var updateInfo = new UpdateInfo();
            updateInfo.Version = ParseVersion(version);
            updateInfo.Path = path;
            updateInfo.Type = type;
            updateInfo.ChangeListLink = changeListLink;

            return updateInfo;
        }

        public static MainWindowVM Load(string path)
        {
            path = GetUniformPath(path);

            Console.WriteLine(path);

            var viewModel = new MainWindowVM();
            var entries = System.IO.Directory.EnumerateFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
            var files = new ObservableCollection<FileVM>();
            viewModel.Files = files;

            viewModel.Version = "1.0.0.0";
            viewModel.Link = "http://developerstree.com/";
            viewModel.Path = "data";
            viewModel.Type = UpdateType.Normal;

            foreach (var item in entries)
            {
                var file = new FileVM();
                file.FileInfo = new System.IO.FileInfo(item);
                viewModel.Length += file.FileInfo.Length;
                file.Name = file.FileInfo.Name;
                var dirName = GetUniformPath(System.IO.Path.GetDirectoryName(item));
                file.Folder = dirName.Replace(path, string.Empty).Replace(@"\", @"/");
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
                file.IsIncluded = true;
                file.Overwrite = item.EndsWith(".exe") || item.EndsWith(".dll") ? false : true;
                files.Add(file);
            }

            return viewModel;
        }

        public static string GetUniformPath(params string[] segments)
        {
            string fullPath = string.Join("/", segments);
            return _pattern.Replace(fullPath, "/").Trim('/');
        }

        private static Version ParseVersion(string version)
        {
            var parts = version.Split('.');
            if (parts.Length > 4)
                throw new ArgumentException("Invalid format: " + version);

            var components = new int[4];
            for (int i = 0; i < 4; i++)
            {
                if (parts.Length >= i + 1)
                    components[i] = int.Parse(parts[i]);
            }

            return new Version(components[0], components[1], components[2], components[3]);
        }
    }
}
