using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace QuickMusic3.MVVM.Model;

public abstract class SourceAggregator : ISongSource
{
    protected readonly List<SongFile> FlatList = new();
    protected readonly Dictionary<ISongSource, int> SourcePositions = new();

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public SongFile this[int index] => FlatList[index];
    public int Count => FlatList.Count;
    public int IndexOf(SongFile song) => FlatList.IndexOf(song);
    public IEnumerator<SongFile> GetEnumerator() => FlatList.GetEnumerator();

    public async Task GetInOrderAsync(int index)
    {
        foreach (var item in SourcePositions.OrderBy(x => x.Value))
        {
            if (index >= item.Value)
            {
                await item.Key.GetInOrderAsync(index - item.Value);
            }
        }
    }

    public void Remove(SongFile song)
    {
        foreach (var item in SourcePositions.Keys)
        {
            item.Remove(song);
        }
    }

    protected void SendEvent(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    protected void HandleCollectionChanged(ISongSource source, NotifyCollectionChangedEventArgs e)
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
                SendEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, old_index + e.OldStartingIndex));
                //Debug.WriteLine("R " + String.Join(' ', FlatList.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : "-")));
            }
            else if (e.Action == NotifyCollectionChangedAction.Move)
            {
                var item = FlatList[old_index + e.OldStartingIndex];
                int destination = old_index + e.NewStartingIndex;
                FlatList.RemoveAt(old_index + e.OldStartingIndex);
                FlatList.Insert(destination, item);
                SendEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, destination, old_index + e.OldStartingIndex));
                //Debug.WriteLine("M " + String.Join(' ', FlatList.Select(x => x.Metadata.IsLoaded ? x.Metadata.Item.TrackNumber.ToString() : x.Metadata.LoadStatus == LoadStatus.Failed ? "F" : "-")));
            }
            else
                throw new NotSupportedException();
        }
    }

    protected abstract void AddSource(ISongSource source, bool send_events);
    public void AddSource(ISongSource source) => AddSource(source, true);
}
