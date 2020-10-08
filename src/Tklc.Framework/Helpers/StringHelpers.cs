using System;

namespace Tklc.Framework.Helpers {
    public static class StringHelpers {
        /// <summary>
        /// Convert a number to an appropriate unit, which makes the number as small as possible but still larger than 1.<br/>
        /// For example, given ratio=1024 and units={"B","KB","MB"}, 123,456B converts to 120.56KB, and 123,456,789B converts to 117.73MB.
        /// </summary>
        /// <param name="number">The number in the smallest unit.</param>
        /// <param name="ratio">The ratio between consecutive units.</param>
        /// <param name="units">All available units.</param>
        /// <returns>Converted number.</returns>
        public static (double, string) ConvertUnits(double number, double ratio, params string[] units) {
            if (ratio <= 0) {
                throw new ArgumentOutOfRangeException(nameof(ratio), "Ratio must be positive");
            }
            if (units.Length == 0) {
                throw new ArgumentException("At least one unit is required", nameof(units));
            }

            var idx = 0;
            while (Math.Abs(number) > ratio && idx < units.Length - 1) {
                number /= ratio;
                ++idx;
            }

            return (number, units[idx]);
        }
    }
}
