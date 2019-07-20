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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MakeDistWPF {
    /// <summary>
    /// Distribution maker.
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public bool IsReleaseChecked {
            get { return mIsReleaseChecked; }
            set { mIsReleaseChecked = value; OnPropertyChanged(); }
        }
        private bool mIsReleaseChecked;
        public bool IsDebugChecked {
            get { return mIsDebugChecked; }
            set { mIsDebugChecked = value; OnPropertyChanged(); }
        }
        private bool mIsDebugChecked;
        public bool DoIncludeRegressionTests {
            get { return mDoIncludeRegressionTests; }
            set { mDoIncludeRegressionTests = value; OnPropertyChanged(); }
        }
        private bool mDoIncludeRegressionTests;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindow() {
            InitializeComponent();
            DataContext = this;

            IsReleaseChecked = true;
        }

        private void BuildButton_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("rel=" + IsReleaseChecked + " dbg=" + IsDebugChecked +
                " incl=" + DoIncludeRegressionTests);

            FileCopier.BuildType buildType;
            if (IsReleaseChecked) {
                buildType = FileCopier.BuildType.Release;
            } else {
                buildType = FileCopier.BuildType.Debug;
            }
            bool copyTestFiles = DoIncludeRegressionTests;

            CopyProgress dlg = new CopyProgress(this, buildType, copyTestFiles);
            dlg.ShowDialog();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
