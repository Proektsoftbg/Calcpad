namespace Calcpad.Tests
{
    public class HPVectorScalarFunctionInFunctionTests
    {
        #region HPScalarFunctions
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorSin()
        {
            string vector = "hp([30; 60; 45; 90; 120; 180; 270])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(sin(a)*1000)/1000", $"f({vector})"]);
            Assert.Equal("[0.5 0.866 0.707 1 0.866 0 -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorCos()
        {
            string vector = "hp([30; 60; 45; 90; 120; 180; 270])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(cos(a)*1000)/1000", $"f({vector})"]);
            Assert.Equal("[0.866 0.5 0.707 0 -0.5 -1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorTan()
        {
            string vector = "hp([0; 15; 30; 45; 60; 75; 89])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(tan(a)*1000)/1000", $"f({vector})"]);
            Assert.Equal("[0 0.268 0.577 1 1.73 3.73 57.29]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorCot()
        {
            string vector = "hp([1; 45; 135; 30; 60; 150; 210])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cot(a)", $"f({vector})"]);
            Assert.Equal("[57.29 1 -1 1.73 0.577 -1.73 1.73]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorSec()
        {
            string vector = "hp([1; 60; 120; 180; 45; 30; 150])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(Sec(a)*100)/100", $"f({vector})"]);
            Assert.Equal("[1 2 -2 -1 1.41 1.15 -1.15]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorCsc()
        {
            string vector = "hp([1; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = csc(a)", $"f({vector})"]);
            Assert.Equal("[57.3 28.65 19.11 14.34 11.47 9.57 8.21]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorSinh()
        {
            string vector = "hp([0; 1; 2; 3; 4; 5; 6])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sinh(a)", $"f({vector})"]);
            Assert.Equal("[0 1.18 3.63 10.02 27.29 74.2 201.71]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorCosh()
        {
            string vector = "hp([0; 1; 2; 3; 4; 5; 6])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cosh(a)", $"f({vector})"]);
            Assert.Equal("[1 1.54 3.76 10.07 27.31 74.21 201.72]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorTanh()
        {
            string vector = "hp([30; 60; 45; 90; 120; 180; 270])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Tanh(a)", $"f({vector})"]);
            Assert.Equal("[1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorCoth()
        {
            string vector = "hp([30; 60; 45; 90; 120; 180; 270])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Coth(a)", $"f({vector})"]);
            Assert.Equal("[1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorSech()
        {
            string vector = "hp([0; 1; 2; 3; 4; 5; 6])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Sech(a)", $"f({vector})"]);
            Assert.Equal("[1 0.648 0.266 0.0993 0.0366 0.0135 0.00496]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorCsch()
        {
            string vector = "hp([1; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Csch(a)", $"f({vector})"]);
            Assert.Equal("[0.851 0.276 0.0998 0.0366 0.0135 0.00496 0.00182]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAsin()
        {
            string vector = "hp([0.5; 0.866; 1; -0.5; -0.866; -1; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Asin(a)", $"f({vector})"]);
            Assert.Equal("[30 60 90 -30 -60 -90 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAcos()
        {
            string vector = "hp([0.5; 0.866; 1; -0.5; -0.866; -1; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Acos(a)", $"f({vector})"]);
            Assert.Equal("[60 30 0 120 150 180 90]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAtan()
        {
            string vector = "hp([0.5; 0.866; 1; -0.5; -0.866; -1; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Atan(a)", $"f({vector})"]);
            Assert.Equal("[26.57 40.89 45 -26.57 -40.89 -45 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAcsc()
        {
            string vector = "hp([-3.5; 7.8; 4.25; π/3; 0; 10; -2.3; 6.75])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Acsc(a)", $"f({vector})"]);
            Assert.Equal("[-16.6 7.37 13.61 72.73 +∞ 5.74 -25.77 8.52]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAsec()
        {
            string vector = "hp([0.5; 0.866; 1; -0.5; -0.866; -1; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Asec(a)", $"f({vector})"]);
            Assert.Equal("[ Undefined   Undefined  0  Undefined   Undefined  180 +∞]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAcot()
        {
            string vector = "hp([0.5; 0.866; 1; -0.5; -0.866; -1; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Acot(a)", $"f({vector})"]);
            Assert.Equal("[63.43 49.11 45 -63.43 -49.11 -45 90]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAtan2()
        {
            string vector1 = "hp([-3.5; 7.8; 4.25; 0; 10; -2.3; 6.75])";
            string vector2 = "hp([-3.5; 7.8; 4.25; π/4; 0; 10; -2.3; 6.75])";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = Atan2(a; b)", $"f({vector1}; {vector2})"]);
            Assert.Equal("[-135 45 45 90 0 102.95 -18.82 90]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAsinh()
        {
            string vector = "hp([0.5; 0.866; 1; -0.5; -0.866; -1; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Asinh(a)", $"f({vector})"]);
            Assert.Equal("[0.481 0.783 0.881 -0.481 -0.783 -0.881 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAcosh()
        {
            string vector = "hp([1; 1.5; 2; 3; 4; 5; 6])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Acosh(a)", $"f({vector})"]);
            Assert.Equal("[0 0.962 1.32 1.76 2.06 2.29 2.48]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAtanh()
        {
            string vector = "hp([0.5; 0.866; 1; -0.5; -0.866; -1; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Atanh(a)", $"f({vector})"]);
            Assert.Equal("[0.549 1.32 +∞ -0.549 -1.32 -∞ 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAcsch()
        {
            string vector = "hp([0.5; 0.866; 1; -0.5; -0.866; -1; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Acsch(a)", $"f({vector})"]);
            Assert.Equal("[1.44 0.987 0.881 -1.44 -0.987 -0.881 +∞]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAsech()
        {
            string vector = "hp([0.5; 0.866; 1; -0.5; -0.866; -1; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Asech(a)", $"f({vector})"]);
            Assert.Equal("[1.32 0.549 0  Undefined   Undefined   Undefined  +∞]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAcoth()
        {
            string vector = "hp([1.5; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = Acoth(a)", $"f({vector})"]);
            Assert.Equal("[0.805 0.549 0.347 0.255 0.203 0.168 0.144]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorLn()
        {
            string vector = "hp([1.5; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = ln(a)", $"f({vector})"]);
            Assert.Equal("[0.405 0.693 1.1 1.39 1.61 1.79 1.95]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorLog()
        {
            string vector = "hp([1.5; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = log(a)", $"f({vector})"]);
            Assert.Equal("[0.176 0.301 0.477 0.602 0.699 0.778 0.845]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorLog2()
        {
            string vector = "hp([1.5; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = log_2(a)", $"f({vector})"]);
            Assert.Equal("[0.585 1 1.58 2 2.32 2.58 2.81]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorExp()
        {
            string vector = "hp([1.5; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = exp(a)", $"f({vector})"]);
            Assert.Equal("[4.48 7.39 20.09 54.6 148.41 403.43 1096.63]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorSqr()
        {
            string vector = "hp([1.5; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sqr(a)", $"f({vector})"]);
            Assert.Equal("[1.22 1.41 1.73 2 2.24 2.45 2.65]", calc.ToString());
        }
        [Fact]
        [Trait("Category",  "HPScalarFunctions")]
        public void HPVectorSqrt()
        {
            string vector = "hp([1.5; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sqrt(a)", $"f({vector})"]);
            Assert.Equal("[1.22 1.41 1.73 2 2.24 2.45 2.65]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorCbrt()
        {
            string vector = "hp([1.5; 2; 3; 4; 5; 6; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = cbrt(a)", $"f({vector})"]);
            Assert.Equal("[1.14 1.26 1.44 1.59 1.71 1.82 1.91]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorRoot()
        {
            string vector = "hp([1.5; 2; 3; 4; 5; 6; 7])";
            int x = 3;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = root(a; b)", $"f({vector}; {x})"]);
            Assert.Equal("[1.14 1.26 1.44 1.59 1.71 1.82 1.91]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorRound()
        {
            string vector = "hp([1.14; 1.26; 1.44; 1.59; 1.71; 1.82; 1.91])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = round(a)", $"f({vector})"]);
            Assert.Equal("[1 1 1 2 2 2 2]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorFloor()
        {
            string vector = "hp([1.7; 2.8; 3.4; 4.9; 5.12; 6.91; 7.25])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = floor(a)", $"f({vector})"]);
            Assert.Equal("[1 2 3 4 5 6 7]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorCeiling()
        {
            string vector = "hp([1.5; 2.1; 3.2; 4.4; 5.5; 6.9; 7.111])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = ceiling(a)", $"f({vector})"]);
            Assert.Equal("[2 3 4 5 6 7 8]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorTrunc()
        {
            string vector = "hp([3.7; -4.9; 5.0; -2.0; 0.1; -0.32; 7])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = trunc(a)", $"f({vector})"]);
            Assert.Equal("[3 -4 5 -2 0 0 7]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorMod()
        {
            string vector = "hp([10; -15; 20; -25; 30; -35; 40])";
            int n = 7;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = mod(a; b)", $"f({vector}; {n})"]);
            Assert.Equal("[3 -1 6 -4 2 0 5]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorGcd()
        {
            string vector = "hp([12; 15; 9; 18; 21; 24; 30])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = gcd(a)", $"f({vector})"]);
            Assert.Equal("3", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorLcm()
        {
            string vector = "hp([4; 5; 6; 7; 8; 9; 10])";
            int n = 12;
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = lcm(a; b)", $"f({vector}; {n})"]);
            Assert.Equal("2520", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorAbs()
        {
            string vector = "hp([3.7; -4.9; 5.0; -2.0; 0.1; -0.32; 7.5])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = abs(a)", $"f({vector})"]);
            Assert.Equal("[3.7 4.9 5 2 0.1 0.32 7.5]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorSign()
        {
            string vector = "hp([3.7; -4.9; 5.0; -2.0; 0.1; -0.32; 0])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = sign(a)", $"f({vector})"]);
            Assert.Equal("[1 -1 1 -1 1 -1 0]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorGetUnits()
        {
            string vector = "hp([1dm; -2dm; -3dm; 4dm; 5dm; 6dm; 7dm; 8dm; 9dm])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = getunits(a)", $"f({vector})"]);
            Assert.Equal("1dm", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorSetUnits()
        {
            string vector = "hp([1; -2; -3; 4; 5; 6; 7; 8; 9])";
            string unit = "cm";
            var calc = new TestCalc(new());
            calc.Run(["f(a; b) = setunits(a; b)", $"f({vector};{unit})"]);
            Assert.Equal("[1cm -2cm -3cm 4cm 5cm 6cm 7cm 8cm 9cm]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorClrUnits()
        {
            string vector = "hp([1dm; -2dm; -3dm; 4dm; 5dm; 6dm; 7dm; 8dm; 9dm])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = clrunits(a)", $"f({vector})"]);
            Assert.Equal("[1 -2 -3 4 5 6 7 8 9]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "HPScalarFunctions")]
        public void HPVectorIsHP()
        {
            string vector = "hp([1; -2; -3; 4; 5; 6; 7; 8; 9])";
            var calc = new TestCalc(new());
            calc.Run(["f(a) = isHp(a)", $"f({vector})"]);
            Assert.Equal("1", calc.ToString());

        }
        #endregion
    }
}
