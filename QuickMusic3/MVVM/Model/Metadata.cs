using NAudio.Wave;
using QuickMusic3.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QuickMusic3.MVVM.Model;

public class Metadata : ObservableObject
{
    private string title;
    private string artist;
    private string album;
    private uint disc_number;
    private uint track_number;
    private TimeSpan duration;
    private BitmapSource thumbnail;
    private decimal replay_gain;
    public event EventHandler Loaded;
    public bool IsLoaded { get; private set; } = false;
    private Task LoadingTask;
    private readonly object loading_lock = new();
    public string Title { get { LoadBackground(); if (!IsLoaded) return Path.GetFileName(FilePath); return title; } }
    public string Artist { get { LoadBackground(); return artist; } }
    public string Album { get { LoadBackground(); return album; } }
    public uint DiscNumber { get { LoadBackground(); return disc_number; } }
    public uint TrackNumber { get { LoadBackground(); return track_number; } }
    public TimeSpan Duration { get { LoadBackground(); return duration; } }
    public BitmapSource Thumbnail { get { LoadBackground(); return thumbnail; } }
    public decimal ReplayGain { get { LoadBackground(); return replay_gain; } }
    public BitmapSource HighResImage
    {
        get
        {
            using var file = TagLib.File.Create(FilePath);
            return ArtCache.GetHighResEmbeddedImage(file.Tag);
        }
    }

    private readonly string FilePath;
    public Metadata(string path)
    {
        FilePath = path;
    }

    public void LoadBackground()
    {
        lock (loading_lock)
        {
            if (!IsLoaded && (LoadingTask == null || LoadingTask.IsCompleted))
                LoadingTask = Task.Run(Load).ContinueWith(x => SignalChanges(), TaskContinuationOptions.ExecuteSynchronously);
        }
    }

    public void LoadNow()
    {
        lock (loading_lock)
        {
            if (LoadingTask != null && !LoadingTask.IsCompleted)
                LoadingTask.Wait();
            else if (!IsLoaded)
            {
                Load();
                SignalChanges();
            }
        }
    }

    private void Load()
    {
        try
        {
            using var file = TagLib.File.Create(FilePath);
            title = file.Tag.Title ?? Path.GetFileName(FilePath);
            artist = file.Tag.FirstPerformer;
            album = file.Tag.Album;
            track_number = file.Tag.Track;
            disc_number = file.Tag.Disc;
            duration = file.Properties.Duration;
            thumbnail = ArtCache.GetEmbeddedImage(file.Tag);
            replay_gain = LoadReplayGain(file);
            IsLoaded = true;
        }
        catch { return; }
    }

    private void SignalChanges()
    {
        Loaded?.Invoke(this, EventArgs.Empty);
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Artist));
        OnPropertyChanged(nameof(Album));
        OnPropertyChanged(nameof(Thumbnail));
        OnPropertyChanged(nameof(DiscNumber));
        OnPropertyChanged(nameof(TrackNumber));
        OnPropertyChanged(nameof(Duration));
        OnPropertyChanged(nameof(ReplayGain));
        OnPropertyChanged(nameof(IsLoaded));
        Debug.WriteLine($"Loaded metadata for {Path.GetFileName(FilePath)}");
    }

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
