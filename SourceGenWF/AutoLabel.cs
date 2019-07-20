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
using System.Text;
using System.Diagnostics;

namespace SourceGenWF {
    /// <summary>
    /// Functions for generation of "auto" labels.
    /// </summary>
    public static class AutoLabel {
        /// <summary>
        /// Auto-label style enumeration.  Values were chosen to map directly to a combo box.
        /// </summary>
        public enum Style {
            Unknown = -1,
            Simple = 0,
            Annotated = 1,
            FullyAnnotated = 2
        }

        /// <summary>
        /// Generates a unique address symbol.  Does not add the symbol to the table.
        /// 
        /// This does not follow any Formatter rules -- labels are always entirely upper-case.
        /// </summary>
        /// <param name="addr">Address that label will be applied to.</param>
        /// <param name="symbols">Symbol table, for uniqueness check.</param>
        /// <param name="prefix">Prefix to use; must start with a letter.</param>
        /// <returns>Newly-created, unique symbol.</returns>
        public static Symbol GenerateUniqueForAddress(int addr, SymbolTable symbols,
                string prefix) {
            // $1234 == L1234, $05/1234 == L51234.
            string label = prefix + addr.ToString("X4");    // always upper-case
            if (symbols.TryGetValue(label, out Symbol unused)) {
                const int MAX_RENAME = 999;
                string baseLabel = label;
                StringBuilder sb = new StringBuilder(baseLabel.Length + 8);
                int index = -1;

                do {
                    // This is expected to be unlikely and infrequent, so a simple linear
                    // probe for uniqueness is fine.  Labels are based on the address, not
                    // the offset, so even without user-created labels there's still an
                    // opportunity for duplication.
                    index++;
                    sb.Clear();
                    sb.Append(baseLabel);
                    sb.Append('_');
                    sb.Append(index);
                    label = sb.ToString();
                } while (index <= MAX_RENAME && symbols.TryGetValue(label, out unused));
                if (index > MAX_RENAME) {
                    // I give up
                    throw new Exception("Too many identical symbols: " + label);
                }
            }
            Symbol sym = new Symbol(label, addr, Symbol.Source.Auto,
                Symbol.Type.LocalOrGlobalAddr);
            return sym;
        }

        /// <summary>
        /// Source reference type.
        /// 
        /// The enum is in priority order, i.e. the lowest-valued item "wins" in situations
        /// where only one value is used.
        /// </summary>
        [Flags]
        private enum RefTypes {
            None            = 0,
            SubCall         = 1 << 0,
            Branch          = 1 << 1,
            DataRef         = 1 << 2,
            Write           = 1 << 3,
            Read            = 1 << 4,
        }
        private static readonly char[] TAGS = { 'S', 'B', 'D', 'W', 'R' };

        /// <summary>
        /// Generates an auto-label with a prefix string based on the XrefSet.
        /// </summary>
        /// <param name="addr">Address that label will be applied to.</param>
        /// <param name="symbols">Symbol table, for uniqueness check.</param>
        /// <param name="xset">Cross-references for this location.</param>
        /// <returns>Newly-created, unique symbol.</returns>
        public static Symbol GenerateAnnotatedLabel(int addr, SymbolTable symbols,
                XrefSet xset, Style style) {
            Debug.Assert(xset != null);
            Debug.Assert(style != Style.Simple);

            RefTypes rtypes = RefTypes.None;
            foreach (XrefSet.Xref xr in xset) {
                switch (xr.Type) {
                    case XrefSet.XrefType.SubCallOp:
                        rtypes |= RefTypes.SubCall;
                        break;
                    case XrefSet.XrefType.BranchOp:
                        rtypes |= RefTypes.Branch;
                        break;
                    case XrefSet.XrefType.RefFromData:
                        rtypes |= RefTypes.DataRef;
                        break;
                    case XrefSet.XrefType.MemAccessOp:
                        switch (xr.AccType) {
                            case Asm65.OpDef.MemoryEffect.Read:
                                rtypes |= RefTypes.Read;
                                break;
                            case Asm65.OpDef.MemoryEffect.Write:
                                rtypes |= RefTypes.Write;
                                break;
                            case Asm65.OpDef.MemoryEffect.ReadModifyWrite:
                                rtypes |= RefTypes.Read;
                                rtypes |= RefTypes.Write;
                                break;
                            case Asm65.OpDef.MemoryEffect.None:
                            case Asm65.OpDef.MemoryEffect.Unknown:
                                break;
                            default:
                                Debug.Assert(false);
                                break;
                        }
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }

            if (rtypes == RefTypes.None) {
                // unexpected
                Debug.Assert(false);
                return GenerateUniqueForAddress(addr, symbols, "X_");
            }

            StringBuilder sb = new StringBuilder(8);
            for (int i = 0; i < TAGS.Length; i++) {
                if (((int) rtypes & (1 << i)) != 0) {
                    sb.Append(TAGS[i]);

                    if (style == Style.Annotated) {
                        break;
                    }
                }
            }
            sb.Append('_');
            return GenerateUniqueForAddress(addr, symbols, sb.ToString());
        }
    }
}
