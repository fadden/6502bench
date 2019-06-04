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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
///
/// TODO: if you have more than one handler, the handler that completes a sequence will
/// "eat" the final key, and the other handlers won't reset.  Might need to define an event
/// that all gesture objects subscribe to, so they all reset at once.  In the mean time, the
/// reset-after-time handler solves the problem if the user is moving slowly enough.
/// </remarks>
namespace CommonWPF {
    public class MultiKeyInputGesture : InputGesture {
        private const int MAX_PAUSE_MILLIS = 1500;

        private InputGestureCollection mGestures = new InputGestureCollection();

        private DateTime mLastWhen = DateTime.Now;
        private int mCheckIdx;
        private string mIdStr;


        public MultiKeyInputGesture(KeyGesture[] keys) {
            Debug.Assert(keys.Length > 0);

            StringBuilder idSb = new StringBuilder();

            // Grab a copy of the array contents.
            foreach (KeyGesture kg in keys) {
                mGestures.Add(kg);
                idSb.Append(kg.DisplayString[kg.DisplayString.Length - 1]);
            }
            mIdStr = idSb.ToString();
        }

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
                return true;
            }

            return false;
        }
    }
}
