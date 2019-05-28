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

namespace SourceGenWPF.AsmGen {
    /// <summary>
    /// Common interface for executing assemblers.
    /// </summary>
    public interface IAssembler {
        /// <summary>
        /// Gets identification strings for the executable.  These are used when browsing for
        /// the assembler binary.
        /// </summary>
        /// <param name="humanName">Human-readable name to show in the "open" dialog.</param>
        /// <param name="exeName">Name of executable to find, without ".exe".</param>
        void GetExeIdentifiers(out string humanName, out string exeName);

        /// <summary>
        /// Queries the assembler for its default configuration.
        /// </summary>
        /// <returns>Config object with default values.</returns>
        AssemblerConfig GetDefaultConfig();

        /// <summary>
        /// Queries the assembler for its version.  Assembler executable paths are queried from
        /// the global settings object.
        /// </summary>
        /// <returns>Assembler version info, or null if query failed.</returns>
        AssemblerVersion QueryVersion();

        /// <summary>
        /// Configures the object.  Pass in the list of pathnames returned by IGenerator.Run(),
        /// and the working directory to use for the shell command.
        /// </summary>
        /// <param name="pathNames">Assembler source pathnames.</param>
        /// <param name="workDirectory">Working directory for shell command.</param>
        void Configure(List<string> pathNames, string workDirectory);

        /// <summary>
        /// Executes the assembler.  Must call Configure() first.  Executed on background thread.
        /// </summary>
        /// <param name="worker">Async work object, used to report progress updates and
        ///   check for cancellation.</param>
        /// <returns>Execution results, or null on internal failure.</returns>
        AssemblerResults RunAssembler(BackgroundWorker worker);
    }

    /// <summary>
    /// Set of values returned by the assembler.
    /// </summary>
    public class AssemblerResults {
        public string CommandLine { get; private set; }
        public int ExitCode { get; private set; }
        public string Stdout { get; private set; }
        public string Stderr { get; private set; }
        public string OutputPathName { get; private set; }

        public AssemblerResults(string commandLine, int exitCode, string stdout, string stderr,
                string outputFile) {
            CommandLine = commandLine;
            ExitCode = exitCode;
            Stdout = stdout;
            Stderr = stderr;
            OutputPathName = outputFile;
        }
    }
}
