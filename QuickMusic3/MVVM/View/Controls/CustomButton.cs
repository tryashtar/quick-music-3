using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickMusic3.MVVM.View;

public class CustomButton : Button
{
    static CustomButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomButton),
            new FrameworkPropertyMetadata(typeof(CustomButton)));
    }

    public static readonly DependencyProperty NoHoverProperty =
        DependencyProperty.Register(nameof(NoHover), typeof(Brush), typeof(CustomButton), new FrameworkPropertyMetadata());
    public Brush NoHover
    {
        get { return (Brush)GetValue(NoHoverProperty); }
        set { SetValue(NoHoverProperty, value); }
    }

    public static readonly DependencyProperty HoverProperty =
        DependencyProperty.Register(nameof(Hover), typeof(Brush), typeof(CustomButton), new FrameworkPropertyMetadata());
    public Brush Hover
    {
        get { return (Brush)GetValue(HoverProperty); }
        set { SetValue(HoverProperty, value); }
    }
}
