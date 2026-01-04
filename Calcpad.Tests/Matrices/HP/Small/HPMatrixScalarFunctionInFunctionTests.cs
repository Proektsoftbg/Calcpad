namespace Calcpad.Tests
{
    public class HPMatrixScalarFunctionInFunctionTests
    {
        #region HPScalarFunctions
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixSin()
        {
            string matrix = "hp([30; 45; 60| 90; 120; 135| 150; 180; 210])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(sin(a)*1000)/1000", $"f({matrix})"]);
            Assert.Equal("[0.5 0.707 0.866|1 0.866 0.707|0.5 0 -0.5]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixCos()
        {
            string matrix = "hp([0; 30; 45| 60; 90; 120| 150; 180; 210])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(cos(a)*1000)/1000", $"f({matrix})"]);
            Assert.Equal("[1 0.866 0.707|0.5 0 -0.5|-0.866 -1 -0.866]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixTan()
        {
            string matrix = "hp([0; 30; 45| 60; 120; 150| 180; 210; 360])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(tan(a)*1000)/1000", $"f({matrix})"]);
            Assert.Equal("[0 0.577 1|1.73 -1.73 -0.577|0 0.577 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixCot()
        {
            string matrix = "hp([30; 45; 60| 0; 120; 135| 150; 0; 210])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cot(a)", $"f({matrix})"]);
            Assert.Equal("[1.73 1 0.577|+∞ -0.577 -1|-1.73 +∞ 1.73]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixSec()
        {
            string matrix = "hp([0; 30; 45| 60; 120; 150| 180; 210; 360])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(sec(a)*100)/100", $"f({matrix})"]);
            Assert.Equal("[1 1.15 1.41|2 -2 -1.15|-1 -1.15 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixCsc()
        {
            string matrix = "hp([0; 30; 60| 45; 90; 120| 150; 0; 270])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = csc(a)", $"f({matrix})"]);
            Assert.Equal("[+∞ 2 1.15|1.41 1 1.15|2 +∞ -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixSinh()
        {
            string matrix = "hp([0.1; 0.3; 0.6| 1.2; -1.3; -0.7| 0.4; -0.4; 0.9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sinh(a)", $"f({matrix})"]);
            Assert.Equal("[0.1 0.305 0.637|1.51 -1.7 -0.759|0.411 -0.411 1.03]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixCosh()
        {
            string matrix = "hp([-2;-1; 0|1; 1.5; -1.5|0.5; -0.5; 2])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cosh(a)", $"f({matrix})"]);
            Assert.Equal("[3.76 1.54 1|1.54 2.35 2.35|1.13 1.13 3.76]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixTanh()
        {
            string matrix = "hp([0; 30; 45| 60; 90; 120| 150; 180; 210])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = tanh(a)", $"f({matrix})"]);
            Assert.Equal("[0 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixCoth()
        {
            string matrix = "hp([0; 30; 45| 60; 90; 120| 150; 180; 210])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = coth(a)", $"f({matrix})"]);
            Assert.Equal("[+∞ 1 1|1 1 1|1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixSech()
        {
            string matrix = "hp([0.1; 0.3; 0.6| 1.2; -1.3; -0.7| 0.4; -0.4; 0.9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sech(a)", $"f({matrix})"]);
            Assert.Equal("[0.995 0.957 0.844|0.552 0.507 0.797|0.925 0.925 0.698]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixCsch()
        {
            string matrix = "hp([0; 0.5; 1| -0.5; -1; 0.707| 0.866; -0.866; 1.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = csch(a)", $"f({matrix})"]);
            Assert.Equal("[+∞ 1.92 0.851|-1.92 -0.851 1.3|1.02 -1.02 0.47]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAsin()
        {
            string matrix = "hp([0.5; -0.5; 1| 0.866; -0.866; -1| 0.707; -0.707; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = asin(a)", $"f({matrix})"]);
            Assert.Equal("[30 -30 90|60 -60 -90|44.99 -44.99 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAcos()
        {
            string matrix = "hp([0.5; -0.5; 1| 0.866; -0.866; -1| 0.707; -0.707; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = acos(a)", $"f({matrix})"]);
            Assert.Equal("[60 120 0|30 150 180|45.01 134.99 90]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAtan()
        {
            string matrix = "hp([0.2588; 0.5; -0.2588| 0.866; -0.866; -0.5| 0.707; 0; -0.707])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = atan(a)", $"f({matrix})"]);
            Assert.Equal("[14.51 26.57 -14.51|40.89 -40.89 -26.57|35.26 0 -35.26]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAcsc()
        {
            string matrix = "hp([0.2588; 0.5; -0.2588| 0.866; -0.866; -0.5| 0.707; 0; -0.707])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = acsc(a)", $"f({matrix})"]);
            Assert.Equal("[ Undefined   Undefined   Undefined | Undefined   Undefined   Undefined | Undefined  +∞  Undefined ]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAsec()
        {
            string matrix = "hp([0.577; -0.577; 1| 0.577; 0.707; -1| 0; -1; 0.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = asec(a)", $"f({matrix})"]);
            Assert.Equal("[ Undefined   Undefined  0| Undefined   Undefined  180|+∞ 180  Undefined ]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAcot()
        {
            string matrix = "hp([0.577; -0.577; 1| 0.577; 0.707; -1| 0; -1; 0.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = acot(a)", $"f({matrix})"]);
            Assert.Equal("[60.02 -60.02 45|60.02 54.74 -45|90 -45 63.43]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAtan2()
        {
            string matrix1 = "hp([0.577; -0.577; 1| 0.577; 0.707; -1| 0; -1; 0.5])";
            string matrix2 = "hp([0.5; -0.5; 1| 0.866; -0.866; -1| 0.707; -0.707; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = atan2(a; b)", $"f({matrix1}; {matrix2})"]);
            Assert.Equal("[40.91 -139.09 45|56.33 -50.77 -135|90 -144.74 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAsinh()
        {
            string matrix = "hp([1.2; 0.5; -0.5| 1.1; -1.2; 0.8| 0.9; -0.9; 1.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = asinh(a)", $"f({matrix})"]);
            Assert.Equal("[1.02 0.481 -0.481|0.95 -1.02 0.733|0.809 -0.809 1.19]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAcosh()
        {
            string matrix = "hp([1.2; 0.5; -0.5| 1.1; -1.2; 0.8| 0.9; -0.9; 1.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = acosh(a)", $"f({matrix})"]);
            Assert.Equal("[0.622  Undefined   Undefined |0.444  Undefined   Undefined | Undefined   Undefined  0.962]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAtanh()
        {
            string matrix = "hp([1.2; 0.5; -0.5| 1.1; -1.2; 0.8| 0.9; -0.9; 1.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = atanh(a)", $"f({matrix})"]);
            Assert.Equal("[ Undefined  0.549 -0.549| Undefined   Undefined  1.1|1.47 -1.47  Undefined ]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAcsch()
        {
            string matrix = "hp([1.2; 0.5; -0.5| 1.1; -1.2; 0.8| 0.9; -0.9; 1.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = acsch(a)", $"f({matrix})"]);
            Assert.Equal("[0.758 1.44 -1.44|0.816 -0.758 1.05|0.958 -0.958 0.625]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAsech()
        {
            string matrix = "hp([0.7; 0.6; -0.6| 1.3; -1.5; 0.4| 0.2; -0.3; 1.1])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = asech(a)", $"f({matrix})"]);
            Assert.Equal("[0.896 1.1  Undefined | Undefined   Undefined  1.57|2.29  Undefined   Undefined ]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixAcoth()
        {
            string matrix = "hp([0.7; 0.6; -0.6| 1.3; -1.5; 0.4| 0.2; -0.3; 1.1])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = acoth(a)", $"f({matrix})"]);
            Assert.Equal("[ Undefined   Undefined   Undefined |1.02 -0.805  Undefined | Undefined   Undefined  1.52]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixLn()
        {
            string matrix = "hp([0.1; 0.5; 0.9| 1.5; 2.5; 5.0| 7.5; 10; 15])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = ln(a)", $"f({matrix})"]);
            Assert.Equal("[-2.3 -0.693 -0.105|0.405 0.916 1.61|2.01 2.3 2.71]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixLog()
        {
            string matrix = "hp([1; 10; 100| 2; 20; 200| 5; 50; 500])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = log(a)", $"f({matrix})"]);
            Assert.Equal("[0 1 2|0.301 1.3 2.3|0.699 1.7 2.7]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixLog2()
        {
            string matrix = "hp([1; 2; 4| 8; 16; 32| 64; 128; 256])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = log_2(a)", $"f({matrix})"]);
            Assert.Equal("[0 1 2|3 4 5|6 7 8]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixExp()
        {
            string matrix = "hp([2; 3; 4| 1; 5; 6| 2.5; 3.5; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = exp(a)", $"f({matrix})"]);
            Assert.Equal("[7.39 20.09 54.6|2.72 148.41 403.43|12.18 33.12 1096.63]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixSqr()
        {
            string matrix = "hp([1; 2; 3| 4; 5; 6| 7; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sqr(a)", $"f({matrix})"]);
            Assert.Equal("[1 1.41 1.73|2 2.24 2.45|2.65 2.83 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixSqrt()
        {
            string matrix = "hp([1; 2; 3| 4; 5; 6| 7; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sqrt(a)", $"f({matrix})"]);
            Assert.Equal("[1 1.41 1.73|2 2.24 2.45|2.65 2.83 3]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixCbrt()
        {
            string matrix = "hp([1; 2; 3| 4; 5; 6| 7; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cbrt(a)", $"f({matrix})"]);
            Assert.Equal("[1 1.26 1.44|1.59 1.71 1.82|1.91 2 2.08]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixRoot()
        {
            string matrix = "hp([1; 2; 3| 4; 5; 6| 7; 8; 9])";
            string x = "3";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = root(a; b)", $"f({matrix}; {x})"]);
            Assert.Equal("[1 1.26 1.44|1.59 1.71 1.82|1.91 2 2.08]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixRound()
        {
            string matrix = "hp([1.4; 2.5; 3.5| 4.7; 5.8; 6.11| 7.12; 8.34; 9.7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(a)", $"f({matrix})"]);
            Assert.Equal("[1 3 4|5 6 6|7 8 10]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixFloor()
        {
            string matrix = "hp([1.7; 2.8; 3.9| 4.1; 5.3; 6.4| 7.7; 8.9; 9.99])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = floor(a)", $"f({matrix})"]);
            Assert.Equal("[1 2 3|4 5 6|7 8 9]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixCeiling()
        {
            string matrix = "hp([1.4; 2.3; 3.7| 4.5; 5.4; 6.9| 7.8; 8.2; 9.1])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = ceiling(a)", $"f({matrix})"]);
            Assert.Equal("[2 3 4|5 6 7|8 9 10]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixTrunc()
        {
            string matrix = "hp([1.4; 2.3; 3.2| 4.8; 5.9; 6.12| 7; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = trunc(a)", $"f({matrix})"]);
            Assert.Equal("[1 2 3|4 5 6|7 8 9]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixMod()
        {
            string matrix = "hp([1; 2; 3| 4; 5; 6| 7; 8; 9])";
            string n = "7";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mod(a; b)", $"f({matrix}; {n})"]);
            Assert.Equal("[1 2 3|4 5 6|0 1 2]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixGcd()
        {
            string matrix = "hp([1; 2; 3| 4; 5; 6| 7; 8; 9])";
            string n = "6";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = gcd(a; b)", $"f({matrix}; {n})"]);
            Assert.Equal("1", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixLcm()
        {
            string matrix = "hp([1; 2; 3| 4; 5; 6| 7; 8; 9])";
            string n = "12";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = lcm(a; b)", $"f({matrix}; {n})"]);
            Assert.Equal("2520", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HP ScalarFunctions")]
        public void HPMatrixSign()
        {
            string matrix = "hp([1; -2; -3| 4; 5; 6| 7; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sign(a)", $"f({matrix})"]);
            Assert.Equal("[1 -1 -1|1 1 1|1 1 1]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixGetUnits()
        {
            string matrix = "hp([1dm; -2dm; -3dm| 4dm; 5dm; 6dm| 7dm; 8dm; 9dm])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = getunits(a)", $"f({matrix})"]);
            Assert.Equal("1dm", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixSetUnits()
        {
            string matrix = "hp([1; -2; -3| 4; 5; 6| 7; 8; 9])";
            string unit = "cm";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = setunits(a; b)", $"f({matrix};{unit})"]);
            Assert.Equal("[1cm -2cm -3cm|4cm 5cm 6cm|7cm 8cm 9cm]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixClrUnits()
        {
            string matrix = "hp([1dm; -2dm; -3dm| 4dm; 5dm; 6dm| 7dm; 8dm; 9dm])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = clrunits(a)", $"f({matrix})"]);
            Assert.Equal("[1 -2 -3|4 5 6|7 8 9]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPMatrixIsHP()
        {
            string matrix = "hp([1; -2; -3| 4; 5; 6| 7; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = isHp(a)", $"f({matrix})"]);
            Assert.Equal("1", calc.ToString());

        }
        #endregion

    }
}
