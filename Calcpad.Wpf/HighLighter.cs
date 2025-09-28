using Calcpad.Core;
using System;
using System.Buffers;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static Calcpad.Wpf.MainWindow;

namespace Calcpad.Wpf
{
    internal class HighLighter
    {
        internal UserDefined Defined = new();
        private sealed class TagHelper
        {
            internal enum Tags
            {
                None,
                Starting,
                Closing,
                SelfClosing
            }

            internal Tags Type { get; private set; } = Tags.None;

            internal void Initialize()
            {
                Type = Tags.None;
            }
            internal bool CheckTag(char c, StringBuilder _builder, ref Types t)
            {
                if (t == Types.Comment)
                {
                    if (Type == Tags.None)
                    {
                        if (c == ' ')
                        {
                            if (_builder.Length == 1)
                                return false;
                            else
                                Type = Tags.Starting;
                        }
                        else if (c == '/')
                        {
                            Type = _builder.Length == 1 ? Tags.Closing : Tags.SelfClosing;
                        }
                        else if (!(Validator.IsLatinLetter(c) || char.IsDigit(c) && _builder.Length == 2 && _builder[1] == 'h'))
                            return false;
                    }
                    else
                        t = Types.Tag;
                }
                else if (Type == Tags.SelfClosing || Type == Tags.Closing && !Validator.IsLatinLetter(c))
                    return false;

                return true;
            }
        }

        private struct ParserState
        {
            internal Paragraph Paragraph;
            internal int Line;
            internal string Text;
            internal string Message;
            internal char TextComment;
            internal char TagComment;
            internal char InputChar { get; private set; }
            internal bool IsSubscript;
            internal bool IsLeading;
            internal bool IsUnits;
            internal bool IsPlot;
            internal bool IsTag;
            internal bool IsTagComment;
            internal bool IsMacro;
            internal bool IsFunction;
            internal string Keyword;
            internal bool HasMacro;
            internal bool Redefine;
            internal int MacroArgs;
            internal int CommandCount;
            internal int BracketCount;
            internal int MatrixCount;
            internal Types CurrentType;
            internal Types PreviousType;

            internal void RetainType()
            {
                if (CurrentType != Types.None)
                    PreviousType = CurrentType;
            }

            internal Types PreviousTypeIfCurrentIsNone =>
                CurrentType == Types.None ?
                PreviousType :
                CurrentType;

            internal void GetInputState(char c)
            {
                if (c == '?' || c == '#' && !IsLeading)
                    InputChar = c;
                else if (c == '{')
                {
                    if (InputChar == '?' || InputChar == '#')
                        InputChar = '{';
                    else
                        InputChar = '\0';
                }
                else if (c == '}')
                {
                    InputChar = InputChar == '{' ? '}' : '\0';
                }
                else if (c != ' ' && InputChar != '{')
                     InputChar = '\0';
            }
        }

        internal static MouseButtonEventHandler IncludeClickEventHandler;
        internal static readonly HashSet<string> LocalVariables = new(StringComparer.Ordinal);
        internal static readonly Brush KeywordBrush = Brushes.Magenta;
        internal static readonly SearchValues<char> Comments = SearchValues.Create("'\"");
        internal bool All;

        private static readonly Thickness _thickness = new(0.5);
        private bool _allowUnaryMinus = true;
        private TagHelper _tagHelper;
        private ParserState _state;
        private readonly StringBuilder _builder = new(100);

        private enum Types
        {
            None,
            Const,
            Units,
            Operator,
            Variable,
            Function,
            Keyword,
            Command,
            Bracket,
            Comment,
            Tag,
            Input,
            Include,
            Macro,
            HtmlComment,
            Format,
            Error
        }

        private static readonly Thickness ToolTipPadding = new(3, 1, 3, 2);
        private static readonly SolidColorBrush ToolTipBackground = new(Color.FromArgb(196, 0, 0, 0));
        private static readonly SolidColorBrush TitleBackground = new(Color.FromRgb(245, 255, 240));
        private static readonly SolidColorBrush ErrorBackground = new(Color.FromRgb(255, 225, 225));
        private static readonly SolidColorBrush BackgroundBrush = new(Color.FromArgb(160, 240, 248, 255));
        private static readonly SolidColorBrush HtmlCommentBrush = new(Color.FromRgb(160, 160, 160));


        private static readonly Brush[] Colors =
        [
             Brushes.Gray,
             Brushes.Black,
             Brushes.DarkCyan,
             Brushes.Goldenrod,
             Brushes.Blue,
             Brushes.Black,
             KeywordBrush,
             Brushes.Magenta,
             Brushes.DeepPink,
             Brushes.ForestGreen,
             Brushes.DarkOrchid,
             Brushes.Red,
             Brushes.Indigo,
             Brushes.DarkMagenta,
             HtmlCommentBrush,
             Brushes.DarkGray,
             Brushes.Crimson
        ];

        private static readonly FrozenSet<char> Operators = new HashSet<char>() { '!', '^', '/', '÷', '\\', '⦼', '*', '-', '+', '<', '>', '≤', '≥', '≡', '≠', '=', '∧', '∨', '⊕', '∠' }.ToFrozenSet();
        private static readonly FrozenSet<char> Delimiters = new HashSet<char>() { ';', '|', '&', '@', ':' }.ToFrozenSet();
        private static readonly FrozenSet<char> Brackets = new HashSet<char>() { '(', ')', '{', '}', '[', ']' }.ToFrozenSet();

        private static readonly FrozenSet<string> Functions =
        new HashSet<string>()
        {
            "abs",
            "mod",
            "gcd",
            "lcm",
            "sin",
            "cos",
            "tan",
            "csc",
            "sec",
            "cot",
            "asin",
            "acos",
            "atan",
            "atan2",
            "acsc",
            "asec",
            "acot",
            "sinh",
            "cosh",
            "tanh",
            "csch",
            "sech",
            "coth",
            "asinh",
            "acosh",
            "atanh",
            "acsch",
            "asech",
            "acoth",
            "log",
            "ln",
            "log_2",
            "exp",
            "sqr",
            "sqrt",
            "cbrt",
            "conj",
            "root",
            "round",
            "floor",
            "ceiling",
            "trunc",
            "sign",
            MathParser.NegateString,
            "re",
            "im",
            "phase",
            "conj",
            "min",
            "max",
            "sum",
            "sumsq",
            "srss",
            "product",
            "average",
            "mean",
            "if",
            "switch",
            "take",
            "line",
            "spline",
            "random",
            "not",
            "and",
            "or",
            "xor",
            "timer",
            "hp",
            "ishp",
            "vector",
            "vector_hp",
            "len",
            "size",
            "fill",
            "range",
            "range_hp",
            "join",
            "resize",
            "first",
            "last",
            "slice",
            "sort",
            "rsort",
            "order",
            "revorder",
            "reverse",
            "extract",
            "search",
            "count",
            "find",
            "find_eq",
            "find_ne",
            "find_lt",
            "find_gt",
            "find_le",
            "find_ge",
            "lookup",
            "Lookup_eq",
            "Lookup_ne",
            "Lookup_lt",
            "Lookup_gt",
            "Lookup_le",
            "Lookup_ge",
            "norm",
            "norm_1",
            "norm_2",
            "norm_e",
            "norm_i",
            "norm_p",
            "unit",
            "dot",
            "cross",
            "matrix",
            "identity",
            "diagonal",
            "column",
            "utriang",
            "ltriang",
            "symmetric",
            "vec2diag",
            "diag2vec",
            "vec2col",
            "vec2row",
            "matrix_hp",
            "identity_hp",
            "diagonal_hp",
            "column_hp",
            "utriang_hp",
            "ltriang_hp",
            "symmetric_hp",
            "join_cols",
            "join_rows",
            "augment",
            "stack",
            "mfill",
            "fill_row",
            "fill_col",
            "mresize",
            "copy",
            "add",
            "n_rows",
            "n_cols",
            "row",
            "col",
            "extract_rows",
            "extract_cols",
            "submatrix",
            "mnorm",
            "mnorm_2",
            "mnorm_e",
            "mnorm_1",
            "mnorm_i",
            "cond",
            "cond_1",
            "cond_2",
            "cond_e",
            "cond_i",
            "det",
            "rank",
            "transp",
            "trace",
            "inverse",
            "adj",
            "cofactor",
            "eigenvals",
            "eigenvecs",
            "eigen",
            "lu",
            "qr",
            "svd",
            "cholesky",
            "lsolve",
            "clsolve",
            "slsolve",
            "msolve",
            "cmsolve",
            "smsolve",
            "hprod",
            "fprod",
            "kprod",
            "sort_cols",
            "rsort_cols",
            "sort_rows",
            "rsort_rows",
            "order_cols",
            "revorder_cols",
            "order_rows",
            "revorder_rows",
            "mcount",
            "mfind",
            "mfind_eq",
            "mfind_ne",
            "mfind_lt",
            "mfind_le",
            "mfind_gt",
            "mfind_ge",
            "msearch",
            "hlookup",
            "hlookup_eq",
            "hlookup_ne",
            "hlookup_lt",
            "hlookup_le",
            "hlookup_gt",
            "hlookup_ge",
            "vlookup",
            "vlookup_eq",
            "vlookup_ne",
            "vlookup_lt",
            "vlookup_le",
            "vlookup_gt",
            "vlookup_ge",
            "getunits",
            "setunits",
            "clrunits",
            "fft",
            "ift"
       }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenSet<string> Keywords =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "#if",
            "#else",
            "#else if",
            "#end if",
            "#rad",
            "#deg",
            "#gra",
            "#val",
            "#equ",
            "#noc",
            "#round",
            "#format",
            "#show",
            "#hide",
            "#varsub",
            "#nosub",
            "#novar",
            "#split",
            "#wrap",
            "#pre",
            "#post",
            "#repeat",
            "#for",
            "#while",
            "#loop",
            "#break",
            "#continue",
            "#include",
            "#local",
            "#global",
            "#def",
            "#end def",
            "#pause",
            "#input",
            "#md",
            "#read",
            "#write",
            "#append",
            "#phasor",
            "#complex",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenSet<string> SingleExpressionKeywords =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "#if",
            "#else if",
            "#repeat",
            "#for",
            "#round",
            "#format",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenSet<string> CompoundExpressionKeywords =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "#while",
            "#def",
            "#include",
            "#read",
            "#write",
            "#append",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenSet<string> DataExchangeKeywords =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "#read",
            "#write",
            "#append",
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        private static readonly FrozenSet<string> Commands =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "$find",
            "$root",
            "$sup",
            "$inf",
            "$area",
            "$integral",
            "$slope",
            "$repeat",
            "$sum",
            "$product",
            "$plot",
            "$map"
        }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

        internal static void Clear(Paragraph p)
        {
            if (p is null)
                return;

            foreach (var inline in p.Inlines)
            {
                inline.Background = null;
                inline.ToolTip = null;
                if (inline.Cursor == Cursors.Hand)
                {
                    inline.Cursor = null;
                    inline.TextDecorations.Clear();
                    inline.MouseLeftButtonDown -= IncludeClickEventHandler;
                    inline.MouseLeftButtonDown -= File_Click;
                }
            }
            p.Background = BackgroundBrush;
            p.BorderBrush = Brushes.LightBlue;
            p.BorderThickness = _thickness;
        }

        private bool IsFunction(string s, int line) => Defined.IsFunction(s, line) || Functions.Contains(s);
        private bool IsVariable(string s, int line) => LocalVariables.Contains(s) || Defined.IsVariable(s, line);
        private bool IsUnit(string s, int line) => Defined.IsUnit(s, line) || MathParser.IsUnit(s);

        private static void InitLocalValraibles(Paragraph p)
        {
            LocalVariables.Clear();
            var b = p;
            var isCollect = false;
            Run run = null;
            while (b is not null)
            {
                if (isCollect)
                    GetLocalVariables(run, false);
                else
                {
                    foreach (var inline in p.Inlines)
                    {
                        if (inline is not Run r)
                            continue;

                        var s = r.Text.AsSpan().Trim();
                        if (!s.IsEmpty && s[0] != '\'' && s.Contains('='))
                        {
                            isCollect = true;
                            if (!ReferenceEquals(b, p))
                                GetLocalVariables(r, false);

                            break;
                        }
                    }
                }

                b = b.PreviousBlock as Paragraph;
                if (b is null)
                    return;

                run = b.Inlines.LastInline as Run;
                if (run is null || !run.Text.AsSpan().EndsWith("_"))
                    return;
            }
        }

        internal void Parse(Paragraph p, bool isComplex, int line, string text = null)
        {
            if (p is null)
                return;

            InitParagraph(p);
            text ??= new TextRange(p.ContentStart, p.ContentEnd).Text;
            if (text.Length > 10000)
                return;

            InitLocalValraibles(p);
            p.Inlines.Clear();
            InitState(p, text, line);
            _tagHelper = new();
            _allowUnaryMinus = true;
            _builder.Clear();
            var skip = 0;
            for (int i = 0, len = text.Length; i < len; ++i)
            {
                if (i < skip)
                    continue;

                var c = text[i];
                if (_state.CurrentType != Types.Comment)
                {
                    if (char.IsWhiteSpace(c))
                        c = ' ';
                    else if (c == '·')
                        c = '*';
                }
                if (i == len - 1 && 
                    (i > 0 && text[i - 1] == ' ' || _builder.Length == 0) && 
                    ParseLineBreak(c))
                    break;

                if (_state.CurrentType == Types.Format && !Comments.Contains(c))
                {
                    _builder.Append(c);
                    continue;
                }
                _state.GetInputState(c);
                if (!_state.IsPlot &&
                    _state.MatrixCount == 0 &&
                    _state.CurrentType != Types.Comment &&
                    c == '|')
                    _state.IsUnits = true;

                if (_state.MacroArgs == 0)
                    ParseComment(c);

                if (_state.MacroArgs > 0)
                    ParseMacroArgs(c);
                else if (_state.TextComment != '\0')
                    ParseTagInComment(c);
                else if (_state.CurrentType == Types.Include)
                {
                    if (c == '#')
                    {
                        Append(Types.Include);
                        _state.CurrentType = Types.Bracket;
                    }
                    _builder.Append(c);
                }
                else if (c == '$' && _builder.Length > 0)
                    ParseMacro();
                else if (IsDoubleOp(c, '%'))
                    AppendDoubleOperatorShortcut('⦼');
                else if (IsDoubleOp(c, '<'))
                    AppendDoubleOperatorShortcut('∠');
                else if (Validator.IsWhiteSpace(c))
                    ParseSpace(c);
                else if (Brackets.Contains(c))
                    ParseBrackets(c);
                else if (Operators.Contains(c))
                {
                    if (c == '<' && i < len - 1 && text[i + 1] == '<')
                        _builder.Append('<');
                    else
                    {
                        ParseOperator(c);
                        if (ParseMacroContent(c, i, len))
                            return;
                    }
                }
                else if (Delimiters.Contains(c))
                    ParseDelimiter(c);
                else if (_builder.Length == 0)
                {
                    _state.CurrentType = InitType(c, _state.CurrentType);
                    if (_state.CurrentType != Types.Comment)
                        _builder.Append(c);

                    if (_state.CurrentType == Types.Input)
                        Append(Types.Input);
                }
                else if (isComplex && _state.CurrentType == Types.Const && c == 'i')
                    ParseImaginary(c, i, len);
                else if (_state.CurrentType == Types.Const && Validator.IsLetter(c))
                    ParseUnits(c);
                else if (_state.CurrentType == Types.Variable && c == '.' &&
                    Defined.IsVariable(_builder.ToString(), line))
                {
                    Append(Types.Variable);
                    _builder.Append(c);
                    _state.CurrentType = Types.Operator;
                    Append(Types.Operator);
                }
                else
                {
                    if (IsParseError(c, _state.CurrentType))
                    {
                        _state.Message = string.Format(FindReplaceResources.Invalid_character_0, c);
                        _state.CurrentType = Types.Error;
                    }
                    else if (_state.CurrentType == Types.None && _builder.Length > 0)
                    {
                        _state.CurrentType = _state.PreviousType;
                        Append(_state.CurrentType);
                        if (_state.CurrentType == Types.Variable &&
                            IsDataExchangeKeyword && p.Inlines.Count == 4)
                        {
                            skip = text.LastIndexOf('.');
                            if (skip < 0)
                            {
                                _builder.Append(text[i..]);
                                Append(Types.Error);
                                break;
                            }
                            skip = text.IndexOfAny([' ', '@', '!'], skip + 1);
                            if (skip < 0)
                                skip = len;
                            var fileName = text[i..skip];
                            _builder.Append(fileName);
                            var filePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(fileName));
                            if (File.Exists(filePath) || _state.Keyword != "#read")
                            {
                                _state.Message = AppMessages.ClickToOpenFile;
                                Append(Types.Macro);
                            }
                            else
                            {
                                _state.Message = AppMessages.File_not_found;
                                Append(Types.Error);
                            }
                            if (skip < len && text[skip] == ' ')
                                ++skip;
                            _state.CurrentType = Types.None;
                            continue;
                        }
                        _state.CurrentType = InitType(c, _state.CurrentType);
                    }
                    _builder.Append(c);
                }
                if (!string.IsNullOrEmpty(_state.Keyword))
                {
                    var isSingleExpressionKeyword = SingleExpressionKeywords.Contains(_state.Keyword);
                    var isEndOfLineLineKeyword = !CompoundExpressionKeywords.Contains(_state.Keyword);
                    if ((isSingleExpressionKeyword || isEndOfLineLineKeyword) &&
                        (c == '\'' || c == '"'))
                    {
                        _builder.Append(text[++i..]);
                        _state.Message = isEndOfLineLineKeyword ?
                            AppMessages.End_of_line_expected :
                            AppMessages.Single_expression_expected;
                        Append(Types.Error);
                        return;
                    }
                    if (_state.Keyword == "#format")
                        _state.CurrentType = Types.Format;
                }
                _state.RetainType();
                if (!Validator.IsWhiteSpace(c))
                    _state.IsLeading = false;

                if (_state.CurrentType == Types.Units || _state.CurrentType == Types.Variable)
                {
                    if (c == '_')
                        _state.IsSubscript = true;
                }
                else
                    _state.IsSubscript = false;
            }
            _state.CurrentType = _state.PreviousTypeIfCurrentIsNone;
            if (_state.CurrentType == Types.Comment)
                CheckHtmlComment();

            Append(_state.CurrentType);
            if (_state.Redefine)
            {
                text = new TextRange(p.ContentStart, p.ContentEnd).Text;
                Defined.Get(text.AsSpan(), line);
                Parse(p, isComplex, line, text);
            }
            if (!All)
            {
                var r = p.Inlines.LastInline as Run;
                if (r is not null && r.Text.AsSpan().EndsWith(" _"))
                {
                    var next = p.NextBlock as Paragraph;
                    Parse(next, isComplex, line + 1);
                }
            }
        }

        private bool IsDoubleOp(char c, char op) =>
            (c == op && _builder.Length > 0 && _builder[^1] == op);
        private static void InitParagraph(Paragraph p)
        {
            p.Background = null;
            p.BorderBrush = null;
            p.BorderThickness = _thickness;
        }

        private void InitState(Paragraph p, string text, int line)
        {
            _state = new()
            {
                Paragraph = p,
                Line = line,
                Text = text,
                IsLeading = true,
                IsPlot = Validator.IsPlot(text)
            };
            ContinuePrevLineState(p);
        }

        private void ContinuePrevLineState(Paragraph p)
        {
            if (p.PreviousBlock is Paragraph para && para.Inlines.LastInline is Run r)
            {
                var span = r.Text.AsSpan().TrimEnd();
                if (!span.IsEmpty && span[^1] == '_')
                {
                    var type = GetTypeFromColor(r.Foreground);
                    if (type != Types.Variable)
                        _state.CurrentType = type;

                    _state.TextComment = r.Tag is char c ? c : '\0';
                }
            }
        }

        internal static string GetCSSClassFromColor(Brush color) =>
            GetTypeFromColor(color).ToString().ToLowerInvariant();

        private static Types GetTypeFromColor(Brush color)
        {
            for (int i = 0, len = Colors.Length; i < len; ++i)
                if (Colors[i] == color)
                    return (Types)i;

            return Types.None;
        }

        private bool ParseLineBreak(char c)
        {
            if (c != '_')
                return false;

            var p = _state.Paragraph;
            if (_builder.Length == 0)
            {
                var inline = p.Inlines.LastInline;
                if (inline is null || inline is Run run && run.Text.EndsWith(' '))
                    c = ' ';
            }
            else
            {
                c = _builder[^1];
                Append(_state.CurrentType);
            }
            if (c == ' ')
                _builder.Append('_');
            else
                _builder.Append(" _");

            var isComment = _state.CurrentType == Types.Comment || _state.CurrentType == Types.HtmlComment;
            if (isComment)
                Append(_state.CurrentType);
            else
                Append(Types.None);

            var r = p.Inlines.LastInline as Run;
            if (r is not null && r.Text.AsSpan().EndsWith("_"))
                r.Tag = isComment ? _state.TextComment : null;

            return true;
        }

        private void ParseComment(char c)
        {
            if (c >= 'А' && c <= 'я' && _state.CurrentType != Types.Include)
            {
                if (_state.TextComment == '\0' && !_state.IsSubscript)
                {
                    Append(_state.CurrentType);
                    _state.TextComment = _state.Paragraph.PreviousBlock is null ? '"' : '\'';
                    _builder.Clear();
                    _builder.Append(_state.TextComment);
                    _state.CurrentType = Types.Comment;
                    _state.Redefine = true;
                }
            }
            else if (c == '\'' || c == '"')
            {
                if (_state.TextComment == '\0')
                {
                    _state.IsUnits = false;
                    _state.TextComment = c;
                    _state.TagComment = c == '\'' ? '"' : '\'';
                    Append(_state.CurrentType);
                    if (_state.IsTag)
                    {
                        if (_tagHelper.Type == TagHelper.Tags.Starting)
                            _state.CurrentType = Types.Tag;
                        else
                        {
                            _state.CurrentType = Types.Comment;
                            _state.IsTag = false;
                        }
                    }
                    else
                        _state.CurrentType = Types.Comment;
                }
                else if (c == _state.TextComment)
                {
                    _state.TextComment = '\0';
                    _state.CurrentType = Types.Comment;
                    CheckHtmlComment();

                    _builder.Append(c);
                    Append(_state.CurrentType);
                    _state.CurrentType = Types.Comment;
                }
                else if (_state.IsTag)
                {
                    if (_state.IsTagComment)
                        _state.CurrentType = Types.Comment;
                    else
                        _builder.Append(c);

                    Append(_state.CurrentType);
                    if (_state.IsTagComment && c == _state.TagComment)
                    {
                        _builder.Append(c);
                        Append(Types.Tag);
                    }
                    _state.IsTagComment = !_state.IsTagComment;
                }
            }
        }

        private bool IsParseError(char c, Types t) =>
                t == Types.Const && !Validator.IsDigit(c) ||
                t == Types.Macro && !Validator.IsMacroLetter(c, _builder.Length) ||
                t == Types.Variable && !(Validator.IsVarChar(c) || _state.IsSubscript && char.IsLetter(c));

        private void ParseMacroInComment(Types t)
        {
            var len = _builder.Length;
            int i;
            for (i = len - 2; i >= 0; --i)
                if (!Validator.IsMacroLetter(_builder[i], i))
                    break;

            var s = _builder.ToString();
            for (int j = i + 1; j < len - 1; ++j)
            {
                if (Defined.IsMacroOrParameter(s[j..], _state.Line))
                {
                    _builder.Remove(j, len - j);
                    Append(t);
                    _builder.Append(s[j..]);
                    Append(Types.Macro);
                    _state.CurrentType = Types.Macro;
                    break;
                }
            }
        }

        private void ParseMacroArgs(char c)
        {
            _state.CurrentType = Types.None;
            if (c == '$' && _builder.Length > 0)
            {
                _builder.Append('$');
                ParseMacroInComment(_state.CurrentType);
            }
            else if (c == '(' || c == ')')
            {
                if (c == '(')
                    ++_state.BracketCount;
                else
                {
                    if (_state.BracketCount <= _state.MacroArgs)
                        _state.MacroArgs = 0;

                    --_state.BracketCount;
                }
                Append(_state.CurrentType);
                _builder.Append(c);
                _state.CurrentType = Types.Bracket;
                Append(_state.CurrentType);
                if (_state.TextComment != '\0' && c == ')' && _state.BracketCount == 0)
                    _state.CurrentType = Types.Comment;
            }
            else if (c == ';')
            {
                Append(_state.CurrentType);
                _builder.Append(c);
                _state.CurrentType = Types.Operator;
                Append(_state.CurrentType);
            }
            else if (!(_state.HasMacro && c == ' '))
                _builder.Append(c);

            _state.PreviousType = _state.CurrentType;
        }

        private void ParseTagInComment(char c)
        {
            if (c == '>')
            {
                _builder.Append(c);
                if (_state.IsTag)
                    _state.CurrentType = Types.Tag;
                else
                    CheckHtmlComment();

                Append(_state.CurrentType);
                _state.CurrentType = Types.Comment;
                _state.IsTag = false;
            }
            else if (c == '<')
            {
                _tagHelper.Initialize();
                Append(_state.CurrentType);
                _builder.Append(c);
                _state.IsTag = true;
            }
            else
            {
                if (_state.IsTag)
                    _state.IsTag = _tagHelper.CheckTag(c, _builder, ref _state.CurrentType);

                if (!(_state.IsTag && c == _state.TagComment))
                {
                    if (_state.CurrentType == Types.Macro)
                    {
                        if (c == '(' || c == ')')
                            ParseBrackets(c);
                        else
                            _builder.Append(c);

                        if (c != '(')
                            _state.CurrentType = Types.Comment;
                    }
                    else
                        _builder.Append(c);

                    if (c == '$' && Defined.Macros.Count > 0)
                        ParseMacroInComment(Types.Comment);
                }
            }
        }

        private void ParseMacro()
        {
            _builder.Append('$');
            if (_state.IsMacro)
            {
                if (_state.CurrentType != Types.Macro)
                    _state.CurrentType = Types.Error;
            }
            else if (_state.CurrentType != Types.Variable && _state.CurrentType != Types.Units)
                _state.CurrentType = Types.Error;
            else if (IsValidMacroName())
                _state.CurrentType = Types.Macro;
            else
            {
                _state.Message = Calcpad.Wpf.AppMessages.InvalidMacroName;
                _state.CurrentType = Types.Error;
            }
            Append(_state.CurrentType);
            if (_state.IsMacro)
                _state.HasMacro = true;

            _state.IsMacro = false;
        }


        private bool IsValidMacroName()
        {
            var len = _builder.Length;
            for (int i = len - 2; i >= 0; --i)
            {
                if (!Validator.IsMacroLetter(_builder[i], i))
                    return false;

                var s = _builder.ToString(i, len - i);
                if (Defined.IsMacroOrParameter(s, _state.Line))
                {
                    _builder.Remove(i, len - i);
                    Append(_state.CurrentType);
                    _builder.Append(s);
                    return true;
                }
            }
            return true;
        }

        private void ParseSpace(char c)
        {
            if (IsContinuedCondition(_builder.ToString()))
                _builder.Append(' ');
            else
            {
                if (_builder.Length == 4 &&
                     _builder[1] == 'd' &&
                     _builder[2] == 'e' &&
                     _builder[3] == 'f')
                    _state.IsMacro = true;
                var isInclude = false;
                //Spaces are added only if they are leading
                if (_state.IsLeading ||
                    _state.CurrentType == Types.Keyword ||
                    _state.CurrentType == Types.Error)
                {
                    isInclude = _builder.Length == 8 &&
                        _builder[1] == 'i' &&
                        _builder[2] == 'n' &&
                        _builder[3] == 'c';
                    Append(_state.CurrentType);
                    _builder.Append(c);
                    var t = _state.CurrentType == Types.Error ?
                        Types.Error :
                        Types.None;
                    Append(t);
                }
                _state.CurrentType = isInclude ? Types.Include : Types.None;
            }

            static bool IsContinuedCondition(string text) =>
                text.Equals("#else", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("#end", StringComparison.OrdinalIgnoreCase);
        }

        private void ParseBrackets(char c)
        {
            var t = _state.PreviousTypeIfCurrentIsNone;
            if (c == '(')
            {
                ++_state.BracketCount;
                if (t == Types.Variable)
                    _state.CurrentType = Types.Function;
                else if (t == Types.Macro && _state.MacroArgs == 0)
                {
                    _state.CurrentType = Types.Macro;
                    _state.MacroArgs = _state.BracketCount;
                }
                else if (t != Types.Operator && t != Types.Bracket && t != Types.None)
                    _state.CurrentType = Types.Error;
            }
            else if (c == ')')
            {
                if (_state.BracketCount == _state.MacroArgs)
                    _state.MacroArgs = 0;

                --_state.BracketCount;
            }
            Append(_state.CurrentType);
            if (_state.InputChar == c)
                _state.CurrentType = Types.Bracket;
            else if (c == '{')
            {
                if (_state.CurrentType == Types.Command)
                    ++_state.CommandCount;

                _state.CurrentType = _state.CommandCount > 0 ?
                    Types.Bracket : Types.Error;
            }
            else if (c == '}')
            {
                --_state.CommandCount;
                _state.CurrentType = _state.CommandCount >= 0 ? Types.Bracket : Types.Error;
            }
            else
            {
                if (c == '[')
                    ++_state.MatrixCount;
                else if (c == ']')
                    --_state.MatrixCount;

                _state.CurrentType = Types.Bracket;
            }

            _builder.Append(c);
            Append(_state.CurrentType);
        }

        private void ParseOperator(char c)
        {
            var len = _builder.Length;
            var isPercent = c == '%' && (len > 0 && _builder[len - 1] == '.' || _state.IsUnits);
            Append(_state.PreviousTypeIfCurrentIsNone);
            if (IsDataExchangeKeyword && c == '=')
            { 
                AppendRun(Types.Operator, "=");
                _state.CurrentType = Types.Operator;
                return;
            }
            _builder.Append(c);
            Append(isPercent ? Types.Units : Types.Operator);
            _state.CurrentType = Types.Operator;
            if (c == '=')
            {
                var lastInline = _state.Paragraph.Inlines.LastInline;
                if (lastInline.PreviousInline is Run run && run.Text == ")")
                    _state.IsFunction = true;

                GetLocalVariables(lastInline, _state.CommandCount > 0);
            }
        }

        private bool ParseMacroContent(char c, int i, int len)
        {
            if (c == '=' && _state.HasMacro)
            {
                ++i;
                if (i < len)
                {
                    for (int j = i; j < len; ++j)
                    {
                        char ct = _state.Text[j];
                        if (ct == '$' && _builder.Length > 0)
                        {
                            _builder.Append('$');
                            ParseMacroInComment(Types.None);
                        }
                        else if (j > i || ct != ' ')
                            _builder.Append(ct);
                    }
                    Append(Types.None);
                }
                return true;
            }
            return false;
        }

        private void ParseDelimiter(char c)
        {
            Append(_state.PreviousTypeIfCurrentIsNone);
            _builder.Append(c);
            if (_state.CommandCount > 0 || c == ';')
                Append(Types.Operator);
            else if (c == '|' && (_state.IsUnits || _state.MatrixCount > 0))
                Append(Types.Bracket);
            else if (c == ':')
            {
                if (string.IsNullOrEmpty(_state.Keyword))
                {
                    _state.CurrentType = Types.Format;
                    return;
                }
                else if(IsDataExchangeKeyword || _state.Keyword == "#for")
                {
                    Append(Types.Operator);
                    //_state.CurrentType = Types.None;
                }
            }
            else if (IsDataExchangeKeyword)
            {
                Append(Types.Operator);
                _state.CurrentType = Types.None;
                return;
            }
            else
                Append(Types.Error);

            _state.CurrentType = Types.Bracket;
        }

        private Types InitType(char c, Types t)
        {
            if (c == '$')
                return Types.Command;
            if (c == '#')
                return _state.IsLeading ?
                    Types.Keyword :
                    _state.InputChar == '#' ?
                        Types.Bracket :
                        Types.Error;
            if (Validator.IsDigit(c))
                return Types.Const;
            if (_state.IsMacro && Validator.IsMacroLetter(c, 0))
                return Types.Macro;
            if (Validator.IsLetter(c))
            {
                bool isUnitStart = Validator.IsUnitStart(c);
                if (!(char.IsLetter(c) || isUnitStart || c == '∡' || c == '℧'))
                    return Types.Error;
                if (_state.IsUnits || _state.PreviousType == Types.Const || isUnitStart)
                    return Types.Units;
                else
                    return Types.Variable;
            }
            if (c == '?')
                return Types.Input;
            if (_state.CurrentType != Types.Comment)
                return Types.Error;
            return t;
        }

        private void ParseImaginary(char c, int i, int len)
        {
            var j = i + 1;
            if (j < len && _state.Text[j] == 'n')
            {
                Append(Types.Const);
                _builder.Append(c);
                _state.CurrentType = Types.Units;
            }
            else
            {
                _builder.Append('i');
                Append(Types.Const);
            }
        }

        private void ParseUnits(char c)
        {
            Append(Types.Const);
            _builder.Append(c);
            if (char.IsLetter(c) || Validator.IsUnitStart(c))
                _state.CurrentType = Types.Units;
            else
                _state.CurrentType = Types.Error;
        }

        private void CheckHtmlComment()
        {
            var s = _builder.ToString();
            if (s.StartsWith("<!--") || s.EndsWith("-->"))
                _state.CurrentType = Types.HtmlComment;
        }

        private void Append(Types t)
        {
            if (_builder.Length == 0)
                return;

            if (t == Types.Input)
            {
                AppendRun(t, "? ");
                _builder.Clear();
                return;
            }
            var s = _builder.ToString();
            if (t == Types.Bracket && _builder[0] == '#')
            {
                AppendRun(t, $" {s}");
                _builder.Clear();
                return;
            }
            _builder.Clear();
            if (AppendRelOperatorShortcut(s))
                return;

            if (t == Types.Include)
            {
                s = s.Trim();
                t = AppendInclude(s);
            }
            else if (t == Types.Operator)
            {
                if (_state.IsUnits)
                {
                    var isAllowed = s == "*" || s == "/" || s == "^";
                    if (!isAllowed)
                    {
                        if (s == "-" || s == "+")
                        {
                            var r = _state.Paragraph.Inlines.LastInline as Run;
                            isAllowed = r.Text == "E";
                        }
                        if (!isAllowed)
                        {
                            _state.CurrentType = Types.Error;
                            _state.Message = AppMessages.InvalidOperator;
                        }
                    }
                }
                else if (!IsDataExchangeKeyword)
                    s = FormatOperator(s);

                if (s[0] == ' ')
                {
                    var r = _state.Paragraph.Inlines.LastInline as Run;
                    if (r is not null && r.Text == " ")
                        _state.Paragraph.Inlines.Remove(r);
                }
            }
            else if (t != Types.Error && t != Types.None)
            {
                t = CheckError(t, ref s);
                if (t == Types.Error)
                    _state.CurrentType = Types.Error;
            }
            if (s is not null)
                AppendRun(t, s);

            _allowUnaryMinus =
                t == Types.Comment || t == Types.Operator ||
                s == "(" || s == "{" || s == "[";
        }
        private bool IsDataExchangeKeyword => DataExchangeKeywords.Contains(_state.Keyword);
        private bool AppendRelOperatorShortcut(string s)
        {
            if (_state.Paragraph.Inlines.Count > 0)
            {
                if (s == "=")
                {
                    var r = _state.Paragraph.Inlines.LastInline as Run;
                    switch (r.Text)
                    {
                        case " = ": r.Text = " ≡ "; return true;
                        case "!": r.Text = " ≠ "; return true;
                        case " > ": r.Text = " ≥ "; return true;
                        case " < ": r.Text = " ≤ "; return true;
                    }
                }
                else if (s == "&")
                {
                    var r = _state.Paragraph.Inlines.LastInline as Run;
                    if (r.Text == "&")
                    {
                        r.Text = " ∧ ";
                        r.Background = null;
                        r.Foreground = Colors[(int)Types.Operator];
                        return true;
                    }
                }
                else if (s == "|")
                {
                    var r = _state.Paragraph.Inlines.LastInline as Run;
                    if (r.Text == "|")
                    {
                        r.Text = " ∨ ";
                        r.Background = null;
                        r.Foreground = Colors[(int)Types.Operator];
                        return true;
                    }
                }
                else if (s == "^")
                {
                    var r = _state.Paragraph.Inlines.LastInline as Run;
                    if (r.Text == "^")
                    {
                        r.Text = " ⊕ ";
                        r.Background = null;
                        r.Foreground = Colors[(int)Types.Operator];
                        return true;
                    }
                }
            }
            return false;
        }

        private void AppendDoubleOperatorShortcut(char op)
        {
            if (_builder.Length > 1)
            {
                _builder.Remove(_builder.Length - 1, 1);
                Append(_state.CurrentType);
                _builder.Append(op);
            }
            else
                _builder[0] = op;

            _state.CurrentType = Types.Operator;
            Append(_state.CurrentType);
        }

        private Types AppendInclude(string s)
        {
            var fileName = Path.GetFullPath(Environment.ExpandEnvironmentVariables(s));
            if (File.Exists(fileName))
            {
                try
                {
                    var sourceCode = UserDefined.Include(fileName, null);
                    _state.Message = GetPartialSource(sourceCode);
                }
                catch (Exception e)
                {
                    _state.Message = e.Message;
                }
                return Types.Include;
            }
            _state.Message = AppMessages.File_not_found;
            return Types.Error;
        }

        private Types CheckError(Types t, ref string s)
        {
            if (t == Types.Function)
            {
                if (!IsFunction(s, _state.Line))
                {
                    _state.Message = AppMessages.Undeclared_function;
                    return Types.Error;
                }
            }
            else if (t == Types.Variable)
            {
                var lower = s.ToLower();
                var kwd = _state.Keyword;
                if (kwd is not null)
                {
                    switch (lower)
                    {
                        case "default":
                            if (kwd.Equals("#round"))
                                return Types.Keyword;
                            break;
                        case "on":
                        case "off":
                            if (kwd.Equals("#md"))
                                return Types.Keyword;
                            break;
                        case "from":
                            if (kwd.Equals("#read"))
                            {
                                s = $" {s} ";
                                return Types.Keyword;
                            }
                            break;
                        case "to":
                            if (kwd.Equals("#write") || kwd.Equals("#append"))
                            {
                                s = $" {s} ";
                                return Types.Keyword;
                            }
                            break;
                    }
                }
                var r = _state.Paragraph.Inlines.LastInline as Run;
                var pt = r is null ? Types.None : GetTypeFromColor(r.Foreground);
                if (IsDataExchangeKeyword && _state.Paragraph.Inlines.Count > 3)
                {
                    if (pt == Types.Operator)
                        return Types.Macro;
                    if (!r.Text.EndsWith(' '))
                        _state.Paragraph.Inlines.Add(new Run() { Text = " "});

                    if (lower.Equals("sep") || lower.Equals("type"))
                        return Types.Keyword;
                }
                if (pt == Types.Variable || pt == Types.Units)
                {
                    _state.Message = AppMessages.Invalid_syntax;
                    r.Text += ' ' + s;
                    r.Foreground = Colors[(int)Types.Error];
                    r.Background = ErrorBackground;
                    r.ToolTip = _state.Message;
                    s = null;
                    return Types.Error;
                }
                if (!_state.IsFunction && !IsVariable(s, _state.Line))
                {
                    if (IsUnit(s, _state.Line))
                        return Types.Units;

                    _state.Message = AppMessages.Undeclared_variable;
                    return Types.Error;
                }
            }
            else if (t == Types.Units)
            {
                if (!IsUnit(s, _state.Line))
                {
                    _state.Message = AppMessages.Undefined_units;
                    return Types.Error;
                }
            }
            else if (t == Types.Macro)
            {
                if (!IsDataExchangeKeyword && !Defined.IsMacroOrParameter(s, _state.Line))
                {
                    _state.Message = AppMessages.Undefined_macro_or_parameter;
                    return Types.Error;
                }
            }
            else if (t == Types.Keyword)
            {
                var st = s.TrimEnd();
                if (!Keywords.Contains(st))
                {
                    _state.Message = AppMessages.Invalid_compiler_directive;
                    return Types.Error;
                }
                _state.Keyword = st;
            }
            else if (t == Types.Command)
            {
                if (!Commands.Contains(s))
                {
                    _state.Message = AppMessages.Invalid_method;
                    return Types.Error;
                }
            }
            else if (t == Types.Format)
            {
                if (s.Equals("default", StringComparison.OrdinalIgnoreCase) &&
                    _state.Keyword.Equals("#format"))
                    return Types.Keyword;

                var format = s.StartsWith(':') ? s[1..] : s;
                if (!Validator.IsValidFormatString(format))
                {
                    _state.Message = AppMessages.InvalidFormatString;
                    return Types.Error;
                }
            }
            else
                _state.Message = null;

            return t;
        }

        private void AppendRun(Types t, string s)
        {
            var run = new Run(s)
            {
                Foreground = Colors[(int)t]
            };
            bool isTitle = t == Types.Comment && s[0] == '"';
            if (t == Types.Function || isTitle)
            {
                run.FontWeight = FontWeights.Bold;
                if (isTitle)
                    run.Background = TitleBackground;
            }
            else if (t == Types.Input ||
                     t == Types.Include ||
                     t == Types.Macro ||
                     t == Types.Error)
            {
                if (t == Types.Error)
                    run.Background = ErrorBackground;

                var message = _state.Message;
                if (!string.IsNullOrWhiteSpace(message))
                {
                    run.ToolTip = AppendToolTip(message);
                    _state.Message = null;
                }
                var isInclude = _state.Keyword == "#include";
                if (message is not null && t != Types.Error && (isInclude || IsDataExchangeKeyword))
                {
                    run.Cursor = Cursors.Hand;
                    run.Foreground = Brushes.DodgerBlue;
                    run.TextDecorations.Add(TextDecorations.Underline);
                    if (isInclude)
                        run.AddHandler(UIElement.MouseLeftButtonDownEvent, IncludeClickEventHandler);
                    else
                        run.MouseLeftButtonDown += File_Click;
                }
            }
            else if (t == Types.HtmlComment || t == Types.Format)
                run.Background = Brushes.WhiteSmoke;

            _state.Paragraph.Inlines.Add(run);
        }

        private static ToolTip AppendToolTip(string message) => new()
        {
            Background = ToolTipBackground,
            BorderBrush = Brushes.Black,
            Foreground = Brushes.White,
            FontSize = 12,
            Padding = ToolTipPadding,
            Content = message,
            Placement = PlacementMode.MousePoint,
            HorizontalOffset = -8,
            VerticalOffset = -36
        };


        internal void CheckHighlight(Paragraph p, int line)
        {
            if (p is null)
                return;

            InitLocalValraibles(p);
            var isCommand = false;
            var commandCount = 0;
            var isDataExchangeKeyword = false;
            var i = 0;
            foreach (var inline in p.Inlines)
            {
                if (inline is not Run r)
                    continue;

                var s = r.Text;
                if (i == 0)
                {
                    var lower = s.ToLower();
                    if (lower.Equals("#read") ||
                        lower.Equals("#write") ||
                        lower.Equals("#append"))
                        isDataExchangeKeyword = true;
                }
                ++i;
                var t1 = r.Foreground == Colors[(int)Types.Function] &&
                         r.FontWeight == FontWeights.Bold ?
                         Types.Function :
                         GetTypeFromColor(r.Foreground);
                var t2 = t1;
                if (t1 == Types.Keyword && s[0] == '$')
                    isCommand = true;
                else if (string.Equals(s, "{"))
                {
                    if (isCommand)
                        ++commandCount;
                }
                else if (string.Equals(s, "}"))
                {
                    if (isCommand)
                        --commandCount;

                    if (commandCount == 0)
                        isCommand = false;
                }
                else if (string.Equals(s, " = "))
                    GetLocalVariables(r, commandCount > 0);

                var isFunction = r.NextInline is not null && (r.NextInline as Run).Text == "(";
                bool IsDefined() => isFunction ? IsFunction(s, line) : IsVariable(s, line);
                switch (t1)
                {
                    case Types.Error:
                        if (s[^1] == '$')
                        {
                            if (Defined.IsMacroOrParameter(s, line))
                                t2 = Types.Macro;
                        }
                        else if (IsDefined())
                            t2 = isFunction ? Types.Function : Types.Variable;
                        else if (IsUnit(s, line))
                            t2 = Types.Units;

                        break;
                    case Types.Variable:
                        if (!IsVariable(s, line))
                            t2 = IsUnit(s, line) ? Types.Units : Types.Error;

                        break;
                    case Types.Function:
                        if (!IsFunction(s, line))
                            t2 = Types.Error;

                        break;
                    case Types.Units:
                        if (IsVariable(s, line))
                            t2 = Types.Variable;
                        else if (!IsUnit(s, line))
                            t2 = Types.Error;

                        break;
                    case Types.Macro:
                        if (!isDataExchangeKeyword && !Defined.IsMacroOrParameter(s, line))
                            t2 = Types.Error;

                        break;
                }
                if (t1 != t2)
                {
                    r.Foreground = Colors[(int)t2];
                    r.Background = t2 == Types.Error ? ErrorBackground : null;
                }
            }
        }

        private static void GetLocalVariables(Inline currentInline, bool isCommand)
        {
            var inline = currentInline.PreviousInline;
            if (isCommand)
                GetLocalCommandVariables(inline);
            else
                GetLocalFunctionVariables(inline);
        }

        private static void GetLocalCommandVariables(Inline inline)
        {
            string name = null, s = null;
            var foreground = Colors[(int)Types.Variable];
            while (inline != null)
            {
                s = (inline as Run).Text.Trim();
                if (Validator.IsVariable(s))
                {
                    inline.Background = null;
                    inline.Foreground = foreground;
                    name = s;
                    break;
                }
                inline = inline.PreviousInline;
            }
            if (!string.IsNullOrEmpty(name))
            {
                inline = inline.PreviousInline;
                while (inline != null)
                {
                    s = (inline as Run).Text.Trim();
                    if (s == "@")
                        break;

                    inline = inline.PreviousInline;
                }
                if (s == "@")
                {
                    var bracketCount = 1;
                    inline = inline.PreviousInline;
                    while (inline != null)
                    {
                        s = (inline as Run).Text.Trim();
                        if (s == "{")
                            --bracketCount;
                        else if (s == "}")
                            ++bracketCount;

                        if (bracketCount == 0)
                            break;

                        if (s == name)
                        {
                            inline.Background = null;
                            inline.Foreground = foreground;
                        }
                        inline = inline.PreviousInline;
                    }
                }
            }
        }

        private static void GetLocalFunctionVariables(Inline inline)
        {
            var brackets = false;
            var foreground = Colors[(int)Types.Variable];
            while (inline != null)
            {
                var s = (inline as Run).Text.Trim();
                if (string.Equals(s, "("))
                    return;

                if (!string.IsNullOrEmpty(s))
                {
                    var c = s[^1];
                    if (c == '\'' || c == '"')
                        return;

                    if (string.Equals(s, ")"))
                        brackets = true;
                    else if (brackets && Validator.IsVariable(s))
                    {
                        LocalVariables.Add(s);
                        inline.Background = null;
                        inline.Foreground = foreground;
                    }
                }
                inline = inline.PreviousInline;
            }
        }

        internal static void HighlightBrackets(Paragraph p, int position)
        {
            var tr = new TextRange(p.ContentStart, p.ContentEnd);
            var s = tr.Text;
            var len = s.Length;
            if (len < 2)
                return;

            if (position > len)
                position = len;

            if (position > 0)
                --position;

            var c = s[position];
            if (c != '(' && c != ')' && position > 0 && position < len - 1)
                c = s[++position];

            var otherPosition = position;
            var bracketCount = c == '(' ? 1 : -1;
            if (c == '(')
            {
                for (int i = position + 1; i < len; ++i)
                {
                    UpdateBracketCount(s[i]);
                    if (bracketCount == 0)
                    {
                        otherPosition = i;
                        break;
                    }
                }
            }
            else if (c == ')')
            {
                for (int i = position - 1; i >= 0; --i)
                {
                    UpdateBracketCount(s[i]);
                    if (bracketCount == 0)
                    {
                        otherPosition = i;
                        break;
                    }
                }
            }

            if (otherPosition == position) return;
            if (otherPosition < position)
            {
                AddBracketHighlight(len - position);
                AddBracketHighlight(len - otherPosition);
            }
            else
            {
                AddBracketHighlight(len - otherPosition);
                AddBracketHighlight(len - position);
            }
            return;

            void UpdateBracketCount(char c)
            {
                switch (c)
                {
                    case '(':
                        ++bracketCount;
                        break;
                    case ')':
                        --bracketCount;
                        break;
                }
            }

            void AddBracketHighlight(int offset)
            {
                TextPointer tp = FindPositionAtOffset(p, offset);
                tr = new TextRange(tp, tp.GetPositionAtOffset(1, LogicalDirection.Forward));
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Colors[(int)Types.Bracket]);
            }
        }

        internal static TextPointer FindPositionAtOffset(Paragraph p, int offset)
        {
            var tpe = p.ContentEnd;
            if (offset == 0)
                return tpe;
            var tps = p.ContentStart;
            var x1 = 0;
            var x2 = tps.GetOffsetToPosition(tpe);
            TextPointer tpm = tps;
            while (Math.Abs(x2 - x1) > 1)
            {
                var xm = (x1 + x2) / 2;
                tpm = tps.GetPositionAtOffset(xm);
                var len = new TextRange(tpm, tpe).Text.Length;
                if (len < offset)
                    x2 = xm;
                else if (len > offset)
                    x1 = xm;
                else
                    break;
            }
            return tpm;
        }

        internal static string GetPartialSource(string s)
        {
            if (s.Length > 200)
            {
                int n = s.IndexOf('\n', 200);
                if (n > 0)
                    return s[..n] + "\n...";
            }
            if (s[^1] == '\n')
                return s[..^2];

            return s;
        }

        private string FormatOperator(string name) =>
            name switch
            {
                "-" => _allowUnaryMinus ? "-" : " - ",
                "+" or "=" or "≡" or "≠" or "<" or ">" or "≤" or "≥" or "&" or "@" or ":" or "∧" or "∨" or "⊕" => ' ' + name + ' ',
                ";" => !_state.HasMacro && _state.MacroArgs > 0 ? ";" : "; ",
                _ => name,
            };

        private static void File_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var r = (Run)sender;
                var fileName = r?.Text.Trim();
                fileName = Path.GetFullPath(Environment.ExpandEnvironmentVariables(fileName));
                if (File.Exists(fileName))
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = fileName,
                        WindowStyle = ProcessWindowStyle.Maximized,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                }
            }
        }
    }
}