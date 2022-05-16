using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.ExceptionServices;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    /// <summary>
    /// Represents an audio device.
    /// </summary>
    public unsafe sealed class AudioDevice : IDisposable
    {
        private ALContext alc;
        private AL al;

        private Device* device;
        private Context* context;

        private Vector3 listenerPosition;
        private float[] listenerOrientation;

        private List<Channel> channels;
        private List<WaveData> waveDatas;
        private List<AudioStream> streams;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevice"/> class.
        /// </summary>
        public AudioDevice()
        {
            try
            {
                alc = ALContext.GetApi(true);
                al = AL.GetApi(true);

                device = alc.OpenDevice("");
                if (device == default(Device*))
                {
                    throw new Exception("Failed to open the audio device.");
                }

                context = alc.CreateContext(device, null);
                if (context == default(Context*))
                {
                    throw new Exception("Failed to open the audio device.");
                }

                if (!alc.MakeContextCurrent(context))
                {
                    throw new Exception("Failed to open the audio device.");
                }

                listenerPosition = new Vector3(0F, 0F, 0F);
                al.SetListenerProperty(ListenerVector3.Position, in listenerPosition);

                listenerOrientation = new float[]
                {
                    0F, 0F, -1F,
                    0F, 1F,  0F
                };
                fixed (float* p = listenerOrientation)
                {
                    al.SetListenerProperty(ListenerFloatArray.Orientation, p);
                }

                channels = new List<Channel>();
                waveDatas = new List<WaveData>();
                streams = new List<AudioStream>();
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        /// <summary>
        /// Disposes the resources held by the <see cref="AudioDevice"/>.
        /// </summary>
        public void Dispose()
        {
            if (streams != null)
            {
                if (streams.Count > 0)
                {
                    var copy = streams.ToArray();
                    foreach (var stream in copy)
                    {
                        stream.Dispose();
                    }
                }

                streams = null;
            }

            if (waveDatas != null)
            {
                if (waveDatas.Count > 0)
                {
                    var copy = waveDatas.ToArray();
                    foreach (var waveData in copy)
                    {
                        waveData.Dispose();
                    }
                }

                waveDatas = null;
            }

            if (channels != null)
            {
                if (channels.Count > 0)
                {
                    var copy = channels.ToArray();
                    foreach (var channel in copy)
                    {
                        channel.Dispose();
                    }
                }

                channels = null;
            }

            if (alc != null)
            {
                if (context != default(Context*))
                {
                    alc.DestroyContext(context);
                    context = default(Context*);
                }

                if (device != default(Device*))
                {
                    alc.CloseDevice(device);
                    device = default(Device*);
                }
            }

            if (al != null)
            {
                al.Dispose();
                al = null;
            }

            if (alc != null)
            {
                alc.Dispose();
                alc = null;
            }
        }

        internal void AddResource(Channel channel)
        {
            channels.Add(channel);
        }

        internal void RemoveResource(Channel channel)
        {
            channels.Remove(channel);
        }

        internal void AddResource(WaveData waveData)
        {
            waveDatas.Add(waveData);
        }

        internal void RemoveResource(WaveData waveData)
        {
            // To dispose the wave data, we must ensure that no channel is using the wave data.
            // We therefore detach the wave data from all the channels using the wave data.
            foreach (var channel in channels!)
            {
                if (channel.WaveData == waveData)
                {
                    // Set the WaveData property to null to detach the wave data.
                    channel.WaveData = null;
                }
            }

            waveDatas.Remove(waveData);
        }

        internal void AddResource(AudioStream stream)
        {
            streams.Add(stream);
        }

        internal void RemoveResource(AudioStream stream)
        {
            streams.Remove(stream);
        }

        internal AL AL => al;
        internal IReadOnlyList<Channel> Channels => channels;
        internal IReadOnlyList<WaveData> WaveDatas => waveDatas;
        internal IReadOnlyList<AudioStream> Streams => streams;

        /// <summary>
        /// Gets or sets the position of the listener.
        /// </summary>
        public Vector3 ListernerPosition
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(AudioDevice));
                }

                return listenerPosition;
            }

            set
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(AudioDevice));
                }

                listenerPosition = value;
                al.SetListenerProperty(ListenerVector3.Position, listenerPosition);
            }
        }

        /// <summary>
        /// Gets or sets the direction of the listener.
        /// </summary>
        public Vector3 ListernerDirection
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(AudioDevice));
                }

                return new Vector3(
                    listenerOrientation[0],
                    listenerOrientation[1],
                    listenerOrientation[2]);
            }

            set
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(AudioDevice));
                }

                listenerOrientation[0] = value.X;
                listenerOrientation[1] = value.Y;
                listenerOrientation[2] = value.Z;
                fixed (float* p = listenerOrientation)
                {
                    al.SetListenerProperty(ListenerFloatArray.Orientation, p);
                }
            }
        }

        /// <summary>
        /// Gets or sets the up vector of the listener.
        /// </summary>
        public Vector3 ListernerUpVector
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(AudioDevice));
                }

                return new Vector3(
                    listenerOrientation[3],
                    listenerOrientation[4],
                    listenerOrientation[5]);
            }

            set
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(AudioDevice));
                }

                listenerOrientation[3] = value.X;
                listenerOrientation[4] = value.Y;
                listenerOrientation[5] = value.Z;
                fixed (float* p = listenerOrientation)
                {
                    al.SetListenerProperty(ListenerFloatArray.Orientation, p);
                }
            }
        }
    }
}
