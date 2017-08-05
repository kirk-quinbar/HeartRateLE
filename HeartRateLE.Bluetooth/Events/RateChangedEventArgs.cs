using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartRateLE.Bluetooth.Events
{
    public class RateChangedEventArgs : EventArgs
    {
        public int BeatsPerMinute { get; set; }
    }
}
