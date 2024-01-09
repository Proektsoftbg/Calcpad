using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Calcpad.Core
{
    internal class Vector : IEnumerable<Vector>
    {
        private readonly Value[] _items;
        public IEnumerator<Vector> GetEnumerator() =>
            (IEnumerator<Vector>)_items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _items.GetEnumerator();

        internal Value this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        internal int Count => _items.Length;

        internal Vector(Value[] items) => _items = items;
        internal Vector(int size) => _items = new Value[size];

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            for (int i = 0, n = _items.Length; i < n; ++i)
                hashCode.Add(_items[i]);

            return hashCode.ToHashCode();
        }

        public override bool Equals(object obj) => obj is Vector v && Equals(v);

        public static Vector operator -(Vector a)
        {
            Vector b = new(a.Count);
            for (int i = 0, n = a.Count; i < n; ++i)
                b[i] = -a[i];

            return b;
        }

        public static Vector operator +(Vector a, Vector b)
        {
            var n1 = Math.Min(a.Count, b.Count);
            var n2 = Math.Max(a.Count, b.Count);
            Vector c = new(n2);
            for (int i = 0; i < n1; ++i)
                c[i] = a[i] + b[i];

            if (a.Count > b.Count)
                Array.Copy(a._items, n1, c._items, n1, n2 - n1);
            else if (b.Count > a.Count)
                Array.Copy(b._items, n1, c._items, n1, n2 - n1);

            return c;
        }

        public static Vector operator -(Vector a, Vector b)
        {
            var n1 = Math.Min(a.Count, b.Count);
            var n2 = Math.Max(a.Count, b.Count);
            Vector c = new(n2);
            for (int i = 0; i < n1; ++i)
                c[i] = a[i] - b[i];

            if (a.Count > b.Count)
                Array.Copy(a._items, n1, c._items, n1, n2 - n1);
            else if (b.Count > a.Count)
                Array.Copy(b._items, n1, c._items, n1, n2 - n1);

            return c;
        }

        //Dot product
        public static Value operator *(Vector a, Vector b) => DotProduct(a, b);

        public static Value DotProduct(Vector a, Vector b)
        {
            var c = Value.Zero;
            for (int i = 0, n = Math.Min(a.Count, b.Count); i < n; ++i)
                c += a[i] * b[i];

            return c;
        }

        public static Vector CrossProduct(Vector a, Vector b)
        {
            var na = a.Count;
            var nb = b.Count;
            if (na < 2 || na > 3 || nb < 2 || nb > 3)
                throw new MathParser.MathParserException("Cross product is defined only for 2D and 3D vectors.");

            Vector c = new(3);
            c[2] = a[0] * b[1] - a[1] * b[0];
            if (na == 3)
            {
                c[0] = -a[2] * b[1];
                c[1] = a[2] * b[0];
            }
            if (nb == 3)
            {
                c[0] += a[1] * b[2];
                c[1] -= a[0] * b[2];
            }
            return c;
        }

        public static Value Multiply(Vector a, Vector b)
        {
            var c = Value.Zero;
            for (int i = 0, n = Math.Min(a.Count, b.Count); i < n; ++i)
                c += Value.Multiply(a[i], b[i]);

            return c;
        }

        public static Vector operator /(Vector a, Value b)
        {
            var n = a.Count;
            Vector c = new(n);
            for (int i = 0; i < n; ++i)
                c[i] = a[i] / b;

            return c;
        }

        public static Vector Divide(Vector a, Value b)
        {
            var n = a.Count;
            Vector c = new(n);
            for (int i = 0; i < n; ++i)
                c[i] = Value.Divide(a[i], b);

            return c;
        }

        public static Vector operator *(Vector a, Value b)
        {
            var n = a.Count;
            Vector c = new(n);
            for (int i = 0; i < n; ++i)
                c[i] = a[i] * b;

            return c;
        }

        public static Value operator ==(Vector a, Vector b) =>
            new(a.Equals(b) ? 1d : 0d);

        public static Value operator !=(Vector a, Vector b) =>
            new(a.Equals(b) ? 0d : 1d);

        public bool Equals(Vector other) =>
            _items.AsSpan().SequenceEqual(other._items);

        internal Vector Sort()
        {
            var n = Count;
            Vector b = new(n);
            Array.Copy(_items, b._items, n);
            Array.Sort(b._items);
            return b;
        }

        internal Vector Reverse()
        {
            var n = Count;
            Vector b = new(n);
            int n1 = n - 1;
            for (int i = 0; i < n; ++i)
                b[i] = _items[n1 - i];
            return b;
        }

        internal Vector First(int length)
        {
            if (length >= Count)
                return this;
            Vector b = new(length);
            Array.Copy(_items, b._items, length);
            return b;
        }

        internal Vector Last(int length)
        {
            if (length >= Count)
                return this;
            Vector b = new(length);
            Array.Copy(_items, Count - length, b._items, 0, length);
            return b;
        }

        internal Vector Slice(int start, int length)
        {
            var n = Math.Min(length, Count - start);
            Vector b = new(n);
            Array.Copy(_items, start, b._items, 0, n);
            return b;
        }
    }
}
