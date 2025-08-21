using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Buffers;

namespace Calcpad.Core
{
    internal class HpLinearSolver
    {
        private readonly HpSymmetricMatrix _matrix;

        internal HpLinearSolver(HpSymmetricMatrix matrix)
        {
            _matrix = matrix;
        }

        //Solves A*x = b via preconditioned conjugate gradient method
        internal double[] SolvePCG(double[] b, double tol)
        {
            int n = _matrix.RowCount;
            var x = new double[n];
            var r = new double[n];
            var z = new double[n];
            var p = new double[n];
            var Ap = new double[n];
            var diag = new double[n];
            //Apply Jacobi preconditioner
            for (int i = 0; i < n; ++i)
            {
                var row = _matrix.HpRows[i];
                if (row.Size == 0)
                    throw Exceptions.MatrixSingular();

                diag[i] = 1.0 / row.Raw[0];// M^-1 = 1/diag(A)
            }


            b.AsSpan().CopyTo(r);
            SolvePCGWithArrays(r, x, z, p, Ap, diag, tol);
            return x;
        }

        //Solves A*X = B where B is transposed for efficiency
        //Preconditioned conjugate gradient method is used
        internal double[][] MSolvePCG(double[][] B, double tol)
        {
            var n = _matrix.RowCount;
            var nB = B.Length;
            var X = new double[nB][];
            var rows = _matrix.HpRows;
            var diag = new double[n];
            var pool = ArrayPool<double>.Shared;

            //Apply Jacobi preconditioner
            for (int i = 0; i < n; ++i)
                diag[i] = 1.0 / rows[i].Raw[0];  // M^-1 = 1/diag(A)

            for (int i = 0; i < nB; ++i)
                X[i] = new double[n];            // initialize X rows

            Parallel.For(0, nB, j =>
            {
                // Rent working arrays from the pool
                var x = pool.Rent(n);
                var r = pool.Rent(n);
                var z = pool.Rent(n);
                var p = pool.Rent(n);
                var Ap = pool.Rent(n);
                try
                {
                    // Extract column from B  
                    B[j].AsSpan().CopyTo(r);
                    // Solve with pre-computed diagonal
                    SolvePCGWithArrays(r, x, z, p, Ap, diag, tol);
                    var X_j = X[j];
                    for (int i = 0; i < n; ++i)   // Store result
                        X_j[i] = x[i];
                }
                finally
                {
                    // Return arrays to the pool
                    pool.Return(x);
                    pool.Return(r);
                    pool.Return(z);
                    pool.Return(p);
                    pool.Return(Ap);
                }
            });
            return X;
        }

        // Helper: PCG solver using provided work arrays
        private void SolvePCGWithArrays(double[] r, double[] x, double[] z, double[] p, double[] Ap,
                                        double[] diag, double tol)
        {
            var n = _matrix.RowCount;
            var maxIterations = Math.Min(2 * n, 1000);
            Vectorized.Multiply(diag, r, z);  // z0 = M^-1 * r0
            z.AsSpan().CopyTo(p); // p0 = z0
            var rz_old = Vectorized.DotProduct(r, z, 0, n);
            var tol_sq = tol * tol;
            int iter = 0;
            Array.Clear(x, 0, n);
            for (; iter < maxIterations; ++iter)
            {
                // Check convergence (using ||r||^2)
                var rnorm_sq = Vectorized.SumSq(r, 0, n);
                if (rnorm_sq < tol_sq)
                    break; //Converged successfully

                SymmetricMatrixVectorProduct(p, Ap);
                var pAp = Vectorized.DotProduct(p, Ap, 0, n);
                if (Math.Abs(pAp) < 1e-15) //Check for numerical stability
                    throw new MathParserException(Messages.TheMatrixIsIllConditioned);

                var alpha = rz_old / pAp;
                Vectorized.MultiplyAdd(p, alpha, x);    // x = x + alpha * p
                Vectorized.MultiplyAdd(Ap, -alpha, r);  // r = r - alpha * Ap
                Vectorized.Multiply(diag, r, z);  // z = M^-1 * r
                var rznew = Vectorized.DotProduct(r, z, 0, n);
                var beta = rznew / rz_old;
                Vectorized.Scale(p, beta);   // p = beta * p
                Vectorized.Add(z, p);        // p = z + p
                rz_old = rznew;
            }
            if (iter == maxIterations)
                throw new MathParserException(string.Format(Messages.ThePCGSolverFailedToConvergeIn_0_Iterations, maxIterations));
        }

        private void SymmetricMatrixVectorProduct(double[] x, double[] y)
        {
            Array.Clear(y, 0, y.Length);
            var m = _matrix.RowCount;
            var n = x.Length;
            for (int i = 0; i < m; ++i)
            {
                var row = _matrix.HpRows[i];
                var raw = row.Raw;
                var x_i = x[i];
                var sum = raw[0] * x_i;
                for (int k = 1, size = row.Size; k < size; ++k)
                {
                    var j = i + k;
                    if (j < n)
                    {
                        var val = raw[k];
                        sum += val * x[j];
                        y[j] += val * x_i; // Symmetric contribution
                    }
                }
                y[i] += sum;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double Sqr(in double x) => x * x;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double Hypot(in double a, in double b)
        {
            var a1 = Math.Abs(a);
            var b1 = Math.Abs(b);
            if (a1.CompareTo(b1) > 0)
                return a1 * Math.Sqrt(1 + Sqr(b1 / a1));
            else
                return b1 == 0d ? 0d : b1 * Math.Sqrt(1d + Sqr(a1 / b1));
        }
    }
}
