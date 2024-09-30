namespace Calcpad.Tests
{
    public class NumericalMethodsTests
    {
        private const double Tol = 1e-12;
        #region Optimization
        [Fact]
        [Trait("Category", "Optimization")]
        public void Inf_Quad()
        {
            var calc = new TestCalc(new());
            calc.Run("f(x) = x^2 - 2*x - 1");
            var result = calc.Run("$Inf{f(x) @ x = -1 : 2}");
            Assert.Equal(-2d, result, Tol);
            result = calc.Run("x_inf");
            Assert.Equal(1d, result, 1e-7);
        }

        [Fact]
        [Trait("Category", "Optimization")]
        public void Sup_Quad()
        {
            var calc = new TestCalc(new());
            calc.Run("f(x) = x^2 - 2*x - 1");
            var result = calc.Run("$Sup{f(x) @ x = -1 : 2}");
            Assert.Equal(2d, result, Tol);
            result = calc.Run("x_sup");
            Assert.Equal(-1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Optimization")]
        public void Inf_Cos()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run("f(x) = cos(x - 1)");
            var result = calc.Run("$Inf{f(x) @ x = -1 : 2}");
            Assert.Equal(Math.Cos(-2d), result, Tol);
            result = calc.Run("x_inf");
            Assert.Equal(-1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Optimization")]
        public void Sup_Cos()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run("f(x) = cos(x - 1)");
            var result = calc.Run("$Sup{f(x) @ x = -1 : 2}");
            Assert.Equal(1d, result, Tol);
            result = calc.Run("x_sup");
            Assert.Equal(1d, result, 1e-7);
        }
        #endregion

        #region Slope  
        [Fact]
        [Trait("Category", "Slope")]
        public void Slope_Exp()
        {
            var result = new TestCalc(new())
                .Run("$Slope{e^x @ x = 101}");
            var expected = Math.Exp(101d);
            Assert.True(Math.Abs(expected - result) / Math.Abs(expected) < 1e-10);
        }

        [Fact]
        [Trait("Category", "Slope")]
        public void Slope_Sin()
        {
            var result = new TestCalc(new() { Degrees = 1 })
                .Run("sin(1009*π)");
            var expected = Math.Sin(1009d * Math.PI);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "Slope")]
        public void Slope_Cos_Square()
        {
            var result = new TestCalc(new() { Degrees = 1 })
                .Run("$Slope{cos(x^2) @ x = 1013}");
            var expected = -2d * 1013d * Math.Sin(1013d * 1013d);
            Assert.Equal(expected, result, 1e-6);
        }
        #endregion

        #region Find
        [Fact]
        [Trait("Category", "Find")]
        public void Find_If()
        {
            var result = new TestCalc(new())
                .Run("$Find{if(x < 1; -1; 1) @ x = -2 : 2}");
            Assert.Equal(1d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Find")]
        public void Find_Hyp()
        {
            var result = new TestCalc(new())
                .Run("$Find{1/(x - 1) @ x = -1 : 2}");
            Assert.Equal(1d, result, Tol);
        }
        #endregion

        #region Sum
        [Fact]
        [Trait("Category", "Sum")]
        public void Sum_Pi()
        {
            var result = new TestCalc(new())
                .Run("$Sum{1/16^k*(4/(8*k + 1) - 2/(8*k + 4) - 1/(8*k + 5) - 1/(8*k + 6)) @ k = 0 : 11}");
            Assert.Equal(Math.PI, result, Tol);
        }

        [Fact]
        [Trait("Category", "Sum")]
        public void Sum_e()
        {
            var result = new TestCalc(new())
                .Run("$Sum{1/k! @ k = 0 : 20}");
            Assert.Equal(Math.E, result, Tol);
        }

        [Fact]
        [Trait("Category", "Sum")]
        public void Sum_2()
        {
            var result = new TestCalc(new())
                .Run("$Sum{1/(2^k) @ k = 0 : 55}");
            Assert.Equal(2d, result, Tol);
        }
        #endregion

        #region Product
        [Fact]
        [Trait("Category", "Product")]
        public void Factorial()
        {
            var calc = new TestCalc(new());
            calc.Run("F(n) = $Product{k @ k = 1 : n}");
            var result = calc.Run("F(5)");
            Assert.Equal(120d, result, Tol);
        }

        [Fact]
        [Trait("Category", "Product")]
        public void Binomial()
        {
            var calc = new TestCalc(new());
            calc.Run("C(n; k) = $Product{(i + n - k)/i @ i = 1 : k}");
            var result = calc.Run("C(7; 5)");
            Assert.Equal(21d, result, Tol);
        }
        #endregion

        #region Repeat
        [Fact]
        [Trait("Category", "Repeat")]
        public void Babylonian_Method()
        {
            var calc = new TestCalc(new());
            calc.Run("x = 1");
            var result = calc
                .Run("$Repeat{x = 0.5*(x + 5/x) @ k = 1 : 6}");
            var expected = Math.Sqrt(5d);
            Assert.Equal(expected, result, Tol);
        }

        [Fact]
        [Trait("Category", "Repeat")]
        public void Golden_Ratio()
        {
            var calc = new TestCalc(new());
            calc.Run("x = 1");
            var result = calc
                .Run("$Repeat{x = 1/(1 + x) @ k = 1 : 40}");
            var expected = 2d / (1d + Math.Sqrt(5d));
            Assert.Equal(expected, result, Tol);
        }
        #endregion
    }
}