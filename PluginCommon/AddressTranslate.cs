/*
 * Copyright 2019 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Runtime.Serialization;
using CommonUtil;

namespace PluginCommon {
    /// <summary>
    /// Read-only wrapper around AddressMap.
    /// 
    /// Instance is immutable, though in theory the underlying AddressMap could change if
    /// some other code has a reference to it.
    /// </summary>
    /// <remarks>
    /// This is currently simple enough that it could just be an interface, but I don't want
    /// to rely on that remaining true.
    ///
    /// TODO(maybe): add a "IsAddressRangeValid(int srcOffset, int addr, int length)" method
    /// that verifies an entire address range is in memory.  This would allow subsequent access
    /// to skip error checks.  (You still have to do the address-to-offset translation on every
    /// byte though, which is where most of the expense is.)
    /// TODO(maybe): add a "CopyAddressRange(byte[] data, int srcOffset, int addr, int length)"
    /// that returns a newly-allocated buffer with the data copied out.  This would allow fast
    /// access to data that is split into multiple regions.
    /// </remarks>
    public class AddressTranslate {
        private AddressMap mAddrMap;

        public AddressTranslate(AddressMap addrMap) {
            mAddrMap = addrMap;
        }

        /// <summary>
        /// Converts a file offset to an address.
        /// </summary>
        /// <param name="offset">File offset.</param>
        /// <returns>24-bit address.</returns>
        public int OffsetToAddress(int offset) {
            return mAddrMap.OffsetToAddress(offset);
        }

        /// <summary>
        /// Determines the file offset that best contains the specified target address.
        /// </summary>
        /// <param name="srcOffset">Offset of the address reference.  Only matters when
        ///   multiple file offsets map to the same address.</param>
        /// <param name="targetAddr">Address to look up.</param>
        /// <returns>The file offset, or -1 if the address falls outside the file.</returns>
        public int AddressToOffset(int srcOffset, int targetAddr) {
            return mAddrMap.AddressToOffset(srcOffset, targetAddr);
        }

        /// <summary>
        /// Returns the data found at the specified address.  If the address is out
        /// of bounds this throws an AddressException.
        /// </summary>
        /// <param name="data">Data array.</param>
        /// <param name="srcOffset">Offset of the address reference.  Only matters when
        ///   multiple file offsets map to the same address.</param>
        /// <param name="address">Data address.</param>
        /// <returns>Data found.</returns>
        public byte GetDataAtAddress(byte[] data, int srcOffset, int address) {
            int offset = AddressToOffset(srcOffset, address);
            if (offset == -1) {
                Exception ex = new AddressTranslateException("Address $" + address.ToString("X4") +
                    " is outside the file bounds");
                ex.Data.Add("Address", address);
                throw ex;
            }
            try {
                byte foo = data[offset];
            } catch (Exception) {
                throw new AddressTranslateException("FAILED at srcOff=$" + srcOffset.ToString("x4") +
                    " addr=$" + address.ToString("x4"));
            }
            return data[offset];
        }
    }

    /// <summary>
    /// Exception thrown by AddressTranslate's GetDataAtAddress().
    /// </summary>
    [Serializable]
    public class AddressTranslateException : Exception {
        public AddressTranslateException() : base() { }
        public AddressTranslateException(string msg) : base(msg) { }

        protected AddressTranslateException(SerializationInfo info, StreamingContext context) :
                base(info, context) {
            // base class handles everything
        }
    }
}
