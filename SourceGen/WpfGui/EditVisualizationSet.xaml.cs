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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Visualization set editor.
    /// </summary>
    public partial class EditVisualizationSet : Window, INotifyPropertyChanged {
        public VisualizationSet NewVisSet { get; private set; }

        public string PlaceHolder {
            get { return mPlaceHolder; }
            set { mPlaceHolder = value; OnPropertyChanged(); }
        }
        private string mPlaceHolder;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditVisualizationSet(Window owner, VisualizationSet curSet) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            if (curSet != null) {
                PlaceHolder = curSet.PlaceHolder;
            } else {
                PlaceHolder = "New!";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {

        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(PlaceHolder)) {
                NewVisSet = null;
            } else {
                NewVisSet = new VisualizationSet(PlaceHolder);
            }
            DialogResult = true;
        }
    }
}
