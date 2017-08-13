
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace HeartRateLE.Bluetooth.Base
{
    internal class BleCharacteristic : BleBase
    {
        #region Characteristic's properties
        
        /// <summary>
        /// Original, backend GattCharacteristic object.
        /// </summary>
        public GattCharacteristic Characteristic { get; private set; }
        
        /// <summary>
        /// Original GattCharacteristic's properties object of type GattCharacteristicProperties.
        /// </summary>        
        public GattCharacteristicProperties CharacteristicProperties { get; private set; } = GattCharacteristicProperties.None;

        /// <summary>
        /// The name of the characteristic (e.g. "Heart Rate Measurement").
        /// </summary> 
        public string Name { get; private set; }
        
        /// <summary>
        /// Tells if given characteristic is mandatory by the BT LE device profile.
        /// </summary>
        public bool IsMandatory { get; private set; }
        
        /// <summary>
        /// Guid of the characteristic.
        /// </summary>
        public Guid UUID { get; private set; } = Guid.Empty;
        
        /// <summary>
        /// Tells if given characteristic is available for current device. For not-mandatory characteristics it could be false.
        /// </summary>
        public bool IsAvailable => this.Characteristic != null;
        
        /// <summary>
        /// Gets user description of the characteristic or null if it is not available.
        /// </summary>
        public string Description => this.Characteristic?.UserDescription;
        
        /// <summary>
        /// Tells if characteristics could be read.
        /// </summary>
        public bool IsReadable => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.Read);
        
        /// <summary>
        /// Tells if we can put something (write operation) into the characteristic.
        /// </summary>
        public bool IsWritable => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.Write);
        
        /// <summary>
        /// Tells if we can put something (write without response operation) into the characteristic.
        /// </summary>
        public bool IsWritableWithoutResponse => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.WriteWithoutResponse);
        
        /// <summary>
        /// Tells if characteristic supports broadcasting.
        /// </summary>
        public bool SupportBroadcast => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.Broadcast);
        
        /// <summary>
        /// Tells if characteristic supports notifications.
        /// </summary>
        public bool SupportNotification => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.Notify);
        
        /// <summary>
        /// Tells if characteristic supports indications.
        /// </summary>
        public bool SupportIndication => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.Indicate);
        
        /// <summary>
        /// Tells if we can put something (signed write operation) into the characteristic.
        /// </summary>
        public bool SupportSignedWrites => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.AuthenticatedSignedWrites);
        
        /// <summary>
        /// Tells if the characteristic contain any extended properties.
        /// </summary>
        public bool HasExtendedProperties => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.ExtendedProperties);
        
        /// <summary>
        /// Tells if characteristic supports reliable writes operation.
        /// </summary>
        public bool SupportReliableWrites => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.ReliableWrites);
        
        /// <summary>
        /// Tells if characteristic has any writable auxiliaries.
        /// </summary>
        public bool HasWritableAuxiliaries => 0 != (this.CharacteristicProperties & GattCharacteristicProperties.WritableAuxiliaries);

        #endregion
        
        public BleCharacteristic(string name, string uuid, bool mandatory) : base()
        {
            Name = name;
            UUID = uuid.ToGuid();
            IsMandatory = mandatory;
        }

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// Initializes the whole structure. This is called indirectly during the construction of a BleDevice so would not normally be called.
        /// </summary>
        /// <param name="parentService">parent BleService object</param>
        public void Initialize(BleService parentService)
        {
            if (parentService.DeviceService != null)
            {
                Characteristic = parentService.DeviceService.GetCharacteristics(UUID).FirstOrDefault();
                if (Characteristic == null)
                    return;

                CharacteristicProperties = Characteristic.CharacteristicProperties;
                if (SupportNotification || SupportIndication)
                    Characteristic.ValueChanged += ValueChangedHandler;
            }
        }

        /// <summary>
        /// Cleans up after a characteristic, disabling all notifications, indications and resets its internal values.
        /// The object will be unusable after calling this method unless Initialise is called again.
        /// </summary>
        /// <returns>await able task</returns>
        public async Task CloseAsync()
        {
            await DisableNotificationsAsync();
            await DisableIndicationsAsync();
            CharacteristicProperties = GattCharacteristicProperties.None;
            Characteristic = null;
        }

        private void ValueChangedHandler(GattCharacteristic ch, GattValueChangedEventArgs args)
        {
            var valueArg = new ValueChangedEventArgs(args);
            ValueChanged?.Invoke(this, valueArg);
        }

        /// <summary>
        /// Run READ operation if characteristic is available and readable.
        /// </summary>
        /// <returns>GattReadResult object with raw bytes of the characteristic's value</returns>
        public async Task<GattReadResult> ReadAsync()
        {
            if (!IsAvailable || !IsReadable)
                return null;
            return await Characteristic.ReadValueAsync();
        }

        /// <summary>
        /// Run WRITE operation if characteristic is available and writable.
        /// </summary>
        /// <returns>GattCommunicationStatus object communicating if the operation finished with success</returns>
        public async Task<GattCommunicationStatus> WriteAsync(IBuffer buffer)
        {
            if (!IsAvailable || !IsWritable)
                return GattCommunicationStatus.Unreachable;
            return await Characteristic.WriteValueAsync(buffer);
        }

        /// <summary>
        /// Enables Notification if characteristic is available and support notifications.
        /// </summary>
        /// <returns>GattCommunicationStatus object communicating if the operation finished with success</returns>
        public async Task<GattCommunicationStatus> EnableNotificationsAsync()
        {
            if (!IsAvailable || !SupportNotification)
                return GattCommunicationStatus.Unreachable;
            if (await IsNotifyingAsync())
                return GattCommunicationStatus.Success;
            return await Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
        }

        /// <summary>
        /// Disables Notification if characteristic is available and support notifications.
        /// </summary>
        /// <returns>GattCommunicationStatus object communicating if the operation finished with success</returns>
        public async Task<GattCommunicationStatus> DisableNotificationsAsync()
        {
            if (!IsAvailable || !SupportNotification)
                return GattCommunicationStatus.Unreachable;
            return await Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
        }

        /// <summary>
        /// Checks if given characteristic is notifying at this moment.
        /// </summary>
        /// <returns>true if characteristic has notifications enabled at the moment.</returns>
        public async Task<bool> IsNotifyingAsync()
        {
            var result = await Characteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
            if (result.Status != GattCommunicationStatus.Success)
                return false;
            return 0 !=
                   (result.ClientCharacteristicConfigurationDescriptor &
                    GattClientCharacteristicConfigurationDescriptorValue.Notify);
        }

        /// <summary>
        /// Enables Indications if characteristic is available and support indications.
        /// </summary>
        /// <returns>GattCommunicationStatus object communicating if the operation finished with success</returns>
        public async Task<GattCommunicationStatus> EnableIndicationsAsync()
        {
            if (!IsAvailable || !SupportIndication)
                return GattCommunicationStatus.Unreachable;
            if (await IsIndicatingAsync())
                return GattCommunicationStatus.Success;
            return await Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
        }

        /// <summary>
        /// Disables Indications if characteristic is available and support indications.
        /// </summary>
        /// <returns>GattCommunicationStatus object communicating if the operation finished with success</returns>
        public async Task<GattCommunicationStatus> DisableIndicationsAsync()
        {
            if (!IsAvailable || !SupportIndication)
                return GattCommunicationStatus.Unreachable;
            return await Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
        }

        /// <summary>
        /// Checks if given characteristic is indicating at this moment.
        /// </summary>
        /// <returns>true if characteristic has indications enabled at the moment.</returns>
        public async Task<bool> IsIndicatingAsync()
        {
            if (!IsAvailable || !SupportIndication)
                return false;
            var result = await Characteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
            if (result.Status != GattCommunicationStatus.Success)
                return false;
            return 0 !=
                   (result.ClientCharacteristicConfigurationDescriptor &
                    GattClientCharacteristicConfigurationDescriptorValue.Indicate);
        }
    }

    /// <summary>
    /// Class encapsulating the raw IBUffer with the characteristic value,
    /// </summary>	
    internal class ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Original's characteristic buffer with the value.
        /// </summary>	
        public IBuffer Value { get; set; }

        /// <summary>
        /// Timestamp of the value read moment.
        /// </summary>		
        public DateTimeOffset Timestamp { get; set; }

        public ValueChangedEventArgs(GattValueChangedEventArgs original)
        {
            Value = original.CharacteristicValue;
            Timestamp = original.Timestamp;
        }
    }
}
