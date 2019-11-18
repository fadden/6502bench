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

namespace SourceGen {
    /// <summary>
    /// Symbolic representation of a value.  Instances are immutable.
    /// </summary>
    public class Symbol {
        public const char UNCERTAIN_CHAR = '?';
        private const char NO_ANNO_CHAR = '\ufffd';     // REPLACEMENT CHARACTER '�'
        private const char UNIQUE_TAG_CHAR = '\u00a7';  // SECTION SIGN
        private const int NON_UNIQUE_LEN = 7;           // NON_UNIQUE_CHAR + 6 hex digits

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
        /// <remarks>
        /// There's really just three types: unique address symbol, non-unique address symbol,
        /// and constant.  There's also a set of boolean flags indicating whether the symbol
        /// should be forced to be global, whether it should be included in the export table,
        /// and whether it's internal or external.
        ///
        /// It turns out that many combinations of type and flag don't actually make sense,
        /// e.g. I don't know what a non-unique exported external constant is, so we just
        /// enumerate the combinations that make sense.
        /// </remarks>
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
        /// Label without the non-unique tag.  Used for serialization.
        /// </summary>
        public string LabelWithoutTag {
            get {
                if (SymbolType != Type.NonUniqueLocalAddr) {
                    return Label;
                } else {
                    return Label.Substring(0, Label.Length - NON_UNIQUE_LEN);
                }
            }
        }

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
        /// True if the symbol is a non-unique local.
        /// </summary>
        public bool IsNonUnique {
            get { return SymbolType == Type.NonUniqueLocalAddr; }
        }

        public bool CanBeLocal {
            get { return SymbolType == Type.LocalOrGlobalAddr ||
                         SymbolType == Type.NonUniqueLocalAddr; }
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
        /// <param name="labelAnno">Optional annotation.</param>
        public Symbol(string label, int value, Source source, Type type,
                LabelAnnotation labelAnno) {
            Debug.Assert(Asm65.Label.ValidateLabel(label));
            Debug.Assert(type != Type.NonUniqueLocalAddr || value == 0xdead); // use other ctor
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
        }

        /// <summary>
        /// Constructor for non-unique labels.
        /// </summary>
        /// <param name="label">Label string.  Syntax assumed valid.</param>
        /// <param name="value">Symbol value.</param>
        /// <param name="labelAnno">Optional annotation.</param>
        /// <param name="uniqueTag">Tag that makes a non-unique label unique, e.g. the
        ///   offset for which a user label has been created.</param>
        public Symbol(string label, int value, LabelAnnotation labelAnno, int uniqueTag)
                : this(label, 0xdead, Source.User, Type.NonUniqueLocalAddr, labelAnno) {
            Debug.Assert(uniqueTag >= 0 && uniqueTag < 0x01000000); // fit in 6 hex digits
            Debug.Assert(label.IndexOf(UNIQUE_TAG_CHAR) < 0);       // already extended?

            Value = value;      // passed a bogus value to base ctor for assert

            // Add tag to label to make it unique.
            Label = label + UNIQUE_TAG_CHAR + uniqueTag.ToString("x6");
        }

        /// <summary>
        /// Generates a displayable form of the label.  This will have the non-unique label
        /// prefix and annotation suffix, and will have the non-unique tag removed.
        /// </summary>
        /// <param name="formatter">Formatter object.</param>
        /// <returns>Label suitable for display.</returns>
        public string GenerateDisplayLabel(Asm65.Formatter formatter) {
            return ConvertLabelForDisplay(Label, LabelAnno, true, formatter);
        }

        /// <summary>
        /// Returns the annotation suffix character, or NO_ANNO_CHAR if nothing appropriate.
        /// </summary>
        private static char GetLabelAnnoChar(LabelAnnotation anno) {
            char ch = NO_ANNO_CHAR;
            if (anno == LabelAnnotation.Uncertain) {
                ch = UNCERTAIN_CHAR;
            } else if (anno == LabelAnnotation.Generated) {
                //ch = '\u00a4';   // CURRENCY SIGN '¤'
            }
            return ch;
        }

        /// <summary>
        /// Converts a label to displayable form by stripping the uniquification tag (if any),
        /// inserting the non-unique label prefix if appropriate, and appending the optional
        /// annotation character.
        /// </summary>
        /// <remarks>
        /// There's generally two ways to display a label:
        ///  (1) When displaying labels on-screen, we get a label with the uniquification tag,
        ///      and we want to show the non-unique label prefix ('@' or ':') and annotation.
        ///  (2) When generating assembly source, we get a remapped label with no uniquification
        ///      tag, and we don't want to show the prefix or annotation.
        /// For case #2, there's no reason to call here.  (We're currently doing so because
        /// remapping isn't happening yet, but that should change soon.  When that happens, we
        /// should be able to eliminate the showNonUnique arg.)
        /// </remarks>
        /// <param name="label">Base label string.  Has the uniquification tag, but no
        ///   annotation char or non-unique prefix.</param>
        /// <param name="anno">Annotation; may be None.</param>
        /// <param name="showNonUnique">Set true if the returned label should show the
        ///   non-unique label prefix.</param>
        /// <param name="formatter">Format object that holds the non-unique label prefix
        ///   string.</param>
        /// <returns>Formatted label.</returns>
        public static string ConvertLabelForDisplay(string label, LabelAnnotation anno,
                bool showNonUnique, Asm65.Formatter formatter) {
            StringBuilder sb = new StringBuilder(label.Length + 2);

            if (label.Length > NON_UNIQUE_LEN &&
                    label[label.Length - NON_UNIQUE_LEN] == UNIQUE_TAG_CHAR) {
                // showNonUnique may be false if generating assembly code (but by this
                // point the unique tag should be remapped away)
                if (showNonUnique) {
                    sb.Append(formatter.NonUniqueLabelPrefix);
                }
                sb.Append(label.Substring(0, label.Length - NON_UNIQUE_LEN));
            } else {
                sb.Append(label);
            }

            char annoChar = GetLabelAnnoChar(anno);
            if (annoChar != NO_ANNO_CHAR) {
                sb.Append(annoChar);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Performs a detailed validation of a symbol label, breaking out different failure
        /// causes for the benefit of code that reports errors to the user.  The label may
        /// have additional characters, such as annotations, which are trimmed away.  The
        /// trimmed version of the string is returned.
        /// </summary>
        /// <param name="label">Label to examine.</param>
        /// <param name="nonUniquePrefix">For address symbols, the prefix string for
        ///   non-unique labels (e.g. '@' or ':').  May be null if not validating a user
        ///   label.</param>
        /// <param name="isValid">True if the entire label is valid.</param>
        /// <param name="isLenValid">True if the label has a valid length.</param>
        /// <param name="isFirstCharValid">True if the first character is valid.</param>
        /// <param name="hasNonUniquePrefix">True if the first character indicates that this is
        ///   a non-unique label.</param>
        /// <param name="anno">Annotation found, or None if none found.</param>
        /// <returns>Trimmed version of the string.</returns>
        public static string TrimAndValidateLabel(string label, string nonUniquePrefix,
                out bool isValid, out bool isLenValid, out bool isFirstCharValid,
                out bool hasNonUniquePrefix, out LabelAnnotation anno) {
            anno = LabelAnnotation.None;
            hasNonUniquePrefix = false;

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

            // Check for leading non-unique ident char.
            if (trimLabel.Length > 0 && !string.IsNullOrEmpty(nonUniquePrefix)) {
                if (trimLabel[0] == nonUniquePrefix[0]) {
                    hasNonUniquePrefix = true;
                    trimLabel = trimLabel.Substring(1);
                }
            }

            // Now that we're down to the base string, do the full validation test.  If it
            // passes, we don't need to dig any deeper.
            isValid = Asm65.Label.ValidateLabelDetail(trimLabel, out isLenValid,
                out isFirstCharValid);

            return trimLabel;
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
