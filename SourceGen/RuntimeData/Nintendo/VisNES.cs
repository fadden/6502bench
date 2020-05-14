/*
 * Copyright 2020 faddenSoft
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

using PluginCommon;

namespace RuntimeData.Nintendo {
    /// <summary>
    /// Visualization generators for Nintendo Entertainment System graphics.
    ///
    /// The full PPU pattern table grid is pretty straightforward.  The way the tiles are
    /// combined into sprites and background is not.  This presents a "tile grid" that
    /// shows a simple MxN grid of tiles in row-major order, but reality seems to be
    /// more complex than that and may be game-specific.
    ///
    /// To simplify things, the CHR ROM section must be labeled "CHR_ROM", and should have
    /// a unique address.
    /// </summary>
    public class VisNES : MarshalByRefObject, IPlugin, IPlugin_SymbolList, IPlugin_Visualizer {
        // IPlugin
        public string Identifier {
            get { return "Nintendo Entertainment System Graphic Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        private const string CHR_ROM = "CHR_ROM";
        private int mChrRomOffset = -1;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_CHR_ROM = "nes-chr-rom";
        private const string VIS_TILE_GRID = "nes-tile-grid";

        private const string P_OFFSET = "offset";
        private const string P_WIDTH = "width";
        private const string P_HEIGHT = "height";
        private const string P_COLOR_PALETTE = "colorPalette";
        private const string P_SHOW_LABELS = "showLabels";
        private const string P_FLIP_RIGHT = "flipRight";
        private const string P_RIGHT_TABLE = "useRightTable";

        private const int TileWidth = 8;
        private const int TileHeight = 8;
        private const int BytesPerTile = 16;

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_CHR_ROM, "NES CHR ROM Pattern Tables", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    //new VisParamDescr("File offset (hex)",
                    //    P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    // TODO: either make this an enum, or just provide 4 slots that take a color
                    //       (add a SpecialMode.Color and accept six-digit #123456 inputs;
                    //       no need to restrict to NES limitations)
                    new VisParamDescr("Color palette",
                        P_COLOR_PALETTE, typeof(int), 0, 2, 0, 0),
                     new VisParamDescr("Show labels",
                        P_SHOW_LABELS, typeof(bool), 0, 0, 0, true),
                }),
            new VisDescr(VIS_TILE_GRID, "NES Tile Grid", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Color palette",
                        P_COLOR_PALETTE, typeof(int), 0, 2, 0, 0),
                    new VisParamDescr("Width (in tiles)",
                        P_WIDTH, typeof(int), 1, 256, 0, 1),
                    new VisParamDescr("Height (in tiles)",
                        P_HEIGHT, typeof(int), 1, 256, 0, 1),
                    // Flips the pixels of the tiles on the right side.  This handles a common
                    // case, but in practice a sprite can be an arbitrary mix of flipped and
                    // normal tiles.
                    new VisParamDescr("Horiz-flip right side",
                        P_FLIP_RIGHT, typeof(bool), 0, 0, 0, false),
                    new VisParamDescr("Use right table",
                        P_RIGHT_TABLE, typeof(bool), 0, 0, 0, false),
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

        // IPlugin_SymbolList
        public void UpdateSymbolList(List<PlSymbol> plSyms) {
            // reset this every time, in case they remove the symbol
            mChrRomOffset = -1;

            foreach (PlSymbol sym in plSyms) {
                if (sym.Label == CHR_ROM) {
                    int addr = sym.Value;
                    mChrRomOffset = mAddrTrans.AddressToOffset(0, addr);
                    break;
                }
            }
            mAppRef.DebugLog(CHR_ROM + " @ +" + mChrRomOffset.ToString("x6"));
        }
        // IPlugin_SymbolList
        public bool IsLabelSignificant(string beforeLabel, string afterLabel) {
            return beforeLabel == CHR_ROM || afterLabel == CHR_ROM;
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
                case VIS_CHR_ROM:
                    return GenerateRomChart(parms);
                case VIS_TILE_GRID:
                    return GenerateTileGrid(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualization2d GenerateRomChart(ReadOnlyDictionary<string, object> parms) {
            int paletteNum = Util.GetFromObjDict(parms, P_COLOR_PALETTE, 0);
            bool showLabels = Util.GetFromObjDict(parms, P_SHOW_LABELS, true);

            if (mChrRomOffset < 0) {
                mAppRef.ReportError("CHR_ROM symbol not found");
                return null;
            }
            if (mChrRomOffset + 8192 > mFileData.Length) {
                mAppRef.ReportError("8KB CHR ROM runs off end of file");
                return null;
            }

            const int spacing = 1;
            const int tWidth = TileWidth + spacing;
            const int tHeight = TileHeight + spacing;
            const int gap = 4 + spacing * 2;
            int labelSpacing = showLabels ? 9 : 0;

            VisBitmap8 vb = new VisBitmap8(tWidth * 16 * 2 + gap + labelSpacing * 2 + 1,
                tHeight * 16 + labelSpacing + 1);
            SetPalette(vb, (Palette)paletteNum);

            if (showLabels) {
                for (int i = 0; i < 16; i++) {
                    char ch = (i < 10) ? (char)('0' + i) : (char)('A' + i - 10);
                    VisBitmap8.DrawChar(vb, ch, (i + 1) * tWidth + 1, 1,
                        (byte)Color.Black, (byte)Color.White);
                    VisBitmap8.DrawChar(vb, ch, (i + 16 + 1) * tWidth + gap + 1, 1,
                        (byte)Color.Black, (byte)Color.White);
                    VisBitmap8.DrawChar(vb, ch, 1, (i + 1) * tHeight + 1,
                        (byte)Color.Black, (byte)Color.White);
                    VisBitmap8.DrawChar(vb, ch, (1 + 16 + 16) * tWidth + gap + 1,
                        (i + 1) * tHeight + 1, (byte)Color.Black, (byte)Color.White);
                }
            }

            for (int idx = 0; idx < 512; idx++) {
                int xshift = idx < 256 ? 0 : tWidth * 16 + gap;
                int xc = (idx & 0x0f) * tWidth + xshift + labelSpacing + 1;
                int yc = ((idx & 0xff) >> 4) * tHeight + labelSpacing + 1;

                RenderTile(idx, vb, xc, yc, false);
            }

            return vb;
        }

        private IVisualization2d GenerateTileGrid(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int paletteNum = Util.GetFromObjDict(parms, P_COLOR_PALETTE, 0);
            int width = Util.GetFromObjDict(parms, P_WIDTH, 1);
            int height = Util.GetFromObjDict(parms, P_HEIGHT, 1);
            bool flipRight = Util.GetFromObjDict(parms, P_FLIP_RIGHT, false);
            bool useRightTable = Util.GetFromObjDict(parms, P_RIGHT_TABLE, false);

            if (mChrRomOffset < 0) {
                mAppRef.ReportError("CHR_ROM symbol not found");
                return null;
            }

            if (offset < 0 || offset >= mFileData.Length) {
                mAppRef.ReportError("Invalid parameter");
                return null;
            }
            if (offset + width * height > mFileData.Length) {
                mAppRef.ReportError("Data runs off end of file");
                return null;
            }

            VisBitmap8 vb = new VisBitmap8(TileWidth * width, TileHeight * height);
            SetPalette(vb, (Palette)paletteNum);

            for (int row = 0; row < height; row++) {
                for (int col = 0; col < width; col++) {
                    int tileNum = mFileData[offset + row * width + col] +
                        (useRightTable ? 256 : 0);
                    RenderTile(tileNum, vb, TileWidth * col, TileHeight * row,
                        flipRight && col >= (width + 1) / 2);
                }
            }

            return vb;
        }

        /// <summary>
        /// Renders a tile from the PPU pattern table.
        /// </summary>
        /// <param name="tileNum">Tile number (0-511).</param>
        /// <param name="vb">Bitmap to render to.</param>
        /// <param name="xc">X coordinate for upper-left coordinate.</param>
        /// <param name="yc">Y coordinate for upper-left coordinate.</param>
        /// <param name="flipHoriz">Flip pixels horizontally</param>
        private void RenderTile(int tileNum, VisBitmap8 vb, int xc, int yc, bool flipHoriz) {
            int tileOff = mChrRomOffset + tileNum * BytesPerTile;
            for (int row = 0; row < 8; row++) {
                byte part0 = mFileData[tileOff];
                byte part1 = mFileData[tileOff + 8];
                for (int bit = 7; bit >= 0; bit--) {
                    int val = ((part0 >> bit) & 0x01) | (((part1 >> bit) & 0x01) << 1);
                    vb.SetPixelIndex(xc + (flipHoriz ? bit : 7 - bit), yc,
                        (byte)((byte)Color.Color0 + val));
                }

                tileOff++;
                yc++;
            }
        }

        private enum Color : byte {
            Transparent = 0,
            Black = 1,
            White = 2,
            Color0 = 3,
            Color1 = 4,
            Color2 = 5,
            Color3 = 6
        }

        private enum Palette : int {
            Greyscale = 0,
            Pinkish = 1,
            Greenish = 2,
        }

        private void SetPalette(VisBitmap8 vb, Palette pal) {
            vb.AddColor(0, 0, 0, 0);                // 0=transparent
            vb.AddColor(0xff, 0x01, 0x01, 0x01);    // 1=near black (so VB doesn't uniquify)
            vb.AddColor(0xff, 0xfe, 0xfe, 0xfe);    // 2=near white

            switch (pal) {
                case Palette.Greyscale:
                default:
                    vb.AddColor(0xff, 0x00, 0x00, 0x00);    // black
                    vb.AddColor(0xff, 0x80, 0x80, 0x80);    // dark grey
                    vb.AddColor(0xff, 0xb0, 0xb0, 0xb0);    // medium grey
                    vb.AddColor(0xff, 0xe0, 0xe0, 0xe0);    // light grey
                    break;
                case Palette.Pinkish:
                    vb.AddColor(0xff, 0x49, 0x99, 0xfe);    // sky blue
                    vb.AddColor(0xff, 0xff, 0xbd, 0xaf);    // pinkish
                    vb.AddColor(0xff, 0xcd, 0x50, 0x00);    // dark orange
                    vb.AddColor(0xff, 0x00, 0x00, 0x00);    // black
                    break;
                case Palette.Greenish:
                    vb.AddColor(0xff, 0x49, 0x99, 0xfe);    // sky blue
                    vb.AddColor(0xff, 0x00, 0xa4, 0x00);    // medium green
                    vb.AddColor(0xff, 0xfc, 0xfc, 0xfc);    // near white
                    vb.AddColor(0xff, 0xff, 0x99, 0x2b);    // orange
                    break;
            }
        }
    }
}
