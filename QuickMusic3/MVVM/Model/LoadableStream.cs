using NAudio.Wave;
using System;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class LoadableStream : IDisposable
{
    public readonly string Path;
    private Task LoadingTask;
    private WaveStream? stream;
    public WaveStream Stream
    {
        get
        {
            if (stream == null)
                LoadNow();
            return stream;
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

    public void LoadBackground()
    {
        if (stream == null && LoadingTask != null || LoadingTask.IsCompleted)
            LoadingTask = Task.Run(LoadStream);
    }

    public void LoadNow()
    {
        if (LoadingTask != null && !LoadingTask.IsCompleted)
            LoadingTask.Wait();
        else if (stream == null)
            LoadStream();
    }

    public void Close()
    {
        if (LoadingTask != null && !LoadingTask.IsCompleted)
            LoadingTask.Wait();
        if (stream != null)
        {
            stream.Dispose();
            stream = null;
        }
        LoadingTask = null;
    }

    public void Dispose()
    {
        Close();
    }
}
