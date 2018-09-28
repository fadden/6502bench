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

namespace CommonUtil {
    public class RawData {
        /// <summary>
        /// Extracts an integer from the data stream.
        /// </summary>
        /// <param name="data">Raw data stream.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="width">Word width, which may be 1-4 bytes.</param>
        /// <param name="isBigEndian">True if word is in big-endian order.</param>
        /// <returns>Value found.</returns>
        public static int GetWord(byte[] data, int offset, int width, bool isBigEndian) {
            if (width < 1 || width > 4 || offset + width > data.Length) {
                throw new ArgumentOutOfRangeException("GetWord(offset=" + offset + " width=" +
                    width + "), data.Length=" + data.Length);
            }
            if (isBigEndian) {
                switch (width) {
                    case 1:
                        return data[offset];
                    case 2:
                        return (data[offset] << 8) | data[offset + 1];
                    case 3:
                        return (data[offset] << 16) | (data[offset + 1] << 8) | data[offset + 2];
                    case 4:
                        return (data[offset] << 24) | (data[offset + 1] << 16) |
                            (data[offset + 2] << 8) | data[offset + 3];
                }
            } else {
                switch (width) {
                    case 1:
                        return data[offset];
                    case 2:
                        return data[offset] | (data[offset + 1] << 8);
                    case 3:
                        return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);
                    case 4:
                        return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) |
                            data[offset + 3] << 24;
                }
            }

            throw new Exception("GetWord(): should not be here");
        }
    }
}
