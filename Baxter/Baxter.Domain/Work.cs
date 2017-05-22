using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Baxter.Domain
{
    //<summary>The basic form of work performed in and on data within the system</summary>
    public class Work : DynamicObject
    {
        #region Public Constructors
        public Work(Node owner)
        {
            Owner = owner;
        }
        public Work(Construct construct)
        {
            Owner = construct;
        }
        public Work()
        {
        }
        #endregion Public Constructors

        #region Public Properties
        public Construct Owner { get; protected set; }
        #endregion Public Properties

        #region Public Methods
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            Type dictType = typeof(Dictionary<string, object>);

            try
            {
                var feedback = Owner.Type.InvokeMember(
                             binder.Name,
                             BindingFlags.InvokeMethod, null, Owner, args);

                result = feedback;
                return true;
            }
            catch (Exception ex)
            {
                result = false;
            }

            return true;
        }
        #endregion Public Methods
    }
}