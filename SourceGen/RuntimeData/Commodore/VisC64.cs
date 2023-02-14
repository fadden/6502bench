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
//#define SHOW_BORDER
using System;
using System.Collections.ObjectModel;

using PluginCommon;


namespace RuntimeData.Commodore {
    /// <summary>
    /// Visualizer for C64 sprites and fonts.
    /// </summary>
    /// <remarks>
    /// References:
    ///  https://www.c64-wiki.com/wiki/Sprite
    ///  http://sta.c64.org/cbm64disp.html
    ///  http://unusedino.de/ec64/technical/misc/vic656x/colors/
    /// </remarks>
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
        private const string VIS_GEN_HI_RES_SPRITE_GRID = "c64-hi-res-sprite-grid";
        private const string VIS_GEN_MULTI_COLOR_SPRITE = "c64-multi-color-sprite";
        private const string VIS_GEN_MULTI_COLOR_SPRITE_GRID = "c64-multi-color-sprite-grid";
        private const string VIS_GEN_HI_RES_FONT = "c64-hi-res-font";
        private const string VIS_GEN_MULTI_COLOR_FONT = "c64-multi-color-font";

        private const string P_OFFSET = "offset";
        private const string P_COUNT = "count";
        private const string P_DOUBLE_WIDE = "doubleWide";
        private const string P_DOUBLE_HIGH = "doubleHigh";
        private const string P_COLOR = "color";         // sprite color (hi-res or multi-color)
        private const string P_COLOR_01 = "color01";    // multi-color 1
        private const string P_COLOR_11 = "color11";    // multi-color 2

        private const int MAX_COLOR = 15;
        private const int SPRITE_BYTE_WIDTH = 3;
        private const int SPRITE_HEIGHT = 21;
        private const int SPRITE_SIZE = SPRITE_BYTE_WIDTH * SPRITE_HEIGHT;  // 63
        private const int SPRITE_STRIDE = 64;       // hardware sprites are 64-byte aligned

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
            new VisDescr(VIS_GEN_HI_RES_SPRITE_GRID, "C64 Hi-Res Sprite Sheet", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Number of items",
                        P_COUNT, typeof(int), 1, 1024, 0, 16),
                    new VisParamDescr("Sprite color",
                        P_COLOR, typeof(int), 0, 15, 0, 0),
                    new VisParamDescr("Double wide",
                        P_DOUBLE_WIDE, typeof(bool), 0, 0, 0, false),
                    new VisParamDescr("Double high",
                        P_DOUBLE_HIGH, typeof(bool), 0, 0, 0, false),
                }),
            new VisDescr(VIS_GEN_MULTI_COLOR_SPRITE_GRID, "C64 Multi-Color Sprite Sheet", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Number of items",
                        P_COUNT, typeof(int), 1, 1024, 0, 16),
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
            new VisDescr(VIS_GEN_HI_RES_FONT, "C64 Hi-Res Font", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Number of items",
                        P_COUNT, typeof(int), 1, 512, 0, 96),
                }),
            new VisDescr(VIS_GEN_MULTI_COLOR_FONT, "C64 Multi-Color Font", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Number of items",
                        P_COUNT, typeof(int), 1, 512, 0, 96),
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
                    return GenerateSprite(parms, false);
                case VIS_GEN_MULTI_COLOR_SPRITE:
                    return GenerateSprite(parms, true);
                case VIS_GEN_HI_RES_SPRITE_GRID:
                    return GenerateSpriteGrid(parms, false);
                case VIS_GEN_MULTI_COLOR_SPRITE_GRID:
                    return GenerateSpriteGrid(parms, true);
                case VIS_GEN_HI_RES_FONT:
                    return GenerateFont(parms, false);
                case VIS_GEN_MULTI_COLOR_FONT:
                    return GenerateFont(parms, true);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualization2d GenerateSprite(ReadOnlyDictionary<string, object> parms,
                bool isMultiColor) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            byte color = (byte)Util.GetFromObjDict(parms, P_COLOR, 0);
            bool isDoubleWide = Util.GetFromObjDict(parms, P_DOUBLE_WIDE, false);
            bool isDoubleHigh = Util.GetFromObjDict(parms, P_DOUBLE_HIGH, false);
            byte color01 = 0;
            byte color11 = 0;
            if (isMultiColor) {
                color01 = (byte)Util.GetFromObjDict(parms, P_COLOR_01, 0);
                color11 = (byte)Util.GetFromObjDict(parms, P_COLOR_11, 0);
            }

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

            VisBitmap8 vb = new VisBitmap8(SPRITE_BYTE_WIDTH * 8 * xwide, SPRITE_HEIGHT * xhigh);
            SetPalette(vb);
            vb.SetAllPixelIndices(TRANSPARENT);

            if (isMultiColor) {
                RenderMultiColorBitmap(offset, SPRITE_BYTE_WIDTH, SPRITE_HEIGHT,
                    isDoubleWide, isDoubleHigh, color, color01, color11, vb, 0, 0);
            } else {
                RenderHiResBitmap(offset, SPRITE_BYTE_WIDTH, SPRITE_HEIGHT,
                    isDoubleWide, isDoubleHigh, color, vb, 0, 0);
            }
            return vb;
        }

        private IVisualization2d GenerateSpriteGrid(ReadOnlyDictionary<string, object> parms,
                bool isMultiColor) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int count = Util.GetFromObjDict(parms, P_COUNT, 16);
            byte color = (byte)Util.GetFromObjDict(parms, P_COLOR, 0);
            bool isDoubleWide = Util.GetFromObjDict(parms, P_DOUBLE_WIDE, false);
            bool isDoubleHigh = Util.GetFromObjDict(parms, P_DOUBLE_HIGH, false);
            byte color01 = 0;
            byte color11 = 0;
            if (isMultiColor) {
                color01 = (byte)Util.GetFromObjDict(parms, P_COLOR_01, 0);
                color11 = (byte)Util.GetFromObjDict(parms, P_COLOR_11, 0);
            }

            if (offset < 0 || offset >= mFileData.Length ||
                    color < 0 || color > MAX_COLOR ||
                    color01 < 0 || color01 > MAX_COLOR ||
                    color11 < 0 || color11 > MAX_COLOR) {
                // the UI should flag these based on range (and ideally wouldn't have called us)
                mAppRef.ReportError("Invalid parameter");
                return null;
            }
            int lastOffset = offset + SPRITE_STRIDE * count - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Sprite set runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            int xwide = isDoubleWide ? 2 : 1;
            int xhigh = isDoubleHigh ? 2 : 1;

            // Try to make it square, unless there's a large number of them.  Limit the width
            // to 16 sprites (384 pixels + padding).
            int hcells;
            if (count * xwide > 64) {
                hcells = 16 / xwide;
            } else if (count * xwide >= 32) {
                hcells = 8 / xwide;
            } else {
                hcells = (int)Math.Sqrt(count * xwide + 1);
            }

            int vcells = (count + hcells - 1) / hcells;

            VisBitmap8 vb = new VisBitmap8(1 + hcells * SPRITE_BYTE_WIDTH * 8 * xwide + hcells,
                                           1 + vcells * SPRITE_HEIGHT * xhigh + vcells);
            SetPalette(vb);
            vb.SetAllPixelIndices(BORDER_COLOR);

            int cellX = 1;
            int cellY = 1;
            for (int idx = 0; idx < count; idx++) {
                if (isMultiColor) {
                    RenderMultiColorBitmap(offset + idx * SPRITE_STRIDE,
                        SPRITE_BYTE_WIDTH, SPRITE_HEIGHT, isDoubleWide, isDoubleHigh,
                        color, color01, color11, vb, cellX, cellY);
                } else {
                    RenderHiResBitmap(offset + idx * SPRITE_STRIDE,
                        SPRITE_BYTE_WIDTH, SPRITE_HEIGHT, isDoubleWide, isDoubleHigh,
                        color, vb, cellX, cellY);
                }

                cellX += SPRITE_BYTE_WIDTH * 8 * xwide + 1;
                if (cellX == vb.Width) {
                    cellX = 1;
                    cellY += SPRITE_HEIGHT * xhigh + 1;
                }
            }
            return vb;
        }

        private IVisualization2d GenerateFont(ReadOnlyDictionary<string, object> parms,
                bool isMultiColor) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int count = Util.GetFromObjDict(parms, P_COUNT, 96);

            if (offset < 0 || offset >= mFileData.Length) {
                mAppRef.ReportError("Invalid parameter");
                return null;
            }
            int lastOffset = offset + count - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Font runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            // Set the number of horizontal cells.  For small counts we try to make it square,
            // for larger counts we use a reasonable power of 2.
            int hcells;
            if (count > 128) {
                hcells = 32;
            } else if (count > 64) {
                hcells = 16;
            } else if (count >= 32) {
                hcells = 8;
            } else {
                hcells = (int)Math.Sqrt(count + 1);
            }

            int vcells = (count + hcells - 1) / hcells;

            const int FONT_BYTE_WIDTH = 1;
            const int FONT_HEIGHT = 8;
            const int CELL_STRIDE = FONT_BYTE_WIDTH * FONT_HEIGHT;

            // Create a bitmap with room for each cell, plus a 1-pixel boundary
            // between them and around the edges.
            VisBitmap8 vb = new VisBitmap8(1 + hcells * FONT_BYTE_WIDTH * 8 + hcells,
                                           1 + vcells * FONT_HEIGHT + vcells);
            SetPalette(vb);
            vb.SetAllPixelIndices(BORDER_COLOR);

            int cellX = 1;
            int cellY = 1;
            byte color = 0;     /* black */
            byte color01 = 11;  /* dark grey */
            byte color11 = 15;  /* light grey */
            for (int idx = 0; idx < count; idx++) {
                if (isMultiColor) {
                    RenderMultiColorBitmap(offset + idx * CELL_STRIDE, FONT_BYTE_WIDTH, FONT_HEIGHT,
                        false, false, color, color01, color11, vb, cellX, cellY);
                } else {
                    RenderHiResBitmap(offset + idx * CELL_STRIDE, FONT_BYTE_WIDTH, FONT_HEIGHT,
                        false, false, color, vb, cellX, cellY);
                }

                cellX += FONT_BYTE_WIDTH * 8 + 1;
                if (cellX == vb.Width) {
                    cellX = 1;
                    cellY += FONT_HEIGHT + 1;
                }
            }
            return vb;
        }

        private void RenderHiResBitmap(int offset, int byteWidth, int height,
                bool isDoubleWide, bool isDoubleHigh,
                byte color, VisBitmap8 vb, int startx, int starty) {
            int xwide = isDoubleWide ? 2 : 1;
            int xhigh = isDoubleHigh ? 2 : 1;
            for (int row = 0; row < height; row++) {
                for (int col = 0; col < byteWidth; col++) {
                    byte val = mFileData[offset + row * byteWidth + col];
                    for (int bit = 0; bit < 8; bit++) {
                        byte pixColor;
                        if ((val & 0x80) == 0) {
                            pixColor = TRANSPARENT;
                        } else {
                            pixColor = color;
                        }
                        int xc = startx + (col * 8 + bit) * xwide;
                        int yc = starty + row * xhigh;
                        vb.SetPixelIndex(xc, yc, pixColor);
                        if (isDoubleWide || isDoubleHigh) {
                            // Draw doubled pixels.  If we're only doubled in one dimension
                            // this will draw some pixels twice.
                            vb.SetPixelIndex(xc + xwide - 1, yc, pixColor);
                            vb.SetPixelIndex(xc, yc + xhigh - 1, pixColor);
                            vb.SetPixelIndex(xc + xwide - 1, yc + xhigh - 1, pixColor);
                        }
                        val <<= 1;
                    }
                }
            }
        }

        private void RenderMultiColorBitmap(int offset, int byteWidth, int height,
                bool isDoubleWide, bool isDoubleHigh,
                byte color, byte color01, byte color11, VisBitmap8 vb, int startx, int starty) {
            int xwide = isDoubleWide ? 2 : 1;
            int xhigh = isDoubleHigh ? 2 : 1;
            for (int row = 0; row < height; row++) {
                for (int col = 0; col < byteWidth; col++) {
                    byte val = mFileData[offset + row * byteWidth + col];
                    for (int bit = 0; bit < 8; bit += 2) {
                        byte pixColor = 0;
                        switch (val & 0xc0) {
                            case 0x00:  pixColor = TRANSPARENT; break;
                            case 0x80:  pixColor = color;       break;
                            case 0x40:  pixColor = color01;     break;
                            case 0xc0:  pixColor = color11;     break;
                        }
                        int xc = startx + (col * 8 + bit) * xwide;
                        int yc = starty + row * xhigh;
                        // Set two adjacent pixels.
                        vb.SetPixelIndex(xc, yc, pixColor);
                        vb.SetPixelIndex(xc+1, yc, pixColor);
                        if (isDoubleWide || isDoubleHigh) {
                            // Draw doubled pixels.  If we're only doubled in one dimension
                            // this will draw some pixels twice.
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
        }

        private const byte TRANSPARENT = 16;
#if SHOW_BORDER
        private const byte BORDER_COLOR = 0;
#else
        private const byte BORDER_COLOR = TRANSPARENT;
#endif

        // C64 colors, from http://unusedino.de/ec64/technical/misc/vic656x/colors/
        // (the ones on https://www.c64-wiki.com/wiki/Color looked wrong)
        private void SetPalette(VisBitmap8 vb) {
            vb.SetColor(0,  0xff, 0x00, 0x00, 0x00);    // 0=black
            vb.SetColor(1,  0xff, 0xff, 0xff, 0xff);    // 1=white
            vb.SetColor(2,  0xff, 0x68, 0x37, 0x2b);    // 2=red
            vb.SetColor(3,  0xff, 0x70, 0xa4, 0xb2);    // 3=cyan
            vb.SetColor(4,  0xff, 0x6f, 0x3d, 0x86);    // 4=purple
            vb.SetColor(5,  0xff, 0x58, 0x8d, 0x43);    // 5=green
            vb.SetColor(6,  0xff, 0x35, 0x28, 0x79);    // 6=blue
            vb.SetColor(7,  0xff, 0xb8, 0xc7, 0x6f);    // 7=yellow
            vb.SetColor(8,  0xff, 0x6f, 0x4f, 0x25);    // 8=orange
            vb.SetColor(9,  0xff, 0x43, 0x39, 0x00);    // 9-brown
            vb.SetColor(10, 0xff, 0x9a, 0x67, 0x59);    // 10=light red
            vb.SetColor(11, 0xff, 0x44, 0x44, 0x44);    // 11=dark grey
            vb.SetColor(12, 0xff, 0x6c, 0x6c, 0x6c);    // 12=grey
            vb.SetColor(13, 0xff, 0x9a, 0xd2, 0x84);    // 13=light green
            vb.SetColor(14, 0xff, 0x6c, 0x5e, 0xb5);    // 14=light blue
            vb.SetColor(15, 0xff, 0x95, 0x95, 0x95);    // 15=light grey
            vb.SetColor(16, 0, 0, 0, 0);                // 16=transparent
        }
    }
}
