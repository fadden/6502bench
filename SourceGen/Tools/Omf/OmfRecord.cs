/*
 * Copyright 2020 faddenSoft
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
using System.Text;

using Asm65;
using CommonUtil;

namespace SourceGen.Tools.Omf {
    /// <summary>
    /// Apple IIgs OMF record.
    /// </summary>
    public class OmfRecord {
        private const int NUMLEN = 4;   // defined by NUMLEN field in header; always 4 for IIgs

        /// <summary>
        /// Offset of record start within file.
        /// </summary>
        public int FileOffset { get; private set; }

        /// <summary>
        /// Total length, in bytes, of this record.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Opcode.
        /// </summary>
        public Opcode Op { get; private set; }

        /// <summary>
        /// Opcode, in human-readable form.
        /// </summary>
        public string OpName { get; private set; }

        /// <summary>
        /// Value, in human-readable form.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Opcode byte definition.
        /// </summary>
        /// <remarks>
        /// Nearly all of the 256 possible values are assigned.  5 are unused, 5 are reserved.
        /// </remarks>
        public enum Opcode : byte {
            END = 0x00,             // all
            CONST_start = 0x01,     // object
            CONST_end = 0xdf,       // object
            ALIGN = 0xe0,           // object
            ORG = 0xe1,             // object
            RELOC = 0xe2,           // load
            INTERSEG = 0xe3,        // load
            USING = 0xe4,           // object
            STRONG = 0xe5,          // object
            GLOBAL = 0xe6,          // object
            GEQU = 0xe7,            // object
            MEM = 0xe8,             // object ("not needed or supported on the Apple IIgs")
            unused_e9 = 0xe9,
            unused_ea = 0xea,
            EXPR = 0xeb,            // object
            ZEXPR = 0xec,           // object
            BEXPR = 0xed,           // object
            RELEXPR = 0xee,         // object
            LOCAL = 0xef,           // object
            EQU = 0xf0,             // object
            DS = 0xf1,              // all
            LCONST = 0xf2,          // all
            LEXPR = 0xf3,           // object
            ENTRY = 0xf4,           // RTL
            cRELOC = 0xf5,          // load
            cINTERSEG = 0xf6,       // load
            SUPER = 0xf7,           // load
            unused_f8 = 0xf8,
            unused_f9 = 0xf9,
            unused_fa = 0xfa,
            General = 0xfb,         // reserved
            Experimental1 = 0xfc,   // reserved
            Experimental2 = 0xfd,   // reserved
            Experimental3 = 0xfe,   // reserved
            Experimental4 = 0xff,   // reserved
        }


        private OmfRecord() { }

        /// <summary>
        /// Creates a new OmfRecord instance from the data at the specified offset.
        /// </summary>
        /// <remarks>
        /// This does not catch segment boundary overruns, unless they happen to overrun
        /// the buffer entirely.  The caller should either pass in a buffer that holds the
        /// exact segment data, or check the return value for excess length.
        /// </remarks>
        /// <param name="data">Data to analyze.</param>
        /// <param name="offset">Offset of start of record.</param>
        /// <param name="version">OMF segment version number.</param>
        /// <param name="labLen">Label length, defined in OMF segment header.</param>
        /// <param name="msgs">Output message holder.</param>
        /// <param name="omfRec">New record instance.</param>
        /// <returns>True on success.</returns>
        public static bool ParseRecord(byte[] data, int offset,
                OmfSegment.SegmentVersion version, int labLen, Formatter formatter,
                List<string> msgs, out OmfRecord omfRec) {
            omfRec = new OmfRecord();
            omfRec.FileOffset = offset;
            try {
                return omfRec.DoParseRecord(data, offset, version, labLen, formatter, msgs);
            } catch (IndexOutOfRangeException ioore) {
                OmfSegment.AddErrorMsg(msgs, offset, "buffer overrun while parsing record");
                Debug.WriteLine("Exception thrown decoding record: " + ioore.Message);
                return false;
            }
        }

        /// <summary>
        /// Parses OMF record data.
        /// </summary>
        /// <param name="data">Data to analyze.</param>
        /// <param name="offset">Offset of start of record.</param>
        /// <param name="version">OMF segment version number.</param>
        /// <param name="labLen">Label length, defined in OMF segment header.</param>
        /// <param name="msgs">Output message holder.</param>
        /// <returns>Parse result code.</returns>
        private bool DoParseRecord(byte[] data, int offset,
                OmfSegment.SegmentVersion version, int labLen, Formatter formatter,
                List<string> msgs) {
            int len = 1;    // 1 byte for the opcode

            Opcode opcode = Op = (Opcode)data[offset++];
            OpName = opcode.ToString();
            Value = string.Empty;

            if (opcode >= Opcode.CONST_start && opcode <= Opcode.CONST_end) {
                // length determined by opcode value
                int count = (int)opcode;
                len += count;
                OpName = "CONST";
                Value = count + " bytes of data";
            } else {
                switch (opcode) {
                    case Opcode.END:
                        break;
                    case Opcode.ALIGN: {
                            int val = GetNum(data, ref offset, ref len);
                            Value = formatter.FormatHexValue(val, 6);
                        }
                        break;
                    case Opcode.ORG: {
                            int val = GetNum(data, ref offset, ref len);
                            Value = "loc " + formatter.FormatAdjustment(val);
                        }
                        break;
                    case Opcode.RELOC: {
                            len += 1 + 1 + 4 + 4;           // 10
                            int width = data[offset];
                            int operandOff = RawData.GetWord(data, offset + 2, 4, false);
                            Value = width + " bytes @" + formatter.FormatHexValue(operandOff, 4);
                        }
                        break;
                    case Opcode.INTERSEG: {
                            len += 1 + 1 + 4 + 2 + 2 + 4;   // 14
                            int width = data[offset];
                            int operandOff = RawData.GetWord(data, offset + 2, 4, false);
                            int segNum = RawData.GetWord(data, offset + 8, 2, false);
                            Value = width + " bytes @" + formatter.FormatHexValue(operandOff, 4) +
                                " (seg " + segNum + ")";
                        }
                        break;
                    case Opcode.USING:
                    case Opcode.STRONG: {
                            string label = GetLabel(data, ref offset, ref len, labLen);
                            Value = "'" + label + "'";
                        }
                        break;
                    case Opcode.GLOBAL:
                    case Opcode.LOCAL: {
                            string label = GetLabel(data, ref offset, ref len, labLen);
                            int bytes;
                            byte type;
                            byte priv = 0;
                            if (version == OmfSegment.SegmentVersion.v0_0) {
                                bytes = data[offset];
                                type = data[offset + 1];
                                offset += 2;
                                len += 2;
                            } else if (version == OmfSegment.SegmentVersion.v1_0) {
                                bytes = data[offset];
                                type = data[offset + 1];
                                priv = data[offset + 2];
                                offset += 3;
                                len += 3;
                            } else {
                                bytes = RawData.GetWord(data, offset, 2, false);
                                type = data[offset + 2];
                                priv = data[offset + 3];
                                offset += 4;
                                len += 4;
                            }
                            Value = (char)type + " '" + label + "' " +
                                formatter.FormatHexValue(bytes, 4) +
                                ((priv == 0) ? "" : " private");
                        }
                        break;
                    case Opcode.GEQU:
                    case Opcode.EQU: {
                            string label = GetLabel(data, ref offset, ref len, labLen);
                            int bytes;
                            byte type;
                            byte priv = 0;
                            if (version == OmfSegment.SegmentVersion.v0_0) {
                                bytes = data[offset];
                                type = data[offset + 1];
                                offset += 2;
                                len += 2;
                            } else if (version == OmfSegment.SegmentVersion.v1_0) {
                                bytes = data[offset];
                                type = data[offset + 1];
                                priv = data[offset + 2];
                                offset += 3;
                                len += 3;
                            } else {
                                bytes = RawData.GetWord(data, offset, 2, false);
                                type = data[offset + 2];
                                priv = data[offset + 3];
                                offset += 4;
                                len += 4;
                            }
                            string expr = GetExpression(data, ref offset, ref len, labLen,
                                formatter, msgs);
                            Value = (char)type + " '" + label + "' " +
                                formatter.FormatHexValue(bytes, 4) +
                                ((priv == 0) ? "" : " private") + " = " + expr;
                        }
                        break;
                    case Opcode.MEM: {
                            int addr1 = GetNum(data, ref offset, ref len);
                            int addr2 = GetNum(data, ref offset, ref len);
                            Value = formatter.FormatHexValue(addr1, 4) + ", " +
                                formatter.FormatHexValue(addr2, 4);
                        }
                        break;
                    case Opcode.EXPR:
                    case Opcode.ZEXPR:
                    case Opcode.BEXPR:
                    case Opcode.LEXPR: {
                            int cap = data[offset++];
                            len++;
                            string expr = GetExpression(data, ref offset, ref len, labLen,
                                formatter, msgs);
                            Value = "(" + cap + ") " + expr;
                        }
                        break;
                    case Opcode.RELEXPR: {
                            int cap = data[offset++];
                            len++;
                            int rel = GetNum(data, ref offset, ref len);
                            string expr = GetExpression(data, ref offset, ref len, labLen,
                                formatter, msgs);
                            Value = "(" + cap + ") " + formatter.FormatAdjustment(rel) + " " + expr;
                        }
                        break;
                    case Opcode.DS: {
                            int count = GetNum(data, ref offset, ref len);
                            Value = count + " bytes of $00";
                        }
                        break;
                    case Opcode.LCONST: {
                            int count = GetNum(data, ref offset, ref len);
                            len += count;
                            Value = count + " bytes of data";
                        }
                        break;
                    case Opcode.cRELOC: {
                            len += 1 + 1 + 2 + 2;           // 6
                            int width = data[offset];
                            int operandOff = RawData.GetWord(data, offset + 2, 2, false);
                            Value = width + " bytes @" + formatter.FormatHexValue(operandOff, 4);
                        }
                        break;
                    case Opcode.cINTERSEG: {
                            len += 1 + 1 + 2 + 1 + 2;       // 7
                            int width = data[offset];
                            int operandOff = RawData.GetWord(data, offset + 2, 2, false);
                            int segNum = data[offset + 4];
                            Value = width + " bytes @" + formatter.FormatHexValue(operandOff, 4) +
                                " (seg " + segNum + ")";
                        }
                        break;
                    case Opcode.SUPER: {
                            int count = GetNum(data, ref offset, ref len);
                            int type = data[offset];
                            len += count;   // count includes type byte
                            Value = (count - 1) + " bytes, type=" +
                                formatter.FormatHexValue(type, 2);

                            if (type > 37) {
                                OmfSegment.AddErrorMsg(msgs, offset,
                                    "found SUPER record with bogus type=$" + type.ToString("x2"));
                                // the length field allows us to skip it, so keep going
                            }
                        }
                        break;
                    case Opcode.General:
                    case Opcode.Experimental1:
                    case Opcode.Experimental2:
                    case Opcode.Experimental3:
                    case Opcode.Experimental4: {
                            OmfSegment.AddInfoMsg(msgs, offset, "found unusual record type " +
                                formatter.FormatHexValue((int)opcode, 2));
                            int count = GetNum(data, ref offset, ref len);
                            len += count;
                        }
                        break;
                    case Opcode.unused_e9:
                    case Opcode.unused_ea:
                    case Opcode.unused_f8:
                    case Opcode.unused_f9:
                    case Opcode.unused_fa:
                        // These are undefined, can't be parsed.
                    default:
                        Debug.Assert(false);
                        return false;
                }
            }
            Length = len;
            //Debug.WriteLine("REC +" + (offset-1).ToString("x6") + " " + this);

            return true;
        }

        private static int GetNum(byte[] data, ref int offset, ref int len) {
            int val = RawData.GetWord(data, offset, NUMLEN, false);
            offset += NUMLEN;
            len += NUMLEN;
            return val;
        }

        private static string GetLabel(byte[] data, ref int offset, ref int len, int labLen) {
            if (labLen == 0) {
                labLen = data[offset++];
                len++;
            }
            string str = Encoding.ASCII.GetString(data, offset, labLen).Trim();
            offset += labLen;
            len += labLen;
            return str;
        }


        /// <summary>
        /// Expression operations.
        /// </summary>
        private enum ExprOp : byte {
            End = 0x00,
            Addition = 0x01,
            Subtraction = 0x02,
            Multiplication = 0x03,
            Division = 0x04,
            IntegerRemainder = 0x05,
            UnaryNegation = 0x06,
            BitShift = 0x07,
            AND = 0x08,
            OR = 0x09,
            EOR = 0x0a,
            NOT = 0x0b,
            LessThenEqualTo = 0x0c,
            GreaterThanEqualTo = 0x0d,
            NotEqual = 0x0e,
            LessThan = 0x0f,
            GreaterThan = 0x10,
            EqualTo = 0x11,
            BitAND = 0x12,
            BitOR = 0x13,
            BitEOR = 0x14,
            BitNOT = 0x15,

            PushLocation = 0x80,
            PushConstant = 0x81,
            PushLabelWeak = 0x82,
            PushLabelValue = 0x83,
            PushLabelLength = 0x84,
            PushLabelType = 0x85,
            PushLabelCount = 0x86,
            PushRelOffset = 0x87,
        }

        private static readonly string[] ExprStrs = new string[] {
            string.Empty,   // 0x00 End
            "+",            // 0x01 Addition
            "-",            // 0x02 Subtraction
            "*",            // 0x03 Multiplication
            "/",            // 0x04 Division
            "%",            // 0x05 Integer Remainder
            "neg",          // 0x06 Unary Negation
            "shift",        // 0x07 Bit Shift
            "&&",           // 0x08 AND
            "||",           // 0x09 OR
            "^^",           // 0x0a EOR
            "!",            // 0x0b NOT
            "<=",           // 0x0c LE
            ">=",           // 0x0d GE
            "!=",           // 0x0e NE
            "<",            // 0x0f LT
            ">",            // 0x10 GT
            "==",           // 0x11 EQ
            "&",            // 0x12 Bit AND
            "|",            // 0x13 Bit OR
            "^",            // 0x14 Bit EOR
            "~",            // 0x15 Bit NOT
        };

        private static string GetExpression(byte[] data, ref int offset, ref int len, int labLen,
                Formatter formatter, List<string> msgs) {
            StringBuilder sb = new StringBuilder();

            bool done = false;
            while (!done) {
                byte operVal = data[offset++];
                len++;

                // Generate an operand string, if appropriate.
                if (operVal > 0 && operVal < ExprStrs.Length) {
                    sb.Append(' ');
                    sb.Append(ExprStrs[operVal]);
                } else {
                    ExprOp oper = (ExprOp)operVal;
                    switch (oper) {
                        case ExprOp.End:
                            done = true;
                            break;
                        case ExprOp.PushLocation:
                            sb.Append(" [loc]");
                            break;
                        case ExprOp.PushConstant: {
                                int val = GetNum(data, ref offset, ref len);
                                sb.Append(' ');
                                sb.Append(formatter.FormatHexValue(val, 4));
                            }
                            break;
                        case ExprOp.PushLabelWeak: {
                                string label = GetLabel(data, ref offset, ref len, labLen);
                                sb.Append(" weak:'");
                                sb.Append(label);
                                sb.Append("'");
                            }
                            break;
                        case ExprOp.PushLabelValue: {
                                string label = GetLabel(data, ref offset, ref len, labLen);
                                sb.Append(" '");
                                sb.Append(label);
                                sb.Append("'");
                            }
                            break;
                        case ExprOp.PushLabelLength: {
                                string label = GetLabel(data, ref offset, ref len, labLen);
                                sb.Append(" len:'");
                                sb.Append(label);
                                sb.Append("'");
                            }
                            break;
                        case ExprOp.PushLabelType: {
                                string label = GetLabel(data, ref offset, ref len, labLen);
                                sb.Append(" typ:'");
                                sb.Append(label);
                                sb.Append("'");
                            }
                            break;
                        case ExprOp.PushLabelCount: {
                                string label = GetLabel(data, ref offset, ref len, labLen);
                                sb.Append(" cnt:'");
                                sb.Append(label);
                                sb.Append("'");
                            }
                            break;
                        case ExprOp.PushRelOffset: {
                                int adj = GetNum(data, ref offset, ref len);
                                sb.Append(" rel:");
                                sb.Append(formatter.FormatAdjustment(adj));
                            }
                            break;
                        default:
                            OmfSegment.AddErrorMsg(msgs, offset,
                                "Found unexpected expression operator " +
                                    formatter.FormatHexValue((int)oper, 2));
                            sb.Append("???");
                            break;
                    }
                }
            }

            if (sb.Length > 0) {
                sb.Remove(0, 1);        // remove leading space
            }
            return sb.ToString();
        }

        public override string ToString() {
            return Length + " " + OpName + " " + Value;
        }
    }
}
