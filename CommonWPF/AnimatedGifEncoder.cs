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
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

using CommonUtil;

namespace CommonWPF {
    /// <summary>
    /// Creates an animated GIF from a collection of bitmap frames.
    /// </summary>
    public class AnimatedGifEncoder {
        // GIF signature + version.
        private static readonly byte[] GIF89A_SIGNATURE = new byte[] {
            (byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)'9', (byte)'a'
        };

        private static readonly byte[] NetscapeExtStart = new byte[] {
            UnpackedGif.EXTENSION_INTRODUCER,
            UnpackedGif.APP_EXTENSION_LABEL,
            0x0b,           // Block Size
            (byte)'N',      // Application Identifier (8 bytes)
            (byte)'E',
            (byte)'T',
            (byte)'S',
            (byte)'C',
            (byte)'A',
            (byte)'P',
            (byte)'E',
            (byte)'2',      // Appl. Authentication Code (3 bytes)
            (byte)'.',
            (byte)'0',
            0x03,           // size of block
            // followed by loop flag, 2-byte repetition count, and $00 to terminate
        };

        private static readonly byte[] GraphicControlStart = new byte[] {
            UnpackedGif.EXTENSION_INTRODUCER,
            UnpackedGif.GRAPHIC_CONTROL_LABEL,
            0x04,           // Block Size
            // followed by flags, 2-byte delay, transparency color index, and $00 to terminate
        };

        /// <summary>
        /// List of bitmap frames.
        /// </summary>
        private List<BitmapFrame> Frames { get; set; }

        private class MetaData {
            public int DelayMsec { get; private set; }

            public MetaData(int delayMsec) {
                DelayMsec = delayMsec;
            }
        }

        /// <summary>
        /// Per-frame metadata.
        /// </summary>
        private List<MetaData> FrameData { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        public AnimatedGifEncoder() {
            Frames = new List<BitmapFrame>();
            FrameData = new List<MetaData>();
        }

        public void AddFrame(BitmapFrame frame, int delayMsec) {
            Frames.Add(frame);
            FrameData.Add(new MetaData(delayMsec));
        }

        /// <summary>
        /// Converts the list of frames into an animated GIF, and writes it to the stream.
        /// </summary>
        /// <param name="stream">Output stream.</param>
        public void Save(Stream stream) {
            if (Frames.Count == 0) {
                // nothing to do
                Debug.Assert(false);
                return;
            }

            //
            // Step 1: convert all BitmapFrame objects to GIF.  This lets the .NET GIF encoder
            // deal with the data compression.
            //
            List<UnpackedGif> gifs = new List<UnpackedGif>(Frames.Count);
            foreach (BitmapFrame bf in Frames) {
                GifBitmapEncoder encoder = new GifBitmapEncoder();
                encoder.Frames.Add(bf);
                using (MemoryStream ms = new MemoryStream()) {
                    encoder.Save(ms);
                    // We're using GetBuffer() rather than ToArray() to avoid a copy.  One
                    // consequence of this choice is that the byte[] may be oversized.  Since
                    // GIFs are treated as streams with explicit termination this should not
                    // pose a problem.
                    gifs.Add(UnpackedGif.Create(ms.GetBuffer()));
                }
            }

            //
            // Step 2: determine the size of the largest image.  This will become the logical
            // size of the animated GIF.
            //
            // TODO: We have an opportunity to replace all of the local color tables with a
            // single global color table.  This is only possible if all of the local tables are
            // identical and the transparency values in the GCE also match up.  (Well, it's
            // otherwise *possible*, but we'd need to decode, update palettes and pixels, and
            // re-encode.)
            //
            int maxWidth = -1;
            int maxHeight = -1;
            foreach (UnpackedGif gif in gifs) {
                //gif.DebugDump();

                if (maxWidth < gif.LogicalScreenWidth) {
                    maxWidth = gif.LogicalScreenWidth;
                }
                if (maxHeight < gif.LogicalScreenHeight) {
                    maxHeight = gif.LogicalScreenHeight;
                }
            }

            if (maxWidth < 0 || maxHeight < 0) {
                Debug.WriteLine("Unable to determine correct width/height");
                return;
            }

            //
            // Step 3: output data.
            //
            stream.Write(GIF89A_SIGNATURE, 0, GIF89A_SIGNATURE.Length);
            WriteLittleUshort(stream, (ushort)maxWidth);
            WriteLittleUshort(stream, (ushort)maxHeight);
            stream.WriteByte(0x70);         // no GCT; max color resolution (does this matter?)
            stream.WriteByte(0);            // BCI; not relevant
            stream.WriteByte(0);            // no aspect ratio adjustment

            stream.Write(NetscapeExtStart, 0, NetscapeExtStart.Length);
            stream.WriteByte(1);            // yes, we want to loop
            WriteLittleUshort(stream, 0);   // loop forever
            stream.WriteByte(0);            // end of block

            Debug.Assert(gifs.Count == FrameData.Count);
            for (int i = 0; i < Frames.Count; i++) {
                UnpackedGif gif = gifs[i];
                MetaData md = FrameData[i];

                // Just use the first image.
                UnpackedGif.GraphicRenderingBlock grb = gif.ImageBlocks[0];

                byte colorTableSize;
                byte[] colorTable;
                if (grb.LocalColorTableFlag) {
                    colorTableSize = grb.LocalColorTableSize;
                    colorTable = grb.LocalColorTable;
                } else if (gif.GlobalColorTableFlag) {
                    colorTableSize = gif.GlobalColorTableSize;
                    colorTable = gif.GlobalColorTable;
                } else {
                    Debug.Assert(false);
                    colorTableSize = 0x07;
                    colorTable = new byte[256 * 3];     // a whole lotta black
                }
                Debug.Assert(colorTable.Length == (1 << (colorTableSize + 1)) * 3);

                // If it has a GCE, use that.  Otherwise supply default values.  Either way
                // we use the frame delay from the meta-data.
                UnpackedGif.GraphicControlExtension gce = grb.GraphicControlExt;
                byte disposalMethod =
                    (byte)UnpackedGif.GraphicControlExtension.DisposalMethods.RestoreBackground;
                bool userInputFlag = false;
                bool transparencyFlag = false;
                byte transparentColorIndex = 0;
                if (gce != null) {
                    //disposalMethod = gce.DisposalMethod;
                    userInputFlag = gce.UserInputFlag;
                    transparencyFlag = gce.TransparencyFlag;
                    transparentColorIndex = gce.TransparentColorIndex;
                }

                stream.Write(GraphicControlStart, 0, GraphicControlStart.Length);
                stream.WriteByte((byte)((disposalMethod << 2) |
                    (userInputFlag ? 0x02 : 0) | (transparencyFlag ? 0x01 : 0)));
                WriteLittleUshort(stream, (ushort)Math.Round(md.DelayMsec / 10.0));
                stream.WriteByte(transparentColorIndex);
                stream.WriteByte(0);            // end of GCE

                // Output image descriptor.  We can center the images in the animation or
                // just leave them in the top-left corner.
                stream.WriteByte(UnpackedGif.IMAGE_SEPARATOR);
                WriteLittleUshort(stream, 0);       // left
                WriteLittleUshort(stream, 0);       // top
                WriteLittleUshort(stream, gif.LogicalScreenWidth);
                WriteLittleUshort(stream, gif.LogicalScreenHeight);
                stream.WriteByte((byte)(0x80 | colorTableSize));    // local table, no sort/intrl

                // Local color table.
                stream.Write(colorTable, 0, colorTable.Length);

                // Image data.  Trailing $00 is included.
                stream.Write(grb.ImageData, grb.ImageStartOffset,
                    grb.ImageEndOffset - grb.ImageStartOffset + 1);
            }

            stream.WriteByte(UnpackedGif.GIF_TRAILER);
        }

        private static void WriteLittleUshort(Stream stream, ushort val) {
            stream.WriteByte((byte)val);
            stream.WriteByte((byte)(val >> 8));
        }
    }
}
