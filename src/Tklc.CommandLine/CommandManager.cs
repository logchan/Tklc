using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Language;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Clp = CommandLine;

namespace Tklc.CommandLine {
    public class CommandManager {
        private readonly Dictionary<string, ICommandHandler> _handlers = new Dictionary<string, ICommandHandler>();
        private static readonly TokenKind[] _excludedTokenKinds = { TokenKind.EndOfInput };

        public void AddHandler(ICommandHandler handler) {
            void AddCommand(string name, ICommandHandler h) {
                if (_handlers.TryGetValue(name, out var existing)) {
                    throw new Exception(
                        $"Command name or alias {name} is duplicate in {handler.GetType().Name} and {existing.GetType().Name}");
                }

                _handlers.Add(name, h);
            }

            _checkType.MakeGenericMethod(handler.OptionType).Invoke(null, new object[] { handler });

            AddCommand(handler.Command, handler);
            Array.ForEach(handler.Aliases, n => AddCommand(n, handler));
        }

        public void AddHandlers(IEnumerable<ICommandHandler> handlers) {
            foreach (var handler in handlers) {
                AddHandler(handler);
            }
        }

        public void Execute(string commandLine, CommandContext context) {
            System.Management.Automation.Language.Parser.ParseInput(commandLine, out var tokens, out var errors);
            if (errors.Length > 0) {
                context.ErrorOutput.WriteLine(String.Join(Environment.NewLine,
                    from error in errors
                    select error.Message));
                return;
            }

            if (tokens.Length == 0) {
                return;
            }

            var name = tokens[0].Text;
            if (name.Length == 0) {
                return;
            }

            var args = tokens.Skip(1).Where(t => !_excludedTokenKinds.Contains(t.Kind)).Select(t => {
                var text = t.Text;
                if (text.Length > 1 && text[0] == '"' && text[text.Length - 1] == '"') {
                    return text.Substring(1, text.Length - 2);
                }

                return text;
            }).ToArray();

            Execute(name, args, context);
        }

        public void Execute(string command, string[] args, CommandContext context) {
            if (!_handlers.TryGetValue(command, out var handler)) {
                context.ErrorOutput.WriteLine($"Unrecognized command \"{command}\"");
                return;
            }

            var optType = handler.OptionType;
            var func = _executeWithHandlerAndArgs.MakeGenericMethod(optType);
            func.Invoke(this, new object[] { handler, args, context });
        }

        private static readonly MethodInfo _executeWithHandlerAndArgs = 
            typeof(CommandManager).GetMethod("Execute",
            BindingFlags.Instance | BindingFlags.Public, null,
            new[] { typeof(ICommandHandler), typeof(string[]), typeof(CommandContext) }, null);
        public void Execute<T>(ICommandHandler handler, string[] args, CommandContext context) {
            var parser = new Clp.Parser(settings =>
            {
                settings.AutoHelp = true;
                settings.AutoVersion = false;
                settings.HelpWriter = null;
                settings.IgnoreUnknownArguments = false;
                settings.CaseSensitive = false;
            });

            var result = parser.ParseArguments<T>(args).WithParsed(options => Execute(handler, options, context));
            result.WithNotParsed(errors => {
                var help = new HelpText(String.Empty, String.Empty) {
                    AutoHelp = true,
                    AutoVersion = false,
                    AddDashesToOption = true
                };
                help = HelpText.DefaultParsingErrorsHandler(result, help);
                help = help.AddOptions(result);

                var errorList = errors.ToList();
                if (errorList.Count == 0 ||
                    errorList.Count == 1 && errorList[0].Tag == ErrorType.HelpRequestedError) {
                    context.Output.WriteLine(help);
                }
                else {
                    context.ErrorOutput.WriteLine(help);
                }
            });
        }

        public void Execute<T>(ICommandHandler handler, T options, CommandContext context) {
            CheckType<T>(handler).Execute(options, context);
        }

        private static readonly MethodInfo _checkType =
            typeof(CommandManager).GetMethod("CheckType", BindingFlags.Static | BindingFlags.NonPublic);
        private static CommandHandler<T> CheckType<T>(ICommandHandler handler) {
            if (!(handler is CommandHandler<T> typedHandler)) {
                throw new Exception($"The command handler {handler.Command} ({handler.GetType().FullName}) does not inherit from CommandHandler<{typeof(T).Name}>");
            }

            return typedHandler;
        }
    }
}
