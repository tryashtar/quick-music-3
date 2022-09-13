using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TryashtarUtils.Music;

namespace QuickMusic3.MVVM.Model;

public class Player : ObservableObject, IDisposable
{
    private PlaylistStream Stream;
    private WaveOutEvent Output;
    private readonly Timer Timer;
    public SongFile CurrentTrack => Stream?.CurrentTrack;
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
            if (value)
                Playlist.Shuffle(Stream.CurrentIndex);
            else
                Playlist.Unshuffle();
            OnPropertyChanged();
        }
    }

    public string CurrentChapter
    {
        get
        {
            if (CurrentTrack == null)
                return null;
            if (CurrentTrack.Metadata.Item.Chapters == null)
                return null;
            var chapter = CurrentTrack.Metadata.Item.Chapters.ChapterAtTime(CurrentTime);
            return chapter?.Title;
        }
    }

    public LyricsEntry CurrentLine { get; private set; }

    public TimeSpan CurrentTime
    {
        get => Stream?.CurrentTime + MissingTime.Elapsed ?? TimeSpan.Zero;
        set
        {
            if (Stream != null)
            {
                Stream.CurrentTime = value;
                if (MissingTime.IsRunning)
                    MissingTime.Restart();
            }
        }
    }
    public TimeSpan TotalTime => Stream?.TotalTime ?? TimeSpan.Zero;
    public int PlaylistPosition => (Stream?.CurrentIndex ?? 0) + 1;
    public int PlaylistTotal => Stream?.Playlist.Count ?? 1;

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
        TimeChanged();
        // Debug.WriteLine($"Loaded: {String.Join(", ", Stream.Sources.Where(x => x.IsStreamLoaded).Select(x => System.IO.Path.GetFileNameWithoutExtension(x.Path)))}");
    }

    private void TimeChanged()
    {
        OnPropertyChanged(nameof(CurrentTime));
        OnPropertyChanged(nameof(CurrentChapter));
        var line = GetCurrentLine();
        if (line != CurrentLine)
        {
            CurrentLine = line;
            OnPropertyChanged(nameof(CurrentLine));
        }
    }

    private LyricsEntry GetCurrentLine()
    {
        if (CurrentTrack == null)
            return null;
        if (CurrentTrack.Metadata.Item.Lyrics == null)
            return null;
        if (!CurrentTrack.Metadata.Item.Lyrics.Synchronized)
            return null;
        return CurrentTrack.Metadata.Item.Lyrics.LyricAtTime(CurrentTime);
    }

    public void SwitchTo(SongFile song)
    {
        int index = Playlist.IndexOf(song);
        if (index != -1)
            Stream.CurrentIndex = index;
    }

    public ISongSource RawSource { get; private set; }
    public ShufflableSource Playlist { get; private set; }
    public void Open(ISongSource playlist, int? first_index = 0)
    {
        Close();
        RawSource = playlist;
        if (Stream != null)
            Stream.Dispose();
        if (playlist == null)
        {
            Playlist = null;
            Stream = null;
            Output = null;
        }
        else
        {
            Playlist = new ShufflableSource(playlist);
            if (Shuffle)
                Playlist.Shuffle(first_index);
            //if (!first_index.HasValue)
            //    Playlist.GetInOrder(0, true);
            Stream = new(Playlist);
            Stream.RepeatMode = (RepeatMode)Properties.Settings.Default.RepeatMode;
            Stream.PropertyChanged += Stream_PropertyChanged;
            if (!Shuffle && first_index.HasValue)
                Stream.CurrentIndex = first_index.Value;
            Output = new();
            Output.PlaybackStopped += Output_PlaybackStopped;
            UpdateVolume();
            Output.Init(Stream);
        }
        OnPropertyChanged(nameof(Playlist));
        OnPropertyChanged(nameof(PlaylistPosition));
        OnPropertyChanged(nameof(PlaylistTotal));
        OnPropertyChanged(nameof(CurrentTrack));
        TimeChanged();
        OnPropertyChanged(nameof(TotalTime));
    }

    private void Stream_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Stream.CurrentTrack))
        {
            OnPropertyChanged(nameof(CurrentTrack));
            TimeChanged();
            OnPropertyChanged(nameof(TotalTime));
        }
        else if (e.PropertyName == nameof(Stream.CurrentTime))
            TimeChanged();
        else if (e.PropertyName == nameof(Stream.CurrentIndex))
            OnPropertyChanged(nameof(PlaylistPosition));
    }

    public void FreshShuffle()
    {
        Properties.Settings.Default.Shuffle = true;
        Playlist.Shuffle();
        Stream.CurrentIndex = 0;
        OnPropertyChanged(nameof(Shuffle));
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

    public void Play()
    {
        if (Output != null)
        {
            Output.Play();
            Timer.Enabled = true;
            MissingTime.Restart();
            TimeChanged();
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

public class PlayHistory
{
    private readonly Player Parent;
    private record Entry(ISongSource Source, SongFile Song, TimeSpan Time, PlaybackState State);
    private int CurrentIndex = 0;
    private readonly List<Entry> History;

    public PlayHistory(Player parent)
    {
        Parent = parent;
        History = new();
    }

    private Entry MakeCurrentEntry()
    {
        return new Entry(Parent.RawSource, Parent.CurrentTrack, Parent.CurrentTime, Parent.PlayState);
    }

    public void Add()
    {
        if (CurrentIndex < History.Count - 1)
            History.RemoveRange(CurrentIndex + 1, History.Count - CurrentIndex - 1);
        History.Add(MakeCurrentEntry());
        if (History.Count > 1)
            CurrentIndex++;
    }

    private void SwitchTo(Entry entry)
    {
        if (Parent.RawSource != entry.Source)
            Parent.Open(entry.Source);
        Parent.SwitchTo(entry.Song);
        Parent.CurrentTime = entry.Time;
        if (entry.State == PlaybackState.Stopped || entry.State == PlaybackState.Paused)
            Parent.Pause();
        else if (entry.State == PlaybackState.Playing)
            Parent.Play();
    }

    public void Forward()
    {
        if (History.Count == 0)
            return;
        History[CurrentIndex] = MakeCurrentEntry();
        if (CurrentIndex < History.Count - 1)
        {
            CurrentIndex++;
            SwitchTo(History[CurrentIndex]);
        }
    }

    public void Backward()
    {
        if (History.Count == 0)
            return;
        History[CurrentIndex] = MakeCurrentEntry();
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            SwitchTo(History[CurrentIndex]);
        }
    }
}
