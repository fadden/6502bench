/*
 * Copyright 2019 faddenSoft
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
using System.Windows;

namespace SourceGen {
    /// <summary>
    /// Application class.
    /// </summary>
    public partial class App : Application {
        /// <summary>
        /// SourceGen version number.
        /// </summary>
        public static readonly CommonUtil.Version ProgramVersion =
            new CommonUtil.Version(1, 8, 0, CommonUtil.Version.PreRelType.Dev, 1);
    }
}
