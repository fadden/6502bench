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
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace SourceGen.AsmGen {
    /// <summary>
    /// Assembler configuration holder.  Serializes and deserializes information held in
    /// application settings.
    /// </summary>
    public class AssemblerConfig {
        // Public fields are deserialized from JSON.  Changing the names will break compatibility.

        /// <summary>
        /// Path to cross-assembler executable.  Will be null or empty if this assembler
        /// is not configured.
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// Column display widths.
        /// </summary>
        public int[] ColumnWidths { get; set; }

        public const int NUM_COLUMNS = 4;  // label, opcode, operand, comment


        /// <summary>
        /// Nullary constructor, for serialization.
        /// </summary>
        public AssemblerConfig() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="exePath">Path to executable.  May be empty.</param>
        /// <param name="widths">Column widths.</param>
        public AssemblerConfig(string exePath, int[] widths) {
            if (exePath == null) {
                throw new Exception("Bad exe path");
            }
            if (widths.Length != NUM_COLUMNS) {
                throw new Exception("Bad widths.Length " + widths.Length);
            }
            ExecutablePath = exePath;
            ColumnWidths = widths;
        }

        private static string GetSettingName(AssemblerInfo.Id id) {
            return AppSettings.ASM_CONFIG_PREFIX + id.ToString();
        }

        /// <summary>
        /// Creates a populated AssemblerConfig from the app settings for the specified ID.
        /// If the assembler hasn't been configured yet, the default configuration object
        /// will be returned.
        /// </summary>
        /// <param name="settings">Settings object to pull the values from.</param>
        /// <param name="id">Assembler ID.</param>
        /// <returns>The AssemblerConfig.</returns>
        public static AssemblerConfig GetConfig(AppSettings settings, AssemblerInfo.Id id) {
            string cereal = settings.GetString(GetSettingName(id), null);
            if (string.IsNullOrEmpty(cereal)) {
                IAssembler asm = AssemblerInfo.GetAssembler(id);
                return asm.GetDefaultConfig();
            }

            JavaScriptSerializer ser = new JavaScriptSerializer();
            try {
                AssemblerConfig config = ser.Deserialize<AssemblerConfig>(cereal);
                if (config.ColumnWidths == null || config.ColumnWidths.Length != NUM_COLUMNS) {
                    throw new Exception("Bad column widths");
                }
                if (config.ExecutablePath == null) {
                    throw new Exception("Missing exe path");
                }
                return config;
            } catch (Exception ex) {
                Debug.WriteLine("AssemblerConfig deserialization failed: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates the assembler settings for the specified ID.
        /// </summary>
        /// <param name="settings">Settings object to update.</param>
        /// <param name="id">Assembler ID.</param>
        /// <param name="config">Asm configuration.</param>
        public static void SetConfig(AppSettings settings, AssemblerInfo.Id id,
                AssemblerConfig config) {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            string cereal = ser.Serialize(config);

            settings.SetString(GetSettingName(id), cereal);
        }
    }
}
