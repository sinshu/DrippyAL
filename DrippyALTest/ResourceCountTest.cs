using System;
using NUnit.Framework;
using DrippyAL;

namespace DrippyALTest
{
    public class ResourceCountTest
    {
        [Test]
        public void AudioChannel()
        {
            using (var device = new AudioDevice())
            {
                AudioChannel channel1;
                AudioChannel channel2;
                AudioChannel channel3;

                Assert.That(0, Is.EqualTo(device.Channels.Count));
                channel1 = new AudioChannel(device);
                Assert.That(1, Is.EqualTo(device.Channels.Count));
                channel2 = new AudioChannel(device);
                Assert.That(2, Is.EqualTo(device.Channels.Count));
                channel1.Dispose();
                Assert.That(1, Is.EqualTo(device.Channels.Count));
                channel3 = new AudioChannel(device);
                Assert.That(2, Is.EqualTo(device.Channels.Count));
                channel3.Dispose();
                Assert.That(1, Is.EqualTo(device.Channels.Count));
                channel2.Dispose();
                Assert.That(0, Is.EqualTo(device.Channels.Count));
            }
        }

        [Test]
        public void AudioClip()
        {
            using (var device = new AudioDevice())
            {
                AudioClip audioClip1;
                AudioClip audioClip2;
                AudioClip audioClip3;

                Assert.That(0, Is.EqualTo(device.AudioClips.Count));
                audioClip1 = new AudioClip(device, 44100, 2, new short[2]);
                Assert.That(1, Is.EqualTo(device.AudioClips.Count));
                audioClip2 = new AudioClip(device, 44100, 2, new short[2]);
                Assert.That(2, Is.EqualTo(device.AudioClips.Count));
                audioClip1.Dispose();
                Assert.That(1, Is.EqualTo(device.AudioClips.Count));
                audioClip3 = new AudioClip(device, 44100, 2, new short[2]);
                Assert.That(2, Is.EqualTo(device.AudioClips.Count));
                audioClip3.Dispose();
                Assert.That(1, Is.EqualTo(device.AudioClips.Count));
                audioClip2.Dispose();
                Assert.That(0, Is.EqualTo(device.AudioClips.Count));
            }
        }

        [Test]
        public void AudioStream()
        {
            using (var device = new AudioDevice())
            {
                AudioStream audioStream1;
                AudioStream audioStream2;
                AudioStream audioStream3;

                Assert.That(0, Is.EqualTo(device.AudioStreams.Count));
                audioStream1 = new AudioStream(device, 44100, 2);
                Assert.That(1, Is.EqualTo(device.AudioStreams.Count));
                audioStream2 = new AudioStream(device, 44100, 2);
                Assert.That(2, Is.EqualTo(device.AudioStreams.Count));
                audioStream1.Dispose();
                Assert.That(1, Is.EqualTo(device.AudioStreams.Count));
                audioStream3 = new AudioStream(device, 44100, 2);
                Assert.That(2, Is.EqualTo(device.AudioStreams.Count));
                audioStream3.Dispose();
                Assert.That(1, Is.EqualTo(device.AudioStreams.Count));
                audioStream2.Dispose();
                Assert.That(0, Is.EqualTo(device.AudioStreams.Count));
            }
        }
    }
}
