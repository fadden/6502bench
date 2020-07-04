/*
 * Copyright 2018 faddenSoft
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

/*
# inline_entry equ $e100a8
#  jsl inline_entry
#  dc  i2'callnum'
#  dc  i4'parmblock'
#  bcs error

# stack_entry equ $e100b0
#  pea parmblock|-16
#  pea parmblock
#  pea callnum
#  jsl stack_entry
#  bcs error
*/

namespace RuntimeData.Apple {
    public class GSOS : MarshalByRefObject, IPlugin, IPlugin_SymbolList, IPlugin_InlineJsl {
        private const string GSOS_FUNC_TAG = "AppleIIgs-GSOS-Functions";  // tag used in .sym65 file
        private bool VERBOSE = false;

        #region Parameter block defs

        private class Param {
            public DataType Type { get; private set; }
            public DataSubType SubType { get; private set; }
            public int Length { get; private set; }

            public Param(DataType type, DataSubType subType, int length) {
                Type = type;
                SubType = subType;
                Length = length;
            }
        }
        private static Param PARAM_COUNT = new Param(DataType.NumericLE, DataSubType.Decimal, 2);
        private static Param REF_NUM = new Param(DataType.NumericLE, DataSubType.Decimal, 2);
        private static Param DEV_NUM = new Param(DataType.NumericLE, DataSubType.Decimal, 2);
        private static Param PATHNAME = new Param(DataType.NumericLE, DataSubType.Address, 4);
        private static Param BUFFER = new Param(DataType.NumericLE, DataSubType.Address, 4);
        private static Param PROC_POINTER = new Param(DataType.NumericLE, DataSubType.Address, 4);
        private static Param ACCESS = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param FILE_TYPE = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param AUX_TYPE = new Param(DataType.NumericLE, DataSubType.Hex, 4);
        private static Param STORAGE_TYPE = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param FILE_OFFSET = new Param(DataType.NumericLE, DataSubType.Hex, 4);
        private static Param DATE_TIME = new Param(DataType.Dense, DataSubType.None, 8);
        private static Param MISC2 = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param MISC4 = new Param(DataType.NumericLE, DataSubType.Hex, 4);
        private static Param RESULT2 = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param RESULT4 = new Param(DataType.NumericLE, DataSubType.Hex, 4);

        private Dictionary<int, Param[]> ParamDescrs = new Dictionary<int, Param[]>() {
            { 0x2034,   // AddNotifyProcGS
                new Param[] { PARAM_COUNT, PROC_POINTER }
            },
            { 0x201d,   // BeginSessionGS
                new Param[] { PARAM_COUNT }
            },
            { 0x2031,   // BindIntGS
                new Param[] { PARAM_COUNT, RESULT2, MISC2, MISC4 }
            },
            { 0x2004,   // ChangePathGS
                new Param[] { PARAM_COUNT, PATHNAME, PATHNAME }
            },
            { 0x200b,   // ClearBackupBitGS
                new Param[] { PARAM_COUNT, PATHNAME }
            },
            { 0x2014,   // CloseGS
                new Param[] { PARAM_COUNT, REF_NUM }
            },
            { 0x2001,   // CreateGS
                new Param[] { PARAM_COUNT, PATHNAME, ACCESS, FILE_TYPE, AUX_TYPE,
                    STORAGE_TYPE, FILE_OFFSET, FILE_OFFSET }
            },
            { 0x202e,   // DControlGS (has a bunch of sub-calls)
                new Param[] { PARAM_COUNT, DEV_NUM, MISC2, BUFFER, PARAM_COUNT, RESULT4 }
            },
            { 0x2035,   // DelNotifyProcGS
                new Param[] { PARAM_COUNT, PROC_POINTER }
            },
            { 0x2002,   // DestroyGS
                new Param[] { PARAM_COUNT, PATHNAME }
            },
            { 0x202c,   // DInfoGS
                new Param[] { PARAM_COUNT, DEV_NUM, PATHNAME, MISC2, MISC4, MISC2, MISC2,
                    MISC2, MISC2, MISC2, MISC2, BUFFER }
            },
            { 0x202f,   // DReadGS
                new Param[] { PARAM_COUNT, DEV_NUM, BUFFER, MISC4, MISC4, MISC2, MISC4 }
            },
            { 0x2036,   // DRenameGS
                new Param[] { PARAM_COUNT, DEV_NUM, PATHNAME }
            },
            { 0x202d,   // DStatus (has a bunch of sub-calls)
                new Param[] { PARAM_COUNT, DEV_NUM, MISC2, BUFFER, MISC4, MISC4 }
            },
            { 0x2030,   // DWriteGS
                new Param[] { PARAM_COUNT, DEV_NUM, BUFFER, MISC4, MISC4, MISC2, MISC4 }
            },
            { 0x201e,   // EndSessionGS
                new Param[] { PARAM_COUNT }
            },
            { 0x2025,   // EraseDiskGS
                new Param[] { PARAM_COUNT, PATHNAME, PATHNAME, MISC2, MISC2 }
            },
            { 0x200e,   // ExpandPathGS
                new Param[] { PARAM_COUNT, PATHNAME, PATHNAME, MISC2 }
            },
            { 0x2015,   // FlushGS
                new Param[] { PARAM_COUNT, REF_NUM }
            },
            { 0x2024,   // FormatGS
                new Param[] { PARAM_COUNT, PATHNAME, PATHNAME, MISC2, MISC2 }
            },
            { 0x2033,   // FSTSpecific
                new Param[] { PARAM_COUNT, MISC2, MISC2 /*...*/ }
            },
            { 0x2028,   // GetBootVolGS
                new Param[] { PARAM_COUNT, BUFFER }
            },
            { 0x2020,   // GetDevNumberGS
                new Param[] { PARAM_COUNT, PATHNAME, DEV_NUM }
            },
            { 0x201c,   // GetDirEntryGS
                new Param[] { PARAM_COUNT, REF_NUM, MISC2, MISC2, MISC2, PATHNAME, MISC2,
                FILE_TYPE, FILE_OFFSET, MISC4, DATE_TIME, DATE_TIME, ACCESS, AUX_TYPE, MISC2,
                BUFFER, FILE_OFFSET, MISC4 }
            },
            { 0x2019,   // GetEOFGS
                new Param[] { PARAM_COUNT, REF_NUM, FILE_OFFSET }
            },
            { 0x2006,   // GetFileInfoGS
                new Param[] { PARAM_COUNT, PATHNAME, ACCESS, FILE_TYPE, AUX_TYPE, STORAGE_TYPE,
                    DATE_TIME, DATE_TIME, BUFFER, FILE_OFFSET, MISC4, FILE_OFFSET, MISC4 }
            },
            { 0x202b,   // GetFSTInfoGS
                new Param[] { PARAM_COUNT, MISC2, MISC2, PATHNAME, MISC2, MISC2, MISC2,
                    MISC4, MISC4}
            },
            { 0x201b,   // GetLevelGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2017,   // GetMarkGS
                new Param[] { PARAM_COUNT, REF_NUM, FILE_OFFSET }
            },
            { 0x2027,   // GetNameGS
                new Param[] { PARAM_COUNT, BUFFER }
            },
            { 0x200a,   // GetPrefixGS
                new Param[] { PARAM_COUNT, MISC2, PATHNAME }
            },
            { 0x2039,   // GetRefInfoGS
                new Param[] { PARAM_COUNT, REF_NUM, ACCESS, PATHNAME }
            },
            { 0x2038,   // GetRefNumGS
                new Param[] { PARAM_COUNT, PATHNAME, REF_NUM, ACCESS, MISC2, MISC2, MISC2 }
            },
            { 0x2037,   // GetStdRefNumGS
                new Param[] { PARAM_COUNT, MISC2, REF_NUM }
            },
            { 0x200f,   // GetSysPrefsGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x202a,   // GetVersionGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2011,   // NewLine
                new Param[] { PARAM_COUNT, REF_NUM, MISC2, MISC2, BUFFER }
            },
            { 0x200d,   // NullGS
                new Param[] { PARAM_COUNT }
            },
            { 0x2010,   // OpenGS
                new Param[] { PARAM_COUNT, REF_NUM, PATHNAME, ACCESS, MISC2, ACCESS, FILE_TYPE,
                    AUX_TYPE, STORAGE_TYPE, DATE_TIME, DATE_TIME, BUFFER, FILE_OFFSET, MISC4,
                    FILE_OFFSET, MISC4 }
            },
            { 0x2003,   // OSShutdownGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2029,   // QuitGS
                new Param[] { PARAM_COUNT, PATHNAME, MISC2 }
            },
            { 0x2012,   // ReadGS
                new Param[] { PARAM_COUNT, REF_NUM, BUFFER, MISC4, MISC4, MISC2 }
            },
            { 0x2026,   // ResetCacheGS
                new Param[] { PARAM_COUNT }
            },
            { 0x201f,   // SessionStatusGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2018,   // SetEOFGS
                new Param[] { PARAM_COUNT, REF_NUM, MISC2, FILE_OFFSET }
            },
            { 0x2005,   // SetFileInfoGS
                new Param[] { PARAM_COUNT, PATHNAME, ACCESS, FILE_TYPE, AUX_TYPE, MISC2,
                    DATE_TIME, DATE_TIME, BUFFER, MISC4, MISC4, MISC4, MISC4 }
            },
            { 0x201a,   // SetLevelGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2016,   // SetMarkGS
                new Param[] { PARAM_COUNT, REF_NUM, MISC2, FILE_OFFSET }
            },
            { 0x2009,   // SetPrefixGS
                new Param[] { PARAM_COUNT, MISC2, PATHNAME }
            },
            { 0x200c,   // SetSysPrefsGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2032,   // UnbindIntGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2008,   // VolumeGS
                new Param[] { PARAM_COUNT, PATHNAME, PATHNAME, MISC4, MISC4, MISC2, MISC2 }
            },
            { 0x2013,   // WriteGS
                new Param[] { PARAM_COUNT, REF_NUM, BUFFER, MISC4, MISC4, MISC2 }
            },

            // TODO: ProDOS 16 calls
        };

        #endregion Parameter block defs

        private IApplication mAppRef;
        private byte[] mFileData;
        private Dictionary<int, PlSymbol> mFunctionList;
        private AddressTranslate mAddrTrans;

        public string Identifier {
            get {
                return "Apple IIgs GS/OS call handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;

            mAppRef.DebugLog("GSOS(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
            //System.Diagnostics.Debugger.Break();
        }

        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
            mAddrTrans = null;
        }

        public void UpdateSymbolList(List<PlSymbol> plSyms) {
            // Extract the list of function name constants from the platform symbol file.
            mFunctionList = PlSymbol.GeneratePlatformValueList(plSyms, GSOS_FUNC_TAG, mAppRef);
        }
        public bool IsLabelSignificant(string beforeLabel, string afterLabel) {
            return false;
        }

        public void CheckJsl(int offset, int operand, out bool noContinue) {
            const int INLINE_ENTRY = 0xe100a8;

            noContinue = false;
            if (offset + 7 < mFileData.Length && operand == INLINE_ENTRY) {
                // match!

                int req = Util.GetWord(mFileData, offset + 4, 2, false);
                int blockAddr = Util.GetWord(mFileData, offset + 6, 4, false);
                if (VERBOSE) {
                    mAppRef.DebugLog("GSOS call detected at +" + offset.ToString("x6") +
                        ", cmd=$" + req.ToString("x4") + " addr=$" + blockAddr.ToString("x6"));
                }

                PlSymbol sym;
                if (mFunctionList.TryGetValue(req, out sym)) {
                    mAppRef.SetInlineDataFormat(offset + 4, 2, DataType.NumericLE,
                        DataSubType.Symbol, sym.Label);
                } else {
                    mAppRef.SetInlineDataFormat(offset + 4, 2, DataType.NumericLE,
                        DataSubType.None, null);
                }
                mAppRef.SetInlineDataFormat(offset + 6, 4, DataType.NumericLE,
                    DataSubType.Address, null);

                FormatParameterBlock(offset, req, blockAddr);

                // Try to format parameter block.
                if (req == 0x2029) {        // QuitGS call
                    noContinue = true;
                }

            }
        }

        /// <summary>
        /// Attempts to format the parameter block that is passed into the GS/OS call.
        /// </summary>
        /// <remarks>
        /// All "class 1" GS/OS calls have a parameter block that begins with a parameter
        /// count.  The count indicates how many parameters are provided after the pCount
        /// field.  This is a field count, not a byte count, and may be zero.
        /// </remarks>
        private void FormatParameterBlock(int offset, int req, int blockAddr) {
            Param[] parms;
            if (!ParamDescrs.TryGetValue(req, out parms)) {
                // We don't have a parameter list for this call.
                return;
            }

            // Confirm we can at least get the parameter count.
            int blockOff = mAddrTrans.AddressToOffset(offset, blockAddr);
            if (!Util.IsInBounds(mFileData, blockOff, 2)) {
                return;
            }
            int pCount = Util.GetWord(mFileData, blockOff, 2, false);
            if (pCount >= parms.Length) {
                // Might be uninitialized data.  Whatever the case, it's not something we
                // can deal with.
                return;
            }

            int paramCount = pCount + 1;    // add 1 for pCount itself

            // Compute parameter block length in bytes.
            int blockLen = 0;
            for (int i = 0; i < paramCount; i++) {
                blockLen += parms[i].Length;
            }

            // Confirm that the entire thing fits in the file.
            if (!Util.IsInBounds(mFileData, blockOff, blockLen)) {
                return;
            }

            if (VERBOSE) {
                mAppRef.DebugLog("Formatting GS/OS call block at +" + blockOff.ToString("x6") +
                    ", count=" + paramCount);
            }

            // Format each entry.
            for (int i = 0; i < paramCount; i++) {
                Param parm = parms[i];
                mAppRef.SetInlineDataFormat(blockOff, parm.Length, parm.Type,
                    parm.SubType, null);
                blockOff += parm.Length;
            }
        }
    }
}
