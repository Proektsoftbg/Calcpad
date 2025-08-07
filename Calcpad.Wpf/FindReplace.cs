using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        internal WebView2 WebViewer { get; set; }
        internal bool IsWebView2Focused { get; set; }
        internal bool MatchCase { get; set; }
        internal bool WholeWords { get; set; }
        internal bool Selection { get; set; }
        internal List<string> SearchList { get; init; } = [];
        internal List<string> ReplaceList { get; init; } = [];

        private Directions _direction;
        private int _col;

        internal void InitPosition()
        {
            if (IsWebView2Focused)
                return;

            var tp = _direction == Directions.Up ?
            RichTextBox.Selection.Start :
            RichTextBox.Selection.End;
            var p = tp.Paragraph ?? RichTextBox.Document.Blocks.FirstBlock;
            _col = p.ContentStart.GetOffsetToPosition(tp);
            if (_direction == Directions.Up)
                _col -= 2;
        }
        private readonly SolidColorBrush _highlightBrush = new(Color.FromArgb(40, 0, 155, 255));
        internal void HighlightSelection()
        {
            if (IsWebView2Focused)
                return;

            BeginSearch(this, null);
            RichTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, _highlightBrush);
            EndSearch(this, null);
        }

        internal void ClearSelection()
        {
            if (IsWebView2Focused)
                return;

            BeginSearch(this, null);
            RichTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            EndSearch(this, null);
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
            var sb = new StringBuilder(10000);
            var tr = new TextRange(from, to);
            var contentString = tr.Text;
            var lowerCaseContentString = MatchCase ? contentString : contentString.ToLowerInvariant();
            var len = SearchString.Length;
            var lowerCaseSearchString = MatchCase ? SearchString : SearchString.ToLowerInvariant();
            int i = 0, j = 0;
            while (j >= 0)
            {
                j = lowerCaseContentString.IndexOf(lowerCaseSearchString, i, StringComparison.Ordinal);
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
                MessageBox.Show(FindReplaceResources.Search_text_not_found, FindReplaceResources.Find_And_Replace_Caption);
                if (sb.Length == 0)
                    return;
            }
            if (i < contentString.Length)
                sb.Append(contentString[i..]);

            tr.Text = sb.ToString();
            EndReplace(this, null);
            RichTextBox.Selection.Select(from, from);
            MessageBox.Show(string.Format(FindReplaceResources.count_matches_were_replaced, count), FindReplaceResources.Find_And_Replace_Caption);
        }

        private async void FindNext(bool replace)
        {
            if (IsWebView2Focused)
            {
                var matchCase = MatchCase.ToString().ToLowerInvariant();           
                var searchUpward = (Direction == FindReplace.Directions.Up).ToString().ToLowerInvariant();
                var wrapAround = (Direction == FindReplace.Directions.All).ToString().ToLowerInvariant();
                var wholeWord = WholeWords.ToString().ToLowerInvariant();
                var script = $"window.find('{SearchString.Trim()}', {matchCase}, {searchUpward}, {wrapAround}, {wholeWord})";
                var json = await WebViewer.ExecuteScriptAsync(script);
                var isFound = JsonSerializer.Deserialize<bool>(json);
                if (!isFound)
                    MessageBox.Show(FindReplaceResources.Search_text_not_found, FindReplaceResources.Find_And_Replace_Caption);

                return;
            }

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

                var found = _col >= 0;
                if (found)
                    found = IsWholeWord(contentString, _col, _col + len);

                if (_col >= 0)
                    _col++;

                if (found)
                {
                    RichTextBox.Selection.Select(p.ContentStart, p.ContentEnd);
                    BeginSearch(this, null);
                    RichTextBox.Selection.ClearAllProperties();
                    RichTextBox.Selection.Select(p.ContentStart.GetPositionAtOffset(_col), p.ContentStart.GetPositionAtOffset(_col + len));
                    var y = RichTextBox.Selection.Start.GetCharacterRect(LogicalDirection.Forward).Bottom;
                    var h = RichTextBox.ActualHeight;
                    if (y < 0 || y > h)
                    {
                        y -= RichTextBox.Document.ContentStart.GetCharacterRect(LogicalDirection.Forward).Bottom;
                        DoEvents();
                        RichTextBox.ScrollToVerticalOffset(y - h / 4d);
                    }
                    HighlightSelection();
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
                        if (isStart)
                        {

                            MessageBox.Show(_direction == Directions.Up ?
                                FindReplaceResources.Start_of_text_reached_There_are_no_other_occurrences :
                                FindReplaceResources.End_of_text_reached_There_are_no_other_occurrences,
                                FindReplaceResources.Find_And_Replace_Caption);
                            return;
                        }
                        isStart = true;
                        p = (Paragraph)RichTextBox.Document.Blocks.FirstBlock;
                    }
                    _col = Direction == Directions.Up ?
                        p.ContentStart.GetOffsetToPosition(p.ContentEnd) :
                        0;
                }
            }
        }

        private bool InitSearch(bool replace)
        {
            if (string.IsNullOrEmpty(SearchString))
            {
                MessageBox.Show(FindReplaceResources.The_search_string_is_empty, FindReplaceResources.Find_And_Replace_Caption);
                return false;
            }
            if (replace && ReplaceString is null)
                ReplaceString = string.Empty;

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
