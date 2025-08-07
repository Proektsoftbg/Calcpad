using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class HpColumnMatrix : HpMatrix
    {
        internal HpColumnMatrix(int length, Unit units)
        {
            if (length > MaxSize)
                throw Exceptions.MatrixDimensions();

            _type = MatrixType.Column;
            _rowCount = length;
            _colCount = 1;
            _units = units;
            _hpRows = [new HpVector(length, units)];
            _rows = _hpRows;
        }

        internal HpColumnMatrix(int length, RealValue value) : this(length, value.Units)
        {
            _hpRows[0].Fill(value);
        }

        internal HpColumnMatrix(Vector v)
        {
            if (v.Length > MaxSize)
                throw Exceptions.MatrixDimensions();

            _type = MatrixType.Column;
            _rowCount = v.Length;
            _colCount = 1;
            var hp_v = v is HpVector hp ? hp : new HpVector(v, null);
            _hpRows = [hp_v];
            _rows = _hpRows;
            _units = hp_v.Units;
        }

        internal HpColumnMatrix(ColumnMatrix matrix) : this(matrix.Rows[0]) { }

        internal override RealValue this[int row, int col]
        {
            get => _hpRows[0][row];
            set => _hpRows[0][row] = value;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override double GetValue(int row, int col) => _hpRows[0].GetValue(row);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetValue(double value, int row, int col) => _hpRows[0].SetValue(value, row);

        internal override HpMatrix Clone() => new HpColumnMatrix(_rowCount, _units);
        internal override Matrix CloneAsMatrix() => new ColumnMatrix(_rowCount);

        internal HpColumnMatrix Resize(int n)
        {
            if (n != _rowCount)
            {
                _hpRows[0].Resize(n);
                _rowCount = n;
            }
            return this;
        }

        public static HpColumnMatrix operator *(HpMatrix a, HpColumnMatrix b)
        {
            HpColumnMatrix c;
            if (a is HpColumnMatrix ac)
            {
                if (ac._rowCount != b._rowCount)
                    throw Exceptions.MatrixDimensions();

                c = new HpColumnMatrix(ac._hpRows[0] * b._hpRows[0]);
            }
            else
            {
                var m = a.RowCount;
                var u = Unit.Multiply(a.Units, b.Units, out var d, true);
                var br = b._hpRows[0];
                var cr = new double[m];
                if (m > ParallelThreshold)
                    Parallel.For(0, m, i =>
                        cr[i] = HpVector.DotProduct(a.RowRef(i), br).D * d);
                else
                    for (int i = 0; i < m; ++i)
                        cr[i] = HpVector.DotProduct(a.RowRef(i), br).D * d;

                c = new HpColumnMatrix(new HpVector(cr, u));
            }
            return c;
        }

        public static HpColumnMatrix operator *(HpUpperTriangularMatrix a, HpColumnMatrix b)
        {
            var m = a.RowCount;
            var br = b._hpRows[0];
            var vb = br.Raw;
            var n = br.Size;
            var vc = new double[m];
            var u = Unit.Multiply(a.Units, b.Units, out var d, true);
            if (m > ParallelThreshold)
                Parallel.For(0, m, MultiplyRow);
            else
                for (int i = 0; i < m; ++i)
                    MultiplyRow(i);

            return new HpColumnMatrix(new HpVector(vc, u));

            void MultiplyRow(int i)
            {
                var ar = a.HpRows[i];
                var size = Math.Min(n - i, ar.Size);
                vc[i] = Vectorized.DotProduct(ar.Raw, vb.AsSpan(i), 0, size) * d;
            }
        }

        public static HpColumnMatrix operator *(HpSymmetricMatrix a, HpColumnMatrix b)
        {
            var m = a.RowCount;
            var bc = b._hpRows[0];
            var vb = bc.Raw;
            var n = bc.Size;
            var vc = new double[m];
            var u = Unit.Multiply(a.Units, b.Units, out var d, true);
            for (int i = 0; i < m; ++i)
            {
                var row = a.HpRows[i];
                var va = row.Raw;
                var b_i = vb[i];
                var sum = va[0] * b_i;
                for (int k = 1, size = row.Size; k < size; ++k)
                {
                    var j = i + k;
                    if (j < n)
                    {
                        var val = va[k];
                        sum += val * vb[j];
                        vc[j] += val * b_i; // Symmetric contribution
                    }
                }
                vc[i] += sum;
            }
            return new HpColumnMatrix(new HpVector(vc, u));
        }

        internal override HpMatrix Transpose()
        {
            var c = new HpMatrix(1, _rowCount, _units);
            c.HpRows[0] = _hpRows[0].Copy();
            return c;
        }

        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm() => _hpRows[0].InfNorm();

        internal Vector ColRef() => _hpRows[0];
    }
}
