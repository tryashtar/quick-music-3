using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace QuickMusic3.MVVM.Model;

public class PlaylistFileSource : SourceAggregator
{
    public PlaylistFileSource(string file)
    {
        var folder = Path.GetDirectoryName(file);
        var entries = new List<string>();
        foreach (var line in File.ReadAllLines(file))
        {
            if (String.IsNullOrWhiteSpace(line))
                continue;
            if (line.StartsWith("#"))
                continue;
            // make sure references load relative to the m3u file itself
            // if reference path is absolute, it just uses that which is perfect
            entries.Add(Path.Combine(folder, line));
        }
        foreach (var item in SongSourceExtensions.FromFileList(entries, SearchOption.TopDirectoryOnly))
        {
            AddSource(item, false);
        }
    }

    protected override void AddSource(ISongSource source, bool send_events)
    {
        int index;
        lock (FlatList)
        {
            index = FlatList.Count;
            SourcePositions[source] = index;
            FlatList.AddRange(source);
        }
        if (send_events)
            SendEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, source.ToList(), index));
        source.CollectionChanged += (s, e) => HandleCollectionChanged(source, e);
    }
}
