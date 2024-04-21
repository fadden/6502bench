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
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Select parameters for label file generation.
    /// </summary>
    public partial class GenerateLabels : Window, INotifyPropertyChanged {
        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IncludeAutoLabels {
            get { return mIncludeAutoLabels; }
            set { mIncludeAutoLabels = value; OnPropertyChanged(); }
        }
        private bool mIncludeAutoLabels;

        public LabelFileGenerator.LabelFmt Format { get; private set; }

        public bool Format_VICE {
            get { return Format == LabelFileGenerator.LabelFmt.VICE;}
            set { Format = LabelFileGenerator.LabelFmt.VICE; UpdateFormats(); }
        }

        private void UpdateFormats() {
            OnPropertyChanged(nameof(Format_VICE));
        }


        public GenerateLabels(Window owner) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            Format = AppSettings.Global.GetEnum(AppSettings.LABGEN_FORMAT,
                LabelFileGenerator.LabelFmt.VICE);
            UpdateFormats();
            mIncludeAutoLabels = AppSettings.Global.GetBool(AppSettings.LABGEN_INCLUDE_AUTO, false);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            // Save settings.
            AppSettings.Global.SetEnum(AppSettings.LABGEN_FORMAT, Format);
            AppSettings.Global.SetBool(AppSettings.LABGEN_INCLUDE_AUTO, mIncludeAutoLabels);
            DialogResult = true;
        }
    }
}
