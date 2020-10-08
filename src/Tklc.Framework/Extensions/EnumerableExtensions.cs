using System.Collections.Generic;
using System.Linq;

namespace Tklc.Framework.Extensions {
    public static class EnumerableExtensions {
        public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T first, out IEnumerable<T> remaining) {
            first = enumerable.First();
            remaining = enumerable.Skip(1);
        }

        public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T first, out T second, out IEnumerable<T> remaining) {
            (first, (second, remaining)) = enumerable;
        }

        public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T first, out T second, out T third, out IEnumerable<T> remaining) {
            (first, (second, (third, remaining))) = enumerable;
        }

        public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T first, out T second, out T third, out T fourth, out IEnumerable<T> remaining) {
            (first, (second, (third, (fourth, remaining)))) = enumerable;
        }

        public static void Deconstruct<T>(this IEnumerable<T> enumerable, out T first, out T second, out T third, out T fourth, out T fifth, out IEnumerable<T> remaining) {
            (first, (second, (third, (fourth, (fifth, remaining))))) = enumerable;
        }
    }
}
