using HeartRateLE.Bluetooth.Base;

namespace HeartRateLE.Bluetooth.HeartRate
{
    internal class BleBatteryServiceService : BleService
    {
        /// <summary>
        /// Battery Level characteristic.
        /// </summary>
        public BleCharacteristic BatteryLevel { get; set; } = new BleCharacteristic("Battery Level", "2A19", true);  
        
        private const bool IsServiceMandatory = true;

        public BleBatteryServiceService() : base("180F", IsServiceMandatory)
        {
        }
    }
}
