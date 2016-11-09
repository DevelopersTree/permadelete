using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        private const int KILOBYTE = 1024;
        private const int MEGABYTE = KILOBYTE * KILOBYTE;
        private const int MAX_BUFFER_SIZE = 64 * MEGABYTE;

        Random _random = new Random();
        #endregion

        #region Properties
        private static ShredderService _instance;
        public static ShredderService Instance
        {
            get { return _instance ?? (_instance = new ShredderService()); }
        }
        #endregion

        #region Public Methods
        public async Task<bool> ShredEntityAsync(FileSystemInfo entity)
        {
            var file = entity as FileInfo;
            var folder = entity as DirectoryInfo;

            if (file != null)
            {
                if (!file.Exists) return false;

                return await ShredFileAsync(file).ConfigureAwait(false);
            }
            else if (folder != null)
            {
                if (!folder.Exists) return false;

                return await ShredFolderAsync(folder).ConfigureAwait(false);
            }

            return false;
        }

        public async Task<bool> ShredFileAsync(FileInfo file)
        {
            if (!file.Exists) return false;

            var result = await OverWriteFileAsync(file).ConfigureAwait(false);
            if (!result) return result;

            await Task.Factory.StartNew(() =>
            {
                var newPath = Path.Combine(Path.GetDirectoryName(file.FullName), _random.NextDouble().ToString().Substring(2));
                file.MoveTo(Path.GetRandomFileName());
                file.Delete();
            }).ConfigureAwait(false);

            return true;
        }

        public async Task<bool> ShredFolderAsync(DirectoryInfo folder)
        {
            if (!folder.Exists) return false;

            foreach (var info in folder.EnumerateFileSystemInfos())
            {
                var file = (FileInfo)info;
                var dir = (DirectoryInfo)info;

                if (file != null)
                {
                    var result = await ShredFileAsync(file).ConfigureAwait(false);
                    if (!result) return result;
                }

                if (dir != null)
                {
                    var result = await ShredFolderAsync(dir).ConfigureAwait(false);
                    if (!result) return result;
                }
            }

            return true;
        }

        private async Task<bool> OverWriteFileAsync(FileInfo file)
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
                }


                await Task.Factory.StartNew(() =>
                {
                    stream.SetLength(0);
                }).ConfigureAwait(false);
            }

            return true;
        }
        #endregion
    }
}
