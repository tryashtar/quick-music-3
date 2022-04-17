using NAudio.Wave;
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
    public LoadableStream(string path, WaveFormat? format=null)
    {
        Path = path;
        RequiredFormat = format;
    }

    private void LoadStream()
    {
        base_stream = new AudioFileReader(Path);
        playable_stream = base_stream;
        if (RequiredFormat != null && base_stream.WaveFormat.SampleRate != RequiredFormat.SampleRate)
            playable_stream = new ResamplerDmoStream(playable_stream, RequiredFormat);
        System.Diagnostics.Debug.WriteLine($"{System.IO.Path.GetFileName(Path)}: {base_stream.WaveFormat.SampleRate}");
        ApplyReplayGain();
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
        if (base_stream == null && (StreamLoadingTask == null || StreamLoadingTask.IsCompleted))
            StreamLoadingTask = Task.Run(LoadStream);
    }

    public void LoadStreamNow()
    {
        if (StreamLoadingTask != null && !StreamLoadingTask.IsCompleted)
            StreamLoadingTask.Wait();
        else if (base_stream == null)
            LoadStream();
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
        playable_stream = null;
    }

    public void Dispose()
    {
        Close();
    }
}
