using System;
using System.Numerics;
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

            var wave = new WaveData(device, 44100, 1, data);

            var channel = new Channel(device);

            channel.Play(wave);
            Console.ReadKey();

            channel.Position = new Vector3(-1F, 0F, 0F);

            Console.ReadKey();

            channel.Position = new Vector3(1F, 0F, 0F);

            Console.ReadKey();
        }
    }
}
