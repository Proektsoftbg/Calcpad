using System;
using System.Runtime.CompilerServices;

namespace Calcpad.Core
{
    internal static class HpSvdSolver
    {
        internal static void Bidiagonalize(double[][] U, double[] sig, double[] rv1, Span<double> U_col_i, int m, int n, out double anorm)
        {
            double sum;
            double scale, g;
            scale = g = anorm = 0d;
            for (int i = 0; i <= n; ++i)
            {
                var l = i + 1;
                rv1[i] = scale * g;
                if (i <= m)
                {
                    //Cache column i of U for optimized access
                    CacheUColumn(U, U_col_i, i, m);
                    var U_ii = U_col_i[i];
                    scale = Math.Abs(U_ii);
                    scale += Vectorized.SumAbs(U_col_i, l, m + 1);
                    if (scale != 0)
                    {
                        U_ii /= scale;
                        sum = HpLinearSolver.Sqr(U_ii);
                        U_col_i[i] = U_ii;
                        sum += Vectorized.NormalizeAndSumSq(U_col_i, l, m + 1, scale);
                        var f = U_ii;
                        g = -Math.CopySign(Math.Sqrt(sum), f);
                        var h = f * g - sum;
                        U_ii = f - g;
                        U_col_i[i] = U_ii;
                        for (int j = l; j <= n; ++j)
                        {
                            sum = U_ii * U[i][j];
                            int k = l;
                            int limit = m - 3;
                            for (; k <= limit; k += 4)
                            {
                                sum += U_col_i[k] * U[k][j] +
                                       U_col_i[k + 1] * U[k + 1][j] +
                                       U_col_i[k + 2] * U[k + 2][j] +
                                       U_col_i[k + 3] * U[k + 3][j];
                            }
                            for (; k <= m; ++k)
                                sum += U_col_i[k] * U[k][j];

                            f = sum / h;
                            for (k = i; k <= m; ++k)
                                U[k][j] += f * U_col_i[k];
                        }
                        for (int k = i; k <= m; ++k)
                            U_col_i[k] *= scale;
                    }
                    //Restore the cached column i of U
                    RestoreUColumn(U, U_col_i, i, m);
                }
                sig[i] = scale * g;
                if (i <= m && i != n)
                {
                    var U_i = U[i];
                    scale = Vectorized.SumAbs(U_i, l, n + 1);
                    if (scale != 0d)
                    {
                        var U_il = U_i[l] / scale;
                        sum = HpLinearSolver.Sqr(U_il);
                        U_i[l] = U_il;
                        sum += Vectorized.NormalizeAndSumSq(U_i, l + 1, n + 1, scale);
                        var f = U_il;
                        g = -Math.CopySign(Math.Sqrt(sum), f);
                        var h = 1d / (f * g - sum);
                        U_il = f - g;
                        U_i[l] = U_il;
                        for (int k = l; k <= n; ++k)
                            rv1[k] = U_i[k] * h;

                        for (int j = l; j <= m; ++j)
                        {
                            var U_j = U[j];
                            sum = Vectorized.DotProduct(U_i, U[j], l, m + 1);
                            for (int k = l; k <= n; ++k)
                                U_j[k] += sum * rv1[k];
                        }
                        for (int k = l; k <= n; ++k)
                            U_i[k] *= scale;
                    }
                }
                var a = Math.Abs(sig[i]);
                var rvi = rv1[i];
                if (rvi != 0d)
                    a += Math.Abs(rvi);

                if (anorm == 0d || a > anorm)
                    anorm = a;
            }
        }

        internal static void RightTransform(double[][] U, double[][] V, double[] rv1, int n)
        {
            double sum;
            var l = n + 1;
            var g = 0d;
            for (int i = n; i >= 0; --i)
            {
                var V_i = V[i];
                if (i < n)
                {
                    if (g != 0)
                    {
                        g = 1d / g;
                        var U_i = U[i];
                        for (int j = l; j <= n; ++j)
                            V_i[j] = (U_i[j] / U_i[l]) * g;

                        var U_il = U_i[l];
                        for (int j = l; j <= n; ++j)
                        {
                            var V_j = V[j];
                            sum = Vectorized.DotProduct(U_i, V_j, l, n + 1);
                            for (int k = l; k <= n; ++k)
                                V_j[k] += sum * V_i[k];
                        }
                    }
                    for (int j = l; j <= n; ++j)
                        V_i[j] = V[j][i] = 0d;
                }
                V_i[i] = 1d;
                g = rv1[i];
                l = i;
            }
        }

        internal static void LeftTransform(double[][] U, double[] sig, Span<double> U_col_i, int m, int n)
        {
            for (int i = Math.Min(m, n); i >= 0; --i)
            {
                var l = i + 1;
                var g = sig[i];
                var U_i = U[i];
                var U_ii = U_i[i];
                for (int j = l; j <= n; ++j)
                    U_i[j] = 0d;

                //Cache column i of U to optimize access
                CacheUColumn(U, U_col_i, i, m);
                if (g != 0)
                {
                    g = 1d / g;
                    if (l <= n)
                    {
                        var U_li = U_col_i[l];
                        for (int j = l; j <= n; ++j)
                        {
                            var sum = U_li * U[l][j];
                            int k = l + 1;
                            int limit = m - 3;
                            for (; k <= limit; k += 4)
                                sum += U_col_i[k] * U[k][j] +
                                       U_col_i[k + 1] * U[k + 1][j] +
                                       U_col_i[k + 2] * U[k + 2][j] +
                                       U_col_i[k + 3] * U[k + 3][j];

                            for (; k <= m; ++k)
                                sum += U_col_i[k] * U[k][j];

                            var f = (sum / U_ii) * g;
                            for (k = i; k <= m; ++k)
                                U[k][j] += f * U_col_i[k];
                        }
                    }
                    for (int j = i; j <= m; ++j)
                        U_col_i[j] *= g;
                }
                else
                    for (int j = i; j <= m; ++j)
                        U_col_i[j] = 0d;

                U_col_i[i] += 1d;
                //Restore the cached column i of U
                RestoreUColumn(U, U_col_i, i, m);
            }
        }

        internal static void Diagonalize(double[][] U, double[] sig, double[][] V, double[] rv1, int m, int n, double anorm)
        {
            double g;
            for (int k = n; k >= 0; --k)
            {
                for (int iter = 1; iter <= 30; ++iter)
                {
                    var flag = 1;
                    var nm = 0;
                    int l;
                    for (l = k; l >= 0; --l)
                    {
                        if (rv1[l] == 0 || (Math.Abs(rv1[l]) + anorm) == anorm)
                        {
                            flag = 0;
                            break;
                        }
                        if ((Math.Abs(sig[nm]) + anorm) == anorm)
                            break;
                    }
                    nm = l - 1;
                    double f, h, c, s;
                    if (flag != 0)
                    {
                        c = 0d;
                        s = 1d;
                        for (int i = l; i <= k; ++i)
                        {
                            f = s * rv1[i];
                            rv1[i] = c * rv1[i];
                            if ((Math.Abs(f) + anorm) == anorm)
                                break;

                            g = sig[i];
                            h = HpLinearSolver.Hypot(f, g);
                            sig[i] = h;
                            h = 1d / h;
                            c = g * h;
                            s = -f * h;
                            for (int j = 0; j <= m; ++j)
                            {
                                var U_j = U[j];
                                var Uy = U_j[nm];
                                var Uz = U_j[i];
                                U_j[nm] = Uy * c + Uz * s;
                                U_j[i] = Uz * c - Uy * s;
                            }
                        }
                    }
                    var z = sig[k];
                    if (l == k)
                    {
                        if (z < 0d)
                        {
                            sig[k] = -z;
                            var V_k = V[k];
                            for (int j = 0; j <= n; ++j)
                                V_k[j] = -V_k[j];
                        }
                        break;
                    }
                    if (iter == 30)
                        throw new MathParserException("no convergence in 30 SVD iterations.");

                    var x = sig[l];
                    nm = k - 1;
                    var y = sig[nm];
                    g = rv1[nm];
                    h = rv1[k];
                    if (g == 0)
                        f = ((y - z) * (y + z) - h * h) / (h * y * 2d);
                    else
                        f = ((y - z) * (y + z) + (g - h) * (g + h)) / (h * y * 2d);

                    g = HpLinearSolver.Hypot(f, 1d);
                    f = ((x - z) * (x + z) + h * ((y / (f + Math.CopySign(g, f))) - h)) / x;
                    c = 1d;
                    s = 1d;
                    for (int j = l; j <= nm; ++j)
                    {
                        var i = j + 1;
                        g = rv1[i];
                        y = sig[i];
                        h = s * g;
                        g = c * g;
                        z = HpLinearSolver.Hypot(f, h);
                        rv1[j] = z;
                        c = f / z;
                        s = h / z;
                        f = x * c + g * s;
                        g = g * c - x * s;
                        h = y * s;
                        y *= c;
                        var V_j = V[j];
                        var V_i = V[i];
                        for (int jj = 0; jj <= n; ++jj)
                        {
                            x = V_j[jj];
                            z = V_i[jj];
                            V_j[jj] = x * c + z * s;
                            V_i[jj] = z * c - x * s;
                        }
                        z = HpLinearSolver.Hypot(f, h);
                        sig[j] = z;
                        if (z != 0)
                        {
                            z = 1d / z;
                            c = f * z;
                            s = h * z;
                        }
                        f = c * g + s * y;
                        x = c * y - s * g;
                        for (int jj = 0; jj <= m; ++jj)
                        {
                            var U_jj = U[jj];
                            y = U_jj[j];
                            z = U_jj[i];
                            U_jj[j] = y * c + z * s;
                            U_jj[i] = z * c - y * s;
                        }
                    }
                    rv1[l] = 0d;
                    rv1[k] = f;
                    sig[k] = x;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CacheUColumn(double[][] U, Span<double> col_j, int j, int m)
        {
            int k = j;
            for (; k + 3 <= m; k += 4)
            {
                col_j[k] = U[k][j];
                col_j[k + 1] = U[k + 1][j];
                col_j[k + 2] = U[k + 2][j];
                col_j[k + 3] = U[k + 3][j];
            }
            for (; k <= m; ++k)
                col_j[k] = U[k][j];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RestoreUColumn(double[][] U, Span<double> col_j, int j, int m)
        {
            int k = j;
            for (; k + 3 <= m; k += 4)
            {
                U[k][j] = col_j[k];
                U[k + 1][j] = col_j[k + 1];
                U[k + 2][j] = col_j[k + 2];
                U[k + 3][j] = col_j[k + 3];
            }
            for (; k <= m; ++k)
                U[k][j] = col_j[k];
        }
    }
}
