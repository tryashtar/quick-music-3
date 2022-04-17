using NAudio.Wave;
using QuickMusic3.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TryashtarUtils.Utility;

namespace QuickMusic3.Converters;

public class PlayPauseIcon : IMultiValueConverter
{
    private static readonly BitmapFrame Pause = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/pause.png"));
    private static readonly BitmapFrame Play = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/play.png"));
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // fix for designer exploding
        if (values[0] is not bool)
            return null;
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

[ValueConversion(typeof(RepeatMode), typeof(BitmapSource))]
public class RepeatModeIcon : OneWayConverter<RepeatMode, BitmapSource>
{
    private static readonly BitmapFrame RepeatAll = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/repeat_all.png"));
    private static readonly BitmapFrame RepeatOne = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/repeat_one.png"));
    private static readonly BitmapFrame PlayAll = BitmapFrame.Create(new Uri("pack://application:,,,/Resources/once.png"));

    public override BitmapSource Convert(RepeatMode value)
    {
        if (value == RepeatMode.RepeatAll)
            return RepeatAll;
        if (value == RepeatMode.RepeatOne)
            return RepeatOne;
        return PlayAll;
    }
}

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
