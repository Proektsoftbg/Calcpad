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

        public static HpMatrix operator *(HpDiagonalMatrix a, HpMatrix b)
        {
            var c = b.Clone();
            var diag = a._hpRows[0];
            for (int i = a._rowCount - 1; i >= 0; --i)
                c.HpRows[i] = b.HpRows[i] * diag[i];

            return c;
        }

        public static HpMatrix operator *(HpMatrix a, HpDiagonalMatrix b)
        {
            var c = a.Clone();
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

        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm() => _hpRows[0].InfNorm();
        internal HpVector EigenValues(int count) => _hpRows[0].Sort().First(count);
        internal HpMatrix EigenVectors(int count)
        {
            var indexes = _hpRows[0].GetOrderIndexes(false);
            HpMatrix M = new(count, _rowCount, null);
            for (int i = 0; i < count; ++i)
                M.SetValue(1d, indexes[i], i);

            return M;
        }

        internal Matrix Eigen(int count)
        {
            var indexes = _hpRows[0].GetOrderIndexes(false);
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
