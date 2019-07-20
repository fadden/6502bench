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

namespace SourceGenWF {
    /// <summary>
    /// Weak reference to a symbol for use in an operand or data statement.  The reference
    /// is by name; if the symbol disappears or changes value, the reference can be ignored.
    /// This also specifies which part of the numeric value is of interest, so we can reference
    /// the high or low byte of a 16-bit value in (say) LDA #imm.
    /// 
    /// Instances are immutable.
    /// </summary>
    public class WeakSymbolRef {
        /// <summary>
        /// This identifies the part of the value that we're interested in.  All values are
        /// signed 32-bit integers.
        /// </summary>
        public enum Part {
            // This indicates which byte we start with, useful for immediate operands
            // and things like PEA.  By popular convention, these are referred to as
            // low, high, and bank.
            //
            // With 16-bit registers, Merlin 32 grabs the high *word*, while cc65's assembler
            // grabs the high *byte*.  One is a shift, the other is a byte select.  We use
            // low/high/bank just to mean position here.
            //
            // (Could make this orthogonal with a pair of bit fields, one for position and
            // one for width, but there's really only three widths of interest (1, 2, 3 bytes)
            // and that's defined by context.)
            Unknown = 0,
            Low,                // LDA #label, LDA #<label
            High,               // LDA #>label
            Bank,               // LDA #^label
        }

        /// <summary>
        /// Label of symbol of interest.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Which part of the value we're referencing.
        /// </summary>
        public Part ValuePart { get; private set; }

        /// <summary>
        /// Full constructor.
        /// </summary>
        public WeakSymbolRef(string label, Part part) {
            Debug.Assert(label != null);
            Label = label;
            ValuePart = part;
        }

        public static bool operator ==(WeakSymbolRef a, WeakSymbolRef b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            return Asm65.Label.LABEL_COMPARER.Equals(a.Label, b.Label) &&
                a.ValuePart == b.ValuePart;
        }
        public static bool operator !=(WeakSymbolRef a, WeakSymbolRef b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is WeakSymbolRef && this == (WeakSymbolRef)obj;
        }
        public override int GetHashCode() {
            return Asm65.Label.ToNormal(Label).GetHashCode() ^ (int)ValuePart;
        }

        public override string ToString() {
            return "WeakSym: " + Label + ":" + ValuePart;
        }
    }
}
