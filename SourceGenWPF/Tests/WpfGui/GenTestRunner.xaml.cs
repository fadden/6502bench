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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using CommonWPF;

namespace SourceGenWPF.Tests.WpfGui {
    /// <summary>
    /// Source generation test runner.
    /// </summary>
    public partial class GenTestRunner : Window, INotifyPropertyChanged {
        private List<GenTest.GenTestResults> mLastResults;

        private BackgroundWorker mWorker;

        private FlowDocument mFlowDoc = new FlowDocument();
        private Color mDefaultColor;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// True when we're not running.  Used to enable the "run test" button.
        /// </summary>
        public bool IsNotRunning {
            get { return mIsNotRunning; }
            set {
                mIsNotRunning = value;
                OnPropertyChanged();
            }
        }
        private bool mIsNotRunning;

        public bool IsOutputRetained {
            get { return mIsOutputRetained; }
            set {
                mIsOutputRetained = value;
                OnPropertyChanged();
            }
        }
        private bool mIsOutputRetained;

        public string RunButtonLabel {
            get { return mRunButtonLabel; }
            set {
                mRunButtonLabel = value;
                OnPropertyChanged();
            }
        }
        private string mRunButtonLabel;


        public GenTestRunner(Window owner) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mDefaultColor = ((SolidColorBrush)progressRichTextBox.Foreground).Color;

            // Create and configure the BackgroundWorker.
            mWorker = new BackgroundWorker();
            mWorker.WorkerReportsProgress = true;
            mWorker.WorkerSupportsCancellation = true;
            mWorker.DoWork += BackgroundWorker_DoWork;
            mWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            mWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            IsNotRunning = true;
            RunButtonLabel = (string)FindResource("str_RunTest");
            progressRichTextBox.Document = mFlowDoc;
        }

        /// <summary>
        /// Handles a click on the "run test" button, which becomes a "cancel test" button once
        /// the test has started.
        /// </summary>
        private void RunCancelButton_Click(object sender, RoutedEventArgs e) {
            if (mWorker.IsBusy) {
                IsNotRunning = false;
                mWorker.CancelAsync();
            } else {
                ResetDialog();
                RunButtonLabel = (string)FindResource("str_CancelTest");
                mWorker.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Cancels the test if the user closes the window.
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e) {
            if (mWorker.IsBusy) {
                mWorker.CancelAsync();
            }
        }

        private void ResetDialog() {
            outputSelectComboBox.Items.Clear();
            mFlowDoc.Blocks.Clear();
            outputTextBox.Clear();
            mLastResults = null;
        }

        // NOTE: executes on work thread.  DO NOT do any UI work here.  Pass the test
        // results through e.Result.
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            BackgroundWorker worker = sender as BackgroundWorker;

            GenTest test = new GenTest();
            test.RetainOutput = IsOutputRetained;   // should be okay to read from work thread
            List<GenTest.GenTestResults> results = test.Run(worker);

            if (worker.CancellationPending) {
                e.Cancel = true;
            } else {
                e.Result = results;
            }
        }

        // Callback that fires when a progress update is made.
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            // We get progress from GenTest, and from the IAssembler/IGenerator classes.  This
            // is necessary to make cancellation work right, and allows us to show the
            // asm/gen progress messages if we want to.
            if (e.UserState is ProgressMessage) {
                ProgressMessage msg = e.UserState as ProgressMessage;
                if (msg.HasColor) {
                    progressRichTextBox.AppendText(msg.Text, msg.Color);
                } else {
                    // plain foreground text color
                    progressRichTextBox.AppendText(msg.Text, mDefaultColor);
                }
                progressRichTextBox.ScrollToEnd();
            } else {
                // Most progress updates have an e.ProgressPercentage value and a blank string.
                if (!string.IsNullOrEmpty((string)e.UserState)) {
                    Debug.WriteLine("Sub-progress: " + e.UserState);
                }
            }
        }

        // Callback that fires when execution completes.
        private void BackgroundWorker_RunWorkerCompleted(object sender,
                RunWorkerCompletedEventArgs e) {
            if (e.Cancelled) {
                Debug.WriteLine("Test halted -- user cancellation");
            } else if (e.Error != null) {
                // test harness shouldn't be throwing errors like this
                Debug.WriteLine("Test failed: " + e.Error.ToString());
                progressRichTextBox.AppendText("\r\n");
                progressRichTextBox.AppendText(e.Error.ToString(), mDefaultColor);
                progressRichTextBox.ScrollToEnd();
            } else {
                Debug.WriteLine("Tests complete");
                mLastResults = e.Result as List<GenTest.GenTestResults>;
                if (mLastResults != null) {
                    PopulateOutputSelect();
                }
            }

            RunButtonLabel = (string)FindResource("str_RunTest");
            IsNotRunning = true;
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

        private void OutputSelectComboBox_SelectedIndexChanged(object sender,
                SelectionChangedEventArgs e) {
            int sel = outputSelectComboBox.SelectedIndex;
            if (sel < 0) {
                // selection has been cleared
                outputTextBox.Text = string.Empty;
                return;
            }
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
