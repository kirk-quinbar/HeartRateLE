using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wwssi.Bluetooth.Schema
{
    public class HeartRateDevice
    {
        public string Name { get; set; }
        public bool IsConnected { get; set; }
        public string ErrorMessage { get; set; }
    }
}
