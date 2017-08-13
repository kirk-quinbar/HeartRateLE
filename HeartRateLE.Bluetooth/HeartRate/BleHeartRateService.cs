using HeartRateLE.Bluetooth.Base;

namespace HeartRateLE.Bluetooth.HeartRate
{
    internal class BleHeartRateService : BleService
    {
        /// <summary>
        /// Heart Rate Measurement characteristic.
        /// </summary>
        public BleCharacteristic HeartRateMeasurement { get; set; } = new BleCharacteristic("Heart Rate Measurement", "2A37", true);

        /// <summary>
        /// Body Sensor Location characteristic.
        /// </summary>
        public BleCharacteristic BodySensorLocation { get; set; } = new BleCharacteristic("Body Sensor Location", "2A38", false);

        /// <summary>
        /// Heart Rate Control Point characteristic.
        /// </summary>
        public BleCharacteristic HeartRateControlPoint { get; set; } = new BleCharacteristic("Heart Rate Control Point", "2A39", false);

        private const bool IsServiceMandatory = true;

        public BleHeartRateService() : base("180D", IsServiceMandatory)
        {
        }
    }
}
