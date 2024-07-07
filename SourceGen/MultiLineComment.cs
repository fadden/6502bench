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
using System.Diagnostics;
using System.Windows.Media;
using System.Text;

// Spaces and hyphens are different.  For example, if width is 10,
// "long words<space>more words" becomes:
//   0123456789
//   long words
//   more words
// However, "long words-more words" becomes:
//   long
//   words-more
//   words
// because the hyphen is retained but the space is discarded.

namespace SourceGen {
    /// <summary>
    /// <para>Representation of a multi-line comment, which is a string plus some format options.
    /// Used for long comments and notes.</para>
    /// 
    /// <para>Instances are effectively immutable, as the text and options can't be modified
    /// after the object is created.  The object does cache the result of the last FormatText()
    /// call, which is determined in part by the Formatter argument, which can change between
    /// calls.</para>
    /// </summary>
    public class MultiLineComment {
        /// <summary>
        /// Unformatted text.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// True if this uses "fancy" formatting.  If set, the BoxMode and MaxWidth properties
        /// are ignored.
        /// </summary>
        public bool IsFancy { get; private set; }

        /// <summary>
        /// Set to true to render text surrounded by a box of ASCII characters.
        /// </summary>
        public bool BoxMode { get; private set; }

        /// <summary>
        /// Maximum line width.  Box mode effectively reduces this by four.
        /// </summary>
        public int MaxWidth { get; private set; }

        /// <summary>
        /// Background color for notes.
        /// </summary>
        public Color BackgroundColor { get; private set; }

        /// <summary>
        /// Box character to use for "basic" formatting.
        /// </summary>
        private const char BASIC_BOX_CHAR = '*';

        private const int DEFAULT_WIDTH = 80;
        private const int MIN_WIDTH = 8;
        private const int MAX_WIDTH = 128;
        private const string SPACES =       // MAX_WIDTH spaces
            "                                                                " +
            "                                                                ";


        /// <summary>
        /// Constructor.  By default, comments use basic formatting, have a basic-mode max
        /// width of 80, and aren't boxed.
        /// </summary>
        /// <remarks>
        /// We'd actually prefer to have fancy formatting be the default, but that does the
        /// wrong thing when deserializing older projects.
        /// </remarks>
        /// <param name="text">Unformatted comment text.</param>
        public MultiLineComment(string text) {
            Debug.Assert(text != null);     // empty string is okay
            Text = text;
            IsFancy = false;
            BoxMode = false;
            MaxWidth = DEFAULT_WIDTH;
            BackgroundColor = CommonWPF.Helper.ZeroColor;
        }

        /// <summary>
        /// Constructor.  Used when creating an empty MLC for editing.
        /// </summary>
        /// <param name="isFancy">True if we want to be in "fancy" mode initially.</param>
        public MultiLineComment(bool isFancy) : this(string.Empty) {
            IsFancy = isFancy;
        }

        /// <summary>
        /// Constructor.  Used for long comments.
        /// </summary>
        /// <param name="text">Unformatted text.</param>
        /// <param name="isFancy">True if we're using fancy format mode.</param>
        /// <param name="boxMode">For basic mode, set to true to enable box mode.</param>
        /// <param name="maxWidth">For basic mode, maximum line width.</param>
        public MultiLineComment(string text, bool isFancy, bool boxMode, int maxWidth)
                : this(text) {
            if (maxWidth < MIN_WIDTH) {
                Debug.Assert(false, "unexpectedly small max width");
                maxWidth = MIN_WIDTH;
            }
            IsFancy = isFancy;
            BoxMode = boxMode;
            MaxWidth = maxWidth;
        }

        /// <summary>
        /// Constructor.  Used for notes.
        /// </summary>
        /// <param name="text">Unformatted text.</param>
        /// <param name="bkgndColor">Background color.</param>
        public MultiLineComment(string text, Color bkgndColor) : this(text) {
            BackgroundColor = bkgndColor;
        }

        private List<string> mPreviousRender = null;
        private Asm65.Formatter mPreviousFormatter = null;
        private string mPreviousPrefix = null;

        /// <summary>
        /// Generates one or more lines of formatted text.
        /// </summary>
        /// <param name="formatter">Formatter, with comment delimiters.</param>
        /// <param name="textPrefix">String to prepend to text before formatting.  If this
        ///   is non-empty, comment delimiters aren't emitted.  (Used for notes.)</param>
        /// <returns>List of formatted strings.  Do not modify the list.</returns>
        public List<string> FormatText(Asm65.Formatter formatter, string textPrefix) {
            if (mPreviousRender != null && formatter == mPreviousFormatter &&
                    textPrefix == mPreviousPrefix) {
                // We rendered this with the same formatter before.  Return the list.  It would
                // be safer to clone the list, but I'm not expecting the caller to edit it.
                return mPreviousRender;
            }
            List<string> lines;
            try {
                if (IsFancy) {
                    Debug.Assert(string.IsNullOrEmpty(textPrefix));
                    lines = FormatFancyText(formatter);
                } else {
                    lines = FormatSimpleText(formatter, textPrefix);
                }
            } catch (Exception ex) {
                Debug.WriteLine("FormatText failed: " + ex);
                lines = new List<string>();
                lines.Add("Internal error: " + ex.Message);
            }
            // Cache result.
            mPreviousRender = lines;
            mPreviousFormatter = formatter;
            mPreviousPrefix = textPrefix;
            return lines;
        }

        /// <summary>
        /// Generates one or more lines of formatted text, using the basic formatter.
        /// </summary>
        /// <param name="formatter">Formatter, with comment delimiters.</param>
        /// <param name="textPrefix">String to prepend to text before formatting.  If this
        ///   is non-empty, comment delimiters aren't emitted.  (Used for notes.)</param>
        /// <returns>List of formatted strings.</returns>
        private List<string> FormatSimpleText(Asm65.Formatter formatter, string textPrefix) {
            const char spcRep = '\u2219';   // BULLET OPERATOR
            string workString = string.IsNullOrEmpty(textPrefix) ? Text : textPrefix + Text;
            List<string> lines = new List<string>();
            bool debugMode = formatter.DebugLongComments;

            if (MaxWidth > MAX_WIDTH) {
                lines.Add("!Bad MaxWidth!");
                return lines;
            }

            string linePrefix;
            if (!string.IsNullOrEmpty(textPrefix)) {
                // This is a Note, no comment delimiter needed.
                linePrefix = string.Empty;
            } else if (BoxMode) {
                if (formatter.FullLineCommentDelimiterBase.Length == 1 &&
                        formatter.FullLineCommentDelimiterBase[0] == BASIC_BOX_CHAR) {
                    // Box char is same as comment delimiter, don't double-up.
                    linePrefix = string.Empty;
                } else {
                    // Prefix with comment delimiter, but don't include optional space.
                    linePrefix = formatter.FullLineCommentDelimiterBase;
                }
            } else {
                // No box, prefix every line with comment delimiter and optional space.
                linePrefix = formatter.FullLineCommentDelimiterPlus;
            }

            StringBuilder sb = new StringBuilder(MaxWidth);
            if (debugMode) {
                for (int i = 0; i < MaxWidth; i++) {
                    sb.Append((i % 10).ToString());
                }
                lines.Add(sb.ToString());
                sb.Clear();
            }
            string boxLine;
            if (BoxMode) {
                for (int i = 0; i < MaxWidth - linePrefix.Length; i++) {
                    sb.Append(BASIC_BOX_CHAR);
                }
                boxLine = sb.ToString();
                sb.Clear();
            } else {
                boxLine = null;
            }

            if (BoxMode && workString.Length > 0) {
                lines.Add(linePrefix + boxLine);
            }

            int lineWidth = BoxMode ?
                    MaxWidth - linePrefix.Length - 4 :
                    MaxWidth - linePrefix.Length;
            Debug.Assert(lineWidth > 0);
            int startIndex = 0;
            int breakIndex = -1;
            for (int i = 0; i < workString.Length; i++) {
                if (workString[i] == '\r' || workString[i] == '\n') {
                    // explicit line break, emit line
                    string str = workString.Substring(startIndex, i - startIndex);
                    if (debugMode) { str = str.Replace(' ', spcRep); }
                    if (BoxMode) {
                        if (str == "" + BASIC_BOX_CHAR) {
                            // asterisk on a line by itself means "output row of asterisks"
                            str = linePrefix + boxLine;
                        } else {
                            int padLen = lineWidth - str.Length;
                            str = linePrefix + BASIC_BOX_CHAR + " " + str +
                                SPACES.Substring(0, padLen + 1) + BASIC_BOX_CHAR;
                        }
                    } else {
                        str = linePrefix + str;
                    }
                    lines.Add(str);
                    // Eat the LF in CRLF.
                    if (workString[i] == '\r' && i < workString.Length - 1 &&
                            workString[i + 1] == '\n') {
                        i++;
                    }
                    startIndex = i + 1;
                    breakIndex = -1;
                } else if (workString[i] == ' ') {
                    // can break on a space even if it's one char too far
                    breakIndex = i;
                }

                if (i - startIndex >= lineWidth) {
                    // this character was one too many, break line at last break point
                    if (breakIndex <= 0) {
                        // no break found, just chop it
                        string str = workString.Substring(startIndex, i - startIndex);
                        if (debugMode) { str = str.Replace(' ', spcRep); }
                        if (BoxMode) {
                            str = linePrefix + BASIC_BOX_CHAR + " " + str + " " + BASIC_BOX_CHAR;
                        } else {
                            str = linePrefix + str;
                        }
                        lines.Add(str);
                        startIndex = i;
                    } else {
                        // Copy everything from start to break.  If the break was a hyphen,
                        // we want to keep it.
                        int adj = 0;
                        if (workString[breakIndex] == '-') {
                            adj = 1;
                        }
                        string str = workString.Substring(startIndex,
                            breakIndex + adj - startIndex);
                        if (debugMode) { str = str.Replace(' ', spcRep); }
                        if (BoxMode) {
                            int padLen = lineWidth - str.Length;
                            str = linePrefix + BASIC_BOX_CHAR + " " + str +
                                SPACES.Substring(0, padLen + 1) + BASIC_BOX_CHAR;
                        } else {
                            str = linePrefix + str;
                        }
                        lines.Add(str);
                        startIndex = breakIndex + 1;
                        if (adj == 0 && startIndex < workString.Length &&
                                workString[startIndex] == ' ') {
                            // We broke on a space, and are now starting a line on a space,
                            // which looks weird (and happens commonly at the end of a
                            // sentence).  Eat one more space.
                            startIndex++;
                        }
                        breakIndex = -1;
                    }
                }

                if (workString[i] == '-') {
                    // can break on hyphen if it fits in line
                    breakIndex = i;
                }
            }

            if (startIndex < workString.Length) {
                // Output remainder.
                string str = workString.Substring(startIndex, workString.Length - startIndex);
                if (debugMode) { str = str.Replace(' ', spcRep); }
                if (BoxMode) {
                    int padLen = lineWidth - str.Length;
                    str = linePrefix + BASIC_BOX_CHAR + " " + str +
                        SPACES.Substring(0, padLen + 1) + BASIC_BOX_CHAR;
                } else {
                    str = linePrefix + str;
                }
                lines.Add(str);
            }

            if (BoxMode && workString.Length > 0) {
                lines.Add(linePrefix + boxLine);
            }

            return lines;
        }

        #region Fancy

        /// <summary>
        /// Input data source.
        /// </summary>
        /// <remarks>
        /// <para>When we encounter a tag, we create a new DataSource that has the contents of
        /// the tag in it, and skip over the full original extent.  This is especially handy for
        /// generated text, e.g. [url=x] link text, where the source isn't simply a subset of
        /// the original.</para>
        /// <para>Various bits of state are also stored here, so that we can prevent
        /// inappropriate nesting and track options set by the format tags.</para>
        /// </remarks>
        private class DataSource {
            private string mString;
            private int mPosn;

            public string Text => mString;
            public char this[int i] {
                get {
                    Debug.Assert(i >= 0 && i < mString.Length);
                    return mString[i];
                }
            }
            public int Posn { get { return mPosn; } set { mPosn = value; } }
            public int Length => mString.Length;
            public char CurChar => mString[mPosn];

            // These are true if the text is appearing inside start/end tags.
            public bool InBox { get; set; }
            public bool InUrl { get; set; }

            public bool InsideElement { get { return InBox || InsideNonBoxElement; } }
            public bool InsideNonBoxElement { get { return InUrl; } }

            // If true, don't prefix lines with the comment delimiter (used for [br]).
            public bool SuppressPrefix { get; set; }

            // True if using default char (comment delimiter) for boxes.
            public bool BoxCharIsDefault { get; set; } = true;
            public char BoxChar { get; set; } = '?';

            // If true, don't inset text inside a box (used for [hr]).
            public bool FullWidth { get; set; } = false;

            public DataSource(string str, int posn, DataSource outer) {
                mString = str;
                mPosn = posn;

                if (outer != null) {
                    // Inherit the values from the "outer" source.
                    InBox = outer.InBox;
                    InUrl = outer.InUrl;
                    SuppressPrefix = outer.SuppressPrefix;
                    BoxCharIsDefault = outer.BoxCharIsDefault;
                    BoxChar = outer.BoxChar;
                    FullWidth = outer.FullWidth;
                }
            }

            /// <summary>
            /// Returns true if the string at the current position matches the argument.  The
            /// comparison is case-insensitive.
            /// </summary>
            public bool Match(string str, int offset) {
                if (mPosn + offset + str.Length > mString.Length) {
                    return false;
                }
                for (int i = 0; i < str.Length; i++) {
                    // Shouldn't need to worry about InvariantCultureIgnoreCase since this is
                    // only used for tags.
                    if (char.ToUpper(str[i]) != char.ToUpper(mString[mPosn + offset + i])) {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Returns the position of the matching string.  The search starts at the current
            /// position, and is case-insensitive.
            /// </summary>
            public int FindNext(string str) {
                return Text.IndexOf(str, mPosn, StringComparison.InvariantCultureIgnoreCase);
            }
        }
        private Stack<DataSource> mSourceStack = new Stack<DataSource>();
        private StringBuilder mLineBuilder = new StringBuilder(MAX_WIDTH);

        private const char DEFAULT_RULE_CHAR = '-';

        private int mLineWidth;
        private string mLinePrefix;
        private bool mDebugMode;


        /// <summary>
        /// Calculates the width of the usable text area, given the current attributes.
        /// </summary>
        private int CalcTextWidth(DataSource source, bool forceFullWidth = false) {
            bool fullWidth = source.FullWidth | forceFullWidth;
            if (source.InBox) {
                if (source.BoxCharIsDefault) {
                    // Leave space for left/right box edges, no comment delimiter.
                    return mLineWidth - (fullWidth ? 2 : 4);
                } else {
                    // Also leave space for a leading comment delimiter, even if the chosen
                    // box char happens to match the current delimiter.  It might not match when
                    // it's rendered for asm gen, and we don't want the output to change.
                    return mLineWidth - (fullWidth ? 3 : 5);
                }
            } else {
                return mLineWidth - mLinePrefix.Length;
            }
        }

        /// <summary>
        /// Generates one or more lines of formatted text, using the fancy formatter.
        /// </summary>
        /// <param name="formatter">Formatter, which specifies comment delimiters.</param>
        /// <returns>List of formatted strings.</returns>
        private List<string> FormatFancyText(Asm65.Formatter formatter) {
            Debug.Assert(SPACES.Length == MAX_WIDTH);

            mLineWidth = DEFAULT_WIDTH;     // could make this a setting
            mDebugMode = formatter.DebugLongComments;
            mSourceStack.Clear();

            mLinePrefix = formatter.FullLineCommentDelimiterPlus;   // does not change
            //mBoxPrefix = "! ";                                      // changes with [box]

            DataSource source = new DataSource(Text, 0, null);
            int textWidth = CalcTextWidth(source);
            bool escapeNext = false;
            //bool eatNextIfNewline = false;

            char[] outBuf = new char[MAX_WIDTH];
            int outIndex = 0;
            int outBreakIndex = -1;

            List<string> lines = new List<string>();
            if (mDebugMode) {
                for (int i = 0; i < mLineWidth; i++) {
                    outBuf[i] = (char)('0' + i % 10);
                }
                lines.Add(new string(outBuf, 0, mLineWidth));
            }

            // Walk through the input source.
            while (true) {
                if (source.Posn == source.Length) {
                    if (mSourceStack.Count == 0) {
                        break;      // all done
                    }

                    source = mSourceStack.Pop();
                    textWidth = CalcTextWidth(source);
                    continue;   // resume earlier string
                }

                if (source.CurChar == '\r' || source.CurChar == '\n') {
                    // Explicit line break.  If it's a CRLF, eat both.
                    if (source.CurChar == '\r' && source.Posn + 1 < source.Length &&
                            source[source.Posn + 1] == '\n') {
                        source.Posn++;
                    }

                    escapeNext = false;        // can't escape newlines

                    // Output what we have.
                    OutputLine(outBuf, outIndex, source, lines);
                    outIndex = 0;
                    outBreakIndex = -1;
                    source.Posn++;
                    continue;
                }

                char thisCh = source.CurChar;
                if (thisCh == '\\') {
                    if (!escapeNext) {
                        escapeNext = true;
                        source.Posn++;      // eat the backslash
                        continue;           // restart loop to get next char (if any)
                    }
                } else if (thisCh == '[' && !escapeNext) {
                    // Start of format tag?
                    if (TryParseTag(source, formatter, out int skipLen, out DataSource subSource,
                            out bool requireLineStart)) {
                        if (requireLineStart && outIndex != 0) {
                            OutputLine(outBuf, outIndex, source, lines);
                            outIndex = 0;
                            outBreakIndex = -1;
                        }
                        source.Posn += skipLen;
                        if (subSource != null) {
                            mSourceStack.Push(source);
                            source = subSource;
                        }
                        textWidth = CalcTextWidth(source);
                        continue;
                    }
                } else if (thisCh == ' ') {
                    // Remember position of space for line break.  If there are multiple
                    // consecutive spaces, remember the position of the first one.
                    if (outBreakIndex < 0 || outBuf[outBreakIndex] != ' ' ||
                            (outIndex > 0 && outBuf[outIndex - 1] != ' ')) {
                        outBreakIndex = outIndex;
                    }
                }
                escapeNext = false;

                // We need to add a character to the out buffer.  Will this put us over the limit?
                if (outIndex == textWidth) {
                    int outputCount;
                    int adj = 0;
                    if (outBreakIndex <= 0) {
                        // No break found, or break char was at start of line.  Just chop what
                        // we have.
                        outputCount = outIndex;
                        if (outputCount > 0 && char.IsSurrogate(outBuf[outIndex - 1])) {
                            outputCount--;      // don't split surrogate pairs
                        }
                    } else {
                        // Break was a hyphen or space.
                        outputCount = outBreakIndex;

                        if (outBuf[outputCount] == '-') {
                            // Break was a hyphen, include it.
                            adj = 1;
                        }
                    }

                    // Output everything up to the break point, but not the break char itself
                    // unless it's a hyphen.
                    OutputLine(outBuf, outputCount + adj, source, lines);

                    // Consume any trailing spaces (which are about to become leading spaces).
                    while (outputCount < outIndex && outBuf[outputCount] == ' ') {
                        outputCount++;
                    }
                    // Copy any remaining chars to start of buffer.
                    outputCount += adj;
                    if (outputCount < outIndex) {
                        for (int i = 0; i < outIndex - outputCount; i++) {
                            outBuf[i] = outBuf[outputCount + i];
                        }
                    }
                    outIndex -= outputCount;
                    outBreakIndex = -1;
                    Debug.Assert(outIndex >= 0);

                    // If we're at the start of a line, eat all leading spaces.  (This is what
                    // the WPF TextEdit dialog does when word-wrapping.)
                    if (outIndex == 0) {
                        while (source.Posn < source.Length && source.CurChar == ' ') {
                            source.Posn++;
                        }
                        if (source.Posn == source.Length) {
                            // Whoops, ran out of input.
                            continue;
                        }
                    }
                }

                // Fold lines at hyphens.  We need to check for it after the "line full" test
                // because we want to retain it at the end of the line.
                if (source.CurChar == '-') {
                    outBreakIndex = outIndex;
                }

                Debug.Assert(outIndex >= 0 && outIndex < outBuf.Length);
                outBuf[outIndex++] = source[source.Posn++];
            }

            // If we didn't end with a CRLF, output the last bits.
            if (outIndex > 0) {
                OutputLine(outBuf, outIndex, source, lines);
            }

            return lines;
        }

        /// <summary>
        /// Adds the contents of the output buffer to the line list, prefixing it with comment
        /// delimiters and/or wrapping it in a box.
        /// </summary>
        /// <param name="outBuf">Output buffer.</param>
        /// <param name="length">Length of data in output buffer.</param>
        /// <param name="inBox">True if we're inside a box.</param>
        /// <param name="lines">Line list to add the line to.</param>
        private void OutputLine(char[] outBuf, int length, DataSource source, List<string> lines) {
            Debug.Assert(length >= 0);
            mLineBuilder.Clear();
            if (source.InBox) {
                // If the box character doesn't match the comment delimiter, output the
                // comment delimiter.
                bool boxMatchesCmt = (mLinePrefix[0] == source.BoxChar);
                if (!boxMatchesCmt) {
                    mLineBuilder.Append(mLinePrefix[0]);
                }
                mLineBuilder.Append(source.BoxChar);
                if (!source.FullWidth) {
                    mLineBuilder.Append(' ');       // inset text, unless we're doing an [hr]
                }
                mLineBuilder.Append(outBuf, 0, length);
                // Fill out the rest of the line with spaces, then add the final char.
                int trailingCount = mLineWidth - mLineBuilder.Length;
                // Line is one char shorter when the box character is specified and it matches
                // the comment.  (If the box character isn't specified then it always matches
                // the comment; if the box doesn't match the comment then we're shoved over one
                // char because the comment delimiter is present.)
                if (!source.BoxCharIsDefault && boxMatchesCmt) {
                    trailingCount--;
                }
                if (trailingCount > 1) {
                    mLineBuilder.Append(SPACES, 0, trailingCount - 1);
                }
                mLineBuilder.Append(source.BoxChar);
            } else {
                if (!source.SuppressPrefix) {
                    mLineBuilder.Append(mLinePrefix);
                }
                mLineBuilder.Append(outBuf, 0, length);
            }
            string str = mLineBuilder.ToString();
            if (mDebugMode) {
                str = str.Replace(' ', '\u2219');   // replace spaces with BULLET OPERATOR
            }
            lines.Add(str);
        }

        private enum Tag {
            Unknown = 0, Break, HorizRule, Width, BoxStart, UrlStart
        }
        private class TagMatch {
            public string mPatStr;
            public Tag mTag;

            public TagMatch(string pat, Tag tag) {
                mPatStr = pat;
                mTag = tag;
            }
        }
        private static readonly TagMatch[] sTagTable = {
            new TagMatch("br", Tag.Break),
            new TagMatch("hr", Tag.HorizRule),
            new TagMatch("width", Tag.Width),
            new TagMatch("box", Tag.BoxStart),
            new TagMatch("url", Tag.UrlStart),
        };

        /// <summary>
        /// Attempts to parse a tag at the current source position.
        /// </summary>
        /// <remarks>
        /// <para>This attempts to parse the full tag, including the closing tag if such is
        /// appropriate.</para>
        /// </remarks>
        /// <param name="source">Input data source.</param>
        /// <param name="formatter">Output formatter.</param>
        /// <param name="skipLen">Number of characters to advance in data source.</param>
        /// <param name="subSource">Result: data source with tag contents.  May be null.</param>
        /// <param name="requireLineStart">Result: if true, and the output buffer has characters
        ///   in it, they must be flushed before continuing.</param>
        /// <returns>True if the tag was successfully parsed.</returns>
        private bool TryParseTag(DataSource source, Asm65.Formatter formatter,
                out int skipLen, out DataSource subSource, out bool requireLineStart) {
            skipLen = 0;
            requireLineStart = false;
            subSource = null;

            Tag tag = Tag.Unknown;
            foreach (TagMatch pat in sTagTable) {
                if (source.Match(pat.mPatStr, 1)) {
                    tag = pat.mTag;
                    break;
                }
            }
            string tagStr = null;
            if (tag != Tag.Unknown) {
                // Look for the end.
                for (int endpos = source.Posn + 2; endpos < source.Length; endpos++) {
                    char ch = source[endpos];
                    if (ch == ']') {
                        // Found the end of the tag.
                        tagStr = source.Text.Substring(source.Posn, endpos - source.Posn + 1);
                        break;
                    } else if (ch == '\r' || ch == '\n') {
                        // Stop looking if we hit a line break mid-tag.
                        break;
                    }
                }
            }
            if (tagStr == null) {
                return false;
            }
            //Debug.WriteLine("Initial match at " + source.Posn + ": " + tag + " '" + tagStr + "'");

            bool eatNextIfNewline = false;
            switch (tag) {
                case Tag.Break:
                    int brWidth = "[br]".Length;
                    if (tagStr.Length != brWidth) {
                        return false;
                    }
                    if (source.InsideElement) {
                        return false;       // can't use inside a box
                    }
                    skipLen = brWidth;
                    // Just a blank line, but with "suppress prefix" enabled.
                    requireLineStart = eatNextIfNewline = true;
                    subSource = new DataSource("\r\n", 0, source);
                    subSource.SuppressPrefix = true;
                    break;
                case Tag.HorizRule:
                    if (source.InsideNonBoxElement) {
                        return false;
                    }
                    char defaultCh;
                    if (source.InBox) {
                        defaultCh = source.BoxChar;
                    } else {
                        defaultCh = DEFAULT_RULE_CHAR;
                    }
                    if (!HandleHorizRule(tagStr, defaultCh, out skipLen, out char hrChar)) {
                        return false;
                    }
                    int ruleWidth = CalcTextWidth(source, true);
                    StringBuilder rulerSb = new StringBuilder(ruleWidth);
                    for (int i = 0; i < ruleWidth; i++) {
                        rulerSb.Append(hrChar);
                    }
                    rulerSb.Append("\r\n");
                    subSource = new DataSource(rulerSb.ToString(), 0, source);
                    subSource.FullWidth = true;
                    requireLineStart = eatNextIfNewline = true;
                    break;
                case Tag.Width:
                    if (source.InsideElement) {
                        return false;
                    }
                    int newWidth = HandleWidth(tagStr, out skipLen);
                    if (newWidth < 0) {
                        return false;
                    }
                    requireLineStart = eatNextIfNewline = true;
                    mLineWidth = newWidth;
                    break;
                case Tag.BoxStart:
                    if (source.InsideElement) {
                        return false;
                    }
                    char defBoxChar = formatter.FullLineCommentDelimiterBase[0];
                    if (!HandleBox(tagStr, source, defBoxChar, out skipLen, out char boxChar,
                            out bool isBoxCharDef, out string insideBox)) {
                        return false;
                    }
                    requireLineStart = eatNextIfNewline = true;
                    subSource = new DataSource(insideBox, 0, source);
                    subSource.InBox = true;
                    subSource.BoxChar = boxChar;
                    subSource.BoxCharIsDefault = isBoxCharDef;
                    break;
                case Tag.UrlStart:
                    if (source.InsideNonBoxElement) {
                        return false;
                    }
                    if (!HandleUrl(tagStr, source, out skipLen, out string showText)) {
                        return false;
                    }
                    requireLineStart = eatNextIfNewline = false;
                    subSource = new DataSource(showText, 0, source);
                    subSource.InUrl = true;
                    break;
                default:
                    return false;
            }

            // Some tags cause a newline to happen, e.g. [box] and [hr] always start on a new line
            // of output.  It can feel natural to type these on a line by themselves, but that
            // will generate an extra newline unless we suppress it here.
            if (eatNextIfNewline) {
                if (source.Posn + skipLen < source.Length &&
                        source[source.Posn + skipLen] == '\r') {
                    skipLen++;
                }
                if (source.Posn + skipLen < source.Length &&
                        source[source.Posn + skipLen] == '\n') {
                    skipLen++;
                }
            }

            return true;
        }
        /// <summary>
        /// Parses an [hr] or [hr char='x'] tag.  Returns the ruler char, or '\0' on error.
        /// </summary>
        private static bool HandleHorizRule(string tagStr, char defaultChar, out int skipLen,
                out char hrChar) {
            const string simpleForm = "[hr]";
            const string prefix = "[hr char='";
            const string suffix = "']";
            hrChar = '\0';
            skipLen = tagStr.Length;

            if (tagStr.Equals(simpleForm, StringComparison.OrdinalIgnoreCase)) {
                // use default char
                hrChar = defaultChar;
            } else if (tagStr.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)) {
                // char explicitly set
                int charStrLen = tagStr.Length - prefix.Length - suffix.Length;
                if (charStrLen != 1) {
                    return false;
                }
                if (!tagStr.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase)) {
                    return false;
                }
                hrChar = tagStr[prefix.Length];
            } else {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses a [width] tag.  Returns the width, or -1 on error.
        /// </summary>
        private static int HandleWidth(string tagStr, out int skipLen) {
            const string prefix = "[width=";
            const string suffix = "]";
            skipLen = 0;

            int widthStrLen = tagStr.Length - prefix.Length - suffix.Length;
            if (widthStrLen <= 0) {
                return -1;
            }
            if (!tagStr.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase)) {
                return -1;
            }
            if (!tagStr.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase)) {
                return -1;
            }
            string widthStr = tagStr.Substring(prefix.Length, widthStrLen);
            int newWidth;
            if (widthStr == "*") {
                newWidth = DEFAULT_WIDTH;
            } else if (!int.TryParse(widthStr, out newWidth)) {
                Debug.WriteLine("Unable to parse width '" + widthStr + "'");
                return -1;
            }
            if (newWidth < MIN_WIDTH || newWidth > MAX_WIDTH) {
                return -1;
            }
            skipLen = tagStr.Length;
            return newWidth;
        }

        /// <summary>
        /// Parses a [box]...[/box] tag, which could also be [box char='x'].
        /// </summary>
        private static bool HandleBox(string tagStr, DataSource source, char defBoxChar,
                out int skipLen, out char boxChar, out bool isBoxCharDef, out string insideBox) {
            const string startTagDefault = "[box]";
            const string startTagPrefix = "[box char='";
            const string startTagSuffix = "']";
            const string endTag = "[/box]";
            skipLen = 0;
            boxChar = '?';
            isBoxCharDef = false;
            insideBox = "!!!";

            if (tagStr.Equals(startTagDefault, StringComparison.InvariantCultureIgnoreCase)) {
                boxChar = defBoxChar;
                isBoxCharDef = true;
            } else if (tagStr.StartsWith(startTagPrefix,
                            StringComparison.InvariantCultureIgnoreCase) &&
                        tagStr.EndsWith(startTagSuffix,
                            StringComparison.InvariantCultureIgnoreCase) &&
                        tagStr.Length == startTagPrefix.Length + 1 + startTagSuffix.Length) {
                boxChar = tagStr[startTagPrefix.Length];
                isBoxCharDef = false;
            } else {
                return false;
            }

            int boxEndPosn = source.FindNext(endTag);
            if (boxEndPosn < 0) {
                return false;
            }

            int innerLen = boxEndPosn - (source.Posn + tagStr.Length);
            skipLen = tagStr.Length + innerLen + endTag.Length;
            insideBox = "[hr]" + source.Text.Substring(source.Posn + tagStr.Length, innerLen) +
                "[hr]";

            return true;
        }

        /// <summary>
        /// Parses a [url]...[/url] tag, which could also be [url=xyzzy].
        /// </summary>
        private static bool HandleUrl(string tagStr, DataSource source,
                out int skipLen, out string showText) {
            const string simpleStart = "[url]";
            const string linkStartPrefix = "[url=";
            const string linkStartSuffix = "]";
            const string endTag = "[/url]";
            skipLen = 0;
            showText = string.Empty;

            string linkStr;
            if (tagStr.Equals(simpleStart, StringComparison.InvariantCultureIgnoreCase)) {
                // The text is also the link.
                linkStr = string.Empty;
            } else if (tagStr.StartsWith(linkStartPrefix,
                            StringComparison.InvariantCultureIgnoreCase) &&
                        tagStr.EndsWith(linkStartSuffix,
                            StringComparison.InvariantCultureIgnoreCase) &&
                        tagStr.Length > linkStartPrefix.Length + linkStartSuffix.Length) {
                // URI is specified in tag.
                linkStr = tagStr.Substring(linkStartPrefix.Length,
                    tagStr.Length - (linkStartPrefix.Length + linkStartSuffix.Length));
            } else {
                return false;
            }

            int urlEndPosn = source.FindNext(endTag);
            if (urlEndPosn < 0) {
                return false;
            }

            int innerLen = urlEndPosn - (source.Posn + tagStr.Length);
            skipLen = tagStr.Length + innerLen + endTag.Length;
            showText = source.Text.Substring(source.Posn + tagStr.Length, innerLen);
            if (!string.IsNullOrEmpty(linkStr)) {
                showText += " (" + linkStr + ")";
            }
            return true;
        }

        #endregion Fancy

        public override string ToString() {
            if (IsFancy) {
                return "MLC fancy text='" + Text + "'";
            } else {
                return "MLC box=" + BoxMode + " width=" + MaxWidth + " text='" + Text + "'";
            }
        }

        public static bool operator ==(MultiLineComment a, MultiLineComment b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            return a.Text.Equals(b.Text) && a.BoxMode == b.BoxMode && a.MaxWidth == b.MaxWidth
                && a.BackgroundColor == b.BackgroundColor;
        }
        public static bool operator !=(MultiLineComment a, MultiLineComment b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is MultiLineComment && this == (MultiLineComment)obj;
        }
        public override int GetHashCode() {
            return Text.GetHashCode() ^ MaxWidth ^ (BoxMode ? 1 : 0) ^ BackgroundColor.GetHashCode();
        }
    }
}
