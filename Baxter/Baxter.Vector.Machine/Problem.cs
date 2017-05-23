using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Vector.Machine
{
    public class Problem
    {
        public int L { get; }

        public double[] Y { get; }

        public Node[][] X { get; }
    }
}
