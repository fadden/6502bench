/*
 * Copyright 2025 faddenSoft
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using CommonUtil;

namespace SourceGen {
    /// <summary>
    /// Special handling for VICE .CRT cartridge files.
    /// </summary>
    /// <remarks>
    /// <para>This is based on the format described in section 17.14 of the VICE documentation,
    /// <see href="https://vice-emu.sourceforge.io/vice_17.html#SEC442">"The CRT cartridge image format"</see>.
    /// </para>
    /// </remarks>
    public static class ViceCrt {
        private static readonly byte[] CBM80 = new byte[] { 0xc3, 0xc2, 0xcd, 0x38, 0x30 };

        // End-of-line comments for the file header fields.
        private static readonly Dictionary<int, string> sHeaderComments =
                new Dictionary<int, string>() {
            { 0x00, "signature" },
            { 0x10, "header length"},
            { 0x14, "cartridge version" },
            { 0x16, "hardware type" },
            { 0x18, "EXROM line status" },
            { 0x19, "GAME line status" },
            { 0x1a, "hardware revision/subtype" },
            { 0x1b, "reserved" },
            { 0x20, "name" },
        };

        // End-of-line comments for CHIP fields.
        private static readonly Dictionary<int, string> sChipComments =
                new Dictionary<int, string>() {
            { 0x00, "signature" },
            { 0x04, "packet length" },
            { 0x08, "chip type" },
            { 0x0a, "bank number" },
            { 0x0c, "load address" },
            { 0x0e, "image size" },
        };

        /// <summary>
        /// Cartridge file header.
        /// </summary>
        private class Header {
            public const int MIN_LENGTH = 64;
            public const int CART_NAME_BUF_LEN = 32;

            // Known header signatures.
            private static readonly byte[][] SIGNATURES = new byte[][] {
                Encoding.ASCII.GetBytes("C64 CARTRIDGE   "),
                Encoding.ASCII.GetBytes("C128 CARTRIDGE  "),
                Encoding.ASCII.GetBytes("CBM2 CARTRIDGE  "),
                Encoding.ASCII.GetBytes("VIC20 CARTRIDGE "),
                Encoding.ASCII.GetBytes("PLUS4 CARTRIDGE "),
            };

            /// <summary>
            /// Cartridge signature string.  16 chars, padded with spaces.
            /// </summary>
            public string Signature { get { return Encoding.ASCII.GetString(mSignature); } }

            /// <summary>
            /// True if the signature is one we recognize.
            /// </summary>
            public bool HasValidSignature {
                get {
                    foreach (byte[] sig in SIGNATURES) {
                        if (RawData.CompareBytes(sig, mSignature, mSignature.Length)) {
                            return true;
                        }
                    }
                    return false;
                }
            }

            /// <summary>
            /// Cartridge name.  Upper case, padded with null bytes.
            /// </summary>
            public string CartName {
                get {
                    int len;
                    for (len = 0; len < mCartName.Length; len++) {
                        if (mCartName[len] == 0x00) {
                            break;
                        }
                    }
                    return Encoding.ASCII.GetString(mCartName, 0, len);
                }
            }

            public int ActualLength {
                get { return mHeaderLen < MIN_LENGTH ? MIN_LENGTH : (int)mHeaderLen; }
            }

            public byte[] mSignature = new byte[16];
            public uint mHeaderLen;
            public ushort mCartVersion;
            public ushort mCartHardwareType;
            public byte mCartExromLine;
            public byte mCartGameLine;
            public byte mCartHardwareRevision;
            public byte[] mReserved = new byte[5];
            public byte[] mCartName = new byte[CART_NAME_BUF_LEN];

            public void Load(byte[] buf, ref int offset) {
                if (offset >= buf.Length - MIN_LENGTH) {
                    return;     // too short
                }
                int startOffset = offset;
                Array.Copy(buf, offset, mSignature, 0, mSignature.Length);
                offset += mSignature.Length;
                mHeaderLen = RawData.ReadU32BE(buf, ref offset);
                mCartVersion = RawData.ReadU16BE(buf, ref offset);
                mCartHardwareType = RawData.ReadU16BE(buf, ref offset);
                mCartExromLine = RawData.ReadU8(buf, ref offset);
                mCartGameLine = RawData.ReadU8(buf, ref offset);
                mCartHardwareRevision = RawData.ReadU8(buf, ref offset);
                Array.Copy(buf, offset, mReserved, 0, mReserved.Length);
                offset += mReserved.Length;
                Array.Copy(buf, offset, mCartName, 0, mCartName.Length);
                offset += mCartName.Length;
                Debug.Assert(offset == startOffset + MIN_LENGTH);

                // Skip past any additional bytes.  Not expected.
                offset += ActualLength - MIN_LENGTH;
            }

            public bool Validate() {
                if (!HasValidSignature) {
                    return false;
                }
                if (mHeaderLen > 1024) {
                    // Arbitrary limit.  In practice, should always be 0x40, though the docs
                    // say some (malformed) files have 0x20.
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// "CHIP" chunk header.
        /// </summary>
        private class Chip {
            public const int LENGTH = 16;
            private static byte[] SIGNATURE = Encoding.ASCII.GetBytes("CHIP");

            public bool HasValidSignature {
                get { return RawData.CompareBytes(SIGNATURE, mSignature, SIGNATURE.Length); }
            }

            public byte[] mSignature = new byte[4];
            public uint mPacketLength;
            public ushort mChipType;
            public ushort mBankNumber;
            public ushort mLoadAddr;
            public ushort mImageSize;

            public int mHdrOffset;
            public int mDataOffset;

            public void Load(byte[] buf, ref int offset) {
                if (offset >= buf.Length - LENGTH) {
                    return;     // too short
                }
                int startOffset = offset;
                mHdrOffset = offset;
                Array.Copy(buf, offset, mSignature, 0, mSignature.Length);
                offset += mSignature.Length;
                mPacketLength = RawData.ReadU32BE(buf, ref offset);
                mChipType = RawData.ReadU16BE(buf, ref offset);
                mBankNumber = RawData.ReadU16BE(buf, ref offset);
                mLoadAddr = RawData.ReadU16BE(buf, ref offset);
                mImageSize = RawData.ReadU16BE(buf, ref offset);
                Debug.Assert(offset == startOffset + LENGTH);

                mDataOffset = offset;
            }

            public bool Validate(byte[] buf) {
                if (!HasValidSignature) {
                    Debug.WriteLine("Invalid CHIP signature");
                    return false;
                }
                if (mImageSize == 0 || (mImageSize & 0x00ff) != 0) {
                    Debug.WriteLine("Bad CHIP image size: " + mImageSize);
                    return false;
                }
                if (mPacketLength != mImageSize + LENGTH) {
                    // Docs say "should always be equal to ROM image size + $10".
                    Debug.WriteLine("Bad CHIP length: pkt=" + mPacketLength +
                        ", img=" + mImageSize);
                    return false;
                }
                if (mDataOffset > buf.Length - mImageSize) {
                    Debug.WriteLine("CHIP extends off end of file: off=" + mDataOffset +
                        " size=" + mImageSize + " bufLen=" + buf.Length);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Configures the project based on the contents of the data file, which must be in
        /// VICE CRT format.
        /// </summary>
        /// <param name="project">Project instance.</param>
        /// <returns>Failure message, or empty string on success.</returns>
        public static string ConfigureProject(DisasmProject project) {
            byte[] dataBuf = project.FileData;
            int offset = 0;

            //
            // Read the file header and all CHIPs.  Validate everything before we make any
            // changes to the project.
            //
            Header hdr = new Header();
            hdr.Load(dataBuf, ref offset);
            if (!hdr.Validate()) {
                return "invalid CRT header";
            }

            List<Chip> chips = new List<Chip>();
            while (offset < dataBuf.Length - Chip.LENGTH) {
                Chip chip = new Chip();
                chip.Load(dataBuf, ref offset);
                if (!chip.Validate(dataBuf)) {
                    return "invalid CHIP #" + chips.Count;
                }
                chips.Add(chip);
                offset += chip.mImageSize;
            }
            if (chips.Count == 0) {
                return "no CHIPs found";
            }
            if (offset != dataBuf.Length) {
                Debug.WriteLine("Warning: found extra bytes at end: " + (dataBuf.Length - offset));
                // keep going
            }

            Debug.WriteLine("name='" + hdr.CartName + "' chips=" + chips.Count);

            // Configure address map.
            project.AddrMap.Clear();
            project.AddrMap.AddEntry(0, hdr.ActualLength, AddressMap.NON_ADDR);
            offset = hdr.ActualLength;
            foreach (Chip chip in chips) {
                project.AddrMap.AddEntry(offset, Chip.LENGTH, AddressMap.NON_ADDR);
                offset += Chip.LENGTH;
                Debug.Assert(offset == chip.mDataOffset);
                project.AddrMap.AddEntry(offset, chip.mImageSize, chip.mLoadAddr);
                offset += chip.mImageSize;
            }

            // Generate a file comment.
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(Res.Strings.DEFAULT_HEADER_COMMENT_FMT, App.ProgramVersion);
            sb.AppendLine();
            sb.AppendLine();
            sb.Append("File signature: ");
            sb.AppendLine(hdr.Signature.Trim());
            sb.Append("Cartridge name: ");
            sb.AppendLine(hdr.CartName);
            sb.AppendLine("CHIP packets:");

            // Format data items in file header.
            project.OperandFormats[0x00] = FormatDescriptor.Create(16,
                FormatDescriptor.Type.StringGeneric, FormatDescriptor.SubType.Ascii);
            project.OperandFormats[0x10] = FormatDescriptor.Create(4,
                FormatDescriptor.Type.NumericBE, FormatDescriptor.SubType.None);
            project.OperandFormats[0x14] = FormatDescriptor.Create(2,
                FormatDescriptor.Type.NumericBE, FormatDescriptor.SubType.None);
            project.OperandFormats[0x16] = FormatDescriptor.Create(2,
                FormatDescriptor.Type.NumericBE, FormatDescriptor.SubType.None);
            project.OperandFormats[0x1b] = FormatDescriptor.Create(5,
                FormatDescriptor.Type.Dense, FormatDescriptor.SubType.None);
            int cartNameLen = hdr.CartName.Length;
            project.OperandFormats[0x20] = FormatDescriptor.Create(cartNameLen,
                FormatDescriptor.Type.StringGeneric, FormatDescriptor.SubType.Ascii);
            if (cartNameLen < Header.CART_NAME_BUF_LEN) {
                project.OperandFormats[0x20 + cartNameLen] =
                    FormatDescriptor.Create(Header.CART_NAME_BUF_LEN - cartNameLen,
                        FormatDescriptor.Type.Fill, FormatDescriptor.SubType.None);
            }

            foreach (KeyValuePair<int, string> kvp in sHeaderComments) {
                project.Comments[kvp.Key] = kvp.Value;
            }

            // Format data items in CHIP headers.
            for (int i = 0; i < chips.Count; i++) {
                Chip chip = chips[i];
                offset = chip.mHdrOffset;

                project.Notes[offset] =
                    new MultiLineComment("CHIP #" + i + " $" + chip.mLoadAddr.ToString("X4"));

                project.OperandFormats[offset + 0x00] = FormatDescriptor.Create(4,
                    FormatDescriptor.Type.StringGeneric, FormatDescriptor.SubType.Ascii);
                project.OperandFormats[offset + 0x04] = FormatDescriptor.Create(4,
                    FormatDescriptor.Type.NumericBE, FormatDescriptor.SubType.None);
                project.OperandFormats[offset + 0x08] = FormatDescriptor.Create(2,
                    FormatDescriptor.Type.NumericBE, FormatDescriptor.SubType.None);
                project.OperandFormats[offset + 0x0a] = FormatDescriptor.Create(2,
                    FormatDescriptor.Type.NumericBE, FormatDescriptor.SubType.None);
                project.OperandFormats[offset + 0x0c] = FormatDescriptor.Create(2,
                    FormatDescriptor.Type.NumericBE, FormatDescriptor.SubType.None);
                project.OperandFormats[offset + 0x0e] = FormatDescriptor.Create(2,
                    FormatDescriptor.Type.NumericBE, FormatDescriptor.SubType.None);

                foreach (KeyValuePair<int, string> kvp in sChipComments) {
                    project.Comments[offset + kvp.Key] = kvp.Value;
                }

                sb.AppendFormat(" #{0:D2}: addr=${1:X4} len=${2:X4}",
                    i, chip.mLoadAddr, chip.mImageSize);

                // Look for CBM80 at +0x04.  If found, do additional formatting.  This is
                // often present in the first chunk, and occasionally present in later chunks
                // (see e.g. Action Replay 5 NTSC).
                offset = chip.mDataOffset;
                if (RawData.CompareBytes(CBM80, 0, dataBuf, offset + 4, CBM80.Length)) {
                    // First two words are the addresses of the hard-reset and soft-reset handlers.
                    project.OperandFormats[offset + 0x00] = FormatDescriptor.Create(2,
                        FormatDescriptor.Type.NumericLE, FormatDescriptor.SubType.Address);
                    project.OperandFormats[offset + 0x02] = FormatDescriptor.Create(2,
                        FormatDescriptor.Type.NumericLE, FormatDescriptor.SubType.Address);
                    project.OperandFormats[offset + 0x04] = FormatDescriptor.Create(5,
                        FormatDescriptor.Type.StringGeneric, FormatDescriptor.SubType.C64Petscii);

                    // If the handler addresses fall within this chunk, tag them as code starts.
                    TryTagChunkAddr(project, chip, RawData.GetU16LE(dataBuf, offset + 0x00));
                    TryTagChunkAddr(project, chip, RawData.GetU16LE(dataBuf, offset + 0x02));

                    // The first byte past "CBM80" is sometimes code and sometimes not.
                    //project.AnalyzerTags[offset + 0x09] = CodeAnalysis.AnalyzerTag.Code;

                    sb.Append(" CBM80");
                }

                sb.AppendLine();
            }

            // Remove the code start tag added by the new-project code.
            project.AnalyzerTags[0] = CodeAnalysis.AnalyzerTag.None;

            // Replace the project-header comment.
            sb.AppendLine();
            project.LongComments[LineListGen.Line.HEADER_COMMENT_OFFSET] =
                new MultiLineComment(sb.ToString());

            return string.Empty;
        }

        private static void TryTagChunkAddr(DisasmProject project, Chip chip, ushort addr) {
            if (addr < chip.mLoadAddr || addr >= chip.mLoadAddr + chip.mImageSize) {
                return;
            }
            int offset = addr - chip.mLoadAddr;
            project.AnalyzerTags[chip.mDataOffset + offset] = CodeAnalysis.AnalyzerTag.Code;
        }
    }
}
