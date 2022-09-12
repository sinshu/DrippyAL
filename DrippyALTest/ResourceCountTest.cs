using System;
using NUnit.Framework;
using DrippyAL;

namespace DrippyALTest
{
    public class ResourceCountTest
    {
        [Test]
        public void Channel()
        {
            using (var device = new AudioDevice())
            {
                Channel channel1;
                Channel channel2;
                Channel channel3;

                Assert.AreEqual(0, device.Channels.Count);
                channel1 = new Channel(device);
                Assert.AreEqual(1, device.Channels.Count);
                channel2 = new Channel(device);
                Assert.AreEqual(2, device.Channels.Count);
                channel1.Dispose();
                Assert.AreEqual(1, device.Channels.Count);
                channel3 = new Channel(device);
                Assert.AreEqual(2, device.Channels.Count);
                channel3.Dispose();
                Assert.AreEqual(1, device.Channels.Count);
                channel2.Dispose();
                Assert.AreEqual(0, device.Channels.Count);
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

                Assert.AreEqual(0, device.AudioClips.Count);
                audioClip1 = new AudioClip(device, 44100, 2, new short[2]);
                Assert.AreEqual(1, device.AudioClips.Count);
                audioClip2 = new AudioClip(device, 44100, 2, new short[2]);
                Assert.AreEqual(2, device.AudioClips.Count);
                audioClip1.Dispose();
                Assert.AreEqual(1, device.AudioClips.Count);
                audioClip3 = new AudioClip(device, 44100, 2, new short[2]);
                Assert.AreEqual(2, device.AudioClips.Count);
                audioClip3.Dispose();
                Assert.AreEqual(1, device.AudioClips.Count);
                audioClip2.Dispose();
                Assert.AreEqual(0, device.AudioClips.Count);
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

                Assert.AreEqual(0, device.AudioStreams.Count);
                audioStream1 = new AudioStream(device, 44100, 2);
                Assert.AreEqual(1, device.AudioStreams.Count);
                audioStream2 = new AudioStream(device, 44100, 2);
                Assert.AreEqual(2, device.AudioStreams.Count);
                audioStream1.Dispose();
                Assert.AreEqual(1, device.AudioStreams.Count);
                audioStream3 = new AudioStream(device, 44100, 2);
                Assert.AreEqual(2, device.AudioStreams.Count);
                audioStream3.Dispose();
                Assert.AreEqual(1, device.AudioStreams.Count);
                audioStream2.Dispose();
                Assert.AreEqual(0, device.AudioStreams.Count);
            }
        }
    }
}
