﻿using NAudio.Wave;
using QuickMusic3.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TryashtarUtils.Utility;

namespace QuickMusic3.Converters;

public class AlbumArtFitter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        double height = (double)values[1];
        if (values[0] is not ImageSource img)
            return height;
        return height * img.Width / img.Height;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}

public class ProgressDivvy : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is double value && values[1] is double total && values[2] is bool remaining && total > 0)
        {
            double result = value / total;
            if (remaining)
                result = 1 - result;
            return new GridLength(result, GridUnitType.Star);
        }
        return new GridLength(1, GridUnitType.Star);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }

    public static readonly ProgressDivvy Instance = new ProgressDivvy();
}
