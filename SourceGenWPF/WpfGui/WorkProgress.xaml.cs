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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace SourceGenWPF.WpfGui {
    /// <summary>
    /// Cancelable progress dialog.
    /// </summary>
    public partial class WorkProgress : Window {
        /// <summary>
        /// Task-specific stuff.
        /// </summary>
        public interface IWorker {
            /// <summary>
            /// Does the work, executing on a work thread.
            /// </summary>
            /// <param name="worker">BackgroundWorker object reference.</param>
            /// <returns>Results of work.</returns>
            object DoWork(BackgroundWorker worker);

            /// <summary>
            /// Called on successful completion of the work.  Executes on main thread.
            /// </summary>
            /// <param name="results">Results of work.</param>
            void RunWorkerCompleted(object results);
        }

        private IWorker mCallbacks;

        private BackgroundWorker mWorker;


        public WorkProgress(Window owner, IWorker callbacks, bool indeterminate) {
            InitializeComponent();
            Owner = owner;

            progressBar.IsIndeterminate = indeterminate;

            mCallbacks = callbacks;

            // Create and configure the BackgroundWorker.
            mWorker = new BackgroundWorker();
            mWorker.WorkerReportsProgress = true;
            mWorker.WorkerSupportsCancellation = true;
            mWorker.DoWork += DoWork;
            mWorker.ProgressChanged += ProgressChanged;
            mWorker.RunWorkerCompleted += RunWorkerCompleted;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            mWorker.RunWorkerAsync();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            mWorker.CancelAsync();
            cancelButton.IsEnabled = false;
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            // Either we're closing naturally, or the user clicked the 'X' in the window frame.
            //
            // Strictly speaking, we should treat this as a cancel request, and set
            // e.Cancel = true to prevent the form from closing until the worker stops.
            // However, we don't currently kill runaway processes, which would leave the
            // user with no way to close the dialog, potentially requiring them to kill the
            // entire app with unsaved work.  Better to abandon the runaway process.
            //
            // We call CancelAsync so that the results are discarded should the worker
            // eventually finish.
            if (mWorker.IsBusy) {
                mWorker.CancelAsync();
                DialogResult = false;
            }
        }

        // NOTE: executes on work thread.  DO NOT do any UI work here.  DO NOT access
        // the Results property directly.
        private void DoWork(object sender, DoWorkEventArgs e) {
            Debug.Assert(sender == mWorker);

            object results = mCallbacks.DoWork(mWorker);
            if (mWorker.CancellationPending) {
                e.Cancel = true;
            } else {
                e.Result = results;
            }
        }

        // Callback that fires when a progress update is made.
        private void ProgressChanged(object sender, ProgressChangedEventArgs e) {
            int percent = e.ProgressPercentage;
            string msg = e.UserState as string;

            Debug.Assert(percent >= 0 && percent <= 100);

            if (!string.IsNullOrEmpty(msg)) {
                messageText.Text = msg;
            }
            progressBar.Value = percent;
        }

        // Callback that fires when execution completes.
        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Cancelled) {
                Debug.WriteLine("CANCELED " + DialogResult);
                // If the window was closed, the DialogResult will already be set, and WPF
                // throws a misleading exception ("only after Window is created and shown")
                // if you try to set the result twice.
                if (DialogResult == null) {
                    DialogResult = false;
                }
            } else if (e.Error != null) {
                // Unexpected -- shell command execution shouldn't throw exceptions.
                MessageBox.Show(e.Error.ToString(), Res.Strings.OPERATION_FAILED,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
            } else {
                mCallbacks.RunWorkerCompleted(e.Result);
                DialogResult = true;
            }
        }
    }
}
