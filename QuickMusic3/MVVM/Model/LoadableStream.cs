using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class LoadableStream : IDisposable
{
    public readonly string Path;
    public bool IsStreamLoaded => base_stream != null;
    private Task StreamLoadingTask;
    private WaveStream? base_stream;
    private IWaveProvider playable_stream;
    public Metadata Metadata { get; }
    private readonly WaveFormat? RequiredFormat;
    private readonly object loading_lock = new();
    public event EventHandler StreamLoaded;
    public event EventHandler StreamFailed;
    public TimeSpan GuessDuration
    {
        get
        {
            if (IsStreamLoaded)
                return BaseStream.TotalTime;
            return Metadata.Duration;
        }
    }
    public WaveStream BaseStream
    {
        get
        {
            if (base_stream == null)
                LoadStreamNow();
            return base_stream;
        }
    }
    public IWaveProvider PlayableStream
    {
        get
        {
            if (base_stream == null)
                LoadStreamNow();
            return playable_stream;
        }
    }

    public LoadableStream(string path, WaveFormat? format = null)
    {
        Path = path;
        Metadata = new(Path);
        RequiredFormat = format;
    }

    private void LoadStream()
    {
        try
        {
            base_stream = new AudioFileReader(Path);
            playable_stream = base_stream;
            List<Func<ISampleProvider, ISampleProvider>> sample_transforms = new();
            if (RequiredFormat != null)
            {
                if (base_stream.WaveFormat.SampleRate != RequiredFormat.SampleRate)
                    sample_transforms.Add(x => new WdlResamplingSampleProvider(x, RequiredFormat.SampleRate));
                if (base_stream.WaveFormat.Channels != RequiredFormat.Channels)
                    sample_transforms.Add(x => new MonoToStereoSampleProvider(x));
            }
            Metadata.LoadNow();
            if (Metadata.ReplayGain != 0)
                sample_transforms.Add(x => new DecibalOffsetProvider(x, Metadata.ReplayGain));
            if (sample_transforms.Count > 0)
            {
                var sample = playable_stream.ToSampleProvider();
                foreach (var transform in sample_transforms)
                {
                    sample = transform(sample);
                }
                playable_stream = sample.ToWaveProvider();
            }
        }
        catch { }
    }

    public void LoadStreamBackground()
    {
        lock (loading_lock)
        {
            if (base_stream == null && (StreamLoadingTask == null || StreamLoadingTask.IsCompleted))
                StreamLoadingTask = Task.Run(LoadStream).ContinueWith(x => LoadDone(), TaskContinuationOptions.ExecuteSynchronously);
        }
    }

    private void LoadDone()
    {
        if (IsStreamLoaded)
        {
            StreamLoaded?.Invoke(this, EventArgs.Empty);
            Debug.WriteLine($"{System.IO.Path.GetFileName(Path)}: {base_stream.WaveFormat.SampleRate} / {base_stream.WaveFormat.Channels}");
        }
        else
        {
            StreamFailed?.Invoke(this, EventArgs.Empty);
            Debug.WriteLine($"{System.IO.Path.GetFileName(Path)}: Failed to load stream");
        }
    }

    public void LoadStreamNow()
    {
        lock (loading_lock)
        {
            if (StreamLoadingTask != null && !StreamLoadingTask.IsCompleted)
                StreamLoadingTask.Wait();
            else if (base_stream == null)
            {
                LoadStream();
                LoadDone();
            }
        }
    }

    public void Close()
    {
        if (StreamLoadingTask != null && !StreamLoadingTask.IsCompleted)
            StreamLoadingTask.Wait();
        StreamLoadingTask = null;
        if (base_stream != null)
        {
            base_stream.Dispose();
            base_stream = null;
        }
        if (playable_stream != null)
        {
            if (playable_stream is IDisposable d)
                d.Dispose();
            playable_stream = null;
        }
    }

    public void Dispose()
    {
        Close();
    }
}
