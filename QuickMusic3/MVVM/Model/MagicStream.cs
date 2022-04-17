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
                Current.CurrentTime = Current.TotalTime;
            else
                Current.CurrentTime = TimeSpan.Zero;
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
    private WaveStream Current => CurrentTrack.Stream;

    public MagicStream(IEnumerable<string> files)
    {
        this.Sources = files.Select(x => new LoadableStream(x)).ToArray();
    }

    public override WaveFormat WaveFormat => Current.WaveFormat;

    public override long Length => Current.Length;

    public override long Position
    {
        get => Current.Position;
        set { Current.Position = value; Seeked?.Invoke(this, EventArgs.Empty); }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = 0;
        while (read < count)
        {
            var needed = count - read;
            var readThisTime = Current.Read(buffer, offset + read, needed);
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