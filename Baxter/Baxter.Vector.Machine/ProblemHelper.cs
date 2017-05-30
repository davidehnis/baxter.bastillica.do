using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Baxter.Vector.Machine
{
    public static class ProblemHelper
    {
        public static Problem ReadProblem(string inputFileName)
        {
            var y = new List<double>();
            var x = new List<Node[]>();
            var lines = File.ReadAllLines(inputFileName);

            foreach (var tokens in lines.Select(line => line.Split(" \t\n\r\f".ToCharArray())
                .Where(c => c != String.Empty).ToArray()))
            {
                y.Add(tokens[0].ToDouble());
                x.Add(GetNodes(tokens).ToArray());
            }

            return new Problem { L = y.Count, X = x.ToArray(), Y = y.ToArray() };
        }

        private static IEnumerable<Node> GetNodes(IList<string> tokens)
        {
            for (var i = 1; i <= (tokens.Count() - 1); i++)
            {
                var token = tokens[i].Trim().Split(':');

                yield return new Node
                {
                    Index = token[0].ToInteger(),
                    Value = token[1].ToDouble(),
                };
            }
        }

        public static Problem ReadProblem(List<List<double>> dataset)
        {
            if (dataset == null)
            {
                throw new ArgumentNullException("dataset", "dataset passed in could not be null.");
            }

            if (dataset.Count == 0)
            {
                throw new ArgumentException("dataset should contain at least one field");
            }

            var vy = new List<double>();
            var vx = new List<Node[]>();
            int featureCount = dataset.First().Count - 1;

            for (int i = 0; i < dataset.Count(); i++)
            {
                vy.Add(dataset[i][0]);

                if (!((dataset[i].Count - 1).Equals(featureCount)))
                {
                    throw new InvalidFeatureException(string.Format("The features extracted from the {0} row of dataset does not equal to {1}. Missing one or more feature columns?", i, featureCount));
                }
                var x = new List<Node>();
                for (int j = 1; j < dataset[i].Count; j++)
                {
                    x.Add(new Node
                    {
                        Index = j,
                        Value = dataset[i][j],
                    });
                }
                vx.Add(x.ToArray());
            }

            return new Problem { L = dataset.Count(), X = vx.ToArray(), Y = vy.ToArray() };
        }

        public static Problem ScaleProblem(Problem prob, double lower = -1.0, double upper = 1.0)
        {
            int indexMax = prob.X.Max(X => X.Max(e => e.Index));
            var featureMax = new double[(indexMax + 1)];
            var featureMin = new double[(indexMax + 1)];
            int n = prob.L;

            for (int i = 0; i <= indexMax; i++)
            {
                featureMax[i] = -Double.MaxValue;
                featureMin[i] = Double.MaxValue;
            }

            for (int i = 0; i < n; i++)
            {
                int m = prob.X[i].Count();
                for (int j = 0; j < m; j++)
                {
                    int index = prob.X[i][j].Index;
                    featureMax[index - 1] = Math.Max(featureMax[index - 1], prob.X[i][j].Value);
                    featureMin[index - 1] = Math.Min(featureMin[index - 1], prob.X[i][j].Value);
                }
            }

            var scaledProb = new Problem { L = n, Y = prob.Y.ToArray(), X = new Node[n][] };

            for (int i = 0; i < n; i++)
            {
                int m = prob.X[i].Count();
                scaledProb.X[i] = new Node[m];
                for (int j = 0; j < m; j++)
                {
                    int index = prob.X[i][j].Index;
                    double value = prob.X[i][j].Value;
                    double max = featureMax[index - 1];
                    double min = featureMin[index - 1];

                    scaledProb.X[i][j] = new Node { Index = index };

                    if (Math.Abs(min - max) < double.Epsilon)
                        scaledProb.X[i][j].Value = 0;
                    else
                        scaledProb.X[i][j].Value = lower + (upper - lower) * (value - min) / (max - min);
                }
            }
            return scaledProb;
        }

        public static Problem Scale(this Problem prob, double lower = -1.0, double upper = 1.0)
        {
            return ScaleProblem(prob, lower, upper);
        }

        public static Problem ReadAndScaleProblem(string inputFileName, double lower = -1.0, double upper = 1.0)
        {
            return ScaleProblem(ReadProblem(inputFileName), lower, upper);
        }

        public static Problem ReadAndScaleProblem(List<List<double>> dataset, double lower = -1.0, double upper = 1.0)
        {
            return ScaleProblem(ReadProblem(dataset), lower, upper);
        }

        public static void WriteProblem(string outputFileName, Problem prob)
        {
            using (var sw = new StreamWriter(outputFileName))
            {
                for (int i = 0; i < prob.L; i++)
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("{0} ", prob.Y[i]);
                    for (int j = 0; j < prob.X[i].Count(); j++)
                    {
                        Node node = prob.X[i][j];
                        sb.AppendFormat("{0}:{1} ", node.Index, node.Value);
                    }
                    sw.WriteLine(sb.ToString().Trim());
                }
                sw.Close();
            }
        }
    }
}