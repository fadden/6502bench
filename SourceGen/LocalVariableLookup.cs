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
            Reset();
        }

        public void Reset() {
            mRecentOffset = -1;
            mRecentSymbols = null;
            mCurrentTable.Clear();
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
        /// Gets the symbol associated with a symbol reference.
        /// </summary>
        /// <param name="offset">Offset of start of instruction.</param>
        /// <param name="symRef">Reference to symbol.</param>
        /// <returns>Symbol, or null if no match found.</returns>
        public DefSymbol GetSymbol(int offset, WeakSymbolRef symRef) {
            AdvanceToOffset(offset);

            return mCurrentTable.GetByLabel(symRef.Label);
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
        /// <param name="offset">Target offset.</param>
        private void AdvanceToOffset(int offset) {
            if (mNextLvtIndex < 0) {
                return;
            }
            if (offset < mRecentOffset) {
                // We went backwards.
                Reset();
            }
            while (mNextLvtOffset <= offset) {
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
                    // discards entries that clash.
                    for (int i = 0; i < lvt.Count; i++) {
                        // TODO: uniquify
                        mCurrentTable.AddOrReplace(lvt[i]);

                        mRecentSymbols.Add(lvt[i]);
                    }

                    mCurrentTable.DebugDump();
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
