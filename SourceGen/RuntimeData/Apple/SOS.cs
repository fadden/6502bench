/*
 * Copyright 2019 faddenSoft
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
 BRK
 DFB command_code
 DW  parm_block

parm_block
 dfb parm_count
 parameters...
*/

namespace RuntimeData.Apple {
    public class SOS : MarshalByRefObject, IPlugin, IPlugin_SymbolList, IPlugin_InlineBrk {
        private const string SOS_MLI_TAG = "SOS-MLI-Functions";   // tag used in .sym65 file
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
        private static Param[] NO_PARAMS = new Param[0];

        private class ParamSet {
            public Param[] Required { get; private set; }
            public Param[] Optional { get; private set; }

            public ParamSet(Param[] required, Param[] optional) {
                Required = required;
                Optional = optional;
            }
        }

        private static Param PARAM_COUNT = new Param(DataType.NumericLE, DataSubType.Decimal, 1);
        private static Param OPTION_LIST = new Param(DataType.NumericLE, DataSubType.Address, 2);
        private static Param OPTION_LENGTH = new Param(DataType.NumericLE, DataSubType.Decimal, 1);
        private static Param SEG_ADDR = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param SEG_ID = new Param(DataType.NumericLE, DataSubType.Hex, 1);
        private static Param SEG_NUM = new Param(DataType.NumericLE, DataSubType.Hex, 1);
        private static Param PATHNAME = new Param(DataType.NumericLE, DataSubType.Address, 2);
        private static Param TIME_PTR = new Param(DataType.NumericLE, DataSubType.Address, 2);
        private static Param POINTER = new Param(DataType.NumericLE, DataSubType.Address, 2);
        private static Param REF_NUM = new Param(DataType.NumericLE, DataSubType.Decimal, 1);
        private static Param ACCESS = new Param(DataType.NumericLE, DataSubType.Hex, 1);
        private static Param FILE_POS = new Param(DataType.NumericLE, DataSubType.Hex, 4);
        private static Param DATE_TIME = new Param(DataType.NumericLE, DataSubType.Hex, 4);
        private static Param MISC1 = new Param(DataType.NumericLE, DataSubType.Hex, 1);
        private static Param MISC2 = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param MISC4 = new Param(DataType.NumericLE, DataSubType.Hex, 4);
        private static Param UNUSED7 = new Param(DataType.Dense, DataSubType.None, 7);

        private Dictionary<int, ParamSet> mParamDescrs = new Dictionary<int, ParamSet>() {
            { 0x40,     // SOS_REQUEST_SEG
                new ParamSet(
                    new Param[] { PARAM_COUNT, SEG_ADDR, SEG_ADDR, SEG_ID, SEG_NUM },
                    NO_PARAMS
                )
            },
            { 0x41,     // SOS_FIND_SEG
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1, SEG_ID, MISC2, SEG_ADDR, SEG_ADDR, SEG_NUM },
                    NO_PARAMS
                )
            },
            { 0x42,     // SOS_CHANGE_SEG
                new ParamSet(
                    new Param[] { PARAM_COUNT, SEG_NUM, MISC1, MISC2 },
                    NO_PARAMS
                )
            },
            { 0x43,     // SOS_GET_SEG_INFO
                new ParamSet(
                    new Param[] { PARAM_COUNT, SEG_NUM, SEG_ADDR, SEG_ADDR, MISC2, SEG_ID },
                    NO_PARAMS
                )
            },
            { 0x44,     // SOS_GET_SEG_NUM
                new ParamSet(
                    new Param[] { PARAM_COUNT, SEG_ADDR, SEG_NUM },
                    NO_PARAMS
                )
            },
            { 0x45,     // SOS_RELEASE_SEG
                new ParamSet(
                    new Param[] { PARAM_COUNT, SEG_NUM },
                    NO_PARAMS
                )
            },
            { 0x60,     // SOS_SET_FENCE
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1 },
                    NO_PARAMS
                )
            },
            { 0x61,     // SOS_GET_FENCE
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1 },
                    NO_PARAMS
                )
            },
            { 0x62,     // SOS_SET_TIME
                new ParamSet(
                    new Param[] { PARAM_COUNT, TIME_PTR },
                    NO_PARAMS
                )
            },
            { 0x63,     // SOS_GET_TIME
                new ParamSet(
                    new Param[] { PARAM_COUNT, TIME_PTR },
                    NO_PARAMS
                )
            },
            { 0x64,     // SOS_GET_ANALOG
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1, MISC4 },
                    NO_PARAMS
                )
            },
            { 0x65,     // SOS_TERMINATE
                new ParamSet(
                    NO_PARAMS,
                    NO_PARAMS
                )
            },
            { 0x80,     // SOS_D_READ
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1, POINTER, MISC2, MISC2, MISC2 },
                    NO_PARAMS
                )
            },
            { 0x81,     // SOS_D_WRITE
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1, POINTER, MISC2, MISC2 },
                    NO_PARAMS
                )
            },
            { 0x82,     // SOS_D_STATUS
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1, MISC1, POINTER },
                    NO_PARAMS
                )
            },
            { 0x83,     // SOS_D_CONTROL
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1, MISC1, POINTER },
                    NO_PARAMS
                )
            },
            { 0x84,     // SOS_GET_DEV_NUM
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME, MISC1 },
                    NO_PARAMS
                )
            },
            { 0x85,     // SOS_D_INFO
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1, PATHNAME, OPTION_LIST, OPTION_LENGTH },
                    new Param[] { MISC1, MISC1, MISC1, MISC1, MISC1, MISC2, MISC2, MISC2 }
                )
            },
            { 0xc0,     // SOS_CREATE
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME, OPTION_LIST, OPTION_LENGTH },
                    new Param[] { MISC1, MISC2, MISC1, FILE_POS }
                )
            },
            { 0xc1,     // SOS_DESTROY
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME },
                    NO_PARAMS
                )
            },
            { 0xc2,     // SOS_RENAME
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME, PATHNAME },
                    NO_PARAMS
                )
            },
            { 0xc3,     // SOS_SET_FILE_INFO
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME, OPTION_LIST, OPTION_LENGTH },
                    new Param[] { ACCESS, MISC1, MISC2, UNUSED7, DATE_TIME }
                )
            },
            { 0xc4,     // SOS_GET_FILE_INFO
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME, OPTION_LIST, OPTION_LENGTH },
                    new Param[] { ACCESS, MISC1, MISC2, MISC1, FILE_POS, MISC2, DATE_TIME }
                )
            },
            { 0xc5,     // SOS_VOLUME
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME, PATHNAME, MISC2, MISC2 },
                    NO_PARAMS
                )
            },
            { 0xc6,     // SOS_SET_PREFIX
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME },
                    NO_PARAMS
                )
            },
            { 0xc7,     // SOS_GET_PREFIX
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME, MISC1 },
                    NO_PARAMS
                )
            },
            { 0xc8,     // OPEN
                new ParamSet(
                    new Param[] { PARAM_COUNT, PATHNAME, REF_NUM, OPTION_LIST, OPTION_LENGTH },
                    new Param[] { ACCESS, MISC1, POINTER }
                )
            },
            { 0xc9,     // SOS_NEWLINE
                new ParamSet(
                    new Param[] { PARAM_COUNT, REF_NUM, MISC1, MISC1 },
                    NO_PARAMS
                )
            },
            { 0xca,     // SOS_READ
                new ParamSet(
                    new Param[] { PARAM_COUNT, REF_NUM, POINTER, MISC2, MISC2 },
                    NO_PARAMS
                )
            },
            { 0xcb,     // SOS_WRITE
                new ParamSet(
                    new Param[] { PARAM_COUNT, REF_NUM, POINTER, MISC2 },
                    NO_PARAMS
                )
            },
            { 0xcc,     // SOS_CLOSE
                new ParamSet(
                    new Param[] { PARAM_COUNT, REF_NUM },
                    NO_PARAMS
                )
            },
            { 0xcd,     // SOS_FLUSH
                new ParamSet(
                    new Param[] { PARAM_COUNT, REF_NUM },
                    NO_PARAMS
                )
            },
            { 0xce,     // SOS_SET_MARK
                new ParamSet(
                    new Param[] { PARAM_COUNT, REF_NUM, MISC1, FILE_POS },
                    NO_PARAMS
                )
            },
            { 0xcf,     // SOS_GET_MARK
                new ParamSet(
                    new Param[] { PARAM_COUNT, REF_NUM, FILE_POS },
                    NO_PARAMS
                )
            },
            { 0xd0,     // SOS_SET_EOF
                new ParamSet(
                    new Param[] { PARAM_COUNT, REF_NUM, MISC1, FILE_POS },
                    NO_PARAMS
                )
            },
            { 0xd1,     // SOS_GET_EOF
                new ParamSet(
                    new Param[] { PARAM_COUNT, REF_NUM, FILE_POS },
                    NO_PARAMS
                )
            },
            { 0xd2,     // SOS_SET_LEVEL
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1 },
                    NO_PARAMS
                )
            },
            { 0xd3,     // SOS_GET_LEVEL
                new ParamSet(
                    new Param[] { PARAM_COUNT, MISC1 },
                    NO_PARAMS
                )
            },
        };

        #endregion Parameter block defs

        private IApplication mAppRef;
        private byte[] mFileData;
        private Dictionary<int, PlSymbol> mFunctionList;
        private AddressTranslate mAddrTrans;

        public string Identifier {
            get {
                return "Apple III SOS MLI call handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;

            mAppRef.DebugLog("SOS(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
            //System.Diagnostics.Debugger.Break();
        }

        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
            mAddrTrans = null;
        }

        public void UpdateSymbolList(List<PlSymbol> plSyms) {
            // Extract the list of function name constants from the platform symbol file.
            mFunctionList = PlSymbol.GeneratePlatformValueList(plSyms, SOS_MLI_TAG, mAppRef);
        }
        public bool IsLabelSignificant(string beforeLabel, string afterLabel) {
            return false;
        }

        public void CheckBrk(int offset, bool twoByteBrk, out bool noContinue) {
            noContinue = true;
            if (offset + 4 >= mFileData.Length) {
                // ran off the end
                return;
            }

            // We don't want every BRK to get formatted, so we only format it if we find
            // a matching symbol for the command code.

            byte req = mFileData[offset + 1];
            if (VERBOSE) {
                int addr = Util.GetWord(mFileData, offset + 2, 2, false);
                mAppRef.DebugLog("Potential SOS call detected at +" + offset.ToString("x6") +
                    ", cmd=$" + req.ToString("x2") + " addr=$" + addr.ToString("x4"));
            }

            PlSymbol sym;
            if (!mFunctionList.TryGetValue(req, out sym)) {
                return;
            }
            Util.FormatBrkByte(mAppRef, twoByteBrk, offset, DataSubType.Symbol, sym.Label);
            mAppRef.SetInlineDataFormat(offset + 2, 2, DataType.NumericLE,
                DataSubType.Address, null);

            ParamSet pset;
            if (mParamDescrs.TryGetValue(req, out pset)) {
                FormatParameterBlock(offset, pset);
            }

            // Clear the "no continue" flag unless this is a TERMINATE (QUIT) call.
            if (req != 0x65) {          // SOS_TERMINATE
                noContinue = false;
            }
        }

        private void FormatParameterBlock(int offset, ParamSet pset) {
            if (VERBOSE) {
                mAppRef.DebugLog("SOSPARM: trying to format SOS at +" + offset.ToString("x6"));
            }
            int blockAddr = Util.GetWord(mFileData, offset + 2, 2, false);

            // Try to format the parameter block.  Start by figuring out how long the
            // required portion is.
            int blockLen = 0;
            foreach (Param parm in pset.Required) {
                blockLen += parm.Length;
            }

            int optionListAddr = -1;
            int optionListLen = -1;

            // Locate it and verify that the entire thing fits in the file.
            int blockOff = mAddrTrans.AddressToOffset(offset, blockAddr);
            if (VERBOSE) {
                mAppRef.DebugLog("SOSPARM:  checking addr=$" + blockAddr.ToString("x4") +
                    " off=+" + blockOff.ToString("x6") + " len=" + blockLen);
            }
            if (Util.IsInBounds(mFileData, blockOff, blockLen)) {
                if (VERBOSE) {
                    mAppRef.DebugLog("SOSPARM:  formatting block at +" + blockOff.ToString("x6"));
                }

                foreach (Param parm in pset.Required) {
                    // Watch for option list parameters.
                    if (parm == OPTION_LIST) {
                        optionListAddr = Util.GetWord(mFileData, blockOff, 2, false);
                    } else if (parm == OPTION_LENGTH) {
                        optionListLen = mFileData[blockOff];
                    }

                    // We could try to dereference pathname buffers to see if it's a
                    // fixed value and not an empty buffer, but it's hard for us to
                    // reliably tell the difference between a length-limited pathname
                    // and junk.  If the length byte is bad, we run the risk of lumping
                    // a bunch of stuff into the pathname buffer.

                    mAppRef.SetInlineDataFormat(blockOff, parm.Length, parm.Type,
                        parm.SubType, null);
                    blockOff += parm.Length;
                }
            } else {
                if (VERBOSE) {
                    mAppRef.DebugLog("SOSPARM:  NOT in bounds");
                }
            }

            if (optionListAddr >= 0 && optionListLen > 0) {
                if (VERBOSE) {
                    mAppRef.DebugLog("SOSPARM:  format optionList addr=$" +
                        optionListAddr.ToString("x4") + " len=" + optionListLen);
                }
                blockOff = mAddrTrans.AddressToOffset(offset, optionListAddr);
                if (Util.IsInBounds(mFileData, blockOff, optionListLen)) {
                    // Format the parts of the option list that are present.
                    int usedLen = 0;

                    foreach (Param parm in pset.Optional) {
                        if (usedLen + parm.Length > optionListLen) {
                            // This parameter was not provided.
                            break;
                        }

                        mAppRef.SetInlineDataFormat(blockOff, parm.Length, parm.Type,
                            parm.SubType, null);
                        blockOff += parm.Length;
                        usedLen += parm.Length;
                    }
                }
            }
        }
    }
}
