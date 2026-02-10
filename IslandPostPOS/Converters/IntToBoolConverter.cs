using Microsoft.UI.Xaml.Data;
using System;

namespace IslandPostPOS.Converters
{
    internal class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is int val)
            {
                return val == 1;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is bool val)
            {
                return val == true ? 1 : 0;
            }

            return 0;
        }
    }
}
