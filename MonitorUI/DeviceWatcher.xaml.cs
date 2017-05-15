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

        private Wwssi.Bluetooth.Watcher _unpairedWatcher;

        public DeviceWatcher()
        {
            InitializeComponent();

            UnpairedCollection = new ObservableCollection<WatcherDevice>();
            PairedCollection = new ObservableCollection<WatcherDevice>();
            this.DataContext = this;

            _unpairedWatcher = new Wwssi.Bluetooth.Watcher(DeviceSelector.BluetoothLeUnpairedOnly);
            _unpairedWatcher.DeviceAdded += OnDeviceAdded;
            _unpairedWatcher.DeviceRemoved += OnDeviceRemoved;
            _unpairedWatcher.DeviceUpdated += OnDeviceUpdated;
            _unpairedWatcher.DeviceEnumerationStopped += OnDeviceEnumerationStopped;
            _unpairedWatcher.DeviceEnumerationCompleted += OnDeviceEnumerationCompleted;
            _unpairedWatcher.Start();
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
            _unpairedWatcher.Stop();
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
            {
                var result = await _unpairedWatcher.PairDevice(selectedItem.Id);
                MessageBox.Show(result.Status);
            }

            this.Close();
        }
    }
}
