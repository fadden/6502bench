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
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;

using Asm65;
using CommonUtil;
using SourceGen.AsmGen;

namespace SourceGen.Tests {
    /// <summary>
    /// Source code generation regression test.
    ///
    /// The generator is tested in two ways: (1) by comparing the output to known-good
    /// sources, and (2) by running it through the assembler.  Assembling the sources is
    /// important to ensure that we don't get bad sources in the "known-good" set.
    ///
    /// This does not take assembler version into account, so it will not be helpful for
    /// monitoring compatibility with old versions of assemblers.
    /// </summary>
    public class GenTest {
        private const string TEST_DIR_NAME = "SGTestData";
        private const string EXPECTED_DIR_NAME = "Expected";

        //private static char[] sInvalidChars = new char[] { '.', '_' };
        private const string TestCasePattern = @"^\d\d\d\d\d-[A-Za-z0-9-]+$";
        private static Regex sTestCaseRegex = new Regex(TestCasePattern);

        /// <summary>
        /// Whitelist of removable names for ScrubWorkDirectory().
        /// </summary>
        private static string[] sScrubList = new string[] {
            "_FileInformation.txt",             // created by Merlin 32
            "error_output.txt",                 // created by Merlin 32 (only when errors found)
        };

        /// <summary>
        /// Test result.  One of these will be created for every {test-case, assembler} pair.
        /// </summary>
        public class GenTestResults {
            public string PathName { get; private set; }
            public string FileName { get; private set; }
            public AssemblerInfo.Id AsmId { get; private set; }

            public FileLoadReport ProjectLoadReport { get; set; }

            public bool GenerateOkay { get; set; }
            public bool AssembleOkay { get; set; }
            public AssemblerResults AsmResults { get; set; }    // may be null

            public TaskTimer Timer { get; set; }                // may be null

            public GenTestResults(string pathName, AssemblerInfo.Id asmId) {
                PathName = pathName;
                AsmId = asmId;

                FileName = Path.GetFileName(pathName);
            }

            // Return a string for use in the UI combo box.
            public override string ToString() {
                return (GenerateOkay && AssembleOkay ? "OK" : "FAIL") + " - " +
                    FileName + " - " + AsmId.ToString();
            }
        }

        /// <summary>
        /// If true, don't scrub directories.
        /// </summary>
        public bool RetainOutput { get; set; }

        /// <summary>
        /// Directory with test cases.
        /// </summary>
        private string mTestDir;

        private BackgroundWorker mWorker;

        private List<GenTestResults> mResults = new List<GenTestResults>();


        /// <summary>
        /// Runs generate/assemble test cases.  Main entry point.
        /// </summary>
        /// <param name="worker">Background worker object from dialog box.</param>
        public List<GenTestResults> Run(BackgroundWorker worker) {
            Debug.Assert(mWorker == null);  // don't re-use object

            mWorker = worker;
            string runtimeDir = RuntimeDataAccess.GetDirectory();
            mTestDir = Path.Combine(Path.GetDirectoryName(runtimeDir), TEST_DIR_NAME);

            if (!Directory.Exists(mTestDir)) {
                ReportErrMsg("Regression test directory not found: " + mTestDir);
                ReportFailure();
                return null;
            }

            List<string> testCases = new List<string>();
            foreach (string pathName in Directory.EnumerateFiles(mTestDir)) {
                // Filter out everything that doesn't look like "1000-nifty-test".  We
                // want to ignore .dis65 files and assembler output (which has the name
                // of the assembler following an underscore).
                string fileName = Path.GetFileName(pathName);
                MatchCollection matches = sTestCaseRegex.Matches(fileName);
                if (matches.Count == 0) {
                    //ReportProgress("Ignoring " + fileName + "\r\n", Color.Gray);
                    continue;
                }

                ReportProgress("Found " + fileName + "\r\n");
                testCases.Add(pathName);
            }

            ReportProgress("Processing " + testCases.Count + " test cases...\r\n");
            DateTime startWhen = DateTime.Now;

            int successCount = 0;
            int asmFailCount = 0;
            foreach (string pathName in testCases) {
                if (GenerateAndAssemble(pathName, out bool someAsmFailed)) {
                    successCount++;
                }
                if (someAsmFailed) {
                    asmFailCount++;
                }

                if (worker.CancellationPending) {
                    ReportProgress("\r\nCancelled.\r\n", Colors.Red);
                    return mResults;
                }
            }

            DateTime endWhen = DateTime.Now;

            if (successCount == testCases.Count) {
                ReportProgress(string.Format("All " + testCases.Count +
                    " tests passed in {0:N3} sec\r\n",
                    (endWhen - startWhen).TotalSeconds), Colors.Green);
            } else {
                ReportProgress(successCount + " of " + testCases.Count + " tests passed\r\n",
                    Colors.OrangeRed);
                ReportProgress(asmFailCount + " tests reported assembler failures\r\n");
            }

            PrintAsmVersions();

            return mResults;
        }

        private void ReportProgress(string msg) {
            mWorker.ReportProgress(0, new ProgressMessage(msg));
        }

        private void ReportProgress(string msg, Color color) {
            mWorker.ReportProgress(0, new ProgressMessage(msg, color));
        }

        private void ReportErrMsg(string msg) {
            ReportProgress(" [" + msg + "] ", Colors.Blue);
        }

        private void ReportSuccess() {
            ReportProgress(" success\r\n", Colors.Green);
        }

        private void ReportFailure() {
            ReportProgress(" failed\r\n", Colors.Red);
        }

        /// <summary>
        /// Extracts the test's number from the pathname.
        /// </summary>
        /// <param name="pathName">Full or partial path to test file.</param>
        /// <returns>Test number.</returns>
        private int GetTestNum(string pathName) {
            // Should always succeed if pathName matched on our regex.
            string fileName = Path.GetFileName(pathName);
            return int.Parse(fileName.Substring(0, 5));
        }

        /// <summary>
        /// Determines the desired CPU from the test case number.
        /// </summary>
        /// <param name="testNum">Test number.</param>
        /// <returns>CPU type enumeration value.</returns>
        private CpuDef.CpuType GetCpuTypeFromNum(int testNum) {
            switch (testNum % 10) {
                case 0:     return CpuDef.CpuType.Cpu6502;
                case 1:     return CpuDef.CpuType.Cpu65C02;
                case 2:     return CpuDef.CpuType.Cpu65816;
                case 3:     return CpuDef.CpuType.CpuW65C02;
                default:    return CpuDef.CpuType.CpuUnknown;
            }
        }

        /// <summary>
        /// Generates source code for the specified test case, assembles it, and compares
        /// the output of both steps to expected values.  The process is repeated for every
        /// known assembler.
        ///
        /// If an assembler is known but not configured, the assembly step is skipped, and
        /// does not count as a failure.
        /// </summary>
        /// <param name="pathName">Full path to test case.</param>
        /// <param name="someAsmFailed">Set to true if one or more assemblers reported an error,
        ///   or the assembled binary did not match.  Useful when running the tests against an
        ///   older version of an assembler for which the generated code does not match the
        ///   expected text, but we can still evaluate correctness.</param>
        /// <returns>True if all assemblers worked as expected.</returns>
        private bool GenerateAndAssemble(string pathName, out bool someAsmFailed) {
            someAsmFailed = false;

            ReportProgress(Path.GetFileName(pathName) + "...\r\n");

            // Create DisasmProject object, either as a new project for a plain data file,
            // or from a project file.
            DisasmProject project = InstantiateProject(pathName,
                out FileLoadReport projectLoadReport);
            if (project == null) {
                ReportFailure();
                return false;
            }

            int testNum = GetTestNum(pathName);

            // Create a temporary directory to work in.
            string workDir = CreateWorkDirectory(pathName);
            if (string.IsNullOrEmpty(workDir)) {
                ReportFailure();
                project.Cleanup();
                return false;
            }

            AppSettings settings = CreateNormalizedSettings();
            ApplyProjectSettings(settings, project);

            // Iterate through all known assemblers.
            bool didFail = false;
            int numAsmFailures = 0;
            foreach (AssemblerInfo.Id asmId in
                    (AssemblerInfo.Id[])Enum.GetValues(typeof(AssemblerInfo.Id))) {
                if (asmId == AssemblerInfo.Id.Unknown) {
                    continue;
                }

                string fileName = Path.GetFileName(pathName);
                TaskTimer timer = new TaskTimer();
                timer.StartTask("Full Test Duration");

                // Create results object and add it to the list.  We'll add stuff to it for
                // as far as we get.
                GenTestResults results = new GenTestResults(pathName, asmId);
                mResults.Add(results);
                results.ProjectLoadReport = projectLoadReport;

                // Generate source code.
                ReportProgress("  " + asmId.ToString() + " generate...");
                IGenerator gen = AssemblerInfo.GetGenerator(asmId);
                if (gen == null) {
                    ReportErrMsg("generator unavailable");
                    ReportProgress("\r\n");
                    //didFail = true;
                    continue;
                }
                timer.StartTask("Generate Source");
                gen.Configure(project, workDir, fileName,
                    AssemblerVersionCache.GetVersion(asmId), settings);
                GenerationResults genResults = gen.GenerateSource(mWorker);
                timer.EndTask("Generate Source");
                if (mWorker.CancellationPending) {
                    // The generator will stop early if a cancellation is requested.  If we
                    // don't break here, the compare function will report a failure, which
                    // isn't too problematic but looks funny.
                    break;
                }

                ReportProgress(" verify...");
                timer.StartTask("Compare Source to Expected");
                bool match = CompareGeneratedToExpected(pathName, genResults.PathNames);
                timer.EndTask("Compare Source to Expected");
                if (match) {
                    ReportSuccess();
                    results.GenerateOkay = true;
                } else {
                    ReportFailure();
                    didFail = true;

                    // The fact that it doesn't match the expected sources doesn't mean it's
                    // invalid.  Go ahead and try to build it.
                    //continue;
                }

                // Generate binary includes.  These are not verified in the "expected source"
                // section because we'll do the necessary check in the binary diff.
                if (!BinaryInclude.PrepareList(genResults.BinaryIncludes, workDir,
                        out string failMsg)) {
                    ReportErrMsg("Failed processing binary includes: " + failMsg);
                    ReportProgress("\r\n");
                    didFail = true;
                } else {
                    foreach (BinaryInclude.Excision exc in genResults.BinaryIncludes) {
                        if (!BinaryInclude.GenerateOutputFile(exc, project.FileData,
                                out string failMsg2)) {
                            ReportErrMsg("Failed processing binary include at +" +
                                exc.Offset.ToString("x6") + ": " + failMsg2);
                            ReportProgress("\r\n");
                            didFail = true;
                            break;
                        }
                    }
                }

                // Assemble code.
                ReportProgress("  " + asmId.ToString() + " assemble...");
                IAssembler asm = AssemblerInfo.GetAssembler(asmId);
                if (asm == null) {
                    ReportErrMsg("assembler unavailable");
                    ReportProgress("\r\n");
                    continue;
                }

                timer.StartTask("Assemble Source");
                asm.Configure(genResults, workDir);
                AssemblerResults asmResults = asm.RunAssembler(mWorker);
                timer.EndTask("Assemble Source");
                if (asmResults == null) {
                    ReportErrMsg("unable to run assembler");
                    ReportFailure();
                    didFail = true;
                    continue;
                }
                results.AsmResults = asmResults;
                numAsmFailures++;       // assume failure until the end
                if (asmResults.ExitCode != 0) {
                    ReportErrMsg("assembler returned code=" + asmResults.ExitCode);
                    ReportFailure();
                    didFail = true;
                    continue;
                }

                ReportProgress(" verify...");
                timer.StartTask("Compare Binary to Expected");
                FileInfo fi = new FileInfo(asmResults.OutputPathName);
                if (!fi.Exists) {
                    // This can happen if the assembler fails to generate output but doesn't
                    // report an error code (e.g. Merlin 32 in certain situations).
                    ReportErrMsg("asm output missing");
                    ReportFailure();
                    didFail = true;
                    continue;
                } else if (fi.Length != project.FileData.Length) {
                    ReportErrMsg("asm output mismatch: length is " + fi.Length + ", expected " +
                        project.FileData.Length);
                    ReportFailure();
                    didFail = true;
                    continue;
                } else if (!FileUtil.CompareBinaryFile(project.FileData, asmResults.OutputPathName,
                        out int badOffset, out byte badFileVal)) {
                    ReportErrMsg("asm output mismatch: offset +" + badOffset.ToString("x6") +
                        " has value $" + badFileVal.ToString("x2") + ", expected $" +
                        project.FileData[badOffset].ToString("x2"));
                    ReportFailure();
                    didFail = true;
                    continue;
                }
                timer.EndTask("Compare Binary to Expected");

                // Victory!
                results.AssembleOkay = true;
                numAsmFailures--;
                ReportSuccess();

                timer.EndTask("Full Test Duration");
                results.Timer = timer;

                // We don't scrub the directory on success at this point.  We could, but we'd
                // need to remove only those files associated with the currently assembler.
                // Otherwise, a failure followed by a success would wipe out the unsuccessful
                // temporaries.
            }

            // If something failed, leave the bits around for examination.  Otherwise, try to
            // remove the directory and all its contents.
            if (!didFail && !RetainOutput) {
                ScrubWorkDirectory(workDir, testNum);
                RemoveWorkDirectory(workDir);
            }

            project.Cleanup();

            someAsmFailed = (numAsmFailures != 0);
            return !didFail;
        }

        private void PrintAsmVersions() {
            ReportProgress("\nTested assemblers:");
            IEnumerator<AssemblerInfo> iter = AssemblerInfo.GetInfoEnumerator();
            while (iter.MoveNext()) {
                AssemblerInfo info = iter.Current;
                AssemblerVersion version = AssemblerVersionCache.GetVersion(info.AssemblerId);
                ReportProgress("  " + info.Name + " v" + version.VersionStr);
            }
            ReportProgress("\n");
        }

        /// <summary>
        /// Gets a copy of the AppSettings with a standard set of formatting options (e.g. lower
        /// case for everything).
        /// </summary>
        /// <returns>New app settings object.</returns>
        private AppSettings CreateNormalizedSettings() {
            AppSettings settings = AppSettings.Global.GetCopy();

            // Override all asm formatting options.  We can ignore ShiftBeforeAdjust and the
            // pseudo-op names because those are set by the generators.
            settings.SetBool(AppSettings.FMT_UPPER_HEX_DIGITS, false);
            settings.SetBool(AppSettings.FMT_UPPER_OP_MNEMONIC, false);
            settings.SetBool(AppSettings.FMT_UPPER_PSEUDO_OP_MNEMONIC, false);
            settings.SetBool(AppSettings.FMT_UPPER_OPERAND_A, true);
            settings.SetBool(AppSettings.FMT_UPPER_OPERAND_S, true);
            settings.SetBool(AppSettings.FMT_UPPER_OPERAND_XY, false);
            settings.SetBool(AppSettings.FMT_ADD_SPACE_FULL_COMMENT, false);
            settings.SetInt(AppSettings.FMT_OPERAND_WRAP_LEN, 64);

            // Don't show the assembler ident line.  You can make a case for this being
            // mandatory, since the generated code is only guaranteed to work with the
            // assembler for which it was targeted, but I expect we'll quickly get to a
            // place where we don't have to work around assembler bugs, and this will just
            // become a nuisance.
            settings.SetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, false);

            // Don't break lines with long labels.  That way we can redefine "long"
            // without breaking our tests.  (This is purely cosmetic.)
            settings.SetEnum(AppSettings.SRCGEN_LABEL_NEW_LINE,
                GenCommon.LabelPlacement.PreferSameLine);

            // This could be on or off.  Off seems less distracting.
            settings.SetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS, false);

            settings.SetBool(AppSettings.SRCGEN_OMIT_IMPLIED_ACC_OPERAND, false);

            IEnumerator<AssemblerInfo> iter = AssemblerInfo.GetInfoEnumerator();
            while (iter.MoveNext()) {
                AssemblerInfo.Id asmId = iter.Current.AssemblerId;
                AssemblerConfig curConfig =
                    AssemblerConfig.GetConfig(settings, asmId);
                AssemblerConfig defConfig =
                    AssemblerInfo.GetAssembler(asmId).GetDefaultConfig();

                // Merge the two together.  We want the default assembler config for most
                // things, but the executable path from the current config.
                defConfig.ExecutablePath = curConfig.ExecutablePath;

                // Write it into the test settings.
                AssemblerConfig.SetConfig(settings, asmId, defConfig);
            }

            return settings;
        }

        /// <summary>
        /// Applies app setting overrides that were specified in the project properties.
        /// </summary>
        private void ApplyProjectSettings(AppSettings settings, DisasmProject project) {
            // We could probably make this a more general mechanism, but that would strain
            // things a bit, since we need to know the settings name, bool/int/string, and
            // desired value.  Easier to just have a set of named features.
            const string ENABLE_LABEL_NEWLINE = "__ENABLE_LABEL_NEWLINE";
            const string ENABLE_ALL_LABEL_NEWLINE = "__ENABLE_ALL_LABEL_NEWLINE";
            const string ENABLE_CYCLE_COUNTS = "__ENABLE_CYCLE_COUNTS";

            if (project.ProjectProps.ProjectSyms.ContainsKey(ENABLE_LABEL_NEWLINE)) {
                settings.SetEnum(AppSettings.SRCGEN_LABEL_NEW_LINE,
                    GenCommon.LabelPlacement.SplitIfTooLong);
            }
            if (project.ProjectProps.ProjectSyms.ContainsKey(ENABLE_ALL_LABEL_NEWLINE)) {
                settings.SetEnum(AppSettings.SRCGEN_LABEL_NEW_LINE,
                    GenCommon.LabelPlacement.PreferSeparateLine);
            }
            if (project.ProjectProps.ProjectSyms.ContainsKey(ENABLE_CYCLE_COUNTS)) {
                settings.SetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS, true);
            }
        }

        private DisasmProject InstantiateProject(string dataPathName,
                out FileLoadReport projectLoadReport) {
            DisasmProject project = new DisasmProject();
            // always use AppDomain sandbox

            projectLoadReport = null;

            int testNum = GetTestNum(dataPathName);
            CpuDef.CpuType cpuType = GetCpuTypeFromNum(testNum);

            if (testNum < 20000) {
                // create new disasm project for data file
                byte[] fileData;
                try {
                    fileData = LoadDataFile(dataPathName);
                } catch (Exception ex) {
                    ReportErrMsg(ex.Message);
                    return null;
                }

                project.Initialize(fileData.Length);
                project.ProjectProps.CpuType = cpuType;
                project.ProjectProps.IncludeUndocumentedInstr = true;
                project.ProjectProps.TwoByteBrk = false;
                project.UpdateCpuDef();
                project.PrepForNew(fileData, Path.GetFileName(dataPathName));
                // no platform symbols to load
            } else {
                // deserialize project file, failing if we can't find it
                string projectPathName = dataPathName + ProjectFile.FILENAME_EXT;
                if (!ProjectFile.DeserializeFromFile(projectPathName,
                        project, out projectLoadReport)) {
                    ReportErrMsg(projectLoadReport.Format());
                    return null;
                }

                byte[] fileData;
                try {
                    fileData = LoadDataFile(dataPathName);
                } catch (Exception ex) {
                    ReportErrMsg(ex.Message);
                    return null;
                }

                FileLoadReport unused = new FileLoadReport("test");
                project.SetFileData(fileData, Path.GetFileName(dataPathName), ref unused);
                project.ProjectPathName = projectPathName;
                string extMsgs = project.LoadExternalFiles();
                if (!string.IsNullOrEmpty(extMsgs)) {
                    ReportErrMsg(extMsgs);
                    // keep going
                }

                if (project.ProjectProps.CpuType != cpuType) {
                    ReportErrMsg("Mismatch CPU type for test " + testNum + ": project wants " +
                        project.ProjectProps.CpuType);
                    // keep going
                }
            }

            TaskTimer genTimer = new TaskTimer();
            DebugLog genLog = new DebugLog();
            genLog.SetMinPriority(DebugLog.Priority.Silent);
            project.Analyze(UndoableChange.ReanalysisScope.CodeAndData, genLog, genTimer);

            return project;
        }

        /// <summary>
        /// Loads the test case data file.
        ///
        /// Throws an exception on failure.
        /// </summary>
        /// <param name="pathName">Full path to test case data file.</param>
        /// <returns>File contents.</returns>
        private byte[] LoadDataFile(string pathName) {
            byte[] fileData;

            using (FileStream fs = File.Open(pathName, FileMode.Open, FileAccess.Read)) {
                Debug.Assert(fs.Length <= DisasmProject.MAX_DATA_FILE_SIZE);
                fileData = new byte[fs.Length];
                int actual = fs.Read(fileData, 0, (int)fs.Length);
                if (actual != fs.Length) {
                    // Not expected -- should be able to read the entire file in one shot.
                    throw new Exception(Res.Strings.OPEN_DATA_PARTIAL_READ);
                }
            }
            return fileData;
        }

        /// <summary>
        /// Creates a work directory for the specified test case.  The new directory will be
        /// created in the same directory as the test, and named after it.
        ///
        /// If the directory already exists, the previous contents will be scrubbed.
        ///
        /// If the file already exists but isn't a directory, this will fail.
        /// </summary>
        /// <param name="pathName">Test case path name.</param>
        /// <returns>Path of work directory, or null if creation failed.</returns>
        private string CreateWorkDirectory(string pathName) {
            string baseDir = Path.GetDirectoryName(pathName);
            int testNum = GetTestNum(pathName);
            string workDirName = "tmp" + testNum.ToString();
            string workDirPath = Path.Combine(baseDir, workDirName);
            if (Directory.Exists(workDirPath)) {
                ScrubWorkDirectory(workDirPath, testNum);
            } else if (File.Exists(workDirPath)) {
                ReportErrMsg("file '" + workDirPath + "' exists, not directory");
                return null;
            } else {
                try {
                    Directory.CreateDirectory(workDirPath);
                } catch (Exception ex) {
                    ReportErrMsg(ex.Message);
                    return null;
                }
            }
            return workDirPath;
        }

        /// <summary>
        /// Removes the contents of a temporary work directory.  Only files that we believe
        /// to be products of the generator or assembler are removed.
        /// </summary>
        /// <param name="workDir">Full pathname of work directory.</param>
        /// <param name="testNum">Test number, used to evaluate files for removal.</param>
        private void ScrubWorkDirectory(string workDir, int testNum) {
            string checkString = testNum.ToString();
            if (checkString.Length != 5) {
                Debug.Assert(false);
                return;
            }

            // Remove any subdirectories that match the pattern, e.g. for binary includes.
            foreach (string pathName in Directory.EnumerateDirectories(workDir)) {
                string fileName = Path.GetFileName(pathName);
                if (fileName.Contains(checkString)) {
                    ScrubWorkDirectory(pathName, testNum);
                    try {
                        Directory.Delete(pathName);
                    } catch (Exception ex) {
                        ReportErrMsg("unable to remove dir '" + fileName + "': " + ex.Message);
                    }
                }
            }

            // Remove all matching files.
            foreach (string pathName in Directory.EnumerateFiles(workDir)) {
                bool doRemove = false;
                string fileName = Path.GetFileName(pathName);
                if (fileName.Contains(checkString)) {
                    doRemove = true;
                } else {
                    foreach (string str in sScrubList) {
                        if (fileName == str) {
                            doRemove = true;
                        }
                    }
                }

                if (!doRemove) {
                    ReportErrMsg("not removing '" + fileName + "'");
                    continue;
                } else {
                    try {
                        File.Delete(pathName);
                        //Debug.WriteLine("removed " + pathName);
                    } catch (Exception ex) {
                        ReportErrMsg("unable to remove '" + fileName + "': " + ex.Message);
                        // don't stop -- keep trying to remove things
                    }
                }
            }
        }

        private void RemoveWorkDirectory(string workDir) {
            try {
                Directory.Delete(workDir);
            } catch (Exception ex) {
                ReportErrMsg("unable to remove work dir: " + ex.Message);
            }
        }

        /// <summary>
        /// Compares each file in genFileNames to the corresponding file in Expected.
        /// </summary>
        /// <param name="pathName">Full pathname of test case.</param>
        /// <param name="genPathNames">List of file names from source generator.</param>
        /// <returns></returns>
        private bool CompareGeneratedToExpected(string pathName, List<string> genPathNames) {
            string expectedDir = Path.Combine(Path.GetDirectoryName(pathName), EXPECTED_DIR_NAME);

            foreach (string path in genPathNames) {
                string fileName = Path.GetFileName(path);
                string compareName = Path.Combine(expectedDir, fileName);

                if (!File.Exists(compareName)) {
                    // File was generated unexpectedly.
                    ReportErrMsg("file '" + fileName + "' not found in " + EXPECTED_DIR_NAME);
                    return false;
                }

                // Compare the file contents as lines of text.  The files may use different
                // line terminators (e.g. LF vs. CRLF), so we can't use file length as a
                // factor.
                if (!FileUtil.CompareTextFiles(path, compareName, out int firstDiffLine,
                        out string line1, out string line2)) {
                    ReportErrMsg("file '" + fileName + "' differs on line " + firstDiffLine);

                    Debug.WriteLine("Difference on line " + firstDiffLine);
                    Debug.WriteLine(" generated: " + line1);
                    Debug.WriteLine(" expected : " + line2);
                    return false;
                }
            }

            // NOTE: to be thorough, we should check to see if a file exists in Expected
            // that doesn't exist in the work directory.  This is slightly more awkward since
            // Expected is a big pile of everything, but we should be able to do it by
            // filtering filenames with the test number.

            return true;
        }
    }
}
