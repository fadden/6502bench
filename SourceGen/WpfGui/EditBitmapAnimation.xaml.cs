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
        public VisualizationAnimation NewAnim { get; private set; }

        private int mSetOffset;
        private SortedList<int, VisualizationSet> mEditedList;
        private VisualizationAnimation mOrigAnim;

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
                SortedList<int, VisualizationSet> editedList, VisualizationAnimation origAnim) {
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
                int frameDelay = PluginCommon.Util.GetFromObjDict(origAnim.VisGenParams,
                    VisualizationAnimation.FRAME_DELAY_MSEC_PARAM, DEFAULT_FRAME_DELAY);
            } else {
                TagString = "anim" + mSetOffset.ToString("x6");
            }

            PopulateItemLists();
        }

        private void PopulateItemLists() {
            // Add the animation's visualizations, in order.
            if (mOrigAnim != null) {
                foreach (int serial in mOrigAnim.GetSerialNumbers()) {
                    Visualization vis = VisualizationSet.FindVisualizationBySerial(mEditedList,
                        serial);
                    if (vis != null) {
                        VisAnimItems.Add(vis);
                    } else {
                        Debug.Assert(false);
                    }
                }
            }

            // Add all remaining non-animation Visualizations to the "source" set.
            foreach (KeyValuePair<int, VisualizationSet> kvp in mEditedList) {
                foreach (Visualization vis in kvp.Value) {
                    if (vis is VisualizationAnimation) {
                        // disallow using animations as animation frames
                        continue;
                    }
                    if (!VisAnimItems.Contains(vis)) {
                        VisSourceItems.Add(vis);
                    }
                }
            }

            if (VisSourceItems.Count > 0) {
                visSourceGrid.SelectedIndex = 0;
            }
            if (VisAnimItems.Count > 0) {
                visAnimGrid.SelectedIndex = 0;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            previewAnim.Stop();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            Dictionary<string, object> visGenParams = new Dictionary<string, object>(1);
            visGenParams.Add(VisualizationAnimation.FRAME_DELAY_MSEC_PARAM, mFrameDelayIntMsec);

            List<int> serials = new List<int>(VisAnimItems.Count);
            foreach (Visualization vis in VisAnimItems) {
                serials.Add(vis.SerialNumber);
            }

            NewAnim = new VisualizationAnimation(TagString, VisualizationAnimation.ANIM_VIS_GEN,
                new ReadOnlyDictionary<string, object>(visGenParams), serials, mOrigAnim);

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
        /// Adds an item to the animation list, moving it from the source list.
        /// </summary>
        private void AddSelection() {
            if (!IsAddEnabled) {
                return;
            }

            Visualization item = (Visualization)visSourceGrid.SelectedItem;
            int index = VisSourceItems.IndexOf(item);
            Debug.Assert(index >= 0);
            VisSourceItems.Remove(item);
            VisAnimItems.Add(item);

            if (index == VisSourceItems.Count) {
                index--;
            }
            if (index >= 0) {
                visSourceGrid.SelectedIndex = index;
            }

            if (visAnimGrid.SelectedIndex < 0) {
                visAnimGrid.SelectedIndex = 0;
            }

            RefreshAnim();
        }

        /// <summary>
        /// Removes an item from the animation list, moving it to the source list.
        /// </summary>
        private void RemoveSelection() {
            if (!IsRemoveEnabled) {
                return;
            }

            Visualization item = (Visualization)visAnimGrid.SelectedItem;
            int index = VisAnimItems.IndexOf(item);
            Debug.Assert(index >= 0);
            VisAnimItems.Remove(item);
            VisSourceItems.Add(item);

            if (index == VisAnimItems.Count) {
                index--;
            }
            if (index >= 0) {
                visAnimGrid.SelectedIndex = index;
            }

            if (visSourceGrid.SelectedIndex < 0) {
                visSourceGrid.SelectedIndex = 0;
            }

            RefreshAnim();
        }

        /// <summary>
        /// Clears the animation list.
        /// </summary>
        private void ClearButton_Click(object sender, RoutedEventArgs e) {
            Debug.Assert(IsRemoveEnabled);

            while (VisAnimItems.Count > 0) {
                Visualization item = VisAnimItems[0];
                VisAnimItems.Remove(item);
                VisSourceItems.Add(item);
            }

            if (visSourceGrid.SelectedIndex < 0) {
                visSourceGrid.SelectedIndex = 0;
            }

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
