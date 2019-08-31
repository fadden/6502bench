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
        /// Table of symbols, used to ensure that all symbols are globally unique.  Only used
        /// when generating code for an assembler that doesn't support redefinable variables.
        /// </summary>
        private SymbolTable mSymbolTable;

        /// <summary>
        /// Label uniquification helper.
        /// </summary>
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
                } while (symbolTable.TryGetValue(testLabel, out Symbol unused1));
                Label = testLabel;
            }
        }
        private Dictionary<string, UniqueLabel> mUniqueLabels;

        /// <summary>
        /// Reference to project, so we can query the Anattrib array to identify "hidden" tables.
        /// </summary>
        private DisasmProject mProject;

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

        private int mNextLvtIndex;
        private int mNextLvtOffset;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lvTables">List of tables from the DisasmProject.</param>
        /// <param name="symbolTable">Full SymbolTable from the DisasmProject.  Used to
        ///   generate globally unique symbol names.  Pass null if uniqueness is not a
        ///   requirement.</param>
        /// <param name="project">Project reference.</param>
        public LocalVariableLookup(SortedList<int, LocalVariableTable> lvTables,
                SymbolTable symbolTable, DisasmProject project) {
            mLvTables = lvTables;
            mSymbolTable = symbolTable;
            mProject = project;

            mCurrentTable = new LocalVariableTable();
            if (mSymbolTable != null) {
                mUniqueLabels = new Dictionary<string, UniqueLabel>();
            }
            Reset();
        }

        public void Reset() {
            mRecentOffset = -1;
            mRecentSymbols = null;
            mCurrentTable.Clear();
            mUniqueLabels?.Clear();
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

            // The symRef uses the non-uniqified symbol, so we need to get the unique value at
            // the current offset.
            string label = symRef.Label;
            if (mUniqueLabels != null && mUniqueLabels.TryGetValue(label, out UniqueLabel ulab)) {
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
                        if (mSymbolTable != null) {
                            if (mUniqueLabels.TryGetValue(defSym.Label, out UniqueLabel ulab)) {
                                // We've seen this label before; generate a unique version.
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

                    mCurrentTable.DebugDump(mNextLvtOffset);
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
    }
}
