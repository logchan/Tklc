using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tklc.CommandLine {
    public class CommandOutput<T> where T : CommandContext, new() {

        public T Context { get; }
        public List<OutputEntry> Entries { get; } = new List<OutputEntry>();

        public CommandOutput(Guid id) {
            Context = new T {
                Id = id,
                Output = new EntryWriter(Entries),
                ErrorOutput = new EntryWriter(Entries, true)
            };
        }

        public sealed class OutputEntry {
            public string Text { get; set; }
            public bool Error { get; set; }
        }

        private sealed class EntryWriter : TextWriter {
            private readonly IList<OutputEntry> _entries;
            private readonly StringBuilder _sb = new StringBuilder();

            public bool Error { get; }
            public override Encoding Encoding => Encoding.UTF8;

            public EntryWriter(IList<OutputEntry> entries, bool error = false) {
                _entries = entries;
                Error = error;
            }

            public override void Write(char value) {
                _sb.Append(value);
            }

            public override void Write(string value) {
                _sb.Append(value);
            }

            public override void WriteLine() {
                Flush();
            }

            public override void WriteLine(string value) {
                _sb.AppendLine(value);
                Flush();
            }

            public override void Flush() {
                _entries.Add(new OutputEntry {
                    Text = _sb.ToString(),
                    Error = Error
                });
                _sb.Clear();
            }
        }
    }
}
