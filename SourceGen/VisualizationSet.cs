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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using PluginCommon;

namespace SourceGen {
    /// <summary>
    /// Ordered list of visualization objects.
    /// </summary>
    /// <remarks>
    /// There's not much separating this from a plain List<>, except perhaps the operator== stuff.
    /// </remarks>
    public class VisualizationSet : IEnumerable<Visualization> {
        /// <summary>
        /// Object list.
        /// </summary>
        private List<Visualization> mList;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="initialCap">Initial capacity.</param>
        public VisualizationSet(int initialCap = 1) {
            mList = new List<Visualization>(initialCap);
        }

        // IEnumerable
        public IEnumerator<Visualization> GetEnumerator() {
            return mList.GetEnumerator();
        }

        // IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return mList.GetEnumerator();
        }

        /// <summary>
        /// The number of entries in the table.
        /// </summary>
        public int Count {
            get { return mList.Count; }
        }

        /// <summary>
        /// Accesses the Nth element.
        /// </summary>
        /// <param name="key">Element number.</param>
        public Visualization this[int key] {
            get {
                return mList[key];
            }
        }

        public void Add(Visualization vis) {
            mList.Add(vis);
        }

        public void Remove(Visualization vis) {
            mList.Remove(vis);
        }

        public Visualization[] ToArray() {
            Visualization[] arr = new Visualization[mList.Count];
            for (int i = 0; i < mList.Count; i++) {
                arr[i] = mList[i];
            }
            return arr;
        }

        #region Image generation

        private class ScriptSupport : MarshalByRefObject, PluginCommon.IApplication {
            public ScriptSupport() { }

            public void DebugLog(string msg) {
                Debug.WriteLine("Vis plugin: " + msg);
            }

            public bool SetOperandFormat(int offset, DataSubType subType, string label) {
                throw new InvalidOperationException();
            }
            public bool SetInlineDataFormat(int offset, int length, DataType type,
                    DataSubType subType, string label) {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Informs all list elements that a refresh is needed.  Call this when the set of active
        /// plugins changes.  The actual refresh will happen later.
        /// </summary>
        public void RefreshNeeded() {
            foreach (Visualization vis in mList) {
                vis.SetThumbnail(null);
            }
        }

        /// <summary>
        /// Attempts to refresh broken thumbnails across all visualization sets in the project.
        /// </summary>
        /// <param name="project">Project reference.</param>
        public static void RefreshAllThumbnails(DisasmProject project) {
            ScriptSupport iapp = null;
            List<IPlugin> plugins = null;

            foreach (KeyValuePair<int, VisualizationSet> kvp in project.VisualizationSets) {
                VisualizationSet visSet = kvp.Value;
                foreach (Visualization vis in visSet) {
                    if (vis.CachedImage != Visualization.BROKEN_IMAGE) {
                        continue;
                    }
                    Debug.WriteLine("Vis needs refresh: " + vis.Tag);

                    if (plugins == null) {
                        plugins = project.GetActivePlugins();
                    }
                    IPlugin_Visualizer vplug = FindPluginByVisGenIdent(plugins,
                        vis.VisGenIdent, out VisDescr visDescr);
                    if (vplug == null) {
                        Debug.WriteLine("Unable to referesh " + vis.Tag + ": plugin not found");
                        continue;
                    }

                    if (iapp == null) {
                        // Prep the plugins on first need.
                        iapp = new ScriptSupport();
                        project.PrepareScripts(iapp);
                    }

                    IVisualization2d vis2d;
                    try {
                        vis2d = vplug.Generate2d(visDescr, vis.VisGenParams);
                        if (vis2d == null) {
                            Debug.WriteLine("Vis generator returned null");
                        }
                    } catch (Exception ex) {
                        Debug.WriteLine("Vis generation failed: " + ex);
                        vis2d = null;
                    }
                    if (vis2d != null) {
                        Debug.WriteLine(" Rendered thumbnail: " + vis.Tag);
                        vis.SetThumbnail(vis2d);
                    }
                }
            }

            if (iapp != null) {
                project.UnprepareScripts();
            }
        }

        /// <summary>
        /// Finds a plugin that provides the named visualization generator.
        /// </summary>
        /// <param name="plugins">List of plugins, from project ScriptManager.</param>
        /// <param name="visGenIdent">Visualization generator identifier.</param>
        /// <returns>A plugin that matches, or null if none found.</returns>
        private static IPlugin_Visualizer FindPluginByVisGenIdent(List<IPlugin> plugins,
                string visGenIdent, out VisDescr visDescr) {
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

        #endregion Image generation


        public override string ToString() {
            return "[VS: " + mList.Count + " items]";
        }

        public static bool operator ==(VisualizationSet a, VisualizationSet b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            // All fields must be equal.
            if (a.mList.Count != b.mList.Count) {
                return false;
            }
            // Order matters.
            for (int i = 0; i < a.mList.Count; i++) {
                if (a.mList[i] != b.mList[i]) {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(VisualizationSet a, VisualizationSet b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is VisualizationSet && this == (VisualizationSet)obj;
        }
        public override int GetHashCode() {
            int hashCode = 0;
            foreach (Visualization vis in mList) {
                hashCode ^= vis.GetHashCode();
            }
            return hashCode;
        }
    }
}
