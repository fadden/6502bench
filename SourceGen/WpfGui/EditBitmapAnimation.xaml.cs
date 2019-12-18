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
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Bitmap animation visualization editor.
    /// </summary>
    public partial class EditBitmapAnimation : Window, INotifyPropertyChanged {
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
        /// Time between frames, in milliseconds.
        /// </summary>
        public int FrameDelayTimeMsec {
            get { return mFrameDelayTimeMsec; }
            set { mFrameDelayTimeMsec = value; OnPropertyChanged(); }
        }
        private int mFrameDelayTimeMsec;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner"></param>
        public EditBitmapAnimation(Window owner) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {

        }

        private void VisSourceGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }

        private void VisAnimGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }
    }
}
