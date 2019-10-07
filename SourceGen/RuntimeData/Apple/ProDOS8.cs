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

        private IApplication mAppRef;
        private byte[] mFileData;
        private Dictionary<int, PlSymbol> mFunctionList;

        public string Identifier {
            get {
                return "Apple II ProDOS 8 MLI call handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans,
                List<PlSymbol> plSyms) {
            mAppRef = appRef;
            mFileData = fileData;

            mAppRef.DebugLog("ProDOS(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
            //System.Diagnostics.Debugger.Break();

            mFunctionList = PlSymbol.GeneratePlatformValueList(plSyms, P8_MLI_TAG, appRef);
        }

        public void CheckJsr(int offset, out bool noContinue) {
            noContinue = false;
            if (offset + 6 < mFileData.Length &&
                    mFileData[offset + 1] == 0x00 && mFileData[offset + 2] == 0xbf) {
                // match!

                byte req = mFileData[offset + 3];
                if (VERBOSE) {
                    int addr = Util.GetWord(mFileData, offset + 4, 2, false);
                    mAppRef.DebugLog("P8 MLI call detected at +" + offset.ToString("x6") +
                        ", cmd=$" + req.ToString("x2") + " addr=$" + addr.ToString("x4"));
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

                if (req == 0x65) {          // QUIT call
                    noContinue = true;
                }
            }
        }
    }
}
