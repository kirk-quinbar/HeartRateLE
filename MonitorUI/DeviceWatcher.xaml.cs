using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace MonitorUI
{
    /// <summary>
    /// Interaction logic for DeviceWatcher.xaml
    /// </summary>
    public partial class DeviceWatcher : Window
    {
        public ObservableCollection<Object> ResultCollection
        {
            get;
            private set;
        }

        private Wwssi.Bluetooth.Watcher _deviceWatcher;

        public DeviceWatcher()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            _deviceWatcher = new Wwssi.Bluetooth.Watcher();
            _deviceWatcher.DeviceAdded += OnDeviceAdded;
            _deviceWatcher.DeviceRemoved += OnDeviceRemoved;
            _deviceWatcher.DeviceUpdated += OnDeviceUpdated;
            _deviceWatcher.DeviceEnumerationStopped += OnDeviceEnumerationStopped;
            _deviceWatcher.DeviceEnumerationCompleted += OnDeviceEnumerationCompleted;
        }

        private void OnDeviceEnumerationCompleted(object sender, object e)
        {
            
        }

        private void OnDeviceEnumerationStopped(object sender, object e)
        {
            
        }

        private void OnDeviceUpdated(object sender, Wwssi.Bluetooth.Events.DeviceUpdatedEventArgs e)
        {
            
        }

        private void OnDeviceRemoved(object sender, Wwssi.Bluetooth.Events.DeviceRemovedEventArgs e)
        {
            
        }

        private void OnDeviceAdded(object sender, Wwssi.Bluetooth.Events.DeviceAddedEventArgs e)
        {
            
        }

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
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
            startWatcherButton.IsEnabled = false;

            _deviceWatcher.Start();

            stopWatcherButton.IsEnabled = true;
        }

        private void StopWatcher()
        {
            stopWatcherButton.IsEnabled = false;

            _deviceWatcher.Stop();

            startWatcherButton.IsEnabled = true;
        }
    }
}
