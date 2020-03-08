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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SourceGen {
    /// <summary>
    /// A visualization with animated contents.
    /// </summary>
    /// <remarks>
    /// This has a list of references to Visualization objects.  The cached images are
    /// displayed in sequence, at a rate determined by animation parameters also held here.
    ///
    /// As with the base class, instances are generally immutable for the benefit of undo/redo.
    /// </remarks>
    public class VisBitmapAnimation : Visualization {
        /// <summary>
        /// Frame delay parameter.
        /// </summary>
        public const string P_FRAME_DELAY_MSEC_PARAM = "_frame-delay-msec";
        public const string P_FRAME_DELAY_MSEC_PARAM_OLD = "frame-delay-msec";

        /// <summary>
        /// Fake visualization generation identifier.
        /// </summary>
        public const string ANIM_VIS_GEN = "(animation)";

        /// <summary>
        /// Serial numbers of visualizations, e.g. bitmap frames.
        /// </summary>
        /// <remarks>
        /// We don't reference the Visualization objects directly because they might get
        /// edited (e.g. the tag gets renamed), which replaces them with a new object with
        /// the same serial number.  We don't do things like renames in place because that
        /// makes undo/redo harder.
        ///
        /// (We could reference the Visualization objects and then do a serial number lookup
        /// before using it.  Some opportunities for optimization should the need arise.  This
        /// might also allow us to avoid exposing the serial number as a public property, though
        /// there's not much advantage to that.)
        /// </remarks>
        private List<int> mSerialNumbers;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tag">Unique identifier.</param>
        /// <param name="visGenIdent">Visualization generator identifier.</param>
        /// <param name="visGenParams">Parameters for visualization generator.</param>
        /// <param name="visSerialNumbers">Serial numbers of referenced Visualizations.</param>
        public VisBitmapAnimation(string tag, string visGenIdent,
                ReadOnlyDictionary<string, object> visGenParams, VisBitmapAnimation oldObj,
                List<int> visSerialNumbers)
                : base(tag, visGenIdent, visGenParams, oldObj) {
            Debug.Assert(visSerialNumbers != null);

            // Make a copy of the list.
            mSerialNumbers = new List<int>(visSerialNumbers.Count);
            foreach (int serial in visSerialNumbers) {
                mSerialNumbers.Add(serial);
            }

            CachedImage = BLANK_IMAGE;
            OverlayImage = ANIM_OVERLAY_IMAGE;
        }

        /// <summary>
        /// Generates a cached image for the animation.
        /// </summary>
        /// <remarks>
        /// Currently just using the first frame.  We could do fancy things, like make a
        /// poster with the first N images.
        /// </remarks>
        /// <param name="visSets">List of visualization sets.</param>
        public void GenerateImage(SortedList<int, VisualizationSet> visSets) {
            CachedImage = BLANK_IMAGE;
            if (mSerialNumbers.Count == 0) {
                return;
            }
            Visualization vis = VisualizationSet.FindVisualizationBySerial(visSets,
                mSerialNumbers[0]);
            if (vis != null) {
                CachedImage = vis.CachedImage;
            }
        }

        /// <summary>
        /// The number of Visualizations linked from this animation.
        /// </summary>
        /// <remarks>
        /// Visualizations may appear more than once in the list.  Each instance is counted.
        /// </remarks>
        public int Count {
            get { return mSerialNumbers.Count; }
        }

        /// <summary>
        /// Indexes the serial number list.
        /// </summary>
        public int this[int index] {
            get {
                return mSerialNumbers[index];
            }
        }

        /// <summary>
        /// Returns true if this visualization holds a reference to the specified serial number.
        /// </summary>
        public bool ContainsSerial(int serial) {
            // Linear search.  We don't do this a lot and our lists our short, so okay for now.
            foreach (int ser in mSerialNumbers) {
                if (ser == serial) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Strips serial numbers out of the list.
        /// </summary>
        /// <param name="visAnim">Object to strip serial numbers from.</param>
        /// <param name="removedSerials">List of serial numbers to remove.</param>
        /// <param name="newAnim">Object with changes, or null if nothing changed.</param>
        /// <returns>True if something was actually removed.</returns>
        public static bool StripEntries(VisBitmapAnimation visAnim, List<int> removedSerials,
                out VisBitmapAnimation newAnim) {
            bool somethingRemoved = false;

            // Both sets should be small, so not worried about O(m*n).
            List<int> newSerials = new List<int>(visAnim.mSerialNumbers.Count);
            foreach (int serial in visAnim.mSerialNumbers) {
                if (removedSerials.Contains(serial)) {
                    Debug.WriteLine("Removing serial #" + serial + " from " + visAnim.Tag);
                    somethingRemoved = true;
                    continue;
                }
                newSerials.Add(serial);
            }

            if (somethingRemoved) {
                newAnim = new VisBitmapAnimation(visAnim.Tag, visAnim.VisGenIdent,
                    visAnim.VisGenParams, visAnim, newSerials);
            } else {
                newAnim = null;
            }
            return somethingRemoved;
        }


        public static bool operator ==(VisBitmapAnimation a, VisBitmapAnimation b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            return a.Equals(b);
        }
        public static bool operator !=(VisBitmapAnimation a, VisBitmapAnimation b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            if (!(obj is VisBitmapAnimation)) {
                return false;
            }
            // Do base-class equality comparison and the ReferenceEquals check.
            if (!base.Equals(obj)) {
                return false;
            }
            VisBitmapAnimation other = (VisBitmapAnimation)obj;
            if (other.mSerialNumbers.Count != mSerialNumbers.Count) {
                return false;
            }
            for (int i = 0; i < mSerialNumbers.Count; i++) {
                if (other.mSerialNumbers[i] != mSerialNumbers[i]) {
                    return false;
                }
            }
            return true;
        }
        public override int GetHashCode() {
            return base.GetHashCode() ^ mSerialNumbers.Count;   // weak
        }
    }
}
