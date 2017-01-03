using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RudeFox.Updater
{
    public class UpdateAttempt
    {
        public Version Version { get; set; }
        public DateTime Date { get; set; }
        public bool DownloadCompleted { get; set; }
    }
}
