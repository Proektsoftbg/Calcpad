using Calcpad.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private static bool _isInMacros = false;
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
            Error,
            MacrosDefinition,
            Args,
            Macros
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
             Brushes.Crimson,
             Brushes.Blue,
             Brushes.Blue,
             Brushes.Blue
        };

        private static readonly Brush ErrorBackground = new SolidColorBrush(Color.FromRgb(255, 225, 225));
        
        private static readonly HashSet<char> Operators = new() { '!', '^', '/', '÷', '\\', '%', '*', '-', '+', '<', '>', '≤', '≥', '≡', '≠', '=' };

        private static readonly HashSet<char> Delimiters = new() { ';', '|', '&', '@', ':' };

        internal static readonly HashSet<string> Functions = new()
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
            "if",
            "switch",
            "take",
            "line",
            "spline"
        };

        internal static readonly HashSet<string> Conditions = new() { "#if", "#else", "#else if", "#end if", "#rad", "#deg", "#val", "#equ", "#show", "#hide", "#pre", "#post", "#repeat", "#loop", "#break", "#def", "#end def" };

        internal static readonly HashSet<string> Commands = new() { "$find", "$root", "$sup", "$inf", "$area", "$slope", "$repeat", "$sum", "$product", "$plot", "$map" };

        internal static readonly Dictionary<string, int> DefinedVariables = new();

        internal static readonly HashSet<string> LocalVariables = new();

        internal static readonly Dictionary<string, int> DefinedFunctions = new();

        internal static readonly Dictionary<string, int> DefinedMacros = new();

        internal static readonly HashSet<string> MacrosVariables = new();

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

        private static void GetValues(Paragraph p)
        {
            if (p.Tag is bool b && b)
            {
                values = new Queue<string>();
                foreach (var inline in p.Inlines)
                    if (inline.ToolTip is ToolTip tt)
                        values.Enqueue(tt.Content.ToString());
            }
            else
                values = null;
        }

        internal static void Clear(Paragraph p)
        {
            if (p is null)
                return;

            GetValues(p);
            if (values is not null)
                p.Tag = values;

            foreach (var inline in p.Inlines)
                inline.Background = null;   

            p.Background = Brushes.FloralWhite;
            p.BorderBrush = Brushes.NavajoWhite;
            p.BorderThickness = _thickness;
        }

        private static bool IsContinuedCondition(string text)
        {
            var t = text.ToLowerInvariant();
            return t == "#else" || t == "#end";
        }

        private static List<string> GetChunks(string text)
        {
            const char noComment = (char)0;
            var chunks = new List<string>();    
            var comment = noComment;
            int j = 0, len = text.Length;
            for(int i = 0; i < len; ++i)
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
            }
            --len;
            if (comment == noComment && j < len)
                chunks.Add(text[j..len]);

            return chunks;  
        }

        private static void GetLocalVariables(Paragraph p, bool isCommand)
        {
            var brackets = false;
            string name = null, s = null;
            Brush foreground = Colors[(int)Types.Variable];
            if (isCommand)
            {
                Run run = (Run)p.Inlines.LastInline.PreviousInline;
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
                foreach (Run run in p.Inlines)
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

        internal static void Parse(Paragraph p, bool isComplex, int line)
        {
            if (p is null)
                return;

            LocalVariables.Clear();
            _hasTargetUnitsDelimiter = false;
            if (p.Tag is Queue<string> queue)
                values = queue;
            else
                GetValues(p);

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
            var isArgs = false;
            var commandCount = 0;
            var bracketCount = 0;
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
                        Append(p, t, line);
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
                        Append(p, t, line);
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
                        Append(p, t, line);
                        t = Types.Comment;
                    }
                    else if (isTag)
                    {
                        if (isTagComment)
                            t = Types.Comment;
                        else
                            _stringBuilder.Append(c);

                        Append(p, t, line);
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
                        Append(p, t, line);
                        t = Types.Comment;
                        isTag = false;
                    }
                    else if (c == '<')
                    {
                        TagHelper.Initialize();
                        Append(p, t, line);
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
                        var nextType = Types.None;
                        if (_stringBuilder.ToString() == "#def")
                        {
                            nextType = Types.MacrosDefinition;
                        }
                        else if (_stringBuilder.ToString() == "#end def")
                        {
                            _isInMacros = false;
                        }
                        Append(p, t, line);
                        //Spaces are added only if they are leading
                        if (isLeading || t == Types.Condition)
                        {
                            _stringBuilder.Append(c);
                            Append(p, Types.None, line);
                        }
                        t = nextType;
                    }
                }
                else if ((c == ',' || c == ')') && isArgs)
                {
                    if (t != Types.Args)
                        t = Types.Error;

                    var sepType = Types.Bracket;
                    switch (c) {
                        case ',':
                            if (_stringBuilder.Length == 0 || _stringBuilder[^1] == ',')
                                sepType = Types.Error;
                            break;
                        case ')':
                            if (_stringBuilder.Length != 0 && _stringBuilder[^1] == ',')
                            {
                                sepType = Types.Error;
                            }
                            _isInMacros = true;
                            --bracketCount;
                            break;
                    }
                    Append(p, t, line);

                    _stringBuilder.Append(c);
                    Append(p, sepType, line);
                }
                else if (c == '{' || c == '(' || c == ')' || c == '}')
                {
                    if (c == '(')
                    {
                        if (t == Types.Variable)
                           t = Types.Function;
                        else if (t == Types.MacrosDefinition)
                            isArgs = true;
                        else if (t != Types.Operator && t != Types.Bracket && t != Types.Macros && t != Types.None)
                            t = Types.Error;
                        ++bracketCount;
                    }
                    else if (c == ')') {
                        --bracketCount;
                    }
                    Append(p, t, line);
                    if (c == '{')
                    {
                        if (t == Types.Command)
                            ++commandCount;

                        t = commandCount > 0 ? Types.Bracket : Types.Error;
                    }
                    else if (c == '}')
                    {
                        --commandCount;
                        t = commandCount >= 0 ? Types.Bracket : Types.Error;
                    }
                    else
                        t = Types.Bracket;

                    _stringBuilder.Append(c);
                    Append(p, t, line);
                }
                else if (Operators.Contains(c))
                {
                    Append(p, t, line);
                    _stringBuilder.Append(c);
                    Append(p, Types.Operator, line);
                    t = Types.Operator;
                    if (c == '=')
                        GetLocalVariables(p, commandCount > 0);
                }
                else if (Delimiters.Contains(c))
                {
                    Append(p, t, line);
                    _stringBuilder.Append(c);
                    if (commandCount > 0 || c == ';')
                        Append(p, Types.Operator, line);
                    else
                        Append(p, Types.Error, line);
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
                        else if (isArgs)
                            t = Types.Args;
                        else
                            if (t != Types.MacrosDefinition)
                                t = Types.Variable;
                    }
                    else if (c == '?')
                        t = Types.Input;
                    else if (t != Types.Comment)
                        t = Types.Error;

                    if (t != Types.Comment)
                        _stringBuilder.Append(c);

                    if (t == Types.Input)
                        Append(p, Types.Input, line);
                }
                else if (t == Types.Variable && c == '$')
                {
                    t = Types.Macros;
                    _stringBuilder.Append(c);
                }
                else if (isComplex && t == Types.Const && c == 'i')
                {
                    var j = i + 1;
                    if (j < len && s[j] == 'n')
                    {
                        Append(p, Types.Const, line);
                        _stringBuilder.Append(c);
                        t = Types.Units;
                    }
                    else
                    {
                        _stringBuilder.Append('i');
                        Append(p, Types.Const, line);
                    }
                }
                else if (t == Types.Const && IsLetter(c))
                {
                    Append(p, Types.Const, line);
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
            Append(p, t, line);
            if (commandCount > 0 || bracketCount > 0)
            {
                Run run = new(" ")
                {
                    Background = ErrorBackground
                };
                p.Inlines.Add(run);
            }
        }
        internal static void GetDefinedVariablesAndFunctions(string code, bool IsComplex) =>
            GetDefinedVariablesAndFunctions(code.Split('\n'), IsComplex);

        internal static void ClearDefinedVariablesAndFunctions(bool IsComplex)
        {
            DefinedVariables.Clear();
            DefinedFunctions.Clear();
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
        }

        internal static void GetDefinedVariablesAndFunctions(IEnumerable<string> lines, bool IsComplex)
        {
            ClearDefinedVariablesAndFunctions(IsComplex);
            DefinedMacros.Clear();
            var lineNumber = 0;
            var macrosDefinitionPattern = new Regex(MacrosNamePattern);
            foreach (var item in lines.Select((value, i) => new { i, value }))
            {
                var line = item.value;
                var index = item.i;

                GetDefinedVariablesAndFunctions(line, ++lineNumber);
                var match = macrosDefinitionPattern.Match(line);
                if (match.Success)
                {
                    var macro = match.Groups[1].Value;
                    DefinedMacros.TryAdd(macro, index);
                }
            }
        }


        internal static void GetDefinedVariablesAndFunctions(string code, int lineNumber)
        {
            var chunks = GetChunks(code);
            for (int i = 0; i < chunks.Count; ++i)
            {
                var chunk = chunks[i];
                var index = chunk.IndexOf('=');
                if (index > 0)
                {
                    int j, j0 = 0;
                    var c = chunk[0];
                    while (c == ' ')
                        c = chunk[++j0];

                    for (j = j0; j < index; ++j)
                    {
                        c = chunk[j];
                        if (!(IsLetter(c) || IsDigit(c)))
                            break;
                    }
                    var s = chunk[j0..j];
                    while (c == ' ')
                        c = chunk[++j];

                    if (c == '(')
                        DefinedFunctions.TryAdd(s, lineNumber);
                    else
                        DefinedVariables.TryAdd(s, lineNumber);
                }
            }
        }

        private static void Append(Paragraph p, Types t, int line)
        {
            if (_stringBuilder.Length == 0)
                return;

            var s = _stringBuilder.ToString();
            _stringBuilder.Clear();

            if (t == Types.Operator)
                s = FormatOperator(s);
            else if (t == Types.Input)
                s = "? ";
            else if (t == Types.Function)
            {
                if (!DefinedFunctions.TryGetValue(s, out int funcLine))
                    funcLine = int.MaxValue;

                if (!Functions.Contains(s) && funcLine > line)
                    t = Types.Error;
            }
            else if (t == Types.Variable)
            {
                int varLine = line;
                if (!LocalVariables.Contains(s) &&
                    !DefinedVariables.TryGetValue(s, out varLine))
                    varLine = int.MaxValue;

                if (varLine > line && !_isInMacros)
                {
                    if (MathParser.IsUnit(s))
                        t = Types.Units;
                    else
                        t = Types.Error;
                }
            }
            else if (t == Types.Macros)
            {
                var defLine = line;
                if (!DefinedMacros.TryGetValue(s, out defLine))
                    defLine = int.MaxValue;

                if (defLine > line)
                {
                    t = Types.Error;
                }
            }
            else if (t == Types.Units && !MathParser.IsUnit(s))
                t = Types.Error;
            else if (t == Types.MacrosDefinition && !s.EndsWith("$"))
                t = Types.Error;
            var run = new Run(s);
            s = s.ToLowerInvariant();

            if (t == Types.Condition && !Conditions.Contains(s.TrimEnd()) ||
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
                if (values is not null && values.Count > 0)
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

        private static bool IsVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            char c = name[0];
            if (!IsLetter(c) || "_,′″‴⁗".Contains(c))
                return false;

            for (int i = 1, len = name.Length ; i < len; ++i)
            {
                c = name[i];
                if (!(IsLetter(c) || IsDigit(c)))
                    return false;
            }
            return true;
        }

        internal static bool IsLetter(char c) =>
            c >= 'a' && c <= 'z' || // a - z
            c >= 'A' && c <= 'Z' || // A - Z 
            "_,°∡′″‴⁗ϑϕøØ".Contains(c) || // _ , ° ∡ ′ ″ ‴ ⁗ ϑ ϕ ø Ø
            c >= 'α' && c <= 'ω' ||   // alpha - omega
            c >= 'Α' && c <= 'Ω';  // Alpha - Omega

        internal static bool IsLatinLetter(char c) =>
            c >= 'a' && c <= 'z' || // a - z
            c >= 'A' && c <= 'Z'; // A - Z 

        internal static bool IsDigit(char c) =>
            c >= '0' && c <= '9' || c == MathParser.DecimalSymbol;

        private static string FormatOperator(string name) =>
            name switch
            {
                "-" => AllowUnaryMinus ? "-" : " - ",
                "+" or "=" or "≡" or "≠" or "<" or ">" or "≤" or "≥" or "&" or "@" or ":" => ' ' + name + ' ',
                ";" => name + ' ',
                _ => name,
            };
        private const string MacrosNamePattern = @"#def ([a-zA-Zα-ωΑ-Ω][a-zA-Zα-ωΑ-Ω,_′″‴⁗øØ°∡]*\$)\([a-zA-Z]*\)";
    }
}