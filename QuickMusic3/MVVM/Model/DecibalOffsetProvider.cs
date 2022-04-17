using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace QuickMusic3.MVVM.Model;

public class DecibalOffsetProvider : ISampleProvider
{
    private const float Epsilon = 0.0001f;
    public readonly decimal DecibalOffset = 0;
    public readonly ISampleProvider BaseSource;

    public DecibalOffsetProvider(ISampleProvider source, decimal decibals)
    {
        BaseSource = source;
        DecibalOffset = decibals;
    }

    public WaveFormat WaveFormat => BaseSource.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int read = BaseSource.Read(buffer, offset, count);
        float volume = (float)Math.Pow(10, (double)(DecibalOffset / 20));
        if (volume == 0f || (volume > -Epsilon && volume < Epsilon))
            Array.Clear(buffer, offset, read);
        else if (volume != 1f && !(volume > (1f - Epsilon) && volume < (1f + Epsilon)))
        {
            for (int i = offset; i < read + offset; i++)
            {
                buffer[i] *= volume;
            }
        }
        return read;
    }
}
