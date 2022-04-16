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

    private readonly LoadableStream[] Sources;
    private int CurrentIndex;
    private WaveStream Current => CurrentIndex >= Sources.Length ? Sources[0].Stream : Sources[CurrentIndex].Stream;

    public MagicStream(IEnumerable<string> files)
    {
        this.Sources = files.Select(x => new LoadableStream(x)).ToArray();
    }

    public override WaveFormat WaveFormat => Sources[0].Stream.WaveFormat;

    public override long Length => Current.Length;

    public override long Position
    {
        get => Current.Position;
        set => Current.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = 0;
        while (read < count && CurrentIndex < Sources.Length)
        {
            var needed = count - read;
            var readThisTime = Sources[CurrentIndex].Stream.Read(buffer, offset + read, needed);
            read += readThisTime;
            if (readThisTime == 0)
            {
                CurrentIndex++;
                CurrentChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        return read;
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
