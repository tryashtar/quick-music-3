using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuickMusic3.MVVM.Model;

// manages images embedded in ID3 tags
// they're all "separate" per se, so we could waste a ton of memory loading them all in naively
// instead we do some quick hashing here to only keep unique images
public static class ArtCache
{
    private static readonly Dictionary<string, BitmapSource> HashToImage = new();
    private static readonly Dictionary<BitmapSource, string> ImageToHash = new();
    public static BitmapSource GetEmbeddedImage(TagLib.Tag tag)
    {
        var data = FetchEmbeddedImageData(tag);
        if (data == null)
            return null;
        using var hasher = MD5.Create();
        var hash = Convert.ToBase64String(hasher.ComputeHash(data));
        if (HashToImage.TryGetValue(hash, out var existing))
            return existing;
        var thumbnail = DataToImage(data, 64);
        lock (HashToImage)
        {
            HashToImage[hash] = thumbnail;
            ImageToHash[thumbnail] = hash;
        }
        return thumbnail;
    }

    internal static void SaveTo(string folder)
    {
        Directory.CreateDirectory(folder);
        foreach (var item in HashToImage)
        {
            using var stream = new FileStream(Path.Combine(folder, item.Key.Replace('/', '_') + ".png"), FileMode.Create);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(item.Value));
            encoder.Save(stream);
        }
    }

    public static BitmapSource GetHighResEmbeddedImage(TagLib.Tag tag)
    {
        var data = FetchEmbeddedImageData(tag);
        return DataToImage(data);
    }

    private static BitmapSource DataToImage(byte[] data, int? set_width = null)
    {
        if (data == null)
            return null;
        var stream = new MemoryStream(data);
        try
        {
            var img = new BitmapImage();
            img.BeginInit();
            if (set_width != null)
                img.DecodePixelWidth = set_width.Value;
            img.StreamSource = stream;
            img.EndInit();
            img.Freeze();
            return img;
        }
        catch { stream.Dispose(); return null; }
    }

    private static byte[] FetchEmbeddedImageData(TagLib.Tag tag)
    {
        var frames = tag.Pictures;
        if (frames == null || frames.Length == 0)
            return null;
        return frames[0].Data.Data;
    }

    public static BitmapSource Get(string hash)
    {
        HashToImage.TryGetValue(hash, out var result);
        return result;
    }

    public static string Hash(BitmapSource image)
    {
        ImageToHash.TryGetValue(image, out var result);
        return result;
    }
}
