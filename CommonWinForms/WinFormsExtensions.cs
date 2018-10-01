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
using System.Drawing;
using System.Windows.Forms;

namespace CommonWinForms {
    /// <summary>
    /// Overload RichTextBox.AppendText() with a version that takes a color as an argument.
    /// 
    /// From https://stackoverflow.com/a/1926822/294248
    /// </summary>
    public static class RichTextBoxExtensions {
        public static void AppendText(this RichTextBox box, string text, Color color) {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }

    /// <summary>
    /// Add functions to select and deselect all items.
    /// </summary>
    public static class ListViewExtensions {
        // I don't know if NativeMethods is going to work on other platforms, but I assume
        // not.  I'm not sure how expensive the runtime check is, so cache the result.
        private enum DNY { Dunno = 0, No, Yes };
        private static DNY IsPlatformWindows;
        private static bool IsWindows {
            get {
                if (IsPlatformWindows == DNY.Dunno) {
                    // This is the .NET framework 4.6 way.  Later versions (4.7, netcommon)
                    // prefer the RuntimeInformation.IsOSPlatform() approach.
                    OperatingSystem os = Environment.OSVersion;
                    if (os.Platform == PlatformID.Win32NT) {
                        IsPlatformWindows = DNY.Yes;
                    } else {
                        IsPlatformWindows = DNY.No;
                    }
                }
                return (IsPlatformWindows == DNY.Yes);
            }
        }

        /// <summary>
        /// Selects all items in the list view.
        /// </summary>
        public static void SelectAll(this ListView listView) {
            // Neither I nor the Internet can figure out how to do this efficiently for
            // large lists without P/Invoke interop.  With 554253 lines, it takes 24.3 seconds
            // to select each item individually, but only 4 milliseconds to do it through an
            // LVITEM.  The latter causes a single VirtualItemsSelectionRangeChanged event
            // instead of 554K ItemSelectionChanged events.
            //
            // https://stackoverflow.com/questions/9039989/
            // https://stackoverflow.com/questions/1019388/

            if (IsWindows) {
                NativeMethods.SelectAllItems(listView);
            } else {
                try {
                    Application.UseWaitCursor = true;
                    Cursor.Current = Cursors.WaitCursor;
                    listView.BeginUpdate();
                    int max = listView.VirtualListSize;
                    for (int i = 0; i < max; i++) {
                        //codeListView.Items[i].Selected = true;
                        listView.SelectedIndices.Add(i);
                    }
                } finally {
                    listView.EndUpdate();
                    Application.UseWaitCursor = false;
                }
            }
        }

        /// <summary>
        /// Deselects all items in the list view.
        /// </summary>
        public static void DeselectAll(this ListView listView) {
            // This is as fast as the native DeselectAllItems(), so just use it.
            listView.SelectedIndices.Clear();
        }

        /// <summary>
        /// Sets the double-buffered status of the list view.
        /// </summary>
        public static void SetDoubleBuffered(this ListView listView, bool enable) {
            WinFormsUtil.SetDoubleBuffered(listView, enable);
        }

        /// <summary>
        /// Determines whether the specified item is visible in the list view.
        /// </summary>
        public static bool IsItemVisible(this ListView listView, ListViewItem item) {
            Rectangle lvBounds = listView.ClientRectangle;
            if (listView.HeaderStyle != ColumnHeaderStyle.None) {
                // Need to factor the header height out.  There's no easy way to do that,
                // but the header should be (almost) the same height as an item.
                // https://stackoverflow.com/q/538906/294248
                int headerHeight = item.Bounds.Height + 5;  // 5 is magic, will probably break
                lvBounds = new Rectangle(lvBounds.X, lvBounds.Y + headerHeight,
                    lvBounds.Width, lvBounds.Height - headerHeight);
            }
            //Console.WriteLine("IsVis LV: " + lvBounds + " IT: " +
            //    item.GetBounds(ItemBoundsPortion.Entire));
            return lvBounds.IntersectsWith(item.GetBounds(ItemBoundsPortion.Entire));
        }
    }

    public static class WinFormsUtil {
        /// <summary>
        /// Sets the "DoubleBuffered" property on a Control.  For some reason the
        /// property is defined as "protected", but I don't want to subclass a ListView
        /// just so I can enable double-buffering.
        /// </summary>
        /// <param name="ctrl">Control to update.</param>
        /// <param name="enable">New state.</param>
        public static void SetDoubleBuffered(Control ctrl, bool enable) {
            System.Reflection.PropertyInfo prop = ctrl.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            prop.SetValue(ctrl, enable, null);
        }
    }
}
