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
using System.Diagnostics;

using CommonUtil;
using PluginCommon;

namespace SourceGen {
    /// <summary>
    /// Renders a wireframe visualization, generating a collection of line segments in clip space.
    /// </summary>
    public class WireframeObject {
        /// <summary>
        /// Line segment.
        /// </summary>
        public class LineSeg {
            public double X0 { get; private set; }
            public double Y0 { get; private set; }
            public double X1 { get; private set; }
            public double Y1 { get; private set; }

            public LineSeg(double x0, double y0, double x1, double y1) {
                X0 = x0;
                Y0 = y0;
                X1 = x1;
                Y1 = y1;
            }
        }

        private class Vertex {
            public List<Face> Faces { get; private set; }

            public Vector3 Vec { get; private set; }

            public Vertex(double x, double y, double z) {
                Vec = new Vector3(x, y, z);
                Faces = new List<Face>();
            }
        }

        private class Edge {
            public Vertex Vertex0 { get; private set; }
            public Vertex Vertex1 { get; private set; }
            public List<Face> Faces { get; private set; }

            public Edge(Vertex v0, Vertex v1) {
                Vertex0 = v0;
                Vertex1 = v1;
                Faces = new List<Face>();
            }
        }

        private class Face {
            public Vector3 Normal { get; private set; }

            public Face(double x, double y, double z) {
                Normal = new Vector3(x, y, z);
                Normal.Normalize();
            }
        }

        private List<Vertex> mVertices = new List<Vertex>();
        private List<Edge> mEdges = new List<Edge>();
        private List<Face> mFaces = new List<Face>();
        private double mBigMag = -1.0;


        // private constructor; use Create()
        private WireframeObject() { }

        /// <summary>
        /// Creates a new object from a wireframe visualization.
        /// </summary>
        /// <param name="visWire">Visualization object.</param>
        /// <returns>New object.</returns>
        public static WireframeObject Create(IVisualizationWireframe visWire) {
            WireframeObject wireObj = new WireframeObject();

            //
            // Start by extracting data from the visualization object.  Everything stored
            // there is loaded into this object.
            //

            float[] normalsX = visWire.GetNormalsX();
            if (normalsX.Length > 0) {
                float[] normalsY = visWire.GetNormalsY();
                float[] normalsZ = visWire.GetNormalsZ();

                if (normalsX.Length != normalsY.Length || normalsX.Length != normalsZ.Length) {
                    Debug.Assert(false);
                    return null;
                }

                for (int i = 0; i < normalsX.Length; i++) {
                    wireObj.mFaces.Add(new Face(normalsX[i], normalsY[i], normalsZ[i]));
                }
            }

            float[] verticesX = visWire.GetVerticesX();
            float[] verticesY = visWire.GetVerticesY();
            float[] verticesZ = visWire.GetVerticesZ();
            if (verticesX.Length == 0) {
                Debug.Assert(false);
                return null;
            }
            if (verticesX.Length != verticesY.Length || verticesX.Length != verticesZ.Length) {
                Debug.Assert(false);
                return null;
            }

            for (int i = 0; i < verticesX.Length; i++) {
                wireObj.mVertices.Add(new Vertex(verticesX[i], verticesY[i], verticesZ[i]));
            }

            IntPair[] edges = visWire.GetEdges();
            for (int i = 0; i < edges.Length; i++) {
                int v0index = edges[i].Val0;
                int v1index = edges[i].Val1;

                if (v0index < 0 || v0index >= wireObj.mVertices.Count ||
                        v1index < 0 || v1index >= wireObj.mVertices.Count) {
                    Debug.Assert(false);
                    return null;
                }

                wireObj.mEdges.Add(
                    new Edge(wireObj.mVertices[v0index], wireObj.mVertices[v1index]));
            }

            IntPair[] vfaces = visWire.GetVertexFaces();
            for (int i = 0; i < vfaces.Length; i++) {
                int vindex = vfaces[i].Val0;
                int findex = vfaces[i].Val1;

                if (vindex < 0 || vindex >= wireObj.mVertices.Count ||
                        findex < 0 || findex >= wireObj.mFaces.Count) {
                    Debug.Assert(false);
                    return null;
                }

                wireObj.mVertices[vindex].Faces.Add(wireObj.mFaces[findex]);
            }

            IntPair[] efaces = visWire.GetEdgeFaces();
            for (int i = 0; i < efaces.Length; i++) {
                int eindex = efaces[i].Val0;
                int findex = efaces[i].Val1;

                if (eindex < 0 || eindex >= wireObj.mEdges.Count ||
                        findex < 0 || findex >= wireObj.mFaces.Count) {
                    Debug.Assert(false);
                    return null;
                }

                wireObj.mEdges[eindex].Faces.Add(wireObj.mFaces[findex]);
            }

            //
            // All data has been loaded into friendly classes.
            //

            // Compute the magnitude of the largest vertex, for scaling.
            double bigMag = -1.0;
            for (int i = 0; i < wireObj.mVertices.Count; i++) {
                double mag = wireObj.mVertices[i].Vec.Magnitude();
                if (bigMag < mag) {
                    bigMag = mag;
                }
            }
            wireObj.mBigMag = bigMag;

            return wireObj;
        }

        /// <summary>
        /// Generates a list of line segments for the wireframe data and the specified
        /// parameters.
        /// </summary>
        /// <param name="parms">Visualization parameters.</param>
        /// <returns>List of line segments, which could be empty if backface removal
        ///   was especially successful.</returns>
        public List<LineSeg> Generate(ReadOnlyDictionary<string, object> parms) {
            List<LineSeg> segs = new List<LineSeg>(mEdges.Count);

            // Perspective distance adjustment.
            const double zadj = 3.0;

            // Scale values to [-1,1].
            bool doPersp = Util.GetFromObjDict(parms, VisWireframe.P_IS_PERSPECTIVE, false);
            double scale = 1.0 / mBigMag;
            if (doPersp) {
                scale = (scale * zadj) / (zadj + 1);
            }

            foreach (Edge edge in mEdges) {
                double x0, y0, x1, y1;

                if (doPersp) {
                    // +Z is closer to the viewer, so we negate it here
                    double z0 = -edge.Vertex0.Vec.Z * scale;
                    double z1 = -edge.Vertex1.Vec.Z * scale;
                    x0 = (edge.Vertex0.Vec.X * scale * zadj) / (zadj + z0);
                    y0 = (edge.Vertex0.Vec.Y * scale * zadj) / (zadj + z0);
                    x1 = (edge.Vertex1.Vec.X * scale * zadj) / (zadj + z1);
                    y1 = (edge.Vertex1.Vec.Y * scale * zadj) / (zadj + z1);
                } else {
                    x0 = edge.Vertex0.Vec.X * scale;
                    y0 = edge.Vertex0.Vec.Y * scale;
                    x1 = edge.Vertex1.Vec.X * scale;
                    y1 = edge.Vertex1.Vec.Y * scale;
                }

                segs.Add(new LineSeg(x0, y0, x1, y1));
            }

            return segs;
        }
    }
}
