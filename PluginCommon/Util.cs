/*
 * Copyright 2018 faddenSoft
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

using CommonUtil;

namespace PluginCommon {
    /// <summary>
    /// Utility functions available for plugins to use.
    /// 
    /// The idea is to make CommonUtil functions available to plugins while isolating
    /// them from changes to the library.  Anything here is guaranteed to keep working,
    /// while other classes and functions in CommonUtil may change between releases.
    /// </summary>
    public static class Util {
        /// <summary>
        /// Extracts an integer from the data stream.
        /// </summary>
        /// <param name="data">Raw data stream.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="width">Word width, which may be 1-4 bytes.</param>
        /// <param name="isBigEndian">True if word is in big-endian order.</param>
        /// <returns>Value found.</returns>
        public static int GetWord(byte[] data, int offset, int width, bool isBigEndian) {
            return RawData.GetWord(data, offset, width, isBigEndian);
        }

        /// <summary>
        /// Compute a standard CRC-32 (polynomial 0xedb88320) on a buffer of data.
        /// </summary>
        /// <param name="data">Buffer to process.</param>
        /// <returns>CRC value.</returns>
        public static uint ComputeBufferCRC(byte[] data) {
            return CRC32.OnBuffer(0, data, 0, data.Length);
        }
    }
}
