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
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace CommonWPF {
    /// <summary>
    /// Handle a multi-key input sequence for WPF windows.
    /// </summary>
    /// <remarks>
    /// Also posted as https://stackoverflow.com/a/56452142/294248
    /// 
    /// Example:
    ///   {RoutedUICommand}.InputGestures.Add(
    ///       new MultiKeyInputGesture(new KeyGesture[] {
    ///           new KeyGesture(Key.H, ModifierKeys.Control, "Ctrl+H"),
    ///           new KeyGesture(Key.C, ModifierKeys.Control, "Ctrl+C")
    ///       }) );
    /// </remarks>
    public class MultiKeyInputGesture : InputGesture {
        private const int MAX_PAUSE_MILLIS = 2000;

        private InputGestureCollection mGestures = new InputGestureCollection();

        private DateTime mLastWhen = DateTime.Now;
        private int mCheckIdx;
        private string mIdStr;

        // On a successful match, the handler "eats" the final keypress.  If you have multiple
        // handlers, the ones that are called later won't see the non-matching key and will
        // still be waiting.  This can be a problem if the user types multiple multi-key
        // sequences in rapid succession (or even not-so-rapid if you disable the timeout).  To
        // deal with this, all instances subscribe to this event, which fires when a match is
        // found.
        private delegate void GotMatchHandler(object sender, EventArgs e);
        private static event GotMatchHandler sGotMatch;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="keys">Sequence of keys to watch for.</param>
        public MultiKeyInputGesture(KeyGesture[] keys) {
            Debug.Assert(keys.Length > 0);  // arguably also bad input if == 1

            StringBuilder idSb = new StringBuilder();

            // Grab a copy of the array contents.
            foreach (KeyGesture kg in keys) {
                mGestures.Add(kg);
                idSb.Append(kg.DisplayString[kg.DisplayString.Length - 1]);
            }
            mIdStr = idSb.ToString();

            sGotMatch += delegate(object sender, EventArgs e) {
                mCheckIdx = 0;
            };
        }

        /// <summary>
        /// InputGesture interface.  Tests an input event to see if it's part of a sequence.
        /// </summary>
        /// <param name="targetElement">Not used.</param>
        /// <param name="inputEventArgs">Input event.  Ignored if not a key event.</param>
        /// <returns>True if the key matches and we're at the end of the sequence.</returns>
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs) {
            if (!(inputEventArgs is KeyEventArgs)) {
                // does this actually happen?
                return false;
            }

            DateTime now = DateTime.Now;
            if ((now - mLastWhen).TotalMilliseconds > MAX_PAUSE_MILLIS) {
                //Debug.WriteLine("MKIG " + mIdStr + ": too long since last key (" +
                //    (now - mLastWhen).TotalMilliseconds + " ms");
                mCheckIdx = 0;
            }
            mLastWhen = now;

            if (((KeyEventArgs)inputEventArgs).IsRepeat) {
                // ignore key-repeat noise (especially from modifiers)
                return false;
            }

            if (!mGestures[mCheckIdx].Matches(null, inputEventArgs)) {
                if (mCheckIdx > 0) {
                    //Debug.WriteLine("MKIG " + mIdStr + ": no match, resetting");
                    mCheckIdx = 0;
                }
                return false;
            }

            //Debug.WriteLine("MKIG " + mIdStr + ": matched gesture #" + mCheckIdx);
            mCheckIdx++;
            if (mCheckIdx == mGestures.Count) {
                //Debug.WriteLine("MKIG " + mIdStr + ": match");
                mCheckIdx = 0;
                inputEventArgs.Handled = true;

                // signal other instances
                sGotMatch(this, null);
                return true;
            }

            return false;
        }
    }
}
