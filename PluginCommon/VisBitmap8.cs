/*
 * Copyright 2019 faddenSoft
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
using System.Text;

namespace PluginCommon {
    /// <summary>
    /// Bitmap with 8-bit palette indices, for use with visualization generators.
    /// </summary>
    [Serializable]
    public class VisBitmap8 : IVisualization2d {
        public const int MAX_DIMENSION = 4096;

        // IVisualization2d
        public int Width { get; private set; }

        // IVisualization2d
        public int Height { get; private set; }

        private byte[] mData;
        private int[] mPalette;
        private int mNextColor;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="width">Bitmap width, in pixels.</param>
        /// <param name="height">Bitmap height, in pixels.</param>
        public VisBitmap8(int width, int height) {
            Debug.Assert(width > 0 && width <= MAX_DIMENSION);
            Debug.Assert(height > 0 && height <= MAX_DIMENSION);

            Width = width;
            Height = height;

            mData = new byte[width * height];
            mPalette = new int[256];
            mNextColor = 0;
        }

        public int GetPixel(int x, int y) {
            byte pix = mData[x + y * Width];
            return mPalette[pix];
        }

        public void SetPixelIndex(int x, int y, byte colorIndex) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) {
                throw new ArgumentException("Bad x/y: " + x + "," + y + " (width=" + Width +
                    " height=" + Height + ")");
            }
            if (colorIndex < 0 || colorIndex >= mNextColor) {
                throw new ArgumentException("Bad color: " + colorIndex + " (nextCol=" +
                    mNextColor + ")");
            }
            mData[x + y * Width] = colorIndex;
        }

        // IVisualization2d
        public byte[] GetPixels() {
            return mData;
        }

        // IVisualization2d
        public int[] GetPalette() {
            int[] pal = new int[mNextColor];
            for (int i = 0; i < mNextColor; i++) {
                pal[i] = mPalette[i];
            }
            return pal;
        }


        /// <summary>
        /// Adds a new color to the palette.  If the color already exists, the call has no
        /// effect.
        /// </summary>
        /// <param name="color">32-bit ARGB color value.</param>
        public void AddColor(int color) {
            if (mNextColor == 256) {
                Debug.WriteLine("Palette is full");
                return;
            }
            for  (int i = 0; i < mNextColor; i++) {
                if (mPalette[i] == color) {
                    Debug.WriteLine("Color " + color.ToString("x6") +
                        " already exists in palette (" + i + ")");
                    return;
                }
            }
            mPalette[mNextColor++] = color;
        }

        /// <summary>
        /// Adds a new color to the palette.  If the color already exists, the call has no
        /// effect.
        /// </summary>
        /// <param name="a">Alpha value.</param>
        /// <param name="r">Red value.</param>
        /// <param name="g">Green value.</param>
        /// <param name="b">Blue value.</param>
        public void AddColor(byte a, byte r, byte g, byte b) {
            AddColor(Util.MakeARGB(a, r, g, b));
        }
    }
}
