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

namespace RuntimeData.Atari {
    public class Vis2600 : MarshalByRefObject, IPlugin, IPlugin_Visualizer {
        // IPlugin
        public string Identifier {
            get { return "Atari 2600 Graphic Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_GEN_SPRITE = "atari2600-sprite";
        private const string VIS_GEN_PLAYFIELD = "atari2600-playfield";

        private const string P_OFFSET = "offset";
        private const string P_HEIGHT = "height";
        private const string P_ROW_THICKNESS = "rowThickness";
        private const string P_REFLECTED = "reflected";

        private const int MAX_HEIGHT = 192;
        private const int HALF_WIDTH = 20;

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_SPRITE, "Atari 2600 Sprite", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Height",
                        P_HEIGHT, typeof(int), 1, 192, 0, 1),
                }),
            new VisDescr(VIS_GEN_PLAYFIELD, "Atari 2600 Playfield", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Height",
                        P_HEIGHT, typeof(int), 1, 192, 0, 1),
                    new VisParamDescr("Row thickness",
                        P_ROW_THICKNESS, typeof(int), 1, 20, 0, 4),
                    new VisParamDescr("Reflected",
                        P_REFLECTED, typeof(bool), 0, 0, 0, false),
                }),
        };


        // IPlugin
        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;
            mAddrTrans = addrTrans;
        }

        // IPlugin
        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
            mAddrTrans = null;
        }

        // IPlugin_Visualizer
        public VisDescr[] GetVisGenDescrs() {
            // We're using a static set, but it could be generated based on file contents.
            // Confirm that we're prepared.
            if (mFileData == null) {
                return null;
            }
            return mDescriptors;
        }

        // IPlugin_Visualizer
        public IVisualization2d Generate2d(VisDescr descr,
                ReadOnlyDictionary<string, object> parms) {
            switch (descr.Ident) {
                case VIS_GEN_SPRITE:
                    return GenerateSprite(parms);
                case VIS_GEN_PLAYFIELD:
                    return GeneratePlayfield(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualization2d GenerateSprite(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int height = Util.GetFromObjDict(parms, P_HEIGHT, 1);

            if (offset < 0 || offset >= mFileData.Length ||
                    height <= 0 || height > MAX_HEIGHT) {
                // the UI should flag these based on range (and ideally wouldn't have called us)
                mAppRef.ReportError("Invalid parameter");
                return null;
            }

            int lastOffset = offset + height - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Sprite runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            VisBitmap8 vb = new VisBitmap8(8, height);
            SetPalette(vb);

            for (int row = 0; row < height; row++) {
                byte val = mFileData[offset + row];
                for (int col = 0; col < 8; col++) {
                    if ((val & 0x80) != 0) {
                        vb.SetPixelIndex(col, row, (byte)Color.White);
                    } else {
                        vb.SetPixelIndex(col, row, (byte)Color.Black);
                    }
                    val <<= 1;
                }
            }
            return vb;
        }

        private IVisualization2d GeneratePlayfield(ReadOnlyDictionary<string, object> parms) {
            const int BYTE_WIDTH = 3;

            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int height = Util.GetFromObjDict(parms, P_HEIGHT, 1);
            int rowThick = Util.GetFromObjDict(parms, P_ROW_THICKNESS, 4);
            bool isReflected = Util.GetFromObjDict(parms, P_REFLECTED, false);

            if (offset < 0 || offset >= mFileData.Length ||
                    height <= 0 || height > MAX_HEIGHT ||
                    rowThick <= 0 || rowThick > MAX_HEIGHT) {
                // the UI should flag these based on range (and ideally wouldn't have called us)
                mAppRef.ReportError("Invalid parameter");
                return null;
            }

            int lastOffset = offset + BYTE_WIDTH * height - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Playfield runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            // Each half of the playfield is 20 bits wide.
            VisBitmap8 vb = new VisBitmap8(40, height * rowThick);
            SetPalette(vb);

            for (int row = 0; row < height; row++) {
                // Assume data is stored as PF0,PF1,PF2.  PF0/PF2 are in reverse order, so
                // start by assembling them as a reversed 20-bit word.
                int srcOff = offset + row * BYTE_WIDTH;
                int rev = (mFileData[srcOff] >> 4) | (RevBits(mFileData[srcOff + 1], 8) << 4) |
                    (mFileData[srcOff + 2] << 12);

                // Now generate the forward order.
                int fwd = RevBits(rev, 20);

                // Render the first part of the line forward.
                RenderHalfField(vb, row * rowThick, rowThick, 0,
                    fwd, Color.White);
                // Render the second half forward or reversed, in grey.
                RenderHalfField(vb, row * rowThick, rowThick, HALF_WIDTH,
                    isReflected ? rev : fwd, Color.Grey);
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

        private void RenderHalfField(VisBitmap8 vb, int row, int rowDup, int startCol, int val,
                Color setColor) {
            for (int col = startCol; col < startCol + HALF_WIDTH; col++) {
                val <<= 1;
                byte colorIdx;
                if ((val & (1 << HALF_WIDTH)) != 0) {
                    colorIdx = (byte)setColor;
                } else {
                    colorIdx = (byte)Color.Black;
                }

                for (int r = row; r < row + rowDup; r++) {
                    vb.SetPixelIndex(col, r, colorIdx);
                }
            }
        }

        private enum Color : byte {
            Transparent = 0,
            Black = 1,
            White = 2,
            Grey = 3
        }

        private void SetPalette(VisBitmap8 vb) {
            vb.AddColor(0, 0, 0, 0);                // 0=transparent
            vb.AddColor(0xff, 0x00, 0x00, 0x00);    // 1=black
            vb.AddColor(0xff, 0xff, 0xff, 0xff);    // 2=white
            vb.AddColor(0xff, 0xd0, 0xd0, 0xd0);    // 3=grey
        }
    }
}
