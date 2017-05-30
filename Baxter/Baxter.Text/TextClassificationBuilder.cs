using Baxter.Vector.Machine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Baxter.Text
{
    internal class TextClassificationBuilder
    {
        public Problem CreateProblem(IEnumerable<string> x, double[] y, IReadOnlyList<string> vocabulary)
        {
            return new Problem
            {
                Y = y,
                X = x.Select(xVector => CreateNode(xVector, vocabulary)).ToArray(),
                L = y.Length
            };
        }

        public static Node[] CreateNode(string x, IReadOnlyList<string> vocabulary)
        {
            var node = new List<Node>(vocabulary.Count);

            string[] words = x.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < vocabulary.Count; i++)
            {
                int occurenceCount = words.Count(s => String.Equals(s, vocabulary[i], StringComparison.OrdinalIgnoreCase));
                if (occurenceCount == 0)
                    continue;

                node.Add(new Node
                {
                    Index = i + 1,
                    Value = occurenceCount
                });
            }

            return node.ToArray();
        }
    }
}