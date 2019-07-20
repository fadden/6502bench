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
using System.IO;
using System.Windows.Forms;

namespace SourceGenWF.AppForms {
    public partial class AboutBox : Form {
        private const string IMAGE_FILE_NAME = "AboutImage.png";
        private const string LEGAL_STUFF_FILE_NAME = "LegalStuff.txt";

        public AboutBox() {
            InitializeComponent();

            boardPictureBox.ImageLocation = RuntimeDataAccess.GetPathName(IMAGE_FILE_NAME);
            versionLabel.Text = string.Format(Properties.Resources.VERSION_FMT,
                Program.ProgramVersion);

            osPlatformLabel.Text = "OS: " +
                System.Runtime.InteropServices.RuntimeInformation.OSDescription;
#if DEBUG
            debugEnabledLabel.Visible = true;
#endif
        }

        private void AboutBox_Load(object sender, EventArgs e) {
            string text;
            string pathName = RuntimeDataAccess.GetPathName(LEGAL_STUFF_FILE_NAME);
            try {
                text = File.ReadAllText(pathName);
            } catch (Exception ex) {
                text = ex.ToString();
            }

            legalStuffTextBox.Text = text;
        }
    }
}
