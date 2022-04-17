using NAudio.Wave;
using QuickMusic3.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TryashtarUtils.Utility;

namespace QuickMusic3.Converters;

public class PlayPauseIcon : IMultiValueConverter
{
    private static readonly BitmapFrame Pause = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/pause.png"));
    private static readonly BitmapFrame Play = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/play.png"));
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)values[0])
            return Pause;
        if ((PlaybackState)values[1] == PlaybackState.Playing)
            return Pause;
        return Play;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}

public class RepeatModeIcon : IValueConverter
{
    private static readonly BitmapFrame RepeatAll = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/repeat_all.png"));
    private static readonly BitmapFrame RepeatOne = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/repeat_one.png"));
    private static readonly BitmapFrame PlayAll = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/once.png"));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var mode = (RepeatMode)value;
        if (mode == RepeatMode.RepeatAll)
            return RepeatAll;
        if (mode == RepeatMode.RepeatOne)
            return RepeatOne;
        return PlayAll;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException();
    }
}
