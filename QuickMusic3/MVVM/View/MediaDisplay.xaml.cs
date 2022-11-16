using Microsoft.Win32;
using NAudio.Wave;
using QuickMusic3.Core;
using QuickMusic3.MVVM.Model;
using QuickMusic3.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;
using TryashtarUtils.Music;

namespace QuickMusic3.MVVM.View;

public partial class MediaDisplay : UserControl
{
    public static readonly DependencyProperty MetadataFontSizeProperty =
            DependencyProperty.Register(nameof(MetadataFontSize), typeof(double),
            typeof(MediaDisplay), new FrameworkPropertyMetadata(30d));
    public static readonly DependencyProperty TopRightProperty =
            DependencyProperty.Register(nameof(TopRight), typeof(FrameworkElement),
            typeof(MediaDisplay), new FrameworkPropertyMetadata());
    public static readonly DependencyProperty MediaControlsProperty =
            DependencyProperty.Register(nameof(MediaControls), typeof(MediaControls),
            typeof(MediaDisplay), new FrameworkPropertyMetadata());
    public static readonly DependencyProperty LyricsEnabledProperty =
            DependencyProperty.Register(nameof(LyricsEnabled), typeof(bool),
            typeof(MediaDisplay), new FrameworkPropertyMetadata(true));
    public double MetadataFontSize
    {
        get { return (double)GetValue(MetadataFontSizeProperty); }
        set { SetValue(MetadataFontSizeProperty, value); }
    }
    public FrameworkElement TopRight
    {
        get { return (FrameworkElement)GetValue(TopRightProperty); }
        set { SetValue(TopRightProperty, value); }
    }
    public MediaControls MediaControls
    {
        get { return (MediaControls)GetValue(MediaControlsProperty); }
        set { SetValue(MediaControlsProperty, value); }
    }
    public bool LyricsEnabled
    {
        get { return (bool)GetValue(LyricsEnabledProperty); }
        set { SetValue(LyricsEnabledProperty, value); }
    }
    public BaseViewModel Model => (BaseViewModel)DataContext;

    private NestedListener<PlaylistStream>? Listener;
    public MediaDisplay()
    {
        InitializeComponent();
        this.DataContextChanged += (s, e) =>
        {
            if (Listener != null)
                Listener.ItemChanged -= Listener_ItemChanged;
            if (DataContext is BaseViewModel)
            {
                Listener = new NestedListener<PlaylistStream>(Model.Shared.Player, nameof(Player.Stream));
                Listener.ItemChanged += Listener_ItemChanged;
            }
        };
    }

    private void Listener_ItemChanged(object? sender, (PlaylistStream item, string propertyName) e)
    {
    }

    private int LastLineIndex = -1;
    private void ScrollLine()
    {
        if (this.DataContext == null)
            return;
        //var current = ((BaseViewModel)this.DataContext).Shared.Player.CurrentLine;
        //var element = (FrameworkElement)LyricsBox.ItemContainerGenerator.ContainerFromItem(current);
        //if (element != null)
        //{
        //    int index = ((BaseViewModel)this.DataContext).Shared.Player.Stream.CurrentTrack.Metadata.Item.Lyrics.Lines.IndexOf(current);
        //    if (LastLineIndex != -1 && index != -1 && index > LastLineIndex)
        //    {
        //        for (int i = 0; i < index - LastLineIndex; i++)
        //        {
        //            LyricsScroller.LineDown();
        //        }
        //    }
        //    else if (LastLineIndex != -1 && index != -1 && index < LastLineIndex)
        //    {
        //        for (int i = 0; i < LastLineIndex - index; i++)
        //        {
        //            LyricsScroller.LineUp();
        //        }
        //    }
        //    element.BringIntoView();
        //    LastLineIndex = index;
        //}
    }

    private void Lyric_MouseDown(object? sender, MouseButtonEventArgs e)
    {
        var player = ((BaseViewModel)this.DataContext).Shared.Player;
        if (player.Stream.CurrentTrack.Metadata.Item.Lyrics.Synchronized)
        {
            var entry = (LyricsEntry)((FrameworkElement)sender).DataContext;
            player.CurrentTime = entry.Start;
            //LastLineIndex = player.Stream.CurrentTrack.Metadata.Item.Lyrics.Lines.IndexOf(entry);
        }
    }
}
