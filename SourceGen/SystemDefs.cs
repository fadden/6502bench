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
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

using Asm65;

namespace SourceGen {
    /// <summary>
    /// Target system definition, read from a config file.
    /// </summary>
    public class SystemDef {
        // Fields are deserialized from JSON.  Do not change the field names without updating
        // the config files.
        public string Name { get; set; }
        public string GroupName { get; set; }
        public string Cpu { get; set; }
        public float Speed { get; set; }
        public string Description { get; set; }
        public string[] SymbolFiles { get; set; }
        public string[] ExtensionScripts { get; set; }
        public Dictionary<string, string> Parameters { get; set; }


        /// <summary>
        /// Generates a human-readable summary of this system definition for display to
        /// the user.
        /// </summary>
        /// <returns>Multi-line string</returns>
        public string GetSummaryString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(Description);
            sb.Append("\r\n\r\n");

            sb.AppendFormat(Res.Strings.SETUP_SYSTEM_SUMMARY_FMT, Name, Cpu, Speed);

            if (SymbolFiles.Length > 0) {
                sb.Append("\r\n\r\n");
                sb.Append(Res.Strings.INITIAL_SYMBOL_FILES);
                foreach (string str in SymbolFiles) {
                    sb.Append("\r\n  ");
                    ExternalFile ef = ExternalFile.CreateFromIdent(str);
                    if (ef == null) {
                        // Shouldn't happen unless somebody botches an edit.
                        sb.Append("[INVALID] " + str);
                    } else {
                        sb.Append(ef.GetInnards());
                    }
                }
            }

            if (ExtensionScripts.Length > 0) {
                sb.Append("\r\n\r\n");
                sb.Append(Res.Strings.INITIAL_EXTENSION_SCRIPTS);
                foreach (string str in ExtensionScripts) {
                    sb.Append("\r\n  ");
                    ExternalFile ef = ExternalFile.CreateFromIdent(str);
                    if (ef == null) {
                        // Shouldn't happen unless somebody botches an edit.
                        sb.Append("[INVALID] " + str);
                    } else {
                        sb.Append(ef.GetInnards());
                    }
                }
            }

            if (Parameters.Count > 0) {
                sb.Append("\r\n\r\n");
                sb.Append(Res.Strings.INITIAL_PARAMETERS);
                foreach (KeyValuePair<string, string> kvp in Parameters) {
                    sb.Append("\r\n  ");
                    sb.Append(kvp.Key);
                    sb.Append(" = ");
                    sb.Append(kvp.Value);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Validates the values read from JSON.
        /// </summary>
        /// <returns>True if the inputs are valid and complete.</returns>
        public bool Validate() {
            if (string.IsNullOrEmpty(Name)) {
                return false;
            }
            if (string.IsNullOrEmpty(GroupName)) {
                return false;
            }
            if (CpuDef.GetCpuTypeFromName(Cpu) == CpuDef.CpuType.CpuUnknown) {
                return false;
            }
            if (Speed == 0.0f) {
                return false;
            }
            if (SymbolFiles == null || ExtensionScripts == null || Parameters == null) {
                // We don't really need to require these, but it's probably best to
                // insist on fully-formed entries.
                return false;
            }

            // Disallow file idents that point outside the runtime directory.  I don't think
            // there's any harm in allowing it, but there's currently no value in it either.
            foreach (string str in SymbolFiles) {
                if (!str.StartsWith("RT:")) {
                    return false;
                }
            }
            foreach (string str in ExtensionScripts) {
                if (!str.StartsWith("RT:")) {
                    return false;
                }
            }

            return true;
        }

        public override string ToString() {
            StringBuilder symFilesStr = new StringBuilder();
            foreach (string str in SymbolFiles) {
                if (symFilesStr.Length != 0) {
                    symFilesStr.Append(", ");
                }
                symFilesStr.Append(str);
            }
            StringBuilder scriptFilesStr = new StringBuilder();
            foreach (string str in ExtensionScripts) {
                if (scriptFilesStr.Length != 0) {
                    scriptFilesStr.Append(", ");
                }
                scriptFilesStr.Append(str);
            }
            StringBuilder paramStr = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in Parameters) {
                if (paramStr.Length != 0) {
                    paramStr.Append(", ");
                }
                paramStr.Append(kvp.Key);
                paramStr.Append('=');
                paramStr.Append(kvp.Value);
            }
            return "'" + Name + "', '" + GroupName + "', " + Cpu + " @ " + Speed + "MHz" +
                ", sym={" + symFilesStr + "}, scr={" + scriptFilesStr + "}, par={" +
                paramStr + "}";
        }
    }

    /// <summary>
    /// System definition collection.
    /// </summary>
    public class SystemDefSet {
        // Identification string, embedded in the JSON data.
        const string MAGIC = "6502bench SourceGen sysdef v1";

        // Fields are deserialized from JSON.  Do not change the field names without updating
        // the config files.
        public string Contents { get; set; }
        public SystemDef[] Defs { get; set; }


        /// <summary>
        /// Empty constructor, required for deserialization.
        /// </summary>
        public SystemDefSet() {}

        /// <summary>
        /// Reads the named config file.  Throws an exception on failure.
        /// </summary>
        /// <param name="pathName">Config file path name</param>
        /// <returns>Fully-populated system defs.</returns>
        public static SystemDefSet ReadFile(string pathName) {
            string fileStr = File.ReadAllText(pathName);
            //Debug.WriteLine("READ " + fileStr);

            JavaScriptSerializer ser = new JavaScriptSerializer();
            SystemDefSet sdf = ser.Deserialize<SystemDefSet>(fileStr);

            if (sdf.Contents != MAGIC) {
                // This shouldn't happen unless somebody is tampering with the
                // config file.
                Debug.WriteLine("Expected contents '" + MAGIC + "', got " +
                    sdf.Contents + "'");
                throw new InvalidDataException("Sys def file '" + pathName +
                    "': Unexpected contents '" + sdf.Contents + "'");
            }

            foreach (SystemDef sd in sdf.Defs) {
                Debug.WriteLine("### " + sd);
            }
            return sdf;
        }
    }
}
