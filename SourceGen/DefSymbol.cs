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
    /// Subclass of Symbol used for symbols defined in a platform symbol file, in the project
    /// symbol table, or in a local variable table.
    ///
    /// Instances are immutable, except for the Xrefs field.
    /// </summary>
    /// <remarks>
    /// The Xrefs field isn't really part of the object.  It's just convenient to access
    /// them from here.
    /// </remarks>
    public class DefSymbol : Symbol {
        // Absolute min/max width.  Zero-page variables are more limited, because they're not
        // allowed to wrap around the end of the page.
        public const int MIN_WIDTH = 1;
        public const int MAX_WIDTH = 65536;

        // Value to pass to the FormatDescriptor when no width is given.
        private const int DEFAULT_WIDTH = 1;

        /// <summary>
        /// Data format descriptor.
        /// </summary>
        public FormatDescriptor DataDescriptor { get; private set; }

        /// <summary>
        /// True if a width was specified for this symbol.
        /// </summary>
        /// <remarks>
        /// All symbols have a positive width, stored in the FormatDescriptor Length property.
        /// We may not want to display widths that haven't been explicitly set, however, so we
        /// keep track here.
        /// </remarks>
        public bool HasWidth { get; private set; }

        /// <summary>
        /// User-supplied comment.
        /// </summary>
        public string Comment { get; private set; }

        /// <summary>
        /// Platform symbols only: tag used to organize symbols into groups.  Used by
        /// extension scripts.
        ///
        /// Not serialized.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// Platform symbols only: this indicates the position of the defining platform symbol
        /// file in the set of symbol files.  Higher numbers mean higher priority.
        ///
        /// Not serialized.
        /// </summary>
        public int LoadOrdinal { get; private set; }

        /// <summary>
        /// Platform symbols only: external file identifier for the platform symbol file that
        /// defined this symbol.  Can be displayed to the user in the Info panel.
        ///
        /// Not serialized.
        /// </summary>
        public string FileIdentifier { get; private set; }

        /// <summary>
        /// I/O direction enumeration.
        /// </summary>
        /// <remarks>
        /// The numeric value determines the sort order in the Symbols window.  See the Compare
        /// function over in Symbol.
        /// </remarks>
        [Flags]
        public enum DirectionFlags {
            None        = 0,
            Read        = 1 << 1,
            Write       = 1 << 2,
            ReadWrite   = Read | Write
        }

        /// <summary>
        /// I/O direction, used for memory-mapped I/O locations that have different meanings
        /// (and hence different symbols) depending on whether they're read or written.
        /// </summary>
        public DirectionFlags Direction { get; private set; }

        /// <summary>
        /// Bit masks for symbols that represent multiple addresses.  Instances are immutable.
        /// </summary>
        /// <remarks>
        /// Given an integer "addr" to test:
        /// <code>
        ///   if ((addr &amp; CompareMask) == CompareValue &amp;&amp;
        ///           (addr &amp; AddressMask) == (Value &amp; AddressMask)) {
        ///       // match!
        ///   }
        /// </code>
        /// </remarks>
        public class MultiAddressMask {
            public int CompareMask { get; private set; }
            public int CompareValue { get; private set; }
            public int AddressMask { get; private set; }

            public MultiAddressMask(int cmpMask, int cmpValue, int addrMask) {
                CompareMask = cmpMask;
                CompareValue = cmpValue;
                AddressMask = addrMask;
            }
            public override string ToString() {
                return "MultiAddrMask: cmpMask=$" + CompareMask.ToString("x4") +
                    " cmpValue=$" + CompareValue.ToString("x4") +
                    " addrMask=$" + AddressMask.ToString("x4");
            }
            public static bool operator ==(MultiAddressMask a, MultiAddressMask b) {
                if (ReferenceEquals(a, b)) {
                    return true;        // same object, or both null
                }
                if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                    return false;       // one is null
                }
                return a.CompareMask == b.CompareMask && a.CompareValue == b.CompareValue &&
                    a.AddressMask == b.AddressMask;
            }
            public static bool operator !=(MultiAddressMask a, MultiAddressMask b) {
                return !(a == b);
            }
            public override bool Equals(object obj) {
                return obj is MultiAddressMask && this == (MultiAddressMask)obj;
            }
            public override int GetHashCode() {
                return CompareMask ^ CompareValue ^ AddressMask;
            }
        }

        /// <summary>
        /// Bit masks to apply when performing comparisons.  Useful when more than one address
        /// maps to the same thing (e.g. Atari 2600 registers).
        ///
        /// Will be null if no mask is specified.
        /// </summary>
        public MultiAddressMask MultiMask { get; private set; }

        /// <summary>
        /// Cross-reference data, generated by the analyzer.
        /// </summary>
        /// <remarks>
        /// This is just a convenient place to reference some data generated at run-time.  It's
        /// not serialized, and not included in the test for equality.
        /// </remarks>
        public XrefSet Xrefs { get; private set; }


        /// <summary>
        /// Internal base-object (Symbol) constructor, called by other constructors.
        /// </summary>
        private DefSymbol(string label, int value, Source source, Type type,
                LabelAnnotation labelAnno)
                : base(label, value, source, type, labelAnno) {
            Debug.Assert(source == Source.Platform || source == Source.Project ||
                source == Source.Variable);
            Debug.Assert(type == Type.ExternalAddr || type == Type.Constant);
            Xrefs = new XrefSet();
        }

        /// <summary>
        /// Constructor.  Limited form, used in a couple of places, e.g. when we need to start
        /// with a default value.  The symbol will have unspecified width, ReadWrite direction,
        /// and no mask.
        /// </summary>
        /// <param name="label">Symbol's label.</param>
        /// <param name="value">Symbol's value.</param>
        /// <param name="source">Symbol source (general point of origin).</param>
        /// <param name="type">Symbol type.</param>
        /// <param name="formatSubType">Format descriptor sub-type, so we know how the
        ///   user wants the value to be displayed.</param>
        public DefSymbol(string label, int value, Source source, Type type,
                FormatDescriptor.SubType formatSubType)
                : this(label, value, source, type, LabelAnnotation.None, formatSubType, -1, false,
                       string.Empty, DirectionFlags.ReadWrite, null, string.Empty) { }

        /// <summary>
        /// Constructor.  General form.
        /// </summary>
        /// <param name="label">Symbol's label.</param>
        /// <param name="value">Symbol's value.</param>
        /// <param name="source">Symbol source (general point of origin).</param>
        /// <param name="type">Symbol type.</param>
        /// <param name="formatSubType">Format descriptor sub-type, so we know how the
        ///   user wants the value to be displayed.</param>
        /// <param name="width">Variable width.</param>
        /// <param name="widthSpecified">True if width was explicitly specified.  If this is
        /// <param name="comment">End-of-line comment.</param>
        /// <param name="direction">I/O direction.</param>
        /// <param name="multiMask">Bit mask to apply before comparisons.</param>
        /// <param name="tag">Symbol tag, used for grouping platform symbols.</param>
        ///   false, the value of the "width" argument is ignored.</param>
        public DefSymbol(string label, int value, Source source, Type type,
                LabelAnnotation labelAnno, FormatDescriptor.SubType formatSubType,
                int width, bool widthSpecified, string comment,
                DirectionFlags direction, MultiAddressMask multiMask, string tag)
                : this(label, value, source, type, labelAnno) {
            Debug.Assert(comment != null);
            Debug.Assert(tag != null);

            if (widthSpecified && type == Type.Constant && source != Source.Variable) {
                // non-variable constants don't have a width; override arg
                Debug.WriteLine("Overriding constant DefSymbol width");
                widthSpecified = false;
            }
            HasWidth = widthSpecified;
            if (!widthSpecified) {
                width = DEFAULT_WIDTH;
            }
            Debug.Assert(width >= MIN_WIDTH && width <= MAX_WIDTH);

            DataDescriptor = FormatDescriptor.Create(width,
                FormatDescriptor.Type.NumericLE, formatSubType);
            Comment = comment;

            Debug.Assert(((int)direction & ~(int)DirectionFlags.ReadWrite) == 0);
            Direction = direction;

            // constants don't have masks
            if (type != Type.Constant) {
                MultiMask = multiMask;
            }

            Tag = tag;
        }

        /// <summary>
        /// Constructor.  Used for platform symbol files.
        /// </summary>
        /// <param name="loadOrdinal">Indicates the order in which the defining platform
        ///   symbol file was loaded.  Higher numbers indicate later loading, which translates
        ///   to higher priority.</param>
        /// <param name="fileIdent">Platform symbol file identifier, for the Info panel.</param>
        public DefSymbol(string label, int value, Source source, Type type,
                FormatDescriptor.SubType formatSubType, int width, bool widthSpecified,
                string comment, DirectionFlags direction, MultiAddressMask multiMask, string tag,
                int loadOrdinal, string fileIdent)
                : this(label, value, source, type, LabelAnnotation.None, formatSubType,
                      width, widthSpecified, comment, direction, multiMask, tag) {
            LoadOrdinal = loadOrdinal;
            FileIdentifier = fileIdent;
        }

        /// <summary>
        /// Create a DefSymbol given a Symbol, FormatDescriptor, and a few other things.  Used
        /// for deserialization.
        /// </summary>
        /// <param name="sym">Base symbol.</param>
        /// <param name="dfd">Format descriptor.</param>
        /// <param name="widthSpecified">Set if a width was explicitly specified.</param>
        /// <param name="comment">End-of-line comment.</param>
        /// <param name="direction">I/O direction.</param>
        /// <param name="multiMask">Bit mask to apply before comparisons.</param>
        public static DefSymbol Create(Symbol sym, FormatDescriptor dfd, bool widthSpecified,
                string comment, DirectionFlags direction, MultiAddressMask multiMask) {
            int width = dfd.Length;
            if (widthSpecified && sym.SymbolType == Type.Constant &&
                    sym.SymbolSource != Source.Variable) {
                // non-variable constants don't have a width; override arg
                Debug.WriteLine("Overriding constant DefSymbol width");
                widthSpecified = false;
            }
            Debug.Assert(dfd.FormatType == FormatDescriptor.Type.NumericLE);
            return new DefSymbol(sym.Label, sym.Value, sym.SymbolSource, sym.SymbolType,
                sym.LabelAnno, dfd.FormatSubType, width, widthSpecified,
                comment, direction, multiMask, string.Empty);
        }

        /// <summary>
        /// Constructs a DefSymbol from an existing DefSymbol, with a different label.  Use
        /// this to change the label while keeping everything else the same.
        /// </summary>
        /// <remarks>
        /// This can't be a simple Rename() function that uses a copy constructor because
        /// the label is in the base class.
        ///
        /// The Xrefs reference points to the actual XrefSet in the original.  This is not
        /// ideal, but it's the easiest way to keep xrefs working across Lv de-duplication
        /// (you actually *want* xrefs added to copies to be held by the original).
        /// </remarks>
        /// <param name="defSym">Source DefSymbol.</param>
        /// <param name="label">Label to use.</param>
        public DefSymbol(DefSymbol defSym, string label)
            : this(label, defSym.Value, defSym.SymbolSource, defSym.SymbolType,
                  defSym.LabelAnno, defSym.DataDescriptor.FormatSubType,
                  defSym.DataDescriptor.Length, defSym.HasWidth, defSym.Comment,
                  defSym.Direction, defSym.MultiMask, defSym.Tag)
        {
            Debug.Assert(SymbolSource == Source.Variable);
            Xrefs = defSym.Xrefs;
        }

        /// <summary>
        /// Determines whether a symbol overlaps with a region.  Useful for variables.
        /// </summary>
        /// <param name="a">Symbol to check.</param>
        /// <param name="value">Address.</param>
        /// <param name="width">Symbol width.</param>
        /// <param name="type">Symbol type to check against.</param>
        /// <returns>True if the symbols overlap.</returns>
        public static bool CheckOverlap(DefSymbol a, int value, int width, Type type) {
            if (a.DataDescriptor.Length <= 0 || width <= 0) {
                return false;
            }
            if (a.Value < 0 || value < 0) {
                return false;
            }
            if (a.SymbolType != type) {
                return false;
            }
            int maxStart = Math.Max(a.Value, value);
            int minEnd = Math.Min(a.Value + a.DataDescriptor.Length - 1, value + width - 1);
            return (maxStart <= minEnd);
        }


        public static bool operator ==(DefSymbol a, DefSymbol b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            return a.Equals(b);
        }
        public static bool operator !=(DefSymbol a, DefSymbol b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            if (!(obj is DefSymbol)) {
                return false;
            }
            // Do base-class equality comparison and the ReferenceEquals check.
            if (!base.Equals(obj)) {
                return false;
            }

            // All fields must be equal, except Xrefs.
            DefSymbol other = (DefSymbol)obj;
            if (DataDescriptor != other.DataDescriptor ||
                    Comment != other.Comment ||
                    Tag != other.Tag) {
                return false;
            }
            return true;
        }
        public override int GetHashCode() {
            return base.GetHashCode() ^
                DataDescriptor.GetHashCode() ^
                Comment.GetHashCode() ^
                Tag.GetHashCode();
        }

        public override string ToString() {
            return base.ToString() + ":" + DataDescriptor + ";" + Comment +
                " dir=" + Direction + " mask=" + (MultiMask == null ? "-" : MultiMask.ToString()) +
                (string.IsNullOrEmpty(Tag) ? "" : " [" + Tag + "]");
        }
    }
}
