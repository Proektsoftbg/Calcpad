namespace Calcpad.Tests
{
    public class HDiagonalMatrixComparison
    {
        #region HPDiagonalMatrixOperators

        private const string RandomMatrixA = "a = random(diagonal(n; 1))";
        private const string RandomMatrixB = "b = random(diagonal(n; 1))";
        private const string WellConditionedMatrix = "a = vec2diag((0.55 + range(0; n - 1; 1))/n)";

        private static string[] OperatorTestHelper(char o, string tol = "0") => [
            "n = 500",
            RandomMatrixA,
            RandomMatrixB,
            $"c = a {o} b",
            "a_hp = hp(a)",
            "b_hp = hp(b)",
            $"c_hp = a_hp {o} b_hp",
            $"r = if({tol} ≡ 0; c_hp ≡ c; abs(c_hp - c) ≤ {tol})",
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
            "m = 250",
            "n = 500",
            "i = random(m - 1) + 1",
            "j = random(n - 1) + 1",
            RandomMatrixA,
            $"c = {func}(i; j; a)",
            $"c_hp = {func}(i; j; hp(a))",
            "r = abs(c - c_hp) ≤ 10^-14*abs(c)"
];

        private static string[] PositiveDefiniteTestHelper(string func, string tol) => [
            "n = 250",
            WellConditionedMatrix,
            $"c = {func}(a)",
            $"c_hp = {func}(hp(a))",
            $"r = abs(c - c_hp) ≤ {tol}*abs(c)",
            "mcount(r; 0)"
        ];

        private static string[] MatrixEquationTestHelper(string func, string tol) => [
            "n = 250",
            WellConditionedMatrix,
            "b = random(fill(vector(n); 1))",
            $"c = {func}(a; b)",
            $"c_hp = {func}(hp(a); hp(b))",
            $"r = abs(c - c_hp) ≤ {tol}*abs(c)",
            "count(r; 0; 1)"
        ];

        private static string[] MatrixMultiEquationTestHelper(string func, string tol) => [
            "n = 250",
            WellConditionedMatrix,
            "b = random(mfill(matrix(n; 2); 1))",
            $"c = {func}(a; b)",
            $"c_hp = {func}(hp(a); hp(b))",
            $"r = abs(c - c_hp) ≤ {tol}*abs(c)",
            "mcount(r; 0)"
        ];

        [Fact]
        [Trait("Category", "HPDiagonalMatrixOperators")]
        public void HPDiagonalMatrixAddition()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('+'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixOperators")]
        public void HPDiagonalMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('-'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixOperators")]
        public void HPDiagonalMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('*'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixOperators")]
        public void HPDiagonalMatrixDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('/'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixOperators")]
        public void HPDiagonalMatrixForceDivisionBar()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('÷'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixOperators")]
        public void HPDiagonalMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('\\'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixOperators")]
        public void HPDiagonalMatrixModulo()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('⦼'));
            Assert.Equal(0, result);
        }
        #endregion

        #region HPDiagonalMatrixFunctions
        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixTan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("tan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("csc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixTanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Tanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Csch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("coth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAsin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAcos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAtan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAtan2()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAcsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAsec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAcot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAsinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAcosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAtanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAcsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAsech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAcoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acoth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixLog()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixLn()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ln"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixLog2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log_2"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixExp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("exp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSqrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sqrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCbrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cbrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixRoot()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixRound()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("round"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixFloor()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("floor"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCeiling()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ceiling"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixTrunc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("trunc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixMin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("min"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixMax()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("max"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSum()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sum"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSumsq()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sumsq"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSrss()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("srss"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAverage()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("average"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixProduct()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("product"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixMean()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mean"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixTake()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("take"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixLine()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("line"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSpline()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("spline"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSortCols()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixRSortCols()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSortRows()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixRSortRows()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixOrderCols()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixRevOrderCols()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixOrderRows()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixMcount()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "x = 1",
                "a = round(random(diagonal(n; 1000)))",
                "c = mcount(a; x)",
                "c_hp = mcount(hp(a); x)",
                "c ≡ c_hp"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixHprod()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixFprod()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixKprod()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixMnorm1()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mnorm_1"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixMnorm2()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixMnormi()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCond1()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCond2()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCondE()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCondI()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixDet()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                RandomMatrixA,
                "c = det(a)",
                "c_hp = det(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixRank()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixTrace()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixTransp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("transp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixAdj()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCofactor()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixEigenvals()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                WellConditionedMatrix,
                "c = eigenvals(a)",
                "c_hp = eigenvals(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-12*abs(c)",
                "count(r; 0; 1)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixEigenvecs()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
               WellConditionedMatrix,
                "c = eigenvecs(a)",
                "c_hp = eigenvecs(hp(a))",
                "r = abs(abs(c) - abs(c_hp)) ≤ 10^-12",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixEigen()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 200",
                WellConditionedMatrix,
                "c = eigen(a)",
                "c_hp = eigen(hp(a))",
                "r = abs(abs(c) - abs(c_hp)) ≤ 10^-12*max(abs(c);1)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCholesky()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(PositiveDefiniteTestHelper("cholesky", "10^-10"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixLu()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(PositiveDefiniteTestHelper("lu", "10^-14"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixQr()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSvd()
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

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixInverse()
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
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixLsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixEquationTestHelper("lsolve", "10^-12"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixClsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixEquationTestHelper("clsolve", "10^-8"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSlsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "Tol = 10^-5",
                "n = 250",
                "a = vec2diag((1000 + range_hp(0; n - 1; 1))/1234)",
                "b = random(fill(vector_hp(n); 1))",
                "c = slsolve(a; b)",
                "r = a * c - b ≤ 10^-5",
                "count(r; 0; 1)",
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixMsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixMultiEquationTestHelper("msolve", "10^-8"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixCmsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixMultiEquationTestHelper("cmsolve", "10^-8"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPDiagonalMatrixFunctions")]
        public void HPDiagonalMatrixSmsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "Tol = 10^-5",
                "n = 250",
                "a = vec2diag((1000 + range_hp(0; n - 1; 1))/1234)",
                "b = random(mfill(matrix_hp(n; 2); 1))",
                "c = smsolve(a; b)",
                "r = a * c - b ≤ 10^-5",
                "mcount(r; 0)",
            ]);
            Assert.Equal(0, result);
        }
        #endregion
    }
}