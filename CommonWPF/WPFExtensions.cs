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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CommonWPF {
    /// <summary>
    /// Generic Visual helper.
    /// </summary>
    public static class VisualHelper {
        /// <summary>
        /// Find a child object in a WPF visual tree.
        /// </summary>
        /// <remarks>
        /// Sample usage:
        ///   GridViewHeaderRowPresenter headerRow = listView.GetVisualChild&lt;GridViewHeaderRowPresenter&gt;();
        ///   
        /// From https://social.msdn.microsoft.com/Forums/vstudio/en-US/7d0626cb-67e8-4a09-a01e-8e56ee7411b2/gridviewcolumheader-radiobuttons?forum=wpf
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="referenceVisual"></param>
        /// <returns></returns>
        public static T GetVisualChild<T>(this Visual referenceVisual) where T : Visual {
            Visual child = null;
            for (Int32 i = 0; i < VisualTreeHelper.GetChildrenCount(referenceVisual); i++) {
                child = VisualTreeHelper.GetChild(referenceVisual, i) as Visual;
                if (child != null && child is T) {
                    break;
                } else if (child != null) {
                    child = GetVisualChild<T>(child);
                    if (child != null && child is T) {
                        break;
                    }
                }
            }
            return child as T;
        }
    }

    /// <summary>
    /// Add functions to get the element that's currently shown at the top of the ListView
    /// window, and to scroll the list so that a specific item is at the top.
    /// </summary>
    public static class ListViewExtensions {
        /// <summary>
        /// Figures out which item index is at the top of the window.  This only works for a
        /// ListView with a VirtualizingStackPanel.
        /// </summary>
        /// <returns>The item index, or -1 if the list is empty.</returns>
        public static int GetTopItemIndex(this ListView lv) {
            if (lv.Items.Count == 0) {
                return -1;
            }

            VirtualizingStackPanel vsp = lv.GetVisualChild<VirtualizingStackPanel>();
            if (vsp == null) {
                Debug.Assert(false, "ListView does not have a VirtualizingStackPanel");
                return -1;
            }
            return (int) vsp.VerticalOffset;
        }

        /// <summary>
        /// Scrolls the ListView so that the specified item is at the top.  The standard
        /// ListView.ScrollIntoView() makes the item visible but doesn't ensure a
        /// specific placement.
        /// </summary>
        /// <remarks>
        /// Equivalent to setting myListView.TopItem in WinForms.
        /// </remarks>
        public static void ScrollToTopItem(this ListView lv, object item) {
            ScrollViewer sv = lv.GetVisualChild<ScrollViewer>();
            sv.ScrollToBottom();
            lv.ScrollIntoView(item);
        }
    }
}
