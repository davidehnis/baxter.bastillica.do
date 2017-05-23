using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Baxter.Agents.Automaton.Console.Automaton;

namespace Baxter.Agents.Automaton.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AutomatonClient service = new AutomatonClient();

            System.Console.WriteLine(service.GetData(7));
            System.Console.Read();
        }
    }
}