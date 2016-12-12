using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RudeFox.Updater
{
    public class UpdateInfo
    {
        public Version Version { get; set; }
        public UpdateType Type { get; set; }
        public string ChangeListLink { get; set; }
        public string Path { get; set; }
        public long Length { get; set; }
        public List<File> Files { get; set; }
    }
}
