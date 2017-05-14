using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Shapes;
using Wwssi.Bluetooth.Schema;

namespace MonitorUI
{
    /// <summary>
    /// Interaction logic for DeviceWatcher.xaml
    /// </summary>
    public partial class DeviceWatcher : Window
    {
        public ObservableCollection<WatcherDevice> UnpairedCollection
        {
            get;
            private set;
        }

        public ObservableCollection<WatcherDevice> PairedCollection
        {
            get;
            private set;
        }

        private Wwssi.Bluetooth.Watcher _deviceWatcher;

        public DeviceWatcher()
        {
            InitializeComponent();

            UnpairedCollection = new ObservableCollection<WatcherDevice>();
            PairedCollection = new ObservableCollection<WatcherDevice>();
            this.DataContext = this;

            _deviceWatcher = new Wwssi.Bluetooth.Watcher();
            _deviceWatcher.DeviceAdded += OnDeviceAdded;
            _deviceWatcher.DeviceRemoved += OnDeviceRemoved;
            _deviceWatcher.DeviceUpdated += OnDeviceUpdated;
            _deviceWatcher.DeviceEnumerationStopped += OnDeviceEnumerationStopped;
            _deviceWatcher.DeviceEnumerationCompleted += OnDeviceEnumerationCompleted;
            StartWatcher();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
        }

        private async void OnDeviceEnumerationCompleted(object sender, object e)
        {
            Debug.WriteLine("Device Enumeration Completed");
        }

        private async void OnDeviceEnumerationStopped(object sender, object e)
        {
            Debug.WriteLine("Device Enumeration Stopped");
        }

        private async void OnDeviceUpdated(object sender, Wwssi.Bluetooth.Events.DeviceUpdatedEventArgs e)
        {
            Debug.WriteLine("Device Updated");
        }

        private async void OnDeviceRemoved(object sender, Wwssi.Bluetooth.Events.DeviceRemovedEventArgs e)
        {
            await RunOnUiThread(() =>
            {
                var foundItem = UnpairedCollection.FirstOrDefault(a => a.Id == e.Device.Id);
                if (foundItem != null)
                    UnpairedCollection.Remove(foundItem);
                Debug.WriteLine("Device Removed: " + e.Device.Id);
            });
        }

        private async void OnDeviceAdded(object sender, Wwssi.Bluetooth.Events.DeviceAddedEventArgs e)
        {
            await RunOnUiThread(() =>
            {
                UnpairedCollection.Add(e.Device);
                Debug.WriteLine("Device Added: " + e.Device.Id);
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            StopWatcher();
        }

        private void StartWatcherButton_Click(object sender, RoutedEventArgs e)
        {
            StartWatcher();
        }

        private void StopWatcherButton_Click(object sender, RoutedEventArgs e)
        {
            StopWatcher();
        }

        private void StartWatcher()
        {
            //startWatcherButton.IsEnabled = false;

            _deviceWatcher.Start();

            //stopWatcherButton.IsEnabled = true;
        }

        private void StopWatcher()
        {
            //stopWatcherButton.IsEnabled = false;

            _deviceWatcher.Stop();

            //startWatcherButton.IsEnabled = true;
        }

        private async Task RunOnUiThread(Action a)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                a();
            });
        }

        private async void PairDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = (WatcherDevice)unpairedListView.SelectedItem;
            if (selectedItem != null)
                await _deviceWatcher.PairDevice(selectedItem.Id);

            this.Close();
        }
    }
}
