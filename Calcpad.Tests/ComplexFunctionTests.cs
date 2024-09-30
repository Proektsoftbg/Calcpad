using System.Globalization;

namespace Calcpad.Tests
{
    public class ComplexFunctionTests
    {
        private const double Tol = 1e-14;
        private readonly double _sqrt2 = Math.Sqrt(2);
        private readonly double _sqrt3 = Math.Sqrt(3);

        #region Trig
        [Fact]
        [Trait("Category", "Trig")]
        public void Sin()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("sin(i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.Sinh(1d), im, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Cos()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("cos(i)");
            var im = calc.Imaginary;
            Assert.Equal(Math.Cosh(1d), re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Tan()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("tan(i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.Tanh(1d), im, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Csc()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("csc(i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-1d / Math.Sinh(1d), im, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Sec()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("sec(i)");
            var im = calc.Imaginary;
            Assert.Equal(1d / Math.Cosh(1d), re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Cot()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("cot(i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-1d / Math.Tanh(1d), im, Tol);
        }
        #endregion

        #region Hyp
        [Fact]
        [Trait("Category", "Hyp")]
        public void Sinh()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("sinh(πi/2)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(1d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Cosh()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("cosh(-3*πi/4)");
            var im = calc.Imaginary;
            Assert.Equal(-_sqrt2 / 2d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Tanh()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("Tanh(πi/4)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(1d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Csch()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("Csch(πi/3)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-2d / _sqrt3, im, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Sech()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("sech(-πi/6)");
            var im = calc.Imaginary;
            Assert.Equal(2d / _sqrt3, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Coth()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("Coth(πi/4)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-1d, im, Tol);
        }
        #endregion

        #region InvTrig
        [Fact]
        [Trait("Category", "InvTrig")]
        public void Asin()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("asin(1i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.Log(_sqrt2 + 1d), im, Tol);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Acos()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("acos(1i)");
            var im = calc.Imaginary;
            Assert.Equal(Math.PI / 2d, re, Tol);
            Assert.Equal(Math.Log(_sqrt2 - 1d), im, Tol);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Atan()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("atan(1i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(double.PositiveInfinity, im);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Atan_I_div_2()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("atan(1i/2)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.Log(3d) / 2d, im);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Acsc()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("acsc(1i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-Math.Log(_sqrt2 + 1d), im, Tol);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Acec()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("asec(1i)");
            var im = calc.Imaginary;
            Assert.Equal(Math.PI / 2d, re, Tol);
            Assert.Equal(Math.Log(_sqrt2 + 1d), im, Tol);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Acot_i()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("acot(1i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(double.NegativeInfinity, im, Tol);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Acot_2i()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("acot(2i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-Math.Log(3d) / 2d, im, Tol);
        }
        #endregion

        #region InvHyp
        [Fact]
        [Trait("Category", "InvHyp")]
        public void Asinh()
        {
            var calc = new TestCalc(
                new()
                {
                    Degrees = 1,
                    IsComplex = true
                }
            );
            var re = calc.Run("asinh(i*sqrt(3)/2)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.PI / 3d, im, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acosh()
        {
            var calc = new TestCalc(
               new()
               {
                   Degrees = 1,
                   IsComplex = true
               }
            );
            var re = calc.Run("acosh(sqrt(2)/2)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.PI / 4d, im, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Atanh()
        {
            var calc = new TestCalc(
               new()
               {
                   Degrees = 1,
                   IsComplex = true
               }
            );
            var re = calc.Run("atanh(i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.PI / 4d, im, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acsch()
        {
            var calc = new TestCalc(
               new()
               {
                   Degrees = 1,
                   IsComplex = true
               }
            );
            var re = calc.Run("acsch(i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-Math.PI / 2d, im, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Asech()
        {
            var calc = new TestCalc(
               new()
               {
                   Degrees = 1,
                   IsComplex = true
               }
            );
            var re = calc.Run("asech(sqrt(2))");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.PI / 4d, im, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acoth()
        {
            var calc = new TestCalc(
               new()
               {
                   Degrees = 1,
                   IsComplex = true
               }
            );
            var re = calc.Run("acoth(0)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.PI / 2d, im, Tol);
        }
        #endregion

        #region Exp
        [Fact]
        [Trait("Category", "Exp")]
        public void Ln_Minus_1()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("ln(-1)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.PI, im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Ln_I()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("ln(i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.PI / 2d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Ln_Minus_e_Pow_3()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("ln(-e^3)");
            var im = calc.Imaginary;
            Assert.Equal(3d, re, Tol);
            Assert.Equal(Math.PI, im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Log_I()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("log(i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(Math.PI / (2 * Math.Log(10)), im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Log2_Minus_2()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("log_2(-2)");
            var im = calc.Imaginary;
            Assert.Equal(1d, re, Tol);
            Assert.Equal(Math.PI / Math.Log(2), im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Exp_Minus_PI_I()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("-exp(πi)");
            var im = calc.Imaginary;
            Assert.Equal(1d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Sqrt_I()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("sqrt(i)");
            var im = calc.Imaginary;
            Assert.Equal(1d / _sqrt2, re, Tol);
            Assert.Equal(1d / _sqrt2, im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Sqrt_Minus_4()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("sqrt(-4)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(2d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Sqrt_Minus_2_Plus_2i_Sqrt_3()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("sqrt(-2 + 2*sqrt(3)*i)");
            var im = calc.Imaginary;
            Assert.Equal(1d, re, Tol);
            Assert.Equal(_sqrt3, im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Cbrt_Minus_8()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("cbrt(-8)");
            var im = calc.Imaginary;
            Assert.Equal(1d, re, Tol);
            Assert.Equal(_sqrt3, im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Root_Minus_16()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("root(-16; 4)");
            var im = calc.Imaginary;
            Assert.Equal(_sqrt2, re, Tol);
            Assert.Equal(_sqrt2, im, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Root_Minus_32_5()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("root(-32; 5)");
            var im = calc.Imaginary;
            var sqrt5 = Math.Sqrt(5d);
            Assert.Equal((1d + sqrt5) / 2d, re, Tol);
            Assert.Equal(Math.Sqrt((5d - sqrt5) / 2d), im, Tol);
        }
        #endregion

        #region Round
        [Fact]
        [Trait("Category", "Round")]
        public void Round_Pos()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("round(3.5 + 4.5i)");
            var im = calc.Imaginary;
            Assert.Equal(4d, re, Tol);
            Assert.Equal(5d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Round_Neg()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("round(-4.5i)");
            var im = calc.Imaginary;
            Assert.Equal(0, re, Tol);
            Assert.Equal(-5, im, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Floor_Pos()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("floor(4.8i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(4d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Floor_Neg()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("floor(-2.4 - 4.8i)");
            var im = calc.Imaginary;
            Assert.Equal(-3d, re, Tol);
            Assert.Equal(-5d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Ceiling_Pos()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("ceiling(4.2i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(5d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Ceiling_Neg()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("ceiling(-4.2i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-4d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Trunc_Pos()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("trunc(2.4 + 4.8i)");
            var im = calc.Imaginary;
            Assert.Equal(2d, re, Tol);
            Assert.Equal(4d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Trunc_Neg()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("trunc(-4.8i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-4d, im, Tol);
        }
        #endregion

        #region Int
        [Fact]
        [Trait("Category", "Int")]
        public void Mod()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("mod(7i; 5i)");
            Assert.Equal(double.NaN, re);
        }

        [Fact]
        [Trait("Category", "Int")]
        public void Gcd()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("gcd(18i; 24i)");
            Assert.Equal(double.NaN, re);
        }

        [Fact]
        [Trait("Category", "Int")]
        public void Lcm()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("lcm(6i; 4i)");
            Assert.Equal(double.NaN, re);
        }

        [Fact]
        [Trait("Category", "Int")]
        public void Fact()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var e = Assert.Throws<MathParser.MathParserException>(
               () =>
               {
                   calc.Run("fact(5i)");
               }
            );
            Assert.Contains("The argument of n! cannot be complex.", e.Message);
        }
        #endregion

        #region Complex 
        [Fact]
        [Trait("Category", "Complex")]
        public void Abs()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("abs((3 + 4i))");
            var im = calc.Imaginary;
            Assert.Equal(5d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Complex")]
        public void Re()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("re(3 + 4i)");
            var im = calc.Imaginary;
            Assert.Equal(3d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Complex")]
        public void Im()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("im(3 + 4i)");
            var im = calc.Imaginary;
            Assert.Equal(4d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Complex")]
        public void Phase()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run($"phase({_sqrt3.ToString(CultureInfo.InvariantCulture)}/2 + i/2)");
            var im = calc.Imaginary;
            Assert.Equal(Math.PI / 6d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }
        #endregion

        #region Aggregate
        [Fact]
        [Trait("Category", "Aggregate")]
        public void Min()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("min(1i; 2i)");
            Assert.Equal(double.NaN, re);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Max()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("max(1i; 2i)");
            Assert.Equal(double.NaN, re);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Sum()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("sum(1i; 2i; 3; 4)");
            var im = calc.Imaginary;
            Assert.Equal(7d, re, Tol);
            Assert.Equal(3d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void SumSq()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("sumsq(3i; 4i)");
            var im = calc.Imaginary;
            Assert.Equal(-25d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Srss()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("srss(3i; 4i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(5d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Average()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("average(2i; 4i; 6i; 8i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(5d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Product()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("product(1i; 2i; 3i; 4i; 5i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(120d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Mean()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("mean(2i; 4i; 8i; 16i; 32)");
            var im = calc.Imaginary;
            Assert.Equal(8d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }
        #endregion

        #region List
        [Fact]
        [Trait("Category", "List")]
        public void Take()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("take(2.5; 1i; 2i; 3i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re);
            Assert.Equal(3d, im);
        }

        [Fact]
        [Trait("Category", "List")]
        public void Line()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("line(1.5 + 2.5i; 1 + 4i; 2 + 5i; 3 + 6i)");
            var im = calc.Imaginary;
            Assert.Equal(1.5, re, Tol);
            Assert.Equal(0, im);
        }

        [Fact]
        [Trait("Category", "List")]
        public void Spline()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("spline(1.5 + 2.5i; 2 + 16i; 4 + 32i; 8 + 64i)");
            var im = calc.Imaginary;
            Assert.Equal(2.8125, re, Tol);
            Assert.Equal(0d, im);
        }
        #endregion

        #region Cond
        [Fact]
        [Trait("Category", "Cond")]
        public void If()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("if(1; 1 + 2i; 3i)");
            var im = calc.Imaginary;
            Assert.Equal(1d, re);
            Assert.Equal(2d, im);
        }

        [Fact]
        [Trait("Category", "Cond")]
        public void Switch()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            calc.Run("x = 10");
            var re = calc.Run("switch(x ≤ 1; 1i; x ≤ 2; 2i; x ≤ 3; 3i; 4i)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re);
            Assert.Equal(4d, im);
        }
        #endregion

        #region Other
        [Fact]
        [Trait("Category", "Other")]
        public void Sign_Neg()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("sign(-10i)");
            Assert.Equal(double.NaN, re);
        }

        [Fact]
        [Trait("Category", "Other")]
        public void Sign_Zero()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("sign(0)");
            Assert.Equal(0d, re, Tol);
        }

        [Fact]
        [Trait("Category", "Other")]
        public void Sign_Pos()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("sign(10i)");
            Assert.Equal(double.NaN, re);
        }

        [Fact]
        [Trait("Category", "Other")]
        public void Random()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("random(1)");
            var im = calc.Imaginary;
            Assert.InRange(re, 0, 1);
            Assert.InRange(im, 0, 1);
        }
        #endregion
    }
}