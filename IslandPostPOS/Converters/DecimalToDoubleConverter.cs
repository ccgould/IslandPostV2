using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace IslandPostPOS.Converters;

public class DecimalToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is decimal d ? (double)d : 0.0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is double dbl ? (decimal)dbl : 0m;
    }
}
