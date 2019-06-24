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

using AssemblerInfo = SourceGenWPF.AsmGen.AssemblerInfo;
using AssemblerConfig = SourceGenWPF.AsmGen.AssemblerConfig;
using ExpressionMode = Asm65.Formatter.FormatConfig.ExpressionMode;

namespace SourceGenWPF.WpfGui {
    /// <summary>
    /// Application settings dialog.
    /// </summary>
    public partial class EditAppSettings : Window {
        /// <summary>
        /// Tab page enumeration.  Numbers must match page indices in designer.
        /// </summary>
        public enum Tab {
            Unknown = -1,
            CodeView = 0,
            AsmConfig = 1,
            DisplayFormat = 2,
            PseudoOp = 3
        }

        /// <summary>
        /// Tab to show when dialog is first opened.
        /// </summary>
        private Tab mInitialTab;

        /// <summary>
        /// Assembler to initially select in combo boxes.
        /// </summary>
        private AssemblerInfo.Id mInitialAsmId;


        public EditAppSettings(Window owner, Tab initialTab,
                AsmGen.AssemblerInfo.Id initialAsmId) {
            InitializeComponent();
            Owner = owner;

            mInitialTab = initialTab;
            mInitialAsmId = initialAsmId;
        }
    }
}
