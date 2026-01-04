namespace Calcpad.Tests
{
    public class SymmetricMatrixOperatorTests
    {
        #region Matrix
        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric(3); 1; 1)",
                "D2 = copy([1; 1; 1|0; 1; 1|0; 0; 1]; symmetric(3); 1; 1)",
                "D3 = D1 + D2"
            ]);
            Assert.Equal("[3 4 5|4 6 7|5 7 8]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 6; 5|0; 4; 3|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = copy([2; 1; 1|0; 2; 1|0; 0; 1]; symmetric(3); 1; 1)",
                "D3 = D1 - D2"
            ]);
            Assert.Equal("[5 5 4|5 2 2|4 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; symmetric(3); 1; 1)",
                "D3 = D1 ^ D2"
            ]);
            Assert.Equal("[1 4 9|4 16 25|9 25 36]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 6; 4|0; 3; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; symmetric(3); 1; 1)",
                "D3 = D1 / D2"
            ]);
            Assert.Equal("[4 3 2|3 1.5 1|2 1 0.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 4; 5|0; 6; 7|0; 0; 8]; symmetric(3); 1; 1)",
                "D2 = copy([3; 3; 3|0; 3; 3|0; 0; 3]; symmetric(3); 1; 1)",
                "D3 = D1 * D2"
            ]);
            Assert.Equal("[36 36 36|51 51 51|60 60 60]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 7; 6|0; 5; 4|0; 0; 3]; symmetric(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; symmetric(3); 1; 1)",
                "D3 = D1 \\ D2"
            ]);
            Assert.Equal("[4 3 3|3 2 2|3 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 7; 5|0; 6; 4|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; symmetric(3); 1; 1)",
                "D3 = D1 ÷ D2"
            ]);
            Assert.Equal("[4.5 3.5 2.5|3.5 3 2|2.5 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 8; 9|0; 5; 6|0; 0; 3]; symmetric(3); 1; 1)",
                "D2 = copy([4; 4; 4|0; 4; 4|0; 0; 4]; symmetric(3); 1; 1)",
                "D3 = D1 ⦼ D2"
            ]);
            Assert.Equal("[3 0 1|0 1 2|1 2 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixFactorial()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 4; 5|0; 6; 7|0; 0; 8]; symmetric(3); 1; 1)",
                "D2 = copy([3; 4; 5|0; 6; 7|0; 0; 8]; symmetric(3); 1; 1)",
                "D3 = D1!"
            ]);
            Assert.Equal("[6 24 120|24 720 5040|120 5040 40320]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 4; 3|0; 5; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = copy([3; 3; 3|0; 3; 3|0; 0; 3]; symmetric(3); 1; 1)",
                "D3 = D1 ≡ D2"
            ]);
            Assert.Equal("[0 0 1|0 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 4; 3|0; 5; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = copy([4; 4; 4|0; 4; 4|0; 0; 4]; symmetric(3); 1; 1)",
                "D3 = D1 ≠ D2"
            ]);
            Assert.Equal("[1 0 1|0 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 6; 7|0; 4; 3|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = copy([5; 5; 5|0; 5; 5|0; 0; 5]; symmetric(3); 1; 1)",
                "D3 = D1 < D2"
            ]);
            Assert.Equal("[0 0 0|0 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 3; 2|0; 6; 5|0; 0; 7]; symmetric(3); 1; 1)",
                "D2 = copy([3; 3; 3|0; 3; 3|0; 0; 3]; symmetric(3); 1; 1)",
                "D3 = D1 > D2"
            ]);
            Assert.Equal("[1 0 0|0 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 6; 7|0; 3; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = copy([6; 6; 6|0; 6; 6|0; 0; 6]; symmetric(3); 1; 1)",
                "D3 = D1 ≤ D2"
            ]);
            Assert.Equal("[1 1 0|1 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 5; 7|0; 6; 4|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = copy([4; 4; 4|0; 4; 4|0; 0; 4]; symmetric(3); 1; 1)",
                "D3 = D1 ≥ D2"
            ]);
            Assert.Equal("[0 1 1|1 1 1|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric(3); 1; 1)",
                "D2 = copy([1; 1; 1|0; 1; 1|0; 0; 1]; symmetric(3); 1; 1)",
                "D3 = D1 ∧ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 0; 0|0; 1; 0|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = copy([0; 1; 1|0; 1; 1|0; 0; 1]; symmetric(3); 1; 1)",
                "D3 = D1 ∨ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void SymmetricMatrixLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 1; 0|0; 1; 0|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = copy([1; 0; 1|0; 1; 1|0; 0; 1]; symmetric(3); 1; 1)",
                "D3 = D1 ⊕ D2"
            ]);
            Assert.Equal("[0 1 1|1 0 1|1 1 0]", calc.ToString());
        }
        #endregion

        #region MatrixScalar
        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric(3); 1; 1)",
                "D2 = 5",
                "D3 = D1 + D2"
            ]);
            Assert.Equal("[7 8 9|8 10 11|9 11 12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 6; 5|0; 4; 3|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 - D2"
            ]);
            Assert.Equal("[4 3 2|3 1 0|2 0 -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ^ D2"
            ]);
            Assert.Equal("[1 4 9|4 16 25|9 25 36]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 6; 4|0; 3; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 / D2"
            ]);
            Assert.Equal("[4 3 2|3 1.5 1|2 1 0.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 4; 5|0; 6; 7|0; 0; 8]; symmetric(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 * D2"
            ]);
            Assert.Equal("[9 12 15|12 18 21|15 21 24]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 7; 6|0; 5; 4|0; 0; 3]; symmetric(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 \\ D2"
            ]);
            Assert.Equal("[4 3 3|3 2 2|3 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 7; 5|0; 6; 4|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ÷ D2"
            ]);
            Assert.Equal("[4.5 3.5 2.5|3.5 3 2|2.5 2 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 8; 9|0; 5; 6|0; 0; 3]; symmetric(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 ⦼ D2"
            ]);
            Assert.Equal("[3 0 1|0 1 2|1 2 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 4; 3|0; 5; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 ≡ D2"
            ]);
            Assert.Equal("[0 0 1|0 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 4; 3|0; 5; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 ≠ D2"
            ]);
            Assert.Equal("[1 0 1|0 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 6; 7|0; 4; 3|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = 5",
                "D3 = D1 < D2"
            ]);
            Assert.Equal("[0 0 0|0 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 3; 2|0; 6; 5|0; 0; 7]; symmetric(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 > D2"
            ]);
            Assert.Equal("[1 0 0|0 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 6; 7|0; 3; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = 6",
                "D3 = D1 ≤ D2"
            ]);
            Assert.Equal("[1 1 0|1 1 1|0 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 5; 7|0; 6; 4|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 ≥ D2"
            ]);
            Assert.Equal("[0 1 1|1 1 1|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric(3); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∧ D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric(3); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∨ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void SymmetricMatrixScalarLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric(3); 1; 1)",
                "D2 = 1",
                "D3 = D1 ⊕ D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        #endregion

        #region ScalarMatrix
        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric(3); 1; 1)",
                "D2 = 5",
                "D3 = D2 + D1"
            ]);
            Assert.Equal("[7 8 9|8 10 11|9 11 12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 6; 5|0; 4; 3|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 - D1"
            ]);
            Assert.Equal("[-4 -3 -2|-3 -1 0|-2 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; symmetric(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 ^ D1"
            ]);
            Assert.Equal("[2 4 8|4 16 32|8 32 64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 6; 4|0; 3; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 / D1"
            ]);
            Assert.Equal("[0.25 0.333 0.5|0.333 0.667 1|0.5 1 2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 4; 5|0; 6; 7|0; 0; 8]; symmetric(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 * D1"
            ]);
            Assert.Equal("[9 12 15|12 18 21|15 21 24]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 7; 6|0; 5; 4|0; 0; 3]; symmetric(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 \\ D1"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 7; 5|0; 6; 4|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 ÷ D1"
            ]);
            Assert.Equal("[0.222 0.286 0.4|0.286 0.333 0.5|0.4 0.5 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 8; 9|0; 5; 6|0; 0; 3]; symmetric(3); 1; 1)",
                "D2 = 4",
                "D3 = D2 ⦼ D1"
            ]);
            Assert.Equal("[4 4 4|4 4 4|4 4 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 6; 7|0; 4; 3|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = 5",
                "D3 = D2 < D1"
            ]);
            Assert.Equal("[0 1 1|1 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 3; 2|0; 6; 5|0; 0; 7]; symmetric(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 > D1"
            ]);
            Assert.Equal("[0 0 1|0 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 6; 7|0; 3; 2|0; 0; 1]; symmetric(3); 1; 1)",
                "D2 = 6",
                "D3 = D2 ≤ D1"
            ]);
            Assert.Equal("[0 1 1|1 0 0|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarSymmetricMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 5; 7|0; 6; 4|0; 0; 2]; symmetric(3); 1; 1)",
                "D2 = 4",
                "D3 = D2 ≥ D1"
            ]);
            Assert.Equal("[1 0 0|0 0 1|0 1 1]", calc.ToString());
        }

        #endregion
    }
}
