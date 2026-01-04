namespace Calcpad.Tests
{
    public class FunctionTests
    {
        private const double Tol = 1e-14;
        private readonly double _sqrt2 = Math.Sqrt(2);
        private readonly double _sqrt3 = Math.Sqrt(3);
        private const double E = Math.E;
        private static readonly string[] expressions =
        [
            "ReturnAngleUnits = 1",
            "asin(sqrt(3)/2)"
        ];

        #region Trig
        [Fact]
        [Trait("Category", "Trig")]
        public void Sin()
        {
            var result = new TestCalc(
                new()
                {
                    Degrees = 1
                }
            ).Run("sin(pi/2)");
            Assert.Equal(1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Cos()
        {
            var result = new TestCalc(
                new()
                {
                    Degrees = 1
                }
            ).Run("cos(-3*pi/4)");
            Assert.Equal(-_sqrt2 / 2d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Tan()
        {
            var result = new TestCalc(
                new()
                {
                    Degrees = 1
                }
            ).Run("Tan(pi/4)");
            Assert.Equal(1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Tan_Pi_Div_2()
        {
            var result = new TestCalc(
                new()
                {
                    Degrees = 1
                }
            ).Run("Tan(pi/2)");
            Assert.Equal(16331239353195370d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Csc()
        {
            var result = new TestCalc(
                new()
                {
                    Degrees = 1
                }
            ).Run("Csc(π/3)");
            Assert.Equal(2d / _sqrt3, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Sec()
        {
            var result = new TestCalc(
                new()
                {
                    Degrees = 1
                }
            ).Run("SEC(-30°)");
            Assert.Equal(2d / _sqrt3, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Cot_90_deg()
        {
            var result = new TestCalc(new()).Run("Cot(90)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Cot_30_deg()
        {
            var result = new TestCalc(new()).Run("Cot(30)");
            Assert.Equal(_sqrt3, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Csc_0()
        {
            var result = new TestCalc(new()).Run("csc(0)");
            Assert.Equal(double.PositiveInfinity, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Sec_0()
        {
            var result = new TestCalc(new()).Run("sec(0)");
            Assert.Equal(1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Trig")]
        public void Cot_Minus_0()
        {
            var result = new TestCalc(new()).Run("Cot(-0)");
            Assert.Equal(double.NegativeInfinity, result, Tol);
        }
        #endregion

        #region Hyp
        [Fact]
        [Trait("Category", "Hyp")]
        public void Sinh_1()
        {
            var result = new TestCalc(new()).Run("sinh(1)");
            var expected = (E * E - 1d) / (2d * E);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Cosh_1()
        {
            var result = new TestCalc(new()).Run("cosh(1)");
            var expected = (E * E + 1d) / (2d * E);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Tanh_1()
        {
            var result = new TestCalc(new()).Run("Tanh(1)");
            var expected = (E * E - 1d) / (E * E + 1d);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Csch_1()
        {
            var result = new TestCalc(new()).Run("Csch(1)");
            var expected = 2d * E / (E * E - 1d);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Sech_1()
        {
            var result = new TestCalc(new()).Run("sech(1)");
            var expected = 2d * E / (E * E + 1d);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Coth_1()
        {
            var result = new TestCalc(new()).Run("Coth(1)");
            var expected = (E * E + 1d) / (E * E - 1d);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Sinh_0()
        {
            var result = new TestCalc(new()).Run("sinh(0)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Cosh_0()
        {
            var result = new TestCalc(new()).Run("cosh(0)");
            Assert.Equal(1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Tanh_0()
        {
            var result = new TestCalc(new()).Run("Tanh(0)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Csch_0()
        {
            var result = new TestCalc(new()).Run("Csc(0)");
            Assert.Equal(double.PositiveInfinity, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Sech_0()
        {
            var result = new TestCalc(new()).Run("sech(0)");
            Assert.Equal(1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Hyp")]
        public void Coth_0()
        {
            var result = new TestCalc(new()).Run("Coth(0)");
            Assert.Equal(double.PositiveInfinity, result, Tol);
        }
        #endregion

        #region InvTrig
        [Fact]
        [Trait("Category", "InvTrig")]
        public void Asin()
        {
            var calc = new TestCalc(new());
            calc.Run(
                expressions
            );
            Assert.Equal("60°", calc.ToString());
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Acos()
        {
            var result = new TestCalc(new()).Run("acos(-1/2)");
            Assert.Equal(120d, result, 10 * Tol);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Atan()
        {
            var result = new TestCalc(new()).Run("atan(1)");
            Assert.Equal(45d, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Acsc()
        {
            var result = new TestCalc(new()).Run("acsc(2)");
            Assert.Equal(30d, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Acec()
        {
            var result = new TestCalc(new()).Run("asec(sqrt(2))");
            Assert.Equal(45d, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvTrig")]
        public void Acot()
        {
            var result = new TestCalc(new() { IsComplex = true }).Run("acot(0)");
            Assert.Equal(90d, result, Tol);
        }
        #endregion

        #region InvHyp
        [Fact]
        [Trait("Category", "InvHyp")]
        public void Asinh_1()
        {
            var result = new TestCalc(new()).Run("asinh(1)");
            var expected = Math.Log(1 + _sqrt2);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acosh_1()
        {
            var result = new TestCalc(new()).Run("acosh(1)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acosh_2()
        {
            var result = new TestCalc(new()).Run("acosh(2)");
            var expected = Math.Log(2d + _sqrt3);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Atanh_1()
        {
            var result = new TestCalc(new()).Run("atanh(1)");
            Assert.Equal(double.PositiveInfinity, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Atanh_05()
        {
            var result = new TestCalc(new()).Run("atanh(0.5)");
            var expected = Math.Log(3d) / 2d;
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acsch_1()
        {
            var result = new TestCalc(new()).Run("acsch(1)");
            var expected = Math.Log(1d + _sqrt2);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Asech_1()
        {
            var result = new TestCalc(new()).Run("asech(1)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Asech_05()
        {
            var result = new TestCalc(new()).Run("asech(0.5)");
            var expected = Math.Log(2d + _sqrt3);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acoth_1()
        {
            var result = new TestCalc(new()).Run("acoth(1)");
            Assert.Equal(double.PositiveInfinity, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acoth_2()
        {
            var result = new TestCalc(new()).Run("acoth(2)");
            var expected = Math.Log(3d) / 2d;
            Assert.Equal(expected, result, Tol);
        }


        [Fact]
        [Trait("Category", "InvHyp")]
        public void Asinh_0()
        {
            var result = new TestCalc(new()).Run("asinh(0)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acosh_0()
        {
            var result = new TestCalc(new()).Run("acosh(0)");
            Assert.Equal(double.NaN, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Atanh_0()
        {
            var result = new TestCalc(new()).Run("atanh(0)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acsch_0()
        {
            var result = new TestCalc(new()).Run("acsch(0)");
            Assert.Equal(double.PositiveInfinity, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Asech_0()
        {
            var result = new TestCalc(new()).Run("asech(0)");
            Assert.Equal(double.PositiveInfinity, result, Tol);
        }

        [Fact]
        [Trait("Category", "InvHyp")]
        public void Acoth_0()
        {
            var result = new TestCalc(new()).Run("acoth(0)");
            Assert.Equal(double.NaN, result, Tol);
        }
        #endregion

        #region Exp
        [Fact]
        [Trait("Category", "Exp")]
        public void Log_1000()
        {
            var result = new TestCalc(new()).Run("log(1000)");
            Assert.Equal(3d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Ln_e_Pow_3()
        {
            var result = new TestCalc(new()).Run("ln(e^3)");
            Assert.Equal(3d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Ln_Minus_One()
        {
            var result = new TestCalc(new()).Run("ln(-1)");
            Assert.Equal(double.NaN, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Log2_1048576()
        {
            var result = new TestCalc(new()).Run("log_2(1048576)");
            Assert.Equal(20d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Sqrt_25()
        {
            var result = new TestCalc(new()).Run("sqrt(25)");
            Assert.Equal(5d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Sqrt_Minus_4()
        {
            var result = new TestCalc(new()).Run("sqrt(-4)");
            Assert.Equal(double.NaN, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Cbrt_64()
        {
            var result = new TestCalc(new()).Run("cbrt(64)");
            Assert.Equal(4d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Cbrt_Minus_8()
        {
            var result = new TestCalc(new()).Run("cbrt(-8)");
            Assert.Equal(-2d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Root_81_4()
        {
            var result = new TestCalc(new()).Run("root(81; 4)");
            Assert.Equal(3d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Root_Minus_16_4()
        {
            var result = new TestCalc(new()).Run("root(-16; 4)");
            Assert.Equal(double.NaN, result, Tol);
        }

        [Fact]
        [Trait("Category", "Exp")]
        public void Root_Minus_32_5()
        {
            var result = new TestCalc(new()).Run("root(-32; 5)");
            Assert.Equal(-2d, result, Tol);
        }
        #endregion

        #region Round
        [Fact]
        [Trait("Category", "Round")]
        public void Round_Pos()
        {
            var result = new TestCalc(new()).Run("round(4.5)");
            Assert.Equal(5d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Round_Neg()
        {
            var result = new TestCalc(new()).Run("round(-4.5)");
            Assert.Equal(-5d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Floor_Pos()
        {
            var result = new TestCalc(new()).Run("floor(4.8)");
            Assert.Equal(4d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Floor_Neg()
        {
            var result = new TestCalc(new()).Run("floor(-4.8)");
            Assert.Equal(-5d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Ceiling_Pos()
        {
            var result = new TestCalc(new()).Run("ceiling(4.2)");
            Assert.Equal(5d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void Ceiling_Neg()
        {
            var result = new TestCalc(new()).Run("ceiling(-4.2)");
            Assert.Equal(-4d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void TruncPos()
        {
            var result = new TestCalc(new()).Run("trunc(4.8)");
            Assert.Equal(4d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Round")]
        public void TruncNeg()
        {
            var result = new TestCalc(new()).Run("trunc(-4.8)");
            Assert.Equal(-4d, result, Tol);
        }
        #endregion

        #region Int
        [Fact]
        [Trait("Category", "Int")]
        public void Mod()
        {
            var result = new TestCalc(new()).Run("mod(7; 5)");
            Assert.Equal(2d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Int")]
        public void Gcd()
        {
            var result = new TestCalc(new()).Run("gcd(18; 24)");
            Assert.Equal(6d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Int")]
        public void Lcm()
        {
            var result = new TestCalc(new()).Run("lcm(6; 4)");
            Assert.Equal(12d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Int")]
        public void Fact()
        {
            var result = new TestCalc(new()).Run("fact(5)");
            Assert.Equal(120d, result, Tol);
        }
        #endregion

        #region Complex 
        [Fact]
        [Trait("Category", "Complex")]
        public void Abs()
        {
            var result = new TestCalc(new()).Run("abs(-2)");
            Assert.Equal(2d, result);
        }

        [Fact]
        [Trait("Category", "Complex")]
        public void Re()
        {
            var result = new TestCalc(new()).Run("re(1)");
            Assert.Equal(1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Complex")]
        public void Im()
        {
            var result = new TestCalc(new()).Run("im(1)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Complex")]
        public void Phase_1()
        {
            var result = new TestCalc(new()).Run("phase(1)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Complex")]
        public void Phase_Minus_1()
        {
            var result = new TestCalc(new()).Run("phase(-1)");
            Assert.Equal(Math.PI, result, Tol);
        }
        #endregion

        #region Aggregate
        [Fact]
        [Trait("Category", "Aggregate")]
        public void Min()
        {
            var result = new TestCalc(new()).Run("min(1; 2; -3; 0)");
            Assert.Equal(-3d, result);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Max()
        {
            var result = new TestCalc(new()).Run("max(1; 2; -3; 0)");
            Assert.Equal(2d, result);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Sum()
        {
            var result = new TestCalc(new()).Run("sum(1; 2; 3; 4)");
            Assert.Equal(10d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void SumSq()
        {
            var result = new TestCalc(new()).Run("sumsq(3; 4)");
            Assert.Equal(25d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Srss()
        {
            var result = new TestCalc(new()).Run("srss(3; 4)");
            Assert.Equal(5d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Average()
        {
            var result = new TestCalc(new()).Run("average(2; 4; 6; 8)");
            Assert.Equal(5d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Product()
        {
            var result = new TestCalc(new()).Run("product(1; 2; 3; 4; 5)");
            Assert.Equal(120d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Aggregate")]
        public void Mean()
        {
            var result = new TestCalc(new()).Run("mean(2; 4; 8; 16; 32)");
            Assert.Equal(8d, result, Tol);
        }
        #endregion

        #region List
        [Fact]
        [Trait("Category", "List")]
        public void Take()
        {
            var result = new TestCalc(new()).Run("take(2.5; 1; 2; 3)");
            Assert.Equal(3d, result, Tol);
        }

        [Fact]
        [Trait("Category", "List")]
        public void Line()
        {
            var result = new TestCalc(new()).Run("line(1.5; 1; 2; 3)");
            Assert.Equal(1.5, result, Tol);
        }

        [Fact]
        [Trait("Category", "List")]
        public void Spline()
        {
            var result = new TestCalc(new()).Run("spline(1.5; 2; 4; 8)");
            Assert.Equal(2.8125, result, Tol);
        }
        #endregion

        #region Cond
        [Fact]
        [Trait("Category", "Cond")]
        public void If()
        {
            var result = new TestCalc(new()).Run("if(1; 2; 3)");
            Assert.Equal(2d, result);
        }

        [Fact]
        [Trait("Category", "Cond")]
        public void Switch1()
        {
            var result = new TestCalc(new()).Run("switch(0)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Cond")]
        public void Switch2()
        {
            var result = new TestCalc(new()).Run("switch(1; 2)");
            Assert.Equal(2d, result);
        }

        [Fact]
        [Trait("Category", "Cond")]
        public void Switch()
        {
            var result = new TestCalc(new()).Run("switch(0; 1; 0; 2; 1; 3; 4)");
            Assert.Equal(3d, result);
        }
        #endregion

        #region Logic
        [Fact]
        [Trait("Category", "Logic")]
        public void Not_0()
        {
            var result = new TestCalc(new()).Run("not(0)");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void Not_1()
        {
            var result = new TestCalc(new()).Run("not(1)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void And_0_0()
        {
            var result = new TestCalc(new()).Run("and(0; 0)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void And_0_1()
        {
            var result = new TestCalc(new()).Run("and(0; 1)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void And_1_0()
        {
            var result = new TestCalc(new()).Run("and(1; 0)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void And_1_1()
        {
            var result = new TestCalc(new()).Run("and(1; 1)");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void Or_0_0()
        {
            var result = new TestCalc(new()).Run("or(0; 0)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void Or_0_1()
        {
            var result = new TestCalc(new()).Run("or(0; 1)");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void Or_1_0()
        {
            var result = new TestCalc(new()).Run("or(1; 0)");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void Or_1_1()
        {
            var result = new TestCalc(new()).Run("or(1; 1)");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void Xor_0_0()
        {
            var result = new TestCalc(new()).Run("xor(0; 0)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void Xor_0_1()
        {
            var result = new TestCalc(new()).Run("xor(0; 1)");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void Xor_1_0()
        {
            var result = new TestCalc(new()).Run("xor(1; 0)");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Logic")]
        public void Xor_1_1()
        {
            var result = new TestCalc(new()).Run("xor(1; 1)");
            Assert.Equal(0d, result);
        }
        #endregion

        #region Other
        [Fact]
        [Trait("Category", "Other")]
        public void Sign_Neg()
        {
            var result = new TestCalc(new()).Run("sign(-10)");
            Assert.Equal(-1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Other")]
        public void Sign_Zero()
        {
            var result = new TestCalc(new()).Run("sign(0)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Other")]
        public void Sign_Pos()
        {
            var result = new TestCalc(new()).Run("sign(10)");
            Assert.Equal(1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Other")]
        public void Random()
        {
            var result = new TestCalc(new()).Run("random(1)");
            Assert.InRange(result, 0, 1);
        }
        #endregion
    }
}