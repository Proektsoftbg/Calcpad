using Calcpad.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Calcpad.Wpf
{
    internal class HighLighter
    {
        internal static Func<string, Queue<string>, string> Include;
        internal static MouseButtonEventHandler InputClickEventHandler;
        internal static MouseButtonEventHandler IncludeClickEventHandler;

        private const char NullChar = (char)0;
        private bool _allowUnaryMinus = true;
        private Queue<string> _values = null;
        private bool _hasTargetUnitsDelimiter;
        private static bool _hasIncludes;
        private static readonly Thickness _thickness = new(0.5);

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
            Error
        }

        private static readonly Brush[] Colors =
        {
             Brushes.Gray,
             Brushes.Black,
             Brushes.DarkCyan,
             Brushes.Goldenrod,
             Brushes.Blue,
             Brushes.Black,
             Brushes.Magenta,
             Brushes.Magenta,
             Brushes.DeepPink,
             Brushes.ForestGreen,
             Brushes.DarkOrchid,
             Brushes.Red,
             Brushes.Indigo,
             Brushes.DarkMagenta,
             Brushes.Crimson
        };

        private static readonly Thickness ToolTipPadding = new(3, 1, 3, 2);
        private static readonly SolidColorBrush ToolTipBackground = new(Color.FromArgb(196, 0, 0, 0));
        private static readonly SolidColorBrush TitleBackground = new(Color.FromRgb(245, 255, 240));
        private static readonly SolidColorBrush ErrorBackground = new (Color.FromRgb(255, 225, 225));
        private static readonly HashSet<char> Operators = new() { '!', '^', '/', '÷', '\\', '%', '*', '-', '+', '<', '>', '≤', '≥', '≡', '≠', '=' };
        private static readonly HashSet<char> Delimiters = new() { ';', '|', '&', '@', ':' };
        private static readonly HashSet<char> Brackets = new() { '(', ')', '{', '}' };

        private static readonly HashSet<string> Functions = new(StringComparer.OrdinalIgnoreCase)
        {
            "abs",
            "sin",
            "cos",
            "tan",
            "csc",
            "sec",
            "cot",
            "asin",
            "acos",
            "atan",
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
            "random"
        };

        private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase) {
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
            "#show",
            "#hide",
            "#pre",
            "#post",
            "#repeat",
            "#loop",
            "#break",
            "#include",
            "#local",
            "#global",
            "#def",
            "#end def"
        };

        private static readonly HashSet<string> Commands = new(StringComparer.OrdinalIgnoreCase)
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
        };

        internal readonly HashSet<string> LocalVariables = new(StringComparer.Ordinal);
        internal static readonly Dictionary<string, int> DefinedVariables = new(StringComparer.Ordinal);
        internal static readonly Dictionary<string, int> DefinedFunctions = new(StringComparer.Ordinal);
        internal static readonly Dictionary<string, int> DefinedMacros = new(StringComparer.Ordinal);
        internal static readonly Dictionary<string, List<int>> DefinedMacroParameters = new(StringComparer.Ordinal);
        internal static readonly char[] Comments = { '\'', '"' };
        internal static readonly string[] NewLines = { "\r\n", "\r", "\n" };
        internal static bool HasMacros => _hasIncludes || DefinedMacros.Count > 0;
        private class TagHelper
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
                            if (_builder.Length == 1)
                                Type = Tags.Closing;
                            else
                                Type = Tags.SelfClosing;
                        }
                        else if (!(IsLatinLetter(c) || char.IsDigit(c) && _builder.Length == 2 && _builder[1] == 'h'))
                            return false;
                    }
                    else
                        t = Types.Tag;
                }
                else if (Type == Tags.SelfClosing || Type == Tags.Closing && !IsLatinLetter(c))
                    return false;

                return true;
            }
        }

        private TagHelper _tagHelper;

        private struct ParserState
        {
            internal Paragraph Paragraph;
            internal int Line;
            internal string Text;
            internal string Message;
            internal char TextComment;
            internal char TagComment;
            internal bool IsLeading;
            internal bool IsUnits;
            internal bool IsPlot;
            internal bool IsTag;
            internal bool IsTagComment;
            internal bool IsMacro;
            internal bool IsSingleLineKeyword;
            internal bool HasMacro;
            internal int MacroArgs;
            internal int CommandCount;
            internal int BracketCount;
            internal Types CurrentType;
            internal Types PreviousType;

            internal void RetainType()
            {
                if (CurrentType != Types.None)
                    PreviousType = CurrentType;
            }
        }

        private ParserState _state;
        private readonly StringBuilder _builder = new(100);

        internal void Clear(Paragraph p)
        {
            if (p is null)
                return;

            GetValues(p);
            if (_values is not null)
                p.Tag = _values;

            foreach (var inline in p.Inlines)
                inline.Background = null;

            p.Background = Brushes.FloralWhite;
            p.BorderBrush = Brushes.NavajoWhite;
            p.BorderThickness = _thickness;
        }

        internal void Parse(Paragraph p, bool isComplex, int line)
        {
            if (p is null)
                return;

            if (p.Tag is Queue<string> queue)
                _values = queue;
            else
                GetValues(p);

            var text = InitParagraph(p);
            InitState(p, text, line);
            _tagHelper = new();
            LocalVariables.Clear();
            _hasTargetUnitsDelimiter = false;
            _allowUnaryMinus = true;
            _builder.Clear();
            for (int i = 0, len = text.Length; i < len; ++i)
            {
                var c = text[i];
                if (!(_state.IsPlot || _state.CurrentType == Types.Comment) && c == '|')
                    _state.IsUnits = true;

                if (_state.MacroArgs == 0)
                    ParseComment(c);

                if (_state.MacroArgs > 0)
                    ParseMacroArgs(c);
                else if (_state.TextComment != NullChar)
                    ParseTagInComment(c);
                else if (_state.CurrentType == Types.Include)
                    _builder.Append(c);
                else if (c == '$' && _builder.Length > 0)
                    ParseMacro();
                else if (c == ' ' || c == '\t')
                    ParseSpace(c);
                else if (Brackets.Contains(c))
                    ParseBrackets(c);
                else if (Operators.Contains(c))
                {
                    ParseOperator(c);
                    if (ParseMacroContent(c, i, len))
                        return;
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
                else if (_state.CurrentType == Types.Const && IsLetter(c))
                    ParseUnits(c);
                else
                {
                    if (IsParseError(c, _state.CurrentType))
                    {
#if BG
                        _state.Message = $"Невалиден символ: {c}.";
#else
                        _state.Message = $"Invalid character: {c}.";
#endif
                        _state.CurrentType = Types.Error;
                    }

                    _builder.Append(c);
                }

                if (_state.IsSingleLineKeyword)
                {
                    _builder.Append(text[++i..]);
#if BG
                        _state.Message = $"Очаква се край на ред.";
#else
                        _state.Message = $"End of line expected.";
#endif
                    Append(Types.Error);
                    return;
                }

                _state.RetainType();

                if (c != ' ' && c != '\t')
                    _state.IsLeading = false;
            }
            Append(_state.CurrentType);
        }

        private static string InitParagraph(Paragraph p)
        {
            p.Tag = false;
            p.Background = null;
            p.BorderBrush = null;
            p.BorderThickness = _thickness;
            var text = new TextRange(p.ContentStart, p.ContentEnd).Text;
            p.Inlines.Clear();
            return text;
        }
        private void InitState(Paragraph p, string text, int line)
        {
            _state = new()
            {
                Paragraph = p,
                Line = line,
                Text = text,
                Message = null,
                TextComment = NullChar,
                TagComment = NullChar,
                IsLeading = true,
                IsUnits = false,
                IsPlot = IsPlot(text),
                IsTag = false,
                IsTagComment = false,
                IsMacro = false,
                IsSingleLineKeyword = false,
                HasMacro = false,
                MacroArgs = 0,
                CommandCount = 0,
                BracketCount = 0,
                CurrentType = Types.None,
                PreviousType = Types.None,
            };
        }

        internal static string GetTypeFromColor(Brush color)
        {
            for (int i = 0, len = Colors.Length; i < len; ++i)
                if (Colors[i] == color)
                    return typeof(Types).GetEnumName(i).ToLowerInvariant();

            return null;
        }

        private void ParseComment(char c)
        {
            if (c >= 'А' && c <= 'я' && _state.CurrentType != Types.Include)
            {
                if (_state.TextComment == NullChar)
                {
                    Append(_state.CurrentType);
                    _state.TextComment = _state.Paragraph.PreviousBlock is null ? '"' : '\'';
                    _builder.Clear();
                    _builder.Append(_state.TextComment);
                    _state.CurrentType = Types.Comment;
                }
            }
            else if (c == '\'' || c == '"')
            {
                if (_state.TextComment == NullChar)
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
                else if (_state.TextComment == c)
                {
                    _state.TextComment = NullChar;
                    if (_state.IsTag && _state.IsTagComment)
                        _state.CurrentType = Types.Comment;
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
                    _state.IsTagComment = !_state.IsTagComment;
                }
            }
        }

        private bool IsParseError(char c, Types t) =>
                t == Types.Const && !IsDigit(c) ||
                t == Types.Macro && !IsMacroLetter(c, _builder.Length) ||
                t == Types.Variable && !IsLetter(c) && !IsDigit(c);

        private void ParseMacroInComment(Types t)
        {
            var len = _builder.Length;
            int i;
            for (i = len - 2; i >= 0; --i)
                if (!IsMacroLetter(_builder[i], i))
                    break;

            var s = _builder.ToString();
            for (int j = i + 1; j < len - 1; ++j)
            {
                if (IsDefinedMacroOrParameter(s[j..], _state.Line))
                {
                    _builder.Remove(j, len - j);
                    Append(t);
                    _builder.Append(s[j..]);
                    Append(Types.Macro);
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
                    --_state.BracketCount;
                    if (_state.BracketCount <= _state.MacroArgs)
                        _state.MacroArgs = 0;
                }
                Append(_state.CurrentType);
                _builder.Append(c);
                _state.CurrentType = Types.Bracket;
                Append(_state.CurrentType);
            }
            else if (c == ';')
            {
                Append(_state.CurrentType);
                _builder.Append(c);
                _state.CurrentType = Types.Operator;
                Append(_state.CurrentType);
            }
            else
            {
                if (_state.PreviousType != Types.Operator || c != ' ')
                    _builder.Append(c);
            }
            _state.PreviousType = _state.CurrentType;
        }

        private void ParseTagInComment(char c)
        {
            if (c == '>')
            {
                _builder.Append(c);
                if (_state.IsTag)
                    _state.CurrentType = Types.Tag;
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

                if (!(_state.IsTagComment && c == _state.TagComment))
                {
                    _builder.Append(c);
                    if (c == '$' && DefinedMacros.Count > 0)
                        ParseMacroInComment(Types.Comment);
                }
            }
        }

        private void ParseMacro()
        {
            if (_state.IsMacro)
            {
                if (_state.CurrentType != Types.Macro)
                    _state.CurrentType = Types.Error;
            }
            else if (_state.CurrentType != Types.Variable)
                _state.CurrentType = Types.Error;
            else if (IsValidMacroName())
                _state.CurrentType = Types.Macro;
            else
            {
                _state.Message = "Invalid macro name.";
                _state.CurrentType = Types.Error;
            }
            _builder.Append('$');
            Append(_state.CurrentType);
            if (_state.IsMacro)
                _state.HasMacro = true;

            _state.IsMacro = false;
        }


        private bool IsValidMacroName()
        {
            for (int i = 0, len = _builder.Length; i < len; ++i)
                if (!IsMacroLetter(_builder[i], i))
                    return false;

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
                var isInclude = _builder.Length == 8 &&
                    _builder[1] == 'i' &&
                    _builder[2] == 'n' &&
                    _builder[3] == 'c';
                Append(_state.CurrentType);
                //Spaces are added only if they are leading
                if (_state.IsLeading || _state.CurrentType == Types.Keyword)
                {
                    _builder.Append(c);
                    Append(Types.None);
                }
                if (isInclude)
                    _state.CurrentType = Types.Include;
                else
                    _state.CurrentType = Types.None;
            }

            static bool IsContinuedCondition(string text) =>
                text.Equals("#else", StringComparison.OrdinalIgnoreCase) ||
                text.Equals("#end", StringComparison.OrdinalIgnoreCase);
        }

        private void ParseBrackets(char c)
        {
            if (c == '(')
            {
                ++_state.BracketCount;
                if (_state.CurrentType == Types.Variable)
                    _state.CurrentType = Types.Function;
                else if (_state.CurrentType == Types.Macro && _state.MacroArgs == 0)
                    _state.MacroArgs = _state.BracketCount;
                else if (_state.CurrentType != Types.Operator && _state.CurrentType != Types.Bracket && _state.CurrentType != Types.None)
                    _state.CurrentType = Types.Error;
            }
            else if (c == ')')
            {
                --_state.BracketCount;
                if (_state.BracketCount == _state.MacroArgs)
                    _state.MacroArgs = 0;
            }
            Append(_state.CurrentType);
            if (c == '{')
            {
                if (_state.CurrentType == Types.Command)
                    ++_state.CommandCount;

                _state.CurrentType = _state.CommandCount > 0 ? Types.Bracket : Types.Error;
            }
            else if (c == '}')
            {
                --_state.CommandCount;
                _state.CurrentType = _state.CommandCount >= 0 ? Types.Bracket : Types.Error;
            }
            else
                _state.CurrentType = Types.Bracket;

            _builder.Append(c);
            Append(_state.CurrentType);
        }

        private void ParseOperator(char c)
        {
            Append(_state.CurrentType);
            _builder.Append(c);
            Append(Types.Operator);
            _state.CurrentType = Types.Operator;
            if (c == '=')
                GetLocalVariables(_state.CommandCount > 0);
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
            Append(_state.CurrentType);
            _builder.Append(c);
            if (_state.CommandCount > 0 || c == ';')
                Append(Types.Operator);
            else
                Append(Types.Error);
            _state.CurrentType = Types.Bracket;
        }

        private Types InitType(char c, Types t)
        {
            if (c == '$')
                return Types.Command;
            if (c == '#')
                return _state.IsLeading ? Types.Keyword : Types.Error;
            if (IsDigit(c))
                return Types.Const;
            if (_state.IsMacro && IsMacroLetter(c, 0))
                return Types.Macro;
            if (IsLetter(c))
            {
                bool isUnitStart = IsUnitStart(c);
                if (!(char.IsLetter(c) || isUnitStart ||  c == '∡' || c == '℧'))
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
            if (char.IsLetter(c) || IsUnitStart(c))
                _state.CurrentType = Types.Units;
            else
                _state.CurrentType = Types.Error;
        }

        private void Append(Types t)
        {
            if (_builder.Length == 0)
                return;

            var s = _builder.ToString();
            _builder.Clear();

            if (AppendRelOperatorShortcut(s))
                return;

            if (t == Types.Include)
                t = AppendInclude(s);
            else if (t == Types.Operator)
                s = FormatOperator(s);
            else if (t == Types.Input)
            {
                s = "? ";
                if (_values is not null && _values.Count > 0)
                    _state.Message = _values.Dequeue();
                else
                    _state.Message = "0";
            }
            else if (t != Types.Error)
                t = CheckError(t, s);

            AppendRun(t, s);
            _allowUnaryMinus = t == Types.Operator || s == "(" || s == "{";
        }


        private bool AppendRelOperatorShortcut(string s)
        {
            var count = _state.Paragraph.Inlines.Count;
            if (s == "=" && count > 0)
            {
                Run r = (Run)_state.Paragraph.Inlines.LastInline;
                var runText = r.Text;
                if (runText == " = " || runText == "!" || runText == " > " || runText == " < ")
                {
                    r.Text = runText switch
                    {
                        " = " => " ≡ ",
                        "!" => " ≠ ",
                        " > " => " ≥ ",
                        _ => " ≤ "
                    };
                    return true;
                }
            }
            return false;
        }

        private Types AppendInclude(string s)
        {
            var sourceFlieName = s.Trim();
            if (File.Exists(sourceFlieName))
            {
                var values = new Queue<string>();
                var sourceCode = Include(sourceFlieName, values);
                _state.Paragraph.Tag = values;
                var lines = sourceCode.Split(Environment.NewLine);
                for (int i = 0, len = lines.Length; i < len; ++i)
                    GetDefinedVariablesFunctionsAndMacros(lines[i], _state.Line);

                _state.Message = GetPartialSource(sourceCode);
                return Types.Include;
            }
#if BG
            _state.Message = "Файлът не е намерен.";
#else
            _state.Message = "File not found.";
#endif
            return Types.Error;
        }

        private Types CheckError(Types t, string s)
        {
            if (t == Types.Function)
            {
                if (!DefinedFunctions.TryGetValue(s, out int funcLine))
                    funcLine = int.MaxValue;

                if (!Functions.Contains(s) && funcLine > _state.Line)
                {
#if BG
                    _state.Message = "Недекларирана функция.";
#else
                    _state.Message = "Undeclared function.";
#endif
                    return Types.Error;
                }
            }
            else if (t == Types.Variable)
            {
                var varLine = _state.Line;
                if (!LocalVariables.Contains(s) &&
                    !DefinedVariables.TryGetValue(s, out varLine))
                    varLine = int.MaxValue;

                if (varLine > _state.Line)
                {
                    if (MathParser.IsUnit(s))
                        return Types.Units;
#if BG
                    _state.Message = "Недекларирана променлива.";
#else
                    _state.Message = "Undeclared variable.";
#endif
                    return Types.Error;
                }
            }
            else if (t == Types.Units)
            {
                if (!MathParser.IsUnit(s))
                {
#if BG
                    _state.Message = "Недефинирани мерни единици.";
#else
                    _state.Message = "Undefined units.";
#endif
                    return Types.Error;
                }
            }
            else if (t == Types.Macro)
            {
                if (!IsDefinedMacroOrParameter(s, _state.Line))
                {
#if BG
                    _state.Message = "Недефиниран макрос или параметър.";
#else
                    _state.Message = "Undefined macro or parameter.";
#endif
                    return Types.Error;
                }
            }
            else if (t == Types.Keyword)
            {
                var st = s.TrimEnd();
                if (!Keywords.Contains(st))
                {
#if BG
                    _state.Message = "Невалидна директива за компилатор.";
#else
                    _state.Message = "Invalid compiler directive.";
#endif
                    return Types.Error;
                }
                if (!(st.Equals("#if", StringComparison.OrdinalIgnoreCase) || 
                      st.Equals("#else if", StringComparison.OrdinalIgnoreCase) || 
                      st.Equals("#repeat", StringComparison.OrdinalIgnoreCase) || 
                      st.Equals("#def", StringComparison.OrdinalIgnoreCase) || 
                      st.Equals("#include", StringComparison.OrdinalIgnoreCase)))
                    _state.IsSingleLineKeyword = true;
            }
            else if (t == Types.Command)
            {
                if (!Commands.Contains(s))
                {
#if BG
                    _state.Message = "Невалиден метод.";
#else
                    _state.Message = "Invalid method.";
#endif
                    return Types.Error;
                }
            }
            else
                _state.Message = null;

            return t;
        }

        private void AppendRun(Types t, string s)
        {
            var run = new Run(s);
            s = s.ToLowerInvariant();
            run.Foreground = Colors[(int)t];
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
                {
                    if (s == "|" && !_hasTargetUnitsDelimiter)
                        _hasTargetUnitsDelimiter = true;
                    else
                        run.Background = ErrorBackground;
                }
                if (!string.IsNullOrWhiteSpace(_state.Message))
                {
                    run.ToolTip = AppendToolTip(_state.Message);
                    _state.Message = null;
                }

                if (run.ToolTip is not null && t == Types.Input || t == Types.Include)
                {
                    run.Cursor = Cursors.Hand;
                    if (t == Types.Include)
                        run.AddHandler(UIElement.MouseLeftButtonDownEvent, IncludeClickEventHandler);
                    else if (t == Types.Input)
                    {
                        run.FontWeight = FontWeights.Bold;
                        run.AddHandler(UIElement.MouseLeftButtonDownEvent, InputClickEventHandler);
                        _state.Paragraph.Tag = true;
                    }
                }
            }
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
        

        private void GetValues(Paragraph p)
        {
            if (p.Tag is bool b && b)
            {
                _values = new Queue<string>();
                foreach (var inline in p.Inlines)
                    if (inline.ToolTip is ToolTip tt)
                        _values.Enqueue(tt.Content.ToString());
            }
            else
                _values = null;
        }

        private void GetLocalVariables(bool isCommand)
        {
            var brackets = false;
            string name = null, s = null;
            Brush foreground = Colors[(int)Types.Variable];
            if (isCommand)
            {
                Run run = (Run)_state.Paragraph.Inlines.LastInline.PreviousInline;
                while (run != null)
                {
                    s = run.Text.Trim();
                    if (IsVariable(s))
                    {
                        run.Background = null;
                        run.Foreground = foreground;
                        name = s;
                        break;
                    }
                    run = (Run)run.PreviousInline;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    run = (Run)run.PreviousInline;
                    while (run != null)
                    {
                        s = run.Text.Trim();
                        if (s == "@")
                            break;

                        run = (Run)run.PreviousInline;
                    }
                    if (s == "@")
                    {
                        var bracketCount = 1;
                        run = (Run)run.PreviousInline;
                        while (run != null)
                        {
                            s = run.Text.Trim();
                            if (s == "{")
                                --bracketCount;
                            else if (s == "}")
                                ++bracketCount;

                            if (bracketCount == 0)
                                break;

                            if (s == name)
                            {
                                run.Background = null;
                                run.Foreground = foreground;
                            }
                            run = (Run)run.PreviousInline;
                        }
                    }
                }
            }
            else
            {
                foreach (Run run in _state.Paragraph.Inlines.Cast<Run>())
                {
                    s = run.Text.Trim();
                    if (s == "(")
                        brackets = true;
                    else if (s == ")")
                        return;
                    else if (brackets)
                    {
                        if (IsVariable(s))
                        {
                            LocalVariables.Add(s);
                            run.Background = null;
                            run.Foreground = foreground;
                        }
                    }
                }
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

            if (position < 1)
                position = 1;

            var c = s[--position];
            if (c != '(' && c != ')' && position < len - 1)
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

            if (otherPosition != position)
            {
                if (otherPosition < position)
                {
                    AddBracketHighlight(otherPosition + 1);
                    AddBracketHighlight(position - len - 1);
                }
                else
                {
                    AddBracketHighlight(position + 1);
                    AddBracketHighlight(otherPosition - len - 1);
                }
            }

            void UpdateBracketCount(char c)
            {
                if (c == '(')
                    ++bracketCount;
                else if (c == ')')
                    --bracketCount;
            }

            void AddBracketHighlight(int offset)
            {
                TextPointer tp = offset >= 0 ?
                    p.ContentStart.GetPositionAtOffset(offset) :
                    p.ContentEnd.GetPositionAtOffset(offset);
                tr = new TextRange(tp, tp.GetPositionAtOffset(1, LogicalDirection.Forward));
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Colors[(int)Types.Bracket]);
            }
        }

        internal static void GetDefinedVariablesFunctionsAndMacros(string code, bool IsComplex) =>
            GetDefinedVariablesFunctionsAndMacros(code.Split(Environment.NewLine), IsComplex);

        internal static void ClearDefinedVariablesFunctionsAndMacros(bool IsComplex)
        {
            DefinedVariables.Clear();
            DefinedFunctions.Clear();
            DefinedMacros.Clear();
            DefinedMacroParameters.Clear();
            DefinedVariables.Add("e", -1);
            DefinedVariables.Add("pi", -1);
            DefinedVariables.Add("π", -1);
            DefinedVariables.Add("g", -1);
            if (IsComplex)
            {
                DefinedVariables.Add("i", -1);
                DefinedVariables.Add("ei", -1);
                DefinedVariables.Add("πi", -1);
            }
            _hasIncludes = false;
        }


        internal static void GetDefinedVariablesFunctionsAndMacros(IEnumerable<string> lines, bool IsComplex)
        {
            ClearDefinedVariablesFunctionsAndMacros(IsComplex);
            var lineNumber = 0;
            foreach (var lineContent in lines)
                GetDefinedVariablesFunctionsAndMacros(lineContent, ++lineNumber);
        }

        internal static void GetDefinedVariablesFunctionsAndMacros(string lineContent, int lineNumber)
        {
            if (IsKeyword(lineContent, "#include"))
            {
                var s = GetFile(lineContent);
                if (File.Exists(s))
                {
                    s = Include(s, null);
                    var lines = s.Split(Environment.NewLine);
                    foreach (var line in lines)
                        GetDefinedVariablesFunctionsAndMacros(line, lineNumber);
                }
                _hasIncludes = true;
            }
            else if (IsKeyword(lineContent, "#def"))
                GetDefinedMacros(lineContent, lineNumber);
            else if (IsKeyword(lineContent, "#end def"))
                CompleteDefinedMacroParameters(lineNumber);
            else
                GetDefinedVariablesAndFunctions(lineContent, lineNumber);
        }

        internal static bool IsKeyword(string s, string keyword) =>
            s.TrimStart().StartsWith(keyword, StringComparison.OrdinalIgnoreCase);

        internal static string GetFile(string s)
        {
            int n = s.Length;
            if (n < 9)
                return null;

            n = s.IndexOfAny(Comments);
            if (n < 9)
                n = s.Length;

            return s[8..n].Trim();
        }

        private static void GetDefinedVariablesAndFunctions(string lineContent, int lineNumber)
        {
            var chunks = GetChunks(lineContent);
            for (int i = 0, count = chunks.Count; i < count; ++i)
            {
                var chunk = chunks[i];
                var isDone = false;
                var isFunction = false;
                var _builder = new StringBuilder(50);
                for (int j = 0, len = chunk.Length; j < len; ++j)
                {
                    var c = chunk[j];
                    if (c == ' ' && _builder.Length == 0)
                        continue;
                    else if (IsLetter(c) || IsDigit(c))
                    {
                        if (isDone)
                        {
                            if (isFunction)
                                continue;

                            break;
                        }
                        _builder.Append(c);
                    }
                    else if (c == '=')
                    {
                        if (_builder.Length > 0)
                        {
                            var s = _builder.ToString();
                            if (isFunction)
                                DefinedFunctions.TryAdd(s, lineNumber);
                            else
                                DefinedVariables.TryAdd(s, lineNumber);
                        }
                        break;
                    }
                    else
                    {
                        isDone = true;
                        if (c == '(')
                            isFunction = true;
                    }
                }
            }
        }

        private static List<string> GetChunks(string text)
        {
            const char noComment = (char)0;
            var chunks = new List<string>();
            var comment = noComment;
            int j = 0, len = text.Length;
            for (int i = 0; i < len; ++i)
            {
                char c = text[i];
                if (c == '"' || c == '\'')
                {
                    if (comment == c)
                    {
                        comment = noComment;
                        j = i + 1;
                    }
                    else if (comment == noComment)
                    {
                        comment = c;
                        if (i > j)
                            chunks.Add(text[j..i]);
                    }
                }
                else if (c == '\n')
                {
                    if (i > j)
                    {
                        if (text[i - 1] == '\r')
                            chunks.Add(text[j..(i - 1)]);
                        else
                            chunks.Add(text[j..i]);
                    }
                    j = i + 1;
                }
            }
            if (comment == noComment && j < len)
                chunks.Add(text[j..len]);

            return chunks;
        }

        private static void GetDefinedMacros(string lineContent, int lineNumber)
        {
            var isFunction = false;
            var isComplete = false;
            var macroBuilder = new StringBuilder(20);
            for (int i = 4, len = lineContent.Length; i < len; ++i)
            {
                var c = lineContent[i];
                if (c == ' ' && macroBuilder.Length == 0)
                    continue;
                else if (IsMacroLetter(c, macroBuilder.Length))
                    macroBuilder.Append(c);
                else if (c == '$')
                {
                    if (macroBuilder.Length > 0)
                    {
                        macroBuilder.Append('$');
                        var s = macroBuilder.ToString();
                        if (isFunction)
                        {
                            if (!DefinedMacroParameters.TryAdd(s, new() { lineNumber, -1 }))
                            {
                                var bounds = DefinedMacroParameters[s];
                                bounds.Add(lineNumber);
                                bounds.Add(-1);
                            }
                        }
                        else
                            DefinedMacros.TryAdd(s, lineNumber);
                    }
                }
                else
                {
                    if (c == '(' || c == ';')
                        macroBuilder.Clear();

                    if (c == '(')
                        isFunction = true;
                    else if (isFunction && c == ')')
                        isComplete = true;
                    else if (isComplete && c == '=')
                    {
                        CompleteDefinedMacroParameters(lineNumber);
                        return;
                    }
                    else if (c != ' ' && c != ';')
                        break;
                }
            }
            if (DefinedMacros.Count == 0)
                DefinedMacros.Add("Invalid$", -1);
        }

        private static void CompleteDefinedMacroParameters(int lineNumber)
        {
            foreach (var kvp in DefinedMacroParameters)
            {
                var bounds = kvp.Value;
                for (int i = 1, count = bounds.Count; i < count; i += 2)
                {
                    if (bounds[i] == -1)
                        bounds[i] = lineNumber;
                }
            }
        }


        private static bool IsDefinedMacroOrParameter(string s, int line)
        {
            if (DefinedMacros.TryGetValue(s, out var macroLine))
            {
                if (macroLine <= line)
                    return true;
            }
            else if (DefinedMacroParameters.TryGetValue(s, out var bounds))
                for (int i = 1, count = bounds.Count; i < count; i += 2)
                    if (line >= bounds[i - 1] && line <= bounds[i])
                        return true;

            return false;
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

        private static bool IsVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            char c = name[0];
            if (!IsLetter(c) || "_,′″‴⁗".Contains(c))
                return false;

            for (int i = 1, len = name.Length; i < len; ++i)
            {
                c = name[i];
                if (!(IsLetter(c) || IsDigit(c)))
                    return false;
            }
            return true;
        }


        private static bool IsPlot(string text)
        {
            var s = text.TrimStart();
            if (!string.IsNullOrEmpty(s) && s[0] == '$')
                return s.StartsWith("$plot", StringComparison.OrdinalIgnoreCase);

            return false;
        }

        private static bool IsMacroLetter(char c, int position) =>
            c >= 'a' && c <= 'z' ||
            c >= 'A' && c <= 'Z' ||
            c == '_' ||
            char.IsDigit(c) && position > 0;

        internal static bool IsLetter(char c) =>
            c >= 'a' && c <= 'z' || // a - z
            c >= 'A' && c <= 'Z' || // A - Z 
            c >= 'α' && c <= 'ω' ||   // alpha - omega
            c >= 'Α' && c <= 'Ω' ||  // Alpha - Omega
            "_,°′″‴⁗ϑϕøØ℧∡".Contains(c, StringComparison.Ordinal); // _ , ° ′ ″ ‴ ⁗ ϑ ϕ ø Ø ℧ ∡

        internal static bool IsLatinLetter(char c) =>
            c >= 'a' && c <= 'z' || // a - z
            c >= 'A' && c <= 'Z'; // A - Z 

        internal static bool IsDigit(char c) =>
            c >= '0' && c <= '9' || c == MathParser.DecimalSymbol;

        internal static bool IsUnitStart(char c) =>
            c == '°' || c == '′' || c == '″';

        private string FormatOperator(string name) =>
            name switch
            {
                "-" => _allowUnaryMinus ? "-" : " - ",
                "+" or "=" or "≡" or "≠" or "<" or ">" or "≤" or "≥" or "&" or "@" or ":" => ' ' + name + ' ',
                ";" => name + ' ',
                _ => name,
            };
    }
}