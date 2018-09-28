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

namespace SourceGen.AsmGen {
    /// <summary>
    /// Multi-line string gatherer.  Accumulates characters and raw bytes, emitting
    /// them when we have a full operand's worth.
    /// 
    /// If the delimiter character appears, it will be output inline as a raw byte.
    /// The low-ASCII string ['hello'world'] will become [27,'hello',27,'world',27]
    /// (or something similar).
    /// </summary>
    public class StringGather {
        // Inputs.
        public IGenerator Gen { get; private set; }
        public string Label { get; private set; }
        public string Opcode { get; private set; }
        public string Comment { get; private set; }
        public char Delimiter { get; private set; }
        public char DelimiterReplacement { get; private set; }
        public ByteStyle ByteStyleX { get; private set; }
        public int MaxOperandLen { get; private set; }
        public bool IsTestRun { get; private set; }

        public enum ByteStyle { DenseHex, CommaSep };

        // Outputs.
        public bool HasDelimiter { get; private set; }
        public int NumLinesOutput { get; private set; }

        private char[] mHexChars;

        /// <summary>
        /// Character collection buffer.  The delimiters are written into the buffer
        /// because they're mixed with bytes, particularly when we have to escape the
        /// delimiter character.  Strings might start or end with escaped delimiters,
        /// so we don't add them until we have to.
        private char[] mBuffer;

        /// <summary>
        /// Next available character position.
        /// </summary>
        private int mIndex = 0;

        /// <summary>
        /// State of the buffer, based on the last thing we added.
        /// </summary>
        private enum State {
            Unknown = 0,
            StartOfLine,
            InQuote,
            OutQuote
        }
        private State mState = State.StartOfLine;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gen">Reference back to generator, for output function and
        ///   format options.</param>
        /// <param name="label">Line label.  Appears on first output line only.</param>
        /// <param name="opcode">Opcode to use for all lines.</param>
        /// <param name="comment">End-of-line comment.  Appears on first output line
        ///   only.</param>
        /// <param name="delimiter">String delimiter character.</param>
        /// <param name="isTestRun">If true, no file output is produced.</param>
        public StringGather(IGenerator gen, string label, string opcode,
                string comment, char delimiter, char delimReplace, ByteStyle byteStyle,
                int maxOperandLen, bool isTestRun) {
            Gen = gen;
            Label = label;
            Opcode = opcode;
            Comment = comment;
            Delimiter = delimiter;
            DelimiterReplacement = delimReplace;
            ByteStyleX = byteStyle;
            MaxOperandLen = maxOperandLen;
            IsTestRun = isTestRun;

            mBuffer = new char[MaxOperandLen];
            mHexChars = Gen.SourceFormatter.HexDigits;
        }

        /// <summary>
        /// Write a character into the buffer.
        /// </summary>
        /// <param name="ch">Character to add.</param>
        public void WriteChar(char ch) {
            Debug.Assert(ch >= 0 && ch <= 0xff);
            if (ch == Delimiter) {
                // Must write it as a byte.
                HasDelimiter = true;
                WriteByte((byte)DelimiterReplacement);
                return;
            }

            // If we're at the start of a line, add delimiter, then new char.
            // If we're inside quotes, just add the character.  We must have space for
            //   two chars (new char, close quote).
            // If we're outside quotes, add a comma and delimiter, then the character.
            //   We must have 4 chars remaining (comma, open quote, new char, close quote).
            switch (mState) {
                case State.StartOfLine:
                    mBuffer[mIndex++] = Delimiter;
                    break;
                case State.InQuote:
                    if (mIndex + 2 > MaxOperandLen) {
                        Flush();
                        mBuffer[mIndex++] = Delimiter;
                    }
                    break;
                case State.OutQuote:
                    if (mIndex + 4 > MaxOperandLen) {
                        Flush();
                        mBuffer[mIndex++] = Delimiter;
                    } else {
                        mBuffer[mIndex++] = ',';
                        mBuffer[mIndex++] = Delimiter;
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
            // If we're at the start of a line, just output the byte.
            // If we're inside quotes, emit a delimiter, comma, and the byte.  We must
            //   have space for four (DenseHex) or five (CommaSep) chars.
            // If we're outside quotes, add the byte.  We must have two (DenseHex) or
            //   four (CommaSep) chars remaining.
            switch (mState) {
                case State.StartOfLine:
                    break;
                case State.InQuote:
                    int minWidth = (ByteStyleX == ByteStyle.CommaSep) ? 5 : 4;
                    if (mIndex + minWidth > MaxOperandLen) {
                        Flush();
                    } else {
                        mBuffer[mIndex++] = Delimiter;
                        mBuffer[mIndex++] = ',';
                    }
                    break;
                case State.OutQuote:
                    minWidth = (ByteStyleX == ByteStyle.CommaSep) ? 4 : 2;
                    if (mIndex + minWidth > MaxOperandLen) {
                        Flush();
                    } else {
                        if (ByteStyleX == ByteStyle.CommaSep) {
                            mBuffer[mIndex++] = ',';
                        }
                    }
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            if (ByteStyleX == ByteStyle.CommaSep) {
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
                    mBuffer[mIndex++] = Delimiter;
                    mBuffer[mIndex++] = Delimiter;
                    NumLinesOutput++;
                    break;
                case State.InQuote:
                    // add delimiter and finish
                    mBuffer[mIndex++] = Delimiter;
                    NumLinesOutput++;
                    break;
                case State.OutQuote:
                    // just output it
                    NumLinesOutput++;
                    break;
            }
            if (!IsTestRun) {
                Gen.OutputLine(Label, Opcode, new string(mBuffer, 0, mIndex),
                    Comment);
            }
            mIndex = 0;

            // Erase these after first use so we don't put them on every line.
            Label = Comment = string.Empty;
        }
    }
}
