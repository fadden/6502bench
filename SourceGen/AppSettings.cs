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
using System.Diagnostics;
using System.Text;
using System.Web.Script.Serialization;

namespace SourceGen {
    /// <summary>
    /// Application settings registry.  This holds both user-accessible settings and saved
    /// values like window widths.
    /// 
    /// Everything is stored as name/value pairs, where the value is serialized as a string.
    /// Names are case-sensitive.
    /// 
    /// We don't discard things we don't recognize.  If we somehow end up reading a config
    /// file from a newer version of the app, the various settings will be retained.
    /// </summary>
    public class AppSettings {
        #region Names

        // Name constants.  Having them defined here avoids collisions and misspellings, and
        // makes it easy to find all uses.

        // Main window.
        public const string MAIN_WINDOW_PLACEMENT = "main-window-placement";
        public const string MAIN_LEFT_PANEL_WIDTH = "main-left-panel-width";
        public const string MAIN_RIGHT_PANEL_WIDTH = "main-right-panel-width";
        public const string MAIN_REFERENCES_HEIGHT = "main-references-height";
        public const string MAIN_SYMBOLS_HEIGHT = "main-symbols-height";

        // New project dialog.
        public const string NEWP_SELECTED_SYSTEM = "newp-selected-system";

        // Formatting choices.
        public const string FMT_UPPER_HEX_DIGITS = "fmt-upper-hex-digits";
        public const string FMT_UPPER_OP_MNEMONIC = "fmt-upper-op-mnemonic";
        public const string FMT_UPPER_PSEUDO_OP_MNEMONIC = "fmt-upper-pseudo-op-mnemonic";
        public const string FMT_UPPER_OPERAND_A = "fmt-upper-operand-a";
        public const string FMT_UPPER_OPERAND_S = "fmt-upper-operand-s";
        public const string FMT_UPPER_OPERAND_XY = "fmt-upper-operand-xy";
        public const string FMT_ADD_SPACE_FULL_COMMENT = "fmt-add-space-full-comment";
        public const string FMT_SPACES_BETWEEN_BYTES = "fmt-spaces-between-bytes";

        public const string FMT_OPCODE_SUFFIX_ABS = "fmt-opcode-suffix-abs";
        public const string FMT_OPCODE_SUFFIX_LONG = "fmt-opcode-suffix-long";
        public const string FMT_OPERAND_PREFIX_ABS = "fmt-operand-prefix-abs";
        public const string FMT_OPERAND_PREFIX_LONG = "fmt-operand-prefix-long";
        public const string FMT_EXPRESSION_MODE = "fmt-expression-mode";

        public const string FMT_PSEUDO_OP_NAMES = "fmt-pseudo-op-names";
        public const string FMT_CHAR_DELIM = "fmt-char-delim";
        public const string FMT_STRING_DELIM = "fmt-string-delim";
        public const string FMT_LOCAL_VARIABLE_PREFIX = "fmt-local-variable-prefix";

        public const string CLIP_LINE_FORMAT = "clip-line-format";

        // Main project view settings.
        public const string PRVW_RECENT_PROJECT_LIST = "prvw-recent-project-list";

        // Symbol-list window options.
        public const string SYMWIN_SHOW_USER = "symwin-show-user";
        public const string SYMWIN_SHOW_AUTO = "symwin-show-auto";
        public const string SYMWIN_SHOW_PROJECT = "symwin-show-project";
        public const string SYMWIN_SHOW_PLATFORM = "symwin-show-platform";
        public const string SYMWIN_SHOW_CONST = "symwin-show-const";
        public const string SYMWIN_SHOW_ADDR = "symwin-show-addr";
        public const string SYMWIN_SORT_ASCENDING = "symwin-sort-ascending";
        public const string SYMWIN_SORT_COL = "symwin-sort-col";

        public const string SYMWIN_COL_WIDTHS = "symwin-col-widths";

        // References window options.
        public const string REFWIN_COL_WIDTHS = "refwin-col-widths";

        // Notes window options.
        public const string NOTEWIN_COL_WIDTHS = "notewin-col-widths";

        // Code List View settings.
        public const string CDLV_COL_WIDTHS = "cdlv-col-widths1";
        public const string CDLV_FONT_FAMILY = "cdlv-font-family";
        public const string CDLV_FONT_SIZE = "cdlv-font-size";

        // Operand edit settings.
        public const string OPED_DEFAULT_STRING_ENCODING = "oped-default-string-encoding";

        // Hex dump viewer settings.
        public const string HEXD_ASCII_ONLY = "hexd-ascii-only";
        public const string HEXD_CHAR_CONV = "hexd-char-conv1";

        // ASCII chart viewer settings.
        public const string ASCCH_MODE = "ascch-mode1";

        // Source generation settings.
        public const string SRCGEN_DEFAULT_ASM = "srcgen-default-asm";
        public const string SRCGEN_ADD_IDENT_COMMENT = "srcgen-add-ident-comment";
        public const string SRCGEN_DISABLE_LABEL_LOCALIZATION = "srcgen-disable-label-localization";
        public const string SRCGEN_LONG_LABEL_NEW_LINE = "srcgen-long-label-new-line";
        public const string SRCGEN_SHOW_CYCLE_COUNTS = "srcgen-show-cycle-counts";

        // Assembler settings prefix
        public const string ASM_CONFIG_PREFIX = "asm-config-";

        // Text/HTML export settings.
        public const string EXPORT_INCLUDE_NOTES = "export-include-notes";
        public const string EXPORT_SHOW_OFFSET = "export-show-offset";
        public const string EXPORT_SHOW_ADDR = "export-show-addr";
        public const string EXPORT_SHOW_BYTES = "export-show-bytes";
        public const string EXPORT_SHOW_FLAGS = "export-show-flags";
        public const string EXPORT_SHOW_ATTR = "export-show-attr";
        public const string EXPORT_COL_WIDTHS = "export-col-widths";
        public const string EXPORT_TEXT_MODE = "export-text-mode";
        public const string EXPORT_SELECTION_ONLY = "export-selection-only";
        public const string EXPORT_LONG_LABEL_NEW_LINE = "export-long-label-new-line";

        // Internal debugging features.
        public const string DEBUG_MENU_ENABLED = "debug-menu-enabled";

        #endregion Names

        #region Implementation

        // App settings file header.
        public const string MAGIC = "### 6502bench SourceGen settings v1.0 ###";


        /// <summary>
        /// Single global instance of app settings.
        /// </summary>
        public static AppSettings Global {
            get {
                return sSingleton;
            }
        }
        private static AppSettings sSingleton = new AppSettings();


        /// <summary>
        /// Dirty flag, set to true by every "set" call.
        /// </summary>
        public bool Dirty { get; set; }

        /// <summary>
        /// Settings storage.
        /// </summary>
        private Dictionary<string, string> mSettings = new Dictionary<string, string>();


        private AppSettings() { }

        /// <summary>
        /// Creates a copy of this object.
        /// </summary>
        /// <returns></returns>
        public AppSettings GetCopy() {
            AppSettings copy = new AppSettings();
            //copy.mSettings.EnsureCapacity(mSettings.Count);
            foreach (KeyValuePair<string, string> kvp in mSettings) {
                copy.mSettings.Add(kvp.Key, kvp.Value);
            }
            return copy;
        }

        /// <summary>
        /// Replaces the existing list of settings with a new list.
        /// 
        /// This can be used to replace the contents of the global settings object without
        /// discarding the object itself, which is useful in case something has cached a
        /// reference to the singleton.
        /// </summary>
        /// <param name="newSettings"></param>
        public void ReplaceSettings(AppSettings newSettings) {
            // Clone the new list, and stuff it into the old object.  This way the
            // objects aren't sharing lists.
            mSettings = newSettings.GetCopy().mSettings;
            Dirty = true;
        }

        /// <summary>
        /// Merges settings from another settings object into this one.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="newSettings"></param>
        public void MergeSettings(AppSettings newSettings) {
            foreach (KeyValuePair<string, string> kvp in newSettings.mSettings) {
                mSettings[kvp.Key] = kvp.Value;
            }
            Dirty = true;
        }

        /// <summary>
        /// Retrieves an integer setting.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="defaultValue">Setting default value.</param>
        /// <returns>The value found, or the default value if no setting with the specified
        ///   name exists, or the stored value is not an integer.</returns>
        public int GetInt(string name, int defaultValue) {
            if (!mSettings.TryGetValue(name, out string valueStr)) {
                return defaultValue;
            }
            if (!int.TryParse(valueStr, out int value)) {
                Debug.WriteLine("Warning: int parse failed on " + name + "=" + valueStr);
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Sets an integer setting.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        public void SetInt(string name, int value) {
            mSettings[name] = value.ToString();
            Dirty = true;
        }

        /// <summary>
        /// Retrieves a boolean setting.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="defaultValue">Setting default value.</param>
        /// <returns>The value found, or the default value if no setting with the specified
        ///   name exists, or the stored value is not a boolean.</returns>
        public bool GetBool(string name, bool defaultValue) {
            if (!mSettings.TryGetValue(name, out string valueStr)) {
                return defaultValue;
            }
            if (!bool.TryParse(valueStr, out bool value)) {
                Debug.WriteLine("Warning: bool parse failed on " + name + "=" + valueStr);
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Sets a boolean setting.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.</param>
        public void SetBool(string name, bool value) {
            mSettings[name] = value.ToString();
            Dirty = true;
        }

        /// <summary>
        /// Retrieves an enumerated value setting.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="enumType">Enum type that the value is part of.</param>
        /// <param name="defaultValue">Setting default value.</param>
        /// <returns>The value found, or the default value if no setting with the specified
        ///   name exists, or the stored value is not a member of the specified enumerated
        ///   type.</returns>
        public int GetEnum(string name, Type enumType, int defaultValue) {
            if (!mSettings.TryGetValue(name, out string valueStr)) {
                return defaultValue;
            }
            try {
                object o = Enum.Parse(enumType, valueStr);
                return (int)o;
            } catch (ArgumentException ae) {
                Debug.WriteLine("Failed to parse " + valueStr + " (enum " + enumType + "): " +
                    ae.Message);
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets an enumerated setting.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="enumType">Enum type.</param>
        /// <param name="value">Setting value (integer enum index).</param>
        public void SetEnum(string name, Type enumType, int value) {
            mSettings[name] = Enum.GetName(enumType, value);
            Dirty = true;
        }

        /// <summary>
        /// Retrieves a string setting.  The default value will be returned if the key
        /// is not found, or if the value is null.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="defaultValue">Setting default value.</param>
        /// <returns>The value found, or defaultValue if not value is found.</returns>
        public string GetString(string name, string defaultValue) {
            if (!mSettings.TryGetValue(name, out string valueStr) || valueStr == null) {
                return defaultValue;
            }
            return valueStr;
        }

        /// <summary>
        /// Sets a string setting.
        /// </summary>
        /// <param name="name">Setting name.</param>
        /// <param name="value">Setting value.  If the value is null, the setting will be
        ///   removed.</param>
        public void SetString(string name, string value) {
            if (value == null) {
                mSettings.Remove(name);
            } else {
                mSettings[name] = value;
            }
            Dirty = true;
        }

        /// <summary>
        /// Serializes settings dictionary into a string, for saving settings to a file.
        /// </summary>
        /// <returns>Serialized settings.</returns>
        public string Serialize() {
            StringBuilder sb = new StringBuilder(1024);
            sb.Append(MAGIC);   // augment with version string, which will be stripped
            sb.Append("\r\n");  // will be ignored by deserializer; might get converted to \n

            JavaScriptSerializer ser = new JavaScriptSerializer();
            string cereal = ser.Serialize(mSettings);

            // add some linefeeds to make it easier for humans
            cereal = CommonUtil.TextUtil.NonQuoteReplace(cereal, ",\"", ",\r\n\"");
            sb.Append(cereal);

            // Stick a linefeed at the end.
            sb.Append("\r\n");
            return sb.ToString();
        }

        /// <summary>
        /// Deserializes settings from a string, for loading settings from a file.
        /// </summary>
        /// <param name="cereal">Serialized settings.</param>
        /// <returns>Deserialized settings, or null if deserialization failed.</returns>
        public static AppSettings Deserialize(string cereal) {
            if (!cereal.StartsWith(MAGIC)) {
                return null;
            }

            // Skip past header.
            cereal = cereal.Substring(MAGIC.Length);

            AppSettings settings = new AppSettings();
            JavaScriptSerializer ser = new JavaScriptSerializer();
            try {
                settings.mSettings = ser.Deserialize<Dictionary<string, string>>(cereal);
                return settings;
            } catch (Exception ex) {
                Debug.WriteLine("Settings deserialization failed: " + ex.Message);
                return null;
            }
        }

        #endregion Implementation
    }
}
