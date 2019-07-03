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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Asm65;

namespace SourceGenWPF.WpfGui {
    /// <summary>
    /// Project properties dialog.
    /// </summary>
    public partial class EditProjectProperties : Window, INotifyPropertyChanged {
        /// <summary>
        /// New set.  Updated when Apply or OK is hit.  This will be null if no changes have
        /// been applied.
        /// </summary>
        public ProjectProperties NewProps { get; private set; }

        /// <summary>
        /// Format object to use when formatting addresses and constants.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// Working set.  Used internally to hold state.
        /// </summary>
        private ProjectProperties mWorkProps;

        /// <summary>
        /// Dirty flag.  Determines whether or not the Apply button is enabled.
        /// </summary>
        /// <remarks>
        /// Ideally this would just be "WorkProps != OldProps", but it doesn't
        /// seem worthwhile to maintain an equality operator.
        /// </remarks>
        public bool IsDirty {
            get { return mIsDirty; }
            set {
                mIsDirty = value;
                OnPropertyChanged();
            }
        }
        private bool mIsDirty;

        /// <summary>
        /// Project directory, if one has been established; otherwise empty.
        /// </summary>
        private string mProjectDir;


        /// <summary>
        /// Constructor.  Initial state is configured from an existing ProjectProperties object.
        /// </summary>
        /// <param name="props">Property holder to clone.</param>
        /// <param name="projectDir">Project directory, if known.</param>
        /// <param name="formatter">Text formatter.</param>
        public EditProjectProperties(Window owner, ProjectProperties props, string projectDir,
                Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mWorkProps = new ProjectProperties(props);
            mProjectDir = projectDir;
            mFormatter = formatter;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Loaded_General();

#if false
            LoadProjectSymbols();
            LoadPlatformSymbolFiles();
            LoadExtensionScriptNames();
#endif
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e) {
            NewProps = new ProjectProperties(mWorkProps);
            IsDirty = false;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            // TODO(maybe): ideally we'd return false here if nothing has changed from the
            // state things were in when the dialog first opened.  Checking IsDirty is
            // insufficient.  Might be best to just let the UndoableChange stuff figure out
            // that nothing changed.
            NewProps = new ProjectProperties(mWorkProps);
            DialogResult = true;
        }

        #region General

        // CPU combo box items
        public class CpuItem {
            public string Name { get; private set; }
            public CpuDef.CpuType Type { get; private set; }

            public CpuItem(string name, CpuDef.CpuType type) {
                Name = name;
                Type = type;
            }
        }
        private static CpuItem[] sCpuItems = {
            new CpuItem("MOS 6502", CpuDef.CpuType.Cpu6502),
            new CpuItem("WDC W65C02S", CpuDef.CpuType.Cpu65C02),
            new CpuItem("WDC W65C816S", CpuDef.CpuType.Cpu65816),
        };
        public CpuItem[] CpuItems { get { return sCpuItems; } }

        // Min chars for string combo box items
        public class MinCharsItem {
            public string Name { get; private set; }
            public int Value { get; private set; }

            public MinCharsItem(string name, int value) {
                Name = name;
                Value = value;
            }
        }
        private static MinCharsItem[] sMinCharsItems = {
            new MinCharsItem("None (disabled)", DataAnalysis.MIN_CHARS_FOR_STRING_DISABLED),
            new MinCharsItem("3", 3),
            new MinCharsItem("4", 4),
            new MinCharsItem("5", 5),
            new MinCharsItem("6", 6),
            new MinCharsItem("7", 7),
            new MinCharsItem("8", 8),
            new MinCharsItem("9", 9),
            new MinCharsItem("10", 10),
        };
        public MinCharsItem[] MinCharsItems { get { return sMinCharsItems; } }

        // Auto-label style combo box items
        public class AutoLabelItem {
            public string Name { get; private set; }
            public AutoLabel.Style Style { get; private set; }

            public AutoLabelItem(string name, AutoLabel.Style style) {
                Name = name;
                Style = style;
            }
        }
        private static AutoLabelItem[] sAutoLabelItems = {
            new AutoLabelItem("Simple (\"L1234\")", AutoLabel.Style.Simple),
            new AutoLabelItem("Annotated (\"W_1234\")", AutoLabel.Style.Annotated),
            new AutoLabelItem("Fully Annotated (\"DWR_1234\")", AutoLabel.Style.FullyAnnotated),
        };
        public AutoLabelItem[] AutoLabelItems {  get { return sAutoLabelItems; } }

        // properties for checkboxes

        public bool IncludeUndocumentedInstr {
            get { return mWorkProps.IncludeUndocumentedInstr; }
            set {
                mWorkProps.IncludeUndocumentedInstr = value;
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool AnalyzeUncategorizedData {
            get { return mWorkProps.AnalysisParams.AnalyzeUncategorizedData; }
            set {
                mWorkProps.AnalysisParams.AnalyzeUncategorizedData = value;
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool SeekNearbyTargets {
            get { return mWorkProps.AnalysisParams.SeekNearbyTargets; }
            set {
                mWorkProps.AnalysisParams.SeekNearbyTargets = value;
                OnPropertyChanged();
                IsDirty = true;
            }
        }

        private void Loaded_General() {
            for (int i = 0; i < CpuItems.Length; i++) {
                if (CpuItems[i].Type == mWorkProps.CpuType) {
                    cpuComboBox.SelectedItem = CpuItems[i];
                    break;
                }
            }
            if (cpuComboBox.SelectedItem == null) {
                cpuComboBox.SelectedIndex = 0;
            }

            for (int i = 0; i < MinCharsItems.Length; i++) {
                if (MinCharsItems[i].Value == mWorkProps.AnalysisParams.MinCharsForString) {
                    minStringCharsComboBox.SelectedItem = MinCharsItems[i];
                    break;
                }
            }
            if (minStringCharsComboBox.SelectedItem == null) {
                minStringCharsComboBox.SelectedIndex = 2;
            }

            for (int i = 0; i < AutoLabelItems.Length; i++) {
                if (AutoLabelItems[i].Style == mWorkProps.AutoLabelStyle) {
                    autoLabelStyleComboBox.SelectedItem = AutoLabelItems[i];
                    break;
                }
            }
            if (autoLabelStyleComboBox.SelectedItem == null) {
                autoLabelStyleComboBox.SelectedIndex = 0;
            }

            UpdateEntryFlags();
        }

        private void UpdateEntryFlags() {
            const string FLAGS = "CZIDXMVNE";   // flags, in order low to high, plus emu bit
            const string VALUES = "-?01";
            StringBuilder sb = new StringBuilder(27);
            StatusFlags flags = mWorkProps.EntryFlags;
            for (int i = 0; i < 9; i++) {
                // Want to show P reg flags (first 8) in conventional high-to-low order.
                int idx = (7 - i) + (i == 8 ? 9 : 0);
                int val = flags.GetBit((StatusFlags.FlagBits)idx);
                sb.Append(FLAGS[idx]);
                sb.Append(VALUES[val + 2]);
                sb.Append(' ');
            }

            currentFlagsText.Text = sb.ToString();
        }


        private void CpuComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            CpuItem item = (CpuItem)cpuComboBox.SelectedItem;
            mWorkProps.CpuType = item.Type;
            IsDirty = true;
        }

        private void MinStringCharsComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            MinCharsItem item = (MinCharsItem)minStringCharsComboBox.SelectedItem;
            mWorkProps.AnalysisParams.MinCharsForString = item.Value;
            IsDirty = true;
        }

        private void AutoLabelStyleComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            AutoLabelItem item = (AutoLabelItem)autoLabelStyleComboBox.SelectedItem;
            mWorkProps.AutoLabelStyle = item.Style;
            IsDirty = true;
        }

        private void ChangeFlagButton_Click(object sender, RoutedEventArgs e) {
            CpuDef cpuDef = CpuDef.GetBestMatch(mWorkProps.CpuType,
                mWorkProps.IncludeUndocumentedInstr);
            EditStatusFlags dlg =
                new EditStatusFlags(this, mWorkProps.EntryFlags, cpuDef.HasEmuFlag);

            dlg.ShowDialog();
            if (dlg.DialogResult == true) {
                if (mWorkProps.EntryFlags != dlg.FlagValue) {
                    // Flags changed.
                    mWorkProps.EntryFlags = dlg.FlagValue;
                    UpdateEntryFlags();
                    IsDirty = true;
                }
            }
        }

        #endregion General
    }
}
