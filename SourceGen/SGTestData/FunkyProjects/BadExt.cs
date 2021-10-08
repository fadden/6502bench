// Copyright 2021 faddenSoft. All Rights Reserved.
// See the LICENSE.txt file for distribution terms (Apache 2.0).

using System;
//using System.Collections.Generic;
using System.IO;

using PluginCommon;

namespace FunkyTest {
    /// <summary>
    /// Extension script that tries to violate the security sandbox.
    /// </summary>
    public class BadExt: MarshalByRefObject, IPlugin {
        private IApplication mAppRef;
        private byte[] mFileData;

        public string Identifier {
            get {
                return "Bad test";
            }
        }

        public void Prepare(IApplication appRef, byte[] fileData, AddressTranslate addrTrans) {
            mAppRef = appRef;
            mFileData = fileData;

            mAppRef.DebugLog("BadTest(id=" + AppDomain.CurrentDomain.Id + "): prepare()");

            // The behavior should be either "found" or "not found" depending on whether or
            // not the security sandbox is enabled.  The output is visible in the analyzer
            // output window.
            mAppRef.DebugLog("Testing file access...");
            string testDir = @"C:\";
            if (Directory.Exists(testDir)) {
                mAppRef.DebugLog("Found " + testDir);
            } else {
                mAppRef.DebugLog("No such file " + testDir);
            }
        }

        public void Unprepare() {
            mAppRef = null;
            mFileData = null;
        }
    }
}
