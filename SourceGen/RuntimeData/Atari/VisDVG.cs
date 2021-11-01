/*
 * Copyright 2021 faddenSoft
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
    /// <summary>
    /// Visualizer for Atari Digital Vector Generator commands (Asteroids, etc).
    ///
    /// Currently ignores beam brightness, except as on/off.
    ///
    /// References:
    ///  http://computerarcheology.com/Arcade/Asteroids/DVG.html
    ///  https://www.nicholasmikstas.com/asteroids-vector-rom
    ///
    /// https://github.com/mamedev/mame/blob/master/src/mame/video/avgdvg.cpp is the
    /// definitive description, but it's harder to parse than the above (it's emulating
    /// the hardware at a lower level).
    /// </summary>
    public class VisDVG : MarshalByRefObject, IPlugin, IPlugin_Visualizer_v2 {
        private readonly bool VERBOSE = false;

        // IPlugin
        public string Identifier {
            get { return "Atari DVG Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_GEN_DVG = "atari-dvg";

        private const string P_OFFSET = "offset";
        private const string P_BASE_ADDR = "baseAddr";
        private const string P_IGNORE_CUR = "ignoreCur";

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_DVG, "Atari DVG Commands", VisDescr.VisType.Wireframe,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Base address",
                        P_BASE_ADDR, typeof(int), 0x0000, 0xffff, 0, 0x4000),
                    new VisParamDescr("Ignore CUR movement",
                        P_IGNORE_CUR, typeof(bool), false, true, 0, false),

                    VisWireframe.Param_IsRecentered("Centered", true),
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
            if (mFileData == null) {
                return null;
            }
            return mDescriptors;
        }

        // IPlugin_Visualizer
        public IVisualization2d Generate2d(VisDescr descr,
                ReadOnlyDictionary<string, object> parms) {
            mAppRef.ReportError("2d not supported");
            return null;
        }

        // IPlugin_Visualizer_v2
        public IVisualizationWireframe GenerateWireframe(VisDescr descr,
                ReadOnlyDictionary<string, object> parms) {
            switch (descr.Ident) {
                case VIS_GEN_DVG:
                    return GenerateWireframe(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private enum Opcode {
            Unknown = 0, VEC, CUR, HALT, JSR, RTS, JMP, SVEC
        }

        private IVisualizationWireframe GenerateWireframe(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int baseAddr = Util.GetFromObjDict(parms, P_BASE_ADDR, 0);
            bool ignoreCur = Util.GetFromObjDict(parms, P_IGNORE_CUR, false);

            if (offset < 0 || offset >= mFileData.Length) {
                // should be caught by editor
                mAppRef.ReportError("Invalid parameter");
                return null;
            }

            VisWireframe vw = new VisWireframe();
            vw.Is2d = true;

            try {
                int[] stack = new int[4];
                int stackPtr = 0;

                double beamX = 0;
                double beamY = 0;
                int scaleFactor = 0;    // tiny
                bool done = false;
                int centerVertex = vw.AddVertex(0, 0, 0);
                int curVertex = centerVertex;

                while (!done && offset < mFileData.Length) {
                    ushort code0 = (ushort)Util.GetWord(mFileData, offset, 2, false);
                    offset += 2;
                    Opcode opc = GetOpcode(code0);

                    switch (opc) {
                        case Opcode.VEC: {  // SSSS -mYY YYYY YYYY | BBBB -mXX XXXX XXXX
                                ushort code1 = (ushort)Util.GetWord(mFileData, offset, 2, false);
                                offset += 2;

                                int yval = sign11(code0 & 0x07ff);
                                int xval = sign11(code1 & 0x07ff);
                                int localsc = code0 >> 12;   // local scale
                                int bb = code1 >> 12;   // brightness

                                double scale = CalcScaleVEC(scaleFactor + localsc);
                                double dx = xval * scale;
                                double dy = yval * scale;

                                beamX += dx;
                                beamY += dy;
                                if (bb == 0) {
                                    // move only
                                    curVertex = vw.AddVertex((float)beamX, (float)beamY, 0);
                                } else if (xval == 0 && yval == 0) {
                                    // plot point
                                    vw.AddPoint(curVertex);
                                    //mAppRef.DebugLog("PLOT v" + curVertex + ": "
                                    //    + beamX + "," + beamY);
                                } else {
                                    // draw line from previous vertex
                                    int newVertex = vw.AddVertex((float)beamX, (float)beamY, 0);
                                    vw.AddEdge(curVertex, newVertex);
                                    curVertex = newVertex;
                                }

                                if (VERBOSE) {
                                    mAppRef.DebugLog("VEC scale=" + localsc + " x=" + dx +
                                        " y=" + dy + " b=" + bb + " --> dx=" + dx +
                                        " dy=" + dy);
                                }
                            }
                            break;
                        case Opcode.CUR: {  // 1010 00yy yyyy yyyy | SSSS 00xx xxxx xxxx
                                ushort code1 = (ushort)Util.GetWord(mFileData, offset, 2, false);
                                offset += 2;

                                int yc = code0 & 0x07ff;
                                int xc = code1 & 0x07ff;
                                int scale = code1 >> 12;

                                if (!ignoreCur) {
                                    // Some things do a big screen movement before they start
                                    // drawing, which throws off the auto-scaling.  The output
                                    // looks better if we ignore the initial movement.
                                    beamX = xc;
                                    beamY = yc;
                                }
                                // Sign-extend the scale factor.  (It's usually 0 or 1 in ROM.)
                                byte left = (byte)(scale << 4);
                                scaleFactor = (sbyte)left >> 4;

                                if (VERBOSE) {
                                    mAppRef.DebugLog("CUR scale=" + scale + " x=" + xc +
                                        " y=" + yc);
                                }
                            }
                            break;
                        case Opcode.HALT:   // 1011 0000 0000 0000
                            if (stackPtr != 0) {
                                mAppRef.DebugLog("NOTE: encountered HALT with nonzero stack");
                            }
                            done = true;
                            break;
                        case Opcode.JSR: {  // 1100 aaaa aaaa aaaa
                                int vaddr = code0 & 0x0fff;
                                if (stackPtr == stack.Length) {
                                    mAppRef.ReportError("Stack overflow at +" +
                                        offset.ToString("x6"));
                                    return null;
                                }
                                stack[stackPtr++] = offset;
                                if (!Branch(vaddr, baseAddr, ref offset)) {
                                    return null;
                                }
                            }
                            break;
                        case Opcode.JMP: {  // 1110 aaaa aaaa aaaa
                                int vaddr = code0 & 0x0fff;
                                if (!Branch(vaddr, baseAddr, ref offset)) {
                                    return null;
                                }
                            }
                            break;
                        case Opcode.RTS:    // 1101 0000 0000 0000
                            if (stackPtr == 0) {
                                done = true;
                            } else {
                                offset = stack[--stackPtr];
                            }
                            break;
                        case Opcode.SVEC: { // 1111 smYY BBBB SmXX
                                int yval = sign3((code0 >> 8) & 0x07);
                                int xval = sign3(code0 & 0x07);
                                int localsc = ((code0 >> 11) & 0x01) | ((code0 >> 2) & 0x02);
                                // SVEC scale is VEC scale + 2
                                double scale = CalcScaleVEC(scaleFactor + localsc + 2);
                                int bb = (code0 >> 4) & 0x0f;

                                // The dx/dy values need to be * 256 to make them work right.
                                double dx = (xval << 8) * scale;
                                double dy = (yval << 8) * scale;
                                beamX += dx;
                                beamY += dy;

                                if (bb == 0) {
                                    // move only
                                    curVertex = vw.AddVertex((float)beamX, (float)beamY, 0);
                                } else if (xval == 0 && yval == 0) {
                                    // plot point
                                    vw.AddPoint(curVertex);
                                    //mAppRef.DebugLog("SPLOT v" + curVertex + ": "
                                    //    + beamX + "," + beamY);
                                } else {
                                    // draw line from previous vertex
                                    int newVertex = vw.AddVertex((float)beamX, (float)beamY, 0);
                                    vw.AddEdge(curVertex, newVertex);
                                    curVertex = newVertex;
                                }
                                if (VERBOSE) {
                                    mAppRef.DebugLog("SVEC scale=" + localsc + " x=" + dx +
                                        " y=" + dy + " b=" + bb + " --> dx=" + dx +
                                        " dy=" + dy);
                                }
                            }
                            break;
                        default:
                            mAppRef.ReportError("Unhandled code $" + code0.ToString("x4"));
                            return null;
                    }
                }

            } catch (IndexOutOfRangeException) {
                // assume it was our file data access that caused the failure
                mAppRef.ReportError("Ran off end of file");
                return null;
            }

            string msg;
            if (!vw.Validate(out msg)) {
                mAppRef.ReportError("Data error: " + msg);
                return null;
            }

            return vw;
        }

        private static Opcode GetOpcode(ushort code) {
            switch (code & 0xf000) {
                case 0xa000:    return Opcode.CUR;
                case 0xb000:    return Opcode.HALT;
                case 0xc000:    return Opcode.JSR;
                case 0xd000:    return Opcode.RTS;
                case 0xe000:    return Opcode.JMP;
                case 0xf000:    return Opcode.SVEC;
                default:        return Opcode.VEC;      // 0x0nnn - 0x9nnn
            }
        }

        // Convert scale factor (0-9) to a multiplier.
        private static double CalcScaleVEC(int scaleFactor) {
            // 0 is N/512, 9 is N/1.
            if (scaleFactor < 0) {
                scaleFactor = 0;
            } else if (scaleFactor > 9) {
                scaleFactor = 9;
            }
            return 1.0 / (1 << (9 - scaleFactor));
        }

        // Set the sign for a 2-bit value with a sign flag in the 3rd bit.
        private static int sign3(int val) {
            if ((val & 0x0004) == 0) {
                return val;
            } else {
                return -(val & 0x03);
            }
        }

        // Set the sign for a 10-bit value with a sign flag in the 11th bit.
        private static int sign11(int val) {
            if ((val & 0x0400) == 0) {
                return val;
            } else {
                return -(val & 0x03ff);
            }
        }

        /// <summary>
        /// Converts a JSR/JMP operand to a file offset.
        /// </summary>
        /// <param name="vaddr">DVG address operand.</param>
        /// <param name="baseAddr">Base address of vector memory.</param>
        /// <param name="offset">File offset of instruction.</param>
        /// <returns>False if the target address is outside the file bounds.</returns>
        private bool Branch(int vaddr, int baseAddr, ref int offset) {
            int fileAddr = baseAddr + vaddr * 2;
            int fileOffset = mAddrTrans.AddressToOffset(offset, fileAddr);
            if (fileOffset < 0) {
                mAppRef.ReportError("JMP/JSR to " + vaddr.ToString("x4") + " invalid");
                return false;
            }
            offset = fileOffset;
            return true;
        }
    }
}
