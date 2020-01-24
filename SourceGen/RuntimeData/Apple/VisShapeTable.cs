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
    /// See the Applesoft BASIC Programming Reference Manual, pages 91-100, for a full
    /// description of the format.
    ///
    /// Table format:
    ///  +00    number of shapes
    ///  +01    (unused)
    ///  +02/03 offset from start of table to first shape
    ///  +04/05 offset from start of table to second shape
    ///   ...
    ///  +xx    shape #1 data
    ///  +yy    shape #2 data
    ///
    /// Shape data is a series of bytes ending in $00.  Each byte holds three vectors,
    /// CCBBBAAA.  AAA and BBB specify a direction (up/right/down/left) and whether or
    /// not to plot a point.  CC cannot specify whether to plot and cannot move up (a 00
    /// in CC means "do nothing").
    /// </remarks>
    public class VisShapeTable : MarshalByRefObject, IPlugin, IPlugin_Visualizer {
        // IPlugin
        public string Identifier {
            get { return "Apple II Shape Table Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_GEN_SHAPE_TABLE = "apple2-shape-table";

        private const string P_OFFSET = "offset";
        private const string P_INDEX = "index";

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_SHAPE_TABLE, "Apple II Shape Table", VisDescr.VisType.Bitmap,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Image index",
                        P_INDEX, typeof(int), 0, 256, 0, 0),
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
                    return GenerateBitmap(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualization2d GenerateBitmap(ReadOnlyDictionary<string, object> parms) {
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

                int bits = val & 0x07;
                DrawVector(bits, false, ref xc, ref yc, ref xmin, ref xmax, ref ymin, ref ymax, vb);
                bits = (val >> 3) & 0x07;
                DrawVector(bits, false, ref xc, ref yc, ref xmin, ref xmax, ref ymin, ref ymax, vb);
                bits = (val >> 6) & 0x03;
                DrawVector(bits, true, ref xc, ref yc, ref xmin, ref xmax, ref ymin, ref ymax, vb);
            }

            return true;
        }

        /// <summary>
        /// Plots a point if appropriate, and updates xc/yc according to the vector.  The
        /// min/max values are updated if a point is plotted -- no need to expand the bitmap
        /// outside the range of actual plotted points.
        /// </summary>
        private void DrawVector(int bits, bool isC, ref int xc, ref int yc,
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
                    if (isC) {
                        // do nothing
                    } else {
                        // move up
                        yc--;
                    }
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
