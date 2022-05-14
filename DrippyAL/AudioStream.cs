using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    public unsafe sealed class AudioStream : IDisposable
    {
        private AL? al;

        private int sampleRate;
        private int channelCount;

        private int latency;
        private int blockLength;

        private uint alSource;
        private uint[] alBuffers;
        private BufferFormat format;

        private short[] blockData;
        private uint[] alBufferQueue;

        private volatile bool stopped;
        private Action<short[]>? fillBlock;
        private Task? pollingTask;

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

                if (!(channelCount == 1 || channelCount == 2))
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

                al = device.AL;

                this.sampleRate = sampleRate;
                this.channelCount = channelCount;

                this.latency = latency;
                this.blockLength = blockLength;

                var bufferCount = (int)Math.Ceiling((double)(sampleRate * latency) / (1000 * blockLength));
                if (bufferCount < 2)
                {
                    bufferCount = 2;
                }

                alSource = al.GenSource();
                if (al.GetError() != AudioError.NoError)
                {
                    throw new Exception("Failed to generate an audio source.");
                }

                alBuffers = new uint[bufferCount];
                for (var i = 0; i < alBuffers.Length; i++)
                {
                    alBuffers[i] = al.GenBuffer();
                    if (al.GetError() != AudioError.NoError)
                    {
                        throw new Exception("Failed to generate an audio buffer.");
                    }
                }

                format = channelCount == 1 ? BufferFormat.Mono16 : BufferFormat.Stereo16;

                blockData = new short[channelCount * blockLength];
                alBufferQueue = new uint[1];
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        public AudioStream(AudioDevice device, int sampleRate, int channelCount)
            : this(device, sampleRate, channelCount, 200, 1024)
        {
        }

        public AudioStream(AudioDevice device, int sampleRate, int channelCount, int latency)
            : this(device, sampleRate, channelCount, latency, 1024)
        {
        }

        public void Dispose()
        {
            if (al == null)
            {
                return;
            }

            if (pollingTask != null)
            {
                stopped = true;
                pollingTask.Wait();
            }

            if (alSource != 0)
            {
                al.SourceStop(alSource);
                al.DeleteSource(alSource);
                alSource = 0;
            }

            if (alBuffers != null)
            {
                for (var i = 0; i < alBuffers.Length; i++)
                {
                    if (alBuffers[i] != 0)
                    {
                        al.DeleteBuffer(alBuffers[i]);
                        alBuffers[i] = 0;
                    }
                }
            }

            al = null;
        }

        public void Play(Action<short[]> fillBlock)
        {
            if (al == null)
            {
                throw new Exception();
            }

            if (fillBlock == null)
            {
                throw new ArgumentNullException(nameof(fillBlock));
            }

            if (pollingTask != null)
            {
                stopped = true;
                pollingTask.Wait();
            }

            stopped = false;

            this.fillBlock = fillBlock;

            for (var i = 0; i < alBuffers.Length; i++)
            {
                fillBlock(blockData);
                al.BufferData(alBuffers[i], format, blockData, sampleRate);
                alBufferQueue[0] = alBuffers[i];
                al.SourceQueueBuffers(alSource, alBufferQueue);
            }

            al.SourcePlay(alSource);

            pollingTask = Task.Run(PollingLoop);
        }

        public void Stop()
        {
            stopped = true;
        }

        private void PollingLoop()
        {
            // This is to avoid nullable warning.
            if (al == null || fillBlock == null)
            {
                throw new Exception();
            }

            while (!stopped)
            {
                int processedCount;
                al.GetSourceProperty(alSource, GetSourceInteger.BuffersProcessed, out processedCount);
                for (var i = 0; i < processedCount; i++)
                {
                    fillBlock(blockData);
                    al.SourceUnqueueBuffers(alSource, alBufferQueue);
                    al.BufferData(alBufferQueue[0], format, blockData, sampleRate);
                    al.SourceQueueBuffers(alSource, alBufferQueue);
                }

                int value;
                al.GetSourceProperty(alSource, GetSourceInteger.SourceState, out value);
                if (value == (int)SourceState.Stopped)
                {
                    al.SourcePlay(alSource);
                }

                Thread.Sleep(1);
            }

            al.SourceStop(alSource);

            {
                // We have to unqueue remaining buffers for next playback.
                int processedCount;
                al.GetSourceProperty(alSource, GetSourceInteger.BuffersProcessed, out processedCount);
                for (var i = 0; i < processedCount; i++)
                {
                    al.SourceUnqueueBuffers(alSource, alBufferQueue);
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the sound is playing.
        /// </summary>
        public PlaybackState State
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                int value;
                al.GetSourceProperty(alSource, GetSourceInteger.SourceState, out value);

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
