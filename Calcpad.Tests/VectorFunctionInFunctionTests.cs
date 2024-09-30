namespace Calcpad.Tests
{
    public class VectorFunctionInFunctionTests
    {
        #region StructuralFunctions
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorCreation()
        {
            int n = 3;
            var calc = new TestCalc(new());
            calc.Run(["f(a) = vector(a)", $"f({n})"]);
            Assert.Equal("[0 0 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorFill()
        {
            string vector = "[0; 0; 0; 1; 2; 3]";
            int n = 4;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = fill(a; b)", $"f({vector}; {n})"]);
            Assert.Equal("[4 4 4 4 4 4]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorRange()
        {
            string x = "1";
            int y = 4;
            int s = 1;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = range(a; b; c)", $"f({x}; {y}; {s})"]);
            Assert.Equal("[1 2 3 4]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorJoin()
        {
            string vector1 = "[4; 8; 5; 2; 5; 1]";
            string vector2 = "[-3.5; 5; 4.25; 0]";
            int n = 3;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = join(a; b; c)", $"f({vector1}; {vector2}; {n})"]);
            Assert.Equal("[4 8 5 2 5 1 -3.5 5 4.25 0 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorResize()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            int n = 3;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = resize(a; b)", $"f({vector}; {n})"]);
            Assert.Equal("[0.5 0.866 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorLen()
        {
            string vector = "[1; 4; 4; 5; 2; 12; 54]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = len(a)", $"f({vector})"]);
            Assert.Equal("7", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorSize()
        {
            string vector = "[1; 4; 4; 5; 2; 12; 54]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = size(a)", $"f({vector})"]);
            Assert.Equal("7", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorSlice()
        {
            string vector = "[-3; 0; 4.5; -7; 2.25]";
            string x = "2";
            int y = 4;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = slice(a; b; c)", $"f({vector}; {x}; {y})"]);
            Assert.Equal("[0 4.5 -7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorFirst()
        {
            string vector = "[-0.5; 3; 0; -7.8; 5.6; 4.2; -6; 1.8; -3.1]";
            int n = 4;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = first(a; b)", $"f({vector}; {n})"]);
            Assert.Equal("[-0.5 3 0 -7.8]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorLast()
        {
            string vector = "[-0.5; 3; 0; -7.8; 5.6; 4.2; -6; 1.8; -3.1]";
            int n = 5;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = last(a; b)", $"f({vector}; {n})"]);
            Assert.Equal("[5.6 4.2 -6 1.8 -3.1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void VectorExtract()
        {
            string vector = "[8.2; 0; 6.9; -2.8; 3; 8.2; -7; 4.6]";
            string i = "[1; 3; 2; 4; 4; 5]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = extract(a; b)", $"f({vector}; {i})"]);
            Assert.Equal("[8.2 6.9 0 -2.8 -2.8 3]", calc.ToString());
        }
        [Fact]
        #endregion

        #region MathFunctions
        [Trait("Category", "MathFunctions")]
        public void VectorNorm1()
        {
            string vector = "[5; -2.1; 0; 7.8; -4.6]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = norm_1(a)", $"f({vector})"]);
            Assert.Equal("19.5", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void VectorNorm2()
        {
            string vector = "[5; -2.1; 0; 7.8; -4.6]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = norm_2(a)", $"f({vector})"]);
            Assert.Equal("10.56", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void VectorNormP()
        {
            string vector = "[5; -2.1; 0; 7.8; -4.6]";
            int p = 4;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = norm_p(a; b)", $"f({vector}; {p})"]); ;
            Assert.Equal("8.32", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void VectorNormInfinity()
        {
            string vector = "[5; -2.1; 0; 7.8; -4.6]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = norm_i(a)", $"f({vector})"]);
            Assert.Equal("7.8", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void VectorUnit()
        {
            string vector = "[5; -2.1; 0; 7.8; -4.6]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = unit(a)", $"f({vector})"]);
            Assert.Equal("[0.474 -0.199 0 0.739 -0.436]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void VectorDot()
        {
            string vector1 = "[5; -2.1; 0; 7.8; -4.6]";
            string vector2 = "[-3.5; 5; 4.25; 0; 10]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = dot(a; b)", $"f({vector1}; {vector2})"]);
            Assert.Equal("-74", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void VectorCross()
        {
            string vector1 = "[5; -2.1; 0]";
            string vector2 = "[4.25; 0; 10]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = cross(a; b)", $"f({vector1}; {vector2})"]);
            Assert.Equal("[-21 -50 8.93]", calc.ToString());
        }

        #endregion

        #region SortReorderFunctions
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void VectorSort()
        {
            string vector = "[2.9; -8; 0; 12; -6.5]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sort(a)", $"f({vector})"]);
            Assert.Equal("[-8 -6.5 0 2.9 12]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void VectorRsort()
        {
            string vector = "[2.9; -8; 0; 12; -6.5]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = rsort(a)", $"f({vector})"]);
            Assert.Equal("[12 2.9 0 -6.5 -8]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void VectorOrder()
        {
            string vector = "[2.9; -8; 0; 12; -6.5]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = order(a)", $"f({vector})"]);
            Assert.Equal("[2 5 3 1 4]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void VectorRevorder()
        {
            string vector = "[2.9; -8; 0; 12; -6.5]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = revorder(a)", $"f({vector})"]);
            Assert.Equal("[4 1 3 5 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void VectorReverse()
        {
            string vector = "[2.9; -8; 0; 12; -6.5]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = reverse(a)", $"f({vector})"]);
            Assert.Equal("[-6.5 12 0 -8 2.9]", calc.ToString());
        }
        #endregion

        #region SearchFunctions
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorCount()
        {
            string vector = "[12; -1.2; 5.25; 2; 8; 3; 4]";
            string x = "5.25";
            int i = 2;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = count(a; b; c)", $"f({vector}; {x}; {i})"]);
            Assert.Equal("1", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorSearch()
        {
            string vector = "[12; -1.2; 5.25; 2; 8; 3; 4]";
            string x = "2";
            int i = 3;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = search(a; b; c)", $"f({vector}; {x}; {i})"]);
            Assert.Equal("4", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorFind()
        {
            string vector = "[12; -1.2; 5.25; 2; 4; 4; 4]";
            string x = "4";
            int i = 1;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = find(a; b; c)", $"f({vector}; {x}; {i})"]);
            Assert.Equal("[5 6 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorFindNe()
        {
            string vector = "[12; -1.2; 5.25; 2; 8; 3; 4]";
            string x = "2";
            int i = 3;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = find_ne(a; b; c)", $"f({vector}; {x}; {i})"]);
            Assert.Equal("[3 5 6 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorFindLt()
        {
            string vector = "[12; -1.2; 5.25; 2; 8; 3; 4]";
            string x = "8";
            int i = 1;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = find_lt(a; b; c)", $"f({vector}; {x}; {i})"]);
            Assert.Equal("[2 3 4 6 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorFindLe()
        {
            string vector = "[12; -1.2; 5.25; 2; 8; 3; 4]";
            string x = "2";
            int i = 1;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = find_le(a; b; c)", $"f({vector}; {x}; {i})"]);
            Assert.Equal("[2 4]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorFindGt()
        {
            string vector = "[12; -1.2; 5.25; 2; 8; 3; 4]";
            string x = "8";
            int i = 1;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = find_gt(a; b; c)", $"f({vector}; {x}; {i})"]);
            Assert.Equal("[1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorFindGe()
        {
            string vector = "[12; -1.2; 5.25; 2; 8; 3; 4]";
            string x = "5.25";
            int i = 1;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = find_ge(a; b; c)", $"f({vector}; {x}; {i})"]);
            Assert.Equal("[1 3 5]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorLookup()
        {
            string vector1 = "[6.2; 6.2; 0; 6.2; 7.4]";
            string vector2 = "[5; -2.1; 0; 7.8; 11]";
            string x = "6.2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = lookup(a; b; c)", $"f({vector1}; {vector2}; {x})"]);
            Assert.Equal("[5 -2.1 7.8]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorLookupNe()
        {
            string vector1 = "[12; -1.2; 5.25; 2; 8]";
            string vector2 = "[5; -2.1; 0; 7.8; 11]";
            string x = "5.25";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = lookup_ne(a; b; c)", $"f({vector1}; {vector2}; {x})"]);
            Assert.Equal("[5 -2.1 7.8 11]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorLookupLt()
        {
            string vector1 = "[12; -1.2; 5.25; 2; 8; 3; 4]";
            string vector2 = "[1.5; 2.1; 3.2; 4.4; 5.5; 6.9; 7.111]";
            string x = "5.25";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = lookup_lt(a; b; c)", $"f({vector1}; {vector2}; {x})"]);
            Assert.Equal("[2.1 4.4 6.9 7.11]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorLookupLe()
        {
            string vector1 = "[12; -1.2; 5.25; 2; 8]";
            string vector2 = "[1; 6; 78; -24; 5.3]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = lookup_le(a; b; c)", $"f({vector1}; {vector2}; {x})"]);
            Assert.Equal("[6 78 -24]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorLookupGt()
        {
            string vector1 = "[12; -1.2; 5.25; 2; 8; 3; 4]";
            string vector2 = "[0; -6.7; 3.8; -2; 9.5; 7.2; 0; -1.3; 5]";
            string x = "4";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = lookup_gt(a; b; c)", $"f({vector1}; {vector2}; {x})"]);
            Assert.Equal("[0 3.8 9.5]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SearchFunctions")]
        public void VectorLookupGe()
        {
            string vector1 = "[12; -1.2; 5.25; 2; 8]";
            string vector2 = "[5; -2.1; 0; 7.8; -4.6]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = lookup_ge(a; b; c)", $"f({vector1}; {vector2}; {x})"]);
            Assert.Equal("[5 0 7.8 -4.6]", calc.ToString());
        }
        #endregion

        #region ComparisonFunctions
        [Fact]
        [Trait("Category", "ComparisonFunctions")]
        public void VectorMin()
        {
            string vector = "[77; -24; 1.1; 2.3; -5.6]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = min(a)", $"f({vector})"]);
            Assert.Equal("-24", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ComparisonFunctions")]
        public void VectorMax()
        {
            string vector = "[12; 44; -27; 56; 199.9; -2.4]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = max(a)", $"f({vector})"]);
            Assert.Equal("199.9", calc.ToString());
        }
        #endregion

        #region CumalatativeFunctions
        [Fact]
        [Trait("Category", "CumulativeFunctions")]
        public void VectorSum()
        {
            string vector = "[10.5; 23.2; -5.1; 8.4; 15.3]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sum(a)", $"f({vector})"]);
            Assert.Equal("52.3", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CumulativeFunctions")]
        public void VectorSumsq()
        {
            string vector = "[1.5; 2.3; -4.4; 6.2; -7.1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sumsq(a)", $"f({vector})"]);
            Assert.Equal("115.75", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CumulativeFunctions")]
        public void VectorSrss()
        {
            string vector = "[3; 4; 12]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = srss(a)", $"f({vector})"]);
            Assert.Equal("13", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CumulativeFunctions")]
        public void VectorAverage()
        {
            string vector = "[5.5; 7.3; -2.8; 4.0; 10.1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = average(a)", $"f({vector})"]);
            Assert.Equal("4.82", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CumulativeFunctions")]
        public void VectorProduct()
        {
            string vector = "[1.2; 3.0; -2.0; 4.5; 5.0]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = product(a)", $"f({vector})"]);
            Assert.Equal("-162", calc.ToString());
        }

        [Fact]
        [Trait("Category", "CumulativeFunctions")]
        public void VectorMean()
        {
            string vector = "[9.1; 15.4; 7.3; 12.0; -3.5]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = mean(a)", $"f({vector})"]);
            Assert.Equal(" Undefined ", calc.ToString());
        }
        #endregion

        #region InterpolationalFunctions
        [Fact]
        [Trait("Category", "InterpolationalFunctions")]
        public void VectorTake()
        {
            string vector = "[0; 3.29; 2; -1.5; 7]";
            string x = "3.8";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = take(a; b)", $"f({x}; {vector})"]);
            Assert.Equal("-1.5", calc.ToString());
        }
        [Fact]
        [Trait("Category", "InterpolationalFunctions")]
        public void VectorLine()
        {
            string vector = "[0; 3.29; 2; -1.5; 7]";
            string x = "4.5";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = line(a; b)", $"f({x}; {vector})"]);
            Assert.Equal("2.75", calc.ToString());
        }
        [Fact]
        [Trait("Category", "InterpolationalFunctions")]
        public void VectorSpline()
        {
            string vector = "[0; 3.29; 2; -1.5; 7]";
            string x = "4.5";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = spline(a; b)", $"f({x}; {vector})"]);
            Assert.Equal("1.39", calc.ToString());
        }
        #endregion

        #region LogicalFunctions
        [Fact]
        [Trait("Category", "LogicalFunctions")]
        public void VectorNot()
        {
            string vector = "[1; 0; 22; 3.7; 55; -6.3; 0]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = not(a)", $"f({vector})"]);
            Assert.Equal("[0 1 0 0 0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "LogicalFunctions")]
        public void VectorOr()
        {
            string vector = "[1; 0; 22; 3.7; 55; -6.3; 0]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = or(a)", $"f({vector})"]);
            Assert.Equal("1", calc.ToString());
        }
        [Fact]
        [Trait("Category", "LogicalFunctions")]
        public void VectorAnd()
        {
            string vector = "[1; 0; 22; 3.7; 55; -6.3; 0]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = and(a)", $"f({vector})"]);
            Assert.Equal("0", calc.ToString());
        }
        [Fact]
        [Trait("Category", "LogicalFunctions")]
        public void VectorXor()
        {
            string vector = "[1; 0; 22; 3.7; 55; -6.3; 0]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = xor(a)", $"f({vector})"]);
            Assert.Equal("1", calc.ToString());
        }
        #endregion

    }
}
