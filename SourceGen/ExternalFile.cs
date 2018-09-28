/*
 * Copyright 2018 faddenSoft
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
using System.Diagnostics;
using System.IO;

namespace SourceGen {
    /// <summary>
    /// Manages references to external files, notably symbol files (.sym65) and extension
    /// scripts.  Identifiers look like "RT:subdir/file.sym65".
    /// 
    /// Instances are immutable.
    /// </summary>
    public class ExternalFile {
        private const string INVALID_IDENT = "!!!INVALID!!!";   // probably don't need localization

        /// <summary>
        /// Pathname separator character for use for file identifiers.  We want this
        /// to be the same on all platforms, with local conversion, so we should probably be
        /// using something like ':' that makes Windows barf.  In practice, being rigorous
        /// doesn't seem important, and '/' is pretty universal these days.  Just don't do \.
        /// </summary>
        private const char PATH_SEP_CHAR = '/';

        private const string RUNTIME_DIR_PREFIX = "RT:";
        private const string PROJECT_DIR_PREFIX = "PROJ:";

        private enum Location {
            Unknown = 0,
            RuntimeDir,
            ProjectDir
        }

        // Not sure there's value in tracking the type, except for some validation checks.
        private enum Type {
            Unknown = 0,
            SymbolFile,
            ExtensionScript
        }

        /// <summary>
        /// Identifier for this file.
        /// </summary>
        public string Identifier { get { return mIdent; } }
        private string mIdent;

        /// <summary>
        /// File location.
        /// </summary>
        private Location mIdentLocation;

        /// <summary>
        /// File type.
        /// </summary>
        private Type mIdentType;

        /// <summary>
        /// Identifier without location prefix or filename extension.
        /// </summary>
        private string mInnards;


        /// <summary>
        /// Creates a new ExternalFile instance from the identifier.
        /// </summary>
        public static ExternalFile CreateFromIdent(string ident) {
            if (!DecodeIdent(ident, out Location identLocation, out Type identType,
                    out string innards)) {
                return null;
            }
            return new ExternalFile(ident, identLocation, identType, innards);
        }

        /// <summary>
        /// Creates a new ExternalFile instance from a full path.
        /// </summary>
        /// <param name="pathName">Full path of external file, in canonical
        ///   form.</param>
        /// <param name="projectDir">Full path to directory in which project file lives, in
        ///   canonical form.  If the project hasn't been saved yet, pass an empty string.</param>
        /// <returns>New object, or null if the path isn't valid.</returns>
        public static ExternalFile CreateFromPath(string pathName, string projectDir) {
            string stripDir;

            string rtDir = RuntimeDataAccess.GetDirectory();
            string prefix;

            // Check path prefix for RT:, and full directory name for PROJ:.
            if (pathName.StartsWith(rtDir)) {
                stripDir = rtDir;
                prefix = RUNTIME_DIR_PREFIX;
            } else if (!string.IsNullOrEmpty(projectDir) &&
                    Path.GetDirectoryName(pathName) == projectDir) {
                stripDir = projectDir;
                prefix = PROJECT_DIR_PREFIX;
            } else {
                Debug.WriteLine("Path not in RuntimeData or project: " + pathName);
                return null;
            }

            // Remove directory component.
            string partialPath = pathName.Substring(stripDir.Length);

            // If directory string didn't end with '/' or '\\', remove char from start.
            if (partialPath[0] == '\\' || partialPath[0] == '/') {
                partialPath = partialPath.Substring(1);
            }

            // Replace canonical path sep with '/'.
            partialPath = partialPath.Replace(Path.DirectorySeparatorChar, PATH_SEP_CHAR);

            string ident = prefix + partialPath;
            Debug.WriteLine("Converted path '" + pathName + "' to ident '" + ident + "'");
            return CreateFromIdent(ident);
        }

        /// <summary>
        /// Internal constructor.
        /// </summary>
        private ExternalFile(string ident, Location identLocation, Type identType,
                string innards) {
            mIdent = ident;
            mIdentLocation = identLocation;
            mIdentType = identType;
            mInnards = innards;
        }

        /// <summary>
        /// Decodes an ident string into its constituent parts.
        /// </summary>
        private static bool DecodeIdent(string ident, out Location identLocation,
                out Type identType, out string innards) {
            identLocation = Location.Unknown;
            identType = Type.Unknown;
            innards = string.Empty;

            int prefixLen;
            if (ident.StartsWith(RUNTIME_DIR_PREFIX)) {
                identLocation = Location.RuntimeDir;
                prefixLen = RUNTIME_DIR_PREFIX.Length;
            } else if (ident.StartsWith(PROJECT_DIR_PREFIX)) {
                identLocation = Location.ProjectDir;
                prefixLen = PROJECT_DIR_PREFIX.Length;
            } else {
                return false;
            }

            int extLen;
            if (ident.EndsWith(PlatformSymbols.FILENAME_EXT)) {
                identType = Type.SymbolFile;
                extLen = PlatformSymbols.FILENAME_EXT.Length;
            } else if (ident.EndsWith(Sandbox.ScriptManager.FILENAME_EXT)) {
                identType = Type.ExtensionScript;
                extLen = Sandbox.ScriptManager.FILENAME_EXT.Length;
            } else {
                return false;
            }

            // Fail idents with no actual name, e.g. "RT:.cs".
            if (ident.Length == prefixLen + extLen) {
                return false;
            }

            innards = ident.Substring(prefixLen, ident.Length - prefixLen - extLen);
            return true;
        }

        /// <summary>
        /// Strips the prefix and filename extension off of an identifier.
        /// </summary>
        /// <returns>Stripped identifier, or null if the identifier was malformed.</returns>
        public string GetInnards() {
            return mInnards;
        }

        /// <summary>
        /// Converts an identifier to a full path.  For PROJ: identifiers, the project
        /// directory argument is used.
        /// </summary>
        /// <param name="ident">Identifier to convert.</param>
        /// <param name="projectDir">Full path to directory in which project file lives, in
        ///   canonical form.  If the project hasn't been saved yet, pass an empty string.</param>
        /// <returns>Full path, or null if the identifier points to a file outside the
        ///   directory, or if this is a ProjectDir ident and the project dir isn't set.</returns>
        public string GetPathName(string projectDir) {
            string dir;

            bool subdirAllowed;
            switch (mIdentLocation) {
                case Location.RuntimeDir:
                    dir = RuntimeDataAccess.GetDirectory();
                    subdirAllowed = true;
                    break;
                case Location.ProjectDir:
                    if (string.IsNullOrEmpty(projectDir)) {
                        // Shouldn't happen in practice -- we don't create PROJ: identifiers
                        // unless a project directory has been established.
                        Debug.Assert(false);
                        return null;
                    }
                    dir = projectDir;
                    subdirAllowed = false;
                    break;
                default:
                    Debug.Assert(false);
                    return null;
            }

            int extLen = mIdent.IndexOf(':') + 1;
            string fullPath = Path.GetFullPath(Path.Combine(dir, mIdent.Substring(extLen)));

            // Confirm the file actually lives in the directory.  RT: files can be anywhere
            // below the RuntimeData directory, while PROJ: files must live in the project
            // directory.
            if (subdirAllowed) {
                dir += Path.DirectorySeparatorChar;
                if (!fullPath.StartsWith(dir)) {
                    Debug.WriteLine("WARNING: ident resolves outside subdir: " + mIdent);
                    Debug.Assert(false);
                    return null;
                }
            } else {
                if (dir != Path.GetDirectoryName(fullPath)) {
                    Debug.WriteLine("WARNING: ident resolves outside dir: " + mIdent);
                    return null;
                }
            }

            return fullPath;
        }

        /// <summary>
        /// Generates a script DLL name from the ident.  If the ident is for a project-scope
        /// extension script, the project's file name will be included.
        /// </summary>
        /// <param name="projectPathName">Full path to project.</param>
        /// <returns>DLL filename.</returns>
        public string GenerateDllName(string projectFileName) {
            switch (mIdentLocation) {
                case Location.RuntimeDir:
                    return "RT_" + mInnards.Replace(PATH_SEP_CHAR, '_') + ".dll";
                case Location.ProjectDir:
                    string noExt = Path.GetFileNameWithoutExtension(projectFileName);
                    return "PROJ_" + noExt + "_" + mInnards.Replace(PATH_SEP_CHAR, '_') + ".dll";
                default:
                    Debug.Assert(false);
                    return null;
            }
        }
    }
}
