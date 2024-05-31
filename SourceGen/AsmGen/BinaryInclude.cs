/*
 * Copyright 2024 faddenSoft
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
using System.Diagnostics;
using System.IO;

namespace SourceGen.AsmGen {
    /// <summary>
    /// Helper functions for working with binary includes.
    /// </summary>
    public static class BinaryInclude {
        // Character placed at the start of a path as a check that the field holds what we
        // expect.  If we want to modify the structure of the string, e.g. to add or remove
        // additional fields, we can change the character to something else.
        private static char PATH_PREFIX_CHAR = '\u2191';  // UPWARDS ARROW

        /// <summary>
        /// Class to help when gathering up binary includes during asm gen.
        /// </summary>
        public class Excision {
            // Offset of start of region to excise.
            public int Offset { get; private set; }

            // Length of region to excise.
            public int Length { get; private set; }

            // Partial pathname of output file, as stored in the project.
            public string PathName { get; private set; }

            // Full output file path, initially null, set by PrepareList().
            public string FullPath { get; set; }

            public Excision(int offset, int length, string pathName) {
                Offset = offset;
                Length = length;
                PathName = pathName;
            }

            public override string ToString() {
                return "[Exc: offset=+" + Offset.ToString("x6") + " len=" + Length +
                    "path=\"" + PathName + "\" fullPath=\"" + FullPath + "\"]";
            }
        }

        /// <summary>
        /// Determines the full path of each binary include output file.  Checks for duplicates.
        /// Sorts the list by case-insensitive pathname.
        /// </summary>
        /// <param name="list">List of binary include excisions.</param>
        /// <param name="workDir">Working directory.</param>
        /// <param name="failMsg">On failure, a human-readable error message.</param>
        /// <returns>True on success.</returns>
        public static bool PrepareList(List<Excision> list, string workDir, out string failMsg) {
            // Normalize the pathname.  This is not expected to fail.
            string fullWorkDir = Path.GetFullPath(workDir);

            string oldCurrentDir = Environment.CurrentDirectory;
            try {
                Environment.CurrentDirectory = workDir;

                foreach (Excision exc in list) {
                    try {
                        exc.FullPath = Path.GetFullPath(exc.PathName);
                    } catch (Exception ex) {
                        failMsg = "unable to get full path for binary include \"" +
                            exc.PathName + "\": " + ex.Message;
                        return false;
                    }

                    if (!exc.FullPath.StartsWith(fullWorkDir)) {
                        failMsg = "binary include path for \"" + exc.PathName +
                            "\" resolved to parent directory";
                        return false;
                    }
                }
            } finally {
                Environment.CurrentDirectory = oldCurrentDir;
            }

            // Check for duplicates.  Assume filenames are case-insensitive.
            list.Sort(delegate (Excision a, Excision b) {
                return string.Compare(a.PathName, b.PathName,
                    StringComparison.InvariantCultureIgnoreCase);
            });
            string prev = null;
            foreach (Excision exc in list) {
                if (prev != null && exc.FullPath == prev) {
                    failMsg = "found multiple binary includes that output to \"" + prev + "\"";
                    return false;
                }
                prev = exc.FullPath;
            }

            failMsg = string.Empty;
            return true;
        }

        /// <summary>
        /// Generates the output file with the binary include data.
        /// </summary>
        /// <param name="exc">Binary include object, with full pathname computed.</param>
        /// <param name="data">Project data array.</param>
        /// <param name="failMsg">On failure, a human-readable error message.</param>
        /// <returns>True on success.</returns>
        public static bool GenerateOutputFile(Excision exc, byte[] data, out string failMsg) {
            if (exc.FullPath == null) {
                failMsg = "internal error";
                return false;
            }
            if (File.Exists(exc.FullPath)) {
                // Test the file length.  If it's different, don't overwrite the existing file.
                // Make an exception if it's zero bytes long?
                long fileLen = new FileInfo(exc.FullPath).Length;
                if (exc.Length != fileLen) {
                    failMsg = "output file \"" + exc.PathName + "\" exists and " +
                        "has a different length (" + fileLen + " vs. " + exc.Length + ")";
                    return false;
                }
            }
            try {
                // Create any directories in the path.
                string dirName = Path.GetDirectoryName(exc.FullPath);
                Directory.CreateDirectory(dirName);
                // Create the file and copy the data into it.
                Debug.Assert(exc.Offset < data.Length && exc.Offset + exc.Length <= data.Length);
                using (Stream stream = new FileStream(exc.FullPath, FileMode.OpenOrCreate,
                        FileAccess.ReadWrite, FileShare.None)) {
                    stream.SetLength(0);
                    stream.Write(data, exc.Offset, exc.Length);
                }
            } catch (Exception ex) {
                failMsg = "unable to create '" + exc.PathName + "': " + ex.Message;
                return false;
            }
            failMsg = string.Empty;
            return true;
        }

        /// <summary>
        /// Validates a binary-include filename.  We allow partial paths, but they're not allowed
        /// to ascend above the current directory.  Does not access the filesystem.
        /// </summary>
        /// <remarks>
        /// <para>The Path.GetFullPath() call hits the filesystem, which is undesirable for
        /// a check-as-you-type test.  We just want to avoid having a "rooted" path or something
        /// with a ".." directory reference.</para>
        /// <para>This is intended as a simple measure to avoid having important files
        /// overwritten by an asm generation command.  The file generator could employ other
        /// measures, e.g. checking to see if an existing output file has the same size.  (Note
        /// some malicious individual could hand-edit the filename in the project file.)</para>
        /// <para>We screen the filename for illegal characters, though what works on one
        /// platform might not on another.  We can't guarantee validity.</para>
        /// </remarks>
        /// <param name="pathName">Partial path to verify.</param>
        /// <returns>True if the path looks correct.</returns>
        public static bool ValidatePathName(string pathName) {
            if (string.IsNullOrEmpty(pathName)) {
                return false;
            }
            // In .NET Framework, IsPathRooted() will throw if invalid chars are found.  This is
            // not a full syntax check, just a char test.  The behavior changed in .NET Core 2.1.
            try {
                if (Path.IsPathRooted(pathName)) {
                    return false;
                }
            } catch (Exception ex) {
                Debug.WriteLine("GetFileName rejected pathname: " + ex.Message);
                return false;
            }

            // Try to screen out "../foo", "x/../y", "bar/..", without rejecting "..my..stuff..".
            // Normalize to forward-slash and split into components.
            string normal = pathName.Replace('\\', '/');
            string[] parts = normal.Split('/');
            foreach (string part in parts) {
                if ("..".Equals(part)) {
                    return false;
                }
            }

            // Reject names with a double quote, so we don't have to figure out the quote-quoting
            // mechanism for every assembler.
            if (normal.Contains("\"")) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a binary include pathname to a format suited for storage.
        /// </summary>
        /// <param name="pathName">Partial pathname.</param>
        /// <returns>String to store.</returns>
        public static string ConvertPathNameToStorage(string pathName) {
            return PATH_PREFIX_CHAR + pathName;
        }

        /// <summary>
        /// Converts the stored name back to a path prefix string.
        /// </summary>
        /// <param name="storageStr">Stored string.</param>
        /// <returns>Path prefix.</returns>
        public static string ConvertPathNameFromStorage(string storageStr) {
            if (string.IsNullOrEmpty(storageStr) || storageStr[0] != PATH_PREFIX_CHAR) {
                return "!BAD STORED NAME!";
            }
            return storageStr.Substring(1);
        }
    }
}
