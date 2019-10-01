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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
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

            LoadVariables();
        }

        public void Window_Loaded(object sender, RoutedEventArgs e) {
            string fmt = (string)FindResource("str_TableHeaderFmt");
            TableHeaderText = string.Format(fmt, mFormatter.FormatOffset24(mOffset));

            UpdateControls();
        }

        /// <summary>
        /// Loads entries from the work table into the items source.
        /// </summary>
        private void LoadVariables() {
            Variables.Clear();

            for (int i = 0; i < mWorkTable.Count; i++) {
                DefSymbol defSym = mWorkTable[i];
                string typeStr;
                if (defSym.SymbolType == Symbol.Type.Constant) {
                    typeStr = Res.Strings.ABBREV_STACK_RELATIVE;
                } else {
                    typeStr = Res.Strings.ABBREV_ADDRESS;
                }

                FormattedSymbol fsym = new FormattedSymbol(
                    defSym.Label,
                    mFormatter.FormatValueInBase(defSym.Value, defSym.DataDescriptor.NumBase),
                    typeStr,
                    defSym.DataDescriptor.Length.ToString(),
                    defSym.Comment);

                Variables.Add(fsym);
            }
        }

        private void UpdateControls() {
            // Enable or disable the edit/remove buttons based on how many items are selected.
            // (We're currently configured for single-select, so this is really just a != 0 test.)
            int symSelCount = symbolsListView.SelectedItems.Count;
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

        private void SymbolsListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            UpdateControls();
        }

        private void SymbolsListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ListViewItem lvi = symbolsListView.GetClickedItem(e);
            if (lvi == null) {
                return;
            }
            FormattedSymbol item = (FormattedSymbol)lvi.Content;
            DefSymbol defSym = mWorkTable.GetByLabel(item.Label);
            DoEditSymbol(defSym);
        }

        private void NewSymbolButton_Click(object sender, RoutedEventArgs e) {
            EditDefSymbol dlg = new EditDefSymbol(this, mFormatter, mWorkTable.GetSortedByLabel(),
                null, mSymbolTable, true, false);
            dlg.ShowDialog();
            if (dlg.DialogResult == true) {
                Debug.WriteLine("ADD: " + dlg.NewSym);
                mWorkTable.AddOrReplace(dlg.NewSym);

                // Reload the contents.  This loses the selection, but that shouldn't be an
                // issue when adding new symbols.  To do this incrementally we'd need to add
                // the symbol at the correct sorted position.
                LoadVariables();
                UpdateControls();
            }
        }

        private void EditSymbolButton_Click(object sender, EventArgs e) {
            // Single-select list view, button dimmed when no selection.
            Debug.Assert(symbolsListView.SelectedItems.Count == 1);
            FormattedSymbol item = (FormattedSymbol)symbolsListView.SelectedItems[0];
            DefSymbol defSym = mWorkTable.GetByLabel(item.Label);
            DoEditSymbol(defSym);
        }

        private void DoEditSymbol(DefSymbol defSym) {
            EditDefSymbol dlg = new EditDefSymbol(this, mFormatter, mWorkTable.GetSortedByLabel(),
                defSym, mSymbolTable, true, false);
            dlg.ShowDialog();
            if (dlg.DialogResult == true) {
                // Label might have changed, so remove old before adding new.
                mWorkTable.RemoveByLabel(defSym.Label);
                mWorkTable.AddOrReplace(dlg.NewSym);
                LoadVariables();
                UpdateControls();
            }
        }

        private void RemoveSymbolButton_Click(object sender, RoutedEventArgs e) {
            // Single-select list view, button dimmed when no selection.
            Debug.Assert(symbolsListView.SelectedItems.Count == 1);

            int selectionIndex = symbolsListView.SelectedIndex;
            FormattedSymbol item = (FormattedSymbol)symbolsListView.SelectedItems[0];
            mWorkTable.RemoveByLabel(item.Label);
            LoadVariables();
            UpdateControls();

            // Restore selection to the item that used to come after the one we just deleted,
            // so you can hit "Remove" repeatedly to delete multiple items.
            int newCount = symbolsListView.Items.Count;
            if (selectionIndex >= newCount) {
                selectionIndex = newCount - 1;
            }
            if (selectionIndex >= 0) {
                symbolsListView.SelectedIndex = selectionIndex;
                removeSymbolButton.Focus();
            }
        }
    }
}
