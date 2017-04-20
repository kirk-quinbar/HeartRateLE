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
    public class HeartRateMonitor
    {
        private BleHeartRate _heartRateDevice;
        private readonly HeartRateMeasurementParser _heartRateParser;
        private readonly BatteryLevelParser _batteryParser;

        public event EventHandler<Events.ConnectionStatusChangedEventArgs> ConnectionStatusChanged;
        protected virtual void OnConnectionStatusChanged(Events.ConnectionStatusChangedEventArgs e)
        {
            ConnectionStatusChanged?.Invoke(this, e);
        }

        public event EventHandler<Events.ValueChangedEventArgs> ValueChanged;
        protected virtual void OnValueChanged(Events.ValueChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        public HeartRateMonitor()
        {
            _heartRateParser = new HeartRateMeasurementParser();
            _batteryParser = new BatteryLevelParser();
        }

        public async Task<Schema.Device> Connect()
        {
            _heartRateDevice = await BleHeartRate.FirstOrDefault();
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

        private void BleDeviceValueChanged(object sender, ValueChangedEventArgs<short> e)
        {
            var args = new Events.ValueChangedEventArgs()
            {
                BeatsPerMinute = e.Value
            };
            OnValueChanged(args);
        }

        private void BleDeviceConnectionStatusChanged(object sender, BleDeviceConnectionStatusChangedEventArgs e)
        {
            var args = new ConnectionStatusChangedEventArgs()
            {
                IsConnected = (e.ConnectionStatus == BluetoothConnectionStatus.Connected)
            };

            OnConnectionStatusChanged(args);
        }

        public async Task Disconnect()
        {
            if (_heartRateDevice != null) await _heartRateDevice.Close();
        }

        public async Task EnableNotifications()
        {
            await _heartRateParser.EnableNotifications();
        }

        public async Task DisableNotifications()
        {
            await _heartRateParser.DisableNotifications();
        }

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
