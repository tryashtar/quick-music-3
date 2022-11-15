using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TryashtarUtils.Music;

namespace QuickMusic3.MVVM.Model;

public sealed class Metadata : ObservableObject
{
    public string Title { get; init; }
    public string Artist { get; init; }
    public string Album { get; init; }
    public uint DiscNumber { get; init; }
    public uint TrackNumber { get; init; }
    public TimeSpan Duration { get; init; }
    public BitmapSource Thumbnail { get; init; }
    public decimal ReplayGain { get; init; }
    public ChapterCollection Chapters { get; init; }
    public Lyrics Lyrics { get; init; }
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
        var artists = file.Tag.Performers.Concat(file.Tag.Composers);
        if (file.Tag.RemixedBy != null)
            artists = artists.Append(file.Tag.RemixedBy);
        Artist = String.Join("; ", artists.Select(x => x.Trim()).Distinct());
        Album = file.Tag.Album;
        TrackNumber = file.Tag.Track;
        DiscNumber = file.Tag.Disc;
        Duration = file.Properties.Duration;
        Thumbnail = ArtCache.GetEmbeddedImage(file.Tag);
        ReplayGain = LoadReplayGain(file);
        Chapters = ChaptersIO.FromFile(file);
        Lyrics = LyricsIO.FromFile(file);
    }
    public Metadata() { }

    private static decimal LoadReplayGain(TagLib.File file)
    {
        const string TRACK_GAIN = "REPLAYGAIN_TRACK_GAIN";
        var id3v2 = (TagLib.Id3v2.Tag)file.GetTag(TagLib.TagTypes.Id3v2);
        if (id3v2 != null)
        {
            var frames = id3v2.GetFrames<TagLib.Id3v2.RelativeVolumeFrame>();
            foreach (var frame in frames)
            {
                foreach (var channel in frame.Channels)
                {
                    return (decimal)frame.GetVolumeAdjustment(channel);
                }
            }
        }
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
