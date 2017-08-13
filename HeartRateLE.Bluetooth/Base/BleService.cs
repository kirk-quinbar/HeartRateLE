using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Media.Devices;

namespace HeartRateLE.Bluetooth.Base
{ 
    /// <summary>
    /// Base class for all services included in the Bluetooth Smart device.
    /// </summary>
    internal class BleService : BleBase
    {
        /// <summary>
        /// Tells if given service is mandatory for the device.
        /// </summary>
        public bool IsMandatory { get; private set; }
        
        /// <summary>
        /// Gives the GUID of the service.
        /// </summary>
        public Guid UUID { get; private set; }

        private List<BleCharacteristic> AllCharacteristics { get; } = new List<BleCharacteristic>();
        
        /// <summary>
        /// Returns underlying GattDeviceService object (from standard Windows 10 SDK)
        /// </summary>
        public GattDeviceService DeviceService { get; private set; }
        
        /// <summary>
        /// Tells if given service is available (it will be false if the device doesn't implement
        /// that service)
        /// </summary>
        public bool IsAvailable => DeviceService != null;

        
        /// <summary>
        /// Initializes whole service object. Needs to be called before any use. It is called
        /// automatically by parent BleDevice object while its initialization.
        /// </summary>
        public void Initialize(BleDevice device)
        {
            try
            {
                DeviceService = device.LEDevice.GetGattService(UUID);
            }
            catch 
            {
                DeviceService = null;
            }

            if (!IsAvailable)
                return;

            var props = this.GetType().GetProperties();
            foreach (var property in props)
            {
                if (!property.PropertyType.IsAssignableFrom(typeof(BleCharacteristic)))
                    continue;

                BleCharacteristic characteristic = (BleCharacteristic) property.GetValue(this);
                if (characteristic != null)
                {
                    AllCharacteristics.Add(characteristic);
                    characteristic.Initialize(this);
                }
            }
        }

        
        /// <summary>
        /// Disposing and cleaning after service. It is automatically called by parent's BleDevice
        /// Close() function.
        /// </summary>
        public async Task CloseAsync()
        {
            foreach (var ch in AllCharacteristics)
            {
                await ch.CloseAsync();
            }

            AllCharacteristics.Clear();
            DeviceService?.Dispose();
            DeviceService = null;
        }

        protected BleService(string id, bool mandatory)
        {
            UUID = id.ToGuid();
            IsMandatory = mandatory;
        }
    }
}
