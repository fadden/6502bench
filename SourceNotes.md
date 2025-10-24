# 6502bench Source Code Notes # 

All of the code is written in C# .NET, using the (free to download) Visual
Studio Community 2022 IDE as the primary development environment.  The user
interface uses the WPF API, targeted at the final release of .NET
Framework (4.8.1).  To build the sources, clone the git repository and open
"WorkBench.sln" in Visual Studio 2019 or later.  The Solution file is called
"WorkBench.sln" rather than "6502bench.sln" because some things in
Visual Studio got weird when it didn't start with a letter.

When installing Visual Studio, be sure to include ".NET Desktop Development".
You may also need to install the .NET Framework 4.8.1 "Dev Pack" (as a
separate download, or via the "individual components" tab in the
Visual Studio Installer).

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

## Publishing a New Release ##

Steps:

 1. Update the version number in `SourceGen/App.xaml.cs`.
 2. Run Debug &gt; Source Generation Tests to verify that the code generation
    tests pass.  This requires that all cross-assemblers be installed and
    configured.
 3. Remove any existing `DIST_Release` directory from the top level.
 4. In Visual Studio, change the build configuration to Release, and the
	startup project to MakeDist.
 5. Do a full clean build.
 6. Hit F5 to start MakeDist.  Click "Build" to generate a release build.  The
	files will be copied into `DIST_Release`.
 7. Create an empty ZIP file (e.g. `6502bench1_10_0-alpha1.zip`).
 8. Copy all files from `DIST_Release` into it.
 9. Submit all changes to git, push them to the server.
 10. Create a new release on github.  Drag the ZIP file into it.
 11. Update/close any issues that have been addressed by the new release.
	 Add the change notes to the wiki page.

Version numbers should follow the semantic versioning scheme: v1.2.3,
v1.2.3-dev1, etc.
