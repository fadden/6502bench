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
using System.Text;

namespace CommonUtil {
    /// <summary>
    /// Utility functions for manipulating streams of primitive data types.
    /// </summary>
    public static class RawData {
        /// <summary>
        /// Zero-length array, useful for initializing properties to a non-null value without
        /// allocating anything.
        /// </summary>
        public static readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];
        public static readonly ushort[] EMPTY_USHORT_ARRAY = new ushort[0];
        public static readonly int[] EMPTY_INT_ARRAY = new int[0];

        /// <summary>
        /// Extracts a null-terminated ASCII string from byte data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="offset">Start offset in data buffer.</param>
        /// <param name="maxLen">Maximum length of string.</param>
        /// <param name="andMask">AND mask to apply to bytes.</param>
        /// <returns>String found.  May be empty.</returns>
        public static string GetNullTermString(byte[] data, int offset, int maxLen, byte andMask) {
            Debug.Assert(offset + maxLen <= data.Length);
            StringBuilder sb = new StringBuilder(maxLen);
            for (int i = 0; i < maxLen; i++) {
                byte val = data[offset + i];
                if (val == 0) {
                    break;
                }
                sb.Append((char)(val & andMask));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets an integer from the data stream.  Integers less than 4 bytes wide
        /// are not sign-extended.
        /// </summary>
        /// <param name="data">Raw data stream.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="width">Word width in bytes (1-4).</param>
        /// <param name="isBigEndian">True if word is in big-endian order.</param>
        /// <returns>Value read.</returns>
        public static int GetWord(byte[] data, int offset, int width, bool isBigEndian) {
            return (int)GetUWord(data, offset, width, isBigEndian);
        }

        /// <summary>
        /// Gets an unsigned integer from the data stream.
        /// </summary>
        /// <param name="data">Raw data stream.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="width">Word width in bytes (1-4).</param>
        /// <param name="isBigEndian">True if word is in big-endian order.</param>
        /// <returns>Value read.</returns>
        public static uint GetUWord(byte[] data, int offset, int width, bool isBigEndian) {
            if (width < 1 || width > 4 || offset > data.Length - width) {
                throw new ArgumentOutOfRangeException(nameof(offset),
                    "GetUWord(offset=" + offset + " width=" + width +
                    "), data.Length=" + data.Length);
            }
            if (isBigEndian) {
                switch (width) {
                    case 1:
                        return data[offset];
                    case 2:
                        return (uint)((data[offset] << 8) | data[offset + 1]);
                    case 3:
                        return (uint)((data[offset] << 16) | (data[offset + 1] << 8) |
                            data[offset + 2]);
                    case 4:
                        return (uint)((data[offset] << 24) | (data[offset + 1] << 16) |
                            (data[offset + 2] << 8) | data[offset + 3]);
                }
            } else {
                switch (width) {
                    case 1:
                        return data[offset];
                    case 2:
                        return (uint)(data[offset] | (data[offset + 1] << 8));
                    case 3:
                        return (uint)(data[offset] | (data[offset + 1] << 8) |
                            (data[offset + 2] << 16));
                    case 4:
                        return (uint)(data[offset] | (data[offset + 1] << 8) |
                            (data[offset + 2] << 16) | data[offset + 3] << 24);
                }
            }
            throw new Exception("GetUWord should not be here");
        }

        /// <summary>
        /// Reads an unsigned 8-bit value from a byte stream, advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 1.</param>
        /// <returns>Value retrieved.</returns>
        public static byte ReadU8(byte[] data, ref int offset) {
            return data[offset++];
        }

        /// <summary>
        /// Writes an unsigned 8-bit value to a byte stream, advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 1.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU8(byte[] data, ref int offset, byte val) {
            data[offset++] = val;
        }

        #region Little-Endian

        /// <summary>
        /// Reads a little-endian value from a file stream.
        /// </summary>
        private static int ReadLE(Stream stream, int width, out bool ok) {
            int result = 0;
            int okCheck = 0;
            for (int i = 0; i < width; i++) {
                int val = stream.ReadByte();        // returns -1 on EOF
                okCheck |= val;                     // sign bit set after bad read
                result |= val << (i * 8);
            }
            ok = (okCheck >= 0);
            return result;
        }

        /// <summary>
        /// Writes a little-endian value to a file stream.
        /// </summary>
        private static void WriteLE(Stream stream, int width, int value) {
            for (int i = 0; i < width; i++) {
                stream.WriteByte((byte)value);
                value >>= 8;
            }
        }

        /// <summary>
        /// Gets an unsigned 16-bit little-endian value from a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <returns>Value retrieved.</returns>
        public static ushort GetU16LE(byte[] data, int offset) {
            return (ushort)(data[offset] | (data[offset + 1] << 8));
        }

        /// <summary>
        /// Sets an unsigned 16-bit little-endian value in a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <param name="val">Value to write.</param>
        public static void SetU16LE(byte[] data, int offset, ushort val) {
            data[offset] = (byte)val;
            data[offset + 1] = (byte)(val >> 8);
        }

        /// <summary>
        /// Reads an unsigned 16-bit little-endian value from a byte stream,
        /// advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 2.</param>
        /// <returns>Value retrieved.</returns>
        public static ushort ReadU16LE(byte[] data, ref int offset) {
            ushort val = (ushort)(data[offset] | (data[offset + 1] << 8));
            offset += 2;
            return val;
        }

        /// <summary>
        /// Reads an unsigned 16-bit little-endian value from a file stream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="ok">Result: true if file read succeeded.</param>
        /// <returns>Value retrieved.</returns>
        public static ushort ReadU16LE(Stream stream, out bool ok) {
            return (ushort)ReadLE(stream, sizeof(ushort), out ok);
        }

        /// <summary>
        /// Writes an unsigned 16-bit little-endian value to a byte stream, advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 2.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU16LE(byte[] data, ref int offset, ushort val) {
            data[offset++] = (byte)val;
            data[offset++] = (byte)(val >> 8);
        }

        /// <summary>
        /// Writes an unsigned 16-bit little-endian value to a file stream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU16LE(Stream stream, ushort val) {
            stream.WriteByte((byte)val);
            stream.WriteByte((byte)(val >> 8));
        }

        /// <summary>
        /// Gets an unsigned 24-bit little-endian value from a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <returns>Value retrieved.</returns>
        public static uint GetU24LE(byte[] data, int offset) {
            uint val = (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16));
            return val;
        }

        /// <summary>
        /// Writes an unsigned 24-bit little-endian value to a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <param name="val">Value to write</param>
        public static void SetU24LE(byte[] data, int offset, uint val) {
            data[offset] = (byte)val;
            data[offset + 1] = (byte)(val >> 8);
            data[offset + 2] = (byte)(val >> 16);
        }

        /// <summary>
        /// Reads an unsigned 24-bit little-endian value from a byte stream,
        /// advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 3.</param>
        /// <returns>Value retrieved.</returns>
        public static uint ReadU24LE(byte[] data, ref int offset) {
            uint val = (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16));
            offset += 3;
            return val;
        }

        /// <summary>
        /// Writes an unsigned 24-bit little-endian value to a byte stream, advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 3.</param>
        /// <param name="val">Value to write</param>
        public static void WriteU24LE(byte[] data, ref int offset, uint val) {
            data[offset++] = (byte)val;
            data[offset++] = (byte)(val >> 8);
            data[offset++] = (byte)(val >> 16);
        }

        /// <summary>
        /// Gets an unsigned 32-bit little-endian value from a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <returns>Value retrieved.</returns>
        public static uint GetU32LE(byte[] data, int offset) {
            return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) |
                (data[offset + 3] << 24));
        }

        /// <summary>
        /// Sets an unsigned 32-bit little-endian value in a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <param name="val">Value to write.</param>
        public static void SetU32LE(byte[] data, int offset, uint val) {
            data[offset] = (byte)val;
            data[offset + 1] = (byte)(val >> 8);
            data[offset + 2] = (byte)(val >> 16);
            data[offset + 3] = (byte)(val >> 24);
        }

        /// <summary>
        /// Reads an unsigned 32-bit little-endian value from a byte stream,
        /// advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 4.</param>
        /// <returns>Value retrieved.</returns>
        public static uint ReadU32LE(byte[] data, ref int offset) {
            uint val = (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) |
                (data[offset + 3] << 24));
            offset += 4;
            return val;
        }

        /// <summary>
        /// Reads an unsigned 32-bit little-endian value from a file stream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="ok">Result: true if file read succeeded.</param>
        /// <returns>Value retrieved.</returns>
        public static uint ReadU32LE(Stream stream, out bool ok) {
            return (uint)ReadLE(stream, sizeof(uint), out ok);
        }

        /// <summary>
        /// Writes an unsigned 32-bit little-endian value to a byte stream, advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 4.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU32LE(byte[] data, ref int offset, uint val) {
            data[offset++] = (byte)val;
            data[offset++] = (byte)(val >> 8);
            data[offset++] = (byte)(val >> 16);
            data[offset++] = (byte)(val >> 24);
        }

        /// <summary>
        /// Writes an unsigned 32-bit little-endian value to a file stream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU32LE(Stream stream, uint val) {
            WriteLE(stream, sizeof(uint), (int)val);
        }

        /// <summary>
        /// Gets an unsigned 64-bit little-endian value from a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <returns>Value retrieved.</returns>
        public static ulong GetU64LE(byte[] data, int offset) {
            ulong val = 0;
            for (int i = 0; i < 8; i++) {
                val |= (ulong)data[offset + i] << (i * 8);
            }
            return val;
        }

        /// <summary>
        /// Sets an unsigned 64-bit little-endian value in a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <param name="val">Value to write.</param>
        public static void SetU64LE(byte[] data, int offset, ulong val) {
            for (int i = 0; i < 8; i++) {
                data[offset + i] = (byte)val;
                val >>= 8;
            }
        }

        /// <summary>
        /// Reads an unsigned 64-bit little-endian value from a byte stream,
        /// advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 8.</param>
        /// <returns>Value retrieved.</returns>
        public static ulong ReadU64LE(byte[] data, ref int offset) {
            ulong val = GetU64LE(data, offset);
            offset += 8;
            return val;
        }

        /// <summary>
        /// Writes an unsigned 64-bit little-endian value to a byte stream, advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 8.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU64LE(byte[] data, ref int offset, ulong val) {
            SetU64LE(data, offset, val);
            offset += 8;
        }

        #endregion Little-Endian

        #region Big-Endian

        /// <summary>
        /// Reads a big-endian value from a file stream.
        /// </summary>
        private static int ReadBE(Stream stream, int width, out bool ok) {
            int result = 0;
            int okCheck = 0;
            for (int i = 0; i < width; i++) {
                int val = stream.ReadByte();        // returns -1 on EOF
                okCheck |= val;                     // sign bit set after bad read
                result = (result << 8) | val;
            }
            ok = (okCheck >= 0);
            return result;
        }

        /// <summary>
        /// Writes a big-endian value to a file stream.
        /// </summary>
        private static void WriteBE(Stream stream, int width, int value) {
            for (int i = 0; i < width; i++) {
                stream.WriteByte((byte)(value >> 24));
                value <<= 8;
            }
        }

        /// <summary>
        /// Gets an unsigned 16-bit big-endian value from a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <returns>Value retrieved.</returns>
        public static ushort GetU16BE(byte[] data, int offset) {
            return (ushort)((data[offset] << 8) | data[offset + 1]);
        }

        /// <summary>
        /// Sets an unsigned 16-bit big-endian value in a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <param name="val">Value to write.</param>
        public static void SetU16BE(byte[] data, int offset, ushort val) {
            data[offset] = (byte)(val >> 8);
            data[offset + 1] = (byte)val;
        }


        /// <summary>
        /// Reads an unsigned 16-bit big-endian value from a byte stream,
        /// advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 2.</param>
        /// <returns>Value retrieved.</returns>
        public static ushort ReadU16BE(byte[] data, ref int offset) {
            ushort val = (ushort)((data[offset] << 8) | data[offset + 1]);
            offset += 2;
            return val;
        }

        /// <summary>
        /// Writes an unsigned 16-bit big-endian value to a byte stream, advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 2.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU16BE(byte[] data, ref int offset, ushort val) {
            data[offset++] = (byte)(val >> 8);
            data[offset++] = (byte)val;
        }

        /// <summary>
        /// Gets an unsigned 24-bit big-endian value from a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <returns>Value retrieved.</returns>
        public static uint GetU24BE(byte[] data, int offset) {
            return (uint)((data[offset] << 16) | (data[offset + 1] << 8) | data[offset + 2]);
        }

        /// <summary>
        /// Writes an unsigned 24-bit big-endian value to a file stream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU24BE(Stream stream, uint val) {
            WriteBE(stream, 3, (int)(val << 8));
        }

        /// <summary>
        /// Gets an unsigned 32-bit big-endian value from a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <returns>Value retrieved.</returns>
        public static uint GetU32BE(byte[] data, int offset) {
            return (uint)((data[offset] << 24) | (data[offset + 1] << 16) |
                (data[offset + 2] << 8) | data[offset + 3]);
        }

        /// <summary>
        /// Sets an unsigned 32-bit big-endian value in a byte stream.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Data offset.</param>
        /// <param name="val">Value to write.</param>
        public static void SetU32BE(byte[] data, int offset, uint val) {
            data[offset] = (byte)(val >> 24);
            data[offset + 1] = (byte)(val >> 16);
            data[offset + 2] = (byte)(val >> 8);
            data[offset + 3] = (byte)val;
        }

        /// <summary>
        /// Reads an unsigned 32-bit big-endian value from a byte stream,
        /// advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 4.</param>
        /// <returns>Value retrieved.</returns>
        public static uint ReadU32BE(byte[] data, ref int offset) {
            uint val = (uint)((data[offset] << 24) | (data[offset + 1] << 16) |
                (data[offset + 2] << 8) | data[offset + 3]);
            offset += 4;
            return val;
        }

        /// <summary>
        /// Reads an unsigned 32-bit big-endian value from a file stream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="ok">Result: true if file read succeeded.</param>
        /// <returns>Value retrieved.</returns>
        public static uint ReadU32BE(Stream stream, out bool ok) {
            return (uint)ReadBE(stream, sizeof(uint), out ok);
        }

        /// <summary>
        /// Writes an unsigned 32-bit big-endian value to a byte stream, advancing the offset.
        /// </summary>
        /// <param name="data">Byte stream.</param>
        /// <param name="offset">Initial offset.  Value will be incremented by 4.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU32BE(byte[] data, ref int offset, uint val) {
            data[offset++] = (byte)(val >> 24);
            data[offset++] = (byte)(val >> 16);
            data[offset++] = (byte)(val >> 8);
            data[offset++] = (byte)val;
        }

        /// <summary>
        /// Writes an unsigned 32-bit big-endian value to a file stream.
        /// </summary>
        /// <param name="stream">File stream.</param>
        /// <param name="val">Value to write.</param>
        public static void WriteU32BE(Stream stream, uint val) {
            WriteBE(stream, sizeof(uint), (int)val);
        }

        #endregion Big-Endian


        /// <summary>
        /// Determines whether a region of data is entirely zeroes.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="length">Length of data to check.</param>
        /// <returns>True if all values are zero.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid length.</exception>
        public static bool IsAllZeroes(byte[] data, int offset, int length) {
            return IsAllValue(data, offset, length, 0);
        }

        /// <summary>
        /// Determines whether a region of data is entirely one specific value.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="length">Length of data to check.</param>
        /// <param name="value">Value to compare to.</param>
        /// <returns>True if all values in the buffer match the argument.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Invalid length.</exception>
        public static bool IsAllValue(byte[] data, int offset, int length, byte value) {
            if (length <= 0) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            while (length-- != 0) {
                if (data[offset++] != value) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Finds the first zero byte in a region of data.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="length">Length of data to check.</param>
        /// <returns>Index of first zero byte.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static int FirstZero(byte[] data, int offset, int length) {
            if (length <= 0) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            for (int i = 0; i < length; i++) {
                if (data[offset + i] == 0) {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Compares two byte arrays.
        /// </summary>
        /// <returns>True if they match, false if they're different.</returns>
        public static bool CompareBytes(byte[] ar1, byte[] ar2, int count) {
            if (ar1.Length < count || ar2.Length < count) {
                return false;
            }
            for (int i = 0; i < count; i++) {
                if (ar1[i] != ar2[i]) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compares two byte arrays.
        /// </summary>
        /// <returns>True if they match, false if they're different.</returns>
        public static bool CompareBytes(byte[] ar1, int offset1, byte[] ar2, int offset2,
                int count) {
            for (int i = 0; i < count; i++) {
                if (ar1[offset1 + i] != ar2[offset2 + i]) {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Compares the contents of two byte arrays.
        /// </summary>
        /// <param name="ar1">First array.</param>
        /// <param name="ar2">Second array.</param>
        /// <param name="count">Number of bytes to compare.  May be zero.</param>
        /// <returns>A value less than, equal to, or greater than zero depending on whether the
        ///   first "count" bytes of ar1 are less then, equal to, or greater than those in
        ///   ar2.</returns>
        public static int MemCmp(byte[] ar1, byte[] ar2, int count) {
            if (count < 0) {
                throw new ArgumentOutOfRangeException(nameof(count), "must be positive");
            }
            if (ar1.Length < count || ar2.Length < count) {
                throw new ArgumentException("invalid count: len1=" + ar1.Length +
                    " len2=" + ar2.Length + " count=" + count);
            }
            for (int i = 0; i < count; i++) {
                if (ar1[i] != ar2[i]) {
                    if (ar1[i] < ar2[i]) {
                        return -1;
                    } else {
                        return 1;
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Sets a range of bytes to a value.
        /// </summary>
        /// <remarks>
        /// cf. https://stackoverflow.com/q/1897555/294248
        /// </remarks>
        /// <param name="array">Array to operate on.</param>
        /// <param name="offset">Starting offset.</param>
        /// <param name="length">Length of region.</param>
        /// <param name="value">Value to store.</param>
        public static void MemSet(byte[] array, int offset, int length, byte value) {
            for (int i = 0; i < length; i++) {
                array[offset + i] = value;
            }
        }

        /// <summary>
        /// Converts a 32-bit little-endian character constant to a 4-character string.
        /// </summary>
        /// <remarks>
        /// Assumes ASCII; does not restrict characters to printable values.
        /// </remarks>
        /// <param name="val">Constant.</param>
        /// <returns>Four-character string.</returns>
        public static string StringifyU32LE(uint val) {
            char[] arr = new char[4];
            for (int i = 0; i < 4; i++) {
                arr[i] = (char)((byte)(val >> i * 8));
            }
            return new string(arr);
        }

        /// <summary>
        /// Converts a 32-bit big-endian character constant to a 4-character string.
        /// </summary>
        /// <remarks>
        /// Assumes ASCII; does not restrict characters to printable values.
        /// </remarks>
        /// <param name="val">Constant.</param>
        /// <returns>Four-character string.</returns>
        public static string StringifyU32BE(uint val) {
            uint leVal = (val >> 16) | ((val << 16) & 0xffff0000);
            leVal = ((leVal >> 8) & 0x00ff00ff) | ((leVal << 8) & 0xff00ff00);
            return StringifyU32LE(leVal);
        }

        /// <summary>
        /// Converts a 4-character ASCII string to a 32-bit integer.
        /// </summary>
        /// <remarks>
        /// Results are undefined if the argument includes non-ASCII chars.
        /// </remarks>
        /// <param name="str">String to convert.</param>
        /// <returns>Integer representation.</returns>
        public static int IntifyASCII(string str) {
            if (str.Length != 4) {
                throw new ArgumentException("String must be four characters: '" + str + "'");
            }
            return (str[0] << 24) | (str[1] << 16) | (str[2] << 8) | str[3];
        }

        /// <summary>
        /// Generates a simple hex dump from a byte array.
        /// </summary>
        /// <param name="array">Data to dump.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="len">Length of data.</param>
        /// <returns>Hex dump in string.</returns>
        public static string SimpleHexDump(byte[] array, int offset, int len) {
            StringBuilder sb = new StringBuilder();
            string addrFmt;
            if (len >= 0x100) {
                addrFmt = "x4";
            } else {
                addrFmt = "x2";
            }
            for (int i = 0; i < len; i++) {
                if ((i & 0x0f) == 0) {
                    if (i != 0) {
                        sb.AppendLine();
                    }
                    sb.Append(i.ToString(addrFmt));
                    sb.Append(':');
                }

                sb.Append(' ');
                sb.Append(array[offset + i].ToString("x2"));
            }
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Generates a simple hex dump from a byte array.
        /// </summary>
        /// <param name="array">Data to dump.</param>
        /// <returns>Hex dump in string.</returns>
        public static string SimpleHexDump(byte[] array) {
            return SimpleHexDump(array, 0, array.Length);
        }

#if false
        // The compiler is apparently clever enough to figure out what CompareBytes() is
        // doing and make it very efficient, so there's no value in getting fancy.  In fact,
        // the fancy versions are slightly slower.
        // cf. https://stackoverflow.com/a/48599119/294248

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        static bool CompareBytesM(byte[] b1, byte[] b2) {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        public static bool CompareBytesS(byte[] ar1, int offset1, byte[] ar2, int offset2,
                int count) {
            ReadOnlySpan<byte> span1 = new ReadOnlySpan<byte>(ar1, offset1, count);
            ReadOnlySpan<byte> span2 = new ReadOnlySpan<byte>(ar2, offset2, count);

            return span1.SequenceEqual(span2);
        }

        public static void TestSpeed() {
            int count = 1024 * 1024 + 9;        // just over 1MB

            byte[] array1 = new byte[count];
            byte[] array2 = new byte[count];
            for (int i = 0; i < count; i++) {
                array1[i] = array2[i] = (byte)i;
            }

            Debug.Assert(CompareBytes(array1, array2));
            Debug.Assert(CompareBytesM(array1, array2));
            Debug.Assert(CompareBytes(array1, 0, array2, 0, array1.Length));
            Debug.Assert(CompareBytesS(array1, 0, array2, 0, array1.Length));

            const int RUNS = 100;
            Stopwatch stopWatch = new Stopwatch();
            int good;

            good = 0;
            stopWatch.Start();
            for (int i = 0; i < RUNS; i++) {
                good += CompareBytes(array1, array2) ? 1 : 0;
            }
            stopWatch.Stop();
            Debug.WriteLine("Simple (" + good + "): " + stopWatch.ElapsedMilliseconds);

            good = 0;
            stopWatch.Start();
            for (int i = 0; i < RUNS; i++) {
                good += CompareBytesM(array1, array2) ? 1 : 0;
            }
            stopWatch.Stop();
            Debug.WriteLine("Memcmp (" + good + "): " + stopWatch.ElapsedMilliseconds);

            good = 0;
            stopWatch.Start();
            for (int i = 0; i < RUNS; i++) {
                good += CompareBytes(array1, 3, array2, 3, array1.Length - 3) ? 1 : 0;
            }
            stopWatch.Stop();
            Debug.WriteLine("Offsets (" + good + "): " + stopWatch.ElapsedMilliseconds);

            good = 0;
            stopWatch.Start();
            for (int i = 0; i < RUNS; i++) {
                good += CompareBytesS(array1, 3, array2, 3, array1.Length - 3) ? 1 : 0;
            }
            stopWatch.Stop();
            Debug.WriteLine("Spans (" + good + "): " + stopWatch.ElapsedMilliseconds);
        }
#endif
    }
}
