using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tklc.Wpf.Converters {
    public class ValueToVisibilityConverter : IValueConverter {
        public IComparable VisibleValue { get; set; }
        public IComparable CollapsedValue { get; set; }
        public IComparable HiddenValue { get; set; }
        public Visibility DefaultVisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is IComparable ic) {
                return ic.CompareTo(VisibleValue) == 0 ? Visibility.Visible :
                    ic.CompareTo(CollapsedValue) == 0 ? Visibility.Collapsed :
                    ic.CompareTo(HiddenValue) == 0 ? Visibility.Hidden :
                    DefaultVisibility;
            }

            return DefaultVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is Visibility visibility) {
                switch (visibility) {
                    case Visibility.Visible:
                        return VisibleValue;
                    case Visibility.Collapsed:
                        return CollapsedValue;
                    case Visibility.Hidden:
                        return HiddenValue;
                }
            }

            return null;
        }
    }
}