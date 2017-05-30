﻿using System;

namespace Baxter.Vector.Machine
{
    public static class KernelHelper
    {
        /// <summary>
        /// Linear Kernel
        /// K(xi , xj ) = transpose(xi) * xj .
        /// </summary>
        /// <returns>A Linear Kernel</returns>
        public static Kernel LinearKernel()
        {
            return new Kernel(KernelType.Linear, 0, 0, 0);
        }

        /// <summary>
        /// Polynomial Kernel
        /// K(xi , xj ) = Pow( gamma * transpose(xi) * xj + r , d )
        /// </summary>
        /// <param name="degree">degree of the polynome</param>
        /// <param name="gamma">width parameter</param>
        /// <returns>Polynomial Kernel with parameters</returns>
        public static Kernel PolynomialKernel(int degree, double gamma, double r)
        {
            if (Math.Abs(gamma) < double.Epsilon)
                throw new ArgumentOutOfRangeException("gamma");
            if (degree < 2)
                throw new ArgumentOutOfRangeException("degree");

            return new Kernel(KernelType.Poly, gamma, r, degree);
        }

        /// <summary>
        /// RBF Kernel
        /// K(xi,yi) = exp( -gamma * || x - y ||² )
        /// </summary>
        /// <param name="gamma">width parameter</param>
        /// <returns>RBF kernel with parameters</returns>
        public static Kernel RadialBasisFunctionKernel(double gamma)
        {
            if (Math.Abs(gamma) < double.Epsilon)
                throw new ArgumentOutOfRangeException("gamma");

            return new Kernel(KernelType.Rbf, gamma, 0, 0);
        }

        /// <summary>
        ///Sigmoid Kernel
        ///K(xi , xj ) = tanh( gamma * transpose(xi) * xj + r)
        /// </summary>
        /// <param name="gamma">gamma parameter</param>
        /// <param name="r">r parameter</param>
        /// <returns>Sigmoid Kernel with parameters</returns>
        public static Kernel SigmoidKernel(double gamma, double r)
        {
            if (Math.Abs(gamma) < double.Epsilon)
                throw new ArgumentOutOfRangeException("gamma");

            return new Kernel(KernelType.Sigmoid, gamma, r, 0);
        }
    }
}