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
using System.Collections.ObjectModel;

namespace PluginCommon {
    /// <summary>
    /// Extension script "plugins" must implement this interface.
    /// </summary>
    public interface IPlugin {
        /// <summary>
        /// Identification string.  Contents are arbitrary, but should briefly identify the
        /// purpose of the plugin, e.g. "Apple II ProDOS 8 MLI call handler".  It may
        /// contain version information, but should not be expected to be machine-readable.
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Prepares the plugin for action.  Called at the start of every code analysis pass
        /// and when generating visualization images.
        ///
        /// In the current implementation, the file data will be the same every time,
        /// because it doesn't change after the project is opened.  However, this could
        /// change if we add a descramble feature.  The IApplication and AddressTranslate
        /// references will likely change between invocations.
        ///
        /// This may be called even when the plugin won't be asked to do anything.  Avoid
        /// performing expensive operations here.
        /// </summary>
        /// <param name="appRef">Reference to application interface.</param>
        /// <param name="fileData">65xx code and data.</param>
        /// <param name="addrTrans">Mapping between offsets and addresses.</param>
        void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans);

        /// <summary>
        /// Tells the plugin that we're done talking to it for now.
        /// </summary>
        void Unprepare();
    }

    /// <summary>
    /// Extension scripts that want to receive the list of symbols must implement this interface.
    /// </summary>
    public interface IPlugin_SymbolList {
        /// <summary>
        /// Receives a list of the currently defined platform, project, and user symbols.
        /// The list does not include auto-generated labels or local variables.
        ///
        /// This is called immediately after Prepare(), before any other interfaces are
        /// invoked, at the start of every code analysis pass.
        /// </summary>
        /// <param name="plSyms">Symbols available to plugins, in no particular order.</param>
        void UpdateSymbolList(List<PlSymbol> plSyms);

        /// <summary>
        /// Handles a notification that a user symbol has been added, edited, or removed.  If the
        /// label is of interest to the plugin, e.g. it changes how the plugin formats code or
        /// data, the app needs to know.
        /// </summary>
        /// <remarks>
        /// The application does a full re-analysis when project properties change, but not
        /// when labels are edited.  The CheckJsr/Jsl/Brk methods are only called during code
        /// analysis, so if their behavior changes based on the presence or absence of a user
        /// label then we need to tell the application that a full re-analysis is needed.
        ///
        /// Plugins that don't care about user symbols, e.g. that just use tagged platform
        /// symbols, can simply return false.  (Changes to user labels that overlap with
        /// project/platform symbols are detected by the app.)
        /// </remarks>
        /// <param name="beforeLabel">The label before the change, or empty if this is a
        ///   newly-added label.</param>
        /// <param name="afterLabel">The label after the change, or empty if the label was
        ///   removed.</param>
        /// <returns>True if the label change could affect the plugin's actions.</returns>
        bool IsLabelSignificant(string beforeLabel, string afterLabel);
    }

    /// <summary>
    /// Extension scripts that want to handle inline JSRs must implement this interface.
    /// </summary>
    public interface IPlugin_InlineJsr {
        /// <summary>
        /// Checks to see if code/data near a JSR instruction should be formatted.
        ///
        /// The file data is guaranteed to hold all bytes of the JSR (offset + 2).
        /// </summary>
        /// <param name="offset">Offset of the JSR instruction.</param>
        /// <param name="operand">16-bit JSR operand.</param>
        /// <param name="noContinue">Set to true if the JSR doesn't actually return.</param>
        void CheckJsr(int offset, int operand, out bool noContinue);
    }

    /// <summary>
    /// Extension scripts that want to handle inline JSLs must implement this interface.
    /// </summary>
    public interface IPlugin_InlineJsl {
        /// <summary>
        /// Checks to see if code/data near a JSL instruction should be formatted.
        ///
        /// The file data is guaranteed to hold all bytes of the JSL (offset + 3).
        /// </summary>
        /// <param name="offset">Offset of the JSL instruction.</param>
        /// <param name="operand">24-bit JSL operand.</param>
        /// <param name="noContinue">Set to true if the JSL doesn't actually return.</param>
        void CheckJsl(int offset, int operand, out bool noContinue);
    }

    /// <summary>
    /// Extension scripts that want to handle inline BRKs must implement this interface.
    /// </summary>
    public interface IPlugin_InlineBrk {
        /// <summary>
        /// Checks to see if code/data near a BRK instruction should be formatted.
        ///
        /// The file data is only guaranteed to hold the BRK opcode byte.
        /// </summary>
        /// <param name="offset">Offset of the BRK instruction.</param>
        /// <param name="isTwoBytes">True if the CPU is configured for two-byte BRKs.</param>
        /// <param name="noContinue">Set to true if the BRK doesn't actually return.</param>
        void CheckBrk(int offset, bool isTwoBytes, out bool noContinue);
    }

    /// <summary>
    /// Extension scripts that want to generate visualizations must implement this interface.
    /// </summary>
    public interface IPlugin_Visualizer {
        /// <summary>
        /// Retrieves a list of descriptors for visualization generators implemented by this
        /// plugin.  The items in the set may have values based on the file data.
        ///
        /// The caller must not modify the contents.
        /// </summary>
        VisDescr[] GetVisGenDescrs();

        /// <summary>
        /// Executes the specified visualization generator with the supplied parameters.
        /// </summary>
        /// <param name="descr">Visualization generator descriptor.</param>
        /// <param name="parms">Parameter set.</param>
        /// <returns>2D visualization object reference, or null if something went
        ///   wrong (unknown ident, bad parameters, etc).  By convention, an error
        ///   message is reported through the IApplication ReportError interface.</returns>
        IVisualization2d Generate2d(VisDescr descr, ReadOnlyDictionary<string, object> parms);
    }

    /// <summary>
    /// Version 2 of the visualizer interface.  Adds wireframe objects.
    /// </summary>
    public interface IPlugin_Visualizer_v2 : IPlugin_Visualizer {
        /// <summary>
        /// Executes the specified visualization generator with the supplied parameters.
        /// </summary>
        /// <param name="descr">Visualization generator descriptor.</param>
        /// <param name="parms">Parameter set.</param>
        /// <returns>Wireframe visualization object reference, or null if something went
        ///   wrong (unknown ident, bad parameters, etc).  By convention, an error
        ///   message is reported through the IApplication ReportError interface.</returns>
        IVisualizationWireframe GenerateWireframe(VisDescr descr,
            ReadOnlyDictionary<string, object> parms);
    }

    /// <summary>
    /// Visualization generator descriptor.  IPlugin_Visualizer instances publish a list of
    /// these to tell the application about the generators it supports.
    /// </summary>
    [Serializable]
    public class VisDescr {
        /// <summary>
        /// Unique identifier.  This is stored in the project file.  Names beginning with
        /// underscores ('_') are reserved and must not be defined by plugins.
        /// </summary>
        public string Ident { get; private set; }

        /// <summary>
        /// Human-readable string describing the visualizer.  Used for combo boxes and
        /// other UI elements.
        /// </summary>
        public string UiName { get; private set; }

        public enum VisType {
            Unknown = 0,
            Bitmap,             // 2D bitmap
            Wireframe,          // wireframe mesh
        }

        /// <summary>
        /// Visualization type.
        /// </summary>
        public VisType VisualizationType { get; private set; }

        /// <summary>
        /// Visualization parameter descriptors.
        /// </summary>
        public VisParamDescr[] VisParamDescrs { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisDescr(string ident, string uiName, VisType type, VisParamDescr[] descrs) {
            Ident = ident;
            UiName = uiName;
            VisualizationType = type;
            VisParamDescrs = descrs;
        }
    }

    /// <summary>
    /// Visualization parameter descriptor.
    /// </summary>
    /// <remarks>
    /// We provide min/max for individual numeric fields, but there's no way to check other
    /// fields to see if e.g. Stride >= Width.  We'd need a "verify" function and a way to
    /// report errors that the GUI could use to point out what was wrong.
    /// </remarks>
    [Serializable]
    public class VisParamDescr {
        /// <summary>
        /// Special feature enumeration.
        /// </summary>
        public enum SpecialMode {
            None = 0, Offset
        }

        /// <summary>
        /// Label to show in the UI.
        /// </summary>
        public string UiLabel { get; private set; }

        /// <summary>
        /// Name to use internally.  Do not use names that start with an underscore ('_'), as
        /// these are reserved for future internal use.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Parameter data type.
        /// </summary>
        public Type CsType { get; private set; }

        /// <summary>
        /// Minimum allowable value for int/float (and perhaps string length).
        /// </summary>
        public object Min { get; private set; }

        /// <summary>
        /// Maximum allowable value for int/float (and perhaps string length).
        /// </summary>
        public object Max { get; private set; }

        /// <summary>
        /// Set to a value if the field requires special treatment.
        /// </summary>
        public SpecialMode Special { get; private set; }

        /// <summary>
        /// Default value for this field.
        /// </summary>
        public object DefaultValue { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisParamDescr(string uiLabel, string name, Type csType, object min, object max,
                SpecialMode special, object defVal) {
            if (defVal.GetType() != csType) {
                throw new ArgumentException("Mismatch between type and default value in \"" +
                    name + "\"");
            }
            UiLabel = uiLabel;
            Name = name;
            CsType = csType;
            Min = min;
            Max = max;
            Special = special;
            DefaultValue = defVal;
        }
    }

    /// <summary>
    /// Rendered 2D visualization.  The object represents the "raw" form of the data,
    /// without scaling or filtering.
    /// </summary>
    public interface IVisualization2d {
        /// <summary>
        /// Bitmap width, in pixels.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Bitmap height, in pixels.
        /// </summary>
        int Height { get; }

        //void SetPixelIndex(int x, int y, byte colorIndex);
        //int GetPixel(int x, int y);     // returns ARGB value

        /// <summary>
        /// Returns a densely-packed array of color indices or ARGB values.
        /// Do not modify.
        /// </summary>
        byte[] GetPixels();

        /// <summary>
        /// Returns the color palette as a series of 32-bit ARGB values.  Will be null for
        /// direct-color images.
        /// Do not modify.
        /// </summary>
        int[] GetPalette();

        // TODO(maybe): report pixel aspect ratio?
    }

    /// <summary>
    /// Holds raw vertex/edge/normal data collected from a 2D or 3D wireframe mesh.  We use
    /// a typical right-handed coordinate system (Z comes out of the screen), with the
    /// expectation that the mesh will be presented such that the object is right-side up
    /// and facing toward the viewer (nose toward +Z).
    ///
    /// All objects will have vertices and edges.  Face normals are optional.
    /// </summary>
    /// <remarks>
    /// The face-normal stuff is designed specifically for Elite.  Besides being one of the
    /// very few 6502-based games to use backface culling, it extended the concept to allow
    /// convex shapes to have protrusions.
    ///
    /// We favor multiple arrays over compound objects for this interface to avoid making
    /// such objects part of the plugin interface.
    ///
    /// TODO(maybe): specify colors for edges.  Not widely used?
    /// </remarks>
    public interface IVisualizationWireframe {
        // Each function returns the specified data.  Do not modify the returned arrays.

        float[] GetVerticesX();
        float[] GetVerticesY();
        float[] GetVerticesZ();

        IntPair[] GetEdges();

        float[] GetNormalsX();
        float[] GetNormalsY();
        float[] GetNormalsZ();

        IntPair[] GetVertexFaces();
        IntPair[] GetEdgeFaces();
        int[] GetExcludedVertices();
        int[] GetExcludedEdges();
    }

    /// <summary>
    /// Represents a pair of integers.  Immutable.
    /// </summary>
    [Serializable]
    public struct IntPair {
        public int Val0 { get; private set; }
        public int Val1 { get; private set; }

        public IntPair(int val0, int val1) {
            Val0 = val0;
            Val1 = val1;
        }
    }

    /// <summary>
    /// Interfaces provided by the application for use by plugins.  An IApplication instance
    /// is passed to the plugin as an argument to Prepare().
    /// </summary>
    public interface IApplication {
        /// <summary>
        /// Reports an error message to the application.  Used by visualizers.
        /// </summary>
        /// <param name="msg"></param>
        void ReportError(string msg);

        /// <summary>
        /// Sends a debug message to the application.  This can be useful when debugging scripts.
        /// (For example, DEBUG > Show Analyzer Output shows output generated while performing
        /// code analysis.)
        /// </summary>
        /// <param name="msg">Message to send.</param>
        void DebugLog(string msg);

        /// <summary>
        /// Specifies operand formatting.
        /// </summary>
        /// <param name="offset">File offset of opcode.</param>
        /// <param name="subType">Sub-type.  Must be appropriate for NumericLE.</param>
        /// <param name="label">Optional symbolic label.</param>
        /// <returns>True if the change was made, false if it was rejected.</returns>
        bool SetOperandFormat(int offset, DataSubType subType, string label);

        /// <summary>
        /// Formats file data as inline data.
        /// </summary>
        /// <param name="offset">File offset.</param>
        /// <param name="length">Length of item.</param>
        /// <param name="type">Type of item.  Must be NumericLE, NumericBE, or Dense.</param>
        /// <param name="subType">Sub-type.  Must be appropriate for type.</param>
        /// <param name="label">Optional symbolic label.</param>
        /// <returns>True if the change was made, false if it was rejected (e.g. because
        ///   the area is already formatted, or contains code).</returns>
        /// <exception cref="PluginException">If something is really wrong, e.g. data runs
        ///   off end of file.</exception>
        bool SetInlineDataFormat(int offset, int length, DataType type,
                DataSubType subType, string label);
    }

    /// <summary>
    /// Data format type.
    /// </summary>
    /// <remarks>
    /// Essentially a clone of FormatDescriptor.Type.
    /// </remarks>
    public enum DataType {
        Unknown = 0,
        NumericLE,
        NumericBE,
        StringGeneric,
        StringReverse,
        StringNullTerm,
        StringL8,
        StringL16,
        StringDci,
        Dense,
        Fill
    }

    /// <summary>
    /// Data format sub-type.
    /// </summary>
    /// <remarks>
    /// Essentially a clone of FormatDescriptor.SubType.
    /// </remarks>
    public enum DataSubType {
        // No sub-type specified.
        None = 0,

        // For NumericLE/BE
        Hex,
        Decimal,
        Binary,
        Address,
        Symbol,

        // Strings and NumericLE/BE (single character)
        Ascii,
        HighAscii,
        C64Petscii,
        C64Screen
    }
}
