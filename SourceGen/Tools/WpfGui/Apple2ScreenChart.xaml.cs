/*
 * Copyright 2020 faddenSoft
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Asm65;

namespace SourceGen.Tools.WpfGui {
    /// <summary>
    /// Apple II text/graphics memory map chart.
    /// </summary>
    public partial class Apple2ScreenChart : Window {
        private const int NUM_HI_RES_ROWS = 192;
        private const int NUM_TEXT_ROWS = 24;
        private const int NUM_TEXT_HOLES = 8;
        private Formatter mFormatter;

        private static int[] sHiResRowsByAddr = GenerateHiResRowsByAddr();
        private static int[] sTextRowsByAddr = GenerateTextRowsByAddr();

        public enum ChartMode {
            Unknown = 0,
            HiRes1_L,
            HiRes2_L,
            HiRes1_A,
            HiRes2_A,
            TextWithHoles
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


        public Apple2ScreenChart(Window owner, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mFormatter = formatter;

            ChartModeItems = new ChartModeItem[] {
                new ChartModeItem((string)FindResource("str_HiRes1_L"), ChartMode.HiRes1_L),
                new ChartModeItem((string)FindResource("str_HiRes2_L"), ChartMode.HiRes2_L),
                new ChartModeItem((string)FindResource("str_HiRes1_A"), ChartMode.HiRes1_A),
                new ChartModeItem((string)FindResource("str_HiRes2_A"), ChartMode.HiRes2_A),
                new ChartModeItem((string)FindResource("str_TextWithHoles"), ChartMode.TextWithHoles),
            };
        }

        public void Window_Loaded(object sender, RoutedEventArgs e) {
            // Restore chart mode setting.
            ChartMode mode = (ChartMode)AppSettings.Global.GetEnum(
                AppSettings.A2SC_MODE, typeof(ChartMode), (int)ChartMode.HiRes1_L);
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

            AppSettings.Global.SetEnum(AppSettings.A2SC_MODE, typeof(ChartMode), (int)item.Mode);

            string text;
            switch (item.Mode) {
                case ChartMode.HiRes1_L:
                    text = DrawHiRes(0x2000, true);
                    break;
                case ChartMode.HiRes2_L:
                    text = DrawHiRes(0x4000, true);
                    break;
                case ChartMode.HiRes1_A:
                    text = DrawHiRes(0x2000, false);
                    break;
                case ChartMode.HiRes2_A:
                    text = DrawHiRes(0x4000, false);
                    break;
                case ChartMode.TextWithHoles:
                    text = DrawText();
                    break;
                default:
                    text = "UNKNOWN MODE";
                    break;
            }

            chartTextBox.Text = text;
            chartTextBox.SelectionStart = text.Length;
            chartTextBox.SelectionLength = 0;
        }

        /// <summary>
        /// Draws chart for hi-res graphics.
        /// </summary>
        /// <param name="baseAddr">Base address ($2000/$4000).</param>
        /// <param name="byLine">True if we want to sort by line number, false for address.</param>
        /// <returns>String with entire chart.</returns>
        private string DrawHiRes(int baseAddr, bool byLine) {
            const string eol = " \r\n";     // add space for balance
            const string div = "  |  ";
            string hdr;
            if (byLine) {
                hdr = "Line Addr ";
            } else {
                hdr = "Addr  Line";
            }

            StringBuilder sb = new StringBuilder(
                (hdr.Length * 4 + div.Length * 3 + eol.Length) * (NUM_HI_RES_ROWS / 4));

            sb.Append(hdr);
            sb.Append(div);
            sb.Append(hdr);
            sb.Append(div);
            sb.Append(hdr);
            sb.Append(div);
            sb.Append(hdr);
            for (int i = 0; i < NUM_HI_RES_ROWS / 4; i++) {
                sb.Append(eol);

                DrawHiResEntry(baseAddr, byLine, i, sb);
                sb.Append(div);
                DrawHiResEntry(baseAddr, byLine, i + NUM_HI_RES_ROWS / 4, sb);
                sb.Append(div);
                DrawHiResEntry(baseAddr, byLine, i + (NUM_HI_RES_ROWS * 2) / 4, sb);
                sb.Append(div);
                DrawHiResEntry(baseAddr, byLine, i + (NUM_HI_RES_ROWS * 3) / 4, sb);
            }
            return sb.ToString();
        }

        private void DrawHiResEntry(int baseAddr, bool byLine, int index, StringBuilder sb) {
            if (byLine) {
                sb.AppendFormat("{0,3:D}  {1}", index,
                    mFormatter.FormatHexValue(HiResRowToAddr(baseAddr, index), 4));
            } else {
                int row = sHiResRowsByAddr[index];
                sb.AppendFormat("{1}  {0,3:D}", row,
                    mFormatter.FormatHexValue(HiResRowToAddr(baseAddr, row), 4));
            }
        }

        /// <summary>
        /// Generates the address of a line on the hi-res screen.
        /// </summary>
        /// <param name="baseAddr">Base address ($2000 or $4000).</param>
        /// <param name="row">Row number, 0-191.</param>
        /// <returns>Address of start of line.</returns>
        private static int HiResRowToAddr(int baseAddr, int row) {
            // If row is ABCDEFGH, we want pppFGHCD EABAB000 (where p would be $20/$40).
            int low = ((row & 0xc0) >> 1) | ((row & 0xc0) >> 3) | ((row & 0x08) << 4);
            int high = ((row & 0x07) << 2) | ((row & 0x30) >> 4);
            int rowAddr = baseAddr + ((high << 8) | low);
            return rowAddr;
        }

        /// <summary>
        /// Generates a sorted list of hi-res row numbers.  The ordering is determined by the
        /// address in memory of the row.
        /// </summary>
        /// <returns>List of rows, in memory order.</returns>
        private static int[] GenerateHiResRowsByAddr() {
            SortedList<int, int> addrList = new SortedList<int, int>(NUM_HI_RES_ROWS);
            for (int i = 0; i < NUM_HI_RES_ROWS; i++) {
                addrList.Add(HiResRowToAddr(0, i), i);
            }

            return addrList.Values.ToArray();
        }

        /// <summary>
        /// Draws chart for the text screen.  There are few enough rows that we can do
        /// by-line and by-address for both pages in a reasonable amount of space.
        /// </summary>
        /// <returns>String with entire chart.</returns>
        private string DrawText() {
            const string eol = " \r\n";     // add space for balance
            const string div = "  |  ";
            const string hdr1 = "Line Page1  Page2";
            const string hdr2 = "Page1  Page2 Line";

            StringBuilder sb = new StringBuilder(
                (hdr1.Length * 2 + div.Length * 1 + eol.Length) * (NUM_TEXT_ROWS + NUM_TEXT_HOLES));

            sb.Append("     By Line     " + div + "   By Address" + eol);
            sb.Append(hdr1);
            sb.Append(div);
            sb.Append(hdr2);
            for (int i = 0; i < NUM_TEXT_ROWS + NUM_TEXT_HOLES; i++) {
                const int base1 = 0x400;
                const int base2 = 0x800;

                sb.Append(eol);

                int rowIndex = i < NUM_TEXT_ROWS ? i : NUM_TEXT_ROWS - i - 1;
                int textRow = sTextRowsByAddr[i];

                if (rowIndex >= 0) {
                    sb.AppendFormat(" {0,2:D}  {1}  {2}", rowIndex,
                        mFormatter.FormatHexValue(TextRowToAddr(base1, rowIndex), 4),
                        mFormatter.FormatHexValue(TextRowToAddr(base2, rowIndex), 4));
                } else {
                    sb.AppendFormat(" H{0}  {1}  {2}", -rowIndex - 1,
                        mFormatter.FormatHexValue(TextRowToAddr(base1, rowIndex), 4),
                        mFormatter.FormatHexValue(TextRowToAddr(base2, rowIndex), 4));
                }
                sb.Append(div);
                if (textRow >= 0) {
                    sb.AppendFormat("{1}  {2}   {0,2:D}", textRow,
                        mFormatter.FormatHexValue(TextRowToAddr(base1, textRow), 4),
                        mFormatter.FormatHexValue(TextRowToAddr(base2, textRow), 4));
                } else {
                    sb.AppendFormat("{1}  {2}   H{0}", -textRow - 1,
                        mFormatter.FormatHexValue(TextRowToAddr(base1, textRow), 4),
                        mFormatter.FormatHexValue(TextRowToAddr(base2, textRow), 4));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Generates the address of a line on the text screen.
        /// </summary>
        /// <param name="baseAddr">Base address (0x0400 or 0x0800).</param>
        /// <param name="row">Row number (0-23), or screen hole (-1 - -8).</param>
        /// <returns>Address of start of line.</returns>
        private static int TextRowToAddr(int baseAddr, int row) {
            if (row < 0) {
                // Screen hole: $478, $4f8, ...
                return baseAddr + (-row) * 128 - 8;
            } else {
                // If row is 000ABCDE, we want 0000ppCD EABAB000 (where p is $04/$08).
                int high = (row & 0x06) >> 1;
                int low = (row & 0x18) | ((row & 0x18) << 2) | ((row & 0x01) << 7);
                int rowAddr = baseAddr + ((high << 8) | low);
                return rowAddr;
            }
        }

        /// <summary>
        /// Generates a sorted list of text row numbers.  The ordering is determined by the
        /// address in memory of the row.
        /// </summary>
        /// <returns>List of rows, in memory order.</returns>
        private static int[] GenerateTextRowsByAddr() {
            SortedList<int, int> addrList = new SortedList<int, int>(NUM_TEXT_ROWS);
            for (int i = -NUM_TEXT_HOLES - 1; i < NUM_TEXT_ROWS; i++) {
                addrList.Add(TextRowToAddr(0, i), i);
            }

            return addrList.Values.ToArray();
        }
    }
}
