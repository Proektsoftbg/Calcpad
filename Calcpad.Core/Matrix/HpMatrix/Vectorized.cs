using System;
using System.Runtime.InteropServices;
using SN = System.Numerics;

namespace Calcpad.Core
{
    internal static class Vectorized
    {
        private static readonly int _vecSize = SN.Vector<double>.Count;
        internal static void Scale(Span<double> x, double d)
        {
            var len = x.Length;
            var nv = 0;
            if (len > 0)
            {
                nv = len / _vecSize;
                Span<SN.Vector<double>> vx = MemoryMarshal.Cast<double, SN.Vector<double>>(x);
                for (int i = 0; i < nv; ++i)
                    vx[i] *= d;

                nv *= _vecSize;
            }
            for (int i = nv; i < len; ++i)
                x[i] *= d;
        }

        internal static void Add(double[] x, double[] y)
        {
            var len = x.Length;
            var nv = 0;
            if (len > 15)
            {
                nv = len / _vecSize;
                ReadOnlySpan<SN.Vector<double>> vx = MemoryMarshal.Cast<double, SN.Vector<double>>(x.AsSpan());
                Span<SN.Vector<double>> vy = MemoryMarshal.Cast<double, SN.Vector<double>>(y.AsSpan());
                for (int i = 0; i < nv; ++i)
                    vy[i] += vx[i];

                nv *= _vecSize;
            }
            for (int i = nv; i < len; ++i)
                y[i] += x[i];
        }

        internal static void Multiply(double[] x, double[] y, double[] z)
        {
            int len = x.Length;
            int nv = 0;
            if (len > 15)
            {
                nv = len / _vecSize;
                ReadOnlySpan<SN.Vector<double>> vx = MemoryMarshal.Cast<double, SN.Vector<double>>(x.AsSpan());
                ReadOnlySpan<SN.Vector<double>> vy = MemoryMarshal.Cast<double, SN.Vector<double>>(y.AsSpan());
                Span<SN.Vector<double>> vz = MemoryMarshal.Cast<double, SN.Vector<double>>(z.AsSpan());
                for (int i = 0; i < nv; ++i)
                    vz[i] = vx[i] * vy[i];

                nv *= _vecSize;
            }
            for (int i = nv; i < len; ++i)
                z[i] = x[i] * y[i];
        }

        internal static double SumAbs(Span<double> sv, int l, int m)
        {
            var len = m - l;
            if (len <= 0)
                return 0;

            var sum = 0d;
            var sa = sv.Slice(l, len);
            var nv = 0;
            if (len > 15)
            {
                nv = len / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                var vb = va[0];
                for (int i = 1; i < nv; ++i)
                    vb += SN.Vector.Abs(va[i]);

                sum = SN.Vector.Sum(vb);
                nv *= _vecSize;
            }
            for (int i = nv; i < len; ++i)
                sum += Math.Abs(sa[i]);

            return sum;
        }

        internal static double NormalizeAndSumSq(Span<double> sv, int l, int m, double scale)
        {
            int len = m - l;
            if (len <= 0)
                return 0;

            var sum = 0d;
            var sa = sv.Slice(l, len);
            var nv = 0;
            var inv = 1d / scale;
            if (nv > 2)
            {
                nv = len / _vecSize;
                Span<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                var vb = SN.Vector<double>.Zero;
                var vs = new SN.Vector<double>(inv);
                for (int i = 0; i < nv; ++i)
                {
                    va[i] = va[i] * vs;
                    var va_i = va[i];
                    vb += va_i * va_i;
                }
                sum += SN.Vector.Sum(vb);
                nv *= _vecSize;
            }
            for (int i = nv; i < len; ++i)
            {
                var v_j = sa[i] * inv;
                sa[i] = v_j;
                sum += v_j * v_j;
            }
            return sum;
        }

        internal static double DotProduct(ReadOnlySpan<double> a, ReadOnlySpan<double> b, int l, int m)
        {
            var len = m - l;
            if (len <= 0)
                return 0;

            var sum = 0d;
            ReadOnlySpan<double> sa = a.Slice(l, len);
            ReadOnlySpan<double> sb = b.Slice(l, len);
            var nv = 0;
            if (len > 15)
            {
                nv = len / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                ReadOnlySpan<SN.Vector<double>> vb = MemoryMarshal.Cast<double, SN.Vector<double>>(sb);
                for (int i = 0; i < nv; ++i)
                    sum += SN.Vector.Dot(va[i], vb[i]);

                nv *= _vecSize;
            }
            for (int i = nv; i < len; ++i)
                sum += sa[i] * sb[i];

            return sum;
        }

        internal static double SumSq(ReadOnlySpan<double> a, int l, int m)
        {
            var len = m - l;
            if (len <= 0)
                return 0;

            var sumsq = 0d;
            var sa = a.Slice(l, len);
            var nv = 0;
            if (len > 15)
            {
                nv = len / _vecSize;
                ReadOnlySpan<SN.Vector<double>> va = MemoryMarshal.Cast<double, SN.Vector<double>>(sa);
                for (int i = 0; i < nv; ++i)
                {
                    var vb = va[i];
                    sumsq += SN.Vector.Dot(vb, vb);
                }
                nv *= _vecSize;
            }
            for (int i = nv; i < len; ++i)
            {
                var b = sa[i];
                sumsq += b * b;
            }
            return sumsq;
        }
        internal static double Norm(double[] a) => Math.Sqrt(SumSq(a, 0, a.Length));

        //y += decimal * x
        internal static void MultiplyAdd(double[] x, double d, double[] y)
        {
            var len = x.Length;
            var nv = 0;
            if (len > 15)
            {
                nv = len / _vecSize;
                Span<SN.Vector<double>> vx = MemoryMarshal.Cast<double, SN.Vector<double>>(x.AsSpan());
                Span<SN.Vector<double>> vy = MemoryMarshal.Cast<double, SN.Vector<double>>(y.AsSpan());

                for (int i = 0; i < nv; ++i)
                    vy[i] += d * vx[i];

                nv *= _vecSize;
            }
            for (int i = nv; i < len; ++i)
                y[i] += d * x[i];
        }
    }
}