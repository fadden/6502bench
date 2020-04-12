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

namespace RuntimeData.Atari {
    /// <summary>
    /// Visualizer for Atari Analog Vector Generator commands.
    /// </summary>
    public class VisAVG : MarshalByRefObject, IPlugin, IPlugin_Visualizer_v2 {
        // IPlugin
        public string Identifier {
            get { return "Atari AVG Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;
        private AddressTranslate mAddrTrans;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_GEN_AVG = "atari-avg";

        private const string P_OFFSET = "offset";
        private const string P_BASE_ADDR = "baseAddr";

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_AVG, "Atari AVG Commands", VisDescr.VisType.Wireframe,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),
                    new VisParamDescr("Base address",
                        P_BASE_ADDR, typeof(int), 0x0000, 0xffff, 0, 0x2000),

                    VisWireframe.Param_IsRecentered("Re-center", true),
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
                case VIS_GEN_AVG:
                    return GenerateWireframe(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private enum Opcode {
            Unknown = 0, VCTR, HALT, SVEC, STAT, SCAL, CNTR, JSR, RTS, JMP
        }

        private IVisualizationWireframe GenerateWireframe(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);
            int baseAddr = Util.GetFromObjDict(parms, P_BASE_ADDR, 0);

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

                int beamX = 0;
                int beamY = 0;
                double scale = 1.0;
                bool done = false;
                int centerVertex = vw.AddVertex(0, 0, 0);
                int curVertex = centerVertex;

                while (!done && offset < mFileData.Length) {
                    ushort code0 = (ushort)Util.GetWord(mFileData, offset, 2, false);
                    offset += 2;
                    Opcode opc = GetOpcode(code0);

                    int dx, dy, ii, vaddr;
                    switch (opc) {
                        case Opcode.VCTR:   // 000YYYYY YYYYYYYY  IIIXXXXX XXXXXXXX
                            ushort code1 = (ushort)Util.GetWord(mFileData, offset, 2, false);
                            offset += 2;
                            dy = sign13(code0 & 0x1fff);
                            dx = sign13(code1 & 0x1fff);
                            ii = code1 >> 13;

                            beamX += (int)Math.Round(dx * scale);
                            beamY += (int)Math.Round(dy * scale);
                            if (ii == 0) {
                                // move only
                                curVertex = vw.AddVertex(beamX, beamY, 0);
                            } else if (dx == 0 && dy == 0) {
                                // plot point
                                vw.AddPoint(curVertex);
                                //mAppRef.DebugLog("PLOT v" + curVertex + ": "
                                //    + beamX + "," + beamY);
                            } else {
                                // draw line from previous vertex
                                int newVertex = vw.AddVertex(beamX, beamY, 0);
                                vw.AddEdge(curVertex, newVertex);
                                curVertex = newVertex;
                            }
                            break;
                        case Opcode.HALT:   // 00100000 00100000
                            if (stackPtr != 0) {
                                mAppRef.DebugLog("NOTE: encountered HALT with nonzero stack");
                            }
                            done = true;
                            break;
                        case Opcode.SVEC:   // 010YYYYY IIIXXXXX
                            dy = sign5((code0 >> 8) & 0x1f) * 2;
                            dx = sign5(code0 & 0x1f) * 2;
                            ii = (code0 >> 5) & 0x07;
                            if (ii != 1) {
                                ii *= 2;
                            }

                            // note dx/dy==0 is not supported for SVEC
                            beamX += (int)Math.Round(dx * scale);
                            beamY += (int)Math.Round(dy * scale);
                            if (ii == 0) {
                                // move only
                                curVertex = vw.AddVertex(beamX, beamY, 0);
                            } else {
                                // draw line from previous vertex
                                int newVertex = vw.AddVertex(beamX, beamY, 0);
                                vw.AddEdge(curVertex, newVertex);
                                //mAppRef.DebugLog("SVEC edge " + curVertex + " - " + newVertex);
                                curVertex = newVertex;
                            }
                            break;
                        case Opcode.STAT:   // 0110-EHO IIIICCCC
                            ii = (code0 >> 4) & 0x0f;
                            break;
                        case Opcode.SCAL:   // 0111-BBB LLLLLLLL
                            int bs = (code0 >> 8) & 0x07;
                            int ls = code0 & 0xff;
                            scale = (16384 - (ls << 6)) >> bs;
                            break;
                        case Opcode.CNTR:   // 10000000 01------
                            beamX = beamY = 0;
                            curVertex = centerVertex;
                            break;
                        case Opcode.JSR:    // 101-AAAA AAAAAAAA
                            vaddr = code0 & 0x0fff;
                            if (stackPtr == stack.Length) {
                                mAppRef.ReportError("Stack overflow at +" + offset.ToString("x6"));
                                return null;
                            }
                            stack[stackPtr++] = offset;
                            if (!Branch(vaddr, baseAddr, ref offset)) {
                                return null;
                            }
                            break;
                        case Opcode.RTS:    // 110----- --------
                            if (stackPtr == 0) {
                                done = true;
                            } else {
                                offset = stack[--stackPtr];
                            }
                            break;
                        case Opcode.JMP:    // 111-AAAA AAAAAAAA
                            vaddr = code0 & 0x0fff;
                            if (!Branch(vaddr, baseAddr, ref offset)) {
                                return null;
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

        private Opcode GetOpcode(ushort code) {
            switch (code & 0xe000) {
                case 0x0000:    return Opcode.VCTR;
                case 0x2000:    return Opcode.HALT;
                case 0x4000:    return Opcode.SVEC;
                case 0x6000:    return ((code & 0xf000) == 0x6000) ? Opcode.STAT : Opcode.SCAL;
                case 0x8000:    return Opcode.CNTR;
                case 0xa000:    return Opcode.JSR;
                case 0xc000:    return Opcode.RTS;
                case 0xe000:    return Opcode.JMP;
                default:        return Opcode.Unknown;      // shouldn't be possible
            }
        }

        // Sign-extend a signed 5-bit value.
        int sign5(int val) {
            byte val5 = (byte)(val << 3);
            return (sbyte)val5 >> 3;
        }

        // Sign-extend a signed 13-bit value.
        int sign13(int val) {
            ushort val13 = (ushort)(val << 3);
            return (short)val13 >> 3;
        }

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
