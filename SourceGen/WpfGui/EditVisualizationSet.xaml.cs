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
        /// <summary>
        /// Modified visualization set.  Only valid after OK is hit.
        /// </summary>
        public VisualizationSet NewVisSet { get; private set; }

        /// <summary>
        /// List of Visualization serial numbers that were removed from the set.  The caller
        /// can use this to update animations in other sets that referred to the removed items.
        /// </summary>
        /// <remarks>
        /// We have to use serial numbers because the user might have edited the Visualization
        /// before removing it.
        /// </remarks>
        public List<int> RemovedSerials { get; private set; }

        private DisasmProject mProject;
        private Formatter mFormatter;
        private VisualizationSet mOrigSet;
        private int mOffset;

        /// <summary>
        /// ItemsSource for visualizationGrid.
        /// </summary>
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

        //
        // Every action causes a selection change, so we don't explicitly call an "update
        // controls" function.
        //

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

            RemovedSerials = new List<int>();

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
            Dictionary<string, IPlugin> plugins = project.GetActivePlugins();
            foreach (IPlugin chkPlug in plugins.Values) {
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
            VisualizationSet newSet;
            if (VisualizationList.Count == 0) {
                newSet = null;
            } else {
                newSet = MakeVisSet();
            }
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

        private void NewVisualizationButton_Click(object sender, RoutedEventArgs e) {
            EditVisualization dlg = new EditVisualization(this, mProject, mFormatter, mOffset,
                CreateEditedSetList(), null);
            if (dlg.ShowDialog() != true) {
                return;
            }
            VisualizationList.Add(dlg.NewVis);
            visualizationGrid.SelectedIndex = VisualizationList.Count - 1;

            okButton.Focus();
        }

        private void NewWireframeAnimationButton_Click(object sender, RoutedEventArgs e) {
            // TODO(xyzzy)
        }

        private void NewBitmapAnimationButton_Click(object sender, RoutedEventArgs e) {
            EditBitmapAnimation dlg = new EditBitmapAnimation(this, mOffset,
                CreateEditedSetList(), null);
            if (dlg.ShowDialog() != true) {
                return;
            }
            VisualizationList.Add(dlg.NewAnim);
            visualizationGrid.SelectedIndex = VisualizationList.Count - 1;

            okButton.Focus();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            EditSelectedItem();
        }

        private void EditSelectedItem() {
            if (!IsEditEnabled) {
                // can get called here by a double-click
                return;
            }
            Visualization item = (Visualization)visualizationGrid.SelectedItem;
            Visualization newVis;

            if (item is VisBitmapAnimation) {
                EditBitmapAnimation dlg = new EditBitmapAnimation(this, mOffset,
                    CreateEditedSetList(), (VisBitmapAnimation)item);
                if (dlg.ShowDialog() != true) {
                    return;
                }
                newVis = dlg.NewAnim;
            } else {
                EditVisualization dlg = new EditVisualization(this, mProject, mFormatter, mOffset,
                    CreateEditedSetList(), item);
                if (dlg.ShowDialog() != true) {
                    return;
                }
                newVis = dlg.NewVis;
            }

            int index = VisualizationList.IndexOf(item);
            VisualizationList.Remove(item);
            VisualizationList.Insert(index, newVis);
            visualizationGrid.SelectedIndex = index;

            okButton.Focus();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e) {
            Visualization item = (Visualization)visualizationGrid.SelectedItem;
            int index = visualizationGrid.SelectedIndex;
            VisualizationList.RemoveAt(index);

            // Keep selection at same index, unless we just removed the item at the end.
            if (index == VisualizationList.Count) {
                index--;
            }
            if (index >= 0) {
                visualizationGrid.SelectedIndex = index;
            }

            RemovedSerials.Add(item.SerialNumber);

            // Update any animations in this set.  Animations in other sets will be updated later.
            // (This is a bit awkward because we can't modify VisualizationList while iterating
            // through it, and there's no simple "replace entry" operation on an observable
            // collection.  Fortunately we don't do this often and the data sets are small.)
            List<VisBitmapAnimation> needsUpdate = new List<VisBitmapAnimation>();
            foreach (Visualization vis in VisualizationList) {
                if (vis is VisBitmapAnimation) {
                    VisBitmapAnimation visAnim = (VisBitmapAnimation)vis;
                    if (visAnim.ContainsSerial(item.SerialNumber)) {
                        needsUpdate.Add(visAnim);
                    }
                }
            }
            foreach (VisBitmapAnimation visAnim in needsUpdate) {
                VisBitmapAnimation newAnim;
                if (VisBitmapAnimation.StripEntries(visAnim,
                        new List<int>(1) { item.SerialNumber }, out newAnim)) {
                    if (newAnim.Count == 0) {
                        VisualizationList.Remove(visAnim);
                    } else {
                        newAnim.GenerateImage(CreateEditedSetList());
                        index = VisualizationList.IndexOf(visAnim);
                        VisualizationList.Remove(visAnim);
                        VisualizationList.Insert(index, newAnim);
                    }
                }
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e) {
            Visualization item = (Visualization)visualizationGrid.SelectedItem;
            int index = visualizationGrid.SelectedIndex;
            Debug.Assert(index > 0);
            VisualizationList.RemoveAt(index);
            VisualizationList.Insert(index - 1, item);
            visualizationGrid.SelectedIndex = index - 1;
            visualizationGrid.ScrollIntoView(item);
        }

        private void DownButton_Click(object sender, RoutedEventArgs e) {
            Visualization item = (Visualization)visualizationGrid.SelectedItem;
            int index = visualizationGrid.SelectedIndex;
            Debug.Assert(index >= 0 && index < VisualizationList.Count - 1);
            VisualizationList.RemoveAt(index);
            VisualizationList.Insert(index + 1, item);
            visualizationGrid.SelectedIndex = index + 1;
            visualizationGrid.ScrollIntoView(item);
        }

        /// <summary>
        /// Creates a VisualizationSet from the current list of Visualizations.
        /// </summary>
        /// <returns>New VisualizationSet.</returns>
        private VisualizationSet MakeVisSet() {
            VisualizationSet newSet = new VisualizationSet(VisualizationList.Count);
            foreach (Visualization vis in VisualizationList) {
                newSet.Add(vis);
            }
            return newSet;
        }

        /// <summary>
        /// Generates a list of VisualizationSet references.  This is the list from the
        /// DisasmProject, but with the set we're editing added or substituted.
        /// </summary>
        /// <remarks>
        /// The editors sometimes need access to the full collection of Visualization objects,
        /// such as when testing a tag for uniqueness or getting a list of all bitmap
        /// frames for an animation.  The editor needs access to recent edits that have not
        /// been pushed to the project yet.
        /// </remarks>
        /// <returns>List of VisualizationSet.</returns>
        private SortedList<int, VisualizationSet> CreateEditedSetList() {
            SortedList<int, VisualizationSet> mixList =
                new SortedList<int, VisualizationSet>(mProject.VisualizationSets.Count);

            mixList[mOffset] = MakeVisSet();
            foreach (KeyValuePair<int, VisualizationSet> kvp in mProject.VisualizationSets) {
                // Skip the entry for mOffset (if it exists).
                if (kvp.Key != mOffset) {
                    mixList[kvp.Key] = kvp.Value;
                }
            }
            return mixList;
        }

        /// <summary>
        /// Finds a Visualization with a matching tag, searching across all sets in the
        /// edited list.
        /// </summary>
        /// <param name="tag">Tag to search for.</param>
        /// <returns>Matching Visualization, or null if not found.</returns>
        public static Visualization FindVisualizationByTag(SortedList<int, VisualizationSet> list,
                string tag) {
            foreach (KeyValuePair<int, VisualizationSet> kvp in list) {
                foreach (Visualization vis in kvp.Value) {
                    if (vis.Tag == tag) {
                        return vis;
                    }
                }
            }
            return null;
        }
    }
}
