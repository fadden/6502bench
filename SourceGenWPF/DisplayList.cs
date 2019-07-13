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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;

namespace SourceGenWPF {
    /// <summary>
    /// List of items formatted for display.
    /// </summary>
    /// <remarks>
    /// This is intended to be useful as an ItemSource for a WPF ListView.  We need to implement
    /// plain IList to cause ListView to perform data virtualization, and the property/collection
    /// changed events so the view will pick up our changes.
    /// 
    /// The ItemsControl.ItemsSource property wants an IEnumerable (which IList implements).
    /// According to various articles, if the object implements IList, and the UI element
    /// is providing *UI* virtualization, you will also get *data* virtualization.  This behavior
    /// doesn't seem to be documented anywhere, but the consensus is that it's expected to work.
    /// 
    /// Implementing generic IList&lt;&gt; doesn't seem necessary for XAML, but may be useful
    /// for other consumers of the data.
    /// 
    /// The list is initially filled with null references, with FormattedParts instances
    /// generated on demand.  This is done by requesting individual items from the
    /// LineListGen object.
    /// 
    /// NOTE: it may or may not be possible to implement this trivially with an
    /// ObservableCollection.  At an earlier iteration it wasn't, and I'd like to keep this
    /// around even if it is now possible, in case the pendulum swings back the other way.
    ///
    /// Additional reading on data virtualization:
    ///  https://www.codeproject.com/Articles/34405/WPF-Data-Virtualization?msg=5635751
    ///  https://web.archive.org/web/20121216034305/http://www.zagstudio.com/blog/498
    ///  https://web.archive.org/web/20121107200359/http://www.zagstudio.com/blog/378
    /// </remarks>
    public class DisplayList : IList<DisplayList.FormattedParts>, IList,
            INotifyCollectionChanged, INotifyPropertyChanged {

        /// <summary>
        /// List of formatted parts.  DO NOT access this directly outside the event-sending
        /// method wrappers.
        /// </summary>
        private List<FormattedParts> mList;

        /// <summary>
        /// Data generation object.
        /// </summary>
        /// <remarks>
        /// This property is set by the LineListGen constructor.
        /// </remarks>
        public LineListGen ListGen { get; set; }

        /// <summary>
        /// Set of selected items, by list index.
        /// </summary>
        public DisplayListSelection SelectedIndices { get; private set; }


        /// <summary>
        /// Constructs an empty collection, with the default initial capacity.
        /// </summary>
        public DisplayList() {
            mList = new List<FormattedParts>();
            SelectedIndices = new DisplayListSelection();
        }


        #region Property / Collection Changed

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        // See ObservableCollection class, e.g.
        // https://github.com/Microsoft/referencesource/blob/master/System/compmod/system/collections/objectmodel/observablecollection.cs

        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e) {
            PropertyChanged?.Invoke(this, e);
        }

        private void OnPropertyChanged(string propertyName) {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            CollectionChanged?.Invoke(this, e);
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action,
                object item, int index) {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action,
                object item, int index, int oldIndex) {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action,
                object oldItem, object newItem, int index) {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset() {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion Property / Collection Changed



        #region IList / IList<T>
        public int Count => ((IList<FormattedParts>)mList).Count;

        public bool IsReadOnly => ((IList<FormattedParts>)mList).IsReadOnly;

        public bool IsFixedSize => ((IList)mList).IsFixedSize;

        public object SyncRoot => ((IList)mList).SyncRoot;

        public bool IsSynchronized => ((IList)mList).IsSynchronized;

        public void Add(FormattedParts item) {
            ((IList<FormattedParts>)mList).Add(item);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        public int Add(object value) {
            int posn = ((IList)mList).Add(value);
            if (posn >= 0) {
                OnPropertyChanged(CountString);
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, value, posn);
            }
            return posn;
        }

        public void Clear() {
            ((IList<FormattedParts>)mList).Clear();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();

            // Not strictly necessary, but does free up the memory sooner.
            SelectedIndices = new DisplayListSelection();
        }

        public bool Contains(FormattedParts item) {
            return ((IList<FormattedParts>)mList).Contains(item);
        }
        bool IList.Contains(object value) {
            return Contains((FormattedParts)value);
        }

        public void CopyTo(FormattedParts[] array, int arrayIndex) {
            ((IList<FormattedParts>)mList).CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index) {
            ((IList)mList).CopyTo(array, index);
        }

        public IEnumerator<FormattedParts> GetEnumerator() {
            // Use the indexer, rather than mList's enumerator, to get on-demand string gen.
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public int IndexOf(FormattedParts item) {
            return ((IList<FormattedParts>)mList).IndexOf(item);
        }
        int IList.IndexOf(object value) {
            return IndexOf((FormattedParts)value);
        }

        public void Insert(int index, FormattedParts item) {
            ((IList<FormattedParts>)mList).Insert(index, item);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }
        void IList.Insert(int index, object value) {
            Insert(index, (FormattedParts)value);
        }

        public void RemoveAt(int index) {
            FormattedParts removed = mList[index];
            ((IList<FormattedParts>)mList).RemoveAt(index);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removed, index);
        }

        public bool Remove(FormattedParts item) {
            // NotifyCollectionChangedAction.Remove wants an index.  We can find the index
            // of the first matching item and then do a RemoveAt, but this call just isn't
            // all that interesting for us, so it's easier to ignore it.
            //return ((IList<FormattedParts>)mList).Remove(item);
            throw new NotSupportedException();
        }

        void IList.Remove(object value) {
            //Remove((FormattedParts)value);
            throw new NotSupportedException();
        }

        object IList.this[int index] {
            // forward to generic impl
            get { return this[index]; }
            set { this[index] = (FormattedParts)value; }
        }

        // For IList<T>.
        public FormattedParts this[int index] {
            get {
                FormattedParts parts = mList[index];
                if (parts == null) {
                    parts = mList[index] = GetEntry(index);
                }
                return parts;
            }
            set {
                FormattedParts orig = mList[index];
                mList[index] = value;
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, orig, value, index);
            }
        }

        #endregion IList / IList<T>



        /// <summary>
        /// Retrieves the Nth element.
        /// </summary>
        private FormattedParts GetEntry(int index) {
            FormattedParts parts = mList[index];
            if (parts == null) {
                parts = mList[index] = ListGen.GetFormattedParts(index);
                parts.ListIndex = index;
            }
            return parts;
        }

        /// <summary>
        /// Resets the list, filling it with empty elements.  Also resets the selected indices.
        /// </summary>
        /// <param name="size">New size of the list.</param>
        public void ResetList(int size) {
            // TODO: can we recycle existing elements and just add/trim as needed?
            Clear();
            mList.Capacity = size;
            for (int i = 0; i < size; i++) {
                // add directly to list so we don't send events
                mList.Add(null);
            }

            SelectedIndices = new DisplayListSelection(size);

            // send one big notification at the end; "reset" means "forget everything you knew"
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        /// <summary>
        /// A range of lines has been replaced with a new range of lines.  The new set may be
        /// the same size, larger, or smaller than the previous.
        /// </summary>
        /// <param name="startIndex">Start index of change area.</param>
        /// <param name="oldCount">Number of old lines.</param>
        /// <param name="newCount">Number of new lines.  May be zero.</param>
        public void ClearListSegment(int startIndex, int oldCount, int newCount) {
            Debug.WriteLine("ClearListSegment start=" + startIndex + " old=" + oldCount +
                " new=" + newCount + " (mList.Count=" + mList.Count + ")");

            Debug.Assert(startIndex >= 0 && startIndex < mList.Count);
            Debug.Assert(oldCount > 0 && startIndex + oldCount < mList.Count);
            Debug.Assert(newCount >= 0);

            // Remove the old elements to clear them.
            mList.RemoveRange(startIndex, oldCount);
            // Replace with the appropriate number of null entries.
            for (int i = 0; i < newCount; i++) {
                mList.Insert(startIndex, null);
            }
            // TODO: can we null out existing entries, and just insert/remove when counts differ?

            if (oldCount != newCount) {
                SelectedIndices = new DisplayListSelection(mList.Count);
            }

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        /// <summary>
        /// List elements.  Instances are immutable.
        /// </summary>
        public class FormattedParts {
            public string Offset { get; private set; }
            public string Addr { get; private set; }
            public string Bytes { get; private set; }
            public string Flags { get; private set; }
            public string Attr { get; private set; }
            public string Label { get; private set; }
            public string Opcode { get; private set; }
            public string Operand { get; private set; }
            public string Comment { get; private set; }
            public bool IsLongComment { get; private set; }

            // Set to true if we want to highlight the address and label fields.
            public bool HasAddrLabelHighlight { get; private set; }

            public int ListIndex { get; set; } = -1;

            // Private constructor -- create instances with factory methods.
            private FormattedParts() { }

            /// <summary>
            /// Clones the specified object.
            /// </summary>
            private static FormattedParts Clone(FormattedParts orig) {
                FormattedParts newParts = FormattedParts.Create(orig.Offset, orig.Addr,
                    orig.Bytes, orig.Flags, orig.Attr, orig.Label, orig.Opcode, orig.Operand,
                    orig.Comment);

                newParts.IsLongComment = orig.IsLongComment;
                newParts.HasAddrLabelHighlight = orig.HasAddrLabelHighlight;

                newParts.ListIndex = orig.ListIndex;
                return newParts;
            }

            public static FormattedParts Create(string offset, string addr, string bytes,
                    string flags, string attr, string label, string opcode, string operand,
                    string comment) {
                FormattedParts parts = new FormattedParts();
                parts.Offset = offset;
                parts.Addr = addr;
                parts.Bytes = bytes;
                parts.Flags = flags;
                parts.Attr = attr;
                parts.Label = label;
                parts.Opcode = opcode;
                parts.Operand = operand;
                parts.Comment = comment;
                parts.IsLongComment = false;

                return parts;
            }

            public static FormattedParts CreateBlankLine() {
                FormattedParts parts = new FormattedParts();
                return parts;
            }

            public static FormattedParts CreateLongComment(string comment) {
                FormattedParts parts = new FormattedParts();
                parts.Comment = comment;
                parts.IsLongComment = true;
                return parts;
            }

            public static FormattedParts CreateDirective(string opstr, string addrStr) {
                FormattedParts parts = new FormattedParts();
                parts.Opcode = opstr;
                parts.Operand = addrStr;
                return parts;
            }

            public static FormattedParts CreateEquDirective(string label, string opstr,
                    string addrStr, string comment) {
                FormattedParts parts = new FormattedParts();
                parts.Label = label;
                parts.Opcode = opstr;
                parts.Operand = addrStr;
                parts.Comment = comment;
                return parts;
            }

            public static FormattedParts AddSelectionHighlight(FormattedParts orig) {
                FormattedParts newParts = Clone(orig);
                newParts.HasAddrLabelHighlight = true;
                return newParts;
            }

            public static FormattedParts RemoveSelectionHighlight(FormattedParts orig) {
                FormattedParts newParts = Clone(orig);
                newParts.HasAddrLabelHighlight = false;
                return newParts;
            }
        }
    }
}
