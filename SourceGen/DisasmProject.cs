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

using Asm65;
using CommonUtil;
using SourceGen.Sandbox;

namespace SourceGen {
    /// <summary>
    /// All state for an open project.
    /// 
    /// This class does no file I/O or user interaction.
    /// </summary>
    public class DisasmProject {
        // Arbitrary 1MB limit.  Could be increased to 16MB if performance is acceptable.
        public const int MAX_DATA_FILE_SIZE = 1 << 20;

        // File magic.
        private const long MAGIC = 6982516645493599905;


        #region Data that is saved to the project file
        // All data held by structures in this section are persistent, and will be
        // written to the project file.  Anything not in this section may be discarded
        // at any time.  Smaller items are kept in arrays, with one entry per byte
        // of file data.

        /// <summary>
        /// Length of input data.  (This is redundant with FileData.Length while in memory,
        /// but we want this value to be serialized into the project file.)
        /// </summary>
        public int FileDataLength { get; private set; }

        /// <summary>
        /// CRC-32 on input data.
        /// </summary>
        public uint FileDataCrc32 { get; private set; }

        /// <summary>
        /// Map file offsets to addresses.
        /// </summary>
        public AddressMap AddrMap { get; private set; }

        /// <summary>
        /// Type hints.  Default value is "no hint".
        /// </summary>
        public CodeAnalysis.TypeHint[] TypeHints { get; private set; }

        /// <summary>
        /// Status flag overrides.  Default value is "all unspecified".
        /// </summary>
        public StatusFlags[] StatusFlagOverrides { get; private set; }

        /// <summary>
        /// End-of-line comments.  Empty string means "no comment".
        /// </summary>
        public string[] Comments { get; private set; }

        /// <summary>
        /// Full line, possibly multi-line comments.
        /// </summary>
        public Dictionary<int, MultiLineComment> LongComments { get; private set; }

        /// <summary>
        /// Notes, which are like comments but not included in the assembled output.
        /// </summary>
        public SortedList<int, MultiLineComment> Notes { get; private set; }

        /// <summary>
        /// Labels, defined by the user; uses file offset as key.  Ideally the label names
        /// are unique, but there are ways around that.
        /// </summary>
        public Dictionary<int, Symbol> UserLabels { get; private set; }

        /// <summary>
        /// Local variable tables.
        /// </summary>
        public SortedList<int, LocalVariableTable> LvTables { get; private set; }

        /// <summary>
        /// Format descriptors for operands and data items; uses file offset as key.
        /// </summary>
        public SortedList<int, FormatDescriptor> OperandFormats { get; private set; }

        /// <summary>
        /// Project properties.  Includes CPU type, platform symbol file names, project
        /// symbols, etc.
        /// </summary>
        public ProjectProperties ProjectProps { get; private set; }

        #endregion // data to save & restore


        /// <summary>
        /// The contents of the 65xx data file.
        /// </summary>
        public byte[] FileData { get { return mFileData; } }
        private byte[] mFileData;

        /// <summary>
        /// CPU definition to use when analyzing input.
        /// </summary>
        public CpuDef CpuDef { get; private set; }

        /// <summary>
        /// If true, plugins will execute in the main application's AppDomain instead of
        /// the sandbox.  Must be set before calling Initialize().
        /// </summary>
        public bool UseMainAppDomainForPlugins { get; set; }

        /// <summary>
        /// Full pathname of project file.  The directory name is needed when loading
        /// platform symbols and extension scripts from the project directory, and the
        /// filename is used to give project-local extension scripts unique DLL names.
        ///
        /// For a new project that hasn't been saved yet, this will be empty.
        /// </summary>
        public string ProjectPathName { get; set; }

        // Filename only of data file.  This is used for debugging and text export.
        public string DataFileName { get; private set; }

        // This holds working state for the code and data analyzers.  Some of the state
        // is presented directly to the user, e.g. status flags.  All of the data here
        // should be considered transient; it may be discarded at any time without
        // causing user data loss.
        private Anattrib[] mAnattribs;

        // A snapshot of the Anattribs array, taken after code analysis has completed,
        // before data analysis has begun.
        private Anattrib[] mCodeOnlyAnattribs;

        // Symbol lists loaded from platform symbol files.  This is essentially a list
        // of lists, of symbols.
        private List<PlatformSymbols> PlatformSyms { get; set; }

        // Extension script manager.  Controls AppDomain sandbox.
        private ScriptManager mScriptManager;

        // All symbols, including user-defined, platform-specific, and auto-generated, keyed by
        // label string.  This is rebuilt whenever we do a refresh, and modified whenever
        // labels or platform definitions are edited.
        //
        // Note this includes project/platform symbols that will not be in the assembled output.
        public SymbolTable SymbolTable { get; private set; }

        // Cross-reference data, indexed by file offset.
        private Dictionary<int, XrefSet> mXrefs = new Dictionary<int, XrefSet>();

        // Project and platform symbols that are being referenced from code.
        public List<DefSymbol> ActiveDefSymbolList { get; private set; }

#if DATA_PRESCAN
        // Data scan results.
        public TypedRangeSet RepeatedBytes { get; private set; }
        public RangeSet StdAsciiBytes { get; private set; }
        public RangeSet HighAsciiBytes { get; private set; }
#endif

        // List of changes for undo/redo.
        private List<ChangeSet> mUndoList = new List<ChangeSet>();

        // Index of slot where next undo operation will be placed.
        private int mUndoTop = 0;

        // Index of top when the file was last saved.
        private int mUndoSaveIndex = 0;


        /// <summary>
        /// Constructs a new project.
        /// </summary>
        public DisasmProject() { }

        /// <summary>
        /// Prepares the object by instantiating various fields, some of which are sized to
        /// match the length of the data file.  The data file may not have been loaded yet
        /// (e.g. when deserializing a project file).
        /// </summary>
        public void Initialize(int fileDataLen) {
            Debug.Assert(FileDataLength == 0);      // i.e. Initialize() hasn't run yet
            Debug.Assert(fileDataLen > 0);

            FileDataLength = fileDataLen;
            ProjectPathName = string.Empty;

            AddrMap = new AddressMap(fileDataLen);
            AddrMap.Set(0, 0x1000);    // default load address to $1000; override later

            // Default value is "no hint".
            TypeHints = new CodeAnalysis.TypeHint[fileDataLen];

            // Default value is "unspecified" for all bits.
            StatusFlagOverrides = new StatusFlags[fileDataLen];

            Comments = new string[fileDataLen];

            // Populate with empty strings so we don't have to worry about null refs.
            for (int i = 0; i < Comments.Length; i++) {
                Comments[i] = string.Empty;
            }

            LongComments = new Dictionary<int, MultiLineComment>();
            Notes = new SortedList<int, MultiLineComment>();

            UserLabels = new Dictionary<int, Symbol>();
            OperandFormats = new SortedList<int, FormatDescriptor>();
            LvTables = new SortedList<int, LocalVariableTable>();
            ProjectProps = new ProjectProperties();

            SymbolTable = new SymbolTable();
            PlatformSyms = new List<PlatformSymbols>();
            ActiveDefSymbolList = new List<DefSymbol>();

            // Default to 65816.  This will be replaced with value from project file or
            // system definition.
            ProjectProps.CpuType = CpuDef.CpuType.Cpu65816;
            ProjectProps.IncludeUndocumentedInstr = false;
            UpdateCpuDef();
        }

        /// <summary>
        /// Discards resources, notably the sandbox AppDomain.
        /// </summary>
        public void Cleanup() {
            Debug.WriteLine("DisasmProject.Cleanup(): scriptMgr=" + mScriptManager);
            if (mScriptManager != null) {
                mScriptManager.Cleanup();
                mScriptManager = null;
            }
        }

        /// <summary>
        /// Prepares the DisasmProject for use as a new project.
        /// </summary>
        /// <param name="fileData">65xx data file contents.</param>
        /// <param name="dataFileName">Data file's filename (not pathname).</param>
        public void PrepForNew(byte[] fileData, string dataFileName) {
            Debug.Assert(fileData.Length == FileDataLength);

            mFileData = fileData;
            DataFileName = dataFileName;
            FileDataCrc32 = CommonUtil.CRC32.OnWholeBuffer(0, mFileData);
#if DATA_PRESCAN
            ScanFileData();
#endif

            // Mark the first byte as code so we have something to do.  This may get
            // overridden later.
            TypeHints[0] = CodeAnalysis.TypeHint.Code;
        }

        /// <summary>
        /// Pulls items of interest out of the system definition object and applies them
        /// to the project.  Call this after LoadDataFile() for a new project.
        /// </summary>
        /// <param name="sysDef">Target system definition.</param>
        public void ApplySystemDef(SystemDef sysDef) {
            CpuDef.CpuType cpuType = CpuDef.GetCpuTypeFromName(sysDef.Cpu);
            bool includeUndoc = SystemDefaults.GetUndocumentedOpcodes(sysDef);
            CpuDef tmpDef = CpuDef.GetBestMatch(cpuType, includeUndoc);

            // Store the best-matched CPU in properties, rather than whichever was originally
            // requested.  This way the behavior of the project is the same for everyone, even
            // if somebody has a newer app version with specialized handling for the
            // originally-specified CPU.
            ProjectProps.CpuType = tmpDef.Type;
            ProjectProps.IncludeUndocumentedInstr = includeUndoc;
            UpdateCpuDef();

            ProjectProps.AnalysisParams.DefaultTextScanMode =
                SystemDefaults.GetTextScanMode(sysDef);

            ProjectProps.EntryFlags = SystemDefaults.GetEntryFlags(sysDef);

            // Configure the load address.
            if (SystemDefaults.GetFirstWordIsLoadAddr(sysDef) && mFileData.Length > 2) {
                // First two bytes are the load address, code starts at offset +000002.  We
                // need to put the load address into the stream, but don't want it to get
                // picked up as an address for something else.  So we set it to the same
                // address as the start of the file.  The overlapping-address code should do
                // the right thing with it.
                int loadAddr = RawData.GetWord(mFileData, 0, 2, false);
                AddrMap.Set(0, loadAddr);
                AddrMap.Set(2, loadAddr);
                OperandFormats[0] = FormatDescriptor.Create(2, FormatDescriptor.Type.NumericLE,
                    FormatDescriptor.SubType.None);
                TypeHints[0] = CodeAnalysis.TypeHint.NoHint;
                TypeHints[2] = CodeAnalysis.TypeHint.Code;
            } else {
                int loadAddr = SystemDefaults.GetLoadAddress(sysDef);
                AddrMap.Set(0, loadAddr);
            }

            foreach (string str in sysDef.SymbolFiles) {
                ProjectProps.PlatformSymbolFileIdentifiers.Add(str);
            }
            foreach (string str in sysDef.ExtensionScripts) {
                ProjectProps.ExtensionScriptFileIdentifiers.Add(str);
            }
        }

        public void UpdateCpuDef() {
            CpuDef = CpuDef.GetBestMatch(ProjectProps.CpuType,
                ProjectProps.IncludeUndocumentedInstr);
        }

        /// <summary>
        /// Sets the file CRC.  Called during deserialization.
        /// </summary>
        /// <param name="crc">Data file CRC.</param>
        public void SetFileCrc(uint crc) {
            Debug.Assert(FileDataLength > 0);
            FileDataCrc32 = crc;
        }

        /// <summary>
        /// Sets the file data array.  Used when the project is created from a project file.
        /// </summary>
        /// <param name="fileData">65xx data file contents.</param>
        /// <param name="dataFileName">Data file's filename (not pathname).</param>
        /// <param name="report">Reporting object for validation errors.</param>
        public void SetFileData(byte[] fileData, string dataFileName, ref FileLoadReport report) {
            Debug.Assert(fileData.Length == FileDataLength);
            Debug.Assert(CRC32.OnWholeBuffer(0, fileData) == FileDataCrc32);
            mFileData = fileData;
            DataFileName = dataFileName;

            FixAndValidate(ref report);

#if DATA_PRESCAN
            ScanFileData();
#endif
        }

#if DATA_PRESCAN
        private delegate bool ByteTest(byte val);   // for ScanFileData()

        /// <summary>
        /// Scans the contents of the file data array, noting runs of identical bytes and
        /// other interesting bits.
        /// 
        /// The file data is guaranteed not to change, so doing a bit of work here can save
        /// us time during data analysis.
        /// </summary>
        private void ScanFileData() {
            DateTime startWhen = DateTime.Now;
            // Find runs of identical bytes.
            TypedRangeSet repeats = new TypedRangeSet();

            Debug.Assert(mFileData.Length > 0);
            byte matchByte = mFileData[0];
            int count = 1;
            for (int i = 1; i < mFileData.Length; i++) {
                if (mFileData[i] == matchByte) {
                    count++;
                    continue;
                }
                if (count >= DataAnalysis.MIN_RUN_LENGTH) {
                    repeats.AddRange(i - count, i - 1, matchByte);
                }
                matchByte = mFileData[i];
                count = 1;
            }
            if (count >= DataAnalysis.MIN_RUN_LENGTH) {
                repeats.AddRange(mFileData.Length - count, mFileData.Length - 1, matchByte);
            }

            RangeSet ascii = new RangeSet();
            CreateByteRangeSet(ascii, mFileData, DataAnalysis.MIN_STRING_LENGTH,
                delegate (byte val) {
                    return val >= 0x20 && val < 0x7f;
                }
            );
            RangeSet highAscii = new RangeSet();
            CreateByteRangeSet(highAscii, mFileData, DataAnalysis.MIN_STRING_LENGTH,
                delegate (byte val) {
                    return val >= 0xa0 && val < 0xff;
                }
            );

            if (false) {
                repeats.DebugDump("Repeated-Bytes (" + DataAnalysis.MIN_RUN_LENGTH + "+)");
                ascii.DebugDump("Standard-ASCII (" + DataAnalysis.MIN_STRING_LENGTH + "+)");
                highAscii.DebugDump("High-ASCII (" + DataAnalysis.MIN_STRING_LENGTH + "+)");
            }
            Debug.WriteLine("ScanFileData took " +
                ((DateTime.Now - startWhen).TotalMilliseconds) + " ms");

            RepeatedBytes = repeats;
            StdAsciiBytes = ascii;
            HighAsciiBytes = highAscii;
        }

        private void CreateByteRangeSet(RangeSet set, byte[] data, int minLen, ByteTest tester) {
            int count = 0;
            for (int i = 0; i < data.Length; i++) {
                if (tester(data[i])) {
                    count++;
                } else if (count < minLen) {
                    count = 0;
                } else {
                    set.AddRange(i - count, i - 1);
                    count = 0;
                }
            }
            if (count >= minLen) {
                set.AddRange(data.Length - count, data.Length - 1);
            }
        }
#endif

        /// <summary>
        /// Walks the list of format descriptors, fixing places where the data doesn't match.
        /// </summary>
        private void FixAndValidate(ref FileLoadReport report) {
            Dictionary<int, FormatDescriptor> changes = new Dictionary<int, FormatDescriptor>();

            foreach (KeyValuePair<int, FormatDescriptor> kvp in OperandFormats) {
                FormatDescriptor dfd = kvp.Value;

                // v1 project files specified string layouts as sub-types, and assumed they
                // were high or low ASCII.  Numeric values could use the ASCII sub-type, which
                // included both high and low.
                //
                // v2 project files changed this to make string layouts types, with the
                // character encoding specified in the sub-type.  High and low ASCII became
                // separate, explicitly specified items.
                //
                // When loading a v1 file, the old "Ascii" sub-type is deserialized to
                // ASCII_GENERIC.  Now that we have access to the file data, we need to refine
                // the sub-type to high or low.
                if (dfd.FormatSubType == FormatDescriptor.SubType.ASCII_GENERIC) {
                    FormatDescriptor newDfd;
                    if (dfd.IsString) {
                        // Determine the string encoding by looking at the first character.
                        // For some strings (StringL8, StringL16) we need to skip forward a
                        // byte or two.  Empty strings with lengths or null-termination will
                        // be treated as low ASCII.
                        int checkOffset = kvp.Key;
                        if (dfd.FormatType == FormatDescriptor.Type.StringL8 && dfd.Length > 1) {
                            checkOffset++;
                        } else if (dfd.FormatType == FormatDescriptor.Type.StringL16 && dfd.Length > 2) {
                            checkOffset += 2;
                        }
                        bool isHigh = (FileData[checkOffset] & 0x80) != 0;
                        newDfd = FormatDescriptor.Create(dfd.Length, dfd.FormatType,
                            isHigh ? FormatDescriptor.SubType.HighAscii :
                                FormatDescriptor.SubType.Ascii);
                    } else if (dfd.IsNumeric) {
                        // This is a character constant in an instruction or data operand, such
                        // as ".dd1 'f'" or "LDA #'f'".  Could be multi-byte (even instructions
                        // can be 16-bit).  This is a little awkward, because at this point we
                        // can't tell the difference between instructions and data.
                        //
                        // However, we do know that instructions are always little-endian, that
                        // opcodes are one byte, that data values > $ff can't be ASCII encoded,
                        // and that $00 isn't a valid ASCII character.  So we can apply the
                        // following test:
                        // - if the length is 1, it's data; grab the first byte
                        // - if it's NumericBE, it's data; grab the last byte
                        // - if the second byte is $00, it's data; grab the first byte
                        // - otherwise, it's an instruction; grab the second byte
                        int checkOffset;
                        if (dfd.FormatType == FormatDescriptor.Type.NumericBE) {
                            Debug.Assert(dfd.Length <= FormatDescriptor.MAX_NUMERIC_LEN);
                            checkOffset = kvp.Key + dfd.Length - 1;
                        } else if (dfd.Length < 2 || FileData[kvp.Key + 1] == 0x00) {
                            checkOffset = kvp.Key;
                        } else {
                            Debug.Assert(dfd.FormatType == FormatDescriptor.Type.NumericLE);
                            checkOffset = kvp.Key + 1;
                        }
                        bool isHigh = (FileData[checkOffset] & 0x80) != 0;
                        newDfd = FormatDescriptor.Create(dfd.Length, dfd.FormatType,
                            isHigh ? FormatDescriptor.SubType.HighAscii :
                                FormatDescriptor.SubType.Ascii);
                    } else {
                        Debug.Assert(false);
                        newDfd = dfd;
                    }
                    changes[kvp.Key] = newDfd;
                    Debug.WriteLine("Fix +" + kvp.Key.ToString("x6") + ": " +
                        dfd + " -> " + newDfd);
                }
            }

            // apply changes to main list
            foreach (KeyValuePair<int, FormatDescriptor> kvp in changes) {
                OperandFormats[kvp.Key] = kvp.Value;
                //report.Add(FileLoadItem.Type.Notice,
                //    "Fixed format at +" + kvp.Key.ToString("x6"));
            }

            // TODO: validate strings
            // - null-terminated strings must not have 0x00 bytes, except for the last byte,
            //   which must be 0x00
            // - the length stored in L8/L16 strings much match the format descriptor length
            // - DCI strings must have the appropriate pattern for the high bit
            //
            // Note it is not required that string data match the encoding, since you're allowed
            // to have random gunk mixed in.  It just can't violate the above rules.
        }

        /// <summary>
        /// Loads platform symbol files and extension scripts.
        /// 
        /// Call this on initial load and whenever the set of platform symbol files changes
        /// in the project config.
        /// 
        /// Failures here will be reported to the user but aren't fatal.
        /// </summary>
        /// <returns>String with all warnings from load process.</returns>
        public string LoadExternalFiles() {
            TaskTimer timer = new TaskTimer();
            timer.StartTask("Total");

            StringBuilder sb = new StringBuilder();

            string projectDir = string.Empty;
            if (!string.IsNullOrEmpty(ProjectPathName)) {
                projectDir = Path.GetDirectoryName(ProjectPathName);
            }

            // Load the platform symbols first.
            timer.StartTask("Platform Symbols");
            PlatformSyms.Clear();
            foreach (string fileIdent in ProjectProps.PlatformSymbolFileIdentifiers) {
                PlatformSymbols ps = new PlatformSymbols();
                bool ok = ps.LoadFromFile(fileIdent, projectDir, out FileLoadReport report);
                if (ok) {
                    PlatformSyms.Add(ps);
                }
                if (report.Count > 0) {
                    sb.Append(report.Format());
                }
            }
            timer.EndTask("Platform Symbols");

            // Instantiate the script manager on first use.
            timer.StartTask("Create ScriptManager");
            if (mScriptManager == null) {
                mScriptManager = new ScriptManager(this);
            } else {
                mScriptManager.Clear();
            }
            timer.EndTask("Create ScriptManager");

            // Load the extension script files.
            timer.StartTask("Load Extension Scripts");
            foreach (string fileIdent in ProjectProps.ExtensionScriptFileIdentifiers) {
                bool ok = mScriptManager.LoadPlugin(fileIdent, out FileLoadReport report);
                if (report.Count > 0) {
                    sb.Append(report.Format());
                }
            }
            timer.EndTask("Load Extension Scripts");

            timer.EndTask("Total");
            timer.DumpTimes("Time to load external files:");

            return sb.ToString();
        }

        /// <summary>
        /// Checks some stuff.  Problems are handled with assertions, so this is only
        /// useful in debug builds.
        /// </summary>
        public void Validate() {
            // Confirm that we can walk through the file, stepping directly from the start
            // of one thing to the start of the next.
            int offset = 0;
            while (offset < mFileData.Length) {
                Anattrib attr = mAnattribs[offset];
                bool thisIsCode = attr.IsInstructionStart;
                Debug.Assert(attr.IsStart);
                Debug.Assert(attr.Length != 0);
                offset += attr.Length;

                // Sometimes embedded instructions continue past the "outer" instruction,
                // usually because we're misinterpreting the code.  We need to deal with
                // that here.
                int extraInstrBytes = 0;
                while (offset < mFileData.Length && mAnattribs[offset].IsInstruction &&
                        !mAnattribs[offset].IsInstructionStart) {
                    extraInstrBytes++;
                    offset++;
                }

                // Make sure the extra code bytes were part of an instruction.  Otherwise it
                // means we moved from the end of a data area to the middle of an instruction,
                // which is very bad.
                Debug.Assert(extraInstrBytes == 0 || thisIsCode);

                //if (extraInstrBytes > 0) { Debug.WriteLine("EIB=" + extraInstrBytes); }
                // Max instruction len is 4, so the stray part must be shorter.
                Debug.Assert(extraInstrBytes < 4);
            }
            Debug.Assert(offset == mFileData.Length);

            // Confirm that all bytes are tagged as code, data, or inline data.  The Asserts
            // in Anattrib should confirm that nothing is tagged as more than one thing.
            for (offset = 0; offset < mAnattribs.Length; offset++) {
                Anattrib attr = mAnattribs[offset];
                Debug.Assert(attr.IsInstruction || attr.IsInlineData || attr.IsData);
            }

            // Confirm that there are no Default format entries in OperandFormats.
            foreach (KeyValuePair<int, FormatDescriptor> kvp in OperandFormats) {
                Debug.Assert(kvp.Value.FormatType != FormatDescriptor.Type.Default);
                Debug.Assert(kvp.Value.FormatType != FormatDescriptor.Type.REMOVE);
            }
        }

        #region Analysis

        /// <summary>
        /// Analyzes the file data.  This is the main entry point for code/data analysis.
        /// </summary>
        /// <param name="reanalysisRequired">How much work to do.</param>
        /// <param name="debugLog">Object to send debug output to.</param>
        /// <param name="reanalysisTimer">Task timestamp collection object.</param>
        public void Analyze(UndoableChange.ReanalysisScope reanalysisRequired,
                CommonUtil.DebugLog debugLog, TaskTimer reanalysisTimer) {
            // This method doesn't report failures.  It succeeds to the best of its ability,
            // and handles problems by discarding bad data.  The overall philosophy is that
            // the program will never generate bad data, and any bad project file contents
            // (possibly introduced by hand-editing) are identified at load time, called out
            // to the user, and discarded.
            Debug.Assert(reanalysisRequired != UndoableChange.ReanalysisScope.None);
            reanalysisTimer.StartTask("DisasmProject.Analyze()");

            // Populate the symbol table with platform symbols, in file load order, then
            // merge in the project symbols, potentially replacing platform symbols that
            // have the same label.  This version of the table is passed to plugins during
            // code analysis.
            reanalysisTimer.StartTask("SymbolTable init");
            SymbolTable.Clear();
            MergePlatformProjectSymbols();
            // Merge user labels into the symbol table, overwriting platform/project symbols
            // where they conflict.  Labels whose values are out of sync (because of a change
            // to the address map) are updated as part of this.
            UpdateAndMergeUserLabels();
            reanalysisTimer.EndTask("SymbolTable init");

            if (reanalysisRequired == UndoableChange.ReanalysisScope.CodeAndData) {
                // Always want to start with a blank array.  Going to be lazy and let the
                // system allocator handle that for us.
                mAnattribs = new Anattrib[mFileData.Length];

                reanalysisTimer.StartTask("CodeAnalysis.Analyze");

                CodeAnalysis ca = new CodeAnalysis(mFileData, CpuDef, mAnattribs, AddrMap,
                    TypeHints, StatusFlagOverrides, ProjectProps.EntryFlags,
                    ProjectProps.AnalysisParams, mScriptManager, debugLog);

                ca.Analyze();
                reanalysisTimer.EndTask("CodeAnalysis.Analyze");

                // Save a copy of the current state.
                mCodeOnlyAnattribs = new Anattrib[mAnattribs.Length];
                Array.Copy(mAnattribs, mCodeOnlyAnattribs, mAnattribs.Length);
            } else {
                // Load Anattribs array from the stored copy.
                Debug.WriteLine("Partial reanalysis");
                reanalysisTimer.StartTask("CodeAnalysis (restore prev)");
                Debug.Assert(mCodeOnlyAnattribs != null);
                Array.Copy(mCodeOnlyAnattribs, mAnattribs, mAnattribs.Length);
                reanalysisTimer.EndTask("CodeAnalysis (restore prev)");
            }

            reanalysisTimer.StartTask("Apply labels, formats, etc.");
            // Apply any user-defined labels to the Anattribs array.
            ApplyUserLabels(debugLog);

            // Apply user-created format descriptors to instructions and data items.
            ApplyFormatDescriptors(debugLog);
            reanalysisTimer.EndTask("Apply labels, formats, etc.");

            reanalysisTimer.StartTask("DataAnalysis");
            DataAnalysis da = new DataAnalysis(this, mAnattribs);
            da.DebugLog = debugLog;

            reanalysisTimer.StartTask("DataAnalysis.AnalyzeDataTargets");
            da.AnalyzeDataTargets();
            reanalysisTimer.EndTask("DataAnalysis.AnalyzeDataTargets");

            // Analyze uncategorized regions.  When this completes, the Anattrib array will
            // be complete for every offset, and the file will be traversible by walking
            // through the lengths of each entry.
            reanalysisTimer.StartTask("DataAnalysis.AnalyzeUncategorized");
            da.AnalyzeUncategorized();
            reanalysisTimer.EndTask("DataAnalysis.AnalyzeUncategorized");

            reanalysisTimer.EndTask("DataAnalysis");

            reanalysisTimer.StartTask("RemoveHiddenLabels");
            RemoveHiddenLabels();
            reanalysisTimer.EndTask("RemoveHiddenLabels");


            // ----------
            // NOTE: we could add an additional re-analysis entry point here, that just deals with
            // platform symbols and xrefs, to be used after a change to project symbols.  We'd
            // need to check all existing refs to confirm that the symbol hasn't been removed.
            // Symbol updates are sufficiently infrequent that this probably isn't worthwhile.

            reanalysisTimer.StartTask("GenerateVariableRefs");
            // Generate references to variables.
            GenerateVariableRefs();
            reanalysisTimer.EndTask("GenerateVariableRefs");

            // NOTE: we could at this point apply platform address symbols as code labels, so
            // that locations in the code that correspond to well-known addresses would pick
            // up the appropriate label instead of getting auto-labeled.  It's unclear
            // whether this is desirable, especially if the user is planning to modify the
            // output later on, and it could mess things up if we start slapping
            // labels into the middle of data regions.  It's generally safer to treat
            // platform symbols as labels for constants and external references.  If somebody
            // finds an important use case we can revisit this; might merit a special type
            // of equate or section in the platform symbol definition file.

            reanalysisTimer.StartTask("GeneratePlatformSymbolRefs");
            // Generate references to platform and project external symbols.
            GeneratePlatformSymbolRefs();
            reanalysisTimer.EndTask("GeneratePlatformSymbolRefs");

            reanalysisTimer.StartTask("GenerateXrefs");
            // Generate cross-reference lists.
            mXrefs.Clear();
            GenerateXrefs();
            reanalysisTimer.EndTask("GenerateXrefs");

            // replace simple auto-labels ("L1234") with annotated versions ("WR_1234")
            if (ProjectProps.AutoLabelStyle != AutoLabel.Style.Simple) {
                reanalysisTimer.StartTask("AnnotateAutoLabels");
                AnnotateAutoLabels();
                reanalysisTimer.EndTask("AnnotateAutoLabels");
            }

            reanalysisTimer.StartTask("GenerateActiveDefSymbolList");
            // Generate the list of project/platform symbols that are being used.  This forms
            // the list of EQUates at the top of the file.  The active set is identified from
            // the cross-reference data.
            GenerateActiveDefSymbolList();
            reanalysisTimer.EndTask("GenerateActiveDefSymbolList");

#if DEBUG
            reanalysisTimer.StartTask("Validate");
            Validate();
            reanalysisTimer.EndTask("Validate");
#endif

            reanalysisTimer.EndTask("DisasmProject.Analyze()");
            //reanalysisTimer.DumpTimes("DisasmProject timers:", debugLog);

            debugLog.LogI("Analysis complete");
        }

        /// <summary>
        /// Applies user labels to the Anattribs array.  Symbols with stale Value fields will
        /// be replaced.
        /// </summary>
        /// <param name="genLog">Log for debug messages.</param>
        private void ApplyUserLabels(DebugLog genLog) {
            foreach (KeyValuePair<int, Symbol> kvp in UserLabels) {
                int offset = kvp.Key;
                if (offset < 0 || offset >= mAnattribs.Length) {
                    genLog.LogE("Invalid offset +" + offset.ToString("x6") +
                        "(label=" + kvp.Value.Label + ")");
                    continue;       // ignore this
                }

                if (mAnattribs[offset].Symbol != null) {
                    genLog.LogW("Multiple labels at offset +" + offset.ToString("x6") +
                        ": " + kvp.Value.Label + " / " + mAnattribs[offset].Symbol.Label);
                    continue;
                }

                int expectedAddr = kvp.Value.Value;
                Debug.Assert(expectedAddr == AddrMap.OffsetToAddress(offset));

                // Add direct reference to the UserLabels Symbol object.
                mAnattribs[offset].Symbol = kvp.Value;
            }
        }

        /// <summary>
        /// Applies user-defined format descriptors to the Anattribs array.  This specifies the
        /// format for instruction operands, and identifies data items.
        /// </summary>
        /// <param name="genLog">Log for debug messages.</param>
        private void ApplyFormatDescriptors(DebugLog genLog) {
            foreach (KeyValuePair<int, FormatDescriptor> kvp in OperandFormats) {
                int offset = kvp.Key;

                // If you hint as data, apply formats, and then hint as code, all sorts
                // of strange things can happen.  We want to ignore anything that doesn't
                // appear to be valid.  While we're at it, we do some internal consistency
                // checks in the name of catching bugs as soon as possible.

                // Check offset.
                if (offset < 0 || offset >= mAnattribs.Length) {
                    genLog.LogE("Invalid offset +" + offset.ToString("x6") +
                        "(desc=" + kvp.Value + ")");
                    Debug.Assert(false);
                    continue;       // ignore this one
                }

                // Make sure it doesn't run off the end
                if (offset + kvp.Value.Length > mAnattribs.Length) {
                    genLog.LogE("Invalid offset+len +" + offset.ToString("x6") +
                        " len=" + kvp.Value.Length + " file=" + mAnattribs.Length);
                    Debug.Assert(false);
                    continue;       // ignore this one
                }

                if (mAnattribs[offset].IsInstructionStart) {
                    // Check length for instruction formatters.  This can happen if you format
                    // a bunch of bytes as single-byte data items and then add a code entry
                    // point.
                    if (kvp.Value.Length != mAnattribs[offset].Length) {
                        genLog.LogW("+" + offset.ToString("x6") +
                            ": unexpected length on instr format descriptor (" +
                            kvp.Value.Length + " vs " + mAnattribs[offset].Length + ")");
                        continue;       // ignore this one
                    }
                    if (kvp.Value.Length == 1) {
                        // No operand to format!
                        genLog.LogW("+" + offset.ToString("x6") +
                            ": unexpected format descriptor on single-byte op");
                        continue;       // ignore this one
                    }
                    if (!kvp.Value.IsValidForInstruction) {
                        genLog.LogW("Descriptor not valid for instruction: " + kvp.Value);
                        continue;       // ignore this one
                    }
                } else if (mAnattribs[offset].IsInstruction) {
                    // Mid-instruction format.
                    genLog.LogW("+" + offset.ToString("x6") +
                        ": unexpected mid-instruction format descriptor");
                    continue;       // ignore this one
                }

                mAnattribs[offset].DataDescriptor = kvp.Value;
            }
        }

        /// <summary>
        /// Merges symbols from PlatformSymbols and ProjectSymbols into SymbolTable.
        ///
        /// This should be done before any other symbol assignment or generation, so that user
        /// labels take precedence (by virtue of overwriting the earlier platform symbols),
        /// and auto label generation can propery generate a unique label.
        ///
        /// Within platform symbol loading, later symbols should replace earlier symbols,
        /// so that ordering of platform files behaves in an intuitive fashion.
        /// </summary>
        private void MergePlatformProjectSymbols() {
            // Start by pulling in the platform symbols.
            foreach (PlatformSymbols ps in PlatformSyms) {
                foreach (Symbol sym in ps) {
                    SymbolTable[sym.Label] = sym;
                }
            }

            // Now add project symbols, overwriting platform symbols with the same label.
            foreach (KeyValuePair<string, DefSymbol> kvp in ProjectProps.ProjectSyms) {
                SymbolTable[kvp.Value.Label] = kvp.Value;
            }
        }

        /// <summary>
        /// Merges symbols from UserLabels into SymbolTable.  Existing entries with matching
        /// labels will be replaced.
        /// </summary>
        private void UpdateAndMergeUserLabels() {
            // We store symbols as label+value, but for a user label the actual value is
            // the address of the offset the label is associated with.  It's convenient
            // to store labels as Symbols because we also want the Type value, and it avoids
            // having to create Symbol objects on the fly.  If the value in the UserLabel
            // is wrong, we fix it here.

            Dictionary<int, Symbol> changes = new Dictionary<int, Symbol>();

            foreach (KeyValuePair<int, Symbol> kvp in UserLabels) {
                int offset = kvp.Key;
                Symbol sym = kvp.Value;
                int expectedAddr = AddrMap.OffsetToAddress(offset);
                if (sym.Value != expectedAddr) {
                    Symbol newSym = new Symbol(sym.Label, expectedAddr, sym.SymbolSource,
                        sym.SymbolType);
                    Debug.WriteLine("Replacing label sym: " + sym + " --> " + newSym);
                    changes[offset] = newSym;
                    sym = newSym;
                }
                SymbolTable[kvp.Value.Label] = sym;
            }

            // If we updated any symbols, merge the changes back into UserLabels.
            if (changes.Count != 0) {
                Debug.WriteLine("...merging " + changes.Count + " symbols into UserLabels");
            }
            foreach (KeyValuePair<int, Symbol> kvp in changes) {
                UserLabels[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// Removes user labels from the symbol table if they're in the middle of an
        /// instruction or multi-byte data area.  (Easy way to cause this: hint a 3-byte
        /// instruction as data, add a label to the middle byte, remove hints.)
        /// 
        /// Call this after the code and data analysis passes have completed.  Any
        /// references to the hidden labels will just fall through.  It will be possible
        /// to create multiple labels with the same name, because the app won't see them
        /// in the symbol table.
        /// </summary>
        private void RemoveHiddenLabels() {
            // TODO(someday): keep the symbols in the symbol table so we can't create a
            //   duplicate, but flag it as hidden.  The symbol resolver will need to know
            //   to ignore it.  Provide a way for users to purge them.  We could just blow
            //   them out of UserLabels right now, but I'm trying to avoid discarding user-
            //   created data without permission.
            foreach (KeyValuePair<int, Symbol> kvp in UserLabels) {
                int offset = kvp.Key;
                if (!mAnattribs[offset].IsStart) {
                    Debug.WriteLine("Stripping hidden label '" + kvp.Value.Label + "'");
                    SymbolTable.Remove(kvp.Value);
                }
            }
        }

        /// <summary>
        /// Generates references to symbols in the local variable tables.
        ///
        /// These only apply to instructions with a specific set of addressing modes.
        ///
        /// This must be called after the code and data analysis passes have completed.  It
        /// should run before project/platform symbol references are generated, since we want
        /// variables to take precedence.
        ///
        /// This also adds all symbols in non-hidden variable tables to the main SymbolTable,
        /// for the benefit of future uniqueness checks.
        /// </summary>
        private void GenerateVariableRefs() {
            LocalVariableLookup lvLookup = new LocalVariableLookup(LvTables, this, false);

            for (int offset = 0; offset < FileData.Length; ) {
                // Was a table defined at this offset?
                List<DefSymbol> vars = lvLookup.GetVariablesDefinedAtOffset(offset);
                if (vars != null) {
                    // All entries also get added to the main SymbolTable.  This is a little
                    // wonky because the symbol might already exist with a different value.
                    // So long as the previous thing was also a variable, it doesn't matter.
                    foreach (DefSymbol defSym in vars) {
                        if (!SymbolTable.TryGetValue(defSym.Label, out Symbol sym)) {
                            // Symbol not yet in symbol table.  Add it.
                            //
                            // NOTE: if you try to run the main app with uniqification enabled,
                            // this will cause the various uniquified forms of local variables
                            // to end up in the main symbol table.  This can cause clashes with
                            // user labels that would not occur otherwise.
                            SymbolTable[defSym.Label] = defSym;
                        } else if (!sym.IsVariable) {
                            // Somehow we have a variable and a non-variable with the same
                            // name.  Platform/project symbols haven't been processed yet, so
                            // this must be a clash with a user label.  This could cause
                            // assembly source gen to fail later on.  It's possible to do this
                            // by "hiding" a table and then adding a user label, so we can't just
                            // fix it at project load time.
                            //
                            // This is now handled by the LvLookup code, which renames the
                            // duplicate label, so we shouldn't get here.
                            Debug.WriteLine("Found non-variable with var name in symbol table: "
                                + sym);
                            Debug.Assert(false);
                        }
                    }
                }

                Anattrib attr = mAnattribs[offset];
                if (attr.IsInstructionStart && attr.DataDescriptor == null) {
                    OpDef op = CpuDef.GetOpDef(FileData[offset]);
                    DefSymbol defSym = null;
                    if (op.IsDirectPageInstruction) {
                        Debug.Assert(attr.OperandAddress == FileData[offset + 1]);
                        defSym = lvLookup.GetSymbol(offset, FileData[offset + 1],
                            Symbol.Type.ExternalAddr);
                    } else if (op.IsStackRelInstruction) {
                        defSym = lvLookup.GetSymbol(offset, FileData[offset + 1],
                            Symbol.Type.Constant);
                    }
                    if (defSym != null) {
                        WeakSymbolRef vref = new WeakSymbolRef(defSym.Label,
                            WeakSymbolRef.Part.Low, op.IsStackRelInstruction ?
                                WeakSymbolRef.LocalVariableType.StackRelConst :
                                WeakSymbolRef.LocalVariableType.DpAddr);
                        mAnattribs[offset].DataDescriptor =
                            FormatDescriptor.Create(attr.Length, vref, false);
                    }
                }

                if (attr.IsDataStart || attr.IsInlineDataStart) {
                    offset += attr.Length;
                } else {
                    // Advance by one, not attr.Length, so we don't miss embedded instructions.
                    offset++;
                }
            }
        }

        /// <summary>
        /// Generates references to symbols in the project/platform symbol tables.
        /// 
        /// For each instruction or data item that appears to reference an address, and
        /// does not have a target offset, look for a matching address in the symbol tables.
        /// 
        /// This works pretty well for addresses, but is a little rough for constants.
        /// 
        /// Call this after the code and data analysis passes have completed.  This doesn't
        /// interact with labels, so the ordering there doesn't matter.
        /// </summary>
        private void GeneratePlatformSymbolRefs() {
            bool checkNearby = ProjectProps.AnalysisParams.SeekNearbyTargets;

            for (int offset = 0; offset < mAnattribs.Length; ) {
                Anattrib attr = mAnattribs[offset];
                if (attr.IsInstructionStart && attr.DataDescriptor == null &&
                        attr.OperandAddress >= 0 && attr.OperandOffset < 0) {
                    // Has an operand address, but not an offset, meaning it's a reference
                    // to an address outside the scope of the file. See if it has a
                    // platform symbol definition.
                    //
                    // It might seem unwise to examine the full symbol table, because it has
                    // non-project non-platform symbols in it.  However, any matching user
                    // labels would have been applied already.  Also, we want to ensure that
                    // conflicting user labels take precedence, e.g. creating a user label "COUT"
                    // will prevent a platform symbol with the same name from being visible.
                    // Using the full symbol table is potentially a tad less efficient than
                    // looking for a match exclusively in project/platform symbols, but it's
                    // the correct thing to do.
                    Symbol sym = SymbolTable.FindAddressByValue(attr.OperandAddress);

                    // If we didn't find it, check addr-1.  This is very helpful when working
                    // with pointers, because it gets us references to "PTR+1" when "PTR" is
                    // defined.  (It's potentially helpful in labeling the "near side" of an
                    // address map split as well, since the first byte past is an external
                    // address, and a label at the end of the current region will be offset
                    // from by this.)
                    if (sym == null && (attr.OperandAddress & 0xffff) > 0 && checkNearby) {
                        sym = SymbolTable.FindAddressByValue(attr.OperandAddress - 1);
                    }
                    // If that didn't work, try addr-2.  Good for 24-bit addresses and jump
                    // vectors that start with a JMP instruction.
                    if (sym == null && (attr.OperandAddress & 0xffff) > 1 && checkNearby) {
                        sym = SymbolTable.FindAddressByValue(attr.OperandAddress - 2);
                    }
                    // Still nothing, try addr+1.  Sometimes indexed addressing will use
                    // "STA addr-1,y".  This will also catch "STA addr-1" when addr is the
                    // very start of a segment, which means we're actually finding a label
                    // reference rather than project/platform symbol; only works if the
                    // location already has a label.
                    if (sym == null && (attr.OperandAddress & 0xffff) < 0xffff && checkNearby) {
                        sym = SymbolTable.FindAddressByValue(attr.OperandAddress + 1);
                        if (sym != null && sym.SymbolSource != Symbol.Source.Project &&
                                sym.SymbolSource != Symbol.Source.Platform) {
                            Debug.WriteLine("Applying non-platform in GeneratePlatform: " + sym);
                            // should be okay to do this
                        }
                    }

                    // If we found something, and it's not a variable, create a descriptor.
                    if (sym != null && !sym.IsVariable) {
                        mAnattribs[offset].DataDescriptor =
                            FormatDescriptor.Create(mAnattribs[offset].Length,
                                new WeakSymbolRef(sym.Label, WeakSymbolRef.Part.Low), false);

                        // Used to do this here; now do it in GenerateXrefs() so we can
                        // pick up user-edited operand formats that reference project symbols.
                        //(sym as DefSymbol).Xrefs.Add(new XrefSet.Xref(offset,
                        //    XrefSet.XrefType.NameReference, 0));
                    }
                }

                if (attr.IsDataStart || attr.IsInlineDataStart) {
                    offset += attr.Length;
                } else {
                    // Advance by one, not attr.Length, so we don't miss embedded instructions.
                    offset++;
                }
            }
        }

        /// <summary>
        /// Generates labels for branch and data targets, and xref lists for all referenced
        /// offsets.  Also generates Xref entries for DefSymbols (for .eq directives).
        /// 
        /// Call this after the code and data analysis passes have completed.
        /// </summary>
        private void GenerateXrefs() {
            // Xref generation.  There are two general categories of references:
            //  (1) Numeric reference.  Comes from instructions (e.g. "LDA $1000" or "BRA $1000")
            //      and Numeric/Address data items.
            //  (2) Symbolic reference.  Comes from instructions and data with Symbol format
            //      descriptors.  In some cases this may be a partial ref, e.g. "LDA #>label".
            //      The symbol's value may not match the operand, in which case an adjustment
            //      is applied.
            //
            // We want to tag both.  So if "LDA $1000" becomes "LDA label-2", we want to
            // add a numeric reference to the code at $1000, and a symbolic reference to the
            // labe at $1002, that point back to the LDA instruction.  These are presented
            // slightly differently to the user.  For a symbolic reference with no adjustment,
            // we don't add the (redundant) numeric reference.
            //
            // In some cases the numeric reference will land in the middle of an instruction
            // or multi-byte data area and won't be visible.

            // Clear previous cross-reference data from project/platform symbols.  These
            // symbols don't have file offsets, so we can't store them in the main mXrefs
            // list.
            // TODO(someday): DefSymbol is otherwise immutable.  We should put these elsewhere,
            //   maybe a Dictionary<DefSymbol, XrefSet>?  Just mind the garbage collection.
            foreach (Symbol sym in SymbolTable) {
                if (sym is DefSymbol) {
                    (sym as DefSymbol).Xrefs.Clear();
                }
            }

            // Create a mapping from label (which must be unique) to file offset.  This
            // is different from UserLabels (which only has user-created labels, and is
            // sorted by offset) and SymbolTable (which has constants and platform symbols,
            // and uses the address as value rather than the offset).
            SortedList<string, int> labelList = new SortedList<string, int>(mFileData.Length,
                Asm65.Label.LABEL_COMPARER);
            for (int offset = 0; offset < mAnattribs.Length; offset++) {
                Anattrib attr = mAnattribs[offset];
                if (attr.Symbol != null) {
                    try {
                        labelList.Add(attr.Symbol.Label, offset);
                    } catch (ArgumentException ex) {
                        // Duplicate UserLabel entries are stripped when projects are loaded,
                        // but it might be possible to cause this by hiding/unhiding a
                        // label (e.g. using hints to place it in the middle of an instruction).
                        // Just ignore the duplicate.
                        Debug.WriteLine("Xref ignoring duplicate label '" + attr.Symbol.Label +
                            "': " + ex.Message);
                    }
                }
            }

            LocalVariableLookup lvLookup = new LocalVariableLookup(LvTables, this, false);

            // Walk through the Anattrib array, adding xref entries to things referenced
            // by the entity at the current offset.
            for (int offset = 0; offset < mAnattribs.Length; ) {
                Anattrib attr = mAnattribs[offset];

                XrefSet.XrefType xrefType = XrefSet.XrefType.Unknown;
                OpDef.MemoryEffect accType = OpDef.MemoryEffect.Unknown;
                if (attr.IsInstruction) {
                    OpDef op = CpuDef.GetOpDef(FileData[offset]);
                    if (op.IsSubroutineCall) {
                        xrefType = XrefSet.XrefType.SubCallOp;
                    } else if (op.IsBranchOrSubCall) {
                        xrefType = XrefSet.XrefType.BranchOp;
                    } else {
                        xrefType = XrefSet.XrefType.MemAccessOp;
                        accType = op.MemEffect;
                    }
                } else if (attr.IsData || attr.IsInlineData) {
                    xrefType = XrefSet.XrefType.RefFromData;
                }

                bool hasZeroOffsetSym = false;
                if (attr.DataDescriptor != null) {
                    FormatDescriptor dfd = attr.DataDescriptor;
                    if (dfd.FormatSubType == FormatDescriptor.SubType.Symbol) {
                        // For instructions with address operands that resolve in-file, grab
                        // the target offset.
                        int operandOffset = -1;
                        if (attr.IsInstructionStart) {
                            operandOffset = attr.OperandOffset;
                        }

                        // Is this a reference to a label?
                        if (labelList.TryGetValue(dfd.SymbolRef.Label, out int symOffset)) {
                            // Compute adjustment.
                            int adj = 0;
                            if (operandOffset >= 0) {
                                // We can compute (symOffset - operandOffset), but that gives us
                                // the offset adjustment, not the address adjustment.
                                adj = mAnattribs[symOffset].Address -
                                    mAnattribs[operandOffset].Address;
                            }

                            AddXref(symOffset,
                                new XrefSet.Xref(offset, true, xrefType, accType, adj));
                            if (adj == 0) {
                                hasZeroOffsetSym = true;
                            }
                        } else if (dfd.SymbolRef.IsVariable) {
                            DefSymbol defSym = lvLookup.GetSymbol(offset, dfd.SymbolRef);
                            if (defSym != null) {
                                int adj = 0;
                                if (operandOffset >= 0) {
                                    adj = defSym.Value - operandOffset;
                                }
                                defSym.Xrefs.Add(
                                    new XrefSet.Xref(offset, true, xrefType, accType, adj));
                            }
                        } else if (SymbolTable.TryGetValue(dfd.SymbolRef.Label, out Symbol sym)) {
                            // Is this a reference to a project/platform symbol?
                            if (sym.SymbolSource == Symbol.Source.Project ||
                                    sym.SymbolSource == Symbol.Source.Platform) {
                                DefSymbol defSym = sym as DefSymbol;
                                int adj = 0;
                                if (operandOffset >= 0) {
                                    adj = defSym.Value - operandOffset;
                                }
                                defSym.Xrefs.Add(
                                    new XrefSet.Xref(offset, true, xrefType, accType, adj));
                            } else {
                                // Can get here if somebody creates an address operand symbol
                                // that refers to a local variable.
                                Debug.WriteLine("NOTE: not xrefing +" + offset.ToString("x6") +
                                    " " + sym);
                            }
                        }
                    } else if (dfd.FormatSubType == FormatDescriptor.SubType.Address) {
                        // not expecting this format on an instruction operand
                        Debug.Assert(attr.IsData || attr.IsInlineData);
                        int operandOffset = RawData.GetWord(mFileData, offset,
                            dfd.Length, dfd.FormatType == FormatDescriptor.Type.NumericBE);
                        AddXref(operandOffset,
                            new XrefSet.Xref(offset, false, xrefType, accType, 0));
                    }

                    // Look for instruction offset references.  We skip this if we've already
                    // added a reference from a symbol with zero adjustment, since that would
                    // just leave a duplicate entry.  (The symbolic ref wins because we need
                    // it for the label localizer and possibly the label refactorer.)
                    if (!hasZeroOffsetSym && attr.IsInstructionStart && attr.OperandOffset >= 0) {
                        AddXref(attr.OperandOffset,
                            new XrefSet.Xref(offset, false, xrefType, accType, 0));
                    }
                }

                if (attr.IsDataStart) {
                    // There shouldn't be data items inside of other data items.
                    offset += attr.Length;
                } else {
                    // Advance by one, not attr.Length, so we don't miss embedded instructions.
                    offset++;
                }
            }
        }

        /// <summary>
        /// Adds an Xref entry to an XrefSet.  The XrefSet will be created if necessary.
        /// </summary>
        /// <param name="offset">File offset for which cross-references are being noted.</param>
        /// <param name="xref">Cross reference to add to the set.</param>
        private void AddXref(int offset, XrefSet.Xref xref) {
            if (!mXrefs.TryGetValue(offset, out XrefSet xset)) {
                xset = mXrefs[offset] = new XrefSet();
            }
            xset.Add(xref);
        }

        /// <summary>
        /// Returns the XrefSet for the specified offset.  May return null if the set is
        /// empty.
        /// </summary>
        public XrefSet GetXrefSet(int offset) {
            mXrefs.TryGetValue(offset, out XrefSet xset);
            return xset;        // will be null if not found
        }

        /// <summary>
        /// Replaces generic auto-labels with fancier versions generated from xrefs.
        /// </summary>
        private void AnnotateAutoLabels() {
            AutoLabel.Style style = ProjectProps.AutoLabelStyle;
            Debug.Assert(style != AutoLabel.Style.Simple);

            for (int offset = 0; offset < mAnattribs.Length; offset++) {
                Anattrib attr = mAnattribs[offset];
                if (attr.Symbol != null && attr.Symbol.SymbolSource == Symbol.Source.Auto) {
                    XrefSet xset = GetXrefSet(offset);
                    if (xset == null) {
                        // Nothing useful to do here. This is unexpected, since auto-labels
                        // should only exist because something referenced the offset.
                        continue;
                    }
                    Symbol newSym =
                        AutoLabel.GenerateAnnotatedLabel(attr.Address, SymbolTable, xset, style);
                    if (!newSym.Equals(attr.Symbol)) {
                        //Debug.WriteLine("Replace " + attr.Symbol.Label + " with " +newSym.Label);

                        // Replace the symbol in Anattribs, update the symbol table, then
                        // call Refactor to update everything that referenced it.
                        Symbol oldSym = mAnattribs[offset].Symbol;
                        mAnattribs[offset].Symbol = newSym;
                        SymbolTable.Remove(oldSym);
                        SymbolTable.Add(newSym);
                        RefactorLabel(offset, oldSym.Label);
                    }
                }
            }
        }

        /// <summary>
        /// Generates the list of project/platform symbols that are being used.  Any
        /// DefSymbol with a non-empty Xrefs is included.  Previous contents are cleared.
        /// 
        /// The list is sorted primarily by value, secondarily by symbol name.
        /// 
        /// Call this after Xrefs are generated.
        /// </summary>
        private void GenerateActiveDefSymbolList() {
            ActiveDefSymbolList.Clear();

            foreach (Symbol sym in SymbolTable) {
                if (!(sym is DefSymbol) || sym.IsVariable) {
                    continue;
                }
                DefSymbol defSym = sym as DefSymbol;
                if (defSym.Xrefs.Count == 0) {
                    continue;
                }
                ActiveDefSymbolList.Add(defSym);
            }

            // We could make symbol source the primary sort key, so that all platform
            // symbols appear before all project symbols.  Not sure if that's better.
            //
            // Could also skip this by replacing the earlier foreach with a walk through
            // SymbolTable.mSymbolsByValue, but I'm not sure that should be exposed.
            ActiveDefSymbolList.Sort(delegate (DefSymbol a, DefSymbol b) {
                if (a.Value < b.Value) {
                    return -1;
                } else if (a.Value > b.Value) {
                    return 1;
                }
                return Asm65.Label.LABEL_COMPARER.Compare(a.Label, b.Label);
            });
        }

        #endregion Analysis


        #region Change Management

        /// <summary>
        /// Generates a ChangeSet that merges the FormatDescriptors in the new list into
        /// OperandFormats.
        /// 
        /// All existing descriptors that overlap with new descriptors will be removed.
        /// In cases where old and new descriptors have the same starting offset, this
        /// will be handled with a single change object.
        /// 
        /// If old and new descriptors are identical, no change object will be generated.
        /// It's possible for this to return an empty change set.
        /// </summary>
        /// <param name="newList">List of new format descriptors.</param>
        /// <returns>Change set.</returns>
        public ChangeSet GenerateFormatMergeSet(SortedList<int, FormatDescriptor> newList) {
            Debug.WriteLine("Generating format merge set...");
            ChangeSet cs = new ChangeSet(newList.Count * 2);

            // The Keys and Values properties are documented to return the internal data
            // structure, not make a copy, so this will be fast.
            IList<int> mainKeys = OperandFormats.Keys;
            IList<FormatDescriptor> mainValues = OperandFormats.Values;
            IList<int> newKeys = newList.Keys;
            IList<FormatDescriptor> newValues = newList.Values;

            // The basic idea is to walk through the new list, checking each entry for
            // conflicts with the main list.  If there's no conflict, we create a change
            // object for the new item.  If there is a conflict, we resolve it appropriately.
            //
            // The check on the main list is very fast because both lists are in sorted
            // order, so we can just walk the main list forward.  If a main-list entry
            // conflicts, we create a removal object, and advance the main index.
            int mainIndex = 0;
            int newIndex = 0;
            while (newIndex < newKeys.Count) {
                int newOffset = newKeys[newIndex];
                int newLength = newValues[newIndex].Length;
                if (mainIndex >= mainKeys.Count) {
                    // We've run off the end of the main list.  Just add the new item.
                    UndoableChange uc = UndoableChange.CreateActualOperandFormatChange(
                        newOffset, null, newValues[newIndex]);
                    cs.AddNonNull(uc);
                    newIndex++;
                    continue;
                }

                // Check for overlap by computing the intersection.  Start and end form two
                // points; the intersection is the largest of the start points and the
                // smallest of the end points.  If the result of the computation puts end before
                // start, there's no overlap.
                int mainOffset = mainKeys[mainIndex];
                int mainLength = mainValues[mainIndex].Length;
                Debug.Assert(newLength > 0 && mainLength > 0);
                int interStart = Math.Max(mainOffset, newOffset);
                int interEnd = Math.Min(mainOffset + mainLength, newOffset + newLength);
                // exclusive end point, so interEnd == interStart means no overlap
                if (interEnd > interStart) {
                    Debug.WriteLine("Found overlap: main(+" + mainOffset.ToString("x6") +
                        "," + mainLength + ") : new(+" + newOffset.ToString("x6") +
                        "," + newLength + ")");

                    // See if the initial offsets are identical.  If so, put the add and
                    // remove into a single change.  This isn't strictly necessary, but it's
                    // slightly more efficient.
                    if (mainOffset == newOffset) {
                        // Check to see if the descriptors are identical.  If so, ignore this.
                        if (mainValues[mainIndex] == newValues[newIndex]) {
                            Debug.WriteLine(" --> no-op change " + newValues[newIndex]);
                        } else {
                            Debug.WriteLine(" --> replace change " + newValues[newIndex]);
                            UndoableChange uc = UndoableChange.CreateActualOperandFormatChange(
                                newOffset, mainValues[mainIndex], newValues[newIndex]);
                            cs.AddNonNull(uc);
                        }
                    } else {
                        // Remove the old entry, add the new entry.
                        Debug.WriteLine(" --> remove/add change " + newValues[newIndex]);
                        UndoableChange ruc = UndoableChange.CreateActualOperandFormatChange(
                            mainOffset, mainValues[mainIndex], null);
                        UndoableChange auc = UndoableChange.CreateActualOperandFormatChange(
                            newOffset, null, newValues[newIndex]);
                        cs.AddNonNull(ruc);
                        cs.AddNonNull(auc);
                    }
                    newIndex++;

                    // Remove all other main-list entries that overlap with this one.
                    while (++mainIndex < mainKeys.Count) {
                        mainOffset = mainKeys[mainIndex];
                        mainLength = mainValues[mainIndex].Length;
                        interStart = Math.Max(mainOffset, newOffset);
                        interEnd = Math.Min(mainOffset + mainLength, newOffset + newLength);
                        // exclusive end point, so interEnd == interStart means no overlap
                        if (interEnd <= interStart) {
                            break;
                        }
                        Debug.WriteLine(" also remove +" + mainOffset.ToString("x6") +
                            mainValues[mainIndex]);
                        UndoableChange uc = UndoableChange.CreateActualOperandFormatChange(
                            mainOffset, mainValues[mainIndex], null);
                        cs.AddNonNull(uc);
                    }
                } else {
                    // No overlap.  If the main entry is earlier, we can cross it off the list
                    // and advance to the next one.  Otherwise, we add the change and advance
                    // that list.
                    if (mainOffset < newOffset) {
                        mainIndex++;
                    } else {
                        Debug.WriteLine("Add non-overlap " + newOffset.ToString("x6") +
                            newValues[newIndex]);
                        UndoableChange uc = UndoableChange.CreateActualOperandFormatChange(
                            newOffset, null, newValues[newIndex]);
                        cs.AddNonNull(uc);
                        newIndex++;
                    }
                }
            }

            // Trim away excess capacity, since this will probably be sitting in an undo
            // list for a long time.
            cs.TrimExcess();
            Debug.WriteLine("Total " + cs.Count + " changes");
            return cs;
        }

        /// <summary>
        /// Returns the analyzer attributes for the specified byte offset.
        /// 
        /// Bear in mind that Anattrib is a struct, and thus the return value is a copy.
        /// </summary>
        public Anattrib GetAnattrib(int offset) {
            return mAnattribs[offset];
        }

        /// <summary>
        /// Returns true if the offset has a long comment or note.  Used for determining how to
        /// split up a data area.  Currently not returning true for an end-of-line comment.
        /// </summary>
        /// <param name="offset">Offset of interest.</param>
        /// <returns>True if a comment or note was found.</returns>
        public bool HasCommentOrNote(int offset) {
            return (LongComments.ContainsKey(offset) ||
                    Notes.ContainsKey(offset));
        }

        /// <summary>
        /// True if an "undo" operation is available.
        /// </summary>
        public bool CanUndo { get { return mUndoTop > 0; } }

        /// <summary>
        /// True if a "redo" operation is available.
        /// </summary>
        public bool CanRedo { get { return mUndoTop < mUndoList.Count; } }

        /// <summary>
        /// True if something has changed since the last time the file was saved.
        /// </summary>
        public bool IsDirty { get { return mUndoTop != mUndoSaveIndex; } }

        /// <summary>
        /// Sets the save index equal to the undo position.  Do this after the file has
        /// been successfully saved.
        /// </summary>
        public void ResetDirtyFlag() {
            mUndoSaveIndex = mUndoTop;
        }

        /// <summary>
        /// Returns the next undo operation, and moves the pointer to the previous item.
        /// </summary>
        public ChangeSet PopUndoSet() {
            if (!CanUndo) {
                throw new Exception("Can't undo");
            }
            Debug.WriteLine("PopUndoSet: returning entry " + (mUndoTop - 1) + ": " +
                mUndoList[mUndoTop - 1]);
            return mUndoList[--mUndoTop];
        }

        /// <summary>
        /// Returns the next redo operation, and moves the pointer to the next item.
        /// </summary>
        /// <returns></returns>
        public ChangeSet PopRedoSet() {
            if (!CanRedo) {
                throw new Exception("Can't redo");
            }
            Debug.WriteLine("PopRedoSet: returning entry " + mUndoTop + ": " +
                mUndoList[mUndoTop]);
            return mUndoList[mUndoTop++];
        }

        /// <summary>
        /// Adds a change set to the undo list.  All redo operations above it on the
        /// stack are removed.
        /// 
        /// We currently allow empty sets.
        /// </summary>
        /// <param name="changeSet">Set to push.</param>
        public void PushChangeSet(ChangeSet changeSet) {
            Debug.WriteLine("PushChangeSet: adding " + changeSet);

            // Remove all of the "redo" entries from the current position to the end.
            if (mUndoTop < mUndoList.Count) {
                Debug.WriteLine("PushChangeSet: removing " + (mUndoList.Count - mUndoTop) +
                    " entries");
                mUndoList.RemoveRange(mUndoTop, mUndoList.Count - mUndoTop);
            }

            mUndoList.Add(changeSet);
            mUndoTop = mUndoList.Count;
        }

        public string DebugGetUndoRedoHistory() {
            StringBuilder sb = new StringBuilder();
            sb.Append("Bracketed change will be overwritten by next action\r\n\r\n");

            for (int i = 0; i < mUndoList.Count; i++) {
                ChangeSet cs = mUndoList[i];

                char lbr, rbr;
                if (i == mUndoTop) {
                    lbr = '[';
                    rbr = ']';
                } else {
                    lbr = rbr = ' ';
                }
                sb.AppendFormat("{0}{3,3:D}{1}{2}: {4} change{5}\r\n",
                    lbr, rbr, i == mUndoSaveIndex ? "*" : " ",
                    i, cs.Count, cs.Count == 1 ? "" : "s");

                for (int j = 0; j < cs.Count; j++) {
                    UndoableChange uc = cs[j];
                    sb.AppendFormat("       type={0} offset=+{1} reReq={2}\r\n",
                        uc.Type, uc.HasOffset ? uc.Offset.ToString("x6") : "N/A",
                        uc.ReanalysisRequired);
                }
            }
            if (mUndoTop == mUndoList.Count) {
                sb.AppendFormat("[ - ]{0}\r\n", mUndoTop == mUndoSaveIndex ? "*" : " ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Applies the changes to the project, and updates the display.
        /// </summary>
        /// <param name="cs">Set of changes to apply.</param>
        /// <param name="backward">If set, undo the changes instead.</param>
        /// <param name="affectedOffsets">List of offsets affected by change.  Only meaningful
        ///   when the result is not "None".</param>
        /// <returns>An indication of the level of reanalysis required.  If this returns None,
        ///   the list of offsets to update will be in affectedOffsets.</returns>
        public UndoableChange.ReanalysisScope ApplyChanges(ChangeSet cs, bool backward,
                out RangeSet affectedOffsets) {
            affectedOffsets = new RangeSet();

            UndoableChange.ReanalysisScope needReanalysis = UndoableChange.ReanalysisScope.None;

            // TODO(maybe): if changes overlap, we need to apply them in reverse order when
            //   "backward" is set.  This requires a reverse-order enumerator from
            //   ChangeSet.  Not currently needed.
            foreach (UndoableChange uc in cs) {
                object oldValue, newValue;

                // Unpack change, flipping old/new for undo.
                if (!backward) {
                    oldValue = uc.OldValue;
                    newValue = uc.NewValue;
                } else {
                    oldValue = uc.NewValue;
                    newValue = uc.OldValue;
                }
                int offset = uc.Offset;

                switch (uc.Type) {
                    case UndoableChange.ChangeType.Dummy:
                        //if (uc.ReanalysisRequired == UndoableChange.ReanalysisFlags.None) {
                        //    affectedOffsets.AddRange(0, FileData.Length - 1);
                        //}
                        break;
                    case UndoableChange.ChangeType.SetAddress: {
                            AddressMap addrMap = AddrMap;
                            if (addrMap.Get(offset) != (int)oldValue) {
                                Debug.WriteLine("GLITCH: old address value mismatch (" +
                                    addrMap.Get(offset) + " vs " + (int)oldValue + ")");
                                Debug.Assert(false);
                            }
                            addrMap.Set(offset, (int)newValue);

                            Debug.WriteLine("Map offset +" + offset.ToString("x6") + " to $" +
                                ((int)newValue).ToString("x6"));

                            // ignore affectedOffsets
                            Debug.Assert(uc.ReanalysisRequired ==
                                UndoableChange.ReanalysisScope.CodeAndData);
                        }
                        break;
                    case UndoableChange.ChangeType.SetTypeHint: {
                            // Always requires full code+data re-analysis.
                            ApplyTypeHints((TypedRangeSet)oldValue, (TypedRangeSet)newValue);
                            // ignore affectedOffsets
                            Debug.Assert(uc.ReanalysisRequired ==
                                UndoableChange.ReanalysisScope.CodeAndData);
                        }
                        break;
                    case UndoableChange.ChangeType.SetStatusFlagOverride: {
                            if (StatusFlagOverrides[offset] != (StatusFlags)oldValue) {
                                Debug.WriteLine("GLITCH: old status flag mismatch (" +
                                    StatusFlagOverrides[offset] + " vs " +
                                    (StatusFlags)oldValue + ")");
                                Debug.Assert(false);
                            }
                            StatusFlagOverrides[offset] = (StatusFlags)newValue;
                            // ignore affectedOffsets
                            Debug.Assert(uc.ReanalysisRequired ==
                                UndoableChange.ReanalysisScope.CodeAndData);
                        }
                        break;
                    case UndoableChange.ChangeType.SetLabel: {
                            // NOTE: this is about managing changes to UserLabels.  Adding
                            // or removing a user-defined label requires a full reanalysis,
                            // even if there was already an auto-generated label present,
                            // so we don't need to undo/redo Anattribs for anything except
                            // for renaming a user-defined label.
                            UserLabels.TryGetValue(offset, out Symbol oldSym);
                            if (oldSym != (Symbol) oldValue) {
                                Debug.WriteLine("GLITCH: old label value mismatch ('" +
                                    oldSym + "' vs '" + oldValue + "')");
                                Debug.Assert(false);
                            }

                            if (newValue == null) {
                                // We're removing a user label.
                                UserLabels.Remove(offset);
                                SymbolTable.Remove((Symbol)oldValue);   // unnecessary?
                                Debug.Assert(uc.ReanalysisRequired ==
                                    UndoableChange.ReanalysisScope.DataOnly);
                            } else {
                                // We're adding or renaming a user label.
                                //
                                // We should not be changing a label to the same value as an
                                // existing label -- the dialog should have prevented it.
                                // This is important because, if we edit a label to match an
                                // auto-generated label, we'll have a duplicate label unless we
                                // do a full code+data reanalysis.  If we're okay with reanalyzing
                                // on user-label renames, we can allow such conflicts.
                                if (oldValue != null) {
                                    SymbolTable.Remove((Symbol)oldValue);
                                }
                                UserLabels[offset] = (Symbol)newValue;
                                //SymbolTable[((Symbol)newValue).Label] = (Symbol)newValue;
                                SymbolTable.Add((Symbol)newValue);
                                Debug.Assert(oldSym != null || uc.ReanalysisRequired ==
                                    UndoableChange.ReanalysisScope.DataOnly);
                            }

                            if (uc.ReanalysisRequired == UndoableChange.ReanalysisScope.None) {
                                // Shouldn't really be "changing" from null to null, but
                                // it's legal, so don't blow up if it happens.
                                // (The assert on SymbolSource is older -- we now only care about
                                // what's in UserLabels, which are always Source=User.)
                                Debug.Assert((oldValue == null && newValue == null) ||
                                    (((Symbol)oldValue).SymbolSource == Symbol.Source.User &&
                                     ((Symbol)newValue).SymbolSource == Symbol.Source.User));
                                // Not doing a full refresh, so keep this up to date.
                                mAnattribs[offset].Symbol = (Symbol)newValue;

                                if (oldValue != null) {
                                    // Update everything in Anattribs and OperandFormats that
                                    // referenced the old symbol.
                                    RefactorLabel(offset, ((Symbol)oldValue).Label);
                                }

                                affectedOffsets.Add(offset);

                                // Use the cross-reference table to identify the offsets that
                                // we need to update.
                                if (mXrefs.TryGetValue(offset, out XrefSet xrefs)) {
                                    foreach (XrefSet.Xref xr in xrefs) {
                                        // This isn't quite right -- in theory we should be adding
                                        // all offsets that are part of the instruction, so that
                                        // affectedOffsets can hold a contiguous range instead of
                                        // a collection of opcode offsets.  In practice, for a
                                        // label change, it shouldn't matter.
                                        affectedOffsets.Add(xr.Offset);
                                    }
                                }
                            } else {
                                // We're not calling RefactorLabel() here because we should
                                // only be doing the reanalysis if we're adding or removing
                                // the label, not renaming it.  If that changes, we'll need
                                // to do the refactor here, though we can skip Anattribs work.
                                Debug.Assert(oldValue == null || newValue == null);
                            }
                        }
                        break;
                    case UndoableChange.ChangeType.SetOperandFormat: {
                            // Note this is used for data/inline-data as well as instructions.
                            OperandFormats.TryGetValue(offset, out FormatDescriptor current);
                            if (current != (FormatDescriptor)oldValue) {
                                Debug.WriteLine("GLITCH: old operand format mismatch (" +
                                    current + " vs " + oldValue + ")");
                                Debug.Assert(false);
                            }
                            if (newValue == null) {
                                OperandFormats.Remove(offset);
                                mAnattribs[offset].DataDescriptor = null;
                            } else {
                                OperandFormats[offset] = mAnattribs[offset].DataDescriptor =
                                    (FormatDescriptor)newValue;
                            }
                            if (uc.ReanalysisRequired == UndoableChange.ReanalysisScope.None) {
                                // Add every offset in the range.  The length might be changing
                                // (e.g. an offset with a single byte is now the start of a
                                // 10-byte string), so touch everything that was affected by
                                // the old descriptor or is affected by the new descriptor.
                                // [This may no longer be necessary -- size changes now
                                // require reanalysis.]
                                int afctLen = 1;
                                if (oldValue != null) {
                                    afctLen =
                                        Math.Max(afctLen, ((FormatDescriptor)oldValue).Length);
                                }
                                if (newValue != null) {
                                    afctLen =
                                        Math.Max(afctLen, ((FormatDescriptor)newValue).Length);
                                }

                                for (int i = offset; i < offset + afctLen; i++) {
                                    affectedOffsets.Add(i);
                                }
                            }
                        }
                        break;
                    case UndoableChange.ChangeType.SetComment: {
                            if (!Comments[offset].Equals(oldValue)) {
                                Debug.WriteLine("GLITCH: old comment value mismatch ('" +
                                    Comments[offset] + "' vs '" + oldValue + "')");
                                Debug.Assert(false);
                            }
                            Comments[offset] = (string)newValue;

                            // Only affects this offset.
                            affectedOffsets.Add(offset);
                        }
                        break;
                    case UndoableChange.ChangeType.SetLongComment: {
                            LongComments.TryGetValue(offset, out MultiLineComment current);
                            if (current != (MultiLineComment)oldValue) {
                                Debug.WriteLine("GLITCH: old long comment value mismatch ('" +
                                    current + "' vs '" + oldValue + "')");
                                Debug.Assert(false);
                            }
                            if (newValue == null) {
                                LongComments.Remove(offset);
                            } else {
                                LongComments[offset] = (MultiLineComment)newValue;
                            }

                            // Only affects this offset.
                            affectedOffsets.Add(offset);
                        }
                        break;
                    case UndoableChange.ChangeType.SetNote: {
                            Notes.TryGetValue(offset, out MultiLineComment current);
                            if (current != (MultiLineComment)oldValue) {
                                Debug.WriteLine("GLITCH: old note value mismatch ('" +
                                    current + "' vs '" + oldValue + "')");
                                Debug.Assert(false);
                            }
                            if (newValue == null) {
                                Notes.Remove(offset);
                            } else {
                                Notes[offset] = (MultiLineComment)newValue;
                            }

                            // Only affects this offset.
                            affectedOffsets.Add(offset);
                        }
                        break;
                    case UndoableChange.ChangeType.SetProjectProperties: {
                            bool needExternalFileReload = !CommonUtil.Container.StringListEquals(
                                ((ProjectProperties)oldValue).PlatformSymbolFileIdentifiers,
                                ((ProjectProperties)newValue).PlatformSymbolFileIdentifiers,
                                null /*StringComparer.InvariantCulture*/);
                            needExternalFileReload |= !CommonUtil.Container.StringListEquals(
                                ((ProjectProperties)oldValue).ExtensionScriptFileIdentifiers,
                                ((ProjectProperties)newValue).ExtensionScriptFileIdentifiers,
                                null);

                            // ProjectProperties are mutable, so create a new object that's
                            // a clone of the one that will live in the undo buffer.
                            ProjectProps = new ProjectProperties((ProjectProperties)newValue);

                            // Most of the properties are simply used during the reanalysis
                            // process.  This must be set explicitly.  NOTE: replacing this
                            // could cause cached data (such as Formatter strings) to be
                            // discarded, so ideally we wouldn't do this unless we know the
                            // CPU definition has changed (or we know that GetBestMatch is
                            // memoizing results and will return the same object).
                            Debug.WriteLine("Replacing CPU def object");
                            UpdateCpuDef();

                            if (needExternalFileReload) {
                                LoadExternalFiles();
                            }
                        }
                        // ignore affectedOffsets
                        Debug.Assert(uc.ReanalysisRequired ==
                            UndoableChange.ReanalysisScope.CodeAndData);
                        break;
                    case UndoableChange.ChangeType.SetLocalVariableTable: {
                            LvTables.TryGetValue(offset, out LocalVariableTable current);
                            if (current != (LocalVariableTable)oldValue) {
                                Debug.WriteLine("GLITCH: old lvt value mismatch: current=" +
                                    current + " old=" + (LocalVariableTable)oldValue);
                                Debug.Assert(false);
                            }
                            if (newValue == null) {
                                LvTables.Remove(offset);
                            } else {
                                LvTables[offset] = (LocalVariableTable)newValue;
                            }
                            // ignore affectedOffsets
                            Debug.Assert(uc.ReanalysisRequired ==
                                UndoableChange.ReanalysisScope.DataOnly);
                        }
                        break;
                    default:
                        break;
                }
                needReanalysis |= uc.ReanalysisRequired;
            }

            return needReanalysis;
        }

        /// <summary>
        /// Updates all symbolic references to the old label.  Call this after replacing
        /// mAnattribs[labelOffset].Symbol.
        /// </summary>
        /// <param name="labelOffset">Offset with the just-renamed label.</param>
        /// <param name="oldLabel">Previous value.</param>
        private void RefactorLabel(int labelOffset, string oldLabel) {
            if (!mXrefs.TryGetValue(labelOffset, out XrefSet xrefs)) {
                // This can happen if you add a label in a file section that nothing references,
                // and then rename it.
                Debug.WriteLine("RefactorLabel: no references to " + oldLabel);
                return;
            }

            string newLabel = mAnattribs[labelOffset].Symbol.Label;

            //
            // Update format descriptors in Anattribs.
            //
            foreach (XrefSet.Xref xr in xrefs) {
                FormatDescriptor dfd = mAnattribs[xr.Offset].DataDescriptor;
                if (dfd == null) {
                    // Should be a data target reference here?  Where'd the xref come from?
                    Debug.Assert(false);
                    continue;
                }
                if (!dfd.HasSymbol) {
                    // The auto-gen stuff would have created a symbol, but the user can
                    // override that and display as e.g. hex.
                    continue;
                }
                if (!Label.LABEL_COMPARER.Equals(oldLabel, dfd.SymbolRef.Label)) {
                    // This can happen if the xref is based on the operand offset,
                    // but the user picked a different symbol.  The xref generator
                    // creates entries for both target offsets, but only one will
                    // have a matching label.
                    continue;
                }

                mAnattribs[xr.Offset].DataDescriptor = FormatDescriptor.Create(
                    dfd.Length, new WeakSymbolRef(newLabel, dfd.SymbolRef.ValuePart),
                    dfd.FormatType == FormatDescriptor.Type.NumericBE);
            }

            //
            // Update value in OperandFormats.
            //
            foreach (XrefSet.Xref xr in xrefs) {
                if (!OperandFormats.TryGetValue(xr.Offset, out FormatDescriptor dfd)) {
                    // Probably an auto-generated symbol ref, so no entry in OperandFormats.
                    continue;
                }
                if (!dfd.HasSymbol) {
                    continue;
                }
                if (!Label.LABEL_COMPARER.Equals(oldLabel, dfd.SymbolRef.Label)) {
                    continue;
                }

                Debug.WriteLine("Replacing OpFor symbol at +" + xr.Offset.ToString("x6") +
                    " with " + newLabel);
                OperandFormats[xr.Offset] = FormatDescriptor.Create(
                    dfd.Length, new WeakSymbolRef(newLabel, dfd.SymbolRef.ValuePart),
                    dfd.FormatType == FormatDescriptor.Type.NumericBE);
            }
        }


        /// <summary>
        /// Applies the values in the set to the project hints.
        /// </summary>
        /// <param name="oldSet">Previous values; must match current contents.</param>
        /// <param name="newSet">Values to apply.</param>
        private void ApplyTypeHints(TypedRangeSet oldSet, TypedRangeSet newSet) {
            CodeAnalysis.TypeHint[] hints = TypeHints;
            foreach (TypedRangeSet.Tuple tuple in newSet) {
                CodeAnalysis.TypeHint curType = hints[tuple.Value];
                if (!oldSet.GetType(tuple.Value, out int oldType) || oldType != (int)curType) {
                    Debug.WriteLine("Type mismatch at " + tuple.Value);
                    Debug.Assert(false);
                }

                //Debug.WriteLine("Set +" + tuple.Value.ToString("x6") + " to " +
                //    (CodeAnalysis.TypeHint)tuple.Type + " (was " +
                //    curType + ")");

                hints[tuple.Value] = (CodeAnalysis.TypeHint)tuple.Type;
            }
        }

        #endregion Change Management

        /// <summary>
        /// Finds a label by name.  SymbolTable must be populated.
        /// </summary>
        /// <param name="name">Label to find.</param>
        /// <returns>File offset associated with label, or -1 if not found.</returns>
        public int FindLabelOffsetByName(string name) {
            // We're interested in user labels and auto-generated labels.  Do a lookup in
            // SymbolTable to find the symbol, then if it's user or auto, we do a second
            // search to find the file offset it's associated with.  The second search
            // requires a linear walk through anattribs; if we do this often we'll want to
            // maintain a symbol-to-offset structure.
            //
            // This will not find "hidden" labels, i.e. labels that are in the middle of an
            // instruction or multi-byte data area, because those are removed from SymbolTable.

            if (!SymbolTable.TryGetValue(name, out Symbol sym)) {
                return -1;
            }
            if (!sym.IsInternalLabel) {
                return -1;
            }
            for (int i = 0; i < mAnattribs.Length; i++) {
                if (mAnattribs[i].Symbol == sym) {
                    return i;
                }
            }
            Debug.WriteLine("NOTE: symbol '" + name + "' exists, but wasn't found in labels");
            return -1;
        }

        /// <summary>
        /// For debugging purposes, get some information about the currently loaded
        /// extension scripts.
        /// </summary>
        public string DebugGetLoadedScriptInfo() {
            return mScriptManager.DebugGetLoadedScriptInfo();
        }
    }
}
