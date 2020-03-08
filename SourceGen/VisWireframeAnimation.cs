/*
 * Copyright 2020 faddenSoft
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

using PluginCommon;

namespace SourceGen {
    /// <summary>
    /// A wireframe visualization with animation.
    /// </summary>
    /// <remarks>
    /// All of the animation parameters get added to the Visualization parameter set, so this
    /// is just a place to hold the IVisualizationWireframe reference and some constants.
    /// </remarks>
    public class VisWireframeAnimation : Visualization {
        /// <summary>
        /// Frame delay parameter.
        /// </summary>
        public const string P_FRAME_DELAY_MSEC = "_frame-delay-msec";

        /// <summary>
        /// Frame count parameter.
        /// </summary>
        public const string P_FRAME_COUNT = "_frame-count";

        public const string P_EULER_ROT_X = "_eulerRotX";
        public const string P_EULER_ROT_Y = "_eulerRotY";
        public const string P_EULER_ROT_Z = "_eulerRotZ";

        private IVisualizationWireframe mVisWire;

        public VisWireframeAnimation(string tag, string visGenIdent,
                ReadOnlyDictionary<string, object> visGenParams, Visualization oldObj,
                IVisualizationWireframe visWire)
                : base(tag, visGenIdent, visGenParams, oldObj) {
            Debug.Assert(visWire != null);

            mVisWire = visWire;
        }
    }
}
