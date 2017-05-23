using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Baxter.Agents.Automaton
{
    public abstract class Advice : IAdvice
    {
        #region Public Properties
        public bool Dependent { get; protected set; }
        #endregion Public Properties

        #region Public Properties
        public Guid Id { get; protected set; }

        public string Input { get; protected set; }

        public String Name { get; protected set; }

        public IAdvice Predecessor { get; protected set; }

        public String Result { get; protected set; }

        public String Script { get; protected set; }
        #endregion Public Properties

        #region Public Methods
        public virtual void Execute(String input = null)
        {
            if (Predecessor != null)
            {
                Predecessor.Execute(input);
            }

            Execute();
        }

        /// <summary>Sets the necessary predecessor</summary>
        public virtual void Needs(IAdvice advice)
        {
            Predecessor = advice;
        }
        #endregion Public Methods
    }
}