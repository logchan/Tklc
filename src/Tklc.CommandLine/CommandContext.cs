using System;
using System.IO;

namespace Tklc.CommandLine {
    public class CommandContext {
        public Guid Id { get; set; }
        public TextWriter Output { get; set; }
        public TextWriter ErrorOutput { get; set; }

        public CommandContext() {}
        public CommandContext(Guid id, TextWriter output, TextWriter errorOutput) {
            Id = id;
            Output = output;
            ErrorOutput = errorOutput;
        }
    }
}
