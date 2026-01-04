namespace Calcpad.Tests
{
    public class UtriangMatrixOperatorInFunctionTests
    {
        #region Matrix
        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([4; 7; 5|0; 1; 1|0; 0; 2]; utriang(3); 1; 1)",
                "f(a; b) = a + b", $"f(D2;D1)"
            ]);
            Assert.Equal("[6 10 9|0 6 7|0 0 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; utriang(3); 1; 1)",
                "f(a; b) = a - b", $"f(D2;D1)"
            ]);
            Assert.Equal("[-1 -1 -1|0 -1 -1|0 0 -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; utriang(3); 1; 1)",
                "f(a; b) = a ^ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4 8 16|1 32 64|1 1 128]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; utriang(3); 1; 1)",
                "f(a; b) = a / b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0.5 0.667 0.75| Undefined  0.8 0.833| Undefined   Undefined  0.857]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([2; 4; 6|0; 1; 3|0; 0; 5]; utriang(3); 1; 1)",
                "f(a; b) = a * b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4 26 74|0 5 27|0 0 35]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([1; 2; 1|0; 3; 2|0; 0; 3]; utriang(3); 1; 1)",
                "f(a; b) = a \\ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 0 0| Undefined  0 0| Undefined   Undefined  0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([4; 8; 12|0; 5; 10|0; 0; 14]; utriang(3); 1; 1)",
                "f(a; b) = a ÷ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[2 2.67 3| Undefined  1 1.67| Undefined   Undefined  2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([4; 7; 5|0; 3; 6|0; 0; 8]; utriang(3); 1; 1)",
                "f(a; b) = a ⦼ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 1 1| Undefined  3 0| Undefined   Undefined  1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixFactorial()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "f(a) = a!", $"f(D1)"
            ]);
            Assert.Equal("[2 6 24|1 120 720|1 1 5040]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "f(a; b) = a ≡ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; utriang(3); 1; 1)",
                "f(a; b) = a ≠ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([6; 5; 4|0; 4; 3|0; 0; 2]; utriang(3); 1; 1)",
                "f(a; b) = a < b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 0 0|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 6; 5|0; 0; 7]; utriang(3); 1; 1)",
                "f(a; b) = a > b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 0 0|0 1 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([5; 5; 5|0; 5; 5|0; 0; 5]; utriang(3); 1; 1)",
                "f(a; b) = a ≤ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 0 0|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "f(a; b) = a ≥ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 0; 1|0; 0; 1]; utriang(3); 1; 1)",
                "f(a; b) = a ∧ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|0 0 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 0; 1|0; 0; 0]; utriang(3); 1; 1)",
                "f(a; b) = a ∨ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void UtriangMatrixLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = copy([1; 0; 1|0; 1; 0|0; 0; 1]; utriang(3); 1; 1)",
                "f(a; b) = a ⊕ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 1 0|0 0 1|0 0 0]", calc.ToString());
        }
        #endregion

        #region MatrixScalar
        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 5",
                "f(a; b) = a + b", $"f(D1;D2)"
            ]);
            Assert.Equal("[7 8 9|5 10 11|5 5 12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a - b", $"f(D1;D2)"
            ]);
            Assert.Equal("[-1 0 1|-3 2 3|-3 -3 4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ^ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[4 9 16|0 25 36|0 0 49]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a / b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1.5 2|0 2.5 3|0 0 3.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a * b", $"f(D1;D2)"
            ]);
            Assert.Equal("[6 9 12|0 15 18|0 0 21]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 4",
                "f(a; b) = a \\ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ÷ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1.5 2|0 2.5 3|0 0 3.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 4",
                "f(a; b) = a ⦼ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[2 3 0|0 1 2|0 0 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ≡ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ≠ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 5",
                "f(a; b) = a < b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|1 0 0|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 1",
                "f(a; b) = a > b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a ≤ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 0|1 0 0|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 4",
                "f(a; b) = a ≥ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 0",
                "f(a; b) = a ∧ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 0",
                "f(a; b) = a ∨ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[1 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void UtriangMatrixScalarLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 1",
                "f(a; b) = a ⊕ b", $"f(D1;D2)"
            ]);
            Assert.Equal("[0 0 0|1 0 0|1 1 0]", calc.ToString());
        }

        #endregion

        #region ScalarMatrix
        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 5",
                "f(a; b) = a + b", $"f(D2;D1)"
            ]);
            Assert.Equal("[7 8 9|5 10 11|5 5 12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a - b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 0 -1|3 -2 -3|3 3 -4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 2",
                "f(a; b) = a ^ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4 8 16|1 32 64|1 1 128]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 12",
                "f(a; b) = a / b", $"f(D2;D1)"
            ]);
            Assert.Equal("[6 4 3|+∞ 2.4 2|+∞ +∞ 1.71]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a * b", $"f(D2;D1)"
            ]);
            Assert.Equal("[6 9 12|0 15 18|0 0 21]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 8",
                "f(a; b) = a \\ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[4 2 2| Undefined  1 1| Undefined   Undefined  1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 10",
                "f(a; b) = a ÷ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[5 3.33 2.5|+∞ 2 1.67|+∞ +∞ 1.43]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 9",
                "f(a; b) = a ⦼ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 0 1| Undefined  4 3| Undefined   Undefined  2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 5",
                "f(a; b) = a < b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 0 0|0 0 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 4",
                "f(a; b) = a > b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 0|1 0 0|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 3",
                "f(a; b) = a ≤ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[0 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarUtriangMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang(3); 1; 1)",
                "D2 = 5",
                "f(a; b) = a ≥ b", $"f(D2;D1)"
            ]);
            Assert.Equal("[1 1 1|1 1 0|1 1 0]", calc.ToString());
        }

        #endregion

    }
}
