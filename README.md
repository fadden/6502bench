# 6502bench #

6502bench is a code development "workbench" for 6502, 65C02, and 65802/65816
code.  It currently features one tool, the SourceGen disassembler.

You can download the source code and build it yourself, or click the
"Releases" tab near the top of the github page for pre-built downloads.


## SourceGen ##

SourceGen converts machine-language programs to assembly-language source
code.  It has most of the features you will find in other 6502 disassemblers,
as well as many less-common ones.

A demo video is available: https://youtu.be/dalISyBPQq8

#### Features ####

Analysis:
- Support for 6502 (including undocumented opcodes), 65C02, and 65816.
- Code flow analyzer traces execution to find all reachable
  instructions.  Hinting mechanism allows manual specification of
  entry points.
- Processor status flags are tracked, allowing automatic detection
  of branch-always and branch-never, as well as register widths on
  16-bit CPUs.  The analyzer tracks these across subroutine calls and
  branches.  Cycle counts factor these in automatically.
- Editable labels are generated for every branch and data target.
- Automatic detection and classification of ASCII strings and runs of
  identical bytes.
- All target-platform-specific stuff is stored in plain text files
  loaded at runtime.  This includes symbols for ROM entry points
  and standard zero-page locations.  Additional symbols and overrides
  can be specified at the project level.
- Extension scripts can be used to reformat code and identify inline
  data that follows JSR/JSL.  Code is compiled at run time and
  executes in a sandbox.

UI:
- Fully interactive point-and-click GUI.  Add labels and multi-line
  comments, change addresses, and see the changes immediately.
- Instruction operand formats (hex, decimal, etc) can be set for
  individual lines.  Symbols that don't match the operands are automatically
  offset, allowing simple expressions like "address - 1".
- Full-line comments are automatically word-wrapped, and can be
  "boxed" for an authentic retro feel.
- Data areas can be formatted as bytes, words, addresses, and more.
  Several types of strings are recognized (null-terminated, length
  prefixed, etc).
- "Infinite" undo/redo of all actions.
- Notes can be added that aren't included in generated output.  Very
  useful for marking up a work in progress.
- Cross-reference tables are generated for every branch and data
  target address, as well as for external platform symbols.
- Instruction summaries, including cpu cycles and flags modified, are
  shown along with a description of the opcode function.
- Display is configurable for upper/lower case, choice of pseudo-op
  names, expression formats, and more.

Output:
- Assembly source can be generated for multiple assemblers (currently
  cc65 and Merlin 32).
- Cross-assemblers can be launched directly from the generation GUI to
  verify output correctness.
- Optional automatic conversion of labels from global to local.
- Symbols may be exported from one project and imported into another
  to facilitate multi-binary disassembly.

Misc:
- Preset project attributes (CPU type, platform symbol file sets) are defined
  for a variety of platforms.
- Project file is stored in a text format, and only holds metadata.  None
  of the original file is included, allowing the project file to be shared
  without violating copyrights (note: may vary depending on local laws).

There are a couple of significant areas where support is currently lacking:
- Poor support for multi-bank 65816 files (IIgs OMF, SNES).
- No support for alternate character sets (e.g. PETSCII).


## About the Code ##

All of the code is written in C# .NET, using the (free to download) Visual
Studio Community 2017 IDE as the primary development environment.  The user
interface uses the WinForms API.  Efforts have been made to avoid doing
anything Windows-specific, in the hope that the applications will be
straightforward to port to other platforms.

The solution is called "WorkBench.sln" rather than "6502bench.sln" because
some things in Visual Studio got weird when it didn't start with a letter.

The code style is closer to what Android uses than "standard" C#.  Lines
are folded to fit 100 columns.

The source code is licensed under Apache 2.0
(http://www.apache.org/licenses/LICENSE-2.0), which makes it free for use in
both open-source programs and closed-source commercial software.  The license
terms are similar to BSD or MIT, but with some additional constraints on
patent licensing.  (This is the same license Google uses for the Android
Open Source Project.)

Images are licensed under Creative Commons ShareAlike 4.0 International
(https://creativecommons.org/licenses/by-sa/4.0/).

