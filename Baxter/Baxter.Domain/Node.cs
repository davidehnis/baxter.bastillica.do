using System;

namespace Baxter.Domain
{
    //<summary>The most basic data construct</summary>
    public class Node : Construct
    {
        #region Public Constructors
        public Node() : base(typeof(Node))
        {
        }

        public Node(string data) :
            this(new Key(Id.NewId()), new Value(data), typeof(string))
        {
        }

        public Node(object data) :
            this(new Key(Id.NewId()), new Value(data), data.GetType())
        {
        }

        public Node(Key key, Value value, Type type) : base(type)
        {
            Key = key;
            Value = value;
        }
        #endregion Public Constructors

        #region Public Properties
        //<summary>The unique identifier for this node</summary>
        public Key Key { get; protected set; }

        //<summary>The data/value for this node</summary>
        public Value Value { get; protected set; }
        #endregion Public Properties

        #region Public Methods
        //<summary>Converts this node's data to a string</summary>
        public new virtual string ToString()
        {
            if (!Value.IsNull(Value))
            {
                return Value.ToString();
            }

            return string.Empty;
        }
        #endregion Public Methods
    }
}