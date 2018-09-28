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
using System.IO;

namespace SourceGen {
    /// <summary>
    /// Facilitates access to the contents of the RuntimeData directory, which is located
    /// relative to the executable process pathname.
    /// </summary>
    public static class RuntimeDataAccess {
        private const string RUNTIME_DATA_FILENAME = "RuntimeData";

        private static string sBasePath;

        /// <summary>
        /// Attempts to locate the RuntimeData directory.  This will normally live in the
        /// place the executable starts from, but if we're debugging in Visual Studio then
        /// it'll be up a couple levels (e.g. from "bin/Release").
        /// </summary>
        /// <returns>Full path of the RuntimeData directory, or null on failure.</returns>
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

            tryPath = Path.Combine(baseDir, RUNTIME_DATA_FILENAME);
            if (Directory.Exists(tryPath)) {
                sBasePath = Path.GetFullPath(tryPath);
                return sBasePath;
            }

            string upTwo = Path.GetDirectoryName(Path.GetDirectoryName(baseDir));
            tryPath = Path.Combine(upTwo, RUNTIME_DATA_FILENAME);
            if (Directory.Exists(tryPath)) {
                sBasePath = Path.GetFullPath(tryPath);
                return sBasePath;
            }

            Debug.WriteLine("Unable to find RuntimeData dir near " + exeName);
            return null;
        }

        /// <summary>
        /// Returns the full path of the runtime data directory.
        /// </summary>
        /// <returns>Full path name, or null if the base path can't be found.</returns>
        public static string GetDirectory() {
            return FindBasePath();
        }

        /// <summary>
        /// Returns a full path, prefixing the file name with the base path name.
        /// </summary>
        /// <param name="fileName">Relative file name.</param>
        /// <returns>Full path name, or null if the base path can't be found.</returns>
        public static string GetPathName(string fileName) {
            string basePath = FindBasePath();
            if (basePath == null) {
                return null;
            }
            // Combine() joins "C:\foo" and "bar/ack" into "C:\foo\bar/ack", which works, but
            // looks funny.  GetFullPath() normalizes the directory separators.  The file
            // isn't required to exist, but if it does, path information must be available.
            // Given the nature of this class, that shouldn't be limiting.
            return Path.GetFullPath(Path.Combine(basePath, fileName));
        }

        /// <summary>
        /// Given the pathname of a file in the RuntimeData directory, strip off the
        /// directory.
        /// </summary>
        /// <param name="fullPath">Absolute pathname of file.  Assumed to be in canonical
        ///   form.</param>
        /// <returns>Partial path within the runtime data directory.</returns>
        public static string PartialPath(string fullPath) {
            string basePath = FindBasePath();
            if (basePath == null) {
                return null;
            }
            basePath += Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(basePath)) {
                return null;
            }
            return fullPath.Substring(basePath.Length);
        }
    }
}
