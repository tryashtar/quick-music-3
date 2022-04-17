using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace QuickMusic3.MVVM.Model;

public class MagicStream : WaveStream
{
    public event EventHandler CurrentChanged;
    public event EventHandler Seeked;

    private readonly LoadableStream[] Sources;
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
            int? next = UpcomingIndex();
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

    public MagicStream(IEnumerable<string> files)
    {
        this.Sources = files.Select(x => new LoadableStream(x)).ToArray();
        CurrentIndex = 0;
    }

    public override WaveFormat WaveFormat => CurrentPlayable.WaveFormat;

    public override long Length => CurrentBase.Length;

    public override long Position
    {
        get => CurrentBase.Position;
        set { CurrentBase.Position = value; Seeked?.Invoke(this, EventArgs.Empty); }
    }

    public override int Read(byte[] buffer, int offset, int count)
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

    private int UpcomingIndex()
    {
        if (RepeatMode == RepeatMode.RepeatOne)
            return current_index;
        return WrapIndex(current_index + 1);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            foreach (var item in Sources)
            {
                item.Dispose();
            }
        }
    }
}

public enum RepeatMode
{
    PlayAll,
    RepeatAll,
    RepeatOne
}