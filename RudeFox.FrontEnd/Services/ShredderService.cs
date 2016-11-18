using RudeFox.Helpers;
using RudeFox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RudeFox.Services
{
    public class ShredderService
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
        private static ShredderService _instance;
        public static ShredderService Instance
        {
            get { return _instance ?? (_instance = new ShredderService()); }
        }
        #endregion

        #region Methods
        public async Task<bool> ShredItemAsync(string path, CancellationToken cancellationToken, IProgress<int> progress)
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

        public async Task<bool> ShredFileAsync(FileInfo file, CancellationToken cancellationToken, IProgress<int> progress)
        {
            var totalBytes = file.Length;

            var writeProgress = new Progress<int>();
            writeProgress.ProgressChanged += (sender, newBytes) =>
            {
                if (progress == null) return;
                progress.Report(newBytes);
            };

            file.Attributes = FileAttributes.Normal;
            file.Attributes = FileAttributes.NotContentIndexed;

            var result = await OverWriteFileAsync(file, cancellationToken, writeProgress).ConfigureAwait(false);
            if (!result) return result;

            if (cancellationToken != null) cancellationToken.ThrowIfCancellationRequested();
            await DestroyEntityMetaData(file);
            file.Delete();

            return true;
        }

        public async Task<bool> ShredFolderAsync(DirectoryInfo folder, CancellationToken cancellationToken, IProgress<int> progress)
        {
            var totalLength = await GetFolderSize(folder);

            folder.Attributes = FileAttributes.Normal;
            folder.Attributes = FileAttributes.NotContentIndexed;

            Progress<int> itemProgress;
            foreach (var info in folder.EnumerateFileSystemInfos())
            {
                itemProgress = new Progress<int>();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var file = info as FileInfo;
                var dir = info as DirectoryInfo;

                if (file != null)
                {
                    var length = file.Length;

                    itemProgress.ProgressChanged += (sender, newBytes) => progress.Report(newBytes);
                    var result = await ShredFileAsync(file, cancellationToken, itemProgress).ConfigureAwait(false);
                    if (!result) return result;
                }

                if (dir != null)
                {
                    var length = await GetFolderSize(dir);
                    itemProgress.ProgressChanged += (sender, newBytes) => progress.Report(newBytes);
 
                    var result = await ShredFolderAsync(dir, cancellationToken, itemProgress).ConfigureAwait(false);
                    if (!result) return result;
                }
            }

            // BUG: Don't know why this causes issues.
            // await DestroyEntityMetaData(folder);
            folder.Delete();

            return true;
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
        private async Task<bool> OverWriteFileAsync(FileInfo file, CancellationToken cancellationToken, IProgress<int> progress)
        {
            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                var buffer = Enumerable.Repeat((byte)0, MAX_BUFFER_SIZE).ToArray();

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
        private async Task<bool> DestroyEntityMetaData(FileSystemInfo entity, FileSystemType fileSystemType = FileSystemType.Unknown)
        {
            if (!entity.Exists)
                return false;

            await Task.Run(() =>
            {
                var directoryName = Path.GetDirectoryName(entity.FullName);
                // rename the file a few times to remove it from the file system table.
                for (var round = 0; round < OBFUSCATE_ROUNDS; round++)
                {
                    var newPath = Path.Combine(directoryName, Path.GetRandomFileName());

                    var file = entity as FileInfo;
                    var folder = entity as DirectoryInfo;

                    if (file != null)
                        file.MoveTo(newPath);
                    else if (folder != null)
                        folder.MoveTo(newPath);
                }

                var newTime = new DateTime(2000, 1, 1, 0, 0, 1);
                entity.LastAccessTime = newTime;
                entity.LastWriteTime = newTime;
                entity.CreationTime = newTime;
            });

            return true;
        }
        #endregion
    }
}
