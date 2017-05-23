namespace Baxter.Agents.Automaton
{
    internal interface IConsumer<T>
    {
        #region Public Methods
        bool Process(T item);
        #endregion Public Methods
    }
}