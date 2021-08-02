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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using Asm65;
using CommonUtil;

namespace SourceGen.AsmGen {
    public class GenCommon {
        /// <summary>
        /// Generates assembly source.
        /// 
        /// This code is common to all generators.
        /// </summary>
        /// <param name="gen">Reference to generator object (presumably the caller).</param>
        /// <param name="sw">Text output sink.</param>
        /// <param name="worker">Background worker object, for progress updates and
        ///   cancelation requests.</param>
        public static void Generate(IGenerator gen, StreamWriter sw, BackgroundWorker worker) {
            DisasmProject proj = gen.Project;
            Formatter formatter = gen.SourceFormatter;
            int offset = gen.StartOffset;

            bool doAddCycles = gen.Settings.GetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS, false);

            LocalVariableLookup lvLookup = new LocalVariableLookup(proj.LvTables, proj,
                gen.Localizer.LabelMap, gen.Quirks.LeadingUnderscoreSpecial,
                gen.Quirks.NoRedefinableSymbols);

            GenerateHeader(gen, sw);

            // Used for M/X flag tracking.
            StatusFlags prevFlags = StatusFlags.AllIndeterminate;

            int lastProgress = 0;

            while (offset < proj.FileData.Length) {
                Anattrib attr = proj.GetAnattrib(offset);

                if (attr.IsInstructionStart && offset > 0 &&
                        proj.GetAnattrib(offset - 1).IsData) {
                    // Transition from data to code.  (Don't add blank line for inline data.)
                    gen.OutputLine(string.Empty);
                }

                // Long comments come first.
                if (proj.LongComments.TryGetValue(offset, out MultiLineComment longComment)) {
                    List<string> formatted = longComment.FormatText(formatter, string.Empty);
                    foreach (string str in formatted) {
                        gen.OutputLine(str);
                    }
                }

                // Check for address change.
                int orgAddr = proj.AddrMap.Get(offset);
                if (orgAddr >= 0) {
                    gen.OutputOrgDirective(offset, orgAddr);
                }

                List<DefSymbol> lvars = lvLookup.GetVariablesDefinedAtOffset(offset);
                if (lvars != null) {
                    // table defined here
                    gen.OutputLocalVariableTable(offset, lvars,
                        lvLookup.GetMergedTableAtOffset(offset));
                }

                if (attr.IsInstructionStart) {
                    // Generate M/X reg width directive, if necessary.
                    // NOTE: we can suppress the initial directive if we know what the
                    // target assembler's default assumption is.  Probably want to handle
                    // that in the ORG output handler.
                    if (proj.CpuDef.HasEmuFlag) {
                        StatusFlags curFlags = attr.StatusFlags;
                        curFlags.M = attr.StatusFlags.IsShortM ? 1 : 0;
                        curFlags.X = attr.StatusFlags.IsShortX ? 1 : 0;
                        if (curFlags.M != prevFlags.M || curFlags.X != prevFlags.X) {
                            // changed, output directive
                            gen.OutputRegWidthDirective(offset, prevFlags.M, prevFlags.X,
                                curFlags.M, curFlags.X);
                        }

                        prevFlags = curFlags;
                    }

                    // Look for embedded instructions.
                    int len;
                    for (len = 1; len < attr.Length; len++) {
                        if (proj.GetAnattrib(offset + len).IsInstructionStart) {
                            break;
                        }
                    }

                    // Output instruction.
                    GenerateInstruction(gen, sw, lvLookup, offset, len, doAddCycles);

                    if (attr.DoesNotContinue) {
                        gen.OutputLine(string.Empty);
                    }

                    offset += len;
                } else {
                    gen.OutputDataOp(offset);
                    offset += attr.Length;
                }

                // Update progress meter.  We don't want to spam it, so just ping it 10x.
                int curProgress = (offset * 10) / proj.FileData.Length;
                if (lastProgress != curProgress) {
                    if (worker.CancellationPending) {
                        Debug.WriteLine("GenCommon got cancellation request");
                        return;
                    }
                    lastProgress = curProgress;
                    worker.ReportProgress(curProgress * 10);
                    //System.Threading.Thread.Sleep(500);
                }
            }
        }

        private static void GenerateHeader(IGenerator gen, StreamWriter sw) {
            DisasmProject proj = gen.Project;
            Formatter formatter = gen.SourceFormatter;

            // Check for header comment.
            if (proj.LongComments.TryGetValue(LineListGen.Line.HEADER_COMMENT_OFFSET,
                    out MultiLineComment headerComment)) {
                List<string> formatted = headerComment.FormatText(formatter, string.Empty);
                foreach (string str in formatted) {
                    gen.OutputLine(str);
                }
            }

            gen.OutputAsmConfig();

            // Format symbols.
            bool prevConst = false;
            foreach (DefSymbol defSym in proj.ActiveDefSymbolList) {
                if (prevConst && !defSym.IsConstant) {
                    // Output a blank line between the constants and the address equates.
                    gen.OutputLine(string.Empty);
                }
                // Use an operand length of 1 so values are shown as concisely as possible.
                string valueStr = PseudoOp.FormatNumericOperand(formatter, proj.SymbolTable,
                    gen.Localizer.LabelMap, defSym.DataDescriptor, defSym.Value, 1,
                    PseudoOp.FormatNumericOpFlags.OmitLabelPrefixSuffix);
                string labelStr = gen.Localizer.ConvLabel(defSym.Label);
                gen.OutputEquDirective(labelStr, valueStr, defSym.Comment);

                prevConst = defSym.IsConstant;
            }

            // If there was at least one symbol, output a blank line.
            if (proj.ActiveDefSymbolList.Count != 0) {
                gen.OutputLine(string.Empty);
            }
        }

        private static void GenerateInstruction(IGenerator gen, StreamWriter sw,
                LocalVariableLookup lvLookup, int offset, int instrBytes, bool doAddCycles) {
            DisasmProject proj = gen.Project;
            Formatter formatter = gen.SourceFormatter;
            byte[] data = proj.FileData;
            Anattrib attr = proj.GetAnattrib(offset);

            string labelStr = string.Empty;
            if (attr.Symbol != null) {
                labelStr = gen.Localizer.ConvLabel(attr.Symbol.Label);
            }

            OpDef op = proj.CpuDef.GetOpDef(data[offset]);
            int operand = op.GetOperand(data, offset, attr.StatusFlags);
            int instrLen = op.GetLength(attr.StatusFlags);
            OpDef.WidthDisambiguation wdis = OpDef.WidthDisambiguation.None;
            if (op.IsWidthPotentiallyAmbiguous) {
                wdis = OpDef.GetWidthDisambiguation(instrLen, operand);
            }
            if (gen.Quirks.SinglePassAssembler && wdis == OpDef.WidthDisambiguation.None &&
                    (op.AddrMode == OpDef.AddressMode.DP ||
                        op.AddrMode == OpDef.AddressMode.DPIndexX) ||
                        op.AddrMode == OpDef.AddressMode.DPIndexY) {
                // Could be a forward reference to a direct-page label.  For ACME, we don't
                // care if it's forward or not.
                if ((gen.Quirks.SinglePassNoLabelCorrection && IsLabelReference(gen, offset)) ||
                        IsForwardLabelReference(gen, offset)) {
                    wdis = OpDef.WidthDisambiguation.ForceDirect;
                }
            }
            if (wdis == OpDef.WidthDisambiguation.ForceLongMaybe &&
                    gen.Quirks.SinglePassAssembler &&
                    IsForwardLabelReference(gen, offset)) {
                // Assemblers like cc65 can't tell if a symbol reference is Absolute or
                // Long if they haven't seen the symbol yet.  Irrelevant for ACME, which
                // doesn't currently handle 65816 outside bank 0.
                wdis = OpDef.WidthDisambiguation.ForceLong;
            }

            string opcodeStr = formatter.FormatOpcode(op, wdis);

            string formattedOperand = null;
            int operandLen = instrLen - 1;
            PseudoOp.FormatNumericOpFlags opFlags = PseudoOp.FormatNumericOpFlags.OmitLabelPrefixSuffix;
            bool isPcRelBankWrap = false;

            // Tweak branch instructions.  We want to show the absolute address rather
            // than the relative offset (which happens with the OperandAddress assignment
            // below), and 1-byte branches should always appear as a 4-byte hex value.
            // Unless we're outside bank 0 on 65816, in which case most assemblers require
            // them to be 6-byte hex values.
            if (op.AddrMode == OpDef.AddressMode.PCRel ||
                    op.AddrMode == OpDef.AddressMode.DPPCRel) {
                Debug.Assert(attr.OperandAddress >= 0);
                operandLen = 2;
                opFlags |= PseudoOp.FormatNumericOpFlags.IsPcRel;
            } else if (op.AddrMode == OpDef.AddressMode.PCRelLong ||
                    op.AddrMode == OpDef.AddressMode.StackPCRelLong) {
                opFlags |= PseudoOp.FormatNumericOpFlags.IsPcRel;
            } else if (op.AddrMode == OpDef.AddressMode.Imm ||
                    op.AddrMode == OpDef.AddressMode.ImmLongA ||
                    op.AddrMode == OpDef.AddressMode.ImmLongXY) {
                opFlags |= PseudoOp.FormatNumericOpFlags.HasHashPrefix;
            }
            if ((opFlags & PseudoOp.FormatNumericOpFlags.IsPcRel) != 0) {
                int branchDist = attr.Address - attr.OperandAddress;
                isPcRelBankWrap = branchDist > 32767 || branchDist < -32768;
            }
            if (op.IsAbsolutePBR) {
                opFlags |= PseudoOp.FormatNumericOpFlags.IsAbsolutePBR;
            }

            // 16-bit operands outside bank 0 need to include the bank when computing
            // symbol adjustment.
            int operandForSymbol = operand;
            if (attr.OperandAddress >= 0) {
                operandForSymbol = attr.OperandAddress;
            }

            // Check Length to watch for bogus descriptors.  (ApplyFormatDescriptors() should
            // now be screening bad descriptors out, so we may not need the Length test.)
            if (attr.DataDescriptor != null && attr.Length == attr.DataDescriptor.Length) {
                FormatDescriptor dfd = gen.ModifyInstructionOperandFormat(offset,
                    attr.DataDescriptor, operand);

                // Format operand as directed.
                if (op.AddrMode == OpDef.AddressMode.BlockMove) {
                    // Special handling for the double-operand block move.
                    string opstr1 = PseudoOp.FormatNumericOperand(formatter, proj.SymbolTable,
                        gen.Localizer.LabelMap, dfd, operand >> 8, 1,
                        PseudoOp.FormatNumericOpFlags.OmitLabelPrefixSuffix);
                    string opstr2 = PseudoOp.FormatNumericOperand(formatter, proj.SymbolTable,
                        gen.Localizer.LabelMap, dfd, operand & 0xff, 1,
                        PseudoOp.FormatNumericOpFlags.OmitLabelPrefixSuffix);
                    if (gen.Quirks.BlockMoveArgsReversed) {
                        string tmp = opstr1;
                        opstr1 = opstr2;
                        opstr2 = tmp;
                    }
                    string hash = gen.Quirks.BlockMoveArgsNoHash ? "" : "#";
                    formattedOperand = hash + opstr1 + "," + hash + opstr2;
                } else if (op.AddrMode == OpDef.AddressMode.DPPCRel) {
                    // Special handling for double-operand BBR/BBS.  The instruction generally
                    // behaves like a branch, so format that first.
                    string branchStr = PseudoOp.FormatNumericOperand(formatter,
                        proj.SymbolTable, gen.Localizer.LabelMap, dfd,
                        operandForSymbol, operandLen, opFlags);
                    string dpStr = formatter.FormatHexValue(operand & 0xff, 2);
                    formattedOperand = dpStr + "," + branchStr;
                } else {
                    if (attr.DataDescriptor.IsStringOrCharacter) {
                        gen.UpdateCharacterEncoding(dfd);
                    }
                    formattedOperand = PseudoOp.FormatNumericOperand(formatter, proj.SymbolTable,
                        lvLookup, gen.Localizer.LabelMap, dfd,
                        offset, operandForSymbol, operandLen, opFlags);
                }
            } else {
                // Show operand value in hex.
                if (op.AddrMode == OpDef.AddressMode.BlockMove) {
                    int arg1, arg2;
                    if (gen.Quirks.BlockMoveArgsReversed) {
                        arg1 = operand & 0xff;
                        arg2 = operand >> 8;
                    } else {
                        arg1 = operand >> 8;
                        arg2 = operand & 0xff;
                    }
                    string hash = gen.Quirks.BlockMoveArgsNoHash ? "" : "#";
                    formattedOperand = hash + formatter.FormatHexValue(arg1, 2) + "," +
                        hash + formatter.FormatHexValue(arg2, 2);
                } else if (op.AddrMode == OpDef.AddressMode.DPPCRel) {
                    formattedOperand = formatter.FormatHexValue(operand & 0xff, 2) + "," +
                        formatter.FormatHexValue(operandForSymbol, operandLen * 2);
                } else {
                    if (operandLen == 2 && !(op.IsAbsolutePBR && gen.Quirks.Need24BitsForAbsPBR) &&
                            (opFlags & PseudoOp.FormatNumericOpFlags.IsPcRel) == 0) {
                        // This is necessary for 16-bit operands, like "LDA abs" and "PEA val",
                        // when outside bank zero.  The bank is included in the operand address,
                        // but we don't want to show it here.  We may need it for JSR/JMP though,
                        // and the bank is required for relative branch instructions.
                        operandForSymbol &= 0xffff;
                    }
                    formattedOperand = formatter.FormatHexValue(operandForSymbol, operandLen * 2);
                }
            }
            string operandStr = formatter.FormatOperand(op, formattedOperand, wdis);

            if (gen.Quirks.StackIntOperandIsImmediate &&
                    op.AddrMode == OpDef.AddressMode.StackInt) {
                // COP $02 is standard, but some require COP #$02
                operandStr = '#' + operandStr;
            }

            // The BBR/BBS/RMB/SMB instructions include a bit index (0-7).  The standard way is
            // to make it part of the mnemonic, but some assemblers make it an argument.
            if (gen.Quirks.BitNumberIsArg && op.IsNumberedBitOp) {
                // Easy way: do some string manipulation.
                char bitIndex = opcodeStr[opcodeStr.Length - 1];
                opcodeStr = opcodeStr.Substring(0, opcodeStr.Length - 1);
                operandStr = bitIndex.ToString() + "," + operandStr;
            }

            string eolComment = proj.Comments[offset];
            if (doAddCycles) {
                bool branchCross = (attr.Address & 0xff00) != (operandForSymbol & 0xff00);
                int cycles = proj.CpuDef.GetCycles(op.Opcode, attr.StatusFlags, attr.BranchTaken,
                    branchCross);
                if (cycles > 0) {
                    if (!string.IsNullOrEmpty(eolComment)) {
                        eolComment = cycles.ToString() + "  " + eolComment;
                    } else {
                        eolComment = cycles.ToString();
                    }
                } else {
                    if (!string.IsNullOrEmpty(eolComment)) {
                        eolComment = (-cycles).ToString() + "+ " + eolComment;
                    } else {
                        eolComment = (-cycles).ToString() + "+";
                    }
                }
            }
            string commentStr = formatter.FormatEolComment(eolComment);

            string replMnemonic = gen.ModifyOpcode(offset, op);
            if (attr.Length != instrBytes) {
                // This instruction has another instruction inside it.  Throw out what we
                // computed and just output as bytes.
                gen.GenerateShortSequence(offset, instrBytes, out opcodeStr, out operandStr);
            } else if (isPcRelBankWrap && gen.Quirks.NoPcRelBankWrap) {
                // Some assemblers have trouble generating PC-relative operands that wrap
                // around the bank.  Output as raw hex.
                gen.GenerateShortSequence(offset, instrBytes, out opcodeStr, out operandStr);
            } else if (op.AddrMode == OpDef.AddressMode.BlockMove &&
                    gen.Quirks.BlockMoveArgsReversed) {
                // On second thought, just don't even output the wrong thing.
                gen.GenerateShortSequence(offset, instrBytes, out opcodeStr, out operandStr);
            } else if (replMnemonic == null) {
                // No mnemonic exists for this opcode.
                gen.GenerateShortSequence(offset, instrBytes, out opcodeStr, out operandStr);
            } else if (replMnemonic != string.Empty) {
                // A replacement mnemonic has been provided.
                opcodeStr = formatter.FormatMnemonic(replMnemonic, wdis);
            }
            gen.OutputLine(labelStr, opcodeStr, operandStr, commentStr);

            // Assemblers like Merlin32 try to be helpful and track SEP/REP, but they do the
            // wrong thing if we're in emulation mode.  Force flags back to short.
            if (proj.CpuDef.HasEmuFlag && gen.Quirks.TracksSepRepNotEmu && op == OpDef.OpREP_Imm) {
                if ((operand & 0x30) != 0 && attr.StatusFlags.IsEmulationMode) {
                    gen.OutputRegWidthDirective(offset, 0, 0, 1, 1);
                }
            }
        }

        /// <summary>
        /// Determines whether the instruction at the specified offset has an operand that is
        /// a forward reference.  This only matters for single-pass assemblers.
        /// </summary>
        /// <param name="gen">Source generator reference.</param>
        /// <param name="offset">Offset of instruction opcode.</param>
        /// <returns>True if the instruction's operand is a forward reference to a label.</returns>
        private static bool IsForwardLabelReference(IGenerator gen, int offset) {
            return (GetLabelOffsetFromOperand(gen, offset) > offset);
        }

        /// <summary>
        /// Determines whether the instruction at the specified offset has an operand
        /// that references a symbol.
        /// </summary>
        /// <param name="gen">Source generator reference.</param>
        /// <param name="offset">Offset of instruction opcode.</param>
        /// <returns>True if the instruction's operand is a forward reference to a label.</returns>
        private static bool IsLabelReference(IGenerator gen, int offset) {
            return (GetLabelOffsetFromOperand(gen, offset) >= 0);
        }

        /// <summary>
        /// Determines the offset of the label that the operand's symbol references.
        /// </summary>
        /// <param name="gen">Source generator reference.</param>
        /// <param name="offset">Offset of instruction opcode.</param>
        /// <returns>The offset of the label, or -1 if the operand isn't a symbolic reference
        ///   to a known label.</returns>
        private static int GetLabelOffsetFromOperand(IGenerator gen, int offset) {
            DisasmProject proj = gen.Project;
            Debug.Assert(proj.GetAnattrib(offset).IsInstructionStart);

            FormatDescriptor dfd = proj.GetAnattrib(offset).DataDescriptor;
            if (dfd == null || !dfd.HasSymbol) {
                return -1;
            }
            return proj.FindLabelOffsetByName(dfd.SymbolRef.Label);
        }

        /// <summary>
        /// Configures some common format config items from the app settings.  Uses a
        /// passed-in settings object, rather than the global settings.
        /// </summary>
        /// <param name="settings">Application settings.</param>
        /// <param name="config">Format config struct.</param>
        public static void ConfigureFormatterFromSettings(AppSettings settings,
                ref Formatter.FormatConfig config) {
            config.mUpperHexDigits =
                settings.GetBool(AppSettings.FMT_UPPER_HEX_DIGITS, false);
            config.mUpperOpcodes =
                settings.GetBool(AppSettings.FMT_UPPER_OP_MNEMONIC, false);
            config.mUpperPseudoOpcodes =
                settings.GetBool(AppSettings.FMT_UPPER_PSEUDO_OP_MNEMONIC, false);
            config.mUpperOperandA =
                settings.GetBool(AppSettings.FMT_UPPER_OPERAND_A, false);
            config.mUpperOperandS =
                settings.GetBool(AppSettings.FMT_UPPER_OPERAND_S, false);
            config.mUpperOperandXY =
                settings.GetBool(AppSettings.FMT_UPPER_OPERAND_XY, false);
            config.mSpacesBetweenBytes =
                settings.GetBool(AppSettings.FMT_SPACES_BETWEEN_BYTES, false);
            config.mAddSpaceLongComment =
                settings.GetBool(AppSettings.FMT_ADD_SPACE_FULL_COMMENT, true);
            config.mOperandWrapLen =
                settings.GetInt(AppSettings.FMT_OPERAND_WRAP_LEN, 0);

            config.mForceAbsOpcodeSuffix =
                settings.GetString(AppSettings.FMT_OPCODE_SUFFIX_ABS, string.Empty);
            config.mForceLongOpcodeSuffix =
                settings.GetString(AppSettings.FMT_OPCODE_SUFFIX_LONG, string.Empty);
            config.mForceAbsOperandPrefix =
                settings.GetString(AppSettings.FMT_OPERAND_PREFIX_ABS, string.Empty);
            config.mForceLongOperandPrefix =
                settings.GetString(AppSettings.FMT_OPERAND_PREFIX_LONG, string.Empty);

            string exprMode = settings.GetString(AppSettings.FMT_EXPRESSION_MODE, string.Empty);
            config.mExpressionMode = Formatter.FormatConfig.ParseExpressionMode(exprMode);

            // Not doing the delimiter patterns here, because what's in the config file is
            // intended for on-screen display, and hence likely to be unsuited for an assembler.

            // Ditto for the local variable prefix.
        }

        /// <summary>
        /// Checks to see if the junk alignment directive is compatible with the actual
        /// address.  This is used to screen out alignment values that no longer match up
        /// with the actual addresses.
        /// </summary>
        /// <param name="offset">File offset of directive.</param>
        /// <param name="dfd">Format descriptor.</param>
        /// <param name="addrMap">Offset to address map.</param>
        /// <returns>True if the .junk alignment directive is correct, false if it's
        ///   incorrect or not an alignment sub-type (e.g. None).</returns>
        public static bool CheckJunkAlign(int offset, FormatDescriptor dfd,
                CommonUtil.AddressMap addrMap) {
            Debug.Assert(dfd.FormatType == FormatDescriptor.Type.Junk);
            if (dfd.FormatSubType == FormatDescriptor.SubType.None) {
                return false;
            }
            Debug.Assert(dfd.IsAlignedJunk);

            // Just check the address.  Shouldn't need to check the length.
            int lastOffset = offset + dfd.Length - 1;
            int alignToAddr = addrMap.OffsetToAddress(lastOffset) + 1;
            int alignPwr = FormatDescriptor.AlignmentToPower(dfd.FormatSubType);
            int alignMask = (1 << alignPwr) - 1;
            bool result = (alignToAddr & alignMask) == 0;
            //Debug.WriteLine(dfd.FormatSubType + " at +" + offset.ToString("x6") +
            //    "(" + alignToAddr.ToString("x4") + "): " + result);
            return result;
        }

        /// <summary>
        /// Determines whether the project appears to have a PRG header.
        /// </summary>
        /// <param name="project">Project to check.</param>
        /// <returns>True if we think we found a PRG header.</returns>
        public static bool HasPrgHeader(DisasmProject project) {
            if (project.FileDataLength < 3 || project.FileDataLength > 65536+2) {
                // Must fit in 64KB of memory.  A 65538-byte file will work if the
                // first two bytes are the PRG header (and it starts at address zero).
                //Debug.WriteLine("PRG test: incompatible file length");
                return false;
            }
            Anattrib attr0 = project.GetAnattrib(0);
            Anattrib attr1 = project.GetAnattrib(1);
            if (!(attr0.IsDataStart && attr1.IsData)) {
                //Debug.WriteLine("PRG test: +0/1 not data");
                return false;
            }
            if (attr0.Length != 2) {
                //Debug.WriteLine("PRG test: +0/1 not 16-bit value");
                return false;
            }
            if (attr0.Symbol != null || attr1.Symbol != null) {
                //Debug.WriteLine("PRG test: +0/1 has label");
                return false;
            }
            // Confirm there's an address map entry at offset 2.
            if (project.AddrMap.Get(2) == AddressMap.NO_ENTRY_ADDR) {
                //Debug.WriteLine("PRG test: no ORG at +2");
                return false;
            }
            // See if the address at offset 2 matches the value at 0/1.
            int value01 = project.FileData[0] | (project.FileData[1] << 8);
            int addr2 = project.AddrMap.OffsetToAddress(2);
            if (value01 != addr2) {
                //Debug.WriteLine("PRG test: +0/1 value is " + value01.ToString("x4") +
                //    ", address at +2 is " + addr2);
                return false;
            }

            // TODO? confirm project fits in 64K of memory

            return true;
        }
    }
}
