using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiMonitorSwitcher.Model
{
    public class Monitor
    {
        public string DeviceId { get; set; }
        public string Description { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsAttached { get; set; }

    }
}
