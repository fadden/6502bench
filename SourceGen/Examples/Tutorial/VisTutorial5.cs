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
using System.Collections.ObjectModel;

using PluginCommon;

namespace Tutorial {
    public class VisTutorial5 : MarshalByRefObject, IPlugin, IPlugin_Visualizer {
        // IPlugin
        public string Identifier {
            get { return "Tutorial Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_GEN_BITMAP = "tutorial-bitmap";

        private const string P_OFFSET = "offset";
        private const string P_BYTE_WIDTH = "byteWidth";
        private const string P_HEIGHT = "height";
        private const string P_ROW_STRIDE = "rowStride";

        private const int MAX_DIM = 4096;

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_BITMAP, "Tutorial Bitmap", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Width (in bytes)",
                        P_BYTE_WIDTH, typeof(int), 1, 64, 0, 1),
                    new VisParamDescr("Height",
                        P_HEIGHT, typeof(int), 1, 512, 0, 1),
                    new VisParamDescr("Row stride (bytes)",
                        P_ROW_STRIDE, typeof(int), 0, 256, 0, 0),
                })
        };


        // IPlugin
        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
        }

        // IPlugin
        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
        }

        // IPlugin_Visualizer
        public VisDescr[] GetVisGenDescrs() {
            if (mFileData == null) {
                return null;
            }
            return mDescriptors;
        }

        // IPlugin_Visualizer
        public IVisualization2d Generate2d(VisDescr descr,
                ReadOnlyDictionary<string, object> parms) {
            switch (descr.Ident) {
                case VIS_GEN_BITMAP:
                    return GenerateBitmap(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualization2d GenerateBitmap(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int byteWidth = Util.GetFromObjDict(parms, P_BYTE_WIDTH, 1);
            int height = Util.GetFromObjDict(parms, P_HEIGHT, 1);
            int rowStride = Util.GetFromObjDict(parms, P_ROW_STRIDE, 0);

            if (rowStride == 0) {
                rowStride = byteWidth;  // provide nice default when stride==0
            }
            if (offset < 0 || offset >= mFileData.Length ||
                    byteWidth <= 0 || byteWidth > MAX_DIM ||
                    height <= 0 || height > MAX_DIM) {
                mAppRef.ReportError("Invalid parameter");
                return null;
            }
            if (rowStride < byteWidth || rowStride > MAX_DIM) {
                mAppRef.ReportError("Invalid row stride");
                return null;
            }

            int lastOffset = offset + rowStride * height - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Bitmap runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            VisBitmap8 vb = new VisBitmap8(byteWidth * 8, height);
            SetPalette(vb);

            // Convert bits to pixels.
            for (int row = 0; row < height; row++) {
                for (int byteCol = 0; byteCol < byteWidth; byteCol++) {
                    byte val = mFileData[offset + row * rowStride + byteCol];
                    for (int bit = 0; bit < 8; bit++) {
                        if ((val & 0x80) != 0) {
                            vb.SetPixelIndex(byteCol * 8 + bit, row, (byte)Color.Solid);
                        } else {
                            vb.SetPixelIndex(byteCol * 8 + bit, row, (byte)Color.Transparent);
                        }
                        val <<= 1;
                    }
                }
            }
            return vb;
        }

        private int RevBits(int val, int count) {
            int result = 0;
            for (int i = 0; i < count; i++) {
                result <<= 1;
                result |= val & 0x01;
                val >>= 1;
            }
            return result;
        }

        private enum Color : byte {
            Transparent = 0,
            Solid = 1,
        }

        private void SetPalette(VisBitmap8 vb) {
            vb.AddColor(0, 0, 0, 0);                // 0=transparent
            vb.AddColor(0xff, 0x20, 0x20, 0xff);    // 1=solid
        }
    }
}
