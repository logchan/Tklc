using System;

namespace Tklc.CommandLine
{
    public interface ICommandHandler
    {
        string Command { get; }
        string[] Aliases { get; }
        Type OptionType { get; }
    }
}
