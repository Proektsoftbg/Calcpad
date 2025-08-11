using Calcpad.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Calcpad.Wpf
{
    internal class AutoCompleteManager
    {
        private readonly RichTextBox _richTextBox;
        private readonly ListBox _listBox;
        private TextPointer _autoCompleteStart;
        private readonly Dispatcher _dispatcher;
        private readonly InsertManager _insertManager;
        private readonly int _autoCompleteCount;

        internal AutoCompleteManager(RichTextBox richTextBox, ListBox listBox, Dispatcher dispatcher, InsertManager insertManager)
        {
            _richTextBox = richTextBox;
            _listBox = listBox;
            _listBox.PreviewMouseLeftButtonUp += AutoCompleteListBox_PreviewMouseLeftButtonUp;
            _listBox.PreviewKeyDown += AutoCompleteListBox_PreviewKeyDown;
            FillListItems();
            _autoCompleteCount = _listBox.Items.Count;
            _dispatcher = dispatcher;
            _insertManager = insertManager;
        }

        private void FillListItems()
        {
            var items = _listBox.Items;
            items.Add(new ListBoxItem() { Content = "#append", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#break", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#complex", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#continue", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#def", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#deg", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#else if", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#else", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#end def", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#end if", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#equ", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#for", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#format", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#format default", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#global", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#gra", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#hide", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#if", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#include", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#input", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#local", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#loop", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#noc", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#md", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#md on", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#md off", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#nosub", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#novar", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#pause", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#phasor", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#post", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#pre", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#rad", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#read", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#repeat", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#round", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#round default", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#show", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#split", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#val", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#varsub", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#while", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#wrap", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "#write", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Area{f(x) @ x = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Find{f(x) @ x = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Inf{f(x) @ x = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Integral{f(x) @ x = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Map{f(x; y) @ x = a : b & y = c : d}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Plot{f(x) @ x = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Product{f(k) @ k = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Repeat{f(k) @ k = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Root{f(x) = const @ x = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Slope{f(x) @ x = a}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Sum{f(k) @ k = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "$Sup{f(x) @ x = a : b}", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "A", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "a", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "abs(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "ac", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "acos(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "acosh(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "acot(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "acoth(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "acsc(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "acsch(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "add(A; B; i; j)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "adj(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Ah", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "and(M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "asec(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "asech(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "asin(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "asinh(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "at", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "atan(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "atan2(x; y)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "atanh(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "atm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "AU", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "augment(A; B; C…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "average(M; V; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "bar", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "bbl", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "bbl_dry", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "bbl_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "bbl_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Bq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "BTU", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "bu", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "bu_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "bu_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "C", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "C/kg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cable", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cable_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cable_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cal", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cbrt(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cd", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cd/m^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ceiling(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ch", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cholesky(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Ci", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cL", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "clrUnits(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "clsolve(A; b)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cmsolve(A; B)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cm^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cofactor(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "col(M; j)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "column(m; c)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "column_hp(m; c)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cond(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cond_1(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cond_2(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cond_e(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cond_i(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "conj(z)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "copy(A; B; i; j)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cos(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cosh(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cot(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "coth(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "count(v; x; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cross(a; b)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "csc(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "csch(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "cwt", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cwt_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "cwt_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "d", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Da", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "daa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "daL", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "daN", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "daPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "deg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "default", Foreground = Brushes.DarkMagenta });
            items.Add(new ListBoxItem() { Content = "det(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "dg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "diag2vec(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "diagonal(n; d)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "diagonal_hp(n; d)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "dL", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "dm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "dm^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "dm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "dot(a; b)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "dPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "dr", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "EeV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "eigen(M; nₑ)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "eigenvals(M; nₑ)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "eigenvecs(M; nₑ)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "erg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "eV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "extract(v; vi)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "extract_cols(M; vj)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "extract_rows(M; vi)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "F", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "fft(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "ift(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "fill(v; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "fill_col(M; j; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "fill_row(M; i; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "find(v; x; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "find_eq(v; x; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "find_ge(v; x; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "find_gt(v; x; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "find_le(v; x; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "find_lt(v; x; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "find_ne(v; x; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "first(v; n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "fl_oz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "fl_oz_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "fl_oz_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "floor(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "fprod(A; B)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "ft", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ft*lb_f", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ft*lb_f/h", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ft*lb_f/min", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ft*lb_f/s", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ft*oz_f", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ft/s", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ft^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ftm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ftm_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ftm_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "fur", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "g", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "g/cm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "g/mm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gal", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gal_dry", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gal_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gal_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GBq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GC", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gcd(x; y; z…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "getUnits(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "GeV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GF", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GGy", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GH", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GHz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gi", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gi_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gi_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GJ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GN", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "gr", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "grad", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GS", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GSv", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GT", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Gt", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GVA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GVAR", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GW", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GWb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GWh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Gy", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "GΩ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "G℧", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "h", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "H", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ha", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "hg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "hL", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "hlookup(M; x; i₁; i₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "hlookup_eq(M; x; i₁; i₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "hlookup_ge(M; x; i₁; i₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "hlookup_gt(M; x; i₁; i₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "hlookup_le(M; x; i₁; i₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "hlookup_lt(M; x; i₁; i₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "hlookup_ne(M; x; i₁; i₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "hN", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "hp", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "hPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "hpE", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "hp(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "hprod(A; B)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "hpS", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Hz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "identity(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "identity_hp(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "if(cond; vt; vf)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "im(z)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "in", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "in*lb_f", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "in*oz_f", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "in/s", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "in^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "in^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "inverse(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "isHp(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "J", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "join(M; v; x…) ", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "join_cols(c₁; c₂; c₃…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "join_rows(r₁; r₂; r₃…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "K", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kat", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kBq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kC", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kcal", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "keV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kF", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kg/cm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kgf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kgf/cm^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kgf/cm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kGy", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kH", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kHz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kip", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kip*ft", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kip/ft", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kip/ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kip/in^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kip_f", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kip_m", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kipf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kipm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kJ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "klb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "klb/ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "klb/in^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "km", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "km^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "km^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kmh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kN", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kN*cm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kN/cm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kN/cm^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kN/cm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kN/m", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kN/m^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kN/m^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kNm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kprod(A; B)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "kS", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ksf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ksi", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kSv", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kT", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kt", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kVA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kVAR", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kW", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kWb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kWh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "kΩ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "k℧", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "L", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "last(v; n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "lb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb/bu", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb/ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb/gal", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb/in^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb/yd^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb_f", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb_f*ft", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb_f*in", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb_f/ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb_f/in^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lb_m", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lbf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lbm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lcm(x; y; z…) ", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "lea", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "len(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "li", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "line(x; M; v; y…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "line(x; y; M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "lm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lm*s", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lm*s/m^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lm/W", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ln(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "log(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "log_2(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "lookup(a; b; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Lookup_eq(a; b; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Lookup_ge(a; b; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Lookup_gt(a; b; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Lookup_le(a; b; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Lookup_lt(a; b; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Lookup_ne(a; b; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "lsolve(A; b)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "ltriang(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "ltriang_hp(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "lu(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "lx", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "lx*s", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ly", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "m", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "m/s", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "m^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "m^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mAh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "matrix(m; n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "matrix_hp(m; n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "max(M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mbar", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mBq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MBq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mC", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MC", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mcount(M; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mean(M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "MeV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mF", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MF", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mfill(M; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mfind(M; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mfind_eq(M; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mfind_ge(M; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mfind_gt(M; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mfind_le(M; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mfind_lt(M; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mfind_ne(M; x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MGy", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mGy", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MH", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mH", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MHz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mHz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mi", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mi^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mi^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "min", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "min(M; v; x…) ", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "MJ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mJ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mL", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mm^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mmHg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MN", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mnorm(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mnorm_1(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mnorm_2(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mnorm_e(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mnorm_i(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mod(x; y)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "mol", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mph", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mresize(M; m; n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "MS", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mS", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ms", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "msearch(M; x; i; j)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "msolve(A; B)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "MSv", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mSv", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Mt", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MT", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mT", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MVA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mVA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MVAR", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mVAR", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MW", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mW", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mWb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MWb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mWh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MWh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "mΩ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "MΩ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "M℧", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "m℧", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N*cm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N*mm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N/C", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N/cm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N/cm^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N/cm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N/m^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N/mm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N/mm^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "N/mm^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "n_cols(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "n_rows(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "nA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nBq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nC", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nF", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ng", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nGy", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nH", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nHz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nJ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nL", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Nm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nmi", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "norm(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "norm_1(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "norm_2(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "norm_e(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "norm_i(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "norm_p(v; p)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "not(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "nPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nS", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ns", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nSv", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nT", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nVA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nVAR", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nW", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nWb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nWh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "nΩ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "n℧", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "or(M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "order(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "order_cols(M; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "order_rows(M; j)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "osf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "osi", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "oz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "oz/ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "oz/in^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "oz_f", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "oz_f*ft", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "oz_f*in", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "oz_f/ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "oz_f/in^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ozf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Pa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pBq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pC", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pcm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "perch", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "perch^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "PeV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pF", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pGy", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pH", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "phase(z)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "pHz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pJ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pk", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pk_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pk_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pL", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "PlotHeight", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "PlotLightDir", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "PlotPalette", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "PlotShadows", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "PlotStep", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "PlotSmooth", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "PlotSVG", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "PlotWidth", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "pm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pole", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pole^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ppb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ppb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ppm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ppq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ppt", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Precision", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "product(M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "pS", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ps", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "psf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "psi", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pSv", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pt", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pT", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pt_dry", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pt_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pt_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pVA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pVAR", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pW", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pWb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pWh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "pΩ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "p℧", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "qr", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "qr(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "qt", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "qt_dry", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "qt_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "qt_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "quad", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "R", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "rad", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "random(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "range(x₁; xₙ; step)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "range_hp(x₁; xₙ; step)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "rank(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Rd", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "re(z)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "resize(v; n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "ReturnAngleUnits", Foreground = Brushes.Blue });
            items.Add(new ListBoxItem() { Content = "rev", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "reverse(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "revorder(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "revorder_cols(M; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "revorder_rows(M; j)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "rod", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "rod^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "rood", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "root(x; n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "round(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "row(M; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "rpm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "rsort(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "rsort_cols(M; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "rsort_rows(M; j)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "s", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "S", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "S/m", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "search(v; x; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "sec(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "sech(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "setUnits(x; u)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "sin(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "sinh(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "size(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "slice(v; i₁; i₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "slug", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "slug/ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "sort(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "sort_cols(M; i)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "sort_rows(M; j)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "spline(x; M; v; y…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "spline(x; y; M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "sqrt(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "srss(M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "st", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "stack(A; B; C…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "submatrix(M; i₁; i₂; j₁; j₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "sum(M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "sumsq(M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "Sv", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "svd(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "switch(c₁; v₁; c₂; v₂; …; def)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "symmetric(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "symmetric_hp(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "t", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "T", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "t/m^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "take(n; M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "take(x; y; M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "tan(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "tanh(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "TBq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TC", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TeV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TF", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "tf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "tf/m^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "tf/m^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TGy", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TH", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "th", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "th^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "th^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "therm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "THz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TJ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TN", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ton", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ton/ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ton/yd^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ton_f", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ton_f/ft^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ton_f/in^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ton_UK", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "ton_US", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "tonf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Torr", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "trace(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "transp(M)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "trunc(x)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "TS", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "tsf", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "tsi", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TSv", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TT", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TVA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TVAR", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TW", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TWb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TWh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "TΩ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "T℧", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "u", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "unit(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "utriang(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "utriang_hp(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "V", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "V*m", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "V/m", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "VA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "VAR", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "vec2col(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vec2diag(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vec2row(v)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vector(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vector_hp(n)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vlookup(M; x; j₁; j₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vlookup_eq(M; x; j₁; j₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vlookup_ge(M; x; j₁; j₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vlookup_gt(M; x; j₁; j₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vlookup_le(M; x; j₁; j₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vlookup_lt(M; x; j₁; j₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "vlookup_ne(M; x; j₁; j₂)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "W", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "w", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Wb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Wh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "xor(M; v; x…)", FontWeight = FontWeights.Bold });
            items.Add(new ListBoxItem() { Content = "y", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "yd", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "yd/s", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "yd^2", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "yd^3", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "°", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "°C", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "°F", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "°R", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Δ°C", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Δ°F", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Ω", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "Ω*m", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μbar", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μBq", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μC", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μF", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μg", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μGy", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μH", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μHz", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μJ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μL", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μm", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μPa", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μS", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μs", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μSv", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μT", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μV", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μVA", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μVAR", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μW", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μWb", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μWh", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μΩ", Foreground = Brushes.DarkCyan });
            items.Add(new ListBoxItem() { Content = "μ℧", Foreground = Brushes.DarkCyan });
        }

        internal void InitAutoComplete(string input, Paragraph currentParagraph)
        {
            var c = string.IsNullOrEmpty(input) ? '\0' : input[0];
            bool isAutoCompleteTrigger = Validator.IsLetter(c);
            if (!isAutoCompleteTrigger)
            {
                if (_listBox.Visibility == Visibility.Hidden)
                    isAutoCompleteTrigger = c == '#' || c == '$';
                else
                    isAutoCompleteTrigger = c == '/' || c == '*' || c == '^';
            }

            if (isAutoCompleteTrigger)
            {
                var tp = _richTextBox.Selection.Start;
                if (_listBox.Visibility == Visibility.Hidden)
                {
                    var p = tp.Paragraph;
                    var text = p is null ? String.Empty : new TextRange(p.ContentStart, tp).Text;
                    var i = text.Length - 1;
                    var c0 = i < 0 ? '\0' : text[i];
                    if (!Validator.IsLetter(c0))
                    {
                        if (i < 0 && currentParagraph is not null)
                            _autoCompleteStart = currentParagraph.ContentStart;
                        else
                            _autoCompleteStart = tp;

                        SetAutoCompletePosition();
                        FilterAutoComplete(c0, c.ToString());
                    }
                }
                else
                    UpdateAutoComplete(input);
            }
            else
                _listBox.Visibility = Visibility.Hidden;
        }

        internal void MoveAutoComplete()
        {
            if (_autoCompleteStart is null || _listBox.Visibility == Visibility.Hidden)
                return;

            var verticalAlignment = _listBox.VerticalAlignment;
            SetAutoCompletePosition();
            var r = _autoCompleteStart.GetCharacterRect(LogicalDirection.Forward);
            var y = r.Top + r.Height / 2;
            if (y < _richTextBox.Margin.Top || y > _richTextBox.Margin.Top + _richTextBox.ActualHeight)
            {
                _listBox.Visibility = Visibility.Hidden;
                return;
            }
            if (_listBox.VerticalAlignment != verticalAlignment)
                SortAutoComplete();
        }

        private void SetAutoCompletePosition()
        {
            var rect = _autoCompleteStart.GetCharacterRect(LogicalDirection.Forward);
            var x = _richTextBox.Margin.Left + rect.Left - 2;
            var y = _richTextBox.Margin.Top + rect.Bottom;
            if (y > _richTextBox.ActualHeight - _listBox.MaxHeight)
            {
                y = _richTextBox.Margin.Bottom + _richTextBox.ActualHeight - rect.Top;
                _listBox.Margin = new Thickness(x, 0, 0, y);
                _listBox.VerticalAlignment = VerticalAlignment.Bottom;
            }
            else
            {
                _listBox.Margin = new Thickness(x, y, 0, 0);
                _listBox.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        internal bool IsInComment()
        {
            var tp = _richTextBox.Selection.Start;
            var text = tp.GetTextInRun(LogicalDirection.Backward).AsSpan();
            var i = text.IndexOfAny(HighLighter.Comments);
            if (i < 0)
                return false;
            var c = text[i];
            i = text.Count(c);
            return (i % 2 == 1);
        }

        private void UpdateAutoComplete(string input)
        {
            var offset = _autoCompleteStart.GetOffsetToPosition(_richTextBox.Selection.Start);
            if (offset <= 0)
                _listBox.Visibility = Visibility.Hidden;
            else
            {
                string s = _autoCompleteStart.GetTextInRun(LogicalDirection.Backward);
                char c = string.IsNullOrEmpty(s) ? '\0' : s[0];
                s = new TextRange(_autoCompleteStart, _richTextBox.Selection.End).Text;
                if (input is null)
                {
                    if (s.Length > 1)
                        s = s[..^1];
                    else
                    {
                        _listBox.Visibility = Visibility.Hidden;
                        return;
                    }
                }
                else
                    s += input;

                _dispatcher.InvokeAsync(() => FilterAutoComplete(c, s), DispatcherPriority.Send);
            }
        }

        private void FilterAutoComplete(char c, string s)
        {
            if (s is null)
                _listBox.Items.Filter = null;
            else if (Validator.IsDigit(c))
                _listBox.Items.Filter =
                    x => ((string)((ListBoxItem)x).Content).StartsWith(s, StringComparison.OrdinalIgnoreCase) &&
                    ((ListBoxItem)x).Foreground == Brushes.DarkCyan;
            else
                _listBox.Items.Filter =
                    x => ((string)((ListBoxItem)x).Content).StartsWith(s, StringComparison.OrdinalIgnoreCase);

            if (_listBox.HasItems)
            {
                SortAutoComplete();
                _listBox.Visibility = Visibility.Visible;
            }
            else
                _listBox.Visibility = Visibility.Hidden;
        }

        private void SortAutoComplete()
        {
            _listBox.Items.SortDescriptions.Clear();
            if (_listBox.VerticalAlignment == VerticalAlignment.Bottom)
            {
                _listBox.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Descending));
                _listBox.SelectedIndex = _listBox.Items.Count - 1;
            }
            else
            {
                _listBox.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));
                _listBox.SelectedIndex = 0;
            }
            _listBox.ScrollIntoView(_listBox.SelectedItem);
        }

        private void EndAutoComplete()
        {
            var selectedItem = (ListBoxItem)_listBox.SelectedItem;
            string s = (string)selectedItem.Content;
            var items = _listBox.Items;
            var index = items.IndexOf(selectedItem);
            if (index < items.Count - 1)
            {
                var nextItem = (ListBoxItem)items[index + 1];
                if (selectedItem.Foreground == Brushes.DarkCyan &&
                    nextItem.Foreground == Brushes.Blue &&
                    string.Equals((string)nextItem.Content, s, StringComparison.Ordinal))
                    s = "." + s;
            }
            var sel = _richTextBox.Selection;
            var selEnd = sel.End;
            var r = new TextRange(_autoCompleteStart, selEnd);
            r.Text = s;
            _listBox.Visibility = Visibility.Hidden;
            sel.Select(r.Start, r.End);
            sel.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
            _insertManager.SelectInsertedText(s);
            _richTextBox.Focus();
        }

        internal void RestoreAutoComplete()
        {
            var text = _richTextBox.CaretPosition.GetTextInRun(LogicalDirection.Backward);
            var n = text.Length - 1;
            for (int i = n; i >= 0; --i)
            {
                n = i;
                if (!Validator.IsLetter(text[i]))
                    break;

                --n;
            }
            if (n < text.Length - 1)
                Task.Run(
                    () => _dispatcher.InvokeAsync(() =>
                    {
                        text = text[(n + 1)..];
                        _autoCompleteStart = _richTextBox.CaretPosition.GetPositionAtOffset(-text.Length);
                        SetAutoCompletePosition();
                        FilterAutoComplete(text[^1], text);
                    }, DispatcherPriority.Send)
                );
        }


        private void AutoCompleteListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ListBoxItem)
                EndAutoComplete();
        }

        private void AutoCompleteListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape)
                _richTextBox.Focus();
            else if (
                e.Key is
                not Key.PageUp and
                not Key.PageDown and
                not Key.End and
                not Key.Home and
                not Key.Left and
                not Key.Up and
                not Key.Right and
                not Key.Down and
                not Key.LeftShift and
                not Key.RightShift and
                not Key.LeftCtrl and
                not Key.RightCtrl and
                not Key.LeftAlt and
                not Key.RightAlt
            )
            {
                e.Handled = true;
                EndAutoComplete();
            }
        }

        internal void PreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.PageUp:
                case Key.PageDown:
                case Key.Home:
                case Key.End:
                case Key.Delete:
                case Key.Enter:
                case Key.Space:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftAlt:
                case Key.RightAlt:
                    _listBox.Visibility = Visibility.Hidden;
                    return;
                case Key.Up:
                case Key.Down:
                    if (e.Key == Key.Down ^ _listBox.VerticalAlignment == VerticalAlignment.Bottom)
                    {
                        _listBox.Focus();
                        e.Handled = true;
                    }
                    else
                        _listBox.Visibility = Visibility.Hidden;

                    ((ListBoxItem)_listBox.SelectedItem).Focus();
                    return;
                case Key.Back:
                    UpdateAutoComplete(null);
                    return;
                case Key.Tab:
                    EndAutoComplete();
                    e.Handled = true;
                    return;
            }
        }

        internal void FillAutoComplete(UserDefined defs, int currentLineNumber)
        {
            _listBox.Items.Filter = null;
            _listBox.Items.SortDescriptions.Clear();
            var items = _listBox.Items;
            for (int i = items.Count - 1; i >= _autoCompleteCount; --i)
                items.RemoveAt(i);

            try
            {
                FillDefined(defs.Variables, defs.MacroProcedures, Brushes.Blue, currentLineNumber);
                FillDefined(defs.FunctionDefs, defs.MacroProcedures, Brushes.Black, currentLineNumber);
                FillDefined(defs.Units, defs.MacroProcedures, Brushes.DarkCyan, currentLineNumber);
                FillDefined(defs.Macros, defs.MacroProcedures, Brushes.DarkMagenta, currentLineNumber);
                foreach (var kvp in defs.MacroParameters)
                {
                    var bounds = kvp.Value;
                    if (bounds[0] < currentLineNumber && currentLineNumber < bounds[1])
                        items.Add(new ListBoxItem()
                        {
                            Content = kvp.Key,
                            Foreground = Brushes.DarkMagenta
                        });
                }
            }
            catch { }

            void FillDefined(IEnumerable<KeyValuePair<string, int>> defs, Dictionary<string, string> macros, Brush foreground, int currentLineNumber)
            {
                foreach (var kvp in defs)
                {
                    var line = kvp.Value;
                    if (line < currentLineNumber && !IsPlot(kvp.Key))
                    {
                        var s = kvp.Key;
                        if (s[^1] == '$' && macros.TryGetValue(s, out var proc))
                            s += proc;

                        var item = new ListBoxItem()
                        {
                            Content = s
                        };
                        if (foreground == Brushes.Black)
                            item.FontWeight = FontWeights.Bold;
                        else
                            item.Foreground = foreground;

                        items.Add(item);
                    }
                }
            }

            bool IsPlot(string s) => s[0] == 'P' &&
                (s.Equals("PlotWidth", StringComparison.Ordinal) ||
                 s.Equals("PlotHeight", StringComparison.Ordinal) ||
                 s.Equals("PlotStep", StringComparison.Ordinal) ||
                 s.Equals("PlotSVG", StringComparison.Ordinal) ||
                 s.Equals("PlotPalette", StringComparison.Ordinal) ||
                 s.Equals("PlotShadows", StringComparison.Ordinal) ||
                 s.Equals("PlotSmooth", StringComparison.Ordinal) ||
                 s.Equals("PlotLightDir", StringComparison.Ordinal)
            );
        }
    }
}