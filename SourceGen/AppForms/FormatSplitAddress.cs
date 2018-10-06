/*
 * Copyright 2018 faddenSoft
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
using System.Windows.Forms;

using Asm65;
using CommonUtil;

namespace SourceGen.AppForms {
    public partial class FormatSplitAddress : Form {
        /// <summary>
        /// Result set that describes the formatting to perform.  Not all regions will have
        /// the same format, e.g. the "mixed ASCII" mode will alternate strings and bytes
        /// (rather than a dedicated "mixed ASCII" format type).
        /// </summary>
        public SortedList<int, FormatDescriptor> Results { get; private set; }

        /// <summary>
        /// Selected offsets.  An otherwise contiguous range of offsets can be broken up
        /// by user-specified labels and address discontinuities, so this needs to be
        /// processed by range.
        /// </summary>
        public TypedRangeSet Selection { private get; set; }

        /// <summary>
        /// Raw file data.
        /// </summary>
        private byte[] mFileData;

        /// <summary>
        /// Symbol table to use when resolving symbolic values.
        /// </summary>
        private SymbolTable mSymbolTable;

        /// <summary>
        /// Formatter to use when displaying addresses and hex values.
        /// </summary>
        private Asm65.Formatter mFormatter;


        public FormatSplitAddress(byte[] fileData, SymbolTable symbolTable,
                Formatter formatter) {
            InitializeComponent();

            mFileData = fileData;
            mSymbolTable = symbolTable;
            mFormatter = formatter;
        }

        private void FormatSplitAddress_Load(object sender, EventArgs e) {

        }

        private void okButton_Click(object sender, EventArgs e) {
            Results = new SortedList<int, FormatDescriptor>();
        }
    }
}
