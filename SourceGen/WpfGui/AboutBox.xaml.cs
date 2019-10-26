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
using System.IO;
using System.Windows;

namespace SourceGen.WpfGui {
    /// <summary>
    /// "About" dialog.
    /// </summary>
    public partial class AboutBox : Window {
        private const string LEGAL_STUFF_FILE_NAME = "LegalStuff.txt";

        /// <summary>
        /// Version string, for display.
        /// </summary>
        public string ProgramVersionString {
            get { return App.ProgramVersion.ToString(); }
        }

        /// <summary>
        /// Operating system version, for display.
        /// </summary>
        public string OsPlatform {
            get {
                return "OS: " + System.Runtime.InteropServices.RuntimeInformation.OSDescription;
            }
        }

        /// <summary>
        /// Determines whether a message about assertions is visible.
        /// </summary>
        public Visibility DebugMessageVisibility {
            get {
#if DEBUG
                return Visibility.Visible;
#else
                return Visibility.Collapsed;
#endif
            }
        }

        public AboutBox(Window owner) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            string text;
            string pathName = RuntimeDataAccess.GetPathName(LEGAL_STUFF_FILE_NAME);
            try {
                text = File.ReadAllText(pathName);
            } catch (Exception ex) {
                text = ex.ToString();
            }

            legalStuffTextBox.Text = text;
        }

        private void WebSiteButton_Click(object sender, RoutedEventArgs e) {
            CommonUtil.ShellCommand.OpenUrl("https://6502bench.com/");
        }
    }
}
