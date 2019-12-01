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
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommonUtil;
using PluginCommon;

namespace SourceGen {
    public class Visualization {
        /// <summary>
        /// Unique user-specified tag.  Contents are arbitrary, but may not be empty.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// Name of visualization generator (extension script function).
        /// </summary>
        public string VisGenIdent { get; private set; }

        /// <summary>
        /// Parameters passed to the visualization generator.
        /// </summary>
        public Dictionary<string, object> VisGenParams { get; private set; }

        private BitmapSource Thumbnail { get; set; }    // TODO - 64x64(?) bitmap


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tag">Unique identifier.</param>
        /// <param name="visGenIdent">Visualization generator identifier.</param>
        /// <param name="visGenParams">Parameters for visualization generator.</param>
        public Visualization(string tag, string visGenIdent,
                Dictionary<string, object> visGenParams) {
            Tag = tag;
            VisGenIdent = visGenIdent;
            VisGenParams = visGenParams;
        }

        /// <summary>
        /// Converts an IVisualization2d to a BitmapSource for display.
        /// </summary>
        /// <param name="vis2d"></param>
        /// <returns></returns>
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
        /// Finds a plugin that provides the named visualization generator.
        /// </summary>
        /// <param name="proj">Project with script manager.</param>
        /// <param name="visGenIdent">Visualization generator identifier.</param>
        /// <returns>A plugin that matches, or null if none found.</returns>
        public static IPlugin_Visualizer FindPluginByVisGenIdent(DisasmProject proj,
                string visGenIdent, out VisDescr visDescr) {
            List<IPlugin> plugins = proj.GetActivePlugins();
            foreach (IPlugin chkPlug in plugins) {
                if (!(chkPlug is IPlugin_Visualizer)) {
                    continue;
                }
                IPlugin_Visualizer vplug = (IPlugin_Visualizer)chkPlug;
                foreach (VisDescr descr in vplug.GetVisGenDescrs()) {
                    if (descr.Ident == visGenIdent) {
                        visDescr = descr;
                        return vplug;
                    }
                }
            }
            visDescr = null;
            return null;
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
            // All fields must be equal.
            if (a.Tag != b.Tag || a.VisGenIdent != b.VisGenIdent || a.Thumbnail != b.Thumbnail) {
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
            // TODO(maybe): hash code should include up VisGenParams items
            return Tag.GetHashCode() ^ VisGenIdent.GetHashCode() ^ VisGenParams.Count;
        }
    }
}
