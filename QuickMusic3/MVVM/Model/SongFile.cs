using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using QuickMusic3.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class SongFile : ObservableObject, IDisposable
{
    public string FilePath { get; }
    public Loadable<MutableStream> Stream { get; }
    public Loadable<Metadata> Metadata { get; }

    public SongFile(string path)
    {
        FilePath = path;
        Stream = new NeedyLoadable<MutableStream>(() => new MutableStream(path));
        Metadata = new PlaceholderLoadable<Metadata>(
            () => new Metadata(path),
            new Metadata() { Title = Path.GetFileName(path) }
        );
        Metadata.Loaded += (s, e) => OnPropertyChanged(nameof(Metadata));
        Stream.Loaded += (s, e) =>
        {
            OnPropertyChanged(nameof(Stream));
            Metadata.LoadNow();
            if (Metadata.Item.ReplayGain != 0)
                Stream.Item.AddTransform(x => new DecibalOffsetProvider(x, Metadata.Item.ReplayGain));
        };
    }

    public void Dispose()
    {
        if (Stream.IsLoaded)
            Stream.Item.Dispose();
    }
}
