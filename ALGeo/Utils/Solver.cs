using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ALDGP
{
    class Solver
    {
        /// <summary>
        /// Solve the linear equation.
        /// </summary>
        /// <param name="solver">The solver type. 0 = SparseLU, 1 = SimplicialLLT.</param>
        /// <param name="M">Coefficient matrix</param>
        /// <param name="nnzM">The number of non-zero values in the coefficient matrix.</param>
        /// <param name="dim">The dimension of the coefficient matrix.</param>
        /// <param name="V">Right hand side matrix</param>
        /// <param name="nnzV">The number of non-zero values in the right hand side matrix.</param>
        /// <param name="m">The demension of the right hand side matrix.</param>
        /// <param name="X">The result matrix.</param>
        /// <returns></returns>
        [DllImport("LinearSolver.dll")]

        public static extern int Solve(int solver, Triplet[] M, int nnzM, int dim, Triplet[] V, int nnzV, int m, double[] X);

        [DllImport("LinearSolver.dll")]
        public static extern int LeastSquareMethod(Triplet[] M, int nnzM, int rowM, int colM, Triplet[] V, int nnzV, int colV, double[] X);
    }
}
