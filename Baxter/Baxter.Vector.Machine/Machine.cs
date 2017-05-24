namespace Baxter.Vector.Machine
{
    public abstract class Machine
    {
        protected Problem Problem { get; set; }

        protected Parameter Parameter { get; set; }

        protected Model Model { get; set; }

        protected Machine(string modelFileName)
        {
        }

        protected Machine(Problem prob, Parameter param)
        { }

        protected Machine(string input_file_name, Parameter param)
        { }

        protected Machine(Problem prob, int svm_type, Kernel kernel, double C, double nu, double cache_size, double eps, double p, int shrinking, int probability, int nr_weight, int[] weight_label, double[] weight)
        { }

        protected Machine(Problem prob, int svm_type, int kernel_type, int degree, double C, double gamma, double coef0, double nu, double cache_size, double eps, double p, int shrinking, int probability, int nr_weight, int[] weight_label, double[] weight)
        { }

        public void Export(string modelFileName)
        {
        }

        public void Import(string modelFileName)
        {
        }

        public abstract double Predict(Node[] nodes);

        public void Train()
        {
        }
    }
}