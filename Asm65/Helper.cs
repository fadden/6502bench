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
using System.Diagnostics;

namespace Asm65 {
    /// <summary>
    /// Small utility functions.
    /// </summary>
    public static class Helper {
        /// <summary>
        /// Computes the target address of an 8-bit relative branch instruction.
        /// </summary>
        /// <param name="addr">24-bit address of branch instruction opcode.</param>
        /// <param name="branchOffset">Branch operand.</param>
        /// <returns>Target address.</returns>
        public static int RelOffset8(int addr, sbyte branchOffset) {
            // Branch is relative to the start of the following instruction, so add 2.
            // Branches wrap around the current bank (in both directions), and the target
            // is in the same bank as source addr.
            Debug.Assert(addr >= 0 && addr <= 0xffffff);
            int target = (addr + 2 + branchOffset) & 0xffff;
            target |= addr & 0x7fff0000;
            return target;
        }

        /// <summary>
        /// Computes the target address of a 16-bit relative branch instruction.
        /// </summary>
        /// <param name="addr">24-bit address of branch instruction opcode.</param>
        /// <param name="branchOffset">Branch operand.</param>
        /// <returns>Target address.</returns>
        public static int RelOffset16(int addr, short branchOffset) {
            // Branch is relative to the start of the following instruction, so add 3.
            // Branches wrap around the current bank (in both directions), and the target
            // is in the same bank as source addr.
            Debug.Assert(addr >= 0 && addr <= 0xffffff);
            int target = (addr + 3 + branchOffset) & 0xffff;
            target |= addr & 0x7fff0000;
            return target;
        }
    }
}
