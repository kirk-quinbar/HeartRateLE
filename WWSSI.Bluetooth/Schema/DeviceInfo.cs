using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wwssi.Bluetooth.Schema
{
    public class DeviceInfo
    {
        public string Name { get; set; }
        public string Firmware { get; set; }
        public string Hardware { get; set; }
        public string Manufacturer { get; set; }
        public string SerialNumber { get; set; }
        public string ModelNumber { get; set; }
        public int BatteryPercent { get; set; }
    }
}
