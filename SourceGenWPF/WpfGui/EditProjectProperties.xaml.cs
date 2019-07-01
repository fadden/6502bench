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
using System.Windows;

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
#if false
            // Configure CPU chooser.  This must match the order of strings in the designer.
            switch (mWorkProps.CpuType) {
                case CpuDef.CpuType.Cpu6502:
                    cpuComboBox.SelectedIndex = 0;
                    break;
                case CpuDef.CpuType.Cpu65C02:
                    cpuComboBox.SelectedIndex = 1;
                    break;
                case CpuDef.CpuType.Cpu65816:
                    cpuComboBox.SelectedIndex = 2;
                    break;
                default:
                    Debug.Assert(false);
                    cpuComboBox.SelectedIndex = 0;
                    break;
            }

            undocInstrCheckBox.Checked = mWorkProps.IncludeUndocumentedInstr;
            analyzeUncategorizedCheckBox.Checked =
                mWorkProps.AnalysisParams.AnalyzeUncategorizedData;
            seekAltTargetCheckBox.Checked =
                mWorkProps.AnalysisParams.SeekNearbyTargets;

            int matchLen = mWorkProps.AnalysisParams.MinCharsForString;
            int selIndex;
            if (matchLen == DataAnalysis.MIN_CHARS_FOR_STRING_DISABLED) {
                selIndex = 0;       // disabled
            } else {
                selIndex = matchLen - 2;
            }
            if (selIndex < 0 || selIndex >= minStringCharsComboBox.Items.Count) {
                Debug.Assert(false, "bad MinCharsForString " + matchLen);
                selIndex = 0;
            }
            minStringCharsComboBox.SelectedIndex = selIndex;

            selIndex = (int)mWorkProps.AutoLabelStyle;
            if (selIndex < 0 || selIndex >= autoLabelStyleComboBox.Items.Count) {
                Debug.Assert(false, "bad AutoLabelStyle " + mWorkProps.AutoLabelStyle);
                selIndex = 0;
            }
            autoLabelStyleComboBox.SelectedIndex = selIndex;

            LoadProjectSymbols();
            LoadPlatformSymbolFiles();
            LoadExtensionScriptNames();

            // Various callbacks will have fired while configuring controls.  Reset to "clean".
            mDirty = false;
            UpdateControls();
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
    }
}
