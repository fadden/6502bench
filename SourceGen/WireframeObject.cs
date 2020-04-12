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
using System.Diagnostics;

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
            public Vector3 Vec { get; private set; }
            public List<Face> Faces { get; private set; }
            public bool IsExcluded { get; private set; }

            public Vertex(double x, double y, double z, bool isExcluded) {
                Vec = new Vector3(x, y, z);
                Faces = new List<Face>();
                IsExcluded = isExcluded;
            }

            public override string ToString() {
                return Vec.ToString() + " + " + Faces.Count + " faces";
            }
        }

        private class Edge {
            public Vertex Vertex0 { get; private set; }
            public Vertex Vertex1 { get; private set; }
            public List<Face> Faces { get; private set; }
            public bool IsExcluded { get; private set; }

            public Edge(Vertex v0, Vertex v1, bool isExcluded) {
                Vertex0 = v0;
                Vertex1 = v1;
                Faces = new List<Face>();
                IsExcluded = isExcluded;
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

        private bool mIs2d = false;
        private List<Vertex> mVertices = new List<Vertex>();
        private List<Vertex> mPoints = new List<Vertex>();
        private List<Edge> mEdges = new List<Edge>();
        private List<Face> mFaces = new List<Face>();
        private double mBigMag = -1.0;
        private double mBigMagRc = -1.0;
        private double mCenterAdjX, mCenterAdjY;


        // private constructor; use Create()
        private WireframeObject() { }

        /// <summary>
        /// Creates a new object from a wireframe visualization.
        /// </summary>
        /// <param name="visWire">Visualization object.</param>
        /// <returns>New object.</returns>
        public static WireframeObject Create(IVisualizationWireframe visWire) {
            WireframeObject wireObj = new WireframeObject();

            wireObj.mIs2d = visWire.Is2d;

            //
            // Start by extracting data from the visualization object.  Everything stored
            // there is loaded into this object.  The VisWireframe validator will have
            // ensured that all the indices are in range.
            //
            // IMPORTANT: do not retain "visWire", as it may be a proxy for an object with a
            // limited lifespan.
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
            int[] excludedVertices = visWire.GetExcludedVertices();
            if (verticesX.Length == 0) {
                Debug.Assert(false);
                return null;
            }
            if (verticesX.Length != verticesY.Length || verticesX.Length != verticesZ.Length) {
                Debug.Assert(false);
                return null;
            }

            // Compute min/max for X/Y for 2d re-centering.  The trick is that we only want
            // to use vertices that are visible.  If the shape starts with a huge move off to
            // the left, we don't want to include (0,0).
            double xmin, xmax, ymin, ymax;
            xmin = ymin = 10e9;
            xmax = ymax = -10e9;

            for (int i = 0; i < verticesX.Length; i++) {
                wireObj.mVertices.Add(new Vertex(verticesX[i], verticesY[i], verticesZ[i],
                    HasIndex(excludedVertices, i)));
            }

            int[] points = visWire.GetPoints();
            for (int i = 0; i < points.Length; i++) {
                Vertex vert = wireObj.mVertices[points[i]];
                wireObj.mPoints.Add(vert);
                UpdateMinMax(vert, ref xmin, ref xmax, ref ymin, ref ymax);
            }

            IntPair[] edges = visWire.GetEdges();
            int[] excludedEdges = visWire.GetExcludedEdges();
            for (int i = 0; i < edges.Length; i++) {
                int v0index = edges[i].Val0;
                int v1index = edges[i].Val1;

                if (v0index < 0 || v0index >= wireObj.mVertices.Count ||
                        v1index < 0 || v1index >= wireObj.mVertices.Count) {
                    Debug.Assert(false);
                    return null;
                }

                Vertex vert0 = wireObj.mVertices[v0index];
                Vertex vert1 = wireObj.mVertices[v1index];
                wireObj.mEdges.Add(new Edge(vert0, vert1, HasIndex(excludedEdges, i)));

                UpdateMinMax(vert0, ref xmin, ref xmax, ref ymin, ref ymax);
                UpdateMinMax(vert1, ref xmin, ref xmax, ref ymin, ref ymax);
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

            // Compute center of visible vertices.
            wireObj.mCenterAdjX = -(xmin + xmax) / 2;
            wireObj.mCenterAdjY = -(ymin + ymax / 2);

            // Compute the magnitude of the largest vertex, for scaling.
            double bigMag = -1.0;
            double bigMagRc = -1.0;
            for (int i = 0; i < wireObj.mVertices.Count; i++) {
                Vector3 vec = wireObj.mVertices[i].Vec;
                double mag = vec.Magnitude();
                if (bigMag < mag) {
                    bigMag = mag;
                }

                // Repeat the operation with recentering.  This isn't quite right as we're
                // including all vertices, not just the visible ones.
                mag = new Vector3(vec.X + wireObj.mCenterAdjX,
                    vec.Y + wireObj.mCenterAdjY, vec.Z).Magnitude();
                if (bigMagRc < mag) {
                    bigMagRc = mag;
                }
            }
            wireObj.mBigMag = bigMag;
            wireObj.mBigMagRc = bigMagRc;

            return wireObj;
        }

        private static void UpdateMinMax(Vertex vert, ref double xmin, ref double xmax,
                ref double ymin, ref double ymax) {
            if (vert.Vec.X < xmin) {
                xmin = vert.Vec.X;
            } else if (vert.Vec.X > xmax) {
                xmax = vert.Vec.X;
            }
            if (vert.Vec.Y < ymin) {
                ymin = vert.Vec.Y;
            } else if (vert.Vec.Y > ymax) {
                ymax = vert.Vec.Y;
            }
        }

        private static bool HasIndex(int[] arr, int val) {
            for (int i = 0; i < arr.Length; i++) {
                if (arr[i] == val) {
                    return true;
                }
            }
            return false;
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
        /// <param name="doRecenter">Re-center 2D renderings?</param>
        /// <returns>List a of line segments, which could be empty if backface culling
        ///   was especially successful.  All segment coordinates are in the range
        ///   [-1,1].</returns>
        public List<LineSeg> Generate(int eulerX, int eulerY, int eulerZ,
                bool doPersp, bool doBfc, bool doRecenter) {
            // overrule flags that don't make sense
            if (mIs2d) {
                doPersp = doBfc = false;
            } else {
                doRecenter = false;
            }

            List<LineSeg> segs = new List<LineSeg>(mEdges.Count);

            // Camera Z coordinate adjustment, used to control how perspective projections
            // appear.  The larger the value, the farther the object appears to be.  Very
            // large values approximate an orthographic projection.
            const double zadj = 3.0;

            // Scale coordinate values to [-1,1].
            double scale;
            if (doRecenter) {
                scale = 1.0 / mBigMagRc;
            } else {
                scale = 1.0 / mBigMag;
            }
            if (doPersp) {
                // objects closer to camera are bigger; reduce scale slightly
                scale = (scale * zadj) / (zadj + 0.3);
            }

            // Configure X/Y translation for 2D wireframes.
            double transX = 0;
            double transY = 0;
            if (doRecenter) {
                transX = mCenterAdjX;
                transY = mCenterAdjY;
            }

            // In a left-handed coordinate system, +Z is away from the viewer.  The
            // visualizer expects a left-handed system with the "nose" aimed toward +Z,
            // which leaves us looking at the back end of things.  We can add a 180 degree
            // rotation about Y so we're looking at the front instead, though this
            // effectively reverses the direction of rotation about X.  We can compensate
            // for it by reversing the handedness of the X rotation.
            //eulerY = (eulerY + 180) % 360;

            // Form rotation matrix.
            Matrix33 rotMat = new Matrix33();
            rotMat.SetRotationEuler(eulerX, eulerY, eulerZ, Matrix33.RotMode.ZYX_LLL);
            //Debug.WriteLine("ROT: " + rotMat);

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
                        Vector3 camVec = rotMat.Multiply(face.Vert.Vec);    // transform
                        camVec = camVec.Multiply(-scale);   // scale to [-1,1] and negate to get -C
                        camVec = camVec.Add(new Vector3(0, 0, -zadj));      // translate

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
                        face.IsVisible = (rotNorm.Z <= 0);
                    }
                }
            }

            foreach (Vertex point in mPoints) {
                // There are no "point faces" at the moment, so no BFC is applied.
                Vector3 vec = point.Vec;
                if (doRecenter) {
                    vec = new Vector3(vec.X + transX, vec.Y + transY, vec.Z);
                }
                Vector3 trv = rotMat.Multiply(vec);

                double xc, yc;
                if (doPersp) {
                    double zc = trv.Z * scale;
                    xc = (trv.X * scale * zadj) / (zadj + zc);
                    yc = (trv.Y * scale * zadj) / (zadj + zc);
                } else {
                    xc = trv.X * scale;
                    yc = trv.Y * scale;
                }

                //Debug.WriteLine("POINT " + xc + "," + yc);

                // Zero-length line segments don't do anything.  Try a '+'.
                const double dist = 1 / 64.0;
                double x0 = Math.Max(-1.0, xc - dist);
                double x1 = Math.Min(xc + dist, 1.0);
                segs.Add(new LineSeg(x0, yc, x1, yc));
                double y0 = Math.Max(-1.0, yc - dist);
                double y1 = Math.Min(yc + dist, 1.0);
                segs.Add(new LineSeg(xc, y0, xc, y1));
            }

            foreach (Edge edge in mEdges) {
                if (doBfc) {
                    // To be visible, vertices and edges must either not specify any
                    // faces, or must specify a visible face.  They can also be hidden
                    // by the level-of-detail exclusion mechanism.
                    if (!IsVertexVisible(edge.Vertex0) || edge.Vertex0.IsExcluded ||
                            !IsVertexVisible(edge.Vertex1) || edge.Vertex1.IsExcluded ||
                            !IsEdgeVisible(edge) || edge.IsExcluded) {
                        continue;
                    }
                }

                Vector3 vec0 = edge.Vertex0.Vec;
                Vector3 vec1 = edge.Vertex1.Vec;
                if (doRecenter) {
                    vec0 = new Vector3(vec0.X + transX, vec0.Y + transY, vec0.Z);
                    vec1 = new Vector3(vec1.X + transX, vec1.Y + transY, vec1.Z);
                }
                Vector3 trv0 = rotMat.Multiply(vec0);
                Vector3 trv1 = rotMat.Multiply(vec1);
                double x0, y0, x1, y1;

                if (doPersp) {
                    // Left-handed system, so +Z is away from viewer.
                    double z0 = trv0.Z * scale;
                    double z1 = trv1.Z * scale;
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
