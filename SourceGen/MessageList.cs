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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using MainWindow = SourceGen.WpfGui.MainWindow;

namespace SourceGen {
    /// <summary>
    /// List of problems and oddities noted during analysis of the project.
    /// </summary>
    /// <remarks>
    /// Could also make a place for load-time problems, though those tend to be resolved by
    /// discarding the offending data, so not much value in continuing to report them.
    /// </remarks>
    public class MessageList : IEnumerable<MessageList.MessageEntry> {
        /// <summary>
        /// One message entry.
        /// 
        /// Instances are immutable.
        /// </summary>
        public class MessageEntry {
            public enum SeverityLevel {
                Unknown = 0,
                Info,
                Warning,
                Error
            }
            public SeverityLevel Severity { get; private set; }

            public int Offset { get; private set; }

            public enum MessageType {
                Unknown = 0,
                HiddenLabel,
                HiddenLocalVariableTable,
                UnresolvedWeakRef,
                InvalidOffsetOrLength,
                InvalidDescriptor,
                BankOverrun,
            }
            public MessageType MsgType { get; private set; }

            // Context object.  Could be a label string, a format descriptor, etc.
            public object Context { get; private set; }

            public enum ProblemResolution {
                Unknown = 0,
                None,
                LabelIgnored,
                LocalVariableTableIgnored,
                FormatDescriptorIgnored,
            }
            public ProblemResolution Resolution { get; private set; }


            public MessageEntry(SeverityLevel severity, int offset, MessageType mtype,
                    object context, ProblemResolution resolution) {
                Severity = severity;
                Offset = offset;
                MsgType = mtype;
                Context = context;
                Resolution = resolution;
            }

            public override string ToString() {
                return Severity.ToString() + " +" + Offset.ToString("x6") + " " +
                    MsgType + "(" + Context.ToString() + "): " + Resolution;
            }
        }

        /// <summary>
        /// Maximum file offset.  Used to flag offsets as invalid.
        /// </summary>
        //public int MaxOffset { get; set; }

        /// <summary>
        /// List of messages.  This is not kept in sorted order, because the DataGrid used to
        /// display it will do the sorting for us.  Call the Sort() function to establish an
        /// initial sort.
        /// </summary>
        private List<MessageEntry> mList;


        /// <summary>
        /// Constructor.
        /// </summary>
        public MessageList() {
            mList = new List<MessageEntry>();
        }

        // IEnumerable
        public IEnumerator<MessageEntry> GetEnumerator() {
            // .Values is documented as O(1)
            return mList.GetEnumerator();
        }

        // IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return mList.GetEnumerator();
        }

        public int Count {
            get { return mList.Count; }
        }

        public void Add(MessageEntry entry) {
            mList.Add(entry);
        }

        public void Clear() {
            mList.Clear();
        }

        /// <summary>
        /// Sorts the list, by severity then offset.
        /// </summary>
        public void Sort() {
            mList.Sort(delegate (MessageEntry a, MessageEntry b) {
                if (a.Severity != b.Severity) {
                    return (int)b.Severity - (int)a.Severity;
                }
                return a.Offset - b.Offset;
            });
        }

        /// <summary>
        /// Formats a message for display.
        /// </summary>
        public static MainWindow.MessageListItem FormatMessage(MessageEntry entry,
                Asm65.Formatter formatter) {
            string severity = entry.Severity.ToString();        // enum
            string offset = formatter.FormatOffset24(entry.Offset);

            string problem;
            switch (entry.MsgType) {
                case MessageEntry.MessageType.HiddenLabel:
                    problem = Res.Strings.MSG_HIDDEN_LABEL;
                    break;
                case MessageEntry.MessageType.HiddenLocalVariableTable:
                    problem = Res.Strings.MSG_HIDDEN_LOCAL_VARIABLE_TABLE;
                    break;
                case MessageEntry.MessageType.UnresolvedWeakRef:
                    problem = Res.Strings.MSG_UNRESOLVED_WEAK_REF;
                    break;
                case MessageEntry.MessageType.InvalidOffsetOrLength:
                    problem = Res.Strings.MSG_INVALID_OFFSET_OR_LENGTH;
                    break;
                case MessageEntry.MessageType.InvalidDescriptor:
                    problem = Res.Strings.MSG_INVALID_DESCRIPTOR;
                    break;
                case MessageEntry.MessageType.BankOverrun:
                    problem = Res.Strings.MSG_BANK_OVERRUN;
                    break;
                default:
                    problem = "???";
                    break;
            }

            string context = entry.Context.ToString();

            string resolution;
            switch (entry.Resolution) {
                case MessageEntry.ProblemResolution.None:
                    resolution = string.Empty;
                    break;
                case MessageEntry.ProblemResolution.LabelIgnored:
                    resolution = Res.Strings.MSG_LABEL_IGNORED;
                    break;
                case MessageEntry.ProblemResolution.LocalVariableTableIgnored:
                    resolution = Res.Strings.MSG_LOCAL_VARIABLE_TABLE_IGNORED;
                    break;
                case MessageEntry.ProblemResolution.FormatDescriptorIgnored:
                    resolution = Res.Strings.MSG_FORMAT_DESCRIPTOR_IGNORED;
                    break;
                default:
                    resolution = "???";
                    break;
            }

            return new MainWindow.MessageListItem(severity, entry.Offset, offset, problem,
                context, resolution);
        }

        public void DebugDump() {
            Debug.WriteLine("Message list (" + mList.Count + " entries):");
            foreach (MessageEntry entry in mList) {
                Debug.WriteLine(entry);
            }
        }
    }
}
