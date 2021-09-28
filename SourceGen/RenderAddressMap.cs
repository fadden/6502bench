/*
 * Copyright 2021 faddenSoft
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
using AddressChange = CommonUtil.AddressMap.AddressChange;

namespace SourceGen {
    /// <summary>
    /// Functions for generating a human-readable form of the address map.
    /// </summary>
    /// A graphical / interactive visualization would be nicer, but this can be pasted
    /// into bug reports.
    /// </remarks>
    public static class RenderAddressMap {
        private const string CRLF = "\r\n";

        /// <summary>
        /// Formats the address map for viewing.
        /// </summary>
        public static string GenerateString(DisasmProject project, Formatter formatter) {
            AddressMap addrMap = project.AddrMap;
            bool showBank = !project.CpuDef.HasAddr16;

            StringBuilder sb = new StringBuilder();
            int depth = 0;
            int prevOffset = -1;
            int prevAddr = 0;
            int lastEndOffset = -1;

            sb.AppendLine("Map of address regions");
            IEnumerator<AddressChange> iter = addrMap.AddressChangeIterator;
            while (iter.MoveNext()) {
                AddressChange change = iter.Current;
                if (change.IsStart) {
                    //if (change.Offset == lastEndOffset) {
                    //    // Extra vertical space for a START following an END at the same offset.
                    //    PrintDepthLines(sb, depth, true);
                    //    sb.Append(CRLF);
                    //    lastEndOffset = -1;
                    //}

                    if (prevOffset >= 0 && change.Offset != prevOffset) {
                        // Start of region at new offset.  Output address info for space
                        // between previous start or end.
                        PrintAddressInfo(sb, formatter, depth, prevAddr,
                            change.Offset - prevOffset, showBank);
                    }

                    // Start following end, or start following start after a gap.
                    if (!string.IsNullOrEmpty(change.Region.PreLabel)) {
                        PrintDepthLines(sb, depth, true);
                        sb.Append("|  pre='" + change.Region.PreLabel + "' ");
                        PrintAddress(sb, formatter, change.Region.PreLabelAddress, showBank);
                        sb.Append(CRLF);
                    }
                    sb.Append(formatter.FormatOffset24(change.Offset));
                    PrintDepthLines(sb, depth, false);
                    sb.Append("+- " + "start");

                    if (change.IsSynthetic) {
                        sb.Append(" (auto-generated)");
                    } else {
                        // If there's a label here, show it.
                        Anattrib attr = project.GetAnattrib(change.Offset);
                        if (attr.Symbol != null && !string.IsNullOrEmpty(attr.Symbol.Label)) {
                            sb.Append(" : ");
                            sb.Append(attr.Symbol.Label);
                        }
                    }

                    sb.Append(CRLF);

                    prevOffset = change.Offset;
                    prevAddr = change.Address;
                    depth++;
                } else {
                    Debug.Assert(prevOffset >= 0);
                    depth--;

                    if (change.Offset + 1 != prevOffset) {
                        // End of region at new offset.  Output address info for space
                        // between previous start or end.
                        PrintAddressInfo(sb, formatter, depth + 1, prevAddr,
                            change.Offset + 1 - prevOffset, showBank);
                    }

                    sb.Append(formatter.FormatOffset24(change.Offset));
                    PrintDepthLines(sb, depth, false);
                    sb.Append("+- " + "end");
                    //PrintAddress(sb, formatter, change.Address, showBank);
                    //sb.Append(")");
                    sb.Append(CRLF);

                    PrintDepthLines(sb, depth, true);
                    sb.Append(CRLF);

                    // Use offset+1 here so it lines up with start records.
                    prevOffset = lastEndOffset = change.Offset + 1;
                    prevAddr = change.Address;
                }
            }
            Debug.Assert(depth == 0);

            return sb.ToString();
        }

        private static void PrintDepthLines(StringBuilder sb, int depth, bool doIndent) {
            if (doIndent) {
                sb.Append("       ");
            }
            sb.Append("  ");
            while (depth-- > 0) {
                sb.Append("| ");
            }
        }

        private static void PrintAddressInfo(StringBuilder sb, Formatter formatter, int depth,
                int startAddr, int length, bool showBank) {
            //PrintDepthLines(sb, depth);
            //sb.Append(CRLF);

            PrintDepthLines(sb, depth, true);
            sb.Append(' ');
            if (startAddr == AddressMap.NON_ADDR) {
                sb.Append("-NA-");
            } else {
                PrintAddress(sb, formatter, startAddr, showBank);
                sb.Append(" - ");
                PrintAddress(sb, formatter, startAddr + length - 1, showBank);
            }
            sb.Append("  length=" + length + " ($" + length.ToString("x4") + ")");
            sb.Append(CRLF);

            //PrintDepthLines(sb, depth, true);
            //sb.Append(CRLF);
        }

        private static void PrintAddress(StringBuilder sb, Formatter formatter, int addr,
                bool showBank) {
            if (addr == AddressMap.NON_ADDR) {
                sb.Append("-NA-");
            } else {
                sb.Append("$");
                sb.Append(formatter.FormatAddress(addr, showBank));
            }
        }
    }
}
