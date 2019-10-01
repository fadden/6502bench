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
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows.Media;

using CommonUtil;

namespace SourceGen {
    /// <summary>
    /// Load and save project data from/to a ".dis65" file.
    /// 
    /// The various data structures get cloned to avoid situations where you can't freely
    /// rename and rearrange code because it's serialized directly to the save file.  We
    /// want to provide a layer of indirection on fields, output enums as strings rather
    /// than digits, etc.
    /// 
    /// Also, the JavaScriptSerializer can't deal with integer keys, so we have to convert
    /// dictionaries that use those to have string keys.
    /// 
    /// On the deserialization side, we want to verify the inputs to avoid anything strange
    /// getting loaded that could cause a crash or weird behavior.  The goal is to discard
    /// anything that looks wrong, providing a useful notification to the user, rather than
    /// failing outright.
    /// 
    /// I'm expecting the save file format to expand and evolve over time, possibly in
    /// incompatible ways that require independent load routines for old and new formats.
    /// </summary>
    public static class ProjectFile {
        public const string FILENAME_EXT = ".dis65";
        public static readonly string FILENAME_FILTER = Res.Strings.FILE_FILTER_DIS65;

        // This is the version of content we're writing.  Bump this any time we add anything.
        // This doesn't create forward or backward compatibility issues, because JSON will
        // ignore stuff that's in one side but not the other.  However, if we're opening a
        // newer file in an older program, it's worth letting the user know that some stuff
        // may get lost as soon as they save the file.
        public const int CONTENT_VERSION = 2;

        private static readonly bool ADD_CRLF = true;


        /// <summary>
        /// Serializes the project and writes it to the specified file.
        /// </summary>
        /// <param name="proj">Project to serialize.</param>
        /// <param name="pathName">Output path name.</param>
        /// <param name="errorMessage">Human-readable error string, or an empty string if all
        ///   went well.</param>
        /// <returns>True on success.</returns>
        public static bool SerializeToFile(DisasmProject proj, string pathName,
                out string errorMessage) {
            try {
                string serializedData = SerializableProjectFile1.SerializeProject(proj);
                if (ADD_CRLF) {
                    // Add some line breaks.  This looks awful, but it makes text diffs
                    // much more useful.
                    serializedData = TextUtil.NonQuoteReplace(serializedData, "{", "{\r\n");
                    serializedData = TextUtil.NonQuoteReplace(serializedData, "},", "},\r\n");
                }

                // Check to see if the project file is read-only.  We want to fail early
                // so we don't leave our .TMP file sitting around -- the File.Delete() call
                // will fail if the destination is read-only.
                if (File.Exists(pathName) &&
                        (File.GetAttributes(pathName) & FileAttributes.ReadOnly) != 0) { 
                    throw new IOException(string.Format(Res.Strings.ERR_FILE_READ_ONLY_FMT,
                        pathName));
                }

                // The BOM is not required or recommended for UTF-8 files, but anecdotal
                // evidence suggests that it's sometimes useful.  Shouldn't cause any harm
                // to have it in the project file.  The explicit Encoding.UTF8 argument
                // causes it to appear -- WriteAllText normally doesn't.
                //
                // Write to a temp file, then rename over original after write has succeeded.
                string tmpPath = pathName + ".TMP";
                File.WriteAllText(tmpPath, serializedData, Encoding.UTF8);
                if (File.Exists(pathName)) {
                    File.Delete(pathName);
                }
                File.Move(tmpPath, pathName);
                errorMessage = string.Empty;
                return true;
            } catch (Exception ex) {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Reads the specified file and deserializes it into the project.
        ///
        /// The deserialized form may include place-holder entries that can't be resolved
        /// until the data file is available (see the ASCII_GENERIC string sub-type).
        /// </summary>
        /// <param name="pathName">Input path name.</param>
        /// <param name="proj">Project to deserialize into.</param>
        /// <param name="report">File load report, which may contain errors or warnings.</param>
        /// <returns>True on success.</returns>
        public static bool DeserializeFromFile(string pathName, DisasmProject proj,
                out FileLoadReport report) {
            Debug.WriteLine("Deserializing '" + pathName + "'");
            report = new FileLoadReport(pathName);
            string serializedData;
            try {
                serializedData = File.ReadAllText(pathName);
            } catch (Exception ex) {
                report.Add(FileLoadItem.Type.Error, Res.Strings.ERR_PROJECT_LOAD_FAIL +
                    ": " + ex.Message);
                return false;
            }

            if (serializedData.StartsWith(SerializableProjectFile1.MAGIC)) {
                // File is a match for SerializableProjectFile1.  Strip header and deserialize.
                serializedData = serializedData.Substring(SerializableProjectFile1.MAGIC.Length);
                try {
                    bool ok = SerializableProjectFile1.DeserializeProject(serializedData,
                        proj, report);
                    if (ok) {
                        proj.UpdateCpuDef();
                    }
                    return ok;
                } catch (Exception ex) {
                    // Ideally this won't happen -- errors should be caught explicitly.  This
                    // is a catch-all to keep us from crashing on expectedly bad input.
                    report.Add(FileLoadItem.Type.Error,
                        Res.Strings.ERR_PROJECT_FILE_CORRUPT + ": " + ex);
                    return false;
                }
            } else {
                report.Add(FileLoadItem.Type.Error, Res.Strings.ERR_NOT_PROJECT_FILE);
                return false;
            }
        }

#if false
        public Dictionary<string, object> IntKeysToStrings(Dictionary<int, object> input) {
            Dictionary<string, object> output = new Dictionary<string, object>();

            foreach (KeyValuePair<int, object> entry in input) {
                output.Add(entry.Key.ToString(), entry.Value);
            }
            return output;
        }
        public Dictionary<int, object> StringKeysToInts(Dictionary<string, object> input) {
            Dictionary<int, object> output = new Dictionary<int, object>();

            foreach (KeyValuePair<string, object> entry in input) {
                if (!int.TryParse(entry.Key, out int intKey)) {
                    throw new InvalidOperationException("bad non-int key: " + entry.Key);
                }
                output.Add(intKey, entry.Value);
            }
            return output;
        }
#endif
    }

    /// <summary>
    /// Somewhat sloppy-looking JSON state dump.
    /// </summary>
    internal class SerializableProjectFile1 {
        // This appears at the top of the file, not as part of the JSON data.  The version
        // number refers to the file format version, not the application version.  Only
        // change this if a change is made that renders the file unreadable by previous versions.
        public const string MAGIC = "### 6502bench SourceGen dis65 v1.0 ###";

        public SerializableProjectFile1() { }

        public class SerProjectProperties {
            public string CpuName { get; set; }
            public bool IncludeUndocumentedInstr { get; set; }
            public int EntryFlags { get; set; }
            public string AutoLabelStyle { get; set; }
            public SerAnalysisParameters AnalysisParams { get; set; }
            public List<string> PlatformSymbolFileIdentifiers { get; set; }
            public List<string> ExtensionScriptFileIdentifiers { get; set; }
            public SortedList<string, SerDefSymbol> ProjectSyms { get; set; }

            public SerProjectProperties() { }
            public SerProjectProperties(ProjectProperties props) {
                CpuName = Asm65.CpuDef.GetCpuNameFromType(props.CpuType);
                IncludeUndocumentedInstr = props.IncludeUndocumentedInstr;
                EntryFlags = props.EntryFlags.AsInt;
                AutoLabelStyle = props.AutoLabelStyle.ToString();
                AnalysisParams = new SerAnalysisParameters(props.AnalysisParams);

                // External file identifiers require no conversion.
                PlatformSymbolFileIdentifiers = props.PlatformSymbolFileIdentifiers;
                ExtensionScriptFileIdentifiers = props.ExtensionScriptFileIdentifiers;

                // Convert project-defined symbols to serializable form.
                ProjectSyms = new SortedList<string, SerDefSymbol>();
                foreach (KeyValuePair<string, DefSymbol> kvp in props.ProjectSyms) {
                    ProjectSyms.Add(kvp.Key, new SerDefSymbol(kvp.Value));
                }
            }
        }
        public class SerAnalysisParameters {
            public bool AnalyzeUncategorizedData { get; set; }
            public string DefaultTextScanMode { get; set; }
            public int MinCharsForString { get; set; }
            public bool SeekNearbyTargets { get; set; }
            public bool SmartPlpHandling { get; set; }

            public SerAnalysisParameters() { }
            public SerAnalysisParameters(ProjectProperties.AnalysisParameters src) {
                AnalyzeUncategorizedData = src.AnalyzeUncategorizedData;
                DefaultTextScanMode = src.DefaultTextScanMode.ToString();
                MinCharsForString = src.MinCharsForString;
                SeekNearbyTargets = src.SeekNearbyTargets;
                SmartPlpHandling = src.SmartPlpHandling;
            }
        }
        public class SerAddressMap {
            public int Offset { get; set; }
            public int Addr { get; set; }
            // Length is computed field, no need to serialize

            public SerAddressMap() { }
            public SerAddressMap(AddressMap.AddressMapEntry ent) {
                Offset = ent.Offset;
                Addr = ent.Addr;
            }
        }
        public class SerTypeHintRange {
            public int Low { get; set; }
            public int High { get; set; }
            public string Hint { get; set; }

            public SerTypeHintRange() { }
            public SerTypeHintRange(int low, int high, string hintStr) {
                Low = low;
                High = high;
                Hint = hintStr;
            }
        }
        public class SerMultiLineComment {
            // NOTE: Text must be CRLF at line breaks.
            public string Text { get; set; }
            public bool BoxMode { get; set; }
            public int MaxWidth { get; set; }
            public int BackgroundColor { get; set; }

            public SerMultiLineComment() { }
            public SerMultiLineComment(MultiLineComment mlc) {
                Text = mlc.Text;
                BoxMode = mlc.BoxMode;
                MaxWidth = mlc.MaxWidth;
                BackgroundColor = ColorToInt(mlc.BackgroundColor);
            }
        }
        public class SerSymbol {
            public string Label { get; set; }
            public int Value { get; set; }
            public string Source { get; set; }
            public string Type { get; set; }

            public SerSymbol() { }
            public SerSymbol(Symbol sym) {
                Label = sym.Label;
                Value = sym.Value;
                Source = sym.SymbolSource.ToString();
                Type = sym.SymbolType.ToString();
            }
        }
        public class SerFormatDescriptor {
            public int Length { get; set; }
            public string Format { get; set; }
            public string SubFormat { get; set; }
            public SerWeakSymbolRef SymbolRef { get; set; }

            public SerFormatDescriptor() { }
            public SerFormatDescriptor(FormatDescriptor dfd) {
                Length = dfd.Length;
                Format = dfd.FormatType.ToString();
                SubFormat = dfd.FormatSubType.ToString();
                if (dfd.SymbolRef != null) {
                    SymbolRef = new SerWeakSymbolRef(dfd.SymbolRef);
                }
            }
        }
        public class SerWeakSymbolRef {
            public string Label { get; set; }
            public string Part { get; set; }

            public SerWeakSymbolRef() { }
            public SerWeakSymbolRef(WeakSymbolRef weakSym) {
                Label = weakSym.Label;
                Part = weakSym.ValuePart.ToString();
            }
        }
        public class SerDefSymbol : SerSymbol {
            public SerFormatDescriptor DataDescriptor { get; set; }
            public string Comment { get; set; }
            public bool HasWidth { get; set; }
            // Tag not relevant, Xrefs not recorded

            public SerDefSymbol() { }
            public SerDefSymbol(DefSymbol defSym) : base(defSym) {
                DataDescriptor = new SerFormatDescriptor(defSym.DataDescriptor);
                HasWidth = defSym.HasWidth;
                Comment = defSym.Comment;
            }
        }
        public class SerLocalVariableTable {
            public List<SerDefSymbol> Variables { get; set; }
            public bool ClearPrevious { get; set; }

            public SerLocalVariableTable() { }
            public SerLocalVariableTable(LocalVariableTable varTab) {
                Variables = new List<SerDefSymbol>(varTab.Count);
                for (int i = 0; i < varTab.Count; i++) {
                    DefSymbol defSym = varTab[i];
                    Variables.Add(new SerDefSymbol(defSym));
                }

                ClearPrevious = varTab.ClearPrevious;
            }
        }

        // Fields are serialized to/from JSON.  Do not change the field names.
        public int _ContentVersion { get; set; }
        public int FileDataLength { get; set; }
        public int FileDataCrc32 { get; set; }
        public SerProjectProperties ProjectProps { get; set; }
        public List<SerAddressMap> AddressMap { get; set; }
        public List<SerTypeHintRange> TypeHints { get; set; }
        public Dictionary<string, int> StatusFlagOverrides { get; set; }
        public Dictionary<string, string> Comments { get; set; }
        public Dictionary<string, SerMultiLineComment> LongComments { get; set; }
        public Dictionary<string, SerMultiLineComment> Notes { get; set; }
        public Dictionary<string, SerSymbol> UserLabels { get; set; }
        public Dictionary<string, SerFormatDescriptor> OperandFormats { get; set; }
        public Dictionary<string, SerLocalVariableTable> LvTables { get; set; }

        /// <summary>
        /// Serializes a DisasmProject into an augmented JSON string.
        /// </summary>
        /// <param name="proj">Project to serialize.</param>
        /// <returns>Augmented JSON string.</returns>
        public static string SerializeProject(DisasmProject proj) {
            StringBuilder sb = new StringBuilder();
            sb.Append(MAGIC);   // augment with version string, which will be stripped
            sb.Append("\r\n");  // will be ignored by deserializer; might get converted to \n

            SerializableProjectFile1 spf = new SerializableProjectFile1();
            spf._ContentVersion = ProjectFile.CONTENT_VERSION;

            Debug.Assert(proj.FileDataLength == proj.FileData.Length);
            spf.FileDataLength = proj.FileDataLength;
            spf.FileDataCrc32 = (int)proj.FileDataCrc32;

            // Convert AddressMap to serializable form.
            spf.AddressMap = new List<SerAddressMap>();
            foreach (AddressMap.AddressMapEntry ent in proj.AddrMap) {
                spf.AddressMap.Add(new SerAddressMap(ent));
            }

            // Reduce TypeHints to a collection of ranges.  Output the type enum as a string
            // so we're not tied to a specific value.
            spf.TypeHints = new List<SerTypeHintRange>();
            TypedRangeSet trs = new TypedRangeSet();
            for (int i = 0; i < proj.TypeHints.Length; i++) {
                trs.Add(i, (int)proj.TypeHints[i]);
            }
            IEnumerator<TypedRangeSet.TypedRange> iter = trs.RangeListIterator;
            while (iter.MoveNext()) {
                if (iter.Current.Type == (int)CodeAnalysis.TypeHint.NoHint) {
                    continue;
                }
                spf.TypeHints.Add(new SerTypeHintRange(iter.Current.Low, iter.Current.High,
                    ((CodeAnalysis.TypeHint)iter.Current.Type).ToString()));
            }

            // Convert StatusFlagOverrides to serializable form.  Just write the state out
            // as an integer... not expecting it to change.  If it does, we can convert.
            spf.StatusFlagOverrides = new Dictionary<string, int>();
            for (int i = 0; i < proj.StatusFlagOverrides.Length; i++) {
                if (proj.StatusFlagOverrides[i] == Asm65.StatusFlags.DefaultValue) {
                    continue;
                }
                spf.StatusFlagOverrides.Add(i.ToString(), proj.StatusFlagOverrides[i].AsInt);
            }

            // Convert Comments to serializable form.
            spf.Comments = new Dictionary<string, string>();
            for (int i = 0; i < proj.Comments.Length; i++) {
                if (string.IsNullOrEmpty(proj.Comments[i])) {
                    continue;
                }
                spf.Comments.Add(i.ToString(), proj.Comments[i]);
            }

            // Convert multi-line comments to serializable form.
            spf.LongComments = new Dictionary<string, SerMultiLineComment>();
            foreach (KeyValuePair<int, MultiLineComment> kvp in proj.LongComments) {
                spf.LongComments.Add(kvp.Key.ToString(), new SerMultiLineComment(kvp.Value));
            }

            // Convert multi-line notes to serializable form.
            spf.Notes = new Dictionary<string, SerMultiLineComment>();
            foreach (KeyValuePair<int, MultiLineComment> kvp in proj.Notes) {
                spf.Notes.Add(kvp.Key.ToString(), new SerMultiLineComment(kvp.Value));
            }

            // Convert user-defined labels to serializable form.
            spf.UserLabels = new Dictionary<string, SerSymbol>();
            foreach (KeyValuePair<int,Symbol> kvp in proj.UserLabels) {
                spf.UserLabels.Add(kvp.Key.ToString(), new SerSymbol(kvp.Value));
            }

            // Convert operand and data item format descriptors to serializable form.
            spf.OperandFormats = new Dictionary<string, SerFormatDescriptor>();
            foreach (KeyValuePair<int,FormatDescriptor> kvp in proj.OperandFormats) {
                spf.OperandFormats.Add(kvp.Key.ToString(), new SerFormatDescriptor(kvp.Value));
            }

            // Convert local variable tables to serializable form.
            spf.LvTables = new Dictionary<string, SerLocalVariableTable>();
            foreach (KeyValuePair<int, LocalVariableTable> kvp in proj.LvTables) {
                spf.LvTables.Add(kvp.Key.ToString(), new SerLocalVariableTable(kvp.Value));
            }

            spf.ProjectProps = new SerProjectProperties(proj.ProjectProps);

            JavaScriptSerializer ser = new JavaScriptSerializer();
            string cereal = ser.Serialize(spf);
            sb.Append(cereal);

            // Stick a linefeed at the end.  Makes git happier.
            sb.Append("\r\n");

            return sb.ToString();
        }

        /// <summary>
        /// Deserializes an augmented JSON string into a DisasmProject.
        /// </summary>
        /// <param name="cereal">Serialized data.</param>
        /// <param name="proj">Project to populate.</param>
        /// <param name="report">Error report object.</param>
        /// <returns>True on success, false on fatal error.</returns>
        public static bool DeserializeProject(string cereal, DisasmProject proj,
                FileLoadReport report) {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            SerializableProjectFile1 spf;
            try {
                spf = ser.Deserialize<SerializableProjectFile1>(cereal);
            } catch (Exception ex) {
                // The deserializer seems to be stuffing the entire data stream into the
                // exception, which we don't really want, so cap it at 256 chars.
                string msg = ex.Message;
                if (msg.Length > 256) {
                    msg = msg.Substring(0, 256) + " [...]";
                }
                report.Add(FileLoadItem.Type.Error, Res.Strings.ERR_PROJECT_FILE_CORRUPT +
                    ": " + msg);
                return false;
            }

            if (spf._ContentVersion > ProjectFile.CONTENT_VERSION) {
                // Post a warning.
                report.Add(FileLoadItem.Type.Notice, Res.Strings.PROJECT_FROM_NEWER_APP);
            }

            if (spf.FileDataLength <= 0) {
                report.Add(FileLoadItem.Type.Error, Res.Strings.ERR_BAD_FILE_LENGTH +
                    ": " + spf.FileDataLength);
                return false;
            }

            // Initialize the object and set a few simple items.
            proj.Initialize(spf.FileDataLength);
            proj.SetFileCrc((uint)spf.FileDataCrc32);

            // Deserialize ProjectProperties: misc items.
            proj.ProjectProps.CpuType = Asm65.CpuDef.GetCpuTypeFromName(spf.ProjectProps.CpuName);
            proj.ProjectProps.IncludeUndocumentedInstr = spf.ProjectProps.IncludeUndocumentedInstr;
            proj.ProjectProps.EntryFlags = Asm65.StatusFlags.FromInt(spf.ProjectProps.EntryFlags);
            if (Enum.TryParse<AutoLabel.Style>(spf.ProjectProps.AutoLabelStyle,
                    out AutoLabel.Style als)) {
                proj.ProjectProps.AutoLabelStyle = als;
            } else {
                // unknown value, leave as default
            }
            proj.ProjectProps.AnalysisParams = new ProjectProperties.AnalysisParameters();
            proj.ProjectProps.AnalysisParams.AnalyzeUncategorizedData =
                spf.ProjectProps.AnalysisParams.AnalyzeUncategorizedData;
            if (Enum.TryParse<ProjectProperties.AnalysisParameters.TextScanMode>(
                    spf.ProjectProps.AnalysisParams.DefaultTextScanMode,
                    out ProjectProperties.AnalysisParameters.TextScanMode mode)) {
                proj.ProjectProps.AnalysisParams.DefaultTextScanMode = mode;
            } else {
                // unknown value, leave as default
            }
            proj.ProjectProps.AnalysisParams.MinCharsForString =
                spf.ProjectProps.AnalysisParams.MinCharsForString;
            proj.ProjectProps.AnalysisParams.SeekNearbyTargets =
                spf.ProjectProps.AnalysisParams.SeekNearbyTargets;
            if (spf._ContentVersion < 2) {
                // This was made optional in v1.3.  Default it to true for older projects.
                proj.ProjectProps.AnalysisParams.SmartPlpHandling = true;
            } else {
                proj.ProjectProps.AnalysisParams.SmartPlpHandling =
                    spf.ProjectProps.AnalysisParams.SmartPlpHandling;
            }

            // Deserialize ProjectProperties: external file identifiers.
            Debug.Assert(proj.ProjectProps.PlatformSymbolFileIdentifiers.Count == 0);
            foreach (string str in spf.ProjectProps.PlatformSymbolFileIdentifiers) {
                proj.ProjectProps.PlatformSymbolFileIdentifiers.Add(str);
            }
            Debug.Assert(proj.ProjectProps.ExtensionScriptFileIdentifiers.Count == 0);
            foreach (string str in spf.ProjectProps.ExtensionScriptFileIdentifiers) {
                proj.ProjectProps.ExtensionScriptFileIdentifiers.Add(str);
            }

            // Deserialize ProjectProperties: project symbols.
            foreach (KeyValuePair<string, SerDefSymbol> kvp in spf.ProjectProps.ProjectSyms) {
                if (!CreateDefSymbol(kvp.Value, spf._ContentVersion, report,
                        out DefSymbol defSym)) {
                    continue;
                }
                proj.ProjectProps.ProjectSyms[defSym.Label] = defSym;
            }

            // Deserialize address map.
            foreach (SerAddressMap addr in spf.AddressMap) {
                proj.AddrMap.Set(addr.Offset, addr.Addr);
            }

            // Deserialize type hints.  Default value in new array as NoHint, so we don't
            // need to write those.  They should not have been recorded in the file.
            foreach (SerTypeHintRange range in spf.TypeHints) {
                if (range.Low < 0 || range.High >= spf.FileDataLength || range.Low > range.High) {
                    report.Add(FileLoadItem.Type.Warning, Res.Strings.ERR_BAD_RANGE +
                        ": " + Res.Strings.PROJECT_FIELD_TYPE_HINT +
                        " +" + range.Low.ToString("x6") + " - +" + range.High.ToString("x6"));
                    continue;
                }
                CodeAnalysis.TypeHint hint;
                try {
                    hint = (CodeAnalysis.TypeHint) Enum.Parse(
                        typeof(CodeAnalysis.TypeHint), range.Hint);
                } catch (ArgumentException) {
                    report.Add(FileLoadItem.Type.Warning, Res.Strings.ERR_BAD_TYPE_HINT +
                        ": " + range.Hint);
                    continue;
                }
                for (int i = range.Low; i <= range.High; i++) {
                    proj.TypeHints[i] = hint;
                }
            }

            // Deserialize status flag overrides.
            foreach (KeyValuePair<string,int> kvp in spf.StatusFlagOverrides) {
                if (!ParseValidateKey(kvp.Key, spf.FileDataLength,
                        Res.Strings.PROJECT_FIELD_STATUS_FLAGS, report, out int intKey)) {
                    continue;
                }
                proj.StatusFlagOverrides[intKey] = Asm65.StatusFlags.FromInt(kvp.Value);
            }

            // Deserialize comments.
            foreach (KeyValuePair<string,string> kvp in spf.Comments) {
                if (!ParseValidateKey(kvp.Key, spf.FileDataLength,
                        Res.Strings.PROJECT_FIELD_COMMENT, report, out int intKey)) {
                    continue;
                }
                proj.Comments[intKey] = kvp.Value;
            }

            // Deserialize long comments.
            foreach (KeyValuePair<string, SerMultiLineComment> kvp in spf.LongComments) {
                if (!ParseValidateKey(kvp.Key, spf.FileDataLength,
                        Res.Strings.PROJECT_FIELD_LONG_COMMENT, report, out int intKey)) {
                    continue;
                }
                proj.LongComments[intKey] = new MultiLineComment(kvp.Value.Text,
                    kvp.Value.BoxMode, kvp.Value.MaxWidth);
            }

            // Deserialize notes.
            foreach (KeyValuePair<string, SerMultiLineComment> kvp in spf.Notes) {
                if (!ParseValidateKey(kvp.Key, spf.FileDataLength,
                        Res.Strings.PROJECT_FIELD_NOTE, report, out int intKey)) {
                    continue;
                }
                proj.Notes[intKey] = new MultiLineComment(kvp.Value.Text,
                    ColorFromInt(kvp.Value.BackgroundColor));
            }

            // Deserialize user-defined labels.
            SortedList<string, string> labelDupCheck =
                new SortedList<string, string>(spf.UserLabels.Count);
            foreach (KeyValuePair<string, SerSymbol> kvp in spf.UserLabels) {
                if (!ParseValidateKey(kvp.Key, spf.FileDataLength,
                        Res.Strings.PROJECT_FIELD_USER_LABEL, report, out int intKey)) {
                    continue;
                }

                Symbol.Source source;
                Symbol.Type type;
                try {
                    source = (Symbol.Source)Enum.Parse(typeof(Symbol.Source), kvp.Value.Source);
                    type = (Symbol.Type)Enum.Parse(typeof(Symbol.Type), kvp.Value.Type);
                    if (source != Symbol.Source.User) {
                        // User labels are always source=user.  I don't think it really matters,
                        // but best to keep junk out.
                        throw new Exception("wrong source for user label");
                    }
                } catch (ArgumentException) {
                    report.Add(FileLoadItem.Type.Warning, Res.Strings.ERR_BAD_SYMBOL_ST +
                        ": " + kvp.Value.Source + "/" + kvp.Value.Type);
                    continue;
                }

                // Check for duplicate labels.  We only want to compare label strings, so we
                // can't test UserLabels.ContainsValue (which might be a linear search anyway).
                // Dump the labels into a sorted list.
                if (labelDupCheck.ContainsKey(kvp.Value.Label)) {
                    report.Add(FileLoadItem.Type.Warning,
                        string.Format(Res.Strings.ERR_DUPLICATE_LABEL_FMT,
                            kvp.Value.Label, intKey));
                    continue;
                }
                labelDupCheck.Add(kvp.Value.Label, string.Empty);

                proj.UserLabels[intKey] = new Symbol(kvp.Value.Label, kvp.Value.Value,
                    source, type);
            }

            // Deserialize operand format descriptors.
            foreach (KeyValuePair<string, SerFormatDescriptor> kvp in spf.OperandFormats) {
                if (!ParseValidateKey(kvp.Key, spf.FileDataLength,
                        Res.Strings.PROJECT_FIELD_OPERAND_FORMAT, report, out int intKey)) {
                    continue;
                }

                if (!CreateFormatDescriptor(kvp.Value, spf._ContentVersion, report,
                        out FormatDescriptor dfd)) {
                    continue;
                }
                // Extra validation: make sure dfd doesn't run off end.
                if (intKey < 0 || intKey + dfd.Length > spf.FileDataLength) {
                    report.Add(FileLoadItem.Type.Warning,
                        string.Format(Res.Strings.ERR_BAD_FD_FMT, intKey));
                    continue;
                }

                // TODO(maybe): check to see if the descriptor straddles an address change.
                //   Not fatal but it'll make things look weird.

                proj.OperandFormats[intKey] = dfd;
            }

            // Deserialize local variable tables.  These were added in v1.3.
            if (spf.LvTables != null) {
                foreach (KeyValuePair<string, SerLocalVariableTable> kvp in spf.LvTables) {
                    if (!ParseValidateKey(kvp.Key, spf.FileDataLength,
                            Res.Strings.PROJECT_FIELD_LV_TABLE, report, out int intKey)) {
                        continue;
                    }

                    if (!CreateLocalVariableTable(kvp.Value, spf._ContentVersion, report,
                            out LocalVariableTable lvt)) {
                        report.Add(FileLoadItem.Type.Warning,
                            string.Format(Res.Strings.ERR_BAD_LV_TABLE_FMT, intKey));
                        continue;
                    }

                    proj.LvTables[intKey] = lvt;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a Symbol from a SerSymbol.
        /// </summary>
        /// <param name="ssym">Deserialized data.</param>
        /// <param name="report">Error report object.</param>
        /// <param name="outSym">Created symbol.</param>
        /// <returns>True on success.</returns>
        private static bool CreateSymbol(SerSymbol ssym, FileLoadReport report,
                out Symbol outSym) {
            outSym = null;
            Symbol.Source source;
            Symbol.Type type;
            try {
                source = (Symbol.Source)Enum.Parse(typeof(Symbol.Source), ssym.Source);
                type = (Symbol.Type)Enum.Parse(typeof(Symbol.Type), ssym.Type);
            } catch (ArgumentException) {
                report.Add(FileLoadItem.Type.Warning, Res.Strings.ERR_BAD_SYMBOL_ST +
                    ": " + ssym.Source + "/" + ssym.Type);
                return false;
            }
            outSym = new Symbol(ssym.Label, ssym.Value, source, type/*, ssym.IsExport*/);
            return true;
        }

        /// <summary>
        /// Creates a DefSymbol from a SerDefSymbol.
        /// </summary>
        /// <param name="serDefSym">Deserialized data.</param>
        /// <param name="contentVersion">Serialization version.</param>
        /// <param name="report">Error report object.</param>
        /// <param name="outDefSym">Created symbol.</param>
        /// <returns></returns>
        private static bool CreateDefSymbol(SerDefSymbol serDefSym, int contentVersion,
                FileLoadReport report, out DefSymbol outDefSym) {
            outDefSym = null;

            if (!CreateSymbol(serDefSym, report, out Symbol sym)) {
                return false;
            }
            if (!CreateFormatDescriptor(serDefSym.DataDescriptor, contentVersion, report,
                    out FormatDescriptor dfd)) {
                return false;
            }

            outDefSym = new DefSymbol(sym, dfd, serDefSym.HasWidth, serDefSym.Comment);
            return true;
        }

        /// <summary>
        /// Creates a FormatDescriptor from a SerFormatDescriptor.
        /// </summary>
        /// <param name="sfd">Deserialized data.</param>
        /// <param name="version">Serialization version (CONTENT_VERSION).</param>
        /// <param name="report">Error report object.</param>
        /// <param name="dfd">Created FormatDescriptor.</param>
        /// <returns>True on success.</returns>
        private static bool CreateFormatDescriptor(SerFormatDescriptor sfd, int version,
                FileLoadReport report, out FormatDescriptor dfd) {
            dfd = null;
            FormatDescriptor.Type format;
            FormatDescriptor.SubType subFormat;

            if ("String".Equals(sfd.Format)) {
                // File version 1 used a different set of enumerated values for defining strings.
                // Parse it out here.
                Debug.Assert(version <= 1);
                subFormat = FormatDescriptor.SubType.ASCII_GENERIC;
                if ("None".Equals(sfd.SubFormat)) {
                    format = FormatDescriptor.Type.StringGeneric;
                } else if ("Reverse".Equals(sfd.SubFormat)) {
                    format = FormatDescriptor.Type.StringReverse;
                } else if ("CString".Equals(sfd.SubFormat)) {
                    format = FormatDescriptor.Type.StringNullTerm;
                } else if ("L8String".Equals(sfd.SubFormat)) {
                    format = FormatDescriptor.Type.StringL8;
                } else if ("L16String".Equals(sfd.SubFormat)) {
                    format = FormatDescriptor.Type.StringL16;
                } else if ("Dci".Equals(sfd.SubFormat)) {
                    format = FormatDescriptor.Type.StringDci;
                } else if ("DciReverse".Equals(sfd.SubFormat)) {
                    // No longer supported.  Nobody ever used this but the regression tests,
                    // though, so there's no reason to handle this nicely.
                    format = FormatDescriptor.Type.Dense;
                    subFormat = FormatDescriptor.SubType.None;
                } else {
                    // No idea what this is; output as dense hex.
                    format = FormatDescriptor.Type.Dense;
                    subFormat = FormatDescriptor.SubType.None;
                }
                Debug.WriteLine("Found v1 string, fmt=" + format + ", sub=" + subFormat);
                dfd = FormatDescriptor.Create(sfd.Length, format, subFormat);
                return true;
            }

            try {
                format = (FormatDescriptor.Type)Enum.Parse(
                    typeof(FormatDescriptor.Type), sfd.Format);
                if (version <= 1 && "Ascii".Equals(sfd.SubFormat)) {
                    // File version 1 used "Ascii" for all character data in numeric operands.
                    // It applied to both low and high ASCII.
                    subFormat = FormatDescriptor.SubType.ASCII_GENERIC;
                    Debug.WriteLine("Found v1 char, fmt=" + sfd.Format + ", sub=" + sfd.SubFormat);
                } else {
                    subFormat = (FormatDescriptor.SubType)Enum.Parse(
                        typeof(FormatDescriptor.SubType), sfd.SubFormat);
                }

            } catch (ArgumentException) {
                report.Add(FileLoadItem.Type.Warning, Res.Strings.ERR_BAD_FD_FORMAT +
                    ": " + sfd.Format + "/" + sfd.SubFormat);
                return false;
            }
            if (sfd.SymbolRef == null) {
                dfd = FormatDescriptor.Create(sfd.Length, format, subFormat);
            } else {
                WeakSymbolRef.Part part;
                try {
                    part = (WeakSymbolRef.Part)Enum.Parse(
                        typeof(WeakSymbolRef.Part), sfd.SymbolRef.Part);
                } catch (ArgumentException) {
                    report.Add(FileLoadItem.Type.Warning,
                        Res.Strings.ERR_BAD_SYMREF_PART +
                        ": " + sfd.SymbolRef.Part);
                    return false;
                }
                dfd = FormatDescriptor.Create(sfd.Length,
                    new WeakSymbolRef(sfd.SymbolRef.Label, part),
                    format == FormatDescriptor.Type.NumericBE);
            }
            return true;
        }

        /// <summary>
        /// Creates a LocalVariableTable from a SerLocalVariableTable.
        /// </summary>
        /// <param name="serTable">Deserialized data.</param>
        /// <param name="contentVersion">Serialization version.</param>
        /// <param name="report">Error report object.</param>
        /// <param name="outLvt">Created LocalVariableTable</param>
        /// <returns>True on success.</returns>
        private static bool CreateLocalVariableTable(SerLocalVariableTable serTable,
                int contentVersion, FileLoadReport report, out LocalVariableTable outLvt) {
            outLvt = new LocalVariableTable();
            outLvt.ClearPrevious = serTable.ClearPrevious;
            foreach (SerDefSymbol serDef in serTable.Variables) {
                // Force the "has width" field to true for local variables, because it's
                // non-optional there.  This is really only needed for loading projects
                // created in v1.3, which didn't have the "has width" property.
                serDef.HasWidth = true;
                if (!CreateDefSymbol(serDef, contentVersion, report, out DefSymbol defSym)) {
                    return false;
                }
                if (!defSym.IsVariable) {
                    // not expected to happen; skip it
                    Debug.WriteLine("Found local variable with bad source: " +
                        defSym.SymbolSource);
                    string str = string.Format(Res.Strings.ERR_BAD_LOCAL_VARIABLE_FMT,
                        defSym);
                    report.Add(FileLoadItem.Type.Warning, str);
                    continue;
                }
                outLvt.AddOrReplace(defSym);
            }
            return true;
        }

        /// <summary>
        /// Parses an integer key that was stored as a string, and checks to see if the
        /// value falls within an acceptable range.
        /// </summary>
        /// <param name="keyStr">Integer key, in string form.</param>
        /// <param name="fileLen">Length of file, for range check.</param>
        /// <param name="fieldName">Name of field, for error messages.</param>
        /// <param name="report">Error report object.</param>
        /// <param name="intKey">Returned integer key.</param>
        /// <returns>True on success.</returns>
        private static bool ParseValidateKey(string keyStr, int fileLen, string fieldName,
                FileLoadReport report, out int intKey) {
            if (!int.TryParse(keyStr, out intKey)) {
                report.Add(FileLoadItem.Type.Warning,
                    Res.Strings.ERR_INVALID_INT_VALUE + " (" +
                    fieldName + ": " + keyStr + ")");
                return false;
            }

            // Shouldn't allow DisplayList.Line.HEADER_COMMENT_OFFSET on anything but
            // LongComment.  Maybe "bool allowNegativeKeys"?
            if (intKey < fileLen &&
                    (intKey >= 0 || intKey == LineListGen.Line.HEADER_COMMENT_OFFSET)) {
                return true;
            } else {
                report.Add(FileLoadItem.Type.Warning,
                    Res.Strings.ERR_INVALID_KEY_VALUE +
                        " (" + fieldName + ": " + intKey + ")");
                return false;
            }
        }

        private static int ColorToInt(Color color) {
            return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        }

        private static Color ColorFromInt(int colorInt) {
            return Color.FromArgb((byte)(colorInt >> 24), (byte)(colorInt >> 16),
                (byte)(colorInt >> 8), (byte)colorInt);
        }
    }
}
