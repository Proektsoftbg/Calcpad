﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.Tests
{
    public class MatrixScalarFunctionTests
    {
        #region ScalarFunctions
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixSin()
        {
            string matrix = "[30; 45; 60| 90; 120; 135| 150; 180; 210]";
            var calc = new TestCalc(new());
            calc.Run($"round(Sin({matrix})*1000)/1000");
            Assert.Equal("[0.5 0.707 0.866|1 0.866 0.707|0.5 0 -0.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixCos()
        {
            string matrix = "[0; 30; 45| 60; 90; 120| 150; 180; 210]";
            var calc = new TestCalc(new());
            calc.Run($"round(Cos({matrix})*1000)/1000");
            Assert.Equal("[1 0.866 0.707|0.5 0 -0.5|-0.866 -1 -0.866]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixTan()
        {
            string matrix = "[0; 30; 45| 60; 120; 150| 180; 210; 360]";
            var calc = new TestCalc(new());
            calc.Run($"round(Tan({matrix})*1000)/1000");
            Assert.Equal("[0 0.577 1|1.73 -1.73 -0.577|0 0.577 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixCot()
        {
            string matrix = "[30; 45; 60| 0; 120; 135| 150; 0; 210]";
            var calc = new TestCalc(new());
            calc.Run($"Cot({matrix})");
            Assert.Equal("[1.73 1 0.577|+∞ -0.577 -1|-1.73 +∞ 1.73]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixSec()
        {
            string matrix = "[0; 30; 45| 60; 120; 150| 180; 210; 360]";
            var calc = new TestCalc(new());
            calc.Run($"round(sec({matrix})*100)/100");
            Assert.Equal("[1 1.15 1.41|2 -2 -1.15|-1 -1.15 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixCsc()
        {
            string matrix = "[0; 30; 60| 45; 90; 120| 150; 0; 270]";
            var calc = new TestCalc(new());
            calc.Run($"Csc({matrix})");
            Assert.Equal("[+∞ 2 1.15|1.41 1 1.15|2 +∞ -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixSinh()
        {
            string matrix = "[0.1; 0.3; 0.6| 1.2; -1.3; -0.7| 0.4; -0.4; 0.9]";
            var calc = new TestCalc(new());
            calc.Run($"Sinh({matrix})");
            Assert.Equal("[0.1 0.305 0.637|1.51 -1.7 -0.759|0.411 -0.411 1.03]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixCosh()
        {
            string matrix = "[-2;-1; 0|1; 1.5; -1.5|0.5; -0.5; 2]";
            var calc = new TestCalc(new());
            calc.Run($"Cosh({matrix})");
            Assert.Equal("[3.76 1.54 1|1.54 2.35 2.35|1.13 1.13 3.76]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixTanh()
        {
            string matrix = "[0.1; 0.3; 0.6| 1.2; -1.3; -0.7| 0.4; -0.4; 0.9]";
            var calc = new TestCalc(new());
            calc.Run($"Tanh({matrix})");
            Assert.Equal("[0.0997 0.291 0.537|0.834 -0.862 -0.604|0.38 -0.38 0.716]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixCoth()
        {
            string matrix = "[0.1; 0.3; 0.6| 1.2; -1.3; -0.7| 0.4; -0.4; 0.9]";
            var calc = new TestCalc(new());
            calc.Run($"Coth({matrix})");
            Assert.Equal("[10.03 3.43 1.86|1.2 -1.16 -1.65|2.63 -2.63 1.4]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixSech()
        {
            string matrix = "[0.1; 0.3; 0.6| 1.2; -1.3; -0.7| 0.4; -0.4; 0.9]";
            var calc = new TestCalc(new());
            calc.Run($"Sech({matrix})");
            Assert.Equal("[0.995 0.957 0.844|0.552 0.507 0.797|0.925 0.925 0.698]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixCsch()
        {
            string matrix = "[0; 0.5; 1| -0.5; -1; 0.707| 0.866; -0.866; 1.5]";
            var calc = new TestCalc(new());
            calc.Run($"Csch({matrix})");
            Assert.Equal("[+∞ 1.92 0.851|-1.92 -0.851 1.3|1.02 -1.02 0.47]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAsin()
        {
            string matrix = "[0.5; -0.5; 1| 0.866; -0.866; -1| 0.707; -0.707; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Asin({matrix})");
            Assert.Equal("[30 -30 90|60 -60 -90|44.99 -44.99 0]", calc.ToString());
        }
        [Fact]
        public void MatrixAcos()
        {
            string matrix = "[0.5; -0.5; 1| 0.866; -0.866; -1| 0.707; -0.707; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Acos({matrix})");
            Assert.Equal("[60 120 0|30 150 180|45.01 134.99 90]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAtan()
        {
            string matrix = "[0.2588; 0.5; -0.2588| 0.866; -0.866; -0.5| 0.707; 0; -0.707]";
            var calc = new TestCalc(new());
            calc.Run($"Atan({matrix})");
            Assert.Equal("[14.51 26.57 -14.51|40.89 -40.89 -26.57|35.26 0 -35.26]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAcsc()
        {
            string matrix = "[0.2588; 0.5; -0.2588| 0.866; -0.866; -0.5| 0.707; 0; -0.707]";
            var calc = new TestCalc(new());
            calc.Run($"Acsc({matrix})");
            Assert.Equal("[ Undefined   Undefined   Undefined | Undefined   Undefined   Undefined | Undefined  +∞  Undefined ]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAsec()
        {
            string matrix = "[0.577; -0.577; 1| 0.577; 0.707; -1| 0; -1; 0.5]";
            var calc = new TestCalc(new());
            calc.Run($"Asec({matrix})");
            Assert.Equal("[ Undefined   Undefined  0| Undefined   Undefined  180|+∞ 180  Undefined ]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAcot()
        {
            string matrix = "[0.577; -0.577; 1| 0.577; 0.707; -1| 0; -1; 0.5]";
            var calc = new TestCalc(new());
            calc.Run($"Acot({matrix})");
            Assert.Equal("[60.02 -60.02 45|60.02 54.74 -45|90 -45 63.43]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAtan2()
        {
            string matrix1 = "[0.577; -0.577; 1| 0.577; 0.707; -1| 0; -1; 0.5]";
            string matrix2 = "[0.5; -0.5; 1| 0.866; -0.866; -1| 0.707; -0.707; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Atan2({matrix1}; {matrix2})");
            Assert.Equal("[40.91 -139.09 45|56.33 -50.77 -135|90 -144.74 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAsinh()
        {
            string matrix = "[1.2; 0.5; -0.5| 1.1; -1.2; 0.8| 0.9; -0.9; 1.5]";
            var calc = new TestCalc(new());
            calc.Run($"Asinh({matrix})");
            Assert.Equal("[1.02 0.481 -0.481|0.95 -1.02 0.733|0.809 -0.809 1.19]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAcosh()
        {
            string matrix = "[1.2; 0.5; -0.5| 1.1; -1.2; 0.8| 0.9; -0.9; 1.5]";
            var calc = new TestCalc(new());
            calc.Run($"Acosh({matrix})");
            Assert.Equal("[0.622  Undefined   Undefined |0.444  Undefined   Undefined | Undefined   Undefined  0.962]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAtanh()
        {
            string matrix = "[1.2; 0.5; -0.5| 1.1; -1.2; 0.8| 0.9; -0.9; 1.5]";
            var calc = new TestCalc(new());
            calc.Run($"Atanh({matrix})");
            Assert.Equal("[ Undefined  0.549 -0.549| Undefined   Undefined  1.1|1.47 -1.47  Undefined ]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAcsch()
        {
            string matrix = "[1.2; 0.5; -0.5| 1.1; -1.2; 0.8| 0.9; -0.9; 1.5]";
            var calc = new TestCalc(new());
            calc.Run($"Acsch({matrix})");
            Assert.Equal("[0.758 1.44 -1.44|0.816 -0.758 1.05|0.958 -0.958 0.625]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAsech()
        {
            string matrix = "[0.7; 0.6; -0.6| 1.3; -1.5; 0.4| 0.2; -0.3; 1.1]";
            var calc = new TestCalc(new());
            calc.Run($"Asech({matrix})");
            Assert.Equal("[0.896 1.1  Undefined | Undefined   Undefined  1.57|2.29  Undefined   Undefined ]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixAcoth()
        {
            string matrix = "[0.7; 0.6; -0.6| 1.3; -1.5; 0.4| 0.2; -0.3; 1.1]";
            var calc = new TestCalc(new());
            calc.Run($"Acoth({matrix})");
            Assert.Equal("[ Undefined   Undefined   Undefined |1.02 -0.805  Undefined | Undefined   Undefined  1.52]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixLn()
        {
            string matrix = "[0.1; 0.5; 0.9| 1.5; 2.5; 5.0| 7.5; 10; 15]";
            var calc = new TestCalc(new());
            calc.Run($"ln({matrix})");
            Assert.Equal("[-2.3 -0.693 -0.105|0.405 0.916 1.61|2.01 2.3 2.71]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixLog()
        {
            string matrix = "[1; 10; 100| 2; 20; 200| 5; 50; 500]";
            var calc = new TestCalc(new());
            calc.Run($"log({matrix})");
            Assert.Equal("[0 1 2|0.301 1.3 2.3|0.699 1.7 2.7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixLog2()
        {
            string matrix = "[1; 2; 4| 8; 16; 32| 64; 128; 256]";
            var calc = new TestCalc(new());
            calc.Run($"log_2({matrix})");
            Assert.Equal("[0 1 2|3 4 5|6 7 8]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixExp()
        {
            string matrix = "[2; 3; 4| 1; 5; 6| 2.5; 3.5; 7]";
            var calc = new TestCalc(new());
            calc.Run($"exp({matrix})");
            Assert.Equal("[7.39 20.09 54.6|2.72 148.41 403.43|12.18 33.12 1096.63]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixSqr()
        {
            string matrix = "[1; 2; 3| 4; 5; 6| 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run($"sqr({matrix})");
            Assert.Equal("[1 1.41 1.73|2 2.24 2.45|2.65 2.83 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixSqrt()
        {
            string matrix = "[1; 2; 3| 4; 5; 6| 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run($"sqrt({matrix})");
            Assert.Equal("[1 1.41 1.73|2 2.24 2.45|2.65 2.83 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixCbrt()
        {
            string matrix = "[1; 2; 3| 4; 5; 6| 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run($"cbrt({matrix})");
            Assert.Equal("[1 1.26 1.44|1.59 1.71 1.82|1.91 2 2.08]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixRoot()
        {
            string matrix = "[1; 2; 3| 4; 5; 6| 7; 8; 9]";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run($"root({matrix}; {x})");
            Assert.Equal("[1 1.26 1.44|1.59 1.71 1.82|1.91 2 2.08]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixRound()
        {
            string matrix = "[1.4; 2.5; 3.5| 4.7; 5.8; 6.11| 7.12; 8.34; 9.7]";
            var calc = new TestCalc(new());
            calc.Run($"round({matrix})");
            Assert.Equal("[1 3 4|5 6 6|7 8 10]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixFloor()
        {
            string matrix = "[1.7; 2.8; 3.9| 4.1; 5.3; 6.4| 7.7; 8.9; 9.99]";
            var calc = new TestCalc(new());
            calc.Run($"floor({matrix})");
            Assert.Equal("[1 2 3|4 5 6|7 8 9]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixCeiling()
        {
            string matrix = "[1.4; 2.3; 3.7| 4.5; 5.4; 6.9| 7.8; 8.2; 9.1]";
            var calc = new TestCalc(new());
            calc.Run($"ceiling({matrix})");
            Assert.Equal("[2 3 4|5 6 7|8 9 10]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixTrunc()
        {
            string matrix = "[1.4; 2.3; 3.2| 4.8; 5.9; 6.12| 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run($"trunc({matrix})");
            Assert.Equal("[1 2 3|4 5 6|7 8 9]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixMod()
        {
            string matrix = "[1; 2; 3| 4; 5; 6| 7; 8; 9]";
            string n = "7";
            var calc = new TestCalc(new());
            calc.Run($"mod({matrix}; {n})");
            Assert.Equal("[1 2 3|4 5 6|0 1 2]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixGcd()
        {
            string matrix = "[1; 2; 3| 4; 5; 6| 7; 8; 9]";
            string n = "6";
            var calc = new TestCalc(new());
            calc.Run($"gcd({matrix}; {n})");
            Assert.Equal("1", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixLcm()
        {
            string matrix = "[1; 2; 3| 4; 5; 6| 7; 8; 9]";
            string n = "12";
            var calc = new TestCalc(new());
            calc.Run($"lcm({matrix}; {n})");
            Assert.Equal("2520", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void MatrixSign()
        {
            string matrix = "[1; -2; -3| 4; 5; 6| 7; 8; 9]";
            var calc = new TestCalc(new());
            calc.Run($"sign({matrix})");
            Assert.Equal("[1 -1 -1|1 1 1|1 1 1]", calc.ToString());

        }

        #endregion
    }
}
