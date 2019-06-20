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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using CommonUtil;
using CommonWPF;

namespace SourceGenWPF.ProjWin {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        /// <summary>
        /// Disassembled code display list provided to XAML.
        /// </summary>
        public DisplayList CodeDisplayList { get; private set; }

        /// Version string, for display.
        /// </summary>
        public string ProgramVersionString {
            get { return App.ProgramVersion.ToString(); }
        }

        /// <summary>
        /// Reference to controller object.
        /// </summary>
        private MainController mMainCtrl;

        // Handle to protected ListView.SetSelectedItems() method
        private MethodInfo listViewSetSelectedItems;


        public MainWindow() {
            InitializeComponent();

            listViewSetSelectedItems = codeListView.GetType().GetMethod("SetSelectedItems",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(listViewSetSelectedItems != null);

            this.DataContext = this;

            CodeDisplayList = new DisplayList();
            codeListView.ItemsSource = CodeDisplayList;
            // https://dlaa.me/blog/post/9425496 to re-auto-size after data added (this may
            //  not work with virtual items)

            mMainCtrl = new MainController(this);

            AddMultiKeyGestures();

            // Get an event when the splitters move.  Because of the way things are set up, it's
            // actually best to get an event when the grid row/column sizes change.
            // https://stackoverflow.com/a/22495586/294248
            DependencyPropertyDescriptor widthDesc = DependencyPropertyDescriptor.FromProperty(
                ColumnDefinition.WidthProperty, typeof(ItemsControl));
            DependencyPropertyDescriptor heightDesc = DependencyPropertyDescriptor.FromProperty(
                RowDefinition.HeightProperty, typeof(ItemsControl));
            // main window, left/right panels
            widthDesc.AddValueChanged(triptychGrid.ColumnDefinitions[0], GridSizeChanged);
            widthDesc.AddValueChanged(triptychGrid.ColumnDefinitions[4], GridSizeChanged);
            // references vs. notes
            heightDesc.AddValueChanged(leftPanel.RowDefinitions[0], GridSizeChanged);
            heightDesc.AddValueChanged(rightPanel.RowDefinitions[0], GridSizeChanged);

            // Add events that fire when column headers change size.  We want this for
            // the DataGrids and the main ListView.
            PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty(
                DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
            AddColumnWidthChangeCallback(pd, referencesGrid);
            AddColumnWidthChangeCallback(pd, notesGrid);
            AddColumnWidthChangeCallback(pd, symbolsGrid);

            // Same for the ListView.  cf. https://stackoverflow.com/a/56694219/294248
            pd = DependencyPropertyDescriptor.FromProperty(
                GridViewColumn.WidthProperty, typeof(GridViewColumn));
            AddColumnWidthChangeCallback(pd, (GridView)codeListView.View);
        }

        private void AddColumnWidthChangeCallback(PropertyDescriptor pd, DataGrid dg) {
            foreach (DataGridColumn col in dg.Columns) {
                pd.AddValueChanged(col, ColumnWidthChanged);
            }
        }
        private void AddColumnWidthChangeCallback(PropertyDescriptor pd, GridView gv) {
            foreach (GridViewColumn col in gv.Columns) {
                pd.AddValueChanged(col, ColumnWidthChanged);
            }
        }

        private void AddMultiKeyGestures() {
            RoutedUICommand ruic;

            ruic = (RoutedUICommand)FindResource("HintAsCodeEntryPointCmd");
            ruic.InputGestures.Add(
                new MultiKeyInputGesture(new KeyGesture[] {
                      new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H"),
                      new KeyGesture(Key.C, ModifierKeys.Control, "Ctrl+C")
                }));
            ruic = (RoutedUICommand)FindResource("HintAsDataStartCmd");
            ruic.InputGestures.Add(
                new MultiKeyInputGesture(new KeyGesture[] {
                      new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H"),
                      new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D")
                }));
            ruic = (RoutedUICommand)FindResource("HintAsInlineDataCmd");
            ruic.InputGestures.Add(
                new MultiKeyInputGesture(new KeyGesture[] {
                      new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H"),
                      new KeyGesture(Key.I, ModifierKeys.Control, "Ctrl+I")
                }));
            ruic = (RoutedUICommand)FindResource("RemoveHintsCmd");
            ruic.InputGestures.Add(
                new MultiKeyInputGesture(new KeyGesture[] {
                      new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H"),
                      new KeyGesture(Key.R, ModifierKeys.Control, "Ctrl+R")
                }));
        }

        private void CreateCodeListContextMenu() {
            // Find Actions menu.
            ItemCollection mainItems = this.appMenu.Items;
            MenuItem actionsMenu = null;
            foreach (object obj in mainItems) {
                if (!(obj is MenuItem)) {
                    continue;
                }
                MenuItem mi = (MenuItem)obj;
                if (mi.Name.Equals("ActionsMenu")) {
                    actionsMenu = mi;
                    break;
                }
            }
            Debug.Assert(actionsMenu != null);

            // Clone the Actions menu into the codeListView context menu.
            ContextMenu ctxt = this.codeListView.ContextMenu;
            foreach (object item in actionsMenu.Items) {
                if (item is MenuItem) {
                    MenuItem oldItem = (MenuItem)item;
                    MenuItem newItem = new MenuItem();
                    // I don't see a "clone" method, so just copy the fields we think we care about
                    newItem.Name = oldItem.Name;
                    newItem.Header = oldItem.Header;
                    newItem.Icon = oldItem.Icon;
                    newItem.InputGestureText = oldItem.InputGestureText;
                    newItem.Command = oldItem.Command;
                    ctxt.Items.Add(newItem);
                } else if (item is Separator) {
                    ctxt.Items.Add(new Separator());
                } else {
                    Debug.Assert(false, "Found weird thing in menu: " + item);
                }
            }
        }

        /// <summary>
        /// INotifyPropertyChanged event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Call this when a notification-worthy property changes value.
        /// 
        /// The CallerMemberName attribute puts the calling property's name in the first arg.
        /// </summary>
        /// <param name="propertyName">Name of property that changed.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Which panel are we showing, launchPanel or codeListView?
        /// </summary>
        public bool ShowCodeListView {
            get {
                return mShowCodeListView;
            }
            set {
                mShowCodeListView = value;
                OnPropertyChanged("LaunchPanelVisibility");
                OnPropertyChanged("CodeListVisibility");
            }
        }
        private bool mShowCodeListView;

        /// <summary>
        /// Returns the visibility status of the launch panel.
        /// (Intended for use from XAML.)
        /// </summary>
        public Visibility LaunchPanelVisibility {
            get { return mShowCodeListView ? Visibility.Hidden : Visibility.Visible; }
        }

        /// <summary>
        /// Returns the visibility status of the code ListView.
        /// (Intended for use from XAML.)
        /// </summary>
        public Visibility CodeListVisibility {
            get { return mShowCodeListView ? Visibility.Visible : Visibility.Hidden; }
        }


        /// <summary>
        /// Handles source-initialized event.  This happens before Loaded, before the window
        /// is visible, which makes it a good time to set the size and position.
        /// </summary>
        private void Window_SourceInitialized(object sender, EventArgs e) {
            mMainCtrl.WindowSourceInitialized();
        }

        /// <summary>
        /// Handles window-loaded event.  Window is ready to go, so we can start doing things
        /// that involve user interaction.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            mMainCtrl.WindowLoaded();
            CreateCodeListContextMenu();

#if DEBUG
            // Get more info on CollectionChanged events that do not agree with current
            // state of Items collection.
            PresentationTraceSources.SetTraceLevel(codeListView.ItemContainerGenerator,
                PresentationTraceLevel.High);
#endif
        }

        /// <summary>
        /// Handles window-close event.  The user has an opportunity to cancel.
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e) {
            Debug.WriteLine("Main app window closing");
            if (mMainCtrl == null) {
                // early failure?
                return;
            }
            if (!mMainCtrl.WindowClosing()) {
                e.Cancel = true;
                return;
            }
        }

        /// <summary>
        /// Catch mouse-down events so we can treat the fourth mouse button as "back".
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.XButton1) {
                Debug.WriteLine("TODO: navigate back");
            }
        }

        #region Window placement

        //
        // We record the location and size of the window, the sizes of the panels, and the
        // widths of the various columns.  These events may fire rapidly while the user is
        // resizing them, so we just want to set a flag noting that a change has been made.
        //
        private void Window_LocationChanged(object sender, EventArgs e) {
            //Debug.WriteLine("Main window location changed");
            AppSettings.Global.Dirty = true;
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            //Debug.WriteLine("Main window size changed");
            AppSettings.Global.Dirty = true;
        }
        private void GridSizeChanged(object sender, EventArgs e) {
            //Debug.WriteLine("Splitter size change");
            AppSettings.Global.Dirty = true;
        }
        private void ColumnWidthChanged(object sender, EventArgs e) {
            //Debug.WriteLine("Column width change " + sender);
            AppSettings.Global.Dirty = true;
        }

        public double LeftPanelWidth {
            get { return triptychGrid.ColumnDefinitions[0].ActualWidth; }
            set { triptychGrid.ColumnDefinitions[0].Width = new GridLength(value); }
        }
        public double RightPanelWidth {
            get { return triptychGrid.ColumnDefinitions[4].ActualWidth; }
            set { triptychGrid.ColumnDefinitions[4].Width = new GridLength(value); }
        }
        public double ReferencesPanelHeight {
            get { return leftPanel.RowDefinitions[0].ActualHeight; }
            set {
                // If you set the height to a pixel value, you lose the auto-sizing behavior,
                // and the splitter will happily shove the bottom panel off the bottom of the
                // main window.  The trick is to use "star" units.
                // Thanks: https://stackoverflow.com/q/35000893/294248
                double totalHeight = leftPanel.RowDefinitions[0].ActualHeight +
                    leftPanel.RowDefinitions[2].ActualHeight;
                leftPanel.RowDefinitions[0].Height = new GridLength(value, GridUnitType.Star);
                leftPanel.RowDefinitions[2].Height = new GridLength(totalHeight - value,
                    GridUnitType.Star);
            }
        }
        public double SymbolsPanelHeight {
            get { return rightPanel.RowDefinitions[0].ActualHeight; }
            set {
                double totalHeight = rightPanel.RowDefinitions[0].ActualHeight +
                    rightPanel.RowDefinitions[2].ActualHeight;
                rightPanel.RowDefinitions[0].Height = new GridLength(value, GridUnitType.Star);
                rightPanel.RowDefinitions[2].Height = new GridLength(totalHeight - value,
                    GridUnitType.Star);
            }
        }

        #endregion Window placement

        /// <summary>
        /// Grabs the widths of the columns of the various grids and saves them in the
        /// global AppSettings.
        /// </summary>
        public void CaptureColumnWidths() {
            string widthStr;

            widthStr = CaptureColumnWidths((GridView)codeListView.View);
            AppSettings.Global.SetString(AppSettings.CDLV_COL_WIDTHS, widthStr);

            widthStr = CaptureColumnWidths(referencesGrid);
            AppSettings.Global.SetString(AppSettings.REFWIN_COL_WIDTHS, widthStr);
            widthStr = CaptureColumnWidths(notesGrid);
            AppSettings.Global.SetString(AppSettings.NOTEWIN_COL_WIDTHS, widthStr);
            widthStr = CaptureColumnWidths(symbolsGrid);
            AppSettings.Global.SetString(AppSettings.SYMWIN_COL_WIDTHS, widthStr);
        }
        private string CaptureColumnWidths(GridView gv) {
            int[] widths = new int[gv.Columns.Count];
            for (int i = 0; i < gv.Columns.Count; i++) {
                widths[i] = (int)Math.Round(gv.Columns[i].ActualWidth);
            }
            return TextUtil.SerializeIntArray(widths);
        }
        private string CaptureColumnWidths(DataGrid dg) {
            int[] widths = new int[dg.Columns.Count];
            for (int i = 0; i < dg.Columns.Count; i++) {
                widths[i] = (int)Math.Round(dg.Columns[i].ActualWidth);
            }
            return TextUtil.SerializeIntArray(widths);
        }

        /// <summary>
        /// Applies column widths from the global AppSettings to the various grids.
        /// </summary>
        public void RestoreColumnWidths() {
            RestoreColumnWidths((GridView)codeListView.View,
                AppSettings.Global.GetString(AppSettings.CDLV_COL_WIDTHS, null));

            RestoreColumnWidths(referencesGrid,
                AppSettings.Global.GetString(AppSettings.REFWIN_COL_WIDTHS, null));
            RestoreColumnWidths(notesGrid,
                AppSettings.Global.GetString(AppSettings.NOTEWIN_COL_WIDTHS, null));
            RestoreColumnWidths(symbolsGrid,
                AppSettings.Global.GetString(AppSettings.SYMWIN_COL_WIDTHS, null));
        }
        private void RestoreColumnWidths(GridView gv, string str) {
            int[] widths = null;
            try {
                widths = TextUtil.DeserializeIntArray(str);
            } catch (Exception ex) {
                Debug.WriteLine("Unable to deserialize widths for GridView");
                return;
            }
            if (widths.Length != gv.Columns.Count) {
                Debug.WriteLine("Incorrect column count for GridView");
                return;
            }

            for (int i = 0; i < widths.Length; i++) {
                gv.Columns[i].Width = widths[i];
            }
        }
        private void RestoreColumnWidths(DataGrid dg, string str) {
            int[] widths = null;
            try {
                widths = TextUtil.DeserializeIntArray(str);
            } catch (Exception ex) {
                Debug.WriteLine("Unable to deserialize widths for " + dg.Name);
                return;
            }
            if (widths.Length != dg.Columns.Count) {
                Debug.WriteLine("Incorrect column count for " + dg.Name);
                return;
            }

            for (int i = 0; i < widths.Length; i++) {
                dg.Columns[i].Width = widths[i];
            }
        }

        /// <summary>
        /// Sets the focus on the code list.
        /// </summary>
        //public void CodeListView_Focus() {
        //    codeListView.Focus();
        //}

        /// <summary>
        /// Handles a double-click on the code list.  We have to figure out which row and
        /// column were clicked, which is not easy in WPF.
        /// </summary>
        private void CodeListView_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Debug.Assert(sender == codeListView);

            ListViewItem lvi = codeListView.GetClickedItem(e);
            if (lvi == null) {
                return;
            }
            DisplayList.FormattedParts parts = (DisplayList.FormattedParts)lvi.Content;
            int row = parts.ListIndex;
            int col = codeListView.GetClickEventColumn(e);
            if (col < 0) {
                return;
            }
            mMainCtrl.HandleCodeListDoubleClick(row, col);
        }


        #region Selection management

        private void CodeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //DateTime startWhen = DateTime.Now;

            // Update the selected-item bitmap.
            CodeDisplayList.SelectedIndices.SelectionChanged(e);

            // Notify MainController that the selection has changed.
            mMainCtrl.SelectionChanged();

            Debug.Assert(CodeDisplayList.SelectedIndices.DebugValidateSelectionCount(
                codeListView.SelectedItems.Count));

            //Debug.WriteLine("SelectionChanged took " +
            //    (DateTime.Now - startWhen).TotalMilliseconds + " ms");
        }

        /// <summary>
        /// Returns the number of selected items.
        /// </summary>
        /// <returns>
        /// The SelectedItems list appears to hold the full set, so we can just return the count.
        /// </returns>
        public int CodeListView_GetSelectionCount() {
            return codeListView.SelectedItems.Count;
        }

        /// <summary>
        /// Returns the index of the first selected item, or -1 if nothing is selected.
        /// </summary>
        /// <remarks>
        /// The ListView.SelectedIndex property returns the index of a selected item, but
        /// doesn't make guarantees about first or last.
        /// 
        /// This would be easier if the ListView kept SelectedItems in sorted order.  However,
        /// if you ctrl+click around you can get to a point where entry[0] is not the first
        /// and entry[count-1] is not the last selected item.
        /// 
        /// So we either have to walk the SelectedItems list or the DisplayListSelection array.
        /// For short selections the former will be faster than the later.  I'm assuming the
        /// common cases will be short selections and select-all, so this should handle both
        /// efficiently.
        /// </remarks>
        public int CodeListView_GetFirstSelectedIndex() {
            int count = codeListView.SelectedItems.Count;
            if (count == 0) {
                return -1;
            } else if (count < 500) {
                int min = CodeDisplayList.Count;
                foreach (DisplayList.FormattedParts parts in codeListView.SelectedItems) {
                    if (min > parts.ListIndex) {
                        min = parts.ListIndex;
                    }
                }
                Debug.Assert(min < CodeDisplayList.Count);
                return min;
            } else {
                return CodeDisplayList.SelectedIndices.GetFirstSelectedIndex();
            }
        }

        /// <summary>
        /// Returns the index of the last selected item, or -1 if nothing is selected.
        /// </summary>
        /// <remarks>
        /// Again, the ListView does not provide what we need.
        /// </remarks>
        public int CodeListView_GetLastSelectedIndex() {
            int count = codeListView.SelectedItems.Count;
            if (count == 0) {
                return -1;
            } else if (count < 500) {
                int max = -1;
                foreach (DisplayList.FormattedParts parts in codeListView.SelectedItems) {
                    if (max < parts.ListIndex) {
                        max = parts.ListIndex;
                    }
                }
                Debug.Assert(max >= 0);
                return max;
            } else {
                return CodeDisplayList.SelectedIndices.GetLastSelectedIndex();
            }
        }

        /// <summary>
        /// De-selects all items.
        /// </summary>
        public void CodeListView_DeselectAll() {
            codeListView.SelectedItems.Clear();
        }

        /// <summary>
        /// Selects a range of values.  Does not clear the previous selection.
        /// </summary>
        /// <param name="start">First line to select.</param>
        /// <param name="count">Number of lines to select.</param>
        public void CodeListView_SelectRange(int start, int count) {
            Debug.Assert(start >= 0 && start < CodeDisplayList.Count);
            Debug.Assert(count > 0 && start + count <= CodeDisplayList.Count);

            DisplayList.FormattedParts[] tmpArray = new DisplayList.FormattedParts[count];
            for (int index = 0; index < count; index++) {
                tmpArray[index] = CodeDisplayList[start + index];
            }
            listViewSetSelectedItems.Invoke(codeListView, new object[] { tmpArray });
        }

        /// <summary>
        /// Sets the code list selection.
        /// </summary>
        /// <param name="sel">Selection bitmap.</param>
        public void CodeListView_SetSelection(DisplayListSelection sel) {
            const int MAX_SEL_COUNT = 2000;

            if (sel.IsAllSelected()) {
                Debug.WriteLine("SetSelection: re-selecting all items");
                codeListView.SelectAll();
                return;
            }
            Debug.Assert(codeListView.SelectedItems.Count == 0);    // expected
            codeListView.SelectedItems.Clear();                     // just in case

            if (sel.Count > MAX_SEL_COUNT) {
                // Too much for WPF -- only restore the first item.
                Debug.WriteLine("SetSelection: not restoring (" + sel.Count + " items)");
                codeListView.SelectedItems.Add(CodeDisplayList[sel.GetFirstSelectedIndex()]);
                return;
            }

            //DateTime startWhen = DateTime.Now;

            DisplayList.FormattedParts[] tmpArray = new DisplayList.FormattedParts[sel.Count];
            int ai = 0;
            foreach (int listIndex in sel) {
                tmpArray[ai++] = CodeDisplayList[listIndex];
            }

            // Use a reflection call to provide the full set.  This is much faster than
            // adding the items one at a time to SelectedItems.  (For one thing, it only
            // invokes the SelectionChanged method once.)
            listViewSetSelectedItems.Invoke(codeListView, new object[] { tmpArray });

            //Debug.WriteLine("SetSelection on " + sel.Count + " items took " +
            //    (DateTime.Now - startWhen).TotalMilliseconds + " ms");
        }

        public int CodeListView_GetTopIndex() {
            return codeListView.GetTopItemIndex();
        }

        public void CodeListView_SetTopIndex(int index) {
            // ScrollIntoView does the least amount of scrolling required.  This extension
            // method scrolls to the bottom, then scrolls back up to the top item.
            //
            // NOTE: it looks like scroll-to-bottom (which is done directly on the
            // ScrollViewer) happens immediately, whiel scroll-to-item (which is done via the
            // ListView) kicks in later.  So don't try to check the topmost item immediately.
            codeListView.ScrollToTopItem(CodeDisplayList[index]);
        }

        /// <summary>
        /// Scrolls the code list to ensure that the specified index is visible.
        /// </summary>
        /// <param name="index">Line index of item.</param>
        public void CodeListView_EnsureVisible(int index) {
            Debug.Assert(index >= 0 && index < CodeDisplayList.Count);
            codeListView.ScrollIntoView(CodeDisplayList[index]);
        }

        /// <summary>
        /// Adds an address/label selection highlight to the specified line.
        /// </summary>
        /// <param name="index">Line index.  If &lt; 0, method has no effect.</param>
        public void CodeListView_AddSelectionHighlight(int index) {
            if (index < 0) {
                return;
            }
            CodeListView_ReplaceEntry(index,
                DisplayList.FormattedParts.AddSelectionHighlight(CodeDisplayList[index]));
        }

        /// <summary>
        /// Removes an address/label selection highlight from the specified line.
        /// </summary>
        /// <param name="index">Line index.  If &lt; 0, method has no effect.</param>
        public void CodeListView_RemoveSelectionHighlight(int index) {
            if (index < 0) {
                return;
            }
            CodeListView_ReplaceEntry(index,
                DisplayList.FormattedParts.RemoveSelectionHighlight(CodeDisplayList[index]));
        }

        /// <summary>
        /// Replaces an entry in the code list.  If the item was selected, the selection is
        /// cleared and restored.
        /// </summary>
        /// <param name="index">List index.</param>
        /// <param name="newParts">Replacement parts.</param>
        private void CodeListView_ReplaceEntry(int index, DisplayList.FormattedParts newParts) {
            bool isSelected = CodeDisplayList.SelectedIndices[index];
            if (isSelected) {
                codeListView.SelectedItems.Remove(CodeDisplayList[index]);
            }
            CodeDisplayList[index] = newParts;
            if (isSelected) {
                codeListView.SelectedItems.Add(newParts);
            }
        }

        #endregion Selection management

        #region Can-execute handlers

        /// <summary>
        /// Returns true if the project is open.  Intended for use in XAML CommandBindings.
        /// </summary>
        private void IsProjectOpen(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mMainCtrl != null && mMainCtrl.IsProjectOpen();
        }

        private void CanEditAddress(object sender, CanExecuteRoutedEventArgs e) {
            if (mMainCtrl == null || !mMainCtrl.IsProjectOpen()) {
                e.CanExecute = false;
                return;
            }
            e.CanExecute = mMainCtrl.CanEditAddress();
        }

        private void CanHintAsCodeEntryPoint(object sender, CanExecuteRoutedEventArgs e) {
            if (mMainCtrl == null || !mMainCtrl.IsProjectOpen()) {
                e.CanExecute = false;
                return;
            }
            MainController.EntityCounts counts = mMainCtrl.SelectionAnalysis.mEntityCounts;
            e.CanExecute = (counts.mDataLines > 0 || counts.mCodeLines > 0) &&
                (counts.mDataHints != 0 || counts.mInlineDataHints != 0 || counts.mNoHints != 0);
        }
        private void CanHintAsDataStart(object sender, CanExecuteRoutedEventArgs e) {
            if (mMainCtrl == null || !mMainCtrl.IsProjectOpen()) {
                e.CanExecute = false;
                return;
            }
            MainController.EntityCounts counts = mMainCtrl.SelectionAnalysis.mEntityCounts;
            e.CanExecute = (counts.mDataLines > 0 || counts.mCodeLines > 0) &&
                (counts.mCodeHints != 0 || counts.mInlineDataHints != 0 || counts.mNoHints != 0);
        }
        private void CanHintAsInlineData(object sender, CanExecuteRoutedEventArgs e) {
            if (mMainCtrl == null || !mMainCtrl.IsProjectOpen()) {
                e.CanExecute = false;
                return;
            }
            MainController.EntityCounts counts = mMainCtrl.SelectionAnalysis.mEntityCounts;
            e.CanExecute = (counts.mDataLines > 0 || counts.mCodeLines > 0) &&
                (counts.mCodeHints != 0 || counts.mDataHints != 0 || counts.mNoHints != 0);
        }
        private void CanRemoveHints(object sender, CanExecuteRoutedEventArgs e) {
            if (mMainCtrl == null || !mMainCtrl.IsProjectOpen()) {
                e.CanExecute = false;
                return;
            }
            MainController.EntityCounts counts = mMainCtrl.SelectionAnalysis.mEntityCounts;
            e.CanExecute = (counts.mDataLines > 0 || counts.mCodeLines > 0) &&
                (counts.mCodeHints != 0 || counts.mDataHints != 0 || counts.mInlineDataHints != 0);
        }

        private void CanRedo(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mMainCtrl != null && mMainCtrl.CanRedo();
        }
        private void CanUndo(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mMainCtrl != null && mMainCtrl.CanUndo();
        }

        private void CanNavigateBackward(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mMainCtrl != null && mMainCtrl.CanNavigateBackward();
        }
        private void CanNavigateForward(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = mMainCtrl != null && mMainCtrl.CanNavigateForward();
        }

        #endregion Can-execute handlers


        #region Command handlers

        private void AssembleCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            // test
            Debug.WriteLine("assembling");
        }

        private void CloseCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (!mMainCtrl.CloseProject()) {
                Debug.WriteLine("Close canceled");
            }
        }

        private void EditAddressCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditAddress();
        }

        private void HelpCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ShowHelp();
        }

        private void HintAsCodeEntryPointCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.WriteLine("hint as code entry point");
            mMainCtrl.MarkAsType(CodeAnalysis.TypeHint.Code, true);
        }

        private void HintAsDataStartCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.WriteLine("hint as data start");
            mMainCtrl.MarkAsType(CodeAnalysis.TypeHint.Data, true);
        }

        private void HintAsInlineDataCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.WriteLine("hint as inline data");
            mMainCtrl.MarkAsType(CodeAnalysis.TypeHint.InlineData, false);
        }

        private void NavigateBackwardCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.NavigateBackward();
        }

        private void NavigateForwardCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.NavigateForward();
        }

        private void RemoveHintsCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.WriteLine("remove hints");
            mMainCtrl.MarkAsType(CodeAnalysis.TypeHint.NoHint, false);
        }

        private void RedoCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.RedoChanges();
        }

        private void SelectAllCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            DateTime start = DateTime.Now;

            codeListView.SelectAll();

            //codeListView.SelectedItems.Clear();
            //foreach (var item in codeListView.Items) {
            //    codeListView.SelectedItems.Add(item);
            //}

            // This seems to be faster than setting items individually (10x), but is still O(n^2)
            // or worse, and hence unsuitable for very large lists.
            //codeListView.SelectedItems.Clear();
            //listViewSetSelectedItems.Invoke(codeListView, new object[] { codeListView.Items });

            Debug.WriteLine("Select All cmd: " + (DateTime.Now - start).TotalMilliseconds + " ms");
        }

        private void RecentProjectCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (!int.TryParse((string)e.Parameter, out int recentIndex) ||
                    recentIndex < 0 || recentIndex >= MainController.MAX_RECENT_PROJECTS) {
                throw new Exception("Bad parameter: " + e.Parameter);
            }

            Debug.WriteLine("Recent project #" + recentIndex);
            mMainCtrl.OpenRecentProject(recentIndex);
        }

        private void UndoCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.UndoChanges();
        }

        #endregion Command handlers


        #region References panel

        public class ReferencesListItem {
            public int OffsetValue { get; private set; }
            public string Offset { get; private set; }
            public string Addr { get; private set; }
            public string Type { get; private set; }

            public ReferencesListItem(int offsetValue, string offset, string addr, string type) {
                OffsetValue = offsetValue;
                Offset = offset;
                Addr = addr;
                Type = type;
            }

            public override string ToString() {
                return "[ReferencesListItem: off=" + Offset + " addr=" + Addr + " type=" +
                    Type + "]";
            }
        }

        public ObservableCollection<ReferencesListItem> ReferencesList { get; private set; } =
            new ObservableCollection<ReferencesListItem>();

        private void ReferencesList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (!referencesGrid.GetClickRowColItem(e, out int rowIndex, out int colIndex,
                    out object item)) {
                // Header or empty area; ignore.
                return;
            }
            ReferencesListItem rli = (ReferencesListItem)item;

            // Jump to the offset, then shift the focus back to the code list.
            mMainCtrl.GoToOffset(rli.OffsetValue, false, true);
            codeListView.Focus();
        }

        #endregion References panel


        #region Notes panel

        public class NotesListItem {
            public int OffsetValue { get; private set; }
            public string Offset { get; private set; }
            public string Note { get; private set; }
            public SolidColorBrush BackBrush { get; private set; }

            public NotesListItem(int offsetValue, string offset, string note, Color backColor) {
                OffsetValue = offsetValue;
                Offset = offset;
                Note = note;
                BackBrush = new SolidColorBrush(backColor);
            }

            public override string ToString() {
                return "[NotesListItem: off=" + Offset + " note=" + Note + " brush=" +
                    BackBrush + "]";
            }
        }

        public ObservableCollection<NotesListItem> NotesList { get; private set; } =
            new ObservableCollection<NotesListItem>();

        private void NotesList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (!notesGrid.GetClickRowColItem(e, out int rowIndex, out int colIndex,
                    out object item)) {
                // Header or empty area; ignore.
                return;
            }
            NotesListItem nli = (NotesListItem)item;

            // Jump to the offset, then shift the focus back to the code list.
            mMainCtrl.GoToOffset(nli.OffsetValue, true, true);
            codeListView.Focus();
        }

        #endregion Notes panel


        #region Symbols panel

        public class SymbolsListItem {
            public Symbol Sym { get; private set; }
            public string Type { get; private set; }
            public string Value { get; private set; }
            public string Name { get; private set; }

            public SymbolsListItem(Symbol sym, string type, string value, string name) {
                Sym = sym;

                Type = type;
                Value = value;
                Name = name;
            }

            public override string ToString() {
                return "[SymbolsListItem: type=" + Type + " value=" + Value + " name=" +
                    Name + "]";
            }
        }

        public ObservableCollection<SymbolsListItem> SymbolsList { get; private set; } =
            new ObservableCollection<SymbolsListItem>();

        private void SymbolsList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (!symbolsGrid.GetClickRowColItem(e, out int rowIndex, out int colIndex,
                    out object item)) {
                // Header or empty area; ignore.
                return;
            }
            SymbolsListItem sli = (SymbolsListItem)item;

            mMainCtrl.GoToLabel(sli.Sym);
            codeListView.Focus();
        }

        private void SymbolsList_Filter(object sender, FilterEventArgs e) {
            SymbolsListItem sli = (SymbolsListItem)e.Item;
            if (sli == null) {
                return;
            }
            if ((symUserLabels.IsChecked != true && sli.Sym.SymbolSource == Symbol.Source.User) ||
                (symProjectSymbols.IsChecked != true && sli.Sym.SymbolSource == Symbol.Source.Project) ||
                (symPlatformSymbols.IsChecked != true && sli.Sym.SymbolSource == Symbol.Source.Platform) ||
                (symAutoLabels.IsChecked != true && sli.Sym.SymbolSource == Symbol.Source.Auto) ||
                (symConstants.IsChecked != true && sli.Sym.SymbolType == Symbol.Type.Constant) ||
                (symAddresses.IsChecked != true && sli.Sym.SymbolType != Symbol.Type.Constant))
            {
                e.Accepted = false;
            } else {
                e.Accepted = true;
            }
        }

        /// <summary>
        /// Refreshes the symbols list when a filter option changes.  Set this to be called
        /// for Checked/Unchecked events on the filter option buttons.
        /// </summary>
        private void SymbolsListFilter_Changed(object sender, RoutedEventArgs e) {
            // This delightfully obscure call causes the list to refresh.  See
            // https://docs.microsoft.com/en-us/dotnet/framework/wpf/controls/how-to-group-sort-and-filter-data-in-the-datagrid-control
            CollectionViewSource.GetDefaultView(symbolsGrid.ItemsSource).Refresh();
        }

        /// <summary>
        /// Handles a Sorting event.  We want to do a secondary sort on Name when one of the
        /// other columns is the primary sort key.
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

            IComparer comparer;

            switch (col.Header) {
                case "Type":
                    comparer = new SymTabSortComparer(Symbol.SymbolSortField.CombinedType,
                        isAscending);
                    break;
                case "Value":
                    comparer = new SymTabSortComparer(Symbol.SymbolSortField.Value, isAscending);
                    break;
                case "Name":
                    comparer = new SymTabSortComparer(Symbol.SymbolSortField.Name, isAscending);
                    break;
                default:
                    comparer = null;
                    Debug.Assert(false);
                    break;
            }

            ListCollectionView lcv =
                (ListCollectionView)CollectionViewSource.GetDefaultView(symbolsGrid.ItemsSource);
            lcv.CustomSort = comparer;
            e.Handled = true;
        }

        // Symbol table sort comparison helper.
        private class SymTabSortComparer : IComparer {
            private Symbol.SymbolSortField mSortField;
            private bool mIsAscending;

            public SymTabSortComparer(Symbol.SymbolSortField prim, bool isAscending) {
                mSortField = prim;
                mIsAscending = isAscending;
            }

            // IComparer interface
            public int Compare(object oa, object ob) {
                Symbol a = ((SymbolsListItem)oa).Sym;
                Symbol b = ((SymbolsListItem)ob).Sym;

                return Symbol.Compare(mSortField, mIsAscending, a, b);
            }
        }

        #endregion Symbols panel


        #region Info panel

        /// <summary>
        /// Text to display in the Info panel.  This is a simple TextBox.
        /// </summary>
        public string InfoPanelContents {
            get {
                return mInfoBoxContents;
            }
            set {
                mInfoBoxContents = value;
                OnPropertyChanged();
            }
        }
        private string mInfoBoxContents;

        #endregion Info panel
    }
}
