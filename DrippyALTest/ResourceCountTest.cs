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
        public void WaveData()
        {
            using (var device = new AudioDevice())
            {
                WaveData waveData1;
                WaveData waveData2;
                WaveData waveData3;

                Assert.AreEqual(0, device.WaveDatas.Count);
                waveData1 = new WaveData(device, 44100, 2, new short[2]);
                Assert.AreEqual(1, device.WaveDatas.Count);
                waveData2 = new WaveData(device, 44100, 2, new short[2]);
                Assert.AreEqual(2, device.WaveDatas.Count);
                waveData1.Dispose();
                Assert.AreEqual(1, device.WaveDatas.Count);
                waveData3 = new WaveData(device, 44100, 2, new short[2]);
                Assert.AreEqual(2, device.WaveDatas.Count);
                waveData3.Dispose();
                Assert.AreEqual(1, device.WaveDatas.Count);
                waveData2.Dispose();
                Assert.AreEqual(0, device.WaveDatas.Count);
            }
        }

        [Test]
        public void Stream()
        {
            using (var device = new AudioDevice())
            {
                AudioStream stream1;
                AudioStream stream2;
                AudioStream stream3;

                Assert.AreEqual(0, device.Streams.Count);
                stream1 = new AudioStream(device, 44100, 2);
                Assert.AreEqual(1, device.Streams.Count);
                stream2 = new AudioStream(device, 44100, 2);
                Assert.AreEqual(2, device.Streams.Count);
                stream1.Dispose();
                Assert.AreEqual(1, device.Streams.Count);
                stream3 = new AudioStream(device, 44100, 2);
                Assert.AreEqual(2, device.Streams.Count);
                stream3.Dispose();
                Assert.AreEqual(1, device.Streams.Count);
                stream2.Dispose();
                Assert.AreEqual(0, device.Streams.Count);
            }
        }
    }
}
