using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Newtonsoft.Json;
using Dropbox.Api.Stone;
using Dropbox.Api.Files;
using System.IO;
using System.Diagnostics;

namespace RudeFox.Updater
{
    public class UpdateManager
    {
        #region Constructor
        private const string DROPBOX_API_KEY = "";
        static UpdateManager()
        {
            _client = new DropboxClient(DROPBOX_API_KEY);
        }
        #endregion

        #region Fields
        private static DropboxClient _client;
        private static Random _random = new Random();
        private static string _updateFolder = "/update/v0.1";
        private static System.Text.RegularExpressions.Regex _pattern = new System.Text.RegularExpressions.Regex(@"[\\/]{2,}|[\\]");
        #endregion

        #region Methods
        /// <summary>
        /// Tries to update the app and returns the update information if succeeded, otherwise returns null.
        /// </summary>
        /// <param name="currentVersion">The current version of the app</param>
        /// <returns></returns>
        public static async Task<UpdateInfo> DownloadLatestUpdate(Version currentVersion)
        {
            var appFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            DeleteOldFiles(appFolder);

            var updateInfo = await CheckForUpdates().ConfigureAwait(false);
            if (updateInfo.Version <= currentVersion || updateInfo?.Path == null)
                return null;

            updateInfo.TempFolderName = Path.GetTempPath() + "RudeFox " + _random.Next();
            Directory.CreateDirectory(updateInfo.TempFolderName);

            foreach (var remote in updateInfo.Files)
            {
                var localPath = Path.Combine(appFolder, remote.Name);
                var localFileInfo = new FileInfo(localPath);

                Version localVersion;
                if (System.IO.File.Exists(localPath) && (remote.Extention == "exe" || remote.Extention == "dll"))
                    localVersion = new Version(FileVersionInfo.GetVersionInfo(localPath).ProductVersion);
                else
                    localVersion = new Version(0, 0, 0, 0);

                var remoteIsNewer = remote.Version > localVersion;
                var shouldReplace = remote.Overwrite || remoteIsNewer;
                remote.IsDownloaded = !localFileInfo.Exists || shouldReplace;

                if (!remote.IsDownloaded) continue;

                var downloadPath = CombinePathForInternet(_updateFolder, updateInfo.Path, remote.Folder, remote.Name);
                var data = await _client.Files.DownloadAsync(downloadPath).ConfigureAwait(false);

                var tempPath = Path.Combine(updateInfo.TempFolderName, remote.Folder, remote.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(tempPath));

                using (var stream = await data.GetContentAsStreamAsync().ConfigureAwait(false))
                using (var fileStream = System.IO.File.Create(tempPath))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }
            return updateInfo;
        }

        /// <summary>
        /// Applies the update after it has been downloaded. Must be called after calling DownloadLatestUpdate.
        /// </summary>
        /// <param name="tempFolderPath">The path of the folder in which the update files are located.</param>
        public static void ApplyUpdate(string tempFolderPath)
        {
            var appFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            var downloadedFiles = new DirectoryInfo(tempFolderPath).EnumerateFiles("*.*", SearchOption.AllDirectories).Select(info => info.Name);

            var oldFiles = new DirectoryInfo(appFolder).EnumerateFiles("*.*", SearchOption.AllDirectories)
                       .Where(local => (local.FullName.EndsWith(".exe") || local.FullName.EndsWith(".dll") && downloadedFiles.Contains(local.Name)));

            foreach (var item in oldFiles)
            {
                var newFileName = item.FullName + ".bak";
                if (System.IO.File.Exists(newFileName))
                    System.IO.File.Delete(newFileName);
                item.MoveTo(newFileName);
            }

            var entries = Directory.EnumerateFileSystemEntries(tempFolderPath);
            foreach (var entry in entries)
            {
                var newFileName = Path.Combine(appFolder, entry.Replace(tempFolderPath, ""));
                if (System.IO.File.Exists(entry))
                    System.IO.File.Copy(entry, newFileName, true);
                else
                    Directory.Move(entry, appFolder);
            }

            Directory.Delete(tempFolderPath, true);
        }

        /// <summary>
        /// Get the latest version of the app available.
        /// </summary>
        /// <returns></returns>
        public static async Task<Version> GetLatestVersion()
        {
            var info = await CheckForUpdates().ConfigureAwait(false);
            return info.Version;
        }

        private static void DeleteOldFiles(string directory)
        {
            var files = new DirectoryInfo(directory).EnumerateFiles("*.bak", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                file.Delete();
                if (!file.Directory.EnumerateFileSystemInfos().Any())
                    file.Directory.Delete();
            }
        }

        private static async Task<UpdateInfo> CheckForUpdates()
        {
            using (var response = await _client.Files.DownloadAsync(CombinePathForInternet(_updateFolder, "info.json")).ConfigureAwait(false))
            {
                var latestInfo = await response.GetContentAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<UpdateInfo>(latestInfo);
            }
        }

        private static string CombinePathForInternet(params string[] segments)
        {
            string fullPath = string.Join("/", segments);
            return _pattern.Replace(fullPath, "/");
        }
        #endregion
    }
}
