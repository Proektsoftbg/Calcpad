using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class Matrix : IValue, IEquatable<Matrix>
    {
        internal const int ParallelTreshold = 100;
        internal const int MaxSize = 1000000;

        protected int _rowCount;
        protected int _colCount;
        protected internal Vector[] _rows;

        protected enum MatrixType
        {
            Full,
            UpperTriangular,
            LowerTriangular,
            Diagonal,
            Column,
            Symmetric
        }
        protected MatrixType _type;

        private bool IsStructurallyConsistentType =>
            _type == MatrixType.Full ||
            _type == MatrixType.Column ||
            _type == MatrixType.Symmetric;

        protected Matrix() { }

        protected Matrix(int length)
        {
            if (length > MaxSize)
                Throw.MatirixSizeLimitException();

            _rowCount = length;
            _colCount = length;
        }

        internal Matrix(int rows, int cols)
        {
            if (rows > MaxSize || cols > MaxSize)
                Throw.MatirixSizeLimitException();

            _type = MatrixType.Full;
            _rowCount = rows;
            _colCount = cols;
            _rows = new Vector[rows];
            for (int i = rows - 1; i >= 0; --i)
                _rows[i] = new LargeVector(cols);
        }

        internal Matrix(Vector v)
        {
            if (v.Length > MaxSize  )
                Throw.MatirixSizeLimitException();

            _type = MatrixType.Full;
            _rowCount = 1;
            _colCount = v.Length;
            _rows = [v];
        }

        internal virtual RealValue this[int row, int col]
        {
            get => _rows[row][col];
            set => _rows[row][col] = value;
        }

        internal virtual Matrix Clone() => new(_rowCount, _colCount);
        internal int RowCount => _rowCount;
        internal int ColCount => _colCount;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is null)
                return false;

            return obj is Matrix M && Equals(M);
        }

        public bool Equals(Matrix M)
        {
            if (_type != M._type ||
                _rowCount != M._rowCount ||
                _colCount != M._colCount)
                return false;

            var m = _rows.Length;
            if (m > ParallelTreshold)
            {
                var result = true;
                Parallel.For(0, m, (i, state) =>
                {
                    if (!_rows[i].Equals(M._rows[i]))
                    {
                        result = false;
                        state.Break();
                    }
                });
                return result;
            }
            for (int i = m - 1; i >= 0; --i)
                if (!_rows[i].Equals(M._rows[i]))
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(_type);
            hashCode.Add(_rowCount);
            hashCode.Add(_colCount);
            var m = _rows.Length;
            for (int i = m - 1; i >= 0; --i)
                hashCode.Add(_rows[i].GetHashCode);

            return hashCode.ToHashCode();
        }

        private static void CheckBounds(Matrix a, Matrix b)
        {
            if (a._rowCount != b._rowCount ||
                a._colCount != b._colCount)
                Throw.MatrixDimensionsException();
        }

        protected Matrix Copy()
        {
            var M = new Matrix(_rowCount, _colCount);
            for (int i = _rowCount - 1; i >= 0; --i)
                for (int j = _colCount - 1; j >= 0; --j)
                    M[i, j] = this[i, j];

            return M;
        }

        internal Matrix Resize(int m, int n)
        {
            if (m > MaxSize || n > MaxSize)
                Throw.MatrixDimensionsException();

            var m1 = Math.Min(m, _rowCount) - 1;
            if (_type == MatrixType.Full)
            {
                if (m != _rowCount)
                {
                    Array.Resize(ref _rows, m);
                    for (int i = _rowCount; i < m; ++i)
                        _rows[i] = new LargeVector(n);

                    _rowCount = m;
                }
                if (n != _colCount)
                {
                    for (int i = 0; i <= m1; ++i)
                        _rows[i].Resize(n);

                    _colCount = n;
                }
                return this;
            }
            if (n == 1 && this is ColumnMatrix cm)
                return cm.Resize(m);

            if (m == n)
            {
                if (this is DiagonalMatrix dm)
                    return dm.Resize(n);
                if (this is SymmetricMatrix sm)
                    return sm.Resize(n);
                if (this is UpperTriangularMatrix um)
                    return um.Resize(n);
                if (this is LowerTriangularMatrix lm)
                    return lm.Resize(n);
            }
            if (m == _rowCount && n == _colCount)
                return this;

            var M = new Matrix(m, n);
            var n1 = Math.Min(n, _colCount) - 1;
            for (int i = m1; i >= 0; --i)
                for (int j = n1; j >= 0; --j)
                    M[i, j] = this[i, j];

            return M;
        }

        public static Matrix operator -(Matrix a)
        {
            var c = a.Clone();
            var m = a._rows.Length;
            if (m > ParallelTreshold)
                Parallel.For(0, m, i =>
                    c._rows[i] = -a._rows[i]);
            else
                for (int i = m - 1; i >= 0; --i)
                    c._rows[i] = -a._rows[i];

            return c;
        }


        public static Matrix operator -(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] - b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] - b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] - b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] - b[i, j];
            }
            return c;
        }

        public static Matrix operator -(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] - b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] - b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] - b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] - b;
            }
            return c;
        }

        public static Matrix operator -(RealValue a, Matrix b)
        {
            Matrix c;
            if (b.IsStructurallyConsistentType || a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                var m = b._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    c._rows[i] = a - b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a - b._rows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a - b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a - b[i, j];
            }
            return c;
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] + b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] + b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] + b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] + b[i, j];
            }
            return c;
        }

        public static Matrix operator +(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] + b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] + b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] + b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] + b;
            }
            return c;
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a._colCount != b._rowCount)
                Throw.MatrixDimensionsException();

            if (b is ColumnMatrix bc)
                return a * bc;

            if (a is DiagonalMatrix ad)
            {
                if (b is DiagonalMatrix bd)
                    return ad * bd;

                return ad * b;
            }
            else if (b is DiagonalMatrix bd)
                return a * bd;

            Matrix c = new(a._rowCount, b._colCount);
            var m = a._rowCount;
            var na = a._colCount - 1;
            var nb = b._colCount - 1;
            if (a._type == MatrixType.Full)
            {
                var bColVector = new ColumnVector[b._colCount];
                for (int j = nb; j >= 0; --j)
                    bColVector[j] =  b.GetColumnVector(j);

                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._rows[i];
                        var ar = a._rows[i];
                        for (int j = nb; j >= 0; --j)
                            cr[j] = Vector.DotProduct(ar, bColVector[j]);
                    });
                else
                {
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._rows[i];
                        var ar = a._rows[i];
                        for (int j = nb; j >= 0; --j)
                            cr[j] = Vector.DotProduct(ar, bColVector[j]);
                    }
                }
            }
            else
            {
                if (m > ParallelTreshold)
                    Parallel.For(0, m, MultiplyRow);
                else
                    for (int i = m - 1; i >= 0; --i)
                        MultiplyRow(i);
            }
            return c;

            void MultiplyRow(int i)
            {
                var c_i = c._rows[i];
                for (int j = nb; j >= 0; --j)
                {
                    var c_ij = a[i, na] * b[na, j];
                    for (int k = na - 1; k >= 0; --k)
                        c_ij += a[i, k] * b[k, j];

                    c_i[j] = c_ij;
                }
            }
        }

        public static Matrix operator *(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || b.Units == null)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] * b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] * b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] * b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] * b;
            }
            return c;
        }

        public static Matrix operator /(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] / b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] / b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] / b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] / b[i, j];
            }
            return c;
        }

        public static Matrix operator /(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType && !b.Equals(RealValue.Zero) && b.Units is null)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] / b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] / b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] / b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] / b;
            }
            return c;
        }

        public static Matrix operator /(RealValue a, Matrix b)
        {
            Matrix c;
            if (b.IsStructurallyConsistentType)
            {
                c = b.Clone();
                var m = b._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a / b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a / b._rows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a / b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a / b[i, j];
            }
            return c;
        }

        public static Matrix operator %(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] % b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] % b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] % b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] % b[i, j];
            }
            return c;
        }

        public static Matrix operator %(Matrix a, RealValue b)
        {
            if (b.Equals(RealValue.Zero))
                return new Matrix(a._rowCount, a._colCount).Fill(RealValue.NaN);

            Matrix c;
            if (a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] % b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] % b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] % b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] % b;
            }
            return c;
        }

        public static Matrix operator %(RealValue a, Matrix b)
        {
            Matrix c;
            if (b.IsStructurallyConsistentType)
            {
                c = b.Clone();
                var m = b._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a % b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a % b._rows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a % b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a % b[i, j];
            }
            return c;
        }

        public static Matrix operator ==(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] == b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] == b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] == b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] == b[i, j];
            }
            return c;
        }

        public static Matrix operator ==(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] == b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] == b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] == b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] == b;
            }
            return c;
        }

        public static Matrix operator !=(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] != b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] != b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] != b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] != b[i, j];
            }
            return c;
        }

        public static Matrix operator !=(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] != b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] != b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] != b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] != b;
            }
            return c;
        }

        public static Matrix operator <(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] < b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] < b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] < b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] < b[i, j];
            }
            return c;
        }

        public static Matrix operator <(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] < b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] < b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] < b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] < b;
            }
            return c;
        }

        public static Matrix operator <(RealValue a, Matrix b)
        {
            Matrix c;
            if (b.IsStructurallyConsistentType || a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                var m = b._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a < b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a < b._rows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a < b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a < b[i, j];
            }
            return c;
        }

        public static Matrix operator >(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] > b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] > b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] > b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] > b[i, j];
            }
            return c;
        }

        public static Matrix operator >(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] > b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] > b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] > b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] > b;
            }
            return c;
        }

        public static Matrix operator >(RealValue a, Matrix b)
        {
            Matrix c;
            if (b.IsStructurallyConsistentType || a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                var m = b._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a > b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a > b._rows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a > b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a > b[i, j];
            }
            return c;
        }

        public static Matrix operator >=(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] >= b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] >= b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] >= b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] >= b[i, j];
            }
            return c;
        }

        public static Matrix operator >=(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] >= b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] >= b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] >= b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] >= b;
            }
            return c;
        }

        public static Matrix operator >=(RealValue a, Matrix b)
        {
            Matrix c;
            if (b.IsStructurallyConsistentType)
            {
                c = b.Clone();
                var m = b._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a >= b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a >= b._rows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a >= b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a >= b[i, j];
            }
            return c;
        }

        public static Matrix operator <=(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] <= b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] <= b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] <= b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] <= b[i, j];
            }
            return c;
        }

        public static Matrix operator <=(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] <= b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] <= b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] <= b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] <= b;
            }
            return c;
        }

        public static Matrix operator <=(RealValue a, Matrix b)
        {
            Matrix c;
            if (b.IsStructurallyConsistentType)
            {
                c = b.Clone();
                var m = b._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a <= b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a <= b._rows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a <= b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a <= b[i, j];
            }
            return c;
        }

        public static Matrix operator &(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] & b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] & b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] & b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] & b[i, j];
            }
            return c;
        }

        public static Matrix operator &(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] & b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] & b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] & b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] & b;
            }
            return c;
        }

        public static Matrix operator |(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] | b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] | b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] | b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] | b[i, j];
            }
            return c;
        }

        public static Matrix operator |(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] | b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] | b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] | b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] | b;
            }
            return c;
        }

        public static Matrix operator ^(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] ^ b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] ^ b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] ^ b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] ^ b[i, j];
            }
            return c;
        }

        public static Matrix operator ^(Matrix a, RealValue b)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] ^ b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] ^ b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] ^ b;
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] ^ b;
            }
            return c;
        }

        public static implicit operator Matrix(Vector vector) => new ColumnMatrix(vector);

        internal static Matrix EvaluateOperator(Calculator.Operator<RealValue> op, Matrix a, Matrix b, bool isZeroPresrving, bool requireConsistentUnits)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type &&
                (a.IsStructurallyConsistentType || isZeroPresrving))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = Vector.EvaluateOperator(op, a._rows[i], b._rows[i], requireConsistentUnits));
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = Vector.EvaluateOperator(op, a._rows[i], b._rows[i], requireConsistentUnits);
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = op(a[i, j], b[i, j]);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = op(a[i, j], b[i, j]);
            }
            return c;
        }

        internal static Matrix EvaluateOperator(Calculator.Operator<RealValue> op, Matrix a, RealValue b, bool isZeroPreserving, bool requireConsistentUnits)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType ||
                isZeroPreserving && a.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = Vector.EvaluateOperator(op, a._rows[i], b, requireConsistentUnits));
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = Vector.EvaluateOperator(op, a._rows[i], b, requireConsistentUnits);
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = op(a[i, j], b);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = op(a[i, j], b);
            }
            return c;
        }

        internal static Matrix EvaluateOperator(Calculator.Operator<RealValue> op, RealValue a, Matrix b, bool isZeroPreserving, bool requireConsistentUnits)
        {
            Matrix c;
            if (b.IsStructurallyConsistentType ||
                isZeroPreserving && a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                var m = b._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = Vector.EvaluateOperator(op, a, b._rows[i], requireConsistentUnits));
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = Vector.EvaluateOperator(op, a, b._rows[i], requireConsistentUnits);
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = op(a, b[i, j]);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = op(a, b[i, j]);
            }
            return c;
        }

        internal static Matrix EvaluateFunction(Calculator.Function<RealValue> f, Matrix a)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = Vector.EvaluateFunction(f, a._rows[i]));
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = Vector.EvaluateFunction(f, a._rows[i]);

            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = f(a[i, j]);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = f(a[i, j]);
            }
            return c;
        }

        internal RealValue[] Values
        {
            get
            {
                RealValue[] values = new RealValue[_rowCount * _colCount];
                int k = 0;
                for (int i = 0; i < _rowCount; ++i)
                    for (int j = 0; j < _colCount; ++j)
                        values[k++] = this[i, j];

                return values;
            }
        }

        internal static Matrix Hadamard(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] * b._rows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] * b._rows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                if (a._type == MatrixType.Column && b._type == MatrixType.Column)
                    c = new ColumnMatrix(m);
                else
                    c = new Matrix(m, n);

                --n;
                if (m > ParallelTreshold)
                    Parallel.For(0, m, i =>
                    {
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] * b[i, j];
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                        for (int j = n; j >= 0; --j)
                            c[i, j] = a[i, j] * b[i, j];
            }
            return c;
        }

        internal static Matrix Kronecker(Matrix a, Matrix b)
        {
            var m = a._rowCount;
            var n = a._colCount;
            var p = b._rowCount;
            var q = b._colCount;
            var n1 = n - 1;
            var p1 = p - 1;
            var q1 = q - 1;
            var c = new Matrix(m * p, n * q);
            if (m > ParallelTreshold)
                Parallel.For(0, m, InnerLoop);
            else
                for (int i = m - 1; i >= 0; --i)
                    InnerLoop(i);

            void InnerLoop(int i)
            {
                var ip = i * p;
                for (int j = n1; j >= 0; --j)
                {
                    var jq = j * q;
                    for (int k = p1; k >= 0; --k)
                    {
                        var ipk = ip + k;
                        for (int l = q1; l >= 0; --l)
                            c[ipk, jq + l] = a[i, j] * b[k, l];
                    }
                }
            }
            return c;
        }

        internal static RealValue Frobenius(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            var m = a._rows.Length;
            RealValue sum = RealValue.Zero;
            if (m > ParallelTreshold)
                Parallel.For(0, m, i =>
                    sum += Vector.DotProduct(a._rows[i], b._rows[i]));
            else
                for (int i = m - 1; i >= 0; --i)
                    sum += Vector.DotProduct(a._rows[i], b._rows[i]);

            return sum;
        }

        internal Vector Row(int index)
        {
            if (index < 1 || index > _rowCount)
                Throw.IndexOutOfRangeException(index.ToString());   

            --index;
            if (_type == MatrixType.Full)
                return _rows[index].Copy();

            var row = new LargeVector(_colCount);
            for (int j = _colCount - 1; j >= 0; --j)
                row[j] = this[index, j];

            return row;
        }

        internal Vector Col(int index)
        {
            if (index < 1 || index > _colCount)
                Throw.IndexOutOfRangeException(index.ToString());

            --index;
            if (this is ColumnMatrix cm)
                return cm._rows[index].Copy();

            var col = new LargeVector(_rowCount);
            for (int i = _rowCount - 1; i >= 0; --i)
                col[i] = this[i, index];

            return col;
        }

        private ColumnVector GetColumnVector(int index) => new(this, index);

        internal void SetRow(int index, Vector row) =>
            _rows[index] = row.Resize(_colCount);

        internal void SetCol(int index, Vector col)
        {
            var n = Math.Min(col.Length, _rowCount) - 1;
            for (int i = n; i >= 0; --i)
                this[i, index] = col[i];
        }

        internal void SetCol(int index, RealValue[] col)
        {
            var n = Math.Min(col.Length, _rowCount) - 1;
            for (int i = n; i >= 0; --i)
                this[i, index] = col[i];
        }

        internal Matrix Submatrix(int i1, int i2, int j1, int j2)
        {
            if (i2 < i1)
                (i1, i2) = (i2, i1);
            if (j2 < j1)
                (j1, j2) = (j2, j1);
            if (i1 < 1)
                Throw.IndexOutOfRangeException(i1.ToString());
            if (i2 > _rowCount)
                Throw.IndexOutOfRangeException(i2.ToString());
            if (j1 < 1)
                Throw.IndexOutOfRangeException(j1.ToString());
            if (j2 > _colCount)
                Throw.IndexOutOfRangeException(j2.ToString());

            var rows = i2 - i1;
            var cols = j2 - j1;
            var M = new Matrix(rows + 1, cols + 1);
            --i1;
            if (_type == MatrixType.Full)
                for (int i = rows; i >= 0; --i)
                    M._rows[i] = _rows[i1 + i].Slice(j1, j2);
            else
            {
                --j1;
                for (int i = rows; i >= 0; --i)
                {
                    var row = M._rows[i];
                    for (int j = cols; j >= 0; --j)
                        row[j] = this[i1 + i, j1 + j];
                }
            }
            return M;
        }

        internal Matrix ExtractRows(Vector vector)
        {
            var rowCount = vector.Length;
            var M = new Matrix(rowCount, _colCount);
            if (_type == MatrixType.Full)
                for (int i = rowCount - 1; i >= 0; --i)
                {
                    var rowIndex = (int)vector[i].D;
                    if (rowIndex < 1 || rowIndex > _rowCount)
                        Throw.IndexOutOfRangeException(rowIndex.ToString());

                    M._rows[i] = _rows[rowIndex - 1];
                }
            else
            {
                var n = _colCount - 1;
                for (int i = rowCount - 1; i >= 0; --i)
                {
                    var rowIndex = (int)vector[i].D;
                    if (rowIndex < 1 || rowIndex > _rowCount)
                        Throw.IndexOutOfRangeException(rowIndex.ToString());

                    var row = M._rows[i];
                    for (int j = n; j >= 0; --j)
                        row[j] = this[rowIndex - 1, j];
                }
            }
            return M;
        }

        internal Matrix ExtractCols(Vector vector)
        {
            var colCount = vector.Length;
            var M = new Matrix(_rowCount, colCount);
            var m = _rowCount - 1;
            for (int j = colCount - 1; j >= 0; --j)
            {
                var colIndex = (int)vector[j].D;
                if (colIndex < 1 || colIndex > _colCount)
                    Throw.IndexOutOfRangeException(colIndex.ToString());

                for (int i = m; i >= 0; --i)
                    M[i, j] = this[i, colIndex - 1];
            }
            return M;
        }

        internal Matrix Fill(RealValue value)
        {
            var m = _rows.Length;
            if (m > ParallelTreshold)
                Parallel.For(0, m, i =>
                    _rows[i].Fill(value));
            else
                for (int i = m - 1; i >= 0; --i)
                    _rows[i].Fill(value);

            return this;
        }

        internal Matrix FillRow(int i, in RealValue value)
        {
            if (i > _rowCount)
                Throw.IndexOutOfRangeException(i.ToString());

            --i;
            if (_type == MatrixType.Full || 
                _type == MatrixType.Symmetric ||
                _type == MatrixType.UpperTriangular ||
                _type == MatrixType.LowerTriangular)
                _rows[i].Fill(value);
            else
                _rows[0][i] = value;

            return this;
        }

        internal Matrix FillCol(int j, in RealValue value)
        {
            if (j > _colCount)
                Throw.IndexOutOfRangeException(j.ToString());

            --j;
            if (_type == MatrixType.Full)
                for (int i = _rowCount - 1; i >= 0; --i)
                    this[i, j] = value;
            else if (_type == MatrixType.Column ||
                     _type == MatrixType.Symmetric)
                _rows[j].Fill(value);
            else if (_type == MatrixType.Diagonal)
                _rows[0][j] = value;
            else if (_type == MatrixType.LowerTriangular)
                for (int i = _rowCount - 1; i >= j; --i)
                    this[i, j] = value;
            else if (_type == MatrixType.UpperTriangular)
                for (int i = j; i >= 0; --i)
                    this[i, j] = value;

            return this;
        }

        internal static Matrix Augment(params Matrix[] matrices)
        {
            int len = matrices.Length, m = 0, n = 0;
            for (int k = 0; k < len; ++k)
            {
                var matrix = matrices[k];
                n += matrix._colCount;
                m = Math.Max(m, matrix._rowCount);
            }
            var M = new Matrix(m, n);
            var c = 0;
            for (int k = 0; k < len; ++k)
            {
                var matrix = matrices[k];
                m = matrix._rowCount;
                n = matrix._colCount;
                for (int i = 0; i < m; ++i)
                    for (int j = 0; j < n; ++j)
                        M[i, c + j] = matrix[i, j];

                c += n;
            }
            return M;
        }

        internal static Matrix Stack(params Matrix[] matrices)
        {
            int len = matrices.Length, m = 0, n = 0;
            for (int k = 0; k < len; ++k)
            {
                var matrix = matrices[k];
                n = Math.Max(n, matrix._colCount);
                m += matrix._rowCount;
            }
            var M = new Matrix(m, n);
            var r = 0;
            for (int k = 0; k < len; ++k)
            {
                var matrix = matrices[k];
                m = matrix._rowCount;
                n = matrix._colCount;
                for (int i = 0; i < m; ++i)
                    for (int j = 0; j < n; ++j)
                        M[r + i, j] = matrix[i, j];

                r += m;
            }
            return M;
        }

        internal Vector Search(in RealValue value, int iStart, int jStart)
        {
            if (_type == MatrixType.Full)
                for (int i = iStart - 1; i < _rowCount; ++i)
                {
                    var jValue = _rows[i].Search(value, jStart);
                    if (!jValue.Equals(RealValue.Zero))
                        return new([new(i + 1), jValue]);

                    jStart = 1;
                }
            else
            {
                --jStart;
                for (int i = iStart - 1; i < _rowCount; ++i)
                {
                    for (int j = jStart; j < _colCount; ++j)
                        if (this[i, j].AlmostEquals(value))
                            return new([new(i + 1), new(j + 1)]);

                    jStart = 0;
                }
            }
            return new Vector(2);
        }

        internal RealValue Count(in RealValue value)
        {
            var count = 0;
            if (_type == MatrixType.Full)
                for (int i = 0; i < _rowCount; ++i)
                    count += (int)_rows[i].Count(value, 1).D;
            else
                for (int i = 0; i < _rowCount; ++i)
                    for (int j = 0; j < _colCount; ++j)
                        if (this[i, j].AlmostEquals(value))
                            ++count;

            return new(count);
        }

        internal Matrix FindAll(in RealValue value, Vector.Relation rel)
        {
            var M = FromIndexes(FindAllIndexes(value, rel));
            if (M._colCount == 0)
                M = new Matrix(2, 1);

            return M;
        }

        internal Vector HLookup(in RealValue value, int searchRow, int returnRow, Vector.Relation rel)
        {
            if (searchRow < 1 || searchRow > _rowCount)
                Throw.IndexOutOfRangeException(searchRow.ToString());
            if (returnRow < 1 || returnRow > _rowCount)
                Throw.IndexOutOfRangeException(returnRow.ToString());

            var i = searchRow - 1;
            var indexes = new List<int>();
            for (int j = 0; j < _colCount; ++j)
                if (Vector.Relate(this[i, j], value, rel))
                    indexes.Add(j);

            var n = indexes.Count;
            var vector = new Vector(n);
            i = returnRow - 1;
            for (int j = 0; j < n; ++j)
                vector[j] = this[i, indexes[j]];

            return vector;
        }

        internal Vector VLookup(in RealValue value, int searchCol, int returnCol, Vector.Relation rel)
        {
            if (searchCol > _colCount)
                Throw.IndexOutOfRangeException(searchCol.ToString());
            if (returnCol > _colCount)
                Throw.IndexOutOfRangeException(returnCol.ToString());

            var j = searchCol - 1;
            var indexes = new List<int>();
            for (int i = 0; i < _rowCount; ++i)
                if (Vector.Relate(this[i, j], value, rel))
                    indexes.Add(i);

            j = returnCol - 1;
            var vector = new Vector(indexes.Count);
            for (int i = 0, n = indexes.Count; i < n; ++i)
                vector[i] = this[indexes[i], j];

            return vector;
        }

        private List<(int, int)> FindAllIndexes(in RealValue value, Vector.Relation rel)
        {
            var indexes = new List<(int, int)>();
            for (int i = 0; i < _rowCount; ++i)
                for (int j = 0; j < _colCount; ++j)
                    if (Vector.Relate(this[i, j], value, rel))
                        indexes.Add((i + 1, j + 1));

            return indexes;
        }

        internal static Matrix FromIndexes(IEnumerable<(int, int)> indexes)
        {
            var n = indexes.Count();
            var matrix = new Matrix(2, n);
            var k = 0;
            foreach (var (i, j) in indexes)
            {
                matrix[0, k] = new(i);
                matrix[1, k] = new(j);
                ++k;
            }
            return matrix;
        }

        internal Matrix SortRows(int j, bool reverse = false)
        {
            --j;
            var indexes = new ColumnVector(this, j).GetOrderIndexes(reverse);
            return ReorderRows(indexes);
        }

        internal Matrix ReorderRows(int[] indexes)
        {
            Matrix M = new(_rowCount, _colCount);
            var m = _rowCount - 1;
            if (_type == MatrixType.Full)
                for (int i = m; i >= 0; --i)
                    M._rows[i] = _rows[indexes[i]];
            else
            {
                var n = _colCount - 1;
                for (int i = m; i >= 0; --i)
                {
                    var row = M._rows[i];
                    for (int k = n; k >= 0; --k)
                        row[k] = this[indexes[i], k];
                }
            }
            return M;
        }

        internal Matrix SortCols(int i, bool reverse = false)
        {
            if (i > _rowCount)
                Throw.IndexOutOfRangeException(i.ToString());

            --i;
            var row = _type == MatrixType.Full ? _rows[i] : new RowVector(this, i);
            var indexes = row.GetOrderIndexes(reverse);
            return ReorderCols(indexes);
        }

        internal Matrix ReorderCols(int[] indexes)
        {
            Matrix M = new(_rowCount, _colCount);
            var m = _rowCount - 1;
            var n = _colCount - 1;
            for (int k = m; k >= 0; --k)
            {
                var row = M._rows[k];
                for (int j = n; j >= 0; --j)
                    row[j] = this[k, indexes[j]];
            }
            return M;
        }

        internal Vector OrderRows(int j, bool reverse = false)
        {
            if (j > _colCount)
                Throw.IndexOutOfRangeException(j.ToString());
            --j;
            var col = _type == MatrixType.Column ? _rows[0] : new ColumnVector(this, j);
            return col.Order(reverse);
        }

        internal Vector OrderCols(int i, bool reverse = false)
        {
            if (i > _rowCount)
                Throw.IndexOutOfRangeException(i.ToString());
            --i;
            var row = _type == MatrixType.Full ? _rows[i] : new RowVector(this, i);
            return row.Order(reverse);
        }

        internal RealValue Min()
        {
            var v = _rows[0].Min();
            var min = v.D;
            var u = v.Units;
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                v = _rows[i].Min();
                var b = v.D * Unit.Convert(u, v.Units, ',');
                if (b < min)
                    min = b;
            }
            if (!IsStructurallyConsistentType && min > 0)
                min = 0;

            return new(min, u);
        }

        internal RealValue Max()
        {
            var v = _rows[0].Max();
            var max = v.D;
            var u = v.Units;
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                v = _rows[i].Max();
                var b = v.D * Unit.Convert(u, v.Units, ',');
                if (b > max)
                    max = b;
            }
            if (!IsStructurallyConsistentType && max < 0)
                max = 0;

            return new(max, u);
        }

        internal virtual RealValue Sum()
        {
            var v = _rows[0].Sum();
            var sum = v.D;
            var u = v.Units;
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                v = _rows[i].Sum();
                sum += v.D * Unit.Convert(u, v.Units, ',');
            }
            return new(sum, u);
        }

        internal virtual RealValue SumSq()
        {
            var v = _rows[0].SumSq();
            var sumsq = v.D;
            var u = v.Units;
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                v = _rows[i].SumSq();
                sumsq += v.D * Unit.Convert(u, v.Units, ',');
            }
            return new(sumsq, u);
        }

        internal RealValue Srss()
        {
            var sumsq = SumSq();
            var srss = Math.Sqrt(sumsq.D);
            var u = sumsq.Units;
            return new(srss, u?.Pow(0.5f));
        }

        internal RealValue Average()
        {
            var sum = Sum();
            var n = _rowCount * _colCount;
            return new(sum.D / n, sum.Units);
        }

        internal virtual RealValue Product()
        {
            var v = _rows[0].Product();
            var product = v.D;
            var u = v.Units;
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                v = _rows[i].Product();
                u = Unit.Multiply(u, v.Units, out var b);
                product *= v.D * b;
            }
            if (!IsStructurallyConsistentType)
                return new(0d, u);

            return new(product, u);
        }

        internal RealValue Mean()
        {
            var product = Product();
            var n = _rowCount * _colCount;
            var result = Math.Pow(product.D, 1d / n);
            var u = product.Units;
            if (u is null)
                return new(result);

            u = Unit.Root(u, n);
            return new(result, u);
        }

        internal RealValue And()
        {
            if (!IsStructurallyConsistentType)
                return RealValue.Zero;

            for (int i = 0, len = _rows.Length; i < len; ++i)
                if (Math.Abs(_rows[i].And().D) < RealValue.LogicalZero)
                    return RealValue.Zero;

            return RealValue.One;
        }

        internal RealValue Or()
        {
            for (int i = 0, len = _rows.Length; i < len; ++i)
                if (Math.Abs(_rows[i].Or().D) >= RealValue.LogicalZero)
                    return RealValue.One;

            return RealValue.Zero;
        }

        internal RealValue Xor()
        {
            var b = Math.Abs(_rows[0].Xor().D) >= RealValue.LogicalZero;
            for (int i = 1, len = _rows.Length; i < len; ++i)
                b = b != Math.Abs(_rows[i].Xor().D) >= RealValue.LogicalZero;

            return b ? RealValue.One : RealValue.Zero;
        }

        internal RealValue Gcd()
        {
            var v = _rows[0].Gcd();
            var a = Calculator.AsLong(v.D);
            var u = v.Units;
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                v = _rows[i].Gcd();
                var b = Calculator.AsLong(v.D * Unit.Convert(u, v.Units, ','));
                a = Calculator.Gcd(a, b);
            }
            return new(a);
        }

        internal RealValue Lcm()
        {
            var v = _rows[0].Lcm();
            var a = Calculator.AsLong(v.D);
            var u = v.Units;
            for (int i = 1, len = _rows.Length; i < len; ++i)
            {
                v = _rows[i].Lcm();
                var b = Calculator.AsLong(v.D * Unit.Convert(u, v.Units, ','));
                a = a * b / Calculator.Gcd(a, b);
            }
            return new(a);
        }

        internal RealValue Take(in RealValue x, in RealValue y)
        {
            var dx = Math.Round(x.D, MidpointRounding.AwayFromZero);
            var dy = Math.Round(y.D, MidpointRounding.AwayFromZero);
            if (!double.IsNormal(dy) || dy < Calculator.DeltaMinus || dy > _rowCount * Calculator.DeltaPlus ||
                !double.IsNormal(dx) || dx < Calculator.DeltaMinus || dx > _colCount * Calculator.DeltaPlus)
                return RealValue.NaN;

            return this[(int)dy - 1, (int)dx - 1];
        }

        internal RealValue Line(in RealValue x, in RealValue y)
        {
            var dx = x.D;
            var dy = y.D;
            if (!double.IsNormal(dy) || dy < Calculator.DeltaMinus || dy > _rowCount * Calculator.DeltaPlus ||
                !double.IsNormal(dx) || dx < Calculator.DeltaMinus || dx > _colCount * Calculator.DeltaPlus)
                return RealValue.NaN;

            var j = (int)Math.Floor(dx);
            var i = (int)Math.Floor(dy);
            var z11 = this[i - 1, j - 1];
            if (j == dx || dx >= _colCount)
            {
                if (i == dy || dy >= _rowCount)
                    return z11;

                var z21_ = this[i, j - 1];
                return z11 + (z21_ - z11) * (dy - i);
            }
            if (i == dy || dy >= _rowCount)
            {
                var z12_ = this[i - 1, j];
                return z11 + (z12_ - z11) * (dx - j);
            }
            var z12 = this[i - 1, j];
            var z21 = this[i, j - 1];
            var z22 = this[i, j];
            var z1 = z11 + (z12 - z11) * (dx - j);
            var z2 = z21 + (z22 - z21) * (dx - j);
            return z1 + (z2 - z1) * (dy - i);
        }

        internal RealValue Spline(in RealValue x, in RealValue y)
        {
            var dx = x.D;
            var dy = y.D;
            if (!double.IsNormal(dy) || dy < Calculator.DeltaMinus || dy > _rowCount * Calculator.DeltaPlus ||
                !double.IsNormal(dx) || dx < Calculator.DeltaMinus || dx > _colCount * Calculator.DeltaPlus)
                return RealValue.NaN;

            var j = (int)Math.Floor(dx) - 1;
            var i = (int)Math.Floor(dy) - 1;
            var v = SplineRow(i, j, dx);
            if (i == dy || dy >= _rowCount)
                return v;
            var u = v.Units;
            var z0 = v.D;
            v = SplineRow(i + 1, j, dx);
            var z1 = v.D * Unit.Convert(u, v.Units, ',');
            var dz = z1 - z0;
            var a = dz;
            var b = dz;
            dz = Math.Sign(dz);
            if (i > 0)
            {
                v = SplineRow(i - 1, j, dx);
                var z2 = v.D * Unit.Convert(u, v.Units, ',');
                a = (z1 - z2) * (Math.Sign(z0 - z2) == dz ? 0.5 : 0.25);
            }
            if (i < _rowCount - 2)
            {
                v = SplineRow(i + 2, j, dx);
                var z2 = v.D * Unit.Convert(u, v.Units, ',');
                b = (z2 - z0) * (Math.Sign(z2 - z1) == dz ? 0.5 : 0.25);
            }
            if (i == 0)
                a += (a - b) / 2;

            if (i == _rowCount - 2)
                b += (b - a) / 2;

            var t = dy - i - 1d;
            var z = z0 + ((z1 - z0) * (3 - 2 * t) * t + ((a + b) * t - a) * (t - 1)) * t;
            return new(z, u);
        }

        private RealValue SplineRow(int i, int j, double dx)
        {
            var v = this[i, j];
            if (j == dx || dx >= _colCount)
                return v;

            var u = v.Units;
            var y0 = v.D;
            v = this[i, j + 1];
            var y1 = v.D * Unit.Convert(u, v.Units, ',');
            var dy = y1 - y0;
            var a = dy;
            var b = dy;
            dy = Math.Sign(dy);
            if (j > 0)
            {
                v = this[i, j - 1];
                var y2 = v.D * Unit.Convert(u, v.Units, ',');
                a = (y1 - y2) * (Math.Sign(y0 - y2) == dy ? 0.5 : 0.25);
            }
            if (j < _colCount - 2)
            {
                v = this[i, j + 2];
                var y2 = v.D * Unit.Convert(u, v.Units, ',');
                b = (y2 - y0) * (Math.Sign(y2 - y1) == dy ? 0.5 : 0.25);
            }
            if (j == 0)
                a += (a - b) / 2;

            if (j == _colCount - 2)
                b += (b - a) / 2;

            var t = dx - j - 1d;
            var y = y0 + ((y1 - y0) * (3 - 2 * t) * t + ((a + b) * t - a) * (t - 1)) * t;
            return new(y, u);
        }

        internal virtual Matrix Transpose()
        {
            if (_rowCount == 1)
                return new ColumnMatrix(_rows[0]);

            var M = new Matrix(_colCount, _rowCount);
            for (int i = _colCount - 1; i >= 0; --i)
                M._rows[i] = Col(i + 1);

            return M;
        }

        internal RealValue Trace()
        {
            if (_rowCount != _colCount)
                Throw.MatrixNotSquareException();

            var trace = this[0, 0];
            for (int i = 1; i < _rowCount; ++i)
                trace += this[i, i];

            return trace;
        }

        //Frobenius norm
        internal RealValue FrobNorm() => Srss();

        //L1 norm   
        internal RealValue L1Norm()
        {
            var norm = new ColumnVector(this, 0).L1Norm();
            for (int j = 1; j < _colCount; ++j)
            {
                var colNorm = new ColumnVector(this, j).L1Norm();
                if (colNorm.CompareTo(norm) > 0)
                    norm = colNorm;
            }
            return norm;
        }

        internal RealValue L2Norm()
        {
            GetSVD(out _, out Vector sig, out _);
            return sig.Max();
        }

        //L∞ (Infinity) or Chebyshev norm     
        internal virtual RealValue InfNorm()
        {
            var norm = _rows[0].L1Norm();
            for (int i = 1, n = _rows.Length; i < n; ++i)
            {
                var rowNorm = _rows[i].L1Norm();
                if (rowNorm.CompareTo(norm) > 0)
                    norm = rowNorm;
            }
            return norm;
        }

        internal Matrix LUDecomposition(out int[] indexes)
        {
            var LU = GetLU(out indexes, out double _, out double _);
            if (LU is null)
                Throw.MatrixSingularException();

            return LU;
        }

        protected virtual Matrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            if (_rowCount != _colCount)
                Throw.MatrixNotSquareException();

            var LU = new Matrix(_rowCount, _rowCount);
            var vv = new RealValue[_rowCount];
            indexes = new int[_rowCount];
            RealValue big;
            minPivot = double.MaxValue; //Used to determine singularity
            det = 1d; //Used to compute the sign change of determinant after pivot interchanges
            for (int i = 0; i < _rowCount; ++i)
            {
                indexes[i] = i;
                var row = LU._rows[i];
                for (int j = 0; j < _colCount; ++j)
                    row[j] = this[i, j];
            }
            for (int i = 0; i < _rowCount; ++i)
            {
                var row = LU._rows[i];
                big = RealValue.Abs(row[0]);
                for (int j = 1; j < row.Size; j++)
                {
                    var temp = RealValue.Abs(row[j]);
                    if (temp.CompareTo(big) > 0)
                        big = temp;
                }
                if (big.D == 0d)
                    return null;

                vv[i] = RealValue.One / big;
            }
            for (int j = 0; j < _colCount; ++j)
            {
                for (int i = 0; i < j; ++i)
                {
                    var row = LU._rows[i];
                    var sum = row[j];
                    var k0 = 0;
                    while (k0 < i && row[k0].D == 0d) ++k0;
                    for (int k = k0; k < i; ++k)
                        if (j < LU._rows[k].Size)
                            sum -= row[k] * LU[k, j];

                    row[j] = sum;
                }
                big = RealValue.Zero;
                var imax = j;
                for (int i = j; i < _rowCount; ++i)
                {
                    var row = LU._rows[i];
                    var sum = row[j];
                    var k0 = 0;
                    while (k0 < j && row[k0].D == 0d) ++k0;
                    for (int k = k0; k < j; k++)
                        if (j < LU._rows[k].Size)
                            sum -= row[k] * LU[k, j];

                    row[j] = sum;
                    var dum = vv[i] * RealValue.Abs(sum);
                    if (dum.CompareTo(big) > 0)
                    {
                        big = dum;
                        imax = i;
                    }
                }
                var d = Math.Abs(big.D);
                if (j != imax)
                {
                    if (d < minPivot)
                        minPivot = d;

                    (LU._rows[j], LU._rows[imax]) = (LU._rows[imax], LU._rows[j]);
                    vv[imax] = vv[j];
                    (indexes[j], indexes[imax]) = (indexes[imax], indexes[j]);
                    det = -det;
                }
                if (j != _rowCount - 1)
                {
                    var dum = RealValue.One / LU[j, j];
                    for (int i = j + 1; i < _rowCount; ++i)
                        LU[i, j] *= dum;
                }
                else if (d < minPivot)
                    minPivot = d;
            }
            return LU;
        }

        private void GetQR(out Matrix Q, out Matrix R)
        {
            // QR decomposition, Householder algorithm.
            if (_rowCount != _colCount)
                Throw.MatrixNotSquareException();

            var n = _rowCount;
            R = Copy();
            Q = Identity(n);
            var end = n - 1;
            var two = new RealValue(2);
            for (int i = 0; i < end; ++i)
            {
                var H = Identity(n);
                var a = new Vector(n - i);
                var k = 0;
                for (int ii = i; ii < n; ++ii)
                    a[k++] = R[ii, i];

                var normA = a.Norm();
                if (a[0].D < 0.0)
                    normA = -normA;

                var v = new Vector(a.Length);
                for (int j = 0; j < v.Length; ++j)
                    v[j] = a[j] / (a[0] + normA);

                v[0] = RealValue.One;

                var h = Identity(a.Length);
                var vvDot = v.SumSq();
                var A = new ColumnMatrix(v);
                var B = new Matrix(v);
                var C = A * B;
                for (int ii = 0; ii < h._rowCount; ++ii)
                    for (int jj = 0; jj < h._colCount; ++jj)
                        h[ii, jj] -= (two / vvDot) * C[ii, jj];

                // copy h into lower right of H
                int d = n - h._rowCount;
                for (int ii = 0; ii < h._colCount; ++ii)
                    for (int jj = 0; jj < h._colCount; ++jj)
                        H[ii + d, jj + d] = h[ii, jj];

                Q *= H;
                R = H * R;
            }
        }
        protected static RealValue CopySign(in RealValue a, in RealValue b) => 
            b.D >= 0.0 ? RealValue.Abs(a) : -RealValue.Abs(a);
        protected static RealValue Sqr(in RealValue x) => x * x;
        protected static RealValue Hypot(in RealValue a, in RealValue b)
        {
            var a1 = RealValue.Abs(a);
            var b1 = RealValue.Abs(b);
            if (a1.CompareTo(b1) > 0)
                return a1 * RealValue.Sqrt(RealValue.One + Sqr(b1 / a1));
            else
                return b1.D == 0.0 ? b1 : b1 * RealValue.Sqrt(RealValue.One + Sqr(a1 / b1));
        }

        private void GetSVD(out Matrix U, out Vector sig, out Matrix V)
        {
            if (_rowCount < _colCount)
                Throw.MatrixNotHighException();

            var m = _rowCount - 1;
            var n = _colCount - 1;
            var rv1 = new Vector(_colCount);
            U = Copy();
            sig = new Vector(_colCount);
            V = new Matrix(_colCount, _colCount);
            SvdBidiagonalize(U, sig, rv1, m, n, out RealValue anorm);
            SvdRightTransform(U, V, rv1, n);
            SvdLeftTransform(U, sig, m ,n);
            SvdDiagonalize(U, sig, V, rv1,m, n, anorm);
        }
        
        private static void SvdBidiagonalize(Matrix U, Vector sig, Vector rv1, int m, int n, out RealValue anorm)
        {
            RealValue sum;            
            RealValue scale, g;
            scale = g = anorm = RealValue.Zero;
            for (int i = 0; i <= n; ++i)
            {
                var l = i + 1;
                rv1[i] = scale * g;
                if (i <= m)
                {
                    scale = RealValue.Abs(U[i, i]);
                    for (int k = i + 1; k <= m; ++k)
                        scale += RealValue.Abs(U[k, i]);

                    if (scale.D != 0)
                    {
                        U[i, i] /= scale;
                        sum = Sqr(U[i, i]);
                        for (int k = i + 1; k <= m; ++k)
                        {
                            U[k, i] /= scale;
                            sum += Sqr(U[k, i]);
                        }
                        var f = U[i, i];
                        g = -CopySign(RealValue.Sqrt(sum), f);
                        var h = f * g - sum;
                        U[i, i] = f - g;
                        for (int j = l; j <= n; ++j)
                        {
                            sum = U[i, i] * U[i, j];
                            for (int k = l; k <= m; ++k)
                                sum += U[k, i] * U[k, j];

                            f = sum / h;
                            for (int k = i; k <= m; ++k)
                                U[k, j] += f * U[k, i];
                        }
                        for (int k = i; k <= m; ++k)
                            U[k, i] *= scale;
                    }
                }
                sig[i] = scale * g;
                if (i <= m && i != n)
                {
                    scale = RealValue.Abs(U[i, l]);
                    for (int k = l + 1; k <= n; ++k)
                        scale += RealValue.Abs(U[i, k]);

                    if (scale.D != 0)
                    {
                        U[i, l] /= scale;
                        sum = Sqr(U[i, l]);
                        for (int k = l + 1; k <= n; ++k)
                        {
                            U[i, k] /= scale;
                            sum += Sqr(U[i, k]);
                        }
                        var f = U[i, l];
                        g = -CopySign(RealValue.Sqrt(sum), f);
                        var h = f * g - sum;
                        U[i, l] = f - g;
                        for (int k = l; k <= n; ++k)
                            rv1[k] = U[i, k] / h;

                        for (int j = l; j <= m; ++j)
                        {
                            sum = U[j, l] * U[i, l];
                            for (int k = l + 1; k <= n; ++k)
                                sum += U[j, k] * U[i, k];

                            for (int k = l; k <= n; ++k)
                                U[j, k] += sum * rv1[k];
                        }
                        for (int k = l; k <= n; ++k)
                            U[i, k] *= scale;
                    }
                }

                var a = RealValue.Abs(sig[i]);
                var rvi = rv1[i];                
                if (rvi.D != 0)
                    a += RealValue.Abs(rvi);

                if (anorm.D == 0 || a.CompareTo(anorm) > 0)
                    anorm = a;
            }
        }

        private static void SvdRightTransform(Matrix U, Matrix V, Vector rv1, int n)
        {
            RealValue sum;
            var l = n + 1;
            var g = RealValue.Zero;
            for (int i = n; i >= 0; --i)
            {
                if (i < n)
                {
                    if (g.D != 0)
                    {
                        for (int j = l; j <= n; ++j)
                            V[j, i] = (U[i, j] / U[i, l]) / g;

                        for (int j = l; j <= n; ++j)
                        {
                            sum = U[i, l] * V[l, j];
                            for (int k = l + 1; k <= n; ++k)
                                sum += U[i, k] * V[k, j];

                            for (int k = l; k <= n; ++k)
                                V[k, j] += sum * V[k, i];
                        }
                    }
                    for (int j = l; j <= n; ++j)
                        V[i, j] = V[j, i] = RealValue.Zero;
                }
                V[i, i] = RealValue.One;
                g = rv1[i];
                l = i;
            }
        }

        private static void SvdLeftTransform(Matrix U, Vector sig, int m, int n)
        {
            for (int i = Math.Min(m, n); i >= 0; --i)
            {
                var l = i + 1;
                var g = sig[i];
                for (int j = l; j <= n; ++j)
                    U[i, j] = RealValue.Zero;

                if (g.D != 0)
                {
                    g = RealValue.One / g;
                    for (int j = l; j <= n; ++j)
                    {
                        var sum = U[l, i] * U[l, j];
                        for (int k = l + 1; k <= m; ++k)
                            sum += U[k, i] * U[k, j];

                        var f = (sum / U[i, i]) * g;
                        for (int k = i; k <= m; ++k)
                            U[k, j] += f * U[k, i];
                    }
                    for (int j = i; j <= m; ++j)
                        U[j, i] *= g;
                }
                else
                    for (int j = i; j <= m; ++j)
                        U[j, i] = RealValue.Zero;

                U[i, i] += RealValue.One;
            }
        }

        private static void SvdDiagonalize(Matrix U, Vector sig, Matrix V, Vector rv1, int m, int n, RealValue anorm)
        {
            RealValue g;
            for (int k = n; k >= 0; --k)
            {
                for (int iter = 1; iter <= 30; ++iter)
                {
                    var flag = 1;
                    var nm = 0;
                    int l;
                    for (l = k; l >= 0; --l)
                    {
                        if (rv1[l].D == 0 || (RealValue.Abs(rv1[l]) + anorm).CompareTo(anorm) == 0)
                        {
                            flag = 0;
                            break;
                        }
                        if ((RealValue.Abs(sig[nm]) + anorm).CompareTo(anorm) == 0)
                            break;
                    }
                    nm = l - 1;
                    RealValue f, h, c, s;
                    if (flag != 0)
                    {
                        c = RealValue.Zero;
                        s = RealValue.One;
                        for (int i = l; i <= k; ++i)
                        {
                            f = s * rv1[i];
                            rv1[i] = c * rv1[i];
                            if ((RealValue.Abs(f) + anorm).CompareTo(anorm) == 0)
                                break;

                            g = sig[i];
                            h = Hypot(f, g);
                            sig[i] = h;
                            h = RealValue.One / h;
                            c = g * h;
                            s = -f * h;
                            for (int j = 0; j <= m; ++j)
                            {
                                var Uy = U[j, nm];
                                var Uz = U[j, i];
                                U[j, nm] = Uy * c + Uz * s;
                                U[j, i] = Uz * c - Uy * s;
                            }
                        }
                    }
                    var z = sig[k];
                    if (l == k)
                    {
                        if (z.D < 0d)
                        {
                            sig[k] = -z;
                            for (int j = 0; j <= n; ++j)
                                V[j, k] = -V[j, k];
                        }
                        break;
                    }
                    if (iter == 30)
                        throw new MathParser.MathParserException("no convergence in 30 svdcmp iterations");

                    var x = sig[l];
                    nm = k - 1;
                    var y = sig[nm];
                    g = rv1[nm];
                    h = rv1[k];
                    if (g.D == 0)
                        f = ((y - z) * (y + z) - h * h) / (h * y * 2d);
                    else
                        f = ((y - z) * (y + z) + (g - h) * (g + h)) / (h * y * 2d);
                    g = Hypot(f, RealValue.One);
                    f = ((x - z) * (x + z) + h * ((y / (f + CopySign(g, f))) - h)) / x;
                    c = RealValue.One;
                    s = RealValue.One;
                    for (int j = l; j <= nm; ++j)
                    {
                        var i = j + 1;
                        g = rv1[i];
                        y = sig[i];
                        h = s * g;
                        g = c * g;
                        z = Hypot(f, h);
                        rv1[j] = z;
                        c = f / z;
                        s = h / z;
                        f = x * c + g * s;
                        g = g * c - x * s;
                        h = y * s;
                        y *= c;
                        for (int jj = 0; jj <= n; ++jj)
                        {
                            x = V[jj, j];
                            z = V[jj, i];
                            V[jj, j] = x * c + z * s;
                            V[jj, i] = z * c - x * s;
                        }
                        z = Hypot(f, h);
                        sig[j] = z;
                        if (z.D != 0)
                        {
                            z = RealValue.One / z;
                            c = f * z;
                            s = h * z;
                        }
                        f = c * g + s * y;
                        x = c * y - s * g;
                        for (int jj = 0; jj <= m; ++jj)
                        {
                            y = U[jj, j];
                            z = U[jj, i];
                            U[jj, j] = y * c + z * s;
                            U[jj, i] = z * c - y * s;
                        }
                    }
                    rv1[l] = RealValue.Zero;
                    rv1[k] = f;
                    sig[k] = x;
                }
            }
        }

        private static void SvdSortDescending(ref Matrix U, ref Vector sig, ref Matrix V)
        {
            var indexes = sig.GetOrderIndexes(true);
            var isSorted = true;
            for (int i = 0, n = indexes.Length; i < n; ++i)
                if (indexes[i] != i)
                {
                    isSorted = false;
                    break;
                }

            if (isSorted)
                V = V.Transpose();
            else
            {
                sig = sig.Sort(true);
                U = U.ReorderCols(indexes);
                V = V.ReorderCols(indexes).Transpose();
            }
        }

        /*
        private static void SvdFlipSigns(Matrix U, Matrix V)
        {
            var m = U._rowCount;
            var n = U._colCount;    
            for (int k = 0; k < n; ++k)
            {
                var s = 0;
                for (int i = 0; i < m; ++i)
                    if (U[i, k].Re < 0d)
                        ++s;

                for (int j = 0; j < n; ++j) 
                    if (V[j, k].Re < 0d)
                        ++s;

                if (s > (m + n) / 2)
                {
                    for (int i = 0; i < m; ++i) 
                        U[i, k] = -U[i, k];

                    for (int j = 0; j < n; ++j) 
                        V[j, k] = -V[j, k];
                }
            }
        }
        */

        internal Matrix QRDecomposition()
        {
            GetQR(out Matrix Q, out Matrix R);
            return Augment(Q, R);
        }

        internal Matrix SVDDecomposition()
        {
            GetSVD(out Matrix U, out Vector sig, out Matrix V);
            //SvdFlipSigns(U, V);
            SvdSortDescending(ref U, ref sig, ref V);
            return Augment(U, sig, V);
        }

        protected static Matrix Identity(int n)
        {
            var M = new Matrix(n, n);
            for (int i = 0; i < n; ++i)
                M[i, i] = RealValue.One;

            return M;
        }

        private static void FwdAndBackSubst(Matrix LU, int[] indexes, Vector v, ref RealValue[] x)
        {
            var n = LU._rowCount;
            var start = -1;
            //Forward substitution. Solve Ly = v by storing y in x. 
            for (int i = 0; i < n; ++i)
            {
                var index = indexes[i];
                var sum = v[index];
                if (start >= 0)
                    for (int j = start; j < i; ++j)
                        sum -= LU[i, j] * x[j];
                else if (!sum.Equals(RealValue.Zero))
                    start = i;

                x[i] = sum;
            }
            //Back substitution. Solve Ux = y.
            for (int i = n - 1; i >= 0; --i)
            {
                var sum = x[i];
                for (int j = i + 1; j < n; ++j)
                    sum -= LU[i, j] * x[j];

                var LU_ii = LU[i, i];
                x[i] = sum / LU_ii;
            }
        }

        internal virtual Vector LSolve(Vector v)
        {
            var LU = GetLU(out int[] indexes, out double minPivot, out double _);
            if (LU is null)
                Throw.MatrixSingularException();

            if (minPivot < 1e-15)
                Throw.MatrixCloseToSingularException();

            var x = new RealValue[_rowCount];
            FwdAndBackSubst(LU, indexes, v, ref x);
            return new(x);
        }

        internal virtual Matrix MSolve(Matrix M)
        {
            var LU = GetLU(out int[] indexes, out double minPivot, out double _);
            if (LU is null)
                Throw.MatrixSingularException();

            if (minPivot < 1e-15)
                Throw.MatrixCloseToSingularException();

            var m = _rowCount;
            var n = M._colCount;
            var v = new Vector[n];
            Parallel.For(0, n, j =>
            {
                var x = new RealValue[m];
                FwdAndBackSubst(LU, indexes, M.Col(j + 1), ref x);
                v[j] = new(x);
            });
            return CreateFromCols(v, m);
        }

        internal virtual Matrix Invert()
        {
            var LU = GetLU(out int[] indexes, out double minPivot, out double _);
            if (LU is null)
                Throw.MatrixSingularException();

            if (minPivot < 1e-15)
                Throw.MatrixCloseToSingularException();

            var M = GetInverse(LU, indexes);
            return M;
        }

        private Matrix GetInverse(Matrix lu, int[] indexes)
        {
            var vector = new Vector(_rowCount);
            var x = new RealValue[_rowCount];
            var M = new Matrix(_rowCount, _rowCount);
            for (int j = 0; j < _rowCount; ++j)  //Find inverse by columns.
            {
                vector[j] = RealValue.One;
                FwdAndBackSubst(lu, indexes, vector, ref x);
                for (int i = 0; i < _rowCount; ++i)
                    M[i, j] = x[i];

                vector[j] = RealValue.Zero;
            }
            return M;
        }

        internal virtual RealValue Determinant()
        {
            if (_rowCount != _colCount)
                Throw.MatrixNotSquareException();

            var LU = GetLU(out int[] _, out double _, out double det);
            if (LU is null)
                return RealValue.Zero;

            return GetDeterminant(LU) * det;
        }

        private RealValue GetDeterminant(Matrix lu)
        {
            var det = lu._rows[0][0];
            for (int i = 1; i < _rowCount; ++i)
                det *= lu._rows[i][i];

            return det;
        }

        internal RealValue Cond2()
        {
            GetSVD(out Matrix _, out Vector sig, out Matrix _);
            return sig.Max() / sig.Min();
        }

        internal RealValue Cond1() => Condition(M => M.L1Norm());
        internal RealValue CondFrob() => Condition(M => M.FrobNorm());
        internal RealValue CondInf() => Condition(M => M.InfNorm());

        protected virtual RealValue Condition(Func<Matrix, RealValue> norm)
        {
            if (_rowCount != _colCount)
                Throw.MatrixNotSquareException();

            var LU = GetLU(out int[] indexes, out double _, out double _); //Decompose the matrix by LU decomp.
            if (LU is null)
                return RealValue.PositiveInfinity;

            var M = GetInverse(LU, indexes);
            return norm(this) * norm(M);
        }

        internal RealValue Rank()
        {
            var rank = 0;
            GetSVD(out Matrix _, out Vector sig, out Matrix _);
            var sigMax = sig.Max().D; 
            var eps = (double.BitIncrement(sigMax) - sigMax);
            for (int i = 0, len = sig.Length; i < len; ++i)
            {
                if (Math.Abs(sig[i].D) > eps)
                    ++rank;
            }
            return new(rank);
        }

        internal virtual Matrix Adjoint()
        {
            var LU = GetLU(out int[] indexes, out double _, out double det);
            if (LU is null)
                Throw.MatrixSingularException();

            var determinant = GetDeterminant(LU) * det;
            var M = GetInverse(LU, indexes);
            return M * determinant;
        }
        internal Matrix Cofactor() => Adjoint().Transpose();

        internal virtual Matrix CopyTo(Matrix target, int i, int j)
        {
            --i; --j;
            var m = Math.Min(_rowCount, target._rowCount - i);
            var n = Math.Min(_colCount, target._colCount - j);
            for (int ii = 0; ii < m; ++ii)
            {
                var i1 = i + ii;
                for (int jj = 0; jj < n; ++jj)
                {
                    var j1 = j + jj;
                    if (IsTarget(target._type, i1, j1))
                        target[i1, j1] = this[ii, jj];
                }
            }
            return target;
        }

        internal virtual Matrix AddTo(Matrix target, int i, int j)
        {
            --i; --j;
            var m = Math.Min(_rowCount, target._rowCount - i);
            var n = Math.Min(_colCount, target._colCount - j);
            for (int ii = 0; ii < m; ++ii)
            {
                var i1 = i + ii;
                for (int jj = 0; jj < n; ++jj)
                {
                    var j1 = j + jj;
                    if (IsTarget(target._type, i1, j1))
                    {
                        var v = target[i1, j1];
                        if (v.Equals(RealValue.Zero))
                            target[i1, j1] = this[ii, jj];
                        else
                            target[i1, j1] = v + this[ii, jj];
                    }
                }
            }
            return target;
        }

        internal Vector Diagonal()
        {
            var n = Math.Min(_rowCount, _colCount);
            Vector v = new(n);
            for (int i = 0; i < n; ++i)
                v[i] = this[i, i];

            return v;
        }

        private static bool IsTarget(MatrixType type, int i, int j)
        {
            if (type == MatrixType.Full)
                return true;

            if (type == MatrixType.Symmetric || type == MatrixType.UpperTriangular)
                return j >= i;

            if (type == MatrixType.LowerTriangular)
                return j <= i;

            if (type == MatrixType.Diagonal)
                return i == j;

            return true;
        }

        internal static Matrix CreateFromCols(Vector[] cols, int m)
        {
            var n = cols.Length;
            var M = new Matrix(m, n);
            for (int j = n - 1; j >= 0; --j)
                M.SetCol(j, cols[j]); 

            return M;
        }

        internal static Matrix CreateFromRows(Vector[] rows, int n)
        {
            var m = rows.Length;
            var M = new Matrix(m, n);
            for (int i = m - 1; i >= 0; --i)
                M.SetRow(i, rows[i]);

            return M;
        }
    }
}