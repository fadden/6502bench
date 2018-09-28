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
using System.Text.RegularExpressions;

namespace Asm65 {
    /// <summary>
    /// Utility classes for working with labels.
    /// 
    /// The decision of whether to treat labels as case-sensitive or case-insensitive is
    /// encapsulated here.  All code should be case-preserving, but the comparison method
    /// and "normal form" are defined here.
    /// </summary>
    public class Label {
        // Arbitrary choice for SourceGen. Different assemblers have different limits.
        public const int MAX_LABEL_LEN = 32;

        public const bool LABELS_CASE_SENSITIVE = true;

        /// <summary>
        /// String comparer to use when comparing labels.
        /// 
        /// We may want case-insensitive string compares, and we want the "invariant culture"
        /// version for consistent results across users in multiple locales.  (The labels are
        /// expected to be ASCII strings, so the latter isn't crucial unless we change the
        /// allowed set.)
        /// </summary>
        public static readonly StringComparer LABEL_COMPARER = LABELS_CASE_SENSITIVE ?
            StringComparer.InvariantCulture :
            StringComparer.InvariantCultureIgnoreCase;

        /// <summary>
        /// Regex pattern for a valid label.
        /// 
        /// ASCII-only, starts with letter or underscore, followed by at least
        /// one alphanumeric or underscore.  Some assemblers may allow single-letter
        /// labels, but I don't want to risk confusion with A/S/X/Y.  So either we
        /// reserve those, or we just mandate a two-character minimum.
        /// </summary>
        private static string sValidLabelPattern = @"^[a-zA-Z_][a-zA-Z0-9_]+$";
        private static Regex sValidLabelCharRegex = new Regex(sValidLabelPattern);

        /// <summary>
        /// Validates a label, confirming that it is correctly formed.
        /// </summary>
        /// <param name="label">Label to validate.</param>
        /// <returns>True if the label is correctly formed.</returns>
        public static bool ValidateLabel(string label) {
            if (label.Length > MAX_LABEL_LEN) {
                return false;
            }
            MatchCollection matches = sValidLabelCharRegex.Matches(label);
            return matches.Count == 1;
        }

        /// <summary>
        /// Returns "normal form" of label.  This matches LABEL_COMPARER behavior.
        /// </summary>
        /// <param name="label">Label to transform.</param>
        /// <returns>Transformed label.</returns>
        public static string ToNormal(string label) {
            return LABELS_CASE_SENSITIVE ? label : label.ToUpperInvariant();
        }
    }
}
