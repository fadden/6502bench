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
using System.Diagnostics;

namespace SourceGen.AsmGen {
    /// <summary>
    /// Static information on assemblers supported by SourceGen.  This is relevant for both
    /// assembly source generation and assembler execution.  Nothing here is affected
    /// by whether or not the assembler in question is actually installed.
    /// </summary>
    public class AssemblerInfo {
        /// <summary>
        /// Enumeration of supported assemblers.  Sorted alphabetically by human-readable name
        /// looks nicest.
        /// </summary>
        public enum Id {
            Unknown = 0,
            Tass64,
            Acme,
            Cc65,
            Merlin32,
        }

        /// <summary>
        /// Static information for all known assemblers.
        /// 
        /// The AsmType argument may be null.  This is useful for non-cross assemblers.
        /// </summary>
        private static AssemblerInfo[] sInfo = new AssemblerInfo[] {
            new AssemblerInfo(Id.Unknown, "???", null, null),
            new AssemblerInfo(Id.Tass64, "64tass", typeof(GenTass64), typeof(AsmTass64)),
            new AssemblerInfo(Id.Acme, "ACME", typeof(GenAcme), typeof(AsmAcme)),
            new AssemblerInfo(Id.Cc65, "cc65", typeof(GenCc65), typeof(AsmCc65)),
            new AssemblerInfo(Id.Merlin32, "Merlin 32", typeof(GenMerlin32), typeof(AsmMerlin32)),
        };

        /// <summary>
        /// Identifier.
        /// </summary>
        public Id AssemblerId { get; private set; }

        /// <summary>
        /// Human-readable name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Type of generator class.
        /// </summary>
        public Type GenType { get; private set; }

        /// <summary>
        /// Type of assembler class.
        /// </summary>
        public Type AsmType { get; private set; }


        private AssemblerInfo(Id id, string name, Type genType, Type asmType) {
            AssemblerId = id;
            Name = name;
            GenType = genType;
            AsmType = asmType;
        }

        /// <summary>
        /// Returns an AssemblerInfo object for the specified id.
        /// </summary>
        /// <param name="id">Assembler identifier.</param>
        /// <returns>Reference to AssemblerInfo object.</returns>
        public static AssemblerInfo GetAssemblerInfo(Id id) {
            return sInfo[(int)id];
        }

        /// <summary>
        /// Generator factory method.
        /// </summary>
        /// <param name="id">ID of assembler to return generator instance for.</param>
        /// <returns>New source generator object.</returns>
        public static IGenerator GetGenerator(Id id) {
            Type genType = sInfo[(int)id].GenType;
            if (genType == null) {
                Debug.Assert(false);    // unexpected for generator
                return null;
            } else {
                return (IGenerator)Activator.CreateInstance(genType);
            }
        }

        /// <summary>
        /// Assembler factory method.
        /// </summary>
        /// <param name="id">ID of assembler to return assembler instance for.</param>
        /// <returns>New assembler interface object.</returns>
        public static IAssembler GetAssembler(Id id) {
            Type asmType = sInfo[(int)id].AsmType;
            if (asmType == null) {
                return null;
            } else {
                return (IAssembler)Activator.CreateInstance(asmType);
            }
        }

        /// <summary>
        /// Provides a way to iterate through the set of known assemblers.  This is probably
        /// YAGNI -- we could just return the array -- but it would allow us to apply filters,
        /// e.g. strip out assemblers that don't support 65816 code when that's the selected
        /// CPU definition.
        /// </summary>
        private class AssemblerInfoIterator : IEnumerator<AssemblerInfo> {
            private int mIndex = -1;

            public AssemblerInfo Current {
                get {
                    if (mIndex < 0) {
                        // not started
                        return null;
                    }
                    return sInfo[mIndex];
                }
            }

            object IEnumerator.Current {
                get {
                    return Current;
                }
            }

            public void Dispose() { }

            public bool MoveNext() {
                if (mIndex < 0) {
                    // skip element 0 (Unknown)
                    mIndex = 1;
                } else {
                    mIndex++;
                    if (mIndex >= sInfo.Length) {
                        return false;
                    }
                }
                return true;
            }

            public void Reset() {
                mIndex = -1;
            }
        }

        public static IEnumerator<AssemblerInfo> GetInfoEnumerator() {
            return new AssemblerInfoIterator();
        }


        public override string ToString() {
            return "AsmInfo " + ((int)AssemblerId).ToString() + ": " + Name;
        }
    }
}
