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
using System.Diagnostics;
using System.Windows.Forms;

namespace SourceGen.Tools {
    /// <summary>
    /// Simple form for showing text in a TextBox.  This can be used as a modeless
    /// dialog, so a "window closing" event is available.
    /// </summary>
    public partial class ShowText : Form {
        /// <summary>
        /// Window title.
        /// </summary>
        public string Title {
            set {
                Text = value;
            }
        }

        /// <summary>
        /// Text to display.
        /// </summary>
        public string BodyText {
            get {
                return textBox.Text;
            }
            set {
                textBox.Text = value;
                textBox.SelectionStart = textBox.Text.Length;
                textBox.ScrollToCaret();
            }
        }

        /// <summary>
        /// Subscribe to this to be notified when the dialog closes.
        /// </summary>
        public event WindowClosing OnWindowClosing;
        public delegate void WindowClosing(object sender);

        public ShowText() {
            InitializeComponent();
        }

        private void ShowText_Load(object sender, EventArgs e) {
            //textBox.Select(BodyText.Length, BodyText.Length);

            if (Modal) {
                MaximizeBox = false;
                MinimizeBox = false;
                // Changing the ShowInTaskbar value kills the dialog.  If we really care we
                // can pass a "will be modal" parameter to the constructor and do it there.
                // https://stackoverflow.com/a/20443430/294248
                //ShowInTaskbar = false;
            }
        }

        private void ShowText_FormClosed(object sender, FormClosedEventArgs e) {
            if (OnWindowClosing != null) {
                OnWindowClosing(this);
            }
        }
    }
}
