namespace Calcpad.Tests
{
    public class HPDiagMatrixOperatorInFunctionTests
    {
        #region HPMatrix
        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([4|0; 5|0; 0; 6]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a + b", $"f(D1;D2)"
            ]);
            Assert.Equal("[5 0 0|0 7 0|0 0 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9|0; 6|0; 0; 8]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([4|0; 3|0; 0; 5]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a - b", $"f(D1;D2)"
            ]);
            Assert.Equal("[5 0 0|0 3 0|0 0 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 3|0; 0; 4]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([3|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ^ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[8 1 1|1 9 1|1 1 64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8|0; 7|0; 0; 6]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([4|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a / b", $"f(D1;D2)"
            ]);
            Assert.Equal("[2  Undefined   Undefined | Undefined  3.5  Undefined | Undefined   Undefined  2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5|0; 4|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([3|0; 6|0; 0; 2]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a * b", $"f(D1;D2)"
            ]);
            Assert.Equal("[15 0 0|0 24 0|0 0 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9|0; 7|0; 0; 8]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([3|0; 2|0; 0; 4]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a \\ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[3  Undefined   Undefined | Undefined  3  Undefined | Undefined   Undefined  2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8|0; 6|0; 0; 9]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([4|0; 3|0; 0; 5]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ÷ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[2  Undefined   Undefined | Undefined  2  Undefined | Undefined   Undefined  1.8]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HP  Matrix")]
        public void HPDiagMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([10|0; 9|0; 0; 7]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([4|0; 5|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ⦼ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[2  Undefined   Undefined | Undefined  4  Undefined | Undefined   Undefined  1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixFactorial()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3|0; 4|0; 0; 5]; diagonal_hp(3; 1); 1; 1)",
                "f(a) = a!", $"f(D1)"
            ]);
            Assert.Equal("[6 1 1|1 24 1|1 1 120]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 4|0; 0; 6]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([3|0; 5|0; 0; 7]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ≡ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 1 1|1 0 1|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3|0; 5|0; 0; 7]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([2|0; 4|0; 0; 6]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ≠ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 3|0; 0; 4]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([5|0; 6|0; 0; 7]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a < b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7|0; 8|0; 0; 9]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([3|0; 4|0; 0; 5]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a > b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 4|0; 0; 6]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([5|0; 6|0; 0; 8]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ≤ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([6|0; 7|0; 0; 8]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([3|0; 4|0; 0; 5]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ≥ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([0|0; 1|0; 0; 1]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ∧ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([0|0; 1|0; 0; 1]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([1|0; 0|0; 0; 1]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ∨ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPDiagMatrixLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 1|0; 0; 1]; diagonal_hp(3; 1); 1; 1)",
                "D2 = copy([0|0; 1|0; 0; 0]; diagonal_hp(3; 1); 1; 1)",
                "f(a; b) = a ⊕ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|0 0 0|0 0 1]", calc.ToString());
        }
        #endregion

        #region HPMatrixScalar
        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 3|0; 0; 4]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 5",
                "f(a; b) = a + b", $"f(D1;D2)"
            ]);
            Assert.Equal("[7 5 5|5 8 5|5 5 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8|0; 9|0; 0; 10]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 3",
                "f(a; b) = a - b", $"f(D1;D2)"
            ]);
            Assert.Equal("[5 -3 -3|-3 6 -3|-3 -3 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 3|0; 0; 4]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 3",
                "f(a; b) = a ^ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[8 0 0|0 27 0|0 0 64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([6|0; 8|0; 0; 12]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 2",
                "f(a; b) = a / b", $"f(D1;D2)"
            ]);
            Assert.Equal("[3 0 0|0 4 0|0 0 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 4|0; 0; 5]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 3",
                "f(a; b) = a * b", $"f(D1;D2)"
            ]);
            Assert.Equal("[6 0 0|0 12 0|0 0 15]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8|0; 10|0; 0; 12]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 4",
                "f(a; b) = a \\ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[2 0 0|0 2 0|0 0 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([6|0; 8|0; 0; 10]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ÷ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[3 0 0|0 4 0|0 0 5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7|0; 10|0; 0; 13]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 4",
                "f(a; b) = a ⦼ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[3 0 0|0 2 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 1",
                "f(a; b) = a ≡ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ≠ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 0 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 4",
                "f(a; b) = a < b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 11",
                "f(a; b) = a > b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ≤ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 3",
                "f(a; b) = a ≥ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 1|0; 0; 1]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 0",
                "f(a; b) = a ∧ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 1|0; 0; 1]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 0",
                "f(a; b) = a ∨ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPDiagMatrixScalarLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 0|0; 0; 1]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 1",
                "f(a; b) = a ⊕ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 1 1|1 1 1|1 1 0]", calc.ToString());
        }
        #endregion

        #region HPScalarMatrix
        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 4",
                "f(a; b) = a + b", $"f(D2;D1)"
            ]);
            Assert.Equal("[5 4 4|4 6 4|4 4 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7|0; 8|0; 0; 9]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 4",
                "f(a; b) = a - b", $"f(D2;D1)"
            ]);
            Assert.Equal("[-3 4 4|4 -4 4|4 4 -5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 4",
                "f(a; b) = a ^ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4 1 1|1 16 1|1 1 64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 4",
                "f(a; b) = a / b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4 +∞ +∞|+∞ 2 +∞|+∞ +∞ 1.33]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 2",
                "f(a; b) = a * b", $"f(D2;D1)"
            ]);
            Assert.Equal("[2 0 0|0 4 0|0 0 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 4",
                "f(a; b) = a \\ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4  Undefined   Undefined | Undefined  2  Undefined | Undefined   Undefined  1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 5",
                "f(a; b) = a ÷ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[5 +∞ +∞|+∞ 2.5 +∞|+∞ +∞ 1.67]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 3",
                "f(a; b) = a ⦼ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0  Undefined   Undefined | Undefined  1  Undefined | Undefined   Undefined  0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 4",
                "f(a; b) = a < b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 11",
                "f(a; b) = a > b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ≤ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarDiagMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal_hp(3; 1); 1; 1)",
                "D2 = 3",
                "f(a; b) = a ≥ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }
        #endregion

    }
}
