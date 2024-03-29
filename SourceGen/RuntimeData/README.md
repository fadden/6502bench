# Runtime Data #

Symbol files and extension scripts are split into directories by
platform manufacturer.  General-purpose scripts are in the `Common`
directory.

A Visual Studio project (`RuntimeData.csproj`) exists in the source tree
so you can edit platform-specific scripts with IntelliSense and error
highlighting.  Everything in this directory is distributed as source,
not in compiled form; all compilation occurs at run time.

The `Tips` directory holds the files used by the "daily tips" feature,
and `TestData` has some files used by the regression tests.

## SystemDefs.json ##

This file defines the systems available in the "new project" screen.
The following fields are mandatory:

 * Name - Short name that identifies the system.
 * GroupName - Short string used to group common items together in the UI.
 * CPU - Type of CPU used.  The string must be part of the known set
    (see CpuDef.cs).
 * Speed - Clock rate, in MHz, of the CPU on the system.  When multiple
    speeds are possible, use the most common, favoring NTSC over PAL.
 * SymbolFiles - List of platform symbol file identifiers (see below).
 * ExtensionScripts - List of extension script file identifiers (see below).
 * Parameters - List of optional parameters (see below).

The currently-supported parameters are:

 * `load-address=<addr>` - Specify the initial load address.  The default
   is 0x1000.
 * `entry-flags=<flag-set>` - Specify the processor status flag values to
   use at entry points.  This is intended for use on the 65802/65816, and
   may be one of `emulation`, `native-short`, and `native-long`.  The
   default is `emulation`.
 * `undocumented-opcodes={true|false}` - Enable or disable undocumented
   opcodes.  They are disabled by default.
 * `first-word-is-load-addr={true|false}` - If true, the first two bytes of
   the file contain the load address.
 * `default-text-encoding=<mode>` - Specify default character encoding.
   Use `c64-petscii` for PETSCII.  The default is low/high ASCII.

All of these things can be changed after the project has begun, but it's
nice to have them configured in advance.

SymbolFiles and ExtensionScripts use file identifiers, which look like
"`RT:Apple/ProDOS8.sym65`".  The "RT:" means that the file lives in the
RuntimeData directory, and the rest is a partial pathname.  Files that
live in the same directory as the project file are prefixed with "PROJ:".
All symbol files and extension scripts must live in the RuntimeData
directory or project file directory, or they will not be loaded.

All "RT:" identifier paths are relative to the RuntimeData directory. The
GroupName string is not automatically added.

## Platform Symbol Files and Extension Scripts ##

The format of these files is described in detail in the "Advanced Topics"
section of the manual ([here](../../docs/sgmanual/advanced.html)).

## Misc Files ##

`ExportTemplate.html` and `SGStyle.css` are used by SourceGen's HTML export
feature.

`LegalStuff.txt` is displayed in the About box.
