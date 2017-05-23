using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Agents.Automaton
{
    public interface IAdvice
    {
        #region Public Properties
        /// <summary>Determines the need for the predecessor's result</summary>
        bool Dependent { get; }
        Guid Id { get; }

        String Input { get; }
        String Name { get; }

        IAdvice Predecessor { get; }
        String Result { get; }
        String Script { get; }
        #endregion Public Properties

        #region Public Methods
        void Execute(String input = null);

        /// <summary>Sets the necessary predecessor</summary>
        void Needs(IAdvice advice);
        #endregion Public Methods
    }
}