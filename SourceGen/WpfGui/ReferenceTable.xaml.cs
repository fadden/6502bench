/*
 * Copyright 2025 faddenSoft
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
using System.Windows;
using System.Windows.Input;

using CommonWPF;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Side window with references to the project.  This could be copied out of the References
    /// panel or generated from a "find all" command.
    /// </summary>
    public partial class ReferenceTable : Window {
        public class ReferenceTableItem {
            public int OffsetValue { get; private set; }
            public string Offset { get; private set; }
            public string Addr { get; private set; }
            public string Type { get; private set; }

            public ReferenceTableItem(int offsetValue, string offset, string addr, string type) {
                OffsetValue = offsetValue;
                Offset = offset;
                Addr = addr;
                Type = type;
            }

            public override string ToString() {
                return "[ReferenceTableItem: off=" + Offset + " addr=" + Addr + " type=" +
                    Type + "]";
            }
        }

        public ObservableCollection<ReferenceTableItem> ReferencesList { get; private set; } =
            new ObservableCollection<ReferenceTableItem>();

        private MainController mMainCtrl;

        public ReferenceTable(Window owner, MainController mainCtrl) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mMainCtrl = mainCtrl;
        }

        /// <summary>
        /// Replaces the existing list of items with a new list.
        /// </summary>
        public void SetItems(List<ReferenceTableItem> items) {
            ReferencesList.Clear();
            foreach (ReferenceTableItem item in items) {
                ReferencesList.Add(item);
            }
        }

        // Catch ESC key.
        private void Window_KeyEventHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                Close();
                e.Handled = true;
            }
        }

        private void ReferencesList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (!tableDataGrid.GetClickRowColItem(e, out int unusedRow, out int unusedCol,
                    out object item)) {
                // Header or empty area; ignore.
                return;
            }
            ReferenceTableItem rli = (ReferenceTableItem)item;

            // Jump to the offset, then shift the focus back to the code list.
            mMainCtrl.GoToLocation(new NavStack.Location(rli.OffsetValue, 0,
                NavStack.GoToMode.JumpToCodeData), true);
        }
    }
}
