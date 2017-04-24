using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Wwssi.Bluetooth.Base;
using Wwssi.Bluetooth.Events;
using Wwssi.Bluetooth.HeartRate;
using Wwssi.Bluetooth.Parsers;

namespace Wwssi.Bluetooth
{
    /// <summary>
    /// 
    /// </summary>
    public class HeartRateMonitor
    {
        private BleHeartRate _heartRateDevice;
        private readonly HeartRateMeasurementParser _heartRateParser;
        private readonly BatteryLevelParser _batteryParser;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartRateMonitor"/> class.
        /// </summary>
        public HeartRateMonitor()
        {
            _heartRateParser = new HeartRateMeasurementParser();
            _batteryParser = new BatteryLevelParser();
        }

        /// <summary>
        /// Gets all paired BLE heart rate devices.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Schema.Device>> GetAllDevices()
        {
            var devices = await BleHeartRate.FindAll();

            return devices.Select(a => new Schema.Device()
            {
                IsConnected = a.IsConnected,
                Name = a.Name
            }).ToList();
        }

        /// <summary>
        /// Connects the specified BLE heart rate device name.
        /// </summary>
        /// <param name="deviceName">Name of the device.</param>
        /// <returns></returns>
        public async Task<Schema.Device> Connect (string deviceName)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                _heartRateDevice = await BleHeartRate.FirstOrDefault();
            }
            else
            {
                _heartRateDevice = await BleHeartRate.FindByName(deviceName);
            }
            
            if (_heartRateDevice == null)
            {
                return new Schema.Device()
                {
                    IsConnected = false,
                    ErrorMessage = "Could not find any heart rate device"
                };
            }

            // we should always monitor the connection status
            _heartRateDevice.DeviceConnectionStatusChanged -= BleDeviceConnectionStatusChanged;
            _heartRateDevice.DeviceConnectionStatusChanged += BleDeviceConnectionStatusChanged;

            // we can create value parser and listen for parsed values of given characteristic
            _heartRateParser.ConnectWithCharacteristic(_heartRateDevice.HeartRate.HeartRateMeasurement);
            _heartRateParser.ValueChanged -= BleDeviceValueChanged;
            _heartRateParser.ValueChanged += BleDeviceValueChanged;

            // connect also battery level parser to proper characteristic
            _batteryParser.ConnectWithCharacteristic(_heartRateDevice.BatteryService.BatteryLevel);

            //            // we can monitor raw data notified by BLE device for specific characteristic
            //            HrDevice.HeartRate.HeartRateMeasurement.ValueChanged -= HeartRateMeasurementOnValueChanged;
            //            HrDevice.HeartRate.HeartRateMeasurement.ValueChanged += HeartRateMeasurementOnValueChanged;

            // we could force propagation of event with connection status change, to run the callback for initial status
            _heartRateDevice.NotifyConnectionStatus();

            return new Schema.Device()
            {
                IsConnected = _heartRateDevice.IsConnected,
                Name = _heartRateDevice.Name
            };
        }

        /// <summary>
        /// Connects the first BLE heart rate device.
        /// </summary>
        public async Task<Schema.Device> Connect()
        {
            return await Connect(string.Empty);
        }

        private void BleDeviceValueChanged(object sender, ValueChangedEventArgs<short> e)
        {
            var args = new Events.RateChangedEventArgs()
            {
                BeatsPerMinute = e.Value
            };
            OnRateChanged(args);
        }

        private void BleDeviceConnectionStatusChanged(object sender, BleDeviceConnectionStatusChangedEventArgs e)
        {
            var args = new ConnectionStatusChangedEventArgs()
            {
                IsConnected = (e.ConnectionStatus == BluetoothConnectionStatus.Connected)
            };

            OnConnectionStatusChanged(args);
        }

        /// <summary>
        /// Disconnects the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect()
        {
            if (_heartRateDevice != null) await _heartRateDevice.Close();
        }

        /// <summary>
        /// Enables the notifications for the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public async Task EnableNotifications()
        {
            await _heartRateParser.EnableNotifications();
        }

        /// <summary>
        /// Disables the notifications for the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public async Task DisableNotifications()
        {
            await _heartRateParser.DisableNotifications();
        }

        /// <summary>
        /// Gets the device information for the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public async Task<Schema.DeviceInfo> GetDeviceInfo()
        {
            byte battery = await _batteryParser.Read();

            return new Schema.DeviceInfo()
            {
                Name = _heartRateDevice.Name,
                Firmware = await _heartRateDevice.DeviceInformation.FirmwareRevisionString.ReadAsString(),
                Hardware = await _heartRateDevice.DeviceInformation.HardwareRevisionString.ReadAsString(),
                Manufacturer = await _heartRateDevice.DeviceInformation.ManufacturerNameString.ReadAsString(),
                SerialNumber = await _heartRateDevice.DeviceInformation.SerialNumberString.ReadAsString(),
                ModelNumber = await _heartRateDevice.DeviceInformation.ModelNumberString.ReadAsString(),
                BatteryPercent = Convert.ToInt32(battery)
            };
        }
    }
}
