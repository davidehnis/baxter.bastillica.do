using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Vector.Machine
{
    internal class SvcQ : Kernel
    {
        private byte[] y;
        private Cache cache;
        private double[] QD;

        public SvcQ(Problem prob, Parameter param, byte[] y_)
            : base(prob.L, prob.X, param)
        {
            y = y_;
            cache = new Cache(prob.L, (long)(param.CacheSize * (1 << 20)));
            QD = new double[prob.L];
            for (int i = 0; i < prob.L; i++)
                QD[i] = kernel_function(i, i);
        }

        private float[] get_Q(int i, int len)
        {
            float[][] data = new float[1][];
            int start, j;
            if ((start = cache.get_data(i, data, len)) < len)
            {
                for (j = start; j < len; j++)
                    data[0][j] = (float)(y[i] * y[j] * kernel_function(i, j));
            }
            return data[0];
        }

        private double[] get_QD()
        {
            return QD;
        }

        private void swap_index(int i, int j)
        {
            cache.swap_index(i, j);
            base.swap_index(i, j);
            do { byte tmp = y[i]; y[i] = y[j]; y[j] = tmp; } while (false);
            do { double tmp = QD[i]; QD[i] = QD[j]; QD[j] = tmp; } while (false);
        }
    }
}