namespace Calcpad.Tests
{
    public class HPColumnMatrixComparison
    {
        #region HPColumnMatrixOperators

        private const string RandomMatrixA = "a = random(column(n; 1))";
        private const string RandomMatrixB = "b = random(column(n; 1))";

        private static string[] OperatorTestHelper(char o) => [
            "n = 500",
            RandomMatrixA,
            RandomMatrixB,
            $"c = a {o} b",
            "a_hp = hp(a)",
            "b_hp = hp(b)",
            $"c_hp = a_hp {o} b_hp",
            "r = c_hp ≡ c",
            "mcount(r; 0)"
        ];

        private static string[] FunctionTestHelper(string func) => [
            "n = 500",
            RandomMatrixA,
            $"c = {func}(a)",
            $"c_hp = {func}(hp(a))",
            "r = c ≡ c_hp",
            "mcount(r; 0)"
        ];

        private static string[] ScalarTestHelper(string func) => [
            "n = 500",
            RandomMatrixA,
            $"c = {func}(a)",
            $"c_hp = {func}(hp(a))",
            "r = abs(c - c_hp) ≤ 10^-14*abs(c)"
        ];

        private static string[] InterpolationTestHelper(string func) => [
            "n = 500",
            "i = random(n - 1) + 1",
            RandomMatrixA,
            $"c = {func}(i; 1; a)",
            $"c_hp = {func}(i; 1; hp(a))",
            "r = abs(c - c_hp) ≤ 10^-14*abs(c)"
];

        [Fact]
        [Trait("Category", "HPColumnMatrixOperators")]
        public void HPColumnMatrixAddition()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('+'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixOperators")]
        public void HPColumnMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('-'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixOperators")]
        public void HPColumnMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                RandomMatrixA,
                RandomMatrixB,
                "a = transp(a)",
                $"c = a * b",
                "a_hp = hp(a)",
                "b_hp = hp(b)",
                $"c_hp = a_hp * b_hp",
                "r = abs(c - c_hp) ≤ 10^-14*abs(c)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixOperators")]
        public void HPColumnMatrixDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('/'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixOperators")]
        public void HPColumnMatrixForceDivisionBar()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('÷'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixOperators")]
        public void HPColumnMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('\\'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixOperators")]
        public void HPColumnMatrixModulo()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('⦼'));
            Assert.Equal(0, result);
        }
        #endregion

        #region HPColumnMatrixFunctions
        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixCos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixTan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("tan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixCsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("csc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixCot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixCosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixTanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Tanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixCsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Csch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixCoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("coth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAsin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAcos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAtan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAtan2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                RandomMatrixA,
                RandomMatrixB,
                "c = atan2(a; b)",
                "c_hp = atan2(hp(a); hp(b))",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAcsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAsec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAcot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAsinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAcosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAtanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAcsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAsech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAcoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acoth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixLog()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixLn()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ln"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixLog2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log_2"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixExp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("exp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSqrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sqrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixCbrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cbrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixRoot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "nth = 4",
                RandomMatrixA,
                "c = root(a; nth)",
                "c_hp = root(hp(a); nth)",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixRound()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("round"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixFloor()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("floor"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixCeiling()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ceiling"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixTrunc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("trunc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixMin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("min"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixMax()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("max"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSum()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sum"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSumsq()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sumsq"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSrss()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("srss"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixAverage()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("average"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixProduct()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("product"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixMean()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mean"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixTake()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("take"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixLine()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("line"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSpline()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("spline"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSortRows()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "j = 50",
                RandomMatrixA,
                "c = sort_rows(a; j)",
                "c_hp = sort_rows(hp(a); j)",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixRSortRows()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "j = 50",
                RandomMatrixA,
                "c = rsort_rows(a; j)",
                "c_hp = rsort_rows(hp(a); j)",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixOrderRows()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "j = 1",
                RandomMatrixA,
                "c = order_rows(a; j)",
                "c_hp = order_rows(hp(a); j)",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixMcount()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "x = 1",
                "a = round(random(column(n; 1000)))",
                "c = mcount(a; x)",
                "c_hp = mcount(hp(a); x)",
                "c ≡ c_hp"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixHprod()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                RandomMatrixA,
                RandomMatrixB,
                "c = hprod(a; b)",
                "c_hp = hprod(hp(a); hp(b))",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixFprod()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                RandomMatrixA,
                RandomMatrixB,
                "c = fprod(a; b)",
                "c_hp = fprod(hp(a); hp(b))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixKprod()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 100",
                RandomMatrixA,
                RandomMatrixB,
                "c = kprod(a; b)",
                "c_hp = kprod(hp(a); hp(b))",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixMnorm1()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mnorm_1"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixMnorm2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 200",
                RandomMatrixA,
                "c = mnorm_2(a)",
                "c_hp = mnorm_2(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixMnormi()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                RandomMatrixA,
                "c = mnorm_i(a)",
                "c_hp = mnorm_i(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixRank()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 100",
                RandomMatrixA,
                "c = rank(a)",
                "c_hp = rank(hp(a))",
                "r = c ≡ c_hp"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixTransp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("transp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPColumnMatrixFunctions")]
        public void HPColumnMatrixSvd()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 200",
                RandomMatrixA,
                "c = svd(a)",
                "c_hp = svd(hp(a))",
                "r = abs(abs(c) - abs(c_hp)) ≤ 10^-8*max(abs(c); 1)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }
        #endregion
    }
}