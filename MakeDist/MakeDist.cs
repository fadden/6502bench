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
using System.Windows.Forms;

namespace MakeDist {
    public partial class MakeDist : Form {
        public MakeDist() {
            InitializeComponent();
        }

        private void MakeDist_Load(object sender, EventArgs e) {
            releaseDistRadio.Checked = true;
        }

        private void goButton_Click(object sender, EventArgs e) {
            FileCopier.BuildType buildType;
            if (releaseDistRadio.Checked) {
                buildType = FileCopier.BuildType.Release;
            } else {
                buildType = FileCopier.BuildType.Debug;
            }
            bool copyTestFiles = includeTestsCheckBox.Checked;

            CopyProgress dlg = new CopyProgress(buildType, copyTestFiles);
            dlg.ShowDialog();
            dlg.Dispose();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
