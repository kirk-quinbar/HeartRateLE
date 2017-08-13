using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using HeartRateLE.Bluetooth.Base;

namespace HeartRateLE.Bluetooth.Parsers
{
    internal class StringParser : BleValueParser<String, String>
    {
        private static readonly StringParser GlobalInstanceUtf8 = new StringParser().SetStringFormat(GattPresentationFormatTypes.Utf8);
        private static readonly StringParser GlobalInstanceUtf16 = new StringParser().SetStringFormat(GattPresentationFormatTypes.Utf16);

        private byte StringFormat { get; set; } = GattPresentationFormatTypes.Utf8;

        protected override string ParseReadValue(IBuffer raw)
        {
            if (StringFormat == GattPresentationFormatTypes.Utf8)
            {
                return Encoding.UTF8.GetString(raw.ToArray());
            }
            else
            {
                return Encoding.Unicode.GetString(raw.ToArray());
            }
        }

        protected override IBuffer ParseWriteValue(string data)
        {
            if (StringFormat == GattPresentationFormatTypes.Utf8)
            {
                return Encoding.UTF8.GetBytes(data).AsBuffer();
            }
            else
            {
                return Encoding.Unicode.GetBytes(data).AsBuffer();
            }
        }

        public static string Convert(IBuffer input, byte format)
        {
            if (format == GattPresentationFormatTypes.Utf16)
            {
                return GlobalInstanceUtf16.ParseReadValue(input);
            }
            else
            {
                return GlobalInstanceUtf8.ParseReadValue(input);
            }
        }

        public static IBuffer Convert(string input, byte format)
        {
            if (format == GattPresentationFormatTypes.Utf16)
            {
                return GlobalInstanceUtf16.ParseWriteValue(input);
            }
            else
            {
                return GlobalInstanceUtf8.ParseWriteValue(input);
            }
        }

        public StringParser SetStringFormat(byte format)
        {
            if (format != GattPresentationFormatTypes.Utf8 && format != GattPresentationFormatTypes.Utf16)
                throw new ArgumentOutOfRangeException("format");

            StringFormat = format;

            return this;
        }
    }

    internal static class BleCharacteristicString
    {
        public static async Task<string> ReadAsStringAsync(this BleCharacteristic me)
        {
            var readStatus = await me.ReadAsync();

            if (readStatus.Status == GattCommunicationStatus.Unreachable)
                return null;

            return StringParser.Convert(readStatus.Value, GattPresentationFormatTypes.Utf8);
        }

        public static async Task<string> ReadAsUnicodeStringAsync(this BleCharacteristic me)
        {
            var readStatus = await me.ReadAsync();

            if (readStatus.Status == GattCommunicationStatus.Unreachable)
                return null;

            return StringParser.Convert(readStatus.Value, GattPresentationFormatTypes.Utf16);
        }

        public static async Task<GattCommunicationStatus> WriteAsStringAsync(this BleCharacteristic me, string input)
        {
            var buffer = StringParser.Convert(input, GattPresentationFormatTypes.Utf8);
            return await me.WriteAsync(buffer);
        }

        public static async Task<GattCommunicationStatus> WriteAsUnicodeStringAsync(this BleCharacteristic me, string input)
        {
            var buffer = StringParser.Convert(input, GattPresentationFormatTypes.Utf16);
            return await me.WriteAsync(buffer);
        }
    }
}
