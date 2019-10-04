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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SourceGen.Tools.WpfGui {
    /// <summary>
    /// Floating problem list window.
    /// </summary>
    public partial class ProblemListViewer : Window, INotifyPropertyChanged {
        public class ProblemsListItem {
            public string Severity { get; private set; }
            public string Offset { get; private set; }
            public string Type { get; private set; }
            public string Context { get; private set; }
            public string Resolution { get; private set; }

            public ProblemsListItem(string severity, string offset, string type, string context,
                    string resolution) {
                Severity = severity;
                Offset = offset;
                Type = type;
                Context = context;
                Resolution = resolution;
            }
        }

        /// <summary>
        /// Reference to project.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Text formatter.
        /// </summary>
        private Asm65.Formatter mFormatter;

        /// <summary>
        /// ItemsSource for DataGrid.
        /// </summary>
        public ObservableCollection<ProblemsListItem> FormattedProblems { get; private set; } =
            new ObservableCollection<ProblemsListItem>();

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ProblemListViewer(Window owner, DisasmProject project, Asm65.Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = project;
            mFormatter = formatter;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Update();
        }

        /// <summary>
        /// Updates the contents of the DataGrid to the current values in mProject.Problems.
        /// </summary>
        public void Update() {
            FormattedProblems.Clear();

            foreach (ProblemList.ProblemEntry entry in mProject.Problems) {
                ProblemsListItem newItem = FormatEntry(entry);
                FormattedProblems.Add(newItem);
            }
        }

        private ProblemsListItem FormatEntry(ProblemList.ProblemEntry entry) {
            string severity = entry.Severity.ToString();        // enum
            string offset = mFormatter.FormatOffset24(entry.Offset);

            string problem;
            switch (entry.Problem) {
                case ProblemList.ProblemEntry.ProblemType.HiddenLabel:
                    problem = (string)FindResource("str_HiddenLabel");
                    break;
                case ProblemList.ProblemEntry.ProblemType.UnresolvedWeakRef:
                    problem = (string)FindResource("str_UnresolvedWeakRef");
                    break;
                case ProblemList.ProblemEntry.ProblemType.InvalidOffsetOrLength:
                    problem = (string)FindResource("str_InvalidOffsetOrLength");
                    break;
                case ProblemList.ProblemEntry.ProblemType.InvalidDescriptor:
                    problem = (string)FindResource("str_InvalidDescriptor");
                    break;
                default:
                    problem = "???";
                    break;
            }

            string context = entry.Context.ToString();

            string resolution;
            switch (entry.Resolution) {
                case ProblemList.ProblemEntry.ProblemResolution.LabelIgnored:
                    resolution = (string)FindResource("str_LabelIgnored");
                    break;
                case ProblemList.ProblemEntry.ProblemResolution.FormatDescriptorIgnored:
                    resolution = (string)FindResource("str_FormatDescriptorIgnored");
                    break;
                default:
                    resolution = "???";
                    break;
            }

            return new ProblemsListItem(severity, offset, problem, context, resolution);
        }
    }
}
