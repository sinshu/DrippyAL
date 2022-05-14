using System;
using DrippyAL;

public static class Program
{
    public static void Main()
    {
        using (var device = new AudioDevice())
        {
            var random = new Random();

            var data = new short[2 * 44100];
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = (short)random.Next(short.MinValue, short.MaxValue);
            }

            var waveData = new WaveData(device, 44100, 1, data);

            var channel = new Channel(device);

            channel.Play(waveData);

            Console.ReadKey();

            channel.Pause();

            Console.ReadKey();

            channel.Resume();

            Console.ReadKey();
        }
    }
}
