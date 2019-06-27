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
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace CommonWPF {
    /// <summary>
    /// Miscellaneous helper functions.
    /// </summary>
    public static class Helper {
        /// <summary>
        /// Measures the size of a string when rendered with the specified parameters.  Uses
        /// the current culture, left-to-right flow, and 1 pixel per DIP.
        /// </summary>
        /// <remarks>
        /// The Graphics.MeasureString approach from WinForms doesn't work in WPF, but we can
        /// accomplish the same thing with the FormattedText class.
        /// </remarks>
        /// <seealso cref="https://stackoverflow.com/a/9266288/294248"/>
        /// <param name="str">Text to be displayed.</param>
        /// <param name="fontFamily">Font family for Typeface.</param>
        /// <param name="fontStyle">Font style for Typeface.</param>
        /// <param name="fontWeight">Font weight for Typeface.</param>
        /// <param name="fontStretch">Font stretch for Typeface.</param>
        /// <param name="emSize">Font size.</param>
        /// <returns>Width and height of rendered text.</returns>
        public static Size MeasureString(string str, FontFamily fontFamily, FontStyle fontStyle,
                FontWeight fontWeight, FontStretch fontStretch, double emSize) {
            FormattedText fmt = new FormattedText(
                str,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                emSize,
                Brushes.Black,
                new NumberSubstitution(),
                1.0);

            return new Size(fmt.Width, fmt.Height);
        }

        /// <summary>
        /// Measures the size of a string when rendered with the specified parameters.  Uses
        /// the current culture, left-to-right flow, and 1 pixel per DIP.
        /// </summary>
        /// <param name="str">Text to be displayed.</param>
        /// <param name="typeface">Font typeface to use.</param>
        /// <param name="emSize">Font size.</param>
        /// <returns>Width of rendered text.</returns>
        public static double MeasureStringWidth(string str, Typeface typeface, double emSize) {
            FormattedText fmt = new FormattedText(
                str,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                emSize,
                Brushes.Black,
                new NumberSubstitution(),
                1.0);

            return fmt.Width;
        }
    }
}
