using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using HeartRateLE.Bluetooth.Base;
using HeartRateLE.Bluetooth.Events;
using HeartRateLE.Bluetooth.HeartRate;
using HeartRateLE.Bluetooth.Parsers;

namespace HeartRateLE.Bluetooth
{
    /// <summary>
    /// 
    /// </summary>
    public class HeartRateMonitorOld
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
        /// Initializes a new instance of the <see cref="HeartRateMonitorOld"/> class.
        /// </summary>
        public HeartRateMonitorOld()
        {
            _heartRateParser = new HeartRateMeasurementParser();
            _batteryParser = new BatteryLevelParser();
        }

        /// <summary>
        /// Gets all paired BLE heart rate devices.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Schema.HeartRateDevice>> GetAllDevicesAsync()
        {
            var devices = await BleHeartRate.FindAll();

            return devices.Select(a => new Schema.HeartRateDevice()
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
        public async Task<Schema.HeartRateDevice> ConnectAsync(string deviceName)
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
                return new Schema.HeartRateDevice()
                {
                    IsConnected = false,
                    ErrorMessage = "Could not find any heart rate device"
                };
            }

            // we should always monitor the connection status
            _heartRateDevice.DeviceConnectionStatusChanged -= BleDeviceConnectionStatusChanged;
            _heartRateDevice.DeviceConnectionStatusChanged += BleDeviceConnectionStatusChanged;

            // we can create value parser and listen for parsed values of given characteristic
            await _heartRateParser.ConnectWithCharacteristicAsync(_heartRateDevice.HeartRate.HeartRateMeasurement);
            _heartRateParser.ValueChanged -= BleDeviceValueChanged;
            _heartRateParser.ValueChanged += BleDeviceValueChanged;

            // connect also battery level parser to proper characteristic
            await _batteryParser.ConnectWithCharacteristicAsync(_heartRateDevice.BatteryService.BatteryLevel);

            //            // we can monitor raw data notified by BLE device for specific characteristic
            //            HrDevice.HeartRate.HeartRateMeasurement.ValueChanged -= HeartRateMeasurementOnValueChanged;
            //            HrDevice.HeartRate.HeartRateMeasurement.ValueChanged += HeartRateMeasurementOnValueChanged;

            // we could force propagation of event with connection status change, to run the callback for initial status
            _heartRateDevice.NotifyConnectionStatus();

            return new Schema.HeartRateDevice()
            {
                IsConnected = _heartRateDevice.IsConnected,
                Name = _heartRateDevice.Name
            };
        }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get { return _heartRateDevice != null ? _heartRateDevice.IsConnected : false; }
        }

        /// <summary>
        /// Connects the first BLE heart rate device.
        /// </summary>
        public async Task<Schema.HeartRateDevice> ConnectAsync()
        {
            return await ConnectAsync(string.Empty);
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
        public async Task DisconnectAsync()
        {
            if (_heartRateDevice != null && _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
                await _heartRateDevice.CloseAsync();
        }

        /// <summary>
        /// Enables the notifications for the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public async Task EnableNotificationsAsync()
        {
            await _heartRateParser.EnableNotificationsAsync();
        }

        /// <summary>
        /// Disables the notifications for the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public async Task DisableNotificationsAsync()
        {
            await _heartRateParser.DisableNotificationsAsync();
        }

        /// <summary>
        /// Gets the device information for the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public async Task<Schema.HeartRateDeviceInfo> GetDeviceInfoAsync()
        {
            if (_heartRateDevice != null && _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                byte battery = await _batteryParser.ReadAsync();

                return new Schema.HeartRateDeviceInfo()
                {
                    DeviceId = _heartRateDevice.DeviceInfo.Id,
                    Name = _heartRateDevice.Name,
                    Firmware = await _heartRateDevice.DeviceInformation.FirmwareRevisionString.ReadAsStringAsync(),
                    Hardware = await _heartRateDevice.DeviceInformation.HardwareRevisionString.ReadAsStringAsync(),
                    Manufacturer = await _heartRateDevice.DeviceInformation.ManufacturerNameString.ReadAsStringAsync(),
                    SerialNumber = await _heartRateDevice.DeviceInformation.SerialNumberString.ReadAsStringAsync(),
                    ModelNumber = await _heartRateDevice.DeviceInformation.ModelNumberString.ReadAsStringAsync(),
                    BatteryPercent = Convert.ToInt32(battery)
                };
            }
            else
            {
                return new Schema.HeartRateDeviceInfo();
            }
        }
    }
}
