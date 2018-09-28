# Extension Script Reference #

Extension scripts, also called "plugins", are C# programs with access to
the full .NET Standard 2.0 APIs.  They're compiled at run time by SourceGen
and executed in a sandbox with security restrictions.

## Features ##

SourceGen defines an interface that plugins must implement, and an
interface that plugins can use to interact with SourceGen.  See
Interfaces.cs in the PluginCommon directory.  Bear in mind that this
feature is still evolving, and the interfaces may change significantly
in the near future.

The current interfaces can be used to identify inline data that follows
JSR/JSL instructions, and to format operands.  The latter can be useful for
replacing immediate load operands with symbolic constants.

Scripts may be loaded from the RuntimeData directory, or from the directory
where the project file lives.  Attempts to load them from other locations
will fail.

## Development ##

The easiest way to develop extension scripts is inside the 6502bench
solution in Visual Studio.  This way you have the interfaces available
for IntelliSense completion, and get all the usual syntax and compile
checking in the editor.  (This is why there's a RuntimeData project for
Visual Studio.)

If you have the solution configured for debug builds, SourceGen will set
the IncludeDebugInformation flag to true when compiling scripts.  This
causes a .PDB file to be created.

## Utility Functions ##

Some commonly useful functions are defined in the PluginCommon.Util class,
which is available to plugins.  These call into the CommonUtil library,
which is shared with SourceGen.

While plugins can use CommonUtil directly, they should avoid doing so.  The
APIs there are not guaranteed to be stable, so plugins that rely on them
may break in a subsequent release of SourceGen.

## PluginDll Directory ##

Extension scripts are compiled into .DLLs, and saved in the PluginDll
directory, which lives next to the application executable and RuntimeData.
If the extension script is the same age or older than the DLL, SourceGen
will continue to use the existing DLL.

The DLLs names are a combination of the script filename and script location.
The compiled name for "MyPlatform/MyScript.cs" in the RuntimeData directory
will be "RT_MyPlatform_MyScript.dll".  For a project-specific script, it
would look like "PROJ_MyProject_MyScript.dll".

The PluginCommon and CommonUtil DLLs will be copied into the directory, so
that code in the sandbox has access to them.

## Sandboxing ##

Extension scripts are executed in an App Domain sandbox.  App domains are
a .NET feature that creates a partition inside the virtual machine, isolating
code.  It still runs in the same address space, on the same threads, so the
isolation is only effective for "partially trusted" code that has been
declared safe by the bytecode verifier.

SourceGen disallows most actions, notably file access.  An exception is
made for reading files from the directory where the plugin DLLs live, but
scripts are otherwise unable to read or write from the filesystem.  (A
future version of SourceGen may provide an API that allows limited access
to data files.)

App domain security is not absolute.  I don't really expect SourceGen to
be used as a malware vector, so there's no value in forcing scripts to
execute in an isolated server process, or to jump through the other hoops
required to really lock things down.  I do believe there's value in
defining the API in such a way that we *could* implement full security if
circumstances change, so I'm using app domains as a way to keep everybody
honest.
