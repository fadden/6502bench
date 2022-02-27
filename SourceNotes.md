# 6502bench Source Code Notes # 

All of the code is written in C# .NET, using the (free to download) Visual
Studio Community 2022 IDE as the primary development environment.  The user
interface uses the WPF API, targeted at .NET Framework 4.6.2.  When
installing Visual Studio, be sure to include ".NET Desktop Development".
You may also need to install the .NET Framework 4.6.2 "Dev Pack" (as a
separate download, or via the "individual components" tab in the
Visual Studio Installer).

The Solution file is called "WorkBench.sln" rather than "6502bench.sln"
because some things in Visual Studio got weird when it didn't start with a
letter.

The code style is closer to what Android uses than "standard" C#.  Lines
are folded to fit 100 columns.


## SourceGen Points of Interest ##

Places to start...

The main window UI is in WpfGui/MainWindow.xaml[.cs].  Much of the
implementation lives in MainController.cs.

The top-level object for the project data is DisasmProject.cs.  The
Analyze() method drives the code and data analysis process.  ApplyChanges()
is the heart of the undo/redo system.

Source code generation and assembler execution is routed through
AsmGen/AssemblerInfo.cs.  If you want to add support for a new
cross-assembler, start by adding new entries to the enum and data
tables there.

Nothing specific to a target system is baked into the main application.  The
SourceGen/RuntimeData directory has the system definitions used for the
"new project" list, along with subdirectories with symbol files and extension
scripts.  The [README file there](SourceGen/RuntimeData/README.md)
explains a bit more.
