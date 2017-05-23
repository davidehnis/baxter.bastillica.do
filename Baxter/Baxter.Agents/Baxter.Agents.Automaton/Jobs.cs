namespace Baxter.Agents.Automaton
{
    internal class Jobs : SafeQueue<Job>
    {
        public Jobs(IConsumer<Job> consumer) : base(consumer)
        {
        }
    }
}