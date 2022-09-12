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
        public void AudioClip()
        {
            using (var device = new AudioDevice())
            {
                var audioClip = new AudioClip(device, 44100, 1, new short[44100]);
                Assert.AreEqual(44100, audioClip.SampleRate);
                Assert.AreEqual(1, audioClip.ChannelCount);
                Assert.AreEqual(1.0, audioClip.Duration.TotalSeconds, 1.0E-6);
            }
        }

        [Test]
        public void AudioStream()
        {
            using (var device = new AudioDevice())
            {
                var audioStream = new AudioStream(device, 44100, 2, 500, 4096);
                Assert.AreEqual(44100, audioStream.SampleRate);
                Assert.AreEqual(2, audioStream.ChannelCount);
                Assert.AreEqual(500, audioStream.Latency);
                Assert.AreEqual(4096, audioStream.BlockLength);
                Assert.AreEqual(1F, audioStream.Volume);
                Assert.AreEqual(1F, audioStream.Pitch);
                Assert.AreEqual(new Vector3(0, 0, 1), audioStream.Position);
                Assert.AreEqual(PlaybackState.Stopped, audioStream.State);
            }
        }
    }
}
