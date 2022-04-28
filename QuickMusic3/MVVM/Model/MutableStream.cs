using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class MutableStream : IDisposable
{
    public readonly string Path;
    public WaveStream BaseStream { get; private set; }
    public IWaveProvider PlayableStream { get; private set; }

    public MutableStream(string path)
    {
        BaseStream = new AudioFileReader(path);
        PlayableStream = BaseStream;
    }

    public void Close()
    {
        if (BaseStream != null)
        {
            BaseStream.Dispose();
            BaseStream = null;
        }
        if (PlayableStream != null)
        {
            if (PlayableStream is IDisposable d)
                d.Dispose();
            PlayableStream = null;
        }
    }

    public void Dispose()
    {
        Close();
    }
}
