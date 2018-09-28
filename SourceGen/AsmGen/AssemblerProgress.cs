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
    public partial class AssemblerProgress : Form {
        /// <summary>
        /// On success, assembler results will be here.
        /// </summary>
        public AssemblerResults Results { get; private set; }

        /// <summary>
        /// Assembler executor.
        /// </summary>
        private IAssembler mAssembler;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gen">Fully-configured source generator.</param>
        public AssemblerProgress(IAssembler asm) {
            InitializeComponent();

            mAssembler = asm;
        }

        private void AssemblerProgress_Load(object sender, EventArgs e) {
            backgroundWorker1.RunWorkerAsync();
        }

        private void cancelButton_Click(object sender, EventArgs e) {
            backgroundWorker1.CancelAsync();
            cancelButton.Enabled = false;

            // We don't have a polite way to ask a shell command to stop, so we should
            // kill the process here.  Need to figure out how to do that.  We don't need
            // to clean up partial output.
        }

        private void AssemblerProgress_FormClosing(object sender, FormClosingEventArgs e) {
            // Strictly speaking, we should treat this as a cancel request, and set
            // e.Cancel = true to prevent the form from closing until the assembler stops.
            // However, we don't currently kill runaway processes, which would leave the
            // user with no way to close the dialog, potentially requiring them to kill the
            // entire app with unsaved work.  Better to abandon the runaway process.
            //
            // We call CancelAsync so that the results are discarded should the worker
            // eventually finish.
            if (backgroundWorker1.IsBusy) {
                backgroundWorker1.CancelAsync();
                DialogResult = DialogResult.Cancel;
            }
        }

        // NOTE: executes on work thread.  DO NOT do any UI work here.  DO NOT access
        // the Results property directly.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            AssemblerResults results = mAssembler.RunAssembler(worker);
            if (worker.CancellationPending) {
                e.Cancel = true;
            } else {
                e.Result = results;
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
                // Unexpected -- shell command execution shouldn't throw exceptions.
                MessageBox.Show(e.Error.ToString(), Properties.Resources.OPERATION_FAILED,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
            } else {
                // Make results available in properties.
                Results = e.Result as AssemblerResults;
                Debug.WriteLine("Asm complete, exit=" + Results.ExitCode);
                DialogResult = DialogResult.OK;
            }

            // Whatever the case, we're done.
            this.Close();
        }
    }
}
