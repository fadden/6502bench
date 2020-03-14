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
using System.Text;

namespace PluginCommon {
    /// <summary>
    /// Simple 4x4 matrix.
    /// </summary>
    public class Matrix33 {
        private const int DIM = 3;

        public double[,] Val {
            get { return mVal; }
            private set { mVal = value; }
        }
        private double[,] mVal;

        public Matrix33() {
            Val = new double[DIM, DIM];
        }

        public void Clear() {
            for (int col = 0; col < DIM; col++) {
                for (int row = 0; row < DIM; row++) {
                    Val[col, row] = 0.0;
                }
            }
        }

        public void SetToIdentity() {
            Clear();
            Val[0, 0] = Val[1, 1] = Val[2, 2] = 1.0;
        }

        /// <summary>
        /// Rotation mode.  Determines the order in which axes are rotated, and whether the
        /// rotation is for a right-handed or left-handed system.
        /// </summary>
        public enum RotMode { XYZ_RRR, ZYX_RRR, ZYX_LLL, ZXY_RRR };

        /// <summary>
        /// Sets the matrix to perform rotation about Euler angles X/Y/Z, with a
        /// configurable order.
        /// </summary>
        /// <param name="xdeg">Rotation about the X axis, in degrees.</param>
        /// <param name="ydeg">Rotation about the Y axis, in degrees.</param>
        /// <param name="zdeg">Rotation about the Z axis, in degrees.</param>
        public void SetRotationEuler(int xdeg, int ydeg, int zdeg, RotMode mode) {
            const double degToRad = Math.PI / 180.0;
            double xrad = xdeg * degToRad;
            double yrad = ydeg * degToRad;
            double zrad = zdeg * degToRad;

            double cx = Math.Cos(xrad);
            double sx = Math.Sin(xrad);
            double cy = Math.Cos(yrad);
            double sy = Math.Sin(yrad);
            double cz = Math.Cos(zrad);
            double sz = Math.Sin(zrad);
            double sycx = sy * cx;
            double sysx = sy * sx;

            switch (mode) {
                case RotMode.ZYX_RRR:
                    // R = Rz * Ry * Rx, right-handed
                    Val[0, 0] = cz * cy;
                    Val[0, 1] = sz * cy;
                    Val[0, 2] = -sy;

                    Val[1, 0] = cz * sysx - sz * cx;
                    Val[1, 1] = sz * sysx + cz * cx;
                    Val[1, 2] = cy * sx;

                    Val[2, 0] = cz * sycx + sz * sx;
                    Val[2, 1] = sz * sycx - cz * sx;
                    Val[2, 2] = cy * cx;
                    break;
                case RotMode.ZYX_LLL:
                    // R = Rz * Ry * Rx, left-handed
                    Val[0, 0] = cz * cy;
                    Val[0, 1] = -sz * cy;
                    Val[0, 2] = sy;

                    Val[1, 0] = cz * sysx + sz * cx;
                    Val[1, 1] = -sz * sysx + cz * cx;
                    Val[1, 2] = -cy * sx;

                    Val[2, 0] = -cz * sycx + sz * sx;
                    Val[2, 1] = sz * sycx + cz * sx;
                    Val[2, 2] = cy * cx;
                    break;
                case RotMode.XYZ_RRR:
                    // R = Rx * Ry * Rz
                    Val[0, 0] = cz * cy;
                    Val[0, 1] = -sz * cy;
                    Val[0, 2] = sy;

                    Val[1, 0] = cz * sysx + sz * cx;
                    Val[1, 1] = -sz * sysx + cz * cx;
                    Val[1, 2] = -cy * sx;

                    Val[2, 0] = -cz * sycx + sz * sx;
                    Val[2, 1] = sz * sycx + cz * sx;
                    Val[2, 2] = cy * cx;
                    break;
                case RotMode.ZXY_RRR:
                    // R = Rz * Rx * Ry
                    double cysx = cy * sx;
                    Val[0, 0] = cz * cy + sz * sysx;
                    Val[0, 1] = -sz * cy + cz * sysx;
                    Val[0, 2] = sy * cx;

                    Val[1, 0] = sz * cx;
                    Val[1, 1] = cz * cx;
                    Val[1, 2] = -sx;

                    Val[2, 0] = -cz * sy + sz * cysx;
                    Val[2, 1] = sz * sy + cz * cysx;
                    Val[2, 2] = cy * cx;
                    break;
            }
        }

        /// <summary>
        /// Multiplies a 3-element vector.
        /// </summary>
        /// <param name="vec">Column vector to multiply.</param>
        /// <returns>Result vector.</returns>
        public Vector3 Multiply(Vector3 vec) {
            double rx = vec.X * Val[0, 0] + vec.Y * Val[1, 0] + vec.Z * Val[2, 0];
            double ry = vec.X * Val[0, 1] + vec.Y * Val[1, 1] + vec.Z * Val[2, 1];
            double rz = vec.X * Val[0, 2] + vec.Y * Val[1, 2] + vec.Z * Val[2, 2];
            return new Vector3(rx, ry, rz);
        }


        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < DIM; row++) {
                sb.AppendLine();
                sb.AppendFormat("|{0,8:N3} {1,8:N3} {2,8:N3}|",
                    Val[0, row], Val[1, row], Val[2, row]);
            }
            return sb.ToString();
        }
    }
}
