using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Baxter.Vector.Machine
{
    // An SMO algorithm in Fan et al., JMLR 6(2005), p. 1889--1918 Solves:
    //
    // min 0.5(\solution.Alpha^T Q \solution.Alpha) + p^T \solution.Alpha
    //
    // y^T \solution.Alpha = \delta y_i = +1 or -1 0 <= solution.Alpha_i <= Cp for y_i = 1 0 <=
    // solution.Alpha_i <= Cn for y_i = -1
    //
    // Given:
    //
    // Q, p, y, Cp, Cn, and an initial feasible point \solution.Alpha l is the size of vectors and
    // matrices eps is the stopping tolerance
    //
    // solution will be put in \solution.Alpha, objective value will be put in obj
    internal class Solver
    {
        public static byte FREE = 2;
        public static byte LOWER_BOUND = 0;
        public static byte UPPER_BOUND = 1;

        public static Solution Solve(Quandary quandary, SolutionInfo si)
        {
            var solution = new Solution(quandary);
            return SolveIt(solution);
        }

        public static Solution SolveIt(Solution solution)
        {
            var iter = 0;
            var max_iter = Math.Max(10000000, solution.L > int.MaxValue / 100 ? int.MaxValue : 100 * solution.L);
            var counter = Math.Min(solution.L, 1000) + 1;

            while (iter < max_iter)
            {
                // show progress and do shrinking

                if (--counter == 0)
                {
                    counter = Math.Min(solution.L, 1000);
                    if (solution.Shrinking != 0)
                    {
                        solution = Shrink(solution);
                    }
                }

                solution = SelectWorkingSet(solution);

                if (solution.WorkingSetStatus != 0)
                {
                    // reconstruct the whole gradient
                    solution = ReconstructGradient(solution);
                    // reset active set size and check
                    solution.ActiveSize = solution.L;
                    //svm.info("*");
                    if (solution.WorkingSetStatus != 0)
                        break;
                    else
                        counter = 1;    // do shrinking next iteration
                }

                var i = solution.WorkingSet[0];
                var j = solution.WorkingSet[1];

                ++iter;

                // update solution.Alpha[i] and solution.Alpha[j], handle bounds carefully

                var Q_i = solution.Q.get_Q(i, solution.ActiveSize);
                var Q_j = solution.Q.get_Q(j, solution.ActiveSize);

                var C_i = get_C(i, solution);
                var C_j = get_C(j, solution);

                var old_alpha_i = solution.Alpha[i];
                var old_alpha_j = solution.Alpha[j];

                if (solution.Y[i] != solution.Y[j])
                {
                    double quad_coef = solution.QD[i] + solution.QD[j] + 2 * Q_i[j];
                    if (quad_coef <= 0)
                        quad_coef = 1e-12;
                    double delta = (-solution.G[i] - solution.G[j]) / quad_coef;
                    double diff = solution.Alpha[i] - solution.Alpha[j];
                    solution.Alpha[i] += delta;
                    solution.Alpha[j] += delta;

                    if (diff > 0)
                    {
                        if (solution.Alpha[j] < 0)
                        {
                            solution.Alpha[j] = 0;
                            solution.Alpha[i] = diff;
                        }
                    }
                    else
                    {
                        if (solution.Alpha[i] < 0)
                        {
                            solution.Alpha[i] = 0;
                            solution.Alpha[j] = -diff;
                        }
                    }
                    if (diff > C_i - C_j)
                    {
                        if (solution.Alpha[i] > C_i)
                        {
                            solution.Alpha[i] = C_i;
                            solution.Alpha[j] = C_i - diff;
                        }
                    }
                    else
                    {
                        if (solution.Alpha[j] > C_j)
                        {
                            solution.Alpha[j] = C_j;
                            solution.Alpha[i] = C_j + diff;
                        }
                    }
                }
                else
                {
                    double quad_coef = solution.QD[i] + solution.QD[j] - 2 * Q_i[j];
                    if (quad_coef <= 0)
                        quad_coef = 1e-12;
                    double delta = (solution.G[i] - solution.G[j]) / quad_coef;
                    double sum = solution.Alpha[i] + solution.Alpha[j];
                    solution.Alpha[i] -= delta;
                    solution.Alpha[j] += delta;

                    if (sum > C_i)
                    {
                        if (solution.Alpha[i] > C_i)
                        {
                            solution.Alpha[i] = C_i;
                            solution.Alpha[j] = sum - C_i;
                        }
                    }
                    else
                    {
                        if (solution.Alpha[j] < 0)
                        {
                            solution.Alpha[j] = 0;
                            solution.Alpha[i] = sum;
                        }
                    }
                    if (sum > C_j)
                    {
                        if (solution.Alpha[j] > C_j)
                        {
                            solution.Alpha[j] = C_j;
                            solution.Alpha[i] = sum - C_j;
                        }
                    }
                    else
                    {
                        if (solution.Alpha[i] < 0)
                        {
                            solution.Alpha[i] = 0;
                            solution.Alpha[j] = sum;
                        }
                    }
                }

                // update G

                var delta_alpha_i = solution.Alpha[i] - old_alpha_i;
                var delta_alpha_j = solution.Alpha[j] - old_alpha_j;

                for (var k = 0; k < solution.ActiveSize; k++)
                {
                    solution.G[k] += Q_i[k] * delta_alpha_i + Q_j[k] * delta_alpha_j;
                }

                // update solution.AlphaStatus and solution.GBar

                {
                    var ui = IsUpperBound(i, solution);
                    var uj = IsUpperBound(j, solution);
                    solution = update_alpha_status(i, solution);
                    solution = update_alpha_status(j, solution);

                    int k;
                    if (ui != IsUpperBound(i, solution))
                    {
                        Q_i = solution.Q.get_Q(i, solution.L);
                        if (ui)
                            for (k = 0; k < solution.L; k++)
                                solution.GBar[k] -= C_i * Q_i[k];
                        else
                            for (k = 0; k < solution.L; k++)
                                solution.GBar[k] += C_i * Q_i[k];
                    }

                    if (uj != IsUpperBound(j, solution))
                    {
                        Q_j = solution.Q.get_Q(j, solution.L);
                        if (uj)
                            for (k = 0; k < solution.L; k++)
                                solution.GBar[k] -= C_j * Q_j[k];
                        else
                            for (k = 0; k < solution.L; k++)
                                solution.GBar[k] += C_j * Q_j[k];
                    }
                }
            }

            if (iter >= max_iter)
            {
                if (solution.ActiveSize < solution.L)
                {
                    // reconstruct the whole gradient to calculate objective value
                    ReconstructGradient(solution);
                    solution.ActiveSize = solution.L;
                }
            }

            // calculate rho

            solution = CalculateRho(solution);

            // calculate objective value
            {
                double v = 0;
                int i;
                for (i = 0; i < solution.L; i++)
                    v += solution.Alpha[i] * (solution.G[i] + solution.P[i]);

                solution.obj = v / 2;
            }

            // put back the solution
            {
                for (int i = 0; i < solution.L; i++)
                    solution.Alpha[solution.ActiveSet[i]] = solution.Alpha[i];
            }

            solution.upper_bound_p = solution.Cp;
            solution.upper_bound_n = solution.Cn;

            return solution;
        }

        public static Solution SwapIndex(int i, int j, Solution solution)
        {
            solution.Q.swap_index(i, j);
            do { var tmp = solution.Y[i]; solution.Y[i] = solution.Y[j]; solution.Y[j] = tmp; } while (false);
            do { var tmp = solution.G[i]; solution.G[i] = solution.G[j]; solution.G[j] = tmp; } while (false);
            do { var tmp = solution.AlphaStatus[i]; solution.AlphaStatus[i] = solution.AlphaStatus[j]; solution.AlphaStatus[j] = tmp; } while (false);
            do { var tmp = solution.Alpha[i]; solution.Alpha[i] = solution.Alpha[j]; solution.Alpha[j] = tmp; } while (false);
            do { var tmp = solution.P[i]; solution.P[i] = solution.P[j]; solution.P[j] = tmp; } while (false);
            do { var tmp = solution.ActiveSet[i]; solution.ActiveSet[i] = solution.ActiveSet[j]; solution.ActiveSet[j] = tmp; } while (false);
            do { var tmp = solution.GBar[i]; solution.GBar[i] = solution.GBar[j]; solution.GBar[j] = tmp; } while (false);

            return solution;
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

        protected static Solution ReconstructGradient(Solution solution)
        {
            // reconstruct inactive elements of G from solution.GBar and free variables

            if (solution.ActiveSize == solution.L)
            {
                return solution;
            }

            int i, j;
            int nr_free = 0;

            for (j = solution.ActiveSize; j < solution.L; j++)
                solution.G[j] = solution.GBar[j] + solution.P[j];

            for (j = 0; j < solution.ActiveSize; j++)
                if (IsFree(j, solution))
                    nr_free++;

            //if (2 * nr_free < solution.ActiveSize)
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

            return solution;
        }

        private static bool be_shrunk(int i, Solution solution)
        {
            if (IsUpperBound(i, solution))
            {
                if (solution.Y[i] == +1)
                    return (-solution.G[i] > solution.Gmax1);
                else
                    return (-solution.G[i] > solution.Gmax2);
            }
            else if (IsLowerBound(i, solution))
            {
                if (solution.Y[i] == +1)
                    return (solution.G[i] > solution.Gmax2);
                else
                    return (solution.G[i] > solution.Gmax1);
            }
            else
                return (false);
        }

        private static Solution CalculateRho(Solution solution)
        {
            double r;
            var nrFree = 0;
            var ub = double.PositiveInfinity;
            var lb = double.NegativeInfinity;
            double sumFree = 0;

            for (var i = 0; i < solution.ActiveSize; i++)
            {
                var yG = solution.Y[i] * solution.G[i];

                if (IsUpperBound(i, solution))
                {
                    if (solution.Y[i] > 0)
                        ub = Math.Min(ub, yG);
                    else
                        lb = Math.Max(lb, yG);
                }
                else if (IsUpperBound(i, solution))
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

        private static double CalculateRhoValue(Solution solution)
        {
            double r;
            var nrFree = 0;
            var ub = double.PositiveInfinity;
            var lb = double.NegativeInfinity;
            double sumFree = 0;

            for (var i = 0; i < solution.ActiveSize; i++)
            {
                var yG = solution.Y[i] * solution.G[i];

                if (IsUpperBound(i, solution))
                {
                    if (solution.Y[i] > 0)
                        ub = Math.Min(ub, yG);
                    else
                        lb = Math.Max(lb, yG);
                }
                else if (IsUpperBound(i, solution))
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

            return r;
        }

        // XXX
        private static double get_C(int i, Solution solution)
        {
            return (solution.Y[i] > 0) ? solution.Cp : solution.Cn;
        }

        // return 1 if already optimal, return 0 otherwise
        private static Solution SelectWorkingSet(Solution solution)
        {
            // return i,j such that
            // i: maximizes -y_i * grad(f)_i, i in I_up(\solution.Alpha)
            // j: mimimizes the decrease of obj value (if quadratic coefficeint <= 0, replace it with tau)
            // -y_j*grad(f)_j < -y_i*grad(f)_i, j in I_low(\solution.Alpha)

            var Gmax = double.NegativeInfinity;
            var Gmax2 = double.NegativeInfinity;
            var Gmax_idx = -1;
            var Gmin_idx = -1;
            var obj_diff_min = double.PositiveInfinity;

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

            var i = Gmax_idx;
            float[] Q_i = null;
            if (i != -1) // null Q_i not accessed: Gmax=-INF if i=-1
                Q_i = solution.Q.get_Q(i, solution.ActiveSize);

            for (var j = 0; j < solution.ActiveSize; j++)
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
                        var grad_diff = Gmax - solution.G[j];
                        if (-solution.G[j] >= Gmax2)
                            Gmax2 = -solution.G[j];
                        if (grad_diff > 0)
                        {
                            double obj_diff;
                            var quad_coef = solution.QD[i] + solution.QD[j] + 2.0 * solution.Y[i] * Q_i[j];
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

            solution.WorkingSet = new int[2];
            solution.WorkingSet[0] = Gmax_idx;
            solution.WorkingSet[1] = Gmin_idx;

            if (Gmax + Gmax2 < solution.Eps || Gmin_idx == -1)
            {
                solution.WorkingSetStatus = 1;
            }
            else
            {
                solution.WorkingSetStatus = 0;
            }

            return solution;
        }

        private static Solution Shrink(Solution solution)
        {
            int i;
            solution.Gmax1 = double.NegativeInfinity;        // max { -y_i * grad(f)_i | i in I_up(\solution.Alpha) }
            solution.Gmax2 = double.NegativeInfinity;        // max { y_i * grad(f)_i | i in I_low(\solution.Alpha) }

            // find maximal violating pair first
            for (i = 0; i < solution.ActiveSize; i++)
            {
                if (solution.Y[i] == +1)
                {
                    if (!IsUpperBound(i, solution))
                    {
                        if (-solution.G[i] >= solution.Gmax1)
                            solution.Gmax1 = -solution.G[i];
                    }
                    if (!IsLowerBound(i, solution))
                    {
                        if (solution.G[i] >= solution.Gmax2)
                            solution.Gmax2 = solution.G[i];
                    }
                }
                else
                {
                    if (!IsUpperBound(i, solution))
                    {
                        if (-solution.G[i] >= solution.Gmax2)
                            solution.Gmax2 = -solution.G[i];
                    }
                    if (!IsLowerBound(i, solution))
                    {
                        if (solution.G[i] >= solution.Gmax1)
                            solution.Gmax1 = solution.G[i];
                    }
                }
            }

            if (solution.Unshrink == false && solution.Gmax1 + solution.Gmax2 <= solution.Eps * 10)
            {
                solution.Unshrink = true;
                solution = ReconstructGradient(solution);
                solution.ActiveSize = solution.L;
            }

            for (i = 0; i < solution.ActiveSize; i++)
            {
                if (be_shrunk(i, solution))
                {
                    solution.ActiveSize--;
                    while (solution.ActiveSize > i)
                    {
                        if (!be_shrunk(solution.ActiveSize, solution))
                        {
                            solution = SwapIndex(i, solution.ActiveSize, solution);
                            break;
                        }
                        solution.ActiveSize--;
                    }
                }
            }

            return solution;
        }

        private static Solution update_alpha_status(int i, Solution solution)
        {
            if (solution.AlphaStatus[i] >= get_C(i, solution))
                solution.AlphaStatus[i] = UPPER_BOUND;
            else if (solution.Alpha[i] <= 0)
                solution.AlphaStatus[i] = LOWER_BOUND;
            else solution.AlphaStatus[i] = FREE;

            return solution;
        }

        //UpdateRowSource alpa status

        // java: information about solution except solution.Alpha, because we cannot return multiple
        //       values otherwise...
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