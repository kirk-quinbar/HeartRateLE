using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using HeartRateLE.Bluetooth.Base;

namespace HeartRateLE.Bluetooth.Parsers
{
    internal class BatteryLevelParser : BleValueParser<byte, byte>
    {
        protected override byte ParseReadValue(IBuffer raw)
        {
            return (byte)raw.AsStream().ReadByte();
        }

        protected override IBuffer ParseWriteValue(byte data)
        {
            throw new NotImplementedException();
        }
    }
}
