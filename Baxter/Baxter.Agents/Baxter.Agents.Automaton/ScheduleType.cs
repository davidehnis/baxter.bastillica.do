using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Baxter.Agents.Automaton
{
    public enum ScheduleType
    {
        /// <summary>
        /// Indicates a Task should be consecutively run after a predefined number of seconds
        /// </summary>
        Periodical,

        /// <summary>Indicates a task should be run at a pre-defined time every day</summary>
        Scheduled,

        /// <summary>Indicates a task is responsible for managing its own scheduled execution</summary>
        Task,

        Once
    }
}