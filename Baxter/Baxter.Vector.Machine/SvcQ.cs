namespace Baxter.Vector.Machine
{
    internal class SvcQ : Kernel
    {
        //private byte[] y;
        //private Cache cache;
        //private double[] QD;

        public SvcQ(Problem prob, Parameter param, byte[] y_)
            : base(prob.L, prob.X, param)
        {
            Y = y_;
            Cache = new Cache(prob.L, (long)(param.CacheSize * (1 << 20)));
            QD = new double[prob.L];
            for (int i = 0; i < prob.L; i++)
                QD[i] = kernel_function(i, i);
        }

        public override float[] get_Q(int i, int len)
        {
            float[][] data = new float[1][];
            int start, j;
            if ((start = Cache.get_data(i, data, len)) < len)
            {
                for (j = start; j < len; j++)
                    data[0][j] = (float)(Y[i] * Y[j] * kernel_function(i, j));
            }
            return data[0];
        }

        private double[] get_QD()
        {
            return QD;
        }

        public override void swap_index(int i, int j)
        {
            Cache.swap_index(i, j);
            base.swap_index(i, j);
            do { byte tmp = Y[i]; Y[i] = Y[j]; Y[j] = tmp; } while (false);
            do { double tmp = QD[i]; QD[i] = QD[j]; QD[j] = tmp; } while (false);
        }
    }
}