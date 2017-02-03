using Permadelete.Helpers;
using Permadelete.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiskDetector;
namespace Permadelete.Services
{
    public sealed class ShredderService
    {
        #region Constructor
        private ShredderService()
        {

        }
        #endregion

        #region Fields
        private const int MAX_BUFFER_SIZE = Constants.MEGABYTE;
        private const int OBFUSCATE_ROUNDS = 5;

        Random _random = new Random();
        #endregion

        #region Properties
        // using Lazy<T> here makes the the field both lazily loaded and thread safe
        // more info: http://csharpindepth.com/Articles/General/Singleton.aspx
        private static Lazy<ShredderService> _instance = new Lazy<ShredderService>(() => new ShredderService());
        public static ShredderService Instance
        {
            get { return _instance.Value; }
        }
        #endregion

        #region Methods
        public async Task<bool> ShredItemAsync(string path, CancellationToken cancellationToken, IProgress<long> progress)
        {
            FileInfo file = null;
            DirectoryInfo folder = null;

            if (File.Exists(path))
                file = new FileInfo(path);
            else if (Directory.Exists(path))
                folder = new DirectoryInfo(path);
            else
                throw new ArgumentException($"This path does not exist: {path}");

            if (file != null)
                return await ShredFileAsync(file, cancellationToken, progress).ConfigureAwait(false);

            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (folder != null)
                return await ShredFolderAsync(folder, cancellationToken, progress).ConfigureAwait(false);

            return false;
        }

        public async Task<long> GetFolderSize(DirectoryInfo folder)
        {
            long length = 0;
            await Task.Run(async () =>
            {
                foreach (var item in folder.EnumerateFiles())
                {
                    length += item.Length;
                }

                foreach (var item in folder.EnumerateDirectories())
                {
                    length += await GetFolderSize(item);
                }
            });

            return length;
        }

        public async Task<bool> ShredFileAsync(FileInfo file, CancellationToken cancellationToken, IProgress<long> progress)
        {
            var totalBytes = file.Length;

            var writeProgress = new Progress<long>();
            writeProgress.ProgressChanged += (sender, newBytes) =>
            {
                if (progress == null) return;
                progress.Report(newBytes);
            };

            // if the file was a syslink, then only remove the link not the contents.
            // and if the drive was an ssd, then there is no point in overwriting content
            // more info: https://samsclass.info/121/proj/ssd-evaporation.htm
            var driveName = Path.GetPathRoot(file.FullName);
            if ((file.Attributes & FileAttributes.ReparsePoint) == 0 && !DiskIsSSD(driveName))
            {
                file.Attributes = FileAttributes.Normal;
                file.Attributes = FileAttributes.NotContentIndexed;

                var result = await OverWriteFileAsync(file, cancellationToken, writeProgress).ConfigureAwait(false);
                if (!result) return result;

                if (cancellationToken != null) cancellationToken.ThrowIfCancellationRequested();
                DestroyFileMetadata(file);
            }
            else
            {
                progress.Report(file.Length);
            }

            file.Delete();

            return true;
        }

        public async Task<bool> ShredFolderAsync(DirectoryInfo folder, CancellationToken cancellationToken, IProgress<long> progress)
        {
            var totalLength = await GetFolderSize(folder);
            var everythingWasShredded = true;

            // if the folder was a syslink then only remove the link not the contents.
            if ((folder.Attributes & FileAttributes.ReparsePoint) == 0)
            {
                Progress<long> itemProgress;
                foreach (var info in folder.EnumerateFileSystemInfos())
                {
                    itemProgress = new Progress<long>();

                    if (cancellationToken != null)
                        cancellationToken.ThrowIfCancellationRequested();

                    var file = info as FileInfo;
                    var dir = info as DirectoryInfo;
                    long length = 0;

                    try
                    {
                        if (file != null)
                        {
                            length = file.Length;
                            itemProgress.ProgressChanged += (sender, newBytes) => progress.Report(newBytes);
                            var result = await ShredFileAsync(file, cancellationToken, itemProgress).ConfigureAwait(false);

                            if (!result) return result;
                        }

                        if (dir != null)
                        {
                            length = await GetFolderSize(dir);
                            itemProgress.ProgressChanged += (sender, newBytes) => progress.Report(newBytes);
                            var result = await ShredFolderAsync(dir, cancellationToken, itemProgress).ConfigureAwait(false);

                            if (!result) return result;
                        }
                    }
                    catch (Exception)
                    {
                        // if an exception was thrown, just skip this item
                        everythingWasShredded = false;
                        progress.Report((length));
                        continue;
                    }
                }
            }

            if (everythingWasShredded)
                folder.Delete();

            return everythingWasShredded;
        }

        public bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null) stream.Close();
            }

            return false;
        }
        #endregion

        #region Private Methods
        private async Task<bool> OverWriteFileAsync(FileInfo file, CancellationToken cancellationToken, IProgress<long> progress)
        {
            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[MAX_BUFFER_SIZE];
                for (int i = 0; i < MAX_BUFFER_SIZE; i++)
                {
                    buffer[i] = (byte)_random.Next(0, 255);
                }

                for (var length = file.Length; length > 0; length -= MAX_BUFFER_SIZE)
                {
                    int bytesToWrite = (length > MAX_BUFFER_SIZE) ? MAX_BUFFER_SIZE : (int)length;

                    await stream.WriteAsync(buffer, 0, bytesToWrite).ConfigureAwait(false);
                    await stream.FlushAsync().ConfigureAwait(false);

                    if (progress != null)
                        progress.Report(bytesToWrite);

                    if (cancellationToken != null)
                        cancellationToken.ThrowIfCancellationRequested();
                }

                await Task.Run(() =>
                {
                    stream.SetLength(0);
                }).ConfigureAwait(false);
            }

            return true;
        }

        private bool DestroyFileMetadata(FileInfo file, FileSystemType fileSystemType = FileSystemType.Unknown)
        {
            if (!file.Exists)
                return false;

            var directoryName = Path.GetDirectoryName(file.FullName);

            // rename the file a few times to remove it from the file system table.
            for (var round = 0; round < OBFUSCATE_ROUNDS; round++)
            {
                var newPath = Path.Combine(directoryName, Path.GetRandomFileName());
                file.MoveTo(newPath);
            }

            var newTime = new DateTime(2000, 1, 1, 0, 0, 1);
            file.LastWriteTime = newTime;
            file.CreationTime = newTime;
            file.LastAccessTime = newTime;

            return true;
        }

        private bool DiskIsSSD(string driveName)
        {
            try
            {
                var result = Detector.DetectDrive(driveName);
                return result.HardwareType == DiskDetector.Models.HardwareType.Ssd;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
