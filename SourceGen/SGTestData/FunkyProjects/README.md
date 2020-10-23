# Funky SourceGen Projects #

This is a collection of project files that aren't quite right.  Some of
them will load with warnings, others will fail at different points.  They
can be used to manually exercise the various project-load failure modes.

Normally project files use the name of the data file, with ".dis65" added.
All of the projects use the same data file ("Simple"), so it will be
necessary to specify the data file manually for most of them (you will be
prompted to do so).

The files are:

 * Simple.dis65 : A trivial but correct project.
 * Simple-BadATag.dis65 : An analyzer tag claims to cover a range outside
   the file bounds.  The program should tell you that it's discarding the
   bad tags and continuing.
 * Simple-BadCRC.dis65 : The data file CRC stored in the project file does
   not match the data file contents.  The program should tell you this and
   offer to let you locate the correct file.
 * Simple-BadDescLen.dis65 : An operand format descriptor covers a range
   off the end of the file.  THe program should tell you that it's discarding
   the bad format and continuing.
 * Simple-BadJSON.dis65 : Garbage has been inserted into the JSON data
   stream.  The project load should fail with an appropriate message.
 * Simple-BadLen.dis65 : The data file length stored in the project file
   does not match the data file contents.  The program should tell you this
   and offer to let you locate the correct file.
 * Simple-BadMagic.dis65 : The "magic number" at the start of the project
   file has been damaged.  The project load should fail with an
   appropriate message.
 * Simple-DupLabel.dis65 : More than one line has the same label.  You
   should be warned that the duplicates are being stripped away.
 * Simple-FutureVersion.dis65 : The project has a content-version higher
   than the application.  You should see a warning to that effect.
 * Simple-MissingPlatSym.dis65 : One of the platform symbol files listed in
   the project file does not exist.  You should be notified of the problem
   and loading should continue.  (Furthermore, if you look in the project
   settings, the missing project file should still be present.)
 * Simple-TooShort.dis65 : Same as Simple-BadMagic, but tests to see if we
   choke when the file is shorter than the magic string.

 * ZeroLengthFile : This is intended for use with "new project".  The
   application should refuse to create a new project for a zero-length file.

