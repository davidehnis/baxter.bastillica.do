using System;
using System.Collections.Generic;

namespace Baxter.Agents.Automaton
{
    public class Job : IScheduled
    {
        internal Job(Guid id)
        {
            Id = id;
        }

        internal Job()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>The date and time that this job was last updated</summary>
        public DateTime Changed { get; private set; }

        /// <summary>The date and time that this job was created</summary>
        public DateTime Created { get; private set; }

        public IEnumerable<object> Data { get; private set; }

        public Guid Id { get; private set; }

        public bool IsBusy { get; private set; }

        public JobType JobType { get; private set; }

        public IAutomatonObserver Observer { get; private set; }

        public ResultContract Result { get; private set; }

        public Status Status { get; private set; }

        public Script Work { get; protected set; }

        public Boolean Complete(ResultContract result)
        {
            if ((Status == Status.Running) || (Status == Status.Suspended))
            {
                Result = result;
                ChangeStatusTo(result.Errors ? Status.CompletedWithErrors : Status.Completed);
                Observer.Send(Data);
                IsBusy = false;
                return true;
            }

            return false;
        }

        public Boolean Resume()
        {
            if (Status == Status.Suspended)
            {
                ChangeStatusTo(Status.Running);

                return true;
            }

            return false;
        }

        public void Run()
        {
            IsBusy = Start();
        }

        public Boolean Start()
        {
            if (Status == Status.New)
            {
                ChangeStatusTo(Status.Running);

                return true;
            }

            return false;
        }

        public Boolean Suspend()
        {
            if (Status == Status.Running)
            {
                ChangeStatusTo(Status.Suspended);

                return true;
            }

            return false;
        }

        internal static Job Create(IAutomatonObserver observer, string code, IEnumerable<object> data)
        {
            var now = DateTime.UtcNow;
            var script = Scripter.CreateScript(code);

            var job = new Job()
            {
                Created = now,
                Id = Guid.NewGuid(),
                Changed = now,
                Status = Status.New,
                Data = data,
                Work = script,
                Observer = observer
            };

            return job;
        }

        private void ChangeStatusTo(Status status)
        {
            Changed = DateTime.UtcNow;
            Status = status;
        }
    }
}