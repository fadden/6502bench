/*
 * Copyright 2020 faddenSoft
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
using System.Diagnostics;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using PluginCommon;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Test window for wireframe animations.
    /// </summary>
    public partial class ShowWireframeAnimation : Window {
        /// <summary>
        /// Dispatcher-linked timer object.
        /// </summary>
        private DispatcherTimer mTimer;

        IVisualizationWireframe mVisWire;
        private int mFrameCount;
        private int mInitialX, mInitialY, mInitialZ;
        private int mDeltaX, mDeltaY, mDeltaZ;
        private bool mDoPersp, mDoBfc;

        private int mCurX, mCurY, mCurZ;

        private int mCurFrame;

        public ShowWireframeAnimation(Window owner, IVisualizationWireframe visWire,
                ReadOnlyDictionary<string, object> parms) {
            InitializeComponent();
            Owner = owner;

            mVisWire = visWire;

            mCurX = mInitialX = Util.GetFromObjDict(parms, VisWireframeAnimation.P_EULER_ROT_X, 0);
            mCurY = mInitialY = Util.GetFromObjDict(parms, VisWireframeAnimation.P_EULER_ROT_Y, 0);
            mCurZ = mInitialZ = Util.GetFromObjDict(parms, VisWireframeAnimation.P_EULER_ROT_Z, 0);
            mDeltaX = Util.GetFromObjDict(parms, VisWireframeAnimation.P_DELTA_ROT_X, 0);
            mDeltaY = Util.GetFromObjDict(parms, VisWireframeAnimation.P_DELTA_ROT_Y, 0);
            mDeltaZ = Util.GetFromObjDict(parms, VisWireframeAnimation.P_DELTA_ROT_Z, 0);
            mFrameCount = Util.GetFromObjDict(parms, VisWireframeAnimation.P_FRAME_COUNT, 1);
            mDoPersp = Util.GetFromObjDict(parms, VisWireframe.P_IS_PERSPECTIVE, true);
            mDoBfc = Util.GetFromObjDict(parms, VisWireframe.P_IS_BFC_ENABLED, false);

            int intervalMsec = Util.GetFromObjDict(parms,
                VisWireframeAnimation.P_FRAME_DELAY_MSEC, 100);

            mCurFrame = 0;

            mTimer = new DispatcherTimer(DispatcherPriority.Render);
            mTimer.Interval = TimeSpan.FromMilliseconds(intervalMsec);
            mTimer.Tick += Tick;
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            //Debug.WriteLine("INITIAL: " + testViewBox.ActualWidth + "x" +
            //    testViewBox.ActualHeight);
            Tick(null, null);   // show something immediately
            mTimer.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            mTimer.Stop();
        }

        private void Tick(object sender, EventArgs e) {
            mCurFrame++;
            if (mCurFrame == mFrameCount) {
                // reset
                mCurX = mInitialX;
                mCurY = mInitialY;
                mCurZ = mInitialZ;
                mCurFrame = 0;
            } else {
                mCurX = (mCurX + 360 + mDeltaX) % 360;
                mCurY = (mCurY + 360 + mDeltaY) % 360;
                mCurZ = (mCurZ + 360 + mDeltaZ) % 360;
            }

            // We use the dimensions of the Border surrounding the ViewBox, rather than the
            // ViewBox itself, because on the first iteration the ViewBox has a size of zero.
            double dim = Math.Floor(Math.Min(testBorder.ActualWidth, testBorder.ActualHeight));
            wireframePath.Data = Visualization.GenerateWireframePath(mVisWire, dim,
                mCurX, mCurY, mCurZ, mDoPersp, mDoBfc);
        }
    }
}
