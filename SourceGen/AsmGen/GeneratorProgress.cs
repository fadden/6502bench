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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace SourceGen.AsmGen {
    /// <summary>
    /// Dialog that shows the progress of the assembler, and allows cancellation.
    /// </summary>
    public partial class GeneratorProgress : Form {
        /// <summary>
        /// Full pathnames of generated files.  Will be null on error or user cancelation.
        /// </summary>
        public List<string> Results { get; private set; }

        private IGenerator mGenerator;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gen">Fully-configured source generator.</param>
        public GeneratorProgress(IGenerator gen) {
            InitializeComponent();

            mGenerator = gen;
        }

        private void GeneratorProgress_Load(object sender, EventArgs e) {
            backgroundWorker1.RunWorkerAsync();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            backgroundWorker1.CancelAsync();
            cancelButton.Enabled = false;
        }

        private void GeneratorProgress_FormClosing(object sender, FormClosingEventArgs e) {
            // The close button will close the dialog without canceling the event.  We
            // cancel it here, which should cause it to stop relatively quickly, but we don't
            // wait for it on the off chance that something weird is going on and it got
            // stuck.  If nothing else, this gives the user a chance to save their work.
            if (backgroundWorker1.IsBusy) {
                backgroundWorker1.CancelAsync();
                DialogResult = DialogResult.Cancel;
            }
        }

        // NOTE: executes on work thread.  DO NOT do any UI work here.  DO NOT access
        // the Results property directly.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            // This will throw an I/O exception if there's a problem with the file.  This
            // will be caught and transferred to RunWorkerCompleted.
            List<string> fileNames = mGenerator.GenerateSource(worker);
            if (worker.CancellationPending) {
                e.Cancel = true;
            } else {
                e.Result = fileNames;
            }
        }

        // Callback that fires when a progress update is made.
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            int percent = e.ProgressPercentage;
            string msg = e.UserState as string;

            Debug.Assert(percent >= 0 && percent <= 100);

            if (!string.IsNullOrEmpty(msg)) {
                progressLabel.Text = msg;
            }
            progressBar1.Value = percent;
        }

        // Callback that fires when execution completes.
        private void backgroundWorker1_RunWorkerCompleted(object sender,
                RunWorkerCompletedEventArgs e) {
            if (e.Cancelled) {
                Debug.WriteLine("CANCELED");
                DialogResult = DialogResult.Cancel;
            } else if (e.Error != null) {
                // This should only happen on a file I/O error, e.g. out of disk space or
                // unable to overwrite an existing file.
                MessageBox.Show(e.Error.ToString(), Properties.Resources.OPERATION_FAILED,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
            } else {
                // Make results available in property.
                Results = e.Result as List<string>;

                if (Results == null || Results.Count == 0) {
                    // Shouldn't happen -- generator should have reported error.
                    MessageBox.Show("Internal error: no files generated",
                         Properties.Resources.OPERATION_FAILED,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                } else {
                    Debug.WriteLine("SUCCESS " + Results.Count);
                }
                DialogResult = DialogResult.OK;
            }

            // Whatever the case, we're done.
            this.Close();
        }
    }
}
