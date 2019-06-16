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
using System.Windows;

namespace SourceGenWPF.ProjWin {
    /// <summary>
    /// Prompt the user before discarding changes.
    /// 
    /// Dialog result will be false if the user cancels out.  Otherwise, the result will be
    /// true, with the selected option in UserChoice.
    /// </summary>
    public partial class DiscardChanges : Window {
        public enum Choice {
            Unknown = 0,
            SaveAndContinue,
            DiscardAndContinue
        }
        public Choice UserChoice { get; private set; }

        public DiscardChanges(Window owner) {
            InitializeComponent();
            Owner = owner;
        }

        // TODO:
        // https://stackoverflow.com/questions/817610/wpf-and-initial-focus
        // FocusManager.FocusedElement={Binding ElementName=cancelButton}"

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            UserChoice = Choice.SaveAndContinue;
            DialogResult = true;
        }

        private void DontSaveButton_Click(object sender, RoutedEventArgs e) {
            UserChoice = Choice.DiscardAndContinue;
            DialogResult = true;
        }
    }
}
