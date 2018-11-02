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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using Asm65;

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
            int offset = 0;

            bool doAddCycles = gen.Settings.GetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS, false);

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

                if (attr.IsInstructionStart) {
                    // Generate M/X reg width directive, if necessary.
                    // NOTE: we can suppress the initial directive if we know what the
                    // target assembler's default assumption is.  Probably want to handle
                    // that in the ORG output handler.
                    if (proj.CpuDef.HasEmuFlag) {
                        StatusFlags curFlags = attr.StatusFlags;
                        curFlags.M = attr.StatusFlags.ShortM ? 1 : 0;
                        curFlags.X = attr.StatusFlags.ShortX ? 1 : 0;
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
                    GenerateInstruction(gen, sw, offset, len, doAddCycles);

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
            if (proj.LongComments.TryGetValue(DisplayList.Line.HEADER_COMMENT_OFFSET,
                    out MultiLineComment headerComment)) {
                List<string> formatted = headerComment.FormatText(formatter, string.Empty);
                foreach (string str in formatted) {
                    gen.OutputLine(str);
                }
            }

            gen.OutputAsmConfig();

            // Format symbols.
            foreach (DefSymbol defSym in proj.ActiveDefSymbolList) {
                // Use an operand length of 1 so things are shown as concisely as possible.
                string valueStr = PseudoOp.FormatNumericOperand(formatter, proj.SymbolTable,
                    gen.Localizer.LabelMap, defSym.DataDescriptor, defSym.Value, 1,
                    PseudoOp.FormatNumericOpFlags.None);
                gen.OutputEquDirective(defSym.Label, valueStr, defSym.Comment);
            }

            // If there was at least one symbol, output a blank line.
            if (proj.ActiveDefSymbolList.Count != 0) {
                gen.OutputLine(string.Empty);
            }
        }

        private static void GenerateInstruction(IGenerator gen, StreamWriter sw, int offset,
                int instrBytes, bool doAddCycles) {
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
                // Could be a forward reference to a direct-page label.
                if (IsForwardLabelReference(gen, offset)) {
                    wdis = OpDef.WidthDisambiguation.ForceDirect;
                }
            }

            string opcodeStr = formatter.FormatOpcode(op, wdis);

            string formattedOperand = null;
            int operandLen = instrLen - 1;
            PseudoOp.FormatNumericOpFlags opFlags = PseudoOp.FormatNumericOpFlags.None;
            bool isPcRelBankWrap = false;

            // Tweak branch instructions.  We want to show the absolute address rather
            // than the relative offset (which happens with the OperandAddress assignment
            // below), and 1-byte branches should always appear as a 4-byte hex value.
            if (op.AddrMode == OpDef.AddressMode.PCRel) {
                Debug.Assert(attr.OperandAddress >= 0);
                operandLen = 2;
                opFlags = PseudoOp.FormatNumericOpFlags.IsPcRel;
            } else if (op.AddrMode == OpDef.AddressMode.PCRelLong ||
                    op.AddrMode == OpDef.AddressMode.StackPCRelLong) {
                opFlags = PseudoOp.FormatNumericOpFlags.IsPcRel;
            } else if (op.AddrMode == OpDef.AddressMode.Imm ||
                    op.AddrMode == OpDef.AddressMode.ImmLongA ||
                    op.AddrMode == OpDef.AddressMode.ImmLongXY) {
                opFlags = PseudoOp.FormatNumericOpFlags.HasHashPrefix;
            }
            if (opFlags == PseudoOp.FormatNumericOpFlags.IsPcRel) {
                int branchDist = attr.Address - attr.OperandAddress;
                isPcRelBankWrap = branchDist > 32767 || branchDist < -32768;
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
                // Format operand as directed.
                if (op.AddrMode == OpDef.AddressMode.BlockMove) {
                    // Special handling for the double-operand block move.
                    string opstr1 = PseudoOp.FormatNumericOperand(formatter, proj.SymbolTable,
                        gen.Localizer.LabelMap, attr.DataDescriptor, operand >> 8, 1,
                        PseudoOp.FormatNumericOpFlags.None);
                    string opstr2 = PseudoOp.FormatNumericOperand(formatter, proj.SymbolTable,
                        gen.Localizer.LabelMap, attr.DataDescriptor, operand & 0xff, 1,
                        PseudoOp.FormatNumericOpFlags.None);
                    if (gen.Quirks.BlockMoveArgsReversed) {
                        string tmp = opstr1;
                        opstr1 = opstr2;
                        opstr2 = tmp;
                    }
                    formattedOperand = opstr1 + "," + opstr2;
                } else {
                    formattedOperand = PseudoOp.FormatNumericOperand(formatter, proj.SymbolTable,
                        gen.Localizer.LabelMap, attr.DataDescriptor,
                        operandForSymbol, operandLen, opFlags);
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
                    formattedOperand = formatter.FormatHexValue(arg1, 2) + "," +
                        formatter.FormatHexValue(arg2, 2);
                } else {
                    if (operandLen == 2) {
                        // This is necessary for 16-bit operands, like "LDA abs" and "PEA val",
                        // when outside bank zero.  The bank is included in the operand address,
                        // but we don't want to show it here.
                        operandForSymbol &= 0xffff;
                    }
                    formattedOperand = formatter.FormatHexValue(operandForSymbol, operandLen * 2);
                }
            }
            string operandStr = formatter.FormatOperand(op, formattedOperand, wdis);

            string eolComment = proj.Comments[offset];
            if (doAddCycles) {
                bool branchCross = (attr.Address & 0xff00) != (operandForSymbol & 0xff00);
                int cycles = proj.CpuDef.GetCycles(op.Opcode, attr.StatusFlags, attr.BranchTaken,
                    branchCross);
                if (cycles > 0) {
                    eolComment = cycles.ToString() + "  " + eolComment;
                } else {
                    eolComment = (-cycles).ToString() + "+ " + eolComment;
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
                if ((operand & 0x30) != 0 && attr.StatusFlags.E == 1) {
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
            DisasmProject proj = gen.Project;
            Debug.Assert(proj.GetAnattrib(offset).IsInstructionStart);

            FormatDescriptor dfd = proj.GetAnattrib(offset).DataDescriptor;
            if (dfd == null || !dfd.HasSymbol) {
                return false;
            }
            if (!proj.SymbolTable.TryGetValue(dfd.SymbolRef.Label, out Symbol sym)) {
                return false;
            }
            if (!sym.IsInternalLabel) {
                return false;
            }

            // It's an internal label reference.  We don't currently have a data structure
            // that lets us go from label name to file offset.  This situation is sufficiently
            // rare that an O(n) approach is acceptable.  We may need to fix this someday.
            //
            // We only want to know if it is defined after the current instruction.  This is
            // probably being used for a direct-page reference, which is probably at the start
            // of the file, so we run from the start to the current instruction.
            for (int i = 0; i < offset; i++) {
                Anattrib attr = proj.GetAnattrib(i);
                if (attr.Symbol != null && attr.Symbol == sym) {
                    // Found it earlier in file.
                    return false;
                }
            }
            // Must appear later in file.
            return true;
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
        }
    }
}
