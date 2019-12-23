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
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CommonWPF {
    /// <summary>
    /// The Frame Animation control provides a simple way to display a series of images.
    /// (Think of an animated GIF.)
    ///
    /// Set Bitmaps and IntervalMsec, then call Start.
    /// </summary>
    public partial class FrameAnimationControl : UserControl {
        /// <summary>
        /// List of bitmaps to be displayed.
        /// </summary>
        public List<BitmapSource> Bitmaps {
            get { return mBitmaps; }
            set {
                if (value == null || value.Count == 0) {
                    throw new ArgumentException("Invalid bitmap list");
                }
                mBitmaps = value;
                if (mNext >= value.Count) {
                    mNext = 0;
                }
            }
        }
        private List<BitmapSource> mBitmaps;

        /// <summary>
        /// How long to wait before showing next bitmap.
        /// </summary>
        public int IntervalMsec {
            get { return mIntervalMsec; }
            set {
                if (value < 1) {
                    throw new ArgumentException("Invalid interval " + value);
                }
                mIntervalMsec = value;
                mTimer.Interval = TimeSpan.FromMilliseconds(value);
            }
        }
        private int mIntervalMsec = 100;

        /// <summary>
        /// True if the animation is currently running.
        /// </summary>
        public bool IsRunning {
            get { return mTimer.IsEnabled; }
        }

        /// <summary>
        /// Index of next image to display.
        /// </summary>
        private int mNext;

        /// <summary>
        /// Dispatcher-linked timer object.
        /// </summary>
        private DispatcherTimer mTimer;


        /// <summary>
        /// Constructor, invoked from XAML.
        /// </summary>
        public FrameAnimationControl() {
            InitializeComponent();

            mTimer = new DispatcherTimer(DispatcherPriority.Render);
            mTimer.Interval = TimeSpan.FromMilliseconds(IntervalMsec);
            mTimer.Tick += Tick;
        }

        public void Start() {
            if (mBitmaps == null) {
                throw new InvalidOperationException("Must set bitmaps before starting");
            }
            Tick(null, null);   // show something immediately
            mTimer.Start();
        }

        public void Stop() {
            mTimer.Stop();
        }

        private void Tick(object sender, EventArgs e) {
            if (mNext >= mBitmaps.Count) {
                mNext = 0;
            }
            theImage.Source = mBitmaps[mNext];
            mNext++;
        }
    }
}
