using System;

namespace Baxter.Vector.Machine
{
    public abstract class Machine
    {
        protected Machine(string modelFileName)
        {
        }

        protected Machine(Problem prob, Parameter param)
        {
            var error = svm_check_parameter(prob, param);
            if (error != null)
            {
                throw new Exception(error);
            }

            Problem = prob;
            Parameter = param;

            Train();
        }

        /// <summary>
        /// Train the SVM and save the model
        /// </summary>
        public void Train()
        {
            Model = svm_train(Problem, Parameter);
        }

        public static Model svm_train(Problem prob, Parameter param)
        {
            Model model = new Model();
            model.Parameter = param;

            if (param.SvmType == Parameter.OneClass ||
                param.SvmType == Parameter.Epsilon ||
                param.SvmType == Parameter.NuSvr)
            {
                // regression or one-class-svm
                model.NrClass = 2;
                model.Label = null;
                model.Nsv = null;
                model.ProbA = null; model.ProbB = null;
                model.SvCoef = new double[1][];

                if (param.Probability == 1 &&
                    (param.SvmType == Parameter.Epsilon ||
                     param.SvmType == Parameter.NuSvr))
                {
                    model.ProbA = new double[1];
                    model.ProbB[0] = svm_svr_probability(prob, param);
                }

                decision_function f = svm_train_one(prob, param, 0, 0);
                model.Rho = new double[1];
                model.Rho[0] = f.rho;

                int nSV = 0;
                int i;
                for (i = 0; i < prob.L; i++)
                    if (Math.Abs(f.alpha[i]) > 0) ++nSV;
                model.L = nSV;
                model.SvNodes = new Node[nSV][];
                model.SvCoef[0] = new double[nSV];
                model.SvIndicies = new int[nSV];
                int j = 0;
                for (i = 0; i < prob.L; i++)
                    if (Math.Abs(f.alpha[i]) > 0)
                    {
                        model.SvNodes[j] = prob.X[i];
                        model.SvCoef[0][j] = f.alpha[i];
                        model.SvIndicies[j] = i + 1;
                        ++j;
                    }
            }
            else
            {
                // classification
                int l = prob.L;
                int[] tmp_nr_class = new int[1];
                int[][] tmp_label = new int[1][];
                int[][] tmp_start = new int[1][];
                int[][] tmp_count = new int[1][];
                int[] perm = new int[l];

                // group training data of the same class
                svm_group_classes(prob, out tmp_nr_class, out tmp_label, out tmp_start, out tmp_count, perm);
                int nr_class = tmp_nr_class[0];
                int[] label = tmp_label[0];
                int[] start = tmp_start[0];
                int[] count = tmp_count[0];

                //if (nr_class == 1)
                //    svm.info("WARNING: training data in only one class. See README for details.\n");

                Node[][] x = new Node[l][];
                int i;
                for (i = 0; i < l; i++)
                    x[i] = prob.X[perm[i]];

                // calculate weighted C

                double[] weighted_C = new double[nr_class];
                for (i = 0; i < nr_class; i++)
                    weighted_C[i] = param.C;
                for (i = 0; i < param.NrWeight; i++)
                {
                    int j;
                    for (j = 0; j < nr_class; j++)
                        if (param.WeightLabel[i] == label[j])
                            break;
                    if (j == nr_class)
                    { }
                    //  System.err.print("WARNING: class label " + param.weight_label[i] + " specified in weight is not found\n");
                    else
                        weighted_C[j] *= param.Weight[i];
                }

                // train k*(k-1)/2 models

                bool[] nonzero = new bool[l];
                for (i = 0; i < l; i++)
                    nonzero[i] = false;
                decision_function[] f = new decision_function[nr_class * (nr_class - 1) / 2];

                double[] probA = null, probB = null;
                if (param.Probability == 1)
                {
                    probA = new double[nr_class * (nr_class - 1) / 2];
                    probB = new double[nr_class * (nr_class - 1) / 2];
                }

                int p = 0;
                for (i = 0; i < nr_class; i++)
                    for (int j = i + 1; j < nr_class; j++)
                    {
                        Problem sub_prob = new Problem();
                        int si = start[i], sj = start[j];
                        int ci = count[i], cj = count[j];
                        sub_prob.L = ci + cj;
                        sub_prob.X = new Node[sub_prob.L][];
                        sub_prob.Y = new double[sub_prob.L];
                        int k;
                        for (k = 0; k < ci; k++)
                        {
                            sub_prob.X[k] = x[si + k];
                            sub_prob.Y[k] = +1;
                        }
                        for (k = 0; k < cj; k++)
                        {
                            sub_prob.X[ci + k] = x[sj + k];
                            sub_prob.Y[ci + k] = -1;
                        }

                        if (param.Probability == 1)
                        {
                            double[] probAB = new double[2];
                            svm_binary_svc_probability(sub_prob, param, weighted_C[i], weighted_C[j], probAB);
                            probA[p] = probAB[0];
                            probB[p] = probAB[1];
                        }

                        f[p] = svm_train_one(sub_prob, param, weighted_C[i], weighted_C[j]);
                        for (k = 0; k < ci; k++)
                            if (!nonzero[si + k] && Math.Abs(f[p].alpha[k]) > 0)
                                nonzero[si + k] = true;
                        for (k = 0; k < cj; k++)
                            if (!nonzero[sj + k] && Math.Abs(f[p].alpha[ci + k]) > 0)
                                nonzero[sj + k] = true;
                        ++p;
                    }

                // build output

                model.NrClass = nr_class;

                model.Label = new int[nr_class];
                for (i = 0; i < nr_class; i++)
                    model.Label[i] = label[i];

                model.Rho = new double[nr_class * (nr_class - 1) / 2];
                for (i = 0; i < nr_class * (nr_class - 1) / 2; i++)
                    model.Rho[i] = f[i].rho;

                if (param.Probability == 1)
                {
                    model.ProbA = new double[nr_class * (nr_class - 1) / 2];
                    model.ProbB = new double[nr_class * (nr_class - 1) / 2];
                    for (i = 0; i < nr_class * (nr_class - 1) / 2; i++)
                    {
                        model.ProbA[i] = probA[i];
                        model.ProbB[i] = probB[i];
                    }
                }
                else
                {
                    model.ProbA = null;
                    model.ProbB = null;
                }

                int nnz = 0;
                int[] nz_count = new int[nr_class];
                model.Nsv = new int[nr_class];
                for (i = 0; i < nr_class; i++)
                {
                    int nSV = 0;
                    for (int j = 0; j < count[i]; j++)
                        if (nonzero[start[i] + j])
                        {
                            ++nSV;
                            ++nnz;
                        }
                    model.Nsv[i] = nSV;
                    nz_count[i] = nSV;
                }

                //svm.info("Total nSV = " + nnz + "\n");

                model.L = nnz;
                model.SvNodes = new Node[nnz][];
                model.SvIndicies = new int[nnz];
                p = 0;
                for (i = 0; i < l; i++)
                    if (nonzero[i])
                    {
                        model.SvNodes[p] = x[i];
                        model.SvIndicies[p++] = perm[i] + 1;
                    }

                int[] nz_start = new int[nr_class];
                nz_start[0] = 0;
                for (i = 1; i < nr_class; i++)
                    nz_start[i] = nz_start[i - 1] + nz_count[i - 1];

                model.SvCoef = new double[nr_class - 1][];
                for (i = 0; i < nr_class - 1; i++)
                    model.SvCoef[i] = new double[nnz];

                p = 0;
                for (i = 0; i < nr_class; i++)
                    for (int j = i + 1; j < nr_class; j++)
                    {
                        // classifier (i,j): coefficients with
                        // i are in sv_coef[j-1][nz_start[i]...],
                        // j are in sv_coef[i][nz_start[j]...]

                        int si = start[i];
                        int sj = start[j];
                        int ci = count[i];
                        int cj = count[j];

                        int q = nz_start[i];
                        int k;
                        for (k = 0; k < ci; k++)
                            if (nonzero[si + k])
                                model.SvCoef[j - 1][q++] = f[p].alpha[k];
                        q = nz_start[j];
                        for (k = 0; k < cj; k++)
                            if (nonzero[sj + k])
                                model.SvCoef[i][q++] = f[p].alpha[ci + k];
                        ++p;
                    }
            }
            return model;
        }

        protected Machine(string input_file_name, Parameter param)
        {
        }

        protected Machine(Problem prob, int svm_type, Kernel kernel, double C, double nu, double cache_size, double eps,
            double p, int shrinking, int probability, int nr_weight, int[] weight_label, double[] weight)
            : this(prob, new Parameter()
            {
                SvmType = svm_type,
                KernelType = (int)kernel.KernelType,
                Degree = kernel.Degree,
                C = C,
                Gamma = kernel.Gamma,
                Coef0 = kernel.R,
                Nu = nu,
                CacheSize = cache_size,
                Eps = eps,
                P = p,
                Shrinking = shrinking,
                Probability = probability,
                NrWeight = nr_weight,
                WeightLabel = weight_label,
                Weight = weight,
            })
        {
        }

        protected Machine(Problem prob, int svm_type, int kernel_type, int degree, double C, double gamma, double coef0,
            double nu, double cache_size, double eps, double p, int shrinking, int probability, int nr_weight,
            int[] weight_label, double[] weight)
            : this(prob, new Parameter()
            {
                SvmType = svm_type,
                KernelType = kernel_type,
                Degree = degree,
                C = C,
                Gamma = gamma,
                Coef0 = coef0,
                Nu = nu,
                CacheSize = cache_size,
                Eps = eps,
                P = p,
                Shrinking = shrinking,
                Probability = probability,
                NrWeight = nr_weight,
                WeightLabel = weight_label,
                Weight = weight,
            })
        {
        }

        protected Model Model { get; set; }

        protected Parameter Parameter { get; set; }

        protected Problem Problem { get; set; }

        public static String svm_check_parameter(Problem prob, Parameter param)
        {
            // svm_type

            int svm_type = param.SvmType;
            if (svm_type != Parameter.Svc &&
                svm_type != Parameter.NuSvc &&
                svm_type != Parameter.OneClass &&
                svm_type != Parameter.Epsilon &&
                svm_type != Parameter.NuSvr)
                return "unknown svm type";

            // kernel_type, degree

            int kernel_type = param.KernelType;
            if (kernel_type != Parameter.Linear &&
                kernel_type != Parameter.Poly &&
                kernel_type != Parameter.Rbf &&
                kernel_type != Parameter.Sigmoid &&
                kernel_type != Parameter.Precomputed)
                return "unknown kernel type";

            if (param.Gamma < 0)
                return "gamma < 0";

            if (param.Degree < 0)
                return "degree of polynomial kernel < 0";

            // cache_size,eps,C,nu,p,shrinking

            if (param.CacheSize <= 0)
                return "cache_size <= 0";

            if (param.Eps <= 0)
                return "eps <= 0";

            if (svm_type == Parameter.Svc ||
                svm_type == Parameter.Epsilon ||
                svm_type == Parameter.NuSvr)
                if (param.C <= 0)
                    return "C <= 0";

            if (svm_type == Parameter.NuSvc ||
                svm_type == Parameter.OneClass ||
                svm_type == Parameter.NuSvr)
                if (param.Nu <= 0 || param.Nu > 1)
                    return "nu <= 0 or nu > 1";

            if (svm_type == Parameter.Epsilon)
                if (param.P < 0)
                    return "p < 0";

            if (param.Shrinking != 0 &&
                param.Shrinking != 1)
                return "shrinking != 0 and shrinking != 1";

            if (param.Probability != 0 &&
                param.Probability != 1)
                return "probability != 0 and probability != 1";

            if (param.Probability == 1 &&
                svm_type == Parameter.OneClass)
                return "one-class SVM probability output not supported yet";

            // check whether nu-svc is feasible

            if (svm_type == Parameter.NuSvc)
            {
                int l = prob.L;
                int max_nr_class = 16;
                int nr_class = 0;
                int[] label = new int[max_nr_class];
                int[] count = new int[max_nr_class];

                int i;
                for (i = 0; i < l; i++)
                {
                    int this_label = (int)prob.Y[i];
                    int j;
                    for (j = 0; j < nr_class; j++)
                        if (this_label == label[j])
                        {
                            ++count[j];
                            break;
                        }

                    if (j == nr_class)
                    {
                        if (nr_class == max_nr_class)
                        {
                            max_nr_class *= 2;
                            int[] new_data = new int[max_nr_class];
                            Array.Copy(label, 0, new_data, 0, label.Length);
                            label = new_data;

                            new_data = new int[max_nr_class];
                            Array.Copy(count, 0, new_data, 0, count.Length);
                            count = new_data;
                        }
                        label[nr_class] = this_label;
                        count[nr_class] = 1;
                        ++nr_class;
                    }
                }

                for (i = 0; i < nr_class; i++)
                {
                    int n1 = count[i];
                    for (int j = i + 1; j < nr_class; j++)
                    {
                        int n2 = count[j];
                        if (param.Nu * (n1 + n2) / 2 > Math.Min(n1, n2))
                            return "specified nu is infeasible";
                    }
                }
            }

            return null;
        }

        // Stratified cross validation
        public static void svm_cross_validation(Problem prob, Parameter param, int nr_fold, double[] target)
        {
            int i;
            int[] fold_start = new int[nr_fold + 1];
            int l = prob.L;
            int[] perm = new int[l];
            var rand = new Random();

            // stratified cv may not give leave-one-out rate Each class to l folds -> some folds may
            // have zero elements
            if ((param.SvmType == Parameter.Svc ||
                 param.SvmType == Parameter.NuSvc) && nr_fold < l)
            {
                int[] tmp_nr_class = new int[1];
                int[][] tmp_label = new int[1][];
                int[][] tmp_start = new int[1][];
                int[][] tmp_count = new int[1][];

                svm_group_classes(prob, out tmp_nr_class, out tmp_label, out tmp_start, out tmp_count, perm);

                int nr_class = tmp_nr_class[0];
                int[] start = tmp_start[0];
                int[] count = tmp_count[0];

                // random shuffle and then data grouped by fold using the array perm
                int[] fold_count = new int[nr_fold];
                int c;
                int[] index = new int[l];
                for (i = 0; i < l; i++)
                    index[i] = perm[i];
                for (c = 0; c < nr_class; c++)
                    for (i = 0; i < count[c]; i++)
                    {
                        int j = i + rand.Next(count[c] - i);
                        do
                        {
                            int tmp = index[start[c] + j];
                            index[start[c] + j] = index[start[c] + i];
                            index[start[c] + i] = tmp;
                        } while (false);
                    }
                for (i = 0; i < nr_fold; i++)
                {
                    fold_count[i] = 0;
                    for (c = 0; c < nr_class; c++)
                        fold_count[i] += (i + 1) * count[c] / nr_fold - i * count[c] / nr_fold;
                }
                fold_start[0] = 0;
                for (i = 1; i <= nr_fold; i++)
                    fold_start[i] = fold_start[i - 1] + fold_count[i - 1];
                for (c = 0; c < nr_class; c++)
                    for (i = 0; i < nr_fold; i++)
                    {
                        int begin = start[c] + i * count[c] / nr_fold;
                        int end = start[c] + (i + 1) * count[c] / nr_fold;
                        for (int j = begin; j < end; j++)
                        {
                            perm[fold_start[i]] = index[j];
                            fold_start[i]++;
                        }
                    }
                fold_start[0] = 0;
                for (i = 1; i <= nr_fold; i++)
                    fold_start[i] = fold_start[i - 1] + fold_count[i - 1];
            }
            else
            {
                for (i = 0; i < l; i++) perm[i] = i;
                for (i = 0; i < l; i++)
                {
                    int j = i + rand.Next(l - i);
                    do
                    {
                        int tmp = perm[i];
                        perm[i] = perm[j];
                        perm[j] = tmp;
                    } while (false);
                }
                for (i = 0; i <= nr_fold; i++)
                    fold_start[i] = i * l / nr_fold;
            }

            for (i = 0; i < nr_fold; i++)
            {
                int begin = fold_start[i];
                int end = fold_start[i + 1];
                int j, k;
                var subprob = new Problem();

                subprob.L = l - (end - begin);
                subprob.X = new Node[subprob.L][];
                subprob.Y = new double[subprob.L];

                k = 0;
                for (j = 0; j < begin; j++)
                {
                    subprob.X[k] = prob.X[perm[j]];
                    subprob.Y[k] = prob.Y[perm[j]];
                    ++k;
                }
                for (j = end; j < l; j++)
                {
                    subprob.X[k] = prob.X[perm[j]];
                    subprob.Y[k] = prob.Y[perm[j]];
                    ++k;
                }
                Model submodel = Train(subprob, param);

                if (param.Probability == 1 &&
                    (param.SvmType == Parameter.Svc ||
                     param.SvmType == Parameter.NuSvc))
                {
                    double[] prob_estimates = new double[submodel.NrClass];
                    for (j = begin; j < end; j++)
                        target[perm[j]] = svm_predict_probability(submodel, prob.X[perm[j]], prob_estimates);
                }
                else
                    for (j = begin; j < end; j++)
                        target[perm[j]] = svm_predict(submodel, prob.X[perm[j]]);
            }
        }

        public static void svm_get_labels(Model model, int[] label)
        {
            if (model.Label != null)
                for (int i = 0; i < model.NrClass; i++)
                    label[i] = model.Label[i];
        }

        public static double svm_predict(Model model, Node[] x)
        {
            int nr_class = model.NrClass;
            double[] dec_values;
            if (model.Parameter.SvmType == Parameter.OneClass ||
                model.Parameter.SvmType == Parameter.Epsilon ||
                model.Parameter.SvmType == Parameter.NuSvr)
                dec_values = new double[1];
            else
                dec_values = new double[nr_class * (nr_class - 1) / 2];
            double pred_result = svm_predict_values(model, x, dec_values);
            return pred_result;
        }

        public static double svm_predict_probability(Model model, Node[] x, double[] prob_estimates)
        {
            if ((model.Parameter.SvmType == Parameter.Svc || model.Parameter.SvmType == Parameter.NuSvc) &&
                model.ProbA != null && model.ProbB != null)
            {
                int i;
                int nr_class = model.NrClass;
                double[] dec_values = new double[nr_class * (nr_class - 1) / 2];
                svm_predict_values(model, x, dec_values);

                double min_prob = 1e-7;
                double[][] pairwise_prob = new double[nr_class][];

                int k = 0;
                for (i = 0; i < nr_class; i++)
                    for (int j = i + 1; j < nr_class; j++)
                    {
                        pairwise_prob[i][j] =
                            Math.Min(Math.Max(sigmoid_predict(dec_values[k], model.ProbA[k], model.ProbB[k]), min_prob),
                                1 - min_prob);
                        pairwise_prob[j][i] = 1 - pairwise_prob[i][j];
                        k++;
                    }
                if (nr_class == 2)
                {
                    prob_estimates[0] = pairwise_prob[0][1];
                    prob_estimates[1] = pairwise_prob[1][0];
                }
                else
                    multiclass_probability(nr_class, pairwise_prob, prob_estimates);

                int prob_max_idx = 0;
                for (i = 1; i < nr_class; i++)
                    if (prob_estimates[i] > prob_estimates[prob_max_idx])
                        prob_max_idx = i;
                return model.Label[prob_max_idx];
            }
            else
                return svm_predict(model, x);
        }

        public static double svm_predict_values(Model model, Node[] x, double[] dec_values)
        {
            int i;
            if (model.Parameter.SvmType == Parameter.OneClass ||
                model.Parameter.SvmType == Parameter.Epsilon ||
                model.Parameter.SvmType == Parameter.NuSvr)
            {
                double[] sv_coef = model.SvCoef[0];
                double sum = 0;
                for (i = 0; i < model.L; i++)
                    sum += sv_coef[i] * Kernel.k_function(x, model.SvNodes[i], model.Parameter);
                sum -= model.Rho[0];
                dec_values[0] = sum;

                if (model.Parameter.SvmType == Parameter.OneClass)
                    return (sum > 0) ? 1 : -1;
                else
                    return sum;
            }
            else
            {
                int nr_class = model.NrClass;
                int l = model.L;

                double[] kvalue = new double[l];
                for (i = 0; i < l; i++)
                    kvalue[i] = Kernel.k_function(x, model.SvNodes[i], model.Parameter);

                int[] start = new int[nr_class];
                start[0] = 0;
                for (i = 1; i < nr_class; i++)
                    start[i] = start[i - 1] + model.Nsv[i - 1];

                int[] vote = new int[nr_class];
                for (i = 0; i < nr_class; i++)
                    vote[i] = 0;

                int p = 0;
                for (i = 0; i < nr_class; i++)
                    for (int j = i + 1; j < nr_class; j++)
                    {
                        double sum = 0;
                        int si = start[i];
                        int sj = start[j];
                        int ci = model.Nsv[i];
                        int cj = model.Nsv[j];

                        int k;
                        double[] coef1 = model.SvCoef[j - 1];
                        double[] coef2 = model.SvCoef[i];
                        for (k = 0; k < ci; k++)
                            sum += coef1[si + k] * kvalue[si + k];
                        for (k = 0; k < cj; k++)
                            sum += coef2[sj + k] * kvalue[sj + k];
                        sum -= model.Rho[p];
                        dec_values[p] = sum;

                        if (dec_values[p] > 0)
                            ++vote[i];
                        else
                            ++vote[j];
                        p++;
                    }

                int vote_max_idx = 0;
                for (i = 1; i < nr_class; i++)
                    if (vote[i] > vote[vote_max_idx])
                        vote_max_idx = i;

                return model.Label[vote_max_idx];
            }
        }

        //svm_train
        public static Model Train(Problem problem, Parameter parameter)
        {
            Model model = new Model();
            model.Parameter = parameter;

            if (parameter.IsRegression())
            {
                // regression or one-class-svm
                model.NrClass = 2;
                model.Label = null;
                model.Nsv = null;
                model.ProbA = null;
                model.ProbB = null;
                model.SvCoef = new double[1][];

                if (parameter.IsGoodProbability())
                {
                    model.ProbA = new double[1];
                    model.ProbA[0] = svm_svr_probability(problem, parameter);
                }

                decision_function f = svm_train_one(problem, parameter, 0, 0);
                model.Rho = new double[1];
                model.Rho[0] = f.rho;

                int nSV = 0;
                int i;
                for (i = 0; i < problem.L; i++)
                    if (Math.Abs(f.alpha[i]) > 0) ++nSV;
                model.L = nSV;
                model.SvNodes = new Node[nSV][];
                model.SvCoef[0] = new double[nSV];
                model.SvIndicies = new int[nSV];
                int j = 0;
                for (i = 0; i < problem.L; i++)
                    if (Math.Abs(f.alpha[i]) > 0)
                    {
                        model.SvNodes[j] = problem.X[i];
                        model.SvCoef[0][j] = f.alpha[i];
                        model.SvIndicies[j] = i + 1;
                        ++j;
                    }
            }
            else
            {
                // classification
                int l = problem.L;
                int[] tmp_nr_class = new int[1];
                int[][] tmp_label = new int[1][];
                int[][] tmp_start = new int[1][];
                int[][] tmp_count = new int[1][];
                int[] perm = new int[l];

                // group training data of the same class
                svm_group_classes(problem, out tmp_nr_class, out tmp_label, out tmp_start, out tmp_count, perm);
                int nr_class = tmp_nr_class[0];
                int[] label = tmp_label[0];
                int[] start = tmp_start[0];
                int[] count = tmp_count[0];

                //if (nr_class == 1)
                //    svm.info("WARNING: training data in only one class. See README for details.\n");

                Node[][] x = new Node[l][];
                int i;
                for (i = 0; i < l; i++)
                    x[i] = problem.X[perm[i]];

                // calculate weighted C

                double[] weighted_C = new double[nr_class];
                for (i = 0; i < nr_class; i++)
                    weighted_C[i] = parameter.C;
                for (i = 0; i < parameter.NrWeight; i++)
                {
                    int j;
                    for (j = 0; j < nr_class; j++)
                        if (parameter.WeightLabel[i] == label[j])
                            break;
                    if (j == nr_class)
                    {
                    }
                    //System.err.print("WARNING: class label " + param.weight_label[i] + " specified in weight is not found\n");
                    else
                        weighted_C[j] *= parameter.Weight[i];
                }

                // train k*(k-1)/2 models

                bool[] nonzero = new bool[l];
                for (i = 0; i < l; i++)
                    nonzero[i] = false;
                decision_function[] f = new decision_function[nr_class * (nr_class - 1) / 2];

                double[] probA = null, probB = null;
                if (parameter.Probability == 1)
                {
                    probA = new double[nr_class * (nr_class - 1) / 2];
                    probB = new double[nr_class * (nr_class - 1) / 2];
                }

                int p = 0;
                for (i = 0; i < nr_class; i++)
                    for (int j = i + 1; j < nr_class; j++)
                    {
                        Problem sub_prob = new Problem();
                        int si = start[i], sj = start[j];
                        int ci = count[i], cj = count[j];
                        sub_prob.L = ci + cj;
                        sub_prob.X = new Node[sub_prob.L][];
                        sub_prob.Y = new double[sub_prob.L];
                        int k;
                        for (k = 0; k < ci; k++)
                        {
                            sub_prob.X[k] = x[si + k];
                            sub_prob.Y[k] = +1;
                        }
                        for (k = 0; k < cj; k++)
                        {
                            sub_prob.X[ci + k] = x[sj + k];
                            sub_prob.Y[ci + k] = -1;
                        }

                        if (parameter.Probability == 1)
                        {
                            double[] probAB = new double[2];
                            svm_binary_svc_probability(sub_prob, parameter, weighted_C[i], weighted_C[j], probAB);
                            probA[p] = probAB[0];
                            probB[p] = probAB[1];
                        }

                        f[p] = svm_train_one(sub_prob, parameter, weighted_C[i], weighted_C[j]);
                        for (k = 0; k < ci; k++)
                            if (!nonzero[si + k] && Math.Abs(f[p].alpha[k]) > 0)
                                nonzero[si + k] = true;
                        for (k = 0; k < cj; k++)
                            if (!nonzero[sj + k] && Math.Abs(f[p].alpha[ci + k]) > 0)
                                nonzero[sj + k] = true;
                        ++p;
                    }

                // build output

                model.NrClass = nr_class;

                model.Label = new int[nr_class];
                for (i = 0; i < nr_class; i++)
                    model.Label[i] = label[i];

                model.Rho = new double[nr_class * (nr_class - 1) / 2];
                for (i = 0; i < nr_class * (nr_class - 1) / 2; i++)
                    model.Rho[i] = f[i].rho;

                if (parameter.Probability == 1)
                {
                    model.ProbA = new double[nr_class * (nr_class - 1) / 2];
                    model.ProbB = new double[nr_class * (nr_class - 1) / 2];
                    for (i = 0; i < nr_class * (nr_class - 1) / 2; i++)
                    {
                        model.ProbA[i] = probA[i];
                        model.ProbB[i] = probB[i];
                    }
                }
                else
                {
                    model.ProbA = null;
                    model.ProbB = null;
                }

                int nnz = 0;
                int[] nz_count = new int[nr_class];
                model.Nsv = new int[nr_class];
                for (i = 0; i < nr_class; i++)
                {
                    int nSV = 0;
                    for (int j = 0; j < count[i]; j++)
                        if (nonzero[start[i] + j])
                        {
                            ++nSV;
                            ++nnz;
                        }
                    model.Nsv[i] = nSV;
                    nz_count[i] = nSV;
                }

                //svm.info("Total nSV = " + nnz + "\n");

                model.L = nnz;
                model.SvNodes = new Node[nnz][];
                model.SvIndicies = new int[nnz];
                p = 0;
                for (i = 0; i < l; i++)
                    if (nonzero[i])
                    {
                        model.SvNodes[p] = x[i];
                        model.SvIndicies[p++] = perm[i] + 1;
                    }

                int[] nz_start = new int[nr_class];
                nz_start[0] = 0;
                for (i = 1; i < nr_class; i++)
                    nz_start[i] = nz_start[i - 1] + nz_count[i - 1];

                model.SvCoef = new double[nr_class - 1][];
                for (i = 0; i < nr_class - 1; i++)
                    model.SvCoef[i] = new double[nnz];

                p = 0;
                for (i = 0; i < nr_class; i++)
                    for (int j = i + 1; j < nr_class; j++)
                    {
                        // classifier (i,j): coefficients with i are in sv_coef[j-1][nz_start[i]...],
                        // j are in sv_coef[i][nz_start[j]...]

                        int si = start[i];
                        int sj = start[j];
                        int ci = count[i];
                        int cj = count[j];

                        int q = nz_start[i];
                        int k;
                        for (k = 0; k < ci; k++)
                            if (nonzero[si + k])
                                model.SvCoef[j - 1][q++] = f[p].alpha[k];
                        q = nz_start[j];
                        for (k = 0; k < cj; k++)
                            if (nonzero[sj + k])
                                model.SvCoef[i][q++] = f[p].alpha[ci + k];
                        ++p;
                    }
            }
            return model;
        }

        public void Export(string modelFileName)
        {
        }

        public void Import(string modelFileName)
        {
        }

        public abstract double Predict(Node[] nodes);

        // Method 2 from the multiclass_prob paper by Wu, Lin, and Weng
        private static void multiclass_probability(int k, double[][] r, double[] p)
        {
            int t, j;
            int iter = 0, max_iter = Math.Max(100, k);
            double[][] Q = new double[k][];
            double[] Qp = new double[k];
            double pQp, eps = 0.005 / k;

            for (t = 0; t < k; t++)
            {
                p[t] = 1.0 / k; // Valid if k = 1
                Q[t][t] = 0;
                for (j = 0; j < t; j++)
                {
                    Q[t][t] += r[j][t] * r[j][t];
                    Q[t][j] = Q[j][t];
                }
                for (j = t + 1; j < k; j++)
                {
                    Q[t][t] += r[j][t] * r[j][t];
                    Q[t][j] = -r[j][t] * r[t][j];
                }
            }
            for (iter = 0; iter < max_iter; iter++)
            {
                // stopping condition, recalculate QP,pQP for numerical accuracy
                pQp = 0;
                for (t = 0; t < k; t++)
                {
                    Qp[t] = 0;
                    for (j = 0; j < k; j++)
                        Qp[t] += Q[t][j] * p[j];
                    pQp += p[t] * Qp[t];
                }
                double max_error = 0;
                for (t = 0; t < k; t++)
                {
                    double error = Math.Abs(Qp[t] - pQp);
                    if (error > max_error)
                        max_error = error;
                }
                if (max_error < eps) break;

                for (t = 0; t < k; t++)
                {
                    double diff = (-Qp[t] + pQp) / Q[t][t];
                    p[t] += diff;
                    pQp = (pQp + diff * (diff * Q[t][t] + 2 * Qp[t])) / (1 + diff) / (1 + diff);
                    for (j = 0; j < k; j++)
                    {
                        Qp[j] = (Qp[j] + diff * Q[t][j]) / (1 + diff);
                        p[j] /= (1 + diff);
                    }
                }
            }
        }

        private static double sigmoid_predict(double decision_value, double A, double B)
        {
            double fApB = decision_value * A + B;
            if (fApB >= 0)
                return Math.Exp(-fApB) / (1.0 + Math.Exp(-fApB));
            else
                return 1.0 / (1 + Math.Exp(fApB));
        }

        // Platt's binary SVM Probablistic Output: an improvement from Lin et al.
        private static void sigmoid_train(int l, double[] dec_values, double[] labels,
            double[] probAB)
        {
            double A, B;
            double prior1 = 0, prior0 = 0;
            int i;

            for (i = 0; i < l; i++)
                if (labels[i] > 0) prior1 += 1;
                else prior0 += 1;

            int max_iter = 100; // Maximal number of iterations
            double min_step = 1e-10; // Minimal step taken in line search
            double sigma = 1e-12; // For numerically strict PD of Hessian
            double eps = 1e-5;
            double hiTarget = (prior1 + 1.0) / (prior1 + 2.0);
            double loTarget = 1 / (prior0 + 2.0);
            double[] t = new double[l];
            double fApB, p, q, h11, h22, h21, g1, g2, det, dA, dB, gd, stepsize;
            double newA, newB, newf, d1, d2;
            int iter;

            // Initial Point and Initial Fun Value
            A = 0.0;
            B = Math.Log((prior0 + 1.0) / (prior1 + 1.0));
            double fval = 0.0;

            for (i = 0; i < l; i++)
            {
                if (labels[i] > 0) t[i] = hiTarget;
                else t[i] = loTarget;
                fApB = dec_values[i] * A + B;
                if (fApB >= 0)
                    fval += t[i] * fApB + Math.Log(1 + Math.Exp(-fApB));
                else
                    fval += (t[i] - 1) * fApB + Math.Log(1 + Math.Exp(fApB));
            }
            for (iter = 0; iter < max_iter; iter++)
            {
                // Update Gradient and Hessian (use H' = H + sigma I)
                h11 = sigma; // numerically ensures strict PD
                h22 = sigma;
                h21 = 0.0;
                g1 = 0.0;
                g2 = 0.0;
                for (i = 0; i < l; i++)
                {
                    fApB = dec_values[i] * A + B;
                    if (fApB >= 0)
                    {
                        p = Math.Exp(-fApB) / (1.0 + Math.Exp(-fApB));
                        q = 1.0 / (1.0 + Math.Exp(-fApB));
                    }
                    else
                    {
                        p = 1.0 / (1.0 + Math.Exp(fApB));
                        q = Math.Exp(fApB) / (1.0 + Math.Exp(fApB));
                    }
                    d2 = p * q;
                    h11 += dec_values[i] * dec_values[i] * d2;
                    h22 += d2;
                    h21 += dec_values[i] * d2;
                    d1 = t[i] - p;
                    g1 += dec_values[i] * d1;
                    g2 += d1;
                }

                // Stopping Criteria
                if (Math.Abs(g1) < eps && Math.Abs(g2) < eps)
                    break;

                // Finding Newton direction: -inv(H') * g
                det = h11 * h22 - h21 * h21;
                dA = -(h22 * g1 - h21 * g2) / det;
                dB = -(-h21 * g1 + h11 * g2) / det;
                gd = g1 * dA + g2 * dB;

                stepsize = 1; // Line Search
                while (stepsize >= min_step)
                {
                    newA = A + stepsize * dA;
                    newB = B + stepsize * dB;

                    // New function value
                    newf = 0.0;
                    for (i = 0; i < l; i++)
                    {
                        fApB = dec_values[i] * newA + newB;
                        if (fApB >= 0)
                            newf += t[i] * fApB + Math.Log(1 + Math.Exp(-fApB));
                        else
                            newf += (t[i] - 1) * fApB + Math.Log(1 + Math.Exp(fApB));
                    }
                    // Check sufficient decrease
                    if (newf < fval + 0.0001 * stepsize * gd)
                    {
                        A = newA;
                        B = newB;
                        fval = newf;
                        break;
                    }
                    else
                        stepsize = stepsize / 2.0;
                }

                if (stepsize < min_step)
                {
                    //svm.info("Line search fails in two-class probability estimates\n");
                    break;
                }
            }

            if (iter >= max_iter)
            {
            }
            //svm.info("Reaching maximal iterations in two-class probability estimates\n");
            probAB[0] = A;
            probAB[1] = B;
        }

        private static void solve_c_svc(Problem prob, Parameter param,
            double[] alpha, Solver.SolutionInfo si,
            double Cp, double Cn)
        {
            int l = prob.L;
            double[] minus_ones = new double[l];
            byte[] y = new byte[l];

            int i;

            for (i = 0; i < l; i++)
            {
                alpha[i] = 0;
                minus_ones[i] = -1;
                if (prob.Y[i] > 0) y[i] = +1;
                else y[i] = 1;
            }

            Solver s = new Solver();
            s.Solve(l, new SvcQ(prob, param, y), minus_ones, y,
                alpha, Cp, Cn, param.Eps, si, param.Shrinking);

            double sum_alpha = 0;
            for (i = 0; i < l; i++)
                sum_alpha += alpha[i];

            //if (Cp == Cn)
            //    svm.info("nu = " + sum_alpha / (Cp * prob.L) + "\n");

            for (i = 0; i < l; i++)
                alpha[i] *= y[i];
        }

        private static void solve_epsilon_svr(Problem prob, Parameter param,
            double[] alpha, Solver.SolutionInfo si)
        {
            int l = prob.L;
            double[] alpha2 = new double[2 * l];
            double[] linear_term = new double[2 * l];
            byte[] y = new byte[2 * l];
            int i;

            for (i = 0; i < l; i++)
            {
                alpha2[i] = 0;
                linear_term[i] = param.P - prob.Y[i];
                y[i] = 1;

                alpha2[i + l] = 0;
                linear_term[i + l] = param.P + prob.Y[i];
                y[i + l] = 1;
            }

            Solver s = new Solver();
            s.Solve(2 * l, new SvrQ(prob, param), linear_term, y,
                alpha2, param.C, param.C, param.Eps, si, param.Shrinking);

            double sum_alpha = 0;
            for (i = 0; i < l; i++)
            {
                alpha[i] = alpha2[i] - alpha2[i + l];
                sum_alpha += Math.Abs(alpha[i]);
            }
            //svm.info("nu = " + sum_alpha / (param.C * l) + "\n");
        }

        private static void solve_nu_svc(Problem prob, Parameter param,
                                                    double[] alpha, Solver.SolutionInfo si)
        {
            int i;
            int l = prob.L;
            double nu = param.Nu;

            byte[] y = new byte[l];

            for (i = 0; i < l; i++)
                if (prob.Y[i] > 0)
                    y[i] = +1;
                else
                    y[i] = 1;

            double sum_pos = nu * l / 2;
            double sum_neg = nu * l / 2;

            for (i = 0; i < l; i++)
                if (y[i] == +1)
                {
                    alpha[i] = Math.Min(1.0, sum_pos);
                    sum_pos -= alpha[i];
                }
                else
                {
                    alpha[i] = Math.Min(1.0, sum_neg);
                    sum_neg -= alpha[i];
                }

            double[] zeros = new double[l];

            for (i = 0; i < l; i++)
                zeros[i] = 0;

            SolverNu s = new SolverNu();
            s.SolveIt(l, new SvcQ(prob, param, y), zeros, y, alpha, 1.0, 1.0, param.Eps, si, param.Shrinking);
            double r = si.r;

            for (i = 0; i < l; i++)
                alpha[i] *= y[i] / r;

            si.rho /= r;
            si.obj /= (r * r);
            si.upper_bound_p = 1 / r;
            si.upper_bound_n = 1 / r;
        }

        private static void solve_nu_svr(Problem prob, Parameter param,
            double[] alpha, Solver.SolutionInfo si)
        {
            int l = prob.L;
            double C = param.C;
            double[] alpha2 = new double[2 * l];
            double[] linear_term = new double[2 * l];
            byte[] y = new byte[2 * l];
            int i;

            double sum = C * param.Nu * l / 2;
            for (i = 0; i < l; i++)
            {
                alpha2[i] = alpha2[i + l] = Math.Min(sum, C);
                sum -= alpha2[i];

                linear_term[i] = -prob.Y[i];
                y[i] = 1;

                linear_term[i + l] = prob.Y[i];
                y[i + l] = 1;
            }

            var s = new SolverNu();
            s.Solve(2 * l, new SvrQ(prob, param), linear_term, y,
                alpha2, C, C, param.Eps, si, param.Shrinking);

            //svm.info("epsilon = " + (-si.r) + "\n");

            for (i = 0; i < l; i++)
                alpha[i] = alpha2[i] - alpha2[i + l];
        }

        private static void solve_one_class(Problem prob, Parameter param,
            double[] alpha, Solver.SolutionInfo si)
        {
            int l = prob.L;
            double[] zeros = new double[l];
            byte[] ones = new byte[l];
            int i;

            int n = (int)(param.Nu * prob.L); // # of alpha's at upper bound

            for (i = 0; i < n; i++)
                alpha[i] = 1;
            if (n < prob.L)
                alpha[n] = param.Nu * prob.L - n;
            for (i = n + 1; i < l; i++)
                alpha[i] = 0;

            for (i = 0; i < l; i++)
            {
                zeros[i] = 0;
                ones[i] = 1;
            }

            Solver s = new Solver();
            s.Solve(l, new OneClassQ(prob, param), zeros, ones,
                alpha, 1.0, 1.0, param.Eps, si, param.Shrinking);
        }

        // Cross-validation decision values for probability estimates
        private static void svm_binary_svc_probability(Problem prob, Parameter param, double Cp, double Cn,
            double[] probAB)
        {
            int i;
            int nr_fold = 5;
            int[] perm = new int[prob.L];
            double[] dec_values = new double[prob.L];
            var rand = new Random();

            // random shuffle
            for (i = 0; i < prob.L; i++) perm[i] = i;
            for (i = 0; i < prob.L; i++)
            {
                int j = i + rand.Next(prob.L - i);
                do
                {
                    int tmp = perm[i];
                    perm[i] = perm[j];
                    perm[j] = tmp;
                } while (false);
            }
            for (i = 0; i < nr_fold; i++)
            {
                int begin = i * prob.L / nr_fold;
                int end = (i + 1) * prob.L / nr_fold;
                int j, k;
                Problem subprob = new Problem();

                subprob.L = prob.L - (end - begin);
                subprob.X = new Node[subprob.L][];
                subprob.Y = new double[subprob.L];

                k = 0;
                for (j = 0; j < begin; j++)
                {
                    subprob.X[k] = prob.X[perm[j]];
                    subprob.Y[k] = prob.Y[perm[j]];
                    ++k;
                }
                for (j = end; j < prob.L; j++)
                {
                    subprob.X[k] = prob.X[perm[j]];
                    subprob.Y[k] = prob.Y[perm[j]];
                    ++k;
                }
                int p_count = 0, n_count = 0;
                for (j = 0; j < k; j++)
                    if (subprob.Y[j] > 0)
                        p_count++;
                    else
                        n_count++;

                if (p_count == 0 && n_count == 0)
                    for (j = begin; j < end; j++)
                        dec_values[perm[j]] = 0;
                else if (p_count > 0 && n_count == 0)
                    for (j = begin; j < end; j++)
                        dec_values[perm[j]] = 1;
                else if (p_count == 0 && n_count > 0)
                    for (j = begin; j < end; j++)
                        dec_values[perm[j]] = -1;
                else
                {
                    Parameter subparam = param;
                    subparam.Probability = 0;
                    subparam.C = 1.0;
                    subparam.NrWeight = 2;
                    subparam.WeightLabel = new int[2];
                    subparam.Weight = new double[2];
                    subparam.WeightLabel[0] = +1;
                    subparam.WeightLabel[1] = -1;
                    subparam.Weight[0] = Cp;
                    subparam.Weight[1] = Cn;
                    Model submodel = Train(subprob, subparam);
                    for (j = begin; j < end; j++)
                    {
                        double[] dec_value = new double[1];
                        svm_predict_values(submodel, prob.X[perm[j]], dec_value);
                        dec_values[perm[j]] = dec_value[0];
                        // ensure +1 -1 order; reason not using CV subroutine
                        dec_values[perm[j]] *= submodel.Label[0];
                    }
                }
            }
            sigmoid_train(prob.L, dec_values, prob.Y, probAB);
        }

        // label: label name, start: begin of each class, count: #data of classes, perm: indices to
        //        the original data perm, length l, must be allocated before calling this subroutine
        private static void svm_group_classes(Problem prob, out int[] nr_class_ret, out int[][] label_ret, out int[][] start_ret,
            out int[][] count_ret, int[] perm)
        {
            int l = prob.L;
            int max_nr_class = 16;
            int nr_class = 0;
            int[] label = new int[max_nr_class];
            int[] count = new int[max_nr_class];
            int[] data_label = new int[l];
            int i;
            nr_class_ret = new int[1];
            label_ret = new int[1][];
            start_ret = new int[1][];
            count_ret = new int[1][];

            for (i = 0; i < l; i++)
            {
                int this_label = (int)(prob.Y[i]);
                int j;
                for (j = 0; j < nr_class; j++)
                {
                    if (this_label == label[j])
                    {
                        ++count[j];
                        break;
                    }
                }
                data_label[i] = j;
                if (j == nr_class)
                {
                    if (nr_class == max_nr_class)
                    {
                        max_nr_class *= 2;
                        int[] new_data = new int[max_nr_class];
                        Array.Copy(label, 0, new_data, 0, label.Length);
                        label = new_data;
                        new_data = new int[max_nr_class];
                        Array.Copy(count, 0, new_data, 0, count.Length);
                        count = new_data;
                    }
                    label[nr_class] = this_label;
                    count[nr_class] = 1;
                    ++nr_class;
                }
            }

            //
            // Labels are ordered by their first occurrence in the training set.
            // However, for two-class sets with -1/+1 labels and -1 appears first,
            // we swap labels to ensure that internally the binary SVM has positive data corresponding to the +1 instances.
            //
            if (nr_class == 2 && label[0] == -1 && label[1] == +1)
            {
                do { int tmp = label[0]; label[0] = label[1]; label[1] = tmp; } while (false);
                do { int tmp = count[0]; count[0] = count[1]; count[1] = tmp; } while (false);
                for (i = 0; i < l; i++)
                {
                    if (data_label[i] == 0)
                        data_label[i] = 1;
                    else
                        data_label[i] = 0;
                }
            }

            int[] start = new int[nr_class];
            start[0] = 0;
            for (i = 1; i < nr_class; i++)
                start[i] = start[i - 1] + count[i - 1];
            for (i = 0; i < l; i++)
            {
                perm[start[data_label[i]]] = i;
                ++start[data_label[i]];
            }
            start[0] = 0;
            for (i = 1; i < nr_class; i++)
                start[i] = start[i - 1] + count[i - 1];

            nr_class_ret[0] = nr_class;
            label_ret[0] = label;
            start_ret[0] = start;
            count_ret[0] = count;
        }

        // Return parameter of a Laplace distribution
        private static double svm_svr_probability(Problem prob, Parameter param)
        {
            int i;
            int nr_fold = 5;
            double[] ymv = new double[prob.L];
            double mae = 0;

            Parameter newParameter = new Parameter(param);

            newParameter.Probability = 0;
            svm_cross_validation(prob, newParameter, nr_fold, ymv);
            for (i = 0; i < prob.L; i++)
            {
                ymv[i] = prob.Y[i] - ymv[i];
                mae += Math.Abs(ymv[i]);
            }
            mae /= prob.L;
            double std = Math.Sqrt(2 * mae * mae);
            int count = 0;
            mae = 0;
            for (i = 0; i < prob.L; i++)
                if (Math.Abs(ymv[i]) > 5 * std)
                    count = count + 1;
                else
                    mae += Math.Abs(ymv[i]);
            mae /= (prob.L - count);
            //svm.info("Prob. model for test data: target value = predicted value + z,\nz: Laplace distribution e^(-|z|/sigma)/(2sigma),sigma=" + mae + "\n");
            return mae;
        }

        private static decision_function svm_train_one(
                                            Problem prob, Parameter param,
            double Cp, double Cn)
        {
            double[] alpha = new double[prob.L];
            Solver.SolutionInfo si = new Solver.SolutionInfo();
            switch (param.SvmType)
            {
                case Vector.Machine.Parameter.Svc:
                    solve_c_svc(prob, param, alpha, si, Cp, Cn);
                    break;

                case Vector.Machine.Parameter.NuSvc:
                    solve_nu_svc(prob, param, alpha, si);
                    break;

                case Vector.Machine.Parameter.OneClass:
                    solve_one_class(prob, param, alpha, si);
                    break;

                case Vector.Machine.Parameter.Epsilon:
                    solve_epsilon_svr(prob, param, alpha, si);
                    break;

                case Vector.Machine.Parameter.NuSvr:
                    solve_nu_svr(prob, param, alpha, si);
                    break;
            }

            //svm.info("obj = " + si.obj + ", rho = " + si.rho + "\n");

            // output SVs

            int nSV = 0;
            int nBSV = 0;
            for (int i = 0; i < prob.L; i++)
            {
                if (Math.Abs(alpha[i]) > 0)
                {
                    ++nSV;
                    if (prob.Y[i] > 0)
                    {
                        if (Math.Abs(alpha[i]) >= si.upper_bound_p)
                            ++nBSV;
                    }
                    else
                    {
                        if (Math.Abs(alpha[i]) >= si.upper_bound_n)
                            ++nBSV;
                    }
                }
            }

            //svm.info("nSV = " + nSV + ", nBSV = " + nBSV + "\n");

            decision_function f = new decision_function();
            f.alpha = alpha;
            f.rho = si.rho;
            return f;
        }
    }
}