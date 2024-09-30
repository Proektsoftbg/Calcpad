namespace Calcpad.Tests
{
    public class VectorOperatorTests
    {
        #region Vector
        [Fact]
        [Trait("Category", "Vector")]
        public void VectorAddition()
        {
            string vector1 = "[1; 2; 3; -1; 0; 4]";
            string vector2 = "[4; 3; 3; -1; 2; 2]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1} + {vector2}");
            Assert.Equal("[5 5 6 -2 2 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorSubtraction()
        {
            string vector1 = "[2; 5; 7; 3; 6; 8]";
            string vector2 = "[1; 4; 3; -2; 5; 1]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1} - {vector2}");
            Assert.Equal("[1 1 4 5 1 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorExponentiation()
        {
            string vector = "[2; 3; -1; 4; 0; 6]";
            string exp_vector = "[2; 1; 3; 2; 0; 1]";
            var calc = new TestCalc(new());
            calc.Run($"{vector}^{exp_vector}");

            Assert.Equal("[4 3 -1 16 1 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorDivision()
        {
            string vector1 = "[8; 2; -6; 4; 10; 8]";
            string vector2 = "[2; 1; 3; 10; 3; 4]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}/{vector2}");

            Assert.Equal("[4 2 -2 0.4 3.33 2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorMultiplication()
        {
            string vector1 = "[2; -1; 3; 4; 0; -5]";
            string vector2 = "[7; 1; 9; 22; 1231; -90]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}*{vector2}");

            Assert.Equal("[14 -1 27 88 0 450]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorIntegerDivision()
        {
            string vector1 = "[9; 3; 1; 7; 5]";
            string vector2 = "[2; 1; 2; 4; 5]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1} \\ {vector2}");
            Assert.Equal("[4 3 0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorDivisionBar()
        {
            string vector1= "[10; 5; 2; 8; 6]";
            string vector2 = "[3; 4; 1; 2; 3]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}÷{vector2}");
            Assert.Equal("[3.33 1.25 2 4 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Vector")]
        public void VectorRemainder()
        {
            string vector1 = "[-4.3; 0; 6.9; -2.8; 3]";
            string vector2 = "[8.2; -5; 0; 2.7; -1.4]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}⦼{vector2}");
            Assert.Equal("[-4.3 0  Undefined  -0.1 0.2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorEqual()
        {
            string vector1 = "[8.2; 0; 6.9; -2.8; 3]";
            string vector2 = "[8.2; -5; 6.9; 2.7; 3]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}≡{vector2}");
            string actualResult = calc.ToString();
            string expectedResult = "[1 0 1 0 1]";
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorNotEqual()
        {
            string vector1 = "[2; 0; 6.9; -2.8; 3]";
            string vector2 = "[2; 1; 6.9; -2.8; 3]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}≠{vector2}");
            Assert.Equal("[0 1 0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorLessThan()
        {
            string vector1 = "[0; -6.7; 3.8; -2; 9.5]";
            string vector2 = "[4; 7.2; 0; -1.3; 5]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}<{vector2}");
            Assert.Equal("[1 1 0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorFactorial()
        {
            string vector = "[4; 2; 6; 2; 3]";
            var calc = new TestCalc(new());
            calc.Run($"{vector}!");
            string expected = "[24 2 720 2 6]";
            Assert.Equal(expected, calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorGreaterThan()
        {
            string vector1 = "[4.7; -8; 0; 2.3; -6.5]";
            string vector2 = "[-3; 6.2; 0; -5.1; 7.4]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}>{vector2}");
            Assert.Equal("[1 0 0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorLessOrEqual()
        {
            string vector1 = "[5; -2.1; 0; 7.8; -4.6]";
            string vector2 = "[-7; 1.3; 0; 4.5; -9]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}≤{vector2}");
            Assert.Equal("[0 1 1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorGreaterOrEqual()
        {
            string vector1 = "[20; 16.7; 0; 22.3; 13.5]";
            string vector2 = "[-18; 21.2; 0; -19.3; 25]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1}≥{vector2}");
            Assert.Equal("[1 0 1 1 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Vector")]
        public void VectorLogicalAnd()
        {
            string vector1 = "[0; 16.7; 0; 22.3; 13.5]";
            string vector2 = "[-18; 21.2; 0; -19.3; 25]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1} ∧ {vector2}");
            Assert.Equal("[0 1 0 1 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Vector")]
        public void VectorLogicalOr()
        {
            string vector1 = "[30; 25.8; 0; 28.1; 31.9]";
            string vector2 = "[31.9; 0; 37.8; 27.6]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1} ∨ {vector2}");
            Assert.Equal("[1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void VectorLogicalXor()
        {
            string vector1 = "[6; -2.5; 0; 4.3; -8.1]";
            string vector2 = "[0; 8.6; 0; -5.2; 1.9]";
            var calc = new TestCalc(new());
            calc.Run($"{vector1} ⊕ {vector2}");
            Assert.Equal("[1 0 0 0 0]", calc.ToString());
        }

        #endregion

        #region VectorScalar
        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarAddition()
        {
            string vector = "[5; 12.3; -44; 13; 21.4]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{vector} + {x}");
            Assert.Equal("[12 19.3 -37 20 28.4]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarSubtraction()
        {
            string vector = "[-3; 0; 4.5; -7; 2.25]";
            string x = "-1.5";
            var calc = new TestCalc(new());
            calc.Run($"{vector} - {x}");
            Assert.Equal("[-1.5 1.5 6 -5.5 3.75]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarExponentiation()
        {
            string vector = "[-4.25; 0; 7.89; -10; 3.14159]";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run($"{vector}^{x}");

            Assert.Equal("[-76.77 0 491.17 -1000 31.01]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarDivision()
        {
            string vector = "[0.1; -0.2; 0.3; -0.4; 0.5]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"{vector}/{x}");

            Assert.Equal("[0.05 -0.1 0.15 -0.2 0.25]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarMultiplication()
        {
            string vector = "[0.1; -0.2; 0.3; -0.4; 0.5]";
            string x = "3.5";
            var calc = new TestCalc(new());
            calc.Run($"{vector} * {x}");

            Assert.Equal("[0.35 -0.7 1.05 -1.4 1.75]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarIntegerDivision()
        {
            string vector = "[-0.5; 3; 0; -7.8; 5.6; 4.2; -6; 1.8; -3.1]";
            string x = "2.5";
            var calc = new TestCalc(new());
            calc.Run($"{vector} \\ {x}");
            Assert.Equal("[0 1 0 -3 2 1 -2 0 -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarDivisionBar()
        {
            string vector = "[0.1; -0.2; 0.3; -0.4; 0.5; -5; 8.4; 0]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ÷ {x}");
            Assert.Equal("[0.025 -0.05 0.075 -0.1 0.125 -1.25 2.1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarRemainder()
        {
            string vector = "[-4.3; 0; 6.9; -2.8; 3; -3; 6.5; 0]";
            string x = "2.7";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ⦼ {x}");
            Assert.Equal("[-1.6 0 1.5 -0.1 0.3 -0.3 1.1 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarEqual()
        {
            string vector = "[8.2; 0; 6.9; -2.8; 3; 8.2; -7; 4.6]";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≡ {x}");
            string actualResult = calc.ToString();
            string expectedResult = "[0 0 0 0 1 0 0 0]";
            Assert.Equal(expectedResult, actualResult);
        }
        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarNotEqual()
        {
            string vector = "[-4.3; 0; 6.9; -2.8; 3; -5; 0; 2.7; -1.4]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≠ {x}");
            Assert.Equal("[1 1 1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarLessThan()
        {
            string vector = "[0; -6.7; 3.8; -2; 9.5; 7.2; 0; -1.3; 5]";
            string x = "8";
            var calc = new TestCalc(new());
            calc.Run($"{vector} < {x}");
            Assert.Equal("[1 1 1 1 0 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarGreaterThan()
        {
            string vector = "[4.7; -8; 0; 2.3; -6.5; -3; 6.2; 0; -5.1; 7.4]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"{vector} > {x}");
            Assert.Equal("[1 0 0 1 0 0 1 0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarLessOrEqual()
        {
            string vector = "[5; -2.1; 0; 7.8; -4.6]";
            string x = "6";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≤ {x}");
            Assert.Equal("[1 1 1 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarGreaterOrEqual()
        {
            string vector = "[-40; 16.7; 0; 22.3; 13.5; 21.2; 0; -19.3; 25]";
            string x = "10";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≥ {x}");
            Assert.Equal("[0 1 0 1 1 1 0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarLogicalAnd()
        {
            string vector = "[0; 16.7; 0; 22.3; 13.5; -18; 21.2; 0; -19.3; 25]";
            string x = "5";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ∧ {x}");
            Assert.Equal("[0 1 0 1 1 1 1 0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarLogicalOr()
        {
            string vector = "[0; 16.7; 0; 22.3; 13.5; -18; 21.2; 0; -19.3; 25]";
            string x = "-10";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ∨ {x}");
            Assert.Equal("[1 1 1 1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorScalar")]
        public void VectorScalarLogicalXor()
        {
            string vector = "[6; -2.5; 0; 4.3; -8.1; 8.6; 0; -5.2; 1.9]";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ⊕ {x}");
            Assert.Equal("[0 0 1 0 0 0 1 0 0]", calc.ToString());
        }
        #endregion

        #region ScalarVector
        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorAddition()
        {
            string x = "7";
            string vector = "[5; 12.3; -44; 13; 21.4]";
            var calc = new TestCalc(new());
            calc.Run($"{x} + {vector}");
            Assert.Equal("[12 19.3 -37 20 28.4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorSubtraction()
        {
            string x = "5.5";
            string vector = "[-3; 0; 4.5; -7; 2.25]";
            var calc = new TestCalc(new());
            calc.Run($"{x} - {vector}");
            Assert.Equal("[8.5 5.5 1 12.5 3.25]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorExponentiation()
        {
            string x = "3";
            string vector = "[-4.25; 0; 7.89; 2; 3.14159]";
            var calc = new TestCalc(new());
            calc.Run($"{x}^{vector}");

            Assert.Equal("[0.00938 1 5814.16 9 31.54]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorDivision()
        {
            string x = "5";
            string vector = "[0.1; -0.2; 0.3; -0.4; 0.5]";
            var calc = new TestCalc(new());
            calc.Run($"{x}/{vector}");

            Assert.Equal("[50 -25 16.67 -12.5 10]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorMultiplication()
        {
            string x = "4.5";
            string vector = "[0.1; -0.2; 0.3; -0.4; 0.5]";
            var calc = new TestCalc(new());
            calc.Run($"{x}*{vector}");

            Assert.Equal("[0.45 -0.9 1.35 -1.8 2.25]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorIntegerDivision()
        {
            string x = "8.25";
            string vector = "[-0.5; 3; 0; -7.8; 5.6; 4.2; -6; 1.8; -3.1]";
            var calc = new TestCalc(new());
            calc.Run($"{x} \\ {vector}");
            Assert.Equal("[-16 2  Undefined  -1 1 1 -1 4 -2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorDivisionBar()
        {
            string x = "5";
            string vector = "[0.1; -0.2; 0.3; -0.4; 0.5; -5; 8.4; 0]";
            var calc = new TestCalc(new());
            calc.Run($"{x}÷{vector}");
            Assert.Equal("[50 -25 16.67 -12.5 10 -1 0.595 +∞]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorRemainder()
        {
            string x = "3.2";
            string vector = "[-4.3; 0; 6.9; -2.8; 3; -3; 6.5; 0]";
            var calc = new TestCalc(new());
            calc.Run($"{x}⦼{vector}");
            Assert.Equal("[3.2  Undefined  3.2 0.4 0.2 0.2 3.2  Undefined ]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorEqual()
        {
            string x = "5.5";
            string vector = "[5.5; 2.1; 7.9; -1.5; 5.5; 6.7; -5; 5.5]";
            var calc = new TestCalc(new());
            calc.Run($"{x}≡{vector}");
            string actualResult = calc.ToString();
            string expectedResult = "[1 0 0 0 1 0 0 1]";
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorNotEqual()
        {
            string x = "3";
            string vector = "[-4.3; 0; 6.9; -2.8; 3; -5; 0; 2.7; -1.4]";
            var calc = new TestCalc(new());
            calc.Run($"{x}≠{vector}");
            Assert.Equal("[1 1 1 1 0 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorLessThan()
        {
            string x = "6";
            string vector = "[0; -6.7; 3.8; -2; 9.5; 7.2; 0; -1.3; 5]";
            var calc = new TestCalc(new());
            calc.Run($"{x}<{vector}");
            Assert.Equal("[0 0 0 0 1 1 0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorGreaterThan()
        {
            string x = "-1";
            string vector = "[4.7; -8; 0; 2.3; -6.5; -3; 6.2; 0; -5.1; 7.4]";
            var calc = new TestCalc(new());
            calc.Run($"{x}>{vector}");
            Assert.Equal("[0 1 0 0 1 1 0 0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorLessOrEqual()
        {
            string x = "3";
            string vector = "[5; -2.1; 0; 7.8; -4.6]";
            var calc = new TestCalc(new());
            calc.Run($"{x}≤{vector}");
            Assert.Equal("[1 0 0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorGreaterOrEqual()
        {
            string x = "0";
            string vector = "[-40; 16.7; 0; 22.3; 13.5; 21.2; 0; -19.3; 25]";
            var calc = new TestCalc(new());
            calc.Run($"{x}≥{vector}");
            Assert.Equal("[1 0 1 0 0 0 1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorLogicalAnd()
        {
            string x = "5";
            string vector = "[0; 16.7; 0; 22.3; 13.5; -18; 21.2; 0; -19.3; 25]";
            var calc = new TestCalc(new());
            calc.Run($"{x} ∧ {vector}");
            Assert.Equal("[0 1 0 1 1 1 1 0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorLogicalOr()
        {
            string x = "5";
            string vector = "[0; 16.7; 0; 22.3; 13.5; -18; 21.2; 0; -19.3; 25]";
            var calc = new TestCalc(new());
            calc.Run($"{x} ∨ {vector}");
            Assert.Equal("[1 1 1 1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarVector")]
        public void ScalarVectorLogicalXor()
        {
            string x = "5";
            string vector = "[6; -2.5; 0; 4.3; -8.1; 8.6; 0; -5.2; 1.9]";
            var calc = new TestCalc(new());
            calc.Run($"{x} ⊕ {vector}");
            Assert.Equal("[0 0 1 0 0 0 1 0 0]", calc.ToString());
        }
        #endregion
    }
}
