using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace QuickMusic3.MVVM.Model;

public class MagicStream : IWaveProvider, IDisposable
{
    public event EventHandler CurrentChanged;
    public event EventHandler Seeked;

    public readonly LoadableStream[] Sources;
    private int current_index;
    public int CurrentIndex
    {
        get => current_index;
        set
        {
            int destination = WrapIndex(value);
            current_index = Math.Clamp(destination, 0, Sources.Length - 1);
            if (destination >= Sources.Length)
                CurrentBase.CurrentTime = CurrentBase.TotalTime;
            else
                CurrentBase.CurrentTime = TimeSpan.Zero;
            int next = UpcomingIndex();
            for (int i = 0; i < Sources.Length; i++)
            {
                if (i == next)
                    Sources[i].LoadStreamBackground();
                else if (i != current_index)
                    Sources[i].Close();
            }
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public RepeatMode RepeatMode { get; set; }
    public int SourceCount => Sources.Length;
    public LoadableStream CurrentTrack => Sources[current_index];
    private WaveStream CurrentBase => CurrentTrack.BaseStream;
    private IWaveProvider CurrentPlayable => CurrentTrack.PlayableStream;

    private readonly WaveFormat StandardFormat = new WaveFormat();
    public MagicStream(IEnumerable<string> files)
    {
        this.Sources = files.Select(x => new LoadableStream(x, StandardFormat)).ToArray();
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
                if (next < 0 || next >= Sources.Length)
                    break;
                else
                    CurrentIndex = next;
            }
        }
        return read;
    }

    private int WrapIndex(int index)
    {
        if (RepeatMode == RepeatMode.RepeatAll || RepeatMode == RepeatMode.RepeatOne)
            return (index % Sources.Length + Sources.Length) % Sources.Length;
        return Math.Clamp(index, -1, Sources.Length);
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
        foreach (var item in Sources)
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