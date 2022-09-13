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
    public Exception Exception { get; private set; }
    public bool IsLoaded => ItemReady();
    public bool IsFailed => LoadStatus == LoadStatus.Failed;
    private Task LoadingTask;
    private readonly object LoadingLock = new();
    protected T _item;
    public T Item
    {
        get
        {
            if (ItemReady())
                return _item;
            else if (HasUnloaded())
                return ItemReload();
            else if (LoadStatus == LoadStatus.Failed)
                return ItemFailed();
            else
                return ItemRequested();
        }
    }

    public void LoadBackground()
    {
        lock (LoadingLock)
        {
            if (ItemReady())
                return;
            LoadStatus = LoadStatus.Loading;
            if (LoadingTask == null || LoadingTask.IsCompleted)
                LoadingTask = Task.Run(TryLoad).ContinueWith(x => SendEvents(), TaskContinuationOptions.ExecuteSynchronously);
        }
    }

    public void LoadNow()
    {
        lock (LoadingLock)
        {
            if (ItemReady())
                return;
            LoadStatus = LoadStatus.Loading;
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
            _item = Load();
            LoadStatus = LoadStatus.Loaded;
        }
        catch (Exception ex)
        {
            LoadStatus = LoadStatus.Failed;
            Exception = ex;
            Debug.WriteLine($"Failed to load: {ex}");
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

    private bool ItemReady()
    {
        return LoadStatus == LoadStatus.Loaded && !ItemUnloaded(_item);
    }

    private bool HasUnloaded()
    {
        return LoadStatus == LoadStatus.Loaded && ItemUnloaded(_item);
    }

    protected abstract T Load();
    protected virtual void AfterLoad() { }
    protected abstract T ItemRequested();
    protected virtual T ItemFailed()
    {
        throw new InvalidOperationException("Loading failed", Exception);
    }
    protected abstract bool ItemUnloaded(T item);
    protected virtual T ItemReload() => ItemRequested();
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
    private readonly Func<T, bool> UnloadCheck;
    public NeedyLoadable(Func<T> getter, Func<T, bool> unload_check)
    {
        Getter = getter;
        UnloadCheck = unload_check;
    }

    protected override T ItemRequested()
    {
        LoadNow();
        return Item;
    }

    protected override T Load()
    {
        return Getter();
    }

    protected override bool ItemUnloaded(T item) => UnloadCheck(item);
}

public class PlaceholderLoadable<T> : Loadable<T>
{
    private readonly Func<T> RealGetter;
    private readonly T Placeholder;
    public PlaceholderLoadable(Func<T> real, T placeholder)
    {
        RealGetter = real;
        Placeholder = placeholder;
    }

    protected override T ItemRequested()
    {
        LoadBackground();
        return Placeholder;
    }

    protected override T ItemFailed()
    {
        return Placeholder;
    }

    protected override T Load()
    {
        return RealGetter();
    }

    protected override bool ItemUnloaded(T item)
    {
        return false;
    }
}
