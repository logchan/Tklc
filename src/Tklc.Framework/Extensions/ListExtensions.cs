using System.Collections.Generic;

namespace Tklc.Framework.Extensions {
    public static class ListExtensions {
        public static void Deconstruct<T>(this IList<T> list, out T first) {
            first = list[0];
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out T second) {
            first = list[0];
            second = list[1];
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out T third) {
            first = list[0];
            second = list[1];
            third = list[2];
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out T third, out T fourth) {
            first = list[0];
            second = list[1];
            third = list[2];
            fourth = list[3];
        }

        public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out T third, out T fourth, out T fifth) {
            first = list[0];
            second = list[1];
            third = list[2];
            fourth = list[3];
            fifth = list[4];
        }

        public static void Fill<T>(this IList<T> list, int count, T value) {
            while (list.Count < count) {
                list.Add(value);
            }
        }
    }
}
