/*
 * Copyright 2024 faddenSoft
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
using System.Windows.Media;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Present a choice between the project file and the recovery file.
    /// </summary>
    public partial class RecoveryChoice : Window {
        /// <summary>
        /// Dialog result: true if the recovery file was selected.
        /// </summary>
        public bool UseRecoveryFile { get; private set; }

        //
        // Dialog strings.
        //

        public string ProjPathName { get; set; }
        public string ProjModWhen { get; set; }
        public string ProjLength { get; set; }
        public string RecovPathName { get; set; }
        public string RecovModWhen { get; set; }
        public string RecovLength { get; set; }


        public RecoveryChoice(Window parent, string projPathName, string recoveryPathName) {
            InitializeComponent();
            Owner = parent;
            DataContext = this;

            string modWhenStr, lenStr;
            GetFileInfo(projPathName, out DateTime projModWhen, out modWhenStr, out lenStr);
            ProjPathName = projPathName;
            ProjModWhen = modWhenStr;
            ProjLength = lenStr;
            GetFileInfo(recoveryPathName, out DateTime recovModWhen, out modWhenStr, out lenStr);
            RecovPathName = recoveryPathName;
            RecovModWhen = modWhenStr;
            RecovLength = lenStr;

            if (projModWhen >= recovModWhen) {
                projectButton.BorderBrush = Brushes.Green;
                projectButton.BorderThickness = new Thickness(2);
            } else {
                recoveryButton.BorderBrush = Brushes.Green;
                recoveryButton.BorderThickness = new Thickness(2);
            }
        }

        /// <summary>
        /// Reads and formats some basic information about the file.
        /// </summary>
        /// <param name="pathName">Pathname to file.</param>
        /// <param name="modWhen">Result: modification date.</param>
        /// <param name="modWhenStr">Result: formatted modification date.</param>
        /// <param name="lenStr">Result: formatted file length.</param>
        private static void GetFileInfo(string pathName, out DateTime modWhen,
                out string modWhenStr, out string lenStr) {
            try {
                FileInfo fi = new FileInfo(pathName);
                modWhen = fi.LastWriteTime;
                modWhenStr = "Modified: " + modWhen.ToString("G");
                long len = fi.Length;
                if (len >= 4096) {
                    lenStr = "Length: " + (len / 1024.0).ToString("F2") + " kB";
                } else {
                    lenStr = "Length: " + len + " bytes";
                }
            } catch (Exception ex) {
                modWhenStr = "file error";
                lenStr = ex.Message;
                modWhen = DateTime.Now;
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            // Don't allow the window to be resized smaller than its initial size in width.
            MinWidth = ActualWidth;
            // Don't allow the height to be changed.
            MinHeight = ActualHeight;
            MaxHeight = ActualHeight;
        }

        private void ProjectButton_Click(object sender, RoutedEventArgs e) {
            UseRecoveryFile = false;
            DialogResult = true;
        }

        private void RecoveryButton_Click(object sender, RoutedEventArgs e) {
            UseRecoveryFile = true;
            DialogResult = true;
        }
    }
}
