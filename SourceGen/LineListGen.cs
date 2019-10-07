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

using Asm65;
using CommonUtil;
using FormattedParts = SourceGen.DisplayList.FormattedParts;

namespace SourceGen {
    /// <summary>
    /// Converts file data and Anattrib contents into a series of strings and format metadata.
    /// </summary>
    public class LineListGen {
        /// <summary>
        /// List of display lines.
        /// </summary>
        private List<Line> mLineList;

        /// <summary>
        /// List of formatted parts to be presented to the user.  This has one entry per line.
        /// </summary>
        /// <remarks>
        /// Separating FormattedParts out of Line seems odd at first, but we need changes to
        /// DisplayList to cause events in XAML.  I'm thinking the artificial separation of
        /// Line from the formatted data holder may make future ports easier.
        /// </remarks>
        private DisplayList mDisplayList;

        /// <summary>
        /// Project that contains the data we're formatting, notably the FileData and
        /// Anattribs arrays.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Code/data formatter.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// If set, prepend cycle counts to EOL comments.
        /// </summary>
        private bool mShowCycleCounts;

        /// <summary>
        /// Names for pseudo-ops.
        /// </summary>
        private PseudoOp.PseudoOpNames mPseudoOpNames;

        /// <summary>
        /// Cache of previously-formatted data.  The data is stored with references to
        /// dependencies, so it should not be necessary to explicitly clear this.
        /// </summary>
        private FormattedOperandCache mFormattedLineCache;

        /// <summary>
        /// Local variable table data extractor.
        /// </summary>
        private LocalVariableLookup mLvLookup;


        /// <summary>
        /// One of these per line of output in the display.  It should be possible to draw
        /// all of the output without needing to refer back to the project data.  (Currently
        /// making an exception for some selection-dependent field highlighting.)
        /// 
        /// Base fields are immutable, but the Parts property is set after creation.
        /// </summary>
        public class Line {
            // Extremely-negative offset value ensures it's at the very top.
            public const int HEADER_COMMENT_OFFSET = int.MinValue + 1;

            // These need to be bit flags so we can record which parts associated with a
            // given offset are selected.
            [FlagsAttribute]
            public enum Type {
                Unclassified            = 0,

                // Primary functional items.
                Code                    = 1 << 0,
                Data                    = 1 << 1,       // includes inline data
                CodeOrData              = (Code | Data),

                // Decorative items, added by user or formatter.
                LongComment             = 1 << 2,
                Note                    = 1 << 3,
                Blank                   = 1 << 4,

                // Assembler directives.
                OrgDirective            = 1 << 5,
                EquDirective            = 1 << 6,
                RegWidthDirective       = 1 << 7,

                // Additional metadata.
                LocalVariableTable      = 1 << 8,
            }

            /// <summary>
            /// Line type.
            /// </summary>
            public Type LineType { get; private set; }

            /// <summary>
            /// Numeric offset value.  Used to map a line item to the Anattrib.  Note this is
            /// set for all lines, and is the same for all lines in a multi-line sequence,
            /// e.g. every line in a long comment has the file offset with which it is associated.
            /// </summary>
            public int FileOffset { get; private set; }

            /// <summary>
            /// Number of offsets this line covers.  Will be > 0 for code and data, zero for
            /// everything else.  The same value is used for all lines in a multi-line sequence.
            /// </summary>
            public int OffsetSpan { get; private set; }

            /// <summary>
            /// For multi-line entries, this indicates which line is represented.  For
            /// single-line entries, this will be zero.
            /// </summary>
            public int SubLineIndex { get; private set; }

            /// <summary>
            /// Strings for display.  Creation may be deferred.  Use the DisplayList
            /// GetFormattedParts() method to access this property.
            /// </summary>
            /// <remarks>
            /// Certain elements, such as multi-line comments, must be formatted to determine
            /// the number of lines they span.  We retain the results to avoid formatting
            /// them twice.
            /// </remarks>
            public FormattedParts Parts { get; set; }

            /// <summary>
            /// Background color, used for Notes.
            /// </summary>
            public Color BackgroundColor { get; set; }

            /// <summary>
            /// String for searching.  May be created on demand when the Line is first searched.
            /// </summary>
            public string SearchString { get; set; }


            public Line(int offset, int span, Type type) : this(offset, span, type, 0) { }

            public Line(int offset, int span, Type type, int subLineIndex) {
                FileOffset = offset;
                OffsetSpan = span;
                LineType = type;
                SubLineIndex = subLineIndex;
            }

            /// <summary>
            /// True if this line is code or data.
            /// </summary>
            public bool IsCodeOrData {
                get {
                    return LineType == Type.Code || LineType == Type.Data;
                }
            }

            /// <summary>
            /// Returns true if the specified offset is represented by this line.  There
            /// will be only one code/data line for a given offset, but there may be
            /// multiple others (comments, notes, etc.) associated with it.
            /// </summary>
            /// <param name="offset"></param>
            /// <returns></returns>
            public bool Contains(int offset) {
                // Note OffsetSpan can be zero.
                return (offset == FileOffset ||
                        (offset >= FileOffset && offset < FileOffset + OffsetSpan));
            }

            public override string ToString() {
                return "Line type=" + LineType + " off=+" + FileOffset.ToString("x6") +
                    " span=" + OffsetSpan;
            }
        }

        /// <summary>
        /// Captures the set of selected lines.  Lines are identified by offset and type.
        /// 
        /// The idea is to save the selection, rebuild the list -- potentially moving
        /// stuff around -- and then rebuild the selection bitmap by finding matching
        /// items.
        /// 
        /// We don't try to identify parts of multi-line things.  If you've selected
        /// part of a multi-line string, then when we restore the selection you'll have
        /// the entire string selected.  For the operations that are possible across
        /// multiple offsets, this seems like reasonable behavior.
        /// 
        /// We can't precisely restore the selection in terms of which file offsets
        /// are selected.  If you select one byte and apply a code hint, we'll restore
        /// the selection to a line with 1-4 bytes.  This gets weird if you hit "undo",
        /// as you will then have 1-4 bytes selected rather than the original one.  It
        /// might be better to just clear the selection on "undo".
        /// </summary>
        public class SavedSelection {
            private class Tag {
                public int mOffset;
                public int mSpan;
                public Line.Type mTypes;

                public Tag(int offset, int span, Line.Type lineType) {
                    //Debug.Assert(offset >= 0);
                    Debug.Assert(span >= 0);
                    mOffset = offset;
                    mSpan = (span == 0) ? 1 : span;
                    mTypes = lineType;
                }
            }

            private List<Tag> mSelectionTags = new List<Tag>();

            /// <summary>
            /// Holds data that helps us position the scroll view at the correct position
            /// after changes have been applied.
            /// </summary>
            private class Top {
                // File offset of line.
                public int FileOffset { get; private set; }
                // Number of lines between the first line at the specified offset and the
                // target line.
                public int LineDelta { get; private set; }

                public Top(int fileOffset, int lineDelta) {
                    FileOffset = fileOffset;
                    LineDelta = lineDelta;
                    Debug.WriteLine("New Top: " + this);
                }
                public override string ToString() {
                    return "[Top: off=+" + FileOffset.ToString("x6") + " delta=" + LineDelta + "]";
                }
            }

            /// <summary>
            /// This is a place to save the file offset associated with the ListView's
            /// TopItem, so we can position the list appropriately.
            /// </summary>
            private Top mTopPosition;

            // Use Generate().
            private SavedSelection() { }

            /// <summary>
            /// Creates a new SavedSelection object, generating a list of tags from the
            /// lines that are currently selected.
            /// 
            /// If nothing is selected, SavedSelection will have no members.
            /// </summary>
            /// <param name="dl">Display list, with list of Lines.</param>
            /// <param name="sel">Bit vector specifying which lines are selected.</param>
            /// <param name="topIndex">Index of line that appears at the top of the list
            ///   control.</param>
            /// <returns>New SavedSelection object.</returns>
            public static SavedSelection Generate(LineListGen dl, DisplayListSelection sel,
                    int topIndex) {
                SavedSelection savedSel = new SavedSelection();
                //Debug.Assert(topOffset >= 0);

                int topOffset = dl[topIndex].FileOffset;
                int firstIndex = dl.FindLineIndexByOffset(topOffset);
                Debug.Assert(topIndex >= firstIndex);
                savedSel.mTopPosition = new Top(topOffset, topIndex - firstIndex);

                List<Line> lineList = dl.mLineList;
                Debug.Assert(lineList.Count == sel.Length);

                // Generate tags, which are a combination of the offset, span, and a merge
                // of types of all the lines associated with that offset.
                //
                // We may want to consider some sort of optimization for a "select all"
                // operation, although there aren't many changes you can make after selecting
                // all lines in a very large file.
                Tag tag = null;
                int curOffset = -1;
                for (int i = 0; i < lineList.Count; i++) {
                    if (!sel[i]) {
                        continue;
                    }
                    Line line = lineList[i];
                    // Code hinting can transform code to data and vice-versa, so we
                    // want the tag to reflect the fact that both could exist.
                    Line.Type lineType = line.LineType;
                    if (lineType == Line.Type.Code || lineType == Line.Type.Data) {
                        lineType = Line.Type.CodeOrData;
                    }
                    if (line.FileOffset != curOffset) {
                        // advanced to new offset, flush previous
                        if (tag != null) {
                            savedSel.mSelectionTags.Add(tag);
                        }
                        curOffset = line.FileOffset;

                        tag = new Tag(line.FileOffset, line.OffsetSpan, lineType);
                    } else {
                        // another item at same offset
                        tag.mSpan = Math.Max(tag.mSpan, line.OffsetSpan);
                        tag.mTypes |= lineType;
                    }
                }
                if (curOffset == -1) {
                    // It's hard to cause an action that requires save/restore when you don't
                    // have anything selected in the ListView.  However, this can happen if
                    // you do a sequence like:
                    // - Open a file that starts with a JMP followed by data.
                    // - Click on the blank line below the code, which has the code's offset,
                    //   and select "remove hint".  This causes the blank line to vanish,
                    //   so the Restore() won't select anything.
                    // - Click "undo".
                    Debug.WriteLine("NOTE: no selection found");
                } else {
                    // Add the in-progress tag to the list.
                    savedSel.mSelectionTags.Add(tag);
                }

                return savedSel;
            }

            /// <summary>
            /// Creates a selection set by identifying the set of lines in the display list
            /// that correspond to items in the SavedSelection tag list.
            /// </summary>
            /// <param name="dl">Display list, with list of Lines.</param>
            /// <returns>Set of selected lines.</returns>
            public DisplayListSelection Restore(LineListGen dl, out int topIndex) {
                List<Line> lineList = dl.mLineList;
                DisplayListSelection sel = new DisplayListSelection(lineList.Count);

                topIndex = -1;

                // Walk through the tag list, which is ordered by ascending offset, and
                // through the display list, which is similarly ordered.
                int tagIndex = 0;
                int lineIndex = 0;
                while (tagIndex < mSelectionTags.Count && lineIndex < lineList.Count) {
                    Tag tag = mSelectionTags[tagIndex];
                    int lineOffset = lineList[lineIndex].FileOffset;

                    // If a line encompassing this offset was at the top of the ListView
                    // control before, use this line's index as the top.
                    if (topIndex < 0 && lineList[lineIndex].Contains(mTopPosition.FileOffset)) {
                        topIndex = lineIndex;
                    }

                    if (lineOffset >= tag.mOffset && lineOffset < tag.mOffset + tag.mSpan) {
                        // Intersection.  If the line type matches, add it to the set.
                        if ((tag.mTypes & lineList[lineIndex].LineType) != 0) {
                            sel[lineIndex] = true;
                        }

                        // Advance to the next line entry.
                        lineIndex++;
                    } else if (tag.mOffset < lineOffset) {
                        // advance tag
                        tagIndex++;
                    } else {
                        Debug.Assert(tag.mOffset > lineOffset);
                        lineIndex++;
                    }
                }

                // Continue search for topIndex, if necessary.
                while (topIndex < 0 && lineIndex < lineList.Count) {
                    if (lineList[lineIndex].Contains(mTopPosition.FileOffset)) {
                        topIndex = lineIndex;
                        break;
                    }
                    lineIndex++;
                }
                Debug.WriteLine("TopOffset " + mTopPosition + " --> index " + topIndex);

                // Adjust position within an element.  This is necessary so we don't jump to
                // the top of multi-line long comments or notes whenever any part of that
                // comment or note is at the top of the list.
                if (topIndex >= 0 && mTopPosition.LineDelta > 0) {
                    int adjIndex = topIndex + mTopPosition.LineDelta;
                    if (adjIndex >= lineList.Count ||
                            lineList[adjIndex].FileOffset != mTopPosition.FileOffset) {
                        Debug.WriteLine("Can't adjust top position");
                        // can't adjust; maybe they deleted several lines from comment
                    } else {
                        topIndex = adjIndex;
                        Debug.WriteLine("Top index adjusted to " + adjIndex);
                    }
                }

                if (topIndex < 0) {
                    // This can happen if you delete the header comment while scrolled
                    // to the top of the list.
                    topIndex = 0;
                }
                return sel;
            }

            public void DebugDump() {
                Debug.WriteLine("Selection (" + mSelectionTags.Count + " offsets):");
                foreach (Tag tag in mSelectionTags) {
                    Debug.WriteLine(" +" + tag.mOffset.ToString("x6") + "/" +
                        tag.mSpan + ": " + tag.mTypes);
                }
            }
        }



        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="proj">Project object.</param>
        /// <param name="formatter">Formatter object.</param>
        public LineListGen(DisasmProject proj, DisplayList displayList, Formatter formatter,
                PseudoOp.PseudoOpNames opNames) {
            Debug.Assert(proj != null);
            Debug.Assert(displayList != null);
            Debug.Assert(formatter != null);
            Debug.Assert(opNames != null);

            mProject = proj;
            mDisplayList = displayList;
            mFormatter = formatter;
            mPseudoOpNames = opNames;

            mLineList = new List<Line>();
            mFormattedLineCache = new FormattedOperandCache();
            mShowCycleCounts = AppSettings.Global.GetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS,
                false);
            mLvLookup = new LocalVariableLookup(mProject.LvTables, mProject, false);

            mDisplayList.ListGen = this;
        }

        /// <summary>
        /// Changes the Formatter object.  Clears the line list, instigating a full re-render.
        /// </summary>
        /// <param name="formatter">Formatter object.</param>
        public void SetFormatter(Formatter formatter) {
            mFormatter = formatter;
            mLineList.Clear();
            mDisplayList.Clear();

            // We probably just changed settings, so update this as well.
            mShowCycleCounts = AppSettings.Global.GetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS,
                false);
        }

        /// <summary>
        /// Changes the pseudo-op name object.  Clears the line list, instigating a
        /// full re-render.  If the new set is unchanged from the old set, nothing is done.
        /// </summary>
        /// <param name="opNames">Pseudo-op names.</param>
        public void SetPseudoOpNames(PseudoOp.PseudoOpNames opNames) {
            if (mPseudoOpNames == opNames) {
                return;
            }

            mPseudoOpNames = opNames;
            Debug.Assert(mPseudoOpNames == opNames);
            mLineList.Clear();
            mDisplayList.Clear();
        }

        /// <summary>
        /// Number of lines in the list.
        /// </summary>
        public int Count { get { return mLineList.Count; } }

        /// <summary>
        /// Retrieves the Nth element.
        /// </summary>
        public Line this[int key] {
            get {
                return mLineList[key];
            }
        }

        /// <summary>
        /// Returns the Line's FormattedParts object, generating it first if necessary.
        /// </summary>
        /// <returns>Object with formatted strings.</returns>
        public FormattedParts GetFormattedParts(int index) {
            Line line = mLineList[index];
            if (line.Parts == null) {
                FormattedParts parts;
                switch (line.LineType) {
                    case Line.Type.Code:
                        parts = GenerateInstructionLine(line.FileOffset, line.OffsetSpan);
                        break;
                    case Line.Type.Data:
                        parts = GenerateDataLine(line.FileOffset, line.SubLineIndex);
                        break;
                    case Line.Type.LocalVariableTable:
                        parts = GenerateLvTableLine(line.FileOffset, line.SubLineIndex);
                        break;
                    case Line.Type.Blank:
                        // Nothing to do.
                        parts = FormattedParts.CreateBlankLine();
                        break;
                    case Line.Type.OrgDirective:
                    case Line.Type.RegWidthDirective:
                    case Line.Type.LongComment:
                    case Line.Type.Note:
                    // should have been done already
                    default:
                        Debug.Assert(false);
                        parts = FormattedParts.Create("x", "x", "x", "x", "x", "x", "x", "x", "x");
                        break;
                }
                line.Parts = parts;
            }
            return line.Parts;
        }

        /// <summary>
        /// Returns a string with the concatenation of the searchable portions of the line.
        /// Different sections are separated with an unlikely unicode character.  The goal
        /// is to have a single string per line that can be searched quickly, without having
        /// adjacent fields spill into each other.
        /// </summary>
        /// <param name="index">Line index.</param>
        /// <returns>Formatted line contents.</returns>
        public string GetSearchString(int index) {
            Line line = mLineList[index];
            if (line.SearchString == null) {
                const char sep = '\u203b';  // REFERENCE MARK

                FormattedParts parts = GetFormattedParts(index);
                StringBuilder sb = new StringBuilder();
                // Some parts may be null, e.g. for long comments.  Append() can deal.
                sb.Append(parts.Label);
                sb.Append(sep);
                sb.Append(parts.Opcode);
                sb.Append(sep);
                sb.Append(parts.Operand);
                sb.Append(sep);
                sb.Append(parts.Comment);
                line.SearchString = sb.ToString();
            }
            return line.SearchString;
        }

        /// <summary>
        /// Finds the first line entry that encompasses the specified offset.
        /// </summary>
        /// <param name="offset">Offset to search for.  Negative values are allowed.</param>
        /// <returns>Line list index, or -1 if not found.</returns>
        private static int FindLineByOffset(List<Line> lineList, int offset) {
            if (lineList.Count == 0) {
                return -1;
            }

            int low = 0;
            int high = lineList.Count - 1;
            int mid = -1;
            bool found = false;
            while (low <= high) {
                mid = (low + high) / 2;
                Line line = lineList[mid];

                if (line.Contains(offset)) {
                    // found a match
                    found = true;
                    break;
                } else if (line.FileOffset > offset) {
                    // too big, move the high end in
                    high = mid - 1;
                } else if (line.FileOffset < offset) {
                    // too small, move the low end in
                    low = mid + 1;
                } else {
                    // WTF
                    throw new Exception("Bad binary search");
                }
            }

            if (!found) {
                return -1;
            }

            // We found *a* matching line.  Seek backward to find the *first* matching line.
            while (mid > 0) {
                Line upLine = lineList[mid - 1];
                if (upLine.Contains(offset)) {
                    mid--;
                } else {
                    break;
                }
            }

            return mid;
        }

        /// <summary>
        /// Finds the first line entry that encompasses the specified offset.
        /// </summary>
        /// <param name="offset">Offset to search for.</param>
        /// <returns>Line list index, or -1 if not found.</returns>
        public int FindLineIndexByOffset(int offset) {
            return FindLineByOffset(mLineList, offset);
        }

        /// <summary>
        /// Finds the code or data line entry that encompasses the specified offset.
        /// </summary>
        /// <param name="offset">Offset to search for.</param>
        /// <returns>Line list index, or -1 if not found.</returns>
        public int FindCodeDataIndexByOffset(int offset) {
            if (offset < 0) {
                // Header offset. No code or data here.
                return -1;
            }
            int index = FindLineByOffset(mLineList, offset);
            if (index < 0) {
                return -1;
            }
            while (mLineList[index].LineType != Line.Type.Code &&
                    mLineList[index].LineType != Line.Type.Data) {
                index++;
            }
            return index;
        }

        /// <summary>
        /// Generates Lines for the entire project.
        /// </summary>
        public void GenerateAll() {
            mLineList.Clear();
            List<Line> headerLines = GenerateHeaderLines(mProject, mFormatter, mPseudoOpNames);
            mLineList.InsertRange(0, headerLines);

            GenerateLineList(0, mProject.FileData.Length - 1, mLineList);

            mDisplayList.ResetList(mLineList.Count);

            Debug.Assert(ValidateLineList(), "Display list failed validation");
        }

        /// <summary>
        /// Generates a list of Lines for the specified range of offsets, replacing
        /// existing values.
        /// </summary>
        /// <param name="startOffset">First offset. Must be the start of an instruction
        ///   or data area.</param>
        /// <param name="endOffset">End offset (inclusive).</param>
        public void GenerateRange(int startOffset, int endOffset) {
            if (startOffset < 0) {
                // Clear existing header lines. Find the first non-header item.
                int headerEndIndex = FindLineByOffset(mLineList, 0);
                if (headerEndIndex == 0) {
                    // no header lines present
                    Debug.WriteLine("No header lines found");
                } else {
                    Debug.WriteLine("Removing " + headerEndIndex + " header lines");
                    mLineList.RemoveRange(0, headerEndIndex);
                }

                List<Line> headerLines = GenerateHeaderLines(mProject, mFormatter, mPseudoOpNames);
                mLineList.InsertRange(0, headerLines);
                mDisplayList.ClearListSegment(0, headerEndIndex, headerLines.Count);

                if (endOffset < 0) {
                    // nothing else to do -- header-only change
                    return;
                }
                // do the rest
                startOffset = 0;
            }
            Debug.Assert(startOffset >= 0);
            Debug.Assert(endOffset < mProject.FileData.Length);
            Debug.Assert(endOffset >= startOffset);
            //Debug.WriteLine("DL gen range [" + startOffset + "," + endOffset + "]");

            // Find the start index.  The start offset should always appear at the
            // start of a Line (as opposed to being in the middle of a multi-byte instruction)
            // because it comes from item selection.
            int startIndex = FindLineByOffset(mLineList, startOffset);
            if (startIndex < 0) {
                Debug.Assert(false, "Unable to find startOffset " + startOffset);
                GenerateAll();
                return;
            }
            // Find the end index.  The end offset can be part of a multi-line data item, like
            // a long string.  Find the first Line that starts at an offset larger than endOffset.
            int endIndex;
            if (startOffset == endOffset) {
                // Simple optimization for single-offset groups.
                endIndex = startIndex;
            } else {
                endIndex = FindLineByOffset(mLineList, endOffset);
            }
            if (endIndex < 0) {
                Debug.Assert(false, "Unable to find endOffset " + endOffset);
                GenerateAll();
                return;
            }
            // There may be more than one line involved, so we need to scan forward.
            for (endIndex++; endIndex < mLineList.Count; endIndex++) {
                if (mLineList[endIndex].FileOffset > endOffset) {
                    endIndex--;
                    break;
                }
            }
            if (endIndex == mLineList.Count) {
                // whoops, loop ended before we had a chance to decrement
                endIndex = mLineList.Count - 1;
            }
            Debug.WriteLine("GenerateRange: offset [+" + startOffset.ToString("x6") + ",+" +
                endOffset.ToString("x6") +
                "] maps to index [" + startIndex + "," + endIndex + "]");
            Debug.Assert(endIndex >= startIndex);

            // Create temporary list to hold new lines.  Set the initial capacity to
            // the previous size, on the assumption that it won't change much.
            List<Line> newLines = new List<Line>(endIndex - startIndex + 1);

            GenerateLineList(startOffset, endOffset, newLines);

            // Out with the old, in with the new.
            mLineList.RemoveRange(startIndex, endIndex - startIndex + 1);
            mLineList.InsertRange(startIndex, newLines);
            mDisplayList.ClearListSegment(startIndex, endIndex - startIndex + 1, newLines.Count);

            Debug.Assert(ValidateLineList(), "Display list failed validation");
        }

        /// <summary>
        /// Validates the line list, confirming that every offset is represented exactly once.
        /// </summary>
        /// <returns>True if all is well.</returns>
        private bool ValidateLineList() {
            int expectedOffset = 0;
            int lastOffset = Int32.MinValue;
            foreach (Line line in mLineList) {
                // Header lines aren't guaranteed to be sequential and don't have a span.
                // They are expected to be in sorted order, and to be unique (with the
                // notable exception of the header comment, which is multi-line).
                if (line.FileOffset < 0) {
                    if (line.FileOffset < lastOffset || (line.LineType != Line.Type.LongComment &&
                            line.FileOffset == lastOffset)) {
                        Debug.WriteLine("Header offsets went backward: cur=" +
                            line.FileOffset + " last=" + lastOffset);
                        return false;
                    }
                    lastOffset = line.FileOffset;
                    continue;
                }

                // Blank lines and comments can appear before or after code/data.  They
                // must have the offset of the associated line, and a span of zero.
                if (line.FileOffset != expectedOffset && line.FileOffset != lastOffset) {
                    Debug.WriteLine("ValidateLineList: bad offset " + line.FileOffset +
                        " (last=" + lastOffset + ", expected next=" + expectedOffset + ")");
                    return false;
                }

                if (line.SubLineIndex != 0) {
                    // In the middle of a multi-line thing, don't advance last/expected.
                    Debug.Assert(line.FileOffset == lastOffset);
                } else {
                    lastOffset = expectedOffset;
                    expectedOffset += line.OffsetSpan;
                }
            }

            if (expectedOffset != mProject.FileData.Length) {
                Debug.WriteLine("ValidateLineList: did not cover entire file: last offset " +
                    expectedOffset + ", file has " + mProject.FileData.Length);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates a synthetic offset for the FileOffset field from an index value.  The
        /// index arg is the index of an entry in the DisasmProject.ActiveDefSymbolList.
        /// (The exact algorithm isn't too important, as these offsets are not stored in the
        /// project file.)
        /// </summary>
        private static int DefSymOffsetFromIndex(int index) {
            Debug.Assert(index >= 0 && index < (1 << 24));
            return index - (1 << 24);
        }

        /// <summary>
        /// Returns the DisasmProject.ActiveDefSymbolList index for an EQU line with
        /// the specified file offset.
        /// </summary>
        public static int DefSymIndexFromOffset(int offset) {
            Debug.Assert(offset < 0);
            return offset + (1 << 24);
        }

        // NOTE: the two functions above are tied to the implementation of the function
        // below: we output the lines in the order in which they appear in ActiveDefSymbolList.
        // If we want to get fancy and sort them, we'll need to do some additional work.

        /// <summary>
        /// Generates the header lines (header comment, EQU directives).
        /// </summary>
        /// <param name="proj">Project reference.</param>
        /// <param name="formatter">Output formatter.</param>
        /// <param name="opNames">Pseudo-op names.</param>
        /// <returns>List with header lines.</returns>
        private static List<Line> GenerateHeaderLines(DisasmProject proj, Formatter formatter,
                PseudoOp.PseudoOpNames opNames) {
            List<Line> tmpLines = new List<Line>();
            Line line;

            // Check for header comment.
            if (proj.LongComments.TryGetValue(Line.HEADER_COMMENT_OFFSET,
                    out MultiLineComment headerComment)) {
                List<string> formatted = headerComment.FormatText(formatter, string.Empty);
                StringListToLines(formatted, Line.HEADER_COMMENT_OFFSET, Line.Type.LongComment,
                    CommonWPF.Helper.ZeroColor, tmpLines);
            }

            // Format symbols.
            int index = 0;
            foreach (DefSymbol defSym in proj.ActiveDefSymbolList) {
                line = new Line(DefSymOffsetFromIndex(index), 0, Line.Type.EquDirective);
                // Use an operand length of 1 so things are shown as concisely as possible.
                string valueStr = PseudoOp.FormatNumericOperand(formatter, proj.SymbolTable,
                    null, defSym.DataDescriptor, defSym.Value, 1,
                    PseudoOp.FormatNumericOpFlags.None);
                valueStr = PseudoOp.AnnotateEquDirective(formatter, valueStr, defSym);
                string comment = formatter.FormatEolComment(defSym.Comment);
                FormattedParts parts = FormattedParts.CreateEquDirective(defSym.Label,
                    formatter.FormatPseudoOp(opNames.EquDirective),
                    valueStr, comment);
                line.Parts = parts;
                tmpLines.Add(line);
                index++;
            }

            if (proj.ActiveDefSymbolList.Count != 0) {
                // We had some EQUs, throw a blank line at the end.
                index++;
                line = new Line(DefSymOffsetFromIndex(index), 0, Line.Type.Blank);
                tmpLines.Add(line);
            }

            return tmpLines;
        }

        /// <summary>
        /// Generates lines for the specified range of file offsets.
        /// 
        /// Does not generate formatted parts in most cases; that usually happens on demand.
        /// Complicated items, such as word-wrapped long comments, may be generated now
        /// and saved off.
        /// 
        /// This still needs a formatter arg even when no text is rendered because some
        /// options, like maximum per-line operand length, might affect how many lines
        /// are generated.
        /// </summary>
        /// <param name="startOffset">Offset of first byte.</param>
        /// <param name="endOffset">Offset of last byte.</param>
        /// <param name="lines">List to add output lines to.</param>
        private void GenerateLineList(int startOffset, int endOffset, List<Line> lines) {
            //Debug.WriteLine("GenerateRange [+" + startOffset.ToString("x6") + ",+" +
            //    endOffset.ToString("x6") + "]");

            Debug.Assert(startOffset >= 0);
            Debug.Assert(endOffset >= startOffset);

            // Assume variables may have changed.
            mLvLookup.Reset();

            // Find the previous status flags for M/X tracking.
            StatusFlags prevFlags = StatusFlags.AllIndeterminate;
            if (mProject.CpuDef.HasEmuFlag) {
                for (int scanoff = startOffset - 1; scanoff >= 0; scanoff--) {
                    Anattrib attr = mProject.GetAnattrib(scanoff);
                    if (attr.IsInstructionStart) {
                        prevFlags = attr.StatusFlags;
                        // Apply the same tweak here that we do to curFlags below.
                        prevFlags.M = attr.StatusFlags.ShortM ? 1 : 0;
                        prevFlags.X = attr.StatusFlags.ShortX ? 1 : 0;
                        Debug.WriteLine("GenerateLineList startOff=+" +
                            startOffset.ToString("x6") + " using initial flags from +" +
                            scanoff.ToString("x6") + ": " + prevFlags);
                        break;
                    }
                }
            }

            // Configure the initial value of addBlank.  The specific case we're handling is
            // a no-continue instruction (e.g. JMP) followed by an instruction with a label.
            // When we rename the label, we don't want the blank to disappear during the
            // partial-list generation.
            bool addBlank = false;
            if (startOffset > 0) {
                int baseOff = DataAnalysis.GetBaseOperandOffset(mProject, startOffset - 1);
                if (mProject.GetAnattrib(baseOff).DoesNotContinue) {
                    addBlank = true;
                }
            }

            int offset = startOffset;
            while (offset <= endOffset) {
                Anattrib attr = mProject.GetAnattrib(offset);
                if (attr.IsInstructionStart && offset > 0 &&
                        mProject.GetAnattrib(offset - 1).IsData) {
                    // Transition from data to code.  (Don't add blank line for inline data.)
                    lines.Add(GenerateBlankLine(offset));
                } else if (addBlank) {
                    // Previous instruction wanted to be followed by a blank line.
                    lines.Add(GenerateBlankLine(offset));
                }
                addBlank = false;

                // Insert long comments and notes.  These may span multiple display lines,
                // and require word-wrap, so it's easiest just to render them fully here.
                // TODO: integrate into FormattedOperandCache so we don't have to
                //   regenerate them unless they change.  Use the MLC as the dependency.
                if (mProject.Notes.TryGetValue(offset, out MultiLineComment noteData)) {
                    List<string> formatted = noteData.FormatText(mFormatter, "NOTE: ");
                    StringListToLines(formatted, offset, Line.Type.Note,
                        noteData.BackgroundColor, lines);
                }
                if (mProject.LongComments.TryGetValue(offset, out MultiLineComment longComment)) {
                    List<string> formatted = longComment.FormatText(mFormatter, string.Empty);
                    StringListToLines(formatted, offset, Line.Type.LongComment,
                        longComment.BackgroundColor, lines);
                }

                // Local variable tables come next.  Defer rendering.
                if (mProject.LvTables.TryGetValue(offset, out LocalVariableTable lvt)) {
                    int count = lvt.Count;
                    // If "clear previous" is set, we output an additional line.
                    if (lvt.ClearPrevious) {
                        count++;
                    }
                    if (count == 0) {
                        // Need to show something so the user will know an empty table is here.
                        count = 1;
                    }
                    for (int i = 0; i < count; i++) {
                        Line line = new Line(offset, 0, Line.Type.LocalVariableTable, i);
                        lines.Add(line);
                    }
                    lines.Add(GenerateBlankLine(offset));
                }

                if (attr.IsInstructionStart) {
                    // Generate reg width directive, if necessary.
                    if (mProject.CpuDef.HasEmuFlag) {
                        // Changing from "ambiguous but assumed short" to "definitively short"
                        // merits a directive, notably at the start of the file.  The tricky
                        // part is that E=1 means definitively M=1 X=1.  And maybe
                        // indeterminate E also means that.
                        //
                        // We don't want to mess with Anattrib, but we do need to tell the
                        // assembler something.  So we tweak our local copy and propagate it.
                        string operandStr = string.Empty;
                        StatusFlags curFlags = attr.StatusFlags;
                        curFlags.M = attr.StatusFlags.ShortM ? 1 : 0;
                        curFlags.X = attr.StatusFlags.ShortX ? 1 : 0;
                        if (curFlags.M != prevFlags.M) {
                            operandStr = (curFlags.M == 0) ? "longm" : "shortm";
                        }

                        if (curFlags.X != prevFlags.X) {
                            if (operandStr.Length > 0) {
                                operandStr += ",";
                            }
                            operandStr += (curFlags.X == 0) ? "longx" : "shortx";
                        }

                        if (operandStr.Length > 0) {
                            Line rwLine = new Line(offset, 0, Line.Type.RegWidthDirective);
                            // FormatPseudoOp isn't quite right for the operand, but there
                            // isn't anything more suitable, and there are only eight
                            // possible values.  Having the operand capitalization match the
                            // pseudo-op's feels reasonable.
                            rwLine.Parts = FormattedParts.CreateDirective(
                                mFormatter.FormatPseudoOp(mPseudoOpNames.RegWidthDirective),
                                mFormatter.FormatPseudoOp(operandStr));
                            lines.Add(rwLine);
                        }
                        prevFlags = curFlags;
                    }

                    // Look for embedded instructions.
                    int len;
                    for (len = 1; len < attr.Length; len++) {
                        if (mProject.GetAnattrib(offset + len).IsInstructionStart) {
                            break;
                        }
                    }

                    // Create Line entry.  Offset span only covers the instruction up to
                    // the point where the embedded instruction starts.
                    Line line = new Line(offset, len, Line.Type.Code);
                    lines.Add(line);

                    // Insert blank after an instruction that doesn't continue.  Provides a
                    // break in code, and before a data area.
                    // TODO(maybe): Might also want to do this if the next offset is data,
                    // to make things look nicer when code runs directly into data.
                    //
                    // We don't want to add it with the current line's offset.  If we do that,
                    // the binary search will get confused, because blank lines have a span
                    // of zero.  If the code is at offset 10 with length 3, and we search for
                    // the byte at offset 11, then a blank line (with span=0) at offset 10 will
                    // cause the binary search to assume that the target is farther down, when
                    // it's actually one line up.  We deal with this by setting a flag and
                    // generating the blank line on the next trip through the loop.
                    if (attr.DoesNotContinue) {
                        addBlank = true;
                    }

                    offset += len;
                } else {
                    Debug.Assert(attr.DataDescriptor != null);
                    if (attr.DataDescriptor.IsString) {
                        // See if we've already got this one.
                        List<string> strLines = mFormattedLineCache.GetStringEntry(offset,
                            mFormatter, attr.DataDescriptor, mPseudoOpNames, out string popcode);
                        if (strLines == null) {
                            //Debug.WriteLine("FMT string at +" + offset.ToString("x6"));
                            strLines = PseudoOp.FormatStringOp(mFormatter, mPseudoOpNames,
                                attr.DataDescriptor, mProject.FileData, offset,
                                out popcode);
                            mFormattedLineCache.SetStringEntry(offset, strLines, popcode,
                                mFormatter, attr.DataDescriptor, mPseudoOpNames);
                        }
                        FormattedParts[] partsArray = GenerateStringLines(offset,
                            popcode, strLines);
                        for (int i = 0; i < strLines.Count; i++) {
                            Line line = new Line(offset, attr.Length, Line.Type.Data, i);
                            line.Parts = partsArray[i];
                            lines.Add(line);
                        }
                    } else {
                        int numLines = PseudoOp.ComputeRequiredLineCount(mFormatter,
                            mPseudoOpNames,attr.DataDescriptor, mProject.FileData, offset);
                        for (int i = 0; i < numLines; i++) {
                            Line line = new Line(offset, attr.Length, Line.Type.Data, i);
                            lines.Add(line);
                        }
                    }
                    offset += attr.Length;
                }
            }

            // See if there were any address shifts in this section.  If so, go back and
            // insert an ORG statement as the first entry for the offset.  We're expecting to
            // have very few AddressMap entries (usually just one), so it's more efficient to
            // process them here and walk through the sub-list than it is to ping the address map
            // at every line.
            //
            // It should not be possible for an address map change to appear in the middle
            // of an instruction or data item.
            foreach (AddressMap.AddressMapEntry ent in mProject.AddrMap) {
                if (ent.Offset < startOffset || ent.Offset > endOffset) {
                    continue;
                }
                int index = FindLineByOffset(lines, ent.Offset);
                if (index < 0) {
                    Debug.WriteLine("Couldn't find offset " + ent.Offset +
                        " in range we just generated");
                    Debug.Assert(false);
                    continue;
                }
                if (lines[index].LineType == Line.Type.Blank) {
                    index++;
                }
                Line topLine = lines[index];
                Line newLine = new Line(topLine.FileOffset, 0, Line.Type.OrgDirective);
                string addrStr = mFormatter.FormatHexValue(ent.Addr, 4);
                newLine.Parts = FormattedParts.CreateDirective(
                    mFormatter.FormatPseudoOp(mPseudoOpNames.OrgDirective), addrStr);
                lines.Insert(index, newLine);

                // Prepend a blank line if the previous line wasn't already blank, and this
                // isn't the ORG at the start of the file.  (This may temporarily do
                // double-spacing if we do a partial update, because we won't be able to
                // "see" the previous line.  Harmless.)
                // TODO: consider always adding blanks, and doing a fix-up pass afterward.
                //       (but keep in mind that blank lines should always come above things)
                //
                // Interesting case:
                //   .dd2 $1000
                //   <blank>
                //   .org $1234
                //   .dd2 $aabb    ;comment
                // We need to include "index == 0" or we'll lose the blank when the comment
                // is edited.
                if (ent.Offset != 0 &&
                        (index == 0 || (index > 0 && lines[index-1].LineType != Line.Type.Blank))){
                    Line blankLine = GenerateBlankLine(topLine.FileOffset);
                    lines.Insert(index, blankLine);
                }
            }
        }

        /// <summary>
        /// Generates a blank line entry.
        /// </summary>
        private static Line GenerateBlankLine(int offset) {
            return new Line(offset, 0, Line.Type.Blank);
        }

        /// <summary>
        /// Takes a list of strings and adds them to the Line list as long comments.
        /// </summary>
        /// <param name="list">String list.</param>
        /// <param name="offset">File offset of item start.</param>
        /// <param name="lineType">What type of line this is.</param>
        /// <param name="color">Background color (for Notes).</param>
        /// <param name="lines">Line list to add data to.</param>
        private static void StringListToLines(List<string> list, int offset, Line.Type lineType,
                Color color, List<Line> lines) {
            foreach (string str in list) {
                Line line = new Line(offset, 0, lineType);
                FormattedParts parts = FormattedParts.CreateNote(str, color);
                line.Parts = parts;
                line.BackgroundColor = color;
                lines.Add(line);
            }
        }

        private FormattedParts GenerateInstructionLine(int offset, int instrBytes) {
            Anattrib attr = mProject.GetAnattrib(offset);
            byte[] data = mProject.FileData;

            string offsetStr = mFormatter.FormatOffset24(offset);

            int addr = attr.Address;
            string addrStr = mFormatter.FormatAddress(addr, !mProject.CpuDef.HasAddr16);
            string bytesStr = mFormatter.FormatBytes(data, offset, instrBytes);
            string flagsStr = attr.StatusFlags.ToString(mProject.CpuDef.HasEmuFlag);
            string attrStr = attr.ToAttrString();

            string labelStr = string.Empty;
            if (attr.Symbol != null) {
                labelStr = attr.Symbol.Label;
            }

            OpDef op = mProject.CpuDef.GetOpDef(data[offset]);
            int operand = op.GetOperand(data, offset, attr.StatusFlags);
            int instrLen = op.GetLength(attr.StatusFlags);
            OpDef.WidthDisambiguation wdis = OpDef.WidthDisambiguation.None;
            if (op.IsWidthPotentiallyAmbiguous) {
                wdis = OpDef.GetWidthDisambiguation(instrLen, operand);
            }

            string opcodeStr = mFormatter.FormatOpcode(op, wdis);
            if (attr.Length != instrBytes) {
                // An instruction is embedded inside this one.  Note that BRK is a two-byte
                // instruction, so don't freak out if you see it marked as embedded when a
                // $00 is followed by actual code.  (But be a little freaked out that your
                // code is running into a BRK.)
                //opcodeStr = opcodeStr + " \u00bb";  // RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK
                //opcodeStr = opcodeStr + " \u23e9";  // BLACK RIGHT-POINTING DOUBLE TRIANGLE
                opcodeStr = opcodeStr + " \u25bc";  // BLACK DOWN-POINTING TRIANGLE
            }

            string formattedOperand = null;
            int operandLen = instrLen - 1;
            PseudoOp.FormatNumericOpFlags opFlags = PseudoOp.FormatNumericOpFlags.None;

            // Tweak branch instructions.  We want to show the absolute address rather
            // than the relative offset (which happens with the OperandAddress assignment
            // below), and 1-byte branches should always appear as a 4-byte hex value.
            if (op.AddrMode == OpDef.AddressMode.PCRel) {
                Debug.Assert(attr.OperandAddress >= 0);
                operandLen = 2;
                opFlags = PseudoOp.FormatNumericOpFlags.IsPcRel;
            } else if (op.AddrMode == OpDef.AddressMode.PCRelLong ||
                    op.AddrMode == OpDef.AddressMode.StackPCRelLong) {
                opFlags = PseudoOp.FormatNumericOpFlags.IsPcRel;
            } else if (op.AddrMode == OpDef.AddressMode.Imm ||
                    op.AddrMode == OpDef.AddressMode.ImmLongA ||
                    op.AddrMode == OpDef.AddressMode.ImmLongXY) {
                opFlags = PseudoOp.FormatNumericOpFlags.HasHashPrefix;
            }

            // Use the OperandAddress when available.  This is important for relative branch
            // instructions and PER, where we want to show the target address rather than the
            // operand value.
            int operandForSymbol = operand;
            if (attr.OperandAddress >= 0) {
                operandForSymbol = attr.OperandAddress;
            }

            // Check Length to watch for bogus descriptors.  ApplyFormatDescriptors() should
            // have discarded anything appropriate, so we might be able to eliminate this test.
            if (attr.DataDescriptor != null && attr.Length == attr.DataDescriptor.Length) {
                Debug.Assert(operandLen > 0);

                // Format operand as directed.
                if (op.AddrMode == OpDef.AddressMode.BlockMove) {
                    // Special handling for the double-operand block move.
                    string opstr1 = PseudoOp.FormatNumericOperand(mFormatter, mProject.SymbolTable,
                        null, attr.DataDescriptor, operand >> 8, 1,
                        PseudoOp.FormatNumericOpFlags.None);
                    string opstr2 = PseudoOp.FormatNumericOperand(mFormatter, mProject.SymbolTable,
                        null, attr.DataDescriptor, operand & 0xff, 1,
                        PseudoOp.FormatNumericOpFlags.None);
                    formattedOperand = '#' + opstr1 + "," + '#' + opstr2;
                } else {
                    formattedOperand = PseudoOp.FormatNumericOperand(mFormatter,
                        mProject.SymbolTable, mLvLookup, null, attr.DataDescriptor, offset,
                        operandForSymbol, operandLen, opFlags);
                }
            } else {
                // Show operand value in hex.
                if (op.AddrMode == OpDef.AddressMode.BlockMove) {
                    formattedOperand = '#' + mFormatter.FormatHexValue(operand >> 8, 2) + "," +
                        '#' + mFormatter.FormatHexValue(operand & 0xff, 2);
                } else {
                    if (operandLen == 2) {
                        // This is necessary for 16-bit operands, like "LDA abs" and "PEA val",
                        // when outside bank zero.  The bank is included in the operand address,
                        // but we don't want to show it here.
                        operandForSymbol &= 0xffff;
                    }
                    formattedOperand = mFormatter.FormatHexValue(operandForSymbol, operandLen * 2);
                }
            }
            string operandStr = mFormatter.FormatOperand(op, formattedOperand, wdis);

            string eolComment = mProject.Comments[offset];
            if (mShowCycleCounts) {
                bool branchCross = (attr.Address & 0xff00) != (operandForSymbol & 0xff00);
                int cycles = mProject.CpuDef.GetCycles(op.Opcode, attr.StatusFlags,
                    attr.BranchTaken, branchCross);
                if (cycles > 0) {
                    if (!string.IsNullOrEmpty(eolComment)) {
                        eolComment = cycles.ToString() + "  " + eolComment;
                    } else {
                        eolComment = cycles.ToString();
                    }
                } else {
                    if (!string.IsNullOrEmpty(eolComment)) {
                        eolComment = (-cycles).ToString() + "+ " + eolComment;
                    } else {
                        eolComment = (-cycles).ToString() + "+";
                    }
                }
            }
            string commentStr = mFormatter.FormatEolComment(eolComment);

            FormattedParts parts = FormattedParts.Create(offsetStr, addrStr, bytesStr,
                flagsStr, attrStr, labelStr, opcodeStr, operandStr, commentStr);
            if (mProject.StatusFlagOverrides[offset] != StatusFlags.DefaultValue) {
                parts = FormattedParts.SetFlagsModified(parts);
            }
            return parts;
        }

        private FormattedParts GenerateDataLine(int offset, int subLineIndex) {
            Anattrib attr = mProject.GetAnattrib(offset);
            byte[] data = mProject.FileData;

            string offsetStr, addrStr, bytesStr, flagsStr, attrStr, labelStr, opcodeStr,
                operandStr, commentStr;
            offsetStr = addrStr = bytesStr = flagsStr = attrStr = labelStr = opcodeStr =
                operandStr = commentStr = string.Empty;

            PseudoOp.PseudoOut pout = PseudoOp.FormatDataOp(mFormatter, mPseudoOpNames,
                mProject.SymbolTable, null, attr.DataDescriptor, mProject.FileData, offset,
                subLineIndex);
            if (subLineIndex == 0) {
                offsetStr = mFormatter.FormatOffset24(offset);

                addrStr = mFormatter.FormatAddress(attr.Address, !mProject.CpuDef.HasAddr16);
                if (attr.Symbol != null) {
                    labelStr = attr.Symbol.Label;
                }

                bytesStr = mFormatter.FormatBytes(data, offset, attr.Length);
                attrStr = attr.ToAttrString();

                opcodeStr = mFormatter.FormatPseudoOp(pout.Opcode);
                commentStr = mFormatter.FormatEolComment(mProject.Comments[offset]);
            } else {
                opcodeStr = " +";
            }

            operandStr = pout.Operand;

            FormattedParts parts = FormattedParts.Create(offsetStr, addrStr, bytesStr,
                flagsStr, attrStr, labelStr, opcodeStr, operandStr, commentStr);
            return parts;
        }

        private FormattedParts GenerateLvTableLine(int offset, int subLineIndex) {
            if (!mProject.LvTables.TryGetValue(offset, out LocalVariableTable lvt)) {
                Debug.Assert(false);
                return FormattedParts.CreateLongComment("BAD OFFSET +" + offset.ToString("x6") +
                    " sub=" + subLineIndex);
            }

            if (lvt.ClearPrevious) {
                if (subLineIndex == 0) {
                    return FormattedParts.CreateLongComment(
                        Res.Strings.LOCAL_VARIABLE_TABLE_CLEAR);
                } else {
                    // adjust for previously output "clear" line
                    subLineIndex--;
                }
            }

            if (lvt.Count == 0) {
                // If ClearPrevious is set, we returned the "clear" line for index zero.
                // So this is an empty table without a clear.  We want to show something so
                // the user knows there's dead weight here.
                Debug.Assert(subLineIndex == 0 && !lvt.ClearPrevious);
                return FormattedParts.CreateLongComment(
                    Res.Strings.LOCAL_VARIABLE_TABLE_EMPTY);
            }

            if (subLineIndex >= lvt.Count) {
                return FormattedParts.CreateLongComment("BAD INDEX +" + offset.ToString("x6") +
                    " sub=" + subLineIndex);
            } else {
                // We use the entry directly from the LvTable, without LvLookup processing.
                // This is good, because the entries in the display list match what the editor
                // shows, but not quite right, because labels that are duplicates of
                // non-variables (which ideally wouldn't happen) won't show their de-duplicated
                // form.  I think this is fine.
                DefSymbol defSym = lvt[subLineIndex];
                // Use an operand length of 1 so things are shown as concisely as possible.
                string addrStr = PseudoOp.FormatNumericOperand(mFormatter, mProject.SymbolTable,
                    null, defSym.DataDescriptor, defSym.Value, 1,
                    PseudoOp.FormatNumericOpFlags.None);
                addrStr = PseudoOp.AnnotateEquDirective(mFormatter, addrStr, defSym);
                string comment = mFormatter.FormatEolComment(defSym.Comment);
                return FormattedParts.CreateEquDirective(
                    mFormatter.FormatVariableLabel(defSym.Label),
                    mFormatter.FormatPseudoOp(mPseudoOpNames.VarDirective),
                    addrStr, comment);
            }
        }

        public DefSymbol GetLocalVariableFromLine(int lineIndex) {
            Line line = this[lineIndex];
            int offset = line.FileOffset;
            if (!mProject.LvTables.TryGetValue(offset, out LocalVariableTable lvt)) {
                Debug.Assert(false);
                return null;
            }
            int tableIndex = line.SubLineIndex;
            if (lvt.ClearPrevious) {
                tableIndex--;
            }
            if (tableIndex < 0 || tableIndex >= lvt.Count) {
                // Will be -1 on first line when ClearPrevious was set.  Will be zero on
                // first line of empty table.
                return null;
            }

            return lvt[tableIndex];
        }

        private FormattedParts[] GenerateStringLines(int offset, string popcode,
                List<string> operands) {
            FormattedParts[] partsArray = new FormattedParts[operands.Count];

            Anattrib attr = mProject.GetAnattrib(offset);
            byte[] data = mProject.FileData;

            string offsetStr, addrStr, bytesStr, attrStr, labelStr, opcodeStr,
                operandStr, commentStr;

            for (int subLineIndex = 0; subLineIndex < operands.Count; subLineIndex++) {
                if (subLineIndex == 0) {
                    offsetStr = mFormatter.FormatOffset24(offset);

                    addrStr = mFormatter.FormatAddress(attr.Address, !mProject.CpuDef.HasAddr16);
                    if (attr.Symbol != null) {
                        labelStr = attr.Symbol.Label;
                    } else {
                        labelStr = string.Empty;
                    }

                    bytesStr = mFormatter.FormatBytes(data, offset, attr.Length);
                    attrStr = attr.ToAttrString();

                    opcodeStr = mFormatter.FormatPseudoOp(popcode);
                    commentStr = mFormatter.FormatEolComment(mProject.Comments[offset]);
                } else {
                    offsetStr = addrStr = bytesStr = attrStr = labelStr = commentStr =
                        string.Empty;

                    opcodeStr = " +";
                }

                operandStr = operands[subLineIndex];

                FormattedParts parts = FormattedParts.Create(offsetStr, addrStr, bytesStr,
                    /*flags*/string.Empty, attrStr, labelStr, opcodeStr, operandStr, commentStr);

                partsArray[subLineIndex] = parts;
            }

            return partsArray;
        }
    }
}
