namespace Calcpad.Tests
{
    public class HPLTriangMatrixOperatorTests
    {
        #region HPMatrix
        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 0; 0|2; 3; 0|4; 5; 6]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 + D2"
            ]);
            Assert.Equal("[5 0 0|7 9 0|11 12 11]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|7; 8; 0|9; 10; 11]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 - D2"
            ]);
            Assert.Equal("[1 0 0|2 2 0|2 3 6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ^ D2"
            ]);
            Assert.Equal("[16 1 1|243 4096 1|78125 279936 16807]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 0; 0|10; 12; 0|14; 16; 18]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 / D2"
            ]);
            Assert.Equal("[2  Undefined   Undefined |2 2  Undefined |2 2.29 3.6]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 * D2"
            ]);
            Assert.Equal("[12 0 0|41 30 0|115 98 40]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|7; 9; 0|11; 13; 15]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 \\ D2"
            ]);
            Assert.Equal("[1  Undefined   Undefined |1 1  Undefined |1 1 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([10; 0; 0|12; 14; 0|16; 18; 20]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ÷ D2"
            ]);
            Assert.Equal("[2.5  Undefined   Undefined |2.4 2.33  Undefined |2.29 2.57 4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 0; 0|10; 12; 0|14; 16; 18]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ⦼ D2"
            ]);
            Assert.Equal("[1  Undefined   Undefined |0 0  Undefined |0 2 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixFactorial()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang_hp(3); 1; 1)",
                "D2 = D1!"
            ]);
            Assert.Equal("[2 1 1|6 24 1|120 720 5040]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ≡ D2"
            ]);
            Assert.Equal("[0 1 1|0 0 1|0 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ≠ D2"
            ]);
            Assert.Equal("[1 0 0|1 1 0|1 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 < D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|4; 5; 0|6; 7; 8]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 > D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ≤ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ≥ D2"
            ]);
            Assert.Equal("[0 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ∧ D2"
            ]);
            Assert.Equal("[1 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ∨ D2"
            ]);
            Assert.Equal("[1 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPLtriangMatrixLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = copy([4; 0; 0|5; 6; 0|7; 7; 5]; ltriang_hp(3); 1; 1)",
                "D3 = D1 ⊕ D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        #endregion

        #region HPMatrixScalar
        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 0; 0|8; 9; 0|10; 11; 12]; ltriang_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D1 + D2"
            ]);
            Assert.Equal("[12 5 5|13 14 5|15 16 17]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 0; 0|6; 7; 0|8; 9; 10]; ltriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 - D2"
            ]);
            Assert.Equal("[1 -3 -3|3 4 -3|5 6 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ^ D2"
            ]);
            Assert.Equal("[4 0 0|9 16 0|25 36 49]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 0; 0|6; 7; 0|10; 12; 14]; ltriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 / D2"
            ]);
            Assert.Equal("[4 0 0|3 3.5 0|5 6 7]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|2; 5; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 * D2"
            ]);
            Assert.Equal("[9 0 0|6 15 0|21 24 27]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|8; 9; 0|12; 14; 16]; ltriang_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 \\ D2"
            ]);
            Assert.Equal("[1 0 0|2 2 0|3 3 4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([7; 0; 0|9; 10; 0|11; 13; 15]; ltriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ÷ D2"
            ]);
            Assert.Equal("[3.5 0 0|4.5 5 0|5.5 6.5 7.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([11; 0; 0|13; 14; 0|15; 17; 19]; ltriang_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 ⦼ D2"
            ]);
            Assert.Equal("[3 0 0|1 2 0|3 1 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D1 ≡ D2"
            ]);
            Assert.Equal("[0 0 0|0 1 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D1 ≠ D2"
            ]);
            Assert.Equal("[1 1 1|1 0 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = 6",
                "D3 = D1 < D2"
            ]);
            Assert.Equal("[1 1 1|1 0 1|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|4; 5; 0|6; 7; 8]; ltriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 > D2"
            ]);
            Assert.Equal("[0 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = 7",
                "D3 = D1 ≤ D2"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|6; 7; 0|8; 9; 10]; ltriang_hp(3); 1; 1)",
                "D2 = 6",
                "D3 = D1 ≥ D2"
            ]);
            Assert.Equal("[0 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∧ D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang_hp(3); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∨ D2"
            ]);
            Assert.Equal("[1 0 0|1 1 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPLtriangMatrixScalarLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang_hp(3); 1; 1)",
                "D2 = 1",
                "D3 = D1 ⊕ D2"
            ]);
            Assert.Equal("[0 1 1|0 0 1|0 0 0]", calc.ToString());
        }
        #endregion

        #region HPScalarMatrix
        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([1; 0; 0|2; 3; 0|4; 5; 6]; ltriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 + D1"
            ]);
            Assert.Equal("[4 3 3|5 6 3|7 8 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|7; 8; 0|9; 10; 11]; ltriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 - D1"
            ]);
            Assert.Equal("[-3 2 2|-5 -6 2|-7 -8 -9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|3; 4; 0|5; 6; 7]; ltriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 ^ D1"
            ]);
            Assert.Equal("[4 1 1|8 16 1|32 64 128]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([8; 0; 0|10; 12; 0|14; 16; 18]; ltriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 / D1"
            ]);
            Assert.Equal("[0.25 +∞ +∞|0.2 0.167 +∞|0.143 0.125 0.111]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|4; 5; 0|6; 7; 8]; ltriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 * D1"
            ]);
            Assert.Equal("[6 0 0|8 10 0|12 14 16]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([5; 0; 0|7; 9; 0|11; 13; 15]; ltriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 \\ D1"
            ]);
            Assert.Equal("[0  Undefined   Undefined |0 0  Undefined |0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([10; 0; 0|12; 14; 0|16; 18; 20]; ltriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 ÷ D1"
            ]);
            Assert.Equal("[0.2 +∞ +∞|0.167 0.143 +∞|0.125 0.111 0.1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([9; 0; 0|10; 12; 0|14; 16; 18]; ltriang_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D2 ⦼ D1"
            ]);
            Assert.Equal("[4  Undefined   Undefined |4 4  Undefined |4 4 4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([4; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = 6",
                "D3 = D2 < D1"
            ]);
            Assert.Equal("[0 0 0|0 0 0|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 0; 0|4; 5; 0|6; 7; 8]; ltriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 > D1"
            ]);
            Assert.Equal("[1 1 1|0 0 1|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarLtriangMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([3; 0; 0|5; 6; 0|7; 8; 9]; ltriang_hp(3); 1; 1)",
                "D2 = 7",
                "D3 = D2 ≤ D1"
            ]);
            Assert.Equal("[0 0 0|0 0 0|1 1 1]", calc.ToString());
        }
        #endregion

    }
}
