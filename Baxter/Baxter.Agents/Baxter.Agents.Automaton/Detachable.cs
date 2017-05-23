using System;

namespace Baxter.Agents.Automaton
{
    [Serializable()]
    public abstract class Detachable : IDisposable
    {
        [NonSerialized()]
        private volatile bool _disposed;

        protected Detachable()
            : base()
        {
        }

        ~Detachable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected internal void CheckDispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(Boolean disposing)
        {
            _disposed = true;
        }
    }
}