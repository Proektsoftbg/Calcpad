namespace Calcpad.Tests
{
    public class DiagMatrixOperatorTests
    {
        #region Matrix
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = copy([4|0; 5|0; 0; 6]; diagonal(3; 1); 1; 1)",
                "D3 = D1 + D2"
            ]);
            Assert.Equal("[5 0 0|0 7 0|0 0 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9|0; 6|0; 0; 8]; diagonal(3; 1); 1; 1)",
                "D2 = copy([4|0; 3|0; 0; 5]; diagonal(3; 1); 1; 1)",
                "D3 = D1 - D2"
            ]);
            Assert.Equal("[5 0 0|0 3 0|0 0 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 3|0; 0; 4]; diagonal(3; 1); 1; 1)",
                "D2 = copy([3|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ^ D2"
            ]);
            Assert.Equal("[8 1 1|1 9 1|1 1 64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8|0; 7|0; 0; 6]; diagonal(3; 1); 1; 1)",
                "D2 = copy([4|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D3 = D1 / D2"
            ]);
            Assert.Equal("[2  Undefined   Undefined | Undefined  3.5  Undefined | Undefined   Undefined  2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5|0; 4|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = copy([3|0; 6|0; 0; 2]; diagonal(3; 1); 1; 1)",
                "D3 = D1 * D2"
            ]);
            Assert.Equal("[15 0 0|0 24 0|0 0 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9|0; 7|0; 0; 8]; diagonal(3; 1); 1; 1)",
                "D2 = copy([3|0; 2|0; 0; 4]; diagonal(3; 1); 1; 1)",
                "D3 = D1 \\ D2"
            ]);
            Assert.Equal("[3  Undefined   Undefined | Undefined  3  Undefined | Undefined   Undefined  2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8|0; 6|0; 0; 9]; diagonal(3; 1); 1; 1)",
                "D2 = copy([4|0; 3|0; 0; 5]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ÷ D2"
            ]);
            Assert.Equal("[2  Undefined   Undefined | Undefined  2  Undefined | Undefined   Undefined  1.8]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([10|0; 9|0; 0; 7]; diagonal(3; 1); 1; 1)",
                "D2 = copy([4|0; 5|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ⦼ D2"
            ]);
            Assert.Equal("[2  Undefined   Undefined | Undefined  4  Undefined | Undefined   Undefined  1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Vector")]
        public void DiagMatrixFactorial()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3|0; 4|0; 0; 5]; diagonal(3; 1); 1; 1)",
                "D2 = D1!"
            ]);
            Assert.Equal("[6 1 1|1 24 1|1 1 120]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 4|0; 0; 6]; diagonal(3; 1); 1; 1)",
                "D2 = copy([3|0; 5|0; 0; 7]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ≡ D2"
            ]);
            Assert.Equal("[0 1 1|1 0 1|1 1 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3|0; 5|0; 0; 7]; diagonal(3; 1); 1; 1)",
                "D2 = copy([2|0; 4|0; 0; 6]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ≠ D2"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 3|0; 0; 4]; diagonal(3; 1); 1; 1)",
                "D2 = copy([5|0; 6|0; 0; 7]; diagonal(3; 1); 1; 1)",
                "D3 = D1 < D2"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7|0; 8|0; 0; 9]; diagonal(3; 1); 1; 1)",
                "D2 = copy([3|0; 4|0; 0; 5]; diagonal(3; 1); 1; 1)",
                "D3 = D1 > D2"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 4|0; 0; 6]; diagonal(3; 1); 1; 1)",
                "D2 = copy([5|0; 6|0; 0; 8]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ≤ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([6|0; 7|0; 0; 8]; diagonal(3; 1); 1; 1)",
                "D2 = copy([3|0; 4|0; 0; 5]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ≥ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = copy([0|0; 1|0; 0; 1]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ∧ D2"
            ]);
            Assert.Equal("[0 0 0|0 1 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([0|0; 1|0; 0; 1]; diagonal(3; 1); 1; 1)",
                "D2 = copy([1|0; 0|0; 0; 1]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ∨ D2"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void DiagMatrixLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 1|0; 0; 1]; diagonal(3; 1); 1; 1)",
                "D2 = copy([0|0; 1|0; 0; 0]; diagonal(3; 1); 1; 1)",
                "D3 = D1 ⊕ D2"
            ]);
            Assert.Equal("[1 0 0|0 0 0|0 0 1]", calc.ToString());
        }
        #endregion

        #region MatrixScalar
        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 3|0; 0; 4]; diagonal(3; 1); 1; 1)",
                "D2 = 5",
                "D3 = D1 + D2"
            ]);
            Assert.Equal("[7 5 5|5 8 5|5 5 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8|0; 9|0; 0; 10]; diagonal(3; 1); 1; 1)",
                "D2 = 3",
                "D3 = D1 - D2"
            ]);
            Assert.Equal("[5 -3 -3|-3 6 -3|-3 -3 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 3|0; 0; 4]; diagonal(3; 1); 1; 1)",
                "D2 = 3",
                "D3 = D1 ^ D2"
            ]);
            Assert.Equal("[8 0 0|0 27 0|0 0 64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([6|0; 8|0; 0; 12]; diagonal(3; 1); 1; 1)",
                "D2 = 2",
                "D3 = D1 / D2"
            ]);
            Assert.Equal("[3 0 0|0 4 0|0 0 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2|0; 4|0; 0; 5]; diagonal(3; 1); 1; 1)",
                "D2 = 3",
                "D3 = D1 * D2"
            ]);
            Assert.Equal("[6 0 0|0 12 0|0 0 15]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8|0; 10|0; 0; 12]; diagonal(3; 1); 1; 1)",
                "D2 = 4",
                "D3 = D1 \\ D2"
            ]);
            Assert.Equal("[2 0 0|0 2 0|0 0 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([6|0; 8|0; 0; 10]; diagonal(3; 1); 1; 1)",
                "D2 = 2",
                "D3 = D1 ÷ D2"
            ]);
            Assert.Equal("[3 0 0|0 4 0|0 0 5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7|0; 10|0; 0; 13]; diagonal(3; 1); 1; 1)",
                "D2 = 4",
                "D3 = D1 ⦼ D2"
            ]);
            Assert.Equal("[3 0 0|0 2 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 1",
                "D3 = D1 ≡ D2"
            ]);
            Assert.Equal("[1 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 2",
                "D3 = D1 ≠ D2"
            ]);
            Assert.Equal("[1 1 1|1 0 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 4",
                "D3 = D1 < D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 11",
                "D3 = D1 > D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 2",
                "D3 = D1 ≤ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 3",
                "D3 = D1 ≥ D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 1|0; 0; 1]; diagonal(3; 1); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∧ D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 1|0; 0; 1]; diagonal(3; 1); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∨ D2"
            ]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void DiagMatrixScalarLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 0|0; 0; 1]; diagonal(3; 1); 1; 1)",
                "D2 = 1",
                "D3 = D1 ⊕ D2"
            ]);
            Assert.Equal("[0 1 1|1 1 1|1 1 0]", calc.ToString());
        }
        #endregion

        #region ScalarMatrix
        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 4",
                "D3 = D2 + D1"
            ]);
            Assert.Equal("[5 4 4|4 6 4|4 4 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7|0; 8|0; 0; 9]; diagonal(3; 1); 1; 1)",
                "D2 = 4",
                "D3 = D2 - D1"
            ]);
            Assert.Equal("[-3 4 4|4 -4 4|4 4 -5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 4",
                "D3 = D2 ^ D1"
            ]);
            Assert.Equal("[4 1 1|1 16 1|1 1 64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 4",
                "D3 = D2 / D1"
            ]);
            Assert.Equal("[4 +∞ +∞|+∞ 2 +∞|+∞ +∞ 1.33]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 2",
                "D3 = D2 * D1"
            ]);
            Assert.Equal("[2 0 0|0 4 0|0 0 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 4",
                "D3 = D2 \\ D1"
            ]);
            Assert.Equal("[4  Undefined   Undefined | Undefined  2  Undefined | Undefined   Undefined  1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 5",
                "D3 = D2 ÷ D1"
            ]);
            Assert.Equal("[5 +∞ +∞|+∞ 2.5 +∞|+∞ +∞ 1.67]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 3",
                "D3 = D2 ⦼ D1"
            ]);
            Assert.Equal("[0  Undefined   Undefined | Undefined  1  Undefined | Undefined   Undefined  0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 4",
                "D3 = D2 < D1"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 11",
                "D3 = D2 > D1"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 2",
                "D3 = D2 ≤ D1"
            ]);
            Assert.Equal("[0 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarDiagMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1|0; 2|0; 0; 3]; diagonal(3; 1); 1; 1)",
                "D2 = 3",
                "D3 = D2 ≥ D1"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }


        #endregion

    }
}
