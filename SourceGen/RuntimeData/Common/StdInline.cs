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

using PluginCommon;

namespace RuntimeData.Common {
    /// <summary>
    /// Performs inline data formatting for various common situations:
    /// <list type="bullet">
    ///  <item>InAZ_* - inline ASCII null-terminated string</item>
    ///  <item>InA1_* - inline ASCII length-delimited string</item>
    ///  <item>InPZ_* - inline PETSCII null-terminated string</item>
    ///  <item>InP1_* - inline PETSCII length-delimited string</item>
    ///  <item>InW_* - inline 16-bit word</item>
    ///  <item>InWA_* - inline 16-bit address</item>
    ///  <item>InNR_* - non-returning call</item>
    /// </list>
    /// Put a label with the appropriate prefix on the address of the subroutine, and all
    /// calls to it will be formatted automatically.  For example, JSRs to the label
    /// "InAZ_PrintString" will be expected to be followed by null-terminated ASCII string data.
    ///
    /// ASCII functions work for standard and high ASCII, auto-detecting the encoding based on
    /// the first character.
    /// </summary>
    /// <remarks>
    /// As an optimization, we use a lookup table keyed by address, and another keyed by offset.
    /// For a project that doesn't have overlapping address spaces this wouldn't be necessary,
    /// and we could just map the address (JSR operand) to the inline data type.  Since this
    /// code is meant be a general-purpose, we need to use the offset, but that requires a lookup
    /// in the address translation table, which we would prefer to avoid doing for every JSR in
    /// the project.  So we do a quick check on the address first, and only do the offset
    /// translation if it looks like a possible match.
    /// </remarks>
    public class StdInline : MarshalByRefObject, IPlugin, IPlugin_SymbolList, IPlugin_InlineJsr {
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        private class NameMap {
            public string Prefix { get; private set; }
            public InlineKind Kind { get; private set; }
            public NameMap(string prefix, InlineKind kind) {
                Prefix = prefix;
                Kind = kind;
            }
        };
        private enum InlineKind { Unknown = 0, InAZ, InA1, InPZ, InP1, InW, InWA, InNR };
        private static NameMap[] sMap = {
            new NameMap("InNR_", InlineKind.InNR),
            new NameMap("InAZ_", InlineKind.InAZ),
            new NameMap("InA1_", InlineKind.InA1),
            new NameMap("InPZ_", InlineKind.InPZ),
            new NameMap("InP1_", InlineKind.InP1),
            new NameMap("InW_", InlineKind.InW),
            new NameMap("InWA_", InlineKind.InWA),
        };

        // Map of JSR offsets in project to inline data type expected to follow.
        private Dictionary<int, InlineKind> mInlineOffsets = new Dictionary<int, InlineKind>();

        // List of "interesting" addresses.  Used as an optimization.
        private Dictionary<int, int> mInlineAddrs = new Dictionary<int, int>();

        // IPlugin
        public string Identifier {
            get { return "Standard inline data formatter"; }
        }

        // IPlugin
        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;

            mAppRef.DebugLog("StdInline(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
        }

        // IPlugin
        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
            mAddrTrans = null;
        }

        // IPlugin_SymbolList
        public void UpdateSymbolList(List<PlSymbol> plSyms) {
            mInlineOffsets.Clear();
            mInlineAddrs.Clear();

            // Find matching symbols.
            foreach (PlSymbol sym in plSyms) {
                if (sym.Value == AddressTranslate.NON_ADDR) {
                    // The non-addressable target won't be returned by the address-to-offset
                    // lookup, so this doesn't change the behavior.  But there's no value in
                    // having NON_ADDR in the lookup table, so strip it out now.
                    //mAppRef.DebugLog("Ignoring non-addr label '" + sym.Label + "'");
                    continue;
                }
                if (sym.Offset < 0) {
                    // Ignore project/platform symbols.  External symbols are tricky, because
                    // there can be multiple symbols for a given address.  We'd need to know
                    // which specific symbol was referenced by the JSR, and we don't have that
                    // information.
                    continue;
                }
                foreach (NameMap map in sMap) {
                    if (sym.Label.StartsWith(map.Prefix)) {
                        // Offsets will be unique.
                        mInlineOffsets.Add(sym.Offset, map.Kind);
                        // Symbol values (addresses) may not be unique.
                        mInlineAddrs[sym.Value] = sym.Value;
                        break;
                    }
                }
            }
            mAppRef.DebugLog("Found matches for " + mInlineOffsets.Count + " labels");
        }

        // IPlugin_SymbolList
        public bool IsLabelSignificant(string beforeLabel, string afterLabel) {
            return DoesLabelMatch(beforeLabel) || DoesLabelMatch(afterLabel);
        }
        private static bool DoesLabelMatch(string label) {
            foreach (NameMap map in sMap) {
                if (label.StartsWith(map.Prefix)) {
                    return true;
                }
            }
            return false;
        }

        // IPlugin_InlineJsr
        public void CheckJsr(int offset, int operand, out bool noContinue) {
            noContinue = false;

            // Do a quick test on the target address.
            int unused;
            if (!mInlineAddrs.TryGetValue(operand, out unused)) {
                // JSR destination address not special.
                return;
            }

            // Address matched.  Translate the target address to the actual offset.  This is
            // important when multiple offsets have the same address.
            int targetOffset = mAddrTrans.AddressToOffset(offset, operand);
            if (targetOffset < 0) {
                mAppRef.DebugLog("Failed to map address $" + operand.ToString("x4") + " to offset");
                return;
            }
            InlineKind kind;
            if (!mInlineOffsets.TryGetValue(targetOffset, out kind)) {
                // Actual call target doesn't have a matching label.
                return;
            }

            offset += 3;    // move past JSR

            switch (kind) {
                case InlineKind.InAZ:
                    // Null-terminated ASCII string.
                    FormatNullTermString(offset, false);
                    break;
                case InlineKind.InA1:
                    // Length-delimited ASCII string
                    FormatL1String(offset, false);
                    break;
                case InlineKind.InPZ:
                    // Null-terminated PETSCII string.
                    FormatNullTermString(offset, true);
                    break;
                case InlineKind.InP1:
                    // Length-delimited PETSCII string
                    FormatL1String(offset, true);
                    break;
                case InlineKind.InW:
                case InlineKind.InWA:
                    // 16-bit value. Start by confirming next two bytes are inside the file bounds.
                    if (!Util.IsInBounds(mFileData, offset, 2)) {
                        return;
                    }

                    if (kind == InlineKind.InW) {
                        // Format 16-bit value as default (hex).
                        mAppRef.SetInlineDataFormat(offset, 2,
                            DataType.NumericLE, DataSubType.None, null);
                    } else {
                        // Format 16-bit value as an address.
                        mAppRef.SetInlineDataFormat(offset, 2,
                            DataType.NumericLE, DataSubType.Address, null);
                    }
                    break;
                case InlineKind.InNR:
                    // Non-returning call.
                    noContinue = true;
                    break;
            }
        }

        private void FormatNullTermString(int offset, bool isPetscii) {
            if (offset < 0 || offset >= mFileData.Length) {
                return;     // first byte is not inside file
            }

            // search for the terminating null byte
            int nullOff = offset;
            while (nullOff < mFileData.Length) {
                if (mFileData[nullOff] == 0x00) {
                    break;
                }
                nullOff++;
            }
            if (nullOff == mFileData.Length) {
                mAppRef.DebugLog("Unable to find end of null-terminated string at +" +
                    offset.ToString("x6"));
                return;
            }

            DataSubType stype;
            if (isPetscii) {
                stype = DataSubType.C64Petscii;
            } else if (mFileData[offset] >= 0x80) {
                stype = DataSubType.HighAscii;
            } else {
                stype = DataSubType.Ascii;
            }

            mAppRef.SetInlineDataFormat(offset, nullOff - offset + 1,
                DataType.StringNullTerm, stype, null);
        }

        private void FormatL1String(int offset, bool isPetscii) {
            if (offset < 0 || offset >= mFileData.Length) {
                return;     // length byte is not inside file
            }
            int len = mFileData[offset];
            if (offset + 1 + len > mFileData.Length) {
                mAppRef.DebugLog("L1 string ran off end of file at +" + offset.ToString("x6"));
                return;
            }

            DataSubType stype;
            if (isPetscii) {
                stype = DataSubType.C64Petscii;
            } else if (len > 0 && mFileData[offset + 1] >= 0x80) {
                stype = DataSubType.HighAscii;
            } else {
                stype = DataSubType.Ascii;
            }

            mAppRef.SetInlineDataFormat(offset, len + 1,
                DataType.StringL8, stype, null);
        }
    }
}
