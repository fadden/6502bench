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
            // Surface normal.
            public Vector3 Normal { get; private set; }
            // One vertex on the face, for BFC.
            public Vertex Vert { get; set; }
            // Flag set during BFC calculation.
            public bool IsVisible { get; set; }

            public Face(double x, double y, double z) {
                Normal = new Vector3(x, y, z);
                Normal.Normalize();     // not necessary, but easier to read in debug output
                IsVisible = true;
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

                Face face = wireObj.mFaces[findex];
                wireObj.mVertices[vindex].Faces.Add(face);
                if (face.Vert == null) {
                    face.Vert = wireObj.mVertices[vindex];
                }
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

                Face face = wireObj.mFaces[findex];
                wireObj.mEdges[eindex].Faces.Add(face);
                if (face.Vert == null) {
                    face.Vert = wireObj.mEdges[eindex].Vertex0;
                }
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
        /// <param name="eulerX">Rotation about X axis.</params>
        /// <param name="eulerY">Rotation about Y axis.</params>
        /// <param name="eulerZ">Rotation about Z axis.</params>
        /// <param name="doPersp">Perspective or othographic projection?</param>
        /// <param name="doBfc">Perform backface culling?</param>
        /// <returns>List a of line segments, which could be empty if backface culling
        ///   was especially successful.</returns>
        public List<LineSeg> Generate(int eulerX, int eulerY, int eulerZ,
                bool doPersp, bool doBfc) {
            List<LineSeg> segs = new List<LineSeg>(mEdges.Count);

            // Camera Z coordinate adjustment, used to control how perspective projections
            // appear.  The larger the value, the farther the object appears to be.  Very
            // large values approximate an orthographic projection.
            const double zadj = 3.0;

            // Scale coordinate values to [-1,1].
            double scale = 1.0 / mBigMag;
            if (doPersp) {
                // objects closer to camera are bigger; reduce scale slightly
                scale = (scale * zadj) / (zadj + 0.5);
            }

            Matrix44 rotMat = new Matrix44();
            rotMat.SetRotationEuler(eulerX, eulerY, eulerZ);

            if (doBfc) {
                // Mark faces as visible or not.  This is determined with the surface normal,
                // rather than by checking whether a transformed triangle is clockwise.
                foreach (Face face in mFaces) {
                    // Transform the surface normal.
                    Vector3 rotNorm = rotMat.Multiply(face.Normal);
                    if (doPersp) {
                        // Transform one vertex to get a vector from the camera to the
                        // surface.  We want (V0 - C), where C is the camera; since we're
                        // at the origin, we just need -C.
                        if (face.Vert == null) {
                            Debug.WriteLine("GLITCH: no vertex for face");
                            face.IsVisible = true;
                            continue;
                        }
                        Vector3 camVec = rotMat.Multiply(face.Vert.Vec);
                        camVec.Multiply(-scale);    // scale to [-1,1] and negate to get -C
                        camVec.Z += zadj;           // translate

                        // Now compute the dot product of the camera vector.
                        double dot = Vector3.Dot(camVec, rotNorm);
                        face.IsVisible = (dot >= 0);
                        //Debug.WriteLine(string.Format(
                        //    "Face {0} vis={1,-5} dot={2,-8:N2}: camVec={3} rotNorm={4}",
                        //    index++, face.IsVisible, dot, camVec, rotNorm));
                    } else {
                        // For orthographic projection, the camera is essentially looking
                        // down the Z axis at every X,Y, so we can trivially check the
                        // value of Z in the transformed normal.
                        face.IsVisible = (rotNorm.Z >= 0);
                    }
                }
            }

            foreach (Edge edge in mEdges) {
                if (doBfc) {
                    // To be visible, vertices and edges must either not specify any
                    // faces, or must specify a visible face.
                    if (!IsVertexVisible(edge.Vertex0) ||
                            !IsVertexVisible(edge.Vertex1) ||
                            !IsEdgeVisible(edge)) {
                        continue;
                    }
                }

                Vector3 trv0 = rotMat.Multiply(edge.Vertex0.Vec);
                Vector3 trv1 = rotMat.Multiply(edge.Vertex1.Vec);
                double x0, y0, x1, y1;

                if (doPersp) {
                    // +Z on the shape is closer to the viewer, so we negate it here
                    double z0 = -trv0.Z * scale;
                    double z1 = -trv1.Z * scale;
                    x0 = (trv0.X * scale * zadj) / (zadj + z0);
                    y0 = (trv0.Y * scale * zadj) / (zadj + z0);
                    x1 = (trv1.X * scale * zadj) / (zadj + z1);
                    y1 = (trv1.Y * scale * zadj) / (zadj + z1);
                } else {
                    x0 = trv0.X * scale;
                    y0 = trv0.Y * scale;
                    x1 = trv1.X * scale;
                    y1 = trv1.Y * scale;
                }

                segs.Add(new LineSeg(x0, y0, x1, y1));
            }

            return segs;
        }

        private bool IsVertexVisible(Vertex vert) {
            if (vert.Faces.Count == 0) {
                return true;
            }
            foreach (Face face in vert.Faces) {
                if (face.IsVisible) {
                    return true;
                }
            }
            return false;
        }

        private bool IsEdgeVisible(Edge edg) {
            if (edg.Faces.Count == 0) {
                return true;
            }
            foreach (Face face in edg.Faces) {
                if (face.IsVisible) {
                    return true;
                }
            }
            return false;
        }
    }
}
