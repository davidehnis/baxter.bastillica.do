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

        public int[] WeightLabel { get; set; }

        public int Probability { get; set; }

        public int Shrinking { get; set; }

        public double P { get; set; }

        public double Nu { get; set; }

        public double[] Weight { get; set; }

        public int NrWeight { get; set; }

        public double CacheSize { get; set; }

        public double Eps { get; set; }

        public double Coef0 { get; set; }

        public double Gamma { get; set; }

        public int Degree { get; set; }

        public int KernelType { get; set; }

        public int SvmType { get; set; }

        public double C { get; set; }

        public void Copy(Parameter parameter)
        {
            WeightLabel = parameter.WeightLabel;
            Probability = parameter.Probability;
            Shrinking = parameter.Shrinking;
            P = parameter.P;
            Nu = parameter.Nu;
            Weight = parameter.Weight;
            NrWeight = parameter.NrWeight;
            CacheSize = parameter.CacheSize;
            Eps = parameter.Eps;
            Coef0 = parameter.Coef0;
            Gamma = parameter.Gamma;
            Degree = parameter.Degree;
            KernelType = parameter.KernelType;
            SvmType = parameter.SvmType;
            C = parameter.C;
        }

        // regression or one-class-svm
        public bool IsRegression()
        {
            return SvmType == Parameter.OneClass ||
                   SvmType == Parameter.Epsilon ||
                   SvmType == Parameter.NuSvr;
        }

        // See use in Machine... Good probablity
        public bool IsGoodProbability()
        {
            return Probability == 1 &&
                   (SvmType == Parameter.Epsilon ||
                    SvmType == Parameter.NuSvr);
        }

        public Parameter(Parameter parameter)
        {
            Copy(parameter);
        }

        public Parameter()
        {
            WeightLabel = new[] { 0 };
            Probability = 0;
            Shrinking = 0;
            P = 0;
            Nu = 0;
            Weight = new[] { 0.0 };
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