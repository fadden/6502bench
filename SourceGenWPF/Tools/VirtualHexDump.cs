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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Asm65;

namespace SourceGenWPF.Tools {
    /// <summary>
    /// Generates formatted hex dump lines, and makes them available as a list.  The result
    /// is suitable for use with WPF ItemsSource.  Items are generated on demand, providing
    /// data virtualization.
    /// </summary>
    /// <remarks>
    /// Doing proper data virtualization in WPF is tricky and annoying.  We just render on
    /// demand and retain the results indefinitely.
    /// </remarks>
    public class VirtualHexDump : IList, INotifyCollectionChanged, INotifyPropertyChanged {
        /// <summary>
        /// <summary>
        /// Data to display.  We currently require that the entire file fit in memory,
        /// which is reasonable because we impose a 2^24 (16MB) limit.
        /// </summary>
        private byte[] mData;

        /// <summary>
        /// Data formatter object.
        /// </summary>
        private Formatter mFormatter;

        private string[] mLines;

        // Tracks the number of lines we've generated, so we can see if virtualization is
        // actually happening.
        private int mDebugGenLineCount;


        public VirtualHexDump(byte[] data, Formatter formatter) {
            mData = data;
            mFormatter = formatter;

            Count = (mData.Length + 15) / 16;
            mLines = new string[Count];
            mDebugGenLineCount = 0;
        }

        /// <summary>
        /// Causes all lines to be reformatted.  Call this after changing the desired format.
        /// Generates a collection-reset event, which may cause loss of selection.
        /// </summary>
        public void Reformat(Formatter newFormat) {
            mFormatter = newFormat;

            for (int i = 0; i < mLines.Length; i++) {
                mLines[i] = null;
            }
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
        }

        /// <summary>
        /// Returns the Nth line, generating it if it hasn't been yet.
        /// </summary>
        private string GetLine(int index) {
            if (mLines[index] == null) {
                mLines[index] = mFormatter.FormatHexDump(mData, index * 16);

                if ((++mDebugGenLineCount % 1000) == 0) {
                    //Debug.WriteLine("DebugGenLineCount: " + mDebugGenLineCount);
                }
            }
            //Debug.WriteLine("GET LINE " + index + ": " + mLines[index]);
            return mLines[index];
        }


        #region Property / Collection Changed

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

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

        private void OnCollectionReset() {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion Property / Collection Changed

        #region IList

        public bool IsReadOnly => true;

        public bool IsFixedSize => true;

        public int Count { get; private set; }

        public object SyncRoot => throw new NotImplementedException();

        public bool IsSynchronized => false;

        public object this[int index] {
            get => GetLine(index);
            set => throw new NotImplementedException();
        }

        public int Add(object value) {
            throw new NotImplementedException();
        }

        public bool Contains(object value) {
            return (IndexOf(value) >= 0);
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public int IndexOf(object value) {
            //Debug.WriteLine("VHD IndexOf " + value);
            // This gets called sometimes when the selection changes, because the selection
            // mechanism tracks objects rather than indices.  Fortunately we can convert the
            // value string to an index by parsing the first six characters.  (This is
            // somewhat fragile as it relies on the way Formatter formats the string.  Might
            // want to make offset-from-hexdump-string a Formatter method.)
            int offset = Convert.ToInt32(((string)value).Substring(0, 6), 16);
            int index = offset / 16;

            // Either the object at the target location matches, or it doesn't; no need to
            // search.  We'll get requests for nonexistent objects after we reformat the
            // collection, when the list control tries to find the selected items.
            //
            // Object equality is what's desired; no need for string comparison
            if ((object)mLines[index] == value) {
                return index;
            }
            //Debug.WriteLine("    IndexOf not found");
            return -1;
        }

        public void Insert(int index, object value) {
            throw new NotImplementedException();
        }

        public void Remove(object value) {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator() {
            // Use the indexer, rather than mLines's enumerator, to get on-demand string gen.
            for (int i = 0; i < mLines.Length; i++) {
                yield return this[i];
            }
        }

        #endregion IList
    }
}
