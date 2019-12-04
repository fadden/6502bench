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
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using CommonUtil;
using PluginCommon;

namespace SourceGen {
    /// <summary>
    /// Graphical visualization object.  Useful for displaying 2D bitmaps and 3D objects.
    ///
    /// This is generally immutable, except for the CachedImage field.
    /// </summary>
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
        public ReadOnlyDictionary<string, object> VisGenParams { get; private set; }

        /// <summary>
        /// Cached reference to 2D image, useful for thumbnails.  Not serialized.
        /// </summary>
        /// <remarks>
        /// Because the underlying data never changes, we only need to regenerate the
        /// image if the set of active plugins changes.
        /// </remarks>
        public BitmapSource CachedImage { get; set; }

        /// <summary>
        /// Image to show when things are broken.
        /// </summary>
        public static readonly BitmapImage BROKEN_IMAGE =
            new BitmapImage(new Uri("pack://application:,,,/Res/RedX.png"));


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tag">Unique identifier.</param>
        /// <param name="visGenIdent">Visualization generator identifier.</param>
        /// <param name="visGenParams">Parameters for visualization generator.</param>
        public Visualization(string tag, string visGenIdent,
                ReadOnlyDictionary<string, object> visGenParams) {
            Tag = tag;
            VisGenIdent = visGenIdent;
            VisGenParams = visGenParams;
            CachedImage = BROKEN_IMAGE;
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
        /// Converts an IVisualization2d to a BitmapSource for display.
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
