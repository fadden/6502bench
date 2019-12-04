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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CommonUtil {
    public class Container {
        /// <summary>
        /// Compares two lists of strings to see if their contents are equal.  The lists
        /// must contain the same strings, in the same order.
        /// </summary>
        /// <param name="l1">List #1.</param>
        /// <param name="l2">List #2.</param>
        /// <param name="comparer">String comparer (e.g. StringComparer.InvariantCulture).  If
        ///   null, the default string comparer is used.</param>
        /// <returns>True if the lists are equal.</returns>
        public static bool StringListEquals(IList<string> l1, IList<string>l2,
                StringComparer comparer) {
            // Quick check for reference equality.
            if (l1 == l2) {
                return true;
            }
            return Enumerable.SequenceEqual<string>(l1, l2, comparer);
        }

        /// <summary>
        /// Compares two Dictionaries to see if their contents are equal.  Key and value types
        /// must have correctly-implemented equality checks.  (I contend this works incorrectly
        /// for float -- 5.0f is equal to the integer 5.)
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/q/3804367/294248
        ///
        /// TODO: make this work right for float/int comparisons
        /// </remarks>
        /// <typeparam name="TKey">Dictionary key type.</typeparam>
        /// <typeparam name="TValue">Dictionary value type.</typeparam>
        /// <param name="dict1">Dictionary #1.</param>
        /// <param name="dict2">Dictionary #2.</param>
        /// <returns>True if equal, false if not.</returns>
        public static bool CompareDicts<TKey, TValue>(
                Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2) {
            if (dict1 == dict2) {
                return true;
            }
            if (dict1 == null || dict2 == null) {
                return false;
            }
            if (dict1.Count != dict2.Count) {
                return false;
            }

#if false
            var valueComparer = EqualityComparer<TValue>.Default;

            foreach (var kvp in dict1) {
                TValue value2;
                if (!dict2.TryGetValue(kvp.Key, out value2)) return false;
                if (!valueComparer.Equals(kvp.Value, value2)) return false;
            }
            return true;
#else
            // Check to see if there are any elements in the first that are not in the second.
            return !dict1.Except(dict2).Any();
#endif
        }

        public static bool CompareDicts<TKey, TValue>(
                ReadOnlyDictionary<TKey, TValue> dict1, ReadOnlyDictionary<TKey, TValue> dict2) {
            if (dict1 == dict2) {
                return true;
            }
            if (dict1 == null || dict2 == null) {
                return false;
            }
            if (dict1.Count != dict2.Count) {
                return false;
            }

            // Check to see if there are any elements in the first that are not in the second.
            return !dict1.Except(dict2).Any();
        }
    }
}
