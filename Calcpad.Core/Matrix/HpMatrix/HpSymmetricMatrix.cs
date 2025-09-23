using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SN = System.Numerics;

namespace Calcpad.Core
{
    internal class HpSymmetricMatrix : HpMatrix
    {
        private static readonly int _vecSize = SN.Vector<double>.Count;
        internal HpSymmetricMatrix(int length, Unit units) : base(length, units)
        {
            _type = MatrixType.Symmetric;
            _hpRows = new HpVector[length];
            for (int i = length - 1; i >= 0; --i)
                _hpRows[i] = new HpVector(length - i, units);

            _rows = _hpRows;
        }

        internal HpSymmetricMatrix(SymmetricMatrix matrix) : base(matrix)
        {
            _type = MatrixType.Symmetric;
        }

        internal override RealValue this[int row, int col]
        {
            get => row <= col ?
                _hpRows[row][col - row] :
                _hpRows[col][row - col];
            set
            {
                if (row <= col)
                    _hpRows[row][col - row] = value;
                else
                    _hpRows[col][row - col] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override double GetValue(int row, int col) => 
            row <= col ?
            _hpRows[row].GetValue(col - row) :
            _hpRows[col].GetValue(row - col);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetValue(double value, int row, int col)
        {
            if (row <= col)
                _hpRows[row].SetValue(value, col - row);
            else
                _hpRows[col].SetValue(value, row - col);
        }

        private new HpSymmetricMatrix RawCopy()
        {
            var n = _hpRows.Length;
            var M = new HpSymmetricMatrix(n, _units);
            for (int i = n - 1; i >= 0; --i)
                M._hpRows[i] = _hpRows[i].Copy();

            return M;
        }

        internal override HpMatrix Clone() => new HpSymmetricMatrix(_rowCount, _units);
        internal override Matrix CloneAsMatrix() => new SymmetricMatrix(_rowCount);

        internal HpSymmetricMatrix Resize(int n)
        {
            if (n != _rowCount)
            {
                var n1 = Math.Min(n, _rowCount) - 1;
                Array.Resize(ref _hpRows, n);
                for (int i = _rowCount; i < n; ++i)
                    _hpRows[i] = new HpVector(n - i, _units);

                for (int i = 0; i <= n1; ++i)
                    _hpRows[i].Resize(n - i);

                _rowCount = n;
                _colCount = n;
            }
            return this;
        }

        internal HpUpperTriangularMatrix CholeskyDecomposition()
        {
            var L = GetCholesky(out int[] start) ?? 
                throw Exceptions.MatrixNotPositiveDefinite();

            return GetUFromLRows(L, start);
        }

        private double[][] GetCholesky(out int[] start)
        {
            start = GetRowsStart();
            double[][] L = new double[_rowCount][];
            for (int i = 0; i < _rowCount; ++i)
            {
                var i0 = start[i];
                var len = i - i0 + 1;
                if (len < 1)
                    return null;

                L[i] = new double[len];
                var L_i = L[i];
                ReadOnlySpan<double> sa = L_i.AsSpan(0, len);
                ReadOnlySpan<SN.Vector<double>> va = len >= _vecSize ?
                    MemoryMarshal.Cast<double, SN.Vector<double>>(sa):
                    ReadOnlySpan<SN.Vector<double>>.Empty;
                for (int j = i0; j <= i; ++j)
                {
                    var L_j = L[j];
                    var j0 = start[j];
                    var k0 = Math.Max(i0, j0);
                    len = j - k0;
                    double sum = 0;
                    var nv = len / _vecSize;
                    if (i == j)
                    {
                        for (int k = 0; k < nv; ++k)
                            sum += SN.Vector.Dot(va[k], va[k]);

                        for (int k = nv * _vecSize; k < len; ++k)
                            sum += sa[k] * sa[k];
                    }
                    else
                    {
                        ReadOnlySpan<double> sb = L_j.AsSpan(k0 - j0, len);
                        k0 -= i0;
                        if (nv > 0)
                        {
                            ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb);
                            var k1 = k0 / _vecSize;
                            for (int k = 0; k < nv; ++k)
                                sum += SN.Vector.Dot(va[k1 + k], vb[k]);

                            nv *= _vecSize;
                        }
                        for (int k = nv; k < len; ++k)
                            sum += sa[k0 + k] * sb[k];
                    }

                    if (i == j)
                    {
                        var row = _hpRows[i];
                        if (row.Size == 0)
                            return null;

                        var d = row.Raw[0] - sum;
                        if (d <= 0d)
                            return null;

                        L_i[j - i0] = Math.Sqrt(d);
                    }
                    else
                        L_i[j - i0] = (GetValue(i, j) - sum) / L_j[j - j0];
                }
            }
            return L;
        }

        private int[] GetRowsStart()
        {
            int[] start = new int[_rowCount];
            //Get row starting indexes for skyline symmetric matrix
            Array.Fill(start, _rowCount);
            for (int i = 0; i < _rowCount; ++i)
                for (int j = i, len = i + _hpRows[i].Size; j < len; ++j)
                {
                    if (i < start[j])
                        start[j] = i;
                }
            
            for (int j = 0; j < _colCount; ++j)
            {
                var i0 = start[j];
                for (int i = i0; i < j; ++i)
                {
                    var d = GetValue(i, j);
                    if (d != 0)
                        break;

                    ++i0;
                }
                start[j] = i0 - (i0 % _vecSize);
            }
            return start;
        }

        private HpUpperTriangularMatrix GetUFromLRows(double[][] lRows, int[] start)
        { 
            var U = new HpUpperTriangularMatrix(_rowCount, _units?.Pow(0.5f));
            var maxsize = 1;
            for (int i = 0; i < _rowCount; ++i)
            {
                var row = _rows[i];
                if (row.Size > --maxsize)
                    maxsize = row.Size;
                var n = _colCount - i;
                if (n > maxsize)
                    n = maxsize;

                var U_i = U.HpRows[i];
                for (int j = n - 1; j >= 0; --j)
                {
                    var l = i + j;
                    var L_j = lRows[l];
                    var k = i - start[l];
                    if (k >= 0)
                        U_i.SetValue(L_j[k], j);
                }
            }
            return U;
        }

        //Get LDLT decomposition by storing D in the main diagonal of L
        private double[][] GetLDLT(out int[] start)
        {
            start = GetRowsStart();
            double[][] L = new double[_rowCount][];
            double[] d = new double[_rowCount];
            for (int i = 0; i < _rowCount; ++i)
            {
                var i0 = start[i];
                var len = i - i0 + 1;
                if (len == 0)
                    return null;

                L[i] = new double[len];
                var L_i = L[i];
                for (int j = i0; j <= i; ++j)
                {
                    var L_j = L[j];
                    var j0 = start[j];
                    var k0 = Math.Max(i0, j0);
                    len = j + 1;
                    double sum = 0;
                    for (int k = k0; k < len; ++k)
                        sum += L_i[k - i0] * L_j[k - j0] * d[k];

                    if (i == j)
                    {
                        var row = _hpRows[i];
                        if (row.Size == 0)
                            return null;
                            
                        d[i] = row.Raw[0] - sum;
                        L_j[j - j0] = d[i];
                    }
                    else
                        L_i[j - i0] = (GetValue(i, j) - sum) / L_j[j - j0];
                }
            }
            return L;
        }

        internal override RealValue Determinant()
        {
            var L = GetLDLT(out _);
            if (L is null)
                return _units is null ? RealValue.Zero : new(0d, _units.Pow(_rowCount));

            return new(GetDeterminant(L), _units?.Pow(_rowCount));
        }

        private double GetDeterminant(double[][] L)
        {
            var det = 1d;
            for (int i = 0; i < _rowCount; ++i)
            {
                var L_i = L[i];
                det *= L_i[L_i.Length - 1];
            }
            return det;
        }

        internal override HpVector LSolve(HpVector vector)
        {
            var L = GetLDLT(out int[] start) ??
                throw Exceptions.MatrixSingular();

            var x = new double[_rowCount];
            FwdAndBackSubst(L, start, vector, x);
            var units = Unit.Divide(vector.Units, _units, out var d, true);
            if (d != 1d)
                Vectorized.Scale(x, d);

            return new(x, units);
        }

        internal HpVector ClSolve(HpVector v)
        {
            var L = GetCholesky(out int[] start) ??
                throw Exceptions.MatrixNotPositiveDefinite();

            var x = new double[_rowCount];
            FwdAndBackSubst(L, start, v, x, true);
            var units = Unit.Divide(v.Units, _units, out var d, true);
            if (d != 1d)
                Vectorized.Scale(x, d);

            return new(x, units);
        }

        internal HpVector SlSolve(HpVector v, double tol)
        {
            var x = new HpLinearSolver(this).SolvePCG(v.Raw, tol);
            var units = Unit.Divide(v.Units, _units, out var d, true);
            if (d != 1d)
                Vectorized.Scale(x, d);

            return new(x, units);
        }

        internal override HpMatrix MSolve(HpMatrix M)
        {
            var L = GetLDLT(out int[] start) ??
                throw Exceptions.MatrixSingular();

            return MSolveForL(L, start, M, _units);
        }

        internal HpMatrix CmSolve(HpMatrix M)
        {
            var L = GetCholesky(out int[] start) ??
                throw Exceptions.MatrixNotPositiveDefinite();

            return MSolveForL(L, start, M, _units, true);
        }

        internal HpMatrix SmSolve(HpMatrix M, double tol)
        {
            var X = new HpLinearSolver(this).MSolvePCG(M.AsJaggedArrayTransposed(), tol);
            var units = Unit.Divide(M.Units, _units, out var d, true);
            if (d != 1d)
                Parallel.For(0, X.Length, i => Vectorized.Scale(X[i], d));

            return FromJaggedArrayTransposed(X, units);
        }
        private static HpMatrix MSolveForL(double[][] L, int[] start, HpMatrix M, Unit units, bool Cholesky = false)
        {
            var m = L.Length;
            var n = M.ColCount;
            var v = new HpVector[n];
            var u = Unit.Divide(M.Units, units, out var d, true);
            Parallel.For(0, n, j =>
            {
                var x = new double[m];
                FwdAndBackSubst(L, start, M.Col(j + 1), x, Cholesky);
                if (d != 1d)
                    Vectorized.Scale(x, d);

                v[j] = new(x, u);
            });
            return CreateFromCols(v, m);
        }

        private static HpSymmetricMatrix MSolveForL(double[][]L, int[] start, Unit units)
        {
            var m = L.Length;
            var M = Identity(m);
            var result = new HpVector[m];
            Parallel.For(0, m, j =>
            {
                var x = new double[m];
                FwdAndBackSubst(L, start, M._hpRows[m - j - 1], x);
                result[j] = new(x, units);
            });
            for (int j = m - 1; j >= 0; --j)
            {
                var M_j = M._hpRows[j];
                var col = result[j];
                for (int i = m - 1; i >= j; --i)
                    M_j.SetValue(col.GetValue(i), i - j);
            }
            return M;
        }

        private static HpSymmetricMatrix Identity(int n)
        {
            var M = new HpSymmetricMatrix(n, null);
            for (int i = 0; i < n; ++i)
                M.SetValue(1d, i, n - 1);

            return M;
        }

        internal override HpMatrix Invert()
        {
            var L = GetLDLT(out int[] start) ??
                throw Exceptions.MatrixSingular(); //Decompose the matrix by LDLT decomp.

            return GetInverse(L, start);
        }

        private HpSymmetricMatrix GetInverse(double[][] L, int[] start)
        {
            var units = _units?.Pow(-1);
            if (_rowCount > ParallelThreshold)
                return MSolveForL(L, start, units);

            var vector = new HpVector(_rowCount, _rowCount, null);
            var x = new double[_rowCount];
            var M = new HpSymmetricMatrix(_rowCount,units);
            for (int j = 0; j < _rowCount; ++j)  //Find inverse by columns.
            {
                vector.Raw[j] = 1d;
                FwdAndBackSubst(L, start, vector, x);
                for (int i = j; i < _rowCount; ++i)
                    M.SetValue(x[i], i, j);

                vector.Raw[j] = 0d;
            }
            return M;
        }

        private static void FwdAndBackSubst(double[][] L, int[] start, HpVector v, double[] x, bool Cholesky = false)
        {
            var m = L.Length;
            int s0 = -1;
            ReadOnlySpan<double> sx = ReadOnlySpan<double>.Empty;
            ReadOnlySpan<SN.Vector<double>> vx = ReadOnlySpan<SN.Vector<double>>.Empty;
            //Forward substitution. Solves L * y = v, storing y in x
            for (int i = 0; i < m; ++i)
            {
                var sum = v.GetValue(i);
                var L_i = L[i];
                if (s0 >= 0)
                {
                    var j0 = start[i];
                    var j1 = Math.Max(s0, j0);
                    var n = i - j1;
                    ReadOnlySpan<double> sl = L_i.AsSpan(j1 - j0);
                    var nv = n / _vecSize;
                    if (nv > 0)
                    {
                        var v1 = (j1 - s0) / _vecSize;
                        ReadOnlySpan<SN.Vector<double>> vl = MemoryMarshal.Cast<double, SN.Vector<double>>(sl);
                        for (int j = 0; j < nv; ++j)
                            sum -= SN.Vector.Dot(vl[j], vx[v1 + j]);

                        nv *= _vecSize;
                    }
                    for (int j = j1 + nv; j < i; ++j)
                        sum -= sl[j - j1] * sx[j - s0];
                }
                else if (sum != 0)
                {
                    s0 = (i / _vecSize) * _vecSize;
                    sx = x.AsSpan(s0);
                    vx = MemoryMarshal.Cast<double, SN.Vector<double>>(sx);
                }
                //For LDLT decomposition, the diagonal ls scaled later
                x[i] = Cholesky ? sum / L_i[^1] : sum;
            }
            //Diagonal scaling for LDLT decomposition
            if (!Cholesky)
                for (int i = 0; i < m; ++i)
                    x[i] /= L[i][^1];
            //Backward substitution, solving LT * x = y
            for (int i = m - 1; i >= 0; --i)
            {
                var sum = x[i];
                // Process elements that correspond to this column in other rows
                for (int k = i + 1; k < m; ++k)
                {
                    var i0 = start[k];
                    var L_k = L[k];
                    // Check if row k has an element in column i
                    if (i >= i0)
                        sum -= L_k[i - i0] * x[k];
                }
                //Diagonal scaling. The diagonal element is always the last element in the row
                x[i] = Cholesky ? sum / L[i][^1] : sum;
            }
        }

        internal override RealValue Count(RealValue value)
        {
            if (!Unit.IsConsistent(Units, value.Units))
                return RealValue.Zero;

            var d = value.D * Unit.Convert(Units, value.Units, ',');
            var len = _hpRows.Length;
            if (len > ParallelThreshold)
            {
                var result = new HpVector(len, len, null);
                var values = result.Raw;
                Parallel.For(0, len, i =>
                {
                    var row = _hpRows[i];
                    var count = 2d * row.Count(value, 2).D;
                    if (row.GetValue(0).AlmostEquals(d))
                        ++count;

                    values[i] = count;
                });
                return result.Sum();
            }
            else
            {
                var count = 0d;
                for (int i = 0; i < len; ++i)
                {
                    var row = _hpRows[i];
                    count += 2d * row.Count(value, 2).D;
                    if (row.GetValue(0).AlmostEquals(d))
                        ++count;
                }
                return new(count);
            }
        }

        internal override RealValue Sum()
        {
            var len = _hpRows.Length;
            if (len > ParallelThreshold)
            {
                var result = new HpVector(len, len, _units);
                var values = result.Raw;
                Parallel.For(0, len, i =>
                {
                    var row = _hpRows[i];
                    values[i] = 2d * row.Sum().D - row.GetValue(0);
                });
                return result.Sum();
            }
            if (len == 0)
                return new(0d, _units);

            var row = _hpRows[0];
            var sum = 2d * row.Sum().D - row.GetValue(0);
            for (int i = 1; i < len; ++i)
            {
                row = _hpRows[i];
                sum += 2d * row.Sum().D - row.GetValue(0);
            }
            return new(sum, _units);
        }

        internal override RealValue SumSq()
        {
            var len = _hpRows.Length;
            var u = _units?.Pow(2f);
            if (len > ParallelThreshold)
            {
                var result = new HpVector(len, len, u);
                var values = result.Raw;
                Parallel.For(0, len, i =>
                {
                    var row = _hpRows[i];
                    var d = row.GetValue(0);
                    values[i] = 2d * _hpRows[i].SumSq().D - d * d;
                });
                return result.Sum();
            }
            if (len == 0)
                return new(0d, u);

            var row = _hpRows[0];
            var d = row.GetValue(0);
            var sumsq = 2d * row.SumSq().D - d * d;
            for (int i = 1; i < len; ++i)
            {
                row = _hpRows[i];
                d = row.GetValue(0);
                sumsq += 2d * row.SumSq().D - d * d;
            }
            return new(sumsq, u);
        }

        internal override RealValue Product()
        {
            var n = _rowCount * _colCount;
            var len = _hpRows.Length;
            var u = _units?.Pow(n);
            if (!IsStructurallyConsistentType || len == 0)
                return new(0d, u);

            if (len > ParallelThreshold)
            {
                var result = new HpVector(len, len, u);
                var values = result.Raw;
                Parallel.For(0, len, i => 
                {
                    var row = _hpRows[i];
                    var p = row.Product().D;
                    if (p != 0d)
                        values[i] = p * p / row.GetValue(0);
                });
                return result.Product();
            }
            var row = _hpRows[0];
            var p = row.Product().D;
            var product = p * p;
            if (p != 0d)
                product /= row.GetValue(0);

            for (int i = 1; i < len; ++i)
            {
                row = _hpRows[i];
                p = row.Product().D;
                product *= p * p;
                if (p != 0d)
                    product /= row.GetValue(0);
            }
            return new(product, u);
        }

        internal override HpMatrix Transpose() => RawCopy();

        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm()
        {
            double norm = 0d;
            for (int i = 0; i < _rowCount; ++i)
            {
                var sumAbs = Math.Abs(GetValue(i, 0));
                for (int j = 1; j < _colCount; ++j)
                    sumAbs += Math.Abs(GetValue(i, j));

                if (i == 0 || sumAbs.CompareTo(norm) > 0)
                    norm = sumAbs;
            }
            return new(norm, _units);
        }
        internal Matrix EigenVectors(int count, double tol)
        {
            var reverse = CheckCount(ref count);
            if (IsDirect(count))
            {
                var Q = HpEigenSolver.Tridiagonalize(AsJaggedArray(), out var d, out var e, true);
                HpEigenSolver.ImplicitQL(d, e, Q, true);
                return SortEigenVectors(d, Q, count, true, reverse, false, _units);
            }
            else
            {
                var Q = HpEigenSolver.Lanczos(AsJaggedArray(), count, reverse, tol,  out var d);
                return SortEigenVectors(d, Q, count, false, false, false, _units);
            }
        }

        internal HpVector EigenValues(int count, double tol)
        {
            var reverse = CheckCount(ref count);
            if (IsDirect(count))
            {
                var Q = HpEigenSolver.Tridiagonalize(AsJaggedArray(), out var d, out var e, false);
                HpEigenSolver.ImplicitQL(d, e, Q, false);
                return SortEigenValues(d, _units, count, reverse);
            }
            else
            {
                HpEigenSolver.Lanczos(AsJaggedArray(), count, reverse, tol, out var d);
                return new HpVector(d, _units);
            }
        }


        internal Matrix Eigen(int count, double tol)
        {
            var reverse = CheckCount(ref count);
            if (IsDirect(count))
            {
                var Q = HpEigenSolver.Tridiagonalize(AsJaggedArray(), out var d, out var e, true);
                HpEigenSolver.ImplicitQL(d, e, Q, true);
                return SortEigenVectors(d, Q, count, true, reverse, true, _units);
            }
            else
            {
                var Q = HpEigenSolver.Lanczos(AsJaggedArray(), count, reverse, tol, out var d);
                return SortEigenVectors(d, Q, count, false, false, true, _units);
            }
        }

        private bool IsDirect(int count) => _rowCount <= 200 || _rowCount <= 1000 && count > _rowCount / 5;

        private static HpVector SortEigenValues(double[] d, Unit units, int count, bool reverse)
        {
            if (reverse)
                Array.Sort(d, (a, b) => b.CompareTo(a));
            else
                Array.Sort(d);

            return new HpVector(d[0..count], units);
        }

        private static Matrix SortEigenVectors(double[] d, double[][] Q, int count, bool sort, bool reverse, bool combine, Unit units)
        {
            var len = d.Length;
            if (count > len)
                count = len;
            int[] indexes = null;
            if (sort)
            {
                indexes = Enumerable.Range(0, len).ToArray();
                Array.Sort(d, indexes);
            }
            var j0 = combine ? 1 : 0;
            var n = Q.Length;

            if (combine && units is not null)
            {
                var M = new Matrix(count, n + j0);
                for (int i = 0; i < count; ++i)
                {
                    var index = i;
                    if (sort)
                        index = reverse ? indexes[len - 1 - i] : indexes[i];

                    var row = M.Rows[i];
                    for (int j = n - 1; j >= 0; --j)
                        row[j + j0] = new(Q[j][index]);

                    if (combine)
                        row[0] = new(d[i], units);
                }
                return M;
            }
            var hpM = new HpMatrix(count, n + j0, null);
            for (int i = 0; i < count; ++i)
            {
                var index = i;
                if (sort)
                    index = reverse ? indexes[len - 1 - i] : indexes[i];

                var row = hpM.HpRows[i];
                for (int j = n - 1; j >= 0; --j)
                    row.SetValue(Q[j][index], j + j0);

                if (combine)
                    row.SetValue(d[i], 0);
            }
            return hpM;
        }

        protected override RealValue Condition(Func<HpMatrix, RealValue> norm)
        {
            var L = GetLDLT(out int[] start);
            if (L is null)
                return RealValue.PositiveInfinity;

            var M = GetInverse(L, start);
            return norm(this) * norm(M);
        }

        internal override HpMatrix Adjoint()
        {
            var L = GetLDLT(out int[] start) ?? throw Exceptions.MatrixSingular();
            var det = GetDeterminant(L);
            var M = GetInverse(L, start);
            return M * new RealValue(det);
        }

        protected override HpMatrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            indexes = new int[_rowCount];
            minPivot = double.MaxValue;
            det = 1d;
            for (int i = 0; i < _rowCount; ++i)
                indexes[i] = i;

            var L = GetLDLT(out int[] start);
            if (L is null)
                return null;

            var U = GetUFromLRows(L, start);
            var LU = new HpSymmetricMatrix(_rowCount, _units);
            for (int i = 0; i < _rowCount; ++i)
                LU._hpRows[i] = U.HpRows[i];

            return LU;
        }
    }
}
