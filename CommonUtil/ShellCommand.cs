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
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace CommonUtil {
    /// <summary>
    /// Execute a shell command and return stdout/stderr.
    /// 
    /// Returning stdout/stderr separately loses the interleave, but it's unclear whether
    /// that gets lost anyway with output buffering and the asynchronous I/O facility.
    /// </summary>
    public class ShellCommand {
        // These were handy:
        //  https://stackoverflow.com/a/32872174/294248
        //  https://stackoverflow.com/a/18616369/294248
        //  https://stackoverflow.com/a/7334029/294248
        //  https://stackoverflow.com/a/5187715/294248

        private const int CMD_TIMEOUT_MS = 10000;       // 10 sec
        private bool USE_CMD_EXE = false;

        /// <summary>
        /// Filename of shell command to execute.
        /// </summary>
        public string CommandFileName { get; private set; }

        /// <summary>
        /// Arguments to pass to command.  Individual arguments are separated by spaces.
        /// If an argument may contain spaces (e.g. it's a filename), surround it with
        /// double quotes (").
        /// </summary>
        public string Arguments { get; private set; }

        /// <summary>
        /// Working directory for command.  The directory will be changed for the
        /// command only.
        /// </summary>
        public string WorkDirectory { get; private set; }

        public Dictionary<string, string> EnvVars { get; private set; }

        /// <summary>
        /// The full command line, for display purposes.  This is just CommandFileName + Arguments
        /// unless some funny business is going on under the hood.
        /// </summary>
        public string FullCommandLine { get; private set; }

        /// <summary>
        /// Output from stdout.
        /// </summary>
        public string Stdout { get; private set; }

        /// <summary>
        /// Output from stderr.
        /// </summary>
        public string Stderr { get; private set; }

        /// <summary>
        /// Command exit code.  Will be 0 on success.
        /// </summary>
        public int ExitCode { get; private set; }

        /// <summary>
        /// Buffers for gathering stdout/stderr.
        /// </summary>
        private StringBuilder mStdout, mStderr;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="commandFileName">Filename of command to execute.</param>
        /// <param name="arguments">Command arguments, separated by spaces.  Surround args with
        ///   embedded spaces with double quotes.  Pass empty string if no args.</param>
        /// <param name="workDir">Working directory for command.  Pass an empty string if you
        ///   want to use the default.</param>
        /// <param name="env">Dictionary of values to set in the shell environment.</param>
        public ShellCommand(string commandFileName, string arguments, string workDir,
                Dictionary<string, string> env) {
            Debug.Assert(commandFileName != null);
            Debug.Assert(arguments != null);
            Debug.Assert(workDir != null);

            CommandFileName = commandFileName;
            Arguments = arguments;
            WorkDirectory = workDir;
            EnvVars = env;

            ExitCode = -100;

            mStdout = new StringBuilder();
            mStderr = new StringBuilder();
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <returns>Process exit code.  0 on success.</returns>
        public void Execute() {
            // Works for full paths to command, but not for shell stuff like "dir".
            FileInfo fi = new FileInfo(CommandFileName);
            if (!fi.Exists) {
                Debug.WriteLine("Warning: file '" + CommandFileName + "' does not exist");
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            if (USE_CMD_EXE) {
                // Run inside cmd.exe on Windows.
                psi.FileName = "cmd.exe";
                psi.Arguments = "/C " + CommandFileName +
                    (string.IsNullOrEmpty(Arguments) ? "" : " " + Arguments);
            } else {
                psi.FileName = CommandFileName;
                psi.Arguments = Arguments;
            }
            FullCommandLine = psi.FileName + " " + psi.Arguments;
            psi.CreateNoWindow = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.UseShellExecute = false;    // required for stdin/stdout redirect
            if (!string.IsNullOrEmpty(WorkDirectory)) {
                psi.WorkingDirectory = WorkDirectory;
            }

            if (EnvVars != null) {
                foreach (KeyValuePair<string, string> kvp in EnvVars) {
                    Debug.WriteLine("ENV: " + kvp.Key + "=" + kvp.Value);
                    psi.Environment.Add(kvp);
                }
            }

            try {
                using (Process process = Process.Start(psi)) {
                    process.OutputDataReceived += (sendingProcess, outLine) =>
                        mStdout.AppendLine(outLine.Data);
                    process.ErrorDataReceived += (sendingProcess, errLine) =>
                        mStderr.AppendLine(errLine.Data);
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Close stdin so interactive programs don't stall.
                    process.StandardInput.Close();

                    // I'm calling with a (fairly long) timeout, just in case.
                    process.WaitForExit(CMD_TIMEOUT_MS);
                    if (!process.HasExited) {
                        Debug.WriteLine("Process stalled, killing");
                        process.Kill();
                    }

                    // WaitForExit(timeout) can return before the async stdout/stderr stuff
                    // has completed.  Calling it without a timeout ensures correct behavior.
                    process.WaitForExit();

                    // This will be zero on success.  I've seen 1 when the command wasn't
                    // found, and -1 when the process was killed.
                    ExitCode = process.ExitCode;
                }
            } catch (Exception ex) {
                // This can happen if the command doesn't exist.
                ExitCode = -2000;
                Stdout = string.Empty;
                Stderr = "Failed to execute command: " + FullCommandLine + "\r\n" + ex.ToString();
                return;
            }

            Stdout = mStdout.ToString();
            Stderr = mStderr.ToString();
        }

        /// <summary>
        /// Opens a tab in the system web browser for the specified URL.
        /// 
        /// NOTE: on Windows 10, as of 2018/09/02, this loses the anchor (the "#thing" at the
        /// end of the URL).  Chasing through various stackoverflow posts, it appears the
        /// only way around this is to invoke the specific browser (which you dredge out of
        /// the Registry).
        /// </summary>
        public static void OpenUrl(string url) {
            // See https://stackoverflow.com/a/43232486/294248
            // The idea is to see if Start() will just do it, and if it doesn't then fall
            // back on something platform-specific.  I don't know if this is actually
            // necessary -- I suspect Mono will do the right thing.
            try {
                Process.Start(url);
            } catch {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {
                        CreateNoWindow = true
                    });
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                    Process.Start("xdg-open", url);
                } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                    Process.Start("open", url);
                } else {
                    throw;
                }
            }
        }
    }
}
