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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using CommonUtil;

namespace SourceGen.Tools.Omf {
    /// <summary>
    /// Apple IIgs OMF file segment.
    /// </summary>
    /// <remarks>
    /// Three versions of OMF were used for Apple IIgs binaries: v1.0, v2.0, and v2.1.  (There's
    /// also a "v0" used for older Orca/M 8-bit products.)  The Apple IIgs Programmer's
    /// Workshop Reference says:
    ///
    /// "This section describes Version 2.0 of the Apple IIGS object module
    /// format(OMF). The System Loader supports files written in either Version 2.0 or
    /// Version 1.0 of the OMF.  The APW Linker, however, creates load files that
    /// conform to Version 1.0 of the OMF.  Notes in this section describe the differences
    /// between Version 1.0 and Version 2.0 of the OMF.  The Compact utility program,
    /// described in Chapter 3, converts load files from Version 1.0 to Version 2.0."
    ///
    /// Most IIgs binaries are v1.0 or v2.0.
    ///
    /// You'd hope that parsing a segment would be unambiguous, but that is not the case.
    /// From the same reference:
    ///
    /// "In Version 1.0, [the first] field is described as follows. For object files
    /// and load files, BLKCNT is a 4-byte field containing the number of blocks in the file
    /// that the segment requires.  Each block is 512 bytes.  The segment header is part of
    /// the first block of the segment.  Segments in an object file or load file start on block
    /// boundaries.  For library files (ProDOS 16 file type $B2), this field is BYTECNT,
    /// indicating the number of bytes in the segment.  Library-file segments are not
    /// aligned to block boundaries."
    ///
    /// This choice means it's impossible to unambiguously parse a v1 OMF file without knowing
    /// its ProDOS file type, which we don't have access to.  In most cases we can make a
    /// reasonable guess.
    ///
    /// Documentation bugs:
    /// - GS/OS ref: table F-2 says "blockCount" where it should say "SEGNAME", and shows the
    ///   offset of tempOrg as $2a (should be $2c).
    /// - GS/OS ref: appendix F refers to a "REVISION" field, which does not seem to exist.
    /// </remarks>
    public class OmfSegment {
        // v0.0: Original Orca/M OMF format.  0x24 bytes followed by variable-length SEGNAME.
        public const int MIN_HEADER_V0 = 0x24 + 1;
        // v1.0: Initial IIgs OMF format.  Adds LCBANK, SEGNUM, ENTRY, DISPNAME, DISPDATA, and
        // LOADNAME.  Ambiguates BLKCNT/BYTECNT.
        public const int MIN_HEADER_V1 = MIN_HEADER_V0 + 8 + LOAD_NAME_LEN;
        // v2.0: Updated IIgs OMF format.  Removes LCBANK, redefines KIND, and embraces BYTECNT.
        public const int MIN_HEADER_V2 = MIN_HEADER_V1 + 4;
        // v2.1: adds tempORG and a couple of attribute flags.  No "min" constant needed.

        // Length of LOADNAME field.
        private const int LOAD_NAME_LEN = 10;

        public class NameValueNote {
            public string Name { get; set; }
            public object Value { get; set; }
            public string Note { get; set; }
        }

        /// <summary>
        /// Values pulled from file header.  Useful for display.
        /// </summary>
        List<NameValueNote> RawValues = new List<NameValueNote>();

        public enum SegmentVersion { v0_0, v1_0, v2_0, v2_1 }

        public enum SegmentKind {
            Code = 0x00,
            Data = 0x01,
            JumpTable = 0x02,
            PathName = 0x04,
            LibraryDict = 0x08,
            Init = 0x10,
            AbsoluteBank = 0x11,        // v1.0 only; became a flag
            DpStack = 0x12
        }

        /// <summary>
        /// Segment attribute flags, included in the Kind field.
        /// </summary>
        [Flags]
        public enum SegmentAttribute {
            BankRel = 0x0100,           // v2.1
            Skip = 0x0200,              // v2.1
            Reloadable = 0x0400,        // v2.0
            AbsoluteBank = 0x0800,      // v2.0
            NoSpecial = 0x1000,         // v2.0
            PositionIndep = 0x2000,     //
            Private = 0x4000,           //
            Dynamic = 0x8000            //
        }

        //
        // Header fields.
        //

        public int FileLength { get; private set; }     // from BLKCNT or BYTECNT
        public int ResSpc { get; private set; }
        public int Length { get; private set; }
        public int Type { get; private set; }
        public int LabLen { get; private set; }
        public SegmentVersion Version { get; private set; }
        public int BankSize { get; private set; }
        public SegmentKind Kind { get; private set; }
        public SegmentAttribute Attrs { get; private set; }
        public int Org { get; private set; }
        public int Align { get; private set; }
        public int LcBank { get; private set; }         // v1.0 only
        public int SegNum { get; private set; }
        public int Entry { get; private set; }
        public int TempOrg { get; private set; }        // v2.1 only
        public string LoadName { get; private set; }    // unused in load segments
        public string SegName { get; private set; }

        // According to GS/OS ref, an OMF file is considered "foreign" unless:
        // - the NUMSEX field is 0
        // - the NUMLEN field is 4
        // - the BANKSIZE field is <= $10000
        // - the ALIGN field is <= $10000
        //
        // So we don't need to store NUMLEN or NUMSEX.  According to the GS/OS ref,
        // "The BANKSIZE and align restrictions are enforced by the linker, and violations
        // of them are unlikely in a load file."

        private OmfSegment() { }

        public enum ParseResult {
            Unknown = 0,
            Success,
            Failure,
            IsLibrary
        }

        public static ParseResult ParseSegment(byte[] data, int offset, bool parseAsLibrary,
                out OmfSegment segResult) {
            segResult = null;

            Debug.Assert(offset < data.Length);
            if (data.Length - offset < MIN_HEADER_V0) {
                // Definitely too small.
                return ParseResult.Failure;
            }

            //Debug.WriteLine("PARSE offset=" + offset);

            OmfSegment newSeg = new OmfSegment();

            // Start with the version number.  The meaning of everything else depends on this.
            int minLen, expectedDispName;
            switch (data[offset + 0x0f]) {
                case 0:
                    newSeg.Version = SegmentVersion.v0_0;
                    minLen = MIN_HEADER_V0;
                    expectedDispName = 0x24;
                    break;
                case 1:
                    newSeg.Version = SegmentVersion.v1_0;
                    minLen = MIN_HEADER_V1;
                    expectedDispName = 0x2c;
                    break;
                case 2:
                    newSeg.Version = SegmentVersion.v2_0;
                    minLen = MIN_HEADER_V2;
                    expectedDispName = 0x2c;
                    break;
                default:
                    // invalid version, this is probably not OMF
                    return ParseResult.Failure;
            }
            if (data.Length - offset < minLen) {
                // Too small for this version of the header.
                return ParseResult.Failure;
            }

            int blkByteCnt = RawData.GetWord(data, offset + 0x00, 4, false);
            newSeg.ResSpc = RawData.GetWord(data, offset + 0x04, 4, false);
            newSeg.Length = RawData.GetWord(data, offset + 0x08, 4, false);
            newSeg.LabLen = data[offset + 0x0d];
            int numLen = data[offset + 0x0e];
            newSeg.BankSize = RawData.GetWord(data, offset + 0x10, 4, false);
            newSeg.Org = RawData.GetWord(data, offset + 0x18, 4, false);
            newSeg.Align = RawData.GetWord(data, offset + 0x1c, 4, false);
            int numSex = data[offset + 0x20];
            int dispName, dispData;
            if (newSeg.Version == SegmentVersion.v0_0) {
                dispName = 0x24;
                if (newSeg.LabLen == 0) {
                    dispData = dispName + data[offset + dispName];
                } else {
                    dispData = dispName + LOAD_NAME_LEN;
                }
            } else {
                newSeg.LcBank = data[offset + 0x21];
                newSeg.SegNum = RawData.GetWord(data, offset + 0x22, 2, false);
                newSeg.Entry = RawData.GetWord(data, offset + 0x24, 4, false);
                dispName = RawData.GetWord(data, offset + 0x28, 2, false);
                dispData = RawData.GetWord(data, offset + 0x2a, 2, false);
            }

            // The only way to detect a v2.1 segment is by checking DISPNAME.
            if (newSeg.Version == SegmentVersion.v2_0 && dispName > 0x2c) {
                newSeg.Version = SegmentVersion.v2_1;
                expectedDispName += 4;

                if (data.Length - offset < minLen + 4) {
                    return ParseResult.Failure;
                }
                newSeg.TempOrg = RawData.GetWord(data, offset + 0x2c, 4, false);
            }

            // Extract Kind and its attributes.
            int kindByte, kindWord;
            if (newSeg.Version <= SegmentVersion.v1_0) {
                kindByte = data[offset + 0x0c];
                if (!Enum.IsDefined(typeof(SegmentKind), kindByte & 0x1f)) {
                    // Example: Moria GS has a kind of $1F for its GLOBALS segment.
                    Debug.WriteLine("Invalid segment kind $" + kindByte.ToString("x2"));
                    return ParseResult.Failure;
                }
                newSeg.Kind = (SegmentKind)(kindByte & 0x1f);

                int kindAttrs = 0;
                if ((kindByte & 0x20) != 0) {
                    kindAttrs |= (int)SegmentAttribute.PositionIndep;
                }
                if ((kindByte & 0x40) != 0) {
                    kindAttrs |= (int)SegmentAttribute.Private;
                }
                if ((kindByte & 0x80) != 0) {
                    kindAttrs |= (int)SegmentAttribute.Dynamic;
                }
                newSeg.Attrs = (SegmentAttribute)kindAttrs;
            } else {
                // Yank all the attribute bits out at once.  Don't worry about v2.0 vs. v2.1.
                kindWord = RawData.GetWord(data, offset + 0x14, 2, false);
                if (!Enum.IsDefined(typeof(SegmentKind), kindWord & 0x001f)) {
                    Debug.WriteLine("Invalid segment kind $" + kindWord.ToString("x4"));
                    return ParseResult.Failure;
                }
                newSeg.Kind = (SegmentKind)(kindWord & 0x001f);
                newSeg.Attrs = (SegmentAttribute)(kindWord & 0xff00);
            }

            // If we found a library dictionary segment, and we're not currently handling the
            // file as a library, reject this and try again.
            if (newSeg.Kind == SegmentKind.LibraryDict && !parseAsLibrary) {
                return ParseResult.IsLibrary;
            }

            // We've got the basic pieces.  Handle the block-vs-byte debacle.
            int segLen;
            if (newSeg.Version == SegmentVersion.v0_0) {
                // Always block count.
                segLen = blkByteCnt * 512;
            } else if (newSeg.Version >= SegmentVersion.v2_0) {
                // Always byte count.
                segLen = blkByteCnt;
            } else /*v1.0*/ {
                // Only Library files should treat the field as bytes.  We can eliminate Load
                // files by checking for a nonzero SegNum field, but there's no reliable way
                // to tell the difference between Object and Library while looking at a segment
                // in isolation.
                //
                // I have found a couple of examples (e.g. BRIDGE.S16 in Davex v1.23, SYSTEM:START
                // on an old Paintworks GS disk) where the file's length is shy of a multiple
                // of 512, so we ought to handle that.
                if (parseAsLibrary) {
                    segLen = blkByteCnt;
                } else {
                    segLen = blkByteCnt * 512;
                }
            }
            newSeg.FileLength = segLen;

            // Perform validity checks.  If any of these fail, we're probably reading something
            // that isn't OMF (or, if this isn't the first segment, we might have gone off the
            // rails at some point).
            if (numLen != 4 || numSex != 0) {
                Debug.WriteLine("Invalid NUMLEN (" + numLen + ") or NUMSEX (" + numSex + ")");
                return ParseResult.Failure;
            }
            if (offset + segLen > data.Length) {
                // Segment is longer than the file.  (This can happen easily in a static lib.)
                Debug.WriteLine("Segment exceeds EOF: offset=" + offset + " len=" + data.Length +
                    " segLen=" + segLen);
                return ParseResult.Failure;
            }
            if (dispName < expectedDispName || dispName > (segLen - LOAD_NAME_LEN)) {
                Debug.WriteLine("Invalid DISPNAME " + dispName + " segLen=" + segLen);
                return ParseResult.Failure;
            }
            if (dispData < expectedDispName + LOAD_NAME_LEN || dispData > (segLen - 1)) {
                Debug.WriteLine("Invalid DISPDATA " + dispData + " segLen=" + segLen);
                return ParseResult.Failure;
            }
            if (newSeg.BankSize > 0x00010000) {
                Debug.WriteLine("Invalid BANKSIZE $" + newSeg.BankSize.ToString("x"));
                return ParseResult.Failure;
            }
            if (newSeg.Align > 0x00010000) {
                Debug.WriteLine("Invalid ALIGN $" + newSeg.Align.ToString("x"));
                return ParseResult.Failure;
            }

            if (newSeg.BankSize != 0x00010000 && newSeg.BankSize != 0) {
                // This is fine, just a little weird.
                Debug.WriteLine("Unusual BANKSIZE $" + newSeg.BankSize.ToString("x6"));
            }
            if (newSeg.Align != 0 && newSeg.Align != 0x0100 && newSeg.Align != 0x00010000) {
                // Unexpected; the loader will round up.
                Debug.WriteLine("Unusual ALIGN $" + newSeg.Align.ToString("x6"));
            }
            if (newSeg.Entry != 0 && newSeg.Entry >= newSeg.Length) {
                // This is invalid, but if we got this far we might as well keep going.
                Debug.WriteLine("Invalid ENTRY $" + newSeg.Entry.ToString("x6"));
            }

            // Extract LOADNAME.  Fixed-width field, padded with spaces.  Except for the
            // times when it's filled with zeroes instead.
            string loadName = string.Empty;
            int segNameStart = dispName;
            if (newSeg.Version != SegmentVersion.v0_0) { 
                loadName = ExtractString(data, offset + dispName, LOAD_NAME_LEN);
                segNameStart += LOAD_NAME_LEN;
            }

            // Extract SEGNAME.  May be fixed- or variable-width.
            string segName;
            if (newSeg.LabLen == 0) {
                // string preceded by length byte
                int segNameLen = data[offset + segNameStart];
                if (segNameStart + 1 + segNameLen > segLen) {
                    Debug.WriteLine("Var-width SEGNAME ran off end of segment (len=" +
                        segNameLen + ")");
                    return ParseResult.Failure;
                }
                segName = Encoding.ASCII.GetString(data, offset + segNameStart + 1, segNameLen);
            } else {
                // fixed-width string
                if (segNameStart + newSeg.LabLen > segLen) {
                    Debug.WriteLine("Fixed-width SEGNAME ran off end of segment (len=" +
                        newSeg.LabLen + ")");
                    return ParseResult.Failure;
                }
                segName = ExtractString(data, offset + segNameStart, newSeg.LabLen);
            }

            Debug.WriteLine("LOADNAME='" + loadName + "' SEGNAME='" + segName + "'");
            newSeg.LoadName = loadName;
            newSeg.SegName = segName;

            segResult = newSeg;
            return ParseResult.Success;
        }

        private static string ExtractString(byte[] data, int offset, int len) {
            StringBuilder sb = new StringBuilder();
            for (int i = offset; i < offset + len; i++) {
                byte b = data[i];
                if (b == 0) {
                    break;
                }
                sb.Append((char)b);
            }
            return sb.ToString();
        }
    }
}
