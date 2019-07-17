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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SourceGenWPF {
    /// <summary>
    /// Maintains a record of interesting places we've been.
    /// </summary>
    public class NavStack {
        // It's tempting to use a single stack, and just move a cursor up and down.  However,
        // that doesn't quite work.  We always want to record where you came from, so we're
        // pushing locations on when moving both forward and backward.
        //
        // If you move backward and then jump somewhere else, we want to discard the list of
        // previously-recorded forward places.
        //
        // Jumping to Notes is a little different from jumping to anything else, because we
        // want to highlight the note rather than the code at the associated offset.  This
        // is especially important when moving upward through the file, or the note will be
        // off the top of the screen.

        // TODO(someday): change the back button to a pop-up list of locations (like the way
        //   VS 2017 does it).

        /// <summary>
        /// Holds enough information to get us back where we were, in style.
        /// </summary>
        public class Location {
            public int Offset { get; set; }
            public bool IsNote { get; set; }

            public Location(int offset, bool isNote) {
                Offset = offset;
                IsNote = isNote;
            }

            public override string ToString() {
                return string.Format("[+{0:x6},{1}]", Offset, IsNote);
            }

            public static bool operator ==(Location a, Location b) {
                if (ReferenceEquals(a, b)) {
                    return true;    // same object, or both null
                }
                if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                    return false;   // one is null
                }
                return a.Offset == b.Offset && a.IsNote == b.IsNote;
            }
            public static bool operator !=(Location a, Location b) {
                return !(a == b);
            }
            public override bool Equals(object obj) {
                return obj is Location && this == (Location)obj;
            }
            public override int GetHashCode() {
                return Offset + (IsNote ? (1<<24) : 0);
            }
        }

        // Location stacks.
        private List<Location> mBackStack = new List<Location>();
        private List<Location> mFwdStack = new List<Location>();


        public NavStack() { }

        /// <summary>
        /// True if there is an opportunity to pop backward.
        /// </summary>
        public bool HasBackward {
            get {
                return mBackStack.Count > 0;
            }
        }

        /// <summary>
        /// True if there is an opportunity to push forward.
        /// </summary>
        public bool HasForward {
            get {
                return mFwdStack.Count > 0;
            }
        }

        /// <summary>
        /// Clears the stacks.
        /// </summary>
        public void Clear() {
            mBackStack.Clear();
            mFwdStack.Clear();
        }

        /// <summary>
        /// Pushes a new entry onto the back stack.  Clears the forward stack.
        /// 
        /// If the same entry is already at the top of the stack, the entry will not be added.
        /// </summary>
        /// <param name="curLoc">Current location.</param>
        public void Push(Location curLoc) {
            if (mBackStack.Count > 0 && mBackStack[mBackStack.Count - 1] == curLoc) {
                Debug.WriteLine("Not re-pushing " + curLoc);
                return;
            }

            mBackStack.Add(curLoc);
            mFwdStack.Clear();

            //Debug.WriteLine("Stack now: " + this);
        }

        /// <summary>
        /// Pops the top element from the back stack, and pushes the current position
        /// onto the forward stack.
        /// </summary>
        /// <param name="fromLoc">Current location.</param>
        /// <returns>The location to move to.</returns>
        public Location MoveBackward(Location fromLoc) {
            if (mBackStack.Count == 0) {
                throw new Exception("Stack is empty");
            }
            Location toLoc = mBackStack[mBackStack.Count - 1];
            mBackStack.RemoveAt(mBackStack.Count - 1);
            mFwdStack.Add(fromLoc);
            return toLoc;
        }

        /// <summary>
        /// Pops the top element from the forward stack, and pushes the current position
        /// onto the back stack.
        /// </summary>
        /// <param name="fromLoc">Current location.</param>
        /// <returns>The location to move to.</returns>
        public Location MoveForward(Location fromLoc) {
            if (mFwdStack.Count == 0) {
                throw new Exception("Stack is empty");
            }
            Location toLoc = mFwdStack[mFwdStack.Count - 1];
            mFwdStack.RemoveAt(mFwdStack.Count - 1);
            mBackStack.Add(fromLoc);
            return toLoc;
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append("Back:");
            foreach (Location loc in mBackStack) {
                sb.Append(loc);
            }
            sb.Append(" Fwd:");
            foreach (Location loc in mFwdStack) {
                sb.Append(loc);
            }
            return sb.ToString();
        }
    }
}
