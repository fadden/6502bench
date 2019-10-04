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
# toolbox equ $e10000
#  pea ...
#  pea ...
#  ldx #function
#  jsl toolbox
*/

namespace RuntimeData.Apple {
    public class IIgsToolbox : MarshalByRefObject, IPlugin, IPlugin_InlineJsl {
        private const string TOOLBOX_FUNC_TAG = "AppleIIgs-Toolbox-Functions";    // tag used in .sym65 file
        private bool VERBOSE = false;

        private IApplication mAppRef;
        private byte[] mFileData;
        private Dictionary<int, PlSymbol> mFunctionList;

        public string Identifier {
            get {
                return "Apple IIgs toolbox call handler";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, List<PlSymbol> plSyms) {
            mAppRef = appRef;
            mFileData = fileData;

            mAppRef.DebugLog("IIgsToolbox(id=" + AppDomain.CurrentDomain.Id + "): prepare()");

            mFunctionList = PlSymbol.GeneratePlatformValueList(plSyms, TOOLBOX_FUNC_TAG, appRef);
        }

        public void CheckJsl(int offset, out bool noContinue) {
            noContinue = false;
            if (offset < 3) {
                return;
            }
            // This only works if the LDX with the function comes right before the JSL.
            // Fortunately, the assembler macros all work that way.
            if (mFileData[offset + 1] == 0x00 && mFileData[offset + 2] == 0x00 &&
                    mFileData[offset + 3] == 0xe1 && mFileData[offset - 3] == 0xa2 /*LDX imm*/) {
                // match!

                int func = Util.GetWord(mFileData, offset - 2, 2, false);
                if (VERBOSE) {
                    mAppRef.DebugLog("Toolbox call detected at +" + offset.ToString("x6") +
                        ", func=$" + func.ToString("x4"));
                }

                PlSymbol sym;
                if (mFunctionList.TryGetValue(func, out sym)) {
                    mAppRef.SetOperandFormat(offset - 3, DataSubType.Symbol, sym.Label);
                }
            }
        }
    }
}
