using System;
using System.Runtime.ExceptionServices;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    public unsafe sealed class WaveData : IDisposable
    {
        private AL? al;

        private int sampleRate;
        private int channelCount;

        private uint alBuffer;

        private TimeSpan duration;

        public WaveData(AudioDevice device, int sampleRate, int channelCount, Span<short> data)
        {
            try
            {
                if (device == null)
                {
                    throw new ArgumentNullException(nameof(device));
                }

                if (sampleRate <= 0)
                {
                    throw new ArgumentException("The sample rate must be a positive value.", nameof(sampleRate));
                }

                if (!(channelCount == 1 || channelCount == 2))
                {
                    throw new ArgumentException("The number of channels must be 1 or 2.", nameof(channelCount));
                }

                if (data.Length == 0)
                {
                    throw new ArgumentException("The length of the data must be greater than zero.", nameof(data));
                }

                if (data.Length % channelCount != 0)
                {
                    throw new ArgumentException("The length of the data must be even if the audio format is stereo.");
                }

                al = device.AL;

                this.sampleRate = sampleRate;
                this.channelCount = channelCount;

                alBuffer = al.GenBuffer();
                if (al.GetError() != AudioError.NoError)
                {
                    throw new Exception("Failed to generate an audio buffer.");
                }

                var format = channelCount == 1 ? BufferFormat.Mono16 : BufferFormat.Stereo16;
                var size = sizeof(short) * data.Length;
                fixed (short* p = data)
                {
                    al.BufferData(alBuffer, format, p, size, sampleRate);
                }

                duration = TimeSpan.FromSeconds(data.Length / channelCount / (double)sampleRate);
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        public WaveData(AudioDevice device, int sampleRate, int channelCount, Span<byte> data)
        {
            try
            {
                if (device == null)
                {
                    throw new ArgumentNullException(nameof(device));
                }

                if (sampleRate <= 0)
                {
                    throw new ArgumentException("The sample rate must be a positive value.", nameof(sampleRate));
                }

                if (!(channelCount == 1 || channelCount == 2))
                {
                    throw new ArgumentException("The number of channels must be 1 or 2.", nameof(channelCount));
                }

                if (data.Length == 0)
                {
                    throw new ArgumentException("The length of the data must be greater than zero.", nameof(data));
                }

                if (data.Length % channelCount != 0)
                {
                    throw new ArgumentException("The length of the data must be even if the audio format is stereo.");
                }

                al = device.AL;

                this.sampleRate = sampleRate;
                this.channelCount = channelCount;

                alBuffer = al.GenBuffer();
                if (al.GetError() != AudioError.NoError)
                {
                    throw new Exception("Failed to generate an audio buffer.");
                }

                var format = channelCount == 1 ? BufferFormat.Mono8 : BufferFormat.Stereo8;
                var size = sizeof(byte) * data.Length;
                fixed (byte* p = data)
                {
                    al.BufferData(alBuffer, format, p, size, sampleRate);
                }

                duration = TimeSpan.FromSeconds(data.Length / channelCount / (double)sampleRate);
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        public void Dispose()
        {
            if (al == null)
            {
                return;
            }

            if (alBuffer != 0)
            {
                al.DeleteBuffer(alBuffer);
                alBuffer = 0;
            }

            al = null;
        }

        internal uint AlBuffer
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(WaveData));
                }

                return alBuffer;
            }
        }

        public int SampleRate => sampleRate;
        public int ChannelCount => channelCount;
        public TimeSpan Duration => duration;
    }
}
