using System.Security.Cryptography.X509Certificates;

namespace Calcpad.Tests
{
    public class HPSymmetricMatrixComparison
    {
        #region HPSymmetricMatrixOperators

        private const string RandomMatrixA = "a = random(mfill(symmetric(n); 1))";
        private const string RandomMatrixB = "b = random(mfill(symmetric(n); 1))";
        private const string OrthogonalMatrix = "q = submatrix(qr(random(mfill(matrix(n; n); 1))); 1; n; 1; n)";
        private const string WellConditionedMatrix = "a = q*vec2diag(0.55 + range(1; n; 1)/n)*transp(q)";


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
            "n = 500",
            "i = random(n - 1) + 1",
            "j = random(n - 1) + 1",
            RandomMatrixA,
            $"c = {func}(i; j; a)",
            $"c_hp = {func}(i; j; hp(a))",
            "r = abs(c - c_hp) ≤ 10^-14*abs(c)"
        ];

        private static readonly string[] PositiveDefiniteArray = [
            "n = 250",
            "a = symmetric(n)",
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
        [Trait("Category", "HPSymmetricMatrixOperators")]
        public void HPSymmetricMatrixAddition()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('+'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixOperators")]
        public void HPSymmetricMatrixSubtraction()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('-'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixOperators")]
        public void HPSymmetricMatrixMultiplication()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('*', "10^-11"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixOperators")]
        public void HPSymmetricMatrixDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('/'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixOperators")]
        public void HPSymmetricMatrixForceDivisionBar()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('÷'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixOperators")]
        public void HPSymmetricMatrixIntegerDivision()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('\\'));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixOperators")]
        public void HPSymmetricMatrixModulo()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(OperatorTestHelper('⦼'));
            Assert.Equal(0, result);
        }
        #endregion

        #region HPSymmetricMatrixFunctions
        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixTan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("tan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("csc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixTanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Tanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Csch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("Sech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("coth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAsin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asin"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAcos()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acos"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAtan()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atan"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAtan2()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAcsc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAsec()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asec"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAcot()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acot"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAsinh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asinh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAcosh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acosh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAtanh()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("atanh"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAcsch()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acsch"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAsech()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("asech"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAcoth()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("acoth"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixLog()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixLn()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ln"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixLog2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("log_2"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixExp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("exp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSqrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("sqrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCbrt()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("cbrt"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixRoot()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixRound()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("round"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixFloor()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("floor"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCeiling()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("ceiling"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixTrunc()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("trunc"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixMin()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("min"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixMax()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("max"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSum()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sum"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSumsq()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("sumsq"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSrss()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("srss"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAverage()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("average"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixProduct()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("product"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixMean()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mean"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixTake()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("take"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixLine()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("line"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSpline()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(InterpolationTestHelper("spline"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSortCols()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixRSortCols()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSortRows()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixRSortRows()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixOrderCols()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixRevOrderCols()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixOrderRows()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixMcount()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 500",
                "x = 1",
                "a = round(random(mfill(symmetric(n); 1000)))",
                "c = mcount(a; x)",
                "c_hp = mcount(hp(a); x)",
                "c ≡ c_hp"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixHprod()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixFprod()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixKprod()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixMnorm1()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(ScalarTestHelper("mnorm_1"));
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixMnorm2()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixMnormi()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCond1()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                OrthogonalMatrix,
                WellConditionedMatrix,
                "c = cond_1(a)",
                "c_hp = cond_1(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
                ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCond2()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 200",
                OrthogonalMatrix,
                WellConditionedMatrix,
                "c = cond_2(a)",
                "c_hp = cond_2(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCondE()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                OrthogonalMatrix,
                WellConditionedMatrix,
                "c = cond_e(a)",
                "c_hp = cond_e(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCondI()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                OrthogonalMatrix,
                WellConditionedMatrix,
                "c = cond_i(a)",
                "c_hp = cond_i(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixDet()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                OrthogonalMatrix,
                WellConditionedMatrix,
                "c = det(a)",
                "c_hp = det(hp(a))",
                "abs(c - c_hp) ≤ 10^-12*abs(c)"
            ]);
            Assert.Equal(1, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixRank()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixTrace()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixTransp()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(FunctionTestHelper("transp"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixAdj()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                OrthogonalMatrix,
                WellConditionedMatrix,
                "c = adj(a)",
                "c_hp = adj(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-8*abs(c)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCofactor()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                OrthogonalMatrix,
                WellConditionedMatrix,
                "c = cofactor(a)",
                "c_hp = cofactor(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-8*abs(c)",
                "mcount(r; 0)"
                ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixEigenvals()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                RandomMatrixA,
                "c = eigenvals(a)",
                "c_hp = eigenvals(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-10*abs(c)",
                "count(r; 0; 1)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixEigenvecs()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                RandomMatrixA,
                "c = eigenvecs(a)",
                "c_hp = eigenvecs(hp(a))",
                "r = abs(abs(c) - abs(c_hp)) ≤ 10^-10",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixEigen()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 200",
                RandomMatrixA,
                "c = eigen(a)",
                "c_hp = eigen(hp(a))",
                "r = abs(abs(c) - abs(c_hp)) ≤ 10^-10*max(abs(c);1)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCholesky()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(PositiveDefiniteTestHelper("cholesky", "10^-10"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixLu()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(PositiveDefiniteTestHelper("lu", "10^-14"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixQr()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSvd()
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
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixInverse()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "n = 250",
                OrthogonalMatrix,
                WellConditionedMatrix,
                "c = inverse(a)",
                "c_hp = inverse(hp(a))",
                "r = abs(c - c_hp) ≤ 10^-8*abs(c)",
                "mcount(r; 0)"
            ]);
            Assert.Equal(0, result);
        }
        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixLsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixEquationTestHelper("lsolve", "10^-12"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixClsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixEquationTestHelper("clsolve", "10^-8"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSlsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "Tol = 10^-5",
                "n = 250",
                "a = symmetric_hp(n)",
                "v = transp(hp([50000; 20000; 10000; 5000; 2000; 1000; 500; 200; 100; 50]))",
                "$Repeat{ add(v; a; i; i) @ i = 1 : n}",
                "b = random(fill(vector_hp(n); 1))",
                "c = slsolve(a; b)",
                "r = a * c - b ≤ 10^-5",
                "count(r; 0; 1)",
            ]);
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixMsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixMultiEquationTestHelper("msolve", "10^-12"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixCmsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run(MatrixMultiEquationTestHelper("cmsolve", "10^-8"));
            Assert.Equal(0, result);
        }

        [Fact]
        [Trait("Category", "HPSymmetricMatrixFunctions")]
        public void HPSymmetricMatrixSmsolve()
        {
            var calc = new TestCalc(new());
            var result = calc.Run([
                "Tol = 10^-5",
                "n = 250",
                "a = symmetric_hp(n)",
                "v = transp(hp([50000; 20000; 10000; 5000; 2000; 1000; 500; 200; 100; 50]))",
                "$Repeat{ add(v; a; i; i) @ i = 1 : n}",
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