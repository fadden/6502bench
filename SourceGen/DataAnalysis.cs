/*
 * Copyright 2019 faddenSoft
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

using Asm65;
using CommonUtil;
using TextScanMode = SourceGen.ProjectProperties.AnalysisParameters.TextScanMode;

namespace SourceGen {
    /// <summary>
    /// Auto-detection of structured data.
    /// 
    /// This class doesn't really hold any state.  It's just a convenient place to collect
    /// the items needed by the analyzer methods.
    /// </summary>
    public class DataAnalysis {
        // Minimum number of consecutive identical bytes for something to be called a "run".
        private const int MIN_RUN_LENGTH = 5;

        // Minimum length for treating data as a run if the byte is a printable character.
        // (Alternatively, the maximum length of a character string composed of a single value.)
        // Anything shorter than this is handled with a string directive, anything this long or
        // longer becomes FILL.  This should be larger than the MinCharsForString parameter.
        private const int MAX_STRING_RUN_LENGTH = 62;

        // Absolute minimum string length for auto-detection.  This is used when generating the
        // data tables.
        public const int MIN_STRING_LENGTH = 3;

        // Minimum length for an ASCII string.  Anything shorter is just output as bytes.
        // This is the default value; the actual value is configured as a project preference.
        public const int DEFAULT_MIN_STRING_LENGTH = 4;

        // Set min chars to this to disable string detection.
        public const int MIN_CHARS_FOR_STRING_DISABLED = int.MaxValue;

        /// <summary>
        /// Project with which we are associated.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Reference to 65xx data.
        /// </summary>
        private byte[] mFileData;

        /// <summary>
        /// Attributes, one per byte in input file.
        /// </summary>
        private Anattrib[] mAnattribs;

        /// <summary>
        /// Configurable parameters.
        /// </summary>
        private ProjectProperties.AnalysisParameters mAnalysisParams;


        /// <summary>
        /// Debug trace log.
        /// </summary>
        private DebugLog mDebugLog = new DebugLog(DebugLog.Priority.Silent);
        public DebugLog DebugLog {
            set {
                mDebugLog = value;
            }
        }


        public DataAnalysis(DisasmProject proj, Anattrib[] anattribs) {
            mProject = proj;
            mAnattribs = anattribs;

            mFileData = proj.FileData;
            mAnalysisParams = proj.ProjectProps.AnalysisParams;
        }

        // Internal log functions. If we're concerned about performance overhead due to
        // call-site string concatenation, we can #ifdef these to nothing in release builds,
        // which should allow the compiler to elide the concat.
#if false
        private void LogV(int offset, string msg) {
            if (mDebugLog.IsLoggable(DebugLog.Priority.Verbose)) {
                mDebugLog.LogV("+" + offset.ToString("x6") + " " + msg);
            }
        }
#else
        private void LogV(int offset, string msg) { }
#endif
        private void LogD(int offset, string msg) {
            if (mDebugLog.IsLoggable(DebugLog.Priority.Debug)) {
                mDebugLog.LogD("+" + offset.ToString("x6") + " " + msg);
            }
        }
        private void LogI(int offset, string msg) {
            if (mDebugLog.IsLoggable(DebugLog.Priority.Info)) {
                mDebugLog.LogI("+" + offset.ToString("x6") + " " + msg);
            }
        }
        private void LogW(int offset, string msg) {
            if (mDebugLog.IsLoggable(DebugLog.Priority.Warning)) {
                mDebugLog.LogW("+" + offset.ToString("x6") + " " + msg);
            }
        }
        private void LogE(int offset, string msg) {
            if (mDebugLog.IsLoggable(DebugLog.Priority.Error)) {
                mDebugLog.LogE("+" + offset.ToString("x6") + " " + msg);
            }
        }

        /// <summary>
        /// Analyzes instruction operands and Address data descriptors to identify references
        /// to offsets within the file.
        /// 
        /// Instructions with format descriptors are left alone.  Instructions with
        /// operand offsets but no descriptor will have a descriptor generated
        /// using the label at the target offset; if the target offset is unlabeled,
        /// a unique label will be generated.  Data descriptors with type=Address are
        /// handled the same way.
        /// 
        /// In some cases, such as a reference to the middle of an instruction, we will
        /// label a nearby location instead.
        /// 
        /// This should be called after code analysis has run, user labels and format
        /// descriptors have been applied, and platform/project symbols have been merged
        /// into the symbol table.
        /// </summary>
        /// <returns>True on success.</returns>
        public void AnalyzeDataTargets() {
            mDebugLog.LogI("Analyzing data targets...");

            for (int offset = 0; offset < mAnattribs.Length; offset++) {
                Anattrib attr = mAnattribs[offset];
                if (attr.IsInstructionStart) {
                    if (attr.DataDescriptor != null) {
                        // It's being shown as numeric, or as a reference to some other symbol.
                        // Either way there's nothing further for us to do.  (Technically we
                        // would want to treat it like the no-descriptor case if the type was
                        // numeric/Address, but we don't allow that for instructions.)
                        //
                        // Project and platform symbols are applied later.
                        Debug.Assert(attr.DataDescriptor.FormatSubType !=
                            FormatDescriptor.SubType.Address);
                        continue;
                    }
                    int operandOffset = attr.OperandOffset;
                    if (operandOffset >= 0) {
                        // This is an offset reference: a branch or data access instruction whose
                        // target is inside the file.  Create a FormatDescriptor for it, and
                        // generate a label at the target if one is not already present.
                        SetDataTarget(offset, attr.Length, operandOffset);
                    }

                    // We advance by a single byte, rather than .Length, in case there's
                    // an instruction embedded inside another one.
                } else if (attr.DataDescriptor != null) {
                    // We can't check IsDataStart / IsInlineDataStart because the bytes might
                    // still be uncategorized.  If there's a user-specified format, check it
                    // to see if it's an address.
                    FormatDescriptor dfd = attr.DataDescriptor;

                    // Is this numeric/Address?
                    if ((dfd.FormatType == FormatDescriptor.Type.NumericLE ||
                            dfd.FormatType == FormatDescriptor.Type.NumericBE) &&
                            dfd.FormatSubType == FormatDescriptor.SubType.Address) {
                        // Treat like an absolute address.  Convert the operand
                        // to an address, then resolve the file offset.
                        int address = RawData.GetWord(mFileData, offset, dfd.Length,
                                (dfd.FormatType == FormatDescriptor.Type.NumericBE));
                        if (dfd.Length < 3) {
                            // Bank not specified by data, add current program bank.  Not always
                            // correct, but should be often enough.  In most cases we'd just
                            // assume a correct data bank register, but here we need to find
                            // a file offset, so we have to assume data bank == program bank
                            // (unless we find a good way to track the data bank register).
                            address |= attr.Address & 0x7fff0000;
                        }
                        int operandOffset = mProject.AddrMap.AddressToOffset(offset, address);
                        if (operandOffset >= 0) {
                            SetDataTarget(offset, dfd.Length, operandOffset);
                        }
                    }

                    // For other formats, we don't need to do anything.  Numeric/Address is
                    // the only one that represents an offset reference.  Numeric/Symbol
                    // is a name reference.  The others are just data.

                    // There shouldn't be any data items inside other data items, so we
                    // can just skip forward.
                    offset += mAnattribs[offset].DataDescriptor.Length - 1;
                }
            }
        }

        /// <summary>
        /// Extracts the operand offset from a data item.  Only useful for numeric/Address
        /// and numeric/Symbol.
        /// </summary>
        /// <param name="proj">Project reference.</param>
        /// <param name="offset">Offset of data item.</param>
        /// <returns>Operand offset, or -1 if not applicable.</returns>
        public static int GetDataOperandOffset(DisasmProject proj, int offset) {
            Anattrib attr = proj.GetAnattrib(offset);
            if (!attr.IsDataStart && !attr.IsInlineDataStart) {
                return -1;
            }
            FormatDescriptor dfd = attr.DataDescriptor;

            // Is this numeric/Address or numeric/Symbol?
            if ((dfd.FormatType != FormatDescriptor.Type.NumericLE &&
                    dfd.FormatType != FormatDescriptor.Type.NumericBE) ||
                    (dfd.FormatSubType != FormatDescriptor.SubType.Address &&
                    dfd.FormatSubType != FormatDescriptor.SubType.Symbol)) {
                return -1;
            }

            // Treat like an absolute address.  Convert the operand
            // to an address, then resolve the file offset.
            int address = RawData.GetWord(proj.FileData, offset, dfd.Length,
                    (dfd.FormatType == FormatDescriptor.Type.NumericBE));
            if (dfd.Length < 3) {
                // Add the program bank where the data bank should go.  Not perfect but
                // we don't have anything better at the moment.
                address |= attr.Address & 0x7fff0000;
            }
            int operandOffset = proj.AddrMap.AddressToOffset(offset, address);
            return operandOffset;
        }

        /// <summary>
        /// Returns the "base" operand offset.  If the byte at the specified offset is not the
        /// start of a code/data/inline-data item, walk backward until the start is found.
        /// </summary>
        /// <param name="proj">Project reference.</param>
        /// <param name="offset">Start offset.</param>
        /// <returns></returns>
        public static int GetBaseOperandOffset(DisasmProject proj, int offset) {
            Debug.Assert(offset >= 0 && offset < proj.FileDataLength);
            while (!proj.GetAnattrib(offset).IsStart) {
                offset--;

                // Should not be possible to walk off the top of the list, since we're in
                // the middle of something.
                Debug.Assert(offset >= 0);
            }
            return offset;
        }

        /// <summary>
        /// Creates a FormatDescriptor in the Anattrib array at srcOffset that links to
        /// targetOffset, or a nearby label.  If targetOffset doesn't have a useful label,
        /// one will be generated.
        /// 
        /// This is used for both instruction and data operands.
        /// </summary>
        /// <param name="srcOffset">Offset of instruction or address data.</param>
        /// <param name="srcLen">Length of instruction or data item.</param>
        /// <param name="targetOffset">Offset of target.</param>
        private void SetDataTarget(int srcOffset, int srcLen, int targetOffset) {
            // NOTE: don't try to cache mAnattribs[targetOffset] -- we may be changing
            // targetOffset and/or altering the Anattrib entry, so grabbing a copy of the
            // struct may lead to problems.

            // If the target offset has a symbol assigned, use it.  Otherwise, try to
            // find something nearby that might be more appropriate.
            int origTargetOffset = targetOffset;
            if (mAnattribs[targetOffset].Symbol == null) {
                if (mAnalysisParams.SeekNearbyTargets) {
                    targetOffset = FindAlternateTarget(srcOffset, targetOffset);
                }

                // If we're not interested in seeking nearby targets, or we are but we failed
                // to find something useful, we need to make sure that we're not pointing
                // into the middle of the instruction.  The assembler will only see labels on
                // the opcode bytes, so if we're pointing at the middle we need to back up.
                if (mAnattribs[targetOffset].IsInstruction &&
                        !mAnattribs[targetOffset].IsInstructionStart) {
                    while (!mAnattribs[--targetOffset].IsInstructionStart) {
                        // Should not be possible to move past the start of the file,
                        // since we know we're in the middle of an instruction.
                        Debug.Assert(targetOffset > 0);
                    }
                } else if (!mAnattribs[targetOffset].IsInstruction &&
                            !mAnattribs[targetOffset].IsStart) {
                    // This is not part of an instruction, and is not the start of a formatted
                    // data area.  However, it might be part of a formatted data area, in which
                    // case we need to avoid creating an auto label in the middle.  So we seek
                    // backward, looking for the first offset with a descriptor.  If that
                    // descriptor includes this offset, we set the target offset to that.
                    // (Note the uncategorized data pass hasn't run yet, so only instructions
                    // and offsets identified by users or scripts have been categorized.)
                    int scanOffset = targetOffset;
                    while (--scanOffset >= 0) {
                        FormatDescriptor dfd = mAnattribs[scanOffset].DataDescriptor;
                        if (dfd != null) {
                            if (scanOffset + dfd.Length > targetOffset) {
                                // Found a descriptor that encompasses target offset.  Adjust
                                // target to point at the start of the region.
                                targetOffset = scanOffset;
                            }
                            // Descriptors aren't allowed to overlap, so either way we're done.
                            break;
                        }
                    }
                }
            }

            if (mAnattribs[targetOffset].Symbol == null) {
                // No label at target offset, generate one.
                //
                // Generally speaking, the label we generate will be unique, because it
                // incorporates the address.  It's possible through various means to end
                // up with a user or platform label that matches an auto label, so we
                // need to do some renaming in that case.  Shouldn't happen often.
                Symbol sym = AutoLabel.GenerateUniqueForAddress(mAnattribs[targetOffset].Address,
                    mProject.SymbolTable, "L");
                mAnattribs[targetOffset].Symbol = sym;
                // This will throw if the symbol already exists.  That is the desired
                // behavior, as that would be a bug.
                mProject.SymbolTable.Add(sym);
            }

            // Create a Numeric/Symbol descriptor that references the target label.  If the
            // source offset already had a descriptor (e.g. Numeric/Address data item),
            // this will replace it in the Anattrib array.  (The user-specified format
            // is unaffected.)
            //
            // Doing this by target symbol, rather than offset in a Numeric/Address item,
            // allows us to avoid carrying the adjustment stuff everywhere.  OTOH we have
            // to manually refactor label renames in the display list if we don't want to
            // redo the data analysis.
            bool isBigEndian = false;
            if (mAnattribs[srcOffset].DataDescriptor != null) {
                LogD(srcOffset, "Replacing " + mAnattribs[srcOffset].DataDescriptor +
                    " with reference to " + mAnattribs[targetOffset].Symbol.Label +
                    ", adj=" + (origTargetOffset - targetOffset));
                if (mAnattribs[srcOffset].DataDescriptor.FormatType ==
                        FormatDescriptor.Type.NumericBE) {
                    isBigEndian = true;
                }
            } else {
                LogV(srcOffset, "Creating weak reference to label " +
                    mAnattribs[targetOffset].Symbol.Label +
                    ", adj=" + (origTargetOffset - targetOffset));
            }
            mAnattribs[srcOffset].DataDescriptor = FormatDescriptor.Create(srcLen,
                new WeakSymbolRef(mAnattribs[targetOffset].Symbol.Label, WeakSymbolRef.Part.Low),
                isBigEndian);
        }

        /// <summary>
        /// Given a reference from srcOffset to targetOffset, check to see if there's a
        /// nearby location that we'd prefer to refer to.  For example, if targetOffset points
        /// into the middle of an instruction, we'd rather have it refer to the first byte.
        /// </summary>
        /// <param name="srcOffset">Reference source.</param>
        /// <param name="targetOffset">Reference target.</param>
        /// <returns>New value for targetOffset, or original value if nothing better was
        ///   found.</returns>
        private int FindAlternateTarget(int srcOffset, int targetOffset) {
            int origTargetOffset = targetOffset;

            // Is the target outside the instruction stream?  If it's just referencing data,
            // do a simple check and move on.
            if (!mAnattribs[targetOffset].IsInstruction) {
                // We want to use user-defined labels whenever possible.  If they're accessing
                // memory within a few bytes, use that.  We don't want to do this for
                // code references, though, or our branches will get all weird.
                // TODO(someday): make MAX user-configurable?  Seek forward as well as backward?
                const int MAX = 4;
                for (int probeOffset = targetOffset - 1;
                        probeOffset >= 0 && probeOffset != targetOffset - MAX; probeOffset--) {
                    Symbol sym = mAnattribs[probeOffset].Symbol;
                    if (sym != null && sym.SymbolSource == Symbol.Source.User) {
                        // Found a nearby user label.  Make sure it's actually nearby.
                        int addrDiff = mAnattribs[targetOffset].Address -
                            mAnattribs[probeOffset].Address;
                        if (addrDiff == targetOffset - probeOffset) {
                            targetOffset = probeOffset;
                        } else {
                            Debug.WriteLine("NOT probing past address boundary change");
                        }
                        break;
                    }
                }
                return targetOffset;
            }

            // Target is an instruction.  Is the source an instruction or data element
            // (e.g. ".dd2 <addr>").
            if (!mAnattribs[srcOffset].IsInstructionStart) {
                // Might be address-1 to set up an RTS.  If the target address isn't
                // an instruction start, check to see if the following byte is.
                if (!mAnattribs[targetOffset].IsInstructionStart &&
                        targetOffset + 1 < mAnattribs.Length &&
                        mAnattribs[targetOffset + 1].IsInstructionStart) {
                    LogD(srcOffset, "Offsetting address reference");
                    targetOffset++;
                }
                return targetOffset;
            }

            // Source is an instruction, so we have an instruction referencing an instruction.
            // Could be a branch, an address push, or self-modifying code.
            OpDef op = mProject.CpuDef.GetOpDef(mProject.FileData[srcOffset]);
            if (op.IsBranchOrSubCall) {
                // Don't mess with jumps and branches -- always go directly to the
                // target address.
            } else if (op == OpDef.OpPEA_StackAbs || op == OpDef.OpPER_StackPCRelLong) {
                // They might be pushing address-1 to set up an RTS.  If the target address isn't
                // an instruction start, check to see if the following byte is.
                if (!mAnattribs[targetOffset].IsInstructionStart &&
                        targetOffset + 1 < mAnattribs.Length &&
                        mAnattribs[targetOffset + 1].IsInstructionStart) {
                    LogD(srcOffset, "Offsetting PEA/PER");
                    targetOffset++;
                }
            } else {
                // Data operation (LDA, STA, etc).  This could be self-modifying code, or
                // an indexed access with an offset base address (LDA addr-1,Y) to an
                // adjacent data area.  Check to see if there's data right after this.
                bool nearbyData = false;
                for (int i = targetOffset + 1; i <= targetOffset + 2; i++) {
                    if (i < mAnattribs.Length && !mAnattribs[i].IsInstruction) {
                        targetOffset = i;
                        nearbyData = true;
                        break;
                    }
                }
                if (!nearbyData && !mAnattribs[targetOffset].IsInstructionStart) {
                    // There's no data nearby, and the target is not the start of the
                    // instruction, so this is probably self-modifying code.  We want
                    // the label to be on the opcode, so back up to the instruction start.
                    while (!mAnattribs[--targetOffset].IsInstructionStart) {
                        // Should not be possible to move past the start of the file,
                        // since we know we're in the middle of an instruction.
                        Debug.Assert(targetOffset > 0);
                    }
                }
            }

            if (targetOffset != origTargetOffset) {
                LogV(srcOffset, "Creating instruction ref adj=" +
                    (origTargetOffset - targetOffset));
            }

            return targetOffset;
        }

        /// <summary>
        /// Analyzes uncategorized regions of the file to see if they fit common patterns.
        /// 
        /// This is re-run after most changes to the project, so we don't want to do anything
        /// crazily expensive.
        /// </summary>
        /// <returns>True on success.</returns>
        public void AnalyzeUncategorized() {
            FormatDescriptor oneByteDefault = FormatDescriptor.Create(1,
                FormatDescriptor.Type.Default, FormatDescriptor.SubType.None);
            FormatDescriptor.DebugPrefabBump(-1);

            // If it hasn't been identified as code or data, set the "data" flag to
            // give it a positive identification as data.  (This should be the only
            // place outside of CodeAnalysis that sets this flag.)  This isn't strictly
            // necessary, but it helps us assert things when pieces start moving around.
            for (int offset = 0; offset < mAnattribs.Length; offset++) {
                Anattrib attr = mAnattribs[offset];
                if (attr.IsInlineData) {
                    // While we're here, add a default format descriptor for inline data
                    // that doesn't have one.  We don't try to analyze it otherwise.
                    if (attr.DataDescriptor == null) {
                        mAnattribs[offset].DataDescriptor = oneByteDefault;
                        FormatDescriptor.DebugPrefabBump();
                    }
                } else if (!attr.IsInstruction) {
                    mAnattribs[offset].IsData = true;
                }
            }

            mDebugLog.LogI("Analyzing uncategorized data...");

            int startOffset = -1;
            for (int offset = 0; offset < mAnattribs.Length; ) {
                // We want to find a contiguous series of offsets which are not known
                // to hold code or data.  We stop if we encounter a user-defined label
                // or format descriptor.
                Anattrib attr = mAnattribs[offset];

                if (attr.IsInstruction || attr.IsInlineData || attr.IsDataStart) {
                    // Instruction, inline data, or formatted data known to be here.  Analyze
                    // previous chunk, then advance past this.
                    if (startOffset >= 0) {
                        AnalyzeRange(startOffset, offset - 1);
                        startOffset = -1;
                    }
                    if (attr.IsInstruction) {
                        // Because of embedded instructions, we can't simply leap forward.
                        // [or can we?]
                        offset++;
                    } else {
                        Debug.Assert(attr.Length > 0);
                        offset += attr.Length;
                    }
                } else if (attr.Symbol != null || mProject.HasCommentOrNote(offset)) {
                    // In an uncategorized area, but we want to break at this byte
                    // so the user or auto label doesn't get buried in the middle of
                    // a large chunk.
                    //
                    // This is similar to, but independent of, GroupedOffsetSetFromSelected()
                    // in ProjectView.  This is for auto-detection, the other is for user
                    // selection.  It's best if the two behave similarly though.
                    if (startOffset >= 0) {
                        AnalyzeRange(startOffset, offset - 1);
                    }
                    startOffset = offset;
                    offset++;
                } else {
                    // This offset is uncategorized, keep gathering.
                    if (startOffset < 0) {
                        startOffset = offset;
                    }
                    offset++;

                    // Check to see if the address has changed from the previous entry.
                    if (offset < mAnattribs.Length &&
                            mAnattribs[offset-1].Address + 1 != mAnattribs[offset].Address) {
                        // Must be an ORG here.  Scan previous region.
                        AnalyzeRange(startOffset, offset - 1);
                        startOffset = -1;
                    }
                }
            }
            if (startOffset >= 0) {
                AnalyzeRange(startOffset, mAnattribs.Length - 1);
            }
        }

        /// <summary>
        /// Analyzes a range of bytes, looking for opportunities to promote uncategorized
        /// data to a more structured form.
        /// </summary>
        /// <param name="start">Offset of first byte in range.</param>
        /// <param name="end">Offset of last byte in range.</param>
        private void AnalyzeRange(int start, int end) {
            // We want to identify runs of identical bytes, and runs of more than N human-
            // readable characters (ASCII, high ASCII, PETSCII, whatever).  There are a few
            // ways to do this.
            //
            // The simple approach is to walk through the data from start to end, checking at
            // each offset for runs of bytes matching the criteria.  Because the data doesn't
            // change, we can pre-analyze the data at project load time to speed things up.
            //
            // One approach is to put runs into TypedRangeSet (setting the type to the byte
            // value so a run of 0x00 doesn't merge into an adjacent run of 0x01), and the
            // various character encodings into individual RangeSets.  Then, for any given
            // byte address, you can query the length of a potential run directly.  This could
            // be made faster with a mergesort-like algorithm that walked through the various
            // range sets, rather than iterating over every byte in the range.  However, the
            // ranges passed into this method tend to be small, so the initial setup time for
            // each region can dominate the performance.  (The optimized implementation of this
            // approach is also fairly complicated.)
            //
            // A memory-hungry alternative is to create arrays of integers, one entry per byte
            // in the file, and set each entry to the number of bytes in the run that would
            // follow at that point.  So if a run of 20 zeroes began at off set 5, you would
            // set run[5]=20, run[6]=19, and so on.  That avoids searching in the sets, at the
            // cost of potentially several megabytes for a large 65816 file.
            //
            // It's even possible that Regex would handle this faster and more easily.  This
            // can be done fairly quickly with "unsafe" code, e.g.:
            //   https://stackoverflow.com/questions/3028768/net-regular-expressions-on-bytes-instead-of-chars
            //   https://stackoverflow.com/questions/1660694/regular-expression-to-match-any-character-being-repeated-more-than-10-times
            //
            // Ultimately we're just not spending that much time here.  Setting
            // AnalyzeUncategorizedData=false reveals that most of the time is spent in
            // the caller, identifying the regions, so a significant improvement here won't
            // have much impact on the user experience.
            //
            // Vague idea: figure out how to re-use the results from the previous analysis
            // pass.  At a superficial level we can cache the result of calling here with a
            // particular (start, end) pair.  At a higher level we may be able to avoid
            // the search for uncategorized data, certainly at the bank level, possibly within
            // a bank.

            mDebugLog.LogI("Analyzing [+" + start.ToString("x6") + ",+" + end.ToString("x6") +"]");

            FormatDescriptor oneByteDefault = FormatDescriptor.Create(1,
                        FormatDescriptor.Type.Default, FormatDescriptor.SubType.None);
            FormatDescriptor.DebugPrefabBump(-1);
            if (!mAnalysisParams.AnalyzeUncategorizedData) {
                // Analysis is disabled, so just mark everything as single-byte data.
                while (start <= end) {
                    mAnattribs[start].DataDescriptor = oneByteDefault;
                    FormatDescriptor.DebugPrefabBump();
                    start++;
                }
                return;
            }

            int minStringChars = mAnalysisParams.MinCharsForString;

#if DATA_PRESCAN   // this is actually slower (and uses more memory)
            while (start <= end) {
                // This is used to let us skip forward.  It starts past the end of the block,
                // and moves backward as we identify potential points of interest.
                int minNextStart = end + 1;

                bool found = mProject.RepeatedBytes.GetContainingOrSubsequentRange(start,
                        out TypedRangeSet.TypedRange tyRange);
                if (found) {
                    if (tyRange.Low <= start) {
                        // found a matching range
                        Debug.Assert(tyRange.Low <= start && tyRange.High >= start);
                        int clampEnd = Math.Min(tyRange.High, end);
                        int repLen = clampEnd - start + 1;
                        if (repLen >= MIN_RUN_LENGTH) {
                            bool isAscii =
                                TextUtil.IsPrintableAscii((char)(mFileData[start] & 0x7f));

                            // IF the run isn't ASCII, OR it's so long that we don't want to
                            // encode it as a string, OR it's so short that we don't want to
                            // treat it as a string, THEN output it as a run.  Otherwise, just
                            // let the ASCII-catcher handle it later.
                            if (!isAscii ||
                                    repLen > MIN_RUN_LENGTH_ASCII || repLen < minStringChars) {
                                LogV(start, "Run of 0x" + mFileData[start].ToString("x2") + ": " +
                                    repLen + " bytes");
                                mAnattribs[start].DataDescriptor = FormatDescriptor.Create(
                                    repLen, FormatDescriptor.Type.Fill,
                                    FormatDescriptor.SubType.None);
                                start += repLen;
                                continue;
                            }
                        }
                        // We didn't like this range.  We probably won't like it for any other
                        // point within the range, so start again past it.  Ideally we'd use
                        // Range.Low of the range that followed the one that was returned, but
                        // we don't have that handy.
                        minNextStart = Math.Min(minNextStart, tyRange.High + 1);
                    } else {
                        // no match; try to advance to the start of the next range.
                        Debug.Assert(tyRange.Low > start);
                        minNextStart = Math.Min(minNextStart, tyRange.Low);
                    }
                }

                found = mProject.StdAsciiBytes.GetContainingOrSubsequentRange(start,
                        out RangeSet.Range range);
                if (found) {
                    if (range.Low <= start) {
                        // found a matching range
                        Debug.Assert(range.Low <= start && range.High >= start);
                        int clampEnd = Math.Min(range.High, end);
                        int repLen = clampEnd - start + 1;
                        if (repLen >= minStringChars) {
                            LogV(start, "Std ASCII string, len=" + repLen + " bytes");
                            mAnattribs[start].DataDescriptor = FormatDescriptor.Create(repLen,
                                FormatDescriptor.Type.String, FormatDescriptor.SubType.None);
                            start += repLen;
                            continue;
                        }

                        minNextStart = Math.Min(minNextStart, range.High + 1);
                    } else {
                        Debug.Assert(range.Low > start);
                        minNextStart = Math.Min(minNextStart, range.Low);
                    }
                }

                found = mProject.HighAsciiBytes.GetContainingOrSubsequentRange(start,
                        out range);
                if (found) {
                    if (range.Low <= start) {
                        // found a matching range
                        Debug.Assert(range.Low <= start && range.High >= start);
                        int clampEnd = Math.Min(range.High, end);
                        int repLen = clampEnd - start + 1;
                        if (repLen >= minStringChars) {
                            LogV(start, "High ASCII string, len=" + repLen + " bytes");
                            mAnattribs[start].DataDescriptor = FormatDescriptor.Create(repLen,
                                FormatDescriptor.Type.String, FormatDescriptor.SubType.None);
                            start += repLen;
                            continue;
                        }

                        minNextStart = Math.Min(minNextStart, range.High + 1);
                    } else {
                        Debug.Assert(range.Low > start);
                        minNextStart = Math.Min(minNextStart, range.Low);
                    }
                }

                // Advance to the next possible run location.
                int nextStart = minNextStart > 0 ? minNextStart : start + 1;
                Debug.Assert(nextStart > start);

                // No runs found, output as single bytes.  This is the easiest form for users
                // to edit.
                while (start < nextStart) {
                    mAnattribs[start].DataDescriptor = oneByteDefault;
                    FormatDescriptor.DebugPrefabBump();
                    start++;
                }
            }
#else
            // Select "is printable" test.  We use the extended version to include some
            // control characters.
            // TODO(maybe): require some *actually* printable characters in each string
            CharEncoding.InclusionTest testPrintable;
            FormatDescriptor.SubType baseSubType;
            switch (mAnalysisParams.DefaultTextScanMode) {
                case TextScanMode.LowAscii:
                    testPrintable = CharEncoding.IsExtendedAscii;
                    baseSubType = FormatDescriptor.SubType.Ascii;
                    break;
                case TextScanMode.LowHighAscii:
                    testPrintable = CharEncoding.IsExtendedLowOrHighAscii;
                    baseSubType = FormatDescriptor.SubType.ASCII_GENERIC;
                    break;
                case TextScanMode.C64Petscii:
                    testPrintable = CharEncoding.IsExtendedC64Petscii;
                    baseSubType = FormatDescriptor.SubType.C64Petscii;
                    break;
                case TextScanMode.C64ScreenCode:
                    testPrintable = CharEncoding.IsExtendedC64ScreenCode;
                    baseSubType = FormatDescriptor.SubType.C64Screen;
                    break;
                default:
                    Debug.Assert(false);
                    testPrintable = CharEncoding.IsExtendedLowOrHighAscii;
                    baseSubType = FormatDescriptor.SubType.ASCII_GENERIC;
                    break;
            }

            while (start <= end) {
                // Check for block of repeated values.
                int runLen = RecognizeRun(mFileData, start, end);
                int printLen = 0;
                FormatDescriptor.SubType subType = baseSubType;

                if (testPrintable(mFileData[start])) {
                    // The run byte is printable, and the run is shorter than a line.  It's
                    // possible the run is followed by additional printable characters, e.g.
                    // "*****hello".  Text is easier for humans to understand, so we prefer
                    // that unless the run is longer than one line.
                    if (runLen <= MAX_STRING_RUN_LENGTH) {
                        // See if the run is followed by additional printable characters.
                        printLen = runLen;

                        // For LowHighAscii we allow a string to be either low or high, but it
                        // must be entirely one thing.  Refine our test.
                        CharEncoding.InclusionTest refinedTest = testPrintable;
                        if (mAnalysisParams.DefaultTextScanMode == TextScanMode.LowHighAscii) {
                            if (CharEncoding.IsExtendedAscii(mFileData[start])) {
                                refinedTest = CharEncoding.IsExtendedAscii;
                                subType = FormatDescriptor.SubType.Ascii;
                            } else {
                                refinedTest = CharEncoding.IsExtendedHighAscii;
                                subType = FormatDescriptor.SubType.HighAscii;
                            }
                        }
                        for (int i = start + runLen; i <= end; i++) {
                            if (!refinedTest(mFileData[i])) {
                                break;
                            }
                            printLen++;
                        }
                    }
                }

                if (printLen >= minStringChars) {
                    // This either a short run followed by printable characters, or just a
                    // (possibly very large) bunch of printable characters.
                    Debug.Assert(subType != FormatDescriptor.SubType.ASCII_GENERIC);
                    LogD(start, "Character string (" + subType + "), len=" + printLen + " bytes");
                    mAnattribs[start].DataDescriptor = FormatDescriptor.Create(printLen,
                        FormatDescriptor.Type.StringGeneric, subType);
                    start += printLen;
                } else if (runLen >= MIN_RUN_LENGTH) {
                    // Didn't qualify as a string, but it's long enough to be a run.
                    //
                    // TODO(someday): allow .fill pseudo-ops to have character encoding
                    //   sub-types, so we can ".fill 64,'*'".  Easy to do here, but
                    //   proper treatment requires tweaking data operand editor to allow
                    //   char encoding to be specified.
                    LogV(start, "Run of 0x" + mFileData[start].ToString("x2") + ": " +
                        runLen + " bytes");
                    mAnattribs[start].DataDescriptor = FormatDescriptor.Create(
                        runLen, FormatDescriptor.Type.Fill,
                        FormatDescriptor.SubType.None);
                    start += runLen;
                } else {
                    // Nothing useful found, output 1+ values as single bytes.  This is the
                    // easiest form for users to edit.  If we found a run, but it was too short,
                    // we can go ahead and mark all bytes in the run because we know the later
                    // matches will also be too short.
                    Debug.Assert(runLen > 0);
                    while (runLen-- != 0) {
                        mAnattribs[start++].DataDescriptor = oneByteDefault;
                        FormatDescriptor.DebugPrefabBump();
                    }
                }
            }
#endif
        }

        #region Static analyzer methods

        /// <summary>
        /// Checks for a repeated run of the same byte.
        /// </summary>
        /// <param name="fileData">Raw data.</param>
        /// <param name="start">Offset of first byte in range.</param>
        /// <param name="end">Offset of last byte in range.</param>
        /// <returns>Length of run.</returns>
        public static int RecognizeRun(byte[] fileData, int start, int end) {
            byte first = fileData[start];
            int index = start;
            while (++index <= end) {
                if (fileData[index] != first) {
                    break;
                }
            }
            return index - start;
        }

        /// <summary>
        /// Counts the number of low-ASCII, high-ASCII, and non-ASCII values in the
        /// specified region.
        /// </summary>
        /// <param name="fileData">Raw data.</param>
        /// <param name="start">Offset of first byte in range.</param>
        /// <param name="end">Offset of last byte in range</param>
        /// <param name="charTest">Character test delegate.  Must match on both high and
        ///   low characters.</param>
        /// <param name="lowVal">Set to the number of low-range characters found.</param>
        /// <param name="highVal">Set to the number of high-range characters found.</param>
        /// <param name="nonChar">Set to the number of non-character bytes found.</param>
        public static void CountHighLowBytes(byte[] fileData, int start, int end,
                CharEncoding.InclusionTest charTest,
                out int lowVal, out int highVal, out int nonChar) {
            lowVal = highVal = nonChar = 0;

            for (int i = start; i <= end; i++) {
                byte val = fileData[i];
                if (!charTest(val)) {
                    nonChar++;
                } else if ((val & 0x80) == 0) {
                    lowVal++;
                } else {
                    highVal++;
                }
            }
        }

        /// <summary>
        /// Counts the number of bytes that match the character test.
        /// </summary>
        /// <param name="fileData">Raw data.</param>
        /// <param name="start">Offset of first byte in range.</param>
        /// <param name="end">Offset of last byte in range.</param>
        /// <param name="charTest">Character test delegate.</param>
        /// <returns>Number of matching characters.</returns>
        public static int CountCharacterBytes(byte[] fileData, int start, int end,
                CharEncoding.InclusionTest charTest) {
            int count = 0;
            for (int i = start; i <= end; i++) {
                if (charTest(fileData[i])) {
                    count++;
                }
            }
            return count;
        }

        /// <summary>
        /// Counts the number of null-terminated strings in the buffer.
        /// 
        /// Zero-length strings are allowed but not included in the count.
        /// 
        /// If any bad data is found, the scan aborts and returns -1.
        /// </summary>
        /// <param name="fileData">Raw data.</param>
        /// <param name="start">Offset of first byte in range.</param>
        /// <param name="end">Offset of last byte in range.</param>
        /// <param name="charTest">Character test delegate.</param>
        /// <param name="limitHiBit">If set, the high bit in all character must be the
        ///   same.  Used to enforce a single encoding when "low or high ASCII" is used.</param>
        /// <returns>Number of strings found, or -1 if bad data identified.</returns>
        public static int RecognizeNullTerminatedStrings(byte[] fileData, int start, int end,
                CharEncoding.InclusionTest charTest, bool limitHiBit) {
            // Quick test.
            if (fileData[end] != 0x00) {
                return -1;
            }

            int stringCount = 0;
            int expectedHiBit = -1;
            int stringLen = 0;
            for (int i = start; i <= end; i++) {
                byte val = fileData[i];
                if (val == 0x00) {
                    // End of string.  Only update count if string wasn't empty.
                    if (stringLen != 0) {
                        stringCount++;
                    }
                    stringLen = 0;
                    expectedHiBit = -1;
                } else {
                    if (limitHiBit) {
                        if (expectedHiBit == -1) {
                            // First byte in string, set hi/lo expectation.
                            expectedHiBit = val & 0x80;
                        } else if ((val & 0x80) != expectedHiBit) {
                            // Mixed ASCII or non-ASCII, fail.
                            return -1;
                        }
                    }
                    if (!charTest(val)) {
                        // Not a matching character, fail.
                        return -1;
                    }
                    stringLen++;
                }
            }

            return stringCount;
        }

        /// <summary>
        /// Counts strings prefixed with an 8-bit length.
        ///
        /// Zero-length strings are allowed but not counted.
        /// </summary>
        /// <param name="fileData">Raw data.</param>
        /// <param name="start">Offset of first byte in range.</param>
        /// <param name="end">Offset of last byte in range.</param>
        /// <param name="charTest">Character test delegate.</param>
        /// <param name="limitHiBit">If set, the high bit in all character must be the
        ///   same.  Used to enforce a single encoding when "low or high ASCII" is used.</param>
        /// <returns>Number of strings found, or -1 if bad data identified.</returns>
        public static int RecognizeLen8Strings(byte[] fileData, int start, int end,
                CharEncoding.InclusionTest charTest, bool limitHiBit) {
            int posn = start;
            int remaining = end - start + 1;
            int stringCount = 0;

            while (remaining > 0) {
                int strLen = fileData[posn++];
                if (strLen > --remaining) {
                    // Buffer doesn't hold entire string, fail.
                    return -1;
                }

                if (strLen == 0) {
                    continue;
                }
                stringCount++;
                remaining -= strLen;

                int expectedHiBit = fileData[posn] & 0x80;

                while (strLen-- != 0) {
                    byte val = fileData[posn++];
                    if (limitHiBit && (val & 0x80) != expectedHiBit) {
                        // Mixed ASCII, fail.
                        return -1;
                    }
                    if (!charTest(val)) {
                        // Not a matching character, fail.
                        return -1;
                    }
                }
            }

            return stringCount;
        }

        /// <summary>
        /// Counts strings prefixed with a 16-bit length.
        ///
        /// Zero-length strings are allowed but not counted.
        /// </summary>
        /// <param name="fileData">Raw data.</param>
        /// <param name="start">Offset of first byte in range.</param>
        /// <param name="end">Offset of last byte in range.</param>
        /// <param name="charTest">Character test delegate.</param>
        /// <param name="limitHiBit">If set, the high bit in all character must be the
        ///   same.  Used to enforce a single encoding when "low or high ASCII" is used.</param>
        /// <returns>Number of strings found, or -1 if bad data identified.</returns>
        public static int RecognizeLen16Strings(byte[] fileData, int start, int end,
                CharEncoding.InclusionTest charTest, bool limitHiBit) {
            int posn = start;
            int remaining = end - start + 1;
            int stringCount = 0;

            while (remaining > 0) {
                if (remaining < 2) {
                    // Not enough bytes for length, fail.
                    return -1;
                }
                int strLen = fileData[posn++];
                strLen |= fileData[posn++] << 8;
                remaining -= 2;
                if (strLen > remaining) {
                    // Buffer doesn't hold entire string, fail.
                    return -1;
                }

                if (strLen == 0) {
                    continue;
                }
                stringCount++;
                remaining -= strLen;

                int expectedHiBit = fileData[posn] & 0x80;

                while (strLen-- != 0) {
                    byte val = fileData[posn++];
                    if (limitHiBit && (val & 0x80) != expectedHiBit) {
                        // Mixed ASCII, fail.
                        return -1;
                    }
                    if (!charTest(val)) {
                        // Not a matching character, fail.
                        return -1;
                    }
                }
            }

            return stringCount;
        }

        /// <summary>
        /// Counts strings in Dextral Character Inverted format, meaning the high bit on the
        /// last byte is the opposite of the preceding.
        /// 
        /// Each string must be at least two bytes.  To reduce false-positives, we require
        /// that all strings have the same hi/lo pattern.
        /// </summary>
        /// <remarks>
        /// For C64Petscii, this will identify strings that are entirely in lower case except
        /// for the last letteR, or vice-versa.
        /// </remarks>
        /// <param name="fileData">Raw data.</param>
        /// <param name="start">Offset of first byte in range.</param>
        /// <param name="end">Offset of last byte in range.</param>
        /// <param name="charTest">Character test delegate.</param>
        /// <returns>Number of strings found, or -1 if bad data identified.</returns>
        public static int RecognizeDciStrings(byte[] fileData, int start, int end,
                CharEncoding.InclusionTest charTest) {
            int expectedHiBit = fileData[start] & 0x80;
            int stringCount = 0;
            int stringLen = 0;

            // Quick test on last byte.
            if ((fileData[end] & 0x80) == expectedHiBit) {
                return -1;
            }

            for (int i = start; i <= end; i++) {
                byte val = fileData[i];
                if ((val & 0x80) != expectedHiBit) {
                    // end of string
                    if (stringLen == 0) {
                        // Got two consecutive bytes with end-marker polarity... fail.
                        return -1;
                    }
                    stringCount++;
                    stringLen = 0;
                } else {
                    stringLen++;
                }

                if (!charTest((byte)(val & 0x7f))) {
                    // Not a matching character, fail.
                    return -1;
                }
            }

            return stringCount;
        }

        /// <summary>
        /// Counts strings in reverse Dextral Character Inverted format, meaning the string is
        /// stored in reverse order in memory, and the high bit on the first (last) byte is
        /// the opposite of the rest.
        /// 
        /// Each string must be at least two bytes.  To reduce false-positives, we require
        /// that all strings have the same hi/lo pattern.
        /// </summary>
        /// <param name="fileData">Raw data.</param>
        /// <param name="start">Offset of first byte in range.</param>
        /// <param name="end">Offset of last byte in range.</param>
        /// <returns>Number of strings found, or -1 if bad data identified.</returns>
        public static int RecognizeReverseDciStrings(byte[] fileData, int start, int end) {
            int expectedHiBit = fileData[end] & 0x80;
            int stringCount = 0;
            int stringLen = 0;

            // Quick test on last (first) byte.
            if ((fileData[start] & 0x80) == expectedHiBit) {
                return -1;
            }

            for (int i = end; i >= start; i--) {
                byte val = fileData[i];
                if ((val & 0x80) != expectedHiBit) {
                    // end of string
                    if (stringLen == 0) {
                        // Got two consecutive bytes with end-marker polarity... fail.
                        return -1;
                    }
                    stringCount++;
                    stringLen = 0;
                } else {
                    stringLen++;
                }

                val &= 0x7f;
                if (val < 0x20 || val == 0x7f) {
                    // Non-ASCII, fail.
                    return -1;
                }
            }

            return stringCount;
        }

        #endregion // Static analyzers
    }
}



#if DATA_PRESCAN
        /// <summary>
        /// Iterator that generates a list of offsets which are not known to hold code or data.
        /// 
        /// Generates a set of integers in ascending order.
        /// </summary>
        private class UndeterminedValueIterator : IEnumerator {
            /// <summary>
            /// Index of current item, or -1 if we're not started yet.
            /// </summary>
            private int mCurIndex;

            /// <summary>
            /// Reference to Anattrib array we're iterating over.
            /// </summary>
            private Anattrib[] mAnattribs;


            /// <summary>
            /// Constructor.
            /// </summary>
            public UndeterminedValueIterator(Anattrib[] anattribs) {
                mAnattribs = anattribs;
                Reset();
            }

            // IEnumerator: current element
            public object Current {
                get {
                    if (mCurIndex < 0) {
                        // not started
                        return null;
                    }
                    return mCurIndex;
                }
            }

            // IEnumerator: move to the next element, returning false if there isn't one
            public bool MoveNext() {
                while (++mCurIndex < mAnattribs.Length) {
                    Anattrib attr = mAnattribs[mCurIndex];
                    if (attr.IsInstructionStart) {
                        // skip past instruction
                        mCurIndex += attr.Length - 1;
                    } else if (attr.IsUncategorized) {
                        // got one
                        return true;
                    }
                }

                return false;
            }

            // IEnumerator: reset state
            public void Reset() {
                mCurIndex = -1;
            }
        }
#endif
