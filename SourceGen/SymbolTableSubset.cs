/*
 * Copyright 2018 faddenSoft
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
using System.Linq;
using System.Text;

namespace SourceGen {
    public class SymbolTableSubset {
        private SymbolTable mSymbolTable;

        // Internal dirty flag.
        private bool mIsDirty = true;

        // Modification serial number, incremented on every change.
        private int mLastTableSerial;

        /// <summary>
        /// List of items, filtered and sorted according to user preference.  This is the
        /// backing store for the symbol ListView.
        /// </summary>
        private List<Symbol> mSortedSubset = new List<Symbol>();


        public SymbolTableSubset(SymbolTable table) {
            mSymbolTable = table;
            mLastTableSerial = mSymbolTable.ChangeSerial;

            // Configure from Settings object, which is kept up to date with the UI state.
            AppSettings settings = AppSettings.Global;
            IncludeUserLabels = settings.GetBool(AppSettings.SYMWIN_SHOW_USER, false);
            IncludeAutoLabels = settings.GetBool(AppSettings.SYMWIN_SHOW_AUTO, false);
            IncludeProjectSymbols = settings.GetBool(AppSettings.SYMWIN_SHOW_PROJECT, false);
            IncludePlatformSymbols = settings.GetBool(AppSettings.SYMWIN_SHOW_PLATFORM, false);
            IncludeConstants = settings.GetBool(AppSettings.SYMWIN_SHOW_CONST, false);
            IncludeAddresses = settings.GetBool(AppSettings.SYMWIN_SHOW_ADDR, false);

            SortAscending = settings.GetBool(AppSettings.SYMWIN_SORT_ASCENDING, false);
            int col = settings.GetInt(AppSettings.SYMWIN_SORT_COL, 0);
            if (col < 0 || col > 2) {
                col = 0;
            }
            SortColumn = (SortCol)col;
        }

        // Filter: include user-generated labels?
        private bool mIncludeUserLabels = true;
        public bool IncludeUserLabels {
            get {
                return mIncludeUserLabels;
            }
            set {
                if (mIncludeUserLabels != value) {
                    mIncludeUserLabels = value;
                    AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_USER, value);
                    mIsDirty = true;
                }
            }
        }
        // Filter: include auto-generated labels?
        private bool mIncludeAutoLabels = true;
        public bool IncludeAutoLabels {
            get {
                return mIncludeAutoLabels;
            }
            set {
                if (mIncludeAutoLabels != value) {
                    mIncludeAutoLabels = value;
                    AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_AUTO, value);
                    mIsDirty = true;
                }
            }
        }
        // Filter: include symbols from project configuration file?
        private bool mIncludeProjectSymbols = true;
        public bool IncludeProjectSymbols {
            get {
                return mIncludeProjectSymbols;
            }
            set {
                if (mIncludeProjectSymbols != value) {
                    mIncludeProjectSymbols = value;
                    AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_PROJECT, value);
                    mIsDirty = true;
                }
            }
        }
        // Filter: include symbols from platform definition files?
        private bool mIncludePlatformSymbols = true;
        public bool IncludePlatformSymbols {
            get {
                return mIncludePlatformSymbols;
            }
            set {
                if (mIncludePlatformSymbols != value) {
                    mIncludePlatformSymbols = value;
                    AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_PLATFORM, value);
                    mIsDirty = true;
                }
            }
        }
        // Filter: include constants?
        private bool mIncludeConstants = true;
        public bool IncludeConstants {
            get {
                return mIncludeConstants;
            }
            set {
                if (mIncludeConstants != value) {
                    mIncludeConstants = value;
                    AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_CONST, value);
                    mIsDirty = true;
                }
            }
        }
        // Filter: include addresses?
        private bool mIncludeAddresses = true;
        public bool IncludeAddresses {
            get {
                return mIncludeAddresses;
            }
            set {
                if (mIncludeAddresses != value) {
                    mIncludeAddresses = value;
                    AppSettings.Global.SetBool(AppSettings.SYMWIN_SHOW_ADDR, value);
                    mIsDirty = true;
                }
            }
        }
        // Sort: ascending?
        private bool mSortAscending = true;
        public bool SortAscending {
            get {
                return mSortAscending;
            }
            set {
                if (mSortAscending != value) {
                    mSortAscending = value;
                    AppSettings.Global.SetBool(AppSettings.SYMWIN_SORT_ASCENDING, value);
                    mIsDirty = true;
                }
            }
        }
        // Sort: which column?  Note we store the int value in the app settings.
        public enum SortCol { Type = 0, Name = 1, Value = 2 };
        private SortCol mSortColumn = SortCol.Name;
        public SortCol SortColumn {
            get {
                return mSortColumn;
            }
            set {
                if (mSortColumn != value) {
                    mSortColumn = value;
                    AppSettings.Global.SetInt(AppSettings.SYMWIN_SORT_COL, (int)value);
                    mIsDirty = true;
                }
            }
        }


        /// <summary>
        /// Returns the length of the symbol subset list.  As a side-effect this may
        /// re-sort the list.
        /// </summary>
        public int GetSubsetCount() {
            if (mIsDirty || mLastTableSerial != mSymbolTable.ChangeSerial) {
                mLastTableSerial = mSymbolTable.ChangeSerial;
                mSortedSubset.Clear();
                foreach (Symbol sym in mSymbolTable) {
                    if (!IncludeUserLabels && sym.SymbolSource == Symbol.Source.User) {
                        continue;
                    }
                    if (!IncludeProjectSymbols && sym.SymbolSource == Symbol.Source.Project) {
                        continue;
                    }
                    if (!IncludePlatformSymbols && sym.SymbolSource == Symbol.Source.Platform) {
                        continue;
                    }
                    if (!IncludeAutoLabels && sym.SymbolSource == Symbol.Source.Auto) {
                        continue;
                    }
                    if (!IncludeConstants && sym.SymbolType == Symbol.Type.Constant) {
                        continue;
                    }
                    if (!IncludeAddresses && sym.SymbolType != Symbol.Type.Constant) {
                        continue;
                    }
                    mSortedSubset.Add(sym);
                }

                // Sort.  Label is always unique, so we use it as a secondary sort.
                if (SortColumn == SortCol.Type) {
                    if (mSortAscending) {
                        mSortedSubset.Sort(delegate (Symbol a, Symbol b) {
                            int cmp = string.Compare(a.SourceTypeString, b.SourceTypeString);
                            if (cmp == 0) {
                                cmp = string.Compare(a.Label, b.Label);
                            }
                            return cmp;
                        });
                    } else {
                        mSortedSubset.Sort(delegate (Symbol a, Symbol b) {
                            int cmp = string.Compare(a.SourceTypeString, b.SourceTypeString);
                            if (cmp == 0) {
                                // secondary sort is always ascending, so negate
                                cmp = -string.Compare(a.Label, b.Label);
                            }
                            return -cmp;
                        });
                    }
                } else if (SortColumn == SortCol.Name) {
                    if (mSortAscending) {
                        mSortedSubset.Sort(delegate (Symbol a, Symbol b) {
                            return string.Compare(a.Label, b.Label);
                        });
                    } else {
                        mSortedSubset.Sort(delegate (Symbol a, Symbol b) {
                            return -string.Compare(a.Label, b.Label);
                        });
                    }
                } else if (SortColumn == SortCol.Value) {
                    if (mSortAscending) {
                        mSortedSubset.Sort(delegate (Symbol a, Symbol b) {
                            int cmp;
                            if (a.Value < b.Value) {
                                cmp = -1;
                            } else if (a.Value > b.Value) {
                                cmp = 1;
                            } else {
                                cmp = string.Compare(a.Label, b.Label);
                            }
                            return cmp;
                        });
                    } else {
                        mSortedSubset.Sort(delegate (Symbol a, Symbol b) {
                            int cmp;
                            if (a.Value < b.Value) {
                                cmp = -1;
                            } else if (a.Value > b.Value) {
                                cmp = 1;
                            } else {
                                cmp = -string.Compare(a.Label, b.Label);
                            }
                            return -cmp;
                        });
                    }
                }

                mIsDirty = false;
            }
            return mSortedSubset.Count;
        }

        /// <summary>
        /// Returns an item from the subset list.
        /// </summary>
        public Symbol GetSubsetItem(int index) {
            Debug.Assert(!mIsDirty);
            return mSortedSubset[index];
        }
    }
}
