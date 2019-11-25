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

using PluginCommon;

namespace SourceGen {
    public class Visualization {
        /// <summary>
        /// Unique tag.  Contents are arbitrary, but may not be empty.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// Name of visualization generator (extension script function).
        /// </summary>
        public string VisGenName { get; private set; }

        /// <summary>
        /// Parameters passed to the visualization generator.
        /// </summary>
        public Dictionary<string, object> VisGenParams { get; private set; }

        public double Thumbnail { get; }    // TODO - 64x64(?) bitmap


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="visGenName"></param>
        /// <param name="visGenParams"></param>
        public Visualization(string tag, string visGenName,
                Dictionary<string, object> visGenParams) {
            Tag = tag;
            VisGenName = visGenName;
            VisGenParams = visGenParams;
        }


        /// <summary>
        /// Finds a plugin that provides the named visualization generator.
        /// </summary>
        /// <param name="proj">Project with script manager.</param>
        /// <param name="visGenName">Visualization generator name.</param>
        /// <returns>A plugin that matches, or null if none found.</returns>
        public static IPlugin_Visualizer2d FindPluginByVisGenName(DisasmProject proj,
                string visGenName) {
            Sandbox.ScriptManager.CheckMatch check = (chkPlug) => {
                if (!(chkPlug is IPlugin_Visualizer2d)) {
                    return false;
                }
                IPlugin_Visualizer2d vplug = (IPlugin_Visualizer2d)chkPlug;
                string[] names = vplug.GetVisGenNames();
                foreach (string name in names) {
                    if (name == visGenName) {
                        return true;
                    }
                }
                return false;
            };
            return (IPlugin_Visualizer2d)proj.GetMatchingScript(check);
        }


        public override string ToString() {
            return "[Vis: " + Tag + " (" + VisGenName + ")]";
        }

        public static bool operator ==(Visualization a, Visualization b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            // All fields must be equal.
            if (a.Tag != b.Tag || a.VisGenName != b.VisGenName || a.Thumbnail != b.Thumbnail) {
                return false;
            }
            return a.VisGenParams != b.VisGenParams;    // TODO(xyzzy): should be item-by-item
        }
        public static bool operator !=(Visualization a, Visualization b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is Visualization && this == (Visualization)obj;
        }
        public override int GetHashCode() {
            return Tag.GetHashCode() ^ VisGenName.GetHashCode() ^ VisGenParams.Count;
        }
    }
}
