using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Baxter.Agents.Automaton
{
    public class Proxy
    {
        public void Add(Job job)
        {
            Singleton<SafeQueue<Job>>.Instance.Add(job);
        }
    }
}