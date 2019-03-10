﻿using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using PHmiClient.Controls;

namespace PHmiClient.Converters {
    public class DisplayItemConverter : ChainConverter {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var enumerable = parameter as IEnumerable;
            if (enumerable == null)
                return null;
            return (from DisplayItem item in enumerable
                where item.Value.Equals(value)
                select item.DisplayValue).FirstOrDefault();
        }

        public override object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}