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
using System.Windows.Forms;

namespace SourceGen {
    static class Program {
        // Version number applied to the program as a whole.
        public static readonly CommonUtil.Version ProgramVersion =
            new CommonUtil.Version(1, 1, 0, CommonUtil.Version.PreRelType.Dev, 1);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            // Run some utility class unit tests if we're built for debug.
            Debug.Assert(CommonUtil.RangeSet.Test());
            Debug.Assert(CommonUtil.TypedRangeSet.Test());
            Debug.Assert(CommonUtil.Version.Test());
            Debug.Assert(Asm65.CpuDef.DebugValidate());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppForms.ProjectView());
        }
    }
}
