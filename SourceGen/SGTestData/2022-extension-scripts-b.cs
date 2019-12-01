// Copyright 2019 faddenSoft. All Rights Reserved.
// See the LICENSE.txt file for distribution terms (Apache 2.0).

using System;
using System.Collections.Generic;

using PluginCommon;

namespace RuntimeData.Test2022 {
    public class Test2022B : MarshalByRefObject, IPlugin, IPlugin_InlineBrk {
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        public string Identifier {
            get {
                return "Test 2022-extension-scripts B";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;

            mAppRef.DebugLog("Test2022-B(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
        }

        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
            mAddrTrans = null;
        }

        public void CheckBrk(int offset, bool twoByteBrk, out bool noContinue) {
            noContinue = true;

            // need BRK, function byte, and two-byte address
            if (!Util.IsInBounds(mFileData, offset, 4)) {
                return;
            }
            byte func = mFileData[offset + 1];
            if (func != 0x85 && (func < 0x01 || func > 0x02)) {
                return;
            }

            Util.FormatBrkByte(mAppRef, twoByteBrk, offset, DataSubType.Hex, null);
            mAppRef.SetInlineDataFormat(offset + 2, 2, DataType.NumericLE,
                DataSubType.Address, null);
            noContinue = false;

            int structAddr = Util.GetWord(mFileData, offset + 2, 2, false);
            int structOff = mAddrTrans.AddressToOffset(offset, structAddr);
            if (structOff < 0) {
                mAppRef.DebugLog("Unable to get offset for address $" + structAddr.ToString("x6"));
                return;
            }

            switch (func) {
                case 0x01:
                    if (!Util.IsInBounds(mFileData, structOff, 27)) {
                        mAppRef.DebugLog("Struct doesn't fit in file");
                        return;
                    }
                    mAppRef.SetInlineDataFormat(structOff + 0, 2, DataType.NumericLE,
                        DataSubType.Decimal, null);
                    mAppRef.SetInlineDataFormat(structOff + 2, 2, DataType.NumericBE,
                        DataSubType.Hex, null);
                    mAppRef.SetInlineDataFormat(structOff + 4, 4, DataType.NumericLE,
                        DataSubType.Hex, null);
                    mAppRef.SetInlineDataFormat(structOff + 8, 4, DataType.NumericBE,
                        DataSubType.Hex, null);
                    mAppRef.SetInlineDataFormat(structOff + 12, 1, DataType.NumericLE,
                        DataSubType.Ascii, null);
                    mAppRef.SetInlineDataFormat(structOff + 13, 1, DataType.NumericLE,
                        DataSubType.HighAscii, null);
                    mAppRef.SetInlineDataFormat(structOff + 14, 8, DataType.StringDci,
                        DataSubType.Ascii, null);
                    mAppRef.SetInlineDataFormat(structOff + 22, 3, DataType.NumericLE,
                        DataSubType.Address, null);
                    mAppRef.SetInlineDataFormat(structOff + 25, 2, DataType.NumericLE,
                        DataSubType.Symbol, "data02");
                    break;
                case 0x02:
                    if (!Util.IsInBounds(mFileData, structOff, 2)) {
                        mAppRef.DebugLog("Struct doesn't fit in file");
                        return;
                    }
                    mAppRef.SetInlineDataFormat(structOff, 2, DataType.NumericLE,
                        DataSubType.Address, null);
                    int nextAddr = Util.GetWord(mFileData, structOff + 2, 2, false);
                    int nextOff = mAddrTrans.AddressToOffset(structOff, nextAddr);
                    if (!Util.IsInBounds(mFileData, nextOff, 1)) {
                        mAppRef.DebugLog("Struct doesn't fit in file");
                        return;
                    }
                    mAppRef.SetInlineDataFormat(nextOff, 8, DataType.StringGeneric,
                        DataSubType.HighAscii, null);
                    break;
                case 0x85:
                    // do nothing further
                    break;
            }
        }
    }
}
