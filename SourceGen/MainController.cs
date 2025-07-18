﻿/*
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

using Asm65;
using CommonUtil;
using CommonWPF;
using SourceGen.Sandbox;
using SourceGen.WpfGui;

namespace SourceGen {
    /// <summary>
    /// This class manages user interaction.  The goal is for this to be relatively
    /// GUI-toolkit-agnostic, with all the WPF stuff tucked into the code-behind files.  An
    /// instance of this class is created by MainWindow when the app starts.
    /// 
    /// There is some Windows-specific stuff, like MessageBox and OpenFileDialog.
    /// </summary>
    public class MainController {
        private const string SETTINGS_FILE_NAME = "SourceGen-settings";

        #region Project state

        // Currently open project, or null if none.
        private DisasmProject mProject;

        // Pathname to 65xx data file.
        private string mDataPathName;

        // Pathname of .dis65 file.  This will be empty for a new project.
        private string mProjectPathName;

        /// <summary>
        /// Data backing the code list.  Will be null if the project is not open.
        /// </summary>
        public LineListGen CodeLineList { get; private set; }

        #endregion Project state

        /// <summary>
        /// Reference back to MainWindow object.
        /// </summary>
        private MainWindow mMainWin;

        /// <summary>
        /// Hex dump viewer window.  This is used for the currently open project.
        /// </summary>
        private Tools.WpfGui.HexDumpViewer mHexDumpDialog;

        // Debug windows.
        private Tools.WpfGui.ShowText mShowAnalysisTimersDialog;
        public bool IsDebugAnalysisTimersOpen { get { return mShowAnalysisTimersDialog != null; } }
        private Tools.WpfGui.ShowText mShowAnalyzerOutputDialog;
        public bool IsDebugAnalyzerOutputOpen { get { return mShowAnalyzerOutputDialog != null; } }
        private Tools.WpfGui.ShowText mShowUndoRedoHistoryDialog;
        public bool IsDebugUndoRedoHistoryOpen { get { return mShowUndoRedoHistoryDialog != null; } }

        /// <summary>
        /// This holds any un-owned Windows that we don't otherwise track.  It's used for
        /// hex dump windows of arbitrary files.  We need to close them when the main window
        /// is closed.
        /// </summary>
        private List<Window> mUnownedWindows = new List<Window>();

        /// <summary>
        /// ASCII chart reference window.  Not tied to the project.
        /// </summary>
        private Tools.WpfGui.AsciiChart mAsciiChartDialog;

        /// <summary>
        /// Returns true if the ASCII chart window is currently open.
        /// </summary>
        public bool IsAsciiChartOpen { get { return mAsciiChartDialog != null; } }

        /// <summary>
        /// Apple II screen chart window.  Not tied to the project.
        /// </summary>
        private Tools.WpfGui.Apple2ScreenChart mApple2ScreenChartDialog;

        /// <summary>
        /// Returns true if the ASCII chart window is currently open.
        /// </summary>
        public bool IsApple2ScreenChartOpen { get { return mApple2ScreenChartDialog != null; } }

        /// <summary>
        /// Instruction chart reference window.  Not tied to the project.
        /// </summary>
        private Tools.WpfGui.InstructionChart mInstructionChartDialog;

        /// <summary>
        /// Returns true if the instruction chart window is currently open.
        /// </summary>
        public bool IsInstructionChartOpen { get { return mInstructionChartDialog != null; } }

        /// <summary>
        /// Reference table window.  Tied to the current project.
        /// </summary>
        private ReferenceTable mReferenceTableDialog;

        /// <summary>
        /// List of recently-opened projects.
        /// </summary>
        public List<string> RecentProjectPaths = new List<string>(MAX_RECENT_PROJECTS);
        public const int MAX_RECENT_PROJECTS = 6;

        /// <summary>
        /// Analyzed selection state, updated whenever the selection changes.
        /// </summary>
        public SelectionState SelectionAnalysis { get; set; }

        /// <summary>
        /// Activity log generated by the code and data analyzers.  Displayed in window.
        /// </summary>
        private DebugLog mGenerationLog;

        /// <summary>
        /// Timing data generated during analysis.
        /// </summary>
        TaskTimer mReanalysisTimer = new TaskTimer();

        /// <summary>
        /// Stack for navigate forward/backward.
        /// </summary>
        private NavStack mNavStack = new NavStack();

        /// <summary>
        /// Output format configuration.
        /// </summary>
        private Formatter.FormatConfig mFormatterConfig;

        /// <summary>
        /// Output format controller.
        /// 
        /// This is shared with the DisplayList.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// Pseudo-op names.
        /// 
        /// This is shared with the DisplayList.
        /// </summary>
        private PseudoOp.PseudoOpNames mPseudoOpNames;

        /// <summary>
        /// String we most recently searched for.
        /// </summary>
        private string mFindString = string.Empty;

        /// <summary>
        /// Initial start point of most recent search.
        /// </summary>
        private int mFindStartIndex = -1;

        /// <summary>
        /// True if previous search was backward, so we can tell if we changed direction
        /// (otherwise we think we immediately wrapped around and the search stops).
        /// </summary>
        private bool mFindBackward = false;

        /// <summary>
        /// Used to highlight the line that is the target of the selected line.
        /// </summary>
        private int mTargetHighlightIndex = -1;

        /// <summary>
        /// Tracks the operands we have highlighted.
        /// </summary>
        private List<int> mOperandHighlights = new List<int>();

        /// <summary>
        /// Code list color scheme.
        /// </summary>
        private MainWindow.ColorScheme mColorScheme = MainWindow.ColorScheme.Light;

        /// <summary>
        /// CPU definition used when the Formatter was created.  If the CPU choice or
        /// inclusion of undocumented opcodes changes, we need to wipe the formatter.
        /// </summary>
        private CpuDef mFormatterCpuDef;

        /// <summary>
        /// Instruction description object.  Used for Info window.
        /// </summary>
        private OpDescription mOpDesc = OpDescription.GetOpDescription(null);

        /// <summary>
        /// If true, plugins will execute in the main application's AppDomain instead of
        /// the sandbox (effectively disabling the security features).
        /// </summary>
        public bool UseMainAppDomainForPlugins { get; private set; }

        /// <summary>
        /// Code list column numbers.
        /// </summary>
        public enum CodeListColumn {
            Offset = 0, Address, Bytes, Flags, Attributes, Label, Opcode, Operand, Comment,
            COUNT       // must be last; must equal number of columns
        }

        /// <summary>
        /// Clipboard format enumeration.
        /// </summary>
        public enum ClipLineFormat {
            Unknown = -1,
            AssemblerSource = 0,
            Disassembly = 1,
            AllColumns = 2
        }

        /// <summary>
        /// True if a project is open and AnalyzeUncategorizedData is enabled.
        /// </summary>
        public bool IsAnalyzeUncategorizedDataEnabled {
            get {
                if (mProject == null) {
                    return false;
                }
                return mProject.ProjectProps.AnalysisParams.AnalyzeUncategorizedData;
            }
        }

        #region Init and settings

        /// <summary>
        /// Constructor, called from the main window code.
        /// </summary>
        public MainController(MainWindow win) {
            mMainWin = win;

            CreateAutoSaveTimer();

            ScriptManager.UseKeepAliveHack = true;
        }

        /// <summary>
        /// Early initialization, before the window is visible.  Notably, we want to get the
        /// window placement data, so we can position and size the window before it's first
        /// drawn (avoids a blink).
        /// </summary>
        public void WindowSourceInitialized() {
            // Load the settings from the file.  If this fails we have no way to tell the user,
            // so just keep going.
            LoadAppSettings();
            SetAppWindowLocation();     // <-- this causes WindowLoaded to fire
        }

        /// <summary>
        /// Perform one-time initialization after the Window has finished loading.  We defer
        /// to this point so we can report fatal errors directly to the user.
        /// </summary>
        public void WindowLoaded() {
            // Run library unit tests.
            Debug.Assert(CommonUtil.AddressMap.Test());
            Debug.Assert(CommonUtil.RangeSet.Test());
            Debug.Assert(CommonUtil.TypedRangeSet.Test());
            Debug.Assert(CommonUtil.Version.Test());
            Debug.Assert(Asm65.CpuDef.DebugValidate());

            if (RuntimeDataAccess.GetDirectory() == null) {
                MessageBox.Show(Res.Strings.RUNTIME_DIR_NOT_FOUND,
                    Res.Strings.RUNTIME_DIR_NOT_FOUND_CAPTION,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }
            try {
                PluginDllCache.PreparePluginDir();
            } catch (Exception ex) {
                string pluginPath = PluginDllCache.GetPluginDirPath();
                if (pluginPath == null) {
                    pluginPath = "<???>";
                }
                string msg = string.Format(Res.Strings.PLUGIN_DIR_FAIL_FMT,
                    pluginPath + ": " + ex.Message);
                MessageBox.Show(msg, Res.Strings.PLUGIN_DIR_FAIL_CAPTION,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            // Place the main window and apply the various settings.
            ApplyAppSettings();

            UpdateTitle();
            mMainWin.UpdateRecentLinks();

            ProcessCommandLine();

            // Create an initial value.
            SelectionAnalysis = UpdateSelectionState();
        }

        private void ProcessCommandLine() {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2) {
                DoOpenFile(Path.GetFullPath(args[1]));
            }
        }


        /// <summary>
        /// Loads settings from the settings file into AppSettings.Global.  Does not apply
        /// them to the ProjectView.
        /// </summary>
        private void LoadAppSettings() {
            AppSettings settings = AppSettings.Global;

            // Set some default settings for first-time use.  The general rule is to set
            // a default value of false, 0, or the empty string, so we only need to set
            // values here when that isn't the case.  The point at which the setting is
            // actually used is expected to do something reasonable by default.

            settings.SetInt(AppSettings.PROJ_AUTO_SAVE_INTERVAL, 60);   // enabled by default

            settings.SetBool(AppSettings.SYMWIN_SHOW_USER, true);
            settings.SetBool(AppSettings.SYMWIN_SHOW_NON_UNIQUE, false);
            settings.SetBool(AppSettings.SYMWIN_SHOW_PROJECT, true);
            settings.SetBool(AppSettings.SYMWIN_SHOW_PLATFORM, false);
            settings.SetBool(AppSettings.SYMWIN_SHOW_ADDR_PRE_LABELS, true);
            settings.SetBool(AppSettings.SYMWIN_SHOW_AUTO, false);
            settings.SetBool(AppSettings.SYMWIN_SHOW_ADDR, true);
            settings.SetBool(AppSettings.SYMWIN_SHOW_CONST, true);
            settings.SetBool(AppSettings.SYMWIN_SORT_ASCENDING, true);
            settings.SetInt(AppSettings.SYMWIN_SORT_COL, (int)Symbol.SymbolSortField.Name);

            settings.SetBool(AppSettings.FMT_UPPER_OPERAND_A, true);
            settings.SetBool(AppSettings.FMT_UPPER_OPERAND_S, true);
            settings.SetBool(AppSettings.FMT_ADD_SPACE_FULL_COMMENT, true);
            settings.SetBool(AppSettings.FMT_SPACES_BETWEEN_BYTES, true);
            settings.SetString(AppSettings.FMT_OPCODE_SUFFIX_LONG, "l");
            settings.SetString(AppSettings.FMT_OPERAND_PREFIX_ABS, "a:");
            settings.SetString(AppSettings.FMT_OPERAND_PREFIX_LONG, "f:");

            settings.SetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, true);
            settings.SetEnum(AppSettings.SRCGEN_LABEL_NEW_LINE,
                AsmGen.GenCommon.LabelPlacement.SplitIfTooLong);

#if DEBUG
            settings.SetBool(AppSettings.DEBUG_MENU_ENABLED, true);
#else
            settings.SetBool(AppSettings.DEBUG_MENU_ENABLED, false);
#endif

            // Make sure we have entries for these.
            settings.SetString(AppSettings.CDLV_FONT_FAMILY,
                mMainWin.CodeListFontFamily.ToString());
            settings.SetInt(AppSettings.CDLV_FONT_SIZE, (int)mMainWin.CodeListFontSize);

            // Character and string delimiters.
            Formatter.DelimiterSet chrDel = Formatter.DelimiterSet.GetDefaultCharDelimiters();
            string chrSer = chrDel.Serialize();
            settings.SetString(AppSettings.FMT_CHAR_DELIM, chrSer);

            Formatter.DelimiterSet strDel = Formatter.DelimiterSet.GetDefaultStringDelimiters();
            string strSer = strDel.Serialize();
            settings.SetString(AppSettings.FMT_STRING_DELIM, strSer);


            // Load the settings file, and merge it into the globals.
            string runtimeDataDir = RuntimeDataAccess.GetDirectory();
            if (runtimeDataDir == null) {
                Debug.WriteLine("Unable to load settings file");
                return;
            }
            string settingsDir = Path.GetDirectoryName(runtimeDataDir);
            string settingsPath = Path.Combine(settingsDir, SETTINGS_FILE_NAME);
            try {
                string text = File.ReadAllText(settingsPath);
                AppSettings fileSettings = AppSettings.Deserialize(text);
                AppSettings.Global.MergeSettings(fileSettings);
                Debug.WriteLine("Settings file loaded and merged");
            } catch (Exception ex) {
                Debug.WriteLine("Unable to read settings file: " + ex.Message);
            }
        }

        /// <summary>
        /// Saves AppSettings to a file.
        /// </summary>
        private void SaveAppSettings() {
            if (!AppSettings.Global.Dirty) {
                Debug.WriteLine("Settings not dirty, not saving");
                return;
            }

            // Main window position and size.
            AppSettings.Global.SetString(AppSettings.MAIN_WINDOW_PLACEMENT,
                mMainWin.GetPlacement());

            // Horizontal splitters.
            AppSettings.Global.SetInt(AppSettings.MAIN_LEFT_PANEL_WIDTH,
                (int)mMainWin.LeftPanelWidth);
            AppSettings.Global.SetInt(AppSettings.MAIN_RIGHT_PANEL_WIDTH,
                (int)mMainWin.RightPanelWidth);

            // Vertical splitters.
            //AppSettings.Global.SetInt(AppSettings.MAIN_REFERENCES_HEIGHT,
            //    (int)mMainWin.ReferencesPanelHeight);
            //AppSettings.Global.SetInt(AppSettings.MAIN_SYMBOLS_HEIGHT,
            //    (int)mMainWin.SymbolsPanelHeight);

            // Something peculiar happens when we switch from the launch window to the
            // code list: the refs/notes splitter and sym/info splitter shift down a pixel.
            // Closing the project causes everything to shift back.  I'm not sure what's
            // causing the layout to change.  I'm working around the issue by not saving the
            // splitter positions if they've only moved 1 pixel.
            // TODO: fix this properly
            int refSetting = AppSettings.Global.GetInt(AppSettings.MAIN_REFERENCES_HEIGHT, -1);
            if ((int)mMainWin.ReferencesPanelHeight == refSetting ||
                    (int)mMainWin.ReferencesPanelHeight == refSetting - 1) {
                Debug.WriteLine("NOT updating references height");
            } else {
                AppSettings.Global.SetInt(AppSettings.MAIN_REFERENCES_HEIGHT,
                    (int)mMainWin.ReferencesPanelHeight);
            }
            int symSetting = AppSettings.Global.GetInt(AppSettings.MAIN_SYMBOLS_HEIGHT, -1);
            if ((int)mMainWin.SymbolsPanelHeight == symSetting ||
                    (int)mMainWin.SymbolsPanelHeight == symSetting - 1) {
                Debug.WriteLine("NOT updating symbols height");
            } else {
                AppSettings.Global.SetInt(AppSettings.MAIN_SYMBOLS_HEIGHT,
                    (int)mMainWin.SymbolsPanelHeight);
            }

            mMainWin.CaptureColumnWidths();

            string runtimeDataDir = RuntimeDataAccess.GetDirectory();
            if (runtimeDataDir == null) {
                Debug.WriteLine("Unable to save settings file");
                return;
            }
            string settingsDir = Path.GetDirectoryName(runtimeDataDir);
            string settingsPath = Path.Combine(settingsDir, SETTINGS_FILE_NAME);
            try {
                string cereal = AppSettings.Global.Serialize();
                File.WriteAllText(settingsPath, cereal);
                AppSettings.Global.Dirty = false;
                Debug.WriteLine("Saved settings (" + settingsPath + ")");
            } catch (Exception ex) {
                Debug.WriteLine("Failed to save settings: " + ex.Message);
            }
        }

        /// <summary>
        /// Sets the app window's location and size.  This should be called before the window has
        /// finished initialization.
        /// </summary>
        private void SetAppWindowLocation() {
            const int DEFAULT_SPLIT = 250;

            AppSettings settings = AppSettings.Global;

            string placement = settings.GetString(AppSettings.MAIN_WINDOW_PLACEMENT, null);
            if (placement != null) {
                mMainWin.SetPlacement(placement);
            }

            mMainWin.LeftPanelWidth =
                settings.GetInt(AppSettings.MAIN_LEFT_PANEL_WIDTH, DEFAULT_SPLIT);
            mMainWin.RightPanelWidth =
                settings.GetInt(AppSettings.MAIN_RIGHT_PANEL_WIDTH, DEFAULT_SPLIT);
            mMainWin.ReferencesPanelHeight =
                settings.GetInt(AppSettings.MAIN_REFERENCES_HEIGHT, 350);
            mMainWin.SymbolsPanelHeight =
                settings.GetInt(AppSettings.MAIN_SYMBOLS_HEIGHT, 400);

            mMainWin.RestoreColumnWidths();
        }

        /// <summary>
        /// Applies "actionable" settings to the ProjectView, pulling them out of the global
        /// settings object.  If a project is open, refreshes the display list and all sub-windows.
        /// </summary>
        public void ApplyAppSettings() {
            Debug.WriteLine("ApplyAppSettings...");
            AppSettings settings = AppSettings.Global;

            // Set up the formatter with default values.
            mFormatterConfig = new Formatter.FormatConfig();
            AsmGen.GenCommon.ConfigureFormatterFromSettings(AppSettings.Global,
                ref mFormatterConfig);
            mFormatterConfig.EndOfLineCommentDelimiter = ";";

            mFormatterConfig.NonUniqueLabelPrefix =
                settings.GetString(AppSettings.FMT_NON_UNIQUE_LABEL_PREFIX, string.Empty);
            mFormatterConfig.LocalVariableLabelPrefix =
                settings.GetString(AppSettings.FMT_LOCAL_VARIABLE_PREFIX, string.Empty);
            mFormatterConfig.CommaSeparatedDense =
                settings.GetBool(AppSettings.FMT_COMMA_SEP_BULK_DATA, true);
            mFormatterConfig.SuppressImpliedAcc =
                settings.GetBool(AppSettings.SRCGEN_OMIT_IMPLIED_ACC_OPERAND, false);
            mFormatterConfig.DebugLongComments = DebugLongComments;

            string chrDelCereal = settings.GetString(AppSettings.FMT_CHAR_DELIM, null);
            if (chrDelCereal != null) {
                mFormatterConfig.CharDelimiters =
                    Formatter.DelimiterSet.Deserialize(chrDelCereal);
            }
            string strDelCereal = settings.GetString(AppSettings.FMT_STRING_DELIM, null);
            if (strDelCereal != null) {
                mFormatterConfig.StringDelimiters =
                    Formatter.DelimiterSet.Deserialize(strDelCereal);
            }


            // Update the formatter, and null out mFormatterCpuDef to force a refresh
            // of related items.
            mFormatter = new Formatter(mFormatterConfig);
            mFormatterCpuDef = null;

            // Set pseudo-op names.  Entries aren't allowed to be blank, so we start with the
            // default values and merge in whatever the user has configured.
            mPseudoOpNames = PseudoOp.DefaultPseudoOpNames;
            string pseudoCereal = settings.GetString(AppSettings.FMT_PSEUDO_OP_NAMES, null);
            if (!string.IsNullOrEmpty(pseudoCereal)) {
                PseudoOp.PseudoOpNames deser = PseudoOp.PseudoOpNames.Deserialize(pseudoCereal);
                if (deser != null) {
                    mPseudoOpNames = PseudoOp.PseudoOpNames.Merge(mPseudoOpNames, deser);
                }
            }

            // Configure the Symbols window.
            mMainWin.SymFilterUserLabels =
                settings.GetBool(AppSettings.SYMWIN_SHOW_USER, false);
            mMainWin.SymFilterNonUniqueLabels =
                settings.GetBool(AppSettings.SYMWIN_SHOW_NON_UNIQUE, false);
            mMainWin.SymFilterAutoLabels =
                settings.GetBool(AppSettings.SYMWIN_SHOW_AUTO, false);
            mMainWin.SymFilterProjectSymbols =
                settings.GetBool(AppSettings.SYMWIN_SHOW_PROJECT, false);
            mMainWin.SymFilterPlatformSymbols =
                settings.GetBool(AppSettings.SYMWIN_SHOW_PLATFORM, false);
            mMainWin.SymFilterAddrPreLabels =
                settings.GetBool(AppSettings.SYMWIN_SHOW_ADDR_PRE_LABELS, false);
            mMainWin.SymFilterConstants =
                settings.GetBool(AppSettings.SYMWIN_SHOW_CONST, false);
            mMainWin.SymFilterAddresses =
                settings.GetBool(AppSettings.SYMWIN_SHOW_ADDR, false);

            // Get the configured font info.  If nothing is configured, use whatever the
            // code list happens to be using now.
            string fontFamilyName = settings.GetString(AppSettings.CDLV_FONT_FAMILY, null);
            if (fontFamilyName == null) {
                fontFamilyName = mMainWin.CodeListFontFamily.ToString();
            }
            int size = settings.GetInt(AppSettings.CDLV_FONT_SIZE, -1);
            if (size <= 0) {
                size = (int)mMainWin.CodeListFontSize;
            }

            mMainWin.SetCodeListFont(fontFamilyName, size);

            // Update the column widths.  This was done earlier during init, but may need to be
            // repeated if the show/hide buttons were used in Settings.
            mMainWin.RestoreColumnWidths();

            // Unpack the recent-project list.
            UnpackRecentProjectList();

            // Set the color scheme.
            bool useDark = settings.GetBool(AppSettings.SKIN_DARK_COLOR_SCHEME, false);
            if (useDark) {
                mColorScheme = MainWindow.ColorScheme.Dark;
            } else {
                mColorScheme = MainWindow.ColorScheme.Light;
            }
            mMainWin.SetColorScheme(mColorScheme);
            if (CodeLineList != null) {
                SetCodeLineListColorMultiplier();
            }

            // Enable the DEBUG menu if configured.
            mMainWin.ShowDebugMenu =
                AppSettings.Global.GetBool(AppSettings.DEBUG_MENU_ENABLED, false);

            // Refresh the toolbar checkbox.
            mMainWin.DoShowCycleCounts =
                AppSettings.Global.GetBool(AppSettings.FMT_SHOW_CYCLE_COUNTS, false);

            // Update the display list generator with all the fancy settings.
            if (CodeLineList != null) {
                // Regenerate the display list with the latest formatter config and
                // pseudo-op definition.  (These are set as part of the refresh.)
                UndoableChange uc =
                    UndoableChange.CreateDummyChange(UndoableChange.ReanalysisScope.DisplayOnly);
                ApplyChanges(new ChangeSet(uc), false);
            }

            // If auto-save was enabled or disabled, create or remove the recovery file.
            RefreshRecoveryFile();
        }

        private void SetCodeLineListColorMultiplier() {
            if (mColorScheme == MainWindow.ColorScheme.Dark) {
                CodeLineList.NoteColorMultiplier = 0.6f;
            } else {
                CodeLineList.NoteColorMultiplier = 1.0f;
            }
        }

        private void UnpackRecentProjectList() {
            RecentProjectPaths.Clear();

            string cereal = AppSettings.Global.GetString(
                AppSettings.PRVW_RECENT_PROJECT_LIST, null);
            if (string.IsNullOrEmpty(cereal)) {
                return;
            }

            try {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                RecentProjectPaths = ser.Deserialize<List<string>>(cereal);
            } catch (Exception ex) {
                Debug.WriteLine("Failed deserializing recent projects: " + ex.Message);
                return;
            }
        }

        /// <summary>
        /// Ensures that the named project is at the top of the list.  If it's elsewhere
        /// in the list, move it to the top.  Excess items are removed.
        /// </summary>
        /// <param name="projectPath"></param>
        private void UpdateRecentProjectList(string projectPath) {
            if (string.IsNullOrEmpty(projectPath)) {
                // This can happen if you create a new project, then close the window
                // without having saved it.
                return;
            }
            int index = RecentProjectPaths.IndexOf(projectPath);
            if (index == 0) {
                // Already in the list, nothing changes.  No need to update anything else.
                return;
            }
            if (index > 0) {
                RecentProjectPaths.RemoveAt(index);
            }
            RecentProjectPaths.Insert(0, projectPath);

            // Trim the list to the max allowed.
            while (RecentProjectPaths.Count > MAX_RECENT_PROJECTS) {
                Debug.WriteLine("Recent projects: dropping " +
                    RecentProjectPaths[MAX_RECENT_PROJECTS]);
                RecentProjectPaths.RemoveAt(MAX_RECENT_PROJECTS);
            }

            // Store updated list in app settings.  JSON-in-JSON is ugly and inefficient,
            // but it'll do for now.
            JavaScriptSerializer ser = new JavaScriptSerializer();
            string cereal = ser.Serialize(RecentProjectPaths);
            AppSettings.Global.SetString(AppSettings.PRVW_RECENT_PROJECT_LIST, cereal);

            mMainWin.UpdateRecentLinks();
        }

        /// <summary>
        /// Updates the main form title to show project name and modification status.
        /// </summary>
        private void UpdateTitle() {
            // Update main window title.
            StringBuilder sb = new StringBuilder();
            if (mProject != null) {
                if (string.IsNullOrEmpty(mProjectPathName)) {
                    sb.Append(Res.Strings.TITLE_NEW_PROJECT);
                } else {
                    sb.Append(Path.GetFileName(mProjectPathName));
                }
                if (mProject.IsReadOnly) {
                    sb.Append(" ");
                    sb.Append(Res.Strings.TITLE_READ_ONLY);
                }
                sb.Append(" - ");
            }

            sb.Append(Res.Strings.TITLE_BASE);

            if (mProject != null && mProject.IsDirty) {
                sb.Append(" - ");
                sb.Append(Res.Strings.TITLE_MODIFIED);
            }
            mMainWin.Title = sb.ToString();

            UpdateByteCounts();
        }

        /// <summary>
        /// Updates the code/data/junk percentages in the status bar.
        /// </summary>
        private void UpdateByteCounts() {
            if (mProject == null) {
                mMainWin.ByteCountText = string.Empty;
                return;
            }

            Debug.Assert(mProject.ByteCounts.CodeByteCount + mProject.ByteCounts.DataByteCount +
                mProject.ByteCounts.JunkByteCount == mProject.FileData.Length);

            int total = mProject.FileData.Length;
            float codePerc = (mProject.ByteCounts.CodeByteCount * 100.0f) / total;
            float dataPerc = (mProject.ByteCounts.DataByteCount * 100.0f) / total;
            float junkPerc = (mProject.ByteCounts.JunkByteCount * 100.0f) / total;
            mMainWin.ByteCountText = string.Format(Res.Strings.STATUS_BYTE_COUNT_FMT,
                total / 1024.0f, codePerc, dataPerc, junkPerc);
        }

        #endregion Init and settings

        #region Auto-save

        private const string RECOVERY_EXT_ADD = "_rec";
        private const string RECOVERY_EXT = ProjectFile.FILENAME_EXT + RECOVERY_EXT_ADD;

        private string mRecoveryPathName = string.Empty;    // path to recovery file, or empty str
        private Stream mRecoveryStream = null;              // stream for recovery file, or null

        private DispatcherTimer mAutoSaveTimer = null;      // auto-save timer, may be disabled
        private DateTime mLastEditWhen = DateTime.Now;      // timestamp of last user edit
        private DateTime mLastAutoSaveWhen = DateTime.Now;  // timestamp of last auto-save

        private bool mAutoSaveDeferred = false;


        /// <summary>
        /// Creates an interval timer that fires an event on the GUI thread.
        /// </summary>
        private void CreateAutoSaveTimer() {
            mAutoSaveTimer = new DispatcherTimer();
            mAutoSaveTimer.Tick += new EventHandler(AutoSaveTick);
            mAutoSaveTimer.Interval = TimeSpan.FromSeconds(30); // place-holder, overwritten later
        }

        /// <summary>
        /// Resets the auto-save timer to the configured interval.  Has no effect if the timer
        /// isn't currently running.
        /// </summary>
        private void ResetAutoSaveTimer() {
            if (mAutoSaveTimer.IsEnabled) {
                // Setting the Interval resets the timer.
                mAutoSaveTimer.Interval = mAutoSaveTimer.Interval;
            }
        }

        /// <summary>
        /// Handles the auto-save timer event.
        /// </summary>
        /// <remarks>
        /// We're using a DispatcherTimer, which appears to execute as part of the dispatcher,
        /// not a System.Timers.Timer thread, which runs asynchronously.  So not only do we not
        /// have to worry about SynchronizationObjects, it seems likely that this won't fire
        /// after the timer is disabled.
        /// </remarks>
        private void AutoSaveTick(object sender, EventArgs e) {
            try {
                if (mRecoveryStream == null) {
                    Debug.WriteLine("AutoSave tick: no recovery file");
                    return;
                }
                if (mLastEditWhen <= mLastAutoSaveWhen) {
                    Debug.WriteLine("AutoSave tick: recovery file is current (edit at " +
                        mLastEditWhen + ", auto-save at " + mLastAutoSaveWhen + ")");
                    return;
                }
                if (!mProject.IsDirty) {
                    // This may seem off, because of the following scenario: open a file, make a
                    // single edit, wait for auto-save, then hit Undo.  Changes have been made,
                    // but the project is now back to its original form, so IsDirty is false.  If
                    // we don't auto-save now, the recovery file will have a newer modification
                    // date than the project file, but will be stale.
                    //
                    // Technically, we don't need to update the recovery file, because the base
                    // project file has the correct and complete project.  There's no real need
                    // for us to save another copy.  If we crash, we'll have a stale recovery file
                    // with a newer timestamp, but we could handle that by back-dating the file
                    // timestamp or simply by truncating the recovery stream.
                    //
                    // The real reason for this test is that we don't want to auto-save if the
                    // user is being good about manual saves.
                    if (mRecoveryStream.Length != 0) {
                        Debug.WriteLine("AutoSave tick: project not dirty, truncating recovery");
                        mRecoveryStream.SetLength(0);
                    } else {
                        Debug.WriteLine("AutoSave tick: project not dirty");
                    }
                    mLastAutoSaveWhen = mLastEditWhen;      // bump this so earlier test fires
                    return;
                }

                // The project is dirty, and we haven't auto-saved since the last change was
                // made.  Serialize the project to the recovery file.
                Mouse.OverrideCursor = Cursors.Wait;

                DateTime startWhen = DateTime.Now;
                mRecoveryStream.Position = 0;
                mRecoveryStream.SetLength(0);
                if (!ProjectFile.SerializeToStream(mProject, mRecoveryStream,
                        out string errorMessage)) {
                    Debug.WriteLine("AutoSave FAILED: " + errorMessage);
                }
                mRecoveryStream.Flush();    // flush is very important, timing is not; try Async?
                mLastAutoSaveWhen = DateTime.Now;
                Debug.WriteLine("AutoSave tick: recovery file updated: " + mRecoveryStream.Length +
                    " bytes (" + (mLastAutoSaveWhen - startWhen).TotalMilliseconds + " ms)");
            } catch (Exception ex) {
                // Not expected, but let's not crash just because auto-save is broken.
                Debug.WriteLine("AutoSave FAILED ENTIRELY: " + ex);
            } finally {
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Creates or deletes the recovery file, based on the current app settings.
        /// </summary>
        /// <remarks>
        /// <para>This is called when:</para>
        /// <list type="bullet">
        ///   <item>a new project is created</item>
        ///   <item>an existing project is opened</item>
        ///   <item>app settings are updated</item>
        ///   <item>Save As is used to change the project path</item>
        ///   <item>the project is saved for the first time after a recovery file decision (i.e.
        ///     while mAutoSaveDeferred is true)</item>
        /// </list>
        /// </remarks>
        private void RefreshRecoveryFile() {
            if (mProject == null) {
                // Project not open, nothing to do.
                return;
            }
            if (mProject.IsReadOnly) {
                // Changes cannot be made, so there's no need for a recovery file.  Also, we
                // might be in read-only mode because the project is already open and has a
                // recovery file opened by another process.
                Debug.WriteLine("Recovery: project is read-only, not creating recovery file");
                Debug.Assert(mRecoveryStream == null);
                return;
            }
            if (mAutoSaveDeferred) {
                Debug.WriteLine("Recovery: auto-save deferred, not touching recovery file");
                return;
            }

            int interval = AppSettings.Global.GetInt(AppSettings.PROJ_AUTO_SAVE_INTERVAL, 0);
            if (interval <= 0) {
                // We don't want a recovery file.  If one is open, close it and remove it.
                if (mRecoveryStream != null) {
                    Debug.WriteLine("Recovery: auto-save is disabled");
                    DiscardRecoveryFile();
                    Debug.Assert(string.IsNullOrEmpty(mRecoveryPathName));
                } else {
                    Debug.WriteLine("Recovery: auto-save is disabled, file was not open");
                }
                mAutoSaveTimer.Stop();
            } else {
                // Configure auto-save.  We need to update the interval in case it was changed
                // by an app settings update.
                mAutoSaveTimer.Interval = TimeSpan.FromSeconds(interval);
                // Force an initial auto-save (on next timer tick) if the project is dirty, in
                // case auto-save was previously disabled.
                mLastAutoSaveWhen = mLastEditWhen.AddSeconds(-1);

                string pathName = GenerateRecoveryPathName(mProjectPathName);
                if (!string.IsNullOrEmpty(mRecoveryPathName) && pathName == mRecoveryPathName) {
                    // File is open and the filename hasn't changed.  Nothing to do.
                    Debug.Assert(mRecoveryStream != null);
                    Debug.WriteLine("Recovery: open, no changes");
                } else {
                    if (mRecoveryStream != null) {
                        Debug.WriteLine("Recovery: closing '" + mRecoveryPathName +
                            "' in favor of '" + pathName + "'");
                        DiscardRecoveryFile();
                    }
                    if (!string.IsNullOrEmpty(pathName)) {
                        Debug.WriteLine("Recovery: creating '" + pathName + "'");
                        PrepareRecoveryFile();
                    } else {
                        // Must be a new project that has never been saved.
                        Debug.WriteLine("Recovery: project name not set, can't create recovery file");
                    }
                }
                mAutoSaveTimer.Start();
            }
        }

        private static string GenerateRecoveryPathName(string pathName) {
            if (string.IsNullOrEmpty(pathName)) {
                return string.Empty;
            } else {
                return pathName + RECOVERY_EXT_ADD;
            }
        }

        /// <summary>
        /// Creates the recovery file, overwriting any existing file.  If auto-save is disabled
        /// (indicated by an empty recovery file name), this does nothing.
        /// </summary>
        private void PrepareRecoveryFile() {
            Debug.Assert(mRecoveryStream == null);
            Debug.Assert(!string.IsNullOrEmpty(mProjectPathName));
            Debug.Assert(string.IsNullOrEmpty(mRecoveryPathName));

            string pathName = GenerateRecoveryPathName(mProjectPathName);
            try {
                mRecoveryStream = new FileStream(pathName, FileMode.OpenOrCreate, FileAccess.Write);
                mRecoveryPathName = pathName;
            } catch (Exception ex) {
                MessageBox.Show(mMainWin, "Failed to create recovery file '" +
                    pathName + "': " + ex.Message, "File Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// If we have a recovery file, close and delete it.  This does nothing if the recovery
        /// file is not currently open.
        /// </summary>
        private void DiscardRecoveryFile() {
            if (mRecoveryStream == null) {
                Debug.Assert(string.IsNullOrEmpty(mRecoveryPathName));
                return;
            }
            Debug.WriteLine("Recovery: discarding recovery file '" + mRecoveryPathName + "'");
            mRecoveryStream.Close();
            try {
                File.Delete(mRecoveryPathName);
            } catch (Exception ex) {
                MessageBox.Show(mMainWin, "Failed to delete recovery file '" +
                    mRecoveryPathName + "': " + ex.Message, "File Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                // Discard our internal state.
            }
            mRecoveryStream = null;
            mRecoveryPathName = string.Empty;
            mAutoSaveTimer.Stop();
        }

        /// <summary>
        /// Asks the user if they want to use the recovery file, if one is present and non-empty.
        /// Both files must exist.
        /// </summary>
        /// <param name="projPathName">Path to project file we're trying to open</param>
        /// <param name="recoveryPath">Path to recovery file.</param>
        /// <param name="pathToUse">Result: path the user wishes to use.  If we didn't ask the
        ///   user to choose, because the recovery file was empty or in use by another process,
        ///   this will be an empty string.</param>
        /// <param name="asReadOnly">Result: true if project should be opened read-only.</param>
        /// <returns>False if the user cancelled the operation, true to continue.</returns>
        private bool HandleRecoveryChoice(string projPathName, string recoveryPath,
                out string pathToUse, out bool asReadOnly) {
            pathToUse = string.Empty;
            asReadOnly = false;

            try {
                using (FileStream stream = new FileStream(recoveryPath, FileMode.Open,
                        FileAccess.ReadWrite, FileShare.None)) {
                    if (stream.Length == 0) {
                        // Recovery file exists, but is empty and not open by another process.
                        // Ignore it.  (We could delete it here, but there's no need.)
                        Debug.WriteLine("Recovery: found existing zero-length file (ignoring)");
                        return true;
                    }
                }
            } catch (Exception ex) {
                // Unable to open recovery file.  This is probably happening because another
                // process has the file open.
                Debug.WriteLine("Unable to open recovery file: " + ex.Message);
                MessageBoxResult mbr = MessageBox.Show(mMainWin,
                    "The project has a recovery file that can't be opened, possibly because the " +
                    "project is currently open by another copy of the application.  Do you wish " +
                    "to open the file read-only?",
                    "Unable to Open", MessageBoxButton.OKCancel, MessageBoxImage.Hand);
                if (mbr == MessageBoxResult.OK) {
                    asReadOnly = true;
                    return true;
                } else {
                    asReadOnly = false;
                    return false;
                }
            }

            RecoveryChoice dlg = new RecoveryChoice(mMainWin, projPathName, recoveryPath);
            if (dlg.ShowDialog() != true) {
                return false;
            }
            if (dlg.UseRecoveryFile) {
                Debug.WriteLine("Recovery: user chose recovery file");
                pathToUse = recoveryPath;
            } else {
                Debug.WriteLine("Recovery: user chose project file");
                pathToUse = projPathName;
            }
            return true;
        }

        #endregion Auto-save

        #region Project management

        private bool PrepareNewProject(string dataPathName, SystemDef sysDef) {
            DisasmProject proj = new DisasmProject();
            mDataPathName = dataPathName;
            mProjectPathName = string.Empty;
            byte[] fileData;
            try {
                fileData = LoadDataFile(dataPathName);
            } catch (Exception ex) {
                Debug.WriteLine("PrepareNewProject exception: " + ex);
                string message = Res.Strings.OPEN_DATA_FAIL_CAPTION;
                string caption = Res.Strings.OPEN_DATA_FAIL_MESSAGE + ": " + ex.Message;
                MessageBox.Show(caption, message, MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
            proj.UseMainAppDomainForPlugins = UseMainAppDomainForPlugins;
            proj.Initialize(fileData.Length);
            proj.PrepForNew(fileData, Path.GetFileName(dataPathName));

            // Initial header comment is the program name and version.
            string cmt = string.Format(Res.Strings.DEFAULT_HEADER_COMMENT_FMT, App.ProgramVersion);
            proj.LongComments.Add(LineListGen.Line.HEADER_COMMENT_OFFSET,
                new MultiLineComment(cmt));

            // The system definition provides a set of defaults that can be overridden.
            // We pull everything of interest out and then discard the object.
            proj.ApplySystemDef(sysDef);

            mProject = proj;

            return true;
        }

#if false
        private class FinishPrepProgress : WorkProgress.IWorker {
            public string ExtMessages { get; private set; }
            private MainController mMainCtrl;

            public FinishPrepProgress(MainController mainCtrl) {
                mMainCtrl = mainCtrl;
            }
            public object DoWork(BackgroundWorker worker) {
                string messages = mMainCtrl.mProject.LoadExternalFiles();
                mMainCtrl.DoRefreshProject(UndoableChange.ReanalysisScope.CodeAndData);
                return messages;
            }

            public void RunWorkerCompleted(object results) {
                ExtMessages = (string)results;
            }
        }
#endif

        private void FinishPrep() {
            CodeLineList = new LineListGen(mProject, mMainWin.CodeDisplayList,
                mFormatter, mPseudoOpNames);
            SetCodeLineListColorMultiplier();

            string messages = mProject.LoadExternalFiles();
            if (messages.Length != 0) {
                // ProjectLoadIssues isn't quite the right dialog, but it'll do.  This is
                // purely informative; no decision needs to be made.
                ProjectLoadIssues dlg = new ProjectLoadIssues(mMainWin, messages,
                    ProjectLoadIssues.Buttons.Continue);
                dlg.ShowDialog();
            }

            // Ideally we'd call DoRefreshProject (and LoadExternalFiles) from a progress
            // dialog, but we're not allowed to update the DisplayList from a different thread.
            RefreshProject(UndoableChange.ReanalysisScope.CodeAndData);

            // Populate the Symbols list.
            PopulateSymbolsList();

            // Load initial contents of Notes panel.
            PopulateNotesList();

            mMainWin.ShowCodeListView = true;
            mNavStack.Clear();

            UpdateRecentProjectList(mProjectPathName);

            UpdateTitle();
        }

        /// <summary>
        /// Loads the data file, reading it entirely into memory.
        /// 
        /// All errors are reported as exceptions.
        /// </summary>
        /// <param name="dataFileName">Full pathname.</param>
        /// <returns>Data file contents.</returns>
        private static byte[] LoadDataFile(string dataFileName) {
            byte[] fileData;

            using (FileStream fs = File.Open(dataFileName, FileMode.Open, FileAccess.Read)) {
                // Check length; should have been caught earlier.
                if (fs.Length > DisasmProject.MAX_DATA_FILE_SIZE) {
                    throw new InvalidDataException(
                        string.Format(Res.Strings.OPEN_DATA_TOO_LARGE_FMT,
                            fs.Length / 1024, DisasmProject.MAX_DATA_FILE_SIZE / 1024));
                } else if (fs.Length == 0) {
                    throw new InvalidDataException(Res.Strings.OPEN_DATA_EMPTY);
                }
                fileData = new byte[fs.Length];
                int actual = fs.Read(fileData, 0, (int)fs.Length);
                if (actual != fs.Length) {
                    // Not expected -- should be able to read the entire file in one shot.
                    throw new Exception(Res.Strings.OPEN_DATA_PARTIAL_READ);
                }
            }

            return fileData;
        }

        /// <summary>
        /// Applies the changes to the project, adds them to the undo stack, and updates
        /// the display.
        /// </summary>
        /// <param name="cs">Set of changes to apply.</param>
        private void ApplyUndoableChanges(ChangeSet cs) {
            if (cs.Count == 0) {
                Debug.WriteLine("ApplyUndoableChanges: change set is empty");
                // Apply anyway to create an undoable non-event?
            }
            ApplyChanges(cs, false);
            mProject.PushChangeSet(cs);
            UpdateTitle();

            // If the debug dialog is visible, update it.
            if (mShowUndoRedoHistoryDialog != null) {
                mShowUndoRedoHistoryDialog.DisplayText = mProject.DebugGetUndoRedoHistory();
            }
        }

        /// <summary>
        /// Applies the changes to the project, and updates the display.
        /// 
        /// This is called by the undo/redo commands.  Don't call this directly from the
        /// various UI-driven functions, as this does not add the change to the undo stack.
        /// </summary>
        /// <param name="cs">Set of changes to apply.</param>
        /// <param name="backward">If set, undo the changes instead.</param>
        private void ApplyChanges(ChangeSet cs, bool backward) {
            mReanalysisTimer.Clear();
            mReanalysisTimer.StartTask("ProjectView.ApplyChanges()");

            mReanalysisTimer.StartTask("Save selection");
            mMainWin.CodeListView_DebugValidateSelectionCount();
            int topItemIndex = mMainWin.CodeListView_GetTopIndex();
            LineListGen.SavedSelection savedSel = LineListGen.SavedSelection.Generate(
                CodeLineList, mMainWin.CodeDisplayList.SelectedIndices, topItemIndex);
            //savedSel.DebugDump();

            // Clear the addr/label highlight index.
            // (Certain changes will blow away the CodeDisplayList and affect the selection,
            // which will cause the selection-changed handler to try to un-highlight something
            // that doesn't exist.  We want to clear the index here, but we probably also want
            // to clear the highlighting before we do it.  As it happens, changes will either
            // be big enough to wipe out our highlight, or small enough that we immediately
            // re-highlight the thing that's already highlighted, so it doesn't really matter.
            // If we start to see vestigial highlighting after a change, we'll need to be
            // more rigorous here.)
            mTargetHighlightIndex = -1;

            // Clear operand highlighting indices as well.
            mOperandHighlights.Clear();

            mReanalysisTimer.EndTask("Save selection");

            mReanalysisTimer.StartTask("Apply changes");
            UndoableChange.ReanalysisScope needReanalysis = mProject.ApplyChanges(cs, backward,
                out RangeSet affectedOffsets);
            mReanalysisTimer.EndTask("Apply changes");

            string refreshTaskStr = "Refresh w/reanalysis=" + needReanalysis;
            mReanalysisTimer.StartTask(refreshTaskStr);
            if (needReanalysis != UndoableChange.ReanalysisScope.None) {
                Debug.WriteLine("Refreshing project (" + needReanalysis + ")");
                RefreshProject(needReanalysis);
            } else {
                Debug.WriteLine("Refreshing " + affectedOffsets.Count + " offsets");
                RefreshCodeListViewEntries(affectedOffsets);
                mProject.Validate();    // shouldn't matter w/o reanalysis, but do it anyway
            }
            mReanalysisTimer.EndTask(refreshTaskStr);

            mReanalysisTimer.StartTask("Restore selection and top position");
            DisplayListSelection newSel = savedSel.Restore(CodeLineList, out topItemIndex);
            //newSel.DebugDump();

            // Restore the selection.  The selection-changed event will cause updates to the
            // references, notes, and info panels.
            mMainWin.CodeListView_SetSelection(newSel);
            mMainWin.CodeListView_SetTopIndex(topItemIndex);
            mReanalysisTimer.EndTask("Restore selection and top position");

            // Update the Notes and Symbols windows.  References should refresh automatically
            // when the selection is restored.
            mReanalysisTimer.StartTask("Populate Notes and Symbols");
            PopulateNotesList();
            PopulateSymbolsList();
            mReanalysisTimer.EndTask("Populate Notes and Symbols");

            mReanalysisTimer.EndTask("ProjectView.ApplyChanges()");

            //mReanalysisTimer.DumpTimes("ProjectView timers:", mGenerationLog);
            if (mShowAnalysisTimersDialog != null) {
                string timerStr = mReanalysisTimer.DumpToString("ProjectView timers:");
                mShowAnalysisTimersDialog.DisplayText = timerStr;
            }

            // Lines may have moved around.  Update the selection highlight.  It's important
            // we do it here, and not down in DoRefreshProject(), because at that point the
            // ListView's selection index could be referencing a line off the end.
            // (This may not be necessary with WPF, because the way highlights work changed.)
            UpdateSelectionHighlight();

            // Bump the edit timestamp so the auto-save will run.
            mLastEditWhen = DateTime.Now;
        }

        /// <summary>
        /// Updates all of the specified ListView entries.  This is called after minor changes,
        /// such as editing a comment or renaming a label, that can be handled by regenerating
        /// selected parts of the DisplayList.
        /// </summary>
        /// <param name="offsetSet"></param>
        private void RefreshCodeListViewEntries(RangeSet offsetSet) {
            IEnumerator<RangeSet.Range> iter = offsetSet.RangeListIterator;
            while (iter.MoveNext()) {
                RangeSet.Range range = iter.Current;
                CodeLineList.GenerateRange(range.Low, range.High);
            }
        }

        /// <summary>
        /// Refreshes the project after something of substance has changed.  Some
        /// re-analysis will be done, followed by a complete rebuild of the DisplayList.
        /// </summary>
        /// <param name="reanalysisRequired">Indicates whether reanalysis is required, and
        ///   what level.</param>
        private void RefreshProject(UndoableChange.ReanalysisScope reanalysisRequired) {
            Debug.Assert(reanalysisRequired != UndoableChange.ReanalysisScope.None);

            // NOTE: my goal is to arrange things so that reanalysis (data-only, and ideally
            // code+data) takes less than 100ms.  With that response time there's no need for
            // background processing and progress bars.  Since we need to do data-only
            // reanalysis after many common operations, the program becomes unpleasant to
            // use if we miss this goal, and progress bars won't make it less so.

            if (mProject.FileDataLength > 65536) {
                try {
                    Mouse.OverrideCursor = Cursors.Wait;
                    DoRefreshProject(reanalysisRequired);
                } finally {
                    Mouse.OverrideCursor = null;
                }
            } else {
                DoRefreshProject(reanalysisRequired);
            }

            if (mGenerationLog != null) {
                //mReanalysisTimer.StartTask("Save _log");
                //mGenerationLog.WriteToFile(@"C:\Src\WorkBench\SourceGen\TestData\_log.txt");
                //mReanalysisTimer.EndTask("Save _log");

                if (mShowAnalyzerOutputDialog != null) {
                    mShowAnalyzerOutputDialog.DisplayText = mGenerationLog.WriteToString();
                }
            }

            if (FormatDescriptor.DebugCreateCount != 0) {
                Debug.WriteLine("FormatDescriptor total=" + FormatDescriptor.DebugCreateCount +
                    " prefab=" + FormatDescriptor.DebugPrefabCount + " (" +
                    (FormatDescriptor.DebugPrefabCount * 100) / FormatDescriptor.DebugCreateCount +
                    "%)");
            }
        }

        /// <summary>
        /// Refreshes the project after something of substance has changed.
        /// </summary>
        /// <remarks>
        /// Ideally from this point on we can run on a background thread.  The tricky part
        /// is the close relationship between LineListGen and DisplayList -- we can't update
        /// DisplayList from a background thread.  Until that's fixed, putting up a "working..."
        /// dialog or other UI will be awkward.
        /// </remarks>
        /// <param name="reanalysisRequired">Indicates whether reanalysis is required, and
        ///   what level.</param>
        private void DoRefreshProject(UndoableChange.ReanalysisScope reanalysisRequired) {
            // Changing the CPU type or whether undocumented instructions are supported
            // invalidates the Formatter's mnemonic cache.  We can change these values
            // through undo/redo, so we need to check it here.
            if (mFormatterCpuDef != mProject.CpuDef) {    // reference equality is fine
                Debug.WriteLine("CpuDef has changed, resetting formatter (now " +
                    mProject.CpuDef + ")");
                mFormatter = new Formatter(mFormatterConfig);
                CodeLineList.SetFormatter(mFormatter);
                CodeLineList.SetPseudoOpNames(mPseudoOpNames);
                mFormatterCpuDef = mProject.CpuDef;
            }

            if (reanalysisRequired != UndoableChange.ReanalysisScope.DisplayOnly) {
                mGenerationLog = new CommonUtil.DebugLog();
                mGenerationLog.SetMinPriority(CommonUtil.DebugLog.Priority.Debug);
                mGenerationLog.SetShowRelTime(true);

                mReanalysisTimer.StartTask("Call DisasmProject.Analyze()");
                mProject.Analyze(reanalysisRequired, mGenerationLog, mReanalysisTimer);
                mReanalysisTimer.EndTask("Call DisasmProject.Analyze()");

                mReanalysisTimer.StartTask("Update message list");
                mMainWin.UpdateMessageList(mProject.Messages, mFormatter);
                mReanalysisTimer.EndTask("Update message list");
            }

            mReanalysisTimer.StartTask("Generate DisplayList");
            CodeLineList.GenerateAll();
            mReanalysisTimer.EndTask("Generate DisplayList");

            mReanalysisTimer.StartTask("Refresh Visualization thumbnails");
            VisualizationSet.RefreshAllThumbnails(mProject);
            mReanalysisTimer.EndTask("Refresh Visualization thumbnails");
        }

        #endregion Project management

        #region Main window UI event handlers

        /// <summary>
        /// Handles creation of a new project.
        /// </summary>
        public void NewProject() {
            if (!CloseProject()) {
                return;
            }

            string sysDefsPath = RuntimeDataAccess.GetPathName("SystemDefs.json");
            if (sysDefsPath == null) {
                MessageBox.Show(Res.Strings.ERR_LOAD_CONFIG_FILE, Res.Strings.OPERATION_FAILED,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SystemDefSet sds;
            try {
                sds = SystemDefSet.ReadFile(sysDefsPath);
            } catch (Exception ex) {
                Debug.WriteLine("Failed loading system def set: " + ex);
                MessageBox.Show(Res.Strings.ERR_LOAD_CONFIG_FILE, Res.Strings.OPERATION_FAILED,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            NewProject dlg = new NewProject(mMainWin, sds);
            if (dlg.ShowDialog() != true) {
                return;
            }
            bool ok = PrepareNewProject(Path.GetFullPath(dlg.DataFileName), dlg.SystemDef);
            if (ok) {
                FinishPrep();
                SaveProjectAs();
                RefreshRecoveryFile();
            }
        }

        public void OpenRecentProject(int projIndex) {
            if (!CloseProject()) {
                return;
            }
            DoOpenFile(RecentProjectPaths[projIndex]);
        }

        /// <summary>
        /// Handles opening an existing project by letting the user select the project file.
        /// </summary>
        public void OpenProject() {
            if (!CloseProject()) {
                return;
            }

            OpenFileDialog fileDlg = new OpenFileDialog() {
                Filter = ProjectFile.FILENAME_FILTER + "|" + Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }

            string projPathName = Path.GetFullPath(fileDlg.FileName);
            DoOpenFile(projPathName);
        }

        /// <summary>
        /// Handles opening an existing project, given a full pathname to the project file.
        /// </summary>
        private void DoOpenFile(string projPathName) {
            Debug.WriteLine("DoOpenFile: " + projPathName);
            Debug.Assert(mProject == null);

            if (!File.Exists(projPathName)) {
                // Should only happen for projects in "recents".
                string msg = string.Format(Res.Strings.ERR_FILE_NOT_FOUND_FMT, projPathName);
                MessageBox.Show(msg, Res.Strings.ERR_FILE_GENERIC_CAPTION,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DisasmProject newProject = new DisasmProject();
            newProject.UseMainAppDomainForPlugins = UseMainAppDomainForPlugins;

            // Is there a recovery file?
            mAutoSaveDeferred = false;
            string recoveryPath = GenerateRecoveryPathName(projPathName);
            string openPath = projPathName;
            if (File.Exists(recoveryPath)) {
                // Found a recovery file.
                bool ok = HandleRecoveryChoice(projPathName, recoveryPath, out string pathToUse,
                    out bool asReadOnly);
                if (!ok) {
                    // Open has been cancelled.
                    return;
                }
                if (!string.IsNullOrEmpty(pathToUse)) {
                    // One was chosen.  This should be the case unless the recovery file was
                    // empty, or was open by a different process.
                    Debug.WriteLine("Open: user chose '" + pathToUse + "', deferring auto-save");
                    openPath = pathToUse;
                    mAutoSaveDeferred = true;
                }
                newProject.IsReadOnly |= asReadOnly;
            }

            // Deserialize the project file.  I want to do this before loading the data file
            // in case we decide to store the data file name in the project (e.g. the data
            // file is a disk image or zip archive, and we need to know which part(s) to
            // extract).
            if (!ProjectFile.DeserializeFromFile(openPath, newProject,
                    out FileLoadReport report)) {
                // Should probably use a less-busy dialog for something simple like
                // "permission denied", but the open file dialog handles most simple
                // stuff directly.
                ProjectLoadIssues dlg = new ProjectLoadIssues(mMainWin, report.Format(),
                    ProjectLoadIssues.Buttons.Cancel);
                dlg.ShowDialog();
                // ignore dlg.DialogResult
                return;
            }

            // Now open the data file, generating the pathname by stripping off the ".dis65"
            // extension.  If we can't find the file, show a message box and offer the option to
            // locate it manually, repeating the process until successful or canceled.
            const string UNKNOWN_FILE = "UNKNOWN";
            string dataPathName;
            if (projPathName.EndsWith(ProjectFile.FILENAME_EXT,
                    StringComparison.InvariantCultureIgnoreCase)) {
                dataPathName = projPathName.Substring(0,
                    projPathName.Length - ProjectFile.FILENAME_EXT.Length);
            } else if (projPathName.EndsWith(RECOVERY_EXT,
                    StringComparison.InvariantCultureIgnoreCase)) {
                dataPathName = projPathName.Substring(0,
                    projPathName.Length - RECOVERY_EXT.Length);
            } else {
                dataPathName = UNKNOWN_FILE;
            }
            byte[] fileData;
            while ((fileData = FindValidDataFile(ref dataPathName, newProject,
                    out bool cancel)) == null) {
                if (cancel) {
                    // give up
                    Debug.WriteLine("Abandoning attempt to open project");
                    return;
                }
            }

            newProject.SetFileData(fileData, Path.GetFileName(dataPathName), ref report);

            // If there were warnings, notify the user and give the a chance to cancel.
            if (report.Count != 0) {
                ProjectLoadIssues dlg = new ProjectLoadIssues(mMainWin, report.Format(),
                    ProjectLoadIssues.Buttons.ContinueOrCancel);
                bool? ok = dlg.ShowDialog();

                if (ok != true) {
                    return;
                }

                newProject.IsReadOnly |= dlg.WantReadOnly;
            }

            mProject = newProject;
            mProjectPathName = mProject.ProjectPathName = projPathName;
            mDataPathName = dataPathName;
            FinishPrep();
            RefreshRecoveryFile();
        }

        /// <summary>
        /// Finds and loads the specified data file.  The file's length and CRC must match
        /// the project's expectations.
        /// </summary>
        /// <param name="dataPathName">Full path to file.</param>
        /// <param name="proj">Project object.</param>
        /// <param name="cancel">Returns true if we want to cancel the attempt.</param>
        /// <returns>File data.</returns>
        private byte[] FindValidDataFile(ref string dataPathName, DisasmProject proj,
                out bool cancel) {
            // TODO(someday):
            // It would be nice to "fix" the length and CRC if they don't match while we're
            // making manual edits to test files.  We can pass "can fix" to the ChooseDataFile
            // dialog, and have it return a "want fix" if they click on the "fix" button, and
            // only enable this if the DEBUG menu is enabled.  It's a little ugly but mostly
            // works.  One issue that must be handled is that "proj" has sized a bunch of data
            // structures based on the expected file length, and will blow up if the actual
            // length is different.  So we really need to check both len/crc here, and if
            // all broken things are fixable, return the "do fix" back to the caller so
            // it can re-generate the DisasmProject object with the corrected length.

            FileInfo fi = new FileInfo(dataPathName);
            if (!fi.Exists) {
                Debug.WriteLine("File '" + dataPathName + "' doesn't exist");
                dataPathName = ChooseDataFile(dataPathName,
                    Res.Strings.OPEN_DATA_DOESNT_EXIST);
                cancel = (dataPathName == null);
                return null;
            }
            if (fi.Length != proj.FileDataLength) {
                Debug.WriteLine("File '" + dataPathName + "' has length=" + fi.Length +
                    ", expected " + proj.FileDataLength);
                dataPathName = ChooseDataFile(dataPathName,
                    string.Format(Res.Strings.OPEN_DATA_WRONG_LENGTH_FMT,
                        fi.Length, proj.FileDataLength));
                cancel = (dataPathName == null);
                return null;
            }
            byte[] fileData;
            try {
                fileData = LoadDataFile(dataPathName);
            } catch (Exception ex) {
                Debug.WriteLine("File '" + dataPathName + "' failed to load: " + ex.Message);
                dataPathName = ChooseDataFile(dataPathName,
                    string.Format(Res.Strings.OPEN_DATA_LOAD_FAILED_FMT, ex.Message));
                cancel = (dataPathName == null);
                return null;
            }
            uint crc = CRC32.OnWholeBuffer(0, fileData);
            if (crc != proj.FileDataCrc32) {
                Debug.WriteLine("File '" + dataPathName + "' has CRC32=" + crc +
                    ", expected " + proj.FileDataCrc32);
                // Format the CRC as signed decimal, so that interested parties can
                // easily replace the value in the .dis65 file.
                dataPathName = ChooseDataFile(dataPathName,
                    string.Format(Res.Strings.OPEN_DATA_WRONG_CRC_FMT,
                        (int)crc, (int)proj.FileDataCrc32));
                cancel = (dataPathName == null);
                return null;
            }

            cancel = false;
            return fileData;
        }

        /// <summary>
        /// Displays a "do you want to pick a different file" message, then (on OK) allows the
        /// user to select a file.
        /// </summary>
        /// <param name="origPath">Pathname of original file.</param>
        /// <param name="errorMsg">Message to display in the message box.</param>
        /// <returns>Full path of file to open.</returns>
        private string ChooseDataFile(string origPath, string errorMsg) {
            DataFileLoadIssue dlg = new DataFileLoadIssue(mMainWin, origPath, errorMsg);
            bool? ok = dlg.ShowDialog();
            if (ok != true) {
                return null;
            }

            OpenFileDialog fileDlg = new OpenFileDialog() {
                FileName = Path.GetFileName(origPath),
                Filter = Res.Strings.FILE_FILTER_ALL
            };
            if (fileDlg.ShowDialog() != true) {
                return null;
            }

            string newPath = Path.GetFullPath(fileDlg.FileName);
            Debug.WriteLine("User selected data file " + newPath);
            return newPath;
        }

        /// <summary>
        /// Saves the project, querying for the filename.
        /// </summary>
        /// <returns>True on success, false if the save attempt failed or was canceled.</returns>
        public bool SaveProjectAs() {
            Debug.Assert(!mProject.IsReadOnly);
            SaveFileDialog fileDlg = new SaveFileDialog() {
                Filter = ProjectFile.FILENAME_FILTER + "|" + Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1,
                ValidateNames = true,
                AddExtension = true,
                FileName = Path.GetFileName(mDataPathName) + ProjectFile.FILENAME_EXT
            };
            if (fileDlg.ShowDialog() != true) {
                Debug.WriteLine("SaveAs canceled by user");
                return false;
            }
            string pathName = Path.GetFullPath(fileDlg.FileName);
            Debug.WriteLine("Project save path: " + pathName);
            if (!DoSave(pathName)) {
                return false;
            }

            // Success, record the path name.
            mProjectPathName = mProject.ProjectPathName = pathName;

            RefreshRecoveryFile();

            // add it to the title bar
            UpdateTitle();
            return true;
        }

        /// <summary>
        /// Saves the project.  If it hasn't been saved before, use save-as behavior instead.
        /// </summary>
        /// <returns>True on success, false if the save attempt failed.</returns>
        public bool SaveProject() {
            Debug.Assert(!mProject.IsReadOnly);
            if (string.IsNullOrEmpty(mProjectPathName)) {
                return SaveProjectAs();
            }
            return DoSave(mProjectPathName);
        }

        private bool DoSave(string pathName) {
            Debug.Assert(!mProject.IsReadOnly);     // save commands should be disabled
            Debug.WriteLine("SAVING " + pathName);
            if (!ProjectFile.SerializeToFile(mProject, pathName, out string errorMessage)) {
                MessageBox.Show(Res.Strings.ERR_PROJECT_SAVE_FAIL + ": " + errorMessage,
                    Res.Strings.OPERATION_FAILED,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            mProject.ResetDirtyFlag();
            // If the debug dialog is visible, update it.
            if (mShowUndoRedoHistoryDialog != null) {
                mShowUndoRedoHistoryDialog.DisplayText = mProject.DebugGetUndoRedoHistory();
            }
            UpdateTitle();

            // Update this, in case this was a new project.
            UpdateRecentProjectList(pathName);

            // Seems like a good time to save this off too.
            SaveAppSettings();

            if (mAutoSaveDeferred) {
                mAutoSaveDeferred = false;
                RefreshRecoveryFile();
            }

            // The project file is saved, no need to auto-save for a while.
            ResetAutoSaveTimer();

            return true;
        }

        /// <summary>
        /// Handles main window closing.
        /// </summary>
        /// <returns>True if it's okay for the window to close, false to cancel it.</returns>
        public bool WindowClosing() {
            SaveAppSettings();
            if (!CloseProject()) {
                return false;
            }

            // WPF won't exit until all windows are closed, so any unowned windows need
            // to be cleaned up here.
            mApple2ScreenChartDialog?.Close();
            mAsciiChartDialog?.Close();
            mInstructionChartDialog?.Close();
            mHexDumpDialog?.Close();
            mReferenceTableDialog?.Close();
            mShowAnalysisTimersDialog?.Close();
            mShowAnalyzerOutputDialog?.Close();
            mShowUndoRedoHistoryDialog?.Close();

            while (mUnownedWindows.Count > 0) {
                int count = mUnownedWindows.Count;
                mUnownedWindows[0].Close();
                if (count == mUnownedWindows.Count) {
                    // Window failed to remove itself; this will cause an infinite loop.
                    // The user will have to close them manually.
                    Debug.Assert(false, "Failed to close window " + mUnownedWindows[0]);
                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// Closes the project and associated modeless dialogs.  Unsaved changes will be
        /// lost, so if the project has outstanding changes the user will be given the
        /// opportunity to cancel.
        /// </summary>
        /// <returns>True if the project was closed, false if the user chose to cancel.</returns>
        public bool CloseProject() {
            Debug.WriteLine("CloseProject() - dirty=" +
                (mProject == null ? "N/A" : mProject.IsDirty.ToString()));
            if (mProject != null && mProject.IsDirty) {
                DiscardChanges dlg = new DiscardChanges(mMainWin);
                bool? ok = dlg.ShowDialog();
                if (ok != true) {
                    return false;
                } else if (dlg.UserChoice == DiscardChanges.Choice.SaveAndContinue) {
                    if (!SaveProject()) {
                        return false;
                    }
                }
            }

            // Close modeless dialogs that depend on project.
            mHexDumpDialog?.Close();
            mReferenceTableDialog?.Close();
            mShowAnalysisTimersDialog?.Close();
            mShowAnalyzerOutputDialog?.Close();
            mShowUndoRedoHistoryDialog?.Close();

            // Discard all project state.
            if (mProject != null) {
                mProject.Cleanup();
                mProject = null;
            }
            mDataPathName = null;
            mProjectPathName = null;
            CodeLineList = null;

            // We may get a "selection changed" message as things are being torn down.  Clear
            // these so we don't try to remove the highlight from something that doesn't exist.
            mTargetHighlightIndex = -1;
            mOperandHighlights.Clear();

            mMainWin.ShowCodeListView = false;
            mMainWin.ProjectClosing();

            mGenerationLog = null;

            UpdateTitle();

            DiscardRecoveryFile();

            // Not necessary, but it lets us check the memory monitor to see if we got
            // rid of everything.
            GC.Collect();

            return true;
        }

        public bool IsProjectOpen {
            get { return mProject != null; }
        }
        public bool IsProjectReadOnly {
            get { return mProject != null && mProject.IsReadOnly; }
        }

        public void AssembleProject() {
            if (string.IsNullOrEmpty(mProjectPathName)) {
                // We need a project pathname so we know where to write the assembler
                // source files, and what to call the output files.  We could just pop up the
                // Save As dialog, but that seems confusing unless we do a custom dialog with
                // an explanation, or have some annoying click-through.
                //
                // This only appears for never-saved projects, not projects with unsaved data.
                MessageBox.Show(Res.Strings.SAVE_BEFORE_ASM, Res.Strings.SAVE_BEFORE_ASM_CAPTION,
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AsmGen.WpfGui.GenAndAsm dlg =
                new AsmGen.WpfGui.GenAndAsm(mMainWin, this, mProject, mProjectPathName);
            dlg.ShowDialog();
        }

        /// <summary>
        /// Copies the selection to the clipboard as formatted text.
        /// </summary>
        public void CopyToClipboard() {
            DisplayListSelection selection = mMainWin.CodeDisplayList.SelectedIndices;
            if (selection.Count == 0) {
                Debug.WriteLine("Selection is empty!");
                return;
            }

            ClipLineFormat format = AppSettings.Global.GetEnum(AppSettings.CLIP_LINE_FORMAT,
                    ClipLineFormat.AssemblerSource);

            int[] rightWidths = new int[] { 16, 6, 16, 80 };

            Exporter.ActiveColumnFlags colFlags = Exporter.ActiveColumnFlags.None;
            if (format == ClipLineFormat.Disassembly) {
                colFlags |= Exporter.ActiveColumnFlags.Address |
                    Exporter.ActiveColumnFlags.Bytes;
            } else if (format == ClipLineFormat.AllColumns) {
                colFlags = Exporter.ActiveColumnFlags.ALL;
            }
            Exporter eport = new Exporter(mProject, CodeLineList, mFormatter,
                colFlags, rightWidths);
            eport.Selection = selection;

            // Might want to set Mouse.OverrideCursor if the selection exceeds a few
            // hundred thousand lines.
            eport.SelectionToString(true, out string fullText, out string csvText);

            DataObject dataObject = new DataObject();
            dataObject.SetText(fullText.ToString());

            // We want to have both plain text and CSV data on the clipboard.  To add both
            // formats we need to stream it to a DataObject.  Complicating matters is Excel's
            // entirely reasonable desire to have data in UTF-8 rather than UTF-16.
            //
            // (I'm not sure pasting assembly bits into Excel is actually useful, so this
            // should probably be optional.)
            //
            // https://stackoverflow.com/a/369219/294248
            const bool addCsv = true;
            if (addCsv) {
                byte[] csvData = Encoding.UTF8.GetBytes(csvText.ToString());
                MemoryStream stream = new MemoryStream(csvData);
                dataObject.SetData(DataFormats.CommaSeparatedValue, stream);
            }
            Clipboard.SetDataObject(dataObject, true);
        }

        /// <summary>
        /// Handles Edit &gt; App Settings.
        /// </summary>
        public void EditAppSettings() {
            ShowAppSettings(mMainWin, WpfGui.EditAppSettings.Tab.Unknown,
                AsmGen.AssemblerInfo.Id.Unknown);
        }

        /// <summary>
        /// Opens the application settings dialog.  All changes to settings are made directly
        /// to the AppSettings.Global object.
        /// </summary>
        public void ShowAppSettings(Window owner, EditAppSettings.Tab initialTab,
                    AsmGen.AssemblerInfo.Id initialAsmId) {
            EditAppSettings dlg = new EditAppSettings(owner, mMainWin, initialTab, initialAsmId);
            dlg.SettingsApplied += SetAppSettings;      // called when "Apply" is clicked
            dlg.ShowDialog();
        }

        /// <summary>
        /// Applies settings to the project, and saves them to the settings files.
        /// </summary>
        private void SetAppSettings() {
            ApplyAppSettings();
            SaveAppSettings();
        }

        public void HandleCodeListDoubleClick(int row, int col) {
            //Debug.WriteLine("DCLICK: row=" + row + " col=" + col);
            mMainWin.CodeListView_DebugValidateSelectionCount();

            // Clicking on some types of lines, such as ORG directives, results in
            // specific behavior regardless of which column you click in.  We're just
            // checking the clicked-on line to decide what action to take.  If it doesn't
            // make sense to do for a multi-line selection, the action will have been
            // disabled.
            LineListGen.Line line = CodeLineList[row];
            switch (line.LineType) {
                case LineListGen.Line.Type.EquDirective:
                    // Currently only does something for project symbols; platform symbols
                    // do nothing.
                    if (CanEditProjectSymbol()) {
                        EditProjectSymbol((CodeListColumn)col);
                    }
                    break;
                case LineListGen.Line.Type.ArStartDirective:
                case LineListGen.Line.Type.ArEndDirective:
                    if ((CodeListColumn)col == CodeListColumn.Opcode) {
                        JumpToOperandTarget(line, false);
                    } else if (CanEditAddress()) {
                        EditAddress();
                    }
                    break;
                case LineListGen.Line.Type.RegWidthDirective:
                    if (CanEditStatusFlags()) {
                        EditStatusFlags();
                    }
                    break;
                case LineListGen.Line.Type.DataBankDirective:
                    if (CanEditDataBank()) {
                        EditDataBank();
                    }
                    break;
                case LineListGen.Line.Type.LongComment:
                    if (CanEditLongComment()) {
                        EditLongComment();
                    }
                    break;
                case LineListGen.Line.Type.Note:
                    if (CanEditNote()) {
                        EditNote();
                    }
                    break;
                case LineListGen.Line.Type.LocalVariableTable:
                    if (CanEditLocalVariableTable()) {
                        EditLocalVariableTable();
                    }
                    break;
                case LineListGen.Line.Type.VisualizationSet:
                    if (CanEditVisualizationSet()) {
                        EditVisualizationSet();
                    }
                    break;

                case LineListGen.Line.Type.Code:
                case LineListGen.Line.Type.Data:
                    // For code and data, we have to break it down by column.
                    switch ((CodeListColumn)col) {
                        case CodeListColumn.Offset:
                            // does nothing
                            break;
                        case CodeListColumn.Address:
                            // edit address
                            if (CanEditAddress()) {
                                EditAddress();
                            }
                            break;
                        case CodeListColumn.Bytes:
                            ShowHexDump();
                            break;
                        case CodeListColumn.Flags:
                            if (CanEditStatusFlags()) {
                                EditStatusFlags();
                            }
                            break;
                        case CodeListColumn.Attributes:
                            // does nothing
                            break;
                        case CodeListColumn.Label:
                            if (CanEditLabel()) {
                                EditLabel();
                            }
                            break;
                        case CodeListColumn.Opcode:
                            if (IsPlbInstruction(line) && CanEditDataBank()) {
                                // Special handling for PLB instruction, so you can update the bank
                                // value just by double-clicking on it.  Only used for PLBs without
                                // user- or auto-assigned bank changes.
                                EditDataBank();
                            } else {
                                JumpToOperandTarget(line, false);
                            }
                            break;
                        case CodeListColumn.Operand:
                            if (CanEditOperand()) {
                                EditOperand();
                            }
                            break;
                        case CodeListColumn.Comment:
                            if (CanEditComment()) {
                                EditComment();
                            }
                            break;
                    }
                    break;

                default:
                    Debug.WriteLine("Double-click: unhandled line type " + line.LineType);
                    break;
            }
        }

        private bool IsPlbInstruction(LineListGen.Line line) {
            if (line.LineType != LineListGen.Line.Type.Code) {
                return false;
            }
            int offset = line.FileOffset;
            Anattrib attr = mProject.GetAnattrib(offset);

            // should always be an instruction start since this is a code line
            if (!attr.IsInstructionStart) {
                Debug.Assert(false);
                return false;
            }

            OpDef op = mProject.CpuDef.GetOpDef(mProject.FileData[offset]);
            if (op != OpDef.OpPLB_StackPull) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Jumps to the line referenced by the operand of the selected line.
        /// </summary>
        /// <param name="line">Selected line.</param>
        /// <param name="testOnly">If set, don't actually do the goto.</param>
        /// <returns>True if a jump is available for this line.</returns>
        private bool JumpToOperandTarget(LineListGen.Line line, bool testOnly) {
            if (line.FileOffset < 0) {
                // Double-click on project symbol EQUs and the file header comment are handled
                // elsewhere.
                return false;
            }

            if (line.IsAddressRangeDirective) {
                // TODO(someday): make this jump to the specific directive rather than the first
                //   (should be able to do it with LineDelta)
                AddressMap.AddressRegion region = CodeLineList.GetAddrRegionFromLine(line,
                    out bool unused);
                if (region == null) {
                    Debug.Assert(false);
                    return false;
                }
                if (!testOnly) {
                    if (line.LineType == LineListGen.Line.Type.ArStartDirective) {
                        // jump to end
                        GoToLocation(new NavStack.Location(region.Offset + region.ActualLength - 1,
                            0, NavStack.GoToMode.JumpToArEnd), true);
                    } else {
                        // jump to start
                        GoToLocation(new NavStack.Location(region.Offset,
                            0, NavStack.GoToMode.JumpToArStart), true);
                    }
                }
                return true;
            }

            Anattrib attr = mProject.GetAnattrib(line.FileOffset);
            FormatDescriptor dfd = attr.DataDescriptor;

            if (dfd != null && dfd.HasSymbol) {
                // Operand has a symbol, do a symbol lookup.  This is slower than a simple
                // jump based on OperandOffset, but if we've incorporated reloc data then
                // the jump will be wrong.
                if (dfd.SymbolRef.IsVariable) {
                    if (!testOnly) {
                        GoToVarDefinition(line.FileOffset, dfd.SymbolRef, true);
                    }
                    return true;
                } else {
                    if (mProject.SymbolTable.TryGetValue(dfd.SymbolRef.Label, out Symbol sym)) {
                        if (sym.SymbolSource == Symbol.Source.User ||
                                sym.SymbolSource == Symbol.Source.Auto ||
                                sym.SymbolSource == Symbol.Source.AddrPreLabel) {
                            int labelOffset = mProject.FindLabelOffsetByName(dfd.SymbolRef.Label);
                            if (labelOffset >= 0) {
                                if (!testOnly) {
                                    NavStack.GoToMode mode = NavStack.GoToMode.JumpToCodeData;
                                    if (sym.SymbolSource == Symbol.Source.AddrPreLabel) {
                                        mode = NavStack.GoToMode.JumpToArStart;
                                    }
                                    GoToLocation(new NavStack.Location(labelOffset, 0, mode), true);
                                }
                                return true;
                            }
                        } else if (sym.SymbolSource == Symbol.Source.Platform ||
                                sym.SymbolSource == Symbol.Source.Project) {
                            // find entry
                            for (int i = 0; i < mProject.ActiveDefSymbolList.Count; i++) {
                                if (mProject.ActiveDefSymbolList[i] == sym) {
                                    int offset = LineListGen.DefSymOffsetFromIndex(i);
                                    if (!testOnly) {
                                        GoToLocation(new NavStack.Location(offset, 0,
                                            NavStack.GoToMode.JumpToCodeData), true);
                                    }
                                    return true;
                                }
                            }
                        } else {
                            Debug.Assert(false);
                        }
                    } else {
                        // must be a broken weak symbol ref
                        Debug.WriteLine("Operand symbol not found: " + dfd.SymbolRef.Label);
                    }
                }
            } else if (attr.OperandOffset >= 0) {
                // Operand has an in-file target offset.  We can resolve it as a numeric reference.
                // Find the line for that offset and jump to it.
                if (!testOnly) {
                    GoToLocation(new NavStack.Location(attr.OperandOffset, 0,
                        NavStack.GoToMode.JumpToCodeData), true);
                }
                return true;
            } else if (attr.IsDataStart || attr.IsInlineDataStart) {
                // If it's an Address or Symbol, we can try to resolve
                // the value.  (Symbols should have been resolved by the
                // previous clause, but Address entries would not have been.)
                int operandOffset = DataAnalysis.GetDataOperandOffset(mProject, line.FileOffset,
                    out int unused);
                if (operandOffset >= 0) {
                    if (!testOnly) {
                        GoToLocation(new NavStack.Location(operandOffset, 0,
                            NavStack.GoToMode.JumpToCodeData), true);
                    }
                    return true;
                }
            }

            return false;
        }

        public bool CanDeleteMlc() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            return (SelectionAnalysis.mLineType == LineListGen.Line.Type.LongComment ||
                SelectionAnalysis.mLineType == LineListGen.Line.Type.Note);
        }

        // Delete multi-line comment (Note or LongComment)
        public void DeleteMlc() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            LineListGen.Line line = CodeLineList[selIndex];
            int offset = line.FileOffset;

            UndoableChange uc;
            if (line.LineType == LineListGen.Line.Type.Note) {
                if (!mProject.Notes.TryGetValue(offset, out MultiLineComment oldNote)) {
                    Debug.Assert(false);
                    return;
                }
                uc = UndoableChange.CreateNoteChange(offset, oldNote, null);
            } else if (line.LineType == LineListGen.Line.Type.LongComment) {
                if (!mProject.LongComments.TryGetValue(offset, out MultiLineComment oldComment)) {
                    Debug.Assert(false);
                    return;
                }
                uc = UndoableChange.CreateLongCommentChange(offset, oldComment, null);
            } else {
                Debug.Assert(false);
                return;
            }
            ChangeSet cs = new ChangeSet(uc);
            ApplyUndoableChanges(cs);
        }

        public bool CanEditAddress() {
            // First line must be code, data, or an AR directive.
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            if (selIndex < 0) {
                return false;
            }
            LineListGen.Line selLine = CodeLineList[selIndex];
            if (selLine.LineType != LineListGen.Line.Type.Code &&
                    selLine.LineType != LineListGen.Line.Type.Data &&
                    selLine.LineType != LineListGen.Line.Type.ArStartDirective &&
                    selLine.LineType != LineListGen.Line.Type.ArEndDirective) {
                return false;
            }

            int lastIndex = mMainWin.CodeListView_GetLastSelectedIndex();

            // Can only start with arend if it's single-selection.
            if (selIndex != lastIndex && selLine.LineType == LineListGen.Line.Type.ArEndDirective) {
                return false;
            }

            // If multiple lines with code/data are selected, there must not be an arstart
            // between them unless we're resizing a region.  Determining whether or not a resize
            // is valid is left to the edit dialog.  It's okay for an arend to be in the middle
            // so long as the corresponding arstart is at the current offset.
            if (selLine.LineType == LineListGen.Line.Type.ArStartDirective) {
                // Skip overlapping region check.
                return true;
            }
            int firstOffset = CodeLineList[selIndex].FileOffset;
            int lastOffset = CodeLineList[lastIndex].FileOffset;
            if (firstOffset == lastOffset) {
                // Single-item selection, we're fine.
                return true;
            }

            // Anything else is too complicated to be worth messing with here.  We could do
            // the work, but we have no good way of telling the user what went wrong.
            // Let the dialog explain it.

            //// Compute exclusive end point of selected range.
            //int nextOffset = lastOffset + CodeLineList[lastIndex].OffsetSpan;

            //if (!mProject.AddrMap.IsRangeUnbroken(firstOffset, nextOffset - firstOffset)) {
            //    Debug.WriteLine("Found mid-selection AddressMap entry (len=" +
            //        (nextOffset - firstOffset) + ")");
            //    return false;
            //}

            //Debug.WriteLine("First +" + firstOffset.ToString("x6") +
            //    ", last +" + lastOffset.ToString("x6") + ",next +" + nextOffset.ToString("x6"));

            return true;
        }

        public void EditAddress() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int lastIndex = mMainWin.CodeListView_GetLastSelectedIndex();
            int firstOffset = CodeLineList[selIndex].FileOffset;
            int lastOffset = CodeLineList[lastIndex].FileOffset;
            int nextOffset = lastOffset + CodeLineList[lastIndex].OffsetSpan;

            // The offset of an arend directive is the last byte in the address region.  It
            // has a span length of zero because it's a directive, so if it's selected as
            // the last offset then our nextOffset calculation will be off by one.  (This would
            // be avoided by using an exclusive end offset, but that causes other problems.)
            // Work around it here.
            if (CodeLineList[lastIndex].LineType == LineListGen.Line.Type.ArEndDirective) {
                nextOffset++;
            }

            // Compute length of selection.  May be zero if it's entirely arstart/arend.
            int selectedLen = nextOffset - firstOffset;

            AddressMap.AddressRegion curRegion;
            if (CodeLineList[selIndex].LineType == LineListGen.Line.Type.ArStartDirective ||
                    CodeLineList[selIndex].LineType == LineListGen.Line.Type.ArEndDirective) {
                // First selected line was arstart/arend, find the address map entry.
                curRegion = CodeLineList.GetAddrRegionFromLine(CodeLineList[selIndex],
                    out bool isSynth);
                Debug.Assert(curRegion != null);
                if (isSynth) {
                    // Synthetic regions are created for non-addressable "holes" in the map.
                    // They're not part of the map, so this is a create operation rather than
                    // a resize operation.
                    curRegion = null;
                    Debug.WriteLine("Ignoring synthetic region");
                } else {
                    Debug.WriteLine("Using region from " + CodeLineList[selIndex].LineType +
                        ": " + curRegion);
                }
            } else {
                if (selectedLen == 0) {
                    // A length of zero is only possible if nothing but directives were selected,
                    // but since the first entry wasn't arstart/arend this can't happen.
                    Debug.Assert(false);
                    return;
                }
                curRegion = null;
            }

            AddressMap.AddressMapEntry newEntry = null;
            if (curRegion == null) {
                // No entry, create a new one.  Use the current address as the default value,
                // unless the region is non-addressable.
                Anattrib attr = mProject.GetAnattrib(firstOffset);
                int addr;
                if (attr.IsNonAddressable) {
                    addr = Address.NON_ADDR;
                } else {
                    addr = attr.Address;
                }

                // Create a prototype entry with the various values.
                newEntry = new AddressMap.AddressMapEntry(firstOffset, selectedLen, addr);
                Debug.WriteLine("New entry prototype: " + newEntry);
            }

            EditAddress dlg = new EditAddress(mMainWin, curRegion, newEntry,
                selectedLen, firstOffset == lastOffset, mProject, mFormatter);
            if (dlg.ShowDialog() != true) {
                return;
            }

            ChangeSet cs = new ChangeSet(1);
            if (curRegion != dlg.ResultEntry) {
                UndoableChange uc = UndoableChange.CreateAddressChange(curRegion, dlg.ResultEntry);
                cs.Add(uc);
            }
            if (cs.Count > 0) {
                ApplyUndoableChanges(cs);
            } else {
                Debug.WriteLine("EditAddress: no changes");
            }
        }

        public bool CanEditComment() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            // Line must be code or data.
            return (SelectionAnalysis.mLineType == LineListGen.Line.Type.Code ||
                SelectionAnalysis.mLineType == LineListGen.Line.Type.Data);
        }

        public void EditComment() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;

            string oldComment = mProject.Comments[offset];
            EditComment dlg = new EditComment(mMainWin, oldComment);
            if (dlg.ShowDialog() == true) {
                if (!oldComment.Equals(dlg.CommentText)) {
                    Debug.WriteLine("Changing comment at +" + offset.ToString("x6"));

                    UndoableChange uc = UndoableChange.CreateCommentChange(offset,
                        oldComment, dlg.CommentText);
                    ChangeSet cs = new ChangeSet(uc);
                    ApplyUndoableChanges(cs);
                }
            }
        }

        public void EditHeaderComment() {
            EditLongComment(LineListGen.Line.HEADER_COMMENT_OFFSET);
        }

        public bool CanEditDataBank() {
            if (mProject.CpuDef.HasAddr16) {
                return false;   // only available for 65816
            }
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            return (SelectionAnalysis.mLineType == LineListGen.Line.Type.Code ||
                SelectionAnalysis.mLineType == LineListGen.Line.Type.DataBankDirective);
        }

        public void EditDataBank() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;

            // Get current user-specified value, or null.
            mProject.DbrOverrides.TryGetValue(offset, out CodeAnalysis.DbrValue curValue);

            EditDataBank dlg = new EditDataBank(mMainWin, mProject, mFormatter, curValue);
            if (dlg.ShowDialog() != true) {
                return;
            }

            if (dlg.Result != curValue) {
                Debug.WriteLine("Changing DBR at +" + offset.ToString("x6") + " to $" + dlg.Result);
                UndoableChange uc =
                    UndoableChange.CreateDataBankChange(offset, curValue, dlg.Result);
                ChangeSet cs = new ChangeSet(uc);
                ApplyUndoableChanges(cs);
            }
        }

        public bool CanEditLabel() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            EntityCounts counts = SelectionAnalysis.mEntityCounts;
            // Single line, code or data.
            return (counts.mDataLines > 0 || counts.mCodeLines > 0);
        }

        public void EditLabel() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;
            DoEditLabel(offset);
        }
        private void DoEditLabel(int offset) {
            Anattrib attr = mProject.GetAnattrib(offset);
            int addr = attr.Address;
            if (attr.IsNonAddressable) {
                addr = Address.NON_ADDR;
            }
            EditLabel dlg = new EditLabel(mMainWin, attr.Symbol, addr, offset,
                mProject.SymbolTable, mFormatter);
            if (dlg.ShowDialog() != true) {
                return;
            }

            // NOTE: if label matching is case-insensitive, we want to allow a situation
            // where a label is being renamed from "FOO" to "Foo".  (We should be able to
            // test for object equality on the Symbol.)
            if (attr.Symbol != dlg.LabelSym) {
                Debug.WriteLine("Changing label at offset +" + offset.ToString("x6"));

                // For undo/redo, we want to update the UserLabels value.  This may
                // be different from the Anattrib symbol, which can have an auto-generated
                // value.
                Symbol oldUserValue = null;
                if (mProject.UserLabels.ContainsKey(offset)) {
                    oldUserValue = mProject.UserLabels[offset];
                }
                if (oldUserValue == dlg.LabelSym) {
                    // Only expected when attr.Symbol is Auto
                    Debug.Assert(attr.Symbol.SymbolSource == Symbol.Source.Auto);
                    Debug.Assert(oldUserValue == null);
                    Debug.WriteLine("Ignoring attempt to delete an auto label");
                } else {
                    UndoableChange uc = UndoableChange.CreateLabelChange(offset,
                        oldUserValue, dlg.LabelSym);
                    ChangeSet cs = new ChangeSet(uc);
                    ApplyUndoableChanges(cs);
                }
            }
        }

        /// <summary>
        /// Determines the address and offset referenced by an instruction or data operand.
        /// </summary>
        /// <remarks>
        /// <para>There are five distinct situations (see issue #166):</para>
        /// <list type="number">
        ///   <item>instruction, target address is inside file</item>
        ///   <item>instruction, target address is outside file</item>
        ///   <item>instruction, target is a zero-page address currently defined in an LVT</item>
        ///   <item>data item with address operand, target address is inside file</item>
        ///   <item>data item with address operand, target address is outside file</item>
        /// </list>
        /// <para>For the case of wanting to edit the label at the target location, the caller
        /// will either need to open the LVT editor, the user label editor, or the project
        /// symbol editor.  Determining priority in the event of a conflict is up to the
        /// caller.</para>
        /// <para>It is possible to have an LVT entry and also an internal or external reference
        /// to zero-page (e.g. zero page is included in the file, or there's a project/platform
        /// symbol for the address).  We don't currently try to handle LVT entries.</para>
        /// <para>Sometimes instruction operands are formatted with an explicit symbol.  We pay
        /// no attention to that here.  This causes us to differ from the Ctrl+J "jump to operand"
        /// behavior, which prefers to jump to symbols when available.</para>
        /// <para>This currently ignores explicit symbolic references, and goes directly to
        /// the address referenced.  If you have a jump table that includes address FOO, but
        /// encodes it as FOO-1 (for RTS), we will set a label on FOO-1.</para>
        /// </remarks>
        /// <param name="project">Disassembly project.</param>
        /// <param name="selOffset">File offset of selection.</param>
        /// <param name="isInternal">Result: true if target is internal.</param>
        /// <param name="internalTargetOffset">Result: offset of target in project file.</param>
        /// <param name="externalSym">Result: first matching project/platform symbol,
        ///     if any.</param>
        /// <param name="externalAddr">Result: decoded external address.</param>
        /// <returns>True if a target was found.</returns>
        public static bool GetOperandTargetOffset(DisasmProject project, int selOffset,
                out bool isInternal, out int internalTargetOffset, out Symbol externalSym,
                out int externalAddr, out bool isLV) {
            isInternal = isLV = false;
            internalTargetOffset = externalAddr = -1;
            externalSym = null;

            if (selOffset < 0) {
                // Header comment or equate selected.
                return false;
            }

            Anattrib attr = project.GetAnattrib(selOffset);
            if (attr.IsInstructionStart) {
                if (!attr.IsInstructionWithOperand) {
                    return false;
                }
                if (attr.OperandOffset >= 0) {
                    // Internal address.  Walk back to start of line if necessary.
                    isInternal = true;
                    internalTargetOffset =
                        DataAnalysis.GetBaseOperandOffset(project, attr.OperandOffset);
                } else if (attr.OperandAddress >= 0) {
                    // External address.  Do a symbol table lookup, which will give us the correct
                    // answer when there are overlapping multi-byte values and masks.  If we don't
                    // find a match, we still want to return "true" so that the caller can offer
                    // to create a new project symbol.
                    //
                    // TODO: this symbol table lookup call does not correctly handle isolated
                    //       address regions.  It can return a user label with a matching address
                    //       that shouldn't be visible, because user labels have higher priority
                    //       than project symbols.  We currently work around this by ignoring
                    //       user label results.
                    Symbol sym = project.SymbolTable.FindNonVariableByAddress(attr.OperandAddress,
                        OpDef.MemoryEffect.ReadModifyWrite);    // could get effect from op
                    externalAddr = attr.OperandAddress;
                    if (sym is DefSymbol) {
                        externalSym = sym;
                    } else {
                        // This can happen if we're in an isolated address region that blocks
                        // output resolution.  Ignore the result.  (We still set externalAddr for
                        // the benefit of the project symbol creation dialog's initial values.)
                    }
                } else {
                    // Probably an immediate operand, nothing to do.
                    return false;
                }

                // See if a local variable is defined for this operand.
                // TODO(maybe): we don't currently get this far for 65816 stack relative stuff.
                OpDef opDef = project.CpuDef.GetOpDef(project.FileData[selOffset]);
                if ((opDef.IsDirectPageInstruction || opDef.IsStackRelInstruction) &&
                        attr.DataDescriptor != null && attr.DataDescriptor.HasSymbol &&
                        attr.DataDescriptor.SymbolRef.IsVariable) {
                    // The operand is formatted as a local variable.
                    isLV = true;
                }

                //Debug.WriteLine("Instr ref: offset=+" + internalTargetOffset.ToString("x6") +
                //    " extSym=" + externalSym);
                return true;
            } else if (attr.IsDataStart || attr.IsInlineDataStart) {
                // If it's address or symbol, get the target offset.  This only works for
                // internal addresses.
                int operandOffset = DataAnalysis.GetDataOperandOffset(project, selOffset,
                    out int extAddr);
                if (operandOffset >= 0) {
                    // Internal address reference.  Walk back to start of line if necessary.
                    isInternal = true;
                    internalTargetOffset =
                        DataAnalysis.GetBaseOperandOffset(project, operandOffset);
                } else if (extAddr >= 0) {
                    // External address reference.
                    externalSym = project.SymbolTable.FindNonVariableByAddress(extAddr,
                        OpDef.MemoryEffect.ReadModifyWrite);    // mem effect unknowable
                    externalAddr = extAddr;
                } else {
                    return false;
                }
                //Debug.WriteLine("Data ref: offset=+" + internalTargetOffset.ToString("x6") +
                //    " extSym=" + externalSym);
                return true;
            } else {
                Debug.Assert(false, "should be some sort of start");
                return false;
            }
        }

        public bool CanEditOperandTargetLabel() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int selOffset = CodeLineList[selIndex].FileOffset;
            return GetOperandTargetOffset(mProject, selOffset, out bool unused1,
                out int unused2, out Symbol unused3, out int unused4, out bool unused5);
        }

        public void EditOperandTargetLabel() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int selOffset = CodeLineList[selIndex].FileOffset;
            if (!GetOperandTargetOffset(mProject, selOffset, out bool isInternal,
                    out int internalLabelOffset, out Symbol externalSym, out int externalAddr,
                    out bool isLV)) {
                Debug.Assert(false, "should not be here");
                return;
            }
            Debug.WriteLine("EOTL: isInternal=" + isInternal + " intOff=+" +
                internalLabelOffset.ToString("x6") + " extSym=" + externalSym +
                " extAddr=" + externalAddr + " isLV=" + isLV);

            // Check to see if a symbol has been explicitly set.  If so, we want to edit the
            // label at the address for that symbol, or the project symbol entry, rather than
            // using the numeric value of the operand to do the determination.  This feels better
            // because you're editing the label shown on screen in the operand field.
            if (mProject.OperandFormats.TryGetValue(selOffset, out FormatDescriptor dfd) &&
                    dfd.FormatType == FormatDescriptor.Type.NumericLE &&
                    dfd.FormatSubType == FormatDescriptor.SubType.Symbol) {
                // Symbol formats are weak references, so we need to look the label up in the
                // symbol table to ensure that it exists.  Non-local label references will have
                // the uniquification string appended.
                string label = dfd.SymbolRef.Label;
                bool didEdit = false;
                if (mProject.SymbolTable.TryGetNonVariableValue(label, out Symbol foundSym)) {
                    if (foundSym.SymbolSource == Symbol.Source.User) {
                        // Edit the label at the target address.
                        int labelOffset = mProject.FindLabelOffsetByName(label);
                        if (labelOffset >= 0) {
                            DoEditLabel(labelOffset);
                            didEdit = true;
                        } else {
                            Debug.Assert(false, "Can't edit symbol " + foundSym);
                        }
                    } else if (foundSym.SymbolSource == Symbol.Source.Project) {
                        // Edit the symbol we found.
                        Debug.Assert(foundSym is DefSymbol);
                        DoEditProjectSymbol(CodeListColumn.Label, (DefSymbol)foundSym);
                        didEdit = true;
                    } else {
                        // Could be a platform symbol, or we managed to get something odd in here.
                        // Whatever the case, fall through to the "uneditable" case.
                    }
                }
                if (!didEdit) {
                    // If we can't edit it for whatever reason, just open the appropriate Edit
                    // Operand dialog, so they can fix the symbol name.  Seems more appropriate
                    // than posting an error dialog or quietly doing nothing.
                    Debug.WriteLine("Unable to cleverly edit '" + label + "', doing EO");
                    EditOperand();
                }
                return;
            }

            // Operand targets can be internal or external, and can be in an LVT.  Internal
            // address user labels have the highest priority, LVTs are next, then external
            // project/platform symbols.
            if (isInternal) {
                Debug.Assert(internalLabelOffset >= 0);
                DoEditLabel(internalLabelOffset);
            } else if (isLV) {
                // Formatted as a local variable, edit individual symbol.
                DoEditLVOperand(selOffset);
            } else {
                // Create or edit project symbol.
                if (externalSym != null && externalSym.SymbolSource == Symbol.Source.Project) {
                    // Edit existing project symbol.
                    DoEditProjectSymbol(CodeListColumn.Label, (DefSymbol)externalSym);
                } else {
                    // Platform symbol or nothing at all.  Create a new project symbol.
                    // (Should we refuse to act if there's only a platform symbol?  Could be
                    // confusing if they think they're editing the platform, but it's also
                    // convenient to be able to create the project sym here.)
                    DefSymbol initVals = new DefSymbol("SYM", externalAddr, Symbol.Source.Project,
                        Symbol.Type.ExternalAddr, FormatDescriptor.SubType.None);
                    DoEditProjectSymbol(CodeListColumn.Label, initVals);
                }
            }
        }

        /// <summary>
        /// Edits the local variable referenced by an instruction operand.
        /// </summary>
        /// <remarks>
        /// This is used by the "edit operand target label" feature.  Double-clicking on an LVT
        /// entry edits the whole table rather than a single entry.
        /// </remarks>
        /// <param name="selOffset">Offset of instruction.</param>
        private void DoEditLVOperand(int selOffset) {
            OpDef opDef = mProject.CpuDef.GetOpDef(mProject.FileData[selOffset]);
            if (!opDef.IsDirectPageInstruction && !opDef.IsStackRelInstruction) {
                Debug.Assert(false);
                return;
            }
            Anattrib attr = mProject.GetAnattrib(selOffset);
            if (!attr.DataDescriptor.SymbolRef.IsVariable) {
                Debug.Assert(false);
                return;
            }
            int operandValue = attr.OperandAddress;
            if (operandValue < 0) {
                Debug.Assert(false);
                return;
            }

            // Find the table in which the symbol is defined.
            LocalVariableLookup lvLookup =
                new LocalVariableLookup(mProject.LvTables, mProject, null, false, false);
            int lvTableOffset = lvLookup.GetDefiningTableOffset(selOffset,
                attr.DataDescriptor.SymbolRef);
            if (lvTableOffset < 0) {
                Debug.Assert(false, "LV table not found");
                return;
            }
            LocalVariableTable lvt = mProject.LvTables[lvTableOffset];

            // The SymbolRef is a weak reference, by label.  Get the actual symbol definition
            // from the table.  Because of the de-duplication stuff, we need to use the general
            // LV lookup function (I think).
            DefSymbol sym = lvLookup.GetSymbol(selOffset, operandValue,
                opDef.IsDirectPageInstruction ? Symbol.Type.ExternalAddr : Symbol.Type.Constant);
            if (sym == null) {
                Debug.Assert(false, "LV sym not found");
                return;
            }
            // Un-de-duplicate the label.
            DefSymbol origSym = lvLookup.GetOriginalForm(sym);

            EditDefSymbol dlg = new EditDefSymbol(mMainWin, mFormatter,
                lvt.GetSortedByLabel(), origSym, origSym,
                mProject.SymbolTable, true, true);
            if (dlg.ShowDialog() == true) {
                if (origSym != dlg.NewSym) {
                    // A change as made.  Clone the table, replace the entry, and store the table.
                    Debug.Assert(dlg.NewSym != null);
                    ChangeSet cs = new ChangeSet(1);
                    LocalVariableTable editedLvTable = new LocalVariableTable(lvt);
                    editedLvTable.AddOrReplace(dlg.NewSym);
                    UndoableChange uc = UndoableChange.CreateLocalVariableTableChange(lvTableOffset,
                        lvt, editedLvTable);
                    cs.Add(uc);
                    ApplyUndoableChanges(cs);
                }
            }
        }

        public bool CanCreateLocalVariableTable() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            // Only allow on code lines.  This is somewhat arbitrary; data would work fine.
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            if (CodeLineList[selIndex].LineType != LineListGen.Line.Type.Code) {
                return false;
            }
            int offset = CodeLineList[selIndex].FileOffset;
            // Don't allow creation if a table already exists.
            return !mProject.LvTables.ContainsKey(offset);
        }

        public void CreateLocalVariableTable() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;
            Debug.Assert(!mProject.LvTables.ContainsKey(offset));
            CreateOrEditLocalVariableTable(offset);
        }

        public bool CanEditLocalVariableTable() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            // Check to see if the offset of the first-defined table is less than or equal to
            // the offset of the selected line.  If so, we know there's a table, though we
            // don't know which one.
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;
            return mProject.LvTables.Count > 0 && mProject.LvTables.Keys[0] <= offset;
        }

        public void EditLocalVariableTable() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;

            LocalVariableLookup lvLookup = new LocalVariableLookup(mProject.LvTables, mProject,
                null, false, false);
            int bestOffset = lvLookup.GetNearestTableOffset(offset);
            Debug.Assert(bestOffset >= 0);
            CreateOrEditLocalVariableTable(bestOffset);
        }

        private void CreateOrEditLocalVariableTable(int offset) {
            // Get existing table, if any.
            mProject.LvTables.TryGetValue(offset, out LocalVariableTable oldLvt);

            EditLocalVariableTable dlg = new EditLocalVariableTable(mMainWin, mProject,
                mFormatter, oldLvt, offset);
            if (dlg.ShowDialog() != true) {
                return;
            }
            if (offset != dlg.NewOffset) {
                // Table moved.  We create two changes, one to delete the current table, one
                // to create a new table.
                Debug.Assert(!mProject.LvTables.TryGetValue(dlg.NewOffset,
                    out LocalVariableTable unused));

                UndoableChange rem = UndoableChange.CreateLocalVariableTableChange(offset,
                    oldLvt, null);
                UndoableChange add = UndoableChange.CreateLocalVariableTableChange(dlg.NewOffset,
                    null, dlg.NewTable);
                ChangeSet cs = new ChangeSet(2);
                cs.Add(rem);
                cs.Add(add);
                ApplyUndoableChanges(cs);
            } else if (oldLvt != dlg.NewTable) {
                // New table, edited in place, or deleted.
                UndoableChange uc = UndoableChange.CreateLocalVariableTableChange(offset,
                    oldLvt, dlg.NewTable);
                ChangeSet cs = new ChangeSet(uc);
                ApplyUndoableChanges(cs);
            } else {
                Debug.WriteLine("LvTable unchanged");
            }
        }

        public bool CanEditLongComment() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            EntityCounts counts = SelectionAnalysis.mEntityCounts;
            // Single line, code or data, or a long comment.
            return (counts.mDataLines > 0 || counts.mCodeLines > 0 ||
                SelectionAnalysis.mLineType == LineListGen.Line.Type.LongComment);
        }

        public void EditLongComment() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;
            EditLongComment(offset);
        }

        private void EditLongComment(int offset) {
            EditLongComment dlg = new EditLongComment(mMainWin, mFormatter);
            if (mProject.LongComments.TryGetValue(offset, out MultiLineComment oldComment)) {
                dlg.LongComment = oldComment;
            }
            if (dlg.ShowDialog() != true) {
                return;
            }

            MultiLineComment newComment = dlg.LongComment;
            if (oldComment != newComment) {
                Debug.WriteLine("Changing long comment at +" + offset.ToString("x6"));

                UndoableChange uc = UndoableChange.CreateLongCommentChange(offset,
                    oldComment, newComment);
                ChangeSet cs = new ChangeSet(uc);
                ApplyUndoableChanges(cs);
            }
        }

        public bool CanEditNote() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            EntityCounts counts = SelectionAnalysis.mEntityCounts;
            // Single line, code or data, or a note.
            return (counts.mDataLines > 0 || counts.mCodeLines > 0 ||
                SelectionAnalysis.mLineType == LineListGen.Line.Type.Note);
        }

        public void EditNote() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;

            MultiLineComment oldNote;
            if (!mProject.Notes.TryGetValue(offset, out oldNote)) {
                oldNote = null;
            }
            EditNote dlg = new EditNote(mMainWin, oldNote);
            dlg.ShowDialog();

            if (dlg.DialogResult != true) {
                return;
            }

            MultiLineComment newNote = dlg.Note;
            if (oldNote != newNote) {
                Debug.WriteLine("Changing note at +" + offset.ToString("x6"));

                UndoableChange uc = UndoableChange.CreateNoteChange(offset,
                    oldNote, newNote);
                ChangeSet cs = new ChangeSet(uc);
                ApplyUndoableChanges(cs);
            }
        }

        public bool CanEditOperand() {
            if (SelectionAnalysis.mNumItemsSelected == 0) {
                return false;
            } else if (SelectionAnalysis.mNumItemsSelected == 1) {
                int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
                int selOffset = CodeLineList[selIndex].FileOffset;

                bool editInstr = (CodeLineList[selIndex].LineType == LineListGen.Line.Type.Code &&
                    mProject.GetAnattrib(selOffset).IsInstructionWithOperand);
                bool editData = (CodeLineList[selIndex].LineType == LineListGen.Line.Type.Data);
                return editInstr || editData;
            } else {
                // Data operands are one of the few things we can edit in bulk.  It's okay
                // if meta-data like ORGs and Notes are selected, but we don't allow it if
                // any code is selected.
                EntityCounts counts = SelectionAnalysis.mEntityCounts;
                return (counts.mDataLines > 0 && counts.mCodeLines == 0);
            }
        }

        public void EditOperand() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int selOffset = CodeLineList[selIndex].FileOffset;
            if (CodeLineList[selIndex].LineType == LineListGen.Line.Type.Code) {
                EditInstructionOperand(selOffset);
            } else {
                // We allow the selection to include meta-data like .org and Notes.
                //Debug.Assert(CodeLineList[selIndex].LineType == LineListGen.Line.Type.Data);
                EditDataOperand();
            }
        }

        private void EditInstructionOperand(int offset) {
            EditInstructionOperand dlg = new EditInstructionOperand(mMainWin, mProject,
                offset, mFormatter);
            if (dlg.ShowDialog() != true) {
                return;
            }

            ChangeSet cs = new ChangeSet(1);
            mProject.OperandFormats.TryGetValue(offset, out FormatDescriptor dfd);
            if (dlg.FormatDescriptorResult != dfd) {
                UndoableChange uc = UndoableChange.CreateOperandFormatChange(offset,
                    dfd, dlg.FormatDescriptorResult);
                cs.Add(uc);
            } else {
                Debug.WriteLine("No change to operand format");
            }

            // Check for changes to a local variable table.  The edit dialog can't delete an
            // entire table, so a null value here means no changes were made.
            if (dlg.LocalVariableResult != null) {
                int tableOffset = dlg.LocalVariableTableOffsetResult;
                LocalVariableTable lvt = mProject.LvTables[tableOffset];
                Debug.Assert(lvt != null);  // cannot create a table either
                UndoableChange uc = UndoableChange.CreateLocalVariableTableChange(tableOffset,
                    lvt, dlg.LocalVariableResult);
                cs.Add(uc);
            } else {
                Debug.WriteLine("No change to LvTable");
            }

            // Check for changes to label at operand target address.  Labels can be created,
            // modified, or deleted.
            if (dlg.SymbolEditOffsetResult >= 0) {
                mProject.UserLabels.TryGetValue(dlg.SymbolEditOffsetResult, out Symbol oldLabel);
                UndoableChange uc = UndoableChange.CreateLabelChange(dlg.SymbolEditOffsetResult,
                    oldLabel, dlg.SymbolEditResult);
                cs.Add(uc);
            } else {
                Debug.WriteLine("No change to label");
            }

            // Check for changes to a project symbol.  The dialog can create a new entry or
            // modify an existing entry, but can't delete an entry.
            if (dlg.ProjectSymbolResult != null) {
                DefSymbol oldSym = dlg.OrigProjectSymbolResult;
                DefSymbol newSym = dlg.ProjectSymbolResult;
                if (oldSym == newSym) {
                    Debug.WriteLine("No actual change to project symbol");
                } else {
                    // Generate a completely new set of project properties.
                    ProjectProperties newProps = new ProjectProperties(mProject.ProjectProps);
                    // Add new symbol entry, or replace existing entry.
                    if (oldSym != null) {
                        newProps.ProjectSyms.Remove(oldSym.Label);
                    }
                    newProps.ProjectSyms.Add(newSym.Label, newSym);
                    UndoableChange uc = UndoableChange.CreateProjectPropertiesChange(
                        mProject.ProjectProps, newProps);
                    cs.Add(uc);
                }
            } else {
                Debug.WriteLine("No change to project symbol");
            }

            Debug.WriteLine("EditInstructionOperand: " + cs.Count + " changes");
            if (cs.Count != 0) {
                ApplyUndoableChanges(cs);
            }
        }

        private void EditDataOperand() {
            Debug.Assert(mMainWin.CodeListView_GetSelectionCount() > 0);

            TypedRangeSet trs = GroupedOffsetSetFromSelected();
            if (trs.Count == 0) {
                Debug.Assert(false, "EditDataOperand found nothing to edit"); // shouldn't happen
                return;
            }

            // If the first offset has a FormatDescriptor, pass that in as a recommendation
            // for the default value in the dialog.  This allows single-item editing to work
            // as expected.  If the format can't be applied to the full selection (which
            // would disable that radio button), the dialog will have to pick something
            // that does work.
            //
            // We could pull this out of Anattribs, which would let the dialog reflect the
            // auto-format that the user was just looking at.  However, I think it's better
            // if the dialog shows what's actually there, i.e. no formatting at all.
            IEnumerator<TypedRangeSet.Tuple> iter =
                (IEnumerator<TypedRangeSet.Tuple>)trs.GetEnumerator();
            iter.MoveNext();
            TypedRangeSet.Tuple firstOffset = iter.Current;
            mProject.OperandFormats.TryGetValue(firstOffset.Value, out FormatDescriptor dfd);

            EditDataOperand dlg =
                new EditDataOperand(mMainWin, mProject, mFormatter, trs, dfd);
            if (dlg.ShowDialog() == true) {
                // Merge the changes into the OperandFormats list.  We need to remove all
                // FormatDescriptors that overlap the selected region.  We don't need to
                // pass the selection set in, because the dlg.Results list spans the exact
                // set of ranges.
                //
                // If nothing actually changed, don't generate an undo record.
                ChangeSet cs = mProject.GenerateFormatMergeSet(dlg.Results);
                if (cs.Count != 0) {
                    ApplyUndoableChanges(cs);
                } else {
                    Debug.WriteLine("No change to data formats");
                }
            }

        }

        public void EditProjectProperties(WpfGui.EditProjectProperties.Tab initialTab) {
            string projectDir = string.Empty;
            if (!string.IsNullOrEmpty(mProjectPathName)) {
                projectDir = Path.GetDirectoryName(mProjectPathName);
            }
            EditProjectProperties dlg = new EditProjectProperties(mMainWin, mProject,
                projectDir, mFormatter, initialTab);
            dlg.ShowDialog();
            ProjectProperties newProps = dlg.NewProps;

            // The dialog result doesn't matter, because the user might have hit "apply"
            // before hitting "cancel".
            if (newProps != null) {
                UndoableChange uc = UndoableChange.CreateProjectPropertiesChange(
                    mProject.ProjectProps, newProps);
                ApplyUndoableChanges(new ChangeSet(uc));
            }
        }

        public bool CanEditProjectSymbol() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            if (SelectionAnalysis.mLineType != LineListGen.Line.Type.EquDirective) {
                return false;
            }
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int symIndex = LineListGen.DefSymIndexFromOffset(CodeLineList[selIndex].FileOffset);
            DefSymbol defSym = mProject.ActiveDefSymbolList[symIndex];
            return (defSym.SymbolSource == Symbol.Source.Project);
        }

        public void EditProjectSymbol(CodeListColumn col) {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int symIndex = LineListGen.DefSymIndexFromOffset(CodeLineList[selIndex].FileOffset);
            DefSymbol origDefSym = mProject.ActiveDefSymbolList[symIndex];
            DoEditProjectSymbol(col, origDefSym);
        }
        private void DoEditProjectSymbol(CodeListColumn col, DefSymbol origDefSym) {
            Debug.Assert(origDefSym.SymbolSource == Symbol.Source.Project);

            EditDefSymbol dlg = new EditDefSymbol(mMainWin, mFormatter,
                mProject.ProjectProps.ProjectSyms, origDefSym, origDefSym, null);

            switch (col) {
                case CodeListColumn.Operand:
                    dlg.InitialFocusField = EditDefSymbol.InputField.Value;
                    break;
                case CodeListColumn.Comment:
                    dlg.InitialFocusField = EditDefSymbol.InputField.Comment;
                    break;
                case CodeListColumn.Label:
                default:
                    dlg.InitialFocusField = EditDefSymbol.InputField.Label;
                    break;
            }

            if (dlg.ShowDialog() == true) {
                ProjectProperties newProps = new ProjectProperties(mProject.ProjectProps);
                newProps.ProjectSyms.Remove(origDefSym.Label);
                newProps.ProjectSyms[dlg.NewSym.Label] = dlg.NewSym;

                UndoableChange uc = UndoableChange.CreateProjectPropertiesChange(
                    mProject.ProjectProps, newProps);
                ChangeSet cs = new ChangeSet(uc);
                ApplyUndoableChanges(cs);
            }
        }

        public bool CanEditStatusFlags() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            // Single line, must be code or a RegWidth directive.
            return (SelectionAnalysis.mLineType == LineListGen.Line.Type.Code ||
                SelectionAnalysis.mLineType == LineListGen.Line.Type.RegWidthDirective);
        }

        public void EditStatusFlags() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;

            EditStatusFlags dlg = new EditStatusFlags(mMainWin,
                mProject.StatusFlagOverrides[offset], mProject.CpuDef.HasEmuFlag);
            if (dlg.ShowDialog() != true) {
                return;
            }

            if (dlg.FlagValue != mProject.StatusFlagOverrides[offset]) {
                UndoableChange uc = UndoableChange.CreateStatusFlagChange(offset,
                    mProject.StatusFlagOverrides[offset], dlg.FlagValue);
                ChangeSet cs = new ChangeSet(uc);
                ApplyUndoableChanges(cs);
            }
        }

        public bool CanEditVisualizationSet() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            // Single line, must be a visualization set or a place where one can be created.
            LineListGen.Line.Type lineType = SelectionAnalysis.mLineType;
            return (lineType == LineListGen.Line.Type.VisualizationSet ||
                    lineType == LineListGen.Line.Type.Code ||
                    lineType == LineListGen.Line.Type.Data);
        }

        public void EditVisualizationSet() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;
            mProject.VisualizationSets.TryGetValue(offset, out VisualizationSet curVisSet);

            EditVisualizationSet dlg = new EditVisualizationSet(mMainWin, mProject,
                mFormatter, curVisSet, offset);
            if (dlg.ShowDialog() != true) {
                return;
            }
            VisualizationSet newSet = dlg.NewVisSet;
            if (newSet.Count == 0) {
                // empty sets are deleted
                newSet = null;
            }
            if (curVisSet != newSet) {
                ChangeSet cs = new ChangeSet(1);

                // New table, edited in place, or deleted.
                UndoableChange uc = UndoableChange.CreateVisualizationSetChange(offset,
                    curVisSet, newSet);
                //Debug.WriteLine("Change " + curVisSet + " to " + newSet);
                cs.Add(uc);

                // And now the messy bit.  If Visualizations were removed, we need to purge
                // them from any animations that reference them.  The edit dialog took care
                // of this for animations in the same set, but we need to check other sets.
                foreach (KeyValuePair<int, VisualizationSet> kvp in mProject.VisualizationSets) {
                    if (kvp.Value == curVisSet) {
                        continue;
                    }

                    VisualizationSet stripSet;
                    if (VisualizationSet.StripEntriesFromAnimations(kvp.Value, dlg.RemovedSerials,
                            out stripSet)) {
                        if (stripSet.Count == 0) {
                            stripSet = null;
                        }
                        uc = UndoableChange.CreateVisualizationSetChange(kvp.Key,
                            kvp.Value, stripSet);
                        cs.Add(uc);
                        Debug.WriteLine("Also updating visSet at +" + kvp.Key.ToString("x6"));
                    }
                }

                ApplyUndoableChanges(cs);
            } else {
                Debug.WriteLine("No change to VisualizationSet");
            }
        }

        public void Export() {
            string outName;
            if (string.IsNullOrEmpty(mProjectPathName)) {
                outName = Path.GetFileName(mDataPathName);
            } else {
                outName = Path.GetFileName(mProjectPathName);
            }

            Export dlg = new Export(mMainWin, outName);
            if (dlg.ShowDialog() == false) {
                return;
            }

            int[] rightWidths = new int[] {
                dlg.AsmLabelColWidth, dlg.AsmOpcodeColWidth,
                dlg.AsmOperandColWidth, dlg.AsmCommentColWidth
            };
            Exporter eport = new Exporter(mProject, CodeLineList, mFormatter,
                dlg.ColFlags, rightWidths);
            eport.IncludeNotes = dlg.IncludeNotes;
            eport.GenerateImageFiles = dlg.GenerateImageFiles;
            eport.LongLabelNewLine = dlg.LongLabelNewLine;
            if (dlg.SelectionOnly) {
                DisplayListSelection selection = mMainWin.CodeDisplayList.SelectedIndices;
                if (selection.Count == 0) {
                    // no selection == select all
                    selection = null;
                }
                eport.Selection = selection;
            }

            if (dlg.GenType == WpfGui.Export.GenerateFileType.Html) {
                // Generating wireframe animations can be slow, so we need to use a
                // progress dialog.
                eport.OutputToHtml(mMainWin, dlg.PathName, dlg.OverwriteCss);
            } else {
                // Text output is generally very fast.  Put up a wait cursor just in case.
                try {
                    Mouse.OverrideCursor = Cursors.Wait;
                    eport.OutputToText(dlg.PathName, dlg.TextModeCsv);
                } finally {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        public void GenerateLabels() {
            GenerateLabels dlg = new GenerateLabels(mMainWin);
            if (dlg.ShowDialog() == false) {
                return;
            }

            string ext;
            string filter;
            switch (dlg.Format) {
                case LabelFileGenerator.LabelFmt.VICE:
                    ext = ".lbl";
                    filter = "VICE labels (*.lbl)|*.lbl";
                    break;
                default:
                    Debug.Assert(false, "bad format");
                    return;
            }

            SaveFileDialog fileDlg = new SaveFileDialog() {
                Filter = filter,
                FilterIndex = 1,
                ValidateNames = true,
                AddExtension = true,    // doesn't add extension if non-ext file exists
                FileName = "labels" + ext
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }
            string pathName = Path.GetFullPath(fileDlg.FileName);
            try {
                using (StreamWriter writer = new StreamWriter(pathName, false)) {
                    LabelFileGenerator gen = new LabelFileGenerator(mProject,
                        dlg.Format, dlg.IncludeAutoLabels);
                    gen.Generate(writer);
                }
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message, "Failed", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void Find() {
            FindBox dlg = new FindBox(mMainWin, mFindString, false);
            if (dlg.ShowDialog() == true) {
                mFindString = dlg.TextToFind;
                mFindStartIndex = -1;
                FindText(dlg.IsBackward, true);
            }
        }

        public void FindNext() {
            FindText(false, false);
        }

        public void FindPrevious() {
            FindText(true, false);
        }

        private void FindText(bool goBackward, bool pushLocation) {
            if (string.IsNullOrEmpty(mFindString)) {
                return;
            }
            int incr = goBackward ? -1 : 1;

            // If we reversed direction, reset the "start index" so we don't tell the user
            // we've wrapped around.
            if (mFindBackward != goBackward) {
                mFindStartIndex = -1;
                mFindBackward = goBackward;
            }

            // Start from the topmost selected line, or the start of the file if nothing
            // is selected.
            // TODO(maybe): if multiple lines are selected, search only within the selected set.
            int index = mMainWin.CodeListView_GetFirstSelectedIndex();
            if (index < 0) {
                index = 0;
            }

            // Start one past the selected item.
            index += incr;
            if (index == CodeLineList.Count) {
                index = 0;
            } else if (index == -1) {
                index = CodeLineList.Count - 1;
            }
            //Debug.WriteLine("FindText index=" + index + " start=" + mFindStartIndex +
            //    " str=" + mFindString);
            while (index != mFindStartIndex) {
                if (mFindStartIndex < 0) {
                    // need to latch this inside the loop so the initial test doesn't fail
                    mFindStartIndex = index;
                }

                string searchStr = CodeLineList.GetSearchString(index);
                int matchPos = searchStr.IndexOf(mFindString,
                    StringComparison.InvariantCultureIgnoreCase);
                if (matchPos >= 0) {
                    //Debug.WriteLine("Match " + index + ": " + searchStr);
                    if (pushLocation) {
                        mNavStack.Push(GetCurrentlySelectedLocation());
                    }

                    mMainWin.CodeListView_EnsureVisible(index);
                    mMainWin.CodeListView_SelectRange(index, 1);
                    mMainWin.CodeListView_SetSelectionFocus();
                    return;
                }

                index += incr;
                if (index == CodeLineList.Count) {
                    index = 0;
                } else if (index == -1) {
                    index = CodeLineList.Count - 1;
                }
            }

            // Announce that we've wrapped around, then clear the start index.
            MessageBox.Show(Res.Strings.FIND_REACHED_START,
                Res.Strings.FIND_REACHED_START_CAPTION, MessageBoxButton.OK,
                MessageBoxImage.Information);
            mFindStartIndex = -1;

            //mMainWin.CodeListView_Focus();
        }

        // Finds all matches in the file.  Does not alter the the current position.
        public void FindAll() {
            FindBox dlg = new FindBox(mMainWin, mFindString, true);
            if (dlg.ShowDialog() == false) {
                return;
            }
            mFindString = dlg.TextToFind;
            string SEARCH_SEP_STR = "" + LineListGen.SEARCH_SEP;

            List<ReferenceTable.ReferenceTableItem> items =
                new List<ReferenceTable.ReferenceTableItem>();
            for (int index = 0; index < CodeLineList.Count; index++) {
                string searchStr = CodeLineList.GetSearchString(index);
                int matchPos = searchStr.IndexOf(mFindString,
                    StringComparison.InvariantCultureIgnoreCase);
                if (matchPos >= 0) {
                    int offset = CodeLineList[index].FileOffset;
                    string offsetStr, addrStr, msgStr;

                    if (offset >= 0) {
                        offsetStr = mFormatter.FormatOffset24(offset);
                        Anattrib attr = mProject.GetAnattrib(offset);
                        if (attr.Address >= 0) {
                            addrStr = mFormatter.FormatAddress(attr.Address, attr.Address > 0xffff);
                        } else {
                            addrStr = string.Empty;
                        }
                    } else {
                        offsetStr = "-";
                        addrStr = string.Empty;
                    }
                    msgStr = searchStr.Replace(SEARCH_SEP_STR, "  ");
                    // Create a reference table entry.
                    int lineDelta = index - CodeLineList.FindLineIndexByOffset(offset);
                    bool isNote = (CodeLineList[index].LineType == LineListGen.Line.Type.Note);
                    NavStack.Location loc = new NavStack.Location(offset, lineDelta,
                        isNote ? NavStack.GoToMode.JumpToNote : NavStack.GoToMode.JumpToAdjIndex);

                    items.Add(new ReferenceTable.ReferenceTableItem(loc,
                        offsetStr, addrStr, msgStr));
                }
            }

            if (items.Count > 0) {
                ShowReferenceTable(items);
            } else {
                MessageBox.Show(Res.Strings.FIND_ALL_NO_MATCH,
                    Res.Strings.FIND_ALL_CAPTION, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        public bool CanFormatAsWord() {
            EntityCounts counts = SelectionAnalysis.mEntityCounts;
            // This is insufficient -- we need to know how many bytes are selected, and
            // whether they're already formatted as multi-byte items.  Too expensive to
            // deal with here, so we'll need to show failure dialogs instead (ugh).
            return (counts.mDataLines > 0 && counts.mCodeLines == 0);
        }

        public void FormatAsWord() {
            TypedRangeSet trs = GroupedOffsetSetFromSelected();
            if (trs.Count == 0) {
                Debug.Assert(false, "nothing to edit");         // shouldn't happen
                return;
            }

            // If the user has only selected a single byte, we want to add the following byte
            // to the selection, then proceed as usual.  We can't simply modify the ListView
            // selection because the following item might be an auto-detected string or fill,
            // and we'd be adding multiple bytes.  We have to be careful when grabbing the byte
            // in case there's a region-split at that point (e.g. user label or .ORG).
            //
            // We could expand this to allow multiple regions, each of which is a single byte,
            // but we'd need to deal with the case where the user selects two adjacent bytes that
            // cross a region boundary.
            if (trs.RangeCount == 1) {
                // Exactly one range entry.  Check its size.
                IEnumerator<TypedRangeSet.TypedRange> checkIter = trs.RangeListIterator;
                checkIter.MoveNext();
                TypedRangeSet.TypedRange rng = checkIter.Current;
                if (rng.Low == rng.High && rng.Low < mProject.FileDataLength - 1) {
                    // Single byte selected.  Check to see if it's okay to grab the next byte.
                    Anattrib thisAttr = mProject.GetAnattrib(rng.Low);
                    Debug.Assert(thisAttr.DataDescriptor.Length == 1);

                    int nextOffset = rng.Low + 1;
                    // This must match what GroupedOffsetSetFromSelected() does.
                    if (!mProject.UserLabels.ContainsKey(nextOffset) &&
                            !mProject.HasCommentNoteOrVis(nextOffset) &&
                            mProject.AddrMap.IsRangeUnbroken(nextOffset - 1, 2)) {
                        // Good to go.
                        Debug.WriteLine("Grabbing second byte from +" + nextOffset.ToString("x6"));
                        trs.Add(nextOffset, rng.Type);
                    }
                }
            }

            // Confirm that every selected byte is a single-byte data item (either set by
            // the user or as part of the uncategorized data scan).
            foreach (TypedRangeSet.Tuple tup in trs) {
                FormatDescriptor dfd = mProject.GetAnattrib(tup.Value).DataDescriptor;
                if (dfd != null && dfd.Length != 1) {
                    Debug.WriteLine("Can't format as word: offset +" + tup.Value.ToString("x6") +
                        " has len=" + dfd.Length + " (must be 1)");
                    MessageBox.Show(Res.Strings.INVALID_FORMAT_WORD_SEL_NON1,
                        Res.Strings.INVALID_FORMAT_WORD_SEL_CAPTION,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Confirm that, in each region, an even number of bytes are selected.
            IEnumerator<TypedRangeSet.TypedRange> rngIter = trs.RangeListIterator;
            while (rngIter.MoveNext()) {
                TypedRangeSet.TypedRange rng = rngIter.Current;
                int rangeLen = rng.High - rng.Low + 1;
                if ((rangeLen & 0x01) != 0) {
                    string msg = string.Format(Res.Strings.INVALID_FORMAT_WORD_SEL_UNEVEN_FMT,
                        trs.RangeCount);
                    MessageBox.Show(msg,
                        Res.Strings.INVALID_FORMAT_WORD_SEL_CAPTION,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            // Selection is good, generate changes.
            SortedList<int, FormatDescriptor> newFmts = new SortedList<int, FormatDescriptor>();
            rngIter.Reset();
            FormatDescriptor newDfd = FormatDescriptor.Create(2, FormatDescriptor.Type.NumericLE,
                FormatDescriptor.SubType.None);
            while (rngIter.MoveNext()) {
                TypedRangeSet.TypedRange rng = rngIter.Current;
                for (int i = rng.Low; i <= rng.High; i += 2) {
                    newFmts.Add(i, newDfd);
                }
            }

            ChangeSet cs = mProject.GenerateFormatMergeSet(newFmts);
            if (cs.Count != 0) {
                ApplyUndoableChanges(cs);
            }
        }

        public bool CanFormatAddressTable() {
            EntityCounts counts = SelectionAnalysis.mEntityCounts;
            // Must be at least one line of data, and no code.  Note this is lines, not bytes,
            // so we can't screen out single-byte lines without additional work.
            return (counts.mDataLines > 0 && counts.mCodeLines == 0);
        }

        public void FormatAddressTable() {
            TypedRangeSet trs = GroupedOffsetSetFromSelected();
            if (trs.Count == 0) {
                // shouldn't happen
                Debug.Assert(false, "FormatSplitAddressTable found nothing to edit");
                return;
            }

            FormatAddressTable dlg = new FormatAddressTable(mMainWin, mProject, trs,
                mFormatter);

            dlg.ShowDialog();
            if (dlg.DialogResult != true) {
                return;
            }

            // Start with the format descriptors.
            ChangeSet cs = mProject.GenerateFormatMergeSet(dlg.NewFormatDescriptors);

            // Add in the user labels.
            foreach (KeyValuePair<int, Symbol> kvp in dlg.NewUserLabels) {
                Symbol oldUserValue = null;
                if (mProject.UserLabels.ContainsKey(kvp.Key)) {
                    Debug.Assert(false, "should not be replacing label");
                    oldUserValue = mProject.UserLabels[kvp.Key];
                }
                UndoableChange uc = UndoableChange.CreateLabelChange(kvp.Key,
                    oldUserValue, kvp.Value);
                cs.Add(uc);
            }

            // Apply analyzer tags.
            if (dlg.WantCodeStartPoints) {
                TypedRangeSet newSet = new TypedRangeSet();
                TypedRangeSet undoSet = new TypedRangeSet();

                foreach (int offset in dlg.AllTargetOffsets) {
                    // We don't need to add a "code start" tag if this is already the
                    // start of an instruction.  We do need to add one if it's the *middle*
                    // of an instruction, e.g. the table points inside a "BIT abs".  So we
                    // test against IsInstructionStart, not IsInstruction.
                    if (!mProject.GetAnattrib(offset).IsInstructionStart) {
                        CodeAnalysis.AnalyzerTag oldType = mProject.AnalyzerTags[offset];
                        if (oldType == CodeAnalysis.AnalyzerTag.Code) {
                            continue;       // already set
                        }
                        undoSet.Add(offset, (int)oldType);
                        newSet.Add(offset, (int)CodeAnalysis.AnalyzerTag.Code);
                    }
                }
                if (newSet.Count != 0) {
                    cs.Add(UndoableChange.CreateAnalyzerTagChange(undoSet, newSet));
                }
            }

            // Finally, apply the change.
            if (cs.Count != 0) {
                ApplyUndoableChanges(cs);
            } else {
                Debug.WriteLine("No changes found");
            }
        }

        public bool CanRemoveFormatting() {
            // Want at least one line of code or data.  No need to check for existing formatting.
            EntityCounts counts = SelectionAnalysis.mEntityCounts;
            return (counts.mDataLines > 0 || counts.mCodeLines > 0);
        }

        public void RemoveFormatting() {
            RangeSet sel = OffsetSetFromSelected();
            ChangeSet cs = new ChangeSet(16);
            foreach (int offset in sel) {
                if (offset < 0) {
                    // header comment
                    continue;
                }

                // Formatted?
                if (mProject.OperandFormats.TryGetValue(offset, out FormatDescriptor oldFd)) {
                    Debug.WriteLine("Remove format from +" + offset.ToString("x6"));
                    UndoableChange uc = UndoableChange.CreateOperandFormatChange(offset,
                        oldFd, null);
                    cs.Add(uc);
                }

                // As an added bonus, check for mid-line labels.  The tricky part with this is
                // that the determination of visibility is made before the effects of removing
                // the formatting are known.  In general we try very hard to avoid embedding
                // labels, so this is unlikely to be a problem.
                Anattrib attr = mProject.GetAnattrib(offset);
                if (attr.Symbol != null && !attr.IsStart) {
                    Debug.WriteLine("Remove label from +" + offset.ToString("x6"));
                    UndoableChange uc = UndoableChange.CreateLabelChange(offset, attr.Symbol, null);
                    cs.Add(uc);
                }
            }

            if (cs.Count != 0) {
                ApplyUndoableChanges(cs);
            } else {
                Debug.WriteLine("No formatting or embedded labels found");
            }
        }

        public void ReloadExternalFiles() {
            string messages = mProject.LoadExternalFiles();
            if (messages.Length != 0) {
                // ProjectLoadIssues isn't quite the right dialog, but it'll do.  This is
                // purely informative; no decision needs to be made.
                ProjectLoadIssues dlg = new ProjectLoadIssues(mMainWin, messages,
                    ProjectLoadIssues.Buttons.Continue);
                dlg.ShowDialog();
            }

            // We only really need to do this if the set of extension scripts has changed.
            // For an explicit reload we might as well just do it all the time.
            mProject.ClearVisualizationCache();

            // Update the project.
            UndoableChange uc =
                UndoableChange.CreateDummyChange(UndoableChange.ReanalysisScope.CodeAndData);
            ApplyChanges(new ChangeSet(uc), false);

            // TODO(someday): this would really be better as a special-case dummy change
            // that caused the "external files have changed" behavior in ApplyChanges() to fire.
            // Before we can do that, though, we need a way to propagate the load errors and
            // compiler warnings out for display.
        }

        public void Goto() {
            int index = mMainWin.CodeListView_GetFirstSelectedIndex();
            if (index < 0) {
                index = mMainWin.CodeListView_GetTopIndex();    // nothing selected
            }
            int offset = CodeLineList[index].FileOffset;

            GotoBox dlg = new GotoBox(mMainWin, mProject, offset, mFormatter);
            if (dlg.ShowDialog() == true) {
                GoToLocation(new NavStack.Location(dlg.TargetOffset, 0,
                    NavStack.GoToMode.JumpToCodeData), true);
                //mMainWin.CodeListView_Focus();
            }
        }

        /// <summary>
        /// Moves the view and selection to the specified offset.  We want to select stuff
        /// differently if we're jumping to a note vs. jumping to an instruction.
        /// </summary>
        /// <param name="newLoc">Location to jump to.</param>
        /// <param name="mode">Interesting set of lines within that offset.</param>
        /// <param name="doPush">If set, push new offset onto navigation stack.</param>
        public void GoToLocation(NavStack.Location newLoc, bool doPush) {
            NavStack.Location prevLoc = GetCurrentlySelectedLocation();
            //Debug.WriteLine("GoToLocation: " + loc + " mode=" + mode + " doPush=" + doPush +
            //    " (curLoc=" + prevLoc + ")");

            // Avoid pushing multiple copies of the same address on.  This doesn't quite work
            // because we can't compare the LineDelta without figuring out JumpToCodeData first.
            // If we're sitting in a long comment or LvTable and the user double-clicks on the
            // entry in the symbol table for the current offset, we want to move the selection,
            // so we don't want to bail out if the offset matches.  Easiest thing to do is to
            // do the move but not push it.
            if (newLoc.Offset == prevLoc.Offset && newLoc.Mode == prevLoc.Mode) {
                // we're jumping to ourselves?
                if (doPush) {
                    Debug.WriteLine("Ignoring push for goto to current offset");
                    doPush = false;
                }
            }

            int topLineIndex = CodeLineList.FindLineIndexByOffset(newLoc.Offset);
            if (topLineIndex < 0) {
                Debug.Assert(false, "failed goto offset +" + newLoc.Offset.ToString("x6"));
                return;
            }
            int lastLineIndex;
            if (newLoc.Mode == NavStack.GoToMode.JumpToNote) {
                // Select all note lines, disregard the rest.
                while (CodeLineList[topLineIndex].LineType != LineListGen.Line.Type.Note) {
                    if (CodeLineList[topLineIndex].FileOffset != newLoc.Offset) {
                        // This can happen if the note got deleted.
                        break;
                    }
                    topLineIndex++;
                }
                lastLineIndex = topLineIndex + 1;
                while (lastLineIndex < CodeLineList.Count &&
                        CodeLineList[lastLineIndex].LineType == LineListGen.Line.Type.Note) {
                    lastLineIndex++;
                }
            } else if (newLoc.Offset < 0) {
                // This is the offset of the header comment or a .EQ directive. Don't mess with it.
                lastLineIndex = topLineIndex + 1;
            } else if (newLoc.Mode == NavStack.GoToMode.JumpToCodeData) {
                // Advance to the code or data line.
                while (CodeLineList[topLineIndex].LineType != LineListGen.Line.Type.Code &&
                        CodeLineList[topLineIndex].LineType != LineListGen.Line.Type.Data) {
                    topLineIndex++;
                }

                lastLineIndex = topLineIndex + 1;
            } else if (newLoc.Mode == NavStack.GoToMode.JumpToAdjIndex) {
                // Adjust the line position by the line delta.  If the adjustment moves us to
                // a different element, ignore the adjustment.
                if (CodeLineList[topLineIndex].FileOffset ==
                        CodeLineList[topLineIndex + newLoc.LineDelta].FileOffset) {
                    topLineIndex += newLoc.LineDelta;
                }
                lastLineIndex = topLineIndex + 1;
            } else if (newLoc.Mode == NavStack.GoToMode.JumpToArStart ||
                    newLoc.Mode == NavStack.GoToMode.JumpToArEnd) {
                LineListGen.Line.Type matchType = LineListGen.Line.Type.ArStartDirective;
                if (newLoc.Mode != NavStack.GoToMode.JumpToArStart) {
                    matchType = LineListGen.Line.Type.ArEndDirective;
                }
                // Find first instance of specified type.
                while (CodeLineList[topLineIndex].LineType != matchType) {
                    if (CodeLineList[topLineIndex].FileOffset > newLoc.Offset) {
                        // This can happen if the region got deleted.
                        break;
                    }
                    topLineIndex++;
                }
                lastLineIndex = topLineIndex + 1;
                // If there's multiple lines, make sure they're all on screen.
                while (lastLineIndex < CodeLineList.Count &&
                        CodeLineList[lastLineIndex].LineType == matchType) {
                    lastLineIndex++;
                }
            } else {
                Debug.Assert(false);
                lastLineIndex = topLineIndex + 1;
            }

            // Make sure the item is visible.  For notes, this can span multiple lines.
            mMainWin.CodeListView_EnsureVisible(lastLineIndex - 1);
            mMainWin.CodeListView_EnsureVisible(topLineIndex);

            // Update the selection.
            mMainWin.CodeListView_SelectRange(topLineIndex, lastLineIndex - topLineIndex);

            // Set the focus to the first selected item.  The focus is used by the keyboard
            // handler to decide what the up/down arrows select next.
            mMainWin.CodeListView_SetSelectionFocus();

            if (doPush) {
                // Update the back stack and associated controls.
                mNavStack.Push(prevLoc);
            }
        }

        /// <summary>
        /// Moves the view and selection to the definition of a local variable.
        /// </summary>
        /// <param name="offset">Offset at which the variable was referenced.</param>
        /// <param name="symRef">Reference to variable.</param>
        public void GoToVarDefinition(int offset, WeakSymbolRef symRef, bool doPush) {
            Debug.Assert(offset >= 0);
            Debug.Assert(symRef.IsVariable);

            LocalVariableLookup lvLookup = new LocalVariableLookup(mProject.LvTables, mProject,
                null, false, false);
            int varOffset = lvLookup.GetDefiningTableOffset(offset, symRef);
            if (varOffset < 0) {
                Debug.WriteLine("Local variable not found; offset=" + offset + " ref=" + symRef);
                return;
            }

            // Find the actual symbol definition.
            LocalVariableTable lvTable = mProject.LvTables[varOffset];
            DefSymbol foundSym = lvTable.GetByLabel(symRef.Label);
            if (foundSym == null) {
                // shouldn't be possible
                Debug.WriteLine("Did not find " + symRef.Label + " in expected table");
                Debug.Assert(false);
                return;
            }

            // We have the offset to which the local variable table is bound.  We need to
            // walk down until we find the variable definitions, and find the line with the
            // matching symbol.
            //
            // We're comparing to the formatted strings -- safer than trying to find the symbol
            // in the table and then guess at how the table arranges itself for display -- so we
            // need to compare the formatted form of the label.
            //
            // We need to use GenerateDisplayLabel() because the symbol might have an annotation.
            string cmpStr = mFormatter.FormatVariableLabel(
                foundSym.GenerateDisplayLabel(mFormatter));
            int lineIndex = CodeLineList.FindLineIndexByOffset(varOffset);
            while (lineIndex < mProject.FileDataLength) {
                LineListGen.Line line = CodeLineList[lineIndex];
                if (line.FileOffset != varOffset) {
                    // we've gone too far
                    Debug.WriteLine("ran out of LV table");
                    return;
                }

                if (line.LineType == LineListGen.Line.Type.LocalVariableTable) {
                    DisplayList.FormattedParts parts = CodeLineList.GetFormattedParts(lineIndex);
                    if (cmpStr.Equals(parts.Label)) {
                        // Eureka
                        NavStack.Location prevLoc = GetCurrentlySelectedLocation();

                        mMainWin.CodeListView_EnsureVisible(lineIndex);

                        // Update the selection.
                        mMainWin.CodeListView_SelectRange(lineIndex, 1);
                        mMainWin.CodeListView_SetSelectionFocus();

                        if (doPush) {
                            // Update the back stack and associated controls.
                            mNavStack.Push(prevLoc);
                        }

                        return;
                    } else {
                        //Debug.WriteLine("Var: '" + cmpStr + "' != '" + parts.Label + "'");
                    }
                }

                lineIndex++;
            }
        }

        public bool CanJumpToOperand() {
            if (SelectionAnalysis.mNumItemsSelected != 1) {
                return false;
            }
            LineListGen.Line.Type lineType = SelectionAnalysis.mLineType;
            if (lineType != LineListGen.Line.Type.Code &&
                    lineType != LineListGen.Line.Type.Data &&
                    lineType != LineListGen.Line.Type.ArStartDirective &&
                    lineType != LineListGen.Line.Type.ArEndDirective) {
                return false;
            }

            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            Debug.Assert(selIndex >= 0);
            return JumpToOperandTarget(CodeLineList[selIndex], true);
        }

        public void JumpToOperand() {
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            Debug.Assert(selIndex >= 0);
            JumpToOperandTarget(CodeLineList[selIndex], false);
        }

        /// <summary>
        /// Calculates the currently-selected location.
        /// </summary>
        /// <remarks>
        /// This is done whenever we jump somewhere else.  For the most part we'll be in a
        /// line of code, jumping when an operand or reference is double-clicked, but we might
        /// be in the middle of a long comment when a symbol is double-clicked or the
        /// nav-forward arrow is clicked.  The most interesting case is when a reference for
        /// a local variable table entry is double-clicked, since we want to be sure that we
        /// return to the correct entry in the LvTable (assuming it still exists).
        /// </remarks>
        /// <returns>Returns the location.</returns>
        private NavStack.Location GetCurrentlySelectedLocation() {
            int index = mMainWin.CodeListView_GetFirstSelectedIndex();
            if (index < 0) {
                // nothing selected, use top instead
                index = mMainWin.CodeListView_GetTopIndex();
            }
            int offset = CodeLineList[index].FileOffset;
            int lineDelta = index - CodeLineList.FindLineIndexByOffset(offset);
            bool isNote = (CodeLineList[index].LineType == LineListGen.Line.Type.Note);
            return new NavStack.Location(offset, lineDelta,
                isNote ? NavStack.GoToMode.JumpToNote : NavStack.GoToMode.JumpToAdjIndex);
        }

        public void GotoLastChange() {
            ChangeSet cs = mProject.GetTopChange();
            Debug.Assert(cs.Count > 0);

            // Get the offset from the first change in the set.  Ignore the rest.
            UndoableChange uc = cs[0];
            int offset;
            bool isNote = false;
            if (uc.HasOffset) {
                offset = uc.Offset;

                // If we altered a Note, and didn't remove it, jump to the note instead of
                // the nearby code/data.
                //
                // TODO(someday): we can do similar things for comment edits, e.g. if it's
                // SetLongComment we can find the line on which the comment starts and
                // pass that as a line delta.
                if (uc.Type == UndoableChange.ChangeType.SetNote &&
                        uc.NewValue != null) {
                    isNote = true;
                }
            } else if (uc.Type == UndoableChange.ChangeType.SetProjectProperties) {
                // some chance it modified the EQU statements... jump there
                offset = 0;
            } else if (uc.Type == UndoableChange.ChangeType.SetAnalyzerTag) {
                TypedRangeSet newSet = (TypedRangeSet)uc.NewValue;
                if (newSet.Count == 0) {
                    // unexpected
                    Debug.Assert(false);
                    return;
                }

                // Get the offset of the first entry.
                IEnumerator<TypedRangeSet.Tuple> iter =
                    (IEnumerator<TypedRangeSet.Tuple>)newSet.GetEnumerator();
                iter.MoveNext();
                TypedRangeSet.Tuple firstOffset = iter.Current;
                offset = firstOffset.Value;
            } else {
                Debug.Assert(false);
                return;
            }

            if (isNote) {
                GoToLocation(new NavStack.Location(offset, 0, NavStack.GoToMode.JumpToNote),
                    true);
            } else {
                GoToLocation(new NavStack.Location(offset, 0, NavStack.GoToMode.JumpToCodeData),
                    true);
            }
        }

        public bool CanNavigateBackward() {
            return mNavStack.HasBackward;
        }
        public void NavigateBackward() {
            Debug.Assert(mNavStack.HasBackward);
            NavStack.Location backLoc = mNavStack.MoveBackward(GetCurrentlySelectedLocation());
            GoToLocation(backLoc, false);
        }

        public bool CanNavigateForward() {
            return mNavStack.HasForward;
        }
        public void NavigateForward() {
            Debug.Assert(mNavStack.HasForward);
            NavStack.Location fwdLoc = mNavStack.MoveForward(GetCurrentlySelectedLocation());
            GoToLocation(fwdLoc, false);
        }

        /// <summary>
        /// Scrolls the code list so that the specified label is shown.  If the label can't
        /// be found, nothing happens.
        /// </summary>
        /// <param name="sym">Label symbol.</param>
        public bool GoToSymbol(Symbol sym) {
            bool found = false;

            if (sym.SymbolSource == Symbol.Source.Platform ||
                    sym.SymbolSource == Symbol.Source.Project) {
                // Look for an EQU line for the project or platform symbol.
                for (int i = 0; i < mProject.ActiveDefSymbolList.Count; i++) {
                    if (mProject.ActiveDefSymbolList[i] == sym) {
                        int offset = LineListGen.DefSymOffsetFromIndex(i);
                        Debug.Assert(offset < 0);
                        GoToLocation(new NavStack.Location(offset, 0,
                            NavStack.GoToMode.JumpToCodeData), true);
                        found = true;
                        break;
                    }
                }
            } else {
                // Just look for a matching label.
                int offset = mProject.FindLabelOffsetByName(sym.Label);
                if (offset >= 0) {
                    // TODO(someday):jump to symbol line, not arstart, for address region pre-labels
                    GoToLocation(new NavStack.Location(offset, 0, NavStack.GoToMode.JumpToCodeData),
                        true);
                    found = true;
                }
            }

            return found;
        }

        public void SelectionChanged() {
            SelectionAnalysis = UpdateSelectionState();

            UpdateReferencesPanel();
            UpdateInfoPanel();
            UpdateSelectionHighlight();
        }

        /// <summary>
        /// Gathered facts about the current selection.  Recalculated whenever the selection
        /// changes.
        /// </summary>
        public class SelectionState {
            // Number of selected items or lines, reduced.  This will be:
            //  0 if no lines are selected
            //  1 if a single *item* is selected (regardless of number of lines)
            //  >1 if more than one item is selected (exact value not specified)
            public int mNumItemsSelected;

            // Single selection: the type of line selected.  (Multi-sel: Unclassified)
            public LineListGen.Line.Type mLineType;

            // Single selection: is line an instruction with an operand.  (Multi-sel: False)
            public bool mIsInstructionWithOperand;

            // Single selection: is line an EQU directive for a project symbol.  (Multi-sel: False)
            public bool mIsProjectSymbolEqu;

            // Some totals.
            public EntityCounts mEntityCounts;

            public SelectionState() {
                mLineType = LineListGen.Line.Type.Unclassified;
                mEntityCounts = new EntityCounts();
            }

            public override string ToString() {
                return "SelState: numSel=" + mNumItemsSelected + " type=" + mLineType;
            }
        }

        /// <summary>
        /// Updates Actions menu enable states when the selection changes.
        /// </summary>
        ///   is selected.</param>
        public SelectionState UpdateSelectionState() {
            int selCount = mMainWin.CodeListView_GetSelectionCount();
            //Debug.WriteLine("UpdateSelectionState: selCount=" + selCount);

            SelectionState state = new SelectionState();

            // Use IsSingleItemSelected(), rather than just checking sel.Count, because we
            // want the user to be able to e.g. EditData on a multi-line string even if all
            // lines in the string are selected.
            if (selCount < 0) {
                // nothing selected, leave everything set to false / 0
                state.mEntityCounts = new EntityCounts();
            } else if (IsSingleItemSelected()) {
                int firstIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
                state.mNumItemsSelected = 1;
                state.mEntityCounts = GatherEntityCounts(firstIndex);
                LineListGen.Line line = CodeLineList[firstIndex];
                state.mLineType = line.LineType;

                state.mIsInstructionWithOperand = (line.LineType == LineListGen.Line.Type.Code &&
                    mProject.GetAnattrib(line.FileOffset).IsInstructionWithOperand);
                if (line.LineType == LineListGen.Line.Type.EquDirective) {
                    // See if this EQU directive is for a project symbol.
                    int symIndex = LineListGen.DefSymIndexFromOffset(line.FileOffset);
                    DefSymbol defSym = mProject.ActiveDefSymbolList[symIndex];
                    state.mIsProjectSymbolEqu = (defSym.SymbolSource == Symbol.Source.Project);
                }
            } else {
                state.mNumItemsSelected = 2;
                state.mEntityCounts = GatherEntityCounts(-1);
            }

            return state;
        }

        /// <summary>
        /// Entity count collection, for GatherEntityCounts.
        /// </summary>
        public class EntityCounts {
            public int mCodeLines;
            public int mDataLines;
            public int mBlankLines;
            public int mControlLines;

            public int mCodeStartTags;
            public int mCodeStopTags;
            public int mInlineDataTags;
            public int mNoTags;
        };

        /// <summary>
        /// Gathers a count of different line types and atags that are currently selected.
        /// </summary>
        /// <param name="singleLineIndex">If a single line is selected, pass the index in.
        ///   Otherwise, pass -1 to traverse the entire line list.</param>
        /// <returns>Object with computed totals.</returns>
        private EntityCounts GatherEntityCounts(int singleLineIndex) {
            //DateTime startWhen = DateTime.Now;
            int codeLines, dataLines, blankLines, controlLines;
            int codeStartTags, codeStopTags, inlineDataTags, noTags;
            codeLines = dataLines = blankLines = controlLines = 0;
            codeStartTags = codeStopTags = inlineDataTags = noTags = 0;

            int startIndex, endIndex;
            if (singleLineIndex < 0) {
                startIndex = 0;
                endIndex = mMainWin.CodeDisplayList.Count - 1;
            } else {
                startIndex = endIndex = singleLineIndex;
            }

            for (int i = startIndex; i <= endIndex; i++) {
                if (!mMainWin.CodeDisplayList.SelectedIndices[i]) {
                    // not selected, ignore
                    continue;
                }
                LineListGen.Line line = CodeLineList[i];
                switch (line.LineType) {
                    case LineListGen.Line.Type.Code:
                        codeLines++;
                        break;
                    case LineListGen.Line.Type.Data:
                        dataLines++;
                        break;
                    case LineListGen.Line.Type.Blank:
                        // Don't generally care how many blank lines there are, but we do want
                        // to exclude them from the other categories: if we have nothing but
                        // blank lines, there's nothing to do.
                        blankLines++;
                        break;
                    default:
                        // These are only editable as single-line items.  We do allow mass
                        // atag selection to include them (they will be ignored).
                        // org, equ, rwid, long comment...
                        controlLines++;
                        break;
                }

                // A single line can span multiple offsets, each of which could have a
                // different analyzer tag.  Note the code start/stop tags are only applied to the
                // first byte of each selected line, so we're not quite in sync with that.
                //
                // For multi-line items, the OffsetSpan of the first item covers the entire
                // item (it's the same for all Line instances), so we only want to do this for
                // the first entry.
                if (line.SubLineIndex == 0) {
                    for (int offset = line.FileOffset; offset < line.FileOffset + line.OffsetSpan;
                            offset++) {
                        switch (mProject.AnalyzerTags[offset]) {
                            case CodeAnalysis.AnalyzerTag.Code:
                                codeStartTags++;
                                break;
                            case CodeAnalysis.AnalyzerTag.Data:
                                codeStopTags++;
                                break;
                            case CodeAnalysis.AnalyzerTag.InlineData:
                                inlineDataTags++;
                                break;
                            case CodeAnalysis.AnalyzerTag.None:
                                noTags++;
                                break;
                            default:
                                Debug.Assert(false);
                                break;
                        }
                    }
                }
            }

            //Debug.WriteLine("GatherEntityCounts (start=" + startIndex + " end=" + endIndex +
            //    " len=" + mMainWin.CodeDisplayList.Count +
            //    ") took " + (DateTime.Now - startWhen).TotalMilliseconds + " ms");

            return new EntityCounts() {
                mCodeLines = codeLines,
                mDataLines = dataLines,
                mBlankLines = blankLines,
                mControlLines = controlLines,
                mCodeStartTags = codeStartTags,
                mCodeStopTags = codeStopTags,
                mInlineDataTags = inlineDataTags,
                mNoTags = noTags
            };
        }

        /// <summary>
        /// Determines whether the current selection spans a single item.  This could be a
        /// single-line item or a multi-line item.
        /// </summary>
        private bool IsSingleItemSelected() {
            int firstIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            if (firstIndex < 0) {
                // empty selection
                return false;
            }

            int lastIndex = mMainWin.CodeListView_GetLastSelectedIndex();
            if (lastIndex == firstIndex) {
                // only one line is selected
                return true;
            }

            // Just check the first and last entries to see if they're the same.
            LineListGen.Line firstItem = CodeLineList[firstIndex];
            LineListGen.Line lastItem = CodeLineList[lastIndex];
            if (firstItem.FileOffset == lastItem.FileOffset &&
                    firstItem.LineType == lastItem.LineType) {
                return true;
            }
            return false;
        }

        private bool mUpdatingSelectionHighlight;       // recursion guard for next method

        /// <summary>
        /// Updates the selection highlights.  When a code or data item with an operand offset is
        /// selected, such as a branch, we want to highlight the address and label of the
        /// target.  When a code or data item is referenced by another instruction, such as a
        /// branch, we want to highlight the operands of all such instructions.
        /// </summary>
        private void UpdateSelectionHighlight() {
            if (mUpdatingSelectionHighlight) {
                return;
            }
            mUpdatingSelectionHighlight = true;

            //
            // Start with the target address highlight, for branches and in-file memory accesses.
            //

            int targetIndex = -1;
            LineListGen.Line selectedLine = null;
            if (mMainWin.CodeListView_GetSelectionCount() == 1) {
                int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
                selectedLine = CodeLineList[selIndex];
                if (selectedLine.IsCodeOrData) {
                    Debug.Assert(selectedLine.FileOffset >= 0);
                    targetIndex = FindSelectionAddrHighlight(selectedLine);
                }
            }

            if (mTargetHighlightIndex != targetIndex) {
                Debug.WriteLine("Target highlight moving from " + mTargetHighlightIndex +
                    " to " + targetIndex);

                // The highlight is currently implemented by modifying the item in the
                // display list.  Because those items are immutable, we have to remove the
                // old and add a new.  The WPF ListView maintains its selection by object
                // reference, so replacing an item requires removing the old item from the
                // selection set and adding it to the new.
                //
                // So if a line references itself (like the ZipGS cache conditioner loop does),
                // it will be the selected line while we're doing this little dance.  When the
                // calls below update the selection, this method will be called again. This
                // turns into infinite recursion.
                mMainWin.CodeListView_RemoveSelectionAddrHighlight(mTargetHighlightIndex);
                mMainWin.CodeListView_AddSelectionAddrHighlight(targetIndex);

                mTargetHighlightIndex = targetIndex;
            }

            //
            // Now do the source operand highlight, to see what refers to the current address.
            //

            // Un-highlight anything we had highlighted previously.
            if (mOperandHighlights.Count > 0) {
                foreach (int index in mOperandHighlights) {
                    mMainWin.CodeListView_RemoveSelectionOperHighlight(index);
                }
                mOperandHighlights.Clear();
            }
            if (selectedLine != null) {
                XrefSet xrefs = null;
                if (selectedLine.IsCodeOrData) {
                    xrefs = mProject.GetXrefSet(selectedLine.FileOffset);
                } else if (selectedLine.LineType == LineListGen.Line.Type.LocalVariableTable) {
                    DefSymbol defSym = CodeLineList.GetLocalVariableFromLine(selectedLine);
                    xrefs = (defSym == null) ? null : defSym.Xrefs;
                }
                if (xrefs != null) {
                    foreach (XrefSet.Xref xr in xrefs) {
                        int refIndex = CodeLineList.FindCodeDataIndexByOffset(xr.Offset);
                        mMainWin.CodeListView_AddSelectionOperHighlight(refIndex);
                        mOperandHighlights.Add(refIndex);
                    }
                }
            }

            mUpdatingSelectionHighlight = false;
        }

        private int FindSelectionAddrHighlight(LineListGen.Line line) {
            // Does this have an operand with an in-file target offset?
            // TODO: may not work correctly with reloc data?
            Anattrib attr = mProject.GetAnattrib(line.FileOffset);
            if (attr.OperandOffset >= 0) {
                return CodeLineList.FindCodeDataIndexByOffset(attr.OperandOffset);
            } else if (attr.IsDataStart || attr.IsInlineDataStart) {
                // If it's an Address or Symbol, we can try to resolve
                // the value.
                int operandOffset = DataAnalysis.GetDataOperandOffset(mProject, line.FileOffset,
                    out int unused);
                if (operandOffset >= 0) {
                    return CodeLineList.FindCodeDataIndexByOffset(operandOffset);
                }
            }
            return -1;
        }

        public void ShowHexDump() {
            if (mHexDumpDialog == null) {
                // Create and show modeless dialog.  This one is "always on top" by default,
                // to allow the user to click around to various points.  Note that "on top"
                // means on top of *everything*.  We create this without an owner so that,
                // when it's not on top, it can sit behind the main app window until you
                // double-click something else.
                mHexDumpDialog = new Tools.WpfGui.HexDumpViewer(null,
                    mProject.FileData, mFormatter);
                mHexDumpDialog.Closing += (sender, e) => {
                    Debug.WriteLine("Hex dump dialog closed");
                    //showHexDumpToolStripMenuItem.Checked = false;
                    mHexDumpDialog = null;
                };
                mHexDumpDialog.Topmost = true;
                mHexDumpDialog.Show();
            }

            // Bring it to the front of the window stack.  This also transfers focus to the
            // window.
            mHexDumpDialog.Activate();

            // Set the dialog's position.
            if (mMainWin.CodeListView_GetSelectionCount() > 0) {
                int firstIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
                int lastIndex = mMainWin.CodeListView_GetLastSelectedIndex();
                // offsets can be < 0 if they've selected EQU statements
                int firstOffset = Math.Max(0, CodeLineList[firstIndex].FileOffset);
                int lastOffset = Math.Max(firstOffset, CodeLineList[lastIndex].FileOffset +
                    CodeLineList[lastIndex].OffsetSpan - 1);
                mHexDumpDialog.ShowOffsetRange(firstOffset, lastOffset);
            }
        }

        /// <summary>
        /// Handles the four analyzer tag commands.
        /// </summary>
        /// <param name="atag">Type of tag to apply.</param>
        /// <param name="firstByteOnly">If set, only the first byte on each line is tagged.</param>
        public void MarkAsType(CodeAnalysis.AnalyzerTag atag, bool firstByteOnly) {
            RangeSet sel;

            if (atag == CodeAnalysis.AnalyzerTag.Code && SelectionAnalysis.mNumItemsSelected == 1) {
                // We're applying a code tag to a single line.  Analyze the file to see if special
                // handling for jump tables can be applied here.
                if (TryMarkJumpTable(out bool cancel) || cancel) {
                    return;
                }
            }

            if (firstByteOnly) {
                sel = new RangeSet();
                foreach (int index in mMainWin.CodeDisplayList.SelectedIndices) {
                    int offset = CodeLineList[index].FileOffset;
                    if (offset >= 0) {
                        // Ignore the header lines.
                        sel.Add(offset);
                    }
                }

                // "first byte only" is used for code start/stop tags, which should only be
                // placed at the start of a region.
                if (sel.Count > 1) {
                    MessageBoxResult result = MessageBox.Show(Res.Strings.ANALYZER_TAG_MULTI_CHK,
                        Res.Strings.CONFIRMATION_NEEDED,
                        MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel) {
                        return;
                    }
                }
            } else {
                // Inline data or none.  Select all bytes.
                sel = OffsetSetFromSelected();
            }

            DoMarkAsType(atag, sel);
        }

        /// <summary>
        /// Attempts special handling for "jump tables", i.e. chunks of code with multiple
        /// consecutive JMP abs instructions.
        /// </summary>
        /// <remarks>
        /// The current scan will skip over already-formatted JMP instructions to look for more
        /// beyond.  This is extended behavior to make it easier to tag something when the first
        /// JMP is already tagged.  However, if it only finds one unformatted entry it won't fire.
        /// The user really ought to be tagging the first unformatted $4c byte; it might be better
        /// to require this.
        /// </remarks>
        /// <param name="cancel">Result: true if user asked to cancel the operation.</param>
        /// <returns>True if a jump table was found and processed.</returns>
        private bool TryMarkJumpTable(out bool cancel) {
            cancel = false;
            int selIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            int offset = CodeLineList[selIndex].FileOffset;
            byte JMP = OpDef.OpJMP_Abs.Opcode;      // 0x4c

            RangeSet sel = new RangeSet();
            while (offset + 2 < mProject.FileDataLength) {
                if (mProject.FileData[offset] != JMP) {
                    break;
                }
                Anattrib attr0 = mProject.GetAnattrib(offset);
                bool isInst = attr0.IsInstructionStart;
                bool halt = false;

                // Confirm that all bytes are data/inline-data, or are part of a previously-known
                // JMP instruction.
                for (int i = 0; i < 2; i++) {
                    Anattrib attr = mProject.GetAnattrib(offset + i);
                    if (attr.IsInstruction && !isInst) {
                        halt = true;    // found instruction, but offset+0 wasn't an inst start
                        break;
                    }

                    // Don't continue if a user label is defined here, unless it's on the JMP
                    // opcode byte.
                    if (i != 0 && mProject.UserLabels.TryGetValue(offset + i, out Symbol unused)) {
                        halt = true;
                        break;
                    }
                    // Don't continue if any byte has been formatted.
                    if (mProject.OperandFormats.TryGetValue(offset + i, out FormatDescriptor unu)) {
                        halt = true;
                        break;
                    }
                }
                if (halt) {
                    break;
                }

                // If not already marked as an instruction, add the JMP opcode byte to the set.
                if (!attr0.IsInstructionStart) {
                    Debug.WriteLine("JumpTab: adding offset +" + offset.ToString("x6"));
                    sel.Add(offset);
                }

                offset += 3;
            }
            if (sel.Count <= 1) {
                // Didn't find anything to do, or found only one entry.  Let the general code
                // handle it.
                return false;
            }

            // Ask the user to confirm.
            string msg = string.Format(Res.Strings.ANALYZER_TAG_JMP_TABLE_FMT, sel.Count);
            MessageBoxResult result =
                MessageBox.Show(msg, Res.Strings.ANALYZER_TAG_JMP_TABLE_CAPTION,
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            switch (result) {
                case MessageBoxResult.Cancel:
                default:
                    cancel = true;
                    return false;
                case MessageBoxResult.No:
                    return false;
                case MessageBoxResult.Yes:
                    DoMarkAsType(CodeAnalysis.AnalyzerTag.Code, sel);
                    return true;
            }
        }

        /// <summary>
        /// Generates the change set for analyzer tag updates.
        /// </summary>
        private void DoMarkAsType(CodeAnalysis.AnalyzerTag atag, RangeSet sel) {
            TypedRangeSet newSet = new TypedRangeSet();
            TypedRangeSet undoSet = new TypedRangeSet();

            foreach (int offset in sel) {
                if (offset < 0) {
                    // header comment
                    continue;
                }
                CodeAnalysis.AnalyzerTag oldType = mProject.AnalyzerTags[offset];
                if (oldType == atag) {
                    // no change, don't add to set
                    continue;
                }
                undoSet.Add(offset, (int)oldType);
                newSet.Add(offset, (int)atag);
            }
            if (newSet.Count == 0) {
                Debug.WriteLine("No changes found (" + atag + ", " + sel.Count + " offsets)");
                return;
            }

            UndoableChange uc = UndoableChange.CreateAnalyzerTagChange(undoSet, newSet);
            ChangeSet cs = new ChangeSet(uc);

            ApplyUndoableChanges(cs);
        }

        /// <summary>
        /// Converts the set of selected items into a set of offsets.  If a line
        /// spans multiple offsets (e.g. a 3-byte instruction), offsets for every
        /// byte are included.
        /// 
        /// Boundaries such as labels and address changes are ignored.
        /// </summary>
        /// <returns>RangeSet with all offsets.</returns>
        private RangeSet OffsetSetFromSelected() {
            RangeSet rs = new RangeSet();

            foreach (int index in mMainWin.CodeDisplayList.SelectedIndices) {
                if (CodeLineList[index].LineType == LineListGen.Line.Type.ArEndDirective) {
                    // We don't care about these, and they have the offset of the *last* byte
                    // of multi-byte instructions, which is a little confusing.
                    continue;
                }
                int offset = CodeLineList[index].FileOffset;
                if (offset < 0) {
                    // Ignore file header comment and EQU lines.
                    continue;
                }

                // Mark every byte of an instruction or multi-byte data item --
                // everything that is represented by the line the user selected.
                int len;
                if (offset >= 0) {
                    len = mProject.GetAnattrib(offset).Length;
                } else {
                    // header area
                    len = 1;
                }
                Debug.Assert(len > 0);
                for (int i = offset; i < offset + len; i++) {
                    rs.Add(i);
                }
            }
            return rs;
        }

        /// <summary>
        /// Handles Help - Help
        /// </summary>
        public void ShowHelp() {
            HelpAccess.ShowHelp(HelpAccess.Topic.Contents);
        }

        /// <summary>
        /// Handles Help - About
        /// </summary>
        public void ShowAboutBox() {
            AboutBox dlg = new AboutBox(mMainWin);
            dlg.ShowDialog();
        }

        public void ToggleDataScan() {
            ProjectProperties oldProps = mProject.ProjectProps;
            ProjectProperties newProps = new ProjectProperties(oldProps);
            newProps.AnalysisParams.AnalyzeUncategorizedData =
                !newProps.AnalysisParams.AnalyzeUncategorizedData;
            UndoableChange uc = UndoableChange.CreateProjectPropertiesChange(oldProps, newProps);
            ApplyUndoableChanges(new ChangeSet(uc));
        }

        public bool CanToggleSingleByteFormat() {
            EntityCounts counts = SelectionAnalysis.mEntityCounts;
            return (counts.mDataLines > 0 && counts.mCodeLines == 0);
        }

        public void ToggleSingleByteFormat() {
            TypedRangeSet trs = GroupedOffsetSetFromSelected();
            if (trs.Count == 0) {
                Debug.Assert(false, "nothing to edit");         // shouldn't happen
                return;
            }

            // Check the format descriptor of the first selected offset.
            int firstOffset = -1;
            foreach (TypedRangeSet.Tuple tup in trs) {
                firstOffset = tup.Value;
                break;
            }
            Debug.Assert(mProject.GetAnattrib(firstOffset).IsDataStart ||
                mProject.GetAnattrib(firstOffset).IsInlineDataStart);
            bool toDefault = false;
            if (mProject.OperandFormats.TryGetValue(firstOffset, out FormatDescriptor curDfd)) {
                if (curDfd.FormatType == FormatDescriptor.Type.NumericLE &&
                        curDfd.FormatSubType == FormatDescriptor.SubType.None &&
                        curDfd.Length == 1) {
                    // Currently single-byte, toggle to default.
                    toDefault = true;
                }
            }

            // Iterate through the selected regions.
            SortedList<int, FormatDescriptor> newFmts = new SortedList<int, FormatDescriptor>();
            IEnumerator<TypedRangeSet.TypedRange> rngIter = trs.RangeListIterator;
            while (rngIter.MoveNext()) {
                TypedRangeSet.TypedRange rng = rngIter.Current;
                if (toDefault) {
                    // Create a single REMOVE descriptor that covers the full span.
                    FormatDescriptor newDfd = FormatDescriptor.Create(rng.High - rng.Low + 1,
                        FormatDescriptor.Type.REMOVE, FormatDescriptor.SubType.None);
                    newFmts.Add(rng.Low, newDfd);
                } else {
                    // Add individual single-byte format descriptors for everything.
                    FormatDescriptor newDfd = FormatDescriptor.Create(1,
                        FormatDescriptor.Type.NumericLE, FormatDescriptor.SubType.None);
                    for (int i = rng.Low; i <= rng.High; i++) {
                        newFmts.Add(i, newDfd);
                    }
                }
            }

            ChangeSet cs = mProject.GenerateFormatMergeSet(newFmts);
            if (cs.Count != 0) {
                ApplyUndoableChanges(cs);
            }
        }

        /// <summary>
        /// Converts the ListView's selected items into a set of offsets.  If a line
        /// spans multiple offsets (e.g. a 3-byte instruction), offsets for every
        /// byte are included.
        /// </summary>
        /// <remarks>
        /// Contiguous regions with user labels or address changes are split into
        /// independent regions by using a serial number for the range type.  Same for
        /// long comments and notes.
        ///
        /// We don't split based on existing data format items.  That would make it impossible
        /// to convert from (say) a collection of single bytes to a collection of double bytes
        /// or a string.  It should not be possible to select part of a formatted section,
        /// unless the user has been playing weird games with analyzer tags to get overlapping
        /// format descriptors.
        ///
        /// The type values used in the TypedRangeSet may not be contiguous.  They're only
        /// there to create group separation from otherwise contiguous address ranges.
        /// </remarks>
        /// <returns>TypedRangeSet with all offsets.</returns>
        private TypedRangeSet GroupedOffsetSetFromSelected() {
            TypedRangeSet rs = new TypedRangeSet();
            int groupNum = 0;
            int expectedAddr = -1;

            DateTime startWhen = DateTime.Now;
            int prevOffset = -1;
            foreach (int index in mMainWin.CodeDisplayList.SelectedIndices) {
                // Don't add an offset to the set if the only part of it that is selected
                // is a directive or blank line.  We only care about file offsets, so skip
                // anything that isn't code or data.
                if (!CodeLineList[index].IsCodeOrData) {
                    continue;
                }

                int offset = CodeLineList[index].FileOffset;
                if (offset == prevOffset) {
                    // This is a continuation of a multi-line item like a string.  We've
                    // already accounted for all bytes associated with this offset.
                    continue;
                }
                Anattrib attr = mProject.GetAnattrib(offset);

                if (expectedAddr == -1) {
                    expectedAddr = attr.Address;
                }
                // Check for things that start a new group.
                if (attr.Address != expectedAddr) {
                    // For a contiguous selection, this should only happen if there's a .ORG
                    // address change.  For non-contiguous selection this is expected.  In the
                    // latter case, incrementing the group number is unnecessary but harmless
                    // (the TypedRangeSet splits at the gap).
                    //Debug.WriteLine("Address break: $" + attr.Address.ToString("x4") + " vs. $"
                    //    + expectedAddr.ToString("x4"));
                    expectedAddr = attr.Address;
                    groupNum++;
                } else if (offset > 0 && !mProject.AddrMap.IsRangeUnbroken(offset - 1, 2)) {
                    // Was the previous byte in a different address range?  This is only
                    // strictly necessary if the previous byte was in the selection set (which
                    // it won't be if the selection starts at the beginning of an address
                    // range), but bumping the group number is harmless if it wasn't.
                    groupNum++;
                } else if (mProject.UserLabels.ContainsKey(offset)) {
                    //if (mProject.GetAnattrib(offset).Symbol != null) {
                    // We consider auto labels when splitting regions for the data analysis,
                    // but I don't think we want to take them into account here.  The specific
                    // example that threw me was loading a 16-bit value from an address table.
                    // The code does "LDA table,X / STA / LDA table+1,X / STA", which puts auto
                    // labels at the first two addresses -- splitting the region.  That's good
                    // for the uncategorized data analyzer, but very annoying if you want to
                    // slap a 16-bit numeric format on all entries in a table.
                    groupNum++;
                } else if (mProject.HasCommentNoteOrVis(offset)) {
                    // Don't carry across a long comment, note, or visualization.
                    groupNum++;
                }

                // Mark every byte of an instruction or multi-byte data item --
                // everything that is represented by the line the user selected.  Control
                // statements and blank lines aren't relevant here, as we only care about
                // file offsets.
                int len = CodeLineList[index].OffsetSpan; // attr.Length;
                Debug.Assert(len > 0);
                for (int i = offset; i < offset + len; i++) {
                    rs.Add(i, groupNum);
                }
                // Advance the address.
                expectedAddr += len;

                prevOffset = offset;
            }
            Debug.WriteLine("Offset selection conv took " +
                (DateTime.Now - startWhen).TotalMilliseconds + " ms");
            return rs;
        }

        public bool CanUndo() {
            return (mProject != null && mProject.CanUndo);
        }

        /// <summary>
        /// Handles Edit - Undo.
        /// </summary>
        public void UndoChanges() {
            if (!mProject.CanUndo) {
                Debug.Assert(false, "Nothing to undo");
                return;
            }
            ChangeSet cs = mProject.PopUndoSet();
            ApplyChanges(cs, true);
            UpdateTitle();

            // If the debug dialog is visible, update it.
            if (mShowUndoRedoHistoryDialog != null) {
                mShowUndoRedoHistoryDialog.DisplayText = mProject.DebugGetUndoRedoHistory();
            }
        }

        public bool CanRedo() {
            return (mProject != null && mProject.CanRedo);
        }

        /// <summary>
        /// Handles Edit - Redo.
        /// </summary>
        public void RedoChanges() {
            if (!mProject.CanRedo) {
                Debug.Assert(false, "Nothing to redo");
                return;
            }
            ChangeSet cs = mProject.PopRedoSet();
            ApplyChanges(cs, false);
            UpdateTitle();

            // If the debug dialog is visible, update it.
            if (mShowUndoRedoHistoryDialog != null) {
                mShowUndoRedoHistoryDialog.DisplayText = mProject.DebugGetUndoRedoHistory();
            }
        }

        #endregion Main window UI event handlers

        #region References panel

        /// <summary>
        /// Updates the "references" panel to reflect the current selection.
        /// 
        /// The number of references to any given address should be relatively small, and
        /// won't change without a data refresh, so recreating the list every time shouldn't
        /// be a problem.
        /// </summary>
        private void UpdateReferencesPanel() {
            mMainWin.ReferencesListClear();

            if (mMainWin.CodeListView_GetSelectionCount() != 1) {
                // Nothing selected, or multiple lines selected.
                return;
            }
            int lineIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            LineListGen.Line line = CodeLineList[lineIndex];
            LineListGen.Line.Type type = line.LineType;
            if (type != LineListGen.Line.Type.Code &&
                    type != LineListGen.Line.Type.Data &&
                    type != LineListGen.Line.Type.EquDirective &&
                    type != LineListGen.Line.Type.LocalVariableTable) {
                // Code, data, and platform symbol EQUs have xrefs.
                return;
            }

            XrefSet xrefs;

            // Find the appropriate xref set.
            if (type == LineListGen.Line.Type.LocalVariableTable) {
                DefSymbol defSym = CodeLineList.GetLocalVariableFromLine(line);
                xrefs = (defSym == null) ? null : defSym.Xrefs;
            } else {
                int offset = line.FileOffset;
                if (offset < 0) {
                    // EQU in header
                    int index = LineListGen.DefSymIndexFromOffset(offset);
                    DefSymbol defSym = mProject.ActiveDefSymbolList[index];
                    xrefs = defSym.Xrefs;
                } else {
                    xrefs = mProject.GetXrefSet(offset);
                }
            }
            if (xrefs == null || xrefs.Count == 0) {
                return;
            }

            // TODO(someday): localization
            Asm65.Formatter formatter = mFormatter;
            bool showBank = !mProject.CpuDef.HasAddr16;
            for (int i = 0; i < xrefs.Count; i++) {
                XrefSet.Xref xr = xrefs[i];

                string idxStr = string.Empty;
                string typeStr;
                switch (xr.Type) {
                    case XrefSet.XrefType.SubCallOp:
                        typeStr = "call ";
                        break;
                    case XrefSet.XrefType.BranchOp:
                        typeStr = "branch ";
                        break;
                    case XrefSet.XrefType.RefFromData:
                        typeStr = "data ";
                        break;
                    case XrefSet.XrefType.MemAccessOp:
                        switch (xr.AccType) {
                            case OpDef.MemoryEffect.Read:
                                typeStr = "read ";
                                break;
                            case OpDef.MemoryEffect.Write:
                                typeStr = "write ";
                                break;
                            case OpDef.MemoryEffect.ReadModifyWrite:
                                typeStr = "rmw ";
                                break;
                            case OpDef.MemoryEffect.None:   // e.g. LDA #<symbol, PEA addr
                                typeStr = "ref ";
                                break;
                            case OpDef.MemoryEffect.Unknown:
                            default:
                                Debug.Assert(false);
                                typeStr = "??! ";
                                break;
                        }
                        break;
                    default:
                        Debug.Assert(false);
                        typeStr = "??? ";
                        break;
                }

                // "LDA (dp,X)" gets both of these
                if (xr.IsIndexedAccess) {
                    idxStr += "idx ";
                }
                if (xr.IsPointerAccess) {
                    idxStr += "ptr ";
                }

                MainWindow.ReferencesListItem rli = new MainWindow.ReferencesListItem(xr.Offset,
                    formatter.FormatOffset24(xr.Offset),
                    formatter.FormatAddress(mProject.GetAnattrib(xr.Offset).Address, showBank),
                    (xr.IsByName ? "Sym " : "Oth ") + typeStr + idxStr +
                        formatter.FormatAdjustment(-xr.Adjustment));

                mMainWin.ReferencesListAdd(rli);
            }

            // TODO(maybe): set the selection to something, instead of just inheriting it?
        }

        #endregion References panel

        #region Notes panel

        private void PopulateNotesList() {
            mMainWin.NotesList.Clear();
            foreach (KeyValuePair<int, MultiLineComment> kvp in mProject.Notes) {
                int offset = kvp.Key;
                MultiLineComment mlc = kvp.Value;

                // Replace line break with bullet.  If there's a single CRLF at the end, strip it.
                string nocrlfStr;
                if (mlc.Text.EndsWith("\r\n")) {
                    nocrlfStr =
                        mlc.Text.Substring(0, mlc.Text.Length - 2).Replace("\r\n", " \u2022 ");
                } else {
                    nocrlfStr = mlc.Text.Replace("\r\n", " \u2022 ");
                }

                MainWindow.NotesListItem nli = new MainWindow.NotesListItem(offset,
                    mFormatter.FormatOffset24(offset),
                    nocrlfStr,
                    mlc.BackgroundColor);
                mMainWin.NotesList.Add(nli);
            }
        }

        #endregion Notes panel

        #region Symbols panel

        /// <summary>
        /// Populates the ItemsSource for the Symbols window.  Each entry in the project
        /// symbol table is added.
        /// </summary>
        private void PopulateSymbolsList() {
            mMainWin.SymbolsList.Clear();
            foreach (Symbol sym in mProject.SymbolTable) {
                string valueStr;
                if (sym.SymbolSource == Symbol.Source.User && sym.Value == Address.NON_ADDR) {
                    valueStr = Address.NON_ADDR_STR;
                } else {
                    valueStr = mFormatter.FormatHexValue(sym.Value, 0);
                }
                string sourceTypeStr = sym.SourceTypeString;
                if (sym is DefSymbol) {
                    DefSymbol defSym = (DefSymbol)sym;
                    if (defSym.MultiMask != null) {
                        valueStr += " & " +
                            mFormatter.FormatHexValue(defSym.MultiMask.AddressMask, 4);
                    }
                    if (defSym.Direction == DefSymbol.DirectionFlags.Read) {
                        sourceTypeStr += '<';
                    } else if (defSym.Direction == DefSymbol.DirectionFlags.Write) {
                        sourceTypeStr += '>';
                    }
                }

                MainWindow.SymbolsListItem sli = new MainWindow.SymbolsListItem(sym,
                    sourceTypeStr, valueStr, sym.GenerateDisplayLabel(mFormatter));
                mMainWin.SymbolsList.Add(sli);
            }
        }

        #endregion Symbols panel

        #region Info panel

        private void UpdateInfoPanel() {
            const string CRLF = "\r\n";

            mMainWin.ClearInfoPanel();
            int selCount = mMainWin.CodeListView_GetSelectionCount();
            if (selCount < 1) {
                // Nothing selected.
                return;
            } else if (selCount > 1) {
                // Multiple lines selected.
                mMainWin.InfoLineDescrText = string.Format(Res.Strings.INFO_MULTI_LINE_SUM_FMT,
                    selCount);

                int firstIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
                int lastIndex = mMainWin.CodeListView_GetLastSelectedIndex();
                int firstOffset = CodeLineList[firstIndex].FileOffset;
                int nextOffset = CodeLineList[lastIndex].FileOffset +
                    CodeLineList[lastIndex].OffsetSpan;
                if (firstOffset == nextOffset) {
                    return;     // probably selected a bunch of lines from a long comment or note
                }
                if (firstOffset < 0 || nextOffset < 0) {
                    // We're in the header comment or .equ area.
                    return;
                }
                if (CodeLineList[lastIndex].LineType == LineListGen.Line.Type.ArEndDirective) {
                    nextOffset++;
                }

                StringBuilder msb = new StringBuilder();
                msb.AppendFormat(Res.Strings.INFO_MULTI_LINE_START_FMT, firstIndex,
                    mFormatter.FormatOffset24(firstOffset));
                msb.Append(CRLF);
                msb.AppendFormat(Res.Strings.INFO_MULTI_LINE_END_FMT, lastIndex,
                    mFormatter.FormatOffset24(nextOffset - 1));
                msb.Append(CRLF);
                int len = nextOffset - firstOffset;
                string lenStr = len.ToString() + " (" + mFormatter.FormatHexValue(len, 2) + ")";
                msb.AppendFormat(Res.Strings.INFO_MULTI_LINE_LEN_FMT, lenStr);
                mMainWin.InfoPanelDetail1 = msb.ToString();
                return;
            }

            int lineIndex = mMainWin.CodeListView_GetFirstSelectedIndex();
            LineListGen.Line line = CodeLineList[lineIndex];

            // TODO(someday): this should be made easier to localize
            string lineTypeStr = null;
            bool isSimple = true;
            DefSymbol defSym = null;
            switch (line.LineType) {
                case LineListGen.Line.Type.LongComment:
                    lineTypeStr = "comment";
                    break;
                case LineListGen.Line.Type.Note:
                    lineTypeStr = "note";
                    break;
                case LineListGen.Line.Type.Blank:
                    lineTypeStr = "blank line";
                    break;
                case LineListGen.Line.Type.RegWidthDirective:
                    lineTypeStr = "register width directive";
                    break;
                case LineListGen.Line.Type.DataBankDirective:
                    lineTypeStr = "data bank directive";
                    break;

                case LineListGen.Line.Type.ArStartDirective:
                    isSimple = false;
                    lineTypeStr = "address region start directive";
                    break;
                case LineListGen.Line.Type.ArEndDirective:
                    isSimple = false;
                    lineTypeStr = "address region end directive";
                    break;
                case LineListGen.Line.Type.LocalVariableTable:
                    isSimple = false;
                    lineTypeStr = "variable table";
                    break;
                case LineListGen.Line.Type.Code:
                    isSimple = false;
                    lineTypeStr = "code";
                    break;
                case LineListGen.Line.Type.Data:
                    isSimple = false;
                    if (mProject.GetAnattrib(line.FileOffset).IsInlineData) {
                        lineTypeStr = "inline data";
                    } else {
                        lineTypeStr = "data";
                    }
                    break;
                case LineListGen.Line.Type.EquDirective:
                    isSimple = false;
                    int defSymIndex = LineListGen.DefSymIndexFromOffset(line.FileOffset);
                    defSym = mProject.ActiveDefSymbolList[defSymIndex];
                    if (defSym.SymbolSource == Symbol.Source.Project) {
                        lineTypeStr = "project symbol equate";
                    } else if (defSym.SymbolSource == Symbol.Source.Platform) {
                        lineTypeStr = "platform symbol equate";
                    } else {
                        lineTypeStr = "???";
                    }
                    break;
                case LineListGen.Line.Type.VisualizationSet:
                    lineTypeStr = "visualization set";
                    break;
                default:
                    lineTypeStr = "???";
                    break;
            }

            if (line.IsCodeOrData) {
                // Show number of bytes of code/data.
                if (line.OffsetSpan == 1) {
                    mMainWin.InfoLineDescrText =
                        string.Format(Res.Strings.INFO_LINE_SUM_SINGULAR_FMT,
                            lineIndex, line.OffsetSpan, lineTypeStr);
                } else {
                    mMainWin.InfoLineDescrText =
                        string.Format(Res.Strings.INFO_LINE_SUM_PLURAL_FMT,
                            lineIndex, line.OffsetSpan, lineTypeStr);
                }
            } else {
                mMainWin.InfoLineDescrText = string.Format(Res.Strings.INFO_LINE_SUM_NON_FMT,
                    lineIndex, lineTypeStr);
            }

#if DEBUG
            mMainWin.InfoOffsetText = "[offset=+" + line.FileOffset.ToString("x6") +
                " sub=" + line.SubLineIndex + "]";
#endif
            if (isSimple) {
                return;
            }

            if (line.LineType == LineListGen.Line.Type.ArStartDirective ||
                    line.LineType == LineListGen.Line.Type.ArEndDirective) {
                AddressMap.AddressRegion region = CodeLineList.GetAddrRegionFromLine(line,
                    out bool isSynth);
                if (region == null) {
                    Debug.Assert(false, "Unable to find region at: " + line);
                    return;
                }
                StringBuilder esb = new StringBuilder();
                esb.Append("Address: ");
                if (region.Address == Address.NON_ADDR) {
                    esb.Append("non-addressable");
                } else {
                    esb.Append("$" +
                        mFormatter.FormatAddress(region.Address, !mProject.CpuDef.HasAddr16));
                }
                esb.Append(CRLF);
                esb.Append("Start: " + mFormatter.FormatOffset24(region.Offset));
                esb.Append(CRLF);
                esb.Append("End: ");
                esb.Append(mFormatter.FormatOffset24(region.Offset + region.ActualLength - 1));
                esb.Append(CRLF);
                esb.Append("Length: " + region.ActualLength + " (" +
                    mFormatter.FormatHexValue(region.ActualLength, 2) + ")");
                if (region.Length == AddressMap.FLOATING_LEN) {
                    esb.Append(" [floating]");
                }
                esb.Append(CRLF);
                esb.Append("Pre-label: ");
                if (!string.IsNullOrEmpty(region.PreLabel)) {
                    esb.Append("'");
                    esb.Append(region.PreLabel);
                    if (region.PreLabelAddress == Address.NON_ADDR) {
                        esb.Append("' (non-addressable)");
                    } else {
                        esb.Append("' addr=$");
                        esb.Append(mFormatter.FormatAddress(region.PreLabelAddress,
                            !mProject.CpuDef.HasAddr16));
                    }
                } else {
                    esb.Append("(not set)");
                }
                esb.Append(CRLF);
                esb.Append("Synthetic: " + isSynth);
                esb.Append(CRLF);
                esb.Append("Relative: " + region.IsRelative);
                esb.Append(CRLF);
                mMainWin.InfoPanelDetail1 = esb.ToString();
                return;
            }

            if (line.LineType == LineListGen.Line.Type.LocalVariableTable) {
                string str = string.Empty;
                if (mProject.LvTables.TryGetValue(line.FileOffset,
                        out LocalVariableTable lvt)) {
                    if (lvt.Count == 1) {
                        str = "1 entry";
                    } else {
                        str = lvt.Count + " entries";
                    }
                    if (lvt.ClearPrevious) {
                        str += "; clear previous";
                    }
                }
                mMainWin.InfoPanelDetail1 = str;
                return;
            }

            if (line.LineType == LineListGen.Line.Type.EquDirective) {
                StringBuilder esb = new StringBuilder();
                //esb.Append("\u25b6 ");
                esb.Append("\u2022 ");
                if (defSym.IsConstant) {
                    esb.Append("Constant");
                } else {
                    esb.Append("External address");
                    if (defSym.HasWidth) {
                        esb.Append(", width=");
                        esb.Append(defSym.DataDescriptor.Length);
                    }
                }
                if (defSym.Direction != DefSymbol.DirectionFlags.ReadWrite) {
                    esb.Append("\r\nI/O direction: ");
                    esb.Append(defSym.Direction);
                }
                if (defSym.MultiMask != null) {
                    esb.Append("\r\nMulti-mask:");
                    int i = 23;
                    if ((defSym.MultiMask.AddressMask | defSym.MultiMask.CompareMask |
                            defSym.MultiMask.CompareValue) < 0x10000) {
                        i = 15;
                    }
                    for ( ; i >= 0; i--) {
                        if ((i & 3) == 3) {
                            esb.Append(' ');
                        }
                        int bit = 1 << i;
                        if ((defSym.MultiMask.AddressMask & bit) != 0) {
                            esb.Append('x');
                        } else if ((defSym.MultiMask.CompareMask & bit) != 0) {
                            if ((defSym.MultiMask.CompareValue & bit) != 0) {
                                esb.Append('1');
                            } else {
                                esb.Append('0');
                            }
                        } else {
                            esb.Append('?');
                        }
                    }
                }
                if (defSym.SymbolSource == Symbol.Source.Platform) {
                    esb.Append("\r\n\r\nSource file # ");
                    esb.Append(defSym.LoadOrdinal);
                    esb.Append(": ");
                    esb.Append(defSym.FileIdentifier);

                    if (!string.IsNullOrEmpty(defSym.Tag)) {
                        esb.Append(", tag=");
                        esb.Append(defSym.Tag);
                    }
                }
                mMainWin.InfoPanelDetail1 = esb.ToString();
                return;
            }


            //
            // Handle code/data items.  In particular, the format descriptor.
            //
            Debug.Assert(line.IsCodeOrData);
            bool isCode = (line.LineType == LineListGen.Line.Type.Code);

            StringBuilder sb = new StringBuilder(250);
            Anattrib attr = mProject.GetAnattrib(line.FileOffset);

            if (attr.Symbol != null) {
                string descr;
                switch (attr.Symbol.SymbolType) {
                    case Symbol.Type.NonUniqueLocalAddr:
                        descr = "non-unique local";
                        break;
                    case Symbol.Type.LocalOrGlobalAddr:
                        descr = "unique local";
                        break;
                    case Symbol.Type.GlobalAddr:
                        descr = "unique global";
                        break;
                    case Symbol.Type.GlobalAddrExport:
                        descr = "global + marked for export";
                        break;
                    default:
                        descr = "???";
                        break;
                }
                if (attr.Symbol.SymbolSource == Symbol.Source.Auto) {
                    descr += ", auto-generated";
                } else if (attr.Symbol.LabelAnno == Symbol.LabelAnnotation.Generated) {
                    descr += " [gen]";
                }
                mMainWin.InfoLabelDescrText =
                    string.Format(Res.Strings.INFO_LABEL_DESCR_FMT, descr);
            }

            if (!mProject.OperandFormats.TryGetValue(line.FileOffset, out FormatDescriptor dfd)) {
                // No user-specified format, but there may be a generated format.
                mMainWin.InfoFormatBoxBrush = Brushes.Blue;
                if (attr.DataDescriptor != null) {
                    mMainWin.InfoFormatShowSolid = true;
                    sb.Append(Res.Strings.INFO_AUTO_FORMAT);
                    sb.Append(' ');
                    sb.Append(attr.DataDescriptor.ToUiString(!isCode));
                } else {
                    mMainWin.InfoFormatShowDashes = true;
                    sb.AppendFormat(Res.Strings.INFO_DEFAULT_FORMAT);
                }
            } else {
                // User-specified operand format.
                mMainWin.InfoFormatBoxBrush = Brushes.Green;
                mMainWin.InfoFormatShowSolid = true;
                sb.Append(Res.Strings.INFO_CUSTOM_FORMAT);
                sb.Append(' ');
                sb.Append(dfd.ToUiString(!isCode));
            }
            mMainWin.InfoFormatText = sb.ToString();

            sb.Clear();

            // Debug only
            //sb.Append("DEBUG: opAddr=" + attr.OperandAddress.ToString("x4") +
            //    " opOff=" + attr.OperandOffset.ToString("x4") + "\r\n");

            if (attr.NoContinueScript) {
                sb.AppendLine("\"No-continue\" flag set by script");
            }
            if (attr.HasAnalyzerTag) {
                sb.Append("\u2022 Analyzer Tags: ");
                for (int i = 0; i < line.OffsetSpan; i++) {
                    switch (mProject.AnalyzerTags[line.FileOffset + i]) {
                        case CodeAnalysis.AnalyzerTag.Code:
                            sb.Append("S");     // S)tart
                            break;
                        case CodeAnalysis.AnalyzerTag.Data:
                            sb.Append("E");     // E)nd
                            break;
                        case CodeAnalysis.AnalyzerTag.InlineData:
                            sb.Append("I");     // I)nline
                            break;
                        default:
                            break;
                    }
                    if (i > 8) {
                        sb.Append("...");
                        break;
                    }
                }
                sb.Append("\r\n");
            }

            if (attr.IsInstruction) {
                sb.Append("\r\n");

                Asm65.OpDef op = mProject.CpuDef.GetOpDef(mProject.FileData[line.FileOffset]);

                string shortDesc = mOpDesc.GetShortDescription(op.Mnemonic);
                if (!string.IsNullOrEmpty(shortDesc)) {
                    if (op.IsUndocumented) {
                        sb.Append("\u25b6[*] ");
                    } else {
                        sb.Append("\u25b6 ");
                    }
                    sb.Append(shortDesc);
                    string addrStr = mOpDesc.GetAddressModeDescription(op.AddrMode);
                    if (!string.IsNullOrEmpty(addrStr)) {
                        sb.Append(", ");
                        sb.Append(addrStr);
                    }
                    sb.Append("\r\n");
                }

                sb.Append("\u2022Cycles: ");
                int cycles = op.Cycles;
                sb.Append(cycles.ToString());

                OpDef.CycleMod allMods = op.CycleMods;
                OpDef.CycleMod nowMods =
                    mProject.CpuDef.GetOpCycleMod(mProject.FileData[line.FileOffset]);
                if (allMods != 0) {
                    StringBuilder nowSb = new StringBuilder();
                    StringBuilder otherSb = new StringBuilder();
                    int workBits = (int)allMods;
                    while (workBits != 0) {
                        // Isolate rightmost bit.
                        int firstBit = (~workBits + 1) & workBits;

                        string desc = mOpDesc.GetCycleModDescription((OpDef.CycleMod)firstBit);
                        if (((int)nowMods & firstBit) != 0) {
                            if (nowSb.Length != 0) {
                                nowSb.Append(", ");
                            }
                            nowSb.Append(desc);
                        } else {
                            if (otherSb.Length != 0) {
                                otherSb.Append(", ");
                            }
                            otherSb.Append(desc);
                        }
                        // Remove from set.
                        workBits &= ~firstBit;
                    }
                    if (nowSb.Length != 0) {
                        sb.Append(" (");
                        sb.Append(nowSb);
                        sb.Append(")");
                    }
                    if (otherSb.Length != 0) {
                        sb.Append(" [");
                        sb.Append(otherSb);
                        sb.Append("]");
                    }
                }
                sb.Append("\r\n");

                const string FLAGS = "NVMXDIZC";
                sb.Append("\u2022Flags affected: ");
                Asm65.StatusFlags affectedFlags = op.FlagsAffected;
                for (int i = 0; i < 8; i++) {
                    if (affectedFlags.GetBit((StatusFlags.FlagBits)(7 - i)) >= 0) {
                        sb.Append(' ');
                        sb.Append(FLAGS[i]);
                    } else {
                        sb.Append(" -");
                    }
                }
                sb.Append("\r\n");

                string longDesc = mOpDesc.GetLongDescription(op.Mnemonic);
                if (!string.IsNullOrEmpty(longDesc)) {
                    sb.Append("\r\n");
                    sb.Append(longDesc);
                }
            }

            // Publish
            mMainWin.InfoPanelDetail1 = sb.ToString();
        }

        #endregion Info panel

        #region Tools

        public void ToggleApple2ScreenChart() {
            if (mApple2ScreenChartDialog == null) {
                // Create without owner so it doesn't have to be in front of main window.
                mApple2ScreenChartDialog = new Tools.WpfGui.Apple2ScreenChart(null, mFormatter);
                mApple2ScreenChartDialog.Closing += (sender, e) => {
                    Debug.WriteLine("Apple II screen chart closed");
                    mApple2ScreenChartDialog = null;
                };
                mApple2ScreenChartDialog.Show();
            } else {
                mApple2ScreenChartDialog.Close();
            }
        }

        public void ToggleAsciiChart() {
            if (mAsciiChartDialog == null) {
                // Create without owner so it doesn't have to be in front of main window.
                mAsciiChartDialog = new Tools.WpfGui.AsciiChart(null);
                mAsciiChartDialog.Closing += (sender, e) => {
                    Debug.WriteLine("ASCII chart closed");
                    mAsciiChartDialog = null;
                };
                mAsciiChartDialog.Show();
            } else {
                mAsciiChartDialog.Close();
            }
        }

        public void ToggleInstructionChart() {
            if (mInstructionChartDialog == null) {
                // Create without owner so it doesn't have to be in front of main window.
                mInstructionChartDialog = new Tools.WpfGui.InstructionChart(null, mFormatter);
                mInstructionChartDialog.Closing += (sender, e) => {
                    Debug.WriteLine("Instruction chart closed");
                    mInstructionChartDialog = null;
                };
                mInstructionChartDialog.Show();
            } else {
                mInstructionChartDialog.Close();
            }
        }

        public void ShowFileHexDump() {
            if (!OpenAnyFile(null, out string pathName)) {
                return;
            }
            FileInfo fi = new FileInfo(pathName);
            if (fi.Length > Tools.WpfGui.HexDumpViewer.MAX_LENGTH) {
                string msg = string.Format(Res.Strings.OPEN_DATA_TOO_LARGE_FMT,
                    fi.Length / 1024, Tools.WpfGui.HexDumpViewer.MAX_LENGTH / 1024);
                MessageBox.Show(msg, Res.Strings.OPEN_DATA_FAIL_CAPTION,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            byte[] data;
            try {
                data = File.ReadAllBytes(pathName);
            } catch (Exception ex) {
                // not expecting this to happen
                MessageBox.Show(ex.Message);
                return;
            }

            // Create the dialog without an owner, and add it to the "unowned" list.
            Tools.WpfGui.HexDumpViewer dlg = new Tools.WpfGui.HexDumpViewer(null,
                data, mFormatter);
            dlg.SetFileName(Path.GetFileName(pathName));
            dlg.Closing += (sender, e) => {
                Debug.WriteLine("Window " + dlg + " closed, removing from unowned list");
                mUnownedWindows.Remove(dlg);
            };
            mUnownedWindows.Add(dlg);
            dlg.Show();
        }

        public void ConcatenateFiles() {
            Tools.WpfGui.FileConcatenator concat =
                new Tools.WpfGui.FileConcatenator(this.mMainWin);
            concat.ShowDialog();
        }

        public void SliceFiles() {
            if (!OpenAnyFile(null, out string pathName)) {
                return;
            }

            Tools.WpfGui.FileSlicer slicer = new Tools.WpfGui.FileSlicer(this.mMainWin, pathName,
                mFormatter);
            slicer.ShowDialog();
        }

        public void ConvertOmf() {
            if (!OpenAnyFile(Res.Strings.OMF_SELECT_FILE, out string pathName)) {
                return;
            }

            // Load the file here, so basic problems like empty / oversized files can be
            // reported immediately.

            byte[] fileData = null;
            using (FileStream fs = File.Open(pathName, FileMode.Open, FileAccess.Read)) {
                string errMsg = null;

                if (fs.Length == 0) {
                    errMsg = Res.Strings.OPEN_DATA_EMPTY;
                } else if (fs.Length < Tools.Omf.OmfFile.MIN_FILE_SIZE) {
                    errMsg = string.Format(Res.Strings.OPEN_DATA_TOO_SMALL_FMT, fs.Length);
                } else if (fs.Length > Tools.Omf.OmfFile.MAX_FILE_SIZE) {
                    errMsg = string.Format(Res.Strings.OPEN_DATA_TOO_LARGE_FMT,
                        fs.Length / 1024, Tools.Omf.OmfFile.MAX_FILE_SIZE / 1024);
                }
                if (errMsg == null) {
                    fileData = new byte[fs.Length];
                    int actual = fs.Read(fileData, 0, (int)fs.Length);
                    if (actual != fs.Length) {
                        errMsg = Res.Strings.OPEN_DATA_PARTIAL_READ;
                    }
                }

                if (errMsg != null) {
                    MessageBox.Show(errMsg, Res.Strings.ERR_FILE_GENERIC_CAPTION,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            Tools.Omf.WpfGui.OmfViewer ov =
                new Tools.Omf.WpfGui.OmfViewer(this.mMainWin, pathName, fileData, mFormatter);
            ov.ShowDialog();
        }

        private bool OpenAnyFile(string title, out string pathName) {
            OpenFileDialog fileDlg = new OpenFileDialog() {
                Filter = Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1
            };
            if (title != null) {
                fileDlg.Title = title;
            }
            if (fileDlg.ShowDialog() != true) {
                pathName = null;
                return false;
            }
            pathName = Path.GetFullPath(fileDlg.FileName);
            return true;
        }

        /// <summary>
        /// Displays a representation of the address map.
        /// </summary>
        /// <remarks>
        /// This is in the "tools" section, but it's not a tool.  It's in the "navigation" menu
        /// but has nothing to do with navigation.  Bit of an oddball.
        /// </remarks>
        public void ViewAddressMap() {
            string mapStr = RenderAddressMap.GenerateString(mProject, mFormatter);

            Tools.WpfGui.ShowText dlg = new Tools.WpfGui.ShowText(mMainWin, mapStr);
            dlg.Title = "Address Map";
            dlg.ShowDialog();
        }

        /// <summary>
        /// Shows a table of references in a non-modial dialog window.  If the window is already
        /// being displayed, the contents are replaced.
        /// </summary>
        public void ShowReferenceTable(List<ReferenceTable.ReferenceTableItem> items) {
            if (mReferenceTableDialog == null) {
                // Create without owner so it doesn't have to be in front of main window.
                mReferenceTableDialog = new ReferenceTable(null, this);
                mReferenceTableDialog.Closing += (sender, e) => {
                    Debug.WriteLine("Reference table dialog closed");
                    mReferenceTableDialog = null;
                };
                mReferenceTableDialog.Show();
            }

            mReferenceTableDialog.SetItems(items);
        }

        #endregion Tools

        #region Debug features

        /// <summary>
        /// If set, show rulers and visible spaces in long comments.
        /// </summary>
        internal bool DebugLongComments { get; private set; } = false;

        public void Debug_ExtensionScriptInfo() {
            string info = mProject.DebugGetLoadedScriptInfo();

            Tools.WpfGui.ShowText dlg = new Tools.WpfGui.ShowText(mMainWin, info);
            dlg.Title = "Loaded Extension Script Info";
            dlg.ShowDialog();
        }

        public void Debug_ShowAnalysisTimers() {
            if (mShowAnalysisTimersDialog == null) {
                Tools.WpfGui.ShowText dlg = new Tools.WpfGui.ShowText(null, "(no data yet)");
                dlg.Title = "Analysis Timers";
                dlg.Closing += (sender, e) => {
                    Debug.WriteLine("Analysis timers dialog closed");
                    mShowAnalysisTimersDialog = null;
                };
                dlg.Show();
                mShowAnalysisTimersDialog = dlg;
            } else {
                // Ask the dialog to close.  Do the cleanup in the event.
                mShowAnalysisTimersDialog.Close();
            }
        }

        public void Debug_ShowAnalyzerOutput() {
            if (mShowAnalyzerOutputDialog == null) {
                Tools.WpfGui.ShowText dlg = new Tools.WpfGui.ShowText(null, "(no data yet)");
                dlg.Title = "Analyzer Output";
                dlg.Closing += (sender, e) => {
                    Debug.WriteLine("Analyzer output dialog closed");
                    mShowAnalyzerOutputDialog = null;
                };
                dlg.Show();
                mShowAnalyzerOutputDialog = dlg;
            } else {
                // Ask the dialog to close.  Do the cleanup in the event.
                mShowAnalyzerOutputDialog.Close();
            }
        }

        public void Debug_ShowUndoRedoHistory() {
            if (mShowUndoRedoHistoryDialog == null) {
                Tools.WpfGui.ShowText dlg = new Tools.WpfGui.ShowText(null,
                    mProject.DebugGetUndoRedoHistory());
                dlg.Title = "Undo/Redo History";
                dlg.Closing += (sender, e) => {
                    Debug.WriteLine("Undo/redo history dialog closed");
                    mShowUndoRedoHistoryDialog = null;
                };
                dlg.Show();
                mShowUndoRedoHistoryDialog = dlg;
            } else {
                // Ask the dialog to close.  Do the cleanup in the event.
                mShowUndoRedoHistoryDialog.Close();
            }
        }

        public void Debug_RunSourceGenerationTests() {
            Tests.WpfGui.GenTestRunner dlg = new Tests.WpfGui.GenTestRunner(mMainWin);
            dlg.ShowDialog();
        }

        public void Debug_Refresh() {
            Debug.WriteLine("Reanalyzing...");
            // Call through ApplyChanges so we update the timer task output.
            UndoableChange uc =
                UndoableChange.CreateDummyChange(UndoableChange.ReanalysisScope.CodeAndData);
            ApplyChanges(new ChangeSet(uc), false);
            UpdateTitle();  // in case something changed
        }

        public void Debug_ToggleCommentRulers() {
            DebugLongComments = !DebugLongComments;
            mFormatterConfig.DebugLongComments = DebugLongComments;
            mFormatterConfig.AddSpaceLongComment = !DebugLongComments;
            mFormatterCpuDef = null;        // force mFormatter refresh on next analysis
            if (CodeLineList != null) {
                UndoableChange uc =
                    UndoableChange.CreateDummyChange(UndoableChange.ReanalysisScope.DisplayOnly);
                ApplyChanges(new ChangeSet(uc), false);
            }
        }

        public void Debug_ToggleKeepAliveHack() {
            ScriptManager.UseKeepAliveHack = !ScriptManager.UseKeepAliveHack;
            if (mProject != null) {
                MessageBox.Show("Project must be closed and re-opened for change to take effect");
            }
        }

        public void Debug_ToggleSecuritySandbox() {
            UseMainAppDomainForPlugins = !UseMainAppDomainForPlugins;
            if (mProject != null) {
                MessageBox.Show("Project must be closed and re-opened for change to take effect");
            }
        }

        public void Debug_ApplesoftToHtml() {
            if (!OpenAnyFile(null, out string basPathName)) {
                return;
            }

            byte[] data;
            try {
                data = File.ReadAllBytes(basPathName);
            } catch (IOException ex) {
                // not expecting this to happen
                MessageBox.Show(ex.Message);
                return;
            }

            Tools.ApplesoftToHtml conv = new Tools.ApplesoftToHtml();
            string html = conv.Convert(data);

            Tools.WpfGui.ShowText showTextDlg = new Tools.WpfGui.ShowText(mMainWin, html);
            showTextDlg.Title = "Applesoft to HTML";
            showTextDlg.ShowDialog();
        }

        public void Debug_ExportEditCommands() {
            SaveFileDialog fileDlg = new SaveFileDialog() {
                Filter = Res.Strings.FILE_FILTER_SGEC + "|" + Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1,
                ValidateNames = true,
                AddExtension = true,
                FileName = Path.GetFileName(mDataPathName) + Sgec.SGEC_EXT
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }
            string sgecPathName = Path.GetFullPath(fileDlg.FileName);

            MessageBoxResult res = MessageBox.Show("Use relative offsets?", "Question",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            bool resMode;
            if (res == MessageBoxResult.Cancel) {
                return;
            } else if (res == MessageBoxResult.Yes) {
                resMode = true;
            } else {
                resMode = false;
            }

            if (!Sgec.ExportToFile(sgecPathName, mProject, resMode, out string detailMsg)) {
                MessageBox.Show("Failed: " + detailMsg);
            } else {
                MessageBox.Show("Success: " + detailMsg);
            }
        }

        public void Debug_ApplyEditCommands() {
            // Might want to suggest disabling Edit > Toggle Data Scan for some merges.
            OpenFileDialog fileDlg = new OpenFileDialog() {
                Filter = Res.Strings.FILE_FILTER_SGEC + "|" + Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }

            string sgecPathName = Path.GetFullPath(fileDlg.FileName);

            ChangeSet cs = new ChangeSet(1);
            if (!Sgec.ImportFromFile(sgecPathName, mProject, cs, out string detailMsg)) {
                MessageBox.Show("Failed: " + detailMsg);
            } else {
                if (cs.Count != 0) {
                    ApplyUndoableChanges(cs);
                    MessageBox.Show("Success: " + detailMsg);
                } else {
                    MessageBox.Show("Success; no changes were made.");
                }
            }
        }

        // Disable "analyze uncategorized data" for best results.
        public void Debug_ApplyExternalSymbols() {
            ChangeSet cs = new ChangeSet(1);

            MessageBoxResult result =
                MessageBox.Show("Apply project symbols (in addition to platform symbols)?",
                "Apply project symbols?", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            bool doProject;
            if (result == MessageBoxResult.Cancel) {
                return;
            } else {
                doProject = (result == MessageBoxResult.Yes);
            }

            foreach (Symbol sym in mProject.SymbolTable) {
                if (sym.SymbolSource != Symbol.Source.Platform &&
                        (!doProject || sym.SymbolSource != Symbol.Source.Project)) {
                    continue;
                }
                DefSymbol defSym = (DefSymbol)sym;
                if (defSym.MultiMask != null) {
                    // These would require additional work... probably.
                    continue;
                }

                int offset = mProject.AddrMap.AddressToOffset(0, sym.Value);
                if (offset < 0) {
                    continue;
                }

                // Make sure this is the start of an instruction or data item.  (If you
                // haven't finished tagging code start points, it's best to disable the
                // string/fill finder.)
                Anattrib attr = mProject.GetAnattrib(offset);
                if (!attr.IsStart) {
                    Debug.WriteLine("Found match at non-start +" + offset.ToString("x6") +
                        ": " + defSym);
                    continue;
                }

                // Check for user label.  Okay to overwrite auto label.
                if (mProject.UserLabels.ContainsKey(offset)) {
                    Debug.WriteLine("User label already exists at +" + offset.ToString("x6"));
                    continue;
                }

                // Create a new user label symbol.  We should not be creating a duplicate name,
                // because user labels have priority over platform symbols when populating
                // the symbol table.
                Symbol newSym = new Symbol(sym.Label, sym.Value, Symbol.Source.User,
                    Symbol.Type.GlobalAddr, Symbol.LabelAnnotation.None);
                UndoableChange uc = UndoableChange.CreateLabelChange(offset, null, newSym);
                cs.Add(uc);
            }

            if (cs.Count == 0) {
                MessageBox.Show("No changes made.");
            } else {
                ApplyUndoableChanges(cs);
                MessageBox.Show("Set " + cs.Count + " labels.");
            }
        }

        public void Debug_RebootSecuritySandbox() {
            Debug.Assert(mProject != null);
            mProject.DebugRebootSandbox();
        }

        #endregion Debug features
    }
}
