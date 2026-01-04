namespace Calcpad.Tests
{
    public class HPMatrixComparison
    {
        #region HPMatrixOperators

        private const string RandomMatrixA = "a = random(mfill(matrix(m; n); 1))";
        private const string RandomMatrixB = "b = random(mfill(matrix(m; n); 1))";
        private const string RandomSquareMatrix = "a = random(mfill(matrix(n; n); 1))";
        private const string WellConditionedMatrix = "a = submatrix(qr(random(mfill(matrix(n; n); 1))); 1; n; 1; n)";

        private static string[] OperatorTestHelper(char o) => [
            "m = 500", 
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
            "s = symmetric(n)",
            "v = transp([50000; 20000; 10000; 5000; 2000; 1000; 500; 200; 100; 50])",
            "$Repeat{add(v; s; i; i) @ i = 1 : n}",
            "a = copy(s; matrix(n; n); 1; 1)",
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
        [Trait("Category", "HPMatrixOperators")]
        public void HPMatrixAddition()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('+'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixOperators")]
        public void HPMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('-'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixOperators")]
        public void HPMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('*'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixOperators")]
        public void HPMatrixDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('/'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixOperators")]
        public void HPMatrixForceDivisionBar()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('÷'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixOperators")]
        public void HPMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('\\'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixOperators")]
        public void HPMatrixModulo()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('⦼'));
            Assert.Equal(0, result);
        }
        #endregion

        #region HPMatrixFunctions
        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixTan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("tan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("csc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixTanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Tanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Csch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("coth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAsin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAcos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAtan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAtan2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAcsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAsec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAcot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAsinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAcosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAtanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAcsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAsech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAcoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acoth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixLog()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixLn()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ln"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixLog2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log_2"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixExp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("exp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSqrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sqrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCbrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cbrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixRoot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixRound()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("round"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixFloor()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("floor"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCeiling()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ceiling"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixTrunc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("trunc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixMin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("min"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixMax()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("max"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSum()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sum"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSumsq()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sumsq"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSrss()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("srss"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAverage()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("average"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixProduct()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("product"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixMean()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mean"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixTake()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("take"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixLine()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("line"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSpline()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("spline"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSortCols()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixRSortCols()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSortRows()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixRSortRows()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixOrderCols()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixRevOrderCols()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixOrderRows()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixMcount()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
                "n = 500",
                "x = 1",
                "a = round(random(mfill(matrix(m; n); 1000)))",
                "c = mcount(a; x)",
                "c_hp = mcount(hp(a); x)",
                "c ≡ c_hp"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixHprod()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixFprod()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixKprod()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 75",
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixMnorm1()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mnorm_1"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixMnorm2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 200",
                "n = 200",
                RandomMatrixA,
                "c = mnorm_2(a)",
                "c_hp = mnorm_2(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixMnormi()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "m = 250",
                "n = 500",
                RandomMatrixA,
                "c = mnorm_i(a)",
                "c_hp = mnorm_i(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCond1()
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCond2()
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCondE()
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCondI()
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixDet()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                RandomSquareMatrix,
                "c = det(a)",
                "c_hp = det(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixRank()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 100",
                RandomSquareMatrix,
                "c = rank(a)",
                "c_hp = rank(hp(a))",
                "r = c ≡ c_hp"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixTrace()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                RandomSquareMatrix,
                "c = trace(a)",
                "c_hp = trace(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-12"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixTransp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("transp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixAdj()
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixCofactor()
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixLu()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(PositiveDefiniteTestHelper("lu", "10^-14"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixQr()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                RandomSquareMatrix,
                "c = qr(a)",
                "c_hp = qr(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-12",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixSvd()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 200",
                RandomSquareMatrix,
                "c = svd(a)",
                "c_hp = svd(hp(a))",
                "r = abs(abs(c) - abs(c_hp)) ≤ 10^-8*max(abs(c); 1)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixInverse()
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
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixLsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixEquationTestHelper("lsolve", "10^-12"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPMatrixFunctions")]
        public void HPMatrixMsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixMultiEquationTestHelper("msolve", "10^-12"));
            Assert.Equal(0, result);
        }
        #endregion
    }
}