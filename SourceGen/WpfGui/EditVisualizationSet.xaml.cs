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

using Asm65;
using PluginCommon;
using SourceGen.Sandbox;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Visualization set editor.
    /// </summary>
    public partial class EditVisualizationSet : Window, INotifyPropertyChanged {
        public VisualizationSet NewVisSet { get; private set; }

        private DisasmProject mProject;
        private Formatter mFormatter;

        public ObservableCollection<Visualization> VisualizationList { get; private set; } =
            new ObservableCollection<Visualization>();

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditVisualizationSet(Window owner, DisasmProject project, Formatter formatter,
                VisualizationSet curSet) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = project;
            mFormatter = formatter;

            if (curSet != null) {
                foreach (Visualization vis in curSet) {
                    VisualizationList.Add(vis);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (VisualizationList.Count == 0) {
                NewVisSet = null;
            } else {
                NewVisSet = new VisualizationSet(VisualizationList.Count);
                foreach (Visualization vis in VisualizationList) {
                    NewVisSet.Add(vis);
                }
            }
            DialogResult = true;
        }

        private void VisualizationList_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            Debug.WriteLine("SEL CHANGE");
        }

        private void VisualizationList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Debug.WriteLine("DBL CLICK");
        }

        private void NewButton_Click(object sender, RoutedEventArgs e) {
            VisualizationList.Add(new Visualization("VIS #" + VisualizationList.Count,
                "apple2-hi-res-bitmap", new Dictionary<string, object>()));

            // TODO: disable New button if no appropriate vis plugins can be found (or maybe
            //   MessageBox here)
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            Dictionary<string, object> testDict = new Dictionary<string, object>();
            testDict.Add("offset", 0);
            testDict.Add("byteWidth", 2);
            testDict.Add("height", 7);
            EditVisualization dlg = new EditVisualization(this, mProject, mFormatter,
                new Visualization("arbitrary tag", "apple2-hi-res-bitmap", testDict));
            if (dlg.ShowDialog() == true) {
                Visualization newVis = dlg.NewVis;
                Debug.WriteLine("New vis: " + newVis);
            }

            // TODO: disable edit button if matching vis can't be found (or maybe MessageBox)
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e) {

        }

        private void UpButton_Click(object sender, RoutedEventArgs e) {

        }

        private void DownButton_Click(object sender, RoutedEventArgs e) {

        }
    }
}
