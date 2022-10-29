using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuickMusic3.MVVM.Model;

public static class BaseReaders
{
    public static WaveStream Auto(string fileName)
    {
        var fileStream = File.OpenRead(fileName);
        var possible_streams = new Func<WaveStream>[]
        {
            // disable MP3 because of the "MP3FileReader does not support sample rate changes"
            //() => TryOpenMP3(fileStream),
            () => TryOpenAiff(fileStream),
            () =>
            {
                WaveStream stream = TryOpenWave(fileStream);
                if (stream == null)
                    return null;
                if (stream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && stream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                {
                    stream = WaveFormatConversionStream.CreatePcmStream(stream);
                    stream = new BlockAlignReductionStream(stream);
                }
                return stream;
            },
            () => TryOpenOgg(fileStream),
            () => TryOpenFlac(fileStream),
            () => new MediaFoundationReader(fileStream.Name)
        };
        int first = 0;
        if (fileName.EndsWith(".aiff", StringComparison.OrdinalIgnoreCase) || fileName.EndsWith(".aif", StringComparison.OrdinalIgnoreCase))
            first = 0;
        if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            first = 1;
        if (fileName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            first = 2;
        if (fileName.EndsWith(".flac", StringComparison.OrdinalIgnoreCase))
            first = 3;
        WaveStream readerStream;
        readerStream = possible_streams[first]();
        if (readerStream != null)
            return readerStream;
        for (int i = 0; i < possible_streams.Length; i++)
        {
            if (i == first)
                continue;
            readerStream = possible_streams[i]();
            if (readerStream != null)
                return readerStream;
        }
        throw new ArgumentException($"Couldn't find reader for {fileName}");
    }

    public static Mp3FileReader TryOpenMP3(FileStream stream)
    {
        byte[] first_bytes = new byte[3];
        stream.Read(first_bytes, 0, 3);
        stream.Position = 0;
        if ((first_bytes[0] == 0x49 && first_bytes[1] == 0x44 && first_bytes[2] == 0x33) || first_bytes[0] == 0xFF && first_bytes[1] == 0xFB)
            return new Mp3FileReader(stream, true);
        return null;
    }

    public static AiffFileReader TryOpenAiff(FileStream stream)
    {
        var br = new BinaryReader(stream);
        var name = br.ReadBytes(4);
        stream.Position = 0;
        if (name[0] != 'F' || name[1] != 'O' || name[2] != 'R' || name[3] != 'M')
            return null;
        return new AiffFileReader(stream, true);
    }

    public static WaveFileReader TryOpenWave(FileStream stream)
    {
        var br = new BinaryReader(stream);
        int header = br.ReadInt32();
        stream.Position = 0;
        if (header == ChunkIdentifier.ChunkIdentifierToInt32("RF64") || header == ChunkIdentifier.ChunkIdentifierToInt32("RIFF"))
            return new WaveFileReader(stream, true);
        return null;
    }

    private static NAudio.Vorbis.VorbisWaveReader TryOpenOgg(FileStream stream)
    {
        byte[] first_bytes = new byte[4];
        stream.Read(first_bytes, 0, 4);
        stream.Position = 0;
        if (first_bytes[0] == 'O' && first_bytes[1] == 'g' && first_bytes[2] == 'g' && first_bytes[3] == 'S')
            return new NAudio.Vorbis.VorbisWaveReader(stream, true);
        return null;
    }

    private static NAudio.Flac.FlacReader TryOpenFlac(FileStream stream)
    {
        byte[] first_bytes = new byte[4];
        stream.Read(first_bytes, 0, 4);
        stream.Position = 0;
        if (first_bytes[0] == 'f' && first_bytes[1] == 'L' && first_bytes[2] == 'a' && first_bytes[3] == 'C')
            return new NAudio.Flac.FlacReader(stream);
        return null;
    }
}
