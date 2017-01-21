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

namespace Permadelete.Updater
{
    public class UpdateManager
    {
        #region Fields
        private static DropboxClient _client;
        #endregion

        #region Methods
        /// <summary>
        /// Initializes UpdateManager, should be called before calling the other methods.
        /// </summary>
        /// <param name="dropboxApiKey"></param>
        public static void Initialize(string dropboxApiKey)
        {
            if (_client == null)
                _client = new DropboxClient(dropboxApiKey);
        }

        /// <summary>
        /// Tries to update the app and returns the update information if succeeded, otherwise returns null.
        /// </summary>
        /// <param name="currentVersion">The current version of the app</param>
        /// <returns></returns>
        public static async Task<string> DownloadLatestUpdate(Version currentVersion)
        {
            var updateInfo = await CheckForUpdates().ConfigureAwait(false);
            return await DownloadLatestUpdate(currentVersion, updateInfo).ConfigureAwait(false);
        }

        public static async Task<string> DownloadLatestUpdate(Version currentVersion, UpdateInfo updateInfo)
        {
            var appFolder = Helper.GetUniformPath(Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]));
            Helper.CleanUpFolder(appFolder);

            if (updateInfo.Version <= currentVersion || updateInfo?.Path == null)
                return null;

            var tempFolderName = Helper.GetUniformPath(Path.GetTempPath() + "Permadelete " + currentVersion);
            Directory.CreateDirectory(tempFolderName);

            var updateAttempt = Helper.CheckForPreviousAttempts(tempFolderName, currentVersion, updateInfo.Version);
            var lastAttemptWasNew = updateAttempt.Version == updateInfo.Version;
            if (!lastAttemptWasNew)
                Helper.CleanUpFolder(tempFolderName, "*.*");
            else if (lastAttemptWasNew && updateAttempt.DownloadCompleted)
                return tempFolderName;

            updateAttempt.FilesToDelete.AddRange(updateInfo.ObsoleteFiles);

            foreach (var remote in updateInfo.NewFiles)
            {
                if (!Helper.ShouldDownloadFile(remote, appFolder, tempFolderName, lastAttemptWasNew))
                    continue;

                var downloadPath = Helper.GetUniformPath(Helper._updateFolder, updateInfo.Path, remote.Folder, remote.Name);
                var data = await _client.Files.DownloadAsync(downloadPath).ConfigureAwait(false);

                var tempPath = Helper.GetUniformPath(tempFolderName, remote.Folder, remote.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(tempPath));

                using (var stream = await data.GetContentAsStreamAsync().ConfigureAwait(false))
                using (var fileStream = System.IO.File.Create(tempPath))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }

            updateAttempt.DownloadCompleted = true;
            var attemptJson = JsonConvert.SerializeObject(updateAttempt);
            System.IO.File.WriteAllText(Helper.GetUniformPath(tempFolderName, Helper._attemptFileName), attemptJson);

            return tempFolderName;
        }

        /// <summary>
        /// Applies the update after it has been downloaded. Must be called after calling DownloadLatestUpdate.
        /// </summary>
        /// <param name="tempFolderPath">The path of the folder in which the update files are located.</param>
        public static void ApplyUpdate(string tempFolderPath)
        {
            tempFolderPath = Helper.GetUniformPath(tempFolderPath);

            UpdateAttempt attempt = null;
            var attemptPath = Helper.GetUniformPath(tempFolderPath, Helper._attemptFileName);
            if (System.IO.File.Exists(attemptPath))
            {
                var attemptJson = System.IO.File.ReadAllText(attemptPath);
                attempt = JsonConvert.DeserializeObject<UpdateAttempt>(attemptJson);
            }

            var appFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            string backupPath = Helper.BackUpAppFiles(appFolder);

            try
            {
                var downloadedFiles = new DirectoryInfo(tempFolderPath).EnumerateFiles("*.*", SearchOption.AllDirectories)
                                         .Select(info => Helper.GetRelativePath(tempFolderPath, Helper.GetUniformPath(info.FullName)));

                var blackList = downloadedFiles.Where(p => System.IO.File.Exists(Helper.GetUniformPath(appFolder, p)))
                                .Union(attempt?.FilesToDelete);

                foreach (var item in blackList)
                {
                    if (!System.IO.File.Exists(item)) continue;

                    var newFileName = item + ".bak";
                    if (System.IO.File.Exists(newFileName))
                        System.IO.File.Delete(newFileName);
                    System.IO.File.Move(item, newFileName);
                }

                foreach (var file in downloadedFiles)
                {
                    var tempPath = Helper.GetUniformPath(tempFolderPath, file);
                    if (System.IO.File.Exists(tempPath))
                    {
                        var newFileName = Helper.GetUniformPath(appFolder, file);
                        Helper.FileCopy(tempPath, newFileName);
                    }
                    else if (Directory.Exists(tempPath))
                    {
                        var newDirName = Helper.GetUniformPath(appFolder, file);
                        Helper.DirectoryCopy(tempPath, newDirName);
                    }
                }
            }
            catch (Exception)
            {
                Helper.RollBackChanges(backupPath, appFolder);
                throw;
            }
            finally
            {
                Directory.Delete(backupPath, true);
                Directory.Delete(tempFolderPath, true);
            }
        }

        /// <summary>
        /// Returns information about the latest update if there was internet connection, otherwise returns null.
        /// </summary>
        /// <returns></returns>
        public static async Task<UpdateInfo> CheckForUpdates()
        {
            if (!Helper.InternetConnectionAvailable()) return null;

            using (var response = await _client.Files.DownloadAsync(Helper.GetUniformPath(Helper._updateFolder, "info.json")).ConfigureAwait(false))
            {
                var latestInfo = await response.GetContentAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<UpdateInfo>(latestInfo);
            }
        }
        #endregion
    }
}
