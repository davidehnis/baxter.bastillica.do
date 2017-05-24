using System;
using System.Runtime.Serialization;

namespace Baxter.Vector.Machine
{
    public class Model : ISerializable
    {
        public Parameter Parameter { get; set; }

        public int NrClass { get; set; }

        public int L { get; set; }

        public Node[][] SvNodes { get; set; }

        public double[][] SvCoef { get; set; }

        public double[] Rho { get; set; }

        public double[] ProbA { get; set; }

        public double[] ProbB { get; set; }

        public int[] SvIndicies { get; set; }

        public int[] Label { get; set; }

        public int[] Nsv { get; set; }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}