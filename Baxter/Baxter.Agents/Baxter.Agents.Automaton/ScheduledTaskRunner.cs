using System;

namespace Baxter.Agents.Automaton
{
    internal class ScheduledTaskRunner : IScheduledRunner
    {
        private readonly TimeSpan _runAt;
        private DateTime _lastRun;

        public ScheduledTaskRunner(IScheduled task, TimeSpan runAt)
        {
            Task = task;
            _runAt = runAt;
        }

        public IScheduled Task { get; private set; }

        public void Check()
        {
            if (_lastRun.Date != DateTime.Now.Date && DateTime.Now.TimeOfDay > _runAt && !Task.IsBusy)
            {
                try
                {
                    Task.Run();
                }
                catch { }
                _lastRun = DateTime.Now;
            }
        }
    }
}