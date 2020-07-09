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
using System.Linq;

namespace CommonUtil {
    public static class Misc {
        /// <summary>
        /// Application identifier.  This is an arbitrary string set by the application.  It
        /// will be included in the crash dump.
        /// </summary>
        public static string AppIdent { get; set; } = "(app ident unset)";

        // Given a type, dump all namespaces found in the same assembly.
        // https://stackoverflow.com/a/1549216/294248
        public static void DumpNamespacesInAssembly(Type type) {
            Console.WriteLine("Assembly: " + type.Assembly.Location);
            Type[] typeList = type.Assembly.GetTypes();
            var namespaces = typeList.Select(t => t.Namespace).Distinct();
            foreach (string ns in namespaces) {
                Console.WriteLine("  " + ns);
            }
        }

        /// <summary>
        /// Writes an unhandled exception trace to a crash file.
        /// </summary>
        /// <remarks>
        /// Usage:
        ///   AppDomain.CurrentDomain.UnhandledException +=
        ///       new UnhandledExceptionEventHandler(CommonUtil.Misc.CrashReporter);
        ///
        /// Thanks: https://stackoverflow.com/a/21308327/294248
        /// </remarks>
        public static void CrashReporter(object sender, UnhandledExceptionEventArgs e) {
            const string CRASH_PATH = @"CrashLog.txt";

            Exception ex = (Exception)e.ExceptionObject;
            Debug.WriteLine("CRASHING (term=" + e.IsTerminating + "): " + ex);

            try {
                using (StreamWriter writer = new StreamWriter(CRASH_PATH, true)) {
                    writer.WriteLine("*** " + DateTime.Now.ToLocalTime() + " ***");
                    writer.WriteLine("  App: " + AppIdent);
                    writer.WriteLine("  OS: " +
                        System.Runtime.InteropServices.RuntimeInformation.OSDescription);
                    writer.WriteLine(string.Empty);
                    while (ex != null) {
                        writer.WriteLine(ex.GetType().FullName + ": " + ex.Message);
                        writer.WriteLine("Trace:");
                        writer.WriteLine(ex.StackTrace);
                        writer.WriteLine(string.Empty);

                        ex = ex.InnerException;
                    }
                }
            } catch {
                // damn it
                Debug.WriteLine("Crashed while crashing");
            }
        }

        /// <summary>
        /// Clears an array to a specific value, similar to memset() in libc.  This is much
        /// faster than setting array elements individually.
        /// </summary>
        /// <remarks>
        /// From https://stackoverflow.com/a/18659408/294248
        ///
        /// Invokes Array.Copy() on overlapping elements.  Other approaches involve using
        /// Buffer.BlockCopy or unsafe code.  Apparently .NET Core has an Array.Fill(), but
        /// that doesn't exist in .NET Framework.
        ///
        /// We could get off the line a little faster by setting the first 16 or so elements in
        /// a loop, bailing out if we finish early, so we don't start calling Array.Copy() until
        /// it's actually faster to do so.  I don't expect to be calling this often or for
        /// small arrays though.
        /// </remarks>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="array">Array reference.</param>
        /// <param name="elem">Initialization value.</param>
        public static void Memset<T>(T[] array, T elem) {
            //Array.Fill(array, elem);
            int length = array.Length;
            if (length == 0) return;
            array[0] = elem;
            int count;
            for (count = 1; count <= length / 2; count *= 2)
                Array.Copy(array, 0, array, count, count);
            Array.Copy(array, 0, array, count, length - count);
        }
    }
}
