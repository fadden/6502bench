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
    ///
    /// At a basic level, bitmap and wireframe visualizations are the same: you take a
    /// visualization generation identifier and a bunch of parameters, and generate Stuff.
    /// The nature of the Stuff and what you do with it after are very different, however.
    ///
    /// For a bitmap, we can generate the data once, and then scale or transform it as
    /// necessary.  Bitmap animations are a collection of bitmap visualizations.
    ///
    /// For wireframes, we generate a WireframeObject using some of the parameters, and then
    /// transform it with other parameters.  The parameters are stored in a single dictionary,
    /// but viewer-only parameters are prefixed with '_', which is not allowed in plugins.
    ///
    /// This class represents the common ground between bitmaps and wireframes.  It holds the
    /// identifier and parameters, as well as the thumbnail data that we display in the list.
    /// </remarks>
    public class Visualization {
        public const double THUMBNAIL_DIM = 64;

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
        public static readonly BitmapImage BROKEN_IMAGE;
        static Visualization() {
            BROKEN_IMAGE = new BitmapImage(new Uri("pack://application:,,,/Res/RedX.png"));
            BROKEN_IMAGE.Freeze();
        }

        /// <summary>
        /// Image to overlay on animation visualizations.
        /// </summary>
        internal static readonly BitmapSource ANIM_OVERLAY_IMAGE = GenerateAnimOverlayImage();

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
            Debug.Assert(CachedImage.IsFrozen);
        }

        /// <summary>
        /// Updates the cached thumbnail image.
        /// </summary>
        /// <param name="visWire">Visualization object, or null to clear the thumbnail.</param>
        /// <param name="parms">Visualization parameters.</param>
        public virtual void SetThumbnail(IVisualizationWireframe visWire,
                ReadOnlyDictionary<string, object> parms) {
            if (visWire == null) {
                CachedImage = BROKEN_IMAGE;
            } else {
                Debug.Assert(parms != null);
                WireframeObject wireObj = WireframeObject.Create(visWire);
                CachedImage = GenerateWireframeImage(wireObj, THUMBNAIL_DIM, parms);
            }
            Debug.Assert(CachedImage.IsFrozen);
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
            image.Freeze();

            return image;
        }

        /// <summary>
        /// Generates a BitmapSource from IVisualizationWireframe data.  Useful for thumbnails
        /// and GIF exports.
        /// </summary>
        /// <param name="visWire">Visualization data.</param>
        /// <param name="dim">Output bitmap dimension (width and height).</param>
        /// <param name="parms">Parameter set, for rotations and render options.</param>
        /// <returns>Rendered bitmap.</returns>
        public static BitmapSource GenerateWireframeImage(WireframeObject wireObj,
                double dim, ReadOnlyDictionary<string, object> parms) {
            int eulerX = Util.GetFromObjDict(parms, VisWireframeAnimation.P_EULER_ROT_X, 0);
            int eulerY = Util.GetFromObjDict(parms, VisWireframeAnimation.P_EULER_ROT_Y, 0);
            int eulerZ = Util.GetFromObjDict(parms, VisWireframeAnimation.P_EULER_ROT_Z, 0);
            bool doPersp = Util.GetFromObjDict(parms, VisWireframe.P_IS_PERSPECTIVE, true);
            bool doBfc = Util.GetFromObjDict(parms, VisWireframe.P_IS_BFC_ENABLED, false);
            return GenerateWireframeImage(wireObj, dim, eulerX, eulerY, eulerZ, doPersp, doBfc);
        }

        /// <summary>
        /// Generates a BitmapSource from IVisualizationWireframe data.  Useful for thumbnails
        /// and GIF exports.
        /// </summary>
        public static BitmapSource GenerateWireframeImage(WireframeObject wireObj,
                double dim, int eulerX, int eulerY, int eulerZ, bool doPersp, bool doBfc) {
            if (wireObj == null) {
                // Can happen if the visualization generator is failing on stuff loaded from
                // the project file.
                return BROKEN_IMAGE;
            }

            // Generate the path geometry.
            GeometryGroup geo = GenerateWireframePath(wireObj, dim, eulerX, eulerY, eulerZ,
                doPersp, doBfc);

            // Render geometry to bitmap -- https://stackoverflow.com/a/869767/294248
            Rect bounds = geo.GetRenderBounds(null);

            //Debug.WriteLine("RenderWF dim=" + dim + " bounds=" + bounds + ": " + wireObj);

            // Create bitmap.
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                (int)dim,
                (int)dim,
                96,
                96,
                PixelFormats.Pbgra32);
            //RenderOptions.SetEdgeMode(bitmap, EdgeMode.Aliased);  <-- no apparent effect

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen()) {
                dc.DrawRectangle(Brushes.Black, null, new Rect(0, 0, bounds.Width, bounds.Height));
                Pen pen = new Pen(Brushes.White, 1.0);
                dc.DrawGeometry(Brushes.White, pen, geo);
            }
            bitmap.Render(dv);

#if false
            // Old way: render Path to bitmap -- https://stackoverflow.com/a/23582564/294248

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
#endif

            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// Generates WPF Path geometry from IVisualizationWireframe data.  Line widths get
        /// scaled if the output area is larger or smaller than the path bounds, so this scales
        /// coordinates so they fit within the box.
        /// </summary>
        /// <param name="visWire">Visualization data.</param>
        /// <param name="dim">Width/height to use for path area.</param>
        /// <param name="parms">Visualization parameters.</param>
        public static GeometryGroup GenerateWireframePath(WireframeObject wireObj,
                double dim, ReadOnlyDictionary<string, object> parms) {
            int eulerX = Util.GetFromObjDict(parms, VisWireframeAnimation.P_EULER_ROT_X, 0);
            int eulerY = Util.GetFromObjDict(parms, VisWireframeAnimation.P_EULER_ROT_Y, 0);
            int eulerZ = Util.GetFromObjDict(parms, VisWireframeAnimation.P_EULER_ROT_Z, 0);
            bool doPersp = Util.GetFromObjDict(parms, VisWireframe.P_IS_PERSPECTIVE, true);
            bool doBfc = Util.GetFromObjDict(parms, VisWireframe.P_IS_BFC_ENABLED, false);
            return GenerateWireframePath(wireObj, dim, eulerX, eulerY, eulerZ, doPersp, doBfc);
        }

        /// <summary>
        /// Generates WPF Path geometry from IVisualizationWireframe data.  Line widths get
        /// scaled if the output area is larger or smaller than the path bounds, so this scales
        /// coordinates so they fit within the box.
        /// </summary>
        public static GeometryGroup GenerateWireframePath(WireframeObject wireObj,
                double dim, int eulerX, int eulerY, int eulerZ, bool doPersp, bool doBfc) {
            // WPF path drawing is based on a system where a pixel is drawn at the center
            // of its coordinates, and integer coordinates start at the top left edge of
            // the drawing area.  If you draw a pixel at (0,0), 3/4ths of the pixel will be
            // outside the window (visible or not based on ClipToBounds).
            //
            // If you draw a line from (1,1 to 4,1), the line's length will appear to
            // be (4 - 1) = 3.  It touches four pixels -- the end point is not exclusive --
            // but the filled area is only three, because the thickness doesn't extend the
            // line's length, and the line stops at the coordinate at the center of the pixel.
            // You're not drawing N pixels, you're drawing from one coordinate point to another.
            // If you have a window of size 8x8, and you draw from 0,0 to 7,0, the line will
            // extend for half a line-thickness off the top, but will not go past the right/left
            // edges.  (This becomes very obvious when you're working with an up-scaled 8x8 path.)
            //
            // Similarly, drawing a horizontal line two units long results in a square, and
            // drawing a line that starts and ends at the same point doesn't appear to
            // produce anything.
            //
            // It's possible to clean up the edges by adding 0.5 to all coordinate values.
            // This turns out to be important for another reason: a line from (1,1) to (9,1)
            // shows up as a double-wide half-bright line, while a line from (1.5,1.5) to
            // (9.5,1.5) is drawn as a single-wide full-brightness line.  This is because of
            // the anti-aliasing.  Anti-aliasing can be disabled, but the lines look much
            // nicer with it enabled.
            //
            // The path has an axis-aligned bounding box that covers the pixel centers.  If we
            // want a path-drawn mesh to animate smoothly we want to ensure that the bounds
            // are constant across all renderings of a shape (which could get thinner or wider
            // as it rotates), so we plot an invisible point in our desired bottom-right corner.
            //
            // If we want an 8x8 bitmap, we draw a line from (8,8) to (8,8) to establish the
            // bounds, then draw lines with coordinates from 0.5 to 7.5.

            GeometryGroup geo = new GeometryGroup();

            // Draw invisible line segments to establish Path bounds.
            Point topLeft = new Point(0, 0);
            Point botRight = new Point(dim, dim);
            geo.Children.Add(new LineGeometry(topLeft, topLeft));
            geo.Children.Add(new LineGeometry(botRight, botRight));

            // Generate a list of clip-space line segments.  Coordinate values are in the
            // range [-1,1], with +X to the right and +Y upward.
            List<WireframeObject.LineSeg> segs = wireObj.Generate(eulerX, eulerY, eulerZ,
                doPersp, doBfc);

            // Convert clip-space coords to screen.  We need to translate to [0,2] with +Y
            // toward the bottom of the screen, scale up, round to the nearest whole pixel,
            // and add +0.5 to make thumbnail-size bitmaps look crisp.
            double scale = (dim - 0.5) / 2;
            double adj = 0.5;
            foreach (WireframeObject.LineSeg seg in segs) {
                Point start = new Point(Math.Round((seg.X0 + 1) * scale) + adj,
                    Math.Round((1 - seg.Y0) * scale) + adj);
                Point end = new Point(Math.Round((seg.X1 + 1) * scale) + adj,
                    Math.Round((1 - seg.Y1) * scale) + adj);
                geo.Children.Add(new LineGeometry(start, end));
            }

            return geo;
        }

        /// <summary>
        /// Returns a bitmap with a single transparent pixel.
        /// </summary>
        private static BitmapSource GenerateBlankImage() {
            RenderTargetBitmap bmp = new RenderTargetBitmap(1, 1, 96.0, 96.0,
                PixelFormats.Pbgra32);
            bmp.Freeze();
            return bmp;
        }

        /// <summary>
        /// Generate an image to overlay on thumbnails of animations.
        /// </summary>
        /// <returns></returns>
        private static BitmapSource GenerateAnimOverlayImage() {
            const int IMAGE_SIZE = 128;

            // Glowy "high tech" blue.
            SolidColorBrush outlineBrush = new SolidColorBrush(Color.FromArgb(255, 0, 216, 255));
            SolidColorBrush fillBrush = new SolidColorBrush(Color.FromArgb(128, 0, 182, 215));

            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen()) {
                // Thanks: https://stackoverflow.com/a/29249100/294248
                Point p1 = new Point(IMAGE_SIZE * 5 / 8, IMAGE_SIZE / 2);
                Point p2 = new Point(IMAGE_SIZE * 3 / 8, IMAGE_SIZE / 4);
                Point p3 = new Point(IMAGE_SIZE * 3 / 8, IMAGE_SIZE * 3 / 4);
                StreamGeometry sg = new StreamGeometry();
                using (StreamGeometryContext sgc = sg.Open()) {
                    sgc.BeginFigure(p1, true, true);
                    PointCollection points = new PointCollection() { p2, p3 };
                    sgc.PolyLineTo(points, true, true);
                }
                sg.Freeze();
                dc.DrawGeometry(fillBrush, new Pen(outlineBrush, 3), sg);
            }

            RenderTargetBitmap bmp = new RenderTargetBitmap(IMAGE_SIZE, IMAGE_SIZE, 96.0, 96.0,
                PixelFormats.Pbgra32);
            bmp.Render(visual);
            bmp.Freeze();
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
