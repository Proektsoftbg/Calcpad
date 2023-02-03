﻿using Calcpad.Core;
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
        internal UserDefined Defined = new();
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
                    if (InputChar == '{')
                        InputChar = '}';
                    else
                        InputChar = '\0';
                }
                else if (c != ' ')
                {
                    if (InputChar != '{')
                        InputChar = '\0';
                }
            }
        }

        internal static MouseButtonEventHandler IncludeClickEventHandler;
        internal static readonly HashSet<string> LocalVariables = new(StringComparer.Ordinal);
        private bool _allowUnaryMinus = true;
        private bool _hasTargetUnitsDelimiter;
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
        private static readonly SolidColorBrush ErrorBackground = new(Color.FromRgb(255, 225, 225));
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
            "#continue",
            "#include",
            "#local",
            "#global",
            "#def",
            "#end def",
            "#round",
            "#pause",
            "#input"
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


        internal static readonly char[] Comments = { '\'', '"' };

        private TagHelper _tagHelper;
        private ParserState _state;
        private readonly StringBuilder _builder = new(100);

        internal static void Clear(Paragraph p)
        {
            if (p is null)
                return;

            foreach (var inline in p.Inlines)
                inline.Background = null;

            p.Background = Brushes.FloralWhite;
            p.BorderBrush = Brushes.NavajoWhite;
            p.BorderThickness = _thickness;
        }

        internal void CheckHighlight(Paragraph p, int line)
        {
            if (p is null)
                return;

            LocalVariables.Clear();
            var isCommand = false;
            var commandCount = 0;
            foreach (Run r in p.Inlines.Cast<Run>())
            {
                var s = r.Text;
                var t1 = r.Foreground == Colors[(int)Types.Function] &&
                         r.FontWeight == FontWeights.Bold ?
                         Types.Function :
                         GetTypeFromColor(r.Foreground);
                var t2 = t1;
                if (t1 == Types.Keyword && s[0] == '$')
                    isCommand = true;
                else if (string.Equals(s, "{", StringComparison.Ordinal))
                {
                    if (isCommand)
                        ++commandCount;
                }
                else if (string.Equals(s, "}", StringComparison.Ordinal))
                {
                    if (isCommand)
                        --commandCount;

                    if (commandCount == 0)
                        isCommand = false;
                }
                else if (string.Equals(s, " = ", StringComparison.Ordinal))
                    GetLocalVariables(r, commandCount > 0);

                var isFunction = r.NextInline is not null && ((Run)r.NextInline).Text == "(";
                bool IsDefined() => isFunction ?
                    Defined.IsFunction(s, line) || Functions.Contains(s) :
                    LocalVariables.Contains(s) || Defined.IsVariable(s, line);
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

                        break;
                    case Types.Variable:
                        if (!(LocalVariables.Contains(s) || Defined.IsVariable(s, line)))
                        {
                            if (MathParser.IsUnit(s))
                                t2 = Types.Units;
                            else
                                t2 = Types.Error;
                        }
                        break;
                    case Types.Function:
                        if (!(Defined.IsFunction(s, line) || Functions.Contains(s)))
                            t2 = Types.Error;
                        break;
                    case Types.Units:
                        if (LocalVariables.Contains(s) || Defined.IsVariable(s, line))
                            t2 = Types.Variable;
                        break;
                    case Types.Macro:
                        if (!Defined.IsMacroOrParameter(s, line))
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

        internal void Parse(Paragraph p, bool isComplex, int line, string text = null)
        {
            if (p is null)
                return;

            InitParagraph(p);
            text ??= new TextRange(p.ContentStart, p.ContentEnd).Text;
            p.Inlines.Clear();
            InitState(p, text, line);
            _tagHelper = new();
            LocalVariables.Clear();
            _hasTargetUnitsDelimiter = false;
            _allowUnaryMinus = true;
            _builder.Clear();
            for (int i = 0, len = text.Length; i < len; ++i)
            {
                var c = text[i];
                _state.GetInputState(c);
                if (!(_state.IsPlot || _state.CurrentType == Types.Comment) && c == '|')
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
                else if (Validator.IsWhiteSpace(c))
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
                else if (_state.CurrentType == Types.Const && Validator.IsLetter(c))
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
                    if (_state.CurrentType == Types.None && _builder.Length > 0)
                    {
                        _state.CurrentType = _state.PreviousType;
                        Append(_state.CurrentType);
                        if (_state.CurrentType == Types.Error)
                        {
                            _builder.Append(' ');
                            Append(Types.Error);
                        }
                        _state.CurrentType = InitType(c, _state.CurrentType);
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

                if (!Validator.IsWhiteSpace(c))
                    _state.IsLeading = false;
            }
            Append(_state.PreviousTypeIfCurrentIsNone);
        }

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

        private void ParseComment(char c)
        {
            if (c >= 'А' && c <= 'я' && _state.CurrentType != Types.Include)
            {
                if (_state.TextComment == '\0')
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
                t == Types.Const && !Validator.IsDigit(c) ||
                t == Types.Macro && !Validator.IsMacroLetter(c, _builder.Length) ||
                t == Types.Variable && !Validator.IsLetter(c) && !Validator.IsDigit(c);

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
                    if (c == '$' && Defined.Macros.Count > 0)
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
                if (!Validator.IsMacroLetter(_builder[i], i))
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
                --_state.BracketCount;
                if (_state.BracketCount == _state.MacroArgs)
                    _state.MacroArgs = 0;
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
                _state.CurrentType = Types.Bracket;

            _builder.Append(c);
            Append(_state.CurrentType);
        }

        private void ParseOperator(char c)
        {
            Append(_state.PreviousTypeIfCurrentIsNone);
            _builder.Append(c);
            Append(Types.Operator);
            _state.CurrentType = Types.Operator;
            if (c == '=')
                GetLocalVariables((Run)_state.Paragraph.Inlines.LastInline, _state.CommandCount > 0);
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
                s = FormatOperator(s);
                if (s[0] == ' ')
                {
                    Run r = (Run)_state.Paragraph.Inlines.LastInline;
                    if (r is not null && r.Text == " ")
                        _state.Paragraph.Inlines.Remove(r);
                }
            }
            else if (t != Types.Error)
            {
                t = CheckError(t, ref s);
                if (t == Types.Error)
                    _state.CurrentType = Types.Error;
            }
            if (s is not null)
                AppendRun(t, s);

            _allowUnaryMinus = t == Types.Comment || t == Types.Operator || s == "(" || s == "{";
        }

        private bool AppendRelOperatorShortcut(string s)
        {
            if (s == "=" && _state.Paragraph.Inlines.Count > 0)
            {
                Run r = (Run)_state.Paragraph.Inlines.LastInline;
                switch (r.Text)
                {
                    case " = ": r.Text = " ≡ "; return true;
                    case "!": r.Text = " ≠ "; return true;
                    case " > ": r.Text = " ≥ "; return true;
                    case " < ": r.Text = " ≤ "; return true;
                };
            }
            return false;
        }

        private Types AppendInclude(string s)
        {
            if (File.Exists(s))
            {
                var sourceCode = UserDefined.Include(s, null);
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

        private Types CheckError(Types t, ref string s)
        {
            if (t == Types.Function)
            {
                if (!Functions.Contains(s) && !Defined.IsFunction(s, _state.Line))
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
                var r = (Run)_state.Paragraph.Inlines.LastInline;
                var pt = r is null ? Types.None : GetTypeFromColor(r.Foreground);
                if (pt == Types.Variable || pt == Types.Units)
                {
#if BG
                    _state.Message = "Невалиден синтаксис.";
#else
                    _state.Message = "Invalid syntax.";
#endif
                    r.Text += ' ' + s;
                    r.Foreground = Colors[(int)Types.Error];
                    r.Background = ErrorBackground;
                    r.ToolTip = _state.Message;
                    s = null;
                    return Types.Error;
                }
                if (!LocalVariables.Contains(s) && !Defined.IsVariable(s, _state.Line))
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
                if (!Defined.IsMacroOrParameter(s, _state.Line))
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
                      st.Equals("#include", StringComparison.OrdinalIgnoreCase) ||
                      st.Equals("#round", StringComparison.OrdinalIgnoreCase)))
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

                if (run.ToolTip is not null && t == Types.Include)
                {
                    run.Cursor = Cursors.Hand;
                    run.AddHandler(UIElement.MouseLeftButtonDownEvent, IncludeClickEventHandler);
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
                s = ((Run)inline).Text.Trim();
                if (Validator.IsVariable(s))
                {
                    inline.Background = null;
                    inline.Foreground = foreground;
                    name = s;
                    break;
                }
                inline = (Run)inline.PreviousInline;
            }
            if (!string.IsNullOrEmpty(name))
            {
                inline = (Run)inline.PreviousInline;
                while (inline != null)
                {
                    s = ((Run)inline).Text.Trim();
                    if (s == "@")
                        break;

                    inline = (Run)inline.PreviousInline;
                }
                if (s == "@")
                {
                    var bracketCount = 1;
                    inline = (Run)inline.PreviousInline;
                    while (inline != null)
                    {
                        s = ((Run)inline).Text.Trim();
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
                var s = ((Run)inline).Text.Trim();
                if (string.Equals(s, ")", StringComparison.Ordinal))
                    brackets = true;
                else if (string.Equals(s, "(", StringComparison.Ordinal))
                    return;
                else if (brackets)
                {
                    if (Validator.IsVariable(s))
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
                "+" or "=" or "≡" or "≠" or "<" or ">" or "≤" or "≥" or "&" or "@" or ":" => ' ' + name + ' ',
                ";" => name + ' ',
                _ => name,
            };
    }
}