using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wwssi.Bluetooth.Events
{
    public class DeviceRemovedEventArgs
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public Dictionary<string,object> Properties { get; set; }
    }
}
