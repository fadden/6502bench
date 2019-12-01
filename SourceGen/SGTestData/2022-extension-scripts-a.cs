// Copyright 2019 faddenSoft. All Rights Reserved.
// See the LICENSE.txt file for distribution terms (Apache 2.0).

using System;
using System.Collections.Generic;

using PluginCommon;

namespace RuntimeData.Test2022 {
    public class Test2022A : MarshalByRefObject, IPlugin, IPlugin_SymbolList,
            IPlugin_InlineJsr, IPlugin_InlineJsl {
        private IApplication mAppRef;
        private byte[] mFileData;

        private int mInline8StringAddr;         // jsr
        private int mInlineRev8StringAddr;      // jsr
        private int mInlineNullStringAddr;      // jsr
        private int mInlineL1StringAddr;        // jsl
        private int mInlineL2StringAddr;        // jsl
        private int mInlineDciStringAddr;       // jsl

        public string Identifier {
            get {
                return "Test 2022-extension-scripts A";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;

            mAppRef.DebugLog("Test2022-A(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
        }

        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
        }

        public void UpdateSymbolList(List<PlSymbol> plSyms) {
            foreach (PlSymbol sym in plSyms) {
                switch (sym.Label) {
                    case "PrintInline8String":
                        mInline8StringAddr = sym.Value;
                        break;
                    case "PrintInlineRev8String":
                        mInlineRev8StringAddr = sym.Value;
                        break;
                    case "PrintInlineNullString":
                        mInlineNullStringAddr = sym.Value;
                        break;
                    case "PrintInlineL1String":
                        mInlineL1StringAddr = sym.Value;
                        break;
                    case "PrintInlineL2String":
                        mInlineL2StringAddr = sym.Value;
                        break;
                    case "PrintInlineDciString":
                        mInlineDciStringAddr = sym.Value;
                        break;
                }
            }
        }

        public bool IsLabelSignificant(string beforeLabel, string afterLabel) {
            const string PREFIX = "PrintInline";    // all interesting labels start with this
            return (beforeLabel.StartsWith(PREFIX) || afterLabel.StartsWith(PREFIX));
        }

        public void CheckJsr(int offset, int operand, out bool noContinue) {
            noContinue = false;
            if (operand == mInline8StringAddr) {
                if (offset + 3 + 8 > mFileData.Length) {
                    mAppRef.DebugLog("8string ran off end at +" +
                        (offset + 3).ToString("x6"));
                    return;
                }
                mAppRef.SetInlineDataFormat(offset + 3, 8,
                   DataType.StringGeneric, DataSubType.Ascii, null);
            } else if (operand == mInlineRev8StringAddr) {
                if (offset + 3 + 8 > mFileData.Length) {
                    mAppRef.DebugLog("rev8string ran off end at +" +
                        (offset + 3).ToString("x6"));
                    return;
                }
                mAppRef.SetInlineDataFormat(offset + 3, 8,
                   DataType.StringReverse, DataSubType.Ascii, null);
            } else if (operand == mInlineNullStringAddr) {
                // look for the terminating null byte
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
                mAppRef.SetInlineDataFormat(offset + 3, nullOff - (offset + 3) + 1,
                    DataType.StringNullTerm, DataSubType.Ascii, null);
            }
        }

        public void CheckJsl(int offset, int operand, out bool noContinue) {
            noContinue = false;
            if (operand == mInlineL1StringAddr) {
                //  0  1  2  3   4   5
                // 22 00 10 01  01  66
                if (offset + 4 >= mFileData.Length) {
                    return;     // length byte is off end
                }
                int len = mFileData[offset + 4];    // 1-byte len in first byte past 4-byte JSL
                if (offset + 5 + len > mFileData.Length) {
                    // ran off the end
                    mAppRef.DebugLog("L1 string ran off end of file at +" +
                        (offset + 4).ToString("x6"));
                    return;
                }
                mAppRef.SetInlineDataFormat(offset + 4, len + 1,
                    DataType.StringL8, DataSubType.Ascii, null);
            } else if (operand == mInlineL2StringAddr) {
                if (offset + 5 >= mFileData.Length) {
                    return;     // length word is off end
                }
                int len = Util.GetWord(mFileData, offset + 4, 2, false);
                if (offset + 6 + len > mFileData.Length) {
                    // ran off the end
                    mAppRef.DebugLog("L2 string ran off end of file at +" +
                        (offset+4).ToString("x6"));
                    return;
                }
                mAppRef.SetInlineDataFormat(offset + 4, len + 2,
                    DataType.StringL16, DataSubType.Ascii, null);
            } else if (operand == mInlineDciStringAddr) {
                // look for the first byte whose high bit doesn't match the first byte's bit
                //  0  1  2  3   4  5
                // 22 00 30 01  66 c1
                if (offset + 3 + 2 >= mFileData.Length) {
                    // need at least two bytes
                    return;
                }
                byte firstBit = (byte) (mFileData[offset + 4] & 0x80);
                int endOff = offset + 5;
                while (endOff < mFileData.Length) {
                    if ((mFileData[endOff] & 0x80) != firstBit) {
                        break;
                    }
                    endOff++;
                }
                if (endOff == mFileData.Length) {
                    mAppRef.DebugLog("Unable to find end of DCI string at +" +
                        (offset+4).ToString("x6"));
                    return;
                }
                mAppRef.SetInlineDataFormat(offset + 4, endOff - (offset + 4) + 1,
                    DataType.StringDci, DataSubType.Ascii, null);
            }
        }
    }
}
