using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace QuickMusic3.MVVM.Model;

public class Player : ObservableObject, IDisposable
{
    private MagicStream Stream;
    private WasapiOut Output;
    private Timer Timer;
    public PlaybackState PlayState => Output?.PlaybackState ?? PlaybackState.Stopped;
    public TimeSpan CurrentTime
    {
        get => Stream?.CurrentTime ?? TimeSpan.Zero;
        set
        {
            if (Stream != null)
                Stream.CurrentTime = value;
        }
    }
    public TimeSpan TotalTime => Stream?.TotalTime ?? TimeSpan.Zero;

    public Player()
    {
        Timer = new(5) { Enabled = false };
        Timer.Elapsed += Timer_Elapsed;
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        OnPropertyChanged(nameof(CurrentTime));
    }

    public void OpenFiles(string[] files)
    {
        if (Stream != null)
            Stream.CurrentChanged -= Stream_CurrentChanged;
        Stream = new(files);
        Output?.Dispose();
        Output = new();
        Output.Init(Stream);
        Stream.CurrentChanged += Stream_CurrentChanged;
        OnPropertyChanged(nameof(CurrentTime));
        OnPropertyChanged(nameof(TotalTime));
        Play();
    }

    private void Stream_CurrentChanged(object sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentTime));
        OnPropertyChanged(nameof(TotalTime));
    }

    public void Play()
    {
        Output?.Play();
        Timer.Enabled = true;
        OnPropertyChanged(nameof(CurrentTime));
        OnPropertyChanged(nameof(PlayState));
    }

    public void Pause()
    {
        Output?.Pause();
        Timer.Enabled = false;
        OnPropertyChanged(nameof(PlayState));
    }

    public void Dispose()
    {
        Timer.Enabled = false;
        Timer.Elapsed -= Timer_Elapsed;
        Timer.Dispose();
        if (Stream != null)
            Stream.CurrentChanged -= Stream_CurrentChanged;
        Stream?.Dispose();
    }
}
