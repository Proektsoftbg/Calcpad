using System;

namespace Calcpad.Core
{
    internal class RowVector : Vector
    {
        private readonly Matrix _matrix;
        private readonly int _row;
        internal override RealValue this[int index]
        {
            get => _matrix[_row, index];
            set => _matrix[_row, index] = value;
        }

        internal RowVector(Matrix matrix, int row)
        {
            _matrix = matrix;
            _row = row;
            _size = matrix.ColCount;
        }

        internal override int Length => _size;

        internal override ref RealValue ValueByRef(int index)
        {
            if (_matrix is ColumnMatrix cm)
            {
                if (index != _row)
                    Throw.IndexOutOfRangeException((index + 1).ToString());

                return ref cm._rows[0].ValueByRef(_row);
            }

            if (_matrix is DiagonalMatrix dm)
            {
                if (index != _row)
                    Throw.IndexOutOfRangeException((index + 1).ToString());

                return ref dm._rows[index].ValueByRef(0);

            }
            if (_matrix is SymmetricMatrix sm)
            {
                if (index <= _row)
                    return ref sm._rows[index].ValueByRef(_row - index);

                return ref sm._rows[_row].ValueByRef(index - _row);
            }
            if (_matrix is LowerTriangularMatrix ltm)
            {
                if (index > _row)
                    Throw.IndexOutOfRangeException((index + 1).ToString());

                return ref ltm._rows[_row].ValueByRef(index);
            }
            if (_matrix is UpperTriangularMatrix utm)
            {
                if (index < _row)
                    Throw.IndexOutOfRangeException((index + 1).ToString());

                return ref utm._rows[_row].ValueByRef(index - _row);
            }
            return ref _matrix._rows[_row].ValueByRef(index);
        }

        internal override RealValue[] Values
        {
            get
            {
                var n = Length;
                var values = new RealValue[n];
                for (int j = n - 1; j >= 0; --j)
                    values[j] = _matrix[_row, j];

                return values;
            }
        }

        internal override Vector Fill(RealValue value)
        {
            for (int j = Length - 1; j >= 0; --j)
                _matrix[_row, j] = value;

            return this;
        }

        internal override Vector Resize(int newSize)
        {
            if (newSize == Length)
                return this;

            var vector = new Vector(newSize);
            var n = Math.Min(newSize, Length);
            for (int j = n - 1; j >= 0; --j)
                vector[j] = _matrix[_row, j];

            return vector;
        }
    }
}
