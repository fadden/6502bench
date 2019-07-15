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
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SourceGenWPF.Tools.WpfGui {
    /// <summary>
    /// ASCII chart.
    /// </summary>
    public partial class AsciiChart : Window {
        public enum ChartMode {
            Unknown = 0,
            Standard,
            High
        };
        public class ChartModeItem {
            public string Name { get; private set; }
            public ChartMode Mode { get; private set; }

            public ChartModeItem(string name, ChartMode mode) {
                Name = name;
                Mode = mode;
            }
        }
        public ChartModeItem[] ChartModeItems { get; private set; }


        public AsciiChart(Window owner) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            ChartModeItems = new ChartModeItem[] {
                new ChartModeItem((string)FindResource("str_Standard"), ChartMode.Standard),
                new ChartModeItem((string)FindResource("str_High"), ChartMode.High),
            };
        }

        public void Window_Loaded(object sender, RoutedEventArgs e) {
            // Restore chart mode setting.
            ChartMode mode = (ChartMode)AppSettings.Global.GetEnum(
                AppSettings.ASCCH_MODE, typeof(ChartMode), (int)ChartMode.Standard);
            int index = 0;
            for (int i = 0; i < ChartModeItems.Length; i++) {
                if (ChartModeItems[i].Mode == mode) {
                    index = i;
                    break;
                }
            }
            chartModeComboBox.SelectedIndex = index;
            // should call UpdateControls via SelectionChanged
        }

        // Catch ESC key.
        private void Window_KeyEventHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                Close();
            }
        }

        private void ChartModeComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            UpdateControls();
        }

        private void UpdateControls() {
            ChartModeItem item = (ChartModeItem)chartModeComboBox.SelectedItem;
            if (item == null) {
                // initializing
                return;
            }

            AppSettings.Global.SetEnum(AppSettings.ASCCH_MODE, typeof(ChartMode), (int)item.Mode);

            //
            // Draw box contents.
            //
            const string hdr = "Dec Hex Chr";
            const string div = "  |  ";
            const string eol = "\r\n";

            StringBuilder sb = new StringBuilder(
                (hdr.Length * 4 + div.Length * 3 + eol.Length) * 32);
            sb.Append(hdr);
            sb.Append(div);
            sb.Append(hdr);
            sb.Append(div);
            sb.Append(hdr);
            sb.Append(div);
            sb.Append(hdr);
            sb.Append(eol);
            for (int i = 0; i < 32; i++) {
                DrawEntry(item.Mode, i, sb);
                sb.Append(div);
                DrawEntry(item.Mode, i + 32, sb);
                sb.Append(div);
                DrawEntry(item.Mode, i + 64, sb);
                sb.Append(div);
                DrawEntry(item.Mode, i + 96, sb);
                sb.Append(eol);
            }

            chartTextBox.Text = sb.ToString();
            chartTextBox.SelectionStart = sb.Length;
            chartTextBox.SelectionLength = 0;
        }

        private void DrawEntry(ChartMode mode, int val, StringBuilder sb) {
            // Format is: Dec Hex Chr
            int modVal = (mode == ChartMode.High) ? val | 0x80 : val;
            sb.AppendFormat("{0,3:D} {1,3:X2} ", modVal, modVal);
            if (val < 0x20) {
                sb.Append('^');
                sb.Append((char)(val + 0x40));
                sb.Append(' ');
            } else if (val == 0x20) {
                sb.Append("' '");
            } else if (val < 0x7f) {
                sb.Append(' ');
                sb.Append((char)val);
                sb.Append(' ');
            } else {
                sb.Append("DEL");
            }
        }
    }
}
