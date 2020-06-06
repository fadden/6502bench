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
using System.Collections.ObjectModel;

using PluginCommon;

namespace RuntimeData.Test2004 {
    public class VisTest2004 : MarshalByRefObject, IPlugin, IPlugin_Visualizer {
        // IPlugin
        public string Identifier {
            get { return "Test2004 Dummy Visualizer"; }
        }

        // Visualization identifiers; DO NOT change or projects that use them will break.
        private const string VIS_DUMMY = "dummy";

        // Visualization descriptors.
        private VisDescr[] mDescriptors = new VisDescr[] {
            new VisDescr(VIS_DUMMY, "Dummy", VisDescr.VisType.Bitmap,
                new VisParamDescr[] { })
        };


        // IPlugin
        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) { }

        // IPlugin
        public void Unprepare() { }

        // IPlugin_Visualizer
        public VisDescr[] GetVisGenDescrs() {
            return mDescriptors;
        }

        // IPlugin_Visualizer
        public IVisualization2d Generate2d(VisDescr descr,
                ReadOnlyDictionary<string, object> parms) {
            VisBitmap8 vb = new VisBitmap8(1, 1);
            vb.AddColor(0, 0, 0, 0);
            vb.SetPixelIndex(0, 0, 0);
            return vb;
        }
    }
}
