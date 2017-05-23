using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Agents.Automaton
{
    internal interface IScheduledRunner
    {
        IScheduled Task { get; }

        void Check();
    }
}