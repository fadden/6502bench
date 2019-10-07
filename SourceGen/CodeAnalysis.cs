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
    public class CodeAnalysis {
        /// <summary>
        /// Type hints are specified by the user.  The identify a region as being code
        /// or data.  The code analyzer will stop at data-hinted regions, and will
        /// process any code-hinted regions during the dead-code pass.
        /// 
        /// The hints are not used directly by the data analyzer, but the effects they
        /// have on the Anattrib array are.
        /// </summary>
        public enum TypeHint : sbyte {
            // No hint.  Default value populated in new arrays.
            NoHint = 0,

            // Byte is an instruction.  If the code analyzer doesn't find this
            // naturally, it will be scanned.
            Code,

            // Byte is inline data.  Execution continues "through" the byte.
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
        /// Reference to type hint array, one hint per byte.
        /// </summary>
        private TypeHint[] mTypeHints;

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
        /// <param name="hints">Type hints, one per byte.</param>
        /// <param name="statusFlagOverrides">Status flag overrides for instruction-start
        ///    bytes.</param>
        /// <param name="entryFlags">Status flags to use at code entry points.</param>
        /// <param name="scriptMan">Extension script manager.</param>
        /// <param name="debugLog">Object that receives debug log messages.</param>
        public CodeAnalysis(byte[] data, CpuDef cpuDef, Anattrib[] anattribs,
                AddressMap addrMap, TypeHint[] hints, StatusFlags[] statusFlagOverrides,
                StatusFlags entryFlags, ProjectProperties.AnalysisParameters parms,
                ScriptManager scriptMan, DebugLog debugLog) {
            mFileData = data;
            mCpuDef = cpuDef;
            mAnattribs = anattribs;
            mAddrMap = addrMap;
            mTypeHints = hints;
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

            // Set the "is data" and "is inline data" flags on anything that the user has
            // flagged as being such.  This tells us to stop processing or skip over bytes
            // as we work.  We don't need to flag code hints explicitly for analysis, but
            // we want to be able to display the flags in the info window.
            //
            // The data recognizers may spot additional inline data offsets as we work.  This
            // can cause a race if it mis-identifies code that is also a branch target;
            // whichever marks the code first will win.
            UnpackTypeHints();

            // Find starting place, based on type hints.
            // We only set the "visited" flag on the instruction start, so if the user
            // puts a code hint in the middle of an instruction, we will find it and
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
            // The AddressMap will have at least one entry, will start at offset 0, and
            // will exactly span the file.
            foreach (AddressMap.AddressMapEntry ent in mAddrMap) {
                int addr = ent.Addr;
                for (int i = ent.Offset; i < ent.Offset + ent.Length; i++) {
                    mAnattribs[i].Address = addr++;
                }
            }
        }

        /// <summary>
        /// Sets the "is xxxxx" flags on type-hinted entries, so that the code analyzer
        /// can find them easily.
        /// </summary>
        private void UnpackTypeHints() {
            Debug.Assert(mTypeHints.Length == mAnattribs.Length);
            int offset = 0;
            foreach (TypeHint hint in mTypeHints) {
                switch (hint) {
                    case TypeHint.Code:
                        // Set the IsInstruction flag to prevent inline data from being
                        // placed here.
                        OpDef op = mCpuDef.GetOpDef(mFileData[offset]);
                        if (op == OpDef.OpInvalid) {
                            LogI(offset, "Ignoring code hint on illegal opcode");
                        } else {
                            mAnattribs[offset].IsHinted = true;
                            mAnattribs[offset].IsInstruction = true;
                        }
                        break;
                    case TypeHint.Data:
                        // Tells the code analyzer to stop.  Does not define a data analyzer
                        // "uncategorized data" boundary.
                        mAnattribs[offset].IsHinted = true;
                        mAnattribs[offset].IsData = true;
                        break;
                    case TypeHint.InlineData:
                        // Tells the code analyzer to walk across these.
                        mAnattribs[offset].IsHinted = true;
                        mAnattribs[offset].IsInlineData = true;
                        break;
                    case TypeHint.NoHint:
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
                offset++;
            }
        }

        /// <summary>
        /// Finds the first offset that is hinted as code but hasn't yet been visited.
        /// 
        /// This might be in the middle of an already-visited instruction.
        /// </summary>
        /// <param name="start">Offset at which to start the search.</param>
        /// <returns>Offset found.</returns>
        private int FindFirstUnvisitedInstruction(int start) {
            for (int i = start; i < mAnattribs.Length; i++) {
                if (mAnattribs[i].IsHinted && mTypeHints[i] == TypeHint.Code &&
                        !mAnattribs[i].IsVisited) {
                    LogD(i, "Unvisited code hint");
                    if (mAnattribs[i].IsData || mAnattribs[i].IsInlineData) {
                        // Maybe the user put a code hint on something that was
                        // later recognized as inline data?  Shouldn't have been allowed.
                        LogW(i, "Weird: code hint on data/inline");
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
                    // start in an inline area because the hint is wrong.  However, it's possible
                    // for a JSR to a new section to be registered, and then before we get to
                    // it an extension script formats the area as inline data.  In that case
                    // the inline data "wins", and we stop here.
                    LogW(offset, "Code ran into inline data section");
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
                if (mAnattribs[offset + instrLen -1].Address !=
                        mAnattribs[offset].Address + instrLen - 1) {
                    // Address change happened mid-instruction.  Mark it as data.
                    LogW(offset, "Detected address change mid-instruction");
                    mAnattribs[offset].IsInstructionStart = false;
                    mAnattribs[offset].IsInstruction = false;
                    mAnattribs[offset].IsData = true;
                    return;
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
                // (To cause this, hint a 3-byte instruction as data/inline-data, then
                // hint the first byte of the instruction as code.)
                mAnattribs[offset].IsInstructionStart = true;
                mAnattribs[offset].Length = instrLen;
                for (int i = offset; i < offset + instrLen; i++) {
                    if (mAnattribs[i].IsData) {
                        LogW(i, "Stripping mid-instruction data flag");
                        mAnattribs[i].IsData = false;
                    } else if (mAnattribs[i].IsInlineData) {
                        LogW(i, "Stripping mid-instruction inline-data flag");
                        mAnattribs[i].IsInlineData = false;
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

                // On first visit, check for BRK inline call.
                if (firstVisit) {
                    if (op == OpDef.OpBRK_Implied) {
                        bool noContinue = CheckForInlineCall(op, offset, !doContinue);
                        if (!noContinue) {
                            // We're expected to continue execution past the BRK.
                            doContinue = true;
                        }
                    }
                }

                if (!doContinue) {
                    mAnattribs[offset].DoesNotContinue = true;
                    break;
                } else {
                    mAnattribs[offset].DoesNotContinue = false;
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

                // On first visit, check for JSR/JSL inline call.
                if (firstVisit) {
                    // Currently ignoring OpDef.OpJSR_AbsIndexXInd
                    if (op == OpDef.OpJSR_Abs || op == OpDef.OpJSR_AbsLong) {
                        bool noContinue = CheckForInlineCall(op, offset, false);
                        if (noContinue) {
                            LogD(offset, "Script declared inline call no-continue");
                            mAnattribs[offset].DoesNotContinue = true;
                            break;
                        }
                    }
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

                // Make sure we don't "continue" across an ORG.
                // NOTE: it's possible to do some crazy things with multiple ORGs that will
                // cause us to misinterpret things, but I don't think that matters.  What's
                // important is that the code analyzer doesn't drive into a data area.
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
        /// <param name="plpOffset">Offset of PLP instruction.</param>
        /// <returns>Best guess at status flags.</returns>
        private StatusFlags GuessFlagsForPLP(int plpOffset) {
            // We're not tracking stack contents or register contents, so this just
            // generally won't work.  However, there's a lot of code that uses PHP to
            // save the current state and PLP to restore it, so if we can find a nearby
            // PHP we can just grab from that.
            //
            // Failing that, we mark all flags as "indeterminate" and let the user sort
            // out what it should be.  It's unlikely to matter except for M/X flags on
            // the 65816.
            //
            // The emulation flag is not part of the status register, even if we do carry
            // it around like one.  The E-flag is always carried over from the previous
            // instruction.

            int backOffsetLimit = plpOffset - 128;      // arbitrary 128-byte reach
            if (backOffsetLimit < 0) {
                backOffsetLimit = 0;
            }
            StatusFlags flags = StatusFlags.AllIndeterminate;
            if (mAnalysisParameters.SmartPlpHandling) {
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
        /// For PC-relative operands (e.g. branches) it's tempting to simply adjust the file
        /// offset by the specified amount and convert that to an address.  If the file
        /// has multiple ORGs, this can produce incorrect results.  We need to convert the
        /// opcode's offset to an address, adjust by the operand, and then find the file
        /// offset that corresponds to the target address.
        /// 
        /// Doesn't do anything with immediate data.
        /// </summary>
        /// <param name="offset">Offset of the instruction opcode.</param>
        /// <param name="op">Opcode being handled. (Passed in because the caller has it
        ///   handy.)</param>
        private void DecodeOperandAddress(int offset, OpDef op) {
            //StatusFlags flags = mAnattribs[offset].StatusFlags;

            int operand = op.GetOperand(mFileData, offset, mAnattribs[offset].StatusFlags);

            // Add the bank to get a 24-bit address.  We're currently using the program bank
            // (K) rather than the data bank (B), which is correct for absolute and relative
            // branches but wrong for 16-bit data operations.  We currently have no way to
            // know what the value of B is, so we use K because there's some small chance
            // of it being correct.
            // TODO(someday): figure out how to get the correct value for the B reg
            int bank = mAnattribs[offset].Address & 0x7fff0000;

            // Extract target address.
            switch (op.AddrMode) {
                // These might refer to a location in the file, or might be external.
                case OpDef.AddressMode.Abs:
                case OpDef.AddressMode.AbsIndexX:
                case OpDef.AddressMode.AbsIndexY:
                case OpDef.AddressMode.AbsIndexXInd:
                case OpDef.AddressMode.AbsInd:
                case OpDef.AddressMode.AbsIndLong:
                case OpDef.AddressMode.StackAbs:
                    mAnattribs[offset].OperandAddress = operand | bank;
                    break;
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
                    // 24-bit address, don't add bank
                    mAnattribs[offset].OperandAddress = operand;
                    break;
                case OpDef.AddressMode.PCRel:   // rel operand; convert to absolute addr
                    mAnattribs[offset].OperandAddress =
                        Asm65.Helper.RelOffset8(mAnattribs[offset].Address,
                            (sbyte)operand) | bank;
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
                    // of the operand width.
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
        /// </summary>
        /// <param name="op">Instruction being examined.</param>
        /// <param name="offset">File offset of start of instruction.</param>
        /// <param name="noContinue">Set if any plugin declares the call to be no-continue.</param>
        /// <returns>Updated value for noContinue.</returns>
        private bool CheckForInlineCall(OpDef op, int offset, bool noContinue) {
            for (int i = 0; i < mScriptArray.Length; i++) {
                try {
                    IPlugin script = mScriptArray[i];
                    // The IPlugin object is a MarshalByRefObject, which doesn't define the
                    // interface directly.  A simple test showed it was fairly quick when the
                    // interface was implemented but a bit slow when it wasn't.  For performance
                    // we query the capability flags instead.
                    if (op == OpDef.OpJSR_Abs && (mPluginCaps[i] & PluginCap.JSR) != 0) {
                        ((IPlugin_InlineJsr)script).CheckJsr(offset, out bool noCont);
                        noContinue |= noCont;
                    } else if (op == OpDef.OpJSR_AbsLong && (mPluginCaps[i] & PluginCap.JSL) != 0) {
                        ((IPlugin_InlineJsl)script).CheckJsl(offset, out bool noCont);
                        noContinue |= noCont;
                    } else if (op == OpDef.OpBRK_Implied && (mPluginCaps[i] & PluginCap.BRK) != 0) {
                        ((IPlugin_InlineBrk)script).CheckBrk(offset, out bool noCont);
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

            if (!mAddrMap.IsContiguous(offset, length)) {
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
            // were hinted by the user as something other than inline data.  If the code
            // analyzer comes crashing through later they'll just stomp on what we've done.
            for (int i = offset; i < offset + length; i++) {
                if (mTypeHints[i] != TypeHint.NoHint && mTypeHints[i] != TypeHint.InlineData) {
                    LogW(offset, "SIDF rej: already a hint at " + i.ToString("x6") +
                        " (" + mTypeHints[i] + ")");
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
    }
}