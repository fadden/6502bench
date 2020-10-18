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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace MakeDist {
    public class FileCopier {
        private const string SOURCEGEN_DIRNAME = "SourceGen";

        /// <summary>
        /// Type of build to gather files for.
        /// </summary>
        public enum BuildType { Unknown, Release, Debug };

        private enum SourceFileSpec {
            Unknown = 0,
            All,
            List,
            RegressionTests,
            AsmSources,
            NotBins,
        }

        private class CopySpec {
            public string SourceDir { get; private set; }
            public string DestDir { get; private set; }
            public SourceFileSpec FileSpec { get; private set; }
            public bool IsRecursive { get; private set; }
            public string[] FileList { get; private set; }

            public CopySpec(string srcDir, string dstDir, SourceFileSpec spec, bool recursive,
                    string[] fileList) {
                SourceDir = srcDir;
                DestDir = dstDir;
                FileSpec = spec;
                IsRecursive = recursive;
                FileList = fileList;
            }
        }

        private static CopySpec[] sMainSpec = {
            new CopySpec(".", ".",
                SourceFileSpec.List, false, new string[] { "README.md" }),
            new CopySpec("Asm65/bin/{BUILD_TYPE}/netstandard2.0/", ".",
                SourceFileSpec.List, false, new string[] { "Asm65.dll" }),
            new CopySpec("CommonUtil/bin/{BUILD_TYPE}/netstandard2.0/", ".",
                SourceFileSpec.List, false, new string[] { "CommonUtil.dll" }),
            //new CopySpec("CommonWinForms/bin/{BUILD_TYPE}/", ".",
            //    SourceFileSpec.List, false, new string[] { "CommonWinForms.dll" }),
            new CopySpec("CommonWPF/bin/{BUILD_TYPE}/", ".",
                SourceFileSpec.List, false, new string[] { "CommonWPF.dll" }),
            new CopySpec("PluginCommon/bin/{BUILD_TYPE}/netstandard2.0/", ".",
                SourceFileSpec.List, false, new string[] { "PluginCommon.dll" }),
            new CopySpec("SourceGen/bin/{BUILD_TYPE}/", ".",
                SourceFileSpec.List, false, new string[] { "SourceGen.exe" }),
            new CopySpec("SourceGen/RuntimeData", "RuntimeData",
                SourceFileSpec.NotBins, true, null),
            new CopySpec("SourceGen/Examples", "Examples",
                SourceFileSpec.All, true, null),
        };
        private static CopySpec[] sTestSpec = {
            new CopySpec("SourceGen/SGTestData", "SGTestData",
                SourceFileSpec.RegressionTests, false, null),
            new CopySpec("SourceGen/SGTestData", "SGTestData",
                SourceFileSpec.List, false, new string[] { "README.md" }),
            new CopySpec("SourceGen/SGTestData/Expected", "SGTestData/Expected",
                SourceFileSpec.AsmSources, false, null),
            new CopySpec("SourceGen/SGTestData/Source", "SGTestData/Source",
                SourceFileSpec.AsmSources, false, null),
            new CopySpec("SourceGen/SGTestData/FunkyProjects", "SGTestData/FunkyProjects",
                SourceFileSpec.All, false, null),
        };

        private static string sBasePath;

        // We want all of the regression test binaries, plus the .sym65, .dis65, and .cs,
        // but nothing with an underscore in the part before the extension.
        private const string TestCasePattern = @"^\d\d\d\d\d-[A-Za-z0-9-]+(\..*)?$";
        private static Regex sTestCaseRegex = new Regex(TestCasePattern);


        private BuildType mBuildType;
        private bool mCopyTestFiles;
        private BackgroundWorker mWorker;


        public FileCopier(BuildType buildType, bool copyTestFiles) {
            mBuildType = buildType;
            mCopyTestFiles = copyTestFiles;
        }

        private void ReportProgress(string msg) {
            mWorker.ReportProgress(0, new CopyProgress.ProgressMessage(msg + "\r\n"));
            // This allows the RichTextBox, which appears to be updated by a low-priority thread,
            // to update while we run.  If we don't sleep, the window doesn't update until the
            // entire run has finished.  (Win10, WPF4.5, SSD drive)
            System.Threading.Thread.Sleep(5);
        }

        private void ReportProgress(string msg, Color color) {
            mWorker.ReportProgress(0, new CopyProgress.ProgressMessage(msg + "\r\n", color));
        }

        private void ReportErrMsg(string msg) {
            ReportProgress(msg + "\r\n", Colors.Red);
        }

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="worker">Background task interface object.</param>
        /// <returns>True on success.</returns>
        public bool CopyAllFiles(BackgroundWorker worker) {
            mWorker = worker;

            ReportProgress("Preparing... build type is " + mBuildType + ", test files are " +
                (mCopyTestFiles ? "" : "NOT ") + "included.");
            ReportProgress(""); // the first CRLF is ignored by RichTextBox??

            string buildStr = mBuildType.ToString();
            string basePath = FindBasePath();
            Debug.Assert(basePath != null);
            string distPath = Path.Combine(basePath, "DIST_" + buildStr);

            // TODO(maybe): recursively delete distPath

            if (!CopySpecList(sMainSpec, basePath, distPath, buildStr)) {
                return false;
            }
            if (mCopyTestFiles) {
                if (!CopySpecList(sTestSpec, basePath, distPath, buildStr)) {
                    return false;
                }
            }

            ReportProgress("Success", Colors.Green);
            return true;
        }

        private bool CopySpecList(CopySpec[] specList, string basePath, string distPath,
                string buildStr) {
            foreach (CopySpec cs in specList) {
                string srcDir = Path.GetFullPath(Path.Combine(basePath,
                    cs.SourceDir.Replace("{BUILD_TYPE}", buildStr)));
                string dstDir = Path.GetFullPath(Path.Combine(distPath, cs.DestDir));

                ReportProgress("Scanning [" + cs.FileSpec + "] " + srcDir);

                if (!CopyBySpec(srcDir, dstDir, cs.FileSpec, cs.FileList, cs.IsRecursive)) {
                    return false;
                }
            }

            return true;
        }

        private bool CopyBySpec(string srcDir, string dstDir, SourceFileSpec sfspec,
                string[] specFileList, bool isRecursive) {
            if (!EnsureDirectoryExists(dstDir)) {
                return false;
            }

            string[] fileList;
            if (sfspec == SourceFileSpec.List) {
                fileList = specFileList;
            } else {
                fileList = Directory.GetFiles(srcDir);
            }

            foreach (string str in fileList) {
                // Spec list is filenames, GetFiles is paths; convert to simple filename.
                string fileName = Path.GetFileName(str);

                switch (sfspec) {
                    case SourceFileSpec.All:
                    case SourceFileSpec.List:
                        // keep all
                        break;
                    case SourceFileSpec.NotBins:
                        // Mostly this means "skip obj and bin dirs", which happens later.
                        // Rather than specify everything we do want, just omit this one thing.
                        if (fileName == "RuntimeData.csproj") {
                            continue;
                        }
                        break;
                    case SourceFileSpec.AsmSources:
                        // Need the sources and the ca65 config files.
                        if (!(fileName.ToUpperInvariant().EndsWith(".S") ||
                                !fileName.ToUpperInvariant().EndsWith("_cc65.cfg"))) {
                            continue;
                        }
                        break;
                    case SourceFileSpec.RegressionTests:
                        MatchCollection matches = sTestCaseRegex.Matches(fileName);
                        if (matches.Count != 1) {
                            continue;
                        }
                        // Skip project files. Could probably do this with regex... but why.
                        if (fileName.StartsWith("1") && fileName.EndsWith(".dis65")) {
                            continue;
                        }
                        break;
                    default:
                        throw new Exception("Unsupported spec " + sfspec);
                }

                string srcPath = Path.Combine(srcDir, fileName);
                string dstPath = Path.Combine(dstDir, fileName);
                if (!CopyFile(srcPath, dstPath)) {
                    return false;
                }
            }

            if (isRecursive) {
                string[] dirList = Directory.GetDirectories(srcDir);

                foreach (string str in dirList) {
                    string dirFileName = Path.GetFileName(str);
                    if (sfspec == SourceFileSpec.NotBins &&
                            (dirFileName == "obj" || dirFileName == "bin")) {
                        continue;
                    }

                    if (!CopyBySpec(Path.Combine(srcDir, dirFileName),
                            Path.Combine(dstDir, dirFileName),
                            sfspec, specFileList, isRecursive)) {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool EnsureDirectoryExists(string dirPath) {
            if (Directory.Exists(dirPath)) {
                return true;
            }
            if (File.Exists(dirPath)) {
                ReportErrMsg("File exists and is not directory: " + dirPath);
                return false;
            }
            try {
                Directory.CreateDirectory(dirPath);
                ReportProgress("  Created " + dirPath);
            } catch (Exception ex) {
                ReportErrMsg("Failed creating directory " + dirPath + ": " + ex.Message);
                return false;
            }
            return true;
        }

        private bool CopyFile(string srcPath, string dstPath) {
            // Poll cancel button.
            if (mWorker.CancellationPending) {
                ReportErrMsg("Cancel\r\n");
                return false;
            }

            ReportProgress("  Copy " + srcPath + " --> " + dstPath);

            try {
                File.Copy(srcPath, dstPath, true);
            } catch (Exception ex) {
                ReportErrMsg("Failed: " + ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the base directory of the 6502bench installation.
        /// </summary>
        /// <returns></returns>
        private static string FindBasePath() {
            if (sBasePath != null) {
                return sBasePath;
            }

            string exeName = Process.GetCurrentProcess().MainModule.FileName;
            string baseDir = Path.GetDirectoryName(exeName);
            if (string.IsNullOrEmpty(baseDir)) {
                return null;
            }

            string tryPath;

            // Use the SourceGen directory as a sentinel.
            tryPath = Path.Combine(baseDir, SOURCEGEN_DIRNAME);
            if (Directory.Exists(tryPath)) {
                sBasePath = Path.GetFullPath(tryPath);
                return sBasePath;
            }

            string upThree = Path.GetDirectoryName(
                Path.GetDirectoryName(Path.GetDirectoryName(baseDir)));
            tryPath = Path.Combine(upThree, SOURCEGEN_DIRNAME);
            if (Directory.Exists(tryPath)) {
                sBasePath = Path.GetFullPath(upThree);
                return sBasePath;
            }

            Debug.WriteLine("Unable to find RuntimeData dir near " + exeName);
            return null;
        }
    }
}
