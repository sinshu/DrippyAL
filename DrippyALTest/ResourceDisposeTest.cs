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
            Channel channel1;
            Channel channel2;
            Channel channel3;

            WaveData waveData1;
            WaveData waveData2;
            WaveData waveData3;

            AudioStream stream1;
            AudioStream stream2;
            AudioStream stream3;

            using (var device = new AudioDevice())
            {
                channel1 = new Channel(device);
                channel2 = new Channel(device);
                channel3 = new Channel(device);

                waveData1 = new WaveData(device, 44100, 2, new short[2]);
                waveData2 = new WaveData(device, 44100, 2, new short[2]);
                waveData3 = new WaveData(device, 44100, 2, new short[2]);

                stream1 = new AudioStream(device, 44100, 2);
                stream2 = new AudioStream(device, 44100, 2);
                stream3 = new AudioStream(device, 44100, 2);

                channel1.Dispose();
                waveData2.Dispose();
                stream3.Dispose();

                Assert.Catch(() => channel1.Play());
                Assert.AreEqual(waveData2.AlBuffer, 0);
                Assert.Catch(() => stream3.Play(block => Array.Clear(block)));
            }

            Assert.Catch(() => channel1.Play());
            Assert.Catch(() => channel2.Play());
            Assert.Catch(() => channel3.Play());

            Assert.AreEqual(waveData1.AlBuffer, 0);
            Assert.AreEqual(waveData2.AlBuffer, 0);
            Assert.AreEqual(waveData3.AlBuffer, 0);

            Assert.Catch(() => stream1.Play(block => Array.Clear(block)));
            Assert.Catch(() => stream2.Play(block => Array.Clear(block)));
            Assert.Catch(() => stream3.Play(block => Array.Clear(block)));
        }

        [Test]
        public void DetachWaveDataFromCorrespondingChannels()
        {
            Channel channel1;
            Channel channel2;
            Channel channel3;

            WaveData waveData1;
            WaveData waveData2;

            using (var device = new AudioDevice())
            {
                channel1 = new Channel(device);
                channel2 = new Channel(device);
                channel3 = new Channel(device);

                waveData1 = new WaveData(device, 44100, 2, new short[2]);
                waveData2 = new WaveData(device, 44100, 2, new short[2]);

                channel1.WaveData = waveData1;
                channel2.WaveData = waveData2;
                channel3.WaveData = waveData2;

                Assert.AreEqual(channel1.WaveData, waveData1);
                waveData1.Dispose();
                Assert.AreEqual(channel1.WaveData, null);

                Assert.AreEqual(channel2.WaveData, waveData2);
                Assert.AreEqual(channel3.WaveData, waveData2);
                waveData2.Dispose();
                Assert.AreEqual(channel2.WaveData, null);
                Assert.AreEqual(channel3.WaveData, null);
            }
        }
    }
}
