using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Agents.Automaton
{
    public interface IScheduler
    {
        int Frequency { get; }

        void AddTask(IScheduled task, Schedule schedule);
        void Start();
        void Stop();
    }
}