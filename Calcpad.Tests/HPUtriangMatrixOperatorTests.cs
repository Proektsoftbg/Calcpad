namespace Calcpad.Tests
{
    public class HPUtriangMatrixOperatorTests
    {
        #region HPMatrix
        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([4; 7; 5|0; 1; 1|0; 0; 2]; utriang_hp(3); 1; 1)",
                "D3 = D2 + D1"
            ]);
            Assert.Equal("[6 10 9|0 6 7|0 0 9]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; utriang_hp(3); 1; 1)",
                "D3 = D2 - D1"
            ]);
            Assert.Equal("[-1 -1 -1|0 -1 -1|0 0 -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([2; 2; 2|0; 2; 2|0; 0; 2]; utriang_hp(3); 1; 1)",
                "D3 = D2 ^ D1"
            ]);
            Assert.Equal("[4 8 16|1 32 64|1 1 128]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; utriang_hp(3); 1; 1)",
                "D3 = D2 / D1"
            ]);
            Assert.Equal("[0.5 0.667 0.75| Undefined  0.8 0.833| Undefined   Undefined  0.857]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([2; 4; 6|0; 1; 3|0; 0; 5]; utriang_hp(3); 1; 1)",
                "D3 = D2 * D1"
            ]);
            Assert.Equal("[4 26 74|0 5 27|0 0 35]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([1; 2; 1|0; 3; 2|0; 0; 3]; utriang_hp(3); 1; 1)",
                "D3 = D2 \\ D1"
            ]);
            Assert.Equal("[0 0 0| Undefined  0 0| Undefined   Undefined  0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([4; 8; 12|0; 5; 10|0; 0; 14]; utriang_hp(3); 1; 1)",
                "D3 = D2 ÷ D1"
            ]);
            Assert.Equal("[2 2.67 3| Undefined  1 1.67| Undefined   Undefined  2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([4; 7; 5|0; 3; 6|0; 0; 8]; utriang_hp(3); 1; 1)",
                "D3 = D2 ⦼ D1"
            ]);
            Assert.Equal("[0 1 1| Undefined  3 0| Undefined   Undefined  1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixFactorial()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D3 = D1!"
            ]);
            Assert.Equal("[2 6 24|1 120 720|1 1 5040]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D3 = D2 ≡ D1"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 4; 5|0; 0; 6]; utriang_hp(3); 1; 1)",
                "D3 = D2 ≠ D1"
            ]);
            Assert.Equal("[1 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([6; 5; 4|0; 4; 3|0; 0; 2]; utriang_hp(3); 1; 1)",
                "D3 = D2 < D1"
            ]);
            Assert.Equal("[0 0 0|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 6; 5|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D3 = D2 > D1"
            ]);
            Assert.Equal("[0 0 0|0 1 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([5; 5; 5|0; 5; 5|0; 0; 5]; utriang_hp(3); 1; 1)",
                "D3 = D2 ≤ D1"
            ]);
            Assert.Equal("[0 0 0|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D3 = D2 ≥ D1"
            ]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 0; 1|0; 0; 1]; utriang_hp(3); 1; 1)",
                "D3 = D2 ∧ D1"
            ]);
            Assert.Equal("[1 1 1|0 0 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([1; 2; 3|0; 0; 1|0; 0; 0]; utriang_hp(3); 1; 1)",
                "D3 = D2 ∨ D1"
            ]);
            Assert.Equal("[1 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrix")]
        public void HPUtriangMatrixLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = copy([1; 0; 1|0; 1; 0|0; 0; 1]; utriang_hp(3); 1; 1)",
                "D3 = D2 ⊕ D1"
            ]);
            Assert.Equal("[0 1 0|0 0 1|0 0 0]", calc.ToString());
        }
        #endregion

        #region HPMatrixScalar
        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D1 + D2"
            ]);
            Assert.Equal("[7 8 9|5 10 11|5 5 12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 - D2"
            ]);
            Assert.Equal("[-1 0 1|-3 2 3|-3 -3 4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ^ D2"
            ]);
            Assert.Equal("[4 9 16|0 25 36|0 0 49]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 / D2"
            ]);
            Assert.Equal("[1 1.5 2|0 2.5 3|0 0 3.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 * D2"
            ]);
            Assert.Equal("[6 9 12|0 15 18|0 0 21]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 \\ D2"
            ]);
            Assert.Equal("[0 0 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ÷ D2"
            ]);
            Assert.Equal("[1 1.5 2|0 2.5 3|0 0 3.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 ⦼ D2"
            ]);
            Assert.Equal("[2 3 0|0 1 2|0 0 3]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ≡ D2"
            ]);
            Assert.Equal("[1 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarNotEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D1 ≠ D2"
            ]);
            Assert.Equal("[0 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D1 < D2"
            ]);
            Assert.Equal("[1 1 1|1 0 0|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 1",
                "D3 = D1 > D2"
            ]);
            Assert.Equal("[1 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D1 ≤ D2"
            ]);
            Assert.Equal("[1 1 0|1 0 0|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D1 ≥ D2"
            ]);
            Assert.Equal("[0 0 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarLogicalAnd()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∧ D2"
            ]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarLogicalOr()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 0",
                "D3 = D1 ∨ D2"
            ]);
            Assert.Equal("[1 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMatrixScalar")]
        public void HPUtriangMatrixScalarLogicalXor()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 1",
                "D3 = D1 ⊕ D2"
            ]);
            Assert.Equal("[0 0 0|1 0 0|1 1 0]", calc.ToString());
        }

        #endregion

        #region HPScalarMatrix
        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixAddition()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D2 + D1"
            ]);
            Assert.Equal("[7 8 9|5 10 11|5 5 12]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 - D1"
            ]);
            Assert.Equal("[1 0 -1|3 -2 -3|3 3 -4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixExponentiation()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 2",
                "D3 = D2 ^ D1"
            ]);
            Assert.Equal("[4 8 16|1 32 64|1 1 128]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 12",
                "D3 = D2 / D1"
            ]);
            Assert.Equal("[6 4 3|+∞ 2.4 2|+∞ +∞ 1.71]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 * D1"
            ]);
            Assert.Equal("[6 9 12|0 15 18|0 0 21]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 8",
                "D3 = D2 \\ D1"
            ]);
            Assert.Equal("[4 2 2| Undefined  1 1| Undefined   Undefined  1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixDivisionBar()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 10",
                "D3 = D2 ÷ D1"
            ]);
            Assert.Equal("[5 3.33 2.5|+∞ 2 1.67|+∞ +∞ 1.43]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixRemainder()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 9",
                "D3 = D2 ⦼ D1"
            ]);
            Assert.Equal("[1 0 1| Undefined  4 3| Undefined   Undefined  2]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixLessThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D2 < D1"
            ]);
            Assert.Equal("[0 0 0|0 0 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixGreaterThan()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 4",
                "D3 = D2 > D1"
            ]);
            Assert.Equal("[1 1 0|1 0 0|1 1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixLessOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 3",
                "D3 = D2 ≤ D1"
            ]);
            Assert.Equal("[0 1 1|0 1 1|0 0 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarMatrix")]
        public void HPScalarUtriangMatrixGreaterOrEqual()
        {
            var calc = new TestCalc(new());
            calc.Run([
                "D1 = copy([2; 3; 4|0; 5; 6|0; 0; 7]; utriang_hp(3); 1; 1)",
                "D2 = 5",
                "D3 = D2 ≥ D1"
            ]);
            Assert.Equal("[1 1 1|1 1 0|1 1 0]", calc.ToString());
        }

        #endregion

    }
}
