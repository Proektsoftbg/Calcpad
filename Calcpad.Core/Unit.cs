using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Calcpad.Core
{
    internal class Unit : IEquatable<Unit>
    {
        internal enum Field
        {
            Mechanical,
            Electrical,
            Other
        }

        private string _text;
        private int _hashCode;
        private char _tempChar = 'C';
        private readonly float[] _powers;
        private readonly double[] _factors;
        private int Length => _powers.Length;

        private static bool _isUs;
        private static readonly string CompositeUnitChars = Calculator.NegChar + "/^";
        private static readonly string[] Names = ["g", "m", "s", "A", "°C", "mol", "cd", "rad", ""];
        private static readonly Unit[] ForceUnits = new Unit[9], ForceUnits_US = new Unit[9];
        private static readonly FrozenSet<Unit> ElectricalUnits;
        private static FrozenDictionary<string, Unit> Units;
        private static readonly string[] UnitNames =
        [
            "therm",
            "cwt",
            "ton",
            "fl_oz",
            "gi",
            "pt",
            "qt",
            "gal",
            "bbl",
            "pk",
            "bu",
            "tonf"
        ];

        internal static bool IsUs
        {
            get => _isUs;
            set
            {
                _isUs = value;
                var suffix = value ? "_US" : "_UK";
                var units = new Dictionary<string, Unit>(Units);
                for (var i = 0; i < UnitNames.Length; ++i)
                {
                    ref var s = ref UnitNames[i];
                    units[s] = new(Units[s + suffix], s);
                }
                units["ton_f"] = new(Units["tonf"], "ton_f");
                Units = units.ToFrozenDictionary();
            }
        }

        internal Field GetField()
        {
            switch (Length)
            {
                case 3:
                    if (_powers[0] == 1f)
                    {
                        if (_powers[2] == -2f)
                        {
                            if (_text is null || _text.Contains('s'))
                                return Field.Mechanical;
                        }
                        else if (_powers[2] == -3f)
                        {
                            if (_powers[1] == 2f)
                                return Field.Electrical;
                        }
                    }
                    break;
                case 4:
                    if (_powers[3] != 0)
                        return Field.Electrical;

                    break;
            }
            return Field.Other;
        }

        private bool HasTemp => Length >= 5 && _powers[4] != 0f;

        internal bool IsTemp => Length == 5 &&
                                _powers[4] == 1f &&
                                _powers[0] == 0f &&
                                _powers[1] == 0f &&
                                _powers[2] == 0f &&
                                _powers[3] == 0f;

        internal bool IsAngle => Length == 8 &&
                                 _powers[7] == 1f &&
                                 _powers[0] == 0f &&
                                 _powers[1] == 0f &&
                                 _powers[2] == 0f &&
                                 _powers[3] == 0f &&
                                 _powers[4] == 0f &&
                                 _powers[5] == 0f &&
                                 _powers[6] == 0f;

        internal bool IsDimensionless => _powers.Length == 0;

        internal string Text
        {
            get
            {
                _text ??= GetText(OutputWriter.OutputFormat.Text);
                return _text;
            }
            set => _text = value;
        }

        internal string Html
        {
            get
            {
                if (_text is null)
                    return GetText(OutputWriter.OutputFormat.Html);

                return new HtmlWriter().FormatUnitsText(_text);
            }
        }

        internal string Xml
        {
            get
            {
                if (_text is null)
                    return GetText(OutputWriter.OutputFormat.Xml);

                return new XmlWriter().FormatUnitsText(_text);
            }
        }

        public override int GetHashCode()
        {
            if (_hashCode == 0)
            {
                var hash = new HashCode();
                var n = Length;
                hash.Add(n);
                for (int i = 0; i < n; ++i)
                {
                    if (_powers[i] != 0f)
                    {
                        hash.Add(_powers[i]);
                        hash.Add(_factors[i]);
                    }
                }
                _hashCode = hash.ToHashCode();
            }
            return _hashCode;
        }

        public override bool Equals(object obj) =>
            obj is Unit u && Equals(u);

        public bool Equals(Unit other) =>
            ReferenceEquals(this, other) ||
            IsConsistent(other) &&
            _factors.AsSpan().SequenceEqual(other._factors);

        internal Unit(int n)
        {
            _powers = new float[n];
            _factors = new double[n];
        }

        internal static Unit Get(string text) => Units[text];

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

            _factors = new double[n];
            _factors[0] = 1000d;
            Array.Fill(_factors, 1d, 1, n - 1);

            _powers = new float[n];
            _powers[0] = mass;
            if (n > 1)
            {
                _powers[1] = length;
                if (n > 2)
                {
                    _powers[2] = time;
                    if (n > 3)
                    {
                        _powers[3] = current;
                        if (n > 4)
                        {
                            _powers[4] = temp;
                            if (n > 5)
                            {
                                _powers[5] = substance;
                                {
                                    if (n > 6)
                                    {
                                        _powers[6] = luminosity;
                                        if (n > 7)
                                            _powers[7] = angle;
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
            _tempChar = u._tempChar;
            _hashCode = u._hashCode;
            var n = u.Length;
            _powers = u._powers;
            _factors = new double[n];
            //Array.Copy(u._factors, _factors, n);
            u._factors.AsSpan().CopyTo(_factors);
        }

        internal Unit(Unit u, string alias)
        {
            _text = alias;
            _tempChar = u._tempChar;
            _hashCode = u._hashCode;
            _powers = u._powers;
            _factors = u._factors;
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
            var g = new Unit("kg", 1f).Scale("g", 0.001);
            var m = new Unit("m", 0f, 1f);
            var mi = m.Scale("mi", 1609.344);
            var nmi = m.Scale("nmi", 1852d);
            var a = m.Pow(2).Scale("a", 100d);
            var L = m.Shift(-1).Pow(3);
            L._text = "L";
            var s = new Unit("s", 0f, 0f, 1f);
            var h = s.Scale("h", 3600d);
            var deg = new Unit("°C", 0f, 0f, 0f, 0f, 1f);
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

            var lbm = g.Scale("lb", 453.59237);
            var kipm = g.Scale("kipm", 453592.37);
            var lbf = N.Scale("lbf", 4.4482216153);
            var kipf = N.Scale("kip", 4448.2216153);
            var ksi = Pa.Scale("ksi", 6894757.29322959);

            ForceUnits[0] = kN * m.Pow(-4f);
            ForceUnits[1] = kN * m.Pow(-3f);
            ForceUnits[2] = kPa;
            ForceUnits[3] = kN * m.Pow(-1f);
            ForceUnits[4] = kN;
            ForceUnits[5] = kNm;
            ForceUnits[6] = kN * m.Pow(2f);
            ForceUnits[7] = kN * m.Pow(3f);
            ForceUnits[8] = kN * m.Pow(4f);

            ForceUnits[0]._text = "kN/m^4";
            ForceUnits[1]._text = "kN/m^3";
            ForceUnits[3]._text = "kN/m";
            ForceUnits[6]._text = "kN·m^2";
            ForceUnits[7]._text = "kN·m^3";
            ForceUnits[8]._text = "kN·m^4";

            ForceUnits_US[0] = (kipf * m.Pow(-4f)).Scale("kip/in^4", 1 / 4.162314256E-7);
            ForceUnits_US[1] = (kipf * m.Pow(-3f)).Scale("kip/in^3", 1 / 0.000016387064);
            ForceUnits_US[2] = ksi;
            ForceUnits_US[3] = (kipf * m.Pow(-1f)).Scale("kip/ft", 1 / 0.3048);
            ForceUnits_US[4] = kipf;
            ForceUnits_US[5] = (kipf * m).Scale("kip·ft", 0.3048);
            ForceUnits_US[6] = (kipf * m.Pow(2f)).Scale("kip·ft^2", 0.09290304);
            ForceUnits_US[7] = (kipf * m.Pow(3f)).Scale("kip·ft^3", 0.028316846592);
            ForceUnits_US[8] = (kipf * m.Pow(4f)).Scale("kip·ft^4", 0.0086309748412416);

            ElectricalUnits = new HashSet<Unit>()
            {
                S,    // -1, -2,  3,  2
                F,    // -1, -2,  4,  2
                C,    //  0,  0,  1,  1
                T,    //  1,  0, -2, -1
                Ohm,  //  1,  2, -3, -2
                V,    //  1,  2, -3, -1
                W,    //  1,  2, -3
                H,    //  1,  2, -2, -2
                Wb    //  1,  2, -2, -1
            }.ToFrozenSet();

            Dictionary<string, Unit> units = new(StringComparer.Ordinal)
            {
                {string.Empty, new(9)},
                {"%",     new(0) {_text = "%" } },
                {"‰",     new(0) {_text = "‰" } },
                {"g",     g},
                {"dag",   g.Shift(1)},
                {"hg",    g.Shift(2)},
                {"kg",    g.Shift(3)},
                {"t",     g.Scale("t", 1000000d)},
                {"kt",    g.Scale("kt", 1000000000d)},
                {"Mt",    g.Scale("Mt", 1000000000000d)},
                {"Gt",    g.Scale("Gt", 1E+15)},
                {"dg",    g.Shift(-1)},
                {"cg",    g.Shift(-2)},
                {"mg",    g.Shift(-3)},
                {"μg",    g.Shift(-6)},
                {"ng",    g.Shift(-9)},
                {"pg",    g.Shift(-12)},
                {"Da",    g.Scale("Da", 1.6605390666050505e-24)},
                {"u",     g.Scale("u", 1.6605390666050505e-24)},

                {"gr",    g.Scale("gr", 0.06479891)},
                {"dr",    g.Scale("dr", 1.7718451953125)},
                {"oz",    g.Scale("oz", 28.349523125)},
                {"lb",    lbm},
                {"lbm",   new(lbm, "lbm")},
                {"lb_m",  new(lbm, "lb_m")},
                {"kipm",  kipm},
                {"kip_m", new(kipm, "kip_m")},
                {"klb",   new(kipm, "klb")},
                {"st",    g.Scale("st", 6350.29318)},
                {"qr",    g.Scale("qr", 12700.58636)},
                {"cwt_US",g.Scale("cwt_US", 45359.237 )},
                {"cwt_UK",g.Scale("cwt_UK", 50802.34544)},
                {"ton_US",g.Scale("ton_US", 907184.74)},
                {"ton_UK",g.Scale("ton_UK", 1016046.9088)},
                {"slug",  g.Scale("slug", 14593.90294)},

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
                {"ftm",   m.Scale("ftm", 1.8288)},
                {"ftm_UK",   m.Scale("ftm_UK", 1.852)},
                {"ftm_US",   m.Scale("ftm_US", 1.8288)},
                {"cable",    m.Scale("cable", 182.88)},
                {"cable_UK", m.Scale("cable_UK", 185.2)},
                {"cable_US", m.Scale("cable_US", 219.456)},
                {"nmi",   nmi},
                {"li",    m.Scale("li", 0.201168)},
                {"rod",   m.Scale("rod", 5.0292)},
                {"pole",  m.Scale("pole", 5.0292)},
                {"perch", m.Scale("perch", 5.0292)},
                {"lea",   m.Scale("lea", 4828.032)},

                {"a",  a},
                {"daa",a.Scale("daa", 10d)},
                {"ha", a.Scale("ha", 100d)},
                {"L",  L},
                {"dL", L.Shift(-1)},
                {"cL", L.Shift(-2)},
                {"mL", L.Shift(-3)},
                {"μL", L.Shift(-6)},
                {"nL", L.Shift(-9)},
                {"pL", L.Shift(-12)},
                {"daL", L.Scale("daL", 10d)},
                {"hL", L.Scale("hL", 100d)},

                {"rood",     m.Pow(2).Scale("rood", 1011.7141056)},
                {"ac",       m.Pow(2).Scale("ac", 4046.8564224)},

                {"fl_dr_UK", L.Scale("fl_dr_UK", 3.5516328125e-3)},
                {"fl_oz_UK", L.Scale("fl_oz_UK", 0.0284130625)},
                {"gi_UK",    L.Scale("gi_UK",  0.1420653125)},
                {"pt_UK",    L.Scale("pt_UK", 0.56826125)},
                {"qt_UK",    L.Scale("qt_UK", 1.1365225)},
                {"gal_UK",   L.Scale("gal_UK", 4.54609)},
                {"bbl_UK",   L.Scale("bbl_UK", 163.65924)},
                {"pk_UK",    L.Scale("pk_UK", 9.09218)},
                {"bu_UK",    L.Scale("bu_UK", 36.36872)},

                {"fl_dr_US", L.Scale("fl_dr_US", 3.6966911953125e-3)},
                {"fl_oz_US", L.Scale("fl_oz_US", 0.0295735295625)},
                {"gi_US",    L.Scale("gi_US", 0.11829411825)},
                {"pt_US",    L.Scale("pt_US", 0.473176473)},
                {"qt_US",    L.Scale("qt_US", 0.946352946)},
                {"gal_US",   L.Scale("gal_US", 3.785411784)},
                {"bbl_US",   L.Scale("bbl_US", 119.240471196)},

                {"pt_dry",   L.Scale("pt_dry",0.5506104713575)},
                {"qt_dry",   L.Scale("qt_dry", 1.101220942715)},
                {"gal_dry",  L.Scale("gal_dry", 4.40488377086)},
                {"bbl_dry",  L.Scale("bbl_dry", 115.628198985075)},
                {"pk_US",    L.Scale("pk_US", 8.80976754172)},
                {"bu_US",    L.Scale("bu_US", 35.23907016688)},

                {"s",    s},
                {"ms",   s.Shift(-3)},
                {"μs",   s.Shift(-6)},
                {"ns",   s.Shift(-9)},
                {"ps",   s.Shift(-12)},
                {"min",  s.Scale("min", 60d)},
                {"h",    h},
                {"d",    h.Scale("d", 24)},
                {"w",    h.Scale("w", 7 * 24)},
                {"y",    h.Scale("y", 365 * 24)},
                {"kmh",  new(m.Shift(3) / h, "kmh")},
                {"mph",  new(mi / h, "mph")},
                {"knot", new(nmi / h, "knot")},
                {"Hz",   Hz},
                {"kHz",  Hz.Shift(3)},
                {"MHz",  Hz.Shift(6)},
                {"GHz",  Hz.Shift(9)},
                {"THz",  Hz.Shift(12)},
                {"mHz",  Hz.Shift(-3)},
                {"μHz",  Hz.Shift(-6)},
                {"nHz",  Hz.Shift(-9)},
                {"pHz",  Hz.Shift(-12)},
                {"rpm",  Hz.Scale("rpm", 1d / 60d)},

                {"A",   A},
                {"kA",  A.Shift(3)},
                {"MA",  A.Shift(6)},
                {"GA",  A.Shift(9)},
                {"TA",  A.Shift(12)},
                {"mA",  A.Shift(-3)},
                {"μA",  A.Shift(-6)},
                {"nA",  A.Shift(-9)},
                {"pA",  A.Shift(-12)},
                {"Ah",  new(A * h, "Ah")},
                {"mAh", new(A.Shift(-3) * h, "mAh")},

                {"°C",  deg.Scale("°C", 1d)},
                {"Δ°C", deg.Scale("Δ°C",1d)},
                {"K",   deg.Scale("K",  1d)},
                {"°F",  deg.Scale("°F", 5d / 9d)},
                {"Δ°F", deg.Scale("Δ°F", 5d / 9d)},
                {"°R",  deg.Scale("°R", 5d / 9d)},

                {"mol", new("mol", 0f, 0f, 0f, 0f, 0f, 1f)},
                {"cd",  new("cd",  0f, 0f, 0f, 0f, 0f, 0f, 1f)},

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
                {"lbf",  lbf},
                {"kip",  kipf},
                {"kipf", new(kipf, "kipf")},
                {"tonf_US", N.Scale("tonf_US", 8896.443230521)},
                {"tonf_UK", N.Scale("tonf_UK", 9964.01641818352)},
                {"pdl",  N.Scale("pdl", 0.138254954376)},

                {"oz_f",  N.Scale("oz_f", 0.278013851)},
                {"lb_f",  new(lbf, "lb_f")},
                {"kip_f", new(kipf, "kip_f")},

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

                {"P", (Pa*s).Scale("P", 0.1)},
                {"cP", (Pa*s).Scale("cP", 0.001)},
                {"St", (m*m/s).Scale("St", 0.0001)},
                {"cSt", (m*m/s).Scale("cSt", 0.000001)},

                {"J",   J},
                {"kJ",  J.Shift(3)},
                {"MJ",  J.Shift(6)},
                {"GJ",  J.Shift(9)},
                {"TJ",  J.Shift(12)},
                {"mJ",  J.Shift(-3)},
                {"μJ",  J.Shift(-6)},
                {"nJ",  J.Shift(-9)},
                {"pJ",  J.Shift(-12)},
                {"Wh",  J.Scale("Wh",  3.6e+3)},
                {"kWh", J.Scale("kWh", 3.6e+6)},
                {"MWh", J.Scale("MWh", 3.6e+9)},
                {"GWh", J.Scale("GWh", 3.6e+12)},
                {"TWh", J.Scale("TWh", 3.6e+15)},
                {"mWh", J.Scale("mWh", 3.6)},
                {"μWh", J.Scale("μWh", 3.6e-3)},
                {"nWh", J.Scale("nWh", 3.6e-6)},
                {"pWh", J.Scale("pWh", 3.6e-9)},

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

                {"lm",  new("lm",  0,  0,  0, 0, 0, 0, 1)},
                {"lx",  new("lx",  0, -2,  0, 0, 0, 0, 1)},
                {"kat", new("kat", 0,  0, -1, 0, 0, 1)},

                {"°",  new("°",       0, 0, 0, 0, 0, 0, 0, 1)},
                {"′",  new("′",       0, 0, 0, 0, 0, 0, 0, 1)},
                {"″",  new("″",       0, 0, 0, 0, 0, 0, 0, 1)},
                {"rad",  new("rad",   0, 0, 0, 0, 0, 0, 0, 1)},
                {"grad",  new("grad", 0, 0, 0, 0, 0, 0, 0, 1)},
                {"rev",  new("rev",   0, 0, 0, 0, 0, 0, 0, 1)}
            };
            var u = units[string.Empty];
            u._powers[8] = 1f;
            u._factors[8] = 1d;
            units["°"].Scale(Math.PI / 180.0);
            units["′"].Scale(Math.PI / 10800.0);
            units["″"].Scale(Math.PI / 648000.0);
            units.Add("deg", units["°"]);
            units["grad"].Scale(Math.PI / 200.0);
            units["rev"].Scale(Math.Tau);
            units["K"]._tempChar = 'K';
            units["°F"]._tempChar = 'F';
            units["Δ°F"]._tempChar = 'F';
            units["°R"]._tempChar = 'R';
            for (var i = 0; i < UnitNames.Length; ++i)
            {
                ref var name = ref UnitNames[i];
                units.Add(name, new(units[name + "_UK"], name));
            }
            Units = units.ToFrozenDictionary();
        }

        internal void Scale(double factor)
        {
            for (int i = 0, n = Length; i < n; ++i)
            {
                if (_powers[i] != 0f)
                {
                    _factors[i] *= Math.Pow(factor, 1d / _powers[i]);
                    return;
                }
            }
        }

        internal Unit Shift(int n) => Scale(GetPrefix(n) + _text, GetScale(n));
        internal Unit Scale(string s, double factor)
        {
            var unit = new Unit(this) { _text = s };
            if (factor != 1d)
                unit.Scale(factor);

            return unit;
        }

        private string UnitName(int i) =>
            i == 4 ? _tempChar switch
            {
                'K' => "K",
                'F' => "°F",
                'R' => "°R",
                _ => Names[i]
            } : Names[i];

        private string GetText(OutputWriter.OutputFormat format)
        {
            OutputWriter writer = format switch
            {
                OutputWriter.OutputFormat.Html => new HtmlWriter(),
                OutputWriter.OutputFormat.Xml => new XmlWriter(),
                _ => new TextWriter()
            };
            var stringBuilder = new StringBuilder(50);
            var isFirst = true;
            for (int i = 0, n = Length; i < n; ++i)
            {
                if (_powers[i] != 0f)
                {
                    var absPow = isFirst ? _powers[i] : Math.Abs(_powers[i]);
                    var s = GetDimText(writer, UnitName(i), _factors[i], absPow);
                    if (i == 4 && stringBuilder.Length > 0)
                        s = TemperatureToDelta(s);

                    if (isFirst)
                        isFirst = false;
                    else
                    {
                        var oper = _powers[i] > 0f ? '·' : '/';
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
            unit.Scale(d);
            return unit;
        }

        public static Unit operator *(double d, Unit u) => u * d;

        public static Unit operator *(Unit u1, Unit u2) => MultiplyOrDivide(u1, u2);

        public static Unit operator /(Unit u1, Unit u2) => MultiplyOrDivide(u1, u2, true);

        private static Unit MultiplyOrDivide(Unit u1, Unit u2, bool divide = false)
        {
            if (ReferenceEquals(u1, u2) ||
                ReferenceEquals(u1._powers, u2._powers))
                return divide ? null : u1.Pow(2);

            var n1 = u1.Length;
            var n2 = u2.Length;
            var n = n1 > n2 ? n1 : n2;
            if (n1 == n2)
            {
                if (divide)
                    do { if (--n < 0) return null; } while (u1._powers[n] == u2._powers[n]);
                else
                    do { if (--n < 0) return null; } while (u1._powers[n] == -u2._powers[n]);
                ++n;
            }
            Unit unit = new(n);
            if (u1.HasTemp)
                unit._tempChar = u1._tempChar;
            else if (u2.HasTemp)
                unit._tempChar = u2._tempChar;

            if (n1 > n) n1 = n;
            if (n2 > n) n2 = n;
            for (int i = 0; i < n1; ++i)
            {
                var p1 = u1._powers[i];
                if (i < n2)
                {
                    var p2 = u2._powers[i];
                    unit._powers[i] = divide ? p1 - p2 : p1 + p2;
                    unit._factors[i] =
                        p1 != 0f ?
                            u1._factors[i] :
                            p2 != 0 ?
                                u2._factors[i] :
                                1f;
                }
                else
                {
                    unit._factors[i] = p1 != 0f ? u1._factors[i] : 1f;
                    unit._powers[i] = p1;
                }
            }
            var len = n2 - n1;
            if (len > 0)
            {
                //Array.Copy(u2._factors, n1, unit._factors, n1, len);
                u2._factors.AsSpan(n1, len).CopyTo(unit._factors.AsSpan(n1));

                if (divide)
                    for (int i = n1; i < n2; ++i)
                        unit._powers[i] = -u2._powers[i];
                else
                {
                    //Array.Copy(u2._powers, n1, unit._powers, n1, len);
                    u2._powers.AsSpan(n1, len).CopyTo(unit._powers.AsSpan(n1));
                }
            }
            return unit;
        }

        internal static double GetProductOrDivisionFactor(Unit u1, Unit u2, bool divide = false)
        {
            if (ReferenceEquals(u1, u2))
                return 1d;

            var n1 = u1.Length;
            var n2 = u2.Length;
            var n = n1 < n2 ? n1 : n2;
            var factor = 1d;
            if (divide)
            {
                for (int i = 0; i < n; ++i)
                    if (u1._powers[i] != 0f && u2._powers[i] != 0f)
                        factor *= MyPow(u1._factors[i] / u2._factors[i], u2._powers[i]);
            }
            else
            {
                for (int i = 0; i < n; ++i)
                    if (u1._powers[i] != 0f && u2._powers[i] != 0f)
                        factor *= MyPow(u2._factors[i] / u1._factors[i], u2._powers[i]);
            }
            return factor;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double MyPow(double x, float y) => y switch
        {
            -3f => 1d / (x * x * x),
            -2f => 1d / (x * x),
            -1f => 1d / x,
            0f => 1d,
            1f => x,
            2f => x * x,
            3f => x * x * x,
            _ => Math.Pow(x, y)
        };

        public static Unit operator /(Unit u, double d) => u * (1d / d);

        public static Unit operator /(double d, Unit u) => d * u.Pow(-1f);

        internal Unit Pow(float x)
        {
            var n = Length;
            Unit unit = new(n)
            {
                _tempChar = _tempChar
            };
            //Array.Copy(_factors, unit._factors, n);
            _factors.AsSpan().CopyTo(unit._factors);
            for (int i = 0; i < n; ++i)
                unit._powers[i] = _powers[i] * x;

            return unit;
        }

        internal static bool IsConsistent(Unit u1, Unit u2) =>
            ReferenceEquals(u1, u2) ||
            u1 is not null &&
            u2 is not null &&
            u1.IsConsistent(u2);


        private bool IsConsistent(Unit other)
        {
            if (ReferenceEquals(_powers, other._powers))
                return true;

            return _powers.AsSpan().SequenceEqual(other._powers);
        }

        internal bool IsMultiple(Unit other)
        {
            var n = Length;
            if (n != other.Length)
                return false;

            double d1 = 0d;
            for (int i = 0; i < n; ++i)
            {
                ref var p1 = ref _powers[i];
                ref var p2 = ref other._powers[i];
                if (p1 != p2)
                {
                    if (p1 == 0f || p2 == 0f)
                        return false;

                    if (d1 == 0)
                        d1 = p2 - p1;
                    else
                    {
                        var d2 = p2 - p1;
                        if (d1 != d2)
                            return false;
                    }
                }
            }
            return true;
        }

        internal double ConvertTo(Unit u)
        {
            var factor = 1d;
            for (int i = 0, n = Length; i < n; ++i)
            {
                if (_powers[i] != 0f)
                    factor *= MyPow(_factors[i] / u._factors[i], _powers[i]);
            }
            return factor;
        }

        internal static Unit GetForceUnit(Unit u)
        {
            var i = (int)u._powers[1] + 3;
            if (i < 0 || i > 5)
                return u;

            if (u._factors[0] % 1 == 0)
                return ForceUnits[i];

            return ForceUnits_US[i];
        }

        internal static Unit GetElectricalUnit(Unit u)
        {
            if (!u._text?.StartsWith("VA") ?? true)
                if (ElectricalUnits.TryGetValue(u, out var eu))
                    return eu;

            return u;
        }

        internal static string GetPrefix(int n)
        {
            return n switch
            {
                -24 => "y",
                -21 => "z",
                -18 => "a",
                -15 => "f",
                -12 => "p",
                -9 => "n",
                -6 => "μ",
                -3 => "m",
                -2 => "c",
                -1 => "d",
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
                _ => string.Empty,
            };
        }

        internal static double GetScale(int n)
        {
            return n switch
            {
                -24 => 1E-24,
                -21 => 1E-21,
                -18 => 1E-18,
                -15 => 1E-15,
                -12 => 1E-12,
                -9 => 1E-09,
                -6 => 1E-06,
                -3 => 0.001,
                -2 => 0.01,
                -1 => 0.1,
                0 => 1.0,
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
                _ => Math.Pow(10, n)
            };
        }

        private static int GetPower(double factor) =>
            factor switch
            {
                1E-6 => -6,
                1E-5 => -5,
                1E-4 => -4,
                1E-3 => -3,
                1E-2 => -2,
                0.1 => -1,
                1d => 0,
                10d => 1,
                1E+2 => 2,
                1E+3 => 3,
                1E+4 => 4,
                1E+5 => 5,
                1E+6 => 6,
                _ => GetIntPower(factor)
            };

        private static int GetIntPower(double factor)
        {
            var d = Math.Log10(factor);
            var n = (int)d;
            return Math.Abs(n - d) < 1E-12 ? n : 0;
        }

        internal double Normalize()
        {
            var n = _factors.Length;
            if (n == 0 || _text is not null)
                return 1d;

            var factor = 1d;
            for (int i = 0; i < n; ++i)
            {
                if (_powers[i] == 0f)
                    continue;

                var d = _factors[i];
                if (d != 1d)
                {
                    d = NormDim(d, UnitName(i));
                    _factors[i] /= d;
                    factor *= Math.Pow(d, _powers[i]);
                }
            }
            return factor;
        }

        private static double NormDim(double factor, string name)
        {
            if (factor == 1d)
                return 1d;

            switch (name)
            {
                case "s":
                    {
                        if (factor == 60d ||
                            factor == 3600d ||
                            factor == 86400d)
                            return 1d;
                        break;
                    }
                case "g":
                    {
                        var a1 = factor / 14.59390294;
                        if (Math.Abs(a1 - Math.Round(a1)) < 1e-12)
                            factor = a1;
                        else
                        {
                            var a2 = factor / 453.59237;
                            if (a2 < 1d)
                            {
                                var a3 = 1d / a2;
                                if (Math.Abs(a3 - Math.Round(a3)) < 1e-12)
                                {
                                    a3 = Math.Round(a3);
                                    if (a3 == 7000d ||
                                        a3 == 256d ||
                                        a3 == 16d)
                                        return 1d;
                                    else
                                        factor = a3;
                                }
                            }
                            else if (Math.Abs(a2 - Math.Round(a2)) < 1e-12)
                            {
                                a2 = Math.Round(a2);
                                if (a2 == 14d ||
                                    a2 == 28d ||
                                    a2 == 100d ||
                                    a2 == 112d ||
                                    a2 == 1000d ||
                                    a2 == 2000d ||
                                    a2 == 2240d)
                                    return 1d;
                                else
                                    factor = a2;
                            }
                            else
                                if (factor >= 1000000d)
                                factor /= 1000000d;
                            else if (factor >= 1000d)
                                factor /= 1000d;
                        }
                        break;
                    }
                case "m":
                    {
                        if (factor == 1852d)
                            return 1d;
                        else
                        {
                            var a = factor / 2.54e-05;
                            if (Math.Abs(a - Math.Round(a)) < 1e-12)
                            {
                                a = Math.Round(a);
                                if (a == 1d ||
                                    a == 1000d ||
                                    a == 36000d ||
                                    a == 792000d ||
                                    a == 7920000d ||
                                    a == 63360000d)
                                    return 1d;
                                else
                                    factor = a / 12000d;
                            }
                        }
                        break;
                    }
                case "rad":
                    {
                        if (Math.Abs(factor - Math.Tau) < 1e-12)
                            return 1d;

                        var a = Math.PI / factor;
                        if (Math.Abs(a - Math.Round(a)) < 1e-12)
                        {
                            var b = Math.Round(a);
                            if (b == 180d ||
                                b == 200d ||
                                b == 10800d ||
                                b == 648000d)
                                return 1d;
                            else
                                return a * Math.PI;
                        }
                        break;
                    }
            }
            var n = GetPower(factor);
            factor /= GetScale(n);
            if (Math.Abs(factor - 1) > 1e-12)
            {
                if (name == "g")
                    factor *= 1000d;

                return factor;
            }
            return 1d;
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
                            if (Math.Abs(a1 - Math.Round(a1)) < 1e-12)
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
                                    if (Math.Abs(a3 - Math.Round(a3)) < 1e-12)
                                    {
                                        a3 = Math.Round(a3);
                                        factor = 1d;
                                        switch (a3)
                                        {
                                            case 7000d:
                                                name = "gr";
                                                break;
                                            case 256d:
                                                name = "dr";
                                                break;
                                            case 16d:
                                                name = "oz";
                                                break;
                                            default:
                                                name = "lb";
                                                factor = a3;
                                                break;
                                        }
                                    }
                                }
                                else if (Math.Abs(a2 - Math.Round(a2)) < 1e-12)
                                {
                                    a2 = Math.Round(a2);
                                    factor = 1d;
                                    switch (a2)
                                    {
                                        case 14d:
                                            name = "st";
                                            break;
                                        case 28d:
                                            name = "qr";
                                            break;
                                        case 100d:
                                            name = "cwt_US";
                                            break;
                                        case 112d:
                                            name = "cwt_UK";
                                            break;
                                        case 1000d:
                                            name = "kip_m";
                                            break;
                                        case 2000d:
                                            name = "ton_US";
                                            break;
                                        case 2240d:
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
                                        case >= 1000000d:
                                            factor /= 1000000d;
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
                            if (factor == 1852d)
                            {
                                name = "nmi";
                                factor = 1d;
                                break;
                            }
                            var a = factor / 2.54e-05;
                            if (Math.Abs(a - Math.Round(a)) < 1e-12)
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
                                    case 36000d:
                                        name = "yd";
                                        break;
                                    case 792000d:
                                        name = "ch";
                                        break;
                                    case 7920000d:
                                        name = "fur";
                                        break;
                                    case 63360000d:
                                        name = "mi";
                                        break;
                                    default:
                                        name = "ft";
                                        factor = a / 12000d;
                                        break;
                                }
                            }
                            break;
                        }
                    case "rad":
                        {
                            if (Math.Abs(factor - Math.Tau) < 1e-12)
                            {
                                name = "rev";
                                factor = 1d;
                                break;
                            }
                            var a = Math.PI / factor;
                            if (Math.Abs(a - Math.Round(a)) < 1e-12)
                            {
                                var b = Math.Round(a);
                                factor = 1d;
                                switch (b)
                                {
                                    case 180d:
                                        name = "deg";
                                        break;
                                    case 200d:
                                        name = "grad";
                                        break;
                                    case 10800d:
                                        name = "′";
                                        break;
                                    case 648000d:
                                        name = "″";
                                        break;
                                    default:
                                        name = "rad";
                                        factor = a * Math.PI;
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

            if (power == 1f)
                return name;

            var sp = writer.FormatReal(power, 1);
            if (power < 0 && writer is TextWriter)
                sp = writer.AddBrackets(sp);

            return writer.FormatPower(name, sp, 0, -1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsComposite(double d, Unit units) =>
            units is not null &&
            (
                d > 0d &&
                d != 1d ||
                units._text.AsSpan().IndexOfAny(CompositeUnitChars) >= 0
            );

        internal static double Convert(Unit ua, Unit ub, char op)
        {
            if (ReferenceEquals(ua, ub))
                return 1d;

            if (ua is null || ub is null || !ua.IsConsistent(ub))
                Throw.InconsistentUnitsOperationException(GetText(ua), op, GetText(ub));

            return ub.ConvertTo(ua);
        }

        internal static Unit Multiply(Unit ua, Unit ub, out double d, bool updateText = false)
        {
            if (ua is null)
            {
                d = 1d;
                return ub;
            }
            if (ub is null)
            {
                d = 1d;
                return ua;
            }
            d = GetProductOrDivisionFactor(ua, ub);
            var uc = MultiplyOrDivide(ua, ub);
            if (uc is null)
                return null;

            if (updateText && !ua.IsMultiple(ub))
            {
                uc.Scale(d);
                d = 1d;
                uc._text = ua.Text + '·' + ub.Text;
            }
            return uc;
        }

        internal static Unit Divide(Unit ua, Unit ub, out double d, bool updateText = false)
        {
            if (ub is null)
            {
                d = 1d;
                return ua;
            }
            if (ua is null)
            {
                d = 1d;
                return ub.Pow(-1f);
            }

            d = GetProductOrDivisionFactor(ua, ub, true);
            var uc = MultiplyOrDivide(ua, ub, true);
            if (uc is null)
                return null;

            if (updateText && !ua.IsMultiple(ub))
            {
                uc.Scale(d);
                d = 1d;
                if (ub.Text.Contains('·'))
                    uc._text = $"{ua.Text}/({ub.Text})";
                else
                    uc._text = ua.Text + '/' + ub.Text;
            }
            return uc;
        }

        internal static Unit Pow(Unit u, Value power, bool updateText = false)
        {
            if (power.Units is not null)
                Throw.PowerNotUnitlessException();

            if (u is null)
                return null;

            if (!power.IsReal)
                Throw.UnitsToComplexPowerException();

            var d = power.Re;
            var result = u.Pow((float)d);
            if (updateText)
            {
                ReadOnlySpan<char> s = u.Text;
                if (!s.Contains('^'))
                {
                    var ps = d < 0 ?
                        $"({OutputWriter.FormatNumberHelper(d, 2)})" :
                        OutputWriter.FormatNumberHelper(d, 2);

                    result._text =
                        s.IndexOfAny(CompositeUnitChars) >= 0.0 ?
                        $"({s})^{ps}" :
                        $"{s}^{ps}";
                }
            }
            return result;
        }

        internal static Unit Root(Unit u, int n, bool updateText = false)
        {
            var result = u.Pow(1f / n);

            if (updateText && Math.Abs(n) > 1)
            {
                ReadOnlySpan<char> s = u.Text;
                if (!s.Contains('^'))
                    result._text =
                        s.IndexOfAny(CompositeUnitChars) >= 0 ?
                        $"({s})^1⁄{n}" :
                        $"{s}^1⁄{n}";
            }
            return result;
        }
    }
}