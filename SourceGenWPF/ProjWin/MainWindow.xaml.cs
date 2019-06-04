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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        /// <summary>
        /// </summary>
        private MainController mMainCtrl;

        private MethodInfo listViewSetSelectedItems;

        public MainWindow() {
            InitializeComponent();

            listViewSetSelectedItems = codeListView.GetType().GetMethod("SetSelectedItems",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(listViewSetSelectedItems != null);

            this.DataContext = this;

            CodeDisplayList = new DisplayList();
            codeListView.ItemsSource = CodeDisplayList;

            mMainCtrl = new MainController(this);

            AddMultiKeyGestures();

            //GridView gv = (GridView)codeListView.View;
            //gv.Columns[0].Width = 50;
        }

        private void AddMultiKeyGestures() {
            RoutedUICommand ruic;

            ruic = (RoutedUICommand)FindResource("HintAsCodeEntryPoint");
            ruic.InputGestures.Add(
                new MultiKeyInputGesture(new KeyGesture[] {
                      new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H"),
                      new KeyGesture(Key.C, ModifierKeys.Control, "Ctrl+C")
                }));
            ruic = (RoutedUICommand)FindResource("HintAsDataStart");
            ruic.InputGestures.Add(
                new MultiKeyInputGesture(new KeyGesture[] {
                      new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H"),
                      new KeyGesture(Key.D, ModifierKeys.Control, "Ctrl+D")
                }));
            ruic = (RoutedUICommand)FindResource("HintAsInlineData");
            ruic.InputGestures.Add(
                new MultiKeyInputGesture(new KeyGesture[] {
                      new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H"),
                      new KeyGesture(Key.I, ModifierKeys.Control, "Ctrl+I")
                }));
            ruic = (RoutedUICommand)FindResource("RemoveHints");
            ruic.InputGestures.Add(
                new MultiKeyInputGesture(new KeyGesture[] {
                      new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H"),
                      new KeyGesture(Key.R, ModifierKeys.Control, "Ctrl+R")
                }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            mMainCtrl.WindowLoaded();

#if DEBUG
            // Get more info on CollectionChanged events that do not agree with current
            // state of Items collection.
            PresentationTraceSources.SetTraceLevel(codeListView.ItemContainerGenerator,
                PresentationTraceLevel.High);
#endif
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

        private bool mShowCodeListView;

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

        /// <summary>
        /// Returns true if we should be showing the launch panel.
        /// (Intended for use from XAML.)
        /// </summary>
        public Visibility LaunchPanelVisibility {
            get { return mShowCodeListView ? Visibility.Hidden : Visibility.Visible; }
        }

        /// <summary>
        /// Returns true if we should be showing the code ListView.
        /// (Intended for use from XAML.)
        /// </summary>
        public Visibility CodeListVisibility {
            get { return mShowCodeListView ? Visibility.Visible : Visibility.Hidden; }
        }

        /// Version string, for display.
        /// </summary>
        public string ProgramVersionString {
            get { return App.ProgramVersion.ToString(); }
        }

        private void AssembleCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            // test
            Debug.WriteLine("assembling");
        }

        private void HintAsCodeEntryPoint_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.WriteLine("hint as code entry point");
        }

        private void HintAsDataStart_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.WriteLine("hint as data start");
        }

        private void HintAsInlineData_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.WriteLine("hint as inline data");
        }

        private void RemoveHints_Executed(object sender, ExecutedRoutedEventArgs e) {
            Debug.WriteLine("remove hints");
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

        private void CodeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //Debug.WriteLine("SEL: add " + e.AddedItems.Count + ", rem " + e.RemovedItems.Count);
        }
    }
}
