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

namespace SourceGen {
    /// <summary>
    /// Table of redefinable variables.  A project may have several of these, at different
    /// offsets.  The contents of later tables overwrite the contents of earlier tables.
    /// </summary>
    public class LocalVariableTable {
        /// <summary>
        /// List of variables.  The symbol's label must be unique within a table, so we sort
        /// on that.
        /// </summary>
        private SortedList<string, DefSymbol> mVariables;

        /// <summary>
        /// If set, all values from previous VariableTables should be discarded when this
        /// table is encountered.
        /// </summary>
        /// <remarks>
        /// Might be useful to allow addresses (DP ops) and constants (StackRel ops) to be
        /// cleared independently, but I suspect the typical compiled-language scenario will
        /// involve StackRel for args and a sliding DP for locals, so generally it makes
        /// sense to just clear both.
        /// </remarks>
        public bool ClearPrevious { get; set; }

        /// <summary>
        /// Indexer.
        /// </summary>
        /// <param name="key">Symbol's label.</param>
        /// <returns>Matching symbol.  Throws an exception if not found.</returns>
        public DefSymbol this[string key] {
            get {
                return mVariables[key];
            }
            set {
                mVariables[key] = value;
            }
        }

        /// <summary>
        /// Constructs an empty table.
        /// </summary>
        public LocalVariableTable() {
            mVariables = new SortedList<string, DefSymbol>();
        }
    }
}
