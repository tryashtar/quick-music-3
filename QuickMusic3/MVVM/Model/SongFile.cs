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
    public Loadable<MutableStream> Stream { get; private set; }
    public Loadable<Metadata> Metadata { get; }
    public TimeSpan GuessDuration
    {
        get
        {
            if (Stream.IsLoaded)
                return Stream.Item.BaseStream.TotalTime;
            if (Metadata.IsLoaded)
                return Metadata.Item.Duration;
            return TimeSpan.Zero;
        }
    }

    public SongFile(string path)
    {
        FilePath = path;
        PrepareStream();
        Metadata = new PlaceholderLoadable<Metadata>(
            () => new Metadata(path),
            new Metadata() { Title = Path.GetFileName(path) }
        );
        Metadata.Loaded += (s, e) => OnPropertyChanged(nameof(Metadata));

#if DEBUG
        Metadata.Failed += (s, e) => Debug.WriteLine($"{Path.GetFileName(FilePath)}: Metadata load failed ({Metadata.Exception.Message})");
#endif
    }

    public void CloseStream()
    {
        if (Stream.IsLoaded)
            Stream.Item.Dispose();
        PrepareStream();
    }

    private void PrepareStream()
    {
        Stream = new NeedyLoadable<MutableStream>(() => new MutableStream(FilePath));
        Stream.Loaded += (s, e) =>
        {
            Debug.WriteLine($"{Path.GetFileName(FilePath)}: {Stream.Item.BaseStream.WaveFormat.SampleRate} / {Stream.Item.BaseStream.WaveFormat.Channels}");
            OnPropertyChanged(nameof(Stream));
            Metadata.LoadNow();
            if (Metadata.Item.ReplayGain != 0)
                Stream.Item.AddTransform(x => new DecibalOffsetProvider(x, Metadata.Item.ReplayGain));
        };
#if DEBUG
        Stream.Failed += (s, e) => Debug.WriteLine($"{Path.GetFileName(FilePath)}: Stream load failed ({((Loadable<MutableStream>)s).Exception.Message})");
#endif
    }

    public void Dispose()
    {
        if (Stream.IsLoaded)
            Stream.Item.Dispose();
    }
}
