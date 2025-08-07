using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class HpLowerTriangularMatrix : HpMatrix
    {
        internal HpLowerTriangularMatrix(int length, Unit units) : base(length, units)
        {
            _type = MatrixType.LowerTriangular;
            _hpRows = new HpVector[length];
            for (int i = 0; i < length; ++i)
                _hpRows[i] = new HpVector(i + 1, units);

            _rows = _hpRows;
        }

        internal HpLowerTriangularMatrix(LowerTriangularMatrix matrix) : base(matrix)
        {
            _type = MatrixType.LowerTriangular;
        }

        internal override RealValue this[int row, int col]
        {
            get => row >= col ? _hpRows[row][col] : new(0d, _units);
            set
            {
                if (row >= col)
                    _hpRows[row][col] = value;
                else
                    throw Exceptions.IndexOutOfRange($"{row + 1}, {col + 1}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override double GetValue(int row, int col) => row >= col ? _hpRows[row].GetValue(col) : 0d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetValue(double value, int row, int col)
        {
            if (row >= col)
                _hpRows[row].SetValue(value, col);
            else
                throw Exceptions.IndexOutOfRange($"{row + 1}, {col + 1}");
        }

        internal override HpMatrix Clone() => new HpLowerTriangularMatrix(_rowCount, _units);
        internal override Matrix CloneAsMatrix() => new LowerTriangularMatrix(_rowCount);

        internal HpLowerTriangularMatrix Resize(int n)
        {
            if (n != _rowCount)
            {
                Array.Resize(ref _hpRows, n);
                for (int i = _rowCount; i < n; ++i)
                    _hpRows[i] = new HpVector(i + 1, _units);

                _rowCount = n;
                _colCount = n;
            }
            return this;
        }

        internal double[] RawCol(int col)
        {
            var n = _colCount - col;
            var vector = new double[n];
            for (int i = n - 1; i >= 0; --i)
                vector[i] =_hpRows[col + i].GetValue(col);

            return vector;
        }

        internal override HpUpperTriangularMatrix Transpose()
        {
            var U = new HpUpperTriangularMatrix(_rowCount, _units);
            for (int i = _rowCount - 1; i >= 0; --i)
                U.HpRows[i] = new(RawCol(i), _units);

            return U;
        }

        internal override RealValue Determinant()
        {
            var det = _hpRows[0].GetValue(0);
            for (int i = 1; i < _rowCount; ++i)
                det *= _hpRows[i].GetValue(i);

            return new(det, _units?.Pow(_rowCount));
        }

        protected override HpMatrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            indexes = new int[_rowCount];
            minPivot = double.MaxValue;
            det = 1d;
            for (int i = 0; i < _rowCount; ++i)
            {
                if (_hpRows[i].GetValue(i) == 0d)
                    return null;

                indexes[i] = i;
            }
            return RawCopy();
        }

        internal override HpMatrix Invert()
        {
            var L = new HpLowerTriangularMatrix(_rowCount, _units?.Pow(-1));
            for (int i = 0; i < _rowCount; ++i)
            {
                var row = _hpRows[i].Raw;
                var v = row[i];
                if (v == 0d)
                    throw Exceptions.MatrixSingular();

                v = 1 / v;
                L.SetValue(v, i, i);
                for (int j = 0; j < i; ++j)
                {
                    var sum = row[j] * L.GetValue(j, j);
                    for (int k = j + 1; k < i; ++k)
                        sum += row[k] * L.GetValue(k, j);

                    L.SetValue(-sum * v, i, j);
                }
            }
            return L;
        }

        internal override HpVector LSolve(HpVector vector)
        {
            var units = Unit.Divide(vector.Units, _units, out var d, true);
            HpVector x = new(_rowCount, units);
            var values = vector.Raw;
            var xv = x.Raw;
            for (int i = 0; i < _rowCount; ++i)
            {
                var sum = values[i];
                var row = _hpRows[i].Raw;
                for (int j = 0; j < i; ++j)
                    sum -= row[j] * xv[j];

                var ri = row[i];
                if (ri == 0)
                    throw Exceptions.MatrixSingular();

                xv[i] = sum * d / ri;
            }
            return x;
        }

        internal override HpMatrix MSolve(HpMatrix M)
        {
            var m = _rowCount;
            var n = M.ColCount;
            var result = new HpVector[n];
            Parallel.For(0, n, j => {
                var vector = M.Col(j + 1);
                result[j] = LSolve(vector);
            });
            return CreateFromCols(result, m);
        }

        private new HpLowerTriangularMatrix RawCopy()
        {
            var n = _hpRows.Length;
            var M = new HpLowerTriangularMatrix(n, _units);
            for (int i = n - 1; i >= 0; --i)
                M._hpRows[i] = _hpRows[i].Copy();

            return M;
        }
    }
}
