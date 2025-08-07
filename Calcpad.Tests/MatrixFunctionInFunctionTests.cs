namespace Calcpad.Tests
{
    public class MatrixFunctionInFunctionTests
    {
        #region StructuralFunctions
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void MatrixCreation()
        {
            string m = "3";
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = matrix(a; b)", $"f({m}; {n})"]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void IdentityMatrixCreation()
        {
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = identity(a)", $"f({n})"]);
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void DiagonalMatrixCreation()
        {
            string n = "3";
            string d = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = diagonal(a; b)", $"f({n}; {d})"]);
            Assert.Equal("[7 0 0|0 7 0|0 0 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void ColumnMatrixCreation()
        {
            string m = "3";
            string c = "11";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = column(a; b)", $"f({m}; {c})"]);
            Assert.Equal("[11|11|11]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void UtriangMatrixCreation()
        {
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = utriang(a)", $"f({n})"]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void LtriangMatrixCreation()
        {
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = ltriang(a)", $"f({n})"]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void SymmetricMatrixCreation()
        {
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = symmetric(a)", $"f({n})"]);
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void Vec2diagMatrixCreation()
        {
            string v = "[11; 12; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = vec2diag(a)", $"f({v})"]);
            Assert.Equal("[11 0 0|0 12 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void Diag2vecMatrixCreation()
        {
            string m = "[11; 12; 17| 2; 4; 4| 1; 2; 5]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = diag2vec(a)", $"f({m})"]);
            Assert.Equal("[11 4 5]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void Vec2colMatrixCreation()
        {
            string v = "[1; 7; 5; 4; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = vec2col(a)", $"f({v})"]);
            Assert.Equal("[1|7|5|4|9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void JoinColsMatrixCreation()
        {
            string c1 = "[1; 7; 5; 4; 9]";
            string c2 = "[2; 4; 5; 19]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = join_cols(a; b)", $"f({c1}; {c2})"]);
            Assert.Equal("[1 2|7 4|5 5|4 19|9 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void JoinRowsMatrixCreation()
        {
            string r1 = "[1; 7; 5; 4; 9]";
            string r2 = "[2; 4; 5; 19]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = join_rows(a; b)", $"f({r1}; {r2})"]);
            Assert.Equal("[1 7 5 4 9|2 4 5 19 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void AugmentMatrixCreation()
        {
            string m1 = "[1|2]";
            string m2 = "[2; 7|5; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = augment(a; b)", $"f({m1}; {m2})"]);
            Assert.Equal("[1 2 7|2 5 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void StackMatrixCreation()
        {
            string m1 = "[1; 2| 7; 9]";
            string m2 = "[4; 4|11; 35]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = stack(a; b)", $"f({m1}; {m2})"]);
            Assert.Equal("[1 2|7 9|4 4|11 35]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void ResizeMatrix()
        {
            string M = "[1; 2; 4; 5| 7; 9; 9; 155]";
            string m = "2";
            string n = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = mresize(a; b; c)", $"f({M}; {m}; {n})"]);
            Assert.Equal("[1 2|7 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void MatrixFill()
        {
            string M = "[1; 2; 3|4; 7; 7|9; 9; 9]";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mfill(a; b)", $"f({M}; {x})"]);
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void MatrixCopy()
        {
            string A = "[1; 2; 3|4; 7; 7|9; 9; 9]";
            string B = "[2; 2; 7|11; 45; 6|1; 2; 9]";
            string i = "3";
            string j = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = copy(a; b; c; d)", $"f({A}; {B}; {i}; {j})"]);
            Assert.Equal("[2 2 7|11 45 6|1 2 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void MatrixAdd()
        {
            string A = "[1; 2; 3|4; 7; 7|9; 9; 9]";
            string B = "[2; 2; 7|11; 45; 6|1; 2; 9]";
            string i = "1";
            string j = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = add(a; b; c; d)", $"f({A}; {B}; {i}; {j})"]);
            Assert.Equal("[3 4 10|15 52 13|10 11 18]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void MatrixNrows()
        {
            string M = "[1; 2; 3|4; 7; 7|9; 9; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = n_rows(a)", $"f({M})"]);
            Assert.Equal("3", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void MatrixNcols()
        {
            string M = "[1; 2; 3|4; 7; 7|9; 9; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = n_cols(a)", $"f({M})"]);
            Assert.Equal("3", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void SubmatrixCreation()
        {
            string M = "[1; 2; 3|4; 7; 7|9; 9; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d; e) = submatrix(a; b; c; d; e)", $"f({M}; 2; 3; 2; 3)"]);
            Assert.Equal("[7 7|9 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void MatrixExtractRows()
        {
            string M = "[1; 2; 3|4; 7; 7|9; 9; 9]";
            string i = "[3; 3; 3]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = extract_rows(a; b)", $"f({M}; {i})"]);
            Assert.Equal("[9 9 9|9 9 9|9 9 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "StructuralFunctions")]
        public void MatrixExtractCols()
        {
            string M = "[1; 2; 3|4; 7; 7|9; 9; 9]";
            string i = "[1; 1; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = extract_cols(a; b)", $"f({M}; {i})"]);
            Assert.Equal("[1 1 1|4 4 4|9 9 9]", calc.ToString());
        }
        #endregion

        #region MathFunctions
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixHprod()
        {
            string A = "[1; 3; 5|7; 2; 6|4; 8; 9]";
            string B = "[2; 4; 6|1; 3; 9|5; 7; 3]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = hprod(a; b)", $"f({A}; {B})"]);
            Assert.Equal("[2 12 30|7 6 54|20 56 27]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixFprod()
        {
            string A = "[2; 5; 3|6; 7; 8|1; 4; 9]";
            string B = "[1; 6; 5|7; 3; 2|8; 5; 4]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = fprod(a; b)", $"f({A}; {B})"]);
            Assert.Equal("190", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixKprod()
        {
            string A = "[3; 2|4; 6]";
            string B = "[2; 5|1; 4]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = kprod(a; b)", $"f({A}; {B})"]);
            Assert.Equal("[6 15 4 10|3 12 2 8|8 20 12 30|4 16 6 24]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixMnorm()
        {
            string M = "[1; 4; 6|2; 5; 7|8; 3; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = mnorm(a)", $"f({M})"]);
            Assert.Equal("16.2", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixMnormE()
        {
            string M = "[2; 3; 4|5; 1; 7|6; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = mnorm_e(a)", $"f({M})"]);
            Assert.Equal("16.88", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixMnorm1()
        {
            string M = "[2; 3; 9|5; 6; 7|1; 8; 4]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = mnorm_1(a)", $"f({M})"]);
            Assert.Equal("20", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixMnorm2()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = mnorm_2(a)", $"f({M})"]);
            Assert.Equal("16.07", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixMnormi()
        {
            string M = "[4; 6; 5|3; 8; 2|7; 9; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = mnorm_i(a)", $"f({M})"]);
            Assert.Equal("17", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixCond()
        {
            string M = "[2; 1; 3|4; 7; 6|8; 5; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cond(a)", $"f({M})"]);
            Assert.Equal("29.42", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixCondE()
        {
            string M = "[3; 6; 5|8; 2; 4|9; 1; 7]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cond_e(a)", $"f({M})"]);
            Assert.Equal("10.71", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixCond1()
        {
            string M = "[5; 3; 1|9; 6; 7|2; 8; 4]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cond_1(a)", $"f({M})"]);
            Assert.Equal("11.67", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixCond2()
        {
            string M = "[6; 4; 2|3; 9; 8|5; 1; 7]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cond_2(a)", $"f({M})"]);
            Assert.Equal("3.88", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixCondi()
        {
            string M = "[4; 7; 9|5; 1; 6|8; 2; 3]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cond_i(a)", $"f({M})"]);
            Assert.Equal("10.7", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixDet()
        {
            string M = "[2; 3; 4|5; 6; 7|8; 9; 1]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = det(a)", $"f({M})"]);
            Assert.Equal("27", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixRank()
        {
            string M = "[1; 3; 5|7; 2; 6|4; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = rank(a)", $"f({M})"]);
            Assert.Equal("3", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixTrace()
        {
            string M = "[1; 2; 3|4; 5; 6|7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = trace(a)", $"f({M})"]);
            Assert.Equal("15", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixTransp()
        {
            string M = "[4; 1; 6|7; 2; 9|3; 8; 5]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = transp(a)", $"f({M})"]);
            Assert.Equal("[4 7 3|1 2 8|6 9 5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixAdj()
        {
            string M = "[2; 5; 7|3; 8; 1|6; 4; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = adj(a)", $"f({M})"]);
            Assert.Equal("[68 -17 -51|-21 -24 19|-36 22 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixCofactor()
        {
            string M = "[2; 4; 8|1; 7; 3|5; 9; 6]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cofactor(a)", $"f({M})"]);
            Assert.Equal("[15 9 -26|48 -28 2|-44 2 10]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixEigenvalues()
        {
            var calc = new TestCalc(new());
            calc.Run(["M = copy([1; 3; 1|0; 2; 5|0; 0; 9]; symmetric(3); 1; 1)",
                      "f(a) = eigenvals(a)",
                      "f(M)"]);
            Assert.Equal("[-2.27 2.19 12.09]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixEigenvectors()
        {
            var calc = new TestCalc(new());
            calc.Run(["M = copy([7; 8; 9|0; 5; 6|0; 0; 3]; symmetric(3); 1; 1)",
                      "f(a) = eigenvecs(a)",
                      "f(M)"]);
            Assert.Equal("[0.658 -0.0844 -0.748|-0.359 0.838 -0.411|0.661 0.539 0.521]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixCholeskyDecomp()
        {
            var calc = new TestCalc(new());
            calc.Run(["M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric(3); 1; 1)",
                      "f(a) = cholesky(a)",
                      "f(M)"]);
            Assert.Equal("[2 0.5 1|0 2.18 1.15|0 0 1.92]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixLUdecomp()
        {
            string M = "[6; 2; 1|8; 7; 3|9; 5; 4]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = lu(a)", $"f({M})"]);
            Assert.Equal("[6 2 1|1.33 4.33 1.67|1.5 0.462 1.73]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixSVDdecomp()
        {
            string M = "[2; 4; 6|1; 5; 3|9; 7; 8]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = svd(a)", $"f({M})"]);
            Assert.Equal("[-0.433 -0.526 0.732 16.38 -0.535 -0.564 -0.629|-0.32 -0.67 -0.67 3.62 0.83 -0.491 -0.266|-0.842 0.525 -0.122 1.92 -0.159 -0.664 0.73]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixInverse()
        {
            string M = "[5; 3; 2|1; 4; 7|6; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = inverse(a)", $"f({M})"]);
            Assert.Equal("[0.606 0.333 -0.394|-1 -1 1|0.485 0.667 -0.515]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixLsolve()
        {
            string v = "[5; 10; 7]";
            var calc = new TestCalc(new());
            calc.Run("M = [2; 3; 4|3; 5; 6|4; 6; 7]");
            calc.Run(["f(a; b) = lsolve(a; b)", $"f(M; {v})"]);
            Assert.Equal("[-11 5 3]", calc.ToString());
        }
        public void MatrixMsolve()
        {
            string v = "[5; 7|10; 14|7; 22]";
            var calc = new TestCalc(new());
            calc.Run("M = [2; 3; 4|3; 5; 6|4; 6; 7]");
            calc.Run(["f(a; b) = msolve(a; b)", $"f(M; {v})"]);
            Assert.Equal("[-11 9|5 7|3 -8]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixSymLsolve()
        {
            string v = "[5; 10; 7]";
            var calc = new TestCalc(new());
            calc.Run("M = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric(3); 1; 1)");
            calc.Run(["f(a; b) = lsolve(a; b)", $"f(M; {v})"]);
            Assert.Equal("[-11 5 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixSymClsolve()
        {
            string v = "[7; 14; 22]";
            var calc = new TestCalc(new());
            calc.Run("M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric(3); 1; 1)");
            calc.Run(["f(a; b) = clsolve(a; b)", $"f(M; {v})"]);
            Assert.Equal("[-0.1 0.857 3.27]", calc.ToString());
        }
        public void MatrixSymMsolve()
        {
            string v = "[5; 7|10; 14|7; 22]";
            var calc = new TestCalc(new());
            calc.Run("M = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric(3); 1; 1)");
            calc.Run(["f(a; b) = msolve(a; b)", $"f(M; {v})"]);
            Assert.Equal("[-11 9|5 7|3 -8]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "MathFunctions")]
        public void MatrixSymCmsolve()
        {
            string v = "[5; 7|10; 14|7; 22]";
            var calc = new TestCalc(new());
            calc.Run("M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric(3); 1; 1)");
            calc.Run(["f(a; b) = cmsolve(a; b)", $"f(M; {v})"]);
            Assert.Equal("[0.8 -0.1|1.86 0.857|-0.0286 3.27]", calc.ToString());
        }
        #endregion

        #region SortReorderFunctions
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void MatrixSortCols()
        {
            string M = "[2; 4; 6|1; 3; 9|5; 7; 3]";
            string i = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = sort_cols(a; b)", $"f({M}; {i})"]);
            Assert.Equal("[2 4 6|1 3 9|5 7 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void MatrixRSortCols()
        {
            string M = "[2; 4; 6|1; 3; 9|5; 7; 3]";
            string i = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = rsort_cols(a; b)", $"f({M}; {i})"]);
            Assert.Equal("[6 4 2|9 3 1|3 7 5]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void MatrixSortRows()
        {
            string M = "[2; 4; 6|1; 3; 9|5; 7; 3]";
            string j = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = sort_rows(a; b)", $"f({M}; {j})"]);
            Assert.Equal("[1 3 9|2 4 6|5 7 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void MatrixRSortRows()
        {
            string M = "[2; 4; 6|1; 3; 9|5; 7; 3]";
            string j = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = rsort_rows(a; b)", $"f({M}; {j})"]);
            Assert.Equal("[5 7 3|2 4 6|1 3 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void MatrixOrderCols()
        {
            string M = "[2; 4; 6|1; 3; 9|5; 7; 3]";
            string i = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = order_cols(a; b)", $"f({M}; {i})"]);
            Assert.Equal("[1 2 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void MatrixRevOrderCols()
        {
            string M = "[2; 4; 6|1; 3; 9|5; 7; 3]";
            string i = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = revorder_cols(a; b)", $"f({M}; {i})"]);
            Assert.Equal("[3 2 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void MatrixOrderRows()
        {
            string M = "[2; 4; 6|1; 3; 9|5; 7; 3]";
            string j = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = order_rows(a; b)", $"f({M}; {j})"]);
            Assert.Equal("[2 1 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "SortReorderFunctions")]
        public void MatrixRevOrderRows()
        {
            string M = "[2; 4; 6|1; 3; 9|5; 7; 3]";
            string j = "1";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = revorder_rows(a; b)", $"f({M}; {j})"]);
            Assert.Equal("[3 1 2]", calc.ToString());
        }
        #endregion

        #region CountSearchFunctions
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixCount()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mcount(a; b)", $"f({M}; {x})"]);
            Assert.Equal("3", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixSearch()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = msearch(a; b; c; d)", $"f({M}; {x}; 1; 1)"]);
            Assert.Equal("[1 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixFind()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mfind(a; b)", $"f({M}; {x})"]);
            Assert.Equal("[1 2 3|1 1 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixFindEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mfind_eq(a; b)", $"f({M}; {x})"]);
            Assert.Equal("[1 2 3|1 1 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixFindNotEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mfind_ne(a; b)", $"f({M}; {x})"]);
            Assert.Equal("[1 1 2 2 3 3|2 3 2 3 1 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixFindLessThan()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mfind_lt(a; b)", $"f({M}; {x})"]);
            Assert.Equal("[2|2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixFindLessOrEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mfind_le(a; b)", $"f({M}; {x})"]);
            Assert.Equal("[1 2 2 3|1 1 2 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixFindGreaterThan()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mfind_gt(a; b)", $"f({M}; {x})"]);
            Assert.Equal("[3|3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixFindGreaterOrEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mfind_ge(a; b)", $"f({M}; {x})"]);
            Assert.Equal("[2 3|3 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixHlookup()
        {
            string M = "[2; 3; 7|2; 1; 7|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = hlookup(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixHlookupEqual()
        {
            string M = "[2; 3; 7|2; 1; 7|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = hlookup_eq(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixHlookupNotEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = hlookup_ne(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[2 1 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixHlookupLessThan()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = hlookup_lt(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[2 1 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixHlookupLessOrEqual()
        {
            string M = "[2; 3; 7|2; 1; 7|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = hlookup_le(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[2 1 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixHlookupGreaterThan()
        {
            string M = "[2; 3; 10|2; 1; 4|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = hlookup_gt(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[4]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixHlookupGreaterOrEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = hlookup_ge(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixVlookup()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = vlookup(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[3 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixVlookupEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = vlookup_eq(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[3 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixVlookupNotEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = vlookup_ne(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixVlookupLessThan()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "15";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = vlookup_lt(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[3 1 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixVlookupLessOrEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = vlookup_le(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[3 1 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixVlookupGreaterThan()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = vlookup_gt(a; b; c; d)", $"f({M}; {x}; 1; 2)"]);
            Assert.Equal("[2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CountSearchFunctions")]
        public void MatrixVlookupGreaterOrEqual()
        {
            string M = "[2; 3; 4|2; 1; 7|6; 2; 9]";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c; d) = vlookup_ge(a; b; c; d)", $"f({M}; {x}; 1; 3)"]);
            Assert.Equal("[4 7 9]", calc.ToString());
        }
        #endregion

        #region ComparisonFunctions
        [Fact]
        [Trait("Category", "ComparisonFunctions")]
        public void MatrixMin()
        {
            string M = "[1; 3; 5|7; 2; 6|4; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = min(a)", $"f({M})"]);
            Assert.Equal("1", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ComparisonFunctions")]
        public void MatrixMax()
        {
            string M = "[1; 3; 5|7; 2; 6|4; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = max(a)", $"f({M})"]);
            Assert.Equal("9", calc.ToString());
        }
        #endregion

        #region CumulatativeFunctions
        [Fact]
        [Trait("Category", "CumulatativeFunctions")]
        public void MatrixSum()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sum(a)", $"f({M})"]);
            Assert.Equal("45", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CumulatativeFunctions")]
        public void MatrixSumSq()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sumsq(a)", $"f({M})"]);
            Assert.Equal("285", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CumulatativeFunctions")]
        public void MatrixSrss()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = srss(a)", $"f({M})"]);
            Assert.Equal("16.88", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CumulatativeFunctions")]
        public void MatrixAverage()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = average(a)", $"f({M})"]);
            Assert.Equal("5", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CumulatativeFunctions")]
        public void MatrixProduct()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = product(a)", $"f({M})"]);
            Assert.Equal("362880", calc.ToString());
        }
        [Fact]
        [Trait("Category", "CumulatativeFunctions")]
        public void MatrixMean()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = mean(a)", $"f({M})"]);
            Assert.Equal("4.15", calc.ToString());
        }
        #endregion

        #region InterpolationalFunctions
        [Fact]
        [Trait("Category", "InterpolationalFunctions")]
        public void MatrixTake()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = take(a; b; c)", $"f(1; 2; {M})"]);
            Assert.Equal("5", calc.ToString());
        }
        [Fact]
        [Trait("Category", "InterpolationalFunctions")]
        public void MatrixLine()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = line(a; b; c)", $"f(4; 5; {M})"]);
            Assert.Equal(" Undefined ", calc.ToString());
        }
        [Fact]
        [Trait("Category", "InterpolationalFunctions")]
        public void MatrixSpline()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b; c) = spline(a; b; c)", $"f(2; 2; {M})"]);
            Assert.Equal("2", calc.ToString());
        }
        #endregion

        #region LogicalFunctions
        [Fact]
        [Trait("Category", "LogicalFunctions")]
        public void MatrixNot()
        {
            string M = "[0; 5; 7|8; 2; 6|4; 1; 0]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = not(a)", $"f({M})"]);
            Assert.Equal("[1 0 0|0 0 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "LogicalFunctions")]
        public void MatrixOr()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = or(a)", $"f({M})"]);
            Assert.Equal("1", calc.ToString());
        }
        [Fact]
        [Trait("Category", "LogicalFunctions")]
        public void MatrixAnd()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = and(a)", $"f({M})"]);
            Assert.Equal("1", calc.ToString());
        }
        [Fact]
        [Trait("Category", "LogicalFunctions")]
        public void MatrixXor()
        {
            string M = "[3; 5; 7|8; 2; 6|4; 1; 9]";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = xor(a)", $"f({M})"]);
            Assert.Equal("1", calc.ToString());
        }
        #endregion

    }
}
