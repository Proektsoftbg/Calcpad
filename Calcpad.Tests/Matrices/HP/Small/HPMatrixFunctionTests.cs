namespace Calcpad.Tests
{
    public class HPMatrixFunctionTests
    {
        #region HPStructuralFunctions
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPMatrixCreation()
        {
            string m = "3";
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run($"matrix_hp({m};{n})");
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPIdentityMatrixCreation()
        {
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run($"identity_hp({n})");
            Assert.Equal("[1 0 0|0 1 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPDiagonalMatrixCreation()
        {
            string n = "3";
            string d = "7";
            var calc = new TestCalc(new());
            calc.Run($"diagonal_hp({n}; {d})");
            Assert.Equal("[7 0 0|0 7 0|0 0 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPColumnMatrixCreation()
        {
            string m = "3";
            string c = "11";
            var calc = new TestCalc(new());
            calc.Run($"column_hp({m}; {c})");
            Assert.Equal("[11|11|11]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPUtriangMatrixCreation()
        {
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run($"utriang_hp({n})");
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPLtriangMatrixCreation()
        {
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run($"ltriang_hp({n})");
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPSymmetricMatrixCreation()
        {
            string n = "3";
            var calc = new TestCalc(new());
            calc.Run($"symmetric_hp({n})");
            Assert.Equal("[0 0 0|0 0 0|0 0 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPVec2diagMatrixCreation()
        {
            string v = "hp([11; 12; 1])";
            var calc = new TestCalc(new());
            calc.Run($"vec2diag({v})");
            Assert.Equal("[11 0 0|0 12 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPDiag2vecMatrixCreation()
        {
            string m = "hp([11; 12; 17| 2; 4; 4| 1; 2; 5])";
            var calc = new TestCalc(new());
            calc.Run($"diag2vec({m})");
            Assert.Equal("[11 4 5]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPVec2colMatrixCreation()
        {
            string v = "hp([1; 7; 5; 4; 9])";
            var calc = new TestCalc(new());
            calc.Run($"vec2col({v})");
            Assert.Equal("[1|7|5|4|9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPJoinColsMatrixCreation()
        {
            string c1 = "hp([1; 7; 5; 4; 9])";
            string c2 = "hp([2; 4; 5; 19])";
            var calc = new TestCalc(new());
            calc.Run($"join_cols({c1};{c2})");
            Assert.Equal("[1 2|7 4|5 5|4 19|9 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPJoinRowsMatrixCreation()
        {
            string r1 = "hp([1; 7; 5; 4; 9])";
            string r2 = "hp([2; 4; 5; 19])";
            var calc = new TestCalc(new());
            calc.Run($"join_rows({r1};{r2})");
            Assert.Equal("[1 7 5 4 9|2 4 5 19 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPAugmentMatrixCreation()
        {
            string m1 = "hp([1|2])";
            string m2 = "hp([2; 7|5; 9])";
            var calc = new TestCalc(new());
            calc.Run($"augment({m1};{m2})");
            Assert.Equal("[1 2 7|2 5 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPStackMatrixCreation()
        {
            string m1 = "hp([1; 2| 7; 9])";
            string m2 = "hp([4; 4|11; 35])";
            var calc = new TestCalc(new());
            calc.Run($"stack({m1};{m2})");
            Assert.Equal("[1 2|7 9|4 4|11 35]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPResizeMatrix()
        {
            string M = "hp([1; 2; 4; 5| 7; 9; 9; 155])";
            string m = "2";
            string n = "2";
            var calc = new TestCalc(new());
            calc.Run($"mresize({M};{m};{n})");
            Assert.Equal("[1 2|7 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPMatrixFill()
        {
            string M = "hp([1; 2; 3|4; 7; 7|9; 9; 9])";
            string x = "1";
            var calc = new TestCalc(new());
            calc.Run($"mfill({M};{x})");
            Assert.Equal("[1 1 1|1 1 1|1 1 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPMatrixCopy()
        {
            string A = "hp([1; 2; 3|4; 7; 7|9; 9; 9])";
            string B = "hp([2; 2; 7|11; 45; 6|1; 2; 9])";
            string i = "3";
            string j = "3";
            var calc = new TestCalc(new());
            calc.Run($"copy({A};{B};{i};{j})");
            Assert.Equal("[2 2 7|11 45 6|1 2 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPMatrixAdd()
        {
            string A = "hp([1; 2; 3|4; 7; 7|9; 9; 9])";
            string B = "hp([2; 2; 7|11; 45; 6|1; 2; 9])";
            string i = "1";
            string j = "1";
            var calc = new TestCalc(new());
            calc.Run($"add({A};{B};{i};{j})");
            Assert.Equal("[3 4 10|15 52 13|10 11 18]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPMatrixNrows()
        {
            string M = "hp([1; 2; 3|4; 7; 7|9; 9; 9])";
            var calc = new TestCalc(new());
            calc.Run($"n_rows({M})");
            Assert.Equal("3", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPMatrixNcols()
        {
            string M = "hp([1; 2; 3|4; 7; 7|9; 9; 9])";
            var calc = new TestCalc(new());
            calc.Run($"n_cols({M})");
            Assert.Equal("3", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPSubmatrixCreation()
        {
            string M = "hp([1; 2; 3|4; 7; 7|9; 9; 9])";
            var calc = new TestCalc(new());
            calc.Run($"submatrix({M}; 2; 3; 2; 3)");
            Assert.Equal("[7 7|9 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPMatrixExtractRows()
        {
            string M = "hp([1; 2; 3|4; 7; 7|9; 9; 9])";
            string i = "hp([3; 3; 3])";
            var calc = new TestCalc(new());
            calc.Run($"extract_rows({M}; {i})");
            Assert.Equal("[9 9 9|9 9 9|9 9 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPStructuralFunctions")]
        public void HPMatrixExtractCols()
        {
            string M = "hp([1; 2; 3|4; 7; 7|9; 9; 9])";
            string i = "hp([1; 1; 1])";
            var calc = new TestCalc(new());
            calc.Run($"extract_cols({M}; {i})");
            Assert.Equal("[1 1 1|4 4 4|9 9 9]", calc.ToString());
        }
        #endregion

        #region HPMathFunctions
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixHprod()
        {
            string A = "hp([1; 3; 5|7; 2; 6|4; 8; 9])";
            string B = "hp([2; 4; 6|1; 3; 9|5; 7; 3])";
            var calc = new TestCalc(new()); 
            calc.Run($"hprod({A}; {B})");
            Assert.Equal("[2 12 30|7 6 54|20 56 27]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixFprod()
        {
            string A = "hp([2; 5; 3|6; 7; 8|1; 4; 9])";
            string B = "hp([1; 6; 5|7; 3; 2|8; 5; 4])";
            var calc = new TestCalc(new());
            calc.Run($"fprod({A}; {B})");
            Assert.Equal("190", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixKprod()
        {
            string A = "hp([3; 2|4; 6])";
            string B = "hp([2; 5|1; 4])";
            var calc = new TestCalc(new());
            calc.Run($"kprod({A}; {B})");
            Assert.Equal("[6 15 4 10|3 12 2 8|8 20 12 30|4 16 6 24]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixMnorm()
        {
            string M = "hp([1; 4; 6|2; 5; 7|8; 3; 9])";
            var calc = new TestCalc(new());
            calc.Run($"mnorm({M})");
            Assert.Equal("16.2", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixMnormE()
        {
            string M = "hp([2; 3; 4|5; 1; 7|6; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run($"mnorm_e({M})");
            Assert.Equal("16.88", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixMnorm1()
        {
            string M = "hp([2; 3; 9|5; 6; 7|1; 8; 4])";
            var calc = new TestCalc(new());
            calc.Run($"mnorm_1({M})");
            Assert.Equal("20", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixMnorm2()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"mnorm_2({M})");
            Assert.Equal("16.07", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixMnormi()
        {
            string M = "hp([4; 6; 5|3; 8; 2|7; 9; 1])";
            var calc = new TestCalc(new());
            calc.Run($"mnorm_i({M})");
            Assert.Equal("17", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixCond()
        {
            string M = "hp([2; 1; 3|4; 7; 6|8; 5; 9])";
            var calc = new TestCalc(new());
            calc.Run($"cond({M})");
            Assert.Equal("29.42", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixCondE()
        {
            string M = "hp([3; 6; 5|8; 2; 4|9; 1; 7])";
            var calc = new TestCalc(new());
            calc.Run($"cond_e({M})");
            Assert.Equal("10.71", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixCond1()
        {
            string M = "hp([5; 3; 1|9; 6; 7|2; 8; 4])";
            var calc = new TestCalc(new());
            calc.Run($"cond_1({M})");
            Assert.Equal("11.67", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixCond2()
        {
            string M = "hp([6; 4; 2|3; 9; 8|5; 1; 7])";
            var calc = new TestCalc(new());
            calc.Run($"cond_2({M})");
            Assert.Equal("3.88", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixCondi()
        {
            string M = "hp([4; 7; 9|5; 1; 6|8; 2; 3])";
            var calc = new TestCalc(new());
            calc.Run($"cond_i({M})");
            Assert.Equal("10.7", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixDet()
        {
            string M = "hp([2; 3; 4|5; 6; 7|8; 9; 1])";
            var calc = new TestCalc(new());
            calc.Run($"det({M})");
            Assert.Equal("27", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixRank()
        {
            string M = "hp([1; 3; 5|7; 2; 6|4; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run($"rank({M})");
            Assert.Equal("3", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixTrace()
        {
            string M = "hp([1; 2; 3|4; 5; 6|7; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run($"trace({M})");
            Assert.Equal("15", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixTransp()
        {
            string M = "hp([4; 1; 6|7; 2; 9|3; 8; 5])";
            var calc = new TestCalc(new());
            calc.Run($"transp({M})");
            Assert.Equal("[4 7 3|1 2 8|6 9 5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixAdj()
        {
            string M = "hp([2; 5; 7|3; 8; 1|6; 4; 9])";
            var calc = new TestCalc(new());
            calc.Run($"adj({M})");
            Assert.Equal("[68 -17 -51|-21 -24 19|-36 22 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixCofactor()
        {
            string M = "hp([2; 4; 8|1; 7; 3|5; 9; 6])";
            var calc = new TestCalc(new());
            calc.Run($"cofactor({M})");
            Assert.Equal("[15 9 -26|48 -28 2|-44 2 10]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixEigenvalues()
        {
            var calc = new TestCalc(new());
            calc.Run(["M = copy([1; 3; 1|0; 2; 5|0; 0; 9]; symmetric_hp(3); 1; 1)",
                      "eigenvals(M)"]);
            Assert.Equal("[-2.27 2.19 12.09]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixEigenvectors()
        {
            var calc = new TestCalc(new());
            calc.Run(["M = copy([7; 8; 9|0; 5; 6|0; 0; 3]; symmetric_hp(3); 1; 1)",
                      "eigenvecs(M)"]);
            Assert.Equal("[0.658 -0.0844 -0.748|-0.359 0.838 -0.411|0.661 0.539 0.521]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixCholeskyDecomp()
        {
            var calc = new TestCalc(new());
            calc.Run(["M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric_hp(3); 1; 1)",
                    "cholesky(M)"]);
            Assert.Equal("[2 0.5 1|0 2.18 1.15|0 0 1.92]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixLUdecomp()
        {
            string M = "hp([6; 2; 1|8; 7; 3|9; 5; 4])";
            var calc = new TestCalc(new());
            calc.Run($"lu({M})");
            Assert.Equal("[6 2 1|1.33 4.33 1.67|1.5 0.462 1.73]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSVDdecomp()
        {
            string M = "hp([2; 4; 6|1; 5; 3|9; 7; 8])";
            var calc = new TestCalc(new());
            calc.Run($"svd({M})");
            Assert.Equal("[-0.433 -0.526 0.732 16.38 -0.535 -0.564 -0.629|-0.32 -0.67 -0.67 3.62 0.83 -0.491 -0.266|-0.842 0.525 -0.122 1.92 -0.159 -0.664 0.73]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixInverse()
        {
            string M = "hp([5; 3; 2|1; 4; 7|6; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run($"inverse({M})");
            Assert.Equal("[0.606 0.333 -0.394|-1 -1 1|0.485 0.667 -0.515]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixLsolve()
        {
            string v = "hp([5; 10; 7])";
            var calc = new TestCalc(new());
            calc.Run("M = hp([2; 3; 4|3; 5; 6|4; 6; 7])");
            calc.Run($"lsolve(M; {v})");
            Assert.Equal("[-11 5 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixMsolve()
        {
            string v = "hp([5; 7|10; 14|7; 22])";
            var calc = new TestCalc(new());
            calc.Run("M = hp([2; 3; 4|3; 5; 6|4; 6; 7])");
            calc.Run($"msolve(M; {v})");
            Assert.Equal("[-11 9|5 7|3 -8]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSymLsolve()
        {
            string v = "hp([5; 10; 7])";
            var calc = new TestCalc(new());
            calc.Run("M = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric_hp(3); 1; 1)");
            calc.Run($"lsolve(M; {v})");
            Assert.Equal("[-11 5 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSymClsolve()
        {
            string v = "hp([7; 14; 22])";
            var calc = new TestCalc(new());
            calc.Run("M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric_hp(3); 1; 1)");
            calc.Run($"clsolve(M; {v})");
            Assert.Equal("[-0.1 0.857 3.27]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSymSlsolve()
        {
            string v = "hp([7; 14; 22])";
            var calc = new TestCalc(new());
            calc.Run("M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric_hp(3); 1; 1)");
            calc.Run($"slsolve(M; {v})");
            Assert.Equal("[-0.1 0.857 3.27]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSymMsolve()
        {
            string v = "hp([5; 7|10; 14|7; 22])";
            var calc = new TestCalc(new());
            calc.Run("M = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric_hp(3); 1; 1)");
            calc.Run($"msolve(M; {v})");
            Assert.Equal("[-11 9|5 7|3 -8]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSymCmsolve()
        {
            string v = "hp([5; 7|10; 14|7; 22])";
            var calc = new TestCalc(new());
            calc.Run("M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric_hp(3); 1; 1)");
            calc.Run($"cmsolve(M; {v})");
            Assert.Equal("[0.8 -0.1|1.86 0.857|-0.0286 3.27]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSymSmsolve()
        {
            string v = "hp([5; 7|10; 14|7; 22])";
            var calc = new TestCalc(new());
            calc.Run("M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric_hp(3); 1; 1)");
            calc.Run($"smsolve(M; {v})");
            Assert.Equal("[0.8 -0.1|1.86 0.857|-0.0286 3.27]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSymVecMsolve()
        {
            string vector = "hp([5; 7; 10])";
            var calc = new TestCalc(new());
            calc.Run("M = copy([2; 3; 4|0; 5; 6|0; 0; 7]; symmetric_hp(3); 1; 1)");
            calc.Run([$"msolve(M; {vector})"]);
            Assert.Equal("[4|-1|0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSymVecCmsolve()
        {
            string vector = "hp([5; 7; 10])";
            var calc = new TestCalc(new());
            calc.Run("M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric_hp(3); 1; 1)");
            calc.Run([$"cmsolve(M; {vector})"]);
            Assert.Equal("[0.5|0.571|1.21]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPMathFunctions")]
        public void HPMatrixSymVecSmsolve()
        {
            string vector = "hp([5| 7| 10])";
            var calc = new TestCalc(new());
            calc.Run("M = copy([4; 1; 2|0; 5; 3|0; 0; 6]; symmetric_hp(3); 1; 1)");
            calc.Run([$"smsolve(M; {vector})"]);
            Assert.Equal("[0.5|0.571|1.21]", calc.ToString());
        }
        #endregion

        #region HPSortReorderFunctions
        [Fact]
        [Trait("Category", "HPSortReorderFunctions")]
        public void HPMatrixSortCols()
        {
            string M = "hp([2; 4; 6|1; 3; 9|5; 7; 3])";
            string i = "1";
            var calc = new TestCalc(new());
            calc.Run($"sort_cols({M};{i})");
            Assert.Equal("[2 4 6|1 3 9|5 7 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPSortReorderFunctions")]
        public void HPMatrixRSortCols()
        {
            string M = "hp([2; 4; 6|1; 3; 9|5; 7; 3])";
            string i = "1";
            var calc = new TestCalc(new());
            calc.Run($"rsort_cols({M};{i})");
            Assert.Equal("[6 4 2|9 3 1|3 7 5]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPSortReorderFunctions")]
        public void HPMatrixSortRows()
        {
            string M = "hp([2; 4; 6|1; 3; 9|5; 7; 3])";
            string j = "2";
            var calc = new TestCalc(new());
            calc.Run($"sort_rows({M};{j})");
            Assert.Equal("[1 3 9|2 4 6|5 7 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPSortReorderFunctions")]
        public void HPMatrixRSortRows()
        {
            string M = "hp([2; 4; 6|1; 3; 9|5; 7; 3])";
            string j = "2";
            var calc = new TestCalc(new());
            calc.Run($"rsort_rows({M};{j})");
            Assert.Equal("[5 7 3|2 4 6|1 3 9]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPSortReorderFunctions")]
        public void HPMatrixOrderCols()
        {
            string M = "hp([2; 4; 6|1; 3; 9|5; 7; 3])";
            string i = "2";
            var calc = new TestCalc(new());
            calc.Run($"order_cols({M};{i})");
            Assert.Equal("[1 2 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPSortReorderFunctions")]
        public void HPMatrixRevOrderCols()
        {
            string M = "hp([2; 4; 6|1; 3; 9|5; 7; 3])";
            string i = "1";
            var calc = new TestCalc(new());
            calc.Run($"revorder_cols({M};{i})");
            Assert.Equal("[3 2 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPSortReorderFunctions")]
        public void HPMatrixOrderRows()
        {
            string M = "hp([2; 4; 6|1; 3; 9|5; 7; 3])";
            string j = "1";
            var calc = new TestCalc(new());
            calc.Run($"order_rows({M};{j})");
            Assert.Equal("[2 1 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPSortReorderFunctions")]
        public void HPMatrixRevOrderRows()
        {
            string M = "hp([2; 4; 6|1; 3; 9|5; 7; 3])";
            string j = "1";
            var calc = new TestCalc(new());
            calc.Run($"revorder_rows({M};{j})");
            Assert.Equal("[3 1 2]", calc.ToString());
        }
        #endregion

        #region HPCountSearchFunctions
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixCount()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"mcount({M};{x})");
            Assert.Equal("3", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixSearch()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"msearch({M};{x}; 1; 1)");
            Assert.Equal("[1 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixFind()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"mfind({M};{x})");
            Assert.Equal("[1 2 3|1 1 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixFindEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"mfind_eq({M};{x})");
            Assert.Equal("[1 2 3|1 1 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixFindNotEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"mfind_ne({M};{x})");
            Assert.Equal("[1 1 2 2 3 3|2 3 2 3 1 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixFindLessThan()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"mfind_lt({M};{x})");
            Assert.Equal("[2|2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixFindLessOrEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"mfind_le({M};{x})");
            Assert.Equal("[1 2 2 3|1 1 2 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixFindGreaterThan()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"mfind_gt({M};{x})");
            Assert.Equal("[3|3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixFindGreaterOrEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"mfind_ge({M};{x})");
            Assert.Equal("[2 3|3 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixHlookup()
        {
            string M = "hp([2; 3; 7|2; 1; 7|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"hlookup({M};{x}; 1; 2)");
            Assert.Equal("[7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixHlookupEqual()
        {
            string M = "hp([2; 3; 7|2; 1; 7|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"hlookup_eq({M};{x}; 1; 2)");
            Assert.Equal("[7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixHlookupNotEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"hlookup_ne({M};{x}; 1; 2)");
            Assert.Equal("[2 1 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixHlookupLessThan()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"hlookup_lt({M};{x}; 1; 2)");
            Assert.Equal("[2 1 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixHlookupLessOrEqual()
        {
            string M = "hp([2; 3; 7|2; 1; 7|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"hlookup_le({M};{x}; 1; 2)");
            Assert.Equal("[2 1 7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixHlookupGreaterThan()
        {
            string M = "hp([2; 3; 10|2; 1; 4|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"hlookup_gt({M};{x}; 1; 2)");
            Assert.Equal("[4]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixHlookupGreaterOrEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"hlookup_ge({M};{x}; 1; 2)");
            Assert.Equal("[]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixVlookup()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"vlookup({M};{x}; 1; 2)");
            Assert.Equal("[3 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixVlookupEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"vlookup_eq({M};{x}; 1; 2)");
            Assert.Equal("[3 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixVlookupNotEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"vlookup_ne({M};{x}; 1; 2)");
            Assert.Equal("[2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixVlookupLessThan()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "15";
            var calc = new TestCalc(new());
            calc.Run($"vlookup_lt({M};{x}; 1; 2)");
            Assert.Equal("[3 1 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixVlookupLessOrEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "7";
            var calc = new TestCalc(new());
            calc.Run($"vlookup_le({M};{x}; 1; 2)");
            Assert.Equal("[3 1 2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixVlookupGreaterThan()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"vlookup_gt({M};{x}; 1; 2)");
            Assert.Equal("[2]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCountSearchFunctions")]
        public void HPMatrixVlookupGreaterOrEqual()
        {
            string M = "hp([2; 3; 4|2; 1; 7|6; 2; 9])";
            string x = "2";
            var calc = new TestCalc(new());
            calc.Run($"vlookup_ge({M};{x}; 1; 3)");
            Assert.Equal("[4 7 9]", calc.ToString());
        }
        #endregion

        #region HPComparisonFunctions
        [Fact]
        [Trait("Category", "HPComparisonFunctions")]
        public void HPMatrixMin()
        {
            string M = "hp([1; 3; 5|7; 2; 6|4; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run($"min({M})");
            Assert.Equal("1", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPComparisonFunctions")]
        public void HPMatrixMax()
        {
            string M = "hp([1; 3; 5|7; 2; 6|4; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run($"max({M})");
            Assert.Equal("9", calc.ToString());
        }
        #endregion

        #region HPCumulatativeFunctions
        [Fact]
        [Trait("Category", "HPCumulatativeFunctions")]
        public void HPMatrixSum()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"sum({M})");
            Assert.Equal("45", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCumulatativeFunctions")]
        public void HPMatrixSumSq()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"sumsq({M})");
            Assert.Equal("285", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCumulatativeFunctions")]
        public void HPMatrixSrss()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"srss({M})");
            Assert.Equal("16.88", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCumulatativeFunctions")]
        public void HPMatrixAverage()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"average({M})");
            Assert.Equal("5", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCumulatativeFunctions")]
        public void HPMatrixProduct()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"product({M})");
            Assert.Equal("362880", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPCumulatativeFunctions")]
        public void HPMatrixMean()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"mean({M})");
            Assert.Equal("4.15", calc.ToString());
        }
        #endregion

        #region HPInterpolationalFunctions
        [Fact]
        [Trait("Category", "HPInterpolationalFunctions")]
        public void HPMatrixTake()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"take(1; 2; {M})");
            Assert.Equal("5", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPInterpolationalFunctions")]
        public void HPMatrixLine()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"line(4; 5; {M})");
            Assert.Equal(" Undefined ", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPInterpolationalFunctions")]
        public void HPMatrixSpline()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"spline(2; 2; {M})");
            Assert.Equal("2", calc.ToString());
        }
        #endregion

        #region HPLogicalFunctions
        [Fact]
        [Trait("Category", "HPLogicalFunctions")]
        public void HPMatrixNot()
        {
            string M = "hp([0; 5; 7|8; 2; 6|4; 1; 0])";
            var calc = new TestCalc(new());
            calc.Run($"not({M})");
            Assert.Equal("[1 0 0|0 0 0|0 0 1]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPLogicalFunctions")]
        public void HPMatrixOr()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"or({M})");
            Assert.Equal("1", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPLogicalFunctions")]
        public void HPMatrixAnd()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"and({M})");
            Assert.Equal("1", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPLogicalFunctions")]
        public void HPMatrixXor()
        {
            string M = "hp([3; 5; 7|8; 2; 6|4; 1; 9])";
            var calc = new TestCalc(new());
            calc.Run($"xor({M})");
            Assert.Equal("1", calc.ToString());
        }
        #endregion

    }
}
