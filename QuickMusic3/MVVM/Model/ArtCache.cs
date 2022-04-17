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
        var hd = DataToImage(data);
        if (hd == null)
            return null;
        var thumbnail = Thumbnail(hd);
        lock (HashToImage)
        {
            HashToImage[hash] = thumbnail;
            ImageToHash[thumbnail] = hash;
        }
        return thumbnail;
    }

    private static BitmapSource Thumbnail(BitmapSource image)
    {
        float max_width = 64;
        float max_height = 64;
        double ratio = Math.Min(max_width / image.Width, max_height / image.Height);
        var result = new TransformedBitmap(image, new ScaleTransform(ratio, ratio));
        return result;
    }

    public static BitmapSource GetHighResEmbeddedImage(TagLib.Tag tag)
    {
        var data = FetchEmbeddedImageData(tag);
        return DataToImage(data);
    }

    private static BitmapSource DataToImage(byte[] data)
    {
        if (data == null)
            return null;
        var stream = new MemoryStream(data);
        try
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = stream;
            img.EndInit();
            return img;
        }
        catch { return null; }
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
