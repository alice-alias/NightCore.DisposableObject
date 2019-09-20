using NightCore.Diagnostics;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace NightCore
{
    public class DisposableObject : IDisposable
    {
        int disposed;
        public bool IsDisposed => disposed != 0;
        readonly GCHandle handle;

        public DisposableObject()
        {
            handle = GCHandle.Alloc(this, GCHandleType.Weak);
            LeakChecker.Construct(handle, GetType().FullName);
        }

        protected virtual void Dispose(bool disposing) { }

        public event EventHandler Disposed;

        protected virtual void OnDisposed() => Disposed?.Invoke(this, EventArgs.Empty);

        ~DisposableObject()
        {
            Dispose(false);
            LeakChecker.NotifyLeak(handle);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) == 0)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
                OnDisposed();
                LeakChecker.Destruct(handle);
            }
        }
    }
}
