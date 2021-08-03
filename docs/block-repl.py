#!/usr/bin/python3
#
# Replace blocks of text in an HTML document with the contents of a file.
#
# For example:
#   <div id="masthead">
#       <!-- START: /masthead-incl.html -->
#       <script>$("#masthead").load("/masthead-incl.html");</script>
#       <!-- END: /masthead-incl.html -->
#   </div>
#
# Only the lines between START/END are replaced.  The START/END lines are
# left in place so the process can be performed repeatedly without needing
# a separate ".in" file.
#
#
# Run this from the top-level directory.  Provide a list of all HTML files
# on the command line, using relative paths.
#
# If the name of the file to include (specified on the START line) begins
# with '/', the file will be loaded from the top-level directory, i.e. the
# one where this command was run.  If not, the file will be read from the
# directory where the HTML file lives.
#
#  find . -name '*.html' -print0 | xargs -0 ./block-repl.py
#
# (Tested with Python v3.8.5)
#
# Copyright 2021 Andy McFadden.
#

import filecmp
import os.path
import re
import sys

class LocalError(Exception):
    """Errors generated internally"""
    pass

# Regex pattern for block substitution.  There are three groups:
#  0: (full match)
#  1: START filename
#  2: "active" id (optional)
#  3: middle data (to be replaced)
#  4: END filename
# The START/END names are expected to match.  If not, we probably found the
# end of a different block, and should tell the user that something is off.
findChunk = re.compile(
    "^\s*<!--\s*START:\s*([\w/\-\.]+)\s*"
    "(?:active:#([\w\-]*))?\s*-->\s*$."
    "(.*?)"
    "^\s*<!--\s*END:\s*([\w/\-\.]+)\s*-->",
    re.DOTALL | re.MULTILINE)
GROUP_ALL = 0
GROUP_START = 1
GROUP_ACTIVE_ID = 2
GROUP_CHUNK = 3
GROUP_END = 4


def editFile(inFileName, outFileName):
    """ Edit a file, replacing blocks with the contents of other files. """

    try:
        with open(inFileName, "r") as inFile:
            fileData = inFile.read()
        outFile = open(outFileName, "x")
    except IOError as err:
        raise LocalError(err)

    # For each chunk found, replace the contents.
    startPos = 0
    while True:
        match = findChunk.search(fileData, startPos)
        if not match:
            break

        startTag = match.group(GROUP_START)
        endTag = match.group(GROUP_END)
        if startTag != endTag:
            raise LocalError("START/END tag mismatch: " + startTag + " vs. " + endTag)

        replSpan = match.span(GROUP_CHUNK)

        chunk = fileData[replSpan[0] : replSpan[1]]
        print("== Matched {0}:{1}".format(replSpan[0], replSpan[1]))


        activeId = match.group(GROUP_ACTIVE_ID)
        if activeId:
            print("== active ID: " + activeId)

        # copy everything up to the chunk
        outFile.write(fileData[startPos : replSpan[0]])
        # insert the file, tweaking active ID if appropriate
        copyFromIncl(inFileName, startTag, activeId, outFile)
        # copy the rest of the match
        outFile.write(fileData[replSpan[1] : match.end(GROUP_ALL)])

        # Start next search at end of full search.
        startPos = match.end(GROUP_ALL)
        print("== Start next at {0}".format(startPos))

    print("== done")
    outFile.write(fileData[startPos:])
    outFile.close()


def copyFromIncl(inFileName, tag, activeId, outFile):
    """
    Copy include file in, substituting active ID and path variables when
    appropriate.
      inFileName: relative path to file we're working on
      tag: name of file to include (absolute or relative to inFileName)
      activeID: ID to mark as active
      outFile: file object to write data to
    """

    inFileDir = os.path.dirname(inFileName)
    if tag[0] == '/':
        # file is in top-level (current) directory
        inclFileName = tag[1:]
    else:
        # file is in same directory as input file
        inclFileName = os.path.join(inFileDir, tag)

    print("== replacing section with " + inclFileName)

    try:
        # Use utf-8-sig to skip over Byte Order Marks (BOM).
        with open(inclFileName, "r", encoding="utf-8-sig") as inFile:
            fileData = inFile.read()
    except IOError as err:
        raise LocalError(err)

    # Create a relative path for ${ROOT}, which is defined as the directory
    # in which we're running.  The path gets us from the input file's
    # directory back to the root.
    # TODO: make this work correctly for absolute paths?
    if inFileName[0] == '/':
        raise LocalError("Not a relative path: " + inFileName)
    tmpPair = os.path.split(inFileName)
    rootRel = ""
    while tmpPair[0]:
        if tmpPair[0] != ".":
            # ignore leading "./", which you get from find+xargs
            rootRel += "../"
        tmpPair = os.path.split(tmpPair[0])

    # TODO: consider having a ${LOCAL} ...
    # Create a relative path for ${LOCAL}, which is defined as the directory
    # from which the input file was loaded.  The path gets us from the input
    # file's directory back to the included file's directory
    #
    # Suppose we're working on foo/bar/index.html, which includes
    # ../incl-sidenav.html (i.e. foo/incl-sidenav.html).  We want references
    # to map ${LOCAL}/bar/glob.html to "glob.html", and
    # ${LOCAL}/splat/index.html to "../splat/index.html".  For now, we always
    # use a local offset, so it's actually "../bar/glob.html".  This is a step
    # up from ${ROOT}, which would be "../../foo/bar/glob.html", but not as
    # clever as we could be.
    #
    # What we really want to do is extract the string that follows ${ROOT}
    # from the input file and compute the minimal path, but that requires
    # more effort than a simple variable substitution.  ${LOCAL} would be a
    # half-step, and probably not worth the effort.

    if activeId:
        # Given an HTML block like <li id="sidenav-moving-around">, insert
        # a class assignment: class="active".  The ID to modify is
        # specified by "activeId".
        pattern = 'id="' + re.escape(activeId) + '"'
        repl = 'id="' + activeId + '" class="active"'
        newData = re.sub(pattern, repl, fileData)
        if newData == fileData:
            print("== active ID '" + activeId + "' not found")
        else:
            fileData = newData

    # Replace ${ROOT} with relative path to root directory.
    newData = re.sub("\${ROOT}\/", rootRel, fileData)
    if newData != fileData:
        #print("== ${ROOT}=" + rootRel + " in " + inclFileName)
        fileData = newData

    # Copy data to output file.
    outFile.write(fileData)


def main():
    """ main """

    fileNames = sys.argv[1:]
    outFileName = None

    try:
        for name in fileNames:
            print("Processing: " + name)
            outFileName = name + "_NEW"
            editFile(name, outFileName)

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
