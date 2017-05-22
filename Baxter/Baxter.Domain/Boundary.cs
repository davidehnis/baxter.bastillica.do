namespace Baxter.Domain
{
    //<summary>A boundary represents a logic seperation for work and/or data</summary>
    public abstract class Boundary : Construct
    {
        #region Public Constructors
        public Boundary() : base(typeof(Boundary))
        {
        }
        #endregion Public Constructors
    }
}