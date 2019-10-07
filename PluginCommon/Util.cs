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
        /// Determines whether the provided offset and length are valid for the array.
        /// </summary>
        /// <param name="data">Data array that to check against.</param>
        /// <param name="startOff">Start offset.</param>
        /// <param name="len">Number of bytes.</param>
        /// <returns>True if the specified range falls within the array bounds.</returns>
        public static bool IsInBounds(byte[] data, int startOff, int len) {
            return !(startOff < 0 || len < 0 || startOff >= data.Length || len > data.Length ||
                startOff + len > data.Length);
        }

        /// <summary>
        /// Computes a standard CRC-32 (polynomial 0xedb88320) on a buffer of data.
        /// </summary>
        /// <param name="data">Buffer to process.</param>
        /// <returns>CRC value.</returns>
        public static uint ComputeBufferCRC(byte[] data) {
            return CRC32.OnBuffer(0, data, 0, data.Length);
        }
    }
}
