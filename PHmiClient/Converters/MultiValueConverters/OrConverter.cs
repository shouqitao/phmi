using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PHmiClient.Converters.MultiValueConverters {
    public class OrConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.Any(value => value as bool? == true);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}