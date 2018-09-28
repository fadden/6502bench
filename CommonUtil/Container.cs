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
    }
}
