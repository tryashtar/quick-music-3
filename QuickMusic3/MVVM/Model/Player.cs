using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using YamlDotNet.Core.Tokens;

namespace QuickMusic3.MVVM.Model;

public sealed class Player : ObservableObject, IDisposable
{
    private ShufflableSource? Source;
    private PlaylistStream? Stream;
    private WaveOutEvent? Output;
    private readonly Timer Timer;
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
    public bool IsShuffled => Properties.Settings.Default.Shuffle;
    public TimeSpan TotalTime => Stream?.CurrentStream?.BaseStream?.TotalTime ?? TimeSpan.Zero;
    public TimeSpan CurrentTime
    {
        get
        {
            var stream = Stream?.CurrentStream?.BaseStream;
            if (stream == null)
                return TimeSpan.Zero;
            return stream.CurrentTime + MissingTime.Elapsed;
        }
        set
        {
            var stream = Stream?.CurrentStream?.BaseStream;
            if (stream == null)
            {
                Debug.WriteLine("Tried to set CurrentTime but current track isn't loaded :(");
                return;
            }
            long position = (long)(value.TotalSeconds * stream.WaveFormat.AverageBytesPerSecond);
            position = Math.Max(0, position);
            if (position > stream.Length)
            {
                NextAsync().Wait();
                position = 0;
            }
            stream.Position = position;
            OnPropertyChanged();
            if (MissingTime.IsRunning)
                MissingTime.Restart();
        }
    }

    public int PlaylistPosition => (Stream?.CurrentIndex ?? 0) + 1;
    public int PlaylistTotal => Stream?.Playlist.Count ?? 0;

    public Player()
    {
        Timer = new() { Enabled = false, Interval = 50 };
        Timer.Elapsed += Timer_Elapsed;
    }

    // Stream.Position doesn't update very often since it reads from the stream in chunks
    // so we fill in the missing time ourselves
    private readonly Stopwatch MissingTime = new();
    private long LastPosition;
    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        var stream = Stream?.CurrentStream?.BaseStream;
        if (stream == null)
            Timer.Stop();
        else
        {
            if (LastPosition != stream.CurrentTime.Ticks)
            {
                LastPosition = stream.CurrentTime.Ticks;
                MissingTime.Restart();
            }
            OnPropertyChanged(nameof(CurrentTime));
        }
    }

    public async Task SwitchToAsync(SongFile song)
    {
        int index = Stream.Playlist.IndexOf(song);
        if (index != -1)
            await Stream.SetIndexAsync(index, 1);
    }

    public async Task OpenAsync(ISongSource? playlist, int? first_index = 0)
    {
        Close();
        if (playlist == null)
        {
            Stream = null;
            Output = null;
        }
        else
        {
            Source = new ShufflableSource(playlist);
            if (IsShuffled)
                await Task.Run(() => Source.Shuffle(first_index));
            Stream = new(Source);
            Stream.RepeatMode = (RepeatMode)Properties.Settings.Default.RepeatMode;
            Stream.PropertyChanged += Stream_PropertyChanged;
            if (!IsShuffled && first_index.HasValue)
                await Stream.SetIndexAsync(first_index.Value, 1);
            else
                await Stream.SetIndexAsync(0, 1);
            Output = new();
            Output.PlaybackStopped += Output_PlaybackStopped;
            UpdateVolume();
            Output.Init(Stream);
        }
        OnPropertyChanged(nameof(PlaylistPosition));
        OnPropertyChanged(nameof(PlaylistTotal));
        OnPropertyChanged(nameof(CurrentTime));
        OnPropertyChanged(nameof(TotalTime));
    }

    private void Stream_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Stream.CurrentTrack))
        {
            OnPropertyChanged(nameof(CurrentTime));
            OnPropertyChanged(nameof(TotalTime));
        }
        else if (e.PropertyName == nameof(Stream.CurrentIndex))
        {
            OnPropertyChanged(nameof(PlaylistPosition));
            OnPropertyChanged(nameof(PlaylistTotal));
        }
    }

    public async Task SetShuffleAsync(bool value)
    {
        Properties.Settings.Default.Shuffle = value;
        if (Source != null && Stream != null)
        {
            if (value)
                await Task.Run(() => Source.Shuffle(Stream.CurrentIndex));
            else
                await Task.Run(() => Source.Unshuffle());
        }
        OnPropertyChanged(nameof(IsShuffled));
    }

    public async Task FreshShuffleAsync()
    {
        Properties.Settings.Default.Shuffle = true;
        if (Source != null && Stream != null)
        {
            await Task.Run(() => Source.Shuffle());
            await Stream.SetIndexAsync(0, 1);
        }
        OnPropertyChanged(nameof(IsShuffled));
    }

    // if you spam forward a lot, it stops sometimes with an error
    private void Output_PlaybackStopped(object? sender, StoppedEventArgs e)
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

    public async Task NextAsync()
    {
        if (Stream != null)
            await Stream.SetIndexAsync(Stream.CurrentIndex + 1, 1);
    }

    public async Task PreviousAsync()
    {
        if (Stream != null)
            await Stream.SetIndexAsync(Stream.CurrentIndex - 1, -1);
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
            Stream.PropertyChanged -= Stream_PropertyChanged;
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
