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

namespace SourceGenWPF {
    /// <summary>
    /// Representation of a multi-line comment, which is a string plus some format directives.
    /// Used for long comments and notes.
    /// 
    /// Instances are immutable.
    /// </summary>
    public class MultiLineComment {
        /// <summary>
        /// If set, sticks a MaxWidth "ruler" at the top, and makes spaces visible.
        /// </summary>
        public static bool DebugShowRuler { get; set; }

        /// <summary>
        /// Unformatted text.
        /// </summary>
        public string Text { get; private set; }

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
        /// Constructor.  Object will have a max width of 80 and not be boxed.
        /// </summary>
        /// <param name="text">Unformatted comment text.</param>
        public MultiLineComment(string text) {
            Debug.Assert(text != null);     // empty string is okay
            Text = text;
            BoxMode = false;
            MaxWidth = 80;
            BackgroundColor = Color.FromArgb(0, 0, 0, 0);
        }

        /// <summary>
        /// Constructor.  Used for long comments.
        /// </summary>
        /// <param name="text">Unformatted text.</param>
        /// <param name="boxMode">Set to true to enable box mode.</param>
        /// <param name="maxWidth">Maximum line width.</param>
        public MultiLineComment(string text, bool boxMode, int maxWidth) : this(text) {
            Debug.Assert((!boxMode && maxWidth > 1) || (boxMode && maxWidth > 5));
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

        /// <summary>
        /// Generates one or more lines of formatted text.
        /// </summary>
        /// <param name="formatter">Formatter, with comment delimiters.</param>
        /// <param name="textPrefix">String to prepend to text before formatting.  If this
        ///   is non-empty, comment delimiters aren't emitted.  (Used for notes.)</param>
        /// <returns>Array of formatted strings.</returns>
        public List<string> FormatText(Asm65.Formatter formatter, string textPrefix) {
            const char boxChar = '*';
            const char spcRep = '\u2219';
            string workString = string.IsNullOrEmpty(textPrefix) ? Text : textPrefix + Text;
            List<string> lines = new List<string>();

            string linePrefix;
            if (!string.IsNullOrEmpty(textPrefix)) {
                linePrefix = string.Empty;
            } else if (BoxMode) {
                linePrefix = formatter.BoxLineCommentDelimiter;
            } else {
                linePrefix = formatter.FullLineCommentDelimiter;
            }

            StringBuilder sb = new StringBuilder(MaxWidth);
            if (DebugShowRuler) {
                for (int i = 0; i < MaxWidth; i++) {
                    sb.Append((i % 10).ToString());
                }
                lines.Add(sb.ToString());
                sb.Clear();
            }
            string boxLine, spaces;
            if (BoxMode) {
                for (int i = 0; i < MaxWidth - linePrefix.Length; i++) {
                    sb.Append(boxChar);
                }
                boxLine = sb.ToString();
                sb.Clear();
                for (int i = 0; i < MaxWidth; i++) {
                    sb.Append(' ');
                }
                spaces = sb.ToString();
                sb.Clear();

            } else {
                boxLine = spaces = null;
            }

            if (BoxMode && workString.Length > 0) {
                lines.Add(linePrefix + boxLine);
            }

            int lineWidth = BoxMode ?
                    MaxWidth - linePrefix.Length - 4 :
                    MaxWidth - linePrefix.Length;
            int startIndex = 0;
            int breakIndex = -1;
            for (int i = 0; i < workString.Length; i++) {
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

                if (workString[i] == '\r' || workString[i] == '\n') {
                    // explicit line break, emit line
                    string str = workString.Substring(startIndex, i - startIndex);
                    if (DebugShowRuler) { str = str.Replace(' ', spcRep); }
                    if (BoxMode) {
                        if (str == "" + boxChar) {
                            // asterisk on a line by itself means "output row of asterisks"
                            str = linePrefix + boxLine;
                        } else {
                            int padLen = lineWidth - str.Length;
                            str = linePrefix + boxChar + " " + str +
                                spaces.Substring(0, padLen + 1) + boxChar;
                        }
                    } else {
                        str = linePrefix + str;
                    }
                    lines.Add(str);
                    // Eat the LF in CRLF.  We don't actually work right with just LF,
                    // because this will consume LFLF, but it's okay to insist that the
                    // string use CRLF for line breaks.
                    if (i < workString.Length - 1 && workString[i + 1] == '\n') {
                        i++;
                    }
                    startIndex = i + 1;
                    breakIndex = -1;
                } else if (workString[i] == ' ') {
                    // can break on a space even if it's one char too far
                    breakIndex = i;
                }

                if (i - startIndex >= lineWidth) {
                    // this character was one too many, break line one back
                    if (breakIndex <= 0) {
                        // no break found, just chop it
                        string str = workString.Substring(startIndex, i - startIndex);
                        if (DebugShowRuler) { str = str.Replace(' ', spcRep); }
                        if (BoxMode) {
                            str = linePrefix + boxChar + " " + str + " " + boxChar;
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
                        if (DebugShowRuler) { str = str.Replace(' ', spcRep); }
                        if (BoxMode) {
                            int padLen = lineWidth - str.Length;
                            str = linePrefix + boxChar + " " + str +
                                spaces.Substring(0, padLen + 1) + boxChar;
                        } else {
                            str = linePrefix + str;
                        }
                        lines.Add(str);
                        startIndex = breakIndex + 1;
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
                if (DebugShowRuler) { str = str.Replace(' ', spcRep); }
                if (BoxMode) {
                    int padLen = lineWidth - str.Length;
                    str = linePrefix + boxChar + " " + str +
                        spaces.Substring(0, padLen + 1) + boxChar;
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


        public override string ToString() {
            return "MLC box=" + BoxMode + " width=" + MaxWidth + " text='" + Text + "'";
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
