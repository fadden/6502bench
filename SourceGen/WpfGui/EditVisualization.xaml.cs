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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Asm65;
using PluginCommon;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Visualization editor.
    /// </summary>
    public partial class EditVisualization : Window, INotifyPropertyChanged {
        private DisasmProject mProject;
        private Formatter mFormatter;
        private Visualization mOrigVis;

        public string TagString {
            get { return mTagString; }
            set { mTagString = value; OnPropertyChanged(); }
        }
        private string mTagString;

        public IList<ParameterValue> ParameterList {
            get { return mParameterList; }
        }
        private List<ParameterValue> mParameterList;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditVisualization(Window owner, DisasmProject proj, Formatter formatter,
                Visualization vis) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = proj;
            mFormatter = formatter;
            mOrigVis = vis;

            // TODO: configure ComboBox from vis arg if non-null, then use current
            //   combo box selection, updating in selchange event
            string visGenName = "apple2-hi-res-bitmap";

            mParameterList = new List<ParameterValue>();
            GenerateParamControls(visGenName);
        }

        /// <summary>
        /// Generates the list of parameter controls.
        /// </summary>
        /// <remarks>
        /// We need to get the list of parameters from the VisGen plugin, then for each
        /// parameter we need to merge the value from the Visualization's value list.
        /// If we don't find a corresponding entry in the Visualization, we use the
        /// default value.
        /// </remarks>
        private void GenerateParamControls(string visGenName) {
            IPlugin_Visualizer2d plugin =
                Visualization.FindPluginByVisGenName(mProject, visGenName);
            List<VisParamDescr> descrs = plugin.GetVisGenParams(visGenName);

            mParameterList.Clear();
            foreach (VisParamDescr vpd in descrs) {
                string rangeStr = string.Empty;
                object defaultVal = vpd.DefaultValue;
                if (mOrigVis.VisGenParams.TryGetValue(vpd.Name, out object val)) {
                    // Do we need to confirm that val has the correct type?
                    defaultVal = val;
                }

                if (vpd.CsType == typeof(int) || vpd.CsType == typeof(float)) {
                    if (vpd.Special == VisParamDescr.SpecialMode.Offset) {
                        defaultVal = mFormatter.FormatOffset24((int)defaultVal);
                        rangeStr = "[" + mFormatter.FormatOffset24(0) + "," +
                            mFormatter.FormatOffset24(mProject.FileDataLength - 1) + "]";
                    } else {
                        rangeStr = "[" + vpd.Min + "," + vpd.Max + "]";
                    }
                }

                ParameterValue pv = new ParameterValue(vpd.UiLabel, vpd.Name, vpd.CsType,
                    defaultVal, rangeStr);

                mParameterList.Add(pv);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("PARAMS:");
            foreach (ParameterValue val in mParameterList) {
                Debug.WriteLine("  " + val.Name + ": " + val.Value +
                    " (" + val.Value.GetType() + ")");
            }
            DialogResult = false;       // TODO
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox src = (TextBox)sender;
            ParameterValue pv = (ParameterValue)src.DataContext;
            Debug.WriteLine("TEXT CHANGE " + pv + ": " + src.Text);
        }
    }

    /// <summary>
    /// Describes a parameter and holds its value while being edited by WPF.
    /// </summary>
    /// <remarks>
    /// We use an explicit type so that we can format the initial value as hex or whatever.
    /// </remarks>
    public class ParameterValue {
        public string UiName { get; private set; }
        public string Name { get; private set; }
        public Type CsType { get; private set; }
        public object Value { get; set; }
        public string RangeText { get; private set; }

        public ParameterValue(string uiName, string name, Type csType, object val,
                string rangeText) {
            UiName = uiName;
            Name = name;
            CsType = csType;
            Value = val;
            RangeText = rangeText;
        }

        public override string ToString() {
            return "[PV: " + Name + "=" + Value + "]";
        }
    }

    public class ParameterTemplateSelector : DataTemplateSelector {
        private DataTemplate mBoolTemplate;
        public DataTemplate BoolTemplate {
            get { return mBoolTemplate; }
            set { mBoolTemplate = value; }
        }
        private DataTemplate mIntTemplate;
        public DataTemplate IntTemplate {
            get { return mIntTemplate; }
            set { mIntTemplate = value; }
        }
        private DataTemplate mFloatTemplate;
        public DataTemplate FloatTemplate {
            get { return mFloatTemplate; }
            set { mFloatTemplate = value; }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if (item is ParameterValue) {
                ParameterValue parm = (ParameterValue)item;
                if (parm.CsType == typeof(bool)) {
                    return BoolTemplate;
                } else if (parm.CsType == typeof(int)) {
                    return IntTemplate;
                } else if (parm.CsType == typeof(float)) {
                    return FloatTemplate;
                } else {
                    Debug.WriteLine("WHA?" + parm.Value.GetType());
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
