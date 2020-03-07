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

namespace WireframeTest {
    /// <summary>
    /// Visualizer for wireframe test data.
    /// </remarks>
    public class VisWireframeTest : MarshalByRefObject, IPlugin, IPlugin_Visualizer_v2 {
        // IPlugin
        public string Identifier {
            get { return "Wireframe Test Data Visualizer"; }
        }
        private IApplication mAppRef;
        private byte[] mFileData;

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_GEN_WF_TEST = "wireframe-test";

        private const string P_OFFSET = "offset";

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_GEN_WF_TEST, "Wireframe Test Data", VisDescr.VisType.Wireframe,
                new VisParamDescr[] {
                    new VisParamDescr("File offset (hex)",
                        P_OFFSET, typeof(int), 0, 0x00ffffff, VisParamDescr.SpecialMode.Offset, 0),

                    // These are interpreted by the main app.
                    VisWireframe.Param_EulerX("Rotation about X", 0),
                    VisWireframe.Param_EulerY("Rotation about Y", 0),
                    VisWireframe.Param_EulerZ("Rotation about Z", 0),
                    VisWireframe.Param_IsPerspective("Perspective projection", true),
                    VisWireframe.Param_IsBfcEnabled("Backface culling", true),
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
            mAppRef.ReportError("2d not supported");
            return null;
        }

        // IPlugin_Visualizer_v2
        public IVisualizationWireframe GenerateWireframe(VisDescr descr,
                ReadOnlyDictionary<string, object> parms) {
            switch (descr.Ident) {
                case VIS_GEN_WF_TEST:
                    return GenerateWireframe(parms);
                default:
                    mAppRef.ReportError("Unknown ident " + descr.Ident);
                    return null;
            }
        }

        private IVisualizationWireframe GenerateWireframe(ReadOnlyDictionary<string, object> parms) {
            int offset = Util.GetFromObjDict(parms, P_OFFSET, 0);

            if (offset < 0 || offset >= mFileData.Length) {
                // should be caught by editor
                mAppRef.ReportError("Invalid parameter");
                return null;
            }

            VisWireframe vw = new VisWireframe();
            const sbyte END_MARKER = -128;  // 0x80
            try {
                while (true) {
                    int vx = (sbyte)mFileData[offset++];
                    if (vx == END_MARKER) {
                        break;
                    }
                    int vy = (sbyte)mFileData[offset++];
                    int vz = (sbyte)mFileData[offset++];

                    vw.AddVertex(vx, vy, vz);
                }

                while (true) {
                    int v0 = (sbyte)mFileData[offset++];
                    if (v0 == END_MARKER) {
                        break;
                    }
                    int v1 = mFileData[offset++];
                    int f0 = mFileData[offset++];
                    int f1 = mFileData[offset++];

                    int edge = vw.AddEdge(v0, v1);
                    vw.AddEdgeFace(edge, f0);
                    if (f1 != f0) {
                        vw.AddEdgeFace(edge, f1);
                    }
                }

                while (true) {
                    int nx = (sbyte)mFileData[offset++];
                    if (nx == END_MARKER) {
                        break;
                    }
                    int ny = (sbyte)mFileData[offset++];
                    int nz = (sbyte)mFileData[offset++];

                    vw.AddFaceNormal(nx, ny, nz);
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
    }
}
