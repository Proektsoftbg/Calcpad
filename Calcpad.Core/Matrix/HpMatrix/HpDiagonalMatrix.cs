using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal class HpDiagonalMatrix : HpMatrix
    {
        internal HpDiagonalMatrix(int length, Unit units) : base(length, units)
        {
            _type = MatrixType.Diagonal;
            _hpRows = [new HpVector(length, units)];
            _rows = _hpRows;
        }

        internal HpDiagonalMatrix(int length, in RealValue value) : this(length, value.Units)
        {
            _hpRows[0].Fill(value);
        }

        internal HpDiagonalMatrix(Vector v) : base(v.Length, null)
        {
            _type = MatrixType.Diagonal;
            var hp_v = v is HpVector hp ? hp : new HpVector(v, null);
            _hpRows = [hp_v];
            _rows = _hpRows;
            _units = hp_v.Units;
        }

        internal HpDiagonalMatrix(DiagonalMatrix matrix) : this(matrix.Rows[0]) { }

        internal override RealValue this[int row, int col]
        {
            get => row.Equals(col) ? _hpRows[0][row] : new(0d, _units);
            set
            {
                if (row.Equals(col))
                    _hpRows[0][row] = value;
                else
                    throw Exceptions.IndexOutOfRange($"{row  + 1}, {col + 1}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override double GetValue(int row, int col) => row.Equals(col) ? _hpRows[0].GetValue(row) : 0d;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SetValue(double value, int row, int col)
        {
            if (row.Equals(col))
                _hpRows[0].SetValue(value, row);
            else
                throw Exceptions.IndexOutOfRange($"{row + 1}, {col + 1}");
        }
        internal override HpMatrix Clone() => new HpDiagonalMatrix(_rowCount, _units);
        internal override Matrix CloneAsMatrix() => new DiagonalMatrix(_rowCount);

        private new HpDiagonalMatrix RawCopy()
        {
            var M = new HpDiagonalMatrix(_rowCount, _units);
            M._hpRows[0] = _hpRows[0].Copy();
            return M;
        }

        internal HpDiagonalMatrix Resize(int n)
        {
            if (n != _rowCount)
            {
                _hpRows[0].Resize(n);
                _rowCount = n;
                _colCount = n;
            }
            return this;
        }

        public static HpMatrix operator *(HpDiagonalMatrix a, HpDiagonalMatrix b) =>
            new HpDiagonalMatrix(a._hpRows[0] * b._hpRows[0]);


        public static HpLowerTriangularMatrix operator *(HpDiagonalMatrix a, HpLowerTriangularMatrix b)
        {
            var n = b.RowCount;
            Unit unit = Unit.Multiply(a.Units, b.Units, out _, true);
            var c = new HpLowerTriangularMatrix(b.RowCount, unit);
            var diag = a._hpRows[0];
            if (n > diag.Size)
                n = diag.Size;

            for (int i = n - 1; i >= 0; --i)
                c.HpRows[i] = b.HpRows[i] * diag[i];

            return c;
        }
        public static HpUpperTriangularMatrix operator *(HpDiagonalMatrix a, HpUpperTriangularMatrix b)
        {
            var n = b.RowCount;
            Unit unit = Unit.Multiply(a.Units, b.Units, out _, true);
            var c = new HpUpperTriangularMatrix(b.RowCount, unit);
            var diag = a._hpRows[0];
            if (n > diag.Size)
                n = diag.Size;

            for (int i = n - 1; i >= 0; --i)
                c.HpRows[i] = b.HpRows[i] * diag[i];

            return c;
        }

        public static HpMatrix operator *(HpDiagonalMatrix a, HpSymmetricMatrix b)
        {
            var n = b.RowCount;
            Unit unit = Unit.Multiply(a.Units, b.Units, out var d, true);
            var c = new HpMatrix(n, n, unit);
            var diag = a._hpRows[0];
            if (n > diag.Size)
                n = diag.Size;

            for (int i = n - 1; i >= 0; --i)
            {
                var cRow = c.HpRows[i];
                var k = diag[i].D * d;
                for (int j = n - 1; j >= 0; --j)
                    cRow.SetValue(b.GetValue(i, j) * k, j);
            }
            return c;
        }

        public static HpMatrix operator *(HpDiagonalMatrix a, HpMatrix b)
        {
            var n = b.RowCount;
            Unit unit = Unit.Multiply(a.Units, b.Units, out _, true);
            var c = new HpMatrix(n, b.ColCount, unit);
            var diag = a._hpRows[0];
            if (n > diag.Size)
                n = diag.Size;

            for (int i = n - 1; i >= 0; --i)
                c.HpRows[i] = b.HpRows[i] * diag[i];

            return c;
        }

        public static HpLowerTriangularMatrix operator *(HpLowerTriangularMatrix a, HpDiagonalMatrix b)
        {
            var n = a.RowCount;
            Unit unit = Unit.Multiply(a.Units, b.Units, out _, true);
            var c = new HpLowerTriangularMatrix(n, unit);
            var diag = b._hpRows[0];
            for (int i = n - 1; i >= 0; --i)
                c.HpRows[i] = a.HpRows[i] * diag;

            return c;
        }

        public static HpUpperTriangularMatrix operator *(HpUpperTriangularMatrix a, HpDiagonalMatrix b)
        {
            var n = a.RowCount;
            Unit unit = Unit.Multiply(a.Units, b.Units, out var d, true);
            var c = new HpUpperTriangularMatrix(n, unit);
            var diag = b._hpRows[0].Raw;
            var diagSize = b._hpRows[0].Size;
            for (int i = n - 1; i >= 0; --i)
            {
                var aRow = a.HpRows[i];
                var len = aRow.Length;
                var aRaw = aRow.Raw;
                var size = Math.Min(aRow.Size, diagSize - i);
                c.HpRows[i] = new(len, size, unit);
                var cRaw = c.HpRows[i].Raw;
                for (int j = size - 1; j >= 0; --j)
                    cRaw[j] = aRaw[j] * diag[i + j] * d;
            }
            return c;
        }

        public static HpMatrix operator *(HpSymmetricMatrix a, HpDiagonalMatrix b)
        {
            var n = a.RowCount;
            Unit unit = Unit.Multiply(a.Units, b.Units, out var d, true);
            var c = new HpMatrix(n, n, unit);
            var diag = b._hpRows[0].Raw;
            var diagSize = b._hpRows[0].Size;
            for (int i = n - 1; i >= 0; --i)
            {
                var cRow = c.HpRows[i];
                for (int j = diagSize - 1; j >= 0; --j)
                    cRow.SetValue(a.GetValue(i, j) * diag[j] * d, j);
            }
            return c;

        }

        public static HpMatrix operator *(HpMatrix a, HpDiagonalMatrix b)
        {
            Unit unit = Unit.Multiply(a.Units, b.Units, out _, true);
            var c = new HpMatrix(a.RowCount, a.ColCount, unit);
            var diag = b._hpRows[0];
            for (int i = a.RowCount - 1; i >= 0; --i)
                c.HpRows[i] = a.HpRows[i] * diag;

            return c;
        }

        internal override HpDiagonalMatrix Transpose() => RawCopy();

        internal override RealValue Determinant() => _hpRows[0].Product();

        protected override HpMatrix GetLU(out int[] indexes, out double minPivot, out double det)
        {
            indexes = new int[_rowCount];
            minPivot = double.MaxValue;
            det = 1d;
            for (int i = 0; i < _rowCount; ++i)
            {
                if (_hpRows[0].GetValue(i) == 0d)
                    return null;

                indexes[i] = i;
            }
            return RawCopy();
        }

        internal HpUpperTriangularMatrix CholeskyDecomposition()
        {
            var U = new HpUpperTriangularMatrix(_rowCount, _units);
            for (int i = 0; i < _rowCount; ++i)
                U.SetValue(_hpRows[0].GetValue(i), i, i);

            return U;
        }

        internal override HpMatrix Invert()
        {
            CheckSingular();
            return new HpDiagonalMatrix(RealValue.One / _hpRows[0]);
        }

        internal override HpVector LSolve(HpVector vector)
        {
            CheckSingular();
            return vector / _hpRows[0];
        }

        internal override HpMatrix MSolve(HpMatrix M)
        {
            CheckSingular();
            var m = _rowCount;
            var n = M.ColCount;
            var result = new HpVector[n];
            Parallel.For(0, n, j => {
                result[j] = M.Col(j + 1) / _hpRows[0];
            });
            return CreateFromCols(result, m);
        }

        private void CheckSingular()
        {
            for (int i = _rowCount - 1; i >= 0; --i)
                if (_hpRows[0].GetValue(i) == 0d)
                    throw Exceptions.MatrixSingular();
        }
        internal override RealValue L1Norm() => _hpRows[0].InfNorm();


        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm() => _hpRows[0].InfNorm();
        internal HpVector EigenValues(int count)
        {
            var reverse = CheckCount(ref count);
            return _hpRows[0].Sort(reverse).First(count);
        }
        internal HpMatrix EigenVectors(int count)
        {
            var reverse = CheckCount(ref count);
            var indexes = _hpRows[0].GetOrderIndexes(reverse);
            HpMatrix M = new(count, _rowCount, null);
            for (int i = 0; i < count; ++i)
                M.SetValue(1d, indexes[i], i);

            return M;
        }

        internal Matrix Eigen(int count)
        {
            var reverse = CheckCount(ref count);
            var indexes = _hpRows[0].GetOrderIndexes(reverse);
            Matrix M = new(count, _rowCount + 1);
            for (int i = 0; i < count; ++i)
            {
                M[indexes[i], 0] = _hpRows[0][i]; 
                M[i, indexes[i] + 1] = RealValue.One;
            }
            return M;
        }

        protected override RealValue Condition(Func<HpMatrix, RealValue> norm)
        {
            var det = Determinant();
            if (det.D == 0d)
                return RealValue.PositiveInfinity;

            var M = Invert();
            return norm(this) * norm(M);
        }

        internal override HpMatrix Adjoint()
        {
            var det = Determinant();
            if (det.D == 0)
                throw Exceptions.MatrixSingular();

            var M = Invert();
            return M * det;
        }
    }
}
