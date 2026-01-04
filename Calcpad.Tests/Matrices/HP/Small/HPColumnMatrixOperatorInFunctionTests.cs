namespace Calcpad.Tests
{
    public class HPColumnMatrixOperatorInFunctionTests
    {
        #region HPMatrix
        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixAddition()
        {
            string matrix1 = "hp([1| 4| 7])";
            string matrix2 = "hp([5| 1| 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[6|5|14]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixSubtraction()
        {
            string matrix1 = "hp([8| 2| 9])";
            string matrix2 = "hp([5| 3| 1])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[3|-1|8]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixExponentiation()
        {
            string matrix1 = "hp([2| 1| 3])";
            string matrix2 = "hp([3| 5| 2])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[8|1|9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixDivision()
        {
            string matrix1 = "hp([10| 40| 70])";
            string matrix2 = "hp([2| 10| 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[5|4|10]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixIntegerDivision()
        {
            string matrix1 = "hp([10| 40| 70])";
            string matrix2 = "hp([2| 10| 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[5|4|10]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixDivisionBar()
        {
            string matrix1 = "hp([18| 12| 6])";
            string matrix2 = "hp([2| 3| 1])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[9|4|6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixRemainder()
        {
            string matrix1 = "hp([11| 67| 78])";
            string matrix2 = "hp([2| 6| 9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1|1|6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixFactorial()
        {
            string matrix = "hp([4| 2| 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = a!", $"f({matrix})"]);
            string expected = "[24|2|5040]";
            Assert.Equal(expected, calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixEqual()
        {
            string matrix1 = "hp([1| 4| 7])";
            string matrix2 = "hp([1| 4| 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≡ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixNotEqual()
        {
            string matrix1 = "hp([1| 4| 7])";
            string matrix2 = "hp([9| 6| 3])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≠ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixLessThan()
        {
            string matrix1 = "hp([1| 4| 7])";
            string matrix2 = "hp([9| 6| 3])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1|1|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixGreaterThan()
        {
            string matrix1 = "hp([9| 6| 3])";
            string matrix2 = "hp([1| 4| 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1|1|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixLessOrEqual()
        {
            string matrix1 = "hp([1| 4| 7])";
            string matrix2 = "hp([1| 4| 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixGreaterOrEqual()
        {
            string matrix1 = "hp([9| 6| 3])";
            string matrix2 = "hp([1| 4| 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1|1|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixLogicalAnd()
        {
            string matrix1 = "hp([1| 0| 1])";
            string matrix2 = "hp([1| 1| 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∧ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixLogicalOr()
        {
            string matrix1 = "hp([1| 0| 1])";
            string matrix2 = "hp([1| 1| 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∨ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPColumnMatrixLogicalXor()
        {
            string matrix1 = "hp([1| 0| 1])";
            string matrix2 = "hp([1| 1| 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⊕ b", $"f({matrix1};{matrix2})"]);
            Assert.Equal("[0|1|1]", calc.ToString());
        }
        #endregion

        #region HPMatrixVector
        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorAddition()
        {
            string matrix = "hp([1| 2| 3| 4| 5])";
            string vector = "hp([1; 1; 2; 4; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({matrix};{vector})"]);
            Assert.Equal("[2|3|5|8|12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorSubtraction()
        {
            string matrix = "hp([5| 6| 7| 8| 9])";
            string vector = "hp([1; 1; 2; 4; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({matrix};{vector})"]);
            Assert.Equal("[4|5|5|4|2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorExponentiation()
        {
            string matrix = "hp([2| 3| 4| 5| 6])";
            string vector = "hp([1; 2; 3; 4; 5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({matrix};{vector})"]);
            Assert.Equal("[2|9|64|625|7776]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorDivision()
        {
            string matrix = "hp([8| 18| 24| 36| 42])";
            string vector = "hp([2; 3; 4; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({matrix};{vector})"]);
            Assert.Equal("[4|6|6|6|6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPColumnMatrixVectorMultiplication()
        {
            string matrix = "hp([4.5| -5.7| 7.1])";
            var calc = new TestCalc(new());
            calc.Run(["vec = matrix_hp(1; 3)",
                      "vec.(1; 1) = -9.2",
                      "vec.(1; 2) = 6.3",
                      "vec.(1; 3) = 1",
                      "f(a; b) = a * b",
                      $"f({matrix};vec)"]);
            Assert.Equal("[-41.4 28.35 4.5|52.44 -35.91 -5.7|-65.32 44.73 7.1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorIntegerDivision()
        {
            string matrix = "hp([9| 12| 15| 18| 21])";
            string vector = "hp([3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({matrix};{vector})"]);
            Assert.Equal("[3|3|3|3|3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorDivisionBar()
        {
            string matrix = "hp([9| 18| 27| 36| 45])";
            string vector = "hp([1; 2; 3; 4; 5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({matrix};{vector})"]);
            Assert.Equal("[9|9|9|9|9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorRemainder()
        {
            string matrix = "hp([10| 20| 30| 40| 50])";
            string vector = "hp([3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({matrix};{vector})"]);
            Assert.Equal("[1|0|0|4|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorEqual()
        {
            string matrix = "hp([7| 7| 7| 7| 7])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≡ b", $"f({matrix};{vector})"]);
            Assert.Equal("[1|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorNotEqual()
        {
            string matrix = "hp([7| 8| 9| 10| 11])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≠ b", $"f({matrix};{vector})"]);
            Assert.Equal("[0|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorGreaterThan()
        {
            string matrix = "hp([8| 9| 10| 11| 12])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({matrix};{vector})"]);
            Assert.Equal("[1|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorLessThan()
        {
            string matrix = "hp([5| 6| 7| 8| 9])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({matrix};{vector})"]);
            Assert.Equal("[1|1|0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorLessOrEqual()
        {
            string matrix = "hp([5| 7| 9| 11| 13])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({matrix};{vector})"]);
            Assert.Equal("[1|1|0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorGreaterOrEqual()
        {
            string matrix = "hp([7| 8| 9| 10| 11])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({matrix};{vector})"]);
            Assert.Equal("[1|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorLogicalAnd()
        {
            string matrix = "hp([1| 0| 1| 0| 1])";
            string vector = "hp([1; 1; 0; 0; 1])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∧ b", $"f({matrix};{vector})"]);
            Assert.Equal("[1|0|0|0|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorLogicalOr()
        {
            string matrix = "hp([0| 0| 0| 0| 1])";
            string vector = "hp([1; 1; 1; 0; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∨ b", $"f({matrix};{vector})"]);
            Assert.Equal("[1|1|1|0|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixVector")]
        public void HPColumnMatrixVectorLogicalXor()
        {
            string matrix = "hp([1| 1| 0| 0| 1])";
            string vector = "hp([1; 0; 1; 0; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⊕ b", $"f({matrix};{vector})"]);
            Assert.Equal("[0|1|1|0|1]", calc.ToString());
        }
        #endregion

        #region HPVectorMatrix
        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixAddition()
        {
            string matrix = "hp([1| 2| 3| 4| 5])";
            string vector = "hp([1; 1; 2; 4; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({vector};{matrix})"]);
            Assert.Equal("[2|3|5|8|12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixSubtraction()
        {
            string matrix = "hp([5| 6| 7| 8| 9])";
            string vector = "hp([1; 1; 2; 4; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({vector};{matrix})"]);
            Assert.Equal("[-4|-5|-5|-4|-2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixExponentiation()
        {
            string matrix = "hp([2| 3| 4| 5| 6])";
            string vector = "hp([1; 2; 3; 4; 5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({vector};{matrix})"]);
            Assert.Equal("[1|8|81|1024|15625]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixDivision()
        {
            string matrix = "hp([8| 18| 24| 36| 42])";
            string vector = "hp([2; 3; 4; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({vector};{matrix})"]);
            Assert.Equal("[0.25|0.167|0.167|0.167|0.167]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixMultiplication()
        {
            string matrix = "hp([4.5; -3.2| -5.7; 3.4])";
            var calc = new TestCalc(new());
            calc.Run(["vec = matrix_hp(1; 2)",
                      "vec.(1; 1) = -9.2",
                      "vec.(1; 2) = 6.3",
                      "f(a; b) = a * b",
                      $"f(vec;{matrix})"]);
            Assert.Equal("[-77.31 50.86]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixIntegerDivision()
        {
            string matrix = "hp([9| 2| 15| 18| 21])";
            string vector = "hp([3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({vector};{matrix})"]);
            Assert.Equal("[0|2|0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixDivisionBar()
        {
            string matrix = "hp([9| 18| 27| 36| 45])";
            string vector = "hp([1; 2; 3; 4; 5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({vector};{matrix})"]);
            Assert.Equal("[0.111|0.111|0.111|0.111|0.111]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixRemainder()
        {
            string matrix = "hp([10| 20| 30| 40| 50])";
            string vector = "hp([3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({vector};{matrix})"]);
            Assert.Equal("[3|4|5|6|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixGreaterThan()
        {
            string matrix = "hp([2| 9| 10| 11| 12])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({vector};{matrix})"]);
            Assert.Equal("[1|0|0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixLessThan()
        {
            string matrix = "hp([5| 6| 7| 8| 9])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({vector};{matrix})"]);
            Assert.Equal("[0|0|0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixLessOrEqual()
        {
            string matrix = "hp([5| 7| 9| 11| 13])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({vector};{matrix})"]);
            Assert.Equal("[0|1|1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPVectorMatrix")]
        public void HPVectorColumnMatrixGreaterOrEqual()
        {
            string matrix = "hp([7| 8| 9| 10| 11])";
            string vector = "hp([7; 7; 7; 7; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({vector};{matrix})"]); ;
            Assert.Equal("[1|0|0|0|0]", calc.ToString());
        }
        #endregion

        #region HPMatrixScalar
        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarAddition()
        {
            string matrix = "hp([1| 4| 7])";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({matrix};{x})"]);
            Assert.Equal("[5|8|11]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarSubtraction()
        {
            string matrix = "hp([5| 8| 11])";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({matrix};{x})"]);
            Assert.Equal("[1|4|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarExponentiation()
        {
            string matrix = "hp([2| 5| 8])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({matrix};{x})"]);
            Assert.Equal("[4|25|64]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarDivision()
        {
            string matrix = "hp([8| 32| 56])";
            string x = "8.0";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({matrix};{x})"]);
            Assert.Equal("[1|4|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarMultiplication()
        {
            string matrix = "hp([1| 4| 7])";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a * b", $"f({matrix};{x})"]);
            Assert.Equal("[4|16|28]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarIntegerDivision()
        {
            string matrix = "hp([9| 18| 27])";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({matrix};{x})"]);
            Assert.Equal("[3|6|9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarDivisionBar()
        {
            string matrix = "hp([9| 36| 63])";
            string x = "9";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({matrix};{x})"]);
            Assert.Equal("[1|4|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarRemainder()
        {
            string matrix = "hp([10| 25| 40])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({matrix};{x})"]);
            Assert.Equal("[3|4|5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarEqual()
        {
            string matrix = "hp([1| 7| 7])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≡ b", $"f({matrix};{x})"]);
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarNotEqual()
        {
            string matrix = "hp([7| 10| 13])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≠ b", $"f({matrix};{x})"]);
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarLessThan()
        {
            string matrix = "hp([5| 8| 11])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({matrix};{x})"]);
            Assert.Equal("[1|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarGreaterThan()
        {
            string matrix = "hp([2| 11| 14])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({matrix};{x})"]);
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarLessOrEqual()
        {
            string matrix = "hp([5| 11| 17])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({matrix};{x})"]);
            Assert.Equal("[1|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarGreaterOrEqual()
        {
            string matrix = "hp([7| 10| 13])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({matrix};{x})"]);
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarLogicalAnd()
        {
            string matrix = "hp([1| 0| 1])";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∧ b", $"f({matrix};{x})"]);
            Assert.Equal("[1|0|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarLogicalOr()
        {
            string matrix = "hp([0| 1| 0])";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ∨ b", $"f({matrix};{x})"]);
            Assert.Equal("[1|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPColumnMatrixScalarLogicalXor()
        {
            string matrix = "hp([1| 0| 1])";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⊕ b", $"f({matrix};{x})"]);
            Assert.Equal("[0|1|0]", calc.ToString());
        }
        #endregion

        #region HPScalarMatrix
        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixAddition()
        {
            string matrix = "hp([1| 4| 7])";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a + b", $"f({x};{matrix})"]);
            Assert.Equal("[5|8|11]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixSubtraction()
        {
            string matrix = "hp([5| 8| 11])";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a - b", $"f({x};{matrix})"]);
            Assert.Equal("[-1|-4|-7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixExponentiation()
        {
            string matrix = "hp([2| 5| 8])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ^ b", $"f({x};{matrix})"]);
            Assert.Equal("[4|32|256]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixDivision()
        {
            string matrix = "hp([8| 32| 56])";
            string x = "8.0";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a / b", $"f({x};{matrix})"]);
            Assert.Equal("[1|0.25|0.143]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixMultiplication()
        {
            string matrix = "hp([1| 4| 7])";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a * b", $"f({x};{matrix})"]);
            Assert.Equal("[4|16|28]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixIntegerDivision()
        {
            string matrix = "hp([9| 18| 27])";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a \\ b", $"f({x};{matrix})"]);
            Assert.Equal("[0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixDivisionBar()
        {
            string matrix = "hp([9| 36| 63])";
            string x = "9";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ÷ b", $"f({x};{matrix})"]);
            Assert.Equal("[1|0.25|0.143]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixRemainder()
        {
            string matrix = "hp([10| 25| 40])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ⦼ b", $"f({x};{matrix})"]);
            Assert.Equal("[7|7|7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixLessThan()
        {
            string matrix = "hp([5| 8| 11])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a < b", $"f({x};{matrix})"]);
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixGreaterThan()
        {
            string matrix = "hp([8| 11| 14])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a > b", $"f({x};{matrix})"]);
            Assert.Equal("[0|0|0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixLessOrEqual()
        {
            string matrix = "hp([5| 11| 17])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≤ b", $"f({x};{matrix})"]);
            Assert.Equal("[0|1|1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarColumnMatrixGreaterOrEqual()
        {
            string matrix = "hp([7| 10| 13])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = a ≥ b", $"f({x};{matrix})"]);
            Assert.Equal("[1|0|0]", calc.ToString());
        }
        #endregion

    }
}
