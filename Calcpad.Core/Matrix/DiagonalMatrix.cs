using System;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class DiagonalMatrix : Matrix
    {
        internal DiagonalMatrix(int length) : base(length)
        {
            _type = MatrixType.Diagonal;
            _rows = [new LargeVector(length)];
        }

        internal DiagonalMatrix(int length, in RealValue value) : this(length)
        {
            _rows[0].Fill(value);
        }

        internal DiagonalMatrix(Vector a) : base(a.Length)
        {
            _type = MatrixType.Diagonal;
            _rows = [a];
        }

        internal override RealValue this[int row, int col]
        {
            get => row.Equals(col) ? _rows[0][row] : RealValue.Zero;
            set
            {
                if (row.Equals(col))
                    _rows[0][row] = value;
                else
                    Throw.IndexOutOfRangeException($"{row  + 1}, {col + 1}");
            }
        }
        internal override Matrix Clone() => new DiagonalMatrix(_rowCount);

        private DiagonalMatrix RawCopy()
        {
            var n = _rows.Length;
            var M = new DiagonalMatrix(n);
            M._rows[0] = _rows[0].Copy();
            return M;
        }

        internal DiagonalMatrix Resize(int n)
        {
            if (n != _rowCount)
            {
                _rows[0].Resize(n);
                _rowCount = n;
                _colCount = n;
            }
            return this;
        }

        public static Matrix operator *(DiagonalMatrix a, DiagonalMatrix b) =>
            new DiagonalMatrix(a._rows[0] * b._rows[0]);

        public static Matrix operator *(DiagonalMatrix a, Matrix b)
        {
            var c = b.Clone();
            var diag = a._rows[0];
            for (int i = a._rowCount - 1; i >= 0; --i)
                c._rows[i] = b._rows[i] * diag[i];

            return c;
        }

        public static Matrix operator *(Matrix a, DiagonalMatrix b)
        {
            var c = a.Clone();
            var diag = b._rows[0];
            for (int i = a.RowCount - 1; i >= 0; --i)
                c._rows[i] = a._rows[i] * diag;

            return c;
        }

        internal override DiagonalMatrix Transpose() => RawCopy();

        internal override RealValue Determinant()
        {
            var diag = _rows[0];
            var det = diag[0];
            for (int i = 1; i < _rowCount; ++i)
                det *= diag[i];

            return det;
        }

        protected override Matrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            indexes = new int[_rowCount];
            minPivot = double.MaxValue;
            det = 1d;
            var diag = _rows[0];
            for (int i = 0; i < _rowCount; ++i)
            {
                if (diag[i].D == 0d)
                    return null;

                indexes[i] = i;
            }
            return RawCopy();
        }

        internal UpperTriangularMatrix CholeskyDecomposition()
        {
            var U = new UpperTriangularMatrix(_rowCount);
            var row = _rows[0];
            for (int i = 0; i < _rowCount; ++i)
                U[i, i] = row[i];

            return U;
        }

        internal override Matrix Invert()
        {
            CheckSingular();
            var M = new DiagonalMatrix(_rowCount);
            M._rows[0] = RealValue.One / _rows[0];
            return M;
        }

        internal override Vector LSolve(Vector v)
        {
            CheckSingular();
            return v / _rows[0];
        }

        internal override Matrix MSolve(Matrix M)
        {
            CheckSingular();
            var m = _rowCount;
            var n = M.ColCount;
            var v = new Vector[n];
            Parallel.For(0, n, j => 
                v[j] = M.Col(j) / _rows[0]
            );
            return CreateFromCols(v, m);
        }

        private void CheckSingular()
        {
            var diag = _rows[0];
            for (int i = _rowCount - 1; i >= 0; --i)
                if (diag[i].D == 0)
                    Throw.MatrixSingularException();
        }

        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm() => _rows[0].InfNorm();

        internal Vector EigenValues() => _rows[0].Sort();

        internal Matrix EigenVectors()
        {
            var indexes = _rows[0].GetOrderIndexes(false);
            Matrix M = new(_rowCount, _rowCount);
            for (int i = 0; i < _rowCount; ++i)
                M[indexes[i], i] = RealValue.One;

            return M;
        }

        internal Matrix Eigen()
        {
            var indexes = _rows[0].GetOrderIndexes(false);
            Matrix M = new(_rowCount, _rowCount + 1);
            for (int i = 0; i < _rowCount; ++i)
            {
                M[indexes[i], 0] = _rows[0][i]; 
                M[i, indexes[i] + 1] = RealValue.One;
            }
            return M;
        }

        protected override RealValue Condition(Func<Matrix, RealValue> norm)
        {
            var det = Determinant();
            if (det.D == 0)
                return RealValue.PositiveInfinity;

            var M = Invert();
            return norm(this) * norm(M);
        }

        internal override Matrix Adjoint()
        {
            var det = Determinant();
            if (det.D == 0)
                Throw.MatrixSingularException();

            var M = Invert();
            return M * det;
        }
    }
}
