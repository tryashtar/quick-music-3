using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace QuickMusic3.MVVM.Model;

public class DispatcherPlaylist : SourceAggregator
{
    private readonly Dispatcher Dispatcher;
    public DispatcherPlaylist(Dispatcher event_dispatcher)
    {
        Dispatcher = event_dispatcher;
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
        // this one needs to be normal Invoke, otherwise we can duplicate items sometimes
        if (send_events)
            Dispatcher.Invoke(() => SendEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, source.ToList(), index)));

        source.CollectionChanged += (s, e) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                HandleCollectionChanged(source, e);
            }, DispatcherPriority.ApplicationIdle);
        };
    }
}
