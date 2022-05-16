using System;
using System.Runtime.ExceptionServices;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    /// <summary>
    /// Represents an instance of wave data for playback.
    /// To play a sound from the wave data, use the <see cref="Channel.Play(WaveData)"/> method.
    /// </summary>
    public unsafe sealed class WaveData : IDisposable
    {
        private AudioDevice device;
        private int sampleRate;
        private int channelCount;

        private uint alBuffer;

        private TimeSpan duration;

        /// <summary>
        /// Creates a new instance of wave data from the 16-bit PCM data.
        /// </summary>
        /// <param name="device">The <see cref="AudioDevice"/> to play the wave data.</param>
        /// <param name="sampleRate">The sample rate of the wave data.</param>
        /// <param name="channelCount">The number of channels of the wave data. This value must be 1 or 2.</param>
        /// <param name="data">The wave data for playback. The format must be 16-bit PCM.</param>
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

                if (channelCount != 1 && channelCount != 2)
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

                this.device = device;
                this.sampleRate = sampleRate;
                this.channelCount = channelCount;

                alBuffer = device.AL.GenBuffer();
                if (device.AL.GetError() != AudioError.NoError)
                {
                    throw new Exception("Failed to generate an audio buffer.");
                }

                var format = channelCount == 1 ? BufferFormat.Mono16 : BufferFormat.Stereo16;
                var size = sizeof(short) * data.Length;
                fixed (short* p = data)
                {
                    device.AL.BufferData(alBuffer, format, p, size, sampleRate);
                }

                duration = TimeSpan.FromSeconds(data.Length / channelCount / (double)sampleRate);

                device.AddResource(this);
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        /// <summary>
        /// Creates a new instance of wave data from the 8-bit PCM data.
        /// </summary>
        /// <param name="device">The <see cref="AudioDevice"/> to play the wave data.</param>
        /// <param name="sampleRate">The sample rate of the wave data.</param>
        /// <param name="channelCount">The number of channels of the wave data. This value must be 1 or 2.</param>
        /// <param name="data">The wave data for playback. The format must be 8-bit PCM.</param>
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

                if (channelCount != 1 && channelCount != 2)
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

                this.device = device;
                this.sampleRate = sampleRate;
                this.channelCount = channelCount;

                alBuffer = device.AL.GenBuffer();
                if (device.AL.GetError() != AudioError.NoError)
                {
                    throw new Exception("Failed to generate an audio buffer.");
                }

                var format = channelCount == 1 ? BufferFormat.Mono16 : BufferFormat.Stereo16;
                var size = sizeof(byte) * data.Length;
                fixed (byte* p = data)
                {
                    device.AL.BufferData(alBuffer, format, p, size, sampleRate);
                }

                duration = TimeSpan.FromSeconds(data.Length / channelCount / (double)sampleRate);

                device.AddResource(this);
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        /// <summary>
        /// Disposes the resources held by the <see cref="WaveData"/>.
        /// </summary>
        public void Dispose()
        {
            if (device == null)
            {
                return;
            }

            if (alBuffer != 0)
            {
                device.AL.DeleteBuffer(alBuffer);
                alBuffer = 0;
            }

            device.RemoveResource(this);
            device = null;
        }

        internal uint AlBuffer => alBuffer;

        /// <summary>
        /// Gets the sample rate of the wave data.
        /// </summary>
        public int SampleRate => sampleRate;

        /// <summary>
        /// Gets the number of channels of the wave data.
        /// </summary>
        public int ChannelCount => channelCount;

        /// <summary>
        /// Gets the duration of the wave data.
        /// </summary>
        public TimeSpan Duration => duration;
    }
}
