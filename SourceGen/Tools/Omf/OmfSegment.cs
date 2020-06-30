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
using System.Text;

using Asm65;
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
    /// You'd hope that parsing segments would be unambiguous, but that is not the case.
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
    ///   offset of "tempOrg" as $2a (should be $2c).
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
        // v2.1: adds TEMPORG and a couple of attribute flags.  No "min" constant needed.

        // Length of LOADNAME field.
        private const int LOAD_NAME_LEN = 10;

        private const int DISK_BLOCK_SIZE = 512;

        public class NameValueNote {
            public string Name { get; private set; }
            public object Value { get; private set; }
            public int Width { get; private set; }
            public string Note { get; private set; }

            public NameValueNote(string name, object value, int width, string note) {
                Name = name;
                Value = value;
                Width = width;
                Note = note;
            }
        }

        /// <summary>
        /// Values pulled from file header.  Useful for display.
        /// </summary>
        public List<NameValueNote> RawValues = new List<NameValueNote>();

        /// <summary>
        /// All known OMF versions.
        /// </summary>
        public enum SegmentVersion { v0_0, v1_0, v2_0, v2_1 }

        /// <summary>
        /// All known segment kinds.
        /// </summary>
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
            AbsBank = 0x0800,           // v2.0
            NoSpecial = 0x1000,         // v2.0
            PosnIndep = 0x2000,         //
            Private = 0x4000,           //
            Dynamic = 0x8000            //
        }

        private byte[] mFileData;

        //
        // Header fields and header-derived values.
        //

        public int FileOffset { get; private set; }
        public int RawFileLength { get; private set; }  // from BLKCNT or BYTECNT
        public int FileLength { get; private set; }     // last block may be short

        public int ResSpc { get; private set; }
        public int Length { get; private set; }
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
        public int DispData { get; private set; }
        public int TempOrg { get; private set; }        // v2.1; only used by MPW IIgs
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

        /// <summary>
        /// Record list, from body of segment.
        /// </summary>
        public List<OmfRecord> Records = new List<OmfRecord>();

        /// <summary>
        /// Relocation list, for segments in Load files.
        /// </summary>
        public List<OmfReloc> Relocs = new List<OmfReloc>();

        /// <summary>
        /// True if this is an ExpressLoad segment.
        /// </summary>
        public bool IsExpressLoad {
            get {
                if (Kind != SegmentKind.Data) {
                    return false;
                }
                if ((Attrs & SegmentAttribute.Dynamic) == 0) {
                    return false;
                }
                // Should be case-insensitive?  I'm assuming it's not padded with spaces since
                // it's longer than 10 chars.
                if (!(SegName == EXPRESSLOAD || SegName == EXPRESSLOAD_OLD)) {
                    return false;
                }
                if (SegNum != 1) {
                    Debug.WriteLine("WEIRD: ~ExpressLoad not first segment");
                }
                return true;
            }
        }
        private const string EXPRESSLOAD = "~ExpressLoad";
        private const string EXPRESSLOAD_OLD = "ExpressLoad";


        // Constructor is private; use ParseHeader() to create an instance.
        private OmfSegment() { }

        public enum ParseResult {
            Unknown = 0,
            Success,
            Failure,
            IsLibrary
        }

        /// <summary>
        /// Parses an OMF segment header.  If successful, a new OmfSegment object is created.
        /// </summary>
        /// <param name="data">File data.</param>
        /// <param name="offset">Offset at which to start parsing.</param>
        /// <param name="parseAsLibrary">Set to true to parse the header as if it were part
        ///   of a library file.  Affects parsing of v1 headers.</param>
        /// <param name="msgs">Notes and errors generated by the parser.</param>
        /// <param name="segResult">Completed object, or null on failure.</param>
        /// <returns>Result code.</returns>
        public static ParseResult ParseHeader(byte[] data, int offset, bool parseAsLibrary,
                List<string> msgs, out OmfSegment segResult) {
            segResult = null;

            //Debug.WriteLine("PARSE offset=" + offset);

            Debug.Assert(offset < data.Length);
            if (data.Length - offset < MIN_HEADER_V0) {
                // Definitely too small.
                AddErrorMsg(msgs, offset, "remaining file space too small to hold segment");
                return ParseResult.Failure;
            }

            OmfSegment newSeg = new OmfSegment();
            newSeg.mFileData = data;
            newSeg.FileOffset = offset;

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
                    AddErrorMsg(msgs, offset, "invalid segment type " + data[offset + 0x0f]);
                    return ParseResult.Failure;
            }
            if (data.Length - offset < minLen) {
                // Too small for this version of the header.
                AddErrorMsg(msgs, offset, "remaining file space too small to hold " +
                    newSeg.Version + " segment");
                return ParseResult.Failure;
            }

            int blkByteCnt = RawData.GetWord(data, offset + 0x00, 4, false);
            newSeg.ResSpc = RawData.GetWord(data, offset + 0x04, 4, false);
            newSeg.Length = RawData.GetWord(data, offset + 0x08, 4, false);
            newSeg.LabLen = data[offset + 0x0d];
            int numLen = data[offset + 0x0e];
            newSeg.BankSize = RawData.GetWord(data, offset + 0x10, 4, false);
            int numSex, dispName;
            if (newSeg.Version == SegmentVersion.v0_0) {
                newSeg.Org = RawData.GetWord(data, offset + 0x14, 4, false);
                newSeg.Align = RawData.GetWord(data, offset + 0x18, 4, false);
                numSex = data[offset + 0x1c];
                // 7 unused bytes follow
                dispName = 0x24;
                if (newSeg.LabLen == 0) {
                    newSeg.DispData = dispName + data[offset + dispName];
                } else {
                    newSeg.DispData = dispName + LOAD_NAME_LEN;
                }
            } else {
                newSeg.BankSize = RawData.GetWord(data, offset + 0x10, 4, false);
                newSeg.Org = RawData.GetWord(data, offset + 0x18, 4, false);
                newSeg.Align = RawData.GetWord(data, offset + 0x1c, 4, false);
                numSex = data[offset + 0x20];
                newSeg.LcBank = data[offset + 0x21];    // v1.0 only
                newSeg.SegNum = RawData.GetWord(data, offset + 0x22, 2, false);
                newSeg.Entry = RawData.GetWord(data, offset + 0x24, 4, false);
                dispName = RawData.GetWord(data, offset + 0x28, 2, false);
                newSeg.DispData = RawData.GetWord(data, offset + 0x2a, 2, false);
            }

            // The only way to detect a v2.1 segment is by checking DISPNAME.
            if (newSeg.Version == SegmentVersion.v2_0 && dispName > 0x2c) {
                newSeg.Version = SegmentVersion.v2_1;
                expectedDispName += 4;

                if (data.Length - offset < minLen + 4) {
                    AddErrorMsg(msgs, offset, "remaining file space too small to hold " +
                        newSeg.Version + " segment");
                    return ParseResult.Failure;
                }
                newSeg.TempOrg = RawData.GetWord(data, offset + 0x2c, 4, false);
            }

            // Extract Kind and its attributes.  The Orca/M 2.0 manual refers to the 1-byte
            // field in v0/v1 as "TYPE" and the 2-byte field as "KIND", but we're generally
            // following the GS/OS reference nomenclature.
            int kindByte, kindWord;
            if (newSeg.Version <= SegmentVersion.v1_0) {
                kindByte = data[offset + 0x0c];
                if (!Enum.IsDefined(typeof(SegmentKind), kindByte & 0x1f)) {
                    // Example: Moria GS has a kind of $1F for its GLOBALS segment.
                    AddErrorMsg(msgs, offset, "invalid segment kind $" + kindByte.ToString("x2"));
                    return ParseResult.Failure;
                }
                newSeg.Kind = (SegmentKind)(kindByte & 0x1f);

                int kindAttrs = 0;
                if ((kindByte & 0x20) != 0) {
                    kindAttrs |= (int)SegmentAttribute.PosnIndep;
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
                    AddErrorMsg(msgs, offset, "invalid segment kind $" + kindWord.ToString("x4"));
                    return ParseResult.Failure;
                }
                newSeg.Kind = (SegmentKind)(kindWord & 0x001f);
                newSeg.Attrs = (SegmentAttribute)(kindWord & 0xff00);
            }

            // If we found a library dictionary segment, and we're not currently handling the
            // file as a library, reject this and try again.
            if (newSeg.Kind == SegmentKind.LibraryDict && !parseAsLibrary) {
                AddInfoMsg(msgs, offset, "found Library Dictionary segment, retrying as library");
                return ParseResult.IsLibrary;
            }

            // We've got the basic pieces.  Handle the block-vs-byte debacle.
            int segLen;
            bool asBlocks = false;
            if (newSeg.Version == SegmentVersion.v0_0) {
                // Always block count.
                segLen = blkByteCnt * DISK_BLOCK_SIZE;
                asBlocks = true;
            } else if (newSeg.Version >= SegmentVersion.v2_0) {
                // Always byte count.
                segLen = blkByteCnt;
            } else /*v1.0*/ {
                // Only Library files should treat the field as bytes.  We can eliminate Load
                // files by checking for a nonzero SegNum field, but there's no reliable way
                // to tell the difference between Object and Library while looking at a segment
                // in isolation.
                if (parseAsLibrary) {
                    segLen = blkByteCnt;
                } else {
                    segLen = blkByteCnt * DISK_BLOCK_SIZE;
                    asBlocks = true;
                }
            }
            newSeg.RawFileLength = newSeg.FileLength = segLen;

            //
            // Perform validity checks.  If any of these fail, we're probably reading something
            // that isn't OMF (or, if this isn't the first segment, we might have gone off the
            // rails at some point).
            //

            if (numLen != 4) {
                AddErrorMsg(msgs, offset, "NUMLEN must be 4, was " + numLen);
                return ParseResult.Failure;
            }
            if (numSex != 0) {
                AddErrorMsg(msgs, offset, "NUMSEX must be 0, was " + numSex);
                return ParseResult.Failure;
            }
            if (offset + segLen > data.Length) {
                if (asBlocks && offset + segLen - data.Length < DISK_BLOCK_SIZE) {
                    // I have found a few examples (e.g. BRIDGE.S16 in Davex v1.23, SYSTEM:START
                    // on an old Paintworks GS disk) where the file's length doesn't fill out
                    // the last block in the file.  If we continue, and the segment actually
                    // does pass EOF, we'll fail while reading the records.
                    AddInfoMsg(msgs, offset,
                        "file EOF is not a multiple of 512; last segment may be truncated");
                    newSeg.FileLength = data.Length - offset;
                } else {
                    // Segment is longer than the file.  (This can happen easily in a static lib if
                    // we're not parsing it as such.)
                    AddErrorMsg(msgs, offset, "segment file length exceeds EOF (segLen=" + segLen +
                        ", remaining=" + (data.Length - offset) + ")");
                    return ParseResult.Failure;
                }
            }
            if (dispName < expectedDispName || dispName > (segLen - LOAD_NAME_LEN)) {
                AddErrorMsg(msgs, offset, "invalid DISPNAME " + dispName + " (expected " +
                    expectedDispName + ", segLen=" + segLen + ")");
                return ParseResult.Failure;
            }
            if (newSeg.DispData < expectedDispName + LOAD_NAME_LEN ||
                    newSeg.DispData > (segLen - 1)) {
                AddErrorMsg(msgs, offset, "invalid DISPDATA " + newSeg.DispData + " (expected " +
                    (expectedDispName + LOAD_NAME_LEN) + ", segLen=" + segLen + ")");
                return ParseResult.Failure;
            }
            if (newSeg.BankSize > 0x00010000) {
                AddErrorMsg(msgs, offset, "invalid BANKSIZE $" + newSeg.BankSize.ToString("x"));
                return ParseResult.Failure;
            }
            if (newSeg.Align > 0x00010000) {
                AddErrorMsg(msgs, offset, "invalid ALIGN $" + newSeg.Align.ToString("x"));
                return ParseResult.Failure;
            }

            if (newSeg.BankSize != 0x00010000 && newSeg.BankSize != 0) {
                // This is fine, just a little weird.
                AddInfoMsg(msgs, offset, "unusual BANKSIZE $" + newSeg.BankSize.ToString("x6"));
            }
            if (newSeg.Align != 0 && newSeg.Align != 0x0100 && newSeg.Align != 0x00010000) {
                // Unexpected; the loader will round up.
                AddInfoMsg(msgs, offset, "unusual ALIGN $" + newSeg.Align.ToString("x6"));
            }
            if (newSeg.Entry != 0 && newSeg.Entry >= newSeg.Length) {
                // This is invalid, but if we got this far we might as well keep going.
                AddInfoMsg(msgs, offset, "invalid ENTRY $" + newSeg.Entry.ToString("x6"));
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
                    AddInfoMsg(msgs, offset, "var-width SEGNAME ran off end of segment (len=" +
                        segNameLen + ", segLen=" + segLen + ")");
                    return ParseResult.Failure;
                }
                segName = Encoding.ASCII.GetString(data, offset + segNameStart + 1, segNameLen);
            } else {
                // fixed-width string
                if (segNameStart + newSeg.LabLen > segLen) {
                    AddInfoMsg(msgs, offset, "fixed-width SEGNAME ran off end of segment (LABLEN=" +
                        newSeg.LabLen + ", segLen=" + segLen + ")");
                    return ParseResult.Failure;
                }
                segName = ExtractString(data, offset + segNameStart, newSeg.LabLen);
            }

            //AddInfoMsg(msgs, offset, "GOT LOADNAME='" + loadName + "' SEGNAME='" + segName + "'");

            newSeg.LoadName = loadName;
            newSeg.SegName = segName;

            //
            // Populate the "raw data" table.  We add the fields shown in the specification in
            // the order in which they appear.
            //

            if (newSeg.Version == SegmentVersion.v0_0 ||
                    (newSeg.Version == SegmentVersion.v1_0 && !parseAsLibrary)) {
                newSeg.AddRaw("BLKCNT", blkByteCnt, 4, "blocks");
            } else {
                newSeg.AddRaw("BYTECNT", blkByteCnt, 4, "bytes");
            }
            newSeg.AddRaw("RESSPC", newSeg.ResSpc, 4, string.Empty);
            newSeg.AddRaw("LENGTH", newSeg.Length, 4, string.Empty);
            if (newSeg.Version <= SegmentVersion.v1_0) {
                string attrStr = AttrsToString(newSeg.Attrs);
                if (!string.IsNullOrEmpty(attrStr)) {
                    attrStr = " -" + attrStr;
                }
                newSeg.AddRaw("KIND", data[offset+0x0c], 1,
                    KindToString(newSeg.Kind) + attrStr);
            } else {
                newSeg.AddRaw("undefined", data[offset + 0x0c], 1, string.Empty);
            }
            newSeg.AddRaw("LABLEN", newSeg.LabLen, 1,
                (newSeg.LabLen == 0 ? "variable length" : "fixed length"));
            newSeg.AddRaw("NUMLEN", numLen, 1, "must be 4");
            newSeg.AddRaw("VERSION", data[offset + 0x0f], 1, VersionToString(newSeg.Version));
            newSeg.AddRaw("BANKSIZE", newSeg.BankSize, 4, string.Empty);
            if (newSeg.Version >= SegmentVersion.v2_0) {
                string attrStr = AttrsToString(newSeg.Attrs);
                if (!string.IsNullOrEmpty(attrStr)) {
                    attrStr = " -" + attrStr;
                }
                newSeg.AddRaw("KIND", RawData.GetWord(data, offset + 0x14, 2, false), 2,
                    KindToString(newSeg.Kind) + attrStr);
                newSeg.AddRaw("undefined", RawData.GetWord(data, offset + 0x16, 2, false), 2,
                    string.Empty);
            } else {
                newSeg.AddRaw("undefined", RawData.GetWord(data, offset + 0x14, 4, false), 4,
                    string.Empty);
            }
            newSeg.AddRaw("ORG", newSeg.Org, 4, (newSeg.Org != 0 ? "" : "relocatable"));
            // alignment is rounded up to page/bank
            string alignStr;
            if (newSeg.Align == 0) {
                alignStr = "no alignment";
            } else if (newSeg.Align <= 0x0100) {
                alignStr = "align to page";
            } else {
                alignStr = "align to bank";
            }
            newSeg.AddRaw("ALIGN", newSeg.Align, 4, alignStr);
            newSeg.AddRaw("NUMSEX", numSex, 1, "must be 0");
            if (newSeg.Version == SegmentVersion.v1_0) {
                newSeg.AddRaw("LCBANK", newSeg.LcBank, 1, string.Empty);
            } else {
                newSeg.AddRaw("undefined", data[offset + 0x21], 1, string.Empty);
            }
            if (newSeg.Version >= SegmentVersion.v1_0) {
                newSeg.AddRaw("SEGNUM", newSeg.SegNum, 2, string.Empty);
                newSeg.AddRaw("ENTRY", newSeg.Entry, 4, string.Empty);
                newSeg.AddRaw("DISPNAME", dispName, 2, string.Empty);
                newSeg.AddRaw("DISPDATA", newSeg.DispData, 2, string.Empty);
                if (newSeg.Version >= SegmentVersion.v2_1) {
                    newSeg.AddRaw("TEMPORG", newSeg.TempOrg, 4, string.Empty);
                }
                newSeg.AddRaw("LOADNAME", loadName, 10, string.Empty);
            }
            newSeg.AddRaw("SEGNAME", segName, 0, string.Empty);

            segResult = newSeg;
            return ParseResult.Success;
        }

        public bool ParseBody(Formatter formatter, List<string> msgs) {
            int offset = FileOffset + DispData;
            while (true) {
                bool result = OmfRecord.ParseRecord(mFileData, offset, Version, LabLen,
                    formatter, msgs, out OmfRecord omfRec);
                if (!result) {
                    // Parsing failure.  Bail out.
                    return false;
                }
                if (offset + omfRec.Length > FileOffset + RawFileLength) {
                    // Overrun.
                    AddErrorMsg(msgs, offset, "record ran off end of file (" + omfRec + ")");
                    return false;
                }

                if (omfRec.Op == OmfRecord.Opcode.END) {
                    // v0/v1 pad to 512-byte block boundaries, so some slop is expected there,
                    // but v2.x should be snug.  Doesn't have to be, but might indicate a
                    // bug in the parser.
                    int remaining = (FileOffset + FileLength) - (offset + omfRec.Length);
                    Debug.Assert(remaining >= 0);
                    Debug.WriteLine("END record found, remaining space=" + remaining);
                    if (remaining >= DISK_BLOCK_SIZE ||
                            (Version >= SegmentVersion.v2_0 && remaining != 0)) {
                        AddInfoMsg(msgs, offset, "found " + remaining + " bytes past END record");
                    }
                    return true;
                }

                Records.Add(omfRec);
                offset += omfRec.Length;
            }
        }

        /// <summary>
        /// Tests to see whether the record collection is congruent with a Load file.
        /// </summary>
        public bool CheckRecords_LoadFile() {
            bool constSection = true;
            foreach (OmfRecord omfRec in Records) {
                switch (omfRec.Op) {
                    case OmfRecord.Opcode.LCONST:
                    case OmfRecord.Opcode.DS:
                        if (!constSection) {
                            Debug.WriteLine("Found LCONST/DS past const section");
                            return false;
                        }
                        break;
                    case OmfRecord.Opcode.RELOC:
                    case OmfRecord.Opcode.cRELOC:
                    case OmfRecord.Opcode.INTERSEG:
                    case OmfRecord.Opcode.cINTERSEG:
                    case OmfRecord.Opcode.SUPER:
                        constSection = false;
                        break;
                    default:
                        // incompatible record
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tests to see whether the record collection is congruent with an Object or Library file.
        /// </summary>
        public bool CheckRecords_ObjectOrLib() {
            foreach (OmfRecord omfRec in Records) {
                switch (omfRec.Op) {
                    case OmfRecord.Opcode.RELOC:
                    case OmfRecord.Opcode.cRELOC:
                    case OmfRecord.Opcode.INTERSEG:
                    case OmfRecord.Opcode.cINTERSEG:
                    case OmfRecord.Opcode.SUPER:
                    case OmfRecord.Opcode.ENTRY:
                        return false;
                    default:
                        break;
                }
            }
            return true;
        }

        public void GenerateRelocDict() {
            Debug.Assert(CheckRecords_LoadFile());

            foreach (OmfRecord omfRec in Records) {
                switch (omfRec.Op) {
                    case OmfRecord.Opcode.RELOC:
                    case OmfRecord.Opcode.cRELOC:
                    case OmfRecord.Opcode.INTERSEG:
                    case OmfRecord.Opcode.cINTERSEG:
                    case OmfRecord.Opcode.SUPER:
                        OmfReloc.GenerateRelocs(this, omfRec, mFileData, Relocs);
                        break;
                    default:
                        break;
                }
            }
        }

        private byte[] mConstData;

        /// <summary>
        /// Returns a reference to the unpacked constant data from the body of a Load segment
        /// (i.e. the LCONST/DS part).
        /// </summary>
        public byte[] GetConstData() {
            if (mConstData != null) {
                return mConstData;
            }

            // We haven't generated this yet; do it now.  Start by determining the length.
            int totalLen = 0;
            foreach (OmfRecord omfRec in Records) {
                if (omfRec.Op != OmfRecord.Opcode.LCONST && omfRec.Op != OmfRecord.Opcode.DS) {
                    break;
                }
                // safe to assume NUMLEN=4, NUMSEX=0
                totalLen += RawData.GetWord(mFileData, omfRec.FileOffset + 1, 4, false);
            }

            byte[] data = new byte[totalLen];

            int bufOffset = 0;
            foreach (OmfRecord omfRec in Records) {
                if (omfRec.Op == OmfRecord.Opcode.DS) {
                    int len = RawData.GetWord(mFileData, omfRec.FileOffset + 1, 4, false);
                    bufOffset += len;   // new buffers are zero-filled
                } else if (omfRec.Op == OmfRecord.Opcode.LCONST) {
                    int len = RawData.GetWord(mFileData, omfRec.FileOffset + 1, 4, false);
                    Array.Copy(mFileData, omfRec.FileOffset + 5, data, bufOffset, len);
                    bufOffset += len;
                } else {
                    break;
                }
            }

            Debug.Assert(bufOffset == totalLen);
            Debug.WriteLine("Generated " + totalLen + " bytes of LCONST/DS data for " + this);
            mConstData = data;
            return data;
        }

        //
        // Helper functions.
        //

        private void AddRaw(string name, object value, int width, string note) {
            if (value is byte) {
                value = (int)(byte)value;
            }
            RawValues.Add(new NameValueNote(name, value, width, note));
        }
        public static void AddInfoMsg(List<string> msgs, int offset, string msg) {
            msgs.Add("Note (+" + offset.ToString("x6") + "): " + msg);
        }
        public static void AddErrorMsg(List<string> msgs, int offset, string msg) {
            msgs.Add("Error (+" + offset.ToString("x6") + "): " + msg);
        }

        /// <summary>
        /// Extracts a fixed-length ASCII string, stopping early if a '\0' is encountered.
        /// </summary>
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

        /// <summary>
        /// Converts a segment version to a human-readable string.
        /// </summary>
        public static string VersionToString(SegmentVersion vers) {
            switch (vers) {
                case SegmentVersion.v0_0: return "v0.0";
                case SegmentVersion.v1_0: return "v1.0";
                case SegmentVersion.v2_0: return "v2.0";
                case SegmentVersion.v2_1: return "v2.1";
                default: return "v?.?";
            }
        }

        /// <summary>
        /// Converts a segment kind to a human-readable string.
        /// </summary>
        public static string KindToString(SegmentKind kind) {
            switch (kind) {
                case SegmentKind.Code:          return "Code";
                case SegmentKind.Data:          return "Data";
                case SegmentKind.JumpTable:     return "Jump Table";
                case SegmentKind.PathName:      return "Pathname";
                case SegmentKind.LibraryDict:   return "Library Dict";
                case SegmentKind.Init:          return "Init";
                case SegmentKind.AbsoluteBank:  return "Abs Bank";
                case SegmentKind.DpStack:       return "DP/Stack";
                default:                        return "???";
            }
        }

        public static string AttrsToString(SegmentAttribute attrs) {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 16; i++) {
                int bit = 1 << i;
                if (((int)attrs & bit) != 0) {
                    SegmentAttribute attr = (SegmentAttribute)bit;
                    sb.Append(' ');
                    sb.Append(attr.ToString());
                }
            }
            return sb.ToString();
        }


        public override string ToString() {
            return "[OmfSegment " + SegNum + " '" + LoadName + "' '" + SegName + "']";
        }
    }
}
