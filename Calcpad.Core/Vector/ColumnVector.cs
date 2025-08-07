using System;

namespace Calcpad.Core
{
    internal class ColumnVector : Vector
    {
        private readonly Matrix _matrix;
        private readonly int _col;

        internal override RealValue this[int index]
        {
            get => _matrix[index, _col];
            set => _matrix[index, _col] = value;
        }

        internal ColumnVector(Matrix matrix, int column)
        {
            _matrix = matrix;
            _col = column;
            _size = matrix.RowCount;
        }


        internal override int Length => _size;

        internal override ref RealValue ValueByRef(int index)
        {
            switch (_matrix)
            {
                case ColumnMatrix cm:
                    return ref cm.Rows[0].ValueByRef(index);
                case DiagonalMatrix dm:
                    if (index != _col)
                        throw Exceptions.IndexOutOfRange(index.ToString());

                    return ref dm.Rows[index].ValueByRef(0);
                case SymmetricMatrix sm:
                    if (index <= _col)
                        return ref sm.Rows[index].ValueByRef(_col - index);

                    return ref sm.Rows[_col].ValueByRef(index - _col);
                case LowerTriangularMatrix ltm:
                    if (index < _col)
                        throw Exceptions.IndexOutOfRange(index.ToString());

                    return ref ltm.Rows[index].ValueByRef(_col);
                case UpperTriangularMatrix utm:
                    if (index > _col)
                        throw Exceptions.IndexOutOfRange(index.ToString());

                    return ref utm.Rows[index].ValueByRef(_col - index);
                default:
                    return ref _matrix.Rows[index].ValueByRef(_col);
            }
        }

        internal override RealValue[] Values
        {
            get
            {
                var m = Length;
                var values = new RealValue[m];
                for (int i = m - 1; i >= 0; --i)
                    values[i] = _matrix[i, _col];

                return values;
            }
        }

        internal override Vector Fill(RealValue value)
        {
            for (int i = Length - 1; i >= 0; --i)
                _matrix[i, _col] = value;

            return this;
        }

        internal override Vector Resize(int newSize)
        {
            if (newSize == Length)
                return this;

            var vector = new Vector(newSize);
            var m = Math.Min(newSize, Length);
            for (int i = m - 1; i >= 0; --i)
                vector[i] = _matrix[i, _col];

            return vector;
        }
    }
}