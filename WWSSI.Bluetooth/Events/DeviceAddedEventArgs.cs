using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wwssi.Bluetooth.Events
{
    public class DeviceAddedEventArgs
    {
        public string Id { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDefault { get; set; }
        public string Name { get; set; }
        public bool IsPaired { get; set; }
        public string Kind { get; set; }
        public Dictionary<string,object> Properties { get; set; }
    }
}
