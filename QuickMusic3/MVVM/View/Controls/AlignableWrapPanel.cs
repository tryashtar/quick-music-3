﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace QuickMusic3.MVVM.View;

// https://gist.github.com/gmanny/7450651
public class AlignableWrapPanel : Panel
{
    public HorizontalAlignment HorizontalContentAlignment
    {
        get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
        set { SetValue(HorizontalContentAlignmentProperty, value); }
    }
    public int Rows
    {
        get { return (int)GetValue(RowsProperty.DependencyProperty); }
        private set { SetValue(RowsProperty, value); }
    }

    public static readonly DependencyProperty HorizontalContentAlignmentProperty =
        DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(AlignableWrapPanel), new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));
    public static readonly DependencyPropertyKey RowsProperty =
        DependencyProperty.RegisterReadOnly("Rows", typeof(int), typeof(AlignableWrapPanel), new FrameworkPropertyMetadata());

    protected override Size MeasureOverride(Size constraint)
    {
        int rows = 1;
        Size curLineSize = new Size();
        Size panelSize = new Size();

        UIElementCollection children = InternalChildren;

        for (int i = 0; i < children.Count; i++)
        {
            UIElement child = children[i];

            // Flow passes its own constraint to children
            child.Measure(constraint);
            Size sz = child.DesiredSize;

            if (curLineSize.Width + sz.Width > constraint.Width) //need to switch to another line
            {
                panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
                panelSize.Height += curLineSize.Height;
                curLineSize = sz;
                rows++;

                if (sz.Width > constraint.Width) // if the element is wider then the constraint - give it a separate line                    
                {
                    panelSize.Width = Math.Max(sz.Width, panelSize.Width);
                    panelSize.Height += sz.Height;
                    curLineSize = new Size();
                }
            }
            else //continue to accumulate a line
            {
                curLineSize.Width += sz.Width;
                curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
            }
        }

        // the last line size, if any need to be added
        panelSize.Width = Math.Max(curLineSize.Width, panelSize.Width);
        panelSize.Height += curLineSize.Height;

        if (rows != Rows)
            Rows = rows;
        return panelSize;
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        int firstInLine = 0;
        Size curLineSize = new Size();
        double accumulatedHeight = 0;
        UIElementCollection children = InternalChildren;

        for (int i = 0; i < children.Count; i++)
        {
            Size sz = children[i].DesiredSize;

            if (curLineSize.Width + sz.Width > arrangeBounds.Width) //need to switch to another line
            {
                ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, i);

                accumulatedHeight += curLineSize.Height;
                curLineSize = sz;

                if (sz.Width > arrangeBounds.Width) //the element is wider then the constraint - give it a separate line                    
                {
                    ArrangeLine(accumulatedHeight, sz, arrangeBounds.Width, i, ++i);
                    accumulatedHeight += sz.Height;
                    curLineSize = new Size();
                }
                firstInLine = i;
            }
            else //continue to accumulate a line
            {
                curLineSize.Width += sz.Width;
                curLineSize.Height = Math.Max(sz.Height, curLineSize.Height);
            }
        }

        if (firstInLine < children.Count)
            ArrangeLine(accumulatedHeight, curLineSize, arrangeBounds.Width, firstInLine, children.Count);

        return arrangeBounds;
    }

    private void ArrangeLine(double y, Size lineSize, double boundsWidth, int start, int end)
    {
        UIElementCollection children = InternalChildren;

        Dictionary<HorizontalAlignment, List<UIElement>> controls = new Dictionary<HorizontalAlignment, List<UIElement>>
                {
                    {HorizontalAlignment.Left, new List<UIElement>()},
                    {HorizontalAlignment.Center, new List<UIElement>()},
                    {HorizontalAlignment.Right, new List<UIElement>()}
                };

        // sort line contents by alignment
        for (int i = start; i < end; i++)
        {
            UIElement child = children[i];
            HorizontalAlignment alignment = HorizontalContentAlignment;

            FrameworkElement element = child as FrameworkElement;
            if (element != null)
            {
                alignment = element.HorizontalAlignment;
            }

            // check
            if (alignment == HorizontalAlignment.Stretch)
            {
                throw new InvalidOperationException(HorizontalAlignment.Stretch + " horizontal alignment isn't supported.");
            }

            // put element into the hash
            List<UIElement> list = controls[alignment];

            list.Add(child);
        }

        // calculate center gap size
        double centerGap = (boundsWidth - lineSize.Width) / 2;

        double x = 0.0;
        foreach (HorizontalAlignment alignment in new[] { HorizontalAlignment.Left, HorizontalAlignment.Center, HorizontalAlignment.Right })
        {
            // get element list
            List<UIElement> list = controls[alignment];

            // arrange all elements
            foreach (UIElement child in list)
            {
                child.Arrange(new Rect(x, y, child.DesiredSize.Width, lineSize.Height));
                x += child.DesiredSize.Width;
            }

            // move a center gap
            x += centerGap;
        }
    }
}
