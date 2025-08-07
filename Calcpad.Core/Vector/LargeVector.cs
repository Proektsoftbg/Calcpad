using System;

namespace Calcpad.Core
{
    internal class LargeVector : Vector
    {
        private int _capacity;
        private int _length;

        internal override RealValue this[int index]
        {
            get
            {
                if (index >= _size)
                    return RealValue.Zero;

                return _values[index];
            }
            set
            {
                if (index >= _size)
                {
                    if (value.Equals(RealValue.Zero))
                        return;

                    _size = index + 1;
                    if (index >= _capacity)
                        ExpandCapacity(index);
                }
                _values[index] = value;
            }
        }

        internal override RealValue[] Values
        {
            get
            {
                if (_capacity == _length)
                    return _values;

                var values = new RealValue[_length];
                _values.AsSpan().CopyTo(values);
                return values;
            }
        }

        private void ExpandCapacity(int index)
        {
            _capacity = 125 * index / 100 + 1;
            if (_capacity > _length)
                _capacity = _length;

            if (_values is null)
            {
                if (_length <= 10)
                    _capacity = _length;
                else if (_capacity < _length / 100)
                    _capacity = _length / 100;

                _values = new RealValue[_capacity];
            }
            else
                Array.Resize(ref _values, _capacity);
        }

        internal override int Length => _length;

        internal override ref RealValue ValueByRef(int index)
        {
            if (index >= _size)
            {
                _size = index + 1;
                if (index >= _capacity)
                    ExpandCapacity(index);
            }
            return ref _values[index];
        }
        protected LargeVector() { }

        internal LargeVector(RealValue[] values)
        {
            if (values.Length > MaxLength)
                throw Exceptions.VectorSizeLimit();

            _values = values;
            _length = _values.Length;
            for (int i = _length - 1; i >= 0; --i)
            {
                if (!_values[i].Equals(RealValue.Zero))
                {
                    _size = i + 1;
                    return;
                }
            }
        }

        internal LargeVector(int length)
        {
            if (length > MaxLength)
                throw Exceptions.VectorSizeLimit();

            _length = length;
        }

        internal LargeVector(int length, int size)
        {
            if (length > MaxLength)
                throw Exceptions.VectorSizeLimit();

            _length = length;
            _size = size;
            _capacity = size;
            _values = new RealValue[size];
        }

        internal override LargeVector Copy()
        {
            var vector = new LargeVector(_length, _size);
            _values.AsSpan(0, _size).CopyTo(vector._values);
            return vector;
        }

        internal override LargeVector Resize(int newSize)
        {
            if (newSize > MaxLength)
                throw Exceptions.VectorSizeLimit();

            if (newSize != _length)
            {
                _length = newSize;
                if (_size > _length)
                    _size = _length;

                if (_capacity > _length)
                {
                    _capacity = _length;
                    Array.Resize(ref _values, _length); //Shink the vector if necessary
                }
            }
            return this;
        }

        internal override LargeVector Slice(int start, int end)
        {
            if (CheckBounds(ref start, ref end))
                return Copy();

            var length = end - start;
            if (end > _size) end = _size;
            LargeVector vector = new(length, end - start);
            if (start < end)
                _values.AsSpan(start, end - start).CopyTo(vector._values);

            return vector;
        }


        internal override Vector Reverse()
        {
            var vector = new LargeVector(_length);
            var start = 0;
            while (start < _size)
            {
                if (!_values[start].Equals(RealValue.Zero))
                    break;

                ++start;
            }
            for (int i = start; i < _size; ++i)
                vector[_length - i - 1] = _values[i];

            return vector;
        }

        internal override Vector Fill(RealValue value)
        {
            if (value.Equals(RealValue.Zero))
            {
                _size = 0;
                _capacity = 0;
                _values = [];
                return this;
            }
            if (_size < _length)
            {
                _size = _length;
                if (_capacity < _length)
                {
                    _capacity = _length;
                    _values = new RealValue[_capacity];
                }
            }
            _values.AsSpan().Fill(value);
            return this;
        }

        protected override void Fill(RealValue value, int start, int len)
        {
            var end = start + len;
            if (_size < end)
                _size = end;

            if (_capacity < end)
            {
                _capacity = end;
                Array.Resize(ref _values, end);
            }
            _values.AsSpan(start, len).Fill(value);
        }
    }
}
