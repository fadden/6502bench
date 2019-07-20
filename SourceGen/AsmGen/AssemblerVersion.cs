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

namespace SourceGen.AsmGen {
    public class AssemblerVersion {
        /// <summary>
        /// Version string reported by the assembler.  Retained mostly for debugging.
        /// </summary>
        public string VersionStr { get; private set; }

        /// <summary>
        /// Version string converted to a Version object.  For very complex version strings,
        /// some information may be lost in the conversion.
        /// </summary>
        public CommonUtil.Version Version { get; private set; }

        //// Command pathname and modification date.  Useful for caching values.
        //private string ExeName { get; set; }
        //private DateTime ExeModWhen { get; set; }

        public AssemblerVersion(string versionStr, CommonUtil.Version version) {
            VersionStr = versionStr;
            Version = version;
        }

        public static AssemblerVersion GetVersion(AssemblerInfo.Id id) {
            IAssembler asm = AssemblerInfo.GetAssembler(id);
            if (asm == null) {
                Debug.WriteLine("Assembler " + id + " not configured");
                return null;
            }
            return asm.QueryVersion();
        }

        public override string ToString() {
            return "['" + VersionStr + "'/" + Version + "]";
        }
    }

    /// <summary>
    /// Maintains a cache of the versions of installed assemblers.
    /// </summary>
    public static class AssemblerVersionCache {
        private static Dictionary<AssemblerInfo.Id, AssemblerVersion> sVersions =
            new Dictionary<AssemblerInfo.Id, AssemblerVersion>();
        private static bool sQueried = false;

        /// <summary>
        /// Queries the versions from all known assemblers, replacing any previously held data.
        ///
        /// WARNING: this will execute all configured assemblers, and may cause a noticeable
        /// pause while running.  Should only be a fraction of a second on a modern system,
        /// but it's something to bear in mind.
        /// </summary>
        public static void QueryVersions() {
            IEnumerator<AssemblerInfo> iter = AssemblerInfo.GetInfoEnumerator();
            while (iter.MoveNext()) {
                AssemblerInfo.Id id = iter.Current.AssemblerId;
                if (id == AssemblerInfo.Id.Unknown) {
                    continue;
                }

                AssemblerVersion vers = null;
                IAssembler asm = AssemblerInfo.GetAssembler(id);
                if (asm != null) {
                    vers = asm.QueryVersion();
                }

                Debug.WriteLine("Asm version query: " + id + "=" + vers);
                sVersions[id] = vers;
            }

            sQueried = true;
        }

        /// <summary>
        /// Returns the version information, or null if the query failed for this assembler.
        /// </summary>
        /// <param name="id">Assembler identifier.</param>
        /// <returns>Version info.</returns>
        public static AssemblerVersion GetVersion(AssemblerInfo.Id id) {
            if (!sQueried) {
                QueryVersions();
            }
            if (sVersions.TryGetValue(id, out AssemblerVersion vers)) {
                return vers;
            } else {
                return null;
            }
        }
    }
}
