using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QuickMusic3.MVVM.Model;

public class PlaylistStream : IWaveProvider, IDisposable
{
    public event EventHandler CurrentChanged;
    public event EventHandler Seeked;

    public readonly Playlist Playlist;
    private int current_index;
    public int CurrentIndex
    {
        get => current_index;
        set
        {
            int count = Playlist.Count;
            int destination = WrapIndex(value);
            current_index = Math.Clamp(destination, 0, count - 1);
            if (destination >= count)
                CurrentBase.CurrentTime = CurrentBase.TotalTime;
            else
                CurrentBase.CurrentTime = TimeSpan.Zero;
            int next = UpcomingIndex();
            for (int i = 0; i < count; i++)
            {
                if (i == next)
                    Playlist[i].Stream.LoadBackground();
                else if (i != current_index && Playlist[i].Stream.IsLoaded)
                    Playlist[i].Stream.Item.Close();
            }
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public RepeatMode RepeatMode { get; set; }
    public SongFile CurrentTrack => Playlist[current_index];
    private MutableStream CurrentStream
    {
        get
        {
            // if LoadNow errors, it will be removed from the playlist,
            // calling Playlist_CollectionChanged which will change current_index,
            // ultimately changing CurrentTrack so we try again
            while (true)
            {
                var stream = CurrentTrack.Stream;
                stream.LoadNow();
                if (stream.IsLoaded)
                    return stream.Item;
            }
        }
    }
    private WaveStream CurrentBase => CurrentStream.BaseStream;
    private IWaveProvider CurrentPlayable => CurrentStream.PlayableStream;

    private readonly WaveFormat StandardFormat = new WaveFormat();
    public PlaylistStream(Playlist playlist)
    {
        this.Playlist = playlist;
        CurrentIndex = 0;
        playlist.CollectionChanged += Playlist_CollectionChanged;
    }

    private void Playlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Move && e.OldStartingIndex == current_index)
            current_index = e.NewStartingIndex;
        else if (e.Action == NotifyCollectionChangedAction.Add && e.OldStartingIndex < current_index)
            current_index += e.NewItems.Count;
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldStartingIndex < current_index)
            current_index -= e.OldItems.Count;
        else if (e.Action == NotifyCollectionChangedAction.Remove && e.OldStartingIndex == current_index)
            current_index++;
    }

    // we have to use CurrentBase.WaveFormat, not CurrentPlayable.WaveFormat
    // otherwise it calculates length wrong and stuff
    public WaveFormat WaveFormat => CurrentPlayable.WaveFormat;
    public TimeSpan TotalTime => CurrentBase.TotalTime;
    public TimeSpan CurrentTime
    {
        get => CurrentBase.CurrentTime;
        set
        {
            long position = (long)(value.TotalSeconds * CurrentBase.WaveFormat.AverageBytesPerSecond);
            if (position < 0)
            {
                if (CurrentTime < TimeSpan.FromSeconds(1))
                {
                    CurrentIndex = UpcomingIndex(-1);
                    position = CurrentBase.Length;
                }
                else
                    position = 0;
            }
            else if (position > CurrentBase.Length)
            {
                CurrentIndex = UpcomingIndex(1);
                position = 0;
            }
            CurrentBase.Position = position;
            Seeked?.Invoke(this, EventArgs.Empty);
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