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
    /// <summary>
    /// Identify and format ProDOS-16 and GS/OS system calls.
    /// </summary>
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
        private static Param STRING_PTR = new Param(DataType.NumericLE, DataSubType.Address, 4);
        private static Param BUF_PTR = new Param(DataType.NumericLE, DataSubType.Address, 4);
        private static Param PROC_PTR = new Param(DataType.NumericLE, DataSubType.Address, 4);
        private static Param ACCESS = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param FILE_TYPE = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param AUX_TYPE = new Param(DataType.NumericLE, DataSubType.Hex, 4);
        private static Param STORAGE_TYPE = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param FILE_OFFSET = new Param(DataType.NumericLE, DataSubType.Hex, 4);
        private static Param DATE_TIME = new Param(DataType.Dense, DataSubType.None, 8);
        private static Param OLD_DATE = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param OLD_TIME = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param MISC2 = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param MISC4 = new Param(DataType.NumericLE, DataSubType.Hex, 4);
        private static Param RESULT2 = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param RESULT4 = new Param(DataType.NumericLE, DataSubType.Hex, 4);

        private Dictionary<int, Param[]> ParamDescrs = new Dictionary<int, Param[]>() {
            //
            // "Class 0" ProDOS-16 calls.
            //
            { 0x0031,   // ALLOC_INTERRUPT
                new Param[] { MISC2, BUF_PTR }
            },
            { 0x0004,   // CHANGE_PATH
                new Param[] { STRING_PTR, STRING_PTR }
            },
            { 0x000b,   // CLEAR_BACKUP_BIT
                new Param[] { STRING_PTR }
            },
            { 0x0014,   // CLOSE
                new Param[] { REF_NUM }
            },
            { 0x0001,   // CREATE
                new Param[] { STRING_PTR, ACCESS, FILE_TYPE, AUX_TYPE, STORAGE_TYPE,
                    OLD_DATE, OLD_TIME }
            },
            { 0x0032,   // DEALLOC_INTERRUPT
                new Param[] { MISC2 }
            },
            { 0x0002,   // DESTROY
                new Param[] { STRING_PTR }
            },
            { 0x002c,   // D_INFO
                new Param[] { DEV_NUM, BUF_PTR }
            },
            { 0x0025,   // ERASE_DISK
                new Param[] { STRING_PTR, STRING_PTR, MISC2 }
            },
            { 0x000e,   // EXPAND_PATH
                new Param[] { STRING_PTR, BUF_PTR, MISC2 }
            },
            { 0x0015,   // FLUSH
                new Param[] { REF_NUM }
            },
            { 0x0024,   // FORMAT
                new Param[] { STRING_PTR, STRING_PTR, MISC2 }
            },
            { 0x0028,   // GET_BOOT_VOL
                new Param[] { BUF_PTR }
            },
            { 0x0020,   // GET_DEV_NUM
                new Param[] { STRING_PTR, DEV_NUM }
            },
            { 0x001c,   // GET_DIR_ENTRY
                new Param[] { REF_NUM, MISC2, MISC2, MISC2, BUF_PTR, MISC2, FILE_TYPE,
                    FILE_OFFSET, MISC4, DATE_TIME, DATE_TIME, ACCESS, AUX_TYPE, MISC2 }
            },
            { 0x0019,   // GET_EOF
                new Param[] { REF_NUM, FILE_OFFSET }
            },
            { 0x0006,   // GET_FILE_INFO
                new Param[] { STRING_PTR, ACCESS, FILE_TYPE, AUX_TYPE, STORAGE_TYPE,
                    OLD_DATE, OLD_TIME, OLD_DATE, OLD_TIME, MISC4 }
            },
            { 0x0021,   // GET_LAST_DEV
                new Param[] { DEV_NUM }
            },
            { 0x001b,   // GET_LEVEL
                new Param[] { MISC2 }
            },
            { 0x0027,   // GET_NAME
                new Param[] { BUF_PTR }
            },
            { 0x000a,   // GET_PREFIX
                new Param[] { MISC2, BUF_PTR }
            },
            { 0x002a,   // GET_VERSION
                new Param[] { MISC2 }
            },
            { 0x0011,   // NEWLINE
                new Param[] { REF_NUM, MISC2, MISC2 }
            },
            { 0x0010,   // OPEN
                new Param[] { REF_NUM, STRING_PTR, MISC4 }
            },
            { 0x0029,   // QUIT
                new Param[] { STRING_PTR, MISC2 }
            },
            { 0x0012,   // READ
                new Param[] { REF_NUM, BUF_PTR, MISC4, MISC4 }
            },
            { 0x0022,   // READ_BLOCK
                new Param[] { DEV_NUM, BUF_PTR, MISC4 }
            },
            { 0x0018,   // SET_EOF
                new Param[] { REF_NUM, FILE_OFFSET }
            },
            { 0x0005,   // SET_FILE_INFO
                new Param[] { STRING_PTR, ACCESS, FILE_TYPE, AUX_TYPE, MISC2, OLD_DATE, OLD_TIME,
                    OLD_DATE, OLD_TIME }
            },
            { 0x001a,   // SET_LEVEL
                new Param[] { MISC2 }
            },
            { 0x0016,   // SET_MARK
                new Param[] { REF_NUM, FILE_OFFSET }
            },
            { 0x0009,   // SET_PREFIX
                new Param[] { MISC2, STRING_PTR }
            },
            { 0x0008,   // VOLUME
                new Param[] { STRING_PTR, BUF_PTR, MISC4, MISC4, MISC2 }
            },
            { 0x0013,   // WRITE
                new Param[] { REF_NUM, BUF_PTR, MISC4, MISC4 }
            },
            { 0x0023,   // WRITE_BLOCK
                new Param[] { DEV_NUM, BUF_PTR, MISC4 }
            },

            //
            // "Class 1" GS/OS calls.
            //
            // Some changes were made in System 6.0.  See "Programmer's Reference for
            // System 6.0", by Mike Westerfield / Byteworks.
            //
            { 0x2034,   // AddNotifyProcGS
                new Param[] { PARAM_COUNT, PROC_PTR }
            },
            { 0x201d,   // BeginSessionGS
                new Param[] { PARAM_COUNT }
            },
            { 0x2031,   // BindIntGS
                new Param[] { PARAM_COUNT, RESULT2, MISC2, MISC4 }
            },
            { 0x2004,   // ChangePathGS
                new Param[] { PARAM_COUNT, STRING_PTR, STRING_PTR, MISC2 }
            },
            { 0x200b,   // ClearBackupBitGS
                new Param[] { PARAM_COUNT, STRING_PTR }
            },
            { 0x2014,   // CloseGS
                new Param[] { PARAM_COUNT, REF_NUM }
            },
            { 0x2001,   // CreateGS
                new Param[] { PARAM_COUNT, STRING_PTR, ACCESS, FILE_TYPE, AUX_TYPE,
                    STORAGE_TYPE, FILE_OFFSET, FILE_OFFSET }
            },
            { 0x202e,   // DControlGS (has a bunch of sub-calls)
                new Param[] { PARAM_COUNT, DEV_NUM, MISC2, BUF_PTR, MISC2, RESULT4 }
            },
            { 0x2035,   // DelNotifyProcGS
                new Param[] { PARAM_COUNT, PROC_PTR }
            },
            { 0x2002,   // DestroyGS
                new Param[] { PARAM_COUNT, STRING_PTR }
            },
            { 0x202c,   // DInfoGS
                new Param[] { PARAM_COUNT, DEV_NUM, STRING_PTR, MISC2, MISC4, MISC2, MISC2,
                    MISC2, MISC2, MISC2, MISC2, BUF_PTR }
            },
            { 0x202f,   // DReadGS
                new Param[] { PARAM_COUNT, DEV_NUM, BUF_PTR, MISC4, MISC4, MISC2, MISC4 }
            },
            { 0x2036,   // DRenameGS
                new Param[] { PARAM_COUNT, DEV_NUM, STRING_PTR }
            },
            { 0x202d,   // DStatus (has a bunch of sub-calls)
                new Param[] { PARAM_COUNT, DEV_NUM, MISC2, BUF_PTR, MISC4, MISC4 }
            },
            { 0x2030,   // DWriteGS
                new Param[] { PARAM_COUNT, DEV_NUM, BUF_PTR, MISC4, MISC4, MISC2, MISC4 }
            },
            { 0x201e,   // EndSessionGS
                new Param[] { PARAM_COUNT }
            },
            { 0x2025,   // EraseDiskGS
                new Param[] { PARAM_COUNT, STRING_PTR, STRING_PTR, MISC2, MISC2, MISC2, BUF_PTR }
            },
            { 0x200e,   // ExpandPathGS
                new Param[] { PARAM_COUNT, STRING_PTR, STRING_PTR, MISC2 }
            },
            { 0x2015,   // FlushGS
                new Param[] { PARAM_COUNT, REF_NUM }
            },
            { 0x2024,   // FormatGS
                new Param[] { PARAM_COUNT, STRING_PTR, STRING_PTR, MISC2, MISC2, MISC2, BUF_PTR }
            },
            { 0x2033,   // FSTSpecific
                new Param[] { PARAM_COUNT, MISC2, MISC2 /*...*/ }
            },
            { 0x2028,   // GetBootVolGS
                new Param[] { PARAM_COUNT, BUF_PTR }
            },
            { 0x2020,   // GetDevNumberGS
                new Param[] { PARAM_COUNT, STRING_PTR, DEV_NUM }
            },
            { 0x201c,   // GetDirEntryGS
                new Param[] { PARAM_COUNT, REF_NUM, MISC2, MISC2, MISC2, STRING_PTR, MISC2,
                FILE_TYPE, FILE_OFFSET, MISC4, DATE_TIME, DATE_TIME, ACCESS, AUX_TYPE, MISC2,
                BUF_PTR, FILE_OFFSET, MISC4 }
            },
            { 0x2019,   // GetEOFGS
                new Param[] { PARAM_COUNT, REF_NUM, FILE_OFFSET }
            },
            { 0x2006,   // GetFileInfoGS
                new Param[] { PARAM_COUNT, STRING_PTR, ACCESS, FILE_TYPE, AUX_TYPE, STORAGE_TYPE,
                    DATE_TIME, DATE_TIME, BUF_PTR, FILE_OFFSET, MISC4, FILE_OFFSET, MISC4 }
            },
            { 0x202b,   // GetFSTInfoGS
                new Param[] { PARAM_COUNT, MISC2, MISC2, STRING_PTR, MISC2, MISC2, MISC2,
                    MISC4, MISC4}
            },
            { 0x201b,   // GetLevelGS
                new Param[] { PARAM_COUNT, MISC2, MISC2 }
            },
            { 0x2017,   // GetMarkGS
                new Param[] { PARAM_COUNT, REF_NUM, FILE_OFFSET }
            },
            { 0x2027,   // GetNameGS
                new Param[] { PARAM_COUNT, BUF_PTR, MISC2 }
            },
            { 0x200a,   // GetPrefixGS
                new Param[] { PARAM_COUNT, MISC2, STRING_PTR }
            },
            { 0x2039,   // GetRefInfoGS
                new Param[] { PARAM_COUNT, REF_NUM, ACCESS, STRING_PTR, MISC2, MISC2 }
            },
            { 0x2038,   // GetRefNumGS
                new Param[] { PARAM_COUNT, STRING_PTR, REF_NUM, ACCESS, MISC2, MISC2, MISC2 }
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
            { 0x2007,   // JudgeNameGS
                new Param[] { PARAM_COUNT, MISC2, MISC2, BUF_PTR, MISC2, STRING_PTR, MISC2 }
            },
            { 0x2011,   // NewLine
                new Param[] { PARAM_COUNT, REF_NUM, MISC2, MISC2, BUF_PTR }
            },
            { 0x200d,   // NullGS
                new Param[] { PARAM_COUNT }
            },
            { 0x2010,   // OpenGS
                new Param[] { PARAM_COUNT, REF_NUM, STRING_PTR, ACCESS, MISC2, ACCESS, FILE_TYPE,
                    AUX_TYPE, STORAGE_TYPE, DATE_TIME, DATE_TIME, BUF_PTR, FILE_OFFSET, MISC4,
                    FILE_OFFSET, MISC4 }
            },
            { 0x2003,   // OSShutdownGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2029,   // QuitGS
                new Param[] { PARAM_COUNT, STRING_PTR, MISC2 }
            },
            { 0x2012,   // ReadGS
                new Param[] { PARAM_COUNT, REF_NUM, BUF_PTR, MISC4, MISC4, MISC2 }
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
                new Param[] { PARAM_COUNT, STRING_PTR, ACCESS, FILE_TYPE, AUX_TYPE, MISC2,
                    DATE_TIME, DATE_TIME, BUF_PTR, MISC4, MISC4, MISC4, MISC4 }
            },
            { 0x201a,   // SetLevelGS
                new Param[] { PARAM_COUNT, MISC2, MISC2 }
            },
            { 0x2016,   // SetMarkGS
                new Param[] { PARAM_COUNT, REF_NUM, MISC2, FILE_OFFSET }
            },
            { 0x2009,   // SetPrefixGS
                new Param[] { PARAM_COUNT, MISC2, STRING_PTR }
            },
            { 0x203a,   // SetStdRefNumGS
                new Param[] { PARAM_COUNT, MISC2, REF_NUM }
            },
            { 0x200c,   // SetSysPrefsGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2032,   // UnbindIntGS
                new Param[] { PARAM_COUNT, MISC2 }
            },
            { 0x2008,   // VolumeGS
                new Param[] { PARAM_COUNT, STRING_PTR, STRING_PTR, MISC4, MISC4, MISC2, MISC2,
                    MISC2, MISC2}
            },
            { 0x2013,   // WriteGS
                new Param[] { PARAM_COUNT, REF_NUM, BUF_PTR, MISC4, MISC4, MISC2 }
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

                // Try to format parameter block.
                FormatParameterBlock(offset, req, blockAddr);

                // Check for P16_QUIT or QuitGS call.
                if (req == 0x0029 || req == 0x2029) {
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
        ///
        /// Sometimes different calls will overlap, e.g GET_FILE_INFO and SET_FILE_INFO.  This
        /// is generally only done when the parameters line up, so I'm not expecting things
        /// to come out weird, but it's possible.
        /// </remarks>
        private void FormatParameterBlock(int offset, int req, int blockAddr) {
            Param[] parms;
            if (!ParamDescrs.TryGetValue(req, out parms)) {
                // We don't have a parameter list for this call.
                return;
            }

            int blockOff = mAddrTrans.AddressToOffset(offset, blockAddr);

            int paramCount;
            if (req < 0x0100) {
                // ProDOS-16 parameter blocks are fixed-length.
                paramCount = parms.Length;
            } else {
                // Confirm we can at least get the parameter count.
                if (!Util.IsInBounds(mFileData, blockOff, 2)) {
                    return;
                }
                int pCount = Util.GetWord(mFileData, blockOff, 2, false);
                if (pCount >= parms.Length) {
                    // Might be an uninitialized value.  Just format the pCount.
                    pCount = 1;
                }

                paramCount = pCount + 1;    // add 1 for pCount itself
            }

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
