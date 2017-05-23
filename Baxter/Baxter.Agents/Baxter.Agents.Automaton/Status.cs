using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Baxter.Agents.Automaton
{
    public enum Status
    {
        Completed = 3,

        CompletedWithErrors = 4,

        New = 0,

        Running = 1,

        Suspended = 2
    }
}