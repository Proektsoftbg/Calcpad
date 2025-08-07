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
            _rows = new LargeVector[length];
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
            var U = GetCholesky() ?? 
                throw Exceptions.MatrixNotPositiveDefinite();

            return U;
        }


        private UpperTriangularMatrix GetCholesky()
        {
            UpperTriangularMatrix U = new(_rowCount);
            var maxsize = 1;
            for (int i = 0; i < _rowCount; ++i)
            {
                var row = _rows[i];
                if (row.Size > --maxsize)
                    maxsize = row.Size;

                RealValue sum = row[0];
                for (int k = 0; k < i; ++k)
                {
                    var U_ki = U[k, i];
                    sum -= U_ki * U_ki;
                }
                if (sum.D <= 0)
                    return null;

                var U_i = U.Rows[i];
                var n = _rowCount - i;
                if (n > maxsize)
                    n = maxsize;

                RealValue U_ii = new(Math.Sqrt(sum.D), sum.Units?.Pow(0.5f));
                for (int j = 1; j < n; ++j)
                {
                    sum = row[j];
                    for (int k = 0; k < i; ++k)
                    {
                        var U_k = U.Rows[k];
                        var ik = i - k;
                        sum -= U_k[ik] * U_k[ik + j];
                    }
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
                if (row.Size > --maxsize)
                    maxsize = row.Size;

                var sum = row[0];
                for (int k = 0; k < i; ++k)
                {
                    var U_ki = U[k, i];
                    sum -= U_ki * U_ki * U[k, k];
                }
                if (sum.D == 0d)
                    return null;

                var U_i = U.Rows[i];
                var n = _rowCount - i;
                if (n > maxsize)
                    n = maxsize;

                var U_ii = sum;
                for (int j = 1; j < n; ++j)
                {
                    sum = row[j];
                    for (int k = 0; k < i; ++k)
                    {
                        var U_k = U.Rows[k];
                        var ik = i - k;
                        sum -= U_k[ik] * U_k[ik + j] * U[k, k];
                    }
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
            var U = GetLDLT() ?? 
                throw Exceptions.MatrixSingular();

            var x = new RealValue[_rowCount];
            FwdAndBackSubst(U, v, x);
            return new(x);
        }

        internal Vector ClSolve(Vector v)
        {
            var U = GetCholesky() ??
                throw Exceptions.MatrixNotPositiveDefinite();

            var x = new RealValue[_rowCount];
            FwdAndBackSubst(U, v, x, true);
            return new(x);
        }

        internal override Matrix MSolve(Matrix M)
        {
            var U = GetLDLT() ?? 
                throw Exceptions.MatrixSingular();

            return MSolveForU(U, M);
        }

        internal virtual Matrix CmSolve(Matrix M)
        {
            var U = GetCholesky() ?? 
                throw Exceptions.MatrixNotPositiveDefinite();

            return MSolveForU(U, M, true);
        }

        private static Matrix MSolveForU(UpperTriangularMatrix U, Matrix M, bool Cholesky = false)
        {
            var m = U.RowCount;
            var n = M.ColCount;
            var result = new Vector[n];
            Parallel.For(0, n, j =>
            {
                var x = new RealValue[m];
                FwdAndBackSubst(U, new ColumnVector(M, j), x, Cholesky);
                result[j] = new(x);
            });
            return CreateFromCols(result, m);
        }

        private static SymmetricMatrix MSolveForU(UpperTriangularMatrix U)
        {
            var m = U.RowCount;
            var I = Identity(m);
            var result = new Vector[m];
            Parallel.For(0, m, j =>
            {
                var x = new RealValue[m];
                FwdAndBackSubst(U, I._rows[j], x);
                result[j] = new(x);
            });
            for (int j = m - 1; j >= 0; --j)
            {
                var M_j = I._rows[j];
                var col = result[j];
                for (int i = j; i >= 0 ; --i)
                    M_j[i] = col[i - j];
            }
            return I;
        }

        private static SymmetricMatrix Identity(int n)
        {
            var M = new SymmetricMatrix(n);
            for (int i = 0; i < n; ++i)
                M[i, i] = RealValue.One;

            return M;
        }

        internal override Matrix Invert()
        {
            var U = GetLDLT() ?? 
                throw Exceptions.MatrixSingular(); //Decompose the matrix by LDLT decomp.

            return GetInverse(U);
        }

        private SymmetricMatrix GetInverse(UpperTriangularMatrix U)
        {
            if (_rowCount > ParallelThreshold)
                return MSolveForU(U);

            var vector = new Vector(_rowCount);
            var x = new RealValue[_rowCount];
            var M = new SymmetricMatrix(_rowCount);
            for (int j = 0; j < _rowCount; ++j)  //Find inverse by columns.
            {
                vector[j] = RealValue.One;
                FwdAndBackSubst(U, vector, x);
                for (int i = j; i < _rowCount; ++i)
                    M[i, j] = x[i];

                vector[j] = RealValue.Zero;
            }
            return M;
        }

        private static void FwdAndBackSubst(UpperTriangularMatrix U, Vector v, RealValue[] x, bool Cholesky = false)
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
                //For LDLT decomposition, the diagonal ls scaled later
                x[i] = Cholesky ? sum / U[i, i] :  sum;
            }
            //Diagonal scalling for LDLT decomposition
            if (!Cholesky)
                for (int i = 0; i < m; ++i)
                    x[i] /= U[i, i];

            //Backward substitution, solving U * x = y
            for (int i = m - 1; i >= 0; --i)
            {
                var sum = x[i];
                var row = U.Rows[i];
                var r = row.Raw;
                var n = Math.Min(m - i, row.Size);
                for (int j = 1; j < n; ++j)
                    sum -= r[j] * x[i + j];

                x[i] = Cholesky ? sum / r[0] : sum;
            }
        }

        internal override RealValue Count(RealValue value)
        {
            var len = _rows.Length;
            if (len == 0)
                return RealValue.Zero;
            
            var count = 0d;
            for (int i = 0; i < len; ++i)
            {
                var row = _rows[i];
                count += 2d * row.Count(value, 2).D;
                if (row[0].AlmostEquals(value))
                    ++count;
            }
            return new(count);

        }
        internal override RealValue Sum()
        {
            var len = _rows.Length;
            if (len == 0)
                return RealValue.Zero;

            var row = _rows[0];
            var v = row.Sum();
            var sum = 2d * v.D;
            var u = v.Units;
            RestoreMainDiagonal();
            for (int i = 1; i < len; ++i)
            {
                row = _rows[i];
                v = row.Sum();
                sum += 2d * v.D * Unit.Convert(u, v.Units, ',');
                RestoreMainDiagonal();
            }
            return new(sum, u);

            void RestoreMainDiagonal()
            {
                v = row[0];
                sum -= v.D * Unit.Convert(u, v.Units, ',');
            }
        }

        internal override RealValue SumSq()
        {
            var len = _rows.Length;
            if (len == 0)
                return RealValue.Zero;

            var row = _rows[0];
            var v = row.SumSq();
            var sumsq = 2d * v.D;
            var u = v.Units;
            RestoreMainDiagonal();
            for (int i = 1; i < len; ++i)
            {
                row = _rows[i];
                v = row.SumSq();
                sumsq += 2d * v.D * Unit.Convert(u, v.Units, ',');
                RestoreMainDiagonal();
            }
            return new(sumsq, u);

            void RestoreMainDiagonal()
            {
                v = row[0] * row[0];
                sumsq -= v.D * Unit.Convert(u, v.Units, ',');
            }
        }

        internal override RealValue Product()
        {
            var len = _rows.Length;
            if (len == 0)
                return RealValue.Zero;

            var row = _rows[0];
            var p = row.Product();
            var product = p.D * p.D;
            var u = p.Units?.Pow(2f);
            RestoreMainDiagonal();
            for (int i = 1; i < len; ++i)
            {
                row = _rows[i];
                p = row.Product();
                u = Unit.Multiply(u, p.Units.Pow(2f), out var b);
                product *= p.D * p.D * b;
                RestoreMainDiagonal();
            }
            return new(product, u);

            void RestoreMainDiagonal()
            {
                p = row[0];
                u = Unit.Divide(u, p.Units, out var b);
                product *= b;
                if (p.D != 0)
                    product /= p.D;
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

        internal Matrix EigenVectors(int count)
        {
            var reverse = CheckCount(ref count);
            var Q = Tridiagonalize(out RealValue[] d, out RealValue[] e, true);
            ImplicitQL(d, e, Q, true);
            return SortEigenVectors(d, Q, count, reverse, false);
        }

        internal Vector EigenValues(int count)
        {
            var reverse = CheckCount(ref count);
            var Q = Tridiagonalize(out RealValue[] d, out RealValue[] e, false);
            ImplicitQL(d, e, Q, false);
            return SortEigenValues(d, count, reverse);
        }

        internal Matrix Eigen(int count)
        {
            var reverse = CheckCount(ref count);
            var Q = Tridiagonalize(out RealValue[] d, out RealValue[] e, true);
            ImplicitQL(d, e, Q, true);
            return SortEigenVectors(d, Q, count, reverse, true);
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

        private RealValue[][] Tridiagonalize(out RealValue[] d, out RealValue[] e, bool eigenvecs)
        {                       
            d = new RealValue[_rowCount];
            e = new RealValue[_rowCount];
            var Q = new RealValue[_rowCount][];
            var n = _rowCount - 1;
            var min = new int[_rowCount];

            for (int i = 0; i <= n; ++i)
                Q[i] = new RealValue[_rowCount];

            for (int i = 0; i <= n; ++i)
            {
                var row = _rows[i];
                var m = row.Size - 1;
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
                var h = RealValue.Zero;
                var m = min[i];
                if (l > 0)
                {
                    var scale = RealValue.Abs(Q_i[m]);
                    for (int k = m + 1; k <= l; ++k)
                        scale += RealValue.Abs(Q_i[k]);

                    if (scale.D == 0d)        //Skip transformation.
                        e[i] = Q_i[l];
                    else
                    {
                        Q_i[m] /= scale;      //Use scaled Q’s for transformation.
                        h = Sqr(Q_i[m]);      //Form σ in h.
                        for (int k = m + 1; k <= l; ++k)
                        {
                            Q_i[k] /= scale;  //Use scaled Q’s for transformation.
                            h += Sqr(Q_i[k]); //Form σ in h.
                        }
                        var f = Q_i[l];
                        var g = f.D >= 0d ? -RealValue.Sqrt(h) : RealValue.Sqrt(h);
                        e[i] = scale * g;
                        h -= f * g;           // Now h is equation(11.2.4).
                        Q_i[l] = f - g;      // Store u in the ith row of Q.
                        for (int j = m; j <= l; ++j)
                        {
                            var Q_j = Q[j];
                            /* Next statement can be omitted if eigenvectors not wanted */
                            if (eigenvecs)
                                Q_j[i] = Q_i[j] / h; //Store u/H in ith column of Q.

                            g = Q_j[0] * Q_i[0]; //Form an element of Q · u in g.
                            for (int k = 1; k <= j; ++k)
                                g += Q_j[k] * Q_i[k];

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
                d[0] = RealValue.Zero;

            e[0] = RealValue.Zero;
            /* Contents of this loop can be omitted if eigenvectors not
            wanted except for statement d[i] = Q[i,i]; */
            if (eigenvecs)
                for (int i = 0; i <= n; ++i) // Begin accumulation of transformation matrices.
                {
                    var Q_i = Q[i];
                    var l = i - 1;
                    var m = min[i];
                    if (d[i].D != 0d)
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
                    Q_i[i] = RealValue.One; //Reset row and column of a to identity matrix for next iteration.
                    for (int j = 0; j <= l; j++)
                        Q[j][i] = Q_i[j] = RealValue.Zero;
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
        private static void ImplicitQL(RealValue[] d, RealValue[] e, RealValue[][] Q, bool eigenvecs)
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
                            throw Exceptions.QLAlgorithmFailed();

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
                            if (p.D == 0d)
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
                        if (r.D == 0d && i >= l)
                            continue;

                        d[l] -= p;
                        e[l] = g;
                        e[m] = RealValue.Zero;
                    }
                } while (m != l);
            }
        }

        private static Vector SortEigenValues(RealValue[] d, int count, bool reverse)
        {
            if (reverse)
                Array.Sort(d, Vector.Descending);
            else
                Array.Sort(d);

            return new Vector(d[0..count]);
        }

        private static Matrix SortEigenVectors(RealValue[] d, RealValue[][] Q, int count, bool reverse, bool combine)
        {
            var len = d.Length;
            if (count > len)
                count = len;
            var indexes = Enumerable.Range(0, len).ToArray();
            Array.Sort(d, indexes);
            var j0 = combine ? 1 : 0;
            var n = Q.Length;
            var M = new Matrix(count, n + j0);
            for (int i = 0; i < count; ++i)
            {
                var index = reverse ? indexes[len - 1 - i] : indexes[i];
                var row = M.Rows[i];
                for (int j = n - 1; j >= 0; --j)
                    row[j + j0] = Q[j][index];

                if (combine)
                    row[0] = d[i];
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
            var U = GetLDLT() ?? throw Exceptions.MatrixSingular();
            var det = GetDeterminant(U);
            var M = GetInverse(U);
            return M * det;
        }

        protected override SymmetricMatrix GetLU(out int[] indexes, out double minPivot, out double det)
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
                LU._rows[i] = U.Rows[i];

            return LU;
        }
    }
}
