// Copyright 2019 faddenSoft. All Rights Reserved.
// See the LICENSE.txt file for distribution terms (Apache 2.0).

using System;
using System.Collections.Generic;

using PluginCommon;

namespace ExtensionScriptSample {
    /// <summary>
    /// Sample class for handling a JSR followed by a string prefixed with a 1-byte length.
    /// </summary>
    public class InlineL1String: MarshalByRefObject, IPlugin, IPlugin_SymbolList,
            IPlugin_InlineJsr {
        private IApplication mAppRef;
        private byte[] mFileData;

        // Only one call.
        private const string CALL_LABEL = "PrintInlineL1String";
        private int mInlineL1StringAddr;      // jsr

        public string Identifier {
            get {
                return "Inline L1 ASCII string handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;

            mAppRef.DebugLog("InlineL1String(id=" +
                AppDomain.CurrentDomain.Id + "): prepare()");
        }

        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
        }

        public void UpdateSymbolList(List<PlSymbol> plSyms) {
            // reset this every time, in case they remove the symbol
            mInlineL1StringAddr = -1;

            foreach (PlSymbol sym in plSyms) {
                if (sym.Label == CALL_LABEL) {
                    mInlineL1StringAddr = sym.Value;
                    break;
                }
            }
            mAppRef.DebugLog(CALL_LABEL + " @ $" + mInlineL1StringAddr.ToString("x6"));
        }
        public bool IsLabelSignificant(string beforeLabel, string afterLabel) {
            return beforeLabel == CALL_LABEL || afterLabel == CALL_LABEL;
        }

        public void CheckJsr(int offset, int operand, out bool noContinue) {
            noContinue = false;
            if (operand != mInlineL1StringAddr) {
                return;
            }
            if (offset + 3 >= mFileData.Length) {
                return;     // length byte is off end
            }
            int len = mFileData[3];     // first byte past JSR
            if (offset + 4 + len > mFileData.Length) {
                mAppRef.DebugLog("L1 string ran off end of file at +" +
                    (offset + 4).ToString("x6"));
                return;
            }

            // Assuming ASCII.  This can be hard-coded, use auto-detection, or look
            // up a value in a project constant.
            mAppRef.SetInlineDataFormat(offset + 3, len + 1,
                DataType.StringL8, DataSubType.Ascii, null);
        }
    }
}
