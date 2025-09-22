using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SN = System.Numerics;

namespace Calcpad.Core
{
    internal class HpVector : LargeVector, IEquatable<HpVector>
    {
        private int _capacity;
        private int _length;
        private new double[] _values;
        private static readonly int _vecSize = SN.Vector<double>.Count;
        internal Unit Units;
        internal override RealValue this[int index]
        {
            get
            {
                if (index >= _size)
                    return new(0d, Units);

                return new(_values[index], Units);
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
                var d = Unit.Convert(Units, value.Units, '.');
                if (d == 1d)
                    _values[index] = value.D;
                else
                    _values[index] = value.D * d;
            }
        }

        internal override int Length => _length;
        internal override RealValue[] Values
        {
            get
            {
                var values = new RealValue[_length];
                for (int i = 0; i < _size; ++i)
                    values[i] = new(_values[i], Units); 

                if (Units is not null)
                {
                    var zero = new RealValue(0d, Units);
                    for (int i = _size + 1; i < _length; ++i)
                        values[i] = zero;
                }
                return values;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal double GetValue(int index) => 
            index >= _size ? 0d : _values[index];


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetValue(double value, int index)
        {
            if (index >= _size)
            {
                if (value == 0d)
                    return;

                _size = index + 1;
                if (index >= _capacity)
                    ExpandCapacity(index);
            }
            _values[index] = value;
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(_length);
            for (int i = 0; i < _size; ++i)
            {
                hashCode.Add(_values[i]);
                if (Units is not null)
                    hashCode.Add(Units);
            }
            if (Units is not null)
                for (int i = _size; i < _length; ++i)
                    hashCode.Add(Units);

            return hashCode.ToHashCode();
        }

        internal new double[] Raw => _values;

        private void ExpandCapacity(int index)
        {
            _capacity = 125 * index / 100 + 1;
            if (_capacity > _length)
                _capacity = _length;

            if (_values is null)
            {
                if (_length <= 10)
                    _capacity = _length;
                else if (index == 0 && _capacity < _length / 100)
                    _capacity = _length / 100;

                _values = new double[_capacity];
            }
            else
                Array.Resize(ref _values, _capacity);
        }

        internal HpVector(double[] values, Unit units)
        {
            if (values.Length > MaxLength)
                throw Exceptions.VectorSizeLimit();

            Units = units;  
            _values = values;
            _length = _values.Length;
            for (int i = _length - 1; i >= 0; --i)
            {
                if (_values[i] != 0d)
                {
                    _size = i + 1;
                    return;
                }
            }
        }

        internal HpVector(RealValue[] values)
        {
            if (values.Length > MaxLength)
                throw Exceptions.VectorSizeLimit();

            _length = values.Length;
            if (_length == 0)
            {
                Units = null;
                return;
            }
            Units = values[0].Units;
            for (int i = _length - 1; i >= 0; --i)
                this[i] = values[i];
        }

        internal HpVector(int length, Unit units)
        {
            if (length > MaxLength)
                throw Exceptions.VectorSizeLimit();

            Units = units;
            _length = length;
        }

        internal HpVector(int length, int size, Unit units)
        {
            Units = units;
            _length = length;
            _capacity = _size = size;
            _values = new double[size];
        }

        internal HpVector(Vector vec, Unit units) : this(vec.Length, vec.Size, units)
        {
            if (_size == 0)
                return;

            var values = vec.Raw;
            Units = units ?? values[0].Units;
            for (int i = _size - 1; i >= 0 ; --i)
            {
                var v = values[i];
                _values[i] = v.D * Unit.Convert(Units, v.Units, ',');
            }
        }

        public override bool Equals(object obj) => obj is HpVector v && Equals(v);

        public bool Equals(HpVector other) => 
            Length == other.Length && _size != other._size &&
            _values.AsSpan(0, _size).SequenceEqual(other._values.AsSpan(0, other._size));

        internal override HpVector Copy()
        {
            var vector = new HpVector(_length, _size, Units);
            _values.AsSpan(0, _size).CopyTo(vector._values);
            return vector;
        }

        internal double[] RawCopy()
        {
            var vector = new double[_length];
            _values.AsSpan(0, _size).CopyTo(vector);
            return vector;
        }

        private static HpVector Clone(HpVector a) => new(a._length, a._size, a.Units);
        private static HpVector Clone(HpVector a, HpVector b) => 
            new(Math.Max(a._length, b._length), Math.Max(a._size, b._size), a.Units);

        public static HpVector operator -(HpVector a)
        {
            var b = Clone(a);
            var na = a._size;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sb = b._values.AsSpan();
            ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
            var nv = 0;
            if (na > 15)
            {
                nv = na / _vecSize;
                Span<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb);
                for (int i = 0; i < nv; ++i)
                    vb[i] = -va[i];

                nv *= _vecSize;
            }
            for (int i = nv; i < na; ++i)
                sb[i] = -sa[i];

            return b;
        }

        public static HpVector operator -(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            var d = Unit.Convert(a.Units, b.Units, '-');
            var na = a._size;
            var nb = b._size;
            (var n1, var n2) = MinMax(na, nb);
            var n = n2 - n1;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (n1 > 15)
            {
                nv = n1 / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa[..n1]);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb[..n1]);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                if (d == 1d)
                {
                    for (int i = 0; i < nv; ++i)
                        vc[i] = va[i] - vb[i];
                }
                else
                {
                    var vd = new SN.Vector<double>(d);
                    for (int i = 0; i < nv; ++i)
                        vc[i] = va[i] - vb[i] * vd;
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < n1; ++i)
                sc[i] = sa[i] - sb[i] * d;

            if (na > nb)
                sa.Slice(n1, n).CopyTo(sc.Slice(n1, n));
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = -sb[i] * d;

            return c;
        }

        public static HpVector operator -(HpVector a, RealValue b)
        {
            var c = Clone(a);
            var db = b.D * Unit.Convert(a.Units, b.Units, '-');
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (na > 15)
            {
                nv = na / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                var vb = new SN.Vector<double>(db);
                for (int i = 0; i < nv; ++i)
                    vc[i] = va[i] - vb;

                nv *= _vecSize;
            }
            for (int i = nv; i < na; ++i)
                sc[i] = sa[i] - db;

            if (nc > na && db != 0d)
                c.Fill(-db, na, nc - na);

            return c;
        }

        public static HpVector operator -(RealValue a, HpVector b)
        {
            var c = Clone(b);
            c.Units = a.Units;  
            var da = a.D;
            var k = Unit.Convert(a.Units, b.Units, '-');
            var nb = b._size;
            var nc = c._length;
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (nb > 15)
            {
                nv = nb / _vecSize;
                var va = new SN.Vector<double>(da);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                var vk = new SN.Vector<double>(k);
                for (int i = 0; i < nv; ++i)
                    vc[i] = va - vb[i] * vk;

                nv *= _vecSize;
            }
            for (int i = nv; i < nb; ++i)
                sc[i] = da - sb[i] * k;

            if (nc > nb && da != 0d)
                c.Fill(da, nb, nc - nb);

            return c;
        }

        public static HpVector operator +(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            var d = Unit.Convert(a.Units, b.Units, '+');
            var na = a._size;
            var nb = b._size;
            (var n1, var n2) = MinMax(na, nb);
            var n = n2 - n1;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (n1 > 15)
            {
                nv = n1 / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa[..n1]);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb[..n1]);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                if (d == 1d)
                {
                    for (int i = 0; i < nv; ++i)
                        vc[i] = va[i] + vb[i];
                }
                else
                {
                    var vd = new SN.Vector<double>(d);
                    for (int i = 0; i < nv; ++i)
                        vc[i] = va[i] + vb[i] * vd;
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < n1; ++i)
                sc[i] = sa[i] + sb[i] * d;

            if (na > nb)
                sa.Slice(n1, n).CopyTo(sc.Slice(n1, n));
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = sb[i] * d;

            return c;
        }

        public static HpVector operator +(HpVector a, RealValue b)
        {
            var c = Clone(a);
            var db = b.D * Unit.Convert(a.Units, b.Units, '-');
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (na > 15)
            {
                nv = na / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                var vb = new SN.Vector<double>(db);
                for (int i = 0; i < nv; ++i)
                    vc[i] = va[i] + vb;

                nv *= _vecSize;
            }
            for (int i = nv; i < na; ++i)
                sc[i] = sa[i] + db;

            if (nc > na && db != 0d)
                c.Fill(db, na, nc - na);

            return c;
        }

        public static HpVector operator *(HpVector a, HpVector b)
        {
            var na = a._size;
            var nb = b._size;
            var n1 = Math.Min(na, nb);
            var n2 = Math.Max(a._length, b._length);
            var u = Unit.Multiply(a.Units, b.Units, out var d, true);
            var c = new HpVector(n2, n1, u);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, n1);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, n1);
            Span<double> sc = c._values.AsSpan(0, n1);
            var nv = 0;
            if (n1 > 15)
            {
                nv = n1 / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                if (d == 1d)
                {
                    for (int i = 0; i < nv; ++i)
                        vc[i] = va[i] * vb[i];
                }
                else
                {
                    var vd = new SN.Vector<double>(d);
                    for (int i = 0; i < nv; ++i)
                        vc[i] = va[i] * vb[i] * vd;
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < n1; ++i)
                sc[i] = sa[i] * sb[i] * d;

            return c;
        }

        public static HpVector operator *(HpVector a, RealValue b)
        {
            var na = b.D == 0d ? 0 : a._size;
            var u = Unit.Multiply(a.Units, b.Units, out var db, true);
            var c = new HpVector(a._length, na, u);
            if (na == 0)
                return c;

            db *= b.D; 
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (na > 15)
            {
                nv = na / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                var vb = new SN.Vector<double>(db);
                for (int i = 0; i < nv; ++i)
                    vc[i] = va[i] * vb;

                nv *= _vecSize;
            }
            for (int i = nv; i < na; ++i)
                sc[i] = sa[i] * db;

            return c;
        }

        public static HpVector operator *(HpVector a, double b)
        {
            var na = b == 0d ? 0 : a._size;
            var c = new HpVector(a._length, na, a.Units);
            if (na == 0)
                return c;

            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (na > 15)
            {
                nv = na / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                var vb = new SN.Vector<double>(b);
                for (int i = 0; i < nv; ++i)
                    vc[i] = va[i] * vb;

                nv *= _vecSize;
            }
            for (int i = nv; i < na; ++i)
                sc[i] = sa[i] * b;

            return c;
        }

        public static HpVector operator /(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = Unit.Divide(a.Units, b.Units, out var d, true);
            var na = a._size;
            var nb = b._size;
            var nc = c._length;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (n1 > 15)
            {
                nv = n1 / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa[..n1]);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb[..n1]);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                if (d == 1d)
                {
                    for (int i = 0; i < nv; ++i)
                        vc[i] = va[i] / vb[i];
                }
                else
                {
                    var vd = new SN.Vector<double>(d);
                    for (int i = 0; i < nv; ++i)
                        vc[i] = va[i] / vb[i] * vd;
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < n1; ++i)
                sc[i] = sa[i] / sb[i] * d;

            c.Fill(double.NaN, n2, nc - n2);
            if (na > nb)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = sa[i] / 0d;
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = 0d / sb[i];

            return c;
        }

        public static HpVector operator /(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = Unit.Divide(a.Units, b.Units, out var db, true);
            db /= b.D;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (na > 15)
            {
                nv = na / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                var vb = new SN.Vector<double>(db);
                for (int i = 0; i < nv; ++i)
                    vc[i] = va[i] * vb;

                nv *= _vecSize;
            }
            for (int i = nv; i < na; ++i)
                sc[i] = sa[i] * db;

            if (nc > na)
            {
                var v = 0d * db;
                if (v != 0d)
                    c.Fill(v, na, nc - na);
            }
            return c;
        }

        public static HpVector operator /(RealValue a, HpVector b)
        {
            var c = Clone(b);
            c.Units = Unit.Divide(a.Units, b.Units, out var da, true);
            da *= a.D;
            var nb = b._size;
            var nc = c._length;
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (nb > 15)
            {
                nv = nb / _vecSize;
                var va = new SN.Vector<double>(da);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                for (int i = 0; i < nv; ++i)
                    vc[i] = va / vb[i];

                nv *= _vecSize;
            }
            for (int i = nv; i < nb; ++i)
                sc[i] = da / sb[i];

            if (nc > nb)
                c.Fill(da / 0d, nb, nc - nb);

            return c;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static SN.Vector<double> Remainder(SN.Vector<double> va, SN.Vector<double> vb)
        {
            var vq = va / vb;
            var vt = SN.Vector.ConditionalSelect(
                SN.Vector.GreaterThanOrEqual(vq, SN.Vector<double>.Zero),
                SN.Vector.Floor(vq),
                SN.Vector.Ceiling(vq)
            );
            return va - vt * vb;
        }

        public static HpVector operator %(HpVector a, HpVector b)
        {
            if (b.Units is not null)
                throw Exceptions.CannotEvaluateRemainder(Unit.GetText(a.Units), Unit.GetText(b.Units));
            
            var c = Clone(a, b);
            var na = a._size;
            var nb = b._size;
            var nc = c._length;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (n1 > 15)
            {
                nv = n1 / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa[..n1]);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb[..n1]);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                for (int i = 0; i < nv; ++i)
                    vc[i] = Remainder(va[i], vb[i]);

                nv *= _vecSize;
            }
            for (int i = nv; i < n1; ++i)
                sc[i] = sa[i] % sb[i];

            c.Fill(double.NaN, n2, nc - n2);
            if (na > nb)
                c.Fill(double.NaN, n1, n2 - n1);
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = 0d % sb[i];

            return c;
        }

        public static HpVector operator %(HpVector a, RealValue b)
        {
            if (b.Units is not null)
                throw Exceptions.CannotEvaluateRemainder(Unit.GetText(a.Units), Unit.GetText(b.Units));

            var c = Clone(a);
            var db = b.D;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (na > 15)
            {
                nv = na / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                var vb = new SN.Vector<double>(db);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                for (int i = 0; i < nv; ++i)
                    vc[i] = Remainder(va[i], vb);

                nv *= _vecSize;
            }
            for (int i = nv; i < na; ++i)
                sc[i] = sa[i] % db;

            if (nc > na && db != 0d)
                c.Fill(db, na, nc - na);

            return c;
        }

        public static HpVector operator %(RealValue a, HpVector b)
        {
            if (b.Units is not null)
                throw Exceptions.CannotEvaluateRemainder(Unit.GetText(a.Units), Unit.GetText(b.Units));

            var c = Clone(b);
            c.Units = a.Units;
            var da = a.D;
            var nb = b._size;
            var nc = c._length;
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            var nv = 0;
            if (nb > 15)
            {
                nv = nb / _vecSize;
                var va = new SN.Vector<double>(da);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb);
                Span<SN.Vector<double>> vc = MemoryMarshal.Cast<double, SN.Vector<double>>(sc);
                for (int i = 0; i < nv; ++i)
                    vc[i] = Remainder(va, vb[i]);

                nv *= _vecSize;
            }
            for (int i = nv; i < nb; ++i)
                sc[i] = da % sb[i];

            if (nc > nb)
                c.Fill(double.NaN, nb, nc - nb);

            return c;
        }

        public static HpVector operator ==(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = null;
            var d = Unit.Convert(a.Units, b.Units, '≡');
            var na = a._size;
            var nb = b._size;
            var nc = c._length;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = n1 - 1; i >= 0; --i)
                sc[i] = sa[i].IsEqual(sb[i] * d);

            c.Fill(1d, n2, nc - n2);
            if (na > nb)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = sa[i].IsEqual(0d);
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = 0d.IsEqual(sb[i] * d);

            return c;
        }

        public static HpVector operator ==(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = null;
            var db = Unit.Convert(a.Units, b.Units, '≡') * b.D;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            for (int i = na - 1; i >= 0; --i)
                sc[i] = sa[i].IsEqual(db);

            if (nc > na)
            {
                var v = db.IsEqual(0d);
                if (v != 0d)
                    c.Fill(v, na, nc - na);
            }
            return c;
        }

        public static HpVector operator !=(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = null;
            var d = Unit.Convert(a.Units, b.Units, '≠');
            var na = a._size;
            var nb = b._size;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);  
            Span<double> sc = c._values.AsSpan();
            for (int i = n1 - 1; i >= 0; --i)
                sc[i] = sa[i].IsNotEqual(sb[i] * d);

            if (na > nb)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = sa[i].IsNotEqual(0d);
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = 0d.IsNotEqual(sb[i] * d);

            return c;
        }

        public static HpVector operator !=(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = null;
            var db = Unit.Convert(a.Units, b.Units, '≠') * b.D;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            for (int i = na - 1; i >= 0; --i)
                sc[i] = sa[i].IsNotEqual(db);

            if (nc > na)
            {
                var v = db.IsNotEqual(0d);
                if (v != 0d)
                    c.Fill(v, na, nc - na);
            }
            return c;
        }

        public static HpVector operator <(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = null;
            var d = Unit.Convert(a.Units, b.Units, '<');
            var na = a._size;
            var nb = b._size;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = n1 - 1; i >= 0; --i)
                sc[i] = sa[i].IsLessThan(sb[i] * d);

            if (na > nb)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = sa[i].IsLessThan(0d);
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = 0d.IsLessThan(sb[i] * d);

            return c;
        }

        public static HpVector operator <(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = null;
            var db = Unit.Convert(a.Units, b.Units, '<') * b.D;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            for (int i = na - 1; i >= 0; --i)
                sc[i] = sa[i].IsLessThan(db);

            if (nc > na)
            {
                var v = 0d.IsLessThan(db);
                if (v != 0d)
                    c.Fill(v, na, nc - na);
            }
            return c;
        }

        public static HpVector operator <(RealValue a, HpVector b)
        {
            var c = Clone(b);
            c.Units = null;
            var da = a.D;
            var k = Unit.Convert(a.Units, b.Units, '<');
            var nb = b._size;
            var nc = c._length;
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = nb - 1; i >= 0; --i)
                sc[i] = da.IsLessThan(sb[i] * k);

            if (nc > nb)
            {
                var v = da.IsLessThan(0d);
                if (v != 0d)
                    c.Fill(v, nb, nc - nb);
            }
            return c;
        }

        public static HpVector operator >(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = null;
            var d = Unit.Convert(a.Units, b.Units, '>');
            var na = a._size;
            var nb = b._size;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = n1 - 1; i >= 0; --i)
                sc[i] = sa[i].IsGreaterThan(sb[i] * d);

            if (na > nb)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = sa[i].IsGreaterThan(0d);
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = 0d.IsGreaterThan(sb[i] * d);

            return c;
        }

        public static HpVector operator >(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = null;
            var db = Unit.Convert(a.Units, b.Units, '>') * b.D;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            for (int i = na - 1; i >= 0; --i)
                sc[i] = sa[i].IsGreaterThan(db);

            if (nc > na)
            {
                var v = 0d.IsGreaterThan(db);
                if (v != 0d)
                    c.Fill(v, na, nc - na);
            }
            return c;
        }

        public static HpVector operator >(RealValue a, HpVector b)
        {
            var c = Clone(b);
            c.Units = null;
            var da = a.D;
            var k = Unit.Convert(a.Units, b.Units, '>');
            var nb = b._size;
            var nc = c._length;
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = nb - 1; i >= 0; --i)
                sc[i] = da.IsGreaterThan(sb[i] * k);

            if (nc > nb)
            {
                var v = da.IsGreaterThan(0d);
                if (v != 0d)
                    c.Fill(v, nb, nc - nb);
            }
            return c;
        }

        public static HpVector operator <=(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = null;
            var d = Unit.Convert(a.Units, b.Units, '≤');
            var na = a._size;
            var nb = b._size;
            var nc = c._length;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = n1 - 1; i >= 0; --i)
                sc[i] = sa[i].IsLessThanOrEqual(sb[i] * d);

            c.Fill(1d, n2, nc - n2);
            if (na > nb)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = sa[i].IsLessThanOrEqual(0d);
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = 0d.IsLessThanOrEqual(sb[i] * d);

            return c;
        }

        public static HpVector operator <=(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = null;
            var db = Unit.Convert(a.Units, b.Units, '≤') * b.D;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            for (int i = na - 1; i >= 0; --i)
                sc[i] = sa[i].IsLessThanOrEqual(db);

            if (nc > na)
            {
                var v = 0d.IsLessThanOrEqual(db);
                if (v != 0d)
                    c.Fill(v, na, nc - na);
            }
            return c;
        }

        public static HpVector operator <=(RealValue a, HpVector b)
        {
            var c = Clone(b);
            c.Units = null;
            var da = a.D;
            var nb = b._size;
            var nc = c._length;
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = nb - 1; i >= 0; --i)
                sc[i] = da.IsLessThanOrEqual(sb[i]);

            if (nc > nb)
            {
                var v = da.IsLessThanOrEqual(0d);
                if (v != 0d)
                    c.Fill(v, nb, nc - nb);
            }
            return c;
        }

        public static HpVector operator >=(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = null;
            var d = Unit.Convert(a.Units, b.Units, '≥');
            var na = a._size;
            var nb = b._size;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = n1 - 1; i >= 0; --i)
                sc[i] = sa[i].IsGreaterThanOrEqual(sb[i] * d);

            if (na > nb)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = sa[i].IsGreaterThanOrEqual(0d);
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = 0d.IsGreaterThanOrEqual(sb[i] * d);

            return c;
        }

        public static HpVector operator >=(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = null;
            var db = Unit.Convert(a.Units, b.Units, '≥') * b.D;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            for (int i = na - 1; i >= 0; --i)
                sc[i] = sa[i].IsGreaterThanOrEqual(db);

            if (nc > na)
            {
                var v = 0d.IsGreaterThanOrEqual(db);
                if (v != 0d)
                    c.Fill(v, na, nc - na);
            }
            return c;
        }

        public static HpVector operator >=(RealValue a, HpVector b)
        {
            var c = Clone(b);
            c.Units = null;
            var d = a.D;
            var k = Unit.Convert(a.Units, b.Units, '≥');
            var nb = b._size;
            var nc = c._length;
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = nb - 1; i >= 0; --i)
                sc[i] = d.IsGreaterThanOrEqual(sb[i] * k);

            if (nc > nb)
            {
                var v = d.IsGreaterThanOrEqual(0d);
                if (v != 0d)
                    c.Fill(v, nb, nc - nb);
            }
            return c;
        }

        private const double logicalZero = RealValue.LogicalZero;   
        public static HpVector operator &(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = null;
            var na = a._size;
            var nb = b._size;
            var n = na < nb ? na : nb;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = n - 1; i >= 0; --i)
                sc[i] = Math.Abs(sa[i]) < logicalZero || Math.Abs(sb[i]) < logicalZero ? 0d : 1d;

            return c;
        }

        public static HpVector operator &(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = null;
            var na = a._size;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            if (Math.Abs(b.D) < logicalZero)
                c.Fill(0d, 0, na);
            else
                for (int i = na - 1; i >= 0; --i)
                    sc[i] = Math.Abs(sa[i]) < logicalZero ? 0d : 1d;

            return c;
        }

        public static HpVector operator |(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = null;
            var na = a._size;
            var nb = b._size;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = n1 - 1; i >= 0; --i)
                sc[i] = Math.Abs(sa[i]) >= logicalZero || Math.Abs(sb[i]) >= logicalZero ? 1d : 0d;

            if (na > nb)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = Math.Abs(sa[i]) >= logicalZero ? 1d : 0d;
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = Math.Abs(sb[i]) >= logicalZero ? 1d : 0d;

            return c;
        }

        public static HpVector operator |(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = null;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            var v = Math.Abs(b.D) >= logicalZero;
            if (v)
                c.Fill(1d, 0, na);
            else
                for (int i = na - 1; i >= 0; --i)
                    sc[i] = sa[i] >= logicalZero ? 1d : 0d;

            if (nc > na && v)
                c.Fill(1d, na, nc - na);

            return c;
        }

        public static HpVector operator ^(HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            c.Units = null;
            var na = a._size;
            var nb = b._size;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            for (int i = n1 - 1; i >= 0; --i)
                sc[i] = (Math.Abs(sa[i]) >= logicalZero) != (Math.Abs(sb[i]) >= logicalZero) ? 1d : 0d;

            if (na > nb)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = Math.Abs(sa[i]) >= logicalZero ? 1d : 0d;
            else if (nb > na)
                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = Math.Abs(sb[i]) >= logicalZero ? 1d : 0d;

            return c;
        }

        public static HpVector operator ^(HpVector a, RealValue b)
        {
            var c = Clone(a);
            c.Units = null;
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            var v = Math.Abs(b.D) >= logicalZero;
            for (int i = a._size - 1; i >= 0; --i)
                sc[i] = (Math.Abs(sa[i]) >= logicalZero) != v ? 1d : 0d;

            if (nc > na && v)
                c.Fill(1d, na, nc - na);

            return c;
        }

        internal static HpVector EvaluateOperator(Calculator.Operator<RealValue> op, HpVector a, HpVector b, long index) =>
            Calculator.GetOperatorSymbol(index) switch
            {
                '/' or '÷' => a / b,
                '⦼' => a % b,
                '*' => a * b,
                '-' => a - b,
                '+' => a + b,
                '<' => a < b,
                '>' => a > b,
                '≤' => a <= b,
                '≥' => a >= b,
                '≡' => a == b,
                '≠' => a != b,
                '∧' => a & b,
                '∨' => a | b,
                '⊕' => a ^ b,
                _=> EvaluateOperator2(op, a, b)
            };

        internal static HpVector EvaluateOperator(Calculator.Operator<RealValue> op, HpVector a, in RealValue b, long index) =>
            Calculator.GetOperatorSymbol(index) switch
            {
                 '/' or '÷' => a / b,
                 '⦼' => a % b,
                 '*' => a * b,
                 '-' => a - b,
                 '+' => a + b,
                 '<' => a < b,
                 '>' => a > b,
                 '≤' => a <= b,
                 '≥' => a >= b,
                 '≡' => a == b,
                 '≠' => a != b,
                 '∧' => a & b,
                 '∨' => a | b,
                 '⊕' => a ^ b,
                 _ => EvaluateOperator2(op, a, b)
            };


        internal static HpVector EvaluateOperator(Calculator.Operator<RealValue> op, in RealValue a, HpVector b, long index) =>
            Calculator.GetOperatorSymbol(index) switch
            {
                 '/' or '÷' => a / b,
                 '⦼' => a % b,
                 '*' => b * a,
                 '-' => a - b,
                 '+' => b + a,
                 '<' => a < b,
                 '>' => a > b,
                 '≤' => a <= b,
                 '≥' => a >= b,
                 '≡' => b == a,
                 '≠' => b != a,
                 '∧' => b & a,
                 '∨' => b | a,
                 '⊕' => b ^ a,
                 _ => EvaluateOperator2(op, a, b)
             };

        private static HpVector EvaluateOperator2(Calculator.Operator<RealValue> op, HpVector a, HpVector b)
        {
            var c = Clone(a, b);
            var na = a._size;
            var nb = b._size;
            var nc = c._length;
            (var n1, var n2) = MinMax(na, nb);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            var ua = a.Units;
            var ub = b.Units;
            for (int i = n1 - 1; i >= 0; --i)
                sc[i] = op(new(sa[i], ua), new(sb[i], ub)).D;

            var zero = RealValue.Zero;
            if (nc > n2)
            {
                var d = op(zero, zero).D;
                if (d != 0d)
                    c.Fill(d, n2, nc - n2);
            }
            if (na > nb)
            {

                if (ua != zero.Units)
                    zero = new(0d, ua);

                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = op(new(sa[i], ua), zero).D;
            }
            else if (nb > na)
            {
                if (ub != zero.Units)
                    zero = new(0d, ub);

                for (int i = n2 - 1; i >= n1; --i)
                    sc[i] = op(zero, new(sb[i], ub)).D;
            }
            c.Units = op(new(ua), new(ub)).Units;
            return c;
        }

        private static HpVector EvaluateOperator2(Calculator.Operator<RealValue> op, HpVector a, in RealValue b)
        {
            var c = Clone(a);
            var na = a._size;
            var nc = c._length;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            Span<double> sc = c._values.AsSpan();
            var ua = a.Units;
            if (na < nc)
            {
                var zero = new RealValue(0d, ua) ;
                var v = op(zero, b).D;
                if (v != 0)
                    c.Fill(v, na, nc - na);
            }
            for (int i = na - 1; i >= 0; --i)
                sc[i] = op(new(sa[i], ua), b).D;

            c.Units = op(new(ua), b).Units;
            return c;
        }

        private static HpVector EvaluateOperator2(Calculator.Operator<RealValue> op, in RealValue a, HpVector b)
        {
            var c = Clone(b);
            var nb = b._size;
            var nc = c._length;
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            var ub = b.Units;
            if (nb < nc)
            {
                var zero = new RealValue(0d, ub);
                var d = op(a, zero).D;
                if (d != 0d)
                    c.Fill(d, nb, nc - nb);
            }
            for (int i = nb - 1; i >= 0; --i)
                sc[i] = op(a, new(sb[i], ub)).D;

            c.Units = op(a, new(ub)).Units;
            return c;
        }

        internal static HpVector EvaluateFunction(Calculator.Function<RealValue> f, HpVector a)
        {
            var b = Clone(a);
            var na = a._size;
            var nb = b._length;
            var ua = a.Units;
            var ub = f(new(1d, ua)).Units;
            if (na > 0)
            {
                ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
                Span<double> sb = b._values.AsSpan();
                for (int i = na - 1; i >= 0; --i)
                {
                    var v = f(new(sa[i], ua));
                    sb[i] = v.D * Unit.Convert(ub, v.Units, ',');
                }
            }
            if (nb > na)
            {
                var zero = new RealValue(0d, ua);
                var d = f(zero).D * Unit.Convert(ub, zero.Units, ',');
                if (!d.Equals(0d))
                    b.Fill(d, na, nb - na);
            }
            b.Units = ub;
            return b;
        }

        internal override void SetUnits(Unit units) => Units = units;

        private static (int, int) MinMax(int n1, int n2)
        {
            if (n1 <= n2)
                return (n1, n2);

            return (n2, n1);
        }

        internal static HpVector CrossProduct(HpVector a, HpVector b)
        {
            var na = a._length;
            var nb = b._length;
            if (na < 2 || na > 3 || nb < 2 || nb > 3)
                throw Exceptions.CrossProductVectorDimensions();

            var u = Unit.Multiply(a.Units, b.Units, out var d, true);
            HpVector c = new(3, 3, u);
            ReadOnlySpan<double> sa = a._values.AsSpan(0, na);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, nb);
            Span<double> sc = c._values.AsSpan();
            sc[2] = (sa[0] * sb[1] - sa[1] * sb[0]) * d;
            if (na == 3)
            {
                sc[0] = -sa[2] * sb[1] * d;
                sc[1] = sa[2] * sb[0] * d;
            }
            if (nb == 3)
            {
                sc[0] += sa[1] * sb[2] * d;
                sc[1] -= sa[0] * sb[2] * d;
            }
            return c;
        }

        internal static double RawDotProduct(HpVector a, HpVector b)
        {
            var na = a._size;
            var nb = b._size;
            var n1 = Math.Min(na, nb);
            if (n1 == 0)
                return 0d;

            var sum = 0d;
            ReadOnlySpan<double> sa = a._values.AsSpan(0, n1);
            ReadOnlySpan<double> sb = b._values.AsSpan(0, n1);
            var nv = 0;
            if (n1 > 15)
            {
                nv = n1 / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb);
                for (int i = 0; i < nv; ++i)
                    sum += SN.Vector.Dot(va[i], vb[i]);

                nv *= _vecSize;
            }
            for (int i = nv; i < n1; ++i)
                sum += sa[i] * sb[i];

            return sum;
        }

        internal static RealValue DotProduct(HpVector a, HpVector b)
        {
            var u = Unit.Multiply(a.Units, b.Units, out var d, true);
            var sum = RawDotProduct(a, b);
            return new(sum * d, u);
        }

        internal override HpVector Resize(int newSize)
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

        internal override HpVector Reverse()
        {
            var vector = new HpVector(_length, _length, Units);
            var start = 0;
            while (start < _size)
            {
                if (_values[start] != 0d)
                    break;

                ++start;
            }
            var span = vector._values.AsSpan(); 
            for (int i = start; i < _size; ++i)
                span[_length - i - 1] = _values[i];

            return vector;
        }

        internal override HpVector Fill(RealValue value)
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
                    _values = new double[_capacity];
                }
            }
            Units = value.Units;
            _values.AsSpan().Fill(value.D);
            return this;
        }

        internal void Fill(double value, int start, int len)
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

        //private static readonly Comparison<RealValue> ascending = new((x, y) => x.CompareTo(y));
        internal static new readonly Comparison<double> descending = new((x, y) => -x.CompareTo(y));

        internal override HpVector Sort(bool reverse = false)
        {
            HpVector vector = new(_length, _length, Units);
            if (_size == 0)
                return vector;

            var span = vector._values.AsSpan();
            _values.AsSpan(0, _size).CopyTo(span);
            if (reverse)
                span.Sort(descending);
            else
                span.Sort();

            return vector;
        }

        internal override HpVector Order(bool reverse = false) => FromIndexes(GetOrderIndexes(reverse));

        internal override int[] GetOrderIndexes(bool reverse)
        {
            var values = new double[_length];
            var span = values.AsSpan();
            _values.AsSpan(0, _size).CopyTo(span);
            var indexes = Enumerable.Range(0, _length).ToArray();
            if (reverse)
                span.Sort<double, int>(indexes, descending);
            else
                span.Sort<double, int>(indexes);

            return indexes;
        }

        internal override HpVector First(int length)
        {
            if (length < 1)
                length = 1;
            else if (length >= _length)
                return Copy();

            HpVector vector = new(length, length, Units);
            if (length > _size)
                length = _size;

            _values.AsSpan(0, length).CopyTo(vector._values);
            return vector;
        }

        internal override HpVector Last(int length)
        {
            if (length < 1)
                length = 1;
            else if (length >= _length)
                return Copy();

            HpVector vector = new(length, length, Units);
            var start = _length - length;
            if (start < _size)
                _values.AsSpan(start, _size - start).CopyTo(vector._values);

            return vector;
        }

        internal override HpVector Slice(int start, int end)
        {
            if (start > end)
                (start, end) = (end, start);

            if (start > _length) start = _length;
            if (end > _length) end = _length;

            if (start == 1 && end == _length)
                return Copy();

            start--;
            var len = end - start;
            HpVector vector = new(len, len, Units);
            if (end > _size) end = _size;
            if (start < end)
                _values.AsSpan(start, end - start).CopyTo(vector._values);

            return vector;
        }

        internal static new HpVector Range(RealValue start, RealValue end, RealValue step)
        {
            if (step.D.AlmostEquals(0))
                throw Exceptions.StepCannotBeZero();

            var u = start.Units;
            var startVal = start.D;
            var endVal = end.D * Unit.Convert(u, end.Units, ':');
            var stepVal = step.D * Unit.Convert(u, start.Units, ':');
            var len = ((endVal - startVal) / stepVal) + 1;
            if (len <= 1)
                return new HpVector([startVal, endVal], u);

            if (len > MaxLength)
                throw Exceptions.VectorSizeLimit();

            len = Math.Truncate(len);
            double[] values = new double[(int)len];
            for (int i = 0; i < len; ++i)
                values[i] = startVal + stepVal * i;

            return new(values, u);
        }

        internal override RealValue Search(RealValue value, int start)
        {
            if (!Unit.IsConsistent(Units, value.Units))
                return new(0d, Units);

            var d = value.D * Unit.Convert(Units, value.Units, ',');
            if (start < 1)
                start = 1;
            else if (start > _size)
            {
                if (start > _length)
                    return new(0d, Units);

                if (d.AlmostEquals(0d))
                    return new(start);

                return new(0d, Units);
            }
            for (int i = start - 1; i < _size; ++i)
            {
                if (d.AlmostEquals(_values[i]))
                    return new(i + 1);
            }
            if (_size < _length && d.AlmostEquals(0d))
                return new(_size + 1);

            return new(0d, Units);
        }

        internal override HpVector FindAll(RealValue value, int start, Relation rel) =>
            FromIndexes(FindAllIndexes(value, start, rel));

        internal HpVector Lookup(HpVector dest, RealValue value, Relation rel)
        {
            var indexes = FindAllIndexes(value, 1, rel);
            var n = indexes.Count();
            var vector = new HpVector(n, n, dest.Units);
            var span = vector._values.AsSpan();
            var j = 0;
            foreach (var í in indexes)
            {
                if (í > dest._length)
                    throw Exceptions.IndexOutOfRange((í + 1).ToString());

                span[j] = dest.GetValue(í);
                ++j;
            }
            return vector;
        }

        internal static bool Relate(double a, double b, Relation rel) =>
            rel switch
            {
                Relation.Equal => a.AlmostEquals(b),
                Relation.NotEqual => !a.AlmostEquals(b),
                Relation.LessThan => a.CompareTo(b) < 0 && !a.AlmostEquals(b),
                Relation.LessOrEqual => a.CompareTo(b) <= 0 || a.AlmostEquals(b),
                Relation.GreaterThan => a.CompareTo(b) > 0 && !a.AlmostEquals(b),
                Relation.GreaterOrEqual => a.CompareTo(b) >= 0 || a.AlmostEquals(b),
                _ => false
            };

        private IEnumerable<int> FindAllIndexes(RealValue value, int start, Relation rel)
        {
            if (!Unit.IsConsistent(Units, value.Units))
                return [];

            var d = value.D * Unit.Convert(Units, value.Units, ',');
            if (start < 1)
                start = 1;
            else if (start > _size)
            {
                if (start > _length)
                    return [];

                if (Relate(d, 0d, rel))
                    return Enumerable.Range(_size, _length - _size);

                return [];
            }
            var indexes = new List<int>();
            for (int i = start - 1; i < _size; ++i)
            {
                if (Relate(_values[i], d, rel))
                    indexes.Add(i);
            }
            if (_size < _length && Relate(0d, d, rel))
                indexes.AddRange(Enumerable.Range(_size, _length - _size));

            return indexes;
        }

        internal override HpVector Extract(Vector indexes)
        {
            var n = indexes.Length;
            var vector = new HpVector(n, n, Units);
            var span = vector._values.AsSpan();
            for (int i = 0; i < n; ++i)
            {
                var d = indexes[i].D;
                if (d < 1 || d > int.MaxValue)
                    throw Exceptions.MustBePositiveInteger(Exceptions.Items.Index);

                int j = (int)d;
                if (j > _length)
                    throw Exceptions.IndexOutOfRange(j.ToString());

                span[i] = GetValue(j - 1);
            }
            return vector;
        }

        internal override RealValue Count(RealValue value, int start)
        {
            if (!Unit.IsConsistent(Units, value.Units))
                return RealValue.Zero;

            var d = value.D * Unit.Convert(Units, value.Units, ',');
            var count = 0;
            for (int i = start - 1; i < _size; ++i)
                if (d.AlmostEquals(_values[i]))
                    ++count;

            if (d == 0d)
                count += _length - _size;

            return new(count);
        }

        //L1 or Manhattan norm  
        internal override RealValue L1Norm()
        {
            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units);

            var norm = 0d;
            ReadOnlySpan<double> sa = _values.AsSpan(0, _size);
            var nv = 0;
            if (_size > 15)
            {
                nv = _size / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                SN.Vector<double> vb = SN.Vector.Abs(va[0]);
                for (int i = 1; i < nv; ++i)
                    vb += SN.Vector.Abs(va[i]);

                for (int i = 0; i < _vecSize; ++i)
                    norm += vb[i];

                nv *= _vecSize;
            }
            for (int i = nv; i < _size; ++i)
                norm += Math.Abs(sa[i]);

            return new(norm, Units);
        }

        //L2 or Euclidean norm  
        internal override RealValue Norm()
        {
            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units);

            var norm = 0d;
            ReadOnlySpan<double> sa = _values.AsSpan(0, _size);
            var nv = 0;
            if (_size > 15)
            {
                nv = _size / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                for (int i = 0; i < nv; ++i)
                {
                    var d = va[i];
                    norm += SN.Vector.Dot(d, d);
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < _size; ++i)
            {
                var d = sa[i];
                norm += d * d;
            }    
            return new(Math.Sqrt(norm), Units);
        }

        //Lp norm   
        internal override RealValue LpNorm(int p)
        {
            if (p < 1)
                throw Exceptions.InvalidLpNormArgument();

            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units);

            var norm = 0d;
            for (int i = 0; i < _size; ++i)
                norm += Math.Pow(Math.Abs(_values[i]), p);

            return new(Math.Pow(norm, 1d / p), Units);
        }

        //L∞ (Infinity) or Chebyshev norm

        internal override RealValue InfNorm()
        {
            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units);

            var norm = MaxAbs();
            return new(norm, Units);
        }

        internal double MaxAbs()
        {
            var maxAbs = 0d;
            ReadOnlySpan<double> sa = _values.AsSpan(0, _size);
            var nv = 0;
            if (_size > 15)
            {
                nv = _size / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                var vb = SN.Vector.Abs(va[0]);
                for (int i = 1; i < nv; ++i)
                    vb = SN.Vector.Max(vb, SN.Vector.Abs(va[i]));

                for (int i = 0; i < _vecSize; ++i)
                {
                    var b = Math.Abs(vb[i]);
                    if (b > maxAbs)
                        maxAbs = b;
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < _size; ++i)
            {
                var d = Math.Abs(sa[i]);
                if (d > maxAbs)
                    maxAbs = d;
            }
            return maxAbs;
        }

        internal override HpVector Normalize() => this / Norm();

        internal static new HpVector FromIndexes(IEnumerable<int> indexes)
        {
            var n = indexes.Count();
            var vector = new HpVector(n, n, null);
            var span = vector._values.AsSpan();
            var i = 0;
            foreach (var index in indexes)
            {
                span[i] = index + 1;
                ++i;
            }
            return vector;
        }

        internal override RealValue Min()
        {
            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units);

            var min = double.MaxValue;
            ReadOnlySpan<double> sa = _values.AsSpan(0, _size);
            var nv = 0;
            if (_size > 15)
            {
                nv = _size / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                var vb = va[0];
                for (int i = 1; i < nv; ++i)
                    vb = SN.Vector.Min(vb, va[i]);

                for (int i = 0; i < _vecSize; ++i)
                {
                    var b = vb[i];
                    if (b < min)
                        min = b;
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < _size; ++i)
            {
                var b = sa[i];
                if (b < min)
                    min = b;
            }
            return new(min, Units);
        }

        internal override RealValue Max()
        {
            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units);

            var max = double.MinValue;
            ReadOnlySpan<double> sa = _values.AsSpan(0, _size);
            var nv = 0;
            if (_size > 15)
            {
                nv = _size / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                var vb = va[0];
                for (int i = 1; i < nv; ++i)
                    vb = SN.Vector.Max(vb, va[i]);

                for (int i = 0; i < _vecSize; ++i)
                {
                    var b = vb[i];
                    if (b > max)
                        max = b;
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < _size; ++i)
            {
                var b = sa[i];
                if (b > max)
                    max = b;
            }
            return new(max, Units);
        }

        internal override RealValue Sum()
        {
            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units);

            var sum = 0d;
            ReadOnlySpan<double> sa = _values.AsSpan(0, _size);
            var nv = 0;
            if (_size > 15)
            {
                nv = _size / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                var vb = va[0];
                for (int i = 1; i < nv; ++i)
                    vb += va[i];

                sum = SN.Vector.Sum(vb);
                nv *= _vecSize;
            }
            for (int i = nv; i < _size; ++i)
                sum += sa[i];

            return new(sum, Units);
        }

        internal override RealValue SumSq()
        {
            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units?.Pow(2f));

            var sumsq = 0d;
            ReadOnlySpan<double> sa = _values.AsSpan(0, _size);
            var nv = 0;
            if (_size > 15)
            {
                nv = _size / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                for (int i = 0; i < nv; ++i)
                {
                    var vb = va[i];
                    sumsq += SN.Vector.Dot(vb, vb);
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < _size; ++i)
            {
                var b = sa[i];
                sumsq += b * b;
            }
            return new(sumsq, Units?.Pow(2f));
        }

        internal override RealValue Srss()
        {
            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units);

            return new(Math.Sqrt(SumSq().D), Units);
        }

        internal override RealValue Average()
        {
            if (_size == 0)
                return Units is null ? RealValue.Zero : new(0d, Units);

            return new(Sum().D / _length, Units);
        }

        internal override RealValue Product()
        {
            if (_size == 0 || _length > _size)
                return Units is null ? RealValue.Zero : new(0d, Units?.Pow(_length));

            var product = 1d;
            ReadOnlySpan<double> sa = _values.AsSpan(0, _size);
            var nv = 0;
            if (_size > 15)
            {
                nv = _size / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(_values);
                var vb = va[0];
                for (int i = 1; i < nv; ++i)
                    vb *= va[i];

                for (int i = 0; i < _vecSize; ++i)
                    product *= vb[i];

                nv *= _vecSize;
            }
            for (int i = nv; i < _size; ++i)
                product *= sa[i];
            
            return new(product, Units?.Pow(_length));
        }

        internal override RealValue Mean()
        {
            var product = Product();
            var result = Math.Pow(product.D, 1d / _length);
            var u = product.Units;
            if (u is null)
                return new(result);

            u = Unit.Root(u, _length);
            return new(result, u);
        }

        internal override RealValue And()
        {
            if (_length > _size)
                return RealValue.Zero;

            for (int i = 0; i < _size; ++i)
                if (Math.Abs(_values[i]) < RealValue.LogicalZero)
                    return RealValue.Zero;

            return RealValue.One;
        }

        internal override RealValue Or()
        {
            for (int i = 0; i < _size; ++i)
                if (Math.Abs(_values[i]) >= RealValue.LogicalZero)
                    return RealValue.One;

            return RealValue.Zero;
        }

        internal override RealValue Xor()
        {
            if (_size == 0)
                return RealValue.Zero;

            var b = Math.Abs(_values[0]) >= RealValue.LogicalZero;
            for (int i = 1; i < _size; ++i)
                b = b != Math.Abs(_values[i]) >= RealValue.LogicalZero;

            return b ? RealValue.One : RealValue.Zero;
        }

        internal override RealValue Gcd()
        {
            if (_size == 0)
                return RealValue.One;

            var a = Calculator.AsLong(_values[0]);
            for (int i = 1; i < _size; ++i)
            {
                var b = Calculator.AsLong(_values[i]);
                a = Calculator.Gcd(a, b);
            }
            return new(a);
        }

        internal override RealValue Lcm()
        {
            if (_size == 0)
                return RealValue.One;

            var a = Calculator.AsLong(_values[0]);
            for (int i = 1; i < _size; ++i)
            {
                var b = Calculator.AsLong(_values[i]);
                a = a * b / Calculator.Gcd(a, b);
            }
            return new(a);
        }

        internal override RealValue Take(in RealValue x)
        {
            var d = Math.Round(x.D, MidpointRounding.AwayFromZero);
            if (!double.IsNormal(d) || d < Calculator.DeltaMinus || d > _length * Calculator.DeltaPlus)
                return RealValue.NaN;

            return this[(int)d - 1];
        }

        internal override RealValue Line(in RealValue x)
        {
            var d = x.D;
            if (!double.IsNormal(d) || d < Calculator.DeltaMinus || d > _length * Calculator.DeltaPlus)
                return RealValue.NaN;

            var i = (int)Math.Floor(d);
            var v1 = this[i - 1];
            if (i == d || d >= _length)
                return v1;
            return v1 + (this[i] - v1) * (d - i);
        }

        internal override RealValue Spline(in RealValue x)
        {
            var d = x.D;
            if (!double.IsNormal(d) || d < Calculator.DeltaMinus || d > _length * Calculator.DeltaPlus)
                return RealValue.NaN;

            var i = (int)Math.Floor(d) - 1;
            if (i == d || d >= _length)
                return this[i];

            var y0 = GetValue(i);
            var y1 = GetValue(i + 1);
            var dy = y1 - y0;
            var a = dy;
            var b = dy;
            dy = Math.Sign(dy);
            if (i > 0)
            {
                var y2 = GetValue(i - 1);
                a = (y1 - y2) * (Math.Sign(y0 - y2) == dy ? 0.5 : 0.25);
            }
            if (i < _length - 2)
            {
                var y2 = GetValue(i + 2);
                b = (y2 - y0) * (Math.Sign(y2 - y1) == dy ? 0.5 : 0.25);
            }
            if (i == 0)
                a += (a - b) / 2;

            if (i == _length - 2)
                b += (b - a) / 2;

            var t = d - i - 1d;
            d = y0 + ((y1 - y0) * (3 - 2 * t) * t + ((a + b) * t - a) * (t - 1)) * t;
            return new(d, Units);
        }

        internal void Scale(double d)
        {
            ReadOnlySpan<double> s = _values.AsSpan(0, _size);
            var nv = 0;
            if (_size > 15)
            {
                nv = _size / _vecSize;
                ReadOnlySpan<SN.Vector<double>> v = MemoryMarshal.Cast<double, SN.Vector<double>>(s);
                var vd = new SN.Vector<double>(d);
                for (int i = 0; i < nv; ++i)
                    (v[i] * d).CopyTo(_values, i * _vecSize);

                nv *= _vecSize;
            }
            for (int i = nv; i < _size; ++i)
                _values[i] *= d;
        }
    }
}