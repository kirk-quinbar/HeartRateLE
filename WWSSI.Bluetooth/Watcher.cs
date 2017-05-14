using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace Wwssi.Bluetooth
{
    public class Watcher
    {
        private static Schema.DeviceSelectorInfo BluetoothLE
        {
            get
            {
                // Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.  Typically you wouldn't need this for common scenarios, but it's convenient to demonstrate the
                // various sample scenarios. 
                return new Schema.DeviceSelectorInfo() { DisplayName = "Bluetooth LE", Selector = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"", Kind = DeviceInformationKind.AssociationEndpoint };
            }
        }

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

        protected List<DeviceInformation> _devices;

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

        public Watcher()
        {
            _devices = new List<DeviceInformation>();
            _deviceWatcher = DeviceInformation.CreateWatcher(BluetoothLEUnpairedOnly.Selector);
            _deviceWatcher.Added += Added;
            _deviceWatcher.Updated += Updated;
            _deviceWatcher.Removed += Removed;
            _deviceWatcher.EnumerationCompleted += EnumerationCompleted;
            _deviceWatcher.Stopped += Stopped;
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
            _devices.Add(deviceInformation);

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

            var foundItem = _devices.FirstOrDefault(a => a.Id == deviceInformationUpdate.Id);
            if (foundItem != null)
                _devices.Remove(foundItem);

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

        public async Task<Schema.PairingResult> PairDevice(string Id)
        {
            var foundItem = _devices.FirstOrDefault(a => a.Id == Id);
            if (foundItem != null)
            {
                foundItem.Pairing.Custom.PairingRequested += Custom_PairingRequested;
                var result = await foundItem.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmOnly);

                return new Schema.PairingResult()
                {
                    Status = result.Status.ToString()
                };
            }
            else
            {
                return new Schema.PairingResult()
                {
                    Status = string.Format("Device Id:{0} not found", Id)
                };
            }
        }

        private void Custom_PairingRequested(DeviceInformationCustomPairing sender, DevicePairingRequestedEventArgs args)
        {
            args.Accept();
            //throw new NotImplementedException();
        }

        public async Task<Schema.PairingResult> UnpairDevice(string Id)
        {
            var foundItem = _devices.FirstOrDefault(a => a.Id == Id);
            if (foundItem != null)
            {
                var result = await foundItem.Pairing.UnpairAsync();
                return new Schema.PairingResult()
                {
                    Status = result.Status.ToString()
                };
            }
            else
            {
                return new Schema.PairingResult()
                {
                    Status = string.Format("Device Id:{0} not found", Id)
                };
            }
        }
    }
}

