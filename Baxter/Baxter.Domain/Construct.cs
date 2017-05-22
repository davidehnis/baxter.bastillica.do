using System.Diagnostics;
using System.Runtime.CompilerServices;
using System;

namespace Baxter.Domain
{
    //<summary>The most basic Domain class</summary>
    public abstract class Construct
    {
        #region Public Properties
        //<summary>The appropriate type value</summary>
        public virtual Type Type { get; set; }
        #endregion Public Properties

        #region Constructors
        //<summary>Must at least have the type set</summary>
        public Construct(Type type)
        {
            Type = type;

            Initialize();
        }
        #endregion Constructors

        #region Public Methods
        //<summary>Used by the boundary (context) to determine the "hop" count so that the system can learn/adjust effeciency</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        //<summary>Returns the unique "hash" code for everything in the object graph</summary>
        public override int GetHashCode()
        {
            return string.Format
                ("{0}", Type.GetHashCode()).GetHashCode();
        }

        //<summary>A new ToString hierarchy starts at the Construct level because everything in the framework is serialized to JSON</summary>
        public new virtual string ToString()
        {
            return Json.Serialize(this);
        }
        #endregion Public Methods

        #region Protected Methods
        protected virtual void Initialize()
        {
        }
        #endregion Protected Methods
    }
}