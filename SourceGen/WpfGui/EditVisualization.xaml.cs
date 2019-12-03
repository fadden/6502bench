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
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Asm65;
using PluginCommon;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Visualization editor.
    /// </summary>
    public partial class EditVisualization : Window, INotifyPropertyChanged {
        /// <summary>
        /// Dialog result.
        /// </summary>
        public Visualization NewVis { get; private set; }

        private DisasmProject mProject;
        private Formatter mFormatter;
        private int mSetOffset;
        private Visualization mOrigVis;

        private Brush mDefaultLabelColor = SystemColors.WindowTextBrush;
        private Brush mErrorLabelColor = Brushes.Red;


        /// <summary>
        /// True if all properties are in valid ranges.  Determines whether the OK button
        /// is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        /// <summary>
        /// Visualization tag.
        /// </summary>
        public string TagString {
            get { return mTagString; }
            set { mTagString = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mTagString;

        public Brush TagLabelBrush {
            get { return mTagLabelBrush; }
            set { mTagLabelBrush = value; OnPropertyChanged(); }
        }
        private Brush mTagLabelBrush;

        /// <summary>
        /// Item for combo box.
        /// </summary>
        /// <remarks>
        /// Strictly speaking we could just create an ItemsSource from VisDescr objects, but
        /// the plugin reference saves a lookup later.
        /// </remarks>
        public class VisualizationItem {
            public IPlugin_Visualizer Plugin { get; private set; }
            public VisDescr VisDescriptor { get; private set; }
            public VisualizationItem(IPlugin_Visualizer plugin, VisDescr descr) {
                Plugin = plugin;
                VisDescriptor = descr;
            }
        }

        /// <summary>
        /// List of visualizers, for combo box.
        /// </summary>
        public List<VisualizationItem> VisualizationList { get; private set; }

        /// <summary>
        /// Error message, shown in red.
        /// </summary>
        public string PluginErrMessage {
            get { return mPluginErrMessage; }
            set { mPluginErrMessage = value; OnPropertyChanged(); }
        }
        private string mPluginErrMessage = string.Empty;

        /// <summary>
        /// Set by the plugin callback.  WPF doesn't like it when we try to fire off a
        /// property changed event from here.
        /// </summary>
        public string LastPluginMessage { get; set; }

        /// <summary>
        /// ItemsSource for the ItemsControl with the generated parameter controls.
        /// </summary>
        public ObservableCollection<ParameterValue> ParameterList { get; private set; } =
            new ObservableCollection<ParameterValue>();

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private class ScriptSupport : MarshalByRefObject, PluginCommon.IApplication {
            private EditVisualization mOuter;

            public ScriptSupport(EditVisualization outer) {
                mOuter = outer;
            }

            public void DebugLog(string msg) {
                Debug.WriteLine("Vis plugin: " + msg);
                mOuter.LastPluginMessage = msg;
            }

            public bool SetOperandFormat(int offset, DataSubType subType, string label) {
                throw new InvalidOperationException();
            }
            public bool SetInlineDataFormat(int offset, int length, DataType type,
                    DataSubType subType, string label) {
                throw new InvalidOperationException();
            }
        }
        private ScriptSupport mScriptSupport;

        /// <summary>
        /// Image we show when we fail to generate a visualization.
        /// </summary>
        private static BitmapImage sBadParamsImage = Visualization.BROKEN_IMAGE;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Owner window.</param>
        /// <param name="proj">Project reference.</param>
        /// <param name="formatter">Text formatter.</param>
        /// <param name="vis">Visualization to edit, or null if this is new.</param>
        public EditVisualization(Window owner, DisasmProject proj, Formatter formatter,
                int setOffset, Visualization vis) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = proj;
            mFormatter = formatter;
            mSetOffset = setOffset;
            mOrigVis = vis;

            mScriptSupport = new ScriptSupport(this);

            if (vis != null) {
                TagString = vis.Tag;
            } else {
                // Could make this unique, but probably not worth the bother.
                TagString = "vis" + mSetOffset.ToString("x6");
            }

            int visSelection = 0;
            VisualizationList = new List<VisualizationItem>();
            List<IPlugin> plugins = proj.GetActivePlugins();
            foreach (IPlugin chkPlug in plugins) {
                if (!(chkPlug is IPlugin_Visualizer)) {
                    continue;
                }
                IPlugin_Visualizer vplug = (IPlugin_Visualizer)chkPlug;
                foreach (VisDescr descr in vplug.GetVisGenDescrs()) {
                    if (vis != null && vis.VisGenIdent == descr.Ident) {
                        visSelection = VisualizationList.Count;
                    }
                    VisualizationList.Add(new VisualizationItem(vplug, descr));
                }
            }

            // Set the selection.  This should cause the sel change event to fire.
            visComboBox.SelectedIndex = visSelection;
            mProject.PrepareScripts(mScriptSupport);
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
        private void GenerateParamControls(VisDescr descr) {
            VisParamDescr[] paramDescrs = descr.VisParamDescrs;

            ParameterList.Clear();
            foreach (VisParamDescr vpd in paramDescrs) {
                string rangeStr = string.Empty;
                object defaultVal = vpd.DefaultValue;

                // If we're editing a visualization, use the values from that as default.
                if (mOrigVis != null) {
                    if (mOrigVis.VisGenParams.TryGetValue(vpd.Name, out object val)) {
                        // Do we need to confirm that val has the correct type?
                        defaultVal = val;
                    }
                } else {
                    // New visualization.  Use the set's offset as the default value for
                    // any parameter called "offset".
                    if (vpd.Name.ToLowerInvariant() == "offset") {
                        defaultVal = mSetOffset;
                    }
                }

                // Set up rangeStr, if appropriate.
                VisParamDescr altVpd = vpd;
                if (vpd.CsType == typeof(int) || vpd.CsType == typeof(float)) {
                    if (vpd.Special == VisParamDescr.SpecialMode.Offset) {
                        defaultVal = mFormatter.FormatOffset24((int)defaultVal);
                        rangeStr = "[" + mFormatter.FormatOffset24(0) + "," +
                            mFormatter.FormatOffset24(mProject.FileDataLength - 1) + "]";

                        // Replace the vpd to provide a different min/max.
                        altVpd = new VisParamDescr(vpd.UiLabel, vpd.Name, vpd.CsType,
                            0, mProject.FileDataLength - 1, vpd.Special, vpd.DefaultValue);
                    } else {
                        rangeStr = "[" + vpd.Min + "," + vpd.Max + "]";
                    }
                }

                ParameterValue pv = new ParameterValue(altVpd, defaultVal, rangeStr);

                ParameterList.Add(pv);
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            mProject.UnprepareScripts();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            VisualizationItem item = (VisualizationItem)visComboBox.SelectedItem;
            Debug.Assert(item != null);
            Dictionary<string, object> valueDict = CreateVisGenParams();
            string trimTag = Visualization.TrimAndValidateTag(TagString, out bool isTagValid);
            Debug.Assert(isTagValid);
            NewVis = new Visualization(trimTag, item.VisDescriptor.Ident, valueDict);
            NewVis.CachedImage = (BitmapSource)previewImage.Source;

            DialogResult = true;
        }

        private Dictionary<string, object> CreateVisGenParams() {
            // Generate value dictionary.
            Dictionary<string, object> valueDict =
                new Dictionary<string, object>(ParameterList.Count);
            foreach (ParameterValue pv in ParameterList) {
                if (pv.Descr.CsType == typeof(bool)) {
                    Debug.Assert(pv.Value is bool);
                    valueDict.Add(pv.Descr.Name, (bool)pv.Value);
                } else if (pv.Descr.CsType == typeof(int)) {
                    bool ok = ParseIntObj(pv.Value, pv.Descr.Special, out int intVal);
                    Debug.Assert(ok);
                    valueDict.Add(pv.Descr.Name, intVal);
                } else if (pv.Descr.CsType == typeof(float)) {
                    bool ok = ParseFloatObj(pv.Value, out float floatVal);
                    Debug.Assert(ok);
                    valueDict.Add(pv.Descr.Name, floatVal);
                } else {
                    // skip it
                    Debug.Assert(false);
                }
            }

            return valueDict;
        }

        private bool ParseIntObj(object val, VisParamDescr.SpecialMode special, out int intVal) {
            if (val is int) {
                intVal = (int)val;
                return true;
            }
            string str = (string)val;
            int numBase = (special == VisParamDescr.SpecialMode.Offset) ? 16 : 10;

            string trimStr = str.Trim();
            if (trimStr.Length >= 1 && trimStr[0] == '+') {
                // May be present for an offset.  Just ignore it.  Don't use it as a radix char.
                trimStr = trimStr.Remove(0, 1);
            } else if (trimStr.Length >= 1 && trimStr[0] == '$') {
                numBase = 16;
                trimStr = trimStr.Remove(0, 1);
            } else if (trimStr.Length >= 2 && trimStr[0] == '0' &&
                    (trimStr[1] == 'x' || trimStr[1] == 'X')) {
                numBase = 16;
                trimStr = trimStr.Remove(0, 2);
            }
            if (trimStr.Length == 0) {
                intVal = -1;
                return false;
            }

            try {
                intVal = Convert.ToInt32(trimStr, numBase);
                return true;
            } catch (Exception) {
                intVal = -1;
                return false;
            }
        }

        private bool ParseFloatObj(object val, out float floatVal) {
            if (val is float) {
                floatVal = (float)val;
                return true;
            } else if (val is double) {
                floatVal = (float)(double)val;
                return true;
            } else if (val is int) {
                floatVal = (int)val;
                return true;
            }

            string str = (string)val;
            if (!float.TryParse(str, out floatVal)) {
                floatVal = 0.0f;
                return false;
            }
            return true;
        }

        private void UpdateControls() {
            IsValid = true;

            foreach (ParameterValue pv in ParameterList) {
                pv.ForegroundBrush = mDefaultLabelColor;
                if (pv.Descr.CsType == typeof(bool)) {
                    // always fine
                    continue;
                } else if (pv.Descr.CsType == typeof(int)) {
                    // integer, possibly Offset special
                    bool ok = ParseIntObj(pv.Value, pv.Descr.Special, out int intVal);
                    if (ok && (intVal < (int)pv.Descr.Min || intVal > (int)pv.Descr.Max)) {
                        // TODO(someday): make the range text red instead of the label
                        ok = false;
                    }
                    if (!ok) {
                        pv.ForegroundBrush = mErrorLabelColor;
                        IsValid = false;
                    }
                } else if (pv.Descr.CsType == typeof(float)) {
                    // float
                    bool ok = ParseFloatObj(pv.Value, out float floatVal);
                    if (ok && (floatVal < (float)pv.Descr.Min || floatVal > (float)pv.Descr.Max)) {
                        ok = false;
                    }
                    if (!ok) {
                        pv.ForegroundBrush = mErrorLabelColor;
                        IsValid = false;
                    }
                } else {
                    // unexpected
                    Debug.Assert(false);
                }
            }

            VisualizationItem item = (VisualizationItem)visComboBox.SelectedItem;

            if (!IsValid || item == null) {
                previewImage.Source = sBadParamsImage;
            } else {
                // Invoke the plugin.
                PluginErrMessage = string.Empty;

                IVisualization2d vis2d;
                try {
                    vis2d = item.Plugin.Generate2d(item.VisDescriptor,
                        CreateVisGenParams());
                    if (vis2d == null) {
                        Debug.WriteLine("Vis generator returned null");
                    }
                } catch (Exception ex) {
                    Debug.WriteLine("Vis generation failed: " + ex);
                    vis2d = null;
                }
                if (vis2d == null) {
                    previewImage.Source = sBadParamsImage;
                    if (!string.IsNullOrEmpty(LastPluginMessage)) {
                        // Report the last message we got as an error.
                        PluginErrMessage = LastPluginMessage;
                    }
                } else {
                    previewImage.Source = Visualization.ConvertToBitmapSource(vis2d);
                }
            }

            string trimTag = Visualization.TrimAndValidateTag(TagString, out bool tagOk);
            Visualization match = FindVisualizationByTag(trimTag);
            if (match != null && match != mOrigVis) {
                // Another vis already has this tag.
                tagOk = false;
            }
            if (!tagOk) {
                TagLabelBrush = mErrorLabelColor;
                IsValid = false;
            } else {
                TagLabelBrush = mDefaultLabelColor;
            }
        }

        /// <summary>
        /// Finds a Visualization with a matching tag, searching across all sets.
        /// </summary>
        /// <param name="tag">Tag to search for.</param>
        /// <returns>Matching Visualization, or null if not found.</returns>
        private Visualization FindVisualizationByTag(string tag) {
            foreach (KeyValuePair<int, VisualizationSet> kvp in mProject.VisualizationSets) {
                foreach (Visualization vis in kvp.Value) {
                    if (vis.Tag == tag) {
                        return vis;
                    }
                }
            }
            return null;
        }

        private void VisComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            VisualizationItem item = (VisualizationItem)visComboBox.SelectedItem;
            if (item == null) {
                Debug.Assert(false);    // not expected
                return;
            }
            Debug.WriteLine("VisComboBox sel change: " + item.VisDescriptor.Ident);
            GenerateParamControls(item.VisDescriptor);
            UpdateControls();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox src = (TextBox)sender;
            ParameterValue pv = (ParameterValue)src.DataContext;
            //Debug.WriteLine("TEXT CHANGE " + pv + ": " + src.Text);
            UpdateControls();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e) {
            CheckBox src = (CheckBox)sender;
            ParameterValue pv = (ParameterValue)src.DataContext;
            //Debug.WriteLine("CHECK CHANGE" + pv);
            UpdateControls();
        }
    }

    /// <summary>
    /// Describes a parameter and holds its value while being edited by WPF.
    /// </summary>
    /// <remarks>
    /// We currently detect updates with change events.  We could also tweak the Value setter
    /// to fire an event back to the window class when things change.  I don't know that there's
    /// an advantage to doing so.
    /// </remarks>
    public class ParameterValue : INotifyPropertyChanged {
        public VisParamDescr Descr { get; private set; }
        public string UiString { get; private set; }
        public string RangeText { get; private set; }

        private object mValue;
        public object Value {
            get { return mValue; }
            set { mValue = value; OnPropertyChanged(); }
        }

        private Brush mForegroundBrush;
        public Brush ForegroundBrush {
            get { return mForegroundBrush; }
            set { mForegroundBrush = value; OnPropertyChanged(); }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ParameterValue(VisParamDescr vpd, object val, string rangeText) {
            Descr = vpd;
            Value = val;
            RangeText = rangeText;

            char labelSuffix = (vpd.CsType == typeof(bool)) ? '?' : ':';
            UiString = vpd.UiLabel + labelSuffix;
        }

        public override string ToString() {
            return "[PV: " + Descr.Name + "=" + Value + "]";
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
                if (parm.Descr.CsType == typeof(bool)) {
                    return BoolTemplate;
                } else if (parm.Descr.CsType == typeof(int)) {
                    return IntTemplate;
                } else if (parm.Descr.CsType == typeof(float)) {
                    return FloatTemplate;
                } else {
                    Debug.WriteLine("WHA?" + parm.Value.GetType());
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
