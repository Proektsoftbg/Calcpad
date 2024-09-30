namespace Calcpad.Tests
{
    public class VectorScalarFunctionTests
    {
        #region ScalarFunctions
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorSin()
        {
            string vector = "[30; 60; 45; 90; 120; 180; 270]";
            var calc = new TestCalc(new());
            calc.Run($"round(Sin({vector})*1000)/1000");
            Assert.Equal("[0.5 0.866 0.707 1 0.866 0 -1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorCos()
        {
            string vector = "[30; 60; 45; 90; 120; 180; 270]";
            var calc = new TestCalc(new());
            calc.Run($"round(Cos({vector})*1000)/1000");
            Assert.Equal("[0.866 0.5 0.707 0 -0.5 -1 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorTan()
        {
            string vector = "[0; 15; 30; 45; 60; 75; 89]";
            var calc = new TestCalc(new());
            calc.Run($"round(Tan({vector})*1000)/1000");
            Assert.Equal("[0 0.268 0.577 1 1.73 3.73 57.29]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorCot()
        {
            string vector = "[1; 45; 135; 30; 60; 150; 210]";
            var calc = new TestCalc(new());
            calc.Run($"Cot({vector})");
            Assert.Equal("[57.29 1 -1 1.73 0.577 -1.73 1.73]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorSec()
        {
            string vector = "[1; 60; 120; 180; 45; 30; 150]";
            var calc = new TestCalc(new());
            calc.Run($"round(Sec({vector})*100)/100");
            Assert.Equal("[1 2 -2 -1 1.41 1.15 -1.15]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorCsc()
        {
            string vector = "[1; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"Csc({vector})");
            Assert.Equal("[57.3 28.65 19.11 14.34 11.47 9.57 8.21]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorSinh()
        {
            string vector = "[0; 1; 2; 3; 4; 5; 6]";
            var calc = new TestCalc(new());
            calc.Run($"Sinh({vector})");
            Assert.Equal("[0 1.18 3.63 10.02 27.29 74.2 201.71]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorCosh()
        {
            string vector = "[0; 1; 2; 3; 4; 5; 6]";
            var calc = new TestCalc(new());
            calc.Run($"Cosh({vector})");
            Assert.Equal("[1 1.54 3.76 10.07 27.31 74.21 201.72]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorTanh()
        {
            string vector = "[30; 60; 45; 90; 120; 180; 270]";
            var calc = new TestCalc(new());
            calc.Run($"Tanh({vector})");
            Assert.Equal("[1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorCoth()
        {
            string vector = "[30; 60; 45; 90; 120; 180; 270]";
            var calc = new TestCalc(new());
            calc.Run($"Coth({vector})");
            Assert.Equal("[1 1 1 1 1 1 1]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorSech()
        {
            string vector = "[0; 1; 2; 3; 4; 5; 6]";
            var calc = new TestCalc(new());
            calc.Run($"Sech({vector})");
            Assert.Equal("[1 0.648 0.266 0.0993 0.0366 0.0135 0.00496]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorCsch()
        {
            string vector = "[1; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"Csch({vector})");
            Assert.Equal("[0.851 0.276 0.0998 0.0366 0.0135 0.00496 0.00182]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAsin()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Asin({vector})");
            Assert.Equal("[30 60 90 -30 -60 -90 0]", calc.ToString());
        }

        [Fact]
        public void VectorAcos()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Acos({vector})");
            Assert.Equal("[60 30 0 120 150 180 90]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAtan()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Atan({vector})");
            Assert.Equal("[26.57 40.89 45 -26.57 -40.89 -45 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAcsc()
        {
            string vector = "[-3.5; 7.8; 4.25; π/3; 0; 10; -2.3; 6.75]";
            var calc = new TestCalc(new());
            calc.Run($"Acsc({vector})");
            Assert.Equal("[-16.6 7.37 13.61 72.73 +∞ 5.74 -25.77 8.52]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAsec()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Asec({vector})");
            Assert.Equal("[ Undefined   Undefined  0  Undefined   Undefined  180 +∞]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAcot()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Acot({vector})");
            Assert.Equal("[63.43 49.11 45 -63.43 -49.11 -45 90]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAtan2()
        {
            string vector = "[-3.5; 7.8; 4.25; 0; 10; -2.3; 6.75]";
            string vector2 = "[-3.5; 7.8; 4.25; π/4; 0; 10; -2.3; 6.75]";
            var calc = new TestCalc(new());
            calc.Run($"Atan2({vector}; {vector2})");
            Assert.Equal("[-135 45 45 90 0 102.95 -18.82 90]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAsinh()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Asinh({vector})");
            Assert.Equal("[0.481 0.783 0.881 -0.481 -0.783 -0.881 0]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAcosh()
        {
            string vector = "[1; 1.5; 2; 3; 4; 5; 6]";
            var calc = new TestCalc(new());
            calc.Run($"Acosh({vector})");
            Assert.Equal("[0 0.962 1.32 1.76 2.06 2.29 2.48]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAtanh()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Atanh({vector})");
            Assert.Equal("[0.549 1.32 +∞ -0.549 -1.32 -∞ 0]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAcsch()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Acsch({vector})");
            Assert.Equal("[1.44 0.987 0.881 -1.44 -0.987 -0.881 +∞]", calc.ToString());
        }

        [Fact]
        public void VectorAsech()
        {
            string vector = "[0.5; 0.866; 1; -0.5; -0.866; -1; 0]";
            var calc = new TestCalc(new());
            calc.Run($"Asech({vector})");
            Assert.Equal("[1.32 0.549 0  Undefined   Undefined   Undefined  +∞]", calc.ToString());
        }

        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAcoth()
        {
            string vector = "[1.5; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"Acoth({vector})");
            Assert.Equal("[0.805 0.549 0.347 0.255 0.203 0.168 0.144]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorLn()
        {
            string vector = "[1.5; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"ln({vector})");
            Assert.Equal("[0.405 0.693 1.1 1.39 1.61 1.79 1.95]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorLog()
        {
            string vector = "[1.5; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"log({vector})");
            Assert.Equal("[0.176 0.301 0.477 0.602 0.699 0.778 0.845]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorLog2()
        {
            string vector = "[1.5; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"log_2({vector})");
            Assert.Equal("[0.585 1 1.58 2 2.32 2.58 2.81]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorExp()
        {
            string vector = "[1.5; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"exp({vector})");
            Assert.Equal("[4.48 7.39 20.09 54.6 148.41 403.43 1096.63]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorSqr()
        {
            string vector = "[1.5; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"sqr({vector})");
            Assert.Equal("[1.22 1.41 1.73 2 2.24 2.45 2.65]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorSqrt()
        {
            string vector = "[1.5; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"sqrt({vector})");
            Assert.Equal("[1.22 1.41 1.73 2 2.24 2.45 2.65]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorCbrt()
        {
            string vector = "[1.5; 2; 3; 4; 5; 6; 7]";
            var calc = new TestCalc(new());
            calc.Run($"cbrt({vector})");
            Assert.Equal("[1.14 1.26 1.44 1.59 1.71 1.82 1.91]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorRoot()
        {
            string vector = "[1.5; 2; 3; 4; 5; 6; 7]";
            int x = 3;
            var calc = new TestCalc(new());
            calc.Run($"root({vector}; {x})");
            Assert.Equal("[1.14 1.26 1.44 1.59 1.71 1.82 1.91]", calc.ToString());
        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorRound()
        {
            string vector = "[1.14; 1.26; 1.44; 1.59; 1.71; 1.82; 1.91]";
            var calc = new TestCalc(new());
            calc.Run($"round({vector})");
            Assert.Equal("[1 1 1 2 2 2 2]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorFloor()
        {
            string vector = "[1.7; 2.8; 3.4; 4.9; 5.12; 6.91; 7.25]";
            var calc = new TestCalc(new());
            calc.Run($"floor({vector})");
            Assert.Equal("[1 2 3 4 5 6 7]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorCeiling()
        {
            string vector = "[1.5; 2.1; 3.2; 4.4; 5.5; 6.9; 7.111]";
            var calc = new TestCalc(new());
            calc.Run($"ceiling({vector})");
            Assert.Equal("[2 3 4 5 6 7 8]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorTrunc()
        {
            string vector = "[3.7; -4.9; 5.0; -2.0; 0.1; -0.32; 7]";
            var calc = new TestCalc(new());
            calc.Run($"trunc({vector})");
            Assert.Equal("[3 -4 5 -2 0 0 7]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorMod()
        {
            string vector = "[10; -15; 20; -25; 30; -35; 40]";
            int n = 7;
            var calc = new TestCalc(new());
            calc.Run($"mod({vector}; {n})");
            Assert.Equal("[3 -1 6 -4 2 0 5]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorGcd()
        {
            string vector = "[12; 15; 9; 18; 21; 24; 30]";
            var calc = new TestCalc(new());
            calc.Run($"gcd({vector}; 6)");
            Assert.Equal("3", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorLcm()
        {
            string vector = "[4; 5; 6; 7; 8; 9; 10]";
            int n = 12;
            var calc = new TestCalc(new());
            calc.Run($"lcm({vector}; {n})");
            Assert.Equal("2520", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorAbs()
        {
            string vector = "[3.7; -4.9; 5.0; -2.0; 0.1; -0.32; 7.5]";
            var calc = new TestCalc(new());
            calc.Run($"abs({vector})");
            Assert.Equal("[3.7 4.9 5 2 0.1 0.32 7.5]", calc.ToString());

        }
        [Fact]
        [Trait("Category", "ScalarFunctions")]
        public void VectorSign()
        {
            string vector = "[3.7; -4.9; 5.0; -2.0; 0.1; -0.32; 0]";
            var calc = new TestCalc(new());
            calc.Run($"sign({vector})");
            Assert.Equal("[1 -1 1 -1 1 -1 0]", calc.ToString());

        }
        
        #endregion
    }
}
