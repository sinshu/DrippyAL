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
        private ALContext? alc;
        private AL? al;

        private Device* device;
        private Context* context;

        private Vector3 listenerPosition;
        private float[] listenerOrientation;

        private List<AudioChannel>? channels;
        private List<AudioClip>? audioClips;
        private List<AudioStream>? audioStreams;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevice"/> class.
        /// </summary>
        /// <param name="useSoft">If true, OpenAL Soft will be used for audio processing.</param>
        public AudioDevice(bool useSoft)
        {
            try
            {
                alc = ALContext.GetApi(useSoft);
                al = AL.GetApi(useSoft);

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

                channels = new List<AudioChannel>();
                audioClips = new List<AudioClip>();
                audioStreams = new List<AudioStream>();
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioDevice"/> class.
        /// </summary>
        public AudioDevice() : this(true)
        {
        }

        /// <summary>
        /// Disposes the resources held by the <see cref="AudioDevice"/>.
        /// </summary>
        public void Dispose()
        {
            if (audioStreams != null)
            {
                if (audioStreams.Count > 0)
                {
                    var copy = audioStreams.ToArray();
                    foreach (var audioStream in copy)
                    {
                        audioStream.Dispose();
                    }
                }

                audioStreams = null;
            }

            if (audioClips != null)
            {
                if (audioClips.Count > 0)
                {
                    var copy = audioClips.ToArray();
                    foreach (var audioClip in copy)
                    {
                        audioClip.Dispose();
                    }
                }

                audioClips = null;
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

        internal void AddResource(AudioChannel channel)
        {
            channels!.Add(channel);
        }

        internal void RemoveResource(AudioChannel channel)
        {
            channels!.Remove(channel);
        }

        internal void AddResource(AudioClip audioClip)
        {
            audioClips!.Add(audioClip);
        }

        internal void RemoveResource(AudioClip audioClip)
        {
            // To dispose the audio clip, we must ensure that no channel is using the audio clip.
            // We therefore detach the audio clip from all the channels using it.
            foreach (var channel in channels!)
            {
                if (channel.AudioClip == audioClip)
                {
                    // Set the AudioClip property to null to detach the audio clip.
                    channel.AudioClip = null;
                }
            }

            audioClips!.Remove(audioClip);
        }

        internal void AddResource(AudioStream audioStream)
        {
            audioStreams!.Add(audioStream);
        }

        internal void RemoveResource(AudioStream audioStream)
        {
            audioStreams!.Remove(audioStream);
        }

        internal AL AL
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(AudioDevice));
                }

                return al;
            }
        }

        // For tests.
        internal IReadOnlyList<AudioChannel> Channels => channels!;
        internal IReadOnlyList<AudioClip> AudioClips => audioClips!;
        internal IReadOnlyList<AudioStream> AudioStreams => audioStreams!;

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
