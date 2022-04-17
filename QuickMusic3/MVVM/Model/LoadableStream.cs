using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class LoadableStream : IDisposable
{
    public readonly string Path;
    public bool IsStreamLoaded => stream != null;
    public bool IsMetadataLoaded => metadata != null;
    private Task StreamLoadingTask;
    private WaveStream? stream;
    private Metadata metadata;
    public WaveStream Stream
    {
        get
        {
            if (stream == null)
                LoadStreamNow();
            return stream;
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
    public LoadableStream(string path)
    {
        Path = path;
    }

    private void LoadStream()
    {
        stream = new AudioFileReader(Path);
    }

    private void LoadMetadata()
    {
        metadata = new Metadata(Path);
    }

    public void LoadStreamBackground()
    {
        if (stream == null && (StreamLoadingTask == null || StreamLoadingTask.IsCompleted))
            StreamLoadingTask = Task.Run(LoadStream);
    }

    public void LoadStreamNow()
    {
        if (StreamLoadingTask != null && !StreamLoadingTask.IsCompleted)
            StreamLoadingTask.Wait();
        else if (stream == null)
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
        if (stream != null)
        {
            stream.Dispose();
            stream = null;
        }
    }

    public void Dispose()
    {
        Close();
    }
}
