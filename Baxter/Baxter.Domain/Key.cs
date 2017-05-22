using System;

namespace Baxter.Domain
{
    //<summary>The basic Key object - unique to its owner</summary>
    public class Key
    {
        #region Public Constructors
        /// <summary>Initialize the Key with a new Id, making it type of Id</summary>
        public Key(Id key)
        {
            Type = typeof(Id);
            Value = key;
        }

        /// <summary>Default key constructor that creates a new Id key type</summary>
        public Key() : this(Id.NewId())
        {
        }

        /// <summary>Creates a new key with the value of another key</summary>
        public Key(Key key)
        {
            Type = key.Type;
            Value = key.Value;
        }

        /// <summary>Creates a new key with a string value</summary>
        public Key(string name)
        {
            Type = typeof(string);
            Value = name;
        }
        #endregion Public Constructors

        #region Public Properties
        /// <summary>A helper static to create a new Key value</summary>
        public static Key New
        {
            get
            {
                return new Key(Id.NewId());
            }
        }

        /// <summary>The type of key (Id, string, int, etc)</summary>
        public Type Type { get; protected set; }

        /// <summary>The actual key value</summary>
        public object Value { get; protected set; }
        #endregion Public Properties

        #region Public Methods
        /// <summary>Static helper function to create an empty Key</summary>
        public static Key Empty()
        {
            return new Key();
        }

        /// <summary>Creates the unique hash code for this instance</summary>
        public override int GetHashCode()
        {
            return string.Format
                ("{0}-{1}", Value, Type).GetHashCode();
        }

        /// <summary>Converts the Key's value to a string</summary>
        public override string ToString()
        {
            return Value.ToString();
        }
        #endregion Public Methods
    }
}