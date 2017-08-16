﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeartRateLE.Bluetooth.Schema
{
    public class HeartRateDevice
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public bool IsConnected { get; set; }
        public string ErrorMessage { get; set; }
    }
}