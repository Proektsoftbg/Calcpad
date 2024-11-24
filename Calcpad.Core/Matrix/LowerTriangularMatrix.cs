using System;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class LowerTriangularMatrix : Matrix
    {
        internal LowerTriangularMatrix(int length) : base(length)
        {
            _type = MatrixType.LowerTriangular;
            _rows = new Vector[length];
            for (int i = 0; i < length; ++i)
                _rows[i] = new LargeVector(i + 1);
        }

        internal override Value this[int row, int col]
        {
            get => row >= col ? _rows[row][col] : Value.Zero;
            set
            {
                if (row >= col)
                    _rows[row][col] = value;
                else
                    Throw.IndexOutOfRangeException($"{row + 1}, {col + 1}");
            }
        }
        internal override Matrix Clone() => new LowerTriangularMatrix(_rowCount);

        internal LowerTriangularMatrix Resize(int n)
        {
            if (n != _rowCount)
            {
                Array.Resize(ref _rows, n);
                for (int i = _rowCount; i < n; ++i)
                    _rows[i] = new LargeVector(i + 1);

                _rowCount = n;
                _colCount = n;
            }
            return this;
        }

        internal Vector RawCol(int col)
        {
            var n = _colCount - col;
            var v = new LargeVector(n);
            for (int i = n - 1; i >= 0; --i)
                v[i] = _rows[col + i][col];
            return v;
        }

        internal override Matrix Transpose()
        {
            var U = new UpperTriangularMatrix(_rowCount);
            for (int i = _rowCount - 1; i >= 0; --i)
                U._rows[i] = RawCol(i);

            return U;
        }

        internal override Value Determinant()
        {
            var det = _rows[0][0];
            for (int i = 1; i < _rowCount; ++i)
                det *= _rows[i][i];

            return det;
        }

        protected override Matrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            indexes = new int[_rowCount];
            minPivot = double.MaxValue;
            det = 1d;
            for (int i = 0; i < _rowCount; ++i)
            {
                if (_rows[i][i].Re == 0d)
                    return null;

                indexes[i] = i;
            }
            return RawCopy();
        }

        internal override Matrix Invert()
        {
            var L = new LowerTriangularMatrix(_rowCount);
            for (int i = 0; i < _rowCount; ++i)
            {
                var row = _rows[i];
                var v = row[i];
                if (v.Re == 0)
                    Throw.MatrixSingularException();

                v = Value.One / v;
                L[i, i] = v;
                for (int j = 0; j < i; ++j)
                {
                    var sum = row[j] * L[j, j];
                    for (int k = j + 1; k < i; ++k)
                        sum += row[k] * L[k, j];

                    L[i, j] = -sum * v;
                }
            }
            return L;
        }

        internal override Vector LSolve(Vector v)
        {
            Vector x = new(_rowCount);
            for (int i = 0; i < _rowCount; ++i)
            {
                var sum = v[i];
                var row = _rows[i];
                for (int j = 0; j < i; ++j)
                    sum -= row[j] * x[j];

                var ri = row[i];
                if (ri.Re == 0)
                    Throw.MatrixSingularException();

                x[i] = sum / ri;
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
