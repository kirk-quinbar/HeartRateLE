//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Windows.Storage.Streams;
//using Windows.Devices.Bluetooth.GenericAttributeProfile;

//namespace Wwssi.Bluetooth
//{
//    /// <summary>
//    /// Class encapsulating the raw IBUffer with the characteristic value,
//    /// </summary>	
//    public class ValueChangedEventArgs : EventArgs
//    {
//        /// <summary>
//        /// Original's characteristic buffer with the value.
//        /// </summary>	
//        public IBuffer Value { get; set; }

//        /// <summary>
//        /// Timestamp of the value read moment.
//        /// </summary>		
//        public DateTimeOffset Timestamp { get; set; }

//        public ValueChangedEventArgs(GattValueChangedEventArgs original)
//        {
//            Value = original.CharacteristicValue;
//            Timestamp = original.Timestamp;
//        }
//    }
//}
