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

namespace CommonUtil {
    /// <summary>
    /// Representation of a GIF image, with the various pieces broken out.
    /// </summary>
    /// <remarks>
    /// This has only been tested with the GIF images created by the Windows Media GifEncoder,
    /// which are GIF89a with no global color table.
    ///
    /// References:
    ///   https://www.w3.org/Graphics/GIF/spec-gif87.txt
    ///   https://www.w3.org/Graphics/GIF/spec-gif89a.txt
    /// </remarks>
    public class UnpackedGif {
        //
        // Header.
        //
        public enum FileVersion { Unknown = 0, Gif87a, Gif89a };
        private static readonly byte[] GIF87A = new byte[] {
            (byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)'7', (byte)'a'
        };
        private static readonly byte[] GIF89A = new byte[] {
            (byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)'9', (byte)'a'
        };
        public FileVersion FileVer { get; private set; }

        //
        // Logical screen descriptor.
        //
        public ushort LogicalScreenWidth { get; private set; }
        public ushort LogicalScreenHeight { get; private set; }
        public bool GlobalColorTableFlag { get; private set; }
        public byte ColorResolution { get; private set; }
        public bool SortFlag { get; private set; }
        public byte GlobalColorTableSize { get; private set; }
        public byte BackgroundColorIndex { get; private set; }
        public byte PixelAspectRatio { get; private set; }

        //
        // Global color table.
        //
        public byte[] GlobalColorTable { get; private set; }

        //
        // Extension block constants.
        //
        public const byte EXTENSION_INTRODUCER = 0x21;
        public const byte APP_EXTENSION_LABEL = 0xff;
        public const byte COMMENT_LABEL = 0xfe;
        public const byte GRAPHIC_CONTROL_LABEL = 0xf9;
        public const byte PLAIN_TEXT_LABEL = 0x01;

        //
        // Graphic control extension.
        //
        // These are optional.  At most one may precede a graphic rendering block.
        //
        public class GraphicControlExtension {
            public enum DisposalMethods : byte {
                None = 0,
                DoNotDispose = 1,
                RestoreBackground = 2,
                RestorePrevious = 3
            }

            public byte DisposalMethod { get; private set; }
            public bool UserInputFlag { get; private set; }
            public bool TransparencyFlag { get; private set; }
            public ushort DelayTime { get; private set; }
            public byte TransparentColorIndex { get; private set; }

            private GraphicControlExtension() { }

            public static GraphicControlExtension Create(byte[] data, ref int offset) {
                Debug.Assert(data[offset] == EXTENSION_INTRODUCER &&
                    data[offset + 1] == GRAPHIC_CONTROL_LABEL);

                GraphicControlExtension gce = new GraphicControlExtension();

                offset += 2;
                if (data[offset++] != 4) {
                    Debug.WriteLine("Bad block size in GCE data");
                    return null;
                }
                byte pak = data[offset++];
                gce.DisposalMethod = (byte)((pak >> 2) & 0x07);
                gce.UserInputFlag = (pak & 0x02) != 0;
                gce.TransparencyFlag = (pak & 0x01) != 0;
                gce.DelayTime = RawData.FetchLittleUshort(data, ref offset);
                gce.TransparentColorIndex = data[offset++];
                if (data[offset++] != 0) {
                    Debug.WriteLine("Missing termination in GCE data");
                    return null;
                }
                return gce;
            }
        }

        //
        // Image descriptor.
        //
        public const byte IMAGE_SEPARATOR = 0x2c;

        /// <summary>
        /// The graphic rendering block is an image descriptor, followed by an optional
        /// local color table, and then the image data itself.
        /// </summary>
        public class GraphicRenderingBlock {
            public ushort ImageLeftPosition { get; private set; }
            public ushort ImageTopPosition { get; private set; }
            public ushort ImageWidth { get; private set; }
            public ushort ImageHeight { get; private set; }
            public bool LocalColorTableFlag { get; private set; }
            public bool InterlaceFlag { get; private set; }
            public bool SortFlag { get; private set; }
            public byte LocalColorTableSize { get; private set; }

            //
            // Local color table.
            //
            public byte[] LocalColorTable { get; private set; }

            /// <summary>
            /// Offset of first byte of image data (which will be the LZW minimum code size byte).
            /// </summary>
            public int ImageStartOffset { get; private set; }

            public byte[] ImageData { get; private set; }

            /// <summary>
            /// Offset of last byte of image data (which will be the terminating $00 byte).
            /// </summary>
            public int ImageEndOffset { get; private set; }

            /// <summary>
            /// Optional extension with transparency and delay values.
            /// </summary>
            public GraphicControlExtension GraphicControlExt { get; set; }

            private GraphicRenderingBlock() { }

            public static GraphicRenderingBlock Create(byte[] data, ref int offset) {
                GraphicRenderingBlock grb = new GraphicRenderingBlock();

                //
                // Image descriptor.
                //
                Debug.Assert(data[offset] == IMAGE_SEPARATOR);
                offset++;
                grb.ImageLeftPosition = RawData.FetchLittleUshort(data, ref offset);
                grb.ImageTopPosition = RawData.FetchLittleUshort(data, ref offset);
                grb.ImageWidth = RawData.FetchLittleUshort(data, ref offset);
                grb.ImageHeight = RawData.FetchLittleUshort(data, ref offset);
                byte pak = data[offset++];
                grb.LocalColorTableFlag = (pak & 0x80) != 0;
                grb.InterlaceFlag = (pak & 0x40) != 0;
                grb.SortFlag = (pak & 0x20) != 0;
                grb.LocalColorTableSize = (byte)(pak & 0x07);

                //
                // Local color table (optional).
                //
                if (grb.LocalColorTableFlag) {
                    // Size is expressed as a power of 2. TableSize=7 is 256 entries, 3 bytes each.
                    int tableLen = 1 << (grb.LocalColorTableSize + 1);
                    grb.LocalColorTable = new byte[tableLen * 3];
                    for (int i = 0; i < tableLen * 3; i++) {
                        grb.LocalColorTable[i] = data[offset++];
                    }
                } else {
                    grb.LocalColorTable = new byte[0];
                }

                //
                // Table based image data.
                //
                grb.ImageData = data;
                grb.ImageStartOffset = offset++;
                while (data[offset] != 0) {
                    offset += data[offset] + 1;
                }
                grb.ImageEndOffset = offset++;

                return grb;
            }
        }
        public List<GraphicRenderingBlock> ImageBlocks = new List<GraphicRenderingBlock>();

        //
        // EOF marker.
        //
        public const byte GIF_TRAILER = 0x3b;


        /// <summary>
        /// Constructor.  Internal only; use factory method.
        /// </summary>
        private UnpackedGif() { }

        /// <summary>
        /// </summary>
        /// <param name="gifData">GIF data stream.  The array may be longer than the
        ///   data stream.</param>
        /// <returns>Newly-created object, or null on error.</returns>
        public static UnpackedGif Create(byte[] gifData) {
            UnpackedGif gif = new UnpackedGif();

            try {
                if (!gif.Unpack(gifData)) {
                    return null;
                }
            } catch (Exception ex) {
                Debug.WriteLine("Failure during GIF unpacking: " + ex);
                return null;
            }

            return gif;
        }

        private bool Unpack(byte[] gifData) {
            //
            // Header.  Signature ("GIF") + version ("87a" or "89a").
            //
            if (RawData.CompareArrays(gifData, 0, GIF87A, 0, GIF87A.Length)) {
                FileVer = FileVersion.Gif87a;
            } else if (RawData.CompareArrays(gifData, 0, GIF89A, 0, GIF87A.Length)) {
                FileVer = FileVersion.Gif89a;
            } else {
                Debug.WriteLine("GIF signature not found");
                return false;
            }
            //Debug.WriteLine("GIF: found signature " + FileVer);

            byte pak;

            //
            // Logical screen descriptor.
            //
            int offset = GIF87A.Length;
            LogicalScreenWidth = RawData.FetchLittleUshort(gifData, ref offset);
            LogicalScreenHeight = RawData.FetchLittleUshort(gifData, ref offset);
            pak = gifData[offset++];
            GlobalColorTableFlag = (pak & 0x80) != 0;
            ColorResolution = (byte)((pak >> 4) & 0x07);
            SortFlag = (pak & 0x08) != 0;
            GlobalColorTableSize = (byte)(pak & 0x07);
            BackgroundColorIndex = gifData[offset++];
            PixelAspectRatio = gifData[offset++];

            //
            // Global color table.
            //
            if (GlobalColorTableFlag) {
                // Size is expressed as a power of 2.  TableSize=7 is 256 entries, 3 bytes each.
                int tableLen = 1 << (GlobalColorTableSize + 1);
                GlobalColorTable = new byte[tableLen * 3];
                for (int i = 0; i < tableLen * 3; i++) {
                    GlobalColorTable[i] = gifData[offset++];
                }
            } else {
                GlobalColorTable = new byte[0];
            }

            //
            // Various blocks follow.  Continue until EOF is reached.
            //
            GraphicControlExtension lastGce = null;
            while (true) {
                if (offset >= gifData.Length) {
                    Debug.WriteLine("Error: GIF unpacker ran off end of buffer");
                    return false;
                }
                if (gifData[offset] == GIF_TRAILER) {
                    break;
                } else if (gifData[offset] == EXTENSION_INTRODUCER) {
                    if (gifData[offset + 1] == GRAPHIC_CONTROL_LABEL) {
                        lastGce = GraphicControlExtension.Create(gifData, ref offset);
                    } else {
                        Debug.WriteLine("Skipping unknown extension 0x" +
                            gifData[offset + 1].ToString("x2"));
                        offset += 2;
                        while (gifData[offset] != 0) {
                            offset += gifData[offset] + 1;
                        }
                        offset++;
                    }
                } else if (gifData[offset] == IMAGE_SEPARATOR) {
                    GraphicRenderingBlock grb = GraphicRenderingBlock.Create(gifData, ref offset);
                    if (grb != null) {
                        if (lastGce != null) {
                            grb.GraphicControlExt = lastGce;
                        }
                        ImageBlocks.Add(grb);
                    }

                    // this resets after the image
                    lastGce = null;
                } else {
                    Debug.WriteLine("Found unknown block start 0x" +
                        gifData[offset].ToString("x2"));
                    return false;
                }
            }

            return true;
        }

        public void DebugDump() {
            Debug.WriteLine("UnpackedGif: " + FileVer);
            Debug.WriteLine("  Logical size: " + LogicalScreenWidth + "x" + LogicalScreenHeight);
            Debug.WriteLine("  Global CT: " + GlobalColorTableFlag +
                " size=" + GlobalColorTableSize + " bkci=" + BackgroundColorIndex +
                " sort=" + SortFlag);
            Debug.WriteLine("  Aspect=" + PixelAspectRatio);
            Debug.WriteLine("  Images (" + ImageBlocks.Count + "):");
            foreach (GraphicRenderingBlock grb in ImageBlocks) {
                if (grb.GraphicControlExt != null) {
                    GraphicControlExtension gce = grb.GraphicControlExt;
                    Debug.WriteLine("    GCE: trans=" + gce.TransparencyFlag +
                        " color=" + gce.TransparentColorIndex + " delay=" + gce.DelayTime +
                        " disp=" + gce.DisposalMethod);
                } else {
                    Debug.WriteLine("    No GCE");
                }
                Debug.WriteLine("    left=" + grb.ImageLeftPosition +
                    " top=" + grb.ImageTopPosition + " width=" + grb.ImageWidth +
                    " height=" + grb.ImageHeight);
                Debug.WriteLine("    localCT=" + grb.LocalColorTableFlag + " size=" +
                    grb.LocalColorTableSize + " itrl=" + grb.InterlaceFlag);
                for (int i = 0; i < grb.LocalColorTable.Length; i += 3) {
                    Debug.WriteLine("      " + (i / 3) + ": $" +
                        grb.LocalColorTable[i].ToString("x2") + " $" +
                        grb.LocalColorTable[i + 1].ToString("x2") + " $" +
                        grb.LocalColorTable[i + 2].ToString("x2"));
                }
            }
        }
    }
}
