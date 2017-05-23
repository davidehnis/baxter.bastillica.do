using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Baxter.Agents.Automaton
{
    internal class PeriodicalTaskRunner : IScheduledRunner
    {
        private readonly int _frequency;
        private DateTime _lastRun;
        private DateTime _nextRun;

        public PeriodicalTaskRunner(IScheduled task, int frequency) : this(task, frequency, DateTime.Now)
        {
        }
        public PeriodicalTaskRunner(IScheduled task, int frequency, DateTime lastRun)
        {
            Task = task;
            _frequency = frequency;
            _lastRun = lastRun;
            _nextRun = _lastRun.AddSeconds(_frequency);
        }

        public IScheduled Task { get; private set; }

        public void Check()
        {
            if (DateTime.Now > _nextRun && !Task.IsBusy)
            {
                try
                {
                    Task.Run();
                    _lastRun = _nextRun;
                }
                catch (Exception ex)
                {
                    // this is just here so i can set breakpoints, please be kinda to me compiler and
                    // remove this
                    throw;
                }
                _nextRun = _nextRun.AddSeconds(_frequency);
            }
        }
    }
}