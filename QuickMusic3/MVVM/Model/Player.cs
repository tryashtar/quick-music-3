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
                Stream.CurrentTime = value;
        }
    }
    public TimeSpan TotalTime => Stream?.TotalTime ?? TimeSpan.Zero;

    public Player()
    {
        Timer = new() { Enabled = false, Interval = 50 };
        Timer.Elapsed += Timer_Elapsed;
    }

    // Stream.Position doesn't update very often since it reads from the stream in chunks
    // so we fill in the missing time ourselves
    private readonly Stopwatch MissingTime = new();
    private long LastPosition;
    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (LastPosition != Stream.CurrentTime.Ticks)
        {
            LastPosition = Stream.CurrentTime.Ticks;
            // Debug.WriteLine(MissingTime.ElapsedMilliseconds);
            MissingTime.Restart();
        }
        OnPropertyChanged(nameof(CurrentTime));
        // Debug.WriteLine($"Loaded: {String.Join(", ", Stream.Sources.Where(x => x.IsStreamLoaded).Select(x => System.IO.Path.GetFileNameWithoutExtension(x.Path)))}");
    }

    public Playlist Playlist { get; private set; }
    public void Open(Playlist playlist)
    {
        Close();
        Playlist = playlist;
        Stream = new(playlist);
        Stream.RepeatMode = (RepeatMode)Properties.Settings.Default.RepeatMode;
        Stream.CurrentChanged += Stream_CurrentChanged;
        Stream.Seeked += Stream_Seeked;
        Output = new();
        Output.PlaybackStopped += Output_PlaybackStopped;
        UpdateVolume();
        Output.Init(Stream);
        Stream_CurrentChanged(this, EventArgs.Empty);
    }

    // if you spam forward a lot, it stops sometimes with an error
    private void Output_PlaybackStopped(object sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            Debug.WriteLine(e.Exception.ToString());
            Play();
        }
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
        if (Output != null)
        {
            Output.Play();
            Timer.Enabled = true;
            MissingTime.Restart();
            OnPropertyChanged(nameof(CurrentTime));
            OnPropertyChanged(nameof(PlayState));
        }
    }

    public void Pause()
    {
        if (Output != null)
        {
            // using stop instead of pause fixes audio blips when seeking
            Output.Stop();
            Timer.Enabled = false;
            MissingTime.Reset();
            OnPropertyChanged(nameof(PlayState));
        }
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

    private void Close()
    {
        if (Output != null)
        {
            Output.PlaybackStopped -= Output_PlaybackStopped;
            Output.Dispose();
        }
        if (Stream != null)
        {
            Stream.CurrentChanged -= Stream_CurrentChanged;
            Stream.Seeked -= Stream_Seeked;
            Stream.Dispose();
        }
        Timer.Enabled = false;
        MissingTime.Reset();
    }

    public void Dispose()
    {
        Close();
        Timer.Elapsed -= Timer_Elapsed;
        Timer.Dispose();
    }
}
