using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class LoadableStream : IDisposable
{
    public readonly string Path;
    public bool IsStreamLoaded => base_stream != null;
    public bool IsMetadataLoaded => metadata != null;
    private Task StreamLoadingTask;
    private WaveStream? base_stream;
    private IWaveProvider playable_stream;
    private Metadata metadata;
    private readonly WaveFormat? RequiredFormat;
    private object loading_lock = new();
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
    public Metadata Metadata
    {
        get
        {
            if (metadata == null)
                LoadMetadataNow();
            return metadata;
        }
    }
    public LoadableStream(string path, WaveFormat? format = null)
    {
        Path = path;
        RequiredFormat = format;
    }

    private void LoadStream()
    {
        base_stream = new AudioFileReader(Path);
        playable_stream = base_stream;
        if (RequiredFormat != null)
        {
            if (base_stream.WaveFormat.SampleRate != RequiredFormat.SampleRate || base_stream.WaveFormat.Channels != RequiredFormat.Channels)
            {
                ISampleProvider sample = playable_stream.ToSampleProvider();
                if (base_stream.WaveFormat.SampleRate != RequiredFormat.SampleRate)
                    sample = new WdlResamplingSampleProvider(sample, RequiredFormat.SampleRate);
                if (base_stream.WaveFormat.Channels != RequiredFormat.Channels)
                    sample = new MonoToStereoSampleProvider(sample);
                playable_stream = sample.ToWaveProvider();
            }
        }
        ApplyReplayGain();
        System.Diagnostics.Debug.WriteLine($"{System.IO.Path.GetFileName(Path)}: {base_stream.WaveFormat.SampleRate} / {base_stream.WaveFormat.Channels}");
    }

    private void LoadMetadata()
    {
        metadata = new Metadata(Path);
        ApplyReplayGain();
    }

    private void ApplyReplayGain()
    {
        if (base_stream != null && metadata != null && metadata.ReplayGain != 0)
            playable_stream = new DecibalOffsetProvider(playable_stream.ToSampleProvider(), metadata.ReplayGain).ToWaveProvider();
    }

    public void LoadStreamBackground()
    {
        lock (loading_lock)
        {
            if (base_stream == null && (StreamLoadingTask == null || StreamLoadingTask.IsCompleted))
                StreamLoadingTask = Task.Run(LoadStream);
        }
    }

    public void LoadStreamNow()
    {
        lock (loading_lock)
        {
            if (StreamLoadingTask != null && !StreamLoadingTask.IsCompleted)
                StreamLoadingTask.Wait();
            else if (base_stream == null)
                LoadStream();
        }
    }

    public void LoadMetadataNow()
    {
        if (metadata == null)
            LoadMetadata();
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
