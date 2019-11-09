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
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Asm65;
using CommonWPF;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Edit a LocalVariableTable.
    /// </summary>
    public partial class EditLocalVariableTable : Window, INotifyPropertyChanged {
        /// <summary>
        /// Output.  Will be null if the table was deleted, or if cancel was hit while
        /// creating a new table.
        /// </summary>
        public LocalVariableTable NewTable { get; private set; }

        /// <summary>
        /// Output.  If the table was moved, the new offset will be different from the old.
        /// </summary>
        public int NewOffset { get; private set; }

        // Item for the symbol list view ItemsSource.
        public class FormattedSymbol {
            public string Label { get; private set; }
            public string Value { get; private set; }
            public string Type { get; private set; }
            public int Width { get; private set; }
            public string Comment { get; private set; }

            public DefSymbol DefSym;

            public FormattedSymbol(DefSymbol defSym, string label, string value,
                    string type, int width, string comment) {
                Label = label;
                Value = value;
                Type = type;
                Width = width;
                Comment = comment;

                DefSym = defSym;
            }
        }

        // List of items.  The Label is guaranteed to be unique.
        public ObservableCollection<FormattedSymbol> Variables { get; private set; } =
            new ObservableCollection<FormattedSymbol>();

        /// <summary>
        /// Clear-previous flag.
        /// </summary>
        public bool ClearPrevious {
            get { return mWorkTable.ClearPrevious; }
            set { mWorkTable.ClearPrevious = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// True if this is not a new table.  (Using "not" because that's the sense we
        /// need in XAML.)
        /// </summary>
        public bool IsNotNewTable {
            get { return mIsNotNewTable; }
            set { mIsNotNewTable = value; OnPropertyChanged(); }
        }
        private bool mIsNotNewTable;

        /// <summary>
        /// Table header text string, formatted at load time.
        /// </summary>
        public string TableHeaderText {
            get { return mTableHeaderText; }
            set { mTableHeaderText = value; OnPropertyChanged(); }
        }
        private string mTableHeaderText;

        /// <summary>
        /// Working set.  Used internally to hold state.
        /// </summary>
        private LocalVariableTable mWorkTable;

        /// <summary>
        /// Project reference.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Format object to use when formatting addresses and constants.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// Symbol table for uniqueness check.
        /// </summary>
        private SymbolTable mSymbolTable;

        /// <summary>
        /// Table offset, for move ops.
        /// </summary>
        private int mOffset;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Constructor.  lvt will be null when creating a new entry.
        /// </summary>
        public EditLocalVariableTable(Window owner, DisasmProject project, Formatter formatter,
                LocalVariableTable lvt, int offset) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = project;
            mFormatter = formatter;
            mSymbolTable = project.SymbolTable;
            mOffset = NewOffset = offset;

            if (lvt != null) {
                mWorkTable = new LocalVariableTable(lvt);
                mIsNotNewTable = true;
            } else {
                mWorkTable = new LocalVariableTable();
            }

            for (int i = 0; i < mWorkTable.Count; i++) {
                DefSymbol defSym = mWorkTable[i];
                Variables.Add(CreateFormattedSymbol(defSym));
            }
        }

        public void Window_Loaded(object sender, RoutedEventArgs e) {
            string fmt = (string)FindResource("str_TableHeaderFmt");
            TableHeaderText = string.Format(fmt, mFormatter.FormatOffset24(mOffset));

            UpdateControls();
        }

        /// <summary>
        /// Loads entries from the work table into the items source.
        /// </summary>
        private FormattedSymbol CreateFormattedSymbol(DefSymbol defSym) {
            string typeStr;
            if (defSym.IsConstant) {
                typeStr = Res.Strings.ABBREV_STACK_RELATIVE;
            } else {
                typeStr = Res.Strings.ABBREV_ADDRESS;
            }

            FormattedSymbol fsym = new FormattedSymbol(
                defSym,
                defSym.AnnotatedLabel,
                mFormatter.FormatValueInBase(defSym.Value, defSym.DataDescriptor.NumBase),
                typeStr,
                defSym.DataDescriptor.Length,
                defSym.Comment);
            return fsym;
        }

        /// <summary>
        /// Handles a Sorting event.  We need to sort by numeric value on the Value field, but
        /// we want to use custom-formatted numeric values in multiple bases.
        /// </summary>
        private void SymbolsList_Sorting(object sender, DataGridSortingEventArgs e) {
            DataGridColumn col = e.Column;

            // Set the SortDirection to a specific value.  If we don't do this, SortDirection
            // remains un-set, and the column header doesn't show up/down arrows or change
            // direction when clicked twice.
            ListSortDirection direction = (col.SortDirection != ListSortDirection.Ascending) ?
                ListSortDirection.Ascending : ListSortDirection.Descending;
            col.SortDirection = direction;

            bool isAscending = direction != ListSortDirection.Descending;

            IComparer comparer = new SymbolsListComparer(col.DisplayIndex, isAscending);
            ListCollectionView lcv =
                (ListCollectionView)CollectionViewSource.GetDefaultView(symbolsList.ItemsSource);
            lcv.CustomSort = comparer;
            e.Handled = true;
        }

        private class SymbolsListComparer : IComparer {
            // Must match order of items in DataGrid.  DataGrid must not allow columns to be
            // reordered.  We could check col.Header instead, but then we have to assume that
            // the Header is a string that doesn't get renamed.
            private enum SortField {
                Label = 0, Value = 1, Type = 2, Width = 3, Comment = 4
            }
            private SortField mSortField;
            private bool mIsAscending;

            public SymbolsListComparer(int displayIndex, bool isAscending) {
                Debug.Assert(displayIndex >= 0 && displayIndex <= 4);
                mIsAscending = isAscending;
                mSortField = (SortField)displayIndex;
            }

            // IComparer interface
            public int Compare(object o1, object o2) {
                FormattedSymbol fsym1 = (FormattedSymbol)o1;
                FormattedSymbol fsym2 = (FormattedSymbol)o2;

                // Sort primarily by specified field, secondarily by label (which should
                // be unique).
                int cmp;
                switch (mSortField) {
                    case SortField.Label:
                        cmp = string.Compare(fsym1.Label, fsym2.Label);
                        break;
                    case SortField.Value:
                        cmp = fsym1.DefSym.Value - fsym2.DefSym.Value;
                        break;
                    case SortField.Type:
                        cmp = string.Compare(fsym1.Type, fsym2.Type);
                        break;
                    case SortField.Width:
                        cmp = fsym1.Width - fsym2.Width;
                        break;
                    case SortField.Comment:
                        cmp = string.Compare(fsym1.Comment, fsym2.Comment);
                        break;
                    default:
                        Debug.Assert(false);
                        return 0;
                }

                if (cmp == 0) {
                    cmp = string.Compare(fsym1.Label, fsym2.Label);
                }
                if (!mIsAscending) {
                    cmp = -cmp;
                }
                return cmp;
            }
        }

        private void UpdateControls() {
            // Enable or disable the edit/remove buttons based on how many items are selected.
            // (We're currently configured for single-select, so this is really just a != 0 test.)
            int symSelCount = symbolsList.SelectedItems.Count;
            removeSymbolButton.IsEnabled = (symSelCount == 1);
            editSymbolButton.IsEnabled = (symSelCount == 1);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            NewTable = mWorkTable;
            DialogResult = true;
        }

        private void DeleteTableButton_Click(object sender, RoutedEventArgs e) {
            MessageBoxResult result = MessageBox.Show((string)FindResource("str_ConfirmDelete"),
                (string)FindResource("str_ConfirmDeleteCaption"),
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes) {
                NewTable = null;
                DialogResult = true;
            }
        }

        private void MoveTableButton_Click(object sender, RoutedEventArgs e) {
            EditLvTableLocation dlg = new EditLvTableLocation(this, mProject, mOffset, NewOffset);
            if (dlg.ShowDialog() == true) {
                NewOffset = dlg.NewOffset;
            }
        }

        private void SymbolsList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateControls();
        }

        private void SymbolsList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (!symbolsList.GetClickRowColItem(e, out int unusedRow, out int unusedCol,
                    out object objItem)) {
                // Header or empty area; ignore.
                return;
            }
            FormattedSymbol item = (FormattedSymbol)objItem;
            DoEditSymbol(item.DefSym);
        }

        private void NewSymbolButton_Click(object sender, RoutedEventArgs e) {
            EditDefSymbol dlg = new EditDefSymbol(this, mFormatter, mWorkTable.GetSortedByLabel(),
                null, mSymbolTable, true, false);
            dlg.ShowDialog();
            if (dlg.DialogResult == true) {
                Debug.WriteLine("ADD: " + dlg.NewSym);
                mWorkTable.AddOrReplace(dlg.NewSym);

                // Add it to the display list, select it, and scroll it into view.
                FormattedSymbol newItem = CreateFormattedSymbol(dlg.NewSym);
                Variables.Add(newItem);
                symbolsList.SelectedItem = newItem;
                symbolsList.ScrollIntoView(newItem);

                UpdateControls();
            }
        }

        private void EditSymbolButton_Click(object sender, EventArgs e) {
            // Single-select list view, button dimmed when no selection.
            Debug.Assert(symbolsList.SelectedItems.Count == 1);
            FormattedSymbol item = (FormattedSymbol)symbolsList.SelectedItems[0];
            DoEditSymbol(item.DefSym);
        }

        private void DoEditSymbol(DefSymbol defSym) {
            EditDefSymbol dlg = new EditDefSymbol(this, mFormatter, mWorkTable.GetSortedByLabel(),
                defSym, mSymbolTable, true, false);
            dlg.ShowDialog();
            if (dlg.DialogResult == true) {
                // Label might have changed, so remove old before adding new.
                mWorkTable.RemoveByLabel(defSym.Label);
                mWorkTable.AddOrReplace(dlg.NewSym);

                // Replace entry in items source.
                for (int i = 0; i < Variables.Count; i++) {
                    if (Variables[i].DefSym == defSym) {
                        Variables[i] = CreateFormattedSymbol(dlg.NewSym);
                        break;
                    }
                }

                UpdateControls();
            }
        }

        private void RemoveSymbolButton_Click(object sender, RoutedEventArgs e) {
            // Single-select list view, button dimmed when no selection.
            Debug.Assert(symbolsList.SelectedItems.Count == 1);

            int selectionIndex = symbolsList.SelectedIndex;
            FormattedSymbol item = (FormattedSymbol)symbolsList.SelectedItems[0];
            DefSymbol defSym = item.DefSym;
            mWorkTable.RemoveByLabel(defSym.Label);
            for (int i = 0; i < Variables.Count; i++) {
                if (Variables[i].DefSym == defSym) {
                    Variables.RemoveAt(i);
                    break;
                }
            }
            UpdateControls();

            // Restore selection to the item that used to come after the one we just deleted,
            // so you can hit "Remove" repeatedly to delete multiple items.
            int newCount = symbolsList.Items.Count;
            if (selectionIndex >= newCount) {
                selectionIndex = newCount - 1;
            }
            if (selectionIndex >= 0) {
                symbolsList.SelectedIndex = selectionIndex;
                removeSymbolButton.Focus();     // so you can keep banging on Enter
            }
        }
    }
}
