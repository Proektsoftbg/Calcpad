namespace Calcpad.Tests
{
    public class HPVectorOperatorInFunctionTests
    {
        #region HPVector
        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorAddition()
        {
            string vector1 = "hp([1; 2; 3; -1; 0; 4])";
            string vector2 = "hp([4; 3; 3; -1; 2; 2])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({vector1};{vector2})"]);
        }
        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorSubtraction()
        {
            string vector1 = "hp([2; 5; 7; 3; 6; 8])";
            string vector2 = "hp([1; 4; 3; -2; 5; 1])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({vector1};{vector2})"]);
            Assert.Equal("[1 1 4 5 1 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorExponentiation()
        {
            string vector = "hp([2; 3; -1; 4; 0; 6])";
            string exp_vector = "hp([2; 1; 3; 2; 0; 1])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({vector};{exp_vector})"]);

            Assert.Equal("[4 3 -1 16 1 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorDivision()
        {
            string vector1 = "hp([8; 2; -6; 4; 10; 8])";
            string vector2 = "hp([2; 1; 3; 10; 3; 4])";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}/{vector2}");

            Assert.Equal("[4 2 -2 0.4 3.33 2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorMultiplication()
        {
            string vector1 = "hp([2; -1; 3; 4; 0; -5])";
            string vector2 = "hp([7; 1; 9; 22; 1231; -90])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a * b", $"f({vector1};{vector2})"]);

            Assert.Equal("[14 -1 27 88 0 450]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorIntegerDivision()
        {
            string vector1 = "hp([9; 3; 1; 7; 5])";
            string vector2 = "hp([2; 1; 2; 4; 5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({vector1};{vector2})"]);
            Assert.Equal("[4 3 0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorDivisionBar()
        {
            string vector1 = "hp([10; 5; 2; 8; 6])";
            string vector2 = "hp([3; 4; 1; 2; 3])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({vector1};{vector2})"]);
            Assert.Equal("[3.33 1.25 2 4 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorRemainder()
        {
            string vector1 = "hp([-4.3; 0; 6.9; -2.8; 3])";
            string vector2 = "hp([8.2; -5; 0; 2.7; -1.4])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({vector1};{vector2})"]);
            Assert.Equal("[-4.3 0  Undefined  -0.1 0.2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorEqual()
        {
            string vector1 = "hp([8.2; 0; 6.9; -2.8; 3])";
            string vector2 = "hp([8.2; -5; 6.9; 2.7; 3])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≡ b", $"f({vector1};{vector2})"]);
            string actualResult = calc.ToString();
            string expectedResult = "[1 0 1 0 1]";
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorNotEqual()
        {
            string vector1 = "hp([2; 0; 6.9; -2.8; 3])";
            string vector2 = "hp([2; 1; 6.9; -2.8; 3])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≠ b", $"f({vector1};{vector2})"]);
            Assert.Equal("[0 1 0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorLessThan()
        {
            string vector1 = "hp([0; -6.7; 3.8; -2; 9.5])";
            string vector2 = "hp([4; 7.2; 0; -1.3; 5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({vector1};{vector2})"]);
            Assert.Equal("[1 1 0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorFactorial()
        {
            string vector = "hp([4; 2; 6; 2; 3])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = a!", $"f({vector})"]);
            string expected = "[24 2 720 2 6]";
            Assert.Equal(expected, calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorGreaterThan()
        {
            string vector1 = "hp([4.7; -8; 0; 2.3; -6.5])";
            string vector2 = "hp([-3; 6.2; 0; -5.1; 7.4])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({vector1};{vector2})"]);
            Assert.Equal("[1 0 0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorLessOrEqual()
        {
            string vector1 = "hp([5; -2.1; 0; 7.8; -4.6])";
            string vector2 = "hp([-7; 1.3; 0; 4.5; -9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({vector1};{vector2})"]);
            Assert.Equal("[0 1 1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorGreaterOrEqual()
        {
            string vector1 = "hp([20; 16.7; 0; 22.3; 13.5])";
            string vector2 = "hp([-18; 21.2; 0; -19.3; 25])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({vector1};{vector2})"]);
            Assert.Equal("[1 0 1 1 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorLogicalAnd()
        {
            string vector1 = "hp([0; 16.7; 0; 22.3; 13.5])";
            string vector2 = "hp([-18; 21.2; 0; -19.3; 25])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∧ b", $"f({vector1};{vector2})"]);
            Assert.Equal("[0 1 0 1 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorLogicalOr()
        {
            string vector1 = "hp([30; 25.8; 0; 28.1; 31.9])";
            string vector2 = "hp([31.9; 0; 37.8; 27.6])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∨ b", $"f({vector1};{vector2})"]);
            Assert.Equal("[1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVector")]
        public void HPVectorLogicalXor()
        {
            string vector1 = "hp([6; -2.5; 0; 4.3; -8.1])";
            string vector2 = "hp([0; 8.6; 0; -5.2; 1.9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⊕ b", $"f({vector1};{vector2})"]);
            Assert.Equal("[1 0 0 0 0]", calc.ToString());
        }

        #endregion

        #region HPVectorScalar
        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarAddition()
        {
            string vector = "hp([5; 12.3; -44; 13; 21.4])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({vector};{x})"]);
            Assert.Equal("[12 19.3 -37 20 28.4]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarSubtraction()
        {
            string vector = "hp([-3; 0; 4.5; -7; 2.25])";
            string x = "-1.5";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({vector};{x})"]);
            Assert.Equal("[-1.5 1.5 6 -5.5 3.75]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarExponentiation()
        {
            string vector = "hp([-4.25; 0; 7.89; -10; 3.14159])";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({vector};{x})"]);

            Assert.Equal("[-76.77 0 491.17 -1000 31.01]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarDivision()
        {
            string vector = "hp([0.1; -0.2; 0.3; -0.4; 0.5])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({vector};{x})"]);

            Assert.Equal("[0.05 -0.1 0.15 -0.2 0.25]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarMultiplication()
        {
            string vector = "hp([0.1; -0.2; 0.3; -0.4; 0.5])";
            string x = "3.5";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a * b", $"f({vector};{x})"]);

            Assert.Equal("[0.35 -0.7 1.05 -1.4 1.75]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarIntegerDivision()
        {
            string vector = "hp([-0.5; 3; 0; -7.8; 5.6; 4.2; -6; 1.8; -3.1])";
            string x = "2.5";
            var calc = new TestCalc(new());
            calc.Run($"{vector} \\ {x}");
            Assert.Equal("[0 1 0 -3 2 1 -2 0 -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarDivisionBar()
        {
            string vector = "hp([0.1; -0.2; 0.3; -0.4; 0.5; -5; 8.4; 0])";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ÷ {x}");
            Assert.Equal("[0.025 -0.05 0.075 -0.1 0.125 -1.25 2.1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarRemainder()
        {
            string vector = "hp([-4.3; 0; 6.9; -2.8; 3; -3; 6.5; 0])";
            string x = "2.7";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ⦼ {x}");
            Assert.Equal("[-1.6 0 1.5 -0.1 0.3 -0.3 1.1 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarEqual()
        {
            string vector = "hp([8.2; 0; 6.9; -2.8; 3; 8.2; -7; 4.6])";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≡ {x}");
            string actualResult = calc.ToString();
            string expectedResult = "[0 0 0 0 1 0 0 0]";
            Assert.Equal(expectedResult, actualResult);
        }
        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarNotEqual()
        {
            string vector = "hp([-4.3; 0; 6.9; -2.8; 3; -5; 0; 2.7; -1.4])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≠ {x}");
            Assert.Equal("[1 1 1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarLessThan()
        {
            string vector = "hp([0; -6.7; 3.8; -2; 9.5; 7.2; 0; -1.3; 5])";
            string x = "8";
            var calc = new TestCalc(new());
            calc.Run($"{vector} < {x}");
            Assert.Equal("[1 1 1 1 0 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarGreaterThan()
        {
            string vector = "hp([4.7; -8; 0; 2.3; -6.5; -3; 6.2; 0; -5.1; 7.4])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"{vector} > {x}");
            Assert.Equal("[1 0 0 1 0 0 1 0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarLessOrEqual()
        {
            string vector = "hp([5; -2.1; 0; 7.8; -4.6])";
            string x = "6";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≤ {x}");
            Assert.Equal("[1 1 1 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarGreaterOrEqual()
        {
            string vector = "hp([-40; 16.7; 0; 22.3; 13.5; 21.2; 0; -19.3; 25])";
            string x = "10";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≥ {x}");
            Assert.Equal("[0 1 0 1 1 1 0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarLogicalAnd()
        {
            string vector = "hp([0; 16.7; 0; 22.3; 13.5; -18; 21.2; 0; -19.3; 25])";
            string x = "5";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ∧ {x}");
            Assert.Equal("[0 1 0 1 1 1 1 0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarLogicalOr()
        {
            string vector = "hp([0; 16.7; 0; 22.3; 13.5; -18; 21.2; 0; -19.3; 25])";
            string x = "-10";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ∨ {x}");
            Assert.Equal("[1 1 1 1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorScalar")]
        public void HPVectorScalarLogicalXor()
        {
            string vector = "hp([6; -2.5; 0; 4.3; -8.1; 8.6; 0; -5.2; 1.9])";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ⊕ {x}");
            Assert.Equal("[0 0 1 0 0 0 1 0 0]", calc.ToString());
        }
        #endregion

        #region HPScalarVector
        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorAddition()
        {
            string x = "7";
            string vector = "hp([5; 12.3; -44; 13; 21.4])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({x};{vector})"]);
            Assert.Equal("[12 19.3 -37 20 28.4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorSubtraction()
        {
            string x = "5.5";
            string vector = "hp([-3; 0; 4.5; -7; 2.25])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({x};{vector})"]);
            Assert.Equal("[8.5 5.5 1 12.5 3.25]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorExponentiation()
        {
            string x = "3";
            string vector = "hp([-4.25; 0; 7.89; 2; 3.14159])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({x};{vector})"]);

            Assert.Equal("[0.00938 1 5814.16 9 31.54]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorDivision()
        {
            string x = "5";
            string vector = "hp([0.1; -0.2; 0.3; -0.4; 0.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({x};{vector})"]);

            Assert.Equal("[50 -25 16.67 -12.5 10]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorMultiplication()
        {
            string x = "4.5";
            string vector = "hp([0.1; -0.2; 0.3; -0.4; 0.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a * b", $"f({x};{vector})"]);

            Assert.Equal("[0.45 -0.9 1.35 -1.8 2.25]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorIntegerDivision()
        {
            string x = "8.25";
            string vector = "hp([-0.5; 3; 0; -7.8; 5.6; 4.2; -6; 1.8; -3.1])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({x};{vector})"]);
            Assert.Equal("[-16 2  Undefined  -1 1 1 -1 4 -2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorDivisionBar()
        {
            string x = "5";
            string vector = "hp([0.1; -0.2; 0.3; -0.4; 0.5; -5; 8.4; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({x};{vector})"]);
            Assert.Equal("[50 -25 16.67 -12.5 10 -1 0.595 +∞]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorRemainder()
        {
            string x = "3.2";
            string vector = "hp([-4.3; 0; 6.9; -2.8; 3; -3; 6.5; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({x};{vector})"]);
            Assert.Equal("[3.2  Undefined  3.2 0.4 0.2 0.2 3.2  Undefined ]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorEqual()
        {
            string x = "5.5";
            string vector = "hp([5.5; 2.1; 7.9; -1.5; 5.5; 6.7; -5; 5.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≡ b", $"f({x};{vector})"]);
            string actualResult = calc.ToString();
            string expectedResult = "[1 0 0 0 1 0 0 1]";
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorNotEqual()
        {
            string x = "3";
            string vector = "hp([-4.3; 0; 6.9; -2.8; 3; -5; 0; 2.7; -1.4])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≠ b", $"f({x};{vector})"]);
            Assert.Equal("[1 1 1 1 0 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorLessThan()
        {
            string x = "6";
            string vector = "hp([0; -6.7; 3.8; -2; 9.5; 7.2; 0; -1.3; 5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({x};{vector})"]);
            Assert.Equal("[0 0 0 0 1 1 0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorGreaterThan()
        {
            string x = "-1";
            string vector = "hp([4.7; -8; 0; 2.3; -6.5; -3; 6.2; 0; -5.1; 7.4])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({x};{vector})"]);
            Assert.Equal("[0 1 0 0 1 1 0 0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorLessOrEqual()
        {
            string x = "3";
            string vector = "hp([5; -2.1; 0; 7.8; -4.6])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({x};{vector})"]);
            Assert.Equal("[1 0 0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorGreaterOrEqual()
        {
            string x = "0";
            string vector = "hp([-40; 16.7; 0; 22.3; 13.5; 21.2; 0; -19.3; 25])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({x};{vector})"]);
            Assert.Equal("[1 0 1 0 0 0 1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorLogicalAnd()
        {
            string x = "5";
            string vector = "hp([0; 16.7; 0; 22.3; 13.5; -18; 21.2; 0; -19.3; 25])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∧ b", $"f({x};{vector})"]);
            Assert.Equal("[0 1 0 1 1 1 1 0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorLogicalOr()
        {
            string x = "5";
            string vector = "hp([0; 16.7; 0; 22.3; 13.5; -18; 21.2; 0; -19.3; 25])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∨ b", $"f({x};{vector})"]);
            Assert.Equal("[1 1 1 1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarVector")]
        public void HPScalarVectorLogicalXor()
        {
            string x = "5";
            string vector = "hp([6; -2.5; 0; 4.3; -8.1; 8.6; 0; -5.2; 1.9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⊕ b", $"f({x};{vector})"]);
            Assert.Equal("[0 0 1 0 0 0 1 0 0]", calc.ToString());
        }
        #endregion

    }
}
