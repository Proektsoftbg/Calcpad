using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class Matrix : IValue, IEquatable<Matrix>
    {
        internal const int ParallelThreshold = 100;
        internal const int MaxSize = 1000000;

        protected int _rowCount;
        protected int _colCount;
        protected LargeVector[] _rows;

        internal int RowCount => _rowCount;
        internal int ColCount => _colCount;
        internal LargeVector[] Rows => _rows;

        protected MatrixType _type;

        internal event Action OnChange;
        internal void Change() => OnChange?.Invoke();
        internal MatrixType Type => _type;
        internal enum MatrixType
        {
            Full,
            UpperTriangular,
            LowerTriangular,
            Diagonal,
            Column,
            Symmetric
        }

        protected bool IsStructurallyConsistentType =>
            _type == MatrixType.Full ||
            _type == MatrixType.Column ||
            _type == MatrixType.Symmetric;

        protected Matrix() { }

        protected Matrix(int length)
        {
            if (length > MaxSize)
                throw Exceptions.MatirixSizeLimit();

            _rowCount = length;
            _colCount = length;
        }

        internal Matrix(int rows, int cols)
        {
            if (rows > MaxSize || cols > MaxSize)
                throw Exceptions.MatirixSizeLimit();

            _type = MatrixType.Full;
            _rowCount = rows;
            _colCount = cols;
            _rows = new LargeVector[rows];
            for (int i = rows - 1; i >= 0; --i)
                _rows[i] = new LargeVector(cols);
        }

        internal Matrix(Vector v)
        {
            if (v.Length > MaxSize)
                throw Exceptions.MatirixSizeLimit();

            _type = MatrixType.Full;
            _rowCount = 1;
            _colCount = v.Length;
            _rows = v is LargeVector lv ? [lv] : [new LargeVector(v.Raw)];
        }

        internal virtual RealValue this[int row, int col]
        {
            get => _rows[row][col];
            set => _rows[row][col] = value;
        }

        internal virtual Matrix Clone() => new(_rowCount, _colCount);

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
            if (m > ParallelThreshold)
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
            if (a._rowCount != b._rowCount || a._colCount != b._colCount)
                throw Exceptions.MatrixDimensions();
        }

        protected virtual Matrix Copy()
        {
            var M = new Matrix(_rowCount, _colCount);
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
            {
                if (_rowCount > ParallelThreshold)
                    Parallel.For(0, _rowCount, i => M._rows[i] = _rows[i].Copy());
                else
                    for (int i = 0; i < _rowCount; ++i) 
                        M._rows[i] = _rows[i].Copy();
            }
            else
            {
                if (_rowCount > ParallelThreshold)
                    Parallel.For(0, _rowCount, CopyRow);
                else
                    for (int i = _rowCount - 1; i >= 0; --i) 
                        CopyRow(i);
            }
            return M;

            void CopyRow(int i)
            {
                var row = M._rows[i];
                for (int j = _colCount - 1; j >= 0; --j)
                    row[j] = this[i, j];
            }
        }

        internal virtual Matrix Resize(int m, int n)
        {
            if (m > MaxSize || n > MaxSize)
                throw Exceptions.MatrixDimensions();

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
            if (m > ParallelThreshold)
                Parallel.For(0, m, i =>
                    c._rows[i] = -a._rows[i] as LargeVector);
            else
                for (int i = m - 1; i >= 0; --i)
                    c._rows[i] = -a._rows[i] as LargeVector;

            return c;
        }

        public static Matrix operator -(Matrix a, Matrix b)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type)
            {
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] - b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] - b._rows[i] as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] - b as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] - b as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    c._rows[i] = a - b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a - b._rows[i] as LargeVector;
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] + b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] + b._rows[i] as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] + b as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] + b as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                throw Exceptions.MatrixDimensions();

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
            if (a._type == MatrixType.Full || a._type == MatrixType.LowerTriangular)
            {
                var bColVectors = new ColumnVector[b._colCount];
                if (m > ParallelThreshold)
                {
                    Parallel.For(0, b._colCount, j => bColVectors[j] = b.GetColumnVector(j));
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._rows[i];
                        var ar = a._rows[i];
                        for (int j = nb; j >= 0; --j)
                            cr[j] = Vector.DotProduct(ar, bColVectors[j]);
                    });                
                }
                else
                {
                    for (int j = nb; j >= 0; --j)
                        bColVectors[j] = b.GetColumnVector(j);

                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._rows[i];
                        var ar = a._rows[i];
                        for (int j = nb; j >= 0; --j)
                            cr[j] = Vector.DotProduct(ar, bColVectors[j]);
                    }
                }
            }
            else
            {
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] * b as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] * b as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] / b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] / b._rows[i] as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] / b as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] / b as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a / b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a / b._rows[i] as LargeVector;
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] % b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] % b._rows[i] as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] % b as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] % b as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a % b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a % b._rows[i] as LargeVector;
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] == b._rows[i]) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] == b._rows[i]) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] == b) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] == b) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] != b._rows[i]) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] != b._rows[i]) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] != b) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] != b) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] < b._rows[i]) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] < b._rows[i]) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] < b as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] < b as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a < b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a < b._rows[i] as LargeVector;
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] > b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] > b._rows[i] as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] > b as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] > b as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a > b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a > b._rows[i] as LargeVector;
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] >= b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] >= b._rows[i] as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] >= b as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] >= b as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a >= b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a >= b._rows[i] as LargeVector;
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] <= b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] <= b._rows[i] as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] <= b as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] <= b as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a <= b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a <= b._rows[i] as LargeVector;
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] & b._rows[i]) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] & b._rows[i]) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] & b) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] & b) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] | b._rows[i]) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] | b._rows[i]) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] | b) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] | b) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] ^ b._rows[i]) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] ^ b._rows[i]) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = (a._rows[i] ^ b) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = (a._rows[i] ^ b) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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

        public static implicit operator Matrix(Vector vector) =>
            vector is HpVector hp_v ?
            new HpColumnMatrix(hp_v) :
            new ColumnMatrix(vector);

        internal static Matrix EvaluateOperator(Calculator.Operator<RealValue> op, Matrix a, Matrix b, long index) =>
            Calculator.GetOperatorSymbol(index) switch
            {
                '/' => a / b,
                '⦼' => a % b,
                '*' => a * b,
                '-' => a - b,
                '+' => a + b,
                '<' => a < b,
                '>' => a > b,
                '≤' => a <= b,
                '≥' => a >= b,
                '≡' => a == b,
                '≠' => a != b,
                '∧' => a & b,
                '∨' => a | b,
                '⊕' => a ^ b,
                _ => EvaluateOperator2(op, a, b, index)
            };

        internal static Matrix EvaluateOperator(Calculator.Operator<RealValue> op, Matrix a, RealValue b, long index) =>
            Calculator.GetOperatorSymbol(index) switch
            {
                '/' => a / b,
                '⦼' => a % b,
                '*' => a * b,
                '-' => a - b,
                '+' => a + b,
                '<' => a < b,
                '>' => a > b,
                '≤' => a <= b,
                '≥' => a >= b,
                '≡' => a == b,
                '≠' => a != b,
                '∧' => a & b,
                '∨' => a | b,
                '⊕' => a ^ b,
                _ => EvaluateOperator2(op, a, b, index)
            };

        internal static Matrix EvaluateOperator(Calculator.Operator<RealValue> op, RealValue a, Matrix b, long index) =>
            Calculator.GetOperatorSymbol(index) switch
            {
                '/' => a / b,
                '⦼' => a % b,
                '*' => b * a,
                '-' => a - b,
                '+' => b + a,
                '<' => a < b,
                '>' => a > b,
                '≤' => a <= b,
                '≥' => a >= b,
                '≡' => b == a,
                '≠' => b != a,
                '∧' => b & a,
                '∨' => b | a,
                '⊕' => b ^ a,
                _ => EvaluateOperator2(op, a, b, index)
            };

        private static Matrix EvaluateOperator2(Calculator.Operator<RealValue> op, Matrix a, Matrix b, long index)
        {
            CheckBounds(a, b);
            Matrix c;
            if (a._type == b._type && (a.IsStructurallyConsistentType || Calculator.IsZeroPreservingOperator(index)))
            {
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = Vector.EvaluateOperator(op, a._rows[i], b._rows[i], index) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = Vector.EvaluateOperator(op, a._rows[i], b._rows[i], index) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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

        internal static Matrix EvaluateOperator2(Calculator.Operator<RealValue> op, Matrix a, RealValue b, long index)
        {
            Matrix c;
            if (a.IsStructurallyConsistentType || Calculator.IsZeroPreservingOperator(index) && a.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = Vector.EvaluateOperator(op, a._rows[i], b, index) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = Vector.EvaluateOperator(op, a._rows[i], b, index) as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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

        internal static Matrix EvaluateOperator2(Calculator.Operator<RealValue> op, RealValue a, Matrix b, long index)
        {
            Matrix c;
            if (b.IsStructurallyConsistentType || Calculator.IsZeroPreservingOperator(index) && a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                var m = b._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = Vector.EvaluateOperator(op, a, b._rows[i], index) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = Vector.EvaluateOperator(op, a, b._rows[i], index) as LargeVector;
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new Matrix(m, n);
                --n;
                if (m > ParallelThreshold)
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
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = Vector.EvaluateFunction(f, a._rows[i]) as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = Vector.EvaluateFunction(f, a._rows[i]) as LargeVector;

            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n);
                --n;
                if (m > ParallelThreshold)
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

        internal void SetUnits(Unit units)
        {
            var n = _rows.Length - 1;
            for (int i = n; i >= 0; --i)
                _rows[i].SetUnits(units);
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
                c = a is HpMatrix hp_a ? hp_a.CloneAsMatrix() : a.Clone();
                var m = a._rows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._rows[i] = a._rows[i] * b._rows[i] as LargeVector);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._rows[i] = a._rows[i] * b._rows[i] as LargeVector;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                if (a._type == MatrixType.Column && b._type == MatrixType.Column)
                    c = new ColumnMatrix(m);
                else
                    c = new Matrix(m, n);

                --n;
                if (m > ParallelThreshold)
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
            if (m > ParallelThreshold)
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
            if (m > ParallelThreshold)
                Parallel.For(0, m, i =>
                    sum += Vector.DotProduct(a._rows[i], b._rows[i]));
            else
                for (int i = m - 1; i >= 0; --i)
                    sum += Vector.DotProduct(a._rows[i], b._rows[i]);

            return sum;
        }

        internal virtual Vector Row(int index)
        {
            if (index < 1 || index > _rowCount)
                throw Exceptions.IndexOutOfRange(index.ToString());

            --index;
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
                return _rows[index].Copy();

            var row = new LargeVector(_colCount);
            for (int j = _colCount - 1; j >= 0; --j)
                row[j] = this[index, j];

            return row;
        }

        internal virtual LargeVector Col(int index)
        {
            if (index < 1 || index > _colCount)
                throw Exceptions.IndexOutOfRange(index.ToString());

            --index;
            if (this is ColumnMatrix cm)
                return cm._rows[index].Copy();

            var col = new LargeVector(_rowCount);
            for (int i = _rowCount - 1; i >= 0; --i)
                col[i] = this[i, index];

            return col;
        }

        private ColumnVector GetColumnVector(int index) => new(this, index);

        private void SetRow(int index, Vector row) =>
            _rows[index] = row is LargeVector lv ?
                lv.Resize(_colCount) :
                new LargeVector(row.Raw).Resize(_colCount);

        private void SetCol(int index, Vector col)
        {
            var n = Math.Min(col.Length, _rowCount) - 1;
            for (int i = n; i >= 0; --i)
                this[i, index] = col[i];
        }

        internal virtual Matrix Submatrix(int i1, int i2, int j1, int j2)
        {
            if (i2 < i1)
                (i1, i2) = (i2, i1);
            if (j2 < j1)
                (j1, j2) = (j2, j1);
            if (i1 < 1)
                throw Exceptions.IndexOutOfRange(i1.ToString());
            if (i2 > _rowCount)
                throw Exceptions.IndexOutOfRange(i2.ToString());
            if (j1 < 1)
                throw Exceptions.IndexOutOfRange(j1.ToString());
            if (j2 > _colCount)
                throw Exceptions.IndexOutOfRange(j2.ToString());

            var rows = i2 - i1;
            var cols = j2 - j1;
            var M = new Matrix(rows + 1, cols + 1);
            --i1;
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
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

        internal virtual Matrix ExtractRows(Vector vector)
        {
            var rowCount = vector.Length;
            var M = new Matrix(rowCount, _colCount);
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
                for (int i = rowCount - 1; i >= 0; --i)
                {
                    var rowIndex = (int)vector[i].D;
                    if (rowIndex < 1 || rowIndex > _rowCount)
                        throw Exceptions.IndexOutOfRange(rowIndex.ToString());

                    M._rows[i] = _rows[rowIndex - 1];
                }
            else
            {
                var n = _colCount - 1;
                for (int i = rowCount - 1; i >= 0; --i)
                {
                    var rowIndex = (int)vector[i].D;
                    if (rowIndex < 1 || rowIndex > _rowCount)
                        throw Exceptions.IndexOutOfRange(rowIndex.ToString());

                    var row = M._rows[i];
                    for (int j = n; j >= 0; --j)
                        row[j] = this[rowIndex - 1, j];
                }
            }
            return M;
        }

        internal virtual Matrix ExtractCols(Vector vector)
        {
            var colCount = vector.Length;
            var M = new Matrix(_rowCount, colCount);
            var m = _rowCount - 1;
            for (int j = colCount - 1; j >= 0; --j)
            {
                var colIndex = (int)vector[j].D;
                if (colIndex < 1 || colIndex > _colCount)
                    throw Exceptions.IndexOutOfRange(colIndex.ToString());

                for (int i = m; i >= 0; --i)
                    M[i, j] = this[i, colIndex - 1];
            }
            return M;
        }

        internal virtual Matrix Fill(RealValue value)
        {
            var m = _rows.Length;
            if (m > ParallelThreshold)
                Parallel.For(0, m, i =>
                    _rows[i].Fill(value));
            else
                for (int i = m - 1; i >= 0; --i)
                    _rows[i].Fill(value);

            return this;
        }

        internal virtual Matrix FillRow(int i, in RealValue value)
        {
            if (i > _rowCount)
                throw Exceptions.IndexOutOfRange(i.ToString());

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

        internal virtual Matrix FillCol(int j, in RealValue value)
        {
            if (j > _colCount)
                throw Exceptions.IndexOutOfRange(j.ToString());

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
                m = matrix._rowCount - 1;
                n = matrix._colCount - 1;
                for (int i = m; i >= 0; --i)
                {
                    var row = M._rows[i];
                    for (int j = n; j >= 0; --j)
                        row[c + j] = matrix[i, j];
                }
                c += n + 1;
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
                n = matrix._colCount - 1;
                for (int i = m - 1; i >= 0; --i)
                {
                    var row = M._rows[r + i];
                    for (int j = n; j >= 0; --j)
                        row[j] = matrix[i, j];
                }
                r += m;
            }
            return M;
        }

        internal virtual Vector Search(in RealValue value, int iStart, int jStart)
        {
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
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

        internal virtual RealValue Count(RealValue value)
        {
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular || !value.Equals(RealValue.Zero))
            {
                var count = 0d;
                for (int i = 0, len = _rows.Length; i < len; ++i)
                    count += _rows[i].Count(value, 1).D;

                return new(count);
            }
            else
            {
                var count = 0;
                for (int i = 0; i < _rowCount; ++i)
                    for (int j = 0; j < _colCount; ++j)
                        if (this[i, j].AlmostEquals(value))
                            ++count;

                return new(count);
            }
        }

        internal virtual Matrix FindAll(in RealValue value, Vector.Relation rel)
        {
            var M = FromIndexes(FindAllIndexes(value, rel));
            if (M._colCount == 0)
                M = new Matrix(2, 1);

            return M;
        }

        internal virtual Vector HLookup(in RealValue value, int searchRow, int returnRow, Vector.Relation rel)
        {
            if (searchRow < 1 || searchRow > _rowCount)
                throw Exceptions.IndexOutOfRange(searchRow.ToString());
            if (returnRow < 1 || returnRow > _rowCount)
                throw Exceptions.IndexOutOfRange(returnRow.ToString());

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

        internal virtual Vector VLookup(in RealValue value, int searchCol, int returnCol, Vector.Relation rel)
        {
            if (searchCol > _colCount)
                throw Exceptions.IndexOutOfRange(searchCol.ToString());
            if (returnCol > _colCount)
                throw Exceptions.IndexOutOfRange(returnCol.ToString());

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

        private static Matrix FromIndexes(IEnumerable<(int, int)> indexes)
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

        internal virtual Matrix SortRows(int j, bool reverse = false)
        {
            --j;
            var indexes = new ColumnVector(this, j).GetOrderIndexes(reverse);
            return ReorderRows(indexes);
        }

        internal virtual Matrix ReorderRows(int[] indexes)
        {
            Matrix M = new(_rowCount, _colCount);
            var m = _rowCount - 1;
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
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

        internal virtual Matrix SortCols(int i, bool reverse = false)
        {
            if (i > _rowCount)
                throw Exceptions.IndexOutOfRange(i.ToString());

            --i;
            Vector row = _type == MatrixType.Full || _type == MatrixType.LowerTriangular ? 
                _rows[i] : new RowVector(this, i);

            var indexes = row.GetOrderIndexes(reverse);
            return ReorderCols(indexes);
        }

        internal virtual Matrix ReorderCols(int[] indexes)
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

        internal virtual Vector OrderRows(int j, bool reverse = false)
        {
            if (j > _colCount)
                throw Exceptions.IndexOutOfRange(j.ToString());

            --j;
            Vector col = _type == MatrixType.Column ? _rows[0] : new ColumnVector(this, j);
            return col.Order(reverse);
        }

        internal virtual Vector OrderCols(int i, bool reverse = false)
        {
            if (i > _rowCount)
                throw Exceptions.IndexOutOfRange(i.ToString());

            --i;
            Vector row = _type == MatrixType.Full || _type == MatrixType.LowerTriangular ? 
                _rows[i] : new RowVector(this, i);

            return row.Order(reverse);
        }

        internal virtual RealValue Min()
        {
            var len = _rows.Length;
            if (len == 0)
                return RealValue.Zero;

            var v = _rows[0].Min();
            var min = v.D;
            var u = v.Units;
            for (int i = 1; i < len; ++i)
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

        internal virtual RealValue Max()
        {
            var len = _rows.Length;
            if (len == 0)
                return RealValue.Zero;

            var v = _rows[0].Max();
            var max = v.D;
            var u = v.Units;
            for (int i = 1; i < len; ++i)
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
            var len = _rows.Length;
            if (len == 0)
                return RealValue.Zero;

            var v = _rows[0].Sum();
            var sum = v.D;
            var u = v.Units;
            for (int i = 1; i < len; ++i)
            {
                v = _rows[i].Sum();
                sum += v.D * Unit.Convert(u, v.Units, ',');
            }
            return new(sum, u);
        }

        internal virtual RealValue SumSq()
        {
            var len = _rows.Length;
            if (len == 0)
                return RealValue.Zero;

            var v = _rows[0].SumSq();
            var sumsq = v.D;
            var u = v.Units;
            for (int i = 1; i < len; ++i)
            {
                v = _rows[i].SumSq();
                sumsq += v.D * Unit.Convert(u, v.Units, ',');
            }
            return new(sumsq, u);
        }

        internal virtual RealValue Srss()
        {
            var sumsq = SumSq();
            var srss = Math.Sqrt(sumsq.D);
            var u = sumsq.Units;
            return new(srss, u?.Pow(0.5f));
        }

        internal virtual RealValue Average()
        {
            var sum = Sum();
            var n = _rowCount * _colCount;
            return new(sum.D / n, sum.Units);
        }

        internal virtual RealValue Product()
        {
            var len = _rows.Length;
            if (len == 0)
                return RealValue.Zero;

            var p = _rows[0].Product();
            var product = p.D;
            var u = p.Units;
            for (int i = 1; i < len; ++i)
            {
                p = _rows[i].Product();
                u = Unit.Multiply(u, p.Units, out var b);
                product *= p.D * b;
            }
            if (!IsStructurallyConsistentType)
                return new(0d, u);

            return new(product, u);
        }

        internal virtual RealValue Mean()
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

        internal virtual RealValue And()
        {
            if (!IsStructurallyConsistentType)
                return RealValue.Zero;

            for (int i = 0, len = _rows.Length; i < len; ++i)
                if (Math.Abs(_rows[i].And().D) < RealValue.LogicalZero)
                    return RealValue.Zero;

            return RealValue.One;
        }

        internal virtual RealValue Or()
        {
            for (int i = 0, len = _rows.Length; i < len; ++i)
                if (Math.Abs(_rows[i].Or().D) >= RealValue.LogicalZero)
                    return RealValue.One;

            return RealValue.Zero;
        }

        internal virtual RealValue Xor()
        {
            var b = Math.Abs(_rows[0].Xor().D) >= RealValue.LogicalZero;
            for (int i = 1, len = _rows.Length; i < len; ++i)
                b = b != Math.Abs(_rows[i].Xor().D) >= RealValue.LogicalZero;

            return b ? RealValue.One : RealValue.Zero;
        }

        internal virtual RealValue Gcd()
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

        internal virtual RealValue Lcm()
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

        internal virtual RealValue Take(in RealValue x, in RealValue y)
        {
            var dx = Math.Round(x.D, MidpointRounding.AwayFromZero);
            var dy = Math.Round(y.D, MidpointRounding.AwayFromZero);
            if (!double.IsNormal(dy) || dy < Calculator.DeltaMinus || dy > _rowCount * Calculator.DeltaPlus ||
                !double.IsNormal(dx) || dx < Calculator.DeltaMinus || dx > _colCount * Calculator.DeltaPlus)
                return RealValue.NaN;

            return this[(int)dy - 1, (int)dx - 1];
        }

        internal virtual RealValue Line(in RealValue x, in RealValue y)
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

        internal virtual RealValue Spline(in RealValue x, in RealValue y)
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

        internal virtual RealValue Trace()
        {
            if (_rowCount != _colCount)
                throw Exceptions.MatrixNotSquare();

            var trace = this[0, 0];
            for (int i = 1; i < _rowCount; ++i)
                trace += this[i, i];

            return trace;
        }

        //Frobenius norm
        internal RealValue FrobNorm() => Srss();

        //L1 norm   
        internal virtual RealValue L1Norm()
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

        internal virtual RealValue L2Norm()
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

        internal virtual Matrix LUDecomposition(out int[] indexes)
        {
            var LU = GetLU(out indexes, out double _, out double _) ?? 
                throw Exceptions.MatrixSingular();

            return LU;
        }

        protected virtual Matrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            if (_rowCount != _colCount)
                throw Exceptions.MatrixNotSquare();

            var LU = new Matrix(_rowCount, _rowCount);
            var vv = new RealValue[_rowCount];
            indexes = new int[_rowCount];
            //Column buffer for faster access
            var col_j = new RealValue[_rowCount];
            RealValue big;
            det = 1d; //Used to compute the sign change of determinant after pivot interchanges
            for (int i = 0; i < _rowCount; ++i)
            {
                indexes[i] = i;
                LU._rows[i] = _rows[i].Copy();
            }
            minPivot = 0d;
            for (int i = 0; i < _rowCount; ++i)
            {
                var row = LU._rows[i];
                var size = row.Size;
                if (size == 0)
                    return null;

                var raw = row.Raw;
                big = RealValue.Abs(raw[0]);
                for (int j = 1; j < size; j++)
                {
                    var temp = RealValue.Abs(raw[j]);
                    if (temp.CompareTo(big) > 0)
                        big = temp;
                }
                if (big.D == 0d)
                    return null;

                vv[i] = RealValue.One / big;
            }
            minPivot = double.MaxValue; //Used to determine singularity
            for (int j = 0; j < _colCount; ++j)
            {
                //Cache column j for faster access
                for (int i = 0; i < _rowCount; ++i)
                    col_j[i] = LU[i, j];

                for (int i = 0; i < j; ++i)
                {
                    var row = LU._rows[i];
                    var size = Math.Min(row.Size, i);
                    var raw = row.Raw;
                    var sum = col_j[i];
                    var k0 = 0;
                    while (k0 < size && raw[k0].D == 0d) ++k0;
                    for (int k = k0; k < size; ++k)
                        sum -= raw[k] * col_j[k];

                    col_j[i] = sum;
                }
                big = RealValue.Zero;
                var imax = j;
                for (int i = j; i < _rowCount; ++i)
                {
                    var row = LU._rows[i];
                    var size = Math.Min(row.Size, j);
                    var raw = row.Raw;
                    var sum = col_j[i];
                    var k0 = 0;
                    while (k0 < size && raw[k0].D == 0d) ++k0;
                    for (int k = k0; k < size; k++)
                        sum -= raw[k] * col_j[k];

                    col_j[i] = sum;
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
                    (col_j[j], col_j[imax]) = (col_j[imax], col_j[j]); //Swap also cached values
                    vv[imax] = vv[j];
                    (indexes[j], indexes[imax]) = (indexes[imax], indexes[j]);
                    det = -det;
                }
                if (j != _rowCount - 1)
                {
                    var dum = RealValue.One / col_j[j];
                    for (int i = j + 1; i < _rowCount; ++i)
                        col_j[i] *= dum;
                }
                else if (d < minPivot)
                    minPivot = d;

                //Restore column j from buffer
                for (int i = 0; i < _rowCount; ++i)
                    LU[i, j] = col_j[i];
            }
            return minPivot == 0d ? null : LU;
        }

        private void GetQR(out Matrix Q, out Matrix R)
        {
            var m = _rowCount;
            var n = _colCount;
            R = Copy();
            Q = Identity(m);
            var v = new RealValue[m];
            var mn = Math.Min(m, n);
            for (int k = 0; k < mn; k++)
            {
                // Build the Householder vector
                RealValue r = R[k, k];
                var normX = r * r;
                for (int i = k + 1; i < m; ++i)
                {
                    r = R[i, k];
                    normX += r * r;
                }
                normX = RealValue.Sqrt(normX);
                if (normX.D == 0d)
                    continue;

                r = R[k, k];
                var alpha = r.D > 0d ? -normX : normX;
                var rs = RealValue.Sqrt(alpha * (alpha - r) * 0.5) * 2d;
                v[k] = (r - alpha) / rs;
                for (int i = k + 1; i < m; ++i)
                    v[i] = R[i, k] / rs;

                // Apply reflector to R (from the left): R = (I - 2vvᵗ) * R
                for (int j = k; j < n; ++j)
                {
                    var dot = v[k] * R[k, j];
                    for (int i = k + 1; i < m; ++i)
                        dot += v[i] * R[i, j];

                    r = dot * 2d;
                    for (int i = k; i < m; ++i)
                        R[i, j] -= r * v[i];
                }

                // Apply reflector to Q (from the right): Q = Q * (I - 2vvᵗ)
                for (int i = 0; i < m; ++i)
                {
                    var Q_i = Q._rows[i];
                    var size = Q_i.Size;    
                    var dot = Q_i[k] * v[k];
                    for (int j = k + 1; j < size; ++j)
                        dot += Q_i[j] * v[j];

                    r = dot * 2d;
                    for (int j = m - 1; j >= k; --j)
                        Q_i[j] -= r * v[j];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static RealValue CopySign(in RealValue a, in RealValue b) =>
            b.D >= 0.0 ? RealValue.Abs(a) : -RealValue.Abs(a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static RealValue Sqr(in RealValue x) => x * x;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                throw Exceptions.MatrixNotHigh();

            var m = _rowCount - 1;
            var n = _colCount - 1;
            var rv1 = new Vector(_colCount);
            U = Copy();
            sig = new Vector(_colCount);
            V = new Matrix(_colCount, _colCount);
            SvdBidiagonalize(U, sig, rv1, m, n, out RealValue anorm);
            SvdRightTransform(U, V, rv1, n);
            SvdLeftTransform(U, sig, m, n);
            SvdDiagonalize(U, sig, V, rv1, m, n, anorm);
        }

        private static void SvdBidiagonalize(Matrix U, Vector sig, Vector rv1, int m, int n, out RealValue anorm)
        {
            RealValue sum;
            RealValue scale, g;
            scale = g = anorm = RealValue.Zero;
            var U_col_i = new RealValue[n + 1];
            for (int i = 0; i <= n; ++i)
            {
                var l = i + 1;
                rv1[i] = scale * g;
                if (i <= m)
                {
                    //Cache the i-th column of U for optimized access
                    for (int k = i; k <= m; ++k)
                        U_col_i[k] = U[k, i];

                    scale = RealValue.Abs(U_col_i[i]);
                    for (int k = l; k <= m; ++k)
                        scale += RealValue.Abs(U_col_i[k]);

                    if (scale.D != 0)
                    {

                        var U_ii = U_col_i[i] / scale;
                        sum = Sqr(U_ii);
                        U_col_i[i] = U_ii;
                        for (int k = l; k <= m; ++k)
                        {
                            U_col_i[k] /= scale;
                            sum += Sqr(U_col_i[k]);
                        }
                        var f = U_ii;
                        g = -CopySign(RealValue.Sqrt(sum), f);
                        var h = f * g - sum;
                        U_ii = f - g;
                        U_col_i[i] = U_ii;
                        for (int j = l; j <= n; ++j)
                        {
                            sum = U_ii * U[i, j];
                            for (int k = l; k <= m; ++k)
                                sum += U_col_i[k] * U[k, j];

                            f = sum / h;
                            for (int k = i; k <= m; ++k)
                                U[k, j] += f * U_col_i[k];
                        }
                        for (int k = i; k <= m; ++k)
                            U_col_i[k] *= scale;
                    }
                    //Restore the cached i-th column of U
                    for (int k = i; k <= m; ++k)
                        U[k, i] = U_col_i[k];
                }
                sig[i] = scale * g;
                if (i <= m && i != n)
                {
                    var U_i = U._rows[i];
                    scale = RealValue.Abs(U_i[l]);
                    for (int k = l + 1; k <= n; ++k)
                        scale += RealValue.Abs(U_i[k]);

                    if (scale.D != 0)
                    {
                        var U_il = U_i[l] / scale;
                        sum = Sqr(U_il);
                        U_i[l] = U_il;
                        for (int k = l + 1; k <= n; ++k)
                        {
                            U_i[k] /= scale;
                            sum += Sqr(U_i[k]);
                        }
                        var f = U_il;
                        g = -CopySign(RealValue.Sqrt(sum), f);
                        var h = RealValue.One / (f * g - sum);
                        U_il = f - g;
                        U_i[l] = U_il;
                        for (int k = l; k <= n; ++k)
                            rv1[k] = U_i[k] * h;

                        for (int j = l; j <= m; ++j)
                        {   
                            var U_j = U._rows[j];
                            sum = U_j[l] * U_il;
                            for (int k = l + 1; k <= n; ++k)
                                sum += U_j[k] * U_i[k];

                            for (int k = l; k <= n; ++k)
                                U_j[k] += sum * rv1[k];
                        }
                        for (int k = l; k <= n; ++k)
                            U_i[k] *= scale;
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
                var V_i = V._rows[i];
                if (i < n)
                {
                    if (g.D != 0)
                    {
                        g = RealValue.One / g;
                        var U_i = U._rows[i];
                        for (int j = l; j <= n; ++j)
                            V_i[j] = (U_i[j] / U_i[l]) * g;

                        var U_il = U_i[l];
                        for (int j = l; j <= n; ++j)
                        {
                            var V_j = V._rows[j];
                            sum = U_il * V_j[l];
                            for (int k = l + 1; k <= n; ++k)
                                sum += U_i[k] * V_j[k];

                            for (int k = l; k <= n; ++k)
                                V_j[k] += sum * V_i[k];
                        }
                    }
                    for (int j = l; j <= n; ++j)
                        V_i[j] = V[j, i] = RealValue.Zero;
                }
                V_i[i] = RealValue.One;
                g = rv1[i];
                l = i;
            }
        }

        private static void SvdLeftTransform(Matrix U, Vector sig, int m, int n)
        {
            RealValue[] U_col_i = new RealValue[m + 1];
            for (int i = Math.Min(m, n); i >= 0; --i)
            {
                var l = i + 1;
                var g = sig[i];
                var U_i = U._rows[i];
                var U_ii = U_i[i];
                for (int j = l; j <= n; ++j)
                    U_i[j] = RealValue.Zero;

                //Cache column of U[i] to optimize access
                for (int k = i; k <= m; ++k)
                    U_col_i[k] = U[k, i];

                if (g.D != 0)
                {
                    g = RealValue.One / g;
                    if (l <= n)
                    {
                        var U_li = U_col_i[l];
                        for (int j = l; j <= n; ++j)
                        {
                            var sum = U_li * U[l, j];
                            for (int k = l + 1; k <= m; ++k)
                                sum += U_col_i[k] * U[k, j];

                            var f = (sum / U_ii) * g;
                            for (int k = i; k <= m; ++k)
                                U[k, j] += f * U_col_i[k];
                        }
                    }
                    for (int j = i; j <= m; ++j)
                        U_col_i[j] *= g;
                }
                else
                    for (int j = i; j <= m; ++j)
                        U_col_i[j] = RealValue.Zero;

                U_col_i[i] += RealValue.One;
                //Restore cached column of U
                for (int k = i; k <= m; ++k)
                    U[k, i] = U_col_i[k];
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
                                var U_j = U._rows[j];
                                var Uy = U_j[nm];
                                var Uz = U_j[i];
                                U_j[nm] = Uy * c + Uz * s;
                                U_j[i] = Uz * c - Uy * s;
                            }
                        }
                    }
                    var z = sig[k];
                    if (l == k)
                    {
                        if (z.D < 0d)
                        {
                            sig[k] = -z;
                            var V_k = V._rows[k];   
                            for (int j = 0; j <= n; ++j)
                                V_k[j] = -V_k[j];
                        }
                        break;
                    }
                    if (iter == 30)
                        throw new MathParserException("no convergence in 30 svdcmp iterations");

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
                        var V_j = V._rows[j];
                        var V_i = V._rows[i];
                        for (int jj = 0; jj <= n; ++jj)
                        {
                            x = V_j[jj];
                            z = V_i[jj];
                            V_j[jj] = x * c + z * s;
                            V_i[jj] = z * c - x * s;
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
                            var U_jj = U._rows[jj];
                            y = U_jj[j];
                            z = U_jj[i];
                            U_jj[j] = y * c + z * s;
                            U_jj[i] = z * c - y * s;
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

            if (!isSorted)
            {
                sig = sig.Sort(true);
                U = U.ReorderCols(indexes);
                V = V.ReorderRows(indexes);
            }
        }

        internal virtual Matrix QRDecomposition()
        {
            GetQR(out Matrix Q, out Matrix R);
            return Augment(Q, R);
        }

        internal virtual Matrix SVDDecomposition()
        {
            GetSVD(out Matrix U, out Vector sig, out Matrix V);
            SvdSortDescending(ref U, ref sig, ref V);
            return Augment(U, sig, V);
        }

        private static Matrix Identity(int n)
        {
            var M = new Matrix(n, n);
            for (int i = 0; i < n; ++i)
                M[i, i] = RealValue.One;

            return M;
        }

        private static void FwdAndBackSubst(Matrix LU, int[] indexes, Vector v, RealValue[] x)
        {
            var m = LU._rowCount;
            var start = -1;
            //Forward substitution. Solve Ly = v by storing y in x. 
            for (int i = 0; i < m; ++i)
            {
                var index = indexes[i];
                var sum = v[index];
                if (start >= 0)
                {
                    var row = LU._rows[i].Raw;
                    var size = Math.Min(LU._rows[i].Size, i);
                    for (int j = start; j < size; ++j)
                        sum -= row[j] * x[j];
                }
                else if (!sum.Equals(RealValue.Zero))
                    start = i;

                x[i] = sum;
            }
            //Back substitution. Solve Ux = y.
            for (int i = m - 1; i >= 0; --i)
            {
                var sum = x[i];
                var row = LU._rows[i].Raw;
                var n = Math.Min(LU._rows[i].Size, m);
                for (int j = i + 1; j < n; ++j)
                    sum -= row[j] * x[j];

                x[i] = sum / LU[i, i];
            }
        }

        internal virtual Vector LSolve(Vector v)
        {
            var LU = GetLU(out int[] indexes, out double minPivot, out double _) ?? 
                throw Exceptions.MatrixSingular();

            if (minPivot < 1e-15)
                throw Exceptions.MatrixCloseToSingular();

            var x = new RealValue[_rowCount];
            FwdAndBackSubst(LU, indexes, v, x);
            return new(x);
        }

        internal virtual Matrix MSolve(Matrix M)
        {
            var LU = GetLU(out int[] indexes, out double minPivot, out double _) ?? 
                throw Exceptions.MatrixSingular();

            if (minPivot < 1e-15)
                throw Exceptions.MatrixCloseToSingular();

            return MSolveForLU(LU, indexes, M);
        }

        private static Matrix MSolveForLU(Matrix LU, int[] indexes, Matrix M)
        {
            var m = LU._rowCount;
            var n = M._colCount;
            var result = new Vector[n];
            Parallel.For(0, n, j =>
            {
                var x = new RealValue[m];
                FwdAndBackSubst(LU, indexes, new ColumnVector(M, j), x);
                result[j] = new(x);
            });
            return CreateFromCols(result, m);
        }

        private static Matrix MSolveForLU(Matrix LU, int[] indexes)
        {
            var m = LU._rowCount;
            var I = Identity(m);
            var result = new Vector[m];
            Parallel.For(0, m, j =>
            {
                var x = new RealValue[m];
                FwdAndBackSubst(LU, indexes, I._rows[j], x);
                result[j] = new(x);
            });
            for (int j = m - 1; j >= 0; --j)
                I.SetCol(j, result[j]);

            return I;
        }

        internal virtual Matrix Invert()
        {
            var LU = GetLU(out int[] indexes, out double minPivot, out double _) ?? 
                throw Exceptions.MatrixSingular();

            if (minPivot < 1e-15)
                throw Exceptions.MatrixCloseToSingular();

            var M = GetInverse(LU, indexes);
            return M;
        }

        private Matrix GetInverse(Matrix lu, int[] indexes)
        {
            if (_rowCount > ParallelThreshold)
                return MSolveForLU(lu, indexes);
            
            var vector = new Vector(_rowCount);
            var x = new RealValue[_rowCount];
            var M = new Matrix(_rowCount, _rowCount);
            for (int j = 0; j < _rowCount; ++j)  //Find inverse by columns.
            {
                vector[j] = RealValue.One;
                FwdAndBackSubst(lu, indexes, vector, x);
                for (int i = 0; i < _rowCount; ++i)
                    M[i, j] = x[i];

                vector[j] = RealValue.Zero;
            }
            return M;
        }

        internal virtual RealValue Determinant()
        {
            if (_rowCount != _colCount)
                throw Exceptions.MatrixNotSquare();

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

        internal virtual RealValue Cond2()
        {
            GetSVD(out Matrix _, out Vector sig, out Matrix _);
            return sig.Max() / sig.Min();
        }

        internal virtual RealValue Cond1() => Condition(M => M.L1Norm());
        internal virtual RealValue CondFrob() => Condition(M => M.FrobNorm());
        internal virtual RealValue CondInf() => Condition(M => M.InfNorm());

        protected virtual RealValue Condition(Func<Matrix, RealValue> norm)
        {
            if (_rowCount != _colCount)
                throw Exceptions.MatrixNotSquare();

            var LU = GetLU(out int[] indexes, out double _, out double _); //Decompose the matrix by LU decomp.
            if (LU is null)
                return RealValue.PositiveInfinity;

            var M = GetInverse(LU, indexes);
            return norm(this) * norm(M);
        }

        internal virtual RealValue Rank()
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
            var LU = GetLU(out int[] indexes, out double minPivot, out double det);

            if (Math.Abs(minPivot) < 1e-15 && _rowCount < 11)
                return AdjointByMinors();

            if (LU is null)
                throw Exceptions.MatrixSingular();

            var determinant = GetDeterminant(LU) * det;
            var M = GetInverse(LU, indexes);
            return M * determinant;
        }

        private Matrix AdjointByMinors()
        {
            int n = _rowCount;
            var result = new Matrix(n, n);
            int[] full = new int[n];
            for (int i = 0; i < n; ++i)
            {
                full[i] = i;
                result._rows[i] = new LargeVector(n, n);
            }
            Parallel.For(0, n, i =>
            {
                Span<int> subRows = stackalloc int[n - 1];
                Span<int> subCols = stackalloc int[n - 1];
                RemoveAt(full, i, subRows);
                for (int j = 0; j < n; ++j)
                {
                    RemoveAt(full, j, subCols);
                    double sign = ((i + j) & 1) == 0 ? 1.0 : -1.0;
                    var det = Determinant(subRows, subCols);
                    result[j, i] = det * sign; // note the transpose
                }
            });
            return result;
        }

        private RealValue Determinant(Span<int> rows, Span<int> cols)
        {
            var n = rows.Length;
            if (n == 1)
                return this[rows[0], cols[0]];

            if (n == 2)
                return Determinant2x2(rows, cols);

            if (n == 3)
                return Determinant3x3(rows, cols);

            var det = RealValue.Zero;
            var subRows = rows[1..]; // exclude row 0
            Span<int> subCols = stackalloc int[n - 1];
            for (int j = 0; j < n; ++j)
            {
                var a = this[rows[0], cols[j]];
                if (a.D == 0d) continue;
                RemoveAt(cols, j, subCols);
                var sign = (j % 2 == 0) ? 1.0 : -1.0;
                det += a * Determinant(subRows, subCols) * sign;
            }
            return det;
        }

        private RealValue Determinant2x2(ReadOnlySpan<int> rows, ReadOnlySpan<int> cols)
        {
            var r_0 = rows[0];
            var r_1 = rows[1];
            var a = this[r_0, cols[0]];
            var b = this[r_0, cols[1]];
            var c = this[r_1, cols[0]];
            var d = this[r_1, cols[1]];
            return a * d - b * c;
        }

        private RealValue Determinant3x3(ReadOnlySpan<int> rows, ReadOnlySpan<int> cols)
        {
            var r_0 = rows[0];
            var r_1 = rows[1];
            var r_2 = rows[2];
            var a = this[r_0, cols[0]];
            var b = this[r_0, cols[1]];
            var c = this[r_0, cols[2]];
            var d = this[r_1, cols[0]];
            var e = this[r_1, cols[1]];
            var f = this[r_1, cols[2]];
            var g = this[r_2, cols[0]];
            var h = this[r_2, cols[1]];
            var i = this[r_2, cols[2]];
            return a * (e * i - f * h)
                 - b * (d * i - f * g)
                 + c * (d * h - e * g);
        }

        protected static void RemoveAt(ReadOnlySpan<int> source, int index, Span<int> target)
        {
            int n = source.Length;

            if (index > 0)
                source[..index].CopyTo(target);

            if (index < n - 1)
                source[(index + 1)..].CopyTo(target[index..]);
        }

        internal virtual Matrix Cofactor() => Adjoint().Transpose();

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

        internal virtual Vector Diagonal()
        {
            var n = Math.Min(_rowCount, _colCount);
            Vector v = new(n);
            var values = v.Raw;
            for (int i = 0; i < n; ++i)
                values[i] = this[i, i];

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

        protected bool CheckCount(ref int count)
        {
            if (count < 0)
            {
                count = -count;
                return true; //Reverse order
            }
            if (count == 0 || count > _rowCount)
                count = _rowCount;
            return false; //Normal order
        }
    }
}