using System;

namespace Tklc.CommandLine {
    public abstract class CommandHandler<T> : ICommandHandler {
        public abstract string Command { get; }
        public virtual string[] Aliases => new string[] { };
        public Type OptionType => typeof(T);

        public abstract void Execute(T options, CommandContext context);
    }
}
