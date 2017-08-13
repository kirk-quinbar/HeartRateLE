using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using HeartRateLE.Bluetooth.Base;

namespace HeartRateLE.Bluetooth.HeartRate
{
    internal class BleHeartRate : BleDevice
    {
        private static readonly string[] RequiredServices = new string[] { "180D", "180A", "180F" };
        public BleHeartRateService HeartRate { get; set; } = new BleHeartRateService();
        public BleDeviceInformationService DeviceInformation { get; set; } = new BleDeviceInformationService();
        public BleBatteryServiceService BatteryService { get; set; } = new BleBatteryServiceService();


        /// <summary>
        /// Search and returns all Bluetooth Smart devices matching BleHeartRate profile
        /// </summary>
        /// <returns>List<BleHeartRate> list with all devices matching our device; empty list if there is no device matching</returns>
        public static async Task<List<BleHeartRate>> FindAll()
        {
            List<BleHeartRate> result = new List<BleHeartRate>();
            // get all BT LE devices
            var all = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(BluetoothLEDevice.GetDeviceSelector());
            BluetoothLEDevice leDevice = null;

            foreach (var device in all)
            {
                try
                {
                    leDevice = await BluetoothLEDevice.FromIdAsync(device.Id);
                }
                catch
                {
                    leDevice = null;
                }

                if (leDevice == null)
                    continue;

                bool matches = true;
                foreach (var requiredService in RequiredServices)
                {
                    matches = CheckForCompatibility(leDevice, requiredService.ToGuid());
                    if (!matches)
                        break;
                }

                if (!matches)
                    continue;

                var toAdd = new BleHeartRate(device, leDevice);
                toAdd.Initialize();
                result.Add(toAdd);
            }

            return result;
        }

        /// <summary>
        /// Search and returns first Bluetooth Smart device matching BleHeartRate profile
        /// </summary>
        /// <returns>first BleHeartRate device; null if there is no device matching</returns>
        public static async Task<BleHeartRate> FirstOrDefault()
        {
            var all = await FindAll();
            return all.FirstOrDefault();
        }

        public static async Task<BleHeartRate> FindByName(string deviceName)
        {
            var all = await FindAll();
            return all.FirstOrDefault(a => a.Name.Equals(deviceName, StringComparison.InvariantCultureIgnoreCase));
        }

        private BleHeartRate(DeviceInformation device, BluetoothLEDevice leDevice) : base(device, leDevice)
        {
            RegisterNewService(HeartRate);
            RegisterNewService(DeviceInformation);
            RegisterNewService(BatteryService);
        }
    }
}
