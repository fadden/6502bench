# SourceGen Examples #

These are some sample projects you can play with.  The binaries are
accompanied by the original source code, so you can compare the SourceGen
project to the original.

 * Tutorial: simples project, intended for use with the tutorial in
   the manual.
 * Scripts: extension script samples.
 * A2-lz4fh: two functions for unpacking a simplified form of LZ4 compression.
   One is 6502, the other is 65816.  This is pretty straightforward.
   [(Full project)](https://github.com/fadden/fhpack)
 * A2-Amper-fdraw: 6502 code that provides an Applesoft BASIC interface
   to a machine-language graphics library.  The public interface of the
   graphics library is defined in a .sym65 file.  This example has multiple
   entry points in a jump table, and requires a bit more effort.
   [(Full project)](https://github.com/fadden/fdraw)
 * A2-Zippy: a program for controlling an Apple IIgs CPU accelerator card.
   65816 sources, with a little bit of ProDOS 8 and IIgs toolbox usage.
   Demonstrates how extension scripts are used during analysis.
   [(Full project)](https://fadden.com/apple2/misc.html#zippy)
 * A2-HP-CDA: HardPressed Classic Desk Accessory.  This is 65816 code
   in OMF loader format, which SourceGen doesn't support, so it's a little
   rough.  (This is chiefly here to answer the question, "what does it look
   like when you disassemble OMF binaries?")
   [(Full project)](https://fadden.com/apple2/hardpressed.html)

(You may be wondering why some of the example files have filenames with
things like "#061d60" in them.  It's a method of preserving the file type
for Apple II files used by some utilities.  The potential advantage for
disassembly is that the file type often determines the load address,
possibly removing some initial guesswork.)
