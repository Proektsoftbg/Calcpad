namespace Calcpad.Tests
{
    public class NumericalIntegrationTests
    {
        private const double Tol = 1e-14;
        #region Integral
        [Fact]
        [Trait("Category", "Integral")]
        public void Integral_1()
        {
            var result = new TestCalc(new())
                .Run("$Integral{ln(x) @ x = 0 : 1}");
            Assert.Equal(-1d, result, Tol);
        }
        [Fact]

        [Trait("Category", "Integral")]
        public void Integral_2()
        {
            var result = new TestCalc(new())
                .Run("$Integral{ln(x)/sqrt(x) @ x = 0 : 1}");
            Assert.Equal(-4d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Integral")]
        public void Integral_3()
        {
            var result = new TestCalc(new())
                .Run("$Integral{4/(1 + x^2) @ x = 0 : 1}");
            Assert.Equal(Math.PI, result, Tol);
        }

        [Fact]
        [Trait("Category", "Integral")]
        public void Integral_4()
        {
            var result = new TestCalc(new())
                .Run("$Integral{ln(x)*ln(1 - x) @ x = 0 : 1}");
            Assert.Equal(2 - Math.PI * Math.PI / 6d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Integral")]
        public void Integral_5()
        {
            var result = new TestCalc(new() { Degrees = 1 })
                .Run("$Integral{sin(x)/x @ x = 0 : 1}");
            Assert.Equal(0.946083070367183, result, Tol);
        }

        [Fact]
        [Trait("Category", "Integral")]
        public void Integral_6()
        {
            var result = new TestCalc(new())
                .Run("$Integral{x^2*ln(x)*ln(x + 1)/(x + 1) @ x = 0 : 1}");
            Assert.Equal(-0.0302622016388888, result, Tol);
        }

        [Fact]
        [Trait("Category", "Integral")]
        public void Integral_7()
        {
            var result = new TestCalc(new() { Degrees = 1 })
                .Run("$Integral{(sin(x)/x)^2 @ x = 0 : 10}");
            Assert.Equal(1.51864580413411, result, Tol);
        }

        [Fact]
        [Trait("Category", "Integral")]
        public void Integral_8()
        {
            var result = new TestCalc(new())
                .Run("$Integral{(16*x - 16)/(x^4 - 2*x^3 + 4*x - 4) @ x = 0 : 1}");
            Assert.Equal(Math.PI, result, Tol);
        }

        [Fact]
        [Trait("Category", "Integral")]
        public void Integral_9()
        {
            var result = new TestCalc(new())
                .Run("$Integral{1/sqrt(abs(x)) @ x = 0 : 1}");
            Assert.Equal(2, result, Tol);
        }

        [Fact]
        [Trait("Category", "Integral")]
        public void Integral_10()
        {
            var result = new TestCalc(new())
                .Run("$Area{1/(x*(1 + ln(x)^2)) @ x = 1 : e}");
            Assert.Equal(0.785398163397448, result, Tol);
        }
        #endregion

        #region Area
        [Fact]
        [Trait("Category", "Area")]
        public void Area_1() //19
        {
            var result = new TestCalc(new())
                .Run("$Area{x*ln(1 + x) @ x = 0 : 1}");
            Assert.Equal(0.25, result, Tol);
        }

        [Fact]
        [Trait("Category", "Area")]
        public void Area_2() //24
        {
            var result = new TestCalc(new())
                .Run("$Area{sqrt(1 - x^2) @ x = 0 : 1}");
            Assert.Equal(Math.PI / 4d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Area")]
        public void Area_3() //27
        {
            var result = new TestCalc(new() { Degrees = 1 })
                .Run("$Area{sqrt(tan(x)) @ x = 0 : π/3}");
            Assert.Equal(0.787779048098542, result, Tol);
        }

        [Fact]
        [Trait("Category", "Area")]
        public void Area_4() //32
        {
            var result = new TestCalc(new())
                .Run("$Area{1/(1 - 2*x + 2*x^2) @ x = 0 : 1}");
            Assert.Equal(Math.PI / 2d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Area")]
        public void Area_5() //70
        {
            var result = new TestCalc(new() { Degrees = 1 })
                .Run("$Area{1/(1 + tan(x)^3) @ x = 0 : π/2}");
            Assert.Equal(Math.PI / 4d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Area")]
        public void Area_6() //73
        {
            var result = new TestCalc(new() { Degrees = 1 })
                .Run("$Area{1/(1 + cos(x)^x) @ x = -π/2 : π/2}");
            Assert.Equal(Math.PI / 2d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Area")]
        public void Area_7() //151
        {
            var result = new TestCalc(new())
                .Run("$Area{x^x @ x = 0 : 1}");
            Assert.Equal(0.783430510712134, result, Tol);
        }

        [Fact]
        [Trait("Category", "Area")]
        public void Area_8() //268
        {
            var result = new TestCalc(new() { Degrees = 1 })
                .Run("$Area{sin(x)/sqrt(1 - 4*cos(x) + 4) @ x = 0 : π}");
            Assert.Equal(1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Area")]
        public void Area_9() //459
        {
            var result = new TestCalc(new())
                .Run("$Area{ln(x + sqrt(1 + x^2)) @ x = 0 : 1}");
            Assert.Equal(0.467160024646448, result, Tol);
        }

        [Fact]
        [Trait("Category", "Area")]
        public void Area_10() //516
        {
            var result = new TestCalc(new() { Degrees = 1 })
                .Run("$Area{sin(x)/sqrt(1 - 8*cos(x) + 16) @ x = 0 : π}");
            Assert.Equal(0.5, result, Tol);
        }
        #endregion
    }
}