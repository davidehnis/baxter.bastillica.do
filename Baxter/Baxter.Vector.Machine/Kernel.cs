namespace Baxter.Vector.Machine
{
    public class Kernel
    {
        public Kernel(KernelType kernelType, double gamma, double r, int degree)
        {
            KernelType = kernelType;
            Gamma = gamma;
            R = r;
            Degree = degree;
        }

        public KernelType KernelType { get; }

        public double Gamma { get; set; }

        public double R { get; }

        public int Degree { get; }
    }
}