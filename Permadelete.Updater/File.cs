using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete.Updater
{
    public class File
    {
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
        public long Length { get; set; }
        public bool Delete { get; set; }
    }
}
