using DocumentFormat.OpenXml.Spreadsheet;

namespace Calcpad.Core
{
    internal class ColumnMatrix : Matrix
    {
        internal ColumnMatrix(int length)
        {
            if (length > MaxSize)
                throw Exceptions.MatrixDimensions();

            _type = MatrixType.Column;
            _rowCount = length;
            _colCount = 1;
            _rows = [new LargeVector(length)];
        }

        internal ColumnMatrix(int length, RealValue value) : this(length)
        {
            _rows[0].Fill(value);
        }

        internal ColumnMatrix(Vector v)
        {
            if (v.Length > MaxSize)
                throw Exceptions.MatrixDimensions();

            _type = MatrixType.Column;
            _rowCount = v.Length;
            _colCount = 1;
            _rows = v is LargeVector lv ? [lv] : [new LargeVector(v.Raw)];
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
                    throw Exceptions.MatrixDimensions();

                c = new ColumnMatrix(ac._rows[0] * b._rows[0]);
            }
            else
            {
                var m = a.RowCount;
                c = new ColumnMatrix(m);
                var cr = c._rows[0];
                var br = b._rows[0];
                if (a.Type == MatrixType.Full || a.Type == MatrixType.LowerTriangular)
                    for (int i = m - 1; i >= 0; --i)
                        cr[i] = Vector.DotProduct(a.Rows[i], br);
                else
                    for (int i = m - 1; i >= 0; --i)
                        cr[i] = Vector.DotProduct(new RowVector(a, i), br);
            }
            return c;
        }

        internal override Matrix Transpose()
        {
            var c = new Matrix(1, _rowCount);
            c.Rows[0] = _rows[0].Copy();
            return c;
        }

        //L∞ (Infinity) or Chebyshev norm     
        internal override RealValue InfNorm() => _rows[0].InfNorm();

        internal Vector ColRef() => _rows[0];
    }
}
