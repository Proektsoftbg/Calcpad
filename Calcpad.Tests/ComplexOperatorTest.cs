namespace Calcpad.Tests
{
    public class ComplexOperatorTests
    {
        private const double Tol = 1e-14;
        private readonly double _sqrt2 = Math.Sqrt(2);
        private readonly double _sqrt3 = Math.Sqrt(3);

        #region Power
        [Fact]
        [Trait("Category", "Power")]
        public void Factorial()
        {
            var e = Assert.Throws<MathParser.MathParserException>(
                () =>
                {
                    new TestCalc(new() { IsComplex = true }).Run("1i!");
                }
            );
            Assert.Contains("The argument of n! cannot be complex.", e.Message);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_i_2()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("1i^2");
            var im = calc.Imaginary;
            Assert.Equal(-1d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_i_i()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("1i^1i");
            var im = calc.Imaginary;
            Assert.Equal(Math.Exp(-Math.PI / 2d), re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_i__Minus_1()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("1i^-1");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-1d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_i_0()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("1i^0");
            var im = calc.Imaginary;
            Assert.Equal(1d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_e__2Pi_i()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("e^(2*πi)");
            var im = calc.Imaginary;
            Assert.Equal(1d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Minus_Pow_e__Pi_i_Plus_1()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("-e^(πi + 1)");
            var im = calc.Imaginary;
            Assert.Equal(Math.E, re, Tol);
            Assert.Equal(0d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_Minus_1__1_Div_2()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(-1)^(1/2)");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(1d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_Minus_8__1_Div_3()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(-8)^(1/3)");
            var im = calc.Imaginary;
            Assert.Equal(1d, re, Tol);
            Assert.Equal(_sqrt3, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_Minus_16__1_Div_4()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(-16)^(1/4)");
            var im = calc.Imaginary;
            Assert.Equal(_sqrt2, re, Tol);
            Assert.Equal(_sqrt2, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_Minus_32__1_Div_5()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(-32)^(1/5)");
            var im = calc.Imaginary;
            var sqrt5 = Math.Sqrt(5d);
            Assert.Equal((1d + sqrt5) / 2d, re, Tol);
            Assert.Equal(Math.Sqrt((5d - sqrt5) / 2d), im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_2_Plus_3i__2()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(2 + 3i)^2");
            var im = calc.Imaginary;
            Assert.Equal(-5d, re, Tol);
            Assert.Equal(12d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_1_Plus_i__Minus_2()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(1 + 1i)^-2");
            var im = calc.Imaginary;
            Assert.Equal(0d, re, Tol);
            Assert.Equal(-1d / 2d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_1_Plus_i__3()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(1 + 1i)^3");
            var im = calc.Imaginary;
            Assert.Equal(-2d, re, Tol);
            Assert.Equal(2d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Power")]
        public void Pow_3_Minus_2i__3()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(3 - 2i)^3");
            var im = calc.Imaginary;
            Assert.Equal(-9d, re, Tol);
            Assert.Equal(-46d, im, Tol);
        }
        #endregion

        #region Division
        [Fact]
        [Trait("Category", "Division")]
        public void Div_I_0()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("1i/0");
            var im = calc.Imaginary;
            Assert.Equal(double.PositiveInfinity, re, Tol);
            Assert.Equal(double.PositiveInfinity, im, Tol);
        }

        [Fact]
        [Trait("Category", "Division")]
        public void Div_3_Plus_4i__1_Minus_2i()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(3 + 4i)/(1 - 2i)");
            var im = calc.Imaginary;
            Assert.Equal(-1d, re, Tol);
            Assert.Equal(2d, im, Tol);
        }
        #endregion

        #region Multiplication
        [Fact]
        [Trait("Category", "Multiplication")]
        public void Mult_i_i()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("1i*1i");
            var im = calc.Imaginary;
            Assert.Equal(-1d, re, Tol);
            Assert.Equal(0d, im, Tol);

        }

        [Fact]
        [Trait("Category", "Multiplication")]
        public void Mult_3_Plus_4i__3_Minus_4i()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(3 + 4i)*(3 - 4i)");
            var im = calc.Imaginary;
            Assert.Equal(25d, re, Tol);
            Assert.Equal(0d, im, Tol);
        }
        #endregion

        #region AdditionAndSubtraction 
        [Fact]
        [Trait("Category", "Addition")]
        public void Add_3_Plus_4i__2_Minus_3i()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(3 + 4i) + (2 - 3i)");
            var im = calc.Imaginary;
            Assert.Equal(5d, re, Tol);
            Assert.Equal(1d, im, Tol);
        }

        [Fact]
        [Trait("Category", "Subtraction")]
        public void Subt_3_Plus_4i__2_Plus_3i()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(3 + 4i) - (2 + 3i)");
            var im = calc.Imaginary;
            Assert.Equal(1d, re, Tol);
            Assert.Equal(1d, im, Tol);

        }
        #endregion

        #region Comparison
        [Fact]
        [Trait("Category", "Comparison")]
        public void Equals_1l__Sqrt_Minus_1()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("1i ≡ sqrt(-1)");
            Assert.Equal(1d, re);

        }

        [Fact]
        [Trait("Category", "Comparison")]
        public void Equals_e_Pow_Pi_i__Minus_1()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("e^πi ≡ -1");
            Assert.Equal(1d, re);

        }

        [Fact]
        [Trait("Category", "Comparison")]
        public void Equals_3_Plus_4i__4i_Plus_3()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("3 + 4i ≡ 4i + 3");
            Assert.Equal(1d, re);

        }

        [Fact]
        [Trait("Category", "Comparison")]
        public void NotEquals_i_1()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("1i ≠ 1");
            Assert.Equal(1d, re);

        }

        [Fact]
        [Trait("Category", "Comparison")]
        public void GreaterThan()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(3 + 4i) > (2 + 3i)");
            Assert.Equal(double.NaN, re);

        }

        [Fact]
        [Trait("Category", "Comparison")]
        public void GreaterOrEquals()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("(3 + 4i) ≥ (2 + 3i)");
            Assert.Equal(double.NaN, re);

        }

        [Fact]
        [Trait("Category", "Comparison")]
        public void LessThan()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("-(3 + 2i) < (4 + 3i)");
            Assert.Equal(double.NaN, re);

        }

        [Fact]
        [Trait("Category", "Comparison")]
        public void LessOrEquals_Real()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("2 + 2 ≤ 2*2");
            Assert.Equal(1d, re);

        }

        [Fact]
        [Trait("Category", "Comparison")]
        public void LessOrEquals_WhenEqual()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            var re = calc.Run("3 + 4i ≤ 4i + 3");
            Assert.Equal(double.NaN, re);

        }
        #endregion
    }
}