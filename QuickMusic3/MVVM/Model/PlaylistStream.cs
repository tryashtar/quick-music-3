using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using QuickMusic3.Core;

namespace QuickMusic3.MVVM.Model;

public class PlaylistStream : ObservableObject, IWaveProvider, IDisposable
{
    public readonly ISongSource Playlist;
    private int current_index;
    public int CurrentIndex
    {
        get => current_index;
        set
        {
            current_index = FixIndex(value);
            SetCurrentTrack();
        }
    }
    public RepeatMode RepeatMode { get; set; }
    public SongFile CurrentTrack { get; private set; }
    private MutableStream CurrentStream => CurrentTrack.Stream.Item;
    private WaveStream CurrentBase => CurrentStream.BaseStream;
    private IWaveProvider CurrentPlayable => CurrentStream.PlayableStream;
    private readonly HashSet<SongFile> Loaded = new();

    private readonly WaveFormat StandardFormat = new WaveFormat();
    public PlaylistStream(ISongSource playlist)
    {
        this.Playlist = playlist;
        CurrentIndex = 0;
        playlist.CollectionChanged += Playlist_CollectionChanged;
        AddResamples(playlist.Select(x => x.Song).Where(x => x != null));
    }

    private void SetCurrentTrack()
    {
        while (IsBad(Playlist[current_index]))
        {
            int index = NextGoodIndex();
            if (index == current_index)
            {
                current_index = -1;
                return;
            }
            current_index = index;
            Playlist[current_index].Song.Stream.LoadNow();
        }
        var prev_track = CurrentTrack;
        CurrentTrack = Playlist[current_index].Song;
        if (CurrentTrack != prev_track)
        {
            foreach (var item in Loaded)
            {
                if (item != CurrentTrack)
                    item.CloseStream();
            }
            Loaded.Clear();
        }
        Loaded.Add(CurrentTrack);
        Playlist.GetInOrder(current_index, false);
        int next = UpcomingIndex();
        if (next < Playlist.Count && Playlist[next].Song != null)
        {
            Playlist[next].Song.Stream.LoadBackground();
            Loaded.Add(Playlist[next].Song);
        }
        OnPropertyChanged(nameof(CurrentIndex));
        if (CurrentTrack != prev_track)
            OnPropertyChanged(nameof(CurrentTrack));
        CurrentBase.CurrentTime = TimeSpan.Zero;
    }

    private void AddResamples(IEnumerable<SongFile> songs)
    {
        foreach (var item in songs)
        {
            item.OnStreamLoaded(x => AddResample(x));
        }
    }

    private void AddResample(MutableStream stream)
    {
        if (stream.BaseStream.WaveFormat.SampleRate != StandardFormat.SampleRate)
            stream.AddTransform(x => new WdlResamplingSampleProvider(x, StandardFormat.SampleRate));
        if (stream.BaseStream.WaveFormat.Channels != StandardFormat.Channels)
            stream.AddTransform(x => new MonoToStereoSampleProvider(x));
    }

    private void Playlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        int old_current_index = current_index;
        if (e.Action == NotifyCollectionChangedAction.Add)
            AddResamples(e.NewItems.Cast<SongFile>());
        if (e.Action == NotifyCollectionChangedAction.Move && e.OldStartingIndex == current_index)
        {
            current_index = e.NewStartingIndex;
            Debug.WriteLine($"Move: Current track moved from {e.OldStartingIndex} to {current_index}");
        }
        else if (e.Action == NotifyCollectionChangedAction.Move && e.OldStartingIndex < current_index && e.NewStartingIndex >= current_index)
        {
            current_index -= e.OldItems.Count;
            Debug.WriteLine($"Move: Current index decreased to {current_index}");
        }
        else if (e.Action == NotifyCollectionChangedAction.Move && e.OldStartingIndex > current_index && e.NewStartingIndex <= current_index)
        {
            current_index += e.OldItems.Count;
            Debug.WriteLine($"Move: Current index increased to {current_index}");
        }
        else if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex < current_index)
        {
            current_index += e.NewItems.Count;
            Debug.WriteLine($"Add: Current index moved from {current_index - e.NewItems.Count} to {current_index}");
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldStartingIndex <= current_index)
        {
            if (e.OldStartingIndex + e.OldItems.Count > current_index)
            {
                current_index = e.OldStartingIndex;
                Debug.WriteLine($"Currently playing track removed, current index advanced to {current_index}");
                SetCurrentTrack();
            }
            else
            {
                current_index -= e.OldItems.Count;
                Debug.WriteLine($"Remove: Current index moved from {current_index + e.OldItems.Count} to {current_index}");
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            current_index = Playlist.IndexOf(CurrentTrack);
            Debug.WriteLine($"Reset: Current index relocated to {current_index}");
        }
        if (old_current_index != current_index)
            SetCurrentTrack();
    }

    // we have to use CurrentBase.WaveFormat, not CurrentPlayable.WaveFormat
    // otherwise it calculates length wrong and stuff
    public WaveFormat WaveFormat => CurrentPlayable.WaveFormat;
    public TimeSpan TotalTime => CurrentBase.TotalTime;
    public TimeSpan CurrentTime
    {
        get
        {
            if (!CurrentTrack.Stream.IsLoaded)
            {
                Debug.WriteLine("Requested CurrentTime but current track isn't loaded :(");
                return TimeSpan.Zero;
            }
            return CurrentBase.CurrentTime;
        }
        set
        {
            if (!CurrentTrack.Stream.IsLoaded)
            {
                Debug.WriteLine("Tried to set CurrentTime but current track isn't loaded :(");
                return;
            }
            long position = (long)(value.TotalSeconds * CurrentBase.WaveFormat.AverageBytesPerSecond);
            position = Math.Max(0, position);
            if (position > CurrentBase.Length)
            {
                CurrentIndex = UpcomingIndex(1);
                position = 0;
            }
            CurrentBase.Position = position;
            OnPropertyChanged();
        }
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        var read = 0;
        while (read < count)
        {
            var needed = count - read;
            var readThisTime = CurrentPlayable.Read(buffer, offset + read, needed);
            read += readThisTime;
            if (readThisTime == 0)
            {
                if (RepeatMode == RepeatMode.PlayAll && current_index == Playlist.Count - 1)
                    break;
                CurrentIndex = UpcomingIndex();
            }
        }
        return read;
    }

    public void Next()
    {
        CurrentIndex = NextGoodIndex(1);
    }

    public void Previous()
    {
        CurrentIndex = NextGoodIndex(-1);
    }

    private int WrapIndex(int index)
    {
        int count = Playlist.Count;
        return (index % count + count) % count;
    }

    private int FixIndex(int index)
    {
        if (RepeatMode == RepeatMode.PlayAll)
            return Math.Clamp(index, 0, Playlist.Count - 1);
        return WrapIndex(index);
    }

    private int NextGoodIndex(int direction = 1)
    {
        int index;
        do
        {
            index = WrapIndex(current_index + direction);
            direction += Math.Sign(direction);
        }
        while (index != current_index && IsBad(Playlist[index]));
        return index;
    }

    private bool IsBad(SongReference song)
    {
        if (song.Song == null)
            return true;
        return song.Song.Stream.IsFailed;
    }

    private int UpcomingIndex(int direction = 1)
    {
        if (RepeatMode == RepeatMode.RepeatOne)
            return current_index;
        return NextGoodIndex(direction);
    }

    public void Dispose()
    {
        Debug.WriteLine("Disposing PlaylistStream start");
        foreach (var item in Playlist)
        {
            item.Song?.Dispose();
        }
        Debug.WriteLine("Disposing PlaylistStream complete");
    }
}

public enum RepeatMode
{
    PlayAll,
    RepeatAll,
    RepeatOne
}