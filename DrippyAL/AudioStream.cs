using System;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    /// <summary>
    /// Provides the functionalities for streaming audio.
    /// </summary>
    public sealed class AudioStream : IDisposable
    {
        private static readonly int defaultLatency = 200;
        private static readonly int defaultBlockLength = 2048;

        private AudioDevice device;
        private int sampleRate;
        private int channelCount;
        private int latency;
        private int blockLength;

        private uint[] alBuffers;
        private BufferFormat format;

        private uint alSource;
        private float volume;
        private float pitch;
        private Vector3 position;

        private short[] blockData;
        private uint[] alBufferQueue;

        private Action<short[]> fillBlock;
        private CancellationTokenSource pollingCts;
        private Task pollingTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioStream"/> class.
        /// </summary>
        /// <param name="device">The <see cref="AudioDevice"/> to play the audio stream.</param>
        /// <param name="sampleRate">The sample rate of the audio stream.</param>
        /// <param name="channelCount">The number of channels of the audio stream. This value must be 1 or 2.</param>
        /// <param name="latency">The desired latency for audio processing in milliseconds.</param>
        /// <param name="blockLength">The desired block length for audio processing in sample frames.</param>
        public AudioStream(AudioDevice device, int sampleRate, int channelCount, int latency, int blockLength)
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

                if (latency <= 0)
                {
                    throw new ArgumentException("The latancy must be a positive value.", nameof(latency));
                }

                if (blockLength < 8)
                {
                    throw new ArgumentException("The block length must be greater than or equal to 8.", nameof(blockLength));
                }

                this.device = device;
                this.sampleRate = sampleRate;
                this.channelCount = channelCount;
                this.latency = latency;
                this.blockLength = blockLength;

                var bufferCount = (int)Math.Ceiling((double)(sampleRate * latency) / (1000 * blockLength));
                if (bufferCount < 2)
                {
                    bufferCount = 2;
                }

                alBuffers = new uint[bufferCount];
                for (var i = 0; i < alBuffers.Length; i++)
                {
                    alBuffers[i] = device.AL.GenBuffer();
                    if (device.AL.GetError() != AudioError.NoError)
                    {
                        throw new Exception("Failed to generate an audio buffer.");
                    }
                }

                format = channelCount == 1 ? BufferFormat.Mono16 : BufferFormat.Stereo16;

                alSource = device.AL.GenSource();
                if (device.AL.GetError() != AudioError.NoError)
                {
                    throw new Exception("Failed to generate an audio source.");
                }

                volume = 1F;
                device.AL.SetSourceProperty(alSource, SourceFloat.Gain, volume);

                pitch = 1F;
                device.AL.SetSourceProperty(alSource, SourceFloat.Pitch, pitch);

                position = device.ListernerPosition - device.ListernerDirection;
                device.AL.SetSourceProperty(alSource, SourceVector3.Position, position);

                blockData = new short[channelCount * blockLength];
                alBufferQueue = new uint[1];

                device.AddResource(this);
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioStream"/> class.
        /// </summary>
        /// <param name="device">The <see cref="AudioDevice"/> to play the audio stream.</param>
        /// <param name="sampleRate">The sample rate of the audio stream.</param>
        /// <param name="channelCount">The number of channels of the audio stream. This value must be 1 or 2.</param>
        public AudioStream(AudioDevice device, int sampleRate, int channelCount)
            : this(device, sampleRate, channelCount, defaultLatency, defaultBlockLength)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioStream"/> class.
        /// </summary>
        /// <param name="device">The <see cref="AudioDevice"/> to play the audio stream.</param>
        /// <param name="sampleRate">The sample rate of the audio stream.</param>
        /// <param name="channelCount">The number of channels of the audio stream. This value must be 1 or 2.</param>
        /// <param name="latency">The desired latency for audio processing in milliseconds.</param>
        public AudioStream(AudioDevice device, int sampleRate, int channelCount, int latency)
            : this(device, sampleRate, channelCount, latency, defaultBlockLength)
        {
        }

        /// <summary>
        /// Disposes the resources held by the <see cref="AudioStream"/>.
        /// </summary>
        public void Dispose()
        {
            if (device == null)
            {
                return;
            }

            if (pollingTask != null)
            {
                pollingCts.Cancel();
                pollingTask.Wait();
                pollingCts = null;
                pollingTask = null;
            }

            if (alSource != 0)
            {
                device.AL.SourceStop(alSource);
                device.AL.DeleteSource(alSource);
                alSource = 0;
            }

            if (alBuffers != null)
            {
                for (var i = 0; i < alBuffers.Length; i++)
                {
                    if (alBuffers[i] != 0)
                    {
                        device.AL.DeleteBuffer(alBuffers[i]);
                        alBuffers[i] = 0;
                    }
                }
            }

            device.RemoveResource(this);
            device = null;
        }

        /// <summary>
        /// Plays a sound from the wave data generated by the specified callback function.
        /// </summary>
        /// <param name="fillBlock">The callback function to generate the wave data.</param>
        public void Play(Action<short[]> fillBlock)
        {
            if (device == null)
            {
                throw new ObjectDisposedException(nameof(AudioStream));
            }

            if (fillBlock == null)
            {
                throw new ArgumentNullException(nameof(fillBlock));
            }

            // If the previous playback is still ongoing, we have to stop it.
            if (pollingTask != null)
            {
                pollingCts.Cancel();
                pollingTask.Wait();
            }

            this.fillBlock = fillBlock;

            for (var i = 0; i < alBuffers.Length; i++)
            {
                fillBlock(blockData);
                device.AL.BufferData(alBuffers[i], format, blockData, sampleRate);
                alBufferQueue[0] = alBuffers[i];
                device.AL.SourceQueueBuffers(alSource, alBufferQueue);
            }

            device.AL.SourcePlay(alSource);

            pollingCts = new CancellationTokenSource();
            pollingTask = Task.Run(() => PollingLoop(pollingCts.Token));
        }

        /// <summary>
        /// Stops playing sound.
        /// </summary>
        public void Stop()
        {
            if (device == null)
            {
                throw new ObjectDisposedException(nameof(AudioStream));
            }

            if (pollingTask != null)
            {
                pollingCts.Cancel();
            }
        }

        private void PollingLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                int processedCount;
                device.AL.GetSourceProperty(alSource, GetSourceInteger.BuffersProcessed, out processedCount);
                for (var i = 0; i < processedCount; i++)
                {
                    fillBlock(blockData);
                    device.AL.SourceUnqueueBuffers(alSource, alBufferQueue);
                    device.AL.BufferData(alBufferQueue[0], format, blockData, sampleRate);
                    device.AL.SourceQueueBuffers(alSource, alBufferQueue);
                }

                int value;
                device.AL.GetSourceProperty(alSource, GetSourceInteger.SourceState, out value);
                if (value == (int)SourceState.Stopped)
                {
                    device.AL.SourcePlay(alSource);
                }

                Thread.Sleep(1);
            }

            device.AL.SourceStop(alSource);

            {
                // We have to unqueue remaining buffers for next playback.
                int processedCount;
                device.AL.GetSourceProperty(alSource, GetSourceInteger.BuffersProcessed, out processedCount);
                for (var i = 0; i < processedCount; i++)
                {
                    device.AL.SourceUnqueueBuffers(alSource, alBufferQueue);
                }
            }
        }

        /// <summary>
        /// Gets the sample rate of the audio stream.
        /// </summary>
        public int SampleRate => sampleRate;

        /// <summary>
        /// Gets the number of channels of the audio stream.
        /// </summary>
        public int ChannelCount => channelCount;

        /// <summary>
        /// Gets the latency for audio processing in milliseconds.
        /// </summary>
        public int Latency => latency;

        /// <summary>
        /// Gets the block length for audio processing in sample frames.
        /// </summary>
        public int BlockLength => blockLength;

        /// <summary>
        /// Gets or sets the volume.
        /// The value must be between 0 and 1.
        /// </summary>
        public float Volume
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(AudioStream));
                }

                return volume;
            }

            set
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(AudioStream));
                }

                volume = value;
                device.AL.SetSourceProperty(alSource, SourceFloat.Gain, volume);
            }
        }

        /// <summary>
        /// Gets or sets the pitch.
        /// The playback frequency will be multiplied by this value.
        /// </summary>
        public float Pitch
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(AudioStream));
                }

                return pitch;
            }

            set
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(AudioStream));
                }

                pitch = value;
                device.AL.SetSourceProperty(alSource, SourceFloat.Pitch, pitch);
            }
        }

        /// <summary>
        /// Gets or sets the position of the sound source.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(AudioStream));
                }

                return position;
            }

            set
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(AudioStream));
                }

                position = value;
                device.AL.SetSourceProperty(alSource, SourceVector3.Position, position);
            }
        }

        /// <summary>
        /// Gets the current playback state of the channel.
        /// </summary>
        public PlaybackState State
        {
            get
            {
                if (device == null)
                {
                    throw new ObjectDisposedException(nameof(AudioStream));
                }

                int value;
                device.AL.GetSourceProperty(alSource, GetSourceInteger.SourceState, out value);

                switch ((SourceState)value)
                {
                    case SourceState.Initial:
                    case SourceState.Stopped:
                        return PlaybackState.Stopped;

                    case SourceState.Playing:
                        return PlaybackState.Playing;

                    case SourceState.Paused:
                        return PlaybackState.Paused;

                    default:
                        throw new Exception();
                }
            }
        }
    }
}
