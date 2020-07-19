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
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using CommonUtil;
using CommonWPF;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        /// <summary>
        /// Disassembled code display list provided to XAML.
        /// </summary>
        public DisplayList CodeDisplayList { get; private set; }

        /// <summary>
        /// Version string, for display.
        /// </summary>
        public string ProgramVersionString {
            get { return App.ProgramVersion.ToString(); }
        }

        /// <summary>
        /// Text for status bar at bottom of window.
        /// </summary>
        public string StatusBarText {
            get { return mStatusBarText; }
            set { mStatusBarText = value; OnPropertyChanged(); }
        }
        private string mStatusBarText;

        /// <summary>
        /// Text for code/data breakdown string.
        /// </summary>
        public string ByteCountText {
            get { return mByteCountText; }
            set { mByteCountText = value; OnPropertyChanged(); }
        }
        private string mByteCountText;

        /// <summary>
        /// Width of long comment fields.
        /// </summary>
        /// <remarks>
        /// We need this to be the sum of the leftmost four columns.  If we don't set it, the
        /// text may be cut off, or -- worse -- extend off the side of the window.  If it
        /// extends off the end, a scrollbar appears that will scroll the GridView contents
        /// without scrolling the GridView headers, which looks terrible.
        ///
        /// XAML doesn't do math, so we need to do it here, whenever the column widths change.
        /// </remarks>
        public double LongCommentWidth {
            get { return mLongCommentWidth; }
            set { mLongCommentWidth = value; OnPropertyChanged(); }
        }
        private double mLongCommentWidth;

        /// <summary>
        /// Set to true if the DEBUG menu should be visible on the main menu strip.
        /// </summary>
        public bool ShowDebugMenu {
            get { return mShowDebugMenu; }
            set { mShowDebugMenu = value; OnPropertyChanged(); }
        }
        bool mShowDebugMenu;



        /// <summary>
        /// Reference to controller object.
        /// </summary>
        private MainController mMainCtrl;

        // Handle to protected ListView.SetSelectedItems() method
        private MethodInfo listViewSetSelectedItems;

        // Color theme.
        public enum ColorScheme { Unknown = 0, Light, Dark };
        private ColorScheme mColorScheme;
        private ResourceDictionary mLightTheme;
        private ResourceDictionary mDarkTheme;


        public MainWindow() {
            Debug.WriteLine("START at " + DateTime.Now.ToLocalTime());
            InitializeComponent();

            // Prep the crash handler.
            Misc.AppIdent = "6502bench SourceGen v" + App.ProgramVersion.ToString();
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(Misc.CrashReporter);

            listViewSetSelectedItems = codeListView.GetType().GetMethod("SetSelectedItems",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(listViewSetSelectedItems != null);

            this.DataContext = this;

            mLightTheme = new ResourceDictionary() {
                Source = new Uri("/Res/Theme_Light.xaml", UriKind.Relative)
            };
            mDarkTheme = new ResourceDictionary() {
                Source = new Uri("/Res/Theme_Dark.xaml", UriKind.Relative)
            };
            Resources.MergedDictionaries.Add(mLightTheme);
            mColorScheme = ColorScheme.Light;


            CodeDisplayList = new DisplayList();
            codeListView.ItemsSource = CodeDisplayList;
            // https://dlaa.me/blog/post/9425496 to re-auto-size after data added (this may
            //  not work with virtual items)

            // Obscure tweak to make the arrow keys work right after a change.
            codeListView.ItemContainerGenerator.StatusChanged +=
                ItemContainerGenerator_StatusChanged;

            mMainCtrl = new MainController(this);

            StatusBarText = Res.Strings.STATUS_READY;

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

            UpdateLongCommentWidth();
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
            //// Find Actions menu.
            //ItemCollection mainItems = this.appMenu.Items;
            //foreach (object obj in mainItems) {
            //    if (!(obj is MenuItem)) {
            //        continue;
            //    }
            //    MenuItem mi = (MenuItem)obj;
            //    if (mi.Name.Equals("actionsMenu")) {
            //        actionsMenu = mi;
            //        break;
            //    }
            //}
            //Debug.Assert(actionsMenu != null);

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
            get { return mShowCodeListView; }
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
            get { return mShowCodeListView ? Visibility.Collapsed : Visibility.Visible; }
        }

        /// <summary>
        /// Returns the visibility status of the code ListView.
        /// (Intended for use from XAML.)
        /// </summary>
        public Visibility CodeListVisibility {
            get { return mShowCodeListView ? Visibility.Visible : Visibility.Collapsed; }
        }

        public FontFamily CodeListFontFamily {
            get { return codeListView.FontFamily; }
        }
        public double CodeListFontSize {
            get { return codeListView.FontSize; }
        }

        public void SetCodeListFont(string familyName, int size) {
            FontFamily fam = new FontFamily(familyName);
            codeListView.FontFamily = fam;
            codeListView.FontSize = size;
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
            }
        }

        /// <summary>
        /// Cleans up state when MainController decides to close the project.
        /// </summary>
        public void ProjectClosing() {
            // Clear this to release the memory.
            CodeDisplayList.Clear();

            // Clear these so we're not still showing them after the project closes.
            SymbolsList.Clear();
            NotesList.Clear();
            ClearInfoPanel();

            // If you open a new project while one is already open, the ListView apparently
            // doesn't reset certain state, possibly because it's never asked to draw after
            // the list is cleared.  This results in the new project being open at the same
            // line as the previous project.  This is a little weird, so we reset it here.
            CodeListView_SetTopIndex(0);
        }

        /// <summary>
        /// Catch mouse-down events so we can treat the fourth mouse button as "back".
        /// </summary>
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.XButton1) {
                if (mMainCtrl.CanNavigateBackward()) {
                    mMainCtrl.NavigateBackward();
                }
            }
        }

        /// <summary>
        /// Sets the primary color scheme.
        /// </summary>
        /// <remarks>
        /// H/T http://www.markodevcic.com/post/changing_wpf_themes_dynamically
        /// </remarks>
        public void SetColorScheme(ColorScheme newScheme) {
            if (mColorScheme == newScheme) {
                // nothing to do
                return;
            }

            ResourceDictionary oldDict, newDict;

            if (mColorScheme == ColorScheme.Light) {
                oldDict = mLightTheme;
            } else {
                oldDict = mDarkTheme;
            }
            if (newScheme == ColorScheme.Light) {
                newDict = mLightTheme;
            } else {
                newDict = mDarkTheme;
            }
            Debug.WriteLine("Changing color scheme from " + mColorScheme + " to " + newScheme +
                " (dict count=" + Resources.MergedDictionaries.Count + ")");

            Resources.MergedDictionaries.Remove(oldDict);
            Resources.MergedDictionaries.Add(newDict);
            mColorScheme = newScheme;
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
            //Debug.WriteLine("Grid size change: " + sender);
            AppSettings.Global.Dirty = true;
        }
        private void ColumnWidthChanged(object sender, EventArgs e) {
            //Debug.WriteLine("Column width change " + sender);
            AppSettings.Global.Dirty = true;
            UpdateLongCommentWidth();
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

        private void UpdateLongCommentWidth() {
            GridView gv = (GridView)(codeListView.View);
            double totalWidth = 0;
            for (int i = (int)MainController.CodeListColumn.Label; i < gv.Columns.Count; i++) {
                totalWidth += gv.Columns[i].ActualWidth;
            }
            LongCommentWidth = totalWidth;
            //Debug.WriteLine("Long comment width: " + LongCommentWidth);
        }

        #endregion Window placement

        #region Column widths

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
                Debug.WriteLine("Unable to deserialize widths for GridView: " + ex.Message);
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
                Debug.WriteLine("Unable to deserialize widths for " + dg.Name + ": " + ex.Message);
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

        private static string[] sSampleStrings = {
            "+000000",         // Offset
            "00/0000",         // Address
            "00 00 00 00.",    // Bytes (optional spaces or ellipsis, but not both)
            "00000000 0",      // Flags
            "######",          // Attributes
            "MMMMMMMMM",       // Label (9 chars)
            "MMMMMMM",         // Opcode
            "MMMMMMMMMMMMM",   // Operand
            "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM"   // Comment (50 chars)
        };

        /// <summary>
        /// Computes the default code list column widths, using the currently configured
        /// code list font.
        /// </summary>
        /// <returns></returns>
        public int[] GetDefaultCodeListColumnWidths() {
            // Fudge factor, in DIPs.  This is necessary because the list view style applies
            // a margin to the column border.
            const double FUDGE = 14.0;

            GridView gv = (GridView)codeListView.View;
            int[] widths = new int[gv.Columns.Count];
            Debug.Assert(widths.Length == (int)MainController.CodeListColumn.COUNT);
            Debug.Assert(widths.Length == sSampleStrings.Length);

            Typeface typeface = new Typeface(codeListView.FontFamily, codeListView.FontStyle,
                codeListView.FontWeight, codeListView.FontStretch);
            //Debug.WriteLine("Default column widths (FUDGE=" + FUDGE + "):");
            for (int i = 0; i < widths.Length; i++) {
                 double strLen = Helper.MeasureStringWidth(sSampleStrings[i],
                    typeface, codeListView.FontSize);
                widths[i] = (int)Math.Round(strLen + FUDGE);
                //Debug.WriteLine(" " + i + ":" + widths[i] + " " + sSampleStrings[i]);
            }
            return widths;
        }

        #endregion Column widths


        #region Selection management

        private void CodeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //DateTime startWhen = DateTime.Now;

            // Update the selected-item bitmap.
            CodeDisplayList.SelectedIndices.SelectionChanged(e);

            // Notify MainController that the selection has changed.
            mMainCtrl.SelectionChanged();

            // Don't try to call CodeDisplayList.SelectedIndices.DebugValidateSelectionCount()
            // here.  Events arrive while pieces are still moving.

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
        /// Selects a range of values.  Clears the previous selection.
        /// </summary>
        /// <param name="start">First line to select.</param>
        /// <param name="count">Number of lines to select.</param>
        public void CodeListView_SelectRange(int start, int count) {
            Debug.Assert(start >= 0 && start < CodeDisplayList.Count);
            Debug.Assert(count > 0 && start + count <= CodeDisplayList.Count);

            CodeListView_DeselectAll();

            if (count == 1) {
                //codeListView.SelectedItems.Add(CodeDisplayList[start]);

                // Special handling for single-item selection, for the benefit of Shift+F3.  If
                // we're in multi-select mode, and we select an item while the shift key is
                // down, the control will do a range select instead (as if you shift-clicked).
                // We work around this by temporarily switching to single-select mode.
                //
                // This could cause problems if we wanted to select multiple single lines.
                //
                // NOTE: this causes a selection-changed event, which can cause problems
                // if something tries to fiddle with SelectedItems (you can only do that
                // when in multi-select mode) instead of SelectedItem.  I tried to mitigate this
                // by setting the selection twice, once in multi-select mode, so that the
                // selection is unlikely to change, but that restored the Shift+F3 problem.
                //
                // (To repro problem: double-clicking a line in the message log about
                // a reference to a non-existent symbol associated with a self-referential line
                // blows things up... see test 2010.)
                codeListView.SelectionMode = SelectionMode.Single;
                codeListView.SelectedItem = CodeDisplayList[start];
                codeListView.SelectionMode = SelectionMode.Extended;
                return;
            }

            DisplayList.FormattedParts[] tmpArray = new DisplayList.FormattedParts[count];
            for (int index = 0; index < count; index++) {
                tmpArray[index] = CodeDisplayList[start + index];
            }
            listViewSetSelectedItems.Invoke(codeListView, new object[] { tmpArray });
        }

        /// <summary>
        /// Sets the code list selection to match the selection bitmap.
        /// </summary>
        /// <param name="sel">Selection bitmap.</param>
        public void CodeListView_SetSelection(DisplayListSelection sel) {
            // Time required increases non-linearly.  Quick test:
            //   50K: 10 seconds, 20K: 1.6 sec, 10K: 0.6 sec, 5K: 0.2 sec
            const int MAX_SEL_COUNT = 5000;

            // In the current implementation, a large (500K) list can take a couple of
            // seconds to restore a single-line selection if the selected item is near
            // the bottom of the list.

            TaskTimer timer = new TaskTimer();
            timer.StartTask("TOTAL");

            try {
                timer.StartTask("Clear");
                // The caller will clear the DisplayListSelection before calling here, so we
                // need to clear the ListView selection to match, even if we're about to call
                // SelectAll.  If we don't, the SelectAll() call won't generate the necessary
                // events, and our DisplayListSelection will get out of sync.
                codeListView.SelectedItems.Clear();
                timer.EndTask("Clear");

                if (sel.IsAllSelected()) {
                    Debug.WriteLine("SetSelection: re-selecting all items");
                    timer.StartTask("SelectAll");
                    codeListView.SelectAll();
                    timer.EndTask("SelectAll");
                    return;
                }

                if (sel.Count > MAX_SEL_COUNT) {
                    // Too much for WPF ListView -- only restore the first item.
                    Debug.WriteLine("SetSelection: not restoring (" + sel.Count + " items)");
                    codeListView.SelectedItems.Add(CodeDisplayList[sel.GetFirstSelectedIndex()]);
                    return;
                }

                Debug.WriteLine("SetSelection: selecting " + sel.Count + " of " +
                    CodeDisplayList.Count);

                // Note: if you refresh the display list with F5, the selection will be lost.  This
                // appears to be a consequence of hitting a key -- changing from the built-in
                // "Refresh" command to a locally defined "Re-analyze" command bound to F6 didn't
                // change the behavior.  Selecting "re-analyze" from the DEBUG menu doesn't lose
                // the selection.

                timer.StartTask("tmpArray " + sel.Count);
                DisplayList.FormattedParts[] tmpArray = new DisplayList.FormattedParts[sel.Count];
                int ai = 0;
                foreach (int listIndex in sel) {
                    tmpArray[ai++] = CodeDisplayList[listIndex];
                }
                timer.EndTask("tmpArray " + sel.Count);

                // Use a reflection call to provide the full set.  This is much faster than
                // adding the items one at a time to SelectedItems.  (For one thing, it only
                // invokes the SelectionChanged method once.)
                timer.StartTask("Invoke");
                listViewSetSelectedItems.Invoke(codeListView, new object[] { tmpArray });
                timer.EndTask("Invoke");
            } finally {
                timer.EndTask("TOTAL");
                //timer.DumpTimes("CodeListView_SetSelection");
            }
        }

        public void CodeListView_DebugValidateSelectionCount() {
            Debug.Assert(CodeDisplayList.SelectedIndices.DebugValidateSelectionCount(
                codeListView.SelectedItems.Count));
        }

        /// <summary>
        /// Sets the focus to the ListViewItem identified by SelectedIndex.  This must be done
        /// when the ItemContainerGenerator's StatusChanged event fires.
        /// </summary>
        /// <remarks>
        /// Sample steps to reproduce problem:
        ///  1. select note
        ///  2. delete note
        ///  3. select nearby line
        ///  4. edit > undo
        ///  5. hit the down-arrow key
        ///
        /// Without this event handler, the list jumps to line zero.  Apparently the keyboard
        /// navigation is not based on which element(s) are selected.
        ///
        /// The original article was dealing with a different problem, where you'd have to hit
        /// the down-arrow twice to make it move the first time, because the focus was on the
        /// control rather than an item.  The same fix seems to apply for this issue as well.
        ///
        /// From http://cytivrat.blogspot.com/2011/05/selecting-first-item-in-wpf-listview.html
        ///
        /// Unfortunately, grabbing focus like this causes problems with the GridSplitters.  As
        /// soon as the splitter start to move, the ListView grabs focus and prevents them from
        /// moving more than a few pixels.  The workaround is to do nothing while the
        /// splitters are being moved.  This doesn't solve the problem completely, e.g. you
        /// can't move the splitters with the arrow keys by more than one step because the
        /// ListView gets a StatusChanged event and steals focus away, but at least the
        /// mouse works.
        ///
        /// Ideally we'd do something smarter with the StatusChanged event, or maybe find some
        /// way to deal with the selection-jump problem that doesn't involve the StatusChanged
        /// event, but short of a custom replacement control I don't know what that would be.
        /// https://stackoverflow.com/q/58652064/294248
        /// </remarks>
        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e) {
            if (mIsSplitterBeingDragged) {
                return;
            }
            if (codeListView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
                int index = codeListView.SelectedIndex;

                if (index >= 0) {
                    ListViewItem item =
                        (ListViewItem)codeListView.ItemContainerGenerator.ContainerFromIndex(index);

                    if (item != null) {
                        item.Focus();
                    }
                }
            }
        }

        private bool mIsSplitterBeingDragged = false;
        private void ColSplitter_OnDragStarted(object sender, DragStartedEventArgs e) {
            mIsSplitterBeingDragged = true;
        }

        private void ColSplitter_OnDragCompleted(object sender, DragCompletedEventArgs e) {
            mIsSplitterBeingDragged = false;
        }

        public void CodeListView_SetSelectionFocus() {
            ItemContainerGenerator_StatusChanged(null, null);
        }

        /// <summary>
        /// Returns the index of the line that's currently at the top of the control.
        /// </summary>
        public int CodeListView_GetTopIndex() {
            int index = codeListView.GetTopItemIndex();
            Debug.Assert(index >= 0);
            return index;
        }

        /// <summary>
        /// Scrolls the code list so that the specified index is at the top of the control.
        /// </summary>
        /// <param name="index">Line index.</param>
        public void CodeListView_SetTopIndex(int index) {
            //Debug.WriteLine("CodeListView_SetTopIndex(" + index + "): " + CodeDisplayList[index]);

            // ScrollIntoView does the least amount of scrolling required.  This extension
            // method scrolls to the bottom, then scrolls back up to the top item.
            //
            // It looks like scroll-to-bottom (which is done directly on the ScrollViewer)
            // happens immediately, while scroll-to-item (which is done via the ListView)
            // kicks in later.  So you can't immediately query the top item to see where
            // we were moved to.
            //codeListView.ScrollToTopItem(CodeDisplayList[index]);

            // This works much better.
            codeListView.ScrollToIndex(index);
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
            if (isSelected && codeListView.SelectionMode != SelectionMode.Single) {
                if (codeListView.SelectionMode != SelectionMode.Single) {
                    Debug.WriteLine("HEY: hit unhappy single-select case");
                    codeListView.SelectedIndex = -1;
                } else {
                    codeListView.SelectedItems.Remove(CodeDisplayList[index]);
                }
            }
            CodeDisplayList[index] = newParts;
            if (isSelected) {
                if (codeListView.SelectionMode == SelectionMode.Single) {
                    codeListView.SelectedIndex = index;
                } else {
                    codeListView.SelectedItems.Add(newParts);
                }
            }
        }

        /// <summary>
        /// Ensures the the code ListView control has input focus.
        /// </summary>
        public void CodeListView_Focus() {
            codeListView.Focus();
        }

        #endregion Selection management


        #region Can-execute handlers

        /// <summary>
        /// Returns true if the project is open.
        /// </summary>
        /// <returns></returns>
        private bool IsProjectOpen() {
            return mMainCtrl != null && mMainCtrl.IsProjectOpen;
        }

        /// <summary>
        /// Returns true if the project is open.  Intended for use in XAML CommandBindings.
        /// </summary>
        private void IsProjectOpen(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen();
        }

        private void CanCreateLocalVariableTable(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanCreateLocalVariableTable();
        }

        private void CanDeleteMlc(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanDeleteMlc();
        }

        private void CanEditAddress(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditAddress();
        }

        private void CanEditComment(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditComment();
        }

        private void CanEditDataBank(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditDataBank();
        }

        private void CanEditLabel(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditLabel();
        }

        private void CanEditLocalVariableTable(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditLocalVariableTable();
        }

        private void CanEditLongComment(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditLongComment();
        }

        private void CanEditNote(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditNote();
        }

        private void CanEditOperand(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditOperand();
        }

        private void CanEditProjectSymbol(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditProjectSymbol();
        }

        private void CanEditStatusFlags(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditStatusFlags();
        }

        private void CanEditVisualizationSet(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanEditVisualizationSet();
        }

        private void CanFormatAsWord(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanFormatAsWord();
        }

        private void CanFormatAddressTable(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanFormatAddressTable();
        }

        private void CanHintAsCodeEntryPoint(object sender, CanExecuteRoutedEventArgs e) {
            if (!IsProjectOpen()) {
                e.CanExecute = false;
                return;
            }
            MainController.EntityCounts counts = mMainCtrl.SelectionAnalysis.mEntityCounts;
            e.CanExecute = (counts.mDataLines > 0 || counts.mCodeLines > 0) &&
                (counts.mDataHints != 0 || counts.mInlineDataHints != 0 || counts.mNoHints != 0);
        }
        private void CanHintAsDataStart(object sender, CanExecuteRoutedEventArgs e) {
            if (!IsProjectOpen()) {
                e.CanExecute = false;
                return;
            }
            MainController.EntityCounts counts = mMainCtrl.SelectionAnalysis.mEntityCounts;
            e.CanExecute = (counts.mDataLines > 0 || counts.mCodeLines > 0) &&
                (counts.mCodeHints != 0 || counts.mInlineDataHints != 0 || counts.mNoHints != 0);
        }
        private void CanHintAsInlineData(object sender, CanExecuteRoutedEventArgs e) {
            if (!IsProjectOpen()) {
                e.CanExecute = false;
                return;
            }
            MainController.EntityCounts counts = mMainCtrl.SelectionAnalysis.mEntityCounts;
            e.CanExecute = (counts.mDataLines > 0 || counts.mCodeLines > 0) &&
                (counts.mCodeHints != 0 || counts.mDataHints != 0 || counts.mNoHints != 0);
        }
        private void CanRemoveHints(object sender, CanExecuteRoutedEventArgs e) {
            if (!IsProjectOpen()) {
                e.CanExecute = false;
                return;
            }
            MainController.EntityCounts counts = mMainCtrl.SelectionAnalysis.mEntityCounts;
            e.CanExecute = (counts.mDataLines > 0 || counts.mCodeLines > 0) &&
                (counts.mCodeHints != 0 || counts.mDataHints != 0 || counts.mInlineDataHints != 0);
        }

        private void CanJumpToOperand(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanJumpToOperand();
        }

        private void CanSaveProject(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && !mMainCtrl.IsProjectReadOnly;
        }

        private void CanToggleSingleByteFormat(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanToggleSingleByteFormat();
        }

        private void CanNavigateBackward(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanNavigateBackward();
        }
        private void CanNavigateForward(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanNavigateForward();
        }

        private void CanRedo(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanRedo();
        }
        private void CanUndo(object sender, CanExecuteRoutedEventArgs e) {
            e.CanExecute = IsProjectOpen() && mMainCtrl.CanUndo();
        }

        #endregion Can-execute handlers


        #region Command handlers

        private void AboutCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ShowAboutBox();
        }

        private void AssembleCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.AssembleProject();
        }

        private void CloseCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (!mMainCtrl.CloseProject()) {
                Debug.WriteLine("Close canceled");
            }
        }

        private void ConcatenateFilesCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ConcatenateFiles();
        }

        private void ConvertOmfCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ConvertOmf();
        }

        private void CreateLocalVariableTableCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.CreateLocalVariableTable();
        }

        private void CopyCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.CopyToClipboard();
        }

        private void DeleteMlcCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.DeleteMlc();
        }

        private void EditAddressCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditAddress();
        }

        private void EditAppSettingsCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditAppSettings();
        }

        private void EditCommentCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditComment();
        }

        private void EditDataBankCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditDataBank();
        }

        private void EditHeaderCommentCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditHeaderComment();
        }

        private void EditLabelCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditLabel();
        }

        private void EditLocalVariableTableCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditLocalVariableTable();
        }

        private void EditLongCommentCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditLongComment();
        }

        private void EditNoteCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditNote();
        }

        private void EditOperandCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditOperand();
        }

        private void EditProjectPropertiesCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditProjectProperties(WpfGui.EditProjectProperties.Tab.Unknown);
        }

        private void EditProjectPropertiesSymbolsCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditProjectProperties(WpfGui.EditProjectProperties.Tab.ProjectSymbols);
        }

        private void EditProjectSymbolCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditProjectSymbol(MainController.CodeListColumn.Label);
        }

        private void EditStatusFlagsCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditStatusFlags();
        }

        private void EditVisualizationSetCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.EditVisualizationSet();
        }

        private void ExitCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Close();
        }

        private void ExportCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.Export();
        }

        private void FindCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.Find();
        }

        private void FindNextCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.FindNext();
        }

        private void FindPreviousCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.FindPrevious();
        }

        private void FormatAsWordCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.FormatAsWord();
        }

        private void FormatAddressTableCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.FormatAddressTable();
        }

        private void GotoCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.Goto();
        }

        private void GotoLastChangeCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.GotoLastChange();
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

        private void JumpToOperandCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.JumpToOperand();
        }

        private void NavigateBackwardCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.NavigateBackward();
        }

        private void NavigateForwardCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.NavigateForward();
        }

        private void NewProjectCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.NewProject();
        }

        private void OpenCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.OpenProject();
        }

        private void RecentProjectCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            int recentIndex;
            if (e.Parameter is int) {
                recentIndex = (int)e.Parameter;
            } else if (e.Parameter is string) {
                recentIndex = int.Parse((string)e.Parameter);
            } else {
                throw new Exception("Bad parameter: " + e.Parameter);
            }
            if (recentIndex < 0 || recentIndex >= MainController.MAX_RECENT_PROJECTS) {
                throw new Exception("Bad parameter: " + e.Parameter);
            }

            Debug.WriteLine("Recent project #" + recentIndex);
            mMainCtrl.OpenRecentProject(recentIndex);
        }

        private void RedoCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.RedoChanges();
        }

        private void ReloadExternalFilesCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ReloadExternalFiles();
        }

        private void RemoveHintsCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.WriteLine("remove hints");
            mMainCtrl.MarkAsType(CodeAnalysis.TypeHint.NoHint, false);
        }

        private void SaveCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.SaveProject();
        }

        private void SaveAsCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.SaveProjectAs();
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

        private void ShowFileHexDumpCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ShowFileHexDump();
        }

        private void ShowHexDumpCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ShowHexDump();
        }

        private void SliceFilesCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.SliceFiles();
        }

        private void ToggleAsciiChartCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ToggleAsciiChart();
        }

        private void ToggleDataScanCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ToggleDataScan();
        }

        private void ToggleInstructionChartCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ToggleInstructionChart();
        }

        private void ToggleSingleByteFormatCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.ToggleSingleByteFormat();
        }

        private void UndoCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.UndoChanges();
        }

        private void Debug_ApplesoftToHtmlCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ApplesoftToHtml();
        }

        private void Debug_ApplyEditCommandsCmd_Executed(object sender,
                ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ApplyEditCommands();
        }

        private void Debug_ApplyPlatformSymbolsCmd_Executed(object sender,
                ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ApplyPlatformSymbols();
        }

        private void Debug_ExportEditCommandsCmd_Executed(object sender,
                ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ExportEditCommands();
        }

        private void Debug_ExtensionScriptInfoCmd_Executed(object sender,
            ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ExtensionScriptInfo();
        }

        private void Debug_RebootSecuritySandboxCmd_Executed(object sender,
                ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_RebootSecuritySandbox();
        }

        private void Debug_RefreshCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_Refresh();
        }

        private void Debug_ShowAnalysisTimersCmd_Executed(object sender,
                ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ShowAnalysisTimers();
        }

        private void Debug_ShowAnalyzerOutputCmd_Executed(object sender,
                ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ShowAnalyzerOutput();
        }

        private void Debug_ShowUndoRedoHistoryCmd_Executed(object sender,
                ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ShowUndoRedoHistory();
        }

        private void Debug_SourceGenerationTestsCmd_Executed(object sender,
                ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_RunSourceGenerationTests();
        }

        private void Debug_ToggleCommentRulersCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ToggleCommentRulers();
        }

        private void Debug_ToggleKeepAliveHackCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ToggleKeepAliveHack();
        }

        private void Debug_ToggleSecuritySandboxCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            mMainCtrl.Debug_ToggleSecuritySandbox();
        }

        #endregion Command handlers

        #region Misc

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

        private void RecentProjectsMenu_SubmenuOpened(object sender, RoutedEventArgs e) {
            MenuItem recents = (MenuItem)sender;
            recents.Items.Clear();

            //Debug.WriteLine("COUNT is " + mMainCtrl.RecentProjectPaths.Count);
            if (mMainCtrl.RecentProjectPaths.Count == 0) {
                MenuItem mi = new MenuItem();
                mi.Header = Res.Strings.PARENTHETICAL_NONE;
                recents.Items.Add(mi);
            } else {
                for (int i = 0; i < mMainCtrl.RecentProjectPaths.Count; i++) {
                    MenuItem mi = new MenuItem();
                    mi.Header = string.Format("{0}: {1}", i + 1, mMainCtrl.RecentProjectPaths[i]);
                    mi.Command = recentProjectCmd.Command;
                    mi.CommandParameter = i;
                    recents.Items.Add(mi);
                }
            }
        }

        public Visibility RecentProjectVisibility1 {
            get { return mRecentProjectVisibility1; }
            set { mRecentProjectVisibility1 = value; OnPropertyChanged(); }
        }
        private Visibility mRecentProjectVisibility1;

        public string RecentProjectName1 {
            get { return mRecentProjectName1; }
            set { mRecentProjectName1 = value; OnPropertyChanged(); }
        }
        private string mRecentProjectName1;

        public string RecentProjectPath1 {
            get { return mRecentProjectPath1; }
            set { mRecentProjectPath1 = value; OnPropertyChanged(); }
        }
        private string mRecentProjectPath1;

        public Visibility RecentProjectVisibility2 {
            get { return mRecentProjectVisibility2; }
            set { mRecentProjectVisibility2 = value; OnPropertyChanged(); }
        }
        private Visibility mRecentProjectVisibility2;

        public string RecentProjectName2 {
            get { return mRecentProjectName2; }
            set { mRecentProjectName2 = value; OnPropertyChanged(); }
        }
        private string mRecentProjectName2;

        public string RecentProjectPath2 {
            get { return mRecentProjectPath2; }
            set { mRecentProjectPath2 = value; OnPropertyChanged(); }
        }
        private string mRecentProjectPath2;

        public void UpdateRecentLinks() {
            List<string> pathList = mMainCtrl.RecentProjectPaths;

            if (pathList.Count >= 1) {
                RecentProjectPath1 = pathList[0];
                RecentProjectName1 = Path.GetFileName(pathList[0]);
                RecentProjectVisibility1 = Visibility.Visible;
            } else {
                RecentProjectVisibility1 = Visibility.Collapsed;
            }
            if (pathList.Count >= 2) {
                RecentProjectPath2 = pathList[1];
                RecentProjectName2 = Path.GetFileName(pathList[1]);
                RecentProjectVisibility2 = Visibility.Visible;
            } else {
                RecentProjectVisibility2 = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Update menu items when the "edit" menu is opened.
        /// </summary>
        private void EditMenu_SubmenuOpened(object sender, RoutedEventArgs e) {
            // Set the checkbox on the "Toggle Data Scan" item.
            //
            // I initially bound a property to the menu item's IsChecked, but that caused
            // us to get "set" calls when the menu was selected.  I want to get activity
            // through ICommand, not property set, so things are consistent for menus and
            // keyboard shortcuts.  So we just drive the checkbox manually.  I don't know
            // if there's a better way.
            //
            // The project's AnalyzeUncategorizedData property can be set in various ways
            // (project property dialog, undo, redo), so we want to query it when we need
            // it rather than try to push changes around.
            toggleDataScanMenuItem.IsChecked = mMainCtrl.IsAnalyzeUncategorizedDataEnabled;
        }
        private void ToolsMenu_SubmenuOpened(object sender, RoutedEventArgs e) {
            toggleAsciiChartMenuItem.IsChecked = mMainCtrl.IsAsciiChartOpen;
            toggleInstructionChartMenuItem.IsChecked = mMainCtrl.IsInstructionChartOpen;
        }
        private void DebugMenu_SubmenuOpened(object sender, RoutedEventArgs e) {
            debugCommentRulersMenuItem.IsChecked = MultiLineComment.DebugShowRuler;
            debugKeepAliveHackMenuItem.IsChecked = !Sandbox.ScriptManager.UseKeepAliveHack;
            debugSecuritySandboxMenuItem.IsChecked = mMainCtrl.UseMainAppDomainForPlugins;
            debugAnalysisTimersMenuItem.IsChecked = mMainCtrl.IsDebugAnalysisTimersOpen;
            debugAnalyzerOutputMenuItem.IsChecked = mMainCtrl.IsDebugAnalyzerOutputOpen;
            debugUndoRedoHistoryMenuItem.IsChecked = mMainCtrl.IsDebugUndoRedoHistoryOpen;
        }

        #endregion Misc


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
            if (!referencesGrid.GetClickRowColItem(e, out int unusedRow, out int unusedCol,
                    out object item)) {
                // Header or empty area; ignore.
                return;
            }
            ReferencesListItem rli = (ReferencesListItem)item;

            // Jump to the offset, then shift the focus back to the code list.
            mMainCtrl.GoToLocation(new NavStack.Location(rli.OffsetValue, 0, false),
                MainController.GoToMode.JumpToCodeData, true);
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
                if (backColor == CommonWPF.Helper.ZeroColor) {
                    // Force this to white, so we can always use black text.  This is not ideal.
                    backColor = Colors.White;
                }
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
            if (!notesGrid.GetClickRowColItem(e, out int unusedRow, out int unusedCol,
                    out object item)) {
                // Header or empty area; ignore.
                return;
            }
            NotesListItem nli = (NotesListItem)item;

            // Jump to the offset, then shift the focus back to the code list.
            mMainCtrl.GoToLocation(new NavStack.Location(nli.OffsetValue, 0, true),
                MainController.GoToMode.JumpToNote, true);
            codeListView.Focus();
        }

        #endregion Notes panel

        #region Symbols panel

        //
        // Symbols list filter options.
        //

        private bool mSymFilterUserLabels;
        public bool SymFilterUserLabels {
            get { return mSymFilterUserLabels; }
            set {
                mSymFilterUserLabels = value;
                AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_USER, value);
                SymbolsListFilterChanged();
                OnPropertyChanged();
            }
        }
        private bool mSymFilterNonUniqueLabels;
        public bool SymFilterNonUniqueLabels {
            get { return mSymFilterNonUniqueLabels; }
            set {
                mSymFilterNonUniqueLabels = value;
                AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_NON_UNIQUE, value);
                SymbolsListFilterChanged();
                OnPropertyChanged();
            }
        }
        private bool mSymFilterProjectSymbols;
        public bool SymFilterProjectSymbols {
            get { return mSymFilterProjectSymbols; }
            set {
                mSymFilterProjectSymbols = value;
                AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_PROJECT, value);
                SymbolsListFilterChanged();
                OnPropertyChanged();
            }
        }
        private bool mSymFilterPlatformSymbols;
        public bool SymFilterPlatformSymbols {
            get { return mSymFilterPlatformSymbols; }
            set {
                mSymFilterPlatformSymbols = value;
                AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_PLATFORM, value);
                SymbolsListFilterChanged();
                OnPropertyChanged();
            }
        }
        private bool mSymFilterAutoLabels;
        public bool SymFilterAutoLabels {
            get { return mSymFilterAutoLabels; }
            set {
                mSymFilterAutoLabels = value;
                AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_AUTO, value);
                SymbolsListFilterChanged();
                OnPropertyChanged();
            }
        }
        private bool mSymFilterAddresses;
        public bool SymFilterAddresses {
            get { return mSymFilterAddresses; }
            set {
                mSymFilterAddresses = value;
                AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_ADDR, value);
                SymbolsListFilterChanged();
                OnPropertyChanged();
            }
        }
        private bool mSymFilterConstants;
        public bool SymFilterConstants {
            get { return mSymFilterConstants; }
            set {
                mSymFilterConstants = value;
                AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_CONST, value);
                SymbolsListFilterChanged();
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Symbols list DataGrid item.
        /// </summary>
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
            if (!symbolsGrid.GetClickRowColItem(e, out int unusedRow, out int unusedCol,
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
            if ((SymFilterUserLabels != true && sli.Sym.SymbolSource == Symbol.Source.User) ||
                (SymFilterNonUniqueLabels != true && sli.Sym.IsNonUnique) ||
                (SymFilterProjectSymbols != true && sli.Sym.SymbolSource == Symbol.Source.Project) ||
                (SymFilterPlatformSymbols != true && sli.Sym.SymbolSource == Symbol.Source.Platform) ||
                (SymFilterAutoLabels != true && sli.Sym.SymbolSource == Symbol.Source.Auto) ||
                (SymFilterAddresses != true && !sli.Sym.IsConstant) ||
                (SymFilterConstants != true && sli.Sym.IsConstant) ||
                sli.Sym.IsVariable)
            {
                e.Accepted = false;
            } else {
                e.Accepted = true;
            }
        }

        /// <summary>
        /// Refreshes the symbols list when a filter option changes.
        /// </summary>
        private void SymbolsListFilterChanged() {
            // This delightfully obscure call causes the list to refresh.  See
            // https://docs.microsoft.com/en-us/dotnet/framework/wpf/controls/how-to-group-sort-and-filter-data-in-the-datagrid-control
            CollectionViewSource.GetDefaultView(symbolsGrid.ItemsSource).Refresh();
        }

        /// <summary>
        /// Handles a Sorting event.  We want to do a secondary sort on Name when one of the
        /// other columns is the primary sort key.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/a/2130557/294248
        /// </remarks>
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
        /// Text for the line number / description section.
        /// </summary>
        public string InfoLineDescrText {
            get { return mInfoLineDescrText; }
            set { mInfoLineDescrText = value; OnPropertyChanged(); }
        }
        private string mInfoLineDescrText;

        /// <summary>
        /// Text for the offset, shown only in debug builds.
        /// </summary>
        public string InfoOffsetText {
            get { return mInfoOffsetText; }
            set { mInfoOffsetText = value; OnPropertyChanged(); }
        }
        private string mInfoOffsetText;

        /// <summary>
        /// Text for the label description.
        /// </summary>
        public string InfoLabelDescrText {
            get { return mInfoLabelDescrText; }
            set { mInfoLabelDescrText = value; OnPropertyChanged(); }
        }
        private string mInfoLabelDescrText;

        public SolidColorBrush InfoFormatBoxBrush {
            get { return mInfoFormatBoxBrush; }
            set { mInfoFormatBoxBrush = value; OnPropertyChanged(); }
        }
        private SolidColorBrush mInfoFormatBoxBrush = Brushes.Green;

        public bool InfoFormatShowDashes {
            get { return mInfoFormatShowDashes; }
            set { mInfoFormatShowDashes = value; OnPropertyChanged(); }
        }
        private bool mInfoFormatShowDashes;

        public bool InfoFormatShowSolid {
            get { return mInfoFormatShowSolid; }
            set { mInfoFormatShowSolid = value; OnPropertyChanged(); }
        }
        private bool mInfoFormatShowSolid;

        public string InfoFormatText {
            get { return mInfoFormatText; }
            set { mInfoFormatText = value; OnPropertyChanged(); }
        }
        private string mInfoFormatText;

        public string InfoPanelDetail1 {
            get { return mInfoPanelDetail1; }
            set { mInfoPanelDetail1 = value; OnPropertyChanged(); }
        }
        private string mInfoPanelDetail1;

        public bool InfoShowDebug {
            get {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        //public string InfoPanelMonoContents {
        //    get { return mInfoPanelMonoContents; }
        //    set { mInfoPanelMonoContents = value; OnPropertyChanged(); }
        //}
        //private string mInfoPanelMonoContents;


        /// <summary>
        /// Clears the contents of the info panel.  Call this whenever the contents have
        /// been updated.
        /// </summary>
        public void ClearInfoPanel() {
            InfoLineDescrText = InfoOffsetText = InfoLabelDescrText = InfoFormatText =
                InfoPanelDetail1 = string.Empty;
            InfoFormatShowDashes = InfoFormatShowSolid = false;
        }

        #endregion Info panel

        #region Message list panel

        public class MessageListItem {
            public string Severity { get; private set; }
            public string Offset { get; private set; }
            public string Type { get; private set; }
            public string Context { get; private set; }
            public string Resolution { get; private set; }

            public int OffsetValue { get; private set; }

            public MessageListItem(string severity, int offsetValue, string offset, string type,
                    string context, string resolution) {
                Severity = severity;
                OffsetValue = offsetValue;
                Offset = offset;
                Type = type;
                Context = context;
                Resolution = resolution;
            }
        }

        public Visibility MessageListVisibility {
            get {
                bool visible = !HideMessageList && FormattedMessages.Count > 0;
                return visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private bool HideMessageList {
            get { return AppSettings.Global.GetBool(AppSettings.MAIN_HIDE_MESSAGE_WINDOW, false); }
            set {
                AppSettings.Global.SetBool(AppSettings.MAIN_HIDE_MESSAGE_WINDOW, value);
                OnPropertyChanged("MessageListVisibility");
            }
        }

        private string mMessageStatusText;
        public string MessageStatusText {
            get { return mMessageStatusText; }
            set { mMessageStatusText = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// ItemsSource for DataGrid.
        /// </summary>
        public ObservableCollection<MessageListItem> FormattedMessages { get; private set; } =
            new ObservableCollection<MessageListItem>();

        private void MessageStatusButton_Click(object sender, RoutedEventArgs e) {
            HideMessageList = false;
        }

        private void HideMessageList_Click(object sender, RoutedEventArgs e) {
            HideMessageList = true;
        }

        private void MessageGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (!messageGrid.GetClickRowColItem(e, out int unusedRow, out int unusedCol,
                    out object item)) {
                // Header or empty area; ignore.
                return;
            }
            MessageListItem mli = (MessageListItem)item;

            // Jump to the offset, then shift the focus back to the code list.
            mMainCtrl.GoToLocation(new NavStack.Location(mli.OffsetValue, 0, false),
                MainController.GoToMode.JumpToCodeData, true);
            codeListView.Focus();
        }

        /// <summary>
        /// Regenerates the contents of the message list.
        /// </summary>
        /// <param name="list">Message list.</param>
        /// <param name="formatter">Format object.</param>
        public void UpdateMessageList(MessageList list, Asm65.Formatter formatter) {
            FormattedMessages.Clear();
            list.Sort();

            int warnErrCount = 0;
            foreach (MessageList.MessageEntry entry in list) {
                FormattedMessages.Add(MessageList.FormatMessage(entry, formatter));
                if (entry.Severity != MessageList.MessageEntry.SeverityLevel.Info) {
                    warnErrCount++;
                }
            }

            if (warnErrCount == 0) {
                if (FormattedMessages.Count == 1) {
                    string fmt = (string)FindResource("str_MessageSingularFmt");
                    MessageStatusText = string.Format(fmt, FormattedMessages.Count);
                } else {
                    string fmt = (string)FindResource("str_MessagePluralFmt");
                    MessageStatusText = string.Format(fmt, FormattedMessages.Count);
                }
            } else {
                if (FormattedMessages.Count == 1) {
                    string fmt = (string)FindResource("str_MessageSingularWarningFmt");
                    MessageStatusText = string.Format(fmt, FormattedMessages.Count, warnErrCount);
                } else {
                    string fmt = (string)FindResource("str_MessagePluralWarningFmt");
                    MessageStatusText = string.Format(fmt, FormattedMessages.Count, warnErrCount);
                }
            }

            OnPropertyChanged("MessageListVisibility");
        }

        #endregion Message list panel
    }
}
