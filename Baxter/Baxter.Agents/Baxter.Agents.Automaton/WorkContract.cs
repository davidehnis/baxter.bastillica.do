using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Baxter.Agents.Automaton
{
    [DataContract]
    /// <summary>A proxy class for Job</summary>
    public class WorkContract
    {
        #region Private Fields
        private Stack<IAdvice> _advice = new Stack<IAdvice>();
        #endregion Private Fields

        #region Public Constructors
        public WorkContract()
        {
        }
        #endregion Public Constructors

        #region Public Methods
        public void Add(IAdvice advice)
        {
            _advice.Push(advice);
        }
        #endregion Public Methods

        #region Public Properties
        [DataMember]
        public IEnumerable<IAdvice> Advice
        {
            get
            {
                return _advice;
            }
        }
        [DataMember]
        /// <summary>The date and time that this job was last updated</summary>
        public DateTime Changed { get; set; }

        [DataMember]
        /// <summary>The date and time that this job was created</summary>
        public DateTime Created { get; set; }
        [DataMember]
        public Guid Id { get; private set; }

        [DataMember]
        public ResultContract Result { get; private set; }

        [DataMember]
        public string Script { get; private set; }

        [DataMember]
        public Status Status { get; private set; }
        #endregion Public Properties
    }
}