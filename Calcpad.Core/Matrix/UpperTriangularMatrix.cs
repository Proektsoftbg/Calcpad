using System;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class UpperTriangularMatrix : Matrix
    {
        internal UpperTriangularMatrix(int length) : base(length)
        {
            _type = MatrixType.UpperTriangular;
            _rows = new Vector[length];
            for (int i = length - 1; i >= 0; --i)
                _rows[i] = new LargeVector(length - i);
        }

        internal override RealValue this[int row, int col]
        {
            get => row <= col ? _rows[row][col - row] : RealValue.Zero;
            set
            {
                if (row <= col)
                    _rows[row][col - row] = value;
                else
                    Throw.IndexOutOfRangeException($"{row}, {col}");
            }
        }

        internal override Matrix Clone() => new UpperTriangularMatrix(_rowCount);

        internal UpperTriangularMatrix Resize(int n)
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

        internal Vector RawCol(int col)
        {
            var v = new LargeVector(col + 1);
            for (int i = col; i >= 0; --i)
                v[i] = _rows[i][col - i];
            return v;
        }

        internal override Matrix Transpose()
        {
            var L = new LowerTriangularMatrix(_rowCount);
            for (int i = _rowCount - 1; i >= 0; --i)
                L._rows[i] = RawCol(i);

            return L;
        }

        internal override RealValue Determinant()
        {
            var det = _rows[0][0];
            for (int i = 1; i < _rowCount; ++i)
                det *= _rows[i][0];

            return det;
        }

        protected override Matrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            indexes = new int[_rowCount];
            minPivot = double.MaxValue;
            det = 1d;
            for (int i = 0; i < _rowCount; ++i)
            {
                if (_rows[i][0].D == 0d)
                    return null;

                indexes[i] = i;
            }
            return RawCopy();
        }

        internal override Matrix Invert()
        {
            var U = new UpperTriangularMatrix(_rowCount);
            var n = _rowCount - 1;
            for (int i = n; i >= 0; --i)
            {
                var row = _rows[i];
                var v = row[0];
                if (v.D == 0)
                    Throw.MatrixSingularException();

                v = RealValue.One / v;
                U[i, i] = v;
                for (int j = n; j > i; --j)
                {
                    var sum = row[n - i] * U[n, j];
                    for (int k = n - 1; k > i; --k)
                        sum += row[k - i] * U[k, j];

                    U[i, j] = -sum * v;
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
                var row = _rows[i];
                for (int j = n; j > i; --j)
                    sum -= row[j - i] * x[j];

                x[i] = sum / row[0];
            }
            return x;
        }

        internal override Matrix MSolve(Matrix M)
        {
            var m = _rowCount;
            var n = M.ColCount;
            var v = new Vector[n];
            Parallel.For(0, n, j =>
                v[j] = LSolve(M.Col(j + 1))
            );
            return CreateFromCols(v, m);
        }

        private LowerTriangularMatrix RawCopy()
        {
            var n = _rows.Length;
            var M = new LowerTriangularMatrix(n);
            for (int i = n - 1; i >= 0; --i)
                M._rows[i] = _rows[i].Copy();

            return M;
        }
    }
}
