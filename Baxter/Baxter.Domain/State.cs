namespace Baxter.Domain
{
    public static class StateExtensions
    {
        #region Public Methods
        public static State State(this State state)
        {
            return state;
        }
        #endregion Public Methods
    }

    //<summary>The most basic means of communicating state of the holder/owner</summary>
    public class State
    {
    }
}