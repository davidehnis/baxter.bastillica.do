using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Agents.Automaton.Tests
{
    internal class TestObserver : IAutomatonObserver
    {
        public Action<object> Callback { set; get; }

        public void Notify(object ev)
        {
            throw new NotImplementedException();
        }

        public void Send(object data)
        {
            Callback(data);
        }
    }
}