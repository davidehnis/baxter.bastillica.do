using System;

namespace Baxter.Vector.Machine
{
    // Solver for nu-svm classification and regression
    //
    // additional constraint: e^T \alpha = constant
    internal class SolverNu : Solver
    {
        public static Solution do_shrinking(Solution solution)
        {
            var Gmax1 = double.NegativeInfinity;    // max { -y_i * grad(f)_i | y_i = +1, i in I_up(\alpha) }
            var Gmax2 = double.NegativeInfinity;    // max { y_i * grad(f)_i | y_i = +1, i in I_low(\alpha) }
            var Gmax3 = double.NegativeInfinity;    // max { -y_i * grad(f)_i | y_i = -1, i in I_up(\alpha) }
            var Gmax4 = double.NegativeInfinity;    // max { y_i * grad(f)_i | y_i = -1, i in I_low(\alpha) }

            // find maximal violating pair first
            int i;
            for (i = 0; i < solution.ActiveSize; i++)
            {
                if (!IsUpperBound(i, solution))
                {
                    if (solution.Y[i] == +1)
                    {
                        if (-solution.G[i] > Gmax1) Gmax1 = -solution.G[i];
                    }
                    else if (-solution.G[i] > Gmax4) Gmax4 = -solution.G[i];
                }
                if (!IsLowerBound(i, solution))
                {
                    if (solution.Y[i] == +1)
                    {
                        if (solution.G[i] > Gmax2) Gmax2 = solution.G[i];
                    }
                    else if (solution.G[i] > Gmax3) Gmax3 = solution.G[i];
                }
            }

            if (solution.Unshrink == false && Math.Max(Gmax1 + Gmax2, Gmax3 + Gmax4) <= solution.Eps * 10)
            {
                solution.Unshrink = true;
                solution = ReconstructGradient(solution);
                solution.ActiveSize = solution.L;
            }

            for (i = 0; i < solution.ActiveSize; i++)
            {
                if (be_shrunk(i, ref solution))
                {
                    solution.ActiveSize--;
                    while (solution.ActiveSize > i)
                    {
                        if (!be_shrunk(solution.ActiveSize, ref solution))
                        {
                            SwapIndex(i, solution.ActiveSize, solution);
                            break;
                        }
                        solution.ActiveSize--;
                    }
                }
            }

            return solution;
        }

        public static Solution Solve(Solution solution)
        {
            solution = Solver.SolveIt(solution);
            return solution;
        }

        public Solution SolveIt(int l, QMatrix Q, double[] p, byte[] y,
            double[] alpha, double Cp, double Cn, double Eps,
            SolutionInfo si, int shrinking)
        {
            var solution = new Solution(l, Q, p, y, alpha, Cp, Cn, Eps, shrinking);
            return Solve(solution);
        }

        private static bool be_shrunk(int i, ref Solution solution)
        {
            if (IsUpperBound(i, solution))
            {
                if (solution.Y[i] == +1)
                    return (-solution.G[i] > solution.Gmax1);
                else
                    return (-solution.G[i] > solution.Gmax4);
            }
            else if (IsLowerBound(i, solution))
            {
                if (solution.Y[i] == +1)
                    return (solution.G[i] > solution.Gmax2);
                else
                    return (solution.G[i] > solution.Gmax3);
            }
            else
                return (false);
        }

        private static Solution calculate_rho(Solution solution)
        {
            var nr_free1 = 0;
            var nr_free2 = 0;
            var ub1 = double.PositiveInfinity;
            var ub2 = double.PositiveInfinity;
            double lb1 = double.NegativeInfinity, lb2 = double.NegativeInfinity;
            double sum_free1 = 0, sum_free2 = 0;

            for (int i = 0; i < solution.ActiveSize; i++)
            {
                if (solution.Y[i] == +1)
                {
                    if (IsLowerBound(i, solution))
                        ub1 = Math.Min(ub1, solution.G[i]);
                    else if (IsUpperBound(i, solution))
                        lb1 = Math.Max(lb1, solution.G[i]);
                    else
                    {
                        ++nr_free1;
                        sum_free1 += solution.G[i];
                    }
                }
                else
                {
                    if (IsLowerBound(i, solution))
                        ub2 = Math.Min(ub2, solution.G[i]);
                    else if (IsUpperBound(i, solution))
                        lb2 = Math.Max(lb2, solution.G[i]);
                    else
                    {
                        ++nr_free2;
                        sum_free2 += solution.G[i];
                    }
                }
            }

            double r1, r2;
            if (nr_free1 > 0)
                r1 = sum_free1 / nr_free1;
            else
                r1 = (ub1 + lb1) / 2;

            if (nr_free2 > 0)
                r2 = sum_free2 / nr_free2;
            else
                r2 = (ub2 + lb2) / 2;

            solution.r = (r1 + r2) / 2;
            solution.Rho = (r1 - r2) / 2;

            return solution;
        }

        // return 1 if already optimal, return 0 otherwise
        private static int select_working_set(int[] working_set, Solution solution)
        {
            // return i,j such that y_i = y_j and
            // i: maximizes -y_i * grad(f)_i, i in I_up(\alpha)
            // j: minimizes the decrease of obj value (if quadratic coefficeint <= 0, replace it with tau)
            // -y_j*grad(f)_j < -y_i*grad(f)_i, j in I_low(\alpha)

            double Gmaxp = double.NegativeInfinity;
            double Gmaxp2 = double.NegativeInfinity;
            int Gmaxp_idx = -1;

            double Gmaxn = double.NegativeInfinity;
            double Gmaxn2 = double.NegativeInfinity;
            int Gmaxn_idx = -1;

            int Gmin_idx = -1;
            double obj_diff_min = double.PositiveInfinity;

            for (int t = 0; t < solution.ActiveSize; t++)
                if (solution.Y[t] == +1)
                {
                    if (!IsUpperBound(t, solution))
                        if (-solution.G[t] >= Gmaxp)
                        {
                            Gmaxp = -solution.G[t];
                            Gmaxp_idx = t;
                        }
                }
                else
                {
                    if (!IsLowerBound(t, solution))
                        if (solution.G[t] >= Gmaxn)
                        {
                            Gmaxn = solution.G[t];
                            Gmaxn_idx = t;
                        }
                }

            int ip = Gmaxp_idx;
            int idxN = Gmaxn_idx;
            float[] Q_ip = null;
            float[] Q_in = null;
            if (ip != -1) // null Q_ip not accessed: Gmaxp=double.NegativeInfinity if ip=-1
                Q_ip = solution.Q.get_Q(ip, solution.ActiveSize);
            if (idxN != -1)
                Q_in = solution.Q.get_Q(idxN, solution.ActiveSize);

            for (int j = 0; j < solution.ActiveSize; j++)
            {
                if (solution.Y[j] == +1)
                {
                    if (!IsLowerBound(j, solution))
                    {
                        double grad_diff = Gmaxp + solution.G[j];
                        if (solution.G[j] >= Gmaxp2)
                            Gmaxp2 = solution.G[j];
                        if (grad_diff > 0)
                        {
                            double obj_diff;
                            double quad_coef = solution.QD[ip] + solution.QD[j] - 2 * Q_ip[j];
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
                        double grad_diff = Gmaxn - solution.G[j];
                        if (-solution.G[j] >= Gmaxn2)
                            Gmaxn2 = -solution.G[j];
                        if (grad_diff > 0)
                        {
                            double obj_diff;
                            double quad_coef = solution.QD[idxN] + solution.QD[j] - 2 * Q_in[j];
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

            if (Math.Max(Gmaxp + Gmaxp2, Gmaxn + Gmaxn2) < solution.Eps || Gmin_idx == -1)
                return 1;

            if (solution.Y[Gmin_idx] == +1)
                working_set[0] = Gmaxp_idx;
            else
                working_set[0] = Gmaxn_idx;
            working_set[1] = Gmin_idx;

            return 0;
        }
    }
}