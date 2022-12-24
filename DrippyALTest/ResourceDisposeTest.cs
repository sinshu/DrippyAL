using System;
using NUnit.Framework;
using DrippyAL;

namespace DrippyALTest
{
    public class ResourceDisposeTest
    {
        [Test]
        public void AutoDispose()
        {
            AudioChannel channel1;
            AudioChannel channel2;
            AudioChannel channel3;

            AudioClip audioClip1;
            AudioClip audioClip2;
            AudioClip audioClip3;

            AudioStream audioStream1;
            AudioStream audioStream2;
            AudioStream audioStream3;

            using (var device = new AudioDevice())
            {
                channel1 = new AudioChannel(device);
                channel2 = new AudioChannel(device);
                channel3 = new AudioChannel(device);

                audioClip1 = new AudioClip(device, 44100, 2, new short[2]);
                audioClip2 = new AudioClip(device, 44100, 2, new short[2]);
                audioClip3 = new AudioClip(device, 44100, 2, new short[2]);

                audioStream1 = new AudioStream(device, 44100, 2);
                audioStream2 = new AudioStream(device, 44100, 2);
                audioStream3 = new AudioStream(device, 44100, 2);

                channel1.Dispose();
                audioClip2.Dispose();
                audioStream3.Dispose();

                Assert.Catch(() => channel1.Play());
                Assert.AreEqual(audioClip2.AlBuffer, 0);
                Assert.Catch(() => audioStream3.Play(block => Array.Clear(block)));
            }

            Assert.Catch(() => channel1.Play());
            Assert.Catch(() => channel2.Play());
            Assert.Catch(() => channel3.Play());

            Assert.AreEqual(audioClip1.AlBuffer, 0);
            Assert.AreEqual(audioClip2.AlBuffer, 0);
            Assert.AreEqual(audioClip3.AlBuffer, 0);

            Assert.Catch(() => audioStream1.Play(block => Array.Clear(block)));
            Assert.Catch(() => audioStream2.Play(block => Array.Clear(block)));
            Assert.Catch(() => audioStream3.Play(block => Array.Clear(block)));
        }

        [Test]
        public void DetachWaveDataFromCorrespondingChannels()
        {
            AudioChannel channel1;
            AudioChannel channel2;
            AudioChannel channel3;

            AudioClip audioClip1;
            AudioClip audioClip2;

            using (var device = new AudioDevice())
            {
                channel1 = new AudioChannel(device);
                channel2 = new AudioChannel(device);
                channel3 = new AudioChannel(device);

                audioClip1 = new AudioClip(device, 44100, 2, new short[2]);
                audioClip2 = new AudioClip(device, 44100, 2, new short[2]);

                channel1.AudioClip = audioClip1;
                channel2.AudioClip = audioClip2;
                channel3.AudioClip = audioClip2;

                Assert.AreEqual(channel1.AudioClip, audioClip1);
                audioClip1.Dispose();
                Assert.AreEqual(channel1.AudioClip, null);

                Assert.AreEqual(channel2.AudioClip, audioClip2);
                Assert.AreEqual(channel3.AudioClip, audioClip2);
                audioClip2.Dispose();
                Assert.AreEqual(channel2.AudioClip, null);
                Assert.AreEqual(channel3.AudioClip, null);
            }
        }
    }
}
