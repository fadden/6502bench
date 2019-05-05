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
using System.Diagnostics;
using System.Linq;
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

namespace SourceGenWPF.ProjWin {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public string ProgramVersionString {
            get { return App.ProgramVersion.ToString(); }
        }

        private MainController mUI;

        public MainWindow() {
            InitializeComponent();

            mUI = new MainController();
        }

        private void AssembleCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            // test
            Debug.WriteLine("assembling");
        }

        private void RecentProject_Executed(object sender, ExecutedRoutedEventArgs e) {
            if (!int.TryParse((string)e.Parameter, out int recentIndex) ||
                    recentIndex < 1 || recentIndex > MainController.MAX_RECENT_PROJECTS) {
                throw new Exception("Bad parameter: " + e.Parameter);
            }
            recentIndex--;

            Debug.WriteLine("Recent project #" + recentIndex);
            mUI.OpenRecentProject(recentIndex);
        }
    }
}
