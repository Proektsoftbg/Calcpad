namespace Calcpad.Tests
{
    public class HPUpperTriangularMatrixComparison
    {
        #region HPUpperTriangularMatrixOperators

        private const string RandomMatrixA = "a = random(mfill(utriang(n); 1))";
        private const string RandomMatrixB = "b = random(mfill(utriang(n); 1))";
        private const string WellConditionedMatrix = "a = diagonal(n; 1) + random(mfill(utriang(n); 0.1))";

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
            "m = 250",
            "n = 500",
            RandomMatrixA,
            $"c = {func}(a)",
            $"c_hp = {func}(hp(a))",
            "r = c ≡ c_hp",
            "mcount(r; 0)"
        ];

        private static string[] ScalarTestHelper(string func) => [
            "m = 250",
            "n = 500",
            RandomMatrixA,
            $"c = {func}(a)",
            $"c_hp = {func}(hp(a))",
            "r = abs(c - c_hp) ≤ 10^-14*abs(c)"
        ];

        private static string[] InterpolationTestHelper(string func) => [
            "m = 250",
            "n = 500",
            "i = random(m - 1) + 1",
            "j = random(n - 1) + 1",
            RandomMatrixA,
            $"c = {func}(i; j; a)",
            $"c_hp = {func}(i; j; hp(a))",
            "r = abs(c - c_hp) ≤ 10^-14*abs(c)"
];

        private static readonly string[] PositiveDefiniteArray = [
            "n = 250",
            "a = utriang(n)",
            "v = transp([50000; 20000; 10000; 5000; 2000; 1000; 500; 200; 100; 50])",
            "$Repeat{add(v; a; i; i) @ i = 1 : n}",
        ];

        private static string[] PositiveDefiniteTestHelper(string func, string tol) =>
            PositiveDefiniteArray.Concat([
            $"c = {func}(a)",
            $"c_hp = {func}(hp(a))",
            $"r = abs(c - c_hp) ≤ {tol}*abs(c)",
            "mcount(r; 0)"
        ]).ToArray();

        private static string[] MatrixEquationTestHelper(string func, string tol) =>
            PositiveDefiniteArray.Concat([
            "b = random(fill(vector(n); 1))",
            $"c = {func}(a; b)",
            $"c_hp = {func}(hp(a); hp(b))",
            $"r = abs(c - c_hp) ≤ {tol}*abs(c)",
            "count(r; 0; 1)"
         ]).ToArray();

        private static string[] MatrixMultiEquationTestHelper(string func, string tol) =>
            PositiveDefiniteArray.Concat([
            "b = random(mfill(matrix(n; 2); 1))",
            $"c = {func}(a; b)",
            $"c_hp = {func}(hp(a); hp(b))",
            $"r = abs(c - c_hp) ≤ {tol}*abs(c)",
            "mcount(r; 0)"
         ]).ToArray();

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixOperators")]
        public void HPUpperTriangularMatrixAddition()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('+'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixOperators")]
        public void HPUpperTriangularMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('-'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixOperators")]
        public void HPUpperTriangularMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('*'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixOperators")]
        public void HPUpperTriangularMatrixDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('/'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixOperators")]
        public void HPUpperTriangularMatrixForceDivisionBar()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('÷'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixOperators")]
        public void HPUpperTriangularMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('\\'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixOperators")]
        public void HPUpperTriangularMatrixModulo()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('⦼'));
            Assert.Equal(0, result);
        }
        #endregion

        #region HPUpperTriangularMatrixFunctions
        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixTan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("tan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("csc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixTanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Tanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Csch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("coth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAsin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAcos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAtan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAtan2()
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
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAcsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAsec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAcot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAsinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAcosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAtanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAcsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAsech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAcoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acoth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixLog()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixLn()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ln"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixLog2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log_2"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixExp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("exp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSqrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sqrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCbrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cbrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixRoot()
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
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixRound()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("round"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixFloor()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("floor"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCeiling()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ceiling"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixTrunc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("trunc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixMin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("min"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixMax()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("max"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSum()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sum"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSumsq()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sumsq"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSrss()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("srss"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAverage()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("average"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixProduct()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("product"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixMean()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mean"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixTake()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("take"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixLine()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("line"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSpline()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("spline"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSortCols()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "i = 50",
                RandomMatrixA,
                "c = sort_cols(a; i)",
                "c_hp = sort_cols(hp(a); i)",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixRSortCols()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "i = 50",
                RandomMatrixA,
                "c = rsort_cols(a; i)",
                "c_hp = rsort_cols(hp(a); i)",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSortRows()
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
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixRSortRows()
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
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixOrderCols()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "i = 50",
                RandomMatrixA,
                "c = order_cols(a; i)",
                "c_hp = order_cols(hp(a); i)",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixRevOrderCols()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "i = 50",
                RandomMatrixA,
                "c = revorder_cols(a; i)",
                "c_hp = revorder_cols(hp(a); i)",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixOrderRows()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "j = 50",
                RandomMatrixA,
                "c = order_rows(a; j)",
                "c_hp = order_rows(hp(a); j)",
                "r = c ≡ c_hp",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixMcount()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "x = 1",
                "a = round(random(mfill(utriang(n); 1000)))",
                "c = mcount(a; x)",
                "c_hp = mcount(hp(a); x)",
                "c ≡ c_hp"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixHprod()
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
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixFprod()
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
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixKprod()
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
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixMnorm1()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mnorm_1"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixMnorm2()
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
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixMnormi()
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
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCond1()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                WellConditionedMatrix,
                "c = cond_1(a)",
                "c_hp = cond_1(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
                ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCond2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 200",
                WellConditionedMatrix,
                "c = cond_2(a)",
                "c_hp = cond_2(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCondE()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                WellConditionedMatrix,
                "c = cond_e(a)",
                "c_hp = cond_e(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCondI()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                WellConditionedMatrix,
                "c = cond_i(a)",
                "c_hp = cond_i(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixDet()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                WellConditionedMatrix,
                "c = det(a)",
                "c_hp = det(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixRank()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 100",
                "n = 100",
                WellConditionedMatrix,
                "c = rank(a)",
                "c_hp = rank(hp(a))",
                "r = c ≡ c_hp"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixTrace()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                RandomMatrixA,
                "c = trace(a)",
                "c_hp = trace(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-12"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixTransp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("transp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixAdj()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                WellConditionedMatrix,
                "c = adj(a)",
                "c_hp = adj(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-8*abs(c)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixCofactor()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                WellConditionedMatrix,
                "c = cofactor(a)",
                "c_hp = cofactor(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-8*abs(c)",
                "mcount(r; 0)"
                ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixLu()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(PositiveDefiniteTestHelper("lu", "10^-14"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixQr()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                RandomMatrixA,
                "c = qr(a)",
                "c_hp = qr(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-12",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixSvd()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 200",
                WellConditionedMatrix,
                "c = svd(a)",
                "c_hp = svd(hp(a))",
                "r = abs(abs(c) - abs(c_hp)) ≤ 10^-8*max(abs(c); 1)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixInverse()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                WellConditionedMatrix,
                "c = inverse(a)",
                "c_hp = inverse(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-8*abs(c)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }
        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixLsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixEquationTestHelper("lsolve", "10^-12"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPUpperTriangularMatrixFunctions")]
        public void HPUpperTriangularMatrixMsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixMultiEquationTestHelper("msolve", "10^-12"));
            Assert.Equal(0, result);
        }
        #endregion
    }
}