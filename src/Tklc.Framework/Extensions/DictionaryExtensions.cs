using System;
using System.Collections.Generic;

namespace Tklc.Framework.Extensions {
    public static class DictionaryExtensions {
        /// <summary>
        /// If key is in dictionary, returns dict[key]. Otherwise, construct a TValue object with the constructor function, put it in the dictionary, and returns the object.
        /// </summary>
        /// <returns>Value contained in the dictionary.</returns>
        public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TValue> constructor) {
            if (!dict.TryGetValue(key, out var value)) {
                value = constructor();
                dict[key] = value;
            }
            return value;
        }
    }
}
