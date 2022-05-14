using System;
using System.Numerics;
using System.Runtime.ExceptionServices;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    public unsafe sealed class Channel : IDisposable
    {
        private AL? al;

        private uint alSource;

        private float volume;
        private float pitch;
        private Vector3 position;

        private WaveData? waveData;

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

            this.waveData = waveData;

            al.SetSourceProperty(alSource, SourceInteger.Buffer, waveData.AlBuffer);
            al.SourcePlay(alSource);
        }

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

            waveData = null;
        }

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

        public void Resume()
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

        internal uint AlSource
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                return alSource;
            }
        }

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
