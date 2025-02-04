namespace Calcpad.Core
{
    internal class ColumnMatrix : Matrix
    {
        internal ColumnMatrix(int length)
        {
            if (length > MaxSize)
                Throw.MatrixDimensionsException();

            _type = MatrixType.Column;
            _rowCount = length;
            _colCount = 1;
            _rows = [new LargeVector(length)];
        }

        internal ColumnMatrix(int length, RealValue value) : this(length)
        {
            _rows[0].Fill(value);
        }

        internal ColumnMatrix(Vector a)
        {
            if (a.Length > MaxSize)
                Throw.MatrixDimensionsException();

            _type = MatrixType.Column;
            _rowCount = a.Length;
            _colCount = 1;
            _rows = [a];
        }

        internal override RealValue this[int row, int col]
        {
            get => _rows[0][row];
            set => _rows[0][row] = value;
        }

        internal override Matrix Clone() => new ColumnMatrix(_rowCount);

        internal ColumnMatrix Resize(int n)
        {
            if (n != _rowCount)
            {
                _rows[0].Resize(n);
                _rowCount = n;
            }
            return this;
        }

        public static ColumnMatrix operator *(Matrix a, ColumnMatrix b)
        {
            ColumnMatrix c;
            if (a is ColumnMatrix ac)
            {
                if (ac._rowCount != b._rowCount)
                    Throw.MatrixDimensionsException();

                c = new ColumnMatrix(ac._rows[0] * b._rows[0]);
            }
            else
            {
                var m = a.RowCount;
                c = new ColumnMatrix(m);
                var cr = c._rows[0];
                var br = b._rows[0];
                for (int i = m - 1; i >= 0; --i)
                {
                    var row = new RowVector(a, i);
                    cr[i] = Vector.DotProduct(row, br);
                }
            }
            return c;
        }

        internal override Matrix Transpose()
        {
            var c = new Matrix(1, _rowCount);
            c._rows[0] = _rows[0];
            return c;
        }

        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm() => _rows[0].InfNorm();
    }
}
