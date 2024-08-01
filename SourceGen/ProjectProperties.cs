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

namespace SourceGen {
    /// <summary>
    /// A collection of project properties.
    /// 
    /// The class is mutable, but may only be modified by the property editor (which updates
    /// a work object that gets put into the project by DisasmProject.ApplyChanges) or
    /// the deserializer.
    /// 
    /// All fields are explicitly handled by the ProjectFile serializer.
    /// </summary>
    public class ProjectProperties {
        //
        // *** NOTE ***
        // If you add or modify a member, make sure to update the copy constructor and
        // add serialization code to ProjectFile.
        // *** NOTE ***
        //

        /// <summary>
        /// Some parameters we feed to the analyzers.
        /// </summary>
        public class AnalysisParameters {
            // This is very similar to Formatter.FormatConfig.CharConvMode, but it serves
            // a different purpose and might diverge in the future.
            public enum TextScanMode {
                Unknown = 0,
                LowAscii,
                LowHighAscii,
                C64Petscii,
                C64ScreenCode,
            }

            public bool AnalyzeUncategorizedData { get; set; }
            public TextScanMode DefaultTextScanMode { get; set; }
            public int MinCharsForString { get; set; }
            public bool SeekNearbyTargets { get; set; }
            public bool UseRelocData { get; set; }
            public bool SmartPlpHandling { get; set; }
            public bool SmartPlbHandling { get; set; }

            public AnalysisParameters() {
                // Set default values.
                AnalyzeUncategorizedData = true;
                DefaultTextScanMode = TextScanMode.LowHighAscii;
                MinCharsForString = DataAnalysis.DEFAULT_MIN_STRING_LENGTH;
                SeekNearbyTargets = false;
                UseRelocData = false;
                SmartPlpHandling = false;
                SmartPlbHandling = true;
            }
            public AnalysisParameters(AnalysisParameters src) {
                AnalyzeUncategorizedData = src.AnalyzeUncategorizedData;
                DefaultTextScanMode = src.DefaultTextScanMode;
                MinCharsForString = src.MinCharsForString;
                SeekNearbyTargets = src.SeekNearbyTargets;
                UseRelocData = src.UseRelocData;
                SmartPlpHandling = src.SmartPlpHandling;
                SmartPlbHandling = src.SmartPlbHandling;
            }
        }

        /// <summary>
        /// Configured CPU type.
        /// </summary>
        public Asm65.CpuDef.CpuType CpuType { get; set; }

        /// <summary>
        /// Should we include undocumented instructions?
        /// </summary>
        public bool IncludeUndocumentedInstr { get; set; }

        /// <summary>
        /// Should we treat BRK instructions as 2 bytes?
        /// </summary>
        public bool TwoByteBrk { get; set; }

        /// <summary>
        /// Initial status flags at entry points.
        /// </summary>
        public Asm65.StatusFlags EntryFlags { get; set; }

        /// <summary>
        /// Naming style for auto-generated labels.
        /// </summary>
        public AutoLabel.Style AutoLabelStyle { get; set; }

        /// <summary>
        /// Configurable parameters for the analyzers.
        /// </summary>
        public AnalysisParameters AnalysisParams { get; set; }

        /// <summary>
        /// The identifiers of the platform symbol files we want to load symbols from.
        /// </summary>
        public List<string> PlatformSymbolFileIdentifiers { get; private set; }

        /// <summary>
        /// The identifiers of the extension scripts we want to load.
        /// </summary>
        public List<string> ExtensionScriptFileIdentifiers { get; private set; }

        /// <summary>
        /// Symbols defined at the project level.  These get merged with PlatformSyms.
        /// The list key is the symbol's label.
        /// </summary>
        public SortedList<string, DefSymbol> ProjectSyms { get; private set; }


        /// <summary>
        /// Nullary constructor.
        /// </summary>
        public ProjectProperties() {
            AnalysisParams = new AnalysisParameters();
            PlatformSymbolFileIdentifiers = new List<string>();
            ExtensionScriptFileIdentifiers = new List<string>();
            ProjectSyms = new SortedList<string, DefSymbol>();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="src">Object to clone.</param>
        public ProjectProperties(ProjectProperties src) : this() {
            CpuType = src.CpuType;
            IncludeUndocumentedInstr = src.IncludeUndocumentedInstr;
            TwoByteBrk = src.TwoByteBrk;
            EntryFlags = src.EntryFlags;
            AutoLabelStyle = src.AutoLabelStyle;

            AnalysisParams = new AnalysisParameters(src.AnalysisParams);

            // Clone PlatformSymbolFileIdentifiers
            foreach (string fileName in src.PlatformSymbolFileIdentifiers) {
                PlatformSymbolFileIdentifiers.Add(fileName);
            }
            // Clone ExtensionScriptFileIdentifiers
            foreach (string fileName in src.ExtensionScriptFileIdentifiers) {
                ExtensionScriptFileIdentifiers.Add(fileName);
            }

            // Clone ProjectSyms
            foreach (KeyValuePair<string, DefSymbol> kvp in src.ProjectSyms) {
                ProjectSyms[kvp.Key] = kvp.Value;
            }
        }
    }
}
