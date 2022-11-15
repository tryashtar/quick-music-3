using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public sealed class MutableStream : IDisposable
{
    public readonly string FilePath;
    public bool IsDisposed { get; private set; } = false;
    public WaveStream BaseStream { get; private set; }
    public IWaveProvider PlayableStream { get; private set; }
    public delegate ISampleProvider Transform(ISampleProvider provider);
    private readonly List<Transform> Transforms = new();

    public MutableStream(string path)
    {
        FilePath = path;
        BaseStream = new AudioFileReader(path, BaseReaders.Auto);
        PlayableStream = BaseStream;
    }

    public void AddTransform(Transform transform)
    {
        Transforms.Add(transform);
        var sample = BaseStream.ToSampleProvider();
        foreach (var next in Transforms)
        {
            sample = next(sample);
        }
        PlayableStream = sample.ToWaveProvider();
    }

    public void Dispose()
    {
        IsDisposed = true;
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
}
