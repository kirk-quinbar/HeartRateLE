using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonitorUI;
using System.Diagnostics;

namespace MonitorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Wwssi.Bluetooth.HeartRateMonitor _heartRateMonitor;
        public MainWindow()
        {
            InitializeComponent();
            //Application.Current.Suspending += CurrentOnSuspending;
            _heartRateMonitor = new Wwssi.Bluetooth.HeartRateMonitor();
        }

        //private async void CurrentOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        //{
        ////    if (HrDevice != null) await HrDevice.Close();
        //}

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            var device = await _heartRateMonitor.Connect();
            if (!device.IsConnected)
            {
                MessageBox.Show(device.ErrorMessage);
            }
            else
            {
                MessageBox.Show(device.Name);
            }
            //d("Button CONNECT clicked.");
            //HrDevice = await BleHeartRate.FirstOrDefault();
            //if (HrDevice == null)
            //{
            //    d("I was not able to find any HR device!");
            //    return;
            //}

            //d("Found device: " + HrDevice.Name + " IsConnected=" + HrDevice.IsConnected);
            //// we should always monitor the connection status
            //HrDevice.DeviceConnectionStatusChanged -= HrDeviceOnDeviceConnectionStatusChanged;
            //HrDevice.DeviceConnectionStatusChanged += HrDeviceOnDeviceConnectionStatusChanged;

            //// we can create value parser and listen for parsed values of given characteristic
            //HrParser.ConnectWithCharacteristic(HrDevice.HeartRate.HeartRateMeasurement);
            //HrParser.ValueChanged -= HrParserOnValueChanged;
            //HrParser.ValueChanged += HrParserOnValueChanged;

            //// connect also battery level parser to proper characteristic
            //BatteryParser.ConnectWithCharacteristic(HrDevice.BatteryService.BatteryLevel);

            //// we can monitor raw data notified by BLE device for specific characteristic
            //HrDevice.HeartRate.HeartRateMeasurement.ValueChanged -= HeartRateMeasurementOnValueChanged;
            //HrDevice.HeartRate.HeartRateMeasurement.ValueChanged += HeartRateMeasurementOnValueChanged;

            //// we could force propagation of event with connection status change, to run the callback for initial status
            //HrDevice.NotifyConnectionStatus();
        }

        //private void HeartRateMeasurementOnValueChanged(object sender, ValueChangedEventArgs args)
        //{
        //    d("RAW value change event received:" + args.Value);
        //}

        //private async void HrParserOnValueChanged(object device, ValueChangedEventArgs<short> arg)
        //{
        //    await RunOnUiThread(() =>
        //    {
        //        d("Got new measurement: " + arg.Value);
        //        TxtHr.Text = String.Format("{0} bpm", arg.Value);
        //    });
        //}

        //private async void HrDeviceOnDeviceConnectionStatusChanged(object device, BleDeviceConnectionStatusChangedEventArgs args)
        //{
        //    d("Current connection status is: " + args.ConnectionStatus);
        //    await RunOnUiThread(async () =>
        //    {
        //        bool connected = (args.ConnectionStatus == BluetoothConnectionStatus.Connected);
        //        if (connected)
        //        {
        //            TxtStatus.Text = HrDevice.Name + ": connected";
        //            byte battery = await BatteryParser.Read();
        //            TxtBattery.Text = String.Format("battery level: {0}%", battery);
        //        }
        //        else
        //        {
        //            TxtStatus.Text = "disconnected";
        //            TxtBattery.Text = "battery level: --";
        //            TxtHr.Text = "--";
        //        }

        //        BtnStart.IsEnabled = connected;
        //        BtnStop.IsEnabled = connected;
        //        BtnReadInfo.IsEnabled = connected;
        //    });
        //}

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            //d("Button START clicked.");
            //await HrParser.EnableNotifications();
            //d("Notification enabled");
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            //d("Button STOP clicked.");
            //await HrParser.DisableNotifications();
            //d("Notification disabled.");
            //TxtHr.Text = "--";
        }

        private async void BtnReadInfo_Click(object sender, RoutedEventArgs e)
        {
            var deviceInfo = await _heartRateMonitor.GetDeviceInfo();

            //d("Reading DeviceInformation Characteristic ...");
            //var firmware = await HrDevice.DeviceInformation.FirmwareRevisionString.ReadAsString();

            //var hardware = await HrDevice.DeviceInformation.HardwareRevisionString.ReadAsString();
            //var producer = await HrDevice.DeviceInformation.ManufacturerNameString.ReadAsString();
            //var serialNumber = await HrDevice.DeviceInformation.SerialNumberString.ReadAsString();
            //var modelNumber = await HrDevice.DeviceInformation.ModelNumberString.ReadAsString();
            
            d($" Manufacturer : {deviceInfo.Manufacturer}"); d("");
            d($"    Model : {deviceInfo.ModelNumber}"); d("");
            d($"      S/N : {deviceInfo.SerialNumber}"); d("");
            d($" Firmware : {deviceInfo.Firmware}"); d("");
            d($" Hardware : {deviceInfo.Hardware}"); d("");

            //// update also battery
            //byte battery = await BatteryParser.Read();
            TxtBattery.Text = $"battery level: {deviceInfo.BatteryPercent}%";
        }

        [Conditional("DEBUG")]
        private void d(string txt)
        {
            Debug.WriteLine(txt);
        }

        //private async Task RunOnUiThread(Action a)
        //{
        //    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //    {
        //        a();
        //    });
        //}

        //private BleHeartRate HrDevice { get; set; }
        //private HeartRateMeasurementParser HrParser { get; } = new HeartRateMeasurementParser();
        //private BatteryLevelParser BatteryParser { get; } = new BatteryLevelParser();

    }
}
