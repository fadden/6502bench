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
using System.Text;

namespace CommonUtil {
    public static class FileUtil {
        /// <summary>
        /// Compares the contents of a file against a byte array.
        /// </summary>
        /// <param name="expected">Expected data values.</param>
        /// <param name="pathName">Pathname of file to inspect.</param>
        /// <param name="badOffset">Offset of first mismatch.  If this is equal to expected.Length
        ///   or File(pathName).Length, then the contents were equal but one source stopped
        ///   early.  The badFileVal parameter will be zero.</param>
        /// <param name="badFileVal">Mismatched value, from the file.</param>
        /// <returns>True if the contents match, false if not.</returns>
        public static bool CompareBinaryFile(byte[] expected, string pathName,
                out int badOffset, out byte badFileVal) {
            badOffset = -1;
            badFileVal = 0;

            int chunkOffset = 0;
            byte[] buffer = new byte[65536];
            using (FileStream fs = File.Open(pathName, FileMode.Open, FileAccess.Read)) {
                //if (fs.Length != expected.Length) {
                //    return false;
                //}
                int fileRemain = (int)fs.Length;

                while (fileRemain != 0) {
                    int toRead = Math.Min(buffer.Length, fileRemain);
                    int actual = fs.Read(buffer, 0, toRead);
                    if (actual != toRead) {
                        // File I/O problem; unexpected.
                        return false;
                    }

                    for (int i = 0; i < toRead; i++) {
                        if (chunkOffset + i >= expected.Length) {
                            // File on disk was too long.
                            Debug.Assert(fs.Length > expected.Length);
                            badOffset = chunkOffset + i;
                            return false;
                        }
                        if (expected[chunkOffset + i] != buffer[i]) {
                            badOffset = chunkOffset + i;
                            badFileVal = buffer[i];
                            return false;
                        }
                    }

                    fileRemain -= toRead;
                    chunkOffset += toRead;
                }

                if (fs.Length != expected.Length) {
                    Debug.Assert(fs.Length < expected.Length);
                    badOffset = (int) fs.Length;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Compares two text files line-by-line.  Ignores line termination characters.
        /// Assumes UTF-8 encoding.
        /// </summary>
        /// <param name="pathName1">Full path of first file.</param>
        /// <param name="pathName2">Full path of second file.</param>
        /// <param name="firstDiffLine">Line number of first differing line.</param>
        /// <param name="line1">Differing line from first file.</param>
        /// <param name="line2">Differing line from second file.</param>
        /// <returns>True if the files are equal.</returns>
        public static bool CompareTextFiles(string pathName1, string pathName2,
                out int firstDiffLine, out string line1, out string line2) {
            int line = 0;
            using (StreamReader sr1 = new StreamReader(pathName1, Encoding.UTF8)) {
                using (StreamReader sr2 = new StreamReader(pathName2, Encoding.UTF8)) {
                    while (true) {
                        // ReadLine strips the EOL char(s)
                        string str1 = sr1.ReadLine();
                        string str2 = sr2.ReadLine();
                        line++;
                        if (str1 != str2) {
                            firstDiffLine = line;
                            line1 = str1;
                            line2 = str2;
                            return false;
                        }
                        if (str1 == null) {
                            Debug.Assert(str2 == null);
                            break;
                        }
                    }
                }
            }

            firstDiffLine = -1;
            line1 = line2 = null;
            return true;
        }

        /// <summary>
        /// Determines whether the destination file is missing or older than the source file.
        /// This can be used do decide whether it's necessary to copy srcFile over.
        /// </summary>
        /// <param name="dstFile">File of interest.</param>
        /// <param name="srcFile">File to compare dates with.</param>
        /// <returns>True if dstFile is missing or older than srcFile.</returns>
        public static bool IsFileMissingOrOlder(string dstFile, string srcFile) {
            FileInfo fid = new FileInfo(dstFile);
            if (!fid.Exists) {
                return true;    // not there
            }
            FileInfo fis = new FileInfo(srcFile);
            if (!fis.Exists) {
                return false;   // nothing to compare against
            }

            return (fid.LastWriteTimeUtc < fis.LastWriteTimeUtc);
        }
    }
}
