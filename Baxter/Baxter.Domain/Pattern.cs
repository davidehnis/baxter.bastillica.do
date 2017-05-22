using System;

namespace Baxter.Domain
{
    //<summary>The most basic 'recognizer' for this system.  A pattern is used to find meaning in data.</summary>
    public class Pattern
    {
        #region Public Constructors
        public Pattern()
        {
        }
        #endregion Public Constructors

        #region Public Properties

        public Type Type { get; protected set; }
        #endregion Public Properties
    }
}