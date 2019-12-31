# 6502bench Source Code Notes # 

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


## SourceGen Points of Interest ##

The main window UI is in WpfGui/MainWindow.xaml[.cs].  Much of the
implementation lives in MainController.cs.

The top-level object for the project data is DisasmProject.cs.  The
Analyze() method drives the code and data analysis process.  ApplyChanges()
is the heart of the undo/redo system.

Source code generation and assembler execution is routed through
AsmGen/AssemblerInfo.cs.  If you want to add support for a new
cross-assembler, start by adding new entries to the enum and data
tables there.
