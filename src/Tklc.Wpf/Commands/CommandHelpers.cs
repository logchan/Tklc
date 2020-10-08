using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Tklc.Wpf.Commands {
    public static class CommandHelpers {
        /// <summary>
        /// Add a UIElement's CommandBindings according to its static RoutedCommand properties.
        /// A command of name CMD needs two methods: CMD_CanExecute and CMD_Executed.
        /// </summary>
        public static void RegisterRoutedCommands(this UIElement element) {
            var type = element.GetType();

            var properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public);
            foreach (var prop in properties) {
                if (prop.PropertyType != typeof(RoutedCommand))
                    continue;

                var name = prop.Name;
                var canExecute = type.GetMethod($"{name}_CanExecute",
                    new[] { typeof(object), typeof(CanExecuteRoutedEventArgs) });
                if (canExecute == null)
                    throw new InvalidOperationException($"Missing CanExecute method for command {name}");
                var executed = type.GetMethod($"{name}_Executed",
                    new[] { typeof(object), typeof(ExecutedRoutedEventArgs) });
                if (executed == null)
                    throw new InvalidOperationException($"Missing Executed method for command {name}");

                element.CommandBindings.Add(new CommandBinding(
                    (ICommand)prop.GetValue(null),
                    (ExecutedRoutedEventHandler)Delegate.CreateDelegate(typeof(ExecutedRoutedEventHandler), element,
                        executed),
                    (CanExecuteRoutedEventHandler)Delegate.CreateDelegate(typeof(CanExecuteRoutedEventHandler), element,
                        canExecute)
                ));
            }
        }
    }
}
