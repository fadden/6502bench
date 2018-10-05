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
using System.Diagnostics;

using Asm65;

namespace SourceGen.Setup {
    /// <summary>
    /// Helper functions for extracting values from a SystemDef instance.
    /// </summary>
    public static class SystemDefaults {
        private const string LOAD_ADDRESS = "load-address";
        private const string ENTRY_FLAGS = "entry-flags";
        private const string UNDOCUMENTED_OPCODES = "undocumented-opcodes";
        private const string FIRST_WORD_IS_LOAD_ADDR = "first-word-is-load-addr";

        private const string ENTRY_FLAG_EMULATION = "emulation";
        private const string ENTRY_FLAG_NATIVE_LONG = "native-long";
        private const string ENTRY_FLAG_NATIVE_SHORT = "native-short";


        /// <summary>
        /// Gets the default load address.
        /// </summary>
        /// <param name="sysDef">SystemDef instance.</param>
        /// <returns>Specified load address, or 0x1000 if nothing defined.</returns>
        public static int GetLoadAddress(SystemDef sysDef) {
            Dictionary<string, string> parms = sysDef.Parameters;
            int retVal = 0x1000;

            if (parms.TryGetValue(LOAD_ADDRESS, out string valueStr)) {
                valueStr = valueStr.Trim();
                if (Number.TryParseInt(valueStr, out int parseVal, out int unused)) {
                    retVal = parseVal;
                } else {
                    Debug.WriteLine("WARNING: bad value for " + LOAD_ADDRESS + ": " + valueStr);
                }
            }
            return retVal;
        }

        /// <summary>
        /// Gets the default entry processor status flags.
        /// </summary>
        /// <param name="sysDef">SystemDef instance.</param>
        /// <returns>Status flags.</returns>
        public static StatusFlags GetEntryFlags(SystemDef sysDef) {
            Dictionary<string, string> parms = sysDef.Parameters;
            StatusFlags retFlags = StatusFlags.AllIndeterminate;

            // On 65802/65816, this selects emulation mode.  On 8-bit CPUs, these have
            // no effect, but this reflects how the CPU behaves (short regs, emu mode).
            retFlags.E = retFlags.M = retFlags.X = 1;

            // Decimal mode is rarely used, and interrupts are generally enabled.  Projects
            // that need to assume otherwise can alter the entry flags.  I want to start
            // with decimal mode clear because it affects the cycle timing display on a
            // number of 65C02 instructions.
            retFlags.D = retFlags.I = 0;

            if (parms.TryGetValue(ENTRY_FLAGS, out string valueStr)) {
                switch (valueStr) {
                    case ENTRY_FLAG_EMULATION:
                        break;
                    case ENTRY_FLAG_NATIVE_LONG:
                        retFlags.E = retFlags.M = retFlags.X = 0;
                        break;
                    case ENTRY_FLAG_NATIVE_SHORT:
                        retFlags.E = 0;
                        break;
                    default:
                        Debug.WriteLine("WARNING: bad value for " + ENTRY_FLAGS +
                            ": " + valueStr);
                        break;
                }
            }
            return retFlags;
        }

        /// <summary>
        /// Gets the default setting for undocumented opcode support.
        /// </summary>
        /// <param name="sysDef">SystemDef instance.</param>
        /// <returns>Enable/disable value.</returns>
        public static bool GetUndocumentedOpcodes(SystemDef sysDef) {
            return GetBoolParam(sysDef, UNDOCUMENTED_OPCODES, false);
        }

        /// <summary>
        /// Gets the default setting for using the first two bytes of the file as the
        /// load address.
        /// 
        /// This is primarily for C64.  Apple II DOS 3.3 binary files also put the load
        /// address first, followed by the length, but that's typically stripped out when
        /// the file is extracted.
        /// </summary>
        /// <param name="sysDef"></param>
        /// <returns></returns>
        public static bool GetFirstWordIsLoadAddr(SystemDef sysDef) {
            return GetBoolParam(sysDef, FIRST_WORD_IS_LOAD_ADDR, false);
        }

        /// <summary>
        /// Looks for a parameter with a matching name and a boolean value.
        /// </summary>
        /// <param name="sysDef">SystemDef reference.</param>
        /// <param name="paramName">Name of parameter to look for.</param>
        /// <param name="defVal">Default value.</param>
        /// <returns>Parsed value, or defVal if the parameter doesn't exist or the value is not
        ///   a boolean string.</returns>
        private static bool GetBoolParam(SystemDef sysDef, string paramName, bool defVal) {
            Dictionary<string, string> parms = sysDef.Parameters;
            bool retVal = defVal;

            if (parms.TryGetValue(paramName, out string valueStr)) {
                if (bool.TryParse(valueStr, out bool parseVal)) {
                    retVal = parseVal;
                } else {
                    Debug.WriteLine("WARNING: bad value for " + paramName + ": " + valueStr);
                }
            }
            return retVal;
        }
    }
}
