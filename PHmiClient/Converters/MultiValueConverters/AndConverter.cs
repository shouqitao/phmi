using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PHmiClient.Converters.MultiValueConverters {
    public class AndConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.All(value => value as bool? == true);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}