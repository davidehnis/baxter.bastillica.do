namespace Baxter.Vector.Machine
{
    internal class Quandary
    {
        public Quandary()
        {
        }

        public Quandary(int l, QMatrix q, double[] p_, byte[] y_,
            double[] alpha_, double cp, double cn, double eps, Solver.SolutionInfo si, int shrinking)
        {
            L = l;
            Q = q;
            P = p_;
            Y = y_;
            Alpha = alpha_;
            Cp = cp;
            Eps = eps;
            Si = si;
            Shrinking = shrinking;
        }

        public double[] Alpha { get; set; }

        public double Cn { get; set; }

        public double Cp { get; set; }

        public double Eps { get; set; }

        public int L { get; set; }

        public double[] P { get; set; }

        public QMatrix Q { get; set; }

        public int Shrinking { get; set; }

        public Solver.SolutionInfo Si { get; set; }

        public byte[] Y { get; set; }
    }
}