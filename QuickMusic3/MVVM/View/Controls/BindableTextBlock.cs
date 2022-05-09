using System;
using System.Collections;
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

public class BindableTextBlock : TextBlock
{
    static BindableTextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BindableTextBlock),
            new FrameworkPropertyMetadata(typeof(BindableTextBlock)));
    }

    public static readonly DependencyProperty LineTemplateProperty =
        DependencyProperty.Register(nameof(LineTemplate), typeof(ControlTemplate), typeof(BindableTextBlock), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnLinesChanged)));
    public ControlTemplate LineTemplate
    {
        get { return (ControlTemplate)GetValue(LineTemplateProperty); }
        set { SetValue(LineTemplateProperty, value); }
    }

    public static readonly DependencyProperty LinesProperty =
        DependencyProperty.Register(nameof(Lines), typeof(IEnumerable), typeof(BindableTextBlock), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnLinesChanged)));
    public IEnumerable Lines
    {
        get { return (IEnumerable)GetValue(LinesProperty); }
        set { SetValue(LinesProperty, value); }
    }

    private static void OnLinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var b = (BindableTextBlock)d;
        b.Inlines.Clear();
        if (b.Lines == null)
            return;
        var template = b.LineTemplate;
        var all = new List<Inline>();
        foreach (var line in b.Lines)
        {
            var content = (Inline)template.LoadContent();
            content.DataContext = line;
            all.Add(content);
        }
        b.Inlines.AddRange(all);
    }
}
