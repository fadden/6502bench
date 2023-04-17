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
using System.Diagnostics;

using CommonUtil;

namespace PluginCommon {
    /// <summary>
    /// Bitmap with 8-bit palette indices, for use with visualization generators.
    /// </summary>
    /// <remarks>
    /// The bitmap is initially filled with color index 0.
    /// </remarks>
    [Serializable]
    public class VisBitmap8 : IVisualization2d {
        public const int MAX_DIMENSION = 4096;

        // IVisualization2d
        public int Width { get; private set; }

        // IVisualization2d
        public int Height { get; private set; }

        private byte[] mData;
        private int[] mPalette;
        private int mMaxColorIndex;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="width">Bitmap width, in pixels.</param>
        /// <param name="height">Bitmap height, in pixels.</param>
        public VisBitmap8(int width, int height) {
            if (width <= 0 || width > MAX_DIMENSION || height <= 0 || height > MAX_DIMENSION) {
                throw new ArgumentException("Bad bitmap width/height " + width + "," + height);
            }

            Width = width;
            Height = height;

            mData = new byte[width * height];
            mPalette = new int[256];    // entries initialize to 0, i.e. transparent black
            mMaxColorIndex = 0;
        }

        //[Obsolete("use GetPixelIndex()")]
        public int GetPixel(int x, int y) {
            byte pix = mData[x + y * Width];
            return mPalette[pix];
        }

        /// <summary>
        /// Gets the color index for a single pixel.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>Color index.</returns>
        public byte GetPixelIndex(int x, int y) {
            return mData[x + y * Width];
        }

        /// <summary>
        /// Sets the color for a single pixel.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="colorIndex">Color index.</param>
        public void SetPixelIndex(int x, int y, byte colorIndex) {
            if (x < 0 || x >= Width || y < 0 || y >= Height) {
                throw new ArgumentException("Bad x/y: " + x + "," + y + " (width=" + Width +
                    " height=" + Height + ")");
            }
            if (colorIndex < 0 || colorIndex >= mMaxColorIndex) {
                throw new ArgumentException("Bad color: " + colorIndex + " (nextCol=" +
                    mMaxColorIndex + ")");
            }
            mData[x + y * Width] = colorIndex;
        }

        /// <summary>
        /// Sets the color for all pixels.
        /// </summary>
        /// <param name="colorIndex">Color index.</param>
        public void SetAllPixelIndices(byte colorIndex) {
            if (colorIndex < 0 || colorIndex >= mMaxColorIndex) {
                throw new ArgumentException("Bad color: " + colorIndex + " (nextCol=" +
                    mMaxColorIndex + ")");
            }
            for (int i = 0; i < mData.Length; i++) {
                mData[i] = colorIndex;
            }
        }

        // IVisualization2d
        public byte[] GetPixels() {
            // TODO: remap any duplicate colors to reduce size of GIF
            return mData;
        }

        // IVisualization2d
        public int[] GetPalette() {
            // TODO: remove any duplicate colors to reduce size of GIF
            int[] pal = new int[mMaxColorIndex];
            for (int i = 0; i < mMaxColorIndex; i++) {
                pal[i] = mPalette[i];
            }
            return pal;
        }


        /// <summary>
        /// Adds a new color to the palette.  If the color already exists, the call has no
        /// effect.
        /// </summary>
        /// <param name="color">32-bit ARGB color value.</param>
        //[Obsolete("use SetColor()")]
        public void AddColor(int color) {
            if (mMaxColorIndex == 256) {
                Debug.WriteLine("Palette is full");
                return;
            }
            // I'm expecting palettes to only have a few colors, so O(n^2) is fine for now.
            for (int i = 0; i < mMaxColorIndex; i++) {
                if (mPalette[i] == color) {
                    Debug.WriteLine("Color " + color.ToString("x6") +
                        " already exists in palette (" + i + ")");
                    return;
                }
            }
            mPalette[mMaxColorIndex++] = color;
        }

        /// <summary>
        /// Adds a new color to the palette.  If the color already exists, the call has no
        /// effect.
        /// </summary>
        /// <param name="a">Alpha value.</param>
        /// <param name="r">Red value.</param>
        /// <param name="g">Green value.</param>
        /// <param name="b">Blue value.</param>
        //[Obsolete("use SetColor()")]
        public void AddColor(byte a, byte r, byte g, byte b) {
            AddColor(Util.MakeARGB(a, r, g, b));
        }

        /// <summary>
        /// Sets the Nth entry in the color palette.
        /// </summary>
        /// <remarks>
        /// The size of the color palette will expand to hold the largest index.  For best
        /// results, start with index 0 and count up, avoiding duplicates.
        /// </remarks>
        /// <param name="index">Palette index, 0-255.</param>
        /// <param name="a">Alpha value.</param>
        /// <param name="r">Red value.</param>
        /// <param name="g">Green value.</param>
        /// <param name="b">Blue value.</param>
        public void SetColor(int index, byte a, byte r, byte g, byte b) {
            if (index < 0 || index > 255) {
                throw new ArgumentException("Invalid index: " + index);
            }
            mPalette[index] = Util.MakeARGB(a, r, g, b);
            if (index >= mMaxColorIndex) {
                mMaxColorIndex = index + 1;
            }
        }

        /// <summary>
        /// Draws an 8x8 character cell on the bitmap.
        /// </summary>
        /// <remarks>
        /// Might want a way to specify that the background shouldn't be drawn at all.
        /// </remarks>
        /// <param name="vb">Bitmap to draw on.</param>
        /// <param name="ch">Character to draw.</param>
        /// <param name="xc">X coord of upper-left pixel.</param>
        /// <param name="yc">Y coord of upper-left pixel.</param>
        /// <param name="foreColor">Foreground color index.</param>
        /// <param name="backColor">Background color index.</param>
        public static void DrawChar(VisBitmap8 vb, char ch, int xc, int yc,
                byte foreColor, byte backColor) {
            int origXc = xc;
            int[] charBits = Font8x8.GetBitData(ch);
            for (int row = 0; row < 8; row++) {
                int rowBits = charBits[row];
                for (int col = 7; col >= 0; col--) {
                    if ((rowBits & (1 << col)) != 0) {
                        vb.SetPixelIndex(xc, yc, foreColor);
                    } else {
                        vb.SetPixelIndex(xc, yc, backColor);
                    }
                    xc++;
                }

                xc = origXc;
                yc++;
            }
        }

        // Character cell dimensions.
        public static int CharWidth { get { return 8; } }
        public static int CharHeight { get { return 8; } }
    }
}
