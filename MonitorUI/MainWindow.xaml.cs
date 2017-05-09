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
using Wwssi.Bluetooth.Events;
using System.ComponentModel;

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

        protected async override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            var allDevices = await _heartRateMonitor.GetAllDevicesAsync();
            DeviceComboBox.ItemsSource = allDevices;
            DeviceComboBox.DisplayMemberPath = "Name";

            // we should always monitor the connection status
            _heartRateMonitor.ConnectionStatusChanged -= HrDeviceOnDeviceConnectionStatusChanged;
            _heartRateMonitor.ConnectionStatusChanged += HrDeviceOnDeviceConnectionStatusChanged;

            //// we can create value parser and listen for parsed values of given characteristic
            //HrParser.ConnectWithCharacteristic(HrDevice.HeartRate.HeartRateMeasurement);
            _heartRateMonitor.RateChanged -= HrParserOnValueChanged;
            _heartRateMonitor.RateChanged += HrParserOnValueChanged;
        }

        protected async override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            await _heartRateMonitor.DisconnectAsync();
        }
        //private async void CurrentOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        //{
        ////    if (HrDevice != null) await HrDevice.Close();
        //}

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (Wwssi.Bluetooth.Schema.HeartRateDevice)DeviceComboBox.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("Must select a device to connect");
                return;
            }

            var device = await _heartRateMonitor.ConnectAsync(selectedItem.Name);

            d("Button CONNECT clicked.");
            //HrDevice = await BleHeartRate.FirstOrDefault();
            if (device == null)
            {
                MessageBox.Show("Could not find any heart rate device!");
                return;
            }

            d("Found device: " + device.Name + " IsConnected=" + device.IsConnected);
            //// we should always monitor the connection status
            //_heartRateMonitor.ConnectionStatusChanged -= HrDeviceOnDeviceConnectionStatusChanged;
            //_heartRateMonitor.ConnectionStatusChanged += HrDeviceOnDeviceConnectionStatusChanged;

            ////// we can create value parser and listen for parsed values of given characteristic
            ////HrParser.ConnectWithCharacteristic(HrDevice.HeartRate.HeartRateMeasurement);
            //_heartRateMonitor.ValueChanged -= HrParserOnValueChanged;
            //_heartRateMonitor.ValueChanged += HrParserOnValueChanged;

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

        private async void HrParserOnValueChanged(object sender, RateChangedEventArgs arg)
        {
            await RunOnUiThread(() =>
            {
                d("Got new measurement: " + arg.BeatsPerMinute);
                TxtHr.Text = String.Format("{0} bpm", arg.BeatsPerMinute);
            });
        }

        private async void HrDeviceOnDeviceConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            d("Current connection status is: " + args.IsConnected);
            await RunOnUiThread(async () =>
            {
                bool connected = args.IsConnected;
                if (connected)
                {
                    var device = await _heartRateMonitor.GetDeviceInfoAsync();
                    TxtStatus.Text = device.Name + ": connected";
                    TxtBattery.Text = String.Format("battery level: {0}%", device.BatteryPercent);
                }
                else
                {
                    TxtStatus.Text = "disconnected";
                    TxtBattery.Text = "battery level: --";
                    TxtHr.Text = "--";
                }

                BtnStart.IsEnabled = connected;
                BtnStop.IsEnabled = connected;
                BtnReadInfo.IsEnabled = connected;
            });
        }

        private async void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            d("Button START clicked.");
            await _heartRateMonitor.EnableNotificationsAsync();
            d("Notification enabled");
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            d("Button STOP clicked.");
            await _heartRateMonitor.DisableNotificationsAsync();
            d("Notification disabled.");
            TxtHr.Text = "--";
        }

        private async void BtnReadInfo_Click(object sender, RoutedEventArgs e)
        {
            var deviceInfo = await _heartRateMonitor.GetDeviceInfoAsync();

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

        private async Task RunOnUiThread(Action a)
        {
            await this.Dispatcher.InvokeAsync(() =>
           {
               a();
           });
        }

        private async void PickDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            //GeneralTransform ge = TopStackPanel.TransformToVisual(PickDeviceButton);
            //Point point = ge.Transform(new Point(0,0));
            //Rect rect = new Rect(point, new Point(point.X + PickDeviceButton.ActualWidth, point.Y + PickDeviceButton.ActualHeight));

            //Rect rect = new Rect(0, 0, 200, 200);
            //var device = await _heartRateMonitor.PickDevice(rect);
        }

        //private BleHeartRate HrDevice { get; set; }
        //private HeartRateMeasurementParser HrParser { get; } = new HeartRateMeasurementParser();
        //private BatteryLevelParser BatteryParser { get; } = new BatteryLevelParser();

    }
}
