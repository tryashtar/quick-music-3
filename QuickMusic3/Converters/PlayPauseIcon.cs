using NAudio.Wave;
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
