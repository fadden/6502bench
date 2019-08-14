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

namespace Asm65 {
    /// <summary>
    /// String pseudo-op formatter.  Handles character encoding conversion and quoting of
    /// delimiters and non-printable characters.
    /// </summary>
    public class StringOpFormatter {
        /// <summary>
        /// Text direction.  If text is stored in reverse order, we want to un-reverse it to
        /// make it readable.  This gets tricky for a multi-line item.  For the assembler we
        /// want to break it into lines and then reverse each chunk, but on screen we want to
        /// reverse the entire thing as a single block.
        /// </summary>
        public enum ReverseMode { Forward, LineReverse, FullReverse };

        public CharEncoding.Convert CharConv { get; set; }

        // Output format for raw (non-printable) characters.  Most assemblers use comma-separated
        // hex values, some allow dense hex strings.
        public enum RawOutputStyle { DenseHex, CommaSep };

        // Outputs.
        public bool HasEscapedText { get; private set; }
        public List<string> Lines { get; private set; }

        private Formatter.DelimiterDef mDelimiterDef;
        private RawOutputStyle mRawStyle;
        private int mMaxOperandLen;

        // Reference to array with 16 hex digits.  (May be upper or lower case.)
        private char[] mHexChars;

        /// <summary>
        /// Character collection buffer.  The delimiters are written into the buffer
        /// because they're mixed with bytes, particularly when we have to escape the
        /// delimiter character.  Strings might start or end with escaped delimiters,
        /// so we don't add them until we have to.
        /// </summary>
        private char[] mBuffer;

        /// <summary>
        /// Next available character position.
        /// </summary>
        private int mIndex;

        /// <summary>
        /// State of the buffer, based on the last thing we added.
        /// </summary>
        private enum State {
            Unknown = 0,
            StartOfLine,
            InQuote,
            OutQuote,
            Finished
        }
        private State mState;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="formatter">Reference to text formatter.</param>
        /// <param name="delimiterDef">String delimiter values.</param>
        /// <param name="byteStyle">How to format raw byte data.</param>
        /// <param name="maxOperandLen">Maximum line length.</param>
        /// <param name="charConv">Character conversion delegate.</param>
        public StringOpFormatter(Formatter formatter, Formatter.DelimiterDef delimiterDef,
                RawOutputStyle byteStyle, int maxOperandLen, CharEncoding.Convert charConv) {
            mRawStyle = byteStyle;
            mMaxOperandLen = maxOperandLen;
            CharConv = charConv;

            mDelimiterDef = delimiterDef;
            mBuffer = new char[mMaxOperandLen];
            mHexChars = formatter.HexDigits;
            Lines = new List<string>();

            // suffix not used, so we don't expect it to be set to something
            Debug.Assert(string.IsNullOrEmpty(mDelimiterDef.Suffix));

            Reset();
        }

        public void Reset() {
            mState = State.StartOfLine;
            mIndex = 0;
            Lines.Clear();

            // Copy the prefix string into the buffer for the first line.
            for (int i = 0; i < mDelimiterDef.Prefix.Length; i++) {
                mBuffer[mIndex++] = mDelimiterDef.Prefix[i];
            }
        }

        /// <summary>
        /// Write a character into the buffer.  If the character matches the delimiter, or
        /// isn't printable, the raw character value will be written as a byte instead.
        /// </summary>
        /// <param name="rawCh">Raw character value.</param>
        public void WriteChar(byte rawCh) {
            Debug.Assert(mState != State.Finished);

            char ch = CharConv(rawCh);
            if (ch == mDelimiterDef.OpenDelim || ch == mDelimiterDef.CloseDelim ||
                    ch == CharEncoding.UNPRINTABLE_CHAR) {
                // Must write it as a byte.
                WriteByte(rawCh);
                return;
            }

            // If we're at the start of a line, add delimiter, then new char.
            // If we're inside quotes, just add the character.  We must have space for
            //   two chars (new char, close quote).
            // If we're outside quotes, add a comma and delimiter, then the character.
            //   We must have 4 chars remaining (comma, open quote, new char, close quote).
            switch (mState) {
                case State.StartOfLine:
                    mBuffer[mIndex++] = mDelimiterDef.OpenDelim;
                    break;
                case State.InQuote:
                    if (mIndex + 2 > mMaxOperandLen) {
                        Flush();
                        mBuffer[mIndex++] = mDelimiterDef.OpenDelim;
                    }
                    break;
                case State.OutQuote:
                    if (mIndex + 4 > mMaxOperandLen) {
                        Flush();
                        mBuffer[mIndex++] = mDelimiterDef.OpenDelim;
                    } else {
                        mBuffer[mIndex++] = ',';
                        mBuffer[mIndex++] = mDelimiterDef.OpenDelim;
                    }
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
            mBuffer[mIndex++] = ch;
            mState = State.InQuote;
        }

        /// <summary>
        /// Write a hex value into the buffer.
        /// </summary>
        /// <param name="val">Value to add.</param>
        public void WriteByte(byte val) {
            Debug.Assert(mState != State.Finished);

            HasEscapedText = true;

            // If we're at the start of a line, just output the byte.
            // If we're inside quotes, emit a delimiter, comma, and the byte.  We must
            //   have space for four (DenseHex) or five (CommaSep) chars.
            // If we're outside quotes, add the byte.  We must have two (DenseHex) or
            //   four (CommaSep) chars remaining.
            switch (mState) {
                case State.StartOfLine:
                    break;
                case State.InQuote:
                    int minWidth = (mRawStyle == RawOutputStyle.CommaSep) ? 5 : 4;
                    if (mIndex + minWidth > mMaxOperandLen) {
                        Flush();
                    } else {
                        mBuffer[mIndex++] = mDelimiterDef.CloseDelim;
                        mBuffer[mIndex++] = ',';
                    }
                    break;
                case State.OutQuote:
                    minWidth = (mRawStyle == RawOutputStyle.CommaSep) ? 4 : 2;
                    if (mIndex + minWidth > mMaxOperandLen) {
                        Flush();
                    } else {
                        if (mRawStyle == RawOutputStyle.CommaSep) {
                            mBuffer[mIndex++] = ',';
                        }
                    }
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            if (mRawStyle == RawOutputStyle.CommaSep) {
                mBuffer[mIndex++] = '$';
            }
            mBuffer[mIndex++] = mHexChars[val >> 4];
            mBuffer[mIndex++] = mHexChars[val & 0x0f];
            mState = State.OutQuote;
        }

        /// <summary>
        /// Tells the object to flush any pending data to the output.
        /// </summary>
        public void Finish() {
            Flush();
        }

        /// <summary>
        /// Outputs the buffer of pending data.  A closing delimiter will be added if needed.
        /// </summary>
        private void Flush() {
            switch (mState) {
                case State.StartOfLine:
                    // empty string; put out a pair of delimiters
                    mBuffer[mIndex++] = mDelimiterDef.OpenDelim;
                    mBuffer[mIndex++] = mDelimiterDef.CloseDelim;
                    break;
                case State.InQuote:
                    // add delimiter and finish
                    mBuffer[mIndex++] = mDelimiterDef.CloseDelim;
                    break;
                case State.OutQuote:
                    // just output it
                    break;
            }

            string newStr = new string(mBuffer, 0, mIndex);
            Debug.Assert(newStr.Length <= mMaxOperandLen);
            Lines.Add(newStr);

            mState = State.Finished;

            mIndex = 0;
        }

        /// <summary>
        /// Feeds the bytes into the StringGather.
        /// </summary>
        public void FeedBytes(byte[] data, int offset, int length, int leadingBytes,
                ReverseMode revMode) {
            int startOffset = offset;
            int strEndOffset = offset + length;

            // Write leading bytes.  This is used for the 8- or 16-bit length (when no
            // appropriate pseudo-op is available), because we want to output that as hex
            // even if it maps to a printable character.
            while (leadingBytes-- > 0) {
                WriteByte(data[offset++]);
            }
            if (revMode == ReverseMode.LineReverse) {
                // Max per line is line length minus the two delimiters.  We don't allow
                // any hex quoting in reversed text, so this always works.  (If somebody
                // does try to reverse text with delimiters or unprintable chars, we'll
                // blow out the line limit, but for a cross-assembler that should be purely
                // cosmetic.)
                int maxPerLine = mMaxOperandLen - 2;
                int numBlockLines = (length + maxPerLine - 1) / maxPerLine;

                for (int chunk = 0; chunk < numBlockLines; chunk++) {
                    int chunkOffset = startOffset + chunk * maxPerLine;
                    int endOffset = chunkOffset + maxPerLine;
                    if (endOffset > strEndOffset) {
                        endOffset = strEndOffset;
                    }
                    for (int off = endOffset - 1; off >= chunkOffset; off--) {
                        WriteChar(data[off]);
                    }
                }
            } else if (revMode == ReverseMode.FullReverse) {
                for (; offset < strEndOffset; offset++) {
                    int posn = startOffset + (strEndOffset - offset) - 1;
                    WriteChar(data[posn]);
                }
            } else {
                Debug.Assert(revMode == ReverseMode.Forward);
                for (; offset < strEndOffset; offset++) {
                    WriteChar(data[offset]);
                }
            }

            Finish();
        }
    }
}
