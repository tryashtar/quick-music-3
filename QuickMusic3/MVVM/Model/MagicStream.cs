using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace QuickMusic3.MVVM.Model;

public class MagicStream : IWaveProvider, IDisposable
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
                    Playlist[i].LoadStreamBackground();
                else if (i != current_index)
                    Playlist[i].Close();
            }
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public RepeatMode RepeatMode { get; set; }
    public LoadableStream CurrentTrack => Playlist[current_index];
    private WaveStream CurrentBase => CurrentTrack.BaseStream;
    private IWaveProvider CurrentPlayable => CurrentTrack.PlayableStream;

    private readonly WaveFormat StandardFormat = new WaveFormat();
    public MagicStream(Playlist playlist)
    {
        this.Playlist = playlist;
        CurrentIndex = 0;
    }

    public WaveFormat WaveFormat => CurrentPlayable.WaveFormat;

    // we have to use CurrentBase.WaveFormat, not CurrentPlayable.WaveFormat
    // I promise
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
        System.Diagnostics.Debug.WriteLine("Disposing MagicStream start");
        foreach (var item in Playlist)
        {
            item.Dispose();
        }
        System.Diagnostics.Debug.WriteLine("Disposing MagicStream complete");
    }
}

public enum RepeatMode
{
    PlayAll,
    RepeatAll,
    RepeatOne
}