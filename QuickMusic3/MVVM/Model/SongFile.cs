using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public class SongFile : IDisposable
{
    public readonly string FilePath;
    public readonly Loadable<MutableStream> Stream;
    public readonly Loadable<Metadata> Metadata;

    public SongFile(string path)
    {
        FilePath = path;
        Stream = new NeedyLoadable<MutableStream>(() => new MutableStream(path));
        Metadata = new FakerLoadable<Metadata>(
            () => new Metadata(path),
            () => new Metadata() { Title = Path.GetFileName(path) }
        );
    }

    public void Dispose()
    {
        if (Stream.IsLoaded)
            Stream.Item.Dispose();
    }
}
