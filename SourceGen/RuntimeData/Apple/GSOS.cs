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

        private IApplication mAppRef;
        private byte[] mFileData;
        private Dictionary<int, PlSymbol> mFunctionList;

        public string Identifier {
            get {
                return "Apple IIgs GS/OS call handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;

            mAppRef.DebugLog("GSOS(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
            //System.Diagnostics.Debugger.Break();

        }

        public void UpdateSymbolList(List<PlSymbol> plSyms) {
            // Extract the list of function name constants from the platform symbol file.
            mFunctionList = PlSymbol.GeneratePlatformValueList(plSyms, GSOS_FUNC_TAG, mAppRef);
        }
        public bool IsLabelSignificant(string beforeLabel, string afterLabel) {
            return false;
        }

        public void CheckJsl(int offset, out bool noContinue) {
            noContinue = false;
            if (offset + 7 < mFileData.Length && mFileData[offset + 1] == 0xa8 &&
                    mFileData[offset + 2] == 0x00 && mFileData[offset + 3] == 0xe1) {
                // match!

                int req = Util.GetWord(mFileData, offset + 4, 2, false);
                if (VERBOSE) {
                    int addr = Util.GetWord(mFileData, offset + 6, 4, false);
                    mAppRef.DebugLog("GSOS call detected at +" + offset.ToString("x6") +
                        ", cmd=$" + req.ToString("x4") + " addr=$" + addr.ToString("x6"));
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
            }
        }
    }
}
