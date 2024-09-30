namespace Calcpad.Tests
{
    public class OperatorTests
    {
        private const double Tol = 1e-15;

        #region Real
        [Fact]
        [Trait("Category", "Real")]
        public void Factorial_0()
        {
            var result = new TestCalc(new()).Run("0!");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Factorial_5()
        {
            var result = new TestCalc(new()).Run("5!");
            Assert.Equal(120d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Factorial_180()
        {
            var e = Assert.Throws<MathParser.MathParserException>(
                () =>
                {
                    new TestCalc(new()).Run("180!");
                }
            );
            Assert.Contains("Argument out of range for n!", e.Message);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Factorial_Minus_10()
        {
            var e = Assert.Throws<MathParser.MathParserException>(
                () =>
                {
                    new TestCalc(new()).Run("(-10)!");
                }
            );
            Assert.Contains("Argument out of range for n!", e.Message);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_2_3()
        {
            var result = new TestCalc(new()).Run("2^3");
            Assert.Equal(8d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_3__Minus_2()
        {
            var result = new TestCalc(new()).Run("3^-2");
            Assert.Equal(1d / 9d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_Minus_3__2()
        {
            var result = new TestCalc(new()).Run("-3^2");
            Assert.Equal(-9d, result);
        }


        [Fact]
        [Trait("Category", "Real")]
        public void Power_Minus_3__4_Brackets()
        {
            var result = new TestCalc(new()).Run("(-3)^4");
            Assert.Equal(81d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_4_3_2()
        {
            var result = new TestCalc(new()).Run("4^3^2");
            Assert.Equal(262144d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_10_310()
        {
            var result = new TestCalc(new()).Run("10^310");
            Assert.Equal(double.PositiveInfinity, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_10__Minus_324()
        {
            var result = new TestCalc(new()).Run("10^-324");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_10_0()
        {
            var result = new TestCalc(new()).Run("10^0");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_0_10()
        {
            var result = new TestCalc(new()).Run("0^10");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_0_0()
        {
            var result = new TestCalc(new()).Run("0^0");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_0_Inf()
        {
            var calc = new TestCalc(new());
            calc.Run("inf = 10^310");
            var result = calc.Run("0^inf");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_1_Inf()
        {
            var calc = new TestCalc(new());
            calc.Run("inf = 10^310");
            var result = calc.Run("1^inf");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Power_Inf_0()
        {
            var calc = new TestCalc(new());
            calc.Run("inf = 10^310");
            var result = calc.Run("inf^0");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Divide_7_5()
        {
            var result = new TestCalc(new()).Run("7/5");
            Assert.Equal(1.4d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Divide_2_3()
        {
            var result = new TestCalc(new()).Run("2÷3");
            Assert.Equal(2d / 3d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Divide_1_0()
        {
            var result = new TestCalc(new()).Run("1/0");
            Assert.Equal(double.PositiveInfinity, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Divide_0_0()
        {
            var result = new TestCalc(new()).Run("0/0");
            Assert.Equal(double.NaN, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Divide_Inf_Inf()
        {
            var result = new TestCalc(new()).Run("10^311/10^310");
            Assert.Equal(double.NaN, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Int_Div_10_3()
        {
            var result = new TestCalc(new()).Run("10\\3");
            Assert.Equal(3d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Mod_7_5()
        {
            var result = new TestCalc(new()).Run("7⦼5");
            Assert.Equal(2d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Mod_2_0()
        {
            var result = new TestCalc(new()).Run("2⦼0");
            Assert.Equal(double.NaN, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Mutiply_2_Minus_3()
        {
            var result = new TestCalc(new()).Run("2*-3");
            Assert.Equal(-6d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Mutiply_Minus_3_Minus_2()
        {
            var result = new TestCalc(new()).Run("-3*-2");
            Assert.Equal(6d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Mutiply_125_080()
        {
            var result = new TestCalc(new()).Run("1.25*0.8");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Mutiply_10_0()
        {
            var result = new TestCalc(new()).Run("10*0");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Multiply_Inf_0()
        {
            var calc = new TestCalc(new());
            calc.Run("inf = 10^310");
            var result = calc.Run("inf*0");
            Assert.Equal(double.NaN, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Multiply_2_3_Div_2_3()
        {
            var result = new TestCalc(new()).Run("2*3 / 2*3");
            Assert.Equal(9d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Multiply_2_3_Fact()
        {
            var result = new TestCalc(new()).Run("2*3!");
            Assert.Equal(12d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Multiply_2_Pow_3_2()
        {
            var result = new TestCalc(new()).Run("2^3*2");
            Assert.Equal(16d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Subtract_5_3()
        {
            var result = new TestCalc(new()).Run("5-3");
            Assert.Equal(2d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Subtract_Minus_5_3()
        {
            var result = new TestCalc(new()).Run("-5 - 3");
            Assert.Equal(-8d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Subtract_5_Plus_3_And_5_Plus_3()
        {
            var result = new TestCalc(new()).Run("5 + 3 - 5 + 3");
            Assert.Equal(6d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Subtract_5_Plus_3_And_5_Plus_3_Brackets()
        {
            var result = new TestCalc(new()).Run("5 + 3 - (5 + 3)");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Subtract_10_Minus_2()
        {
            var result = new TestCalc(new()).Run("10 - -2");
            Assert.Equal(12, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Subtract_Minus_2_2()
        {
            var result = new TestCalc(new()).Run("-2 - 2");
            Assert.Equal(-4d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Subtract_Minus_2_Thirds_1_Ningth()
        {
            var result = new TestCalc(new()).Run("2/3 - 1/9");
            Assert.Equal(2d / 3d - 1d / 9d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Subtract_Inf_Inf()
        {
            var calc = new TestCalc(new());
            calc.Run("inf = 10^310");
            var result = calc.Run("inf - inf");
            Assert.Equal(double.NaN, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Add_5_3()
        {
            var result = new TestCalc(new()).Run("5 + 3");
            Assert.Equal(8d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Add_3_2_Mult_3()
        {
            var result = new TestCalc(new()).Run("3+2 * 3");
            Assert.Equal(9d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Add_5_10_div_2()
        {
            var result = new TestCalc(new()).Run("5+10 / 2");
            Assert.Equal(10d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Equals_01_1_Div_10()
        {
            var result = new TestCalc(new()).Run("0.1 ≡ 1/10");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Equals_1_Div_0_2_Div_0()
        {
            var result = new TestCalc(new()).Run("1/0 ≡ 2/0");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Equals_Sin_Small_Number()
        {
            var result = new TestCalc(new() { Degrees = 1 }).Run("sin(10^-8) ≡ 0.00000001");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Equals_Inf_Inf()
        {
            var calc = new TestCalc(new());
            calc.Run("inf = 10^310");
            var result = calc.Run("inf ≡ inf");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Equals_Inf_1_Div_0()
        {
            var calc = new TestCalc(new());
            calc.Run("inf = 10^310");
            var result = calc.Run("inf ≡ 1/0");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void NotEquals_1_2()
        {
            var result = new TestCalc(new()).Run("1 ≠ 2");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void NotEquals_1_Div_9_0_11()
        {
            var result = new TestCalc(new()).Run("1/9 ≠ 0.1111111111111111");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void LessThan_4_5()
        {
            var result = new TestCalc(new()).Run("4 < 5");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void LessThan_Minus_4_Minus_5()
        {
            var result = new TestCalc(new()).Run("-4 < -5");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void GreaterThan_Minus_2_1()
        {
            var result = new TestCalc(new()).Run("-2 > 1");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void GreaterThan_Minus_1_Minus_2()
        {
            var result = new TestCalc(new()).Run("-1 > -2");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void LessOrEqual_1_Minus_2()
        {
            var result = new TestCalc(new()).Run("1 ≤ -2");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void GreaterOrEqual_2_1()
        {
            var result = new TestCalc(new()).Run("2 ≥ 1");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void And_0_0()
        {
            var result = new TestCalc(new()).Run("0 ∧ 0");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void And_0_1()
        {
            var result = new TestCalc(new()).Run("0 ∧ 1");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void And_2_0()
        {
            var result = new TestCalc(new()).Run("2 ∧ 0");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void And_Minus_1_1()
        {
            var result = new TestCalc(new()).Run("-1 ∧ 1");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Or_0_0()
        {
            var result = new TestCalc(new()).Run("0 ∨ 0");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Or_0_1()
        {
            var result = new TestCalc(new()).Run("0 ∨ 1");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Or_2_0()
        {
            var result = new TestCalc(new()).Run("2 ∨ 0");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Or_Minus_1_1()
        {
            var result = new TestCalc(new()).Run("-1 ∨ 1");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Xor_0_0()
        {
            var result = new TestCalc(new()).Run("0 ⊕ 0");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Xor_0_1()
        {
            var result = new TestCalc(new()).Run("0 ⊕ 1");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Xor_2_0()
        {
            var result = new TestCalc(new()).Run("2 ⊕ 0");
            Assert.Equal(1d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Xor_Minus_1_1()
        {
            var result = new TestCalc(new()).Run("-1 ⊕ 1");
            Assert.Equal(0d, result);
        }

        [Fact]
        [Trait("Category", "Real")]
        public void Assign()
        {
            var calc = new TestCalc(new());
            calc.Run("x = 1");
            var result = calc.Run("x");
            Assert.Equal(1d, result);
        }
        #endregion
    }
}