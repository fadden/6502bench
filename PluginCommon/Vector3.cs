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

namespace PluginCommon {
    /// <summary>
    /// Simple 3-element column vector.  Immutable.
    /// </summary>
    public class Vector3 {
        public double X {
            get { return mX; }
            private set { mX = value; }
        }
        public double Y {
            get { return mY; }
            private set { mY = value; }
        }
        public double Z {
            get { return mZ; }
            private set { mZ = value; }
        }
        private double mX, mY, mZ;

        public Vector3(double x, double y, double z) {
            mX = x;
            mY = y;
            mZ = z;
        }

        public double Magnitude() {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public Vector3 Normalize() {
            double len_r = 1.0 / Magnitude();
            return new Vector3(mX * len_r, mY * len_r, mZ * len_r);
        }

        public Vector3 Multiply(double sc) {
            return new Vector3(mX * sc, mY * sc, mZ * sc);
        }

        public Vector3 Add(Vector3 vec) {
            return new Vector3(mX + vec.X, mY + vec.Y, mZ + vec.Z);
        }

        public static Vector3 Add(Vector3 v0, Vector3 v1) {
            return new Vector3(v0.X + v1.X, v0.Y + v1.Y, v0.Z + v1.Z);
        }

        public static Vector3 Subtract(Vector3 v0, Vector3 v1) {
            return new Vector3(v0.X - v1.X, v0.Y - v1.Y, v0.Z - v1.Z);
        }

        public static Vector3 Cross(Vector3 v0, Vector3 v1) {
            return new Vector3(
                v0.Y * v1.Z - v0.Z * v1.Y,
                v0.Z * v1.X - v0.X * v1.Z,
                v0.X * v1.Y - v0.Y * v1.X);
        }

        public static double Dot(Vector3 v0, Vector3 v1) {
            return v0.X * v1.X + v0.Y * v1.Y + v0.Z * v1.Z;
        }


        public override string ToString() {
            return string.Format("|{0,8:N3} {1,8:N3} {2,8:N3}|", X, Y, Z);
        }
    }
}
