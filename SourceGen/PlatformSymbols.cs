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
        private const string ERASE_VALUE_STR = "ERASE";
        public const int ERASE_VALUE = -1;
        public static readonly string FILENAME_FILTER = Res.Strings.FILE_FILTER_SYM65;

        /// <summary>
        /// Regex pattern for symbol definition in platform symbol file.
        ///
        /// Alphanumeric ASCII + underscore for label, which must start at beginning of line.
        /// Value is somewhat arbitrary, but ends if we see a comment delimiter (semicolon) or
        /// whitespace.  Spaces are allowed between tokens, but not required.  Value, width,
        /// and mask may be hex, decimal, or binary; these are simply tokenized by regex.
        ///
        /// Looks like:
        ///   NAME {@,=,<,>} VALUE [WIDTH] [;COMMENT]
        ///
        /// Regex output groups are:
        /// 1. NAME (2+ alphanumeric or underscore, cannot start with number)
        /// 2. type/direction char
        /// 3. VALUE (can be any non-whitespace)
        /// 4. optional: WIDTH (can be any non-whitespace)
        /// 5. optional: COMMENT with leading ';'
        /// </summary>
        /// <remarks>
        /// If you want to make sense of this, I highly recommend https://regex101.com/ .
        /// </remarks>
        private const string SYMBOL_PATTERN =
            @"^([A-Za-z_][A-Za-z0-9_]+)\s*([@=<>])\s*([^\s;]+)\s*([^\s;]+)?\s*(;.*)?$";
        private static Regex sNameValueRegex = new Regex(SYMBOL_PATTERN);
        private const int GROUP_NAME = 1;
        private const int GROUP_TYPE = 2;
        private const int GROUP_VALUE = 3;
        private const int GROUP_WIDTH = 4;
        private const int GROUP_COMMENT = 5;

        /// <summary>
        /// Regex pattern for mask definition in platform symbol file.  This mostly just
        /// performs tokenization.  Syntax and validity checking is done later.
        ///
        /// Looks like:
        ///   CMP_MASK CMP_VALUE ADDR_MASK [;COMMENT]
        /// </summary>
        private const string MULTI_MASK_PATTERN =
            @"^\s*([^\s]+)\s*([^\s]+)\s*([^\s;]+)\s*(;.*)?$";
        private static Regex sMaskRegex = new Regex(MULTI_MASK_PATTERN);

        private const string MULTI_MASK_CMD = "*MULTI_MASK";
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

        public bool ContainsKey(string label) {
            return mSymbols.ContainsKey(label);
        }

        /// <summary>
        /// Loads platform symbols.
        /// </summary>
        /// <param name="fileIdent">External file identifier of symbol file.</param>
        /// <param name="projectDir">Full path to project directory.</param>
        /// <param name="loadOrdinal">Platform file load order.</param>
        /// <param name="report">Report of warnings and errors.</param>
        /// <returns>True on success (no errors), false on failure.</returns>
        public bool LoadFromFile(string fileIdent, string projectDir, int loadOrdinal,
                out FileLoadReport report) {
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

            // These files shouldn't be enormous.  Just read the entire thing into a string array.
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
            DefSymbol.MultiAddressMask multiMask = null;

            int lineNum = 0;
            foreach (string line in lines) {
                lineNum++;      // first line is line 1, says Vim and VisualStudio
                string trimLine = line.Trim();
                if (string.IsNullOrEmpty(trimLine) || trimLine[0] == ';') {
                    // all whitespace, or just a comment; ignore
                } else if (line[0] == '*') {
                    if (line.StartsWith(TAG_CMD)) {
                        tag = ParseTag(line);
                    } else if (line.StartsWith(MULTI_MASK_CMD)) {
                        if (!ParseMask(line, out multiMask, out string badMaskMsg)) {
                            report.Add(lineNum, FileLoadItem.NO_COLUMN, FileLoadItem.Type.Warning,
                                badMaskMsg);
                        }
                        //Debug.WriteLine("Mask is now " + mask.ToString("x6"));
                    } else {
                        // Do something clever with *SYNOPSIS?
                        Debug.WriteLine("Ignoring CMD: " + line);
                    }
                } else {
                    MatchCollection matches = sNameValueRegex.Matches(line);
                    if (matches.Count == 1) {
                        // Our label regex is the same as Asm65.Label's definition; no need
                        // for further validation on the label.
                        string label = matches[0].Groups[GROUP_NAME].Value;
                        char typeAndDir = matches[0].Groups[GROUP_TYPE].Value[0];
                        bool isConst = (typeAndDir == '=');
                        DefSymbol.DirectionFlags direction = DefSymbol.DirectionFlags.ReadWrite;
                        if (typeAndDir == '<') {
                            direction = DefSymbol.DirectionFlags.Read;
                        } else if (typeAndDir == '>') {
                            direction = DefSymbol.DirectionFlags.Write;
                        }

                        string badParseMsg;
                        int value, numBase;
                        bool parseOk;
                        string valueStr = matches[0].Groups[GROUP_VALUE].Value;
                        if (isConst) {
                            // Allow various numeric options, and preserve the value.  We
                            // don't limit the value range.
                            parseOk = Asm65.Number.TryParseInt(valueStr, out value, out numBase);
                            badParseMsg =
                                CommonUtil.Properties.Resources.ERR_INVALID_NUMERIC_CONSTANT;
                        } else if (valueStr.ToUpperInvariant().Equals(ERASE_VALUE_STR)) {
                            parseOk = true;
                            value = ERASE_VALUE;
                            numBase = 10;
                            badParseMsg = CommonUtil.Properties.Resources.ERR_INVALID_ADDRESS;
                        } else {
                            // Allow things like "05/1000".  Always hex.
                            numBase = 16;
                            parseOk = Asm65.Address.ParseAddress(valueStr, (1 << 24) - 1,
                                out value);
                            // limit to positive 24-bit values
                            parseOk &= (value >= 0 && value < 0x01000000);
                            badParseMsg = CommonUtil.Properties.Resources.ERR_INVALID_ADDRESS;
                        }

                        int width = -1;
                        string widthStr = matches[0].Groups[GROUP_WIDTH].Value;
                        if (parseOk && !string.IsNullOrEmpty(widthStr)) {
                            parseOk = Asm65.Number.TryParseInt(widthStr, out width,
                                    out int ignoredBase);
                            if (parseOk) {
                                if (width < DefSymbol.MIN_WIDTH || width > DefSymbol.MAX_WIDTH) {
                                    parseOk = false;
                                    badParseMsg = Res.Strings.ERR_INVALID_WIDTH;
                                }
                            } else {
                                badParseMsg =
                                    CommonUtil.Properties.Resources.ERR_INVALID_NUMERIC_CONSTANT;
                            }
                        }

                        if (parseOk && multiMask != null && !isConst) {
                            // We need to ensure that all possible values fit within the mask.
                            // We don't test AddressValue here, because it's okay for the
                            // canonical value to be outside the masked range.
                            int testWidth = (width > 0) ? width : 1;
                            for (int testValue = value; testValue < value + testWidth; testValue++) {
                                if ((testValue & multiMask.CompareMask) != multiMask.CompareValue) {
                                    parseOk = false;
                                    badParseMsg = Res.Strings.ERR_VALUE_INCOMPATIBLE_WITH_MASK;
                                    Debug.WriteLine("Mask FAIL: value=" + value.ToString("x6") +
                                        " width=" + width +
                                        " testValue=" + testValue.ToString("x6") +
                                        " mask=" + multiMask);
                                    break;
                                }
                            }
                        }

                        if (!parseOk) {
                            report.Add(lineNum, FileLoadItem.NO_COLUMN, FileLoadItem.Type.Warning,
                                badParseMsg);
                        } else {
                            string comment = matches[0].Groups[GROUP_COMMENT].Value;
                            if (comment.Length > 0) {
                                // remove ';'
                                comment = comment.Substring(1);
                            }
                            FormatDescriptor.SubType subType =
                                FormatDescriptor.GetSubTypeForBase(numBase);
                            DefSymbol symDef = new DefSymbol(label, value, Symbol.Source.Platform,
                                isConst ? Symbol.Type.Constant : Symbol.Type.ExternalAddr,
                                subType, width, width > 0, comment, direction, multiMask,
                                tag, loadOrdinal, fileIdent);
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
        /// Parses the mask value out of a mask command line.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <param name="multiMask">Parsed mask value, or null if the line was empty.</param>
        /// <returns>True if the mask was parsed successfully.</returns>
        private bool ParseMask(string line, out DefSymbol.MultiAddressMask multiMask,
                out string badMaskMsg) {
            Debug.Assert(line.StartsWith(MULTI_MASK_CMD));
            const int MIN = 0;
            const int MAX = 0x00ffffff;

            badMaskMsg = Res.Strings.ERR_INVALID_MULTI_MASK;
            multiMask = null;

            string maskStr = line.Substring(MULTI_MASK_CMD.Length).Trim();
            if (string.IsNullOrEmpty(maskStr)) {
                // empty line, disable mask
                return true;
            }

            MatchCollection matches = sMaskRegex.Matches(maskStr);
            if (matches.Count != 1) {
                return false;
            }

            string cmpMaskStr = matches[0].Groups[1].Value;
            string cmpValueStr = matches[0].Groups[2].Value;
            string addrMaskStr = matches[0].Groups[3].Value;
            int cmpMask, cmpValue, addrMask, ignoredBase;

            if (!Asm65.Number.TryParseInt(cmpMaskStr, out cmpMask, out ignoredBase) ||
                    cmpMask < MIN || cmpMask > MAX) {
                Debug.WriteLine("Bad cmpMask: " + cmpMaskStr);
                badMaskMsg = Res.Strings.ERR_INVALID_COMPARE_MASK;
                return false;
            }
            if (!Asm65.Number.TryParseInt(cmpValueStr, out cmpValue, out ignoredBase) ||
                    cmpValue < MIN || cmpValue > MAX) {
                Debug.WriteLine("Bad cmpValue: " + cmpValueStr);
                badMaskMsg = Res.Strings.ERR_INVALID_COMPARE_VALUE;
                return false;
            }
            if (!Asm65.Number.TryParseInt(addrMaskStr, out addrMask, out ignoredBase) ||
                    addrMask < MIN || addrMask > MAX) {
                Debug.WriteLine("Bad addrMask: " + addrMaskStr);
                badMaskMsg = Res.Strings.ERR_INVALID_ADDRESS_MASK;
                return false;
            }

            // The two masks should not overlap: one represents bits that must be in a
            // specific state for a match to exist, the other indicates which bits are used
            // to select a specific register.  This should be a warning.
            if ((cmpMask & ~addrMask) != cmpMask) {
                Debug.WriteLine("Warning: cmpMask/addrMask overlap");
                badMaskMsg = Res.Strings.ERR_INVALID_CMP_ADDR_OVERLAP;
                return false;
            }
            // If cmpValue has bits set that aren't in cmpMask, we will never find a match.
            if ((cmpValue & ~cmpMask) != 0) {
                Debug.WriteLine("cmpValue has unexpected bits set");
                badMaskMsg = Res.Strings.ERR_INVALID_CMP_EXTRA_BITS;
                return false;
            }

            multiMask = new DefSymbol.MultiAddressMask(cmpMask, cmpValue, addrMask);
            return true;
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
