namespace Calcpad.Tests
{
    public class HPSymmetricMatrixOperatorTests
    {
        #region HPMatrix
        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric_hp(3); 1; 1)",
                "D2 = copy([1; 1; 1|0; 1; 1|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D3 = D1 + D2"
            ]);
            Assert.Equal("[3 4 5|4 6 7|5 7 8]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 6; 5|0; 4; 3|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = copy([2; 1; 1|0; 2; 1|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D3 = D1 - D2"
            ]);
            Assert.Equal("[5 5 4|5 2 2|4 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric_hp(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ^ D2"
            ]);
            Assert.Equal("[1 4 9|4 16 25|9 25 36]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 6; 4|0; 3; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D3 = D1 / D2"
            ]);
            Assert.Equal("[4 3 2|3 1.5 1|2 1 0.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 4; 5|0; 6; 7|0; 0; 8]; symmetric_hp(3); 1; 1)",
                "D2 = copy([3; 3; 3|0; 3; 3|0; 0; 3]; symmetric_hp(3); 1; 1)",
                "D3 = D1 * D2"
            ]);
            Assert.Equal("[36 36 36|51 51 51|60 60 60]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 7; 6|0; 5; 4|0; 0; 3]; symmetric_hp(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D3 = D1 \\ D2"
            ]);
            Assert.Equal("[4 3 3|3 2 2|3 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 7; 5|0; 6; 4|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ÷ D2"
            ]);
            Assert.Equal("[4.5 3.5 2.5|3.5 3 2|2.5 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 8; 9|0; 5; 6|0; 0; 3]; symmetric_hp(3); 1; 1)",
                "D2 = copy([4; 4; 4|0; 4; 4|0; 0; 4]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ⦼ D2"
            ]);
            Assert.Equal("[3 0 1|0 1 2|1 2 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixFactorial()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 4; 5|0; 6; 7|0; 0; 8]; symmetric_hp(3); 1; 1)",
                "D3 = D1!"
            ]);
            Assert.Equal("[6 24 120|24 720 5040|120 5040 40320]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 4; 3|0; 5; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = copy([3; 3; 3|0; 3; 3|0; 0; 3]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ≡ D2"
            ]);
            Assert.Equal("[0 0 1|0 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 4; 3|0; 5; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = copy([4; 4; 4|0; 4; 4|0; 0; 4]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ≠ D2"
            ]);
            Assert.Equal("[1 0 1|0 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 6; 7|0; 4; 3|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = copy([5; 5; 5|0; 5; 5|0; 0; 5]; symmetric_hp(3); 1; 1)",
                "D3 = D1 < D2"
            ]);
            Assert.Equal("[0 0 0|0 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 3; 2|0; 6; 5|0; 0; 7]; symmetric_hp(3); 1; 1)",
                "D2 = copy([3; 3; 3|0; 3; 3|0; 0; 3]; symmetric_hp(3); 1; 1)",
                "D3 = D1 > D2"
            ]);
            Assert.Equal("[1 0 0|0 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 6; 7|0; 3; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = copy([6; 6; 6|0; 6; 6|0; 0; 6]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ≤ D2"
            ]);
            Assert.Equal("[1 1 0|1 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 5; 7|0; 6; 4|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = copy([4; 4; 4|0; 4; 4|0; 0; 4]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ≥ D2"
            ]);
            Assert.Equal("[0 1 1|1 1 1|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric_hp(3); 1; 1)",
                "D2 = copy([1; 1; 1|0; 1; 1|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ∧ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 0; 0|0; 1; 0|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = copy([0; 1; 1|0; 1; 1|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ∨ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPSymmetricMatrixLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 1; 0|0; 1; 0|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = copy([1; 0; 1|0; 1; 1|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D3 = D1 ⊕ D2"
            ]);
            Assert.Equal("[0 1 1|1 0 1|1 1 0]", calc.ToString());
        }
        #endregion

        #region HPMatrixScalar
        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D1 + D2"
            ]);
            Assert.Equal("[7 8 9|8 10 11|9 11 12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 6; 5|0; 4; 3|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 - D2"
            ]);
            Assert.Equal("[4 3 2|3 1 0|2 0 -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ^ D2"
            ]);
            Assert.Equal("[1 4 9|4 16 25|9 25 36]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 6; 4|0; 3; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 / D2"
            ]);
            Assert.Equal("[4 3 2|3 1.5 1|2 1 0.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 4; 5|0; 6; 7|0; 0; 8]; symmetric_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 * D2"
            ]);
            Assert.Equal("[9 12 15|12 18 21|15 21 24]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 7; 6|0; 5; 4|0; 0; 3]; symmetric_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 \\ D2"
            ]);
            Assert.Equal("[4 3 3|3 2 2|3 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 7; 5|0; 6; 4|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ÷ D2"
            ]);
            Assert.Equal("[4.5 3.5 2.5|3.5 3 2|2.5 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 8; 9|0; 5; 6|0; 0; 3]; symmetric_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 ⦼ D2"
            ]);
            Assert.Equal("[3 0 1|0 1 2|1 2 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 4; 3|0; 5; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 ≡ D2"
            ]);
            Assert.Equal("[0 0 1|0 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 4; 3|0; 5; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 ≠ D2"
            ]);
            Assert.Equal("[1 0 1|0 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 6; 7|0; 4; 3|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D1 < D2"
            ]);
            Assert.Equal("[0 0 0|0 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 3; 2|0; 6; 5|0; 0; 7]; symmetric_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 > D2"
            ]);
            Assert.Equal("[1 0 0|0 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 6; 7|0; 3; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = 6",
                "D3 = D1 ≤ D2"
            ]);
            Assert.Equal("[1 1 0|1 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 5; 7|0; 6; 4|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 ≥ D2"
            ]);
            Assert.Equal("[0 1 1|1 1 1|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric_hp(3); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∧ D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric_hp(3); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∨ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPSymmetricMatrixScalarLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric_hp(3); 1; 1)",
                "D2 = 1",
                "D3 = D1 ⊕ D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        #endregion

        #region HPScalarMatrix
        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D2 + D1"
            ]);
            Assert.Equal("[7 8 9|8 10 11|9 11 12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 6; 5|0; 4; 3|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 - D1"
            ]);
            Assert.Equal("[-4 -3 -2|-3 -1 0|-2 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 ^ D1"
            ]);
            Assert.Equal("[2 4 8|4 16 32|8 32 64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 6; 4|0; 3; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 / D1"
            ]);
            Assert.Equal("[0.25 0.333 0.5|0.333 0.667 1|0.5 1 2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 4; 5|0; 6; 7|0; 0; 8]; symmetric_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 * D1"
            ]);
            Assert.Equal("[9 12 15|12 18 21|15 21 24]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 7; 6|0; 5; 4|0; 0; 3]; symmetric_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 \\ D1"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 7; 5|0; 6; 4|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 ÷ D1"
            ]);
            Assert.Equal("[0.222 0.286 0.4|0.286 0.333 0.5|0.4 0.5 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 8; 9|0; 5; 6|0; 0; 3]; symmetric_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D2 ⦼ D1"
            ]);
            Assert.Equal("[4 4 4|4 4 4|4 4 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 6; 7|0; 4; 3|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D2 < D1"
            ]);
            Assert.Equal("[0 1 1|1 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 3; 2|0; 6; 5|0; 0; 7]; symmetric_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 > D1"
            ]);
            Assert.Equal("[0 0 1|0 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 6; 7|0; 3; 2|0; 0; 1]; symmetric_hp(3); 1; 1)",
                "D2 = 6",
                "D3 = D2 ≤ D1"
            ]);
            Assert.Equal("[0 1 1|1 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarSymmetricMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 5; 7|0; 6; 4|0; 0; 2]; symmetric_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D2 ≥ D1"
            ]);
            Assert.Equal("[1 0 0|0 0 1|0 1 1]", calc.ToString());
        }

        #endregion
    }
}
