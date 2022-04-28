using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QuickMusic3.MVVM.Model;

public abstract class Loadable<T>
{
    public event EventHandler Loaded;
    public event EventHandler Failed;
    public LoadStatus LoadStatus { get; private set; } = LoadStatus.NotLoaded;
    public bool IsLoaded => LoadStatus == LoadStatus.Loaded;
    private Task LoadingTask;
    private readonly object LoadingLock = new();
    private T item;
    public T Item
    {
        get
        {
            if (LoadStatus == LoadStatus.Loaded)
                return item;
            else if (LoadStatus == LoadStatus.Failed)
                throw new InvalidOperationException("Loading failed");
            else
                return ItemRequested();
        }
    }

    public void LoadBackground()
    {
        if (LoadStatus != LoadStatus.NotLoaded)
            return;
        LoadStatus = LoadStatus.Loading;
        lock (LoadingLock)
        {
            if (LoadingTask == null || LoadingTask.IsCompleted)
                LoadingTask = Task.Run(TryLoad).ContinueWith(x => SendEvents(), TaskContinuationOptions.ExecuteSynchronously);
        }
    }

    public void LoadNow()
    {
        if (LoadStatus == LoadStatus.Failed || LoadStatus == LoadStatus.Loaded)
            return;
        LoadStatus = LoadStatus.Loading;
        lock (LoadingLock)
        {
            if (LoadingTask != null && !LoadingTask.IsCompleted)
                LoadingTask.Wait();
            else
            {
                TryLoad();
                SendEvents();
            }
        }
    }

    private void TryLoad()
    {
        try
        {
            item = Load();
            LoadStatus = LoadStatus.Loaded;
        }
        catch
        {
            LoadStatus = LoadStatus.Failed;
        }
    }

    private void SendEvents()
    {
        if (LoadStatus == LoadStatus.Loaded)
            Loaded?.Invoke(this, EventArgs.Empty);
        else if (LoadStatus == LoadStatus.Failed)
            Failed?.Invoke(this, EventArgs.Empty);
        Loaded = null;
        Failed = null;
        AfterLoad();
    }

    protected abstract T Load();
    protected virtual void AfterLoad() { }
    protected abstract T GetRequested();
}

public enum LoadStatus
{
    NotLoaded,
    Loading,
    Loaded,
    Failed
}

public class NeedyLoadable<T> : Loadable<T>
{
    private readonly Func<T> Getter;
    public NeedyLoadable(Func<T> getter)
    {
        Getter = getter;
    }

    protected override T GetRequested()
    {
        LoadNow();
        return Item;
    }

    protected override T Load()
    {
        return Getter();
    }
}

public class FakerLoadable<T> : Loadable<T>
{
    private readonly Func<T> RealGetter;
    private readonly Func<T> FakeGetter;
    public FakerLoadable(Func<T> real, Func<T> fake)
    {
        RealGetter = real;
        FakeGetter = fake;
    }

    protected override T GetRequested()
    {
        LoadBackground();
        return FakeGetter();
    }

    protected override T Load()
    {
        return RealGetter();
    }
}
