using System;
using System.Runtime.Serialization;

namespace Baxter.Vector.Machine
{
    public class Parameter : ISerializable
    {
        public const int Svc = 0;
        public const int Precomputed = 4;
        public const int Rbf = 2;
        public const int Poly = 1;
        public const int Linear = 0;
        public const int Sigmoid = 3;
        public const int Epsilon = 3;
        public const int OneClass = 2;
        public const int NuSvc = 1;
        public const int NuSvr = 4;

        public int[] WeightLabel { get; }

        public int Probability { get; }

        public int Shrinking { get; }

        public double P { get; }

        public double Nu { get; }

        public double[] Weight { get; }

        public int NrWeight { get; }

        public double CacheSize { get; }

        public double Eps { get; }

        public double Coef0 { get; }

        public double Gamma { get; }

        public int Degree { get; }

        public int KernelType { get; }

        public int SvmType { get; }

        public double C { get; }

        public Parameter()
        {
            WeightLabel = new[] {0};
            Probability = 0;
            Shrinking = 0;
            P = 0;
            Nu = 0;
            Weight = new [] {0.0};
            NrWeight = 0;
            CacheSize = 0;
            Eps = 0;
            Coef0 = 0;
            Gamma = 0;
            Degree = 0;
            KernelType = 0;
            SvmType = 0;
            C = 0;
        }

        public Parameter(int[] weightLabel, int probability, int shrinking, double p, double nu, double[] weight, int nrWeight, double cacheSize, double eps, double coef0, double gamma, int degree, int kernelType, int svmType, double c)
        {
            WeightLabel = weightLabel;
            Probability = probability;
            Shrinking = shrinking;
            P = p;
            Nu = nu;
            Weight = weight;
            NrWeight = nrWeight;
            CacheSize = cacheSize;
            Eps = eps;
            Coef0 = coef0;
            Gamma = gamma;
            Degree = degree;
            KernelType = kernelType;
            SvmType = svmType;
            C = c;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
