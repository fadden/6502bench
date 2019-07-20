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

using Asm65;
using CommonUtil;

/*
*** When is a full (code+data) re-analysis required?
- Adding/removing/changing an address change (ORG directive).  This has a significant impact
on the code analyzer, as blocks of code may become reachable or unreachable.
- Adding/removing/changing a type hint.  These can affect whether a given offset is treated
as code, which can have a dramatic effect on code analysis (consider the offset 0 code hint).
- Adding/removing/changing a status flag override.  This can affect whether a branch is
always taken or never taken, and the M/X flags affect instruction interpretation.  (It may
be possible to do an "incremental" code analysis here, working from the point of the change
forward, propagating changes outward, but that gets tricky when a branch changes from
ambiguously-taken to never-taken, and the destination may need to be treated as data.)

*** When is a partial (data-only) re-analysis required?
- Adding/removing a user label.  The code that tries to adjust data targets to match nearby
user labels must be re-run, possibly impacting auto-generated labels.  A user label added
to the middle of a multi-byte data element will cause the element to be split, requiring
reanalysis of the pieces.
- Adding/removing/changing an operand label, e.g "LDA label".  This can affect which
offsets are marked as data targets, which affects the data analyzer.  (We could be smart
about this and not invoke reanalysis if the label value matches the operand, but address
operands should already have labels via offset reference, so it's unclear how valuable
this would be.)
- Adding/removing/changing a format descriptor with a symbol or Numeric/Address.  This
can affect the data target analysis.

*** When is a partial (late-data) re-analysis required?
- Adding/removing/changing the length of a formatted data item, when that item isn't subject
to conditions above (e.g. the descriptor doesn't specify a symbol).  This affects which bytes
are considered "uncategorized", so the uncategorized-data analysis must be repeated.

*** When is display-only re-analysis needed?
- When altering the way that data is formatted, it's useful to exercise the same code paths,
up to the point where the analyzer is called.  We still want to go through all the steps that
update the display list and cause controls to be redrawn, but we don't want to actually change
anything in the DisasmProject.  "Misc" means we do nothing but pretend there was a full update.

*** When can we get away with only updating part of the display list (re-analysis=none)?
- Changing a user label.  All lines that reference the label need to be updated in the
display, but nothing in the analysis changes.  (This assumes we prevent you from renaming
a label to be the same as an existing label, e.g. auto-generated labels.)
- Adding/removing/changing cosmetic items, like comments and notes.

NOTE: all re-analysis requirements are symmetric for undo/redo.  Undoing a change requires
the same level of work as doing the change.
*/

namespace SourceGen {
    /// <summary>
    /// A single change.
    /// </summary>
    public class UndoableChange {
        public enum ChangeType {
            Unknown = 0,

            // Dummy change, used to force a full update.
            Dummy,

            // Adds, updates, or removes an AddressMap entry.
            SetAddress,

            // Changes the type hint.
            SetTypeHint,

            // Adds, updates, or removes a processor status flag override.
            SetStatusFlagOverride,

            // Adds, updates, or removes a user-specified label.
            SetLabel,

            // Adds, updates, or removes a data or operand format.
            SetOperandFormat,

            // Changes the end-of-line comment.
            SetComment,

            // Changes the long comment.
            SetLongComment,

            // Changes the note.
            SetNote,

            // Updates project properties.
            SetProjectProperties
        }

        /// <summary>
        /// Enum indicating what needs to be reanalyzed after a change.
        /// </summary>
        public enum ReanalysisScope {
            None = 0,
            DisplayOnly,
            DataOnly,
            CodeAndData
        }

        /// <summary>
        /// Identifies the change type.
        /// </summary>
        public ChangeType Type { get; private set; }

        /// <summary>
        /// The "root offset".  For example, changing the type hint for a 4-byte
        /// instruction from code to data will actually affect 4 offsets, but we
        /// only need to specify the root item.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Value we're changing to. 
        /// </summary>
        public object NewValue { get; private set; }

        /// <summary>
        /// Previous value, used for "undo".
        /// </summary>
        public object OldValue { get; private set; }

        /// <summary>
        /// Indicates what amount of reanalysis is required after the change is implemented.
        /// </summary>
        public ReanalysisScope ReanalysisRequired { get; private set; }


        // Don't instantiate directly.
        private UndoableChange() { }

        public bool HasOffset {
            get {
                switch (Type) {
                    case ChangeType.Dummy:
                    case ChangeType.SetTypeHint:
                    case ChangeType.SetProjectProperties:
                        return false;
                    default:
                        return true;
                }
            }
        }

        /// <summary>
        /// Creates an UndoableChange that does nothing but force an update.
        /// </summary>
        /// <param name="flags">Desired reanalysis flags.</param>
        /// <returns>Change record.</returns>
        public static UndoableChange CreateDummyChange(ReanalysisScope flags) {
            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.Dummy;
            uc.Offset = -1;
            uc.ReanalysisRequired = flags;
            return uc;
        }

        /// <summary>
        /// Creates an UndoableChange for an address map update.
        /// </summary>
        /// <param name="offset">Affected offset.</param>
        /// <param name="oldAddress">Previous address map entry, or -1 if none.</param>
        /// <param name="newAddress">New address map entry, or -1 if none.</param>
        /// <returns>Change record.</returns>
        public static UndoableChange CreateAddressChange(int offset, int oldAddress,
                int newAddress) {
            if (oldAddress == newAddress) {
                Debug.WriteLine("No-op address change at +" + offset.ToString("x6") +
                    ": " + oldAddress);
            }
            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.SetAddress;
            uc.Offset = offset;
            uc.OldValue = oldAddress;
            uc.NewValue = newAddress;
            uc.ReanalysisRequired = ReanalysisScope.CodeAndData;
            return uc;
        }

        /// <summary>
        /// Creates an UndoableChange for a type hint update.  Rather than adding a
        /// separate UndoableChange for each affected offset -- which could span the
        /// entire file -- we use range sets to record the before/after state.
        /// </summary>
        /// <param name="undoSet">Current values.</param>
        /// <param name="newSet">New values.</param>
        /// <returns>Change record.</returns>
        public static UndoableChange CreateTypeHintChange(TypedRangeSet undoSet,
                TypedRangeSet newSet) {
            if (newSet.Count == 0) {
                Debug.WriteLine("Empty hint change?");
            }
            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.SetTypeHint;
            uc.Offset = -1;
            uc.OldValue = undoSet;
            uc.NewValue = newSet;
            // Any hint change can affect whether something is treated as code.
            // Either we're deliberately setting it as code or non-code, or we're
            // setting it to "no hint", which means the code analyzer gets
            // to make the decision now.  This requires a full code+data re-analysis.
            uc.ReanalysisRequired = ReanalysisScope.CodeAndData;
            return uc;
        }

        /// <summary>
        /// Creates an UndoableChange for a status flag override update.
        /// </summary>
        /// <param name="offset">Affected offset.</param>
        /// <param name="oldFlags">Current flags.</param>
        /// <param name="newFlags">New flags.</param>
        /// <returns></returns>
        public static UndoableChange CreateStatusFlagChange(int offset, StatusFlags oldFlags,
                StatusFlags newFlags) {
            if (oldFlags == newFlags) {
                Debug.WriteLine("No-op status flag change at " + offset);
            }
            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.SetStatusFlagOverride;
            uc.Offset = offset;
            uc.OldValue = oldFlags;
            uc.NewValue = newFlags;
            // This can affect instruction widths (for M/X) and conditional branches.  We
            // don't need to re-analyze for changes to I/D, but users don't really need to
            // change those anyway, so it's not worth optimizing.
            uc.ReanalysisRequired = ReanalysisScope.CodeAndData;
            return uc;
        }

        /// <summary>
        /// Creates an UndoableChange for a label update.
        /// </summary>
        /// <param name="offset">Affected offset.</param>
        /// <param name="oldSymbol">Current label.  May be null.</param>
        /// <param name="newSymbol">New label.  May be null.</param>
        /// <returns>Change record.</returns>
        public static UndoableChange CreateLabelChange(int offset, Symbol oldSymbol,
                Symbol newSymbol) {
            if (oldSymbol == newSymbol) {
                Debug.WriteLine("No-op label change at +" + offset.ToString("x6") +
                    ": " + oldSymbol);
            }

            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.SetLabel;
            uc.Offset = offset;
            uc.OldValue = oldSymbol;
            uc.NewValue = newSymbol;
            // Data analysis can change if we add or remove a label in a data area.  Label
            // selection can change as well, e.g. switching from an auto-label to a user
            // label with an adjustment.  So renaming a user-defined label doesn't require
            // reanalysis, but adding or removing one does.
            //
            // Do the reanalysis if either is empty.  This will cause an unnecessary
            // reanalysis if we change an empty label to an empty label, but that shouldn't
            // be allowed by the UI anyway.
            Debug.Assert(newSymbol == null || newSymbol.SymbolSource == Symbol.Source.User);
            if ((oldSymbol == null) || (newSymbol == null) /*||
                    (oldSymbol.SymbolSource != newSymbol.SymbolSource)*/) {
                uc.ReanalysisRequired = ReanalysisScope.DataOnly;
            } else {
                uc.ReanalysisRequired = ReanalysisScope.None;
            }
            return uc;
        }

        /// <summary>
        /// Creates an UndoableChange for an operand or data format update.  This method
        /// refuses to create a change for a no-op, returning null instead.  This will
        /// convert a FormatDescriptor with type REMOVE to null, with the intention of
        /// removing the descriptor from the format set.
        /// </summary>
        /// <param name="offset">Affected offset.</param>
        /// <param name="oldFormat">Current format.  May be null.</param>
        /// <param name="newFormat">New format.  May be null.</param>
        /// <returns>Change record, or null for a no-op change.</returns>
        public static UndoableChange CreateActualOperandFormatChange(int offset,
                FormatDescriptor oldFormat, FormatDescriptor newFormat) {
            if (newFormat != null && newFormat.FormatType == FormatDescriptor.Type.REMOVE) {
                Debug.WriteLine("CreateOperandFormatChange: converting REMOVE to null");
                newFormat = null;
            }
            if (oldFormat == newFormat) {
                Debug.WriteLine("No-op format change at +" + offset.ToString("x6") +
                    ": " + oldFormat);
                return null;
            }

            return CreateOperandFormatChange(offset, oldFormat, newFormat);
        }

        /// <summary>
        /// Creates an UndoableChange for an operand or data format update.
        /// </summary>
        /// <param name="offset">Affected offset.</param>
        /// <param name="oldFormat">Current format.  May be null.</param>
        /// <param name="newFormat">New format.  May be null.</param>
        /// <returns>Change record.</returns>
        public static UndoableChange CreateOperandFormatChange(int offset,
                FormatDescriptor oldFormat, FormatDescriptor newFormat) {
            if (oldFormat == newFormat) {
                Debug.WriteLine("No-op format change at +" + offset.ToString("x6") +
                    ": " + oldFormat);
            }

            // We currently allow old/new formats with different lengths.  There doesn't
            // seem to be a reason not to, and a slight performance advantage to doing so.
            // Also, if a change set has two changes at the same offset, undo requires
            // enumerating the list in reverse order.

            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.SetOperandFormat;
            uc.Offset = offset;
            uc.OldValue = oldFormat;
            uc.NewValue = newFormat;

            // Data-only reanalysis is required if the old or new format has a label.  Simply
            // changing from e.g. default to decimal, or decimal to binary, doesn't matter.
            // (The format editing code ensures that labels don't appear in the middle of
            // a formatted region.)  Adding, removing, or changing a symbol can change the
            // layout of uncategorized data, affect data targets, xrefs, etc.
            //
            // We can't only check for a symbol, though, because Numeric/Address will
            // create an auto-label if the reference is within the file.
            //
            // If the number of bytes covered by the format changes, or we're adding or
            // removing a format, we need to redo the analysis of uncategorized data.  For
            // example, an auto-detected string could get larger or smaller.  We don't
            // currently have a separate flag for just that.  Also, because we're focused
            // on just one change, we can't skip reanalysis when (say) one 4-byte numeric
            // is converted to two two-byte numerics.
            if ((oldFormat != null && oldFormat.HasSymbolOrAddress) ||
                    (newFormat != null && newFormat.HasSymbolOrAddress)) {
                uc.ReanalysisRequired = ReanalysisScope.DataOnly;
            } else if (oldFormat == null || newFormat == null ||
                    oldFormat.Length != newFormat.Length) {
                uc.ReanalysisRequired = ReanalysisScope.DataOnly;
            } else {
                uc.ReanalysisRequired = ReanalysisScope.None;
            }
            return uc;
        }

        /// <summary>
        /// Creates an UndoableChange for a comment update.
        /// </summary>
        /// <param name="offset">Affected offset.</param>
        /// <param name="oldComment">Current comment.</param>
        /// <param name="newComment">New comment.</param>
        /// <returns>Change record.</returns>
        public static UndoableChange CreateCommentChange(int offset, string oldComment,
                string newComment) {
            if (oldComment.Equals(newComment)) {
                Debug.WriteLine("No-op comment change at +" + offset.ToString("x6") +
                    ": " + oldComment);
            }

            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.SetComment;
            uc.Offset = offset;
            uc.OldValue = oldComment;
            uc.NewValue = newComment;
            uc.ReanalysisRequired = ReanalysisScope.None;
            return uc;
        }

        /// <summary>
        /// Creates an UndoableChange for a long comment update.
        /// </summary>
        /// <param name="offset">Affected offset.</param>
        /// <param name="oldComment">Current comment.</param>
        /// <param name="newComment">New comment.</param>
        /// <returns>Change record.</returns>
        public static UndoableChange CreateLongCommentChange(int offset,
                MultiLineComment oldComment, MultiLineComment newComment) {
            if (oldComment == newComment) {
                Debug.WriteLine("No-op long comment change at +" + offset.ToString("x6") +
                    ": " + oldComment);
            }

            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.SetLongComment;
            uc.Offset = offset;
            uc.OldValue = oldComment;
            uc.NewValue = newComment;
            uc.ReanalysisRequired = ReanalysisScope.None;
            return uc;
        }

        /// <summary>
        /// Creates an UndoableChange for a note update.
        /// </summary>
        /// <param name="offset">Affected offset.</param>
        /// <param name="oldNote">Current note.</param>
        /// <param name="newNote">New note.</param>
        /// <returns>Change record.</returns>
        public static UndoableChange CreateNoteChange(int offset,
                MultiLineComment oldNote, MultiLineComment newNote) {
            if (oldNote == newNote) {
                Debug.WriteLine("No-op note change at +" + offset.ToString("x6") +
                    ": " + oldNote);
            }

            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.SetNote;
            uc.Offset = offset;
            uc.OldValue = oldNote;
            uc.NewValue = newNote;
            uc.ReanalysisRequired = ReanalysisScope.None;
            return uc;
        }

        /// <summary>
        /// Creates an UndoableChange for a change to the project properties.
        /// </summary>
        /// <param name="oldNote">Current note.</param>
        /// <param name="newNote">New note.</param>
        /// <returns>Change record.</returns>
        public static UndoableChange CreateProjectPropertiesChange(ProjectProperties oldProps,
                ProjectProperties newProps) {
            Debug.Assert(oldProps != null && newProps != null);
            if (oldProps == newProps) { // doesn't currently work except as reference check
                Debug.WriteLine("No-op property change: " + oldProps);
            }

            UndoableChange uc = new UndoableChange();
            uc.Type = ChangeType.SetProjectProperties;
            uc.Offset = -1;
            uc.OldValue = oldProps;
            uc.NewValue = newProps;

            // Project properties could change the CPU type, requiring a full code+data
            // reanalysis.  We could scan the objects to see what actually changed, but that
            // doesn't seem worthwhile.
            uc.ReanalysisRequired = ReanalysisScope.CodeAndData;
            return uc;
        }

        public override string ToString() {
            return "[UC type=" + Type + " offset=+" +
                (HasOffset ? Offset.ToString("x6") : "N/A") + "]";
        }
    }
}
