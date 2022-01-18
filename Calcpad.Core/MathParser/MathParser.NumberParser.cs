﻿using System;

namespace Calcpad.Core
{
    public partial class MathParser
    {
        private static class NumberParser
        {
            private static readonly double[] Pow10 =
{
                0.0,   1.0,   1e-01, 1e-02, 1e-03, 1e-04, 1e-05, 1e-06, 1e-07,
                1e-08, 1e-09, 1e-10, 1e-11, 1e-12, 1e-13, 1e-14, 1e-15, 1e-16,
                1e-17, 1e-18, 1e-19, 1e-20, 1e-21, 1e-22, 1e-23, 1e-24, 1e-25,
                1e-26, 1e-27, 1e-28, 1e-29, 1e-30, 1e-31, 1e-32, 1e-33, 1e-34
            };
            private static readonly long[,] Mult;

            static NumberParser()
            {
                Mult = new long[10, 16];
                long k = 1;
                for (int j = 0; j < 16; ++j)
                {
                    for (int i = 1; i < 10; ++i)
                        Mult[i, j] = i * k;

                    k *= 10;
                }
            }

            internal static Complex Parse(string s)
            {
                var maxDigits = 16;
                var isImaginary = false;
                var digits = s.Length;
                if (s[digits - 1] == 'i')
                {
                    isImaginary = true;
                    --digits;
                }
                var decimalPosition = digits;
                var isLeadingZeros = true;
                var leadingZeros = 0;
                for (int i = 0; i < digits; ++i)
                {
                    if (s[i] == DecimalSymbol)
                    {
                        if (decimalPosition < digits)
                            throw new FormatException();
                        decimalPosition = i;
                    }
                    else if (s[i] != '0')
                        isLeadingZeros = false;

                    if (isLeadingZeros)
                        ++leadingZeros;
                }
                maxDigits += leadingZeros;
                var n = digits;
                if (n > maxDigits)
                    n = maxDigits;
                var n1 = n - 1;
                long k = 0;
                if (decimalPosition > leadingZeros && decimalPosition < n)
                {
                    for (int i = decimalPosition + 1; i < n; ++i)
                        k += Mult[(s[i] - '0'), n1 - i];

                    --n1;
                    for (int i = leadingZeros; i < decimalPosition; ++i)
                        k += Mult[(s[i] - '0'), n1 - i];
                }
                else
                {
                    for (int i = leadingZeros; i < n; ++i)
                        k += Mult[(s[i] - '0'), n1 - i];
                }

                double d;
                if (decimalPosition >= n)
                {
                    d = k;
                    for (int i = n; i < decimalPosition; ++i)
                        d = d * 10.0 + (s[i] - '0');
                }
                else
                {
                    n -= decimalPosition;
                    if (n < Pow10.Length)
                        d = k * Pow10[n];
                    else
                        d = k * Math.Pow(10, -n);
                }

                if (isImaginary)
                    return new Complex(0.0, d);

                return new Complex(d);
            }
        }
    }
}