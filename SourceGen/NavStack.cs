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
using System.Text;

namespace SourceGen {
    /// <summary>
    /// Maintains a record of interesting places we've been.
    /// </summary>
    public class NavStack {
        // If you're at offset 10, and you jump to offset 20, we push offset 10 onto the
        // back list.  If you hit back, you want to be at offset 10.  If you then hit
        // forward, you want to jump to offset 20.  So how does 20 get on there?
        //
        // The trick is to record the "from" and "to" position at each step.  When moving
        // backward we go the previous "from" position.  When moving forward we move to
        // the next "to" position.  This makes the movement asymmetric, but it means that
        // that forward movement is always to places we've jumped to, and backward movement
        // is to places we jumped away from.

        // TODO(someday): record more about what was selected, so e.g. when we move back or
        //   forward to a Note we can highlight it appropriately.
        // TODO(someday): once we have the above, we can change the back button to a pop-up
        //   list of locations (like the way VS 2017 does it).

        private class OffsetPair {
            public int From { get; set; }
            public int To { get; set; }

            public OffsetPair(int from, int to) {
                From = from;
                To = to;
            }
            public override string ToString() {
                return "[fr=+" + From.ToString("x6") + " to=+" + To.ToString("x6") + "]";
            }
        }

        // Offset stack.  Popped items remain in place temporarily.
        private List<OffsetPair> mStack = new List<OffsetPair>();

        // Current stack position.  This is one past the most-recently-pushed element.
        private int mCursor = 0;


        public NavStack() { }

        /// <summary>
        /// True if there is an opportunity to pop backward.
        /// </summary>
        public bool HasBackward {
            get {
                return mCursor > 0;
            }
        }

        /// <summary>
        /// True if there is an opportunity to push forward.
        /// </summary>
        public bool HasForward {
            get {
                return mCursor < mStack.Count;
            }
        }

        /// <summary>
        /// Clears the back stack.
        /// </summary>
        public void Clear() {
            mStack.Clear();
            mCursor = 0;
        }

        /// <summary>
        /// Pops the top entry off the stack.  This moves the cursor but doesn't actually
        /// remove the item.
        /// </summary>
        /// <returns>The "from" element of the popped entry.</returns>
        public int Pop() {
            if (mCursor == 0) {
                throw new Exception("Stack is empty");
            }
            mCursor--;
            //Debug.WriteLine("NavStack popped +" + mStack[mCursor] +
            //    " (now cursor=" + mCursor + ") -- " + this);
            return mStack[mCursor].From;
        }

        /// <summary>
        /// Pushes a new entry onto the stack at the cursor.  If there were additional
        /// entries past the cursor, they will be discarded.
        /// 
        /// If the same entry is already at the top of the stack, the entry will not be added.
        /// </summary>
        /// <param name="fromOffset">File offset associated with line we are moving from.
        ///   This may be negative if we're moving from a header comment or .EQ directive.</param>
        /// <param name="toOffset">File offset associated with line we are moving to.  This
        ///   may be negative if we're moving to the header comment or a .EQ directive.</param>
        public void Push(int fromOffset, int toOffset) {
            if (mStack.Count > mCursor) {
                mStack.RemoveRange(mCursor, mStack.Count - mCursor);
            }
            OffsetPair newPair = new OffsetPair(fromOffset, toOffset);
            mStack.Add(newPair);
            mCursor++;
            //Debug.WriteLine("NavStack pushed +" + newPair + " -- " + this);
        }

        /// <summary>
        /// Pushes a previous entry back onto the stack.
        /// </summary>
        /// <returns>The "to" element of the pushed entry.</returns>
        public int PushPrevious() {
            if (mCursor == mStack.Count) {
                throw new Exception("At top of stack");
            }
            int fwdOff = mStack[mCursor].To;
            mCursor++;
            //Debug.WriteLine("NavStack pushed prev (now cursor=" + mCursor + ") -- " + this);
            return fwdOff;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("NavStack:");
            for (int i = 0; i < mStack.Count; i++) {
                if (i == mCursor) {
                    sb.Append(" [*]");
                }
                sb.Append(mStack[i]);
            }
            if (mCursor == mStack.Count) {
                sb.Append(" [*]");
            }
            return sb.ToString();
        }
    }
}
