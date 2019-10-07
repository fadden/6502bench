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
 JSR $BF00
 DFB command_code
 DW  parm_block

parm_block
 dfb parm_count
 parameters...
*/

namespace RuntimeData.Apple {
    public class ProDOS8 : MarshalByRefObject, IPlugin, IPlugin_InlineJsr {
        private const string P8_MLI_TAG = "ProDOS8-MLI-Functions";   // tag used in .sym65 file
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
        private static Param PARAM_COUNT = new Param(DataType.NumericLE, DataSubType.Decimal, 1);
        private static Param PATHNAME = new Param(DataType.NumericLE, DataSubType.Address, 2);
        private static Param BUFFER = new Param(DataType.NumericLE, DataSubType.Address, 2);
        private static Param CODEPTR = new Param(DataType.NumericLE, DataSubType.Address, 2);
        private static Param ACCESS = new Param(DataType.NumericLE, DataSubType.Hex, 1);
        private static Param FILE_TYPE = new Param(DataType.NumericLE, DataSubType.Hex, 1);
        private static Param AUX_TYPE = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param STORAGE_TYPE = new Param(DataType.NumericLE, DataSubType.Hex, 1);
        private static Param DATE = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param TIME = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param BLOCKS_USED = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param UNIT_NUM = new Param(DataType.NumericLE, DataSubType.Hex, 1);
        private static Param REF_NUM = new Param(DataType.NumericLE, DataSubType.Decimal, 1);
        private static Param COUNT = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param BLOCK_NUM = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param POSITION = new Param(DataType.NumericLE, DataSubType.Hex, 3);
        private static Param MISC1 = new Param(DataType.NumericLE, DataSubType.Hex, 1);
        private static Param MISC2 = new Param(DataType.NumericLE, DataSubType.Hex, 2);
        private static Param NULL3 = new Param(DataType.NumericLE, DataSubType.Hex, 3);

        private Dictionary<int, Param[]> ParamDescrs = new Dictionary<int, Param[]>() {
            { 0x40,     // ALLOC_INTERRUPT
                new Param[] { PARAM_COUNT, MISC1, CODEPTR }
            },
            { 0x41,     // DEALLOC_INTERRUPT
                new Param[] { PARAM_COUNT, MISC1 }
            },
            { 0x65,     // QUIT
                new Param[] { PARAM_COUNT, MISC1, MISC2, MISC1, MISC2 }
            },
            { 0x80,     // READ_BLOCK
                new Param[] { PARAM_COUNT, UNIT_NUM, BUFFER, BLOCK_NUM }
            },
            { 0x81,     // WRITE_BLOCK
                new Param[] { PARAM_COUNT, UNIT_NUM, BUFFER, BLOCK_NUM }
            },
            // 0x82 GET_TIME has no parameter list
            { 0xc0,     // CREATE
                new Param[] { PARAM_COUNT, PATHNAME, ACCESS, FILE_TYPE, AUX_TYPE, STORAGE_TYPE,
                        DATE, TIME }
            },
            { 0xc1,     // DESTROY
                new Param[] { PARAM_COUNT, PATHNAME }
            },
            { 0xc2,     // RENAME
                new Param[] { PARAM_COUNT, PATHNAME, PATHNAME }
            },
            { 0xc3,     // SET_FILE_INFO
                new Param[] { PARAM_COUNT, PATHNAME, ACCESS, FILE_TYPE, AUX_TYPE, NULL3,
                    DATE, TIME }
            },
            { 0xc4,     // GET_FILE_INFO
                new Param[] { PARAM_COUNT, PATHNAME, ACCESS, FILE_TYPE, AUX_TYPE, STORAGE_TYPE,
                    BLOCKS_USED, DATE, TIME, DATE, TIME }
            },
            { 0xc5,     // ON_LINE
                new Param[] { PARAM_COUNT, UNIT_NUM, BUFFER }
            },
            { 0xc6,     // SET_PREFIX
                new Param[] { PARAM_COUNT, PATHNAME }
            },
            { 0xc7,     // GET_PREFIX
                new Param[] { PARAM_COUNT, PATHNAME }
            },
            { 0xc8,     // OPEN
                new Param[] { PARAM_COUNT, PATHNAME, BUFFER, REF_NUM }
            },
            { 0xc9,     // NEWLINE
                new Param[] { PARAM_COUNT, REF_NUM, MISC1, MISC1 }
            },
            { 0xca,     // READ
                new Param[] { PARAM_COUNT, REF_NUM, BUFFER, COUNT, COUNT }
            },
            { 0xcb,     // WRITE
                new Param[] { PARAM_COUNT, REF_NUM, BUFFER, COUNT, COUNT }
            },
            { 0xcc,     // CLOSE
                new Param[] { PARAM_COUNT, REF_NUM }
            },
            { 0xcd,     // FLUSH
                new Param[] { PARAM_COUNT, REF_NUM }
            },
            { 0xce,     // SET_MARK
                new Param[] { PARAM_COUNT, REF_NUM, POSITION }
            },
            { 0xcf,     // GET_MARK
                new Param[] { PARAM_COUNT, REF_NUM, POSITION }
            },
            { 0xd0,     // SET_EOF
                new Param[] { PARAM_COUNT, REF_NUM, POSITION }
            },
            { 0xd1,     // GET_EOF
                new Param[] { PARAM_COUNT, REF_NUM, POSITION }
            },
            { 0xd2,     // SET_BUF
                new Param[] { PARAM_COUNT, REF_NUM, BUFFER }
            },
            { 0xd3,     // GET_BUF
                new Param[] { PARAM_COUNT, REF_NUM, BUFFER }
            },
        };

        #endregion Parameter block defs

        private IApplication mAppRef;
        private byte[] mFileData;
        private Dictionary<int, PlSymbol> mFunctionList;
        private AddressTranslate mAddrTrans;

        public string Identifier {
            get {
                return "Apple II ProDOS 8 MLI call handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans,
                List<PlSymbol> plSyms) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;

            mAppRef.DebugLog("ProDOS(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
            //System.Diagnostics.Debugger.Break();

            // Extract the list of function name constants from the platform symbol file.
            mFunctionList = PlSymbol.GeneratePlatformValueList(plSyms, P8_MLI_TAG, appRef);
        }

        public void CheckJsr(int offset, out bool noContinue) {
            noContinue = false;
            if (offset + 6 < mFileData.Length &&
                    mFileData[offset + 1] == 0x00 && mFileData[offset + 2] == 0xbf) {
                // match!

                byte req = mFileData[offset + 3];
                int blockAddr = Util.GetWord(mFileData, offset + 4, 2, false);
                if (VERBOSE) {
                    mAppRef.DebugLog("P8 MLI call detected at +" + offset.ToString("x6") +
                        ", cmd=$" + req.ToString("x2") + " addr=$" + blockAddr.ToString("x4"));
                }

                PlSymbol sym;
                if (mFunctionList.TryGetValue(req, out sym)) {
                    mAppRef.SetInlineDataFormat(offset + 3, 1, DataType.NumericLE,
                        DataSubType.Symbol, sym.Label);
                } else {
                    mAppRef.SetInlineDataFormat(offset + 3, 1, DataType.NumericLE,
                        DataSubType.None, null);
                }
                mAppRef.SetInlineDataFormat(offset + 4, 2, DataType.NumericLE,
                    DataSubType.Address, null);

                Param[] parms;
                if (ParamDescrs.TryGetValue(req, out parms)) {
                    // Try to format the parameter block.  Start by figuring out how long it is.
                    int blockLen = 0;
                    foreach (Param parm in parms) {
                        blockLen += parm.Length;
                    }

                    // Locate it and verify that the entire thing fits in the file.
                    int blockOff = mAddrTrans.AddressToOffset(offset, blockAddr);
                    if (Util.IsInBounds(mFileData, blockOff, blockLen)) {
                        if (VERBOSE) {
                            mAppRef.DebugLog("Formatting P8 block at +" + blockOff.ToString("x6"));
                        }

                        foreach (Param parm in parms) {
                            // We could try to dereference pathname buffers to see if it's a
                            // fixed value and not an empty buffer, but it's hard for us to
                            // reliably tell the difference between a length-limited pathname
                            // and junk.  If the length byte is bad, we run the risk of lumping
                            // a bunch of stuff into the pathname buffer.
                            mAppRef.SetInlineDataFormat(blockOff, parm.Length, parm.Type,
                                parm.SubType, null);
                            blockOff += parm.Length;
                        }
                    }
                }

                if (req == 0x65) {          // QUIT call
                    noContinue = true;
                }
            }
        }
    }
}
