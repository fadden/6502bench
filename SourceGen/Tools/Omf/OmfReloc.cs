/*
 * Copyright 2020 faddenSoft
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

using CommonUtil;

namespace SourceGen.Tools.Omf {
    /// <summary>
    /// Apple IIgs OMF relocation entry.  These are generated from OMF relocation records.
    /// Note that SUPER records may generate multiple instances of these.
    /// </summary>
    public class OmfReloc {
        /// <summary>
        /// Number of bytes to be relocated.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Number of bits to shift the result before storing.  Positive values shift left,
        /// negative values shift right (unsigned).
        /// </summary>
        public int Shift { get; private set; }

        /// <summary>
        /// Offset within segment of bytes to rewrite.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Relative offset of the thing being referenced.  The segment's address in memory
        /// is added to this to yield the final value.
        /// </summary>
        public int RelOffset { get; private set; }

        /// <summary>
        /// File number of target segment.  Normally 1.  Will be -1 for intra-segment relocs.
        /// </summary>
        public int FileNum { get; private set; }

        /// <summary>
        /// Segment number of target segment.  Will be -1 for intra-segment relocs.
        /// </summary>
        public int SegNum { get; private set; }

        /// <summary>
        /// Set to the SUPER type, or -1 if this record was not from a SUPER.
        /// </summary>
        public int SuperType { get; private set; }


        /// <summary>
        /// Constructor for RELOC/cRELOC.
        /// </summary>
        /// <param name="width">Width in bytes of relocation.</param>
        /// <param name="shift">Bit-shift amount.</param>
        /// <param name="offset">Offset within segment of relocation.</param>
        /// <param name="relOffset">Relative offset of relocation reference.</param>
        /// <param name="fromSuper">True if generated from SUPER.</param>
        private OmfReloc(int width, int shift, int offset, int relOffset, int superType) {
            FileNum = SegNum = -1;
            Width = width;
            Shift = shift;
            Offset = offset;
            RelOffset = relOffset;
            SuperType = superType;
        }

        /// <summary>
        /// Constructor for INTERSEG/cINTERSEG.
        /// </summary>
        /// <param name="width">Width in bytes of relocation.</param>
        /// <param name="shift">Bit-shift amount.</param>
        /// <param name="offset">Offset within segment of relocation.</param>
        /// <param name="relOffset">Relative offset of relocation reference.</param>
        /// <param name="fileNum">File number.</param>
        /// <param name="segNum">Segment number.</param>
        /// <param name="fromSuper">True if generated from SUPER.</param>
        private OmfReloc(int width, int shift, int offset, int relOffset, int fileNum,
                int segNum, int superType) {
            Width = width;
            Shift = shift;
            Offset = offset;
            RelOffset = relOffset;
            FileNum = fileNum;
            SegNum = segNum;
            SuperType = superType;
        }

        /// <summary>
        /// Adds one or more OmfReloc instances to the list to represent the provided record.
        /// </summary>
        /// <param name="omfSeg">Segment that contains the record.</param>
        /// <param name="omfRec">Record to add.</param>
        /// <param name="data">File data.</param>
        /// <param name="relocs">List of relocations.  New entries will be appended.</param>
        /// <returns>True on success.</returns>
        public static bool GenerateRelocs(OmfSegment omfSeg, OmfRecord omfRec, byte[] data,
                List<OmfReloc> relocs) {
            try {
                return DoGenerateRelocs(omfSeg, omfRec, data, relocs);
            } catch (IndexOutOfRangeException ioore) {
                Debug.WriteLine("Caught IOORE during reloc gen (" + omfRec.Op + "): " +
                    ioore.Message);
                return false;
            }
        }

        /// <summary>
        /// Adds one or more OmfReloc instances to the list to represent the provided record.
        /// </summary>
        private static bool DoGenerateRelocs(OmfSegment omfSeg, OmfRecord omfRec, byte[] data,
                List<OmfReloc> relocs) {
            int offset = omfRec.FileOffset;
            Debug.Assert(data[offset] == (int)omfRec.Op);
            switch (omfRec.Op) {
                case OmfRecord.Opcode.RELOC: {
                        byte width = data[offset + 1];
                        sbyte shift = (sbyte)data[offset + 2];
                        int off = RawData.GetWord(data, offset + 3, 4, false);
                        int relOff = RawData.GetWord(data, offset + 7, 4, false);
                        relocs.Add(new OmfReloc(width, shift, off, relOff, -1));
                    }
                    break;
                case OmfRecord.Opcode.cRELOC: {
                        byte width = data[offset + 1];
                        sbyte shift = (sbyte)data[offset + 2];
                        int off = RawData.GetWord(data, offset + 3, 2, false);
                        int relOff = RawData.GetWord(data, offset + 5, 2, false);
                        relocs.Add(new OmfReloc(width, shift, off, relOff, -1));
                    }
                    break;
                case OmfRecord.Opcode.INTERSEG: {
                        byte width = data[offset + 1];
                        sbyte shift = (sbyte)data[offset + 2];
                        int off = RawData.GetWord(data, offset + 3, 4, false);
                        int fileNum = RawData.GetWord(data, offset + 7, 2, false);
                        int segNum = RawData.GetWord(data, offset + 9, 2, false);
                        int relOff = RawData.GetWord(data, offset + 11, 4, false);
                        relocs.Add(new OmfReloc(width, shift, off, relOff, fileNum, segNum, -1));
                    }
                    break;
                case OmfRecord.Opcode.cINTERSEG: {
                        byte width = data[offset + 1];
                        sbyte shift = (sbyte)data[offset + 2];
                        int off = RawData.GetWord(data, offset + 3, 2, false);
                        int fileNum = 1;
                        int segNum = data[offset + 5];
                        int relOff = RawData.GetWord(data, offset + 6, 2, false);
                        relocs.Add(new OmfReloc(width, shift, off, relOff, fileNum, segNum, -1));
                    }
                    break;
                case OmfRecord.Opcode.SUPER:
                    if (!GenerateRelocForSuper(omfSeg, omfRec, data, relocs)) {
                        return false;
                    }
                    break;
                default:
                    Debug.Assert(false);
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a series of relocation items for a SUPER record.
        /// </summary>
        private static bool GenerateRelocForSuper(OmfSegment omfSeg, OmfRecord omfRec, byte[] data,
                List<OmfReloc> relocs) {
            int offset = omfRec.FileOffset;
            int remaining = RawData.GetWord(data, offset + 1, 4, false);
            int type = data[offset + 5];
            offset += 6;        // we've consumed 6 bytes
            remaining--;        // ...but only 1 counts against the SUPER length

            int width, shift, fileNum, segNum;
            bool needSegNum = false;

            if (type == 0) {
                // SUPER RELOC2
                width = 2;
                shift = 0;
                fileNum = segNum = -1;
            } else if (type == 1) {
                // SUPER RELOC3
                width = 3;
                shift = 0;
                fileNum = segNum = -1;
            } else if (type >= 2 && type <= 13) {
                // SUPER INTERSEG1 - SUPER INTERSEG12
                width = 3;
                shift = 0;
                fileNum = type - 1;
                segNum = -100;
                needSegNum = true;
            } else if (type >= 14 && type <= 25) {
                // SUPER INTERSEG13 - SUPER INTERSEG24
                width = 2;
                shift = 0;
                fileNum = 1;
                segNum = type - 13;
            } else if (type >= 26 && type <= 37) {
                // SUPER INTERSEG25 - SUPER INTERSEG36
                width = 2;
                shift = -16;        // right shift
                fileNum = 1;
                segNum = type - 25;
            } else {
                return false;
            }

            int page = 0;
            while (remaining > 0) {
                int patchCount = data[offset++];
                remaining--;

                if ((patchCount & 0x80) != 0) {
                    // high bit set, this is a skip-count
                    page += (patchCount & 0x7f);
                    continue;
                }
                patchCount++;       // zero means one patch
                while (patchCount-- != 0) {
                    int patchOff = data[offset++];
                    remaining--;

                    int relocOff = page * 256 + patchOff;

                    byte[] constData = omfSeg.GetConstData();
                    int relocRelOff = RawData.GetWord(constData, relocOff, 2, false);
                    if (needSegNum) {
                        segNum = constData[relocOff + 2];
                    }

                    relocs.Add(new OmfReloc(width, shift, relocOff, relocRelOff,
                        fileNum, segNum, type));
                }

                page++;
            }

            if (remaining < 0) {
                Debug.WriteLine("Ran off end of SUPER record");
                Debug.Assert(false);
                return false;
            }

            return true;
        }
    }
}
