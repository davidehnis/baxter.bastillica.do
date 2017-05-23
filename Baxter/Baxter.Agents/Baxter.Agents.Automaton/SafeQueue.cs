using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Baxter.Agents.Automaton
{
    internal class SafeQueue<T> : Detachable
    {
        private BlockingCollection<T> _internalQueue;

        private Object _messagePumpLock = new Object();
        private Task _pump;

        public SafeQueue()
        {
        }

        protected SafeQueue(IConsumer<T> consumer)
            : base()
        {
            _internalQueue = new BlockingCollection<T>();

            Consumer = consumer;
        }

        public IConsumer<T> Consumer { get; private set; }

        public void Add(T item)
        {
            CheckDispose();

            _internalQueue.Add(item);

            EnsureMessagePumpIsRunning();
        }

        public void CompleteAdding()
        {
            StopPump();
        }

        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                StopPump();

                if (_pump != null)
                {
                    _pump = null;
                }

                _messagePumpLock = null;

                if (_internalQueue != null)
                {
                    _internalQueue.Dispose();
                    _internalQueue = null;
                }

                Consumer = null;
            }

            base.Dispose(disposing);
        }

        private void EnsureMessagePumpIsRunning()
        {
            if (_pump == null)
            {
                lock (_messagePumpLock)
                {
                    if (_pump == null)
                    {
                        StartPump();
                    }
                }
            }
        }

        private void Read()
        {
            foreach (var item in _internalQueue.GetConsumingEnumerable())
            {
                Consumer.Process(item);
            }
        }

        private void StartPump()
        {
            Action action = () =>
            {
                Read();
            };

            _pump = Task.Factory.StartNew(action).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                }
            });
        }

        /// <summary>Stops the message pump.</summary>
        private void StopPump()
        {
            if (_pump != null)
            {
                lock (_pump)
                {
                    _internalQueue.CompleteAdding();
                }
            }
        }
    }
}