﻿using HeartRateLE.Bluetooth.Events;
using HeartRateLE.Bluetooth.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;

namespace HeartRateLE.Bluetooth
{
    public class HeartRateMonitor
    {
        private BluetoothLEDevice _heartRateDevice = null;
        private List<BluetoothAttribute> _serviceCollection = new List<BluetoothAttribute>();
        private GattCharacteristic _heartRateCharacteristic;
        private GattCharacteristic _batteryCharacteristic;

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

        public async Task<Schema.HeartRateDevice> ConnectAsync(string deviceId)
        {
            _heartRateDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            if (_heartRateDevice == null)
            {
                return new Schema.HeartRateDevice()
                {
                    IsConnected = false,
                    ErrorMessage = "Could not find any heart rate device"
                };
            }

            // we should always monitor the connection status
            _heartRateDevice.ConnectionStatusChanged -= DeviceConnectionStatusChanged;
            _heartRateDevice.ConnectionStatusChanged += DeviceConnectionStatusChanged;

            await GetDeviceServicesAsync();

            CharacteristicResult characteristicResult;
            characteristicResult = await SetupHeartRateCharacteristic();
            if (!characteristicResult.IsSuccess)
                return new Schema.HeartRateDevice()
                {
                    IsConnected = false,
                    ErrorMessage = characteristicResult.Message
                };

            characteristicResult = await SetupBatteryCharacteristic();
            if (!characteristicResult.IsSuccess)
                return new Schema.HeartRateDevice()
                {
                    IsConnected = false,
                    ErrorMessage = characteristicResult.Message
                };


            // we could force propagation of event with connection status change, to run the callback for initial status
            DeviceConnectionStatusChanged(_heartRateDevice, null);

            return new Schema.HeartRateDevice()
            {
                IsConnected = _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected,
                Name = _heartRateDevice.Name
            };
        }

        private async Task<List<BluetoothAttribute>> GetServiceCharacteristicsAsync(BluetoothAttribute service)
        {
            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await service.service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                    // and the new Async functions to get the characteristics of unpaired devices as well. 
                    var result = await service.service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = result.Characteristics;
                    }
                }
                else
                {
                    // Not granted access
                    // On error, act as if there are no characteristics.
                    characteristics = new List<GattCharacteristic>();
                }
            }
            catch (Exception ex)
            {
                characteristics = new List<GattCharacteristic>();
            }

            var characteristicCollection = new List<BluetoothAttribute>();
            characteristicCollection.AddRange(characteristics.Select(a => new BluetoothAttribute(a)));
            return characteristicCollection;
        }

        private async Task<CharacteristicResult> SetupHeartRateCharacteristic()
        {
            var heartRateService = _serviceCollection.Where(a => a.Name == "HeartRate").FirstOrDefault();
            var characteristics = await GetServiceCharacteristicsAsync(heartRateService);
            _heartRateCharacteristic = characteristics.Where(a => a.Name == "HeartRateMeasurement").FirstOrDefault().characteristic;

            // Get all the child descriptors of a characteristics. Use the cache mode to specify uncached descriptors only 
            // and the new Async functions to get the descriptors of unpaired devices as well. 
            var result = await _heartRateCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                return new CharacteristicResult()
                {
                    IsSuccess = false,
                    Message = result.Status.ToString()
                };
            }

            if (_heartRateCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                var status = await _heartRateCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                if (status == GattCommunicationStatus.Success)
                    _heartRateCharacteristic.ValueChanged += HeartRateValueChanged;

                return new CharacteristicResult()
                {
                    IsSuccess = status == GattCommunicationStatus.Success,
                    Message = status.ToString()
                };
            }
            else
            {
                return new CharacteristicResult()
                {
                    IsSuccess = false,
                    Message = "HeartRateMeasurement characteristic does not support notify"
                };

            }

        }

        private async Task<CharacteristicResult> SetupBatteryCharacteristic()
        {
            var batteryService = _serviceCollection.Where(a => a.Name == "Battery").FirstOrDefault();
            var characteristics = await GetServiceCharacteristicsAsync(batteryService);
            _batteryCharacteristic = characteristics.Where(a => a.Name == "BatteryLevel").FirstOrDefault().characteristic;

            return new CharacteristicResult()
            {
                IsSuccess = true
            };
        }

        private async Task GetDeviceServicesAsync()
        {
            // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
            // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
            // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
            GattDeviceServicesResult result = await _heartRateDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

            if (result.Status == GattCommunicationStatus.Success)
            {
                _serviceCollection.AddRange(result.Services.Select(a => new BluetoothAttribute(a)));
            }
        }

        /// <summary>
        /// Disconnects the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {
            if (_heartRateDevice != null && _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                if (_heartRateCharacteristic != null)
                {
                    var result = await _heartRateCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);

                    _heartRateCharacteristic.ValueChanged -= HeartRateValueChanged;
                    _heartRateCharacteristic = null;
                }

                _batteryCharacteristic = null;
                _heartRateDevice.ConnectionStatusChanged -= DeviceConnectionStatusChanged;
                _heartRateDevice.Dispose();
                _heartRateDevice = null;
            }
        }
        private void DeviceConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            var result = new ConnectionStatusChangedEventArgs()
            {
                IsConnected = (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
            };

            OnConnectionStatusChanged(result);
        }

        private void HeartRateValueChanged(GattCharacteristic sender, GattValueChangedEventArgs e)
        {
            byte[] data;
            CryptographicBuffer.CopyToByteArray(e.CharacteristicValue, out data);

            var args = new Events.RateChangedEventArgs()
            {
                BeatsPerMinute = Utilities.ParseHeartRateValue(data)
            };
            OnRateChanged(args);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is connected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get { return _heartRateDevice != null ? _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected : false; }
        }

        /// <summary>
        /// Gets the device information for the current BLE heart rate device.
        /// </summary>
        /// <returns></returns>
        public async Task<Schema.HeartRateDeviceInfo> GetDeviceInfoAsync()
        {
            if (_heartRateDevice != null && _heartRateDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                var deviceInformationService = _serviceCollection.Where(a => a.Name == "DeviceInformation").FirstOrDefault();
                var characteristics = await GetServiceCharacteristicsAsync(deviceInformationService);

                var manufacturerNameString = await characteristics.Where(a => a.Name == "ManufacturerNameString").FirstOrDefault().characteristic.ReadValueAsync();

                //byte battery = await _batteryParser.ReadAsync();

                return new Schema.HeartRateDeviceInfo()
                {
                    DeviceId = _heartRateDevice.DeviceId,
                    Name = _heartRateDevice.Name,
                    Firmware = await Utilities.ReadCharacteristicValueAsync(characteristics, "FirmwareRevisionString"),
                    Hardware = await Utilities.ReadCharacteristicValueAsync(characteristics, "HardwareRevisionString"),
                    Manufacturer = await Utilities.ReadCharacteristicValueAsync(characteristics, "ManufacturerNameString"),
                    SerialNumber = await Utilities.ReadCharacteristicValueAsync(characteristics, "SerialNumberString"),
                    ModelNumber = await Utilities.ReadCharacteristicValueAsync(characteristics, "ModelNumberString")
                    //BatteryPercent = Convert.ToInt32(battery)
                };
            }
            else
            {
                return new Schema.HeartRateDeviceInfo();
            }
        }
    }
}