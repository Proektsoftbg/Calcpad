namespace Calcpad.Tests
{
    public class LTriangMatrixOperatorInFunctionTests
    {
        #region Matrix
        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 0; 0|2; 3; 0|4; 5; 6]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a + b", $"f(D1;D2)"
            ]);
            Assert.Equal("[5 0 0|7 9 0|11 12 11]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|7; 8; 0|9; 10; 11]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a - b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|2 2 0|2 3 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a ^ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[16 1 1|243 4096 1|78125 279936 16807]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 0; 0|10; 12; 0|14; 16; 18]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a / b", $"f(D1;D2)"
            ]);
            Assert.Equal("[2  Undefined   Undefined |2 2  Undefined |2 2.29 3.6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a * b", $"f(D1;D2)"
            ]);
            Assert.Equal("[12 0 0|41 30 0|115 98 40]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|7; 9; 0|11; 13; 15]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a \\ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1  Undefined   Undefined |1 1  Undefined |1 1 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([10; 0; 0|12; 14; 0|16; 18; 20]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a ÷ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[2.5  Undefined   Undefined |2.4 2.33  Undefined |2.29 2.57 4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 0; 0|10; 12; 0|14; 16; 18]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a ⦼ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1  Undefined   Undefined |0 0  Undefined |0 2 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixFactorial()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang(3); 1; 1)",
                "f(a) = a!", $"f(D1)"
            ]);
            Assert.Equal("[2 1 1|6 24 1|120 720 5040]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a ≡ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 1 1|0 0 1|0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a ≠ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|1 1 0|1 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a < b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|4; 5; 0|6; 7; 8]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a > b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a ≤ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a ∧ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a ∨ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void LtriangMatrixLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang(3); 1; 1)",
                "f(a; b) = a ⊕ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        #endregion

        #region MatrixScalar
        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 0; 0|8; 9; 0|10; 11; 12]; ltriang(3); 1; 1)",
                "D2 = 5",
                "f(a; b) = a + b", $"f(D1;D2)"
            ]);
            Assert.Equal("[12 5 5|13 14 5|15 16 17]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 0; 0|6; 7; 0|8; 9; 10]; ltriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a - b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 -3 -3|3 4 -3|5 6 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ^ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[4 0 0|9 16 0|25 36 49]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 0; 0|6; 7; 0|10; 12; 14]; ltriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a / b", $"f(D1;D2)"
            ]);
            Assert.Equal("[4 0 0|3 3.5 0|5 6 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|2; 5; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a * b", $"f(D1;D2)"
            ]);
            Assert.Equal("[9 0 0|6 15 0|21 24 27]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|8; 9; 0|12; 14; 16]; ltriang(3); 1; 1)",
                "D2 = 4",
                "f(a; b) = a \\ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|2 2 0|3 3 4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 0; 0|9; 10; 0|11; 13; 15]; ltriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ÷ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[3.5 0 0|4.5 5 0|5.5 6.5 7.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([11; 0; 0|13; 14; 0|15; 17; 19]; ltriang(3); 1; 1)",
                "D2 = 4",
                "f(a; b) = a ⦼ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[3 0 0|1 2 0|3 1 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang(3); 1; 1)",
                "D2 = 5",
                "f(a; b) = a ≡ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 1 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang(3); 1; 1)",
                "D2 = 5",
                "f(a; b) = a ≠ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 0 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = 6",
                "f(a; b) = a < b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 0 1|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|4; 5; 0|6; 7; 8]; ltriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a > b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = 7",
                "f(a; b) = a ≤ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|6; 7; 0|8; 9; 10]; ltriang(3); 1; 1)",
                "D2 = 6",
                "f(a; b) = a ≥ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = 0",
                "f(a; b) = a ∧ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang(3); 1; 1)",
                "D2 = 0",
                "f(a; b) = a ∨ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void LtriangMatrixScalarLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang(3); 1; 1)",
                "D2 = 1",
                "f(a; b) = a ⊕ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 1 1|0 0 1|0 0 0]", calc.ToString());
        }
        #endregion

        #region ScalarMatrix
        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 0; 0|2; 3; 0|4; 5; 6]; ltriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a + b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4 3 3|5 6 3|7 8 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|7; 8; 0|9; 10; 11]; ltriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a - b", $"f(D2;D1)"
            ]);
            Assert.Equal("[-3 2 2|-5 -6 2|-7 -8 -9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ^ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4 1 1|8 16 1|32 64 128]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 0; 0|10; 12; 0|14; 16; 18]; ltriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a / b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0.25 +∞ +∞|0.2 0.167 +∞|0.143 0.125 0.111]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a * b", $"f(D2;D1)"
            ]);
            Assert.Equal("[6 0 0|8 10 0|12 14 16]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|7; 9; 0|11; 13; 15]; ltriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a \\ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0  Undefined   Undefined |0 0  Undefined |0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([10; 0; 0|12; 14; 0|16; 18; 20]; ltriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ÷ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0.2 +∞ +∞|0.167 0.143 +∞|0.125 0.111 0.1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 0; 0|10; 12; 0|14; 16; 18]; ltriang(3); 1; 1)",
                "D2 = 4",
                "f(a; b) = a ⦼ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4  Undefined   Undefined |4 4  Undefined |4 4 4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = 6",
                "f(a; b) = a < b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|4; 5; 0|6; 7; 8]; ltriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a > b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|0 0 1|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = 7",
                "f(a; b) = a ≤ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarLtriangMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang(3); 1; 1)",
                "D2 = 7",
                "f(a; b) = a ≥ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 0 0]", calc.ToString());
        }
        #endregion

    }
}
