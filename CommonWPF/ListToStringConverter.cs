/*
 * Copyright 2020 faddenSoft
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
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CommonWPF {
    /// <summary>
    /// Converts a List&lt;string&gt; to a multi-line string, suitable for presentation
    /// in a TextBlock or other UI element.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/a/345515/294248
    ///
    /// In XAML, reference with:
    ///   xmlns:common="clr-namespace:CommonWPF;assembly=CommonWPF"
    /// </remarks>
    [ValueConversion(typeof(List<string>), typeof(string))]
    public class ListToStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (targetType != typeof(string)) {
                Debug.WriteLine("Invalid targetType for string conversion");
                return DependencyProperty.UnsetValue;
            }

            return string.Join("\r\n", ((List<string>)value).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }
    }
}
