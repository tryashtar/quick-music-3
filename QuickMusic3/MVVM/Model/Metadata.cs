using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QuickMusic3.MVVM.Model;

public class Metadata : ObservableObject
{
    public string Title { get; init; }
    public string Artist { get; init; }
    public string Album { get; init; }
    public uint DiscNumber { get; init; }
    public uint TrackNumber { get; init; }
    public TimeSpan Duration { get; init; }
    public BitmapSource Thumbnail { get; init; }
    public decimal ReplayGain { get; init; }
    public BitmapSource HighResImage
    {
        get
        {
            if (FilePath == null)
                return null;
            using var file = TagLib.File.Create(FilePath);
            return ArtCache.GetHighResEmbeddedImage(file.Tag);
        }
    }

    private readonly string FilePath;
    public Metadata(string path)
    {
        FilePath = path;
        using var file = TagLib.File.Create(path);
        Title = file.Tag.Title ?? Path.GetFileName(path);
        Artist = String.Join("; ", file.Tag.Performers.Select(x => x.Trim()));
        Album = file.Tag.Album;
        TrackNumber = file.Tag.Track;
        DiscNumber = file.Tag.Disc;
        Duration = file.Properties.Duration;
        Thumbnail = ArtCache.GetEmbeddedImage(file.Tag);
        ReplayGain = LoadReplayGain(file);
    }
    public Metadata() { }

    private static decimal LoadReplayGain(TagLib.File file)
    {
        const string TRACK_GAIN = "REPLAYGAIN_TRACK_GAIN";
        var ape = (TagLib.Ape.Tag)file.GetTag(TagLib.TagTypes.Ape);
        if (ape != null)
        {
            if (ape.HasItem(TRACK_GAIN))
                return ParseDB(ape.GetItem(TRACK_GAIN).ToString());
        }
        var ogg = (TagLib.Ogg.XiphComment)file.GetTag(TagLib.TagTypes.Xiph);
        if (ogg != null)
        {
            var gain = ogg.GetFirstField(TRACK_GAIN);
            if (gain != null)
                return ParseDB(gain);
        }
        return 0;
    }

    private static decimal ParseDB(string db)
    {
        return decimal.Parse(db[..^" dB".Length]);
    }
}
