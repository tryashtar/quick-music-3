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
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace QuickMusic3.MVVM.Model;

public sealed class PlaylistStream : ObservableObject, IWaveProvider, IDisposable
{
    public readonly ISongSource Playlist;
    public int CurrentIndex { get; private set; } = -1;
    public SongFile? CurrentTrack { get; private set; }
    public RepeatMode RepeatMode { get; set; }

    private readonly WaveFormat StandardFormat = new WaveFormat();
    private readonly HashSet<MutableStream> Loaded = new();

    public WaveFormat WaveFormat => CurrentStream?.PlayableStream?.WaveFormat ?? StandardFormat;

    public MutableStream? CurrentStream
    {
        get
        {
            if (CurrentTrack == null)
                return null;
            if (!CurrentTrack.Stream.IsSuccessfullyCompleted)
                return null;
            return CurrentTrack.Stream.Item;
        }
    }

    public PlaylistStream(ISongSource playlist)
    {
        this.Playlist = playlist;
        playlist.CollectionChanged += Playlist_CollectionChanged;
    }

    private async Task<MutableStream> LoadStreamAsync(SongFile song)
    {
        var stream = await song.Stream;
        if (!Loaded.Contains(stream))
        {
            Loaded.Add(stream);
            if (stream.BaseStream.WaveFormat.SampleRate != StandardFormat.SampleRate)
                stream.AddTransform(x => new WdlResamplingSampleProvider(x, StandardFormat.SampleRate));
            if (stream.BaseStream.WaveFormat.Channels != StandardFormat.Channels)
                stream.AddTransform(x => new MonoToStereoSampleProvider(x));
        }
        return stream;
    }

    public async Task SetIndexAsync(int index, int direction)
    {
        foreach (var item in Loaded)
        {
            item.Dispose();
        }
        Loaded.Clear();
        (index, SongFile? song) = await FindGoodSongAsync(index, direction);
        if (song != null)
        {
            _ = Playlist.GetInOrderAsync(index);
            var stream = await LoadStreamAsync(song);
            stream.BaseStream.Position = 0;
        }
        CurrentIndex = index;
        CurrentTrack = song;
        OnPropertyChanged(nameof(CurrentTrack));
        OnPropertyChanged(nameof(CurrentIndex));
        _ = FindGoodSongAsync(UpcomingIndex(), 1);
    }

    private async Task<(int index, SongFile? song)> FindGoodSongAsync(int start, int direction)
    {
        int index = FixIndex(start);
        SongFile? song;
        while (true)
        {
            try
            {
                await Playlist[index].Stream;
                song = Playlist[index];
            }
            catch
            {
                index = FixIndex(index + direction);
                if (index == start)
                    return (-1, null);
                continue;
            }
            break;
        }
        return (index, song);
    }

    private void Playlist_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        int previous_index = CurrentIndex;
        var previous_track = CurrentTrack;
        if (e.Action == NotifyCollectionChangedAction.Move && e.OldStartingIndex == CurrentIndex)
        {
            CurrentIndex = e.NewStartingIndex;
            Debug.WriteLine($"Move: Current track moved from {e.OldStartingIndex} to {CurrentIndex}");
        }
        else if (e.Action == NotifyCollectionChangedAction.Move && e.OldStartingIndex < CurrentIndex && e.NewStartingIndex >= CurrentIndex)
        {
            CurrentIndex -= e.OldItems.Count;
            Debug.WriteLine($"Move: Current index decreased to {CurrentIndex}");
        }
        else if (e.Action == NotifyCollectionChangedAction.Move && e.OldStartingIndex > CurrentIndex && e.NewStartingIndex <= CurrentIndex)
        {
            CurrentIndex += e.OldItems.Count;
            Debug.WriteLine($"Move: Current index increased to {CurrentIndex}");
        }
        else if (e.Action == NotifyCollectionChangedAction.Add && e.NewStartingIndex < CurrentIndex)
        {
            CurrentIndex += e.NewItems.Count;
            Debug.WriteLine($"Add: Current index moved from {CurrentIndex - e.NewItems.Count} to {CurrentIndex}");
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldStartingIndex <= CurrentIndex)
        {
            if (e.OldStartingIndex + e.OldItems.Count > CurrentIndex)
            {
                CurrentIndex = e.OldStartingIndex;
                Debug.WriteLine($"Currently playing track removed, current index advanced to {CurrentIndex}");
                _ = SetIndexAsync(CurrentIndex, 1);
            }
            else
            {
                CurrentIndex -= e.OldItems.Count;
                Debug.WriteLine($"Remove: Current index moved from {CurrentIndex + e.OldItems.Count} to {CurrentIndex}");
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            CurrentIndex = Playlist.IndexOf(CurrentTrack);
            Debug.WriteLine($"Reset: Current index relocated to {CurrentIndex}");
        }
        if (CurrentIndex != previous_index)
            OnPropertyChanged(nameof(CurrentIndex));
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        var read = 0;
        while (read < count)
        {
            var needed = count - read;
            var playable = CurrentStream?.PlayableStream;
            if (playable == null)
                return read;
            var readThisTime = playable.Read(buffer, offset + read, needed);
            read += readThisTime;
            if (readThisTime == 0)
            {
                if (RepeatMode == RepeatMode.PlayAll && CurrentIndex == Playlist.Count - 1)
                    break;
                SetIndexAsync(UpcomingIndex(), 1).Wait();
            }
        }
        return read;
    }

    private int UpcomingIndex()
    {
        if (RepeatMode == RepeatMode.RepeatOne)
            return CurrentIndex;
        return CurrentIndex + 1;
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

    public void Dispose()
    {

    }
}

public enum RepeatMode
{
    PlayAll,
    RepeatAll,
    RepeatOne
}
