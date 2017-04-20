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
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace Wwssi.Bluetooth.Base
{
    /// <summary>
    /// Top Level Bluetooth device's wrapper class. It represents the whole device and includes references
    /// to all device's information and services.
    /// </summary>
    internal class BleDevice : BleBase
    {
        private List<BleService> AllServices { get; } = new List<BleService>();

        /// <summary>
        /// DeviceInformation object for underlying BT device.
        /// </summary>
        public DeviceInformation DeviceInfo { get; private set; }

        /// <summary>
        /// Windows 10 SDK BluetoothLEDevice object which is wrapped by current class.
        /// </summary>
        public BluetoothLEDevice LEDevice { get; private set; }

        /// <summary>
        /// Shortcut to get the device's BT MAC address.
        /// </summary>
        public ulong? BluetoothAddress => LEDevice?.BluetoothAddress;

        /// <summary>
        /// SHortcut to get the device's name.
        /// </summary>
        public string Name => DeviceInfo?.Name;

        /// <summary>
        /// Tells if given devices is currently in connected state.
        /// </summary>
        public bool IsConnected => LEDevice?.ConnectionStatus == BluetoothConnectionStatus.Connected;

        /// <summary>
        /// Gets current connection status of the device.
        /// </summary>
        public BluetoothConnectionStatus ConnectionStatus
        {
            get
            {
                if(LEDevice == null)
                    return BluetoothConnectionStatus.Disconnected;
                return LEDevice.ConnectionStatus;
            }
        }

        /// <summary>
        /// Handlers for connection status changed events.
        /// </summary>
        public event EventHandler<BleDeviceConnectionStatusChangedEventArgs> DeviceConnectionStatusChanged;

        protected BleDevice(DeviceInformation deviceInformation, BluetoothLEDevice leDevice)
        {
            DeviceInfo = deviceInformation;
            LEDevice = leDevice;
        }

        private void ConnectionStatusChanged(BluetoothLEDevice device, object arg)
        {
          var statusArgs = new BleDeviceConnectionStatusChangedEventArgs(device.ConnectionStatus);
          DeviceConnectionStatusChanged?.Invoke(this, statusArgs);
        }

        /// <summary>
        /// Forces object to emit connection status changed event, even without real change.
		/// Could be used at initialization moment to unify how code is handling the state
		/// of connection.
        /// </summary>
        public void NotifyConnectionStatus()
        {
            ConnectionStatusChanged(this.LEDevice, null);
        }

		/// <summary>
        /// Initializes the device, so it can be used. Only after this call, developers can
		/// access included services and characteristics.
        /// </summary>
        protected void Initialize()
        {
            foreach (var service in AllServices)
            {
                service.Initialize(this);
            }

            LEDevice.ConnectionStatusChanged += ConnectionStatusChanged;
        }

        /// <summary>
        /// Closing and disposing all underlying resources. It cleans all included objects as well.
		/// It should be called as soon as the object is not needed anymore. After this call
		/// object is unusable - any further communication with the same device requires to create
		/// completely new BleDevice object from scratch.
        /// </summary>
        public async Task Close()
        {
            foreach (var ch in AllServices)
            {
                await ch.Close();
            }

            AllServices.Clear();

            if(LEDevice != null)
                LEDevice.ConnectionStatusChanged -= ConnectionStatusChanged;
            LEDevice?.Dispose();
            LEDevice = null;
            DeviceInfo = null;
        }

        protected void RegisterNewService(BleService service)
        {
            AllServices.Add(service);
        }

        protected static bool CheckForCompatibility(BluetoothLEDevice device, Guid uuid)
        {
            return device.GattServices.Any(service => uuid.Equals(service.Uuid));
        }
    }

	/// <summary>
	/// Custom event args type, for connection status changed events.
	/// </summary>
    internal class BleDeviceConnectionStatusChangedEventArgs : EventArgs
    {
        public BluetoothConnectionStatus ConnectionStatus { get; }

        public BleDeviceConnectionStatusChangedEventArgs(BluetoothConnectionStatus status)
        {
            ConnectionStatus = status;
        }
    }
}
