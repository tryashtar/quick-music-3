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

public class CustomSlider : Slider
{
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
        // calling the base makes it so if you click off the slider, then move onto it and release, it jumps
        //base.OnPreviewMouseLeftButtonDown(e);
    }
}
