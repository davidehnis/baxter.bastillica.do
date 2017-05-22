namespace Baxter.Domain
{
    //<summary>A set of Hash object helpers</summary>
    public static class HashExtension
    {
        #region Public Methods
        //<summary>Simply turns an int hash code into a hash oject</summary>
        public static Hash Hash(this int code)
        {
            return new Hash(code);
        }
        #endregion Public Methods
    }

    //<summary>For better documentation, the Hash object is a boxed integer representing the hash value for this object</summary>
    public class Hash
    {
        #region Public Constructors
        //<summary>Must be initialized with a hash value</summary>
        public Hash(int hash)
        {
            Code = hash;
        }
        #endregion Public Constructors

        #region Protected Properties
        protected int Code { get; set; }
        #endregion Protected Properties
    }
}