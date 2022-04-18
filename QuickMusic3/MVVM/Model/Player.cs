using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace QuickMusic3.MVVM.Model;

public class Player : ObservableObject, IDisposable
{
    private MagicStream Stream;
    private WaveOutEvent Output;
    private readonly Timer Timer;
    public LoadableStream CurrentTrack => Stream?.CurrentTrack;
    public PlaybackState PlayState
    {
        get
        {
            if (Output == null)
                return PlaybackState.Paused;
            if (Output.PlaybackState == PlaybackState.Stopped)
                return PlaybackState.Paused;
            return Output.PlaybackState;
        }
    }
    public RepeatMode RepeatMode
    {
        get { return (RepeatMode)Properties.Settings.Default.RepeatMode; }
        set
        {
            Properties.Settings.Default.RepeatMode = (int)value;
            if (Stream != null)
                Stream.RepeatMode = value;
            OnPropertyChanged();
        }
    }
    public float Volume
    {
        get { return Properties.Settings.Default.Volume; }
        set
        {
            Properties.Settings.Default.Volume = value;
            Muted = false;
            UpdateVolume();
            OnPropertyChanged();
        }
    }
    public bool Muted
    {
        get { return Properties.Settings.Default.Muted; }
        set
        {
            Properties.Settings.Default.Muted = value;
            UpdateVolume();
            OnPropertyChanged();
        }
    }
    public bool Shuffle
    {
        get { return Properties.Settings.Default.Shuffle; }
        set
        {
            Properties.Settings.Default.Shuffle = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan CurrentTime
    {
        get => Stream?.CurrentTime + MissingTime.Elapsed ?? TimeSpan.Zero;
        set
        {
            if (Stream != null)
                Stream.Position = (long)(value.TotalSeconds * Stream.WaveFormat.AverageBytesPerSecond);
        }
    }
    public TimeSpan TotalTime => Stream?.TotalTime ?? TimeSpan.Zero;

    public Player()
    {
        Timer = new() { Enabled = false };
        Timer.Elapsed += Timer_Elapsed;
    }

    // Stream.Position doesn't update very often since it reads from the stream in chunks
    // so we fill in the missing time ourselves
    private readonly Stopwatch MissingTime = new();
    private long LastPosition;
    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (LastPosition != Stream.Position)
        {
            LastPosition = Stream.Position;
            MissingTime.Restart();
        }
        OnPropertyChanged(nameof(CurrentTime));
        // System.Diagnostics.Debug.WriteLine($"Loaded: {String.Join(", ", Stream.Sources.Where(x => x.IsStreamLoaded).Select(x => System.IO.Path.GetFileNameWithoutExtension(x.Path)))}");
    }

    public void OpenFiles(string[] files)
    {
        if (Stream != null)
        {
            Stream.CurrentChanged -= Stream_CurrentChanged;
            Stream.Seeked -= Stream_Seeked;
        }
        Stream = new(files);
        Stream.RepeatMode = (RepeatMode)Properties.Settings.Default.RepeatMode;
        Stream.CurrentChanged += Stream_CurrentChanged;
        Stream.Seeked += Stream_Seeked;
        Output?.Dispose();
        Output = new();
        UpdateVolume();
        Output.Init(Stream);
        Stream_CurrentChanged(this, EventArgs.Empty);
        Play();
    }

    private void UpdateVolume()
    {
        if (Output != null)
            Output.Volume = Muted ? 0 : Volume * Volume;
    }

    private void Stream_CurrentChanged(object sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentTrack));
        OnPropertyChanged(nameof(CurrentTime));
        OnPropertyChanged(nameof(TotalTime));
    }

    private void Stream_Seeked(object sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CurrentTime));
    }

    public void Play()
    {
        Output?.Play();
        Timer.Enabled = true;
        MissingTime.Restart();
        OnPropertyChanged(nameof(CurrentTime));
        OnPropertyChanged(nameof(PlayState));
    }

    public void Pause()
    {
        // using stop instead of pause fixes audio blips when seeking
        Output?.Stop();
        Timer.Enabled = false;
        MissingTime.Reset();
        OnPropertyChanged(nameof(PlayState));
    }

    public void Next()
    {
        if (Stream != null)
            Stream.CurrentIndex++;
    }

    public void Prev()
    {
        if (Stream != null)
            Stream.CurrentIndex--;
    }

    public void Dispose()
    {
        Timer.Enabled = false;
        Timer.Elapsed -= Timer_Elapsed;
        Timer.Dispose();
        MissingTime.Reset();
        if (Stream != null)
            Stream.CurrentChanged -= Stream_CurrentChanged;
        Stream?.Dispose();
    }
}
