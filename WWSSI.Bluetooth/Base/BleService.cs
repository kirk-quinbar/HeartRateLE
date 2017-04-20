/******************************************************************************
The MIT License (MIT)

Copyright (c) 2016 Matchbox Mobile Limited <info@matchboxmobile.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*******************************************************************************/

// This file was generated by Bluetooth (R) Developer Studio on 2016.03.17 21:39
// with plugin Windows 10 UWP Client (version 1.0.0 released on 2016.03.16).
// Plugin developed by Matchbox Mobile Limited.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Media.Devices;

namespace Wwssi.Bluetooth.Base
{ 
    /// <summary>
    /// Base class for all services included in the Bluetooth Smart device.
    /// </summary>
    internal class BleService : BleBase
    {
        /// <summary>
        /// Tells if given service is mandatory for the device.
        /// </summary>
        public bool IsMandatory { get; private set; }
        
        /// <summary>
        /// Gives the GUID of the service.
        /// </summary>
        public Guid UUID { get; private set; }

        private List<BleCharacteristic> AllCharacteristics { get; } = new List<BleCharacteristic>();
        
        /// <summary>
        /// Returns underlying GattDeviceService object (from standard Windows 10 SDK)
        /// </summary>
        public GattDeviceService DeviceService { get; private set; }
        
        /// <summary>
        /// Tells if given service is available (it will be false if the device doesn't implement
        /// that service)
        /// </summary>
        public bool IsAvailable => DeviceService != null;

        
        /// <summary>
        /// Initializes whole service object. Needs to be called before any use. It is called
        /// automatically by parent BleDevice object while its initialization.
        /// </summary>
        public void Initialize(BleDevice device)
        {
            try
            {
                DeviceService = device.LEDevice.GetGattService(UUID);
            }
            catch (Exception e)
            {
                DeviceService = null;
            }

            if (!IsAvailable)
                return;

            var props = this.GetType().GetProperties();
            foreach (var property in props)
            {
                if (!property.PropertyType.IsAssignableFrom(typeof(BleCharacteristic)))
                    continue;

                BleCharacteristic characteristic = (BleCharacteristic) property.GetValue(this);
                if (characteristic != null)
                {
                    AllCharacteristics.Add(characteristic);
                    characteristic.Initialize(this);
                }
            }
        }

        
        /// <summary>
        /// Disposing and cleaning after service. It is automatically called by parent's BleDevice
        /// Close() function.
        /// </summary>
        public async Task Close()
        {
            foreach (var ch in AllCharacteristics)
            {
                await ch.Close();
            }

            AllCharacteristics.Clear();
            DeviceService?.Dispose();
            DeviceService = null;
        }

        protected BleService(string id, bool mandatory)
        {
            UUID = id.ToGuid();
            IsMandatory = mandatory;
        }
    }
}
