using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Baxter.Agents.Automaton
{
    public class Script
    {
        #region Public Constructors
        public Script(Guid id, String name, String code, DateTime created)
        {
            Id = id;
            Name = name;
            Code = code;
            Created = created;
        }
        #endregion Public Constructors

        #region Public Properties
        /// <summary>The date and time that this job was last updated</summary>
        public DateTime Changed { get; protected set; }

        //<summary>The source code of this script</summary>
        public String Code { get; protected set; }

        //<summary>The associated code compiles without errors</summary>
        public bool Compiles { get; protected set; }

        /// <summary>The date and time that this job was created</summary>
        public DateTime Created { get; protected set; }

        public Guid Id { get; protected set; }

        public String Name { get; protected set; }

        public bool Valid { get; protected set; }
        #endregion Public Properties

        #region Protected Properties
        //<summary>True if </summary>
        protected bool Modified { get; set; }
        #endregion Protected Properties

        #region Public Methods
        //<summary>Use a delegate to check compile</summary>
        public ImmutableArray<Diagnostic> Compile(Func<ImmutableArray<Diagnostic>> compile)
        {
            var results = compile();
            Compiles = !compile().Any();

            Modified = true;
            Changed = DateTime.UtcNow;
            return results;
        }

        public void Compile(Func<bool> compile)
        {
            Compiles = compile();
            Modified = true;
            Changed = DateTime.UtcNow;
        }
        #endregion Public Methods

        #region Protected Methods
        protected virtual bool Validate()
        {
            var valid = Code != null &&
                        Code != string.Empty &&
                        Id != Guid.Empty &&
                        Created != null;

            return valid;
        }
        #endregion Protected Methods
    }
}