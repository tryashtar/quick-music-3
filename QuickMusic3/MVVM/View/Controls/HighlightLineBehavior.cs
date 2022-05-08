using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace QuickMusic3.MVVM.View;

// https://stackoverflow.com/a/60474831/4904863
public static class HighlightTextBehavior
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
        "Text",
        typeof(string),
        typeof(HighlightTextBehavior),
        new FrameworkPropertyMetadata("", OnTextChanged));

    public static string GetText(FrameworkElement frameworkElement) => (string)frameworkElement.GetValue(TextProperty);
    public static void SetText(FrameworkElement frameworkElement, string value) => frameworkElement.SetValue(TextProperty, value);

    public static readonly DependencyProperty LineToBeHighlightedProperty = DependencyProperty.RegisterAttached(
        "LineToBeHighlighted",
        typeof(int),
        typeof(HighlightTextBehavior),
        new FrameworkPropertyMetadata(-1, OnTextChanged));

    public static int GetLineToBeHighlighted(FrameworkElement frameworkElement)
    {
        return (int)frameworkElement.GetValue(LineToBeHighlightedProperty);
    }

    public static void SetLineToBeHighlighted(FrameworkElement frameworkElement, int value)
    {
        frameworkElement.SetValue(LineToBeHighlightedProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBlock textBlock)
            SetTextBlockTextAndHighlightTerm(textBlock, GetText(textBlock), GetLineToBeHighlighted(textBlock));
    }

    private static void SetTextBlockTextAndHighlightTerm(TextBlock textBlock, string text, int lineToBeHighlighted)
    {
        textBlock.Text = string.Empty;
        if (text.Length == 0)
            return;

        if (lineToBeHighlighted < 0)
        {
            AddPartToTextBlock(textBlock, text);
            return;
        }
        string[] lines = text.Split('\n');
        if (lineToBeHighlighted >= lines.Length)
        {
            AddPartToTextBlock(textBlock, text);
            return;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            if (i == lineToBeHighlighted)
                AddHighlightedPartToTextBlock(textBlock, lines[i] + "\n");
            else
                AddPartToTextBlock(textBlock, lines[i] + "\n");
        }
    }

    private static void AddPartToTextBlock(TextBlock textBlock, string part)
    {
        textBlock.Inlines.Add(new Run { Text = part });
    }

    private static void AddHighlightedPartToTextBlock(TextBlock textBlock, string part)
    {
        var run = new Run { Text = part };
        run.SetBinding(Run.BackgroundProperty, new Binding("Shared.ActiveTheme.BarFilled"));
        textBlock.Inlines.Add(run);
    }
}
