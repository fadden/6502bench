/*
 * Copyright 2018 faddenSoft
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

namespace PluginCommon {
    /// <summary>
    /// Symbols loaded from platform symbol files, for use in extension scripts.
    /// 
    /// Instances are immutable.
    /// </summary>
    [Serializable]
    public class PlatSym {
        public string Label { get; private set; }
        public int Value { get; private set; }
        public string Tag { get; private set; }

        /// <summary>
        /// Nullary constructor, for deserialization.
        /// </summary>
        private PlatSym() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="label">Symbol label.</param>
        /// <param name="value">Symbol value.</param>
        /// <param name="tag">Symbol group tag.</param>
        public PlatSym(string label, int value, string tag) {
            Label = label;
            Value = value;
            Tag = tag;
        }

        /// <summary>
        /// Generates a dictionary of platform symbols, keyed by value.  Only symbols with
        /// a matching tag are included.  If more than one symbol has the same value, only
        /// one will be included; which one it will be is undefined.
        /// </summary>
        /// <param name="platSyms">List of platform symbols to select from.</param>
        /// <param name="tag">Tag to match, or null to collect all symbols.</param>
        /// <param name="appRef">Application reference, for debug log output.</param>
        /// <returns></returns>
        public static Dictionary<int, PlatSym> GenerateValueList(List<PlatSym> platSyms,
                string tag, IApplication appRef) {
            Dictionary<int, PlatSym> dict = new Dictionary<int, PlatSym>();

            foreach (PlatSym ps in platSyms) {
                if (tag == null || tag == ps.Tag) {
                    try {
                        dict.Add(ps.Value, ps);
                    } catch (ArgumentException) {
                        appRef.DebugLog("WARNING: GenerateValueList: multiple entries with " +
                            "value " + ps.Value.ToString("x4"));
                    }
                }
            }

            if (dict.Count == 0) {
                appRef.DebugLog("PlatSym: no symbols found for tag=" + tag);
            }

            return dict;
        }

        public override string ToString() {
            return Label + "=" + Value.ToString("x4") + " [" + Tag + "]";
        }
    }
}
