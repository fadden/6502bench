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

namespace SourceGen {
    /// <summary>
    /// Symbolic representation of a value.  Instances are immutable.
    /// </summary>
    public class Symbol {
        public const char UNCERTAIN_CHAR = '?';

        /// <summary>
        /// How was the symbol defined?
        /// </summary>
        public enum Source {
            // These are in order of highest to lowest precedence.  This matters when
            // looking up a symbol by value from the symbol table, because multiple symbols
            // can have the same value.
            Unknown = 0,
            User,               // user-defined; only used for internal address labels
            Project,            // external address or const, from project configuration file
            Platform,           // external address or const, from platform definition file
            Auto,               // auto-generated internal address label
            Variable            // external address or const, from local variable table
        }

        /// <summary>
        /// Unique or non-unique address label?  Is it required to be global or exported?
        /// Constants get a separate type.
        /// </summary>
        public enum Type {
            Unknown = 0,

            NonUniqueLocalAddr, // non-unique local symbol, may be promoted to global
            LocalOrGlobalAddr,  // unique local symbol, may be promoted to global
            GlobalAddr,         // unique global symbol
            GlobalAddrExport,   // unique global symbol; included in linker export table

            ExternalAddr,       // reference to address outside program (e.g. platform sym file)
            Constant            // constant value
        }

        /// <summary>
        /// User-specified commentary on the label.
        /// </summary>
        public enum LabelAnnotation {
            None = 0,
            Uncertain,          // user isn't sure if this is correct
            Generated           // label was generated, e.g. address table formatter
        }

        /// <summary>
        /// Unique label.
        /// </summary>
        /// <remarks>
        /// Non-unique labels have extra stuff at the end to make them unique.  That is
        /// included here, so that the Label field is still viable as a unique identifier.
        /// </remarks>
        public string Label { get; private set; }

        /// <summary>
        /// Symbol's 32-bit numeric value.
        /// </summary>
        /// <remarks>
        /// For address types, the value should be constained to [0,2^24).  For constants,
        /// all values are valid.
        /// </remarks>
        public int Value { get; private set; }

        /// <summary>
        /// Symbol origin, e.g. auto-generated or entered by user.  Enum values are in
        /// priority order.
        /// </summary>
        public Source SymbolSource { get; private set; }

        /// <summary>
        /// Type of symbol, e.g. local or global.
        /// </summary>
        public Type SymbolType { get; private set; }

        /// <summary>
        /// Notes on the label.
        /// </summary>
        public LabelAnnotation LabelAnno { get; private set; }

        /// <summary>
        /// Two-character string representation of Source and Type, for display in the UI.
        /// Generated from SymbolSource and SymbolType.
        /// </summary>
        public string SourceTypeString { get; private set; }

        /// <summary>
        /// Label with annotations.  Generated from Label and LabelAnno.
        /// </summary>
        public string AnnotatedLabel { get; private set; }


        /// <summary>
        /// True if the symbol's type is an internal label (auto or user).  Will be false
        /// for external addresses (including variables) and constants.
        /// </summary>
        public bool IsInternalLabel {
            get { return SymbolSource == Source.User || SymbolSource == Source.Auto; }
        }

        /// <summary>
        /// True if the symbol is a local variable.
        /// </summary>
        public bool IsVariable {
            get { return SymbolSource == Source.Variable; }
        }

        /// <summary>
        /// True if the symbol represents a constant value.
        /// </summary>
        public bool IsConstant {
            get { return SymbolType == Type.Constant; }
        }


        // No nullary constructor.
        private Symbol() { }

        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="label">Label string.  Syntax assumed valid.</param>
        /// <param name="value">Symbol value.</param>
        /// <param name="source">User-defined, auto-generated, ?</param>
        /// <param name="type">Type of symbol this is.</param>
        public Symbol(string label, int value, Source source, Type type,
                LabelAnnotation labelAnno) {
            Debug.Assert(Asm65.Label.ValidateLabel(label));
            Debug.Assert(type != Type.NonUniqueLocalAddr);
            Label = label;
            Value = value;
            SymbolType = type;
            SymbolSource = source;
            LabelAnno = labelAnno;

            // Generate SourceTypeString.
            char sc, tc;
            switch (SymbolSource) {
                case Source.Auto:               sc = 'A';   break;
                case Source.User:               sc = 'U';   break;
                case Source.Platform:           sc = 'P';   break;
                case Source.Project:            sc = 'J';   break;
                case Source.Variable:           sc = 'V';   break;
                default:                        sc = '?';   break;
            }
            switch (SymbolType) {
                case Type.NonUniqueLocalAddr:   tc = 'N';   break;
                case Type.LocalOrGlobalAddr:    tc = 'L';   break;
                case Type.GlobalAddr:           tc = 'G';   break;
                case Type.GlobalAddrExport:     tc = 'X';   break;
                case Type.ExternalAddr:         tc = 'E';   break;
                case Type.Constant:             tc = 'C';   break;
                default:                        tc = '?';   break;
            }
            SourceTypeString = "" + sc + tc;

            // Generate AnnotatedLabel.
            AnnotatedLabel = AppendAnnotation(Label, LabelAnno);
        }

        /// <summary>
        /// Constructor for non-unique labels.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="value"></param>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="labelAnno"></param>
        /// <param name="offset"></param>
        public Symbol(string label, int value, Source source, Type type,
                LabelAnnotation labelAnno, int offset)
                : this(label, value, source, type, labelAnno) {
            Debug.Assert(false);        // TODO(xyzzy)
        }

        /// <summary>
        /// Performs a detailed validation of a symbol label, breaking out different failure
        /// causes for the benefit of code that reports errors to the user.  The label may
        /// have additional characters, such as annotations, which are trimmed away.  The
        /// trimmed version of the string is returned.
        /// </summary>
        /// <param name="label">Label to examine.</param>
        /// <param name="isValid">True if the entire label is valid.</param>
        /// <param name="isLenValid">True if the label has a valid length.</param>
        /// <param name="isFirstCharValid">True if the first character is valid.</param>
        /// <param name="anno">Annotation found, or None if none found.</param>
        /// <returns>Trimmed version of the string.</returns>
        public static string TrimAndValidateLabel(string label, out bool isValid,
                out bool isLenValid, out bool isFirstCharValid, out LabelAnnotation anno) {
            anno = LabelAnnotation.None;

            // Do we have at least one char?
            if (string.IsNullOrEmpty(label)) {
                isValid = isLenValid = isFirstCharValid = false;
                return label;
            }

            string trimLabel = label;
            // Check for an annotation char, remove it if found.
            if (trimLabel[trimLabel.Length - 1] == UNCERTAIN_CHAR) {
                anno = LabelAnnotation.Uncertain;
                trimLabel = trimLabel.Substring(0, trimLabel.Length - 1);
            }

            // Now that we're down to the base string, do the full validation test.  If it
            // passes, we don't need to dig any deeper.
            isValid = Asm65.Label.ValidateLabelDetail(trimLabel, out isLenValid,
                out isFirstCharValid);

            return trimLabel;
        }

        /// <summary>
        /// Augments a label string with an annotation identifier.
        /// </summary>
        /// <param name="label">String to augment.</param>
        /// <param name="anno">Annotation; may be None.</param>
        /// <returns>Original or updated string.</returns>
        public static string AppendAnnotation(string label, LabelAnnotation anno) {
            if (anno == LabelAnnotation.Uncertain) {
                return label + UNCERTAIN_CHAR;
            //} else if (anno == LabelAnnotation.Generated) {
            //    return label + '\u00a4';  // CURRENCY_SIGN '¤'
            } else {
                return label;
            }
        }


        public override string ToString() {
            return Label + "{" + SymbolSource  + "," + SymbolType +
                ",val=$" + Value.ToString("x4") + "," + LabelAnno + "}";
        }

        public static bool operator ==(Symbol a, Symbol b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            // All fields must be equal.  Ignore SourceTypeString and AnnotatedLabel, since
            // they're generated from other fields.
            return Asm65.Label.LABEL_COMPARER.Equals(a.Label, b.Label) && a.Value == b.Value &&
                a.SymbolSource == b.SymbolSource && a.SymbolType == b.SymbolType &&
                a.LabelAnno == b.LabelAnno;
        }
        public static bool operator !=(Symbol a, Symbol b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is Symbol && this == (Symbol)obj;
        }
        public override int GetHashCode() {
            // Convert label to "normal form" if we're doing case-insensitive.  (We're not
            // anymore, so it's a no-op now.)
            return Asm65.Label.ToNormal(Label).GetHashCode() ^
                Value ^ (int)SymbolType ^ (int)SymbolSource ^ (int)LabelAnno;
        }


        //
        // Comparison function, used when sorting the symbol table.
        //

        public enum SymbolSortField { CombinedType, Value, Name };

        public static int Compare(SymbolSortField sortField, bool isAscending,
                Symbol a, Symbol b) {
            // In the symbol table, only the label field is guaranteed to be unique.  We
            // use it as a secondary sort key when comparing the other fields.
            switch (sortField) {
                case SymbolSortField.CombinedType:
                    if (isAscending) {
                        int cmp = string.Compare(a.SourceTypeString, b.SourceTypeString);
                        if (cmp == 0) {
                            int aDir = 0;
                            int bDir = 0;
                            if (a is DefSymbol) {
                                aDir = (int)((DefSymbol)a).Direction;
                            }
                            if (b is DefSymbol) {
                                bDir = (int)((DefSymbol)b).Direction;
                            }
                            cmp = aDir - bDir;
                        }
                        if (cmp == 0) {
                            cmp = string.Compare(a.Label, b.Label);
                        }
                        return cmp;
                    } else {
                        int cmp = string.Compare(a.SourceTypeString, b.SourceTypeString);
                        if (cmp == 0) {
                            // secondary sort is always ascending, so negate
                            cmp = -string.Compare(a.Label, b.Label);
                        }
                        return -cmp;
                    }
                case SymbolSortField.Value:
                    if (isAscending) {
                        int cmp;
                        if (a.Value < b.Value) {
                            cmp = -1;
                        } else if (a.Value > b.Value) {
                            cmp = 1;
                        } else {
                            cmp = string.Compare(a.Label, b.Label);
                        }
                        return cmp;
                    } else {
                        int cmp;
                        if (a.Value < b.Value) {
                            cmp = -1;
                        } else if (a.Value > b.Value) {
                            cmp = 1;
                        } else {
                            cmp = -string.Compare(a.Label, b.Label);
                        }
                        return -cmp;
                    }
                case SymbolSortField.Name:
                    if (isAscending) {
                        return string.Compare(a.Label, b.Label);
                    } else {
                        return -string.Compare(a.Label, b.Label);
                    }
                default:
                    Debug.Assert(false);
                    return 0;
            }
        }
    }
}
