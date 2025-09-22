using DocumentFormat.OpenXml.Drawing.Diagrams;
using System;

namespace Calcpad.Core
{
    internal static class HpEigenSolver
    {
        internal static double[][] Lanczos(double[][] Matrix, int eigenCount, bool reverse, double tol, out double[] eigvals)
        {
            int n = Matrix.Length;
            var k = Math.Min(n, 4 * eigenCount + 100);
            var w = new double[n];
            var alpha = new double[k];
            var beta = new double[k + 1];
            // Initialize
            var V = new double[k + 2][];
            V[0] = new double[n]; // v0 = 0 (stays zero)
            V[1] = CreateRandomNormalizedVector(n, new Random(0));
            double[] previousTargetEigenvalues = null;
            int initialSteps = eigenCount, checkInterval = Math.Max(eigenCount / 8, 1);
            int m = 0;
            for (int j = 1; j <= k; ++j)
            {
                var j1 = j - 1;
                var V_j = V[j];
                SymmetricMatrixVectorProduct(Matrix, V_j, w);
                var sw = w.AsSpan();
                var vw = Vectorized.AsVector(sw);   
                // 3-term recurrence
                Vectorized.MultiplyAdd(V[j1], -beta[j1], sw, vw);
                alpha[j1] = Vectorized.DotProduct(w, V_j, 0, w.Length);
                Vectorized.MultiplyAdd(V[j], -alpha[j1], sw, vw);
                //Reothogonalization by modified Gram-Schmidt process
                for (int s = 1; s <= j; ++s)
                {
                    var dot = Vectorized.DotProduct(sw, V[s], 0, n);
                    Vectorized.MultiplyAdd(V[s], -dot, sw, vw);
                }
                beta[j] = Vectorized.Norm(sw);
                if (beta[j] < 1e-15)
                {
                    m = j;
                    break;
                }
                var j2 = j + 1;
                var V_j2 = new double[n];
                var invBeta_j = 1d / beta[j]; 
                for (int i = 0; i < n; ++i)
                    V_j2[i] = w[i] * invBeta_j;

                V[j2] = V_j2;
                m = j;
                // Kaniel-Paige convergence check: 
                // After initialSteps, check every checkInterval steps
                if (j >= initialSteps && (j - initialSteps) % checkInterval == 0)
                {
                    if (CheckKanielPaigeConvergence(alpha, beta, V, j, n, eigenCount, reverse,
                        tol, ref previousTargetEigenvalues))
                    {
                        m = j;
                        break;
                    }
                }
            }
            eigvals = EigenSolveTridiagonalSystem(alpha, beta, V, m, n, eigenCount, reverse, true, out var eigvecs);
            return eigvecs;
        }

        private static bool CheckKanielPaigeConvergence(double[] alpha, double[] beta, double[][] V,
        int currentDim, int n, int eigenCount, bool reverse, double tol, ref double[] prevEVals)
        {
            if (currentDim < Math.Max(2, eigenCount))
                return false;

            // Use existing method for eigenvalues of a tridiagonal system (no eigenvectors needed)
            var currEVals = EigenSolveTridiagonalSystem(alpha, beta, V, currentDim, n, eigenCount, reverse, false, out _);
            var hasConverged = false;
            if (prevEVals != null && prevEVals.Length == currEVals.Length)
            {
                // Kaniel-Paige convergence: target eigenvalues coincide within tolerance
                hasConverged = true;
                for (int i = 0; i < currEVals.Length; ++i)
                {
                    double err = Math.Abs(currEVals[i] - prevEVals[i]) / (1.0 + Math.Abs(currEVals[i]));
                    if (err > tol)
                    {
                        hasConverged = false;
                        break;
                    }
                }
                // Additional Kaniel-Paige check: β_m should be small relative to eigenvalue scale
                if (hasConverged)
                {
                    double betaM = Math.Abs(beta[currentDim]);
                    double maxEigMagnitude = 0.0;
                    for (int i = 0; i < currEVals.Length; ++i)
                        maxEigMagnitude = Math.Max(maxEigMagnitude, Math.Abs(currEVals[i]));

                    if (betaM > tol * maxEigMagnitude)
                        hasConverged = false;
                }
            }
            // Store for next comparison
            prevEVals = currEVals;
            return hasConverged;
        }

        private static double[] EigenSolveTridiagonalSystem(double[] alpha, double[] beta, double[][] V, int m, int n, int eigenCount, bool reverse, bool returnVecs, out double[][] eigvecs)
        {
            // Prepare for QL algorithm with m Lanczos steps
            if (eigenCount > m) eigenCount = m;
            var d = new double[m];
            alpha.AsSpan(0, m).CopyTo(d);
            var e = new double[m];
            beta.AsSpan(1, m - 1).CopyTo(e.AsSpan(1));
            // Initialize Q as jagged identity matrix
            var Q = new double[m][];
            for (int i = 0; i < m; ++i)
            {
                Q[i] = new double[m];
                Q[i][i] = 1d;
            }
            // Solve tridiagonal eigenvalue problem
            ImplicitQL(d, e, Q, true);
            var indexes = new int[m];
            for (int i = 0; i < m; ++i)
                indexes[i] = i;

            if (reverse)
                Array.Sort(indexes, (a, b) => d[b].CompareTo(d[a]));
            else
                Array.Sort(indexes, (a, b) => d[a].CompareTo(d[b]));

            var eigvals = new double[eigenCount];
            for (int i = 0; i < eigenCount; ++i)
                eigvals[i] = d[indexes[i]];

            if (returnVecs)
            {
                eigvecs = new double[n][];
                for (int l = 0; l < n; ++l)
                    eigvecs[l] = new double[eigenCount];

                var v = new double[n];
                for (int i = 0; i < eigenCount; ++i)
                {
                    int index = indexes[i];
                    var sv = v.AsSpan();
                    var vv = Vectorized.AsVector(sv);
                    sv.Clear();
                    for (int j = 0; j < m; ++j)
                        Vectorized.MultiplyAdd(V[j + 1], Q[j][index], sv, vv);

                    //Normalize v
                    var norm = Vectorized.Norm(sv);
                    if (norm != 0)
                        Vectorized.Scale(sv, 1d / norm);

                    //Assign to eigvecs
                    for (int j = 0; j < n; ++j)
                        eigvecs[j][i] = v[j];
                }
            }
            else
                eigvecs = null;

            return eigvals;
        }

        private static void SymmetricMatrixVectorProduct(double[][] A, double[] x, double[] y)
        {
            Array.Clear(y, 0, y.Length);
            var n = A.Length;
            for (int i = 0; i < n; ++i)
            {
                var A_i = A[i];
                var x_i = x[i];
                var sum = A_i[0] * x_i;
                for (int k = 1, len = A_i.Length; k < len; ++k)
                {
                    int j = i + k;
                    if (j < n)
                    {
                        double val = A_i[k];
                        sum += val * x[j];
                        y[j] += val * x_i; // Symmetric contribution
                    }
                }
                y[i] += sum;
            }
        }

        private static double[] CreateRandomNormalizedVector(int n, Random rnd)
        {
            var v = new double[n];
            for (int i = 0; i < n; ++i)
                v[i] = rnd.NextDouble() - 0.5;

            var norm = Vectorized.Norm(v);
            for (int i = 0; i < n; ++i)
                v[i] /= norm;

            return v;
        }


        /*
        Householder reduction of a real, symmetric matrix. On output, the orthogonal matrix Q
        is returned effecting the transformation. d returns the diagonal elements of the tridiagonal 
        matrix, and e[0..n] the off-diagonal elements, with e[0] = 0.

        The algorithm was adapted from:
        Numerical Recipes 3rd Edition: The Art of Scientific Computing (3rd. ed.).
        William H. Press, Saul A. Teukolsky, William T. Vetterling, and Brian P. Flannery. 2007. 
        Cambridge University Press, USA.
        */

        internal static double[][] Tridiagonalize(double[][] A, out double[] d, out double[] e, bool eigenvecs)
        {
            var n1 = A.Length;
            var n = n1 - 1;
            d = new double[n1];
            e = new double[n1];
            var Q = new double[n1][];
            var min = new int[n1];

            for (int i = 0; i <= n; ++i)
                Q[i] = new double[n1];

            for (int i = 0; i <= n; ++i)
            {
                var row = A[i];
                var m = row.Length - 1;
                for (int j = m; j >= 0; --j)
                {
                    var k = i + j;
                    if (i < min[k]) min[k] = i;
                    Q[k][i] = row[j];
                }
            }
            for (int i = n; i > 0; --i)
            {
                var Q_i = Q[i];
                var l = i - 1;
                var h = 0d;
                var m = min[i];
                if (l > 0)
                {
                    var scale = Vectorized.SumAbs(Q_i, m, i);
                    if (scale == 0d)            //Skip transformation.
                        e[i] = Q_i[l];
                    else
                    {
                        h = Vectorized.NormalizeAndSumSq(Q_i, m, i, scale);
                        var f = Q_i[l];
                        var g = f >= 0d ? -Math.Sqrt(h) : Math.Sqrt(h);
                        e[i] = scale * g;
                        h -= f * g;           // Now h is equation(11.2.4).
                        Q_i[l] = f - g;      // Store u in the ith row of Q.
                        for (int j = m; j <= l; ++j)
                        {
                            var Q_j = Q[j];
                            /* Next statement can be omitted if eigenvectors not wanted */
                            if (eigenvecs)
                                Q_j[i] = Q_i[j] / h; //Store u/H in ith column of Q.

                            g = Vectorized.DotProduct(Q_i, Q_j, 0, j + 1); //Form an element of Q · u in g.
                            for (int k = j + 1; k <= l; ++k)
                                g += Q[k][j] * Q_i[k];

                            e[j] = g / h;          //Form element of p in temporarily unused element of e.
                            if (j == m)
                                f = e[j] * Q_i[j];
                            else
                                f += e[j] * Q_i[j];
                        }
                        var hh = f / (h + h);      //Form K, equation (11.2.11).
                        for (int j = m; j <= l; ++j)
                        {
                            //Form q and store in e overwriting p.
                            f = Q_i[j];
                            e[j] = g = e[j] - hh * f;
                            var Q_j = Q[j];
                            for (int k = m; k <= j; k++) //Reduce Q, equation (11.2.13).
                                Q_j[k] -= (f * e[k] + g * Q_i[k]);
                        }
                    }
                }
                else
                    e[i] = Q_i[l];

                d[i] = h;
            }
            /* Next statement can be omitted if eigenvectors not wanted */
            if (eigenvecs)
                d[0] = 0d;

            e[0] = 0d;
            /* Contents of this loop can be omitted if eigenvectors not
            wanted except for statement d[i] = Q[i,i]; */
            if (eigenvecs)
                for (int i = 0; i <= n; ++i) // Begin accumulation of transformation matrices.
                {
                    var Q_i = Q[i];
                    var l = i - 1;
                    var m = min[i];
                    if (d[i] != 0d)
                    {
                        //This block skipped when i = 0.
                        for (int j = m; j <= l; ++j)
                        {
                            var g = Q_i[m] * Q[m][j];
                            for (int k = m + 1; k <= l; ++k) //Use u and u/H stored in a to form P·Q.
                                g += Q_i[k] * Q[k][j];

                            for (int k = 0; k <= l; ++k)
                                Q[k][j] -= g * Q[k][i];
                        }
                    }
                    d[i] = Q_i[i]; //This statement remains.
                    Q_i[i] = 1d; //Reset row and column of a to identity matrix for next iteration.
                    for (int j = 0; j <= l; j++)
                        Q[j][i] = Q_i[j] = 0d;
                }
            else
                for (int i = 0; i <= n; ++i) // Begin accumulation of transformation matrices.
                    d[i] = Q[i][i];          //This statement remains.

            return Q;

        }

        /*
        QL algorithm with implicit shifts, to determine the eigenvalues and eigenvectors of a 
        real, symmetric, tridiagonal matrix, or of a real, symmetric matrix previously reduced 
        by Tridiagonalize. On input, d[0..n] contains the diagonal elements of the tridiagonal matrix.
        On output, it returns the eigenvalues. The vector e[0..n] inputs the subdiagonal elements 
        of the tridiagonal matrix, with e[0] arbitrary. On output e is destroyed. When finding only 
        the eigenvalues, several lines may be omitted, as noted in the comments. 
        If the eigenvectors of a tridiagonal matrix are desired, the matrix Q[0..n][0..n] is input 
        as the identity matrix. If the eigenvectors of the matrix that has been reduced by Tridiagonalize 
        are required, then Q is input as the matrix output by Tridiagonalize. In either case, the kth column 
        of Q returns the normalized eigenvector corresponding to d[k].

        The algorithm was adapted from:
        Numerical Recipes 3rd Edition: The Art of Scientific Computing (3rd. ed.).
        William H. Press, Saul A. Teukolsky, William T. Vetterling, and Brian P. Flannery. 2007. 
        Cambridge University Press, USA.
        */
        internal static void ImplicitQL(double[] d, double[] e, double[][] Q, bool eigenvecs)
        {
            var n = d.Length - 1;
            //It is convenient to renumber the elements of e.
            for (int i = 0; i < n; ++i)
                e[i] = e[i + 1];

            e[n] = 0d;
            for (int l = 0; l <= n; ++l)
            {
                var iter = 0;
                int m;
                do
                {
                    m = l;
                    while (m < n) // Look for a single small subdiagonal element to split the matrix.
                    {
                        var dd = Math.Abs(d[m]) + Math.Abs(d[m + 1]);
                        if (Math.Abs(e[m]) <= 1e-15 * dd)
                            break;
                        ++m;
                    }
                    if (m != l)
                    {
                        if (++iter == 30)
                            throw Exceptions.QLAlgorithmFailed();

                        var l1 = l + 1;
                        var g = (d[l1] - d[l]) / (e[l] * 2d); //Form shift.
                        var r = HpLinearSolver.Hypot(g, 1d);
                        g = d[m] - d[l] + e[l] / (g + Math.CopySign(r, g)); //This is dm − ks.
                        var s = 1d;
                        var c = s;
                        var p = 0d;
                        var i = m - 1;
                        while (i >= l)     //A plane rotation as in the original QL, 
                        {                  //followed by Givens rotations to restore tridiagonal form
                            var i1 = i + 1;
                            var f = s * e[i];
                            var b = c * e[i];
                            r = HpLinearSolver.Hypot(f, g);
                            e[i1] = r;
                            if (r == 0d) //Recover from underflow.
                            {
                                d[i1] -= p;
                                e[m] = 0d;
                                break;
                            }
                            s = f / r;
                            c = g / r;
                            if (p == 0d)
                                g = d[i1];
                            else
                                g = d[i1] - p;

                            r = (d[i] - g) * s + c * b * 2d;
                            p = s * r;
                            d[i1] = g + p;
                            g = c * r - b;

                            // Next loop can be omitted if eigenvectors not wanted
                            if (eigenvecs)
                                for (int k = 0; k <= n; ++k)     // Form eigenvectors.
                                {
                                    var Q_k = Q[k];
                                    f = Q_k[i1];
                                    Q_k[i1] = s * Q_k[i] + c * f;
                                    Q_k[i] = c * Q_k[i] - s * f;
                                }

                            --i;
                        }
                        if (r == 0d && i >= l)
                            continue;

                        d[l] -= p;
                        e[l] = g;
                        e[m] = 0d;
                    }
                } while (m != l);
            }
        }
    }
}