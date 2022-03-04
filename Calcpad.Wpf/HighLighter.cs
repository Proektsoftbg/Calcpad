using Calcpad.Core;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Calcpad.Wpf
{
    internal static class HighLighter
    {
        internal static MouseButtonEventHandler InputClickEventHandler;
        private static readonly StringBuilder _stringBuilder = new(100);
        private static bool AllowUnaryMinus = true;
        private static Queue<string> values = null;
        private static bool _hasTargetUnitsDelimiter;
        private static Thickness _thickness = new(0.5);
        private enum Types
        {
            None,
            Const,
            Units,
            Operator,
            Variable,
            Function,
            Condition,
            Command,
            Bracket,
            Comment,
            Tag,
            Input,
            Error
        }

        private static readonly Brush[] Colors =
        {
             Brushes.Black,
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
             Brushes.Crimson,
             Brushes.Crimson
        };

        private static readonly Brush ErrorBackground = new SolidColorBrush(Color.FromRgb(255, 225, 225));
        
        private static readonly HashSet<char> Operators = new() { '!', '^', '/', '÷', '\\', '%', '*', '-', '+', '<', '>', '≤', '≥', '≡', '≠', '=' };

        private static readonly HashSet<char> Delimiters = new() { ';', '|', '&', '@', ':' };

        private static readonly HashSet<string> Functions = new()
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
            "sqr",
            "sqrt",
            "cbrt",
            "root",
            "round",
            "floor",
            "ceiling",
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
            "switch",
            "take",
            "line",
            "spline"
        };

        private static readonly HashSet<string> Conditions = new() { "#if", "#else", "#else if", "#end if", "#rad", "#deg", "#val", "#equ", "#show", "#hide", "#pre", "#post", "#repeat", "#loop", "#break" };

        private static readonly HashSet<string> Commands = new() { "$find", "$root", "$sup", "$inf", "$area", "$slope", "$repeat", "$sum", "$product", "$plot", "$map" };

        private static class TagHelper
        {
            internal enum Tags
            {
                None,
                Starting,
                Closing,
                SelfClosing
            }

            internal static Tags Type { get; private set; } = Tags.None;

            internal static void Initialize()
            {
                Type = Tags.None;
            }
            internal static bool CheckTag(char c, ref Types t)
            {
                if (t == Types.Comment)
                {
                    if (Type == Tags.None)
                    {
                        if (c == ' ')
                        {
                            if (_stringBuilder.Length == 1)
                                return false;
                            else
                                Type = Tags.Starting;
                        }
                        else if (c == '/')
                        {
                            if (_stringBuilder.Length == 1)
                                Type = Tags.Closing;
                            else
                                Type = Tags.SelfClosing;
                        }
                        else if (!(IsLatinLetter(c) || char.IsDigit(c) && _stringBuilder.Length == 2 && _stringBuilder[1] == 'h'))
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

        internal static void Clear(Paragraph p)
        {
            if (p is null)
                return;

            values = null;
            if (p.Tag is bool b && b)
            {
                foreach (var inline in p.Inlines)
                {
                    if (inline.ToolTip is ToolTip tt)
                    {
                        if (values is null)
                            values = new Queue<string>();

                        values.Enqueue(tt.Content.ToString());
                    }
                }
                p.Tag = values;
            }
            foreach (var inline in p.Inlines)
            {
                inline.Background = null;   
            }
            p.Background = Brushes.FloralWhite;
            p.BorderBrush = Brushes.NavajoWhite;
            p.BorderThickness = _thickness;
            var tr = new TextRange(p.ContentStart, p.ContentEnd);
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }

        private static bool IsContinuedCondition(string text)
        {
            var t = text.ToLowerInvariant();
            return t == "#else" || t == "#end";
        }

        internal static void Parse(Paragraph p, bool isComplex)
        {
            if (p is null)
                return;

            _hasTargetUnitsDelimiter = false;
            if (p.Tag is Queue<string> queue)
                values = queue;
            else
                values = null;

            p.Tag = false;
            p.Background = null;
            p.BorderBrush = null;
            p.BorderThickness = _thickness;
            var s = new TextRange(p.ContentStart, p.ContentEnd).Text;
            p.Inlines.Clear();
            _stringBuilder.Clear();
            const char noComment = (char)0;
            var textComment = noComment;
            var tagComment = noComment;
            var isLeading = true;
            var isUnits = false;
            var isPlot = false;
            var isTag = false;
            var isTagComment = false;
            var isCommand = 0;
            var st = s.TrimStart();
            if (!string.IsNullOrEmpty(st) && st[0] == '$')
                isPlot = st.ToLowerInvariant().StartsWith("$plot");
            AllowUnaryMinus = true;
            Types t = Types.None, pt = Types.None;
            for (int i = 0, len = s.Length; i < len; ++i)
            {
                var c = s[i];
                //Check for leading spaces.
                if (c != ' ' && c != '\t')
                    isLeading = false;

                if (!(isPlot || t == Types.Comment) && c == '|')
                    isUnits = true;

                if (c >= 'А' && c <= 'я')
                {
                    if (textComment == noComment)
                    {
                        Append(p, t);
                        textComment = p.PreviousBlock is null ? '"' : '\'';
                        _stringBuilder.Clear();
                        _stringBuilder.Append(textComment);
                        t = Types.Comment;
                    }
                }
                else if (c == '\'' || c == '"')
                {
                    if (textComment == noComment)
                    {
                        isUnits = false;
                        textComment = c;
                        tagComment = c == '\'' ? '"' : '\'';
                        Append(p, t);
                        if (isTag)
                        {
                            if (TagHelper.Type == TagHelper.Tags.Starting)
                                t = Types.Tag;
                            else
                            {
                                t = Types.Comment;
                                isTag = false;
                            }
                        }
                        else
                            t = Types.Comment;
                    }
                    else if (textComment == c)
                    {
                        textComment = noComment;
                        if (isTag && isTagComment)
                            t = Types.Comment;
                        _stringBuilder.Append(c);
                        Append(p, t);
                        t = Types.Comment;
                    }
                    else if (isTag)
                    {
                        if (isTagComment)
                            t = Types.Comment;
                        else
                            _stringBuilder.Append(c);

                        Append(p, t);
                        isTagComment = !isTagComment;
                    }
                }

                if (textComment != noComment)
                {
                    if (c == '>')
                    {
                        _stringBuilder.Append(c);
                        if (isTag)
                            t = Types.Tag;
                        Append(p, t);
                        t = Types.Comment;
                        isTag = false;
                    }
                    else if (c == '<')
                    {
                        TagHelper.Initialize();
                        Append(p, t);
                        _stringBuilder.Append(c);
                        isTag = true;
                    }
                    else
                    {
                        if (isTag)
                            isTag = TagHelper.CheckTag(c, ref t);

                        if (!(isTagComment && c == tagComment))
                            _stringBuilder.Append(c);
                    }
                }
                else if (c == ' ' || c == '\t')
                {
                    if (IsContinuedCondition(_stringBuilder.ToString()))
                        _stringBuilder.Append(' ');
                    else
                    {
                        Append(p, t);
                        //Spaces are added only if they are leading
                        if (isLeading || t == Types.Condition)
                        {
                            _stringBuilder.Append(c);
                            Append(p, Types.None);
                        }
                        t = Types.None;
                    }
                }
                else if (c == '{' || c == '(' || c == ')' || c == '}')
                {
                    if (c == '(')
                    {
                        if (t == Types.Variable)
                            t = Types.Function;
                        else if (t != Types.Operator && t != Types.Bracket && t != Types.None)
                            t = Types.Error;
                    }
                    Append(p, t);
                    if (c == '{')
                    {
                        if (t == Types.Command)
                            ++isCommand;

                        t = isCommand > 0 ? Types.Bracket : Types.Error;
                    }
                    else if (c == '}')
                    {
                        --isCommand;
                        t = isCommand >= 0 ? Types.Bracket : Types.Error;
                    }
                    else
                        t = Types.Bracket;

                    _stringBuilder.Append(c);
                    Append(p, t);
                }
                else if (Operators.Contains(c))
                {
                    Append(p, t);
                    _stringBuilder.Append(c);
                    Append(p, Types.Operator);
                    t = Types.Operator;
                }
                else if (Delimiters.Contains(c))
                {
                    Append(p, t);
                    _stringBuilder.Append(c);
                    if (isCommand > 0 || c == ';')
                        Append(p, Types.Operator);
                    else
                        Append(p, Types.Error);
                    t = Types.Bracket;
                }
                else if (_stringBuilder.Length == 0)
                {
                    if (c == '$')
                        t = Types.Command;
                    else if (c == '#')
                        t = Types.Condition;
                    else if (IsDigit(c))
                        t = Types.Const;
                    else if (IsLetter(c))
                    {
                        if (_stringBuilder.Length == 0 && !(char.IsLetter(c) || c == '°' || c == '∡'))
                            t = Types.Error;
                        else if (isUnits || pt == Types.Const || c == '°')
                            t = Types.Units;
                        else
                            t = Types.Variable;
                    }
                    else if (c == '?')
                        t = Types.Input;
                    else if (t != Types.Comment)
                        t = Types.Error;

                    if (t != Types.Comment)
                        _stringBuilder.Append(c);

                    if (t == Types.Input)
                        Append(p, Types.Input);
                }
                else if (isComplex && t == Types.Const && c == 'i')
                {
                    var j = i + 1;
                    if (j < len && s[j] == 'n')
                    {
                        Append(p, Types.Const);
                        _stringBuilder.Append(c);
                        t = Types.Units;
                    }
                    else
                    {
                        _stringBuilder.Append('i');
                        Append(p, Types.Const);
                    }
                }
                else if (t == Types.Const && IsLetter(c))
                {
                    Append(p, Types.Const);
                    _stringBuilder.Append(c);
                    if (char.IsLetter(c) || c == '°')
                        t = Types.Units;
                    else
                        t = Types.Error;
                }
                else
                {
                    if (t == Types.Const && !IsDigit(c) ||
                        t == Types.Variable && !IsLetter(c) && !IsDigit(c))
                        t = Types.Error;

                    _stringBuilder.Append(c);
                }
                if (t != Types.None)
                    pt = t;
            }
            Append(p, t);
        }

        private static void Append(Paragraph p, Types t)
        {
            if (_stringBuilder.Length == 0)
                return;

            var s = _stringBuilder.ToString();
            _stringBuilder.Clear();

            if (t == Types.Operator)
                s = FormatOperator(s);
            else if (t == Types.Input)
                s = "? ";

            if (t == Types.Units && !MathParser.IsUnit(s))
                t = Types.Error;

            var run = new Run(s);
            s = s.ToLowerInvariant();
            if (t == Types.Variable && Functions.Contains(s) ||
                t == Types.Condition && !Conditions.Contains(s.TrimEnd()) ||
                t == Types.Command && !Commands.Contains(s))
                t = Types.Error;

            run.Foreground = Colors[(int)t];
            if (t == Types.Error)
            {
                if (s == "|" && !_hasTargetUnitsDelimiter)
                    _hasTargetUnitsDelimiter = true;
                else
                    run.Background = ErrorBackground;
            }
            else if (t == Types.Function)
                run.FontWeight = FontWeights.Bold;
            else if (t == Types.Input)
            {
                var tt = new ToolTip();
                if (values != null && values.Count > 0)
                    tt.Content = values.Dequeue();
                else
                    tt.Content = "0";

                tt.Placement = PlacementMode.MousePoint;
                tt.HorizontalOffset = 8;
                tt.VerticalOffset = -32;
                tt.HorizontalAlignment = HorizontalAlignment.Left;
                run.ToolTip = tt;
                run.Cursor = Cursors.Hand;
                run.FontWeight = FontWeights.Bold;
                run.AddHandler(UIElement.MouseDownEvent, InputClickEventHandler);
                p.Tag = true;
            }
            p.Inlines.Add(run);
            AllowUnaryMinus = t == Types.Operator || s == "(" || s == "{";
        }
        private static bool IsLetter(char c) =>
            c >= 'a' && c <= 'z' || // a - z
            c >= 'A' && c <= 'Z' || // A - Z 
            "_,°∡′″‴⁗ϑϕøØ".Contains(c) || // _ , ° ∡ ′ ″ ‴ ⁗ ϑ ϕ ø Ø
            c >= 'α' && c <= 'ω' ||   // alpha - omega
            c >= 'Α' && c <= 'Ω';  // Alpha - Omega

        private static bool IsLatinLetter(char c) =>
            c >= 'a' && c <= 'z' || // a - z
            c >= 'A' && c <= 'Z'; // A - Z 

        private static bool IsDigit(char c) =>
            c >= '0' && c <= '9' || c == MathParser.DecimalSymbol;

        private static string FormatOperator(string name) =>
            name switch
            {
                "-" => AllowUnaryMinus ? "-" : " - ",
                "+" or "=" or "≡" or "≠" or "<" or ">" or "≤" or "≥" or "&" or "@" or ":" => ' ' + name + ' ',
                ";" => name + ' ',
                _ => name,
            };
    }
}