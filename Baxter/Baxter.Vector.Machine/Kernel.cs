using System;

namespace Baxter.Vector.Machine
{
    public abstract class Kernel : QMatrix
    {
        public Kernel(KernelType kernelType, double gamma, double r, int degree)
        {
            KernelType = kernelType;
            Gamma = gamma;
            R = r;
            Degree = degree;
        }

        public Kernel(int l, Node[][] x_, Parameter param)
        {
            KernelType = (KernelType)param.KernelType;
            Degree = param.Degree;
            Gamma = param.Gamma;
            Coef0 = param.Coef0;

            X = x_;

            if (KernelType == (KernelType)Parameter.Rbf)
            {
                X_Square = new double[l];
                for (int i = 0; i < l; i++)
                    X_Square[i] = dot(X[i], X[i]);
            }
            else X_Square = null;
        }

        private Node[][] X { get; set; }

        private double Coef0 { get; set; }

        private double[] X_Square { get; set; }

        public KernelType KernelType { get; }

        public double Gamma { get; set; }

        public double R { get; }

        public int Degree { get; }

        private static double dot(Node[] x, Node[] y)
        {
            double sum = 0;
            int xlen = x.Length;
            int ylen = y.Length;
            int i = 0;
            int j = 0;
            while (i < xlen && j < ylen)
            {
                if (x[i].Index == y[j].Index)
                    sum += x[i++].Value * y[j++].Value;
                else
                {
                    if (x[i].Index > y[j].Index)
                        ++j;
                    else
                        ++i;
                }
            }
            return sum;
        }

        private static double powi(double baseNumber, int times)
        {
            double tmp = baseNumber, ret = 1.0;

            for (var t = times; t > 0; t /= 2)
            {
                if (t % 2 == 1) ret *= tmp;
                tmp = tmp * tmp;
            }
            return ret;
        }

        public double kernel_function(int i, int j)
        {
            switch ((int)KernelType)
            {
                case Parameter.Linear:
                    return dot(X[i], X[j]);

                case Parameter.Poly:
                    return powi(Gamma * dot(X[i], X[j]) + Coef0, Degree);

                case Parameter.Rbf:
                    return Math.Exp(-Gamma * (X_Square[i] + X_Square[j] - 2 * dot(X[i], X[j])));

                case Parameter.Sigmoid:
                    return Math.Tanh(Gamma * dot(X[i], X[j]) + Coef0);

                case Parameter.Precomputed:
                    return X[i][(int)(X[j][0].Value)].Value;

                default:
                    return 0;   // java
            }
        }

        public override float[] get_Q(int column, int len)
        {
            throw new NotImplementedException();
        }

        public override double[] get_QD()
        {
            throw new NotImplementedException();
        }

        public override void swap_index(int i, int j)
        {
            do { Node[] tmp = X[i]; X[i] = X[j]; X[j] = tmp; } while (false);
            if (X_Square != null) do { double tmp = X_Square[i]; X_Square[i] = X_Square[j]; X_Square[j] = tmp; } while (false);
        }

        public static double k_function(Node[] x, Node[] y, Parameter param)
        {
            switch (param.KernelType)
            {
                case Parameter.Linear:
                    return dot(x, y);

                case Parameter.Poly:
                    return powi(param.Gamma * dot(x, y) + param.Coef0, param.Degree);

                case Parameter.Rbf:
                    {
                        double sum = 0;
                        int xlen = x.Length;
                        int ylen = y.Length;
                        int i = 0;
                        int j = 0;
                        while (i < xlen && j < ylen)
                        {
                            if (x[i].Index == y[j].Index)
                            {
                                double d = x[i++].Value - y[j++].Value;
                                sum += d * d;
                            }
                            else if (x[i].Index > y[j].Index)
                            {
                                sum += y[j].Value * y[j].Value;
                                ++j;
                            }
                            else
                            {
                                sum += x[i].Value * x[i].Value;
                                ++i;
                            }
                        }

                        while (i < xlen)
                        {
                            sum += x[i].Value * x[i].Value;
                            ++i;
                        }

                        while (j < ylen)
                        {
                            sum += y[j].Value * y[j].Value;
                            ++j;
                        }

                        return Math.Exp(-param.Gamma * sum);
                    }
                case Parameter.Sigmoid:
                    return Math.Tanh(param.Gamma * dot(x, y) + param.Coef0);

                case Parameter.Precomputed:
                    return x[(int)(y[0].Value)].Value;

                default:
                    return 0;   // java
            }
        }
    }
}