// Copyright 2025 faddenSoft. All Rights Reserved.
// See the LICENSE.txt file for distribution terms (Apache 2.0).

using System;
using System.Collections.Generic;

using PluginCommon;

// COP/WDM handler
namespace RuntimeData.Test20180 {
    public class Test20180C : MarshalByRefObject, IPlugin, IPlugin_InlineCop, IPlugin_InlineWdm {
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        public string Identifier {
            get {
                return "Test 20180-extension-scripts C";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;

            mAppRef.DebugLog("Test20180-C(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
        }

        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
            mAddrTrans = null;
        }

        public void CheckCop(int offset, out bool noContinue) {
            HandleString(offset, out noContinue);
        }

        public void CheckWdm(int offset, out bool noContinue) {
            HandleString(offset, out noContinue);
        }

        private void HandleString(int offset, out bool noContinue) {
            const int INSTR_LEN = 2;        // opcode + byte
            noContinue = false;

            // Handle as string with 8-bit length prefix.
            if (offset + INSTR_LEN >= mFileData.Length) {
                return;     // length byte is off end
            }
            int len = mFileData[offset + INSTR_LEN];
            if (offset + INSTR_LEN + 1 + len > mFileData.Length) {
                // ran off the end
                mAppRef.DebugLog("L1 string ran off end of file at +" +
                    (offset + INSTR_LEN).ToString("x6"));
                return;
            }
            mAppRef.SetInlineDataFormat(offset + INSTR_LEN, len + 1,
                DataType.StringL8, DataSubType.Ascii, null);
        }
    }
}
