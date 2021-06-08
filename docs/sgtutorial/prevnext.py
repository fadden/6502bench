#!/usr/bin/python3
#
# Sets the contents of the "prevnext" sections, which have the prev/next
# buttons.  Run this in the directory where the HTML files live.
#
# (This is currently just for the sgtutorial files; it's not written to
# be a general-purpose utility.)
#

import filecmp
import os.path
import re
import sys

# List of files to process, in order.
gFileList = [
    "about-disasm.html",
    "using-sourcegen.html",
    "moving-around.html",
    "simple-edits.html",
    "labels-symbols.html",
    "editing-data.html",
    "generating-code.html",
    "digging-deeper.html",
    "string-formatting.html",
    "local-variables.html",
    "inline-data.html",
    "odds-ends.html",
    "advanced-topics.html",
    "address-tables.html",
    "extension-scripts.html",
    "visualizations.html",
]

class LocalError(Exception):
    """Errors generated internally"""
    pass

# Regex pattern for prevnext section.
findChunk = re.compile(
    '^\s*<div id="prevnext">\s*$.'
    '(.*?)'
    '^\s*<\/div>',
    re.DOTALL | re.MULTILINE)
GROUP_ALL = 0
GROUP_CHUNK = 1


def editFile(index, outFileName):
    """ Edit a file, replacing blocks with the contents of other files. """

    inFileName = gFileList[index]
    try:
        with open(inFileName, "r") as inFile:
            fileData = inFile.read()
        outFile = open(outFileName, "x")
    except IOError as err:
        raise LocalError(err)

    # Find first (and presumably only) matching chunk.
    match = findChunk.search(fileData)
    if not match:
        print("== No prevnext section found")
        return

    replSpan = match.span(GROUP_CHUNK)

    chunk = fileData[replSpan[0] : replSpan[1]]
    print("== Matched {0}:{1}".format(replSpan[0], replSpan[1]))


    # copy everything up to the chunk
    outFile.write(fileData[ : replSpan[0]])
    # insert the file, tweaking active ID if appropriate
    generatePrevNext(index, outFile)
    # copy the rest of the file, including the </div>
    outFile.write(fileData[match.end(GROUP_CHUNK) : ])

    print("== done")
    outFile.close()

    return


def generatePrevNext(index, outFile):
    """ Generate prev/next button HTML """

    # <a href="#" class="btn-previous">&laquo; Previous</a>
    # <a href="#" class="btn-next">Next &raquo;</a>
    if index > 0:
        outFile.write('    <a href="')
        outFile.write(gFileList[index - 1])
        outFile.write('" class="btn-previous">&laquo; Previous</a>\n')

    if index + 1 < len(gFileList):
        outFile.write('    <a href="')
        outFile.write(gFileList[index + 1])
        outFile.write('" class="btn-next">Next &raquo;</a>\n')

    return


def main():
    """ main """

    outFileName = None

    try:
        for index in range(len(gFileList)):
            name = gFileList[index]
            print("Processing #{0}: {1}".format(index, name))
            outFileName = name + "_NEW"
            editFile(index, outFileName)

            # See if the file has changed.  If it hasn't, keep the original
            # so the file dates don't change.
            if filecmp.cmp(name, outFileName, False):
                print("== No changes, removing new")
                os.remove(outFileName)
            else:
                print("== Changed, keeping new")
                os.rename(outFileName, name)
    except LocalError as err:
        print("ERROR: {0}".format(err))
        if outFileName:
            print("  check " + outFileName)
        sys.exit(1)

    sys.exit(0)


main()  # does not return
