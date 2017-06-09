using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Vector.Machine
{
    // An SMO algorithm in Fan et al., JMLR 6(2005), p. 1889--1918 Solves:
    //
    // min 0.5(\alpha^T Q \alpha) + p^T \alpha
    //
    // y^T \alpha = \delta y_i = +1 or -1 0 <= alpha_i <= Cp for y_i = 1 0 <= alpha_i <= Cn for y_i = -1
    //
    // Given:
    //
    // Q, p, y, Cp, Cn, and an initial feasible point \alpha l is the size of vectors and matrices
    // eps is the stopping tolerance
    //
    // solution will be put in \alpha, objective value will be put in obj
    internal class Solver
    {
        public static byte FREE = 2;
        public static byte LOWER_BOUND = 0;
        public static byte UPPER_BOUND = 1;

        public static void Solve(Quandary quandary)
        {
            var solution = new Solution(quandary);

            var iter = 0;
            var max_iter = Math.Max(10000000, quandary.L > int.MaxValue / 100 ? int.MaxValue : 100 * quandary.L);
            var counter = Math.Min(quandary.L, 1000) + 1;

            while (iter < max_iter)
            {
                // show progress and do shrinking

                if (--counter == 0)
                {
                    counter = Math.Min(l, 1000);
                    if (quandary.Shrinking != 0)
                    {
                        solution = Shrink(solution);
                    }
                }

                if (select_working_set(solution) != 0)
                {
                    // reconstruct the whole gradient
                    reconstruct_gradient();
                    // reset active set size and check
                    active_size = l;
                    //svm.info("*");
                    if (select_working_set(out working_set) != 0)
                        break;
                    else
                        counter = 1;    // do shrinking next iteration
                }

                int i = working_set[0];
                int j = working_set[1];

                ++iter;

                // update alpha[i] and alpha[j], handle bounds carefully

                float[] Q_i = Q.get_Q(i, active_size);
                float[] Q_j = Q.get_Q(j, active_size);

                double C_i = get_C(i);
                double C_j = get_C(j);

                double old_alpha_i = alpha[i];
                double old_alpha_j = alpha[j];

                if (y[i] != y[j])
                {
                    double quad_coef = QD[i] + QD[j] + 2 * Q_i[j];
                    if (quad_coef <= 0)
                        quad_coef = 1e-12;
                    double delta = (-G[i] - G[j]) / quad_coef;
                    double diff = alpha[i] - alpha[j];
                    alpha[i] += delta;
                    alpha[j] += delta;

                    if (diff > 0)
                    {
                        if (alpha[j] < 0)
                        {
                            alpha[j] = 0;
                            alpha[i] = diff;
                        }
                    }
                    else
                    {
                        if (alpha[i] < 0)
                        {
                            alpha[i] = 0;
                            alpha[j] = -diff;
                        }
                    }
                    if (diff > C_i - C_j)
                    {
                        if (alpha[i] > C_i)
                        {
                            alpha[i] = C_i;
                            alpha[j] = C_i - diff;
                        }
                    }
                    else
                    {
                        if (alpha[j] > C_j)
                        {
                            alpha[j] = C_j;
                            alpha[i] = C_j + diff;
                        }
                    }
                }
                else
                {
                    double quad_coef = QD[i] + QD[j] - 2 * Q_i[j];
                    if (quad_coef <= 0)
                        quad_coef = 1e-12;
                    double delta = (G[i] - G[j]) / quad_coef;
                    double sum = alpha[i] + alpha[j];
                    alpha[i] -= delta;
                    alpha[j] += delta;

                    if (sum > C_i)
                    {
                        if (alpha[i] > C_i)
                        {
                            alpha[i] = C_i;
                            alpha[j] = sum - C_i;
                        }
                    }
                    else
                    {
                        if (alpha[j] < 0)
                        {
                            alpha[j] = 0;
                            alpha[i] = sum;
                        }
                    }
                    if (sum > C_j)
                    {
                        if (alpha[j] > C_j)
                        {
                            alpha[j] = C_j;
                            alpha[i] = sum - C_j;
                        }
                    }
                    else
                    {
                        if (alpha[i] < 0)
                        {
                            alpha[i] = 0;
                            alpha[j] = sum;
                        }
                    }
                }

                // update G

                double delta_alpha_i = alpha[i] - old_alpha_i;
                double delta_alpha_j = alpha[j] - old_alpha_j;

                for (int k = 0; k < active_size; k++)
                {
                    G[k] += Q_i[k] * delta_alpha_i + Q_j[k] * delta_alpha_j;
                }

                // update alpha_status and G_bar

                {
                    bool ui = IsUpperBound(i, solution);
                    bool uj = IsUpperBound(j, solution);
                    update_alpha_status(i);
                    update_alpha_status(j);
                    int k;
                    if (ui != is_upper_bound(i))
                    {
                        Q_i = Q.get_Q(i, l);
                        if (ui)
                            for (k = 0; k < l; k++)
                                G_bar[k] -= C_i * Q_i[k];
                        else
                            for (k = 0; k < l; k++)
                                G_bar[k] += C_i * Q_i[k];
                    }

                    if (uj != is_upper_bound(j))
                    {
                        Q_j = Q.get_Q(j, l);
                        if (uj)
                            for (k = 0; k < l; k++)
                                G_bar[k] -= C_j * Q_j[k];
                        else
                            for (k = 0; k < l; k++)
                                G_bar[k] += C_j * Q_j[k];
                    }
                }
            }

            if (iter >= max_iter)
            {
                if (active_size < l)
                {
                    // reconstruct the whole gradient to calculate objective value
                    reconstruct_gradient();
                    active_size = l;
                    //svm.info("*");
                }
                //System.err.print("\nWARNING: reaching max number of iterations\n");
            }

            // calculate rho

            si.rho = calculate_rho();

            // calculate objective value
            {
                double v = 0;
                int i;
                for (i = 0; i < l; i++)
                    v += alpha[i] * (G[i] + p[i]);

                si.obj = v / 2;
            }

            // put back the solution
            {
                for (int i = 0; i < l; i++)
                    alpha_[active_set[i]] = alpha[i];
            }

            si.upper_bound_p = Cp;
            si.upper_bound_n = Cn;

            //svm.info("\noptimization finished, #iter = " + iter + "\n");
        }

        protected static bool IsFree(int i, Solution solution)
        {
            return solution.AlphaStatus[i] == FREE;
        }

        protected static bool IsLowerBound(int i, Solution solution)
        {
            return solution.AlphaStatus[i] == LOWER_BOUND;
        }

        protected static bool IsUpperBound(int i, Solution solution)
        {
            return solution.AlphaStatus[i] == UPPER_BOUND;
        }

        protected static void ReconstructGradient(Solution solution)
        {
            // reconstruct inactive elements of G from G_bar and free variables

            if (solution.ActiveSize == solution.L) return;

            int i, j;
            int nr_free = 0;

            for (j = solution.ActiveSize; j < solution.L; j++)
                solution.G[j] = solution.GBar[j] + solution.P[j];

            for (j = 0; j < solution.ActiveSize; j++)
                if (IsFree(j, solution))
                    nr_free++;

            //if (2 * nr_free < active_size)
            //    svm.info("\nWARNING: using -h 0 may be faster\n");

            if (nr_free * solution.L > 2 * solution.ActiveSize * (solution.L - solution.ActiveSize))
            {
                for (i = solution.ActiveSize; i < solution.L; i++)
                {
                    float[] Q_i = solution.Q.get_Q(i, solution.ActiveSize);
                    for (j = 0; j < solution.ActiveSize; j++)
                        if (IsFree(j, solution))
                            solution.G[i] += solution.Alpha[j] * Q_i[j];
                }
            }
            else
            {
                for (i = 0; i < solution.ActiveSize; i++)
                    if (IsFree(i, solution))
                    {
                        float[] Q_i = solution.Q.get_Q(i, solution.L);
                        double alpha_i = solution.Alpha[i];
                        for (j = solution.ActiveSize; j < solution.L; j++)
                            solution.G[j] += alpha_i * Q_i[j];
                    }
            }
        }

        protected void swap_index(int i, int j)
        {
            Q.swap_index(i, j);
            do { byte tmp = y[i]; y[i] = y[j]; y[j] = tmp; } while (false);
            do { double tmp = G[i]; G[i] = G[j]; G[j] = tmp; } while (false);
            do { byte tmp = alpha_status[i]; alpha_status[i] = alpha_status[j]; alpha_status[j] = tmp; } while (false);
            do { double tmp = alpha[i]; alpha[i] = alpha[j]; alpha[j] = tmp; } while (false);
            do { double tmp = p[i]; p[i] = p[j]; p[j] = tmp; } while (false);
            do { int tmp = active_set[i]; active_set[i] = active_set[j]; active_set[j] = tmp; } while (false);
            do { double tmp = G_bar[i]; G_bar[i] = G_bar[j]; G_bar[j] = tmp; } while (false);
        }

        private bool be_shrunk(int i, Solution solution)
        {
            if (is_upper_bound(i))
            {
                if (solution.Y[i] == +1)
                    return (-solution.G[i] > solution.Gmax1);
                else
                    return (-solution.G[i] > solution.Gmax2);
            }
            else if (is_lower_bound(i))
            {
                if (solution.Y[i] == +1)
                    return (solution.G[i] > solution.Gmax2);
                else
                    return (solution.G[i] > solution.Gmax1);
            }
            else
                return (false);
        }

        private Solution CalculateRho(Solution solution)
        {
            double r;
            var nrFree = 0;
            var ub = double.PositiveInfinity;
            var lb = double.NegativeInfinity;
            double sumFree = 0;

            for (var i = 0; i < solution.ActiveSize; i++)
            {
                var yG = solution.Y[i] * solution.G[i];

                if (is_lower_bound(i))
                {
                    if (solution.Y[i] > 0)
                        ub = Math.Min(ub, yG);
                    else
                        lb = Math.Max(lb, yG);
                }
                else if (is_upper_bound(i))
                {
                    if (solution.Y[i] < 0)
                        ub = Math.Min(ub, yG);
                    else
                        lb = Math.Max(lb, yG);
                }
                else
                {
                    ++nrFree;
                    sumFree += yG;
                }
            }

            if (nrFree > 0)
                r = sumFree / nrFree;
            else
                r = (ub + lb) / 2;

            solution.Rho = r;
            return solution;
        }

        private static Solution Shrink(Solution solution)
        {
            int i;
            solution.Gmax1 = double.NegativeInfinity;        // max { -y_i * grad(f)_i | i in I_up(\alpha) }
            solution.Gmax2 = double.NegativeInfinity;        // max { y_i * grad(f)_i | i in I_low(\alpha) }

            // find maximal violating pair first
            for (i = 0; i < solution.ActiveSize; i++)
            {
                if (solution.Y[i] == +1)
                {
                    if (!is_upper_bound(i))
                    {
                        if (-solution.G[i] >= solution.Gmax1)
                            solution.Gmax1 = -solution.G[i];
                    }
                    if (!is_lower_bound(i))
                    {
                        if (solution.G[i] >= solution.Gmax2)
                            solution.Gmax2 = solution.G[i];
                    }
                }
                else
                {
                    if (!is_upper_bound(i))
                    {
                        if (-solution.G[i] >= solution.Gmax2)
                            solution.Gmax2 = -solution.G[i];
                    }
                    if (!is_lower_bound(i))
                    {
                        if (solution.G[i] >= solution.Gmax1)
                            solution.Gmax1 = solution.G[i];
                    }
                }
            }

            if (solution.Unshrink == false && solution.Gmax1 + solution.Gmax2 <= solution.Eps * 10)
            {
                solution.Unshrink = true;
                reconstruct_gradient();
                solution.ActiveSize = solution.L;
            }

            for (i = 0; i < solution.ActiveSize; i++)
            {
                if (be_shrunk(i, solution.Gmax1, solution.Gmax2))
                {
                    solution.ActiveSize--;
                    while (solution.ActiveSize > i)
                    {
                        if (!be_shrunk(solution.ActiveSize, solution.Gmax1, solution.Gmax2))
                        {
                            swap_index(i, solution.ActiveSize);
                            break;
                        }
                        solution.ActiveSize--;
                    }
                }
            }

            return solution;
        }

        // XXX
        private double get_C(int i)
        {
            return (y[i] > 0) ? Cp : Cn;
        }

        // return 1 if already optimal, return 0 otherwise
        private static int SelectWorkingSet(Solution solution)
        {
            // return i,j such that
            // i: maximizes -y_i * grad(f)_i, i in I_up(\alpha)
            // j: mimimizes the decrease of obj value (if quadratic coefficeint <= 0, replace it with tau)
            // -y_j*grad(f)_j < -y_i*grad(f)_i, j in I_low(\alpha)

            double Gmax = double.NegativeInfinity;
            double Gmax2 = double.NegativeInfinity;
            int Gmax_idx = -1;
            var Gmin_idx = -1;
            double obj_diff_min = double.PositiveInfinity;

            for (int t = 0; t < solution.ActiveSize; t++)
            {
                if (solution.Y[t] == +1)
                {
                    if (!IsUpperBound(t, solution))
                        if (-solution.G[t] >= Gmax)
                        {
                            Gmax = -solution.G[t];
                            Gmax_idx = t;
                        }
                }
                else
                {
                    if (!IsUpperBound(t, solution))
                        if (solution.G[t] >= Gmax)
                        {
                            Gmax = solution.G[t];
                            Gmax_idx = t;
                        }
                }
            }

            int i = Gmax_idx;
            float[] Q_i = null;
            if (i != -1) // null Q_i not accessed: Gmax=-INF if i=-1
                Q_i = solution.Q.get_Q(i, solution.ActiveSize);

            for (int j = 0; j < solution.ActiveSize; j++)
            {
                if (solution.Y[j] == +1)
                {
                    if (!IsUpperBound(j, solution))
                    {
                        double grad_diff = Gmax + solution.G[j];
                        if (solution.G[j] >= Gmax2)
                            Gmax2 = solution.G[j];
                        if (grad_diff > 0)
                        {
                            double obj_diff;
                            double quad_coef = solution.QD[i] + solution.QD[j] - 2.0 * solution.Y[i] * Q_i[j];
                            if (quad_coef > 0)
                                obj_diff = -(grad_diff * grad_diff) / quad_coef;
                            else
                                obj_diff = -(grad_diff * grad_diff) / 1e-12;

                            if (obj_diff <= obj_diff_min)
                            {
                                Gmin_idx = j;
                                obj_diff_min = obj_diff;
                            }
                        }
                    }
                }
                else
                {
                    if (!IsUpperBound(j, solution))
                    {
                        double grad_diff = Gmax - solution.G[j];
                        if (-solution.G[j] >= Gmax2)
                            Gmax2 = -solution.G[j];
                        if (grad_diff > 0)
                        {
                            double obj_diff;
                            double quad_coef = solution.QD[i] + solution.QD[j] + 2.0 * solution.Y[i] * Q_i[j];
                            if (quad_coef > 0)
                                obj_diff = -(grad_diff * grad_diff) / quad_coef;
                            else
                                obj_diff = -(grad_diff * grad_diff) / 1e-12;

                            if (obj_diff <= obj_diff_min)
                            {
                                Gmin_idx = j;
                                obj_diff_min = obj_diff;
                            }
                        }
                    }
                }
            }

            working_set = new int[2];
            working_set[0] = Gmax_idx;
            working_set[1] = Gmin_idx;

            if (Gmax + Gmax2 < eps || Gmin_idx == -1)
                return 1;

            return 0;
        }

        //UpdateRowSource alpa status

        // java: information about solution except alpha, because we cannot return multiple values otherwise...
        internal class SolutionInfo
        {
            public double obj;
            public double r;
            public double rho;
            public double upper_bound_n;
            public double upper_bound_p;

            // for Solver_NU
        }
    }
}