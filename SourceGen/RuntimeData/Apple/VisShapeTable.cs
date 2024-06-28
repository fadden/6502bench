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
using System.Collections.ObjectModel;

using PluginCommon;

namespace RuntimeData.Apple {
    /// <summary>
    /// Visualizer for Apple II shape tables.  While the only drawing functions for shape
    /// tables are defined as Applesoft hi-res functions, the format is not inherently
    /// limited to hi-res graphics.  It's really just a list of vectors.
    /// </summary>
    /// <remarks>
    /// <para>See the Applesoft BASIC Programming Reference Manual, pages 91-100, for a full
    /// description of the format.</para>
    ///
    /// <para>Table format:</para>
    /// <code>
    ///  +00    number of shapes
    ///  +01    (undefined)
    ///  +02/03 offset from start of table to first shape
    ///  +04/05 offset from start of table to second shape
    ///   ...
    ///  +xx    shape #1 data
    ///  +yy    shape #2 data
    /// </code>
    ///
    /// <para>Shape data is a series of bytes ending in $00.  Each byte holds three vectors,
    /// CCBBBAAA.  AAA and BBB specify a direction (up/right/down/left) and whether or
    /// not to plot a point.  CC cannot specify whether to plot and cannot move up (a 00
    /// in CC means "do nothing").</para>
    /// <para>The shape indices should start at 1, as is done in Applesoft, but an earlier
    /// version of this visualizer started at 0.  So we're stuck with it in the name of
    /// backward compatibility.</para>
    /// </remarks>
    ///
    /// TODO: optionally render as it would on the hi-res screen.  Some shapes draw with
    ///   HCOLOR=white but use alternating vertical lines to render multiple colors.
    /// TODO: support ROT, using Applesoft-style ugly rotation handling.  Could also support
    ///   SCALE but that's only interesting w.r.t. hi-res color changes.
    public class VisShapeTable : MarshalByRefObject, IPlugin, IPlugin_Visualizer {
        // IPlugin
        public string Identifier {
            get { return "Apple II Shape Table Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_GEN_SHAPE_TABLE = "apple2-shape-table";
        private const string VIS_GEN_SHAPE_TABLE_SHAPE = "apple2-shape-table-shape";

        private const string P_OFFSET = "offset";
        private const string P_INDEX = "index";

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_SHAPE_TABLE, "Apple II Shape Table", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Image index",
                        P_INDEX, typeof(int), 0, 255, 0, 0),
                }),
            new VisDescr(VIS_GEN_SHAPE_TABLE_SHAPE, "Apple II Shape Table Shape", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                }),
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
                case VIS_GEN_SHAPE_TABLE:
                    return GenerateBitmapFromTable(parms);
                case VIS_GEN_SHAPE_TABLE_SHAPE:
                    return GenerateBitmapFromShape(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualization2d GenerateBitmapFromTable(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int shapeIndex = Util.GetFromObjDict(parms, P_INDEX, 0);

            if (offset < 0 || offset >= mFileData.Length || shapeIndex < 0 || shapeIndex > 255) {
                mAppRef.ReportError("Invalid parameter");
                return null;
            }

            int maxIndex = mFileData[offset];   // count + 1
            if (shapeIndex >= maxIndex) {
                mAppRef.ReportError("Image index " + shapeIndex +
                    " exceeds table size " + maxIndex);
                return null;
            }

            int offOffset = offset + 2 + shapeIndex * 2;
            if (offOffset + 1 >= mFileData.Length) {
                mAppRef.ReportError("Offset for shape " + shapeIndex + " is outside file bounds");
                return null;
            }

            int shapeOffset = mFileData[offOffset] | (mFileData[offOffset + 1] << 8);
            return GenerateBitmap(shapeOffset);
        }

        private IVisualization2d GenerateBitmapFromShape(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            if (offset < 0 || offset >= mFileData.Length) {
                mAppRef.ReportError("Invalid parameter");
                return null;
            }
            return GenerateBitmap(offset);
        }

        private IVisualization2d GenerateBitmap(int shapeOffset) {
            // can't know the length of the shape ahead of time; will need to check as we go
            int xmin, xmax, ymin, ymax;
            xmin = ymin = 1000;
            xmax = ymax = -1000;

            // Execute once to get min/max and detect errors.
            VisBitmap8 vb;
            if (!PlotVectors(shapeOffset, ref xmin, ref xmax, ref ymin, ref ymax, false, out vb)) {
                return null;
            }
            //mAppRef.DebugLog("PLOTTED: xmin=" + xmin + " xmax=" + xmax +
            //    " ymin=" + ymin + " ymax=" + ymax);

            // Execute a second time to actually draw.
            PlotVectors(shapeOffset, ref xmin, ref xmax, ref ymin, ref ymax, true, out vb);
            return vb;
        }

        private bool PlotVectors(int shapeOffset, ref int xmin, ref int xmax,
                ref int ymin, ref int ymax, bool doPlot, out VisBitmap8 vb) {
            if (doPlot) {
                vb = new VisBitmap8(xmax - xmin + 1, ymax - ymin + 1);
                SetHiResPalette(vb);
            } else {
                vb = null;
            }

            int xc = 0;
            int yc = 0;

            while (true) {
                if (shapeOffset >= mFileData.Length) {
                    mAppRef.ReportError("Shape definition ran off end of file");
                    return false;
                }
                byte val = mFileData[shapeOffset++];
                if (val == 0) {
                    // done
                    break;
                }

                // "If all the remaining sections of the byte contain only zeroes, then those
                // sections are ignored."  We ignore C if it's zero, and if B and C are both zero
                // then both parts are ignored.
                int abits = val & 0x07;
                int bbits = (val >> 3) & 0x07;
                int cbits = val >> 6;

                DrawVector(abits, ref xc, ref yc, ref xmin, ref xmax, ref ymin, ref ymax, vb);
                if (bbits != 0 || cbits != 0) {
                    DrawVector(bbits, ref xc, ref yc, ref xmin, ref xmax, ref ymin, ref ymax, vb);
                }
                if (cbits != 0) {
                    DrawVector(cbits, ref xc, ref yc, ref xmin, ref xmax, ref ymin, ref ymax, vb);
                }
            }

            // Return true if we actually plotted something.
            if (xmax < 0 || ymax < 0) {
                mAppRef.ReportError("Shape definition doesn't draw anything");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Plots a point if appropriate, and updates xc/yc according to the vector.  The
        /// min/max values are updated if a point is plotted -- no need to expand the bitmap
        /// outside the range of actual plotted points.
        /// </summary>
        private void DrawVector(int bits, ref int xc, ref int yc,
                ref int xmin, ref int xmax, ref int ymin, ref int ymax, VisBitmap8 vb) {
            if ((bits & 0x04) != 0) {
                if (vb != null) {
                    // plot a point
                    vb.SetPixelIndex(xc - xmin, yc - ymin, 1);
                }
                if (xmin > xc) {
                    xmin = xc;
                }
                if (xmax < xc) {
                    xmax = xc;
                }
                if (ymin > yc) {
                    ymin = yc;
                }
                if (ymax < yc) {
                    ymax = yc;
                }
            }
            switch (bits & 0x03) {
                case 0x00:
                    // move up
                    yc--;
                    break;
                case 0x01:
                    // move right
                    xc++;
                    break;
                case 0x02:
                    // move down
                    yc++;
                    break;
                case 0x03:
                    // move left
                    xc--;
                    break;
            }
        }

        private void SetHiResPalette(VisBitmap8 vb) {
            vb.AddColor(0x00, 0x00, 0x00, 0x00);    // 0=transparent
            vb.AddColor(0xff, 0x20, 0x20, 0xe0);    // 1=solid (mostly blue)
        }
    }
}
