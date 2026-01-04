namespace Calcpad.Tests
{
    public class ColumnMatrixOperatorTests
    {
        #region Matrix
        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixAddition()
        {
            string matrix1 = "[1| 4| 7]";
            string matrix2 = "[5| 1| 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} + {matrix2}");
            Assert.Equal("[6|5|14]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixSubtraction()
        {
            string matrix1 = "[8| 2| 9]";
            string matrix2 = "[5| 3| 1]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} - {matrix2}");
            Assert.Equal("[3|-1|8]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixExponentiation()
        {
            string matrix1 = "[2| 1| 3]";
            string matrix2 = "[3| 5| 2]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ^ {matrix2}");
            Assert.Equal("[8|1|9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixDivision()
        {
            string matrix1 = "[10| 40| 70]";
            string matrix2 = "[2| 10| 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} / {matrix2}");
            Assert.Equal("[5|4|10]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixIntegerDivision()
        {
            string matrix1 = "[10| 40| 70]";
            string matrix2 = "[2| 10| 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} \\ {matrix2}");
            Assert.Equal("[5|4|10]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixDivisionBar()
        {
            string matrix1 = "[18| 12| 6]";
            string matrix2 = "[2| 3| 1]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ÷ {matrix2}");
            Assert.Equal("[9|4|6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixRemainder()
        {
            string matrix1 = "[11| 67| 78]";
            string matrix2 = "[2| 6| 9]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ⦼ {matrix2}");
            Assert.Equal("[1|1|6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Vector")]
        public void ColumnMatrixFactorial()
        {
            string matrix = "[4| 2| 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix}!");
            string expected = "[24|2|5040]";
            Assert.Equal(expected, calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixEqual()
        {
            string matrix1 = "[1| 4| 7]";
            string matrix2 = "[1| 4| 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ≡ {matrix2}");
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixNotEqual()
        {
            string matrix1 = "[1| 4| 7]";
            string matrix2 = "[9| 6| 3]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ≠ {matrix2}");
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixLessThan()
        {
            string matrix1 = "[1| 4| 7]";
            string matrix2 = "[9| 6| 3]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} < {matrix2}");
            Assert.Equal("[1|1|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixGreaterThan()
        {
            string matrix1 = "[9| 6| 3]";
            string matrix2 = "[1| 4| 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} > {matrix2}");
            Assert.Equal("[1|1|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixLessOrEqual()
        {
            string matrix1 = "[1| 4| 7]";
            string matrix2 = "[1| 4| 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ≤ {matrix2}");
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixGreaterOrEqual()
        {
            string matrix1 = "[9| 6| 3]";
            string matrix2 = "[1| 4| 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ≥ {matrix2}");
            Assert.Equal("[1|1|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixLogicalAnd()
        {
            string matrix1 = "[1| 0| 1]";
            string matrix2 = "[1| 1| 0]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ∧ {matrix2}");
            Assert.Equal("[1|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixLogicalOr()
        {
            string matrix1 = "[1| 0| 1]";
            string matrix2 = "[1| 1| 0]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ∨ {matrix2}");
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "Matrix")]
        public void ColumnMatrixLogicalXor()
        {
            string matrix1 = "[1| 0| 1]";
            string matrix2 = "[1| 1| 0]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix1} ⊕ {matrix2}");
            Assert.Equal("[0|1|1]", calc.ToString());
        }
        #endregion

        #region MatrixVector
        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorAddition()
        {
            string matrix = "[1| 2| 3| 4| 5]";
            string vector = "[1; 1; 2; 4; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} + {vector}");
            Assert.Equal("[2|3|5|8|12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorSubtraction()
        {
            string matrix = "[5| 6| 7| 8| 9]";
            string vector = "[1; 1; 2; 4; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} - {vector}");
            Assert.Equal("[4|5|5|4|2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorExponentiation()
        {
            string matrix = "[2| 3| 4| 5| 6]";
            string vector = "[1; 2; 3; 4; 5]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ^ {vector}");
            Assert.Equal("[2|9|64|625|7776]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorDivision()
        {
            string matrix = "[8| 18| 24| 36| 42]";
            string vector = "[2; 3; 4; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} / {vector}");
            Assert.Equal("[4|6|6|6|6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void ColumnMatrixVectorMultiplication()
        {
            string matrix = "[4.5| -5.7| 7.1]";
            var calc = new TestCalc(new());
            calc.Run(["vec = matrix(1; 3)",
                      "vec.(1; 1) = -9.2",
                      "vec.(1; 2) = 6.3",
                      "vec.(1; 3) = 1",
                      $"{matrix} * vec"]);
            Assert.Equal("[-41.4 28.35 4.5|52.44 -35.91 -5.7|-65.32 44.73 7.1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorIntegerDivision()
        {
            string matrix = "[9| 12| 15| 18| 21]";
            string vector = "[3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} \\ {vector}");
            Assert.Equal("[3|3|3|3|3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorDivisionBar()
        {
            string matrix = "[9| 18| 27| 36| 45]";
            string vector = "[1; 2; 3; 4; 5]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ÷ {vector}");
            Assert.Equal("[9|9|9|9|9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorRemainder()
        {
            string matrix = "[10| 20| 30| 40| 50]";
            string vector = "[3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ⦼ {vector}");
            Assert.Equal("[1|0|0|4|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorEqual()
        {
            string matrix = "[7| 7| 7| 7| 7]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ≡ {vector}");
            Assert.Equal("[1|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorNotEqual()
        {
            string matrix = "[7| 8| 9| 10| 11]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ≠ {vector}");
            Assert.Equal("[0|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorGreaterThan()
        {
            string matrix = "[8| 9| 10| 11| 12]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} > {vector}");
            Assert.Equal("[1|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorLessThan()
        {
            string matrix = "[5| 6| 7| 8| 9]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} < {vector}");
            Assert.Equal("[1|1|0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorLessOrEqual()
        {
            string matrix = "[5| 7| 9| 11| 13]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ≤ {vector}");
            Assert.Equal("[1|1|0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorGreaterOrEqual()
        {
            string matrix = "[7| 8| 9| 10| 11]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ≥ {vector}");
            Assert.Equal("[1|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorLogicalAnd()
        {
            string matrix = "[1| 0| 1| 0| 1]";
            string vector = "[1; 1; 0; 0; 1]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ∧ {vector}");
            Assert.Equal("[1|0|0|0|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorLogicalOr()
        {
            string matrix = "[0| 0| 0| 0| 1]";
            string vector = "[1; 1; 1; 0; 0]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ∨ {vector}");
            Assert.Equal("[1|1|1|0|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixVector")]
        public void ColumnMatrixVectorLogicalXor()
        {
            string matrix = "[1| 1| 0| 0| 1]";
            string vector = "[1; 0; 1; 0; 0]";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ⊕ {vector}");
            Assert.Equal("[0|1|1|0|1]", calc.ToString());
        }
        #endregion

        #region VectorMatrix
        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixAddition()
        {
            string matrix = "[1| 2| 3| 4| 5]";
            string vector = "[1; 1; 2; 4; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} + {matrix}");
            Assert.Equal("[2|3|5|8|12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixSubtraction()
        {
            string matrix = "[5| 6| 7| 8| 9]";
            string vector = "[1; 1; 2; 4; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} - {matrix}");
            Assert.Equal("[-4|-5|-5|-4|-2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixExponentiation()
        {
            string matrix = "[2| 3| 4| 5| 6]";
            string vector = "[1; 2; 3; 4; 5]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ^ {matrix}");
            Assert.Equal("[1|8|81|1024|15625]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixDivision()
        {
            string matrix = "[8| 18| 24| 36| 42]";
            string vector = "[2; 3; 4; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} / {matrix}");
            Assert.Equal("[0.25|0.167|0.167|0.167|0.167]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixMultiplication()
        {
            string matrix = "[4.5; -3.2| -5.7; 3.4]";
            var calc = new TestCalc(new());
            calc.Run(["vec = matrix(1; 2)",
                      "vec.(1; 1) = -9.2",
                      "vec.(1; 2) = 6.3",
                      $"vec * {matrix}"]);
            Assert.Equal("[-77.31 50.86]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixIntegerDivision()
        {
            string matrix = "[9| 2| 15| 18| 21]";
            string vector = "[3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} \\ {matrix}");
            Assert.Equal("[0|2|0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixDivisionBar()
        {
            string matrix = "[9| 18| 27| 36| 45]";
            string vector = "[1; 2; 3; 4; 5]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ÷ {matrix}");
            Assert.Equal("[0.111|0.111|0.111|0.111|0.111]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixRemainder()
        {
            string matrix = "[10| 20| 30| 40| 50]";
            string vector = "[3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ⦼ {matrix}");
            Assert.Equal("[3|4|5|6|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixGreaterThan()
        {
            string matrix = "[2| 9| 10| 11| 12]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} > {matrix}");
            Assert.Equal("[1|0|0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixLessThan()
        {
            string matrix = "[5| 6| 7| 8| 9]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} < {matrix}");
            Assert.Equal("[0|0|0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixLessOrEqual()
        {
            string matrix = "[5| 7| 9| 11| 13]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≤ {matrix}");
            Assert.Equal("[0|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "VectorMatrix")]
        public void VectorColumnMatrixGreaterOrEqual()
        {
            string matrix = "[7| 8| 9| 10| 11]";
            string vector = "[7; 7; 7; 7; 7]";
            var calc = new TestCalc(new());
            calc.Run($"{vector} ≥ {matrix}");
            Assert.Equal("[1|0|0|0|0]", calc.ToString());
        }
        #endregion

        #region MatrixScalar
        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarAddition()
        {
            string matrix = "[1| 4| 7]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} + {x}");
            Assert.Equal("[5|8|11]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarSubtraction()
        {
            string matrix = "[5| 8| 11]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} - {x}");
            Assert.Equal("[1|4|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarExponentiation()
        {
            string matrix = "[2| 5| 8]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ^ {x}");
            Assert.Equal("[4|25|64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarDivision()
        {
            string matrix = "[8| 32| 56]";
            string x = "8.0";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} / {x}");
            Assert.Equal("[1|4|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarMultiplication()
        {
            string matrix = "[1| 4| 7]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} * {x}");
            Assert.Equal("[4|16|28]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarIntegerDivision()
        {
            string matrix = "[9| 18| 27]";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} \\ {x}");
            Assert.Equal("[3|6|9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarDivisionBar()
        {
            string matrix = "[9| 36| 63]";
            string x = "9";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ÷ {x}");
            Assert.Equal("[1|4|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarRemainder()
        {
            string matrix = "[10| 25| 40]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ⦼ {x}");
            Assert.Equal("[3|4|5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarEqual()
        {
            string matrix = "[1| 7| 7]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ≡ {x}");
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarNotEqual()
        {
            string matrix = "[7| 10| 13]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ≠ {x}");
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarLessThan()
        {
            string matrix = "[5| 8| 11]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} < {x}");
            Assert.Equal("[1|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarGreaterThan()
        {
            string matrix = "[2| 11| 14]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} > {x}");
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarLessOrEqual()
        {
            string matrix = "[5| 11| 17]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ≤ {x}");
            Assert.Equal("[1|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarGreaterOrEqual()
        {
            string matrix = "[7| 10| 13]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ≥ {x}");
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarLogicalAnd()
        {
            string matrix = "[1| 0| 1]";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ∧ {x}");
            Assert.Equal("[1|0|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarLogicalOr()
        {
            string matrix = "[0| 1| 0]";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ∨ {x}");
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MatrixScalar")]
        public void ColumnMatrixScalarLogicalXor()
        {
            string matrix = "[1| 0| 1]";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run($"{matrix} ⊕ {x}");
            Assert.Equal("[0|1|0]", calc.ToString());
        }
        #endregion

        #region ScalarMatrix
        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixAddition()
        {
            string matrix = "[1| 4| 7]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run($"{x} + {matrix}");
            Assert.Equal("[5|8|11]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixSubtraction()
        {
            string matrix = "[5| 8| 11]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run($"{x} - {matrix}");
            Assert.Equal("[-1|-4|-7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixExponentiation()
        {
            string matrix = "[2| 5| 8]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"{x} ^ {matrix}");
            Assert.Equal("[4|32|256]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixDivision()
        {
            string matrix = "[8| 32| 56]";
            string x = "8.0";
            var calc = new TestCalc(new());
            calc.Run($"{x} / {matrix}");
            Assert.Equal("[1|0.25|0.143]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixMultiplication()
        {
            string matrix = "[1| 4| 7]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run($"{x} * {matrix}");
            Assert.Equal("[4|16|28]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixIntegerDivision()
        {
            string matrix = "[9| 18| 27]";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run($"{x} \\ {matrix}");
            Assert.Equal("[0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixDivisionBar()
        {
            string matrix = "[9| 36| 63]";
            string x = "9";
            var calc = new TestCalc(new());
            calc.Run($"{x} ÷ {matrix}");
            Assert.Equal("[1|0.25|0.143]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixRemainder()
        {
            string matrix = "[10| 25| 40]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{x} ⦼ {matrix}");
            Assert.Equal("[7|7|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixLessThan()
        {
            string matrix = "[5| 8| 11]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{x} < {matrix}");
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixGreaterThan()
        {
            string matrix = "[8| 11| 14]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{x} > {matrix}");
            Assert.Equal("[0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixLessOrEqual()
        {
            string matrix = "[5| 11| 17]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{x} ≤ {matrix}");
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarMatrix")]
        public void ScalarColumnMatrixGreaterOrEqual()
        {
            string matrix = "[7| 10| 13]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"{x} ≥ {matrix}");
            Assert.Equal("[1|0|0]", calc.ToString());
        }
        #endregion

    }
}
