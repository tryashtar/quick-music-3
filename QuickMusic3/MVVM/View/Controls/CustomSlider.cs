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
/// <summary>
/// Interaction logic for CustomSlider.xaml
/// </summary>
public partial class CustomSlider : Slider
{
    static CustomSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSlider),
            new FrameworkPropertyMetadata(typeof(CustomSlider)));
    }

    public static readonly DependencyProperty ThumbButtonStyleProperty =
        DependencyProperty.Register(nameof(ThumbButtonStyle), typeof(Style), typeof(CustomSlider), new FrameworkPropertyMetadata());
    public Style ThumbButtonStyle
    {
        get { return (Style)GetValue(ThumbButtonStyleProperty); }
        set { SetValue(ThumbButtonStyleProperty, value); }
    }
    public static readonly DependencyProperty InvisibleHeightProperty =
        DependencyProperty.Register(nameof(InvisibleHeight), typeof(double), typeof(CustomSlider), new FrameworkPropertyMetadata());
    public double InvisibleHeight
    {
        get { return (double)GetValue(InvisibleHeightProperty); }
        set { SetValue(InvisibleHeightProperty, value); }
    }
    public static readonly DependencyProperty BarHeightProperty =
        DependencyProperty.Register(nameof(BarHeight), typeof(double), typeof(CustomSlider), new FrameworkPropertyMetadata());
    public double BarHeight
    {
        get { return (double)GetValue(BarHeightProperty); }
        set { SetValue(BarHeightProperty, value); }
    }
    public static readonly DependencyProperty ProgressBarStyleProperty =
        DependencyProperty.Register(nameof(ProgressBarStyle), typeof(Style), typeof(CustomSlider), new FrameworkPropertyMetadata());
    public Style ProgressBarStyle
    {
        get { return (Style)GetValue(ProgressBarStyleProperty); }
        set { SetValue(ProgressBarStyleProperty, value); }
    }
    public static readonly DependencyProperty RemainingBarStyleProperty =
        DependencyProperty.Register(nameof(RemainingBarStyle), typeof(Style), typeof(CustomSlider), new FrameworkPropertyMetadata());
    public Style RemainingBarStyle
    {
        get { return (Style)GetValue(RemainingBarStyleProperty); }
        set { SetValue(RemainingBarStyleProperty, value); }
    }

    private bool clickedInSlider;
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && this.clickedInSlider)
        {
            var thumb = (Template.FindName("PART_Track", this) as System.Windows.Controls.Primitives.Track).Thumb;
            thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonDownEvent,
                Source = e.Source
            });
        }
        base.OnMouseMove(e);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        clickedInSlider = true;
        base.OnPreviewMouseLeftButtonDown(e);
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        clickedInSlider = false;
        base.OnPreviewMouseLeftButtonUp(e);
    }
}
