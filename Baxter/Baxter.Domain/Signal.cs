using System;

namespace Baxter.Domain
{
    //<summary>The most basic 'message' of the system containing data pertinent for the context</summary>
    public class Signal
    {
        #region Public Constructors
        public Signal()
        {
        }

        public Signal(Type type) : this()
        {
            Type = type;
        }

        public Signal(Type type, Value value) : this(type)
        {
            Value = value;
        }

        public Signal(string msg) : this(msg.GetType(), new Value(msg))
        {
        }
        #endregion Public Constructors

        #region Public Properties
        public Type Type { get; set; }

        public virtual Value Value { get; set; }
        #endregion Public Properties
    }
}