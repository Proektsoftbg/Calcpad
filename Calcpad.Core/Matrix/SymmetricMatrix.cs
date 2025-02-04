using System;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class SymmetricMatrix : Matrix
    {
        internal SymmetricMatrix(int length) : base(length)
        {
            _type = MatrixType.Symmetric;
            _rows = new Vector[length];
            for (int i = length - 1; i >= 0; --i)
                _rows[i] = new LargeVector(length - i);
        }

        internal override RealValue this[int row, int col]
        {
            get => row <= col ?
                _rows[row][col - row] :
                _rows[col][row - col];
            set
            {
                if (row <= col)
                    _rows[row][col - row] = value;
                else
                    _rows[col][row - col] = value;
            }
        }

        private SymmetricMatrix RawCopy()
        {
            var n = _rows.Length;
            var M = new SymmetricMatrix(n);
            for (int i = n - 1; i >= 0; --i)
                M._rows[i] = _rows[i].Copy();

            return M;
        }

        internal override Matrix Clone() => new SymmetricMatrix(_rowCount);

        internal SymmetricMatrix Resize(int n)
        {
            if (n != _rowCount)
            {
                var n1 = Math.Min(n, _rowCount) - 1;
                Array.Resize(ref _rows, n);
                for (int i = _rowCount; i < n; ++i)
                    _rows[i] = new LargeVector(n - i);

                for (int i = 0; i <= n1; ++i)
                    _rows[i].Resize(n - i);

                _rowCount = n;
                _colCount = n;
            }
            return this;
        }

        internal UpperTriangularMatrix CholeskyDecomposition()
        {
            var U = GetCholesky();
            if (U is null)
                Throw.MatrixNotPositiveDefinite();

            return U;
        }


        private UpperTriangularMatrix GetCholesky()
        {
            UpperTriangularMatrix U = new(_rowCount);
            var maxsize = 0;
            for (int i = 0; i < _rowCount; ++i)
            {
                var row = _rows[i];
                if (row.Size > maxsize)
                    maxsize = row.Size;

                RealValue sum = row[0];
                for (int k = i - 1; k >= 0; --k)
                    sum -= RealValue.Pow2(U[k, i]);

                if (sum.D <= 0d)
                    return null;

                var U_i = U._rows[i];
                var n = _rowCount - i;
                if (n > maxsize)
                    n = maxsize;

                --maxsize;
                var U_ii = RealValue.Sqrt(sum);
                var d = 1d / U_ii.D;
                var u = U_ii.Units;
                for (int j = 1; j < n; ++j)
                {
                    sum = row[j];
                    for (int k = i - 1; k >= 0; --k)
                    {
                        var U_k = U._rows[k];
                        var ik = i - k;
                        sum -= U_k[ik] * U_k[ik + j];
                    }
                    if (u is null)
                        U_i[j] = sum * d;
                    else
                        U_i[j] = sum / U_ii;
                }
                U_i[0] = U_ii;
            }
            return U;
        }

        //Get LDLT decomposition by storing LT in U
        //and D in the main diagonal of U 
        private UpperTriangularMatrix GetLDLT()
        {
            UpperTriangularMatrix U = new(_rowCount);
            var maxsize = 0;
            for (int i = 0; i < _rowCount; ++i)
            {
                var row = _rows[i];
                if (row.Size > maxsize)
                    maxsize = row.Size;

                var sum = row[0];
                for (int k = i - 1; k >= 0; --k)
                    sum -= RealValue.Pow2(U[k, i]) * U[k, k];

                if (sum.D == 0d)
                    return null;

                var U_i = U._rows[i];
                var n = _rowCount - i;
                if (n > maxsize)
                    n = maxsize;

                --maxsize;
                var U_ii = sum;
                var u = sum.Units;
                double d = 1d;
                if ( u is null)
                    d = 1d / sum.D;
                
                for (int j = 1; j < n; ++j)
                {
                    sum = row[j];
                    for (int k = i - 1; k >= 0; --k)
                    {
                        var U_k = U._rows[k];
                        var ik = i - k;
                        sum -= U_k[ik] * U_k[ik + j] * U[k, k];
                    }
                    if (u is null)
                        U_i[j] = sum * d;
                    else
                        U_i[j] = sum / U_ii;
                }
                U_i[0] = U_ii;
            }
            return U;
        }

        internal override RealValue Determinant()
        {
            var U = GetLDLT();
            if (U is null)
                return RealValue.Zero;
            return GetDeterminant(U);
        }

        private RealValue GetDeterminant(UpperTriangularMatrix U)
        {
            var det = U[0, 0];
            for (int i = 1; i < _rowCount; ++i)
                det *= U[i, i];

            return det;
        }

        internal override Vector LSolve(Vector v)
        {
            var U = GetLDLT();
            if (U is null)
                Throw.MatrixSingularException();

            var x = new RealValue[_rowCount];
            FwdAndBackSubst(U, v, ref x);
            return new(x);
        }

        internal Vector ClSolve(Vector v)
        {
            var U = GetCholesky();
            if (U is null)
                Throw.MatrixNotPositiveDefinite();

            var x = new RealValue[_rowCount];
            CholeskyFwdAndBackSubst(U, v, ref x);
            return new(x);
        }

        internal override Matrix MSolve(Matrix M)
        {
            var U = GetLDLT();
            if (U is null)
                Throw.MatrixSingularException();

            var m = _rowCount;
            var n = M.ColCount;
            var v = new Vector[n];
            Parallel.For(0, n, j =>
            {
                var x = new RealValue[m];
                FwdAndBackSubst(U, M.Col(j + 1), ref x);
                v[j] = new(x);
            });
            return CreateFromCols(v, m);
        }


        internal virtual Matrix CmSolve(Matrix M)
        {
            var U = GetCholesky();
            if (U is null)
                Throw.MatrixNotPositiveDefinite();

            var m = _rowCount;
            var n = M.ColCount;
            var v = new Vector[n];
            Parallel.For(0, n, j =>
            {
                var x = new RealValue[m];
                CholeskyFwdAndBackSubst(U, M.Col(j + 1), ref x);
                v[j] = new(x);
            });
            return CreateFromCols(v, m);
        }

        internal override Matrix Invert()
        {
            var U = GetLDLT(); //Decompose the matrix by LDLT decomp.
            if (U is null)
                Throw.MatrixSingularException();

            return GetInverse(U);
        }

        private SymmetricMatrix GetInverse(UpperTriangularMatrix U)
        {
            var vector = new Vector(_rowCount);
            var x = new RealValue[_rowCount];
            var M = new SymmetricMatrix(_rowCount);
            for (int j = 0; j < _rowCount; ++j)  //Find inverse by columns.
            {
                vector[j] = RealValue.One;
                FwdAndBackSubst(U, vector, ref x);
                for (int i = j; i < _rowCount; ++i)
                    M[i, j] = x[i];

                vector[j] = RealValue.Zero;
            }
            return M;
        }


        private static void FwdAndBackSubst(UpperTriangularMatrix U, Vector v, ref RealValue[] x)
        {
            var m = U.RowCount;
            var start = -1;
            //Forward substitution. Solves UT * y = v, storing y in x
            for (int i = 0; i < m; ++i)
            {
                var sum = v[i];
                if (start >= 0)
                    for (int j = start; j < i; ++j)
                        sum -= U[j, i] * x[j];
                else if (!sum.Equals(RealValue.Zero))
                    start = i;

                x[i] = sum;
            }
            //Diagonal scalling
            for (int i = 0; i < m; ++i)
                x[i] /= U[i, i];
            //Backward substitution, solving U * x = y  
            for (int i = m - 1; i >= 0; --i)
            {
                var sum = x[i];
                var row = U._rows[i];
                var n = Math.Min(m, i + row.Size);
                for (int j = i + 1; j < n; ++j)
                    sum -= row[j - i] * x[j];

                x[i] = sum;
            }
        }

        private static void CholeskyFwdAndBackSubst(UpperTriangularMatrix U, Vector v, ref RealValue[] x)
        {
            var m = U.RowCount;
            var start = -1;
            //Forward substitution. Solves UT * y = v, storing y in x
            for (int i = 0; i < m; ++i)
            {
                var sum = v[i];
                if (start >= 0)
                    for (int j = start; j < i; ++j)
                        sum -= U[j, i] * x[j];
                else if (!sum.Equals(RealValue.Zero))
                    start = i;

                x[i] = sum / U[i, i];
            }
            //Backward substitution, solving U * x = y
            for (int i = m - 1; i >= 0; --i)
            {
                var sum = x[i];
                var row = U._rows[i];
                var n = Math.Min(m, i + row.Size);
                for (int j = i + 1; j < n; ++j)
                    sum -= row[j - i] * x[j];

                x[i] = sum / row[0];
            }
        }

        internal override RealValue Sum()
        {
            var _row = _rows[0];
            var v = _row.Sum();
            var result = 2 * v.D;
            var u = v.Units;
            RestoreMainDiagonal();
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                _row = _rows[i];
                v = _row.Sum();
                result += 2 * v.D * Unit.Convert(u, v.Units, ',');
                RestoreMainDiagonal();
            }
            return new(result, u);

            void RestoreMainDiagonal()
            {
                v = _row[0];
                result -= v.D * Unit.Convert(u, v.Units, ',');
            }
        }

        internal override RealValue SumSq()
        {
            var _row = _rows[0];
            var v = _row.SumSq();
            var result = 2 * v.D;
            var u = v.Units;
            RestoreMainDiagonal();
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                _row = _rows[i];
                v = _row.SumSq();
                result += 2 * v.D * Unit.Convert(u, v.Units, ',');
                RestoreMainDiagonal();
            }
            return new(result, u);

            void RestoreMainDiagonal()
            {
                v = _row[0] * _row[0];
                result -= v.D * Unit.Convert(u, v.Units, ',');
            }
        }

        internal override RealValue Product()
        {
            var _row = _rows[0];
            var v = _row.Product();
            var result = v.D * v.D;
            var u = v.Units;
            RestoreMainDiagonal();
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                _row = _rows[i];
                v = _row.Product();
                u = Unit.Multiply(u, v.Units, out var b);
                b *= v.D;
                result *= b * b;
                RestoreMainDiagonal();
            }
            return new(result, u);

            void RestoreMainDiagonal()
            {
                v = _row[0];
                u = Unit.Divide(u, v.Units, out var b);
                result *= b;
                if (v.D != 0)
                    result /= v.D;
            }
        }

        internal override Matrix Transpose() => RawCopy();

        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm()
        {
            RealValue norm = RealValue.Zero;
            for (int i = 0; i < _rowCount; ++i)
            {
                var sumAbs = RealValue.Abs(this[i, 0]);
                for (int j = 1; j < _colCount; ++j)
                    sumAbs += RealValue.Abs(this[i, j]);

                if (i == 0 || sumAbs.CompareTo(norm) > 0)
                    norm = sumAbs;
            }
            return norm;
        }
        internal Matrix EigenVectors()
        {
            //Jacobi(out Matrix E);
            //return E;
            var Q = Tridiagonalize(out RealValue[] d, out RealValue[] e, true);
            ImplicitQL(d, e, Q, true);
            return SortEigenVectors(d, Q, false);
        }

        internal Vector EigenValues()
        {
            var Q = Tridiagonalize(out RealValue[] d, out RealValue[] e, false);
            ImplicitQL(d, e, Q, false);
            return SortEigenValues(d);
        }

        internal Matrix Eigen()
        {
            var Q = Tridiagonalize(out RealValue[] d, out RealValue[] e, true);
            ImplicitQL(d, e, Q, true);
            return SortEigenVectors(d, Q, true);
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

        private RealValue[,] Tridiagonalize(out RealValue[] d, out RealValue[] e, bool eigenvecs)
        {
            d = new RealValue[_rowCount];
            e = new RealValue[_rowCount];
            var Q = new RealValue[_rowCount, _rowCount];
            var n = _rowCount - 1;
            var min = new int[_rowCount]; 
            for (int i = 0; i <= n; ++i)
            {
                var row = _rows[i];
                var m = row.Size - 1;
                for (int j = m; j >= 0; --j)
                {
                    var k = i + j;
                    if (i < min[k])
                        min[k] = i;
                    Q[k, i] = row[j];
                }
            }
            for (int i = n; i > 0; --i)
            {
                var l = i - 1;
                var h = RealValue.Zero;
                var m = min[i];
                if (l > 0)
                {

                    var scale = RealValue.Abs(Q[i, m]);
                    for (int k = m + 1; k <= l; ++k)
                        scale += RealValue.Abs(Q[i, k]);

                    if (scale.D == 0d)        //Skip transformation.
                        e[i] = Q[i, l];
                    else
                    {
                        Q[i, m] /= scale;      //Use scaled Q’s for transformation.
                        h = Sqr(Q[i, m]);      //Form σ in h.
                        for (int k = m + 1; k <= l; ++k)
                        {
                            Q[i, k] /= scale;  //Use scaled Q’s for transformation.
                            h += Sqr(Q[i, k]); //Form σ in h.
                        }
                        var f = Q[i, l];
                        var g = f.D >= 0d ? -RealValue.Sqrt(h) : RealValue.Sqrt(h);
                        e[i] = scale * g;
                        h -= f * g;           // Now h is equation(11.2.4).
                        Q[i, l] = f - g;      // Store u in the ith row of Q.
                        for (int j = m; j <= l; ++j)
                        {
                            /* Next statement can be omitted if eigenvectors not wanted */
                            if (eigenvecs)
                                Q[j, i] = Q[i, j] / h; //Store u/H in ith column of Q.

                            g = Q[j, 0] * Q[i, 0]; //Form an element of Q · u in g.
                            for (int k = 1; k <= j; ++k)
                                g += Q[j, k] * Q[i, k];

                            for (int k = j + 1; k <= l; ++k)
                                g += Q[k, j] * Q[i, k];

                            e[j] = g / h;          //Form element of p in temporarily unused element of e.
                            if (j == m)
                                f = e[j] * Q[i, j];
                            else
                                f += e[j] * Q[i, j];
                        }
                        var hh = f / (h + h);      //Form K, equation (11.2.11).
                        for (int j = m; j <= l; ++j)
                        {
                            //Form q and store in e overwriting p.
                            f = Q[i, j];
                            e[j] = g = e[j] - hh * f;
                            for (int k = m; k <= j; k++) //Reduce Q, equation (11.2.13).
                                Q[j, k] -= (f * e[k] + g * Q[i, k]);
                        }
                    }
                }
                else
                    e[i] = Q[i, l];

                d[i] = h;
            }
            /* Next statement can be omitted if eigenvectors not wanted */
            if (eigenvecs)
                d[0] = RealValue.Zero;

            e[0] = RealValue.Zero;
            /* Contents of this loop can be omitted if eigenvectors not
            wanted except for statement d[i] = Q[i,i]; */
            if (eigenvecs)
                for (int i = 0; i <= n; ++i) // Begin accumulation of transformation matrices.
                {
                    var l = i - 1;
                    var m = min[i];
                    if (d[i].D != 0d)
                    {
                        //This block skipped when i = 0.
                        for (int j = m; j <= l; ++j)
                        {
                            var g = Q[i, m] * Q[m, j];
                            for (int k = m + 1; k <= l; ++k) //Use u and u/H stored in a to form P·Q.
                                g += Q[i, k] * Q[k, j];

                            for (int k = 0; k <= l; ++k)
                                Q[k, j] -= g * Q[k, i];
                        }
                    }
                    d[i] = Q[i, i]; //This statement remains.
                    Q[i, i] = RealValue.One; //Reset row and column of a to identity matrix for next iteration.
                    for (int j = 0; j <= l; j++)
                        Q[j, i] = Q[i, j] = RealValue.Zero;
                }
            else
                for (int i = 0; i <= n; ++i) // Begin accumulation of transformation matrices.
                    d[i] = Q[i, i];          //This statement remains.

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
        private static void ImplicitQL(RealValue[] d, RealValue[] e, RealValue[,] Q, bool eigenvecs)
        {
            var n = d.Length - 1;
            //It is convenient to renumber the elements of e.
            for (int i = 0; i < n; ++i)
                e[i] = e[i + 1];

            e[n] = RealValue.Zero;
            for (int l = 0; l <= n; ++l)
            {
                var iter = 0;
                int m;
                do
                {
                    m = l;
                    while (m < n) // Look for a single small subdiagonal element to split the matrix.
                    {
                        var dd = RealValue.Abs(d[m]) + RealValue.Abs(d[m + 1]);
                        if (dd.Equals(RealValue.Abs(e[m]) + dd))
                            break;
                        ++m;
                    }
                    if (m != l)
                    {
                        if (++iter == 30)
                            Throw.QLAlgorithmFailed();

                        var l1 = l + 1;
                        var g = (d[l1] - d[l]) / (e[l] * 2d); //Form shift.
                        var r = Hypot(g, RealValue.One);
                        g = d[m] - d[l] + e[l] / (g + CopySign(r, g)); //This is dm − ks.
                        var s = RealValue.One;
                        var c = s;
                        var p = RealValue.Zero;
                        var i = m - 1;
                        while (i >= l)     //A plane rotation as in the original QL, 
                        {                  //followed by Givens rotations to restore tridiagonal form
                            var i1 = i + 1;
                            var f = s * e[i];
                            var b = c * e[i];
                            r = Hypot(f, g);
                            e[i1] = r;
                            if (r.D == 0d) //Recover from underflow.
                            {
                                d[i1] -= p;
                                e[m] = RealValue.Zero;
                                break;
                            }
                            s = f / r;
                            c = g / r;
                            if (p.Equals(RealValue.Zero))
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
                                    f = Q[k, i1];
                                    Q[k, i1] = s * Q[k, i] + c * f;
                                    Q[k, i] = c * Q[k, i] - s * f;
                                }

                            --i;
                        }
                        if (r.D == 0d && i >= l)
                            continue;

                        d[l] -= p;
                        e[l] = g;
                        e[m] = RealValue.Zero;
                    }
                } while (m != l);
            }
        }

        private static Vector SortEigenValues(RealValue[] d) => new Vector(d).Sort();

        private static Matrix SortEigenVectors(RealValue[] d, RealValue[,] Q, bool combine)
        {
            var n = d.Length;
            var indexes = Enumerable.Range(0, n).ToArray();
            Array.Sort(d, indexes);
            var j0 = combine ? 1 : 0;
            var m = n + j0;
            var M = new Matrix(n, m);
            for (int i = 0; i < n; ++i)
            {
                if (combine)
                    M[i, 0] = d[i]; 
                for (int j = 0; j < n; ++j)
                    M[i, j + j0] = Q[i, indexes[j]];            
            }
            return M;
        }

        protected override RealValue Condition(Func<Matrix, RealValue> norm)
        {
            var U = GetLDLT();
            if (U is null)
                return RealValue.PositiveInfinity;

            var M = GetInverse(U);
            return norm(this) * norm(M);
        }

        internal override Matrix Adjoint()
        {
            var U = GetLDLT();
            if (U is null)
                Throw.MatrixSingularException();

            var det = GetDeterminant(U);
            var M = GetInverse(U);
            return M * det;
        }

        protected override Matrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            indexes = new int[_rowCount];
            minPivot = double.MaxValue;
            det = 1d;
            for (int i = 0; i < _rowCount; ++i)
                indexes[i] = i;

            var U = GetLDLT();
            if (U is null)
                return null;

            var LU = new SymmetricMatrix(_rowCount);
            for (int i = 0; i < _rowCount; ++i)
                LU._rows[i] = U._rows[i];

            return LU;
        }
    }
}
