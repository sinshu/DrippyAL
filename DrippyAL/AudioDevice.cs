using System;
using System.Runtime.ExceptionServices;
using Silk.NET.OpenAL;

namespace DrippyAL
{
    public unsafe sealed class AudioDevice : IDisposable
    {
        private ALContext? alc;
        private AL? al;

        private Device* device;
        private Context* context;

        public AudioDevice()
        {
            try
            {
                alc = ALContext.GetApi(true);
                al = AL.GetApi(true);

                device = alc.OpenDevice("");
                context = alc.CreateContext(device, null);

                alc.MakeContextCurrent(context);
                al.GetError();
            }
            catch (Exception e)
            {
                Dispose();
                ExceptionDispatchInfo.Throw(e);
            }
        }

        public void Dispose()
        {
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
    }
}
