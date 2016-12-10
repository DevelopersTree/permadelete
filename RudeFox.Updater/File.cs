using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RudeFox.Updater
{
    public class File
    {
        public File()
        {
            Version = new Version(0, 0, 0, 0);
            Name = "";
            Folder = "";
        }

        public Version Version { get; set; }
        public string Name { get; set; }
        public string Folder { get; set; }
        [JsonIgnore]
        public string Extention
        {
            get
            {
                return Name == null ? null : Name.Split('.').LastOrDefault();
            }
        }
        public bool Overwrite { get; set; }
        [JsonIgnore]
        public bool IsDownloaded { get; set; }
    }
}
