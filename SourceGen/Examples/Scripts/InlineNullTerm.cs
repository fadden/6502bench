// Copyright 2019 faddenSoft. All Rights Reserved.
// See the LICENSE.txt file for distribution terms (Apache 2.0).

using System;
using System.Collections.Generic;

using PluginCommon;

namespace ExtensionScriptSample {
    /// <summary>
    /// Sample class for handling a JSR followed by an inline null-terminated string.
    /// </summary>
    public class InlineNullStringHandler : MarshalByRefObject, IPlugin, IPlugin_InlineJsr {
        private IApplication mAppRef;
        private byte[] mFileData;

        private int mInlineNullStringAddr;      // jsr

        public string Identifier {
            get {
                return "Inline null-terminated ASCII string handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans,
                List<PlSymbol> plSyms) {
            mAppRef = appRef;
            mFileData = fileData;

            mAppRef.DebugLog("InlineNullStringHandler(id=" +
                AppDomain.CurrentDomain.Id + "): prepare()");

            // reset this every time, in case they remove the label
            mInlineNullStringAddr = -1;
            foreach (PlSymbol sym in plSyms) {
                if (sym.Label == "PrintInlineZString") {
                    mInlineNullStringAddr = sym.Value;
                    break;
                }
            }
            mAppRef.DebugLog("PrintInlineZString @ $" + mInlineNullStringAddr.ToString("x6"));
        }

        public void CheckJsr(int offset, out bool noContinue) {
            noContinue = false;
            int target = Util.GetWord(mFileData, offset + 1, 2, false);
            if (target == mInlineNullStringAddr) {
                // search for the terminating null byte
                int nullOff = offset + 3;
                while (nullOff < mFileData.Length) {
                    if (mFileData[nullOff] == 0) {
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
}
