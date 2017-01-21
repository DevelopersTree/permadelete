using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete.Updater
{
    public class UpdateAttempt
    {
        public UpdateAttempt()
        {
            FilesToDelete = new List<string>();
        }
        public Version Version { get; set; }
        public DateTime Date { get; set; }
        public List<string> FilesToDelete { get; set; }
        public bool DownloadCompleted { get; set; }
    }
}
