﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ReactivePlot.View.Common
{
    public class TrackerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}