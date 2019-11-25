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
using System.Text;

using PluginCommon;

namespace RuntimeData.Apple {
    public class VisHiRes : MarshalByRefObject, IPlugin, IPlugin_Visualizer2d {
        // Visualization identifiers; DO NOT change.
        private const string VIS_GEN_BITMAP = "apple2-hi-res-bitmap";

        public string Identifier {
            get { return "Apple II Hi-Res Graphic Visualizer"; }
        }

        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;
        }

        public string[] GetVisGenNames() {
            return new string[] {
                VIS_GEN_BITMAP,
            };
        }

        public List<VisParamDescr> GetVisGenParams(string name) {
            List<VisParamDescr> parms = new List<VisParamDescr>();

            switch (name) {
                case VIS_GEN_BITMAP:
                    parms.Add(new VisParamDescr("File Offset",
                        "offset", typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset,
                        0x2000));
                    parms.Add(new VisParamDescr("Width (bytes)",
                        "byteWidth", typeof(int), 1, 40, 0, 1));
                    parms.Add(new VisParamDescr("Height",
                        "height", typeof(int), 1, 192, 0, 1));
                    parms.Add(new VisParamDescr("Color",
                        "color", typeof(bool), 0, 0, 0, true));
                    parms.Add(new VisParamDescr("Test Float",
                        "floaty", typeof(float), -5.0f, 5.0f, 0, 0.1f));
                    break;
                default:
                    parms = null;
                    break;
            }

            return parms;
        }

        public IVisualization2d ExecuteVisGen(string name, Dictionary<string, object> parms) {
            throw new NotImplementedException();
        }
    }
}
