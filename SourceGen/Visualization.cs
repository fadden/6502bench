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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommonUtil;
using PluginCommon;

namespace SourceGen {
    /// <summary>
    /// Graphical visualization object.  Useful for displaying 2D bitmaps and 3D objects.
    ///
    /// This is generally immutable, except for the CachedImage field.
    /// </summary>
    /// <remarks>
    /// Immutability is useful here because the undo/redo mechanism operates at VisualizationSet
    /// granularity.  We want to know that the undo/redo operations are operating on objects
    /// that weren't changed while sitting in the undo buffer.
    /// </remarks>
    public class Visualization {
        /// <summary>
        /// Unique user-specified tag.  This may be any valid string that is at least two
        /// characters long after the leading and trailing whitespace have been trimmed.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// Name of visualization generator (extension script function).
        /// </summary>
        public string VisGenIdent { get; private set; }

        /// <summary>
        /// Parameters to be passed to the visualization generator.
        /// </summary>
        /// <remarks>
        /// We use a read-only dictionary to reinforce the idea that the plugin shouldn't be
        /// modifying the parameter dictionary.
        /// </remarks>
        public ReadOnlyDictionary<string, object> VisGenParams { get; private set; }

        /// <summary>
        /// Cached reference to 2D image, useful for thumbnails that we display in the
        /// code listing.  Not serialized.  This always has an image reference; in times
        /// of trouble it will point at BROKEN_IMAGE.
        /// </summary>
        /// <remarks>
        /// Because the underlying data never changes, we only need to regenerate the
        /// image if the set of active plugins changes.
        ///
        /// For 2D bitmaps this should be close to a 1:1 representation of the original,
        /// subject to the limitations of the visualization generator.  For other types of
        /// data (vector line art, 3D meshes) this is a "snapshot" to help the user identify
        /// the data.
        /// </remarks>
        public BitmapSource CachedImage { get; set; }

        /// <summary>
        /// Image overlaid on CachedImage.  Used to identify thumbnails as animations.
        /// </summary>
        public BitmapSource OverlayImage { get; set; }

        /// <summary>
        /// True if CachedImage has something other than the default value.
        /// </summary>
        public bool HasImage {
            get {
                return CachedImage != BROKEN_IMAGE && CachedImage != ANIM_OVERLAY_IMAGE;
            }
        }

        /// <summary>
        /// Image to show when things are broken.
        /// </summary>
        public static readonly BitmapImage BROKEN_IMAGE =
            new BitmapImage(new Uri("pack://application:,,,/Res/RedX.png"));

        /// <summary>
        /// Image to overlay on animation visualizations.
        /// </summary>
        internal static readonly BitmapSource ANIM_OVERLAY_IMAGE =
            VisualizationAnimation.GenerateAnimOverlayImage();

        internal static readonly BitmapSource BLANK_IMAGE = GenerateBlankImage();
        //internal static readonly BitmapSource BLACK_IMAGE = GenerateBlackImage();

        /// <summary>
        /// Serial number, for reference from other Visualization objects.  Not serialized.
        /// </summary>
        /// <remarks>
        /// This value is only valid in the current session.  It exists because animations
        /// need to refer to other Visualization objects, and doing so by Tag gets sticky
        /// if a Tag gets renamed.  We need a way to uniquely identify a reference to a
        /// Visualization that persists across Tag renames and other edits.  When the objects
        /// are serialized to the project file we don't include the serial, and just reference
        /// by Tag.
        /// </remarks>
        public int SerialNumber { get; private set; }

        /// <summary>
        /// Serial number source.
        /// </summary>
        private static int sNextSerial = 1000;


        /// <summary>
        /// Constructor for a new Visualization.
        /// </summary>
        /// <param name="tag">Unique identifier.</param>
        /// <param name="visGenIdent">Visualization generator identifier.</param>
        /// <param name="visGenParams">Parameters for visualization generator.</param>
        public Visualization(string tag, string visGenIdent,
                ReadOnlyDictionary<string, object> visGenParams)
                :this(tag, visGenIdent, visGenParams, null) { }

        /// <summary>
        /// Constructor for a replacement Visualization.
        /// </summary>
        /// <param name="tag">Unique identifier.</param>
        /// <param name="visGenIdent">Visualization generator identifier.</param>
        /// <param name="visGenParams">Parameters for visualization generator.</param>
        /// <param name="oldObj">Visualization being replaced, or null if this is new.</param>
        public Visualization(string tag, string visGenIdent,
                ReadOnlyDictionary<string, object> visGenParams, Visualization oldObj) {
            Debug.Assert(!string.IsNullOrEmpty(tag));
            Debug.Assert(!string.IsNullOrEmpty(visGenIdent));
            Debug.Assert(visGenParams != null);

            Tag = tag;
            VisGenIdent = visGenIdent;
            VisGenParams = visGenParams;
            CachedImage = BROKEN_IMAGE;
            OverlayImage = BLANK_IMAGE;

            if (oldObj == null) {
                // not worried about multiple threads
                SerialNumber = sNextSerial++;
            } else {
                Debug.Assert(oldObj.SerialNumber >= 0 && oldObj.SerialNumber < sNextSerial);
                SerialNumber = oldObj.SerialNumber;
            }
            //Debug.WriteLine("NEW VIS: Serial=" + SerialNumber);
        }

        /// <summary>
        /// Updates the cached thumbnail image.
        /// </summary>
        /// <param name="vis2d">Visualization, or null to clear the thumbnail.</param>
        public void SetThumbnail(IVisualization2d vis2d) {
            if (vis2d == null) {
                CachedImage = BROKEN_IMAGE;
            } else {
                CachedImage = ConvertToBitmapSource(vis2d);
            }
        }

        public void SetThumbnail(IVisualizationWireframe visWire,
                ReadOnlyDictionary<string, object> parms) {
            if (visWire == null) {
                CachedImage = BROKEN_IMAGE;
            } else {
                CachedImage = GenerateWireframeImage(visWire, parms, 64);
            }
        }

        /// <summary>
        /// Trims a tag, removing leading/trailing whitespace, and checks it for validity.
        /// </summary>
        /// <param name="tag">Tag to trim and validate.</param>
        /// <param name="isValid">Set to true if the tag is valid.</param>
        /// <returns>Trimmed tag string.  Returns an empty string if tag is null.</returns>
        public static string TrimAndValidateTag(string tag, out bool isValid) {
            if (tag == null) {
                isValid = false;
                return string.Empty;
            }

            string trimTag = tag.Trim();
            if (trimTag.Length < 2) {
                isValid = false;
            } else {
                isValid = true;
            }
            return trimTag;
        }

        /// <summary>
        /// Converts an IVisualization2d to a BitmapSource for display.  The bitmap will be
        /// the same size as the original content.
        /// </summary>
        public static BitmapSource ConvertToBitmapSource(IVisualization2d vis2d) {
            // Create indexed color palette.
            int[] intPal = vis2d.GetPalette();
            List<Color> colors = new List<Color>(intPal.Length);
            foreach (int argb in intPal) {
                Color col = Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16),
                    (byte)(argb >> 8), (byte)argb);
                colors.Add(col);
            }
            BitmapPalette palette = new BitmapPalette(colors);

            // indexed-color; see https://stackoverflow.com/a/15272528/294248 for direct color
            BitmapSource image = BitmapSource.Create(
                vis2d.Width,
                vis2d.Height,
                96.0,
                96.0,
                PixelFormats.Indexed8,
                palette,
                vis2d.GetPixels(),
                vis2d.Width);

            return image;
        }

        /// <summary>
        /// Generates a BitmapSource from IVisualizationWireframe data.  Useful for thumbnails
        /// and GIF exports.
        /// </summary>
        /// <param name="visWire">Visualization data.</param>
        /// <param name="parms">Parameter set, for rotations and render options.</param>
        /// <param name="dim">Output bitmap dimension (width and height).</param>
        /// <returns>Rendered bitmap.</returns>
        public static BitmapSource GenerateWireframeImage(IVisualizationWireframe visWire,
                ReadOnlyDictionary<string, object> parms, double dim) {
            GeometryGroup geo = GenerateWireframePath(visWire, parms, dim);

            // Render Path to bitmap -- https://stackoverflow.com/a/23582564/294248
            Rect bounds = geo.GetRenderBounds(null);

            Debug.WriteLine("RenderWF dim=" + dim + " bounds=" + bounds + ": " + visWire);

            // Create bitmap.
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                (int)bounds.Width,
                (int)bounds.Height,
                96,
                96,
                PixelFormats.Pbgra32);
            //RenderOptions.SetEdgeMode(bitmap, EdgeMode.Aliased);  <-- doesn't work?

            // Clear the bitmap to black.  (Is there an easier way?)
            GeometryGroup bkgnd = new GeometryGroup();
            bkgnd.Children.Add(new RectangleGeometry(new Rect(0, 0, bounds.Width, bounds.Height)));
            Path path = new Path();
            path.Data = bkgnd;
            path.Stroke = path.Fill = Brushes.Black;
            path.Measure(bounds.Size);
            path.Arrange(bounds);
            bitmap.Render(path);

            path = new Path();
            path.Data = geo;
            path.Stroke = Brushes.White;
            path.Measure(bounds.Size);
            path.Arrange(bounds);
            bitmap.Render(path);

            return bitmap;
        }

        /// <summary>
        /// Generates a WPF path from IVisualizationWireframe data.  Line widths get scaled
        /// if the output area is larger or smaller than the path, so this scales coordinates
        /// so they fit within the box.
        /// </summary>
        /// <param name="visWire">Visualization data.</param>
        /// <param name="parms">Visualization parameters.</param>
        /// <param name="dim">Width/height to use for path area.</param>
        public static GeometryGroup GenerateWireframePath(IVisualizationWireframe visWire,
                ReadOnlyDictionary<string, object> parms, double dim) {
            // WPF path drawing is based on a system where a pixel is drawn at the center
            // of the coordinate, and integer coordinates start at the top left edge.  If
            // you draw a pixel at (0,0), most of the pixel will be outside the window
            // (visible or not based on ClipToBounds).
            //
            // If you draw a line from (1,1 to 4,1), the line's length will appear to
            // be (4 - 1) = 3.  It touches four pixels -- the end point is not exclusive --
            // but because the thickness doesn't extend past the endpoints, the filled
            // area is only three.  If you have a window of size 10x10, and you draw from
            // 0,0 to 9,9, the line will extend for half a line-thickness off the top,
            // but will not go past the right/left edges.
            //
            // Similarly, drawing a horizontal line two units long results in a square, and
            // drawing a line that starts and ends at the same point doesn't appear to
            // produce anything.
            //
            // It's possible to clean up the edges by adding 0.5 to all coordinate values.
            // This turns out to be important for another reason: a line from (1,1) to (9,1)
            // shows up as a double-wide half-bright line, while a line from (1.5,1.5) to
            // (9.5,1.5) is drawn as a single-wide full-brightness line.  This is because of
            // the anti-aliasing.
            //
            // The path has a bounding box that starts at (0,0) in the top left, and extends
            // out as far as needed.  If we want a path-drawn shape to animate smoothly we
            // want to ensure that the bounds are constant across all renderings of a shape
            // (which could get thinner or wider as it rotates), so we draw an invisible
            // pixel in our desired bottom-right corner.
            //
            // If we want an 8x8 bitmap, we draw a line from (8,8) to (8,8) to establish the
            // bounds, then draw lines with coordinates from 0.5 to 7.5.

            GeometryGroup geo = new GeometryGroup();
            // This establishes the geometry bounds.  It's a zero-length line segment, so
            // nothing is actually drawn.
            Debug.WriteLine("using max=" + dim);
            // TODO: currently ignoring dim
            Point corner = new Point(8, 8);
            geo.Children.Add(new LineGeometry(corner, corner));
            corner = new Point(0, 0);
            geo.Children.Add(new LineGeometry(corner, corner));

            // TODO(xyzzy): render
            //geo.Children.Add(new LineGeometry(new Point(0.0, 0.0), new Point(1.0, 0.0)));
            //geo.Children.Add(new LineGeometry(new Point(0.5, 0.5), new Point(1.5, 0.5)));
            //geo.Children.Add(new LineGeometry(new Point(0.75, 0.75), new Point(1.75, 0.75)));
            geo.Children.Add(new LineGeometry(new Point(0.0, 0.0), new Point(5.0, 7.0)));

            geo.Children.Add(new LineGeometry(new Point(0.5, 2), new Point(0.5, 3)));
            geo.Children.Add(new LineGeometry(new Point(1.5, 3), new Point(1.5, 4)));
            geo.Children.Add(new LineGeometry(new Point(2.5, 2), new Point(2.5, 3)));
            geo.Children.Add(new LineGeometry(new Point(3.5, 3), new Point(3.5, 4)));
            geo.Children.Add(new LineGeometry(new Point(4.5, 2), new Point(4.5, 3)));
            geo.Children.Add(new LineGeometry(new Point(5.5, 3), new Point(5.5, 4)));
            geo.Children.Add(new LineGeometry(new Point(6.5, 2), new Point(6.5, 3)));
            geo.Children.Add(new LineGeometry(new Point(7.5, 3), new Point(7.5, 4)));

            //geo.Children.Add(new LineGeometry(new Point(4, 5), new Point(3, 5)));
            //geo.Children.Add(new LineGeometry(new Point(2, 5), new Point(1, 5)));

            //geo.Children.Add(new LineGeometry(new Point(4, 7), new Point(1, 7)));
            //geo.Children.Add(new LineGeometry(new Point(5, 7), new Point(9, 7)));
            //geo.Children.Add(new LineGeometry(new Point(0, 8.5), new Point(9, 8.5)));
            return geo;
        }

        /// <summary>
        /// Returns a bitmap with a single transparent pixel.
        /// </summary>
        private static BitmapSource GenerateBlankImage() {
            RenderTargetBitmap bmp = new RenderTargetBitmap(1, 1, 96.0, 96.0,
                PixelFormats.Pbgra32);
            return bmp;
        }

        /// <summary>
        /// Returns a bitmap with a single black pixel.
        /// </summary>
        //private static BitmapSource GenerateBlackImage() {
        //    BitmapPalette palette = new BitmapPalette(new List<Color> { Colors.Black });
        //    BitmapSource image = BitmapSource.Create(
        //        1,
        //        1,
        //        96.0,
        //        96.0,
        //        PixelFormats.Indexed8,
        //        palette,
        //        new byte[] { 0 },
        //        1);

        //    return image;
        //}


        public override string ToString() {
            return "[Vis: " + Tag + " (" + VisGenIdent + ") count=" + VisGenParams.Count + "]";
        }

        public static bool operator ==(Visualization a, Visualization b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            // All fields must be equal (but we ignore CachedImage).
            if (a.Tag != b.Tag || a.VisGenIdent != b.VisGenIdent) {
                return false;
            }
            // Compare the vis gen parameter lists.
            if (a.VisGenParams == b.VisGenParams) {
                return true;
            }
            if (a.VisGenParams.Count != b.VisGenParams.Count) {
                return false;
            }
            return Container.CompareDicts(a.VisGenParams, b.VisGenParams);
        }
        public static bool operator !=(Visualization a, Visualization b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is Visualization && this == (Visualization)obj;
        }
        public override int GetHashCode() {
            // TODO(maybe): hash code should factor in VisGenParams items
            return Tag.GetHashCode() ^ VisGenIdent.GetHashCode() ^ VisGenParams.Count;
        }
    }
}
