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

namespace SourceGen.Tools.Omf {
    /// <summary>
    /// Apple IIgs OMF file.
    /// </summary>
    /// <remarks>
    /// OMF files are a series of segments.  There is no file header or identifying information.
    /// In some cases the length is expected to be a multiple of 512 bytes, in others it isn't.
    ///
    /// There's no structural limitation on mixing and matching segments, whether different
    /// versions or different types.  The file format provides a structure in which various
    /// things may be stored, but does not provide a way to tell an observer what is contained
    /// within (the ProDOS file type is supposed to do that).
    ///
    /// References:
    /// - (OMF "v0" is documented in an Orca/M manual?)
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

        // TODO:
        // - has an overall file type (load, object, RTL)
        //   - determine with a prioritized series of "could this be ____" checks
        // - holds list of OmfSegment
        // - has a list of warnings and errors that arose during parsing
        // - holds on to byte[] with data
        // OmfSegment:
        // - header (common data, plus name/value dict with version-specific fields for display)
        // - ref back to OmfFile for byte[] access?
        // - list of OmfRecord
        // - file-type-specific stuff can be generated and cached in second pass, e.g.
        //   generate a full relocation dictionary for load files (can't do this until we
        //   know the overall file type, which we can't know until all segments have been
        //   processed a bit)

        private byte[] mFileData;

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

        private bool mIsDamaged;

        private List<OmfSegment> mSegmentList = new List<OmfSegment>();
        public List<OmfSegment> SegmentList {
            get { return mSegmentList; }
        }

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

        public void Analyze() {
            OmfSegment.ParseResult result = DoAnalyze(false);
            if (result == OmfSegment.ParseResult.IsLibrary ||
                    result == OmfSegment.ParseResult.Failure) {
                // Failed; try again as a library.
                List<string> firstFail = new List<string>(MessageList);
                result = DoAnalyze(true);
                if (result == OmfSegment.ParseResult.Failure) {
                    // Failed both ways.  Report the failures from the non-library attempt.
                    MessageList = firstFail;
                }
            }
        }

        private OmfSegment.ParseResult DoAnalyze(bool parseAsLibrary) {
            bool first = true;
            int offset = 0;
            int len = mFileData.Length;

            List<string> msgs = new List<string>();

            while (len > 0) {
                OmfSegment.ParseResult result = OmfSegment.ParseSegment(mFileData, offset,
                    parseAsLibrary, msgs, out OmfSegment seg);

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
                mSegmentList.Add(seg);
                offset += seg.FileLength;
                len -= seg.FileLength;
                Debug.Assert(len >= 0);
            }

            Debug.WriteLine("Num segments = " + mSegmentList.Count);
            return OmfSegment.ParseResult.Success;
        }
    }
}
