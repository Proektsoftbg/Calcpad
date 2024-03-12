namespace Calcpad.Tests
{
    public class UnitsTests
    {
        private const double Tol = 1e-14;

        #region Dimensionless
        [Fact]
        [Trait("Category", "Dimensionless")]
        public void Test_Percent() => Test("1|%", 100d);

        [Fact]
        [Trait("Category", "Dimensionless")]
        public void Test_Permile() => Test("1|‰", 1000d);
        #endregion

        #region Weight
        [Fact]
        [Trait("Category", "Weight")]
        public void Test_dag() => Test("dag|g", 10d);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_hg() => Test("hg|g", 100d);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_kg() => Test("kg|g", 1000d);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_t() => Test("t|g", 1000000d);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_kt() => Test("kt|g", 1000000000d);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_Mt() => Test("Mt|g", 1000000000000d);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_Gt() => Test("Gt|g", 1e+15);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_dg() => Test("dg|g", 1e-1);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_cg() => Test("cg|g", 1e-2);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_mg() => Test("mg|g", 1e-3);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_μg() => Test("μg|g", 1e-6);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_ng() => Test("ng|g", 1e-9);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_pg() => Test("pg|g", 1e-12);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_Da() => Test("Da|kg", 1.6605390666050505e-27);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_u() => Test("u/kg", 1.6605390666050505e-27);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_gr() => Test("gr|g", 0.06479891);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_dr() => Test("dr|g", 1.7718451953125);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_oz() => Test("oz|g", 28.349523125);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_lb() => Test("lb|g", 453.59237);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_lbm() => Test("lbm/g", 453.59237);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_lb_m() => Test("lb_m/g", 453.59237);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_kipm() => Test("kipm|kg", 453.59237);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_kip_m() => Test("kip_m|kg", 453.59237);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_klbk() => Test("klb|kg", 453.59237);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_st() => Test("st|g", 6350.29318);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_qr() => Test("qr|g", 12700.58636);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_cwt_US() => Test("cwt_US|g", 45359.237);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_cwt_UK() => Test("cwt_UK|g", 50802.34544);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_ton_US() => Test("ton_US|g", 907184.74);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_ton_UK() => Test("ton_UK|g", 1016046.9088);

        [Fact]
        [Trait("Category", "Weight")]
        public void Test_slug() => Test("slug|g", 14593.90294);
        #endregion
        #region Length
        [Fact]
        [Trait("Category", "Length")]
        public void Test_km() => Test("km|m", 1e+3);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_dm() => Test("dm|m", 1e-1);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_cm() => Test("cm|m", 1e-2);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_mm() => Test("mm|m", 1e-3);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_μm() => Test("μm|m", 1e-6);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_nm() => Test("nm|m", 1e-9);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_pm() => Test("pm|m", 1e-12);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_AU() => Test("AU|m", 149597870700d);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_ly() => Test("ly|m", 9460730472580800d);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_th() => Test("th|m", 2.54E-05);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_in() => Test("in|m", 0.0254);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_ft() => Test("ft|m", 0.3048);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_yd() => Test("yd|m", 0.9144);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_ch() => Test("ch|m", 20.1168);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_fur() => Test("fur|m", 201.168);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_mi() => Test("mi|m", 1609.344);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_ftm() => Test("ftm|m", 1.8288);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_ftm_US() => Test("ftm_US|m", 1.8288);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_ftm_UK() => Test("ftm_UK|m", 1.852);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_cable() => Test("cable|m", 182.88);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_cable_UK() => Test("cable_UK|m", 185.2);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_cable_US() => Test("cable_US|m", 219.456);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_nmi() => Test("nmi|m", 1852d);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_li() => Test("li|m", 0.201168);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_rod() => Test("rod|m", 5.0292);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_pole() => Test("pole|m", 5.0292);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_perch() => Test("perch|m", 5.0292);

        [Fact]
        [Trait("Category", "Length")]
        public void Test_lea() => Test("lea|m", 4828.032);
        #endregion

        #region Area
        [Fact]
        [Trait("Category", "Area")]
        public void Test_a() => Test("a|m^2", 100d);

        [Fact]
        [Trait("Category", "Area")]
        public void Test_daa() => Test("daa|m^2", 1000d);

        [Fact]
        [Trait("Category", "Area")]
        public void Test_ha() => Test("ha|m^2", 10000d);

        [Fact]
        [Trait("Category", "Area")]
        public void Test_rood() => Test("rood|m^2", 1011.7141056);

        [Fact]
        [Trait("Category", "Area")]
        public void Test_ac() => Test("ac|m^2", 4046.8564224);
        #endregion

        #region Volume
        [Fact]
        [Trait("Category", "Volume")]
        public void Test_L() => Test("L|dm^3", 1d);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_dL() => Test("dL|L", 0.1);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_cL() => Test("cL|L", 0.01);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_mL() => Test("mL|cm^3", 1d);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_μL() => Test("μL|mm^3", 1d);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_nL() => Test("nL|L", 1e-9);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_pL() => Test("pL|L", 1e-12);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_hL() => Test("hL|L", 100d);


        [Fact]
        [Trait("Category", "Volume")]
        public void Test_fl_dr_UK() => Test("fl_dr_UK|L", 3.5516328125e-3);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_fl_oz_UK() => Test("fl_oz_UK|L", 0.0284130625);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_gi_UK() => Test("gi_UK|L", 0.1420653125);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_pt_UK() => Test("pt_UK|L", 0.56826125);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_qt_UK() => Test("qt_UK|L", 1.1365225);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_gal_UK() => Test("gal_UK|L", 4.54609);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_bbl_UK() => Test("bbl_UK|L", 163.65924);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_pk_UK() => Test("pk_UK|L", 9.09218);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_bu_UK() => Test("bu_UK|L", 36.36872);


        [Fact]
        [Trait("Category", "Volume")]
        public void Test_fl_dr_US() => Test("fl_dr_US|L", 3.6966911953125e-3);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_fl_oz_US() => Test("fl_oz_US|L", 0.0295735295625);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_gi_US() => Test("gi_US|L", 0.11829411825);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_pt_US() => Test("pt_US|L", 0.473176473);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_qt_US() => Test("qt_US|L", 0.946352946);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_gal_US() => Test("gal_US|L", 3.785411784);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_bbl_US() => Test("bbl_US|L", 119.240471196);


        [Fact]
        [Trait("Category", "Volume")]
        public void Test_pt_dry() => Test("pt_dry|L", 0.5506104713575);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_qt_dry() => Test("qt_dry|L", 1.101220942715);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_gal_dry() => Test("gal_dry|L", 4.40488377086);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_bbl_dry() => Test("bbl_dry|L", 115.628198985075);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_pk_US() => Test("pk_US|L", 8.80976754172);

        [Fact]
        [Trait("Category", "Volume")]
        public void Test_bu_US() => Test("bu_US|L", 35.23907016688);
        #endregion

        #region Time
        [Fact]
        [Trait("Category", "Time")]
        public void Test_ms() => Test("ms|s", 1e-3);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_μs() => Test("μs|s", 1e-6);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_ns() => Test("ns|s", 1e-9);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_ps() => Test("ps|s", 1e-12);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_min() => Test("min|s", 60d);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_h() => Test("h|min", 60d);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_d() => Test("d|h", 24d);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_w() => Test("w|h", 7d * 24d);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_y() => Test("y|h", 365d * 24d);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_kmh() => Test("kmh|km/h", 1d);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_mph() => Test("mph|mi/h", 1d);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_knot() => Test("knot|nmi/h", 1d);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_Hz() => Test("Hz|s^-1", 1d);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_kHz() => Test("kHz|Hz", 1e+3);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_MHz() => Test("MHz|Hz", 1e+6);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_GHz() => Test("GHz|Hz", 1e+9);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_THz() => Test("THz|Hz", 1e+12);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_mHz() => Test("mHz|Hz", 1e-3);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_μHz() => Test("μHz|Hz", 1e-6);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_nHz() => Test("nHz|Hz", 1e-9);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_pHz() => Test("pHz|Hz", 1e-12);

        [Fact]
        [Trait("Category", "Time")]
        public void Test_rpm() => Test("rpm|Hz", 1d / 60d);
        #endregion

        #region Electric Current
        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_kA() => Test("kA|A", 1e+3);

        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_MA() => Test("MA|A", 1e+6);

        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_GA() => Test("GA|A", 1e+9);

        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_TA() => Test("TA|A", 1e+12);

        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_mA() => Test("mA|A", 1e-3);

        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_μA() => Test("μA|A", 1e-6);

        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_nA() => Test("nA|A", 1e-9);

        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_pA() => Test("pA|A", 1e-12);

        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_Ah() => Test("Ah|A*h", 1d);

        [Fact]
        [Trait("Category", "Electric Current")]
        public void Test_mAh() => Test("mAh|Ah", 1e-3);
        #endregion

        #region Temperature
        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_K_C() => Test("K|°C", 1 - 273.15);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_F_C() => Test("°F|°C", (1 - 32) * 5d / 9d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_R_C() => Test("°R|°C", 5d / 9d - 273.15);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔC_C() => Test("Δ°C|°C", 1d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔF_C() => Test("Δ°F|°C", 5d / 9d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_C_K() => Test("°C|K", 1 + 273.15);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_F_K() => Test("°F|K", (1 + 459.67) * 5d / 9d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_R_K() => Test("°R|K", 5d / 9d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔC_K() => Test("Δ°C|K", 1d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔF_K() => Test("Δ°F|K", 5d / 9d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_C_F() => Test("°C|°F", 9d / 5d + 32d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_K_F() => Test("K|°F", 9d / 5d - 459.67);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_R_F() => Test("°R|°F", 1 - 459.67);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔC_F() => Test("Δ°C|°F", 9d / 5d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔF_F() => Test("Δ°F|°F", 1d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_C_R() => Test("°C|°R", 9d / 5d + 491.67);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_K_R() => Test("K|°R", 9d / 5d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_F_R() => Test("°F|°R", 1 + 459.67);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔC_R() => Test("Δ°C|°R", 9d / 5d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔF_R() => Test("Δ°F|°R", 1d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_C_ΔC() => Test("°C|Δ°C", 1d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_K_ΔC() => Test("K|Δ°C", 1d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_F_ΔC() => Test("°F|Δ°C", 5d / 9d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_R_ΔC() => Test("°R|Δ°C", 5d / 9d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔF_ΔC() => Test("Δ°F|Δ°C", 5d / 9d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_C_ΔF() => Test("°C|Δ°F", 9d / 5d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_K_ΔF() => Test("K|Δ°F", 9d / 5d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_F_ΔF() => Test("°F|Δ°F", 1d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_R_ΔF() => Test("°R|Δ°F", 1d);

        [Fact]
        [Trait("Category", "Temperature")]
        public void Test_ΔC_ΔF() => Test("Δ°C|Δ°F", 9d / 5d);
        #endregion

        #region Force
        [Fact]
        [Trait("Category", "Force")]
        public void Test_N() => Test("N|kg*m/s^2", 1d);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_daN() => Test("daN|N", 1e+1);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_hN() => Test("hN|N", 1e+2);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_kN() => Test("kN|N", 1e+3);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_MN() => Test("MN|N", 1e+6);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_GN() => Test("GN|N", 1e+9);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_TN() => Test("TN|N", 1e+12);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_Nm() => Test("Nm|N*m", 1d);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_kNm() => Test("kNm|kN*m", 1d);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_gf() => Test("gf|N", 0.00980665);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_kgf() => Test("kgf|N", 9.80665);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_tf() => Test("tf|N", 9806.65);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_dyn() => Test("dyn|N", 1e-5);


        [Fact]
        [Trait("Category", "Force")]
        public void Test_ozf() => Test("ozf|N", 0.278013851);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_oz_f() => Test("oz_f|N", 0.278013851);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_lbf() => Test("lbf|N", 4.4482216153);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_lb_f() => Test("lb_f|N", 4.4482216153);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_kip() => Test("kip|N", 4448.2216153);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_kipf() => Test("kipf|N", 4448.2216153);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_kip_f() => Test("kip_f|N", 4448.2216153);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_tonf_US() => Test("tonf_US|N", 8896.443230521);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_tonf_UK() => Test("tonf_UK|N", 9964.01641818352);

        [Fact]
        [Trait("Category", "Force")]
        public void Test_pdl() => Test("pdl|N", 0.138254954376);
        #endregion

        #region Pressure
        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_Pa() => Test("Pa|N/m^2", 1d);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_daPa() => Test("daPa|Pa", 1e+1);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_hPa() => Test("hPa|Pa", 1e+2);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_kPa() => Test("kPa|Pa", 1e+3);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_MPa() => Test("MPa|Pa", 1e+6);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_GPa() => Test("GPa|Pa", 1e+9);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_TPa() => Test("TPa|Pa", 1e+12);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_dPa() => Test("dPa|Pa", 1e-1);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_cPa() => Test("cPa|Pa", 1e-2);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_mPa() => Test("mPa|Pa", 1e-3);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_μPa() => Test("μPa|Pa", 1e-6);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_nPa() => Test("nPa|Pa", 1e-9);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_pPa() => Test("pPa|Pa", 1e-12);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_bar() => Test("bar|Pa", 100000d);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_mbar() => Test("mbar|Pa", 100d);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_μbar() => Test("μbar|Pa", 0.1);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_atm() => Test("atm|Pa", 101325d);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_mmHg() => Test("mmHg|Pa", 133.322387415);


        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_at() => Test("at|Pa", 98066.5);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_Torr() => Test("Torr|Pa", 133.32236842);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_osi() => Test("osi|Pa", 430.922330894662);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_osf() => Test("osf|Pa", 2.99251618676848);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_psi() => Test("psi|Pa", 6894.75729322959);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_ksi() => Test("ksi|Pa", 6894757.29322959);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_tsi() => Test("tsi|Pa", 15444256.3366971);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_psf() => Test("psf|Pa", 47.880258980761);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_ksf() => Test("ksf|Pa", 47880.258980761);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_tsf() => Test("tsf|Pa", 107251.780115952);

        [Fact]
        [Trait("Category", "Pressure")]
        public void Test_inHg() => Test("inHg|Pa", 3386.389);
        #endregion

        #region Viscosity
        [Fact]
        [Trait("Category", "Viscosity")]
        public void Test_P() => Test("P|Pa*s", 0.1);

        [Fact]
        [Trait("Category", "Viscosity")]
        public void Test_cP() => Test("cP|Pa*s", 0.001);

        [Fact]
        [Trait("Category", "Viscosity")]
        public void Test_St() => Test("St|m^2/s", 0.0001);

        [Fact]
        [Trait("Category", "Viscosity")]
        public void Test_cSt() => Test("cSt|m^2/s", 0.000001);
        #endregion

        #region Energy
        [Fact]
        [Trait("Category", "Energy")]
        public void Test_J() => Test("J|kg*m^2*s^-2", 1d);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_kJ() => Test("kJ|J", 1e+3);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_MJ() => Test("MJ|J", 1e+6);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_GJ() => Test("GJ|J", 1e+9);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_TJ() => Test("TJ|J", 1e+12);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_mJ() => Test("mJ|J", 1e-3);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_μJ() => Test("μJ|J", 1e-6);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_nJ() => Test("nJ|J", 1e-9);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_pJ() => Test("pJ|J", 1e-12);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_Wh() => Test("Wh|J", 3600d);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_kWh() => Test("kWh|J", 3600000d);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_MWh() => Test("MWh|J", 3600000000d);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_GWh() => Test("GWh|J", 3600000000000d);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_TWh() => Test("TWh|J", 3.6e+15);


        [Fact]
        [Trait("Category", "Energy")]
        public void Test_erg() => Test("erg|J", 1e-7);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_eV() => Test("eV|J", 1.6021773300241367e-19);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_keV() => Test("keV|J", 1.6021773300241367e-16);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_MeV() => Test("MeV|J", 1.6021773300241367e-13);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_GeV() => Test("GeV|J", 1.6021773300241367e-10);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_TeV() => Test("TeV|J", 1.6021773300241367e-7);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_PeV() => Test("PeV|J", 1.6021773300241367e-4);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_EeV() => Test("EeV|J", 1.6021773300241367e-1);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_BTU() => Test("BTU|J", 1055.05585262);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_therm_US() => Test("therm_US|J", 1054.804e+5);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_therm_UK() => Test("therm_UK|J", 1055.05585262e+5);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_quad() => Test("quad|J", 1055.05585262e+15);


        [Fact]
        [Trait("Category", "Energy")]
        public void Test_cal() => Test("cal|J", 4.1868);

        [Fact]
        [Trait("Category", "Energy")]
        public void Test_kcal() => Test("kcal|J", 4186.8);
        #endregion

        #region Power   
        [Fact]
        [Trait("Category", "Power")]
        public void Test_W() => Test("W|kg*m^2*s^-3", 1d);

        [Fact]
        [Trait("Category", "Power")]
        public void Test_kW() => Test("kW|W", 1e+3);

        [Fact]
        [Trait("Category", "Power")]
        public void Test_MW() => Test("MW|W", 1e+6);

        [Fact]
        [Trait("Category", "Power")]
        public void Test_GW() => Test("GW|W", 1e+9);

        [Fact]
        [Trait("Category", "Power")]
        public void Test_TW() => Test("TW|W", 1e+12);

        [Fact]
        [Trait("Category", "Power")]
        public void Test_mW() => Test("mW|W", 1e-3);

        [Fact]
        [Trait("Category", "Power")]
        public void Test_μW() => Test("μW|W", 1e-6);

        [Fact]
        [Trait("Category", "Power")]
        public void Test_nW() => Test("nW|W", 1e-9);

        [Fact]
        [Trait("Category", "Power")]
        public void Test_pW() => Test("pW|W", 1e-12);


        [Fact]
        [Trait("Category", "Power Horse")]
        public void Test_hp() => Test("hp|W", 745.69987158227022);

        [Fact]
        [Trait("Category", "Power Horse")]
        public void Test_hp_M() => Test("hp_M|W", 735.49875);

        [Fact]
        [Trait("Category", "Power Horse")]
        public void Test_ks() => Test("ks|W", 735.49875);

        [Fact]
        [Trait("Category", "Power Horse")]
        public void Test_hp_E() => Test("hp_E|W", 746);

        [Fact]
        [Trait("Category", "Power Horse")]
        public void Test_hp_S() => Test("hp_S|W", 9812.5);


        [Fact]
        [Trait("Category", "Power Electric")]
        public void Test_VA() => Test("VA|W", 1d);

        [Fact]
        [Trait("Category", "Power Electric")]
        public void Test_kVA() => Test("kVA|W", 1e+3);

        [Fact]
        [Trait("Category", "Power Electric")]
        public void Test_MVA() => Test("MVA|W", 1e+6);

        [Fact]
        [Trait("Category", "Power Electric")]
        public void Test_GVA() => Test("GVA|W", 1e+9);

        [Fact]
        [Trait("Category", "Power Electric")]
        public void Test_TVA() => Test("TVA|W", 1e+12);

        [Fact]
        [Trait("Category", "Power Electric")]
        public void Test_mVA() => Test("mVA|W", 1e-3);

        [Fact]
        [Trait("Category", "Power Electric")]
        public void Test_μVA() => Test("μVA|W", 1e-6);

        [Fact]
        [Trait("Category", "Power Electric")]
        public void Test_nVA() => Test("nVA|W", 1e-9);

        [Fact]
        [Trait("Category", "Power Electric")]
        public void Test_pVA() => Test("pVA|W", 1e-12);


        [Fact]
        [Trait("Category", "Power Reactive")]
        public void Test_VAR() => Test("VAR|W", 1d);

        [Fact]
        [Trait("Category", "Power Reactive")]
        public void Test_kVAR() => Test("kVAR|W", 1e+3);

        [Fact]
        [Trait("Category", "Power Reactive")]
        public void Test_MVAR() => Test("MVAR|W", 1e+6);

        [Fact]
        [Trait("Category", "Power Reactive")]
        public void Test_GVAR() => Test("GVAR|W", 1e+9);

        [Fact]
        [Trait("Category", "Power Reactive")]
        public void Test_TVAR() => Test("TVAR|W", 1e+12);

        [Fact]
        [Trait("Category", "Power Reactive")]
        public void Test_mVAR() => Test("mVAR|W", 1e-3);

        [Fact]
        [Trait("Category", "Power Reactive")]
        public void Test_μVAR() => Test("μVAR|W", 1e-6);

        [Fact]
        [Trait("Category", "Power Reactive")]
        public void Test_nVAR() => Test("nVAR|W", 1e-9);

        [Fact]
        [Trait("Category", "Power Reactive")]
        public void Test_pVAR() => Test("pVAR|W", 1e-12);
        #endregion

        #region Electric
        [Fact]
        [Trait("Category", "Electric Charge")]
        public void Test_C() => Test("C|s*A", 1d);

        [Fact]
        [Trait("Category", "Electric Charge")]
        public void Test_kC() => Test("kC|C", 1e+3);

        [Fact]
        [Trait("Category", "Electric Charge")]
        public void Test_MC() => Test("MC|C", 1e+6);

        [Fact]
        [Trait("Category", "Electric Charge")]
        public void Test_GC() => Test("GC|C", 1e+9);

        [Fact]
        [Trait("Category", "Electric Charge")]
        public void Test_TC() => Test("TC|C", 1e+12);

        [Fact]
        [Trait("Category", "Electric Charge")]
        public void Test_mC() => Test("mC|C", 1e-3);

        [Fact]
        [Trait("Category", "Electric Charge")]
        public void Test_μC() => Test("μC|C", 1e-6);

        [Fact]
        [Trait("Category", "Electric Charge")]
        public void Test_nC() => Test("nC|C", 1e-9);

        [Fact]
        [Trait("Category", "Electric Charge")]
        public void Test_pC() => Test("pC|C", 1e-12);

        [Fact]
        [Trait("Category", "Electric Potential")]
        public void Test_V() => Test("V|kg*m^2*s^-3*A^-1", 1d);

        [Fact]
        [Trait("Category", "Electric Potential")]
        public void Test_kV() => Test("kV|V", 1e+3);

        [Fact]
        [Trait("Category", "Electric Potential")]
        public void Test_MV() => Test("MV|V", 1e+6);

        [Fact]
        [Trait("Category", "Electric Potential")]
        public void Test_GV() => Test("GV|V", 1e+9);

        [Fact]
        [Trait("Category", "Electric Potential")]
        public void Test_TV() => Test("TV|V", 1e+12);

        [Fact]
        [Trait("Category", "Electric Potential")]
        public void Test_mV() => Test("mV|V", 1e-3);

        [Fact]
        [Trait("Category", "Electric Potential")]
        public void Test_μV() => Test("μV|V", 1e-6);

        [Fact]
        [Trait("Category", "Electric Potential")]
        public void Test_nV() => Test("nV|V", 1e-9);

        [Fact]
        [Trait("Category", "Electric Potential")]
        public void Test_pV() => Test("pV|V", 1e-12);


        [Fact]
        [Trait("Category", "Electric Capacitance")]
        public void Test_F() => Test("F|kg^-1*m^-2*s^4*A^2", 1d);

        [Fact]
        [Trait("Category", "Electric Capacitance")]
        public void Test_kF() => Test("kF|F", 1e+3);

        [Fact]
        [Trait("Category", "Electric Capacitance")]
        public void Test_MF() => Test("MF|F", 1e+6);

        [Fact]
        [Trait("Category", "Electric Capacitance")]
        public void Test_GF() => Test("GF|F", 1e+9);

        [Fact]
        [Trait("Category", "Electric Capacitance")]
        public void Test_TF() => Test("TF|F", 1e+12);

        [Fact]
        [Trait("Category", "Electric Capacitance")]
        public void Test_mF() => Test("mF|F", 1e-3);

        [Fact]
        [Trait("Category", "Electric Capacitance")]
        public void Test_μF() => Test("μF|F", 1e-6);

        [Fact]
        [Trait("Category", "Electric Capacitance")]
        public void Test_nF() => Test("nF|F", 1e-9);

        [Fact]
        [Trait("Category", "Electric Capacitance")]
        public void Test_pF() => Test("pF|F", 1e-12);


        [Fact]
        [Trait("Category", "Electric Resistance")]
        public void Test_Ω() => Test("Ω|kg*m^2*s^-3*A^-2", 1d);

        [Fact]
        [Trait("Category", "Electric Resistance")]
        public void Test_kΩ() => Test("kΩ|Ω", 1e+3);

        [Fact]
        [Trait("Category", "Electric Resistance")]
        public void Test_MΩ() => Test("MΩ|Ω", 1e+6);

        [Fact]
        [Trait("Category", "Electric Resistance")]
        public void Test_GΩ() => Test("GΩ|Ω", 1e+9);

        [Fact]
        [Trait("Category", "Electric Resistance")]
        public void Test_TΩ() => Test("TΩ|Ω", 1e+12);

        [Fact]
        [Trait("Category", "Electric Resistance")]
        public void Test_mΩ() => Test("mΩ|Ω", 1e-3);

        [Fact]
        [Trait("Category", "Electric Resistance")]
        public void Test_μΩ() => Test("μΩ|Ω", 1e-6);

        [Fact]
        [Trait("Category", "Electric Resistance")]
        public void Test_nΩ() => Test("nΩ|Ω", 1e-9);

        [Fact]
        [Trait("Category", "Electric Resistance")]
        public void Test_pΩ() => Test("pΩ|Ω", 1e-12);


        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_S() => Test("S|kg^-1*m^-2*s^3*A^2", 1d);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_kS() => Test("kS|S", 1e+3);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_MS() => Test("MS|S", 1e+6);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_GS() => Test("GS|S", 1e+9);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_TS() => Test("TS|S", 1e+12);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_mS() => Test("mS|S", 1e-3);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_μS() => Test("μS|S", 1e-6);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_nS() => Test("nS|S", 1e-9);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_pS() => Test("pS|S", 1e-12);


        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_Mho() => Test("℧|S", 1d);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_kMho() => Test("k℧|S", 1e+3);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_MMho() => Test("M℧|S", 1e+6);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_GMho() => Test("G℧|S", 1e+9);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_TMho() => Test("T℧|S", 1e+12);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_mMho() => Test("m℧|S", 1e-3);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_μMho() => Test("μ℧|S", 1e-6);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_nMho() => Test("n℧|S", 1e-9);

        [Fact]
        [Trait("Category", "Electric Conductance")]
        public void Test_pMho() => Test("p℧|S", 1e-12);
        #endregion

        #region Magnetic
        [Fact]
        [Trait("Category", "Magnetic Flux")]
        public void Test_Wb() => Test("Wb|kg*m^2*s^-2*A^-1", 1d);

        [Fact]
        [Trait("Category", "Magnetic Flux")]
        public void Test_kWb() => Test("kWb|Wb", 1e+3);

        [Fact]
        [Trait("Category", "Magnetic Flux")]
        public void Test_MWb() => Test("MWb|Wb", 1e+6);

        [Fact]
        [Trait("Category", "Magnetic Flux")]
        public void Test_GWb() => Test("GWb|Wb", 1e+9);

        [Fact]
        [Trait("Category", "Magnetic Flux")]
        public void Test_TWb() => Test("TWb|Wb", 1e+12);

        [Fact]
        [Trait("Category", "Magnetic Flux")]
        public void Test_mWb() => Test("mWb|Wb", 1e-3);

        [Fact]
        [Trait("Category", "Magnetic Flux")]
        public void Test_μWb() => Test("μWb|Wb", 1e-6);

        [Fact]
        [Trait("Category", "Magnetic Flux")]
        public void Test_nWb() => Test("nWb|Wb", 1e-9);

        [Fact]
        [Trait("Category", "Magnetic Flux")]
        public void Test_pWb() => Test("pWb|Wb", 1e-12);


        [Fact]
        [Trait("Category", "Magnetic Induction")]
        public void Test_T() => Test("T|kg*s^-2*A^-1", 1d);

        [Fact]
        [Trait("Category", "Magnetic Induction")]
        public void Test_kT() => Test("kT|T", 1e+3);

        [Fact]
        [Trait("Category", "Magnetic Induction")]
        public void Test_MT() => Test("MT|T", 1e+6);

        [Fact]
        [Trait("Category", "Magnetic Induction")]
        public void Test_GT() => Test("GT|T", 1e+9);

        [Fact]
        [Trait("Category", "Magnetic Induction")]
        public void Test_TT() => Test("TT|T", 1e+12);

        [Fact]
        [Trait("Category", "Magnetic Induction")]
        public void Test_mT() => Test("mT|T", 1e-3);

        [Fact]
        [Trait("Category", "Magnetic Induction")]
        public void Test_μT() => Test("μT|T", 1e-6);

        [Fact]
        [Trait("Category", "Magnetic Induction")]
        public void Test_nT() => Test("nT|T", 1e-9);

        [Fact]
        [Trait("Category", "Magnetic Induction")]
        public void Test_pT() => Test("pT|T", 1e-12);


        [Fact]
        [Trait("Category", "Electric Inductance")]
        public void Test_H() => Test("H|kg*m^2*s^-2*A^-2", 1d);

        [Fact]
        [Trait("Category", "Electric Inductance")]
        public void Test_kH() => Test("kH|H", 1e+3);

        [Fact]
        [Trait("Category", "Electric Inductance")]
        public void Test_MH() => Test("MH|H", 1e+6);

        [Fact]
        [Trait("Category", "Electric Inductance")]
        public void Test_GH() => Test("GH|H", 1e+9);

        [Fact]
        [Trait("Category", "Electric Inductance")]
        public void Test_TH() => Test("TH|H", 1e+12);

        [Fact]
        [Trait("Category", "Electric Inductance")]
        public void Test_mH() => Test("mH|H", 1e-3);

        [Fact]
        [Trait("Category", "Electric Inductance")]
        public void Test_μH() => Test("μH|H", 1e-6);

        [Fact]
        [Trait("Category", "Electric Inductance")]
        public void Test_nH() => Test("nH|H", 1e-9);

        [Fact]
        [Trait("Category", "Electric Inductance")]
        public void Test_pH() => Test("pH|H", 1e-12);
        #endregion

        #region Radioactivity
        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_Bq() => Test("Bq|s^-1", 1d);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_kBq() => Test("kBq|Bq", 1e+3);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_MBq() => Test("MBq|Bq", 1e+6);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_GBq() => Test("GBq|Bq", 1e+9);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_TBq() => Test("TBq|Bq", 1e+12);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_mBq() => Test("mBq|Bq", 1e-3);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_μBq() => Test("μBq|Bq", 1e-6);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_nBq() => Test("nBq|Bq", 1e-9);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_pBq() => Test("pBq|Bq", 1e-12);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_Ci() => Test("Ci|Bq", 3.7e+10);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_Rd() => Test("Rd|Bq", 1e+6);


        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_Gy() => Test("Gy|m^2*s^-2", 1d);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_kGy() => Test("kGy|Gy", 1e+3);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_MGy() => Test("MGy|Gy", 1e+6);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_GGy() => Test("GGy|Gy", 1e+9);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_TGy() => Test("TGy|Gy", 1e+12);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_mGy() => Test("mGy|Gy", 1e-3);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_μGy() => Test("μGy|Gy", 1e-6);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_nGy() => Test("nGy|Gy", 1e-9);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_pGy() => Test("pGy|Gy", 1e-12);


        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_Sv() => Test("Sv|m^2*s^-2", 1d);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_kSv() => Test("kSv|Sv", 1e+3);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_MSv() => Test("MSv|Sv", 1e+6);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_GSv() => Test("GSv|Sv", 1e+9);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_TSv() => Test("TSv|Sv", 1e+12);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_mSv() => Test("mSv|Sv", 1e-3);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_μSv() => Test("μSv|Sv", 1e-6);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_nSv() => Test("nSv|Sv", 1e-9);

        [Fact]
        [Trait("Category", "Radioactivity")]
        public void Test_pSv() => Test("pSv|Sv", 1e-12);
        #endregion

        #region Other
        [Fact]
        [Trait("Category", "Illuminance")]
        public void Test_lx() => Test("lx|lm/m^2", 1d);

        [Fact]
        [Trait("Category", "Catalytic Activity")]
        public void Test_kat() => Test("kat|mol/s", 1d);
        #endregion

        #region Angle
        [Fact]
        [Trait("Category", "Angle")]
        public void Test_deg() => Test("deg|°", 1d);

        [Fact]
        [Trait("Category", "Angle")]
        public void Test_ang_min() => Test("′|°", 1d / 60d);

        [Fact]
        [Trait("Category", "Angle")]
        public void Test_ang_sec() => Test("″|°", 1d / 3600d);

        [Fact]
        [Trait("Category", "Angle")]
        public void Test_rad() => Test("rad|°", 180d / Math.PI);

        [Fact]
        [Trait("Category", "Angle")]
        public void Test_grad() => Test("grad|°", 0.9);

        [Fact]
        [Trait("Category", "Angle")]
        public void Test_rev() => Test("rev|°", 360);
        #endregion

        #region Volume Conversion
        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_UK_to_gal_UK() => Test("bbl_UK|gal_UK", 36d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_UK_to_qt_UK() => Test("bbl_UK|qt_UK", 144d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_UK_to_pt_UK() => Test("bbl_UK|pt_UK", 288d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_UK_to_gi_UK() => Test("bbl_UK|gi_UK", 1152d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_UK_to_fl_oz_UK() => Test("bbl_UK|fl_oz_UK", 5760d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_UK_to_fl_dr_UK() => Test("bbl_UK|fl_dr_UK", 46080d);


        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_fl_oz_UK_to_fl_dr_UK() => Test("fl_oz_UK|fl_dr_UK", 8d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_gi_UK_to_fl_oz_UK() => Test("gi_UK|fl_oz_UK", 5d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_pt_UK_to_gi_UK() => Test("pt_UK|gi_UK", 4d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_qt_UK_to_pt_UK() => Test("qt_UK|pt_UK", 2d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_gal_UK_to_qt_UK() => Test("gal_UK|qt_UK", 4d);


        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_US_to_gal_US() => Test("bbl_US|gal_US", 31.5d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_US_to_qt_US() => Test("bbl_US|qt_US", 126d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_US_to_pt_US() => Test("bbl_US|pt_US", 252d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_US_to_gi_US() => Test("bbl_US|gi_US", 1008d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_US_to_fl_oz_US() => Test("bbl_US|fl_oz_US", 4032d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bbl_US_to_fl_dr_US() => Test("bbl_US|fl_dr_US", 32256d);


        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_fl_oz_US_to_fl_dr_US() => Test("fl_oz_US|fl_dr_US", 8d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_gi_US_to_fl_oz_US() => Test("gi_US|fl_oz_US", 4d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_pt_US_to_gi_US() => Test("pt_US|gi_US", 4d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_qt_US_to_pt_US() => Test("qt_US|pt_US", 2d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_gal_US_to_qt_US() => Test("gal_US|qt_US", 4d);


        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bu_US_to_pk_US() => Test("bu_US|pk_US", 4d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bu_US_to_gal_dry() => Test("bu_US|gal_dry", 8d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bu_US_to_qt_dry() => Test("bu_US|qt_dry", 32d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bu_US_to_pt_dry() => Test("bu_US|pt_dry", 64d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_gal_dry_to_bbl_dry() => Test("26.25gal_dry|bbl_dry", 1d);

        [Fact]
        [Trait("Category", "Volume Conversion")]
        public void Test_bu_UK_to_pk_UK() => Test("bu_UK|pk_UK", 4d);
        #endregion

        #region Mechanical Formulas
        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_kg_m_s2() => Test("1000kg*1m/s^2", "1kN");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_kg_m_s2_tf() => Test("1000kg*9.80665m/s^2|tf", "1tf");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_lb_in_s2() => Test("1000lb*386.0885826771654in/s^2", "1kip");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_ton_ft_s2() => Test("100ton*32.17404855643045ft/s^2", "224kip");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_MPa_cm2() => Test("10MPa*1cm^2", "1kN");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_ksi_in2() => Test("10ksi*1in^2", "10kip");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_ksf_tf() => Test("10ksf*1ft^2", "10kip");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_kN_cm() => Test("10kN*50cm", "5kNm");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_lbf_in() => Test("1000lb_f*12in", "1kip·ft");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_kN_m2() => Test("200kN/2m^2", "100kPa");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_kip_in2() => Test("200kip/2in^2", "100ksi");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_Nm_cm3() => Test("2Nm/1000cm^3", "2kPa");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_kip_ft_in3() => Test("2kip*ft/12in^3", "2ksi");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_kPa_cm() => Test("10kPa*200cm", "20kN/m");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_ksf_in() => Test("10ksf*24in", "20kip/ft");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_kpa_m() => Test("300kPa/3m", "100kN/m^3");

        [Fact]
        [Trait("Category", "Mechanical Formulas")]
        public void Test_ksi_in() => Test("300ksi/3in", "100kip/in^3");
        #endregion

        #region Electric Formulas
        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_A_Ω() => Test("4A*2Ω", "8V");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_W_Div_A() => Test("4W/2A", "2V");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_sqrt_Ω_Div_W() => Test("sqrt(2Ω*2W)", "2V");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_V_Div_Ω() => Test("4V/2Ω", "2A");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_W_Div_V() => Test("4W/2V", "2A");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_sqrt_W_Div_Ω() => Test("sqrt(8W/2Ω)", "2A");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_V_Div_A() => Test("4V/2A", "2Ω");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_V2_Div_W() => Test("4V^2/2W", "2Ω");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_W_Div_A2() => Test("4W/2A^2", "2Ω");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_A_V() => Test("4A*2V", "8W");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_V2_Div_Ω() => Test("4V^2/2Ω", "2W");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_A2_Ω() => Test("4A^2*2Ω", "8W");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_4_Div_2Ω() => Test("4/2Ω", "2S");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_4_Div_2Ω_Mho() => Test("4/2Ω|℧", "2℧");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_V_A() => Test("4V*2A", "8W");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_V_A_VAR() => Test("4V*2A|VAR", "8VAR");

        [Fact]
        [Trait("Category", "Electric Formulas")]
        public void Test_V_Ω_VA() => Test("4V*2A|VA", "8VA");
        #endregion

        #region Arithmetics
        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_cm2_m() => Test("10cm^2/m", "0.1cm");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_J_kJ() => Test("100J/kJ", "0.1");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_kNm_m() => Test("5kNm/m", "5kNm/m");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_cm_m()
        {
            var e = Assert.Throws<MathParser.MathParserException>(
                () =>
                {
                    new TestCalc(new()).Run("1|cm/m");
                }
            );
            Assert.Contains("does not evaluate to units", e.Message);
        }

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_kN_kPa() => Test("10kN/5kPa", "2m^2");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_1_min() => Test("1/min", "1min^(-1)");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Hz_min() => Test("1Hz*1min", "60");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_s_min() => Test("1/s*60s/min|min^-1", "60min^(-1)");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_ft_in() => Test("12ft^-1*in", "1");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_sqrt_min_h() => Test("1h + sqrt(1min*60h)", "2h");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Sum_Length_Metric() => Test("1m + 5dm + 30cm + 200mm", "2m");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Sum_Mass() => Test("1t + 1000kg", "2t");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Sum_Length_US1() => Test("1yd + 2ft + 11in + 1000th", "2yd");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Sum_Length_US2() => Test("fur + 5*(ch + 3rod + 25li)", "2fur");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Sum_Mass_UK() => Test("ton_UK + 10*(cwt_UK + 2*(1qr + 1st + 14lb))", "2ton_UK");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Sum_Mass_US() => Test("1ton_US + 10*(1cwt_US + 50*(1lb + 12oz + 32dr + 875gr))", "2ton_US");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_kg_m()
        {
            var e = Assert.Throws<MathParser.MathParserException>(
                () =>
                {
                    new TestCalc(new()).Run("1kg + 1m");
                }
            );
            Assert.Contains("Inconsistent units", e.Message);
        }

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_1_m()
        {
            var e = Assert.Throws<MathParser.MathParserException>(
                () =>
                {
                    new TestCalc(new()).Run("1 + m");
                }
            );
            Assert.Contains("Inconsistent units", e.Message);
        }

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Sum_Dimensionless() => Test("1 + 10% + 10‰", "1.11");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Sum_Angles() => Test("45° + π/8*rad + 25grad", "90°");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_kCal() => Test("(2kcal/cm^2)/(1000kcal/kg)", "20kg/m^2");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Subt_F() => Test("180°F - 18°F", "162°F");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Subt_F_C1() => Test("212°F - 10°C|°C", "90°C");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Subt_F_C2() => Test("180°F - 10°C|Δ°C", "90Δ°C");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Subt_C_F() => Test("30°C - 18°F", "20°C");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_Sum_C_F() => Test("30°C + 18°F", "40°C");

        [Fact]
        [Trait("Category", "Arithmetics")]
        public static void Themodynamics()
        {
            var calc = new TestCalc(new());
            calc.Run("σ = 4 * 10 ^ -8 * W * m ^ -2 * K ^ -4");
            calc.Run("I = 625W/m^2");
            calc.Run("T = (I/(4*σ))^0.25");
            var result = calc.ToString();  
            Assert.Equal("250K", result);
        }

        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_tan_rad() => Test("tan(π/4*rad)", 1);


        [Fact]
        [Trait("Category", "Arithmetics")]
        public void Test_atan_m()
        {
            var e = Assert.Throws<MathParser.MathParserException>(
                () =>
                {
                    new TestCalc(new()).Run("atan(1m)");
                }
            );
            Assert.Contains("Invalid units for function", e.Message);
        }

        [Fact]
        [Trait("Category", "Arithmetics")]
        public static void Atan_deg()
        {
            var calc = new TestCalc(new() { Degrees = 0});
            calc.Run("ReturnAngleUnits = 1");
            calc.Run("atan(1)");
            var result = calc.ToString();
            Assert.Equal("45°", result);
        }

        [Fact]
        [Trait("Category", "Arithmetics")]
        public static void Complex_Assignment()
        {
            var calc = new TestCalc(new() { IsComplex = true });
            calc.Run("z = (2 + 3i)*Ω");
            calc.Run("z");
            var result = calc.ToString();
            Assert.Equal("(2 + 3i)Ω", result);
        }
        #endregion

        #region Custom Units
        [Fact]
        [Trait("Category", "Custom Units")]
        public static void Ktf()
        {
            var calc = new TestCalc(new());
            calc.Run(".ktf = 1000tf");
            calc.Run("10ktf/100m^2");
            var result = calc.ToString();
            Assert.Equal("980.66kPa", result);
        }

        [Fact]
        [Trait("Category", "Custom Units")]
        public static void Euro_Pound()
        {
            var calc = new TestCalc(new());
            calc.Run(".€ = 1");
            calc.Run(".£ = 2€");
            calc.Run("2000€/t|£/kg");
            var result = calc.ToString();
            Assert.Equal("1£/kg", result);
        }

        [Fact]
        [Trait("Category", "Custom Units")]
        public static void Euro_Cent()
        {
            var calc = new TestCalc(new());
            calc.Run(".€ = 1");
            calc.Run(".¢ = 0.01€");
            calc.Run("10€ + 20¢");
            var result = calc.ToString();
            Assert.Equal("10.2€", result);
        }

        [Fact]
        [Trait("Category", "Custom Units")]
        public void Test_Custom_m()
        {
            var e = Assert.Throws<MathParser.MathParserException>(
                () =>
                {
                    new TestCalc(new()).Run(".m = 1");
                }
            );
            Assert.Contains("Cannot rewrite existing units: m.", e.Message);
        }
        #endregion

        private static void Test(string expression, double expected)
        {
            var result = new TestCalc(new()).Run(expression);
            Assert.True(Math.Abs(expected - result) < Tol*Math.Abs(expected));
        }

        private static void Test(string expression, string expected)
        {
            var calc = new TestCalc(new());
            calc.Run(expression);
            var result = calc.ToString();
            Assert.Equal(expected, result);
        }
    }
}