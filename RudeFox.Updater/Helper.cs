using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace RudeFox.Updater
{
    internal static class Helper
    {
        #region P/Invoke Signatures
        [DllImport("wininet.dll", SetLastError = true)]
        extern static bool InternetGetConnectedState(out int lpdwFlags, int dwReserved);
        #endregion

        #region Fields
        static System.Text.RegularExpressions.Regex _pattern = new System.Text.RegularExpressions.Regex(@"[\\/]{2,}|[\\]");
        internal const string _attemptFileName = "attempt.bak";
        internal const string _updateFolder = "/update/v0.2";
        #endregion

        #region Methods
        internal static string BackUpAppFiles(string appFolder)
        {
            var tempPath = GetUniformPath(Path.GetTempPath(), "Rude Fox Backup");
            DirectoryCopy(appFolder, tempPath);

            return tempPath;
        }

        internal static string GetRelativePath(string parentFolder, string fullPath)
        {
            var result = fullPath.Replace(parentFolder, string.Empty);
            if (result.Any() && result.First() == '\\' || result.First() == '/')
                result = result.Substring(1);
            return GetUniformPath(result);
        }

        internal static bool ShouldDownloadFile(File file, string appFolderPath, string tempFolderPath, bool lastAttemptWasNew)
        {
            var localPath = GetUniformPath(appFolderPath, file.Folder, file.Name);
            var tempPath = GetUniformPath(tempFolderPath, file.Folder, file.Name);
            var localVersion = GetFileVersion(localPath, file.Extention);
            var tempVersion = GetFileVersion(tempPath, tempPath.Split('.').LastOrDefault());

            var remoteIsNewer = file.Version > localVersion;
            var shouldReplace = file.Overwrite || remoteIsNewer;

            var tempFileIsComplete = System.IO.File.Exists(tempPath) && (new FileInfo(tempPath).Length == file.Length);
            var isAlreadyDownloaded = lastAttemptWasNew && tempFileIsComplete && (tempVersion == file.Version);

            return !isAlreadyDownloaded && (!System.IO.File.Exists(localPath) || shouldReplace);
        }

        internal static Version GetFileVersion(string path, string extention)
        {
            try
            {
                Version localVersion;
                if (System.IO.File.Exists(path) && System.IO.File.Exists(path) && (extention == "exe" || extention == "dll"))
                    localVersion = ParseVersion(FileVersionInfo.GetVersionInfo(path).FileVersion);
                else
                    localVersion = new Version(0, 0, 0, 0);

                return localVersion;
            }
            catch (Exception)
            {
                return new Version(0, 0, 0, 0);
            }
        }

        internal static void CleanUpFolder(string directory, string searchPattern = "*.bak", SearchOption options = SearchOption.AllDirectories)
        {
            var files = new DirectoryInfo(directory).EnumerateFiles(searchPattern, options);
            foreach (var file in files)
            {
                file.Delete();
                if (!file.Directory.EnumerateFileSystemInfos().Any())
                    file.Directory.Delete();
            }
        }

        internal static void RollBackChanges(string backupPath, string appFolder)
        {
            if (!Directory.Exists(backupPath) || !Directory.Exists(appFolder)) return;

            var files = Directory.EnumerateFiles(backupPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var newPath = GetUniformPath(appFolder, GetRelativePath(backupPath, file));
                try
                {
                    FileCopy(file, newPath);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        internal static void FileCopy(string source, string destination)
        {
            if (!Directory.Exists(Path.GetDirectoryName(destination)))
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
            System.IO.File.Copy(source, destination, true);
        }

        internal static UpdateAttempt CheckForPreviousAttempts(string tempFolderName, Version currentVersion, Version newVersion)
        {
            var attemptPath = GetUniformPath(tempFolderName, _attemptFileName);
            UpdateAttempt attempt;
            if (System.IO.File.Exists(attemptPath))
            {
                attempt = JsonConvert.DeserializeObject<UpdateAttempt>(System.IO.File.ReadAllText(attemptPath));
            }
            else
            {
                attempt = new UpdateAttempt { Date = DateTime.Now, Version = newVersion };
                var attemptJson = JsonConvert.SerializeObject(attempt);
                System.IO.File.WriteAllText(attemptPath, attemptJson);
            }

            return attempt;
        }

        internal static string GetUniformPath(params string[] segments)
        {
            string fullPath = string.Join("/", segments);
            return _pattern.Replace(fullPath, "/");
        }

        // from: https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        internal static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = GetUniformPath(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = GetUniformPath(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        internal static Version ParseVersion(string version)
        {
            var count = version.Split('.').Count();

            if (count == 0) return new Version(0, 0, 0, 0);

            for (; count < 4; count++)
                version += ".0";

            return new Version(version);
        }

        // more info at: https://msdn.microsoft.com/en-us/library/windows/desktop/aa384702%28v=vs.85%29.aspx
        internal static bool InternetConnectionAvailable()
        {
            int description;
            return InternetGetConnectedState(out description, 0);
        }
        #endregion
    }
}
