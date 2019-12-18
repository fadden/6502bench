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

namespace SourceGen {
    /// <summary>
    /// A visualization with animated contents.
    /// </summary>
    /// <remarks>
    /// References to Visualization objects (such as a 3D mesh or list of bitmaps) are held
    /// here.  The VisGenParams property holds animation properties, such as frame rate and
    /// view angles.
    /// </remarks>
    public class VisualizationAnimation : Visualization {
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
        public VisualizationAnimation(string tag, string visGenIdent,
                ReadOnlyDictionary<string, object> visGenParams, List<int> visSerialNumbers)
                : base(tag, visGenIdent, visGenParams) {
            Debug.Assert(visSerialNumbers != null);

            mSerialNumbers = visSerialNumbers;
        }
    }
}
