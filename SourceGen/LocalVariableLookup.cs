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

namespace SourceGen {
    /// <summary>
    /// Given a list of LocalVariableTables, this determines the mapping of values to symbols
    /// at a specific offset.
    /// </summary>
    public class LocalVariableLookup {
        /// <summary>
        /// List of tables.  The table's file offset is used as the key.
        /// </summary>
        private SortedList<int, LocalVariableTable> mLvTables;

        /// <summary>
        /// Table of symbols, used to ensure that symbols are globally unique.
        /// </summary>
        private SymbolTable mSymbolTable;

        /// <summary>
        /// Reference to project, so we can query the Anattrib array to identify "hidden" tables.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Set to true if we want all variables to be globally unique (because the assembler
        /// can't redefine them).
        /// </summary>
        private bool mDoUniquify;

        /// <summary>
        /// Label uniquification helper.
        ///
        /// The BaseLabel does not change, but Label is updated by MakeUnique.
        /// </summary>
        /// <remarks>
        /// LvLookup is run multiple times, and can be restarted in the middle of a run.  It's
        /// essential that UniqueLabel behaves deterministically.  For this to happen, the
        /// contents of SymbolTable can't change in a way that affects the outcome unless it
        /// also causes us to redo the uniquification.  This mostly means that we have to be
        /// very careful about creating duplicate symbols, so that we don't get halfway through
        /// the analysis pass and invalidate our previous work. It's best to leave
        /// uniquification disabled until we're generating assembly source code.
        ///
        /// The issues also make it hard to do the uniquification once, rather than every time we
        /// walk the code.  Not all symbol changes cause a re-analysis (e.g. renaming a user
        /// label does not), and we don't want to fill the symbol table with the uniquified
        /// names because it could block user labels that would otherwise be valid.
        /// </remarks>
        private class UniqueLabel {
            public string BaseLabel { get; private set; }
            public string Label { get; private set; }
            private int Counter { get; set; }

            public UniqueLabel(string baseLabel) {
                Label = BaseLabel = baseLabel;
                Counter = 0;
            }

            /// <summary>
            /// Updates the Label to be unique.  Call this when a symbol is defined or
            /// re-defined.
            /// </summary>
            /// <param name="symbolTable">Symbol table, for uniqueness check.</param>
            public void MakeUnique(SymbolTable symbolTable) {
                // The main symbol table might have user-supplied labels like "ptr_2", so we
                // need to keep testing against that.  However, it should not be possible for
                // us to clash with other uniquified variables.  So we don't need to check
                // for clashes in the UniqueLabel list.
                //
                // It *is* possible to clash with other variable base names, so we can't
                // exclude variables from our SymbolTable lookup.
                string testLabel;
                do {
                    Counter++;
                    testLabel = BaseLabel + "_" + Counter;
                } while (symbolTable.TryGetValue(testLabel, out Symbol sym));
                Label = testLabel;
            }
        }
        private Dictionary<string, UniqueLabel> mUniqueLabels;

        /// <summary>
        /// Duplicate label re-map.  This is applied before uniquification.
        /// </summary>
        /// <remarks>
        /// It's hard to do this as part of uniquification because the remapped base name ends
        /// up in the symbol table, and the uniquifier isn't able to tell that the entry in the
        /// symbol table is itself.  The logic is simpler if we just rename the label before
        /// the uniquifier ever sees it.
        /// </remarks>
        private Dictionary<string, string> mDupRemap;

        /// <summary>
        /// Most recently processed offset.
        /// </summary>
        private int mRecentOffset;

        /// <summary>
        /// Symbols defined at mRecentOffset.
        /// </summary>
        private List<DefSymbol> mRecentSymbols;

        /// <summary>
        /// Cumulative symbols defined at the current offset.
        /// </summary>
        private LocalVariableTable mCurrentTable;

        // Next point of interest.
        private int mNextLvtIndex;
        private int mNextLvtOffset;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lvTables">List of tables from the DisasmProject.</param>
        /// <param name="symbolTable">Full SymbolTable from the DisasmProject.  Used to
        ///   generate globally unique symbol names.</param>
        /// <param name="project">Project reference.</param>
        /// <param name="uniquify">Set to true if variable names cannot be redefined.</param>
        public LocalVariableLookup(SortedList<int, LocalVariableTable> lvTables,
                DisasmProject project, bool uniquify) {
            mLvTables = lvTables;
            mSymbolTable = project.SymbolTable;
            mProject = project;
            mDoUniquify = uniquify;

            mCurrentTable = new LocalVariableTable();
            mDupRemap = new Dictionary<string, string>();
            if (uniquify) {
                mUniqueLabels = new Dictionary<string, UniqueLabel>();
            }
            Reset();
        }

        public void Reset() {
            mRecentOffset = -1;
            mRecentSymbols = null;
            mCurrentTable.Clear();
            mUniqueLabels?.Clear();
            mDupRemap.Clear();
            if (mLvTables.Count == 0) {
                mNextLvtIndex = -1;
                mNextLvtOffset = mProject.FileDataLength;
            } else {
                mNextLvtIndex = 0;
                mNextLvtOffset = mLvTables.Keys[0];
            }
        }

        /// <summary>
        /// Gets the symbol associated with the operand of an instruction.
        /// </summary>
        /// <param name="offset">Offset of start of instruction.</param>
        /// <param name="operandValue">Operand value.</param>
        /// <param name="type">Operand type.  Should be ExternalAddress for DP ops, or
        ///   Constant for StackRel ops.</param>
        /// <returns>Symbol, or null if no match found.</returns>
        public DefSymbol GetSymbol(int offset, int operandValue, Symbol.Type type) {
            AdvanceToOffset(offset);
            return mCurrentTable.GetByValueRange(operandValue, 1, type);
        }

        /// <summary>
        /// Gets the symbol associated with a symbol reference.  If uniquification is enabled,
        /// the unique-label map for the specified offset will be used to transform the
        /// symbol reference.
        /// </summary>
        /// <param name="offset">Offset of start of instruction.</param>
        /// <param name="symRef">Reference to symbol.</param>
        /// <returns>Symbol, or null if no match found.</returns>
        public DefSymbol GetSymbol(int offset, WeakSymbolRef symRef) {
            AdvanceToOffset(offset);

            // The symRef uses the non-uniquified symbol, so we need to get the unique value at
            // the current offset.  We may need to do this even when variables can be
            // redefined, because we might have a variable that's a duplicate of a user label
            // or project symbol.
            string label = symRef.Label;
            if (mDupRemap.TryGetValue(symRef.Label, out string remap)) {
                label = remap;
            }
            //Debug.WriteLine("GetSymbol " + symRef.Label + " -> " + label);
            if (mUniqueLabels != null && mUniqueLabels.TryGetValue(label, out UniqueLabel ulab)) {
                //Debug.WriteLine("  Unique var " + symRef.Label + " -> " + ulab.Label);
                label = ulab.Label;
            }
            DefSymbol defSym = mCurrentTable.GetByLabel(label);

            // In theory this is okay, but in practice the only things asking for symbols are
            // entirely convinced that the symbol exists here.  So this is probably a bug.
            Debug.Assert(defSym != null);

            return defSym;
        }

        /// <summary>
        /// Gets a LocalVariableTable that is the result of merging all tables up to this point.
        /// </summary>
        /// <param name="offset">Target offset.</param>
        /// <returns>Combined table.</returns>
        public LocalVariableTable GetMergedTableAtOffset(int offset) {
            AdvanceToOffset(offset);
            return mCurrentTable;
        }

        /// <summary>
        /// Generates a list of variables defined at the specified offset, if a table is
        /// associated with that offset.
        /// </summary>
        /// <param name="offset">File data offset.</param>
        /// <returns>List of symbols, uniquified if desired, or null if no LocalVariableTable
        ///   exists at the specified offset.</returns>
        public List<DefSymbol> GetVariablesDefinedAtOffset(int offset) {
            AdvanceToOffset(offset);

            if (mRecentOffset == offset) {
                return mRecentSymbols;
            }
            return null;
        }

        /// <summary>
        /// Updates internal state to reflect the state of the world at the specified offset.
        /// </summary>
        /// <remarks>
        /// When the offset is greater than or equal to its value on a previous call, we can
        /// do an incremental update.  If the offset moves backward, we have to reset and walk
        /// forward again.
        /// </remarks>
        /// <param name="targetOffset">Target offset.</param>
        private void AdvanceToOffset(int targetOffset) {
            if (mNextLvtIndex < 0) {
                return;
            }
            if (targetOffset < mRecentOffset) {
                // We went backwards.
                Reset();
            }
            while (mNextLvtOffset <= targetOffset) {
                if (!mProject.GetAnattrib(mNextLvtOffset).IsStart) {
                    // Hidden table, ignore it.
                    Debug.WriteLine("Ignoring LvTable at +" + mNextLvtOffset.ToString("x6"));
                } else {
                    // Process this table.
                    LocalVariableTable lvt = mLvTables.Values[mNextLvtIndex];
                    if (lvt.ClearPrevious) {
                        mCurrentTable.Clear();
                    }

                    // Create a list for GetVariablesDefinedAtOffset
                    mRecentSymbols = new List<DefSymbol>();
                    mRecentOffset = mNextLvtOffset;

                    // Merge the new entries into the work table.  This automatically
                    // discards entries that clash by name or value.
                    for (int i = 0; i < lvt.Count; i++) {
                        DefSymbol defSym = lvt[i];

                        // Look for non-variable symbols with the same label.  Ordinarily the
                        // editor prevents this from happening, but there are ways to trick
                        // the system (e.g. add a symbol while the LvTable is hidden).  We
                        // deal with it here.
                        if (mSymbolTable.TryGetNonVariableValue(defSym.Label, out Symbol unused)) {
                            Debug.WriteLine("Detected duplicate non-var label " + defSym.Label +
                                " at +" + mNextLvtOffset.ToString("x6"));
                            string newLabel = DeDupLabel(defSym.Label);
                            mDupRemap[defSym.Label] = newLabel;
                            defSym = new DefSymbol(defSym, newLabel);
                        }

                        if (mDoUniquify) {
                            if (mUniqueLabels.TryGetValue(defSym.Label, out UniqueLabel ulab)) {
                                // We've seen this label before; generate a unique version by
                                // increasing the appended number.
                                ulab.MakeUnique(mSymbolTable);
                                defSym = new DefSymbol(defSym, ulab.Label);
                            } else {
                                // Haven't seen this before.  Add it to the unique-labels table.
                                mUniqueLabels.Add(defSym.Label, new UniqueLabel(defSym.Label));
                            }
                        }
                        mCurrentTable.AddOrReplace(defSym);

                        mRecentSymbols.Add(defSym);
                    }

                    //mCurrentTable.DebugDump(mNextLvtOffset);
                }

                // Update state to look for next table.
                mNextLvtIndex++;
                if (mNextLvtIndex < mLvTables.Keys.Count) {
                    mNextLvtOffset = mLvTables.Keys[mNextLvtIndex];
                } else {
                    mNextLvtOffset = mProject.FileDataLength;   // never reached
                }
            }
        }

        /// <summary>
        /// Generates a unique label for the duplicate remap table.
        /// </summary>
        private string DeDupLabel(string baseLabel) {
            string testLabel;
            int counter = 0;
            do {
                counter++;
                testLabel = baseLabel + "_DUP" + counter;   // make it ugly and obvious
            } while (mSymbolTable.TryGetNonVariableValue(testLabel, out Symbol unused));
            return testLabel;
        }
    }
}
