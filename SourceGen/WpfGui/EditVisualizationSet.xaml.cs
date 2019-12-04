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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Asm65;
using PluginCommon;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Visualization set editor.
    /// </summary>
    public partial class EditVisualizationSet : Window, INotifyPropertyChanged {
        public VisualizationSet NewVisSet { get; private set; }

        private DisasmProject mProject;
        private Formatter mFormatter;
        private VisualizationSet mOrigSet;
        private int mOffset;

        public ObservableCollection<Visualization> VisualizationList { get; private set; } =
            new ObservableCollection<Visualization>();

        /// <summary>
        /// True if there are plugins that implement the visualization generation interface.
        /// </summary>
        public bool HasVisPlugins {
            get { return mHasVisPlugins; }
            set { mHasVisPlugins = value; OnPropertyChanged(); }
        }
        private bool mHasVisPlugins;

        public Visibility ScriptWarningVisible {
            get { return mHasVisPlugins ? Visibility.Collapsed : Visibility.Visible; }
            // this can't change while the dialog is open, so don't need OnPropertyChanged
        }

        public bool IsEditEnabled {
            get { return mIsEditEnabled; }
            set { mIsEditEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsEditEnabled;

        public bool IsRemoveEnabled {
            get { return mIsRemoveEnabled; }
            set { mIsRemoveEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsRemoveEnabled;

        public bool IsUpEnabled {
            get { return mIsUpEnabled; }
            set { mIsUpEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsUpEnabled;

        public bool IsDownEnabled {
            get { return mIsDownEnabled; }
            set { mIsDownEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsDownEnabled;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditVisualizationSet(Window owner, DisasmProject project, Formatter formatter,
                VisualizationSet curSet, int offset) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = project;
            mFormatter = formatter;
            mOrigSet = curSet;
            mOffset = offset;

            if (curSet != null) {
                // Populate the data grid ItemsSource.
                foreach (Visualization vis in curSet) {
                    VisualizationList.Add(vis);
                }
            }
            if (VisualizationList.Count > 0) {
                visualizationGrid.SelectedIndex = 0;
            }

            // Check to see if we have any relevant plugins.  If not, disable New/Edit.
            List<IPlugin> plugins = project.GetActivePlugins();
            foreach (IPlugin chkPlug in plugins) {
                if (chkPlug is IPlugin_Visualizer) {
                    HasVisPlugins = true;
                    break;
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            NewVisSet = MakeVisSet();
            DialogResult = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            if (DialogResult == true) {
                return;
            }

            // Check to see if changes have been made.
            VisualizationSet newSet = MakeVisSet();
            if (newSet != mOrigSet) {
                string msg = (string)FindResource("str_ConfirmDiscardChanges");
                string caption = (string)FindResource("str_ConfirmDiscardChangesCaption");
                MessageBoxResult result = MessageBox.Show(msg, caption, MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel) {
                    e.Cancel = true;
                }
            }
        }

        private VisualizationSet MakeVisSet() {
            if (VisualizationList.Count == 0) {
                return null;
            }
            VisualizationSet newSet = new VisualizationSet(VisualizationList.Count);
            foreach (Visualization vis in VisualizationList) {
                newSet.Add(vis);
            }
            return newSet;
        }

        private void VisualizationList_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            bool isItemSelected = (visualizationGrid.SelectedItem != null);
            IsEditEnabled = HasVisPlugins && isItemSelected;
            IsRemoveEnabled = isItemSelected;
            IsUpEnabled = isItemSelected && visualizationGrid.SelectedIndex != 0;
            IsDownEnabled = isItemSelected &&
                visualizationGrid.SelectedIndex != VisualizationList.Count - 1;
        }

        private void VisualizationList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            EditSelectedItem();
        }

        private void NewButton_Click(object sender, RoutedEventArgs e) {
            EditVisualization dlg = new EditVisualization(this, mProject, mFormatter, mOffset,
                null);
            if (dlg.ShowDialog() != true) {
                return;
            }
            VisualizationList.Add(dlg.NewVis);
            visualizationGrid.SelectedIndex = VisualizationList.Count - 1;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            EditSelectedItem();
        }

        private void EditSelectedItem() {
            if (!IsEditEnabled) {
                // can happen on a double-click
                return;
            }
            Visualization item = (Visualization)visualizationGrid.SelectedItem;

            EditVisualization dlg = new EditVisualization(this, mProject, mFormatter, mOffset,
                item);
            if (dlg.ShowDialog() != true) {
                return;
            }

            int index = VisualizationList.IndexOf(item);
            VisualizationList.Remove(item);
            VisualizationList.Insert(index, dlg.NewVis);
            visualizationGrid.SelectedIndex = index;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e) {
            Visualization item = (Visualization)visualizationGrid.SelectedItem;
            int index = VisualizationList.IndexOf(item);
            VisualizationList.Remove(item);
            if (index == VisualizationList.Count) {
                index--;
            }
            if (index >= 0) {
                visualizationGrid.SelectedIndex = index;
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e) {
            Visualization item = (Visualization)visualizationGrid.SelectedItem;
            int index = VisualizationList.IndexOf(item);
            Debug.Assert(index > 0);
            VisualizationList.Remove(item);
            VisualizationList.Insert(index - 1, item);
            visualizationGrid.SelectedIndex = index - 1;
        }

        private void DownButton_Click(object sender, RoutedEventArgs e) {
            Visualization item = (Visualization)visualizationGrid.SelectedItem;
            int index = VisualizationList.IndexOf(item);
            Debug.Assert(index < VisualizationList.Count - 1);
            VisualizationList.Remove(item);
            VisualizationList.Insert(index + 1, item);
            visualizationGrid.SelectedIndex = index + 1;
        }
    }
}
