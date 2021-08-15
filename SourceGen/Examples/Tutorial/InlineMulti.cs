// Copyright 2021 faddenSoft. All Rights Reserved.
// See the LICENSE.txt file for distribution terms (Apache 2.0).

using System;
using System.Collections.Generic;
using PluginCommon;

namespace ExtensionScriptSample {
    /// <summary>
    /// Formats three different kinds of inline functions:
    ///
    ///   PrintInlineL1String: format following data as L1 string
    ///   PrintInlineNullString*: format following data as null-term string.
    ///   PrintInlineAddrString*: format following data as 16-bit pointer to null-term string.
    /// </summary>
    public class InlineMultiData : MarshalByRefObject, IPlugin, IPlugin_SymbolList,
            IPlugin_InlineJsr {
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        private const string L1STR_NAME = "PrintInlineL1String";
        private const string NULLSTR_PREFIX = "PrintInlineNullString";
        private const string ADDRSTR_PREFIX = "PrintInlineAddrString";
        private enum InlineKind { Unknown = 0, L1Str, NullStr, AddrStr };

        private Dictionary<int, InlineKind> mInlineLabels = new Dictionary<int, InlineKind>();

        public string Identifier {
            get {
                return "Inline multi-data formatter";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;

            mAppRef.DebugLog("InlineMultiData(id=" +
                AppDomain.CurrentDomain.Id + "): prepare()");
        }

        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
            mAddrTrans = null;
        }

        public void UpdateSymbolList(List<PlSymbol> plSyms) {
            mInlineLabels.Clear();

            // Find matching symbols.  Save the symbol's value (its address) and the type.
            // We want an exact match on L1STR_NAME, and prefix matches on the other two.
            foreach (PlSymbol sym in plSyms) {
                if (sym.Label.Equals(L1STR_NAME)) {
                    mInlineLabels.Add(sym.Value, InlineKind.L1Str);
                } else if (sym.Label.StartsWith(NULLSTR_PREFIX)) {
                    mInlineLabels.Add(sym.Value, InlineKind.NullStr);
                } else if (sym.Label.StartsWith(ADDRSTR_PREFIX)) {
                    mInlineLabels.Add(sym.Value, InlineKind.AddrStr);
                }
            }
            mAppRef.DebugLog("Found matches for " + mInlineLabels.Count + " labels");
        }

        public bool IsLabelSignificant(string beforeLabel, string afterLabel) {
            return DoesLabelMatch(beforeLabel) || DoesLabelMatch(afterLabel);
        }
        private static bool DoesLabelMatch(string label) {
            return (label.Equals(L1STR_NAME) ||
                label.StartsWith(NULLSTR_PREFIX) ||
                label.StartsWith(ADDRSTR_PREFIX));
        }

        public void CheckJsr(int offset, int operand, out bool noContinue) {
            noContinue = false;

            InlineKind kind;
            if (!mInlineLabels.TryGetValue(operand, out kind)) {
                return;
            }

            switch (kind) {
                case InlineKind.L1Str:
                    // Length-delimited ASCII string
                    FormatL1String(offset + 3);
                    break;
                case InlineKind.NullStr:
                    // Null-terminated ASCII string.
                    FormatNullTermString(offset + 3);
                    break;
                case InlineKind.AddrStr:
                    // Pointer to data.  Format as address.  Start by confirming next two
                    // bytes are inside the file bounds.
                    if (!Util.IsInBounds(mFileData, offset + 3, 2)) {
                        return;
                    }
                    // Format 16-bit value as an address.
                    mAppRef.SetInlineDataFormat(offset + 3, 2,
                        DataType.NumericLE, DataSubType.Address, null);

                    // Now format the string that the address points to.  Extract the
                    // address from the operand.
                    int strAddr = Util.GetWord(mFileData, offset + 3, 2, false);
                    // Convert the address to a file offset.  Returns -1 if not in file bounds.
                    int strOff = mAddrTrans.AddressToOffset(offset, strAddr);
                    // Format it if it's in bounds.
                    FormatNullTermString(strOff);
                    break;
            }
        }

        private void FormatL1String(int offset) {
            if (offset < 0 || offset >= mFileData.Length) {
                return;     // length byte is not inside file
            }
            int len = mFileData[offset];
            if (offset + 1 + len > mFileData.Length) {
                mAppRef.DebugLog("L1 string ran off end of file at +" +
                    (offset + 1).ToString("x6"));
                return;
            }

            // Assuming ASCII.
            mAppRef.SetInlineDataFormat(offset, len + 1,
                DataType.StringL8, DataSubType.Ascii, null);
        }

        private void FormatNullTermString(int offset) {
            if (offset < 0 || offset >= mFileData.Length) {
                return;     // start is not inside file
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

            // Assuming ASCII.
            mAppRef.SetInlineDataFormat(offset, nullOff - offset + 1,
                DataType.StringNullTerm, DataSubType.Ascii, null);
        }
    }
}
