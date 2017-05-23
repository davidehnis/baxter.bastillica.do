using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Baxter.Agents.Automaton
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name
    //       "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc
    //       or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Automaton : IAutomaton
    {
        private IScheduler _scheduler;

        public string GetData(int value)
        {
            return $"You entered: {value}";
        }
        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
        public void Post(IAutomatonObserver observer,
            string script, IEnumerable<object> data)
        {
            Scheduler().AddTask(Job.Create(observer, script, data), Schedule.Once());
        }

        public void PostFuture(IAutomatonObserver observer, string script, IEnumerable<object> data, DateTime runAt)
        {
            Scheduler().AddTask(Job.Create(observer, script, data), Schedule.Once());
        }

        protected IScheduler Scheduler()
        {
            return _scheduler ?? (_scheduler = new Scheduler(1));
        }
    }
}