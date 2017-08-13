using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace HeartRateLE.Bluetooth.Base
{
    /// <summary>
    /// Base class for all Value parsers. For any concrete implementation developer needs to implement
    /// two methods: ParseReadValue and ParseWriteValue. If given characteristic doesn't support read or write
    /// operation, the implementation could be in form:
    ///     throw new NotImplementedException();
    /// In most cases both TRead and TWrite types will be the same type.
    /// </summary>
    /// <typeparam name="TRead">type of the read results</typeparam>
    /// <typeparam name="TWrite">type of the object used for write operations</typeparam>
    internal abstract class BleValueParser<TRead, TWrite>
    {
        private BleCharacteristic Characteristic { get; set; }
        
        /// <summary>
        /// Is given characteristic readable
        /// </summary>
        public bool IsReadable => Characteristic?.IsReadable ?? false;
        
        /// <summary>
        /// Is given characteristic writable
        /// </summary>
        public bool IsWritable => Characteristic?.IsWritable ?? false;
        
        /// <summary>
        /// Does connected characteristic support notification
        /// </summary>
        public bool SupportNotification => Characteristic?.SupportNotification ?? false;

        
        /// <summary>
        /// Abstract method for parsing value from raw IBuffer into output TRead type
        /// </summary>
        protected abstract TRead ParseReadValue(IBuffer raw);

        
        /// <summary>
        /// Abstract method to serialize intpu TWrite type into IBuffer ready to be set on
        /// connected characteristic
        /// </summary>
        protected abstract IBuffer ParseWriteValue(TWrite data);

        
        /// <summary>
        /// Custom event handler for notification/indication value changes
        /// </summary>
        public event EventHandler<ValueChangedEventArgs<TRead>> ValueChanged = null;

        
        /// <summary>
        /// Connects parser with particular characteristic. Characteristic can be controlled from that
        /// moment either directly or via this parser object.
        /// </summary>
        public async Task ConnectWithCharacteristicAsync(BleCharacteristic characteristic)
        {
            await DisconnectAsync();
            if (characteristic == null)
                return;

            Characteristic = characteristic;
            Characteristic.ValueChanged -= CharacteristicOnValueChanged;
            Characteristic.ValueChanged += CharacteristicOnValueChanged;
        }

        private void CharacteristicOnValueChanged(object characteristic, ValueChangedEventArgs args)
        {
            ValueChanged?.Invoke((BleCharacteristic)characteristic,  new ValueChangedEventArgs<TRead>(ParseReadValue(args.Value)));
        }

        
        /// <summary>
        /// Disconnects parser from characteristic.
        /// </summary>
        public async Task DisconnectAsync()
        {
            await DisableNotificationsAsync();
            await DisableIndicationsAsync();
            if(Characteristic != null)
                Characteristic.ValueChanged -= CharacteristicOnValueChanged;
            Characteristic = null;
        }

        
        /// <summary>
        /// Run READ operation on connected characteristic.
        /// </summary>
        public async Task<TRead> ReadAsync()
        {
            if (Characteristic == null)
                return default(TRead);
            var val = await Characteristic.ReadAsync();
            return ParseReadValue(val.Value);
        }

        /// <summary>
        /// Run WRITE operation on connected characteristic.
        /// </summary>
        public async Task<GattCommunicationStatus> WriteAsync(TWrite data)
        {
            if (Characteristic == null)
                return GattCommunicationStatus.Unreachable;
            var output = ParseWriteValue(data);
            return await Characteristic?.WriteAsync(output);
        }

        /// <summary>
        /// Enables notifications on connected characteristic.
        /// </summary>
        public async Task<GattCommunicationStatus> EnableNotificationsAsync()
        {
            if (Characteristic == null)
                return GattCommunicationStatus.Unreachable;
            return await  Characteristic.EnableNotificationsAsync();
        }
        
        /// <summary>
        /// Disables notifications on connected characteristic.
        /// </summary>
        public async Task<GattCommunicationStatus> DisableNotificationsAsync()
        {
            if (Characteristic == null)
                return GattCommunicationStatus.Unreachable;
            return await Characteristic.DisableNotificationsAsync();
        }
        
        /// <summary>
        /// Enables inidcations on connected characteristic.
        /// </summary>
        public async Task<GattCommunicationStatus> EnableIndicationsAsync()
        {
            if (Characteristic == null)
                return GattCommunicationStatus.Unreachable;
            return await Characteristic.EnableIndicationsAsync();
        }

        /// <summary>
        /// Disables inidcations on connected characteristic.
        /// </summary>
        public async Task<GattCommunicationStatus> DisableIndicationsAsync()
        {
            if (Characteristic == null)
                return GattCommunicationStatus.Unreachable;
            return await Characteristic.DisableIndicationsAsync();
        }

        /// <summary>
        /// Helper method to check if bitNumber bit is set on byte value
        /// </summary>
        protected bool IsBitSet(byte val, int bitNumber)
        {
            return (val & (1 << (8 - bitNumber))) != 0;
        }

        /// <summary>
        /// Helper method to check if bitNumber bit is set on ushort value
        /// </summary>
        protected bool IsBitSet(ushort val, int bitNumber)
        {
            return (val & (1 << (16 - bitNumber))) != 0;
        }

        /// <summary>
        /// Helper method to check if bitNumber bit is set on uint value
        /// </summary>
        protected bool IsBitSet(uint val, int bitNumber)
        {
            return (val & (1 << (32 - bitNumber))) != 0;
        }
    }

    /// <summary>
    /// Custom value changed event arg type, dedicated for particular
    /// value parser's TRead type
    /// </summary>
    internal class ValueChangedEventArgs<TRead> : EventArgs
    {
        public TRead Value { get; set; }

        public ValueChangedEventArgs(TRead val)
        {
            Value = val;
        }
    }
}
