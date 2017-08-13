using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using HeartRateLE.Bluetooth.Base;

namespace HeartRateLE.Bluetooth.Parsers
{
    internal class HeartRateMeasurementParser : BleValueParser<short, short>
    {
        /// <summary>
        /// Parsing input bytes according to official Bluetooth specification:
        /// https://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicViewer.aspx?u=org.bluetooth.characteristic.heart_rate_measurement.xml
        /// </summary>
        /// <param name="raw">input buffer with raw bytes of the value</param>
        /// <returns></returns>
        protected override short ParseReadValue(IBuffer raw)
        {
            if (raw == null || raw.Length == 0)
                return -1;

            var reader = new BinaryReader(raw.AsStream());
            short value = 0;
            byte flag = reader.ReadByte();

            if (IsBitSet(flag, 0))
            {
                // UINT16 format
                reader.ReadByte(); // omit this, as it is not used in 16 bit format
                value = (short)reader.ReadUInt16();
            }
            else
            {
                // UINT8 format
                value = (short)reader.ReadByte();
            }

            return value;
        }

        protected override IBuffer ParseWriteValue(short data)
        {
            throw new NotImplementedException();
        }
    }
}
