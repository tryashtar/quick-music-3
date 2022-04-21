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

namespace QuickMusic3.MVVM.View;

public class Theme : DependencyObject
{
    public Brush Background { get; set; }
    public Brush AlbumBackground { get; set; }
    public Brush Text { get; set; }
    public Brush SubtleText { get; set; }
    public Brush BarUnfilled { get; set; }
    public Brush BarFilled { get; set; }
    public Brush BarButton { get; set; }
    public Brush Button { get; set; }
    public Brush ButtonOutline { get; set; }
    public Brush ButtonHover { get; set; }
    public Brush Icon { get; set; }
}
