using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Vector.Machine
{
    internal class Solution
    {
        private static byte FREE = 2;
        private static byte LOWER_BOUND = 0;
        private static byte UPPER_BOUND = 1;

        public Solution()
        {
        }

        public Solution(Quandary quandary)
        {
            L = quandary.L;
            Q = quandary.Q;
            QD = quandary.Q.QD;
            P = quandary.P;
            Y = quandary.Y;
            Alpha = quandary.Alpha;
            Cp = quandary.Cp;
            Cn = quandary.Cn;
            Eps = quandary.Eps;
            Unshrink = false;

            Initialize();
        }

        public int[] ActiveSet { get; set; }

        public int ActiveSize { get; set; }

        public double[] Alpha { get; set; }

        public byte[] AlphaStatus { get; set; }

        public double Cn { get; set; }

        public double Cp { get; set; }

        public double Eps { get; set; }

        public double[] G { get; set; }

        public double[] GBar { get; set; }

        public double Infinite { get; set; } = double.PositiveInfinity;

        // gradient, if we treat free variables as 0
        public int L { get; set; }

        public double[] P { get; set; }

        // gradient of objective function LOWER_BOUND, UPPER_BOUND, FREE
        public QMatrix Q { get; set; }

        public double[] QD { get; set; }

        public bool Unshrink { get; set; }

        public byte[] Y { get; set; }

        protected static bool IsFree(byte[] array, int i)
        {
            return array[i] == FREE;
        }

        protected static bool IsLowerBound(byte[] array, int i)
        {
            return array[i] == LOWER_BOUND;
        }

        protected bool IsUpperBound(byte[] array, int i)
        {
            return array[i] == UPPER_BOUND;
        }

        private void Initialize()
        {
            // initialize alpha_status
            {
                AlphaStatus = new byte[L];
                for (var i = 0; i < L; i++)
                {
                    update_alpha_status(i);
                }
            }

            // initialize active set (for shrinking)
            {
                ActiveSet = new int[L];
                for (var i = 0; i < L; i++)
                {
                    ActiveSet[i] = i;
                }
                ActiveSize = L;
            }

            // initialize gradient
            {
                G = new double[L];
                GBar = new double[L];

                for (var i = 0; i < L; i++)
                {
                    G[i] = P[i];
                    GBar[i] = 0;
                }

                for (var i = 0; i < L; i++)
                {
                    if (!IsLowerBound(AlphaStatus, i))
                    {
                        float[] Q_i = Q.get_Q(i, L);
                        double alpha_i = Alpha[i];
                        int j;
                        for (j = 0; j < L; j++)
                            G[j] += alpha_i * Q_i[j];
                        if (IsUpperBound(AlphaStatus, i))
                            for (j = 0; j < L; j++)
                                GBar[j] += get_C(i) * Q_i[j];
                    }
                }
            }
        }
    }
}