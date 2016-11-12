using RudeFox.Helpers;
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
        private const int MAX_BUFFER_SIZE = 1 * Constants.MEGABYTE;

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
        public async Task<bool> ShredItemAsync(string path, CancellationToken cancellationToken, IProgress<double> progress)
        {
            FileSystemInfo item;

            if (File.Exists(path))
                item = new FileInfo(path);
            else if (Directory.Exists(path))
                item = new DirectoryInfo(path);
            else
                throw new ArgumentException($"This path does not exist: {path}");

            var file = item as FileInfo;
            var folder = item as DirectoryInfo;

            if (file != null)
            {
                if (!file.Exists) return false;

                return await ShredFileAsync(file, cancellationToken, progress).ConfigureAwait(false);
            }

            if (cancellationToken != null)
                cancellationToken.ThrowIfCancellationRequested();

            if (folder != null)
            {
                if (!folder.Exists) return false;

                return await ShredFolderAsync(folder, cancellationToken, progress).ConfigureAwait(false);
            }

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

        public async Task<bool> ShredFileAsync(FileInfo file, CancellationToken cancellationToken, IProgress<double> progress)
        {
            var writeProgress = new Progress<double>();
            writeProgress.ProgressChanged += (sender, percent) =>
            {
                if (progress != null) progress.Report(percent - (percent * 0.001));
            };

            if (!file.Exists) return false;

            if (cancellationToken != null) cancellationToken.ThrowIfCancellationRequested();

            var result = await OverWriteFileAsync(file, cancellationToken, writeProgress).ConfigureAwait(false);
            if (!result) return result;

            if (cancellationToken != null) cancellationToken.ThrowIfCancellationRequested();

            await Task.Run(() =>
            {
                var newPath = Path.Combine(Path.GetDirectoryName(file.FullName), _random.NextDouble().ToString().Substring(2));
                file.MoveTo(Path.GetRandomFileName());
                file.Delete();
            }).ConfigureAwait(false);

            if (progress != null) progress.Report(1.0);

            return true;
        }

        public async Task<bool> ShredFolderAsync(DirectoryInfo folder, CancellationToken cancellationToken, IProgress<double> progress)
        {
            if (!folder.Exists) return false;

            var totalLength = await GetFolderSize(folder);
            var bytesComplete = 0.0;

            Progress<double> itemProgress;
            foreach (var info in folder.EnumerateFileSystemInfos())
            {
                itemProgress = new Progress<double>();

                if (cancellationToken != null)
                    cancellationToken.ThrowIfCancellationRequested();

                var file = info as FileInfo;
                var dir = info as DirectoryInfo;

                if (file != null)
                {
                    var length = file.Length;

                    itemProgress.ProgressChanged += (sender, percent) =>
                    {
                        var totalProgress = (bytesComplete + (percent * length)) / totalLength;
                        progress.Report(totalProgress);

                        if (percent == 1.0)
                            bytesComplete += length;
                    };
                    var result = await ShredFileAsync(file, cancellationToken, itemProgress).ConfigureAwait(false);
                    if (!result) return result;
                }

                if (dir != null)
                {
                    var length = await GetFolderSize(dir);
                    itemProgress.ProgressChanged += (sender, percent) =>
                    {
                        var totalProgress = (bytesComplete + (percent * length)) / totalLength;
                        progress.Report(totalProgress);

                        if (percent == 1.0)
                            bytesComplete += length;
                    };
                    var result = await ShredFolderAsync(dir, cancellationToken, itemProgress).ConfigureAwait(false);
                    if (!result) return result;
                }
            }

            folder.Delete();
            return true;
        }

        private async Task<bool> OverWriteFileAsync(FileInfo file, CancellationToken cancellationToken, IProgress<double> progress)
        {
            if (!file.Exists) return false;

            using (var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                for (var length = file.Length; length > 0; length -= MAX_BUFFER_SIZE)
                {

                    int bufferSize = (length > MAX_BUFFER_SIZE) ? MAX_BUFFER_SIZE : (int)length;
                    var buffer = new byte[bufferSize];

                    for (var index = 0; index < bufferSize; index++)
                    {
                        // set the value to a random byte
                        // buffer[index] = (byte)(_random.Next() % 256);
                        buffer[index] = 0;
                    }

                    await stream.WriteAsync(buffer, 0, bufferSize).ConfigureAwait(false);
                    await stream.FlushAsync().ConfigureAwait(false);

                    if (cancellationToken != null)
                        cancellationToken.ThrowIfCancellationRequested();
                    if (progress != null)
                    {
                        var percent = 1.0 - (length / (double)file.Length);
                        progress.Report(percent);
                    }
                }

                await Task.Run(() =>
                {
                    stream.SetLength(0);
                }).ConfigureAwait(false);
            }

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
    }
}
