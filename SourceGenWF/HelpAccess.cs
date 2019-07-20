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
using System.Diagnostics;
using System.IO;

using CommonUtil;

/*
There are a few different options for viewing help files:
 (1) Microsoft HTML Help.  Requires writing stuff in a specific way and then running a
     tool to turn it into a .chm file, which then requires a help viewer application.
     Feels a little weak in terms of future-proofing and cross-platform support.
 (2) Plain HTML, using System.Windows.Forms.WebBrowser class.  This seems like a nice
     way to go, but we need to provide all the standard controls, and it means we have
     a web browser running in-process.
 (3) Plain HTML, with the Microsoft.Toolkit.Win32.UI.Controls.WinForms.WebView control.
     Similar to WebBrowser, but newer and fancier, and probably less portable.
 (4) Plain HTML, viewed with the system browser.  We outsource the problem.  The big
     problem here is that the easy/portable way (Process.Start(url)) discards the anchor
     part (the bit after '#').  There are workarounds, but they seem to involve dredging
     the default browser out of the Registry.
 (5) Custom roll-your-own solution.  Have you seen this round thing I invented?  I'm
     calling it a "wheel".

For now I'm going with #4, and dealing with anchors by ignoring them: the help menu item
just opens the TOC, and individual UI items don't have help buttons.

What we need in terms of API is a way to say, "show the help for XYZ".  The rest can be
encapsulated here.
*/

namespace SourceGenWF {
    /// <summary>
    /// Help viewer API.
    /// </summary>
    public static class HelpAccess {
        private const string HELP_DIR = "Help";     // directory inside RuntimeData

        /// <summary>
        /// Help topics.
        /// </summary>
        public enum Topic {
            Contents,       // table of contents

            Main,           // main window, general workflow

            // Editors
            EditLongComment,
        }

        private static Dictionary<Topic, string> sTopicMap = new Dictionary<Topic, string>() {
            { Topic.Contents, "index.html" },
            { Topic.Main, "main.html" },
            { Topic.EditLongComment, "editor.html#long-comment" }
        };

        /// <summary>
        /// Opens a window with the specified help topic.
        /// </summary>
        /// <param name="topic"></param>
        public static void ShowHelp(Topic topic) {
            if (!sTopicMap.TryGetValue(topic, out string fileName)) {
                Debug.Assert(false, "Unable to find " + topic + " in map");
                return;
            }

            string helpFilePath = Path.Combine(RuntimeDataAccess.GetDirectory(),
                HELP_DIR, fileName);
            string url = "file://" + helpFilePath;
            //url = url.Replace("#", "%23");
            Debug.WriteLine("Requesting help URL: " + url);
            ShellCommand.OpenUrl(url);
        }
    }
}
