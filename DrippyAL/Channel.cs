using System;
using System.Numerics;
using System.Runtime.ExceptionServices;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    /// <summary>
    /// Represents an audio channel to play wave data.
    /// </summary>
    public unsafe sealed class Channel : IDisposable
    {
        private AL? al;

        private uint alSource;

        private float volume;
        private float pitch;
        private Vector3 position;

        private WaveData? waveData;

        /// <summary>
        /// Creates a new audio channel.
        /// </summary>
        /// <param name="device">The <see cref="AudioDevice"/> for which the new channel is to be created.</param>
        public Channel(AudioDevice device)
        {
            try
            {
                if (device == null)
                {
                    throw new ArgumentNullException(nameof(device));
                }

                al = device.AL;

                alSource = al.GenSource();
                if (al.GetError() != AudioError.NoError)
                {
                    throw new Exception("Failed to generate an audio source.");
                }

                volume = 1F;
                al.SetSourceProperty(alSource, SourceFloat.Gain, volume);

                pitch = 1F;
                al.SetSourceProperty(alSource, SourceFloat.Pitch, pitch);

                position = device.ListernerPosition - device.ListernerDirection;
                al.SetSourceProperty(alSource, SourceVector3.Position, position);
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        /// <summary>
        /// Disposes the resources held by the <see cref="Channel"/>.
        /// </summary>
        public void Dispose()
        {
            if (al == null)
            {
                return;
            }

            if (alSource != 0)
            {
                al.SourceStop(alSource);
                al.DeleteSource(alSource);
                alSource = 0;
            }

            al = null;
        }

        /// <summary>
        /// Plays the wave data which is currently attached to the channel.
        /// </summary>
        public void Play()
        {
            if (al == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (waveData == null)
            {
                return;
            }

            al.SourcePlay(alSource);
        }

        /// <summary>
        /// Plays the specified wave data.
        /// </summary>
        /// <param name="waveData">The wave data to be played.</param>
        public void Play(WaveData waveData)
        {
            if (al == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (waveData == null)
            {
                throw new ArgumentNullException(nameof(waveData));
            }

            WaveData = waveData;

            al.SourcePlay(alSource);
        }

        /// <summary>
        /// Stops playing sound.
        /// </summary>
        public void Stop()
        {
            if (al == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (waveData == null)
            {
                return;
            }

            al.SourceStop(alSource);
        }

        /// <summary>
        /// Pauses playing sound.
        /// </summary>
        public void Pause()
        {
            if (al == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (waveData == null)
            {
                return;
            }

            al.SourcePause(alSource);
        }

        /// <summary>
        /// Resets the playback state.
        /// </summary>
        public void Rewind()
        {
            if (al == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            if (waveData == null)
            {
                return;
            }

            al.SourceRewind(alSource);
        }

        /// <summary>
        /// Gets or sets the <see cref="DrippyAL.WaveData"/> to be played.
        /// Set this property to null to detach the wave data from the channel so that the wave data can be safely disposed.
        /// </summary>
        public WaveData? WaveData
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                return waveData;
            }

            set
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                al.SourceStop(alSource);

                if (value == null)
                {
                    waveData = null;
                    al.SetSourceProperty(alSource, SourceInteger.Buffer, 0);
                }
                else
                {
                    waveData = value;
                    al.SetSourceProperty(alSource, SourceInteger.Buffer, waveData.AlBuffer);
                }
            }
        }

        /// <summary>
        /// Gets or sets the volume.
        /// The value must be between 0 and 1.
        /// </summary>
        public float Volume
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                return volume;
            }

            set
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                volume = value;
                al.SetSourceProperty(alSource, SourceFloat.Gain, volume);
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
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                return pitch;
            }

            set
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                pitch = value;
                al.SetSourceProperty(alSource, SourceFloat.Pitch, pitch);
            }
        }

        /// <summary>
        /// Gets or sets the position of the sound source.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                return position;
            }

            set
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                position = value;
                al.SetSourceProperty(alSource, SourceVector3.Position, position);
            }
        }

        /// <summary>
        /// Gets the current playback state of the channel.
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

        /// <summary>
        /// Gets the current playing offset of the wave data.
        /// </summary>
        public TimeSpan PlayingOffset
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                float value;
                al.GetSourceProperty(alSource, SourceFloat.SecOffset, out value);

                return TimeSpan.FromSeconds(value);
            }
        }
    }
}
