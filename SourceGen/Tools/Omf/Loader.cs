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
using System.IO;
using System.Text;

using Asm65;
using CommonUtil;

namespace SourceGen.Tools.Omf {
    /// <summary>
    /// Apple IIgs OMF loader.  This works like the GS/OS System Loader, reading the contents
    /// of an executable file and resolving the relocation records.  This only handles Load
    /// files, as Object and Library files contain unresolved references.
    /// </summary>
    public class Loader {
        private const string IIGS_SYSTEM_DEF = "Apple IIgs (GS/OS)";

        private OmfFile mOmfFile;
        private Formatter mFormatter;

        private byte[] mLoadedData;
        private DisasmProject mNewProject;

        private class SegmentMapEntry {
            public OmfSegment Segment { get; private set; }
            public int Address { get; private set; }

            public SegmentMapEntry(OmfSegment omfSeg, int address) {
                Segment = omfSeg;
                Address = address;
            }
        }
        private List<SegmentMapEntry> mSegmentMap;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="omfFile">OMF file to load.</param>
        public Loader(OmfFile omfFile, Formatter formatter) {
            Debug.Assert(omfFile.OmfFileKind == OmfFile.FileKind.Load);

            mOmfFile = omfFile;
            mFormatter = formatter;
        }

        /// <summary>
        /// Prepares the loaded form of the binary and the disassembly project.
        /// </summary>
        public bool Prepare() {
            if (!CreateMap()) {
                mSegmentMap = null;
                return false;
            }

            Debug.WriteLine("Segment map:");
            for (int i = 0; i < mSegmentMap.Count; i++) {
                SegmentMapEntry ent = mSegmentMap[i];
                if (ent == null) {
                    Debug.Assert(i == 0 || i == 1);     // initial hole and optional ~ExpressLoad
                    continue;
                }
                OmfSegment omfSeg = ent.Segment;
                Debug.WriteLine(i + " " + ent.Address.ToString("x6") + " SegNum=" + omfSeg.SegNum +
                    " '" + omfSeg.SegName + "'");

                Debug.Assert(i == ent.Segment.SegNum);
            }

            if (!GenerateOutput()) {
                mSegmentMap = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes the data and disasm project files.
        /// </summary>
        /// <param name="dataPathName"></param>
        /// <param name="projectPathName"></param>
        public bool WriteProjectFiles(string dataPathName, string projectPathName,
                out string errMsg) {
            Debug.WriteLine("Writing " + dataPathName + " and " + projectPathName);

            using (FileStream fs = new FileStream(dataPathName, FileMode.Create)) {
                fs.Write(mLoadedData, 0, mLoadedData.Length);
            }

            if (!ProjectFile.SerializeToFile(mNewProject, projectPathName, out errMsg)) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a map of file segments.  The position of each segment in the list will
        /// match the segment's position in the file, i.e. the segment in Map[5] will have
        /// SEGNUM==5.
        /// </summary>
        /// <remarks>
        /// I'm assuming that the SEGNUM in the file matches the position.  This seems to be
        /// the case everywhere.  ExpressLoad goes to some lengths to ensure this is still the
        /// case after a file is "expressed", including a remap table so that loader calls made
        /// by the application go to the right place (instead of, say, giving ~ExpressLoad a
        /// SEGNUM of 255.)
        /// </remarks>
        /// <returns>True on success.</returns>
        private bool CreateMap() {
            // Segments are numbered 1-N, so create a map with N+1 entries and leave first blank.
            mSegmentMap = new List<SegmentMapEntry>(mOmfFile.SegmentList.Count + 1);
            mSegmentMap.Add(null);

            // Create a bank in-use map.
            bool[] inUse = new bool[256];

            // Flag special memory as in-use.
            inUse[0x00] = inUse[0x01] = inUse[0xe0] = inUse[0xe1] = true;

            // Find segments that require specific addresses, and mark those banks as in use.
            foreach (OmfSegment omfSeg in mOmfFile.SegmentList) {
                if (omfSeg.Kind == OmfSegment.SegmentKind.DpStack) {
                    // This just allocates space in bank 0.
                    continue;
                }
                if (omfSeg.Length == 0) {
                    // Nothing to do here.
                    continue;
                }

                int addr;

                if (omfSeg.Org == 0) {
                    // The docs say that a value of zero always means relocatable, but that
                    // would mean you can't set the "absolute bank" flag to position code or
                    // data in bank 0.  I'm going to assume that's intentional, since people
                    // (a) shouldn't be doing that, and (b) can use DP/Stack instead (?).
                    continue;
                }

                addr = omfSeg.Org;
                if ((omfSeg.Attrs & OmfSegment.SegmentAttribute.AbsBank) != 0) {
                    // Bank is specified, rest of address is not.
                    addr &= 0x00ff0000;
                }

                // Mark the banks as being in use.  It's okay if multiple segments want the
                // same space.
                MarkBanks(addr, omfSeg.Length, inUse);
            }

            //
            // Assign segments to banks.  Note we always start at offset $0000 within a bank.
            //

            int nextBank = 0;
            int dpAddr = 0x1000;    // somewhat arbitrary
            foreach (OmfSegment omfSeg in mOmfFile.SegmentList) {
                if (omfSeg.Kind == OmfSegment.SegmentKind.DpStack || omfSeg.Length == 0) {
                    mSegmentMap.Add(new SegmentMapEntry(omfSeg, dpAddr));
                    dpAddr += omfSeg.Length;
                    if (dpAddr > 0x00010000) {
                        Debug.WriteLine("Stack/DP overflow");
                        return false;
                    }
                    continue;
                }
                if (omfSeg.IsExpressLoad) {
                    // We totally ignore these.  Add a null ref as a placeholder.
                    mSegmentMap.Add(null);
                    continue;
                }

                int addr;

                if (omfSeg.Org != 0) {
                    // Specific address requested.
                    addr = omfSeg.Org;
                    if ((omfSeg.Attrs & OmfSegment.SegmentAttribute.AbsBank) != 0) {
                        // just keep the bank
                        addr &= 0x00ff0000;
                    }
                } else {
                    // Find next available spot with enough space.
                    while (true) {
                        while (nextBank < 256 && inUse[nextBank]) {
                            nextBank++;
                        }
                        if (nextBank == 256) {
                            // Should be impossible on any sane Apple IIgs Load file.
                            Debug.Assert(false);
                            return false;
                        }
                        if (!CheckBanks(nextBank << 16, omfSeg.Length, inUse)) {
                            // Didn't fit in the space.
                            nextBank++;
                            continue;
                        }

                        // We only go forward, so no need to mark them.

                        break;
                    }

                    addr = nextBank << 16;

                    // Advance nextBank.  We do this by identifying the last address touched,
                    // and moving to the next bank.
                    int lastAddr = addr + omfSeg.Length - 1;
                    nextBank = (lastAddr >> 16) + 1;
                }

                SegmentMapEntry ent = new SegmentMapEntry(omfSeg, addr);
                mSegmentMap.Add(ent);
            }

            return true;
        }

        private static bool CheckBanks(int addr, int memLen, bool[] inUse) {
            Debug.Assert(memLen > 0);
            while (memLen > 0) {
                if (inUse[(addr >> 16) & 0xff]) {
                    return false;
                }
                addr += 65536;
                memLen -= 65536;
            }
            return true;

        }

        private static bool MarkBanks(int addr, int memLen, bool[] inUse) {
            Debug.Assert(memLen > 0);
            while (memLen > 0) {
                inUse[(addr >> 16) & 0xff] = true;
                addr += 65536;
                memLen -= 65536;
            }
            return true;
        }

        private bool GenerateOutput() {
            // Sum up the segment lengths to get the total project size.
            int totalLen = 0;
            foreach (SegmentMapEntry ent in mSegmentMap) {
                if (ent == null) {
                    continue;
                }
                totalLen += ent.Segment.Length;
            }
            Debug.WriteLine("Total length of loaded binary is " + totalLen);

            byte[] data = new byte[totalLen];

            // Create the project object.
            DisasmProject proj = new DisasmProject();
            proj.Initialize(data.Length);

            // Try to get the Apple IIgs system definition.  This is fragile, because it
            // relies on the name in the JSON file, but it's optional.  (If the default CPU
            // type stops being 65816, we should be sure to set that here.)
            try {
                // TODO(maybe): encapsulate this somewhere else
                string sysDefsPath = RuntimeDataAccess.GetPathName("SystemDefs.json");
                SystemDefSet sds = SystemDefSet.ReadFile(sysDefsPath);
                SystemDef sd = sds.FindEntryByName(IIGS_SYSTEM_DEF);
                if (sd != null) {
                    proj.ApplySystemDef(sd);
                } else {
                    Debug.WriteLine("Unable to find Apple IIgs system definition");
                }
            } catch (Exception) {
                // never mind
                Debug.WriteLine("Failed to apply Apple IIgs system definition");
            }

            ChangeSet cs = new ChangeSet(mSegmentMap.Count * 2);
            AddHeaderComment(proj, cs);

            // Load the segments, and add entries to the project.
            int bufOffset = 0;
            foreach (SegmentMapEntry ent in mSegmentMap) {
                if (ent == null) {
                    continue;
                }

                if (ent.Segment.Kind == OmfSegment.SegmentKind.JumpTable) {
                    if (!RelocJumpTable(ent, data, bufOffset, cs)) {
                        // Could treat this as non-fatal, but it really ought to work.
                        Debug.WriteLine("Jump Table reloc failed");
                        return false;
                    }
                } else {
                    // Perform relocation.
                    if (!RelocSegment(ent, data, bufOffset)) {
                        return false;
                    }
                }

                // Add one or more address entries.  (Normally one, but data segments
                // can straddle multiple pages.)
                AddAddressEntries(proj, ent, bufOffset, cs);

                // Add a comment identifying the segment and its attributes.
                string segCmt = string.Format(Res.Strings.OMF_SEG_COMMENT_FMT,
                    ent.Segment.SegNum, ent.Segment.Kind, ent.Segment.Attrs, ent.Segment.SegName);
                UndoableChange uc = UndoableChange.CreateLongCommentChange(bufOffset, null,
                    new MultiLineComment(segCmt));
                cs.Add(uc);

                // Add a note identifying the segment.
                string segNote = string.Format(Res.Strings.OMF_SEG_NOTE_FMT,
                    ent.Segment.SegNum, mFormatter.FormatAddress(ent.Address, true),
                    ent.Segment.SegName);
                uc = UndoableChange.CreateNoteChange(bufOffset, null,
                    new MultiLineComment(segNote));
                cs.Add(uc);

                bufOffset += ent.Segment.Length;
            }

            proj.PrepForNew(data, "new_proj");
            proj.ApplyChanges(cs, false, out RangeSet unused);

            mLoadedData = data;
            mNewProject = proj;
            return true;
        }

        private void AddHeaderComment(DisasmProject proj, ChangeSet cs) {
            // Add header comment.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format(Res.Strings.DEFAULT_HEADER_COMMENT_FMT,
                App.ProgramVersion));
            sb.AppendLine();
            foreach (SegmentMapEntry ent in mSegmentMap) {
                if (ent == null) {
                    continue;
                }
                string segCmt = string.Format(Res.Strings.OMF_SEG_HDR_COMMENT_FMT,
                    ent.Segment.SegNum, ent.Segment.Kind, ent.Segment.SegName,
                    mFormatter.FormatAddress(ent.Address, true));
                sb.AppendLine(segCmt);
            }

            UndoableChange uc = UndoableChange.CreateLongCommentChange(
                LineListGen.Line.HEADER_COMMENT_OFFSET,
                null, new MultiLineComment(sb.ToString()));
            cs.Add(uc);
        }

        /// <summary>
        /// Edits the data file, changing values based on the relocation dictionary.
        /// </summary>
        private bool RelocSegment(SegmentMapEntry ent, byte[] data, int bufOffset) {
            const int INVALID_ADDR = 0x00ffffff;

            byte[] srcData = ent.Segment.GetConstData();
            Array.Copy(srcData, 0, data, bufOffset, srcData.Length);

            foreach (OmfReloc omfRel in ent.Segment.Relocs) {
                int relocAddr = omfRel.RelOffset;
                if (omfRel.FileNum != -1 && omfRel.FileNum != 1) {
                    // Some other file; not much we can do with this.  Drop in an obviously
                    // invalid address and keep going.
                    Debug.WriteLine("Unable to process reloc with FileNum=" + omfRel.FileNum);
                    relocAddr = INVALID_ADDR;
                } else if (omfRel.SegNum == -1) {
                    // Within this segment.
                    relocAddr += ent.Address;
                } else {
                    // Find other segment.  This may fail if the file is damaged.
                    if (omfRel.SegNum < 0 || omfRel.SegNum >= mSegmentMap.Count ||
                            mSegmentMap[omfRel.SegNum] == null) {
                        // Can't find the segment.  Unlike the file case, this was expected to
                        // be something we could resolve with what we were given, so this is
                        // a hard failure.
                        Debug.WriteLine("Reloc SegNum=" + omfRel.SegNum + " not in map");
                        return false;
                    } else {
                        relocAddr += mSegmentMap[omfRel.SegNum].Address;
                    }
                }

                if (omfRel.Shift < 0) {
                    relocAddr >>= -omfRel.Shift;
                } else if (omfRel.Shift > 0) {
                    relocAddr <<= omfRel.Shift;
                }

                switch (omfRel.Width) {
                    case 1:
                        data[bufOffset + omfRel.Offset] = (byte)(relocAddr);
                        break;
                    case 2:
                        data[bufOffset + omfRel.Offset] = (byte)(relocAddr);
                        data[bufOffset + omfRel.Offset + 1] = (byte)(relocAddr >> 8);
                        break;
                    case 3:
                        data[bufOffset + omfRel.Offset] = (byte)(relocAddr);
                        data[bufOffset + omfRel.Offset + 1] = (byte)(relocAddr >> 8);
                        data[bufOffset + omfRel.Offset + 2] = (byte)(relocAddr >> 16);
                        break;
                    case 4:
                        data[bufOffset + omfRel.Offset] = (byte)(relocAddr);
                        data[bufOffset + omfRel.Offset + 1] = (byte)(relocAddr >> 8);
                        data[bufOffset + omfRel.Offset + 2] = (byte)(relocAddr >> 16);
                        data[bufOffset + omfRel.Offset + 3] = (byte)(relocAddr >> 24);
                        break;
                    default:
                        Debug.WriteLine("Invalid reloc width " + omfRel.Width);
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Edits the data file, essentially putting the jump table entries into the
        /// "loaded" state.
        /// </summary>
        private bool RelocJumpTable(SegmentMapEntry ent, byte[] data, int bufOffset,
                ChangeSet cs) {
            const int ENTRY_LEN = 14;

            byte[] srcData = ent.Segment.GetConstData();
            Array.Copy(srcData, 0, data, bufOffset, srcData.Length);

            // For no documented reason, jump tables start with 8 zero bytes.
            for (int i = 0; i < 8; i++) {
                if (data[bufOffset + i] != 0) {
                    Debug.WriteLine("JumpTab: missing 8-byte header");
                    return false;
                }
            }

            TypedRangeSet newSet = new TypedRangeSet();
            TypedRangeSet undoSet = new TypedRangeSet();

            for (int i = 8; i + 4 <= ent.Segment.Length; i += ENTRY_LEN) {
                int userId = RawData.GetWord(data, bufOffset + i, 2, false);
                int fileNum = RawData.GetWord(data, bufOffset + i + 2, 2, false);

                if (fileNum == 0) {
                    // A zero file number indicates end of table.
                    Debug.WriteLine("JumpTab: found fileNum=0 at offset " + i + ", len=" +
                        ent.Segment.Length);
                    break;
                } else if (fileNum != 1) {
                    // External file, ignore entry.
                    Debug.WriteLine("JumpTab: ignoring entry with FileNum=" + fileNum);
                    continue;
                } else if (i + ENTRY_LEN > ent.Segment.Length) {
                    // Make sure the rest fits.
                    Debug.WriteLine("JumpTab: overran buffer");
                    return false;
                }

                // Note: segment ends right after FileNum, so don't try to read further
                // until we've confirmed that FileNum != 0.

                int segNum = RawData.GetWord(data, bufOffset + i + 4, 2, false);
                int segOff = RawData.GetWord(data, bufOffset + i + 6, 4, false);

                if (segNum < 0 || segNum >= mSegmentMap.Count || mSegmentMap[segNum] == null) {
                    Debug.WriteLine("JumpTab: invalid SegNum=" + segNum);
                    return false;
                }
                if (data[bufOffset + i + 10] != 0x22) {
                    Debug.WriteLine("JumpTab: did not find expected JSL at off=" + i);
                    return false;
                }

                int addr = mSegmentMap[segNum].Address + segOff;
                int jmlOffset = bufOffset + i + 10;
                data[jmlOffset] = 0x5c;    // JML
                data[jmlOffset + 1] = (byte)addr;
                data[jmlOffset + 2] = (byte)(addr >> 8);
                data[jmlOffset + 3] = (byte)(addr >> 16);
                //Debug.WriteLine("JumpTab: off=" + i + " -> " +
                //    mFormatter.FormatAddress(addr, true));

                // It seems to be fairly common for jump table entries to not be referenced
                // from the program, which can leave whole dynamic segments unreferenced.  Set
                // a code hint on the JML instruction.
                undoSet.Add(jmlOffset, (int)CodeAnalysis.TypeHint.NoHint);
                newSet.Add(jmlOffset, (int)CodeAnalysis.TypeHint.Code);
            }

            UndoableChange uc = UndoableChange.CreateTypeHintChange(undoSet, newSet);
            cs.Add(uc);

            return true;
        }

        /// <summary>
        /// Adds one or more entries to the address map for the specified segment.
        /// </summary>
        private static void AddAddressEntries(DisasmProject proj, SegmentMapEntry ent,
                int bufOffset, ChangeSet cs) {
            int addr = ent.Address;
            int segRem = ent.Segment.Length;

            while (true) {
                // Generate an ORG directive.
                int origAddr = proj.AddrMap.Get(bufOffset);
                UndoableChange uc = UndoableChange.CreateAddressChange(bufOffset,
                    origAddr, addr);
                cs.Add(uc);

                // Compare amount of space in this bank to amount left in segment.
                int bankRem = 0x00010000 - (addr & 0xffff);
                if (bankRem > segRem) {
                    // All done, bail.
                    break;
                }

                // Advance to start of next bank.
                addr += bankRem;
                Debug.Assert((addr & 0x0000ffff) == 0);
                bufOffset += bankRem;
                segRem -= bankRem;
                Debug.WriteLine("Adding additional ORG at " + addr);
            }
        }
    }
}
