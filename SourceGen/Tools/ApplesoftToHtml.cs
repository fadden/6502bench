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
using System.Diagnostics;
using System.Text;
using System.Windows.Media;

using CommonUtil;

namespace SourceGen.Tools {
    /// <summary>
    /// Convert Applesoft BASIC to HTML.
    /// </summary>
    /// <remarks>
    /// This is essentially a port of the CiderPress converter
    /// (https://github.com/fadden/ciderpress/blob/master/reformat/BASIC.cpp).
    /// </remarks>
    public class ApplesoftToHtml {
        /*
         * Applesoft BASIC file format:
         *
         *  <16-bit file length>  [DOS 3.3 only; not visible here]
         *  <line> ...
         *  <EOF marker ($0000)>
         *
         * Each line consists of:
         *  <16-bit address of next line (relative to start)>
         *  <16-bit line number, usually 0-63999>
         *  <tokens | characters> ...
         *  <EOL marker ($00)>
         *
         * All values are little-endian.  Numbers are stored as characters.
         */

        private static readonly string[] TOKENS = new string[128] {
            "END", "FOR", "NEXT", "DATA", "INPUT", "DEL", "DIM", "READ",
            "GR", "TEXT", "PR#", "IN#", "CALL", "PLOT", "HLIN", "VLIN",
            "HGR2", "HGR", "HCOLOR=", "HPLOT", "DRAW", "XDRAW", "HTAB", "HOME",
            "ROT=", "SCALE=", "SHLOAD", "TRACE", "NOTRACE", "NORMAL", "INVERSE", "FLASH",
            "COLOR=", "POP", "VTAB", "HIMEM:", "LOMEM:", "ONERR", "RESUME", "RECALL",
            "STORE", "SPEED=", "LET", "GOTO", "RUN", "IF", "RESTORE", "&",
            "GOSUB", "RETURN", "REM", "STOP", "ON", "WAIT", "LOAD", "SAVE",
            "DEF", "POKE", "PRINT", "CONT", "LIST", "CLEAR", "GET", "NEW",
            "TAB(", "TO", "FN", "SPC(", "THEN", "AT", "NOT", "STEP",
            "+", "-", "*", "/", "^", "AND", "OR", ">",
            "=", "<", "SGN", "INT", "ABS", "USR", "FRE", "SCRN(",
            "PDL", "POS", "SQR", "RND", "LOG", "EXP", "COS", "SIN",
            "TAN", "ATN", "PEEK", "LEN", "STR$", "VAL", "ASC", "CHR$",
            "LEFT$", "RIGHT$", "MID$", "ERROR", "ERROR", "ERROR", "ERROR", "ERROR",
            "ERROR", "ERROR", "ERROR", "ERROR", "ERROR", "ERROR", "ERROR", "ERROR",
            "ERROR", "ERROR", "ERROR", "ERROR", "ERROR", "ERROR", "ERROR", "ERROR"
        };
        private const int TOK_REM = 0xb2;

        private Color mDefaultColor = Color.FromArgb(0xff, 0x40, 0x40, 0x40);   // Dark Grey
        private Color mLineNumColor = Color.FromArgb(0xff, 0x40, 0x40, 0x40);   // Dark Grey
        private Color mKeywordColor = Color.FromArgb(0xff, 0x00, 0x00, 0x00);   // Black
        private Color mCommentColor = Color.FromArgb(0xff, 0x00, 0x80, 0x00);   // Medium Green
        private Color mStringColor =  Color.FromArgb(0xff, 0x00, 0x00, 0x80);   // Medium Blue
        private Color mColonColor =   Color.FromArgb(0xff, 0xff, 0x00, 0x00);   // Red

        private Color mCurrentColor;
        private bool mInSpan;

        public ApplesoftToHtml() {
            // maybe do something with alternate color maps?
        }

        /// <summary>
        /// Converts a buffer with an Applesoft program into colorful HTML.
        /// </summary>
        /// <param name="data">Applesoft program.</param>
        /// <returns>String with formatted text.</returns>
        public string Convert(byte[] data) {
            Debug.WriteLine("Conversion starting");

            if (data.Length < 2) {
                return "BAS truncated?";
            }

            InitColor();

            StringBuilder sb = new StringBuilder();
            sb.Append("<div>\r\n");

            // Set the default color in the <pre> style.
            sb.Append("<pre style=\"");
            OutputHtmlColor(mDefaultColor, sb);
            sb.Append("\">");

            int offset = 0;
            while (offset < data.Length) {
                int nextAddr, lineNum;
                bool inQuote = false;
                bool inRem = false;

                nextAddr = Read16(data, ref offset);
                if (nextAddr == 0) {
                    if (data.Length - offset > 1) {
                        Debug.WriteLine("WARNING: BAS ended early, at +" + offset.ToString("x6"));
                    }
                    break;
                }

                // print line number
                lineNum = Read16(data, ref offset);
                SetColor(mLineNumColor, sb);
                sb.Append(' ');
                sb.Append(lineNum);
                sb.Append(' ');
                SetColor(mDefaultColor, sb);

                while (offset < data.Length && data[offset] != 0) {
                    char tokVal = (char)data[offset];
                    if ((tokVal & 0x80) != 0) {
                        // token
                        SetColor(mKeywordColor, sb);
                        sb.Append(' ');
                        sb.Append(TOKENS[tokVal & 0x7f]);
                        sb.Append(' ');
                        SetColor(mDefaultColor, sb);

                        if (tokVal == TOK_REM) {
                            // REM -- do rest of line in green
                            SetColor(mCommentColor, sb);
                            inRem = true;
                        }
                    } else {
                        // non-token character
                        if (tokVal == '"' && !inRem) {
                            if (!inQuote) {
                                SetColor(mStringColor, sb);
                                sb.Append(tokVal);
                            } else {
                                sb.Append(tokVal);
                                SetColor(mDefaultColor, sb);
                            }
                            inQuote = !inQuote;
                        } else if (tokVal == ':' && !inRem && !inQuote) {
                            SetColor(mColonColor, sb);
                            sb.Append(tokVal);
                            SetColor(mDefaultColor, sb);
                        } else if (inRem && tokVal == '\r') {
                            sb.Append("\r\n");      // embedded CR
                        } else if (tokVal < 0x20) {
                            // control character, in string or REM
                            //sb.Append("&#x2022;");
                            // output with Unicode "control pictures" block
                            sb.Append("&#x");
                            sb.Append((tokVal + 0x2400).ToString("x4"));
                            sb.Append(";");
                        } else {
                            // Output as ASCII value.
                            sb.Append(tokVal);
                        }
                    }

                    offset++;
                }

                SetColor(mDefaultColor, sb);
                inQuote = inRem = false;

                offset++;

                sb.Append("\r\n");
            }

            sb.Append("</pre>\r\n");
            sb.Append("</div>\r\n");

            return sb.ToString();
        }

        /// <summary>
        /// Initializes the color management.
        /// </summary>
        private void InitColor() {
            mCurrentColor = mDefaultColor;
            mInSpan = false;
        }

        /// <summary>
        /// Changes the current text color.  Does nothing if the new color is the same as the
        /// old color.  Special-cases the default color.
        /// </summary>
        /// <remarks>
        /// This approach is sub-optimal -- if we have two tokens in a row they'll be put
        /// in separate spans.  It works well enough for now.
        /// </remarks>
        /// <param name="newColor">Color to change to.</param>
        /// <param name="sb">StringBuilder that holds output.</param>
        private void SetColor(Color newColor, StringBuilder sb) {
            if (newColor == mCurrentColor) {
                return;
            }
            if (mInSpan) {
                sb.Append("</span>");
                mInSpan = false;
            }
            if (newColor != mDefaultColor) {
                sb.Append("<span style=\"");
                OutputHtmlColor(newColor, sb);
                sb.Append("\">");
                mInSpan = true;
            }
            mCurrentColor = newColor;
        }

        /// <summary>
        /// Outputs a color in the form "#rrggbb".
        /// </summary>
        private void OutputHtmlColor(Color color, StringBuilder sb) {
            sb.Append("color:#");
            sb.Append(color.R.ToString("x2"));
            sb.Append(color.G.ToString("x2"));
            sb.Append(color.B.ToString("x2"));
        }

        /// <summary>
        /// Reads 16 bits of little-endian data from the buffer.
        /// </summary>
        /// <param name="data">Data buffer.</param>
        /// <param name="offset">Initial offset.  Will be increased by two on a successful
        ///   read.</param>
        /// <returns>A value between 0 and 65535, or -1 if we ran off the end.</returns>
        private static int Read16(byte[] data, ref int offset) {
            if (offset + 2 > data.Length) {
                Debug.WriteLine("ERROR: overran Applesoft");
                return -1;
            }
            offset += 2;
            return data[offset-2] | (data[offset - 1] << 8);
        }
    }
}
