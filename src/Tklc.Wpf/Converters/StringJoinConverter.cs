using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Tklc.Wpf.Converters {
    [ValueConversion(typeof(IEnumerable<string>), typeof(string))]
    public class StringJoinConverter : IValueConverter {
        public string Separator { get; set; } = ", ";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is IEnumerable<string> enm))
                return String.Empty;
            return String.Join(Separator, enm);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
