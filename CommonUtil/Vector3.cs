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

namespace CommonUtil {
    /// <summary>
    /// Simple 3-element column vector.
    /// </summary>
    public class Vector3 {
        public double X {
            get { return mX; }
            set { mX = value; }
        }
        public double Y {
            get { return mY; }
            set { mY = value; }
        }
        public double Z {
            get { return mZ; }
            set { mZ = value; }
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

        public void Normalize() {
            double len_r = 1.0 / Magnitude();
            mX *= len_r;
            mY *= len_r;
            mZ *= len_r;
        }
    }
}
