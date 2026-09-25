using System;

namespace MirrorNetTest.Services
{
    public abstract class ServiceBase : IDisposable
    {
        public abstract void Init();

        public virtual void Dispose()
        {
            
        }
    }
}