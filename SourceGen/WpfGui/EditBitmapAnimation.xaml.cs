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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Bitmap animation visualization editor.
    /// </summary>
    public partial class EditBitmapAnimation : Window, INotifyPropertyChanged {
        private const int MAX_FRAME_DELAY = 10000;      // 10 sec, in ms
        private const int DEFAULT_FRAME_DELAY = 100;    // 0.1 sec, in ms

        /// <summary>
        /// New/edited animation, only valid when dialog result is true.
        /// </summary>
        public VisBitmapAnimation NewAnim { get; private set; }

        private int mSetOffset;
        private SortedList<int, VisualizationSet> mEditedList;
        private VisBitmapAnimation mOrigAnim;

        private Brush mDefaultLabelColor = SystemColors.WindowTextBrush;
        private Brush mErrorLabelColor = Brushes.Red;

        /// <summary>
        /// True if current contents represent a valid visualization animation.  Determines
        /// whether the OK button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        public ObservableCollection<Visualization> VisSourceItems { get; private set; } =
            new ObservableCollection<Visualization>();

        public ObservableCollection<Visualization> VisAnimItems { get; private set; } =
            new ObservableCollection<Visualization>();

        /// <summary>
        /// Visualization tag.
        /// </summary>
        public string TagString {
            get { return mTagString; }
            set { mTagString = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mTagString;

        // Text turns red on error.
        public Brush TagLabelBrush {
            get { return mTagLabelBrush; }
            set { mTagLabelBrush = value; OnPropertyChanged(); }
        }
        private Brush mTagLabelBrush;

        /// <summary>
        /// Time between frames, in milliseconds.
        /// </summary>
        public string FrameDelayTimeMsec {
            get { return mFrameDelayTimeMsec; }
            set { mFrameDelayTimeMsec = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mFrameDelayTimeMsec;
        private int mFrameDelayIntMsec = -1;

        public Brush FrameDelayLabelBrush {
            get { return mFrameDelayLabelBrush; }
            set { mFrameDelayLabelBrush = value; OnPropertyChanged(); }
        }
        private Brush mFrameDelayLabelBrush;

        public bool IsAddEnabled {
            get { return mIsAddEnabled; }
            set { mIsAddEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsAddEnabled;

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

        public bool IsPreviewEnabled {
            get { return mIsPreviewEnabled; }
            set { mIsPreviewEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsPreviewEnabled;


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public EditBitmapAnimation(Window owner, int setOffset,
                SortedList<int, VisualizationSet> editedList, VisBitmapAnimation origAnim) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mSetOffset = setOffset;
            mEditedList = editedList;
            mOrigAnim = origAnim;

            // this will cause initialization of the Brush properties
            FrameDelayTimeMsec = DEFAULT_FRAME_DELAY.ToString();
            if (origAnim != null) {
                TagString = origAnim.Tag;
                mFrameDelayIntMsec = PluginCommon.Util.GetFromObjDict(origAnim.VisGenParams,
                    VisBitmapAnimation.P_FRAME_DELAY_MSEC_PARAM, DEFAULT_FRAME_DELAY);
                if (mFrameDelayIntMsec == DEFAULT_FRAME_DELAY) {
                    // check for old-style
                    mFrameDelayIntMsec = PluginCommon.Util.GetFromObjDict(origAnim.VisGenParams,
                        VisBitmapAnimation.P_FRAME_DELAY_MSEC_PARAM_OLD, DEFAULT_FRAME_DELAY);
                }
                FrameDelayTimeMsec = mFrameDelayIntMsec.ToString();
            } else {
                TagString = "anim" + mSetOffset.ToString("x6");
            }

            PopulateItemLists();
        }

        private void PopulateItemLists() {
            // Add the animation's visualizations, in order.
            if (mOrigAnim != null) {
                for (int i = 0; i < mOrigAnim.Count; i++) {
                    int serial = mOrigAnim[i];
                    Visualization vis = VisualizationSet.FindVisualizationBySerial(mEditedList,
                        serial);
                    if (vis != null) {
                        VisAnimItems.Add(vis);
                    } else {
                        // Could happen if the Visualization exists but isn't referenced by
                        // any VisualizationSets.  Shouldn't happen unless the project file
                        // was damaged.  Silently ignore it.
                        Debug.WriteLine("WARNING: unknown vis serial " + serial);
                    }
                }
            }

            // Add all remaining non-animation Visualizations to the "source" set.
            foreach (KeyValuePair<int, VisualizationSet> kvp in mEditedList) {
                foreach (Visualization vis in kvp.Value) {
                    if (vis is VisBitmapAnimation) {
                        // disallow using animations as animation frames
                        continue;
                    }
                    VisSourceItems.Add(vis);
                }
            }

            if (VisSourceItems.Count > 0) {
                visSourceGrid.SelectedIndex = 0;
            }
            if (VisAnimItems.Count > 0) {
                visAnimGrid.SelectedIndex = 0;
            }
        }

        // Want to focus on the first item, not the grid.  Probably need a hack like
        // MainWindow's ItemContainerGenerator_StatusChanged.  Not really worth it here.
        //private void Window_ContentRendered(object sender, EventArgs e) {
        //    visSourceGrid.Focus();
        //    DataGridRow dgr =
        //        (DataGridRow)visSourceGrid.ItemContainerGenerator.ContainerFromIndex(0);
        //    dgr.Focus();
        //}

        private void Window_Closing(object sender, CancelEventArgs e) {
            previewAnim.Stop();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            Dictionary<string, object> visGenParams = new Dictionary<string, object>(1);
            visGenParams.Add(VisBitmapAnimation.P_FRAME_DELAY_MSEC_PARAM, mFrameDelayIntMsec);

            List<int> serials = new List<int>(VisAnimItems.Count);
            foreach (Visualization vis in VisAnimItems) {
                serials.Add(vis.SerialNumber);
            }

            NewAnim = new VisBitmapAnimation(TagString, VisBitmapAnimation.ANIM_VIS_GEN,
                new ReadOnlyDictionary<string, object>(visGenParams), mOrigAnim, serials);
            NewAnim.GenerateImage(mEditedList);

            DialogResult = true;
        }

        private void UpdateControls() {
            IsValid = true;

            if (!int.TryParse(FrameDelayTimeMsec, out int frameDelay) ||
                    frameDelay <= 0 || frameDelay > MAX_FRAME_DELAY) {
                mFrameDelayIntMsec = -1;
                FrameDelayLabelBrush = mErrorLabelColor;
                IsValid = false;
            } else {
                mFrameDelayIntMsec = frameDelay;
                FrameDelayLabelBrush = mDefaultLabelColor;
            }

            bool isSourceItemSelected = (visSourceGrid.SelectedItem != null);
            bool isAnimItemSelected = (visAnimGrid.SelectedItem != null);
            IsAddEnabled = VisSourceItems.Count > 0 && isSourceItemSelected;
            IsRemoveEnabled = VisAnimItems.Count > 0 && isAnimItemSelected;
            IsUpEnabled = isAnimItemSelected && visAnimGrid.SelectedIndex != 0;
            IsDownEnabled = isAnimItemSelected &&
                visAnimGrid.SelectedIndex != VisAnimItems.Count - 1;
            IsPreviewEnabled = VisAnimItems.Count > 0;

            IsValid &= IsPreviewEnabled;    // don't allow animations with no frames

            string trimTag = Visualization.TrimAndValidateTag(TagString, out bool tagOk);
            Visualization match =
                EditVisualizationSet.FindVisualizationByTag(mEditedList, trimTag);
            if (match != null && (mOrigAnim == null || trimTag != mOrigAnim.Tag)) {
                // Another vis already has this tag.  We're checking the edited list, so we'll
                // be current with edits to this or other Visualizations in the same set.
                tagOk = false;
            }
            if (!tagOk) {
                TagLabelBrush = mErrorLabelColor;
                IsValid = false;
            } else {
                TagLabelBrush = mDefaultLabelColor;
            }

            RefreshAnim();
        }

        private void VisSourceGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateControls();
        }

        private void VisAnimGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateControls();
        }

        private void VisSourceGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            AddSelection();
        }

        private void VisAnimGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            RemoveSelection();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            AddSelection();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e) {
            RemoveSelection();
        }

        /// <summary>
        /// Adds an item to the end of the animation list.
        /// </summary>
        /// <remarks>
        /// We could make this an insert or add-at-cursor operation.  This feels a bit more
        /// natural, and works since we're still limited to single-select on the anim list.
        /// The selection should be set to the last item added so we can add repeatedly.
        /// </remarks>
        private void AddSelection() {
            if (!IsAddEnabled) {
                return;
            }

            for (int i = 0; i < visSourceGrid.SelectedItems.Count; i++) {
                Visualization item = (Visualization)visSourceGrid.SelectedItems[i];
                VisAnimItems.Add(item);
            }
            if (visAnimGrid.SelectedIndex < 0) {
                visAnimGrid.SelectedIndex = 0;
            }

            RefreshAnim();
        }

        /// <summary>
        /// Removes an item from the animation list.
        /// </summary>
        private void RemoveSelection() {
            if (!IsRemoveEnabled) {
                return;
            }

            int index = visAnimGrid.SelectedIndex;
            Debug.Assert(index >= 0);
            VisAnimItems.RemoveAt(index);

            if (index == VisAnimItems.Count) {
                index--;
            }
            if (index >= 0) {
                visAnimGrid.SelectedIndex = index;
            }

            RefreshAnim();
        }

        /// <summary>
        /// Clears the animation list.
        /// </summary>
        private void ClearButton_Click(object sender, RoutedEventArgs e) {
            Debug.Assert(IsRemoveEnabled);

            VisAnimItems.Clear();

            RefreshAnim();
        }

        /// <summary>
        /// Repositions an item in the animation list, moving it up one slot.
        /// </summary>
        private void UpButton_Click(object sender, RoutedEventArgs e) {
            Visualization item = (Visualization)visAnimGrid.SelectedItem;
            int index = VisAnimItems.IndexOf(item);
            Debug.Assert(index > 0);
            VisAnimItems.Remove(item);
            VisAnimItems.Insert(index - 1, item);
            visAnimGrid.SelectedIndex = index - 1;
            visAnimGrid.ScrollIntoView(item);

            //RefreshAnim();
        }

        /// <summary>
        /// Repositions an item in the animation list, moving it down one slot.
        /// </summary>
        private void DownButton_Click(object sender, RoutedEventArgs e) {
            Visualization item = (Visualization)visAnimGrid.SelectedItem;
            int index = VisAnimItems.IndexOf(item);
            Debug.Assert(index >= 0 && index < VisAnimItems.Count - 1);
            VisAnimItems.Remove(item);
            VisAnimItems.Insert(index + 1, item);
            visAnimGrid.SelectedIndex = index + 1;
            visAnimGrid.ScrollIntoView(item);

            //RefreshAnim();
        }

        private void showPreviewClick(object sender, RoutedEventArgs e) {
            if (previewAnim.IsRunning) {
                previewAnim.Stop();
            } else {
                if (RefreshAnim()) {
                    previewAnim.Start();
                }
            }
        }

        /// <summary>
        /// Updates the frame animation control's parameters.  Stops the animation if something
        /// looks wrong.
        /// </summary>
        /// <returns>True if all is well, false if something is wrong and the animation
        ///   should not be started.</returns>
        private bool RefreshAnim() {
            if (VisAnimItems.Count == 0 || mFrameDelayIntMsec <= 0) {
                previewAnim.Stop();
                return false;
            }

            List<BitmapSource> bitmaps = new List<BitmapSource>(VisAnimItems.Count);
            foreach (Visualization vis in VisAnimItems) {
                bitmaps.Add(vis.CachedImage);
            }
            previewAnim.Bitmaps = bitmaps;
            previewAnim.IntervalMsec = mFrameDelayIntMsec;
            return true;
        }
    }
}
