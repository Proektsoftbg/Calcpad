using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Calcpad.Wpf
{
    internal class InsertManager
    {
        private readonly RichTextBox _richTextBox;
        internal InsertManager(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
        }

        internal void InsertHtmlHeading(string tag)
        {
            var p = _richTextBox.Selection.Start.Paragraph;
            var start = p.ContentStart;
            var end = p.ContentEnd;
            var tr = new TextRange(start, end);
            var s = tr.Text.Trim();
            var len = s.Length;
            var parts = tag.Split('‖');
            var isComment = s.AsSpan().Count('\'') % 2 == 1;
            int n1, n2;
            if (s.StartsWith('\''))
            {
                (n1, n2) = RemoveHeadingTags(s);
                var span = s.AsSpan(n1, len - n2 - n1);
                if (s.StartsWith($"'{parts[0]}", StringComparison.OrdinalIgnoreCase) &&
                    (s.EndsWith(parts[1], StringComparison.OrdinalIgnoreCase) ||
                     s.EndsWith($"{parts[1]}'", StringComparison.OrdinalIgnoreCase)))
                {
                    tr.Text = $"'{span}";
                    n1 = 2;
                    n2 = 0;
                }
                else
                {
                    if (isComment)
                        tr.Text = $"'{parts[0]}{span}{parts[1]}";
                    else
                        tr.Text = $"'{parts[0]}{span}'{parts[1]}";

                    n1 = parts[0].Length + 2;
                    n2 = parts[1].Length + 1;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(s))
                    tr.Text = $"'{parts[0]}{parts[1]}";
                else if (isComment)
                    tr.Text = $"'{parts[0]}'{s}{parts[1]}";
                else
                    tr.Text = $"'{parts[0]}'{s}'{parts[1]}";
                n1 = parts[0].Length + 2;
                n2 = parts[1].Length + 1;
            }
            start = p.ContentStart.GetPositionAtOffset(n1);
            end = p.ContentEnd.GetPositionAtOffset(-n2);
            if (end is not null)
                _richTextBox.Selection.Select(start ?? end, end);
        }

        internal static (int, int) RemoveHeadingTags(string s)
        {
            int n1 = 1, n2 = 0;
            if (s.StartsWith("'<p>", StringComparison.OrdinalIgnoreCase))
            {
                n1 = 4;
                if (s.EndsWith("</p>", StringComparison.OrdinalIgnoreCase))
                    n2 = 4;
                else if (s.EndsWith("</p>'", StringComparison.OrdinalIgnoreCase))
                    n2 = 5;
                else
                    n1 = 1;
            }
            else if (s.StartsWith("'<h", StringComparison.OrdinalIgnoreCase))
            {
                n1 = s.IndexOf('>') + 1;
                var c = s[n1 - 2];
                if (s.EndsWith('>') || s.EndsWith(">'"))
                {
                    n2 = s.LastIndexOf('<');
                    if (s.Length - n2 < 5 || char.ToLowerInvariant(s[n2 + 2]) != 'h' && s[n2 + 3] != c)
                    {
                        n1 = 1;
                        n2 = 0;
                    }
                    else
                        n2 = s.Length - n2;
                }
                else
                    n1 = 1;
            }
            return (n1, n2);
        }

        internal void InsertMarkdownHeading(ReadOnlySpan<char> tag)
        {
            var p = _richTextBox.Selection.Start.Paragraph;
            var offset = p.ContentStart.GetOffsetToPosition(_richTextBox.Selection.Start);
            var start = p.ContentStart;
            var end = p.ContentEnd;
            var tr = new TextRange(start, end);
            var len0 = tr.Text.Length;
            var s = tr.Text.Trim();
            var len = s.Length;
            tag = tag[1..];
            if (s.StartsWith('\''))
            {
                var n = 0;
                while (++n < len && s[n] == '#') ;
                if (n < len - 1 && s[n] == ' ') ++n;
                var span = s.AsSpan(n);
                if (n < len)
                    --n;

                if (n == tag.Length)
                    tr.Text = $"'{span}";
                else
                    tr.Text = $"'{tag}{span}";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(s))
                    tr.Text = $"'{tag}";
                else
                    tr.Text = $"'{tag}'{s}";
            }
            end = p.ContentStart.GetPositionAtOffset(offset + tr.Text.Length - len0);
            if (end is not null)
                _richTextBox.Selection.Select(end, end);
        }

        internal bool InsertInline(string tag)
        {
            var parts = tag.Split('‖');
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[0]) && !string.IsNullOrEmpty(parts[1]))
                SelectWord();

            var start = _richTextBox.Selection.Start;
            var end = _richTextBox.Selection.End;
            var p = end.Paragraph ?? start.Paragraph;
            var offset = end.GetOffsetToPosition(p.ContentEnd);
            var len = start.GetOffsetToPosition(end);
            if (offset == 0)
                offset = 1;

            bool isComment = false;
            if (!ReferenceEquals(start.Paragraph, p))
                return false;

            if (TryRemoveTags(parts[0], parts[1]))
                return true;

            if (start.GetOffsetToPosition(end) == 0)
                isComment = InsertComment(end, parts[0] + parts[1]);
            else
            {
                if (parts[0].Length > 0)
                    isComment = InsertComment(start, parts[0]);
                if (parts.Length > 1 && parts[1].Length > 0)
                    isComment = InsertComment(end, parts[1]);
            }
            offset += parts[1].Length + (isComment ? 0 : 1);
            end = p.ContentEnd.GetPositionAtOffset(-offset);
            if (end is not null)
            {
                start = end.GetPositionAtOffset(-len);
                _richTextBox.Selection.Select(start ?? end, end);
            }
            return true;
        }

        private void SelectWord()
        {
            var start = _richTextBox.Selection.Start;
            var end = _richTextBox.Selection.End;
            if (start.GetOffsetToPosition(end) == 0)
            {
                var s1 = start.GetTextInRun(LogicalDirection.Backward);
                var s2 = end.GetTextInRun(LogicalDirection.Forward);
                if (!(s1 is null || s2 is null))
                {
                    int n1 = s1.Length, n2 = s2.Length;
                    int i1 = n1, i2 = -1;
                    while (--i1 >= 0 && char.IsLetter(s1[i1])) ;
                    while (++i2 < n2 && char.IsLetter(s2[i2])) ;
                    i1 -= n1 - 1;
                    if (i1 < 0 || i2 > 0)
                    {
                        if (i1 < 0) start = start.GetPositionAtOffset(i1);
                        if (i2 > 0) end = end.GetPositionAtOffset(i2);
                        if (start is not null && end is not null)
                            _richTextBox.Selection.Select(start, end);
                    }
                }
            }
        }

        private bool TryRemoveTags(string t1, string t2)
        {
            var start = _richTextBox.Selection.Start;
            var end = _richTextBox.Selection.End;
            var s1 = start.GetTextInRun(LogicalDirection.Backward);
            var s2 = end.GetTextInRun(LogicalDirection.Forward);
            if (!(string.IsNullOrEmpty(s1) && !string.IsNullOrEmpty(t1) ||
                  string.IsNullOrEmpty(s2) && !string.IsNullOrEmpty(t2)))
            {
                var ts1 = s1.AsSpan();
                if (!string.IsNullOrEmpty(t1))
                    ts1 = ts1.TrimEnd();

                var ts2 = s2.AsSpan();
                if (!string.IsNullOrEmpty(t2))
                    ts2 = ts2.TrimStart();

                if (ts1.EndsWith(t1, StringComparison.OrdinalIgnoreCase) &&
                    ts2.StartsWith(t2, StringComparison.OrdinalIgnoreCase))
                {
                    var n1 = s1.Length - ts1.Length;
                    var n2 = s2.Length - ts2.Length;
                    var s = _richTextBox.Selection.Text;
                    s = s.PadLeft(s.Length + n1, ' ').PadRight(s.Length + n2, ' ');
                    n1 += t1.Length;
                    n2 += t2.Length;
                    var p = end.Paragraph ?? start.Paragraph;
                    var tr = new TextRange(p.ContentStart, p.ContentEnd);
                    tr.Text = string.Concat(s1.AsSpan(0, s1.Length - n1), s, s2.AsSpan(n2));
                    start = p.ContentStart.GetPositionAtOffset(s1.Length - n1 + 1);
                    end = p.ContentEnd.GetPositionAtOffset(n2 - s2.Length - 1);
                    if (start is not null && end is not null)
                    {
                        if (start.GetOffsetToPosition(end) > 0)
                            _richTextBox.Selection.Select(start, end);
                        else
                            _richTextBox.Selection.Select(end, end);

                        return true;
                    }
                }
            }
            return false;
        }

        private static bool InsertComment(TextPointer tp, string comment)
        {
            var ts = tp.Paragraph?.ContentStart ?? tp;
            var s = new TextRange(ts, tp).Text;
            var isComment = s.AsSpan().Count('\'') % 2 == 1;
            if (isComment)
                tp.InsertTextInRun(comment);
            else if (tp.GetOffsetToPosition(tp.Paragraph.ContentEnd) == 0)
                tp.InsertTextInRun('\'' + comment);
            else
                tp.InsertTextInRun('\'' + comment + '\'');

            return isComment;
        }

        internal void RemoveChar() => _richTextBox.Selection.Start.DeleteTextInRun(-1);
        internal void InsertLine() => _richTextBox.CaretPosition = _richTextBox.Selection.Start.InsertParagraphBreak();
        internal void InsertText(string text)
        {
            var sel = _richTextBox.Selection;
            sel.Text = text;
            sel.ClearAllProperties();
            SelectInsertedText(text);
        }

        internal void SelectInsertedText(string text)
        {
            var sel = _richTextBox.Selection;
            var i1 = text.IndexOf('{') + 1;
            if (i1 > 0)
            {
                var i2 = text.IndexOfAny(['@', '}'], i1) - 1;
                var tpEnd = i2 < 0 ? sel.End : sel.Start.GetPositionAtOffset(i2);
                sel.Select(sel.Start.GetPositionAtOffset(i1), tpEnd);
                return;
            }
            i1 = text.IndexOf('(') + 1;
            if (i1 > 0)
            {
                var i2 = text.IndexOfAny([';', ')'], i1);
                var tpEnd = i2 < 0 ? sel.End : sel.Start.GetPositionAtOffset(i2);
                sel.Select(sel.Start.GetPositionAtOffset(i1), tpEnd);
                return;
            }
            sel.Select(sel.End, sel.End);
        }
    }
}
