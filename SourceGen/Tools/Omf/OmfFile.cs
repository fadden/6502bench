/*
 * Copyright 2020 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;

using Asm65;

namespace SourceGen.Tools.Omf {
    /// <summary>
    /// Apple IIgs OMF file.
    /// </summary>
    /// <remarks>
    /// OMF files are a series of segments.  There is no file header or identifying information.
    /// In some cases the length is expected to be a multiple of 512 bytes, in others it isn't.
    /// Each segment is comprised of a header, followed by a series of records.
    ///
    /// There's no structural limitation on mixing and matching segments, whether different
    /// versions or different types.  The file format provides a structure in which various
    /// things may be stored, but does not provide a way to tell an observer what is contained
    /// within (the ProDOS file type is supposed to do that).  A given file may be a Load
    /// file (handled by the System Loader), Object file (fed to a linker), Library file
    /// (also fed to a linker), or Run-Time Library (used by both the linker and the loader).
    ///
    /// References:
    /// - IIgs Orca/M 2.0 manual.  Appendix B documents OMF v0, v1, and v2.1 Load files.
    ///   (This is included with Opus ][.)
    /// - "Apple IIgs Programmer's Workshop Reference".  Chapter 7, page 228, describes
    ///   OMF v1.0 and v2.0.
    /// - "Apple IIgs GS/OS Reference, for GS/OS System Software Version 5.0 and later".
    ///   Appendix F describes OMF v2.1, and Chapter 8 has some useful information about
    ///   how the loader works (e.g. page 205).
    /// - "Undocumented Secrets of the Apple IIGS System Loader" by Neil Parker,
    ///   http://nparker.llx.com/a2/loader.html .  Among other things it documents the
    ///   contents of ExpressLoad segments, which I haven't found in an official reference.
    /// - Apple IIgs Tech Note #66, "ExpressLoad Philosophy".
    ///
    /// Related:
    /// - https://www.brutaldeluxe.fr/products/crossdevtools/omfanalyzer/
    /// - https://github.com/fadden/ciderpress/blob/master/reformat/Disasm.cpp
    /// </remarks>
    public class OmfFile {
        public const int MIN_FILE_SIZE = OmfSegment.MIN_HEADER_V0;
        public const int MAX_FILE_SIZE = (1 << 24) - 1;                 // cap at 16MB


        /// <summary>
        /// Overall file contents, determined by analysis.
        /// </summary>
        public enum FileKind {
            Unknown = 0,
            Load,               // loadable files
            Object,             // output of assembler/compiler, before linking
            Library,            // static code library
            RunTimeLibrary,     // dynamic shared library
            Indeterminate,      // valid OMF, but type is indeterminate
            Foreign             // not OMF, or not IIgs OMF
        }
        public FileKind OmfFileKind { get; private set; }

        /// <summary>
        /// File data.  Files must be loaded completely into memory.
        /// </summary>
        private byte[] mFileData;

        /// <summary>
        /// List of segments.
        /// </summary>
        public List<OmfSegment> SegmentList { get; private set; } = new List<OmfSegment>();

        /// <summary>
        /// List of strings to show in the message window.
        /// </summary>
        public List<string> MessageList { get; private set; } = new List<string>();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileData">File to analyze.</param>
        public OmfFile(byte[] fileData) {
            Debug.Assert(fileData.Length >= MIN_FILE_SIZE && fileData.Length <= MAX_FILE_SIZE);
            mFileData = fileData;

            OmfFileKind = FileKind.Unknown;
        }

        /// <summary>
        /// Analyzes the contents of an OMF file.
        /// </summary>
        public void Analyze(Formatter formatter) {
            OmfSegment.ParseResult result = DoAnalyze(formatter, false);
            if (result == OmfSegment.ParseResult.IsLibrary ||
                    result == OmfSegment.ParseResult.Failure) {
                // Failed; try again as a library.
                List<string> firstFail = new List<string>(MessageList);
                result = DoAnalyze(formatter, true);
                if (result == OmfSegment.ParseResult.Failure) {
                    // Failed both ways.  Report the failures from the non-library attempt.
                    MessageList = firstFail;
                }
            }

            OmfFileKind = DetermineFileKind();
            Debug.WriteLine("File kind is " + OmfFileKind);

            if (OmfFileKind == FileKind.Load) {
                GenerateRelocDicts();
            }
        }

        /// <summary>
        /// Analyzes the contents of an OMF file as a library or non-library.
        /// </summary>
        private OmfSegment.ParseResult DoAnalyze(Formatter formatter, bool parseAsLibrary) {
            bool first = true;
            int offset = 0;
            int len = mFileData.Length;

            List<string> msgs = new List<string>();

            while (len > 0) {
                OmfSegment.ParseResult result = OmfSegment.ParseHeader(mFileData, offset,
                    parseAsLibrary, msgs, out OmfSegment seg);
                if (result == OmfSegment.ParseResult.Success) {
                    if (!seg.ParseBody(formatter, msgs)) {
                        OmfSegment.AddErrorMsg(msgs, offset, "parsing of segment " +
                            seg.SegNum + " '" + seg.SegName + "' incomplete");
                        //result = OmfSegment.ParseResult.Failure;
                    }
                }

                MessageList.Clear();
                foreach (string str in msgs) {
                    MessageList.Add(str);
                }

                if (result == OmfSegment.ParseResult.IsLibrary) {
                    // Need to start over in library mode.
                    Debug.WriteLine("Restarting in library mode");
                    return result;
                } else if (result == OmfSegment.ParseResult.Failure) {
                    // Could be a library we failed to parse, could be a totally bad file.
                    // If we were on the first segment, fail immediately so we can retry as
                    // library.  If not, it's probably not a library (assuming the Library
                    // Dictionary segment appears first), but rather a partially-bad OMF.
                    if (first) {
                        return result;
                    }
                    break;
                }
                first = false;

                Debug.Assert(seg.FileLength > 0);

                SegmentList.Add(seg);
                offset += seg.FileLength;
                len -= seg.FileLength;
                Debug.Assert(len >= 0);
            }

            Debug.WriteLine("Num segments = " + SegmentList.Count);
            return OmfSegment.ParseResult.Success;
        }

        /// <summary>
        /// Analyzes the file to determine the file kind.
        /// </summary>
        private FileKind DetermineFileKind() {
            if (SegmentList.Count == 0) {
                // Couldn't find a single valid segment, this is not OMF.
                return FileKind.Foreign;
            }

            // The rules:
            // * Load files may contain any kind of segment except LibraryDict.  Their
            //   segments must be LCONST/DS followed by relocation records
            //   (INTERSEG, cINTERSEG, RELOC, cRELOC, SUPER).
            // * Object files have Code, Data, and DP/Stack segments, and may not contain
            //   relocation records or ENTRY.
            // * Library files are like Object files, but start with a LibraryDict segment.
            // * Run-Time Library files have an initial segment with ENTRY records.  (These
            //   are not supported because I've never actually seen one... the few files I've
            //   found with the RTL filetype ($B4) did not have any ENTRY records.)

            bool maybeLoad = true;
            bool maybeObject = true;
            bool maybeLibrary = true;

            foreach (OmfSegment omfSeg in SegmentList) {
                switch (omfSeg.Kind) {
                    case OmfSegment.SegmentKind.Code:
                    case OmfSegment.SegmentKind.Data:
                    case OmfSegment.SegmentKind.DpStack:
                        // Allowed anywhere.
                        break;
                    case OmfSegment.SegmentKind.JumpTable:
                    case OmfSegment.SegmentKind.PathName:
                    case OmfSegment.SegmentKind.Init:
                    case OmfSegment.SegmentKind.AbsoluteBank:
                        // Only allowed in Load.
                        maybeObject = maybeLibrary = false;
                        break;
                    case OmfSegment.SegmentKind.LibraryDict:
                        maybeLoad = false;
                        maybeObject = false;
                        break;
                    default:
                        Debug.Assert(false, "Unexpected segment kind " + omfSeg.Kind);
                        break;
                }
            }

            //
            // Initial screening complete.  Dig into the segment records to see if they're
            // compatible.
            //

            if (maybeLoad) {
                foreach (OmfSegment omfSeg in SegmentList) {
                    if (!omfSeg.CheckRecords_LoadFile()) {
                        maybeLoad = false;
                        break;
                    }
                }
                if (maybeLoad) {
                    return FileKind.Load;
                }
            }
            if (maybeLibrary || maybeObject) {
                foreach (OmfSegment omfSeg in SegmentList) {
                    if (!omfSeg.CheckRecords_ObjectOrLib()) {
                        maybeLibrary = maybeObject = false;
                        break;
                    }
                }
                if (maybeObject) {
                    return FileKind.Object;
                } else {
                    return FileKind.Library;
                }
            }

            return FileKind.Indeterminate;
        }

        /// <summary>
        /// Generates relocation dictionaries for all code/data/init segments.
        /// </summary>
        private void GenerateRelocDicts() {
            Debug.Assert(OmfFileKind == FileKind.Load);

            foreach (OmfSegment omfSeg in SegmentList) {
                if (omfSeg.Kind == OmfSegment.SegmentKind.Code ||
                        omfSeg.Kind == OmfSegment.SegmentKind.Data ||
                        omfSeg.Kind == OmfSegment.SegmentKind.Init) {
                    omfSeg.GenerateRelocDict();
                }
            }
        }
    }
}
