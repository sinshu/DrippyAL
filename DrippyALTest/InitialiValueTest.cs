using System;
using System.Numerics;
using NUnit.Framework;
using DrippyAL;

namespace DrippyALTest
{
    public class InitialValueTest
    {
        [Test]
        public void AudioDevice()
        {
            using (var device = new AudioDevice())
            {
                Assert.AreEqual(new Vector3(0, 0, 0), device.ListernerPosition);
                Assert.AreEqual(new Vector3(0, 0, -1), device.ListernerDirection);
                Assert.AreEqual(new Vector3(0, 1, 0), device.ListernerUpVector);
            }
        }

        [Test]
        public void Channel()
        {
            using (var device = new AudioDevice())
            {
                var channel = new Channel(device);
                Assert.AreEqual(1F, channel.Volume);
                Assert.AreEqual(1F, channel.Pitch);
                Assert.AreEqual(new Vector3(0, 0, 1), channel.Position);
                Assert.AreEqual(PlaybackState.Stopped, channel.State);
            }
        }

        [Test]
        public void WaveData()
        {
            using (var device = new AudioDevice())
            {
                var waveData = new WaveData(device, 44100, 1, new short[44100]);
                Assert.AreEqual(44100, waveData.SampleRate);
                Assert.AreEqual(1, waveData.ChannelCount);
                Assert.AreEqual(1.0, waveData.Duration.TotalSeconds, 1.0E-6);
            }
        }

        [Test]
        public void Stream()
        {
            using (var device = new AudioDevice())
            {
                var stream = new AudioStream(device, 44100, 2, 500, 4096);
                Assert.AreEqual(44100, stream.SampleRate);
                Assert.AreEqual(2, stream.ChannelCount);
                Assert.AreEqual(500, stream.Latency);
                Assert.AreEqual(4096, stream.BlockLength);
                Assert.AreEqual(1F, stream.Volume);
                Assert.AreEqual(1F, stream.Pitch);
                Assert.AreEqual(new Vector3(0, 0, 1), stream.Position);
                Assert.AreEqual(PlaybackState.Stopped, stream.State);
            }
        }
    }
}
