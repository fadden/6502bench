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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Asm65;

namespace SourceGen.Tools.WpfGui {
    /// <summary>
    /// CPU instruction chart.
    /// </summary>
    public partial class InstructionChart : Window, INotifyPropertyChanged {
        /// <summary>
        /// Item for CPU selection combo box.
        /// </summary>
        public class CpuItem {
            public string Name { get; private set; }
            public CpuDef.CpuType Type { get; private set; }

            public CpuItem(string name, CpuDef.CpuType type) {
                Name = name;
                Type = type;
            }
        }
        public CpuItem[] CpuItems { get; private set; }

        public bool ShowUndocumented {
            get { return mShowUndocumented; }
            set { mShowUndocumented = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mShowUndocumented;

        /// <summary>
        /// Item for main list.
        /// </summary>
        public class InstructionItem {
            public string Opcode { get; private set; }
            public string Sample { get; private set; }
            public string Flags { get; private set; }
            public string Cycles { get; private set; }
            public string ShortDesc { get; private set; }
            public string AddressMode { get; private set; }

            public bool IsUndocumented { get; private set; }

            public InstructionItem(string opcode, string sample, string flags, string cycles,
                    string shortDesc, string addrMode, bool isUndoc) {
                Opcode = opcode;
                Sample = sample;
                Flags = flags;
                Cycles = cycles;
                ShortDesc = shortDesc;
                AddressMode = addrMode;
                IsUndocumented = isUndoc;
            }
        }

        public ObservableCollection<InstructionItem> InstructionItems { get; private set; } =
            new ObservableCollection<InstructionItem>();

        private OpDescription mOpDesc = OpDescription.GetOpDescription(null);

        private Formatter mFormatter;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        /// <param name="formatter">Text formatter.</param>
        public InstructionChart(Window owner, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mFormatter = formatter;

            CpuItems = new CpuItem[] {
                new CpuItem((string)FindResource("str_6502"), CpuDef.CpuType.Cpu6502),
                new CpuItem((string)FindResource("str_65C02"), CpuDef.CpuType.Cpu65C02),
                new CpuItem((string)FindResource("str_W65C02"), CpuDef.CpuType.CpuW65C02),
                new CpuItem((string)FindResource("str_65816"), CpuDef.CpuType.Cpu65816),
            };
        }

        public void Window_Loaded(object sender, RoutedEventArgs e) {
            // Restore chart settings.
            CpuDef.CpuType type = (CpuDef.CpuType)AppSettings.Global.GetEnum(
                AppSettings.INSTCH_MODE, typeof(CpuDef.CpuType), (int)CpuDef.CpuType.Cpu6502);
            ShowUndocumented = AppSettings.Global.GetBool(AppSettings.INSTCH_SHOW_UNDOC, true);

            int index = 0;
            for (int i = 0; i < CpuItems.Length; i++) {
                if (CpuItems[i].Type == type) {
                    index = i;
                    break;
                }
            }
            cpuSelectionComboBox.SelectedIndex = index;
        }

        // Catch ESC key.
        private void Window_KeyEventHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                Close();
            }
        }

        private void CpuSelectionComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            UpdateControls();
        }

        private void UpdateControls() {
            CpuItem item = (CpuItem)cpuSelectionComboBox.SelectedItem;
            if (item == null) {
                // initializing
                return;
            }

            // Push current choice to settings.
            AppSettings.Global.SetEnum(AppSettings.INSTCH_MODE, typeof(CpuDef.CpuType),
                (int)item.Type);
            AppSettings.Global.SetBool(AppSettings.INSTCH_SHOW_UNDOC, mShowUndocumented);

            // Populate the items source.
            InstructionItems.Clear();
            CpuDef cpuDef = CpuDef.GetBestMatch(item.Type, true, false);
            for (int opc = 0; opc < 256; opc++) {
                OpDef op = cpuDef[opc];
                if (!mShowUndocumented && op.IsUndocumented) {
                    continue;
                }

                int instrLen = op.GetLength(StatusFlags.AllIndeterminate);
                if (op.AddrMode == OpDef.AddressMode.PCRel) {
                    // Single-byte branch instructions are formatted with a 16-bit
                    // absolute addres.
                    instrLen = 3;
                }

                string sampleValue = "$12";
                if (op.AddrMode == OpDef.AddressMode.BlockMove) {
                    sampleValue = "#$12,#$34";
                } else if (op.AddrMode == OpDef.AddressMode.DPPCRel) {
                    sampleValue = "$12,$1234";
                } else if (instrLen == 3) {
                    sampleValue = "$1234";
                } else if (instrLen == 4) {
                    sampleValue = "$123456";
                }
                string instrSample = mFormatter.FormatMnemonic(op.Mnemonic,
                        OpDef.WidthDisambiguation.None) + " " +
                    mFormatter.FormatOperand(op, sampleValue, OpDef.WidthDisambiguation.None);


                StringBuilder flags = new StringBuilder(8);
                const string FLAGS = "NVMXDIZC";
                Asm65.StatusFlags affectedFlags = op.FlagsAffected;
                for (int fl = 0; fl < 8; fl++) {
                    if (affectedFlags.GetBit((StatusFlags.FlagBits)(7 - fl)) >= 0) {
                        flags.Append(FLAGS[fl]);
                    } else {
                        flags.Append("-");
                    }
                }

                string cycles = op.Cycles.ToString();
                OpDef.CycleMod mods = cpuDef.GetOpCycleMod(opc);
                if (mods != 0) {
                    cycles += '+';
                }

                InstructionItems.Add(new InstructionItem(mFormatter.FormatHexValue(opc, 2),
                    instrSample, flags.ToString(), cycles,
                    mOpDesc.GetShortDescription(op.Mnemonic),
                    mOpDesc.GetAddressModeDescription(op.AddrMode),
                    op.IsUndocumented));
            }
        }
    }
}
