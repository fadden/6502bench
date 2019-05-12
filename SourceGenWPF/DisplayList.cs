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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// is providing UI virtualization, you will also get data virtualization.  This behavior
    /// doesn't seem to be documented anywhere, but the consensus is that it's expected to work.
    /// 
    /// Implementing generic IList doesn't seem necessary for XAML, but is useful for other
    /// customers of the data (e.g. the assembler source generator).
    /// </remarks>
    public class DisplayList : IList<DisplayList.FormattedParts>, IList,
            INotifyCollectionChanged, INotifyPropertyChanged {

        // TODO: check VirtualizingStackPanel.VirtualizationMode == recycling (page 259)

        /// <summary>
        /// List of formatted parts.  The idea is that the list is initially populated with
        /// null references, and FormattedParts objects are generated on demand.
        /// </summary>
        private List<FormattedParts> mList;

        /// <summary>
        /// Constructs an empty collection, with the default initial capacity.
        /// </summary>
        public DisplayList() {
            mList = new List<FormattedParts>();
        }

        public DisplayList(int count) {
            mList = new List<FormattedParts>(count);
            for (int i = 0; i < count; i++) {
                mList.Add(null);
            }
        }



        #region Property / Collection Changed

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        // See ObservableCollection class, e.g.
        // https://github.com/Microsoft/referencesource/blob/master/System/compmod/system/collections/objectmodel/observablecollection.cs

        private const string CountString = "Count";
        private const string IndexerName = "Item[]";

#if false
        protected override void ClearItems() {
            base.ClearItems();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        protected override void RemoveItem(int index) {
            FormattedParts removedItem = this[index];

            base.RemoveItem(index);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, removedItem, index);
        }

        protected override void InsertItem(int index, FormattedParts item) {
            base.InsertItem(index, item);

            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        protected override void SetItem(int index, FormattedParts item) {
            FormattedParts originalItem = this[index];
            base.SetItem(index, item);

            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Replace, originalItem, item, index);
        }
#endif

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
            Debug.WriteLine("GEN " + index);
            if ((index % 10) != 0) {
                return FormattedParts.Create("off" + index, "addr" + index, "12 34",
                    "vncidmx", "", "yup:", "LDA", "$1234", "a & b");
            } else {
                return FormattedParts.Create("offN This is a long comment line");
            }
        }

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
            public bool SingleLine { get; private set; }

            // Construct with factory methods.
            private FormattedParts() { }

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
                parts.SingleLine = false;

                return parts;
            }

            public static FormattedParts Create(string longComment) {
                FormattedParts parts = new FormattedParts();
                parts.Comment = longComment;
                parts.SingleLine = true;

                return parts;
            }
        }
    }
}
