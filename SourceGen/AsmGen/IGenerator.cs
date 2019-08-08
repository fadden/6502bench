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
using System.ComponentModel;

using Asm65;

namespace SourceGen.AsmGen {
    /// <summary>
    /// Common interface for generating assembler-specific source code.
    /// </summary>
    public interface IGenerator {
        /// <summary>
        /// Returns some strings and format options for use in for the display list, configurable
        /// through the app settings "quick set" feature.  These are not used when generating
        /// source code.
        /// 
        /// This may be called on an unconfigured IGenerator.
        /// </summary>
        /// <param name="pseudoOps">Table of pseudo-op names.</param>
        /// <param name="formatConfig">Format configuration.</param>
        void GetDefaultDisplayFormat(out PseudoOp.PseudoOpNames pseudoOps,
            out Formatter.FormatConfig formatConfig);


        /// <summary>
        /// Configure generator.  Must be called before calling any other method or using
        /// properties, unless otherwise noted.
        /// </summary>
        /// <param name="project">Project to generate source for.</param>
        /// <param name="workDirectory">Directory in which to create output files.</param>
        /// <param name="fileNameBase">Name to use as base for filenames.</param>
        /// <param name="asmVersion">Version of assembler to target.  Pass in null
        ///   to target latest known version.</param>
        /// <param name="settings">App settings object.</param>
        void Configure(DisasmProject project, string workDirectory, string fileNameBase,
            AssemblerVersion asmVersion, AppSettings settings);

        /// <summary>
        /// Project object with file data and Anattribs.
        /// </summary>
        DisasmProject Project { get; }

        /// <summary>
        /// Source code formatter.
        /// </summary>
        Formatter SourceFormatter { get; }

        /// <summary>
        /// Application settings.
        /// </summary>
        AppSettings Settings { get; }

        /// <summary>
        /// Assembler-specific behavior.  Used to handle quirky behavior for things that
        /// are otherwise managed by common code.
        /// </summary>
        AssemblerQuirks Quirks { get; }

        /// <summary>
        /// Label localization object.  Behavior is assembler-specific.
        /// </summary>
        LabelLocalizer Localizer { get; }

        /// <summary>
        /// Generates source files on a background thread.  Method must not make any UI calls.
        /// </summary>
        /// <param name="worker">Async work object, used to report progress updates and
        ///   check for cancellation.</param>
        /// <returns>List of pathnames of generated files.</returns>
        List<string> GenerateSource(BackgroundWorker worker);

        /// <summary>
        /// Provides an opportunity for the assembler to replace a mnemonic with another, or
        /// output an instruction as hex bytes.
        /// </summary>
        /// <param name="offset">Opcode offset.</param>
        /// <param name="op">Opcode to replace.</param>
        /// <returns>Replacement mnemonic, an empty string if the original is fine, or
        ///   null if the op is unsupported or broken and should be emitted as hex.</returns>
        string ModifyOpcode(int offset, OpDef op);

        /// <summary>
        /// Generates an opcode/operand pair for a short sequence of bytes (1-4 bytes).
        /// Does not produce any source output.
        /// </summary>
        /// <param name="offset">Offset to data.</param>
        /// <param name="count">Number of bytes (1-4).</param>
        /// <param name="opcode">Opcode mnemonic.</param>
        /// <param name="operand">Formatted operand.</param>
        void GenerateShortSequence(int offset, int length, out string opcode, out string operand);

        /// <summary>
        /// Outputs zero or more lines of assembler configuration.  This comes after the
        /// header comment but before any directives.  Useful for configuring the CPU type
        /// and assembler options.
        /// </summary>
        void OutputAsmConfig();

        /// <summary>
        /// Outputs one or more lines of data for the specified offset.
        /// </summary>
        /// <param name="offset">Offset to data.</param>
        void OutputDataOp(int offset);

        /// <summary>
        /// Outputs an equate directive.  The numeric value is already formatted.
        /// </summary>
        /// <param name="name">Symbol label.</param>
        /// <param name="valueStr">Formatted value.</param>
        /// <param name="comment">End-of-line comment.</param>
        void OutputEquDirective(string name, string valueStr, string comment);

        /// <summary>
        /// Outputs a code origin directive.
        /// </summary>
        /// <param name="offset">Offset of code targeted to new address.</param>
        /// <param name="address">24-bit address.</param>
        void OutputOrgDirective(int offset, int address);

        /// <summary>
        /// Notify the assembler of a change in register width.
        /// 
        /// Merlin32 always sets both values (e.g. "MX %00"), cc65 sets each register
        /// individually (".A16", ".I8").  We need to accommodate both styles.
        /// </summary>
        /// <param name="offset">Offset of change.</param>
        /// <param name="prevM">Previous value for M flag.</param>
        /// <param name="prevX">Previous value for X flag.</param>
        /// <param name="newM">New value for M flag.</param>
        /// <param name="newX">New value for X flag.</param>
        void OutputRegWidthDirective(int offset, int prevM, int prevX, int newM, int newX);

        /// <summary>
        /// Output a line of source code.  All elements must be fully formatted, except for
        /// certain assembler-specific things like ':' on labels.  The items will be padded
        /// with spaces to fit specific column widths.
        /// </summary>
        /// <param name="label">Optional label.</param>
        /// <param name="opcode">Opcode mnemonic.</param>
        /// <param name="operand">Operand; may be empty.</param>
        /// <param name="comment">Optional comment.</param>
        void OutputLine(string label, string opcode, string operand, string comment);

        /// <summary>
        /// Output a line of source code.  This will be output as-is.
        /// </summary>
        /// <param name="fullLine">Full text of line to outut.</param>
        void OutputLine(string fullLine);
    }

    /// <summary>
    /// Enumeration of quirky or buggy behavior that GenCommon needs to handle.
    /// </summary>
    public class AssemblerQuirks {
        /// <summary>
        /// Are the arguments to MVN/MVP reversed?
        /// </summary>
        public bool BlockMoveArgsReversed { get; set; }

        /// <summary>
        /// Are 8-bit constant args to MVN/MVP output without a leading '#'?
        /// </summary>
        public bool BlockMoveArgsNoHash { get; set; }

        /// <summary>
        /// Does the assembler configure assembler widths based on SEP/REP, but doesn't
        /// track the emulation bit?
        /// </summary>
        public bool TracksSepRepNotEmu { get; set; }

        /// <summary>
        /// Is the assembler unable to generate relative branches that wrap around banks?
        /// (Note this affects long-distance BRLs that don't appear to wrap.)
        /// </summary>
        public bool NoPcRelBankWrap { get; set; }

        /// <summary>
        /// Is the assembler implemented as a single pass?  (e.g. cc65)
        /// </summary>
        public bool SinglePassAssembler { get; set; }

        /// <summary>
        /// Is the assembler's label width determination performed only in the first pass,
        /// and not corrected when the actual width is determined?
        /// </summary>
        public bool SinglePassNoLabelCorrection { get; set; }
    }
}