namespace Calcpad.Tests
{
    public class RootFindingTests
    {
        private const double Tol = 1e-14;
        private const string P = "p(x) = x + 1.11111";
        private readonly string[] _functions =
        [
            //Sérgio Galdino
            //A family of regula falsi root-finding methods
            "f_01(x) = x^3 - 1",
            "f_02(x) = x^2*(x^2/3 + sqrt(2)*sin(x)) - sqrt(3)/18",
            "f_03(x) = 11*x^11 - 1",
            "f_04(x) = x^3 + 1",
            "f_05(x) = x^3 - 2*x - 5",
            "f_06(x) = 2*x* e^(-5) + 1 - 2*e^(-5*x)",
            "f_07(x) = 2*x* e^(-10) + 1 - 2*e^(-10*x)",
            "f_08(x) = 2*x* e^(-20) + 1 - 2*e^(-20*x)",
            "f_09(x) = (1 + (1 - 5)^2)*x^2 - (1 - 5*x)^2",
            "f_10(x) = (1 + (1 - 10)^2)*x^2 - (1 - 10*x)^2",
            "f_11(x) = (1 + (1 - 20)^2)*x^2 - (1 - 20*x)^2",
            "f_12(x) = x^2 - (1 - x)^5",
            "f_13(x) = x^2 - (1 - x)^10",
            "f_14(x) = x^2 - (1 - x)^20",
            "f_15(x) = (1 + (1 - 5)^4)*x - (1 - 5*x)^4",
            "f_16(x) = (1 + (1 - 10)^4)*x - (1 - 10*x)^4",
            "f_17(x) = (1 + (1 - 20)^4)*x - (1 - 20*x)^4",
            "f_18(x) = e^(-5*x)*(x - 1) + x^5",
            "f_19(x) = e^(-10*x)*(x - 1) + x^10",
            "f_20(x) = e^(-20*x)*(x - 1) + x^20",
            "f_21(x) = x^2 + sin(x/5) - 1/4",
            "f_22(x) = x^2 + sin(x/10) - 1/4",
            "f_23(x) = x^2 + sin(x/20) - 1/4",
            "f_24(x) = (x + 2)*(x + 1)*(x - 3)^3",
            "f_25(x) = (x - 4)^5*ln(x)",
            "f_26(x) = (sin(x) - x/4)^3",
            "f_27(x) = (81 - p(x)*(108 - p(x)*(54 - p(x)*(12 - p(x)))))*sign(p(x) - 3)",
            "f_28(x) = sin((x - 7.143)^3)",
            "f_29(x) = e^((x - 3)^5) - 1",
            "f_30(x) = e^((x - 3)^5) - e^(x - 1)",
            //My functions
            "f_31(x) = π - 1/x",
            "f_32(x) = 4 - tan(x)",
            //Steven A. Stage
            //Comments on An Improvement to the Brent’s Method 
            "f_33(x) = cos(x) - x^3",
            "f_34(x) = cos(x) - x",
            "f_35(x) = sqr(abs(x - 2/3))*if(x ≤ 2/3; 1; -1) - 0.1",
            "f_36(x) = abs(x - 2/3)^0.2*if(x ≤ 2/3; 1; -1)",
            "f_37(x) = (x - 7/9)^3 + (x - 7/9)*10^-3",
            "f_38(x) = if(x ≤ 1/3; -0.5; 0.5)",
            "f_39(x) = if(x ≤ 1/3; -10^-3; 1 - 10^-3)",
            "f_40(x) = if(x ≡ 0; 0; 1/(x - 2/3))",
            //A. Swift and G.R. Lindfield
            //Comparison of a Continuation Method with Brents Method for the Numerical Solution of a Single Nonlinear Equation
            "f_41(x) = 2*x* e^-5 - 2*e^(-5*x) + 1",
            "f_42(x) = (x^2 - x - 6)*(x^2 - 3*x + 2)",
            "f_43(x) = x^3",
            "f_44(x) = x^5",
            "f_45(x) = x^7",
            "f_46(x) = (e^(-5*x) - x - 0.5)/x^5",
            "f_47(x) = 1/sqrt(x) - 2*ln(5*10^3*sqrt(x)) + 0.8",
            "f_48(x) = 1/sqrt(x) - 2*ln(5*10^7*sqrt(x)) + 0.8",
            "f_49(x) = if(x ≤ 0; -x^3 - x - 1; x^(1/3) - x - 1)",
            "f_50(x) = x^3 - 2*x - x + 3",
            //Alojz Suhadolnik
            //Combined bracketing methods for solving nonlinear equations
            "f_51(x) = ln(x)",
            "f_52(x) = (10 - x)*e^(-10*x) - x^10 + 1",
            "f_53(x) = e^sin(x) - x - 1",
            "f_54(x) = 2*sin(x) - 1",
            "f_55(x) = (x - 1)*e^-x",
            "f_56(x) = (x - 1)^3 - 1",
            "f_57(x) = e^(x^2 + 7*x - 30) - 1",
            "f_58(x) = atan(x) - 1",
            "f_59(x) = e^x - 2*x - 1",
            "f_60(x) = e^-x - x - sin(x)",
            "f_61(x) = x^2 - sin(x)^2 - 1",
            "f_62(x) = sin(x) - x/2"
        ];

        private readonly string[] _commands =
        [
            "x_01 = $Root{f_01(x) @ x = 0.5 : 1.5}",
            "x_02 = $Root{f_02(x) @ x = 0.1 : 1}",
            "x_03 = $Root{f_03(x) @ x = 0.1 : 1}",
            "x_04 = $Root{f_04(x) @ x = -1.8 : 0}",
            "x_05 = $Root{f_05(x) @ x = 2 : 3}",
            "x_06 = $Root{f_06(x) @ x = 0 : 1}",
            "x_07 = $Root{f_07(x) @ x = 0 : 1}",
            "x_08 = $Root{f_08(x) @ x = 0 : 1}",
            "x_09 = $Root{f_09(x) @ x = 0 : 1}",
            "x_10 = $Root{f_10(x) @ x = 0 : 1}",
            "x_11 = $Root{f_11(x) @ x = 0 : 1}",
            "x_12 = $Root{f_12(x) @ x = 0 : 1}",
            "x_13 = $Root{f_13(x) @ x = 0 : 1}",
            "x_14 = $Root{f_14(x) @ x = 0 : 1}",
            "x_15 = $Root{f_15(x) @ x = 0 : 1}",
            "x_16 = $Root{f_16(x) @ x = 0 : 1}",
            "x_17 = $Root{f_17(x) @ x = 0 : 1}",
            "x_18 = $Root{f_18(x) @ x = 0 : 1}",
            "x_19 = $Root{f_19(x) @ x = 0 : 1}",
            "x_20 = $Root{f_20(x) @ x = 0 : 1}",
            "x_21 = $Root{f_21(x) @ x = 0 : 1}",
            "x_22 = $Root{f_22(x) @ x = 0 : 1}",
            "x_23 = $Root{f_23(x) @ x = 0 : 1}",
            "x_24 = $Root{f_24(x) @ x = 2.6 : 4.6}",
            "x_25 = $Root{f_25(x) @ x = 3.6 : 5.6}",
            "x_26 = $Root{f_26(x) @ x = 2 : 4}",
            "x_27 = $Root{f_27(x) @ x = 1 : 3}",
            "x_28 = $Root{f_28(x) @ x = 7 : 8}",
            "x_29 = $Root{f_29(x) @ x = 2.6 : 3.6}",
            "x_30 = $Root{f_30(x) @ x = 4 : 5}",
            "x_31 = $Root{f_31(x) @ x = 0.05 : 5}",
            "x_32 = $Root{f_32(x) @ x = 0 : 1.5}",
            "x_33 = $Root{f_33(x) @ x = 0 : 4}",
            "x_34 = $Root{f_34(x) @ x = -11 : 9}",
            "x_35 = $Root{f_35(x) @ x = -11 : 9}",
            "x_36 = $Find{f_36(x) @ x = -11 : 9}",
            "x_37 = $Root{f_37(x) @ x = -11 : 9}",
            "x_38 = $Find{f_38(x) @ x = -11 : 9}",
            "x_39 = $Find{f_39(x) @ x = -11 : 9}",
            "x_40 = $Find{f_40(x) @ x = -11 : 9}",
            "x_41 = $Root{f_41(x) @ x = 0 : 10}",
            "x_42 = $Root{f_42(x) @ x = 0 : 4}",
            "x_43 = $Root{f_43(x) @ x = -1 : 1.5}",
            "x_44 = $Root{f_44(x) @ x = -1 : 1.5}",
            "x_45 = $Root{f_45(x) @ x = -1 : 1.5}",
            "x_46 = $Root{f_46(x) @ x = 0.09 : 0.7}",
            "x_47 = $Root{f_47(x) @ x = 0.0005 : 0.5}",
            "x_48 = $Root{f_48(x) @ x = 0.0005 : 0.5}",
            "x_49 = $Root{f_49(x) @ x = -1 : 1}",
            "x_50 = $Root{f_50(x) @ x = -3 : 2}",
            "x_51 = $Root{f_51(x) @ x = 0.5 : 5}",
            "x_52 = $Root{f_52(x) @ x = 0.5 : 8}",
            "x_53 = $Root{f_53(x) @ x = 1.0 : 4}",
            "x_54 = $Root{f_54(x) @ x = 0.1 : π / 3}",
            "x_55 = $Root{f_55(x) @ x = 0.0 : 1.5}",
            "x_56 = $Root{f_56(x) @ x = 1.5 : 3}",
            "x_57 = $Root{f_57(x) @ x = 2.6 : 3.5}",
            "x_58 = $Root{f_58(x) @ x = 1.0 : 8}",
            "x_59 = $Root{f_59(x) @ x = 0.2 : 3}",
            "x_60 = $Root{f_60(x) @ x = 0.0 : 2}",
            "x_61 = $Root{f_61(x) @ x = -1 : 2}",
            "x_62 = $Root{f_62(x) @ x = π / 2 : π}",
        ];


        private readonly double[] _roots =
        [
            1,
            0.399422291710968,
            0.804133097503664,
            -1,
            2.094551481542326,
            0.138257155056824,
            6.93140886870235e-2,
            3.46573590208539e-2,
            0.109611796797792,
            5.24786034368102e-2,
            2.56237476199882e-2,
            0.345954815848242,
            0.245122333753307,
            0.164920957276442,
            3.61710817890406e-3,
            1.51471334783891e-4,
            7.66859512218534e-6,
            0.516153518757934,
            0.539522226908387,
            0.552704666678488,
            0.409992017989137,
            0.452509145577641,
            0.475626848596062,
            3.000001525878907,
            4.000390625,
            2.474578857421875,
            1.88916015625,
            7.14300537109375,
            3.000390625,
            4.267168304542124,
            0.318309886183791,
            1.325817663668032,
            0.865474033101614,
            0.739085133215161,
            0.656666666666667,
            0.666666666666643,
            0.777777777777778,
            0.333333333333329,
            0.333333333333329,
            0.666666666666643,
            0.138257155056824,
            2,
            3.814697265625e-6,
            9.765625e-4,
            -3.90625e-3,
            0.10162439229355,
            7.7325232006128e-3,
            1.27630494573556e-3,
            -0.682327803828019,
            -2.103803402735536,
            1,
            1.000040835564727,
            1.696812386809752,
            0.523598775598299,
            1,
            2,
            3,
            1.557407724654906,
            1.25643120862617,
            0.354463104375025,
            1.404491648215341,
            1.895494267033981,
            250,
            150,
        ];

        #region Root
        [Fact]
        public void Root_F_01()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[0]);
            var result = calc.Run(_commands[0]);
            Assert.Equal(_roots[0], result, Tol);
            result = calc.Run("f_01(x_01)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_02()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[1]);
            var result = calc.Run(_commands[1]);
            Assert.Equal(_roots[1], result, Tol);
            result = calc.Run("f_02(x_02)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_03()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[2]);
            var result = calc.Run(_commands[2]);
            Assert.Equal(_roots[2], result, Tol);
            result = calc.Run("f_03(x_03)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_04()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[3]);
            var result = calc.Run(_commands[3]);
            Assert.Equal(_roots[3], result, Tol);
            result = calc.Run("f_04(x_04)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_05()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[4]);
            var result = calc.Run(_commands[4]);
            Assert.Equal(_roots[4], result, Tol);
            result = calc.Run("f_05(x_05)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_06()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[5]);
            var result = calc.Run(_commands[5]);
            Assert.Equal(_roots[5], result, Tol);
            result = calc.Run("f_06(x_06)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_07()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[6]);
            var result = calc.Run(_commands[6]);
            Assert.Equal(_roots[6], result, Tol);
            result = calc.Run("f_07(x_07)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_08()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[7]);
            var result = calc.Run(_commands[7]);
            Assert.Equal(_roots[7], result, Tol);
            result = calc.Run("f_08(x_08)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_09()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[8]);
            var result = calc.Run(_commands[8]);
            Assert.Equal(_roots[8], result, Tol);
            result = calc.Run("f_09(x_09)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_10()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[9]);
            var result = calc.Run(_commands[9]);
            Assert.Equal(_roots[9], result, Tol);
            result = calc.Run("f_10(x_10)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_11()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[10]);
            var result = calc.Run(_commands[10]);
            Assert.Equal(_roots[10], result, Tol);
            result = calc.Run("f_11(x_11)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_12()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[11]);
            var result = calc.Run(_commands[11]);
            Assert.Equal(_roots[11], result, Tol);
            result = calc.Run("f_12(x_12)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_13()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[12]);
            var result = calc.Run(_commands[12]);
            Assert.Equal(_roots[12], result, Tol);
            result = calc.Run("f_13(x_13)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_14()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[13]);
            var result = calc.Run(_commands[13]);
            Assert.Equal(_roots[13], result, Tol);
            result = calc.Run("f_14(x_14)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_15()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[14]);
            var result = calc.Run(_commands[14]);
            Assert.Equal(_roots[14], result, Tol);
            result = calc.Run("f_15(x_15)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_16()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[15]);
            var result = calc.Run(_commands[15]);
            Assert.Equal(_roots[15], result, Tol);
            result = calc.Run("f_16(x_16)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_17()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[16]);
            var result = calc.Run(_commands[16]);
            Assert.Equal(_roots[16], result, Tol);
            result = calc.Run("f_17(x_17)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_18()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[17]);
            var result = calc.Run(_commands[17]);
            Assert.Equal(_roots[17], result, Tol);
            result = calc.Run("f_18(x_18)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_19()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[18]);
            var result = calc.Run(_commands[18]);
            Assert.Equal(_roots[18], result, Tol);
            result = calc.Run("f_19(x_19)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_20()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[19]);
            var result = calc.Run(_commands[19]);
            Assert.Equal(_roots[19], result, Tol);
            result = calc.Run("f_20(x_20)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_21()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[20]);
            var result = calc.Run(_commands[20]);
            Assert.Equal(_roots[20], result, Tol);
            result = calc.Run("f_21(x_21)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_22()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[21]);
            var result = calc.Run(_commands[21]);
            Assert.Equal(_roots[21], result, Tol);
            result = calc.Run("f_22(x_22)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_23()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[22]);
            var result = calc.Run(_commands[22]);
            Assert.Equal(_roots[22], result, Tol);
            result = calc.Run("f_23(x_23)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_24()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[23]);
            var result = calc.Run(_commands[23]);
            Assert.Equal(_roots[23], result, Tol);
            result = calc.Run("f_24(x_24)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_25()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[24]);
            var result = calc.Run(_commands[24]);
            Assert.Equal(_roots[24], result, Tol);
            result = calc.Run("f_25(x_25)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_26()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[25]);
            var result = calc.Run(_commands[25]);
            Assert.Equal(_roots[25], result, Tol);
            result = calc.Run("f_26(x_26)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_27()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(P);
            calc.Run(_functions[26]);
            var result = calc.Run(_commands[26]);
            Assert.Equal(_roots[26], result, Tol);
            result = calc.Run("f_27(x_27)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_28()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[27]);
            var result = calc.Run(_commands[27]);
            Assert.Equal(_roots[27], result, Tol);
            result = calc.Run("f_28(x_28)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_29()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[28]);
            var result = calc.Run(_commands[28]);
            Assert.Equal(_roots[28], result, Tol);
            result = calc.Run("f_29(x_29)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_30()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[29]);
            var result = calc.Run(_commands[29]);
            Assert.Equal(_roots[29], result, Tol);
            result = calc.Run("f_30(x_30)");
            Assert.Equal(0d, result, Tol * 100d);
        }

        [Fact]
        public void Root_F_31()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[30]);
            var result = calc.Run(_commands[30]);
            Assert.Equal(_roots[30], result, Tol);
            result = calc.Run("f_31(x_31)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_32()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[31]);
            var result = calc.Run(_commands[31]);
            Assert.Equal(_roots[31], result, Tol);
            result = calc.Run("f_32(x_32)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_33()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[32]);
            var result = calc.Run(_commands[32]);
            Assert.Equal(_roots[32], result, Tol);
            result = calc.Run("f_33(x_33)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_34()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[33]);
            var result = calc.Run(_commands[33]);
            Assert.Equal(_roots[33], result, Tol);
            result = calc.Run("f_34(x_34)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_35()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[34]);
            var result = calc.Run(_commands[34]);
            Assert.Equal(_roots[34], result, Tol);
            result = calc.Run("f_35(x_35)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_36()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[35]);
            var result = calc.Run(_commands[35]);
            Assert.Equal(_roots[35], result, Tol);
            //Find
        }

        [Fact]
        public void Root_F_37()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[36]);
            var result = calc.Run(_commands[36]);
            Assert.Equal(_roots[36], result, Tol);
            result = calc.Run("f_37(x_37)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_38()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[37]);
            var result = calc.Run(_commands[37]);
            Assert.Equal(_roots[37], result, Tol);
            //Find
        }

        [Fact]
        public void Root_F_39()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[38]);
            var result = calc.Run(_commands[38]);
            Assert.Equal(_roots[38], result, Tol);
            //Find
        }

        [Fact]
        public void Root_F_40()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[39]);
            var result = calc.Run(_commands[39]);
            Assert.Equal(_roots[39], result, Tol);
            //Find
        }

        [Fact]
        public void Root_F_41()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[40]);
            var result = calc.Run(_commands[40]);
            Assert.Equal(_roots[40], result, Tol);
            result = calc.Run("f_41(x_41)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_42()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[41]);
            var result = calc.Run(_commands[41]);
            Assert.Equal(_roots[41], result, Tol);
            result = calc.Run("f_42(x_42)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_43()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[42]);
            var result = calc.Run(_commands[42]);
            Assert.Equal(_roots[42], result, Tol);
            result = calc.Run("f_43(x_43)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_44()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[43]);
            var result = calc.Run(_commands[43]);
            Assert.Equal(_roots[43], result, Tol);
            result = calc.Run("f_44(x_44)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_45()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[44]);
            var result = calc.Run(_commands[44]);
            Assert.Equal(_roots[44], result, Tol);
            result = calc.Run("f_45(x_45)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_46()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[45]);
            var result = calc.Run(_commands[45]);
            Assert.Equal(_roots[45], result, Tol);
            result = calc.Run("f_46(x_46)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_47()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[46]);
            var result = calc.Run(_commands[46]);
            Assert.Equal(_roots[46], result, Tol);
            result = calc.Run("f_47(x_47)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_48()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[47]);
            var result = calc.Run(_commands[47]);
            Assert.Equal(_roots[47], result, Tol);
            result = calc.Run("f_48(x_48)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_49()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[48]);
            var result = calc.Run(_commands[48]);
            Assert.Equal(_roots[48], result, Tol);
            result = calc.Run("f_49(x_49)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_50()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[49]);
            var result = calc.Run(_commands[49]);
            Assert.Equal(_roots[49], result, Tol);
            result = calc.Run("f_50(x_50)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_51()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[50]);
            var result = calc.Run(_commands[50]);
            Assert.Equal(_roots[50], result, Tol);
            result = calc.Run("f_51(x_51)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_52()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[51]);
            var result = calc.Run(_commands[51]);
            Assert.Equal(_roots[51], result, Tol);
            result = calc.Run("f_52(x_52)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_53()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[52]);
            var result = calc.Run(_commands[52]);
            Assert.Equal(_roots[52], result, Tol);
            result = calc.Run("f_53(x_53)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_54()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[53]);
            var result = calc.Run(_commands[53]);
            Assert.Equal(_roots[53], result, Tol);
            result = calc.Run("f_54(x_54)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_55()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[54]);
            var result = calc.Run(_commands[54]);
            Assert.Equal(_roots[54], result, Tol);
            result = calc.Run("f_55(x_55)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_56()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[55]);
            var result = calc.Run(_commands[55]);
            Assert.Equal(_roots[55], result, Tol);
            result = calc.Run("f_56(x_56)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_57()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[56]);
            var result = calc.Run(_commands[56]);
            Assert.Equal(_roots[56], result, Tol);
            result = calc.Run("f_57(x_57)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_58()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[57]);
            var result = calc.Run(_commands[57]);
            Assert.Equal(_roots[57], result, Tol);
            result = calc.Run("f_58(x_58)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_59()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[58]);
            var result = calc.Run(_commands[58]);
            Assert.Equal(_roots[58], result, Tol);
            result = calc.Run("f_59(x_59)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_60()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[59]);
            var result = calc.Run(_commands[59]);
            Assert.Equal(_roots[59], result, Tol);
            result = calc.Run("f_60(x_60)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_61()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[60]);
            var result = calc.Run(_commands[60]);
            Assert.Equal(_roots[60], result, Tol);
            result = calc.Run("f_61(x_61)");
            Assert.Equal(0d, result, Tol);
        }

        [Fact]
        public void Root_F_62()
        {
            var calc = new TestCalc(new() { Degrees = 1 });
            calc.Run(_functions[61]);
            var result = calc.Run(_commands[61]);
            Assert.Equal(_roots[61], result, Tol);
            result = calc.Run("f_62(x_62)");
            Assert.Equal(0d, result, Tol);
        }

        #endregion
    }
}