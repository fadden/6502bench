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
using System.Text;
using System.Windows.Forms;

using CommonWinForms;

namespace SourceGen.Tests {
    /// <summary>
    /// Source generation regression test.
    /// </summary>
    public partial class GenTestRunner : Form {
        private List<GenTest.GenTestResults> mLastResults;

        private bool mClosedWhileRunning;

        public GenTestRunner() {
            InitializeComponent();
        }

        private void runButton_Click(object sender, EventArgs e) {
            if (backgroundWorker1.IsBusy) {
                runButton.Enabled = false;
                backgroundWorker1.CancelAsync();
            } else {
                ResetDialog();
                runButton.Text = "Cancel";
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void GenTestRunner_FormClosing(object sender, FormClosingEventArgs e) {
            if (backgroundWorker1.IsBusy) {
                backgroundWorker1.CancelAsync();
                DialogResult = DialogResult.Cancel;

                // If we close now, the app crashes with a "you disposed of the
                // RichTextBox" exception.  So the first time they request it, refuse.
                // The second time, we go ahead and close, in case something is really stuck.
                if (!mClosedWhileRunning) {
                    e.Cancel = true;
                    mClosedWhileRunning = true;
                }
            } else {
                DialogResult = DialogResult.OK;
            }
        }

        private void ResetDialog() {
            outputSelectComboBox.Items.Clear();
            progressRichTextBox.Clear();
            outputTextBox.Clear();
            mLastResults = null;
        }

        // NOTE: executes on work thread.  DO NOT do any UI work here.  Pass the test
        // results through e.Result.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            GenTest test = new GenTest();
            test.RetainOutput = retainOutputCheckBox.Checked;   // okay to do on work thread?
            List<GenTest.GenTestResults> results = test.Run(worker);

            if (worker.CancellationPending) {
                e.Cancel = true;
            } else {
                e.Result = results;
            }
        }

        // Callback that fires when a progress update is made.
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            // We get progress from GenTest, and from the IAssembler/IGenerator classes.  This
            // is necessary to make cancellation work right, and allows us to show the
            // asm/gen progress messages if we want to.
            if (e.UserState is ProgressMessage) {
                ProgressMessage msg = e.UserState as ProgressMessage;
                if (msg.HasColor) {
                    progressRichTextBox.AppendText(msg.Text, msg.Color);
                } else {
                    // plain foreground text color
                    progressRichTextBox.AppendText(msg.Text);
                }
                progressRichTextBox.SelectionStart = progressRichTextBox.Text.Length;
                progressRichTextBox.ScrollToCaret();
            } else {
                // Most progress updates have an e.ProgressPercentage value and a blank string.
                if (!string.IsNullOrEmpty((string)e.UserState)) {
                    Debug.WriteLine("Sub-progress: " + e.UserState);
                }
            }
        }

        // Callback that fires when execution completes.
        private void backgroundWorker1_RunWorkerCompleted(object sender,
                RunWorkerCompletedEventArgs e) {
            if (e.Cancelled) {
                Debug.WriteLine("Test halted -- user cancellation");
            } else if (e.Error != null) {
                // test harness shouldn't be throwing errors like this
                Debug.WriteLine("Test failed: " + e.Error.ToString());
                progressRichTextBox.AppendText("\r\n");
                progressRichTextBox.AppendText(e.Error.ToString());
                progressRichTextBox.SelectionStart = progressRichTextBox.Text.Length;
                progressRichTextBox.ScrollToCaret();
            } else {
                Debug.WriteLine("Tests complete");
                mLastResults = e.Result as List<GenTest.GenTestResults>;
                if (mLastResults != null) {
                    PopulateOutputSelect();
                }
            }

            runButton.Text = "Run Test";
            runButton.Enabled = true;

            if (mClosedWhileRunning) {
                Close();
            }
        }

        private void PopulateOutputSelect() {
            outputSelectComboBox.Items.Clear();
            if (mLastResults.Count == 0) {
                return;
            }

            foreach (GenTest.GenTestResults results in mLastResults) {
                outputSelectComboBox.Items.Add(results);
            }

            // Trigger update.
            outputSelectComboBox.SelectedIndex = 0;
        }

        private void outputSelectComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            int sel = outputSelectComboBox.SelectedIndex;
            if (mLastResults == null && mLastResults.Count <= sel) {
                Debug.WriteLine("SelIndexChanged to " + sel + ", not available");
                return;
            }

            GenTest.GenTestResults results = mLastResults[sel];

            StringBuilder sb = new StringBuilder(512);
            sb.AppendFormat("Path: {0}\r\n", results.PathName);
            sb.AppendFormat("Assembler: {0}\r\n", results.AsmId);
            if (results.ProjectLoadReport != null) {
                sb.AppendFormat("Project load: {0}\r\n", results.ProjectLoadReport.Format());
            }
            if (results.GenerateOkay) {
                sb.Append("Source gen: OK\r\n");
            } else {
                sb.Append("Source gen: FAIL\r\n");
            }
            if (results.AssembleOkay) {
                sb.Append("Asm gen: OK\r\n");
            } else {
                sb.Append("Asm gen: FAIL\r\n");
            }
            if (results.AsmResults != null) {
                AsmGen.AssemblerResults asmr = results.AsmResults;
                sb.AppendFormat("Cmd line: {0}\r\n", asmr.CommandLine);
                if (!results.AssembleOkay) {
                    sb.AppendFormat("Exit code: {0}\r\n", asmr.ExitCode);
                }
                if (asmr.Stdout != null && asmr.Stdout.Length > 2) {
                    sb.Append("----- stdout -----\r\n");
                    sb.Append(asmr.Stdout);
                    sb.Append("\r\n");
                }
                if (asmr.Stderr != null && asmr.Stderr.Length > 2) {
                    sb.Append("----- stderr -----\r\n");
                    sb.Append(asmr.Stderr);
                    sb.Append("\r\n");
                }
            }
            if (results.Timer != null) {
                sb.Append("\r\n----- task times -----\r\n");
                sb.Append(results.Timer.DumpToString(string.Empty));
            }

            outputTextBox.Text = sb.ToString();
        }
    }
}
