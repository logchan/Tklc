using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tklc.Framework {
    public static class ReflectionHelpers {
        /// <summary>
        /// Get all types in an assembly that implements any of the specified interfaces.
        /// </summary>
        public static IEnumerable<Type> GetInterfaceImplementations(Assembly assembly, IEnumerable<Type> interfaces) {
            return from type in assembly.GetTypes()
                   where interfaces.Any(t => t.IsAssignableFrom(type)) && !type.IsInterface && !type.IsAbstract
                   select type;
        }
    }
}
