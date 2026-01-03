namespace Calcpad.Tests
{
    public class HPVectorComparison
    {
        #region HPVectorOperators

        private static string[] OperatorTestHelper(char o) => [
            "n = 1000",
            "a = random(fill(vector(n); 1))",
            "b = random(fill(vector(n); 1))",
            $"c = a {o} b",
            "a_hp = hp(a)",
            "b_hp = hp(b)",
            $"c_hp = a_hp {o} b_hp",
            "r = c_hp ≡ c",
            "check = count(r; 0; 1) ≡ 0"
        ];

        private static string[] FunctionTestHelper(string s) => [
            "n = 1000",
            "a = random(fill(vector(n); 1))",
            $"c = {s}(a)",
            $"c_hp = {s}(hp(a))",
            "r = c ≡ c_hp",
            "check = count(r; 0; 1) ≡ 0"
        ];

        private static string[] ScalarTestHelper(string s) => [
            "n = 1000",
            "a = random(fill(vector(n); 1))",
            $"c = {s}(a)",
            $"c_hp = {s}(hp(a))",
            "r = c ≡ c_hp" 
        ];


        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAddition()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('+'));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSubtraction()
        {

            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('-'));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorMultiplication()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('*'));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('/'));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorForceDivisionBar()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('÷'));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorIntegerDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('\\'));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorModulo()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('⦼'));
            Assert.Equal(1, result);
        }

        #endregion

        #region HPVectorFunctions
        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sin"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorCos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cos"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorTan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("tan"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorCsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("csc"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sec"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorCot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cot"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sinh"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorCosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cosh"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorTanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("tanh"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorCsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("csch"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sech"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorCoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("coth"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAsin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asin"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAcos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acos"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAtan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atan"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAcsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsc"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAsec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asec"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAcot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acot"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAsinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asinh"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAcosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acosh"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAtanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atanh"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAcsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsch"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAsech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asech"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAcoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acoth"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorLog()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorLn()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ln"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorLog2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log_2"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorExp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("exp"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSqrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sqrt"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorCbrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cbrt"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorRoot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(["n = 1000",
                      "nth = 100",
                      "a = random(fill(vector(n); 1))",
                      "c = root(a; nth)",
                      "c_hp = root(hp(a); nth)",
                      "r = c ≡ c_hp",
                      "check = count(r; 0; 1) ≡ 0"]);
            Assert.Equal(1, result);
        }



        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorRound()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("round"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorFloor()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("floor"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorCeiling()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ceiling"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorTrunc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("trunc"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorMin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("min"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorMax()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("max"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSum()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(["n = 1000",
                      "a = random(fill(vector(n); 1))",
                      "c = sum(a)",
                      "c_hp = sum(hp(a))",
                      "abs(c - c_hp) < 10^-12*abs(c)"]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSumsq()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(["n = 1000",
                      "a = random(fill(vector(n); 1))",
                      "c = sumsq(a)",
                      "c_hp = sumsq(hp(a))",
                      "abs(c - c_hp) < 10^-12*abs(c)"]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSrss()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(["n = 1000",
                      "a = random(fill(vector(n); 1))",
                      "c = srss(a)",
                      "c_hp = srss(hp(a))",
                      "abs(c - c_hp) < 10^-12*abs(c)"]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorAverage()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(["n = 1000",
                      "a = random(fill(vector(n); 1))",
                      "c = average(a)",
                      "c_hp = average(hp(a))",
                      "abs(c - c_hp) < 10^-12*abs(c)"]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorProduct()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("product"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorMean()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mean"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorTake()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("take"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorLine()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("Line"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSpline()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("spline"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorSort()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sort"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorRsort()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("rsort"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorOrder()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("order"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorRevorder()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("revorder"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorReverse()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("reverse"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorNorm1()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(["n = 1000",
                      "a = random(fill(vector(n); 1))",
                      "c = norm_1(a)",
                      "c_hp = norm_1(hp(a))",
                      "abs(c - c_hp) < 10^-12*abs(c)"]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorNorm2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("norm_2"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorNormP()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(["n = 1000",
                            "a = random(fill(vector(n); 1))",
                            "c = norm_p(a; 5)",
                            "c_hp = norm_p(hp(a); 5)",
                            "c ≡ c_hp"]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorNormI()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("norm_i"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorUnit()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(["n = 1000",
                            "a = random(fill(vector(n); 1))",
                            "c = unit(a)",
                            "c_hp = unit(hp(a))",
                            "r = abs(c - c_hp) < 10^-12*abs(c)",
                            "check = count(r; 0; 1) ≡ 0"]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPVectorOperators")]
        public void HPVectorDot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(["n = 1000",
                      "a = random(fill(vector(n); 1))",
                      "b = random(fill(vector(n); 1))",
                      "c = dot(a; b)",
                      "c_hp = dot(hp(a); hp(b))",
                      "abs(c - c_hp) < 10^-12*abs(c)"]);
            Assert.Equal(1, result);
        }
        #endregion
    }
}
