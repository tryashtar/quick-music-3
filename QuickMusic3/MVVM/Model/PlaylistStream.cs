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
            int count = Playlist.Count;
            int destination = WrapIndex(value);
            current_index = Math.Clamp(destination, 0, count - 1);
            SetCurrentTrack();
            if (destination >= count)
                CurrentBase.CurrentTime = CurrentBase.TotalTime;
            else
                CurrentBase.CurrentTime = TimeSpan.Zero;
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
        AddResamples(playlist);
    }

    private void SetCurrentTrack()
    {
        var prev_track = CurrentTrack;
        do
        {
            CurrentTrack = Playlist[current_index];
            CurrentTrack.Stream.LoadNow();
            if (CurrentTrack.Stream.IsFailed)
                current_index++;
        }
        while (CurrentTrack.Stream.IsFailed);
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
        if (next < Playlist.Count)
        {
            Playlist[next].Stream.LoadBackground();
            Loaded.Add(Playlist[next]);
        }
        OnPropertyChanged(nameof(CurrentIndex));
        if (CurrentTrack != prev_track)
            OnPropertyChanged(nameof(CurrentTrack));
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
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldStartingIndex < current_index)
        {
            current_index -= e.OldItems.Count;
            Debug.WriteLine($"Remove: Current index moved from {current_index + e.OldItems.Count} to {current_index}");
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldStartingIndex == current_index)
        {
            CurrentIndex += e.OldItems.Count;
            Debug.WriteLine($"Currently playing track removed, current index advanced to {current_index}");
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
                int next = UpcomingIndex();
                if (next < 0 || next >= Playlist.Count)
                    break;
                else
                    CurrentIndex = next;
            }
        }
        return read;
    }

    private int WrapIndex(int index)
    {
        int count = Playlist.Count;
        if (RepeatMode == RepeatMode.RepeatAll || RepeatMode == RepeatMode.RepeatOne)
            return (index % count + count) % count;
        return Math.Clamp(index, -1, count);
    }

    private int UpcomingIndex(int direction = 1)
    {
        if (RepeatMode == RepeatMode.RepeatOne)
            return current_index;
        return WrapIndex(current_index + direction);
    }

    public void Dispose()
    {
        Debug.WriteLine("Disposing PlaylistStream start");
        foreach (var item in Playlist)
        {
            item.Dispose();
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