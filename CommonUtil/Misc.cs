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
    }
}
