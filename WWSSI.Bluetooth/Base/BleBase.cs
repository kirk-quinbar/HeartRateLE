/******************************************************************************
The MIT License (MIT)

Copyright (c) 2016 Matchbox Mobile Limited <info@matchboxmobile.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*******************************************************************************/

// This file was generated by Bluetooth (R) Developer Studio on 2016.03.17 21:39
// with plugin Windows 10 UWP Client (version 1.0.0 released on 2016.03.16).
// Plugin developed by Matchbox Mobile Limited.

using System;

namespace Wwssi.Bluetooth.Base
{
    /// <summary>
    /// Simple Base class for all classes generated by the plugin.
    /// </summary>    
    internal class BleBase
    {
        protected BleBase() { }
    }

    /// <summary>
    /// Add simple helper method to convert any string representation of GUID
    /// into real Guid object in C#.
    /// </summary>
    internal static class GuidExtensionForString
    {
        private const int MaxLenghtOf16bitUUID = 6;
        private const string StandardPrefix = "0000";
        private const string StandardSuffix = "-0000-1000-8000-00805f9b34fb";
        private const string HexPrefix = "0x";

        /// <summary>
        /// Convert string into proper Guid object. Accepts full Guid as well as 16bit value for adopted
        /// BLE profiles.
        /// </summary>
        /// <param name="me">string representation of the Guid</param>
        /// <returns>valid Guid object or empty Guid object in case string was not valid representation of Guid</returns>
        public static Guid ToGuid(this string me)
        {
            if (me == null)
            {
                return Guid.Empty;
            }

            String uuid = me;
            if (uuid.Length <= MaxLenghtOf16bitUUID)
            {
                uuid = StandardPrefix + uuid.Replace(HexPrefix, string.Empty) + StandardSuffix;
            }

            try
            {
                return Guid.Parse(uuid);
            }
            catch (Exception)
            {
                return Guid.Empty;
            }
        }
    }
}
