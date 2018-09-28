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
using System.ComponentModel;

using Asm65;

namespace SourceGen.AsmGen {
    /// <summary>
    /// Common interface for generating assembler-specific source code.
    /// </summary>
    public interface IGenerator {
        /// <summary>
        /// Configure generator.  Must be called before calling any other method or using
        /// properties.
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

        LabelLocalizer Localizer { get; }

        /// <summary>
        /// Generates source files on a background thread.  Method must not make any UI calls.
        /// </summary>
        /// <param name="worker">Async work object, used to report progress updates and
        ///   check for cancellation.</param>
        /// <returns>List of pathnames of generated files.</returns>
        List<string> GenerateSource(BackgroundWorker worker);

        /// <summary>
        /// Provides an opportunity for the assembler to replace a mnemonic with another.  This
        /// is primarily intended for undocumented ops, which don't have standard mnemonics,
        /// and hence can vary between assemblers.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="mnemonic"></param>
        /// <returns>Replacement mnemonic, an empty string if the original is fine, or
        ///   null if the op is not supported at all and should be emitted as hex.</returns>
        string ReplaceMnemonic(OpDef op);

        /// <summary>
        /// Generates an opcode/operand pair for a short sequence of bytes (1-4 bytes).
        /// Does not produce any source output.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="opcode"></param>
        /// <param name="operand"></param>
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
        /// <param name="offset"></param>
        void OutputDataOp(int offset);

        /// <summary>
        /// Outputs an equate directive.  The numeric value is already formatted.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="valueStr"></param>
        /// <param name="comment"></param>
        void OutputEquDirective(string name, string valueStr, string comment);

        /// <summary>
        /// Outputs a code origin directive.
        /// </summary>
        /// <param name="address"></param>
        void OutputOrgDirective(int address);

        /// <summary>
        /// Notify the assembler of a change in register width.
        /// 
        /// Merlin32 always sets both values (e.g. "MX %00"), cc65 sets each register
        /// individually (".A16", ".I8").  We need to accommodate both styles.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="prevM"></param>
        /// <param name="prevX"></param>
        /// <param name="newM"></param>
        /// <param name="newX"></param>
        void OutputRegWidthDirective(int offset, int prevM, int prevX, int newM, int newX);

        /// <summary>
        /// Output a line of source code.  All elements must be fully formatted.  The
        /// items will be padded with spaces to fit specific column widths.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="opcode"></param>
        /// <param name="operand"></param>
        /// <param name="comment"></param>
        void OutputLine(string label, string opcode, string operand, string comment);

        /// <summary>
        /// Output a line of source code.
        /// </summary>
        /// <param name="fullLine"></param>
        void OutputLine(string fullLine);
    }

    public class AssemblerQuirks {
        public bool BlockMoveArgsReversed { get; set; }
        public bool TracksSepRepNotEmu { get; set; }
        public bool NoPcRelBankWrap { get; set; }
    }
}