using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace Calcpad.Core
{
    internal class HpMatrix : Matrix, IEquatable<HpMatrix>
    {
        protected HpVector[] _hpRows;
        internal HpVector[] HpRows => _hpRows;
        internal virtual Unit Units
        {
            get => _units;
            set
            {
                _units = value;
                for (int i = _hpRows.Length - 1; i >= 0; --i)
                    _hpRows[i].Units = value;
            }
        }

        protected Unit _units;
        protected HpMatrix() { }

        protected HpMatrix(int length, Unit units)
        {
            if (length > MaxSize)
                throw Exceptions.MatirixSizeLimit();

            _rowCount = length;
            _colCount = length;
            _units = units;
        }

        internal HpMatrix(int rows, int cols, Unit units)
        {
            if (rows > MaxSize || cols > MaxSize)
                throw Exceptions.MatirixSizeLimit();

            _type = MatrixType.Full;
            _rowCount = rows;
            _colCount = cols;
            _hpRows = new HpVector[rows];
            _units = units;
            for (int i = rows - 1; i >= 0; --i)
                _hpRows[i] = new HpVector(cols, units);

            _rows = _hpRows;
        }

        internal HpMatrix(Vector v)
        {
            _type = MatrixType.Full;
            _rowCount = 1;
            _colCount = v.Length;
            var hp_v = v is HpVector hp ? hp : new HpVector(v, null);
            _hpRows = [hp_v];
            _rows = _hpRows;
            _units = hp_v.Units;
        }

        internal HpMatrix(Matrix matrix)
        {
            _type = MatrixType.Full;
            _rowCount = matrix.RowCount;
            _colCount = matrix.ColCount;
            _hpRows = new HpVector[_rowCount];
            _rows = _hpRows;
            if (_rowCount == 0 || _colCount == 0)
                return;

            _units = matrix[0, 0].Units;
            for (int i = _rowCount - 1; i >= 0; --i)
            {
                _hpRows[i] = new HpVector(matrix.Rows[i], _units);
                var units = _hpRows[i].Units;
                if (_units is null && units is not null)
                    throw Exceptions.InconsistentUnits(Unit.GetText(null), units.Text);
            }
        }

        internal HpMatrix(double[][] M, Unit units)
        {
            _type = MatrixType.Full;
            _rowCount = M.Length;
            _colCount = M[0].Length;
            _units = units;
            _hpRows = new HpVector[_rowCount];
            if (_rowCount > ParallelThreshold)
                Parallel.For(0, _rowCount, i => _hpRows[i] = new HpVector(M[i], units));
            else
                for (int i = 0; i < _rowCount; ++i) _hpRows[i] = new HpVector(M[i], units);

            _rows = _hpRows;
        }

        internal override RealValue this[int row, int col]
        {
            get => _hpRows[row][col];
            set => _hpRows[row][col] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual double GetValue(int row, int col) => _hpRows[row].GetValue(col);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal virtual void SetValue(double value, int row, int col) =>
            _hpRows[row].SetValue(value, col);

        internal override HpMatrix Clone() => new(_rowCount, _colCount, _units);

        internal virtual Matrix CloneAsMatrix() => new(_rowCount, _colCount);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj is null)
                return false;

            return obj is HpMatrix M && Equals(M);
        }

        public bool Equals(HpMatrix M)
        {
            if (_type != M._type ||
                _rowCount != M._rowCount ||
                _colCount != M._colCount)
                return false;

            var m = _hpRows.Length;
            if (m > ParallelThreshold)
            {
                var result = true;
                Parallel.For(0, m, (i, state) =>
                {
                    if (!_hpRows[i].Equals(M._hpRows[i]))
                    {
                        result = false;
                        state.Break();
                    }
                });
                return result;
            }
            for (int i = m - 1; i >= 0; --i)
                if (!_hpRows[i].Equals(M._hpRows[i]))
                    return false;

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(_type);
            hashCode.Add(_rowCount);
            hashCode.Add(_colCount);
            var m = _hpRows.Length;
            for (int i = m - 1; i >= 0; --i)
                hashCode.Add(_hpRows[i].GetHashCode);

            return hashCode.ToHashCode();
        }

        private static void CheckBounds(HpMatrix a, HpMatrix b)
        {
            if (a._rowCount != b._rowCount ||
                a._colCount != b._colCount)
                throw Exceptions.MatrixDimensions();
        }

        protected override HpMatrix Copy()
        {
            var M = new HpMatrix(_rowCount, _colCount, _units);
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
            {
                if (_rowCount > ParallelThreshold)
                    Parallel.For(0, _rowCount, i => M._hpRows[i] = _hpRows[i].Copy());
                else
                    for (int i = 0; i < _rowCount; ++i) M._hpRows[i] = _hpRows[i].Copy();
            }
            else
            {
                if (_rowCount > ParallelThreshold)
                    Parallel.For(0, _rowCount, CopyRow);
                else
                    for (int i = _rowCount - 1; i >= 0; --i) CopyRow(i);
            }
            return M;

            void CopyRow(int i)
            {
                var row = M._hpRows[i];
                for (int j = _colCount - 1; j >= 0; --j)
                    row.SetValue(GetValue(i, j), j);
            }
        }

        protected double[][] RawCopy()
        {
            var M = new double[_rowCount][];
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
            {
                if (_rowCount > ParallelThreshold)
                    Parallel.For(0, _rowCount, i => M[i] = _hpRows[i].RawCopy());
                else
                    for (int i = 0; i < _rowCount; ++i) M[i] = _hpRows[i].RawCopy();
            }
            else
            {
                if (_rowCount > ParallelThreshold)
                    Parallel.For(0, _rowCount, CopyRow);
                else
                    for (int i = _rowCount - 1; i >= 0; --i) CopyRow(i);
            }
            return M;

            void CopyRow(int i)
            {
                M[i] = new double[_colCount];
                var row = M[i];
                for (int j = _colCount - 1; j >= 0; --j)
                    row[j] = GetValue(i, j);
            }
        }


        internal override HpMatrix Resize(int m, int n)
        {
            if (m > MaxSize || n > MaxSize)
                throw Exceptions.MatrixDimensions();

            var m1 = Math.Min(m, _rowCount) - 1;
            if (_type == MatrixType.Full)
            {
                if (m != _rowCount)
                {
                    Array.Resize(ref _hpRows, m);
                    for (int i = _rowCount; i < m; ++i)
                        _hpRows[i] = new HpVector(n, _units);

                    _rowCount = m;
                }
                if (n != _colCount)
                {
                    for (int i = 0; i <= m1; ++i)
                        _hpRows[i].Resize(n);

                    _colCount = n;
                }
                return this;
            }
            if (n == 1 && this is HpColumnMatrix cm)
                return cm.Resize(m);

            if (m == n)
            {
                if (this is HpDiagonalMatrix dm)
                    return dm.Resize(n);
                if (this is HpSymmetricMatrix sm)
                    return sm.Resize(n);
                if (this is HpUpperTriangularMatrix um)
                    return um.Resize(n);
                if (this is HpLowerTriangularMatrix lm)
                    return lm.Resize(n);
            }
            if (m == _rowCount && n == _colCount)
                return this;

            var M = new HpMatrix(m, n, _units);
            var n1 = Math.Min(n, _colCount) - 1;
            for (int i = m1; i >= 0; --i)
                for (int j = n1; j >= 0; --j)
                    M.SetValue(GetValue(i, j), i, j);

            return M;
        }

        public static HpMatrix operator -(HpMatrix a)
        {
            var c = a.Clone();
            var m = a._hpRows.Length;
            if (m > ParallelThreshold)
                Parallel.For(0, m, i =>
                    c._hpRows[i] = -a._hpRows[i]);
            else
                for (int i = m - 1; i >= 0; --i)
                    c._hpRows[i] = -a._hpRows[i];

            return c;
        }


        public static HpMatrix operator -(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] - b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] - b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n, a.Units);
                var d = Unit.Convert(a.Units, b.Units, '-');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) - b.GetValue(i, j) * d, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) - b.GetValue(i, j) * d, j);
                    }
            }
            return c;
        }

        public static HpMatrix operator -(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] - b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] - b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n, a.Units);
                var db = b.D * Unit.Convert(a.Units, b.Units, '-');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) - db, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) - db, j);
                    }
            }
            return c;
        }

        public static HpMatrix operator -(RealValue a, HpMatrix b)
        {
            HpMatrix c;
            if (b.IsStructurallyConsistentType || a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                c._units = a.Units;
                var m = b._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    c._hpRows[i] = a - b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a - b._hpRows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new HpMatrix(m, n, a.Units);
                var da = a.D;
                var k = Unit.Convert(a.Units, b.Units, '-');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da - b.GetValue(i, j) * k, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da - b.GetValue(i, j) * k, j);
                    }
            }
            return c;
        }

        public static HpMatrix operator +(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] + b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] + b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n, a.Units);
                var d = Unit.Convert(a.Units, b.Units, '+');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) + b.GetValue(i, j) * d, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) + b.GetValue(i, j) * d, j);
                    }
            }
            return c;
        }

        public static HpMatrix operator +(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] + b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] + b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n, a.Units);
                var db = b.D * Unit.Convert(a.Units, b.Units, '-');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) + db, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) + db, j);
                    }
            }
            return c;
        }

        public static HpMatrix operator *(HpMatrix a, HpMatrix b)
        {
            if (a._colCount != b._rowCount)
                throw Exceptions.MatrixDimensions();

            if (b is HpColumnMatrix bc)
            {
                if (a is HpSymmetricMatrix hp_sm)
                    return hp_sm * bc;
                if (a is HpUpperTriangularMatrix hp_utm)
                    return hp_utm * bc;

                return a * bc;
            }

            if (a is HpDiagonalMatrix ad)
            {
                if (b is HpDiagonalMatrix bd)
                    return ad * bd;

                return ad * b;
            }
            else if (b is HpDiagonalMatrix bd)
                return a * bd;

            Unit unit = Unit.Multiply(a.Units, b.Units, out var d, true);
            HpMatrix c = new(a._rowCount, b._colCount, unit);
            var m = a._rowCount;
            var na = a._colCount - 1;
            var nb = b._colCount - 1;
            var c_rows = c._hpRows;
            if (a._type == MatrixType.Full || a._type == MatrixType.LowerTriangular)
            {
                var nb1 = nb + 1;
                var a_rows = a._hpRows;
                var b_rows = new Memory<double>[b.RowCount];
                for (int i = 0; i <= na; ++i)
                    b_rows[i] = b._hpRows[i].Raw;

                for (int i = 0; i < m; ++i)
                    c_rows[i] = new HpVector(nb1, nb1, unit);

                if (m > ParallelThreshold)
                {
                    var paralelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
                    Parallel.For(0, m, paralelOptions, MultiplyRow);
                }
                else
                    for (int i = 0; i < m; ++i)
                        MultiplyRow(i);

                if (d != 1d)
                    c.Scale(d);

                void MultiplyRow(int i)
                {
                    var ar = a_rows[i].Raw;
                    var size = a_rows[i].Size;
                    var sc = c_rows[i].Raw.AsSpan();
                    var vr = Vectorized.AsVector(sc);
                    for (int k = 0; k < size; ++k)
                        Vectorized.MultiplyAdd(b_rows[k].Span, ar[k], sc, vr);
                }
            }
            else
            {
                if (m > ParallelThreshold)
                    Parallel.For(0, m, MultiplyRow2);
                else
                    for (int i = m - 1; i >= 0; --i)
                        MultiplyRow2(i);

                void MultiplyRow2(int i)
                {
                    var c_i = c_rows[i];
                    for (int j = nb; j >= 0; --j)
                    {
                        var c_ij = a.GetValue(i, na) * b.GetValue(na, j) * d;
                        for (int k = na - 1; k >= 0; --k)
                            c_ij += a.GetValue(i, k) * b.GetValue(k, j) * d;

                        c_i.SetValue(c_ij, j);
                    }
                }
            }
            return c;
        }

        public static HpMatrix operator *(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            var unit = Unit.Multiply(a.Units, b.Units, out var db, true);
            db *= b.D;
            c = a.Clone();
            c._units = unit;
            var m = a._hpRows.Length;
            if (db != 0d)
            {
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] * db);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] * db;
            }
            return c;
        }

        public static HpMatrix operator /(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            Unit unit = Unit.Divide(a.Units, b.Units, out var d, true);
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                c._units = unit;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] / b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] / b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n, unit);
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) / b.GetValue(i, j) * d, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) / b.GetValue(i, j) * d, j);
                    }
            }
            return c;
        }

        public static HpMatrix operator /(HpMatrix a, RealValue b)
        {
            Unit units = Unit.Divide(a.Units, b.Units, out var db, true);
            HpMatrix c;
            if (a.IsStructurallyConsistentType && !b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = units;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] / b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] / b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n, units);
                db /= b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) * db, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) * db, j);
                    }
            }
            return c;
        }

        public static HpMatrix operator /(RealValue a, HpMatrix b)
        {
            Unit units = Unit.Divide(a.Units, b.Units, out var da, true);
            HpMatrix c;
            if (b.IsStructurallyConsistentType)
            {
                c = b.Clone();
                c._units = units;
                var m = b._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a / b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a / b._hpRows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new(m, n, units);
                da *= a.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da / b.GetValue(i, j), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da / b.GetValue(i, j), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator %(HpMatrix a, HpMatrix b)
        {
            if (b.Units is not null)
                throw Exceptions.CannotEvaluateRemainder(Unit.GetText(a.Units), Unit.GetText(b.Units));

            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] % b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] % b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n, a.Units);
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) % b.GetValue(i, j), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) % b.GetValue(i, j), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator %(HpMatrix a, RealValue b)
        {
            if (b.Equals(RealValue.Zero))
                return new HpMatrix(a._rowCount, a._colCount, null).Fill(RealValue.NaN);

            if (b.Units is not null)
                throw Exceptions.CannotEvaluateRemainder(Unit.GetText(a.Units), Unit.GetText(b.Units));

            HpMatrix c;
            if (a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] % b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] % b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n, a.Units);
                var db = b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) % db, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) % db, j);
                    }
            }
            return c;
        }

        public static HpMatrix operator %(RealValue a, HpMatrix b)
        {
            if (b.Units is not null)
                throw Exceptions.CannotEvaluateRemainder(Unit.GetText(a.Units), Unit.GetText(b.Units));

            HpMatrix c;
            if (b.IsStructurallyConsistentType)
            {
                c = b.Clone();
                var m = b._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a % b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a % b._hpRows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new(m, n, a.Units);
                var da = a.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da % b.GetValue(i, j), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da % b.GetValue(i, j), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator ==(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] == b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] == b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var d = Unit.Convert(a.Units, b.Units, '≡');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsEqual(b.GetValue(i, j) * d), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsEqual(b.GetValue(i, j) * d), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator ==(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] == b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] == b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var db = Unit.Convert(a.Units, b.Units, '≡') * b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsEqual(db), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsEqual(db), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator !=(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] != b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] != b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var d = Unit.Convert(a.Units, b.Units, '≠');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsNotEqual(b.GetValue(i, j) * d), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsNotEqual(b.GetValue(i, j) * d), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator !=(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] != b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] != b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var db = Unit.Convert(a.Units, b.Units, '≠') * b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsNotEqual(db), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsNotEqual(db), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator <(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] < b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] < b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var d = Unit.Convert(a.Units, b.Units, '<');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsLessThan(b.GetValue(i, j) * d), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsLessThan(b.GetValue(i, j) * d), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator <(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] < b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] < b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var db = Unit.Convert(a.Units, b.Units, '<') * b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsLessThan(db), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsLessThan(db), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator <(RealValue a, HpMatrix b)
        {
            HpMatrix c;
            if (b.IsStructurallyConsistentType || a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                c._units = null;
                var m = b._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a < b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a < b._hpRows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new HpMatrix(m, n, null);
                var da = a.D;
                var k = Unit.Convert(a.Units, b.Units, '<');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da.IsLessThan(b.GetValue(i, j) * k), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da.IsLessThan(b.GetValue(i, j) * k), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator >(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] > b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] > b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var d = Unit.Convert(a.Units, b.Units, '>');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsGreaterThan(b.GetValue(i, j) * d), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsGreaterThan(b.GetValue(i, j) * d), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator >(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] > b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] > b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var db = Unit.Convert(a.Units, b.Units, '>') * b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsGreaterThan(db), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsGreaterThan(db), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator >(RealValue a, HpMatrix b)
        {
            HpMatrix c;
            if (b.IsStructurallyConsistentType || a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                c._units = null;
                var m = b._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a > b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a > b._hpRows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new HpMatrix(m, n, null);
                var da = a.D;
                var k = Unit.Convert(a.Units, b.Units, '>');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da.IsGreaterThan(b.GetValue(i, j) * k), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da.IsGreaterThan(b.GetValue(i, j) * k), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator <=(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] <= b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] <= b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var d = Unit.Convert(a.Units, b.Units, '<');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsLessThanOrEqual(b.GetValue(i, j) * d), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsLessThanOrEqual(b.GetValue(i, j) * d), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator <=(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] <= b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] <= b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var db = Unit.Convert(a.Units, b.Units, '<') * b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsLessThanOrEqual(db), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsLessThanOrEqual(db), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator <=(RealValue a, HpMatrix b)
        {
            HpMatrix c;
            if (b.IsStructurallyConsistentType || a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                c._units = null;
                var m = b._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a <= b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a <= b._hpRows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new HpMatrix(m, n, null);
                var da = a.D;
                var k = Unit.Convert(a.Units, b.Units, '<');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da.IsLessThanOrEqual(b.GetValue(i, j) * k), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da.IsLessThanOrEqual(b.GetValue(i, j) * k), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator >=(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type && a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] >= b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] >= b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var d = Unit.Convert(a.Units, b.Units, '>');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsGreaterThanOrEqual(b.GetValue(i, j) * d), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsGreaterThanOrEqual(b.GetValue(i, j) * d), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator >=(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] >= b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] >= b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var db = Unit.Convert(a.Units, b.Units, '>') * b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsGreaterThanOrEqual(db), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j).IsGreaterThanOrEqual(db), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator >=(RealValue a, HpMatrix b)
        {
            HpMatrix c;
            if (b.IsStructurallyConsistentType || a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                c._units = null;
                var m = b._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a >= b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a >= b._hpRows[i];
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new HpMatrix(m, n, null);
                var da = a.D;
                var k = Unit.Convert(a.Units, b.Units, '>');
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da.IsGreaterThanOrEqual(b.GetValue(i, j) * k), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(da.IsGreaterThanOrEqual(b.GetValue(i, j) * k), j);
                    }
            }
            return c;
        }

        private const double logicalZero = RealValue.LogicalZero;
        private static double And(double a, double b) => Math.Abs(a) < logicalZero || Math.Abs(b) < logicalZero ? 0d : 1d;
        private static double Or(double a, double b) => Math.Abs(a) >= logicalZero || Math.Abs(b) >= logicalZero ? 1d : 0d;
        private static double Xor(double a, double b) => (Math.Abs(a) >= logicalZero) != (Math.Abs(b) >= logicalZero) ? 1d : 0d;

        public static HpMatrix operator &(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] & b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] & b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(And(a.GetValue(i, j), b.GetValue(i, j)), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(And(a.GetValue(i, j), b.GetValue(i, j)), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator &(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] & b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] & b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var db = b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(And(a.GetValue(i, j), db), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(And(a.GetValue(i, j), db), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator |(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] | b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] | b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(Or(a.GetValue(i, j), b.GetValue(i, j)), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(Or(a.GetValue(i, j), b.GetValue(i, j)), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator |(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] | b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] | b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var db = b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(Or(a.GetValue(i, j), db), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(Or(a.GetValue(i, j), db), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator ^(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            HpMatrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] ^ b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] ^ b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(Xor(a.GetValue(i, j), b.GetValue(i, j)), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(Xor(a.GetValue(i, j), b.GetValue(i, j)), j);
                    }
            }
            return c;
        }

        public static HpMatrix operator ^(HpMatrix a, RealValue b)
        {
            HpMatrix c;
            if (a.IsStructurallyConsistentType || b.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = null;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] ^ b);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] ^ b;
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, null);
                var db = b.D;
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(Xor(a.GetValue(i, j), db), j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(Xor(a.GetValue(i, j), db), j);
                    }
            }
            return c;
        }

        public static implicit operator HpMatrix(HpVector vector) => new HpColumnMatrix(vector);

        internal static HpMatrix EvaluateOperator(Calculator.Operator<RealValue> op, HpMatrix a, HpMatrix b, long index) =>
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

        internal static HpMatrix EvaluateOperator(Calculator.Operator<RealValue> op, in HpMatrix a, RealValue b, long index) =>
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

        internal static HpMatrix EvaluateOperator(Calculator.Operator<RealValue> op, in RealValue a, HpMatrix b, long index) =>
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


        private static HpMatrix EvaluateOperator2(Calculator.Operator<RealValue> op, HpMatrix a, HpMatrix b, long index)
        {
            CheckBounds(a, b);
            Unit units = op(a[0, 0], b[0, 0]).Units;
            HpMatrix c;
            if (a._type == b._type && (a.IsStructurallyConsistentType || Calculator.IsZeroPreservingOperator(index)))
            {
                c = a.Clone();
                c._units = units;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = HpVector.EvaluateOperator(op, a._hpRows[i], b._hpRows[i], index));
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = HpVector.EvaluateOperator(op, a._hpRows[i], b._hpRows[i], index);
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, units);
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(op(a[i, j], b[i, j]).D, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(op(a[i, j], b[i, j]).D, j);
                    }
            }
            return c;
        }

        internal static HpMatrix EvaluateOperator2(Calculator.Operator<RealValue> op, HpMatrix a, RealValue b, long index)
        {
            Unit units = op(a[0, 0], b).Units;
            HpMatrix c;
            if (a.IsStructurallyConsistentType || Calculator.IsZeroPreservingOperator(index) && a.Equals(RealValue.Zero))
            {
                c = a.Clone();
                c._units = units;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = HpVector.EvaluateOperator(op, a._hpRows[i], b, index));
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = HpVector.EvaluateOperator(op, a._hpRows[i], b, index);
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new HpMatrix(m, n, units);
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(op(a[i, j], b).D, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(op(a[i, j], b).D, j);
                    }
            }
            return c;
        }

        internal static HpMatrix EvaluateOperator2(Calculator.Operator<RealValue> op, RealValue a, HpMatrix b, long index)
        {
            Unit units = op(a, b[0, 0]).Units;
            HpMatrix c;
            if (b.IsStructurallyConsistentType || Calculator.IsZeroPreservingOperator(index) && a.Equals(RealValue.Zero))
            {
                c = b.Clone();
                c._units = units;
                var m = b._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = HpVector.EvaluateOperator(op, a, b._hpRows[i], index));
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = HpVector.EvaluateOperator(op, a, b._hpRows[i], index);
            }
            else
            {
                int m = b._rowCount, n = b._colCount;
                c = new HpMatrix(m, n, units);
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(op(a, b[i, j]).D, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(op(a, b[i, j]).D, j);
                    }
            }
            return c;
        }

        internal static HpMatrix EvaluateFunction(Calculator.Function<RealValue> f, HpMatrix a)
        {
            Unit units = f(a[0, 0]).Units;
            HpMatrix c;
            if (a.IsStructurallyConsistentType)
            {
                c = a.Clone();
                c._units = units;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = HpVector.EvaluateFunction(f, a._hpRows[i]));
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = HpVector.EvaluateFunction(f, a._hpRows[i]);
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                c = new(m, n, units);
                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(f(a[i, j]).D, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(f(a[i, j]).D, j);
                    }
            }
            return c;
        }

        internal static HpMatrix Hadamard(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            Unit units = Unit.Multiply(a.Units, b.Units, out var d, true);
            HpMatrix c;
            if (a._type == b._type)
            {
                c = a.Clone();
                c._units = units;
                var m = a._hpRows.Length;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        c._hpRows[i] = a._hpRows[i] * b._hpRows[i]);
                else
                    for (int i = m - 1; i >= 0; --i)
                        c._hpRows[i] = a._hpRows[i] * b._hpRows[i];
            }
            else
            {
                int m = a._rowCount, n = a._colCount;
                if (a._type == MatrixType.Column && b._type == MatrixType.Column)
                    c = new HpColumnMatrix(m, units);
                else
                    c = new HpMatrix(m, n, units);

                --n;
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) * b.GetValue(i, j) * d, j);
                    });
                else
                    for (int i = m - 1; i >= 0; --i)
                    {
                        var cr = c._hpRows[i];
                        for (int j = n; j >= 0; --j)
                            cr.SetValue(a.GetValue(i, j) * b.GetValue(i, j) * d, j);
                    }
            }
            return c;
        }

        internal static HpMatrix Kronecker(HpMatrix a, HpMatrix b)
        {
            var m = a._rowCount;
            var n = a._colCount;
            var p = b._rowCount;
            var q = b._colCount;
            var n1 = n - 1;
            var p1 = p - 1;
            var q1 = q - 1;
            Unit units = Unit.Multiply(a.Units, b.Units, out var d, true);
            var c = new HpMatrix(m * p, n * q, units);
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
                            c.SetValue(a.GetValue(i, j) * b.GetValue(k, l) * d, ipk, jq + l);
                    }
                }
            }
            return c;
        }

        internal static RealValue Frobenius(HpMatrix a, HpMatrix b)
        {
            CheckBounds(a, b);
            var m = a._hpRows.Length;
            var sum = 0d;
            if (m > ParallelThreshold)
                Parallel.For(0, m, i =>
                    sum += HpVector.RawDotProduct(a._hpRows[i], b._hpRows[i]));
            else
                for (int i = m - 1; i >= 0; --i)
                    sum += HpVector.RawDotProduct(a._hpRows[i], b._hpRows[i]);

            var u = Unit.Multiply(a.Units, b.Units, out var _, true);
            return new(sum, u);
        }

        internal override HpVector Row(int index)
        {
            if (index < 1 || index > _rowCount)
                throw Exceptions.IndexOutOfRange(index.ToString());

            --index;
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
                return _hpRows[index].Copy();

            var row = new HpVector(_colCount, _units);
            for (int j = _colCount - 1; j >= 0; --j)
                row.SetValue(GetValue(index, j), j);

            return row;
        }

        internal HpVector RowRef(int i)
        {
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
                return _hpRows[i];

            var row = new HpVector(_colCount, _units);
            for (int j = _colCount - 1; j >= 0; --j)
                row.SetValue(GetValue(i, j), j);

            return row;
        }

        internal override HpVector Col(int index)
        {
            if (index < 1 || index > _colCount)
                throw Exceptions.IndexOutOfRange(index.ToString());

            --index;
            if (this is HpColumnMatrix cm)
                return cm._hpRows[index].Copy();

            var col = new HpVector(_rowCount, _units);
            for (int i = _rowCount - 1; i >= 0; --i)
                col.SetValue(GetValue(i, index), i);

            return col;
        }

        internal double[] RawCol(int index)
        {
            var col = new double[_rowCount];
            int i = 0;
            int unrollCount = _rowCount - _rowCount % 4;
            for (; i < unrollCount; i += 4)
            {
                col[i] = GetValue(i, index);
                col[i + 1] = GetValue(i + 1, index);
                col[i + 2] = GetValue(i + 2, index);
                col[i + 3] = GetValue(i + 3, index);
            }
            for (; i < _rowCount; ++i)
                col[i] = GetValue(i, index);

            return col;
        }


        internal void SetRow(int index, HpVector row) =>
            _hpRows[index] = row.Resize(_colCount);

        internal void SetCol(int index, HpVector col)
        {
            var n = Math.Min(col.Size, _rowCount) - 1;
            var values = col.Raw;
            for (int i = n; i >= 0; --i)
                SetValue(values[i], i, index);
        }

        internal override HpMatrix Submatrix(int i1, int i2, int j1, int j2)
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
            var M = new HpMatrix(rows + 1, cols + 1, _units);
            --i1;
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
                for (int i = rows; i >= 0; --i)
                    M._hpRows[i] = _hpRows[i1 + i].Slice(j1, j2);
            else
            {
                --j1;
                for (int i = rows; i >= 0; --i)
                {
                    var row = M._hpRows[i];
                    for (int j = cols; j >= 0; --j)
                        row.SetValue(GetValue(i1 + i, j1 + j), j);
                }
            }
            return M;
        }

        internal override HpMatrix ExtractRows(Vector vector)
        {
            var hpVector = vector is HpVector hp_v ? hp_v : new HpVector(vector, null);
            var rowCount = hpVector.Length;
            var M = new HpMatrix(rowCount, _colCount, _units);
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
                for (int i = rowCount - 1; i >= 0; --i)
                {
                    var rowIndex = (int)hpVector[i].D;
                    if (rowIndex < 1 || rowIndex > _rowCount)
                        throw Exceptions.IndexOutOfRange(rowIndex.ToString());

                    M._hpRows[i] = _hpRows[rowIndex - 1];
                }
            else
            {
                var n = _colCount - 1;
                for (int i = rowCount - 1; i >= 0; --i)
                {
                    var rowIndex = (int)hpVector[i].D;
                    if (rowIndex < 1 || rowIndex > _rowCount)
                        throw Exceptions.IndexOutOfRange(rowIndex.ToString());

                    var row = M._hpRows[i];
                    for (int j = n; j >= 0; --j)
                        row.SetValue(GetValue(rowIndex - 1, j), j);
                }
            }
            return M;
        }

        internal override HpMatrix ExtractCols(Vector vector)
        {
            var hpVector = vector is HpVector hp_v ? hp_v : new HpVector(vector, null);
            var colCount = hpVector.Length;
            var M = new HpMatrix(_rowCount, colCount, _units);
            var m = _rowCount - 1;
            for (int j = colCount - 1; j >= 0; --j)
            {
                var colIndex = (int)hpVector[j].D;
                if (colIndex < 1 || colIndex > _colCount)
                    throw Exceptions.IndexOutOfRange(colIndex.ToString());

                for (int i = m; i >= 0; --i)
                    M[i, j] = this[i, colIndex - 1];
            }
            return M;
        }

        internal override HpMatrix Fill(RealValue value)
        {
            var m = _hpRows.Length;
            if (m > ParallelThreshold)
                Parallel.For(0, m, i =>
                    _hpRows[i].Fill(value));
            else
                for (int i = m - 1; i >= 0; --i)
                    _hpRows[i].Fill(value);

            Units = value.Units;
            return this;
        }

        internal override HpMatrix FillRow(int i, in RealValue value)
        {
            if (i > _rowCount)
                throw Exceptions.IndexOutOfRange(i.ToString());

            --i;
            var d = Unit.Convert(Units, value.Units, ',');
            var v = new RealValue(value.D * d, _units);
            if (_type == MatrixType.Full ||
                _type == MatrixType.Symmetric ||
                _type == MatrixType.UpperTriangular ||
                _type == MatrixType.LowerTriangular)
                _hpRows[i].Fill(v);
            else
                _hpRows[0][i] = v;

            return this;
        }

        internal override HpMatrix FillCol(int j, in RealValue value)
        {
            if (j > _colCount)
                throw Exceptions.IndexOutOfRange(j.ToString());

            --j;
            var d = Unit.Convert(Units, value.Units, ',');
            var v = new RealValue(value.D * d, _units);
            if (_type == MatrixType.Full)
                for (int i = _rowCount - 1; i >= 0; --i)
                    SetValue(v.D, i, j);
            else if (_type == MatrixType.Column ||
                     _type == MatrixType.Symmetric)
                _hpRows[j].Fill(v);
            else if (_type == MatrixType.Diagonal)
                _hpRows[0][j] = v;
            else if (_type == MatrixType.LowerTriangular)
                for (int i = _rowCount - 1; i >= j; --i)
                    SetValue(v.D, i, j);
            else if (_type == MatrixType.UpperTriangular)
                for (int i = j; i >= 0; --i)
                    SetValue(v.D, i, j);

            return this;
        }

        internal static HpMatrix Augment(params HpMatrix[] matrices)
        {
            int len = matrices.Length, m = 0, n = 0;
            for (int k = 0; k < len; ++k)
            {
                var matrix = matrices[k];
                n += matrix._colCount;
                m = Math.Max(m, matrix._rowCount);
            }
            var M = new HpMatrix(m, n, matrices[0].Units);
            var c = 0;
            for (int k = 0; k < len; ++k)
            {
                var matrix = matrices[k];
                m = matrix._rowCount - 1;
                n = matrix._colCount - 1;
                var d = Unit.Convert(M.Units, matrix.Units, ',');
                for (int i = m; i >= 0; --i)
                {
                    var row = M._hpRows[i];
                    for (int j = n; j >= 0; --j)
                        row.SetValue(matrix.GetValue(i, j) * d, c + j);
                }
                c += n + 1;
            }
            return M;
        }

        internal static HpMatrix Stack(params HpMatrix[] matrices)
        {
            int len = matrices.Length, m = 0, n = 0;
            for (int k = 0; k < len; ++k)
            {
                var matrix = matrices[k];
                n = Math.Max(n, matrix._colCount);
                m += matrix._rowCount;
            }
            var M = new HpMatrix(m, n, matrices[0].Units);
            var r = 0;
            for (int k = 0; k < len; ++k)
            {
                var matrix = matrices[k];
                m = matrix._rowCount - 1;
                n = matrix._colCount - 1;
                var d = Unit.Convert(M.Units, matrix.Units, ',');
                for (int i = m; i >= 0; --i)
                {
                    var row = M._hpRows[r + i];
                    for (int j = n; j >= 0; --j)
                        row.SetValue(matrix.GetValue(i, j) * d, j);
                }
                r += m + 1;
            }
            return M;
        }

        internal override HpVector Search(in RealValue value, int iStart, int jStart)
        {
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
                for (int i = iStart - 1; i < _rowCount; ++i)
                {
                    var jValue = _hpRows[i].Search(value, jStart).D;
                    if (jValue != 0)
                        return new([i + 1, jValue], null);

                    jStart = 1;
                }
            else
            {
                if (Unit.IsConsistent(Units, value.Units))
                {
                    var d = value.D * Unit.Convert(Units, value.Units, ',');
                    --jStart;
                    for (int i = iStart - 1; i < _rowCount; ++i)
                    {
                        for (int j = jStart; j < _colCount; ++j)
                            if (GetValue(i, j).AlmostEquals(d))
                                return new([i + 1, j + 1], null);

                        jStart = 0;
                    }
                }
            }
            return new HpVector(2, null);
        }

        internal override RealValue Count(RealValue value)
        {
            var count = 0;
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
            {
                if (_rowCount > ParallelThreshold)
                    Parallel.For(0, _rowCount, i =>
                        count += (int)_hpRows[i].Count(value, 1).D);
                else
                    for (int i = 0; i < _rowCount; ++i)
                        count += (int)_hpRows[i].Count(value, 1).D;
            }
            else
            {
                if (!Unit.IsConsistent(Units, value.Units))
                    return RealValue.Zero;

                if (!Unit.IsConsistent(Units, value.Units))
                    return RealValue.Zero;

                var d = value.D * Unit.Convert(Units, value.Units, ',');
                if (_rowCount > ParallelThreshold)
                    Parallel.For(0, _rowCount, i =>
                        count += CountRow(i, d));
                else
                    for (int i = 0; i < _rowCount; ++i)
                        count += CountRow(i, d);

            }
            int CountRow(int i, double d)
            {
                for (int j = 0; j < _colCount; ++j)
                    if (GetValue(i, j).AlmostEquals(d))
                        ++count;

                return count;
            }
            return new(count);
        }

        internal override HpMatrix FindAll(in RealValue value, Vector.Relation rel)
        {
            var M = FromIndexes(FindAllIndexes(value, rel));
            if (M._colCount == 0)
                M = new HpMatrix(2, 1, null);

            return M;
        }

        internal override HpVector HLookup(in RealValue value, int searchRow, int returnRow, HpVector.Relation rel)
        {
            if (searchRow < 1 || searchRow > _rowCount)
                throw Exceptions.IndexOutOfRange(searchRow.ToString());
            if (returnRow < 1 || returnRow > _rowCount)
                throw Exceptions.IndexOutOfRange(returnRow.ToString());

            if (!Unit.IsConsistent(Units, value.Units))
                return new(1, _units);

            var d = value.D * Unit.Convert(Units, value.Units, ',');
            var i = searchRow - 1;
            var indexes = new List<int>();
            for (int j = 0; j < _colCount; ++j)
                if (HpVector.Relate(GetValue(i, j), d, rel))
                    indexes.Add(j);

            var n = indexes.Count;
            var vector = new HpVector(n, _units);
            i = returnRow - 1;
            for (int j = 0; j < n; ++j)
                vector.SetValue(GetValue(i, indexes[j]), j);

            return vector;
        }

        internal override HpVector VLookup(in RealValue value, int searchCol, int returnCol, Vector.Relation rel)
        {
            if (searchCol > _colCount)
                throw Exceptions.IndexOutOfRange(searchCol.ToString());
            if (returnCol > _colCount)
                throw Exceptions.IndexOutOfRange(returnCol.ToString());

            if (!Unit.IsConsistent(Units, value.Units))
                return new(1, _units);

            var d = value.D * Unit.Convert(Units, value.Units, ',');
            var j = searchCol - 1;
            var indexes = new List<int>();
            for (int i = 0; i < _rowCount; ++i)
                if (HpVector.Relate(GetValue(i, j), d, rel))
                    indexes.Add(i);

            var n = indexes.Count;
            var vector = new HpVector(n, _units);
            j = returnCol - 1;
            for (int i = 0; i < n; ++i)
                vector.SetValue(GetValue(indexes[i], j), i);

            return vector;
        }

        private List<(int, int)> FindAllIndexes(in RealValue value, HpVector.Relation rel)
        {
            var indexes = new List<(int, int)>();
            if (!Unit.IsConsistent(Units, value.Units))
                return indexes;

            var d = value.D * Unit.Convert(Units, value.Units, ',');
            for (int i = 0; i < _rowCount; ++i)
                for (int j = 0; j < _colCount; ++j)
                    if (HpVector.Relate(GetValue(i, j), d, rel))
                        indexes.Add((i + 1, j + 1));

            return indexes;
        }

        private static HpMatrix FromIndexes(IEnumerable<(int, int)> indexes)
        {
            var n = indexes.Count();
            var matrix = new HpMatrix(2, n, null);
            var k = 0;
            foreach (var (i, j) in indexes)
            {
                matrix.SetValue(i, 0, k);
                matrix.SetValue(j, 1, k);
                ++k;
            }
            return matrix;
        }

        internal override HpMatrix SortRows(int j, bool reverse = false)
        {
            var col = _type == MatrixType.Column ? _hpRows[0] : Col(j);
            var indexes = col.GetOrderIndexes(reverse);
            return ReorderRows(indexes);
        }

        internal override HpMatrix ReorderRows(int[] indexes)
        {
            HpMatrix M = new(_rowCount, _colCount, _units);
            var m = _rowCount - 1;
            if (_type == MatrixType.Full || _type == MatrixType.LowerTriangular)
                for (int i = m; i >= 0; --i)
                    M._hpRows[i] = _hpRows[indexes[i]];
            else
            {
                var n = _colCount - 1;
                for (int i = m; i >= 0; --i)
                {
                    var row = M._hpRows[i];
                    for (int k = n; k >= 0; --k)
                        row.SetValue(GetValue(indexes[i], k), k);
                }
            }
            return M;
        }

        internal override HpMatrix SortCols(int i, bool reverse = false)
        {
            if (i > _rowCount)
                throw Exceptions.IndexOutOfRange(i.ToString());

            var indexes = RowRef(i - 1).GetOrderIndexes(reverse);
            return ReorderCols(indexes);
        }

        internal override HpMatrix ReorderCols(int[] indexes)
        {
            HpMatrix M = new(_rowCount, _colCount, _units);
            var m = _rowCount - 1;
            var n = _colCount - 1;
            for (int k = m; k >= 0; --k)
            {
                var row = M._hpRows[k];
                for (int j = n; j >= 0; --j)
                    row.SetValue(GetValue(k, indexes[j]), j);
            }
            return M;
        }

        internal override HpVector OrderRows(int j, bool reverse = false)
        {
            if (j > _colCount)
                throw Exceptions.IndexOutOfRange(j.ToString());

            var col = _type == MatrixType.Column ? _hpRows[0] : Col(j);
            return col.Order(reverse);
        }

        internal override HpVector OrderCols(int i, bool reverse = false)
        {
            if (i > _rowCount)
                throw Exceptions.IndexOutOfRange(i.ToString());

            return RowRef(i - 1).Order(reverse);
        }

        internal override RealValue Min()
        {
            var min = _hpRows[0].Min().D;
            for (int i = 1, len = _hpRows.Length; i < len; ++i)
            {
                var b = _hpRows[i].Min().D;
                if (b < min)
                    min = b;
            }
            if (!IsStructurallyConsistentType && min > 0)
                min = 0;

            return new(min, _units);
        }

        internal override RealValue Max()
        {
            var max = _hpRows[0].Max().D;
            for (int i = 1, len = _hpRows.Length; i < len; ++i)
            {
                var b = _hpRows[i].Max().D;
                if (b > max)
                    max = b;
            }
            if (!IsStructurallyConsistentType && max < 0)
                max = 0;

            return new(max, _units);
        }

        internal override RealValue Sum()
        {
            var sum = _hpRows[0].Sum().D;
            for (int i = 1, len = _hpRows.Length; i < len; ++i)
                sum += _hpRows[i].Sum().D;

            return new(sum, _units);
        }

        internal override RealValue SumSq()
        {
            var sumsq = _hpRows[0].SumSq().D;
            for (int i = 1, len = _hpRows.Length; i < len; ++i)
                sumsq += _hpRows[i].SumSq().D;

            return new(sumsq, _units?.Pow(2f));
        }

        internal override RealValue Srss() => new(Math.Sqrt(SumSq().D), _units);

        internal override RealValue Average()
        {
            var n = _rowCount * _colCount;
            return n == 0 ? RealValue.Zero : new(Sum().D / n, _units);
        }

        internal override RealValue Product()
        {
            var n = _rowCount * _colCount;
            var u = _units?.Pow(n);
            if (!IsStructurallyConsistentType)
                return new(0d, u);

            var product = _hpRows[0].Product().D;
            for (int i = 1, len = _hpRows.Length; i < len; ++i)
                product *= _hpRows[i].Product().D;

            return new(product, u);
        }

        internal override RealValue Mean()
        {
            var n = _rowCount * _colCount;
            return n == 0 ? RealValue.Zero : new(Math.Pow(Product().D, 1d / n), _units);
        }

        internal override RealValue And()
        {
            if (!IsStructurallyConsistentType)
                return RealValue.Zero;

            for (int i = 0, len = _hpRows.Length; i < len; ++i)
                if (Math.Abs(_hpRows[i].And().D) < RealValue.LogicalZero)
                    return RealValue.Zero;

            return RealValue.One;
        }

        internal override RealValue Or()
        {
            for (int i = 0, len = _hpRows.Length; i < len; ++i)
                if (Math.Abs(_hpRows[i].Or().D) >= RealValue.LogicalZero)
                    return RealValue.One;

            return RealValue.Zero;
        }

        internal override RealValue Xor()
        {
            var b = Math.Abs(_hpRows[0].Xor().D) >= RealValue.LogicalZero;
            for (int i = 1, len = _hpRows.Length; i < len; ++i)
                b = b != Math.Abs(_hpRows[i].Xor().D) >= RealValue.LogicalZero;

            return b ? RealValue.One : RealValue.Zero;
        }

        internal override RealValue Gcd()
        {
            var v = _hpRows[0].Gcd();
            var a = Calculator.AsLong(v.D);
            var u = v.Units;
            for (int i = 1, len = _hpRows.Length; i < len; ++i)
            {
                v = _hpRows[i].Gcd();
                var b = Calculator.AsLong(v.D * Unit.Convert(u, v.Units, ','));
                a = Calculator.Gcd(a, b);
            }
            return new(a);
        }

        internal override RealValue Lcm()
        {
            var v = _hpRows[0].Lcm();
            var a = Calculator.AsLong(v.D);
            var u = v.Units;
            for (int i = 1, len = _hpRows.Length; i < len; ++i)
            {
                v = _hpRows[i].Lcm();
                var b = Calculator.AsLong(v.D * Unit.Convert(u, v.Units, ','));
                a = a * b / Calculator.Gcd(a, b);
            }
            return new(a);
        }

        internal override RealValue Take(in RealValue x, in RealValue y)
        {
            var dx = Math.Round(x.D, MidpointRounding.AwayFromZero);
            var dy = Math.Round(y.D, MidpointRounding.AwayFromZero);
            if (!double.IsNormal(dy) || dy < Calculator.DeltaMinus || dy > _rowCount * Calculator.DeltaPlus ||
                !double.IsNormal(dx) || dx < Calculator.DeltaMinus || dx > _colCount * Calculator.DeltaPlus)
                return RealValue.NaN;

            return this[(int)dy - 1, (int)dx - 1];
        }

        internal override RealValue Line(in RealValue x, in RealValue y)
        {
            var dx = x.D * Unit.Convert(Units, x.Units, '━');
            var dy = y.D * Unit.Convert(Units, y.Units, '━');
            if (!double.IsNormal(dy) || dy < Calculator.DeltaMinus || dy > _rowCount * Calculator.DeltaPlus ||
                !double.IsNormal(dx) || dx < Calculator.DeltaMinus || dx > _colCount * Calculator.DeltaPlus)
                return RealValue.NaN;

            var j = (int)Math.Floor(dx);
            var i = (int)Math.Floor(dy);
            var z11 = GetValue(i - 1, j - 1);
            if (j == dx || dx >= _colCount)
            {
                if (i == dy || dy >= _rowCount)
                    return new(z11, _units);

                var z21_ = GetValue(i, j - 1);
                return new(z11 + (z21_ - z11) * (dy - i), _units);
            }
            if (i == dy || dy >= _rowCount)
            {
                var z12_ = GetValue(i - 1, j);
                return new(z11 + (z12_ - z11) * (dx - j), _units);
            }
            var z12 = GetValue(i - 1, j);
            var z21 = GetValue(i, j - 1);
            var z22 = GetValue(i, j);
            var z1 = z11 + (z12 - z11) * (dx - j);
            var z2 = z21 + (z22 - z21) * (dx - j);
            return new(z1 + (z2 - z1) * (dy - i), _units);
        }

        internal override RealValue Spline(in RealValue x, in RealValue y)
        {
            var dx = x.D * Unit.Convert(Units, x.Units, '~');
            var dy = y.D * Unit.Convert(Units, y.Units, '~');
            if (!double.IsNormal(dy) || dy < Calculator.DeltaMinus || dy > _rowCount * Calculator.DeltaPlus ||
                !double.IsNormal(dx) || dx < Calculator.DeltaMinus || dx > _colCount * Calculator.DeltaPlus)
                return RealValue.NaN;

            var j = (int)Math.Floor(dx) - 1;
            var i = (int)Math.Floor(dy) - 1;
            var z0 = SplineRow(i, j, dx);
            if (i == dy || dy >= _rowCount)
                return new(z0, _units);

            var z1 = SplineRow(i + 1, j, dx);
            var dz = z1 - z0;
            var a = dz;
            var b = dz;
            dz = Math.Sign(dz);
            if (i > 0)
            {
                var z2 = SplineRow(i - 1, j, dx);
                a = (z1 - z2) * (Math.Sign(z0 - z2) == dz ? 0.5 : 0.25);
            }
            if (i < _rowCount - 2)
            {
                var z2 = SplineRow(i + 2, j, dx);
                b = (z2 - z0) * (Math.Sign(z2 - z1) == dz ? 0.5 : 0.25);
            }
            if (i == 0)
                a += (a - b) / 2;

            if (i == _rowCount - 2)
                b += (b - a) / 2;

            var t = dy - i - 1d;
            var z = z0 + ((z1 - z0) * (3 - 2 * t) * t + ((a + b) * t - a) * (t - 1)) * t;
            return new(z, _units);
        }

        private double SplineRow(int i, int j, double dx)
        {
            var y0 = GetValue(i, j);
            if (j == dx || dx >= _colCount)
                return y0;

            var y1 = GetValue(i, j + 1);
            var dy = y1 - y0;
            var a = dy;
            var b = dy;
            dy = Math.Sign(dy);
            if (j > 0)
            {
                var y2 = GetValue(i, j - 1);
                a = (y1 - y2) * (Math.Sign(y0 - y2) == dy ? 0.5 : 0.25);
            }
            if (j < _colCount - 2)
            {
                var y2 = GetValue(i, j + 2);
                b = (y2 - y0) * (Math.Sign(y2 - y1) == dy ? 0.5 : 0.25);
            }
            if (j == 0)
                a += (a - b) / 2;

            if (j == _colCount - 2)
                b += (b - a) / 2;

            var t = dx - j - 1d;
            var y = y0 + ((y1 - y0) * (3 - 2 * t) * t + ((a + b) * t - a) * (t - 1)) * t;
            return y;
        }

        internal override HpMatrix Transpose()
        {
            if (_rowCount == 1)
                return new HpColumnMatrix(_hpRows[0]);

            var M = new HpMatrix(_colCount, _rowCount, _units);
            for (int i = _colCount - 1; i >= 0; --i)
                M._hpRows[i] = Col(i + 1);

            return M;
        }

        internal override RealValue Trace()
        {
            if (_rowCount != _colCount)
                throw Exceptions.MatrixNotSquare();

            var trace = GetValue(0, 0);
            for (int i = 1; i < _rowCount; ++i)
                trace += GetValue(i, i);

            return new(trace, _units);
        }

        //L1 norm   
        internal override RealValue L1Norm()
        {
            var norm = Col(1).L1Norm().D;
            if (_colCount > ParallelThreshold)
                Parallel.For(1, _colCount, j =>
                {
                    var colNorm = Col(j + 1).L1Norm().D;
                    if (colNorm > norm) norm = colNorm;
                });
            else
                for (int j = 1; j < _colCount; ++j)
                {
                    var colNorm = Col(j + 1).L1Norm().D;
                    if (colNorm > norm) norm = colNorm;
                }
            return new(norm, _units);
        }

        internal override RealValue L2Norm()
        {
            GetSVD(out _, out HpVector sig, out _);
            return sig.Max();
        }

        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm()
        {
            var norm = _hpRows[0].L1Norm().D;
            var len = _hpRows.Length;
            if (len > ParallelThreshold)
                Parallel.For(1, len, i =>
                {
                    var rowNorm = _hpRows[i].L1Norm().D;
                    if (rowNorm > norm) norm = rowNorm;
                });
            else
                for (int i = 1; i < len; ++i)
                {
                    var rowNorm = _hpRows[i].L1Norm().D;
                    if (rowNorm > norm) norm = rowNorm;
                }
            return new(norm, _units);
        }

        internal override HpMatrix LUDecomposition(out int[] indexes)
        {
            var LU = GetLU(out indexes, out double _, out double _) ??
                throw Exceptions.MatrixSingular();

            return LU;
        }

        protected override HpMatrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            if (_rowCount != _colCount)
                throw Exceptions.MatrixNotSquare();

            var LU = new HpMatrix(_rowCount, _rowCount, _units);
            var vv = new double[_rowCount];
            indexes = new int[_rowCount];
            //Column buffer for faster access
            var col_j = _rowCount <= 1000 ? stackalloc double[_rowCount] : new double[_rowCount].AsSpan();
            det = 1d; //Used to compute the sign change of determinant after pivot interchanges
            minPivot = 0d;
            for (int i = 0; i < _rowCount; ++i)
            {
                indexes[i] = i;
                var row = _hpRows[i].Copy();
                LU._hpRows[i] = row;
                var size = row.Size;
                if (size == 0)
                    return null;

                var big = row.MaxAbs();
                if (big == 0d)
                    return null;

                vv[i] = 1d / big;
            }
            minPivot = double.MaxValue; //Used to determine singularity
            for (int j = 0; j < _colCount; ++j)
            {
                //Cache column j for faster access
                CacheLUColumn(LU, col_j, j, _rowCount);
                for (int i = 0; i < j; ++i)
                {
                    var size = Math.Min(LU._rows[i].Size, i);
                    var row = LU._hpRows[i].Raw;
                    var sum = col_j[i];
                    var k0 = 0;
                    while (k0 < i && row[k0] == 0d) ++k0;
                    if (size > 0)
                        sum -= Vectorized.DotProduct(row, col_j, k0, size);

                    col_j[i] = sum;
                }
                var big = 0d;
                var imax = j;
                for (int i = j; i < _rowCount; ++i)
                {
                    var size = Math.Min(LU._hpRows[i].Size, j);
                    var row = LU._hpRows[i].Raw;
                    var sum = col_j[i];
                    var k0 = 0;
                    while (k0 < j && row[k0] == 0d) ++k0;
                    if (size > 0)
                        sum -= Vectorized.DotProduct(row, col_j, k0, size);

                    col_j[i] = sum;
                    var dum = vv[i] * Math.Abs(sum);
                    if (dum > big)
                    {
                        big = dum;
                        imax = i;
                    }
                }
                var d = Math.Abs(big);
                if (j != imax)
                {
                    if (d < minPivot)
                        minPivot = d;

                    (LU._hpRows[j], LU._hpRows[imax]) = (LU._hpRows[imax], LU._hpRows[j]);
                    (col_j[j], col_j[imax]) = (col_j[imax], col_j[j]); //Swap also cached values
                    vv[imax] = vv[j];
                    (indexes[j], indexes[imax]) = (indexes[imax], indexes[j]);
                    det = -det;
                }
                if (j != _rowCount - 1)
                {
                    var dum = 1d / col_j[j];
                    Vectorized.Scale(col_j[(j + 1).._rowCount], dum);
                }
                else if (d < minPivot)
                    minPivot = d;

                //Restore column j from buffer
                RestoreLUColumn(LU, col_j, j, _rowCount);
            }
            return minPivot == 0d ? null : LU;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CacheLUColumn(HpMatrix LU, Span<double> col_j, int j, int m)
        {
            int k = 0;
            for (; k + 3 < m; k += 4)
            {
                col_j[k] = LU.GetValue(k, j);
                col_j[k + 1] = LU.GetValue(k + 1, j);
                col_j[k + 2] = LU.GetValue(k + 2, j);
                col_j[k + 3] = LU.GetValue(k + 3, j);
            }
            for (; k < m; ++k)
                col_j[k] = LU.GetValue(k, j);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RestoreLUColumn(HpMatrix LU, Span<double> col_j, int j, int m)
        {
            int k = 0;
            for (; k + 3 < m; k += 4)
            {
                LU.SetValue(col_j[k], k, j);
                LU.SetValue(col_j[k + 1], k + 1, j);
                LU.SetValue(col_j[k + 2], k + 2, j);
                LU.SetValue(col_j[k + 3], k + 3, j);
            }
            for (; k < m; ++k)
                LU.SetValue(col_j[k], k, j);
        }

        private void GetQR(out HpMatrix Q, out HpMatrix R)
        {
            var m = _rowCount;
            var n = _colCount;
            R = Copy();
            Q = Identity(m);
            var v = new double[m];
            var mn = Math.Min(m, n);
            for (int k = 0; k < mn; ++k)
            {
                // Build the Householder vector
                var normX = 0d;
                double r;
                for (int i = k; i < m; ++i)
                {
                    r = R.GetValue(i, k);
                    normX += r * r;
                }
                normX = Math.Sqrt(normX);
                if (normX == 0d)
                    continue;

                r = R.GetValue(k, k);
                var alpha = r > 0 ? -normX : normX;
                var rs = 2 * Math.Sqrt(0.5 * alpha * (alpha - r));
                v[k] = (r - alpha) / rs;
                for (int i = k + 1; i < m; ++i)
                    v[i] = R.GetValue(i, k) / rs;

                // Apply reflector to R (from the left): R = (I - 2vvᵗ) * R
                for (int j = k; j < n; ++j)
                {
                    var dot = 0d;
                    for (int i = k; i < m; ++i)
                        dot += v[i] * R.GetValue(i, j);

                    r = 2 * dot;
                    for (int i = k; i < m; ++i)
                        R.SetValue(R.GetValue(i, j) - r * v[i], i, j);
                }

                // Apply reflector to Q (from the right): Q = Q * (I - 2vvᵗ)
                for (int i = 0; i < m; ++i)
                {
                    var Q_i = Q._hpRows[i];
                    var size = Q_i.Size;
                    var dot = Vectorized.DotProduct(v, Q_i.Raw, k, size);
                    r = 2 * dot;
                    for (int j = m - 1; j >= k; --j)
                        Q_i.SetValue(Q_i.GetValue(j) - r * v[j], j);
                }
            }
        }

        private void GetSVD(out HpMatrix U, out HpVector sig, out HpMatrix V)
        {
            if (_rowCount < _colCount)
                throw Exceptions.MatrixNotHigh();

            var m = _rowCount - 1;
            var n = _colCount - 1;
            var rv1 = new HpVector(_colCount, _colCount, null);
            var rv1Raw = rv1.Raw;
            var Ud = RawCopy();
            sig = new HpVector(_colCount, _colCount, _units);
            var sigRaw = sig.Raw;
            var Vd = new double[_colCount][];
            //Initialize V matrix rows
            for (int i = 0; i < _colCount; ++i)
                Vd[i] = new double[_colCount];

            var colBuffer = _rowCount <= 1000 ? stackalloc double[_rowCount] : new double[_rowCount].AsSpan();
            HpSvdSolver.Bidiagonalize(Ud, sigRaw, rv1Raw, colBuffer, m, n, out double anorm);
            HpSvdSolver.RightTransform(Ud, Vd, rv1Raw, n);
            HpSvdSolver.LeftTransform(Ud, sigRaw, colBuffer, m, n);
            HpSvdSolver.Diagonalize(Ud, sigRaw, Vd, rv1Raw, m, n, anorm);
            U = new(Ud, null);
            V = new(Vd, null);
        }

        private static void SvdSortDescending(ref HpMatrix U, ref HpVector sig, ref HpMatrix V)
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
        internal override Matrix QRDecomposition()
        {
            GetQR(out HpMatrix Q, out HpMatrix R);
            if (Unit.IsConsistent(Q._units, R._units))
                return Augment(Q, R);

            return Matrix.Augment(Q, R);
        }

        internal override Matrix SVDDecomposition()
        {
            GetSVD(out HpMatrix U, out HpVector sig, out HpMatrix V);
            SvdSortDescending(ref U, ref sig, ref V);
            if (Unit.IsConsistent(U._units, sig.Units) && Unit.IsConsistent(V._units, sig.Units))
                return Augment(U, sig, V);

            return Matrix.Augment(U, sig, V);
        }

        private static HpMatrix Identity(int n)
        {
            var M = new HpMatrix(n, n, null);
            for (int i = 0; i < n; ++i)
                M.SetValue(1d, i, i);

            return M;
        }

        private static void FwdAndBackSubst(HpMatrix LU, int[] indexes, HpVector v, double[] x)
        {
            var m = LU._rowCount;
            var start = -1;
            //Forward substitution. Solve Ly = v by storing y in x. 
            for (int i = 0; i < m; ++i)
            {
                var index = indexes[i];
                var sum = v.GetValue(index);
                if (start >= 0)
                {
                    var row = LU._hpRows[i];
                    var size = Math.Min(row.Size, i);
                    sum -= Vectorized.DotProduct(row.Raw, x, start, size);
                }
                else if (sum != 0d)
                    start = i;

                x[i] = sum;
            }

            //Back substitution. Solve Ux = y.
            for (int i = m - 1; i >= 0; --i)
            {
                var row = LU._hpRows[i];
                var size = Math.Min(row.Size, m);
                var sum = x[i] - Vectorized.DotProduct(row.Raw, x, i + 1, size);
                x[i] = sum / LU.GetValue(i, i);
            }
        }

        internal virtual HpVector LSolve(HpVector vector)
        {
            var LU = GetLU(out int[] indexes, out double minPivot, out double _) ??
                throw Exceptions.MatrixSingular();

            if (minPivot < 1e-15)
                throw Exceptions.MatrixCloseToSingular();

            var x = new double[_rowCount];
            FwdAndBackSubst(LU, indexes, vector, x);
            return new(x, _units);
        }

        internal virtual HpMatrix MSolve(HpMatrix M)
        {
            var LU = GetLU(out int[] indexes, out double minPivot, out double _) ??
                throw Exceptions.MatrixSingular();

            if (minPivot < 1e-15)
                throw Exceptions.MatrixCloseToSingular();


            return MSolveForLU(LU, indexes, M, _units);
        }

        private static HpMatrix MSolveForLU(HpMatrix LU, int[] indexes, HpMatrix M, Unit units)
        {
            var m = LU._rowCount;
            var n = M._colCount;
            var result = new HpVector[n];
            var u = Unit.Divide(M.Units, units, out var d, true);
            Parallel.For(0, n, j =>
            {
                var x = new double[m];
                FwdAndBackSubst(LU, indexes, M.Col(j + 1), x);
                if (d != 1d)
                    Vectorized.Scale(x, d);

                result[j] = new(x, u);
            });
            return CreateFromCols(result, m);
        }

        private static HpMatrix MSolveForLU(HpMatrix LU, int[] indexes, Unit units)
        {
            var m = LU._rowCount;
            var I = Identity(m);
            var result = new HpVector[m];
            Parallel.For(0, m, j =>
            {
                var x = new double[m];
                FwdAndBackSubst(LU, indexes, I._hpRows[j], x);
                result[j] = new(x, units);
            });
            for (int j = m - 1; j >= 0; --j)
                I.SetCol(j, result[j]);

            return I;
        }

        internal override HpMatrix Invert()
        {
            var LU = GetLU(out int[] indexes, out double minPivot, out double _) ??
                throw Exceptions.MatrixSingular();

            if (minPivot < 1e-15)
                throw Exceptions.MatrixCloseToSingular();

            return GetInverse(LU, indexes);
        }

        private HpMatrix GetInverse(HpMatrix lu, int[] indexes)
        {
            var units = _units?.Pow(-1);
            if (_rowCount > ParallelThreshold)
                return MSolveForLU(lu, indexes, units);

            var vector = new HpVector(_rowCount, _rowCount, null);
            var x = new double[_rowCount];
            var M = new HpMatrix(_rowCount, _rowCount, units);
            for (int j = 0; j < _rowCount; ++j)  //Find inverse by columns.
            {
                vector.SetValue(1d, j);
                FwdAndBackSubst(lu, indexes, vector, x);
                for (int i = 0; i < _rowCount; ++i)
                    M.SetValue(x[i], i, j);

                vector.SetValue(0d, j);
            }
            return M;
        }

        internal override RealValue Determinant()
        {
            if (_rowCount != _colCount)
                throw Exceptions.MatrixNotSquare();

            var LU = GetLU(out int[] _, out double _, out double det);
            if (LU is null)
                return RealValue.Zero;

            return GetDeterminant(LU) * det;
        }

        private RealValue GetDeterminant(HpMatrix lu)
        {
            var det = lu.GetValue(0, 0);
            for (int i = 1; i < _rowCount; ++i)
                det *= lu.GetValue(i, i);

            return new(det, _units?.Pow(_rowCount));
        }

        internal override RealValue Cond2()
        {
            GetSVD(out HpMatrix _, out HpVector sig, out HpMatrix _);
            return sig.Max() / sig.Min();
        }

        internal override RealValue Cond1() => Condition(M => M.L1Norm());
        internal override RealValue CondFrob() => Condition(M => M.FrobNorm());
        internal override RealValue CondInf() => Condition(M => M.InfNorm());

        protected virtual RealValue Condition(Func<HpMatrix, RealValue> norm)
        {
            if (_rowCount != _colCount)
                throw Exceptions.MatrixNotSquare();

            var LU = GetLU(out int[] indexes, out double _, out double _); //Decompose the matrix by LU decomp.
            if (LU is null)
                return RealValue.PositiveInfinity;

            var M = GetInverse(LU, indexes);
            return norm(this) * norm(M);
        }

        internal override RealValue Rank()
        {
            var rank = 0;
            GetSVD(out HpMatrix _, out HpVector sig, out HpMatrix _);
            var sigMax = sig.Max().D;
            var eps = (double.BitIncrement(sigMax) - sigMax);
            for (int i = 0, len = sig.Length; i < len; ++i)
            {
                if (Math.Abs(sig[i].D) > eps)
                    ++rank;
            }
            return new(rank);
        }

        internal override HpMatrix Adjoint()
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

        private HpMatrix AdjointByMinors()
        {
            int n = _rowCount;
            var result = new HpMatrix(n, n, _units?.Pow(n - 1));
            int[] full = new int[n];
            for (int i = 0; i < n; ++i)
            {
                full[i] = i;
                result._hpRows[i] = new HpVector(n, n, _units);
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
                    result.SetValue(det * sign, j, i); // note the transpose
                }
            });
            return result;
        }

        private double Determinant(Span<int> rows, Span<int> cols)
        {
            var n = rows.Length;
            if (n == 1)
                return GetValue(rows[0], cols[0]);

            if (n == 2)
                return Determinant2x2(rows, cols);

            if (n == 3)
                return Determinant3x3(rows, cols);

            var det = 0d;
            var subRows = rows[1..]; // exclude row 0
            Span<int> subCols = stackalloc int[n - 1];
            for (int j = 0; j < n; ++j)
            {
                var a = GetValue(rows[0], cols[j]);
                if (a == 0d) continue;
                RemoveAt(cols, j, subCols);
                var sign = (j % 2 == 0) ? 1.0 : -1.0;
                det += a * Determinant(subRows, subCols) * sign;
            }
            return det;
        }

        private double Determinant2x2(ReadOnlySpan<int> rows, ReadOnlySpan<int> cols)
        {
            var r_0 = rows[0];
            var r_1 = rows[1];
            var a = GetValue(r_0, cols[0]);
            var b = GetValue(r_0, cols[1]);
            var c = GetValue(r_1, cols[0]);
            var d = GetValue(r_1, cols[1]);
            return a * d - b * c;
        }

        private double Determinant3x3(ReadOnlySpan<int> rows, ReadOnlySpan<int> cols)
        {
            var r_0 = rows[0];
            var r_1 = rows[1];
            var r_2 = rows[2];
            var a = GetValue(r_0, cols[0]);
            var b = GetValue(r_0, cols[1]);
            var c = GetValue(r_0, cols[2]);
            var d = GetValue(r_1, cols[0]);
            var e = GetValue(r_1, cols[1]);
            var f = GetValue(r_1, cols[2]);
            var g = GetValue(r_2, cols[0]);
            var h = GetValue(r_2, cols[1]);
            var i = GetValue(r_2, cols[2]);
            return a * (e * i - f * h)
                 - b * (d * i - f * g)
                 + c * (d * h - e * g);
        }

        internal override HpMatrix Cofactor() => Adjoint().Transpose();

        internal virtual HpMatrix CopyTo(HpMatrix target, int i, int j)
        {
            --i; --j;
            var m = Math.Min(_rowCount, target._rowCount - i);
            var n = Math.Min(_colCount, target._colCount - j);
            var d = Unit.Convert(target.Units, _units, ',');
            for (int ii = 0; ii < m; ++ii)
            {
                var i1 = i + ii;
                for (int jj = 0; jj < n; ++jj)
                {
                    var j1 = j + jj;
                    if (IsTarget(target._type, i1, j1))
                        target.SetValue(GetValue(ii, jj) * d, i1, j1);
                }
            }
            return target;
        }

        internal virtual HpMatrix AddTo(HpMatrix target, int i, int j)
        {
            --i; --j;
            var m = Math.Min(_rowCount, target._rowCount - i);
            var n = Math.Min(_colCount, target._colCount - j);
            var d = Unit.Convert(target.Units, _units, ',');
            for (int ii = 0; ii < m; ++ii)
            {
                var i1 = i + ii;
                for (int jj = 0; jj < n; ++jj)
                {
                    var j1 = j + jj;
                    if (IsTarget(target._type, i1, j1))
                    {
                        var v = target.GetValue(i1, j1);
                        if (v == 0d)
                            target.SetValue(GetValue(ii, jj) * d, i1, j1);
                        else
                            target.SetValue(v + GetValue(ii, jj) * d, i1, j1);

                    }
                }
            }
            return target;
        }

        internal override HpVector Diagonal()
        {
            var n = Math.Min(_rowCount, _colCount);
            HpVector vector = new(n, n, _units);
            var values = vector.Raw;
            for (int i = 0; i < n; ++i)
                values[i] = GetValue(i, i);

            return vector;
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

        internal static HpMatrix CreateFromCols(HpVector[] cols, int m)
        {
            var n = cols.Length;
            var u = cols[0].Units;
            var M = new HpMatrix(m, n, u);
            for (int j = n - 1; j >= 0; --j)
            {
                var d = Unit.Convert(u, cols[j].Units, ',');
                if (d == 1d)
                    M.SetCol(j, cols[j]);
                else
                    M.SetCol(j, cols[j] * new RealValue(d));
            }
            return M;
        }

        internal static HpMatrix CreateFromRows(HpVector[] rows, int n)
        {
            var m = rows.Length;
            var u = rows[0].Units;
            var M = new HpMatrix(m, n, u);
            for (int i = m - 1; i >= 0; --i)
            {
                var d = Unit.Convert(u, rows[i].Units, ',');
                if (d == 1d)
                    M.SetRow(i, rows[i]);
                else
                    M.SetRow(i, rows[i] * new RealValue(d));
            }
            return M;
        }

        internal void Scale(double d)
        {
            for (int i = 0, n = _hpRows.Length; i < n; ++i)
                _hpRows[i].Scale(d);
        }

        internal double[][] AsJaggedArray()
        {
            var n = _hpRows.Length;
            var jaggedArray = new double[n][];
            for (int i = 0; i < n; ++i)
            {
                var row = _hpRows[i];
                jaggedArray[i] = new double[row.Size];
                for (int j = 0; j < row.Size; ++j)
                    jaggedArray[i][j] = row.Raw[j];
            }
            return jaggedArray;
        }

        internal double[][] AsJaggedArrayTransposed()
        {
            var m = _hpRows[0].Size;
            var n = _hpRows.Length;
            var jaggedArray = new double[m][];
            for (int i = 0; i < m; ++i)
                jaggedArray[i] = new double[n];

            for (int i = 0; i < n; ++i)
            {
                var row = _hpRows[i];
                for (int j = 0; j < m; ++j)
                    jaggedArray[j][i] = row.Raw[j];
            }
            return jaggedArray;
        }

        internal static HpMatrix FromJaggedArrayTransposed(double[][] jaggedArray, Unit units)
        {
            var m = jaggedArray[0].Length;
            var n = jaggedArray.Length;
            var M = new HpMatrix(m, n, units);
            Parallel.For(0, m, i =>
            {
                var row = M._hpRows[i];
                for (int j = 0; j < n; ++j)
                    row.SetValue(jaggedArray[j][i], j);
            });
            return M;
        }

        internal HpMatrix FFT(bool inverse)
        {
            var n = _colCount;
            var m = _rowCount;
            if (m == 0 || n == 0)
                return this;

            var power = Math.Ceiling(Math.Log2(n));
            n = (int)Math.Pow(2, power);
            var re = new double[n];
            var im = new double[n];
            if (_rowCount == 1 || _rowCount == 2)
            {
                var row = _hpRows[0];
                row.Raw.AsSpan(0, row.Size).CopyTo(re);
                if (_rowCount == 2)
                {
                    row = _hpRows[1];
                    row.Raw.AsSpan(0, row.Size).CopyTo(im);
                }
            }
            else
                throw Exceptions.MatrixOneOrTwoRows();

            HpFFT.Transform(re, im, inverse ? -1 : 1);
            var result = new HpMatrix(2, n, _units);
            result._hpRows[0] = new HpVector(re, _units);
            result._hpRows[1] = new HpVector(im, _units);
            if (inverse)
                result.Scale(1d / n);

            return result;
        }
    }
}