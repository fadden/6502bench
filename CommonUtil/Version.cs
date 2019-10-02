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

namespace CommonUtil {
    /// <summary>
    /// Version number container.  Instances are immutable.
    /// </summary>
    /// <remarks>
    /// See https://semver.org/ for explanation of system.
    /// </remarks>
    public struct Version {
        // Must be in ascending order, e.g. Alpha release comes before Beta.
        public enum PreRelType { Dev, Alpha, Beta, Final };

        /// <summary>
        /// Major version number.
        /// </summary>
        public int Major { get; private set; }

        /// <summary>
        /// Minor version number.
        /// </summary>
        public int Minor { get; private set; }

        /// <summary>
        /// Bug fix release number.
        /// </summary>
        public int Patch { get; private set; }

        /// <summary>
        /// Software grade, for pre-release versions.
        /// </summary>
        public PreRelType PreReleaseType { get; private set; }

        /// <summary>
        /// Pre-release version.  Always zero when PreReleaseType is Final.
        /// </summary>
        public int PreRelease { get; private set; }

        /// <summary>
        /// Version instance to use when no version information is available.  This will
        /// always compare as less than a "real" version.
        /// </summary>
        public static readonly Version NO_VERSION = new Version(-1, -1, -1);

        /// <summary>
        /// Shortcut for comparing vs. NO_VERSION.
        /// </summary>
        public bool IsValid {
            get {
                return this != NO_VERSION;
            }
        }


        public Version(int major, int minor) :
            this(major, minor, 0, PreRelType.Final, 0) { }

        public Version(int major, int minor, int patch) :
            this(major, minor, patch, PreRelType.Final, 0) { }

        public Version(int major, int minor, int patch, PreRelType preRelType, int preRel) {
            Debug.Assert(preRelType != PreRelType.Final || preRel == 0);
            Major = major;
            Minor = minor;
            Patch = patch;
            PreReleaseType = preRelType;
            PreRelease = preRel;
        }

        /// <summary>
        /// Attempts to parse the argument into version components.
        /// </summary>
        /// <param name="str">Version string.</param>
        /// <returns>New Version object, or NO_VERSION on parsing failure.</returns>
        public static Version Parse(string str) {
            try {
                int major, minor, patch;
                major = minor = patch = 0;

                string[] parts = str.Split(new char[] { '.', '-' });
                major = int.Parse(parts[0]);
                if (parts.Length > 1) {
                    minor = int.Parse(parts[1]);
                }
                if (parts.Length > 2) {
                    patch = int.Parse(parts[2]);
                }
                // parse the preRel thing someday
                return new Version(major, minor, patch);
            } catch (Exception ex) {
                Debug.WriteLine("Version parse failed: '" + str + "': " + ex.Message);
                return NO_VERSION;
            }
        }


        // this is a struct, so no need for null checks
        public static bool operator ==(Version a, Version b) {
            return a.Major == b.Major && a.Minor == b.Minor && a.Patch == b.Patch &&
                a.PreReleaseType == b.PreReleaseType && a.PreRelease == b.PreRelease;
        }
        public static bool operator !=(Version a, Version b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is Version && this == (Version)obj;
        }
        public override int GetHashCode() {
            return Major * 10000 + Minor * 1000 + Patch * 100 +
                (int)PreReleaseType * 10 + PreRelease;
        }

        public static bool operator <(Version a, Version b) {
            if (a.Major != b.Major) {
                return a.Major < b.Major;
            }
            if (a.Minor != b.Minor) {
                return a.Minor < b.Minor;
            }
            if (a.Patch != b.Patch) {
                return a.Patch < b.Patch;
            }
            if (a.PreReleaseType != b.PreReleaseType) {
                return (int)a.PreReleaseType < (int)b.PreReleaseType;
            }
            if (a.PreRelease != b.PreRelease) {
                return a.PreRelease < b.PreRelease;
            }
            Debug.Assert(a == b);
            return false;
        }
        public static bool operator >(Version a, Version b) {
            return b < a;
        }
        public static bool operator <=(Version a, Version b) {
            return a == b || a < b;
        }
        public static bool operator >=(Version a, Version b) {
            return a == b || a > b;
        }

        public override string ToString() {
            if (this == NO_VERSION) {
                return Properties.Resources.NO_VERSION;
            } else if (PreReleaseType == PreRelType.Final) {
                return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
            } else {
                return string.Format("{0}.{1}.{2}-{3}{4}", Major, Minor, Patch,
                    PreReleaseType.ToString().ToLower(), PreRelease);
            }
        }

        /// <summary>
        /// Simple unit test.
        /// </summary>
        public static bool Test() {
            bool ok = true;

            Version checkVers = new Version(1, 2, 3, PreRelType.Beta, 4);
            Version sameVers = new Version(1, 2, 3, PreRelType.Beta, 4);
            ok &= (checkVers == sameVers);
            ok &= (checkVers <= sameVers);
            ok &= (checkVers >= sameVers);
            ok &= (!(checkVers < sameVers));
            ok &= (!(checkVers > sameVers));

            ok &= (checkVers != new Version(1, 2, 3));
            ok &= (checkVers < new Version(1, 2, 3));
            ok &= (checkVers > new Version(1, 2, 3, PreRelType.Beta, 3));
            ok &= (checkVers < new Version(2, 0));
            ok &= (checkVers > new Version(1, 2, 2));
            ok &= (checkVers < new Version(1, 3, 1));

            Debug.WriteLine("Version struct: test complete (ok=" + ok + ")");
            return ok;
        }
    }
}
