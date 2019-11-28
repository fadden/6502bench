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
using System.Text;

using PluginCommon;

namespace RuntimeData.Apple {
    public class VisHiRes : MarshalByRefObject, IPlugin, IPlugin_Visualizer {
        public string Identifier {
            get { return "Apple II Hi-Res Graphic Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        // Visualization identifiers; DO NOT change or projects will break.
        private const string VIS_GEN_BITMAP = "apple2-hi-res-bitmap";
        private const string VIS_GEN_MULTI_MAP = "apple2-hi-res-multi-map";

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_BITMAP, "Apple II Hi-Res Bitmap", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        "offset", typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset,
                        0x2000),
                    new VisParamDescr("Width (in bytes)",
                        "byteWidth", typeof(int), 1, 40, 0, 1),
                    new VisParamDescr("Height",
                        "height", typeof(int), 1, 192, 0, 1),
                    new VisParamDescr("Column stride (bytes)",
                        "colStride", typeof(int), 0, 256, 0, 0),
                    new VisParamDescr("Row stride (bytes)",
                        "rowStride", typeof(int), 0, 256, 0, 0),
                    new VisParamDescr("Color",
                        "color", typeof(bool), 0, 0, 0, true),
                    new VisParamDescr("First byte odd",
                        "firstOdd", typeof(bool), 0, 0, 0, false),
                    new VisParamDescr("Test Float",
                        "floaty", typeof(float), -5.0f, 5.0f, 0, 0.1f),
                }),
            new VisDescr(VIS_GEN_MULTI_MAP, "Apple II Hi-Res Multi-Map", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        "offset", typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset,
                        0x1000),
                    new VisParamDescr("Item width (in bytes)",
                        "itemByteWidth", typeof(int), 1, 40, 0, 1),
                    new VisParamDescr("Item height",
                        "itemHeight", typeof(int), 1, 192, 0, 8),
                    new VisParamDescr("Number of items",
                        "count", typeof(int), 1, 256, 0, 1),
                }),
        };


        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;
        }

        // IPlugin_Visualizer
        public VisDescr[] GetVisGenDescrs() {
            return mDescriptors;
        }

        // IPlugin_Visualizer
        public IVisualization2d Generate2d(VisDescr descr,
                Dictionary<string, object> parms) {
            // TODO: replace with actual
            VisBitmap8 vb = new VisBitmap8(16, 16);
            vb.AddColor(Util.MakeARGB(0xff, 0x40, 0x40, 0x40));
            vb.AddColor(Util.MakeARGB(0xff, 0xff, 0x00, 0x00));
            vb.AddColor(Util.MakeARGB(0xff, 0x00, 0xff, 0x80));

            for (int i = 0; i < 16; i++) {
                vb.SetPixelIndex(i, i, 1);
                vb.SetPixelIndex(15 - i, i, 2);
            }
            return vb;
        }
    }
}
