using System;
using System.Runtime.ExceptionServices;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    public unsafe sealed class Channel : IDisposable
    {
        private AL? al;

        private uint alSource;

        public Channel(AudioDevice device)
        {
            try
            {
                al = device.AL;

                alSource = al.GenSource();
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        public void Dispose()
        {
            if (al != null)
            {
                if (alSource != 0)
                {
                    al.DeleteSource(alSource);
                    alSource = 0;
                }

                al = null;
            }
        }

        public void Play(WaveData data)
        {
            if (al == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            al.SetSourceProperty(alSource, SourceInteger.Buffer, data.AlBuffer);
            al.SourcePlay(alSource);
        }

        public void Stop()
        {
            if (al == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            al.SourceStop(alSource);
        }

        public void Pause()
        {
            if (al == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
            }

            al.SourcePause(alSource);
        }

        public void Resume()
        {
            if (al == null)
            {
                throw new ObjectDisposedException(nameof(Channel));
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

        public ChannelState Status
        {
            get
            {
                if (al == null)
                {
                    throw new ObjectDisposedException(nameof(Channel));
                }

                int state;
                al.GetSourceProperty(alSource, GetSourceInteger.SourceState, out state);

                switch ((SourceState)state)
                {
                    case SourceState.Initial:
                    case SourceState.Stopped:
                        return ChannelState.Stopped;

                    case SourceState.Playing:
                        return ChannelState.Playing;

                    case SourceState.Paused:
                        return ChannelState.Paused;

                    default:
                        throw new Exception();
                }
            }
        }
    }
}
