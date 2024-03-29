﻿using NAudio.Wave;
using QuickMusic3.MVVM.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TryashtarUtils.Utility;

namespace QuickMusic3.Converters;

public class EqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0)
            return false;
        for (int i = 1; i < values.Length; i++)
        {
            if (values[0] != values[i])
                return false;
        }
        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}

public class ContainsConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Any(x => x == null || x == DependencyProperty.UnsetValue))
            return false;
        return ((dynamic)values[0]).Contains((dynamic)values[1]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}
