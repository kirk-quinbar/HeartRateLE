using HeartRateLE.Bluetooth.Base;

namespace HeartRateLE.Bluetooth.HeartRate
{
    internal class BleDeviceInformationService : BleService
    {
        /// <summary>
        /// Manufacturer Name String characteristic.
        /// </summary>
        public BleCharacteristic ManufacturerNameString { get; set; } = new BleCharacteristic("Manufacturer Name String", "2A29", false);  
        
        /// <summary>
        /// Model Number String characteristic.
        /// </summary>
        public BleCharacteristic ModelNumberString { get; set; } = new BleCharacteristic("Model Number String", "2A24", false);  
        
        /// <summary>
        /// Serial Number String characteristic.
        /// </summary>
        public BleCharacteristic SerialNumberString { get; set; } = new BleCharacteristic("Serial Number String", "2A25", false);  
        
        /// <summary>
        /// Hardware Revision String characteristic.
        /// </summary>
        public BleCharacteristic HardwareRevisionString { get; set; } = new BleCharacteristic("Hardware Revision String", "2A27", false);  
        
        /// <summary>
        /// Firmware Revision String characteristic.
        /// </summary>
        public BleCharacteristic FirmwareRevisionString { get; set; } = new BleCharacteristic("Firmware Revision String", "2A26", false);  
        
        /// <summary>
        /// Software Revision String characteristic.
        /// </summary>
        public BleCharacteristic SoftwareRevisionString { get; set; } = new BleCharacteristic("Software Revision String", "2A28", false);  
        
        /// <summary>
        /// System ID characteristic.
        /// </summary>
        public BleCharacteristic SystemID { get; set; } = new BleCharacteristic("System ID", "2A23", false);  
        
        /// <summary>
        /// IEEE 11073-20601 Regulatory Certification Data List characteristic.
        /// </summary>
        public BleCharacteristic IEEE1107320601RegulatoryCertificationDataList { get; set; } = new BleCharacteristic("IEEE 11073-20601 Regulatory Certification Data List", "2A2A", false);  
        
        /// <summary>
        /// PnP ID characteristic.
        /// </summary>
        public BleCharacteristic PnPID { get; set; } = new BleCharacteristic("PnP ID", "2A50", false);  
        
        private const bool IsServiceMandatory = true;

        public BleDeviceInformationService() : base("180A", IsServiceMandatory)
        {
        }
    }
}
