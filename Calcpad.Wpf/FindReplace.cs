using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace Calcpad.Wpf
{
    internal class FindReplace
    {
        internal event EventHandler BeginSearch;
        internal event EventHandler EndSearch;
        internal event EventHandler BeginReplace;
        internal event EventHandler EndReplace;

        internal enum Directions
        {
            Up = -1,
            All = 0,
            Down = 1
        }

        internal enum Modes
        {
            Find,
            Replace
        }

        internal Modes Mode;
        internal string SearchString { get; set; }
        internal string ReplaceString { get; set; }
        internal string SelectedText => RichTextBox.Selection.Text;
        public Directions Direction
        {
            get => _direction;
            set
            {
                if (_direction != value)
                {
                    InitPosition();
                    _direction = value;
                }
            }
        }
        internal RichTextBox RichTextBox { get; set; }
        internal bool MatchCase { get; set; }
        internal bool WholeWords { get; set; }
        internal bool Selection { get; set; }
        internal List<string> SearchList { get; init; } = new();
        internal List<string> ReplaceList { get; init; } = new();

        private Directions _direction;
        private int _col;

        internal void InitPosition()
        {
            var tp = _direction == Directions.Up ?
            RichTextBox.Selection.Start :
            RichTextBox.Selection.End;
            var p = tp.Paragraph ?? RichTextBox.Document.Blocks.FirstBlock;
            _col = p.ContentStart.GetOffsetToPosition(tp);
            if (_direction == Directions.Up)
                _col -= 2;
        }

        internal void Find() => FindNext(false);
        internal void Replace() => FindNext(true);
        internal void ReplaceAll()
        {
            int count = 0;
            if (!InitSearch(true))
                return;

            TextPointer from, to;
            if (Selection)
            {
                from = RichTextBox.Selection.Start;
                to = RichTextBox.Selection.End;
            }
            else
            {
                from = RichTextBox.Document.ContentStart;
                to = RichTextBox.Document.ContentEnd;
            }
            var sb = new StringBuilder();
            var tr = new TextRange(from, to);
            var contentString = tr.Text;
            var lowerCaseContentString = MatchCase ? contentString : contentString.ToLowerInvariant();
            var len = SearchString.Length;
            var lowerCaseSearchString = MatchCase ? SearchString : SearchString.ToLowerInvariant();
            int i = 0, j = 0;
            BeginReplace(this, null);
            while (j >= 0)
            {
                j = lowerCaseContentString.IndexOf(lowerCaseSearchString, i, StringComparison.InvariantCulture);
                if (j >= 0)
                {
                    if (j > i)
                        sb.Append(contentString[i..j]);
                    if (IsWholeWord(contentString, j, j + len))
                    {
                        sb.Append(ReplaceString);
                        ++count;
                    }
                    else
                        sb.Append(SearchString);
                    i = j + len;
                }
            }
            if (count == 0 && sb.Length == 0)
            {
#if BG
                MessageBox.Show("Търсеният текст не е намерен.", "Търсене/Замяна");
#else
                MessageBox.Show("Search text not found.", "Find And Replace");
#endif
                if (sb.Length == 0)
                    return;
            }
            if (i < contentString.Length)
                sb.Append(contentString[i..]);

            tr.Text = sb.ToString();
            EndReplace(this, null);
#if BG
            MessageBox.Show($"Заменени са {count} срещания.", "Търсене/Замяна");
#else
            MessageBox.Show($"{count} matches were replaced.", "Find And Replace");
#endif
        }

        private void FindNext(bool replace)
        {
            if (!InitSearch(replace))
                return;

            var isStart = Direction != Directions.All;
            var p = RichTextBox.Selection.Start.Paragraph;
            if (p is null)
            {
                if (Direction == Directions.Up)
                    p = (Paragraph)RichTextBox.Document.Blocks.LastBlock;
                else
                    p = (Paragraph)RichTextBox.Document.Blocks.FirstBlock;
                isStart = true;
            }
            if (p is null)
                return;

            var len = SearchString.Length;
            var searchString = MatchCase ?
                SearchString :
                SearchString.ToLowerInvariant();

            while (p is not null)
            {
                var contentString = new TextRange(p.ContentStart, p.ContentEnd).Text;
                if (!MatchCase)
                    contentString = contentString.ToLowerInvariant();

                if (replace)
                {
                    string selectionString = RichTextBox.Selection.Text;
                    if (!string.IsNullOrEmpty(selectionString))
                    {
                        if (!MatchCase)
                            selectionString = selectionString.ToLowerInvariant();

                        if (selectionString == searchString)
                            RichTextBox.Selection.Text = ReplaceString;
                    }
                }
                if (Direction == Directions.Up)
                {
                    _col -= 2;
                    if (_col > contentString.Length)
                        _col = contentString.Length;

                    if (_col >= 0)
                        _col = contentString.LastIndexOf(searchString, _col, StringComparison.InvariantCulture);
                }
                else if (_col >= 0)
                {
                    if (_col > contentString.Length)
                        _col = contentString.Length;

                    _col = contentString.IndexOf(searchString, _col, StringComparison.InvariantCulture);
                }

                bool found = _col >= 0;
                if (found)
                    found = IsWholeWord(contentString, _col, _col + len);

                if (_col >= 0)
                    _col++;

                if (found)
                {
                    BeginSearch(this, null);
                    RichTextBox.Selection.Select(p.ContentStart, p.ContentEnd);
                    RichTextBox.Selection.ClearAllProperties();
                    RichTextBox.Selection.Select(p.ContentStart.GetPositionAtOffset(_col), p.ContentStart.GetPositionAtOffset(_col + len));
                    RichTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.LightBlue);
                    double y = RichTextBox.Selection.Start.GetCharacterRect(LogicalDirection.Forward).Bottom;
                    if (y < 0 || y > RichTextBox.ActualHeight)
                    {
                        DoEvents();
                        RichTextBox.ScrollToVerticalOffset(y + Math.CopySign(20, y));
                    }
                    EndSearch(this, null);
                    if (Direction == Directions.Up)
                        _col--;
                    return;
                }
                if (_col < 0)
                {
                    if (Direction == Directions.Up)
                        p = (Paragraph)p.PreviousBlock;
                    else
                        p = (Paragraph)p.NextBlock;

                    if (p is null)
                    {
#if BG
                        if (isStart)
                        {   
                            if (_direction == Directions.Up)
                                MessageBox.Show("Достигнато е началото на текста. Няма повече съвпадения.", "Търсене/Замяна");
                            else
                                MessageBox.Show("Достигнат е краят на текста. Няма повече съвпадения.", "Търсене/Замяна");

                            return;
                        }
#else
                        if (isStart)
                        {
                            if (_direction == Directions.Up)
                                MessageBox.Show("Start of text reached. There are no other occurrences.", "Find And Replace");
                            else
                                MessageBox.Show("End of text reached. There are no other occurrences. Search text not found.", "Find And Replace");

                            return;
                        }
#endif
                        isStart = true;
                        p = (Paragraph)RichTextBox.Document.Blocks.FirstBlock;
                    }
                    if (Direction == Directions.Up)
                        _col = p.ContentStart.GetOffsetToPosition(p.ContentEnd);
                    else
                        _col = 0;
                }
            }
        }

        private bool InitSearch(bool replace)
        {
            if (string.IsNullOrEmpty(SearchString))
            {
#if BG
                MessageBox.Show($"Търсеният текст е празен.", "Търсене/Замяна");
#else
                MessageBox.Show($"The search string is empty.", "Find And Replace");
#endif
                return false;
            }
            if (string.IsNullOrEmpty(SearchString) || replace && ReplaceString is null)
                return false;

            if (SearchString != SearchList.LastOrDefault())
                SearchList.Add(SearchString);

            if (replace && ReplaceString != ReplaceList.LastOrDefault())
                ReplaceList.Add(ReplaceString);

            return true;
        }

        private bool IsWholeWord(in string s, int from, int to) =>
            !WholeWords ||
            (from <= 0 || !char.IsLetter(s[from - 1])) &&
            (to >= s.Length || !char.IsLetter(s[to]));

        private static void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new Action(delegate { }));
        }
    }
}
