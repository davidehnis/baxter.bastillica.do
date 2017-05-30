using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Vector.Machine
{
    public class OneClassQ : Kernel
    {
        private Cache cache;
        private double[] QD;

        public OneClassQ(Problem prob, Parameter param) : base(prob.L, prob.X, param)
        {
            cache = new Cache(prob.L, (long)(param.CacheSize * (1 << 20)));
            QD = new double[prob.L];
            for (int i = 0; i < prob.L; i++)
                QD[i] = kernel_function(i, i);
        }

        public override float[] get_Q(int i, int len)
        {
            float[][] data = new float[1][];
            int start, j;
            if ((start = cache.get_data(i, data, len)) < len)
            {
                for (j = start; j < len; j++)
                    data[0][j] = (float)kernel_function(i, j);
            }
            return data[0];
        }

        public override void swap_index(int i, int j)
        {
            cache.swap_index(i, j);
            base.swap_index(i, j);
            do { double tmp = QD[i]; QD[i] = QD[j]; QD[j] = tmp; } while (false);
        }
    }
}