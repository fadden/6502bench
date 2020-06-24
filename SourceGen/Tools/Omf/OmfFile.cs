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

namespace SourceGen.Tools.Omf {
    /// <summary>
    /// Apple IIgs OMF file.
    /// </summary>
    /// <remarks>
    /// References:
    /// - "Apple IIgs Programmer's Workshop Reference".  Chapter 7, page 228, describes
    ///   OMF v1.0 and v2.0.
    /// - "Apple IIgs GS/OS Reference, for GS/OS System Software Version 5.0 and later".
    ///   Appendix F describes OMF v2.1, and Chapter 8 has some useful information about
    ///   how the loader works.
    /// - "Undocumented Secrets of the Apple IIGS System Loader" by Neil Parker,
    ///   http://nparker.llx.com/a2/loader.html . Among other things it documents ExpressLoad
    ///   segments, something Apple apparently never did.
    /// - Apple IIgs Tech Note #66, "ExpressLoad Philosophy".
    ///
    /// Related:
    /// - https://www.brutaldeluxe.fr/products/crossdevtools/omfanalyzer/
    /// - https://github.com/fadden/ciderpress/blob/master/reformat/Disasm.cpp
    /// </remarks>
    public class OmfFile {
        public const int MIN_FILE_SIZE = 37;                // can't be smaller than v0 segment hdr
        public const int MAX_FILE_SIZE = (1 << 24) - 1;     // cap it at 16MB

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
    }
}
