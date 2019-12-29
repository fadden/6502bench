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
using System.Diagnostics;
using System.IO;

namespace CommonUtil {
    /// <summary>
    /// Compute a standard CRC-32 (polynomial 0xedb88320, or
    /// x^32+x^26+x^23+x^22+x^16+x^12+x^11+x^10+x^8+x^7+x^5+x^4+x^2+x+1).
    /// </summary>
    public static class CRC32 {
        private static readonly uint[] sTable = ComputeTable();

        private const uint INVERT = 0xffffffff;

        /// <summary>
        /// Generates 256-entry CRC table.
        /// </summary>
        /// <returns>Table.</returns>
        private static uint[] ComputeTable() {
            uint[] table = new uint[256];

            uint poly = 0xedb88320;
            for (int i = 0; i < 256; i++) {
                uint val = (uint) i;
                for (int j = 0; j < 8; j++) {
                    val = (val & 1) != 0  ?  poly ^ (val >> 1)  :  val >> 1;
                }
                table[i] = val;
            }

            return table;
        }

        /// <summary>
        /// Computes a CRC-32 on part of a buffer of data.
        /// </summary>
        /// <param name="crc">Previously computed CRC value. Use zero as initial value.</param>
        /// <param name="buffer">Data to compute CRC on.</param>
        /// <param name="offset">Start offset within buffer.</param>
        /// <param name="count">Number of bytes to process.</param>
        /// <returns>Computed CRC value.</returns>
        public static uint OnBuffer(uint crc, byte[] buffer, int offset, int count) {
            crc = crc ^ INVERT;
            while (count-- != 0) {
                crc = sTable[(crc ^ buffer[offset]) & 0xff] ^ (crc >> 8);
                offset++;
            }
            return crc ^ INVERT;
        }

        /// <summary>
        /// Computes a CRC-32 on a buffer of data.
        /// </summary>
        /// <param name="crc">Previously computed CRC value. Initially zero.</param>
        /// <param name="buffer">Data to compute CRC on.</param>
        /// <returns>Computed CRC value.</returns>
        public static uint OnWholeBuffer(uint crc, byte[] buffer) {
            return OnBuffer(crc, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Computes a CRC-32 on an entire file.  An exception will be thrown on file errors.
        /// </summary>
        /// <param name="pathName">Full path to file to open.</param>
        /// <returns>Computed CRC value.</returns>
        public static uint OnWholeFile(string pathName) {
            using (FileStream fs = File.Open(pathName, FileMode.Open, FileAccess.Read)) {
                byte[] buffer = new byte[8192];
                uint crc = 0;
                long remain = fs.Length;
                while (remain != 0) {
                    int toRead = (remain < buffer.Length) ? (int)remain : buffer.Length;
                    int actual = fs.Read(buffer, 0, toRead);
                    if (toRead != actual) {
                        throw new IOException("Expected " + toRead + ", got " + actual);
                    }

                    crc = OnBuffer(crc, buffer, 0, toRead);
                    remain -= toRead;
                }

                return crc;
            }
        }
    }
}
