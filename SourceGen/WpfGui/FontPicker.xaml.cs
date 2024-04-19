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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using CommonWPF;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Simple font picker.
    /// </summary>
    public partial class FontPicker : Window {
        // Used as ItemsSource for the ListBox.
        public List<FontFamily> MonoFontFamilies { get; private set; }

        // This is bound directly to the ListBox SelectedItem property.
        public FontFamily SelectedFamily { get; set; }

        // Pulled out of combo box.
        public int SelectedSize { get; private set; }


        public FontPicker(Window owner, string initialFamily, int initialSize) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            GenerateMonoFontList(initialFamily);

            int selIndex = -1;
            string sizeStr = initialSize.ToString();
            for (int i = 0; i < sizeComboBox.Items.Count; i++) {
                ComboBoxItem item = (ComboBoxItem)sizeComboBox.Items[i];
                if (sizeStr.Equals(item.Content)) {
                    selIndex = i;
                    break;
                }
            }
            if (selIndex < 0) {
                // Size is not one of the standard combo box items.
                sizeComboBox.Text = initialSize.ToString();
            }
            sizeComboBox.SelectedIndex = selIndex;
        }

        /// <summary>
        /// Populates the MonoFontFamilies list, by finding mono-spaced fonts in the system
        /// font set.
        /// </summary>
        /// <param name="initialFamily">Name of family to select.</param>
        private void GenerateMonoFontList(string initialFamily) {
            SortedList<string, FontFamily> tmpList = new SortedList<string, FontFamily>();
            foreach (Typeface typ in Fonts.SystemTypefaces) {
                if (typ.Style != FontStyles.Normal) {
                    continue;
                }
                if (typ.Weight != FontWeights.Normal) {
                    continue;
                }
                if (typ.Stretch != FontStretches.Normal) {
                    continue;
                }
                string familyName = typ.FontFamily.ToString();
                if (string.IsNullOrEmpty(familyName)) {
                    continue;
                }

                // Unscientific and prone to false-positives.  The only alternative seems
                // to be a PInvoke approach using System.Drawing, which is meant for WinForms
                // (https://stackoverflow.com/a/225027/294248).  Feels a bit weird, but fonts
                // should be equally mono-spaced regardless of system.  The System.Drawing
                // FontFamily would be mapped back to System.Windows.Media FontFamily by name.
                const string SAMPLE_STRING1 = "M#w";
                const string SAMPLE_STRING2 = "i.|";
                if (Helper.MeasureStringWidth(SAMPLE_STRING1, typ, 10) !=
                        Helper.MeasureStringWidth(SAMPLE_STRING2, typ, 10)) {
                    continue;
                }

                tmpList[familyName] = typ.FontFamily;

                if (familyName.Equals(initialFamily)) {
                    SelectedFamily = typ.FontFamily;
                }
            }

            MonoFontFamilies = new List<FontFamily>();
            foreach (FontFamily fam in tmpList.Values) {
                MonoFontFamilies.Add(fam);
            }

            // Select the first entry if nothing else is selected.
            if (SelectedFamily == null && MonoFontFamilies.Count > 0) {
                SelectedFamily = MonoFontFamilies[0];
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            fontFamilyListBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            ComboBoxItem item = (ComboBoxItem)sizeComboBox.SelectedItem;
            if (item != null) {
                SelectedSize = int.Parse((string)item.Content);
            } else {
                // Catch bad font sizes when "OK" is hit.  Not as nice as disabling the OK
                // button on bad input, but much simpler.
                try {
                    SelectedSize = int.Parse(sizeComboBox.Text);
                } catch (FormatException) {
                    SelectedSize = -1;      // trigger next test
                }
                if (SelectedSize < 3 || SelectedSize > 64) {
                    sizeErrMsg.Visibility = Visibility.Visible;
                    return;
                }
            }
            DialogResult = true;
        }
    }
}
