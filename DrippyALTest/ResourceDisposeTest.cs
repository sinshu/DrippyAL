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
                Assert.That(audioClip2.AlBuffer, Is.EqualTo(0));
                Assert.Catch(() => audioStream3.Play(block => Array.Clear(block)));
            }

            Assert.Catch(() => channel1.Play());
            Assert.Catch(() => channel2.Play());
            Assert.Catch(() => channel3.Play());

            Assert.That(audioClip1.AlBuffer, Is.EqualTo(0));
            Assert.That(audioClip2.AlBuffer, Is.EqualTo(0));
            Assert.That(audioClip3.AlBuffer, Is.EqualTo(0));

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

                Assert.That(channel1.AudioClip, Is.EqualTo(audioClip1));
                audioClip1.Dispose();
                Assert.That(channel1.AudioClip, Is.EqualTo(null));

                Assert.That(channel2.AudioClip, Is.EqualTo(audioClip2));
                Assert.That(channel3.AudioClip, Is.EqualTo(audioClip2));
                audioClip2.Dispose();
                Assert.That(channel2.AudioClip, Is.EqualTo(null));
                Assert.That(channel3.AudioClip, Is.EqualTo(null));
            }
        }
    }
}
