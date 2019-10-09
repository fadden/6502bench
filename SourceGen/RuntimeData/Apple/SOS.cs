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
    public class SOS : MarshalByRefObject, IPlugin, IPlugin_InlineBrk {
        private const string SOS_MLI_TAG = "SOS-MLI-Functions";   // tag used in .sym65 file
        private bool VERBOSE = true;

        private IApplication mAppRef;
        private byte[] mFileData;
        private Dictionary<int, PlSymbol> mFunctionList;

        public string Identifier {
            get {
                return "Apple III SOS MLI call handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans,
                List<PlSymbol> plSyms) {
            mAppRef = appRef;
            mFileData = fileData;

            mAppRef.DebugLog("SOS(id=" + AppDomain.CurrentDomain.Id + "): prepare()");
            //System.Diagnostics.Debugger.Break();

            mFunctionList = PlSymbol.GeneratePlatformValueList(plSyms, SOS_MLI_TAG, appRef);
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

            // Clear the "no continue" flag unless this is a QUIT call.
            if (req != 0x65) {          // QUIT call
                noContinue = false;
            }
        }
    }
}
