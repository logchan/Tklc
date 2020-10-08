using System;
using System.Text.RegularExpressions;

namespace Tklc.IO {
    public static partial class IOHelpers {

        private static readonly Regex _digitRegex = new Regex(@"(\d+)");

        /// <summary>
        /// Compare two file names in natural order.
        /// </summary>
        public static int FileNameNaturalCompare(string a, string b) {
            if (a == null || b == null) {
                return String.Compare(a, b);
            }

            var ma = _digitRegex.Match(a);
            var mb = _digitRegex.Match(b);
            var startA = 0;
            var startB = 0;
            while (true) {
                if (!ma.Success || !mb.Success) {
                    return String.Compare(a, b);
                }

                var prefixA = a.Substring(startA, ma.Index - startA);
                var prefixB = b.Substring(startB, mb.Index - startB);
                var cmp = String.Compare(prefixA, prefixB);
                if (cmp != 0) {
                    return cmp;
                }

                var valA = ma.Value.TrimStart('0');
                var valB = mb.Value.TrimStart('0');
                cmp = valA.Length.CompareTo(valB.Length);
                if (cmp != 0) {
                    return cmp;
                }

                cmp = valA.CompareTo(valB);
                if (cmp != 0) {
                    return cmp;
                }

                startA = ma.Index + ma.Length;
                startB = mb.Index + mb.Length;
                ma = ma.NextMatch();
                mb = mb.NextMatch();
            }
        }
    }
}
