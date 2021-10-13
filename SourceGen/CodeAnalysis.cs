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
using System.Collections.Generic;
using System.Diagnostics;

using Asm65;
using CommonUtil;
using PluginCommon;
using SourceGen.Sandbox;

namespace SourceGen {
    /// <summary>
    /// Instruction analyzer.
    /// 
    /// All data held in this object is transient, and will be discarded when analysis
    /// completes.  All user-defined values should be held elsewhere and provided as inputs
    /// to the analyzer.  Any change that merits re-analysis should be handled by creating a
    /// new instance of this object.
    /// 
    /// See the comments at the top of UndoableChange for a list of things that can
    /// mandate code re-analysis.
    /// </summary>
    /// <remarks>
    /// This invokes methods in extension scripts to handle things like inline data
    /// following a JSR.  The added cost is generally low, because the AppDomain security
    /// sandbox doesn't add a lot of overhead.  Unfortunately this approach is deprecated
    /// by Microsoft and may break or become unavailable.  If that happens, and we have to
    /// switch to a sandbox approach with significant overhead, we will most likely want
    /// to move the code analyzer itself into the sandbox.
    ///
    /// For this reason it's best to minimize direct interaction between the code here and
    /// that elsewhere in the program.
    /// </remarks>
    public class CodeAnalysis {
        /// <summary>
        /// Analyzer tags are specified by the user.  They identify an offset as being the
        /// start or end of an executable code region, or part of an inline data block.
        ///
        /// The tags are not used directly by the data analyzer, but the effects they
        /// have on the Anattrib array are.
        /// </summary>
        /// <remarks>
        /// THESE VALUES ARE SERIALIZED to the project data file.  They cannot be renamed
        /// without writing a translator in ProjectFile.
        /// </remarks>
        public enum AnalyzerTag : sbyte {
            // No tag.  Default value populated in new arrays.
            None = 0,

            // Byte is an instruction.  If the code analyzer doesn't find this
            // naturally, it will be scanned.
            Code,

            // Byte is inline data.  Execution skips over the byte.
            InlineData,

            // Byte is data.  Execution halts.
            Data
        }

        /// <summary>
        /// Class for handling callbacks from extension scripts.
        /// </summary>
        private class ScriptSupport : MarshalByRefObject, PluginCommon.IApplication {
            private CodeAnalysis mOuter;

            public ScriptSupport(CodeAnalysis ca) {
                mOuter = ca;
            }

            /// <summary>
            /// Call this when analysis is complete, to ensure that over-active scripts
            /// can't keep doing things.  (This is not part of IApplication.)
            /// </summary>
            public void Shutdown() {
                mOuter = null;
            }

            public void ReportError(string msg) {
                DebugLog(msg);
            }

            public void DebugLog(string msg) {
                mOuter.mDebugLog.LogI("PLUGIN: " + msg);
            }

            public bool SetOperandFormat(int offset, DataSubType subType, string label) {
                return mOuter.SetOperandFormat(offset, subType, label);
            }

            public bool SetInlineDataFormat(int offset, int length, DataType type,
                    DataSubType subType, string label) {
                return mOuter.SetInlineDataFormat(offset, length, type, subType, label);
            }
        }

        /// <summary>
        /// Extension script manager.
        /// </summary>
        private ScriptManager mScriptManager;

        /// <summary>
        /// Local object that implements the IApplication interface for plugins.
        /// </summary>
        private ScriptSupport mScriptSupport;

        /// <summary>
        /// List of interesting plugins.  If we have plugins that don't do code inlining we
        /// can ignore them.  (I'm using an array instead of a List&lt;IPlugin&gt; as a
        /// micro-optimization; see https://stackoverflow.com/a/454923/294248 .)
        /// </summary>
        private IPlugin[] mScriptArray;

        [Flags]
        private enum PluginCap { NONE = 0, JSR = 1 << 0, JSL = 1 << 1, BRK = 1 << 2 };
        private PluginCap[] mPluginCaps;

        /// <summary>
        /// CPU to use when analyzing data.
        /// </summary>
        private CpuDef mCpuDef;

        /// <summary>
        /// Map of offsets to addresses.
        /// </summary>
        private AddressMap mAddrMap;

        /// <summary>
        /// Reference to 65xx data.
        /// </summary>
        private byte[] mFileData;

        /// <summary>
        /// Attributes, one per byte in input file.
        /// </summary>
        private Anattrib[] mAnattribs;

        /// <summary>
        /// Reference to analyzer tag array, one entry per byte.
        /// </summary>
        private AnalyzerTag[] mAnalyzerTags;

        /// <summary>
        /// Reference to status flag override array, one entry per byte.
        /// </summary>
        private StatusFlags[] mStatusFlagOverrides;

        /// <summary>
        /// Initial status flags to use at entry points.
        /// </summary>
        private StatusFlags mEntryFlags;

        /// <summary>
        /// User-configurable analysis parameters.
        /// </summary>
        private ProjectProperties.AnalysisParameters mAnalysisParameters;

        /// <summary>
        /// Debug trace log.
        /// </summary>
        private DebugLog mDebugLog = new DebugLog(DebugLog.Priority.Silent);


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="data">65xx code stream.</param>
        /// <param name="cpuDef">CPU definition to use when interpreting code.</param>
        /// <param name="anattribs">Anattrib array.  Expected to be newly allocated, all
        ///   entries set to default values.</param>
        /// <param name="addrMap">Map of offsets to addresses.</param>
        /// <param name="atags">Analyzer tags, one per byte.</param>
        /// <param name="statusFlagOverrides">Status flag overrides for instruction-start
        ///    bytes.</param>
        /// <param name="entryFlags">Status flags to use at code entry points.</param>
        /// <param name="scriptMan">Extension script manager.</param>
        /// <param name="parms">Analysis parameters.</param>
        /// <param name="debugLog">Object that receives debug log messages.</param>
        public CodeAnalysis(byte[] data, CpuDef cpuDef, Anattrib[] anattribs,
                AddressMap addrMap, AnalyzerTag[] atags, StatusFlags[] statusFlagOverrides,
                StatusFlags entryFlags, ProjectProperties.AnalysisParameters parms,
                ScriptManager scriptMan, DebugLog debugLog) {
            mFileData = data;
            mCpuDef = cpuDef;
            mAnattribs = anattribs;
            mAddrMap = addrMap;
            mAnalyzerTags = atags;
            mStatusFlagOverrides = statusFlagOverrides;
            mEntryFlags = entryFlags;
            mScriptManager = scriptMan;
            mAnalysisParameters = parms;
            mDebugLog = debugLog;

            mScriptSupport = new ScriptSupport(this);
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
#if true
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
#else
        private void LogD(int offset, string msg) { }
        private void LogI(int offset, string msg) { }
        private void LogW(int offset, string msg) { }
        private void LogE(int offset, string msg) { }
#endif

        /// <summary>
        /// Analyze a blob of code and data, annotating all code areas.
        ///
        /// Also identifies data embedded in code, e.g. parameter blocks following a JSR,
        /// with the help of extension scripts.
        ///
        /// Failing here can leave us in a strange state, so prefer to work around unexpected
        /// inputs rather than bailing entirely.
        /// </summary>
        public void Analyze() {
            List<int> scanOffsets = new List<int>();

            mDebugLog.LogI("Analyzing code: " + mFileData.Length + " bytes, CPU=" + mCpuDef.Name);

            PrepareScripts();

            SetAddresses();

            // Set values in the anattrib array based on the user-specified analyzer tags.
            // This tells us to stop processing or skip over bytes as we work.  We set values
            // for the code start tags so we can show them in the "info" window.
            //
            // The data recognizers may spot additional inline data offsets as we work.  This
            // can cause a race if it mis-identifies code that is also a branch target;
            // whichever marks the code first will win.
            UnpackAnalyzerTags();

            // Find starting place, based on analyzer tags.
            //
            // We only set the "visited" flag on the instruction start, so if the user
            // puts a code start in the middle of an instruction, we will find it and
            // treat it as an entry point.  (This is useful for embedded instructions
            // that are branched to by code we aren't able to detect.)
            int searchStart = FindFirstUnvisitedInstruction(0);
            while (searchStart >= 0) {
                mAnattribs[searchStart].IsEntryPoint = true;
                mAnattribs[searchStart].StatusFlags = mEntryFlags;
                mAnattribs[searchStart].ApplyStatusFlags(mStatusFlagOverrides[searchStart]);

                int offset = searchStart;
                while (true) {
                    bool embedded = (mAnattribs[offset].IsInstruction &&
                        !mAnattribs[offset].IsVisited);
                    LogI(offset, "Scan chunk (vis=" + mAnattribs[offset].IsVisited +
                        " chg=" + mAnattribs[offset].IsChanged +
                        (embedded ? " embedded " : "") + ")");

                    AnalyzeSegment(offset, scanOffsets);

                    // Did anything new get added?
                    if (scanOffsets.Count == 0) {
                        break;
                    }

                    // Pop one off the end.
                    int lastItem = scanOffsets.Count - 1;
                    offset = scanOffsets[lastItem];
                    scanOffsets.RemoveAt(lastItem);
                }

                searchStart = FindFirstUnvisitedInstruction(searchStart);
            }

            if (mScriptManager != null) {
                mScriptManager.UnprepareScripts();
            }
            mScriptSupport.Shutdown();

            MarkUnexecutedEmbeddedCode();
        }

        /// <summary>
        /// Prepare a list of relevant extension scripts.
        /// </summary>
        private void PrepareScripts() {
            if (mScriptManager == null) {
                // Currently happens for regression tests with no external files.
                mScriptArray = new IPlugin[0];
                mPluginCaps = new PluginCap[0];
                return;
            }

            // Include all scripts.
            mScriptArray = mScriptManager.GetAllInstances().ToArray();
            mPluginCaps = new PluginCap[mScriptArray.Length];
            for (int i = 0; i < mScriptArray.Length; i++) {
                PluginCap cap = PluginCap.NONE;
                if (mScriptArray[i] is IPlugin_InlineJsr) {
                    cap |= PluginCap.JSR;
                }
                if (mScriptArray[i] is IPlugin_InlineJsl) {
                    cap |= PluginCap.JSL;
                }
                if (mScriptArray[i] is IPlugin_InlineBrk) {
                    cap |= PluginCap.BRK;
                }
                mPluginCaps[i] = cap;
            }

            // Prep them.
            mScriptManager.PrepareScripts(mScriptSupport);
        }

        /// <summary>
        /// Sets the address for every byte in the input.
        /// </summary>
        private void SetAddresses() {
            IEnumerator<AddressMap.AddressChange> addrIter = mAddrMap.AddressChangeIterator;
            addrIter.MoveNext();
            int addr = 0;
            bool nonAddr = false;
            bool addrChange = false;

            for (int offset = 0; offset < mAnattribs.Length; offset++) {
                AddressMap.AddressChange change = addrIter.Current;

                // Process all start events at this offset.  The new address takes effect
                // immediately.
                while (change != null && change.IsStart && change.Offset == offset) {
                    addr = change.Address;
                    if (addr == Address.NON_ADDR) {
                        addr = 0;
                        nonAddr = true;
                    } else {
                        nonAddr = false;
                    }
                    addrChange = true;
                    addrIter.MoveNext();
                    change = addrIter.Current;
                }

                mAnattribs[offset].Address = addr++;
                mAnattribs[offset].IsAddrRegionChange = addrChange;
                mAnattribs[offset].IsNonAddressable = nonAddr;
                addrChange = false;

                // Process all end events at this offset.  The new address and "address
                // region change" flag take effect on the *following* offset.
                while (change != null && !change.IsStart && change.Offset == offset) {
                    addr = change.Address;
                    if (addr == Address.NON_ADDR) {
                        addr = 0;
                        nonAddr = true;
                    } else {
                        nonAddr = false;
                    }
                    addrChange = true;
                    addrIter.MoveNext();
                    change = addrIter.Current;
                }
            }
        }

        /// <summary>
        /// Sets the "is xxxxx" flags on analyzer-tagged entries, so that the code analyzer
        /// can find them easily.
        /// </summary>
        private void UnpackAnalyzerTags() {
            Debug.Assert(mAnalyzerTags.Length == mAnattribs.Length);
            int offset = 0;
            foreach (AnalyzerTag atag in mAnalyzerTags) {
                switch (atag) {
                    case AnalyzerTag.Code:
                        // Set the IsInstruction flag to prevent inline data from being
                        // placed here.
                        OpDef op = mCpuDef.GetOpDef(mFileData[offset]);
                        if (op == OpDef.OpInvalid) {
                            // Might want to set the "has tag" value anyway, since it won't
                            // appear in the "Info" window if we don't.  Or maybe we need a
                            // message about "invisible" code start tags?
                            LogI(offset, "Ignoring code start tag on illegal opcode");
                        } else {
                            mAnattribs[offset].HasAnalyzerTag = true;
                            mAnattribs[offset].IsInstruction = true;
                        }
                        break;
                    case AnalyzerTag.Data:
                        // Tells the code analyzer to stop.
                        mAnattribs[offset].HasAnalyzerTag = true;
                        mAnattribs[offset].IsData = true;
                        break;
                    case AnalyzerTag.InlineData:
                        // Tells the code analyzer to walk across these.
                        mAnattribs[offset].HasAnalyzerTag = true;
                        mAnattribs[offset].IsInlineData = true;
                        break;
                    case AnalyzerTag.None:
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
                offset++;
            }
        }

        /// <summary>
        /// Finds the first offset that is tagged as code start but hasn't yet been visited.
        ///
        /// This might be in the middle of an already-visited instruction.
        /// </summary>
        /// <param name="start">Offset at which to start the search.</param>
        /// <returns>Offset found.</returns>
        private int FindFirstUnvisitedInstruction(int start) {
            for (int i = start; i < mAnattribs.Length; i++) {
                if (mAnattribs[i].HasAnalyzerTag && mAnalyzerTags[i] == AnalyzerTag.Code &&
                        !mAnattribs[i].IsVisited) {
                    LogD(i, "Unvisited code start tag");
                    if (mAnattribs[i].IsData || mAnattribs[i].IsInlineData) {
                        // Maybe the user put a code start tag on something that was
                        // later recognized as inline data?  Shouldn't have been allowed.
                        LogW(i, "Weird: code start tag on data/inline");
                        continue;
                    }
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Finds bits of code that are part of embedded instructions but not actually
        /// executed, and marks them as inline data.
        /// </summary>
        private void MarkUnexecutedEmbeddedCode() {
            // The problem arises when you have a line like 4C 60 EA, with a branch to the
            // middle byte.  The formatter will print "JMP $EA60", then "<label> RTS", and
            // then should print NOP.  The problem is that the NOP wasn't reached by the
            // code analyzer, and so isn't tagged as an instruction start.  It's effectively
            // inline data, so we need to mark it that way.
            //
            // We don't have a quick way to find these, so we just run through the list.
            for (int offset = 0; offset < mFileData.Length; ) {
                if (mAnattribs[offset].IsInstructionStart) {
                    int len;
                    for (len = 1; len < mAnattribs[offset].Length; len++) {
                        if (mAnattribs[offset + len].IsInstructionStart) {
                            break;
                        }
                    }

                    offset += len;
                } else if (mAnattribs[offset].IsInstruction) {
                    // bingo
                    LogI(offset, "Fixing embedded orphan");
                    mAnattribs[offset].IsInstruction = false;
                    mAnattribs[offset].IsInlineData = true;
                    mAnattribs[offset].DataDescriptor = FormatDescriptor.Create(1,
                        FormatDescriptor.Type.NumericLE, FormatDescriptor.SubType.None);
                    offset++;
                } else {
                    offset++;
                }
            }
        }

        /// <summary>
        /// Analyzes a code segment.  A code segment is a contiguous series of instructions.
        /// We halt if we encounter a return, always-taken branch, or the end of the
        /// current address map section.
        /// 
        /// If we find branches to unvisited code, or previously-visited code that has
        /// different status flags, we add that to the list of offsets to scan.
        /// </summary>
        /// <param name="offset">Starting offset.</param>
        /// <param name="scanOffsets">Collection to which additional offsets of interest will
        ///   be added.</param>
        private void AnalyzeSegment(int offset, List<int> scanOffsets) {
            while (offset < mFileData.Length) {
                if (mAnattribs[offset].IsVisited && !mAnattribs[offset].IsChanged) {
                    // already visited, not changed; nothing to do
                    LogD(offset, "Visited and not changed, bailing");
                    return;
                }

                bool firstVisit = !mAnattribs[offset].IsVisited;

                // Set "visited" flag, clear "changed".
                mAnattribs[offset].IsVisited = true;
                mAnattribs[offset].IsChanged = false;

                if (mAnattribs[offset].IsData) {
                    // This area was declared to be data. Go no further.  This shouldn't
                    // usually happen -- either we should have stopped tracing, or we
                    // should have identified the data area as code.
                    LogI(offset, "Code ran into data section");
                    Debug.Assert(false);
                    return;
                } else if (mAnattribs[offset].IsInlineData) {
                    // Generally this won't happen, because we ignore branches into inline data
                    // areas, we reject attempts to convert code to inline data, and we can't
                    // start in an inline area because the tag is wrong.  However, it's possible
                    // for a JSR to a new section to be registered, and then before we get to
                    // it an extension script formats the area as inline data.  In that case
                    // the inline data "wins", and we stop here.
                    LogW(offset, "Code ran into inline data section");
                    return;
                } else if (mAnattribs[offset].IsNonAddressable) {
                    mAnattribs[offset].IsInstruction = false;
                    LogW(offset, "Code ran into non-addressable area");
                    return;
                }

                // Identify the instruction, and see if it runs off the end of the file.
                // If it does, treat it as data.
                OpDef op = mCpuDef.GetOpDef(mFileData[offset]);
                int instrLen = op.GetLength(mAnattribs[offset].StatusFlags);
                LogV(offset, "OP $" + mFileData[offset].ToString("X2") + " len=" + instrLen);
                if (offset + instrLen > mFileData.Length) {
                    // Instruction runs off the end.  It's possible we visited here before with
                    // short M/X flags, or some other code jumps to code embedded in our
                    // operand.  Whatever the case, we want to clear the instruction flag from
                    // the first byte.  We can mark it as data so subsequent passes don't
                    // bump into this.
                    LogW(offset, "Instruction runs off end of file");
                    mAnattribs[offset].IsInstructionStart = false;
                    mAnattribs[offset].IsInstruction = false;
                    mAnattribs[offset].IsData = true;
                    return;
                }

                // Check for mid-instruction address region changes.  An address change on the
                // first byte is fine.
                for (int i = offset + 1; i < offset + instrLen; i++) {
                    if (mAnattribs[i].IsAddrRegionChange) {
                        // Found a region start and/or end.  Mark this offset as data and return.
                        LogW(offset, "Detected address change mid-instruction");
                        mAnattribs[offset].IsInstructionStart = false;
                        mAnattribs[offset].IsInstruction = false;
                        mAnattribs[offset].IsData = true;
                        return;
                    }
                }

                // Instruction not defined for this CPU.  Treat as data.
                if (op.AddrMode == OpDef.AddressMode.Unknown) {
                    LogW(offset, "Instruction stream encountered invalid opcode ($" +
                        mFileData[offset].ToString("x2") + ")");
                    return;
                }

                // Flag as start of valid instruction, and mark all bytes as instructions.
                // There's a possible conflict here if the first byte is marked as an
                // instruction, but bytes within the instruction are marked as data.  The
                // easiest thing to do here is steamroll the data flags.
                //
                // (To cause this, tag a 3-byte instruction as code-stop/inline-data, then
                // tag the first byte of the instruction as code.)
                mAnattribs[offset].IsInstructionStart = true;
                mAnattribs[offset].Length = instrLen;
                for (int i = offset; i < offset + instrLen; i++) {
                    if (mAnattribs[i].IsData) {
                        LogW(i, "Stripping mid-instruction data flag");
                        mAnattribs[i].IsData = false;
                        mAnattribs[i].DataDescriptor = null;
                    } else if (mAnattribs[i].IsInlineData) {
                        LogW(i, "Stripping mid-instruction inline-data flag");
                        mAnattribs[i].IsInlineData = false;
                        mAnattribs[i].DataDescriptor = null;
                    }
                    mAnattribs[i].IsInstruction = true;
                }

                // Compute the effect on the status flags.
                StatusFlags newFlags, condBranchTakenFlags;
                if (op == OpDef.OpPLP_StackPull) {
                    // PLP restores flags from the stack.
                    newFlags = condBranchTakenFlags = GuessFlagsForPLP(offset);
                } else {
                    op.ComputeFlagChanges(mAnattribs[offset].StatusFlags, mFileData, offset,
                        out newFlags, out condBranchTakenFlags);
                }

                // Handle stuff that won't be different on a subsequent visit.
                if (firstVisit) {
                    // Decode the operand for instructions that reference an address.  If
                    // the target address is within the file's address space, record the
                    // offset as well.  This doesn't examine immediate operands.
                    DecodeOperandAddress(offset, op);
                }

                int branchOffset = -1;
                bool doBranch, doContinue;

                // Check for branching.
                if (op.IsBranchOrSubCall) {
                    if (mAnattribs[offset].IsOperandOffsetDirect) {
                        branchOffset = mAnattribs[offset].OperandOffset;
                    }
                    if (branchOffset >= 0 && branchOffset < mFileData.Length) {
                        doBranch = true;
                    } else {
                        // External branch.  Very common for JSR to ROM routines and JMP
                        // through an indirect address.  Not usually expected for relative
                        // branches.
                        if (op.Effect != OpDef.FlowEffect.CallSubroutine) {
                            LogD(offset, "Branch goes external");
                        }
                        doBranch = false;
                        mAnattribs[offset].IsExternalBranch = true;
                    }
                } else {
                    doBranch = false;
                }

                // Check continuation to next instruction.
                switch (op.Effect) {
                    case OpDef.FlowEffect.Cont:
                    case OpDef.FlowEffect.CallSubroutine:
                    case OpDef.FlowEffect.ConditionalBranch:
                        doContinue = true;
                        break;
                    default:
                        doContinue = false;
                        break;
                }

                // Some 6502 code works around the lack of a branch-always instruction with
                // a complement pair (e.g. BCC + BCS), so we don't want to continue past a branch
                // always taken.  The converse is also true: don't pursue a branch if it's
                // never taken.  An example from 6502.org:
                // "... a common sequence on the 6502 family is:
                //  CLEAR_FLAG CLC
                //             DB  $B0
                //  SET_FLAG   SEC
                //             ROR FLAG
                //             RTS
                // When entering via CLEAR_FLAG, the $B0 becomes a 2-cycle BCS instruction, which
                // is not taken (since the carry is clear). Since BCS does not affect any flags,
                // it serves, in this situation, as a two byte, two cycle NOP and provides a
                // subtle, but useful way to efficiently skip the SEC instruction."

                // Revise branch/cont for conditional branch instructions.
                if (op.Effect == OpDef.FlowEffect.ConditionalBranch) {
                    OpDef.BranchTaken taken =
                        OpDef.IsBranchTaken(op, mAnattribs[offset].StatusFlags);
                    if (taken == OpDef.BranchTaken.Never) {
                        doBranch = false;
                    } else if (taken == OpDef.BranchTaken.Always) {
                        doContinue = false;
                    }
                    mAnattribs[offset].BranchTaken = taken;
                }

                // Make sure destination isn't already flagged as data.
                if (doBranch) {
                    Debug.Assert(branchOffset >= 0);
                    if (mAnattribs[branchOffset].IsData || mAnattribs[branchOffset].IsInlineData) {
                        LogW(offset, "Ignoring branch to +" + branchOffset.ToString("x6") +
                            " (data region)");
                        doBranch = false;
                        branchOffset = -1;
                    }
                }

                LogV(offset, "doBranch=" + doBranch + ", doCont=" + doContinue);

                if (doBranch) {
                    // Flag the destination offset as a branch target.
                    mAnattribs[branchOffset].IsBranchTarget = true;

                    // Merge our status flags with theirs.
                    StatusFlags branchStatusBefore = mAnattribs[branchOffset].StatusFlags;
                    mAnattribs[branchOffset].MergeStatusFlags(condBranchTakenFlags);
                    mAnattribs[branchOffset].ApplyStatusFlags(mStatusFlagOverrides[branchOffset]);

                    // If we need to (re-)scan this offset, add it to the list.
                    //AttribFlags branchFlags = mAnattribs[branchOffset].mAttribFlags;
                    bool addToScan = false;
                    string why;
                    if (!mAnattribs[branchOffset].IsVisited) {
                        // Not yet visited. Some flags may have been set by earlier branch.
                        // Merge status flags and add to scan list if not already present.
                        addToScan = true;
                        why = "(not visited)";
                    } else {
                        // Visited before. If the status flags changed, set "changed" and
                        // add to scan offsets.
                        if (branchStatusBefore != mAnattribs[branchOffset].StatusFlags) {
                            mAnattribs[branchOffset].IsChanged = true;
                            addToScan = true;
                        }
                        why = "(flags: " + branchStatusBefore + " -> " +
                            mAnattribs[branchOffset].StatusFlags + ")";
                    }
                    if (addToScan && !scanOffsets.Contains(branchOffset)) {
                        LogD(offset, "Adding " + branchOffset.ToString("x4") +
                            " to scan list " + why);
                        scanOffsets.Add(branchOffset);
                    }
                }

                // On every visit, check for BRK inline call.  The default behavior for BRK
                // is no-continue, the opposite of JSR/JSL.
                // TODO: Ideally we'd have an explicit flag (maybe make NoContinueScript a
                // tri-state) to avoid calling the plugin repeatedly.
                //if (firstVisit) {
                    if (op == OpDef.OpBRK_Implied || op == OpDef.OpBRK_StackInt) {
                        bool noContinue = CheckForInlineCall(op, offset, !doContinue);
                        if (!noContinue) {
                            // We're expected to continue execution past the BRK.
                            doContinue = true;
                        }
                    }
                //}

                mAnattribs[offset].NoContinue = !doContinue;
                if (mAnattribs[offset].DoesNotContinue) {
                    // If we just decided not to continue, or an extension script set a flag
                    // on a previous visit, stop scanning forward.
                    break;
                }

                // Sanity check to avoid infinite loop.
                if (instrLen <= 0) {
                    LogE(offset, "Internal error: instruction length " + instrLen);
                    throw new Exception("Instruction length was " + instrLen);
                }

                int nextOffset = offset + instrLen;
                if (nextOffset >= mFileData.Length) {
                    // next instruction is off the end of the file
                    LogW(offset, "Execution ran off the end of the file");
                    break;
                }

                // On first visit, check for JSR/JSL inline call.  If it's "no-continue",
                // set a flag and halt here.
                if (firstVisit) {
                    // Currently ignoring OpDef.OpJSR_AbsIndexXInd
                    if (op == OpDef.OpJSR_Abs || op == OpDef.OpJSR_AbsLong) {
                        bool noContinue = CheckForInlineCall(op, offset, false);
                        if (noContinue) {
                            LogD(offset, "Script declared inline call no-continue");
                            mAnattribs[offset].NoContinueScript = true;
                            break;
                        }
                    }
                } else if (mAnattribs[offset].NoContinueScript) {
                    // Wanted to stop last time.
                    break;
                }

                // Are we about to walk into inline data?
                int inlineDataGapLen = 0;
                while (nextOffset < mFileData.Length && mAnattribs[nextOffset].IsInlineData) {
                    // Skip over it to find next instruction (or next inline data chunk).
                    // Note Anattrib.Length==0 unless a format has been applied, so we just
                    // walk forward a byte at a time.
                    inlineDataGapLen++;
                    nextOffset++;
                }

                // Re-check after inline data advance.
                if (nextOffset >= mFileData.Length) {
                    // next instruction is off the end of the file
                    LogW(offset, "Execution ran off the end of the file");
                    break;
                }
                if (mAnattribs[nextOffset].IsData) {
                    // Drove into a data section
                    LogW(offset, "Execution ran into a data area");
                    break;
                }

                // Make sure we don't "continue" across an address change.  This is different
                // from the earlier mid-instruction check in that we don't actually care if
                // there's a region change between instructions so long as the next address
                // has the expected value.
                int expectedAddr = mAnattribs[offset].Address + mAnattribs[offset].Length +
                    inlineDataGapLen;
                if (mAnattribs[nextOffset].Address != expectedAddr) {
                    LogW(offset, "Execution ran across address change (" +
                        expectedAddr.ToString("x4") + " vs. " +
                        mAnattribs[nextOffset].Address.ToString("x4") + ")");
                    break;
                }

                // Merge the updated status flags into the next instruction.
                StatusFlags nextStatusBefore = mAnattribs[nextOffset].StatusFlags;
                mAnattribs[nextOffset].MergeStatusFlags(newFlags);
                mAnattribs[nextOffset].ApplyStatusFlags(mStatusFlagOverrides[nextOffset]);

                // If we've already visited the next offset, and the updated status flags are
                // the same as the previous status flags, then there's nothing to gain by
                // continuing forward.
                if (mAnattribs[nextOffset].IsVisited && !mAnattribs[nextOffset].IsChanged) {
                    if (nextStatusBefore == mAnattribs[nextOffset].StatusFlags) {
                        // Instruction has been visited, hasn't been flagged as changed,
                        // and our status flag merge had no effect. No need to continue
                        // through.
                        LogV(offset, "Not re-examining " + nextOffset);
                        break;
                    } else {
                        // We changed the flags, need to re-evaluate conditional branches.
                        mAnattribs[nextOffset].IsChanged = true;
                    }
                }

                offset = nextOffset;
            }
        }

        /// <summary>
        /// Attempts to guess what the flags will be after a PLP instruction.
        /// </summary>
        /// <remarks>
        /// We're not tracking stack contents or register contents, so this just
        /// generally won't work.  However, there's a lot of code that uses PHP to
        /// save the current state and PLP to restore it, so if we can find a nearby
        /// PHP we can just grab from that.
        ///
        /// Failing that, we mark all flags as "indeterminate" and let the user sort
        /// out what it should be.  It's unlikely to matter except for M/X flags on
        /// the 65816.
        ///
        /// The emulation flag is not part of the status register, even if we do carry
        /// it around like one.  The E-flag is always carried over from the previous
        /// instruction.
        /// </remarks>
        /// <param name="plpOffset">Offset of PLP instruction.</param>
        /// <returns>Best guess at status flags.</returns>
        private StatusFlags GuessFlagsForPLP(int plpOffset) {
            StatusFlags flags = StatusFlags.AllIndeterminate;
            if (mAnalysisParameters.SmartPlpHandling) {
                // TODO: this is broken.  In some cases we end up latching the result from the
                // first visit only.  When the PHP instruction gets updated, the subsequent
                // instructions are only re-evaluated if the flags have changed.  If we reach
                // an instruction where the flags match, we stop looking forward, and might
                // not re-visit the PLP.
                int backOffsetLimit = plpOffset - 128;      // arbitrary 128-byte reach
                if (backOffsetLimit < 0) {
                    backOffsetLimit = 0;
                }
                for (int offset = plpOffset - 1; offset >= backOffsetLimit; offset--) {
                    Anattrib attr = mAnattribs[offset];
                    if (!attr.IsInstructionStart || !attr.IsVisited) {
                        continue;
                    }
                    OpDef op = mCpuDef.GetOpDef(mFileData[offset]);
                    if (op == OpDef.OpPHP_StackPush) {
                        LogI(plpOffset, "Found visited PHP at +" + offset.ToString("x6"));
                        flags = mAnattribs[offset].StatusFlags;
                        break;
                    }
                }
            }

            if (flags == StatusFlags.AllIndeterminate &&
                    (mCpuDef.Type == CpuDef.CpuType.Cpu65816 ||
                        mCpuDef.Type == CpuDef.CpuType.Cpu65802)) {
                // Having indeterminate M/X flags is really bad.  If "smart" handling failed or
                // is disabled, copy flags from previous instruction.
                flags.M = mAnattribs[plpOffset].StatusFlags.M;
                flags.X = mAnattribs[plpOffset].StatusFlags.X;
            }

            // Transfer the 'E' flag.
            flags.E = mAnattribs[plpOffset].StatusFlags.E;
            return flags;
        }

        /// <summary>
        /// Extracts the address from the operand of an absolute or relative operation.
        /// Anything that could be referenced by a label or address equate is appropriate.
        /// The goal is to identify data and branch targets, not generate a second copy
        /// of the operand.
        /// 
        /// The operand's address, and if applicable, the operand's file offset, are
        /// stored in the Anattrib array.
        /// 
        /// Doesn't do anything with immediate data.
        /// </summary>
        /// <remarks>
        /// For PC-relative operands (e.g. branches) it's tempting to simply adjust the file
        /// offset by the specified amount and convert that to an address.  If the file
        /// has multiple ORGs, this can produce incorrect results.  We need to convert the
        /// opcode's offset to an address, adjust by the operand, and then find the file
        /// offset that corresponds to the target address.
        ///
        /// This is called once per instruction, on the analyzer's first visit.
        /// </remarks>
        /// <param name="offset">Offset of the instruction opcode.</param>
        /// <param name="op">Opcode being handled. (Passed in because the caller has it
        ///   handy.)</param>
        private void DecodeOperandAddress(int offset, OpDef op) {
            //StatusFlags flags = mAnattribs[offset].StatusFlags;

            int operand = op.GetOperand(mFileData, offset, mAnattribs[offset].StatusFlags);

            // Add the bank to get a 24-bit address.  For some instructions the relevant bank
            // is known, because the operand is merged with the Program Bank Register (K) or
            // is always in bank 0.  For some we need the Data Bank Register (B).
            //
            // Instead of trying to track the B register during code analysis, we mark the
            // relevant instructions now and fix them up later.  We can get away with this
            // because the DBR is only applied to data-load instructions, which don't affect
            // the flow of the analysis pass.  The value of B *is* affected by the analysis
            // pass because a "smart PLB" handler needs to know where all the code is, so it's
            // more efficient to figure it out later.
            int bank = mAnattribs[offset].Address & 0x7fff0000;

            // Extract target address.
            switch (op.AddrMode) {
                // These might refer to a location in the file, or might be external.
                case OpDef.AddressMode.Abs:                 // uses DBR iff !IsAbsolutePBR
                case OpDef.AddressMode.AbsIndexX:           // uses DBR
                case OpDef.AddressMode.AbsIndexY:           // uses DBR
                    if (!op.IsAbsolutePBR) {
                        mAnattribs[offset].UsesDataBankReg = true;
                    }
                    // Merge the PBR even if we eventually want the DBR; less to fix later.
                    mAnattribs[offset].OperandAddress = operand | bank;
                    break;
                case OpDef.AddressMode.StackAbs:            // assume PBR
                case OpDef.AddressMode.AbsIndexXInd:        // JMP (addr,X); uses program bank
                    mAnattribs[offset].OperandAddress = operand | bank;
                    break;
                case OpDef.AddressMode.AbsInd:              // JMP (addr); always bank 0
                case OpDef.AddressMode.AbsIndLong:          // JMP [addr]; always bank 0
                case OpDef.AddressMode.DP:
                case OpDef.AddressMode.DPIndexX:
                case OpDef.AddressMode.DPIndexY:
                case OpDef.AddressMode.DPIndexXInd:
                case OpDef.AddressMode.DPInd:
                case OpDef.AddressMode.DPIndLong:
                case OpDef.AddressMode.DPIndIndexY:
                case OpDef.AddressMode.DPIndIndexYLong:
                case OpDef.AddressMode.StackDPInd:
                    // always bank 0
                    mAnattribs[offset].OperandAddress = operand;
                    break;
                case OpDef.AddressMode.AbsIndexXLong:
                case OpDef.AddressMode.AbsLong:
                    // 24-bit address, don't alter bank
                    mAnattribs[offset].OperandAddress = operand;
                    break;
                case OpDef.AddressMode.PCRel:   // rel operand; convert to absolute addr
                    mAnattribs[offset].OperandAddress =
                        Asm65.Helper.RelOffset8(mAnattribs[offset].Address,
                            (sbyte)operand) | bank;
                    break;
                case OpDef.AddressMode.DPPCRel:
                    // Like PCRel, but part of a 2-byte operand, so we use the 16-bit offset
                    // function.  We totally ignore the DP byte.
                    mAnattribs[offset].OperandAddress =
                        Asm65.Helper.RelOffset16(mAnattribs[offset].Address,
                            (sbyte)(operand >> 8)) | bank;
                    break;
                case OpDef.AddressMode.PCRelLong:
                case OpDef.AddressMode.StackPCRelLong:
                    mAnattribs[offset].OperandAddress =
                        Asm65.Helper.RelOffset16(mAnattribs[offset].Address,
                            (short)operand) | bank;
                    break;
                default:
                    // Immediate, implied, accumulator, stack relative.  We can't do
                    // immediate yet because we won't necessarily have a final assessment
                    // of the operand width on the 16-bit CPUs.
                    Debug.Assert(mAnattribs[offset].OperandAddress == -1);
                    break;
            }

            if (mAnattribs[offset].OperandAddress >= 0) {
                int operandOffset = mAddrMap.AddressToOffset(offset,
                    mAnattribs[offset].OperandAddress);
                if (operandOffset >= 0) {
                    mAnattribs[offset].OperandOffset = operandOffset;

                    // Set a flag if this is a direct offset.  This is used when tracing
                    // through jump instructions, as we can't necessarily decode an indirect
                    // jump.  (There are *some* indirect JMPs we can handle, if the operand
                    // is an address in the file data area.)
                    switch (op.AddrMode) {
                        case OpDef.AddressMode.Abs:
                        case OpDef.AddressMode.AbsLong:
                        case OpDef.AddressMode.DP:
                        case OpDef.AddressMode.DPPCRel:
                        case OpDef.AddressMode.PCRel:
                        case OpDef.AddressMode.PCRelLong:
                        case OpDef.AddressMode.StackPCRelLong:
                        case OpDef.AddressMode.StackAbs:
                            mAnattribs[offset].IsOperandOffsetDirect = true;
                            break;
                        default:
                            mAnattribs[offset].IsOperandOffsetDirect = false;
                            break;
                    }
                }
            } else {
                Debug.Assert(mAnattribs[offset].OperandOffset == -1);
                Debug.Assert(!mAnattribs[offset].IsOperandOffsetDirect);
            }
        }

        /// <summary>
        /// Queries script extensions to check to see if a JSR or JSL is actually an inline call.
        /// The script may format things.
        /// </summary>
        /// <param name="op">Instruction being examined.</param>
        /// <param name="offset">File offset of start of instruction.</param>
        /// <param name="noContinue">Set if any plugin declares the call to be no-continue.</param>
        /// <returns>Updated value for noContinue.</returns>
        private bool CheckForInlineCall(OpDef op, int offset, bool noContinue) {
            int operand = op.GetOperand(mFileData, offset, mAnattribs[offset].StatusFlags);
            for (int i = 0; i < mScriptArray.Length; i++) {
                try {
                    IPlugin script = mScriptArray[i];
                    // The IPlugin object is a MarshalByRefObject, which doesn't define the
                    // interface directly.  A simple test showed it was fairly quick when the
                    // interface was implemented but a bit slow when it wasn't.  For performance
                    // we query the capability flags instead.
                    if (op == OpDef.OpJSR_Abs && (mPluginCaps[i] & PluginCap.JSR) != 0) {
                        ((IPlugin_InlineJsr)script).CheckJsr(offset, operand, out bool noCont);
                        noContinue |= noCont;
                    } else if (op == OpDef.OpJSR_AbsLong && (mPluginCaps[i] & PluginCap.JSL) != 0) {
                        ((IPlugin_InlineJsl)script).CheckJsl(offset, operand, out bool noCont);
                        noContinue |= noCont;
                    } else if ((op == OpDef.OpBRK_Implied || op == OpDef.OpBRK_StackInt) &&
                            (mPluginCaps[i] & PluginCap.BRK) != 0) {
                        ((IPlugin_InlineBrk)script).CheckBrk(offset, op == OpDef.OpBRK_StackInt,
                            out bool noCont);
                        noContinue &= noCont;
                    }
                } catch (PluginException plex) {
                    LogW(offset, "Uncaught PluginException: " + plex.Message);
                } catch (Exception ex) {
                    LogW(offset, "Plugin threw exception: " + ex);
                }
            }
            return noContinue;
        }

        /// <summary>
        /// Sets the format of an instruction operand.
        /// </summary>
        /// <param name="offset">Offset of opcode.</param>
        /// <param name="subType">Format sub-type.</param>
        /// <param name="label">Label, for subType=Symbol.</param>
        /// <returns>True if the format was applied.</returns>
        private bool SetOperandFormat(int offset, DataSubType subType, string label) {
            if (offset <= 0 || offset > mFileData.Length) {
                throw new PluginException("SOF: bad args: offset=+" + offset.ToString("x6") +
                    " subType=" + subType + " label='" + label + "'; file length is" +
                    mFileData.Length);
            }

            // Don't overwrite existing format.
            if (mAnattribs[offset].DataDescriptor != null) {
                LogW(offset, "SOF: already have a descriptor here");
                return false;
            }

            // Must be the start of an instruction.
            if (!mAnattribs[offset].IsInstructionStart) {
                LogW(offset, "SOF: not an instruction start");
                return false;
            }

            if (subType == DataSubType.Symbol && string.IsNullOrEmpty(label)) {
                LogW(offset, "SOF rej: label required for subType=" + subType);
                return false;
            }

            FormatDescriptor.SubType subFmt = ConvertPluginSubType(subType, out bool isStringSub);
            if (subFmt == FormatDescriptor.SubType.None) {
                LogW(offset, "SOF: bad sub-type " + subType);
                return false;
            }

            int instrLen = mAnattribs[offset].Length;
            Debug.Assert(instrLen > 0);

            FormatDescriptor fd;
            if (subType == DataSubType.Symbol) {
                fd = FormatDescriptor.Create(instrLen,
                    new WeakSymbolRef(label, WeakSymbolRef.Part.Low),
                    false);
            } else {
                fd = FormatDescriptor.Create(instrLen, FormatDescriptor.Type.NumericLE, subFmt);
            }
            mAnattribs[offset].DataDescriptor = fd;
            return true;
        }

        /// <summary>
        /// Handles a set inline data format call from an extension script.
        /// </summary>
        /// <param name="offset">Offset of start of data item.</param>
        /// <param name="length">Length of data item.  Must be greater than zero.</param>
        /// <param name="type">Data type.</param>
        /// <param name="subType">Data sub-type.</param>
        /// <param name="label">Label, for type=Symbol.</param>
        private bool SetInlineDataFormat(int offset, int length, DataType type,
                DataSubType subType, string label) {
            if (offset <= 0 || length <= 0 || offset + length > mFileData.Length) {
                throw new PluginException("SIDF: bad args: offset=+" + offset.ToString("x6") +
                    " len=" + length + " type=" + type + " subType=" + subType +
                    " label='" + label + "'; file length is" + mFileData.Length);
            }

            // NOTE: might be faster to check Anattrib IsAddrRegionChange for short regions
            if (!mAddrMap.IsRangeUnbroken(offset, length)) {
                LogW(offset, "SIDF: format crosses address map boundary (len=" + length + ")");
                return false;
            }

            // Already formatted?  We only check the initial offset -- overlapping format
            // descriptors aren't strictly illegal.
            if (mAnattribs[offset].DataDescriptor != null) {
                LogW(offset, "SIDF: already have a descriptor here");
                return false;
            }

            // Don't allow formatting of any bytes that are identified as instructions or
            // were tagged by the user as something other than inline data.  If the code
            // analyzer comes crashing through later they'll just stomp on what we've done.
            for (int i = offset; i < offset + length; i++) {
                if (mAnalyzerTags[i] != AnalyzerTag.None && mAnalyzerTags[i] != AnalyzerTag.InlineData) {
                    LogW(offset, "SIDF rej: already an atag at " + i.ToString("x6") +
                        " (" + mAnalyzerTags[i] + ")");
                    return false;
                }
                if (mAnattribs[offset].IsInstruction) {
                    LogW(offset, "SIDF rej: not for use with instructions");
                    return false;
                }
            }

            //
            // Convert types to FormatDescriptor types, and do some validity checks.
            //
            FormatDescriptor.Type fmt = ConvertPluginType(type, out bool isStringType);
            FormatDescriptor.SubType subFmt = ConvertPluginSubType(subType, out bool isStringSub);

            if (type == DataType.Dense && subType != DataSubType.None) {
                throw new PluginException("SIDF rej: dense data must use subType=None");
            }
            if (type == DataType.Fill && subType != DataSubType.None) {
                throw new PluginException("SIDF rej: fill data must use subType=None");
            }

            if (isStringType && !isStringSub) {
                throw new PluginException("SIDF rej: bad type/subType combo: type=" +
                    type + " subType= " + subType);
            }
            if ((type == DataType.NumericLE || type == DataType.NumericBE) &&
                    (length < 1 || length > 4)) {
                throw new PluginException("SIDF rej: bad length for numeric item (" +
                    length + ")");
            }
            if (subType == DataSubType.Symbol && string.IsNullOrEmpty(label)) {
                throw new PluginException("SIDF rej: label required for subType=" + subType);
            }

            if (isStringType) {
                if (!DataAnalysis.VerifyStringData(mFileData, offset, length, fmt,
                        out string failMsg)) {
                    LogW(offset, failMsg);
                    return false;
                }
            } else if (type == DataType.Fill) {
                if (!VerifyFillData(offset, length)) {
                    return false;
                }
            }

            // Looks good, create a descriptor, and mark all bytes as inline data.
            FormatDescriptor fd;
            if (subType == DataSubType.Symbol) {
                fd = FormatDescriptor.Create(length,
                    new WeakSymbolRef(label, WeakSymbolRef.Part.Low),
                    type == DataType.NumericBE);
            } else {
                fd = FormatDescriptor.Create(length, fmt, subFmt);
            }
            mAnattribs[offset].DataDescriptor = fd;
            for (int i = offset; i < offset + length; i++) {
                mAnattribs[i].IsInlineData = true;
            }
            return true;
        }


        private bool VerifyFillData(int offset, int length) {
            byte first = mFileData[offset];
            while (--length != 0) {
                if (mFileData[++offset] != first) {
                    LogW(offset, "SIDF: mismatched fill data");
                    return false;
                }
            }
            return true;
        }

        private FormatDescriptor.Type ConvertPluginType(DataType pluginType,
                out bool isStringType) {
            isStringType = false;
            switch (pluginType) {
                case DataType.NumericLE:
                    return FormatDescriptor.Type.NumericLE;
                case DataType.NumericBE:
                    return FormatDescriptor.Type.NumericBE;
                case DataType.StringGeneric:
                    isStringType = true;
                    return FormatDescriptor.Type.StringGeneric;
                case DataType.StringReverse:
                    isStringType = true;
                    return FormatDescriptor.Type.StringReverse;
                case DataType.StringNullTerm:
                    isStringType = true;
                    return FormatDescriptor.Type.StringNullTerm;
                case DataType.StringL8:
                    isStringType = true;
                    return FormatDescriptor.Type.StringL8;
                case DataType.StringL16:
                    isStringType = true;
                    return FormatDescriptor.Type.StringL16;
                case DataType.StringDci:
                    isStringType = true;
                    return FormatDescriptor.Type.StringDci;
                case DataType.Fill:
                    return FormatDescriptor.Type.Fill;
                case DataType.Uninit:
                    return FormatDescriptor.Type.Uninit;
                case DataType.Dense:
                    return FormatDescriptor.Type.Dense;
                default:
                    Debug.Assert(false);
                    throw new PluginException("Instr format rej: unknown format type " + pluginType);
            }
        }

        private FormatDescriptor.SubType ConvertPluginSubType(DataSubType pluginSubType,
                out bool isStringSub) {
            isStringSub = false;
            switch (pluginSubType) {
                case DataSubType.None:
                    return FormatDescriptor.SubType.None;
                case DataSubType.Hex:
                    return FormatDescriptor.SubType.Hex;
                case DataSubType.Decimal:
                    return FormatDescriptor.SubType.Decimal;
                case DataSubType.Binary:
                    return FormatDescriptor.SubType.Binary;
                case DataSubType.Address:
                    return FormatDescriptor.SubType.Address;
                case DataSubType.Symbol:
                    return FormatDescriptor.SubType.Symbol;
                case DataSubType.Ascii:
                    isStringSub = true;
                    return FormatDescriptor.SubType.Ascii;
                case DataSubType.HighAscii:
                    isStringSub = true;
                    return FormatDescriptor.SubType.HighAscii;
                case DataSubType.C64Petscii:
                    isStringSub = true;
                    return FormatDescriptor.SubType.C64Petscii;
                case DataSubType.C64Screen:
                    isStringSub = true;
                    return FormatDescriptor.SubType.C64Screen;
                default:
                    throw new PluginException("Instr format rej: unknown sub type " + pluginSubType);
            }
        }

        #region Data Bank Register management

        /// <summary>
        /// Data Bank Register value.
        /// </summary>
        public class DbrValue {
            public const short UNKNOWN = -1;
            public const short USE_PBR = -2;

            /// <summary>
            /// If true, ignore Bank, use Program Bank Register instead.
            /// </summary>
            public bool FollowPbr;

            /// <summary>
            /// Bank number (0-255).
            /// </summary>
            public byte Bank { get; private set; }

            public enum Source { Unknown = 0, User, Auto };
            /// <summary>
            /// From whence this value originates.
            /// </summary>
            public Source ValueSource { get; private set; }

            /// <summary>
            /// Representation of the object state as a short integer.  0-255 specifies the
            /// bank, while negative values are used for special conditions.
            /// </summary>
            public short AsShort {
                get {
                    if (FollowPbr) {
                        return USE_PBR;
                    } else {
                        return Bank;
                    }
                }
            }

            public DbrValue(bool followPbr, byte bank, Source source) {
                FollowPbr = followPbr;
                Bank = bank;
                ValueSource = source;
            }

            public override string ToString() {
                return "DBR:" + (FollowPbr ? "K" : "$" + Bank.ToString("x2"));
            }

            public static bool operator ==(DbrValue a, DbrValue b) {
                if (ReferenceEquals(a, b)) {
                    return true;    // same object, or both null
                }
                if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                    return false;   // one is null
                }
                // All fields must be equal.
                return a.Bank == b.Bank && a.FollowPbr == b.FollowPbr &&
                    a.ValueSource == b.ValueSource;
            }
            public static bool operator !=(DbrValue a, DbrValue b) {
                return !(a == b);
            }
            public override bool Equals(object obj) {
                return obj is Symbol && this == (DbrValue)obj;
            }
            public override int GetHashCode() {
                return Bank + (FollowPbr ? 0x100 : 0);
            }
        }


        /// <summary>
        /// Determines the value of the Data Bank Register (DBR, register 'B') for relevant
        /// instructions, and updates the Anattrib OperandOffset value.
        /// </summary>
        /// <remarks>
        /// This is of questionable value when we have reliable relocation data.  OTOH it's
        /// pretty quick even on very large files.
        /// </remarks>
        public void ApplyDataBankRegister(Dictionary<int, DbrValue> userValues,
                Dictionary<int, DbrValue> dbrChanges) {
            Debug.Assert(!mCpuDef.HasAddr16);   // 65816 only

            dbrChanges.Clear();

            if (mAnalysisParameters.SmartPlbHandling) {
                GenerateSmartPlbChanges(dbrChanges);
            }

            // Apply the user-specified values, overwriting auto-generated values.
            foreach (KeyValuePair<int, DbrValue> kvp in userValues) {
                dbrChanges[kvp.Key] = kvp.Value;
            }

            // Create a full-file array for fast access.
            short[] bval = new short[mAnattribs.Length];
            Misc.Memset(bval, DbrValue.UNKNOWN);
            foreach (KeyValuePair<int, DbrValue> kvp in dbrChanges) {
                bval[kvp.Key] = kvp.Value.AsShort;
            }

            // Run through file, updating instructions as needed.
            short curVal = DbrValue.UNKNOWN;
            for (int offset = 0; offset < mAnattribs.Length; offset++) {
                if (mAnattribs[offset].IsNonAddressable) {
                    continue;
                }
                if (curVal == DbrValue.UNKNOWN) {
                    // On first encounter with addressable memory, init curVal so B=K.
                    curVal = (byte)(mAddrMap.OffsetToAddress(offset) >> 16);
                }
                if (bval[offset] != DbrValue.UNKNOWN) {
                    curVal = bval[offset];
                }
                if (!mAnattribs[offset].UsesDataBankReg) {
                    // Not a relevant instruction, move on to next.
                    continue;
                }
                Debug.Assert(mAnattribs[offset].IsInstructionStart);
                Debug.Assert(curVal != DbrValue.UNKNOWN);

                int bank;
                if (curVal == DbrValue.USE_PBR) {
                    bank = mAnattribs[offset].Address & 0x00ff0000;
                } else {
                    Debug.Assert(curVal >= 0 && curVal < 256);
                    bank = curVal << 16;
                }

                int newAddr = (mAnattribs[offset].OperandAddress & 0x0000ffff) | bank;
                int newOffset = mAddrMap.AddressToOffset(offset, newAddr);
                if (newAddr != mAnattribs[offset].OperandAddress ||
                        newOffset != mAnattribs[offset].OperandOffset) {
                    //Debug.WriteLine("DBR rewrite at +" + offset.ToString("x6") + ": $" +
                    //    mAnattribs[offset].OperandAddress.ToString("x6") + "/+" +
                    //    mAnattribs[offset].OperandOffset.ToString("x6") + " --> $" +
                    //    newAddr.ToString("x6") + "/+" + newOffset.ToString("x6"));

                    mAnattribs[offset].OperandAddress = newAddr;
                    mAnattribs[offset].OperandOffset = newOffset;
                }
            }
        }

        private void GenerateSmartPlbChanges(Dictionary<int, DbrValue> dbrChanges) {
#if false
            // Set B=K every time we cross an address boundary and the program bank changes.
            short prevBank = DbrValue.UNKNOWN;
            foreach (AddressMap.AddressMapEntry ent in mAddrMap) {
                short mapBank = (short)(ent.Addr >> 16);
                if (mapBank != prevBank) {
                    prevBank = mapBank;
                    dbrChanges.Add(ent.Offset, new DbrValue(false, (byte)mapBank,
                        DbrValue.Source.Auto));
                }
            }
#endif

            // Run through the file, looking for PLB.  If the preceding code was something
            // we can reliably pull a value out of, create an entry for it.
            for (int offset = 0; offset < mAnattribs.Length; offset++) {
                if (!mAnattribs[offset].IsInstructionStart) {
                    continue;
                }
                OpDef op = mCpuDef.GetOpDef(mFileData[offset]);
                if (op != OpDef.OpPLB_StackPull) {
                    continue;
                }
                if (offset < 1) {
                    continue;
                }
                // TODO(maybe): strictly speaking this is incorrect, because we're not verifying
                // that the previous bytes are at adjacent addresses in memory.  It's possible
                // somebody did a PHA or PHK at the end of a chunk of code, then started
                // assembling elsewhere with a PLB, and we'll mistakenly assign the wrong value.
                // Seems unlikely, and the penalty for getting it "wrong" is slight.
                if (!mAnattribs[offset - 1].IsInstructionStart) {
                    continue;
                }
                op = mCpuDef.GetOpDef(mFileData[offset - 1]);
                if (op == OpDef.OpPHK_StackPush) {
                    // output B=K
                    dbrChanges.Add(offset, new DbrValue(true, 0, DbrValue.Source.Auto));
                } else if (op == OpDef.OpPHA_StackPush && offset >= 4) {
                    // check for LDA imm
                    if (!mAnattribs[offset - 3].IsInstructionStart) {
                        continue;
                    }
                    op = mCpuDef.GetOpDef(mFileData[offset - 3]);
                    if (!(op == OpDef.OpLDA_ImmLongA || op == OpDef.OpLDA_Imm)) {
                        continue;
                    }

                    byte bank = mFileData[offset - 2];
                    dbrChanges.Add(offset, new DbrValue(false, bank, DbrValue.Source.Auto));
                }
            }
        }
        #endregion Data Bank Register management
    }
}