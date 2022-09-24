using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Calcpad.Core
{
    internal class Unit : IEquatable<Unit>
    {
        private static readonly char[] CompositeUnitChars = { '/', '*', '×', '^' };
        private struct Dimension
        {
            internal float Power;
            internal double Factor;

            internal bool Equals(Dimension other) =>
                Power == other.Power && Factor == other.Factor;
        }
        private string _text = string.Empty;
        private int _hashCode;
        private readonly Dimension[] _dims;

        private static bool _isUs;
        private static readonly string[] Names = { "g", "m", "s", "A", "°C", "mol", "cd", "°" };
        private static readonly Dictionary<string, Unit> Units;
        private static readonly Unit[] ForceUnits = new Unit[9], ForceUnits_US = new Unit[9];
        private static readonly HashSet<Unit> ElectricalUnits = new();
        internal static bool IsUs
        {
            get => _isUs;
            set
            {
                _isUs = value;
                if (value)
                {
                    Units["therm"] = Units["therm_US"];
                    Units["cwt"] = Units["cwt_US"];
                    Units["ton"] = Units["ton_US"];
                    Units["fl_oz"] = Units["fl_oz_US"];
                    Units["gi"] = Units["gi_US"];
                    Units["pt"] = Units["pt_US"];
                    Units["qt"] = Units["qt_US"];
                    Units["gal"] = Units["gal_US"];
                    Units["bbl"] = Units["bbl_US"];
                    Units["bu"] = Units["bu_US"];
                    Units["tonf"] = Units["tonf_US"];
                }
                else
                {
                    Units["therm"] = Units["therm_UK"];
                    Units["cwt"] = Units["cwt_UK"];
                    Units["ton"] = Units["ton_UK"];
                    Units["fl_oz"] = Units["fl_oz_UK"];
                    Units["gi"] = Units["gi_UK"];
                    Units["pt"] = Units["pt_UK"];
                    Units["qt"] = Units["qt_UK"];
                    Units["gal"] = Units["gal_UK"];
                    Units["bbl"] = Units["bbl_UK"];
                    Units["bu"] = Units["bu_UK"];
                    Units["tonf"] = Units["tonf_UK"];
                }
            }
        }

        internal bool IsForce => _dims.Length == 3 &&
                                 _dims[0].Power == 1f &&
                                 _dims[2].Power == -2f &&
                                 Text.Contains('s');

        internal bool IsElectrical => _dims.Length == 4 &&
                                      _dims[3].Power != 0 ||
                                      _dims.Length == 3 &&
                                      _dims[2].Power == -3 && //1,  2, -3
                                      _dims[1].Power == 2 &&
                                      _dims[0].Power == 1;

        internal bool IsTemp => _dims.Length == 5 &&
                                _dims[4].Power == 1f &&
                                _dims[0].Power == 0f &&
                                _dims[1].Power == 0f &&
                                _dims[2].Power == 0f &&
                                _dims[3].Power == 0f;

        internal bool IsAngle => _dims.Length == 0 ||
                                 _dims.Length > 6 && 
                                 _dims[0].Power == 0f && 
                                 _dims[1].Power == 0f && 
                                 _dims[2].Power == 0f && 
                                 _dims[3].Power == 0f && 
                                 _dims[4].Power == 0f && 
                                 _dims[5].Power == 0f && 
                                 _dims[6].Power == 0f &&
                                 _dims[7].Power == 1f;

        internal string Text
        {
            get
            {
                if (string.IsNullOrEmpty(_text))
                    _text = GetText(OutputWriter.OutputFormat.Text);

                return _text;
            }
            set => _text = value;
        }

        internal string Html
        {
            get
            {
                if (string.IsNullOrEmpty(_text))
                    return GetText(OutputWriter.OutputFormat.Html);

                OutputWriter writer = new HtmlWriter();
                return writer.FormatUnitsText(_text);
            }
        }

        internal string Xml
        {
            get
            {
                if (string.IsNullOrEmpty(_text))
                    return GetText(OutputWriter.OutputFormat.Xml);

                OutputWriter writer = new XmlWriter();
                return writer.FormatUnitsText(_text);
            }
        }

        public override int GetHashCode()
        {
            if (_hashCode == 0)
            {
                var hash = new HashCode();
                for (int i = 0, n = _dims.Length; i < n; ++i)
                {
                    hash.Add(_dims[i].Power);
                    hash.Add(_dims[i].Factor);
                }
                _hashCode = hash.ToHashCode();
            }
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is Unit u)
                return Equals(u);

            return false;
        }

        public bool Equals(Unit other)
        {
            if (other is null)
                return false;
            int n = _dims.Length;
            if (n != other._dims.Length)
                return false;

            for (int i = 0; i < n; ++i)
                if (!_dims[i].Equals(other._dims[i]))
                    return false;

            return true;
        }

        internal Unit(int n)
        {
            _dims = new Dimension[n];
            if (n == 0)
                return;

            _dims[0].Factor = 1000d;
            for (int i = 1; i < n; ++i)
                _dims[i].Factor = 1d;
        }

        internal Unit(string text) : this(Units[text]) { }
        internal Unit(
            string text,
            float mass,
            float length = 0f,
            float time = 0f,
            float current = 0f,
            float temp = 0f,
            float substance = 0f,
            float luminosity = 0f,
            float angle = 0f
        )
        {
            _text = text;
            int n;
            if (angle != 0f)
                n = 8;
            else if (luminosity != 0f)
                n = 7;
            else if (substance != 0f)
                n = 6;
            else if (temp != 0f)
                n = 5;
            else if (current != 0f)
                n = 4;
            else if (time != 0f)
                n = 3;
            else if (length != 0f)
                n = 2;
            else
                n = 1;

            _dims = new Dimension[n];
            _dims[0].Factor = 1000d;
            for (int i = 1; i < n; ++i)
                _dims[i].Factor = 1d;

            _dims[0].Power = mass;
            if (n > 1)
            {
                _dims[1].Power = length;
                if (n > 2)
                {
                    _dims[2].Power = time;
                    if (n > 3)
                    {
                        _dims[3].Power = current;
                        if (n > 4)
                        {
                            _dims[4].Power = temp;
                            if (n > 5)
                            {
                                _dims[5].Power = substance;
                                {
                                    if (n > 6)
                                    {
                                        _dims[6].Power = luminosity;

                                        if (n > 7)
                                            _dims[7].Power = angle;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal Unit(Unit u)
        {
            _text = u._text;
            _hashCode = u._hashCode;
            var n = u._dims.Length;
            _dims = new Dimension[n];
            Array.Copy(u._dims, _dims, n);
        }

        private static string TemperatureToDelta(string s)
        {
            return s[0] switch
            {
                '°' => "Δ°" + s[1],
                'K' => "Δ°C",
                'R' => "Δ°F",
                _ => s
            };
        }

        internal static bool Exists(string unit) => Units.ContainsKey(unit);
        public override string ToString() => _text;
        internal static string GetText(Unit u) => u is null ? "unitless" : u.Text;

        static Unit()
        {
            var kg = new Unit("kg", 1f).Scale("g", 0.001);
            var m = new Unit("m", 0f, 1f);
            var mi = m.Scale("mi", 1609.344);
            var a = m.Pow(2).Scale("a", 100d);
            var L = m.Shift(-1).Pow(3);
            L.Text = "L";
            var s = new Unit("s", 0f, 0f, 1f);
            var h = s.Scale("h", 3600d);
            var A = new Unit("A", 0f, 0f, 0f, 1f);
            var N = new Unit("N", 1f, 1f, -2f);
            var kN = N.Shift(3);
            var Nm = new Unit("Nm", 1f, 2f, -2f);
            var kNm = Nm.Shift(3);
            var Hz = new Unit("Hz", 0f, 0f, -1f);
            var Pa = new Unit("Pa", 1f, -1f, -2f);
            var kPa = Pa.Shift(3);
            var J = new Unit("J", 1f, 2f, -2f);
            var W = new Unit("W", 1f, 2f, -3f);
            var C = new Unit("C", 0f, 0f, 1f, 1f);
            var V = new Unit("V", 1f, 2f, -3f, -1f);
            var F = new Unit("F", -1f, -2f, 4f, 2f);
            var Ohm = new Unit("Ω", 1f, 2f, -3f, -2f);
            var S = new Unit("S", -1f, -2f, 3f, 2f);
            var Wb = new Unit("Wb", 1f, 2f, -2f, -1f);
            var T = new Unit("T", 1f, 0f, -2f, -1f);
            var H = new Unit("H", 1f, 2f, -2f, -2f);
            var Bq = new Unit("Bq", 0f, 0f, -1f);
            var Gy = new Unit("Gy", 0f, 2f, -2f);
            var Sv = new Unit("Sv", 0f, 2f, -2f);

            ForceUnits[0] = kN * m.Pow(-4d);
            ForceUnits[1] = kN * m.Pow(-3d);
            ForceUnits[2] = kPa;
            ForceUnits[3] = kN * m.Pow(-1d);
            ForceUnits[4] = kN;
            ForceUnits[5] = kNm;
            ForceUnits[6] = kN * m.Pow(2d);
            ForceUnits[7] = kN * m.Pow(3d);
            ForceUnits[8] = kN * m.Pow(4d);

            ForceUnits[0].Text = "kN/m^4";
            ForceUnits[1].Text = "kN/m^3";
            ForceUnits[3].Text = "kN/m";
            ForceUnits[6].Text = "kN·m^2";
            ForceUnits[7].Text = "kN·m^3";
            ForceUnits[8].Text = "kN·m^4";

            var ksi = Pa.Scale("ksi", 6894757.29322959);
            var kipf = N.Scale("kipf", 4448.2216153);

            ForceUnits_US[0] = (kipf * m.Pow(-4d)).Scale("kipf/in^4", 1 / 4.162314256E-7);
            ForceUnits_US[1] = (kipf * m.Pow(-3d)).Scale("kipf/in^3", 1 / 0.000016387064);
            ForceUnits_US[2] = ksi;
            ForceUnits_US[3] = (kipf * m.Pow(-1d)).Scale("kipf/ft", 1 / 0.3048);
            ForceUnits_US[4] = kipf;
            ForceUnits_US[5] = (kipf * m).Scale("kipf·ft", 0.3048);
            ForceUnits_US[6] = (kipf * m.Pow(2d)).Scale("kipf·ft^2", 0.09290304);
            ForceUnits_US[7] = (kipf * m.Pow(3d)).Scale("kipf·ft^3", 0.028316846592);
            ForceUnits_US[8] = (kipf * m.Pow(4d)).Scale("kipf·ft^4", 0.0086309748412416);

            ElectricalUnits.Add(S);// -1, -2,  3,  2
            ElectricalUnits.Add(F);// -1, -2,  4,  2
            ElectricalUnits.Add(C);//  0,  0,  1,  1
            ElectricalUnits.Add(T);//  1,  0, -2, -1
            ElectricalUnits.Add(Ohm);//1,  2, -3, -2
            ElectricalUnits.Add(V);//  1,  2, -3, -1
            ElectricalUnits.Add(W);//  1,  2, -3
            ElectricalUnits.Add(H);//  1,  2, -2, -2
            ElectricalUnits.Add(Wb);// 1,  2, -2, -1

            Units = new(StringComparer.Ordinal)
            {
                {string.Empty, null},
                {"g",     kg},
                {"hg",    kg.Shift(2)},
                {"kg",    kg.Shift(3)},
                {"t",     kg.Scale("t", 1000000d)},
                {"kt",    kg.Scale("kt", 1000000000d)},
                {"Mt",    kg.Scale("Mt", 1000000000000d)},
                {"Gt",    kg.Scale("Gt", 1E+15)},
                {"dg",    kg.Shift(-1)},
                {"cg",    kg.Shift(-2)},
                {"mg",    kg.Shift(-3)},
                {"μg",    kg.Shift(-6)},
                {"ng",    kg.Shift(-9)},
                {"pg",    kg.Shift(-12)},
                {"Da",    kg.Scale("Da", 1.6605390666050505e-27)},
                {"u",     kg.Scale("u", 1.6605390666050505e-27)},

                {"gr",    kg.Scale("gr", 0.06479891)},
                {"dr",    kg.Scale("dr", 1.7718451953125)},
                {"oz",    kg.Scale("oz", 28.349523125)},
                {"lb",    kg.Scale("lb", 453.59237)},
                {"kip",   kg.Scale("kip", 453592.37)},
                {"st",    kg.Scale("st", 6350.29318)},
                {"qr",    kg.Scale("qr", 12700.58636)},
                {"cwt_US",kg.Scale("cwt_US", 45359.237 )},
                {"cwt_UK",kg.Scale("cwt_UK", 50802.34544)},
                {"ton_US",kg.Scale("ton_US", 907184.74)},
                {"ton_UK",kg.Scale("ton_UK", 1016046.9088)},
                {"slug",  kg.Scale("slug", 14593.90294)},

                {"m",     m},
                {"km",    m.Shift(3)},
                {"dm",    m.Shift(-1)},
                {"cm",    m.Shift(-2)},
                {"mm",    m.Shift(-3)},
                {"μm",    m.Shift(-6)},
                {"nm",    m.Shift(-9)},
                {"pm",    m.Shift(-12)},
                {"AU",    m.Scale("AU", 149597870700d)},
                {"ly",    m.Scale("ly", 9460730472580800d)},

                {"th",    m.Scale("th", 2.54E-05)},
                {"in",    m.Scale("in", 0.0254)},
                {"ft",    m.Scale("ft", 0.3048)},
                {"yd",    m.Scale("yd", 0.9144)},
                {"ch",    m.Scale("ch", 20.1168)},
                {"fur",   m.Scale("fur", 201.168)},
                {"mi",    mi},
                {"ftm",   m.Scale("ftm", 1.852)},
                {"cable", m.Scale("cable", 185.2)},
                {"nmi",   m.Scale("nmi", 1852)},
                {"li",    m.Scale("li", 0.201168)},
                {"rod",   m.Scale("rod", 5.0292)},
                {"pole",  m.Scale("pole", 5.0292)},
                {"perch", m.Scale("perch", 5.0292)},
                {"lea",   m.Scale("lea", 4828.032)},

                {"a",  a},
                {"daa",a.Scale("daa", 10d)},
                {"ha", a.Scale("ha", 100d)},
                {"L",  L},
                {"dL", L.Scale("dL", 0.1)},
                {"cL", L.Scale("cL", 0.01)},
                {"mL", L.Scale("mL", 0.001)},
                {"hL", L.Scale("hL", 100d)},

                {"rood",m.Pow(2).Scale("rood", 1011.7141056)},
                {"ac",  m.Pow(2).Scale("ac", 4046.8564224)},
                {"fl_oz_US", L.Scale("fl_oz_US",  0.0295735295625 )},
                {"fl_oz_UK", L.Scale("fl_oz_UK", 0.0284130625)},
                {"gi_US",  L.Scale("gi_US", 0.11829411825)},
                {"gi_UK",  L.Scale("gi_UK",  0.1420653125)},
                {"pt_US",  L.Scale("pt_US", 0.473176473)},
                {"pt_UK",  L.Scale("pt_UK", 0.56826125)},
                {"qt_US",  L.Scale("qt_US", 0.946352946)},
                {"qt_UK",  L.Scale("qt_UK", 1.1365225)},
                {"gal_US", L.Scale("gal_US", 3.785411784)},
                {"gal_UK", L.Scale("gal_UK", 4.54609)},
                {"bbl_US", L.Scale("bbl_US", 119.240471196)},
                {"bbl_UK", L.Scale("bbl_UK", 163.65924)},
                {"bu_US",  L.Scale("bu_US", 35.2390704) },
                {"bu_UK",  L.Scale("bu_UK", 36.36872) },

                {"s",   s},
                {"ms",  s.Shift(-3)},
                {"μs",  s.Shift(-6)},
                {"ns",  s.Shift(-9)},
                {"ps",  s.Shift(-12)},
                {"min", s.Scale("min", 60d)},
                {"h",   h},
                {"d",   h.Scale("d", 24)},
                {"kmh", (m.Shift(3) / h).Scale("kmh", 1d)},
                {"mph", (mi / h).Scale("mph", 1d)},
                {"Hz",  Hz},
                {"kHz", Hz.Shift(3)},
                {"MHz", Hz.Shift(6)},
                {"GHz", Hz.Shift(9)},
                {"THz", Hz.Shift(12)},
                {"mHz", Hz.Shift(-3)},
                {"μHz", Hz.Shift(-6)},
                {"nHz", Hz.Shift(-9)},
                {"pHz", Hz.Shift(-12)},
                {"rpm", Hz.Scale("rpm", 1d / 60d)},

                {"A",  A},
                {"kA", A.Shift(3)},
                {"MA", A.Shift(6)},
                {"GA", A.Shift(9)},
                {"TA", A.Shift(12)},
                {"mA", A.Shift(-3)},
                {"μA", A.Shift(-6)},
                {"nA", A.Shift(-9)},
                {"pA", A.Shift(-12)},
                {"Ah", (A * h).Scale("Ah", 1.0)},
                {"mAh", (A.Shift(-3) * h).Scale("mAh", 1.0)},

                {"°C",  new Unit("°C",  0f, 0f, 0f, 0f, 1f)},
                {"Δ°C", new Unit("Δ°C", 0f, 0f, 0f, 0f, 1f)},
                {"K",   new Unit("K",   0f, 0f, 0f, 0f, 1f)},
                {"°F",  new Unit("°F",  0f, 0f, 0f, 0f, 1f).Scale("°F", 5d / 9d)},
                {"Δ°F", new Unit("Δ°F", 0f, 0f, 0f, 0f, 1f).Scale("Δ°F", 5d / 9d)},
                {"°R",  new Unit("°R",  0f, 0f, 0f, 0f, 1f).Scale("°R", 5d / 9d)},

                {"mol", new Unit("mol", 0f, 0f, 0f, 0f, 0f, 1f)},
                {"cd",  new Unit("cd",  0f, 0f, 0f, 0f, 0f, 0f, 1f)},

                {"N",   N},
                {"daN", N.Shift(1)},
                {"hN",  N.Shift(2)},
                {"kN",  N.Shift(3)},
                {"MN",  N.Shift(6)},
                {"GN",  N.Shift(9)},
                {"TN",  N.Shift(12)},
                {"Nm",  Nm},
                {"kNm", kNm},

                {"gf",   N.Scale("gf", 0.00980665)},
                {"kgf",  N.Scale("kgf", 9.80665)},
                {"tf",   N.Scale("tf", 9806.65)},
                {"dyn",  N.Scale("dyn", 1e-5)},

                {"ozf",  N.Scale("ozf", 0.278013851)},
                {"lbf",  N.Scale("lbf", 4.4482216153)},
                {"kipf", kipf},
                {"tonf_US", N.Scale("tonf_US", 8896.443230521)},
                {"tonf_UK", N.Scale("tonf_UK", 9964.01641818352)},
                {"pdl",  N.Scale("pdl", 0.138254954376)},

                {"Pa",   Pa},
                {"daPa", Pa.Shift(1)},
                {"hPa",  Pa.Shift(2)},
                {"kPa",  kPa},
                {"MPa",  Pa.Shift(6)},
                {"GPa",  Pa.Shift(9)},
                {"TPa",  Pa.Shift(12)},
                {"dPa",  Pa.Shift(-1)},
                {"cPa",  Pa.Shift(-2)},
                {"mPa",  Pa.Shift(-3)},
                {"μPa",  Pa.Shift(-6)},
                {"nPa",  Pa.Shift(-9)},
                {"pPa",  Pa.Shift(-12)},
                {"bar",  Pa.Scale("bar", 100000d)},
                {"mbar", Pa.Scale("mbar", 100d)},
                {"μbar", Pa.Scale("μbar", 0.1)},
                {"atm",  Pa.Scale("atm", 101325d)},
                {"mmHg", Pa.Scale("mmHg", 133.322387415)},

                {"at",   Pa.Scale("at", 98066.5)},
                {"Torr", Pa.Scale("Torr", 133.32236842)},
                {"osi",  Pa.Scale("osi", 430.922330894662)},
                {"osf",  Pa.Scale("osf", 2.99251618676848)},
                {"psi",  Pa.Scale("psi", 6894.75729322959)},
                {"ksi",  ksi},
                {"tsi",  Pa.Scale("tsi", 15444256.3366971)},
                {"psf",  Pa.Scale("psf", 47.880258980761)},
                {"ksf",  Pa.Scale("ksf", 47880.258980761)},
                {"tsf",  Pa.Scale("tsf", 107251.780115952)},
                {"inHg", Pa.Scale("inHg", 3386.389)},

                {"J",   J},
                {"kJ",  J.Shift(3)},
                {"MJ",  J.Shift(6)},
                {"GJ",  J.Shift(9)},
                {"TJ",  J.Shift(12)},
                {"mJ",  J.Shift(-3)},
                {"μJ",  J.Shift(-6)},
                {"nJ",  J.Shift(-9)},
                {"pJ",  J.Shift(-12)},
                {"Wh",  J.Scale("Wh", 3600d)},
                {"kWh", J.Scale("kWh", 3600000d)},
                {"MWh", J.Scale("MWh", 3600000000d)},
                {"GWh", J.Scale("GWh", 3600000000000d)},
                {"TWh", J.Scale("TWh", 3.6E+15)},

                {"erg", J.Scale("erg", 1e-7)},
                {"eV",  J.Scale("eV",  1.6021773300241367e-19)},
                {"keV", J.Scale("keV", 1.6021773300241367e-16)},
                {"MeV", J.Scale("MeV", 1.6021773300241367e-13)},
                {"GeV", J.Scale("GeV", 1.6021773300241367e-10)},
                {"TeV", J.Scale("TeV", 1.6021773300241367e-7)},
                {"PeV", J.Scale("PeV", 1.6021773300241367e-4)},
                {"EeV", J.Scale("EeV", 1.6021773300241367e-1)},
                {"BTU", J.Scale("BTU", 1055.05585262)},
                {"therm_US", J.Scale("therm_US", 1054.804e+5)},
                {"therm_UK", J.Scale("therm_UK", 1055.05585262e+5)},
                {"quad", J.Scale("quad", 1055.05585262e+15)},
                {"cal",  J.Scale("cal", 4.1868)},
                {"kcal", J.Scale("kcal", 4186.8)},

                {"W",  W},
                {"kW", W.Shift(3)},
                {"MW", W.Shift(6)},
                {"GW", W.Shift(9)},
                {"TW", W.Shift(12)},
                {"mW", W.Shift(-3)},
                {"μW", W.Shift(-6)},
                {"nW", W.Shift(-9)},
                {"pW", W.Shift(-12)},

                {"VA",  W.Scale("VA", 1)},
                {"kVA", W.Scale("kVA", 1e3)},
                {"MVA", W.Scale("MVA", 1e6)},
                {"GVA", W.Scale("GVA", 1e9)},
                {"TVA", W.Scale("TVA", 1e12)},
                {"mVA", W.Scale("mVA", 1e-3)},
                {"μVA", W.Scale("μVA", 1e-6)},
                {"nVA", W.Scale("nVA", 1e-9)},
                {"pVA", W.Scale("pVA", 1e-12)},

                {"VAR",  W.Scale("VAR", 1)},
                {"kVAR", W.Scale("kVAR", 1e3)},
                {"MVAR", W.Scale("MVAR", 1e6)},
                {"GVAR", W.Scale("GVAR", 1e9)},
                {"TVAR", W.Scale("TVAR", 1e12)},
                {"mVAR", W.Scale("mVAR", 1e-3)},
                {"μVAR", W.Scale("μVAR", 1e-6)},
                {"nVAR", W.Scale("nVAR", 1e-9)},
                {"pVAR", W.Scale("pVAR", 1e-12)},

                {"hp",   W.Scale("hp", 745.69987158227022)},
                {"hp_M", W.Scale("hp_M", 735.49875)},
                {"ks",   W.Scale("ks", 735.49875)},
                {"hp_E", W.Scale("hp_E", 746)},
                {"hp_S", W.Scale("hp_S", 9812.5)},

                {"C",  C},
                {"kC", C.Shift(3)},
                {"MC", C.Shift(6)},
                {"GC", C.Shift(9)},
                {"TC", C.Shift(12)},
                {"mC", C.Shift(-3)},
                {"μC", C.Shift(-6)},
                {"nC", C.Shift(-9)},
                {"pC", C.Shift(-12)},

                {"V",  V},
                {"kV", V.Shift(3)},
                {"MV", V.Shift(6)},
                {"GV", V.Shift(9)},
                {"TV", V.Shift(12)},
                {"mV", V.Shift(-3)},
                {"μV", V.Shift(-6)},
                {"nV", V.Shift(-9)},
                {"pV", V.Shift(-12)},

                {"F",  F},
                {"kF", F.Shift(3)},
                {"MF", F.Shift(6)},
                {"GF", F.Shift(9)},
                {"TF", F.Shift(12)},
                {"mF", F.Shift(-3)},
                {"μF", F.Shift(-6)},
                {"nF", F.Shift(-9)},
                {"pF", F.Shift(-12)},

                {"Ω",  Ohm},
                {"kΩ", Ohm.Shift(3)},
                {"MΩ", Ohm.Shift(6)},
                {"GΩ", Ohm.Shift(9)},
                {"TΩ", Ohm.Shift(12)},
                {"mΩ", Ohm.Shift(-3)},
                {"μΩ", Ohm.Shift(-6)},
                {"nΩ", Ohm.Shift(-9)},
                {"pΩ", Ohm.Shift(-12)},

                {"S",  S},
                {"kS", S.Shift(3)},
                {"MS", S.Shift(6)},
                {"GS", S.Shift(9)},
                {"TS", S.Shift(12)},
                {"mS", S.Shift(-3)},
                {"μS", S.Shift(-6)},
                {"nS", S.Shift(-9)},
                {"pS", S.Shift(-12)},

                {"℧",  S.Scale("℧",1)},
                {"k℧", S.Scale("k℧",1e3)},
                {"M℧", S.Scale("M℧",1e6)},
                {"G℧", S.Scale("G℧",1e9)},
                {"T℧", S.Scale("T℧",1e12)},
                {"m℧", S.Scale("m℧",1e-3)},
                {"μ℧", S.Scale("μ℧",1e-6)},
                {"n℧", S.Scale("n℧",1e-9)},
                {"p℧", S.Scale("p℧",1e-12)},

                {"Wb",  Wb},
                {"kWb", Wb.Shift(3)},
                {"MWb", Wb.Shift(6)},
                {"GWb", Wb.Shift(9)},
                {"TWb", Wb.Shift(12)},
                {"mWb", Wb.Shift(-3)},
                {"μWb", Wb.Shift(-6)},
                {"nWb", Wb.Shift(-9)},
                {"pWb", Wb.Shift(-12)},

                {"T",  T},
                {"kT", T.Shift(3)},
                {"MT", T.Shift(6)},
                {"GT", T.Shift(9)},
                {"TT", T.Shift(12)},
                {"mT", T.Shift(-3)},
                {"μT", T.Shift(-6)},
                {"nT", T.Shift(-9)},
                {"pT", T.Shift(-12)},

                {"H",  H},
                {"kH", H.Shift(3)},
                {"MH", H.Shift(6)},
                {"GH", H.Shift(9)},
                {"TH", H.Shift(12)},
                {"mH", H.Shift(-3)},
                {"μH", H.Shift(-6)},
                {"nH", H.Shift(-9)},
                {"pH", H.Shift(-12)},

                {"Bq",  Bq},
                {"kBq", Bq.Shift(3)},
                {"MBq", Bq.Shift(6)},
                {"GBq", Bq.Shift(9)},
                {"TBq", Bq.Shift(12)},
                {"mBq", Bq.Shift(-3)},
                {"μBq", Bq.Shift(-6)},
                {"nBq", Bq.Shift(-9)},
                {"pBq", Bq.Shift(-12)},
                {"Ci",  Bq.Scale("Ci", 3.7e+10)},
                {"Rd",  Bq.Scale("Rd", 1e+6)},

                {"Gy",  Gy},
                {"kGy", Gy.Shift(3)},
                {"MGy", Gy.Shift(6)},
                {"GGy", Gy.Shift(9)},
                {"TGy", Gy.Shift(12)},
                {"mGy", Gy.Shift(-3)},
                {"μGy", Gy.Shift(-6)},
                {"nGy", Gy.Shift(-9)},
                {"pGy", Gy.Shift(-12)},

                {"Sv",  Sv},
                {"kSv", Sv.Shift(3)},
                {"MSv", Sv.Shift(6)},
                {"GSv", Sv.Shift(9)},
                {"TSv", Sv.Shift(12)},
                {"mSv", Sv.Shift(-3)},
                {"μSv", Sv.Shift(-6)},
                {"nSv", Sv.Shift(-9)},
                {"pSv", Sv.Shift(-12)},

                {"lm",  new Unit("lm", 0, 0, 0, 0, 0, 0, 1)},
                {"lx",  new Unit("lx", 0, -2, 0, 0, 0, 0, 1)},
                {"kat", new Unit("kat", 0, 0, -1, 0, 0, 1)},

                {"°",  new Unit("°",       0, 0, 0, 0, 0, 0, 0, 1)},
                {"′",  new Unit("′",       0, 0, 0, 0, 0, 0, 0, 1)},
                {"″",  new Unit("″",       0, 0, 0, 0, 0, 0, 0, 1)},
                {"rad",  new Unit("rad",   0, 0, 0, 0, 0, 0, 0, 1)},
                {"grad",  new Unit("grad", 0, 0, 0, 0, 0, 0, 0, 1)},
                {"rev",  new Unit("rev", 0, 0, 0, 0, 0, 0, 0, 1)}
            };
            Units["°"].Scale(Math.PI / 180.0);
            Units["′"].Scale(Math.PI / 10800.0);
            Units["″"].Scale( Math.PI / 648000.0);
            Units.Add("deg", Units["°"]);
            Units["grad"].Scale(Math.PI / 200.0);
            Units["rev"].Scale(Math.PI * 2.0);

            Units.Add("therm", Units["therm_UK"]);
            Units.Add("cwt", Units["cwt_UK"]);
            Units.Add("ton", Units["ton_UK"]);
            Units.Add("fl_oz", Units["fl_oz_UK"]);
            Units.Add("gi", Units["gi_UK"]);
            Units.Add("pt", Units["pt_UK"]);
            Units.Add("qt", Units["qt_UK"]);
            Units.Add("gal", Units["gal_UK"]);
            Units.Add("bbl", Units["bbl_UK"]);
            Units.Add("bu", Units["bu_UK"]);
            Units.Add("tonf", Units["tonf_UK"]);
        }

        internal void Scale(double factor)
        {
            for (int i = 0, n = _dims.Length; i < n; ++i)
            {
                ref var dim = ref _dims[i];
                if (dim.Power != 0f)
                {
                    dim.Factor *= Math.Pow(factor, 1d / dim.Power);
                    break;
                }
            }
        }

        internal Unit Shift(int n) => Scale(GetPrefix(n) + _text, GetScale(n));
        internal Unit Scale(string s, double factor)
        {
            var unit = new Unit(this)
            {
                _text = s
            };
            unit.Scale(factor);
            return unit;
        }

        private string GetText(OutputWriter.OutputFormat format)
        {
            OutputWriter writer = format switch
            {
                OutputWriter.OutputFormat.Html => new HtmlWriter(),
                OutputWriter.OutputFormat.Xml => new XmlWriter(),
                _ => new TextWriter()
            };
            var stringBuilder = new StringBuilder();
            var isFirst = true;
            for (int i = 0, n = _dims.Length; i < n; ++i)
            {
                ref var dim = ref _dims[i];
                if (dim.Power != 0f)
                {
                    var p = isFirst ? dim.Power : Math.Abs(dim.Power);
                    var s = GetDimText(writer, Names[i], dim.Factor, p);
                    if (i == 4 && stringBuilder.Length > 0)
                        s = TemperatureToDelta(s);

                    if (isFirst)
                        isFirst = false;
                    else
                    {
                        var oper = dim.Power > 0f ? '·' : '/';
                        if (format == OutputWriter.OutputFormat.Xml)
                            stringBuilder.Append($"<m:r><m:t>{oper}</m:t></m:r>");
                        else
                            stringBuilder.Append(oper);
                    }
                    stringBuilder.Append(s);
                }
            }
            return stringBuilder.ToString();
        }

        public static Unit operator *(Unit u, double d)
        {
            var unit = new Unit(u);
            for (int i = 0, n = unit._dims.Length; i < n; ++i)
            {
                ref var dim = ref unit._dims[i];
                if (dim.Power != 0f)
                {
                    dim.Factor *= Math.Pow(d, 1d / dim.Power);
                    break;
                }
            }
            return unit;
        }

        public static Unit operator *(double d, Unit u) => u * d;

        public static Unit operator *(Unit u1, Unit u2) => MultiplyOrDivide(u1, u2);

        public static Unit operator /(Unit u1, Unit u2) => MultiplyOrDivide(u1, u2, true);

        private static Unit MultiplyOrDivide(Unit u1, Unit u2, bool divide = false)
        {
            var n1 = u1._dims.Length;
            var n2 = u2._dims.Length;
            var n = n1 > n2 ? n1 : n2;
            float p1, p2;
            if (n1 == n2)
            {
                while (n > 0)
                {
                    var i = n - 1;
                    p1 = u1._dims[i].Power;
                    p2 = u2._dims[i].Power;
                    if (p1 != (divide ? p2 : -p2))
                        break;

                    n = i;
                }
                if (n == 0)
                    return null;
            }


            Unit unit = new(n);
            for (int i = 0; i < n; ++i)
            {
                p1 = i < n1 ? u1._dims[i].Power : 0f;
                p2 = i < n2 ? u2._dims[i].Power : 0f;
                ref var dim = ref unit._dims[i];
                dim.Factor =
                    p1 != 0f ?
                    u1._dims[i].Factor :
                    p2 != 0f ?
                    u2._dims[i].Factor :
                    1f;
                dim.Power = divide ? p1 - p2 : p1 + p2;
            }
            return unit;
        }

        internal static double GetProductOrDivisionFactor(Unit u1, Unit u2, bool divide = false)
        {
            var n1 = u1._dims.Length;
            var n2 = u2._dims.Length;
            var n = n1 < n2 ? n1 : n2;
            var factor = 1d;
            for (int i = 0; i < n; ++i)
            {
                var p2 = u2._dims[i].Power;
                if (u1._dims[i].Power != 0f && p2 != 0f && u1._dims[i].Factor != u2._dims[i].Factor)
                {
                    var d = divide ?
                        u1._dims[i].Factor / u2._dims[i].Factor :
                        u2._dims[i].Factor / u1._dims[i].Factor;

                    factor *= MyPow(d, p2);
                }
            }
            return factor;
        }

        private static double MyPow(double x, float y) => y switch
        {
            1f => x,
            2f => x * x,
            3f => x * x * x,
            _ => Math.Pow(x, y)
        };

        public static Unit operator /(Unit u, double d) => u * (1d / d);

        public static Unit operator /(double d, Unit u) => d * u.Pow(-1d);

        internal Unit Pow(double x)
        {
            var f = (float)x;
            var n = _dims.Length;
            Unit unit = new(n);
            for (int i = 0; i < n; ++i)
            {
                ref var dim = ref unit._dims[i];
                ref var _dim = ref _dims[i];
                dim.Factor = _dim.Factor;
                dim.Power = _dim.Power * f;
            }
            return unit;
        }

        internal static bool IsConsistent(Unit u1, Unit u2) =>
            u1 is null ?
            u2 is null :
            u1.IsConsistent(u2);


        private bool IsConsistent(Unit other)
        {
            if (other is null)
                return false;

            var n = _dims.Length;
            if (n != other._dims.Length)
                return false;

            for (int i = 0; i < n; ++i)
                if (_dims[i].Power != other._dims[i].Power)
                    return false;

            return true;
        }

        internal bool IsMultiple(Unit other)
        {
            var n = _dims.Length;
            if (n != other._dims.Length)
                return false;

            double? d1 = null;
            for (int i = 0; i < n; ++i)
            {
                var p1 = _dims[i].Power;
                var p2 = other._dims[i].Power;
                if (p1 != p2)
                {
                    if (p1 == 0f || p2 == 0f)
                        return false;

                    if (!d1.HasValue)
                        d1 = p2 - p1;
                    else
                    {
                        var d2 = p2 - p1;
                        if (d1.Value != d2)
                            return false;
                    }
                }
            }
            return true;
        }

        internal double ConvertTo(Unit u)
        {
            var factor = 1d;
            for (int i = 0, n = _dims.Length; i < n; ++i)
            {
                ref var dim = ref _dims[i];
                if (dim.Power != 0f && dim.Factor != u._dims[i].Factor)
                    factor *= MyPow(dim.Factor / u._dims[i].Factor, dim.Power);

            }
            return factor;
        }

        internal static Unit GetForceUnit(Unit u)
        {
            var i = (int)u._dims[1].Power + 3;
            if (i < 0 || i > 5 || Units.ContainsKey(u.Text))
                return u;

            var d = u._dims[0].Factor;
            if (Math.Truncate(d) == d)
                return ForceUnits[i];

            return ForceUnits_US[i];
        }

        internal static Unit GetElectricalUnit(Unit u)
        {
            if (Units.ContainsKey(u.Text))
                return u;

            if (ElectricalUnits.TryGetValue(u, out var eu))
                return eu;

            return u;
        }


        internal static string GetPrefix(int n)
        {
            return n switch
            {
                1 => "da",
                2 => "h",
                3 => "k",
                6 => "M",
                9 => "G",
                12 => "T",
                15 => "P",
                18 => "E",
                21 => "Z",
                24 => "Y",
                -1 => "d",
                -2 => "c",
                -3 => "m",
                -6 => "μ",
                -9 => "n",
                -12 => "p",
                -15 => "f",
                -18 => "a",
                -21 => "z",
                -24 => "y",
                _ => string.Empty,
            };
        }

        internal static double GetScale(int n)
        {
            return n switch
            {
                0 => 1d,
                1 => 10.0,
                2 => 100.0,
                3 => 1E+3,
                6 => 1E+6,
                9 => 1E+9,
                12 => 1E+12,
                15 => 1E+15,
                18 => 1E+18,
                21 => 1E+21,
                24 => 1E+24,
                -1 => 0.1,
                -2 => 0.01,
                -3 => 0.001,
                -6 => 1E-06,
                -9 => 1E-09,
                -12 => 1E-12,
                -15 => 1E-15,
                -18 => 1E-18,
                -21 => 1E-21,
                -24 => 1E-24,
                _ => Math.Pow(10, n),
            };
        }

        private static int GetPower(double factor)
        {
            var d = factor switch
            {
                1d => 0.0,
                10d => 1d,
                1E+2 => 2.0,
                1E+3 => 3.0,
                1E+4 => 4.0,
                1E+5 => 5.0,
                1E+6 => 6.0,
                0.1 => -1d,
                1E-2 => -2.0,
                1E-3 => -3.0,
                1E-4 => -4.0,
                1E-5 => -5.0,
                1E-6 => -6.0,
                _ => Math.Log10(factor),
            };
            var n = (int)d;
            return Math.Abs(n - d) < 1E-12 ? n : 0;
        }

        private static string GetDimText(OutputWriter writer, string name, double factor, float power)
        {
            if (factor != 1d)
            {
                switch (name)
                {
                    case "s":
                        {
                            if (factor == 60d || factor == 3600d || factor == 86400d)
                            {
                                name = factor switch
                                {
                                    60d => "min",
                                    3600d => "h",
                                    86400d => "d",
                                    _ => "s"
                                };
                                factor = 1d;
                            }
                            break;
                        }
                    case "g":
                        {
                            var a1 = factor / 14.59390294;
                            if (Math.Abs(a1 - Math.Round(a1)) < 1E-12)
                            {
                                name = "slug";
                                factor = a1;
                            }
                            else
                            {
                                var a2 = factor / 453.59237;
                                if (a2 < 1d)
                                {
                                    var a3 = 1d / a2;
                                    if (Math.Abs(a3 - Math.Round(a3)) < 1E-12)
                                    {
                                        a3 = Math.Round(a3);
                                        factor = 1d;
                                        switch (a3)
                                        {
                                            case 7000.0:
                                                name = "gr";
                                                break;
                                            case 256.0:
                                                name = "dr";
                                                break;
                                            case 16.0:
                                                name = "oz";
                                                break;
                                            default:
                                                name = "lb";
                                                factor = a3;
                                                break;
                                        }
                                    }
                                }
                                else if (Math.Abs(a2 - Math.Round(a2)) < 1E-12)
                                {
                                    a2 = Math.Round(a2);
                                    factor = 1d;
                                    switch (a2)
                                    {
                                        case 14.0:
                                            name = "st";
                                            break;
                                        case 28.0:
                                            name = "qr";
                                            break;
                                        case 100.0:
                                            name = "cwt_US";
                                            break;
                                        case 112.0:
                                            name = "cwt_UK";
                                            break;
                                        case 1000d:
                                            name = "kip";
                                            break;
                                        case 2000.0:
                                            name = "ton_US";
                                            break;
                                        case 2240.0:
                                            name = "ton_UK";
                                            break;
                                        default:
                                            name = "lb";
                                            factor = a2;
                                            break;
                                    }
                                }
                                else
                                    switch (factor)
                                    {
                                        case >= 1000000.0:
                                            factor /= 1000000.0;
                                            name = "t";
                                            break;
                                        case >= 1000d:
                                            factor /= 1000d;
                                            name = "kg";
                                            break;
                                    }
                            }
                            break;
                        }
                    case "m":
                        {
                            var a = factor / 2.54E-05;
                            if (Math.Abs(a - Math.Round(a)) < 1E-12)
                            {
                                a = Math.Round(a);
                                factor = 1d;
                                switch (a)
                                {
                                    case 1d:
                                        name = "th";
                                        break;
                                    case 1000d:
                                        name = "in";
                                        break;
                                    case 36000.0:
                                        name = "yd";
                                        break;
                                    case 792000.0:
                                        name = "ch";
                                        break;
                                    case 7920000.0:
                                        name = "fur";
                                        break;
                                    case 63360000.0:
                                        name = "mi";
                                        break;
                                    default:
                                        name = "ft";
                                        factor = a / 12000.0;
                                        break;
                                }
                            }
                            break;
                        }
                }
                var n = GetPower(factor);
                name = writer.FormatUnits(GetPrefix(n) + name);
                factor /= GetScale(n);
                if (Math.Abs(factor - 1) > 1e-12)
                    name = writer.AddBrackets(writer.FormatReal(factor, 6) + '·' + name);
            }
            else
                name = writer.FormatUnits(name);

            var sp = writer.FormatReal(power, 1);
            if (power < 0 && writer is TextWriter)
                sp = writer.AddBrackets(sp);

            return power != 1f ? writer.FormatPower(name, sp, 0, -1) : name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsComposite(double d, Unit units) =>
            units is not null &&
            (
                d > 0d &&
                d != 1d ||
                units.Text.IndexOfAny(CompositeUnitChars) >= 0d
            );

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static double Convert(Unit ua, Unit ub, char op)
        {
            if (!Unit.IsConsistent(ua, ub))
#if BG
                throw new MathParser.MathParserException($"Несъвместими мерни единици: \"{Unit.GetText(ua)} {op} {Unit.GetText(ub)}\".");
#else
                throw new MathParser.MathParserException($"Inconsistent units: \"{Unit.GetText(ua)} {op} {Unit.GetText(ub)}\".");
#endif
            return ua is null ? 1.0 : ub.ConvertTo(ua);
        }

        internal static Unit Multiply(Unit ua, Unit ub, out double d, bool updateText = false)
        {
            if (ua is null)
            {
                d = 1d;
                return ub is null ? null : ub;
            }
            if (ub is null)
            {
                d = 1d;
                return ua;
            }
            d = GetProductOrDivisionFactor(ua, ub);
            var uc = ua * ub;
            if (uc is null)
                return null;

            if (updateText && !ua.IsMultiple(ub))
            {
                uc.Scale(d);
                d = 1d;
                uc.Text = ua.Text + '·' + ub.Text;
            }
            return uc;
        }

        internal static Unit Divide(Unit ua, Unit ub, out double d, bool updateText = false)
        {
            if (ub is null)
            {
                d = 1d;
                return ua is null ? null : ua;
            }

            if (ua is null)
            {
                d = 1d;
                return ub.Pow(-1.0);
            }

            d = GetProductOrDivisionFactor(ua, ub, true);
            var uc = ua / ub;
            if (uc is null)
                return null;

            if (updateText && !ua.IsMultiple(ub))
            {
                uc.Scale(d);
                d = 1d;
                uc.Text = ua.Text + '/' + ub.Text;
            }
            return uc;
        }

        internal static Unit Pow(Unit u, Value power, bool updateText = false)
        {
            if (power.Units is not null)
#if BG
                throw new MathParser.MathParserException("Степенният показател трябва да е бездименсионен.");
#else
                throw new MathParser.MathParserException("Power must be unitless.");
#endif
            if (u is null)
                return null;

            if (!power.IsReal)
#if BG
                throw new MathParser.MathParserException("Не мога да повдигна мерни единици на комплексна степен.");
#else
                throw new MathParser.MathParserException("Units cannon be raised to complex power.");
#endif
            var result = u.Pow(power.Re);
            var s = u.Text;
            if (updateText && !s.Contains('^'))
            {
                var ps = OutputWriter.FormatNumberHelper(power.Re, 2);
                result.Text =
                    s.IndexOfAny(CompositeUnitChars) >= 0.0 ?
                    $"({s})^{ps}" :
                    s + '^' + ps;
            }
            return result;
        }

        internal static Unit Root(Unit u, int n, bool updateText = false)
        {
            var result = u.Pow(1.0 / n);
            var s = u.Text;
            if (updateText && !s.Contains('^'))
                result.Text =
                    s.IndexOfAny(CompositeUnitChars) >= 0 ?
                    $"({s})^1/{n}" :
                    s + $"^1/{n}";

            return result;
        }
    }
}