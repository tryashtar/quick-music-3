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
            current_index = FixIndex(value);
            Sources[current_index].Stream.CurrentTime = TimeSpan.Zero;
            int next = FixIndex(NextIndex());
            for (int i = 0; i < Sources.Length; i++)
            {
                if (i == next)
                    Sources[i].LoadBackground();
                else if (i != current_index)
                    Sources[i].Close();
            }
            CurrentChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public RepeatMode RepeatMode { get; set; }
    public int SourceCount => Sources.Length;
    private WaveStream Current => Sources[Math.Clamp(current_index, 0, Sources.Length - 1)].Stream;

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
        while (read < count && current_index < Sources.Length)
        {
            var needed = count - read;
            var readThisTime = Sources[current_index].Stream.Read(buffer, offset + read, needed);
            read += readThisTime;
            if (readThisTime == 0)
                CurrentIndex = NextIndex();
        }
        return read;
    }

    private int FixIndex(int index)
    {
        if (RepeatMode == RepeatMode.RepeatAll)
            return index % Sources.Length;
        return Math.Clamp(index, 0, Sources.Length - 1);
    }

    private int NextIndex()
    {
        if (RepeatMode == RepeatMode.RepeatOne)
            return current_index;
        return current_index + 1;
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