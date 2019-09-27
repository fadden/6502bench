# 6502bench # 

[Features](#key-features) - [Installation](#installation) - [Getting Started](#getting-started) - [About the Code](#about-the-code)

[6502bench](https://6502bench.com/) is a code development "workbench"
for 6502, 65C02, and 65802/65816 code.  It currently features one tool,
the SourceGen disassembler, and runs on Windows 7 or later.

You can download the source code and build it yourself, or click the
[Releases tab](https://github.com/fadden/6502bench/releases) for
downloads with pre-built binaries.


## SourceGen ##

SourceGen converts machine-language programs to assembly-language source
code.  It has most of the features you will find in other 6502 disassemblers,
as well as many less-common ones.

### Key Features ###

- Fully interactive point-and-click GUI.  Define labels, set addresses,
  add comments, and see the results immediately.  Add multi-line comments
  and have them word-wrapped automatically.
- The disassembly engine traces code execution, automatically finding all
  instructions reachable from a given starting point. Changes to the
  processor status flags are tracked, allowing identification of branches
  that are always/never taken, accurate cycle count listings, and correct
  analysis of 65816 code with variable-width registers.
- Easy generation of assembly source code for popular cross-assemblers
  (currently 64tass, ACME, cc65, and Merlin 32). Cross-assemblers can be
  invoked from the GUI to verify correctness.
- Symbols and constants are provided for ROM and operating system entry
  points on several popular systems.
- Project files are designed for sharing and collaboration.

A demo video is [available on YouTube](https://youtu.be/dalISyBPQq8).

#### Additional Features ####

Analyzer:
- Support for 6502, 65C02, and 65816, including undocumented opcodes.
- Hinting mechanism allows manual identification of code, data, and inline
  data.
- Editable labels are generated for every branch destination and data target.
- Automatic detection and classification of character strings and runs of
  identical bytes.
- Symbol files for ROM entry points, operating system constants, and other
  platform-specific data are stored in plain text files loaded at runtime.
- Extension scripts can be defined that automatically reformat code and
  identify inline data that follows a JSR, JSL, or BRK.

User interface:
- "Infinite" undo/redo of all operations.
- Cross-reference tables are generated for every branch and data target
  address, as well as for external platform symbols.
- Instruction operand formats (hex, decimal, binary, character, symbol) can
  be set for individual instructions. References to nearby symbols are
  offset, allowing simple expressions like "addr + 1".
- Data areas can be formatted in various formats, including individual
  bytes, 16-bit and 24-bit words, addresses, or strings.  Multiple
  character encodings are supported, including ASCII, high ASCII,
  C64 PETSCII, and C64 screen codes.
- Zero-page variables can be given different labels at different points
  in the program.
- Multi-line comments can be "boxed" for an authentic retro feel.
- Notes can be added that aren't included in generated output. These also
  function as color-coded bookmarks. Very useful for marking up a work in
  progress.
- Instruction summaries, including CPU cycles and flags modified, are shown
  along with a description of the opcode's function.
- Various aspects of the code display can be reconfigured, including
  upper/lower case, pseudo-opcode naming, and expression formats. These
  choices are not part of the project definition, so everyone can view a
  project according to their own personal preferences.

Code generation:
- Labels can be coaxed from global to local as allowed by the assembler.
- Symbols may be exported from one project and imported into another to
  facilitate multi-binary disassembly.
- Listings can be generated in HTML form for publication on the web.
  Many aspects of the output format can be configured.

Miscellaneous:
- All data files are stored in text formats (primarily JSON).
- The project file includes nothing from the data file but a CRC. This may
  allow the project to be shared without violating copyrights (subject to
  local laws).

#### Limitations ####

Support for 65816 code is largely complete, lacking only a way to specify
the data bank register.  However, the current user interface tries to
keep everything in a single list, which works poorly for multi-bank binaries.

To learn about other areas for improvement, visit the wiki section for the
[current "TO DO" list](https://github.com/fadden/6502bench/wiki/TO-DO-List).

To learn about the past, check the
[change log](https://github.com/fadden/6502bench/wiki/Change-Log).


## Installation ##

There is currently no installer -- just unzip the archive and run the
"SourceGen.exe" executable.  The data files used by the program are found
automatically in the directory where the .EXE lives.

You need to have Microsoft .NET Framework v4.6.2 or later installed.  Most
people will already have this.  If SourceGen doesn't seem to want to start,
download the latest version (currently v4.7.2)
[directly from Microsoft](https://www.microsoft.com/net/download/dotnet-framework-runtime).
The framework requires Win7 SP1, Win8.1, or Win10 updated through at least
the Anniversary Update (1607).  (One user who had trouble with the 4.7.2
installer was able to get the 4.6.2 installer to work.)

SourceGen does not currently run on Linux or Mac OS X.  Versions 1.0 and 1.1
used the WinForms API, which has been implemented for Mono, but after
encountering significant bugs that I wasn't able to work around I abandoned
the idea and switched to WPF.  Besides working better under Windows, WPF
uses a more modern approach (XAML) that may ease the transition to a modern
cross-platform GUI like Avalonia.


## Getting Started ##

The best way to get started is by working through the tutorial.  Launch
SourceGen, hit F1 to open the user manual in your web browser, then look
for the Tutorial link in the index.  Click it and follow the instructions
there.

I strongly recommend doing this.  Some aspects of SourceGen are non-obvious.

The tutorial is one of several examples included in the SourceGen
distribution.  The other directories contain project and data files for
completed disassembly projects alongside the original source code, allowing
a direct comparison between how the code was written and how SourceGen can
display it.


## About the Code ##

All of the code is written in C# .NET, using the (free to download) Visual
Studio Community 2017 IDE as the primary development environment.  The user
interface uses the WPF API.

The Solution file is called "WorkBench.sln" rather than "6502bench.sln"
because some things in Visual Studio got weird when it didn't start with a
letter.

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

