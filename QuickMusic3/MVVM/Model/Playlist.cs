using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace QuickMusic3.MVVM.Model;

public class Playlist : ISongSource
{
    private readonly List<SongFile> FlatList = new();
    private readonly Dictionary<ISongSource, int> SourcePositions = new();

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public SongFile this[int index] => FlatList[index];
    public int Count => FlatList.Count;
    public IEnumerator<SongFile> GetEnumerator() => FlatList.GetEnumerator();

    private readonly Dispatcher Dispatcher;
    public Playlist(Dispatcher event_dispatcher)
    {
        Dispatcher = event_dispatcher;
    }

    public void AddSource(ISongSource source)
    {
        int index;
        lock (FlatList)
        {
            index = FlatList.Count;
            SourcePositions[source] = index;
            FlatList.AddRange(source);
        }
        Dispatcher.Invoke(() => CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, source.ToList(), index)));

        source.CollectionChanged += (s, e) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                lock (FlatList)
                {
                    int old_index = SourcePositions[source];
                    if (e.Action == NotifyCollectionChangedAction.Remove)
                    {
                        var items = FlatList.GetRange(old_index + e.OldStartingIndex, e.OldItems.Count);
                        FlatList.RemoveRange(old_index + e.OldStartingIndex, e.OldItems.Count);
                        foreach (var item in SourcePositions.ToList())
                        {
                            if (item.Value > old_index)
                                SourcePositions[item.Key] = item.Value - e.OldItems.Count;
                        }
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, old_index + e.OldStartingIndex));
                        //Debug.WriteLine("R " + String.Join(' ', FlatList.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : "-")));
                    }
                    else if (e.Action == NotifyCollectionChangedAction.Move)
                    {
                        var item = FlatList[old_index + e.OldStartingIndex];
                        int destination = old_index + e.NewStartingIndex;
                        FlatList.RemoveAt(old_index + e.OldStartingIndex);
                        FlatList.Insert(destination, item);
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, destination, old_index + e.OldStartingIndex));
                        //Debug.WriteLine("M " + String.Join(' ', FlatList.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : "-")));
                    }
                    else
                        throw new NotSupportedException();
                }
            });
        };
    }
}

public interface ISongSource : IReadOnlyList<SongFile>, INotifyCollectionChanged
{
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class FolderSource : ISongSource
{
    private readonly List<SongFile> Streams;
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public int Count => Streams.Count;
    public SongFile this[int index] => Streams[index];
    public IEnumerator<SongFile> GetEnumerator() => Streams.GetEnumerator();

    public FolderSource(string path, SearchOption search, string first = null)
    {
        var directory = new DirectoryInfo(path);
        Streams = directory.GetFiles("*", search)
            .Where(x => !x.Attributes.HasFlag(FileAttributes.Hidden) && !x.Attributes.HasFlag(FileAttributes.System))
            .Select(x => new SongFile(x.FullName)).ToList();
        if (first != null)
        {
            var index = Streams.FindIndex(x => x.FilePath == first);
            if (index != -1)
            {
                var item = Streams[index];
                Streams.RemoveAt(index);
                Streams.Insert(0, item);
            }
        }
        foreach (var item in Streams)
        {
            item.Metadata.Failed += (s, e) => MoveIntoPlace(item);
            item.Metadata.Loaded += (s, e) => MoveIntoPlace(item);
            item.Stream.Failed += (s, e) => Remove(item);
        }
    }

    private void MoveIntoPlace(SongFile item)
    {
        int old_index, destination;
        lock (Streams)
        {
            old_index = Streams.IndexOf(item);
            Streams.RemoveAt(old_index);
            destination = ~Streams.BinarySearch(item, SongSorter.Instance);
            Streams.Insert(destination, item);
            //Debug.WriteLine("M " + String.Join(' ', Streams.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : ".")));
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, destination, old_index));
    }

    private void Remove(SongFile item)
    {
        int old_index;
        lock (Streams)
        {
            old_index = Streams.IndexOf(item);
            Streams.RemoveAt(old_index);
            //Debug.WriteLine("R " + String.Join(' ', Streams.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : ".")));
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, old_index));
    }
}

public class SongSorter : IComparer<SongFile>
{
    public static readonly SongSorter Instance = new();
    public int Compare(SongFile x, SongFile y)
    {
        int status = LoadStatusOrder(x.Metadata.LoadStatus).CompareTo(LoadStatusOrder(y.Metadata.LoadStatus));
        if (status != 0)
            return status;
        if (x.Metadata.LoadStatus != LoadStatus.Loaded)
            return x.FilePath.CompareTo(y.FilePath);
        var xm = x.Metadata.Item;
        var ym = y.Metadata.Item;
        int album = (xm.Album ?? "").CompareTo(ym.Album ?? "");
        if (album != 0)
            return album;
        int disc = xm.DiscNumber.CompareTo(ym.DiscNumber);
        if (disc != 0)
            return disc;
        int track = xm.TrackNumber.CompareTo(ym.TrackNumber);
        if (track != 0)
            return track;
        return (xm.Title ?? "").CompareTo(ym.Title ?? "");
    }

    private static int LoadStatusOrder(LoadStatus status)
    {
        return status switch
        {
            LoadStatus.Loading => 0,
            LoadStatus.NotLoaded => 0,
            LoadStatus.Loaded => 1,
            LoadStatus.Failed => 2,
            _ => 3
        };
    }
}