using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Vector.Machine
{
    public class Svc : Machine
    {
        public Svc(SvmType svm_type, Problem prob, Kernel kernel, double C, double cache_size = 100, int probability = 0)
            : base(prob, (int)svm_type, kernel, C, 0.0, cache_size, 1e-3, 0.1, 1, probability, 0, new int[0], new double[0])
        {
        }

        public Svc(string model_file_name)
            : base(model_file_name)
        {
        }

        public override double Predict(Node[] x)
        {
            if (Model == null)
                throw new Exception("No trained svm model");

            return svm_predict(Model, x);
        }

        public Dictionary<int, double> PredictProbabilities(Node[] x)
        {
            if (Model == null)
                throw new Exception("No trained svm model");

            var probabilities = new Dictionary<int, double>();
            int nr_class = Model.NrClass;

            double[] prob_estimates = new double[nr_class];
            int[] labels = new int[nr_class];
            svm_get_labels(Model, labels);

            svm_predict_probability(Model, x, prob_estimates);
            for (int i = 0; i < nr_class; i++)
                probabilities.Add(labels[i], prob_estimates[i]);

            return probabilities;
        }

        public double GetCrossValidationAccuracy(int nr_fold)
        {
            int i;
            int total_correct = 0;
            double[] target = new double[Problem.L];

            svm_cross_validation(Problem, Parameter, nr_fold, target);

            for (i = 0; i < Problem.L; i++)
                if (Math.Abs(target[i] - Problem.Y[i]) < double.Epsilon)
                    ++total_correct;
            var CVA = total_correct / (double)Problem.L;
            Debug.WriteLine("Cross Validation Accuracy = {0:P} ({1}/{2})", CVA, total_correct, Problem.L);
            return CVA;
        }
    }
}