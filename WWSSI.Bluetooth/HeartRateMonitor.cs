using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wwssi.Bluetooth.Base;
using Wwssi.Bluetooth.HeartRate;
using Wwssi.Bluetooth.Parsers;

namespace Wwssi.Bluetooth
{
    public class HeartRateMonitor
    {
        private BleHeartRate _heartRateDevice;
        private readonly HeartRateMeasurementParser _heartRateParser;
        private readonly BatteryLevelParser _batteryParser;

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

            //            d("Found device: " + HrDevice.Name + " IsConnected=" + HrDevice.IsConnected);
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

        private void BleDeviceValueChanged(object device, ValueChangedEventArgs<short> arg)
        {
            //await RunOnUiThread(() =>
            //{
            //    d("Got new measurement: " + arg.Value);
            //    TxtHr.Text = String.Format("{0} bpm", arg.Value);
            //});
        }
        private void BleDeviceConnectionStatusChanged(object device, BleDeviceConnectionStatusChangedEventArgs args)
        {
            //d("Current connection status is: " + args.ConnectionStatus);
            //await RunOnUiThread(async () =>
            //{
            //    bool connected = (args.ConnectionStatus == BluetoothConnectionStatus.Connected);
            //    if (connected)
            //    {
            //        TxtStatus.Text = HrDevice.Name + ": connected";
            //        byte battery = await BatteryParser.Read();
            //        TxtBattery.Text = String.Format("battery level: {0}%", battery);
            //    }
            //    else
            //    {
            //        TxtStatus.Text = "disconnected";
            //        TxtBattery.Text = "battery level: --";
            //        TxtHr.Text = "--";
            //    }

            //    BtnStart.IsEnabled = connected;
            //    BtnStop.IsEnabled = connected;
            //    BtnReadInfo.IsEnabled = connected;
            //});
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
