namespace Calcpad.Tests
{
    public class MatrixOperatorInFunctionTests
    {
        #region Matrix
        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixAddition()
        {
            string matrix1 = "[1; 2; 3 | 4; 5; 6 | 7; 8; 9]";
            string matrix2 = "[5; 4; 3 | 1; 2; 2 | 7; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[6 6 6|5 7 8|14 9 18]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixSubtraction()
        {
            string matrix1 = "[8; 3; 7 | 2; 6; 5 | 9; 4; 2]";
            string matrix2 = "[5; 2; 4 | 3; 1; 6 | 1; 8; 7]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[3 1 3|-1 5 -1|8 -4 -5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixExponentiation()
        {
            string matrix1 = "[2; 3; 2 | 1; 4; 5 | 3; 2; 2]";
            string matrix2 = "[3; 2; 4 | 5; 1; 2 | 2; 3; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[8 9 16|1 4 25|9 8 2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixDivision()
        {
            string matrix1 = "[10; 20; 30 | 40; 50; 60 | 70; 80; 90]";
            string matrix2 = "[2; 4; 5 | 10; 2; 6 | 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[5 5 6|4 25 10|10 10 10]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixMultiplication()
        {
            string matrix1 = "[1; 4; 7 | 2; 5; 8 | 3; 6; 9]";
            string matrix2 = "[9; 8; 7 | 6; 5; 4 | 3; 2; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a * b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[54 42 30|72 57 42|90 72 54]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixIntegerDivision()
        {
            string matrix1 = "[10; 21; 30 | 40; 55; 66 | 70; 88; 99]";
            string matrix2 = "[2; 4; 5 | 10; 2; 6 | 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[5 5 6|4 27 11|10 11 11]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixDivisionBar()
        {
            string matrix1 = "[18; 16; 14 | 12; 10; 8 | 6; 4; 2]";
            string matrix2 = "[2; 4; 7 | 3; 5; 8 | 1; 2; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[9 4 2|4 2 1|6 2 2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixRemainder()
        {
            string matrix1 = "[11; 23; 34 | 45; 56; 67 | 78; 89; 90]";
            string matrix2 = "[2; 4; 5 | 6; 7; 8 | 9; 10; 11]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1 3 4|3 0 3|6 9 2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void MatrixFactorial()
        {
            string matrix = "[4; 2; 6| 2; 3; 4| 7; 7; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = a!", $"f({matrix})"]);
            Assert.Equal("[24 2 720|2 6 24|5040 5040 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixEqual()
        {
            string matrix1 = "[1; 2; 3 | 4; 5; 6 | 7; 8; 9]";
            string matrix2 = "[1; 4; 3 | 4; 5; 6 | 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≡ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1 0 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixNotEqual()
        {
            string matrix1 = "[1; 2; 3 | 4; 5; 6 | 7; 8; 9]";
            string matrix2 = "[9; 8; 7 | 6; 5; 4 | 3; 2; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≠ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1 1 1|1 0 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixLessThan()
        {
            string matrix1 = "[1; 2; 3 | 4; 5; 6 | 7; 8; 9]";
            string matrix2 = "[9; 8; 7 | 6; 5; 4 | 3; 2; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1 1 1|1 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixGreaterThan()
        {
            string matrix1 = "[9; 8; 7 | 6; 5; 4 | 3; 2; 1]";
            string matrix2 = "[1; 2; 3 | 4; 5; 6 | 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1 1 1|1 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixLessOrEqual()
        {
            string matrix1 = "[1; 2; 10 | 4; 5; 6 | 7; 8; 9]";
            string matrix2 = "[1; 2; 3 | 4; 5; 6 | 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1 1 0|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixGreaterOrEqual()
        {
            string matrix1 = "[9; 8; 7 | 6; 5; 4 | 3; 2; 1]";
            string matrix2 = "[1; 2; 3 | 4; 5; 6 | 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1 1 1|1 1 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixLogicalAnd()
        {
            string matrix1 = "[1; 1; 0 | 0; 1; 0 | 1; 0; 1]";
            string matrix2 = "[1; 0; 1 | 1; 1; 0 | 0; 1; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∧ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixLogicalOr()
        {
            string matrix1 = "[1; 1; 0 | 0; 1; 0 | 1; 0; 1]";
            string matrix2 = "[1; 0; 1 | 1; 1; 0 | 0; 1; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∨ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1 1 1|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void MatrixLogicalXor()
        {
            string matrix1 = "[1; 1; 0 | 0; 1; 0 | 1; 0; 1]";
            string matrix2 = "[1; 0; 1 | 1; 1; 0 | 0; 1; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⊕ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[0 1 1|1 0 0|1 1 0]", calc.ToString());
        }
        #endregion

        #region MatrixScalar
        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarAddition()
        {
            string matrix = "[1; 2; 3 | 4; 5 ; 6 | 7; 8; 9]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({matrix};{x})"]);
            Assert.Equal("[5 6 7|8 9 10|11 12 13]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarSubtraction()
        {
            string matrix = "[5; 6; 7 | 8; 9 ; 10 | 11; 12; 13]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({matrix};{x})"]);
            Assert.Equal("[1 2 3|4 5 6|7 8 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarExponentiation()
        {
            string matrix = "[2; 3; 4 | 5; 6 ; 7 | 8; 9; 10]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({matrix};{x})"]);
            Assert.Equal("[4 9 16|25 36 49|64 81 100]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarDivision()
        {
            string matrix = "[8; 16; 24 | 32; 40 ; 48 | 56; 64; 72]";
            string x = "8.0";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({matrix};{x})"]);
            Assert.Equal("[1 2 3|4 5 6|7 8 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarMultiplication()
        {
            string matrix = "[1; 2; 3 | 4; 5 ; 6 | 7; 8; 9]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a * b", $"f({matrix};{x})"]);
            Assert.Equal("[4 8 12|16 20 24|28 32 36]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarIntegerDivision()
        {
            string matrix = "[9; 12; 15 | 18; 21 ; 24 | 27; 30; 33]";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({matrix};{x})"]);
            Assert.Equal("[3 4 5|6 7 8|9 10 11]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarDivisionBar()
        {
            string matrix = "[9; 18; 27 | 36; 45 ; 54 | 63; 72; 81]";
            string x = "9";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({matrix};{x})"]);
            Assert.Equal("[1 2 3|4 5 6|7 8 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarRemainder()
        {
            string matrix = "[10; 15; 20 | 25; 30 ; 35 | 40; 45; 50]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({matrix};{x})"]);
            Assert.Equal("[3 1 6|4 2 0|5 3 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarEqual()
        {
            string matrix = "[1; 7; 7 | 7; 7 ; 7 | 7; 7; 7]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≡ b", $"f({matrix};{x})"]);
            Assert.Equal("[0 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarNotEqual()
        {
            string matrix = "[7; 8; 9 | 10; 11 ; 12 | 13; 14; 15]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≠ b", $"f({matrix};{x})"]);
            Assert.Equal("[0 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarLessThan()
        {
            string matrix = "[5; 6; 7 | 8; 9 ; 10 | 11; 12; 13]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({matrix};{x})"]);
            Assert.Equal("[1 1 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarGreaterThan()
        {
            string matrix = "[8; 9; 10 | 11; 12 ; 13 | 14; 15; 16]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({matrix};{x})"]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarLessOrEqual()
        {
            string matrix = "[5; 7; 9 | 11; 13 ; 15 | 17; 19; 21]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({matrix};{x})"]);
            Assert.Equal("[1 1 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarGreaterOrEqual()
        {
            string matrix = "[7; 8; 9 | 10; 11 ; 12 | 13; 14; 15]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({matrix};{x})"]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarLogicalAnd()
        {
            string matrix = "[1; 0; 1 | 0; 1 ; 0 | 1; 0; 1]";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∧ b", $"f({matrix};{x})"]);
            Assert.Equal("[1 0 1|0 1 0|1 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarLogicalOr()
        {
            string matrix = "[0; 0; 0 | 0; 1 ; 0 | 0; 0; 0]";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∨ b", $"f({matrix};{x})"]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void MatrixScalarLogicalXor()
        {
            string matrix = "[1; 1; 0 | 0; 1 ; 0 | 1; 0; 1]";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⊕ b", $"f({matrix};{x})"]);
            Assert.Equal("[0 0 1|1 0 1|0 1 0]", calc.ToString());
        }
        #endregion

        #region ScalarMatrix
        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixAddition()
        {
            string matrix = "[1; 2; 3 | 4; 5 ; 6 | 7; 8; 9]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({x};{matrix})"]);
            Assert.Equal("[5 6 7|8 9 10|11 12 13]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixSubtraction()
        {
            string matrix = "[5; 6; 7 | 8; 9 ; 10 | 11; 12; 13]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({x};{matrix})"]);
            Assert.Equal("[-1 -2 -3|-4 -5 -6|-7 -8 -9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixExponentiation()
        {
            string matrix = "[2; 3; 4 | 5; 6 ; 7 | 8; 9; 2]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({x};{matrix})"]);
            Assert.Equal("[4 8 16|32 64 128|256 512 4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixDivision()
        {
            string matrix = "[8; 16; 24 | 32; 40 ; 48 | 56; 64; 72]";
            string x = "8.0";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({x};{matrix})"]);
            Assert.Equal("[1 0.5 0.333|0.25 0.2 0.167|0.143 0.125 0.111]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixMultiplication()
        {
            string matrix = "[1; 2; 3 | 4; 5 ; 6 | 7; 8; 9]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a * b", $"f({x};{matrix})"]);
            Assert.Equal("[4 8 12|16 20 24|28 32 36]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixIntegerDivision()
        {
            string matrix = "[9; 12; 15 | 18; 21 ; 24 | 27; 30; 33]";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({x};{matrix})"]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixDivisionBar()
        {
            string matrix = "[9; 18; 27 | 36; 45 ; 54 | 63; 72; 81]";
            string x = "9";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({x};{matrix})"]);
            Assert.Equal("[1 0.5 0.333|0.25 0.2 0.167|0.143 0.125 0.111]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixRemainder()
        {
            string matrix = "[10; 15; 20 | 25; 30 ; 35 | 40; 45; 50]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({x};{matrix})"]);
            Assert.Equal("[7 7 7|7 7 7|7 7 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixLessThan()
        {
            string matrix = "[5; 6; 7 | 8; 9 ; 10 | 11; 12; 13]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({x};{matrix})"]);
            Assert.Equal("[0 0 0|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixGreaterThan()
        {
            string matrix = "[8; 9; 10 | 11; 12 ; 13 | 14; 15; 16]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({x};{matrix})"]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixLessOrEqual()
        {
            string matrix = "[5; 7; 9 | 11; 13 ; 15 | 17; 19; 21]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({x};{matrix})"]);
            Assert.Equal("[0 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarMatrixGreaterOrEqual()
        {
            string matrix = "[7; 8; 9 | 10; 11 ; 12 | 13; 14; 15]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({x};{matrix})"]);
            Assert.Equal("[1 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        #endregion

    }
}
