// Copyright 2019 faddenSoft. All Rights Reserved.
// See the LICENSE.txt file for distribution terms (Apache 2.0).

using System;
using System.Collections.Generic;
using PluginCommon;

namespace ExtensionScriptSample {
    /// <summary>
    /// Sample class for handling a JSR followed by an inline null-terminated string.  Any
    /// label that starts with "PrintLineNullString" is matched.
    /// </summary>
    public class InlineNullTermString : MarshalByRefObject, IPlugin, IPlugin_SymbolList,
            IPlugin_InlineJsr {
        private IApplication mAppRef;
        private byte[] mFileData;

        private const string LABEL_PREFIX = "PrintInlineNullString";
        private Dictionary<int, PlSymbol> mNullStringAddrs = new Dictionary<int, PlSymbol>();

        public string Identifier {
            get {
                return "Inline null-terminated ASCII string handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;

            mAppRef.DebugLog("InlineNullStringHandler(id=" +
                AppDomain.CurrentDomain.Id + "): prepare()");
        }

        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
        }

        public void UpdateSymbolList(List<PlSymbol> plSyms) {
            mNullStringAddrs.Clear();

            // Find matching symbols.
            foreach (PlSymbol sym in plSyms) {
                if (sym.Label.StartsWith(LABEL_PREFIX)) {
                    mNullStringAddrs.Add(sym.Value, sym);
                }
            }
            mAppRef.DebugLog(LABEL_PREFIX + " matched " + mNullStringAddrs.Count + " labels");
        }
        public bool IsLabelSignificant(string beforeLabel, string afterLabel) {
            return beforeLabel.StartsWith(LABEL_PREFIX) || afterLabel.StartsWith(LABEL_PREFIX);
        }

        public void CheckJsr(int offset, int operand, out bool noContinue) {
            noContinue = false;
            if (!mNullStringAddrs.ContainsKey(operand)) {
                return;
            }

            // search for the terminating null byte
            int nullOff = offset + 3;
            while (nullOff < mFileData.Length) {
                if (mFileData[nullOff] == 0x00) {
                    break;
                }
                nullOff++;
            }
            if (nullOff == mFileData.Length) {
                mAppRef.DebugLog("Unable to find end of null-terminated string at +" +
                    (offset+3).ToString("x6"));
                return;
            }

            // Assuming ASCII.  This can be hard-coded, use auto-detection, or look
            // up a value in a project constant.
            mAppRef.SetInlineDataFormat(offset + 3, nullOff - (offset + 3) + 1,
                DataType.StringNullTerm, DataSubType.Ascii, null);
        }
    }
}
