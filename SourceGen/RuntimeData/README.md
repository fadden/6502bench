# Runtime Data #

Symbol files and analyzer scripts are split into directories by
platform manufacturer.

The Visual Studio project (RuntimeData.csproj) exists so you can edit
scripts with IntelliSense and error highlighting.  Everything here is
distributed as source, not in compiled form; all compilation occurs at
run time.

## SystemDefs.json ##

This file defines the systems available in the "new project" screen.
The following fields are mandatory:

 * Name - Short name that identifies the system.
 * GroupName - Short string used to group common items together in the UI.
 * CPU - Type of CPU used.  The string must be part of the known set
    (see CpuDef.cs)
 * Speed - Clock rate, in MHz, of the CPU on the system.  When multiple
    speeds are possible, use the most common, favoring NTSC over PAL.
 * SymbolFiles - List of platform symbol file identifiers (see below).
 * ExtensionScripts - List of extension script file identifiers (see below).
 * Parameters - List of optional parameters (see below).

The currently-supported parameters are:

 * load-address=<addr> - Specify the initial load address.  The default
   is 0x1000.
 * entry-flags=<flag-set> - Specify the processor status flag values to
   use at entry points.  This is intended for use on the 65802/65816, and
   may be one of "emulation", "native-short", and "native-long".  The
   default is "emulation".
 * undocumented-opcodes={true|false} - Enable or disable undocumented
   opcodes.  They are disabled by default.

All of these things can be changed after the project has begun, but it's
nice to have them configured in advance.

SymbolFiles and ExtensionScripts use file identifiers, which look like
"RT:Apple/ProDOS8.sym65".  The "RT:" means that the file lives in the
RuntimeData directory, and the rest is a partial pathname.  Files that
live in the same directory as the project file are prefixed with "PROJ:".
All symbol files and extension scripts must live in the RuntimeData
directory or project file directory, or they will not be loaded.

All "RT:" identifier paths are relative to the RuntimeData directory. The
Group Name is not automatically added.


## Platform Symbol Files (.sym65) ##

These contain lists of symbols, each of which has a label and a value.
If two symbols have the same value, the older one is discarded.

Blank lines, and lines that begin with a semicolon (';'), are ignored.

Lines that begin with an asterisk are command lines.  Two commands are
currently defined:

 - *SYNOPSIS - a short summary of the file contents.
 - *TAG - a tag string to apply to all following symbols.
 
Tags can be used by extension scripts to identify a subset of symbols.
The symbols are still part of the global set; the tag just provides a
way to extract a subset.  Tags should be comprised of non-whitespace ASCII
characters.  Tags are global, so use a long descriptive string.  If *TAG
is not followed by a string, the tag is cleared.

All other lines are expected to have the form:

  symbol {=|@} value [;comment]

Symbols must be at least two characters long, begin with a letter or
underscore, and consist entirely of alphanumeric ASCII characters
(A-Z, a-z, 0-9) and the underscore.  Different assemblers have different
limitations on symbols, but all reasonable assemblers will accept these.

Use '@' for address values, and '=' for constants.  The only real difference
is that address values will be applied automatically to operands that
reference addresses outside the scope of the file.

The value is a number in decimal, hexadecimal (with a leading '$'), or
binary (with a leading '%').  The numeric base will be saved and used when
formatting the symbol in the generated output, so use whichever form is
most appropriate.  Values are unsigned 24-bit numbers.

The comment is optional.  If present, it will be saved and used as the
end-of-line comment on the equate directive if the symbol is used.


## Extension Scripts ##

See "ExtensionScripts.md".
