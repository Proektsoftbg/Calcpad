using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class HpUpperTriangularMatrix : HpMatrix
    {
        internal HpUpperTriangularMatrix(int length, Unit units) : base(length, units)
        {
            _type = MatrixType.UpperTriangular;
            _hpRows = new HpVector[length];
            for (int i = length - 1; i >= 0; --i)
                _hpRows[i] = new HpVector(length - i, _units);

            _rows = _hpRows;
        }

        internal HpUpperTriangularMatrix(UpperTriangularMatrix matrix) : base(matrix)
        {
            _type = MatrixType.UpperTriangular;
        }

        internal override RealValue this[int row, int col]
        {
            get => row <= col ? _hpRows[row][col - row] : new(0d, _units);
            set
            {
                if (row <= col)
                    _hpRows[row][col - row] = value;
                else
                    throw Exceptions.IndexOutOfRange($"{row}, {col}");
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override double GetValue(int row, int col) => row <= col ? _hpRows[row].GetValue(col - row) :0d;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetValue(double value, int row, int col)
        {
            if (row <= col)
                _hpRows[row].SetValue(value, col - row);
            else
                throw Exceptions.IndexOutOfRange($"{row}, {col}");
        }

        internal override HpMatrix Clone() => new HpUpperTriangularMatrix(_rowCount, _units);
        internal override Matrix CloneAsMatrix() => new UpperTriangularMatrix(_rowCount);

        internal HpUpperTriangularMatrix Resize(int n)
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

        internal HpVector RawCol(int col)
        {
            var vector = new HpVector(col + 1, _units);
            for (int i = col; i >= 0; --i)
                vector.SetValue(_hpRows[i].GetValue(col - i), i);

            return vector;
        }

        internal override HpLowerTriangularMatrix Transpose()
        {
            var L = new HpLowerTriangularMatrix(_rowCount, _units);
            for (int i = _rowCount - 1; i >= 0; --i)
                L.HpRows[i] = RawCol(i);

            return L;
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
                if (_hpRows[i][0].D == 0d)
                    return null;

                indexes[i] = i;
            }
            return RawCopy();
        }

        internal override HpMatrix Invert()
        {
            var U = new HpUpperTriangularMatrix(_rowCount, _units);
            var n = _rowCount - 1;
            for (int i = n; i >= 0; --i)
            {
                var row = _hpRows[i].Raw;
                var v = row[0];
                if (v == 0d)
                    throw Exceptions.MatrixSingular();

                v = 1d / v;
                U.SetValue(v, i, i);
                for (int j = n; j > i; --j)
                {
                    var sum = row[n - i] * U.GetValue(n, j);
                    for (int k = n - 1; k > i; --k)
                        sum += row[k - i] * U.GetValue(k, j);

                    U.SetValue(-sum * v, i, j);
                }
            }
            return U;
        }

        internal override Vector LSolve(Vector v)
        {
            Vector x = new(_rowCount);
            var n = _rowCount - 1;
            for (int i = n; i >= 0; --i)
            {
                var sum = v[i];
                var row = _hpRows[i];
                for (int j = n; j > i; --j)
                    sum -= row[j - i] * x[j];

                x[i] = sum / row[0];
            }
            return x;
        }

        internal override HpMatrix MSolve(HpMatrix M)
        {
            var m = _rowCount;
            var n = M.ColCount;
            var v = new HpVector[n];
            Parallel.For(0, n, j =>
                v[j] = LSolve(M.Col(j + 1))
            );
            return CreateFromCols(v, m);
        }

        private new HpLowerTriangularMatrix RawCopy()
        {
            var n = _hpRows.Length;
            var M = new HpLowerTriangularMatrix(n, _units);
            for (int i = n - 1; i >= 0; --i)
                M.HpRows[i] = _hpRows[i].Copy();

            return M;
        }
    }
}
