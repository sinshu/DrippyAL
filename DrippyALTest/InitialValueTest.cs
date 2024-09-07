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
                Assert.That(new Vector3(0, 0, 0), Is.EqualTo(device.ListernerPosition));
                Assert.That(new Vector3(0, 0, -1), Is.EqualTo(device.ListernerDirection));
                Assert.That(new Vector3(0, 1, 0), Is.EqualTo(device.ListernerUpVector));
            }
        }

        [Test]
        public void AudioChannel()
        {
            using (var device = new AudioDevice())
            {
                var channel = new AudioChannel(device);
                Assert.That(1F, Is.EqualTo(channel.Volume));
                Assert.That(1F, Is.EqualTo(channel.Pitch));
                Assert.That(new Vector3(0, 0, -1), Is.EqualTo(channel.Position));
                Assert.That(PlaybackState.Stopped, Is.EqualTo(channel.State));
            }
        }

        [Test]
        public void AudioClip()
        {
            using (var device = new AudioDevice())
            {
                var audioClip = new AudioClip(device, 44100, 1, new short[44100]);
                Assert.That(44100, Is.EqualTo(audioClip.SampleRate));
                Assert.That(1, Is.EqualTo(audioClip.ChannelCount));
                Assert.That(1.0, Is.EqualTo(audioClip.Duration.TotalSeconds).Within(1.0E-6));
            }
        }

        [Test]
        public void AudioStream()
        {
            using (var device = new AudioDevice())
            {
                var audioStream = new AudioStream(device, 44100, 2, true, 500, 4096);
                Assert.That(44100, Is.EqualTo(audioStream.SampleRate));
                Assert.That(2, Is.EqualTo(audioStream.ChannelCount));
                Assert.That(500, Is.EqualTo(audioStream.Latency));
                Assert.That(4096, Is.EqualTo(audioStream.BlockLength));
                Assert.That(1F, Is.EqualTo(audioStream.Volume));
                Assert.That(1F, Is.EqualTo(audioStream.Pitch));
                Assert.That(new Vector3(0, 0, -1), Is.EqualTo(audioStream.Position));
                Assert.That(PlaybackState.Stopped, Is.EqualTo(audioStream.State));
            }
        }
    }
}
