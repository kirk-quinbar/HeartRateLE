using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace HeartRateLE.Bluetooth
{
    public class HeartDeviceWatcher
    {
        private static Schema.DeviceSelectorInfo BluetoothLEUnpairedOnly
        {
            get
            {
                return new Schema.DeviceSelectorInfo() { DisplayName = "Bluetooth LE (unpaired)", Selector = BluetoothLEDevice.GetDeviceSelectorFromPairingState(false) };
            }
        }

        private static Schema.DeviceSelectorInfo BluetoothLEPairedOnly
        {
            get
            {
                return new Schema.DeviceSelectorInfo() { DisplayName = "Bluetooth LE (paired)", Selector = BluetoothLEDevice.GetDeviceSelectorFromPairingState(true) };
            }
        }

        private DeviceWatcher _deviceWatcher;

        public event EventHandler<Events.DeviceAddedEventArgs> DeviceAdded;
        protected virtual void OnDeviceAdded(Events.DeviceAddedEventArgs e)
        {
            DeviceAdded?.Invoke(this, e);
        }

        public event EventHandler<Events.DeviceUpdatedEventArgs> DeviceUpdated;
        protected virtual void OnDeviceUpdated(Events.DeviceUpdatedEventArgs e)
        {
            DeviceUpdated?.Invoke(this, e);
        }

        public event EventHandler<Events.DeviceRemovedEventArgs> DeviceRemoved;
        protected virtual void OnDeviceRemoved(Events.DeviceRemovedEventArgs e)
        {
            DeviceRemoved?.Invoke(this, e);
        }

        public event EventHandler<object> DeviceEnumerationCompleted;
        protected virtual void OnDeviceEnumerationCompleted(object obj)
        {
            DeviceEnumerationCompleted?.Invoke(this, obj);
        }

        public event EventHandler<object> DeviceEnumerationStopped;
        protected virtual void OnDeviceEnumerationStopped(object obj)
        {
            DeviceEnumerationStopped?.Invoke(this, obj);
        }

        public HeartDeviceWatcher(Schema.DeviceSelector deviceSelector)
        {
            //_devices = new List<DeviceInformation>();
            _deviceWatcher = DeviceInformation.CreateWatcher(GetSelector(deviceSelector));
            _deviceWatcher.Added += Added;
            _deviceWatcher.Updated += Updated;
            _deviceWatcher.Removed += Removed;
            _deviceWatcher.EnumerationCompleted += EnumerationCompleted;
            _deviceWatcher.Stopped += Stopped;
        }

        private string GetSelector(Schema.DeviceSelector deviceSelector)
        {
            switch (deviceSelector)
            {
                case Schema.DeviceSelector.BluetoothLePairedOnly:
                    return BluetoothLEPairedOnly.Selector;
                case Schema.DeviceSelector.BluetoothLeUnpairedOnly:
                    return BluetoothLEUnpairedOnly.Selector;
                default:
                    return BluetoothLEUnpairedOnly.Selector;
            }
        }

        private void Stopped(DeviceWatcher watcher, object obj)
        {
            OnDeviceEnumerationStopped(obj);
        }

        private void EnumerationCompleted(DeviceWatcher watcher, object obj)
        {
            OnDeviceEnumerationCompleted(obj);
        }

        private void Added(DeviceWatcher watcher, DeviceInformation deviceInformation)
        {
            var args = new Events.DeviceAddedEventArgs()
            {
                Device = new Schema.WatcherDevice()
                {
                    Id = deviceInformation.Id,
                    IsDefault = deviceInformation.IsDefault,
                    IsEnabled = deviceInformation.IsEnabled,
                    Name = deviceInformation.Name,
                    IsPaired = deviceInformation.Pairing.IsPaired,
                    Kind = deviceInformation.Kind.ToString(),
                    Properties = deviceInformation.Properties.ToDictionary(pair => pair.Key, pair => pair.Value)
                }
            };

            OnDeviceAdded(args);
        }

        private void Updated(DeviceWatcher watcher, DeviceInformationUpdate deviceInformationUpdate)
        {
            var args = new Events.DeviceUpdatedEventArgs()
            {
                Device = new Schema.WatcherDevice()
                {
                    Id = deviceInformationUpdate.Id,
                    Kind = deviceInformationUpdate.Kind.ToString(),
                    Properties = deviceInformationUpdate.Properties.ToDictionary(pair => pair.Key, pair => pair.Value)
                }
            };


            OnDeviceUpdated(args);
        }

        private void Removed(DeviceWatcher watcher, DeviceInformationUpdate deviceInformationUpdate)
        {
            var args = new Events.DeviceRemovedEventArgs()
            {
                Device = new Schema.WatcherDevice()
                {
                    Id = deviceInformationUpdate.Id,
                    Kind = deviceInformationUpdate.Kind.ToString(),
                    Properties = deviceInformationUpdate.Properties.ToDictionary(pair => pair.Key, pair => pair.Value)
                }
            };

            OnDeviceRemoved(args);
        }

        public void Start()
        {
            _deviceWatcher.Start();
        }

        public void Stop()
        {
            if (_deviceWatcher.Status == DeviceWatcherStatus.Started || _deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted)
            {
                _deviceWatcher.Stop();
            }
        }
    }
}

