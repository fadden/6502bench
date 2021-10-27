/*
 * Copyright 2021 faddenSoft
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
using System.Web.Script.Serialization;
using System.Windows.Media.Imaging;

namespace SourceGen {
    /// <summary>
    /// Holds the collection of daily tips and associated images.
    /// </summary>
    public class DailyTips {
        private static string TIPS_FILE = "daily-tips.json";
        private static string TIPS_DIR = "Tips";

        /// <summary>
        /// A tip is a text string with an optional bitmap.
        /// </summary>
        public class Tip {
            public string Text { get; private set; }
            public string ImageFileName { get; private set; }
            public BitmapSource Bitmap { get; }

            public Tip(string text, string imageFileName) {
                Text = text;
                ImageFileName = imageFileName;

                if (!string.IsNullOrEmpty(imageFileName)) {
                    Bitmap = LoadImage(imageFileName);
                }
            }

            private static BitmapSource LoadImage(string fileName) {
                string pathName = RuntimeDataAccess.GetPathName(TIPS_DIR);
                pathName = Path.Combine(pathName, fileName);
                try {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(pathName);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;  // don't hold file open
                    bitmap.EndInit();
                    return bitmap;
                } catch (Exception ex) {
                    Debug.WriteLine("Unable to load bitmap '" + fileName + "': " + ex);
                    return null;
                }
            }

            private static BitmapSource LoadImage1(string fileName) {
                string pathName = RuntimeDataAccess.GetPathName(TIPS_DIR);
                pathName = Path.Combine(pathName, fileName);
                try {
                    // From "How to: Encode and Decode a PNG Image".
                    // Note: this holds the file open (try deleting the file).
                    BitmapSource bitmapSource;
                    Stream imageStreamSource = new FileStream(pathName, FileMode.Open,
                            FileAccess.Read, FileShare.Read);
                    PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource,
                        BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    bitmapSource = decoder.Frames[0];
                    //imageStreamSource.Dispose();  // image becomes blank
                    return bitmapSource;
                } catch (IOException ex) {
                    Debug.WriteLine("Unable to load image '" + fileName + "': " + ex);
                    return null;
                }
            }
        }

        /// <summary>
        /// List of tips.
        /// </summary>
        private List<Tip> mTips = new List<Tip>();

        public DailyTips() { }

        /// <summary>
        /// Loads the list of tips.  The list will always have at least one element.
        /// </summary>
        public bool Load() {
            mTips.Clear();
            if (!DoLoadTips()) {
                mTips.Add(new Tip(Res.Strings.TIPS_NOT_AVAILABLE, null));
                return false;
            } else {
                Debug.WriteLine("Loaded " + mTips.Count + " tips");
                return true;
            }
        }

        public int DailyNumber {
            get {
                // We show a different tip every day by taking the day-of-year value and
                // modding it by the number of tips we have.
                if (mTips.Count > 0) {
                    DateTime now = DateTime.Now;
                    int dayIndex = now.Year * 365 + now.DayOfYear;
                    return dayIndex % mTips.Count;
                } else {
                    return 0;
                }
            }
        }

        public int Count { get { return mTips.Count; } }

        /// <summary>
        /// Returns the Nth tip.
        /// </summary>
        public Tip Get(int index) {
            if (mTips.Count == 0) {
                // Load tips, or at least generate a "not available" entry.
                Load();
            }
            if (index < 0 || index >= mTips.Count) {
                Debug.WriteLine("Invalid request for tip " + index);
                return mTips[0];
            }

            return mTips[index];
        }


        [Serializable]
        internal class SerTip {
            public string Text { get; set; }
            public string Image { get; set; }
        }
        internal class SerTipFile {
            public List<SerTip> Tips { get; set; }
        }

        private bool DoLoadTips() {
            string pathName = RuntimeDataAccess.GetPathName(TIPS_DIR);
            pathName = Path.Combine(pathName, TIPS_FILE);
            string cereal;
            try {
                cereal = File.ReadAllText(pathName);
            } catch (IOException ex) {
                Debug.WriteLine("Failed reading tip file '" + pathName + "': " + ex.Message);
                return false;
            }

            JavaScriptSerializer ser = new JavaScriptSerializer();
            SerTipFile tipFile;
            try {
                tipFile = ser.Deserialize<SerTipFile>(cereal);
            } catch (Exception ex) {
                Debug.WriteLine("Failed deserializing tip JSON: " + ex.Message);
                return false;
            }
            if (tipFile == null) {
                Debug.WriteLine("Failed to find tip list");
                return false;
            }

            foreach (SerTip serTip in tipFile.Tips) {
                mTips.Add(new Tip(serTip.Text, serTip.Image));
            }

            return true;
        }
    }
}
