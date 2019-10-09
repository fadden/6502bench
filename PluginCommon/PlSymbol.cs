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
    /// Plugin-accessible symbol, for use in extension scripts.
    /// 
    /// Instances are immutable.
    /// </summary>
    [Serializable]
    public class PlSymbol {
        /// <summary>
        /// Subset of Symbol.Source.  Does not include auto-generated labels or variables.
        /// </summary>
        public enum Source {
            Unknown = 0,
            User,
            Project,
            Platform
        }

        /// <summary>
        /// Subset of Symbol.Type.  Does not specify local vs. global or export.
        /// </summary>
        public enum Type {
            Unknown = 0,
            Address,
            Constant
        }

        /// <summary>
        /// Label sent to assembler.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Symbol's numeric value.
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Symbol origin.
        /// </summary>
        private Source SymbolSource { get; set; }
        public bool IsUserSymbol { get { return SymbolSource == Source.User; } }
        public bool IsProjectSymbol { get { return SymbolSource == Source.Project; } }
        public bool IsPlatformSymbol { get { return SymbolSource == Source.Platform; } }

        /// <summary>
        /// Symbol type.
        /// </summary>
        private Type SymbolType { get; set; }
        public bool IsAddress { get { return SymbolType == Type.Address; } }
        public bool IsConstant { get { return SymbolType == Type.Constant; } }

        /// <summary>
        /// Platform/project symbols only: width, in bytes, of data at symbol.  Will be -1
        /// for user labels.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Platform symbols only: tag used to organize symbols into groups..
        /// </summary>
        public string Tag { get; private set; }


        /// <summary>
        /// Nullary constructor, for deserialization.
        /// </summary>
        private PlSymbol() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="label">Symbol label.</param>
        /// <param name="value">Symbol value.</param>
        /// <param name="tag">Symbol group tag.</param>
        public PlSymbol(string label, int value, int width, Source source, Type type, string tag) {
            Label = label;
            Value = value;
            Width = width;
            SymbolSource = source;
            SymbolType = type;
            Tag = tag;
        }

        /// <summary>
        /// Generates a dictionary of platform symbols, keyed by value.  Only symbols with
        /// a matching tag are included.  If more than one symbol has the same value, only
        /// one will be included; which one it will be is undefined.
        /// </summary>
        /// <param name="ppuSyms">List of platform symbols to select from.</param>
        /// <param name="tag">Tag to match, or null to collect all symbols.</param>
        /// <param name="appRef">Application reference, for debug log output.</param>
        /// <returns>Dictionary of matching platform symbols.</returns>
        public static Dictionary<int, PlSymbol> GeneratePlatformValueList(List<PlSymbol> ppuSyms,
                string tag, IApplication appRef) {
            Dictionary<int, PlSymbol> dict = new Dictionary<int, PlSymbol>();

            foreach (PlSymbol ps in ppuSyms) {
                if (ps.SymbolSource != Source.Platform) {
                    continue;
                }
                if (tag == null || tag == ps.Tag) {
                    try {
                        dict.Add(ps.Value, ps);
                    } catch (ArgumentException) {
                        appRef.DebugLog("WARNING: GenerateValueList: multiple entries with " +
                            "value " + ps.Value.ToString("x4") + ": " + dict[ps.Value].Label +
                            " and " + ps.Label);
                    }
                }
            }

            if (dict.Count == 0) {
                appRef.DebugLog("PlSymbol: no symbols found for tag=" + tag);
            }

            return dict;
        }

        public override string ToString() {
            return Label + "=" + Value.ToString("x4") + " [" + Tag + "]";
        }
    }
}
