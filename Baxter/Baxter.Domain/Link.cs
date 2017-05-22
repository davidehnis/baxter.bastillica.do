using System;

namespace Baxter.Domain
{
    //<summary>A basic construct that connects two nodes</summary>
    public class Link
    {
        #region Public Constructors
        /// <summary>A new "blank" link</summary>
        public Link()
        {
        }

        /// <summary>Initializes a new link with left and right nodes</summary>
        public Link(Node left, Node right)
        {
            Left = left;
            Right = right;
        }
        #endregion Public Constructors

        #region Public Properties
        //<summary>The unique identifier for this link</summary>
        public Key Key { get; set; }

        //<summary>The left-hand node</summary>
        public Node Left { get; set; }

        //<summary>The name for this link</summary>
        public string Name { get; set; }

        //<summary>The right-hand node</summary>
        public Node Right { get; set; }

        //<summary>The type of this link</summary>
        public Type Type { get; set; }

        //<summary>The weight or stength of the link between two nodes</summary>
        public int Weight { get; set; }
        #endregion Public Properties

        #region Public Methods
        //<summary>Returns the unique hash object for this link</summary>
        public virtual Hash GetHash()
        {
            return new Hash(GetHashCode());
        }

        //<summary>Returns the unique hash code for this link</summary>
        public override int GetHashCode()
        {
            return string.Format
                ("{0}-{1}-{2}-{3}-{4}-{5}-{6}", Key.GetHashCode(), Left.GetHashCode(), Right.GetHashCode(), Key.GetHashCode(), Name, Type, Weight).GetHashCode();
        }
        #endregion Public Methods
    }
}