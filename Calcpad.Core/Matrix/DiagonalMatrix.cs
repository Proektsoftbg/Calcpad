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
            _rows = a is LargeVector lv ? [lv] : [new LargeVector(a.Raw)];
        }

        internal override RealValue this[int row, int col]
        {
            get => row.Equals(col) ? _rows[0][row] : RealValue.Zero;
            set
            {
                if (row.Equals(col))
                    _rows[0][row] = value;
                else
                    throw Exceptions.IndexOutOfRange($"{row + 1}, {col + 1}");
            }
        }
        internal override Matrix Clone() => new DiagonalMatrix(_rowCount);

        private DiagonalMatrix RawCopy()
        {
            var M = new DiagonalMatrix(_rowCount);
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

        public static DiagonalMatrix operator *(DiagonalMatrix a, DiagonalMatrix b) =>
            new(a._rows[0] * b._rows[0]);


        public static LowerTriangularMatrix operator *(DiagonalMatrix a, LowerTriangularMatrix b)
        {
            var n = b.RowCount;
            var c = new LowerTriangularMatrix(n);
            var diag = a._rows[0];
            if (n > diag.Size)
                n = diag.Size;

            for (int i = n - 1; i >= 0; --i)
                c.Rows[i] = b.Rows[i] * diag[i] as LargeVector;

            return c;
        }

        public static UpperTriangularMatrix operator *(DiagonalMatrix a, UpperTriangularMatrix b)
        {
            var n = b.RowCount;
            var c = new UpperTriangularMatrix(n);
            var diag = a._rows[0];
            if (n > diag.Size)
                n = diag.Size;

            for (int i = n - 1; i >= 0; --i)
                c.Rows[i] = b.Rows[i] * diag[i] as LargeVector;

            return c;
        }

        public static Matrix operator *(DiagonalMatrix a, SymmetricMatrix b)
        {
            var n = b.RowCount;
            var c = new Matrix(n, n);
            var diag = a._rows[0];
            if (n > diag.Size)
                n = diag.Size;

            for (int i = n - 1; i >= 0; --i)
            {
                var cRow = new RowVector(b, i) * diag[i];
                c.Rows[i] = new LargeVector(cRow.Values);
            }            
            return c;
        }

        public static Matrix operator *(DiagonalMatrix a, Matrix b)
        {
            var n = b.RowCount;
            var c = new Matrix(n, b.ColCount);
            var diag = a._rows[0];
            if (n > diag.Size)
                n = diag.Size;

            if (b.Type == MatrixType.Full)
                for (int i =n - 1; i >= 0; --i)
                    c.Rows[i] = b.Rows[i] * diag[i] as LargeVector;
            else
                for (int i = n - 1; i >= 0; --i)
                {
                    var cRow = new RowVector(b, i) * diag[i];
                    c.Rows[i] = new LargeVector(cRow.Values);
                }

            return c;
        }

        public static LowerTriangularMatrix operator *(LowerTriangularMatrix a, DiagonalMatrix b)
        {
            var n = a.RowCount;
            var c = new LowerTriangularMatrix(n);
            var diag = b._rows[0];
            for (int i = n - 1; i >= 0; --i)
                c.Rows[i] = a.Rows[i] * diag as LargeVector;

            return c;
        }

        public static UpperTriangularMatrix operator *(UpperTriangularMatrix a, DiagonalMatrix b)
        {
            var n = a.RowCount;
            var c = new UpperTriangularMatrix(n);
            var diag = b._rows[0].Raw;
            var diagSize = b._rows[0].Size;
            for (int i = n - 1; i >= 0; --i)
            {
                var aRow = a.Rows[i];
                var len = aRow.Length;
                var aRaw = aRow.Raw;
                var size = Math.Min(aRow.Size, diagSize - i); 
                var cRaw = new RealValue[len];
                for (int j = size - 1; j >= 0; --j)
                    cRaw[j] = aRaw[j] * diag[i + j];

                c.Rows[i] = new LargeVector(cRaw);
            }
            return c;
        }

        public static Matrix operator *(SymmetricMatrix a, DiagonalMatrix b)
        {
            var n = a.RowCount;
            var c = new Matrix(n, n);
            var diag = b._rows[0];
            for (int i = n - 1; i >= 0; --i)
            {
                var cRow = new RowVector(a, i) * diag;
                c.Rows[i] = new LargeVector(cRow.Values);
            }
            return c;
        }

        public static Matrix operator *(Matrix a, DiagonalMatrix b)
        {
            var c = new Matrix(a.RowCount, a.ColCount);
            var diag = b._rows[0];
            if (a.Type == MatrixType.Full)
                for (int i = a.RowCount - 1; i >= 0; --i)
                    c.Rows[i] = a.Rows[i] * diag as LargeVector;
            else
                for (int i = a.RowCount - 1; i >= 0; --i)
                {
                    var cRow = new RowVector(a, i) * diag;
                    c.Rows[i] = new LargeVector(cRow.Values);
                }

            return c;
        }

        internal override DiagonalMatrix Transpose() => RawCopy();

        internal override RealValue Determinant() => _rows[0].Product();

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
            M._rows[0] = RealValue.One / _rows[0] as LargeVector;
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
                v[j] = M.Col(j + 1) / _rows[0]
            );
            return CreateFromCols(v, m);
        }

        private void CheckSingular()
        {
            var diag = _rows[0];
            for (int i = _rowCount - 1; i >= 0; --i)
                if (diag[i].D == 0)
                    throw Exceptions.MatrixSingular();
        }

        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm() => _rows[0].InfNorm();

        internal Vector EigenValues(int count)
        {
            var reverse = CheckCount(ref count);
            return _rows[0].Sort(reverse).First(count);
        }

        internal Matrix EigenVectors(int count)
        {
            var reverse = CheckCount(ref count);
            var indexes = _rows[0].GetOrderIndexes(false);
            Matrix M = new(count, _rowCount);
            for (int i = 0; i < count; ++i)
                M[indexes[i], i] = RealValue.One;

            return M;
        }

        internal Matrix Eigen(int count)
        {
            var reverse = CheckCount(ref count);
            var indexes = _rows[0].GetOrderIndexes(reverse);
            Matrix M = new(count, _rowCount + 1);
            for (int i = 0; i < count; ++i)
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
                throw Exceptions.MatrixSingular();

            var M = Invert();
            return M * det;
        }
    }
}
