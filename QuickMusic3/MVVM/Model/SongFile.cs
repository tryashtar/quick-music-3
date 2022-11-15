using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using QuickMusic3.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TagLib.Flac;

namespace QuickMusic3.MVVM.Model;

public sealed class SongFile : ObservableObject, IAsyncDisposable
{
    public string FilePath { get; }
    public NewLoadable<MutableStream> Stream { get; }
    public NewLoadable<Metadata> Metadata { get; }
    public TimeSpan? GuessDuration
    {
        get
        {
            if (Stream.IsSuccessfullyCompleted)
                return Stream.Item.BaseStream.TotalTime;
            if (Metadata.IsSuccessfullyCompleted)
                return Metadata.Item.Duration;
            return null;
        }
    }

    private readonly List<Action<MutableStream>> StreamLoadActions = new();

    public SongFile(string path)
    {
        FilePath = Path.GetFullPath(path);
        Stream = new(create: MakeStreamAsync, invalid_check: x => x.IsDisposed);
        Metadata = new(create: MakeMetadata, new Metadata() { Title = Path.GetFileName(FilePath) });
    }

    private Metadata MakeMetadata()
    {
        var meta = new Metadata(FilePath);
        OnPropertyChanged(nameof(GuessDuration));
        return meta;
    }

    private async Task<MutableStream> MakeStreamAsync()
    {
        var stream = new MutableStream(FilePath);
        var meta = await Metadata;
        if (meta.ReplayGain != 0)
            stream.AddTransform(x => new DecibalOffsetProvider(x, meta.ReplayGain));
        foreach (var action in StreamLoadActions)
        {
            action(stream);
        }
        OnPropertyChanged(nameof(GuessDuration));
        return stream;
    }

    public async ValueTask DisposeAsync()
    {
        if (Stream.Status != TaskStatus.Created)
        {
            var result = await Stream;
            result.Dispose();
        }
    }
}
