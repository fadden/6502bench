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

namespace SourceGen.WpfGui {
    /// <summary>
    /// Edit a LocalVariableTable.
    /// </summary>
    public partial class EditLocalVariableTable : Window {
        // Item for the symbol list view.
        public class FormattedSymbol {
            public string Label { get; private set; }
            public string Value { get; private set; }
            public string Type { get; private set; }
            public string Width { get; private set; }
            public string Comment { get; private set; }

            public FormattedSymbol(string label, string value, string type, string width,
                    string comment) {
                Label = label;
                Value = value;
                Type = type;
                Width = width;
                Comment = comment;
            }
        }
        public ObservableCollection<FormattedSymbol> Variables { get; private set; } =
            new ObservableCollection<FormattedSymbol>();

        public EditLocalVariableTable(Window owner) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;
        }

        public void Window_Loaded(object sender, RoutedEventArgs e) {
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void DeleteTableButton_Click(object sender, RoutedEventArgs e) {
        }

        private void SymbolsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        }

        private void SymbolsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
        }

        private void NewSymbolButton_Click(object sender, RoutedEventArgs e) {
        }

        private void EditSymbolButton_Click(object sender, EventArgs e) {
        }

        private void RemoveSymbolButton_Click(object sender, RoutedEventArgs e) {
        }
    }
}
