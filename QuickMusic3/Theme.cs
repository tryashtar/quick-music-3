using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickMusic3;

public class Theme
{
    public Brush Background { get; set; }
    public Brush AlbumBackground { get; set; }
    public Brush Text { get; set; }
    public Brush SubtleText { get; set; }
    public Brush BarUnfilled { get; set; }
    public Brush BarUnfilledChapter { get; set; }
    public Brush BarFilled { get; set; }
    public Brush BarFilledChapter { get; set; }
    public Brush BarButton { get; set; }
    public Brush Button { get; set; }
    public Brush ButtonOutline { get; set; }
    public Brush ButtonHover { get; set; }
    public Brush Icon { get; set; }
    public Brush Header { get; set; }
    public Brush PlaylistBackground { get; set; }
    public Brush PlaylistHighlight { get; set; }
    public Brush PlaylistActive { get; set; }

    public static readonly Dictionary<string, Theme> DefaultThemes = new()
    {
        {
            "light",
            new Theme()
            {
                Background = new SolidColorBrush(Color.FromRgb(0xf0, 0xf0, 0xf0)),
                AlbumBackground = Brushes.White,
                Text = Brushes.Black,
                SubtleText = new SolidColorBrush(Color.FromRgb(0xa0, 0xa0, 0xa0)),
                BarUnfilled = new SolidColorBrush(Color.FromRgb(0xc8, 0xc8, 0xc8)),
                BarUnfilledChapter = new SolidColorBrush(Color.FromRgb(0xc0, 0xc0, 0xc0)),
                BarFilled = new SolidColorBrush(Color.FromRgb(0x7f, 0xb2, 0xf0)),
                BarFilledChapter = new SolidColorBrush(Color.FromRgb(100, 140, 189)),
                BarButton = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xd7)),
                Button = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd)),
                ButtonOutline = new SolidColorBrush(Color.FromRgb(0x70, 0x70, 0x70)),
                ButtonHover = new SolidColorBrush(Color.FromRgb(0xc0, 0xc0, 0xc0)),
                Icon = Brushes.Black,
                Header = new SolidColorBrush(Color.FromRgb(0xeb, 0xf0, 0xf5)),
                PlaylistBackground = Brushes.White,
                PlaylistHighlight = new SolidColorBrush(Color.FromRgb(0xbd, 0xe7, 0xff)),
                PlaylistActive = new SolidColorBrush(Color.FromRgb(0xd9, 0xff, 0xdb))
            }
        },
        {
            "dark",
            new Theme()
            {
                Background = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
                AlbumBackground = Brushes.Black,
                Text = Brushes.White,
                SubtleText = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90)),
                BarUnfilled = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x44)),
                BarUnfilledChapter = new SolidColorBrush(Color.FromRgb(0x4f, 0x4f, 0x4f)),
                BarFilled = new SolidColorBrush(Color.FromRgb(0x2d, 0x5d, 0xba)),
                BarFilledChapter = new SolidColorBrush(Color.FromRgb(0x2d, 0x50, 0xba)),
                BarButton = new SolidColorBrush(Color.FromRgb(0x52, 0x86, 0xe6)),
                Button = new SolidColorBrush(Color.FromRgb(0x30, 0x30, 0x30)),
                ButtonOutline = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11)),
                ButtonHover = new SolidColorBrush(Color.FromRgb(0x60, 0x60, 0x60)),
                Icon = Brushes.White,
                Header = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x11)),
                PlaylistBackground = new SolidColorBrush(Color.FromRgb(0x20, 0x20, 0x20)),
                PlaylistHighlight = new SolidColorBrush(Color.FromRgb(0x1e, 0x56, 0x75)),
                PlaylistActive = new SolidColorBrush(Color.FromRgb(0x1d, 0x73, 0x55))
            }
        }
    };
}
