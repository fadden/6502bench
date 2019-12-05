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

namespace RuntimeData.Apple {
    public class VisHiRes : MarshalByRefObject, IPlugin, IPlugin_Visualizer {
        // IPlugin
        public string Identifier {
            get { return "Apple II Hi-Res Graphic Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_GEN_BITMAP = "apple2-hi-res-bitmap";
        private const string VIS_GEN_BITMAP_FONT = "apple2-hi-res-bitmap-font";
        private const string VIS_GEN_HR_SCREEN = "apple2-hi-res-screen";

        private const string P_OFFSET = "offset";
        private const string P_BYTE_WIDTH = "byteWidth";
        private const string P_HEIGHT = "height";
        private const string P_COL_STRIDE = "colStride";
        private const string P_ROW_STRIDE = "rowStride";
        private const string P_IS_COLOR = "isColor";
        private const string P_IS_FIRST_ODD = "isFirstOdd";

        private const string P_ITEM_BYTE_WIDTH = "itemByteWidth";
        private const string P_ITEM_HEIGHT = "itemHeight";
        private const string P_COUNT = "count";

        private const int MAX_DIM = 4096;

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_BITMAP, "Apple II Hi-Res Bitmap", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Width (in bytes)",
                        P_BYTE_WIDTH, typeof(int), 1, 40, 0, 1),
                    new VisParamDescr("Height",
                        P_HEIGHT, typeof(int), 1, 192, 0, 1),
                    new VisParamDescr("Column stride (bytes)",
                        P_COL_STRIDE, typeof(int), 0, 256, 0, 0),
                    new VisParamDescr("Row stride (bytes)",
                        P_ROW_STRIDE, typeof(int), 0, 256, 0, 0),
                    new VisParamDescr("Color",
                        P_IS_COLOR, typeof(bool), 0, 0, 0, true),
                    new VisParamDescr("First col odd",
                        P_IS_FIRST_ODD, typeof(bool), 0, 0, 0, false),
                    //new VisParamDescr("Test Float",
                    //    "floaty", typeof(float), -5.0f, 5.0f, 0, 0.1f),
                }),
            new VisDescr(VIS_GEN_BITMAP_FONT, "Apple II Hi-Res Bitmap Font", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Item width (in bytes)",
                        P_ITEM_BYTE_WIDTH, typeof(int), 1, 40, 0, 1),
                    new VisParamDescr("Item height",
                        P_ITEM_HEIGHT, typeof(int), 1, 192, 0, 8),
                    new VisParamDescr("Number of items",
                        P_COUNT, typeof(int), 1, 256, 0, 96),
                }),
            new VisDescr(VIS_GEN_HR_SCREEN, "Apple II Hi-Res Screen Image", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Color",
                        P_IS_COLOR, typeof(bool), 0, 0, 0, true),
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
                case VIS_GEN_BITMAP:
                    return GenerateBitmap(parms);
                case VIS_GEN_BITMAP_FONT:
                    return GenerateBitmapFont(parms);
                case VIS_GEN_HR_SCREEN:
                    return GenerateScreen(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualization2d GenerateBitmap(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int byteWidth = Util.GetFromObjDict(parms, P_BYTE_WIDTH, 1); // width ignoring colStride
            int height = Util.GetFromObjDict(parms, P_HEIGHT, 1);
            int colStride = Util.GetFromObjDict(parms, P_COL_STRIDE, 0);
            int rowStride = Util.GetFromObjDict(parms, P_ROW_STRIDE, 0);
            bool isColor = Util.GetFromObjDict(parms, P_IS_COLOR, true);
            bool isFirstOdd = Util.GetFromObjDict(parms, P_IS_FIRST_ODD, false);

            // We allow the stride entries to be zero to indicate a "dense" bitmap.
            if (colStride == 0) {
                colStride = 1;
            }
            if (rowStride == 0) {
                rowStride = byteWidth * colStride;
            }

            if (offset < 0 || offset >= mFileData.Length ||
                    byteWidth <= 0 || byteWidth > MAX_DIM ||
                    height <= 0 || height > MAX_DIM) {
                // the UI should flag these based on range (and ideally wouldn't have called us)
                mAppRef.ReportError("Invalid parameter");
                return null;
            }
            if (colStride <= 0 || colStride > MAX_DIM) {
                mAppRef.ReportError("Invalid column stride");
                return null;
            }
            if (rowStride < byteWidth * colStride - (colStride - 1) || rowStride > MAX_DIM) {
                mAppRef.ReportError("Invalid row stride");
                return null;
            }

            int lastOffset = offset + rowStride * height - (colStride - 1) - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Bitmap runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            VisBitmap8 vb = new VisBitmap8(byteWidth * 7, height);
            SetHiResPalette(vb);

            RenderBitmap(mFileData, offset, byteWidth, height, colStride, rowStride,
                isColor ? ColorMode.SimpleColor : ColorMode.Mono, isFirstOdd,
                vb, 0, 0);
            return vb;
        }

        private IVisualization2d GenerateBitmapFont(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int itemByteWidth = Util.GetFromObjDict(parms, P_ITEM_BYTE_WIDTH, 1);
            int itemHeight = Util.GetFromObjDict(parms, P_ITEM_HEIGHT, 8);
            int count = Util.GetFromObjDict(parms, P_COUNT, 96);

            if (offset < 0 || offset >= mFileData.Length ||
                    itemByteWidth <= 0 || itemByteWidth > MAX_DIM ||
                    itemHeight <= 0 || itemHeight > MAX_DIM) {
                // should be caught by editor
                mAppRef.ReportError("Invalid parameter");
                return null;
            }

            int lastOffset = offset + itemByteWidth * itemHeight - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Bitmap runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            // Set the number of horizontal cells to 16 or 32 based on the number of elements.
            int hcells;
            if (count > 128) {
                hcells = 32;
            } else {
                hcells = 16;
            }

            int vcells = (count + hcells - 1) / hcells;

            // Create a bitmap with room for each cell, plus a 1-pixel transparent boundary
            // between them and around the edges.
            VisBitmap8 vb = new VisBitmap8(1 + hcells * itemByteWidth * 7 + hcells,
                                           1 + vcells * itemHeight + vcells);
            SetHiResPalette(vb);

            int cellx = 1;
            int celly = 1;

            for (int idx = 0; idx < count; idx++) {

                //byte color = (byte)(1 + idx % 6);
                //for (int y = 0; y < itemHeight; y++) {
                //    for (int x = 0; x < itemByteWidth * 7; x++) {
                //        vb.SetPixelIndex(cellx + x, celly + y, color);
                //    }
                //}
                RenderBitmap(mFileData, offset + idx * itemByteWidth * itemHeight,
                    itemByteWidth, itemHeight, 1, itemByteWidth,
                    ColorMode.Mono, false,
                    vb, cellx, celly);

                cellx += itemByteWidth * 7 + 1;
                if (cellx == vb.Width) {
                    cellx = 1;
                    celly += itemHeight + 1;
                }
            }
            return vb;
        }

        private IVisualization2d GenerateScreen(ReadOnlyDictionary<string, object> parms) {
            const int RAW_IMAGE_SIZE = 0x1ff8;
            const int HR_WIDTH = 280;
            const int HR_BYTE_WIDTH = HR_WIDTH / 7;
            const int HR_HEIGHT = 192;

            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            bool isColor = Util.GetFromObjDict(parms, P_IS_COLOR, true);

            if (offset < 0 || offset >= mFileData.Length) {
                // should be caught by editor
                mAppRef.ReportError("Invalid parameter");
                return null;
            }

            int lastOffset = offset + RAW_IMAGE_SIZE - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.ReportError("Bitmap runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            // Linearize the data.
            byte[] buf = new byte[HR_BYTE_WIDTH * HR_HEIGHT];
            int outIdx = 0;
            for (int row = 0; row < HR_HEIGHT; row++) {
                // If row is ABCDEFGH, we want pppFGHCD EABAB000 (where p is zero for us).
                int low = ((row & 0xc0) >> 1) | ((row & 0xc0) >> 3) | ((row & 0x08) << 4);
                int high = ((row & 0x07) << 2) | ((row & 0x30) >> 4);
                int addr = (high << 8) | low;

                for (int col = 0; col < HR_BYTE_WIDTH; col++) {
                    buf[outIdx++] = mFileData[offset + addr + col];
                }
            }

            VisBitmap8 vb = new VisBitmap8(HR_WIDTH, HR_HEIGHT);
            SetHiResPalette(vb);
            RenderBitmap(buf, 0, HR_BYTE_WIDTH, HR_HEIGHT, 1, HR_BYTE_WIDTH,
                isColor ? ColorMode.SimpleColor : ColorMode.Mono, false,
                vb, 0, 0);
            return vb;
        }


        private enum ColorMode { Mono, ClunkyColor, SimpleColor, IIgsRGB };

        private void RenderBitmap(byte[] data, int offset, int byteWidth, int height,
                int colStride, int rowStride, ColorMode colorMode, bool isFirstOdd,
                VisBitmap8 vb, int xstart, int ystart) {
            int bx = xstart;
            int by = ystart;
            switch (colorMode) {
                case ColorMode.Mono: {
                        // Since we're not displaying this we don't need to worry about
                        // half-pixel shifts, and can just convert 7 bits to pixels.
                        for (int row = 0; row < height; row++) {
                            int colIdx = 0;
                            for (int col = 0; col < byteWidth; col++) {
                                byte val = data[offset + colIdx];
                                for (int bit = 0; bit < 7; bit++) {
                                    if ((val & 0x01) == 0) {
                                        vb.SetPixelIndex(bx, by, (int)HiResColors.Black0);
                                    } else {
                                        vb.SetPixelIndex(bx, by, (int)HiResColors.White0);
                                    }
                                    val >>= 1;
                                    bx++;
                                }
                                colIdx += colStride;
                            }
                            bx = xstart;
                            by++;
                            offset += rowStride;
                        }
                    }
                    break;
                case ColorMode.ClunkyColor: {
                        // We treat the data as a strictly 140-mode bitmap, which doesn't match
                        // up well with how the pixels will be displayed, but does allow a
                        // straightforward conversion between file formats.  Color fringing is
                        // severe.
                        for (int row = 0; row < height; row++) {
                            int lastBit;
                            if (isFirstOdd) {
                                lastBit = 0;        // pretend we already have one bit
                            } else {
                                lastBit = -1;
                            }

                            for (int colByte = 0; colByte < byteWidth; colByte += colStride) {
                                byte val = data[offset + colByte];
                                bool hiBitSet = (val & 0x80) != 0;

                                // Grab 3 or 4 pairs of bits.
                                int pairCount = (lastBit < 0) ? 3 : 4;
                                while (pairCount-- > 0) {
                                    int twoBits;
                                    if (lastBit >= 0) {
                                        // merge with bit from previous byte
                                        twoBits = (lastBit << 1) | (val & 0x01);
                                        val >>= 1;
                                        lastBit = -1;
                                    } else {
                                        // grab two bits
                                        twoBits = (val & 0x03);
                                        val >>= 2;
                                    }

                                    if (hiBitSet) {
                                        twoBits += 4;
                                    }

                                    // We're in 140 mode, so set two adjacent pixels.
                                    vb.SetPixelIndex(bx++, by, sHiResColorMap[twoBits]);
                                    vb.SetPixelIndex(bx++, by, sHiResColorMap[twoBits]);
                                }

                                bool thisEven = ((colByte & 0x01) == 0) ^ isFirstOdd;
                                if (thisEven) {
                                    // started in even column we have one bit left over
                                    lastBit = val & 0x01;
                                } else {
                                    // started in odd column, all bits consumed
                                    lastBit = -1;
                                }
                            }
                            bx = xstart;
                            by++;
                            offset += rowStride;
                        }
                    }
                    break;
                case ColorMode.SimpleColor: {
                        // Straightforward conversion, with no funky border effects.  This
                        // represents an idealized version of the hardware.

                        // Bits for every byte, plus a couple of "fake" bits on the ends so
                        // we don't have to throw range-checks everywhere.
                        const int OVER = 2;
                        bool[] lineBits = new bool[OVER + byteWidth * 7 + OVER];
                        bool[] hiFlags = new bool[OVER + byteWidth * 7 + OVER];
                        for (int row = 0; row < height; row++) {
                            // Unravel the bits.  Note we do each byte "backwards", i.e. the
                            // low bit (which is generally considered to be on the right) is
                            // the leftmost pixel.
                            int idx = OVER;     // start past "fake" bits
                            int colIdx = 0;
                            for (int col = 0; col < byteWidth; col++) {
                                byte val = data[offset + colIdx];
                                bool hiBitSet = (val & 0x80) != 0;

                                for (int bit = 0; bit < 7; bit++) {
                                    hiFlags[idx] = hiBitSet;
                                    lineBits[idx] = (val & 0x01) != 0;
                                    idx++;
                                    val >>= 1;
                                }
                                colIdx += colStride;
                            }

                            // Convert to color.
                            int lastBit = byteWidth * 7;
                            for (idx = OVER; idx < lastBit + OVER; idx++) {
                                int colorShift = hiFlags[idx] ? 2 : 0;
                                if (lineBits[idx] && (lineBits[idx - 1] || lineBits[idx + 1])) {
                                    // [X]11 or [1]1X; two 1s in a row is always white
                                    vb.SetPixelIndex(bx++, by, (byte)HiResColors.White0);
                                } else if (lineBits[idx]) {
                                    // [0]10, color pixel
                                    bool isOdd = ((idx & 0x01) != 0) ^ isFirstOdd;
                                    if (isOdd) {
                                        vb.SetPixelIndex(bx++, by,
                                                (byte)((int)HiResColors.Green + colorShift));
                                    } else {
                                        vb.SetPixelIndex(bx++, by,
                                                (byte)((int)HiResColors.Purple + colorShift));
                                    }
                                } else if (lineBits[idx - 1] && lineBits[idx + 1]) {
                                    // [1]01, keep color going
                                    bool isOdd = ((idx & 0x01) != 0) ^ isFirstOdd;
                                    if (isOdd) {
                                        vb.SetPixelIndex(bx++, by,
                                                (byte)((int)HiResColors.Purple + colorShift));
                                    } else {
                                        vb.SetPixelIndex(bx++, by,
                                                (byte)((int)HiResColors.Green + colorShift));
                                    }
                                } else {
                                    // [0]0X or [X]01
                                    vb.SetPixelIndex(bx++, by, (byte)HiResColors.Black0);
                                }
                            }

                            // move to next row
                            bx = xstart;
                            by++;
                            offset += rowStride;
                        }
                    }
                    break;
                case ColorMode.IIgsRGB: {
                        // Color conversion similar to what CiderPress does, but without the
                        // half-pixel shift (we're trying to create a 1:1 bitmap, not 1:2).
                        //
                        // This replicates some of the oddness in Apple IIgs RGB monitor output,
                        // but it's not quite right though.  For example:
                        //
                        //                   observed                 generated
                        //  d5 2a:    blue   [dk blue] purple       ... black ...
                        //  aa 55:    orange [yellow]  green        ... white ...
                        //  55 aa:    purple [lt blue] blue         ... black ...
                        //  2a d5:    green  [brown]   orange       ... black ...
                        //
                        // KEGS doesn't seem to try to model this; it shows solid colors with no
                        // wackiness.  AppleWin in "Color TV" mode shows similar effects, but is
                        // much blurrier (by design).
                        bool[] lineBits = new bool[byteWidth * 7];
                        bool[] hiFlags = new bool[byteWidth * 7];   // overkill, but simpler
                        int[] colorBuf = new int[byteWidth * 7];
                        for (int row = 0; row < height; row++) {
                            // Unravel the bits.
                            int idx = 0;
                            int colIdx = 0;
                            for (int col = 0; col < byteWidth; col++) {
                                byte val = data[offset + colIdx];
                                bool hiBitSet = (val & 0x80) != 0;

                                for (int bit = 0; bit < 7; bit++) {
                                    hiFlags[idx] = hiBitSet;
                                    lineBits[idx] = (val & 0x01) != 0;
                                    idx++;
                                    val >>= 1;
                                }
                                colIdx += colStride;
                            }

                            // Convert to color.
                            int lastBit = byteWidth * 7;
                            for (idx = 0; idx < lastBit; idx++) {
                                int colorShift = hiFlags[idx] ? 2 : 0;
                                if (!lineBits[idx]) {
                                    // Bit not set, set pixel to black.
                                    colorBuf[idx] = (int)HiResColors.Black0;
                                } else {
                                    // Bit set, set pixel to white or color.
                                    if (idx > 0 && colorBuf[idx - 1] != (int)HiResColors.Black0) {
                                        // previous bit was also set, this is white
                                        colorBuf[idx] = (int)HiResColors.White0;

                                        // the previous pixel is part of a run of white
                                        colorBuf[idx - 1] = (int)HiResColors.White0;
                                    } else {
                                        // previous bit not set *or* was first pixel in line;
                                        // set color based on whether this is even or odd pixel col
                                        bool isOdd = ((idx & 0x01) != 0) ^ isFirstOdd;
                                        if (isOdd) {
                                            colorBuf[idx] = (int)HiResColors.Green + colorShift;
                                        } else {
                                            colorBuf[idx] = (int)HiResColors.Purple + colorShift;
                                        }
                                    }

                                    // Do we have a run of the same color?  If so, smooth the
                                    // color out. Note that white blends smoothly with everything.
                                    if (idx > 1 && (colorBuf[idx - 2] == colorBuf[idx] ||
                                            colorBuf[idx - 2] == (int)HiResColors.White0)) {
                                        colorBuf[idx - 1] = colorBuf[idx];
                                    }
                                }
                            }

                            // Write to bitmap.
                            for (idx = 0; idx < lastBit; idx++) {
                                vb.SetPixelIndex(bx++, by, (byte)colorBuf[idx]);
                            }

                            // move to next row
                            bx = xstart;
                            by++;
                            offset += rowStride;
                        }
                    }
                    break;
                default:
                    // just leave the bitmap empty
                    mAppRef.ReportError("Unknown ColorMode " + colorMode);
                    break;
            }
        }

        /// <summary>
        /// Map hi-res colors to palette entries.
        /// </summary>
        private enum HiResColors : byte {
            Black0      = 1,
            Green       = 3,
            Purple      = 4,
            White0      = 2,
            Black1      = 1,
            Orange      = 5,
            Blue        = 6,
            White1      = 2
        }

        // Maps HiResColors to the palette entries.  Used for mapping 2-bit values to colors.
        private static readonly byte[] sHiResColorMap = new byte[8] {
            1, 3, 4, 2, 1, 5, 6, 2
        };

        private void SetHiResPalette(VisBitmap8 vb) {
            // These don't match directly to hi-res color numbers because we want to
            // avoid adding black/white twice.  The colors correspond to Apple IIgs RGB
            // monitor output.
            vb.AddColor(0, 0, 0, 0);                // 0=transparent
            vb.AddColor(0xff, 0x00, 0x00, 0x00);    // 1=black0/black1
            vb.AddColor(0xff, 0xff, 0xff, 0xff);    // 2=white0/white1
            vb.AddColor(0xff, 0x11, 0xdd, 0x00);    // 3=green
            vb.AddColor(0xff, 0xdd, 0x22, 0xdd);    // 4=purple
            vb.AddColor(0xff, 0xff, 0x66, 0x00);    // 5=orange
            vb.AddColor(0xff, 0x22, 0x22, 0xff);    // 6=blue
        }
    }
}
