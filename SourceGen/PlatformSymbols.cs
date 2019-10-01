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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

using CommonUtil;

namespace SourceGen {
    /// <summary>
    /// Loads and maintains a collection of platform-specific symbols from a ".sym65" file.
    /// </summary>
    public class PlatformSymbols : IEnumerable<Symbol> {
        public const string FILENAME_EXT = ".sym65";
        public static readonly string FILENAME_FILTER = Res.Strings.FILE_FILTER_SYM65;

        /// <summary>
        /// Regex pattern for name/value pairs in symbol file.
        ///
        /// Alphanumeric ASCII + underscore for label, which must start at beginning of line.
        /// Value is somewhat arbitrary, but ends if we see a comment delimiter (semicolon) or
        /// whitespace.  Width is decimal or hex.  Spaces are allowed between tokens.
        ///
        /// Group 1 is the name, group 2 is '=' or '@', group 3 is the value, group 4 is
        /// the symbol width (optional), group 5 is the comment (optional).
        /// </summary>
        /// <remarks>
        /// If you want to make sense of this, I highly recommend https://regex101.com/ .
        /// </remarks>
        private const string NAME_VALUE_PATTERN =
            @"^([A-Za-z0-9_]+)\s*([@=])\s*([^\ ;]+)\s*([0-9\$]+)?\s*(;.*)?$";
        private static Regex sNameValueRegex = new Regex(NAME_VALUE_PATTERN);

        private const string TAG_CMD = "*TAG";

        /// <summary>
        /// List of symbols.  We keep them sorted by label because labels must be unique.
        /// </summary>
        private SortedList<string, Symbol> mSymbols =
            new SortedList<string, Symbol>(Asm65.Label.LABEL_COMPARER);


        public PlatformSymbols() { }

        // IEnumerable
        public IEnumerator<Symbol> GetEnumerator() {
            return mSymbols.Values.GetEnumerator();
        }

        // IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return mSymbols.Values.GetEnumerator();
        }

        /// <summary>
        /// Loads platform symbols.
        /// </summary>
        /// <param name="fileIdent">Relative pathname of file to open.</param>
        /// <param name="projectDir">Full path to project directory.</param>
        /// <param name="report">Report of warnings and errors.</param>
        /// <returns>True on success (no errors), false on failure.</returns>
        public bool LoadFromFile(string fileIdent, string projectDir, out FileLoadReport report) {
            // These files shouldn't be enormous.  Do it the easy way.
            report = new FileLoadReport(fileIdent);

            ExternalFile ef = ExternalFile.CreateFromIdent(fileIdent);
            if (ef == null) {
                report.Add(FileLoadItem.Type.Error,
                    CommonUtil.Properties.Resources.ERR_FILE_NOT_FOUND + ": " + fileIdent);
                return false;
            }

            string pathName = ef.GetPathName(projectDir);
            if (pathName == null) {
                report.Add(FileLoadItem.Type.Error,
                    Res.Strings.ERR_BAD_IDENT + ": " + fileIdent);
                return false;
            }
            string[] lines;
            try {
                lines = File.ReadAllLines(pathName);
            } catch (IOException ioe) {
                Debug.WriteLine("Platform symbol load failed: " + ioe);
                report.Add(FileLoadItem.Type.Error,
                    CommonUtil.Properties.Resources.ERR_FILE_NOT_FOUND + ": " + pathName);
                return false;
            }

            string tag = string.Empty;

            int lineNum = 0;
            foreach (string line in lines) {
                lineNum++;      // first line is line 1, says Vim and VisualStudio
                if (string.IsNullOrEmpty(line) || line[0] == ';') {
                    // ignore
                } else if (line[0] == '*') {
                    if (line.StartsWith(TAG_CMD)) {
                        tag = ParseTag(line);
                    } else {
                        // Do something clever with *SYNOPSIS?
                        Debug.WriteLine("CMD: " + line);
                    }
                } else {
                    MatchCollection matches = sNameValueRegex.Matches(line);
                    if (matches.Count == 1) {
                        //Debug.WriteLine("GOT '" + matches[0].Groups[1] + "' " +
                        //    matches[0].Groups[2] + " '" + matches[0].Groups[3] + "'");
                        string label = matches[0].Groups[1].Value;
                        bool isConst = (matches[0].Groups[2].Value[0] == '=');
                        string badParseMsg;
                        int value, numBase;
                        bool parseOk;
                        if (isConst) {
                            // Allow various numeric options, and preserve the value.
                            parseOk = Asm65.Number.TryParseInt(matches[0].Groups[3].Value,
                                out value, out numBase);
                            badParseMsg =
                                CommonUtil.Properties.Resources.ERR_INVALID_NUMERIC_CONSTANT;
                        } else {
                            // Allow things like "05/1000".  Always hex.
                            numBase = 16;
                            parseOk = Asm65.Address.ParseAddress(matches[0].Groups[3].Value,
                                (1 << 24) - 1, out value);
                            badParseMsg = CommonUtil.Properties.Resources.ERR_INVALID_ADDRESS;
                        }

                        int width = -1;
                        string widthStr = matches[0].Groups[4].Value;
                        if (parseOk && !string.IsNullOrEmpty(widthStr)) {
                            parseOk = Asm65.Number.TryParseInt(widthStr, out width,
                                    out int ignoredBase);
                            if (parseOk) {
                                if (width < DefSymbol.MIN_WIDTH || width > DefSymbol.MAX_WIDTH) {
                                    parseOk = false;
                                }
                            } else {
                                badParseMsg =
                                    CommonUtil.Properties.Resources.ERR_INVALID_NUMERIC_CONSTANT;
                            }
                        }

                        if (!parseOk) {
                            report.Add(lineNum, FileLoadItem.NO_COLUMN, FileLoadItem.Type.Warning,
                                badParseMsg);
                        } else {
                            string comment = matches[0].Groups[5].Value;
                            if (comment.Length > 0) {
                                // remove ';'
                                comment = comment.Substring(1);
                            }
                            FormatDescriptor.SubType subType =
                                FormatDescriptor.GetSubTypeForBase(numBase);
                            DefSymbol symDef = new DefSymbol(label, value, Symbol.Source.Platform,
                                isConst ? Symbol.Type.Constant : Symbol.Type.ExternalAddr,
                                subType, comment, tag, width, width > 0);
                            if (mSymbols.ContainsKey(label)) {
                                // This is very easy to do -- just define the same symbol twice
                                // in the same file.  We don't really need to do anything about
                                // it though.
                                Debug.WriteLine("NOTE: stomping previous definition of " + label);
                            }
                            mSymbols[label] = symDef;
                        }
                    } else {
                        report.Add(lineNum, FileLoadItem.NO_COLUMN, FileLoadItem.Type.Warning,
                            CommonUtil.Properties.Resources.ERR_SYNTAX);
                    }
                }
            }

            return !report.HasErrors;
        }

        /// <summary>
        /// Parses the tag out of a tag command line.  The tag is pretty much everything after
        /// the "*TAG", with whitespace stripped off the start and end.  The empty string
        /// is valid.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <returns>Tag string.</returns>
        private string ParseTag(string line) {
            Debug.Assert(line.StartsWith(TAG_CMD));
            string tag = line.Substring(TAG_CMD.Length).Trim();
            return tag;
        }

        /// <summary>
        /// One-off function to convert the IIgs toolbox function info from NList.Data.TXT
        /// to .sym65 format.  Doesn't really belong in here, but I'm too lazy to put it
        /// anywhere else.
        /// </summary>
        public static void ConvertNiftyListToolboxFuncs(string inPath, string outPath) {
            const string TOOL_START = "* System tools";
            const string TOOL_END = "* User tools";
            const string PATTERN = @"^([0-9a-fA-F]{4}) (\w+)(.*)";
            Regex parseRegex = new Regex(PATTERN);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            string[] lines = File.ReadAllLines(inPath);
            List<String> outs = new List<string>();

            bool inTools = false;
            foreach (string line in lines) {
                if (line == TOOL_START) {
                    inTools = true;
                    continue;
                } else if (line == TOOL_END) {
                    break;
                }
                if (!inTools) {
                    continue;
                }
                if (line.Substring(5, 4) == "=== ") {
                    // make this a comment
                    outs.Add("; " + line.Substring(5));
                    continue;
                }
                MatchCollection matches = parseRegex.Matches(line);
                if (matches.Count != 1) {
                    Debug.WriteLine("NConv: bad match on '" + line + "'");
                    outs.Add("; " + line);
                    continue;
                }

                GroupCollection group = matches[0].Groups;
                string outStr;
                if (matches[0].Groups.Count != 4) {
                    Debug.WriteLine("NConv: partial match (" + group.Count + ") on '" +
                        line + "'");
                    outStr = ";" + group[0];
                } else {
                    sb.Clear();
                    sb.Append(group[2]);
                    while (sb.Length < 19) {        // not really worried about speed
                        sb.Append(' ');
                    }
                    sb.Append(" = $");
                    sb.Append(group[1]);
                    while (sb.Length < 32) {
                        sb.Append(' ');
                    }
                    sb.Append(';');
                    sb.Append(group[3]);
                    outs.Add(sb.ToString());
                }
            }

            File.WriteAllLines(outPath, outs);
            Debug.WriteLine("NConv complete (" + outs.Count + " lines)");
        }
    }
}
