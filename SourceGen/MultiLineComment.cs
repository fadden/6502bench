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
            if (IsFancy) {
                Debug.Assert(string.IsNullOrEmpty(textPrefix));
                lines = FormatFancyText(formatter);
            } else {
                lines = FormatSimpleText(formatter, textPrefix);
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

        private enum Tag {
            Unknown = 0, Width, HorizRule, Break,
            BoxStart, BoxEnd, UrlStart, UrlEnd, SymStart, SymEnd
        }

        private class DataSource {
            private string mString;
            private int mPosn;
            public bool mInBox, mInUrl, mInSym;

            public char this[int i] {
                get { return mString[i]; }
            }
            public int Posn { get { return mPosn; } set { mPosn = value; } }
            public int Length => mString.Length;
            public char CurChar => mString[mPosn];      // mostly for debugger

            public DataSource(string str, int posn, DataSource outer) {
                mString = str;
                mPosn = posn;

                if (outer != null) {
                    // Inherit the values from the "outer" source.
                    mInBox = outer.mInBox;
                    mInUrl = outer.mInUrl;
                    mInSym = outer.mInSym;
                }
            }
        }
        private Stack<DataSource> mSourceStack = new Stack<DataSource>();
        private StringBuilder mLineBuilder = new StringBuilder(MAX_WIDTH);

        private const char DEFAULT_CHAR = '\0';

        private int mLineWidth;
        private char mBoxCharOrDef, mHorizRuleCharOrDef;
        private char mBoxCharActual;
        private bool mEscapeNext, mEatNextIfNewline;
        private string mLinePrefix, mBoxPrefix;
        private bool mDebugMode;

        /// <summary>
        /// Calculates the width of the usable text area, given the current attributes.
        /// </summary>
        private int CalcTextWidth(DataSource source) {
            if (source.mInBox) {
                if (mBoxCharOrDef == DEFAULT_CHAR) {
                    // Leave space for left/right box edges.
                    return mLineWidth - mBoxPrefix.Length - 4;
                } else {
                    // Also leave space for a leading comment delimiter, even if the chosen
                    // box char happens to match the current delimiter.  It might not match when
                    // it's rendered for asm gen, and we don't want the output to change.
                    return mLineWidth - mBoxPrefix.Length - 5;
                }
            } else {
                return mLineWidth - mLinePrefix.Length;
            }
        }

        /// <summary>
        /// Generates one or more lines of formatted text, using the fancy formatter.
        /// </summary>
        /// <param name="formatter">Formatter, with comment delimiters.</param>
        /// <returns>List of formatted strings.</returns>
        private List<string> FormatFancyText(Asm65.Formatter formatter) {
            Debug.Assert(SPACES.Length == MAX_WIDTH);

            mLineWidth = DEFAULT_WIDTH;
            mBoxCharOrDef = mHorizRuleCharOrDef = DEFAULT_CHAR;
            mEscapeNext = mEatNextIfNewline = false;
            mDebugMode = formatter.DebugLongComments;
            mSourceStack.Clear();

            mLinePrefix = formatter.FullLineCommentDelimiterPlus;   // does not change
            mBoxPrefix = formatter.FullLineCommentDelimiterBase;    // changes if box char set
            mBoxCharActual = mBoxPrefix[0];

            DataSource source = new DataSource(Text, 0, null);
            int textWidth = CalcTextWidth(source);

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
                    if (mSourceStack.Count != 0) {
                        source = mSourceStack.Pop();
                        textWidth = CalcTextWidth(source);
                        continue;   // resume earlier string
                    }
                    break;      // all done
                }

                if (source.CurChar == '\r' || source.CurChar == '\n') {
                    // Explicit line break.  If it's a CRLF, eat both.
                    if (source.CurChar == '\r' && source.Posn + 1 < source.Length &&
                            source[source.Posn + 1] == '\n') {
                        source.Posn++;
                    }

                    mEscapeNext = false;        // can't escape newlines

                    if (mEatNextIfNewline) {
                        mEatNextIfNewline = false;
                    } else {
                        // Output what we have.
                        OutputLine(outBuf, outIndex, source.mInBox, lines);
                        outIndex = 0;
                        outBreakIndex = -1;
                    }
                    source.Posn++;
                    continue;
                }
                mEatNextIfNewline = false;

                char thisCh = source.CurChar;
                if (thisCh == '\\') {
                    if (!mEscapeNext) {
                        mEscapeNext = true;
                        source.Posn++;      // eat the backslash
                        continue;           // restart loop; backslash might have been last char
                    }
                } else if (thisCh == '[' && !mEscapeNext) {
                    // Start of format tag?
                    if (TryParseTag(source, out int skipLen, out DataSource subSource)) {
                        source.Posn += skipLen;
                        if (subSource != null) {
                            mSourceStack.Push(source);
                            source = subSource;
                            textWidth = CalcTextWidth(source);
                        }
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

                // We need to add a character to the buffer.  Will this put us over the limit?
                if (outIndex == textWidth) {
                    int outputCount;
                    if (outBreakIndex <= 0) {
                        // No break found, or break char was at start of line.  Just chop what
                        // we have.
                        outputCount = outIndex;
                    } else {
                        // Break was a hyphen or space.
                        outputCount = outBreakIndex;

                    }
                    int adj = 0;
                    if (outBuf[outputCount] == '-') {
                        // Break was a hyphen, include it.
                        adj = 1;
                    }

                    // Output everything up to the break point, but not the break char itself
                    // unless it's a hyphen.
                    OutputLine(outBuf, outputCount + adj, source.mInBox, lines);

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

                if (source.CurChar == '-') {
                    // Can break on hyphen if it fits in line.
                    outBreakIndex = outIndex;
                }

                outBuf[outIndex++] = source[source.Posn++];
            }

            // If we didn't end with a CRLF, output the last bits.
            if (outIndex > 0) {
                OutputLine(outBuf, outIndex, source.mInBox, lines);
            }

            return lines;
        }

        /// <summary>
        /// Attempts to parse a tag at the current source position.
        /// </summary>
        /// <remarks>
        /// <para>This attempts to parse the full tag, including the closing tag if such is
        /// appropriate.</para>
        /// </remarks>
        /// <param name="source">Input data source.</param>
        /// <param name="skipLen">Number of characters to advance in data source.</param>
        /// <param name="subSource">Result: data source with tag contents.  May be null.</param>
        /// <returns>True if the tag was successfully parsed.</returns>
        private bool TryParseTag(DataSource source, out int skipLen, out DataSource subSource) {
            skipLen = 0;
            subSource = null;
            // TODO
            return false;
        }

        /// <summary>
        /// Adds the contents of the output buffer to the line list, prefixing it with comment
        /// delimiters and/or wrapping it in a box.
        /// </summary>
        /// <param name="outBuf">Output buffer.</param>
        /// <param name="length">Length of data in output buffer.</param>
        /// <param name="inBox">True if we're inside a box.</param>
        /// <param name="lines">Line list to add the line to.</param>
        private void OutputLine(char[] outBuf, int length, bool inBox, List<string> lines) {
            Debug.Assert(length >= 0);
            mLineBuilder.Clear();
            if (inBox) {
                mLineBuilder.Append(mBoxPrefix);
                mLineBuilder.Append(outBuf, 0, length);
                int trailingCount = mLineWidth - mBoxPrefix.Length - length - 1;
                if (trailingCount > 0) {
                    mLineBuilder.Append(SPACES, 0, trailingCount);
                }
                mLineBuilder.Append(mBoxCharActual);
            } else {
                mLineBuilder.Append(mLinePrefix);
                mLineBuilder.Append(outBuf, 0, length);
            }
            string str = mLineBuilder.ToString();
            if (mDebugMode) {
                str = str.Replace(' ', '\u2219');   // replace spaces with BULLET OPERATOR
            }
            lines.Add(str);
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
