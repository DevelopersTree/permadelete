using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete.Updater
{
    public class UpdateInfo
    {
        public UpdateInfo()
        {
            NewFiles = new List<File>();
            ObsoleteFiles = new List<string>();
        }

        public Version Version { get; set; }
        public UpdateType Type { get; set; }
        public string ChangeListLink { get; set; }
        public string Path { get; set; }
        public long Length { get; set; }
        public List<File> NewFiles { get; set; }
        public List<string> ObsoleteFiles { get; set; }
    }
}
