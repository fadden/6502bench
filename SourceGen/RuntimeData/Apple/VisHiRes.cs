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
                    new VisParamDescr("Test Float",
                        "floaty", typeof(float), -5.0f, 5.0f, 0, 0.1f),
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
                        P_COUNT, typeof(int), 1, 256, 0, 1),
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
            return mDescriptors;
        }

        // IPlugin_Visualizer
        public IVisualization2d Generate2d(VisDescr descr,
                Dictionary<string, object> parms) {
            switch (descr.Ident) {
                case VIS_GEN_BITMAP:
                    return GenerateBitmap(parms);
                case VIS_GEN_BITMAP_FONT:
                    // TODO (xyzzy)
                    return null;
                default:
                    mAppRef.DebugLog("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualization2d GenerateBitmap(Dictionary<string, object> parms) {
            int offset, byteWidth, height, colStride, rowStride;
            bool isColor, isFirstOdd;

            offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            byteWidth = Util.GetFromObjDict(parms, P_BYTE_WIDTH, 1); // width ignoring colStride
            height = Util.GetFromObjDict(parms, P_HEIGHT, 1);
            colStride = Util.GetFromObjDict(parms, P_COL_STRIDE, 0);
            rowStride = Util.GetFromObjDict(parms, P_ROW_STRIDE, 0);
            isColor = Util.GetFromObjDict(parms, P_IS_COLOR, true);
            isFirstOdd = Util.GetFromObjDict(parms, P_IS_FIRST_ODD, false);

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
                mAppRef.DebugLog("Invalid parameter");
                return null;
            }
            if (colStride <= 0 || colStride > MAX_DIM) {
                mAppRef.DebugLog("Invalid column stride");
                return null;
            }
            if (rowStride < byteWidth * colStride - (colStride-1) || rowStride > MAX_DIM) {
                mAppRef.DebugLog("Invalid row stride");
                return null;
            }

            int lastOffset = offset + rowStride * height - (colStride - 1) - 1;
            if (lastOffset >= mFileData.Length) {
                mAppRef.DebugLog("Bitmap runs off end of file (last offset +" +
                    lastOffset.ToString("x6") + ")");
                return null;
            }

            VisBitmap8 vb = new VisBitmap8(byteWidth * 7, height);
            SetHiResPalette(vb);

            if (!isColor) {
                // B&W mode.  Since we're not displaying this we don't need to worry about
                // half-pixel shifts, and can just convert 7 bits to pixels.
                int bx = 0;
                int by = 0;
                for (int row = 0; row < height; row++) {
                    int colIdx = 0;
                    for (int col = 0; col < byteWidth; col++) {
                        byte val = mFileData[offset + colIdx];
                        for (int bit = 0; bit < 7; bit++) {
                            vb.SetPixelIndex(bx, by, (byte)((val & 0x01) + 1)); // black or white
                            val >>= 1;
                            bx++;
                        }
                        colIdx += colStride;
                    }
                    bx = 0;
                    by++;
                    offset += rowStride;
                }
            } else {
                int bx = 0;
                int by = 0;

#if false
                // Color mode.  We treat the data as a strictly 140-mode bitmap, which doesn't
                // quite match up with how the pixels will be displayed, but does allow a
                // straightforward conversion between file formats.  Color fringing is severe.
                for (int row = 0; row < height; row++) {
                    int lastBit;
                    if (isFirstOdd) {
                        lastBit = 0;        // pretend we already have one bit
                    } else {
                        lastBit = -1;
                    }

                    for (int colByte = 0; colByte < byteWidth; colByte += colStride) {
                        byte val = mFileData[offset + colByte];
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
                    bx = 0;
                    by++;
                    offset += rowStride;
                }
#else
                // Color conversion similar to what CiderPress does, but without the half-pixel
                // shift (we're trying to create a 1:1 bitmap, not 1:2).
                bool[] lineBits = new bool[byteWidth * 7];
                bool[] hiFlags = new bool[byteWidth * 7];   // overkill, but simplifies things
                int[] colorBuf = new int[byteWidth * 7];
                for (int row = 0; row < height; row++) {
                    // Unravel the bits.
                    int idx = 0;
                    int colIdx = 0;
                    for (int col = 0; col < byteWidth; col++) {
                        byte val = mFileData[offset + colIdx];
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
                        int colorShift = hiFlags[idx] ? 4 : 0;
                        if (!lineBits[idx]) {
                            // Bit not set, set pixel to black.
                            colorBuf[idx] = (int)HiResColors.Black0 + colorShift;
                        } else {
                            // Bit set, set pixel to white or color.
                            if (idx > 0 && colorBuf[idx - 1] != (int)HiResColors.Black0 &&
                                    colorBuf[idx - 1] != (int)HiResColors.Black1) {
                                // previous bit was also set, this is white
                                colorBuf[idx] = (int)HiResColors.White0 + colorShift;

                                // the previous pixel is part of a run of white
                                colorBuf[idx - 1] = (int)HiResColors.White0 + colorShift;
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

                            // Do we have a run of the same color?  If so, smooth the color out.
                            // Note that white blends smoothly with everything.
                            if (idx > 1 && (colorBuf[idx - 2] == colorBuf[idx] ||
                                    colorBuf[idx - 2] == (int)HiResColors.White0 ||
                                    colorBuf[idx - 2] == (int)HiResColors.White1)) {

                                //if (colorBuf[idx - 1] != (int)HiResColors.Black0 &&
                                //        colorBuf[idx - 1] != (int) HiResColors.Black1) {
                                //    mAppRef.DebugLog("Unexpected color at row=" + by +
                                //        " idx=" + idx + ": " + colorBuf[idx - 1]);
                                //}

                                colorBuf[idx - 1] = colorBuf[idx];
                            }
                        }
                    }

                    // Write to bitmap.
                    for (idx = 0; idx < lastBit; idx++) {
                        vb.SetPixelIndex(bx++, by, sHiResColorMap[colorBuf[idx]]);
                    }

                    // move to next row
                    bx = 0;
                    by++;
                    offset += rowStride;
                }
#endif
            }
            return vb;
        }

        private enum HiResColors {
            Black0      = 0,
            Green       = 1,
            Purple      = 2,
            White0      = 3,
            Black1      = 4,
            Orange      = 5,
            Blue        = 6,
            White1      = 7
        }

        // Maps HiResColors to the palette entries.
        private static readonly byte[] sHiResColorMap = new byte[8] {
            1, 3, 4, 2, 1, 5, 6, 2
        };

        private void SetHiResPalette(VisBitmap8 vb) {
            // These don't match directly to hi-res color numbers because we want to
            // avoid adding black/white twice.
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
