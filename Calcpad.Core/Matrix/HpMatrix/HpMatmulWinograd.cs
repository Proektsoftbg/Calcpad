using System;
using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace Calcpad.Core
{
    internal static class HpMatmulWinograd
    {
        private const int KernelSize = 64;
        private const int ParallelThreshold = 256;
        private static readonly int VecSize = Vector<double>.Count;
        private static readonly ArrayPool<double> Pool = ArrayPool<double>.Shared;

        private readonly struct MatrixView
        {
            private readonly double[] _data;
            private readonly int _offset;
            internal readonly int Rows;
            internal readonly int Cols;
            internal readonly int Stride;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public MatrixView(double[] data, int n) : this(data, n, n, n, 0) { }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal MatrixView(double[] data, int rows, int cols, int stride, int offset)
            {
                _data = data;
                Rows = rows;
                Cols = cols;
                Stride = stride;
                _offset = offset;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Span<double> GetRow(int row) => _data.AsSpan(_offset + row * Stride, Cols);

            internal double this[int row, int col]
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _data[_offset + row * Stride + col];
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                set => _data[_offset + row * Stride + col] = value;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal MatrixView Slice(int rowStart, int colStart, int rows, int cols) =>
                new(_data, rows, cols, Stride, _offset + rowStart * Stride + colStart);
        }

        internal static HpMatrix Multiply(HpMatrix A, HpMatrix B)
        {
            var n = A.RowCount;
            var m = NextPowerOfTwo(n);
            var l = m * m;
            var AFlat = Pool.Rent(l);
            var BFlat = Pool.Rent(l);
            var CFlat = Pool.Rent(l);
            try
            {
                AFlat.AsSpan().Clear();
                BFlat.AsSpan().Clear();
                CFlat.AsSpan().Clear();
                CopyToFlat(A, AFlat, n, m);
                CopyToFlat(B, BFlat, n, m);
                var viewA = new MatrixView(AFlat, m);
                var viewB = new MatrixView(BFlat, m);
                var viewC = new MatrixView(CFlat, m);
                WinogradRecursive(viewA, viewB, viewC, 0);
                Unit unit = Unit.Multiply(A.Units, B.Units, out var d, true);
                Vectorized.Scale(CFlat, d);
                return HpMatrix.FromFlatArray(CFlat, unit, n, m);
            }
            finally
            {
                Pool.Return(AFlat);
                Pool.Return(BFlat);
                Pool.Return(CFlat);
            }
        }
        private static void CopyToFlat(HpMatrix src, double[] dst, int n, int stride)
        {
            var rows = src.HpRows;
            if (src.Type == Matrix.MatrixType.Full || src.Type == Matrix.MatrixType.LowerTriangular)
                for (int i = 0; i < n; ++i)
                {
                    var row = rows[i];
                    Array.Copy(row.Raw, 0, dst, i * stride, row.Size);
                }
            else
                for (int i = 0; i < n; ++i)
                    for (int j = 0; j < n; ++j)
                        dst[i * stride + j] = src.GetValue(i, j);
        }

        private static int NextPowerOfTwo(int n)
        {
            var power = 1;
            while (power < n) 
                power *= 2;

            return power;
        }

        private static void WinogradRecursive(MatrixView A, MatrixView B, MatrixView C, int level)
        {
            var n = A.Rows;
            if (n == KernelSize)
            {
                if (Avx512F.IsSupported)
                    MultiplyAvx512Kernel_64x64(A, B, C);
                else 
                    MultiplyFmaKernel_64x64(A, B, C);
                return;
            }
            if (n < KernelSize)
            {
                MultiplySimd(A, B, C);
                return;
            }
            var half = n / 2;
            var l = half * half;
            // Zero-copy quadrant slicing!
            var A11 = A.Slice(0, 0, half, half);
            var A12 = A.Slice(0, half, half, half);
            var A21 = A.Slice(half, 0, half, half);
            var A22 = A.Slice(half, half, half, half);
            var B11 = B.Slice(0, 0, half, half);
            var B12 = B.Slice(0, half, half, half);
            var B21 = B.Slice(half, 0, half, half);
            var B22 = B.Slice(half, half, half, half);
            var C11 = C.Slice(0, 0, half, half);
            var C12 = C.Slice(0, half, half, half);
            var C21 = C.Slice(half, 0, half, half);
            var C22 = C.Slice(half, half, half, half);
            // Rent temporaries
            var a22ModArr = Pool.Rent(l);
            var b22ModArr = Pool.Rent(l);
            var t1Arr = Pool.Rent(l);
            var t2Arr = Pool.Rent(l);
            var t3Arr = Pool.Rent(l);
            var t4Arr = Pool.Rent(l);
            var t5Arr = Pool.Rent(l);
            var t6Arr = Pool.Rent(l);
            var m1Arr = Pool.Rent(l);
            var m2Arr = Pool.Rent(l);
            var m3Arr = Pool.Rent(l);
            var m4Arr = Pool.Rent(l);
            var m5Arr = Pool.Rent(l);
            var m6Arr = Pool.Rent(l);
            var m7Arr = Pool.Rent(l);
            try
            {
                // Create views for temporaries
                var A22Mod = new MatrixView(a22ModArr, half);
                var B22Mod = new MatrixView(b22ModArr, half);
                var M1 = new MatrixView(m1Arr, half);
                var M2 = new MatrixView(m2Arr, half);
                var M3 = new MatrixView(m3Arr, half);
                var M4 = new MatrixView(m4Arr, half);
                var M5 = new MatrixView(m5Arr, half);
                var M6 = new MatrixView(m6Arr, half);
                var M7 = new MatrixView(m7Arr, half);
                // A22Mod = A12 - A21 + A22
                // B22Mod = B12 - B21 + B22
                AddSubAdd(A12, A21, A22, A22Mod);
                AddSubAdd(B12, B21, B22, B22Mod);
                int nextLevel = level + 1;
                // Parallel execution
                if (level == 0 && n > ParallelThreshold)
                {
                    Parallel.Invoke(
                        () => WinogradRecursive(A11, B11, M1, nextLevel),
                        () => WinogradRecursive(A12, B21, M2, nextLevel),
                        () =>
                        {
                            var T4 = new MatrixView(t4Arr, half);
                            Subtract(B22Mod, B11, T4);
                            WinogradRecursive(A21, T4, M3, nextLevel);
                        },
                        () => WinogradRecursive(A22Mod, B22Mod, M4, nextLevel),
                        () => {
                            var T1 = new MatrixView(t1Arr, half);
                            var T5 = new MatrixView(t5Arr, half);
                            Add(A21, A22Mod, T1);
                            Add(B21, B22Mod, T5);
                            WinogradRecursive(T1, T5, M5, nextLevel);
                        },
                        () =>
                        {
                            var T2 = new MatrixView(t2Arr, half);
                            var T6 = new MatrixView(t6Arr, half);
                            Subtract(A22Mod, A12, T2);
                            Subtract(B22Mod, B12, T6);
                            WinogradRecursive(T2, T6, M6, nextLevel);
                        },
                        () =>
                        {
                            var T3 = new MatrixView(t3Arr, half);
                            Subtract(A22Mod, A11, T3);
                            WinogradRecursive(T3, B12, M7, nextLevel);
                        }
                    );
                }
                else
                {
                    // Compute temporaries
                    var T1 = new MatrixView(t1Arr, half);
                    var T2 = new MatrixView(t2Arr, half);
                    var T3 = new MatrixView(t3Arr, half);
                    var T4 = new MatrixView(t4Arr, half);
                    var T5 = new MatrixView(t5Arr, half);
                    var T6 = new MatrixView(t6Arr, half);
                    Add(A21, A22Mod, T1);      // T1 = A21 + A22Mod
                    Subtract(A22Mod, A12, T2); // T2 = A22Mod - A12
                    Subtract(A22Mod, A11, T3); // T3 = A22Mod - A11
                    Subtract(B22Mod, B11, T4); // T4 = B22Mod - B11
                    Add(B21, B22Mod, T5);      // T5 = B21 + B22Mod
                    Subtract(B22Mod, B12, T6); // T6 = B22Mod - B12
                    // 7 recursive multiplications
                    WinogradRecursive(A11, B11, M1, nextLevel);
                    WinogradRecursive(A12, B21, M2, nextLevel);
                    WinogradRecursive(A21, T4, M3, nextLevel);
                    WinogradRecursive(A22Mod, B22Mod, M4, nextLevel);
                    WinogradRecursive(T1, T5, M5, nextLevel);
                    WinogradRecursive(T2, T6, M6, nextLevel);
                    WinogradRecursive(T3, B12, M7, nextLevel);
                }
                // Combine results
                Add(M1, M2, C11);                    // C11 = M1 + M2
                Compute_C22(M5, M6, M2, M4, C22);    // C22 = M5 + M6 - M2 - M4
                Subtract(M5, M7, C12);               // C12 = M5 - M7
                SubtractInPlace(C12, C22);           // C12 -= C22
                Add(M3, M6, C21);                    // C21 = M3 + M6
                SubtractFromInPlace(C22, C21);       // C21 = C22 - C21
            }
            finally
            {
                Pool.Return(a22ModArr);
                Pool.Return(b22ModArr);
                Pool.Return(t1Arr);
                Pool.Return(t2Arr);
                Pool.Return(t3Arr);
                Pool.Return(t4Arr);
                Pool.Return(t5Arr);
                Pool.Return(t6Arr);
                Pool.Return(m1Arr);
                Pool.Return(m2Arr);
                Pool.Return(m3Arr);
                Pool.Return(m4Arr);
                Pool.Return(m5Arr);
                Pool.Return(m6Arr);
                Pool.Return(m7Arr);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Add(MatrixView A, MatrixView B, MatrixView C)
        {
            var rows = A.Rows;
            var cols = A.Cols;
            for (int i = 0; i < rows; ++i)
            {
                var sA = A.GetRow(i);
                var sB = B.GetRow(i);
                var sC = C.GetRow(i);
                var vA = MemoryMarshal.Cast<double, Vector<double>>(sA);
                var vB = MemoryMarshal.Cast<double, Vector<double>>(sB);
                var vC = MemoryMarshal.Cast<double, Vector<double>>(sC);
                int vecLen = vA.Length;
                for (int j = 0; j < vecLen; ++j)
                    vC[j] = vA[j] + vB[j];

                for (int j = vecLen * VecSize; j < cols; ++j)
                    sC[j] = sA[j] + sB[j];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Subtract(MatrixView A, MatrixView B, MatrixView C)
        {
            var rows = A.Rows;
            var cols = A.Cols;
            for (int i = 0; i < rows; ++i)
            {
                var sA = A.GetRow(i);
                var sB = B.GetRow(i);
                var sC = C.GetRow(i);
                var vA = MemoryMarshal.Cast<double, Vector<double>>(sA);
                var vB = MemoryMarshal.Cast<double, Vector<double>>(sB);
                var vC = MemoryMarshal.Cast<double, Vector<double>>(sC);
                int vecLen = vA.Length;
                for (int j = 0; j < vecLen; ++j)
                    vC[j] = vA[j] - vB[j];

                for (int j = vecLen * VecSize; j < cols; ++j)
                    sC[j] = sA[j] - sB[j];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SubtractInPlace(MatrixView A, MatrixView B)
        {
            // A = A - B
            var rows = A.Rows;
            var cols = A.Cols;
            for (int i = 0; i < rows; ++i)
            {
                var sA = A.GetRow(i);
                var sB = B.GetRow(i);
                var vA = MemoryMarshal.Cast<double, Vector<double>>(sA);
                var vB = MemoryMarshal.Cast<double, Vector<double>>(sB);
                int vecLen = vA.Length;
                for (int j = 0; j < vecLen; ++j)
                    vA[j] -= vB[j];

                for (int j = vecLen * VecSize; j < cols; ++j)
                    sA[j] -= sB[j];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SubtractFromInPlace(MatrixView A, MatrixView B)
        {
            // B = A - B
            var rows = A.Rows;
            var cols = A.Cols;
            for (int i = 0; i < rows; ++i)
            {
                var sA = A.GetRow(i);
                var sB = B.GetRow(i);
                var vA = MemoryMarshal.Cast<double, Vector<double>>(sA);
                var vB = MemoryMarshal.Cast<double, Vector<double>>(sB);
                int vecLen = vA.Length;
                for (int j = 0; j < vecLen; ++j)
                    vB[j] = vA[j] - vB[j];

                for (int j = vecLen * VecSize; j < cols; ++j)
                    sB[j] = sA[j] - sB[j];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddSubAdd(MatrixView A, MatrixView B, MatrixView C, MatrixView result)
        {
            // result = A - B + C
            var rows = A.Rows;
            var cols = A.Cols;
            for (int i = 0; i < rows; ++i)
            {
                var sA = A.GetRow(i);
                var sB = B.GetRow(i);
                var sC = C.GetRow(i);
                var sR = result.GetRow(i);
                var vA = MemoryMarshal.Cast<double, Vector<double>>(sA);
                var vB = MemoryMarshal.Cast<double, Vector<double>>(sB);
                var vC = MemoryMarshal.Cast<double, Vector<double>>(sC);
                var vR = MemoryMarshal.Cast<double, Vector<double>>(sR);
                int vecLen = vA.Length;
                for (int j = 0; j < vecLen; ++j)
                    vR[j] = vA[j] - vB[j] + vC[j];

                for (int j = vecLen * VecSize; j < cols; ++j)
                    sR[j] = sA[j] - sB[j] + sC[j];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Compute_C22(MatrixView M5, MatrixView M6, MatrixView M2, MatrixView M4, MatrixView C22)
        {
            // C22 = M5 + M6 - M2 - M4
            var rows = M5.Rows;
            var cols = M5.Cols;
            for (int i = 0; i < rows; ++i)
            {
                var s2 = M2.GetRow(i);
                var s4 = M4.GetRow(i);
                var s5 = M5.GetRow(i);
                var s6 = M6.GetRow(i);
                var sC = C22.GetRow(i);
                var v2 = MemoryMarshal.Cast<double, Vector<double>>(s2);
                var v4 = MemoryMarshal.Cast<double, Vector<double>>(s4);
                var v5 = MemoryMarshal.Cast<double, Vector<double>>(s5);
                var v6 = MemoryMarshal.Cast<double, Vector<double>>(s6);
                var vC = MemoryMarshal.Cast<double, Vector<double>>(sC);
                int vecLen = v5.Length;
                for (int j = 0; j < vecLen; ++j)
                    vC[j] = v5[j] + v6[j] - v2[j] - v4[j];

                for (int j = vecLen * VecSize; j < cols; ++j)
                    sC[j] = s5[j] + s6[j] - s2[j] - s4[j];
            }
        }

        // Generic SIMD version
        private static void MultiplySimd(MatrixView A, MatrixView B, MatrixView C)
        {
            int n = A.Rows;
            for (int i = 0; i < n; ++i)
            {
                var sC = C.GetRow(i);
                sC.Clear();
                var vC = MemoryMarshal.Cast<double, Vector<double>>(sC);
                ref Vector<double> c = ref MemoryMarshal.GetReference(vC);
                for (int k = 0; k < n; ++k)
                {
                    var A_ik = A[i, k];
                    var vA = new Vector<double>(A_ik);
                    var sB = B.GetRow(k);
                    var vB = MemoryMarshal.Cast<double, Vector<double>>(sB);
                    ref Vector<double> b = ref MemoryMarshal.GetReference(vB);
                    var vecLen = vB.Length;

                    for (int j = 0; j < vecLen; ++j)
                        Unsafe.Add(ref c, j) += vA * Unsafe.Add(ref b, j);

                    for (int j = vecLen * VecSize; j < n; ++j)
                        sC[j] += A_ik * sB[j];
                }
            }
        }

        // Optimized 64x64 micro-kernel with register blocking
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void MultiplyAvx512Kernel_64x64(MatrixView A, MatrixView B, MatrixView C)
        {
            // For 64x64, process entire matrix with optimal micro-kernels
            // Using 2x8 register blocking (2 rows of C, 8 vectors per row)
            for (int i = 0; i < KernelSize; i += 2)
            {
                ref double rC0 = ref MemoryMarshal.GetReference(C.GetRow(i));
                ref double rC1 = ref MemoryMarshal.GetReference(C.GetRow(i + 1));

                // Clear the two rows
                for (int j = 0; j < KernelSize; j += 8)
                {
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, j)) = Vector512<double>.Zero;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, j)) = Vector512<double>.Zero;
                }
                for (int k = 0; k < KernelSize; ++k)
                {
                    double a0 = A[i, k];
                    double a1 = A[i + 1, k];
                    var vA0 = Vector512.Create(a0);
                    var vA1 = Vector512.Create(a1);
                    ref double rB = ref MemoryMarshal.GetReference(B.GetRow(k));

                    // Process entire row with 8 vector registers (64 doubles)
                    var vB0 = Unsafe.As<double, Vector512<double>>(ref rB);
                    var vB1 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rB, 8));
                    var vB2 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rB, 16));
                    var vB3 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rB, 24));
                    var vB4 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rB, 32));
                    var vB5 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rB, 40));
                    var vB6 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rB, 48));
                    var vB7 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rB, 56));

                    // Row 0: 8 FMAs
                    var vC00 = Unsafe.As<double, Vector512<double>>(ref rC0);
                    var vC01 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 8));
                    var vC02 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 16));
                    var vC03 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 24));
                    var vC04 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 32));
                    var vC05 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 40));
                    var vC06 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 48));
                    var vC07 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 56));

                    vC00 = Avx512F.FusedMultiplyAdd(vA0, vB0, vC00);
                    vC01 = Avx512F.FusedMultiplyAdd(vA0, vB1, vC01);
                    vC02 = Avx512F.FusedMultiplyAdd(vA0, vB2, vC02);
                    vC03 = Avx512F.FusedMultiplyAdd(vA0, vB3, vC03);
                    vC04 = Avx512F.FusedMultiplyAdd(vA0, vB4, vC04);
                    vC05 = Avx512F.FusedMultiplyAdd(vA0, vB5, vC05);
                    vC06 = Avx512F.FusedMultiplyAdd(vA0, vB6, vC06);
                    vC07 = Avx512F.FusedMultiplyAdd(vA0, vB7, vC07);

                    Unsafe.As<double, Vector512<double>>(ref rC0) = vC00;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 8)) = vC01;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 16)) = vC02;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 24)) = vC03;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 32)) = vC04;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 40)) = vC05;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 48)) = vC06;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC0, 56)) = vC07;

                    // Row 1: 8 FMAs
                    var vC10 = Unsafe.As<double, Vector512<double>>(ref rC1);
                    var vC11 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 8));
                    var vC12 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 16));
                    var vC13 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 24));
                    var vC14 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 32));
                    var vC15 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 40));
                    var vC16 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 48));
                    var vC17 = Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 56));

                    vC10 = Avx512F.FusedMultiplyAdd(vA1, vB0, vC10);
                    vC11 = Avx512F.FusedMultiplyAdd(vA1, vB1, vC11);
                    vC12 = Avx512F.FusedMultiplyAdd(vA1, vB2, vC12);
                    vC13 = Avx512F.FusedMultiplyAdd(vA1, vB3, vC13);
                    vC14 = Avx512F.FusedMultiplyAdd(vA1, vB4, vC14);
                    vC15 = Avx512F.FusedMultiplyAdd(vA1, vB5, vC15);
                    vC16 = Avx512F.FusedMultiplyAdd(vA1, vB6, vC16);
                    vC17 = Avx512F.FusedMultiplyAdd(vA1, vB7, vC17);

                    Unsafe.As<double, Vector512<double>>(ref rC1) = vC10;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 8)) = vC11;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 16)) = vC12;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 24)) = vC13;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 32)) = vC14;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 40)) = vC15;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 48)) = vC16;
                    Unsafe.As<double, Vector512<double>>(ref Unsafe.Add(ref rC1, 56)) = vC17;
                }
            }
        }

        // FMA version for AVX2
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void MultiplyFmaKernel_64x64(MatrixView A, MatrixView B, MatrixView C)
        {
            // 2x16 register blocking for AVX2 (2 rows, 16 vectors of 4 doubles each)
            for (int i = 0; i < KernelSize; i += 2)
            {
                ref double rC0 = ref MemoryMarshal.GetReference(C.GetRow(i));
                ref double rC1 = ref MemoryMarshal.GetReference(C.GetRow(i + 1));
                // Clear rows
                for (int j = 0; j < KernelSize; j += 4)
                {
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, j)) = Vector256<double>.Zero;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, j)) = Vector256<double>.Zero;
                }
                for (int k = 0; k < KernelSize; ++k)
                {
                    double a0 = A[i, k];
                    double a1 = A[i + 1, k];
                    var vA0 = Vector256.Create(a0);
                    var vA1 = Vector256.Create(a1);
                    ref double rB = ref MemoryMarshal.GetReference(B.GetRow(k));

                    // Manually unrolled: process all 16 vectors (64 doubles)
                    // This creates 16 independent dependency chains
                    // Load all B vectors
                    var vB00 = Unsafe.As<double, Vector256<double>>(ref rB);
                    var vB01 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 4));
                    var vB02 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 8));
                    var vB03 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 12));
                    var vB04 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 16));
                    var vB05 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 20));
                    var vB06 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 24));
                    var vB07 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 28));
                    var vB08 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 32));
                    var vB09 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 36));
                    var vB10 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 40));
                    var vB11 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 44));
                    var vB12 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 48));
                    var vB13 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 52));
                    var vB14 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 56));
                    var vB15 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rB, 60));

                    // Row 0
                    var vC00 = Unsafe.As<double, Vector256<double>>(ref rC0);
                    var vC01 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 4));
                    var vC02 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 8));
                    var vC03 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 12));
                    var vC04 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 16));
                    var vC05 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 20));
                    var vC06 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 24));
                    var vC07 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 28));
                    var vC08 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 32));
                    var vC09 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 36));
                    var vC10 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 40));
                    var vC11 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 44));
                    var vC12 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 48));
                    var vC13 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 52));
                    var vC14 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 56));
                    var vC15 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 60));

                    vC00 = Fma.MultiplyAdd(vA0, vB00, vC00);
                    vC01 = Fma.MultiplyAdd(vA0, vB01, vC01);
                    vC02 = Fma.MultiplyAdd(vA0, vB02, vC02);
                    vC03 = Fma.MultiplyAdd(vA0, vB03, vC03);
                    vC04 = Fma.MultiplyAdd(vA0, vB04, vC04);
                    vC05 = Fma.MultiplyAdd(vA0, vB05, vC05);
                    vC06 = Fma.MultiplyAdd(vA0, vB06, vC06);
                    vC07 = Fma.MultiplyAdd(vA0, vB07, vC07);
                    vC08 = Fma.MultiplyAdd(vA0, vB08, vC08);
                    vC09 = Fma.MultiplyAdd(vA0, vB09, vC09);
                    vC10 = Fma.MultiplyAdd(vA0, vB10, vC10);
                    vC11 = Fma.MultiplyAdd(vA0, vB11, vC11);
                    vC12 = Fma.MultiplyAdd(vA0, vB12, vC12);
                    vC13 = Fma.MultiplyAdd(vA0, vB13, vC13);
                    vC14 = Fma.MultiplyAdd(vA0, vB14, vC14);
                    vC15 = Fma.MultiplyAdd(vA0, vB15, vC15);

                    Unsafe.As<double, Vector256<double>>(ref rC0) = vC00;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 4)) = vC01;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 8)) = vC02;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 12)) = vC03;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 16)) = vC04;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 20)) = vC05;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 24)) = vC06;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 28)) = vC07;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 32)) = vC08;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 36)) = vC09;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 40)) = vC10;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 44)) = vC11;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 48)) = vC12;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 52)) = vC13;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 56)) = vC14;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC0, 60)) = vC15;

                    // Row 1
                    var vC10_1 = Unsafe.As<double, Vector256<double>>(ref rC1);
                    var vC11_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 4));
                    var vC12_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 8));
                    var vC13_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 12));
                    var vC14_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 16));
                    var vC15_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 20));
                    var vC16_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 24));
                    var vC17_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 28));
                    var vC18_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 32));
                    var vC19_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 36));
                    var vC1A_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 40));
                    var vC1B_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 44));
                    var vC1C_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 48));
                    var vC1D_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 52));
                    var vC1E_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 56));
                    var vC1F_1 = Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 60));

                    vC10_1 = Fma.MultiplyAdd(vA1, vB00, vC10_1);
                    vC11_1 = Fma.MultiplyAdd(vA1, vB01, vC11_1);
                    vC12_1 = Fma.MultiplyAdd(vA1, vB02, vC12_1);
                    vC13_1 = Fma.MultiplyAdd(vA1, vB03, vC13_1);
                    vC14_1 = Fma.MultiplyAdd(vA1, vB04, vC14_1);
                    vC15_1 = Fma.MultiplyAdd(vA1, vB05, vC15_1);
                    vC16_1 = Fma.MultiplyAdd(vA1, vB06, vC16_1);
                    vC17_1 = Fma.MultiplyAdd(vA1, vB07, vC17_1);
                    vC18_1 = Fma.MultiplyAdd(vA1, vB08, vC18_1);
                    vC19_1 = Fma.MultiplyAdd(vA1, vB09, vC19_1);
                    vC1A_1 = Fma.MultiplyAdd(vA1, vB10, vC1A_1);
                    vC1B_1 = Fma.MultiplyAdd(vA1, vB11, vC1B_1);
                    vC1C_1 = Fma.MultiplyAdd(vA1, vB12, vC1C_1);
                    vC1D_1 = Fma.MultiplyAdd(vA1, vB13, vC1D_1);
                    vC1E_1 = Fma.MultiplyAdd(vA1, vB14, vC1E_1);
                    vC1F_1 = Fma.MultiplyAdd(vA1, vB15, vC1F_1);

                    Unsafe.As<double, Vector256<double>>(ref rC1) = vC10_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 4)) = vC11_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 8)) = vC12_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 12)) = vC13_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 16)) = vC14_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 20)) = vC15_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 24)) = vC16_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 28)) = vC17_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 32)) = vC18_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 36)) = vC19_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 40)) = vC1A_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 44)) = vC1B_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 48)) = vC1C_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 52)) = vC1D_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 56)) = vC1E_1;
                    Unsafe.As<double, Vector256<double>>(ref Unsafe.Add(ref rC1, 60)) = vC1F_1;
                }
            }
        }
    }
}