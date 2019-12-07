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
using System.Collections.ObjectModel;
using System.Text;

using PluginCommon;

namespace RuntimeData.Commodore {
    public class VisC64 : MarshalByRefObject, IPlugin, IPlugin_Visualizer {
        // IPlugin
        public string Identifier {
            get { return "C64 Graphic Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_GEN_HI_RES_SPRITE = "c64-hi-res-sprite";
        private const string VIS_GEN_MULTI_COLOR_SPRITE = "c64-multi-color-sprite";

        private const string P_OFFSET = "offset";
        private const string P_DOUBLE_WIDE = "doubleWide";
        private const string P_DOUBLE_HIGH = "doubleHigh";
        private const string P_COLOR = "color";         // sprite color (hi-res or multi-color)
        private const string P_COLOR_01 = "color01";    // multi-color 1
        private const string P_COLOR_11 = "color11";    // multi-color 2

        private const int MAX_COLOR = 15;
        private const int BYTE_WIDTH = 3;
        private const int HEIGHT = 21;
        private const int SPRITE_SIZE = BYTE_WIDTH * HEIGHT;    // 63

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_HI_RES_SPRITE, "C64 Hi-Res Sprite", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Sprite color",
                        P_COLOR, typeof(int), 0, 15, 0, 0),
                    new VisParamDescr("Double wide",
                        P_DOUBLE_WIDE, typeof(bool), 0, 0, 0, false),
                    new VisParamDescr("Double high",
                        P_DOUBLE_HIGH, typeof(bool), 0, 0, 0, false),
                }),
            new VisDescr(VIS_GEN_MULTI_COLOR_SPRITE, "C64 Multi-Color Sprite", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Sprite color",
                        P_COLOR, typeof(int), 0, 15, 0, 1),
                    new VisParamDescr("Multi-color 1",
                        P_COLOR_01, typeof(int), 0, 15, 0, 0),
                    new VisParamDescr("Multi-color 2",
                        P_COLOR_11, typeof(int), 0, 15, 0, 2),
                    new VisParamDescr("Double wide",
                        P_DOUBLE_WIDE, typeof(bool), 0, 0, 0, false),
                    new VisParamDescr("Double high",
                        P_DOUBLE_HIGH, typeof(bool), 0, 0, 0, false),
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
                case VIS_GEN_HI_RES_SPRITE:
                    return GenerateHiResSprite(parms);
                case VIS_GEN_MULTI_COLOR_SPRITE:
                    return GenerateMultiColorSprite(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualization2d GenerateHiResSprite(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            byte color = (byte)Util.GetFromObjDict(parms, P_COLOR, 0);
            bool isDoubleWide = Util.GetFromObjDict(parms, P_DOUBLE_WIDE, false);
            bool isDoubleHigh = Util.GetFromObjDict(parms, P_DOUBLE_HIGH, false);

            if (offset < 0 || offset >= mFileData.Length || color < 0 || color > MAX_COLOR) {
                // the UI should flag these based on range (and ideally wouldn't have called us)
                mAppRef.ReportError("Invalid parameter");
                return null;
            }

            int lastOffset = offset + SPRITE_SIZE - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Sprite runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            int xwide = isDoubleWide ? 2 : 1;
            int xhigh = isDoubleHigh ? 2 : 1;

            VisBitmap8 vb = new VisBitmap8(BYTE_WIDTH * 8 * xwide, HEIGHT * xhigh);
            SetPalette(vb);

            // Clear all pixels to transparent, then just draw the non-transparent ones.
            vb.SetAllPixelIndices(TRANSPARENT);

            for (int row = 0; row < HEIGHT; row++) {
                for (int col = 0; col < BYTE_WIDTH; col++) {
                    byte val = mFileData[offset + row * BYTE_WIDTH + col];
                    for (int bit = 0; bit < 8; bit++) {
                        if ((val & 0x80) != 0) {
                            int xc = (col * 8 + bit) * xwide;
                            int yc = row * xhigh;
                            vb.SetPixelIndex(xc, yc, color);
                            if (isDoubleWide || isDoubleHigh) {
                                // Draw doubled pixels.  If we're only doubled in one dimension
                                // this will draw pixels twice.
                                vb.SetPixelIndex(xc + xwide - 1, yc, color);
                                vb.SetPixelIndex(xc, yc + xhigh - 1, color);
                                vb.SetPixelIndex(xc + xwide - 1, yc + xhigh - 1, color);
                            }
                        }
                        val <<= 1;
                    }
                }
            }
            return vb;
        }

        private IVisualization2d GenerateMultiColorSprite(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            byte color = (byte)Util.GetFromObjDict(parms, P_COLOR, 0);
            byte color01 = (byte)Util.GetFromObjDict(parms, P_COLOR_01, 0);
            byte color11 = (byte)Util.GetFromObjDict(parms, P_COLOR_11, 0);
            bool isDoubleWide = Util.GetFromObjDict(parms, P_DOUBLE_WIDE, false);
            bool isDoubleHigh = Util.GetFromObjDict(parms, P_DOUBLE_HIGH, false);

            if (offset < 0 || offset >= mFileData.Length ||
                    color < 0 || color > MAX_COLOR ||
                    color01 < 0 || color01 > MAX_COLOR ||
                    color11 < 0 || color11 > MAX_COLOR) {
                // the UI should flag these based on range (and ideally wouldn't have called us)
                mAppRef.ReportError("Invalid parameter");
                return null;
            }

            int lastOffset = offset + SPRITE_SIZE - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Sprite runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            int xwide = isDoubleWide ? 2 : 1;
            int xhigh = isDoubleHigh ? 2 : 1;

            VisBitmap8 vb = new VisBitmap8(BYTE_WIDTH * 8 * xwide, HEIGHT * xhigh);
            SetPalette(vb);
            vb.SetAllPixelIndices(TRANSPARENT);

            for (int row = 0; row < HEIGHT; row++) {
                for (int col = 0; col < BYTE_WIDTH; col++) {
                    byte val = mFileData[offset + row * BYTE_WIDTH + col];
                    for (int bit = 0; bit < 8; bit += 2) {
                        byte pixColor = 0;
                        switch (val & 0xc0) {
                            case 0x00:  pixColor = TRANSPARENT; break;
                            case 0x80:  pixColor = color;       break;
                            case 0x40:  pixColor = color01;     break;
                            case 0xc0:  pixColor = color11;     break;
                        }
                        int xc = (col * 8 + bit) * xwide;
                        int yc = row * xhigh;
                        vb.SetPixelIndex(xc, yc, pixColor);
                        vb.SetPixelIndex(xc+1, yc, pixColor);
                        if (isDoubleWide || isDoubleHigh) {
                            // Draw doubled pixels.  If we're only doubled in one dimension
                            // this will draw pixels twice.
                            vb.SetPixelIndex(xc + xwide*2 - 2, yc,             pixColor);
                            vb.SetPixelIndex(xc + xwide*2 - 1, yc,             pixColor);
                            vb.SetPixelIndex(xc,               yc + xhigh - 1, pixColor);
                            vb.SetPixelIndex(xc + 1,           yc + xhigh - 1, pixColor);
                            vb.SetPixelIndex(xc + xwide*2 - 2, yc + xhigh - 1, pixColor);
                            vb.SetPixelIndex(xc + xwide*2 - 1, yc + xhigh - 1, pixColor);
                        }
                        val <<= 2;
                    }
                }
            }
            return vb;
        }

        private const byte TRANSPARENT = 16;

        // C64 colors, from http://unusedino.de/ec64/technical/misc/vic656x/colors/
        // (the ones on https://www.c64-wiki.com/wiki/Color looked wrong)
        private void SetPalette(VisBitmap8 vb) {
            vb.AddColor(0xff, 0x00, 0x00, 0x00);    // 0=black
            vb.AddColor(0xff, 0xff, 0xff, 0xff);    // 1=white
            vb.AddColor(0xff, 0x68, 0x37, 0x2b);    // 2=red
            vb.AddColor(0xff, 0x70, 0xa4, 0xb2);    // 3=cyan
            vb.AddColor(0xff, 0x6f, 0x3d, 0x86);    // 4=purple
            vb.AddColor(0xff, 0x58, 0x8d, 0x43);    // 5=green
            vb.AddColor(0xff, 0x35, 0x28, 0x79);    // 6=blue
            vb.AddColor(0xff, 0xb8, 0xc7, 0x6f);    // 7=yellow
            vb.AddColor(0xff, 0x6f, 0x4f, 0x25);    // 8=orange
            vb.AddColor(0xff, 0x43, 0x39, 0x00);    // 9-brown
            vb.AddColor(0xff, 0x9a, 0x67, 0x59);    // 10=light red
            vb.AddColor(0xff, 0x44, 0x44, 0x44);    // 11=dark grey
            vb.AddColor(0xff, 0x6c, 0x6c, 0x6c);    // 12=grey
            vb.AddColor(0xff, 0x9a, 0xd2, 0x84);    // 13=light green
            vb.AddColor(0xff, 0x6c, 0x5e, 0xb5);    // 14=light blue
            vb.AddColor(0xff, 0x95, 0x95, 0x95);    // 15=light grey
            vb.AddColor(0, 0, 0, 0);                // 16=transparent
        }
    }
}
