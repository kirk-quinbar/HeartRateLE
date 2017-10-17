using HeartRateLE.Bluetooth.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace HeartRateLE.Bluetooth
{
    public class HeartRateMonitor
    {
        private BluetoothLEDevice _heartRateDevice = null;

        // Only one registered characteristic at a time.
        private GattCharacteristic registeredCharacteristic;

        /// <summary>
        /// Occurs when [connection status changed].
        /// </summary>
        public event EventHandler<Events.ConnectionStatusChangedEventArgs> ConnectionStatusChanged;
        /// <summary>
        /// Raises the <see cref="E:ConnectionStatusChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="Events.ConnectionStatusChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnConnectionStatusChanged(Events.ConnectionStatusChangedEventArgs e)
        {
            ConnectionStatusChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when [value changed].
        /// </summary>
        public event EventHandler<Events.RateChangedEventArgs> RateChanged;
        /// <summary>
        /// Raises the <see cref="E:ValueChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="Events.RateChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnRateChanged(Events.RateChangedEventArgs e)
        {
            RateChanged?.Invoke(this, e);
        }


        public async Task<Schema.HeartRateDevice> ConnectAsync(string deviceId)
        {
            _heartRateDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            if (_heartRateDevice == null)
            {
                return new Schema.HeartRateDevice()
                {
                    IsConnected = false,
                    ErrorMessage = "Could not find any heart rate device"
                };
            }

            // we should always monitor the connection status
            _heartRateDevice.ConnectionStatusChanged -= _heartRateDevice_ConnectionStatusChanged;
            _heartRateDevice.ConnectionStatusChanged += _heartRateDevice_ConnectionStatusChanged;

            // we could force propagation of event with connection status change, to run the callback for initial status
            _heartRateDevice_ConnectionStatusChanged(_heartRateDevice, null);

            return new Schema.HeartRateDevice()
            {
                IsConnected = _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected,
                Name = _heartRateDevice.Name
            };
        }

        /// <summary>
        /// Disconnects the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public void Disconnect()
        {
            if (_heartRateDevice != null && _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                _heartRateDevice.ConnectionStatusChanged -= _heartRateDevice_ConnectionStatusChanged;
                _heartRateDevice.Dispose();
                _heartRateDevice = null;
            }
        }
        private void _heartRateDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            var result = new ConnectionStatusChangedEventArgs()
            {
                IsConnected = (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
            };

            OnConnectionStatusChanged(result);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get { return _heartRateDevice != null ? _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected : false; }
        }

        /// <summary>
        /// Gets the device information for the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public Schema.HeartRateDeviceInfo GetDeviceInfo()
        {
            if (_heartRateDevice != null && _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                //byte battery = await _batteryParser.ReadAsync();

                return new Schema.HeartRateDeviceInfo()
                {
                    DeviceId = _heartRateDevice.DeviceId,
                    Name = _heartRateDevice.Name
                    //Firmware = await _heartRateDevice.DeviceInformation.FirmwareRevisionString.ReadAsStringAsync(),
                    //Hardware = await _heartRateDevice.DeviceInformation.HardwareRevisionString.ReadAsStringAsync(),
                    //Manufacturer = await _heartRateDevice.DeviceInformation.ManufacturerNameString.ReadAsStringAsync(),
                    //SerialNumber = await _heartRateDevice.DeviceInformation.SerialNumberString.ReadAsStringAsync(),
                    //ModelNumber = await _heartRateDevice.DeviceInformation.ModelNumberString.ReadAsStringAsync(),
                    //BatteryPercent = Convert.ToInt32(battery)
                };
            }
            else
            {
                return new Schema.HeartRateDeviceInfo();
            }
        }

    }
}