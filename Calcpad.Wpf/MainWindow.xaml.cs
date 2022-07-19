using Calcpad.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TextElement = System.Windows.Documents.TextElement;

namespace Calcpad.Wpf
{
    public partial class MainWindow : Window
    {
        internal readonly struct AppInfo
        {
            static AppInfo()
            {
                Path = AppDomain.CurrentDomain.BaseDirectory;
                Name = AppDomain.CurrentDomain.FriendlyName + ".exe";
                FullName = System.IO.Path.Combine(Path, Name);
                Version = string.Join(".", FileVersionInfo.GetVersionInfo(FullName).ProductVersion.Split('.').Take(3));
                Title = " Calcpad " + Version;
            }
            internal static readonly string Path;
            internal static readonly string Name;
            internal static readonly string FullName;
            internal static readonly string Version;
            internal static readonly string Title;
        }
        private const double AutoIndentStep = 28.0;
        private string _cfn;
        private readonly ExpressionParser _parser;
        private readonly string _htmlWorksheet;
        private readonly string _htmlParsing;
        private readonly string _htmlHelp;
        private readonly string _svgTyping;
        private readonly StringBuilder _htmlBuilder = new();
        private readonly FindReplace _findReplace = new();
        private FindReplaceWindow _findReplaceWindow = null;
        private bool _mustPromptUnlock;
        private bool _forceHighlight;
        private readonly FlowDocument _document;
        private Paragraph _currentParagraph;
        private readonly UndoManager _undoMan;
        private readonly WebBrowserWrapper _wbWarper;
        private Task _parseTask;
        private string DocumentPath { get; set; }
        private bool _isParsing = false;
        private bool _isFileOpen = false;
        private string CurrentFileName
        {
            get => _cfn;
            set
            {
                _cfn = value;
                if (value.Length == 0)
                    Title = AppInfo.Title;
                else
                {
                    DocumentPath = Path.GetDirectoryName(value);
                    Directory.SetCurrentDirectory(DocumentPath ?? string.Empty);
                    Title = AppInfo.Title + " - " + Path.GetFileName(value);
                }
            }
        }
        private bool IsComplex => _parser.Settings.Math.IsComplex;
        private bool IsSaved
        {
            get => _isSaved;
            set
            {
                SaveButton.IsEnabled = !value;
                MenuSave.IsEnabled = !value;
                _isSaved = value;
            }
        }
        private bool _isSaved;
        private bool _isTextChangedEnabled;
        private bool _isSaving;
        private bool _forceBackSpace;
        private bool _isPasting;
        private int _pasteOffset;
        private TextPointer _pasteEnd;
        private bool _scrollOutput;
        private double _scrollY;
        private bool _autoRun;
        private readonly double _screenScaleFactor;
        private readonly int _autoCompleteCount;
        private bool IsWebForm
        {
            get => WebFormButton.Tag.ToString() == "T";
            set
            {
                if (_mustPromptUnlock && !value)
                {
#if BG
                    const string message = "Сигурни ли сте, че искате да отключите изходния код за редактиране?";
#else
                    const string message = "Are you sure you want to unlock the source code for editing?";
#endif
                    if (MessageBox.Show(message, Title, MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;

                    _mustPromptUnlock = false;
                }
                IsCalculated = false;
                SetButton(WebFormButton, value);
                SetUILock(value);
                if (value)
                {
                    if (!_isFileOpen)
                        ReadAndSetInputFields();

                    CalculateAsync(true);
                    InputFrame.Visibility = Visibility.Hidden;
                    FramesGrid.ColumnDefinitions[0].Width = new GridLength(0);
                    FramesGrid.ColumnDefinitions[1].Width = new GridLength(0);
#if BG
                    OutputFrame.Header = "Входни данни";
                    WebFormButton.ToolTip = "Отвори програмния код за редактиране";
#else
                    OutputFrame.Header = "Input";
                    WebFormButton.ToolTip = "Open source code for editing";
#endif
                    MenuWebForm.Icon = "  ✓";
                    AutoRunCheckBox.Visibility = Visibility.Hidden;
                    _findReplaceWindow?.Close();
                }
                else
                {
                    var cursor = WebBrowser.Cursor;
                    WebBrowser.Cursor = Cursors.Wait;
                    ForceHighlight();
                    Task.Run(DispatchLineNumbers);
                    SaveInputToCode(_wbWarper.GetInputVaues());
                    InputFrame.Visibility = Visibility.Visible;
                    FramesGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    FramesGrid.ColumnDefinitions[1].Width = new GridLength(5);
                    FramesGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
#if BG
                    OutputFrame.Header = "Резултати";
                    WebFormButton.ToolTip = "Компилирай до форма за вход на данни";
#else
                    OutputFrame.Header = "Output";
                    WebFormButton.ToolTip = "Compile to input form";
#endif
                    MenuWebForm.Icon = null;
                    WebBrowser.Cursor = cursor;
                    AutoRunCheckBox.Visibility = Visibility.Visible;
                    if (IsAutoRun)
                    {
                        CalculateAsync(false);
                        IsCalculated = true;
                    }
                }
            }
        }
        private string InputText => new TextRange(_document.ContentStart, _document.ContentEnd).Text;
        private int InputTextLength => _document.ContentEnd.GetOffsetToPosition(_document.ContentStart);
        private bool IsCalculated
        {
            get => CalcButton.Tag.ToString() == "T";
            set
            {
                SetButton(CalcButton, value && InputTextLength != 0);
                if (IsWebForm)
                {
                    WebFormButton.IsEnabled = !IsCalculated;
                    MenuWebForm.IsEnabled = WebFormButton.IsEnabled;
                }
                MenuCalculate.Icon = IsCalculated ? "  ✓" : null;
            }
        }
        private static readonly char[] GreekLetters = { 'α', 'β', 'χ', 'δ', 'ε', 'φ', 'γ', 'η', 'ι', 'ø', 'κ', 'λ', 'μ', 'ν', 'ο', 'π', 'θ', 'ρ', 'σ', 'τ', 'υ', 'ϑ', 'ω', 'ξ', 'ψ', 'ζ' };
        private static readonly char[] LatinLetters = { 'a', 'b', 'g', 'd', 'e', 'z', 'h', 'q', 'i', 'k', 'l', 'm', 'n', 'x', 'o', 'p', 'r', 's', 's', 't', 'u', 'f', 'c', 'y', 'w'};

        public MainWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
             _parser = new ExpressionParser();
            InitializeComponent();
            HighLighter.InputClickEventHandler = new MouseButtonEventHandler(Input_Click);
            LineNumbers.ClipToBounds = true;
            DocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Calcpad";
            if (!Directory.Exists(DocumentPath))
                Directory.CreateDirectory(DocumentPath);
            Directory.SetCurrentDirectory(DocumentPath ?? string.Empty);
            var appUrl = "file:///" + AppInfo.Path.Replace("\\", "/");
            _htmlWorksheet = ReadFile(AppInfo.Path + "template.html").Replace("jquery", appUrl + "jquery");
            _htmlParsing = ReadFile(AppInfo.Path + "parsing.html");
#if BG
            var s = GetHelp("https://proektsoft.bg/calcpad/help.html");
#else
            var s = GetHelp("https://proektsoft.bg/calcpad/help.en.html");
#endif
            //_htmlHelp = s.Replace("readme.html", appUrl + "readme.html");
            _htmlHelp = ReadFile(AppInfo.Path + "help.html").Replace("readme.html", appUrl + "readme.html");
            _svgTyping = $"<img style=\"height:1em;\" src=\"{appUrl}typing.gif\" alt=\"...\">";
            _htmlHelp = _htmlHelp.Replace("jquery", appUrl + "jquery");
            InvButton.Tag = false;
            HypButton.Tag = false;
            RichTextBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(RichTextBox_Scroll));
            DataObject.AddPastingHandler(RichTextBox, RichTextBox_Paste);
            var tmpDir = Path.GetTempPath() + "Calcpad\\";
            if (!Directory.Exists(tmpDir))
                Directory.CreateDirectory(tmpDir);
            _document = RichTextBox.Document;
            _currentParagraph = (Paragraph)_document.Blocks.FirstBlock;
            HighLighter.Clear(_currentParagraph);
            _undoMan = new UndoManager();
            _undoMan.Record(InputText, RichTextBox.Selection.End, null);
            _wbWarper = new WebBrowserWrapper(WebBrowser);
            _parser.Settings.Plot.ImagePath = string.Empty; //tmpDir;
            _parser.Settings.Plot.ImageUri = string.Empty; //tmpDir;
            _parser.Settings.Plot.VectorGraphics = false;
            _screenScaleFactor = ScreenMetrics.GetWindowsScreenScalingFactor();
            _autoCompleteCount = AutoCompleteListBox.Items.Count;
            _parser.Settings.Plot.ScreenScaleFactor = _screenScaleFactor;
            _cfn = string.Empty;
            TryOpenOnStartup();
            _isTextChangedEnabled = false;
            IsSaved = true;
            RichTextBox.Focus();
            Keyboard.Focus(RichTextBox);
            _findReplace.RichTextBox = RichTextBox;
            _findReplace.BeginSearch += FindReplace_BeginSearch;
            _findReplace.EndSearch += FindReplace_EndSearch;
            _findReplace.BeginReplace += FindReplace_BeginReplace;
            _findReplace.EndReplace += FindReplace_EndReplace;
            _isTextChangedEnabled = true;
        }

        private void ForceHighlight()
        {
            if (_forceHighlight)
            {
                HighLightAll();
                SetAutoIndent();
                RichTextBox.CaretPosition = _document.ContentStart;
            }
            _forceHighlight = false;
        }

        private static void SetButton(Button b, bool on)
        {
            if (on)
            {
                b.Tag = "T";
                b.BorderBrush = Brushes.SteelBlue;
                b.Background = Brushes.AliceBlue;
            }
            else
            {
                b.Tag = "F";
                b.BorderBrush = Brushes.Transparent;
                b.Background = Brushes.Transparent;
            }
        }

        private void SetUILock(bool locked)
        {
            var enabled = !locked;
            CopyButton.IsEnabled = enabled;
            PasteButton.IsEnabled = enabled;
            UndoButton.IsEnabled = enabled;
            RedoButton.IsEnabled = enabled;
            ImageButton.IsEnabled = enabled;
            KeyPadButton.IsEnabled = enabled;
            MenuEdit.IsEnabled = enabled;
            MenuInsert.IsEnabled = enabled;
            FindButton.IsEnabled = enabled;
        }

        private void RichTextBox_Scroll(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 && !_sizeChanged && !IsWebForm)
            {
                MoveAutoComplete();
                Task.Run(DispatchLineNumbers);
            }
        }

        private static void ClearTempFolder()
        {
            try
            {
                var tmpDir = Path.GetTempPath() + "Calcpad\\";
                var dir = new DirectoryInfo(tmpDir);
                foreach (var f in dir.GetFiles())
                    f.Delete();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var tag = element.Tag.ToString();
            RichTextBox.BeginChange();
            if (tag.Contains('|'))
            {
                var parts = tag.Split('|');
                if (RichTextBox.Selection.Start.GetOffsetToPosition(RichTextBox.Selection.End) == 0)
                    InsertComment(RichTextBox.Selection.End, parts[0] + parts[1]);
                else if (!ReferenceEquals(RichTextBox.Selection.Start.Paragraph, RichTextBox.Selection.End.Paragraph))
                {
                    MessageBox.Show(
#if BG
                        "Стоп! Inline Html елементи не могат да пресичат редове от текста."
#else
                        "Stop! Inline Html elements must not cross text lines."
#endif
                        , Title, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                else
                {
                    if (parts[0].Length > 0)
                        InsertComment(RichTextBox.Selection.Start, parts[0]);
                    if (parts.Length > 1 && parts[1].Length > 0)
                        InsertComment(RichTextBox.Selection.End, parts[1]);
                }
                RichTextBox.Selection.Select(RichTextBox.Selection.End, RichTextBox.Selection.End);
            }
            else if (tag.Contains('§'))
            {
                var parts = tag.Split('§');
                TextPointer tp;
                var p = RichTextBox.Selection.Start.Paragraph;
                var selectionLength = RichTextBox.Selection.Start.GetOffsetToPosition(RichTextBox.Selection.End);
                if (selectionLength > 0)
                    tp = p.ContentStart;
                else
                    tp = p.ContentEnd;

                var pararaphLength = p.ContentStart.GetOffsetToPosition(p.ContentEnd);
                if (pararaphLength > 0)
                {
                    tp = tp.InsertParagraphBreak();
                    if (selectionLength > 0)
                        tp = tp.Paragraph.PreviousBlock.ContentEnd;
                }
                tp.InsertTextInRun(parts[0]);
                p = tp.Paragraph;
                var lineNumber = GetLineNumber(p);
                HighLighter.Parse(p, IsComplex, lineNumber);
                if (selectionLength > 0)
                    tp = RichTextBox.Selection.End;

                for (int i = 1, len = parts.Length; i < len; ++i)
                {
                    p = tp.Paragraph;
                    tp = p.ContentEnd.InsertParagraphBreak();
                    tp.InsertTextInRun(parts[i]);
                    lineNumber = GetLineNumber(p);
                    HighLighter.Parse(p, IsComplex, lineNumber);
                }
                tp = tp.Paragraph.ContentEnd;
                RichTextBox.Selection.Select(tp, tp);
                SetAutoIndent();
            }
            else switch (tag)
                {
                    case "AC": RemoveLine(); break;
                    case "C": RemoveChar(); break;
                    case "Enter": InsertLine(); break;
                    default:
                        var s = tag.ToLowerInvariant();
                        if (tag[0] == '#' || tag[0] == '$' && (s.StartsWith("$plot") || s.StartsWith("$map")))
                        {
                            var p = RichTextBox.Selection.End.Paragraph;
                            if (new TextRange(p.ContentStart, p.ContentEnd).Text.Length > 0)
                            {
                                var tp = p.ContentEnd.InsertParagraphBreak();
                                tp.InsertTextInRun(tag);
                                p = tp.Paragraph;
                                var lineNumber = GetLineNumber(p);
                                HighLighter.Parse(p, IsComplex, lineNumber);
                                SetAutoIndent();
                                tp = p.ContentEnd;
                                RichTextBox.Selection.Select(tp, tp);
                            }
                            else
                                InsertText(tag);
                        }
                        else
                            InsertText(tag);
                        break;
                }
            if (tag == "Enter")
                CalculateAsync();

            RichTextBox.EndChange();
            RichTextBox.Focus();
            Keyboard.Focus(RichTextBox);
        }

        private static void InsertComment(TextPointer tp, string comment)
        {
            var ts = tp.Paragraph.ContentStart;
            var s = new TextRange(ts, tp).Text;
            if (CountCharInString(s, '\'') % 2 == 1)
                tp.InsertTextInRun(comment);
            else
                tp.InsertTextInRun('\'' + comment + '\'');
        }

        private static int CountCharInString(string s, char c)
        {
            var n = 0;
            foreach (var _c in s)
                if (_c == c)
                    ++n;
            return n;
        }

        private void AutoRun(bool syncScroll = false)
        {
            IsCalculated = true;
            if (syncScroll)
                _scrollOutput = true;
            
            _scrollY = _wbWarper.ScrollY;
            CalculateAsync();
        }

        private void RemoveLine() 
        {
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            if (_document.Blocks.Count <= 1)
            {
                _currentParagraph = (Paragraph)_document.Blocks.FirstBlock;
                _currentParagraph.Inlines.Clear();
            }
            else
            {
                _document.Blocks.Remove(RichTextBox.Selection.Start.Paragraph);
                _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            }
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
            if (IsAutoRun)
                AutoRun();
        }

        private void RemoveChar() => RichTextBox.Selection.Start.DeleteTextInRun(-1);
        private void InsertLine() => RichTextBox.CaretPosition = RichTextBox.Selection.Start.InsertParagraphBreak();

        private void InsertText(string text)
        {
            var sel = RichTextBox.Selection;
            sel.Text = text;
            sel.Select(sel.End, sel.End);
        }

        private void LineClicked(string data)
        {
            if (int.TryParse(data, out int line))
            {
                if (line > 0 && line <= _document.Blocks.Count)
                {
                    var block = _document.Blocks.ElementAt(line - 1);
                    if (!object.ReferenceEquals(block, _currentParagraph))
                    {
                        RichTextBox.CaretPosition = block.ContentEnd;
                        Rect r = block.ContentEnd.GetCharacterRect(LogicalDirection.Backward);
                        RichTextBox.ScrollToVerticalOffset(r.Y);
                    }
                }
            }
            RichTextBox.Focus();
            Keyboard.Focus(RichTextBox);
        }

        private void LinkClicked(string data)
        {
            RichTextBox.Selection.Text = string.Empty;
            var lines = data.Split('\n');
            var p = RichTextBox.Selection.Start.Paragraph;
            if (lines.Length == 1)
            {
                TextPointer tp;
                if ((data[0] == '#' || data[0] == '$') && !p.ContentEnd.IsAtLineStartPosition)
                {
                    tp = p.ContentEnd.InsertParagraphBreak();
                    RichTextBox.Selection.Select(tp, tp);
                }
                else
                    tp = RichTextBox.Selection.End;
                InsertText(data);
                if (tp is not null)
                    RichTextBox.Selection.Select(tp, tp);
            }
            else
            {
                var tp = p.ContentStart;
                _isTextChangedEnabled = false;
                RichTextBox.BeginChange();
                foreach (var line in lines)
                {
                    if (!p.ContentEnd.IsAtLineStartPosition)
                        p = p.ContentEnd.InsertParagraphBreak().Paragraph;
                    p.Inlines.Add(line);
                    HighLighter.Parse(p, IsComplex, GetLineNumber(p));
                }
                RichTextBox.Selection.Select(tp, tp);
                RichTextBox.EndChange();
                _isTextChangedEnabled = true;
                Task.Run(DispatchAutoIndent);
                Record();
            }
            RichTextBox.Focus();
            Keyboard.Focus(RichTextBox);
        }
        private void CalcButton_Click(object sender, RoutedEventArgs e) => Calculate();
        private void Command_Calculate(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsCalculated)
                _scrollY = _wbWarper.ScrollY;

            Calculate();
            if (IsCalculated)
                _wbWarper.ScrollY = _scrollY;
        }

        private void Calculate()
        {
            IsCalculated = !IsCalculated;
            if (IsWebForm)
                CalculateAsync(!IsCalculated);
            else if (IsCalculated)
                CalculateAsync();
            else
                ShowHelp();
        }

        private void Command_New(object senter, ExecutedRoutedEventArgs e)
        {
            var r = PromptSave();
            if (r == MessageBoxResult.Cancel)
                return;

            if (_isParsing)
                _parser.Cancel();

            CurrentFileName = string.Empty;
            _document.Blocks.Clear();
            var p = new Paragraph();
            _document.Blocks.Add(p);
            RichTextBox.CaretPosition = p.ContentStart;
            if (IsWebForm)
            {
                _mustPromptUnlock = false;
                IsWebForm = false;
                WebFormButton.Visibility = Visibility.Visible;
                MenuWebForm.Visibility = Visibility.Visible;
            }
            ShowHelp();
            SaveButton.Tag = null;
            _undoMan.Reset();
            Record();
        }

        private void Command_Open(object sender, ExecutedRoutedEventArgs e)
        {
            var r = PromptSave();
            if (r == MessageBoxResult.Cancel)
                return;

            var s = ".cpd";
            if (!string.IsNullOrWhiteSpace(CurrentFileName))
                s = Path.GetExtension(CurrentFileName);

            var dlg = new OpenFileDialog
            {
                DefaultExt = s,
                FileName = '*' + s,
                InitialDirectory = File.Exists(CurrentFileName) ? Path.GetDirectoryName(CurrentFileName) : DocumentPath,
                CheckFileExists = true,
                Multiselect = false,
#if BG
                Filter = s == ".txt"
                    ? "Текстов файл (*.txt)|*.txt|Calcpad документ (*.cpd)|*.cpd|Calcpad двоичен файл (*.cpdz)|*.cpdz"
                    : "Calcpad документ (*.cpd)|*.cpd|Calcpad двоичен файл (*.cpdz)|*.cpdz|Текстов файл (*.txt)|*.txt"
#else
                Filter = s == ".txt"
                    ? "Text File (*.txt)|*.txt|Calcpad Worksheet (*.cpd)|*.cpd|Calcpad Compiled (*.cpdz)|*.cpdz"
                    : "Calcpad Worksheet (*.cpd)|*.cpd|Calcpad Compiled (*.cpdz)|*.cpdz|Text File (*.txt)|*.txt"
#endif
            };

            var result = (bool)dlg.ShowDialog();
            if (result)
                FileOpen(dlg.FileName);
        }

        private void Command_Save(object sender, ExecutedRoutedEventArgs e)
        {
            if ((string)SaveButton.Tag == "S" || string.IsNullOrWhiteSpace(CurrentFileName))
                FileSaveAs();
            else
                FileSave();
        }

        private void ReadRecentFiles()
        {
            MenuRecent.Items.Clear();
            var list = Properties.Settings.Default.RecentFileList;
            var j = 0;
            if (list is not null)
            {
                foreach (var fileName in list)
                {
                    if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
                        continue;

                    ++j;
                    var menu = new MenuItem()
                    {
                        ToolTip = fileName,
                        Icon = $"   {j}",
                        Header = Path.GetFileName(fileName),
                    };
                    menu.Click += RecentFileList_Click;
                    MenuRecent.Items.Add(menu);
                }
                if (MenuRecent.Items.Count > 0)
                {
                    var firstMenu = (MenuItem)MenuRecent.Items[0];
                    DocumentPath = Path.GetDirectoryName((string)firstMenu.ToolTip);
                    Directory.SetCurrentDirectory(DocumentPath ?? string.Empty);
                }
            }
            MenuRecent.IsEnabled = j > 0;
            CloneRecentFilesList();
        }

        private void WriteRecentFiles()
        {
            var n = MenuRecent.Items.Count;
            if (n == 0)
                return;

            var list =
                Properties.Settings.Default.RecentFileList ??
                new StringCollection();

            list.Clear();
            for (int i = 0; i < n; ++i)
            {
                var menu = (MenuItem)MenuRecent.Items[i];
                var value = (string)menu.ToolTip;
                list.Add(value);
            }

            Properties.Settings.Default.RecentFileList = list;
            Properties.Settings.Default.Save();
        }

        private void AddRecentFile(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            var n = MenuRecent.Items.Count;
            for (int i = 0; i < n; ++i)
            {
                var menu = (MenuItem)MenuRecent.Items[i];
                if (!string.Equals(menu.ToolTip, fileName))
                    continue;

                for (int j = i; j > 0; --j)
                {
                    menu = (MenuItem)MenuRecent.Items[j];
                    menu.Header = ((MenuItem)MenuRecent.Items[j - 1]).Header;
                    menu.ToolTip = ((MenuItem)MenuRecent.Items[j - 1]).ToolTip;
                }
                var first = (MenuItem)MenuRecent.Items[0];
                first.Header = Path.GetFileName(fileName);
                first.ToolTip = fileName;
                CloneRecentFilesList();
                return;
            }
            if (n >= 9)
            {
                MenuRecent.Items.RemoveAt(n - 1);
                --n;
            }
            var newMenu = new MenuItem()
            {
                ToolTip = fileName,
                Icon = "   1",
                Header = Path.GetFileName(fileName),
            };
            newMenu.Click += RecentFileList_Click;
            MenuRecent.Items.Insert(0, newMenu);
            for (int i = 1; i <= n; ++i)
            {
                var menu = (MenuItem)MenuRecent.Items[i];
                menu.Icon = $"   {i + 1}";
            }
            MenuRecent.IsEnabled = n >= 0;
            CloneRecentFilesList();
        }


        private void CloneRecentFilesList()
        {
            RecentFliesListButton.IsEnabled = MenuRecent.IsEnabled;
            if (!RecentFliesListButton.IsEnabled)
                return;

            RecentFilesListContextMenu.Items.Clear();
            foreach (MenuItem menu in MenuRecent.Items)
            {
                var value = (string)menu.ToolTip;
                var contextMenuItem = new MenuItem()
                {
                    Header = menu.Header,
                    Icon = menu.Icon,
                    ToolTip = menu.ToolTip,
                };
                contextMenuItem.Click += RecentFileList_Click;
                RecentFilesListContextMenu.Items.Add(contextMenuItem);
            }
        }

        private void RecentFliesListButton_Click(object sender, RoutedEventArgs e)
        {
            RecentFilesListContextMenu.PlacementTarget = RecentFliesListButton;
            RecentFilesListContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Relative;
            RecentFilesListContextMenu.StaysOpen = true;
            RecentFilesListContextMenu.IsOpen = true;
        }

        private void RecentFileList_Click(object sender, RoutedEventArgs e)
        {
            RecentFilesListContextMenu.IsOpen = false;
            var fileName = (string)((MenuItem)sender).ToolTip;
            if (File.Exists(fileName))
                FileOpen(fileName);
        }

        private void Command_SaveAs(object sender, ExecutedRoutedEventArgs e) => FileSaveAs();

        private void FileSaveAs()
        {
            string s;
            if (string.IsNullOrWhiteSpace(CurrentFileName))
                s = Path.GetExtension(CurrentFileName);
            else if (InputText.Contains('?'))
                s = ".cpd";
            else
                s = ".txt";

            var dlg = new SaveFileDialog
            {
                FileName = Path.GetFileName(CurrentFileName),
                InitialDirectory = File.Exists(CurrentFileName) ? Path.GetDirectoryName(CurrentFileName) : DocumentPath,
                DefaultExt = s,
                OverwritePrompt = true,
                Filter = s switch
                {
#if BG
                ".txt" => "Текстов файл (*.txt)|*.txt|Calcpad документ (*.cpd)|*.cpd|Calcpad двоичен файл (*.cpdz)|*.cpdz",
                ".cpdz" => "Calcpad двоичен файл (*.cpdz)|*.cpdz",
                _ => "Calcpad документ (*.cpd)|*.cpd|Calcpad двоичен файл (*.cpdz)|*.cpdz|Текстов файл (*.txt)|*.txt"
#else
                    ".txt" => "Text File (*.txt)|*.txt|Calcpad Worksheet (*.cpd)|*.cpd|Calcpad Compiled (*.cpdz)|*.cpdz",
                    ".cpdz" => "Calcpad Compiled (*.cpdz)|*.cpdz",
                    _ => "Calcpad Worksheet (*.cpd)|*.cpd|Calcpad Compiled (*.cpdz)|*.cpdz|Text File (*.txt)|*.txt"
#endif
                }
            };

            var result = (bool)dlg.ShowDialog();
            if (!result)
                return;
            CopyLocalImages(dlg.FileName);
            CurrentFileName = dlg.FileName;
            
            if (s == ".cpdz" && Path.GetExtension(CurrentFileName) != ".cpdz")
                CurrentFileName = Path.ChangeExtension(CurrentFileName, ".cpdz");

            FileSave();
            AddRecentFile(CurrentFileName);
        }

        private void CopyLocalImages(string newFileName)
        {
            var images = GetLocalImages(InputText);
            if (images is not null)
            {
                var sourcePath = Path.GetDirectoryName(CurrentFileName);
                var targetPath = Path.GetDirectoryName(newFileName);
                if (sourcePath != targetPath && Directory.Exists(targetPath))
                {
                    var sourceParent = Directory.GetParent(sourcePath).FullName;
                    var targetParent = Directory.GetParent(targetPath).FullName;
                    var regexString = @"src\s*=\s*""\s*\.\./";
                    for (int i = 0; i < 2; ++i)
                    {
                        foreach (var image in images)
                        {
                            var m = Regex.Match(image, regexString, RegexOptions.IgnoreCase);
                            if (m.Success)
                            {
                                var n = m.Length;
                                var imageFileName = image[n..^1];
                                var imageSourceFile = Path.Combine(sourceParent, imageFileName);
                                if (File.Exists(imageSourceFile))
                                {
                                    var imageTargetFile = Path.Combine(targetParent, imageFileName);
                                    var imageTargetPath = Path.GetDirectoryName(imageTargetFile);
                                    Directory.CreateDirectory(imageTargetPath);
                                    try
                                    {
                                        File.Copy(imageSourceFile, imageTargetFile, true);
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show(e.Message);
                                        break;
                                    }
                                    
                                }
                            }
                        }
                        regexString = @"src\s*=\s*""\s*\./";
                        sourceParent = sourcePath;
                        targetParent = targetPath;  
                    }
                }
            }
        }

        private void FileSave()
        {
            if (IsWebForm)
                SetAutoIndent();

            var text = GetInputText();
            if (text.Contains('?'))
            {
                if (IsWebForm)
                {
                    if (IsCalculated)
                    {
                        CalculateAsync(true);
                        IsCalculated = false;
                        _isSaving = true;
                        return;
                    }
                    text += GetInputValues();
                }
                else
                {
                    var values = ReadInputFromCode();
                    text += '\v' + string.Join("\t", values);
                }
            }
            WriteFile(CurrentFileName, text);
            SaveButton.Tag = null;
            IsSaved = true;
        }

        private void Command_Help(object sender, ExecutedRoutedEventArgs e)
        {
            var fileName = AppInfo.Path + "readme.html";
            if (File.Exists(fileName))
            {
                var proc = new Process();
                var psi = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = fileName
                };
                proc.StartInfo = psi;

                try
                {
                    proc.Start();
                }
                catch (Exception Ex)
                {
                    MessageBox.Show(Ex.Message);
                }
            }
            else
                ShowHelp();
        }

        private void Command_Close(object sender, ExecutedRoutedEventArgs e) => Application.Current.Shutdown();

        private void Command_Copy(object sender, ExecutedRoutedEventArgs e) => RichTextBox.Copy();

        private void Command_Paste(object sender, ExecutedRoutedEventArgs e)
        {
            if (InputFrame.Visibility == Visibility.Visible)
            {
                RichTextBox.Paste();
                RichTextBox.Focus();
                Keyboard.Focus(RichTextBox);
            }
        }

        private void Command_Undo(object sender, ExecutedRoutedEventArgs e)
        {
            if (_undoMan.Undo())
                RestoreUndoData();
        }

        private void Command_Redo(object sender, ExecutedRoutedEventArgs e)
        {
            if (_undoMan.Redo())
                RestoreUndoData();
        }

        private void Command_Print(object sender, ExecutedRoutedEventArgs e) => _wbWarper.PrintPreview();

        private void Command_Find(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsWebForm)
                return;

            string s = RichTextBox.Selection.Text;
            if (!(string.IsNullOrEmpty(s) || s.Contains('\n')))
                _findReplace.SearchString = s;

            _findReplaceWindow = new()
            {
                Owner = this,
                FindReplace = _findReplace
            };
            bool isSelection = s is not null && s.Length > 5;
            _findReplaceWindow.SelectionCheckbox.IsEnabled = isSelection;
            if (isSelection)
                RichTextBox.IsInactiveSelectionHighlightEnabled = true;

            _findReplaceWindow.Show();
            RichTextBox.IsInactiveSelectionHighlightEnabled = false;
        }

        private void Command_FindNext(object sender, ExecutedRoutedEventArgs e)
        { 
            _findReplace.Find();   
        }
        

        private void FileOpen(string fileName)
        {
            if (_isParsing)
                _parser.Cancel();
            CurrentFileName = fileName;
            var ext = Path.GetExtension(fileName);
            _isFileOpen = true;
            var hasForm = GetInputTextFromFile(ext == ".txt");
            if (ext == ".cpdz")
            {
                IsWebForm = true;
                WebFormButton.Visibility = Visibility.Hidden;
                MenuWebForm.Visibility = Visibility.Collapsed;
                SaveButton.Tag = "S";
            }
            else
            {
                WebFormButton.Visibility = Visibility.Visible;
                MenuWebForm.Visibility = Visibility.Visible;
                if (hasForm)
                {
                    if (!IsWebForm)
                        IsWebForm = true;
                    else
                    {
                        IsCalculated = false;
                        CalculateAsync(true);
                    }
                    SaveButton.Tag = "S";
                }
                else
                {
                    if (IsWebForm)
                        IsWebForm = false;
                    else
                    {
                        ForceHighlight();
                        Task.Run(DispatchLineNumbers);
                    }
                    SaveButton.Tag = null;
                    IsCalculated = true;
                    CalculateAsync();
                }
            }
            _mustPromptUnlock = IsWebForm;
            IsSaved = true;
            _isFileOpen = false;
            AddRecentFile(CurrentFileName);
        }

        private MessageBoxResult PromptSave()
        {
            var result = MessageBoxResult.No;
            if (!IsSaved)
#if BG
                result = MessageBox.Show("Файлът не е записан. Запис?", Title, MessageBoxButton.YesNoCancel);
#else
                result = MessageBox.Show("File not saved. Save?", Title, MessageBoxButton.YesNoCancel);
#endif
            if (result == MessageBoxResult.Yes)
            {
                if ((string)SaveButton.Tag == "S" || string.IsNullOrWhiteSpace(CurrentFileName))
                    FileSaveAs();
                else
                    FileSave();
            }
            return result;
        }

        private async void CalculateAsync(bool toWebForm = false)
        {
            if (_isParsing)
                return;

            if (double.TryParse(DecimalsTextBox.Text, out var d))
            {
                var i = (int)Math.Floor(d);
                _parser.Settings.Math.Decimals = i;
                DecimalsTextBox.Text = i.ToString();
                DecimalsTextBox.Foreground = Brushes.Black;
            }
            else
                DecimalsTextBox.Foreground = Brushes.Red;

            _parser.Settings.Math.Substitute = SubstituteCheckBox.IsChecked ?? false;
            if (IsWebForm)
            {
                if (!toWebForm)
                    GetAndSetInputFields();
            }
            else
                ReadAndSetInputFields();

            var text = SetImageLocalPath(InputText);
            if (toWebForm)
                _parser.Parse(text, false);
            else
            {
                _isParsing = true;
                WebFormButton.IsEnabled = false;
                MenuWebForm.IsEnabled = false;
                CalcButton.IsEnabled = false;
                MenuCalculate.IsEnabled = false;
                try
                {
                    WebBrowser.InvokeScript($"delayedLoad", _htmlParsing);
                }
                catch 
                {
                    WebBrowser.NavigateToString(_htmlParsing);
                }
                _parseTask = Task.Run(() =>
                {
                    _parser.Parse(text);
                });
                await _parseTask;
                _autoRun = false;
                _isParsing = false;
                if (!IsWebForm)
                {
                    MenuWebForm.IsEnabled = true;
                    WebFormButton.IsEnabled = true;
                }
                CalcButton.IsEnabled = true;
                MenuCalculate.IsEnabled = true;
            }
            var htmlResult = HtmlApplyWorksheet(_parser.HtmlResult);
            try
            {
                WebBrowser.NavigateToString(htmlResult);
            }
            catch (Exception e)
            {
#if BG
                MessageBox.Show(e.Message, $"Грешка", MessageBoxButton.OK, MessageBoxImage.Error);
#else
                MessageBox.Show(e.Message, $"Error", MessageBoxButton.OK, MessageBoxImage.Error);
#endif
            }
            if (IsWebForm)
#if BG
                OutputFrame.Header = toWebForm ? "Входни данни" : "Резултати";
#else
                OutputFrame.Header = toWebForm ? "Input" : "Output";
#endif
        }

        private string SetImageLocalPath(string s)
        {
            if (string.IsNullOrWhiteSpace(CurrentFileName))
                return s;

            var path = Path.GetDirectoryName(CurrentFileName);
            var parent = Directory.GetParent(path).FullName;
            path = "file:///" + path.Replace('\\', '/');
            parent = "file:///" + parent.Replace('\\', '/');

            var s1 = Regex.Replace(s, @"src\s*=\s*""\s*\.\.", @"src=""" + parent, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var s2 = Regex.Replace(s1, @"src\s*=\s*""\s*\.", @"src=""" + path, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return s2;
        }

        private static string[] GetLocalImages(string s)
        {
            MatchCollection matches = Regex.Matches(s, @"src\s*=\s*""\s*\.\.?(.+?)""", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var n = matches.Count;
            if (n == 0)
                return null;

            string[] images = new string[n];
            for (int i = 0;  i < n; ++i)
                images[i] = matches[i].Value;

            return images;
        }

        private string HtmlApplyWorksheet(string s)
        {
            _htmlBuilder.Clear();
            _htmlBuilder.Append(_htmlWorksheet);
            _htmlBuilder.Append(s);
            _htmlBuilder.Append(" </body></html>");
            return _htmlBuilder.ToString();
        }

        private void ShowHelp()
        {
            if (!_isParsing)
                WebBrowser.NavigateToString(_htmlHelp);
        }

        private static string GetHelp(string helpURL)
        {
            try
            {
                using var client = new HttpClient();
                return client.GetStringAsync(helpURL).Result;
            }
            catch
            {
                return ReadFile(AppInfo.Path + "help.html");
            }
        }

        private static string ReadFile(string fileName)
        {
            string s;
            try
            {
                using var sr = new StreamReader(fileName);
                s = sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                s = string.Empty;
            }
            return s;
        }

        private static List<string> ReadLines(string fileName)
        {
            var lines = new List<string>();
            try
            {
                if (Path.GetExtension(fileName).ToLowerInvariant() == ".cpdz")
                {
                    var f = new FileInfo(fileName)
                    {
                        IsReadOnly = false
                    };
                    using var fs = f.OpenRead();
                    lines = Zip.Decompress(fs);
                }
                else
                {
                    using var sr = new StreamReader(fileName);
                    while (!sr.EndOfStream)
                    {
                        var s = sr.ReadLine().TrimStart('\t');
                        lines.Add(s);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return lines;
        }

        private static void WriteFile(string fileName, string s)
        {
            try
            {
                if (Path.GetExtension(fileName).ToLowerInvariant() == ".cpdz")
                {
                    var f = new FileInfo(fileName);
                    using var fs = f.Create();
                    Zip.Compress(s, fs);
                }
                else
                {
                    using var sw = new StreamWriter(fileName);
                    sw.Write(s);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool GetInputTextFromFile(bool highLight)
        {
            var lines = ReadLines(CurrentFileName);

            _isTextChangedEnabled = false;
            _document.Blocks.Clear();
            var hasForm = false;
            HighLighter.GetDefinedVariablesAndFunctions(lines, IsComplex);
            int i = 1;
            foreach (var line in lines)
            {
                string s;
                if (line.Contains('\v'))
                {
                    hasForm = true;
                    var n = line.IndexOf('\v');
                    s = line[(n + 1)..];
                    var inputFields = s.Split('\t');
                    _parser.ClearInputFields();
                    SetInputFields(inputFields);
                    s = line[..n];
                }
                else if (highLight)
                    s = line.TrimStart('\t');
                else
                    s = line;

                var p = new Paragraph();
                p.Inlines.Add(new Run(s));
                if (highLight)
                    HighLighter.Parse(p, IsComplex, i++);

                _document.Blocks.Add(p);
            }
            _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            if (highLight)
            {
                HighLighter.Clear(_currentParagraph);
                Task.Run(() => Dispatcher.InvokeAsync(SetAutoIndent, DispatcherPriority.Send));
            }
            _undoMan.Reset();
            Record();
            _isTextChangedEnabled = true;
            _forceHighlight = !highLight;
            return hasForm;
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ResetText();
            Task.Run(DispatchLineNumbers);
            if (IsAutoRun)
                AutoRun();
        }

        private void ResetText()
        {
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            _document.Blocks.Clear();
            _currentParagraph = new Paragraph();
            _document.Blocks.Add(_currentParagraph);
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private string GetInputText()
        {
            string[] tabs =
            {
                "",
                "\t",
                "\t\t",
                "\t\t\t",
                "\t\t\t\t",
                "\t\t\t\t\t",
                "\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t",
                "\t\t\t\t\t\t\t\t\t\t\t\t"
           };
            var stringBuilder = new StringBuilder();
            var b = _document.Blocks.FirstBlock;
            while (b is not null)
            {
                var n = (int)(((Paragraph)b).TextIndent / AutoIndentStep);
                if (n > 12)
                    n = 12;
                var line = new TextRange(b.ContentStart, b.ContentEnd).Text;
                stringBuilder.AppendLine(tabs[n] + line);
                b = b.NextBlock;
            }
            return stringBuilder.ToString();
        }

        private string GetInputValues()
        {
            var s = _wbWarper.GetInputVaues();
            return '\v' + string.Join("\t", s);
        }

        private string GetInputTextAndValues()
        {
            var text = GetInputText();
            var values = GetInputValues();
            return text + values;
        }

        private void HtmlFileSave()
        {
            var dlg = new SaveFileDialog
            {
                DefaultExt = ".html",
                Filter = "Html Files (*.html)|*.html",
                FileName = Path.ChangeExtension(Path.GetFileName(CurrentFileName), "html"),
                InitialDirectory = File.Exists(CurrentFileName) ? Path.GetDirectoryName(CurrentFileName) : DocumentPath,
                OverwritePrompt = true
            };
            var result = (bool)dlg.ShowDialog();
            if (result)
            {
                string html = _wbWarper.GetContents();
                WriteFile(dlg.FileName, html);
                new Process
                {
                    StartInfo = new ProcessStartInfo(dlg.FileName)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
        }

        private void CopyOutputButton_Click(object sender, RoutedEventArgs e) => _wbWarper.ClipboardCopy();

        private void WordButton_Click(object sender, RoutedEventArgs e)
        {
            var isDoc = (Professional.IsChecked ?? false) && (IsCalculated || IsWebForm);
            var fileExt = isDoc ? "docx" : "html";
            string fileName;
            if (IsCalculated || IsWebForm)
            {
                if (CurrentFileName.Length == 0)
                    fileName = Path.GetTempPath() + "Calcpad\\Output." + fileExt;
                else
                    fileName = Path.ChangeExtension(CurrentFileName, fileExt);
            }
            else
            {
                fileName = AppInfo.Path + "help.docx";
            }
            try
            {
                if (IsCalculated || IsWebForm)
                {
                    if (isDoc)
                    {
                        fileName = PromtSaveDoc(fileName);
                        var logString = _wbWarper.ExportOpenXml(fileName);
                        if (logString.Length > 0)
                        {
#if BG
                            const string message = "Неуспешен запис на docx файл. Да покажа ли списък с грешките?";
#else
                            const string message = "Error exporting docx file. Display validation log?";
#endif
                            if (MessageBox.Show(message, Title, MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                            {
                                var logFile = fileName + "_validation.log";
                                WriteFile(logFile, logString);
                                RunExternalApp("NOTEPAD", logFile);
                            }
                        }
                    }
                    else
                    {
                        var html = _wbWarper.GetContents();
                        WriteFile(fileName, html);
                    }
                }
                if (!RunExternalApp("WINWORD", fileName))
                {
                    RunExternalApp("SOFFICE", fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool RunExternalApp(string appName, string fileName)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = appName
            };
            if (fileName is not null)
                startInfo.Arguments = fileName.Contains(' ') ? '\"' + fileName + '\"' : fileName;

            startInfo.UseShellExecute = true;
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            try
            {
                return Process.Start(startInfo) is not null;
            }
            catch
            {
                return false;
            }
        }

        private string PromtSaveDoc(string fileName)
        {
            var dlg = new SaveFileDialog
            {
                FileName = Path.GetFileName(fileName),
                InitialDirectory =
                    File.Exists(CurrentFileName) ? Path.GetDirectoryName(CurrentFileName) : DocumentPath,
                DefaultExt = "docx",
                OverwritePrompt = true,
                Filter = "Microsoft Word Document (*.docx)|*.docx"
            };

            var result = (bool)dlg.ShowDialog();
            return result ? dlg.FileName : fileName;
        }

        private void RestoreUndoData()
        {
            var pointer = _undoMan.RestorePointer;
            var lineNumber = CurrentLineNumber;
            var pointerParagraph = pointer.Paragraph;
            var offset = pointerParagraph is null ? 0 :
                new TextRange(pointer, pointerParagraph.ContentEnd).Text.Length;
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            var values = _undoMan.RestoreValues;
            _document.Blocks.Clear();
            int i = 0, j = 0;
            var n = values is null ? 0 : values.Length;
            var indent = 0d;
            using (var sr = new StringReader(_undoMan.RestoreText))
            {
                HighLighter.ClearDefinedVariablesAndFunctions(IsComplex);
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line is null)
                        break;
                    var p = new Paragraph();
                    p.Inlines.Add(new Run(line));
                    HighLighter.GetDefinedVariablesAndFunctions(line, j);
                    HighLighter.Parse(p, IsComplex, j++);
                    if (!UpdateIndent(p, ref indent))
                        p.TextIndent = indent;

                    if (j == lineNumber)
                        pointerParagraph = p;

                    foreach (var inline in p.Inlines)
                        if (i < n && inline.ToolTip is ToolTip tt)
                            tt.Content = values[i++];

                    _document.Blocks.Add(p);
                }
            }
            _currentParagraph = pointerParagraph;
            HighLighter.Clear(_currentParagraph);         
            pointer = FindPositionAtOffset(pointerParagraph, offset);
            RichTextBox.Selection.Select(pointer, pointer);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
            Task.Run(DispatchLineNumbers);
            if (IsAutoRun)
                AutoRun();
        }

        private static TextPointer FindPositionAtOffset(Paragraph p, int offset)
        {
            var tps = p.ContentStart;
            var tpe = p.ContentEnd;
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

        private void WebFormButton_Click(object sender, RoutedEventArgs e) => WebForm();

        private void Command_WebForm(object sender, ExecutedRoutedEventArgs e)
        {
            if (WebFormButton.IsEnabled)
                WebForm();
        }

        private void WebForm()
        {
            IsWebForm = !IsWebForm;
            if (!IsWebForm)
                ShowHelp();
        }

        private void GetAndSetInputFields()
        {
            if (InputText.Contains("%u"))
            {
                try
                {
                    _parser.Settings.Units = _wbWarper.Units;
                }
                catch
                {
#if BG
                    MessageBox.Show("Грешка при връщане на мерни единици.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
#else
                    MessageBox.Show("Error getting units.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }
            }
            else
                _parser.Settings.Units = "m";

            var values = _wbWarper.GetInputVaues();
            _parser.ClearInputFields();
            if (values.Length > 0)
                SetInputFields(values);
        }

        private void ReadAndSetInputFields()
        {
            var values = ReadInputFromCode();
            _parser.ClearInputFields();
            if (values.Length > 0)
                SetInputFields(values);
        }

        private void SetUnits()
        {
            if (InputText.Contains("%u"))
            {
                try
                {
                    _wbWarper.Units = _parser.Settings.Units;
                }
                catch
                {
#if BG
                    MessageBox.Show("Грешка при задаване на мерни единици.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
#else
                    MessageBox.Show("Error setting units.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
#endif
                }
            }
        }

        private void SetInputFields(string[] s)
        {
            for (int i = 0, len = s.Length; i < len; ++i)
            {
                ref var sloc = ref s[i];
                if (sloc is null || sloc.Length == 0)
                    _parser.SetInputField("0");
                else
                    _parser.SetInputField(sloc.Replace(',', '.'));
            }
        }

        private void SubstituteCheckBox_Clicked(object sender, RoutedEventArgs e) => ClearOutput();

        private void DecimalsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearOutput(false);
            if (IsInitialized && int.TryParse(DecimalsTextBox.Text, out int n))
                DecimalScrollBar.Value = 15 - n;
        }

        private void ClearOutput(bool focus = true)
        {
            if (IsInitialized)
            {
                if (IsCalculated)
                {
                    IsCalculated = false;
                    if (IsWebForm)
                        CalculateAsync(true);
                    else if (IsAutoRun)
                    {
                        _scrollY = _wbWarper.ScrollY;
                        Calculate();
                    }
                    else
                        ShowHelp();
                }
                if (focus)
                {
                    RichTextBox.Focus();
                    Keyboard.Focus(RichTextBox);
                }
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".png",
                Filter = "Image Files (*bmp, *.png, *.gif, *.jpeg *.jpg)|*.png; *.gif; *.jpeg; *.jpg",
                CheckFileExists = true,
                Multiselect = false
            };
            var result = (bool)dlg.ShowDialog();
            if (result)
            {
                var filePath = dlg.FileName;
                var fileName = Path.GetFileName(filePath);
                var size = GetImageSize(filePath);
                filePath = filePath.Replace('\\', '/');
                var p = new Paragraph();
                p.Inlines.Add(new Run($"'<img style=\"float:right; height:{size.Height}pt; width:{size.Width}pt;\" src=\"{filePath}\" alt=\"{fileName}\">"));
                HighLighter.Parse(p, IsComplex, GetLineNumber(p));
                _document.Blocks.InsertBefore(_document.Blocks.FirstBlock, p);
            }
        }

        private static Size GetImageSize(string fileName)
        {
            using var imageStream = File.OpenRead(fileName);
            var decoder = BitmapDecoder.Create(imageStream,
                BitmapCreateOptions.IgnoreColorProfile,
                BitmapCacheOption.Default);
            return new Size
            {
                Height = Math.Round(0.75 * decoder.Frames[0].Height),
                Width = Math.Round(0.75 * decoder.Frames[0].Width)
            };
        }

        private void KeyPadButton_Click(object sender, RoutedEventArgs e)
        {
            if (KeyPadGrid.Visibility == Visibility.Hidden)
            {
                KeyPadGrid.Visibility = Visibility.Visible;
                InputGrid.RowDefinitions[1].Height = new GridLength(154);
            }
            else
            {
                KeyPadGrid.Visibility = Visibility.Hidden;
                InputGrid.RowDefinitions[1].Height = new GridLength(0);
            }
            SetButton(KeyPadButton, KeyPadGrid.Visibility == Visibility.Visible);
        }

        private void GreekLetter_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var tb = (TextBlock)sender;
            InsertText(tb.Text);
        }

        private void EquationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                var pro = sender.Equals(Professional);
                _parser.Settings.Math.FormatEquations = pro;
                Professional.IsChecked = pro;
                Inline.IsChecked = !pro;
                ClearOutput();
            }
        }

        private void AngleRadioButton_Checked(object sender, RoutedEventArgs e)
        {   
            if (IsInitialized)
            {
                var deg = sender.Equals(Deg);
                _parser.Settings.Math.Degrees = deg;
                Deg.IsChecked = deg;
                Rad.IsChecked = !deg;
                ClearOutput();
            }
        }

        private void ModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                var complex = sender.Equals(Complex);
                _parser.Settings.Math.IsComplex = complex;
                Complex.IsChecked = complex;
                Real.IsChecked = !complex;
                ClearOutput();
                if (!IsWebForm)
                    Task.Run(() => Dispatcher.InvokeAsync(HighLightAll, DispatcherPriority.Send));
            }
        }

        private void SaveOutputButton_Click(object sender, RoutedEventArgs e) => HtmlFileSave();

        private void TryOpenOnStartup()
        {
            var args = Environment.GetCommandLineArgs();
            var n = args.Length;
            if (n > 1)
            {
                var s = string.Join(" ", args, 1, n - 1);
                if (File.Exists(s))
                {
                    var ex = Path.GetExtension(s);
                    if (ex == ".cpd" || ex == ".cpdz")
                    {
                        CurrentFileName = s;
                        _isFileOpen = true;
                        GetInputTextFromFile(false);
                        IsWebForm = true;
                        _mustPromptUnlock = true;
                        if (ex == ".cpdz")
                            WebFormButton.Visibility = Visibility.Hidden;
                        _isFileOpen = false;
                        return;
                    }
                }
            }
            ShowHelp();
            Task.Run(DispatchLineNumbers);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var r = PromptSave();
            if (r== MessageBoxResult.Cancel)
                e.Cancel = true;

            ClearTempFolder();
            WriteRecentFiles();
        }

        private void ScrollOutput()
        {
            var lineNumber = CurrentLineNumber;
            var offset = (int)(RichTextBox.CaretPosition.GetCharacterRect(LogicalDirection.Forward).Top * _screenScaleFactor);
            var tempScrollY = _wbWarper.ScrollY;
            _wbWarper.Scroll(lineNumber, offset);
            if (tempScrollY == _wbWarper.ScrollY)
                _wbWarper.ScrollY = _scrollY;

            _scrollOutput = false;
        }

        private bool IsAutoRun =>
            AutoRunCheckBox.Visibility == Visibility.Visible && 
            (AutoRunCheckBox.IsChecked ?? false);

        private void RichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (_forceBackSpace && RichTextBox.CaretPosition.IsAtLineStartPosition)
            {
                _forceBackSpace = false;
                var p = RichTextBox.CaretPosition.Paragraph;
                if (p is not null)
                {
                    var pp = (Paragraph)p.PreviousBlock;
                    if (pp is not null)
                    {
                        _isTextChangedEnabled = false;
                        RichTextBox.CaretPosition = pp.ContentEnd;
                        var s = new TextRange(p.ContentStart, p.ContentEnd).Text;
                        pp.Inlines.Add(s);
                        _document.Blocks.Remove(p);
                        _isTextChangedEnabled = true;
                    }
                }
            }
            else if (e.Key == Key.G && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                var cp = RichTextBox.Selection.End;
                if (!cp.IsAtLineStartPosition)
                {
                    var sel = RichTextBox.Selection;
                    sel.Select(cp.GetPositionAtOffset(-1), cp);
                    string s = sel.Text;
                    if (s.Length == 1)
                    {
                        char c = LatinGreekChar(s[0]);
                        if (c != s[0])
                            InsertText(c.ToString());
                        else
                            sel.Select(cp, cp);
                    }
                }
            }
        }

        private int CurrentLineNumber => GetLineNumber(_currentParagraph);

        private int GetLineNumber(Block block)
        {
            var i = 1;  
            foreach (Block b in _document.Blocks)
            {
                if (object.ReferenceEquals (b, block))  
                    return i;
                ++i;
            }
            return -1;
        }

        private async void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isTextChangedEnabled)
            {
                if (_document.Blocks.Count == 0)
                    ResetText();

                if (IsAutoRun)
                {
                    var p = RichTextBox.Selection.End.Paragraph;
                    if (p is not null)
                    {
                        var len = p.ContentStart.GetOffsetToPosition(p.ContentEnd);
                        if (IsCalculated && len > 2)
                        {
                            var lineNumber = CurrentLineNumber;
                            _wbWarper.SetContent(lineNumber, _svgTyping);
                        }
                    }
                    _autoRun = true;
                }

                if (_isPasting)
                {
                    HighLighter.GetDefinedVariablesAndFunctions(InputText, IsComplex);
                    HighLightPastedText();
                    SetAutoIndent();
                    var p = RichTextBox.Selection.End.Paragraph;
                    if (p is not null)
                        RichTextBox.CaretPosition = FindPositionAtOffset(p, _pasteOffset);
                    _isPasting = false;
                }
                Record();
                IsSaved = false;
                if (IsCalculated)
                {
                    if (!IsAutoRun)
                    {
                        IsCalculated = false;
                        ShowHelp();
                    }
                }
                if (!_isPasting)
                {
                    string text = InputText;
                    await Task.Run(() => HighLighter.GetDefinedVariablesAndFunctions(text, IsComplex));
                    await Task.Run(DispatchAutoIndent);
                }
                await Task.Run(DispatchLineNumbers);
                await Task.Run(DispatchHighLightFromCurrent);
            }
        }

        private void FillAutoCompleteWithDefinedVariablesAndFunctions()
        {
            AutoCompleteListBox.Items.Filter = null;
            AutoCompleteListBox.Items.SortDescriptions.Clear();
            var items = AutoCompleteListBox.Items;
            for (int i = items.Count - 1; i >= _autoCompleteCount; --i)
                items.RemoveAt(i);

            var lineNumber = CurrentLineNumber;
            foreach (string s in HighLighter.DefinedVariables.Keys)
                if (HighLighter.DefinedVariables[s] < lineNumber)
                    items.Add(new ListBoxItem()
                    {
                        Content = s,
                        Foreground = Brushes.Blue
                    });

            foreach (string s in HighLighter.DefinedFunctions.Keys)
                if (HighLighter.DefinedFunctions[s] < lineNumber)
                    items.Add(new ListBoxItem()
                    {
                        Content = s + "()",
                        FontWeight = FontWeights.Bold
                    });
        }

        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var tps = RichTextBox.Selection.Start;
            var tpe = RichTextBox.Selection.End;
            if (tps.Paragraph is null && tpe.Paragraph is null)
                return;

            var p = tps.Paragraph;
            if (p is null)
                p = tpe.Paragraph;

            if (!ReferenceEquals(_currentParagraph, tps.Paragraph) &&
                !ReferenceEquals(_currentParagraph, tpe.Paragraph))
            {
                _isTextChangedEnabled = false;
                HighLighter.Parse(_currentParagraph, IsComplex, CurrentLineNumber);
                if (p is not null)
                {
                    _currentParagraph = p;
                    HighLighter.Clear(_currentParagraph);
                    FillAutoCompleteWithDefinedVariablesAndFunctions();
                }
                e.Handled = true;
                //RichTextBox.Selection.Select(tps, tpe);
                _isTextChangedEnabled = true;
                if (_autoRun)
                {
                    var offset = RichTextBox.CaretPosition.GetOffsetToPosition(_document.ContentEnd);
                    AutoRun(offset <= 2);
                }
            }
            if (p is not null && tpe.GetOffsetToPosition(tps) == 0)
            {
                _isTextChangedEnabled = false;
                var tr = new TextRange(p.ContentStart, p.ContentEnd);
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                HighLighter.HighlightBrackets(p, tpe.GetTextRunLength(LogicalDirection.Backward));
                _isTextChangedEnabled = true;
            }
        }
        
        private void RichTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) =>
            Dispatcher.InvokeAsync(DisableInputWindowAsync, DispatcherPriority.ApplicationIdle);

        private async void DisableInputWindowAsync()
        {
            await Task.Delay(200);
            if (RichTextBox.IsKeyboardFocused ||
                AutoCompleteListBox.Visibility == Visibility.Visible ||
                _wbWarper.IsContextMenu)
                return;

            if (_autoRun && IsCalculated)
                AutoRun();
        }

        private void RichTextBox_Paste(object sender, DataObjectPastingEventArgs e)
        {
            var formats = e.DataObject.GetFormats();
            if (formats.Contains("UnicodeText") && !formats.Any(x => x.Contains("Bitmap")))
            {
                e.FormatToApply = "UnicodeText";
                _isPasting = true;
                GetPasteOffset();
            }
            else
                e.CancelCommand();
        }

        private void GetPasteOffset()
        {
            _pasteEnd = RichTextBox.Selection.End;
            var p = _pasteEnd.Paragraph;
            if (p is not null)
                _pasteOffset = new TextRange(_pasteEnd, p.ContentEnd).Text.Length;
            else
                _pasteOffset = 0;
        }

        private DispatcherOperation _lineNumbersDispatcherOperation;
        private void DispatchLineNumbers()
        {
            _lineNumbersDispatcherOperation?.Abort();
            if (_lineNumbersDispatcherOperation?.Status != DispatcherOperationStatus.Executing)
                _lineNumbersDispatcherOperation = 
                    Dispatcher.InvokeAsync(DrawLineNumbers, DispatcherPriority.Render);
        }

        private void DrawLineNumbers()
        {
            if (_document.Blocks.Count == 0)
            {
                LineNumbers.Children.Clear();
                return;
            }
            int j = 0, n = LineNumbers.Children.Count;
            var ff = _document.FontFamily;
            var sz = _document.FontSize - 2;
            var topMax = -sz;
            var tp = RichTextBox.GetPositionFromPoint(new Point(sz, sz), true);
            var b = tp.Paragraph as Block;
            var i = 0;
            foreach (var block in _document.Blocks)
            {
                ++i;
                if (ReferenceEquals(block, b))
                    break;
            }
            while (b is not null)
            {
                var top = b.ElementStart.GetCharacterRect(LogicalDirection.Forward).Top + 1;
                if (top >= topMax)
                {
                    if (top > LineNumbers.ActualHeight)
                        break;
                    if (j < n)
                    {
                        var tb = (TextBlock)LineNumbers.Children[j];
                        tb.FontSize = sz;
                        tb.Margin = new Thickness(0, top, 0, 0);
                        tb.Text = (i).ToString();
                    }
                    else
                    {
                        var tb = new TextBlock
                        {
                            TextAlignment = TextAlignment.Right,
                            Width = 35,
                            FontSize = sz,
                            FontFamily = ff,
                            Foreground = Brushes.DarkCyan,
                            Margin = new Thickness(0, top, 0, 0),
                            Text = (i).ToString()
                        };
                        LineNumbers.Children.Add(tb);
                    }
                    ++j;
                }
                b = b.NextBlock;
                ++i;
            }
            if (j < n)
                LineNumbers.Children.RemoveRange(j, n - j);
            _sizeChanged = false;
        }

        private void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            _isTextChangedEnabled = false;
            RichTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            _isTextChangedEnabled = true;
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    AutoRun(true);
                    e.Handled = true;
                }
                else
                    RichTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
            }
            else if (e.Key == Key.Back)
            {
                var tp = RichTextBox.Selection.Start;
                var selLength = tp.GetOffsetToPosition(RichTextBox.Selection.End);
                _forceBackSpace = tp.IsAtLineStartPosition && tp.Paragraph.TextIndent > 0 && selLength == 0;
            }
            else
                _forceBackSpace = false;

            if (AutoCompleteListBox.Visibility == Visibility.Visible)
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
                        AutoCompleteListBox.Visibility = Visibility.Hidden;
                        return;
                    case Key.Up:
                    case Key.Down:
                        if (e.Key == Key.Down ^ AutoCompleteListBox.VerticalAlignment == VerticalAlignment.Bottom)
                        {
                            AutoCompleteListBox.Focus();
                            e.Handled = true;   
                        }
                        else
                            AutoCompleteListBox.Visibility = Visibility.Hidden;

                        ((ListBoxItem)AutoCompleteListBox.SelectedItem).Focus();
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
        }

        private DispatcherOperation _autoIndentDispatcherOperation;

        private void DispatchAutoIndent()
        {
            _autoIndentDispatcherOperation?.Abort();
            if (_autoIndentDispatcherOperation?.Status != DispatcherOperationStatus.Executing)
                _autoIndentDispatcherOperation = 
                    Dispatcher.InvokeAsync(AutoIndent, DispatcherPriority.ApplicationIdle);
        }

        private void AutoIndent()
        {
            var p = RichTextBox.Selection.End.Paragraph;
            if (p is null)
                p = (Paragraph)_document.Blocks.FirstBlock;
            else if (p.PreviousBlock is not null)
                p = (Paragraph)p.PreviousBlock;

            if (p is null)
            {
                p = new Paragraph(new Run());
                _document.Blocks.Add(p);
            }
            var indent = 0.0;
            var i = 0;
            var pp = ((Paragraph)p.PreviousBlock);
            if (pp is not null)
            {
                indent = pp.TextIndent;
                var s = new TextRange(pp.ContentStart, pp.ContentEnd).Text.ToLowerInvariant();
                if (s.Length > 0 && s[0] == '#' && (s.StartsWith("#if") || s.StartsWith("#else") || s.StartsWith("#repeat") || s.StartsWith("#def")))
                    indent += AutoIndentStep;
            }

            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            while (p is not null)
            {
                if (!UpdateIndent(p, ref indent))
                {
                    if (p.TextIndent == indent)
                    {
                        ++i;
                        if (i > 5)
                            break;
                    }
                    else
                    {
                        p.TextIndent = indent;
                        i = 0;
                    }
                }
                p = (Paragraph)p.NextBlock;
            }
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private void SetAutoIndent()
        {
            var indent = 0.0;
            var p = (Paragraph)_document.Blocks.FirstBlock;

            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            while (p is not null)
            {
                if (!UpdateIndent(p, ref indent))
                    p.TextIndent = indent;

                p = (Paragraph)p.NextBlock;
            }
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private static bool UpdateIndent(Paragraph p, ref double indent)
        {
            var s = new TextRange(p.ContentStart, p.ContentEnd).Text.ToLowerInvariant().Trim();
            if (s.Length > 0 && s[0] == '#')
            {
                if (!(s.StartsWith("#if") ||
                    s.StartsWith("#el") ||
                    s.StartsWith("#en") ||
                    s.StartsWith("#re") ||
                    s.StartsWith("#lo") ||
                    s.StartsWith("#de")))
                    return false;
                else if (s.StartsWith("#if") || s.StartsWith("#repeat") || s.StartsWith("#def"))
                {
                    p.TextIndent = indent;
                    indent += AutoIndentStep;
                }
                else if (indent > 0 && (s.StartsWith("#end if") || s.StartsWith("#loop") || s.StartsWith("#end def")))
                {
                    indent -= AutoIndentStep;
                    p.TextIndent = indent;
                }
                else
                    p.TextIndent = Math.Max(indent - AutoIndentStep, 0);

                return true;
            }
            return false;
        }

        private void SaveInputToCode(string[] values)
        {
            var p = (Paragraph)_document.Blocks.FirstBlock;
            int i = 0, n = values.Length;
            while (p is not null)
            {
                if (p.Tag is Queue<string> cache)
                    for (int j = 0, count = cache.Count; j < count; ++j)
                    {
                        cache.Dequeue();
                        if (i < n)
                            cache.Enqueue(values[i++]);
                    }

                else
                    foreach (var inline in p.Inlines)
                        if (i < n && inline.ToolTip is ToolTip tt)
                            tt.Content = values[i++];

                p = (Paragraph)p.NextBlock;
            }
        }

        private string[] ReadInputFromCode()
        {
            var p = (Paragraph)_document.Blocks.FirstBlock;
            var values = new List<string>();
            while (p is not null)
            {
                if (p.Tag is Queue<string> cache)
                    for (int i = 0, count = cache.Count; i < count; ++i)
                    {
                        var s = cache.Dequeue();
                        values.Add(s);
                        cache.Enqueue(s);
                    }

                else if (p.Tag is bool b && b)
                    foreach (var inline in p.Inlines)
                        if (inline.ToolTip is ToolTip tt)
                            values.Add(tt.Content?.ToString());

                p = (Paragraph)p.NextBlock;
            }
            return values.ToArray<string>();
        }

        private void HighLightAll()
        {
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            HighLighter.GetDefinedVariablesAndFunctions(InputText, IsComplex);
            var p = (Paragraph)_document.Blocks.FirstBlock;
            var i = 1;
            while (p is not null)
            {
                if (_forceHighlight)
                {
                    var t = new TextRange(p.ContentStart, p.ContentEnd);
                    t.Text = t.Text.TrimStart('\t');
                }
                if (p.Tag is null)
                    HighLighter.Parse(p, IsComplex, i);

                p = (Paragraph)p.NextBlock;
                ++i;
            }
            _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private int _cancelHighLightFromCurrentId = int.MinValue;
        private DispatcherOperation _highLightFromCurrentDispatcherOperation;

        private void DispatchHighLightFromCurrent()
        {
            if (_cancelHighLightFromCurrentId == int.MaxValue)
                _cancelHighLightFromCurrentId = int.MinValue;

            _cancelHighLightFromCurrentId++;
            _highLightFromCurrentDispatcherOperation?.Abort();
            Task.Delay(500).ContinueWith(delegate
            {
                if (_highLightFromCurrentDispatcherOperation?.Status != DispatcherOperationStatus.Executing)
                    _highLightFromCurrentDispatcherOperation =
                        Dispatcher.InvokeAsync(HighLightFromCurrent, DispatcherPriority.ApplicationIdle);
            });
        }

        private void HighLightFromCurrent()
        {
            if (_currentParagraph is null)
                return;

            var cancelId = _cancelHighLightFromCurrentId;
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            var p = (Paragraph)_currentParagraph.NextBlock;
            var lineNumber = GetLineNumber(p);
            while (p is not null)
            {
                if (cancelId != _cancelHighLightFromCurrentId)
                    break;
                HighLighter.Parse(p, IsComplex, lineNumber++);
                p = (Paragraph)p.NextBlock;
            }
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private void HighLightPastedText()
        {
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            var p = _pasteEnd.Paragraph;
            _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            if (p is null)
                p = (Paragraph)_document.Blocks.FirstBlock;

            var lineNumber = GetLineNumber(p);
            while (p != _currentParagraph)
            {
                HighLighter.Parse(p, IsComplex, lineNumber++);
                p = (Paragraph)p.NextBlock;
            }
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private void RichTextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            _isPasting = true;
            GetPasteOffset();
        }

        private void RichTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                var d = RichTextBox.FontSize + Math.CopySign(2, e.Delta);
                if (d > 4 && d < 42)
                {
                    RichTextBox.FontSize = d;
                    Task.Run(DispatchLineNumbers);
                }
            }
        }

        private void InvHypButton_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            b.Tag = !(bool)b.Tag;
            if ((bool)b.Tag)
                b.FontWeight = FontWeights.Bold;
            else
                b.FontWeight = FontWeights.Normal;

            string pref = string.Empty, post = string.Empty;
            if ((bool)InvButton.Tag)
                pref = "a";

            if ((bool)HypButton.Tag)
                post = "h";

            double fs = 14;
            if ((bool)InvButton.Tag && (bool)HypButton.Tag)
                fs = 12;

            SinButton.Content = pref + "sin" + post;
            SinButton.Tag = (string)SinButton.Content + '(';
            SinButton.FontSize = fs;
            CosButton.Content = pref + "cos" + post;
            CosButton.Tag = (string)CosButton.Content + '(';
            CosButton.FontSize = fs;
            TanButton.Content = pref + "tan" + post;
            TanButton.Tag = (string)TanButton.Content + '(';
            TanButton.FontSize = fs;
            CotButton.Content = pref + "cot" + post;
            CotButton.Tag = (string)CotButton.Content + '(';
            CotButton.FontSize = fs;
        }

        private void ColorScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _parser.Settings.Plot.ColorScale = (PlotSettings.ColorScales)ColorScaleComboBox.SelectedIndex;
            _parser.Settings.Plot.Shadows = ShadowsCheckBox.IsChecked ?? false;
            ClearOutput();
        }

        private void LightDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _parser.Settings.Plot.LightDirection = (PlotSettings.LightDirections)LightDirectionComboBox.SelectedIndex;
            ClearOutput();
        }


        private void ShadowsCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            _parser.Settings.Plot.Shadows = ShadowsCheckBox.IsChecked ?? false;
            ClearOutput();
        }

        private void SmoothCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            _parser.Settings.Plot.SmoothScale = SmoothCheckBox.IsChecked ?? false;
            ClearOutput();
        }

        private void AdaptiveCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            _parser.Settings.Plot.IsAdaptive = AdaptiveCheckBox.IsChecked ?? false;
            ClearOutput();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _isParsing)
            {
                _autoRun = false;
                _parser.Cancel();
            }
        }

        bool _sizeChanged = false;
        private void RichTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _sizeChanged = true;
            MoveAutoComplete();
            _lineNumbersDispatcherOperation?.Abort();
            _lineNumbersDispatcherOperation = Dispatcher.InvokeAsync(DrawLineNumbers, DispatcherPriority.ApplicationIdle);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e) =>
            Application.Current.Shutdown();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Top < 0)
                Top = 0;
            var h = SystemParameters.PrimaryScreenHeight;

            if (Height > h)
                Height = h;
            ReadRecentFiles();
        }

        private void WebBrowser_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                IsSaved = false;
        }

        private void Input_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var r = (Run)sender;
                var tt = (ToolTip)(r).ToolTip;
                r = (Run)r.PreviousInline;
                var prompt = ((Run)(r?.PreviousInline))?.Text + r?.Text;
                var content = string.Empty;
                if (tt.Content is string s)
                    content = s;
#if BG
                string message = $"Въведете стойност на входен параметър: {prompt}";
#else
                string message = $"Enter input parameter value: {prompt}";
#endif
                if (InputBox.Show("Calcpad", message, ref content))
                    tt.Content = content;

                IsSaved = false;
                if (IsCalculated)
                {
                    if (IsAutoRun)
                    {
                        _isTextChangedEnabled = false;
                        AutoRun();
                        _isTextChangedEnabled = true;
                    }
                    else
                    {
                        ShowHelp();
                        IsCalculated = false;
                    }
                }
                e.Handled = true;
            }
        }

        private void Logo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var info = new ProcessStartInfo
            {
                FileName = "https://calcpad.eu",
                UseShellExecute = true
            };
            Process.Start(info);
        }

        private void PdfButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                DefaultExt = ".pdf",
                Filter = "Pdf File (*.pdf)|*.pdf",
                FileName = Path.ChangeExtension(Path.GetFileName(CurrentFileName), "pdf"),
                InitialDirectory = File.Exists(CurrentFileName) ? Path.GetDirectoryName(CurrentFileName) : DocumentPath,
                OverwritePrompt = true
            };
            var result = (bool)dlg.ShowDialog();
            if (result)
                SavePdf(dlg.FileName);
        }


        private void SavePdf(string pdfFileName)
        {

            var htmlFileName = Path.ChangeExtension(pdfFileName, "html");
            var html = _wbWarper.GetContents();
            WriteFile(htmlFileName, html);
            var startInfo = new ProcessStartInfo
            {
                FileName = AppInfo.Path + "wkhtmltopdf.exe"
            };
            const string s = " --enable-local-file-access --disable-smart-shrinking --page-size A4  --margin-bottom 15 --margin-left 15 --margin-right 10 --margin-top 15 ";
            if (htmlFileName.Contains(' '))
                startInfo.Arguments = s + '\"' + htmlFileName + "\" \"" + pdfFileName + '\"';
            else
                startInfo.Arguments = s + htmlFileName + " " + pdfFileName;

            startInfo.UseShellExecute = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            var process = Process.Start(startInfo);
            process.WaitForExit();
            //var html = _wbWarper.GetContents();
            //var pdfPrinter = new ChromiumPdfPrinter();
            //await pdfPrinter.PrintToPdfAsync(html, pdfFileName);
            process = new Process
            {
                StartInfo = new ProcessStartInfo(pdfFileName)
                {
                    UseShellExecute = true
                }
            };
            process.Start();
            File.Delete(htmlFileName);
        }

        private void UnitsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                ExpressionParser.IsUs = sender.Equals(US);
                ClearOutput();
            }
        }

        private void WebBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            //OutputFrame.BorderBrush = SystemColors.ActiveBorderBrush;
            if (WebBrowser.Source is not null && WebBrowser.Source.Fragment == "#0")
            {
                var s = _wbWarper.GetLinkData();
                if (!string.IsNullOrEmpty(s))
                {
                    if (IsCalculated)
                        LineClicked(s);
                    else if (!IsWebForm)
                        LinkClicked(s);
                }
            }
            else if (_isSaving)
            {
                var text = GetInputTextAndValues();
                WriteFile(CurrentFileName, text);
                _isSaving = false;
                IsSaved = true;
            }
            else if (IsWebForm || IsCalculated)
            {
                SetUnits();
                if (!IsWebForm)
                {
                    if (_scrollOutput)
                        ScrollOutput();
                    else if (_scrollY != 0)
                    {
                        _wbWarper.ScrollY = _scrollY;
                        _scrollY = 0;
                    }   
                }
                if (!IsCalculated)
                    _wbWarper.ClearHighlight();
            }
        }

        private void DecimalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
            DecimalsTextBox.Text = (15 - e.NewValue).ToString();

        private void Record() => _undoMan.Record(InputText, RichTextBox.Selection.Start, ReadInputFromCode());

        private void ChangeCaseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (FrameworkElement element in GreekLettersWarpPanel.Children)
            {
                if (element is TextBlock tb)
                {
                    char c = tb.Text[0];
                    const int delta = 'Α' - 'α';
                    if (c == 'ς')
                        c = 'Σ';
                    else if (c == 'ϑ')
                        c = '∡';
                    else if (c == '∡')
                        c = 'ϑ';
                    else if (c == 'ø')
                        c = 'Ø';
                    else if (c == 'Ø')
                        c = 'ø';
                    else if (c >= 'α' && c <= 'ω')
                        c = (char)(c + delta);
                    else if ((c == 'Σ') && tb.Tag is string s)
                        c = s[0];
                    else if (c >= 'Α' && c <= 'Ω')
                        c = (char)(c - delta);
                    else if (c == '′')
                        c = '‴';
                    else if (c == '″')
                        c = '⁗';
                    else if (c == '‴')
                        c = '′';
                    else if (c == '⁗')
                        c = '″';
                    tb.Text = c.ToString(); 
                }
            }
        }

        private static char LatinGreekChar(char c) => c switch
        {
            >= 'a' and <= 'z' => GreekLetters[c - 'a'],
            'V' => '∡',
            'J' => 'Ø',
            >= 'A' and <= 'Z' => (char)(GreekLetters[c - 'A'] + 'Α' - 'α'),
            >= 'α' and <= 'ω' => LatinLetters[c - 'α'],
            >= 'Α' and <= 'Ω' => (char)(LatinLetters[c - 'Α'] + 'A' - 'a'),
            'ϑ' => 'v',
            'ø' => 'f',
            'Ø' => 'F',
            _ => c
        };

        private void RichTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsCalculated)
            {
                _scrollY = _wbWarper.ScrollY;
                ScrollOutput();
            }
        }

        private void WebBrowser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                AutoRun(true);
                e.Handled = true;
            }   
        }

        private void AutoRunCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                if (IsAutoRun && !IsCalculated)
                    Calculate();

                RichTextBox.Focus();
                Keyboard.Focus(RichTextBox);
            }
        }

        private void AutoRunCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RichTextBox.Focus();
            Keyboard.Focus(RichTextBox);
        }

        private void RichTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isTextChangedEnabled = false;
            RichTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            AutoCompleteListBox.Visibility = Visibility.Hidden;
            _isTextChangedEnabled = true;
        }

        private void FindReplace_BeginSearch(object sender, EventArgs e)
        {
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            RichTextBox.Focus();
        }

        private void FindReplace_EndSearch(object sender, EventArgs e)
        {
            RichTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.LightBlue);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private string[] _inputValues = null;
        private void FindReplace_BeginReplace(object sender, EventArgs e)
        {
            _inputValues = ReadInputFromCode();
        }

        private void FindReplace_EndReplace(object sender, EventArgs e)
        {
            Task.Run(() => Dispatcher.InvokeAsync(
                delegate {
                    HighLightAll(); 
                    SaveInputToCode(_inputValues); 
                }, 
                DispatcherPriority.Send));
            Task.Run(() => Dispatcher.InvokeAsync(SetAutoIndent, DispatcherPriority.Normal));
        }

        private TextPointer _autoCompleteStart;
        private void InitAutoComplete(string input)
        {
            var c = string.IsNullOrEmpty(input) ? '\0' : input[0];
            bool isAutoCompleteTrigger = HighLighter.IsLetter(c);
            if (!isAutoCompleteTrigger)
            {
                if (AutoCompleteListBox.Visibility == Visibility.Hidden)
                    isAutoCompleteTrigger = c == '#' || c == '$';
                else
                    isAutoCompleteTrigger = c == '/' || c == '*' || c == '^';
            }

            if (isAutoCompleteTrigger)
            {
                var tp = RichTextBox.Selection.Start;
                if (AutoCompleteListBox.Visibility == Visibility.Hidden)
                {
                    var p = tp.Paragraph;
                    var text = p is null ? String.Empty : new TextRange(p.ContentStart, tp).Text;
                    var i = text.Length - 1;
                    var c0 = i < 0 ? '\0' : text[i];   
                    if (!HighLighter.IsLetter(c0))
                    {
                        if (i < 0 && _currentParagraph is not null)
                            _autoCompleteStart = _currentParagraph.ContentStart;
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
                AutoCompleteListBox.Visibility = Visibility.Hidden;
        }

        private void MoveAutoComplete()
        {
            if (_autoCompleteStart is null || AutoCompleteListBox.Visibility == Visibility.Hidden)
                return;

            var verticalAlignment = AutoCompleteListBox.VerticalAlignment;
            SetAutoCompletePosition();
            var r = _autoCompleteStart.GetCharacterRect(LogicalDirection.Forward);
            var y = r.Top + r.Height / 2;
            if (y < RichTextBox.Margin.Top || y > RichTextBox.Margin.Top + RichTextBox.ActualHeight)
            {
                AutoCompleteListBox.Visibility = Visibility.Hidden;
                return;
            }
            if (AutoCompleteListBox.VerticalAlignment != verticalAlignment)
                SortAutoComplete();
        }

        private void SetAutoCompletePosition()
        {
            var rect = _autoCompleteStart.GetCharacterRect(LogicalDirection.Forward);
            var x = RichTextBox.Margin.Left + rect.Left - 2;
            var y = RichTextBox.Margin.Top + rect.Bottom;
            if (y > RichTextBox.ActualHeight - AutoCompleteListBox.MaxHeight)
            {
                y = RichTextBox.Margin.Bottom + RichTextBox.ActualHeight - rect.Top;
                AutoCompleteListBox.Margin = new Thickness(x, 0, 0, y);
                AutoCompleteListBox.VerticalAlignment = VerticalAlignment.Bottom;
            }
            else
            {
                AutoCompleteListBox.Margin = new Thickness(x, y, 0, 0);
                AutoCompleteListBox.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        private bool IsInComment()
        {
            var tp = RichTextBox.Selection.Start;
            var text = tp.GetTextInRun(LogicalDirection.Backward);
            var i = text.IndexOfAny(new[] { '\'', '\"' });
            if (i < 0)
                return false;
            var c = text[i];
            i = text.Count(x => x == c);
            return (i % 2 == 1);
        }

        private void UpdateAutoComplete(string input)
        {
            var offset = _autoCompleteStart.GetOffsetToPosition(RichTextBox.Selection.Start);
            if (offset <= 0)
                AutoCompleteListBox.Visibility = Visibility.Hidden;
            else
            {
                string s = _autoCompleteStart.GetTextInRun(LogicalDirection.Backward);
                char c = string.IsNullOrEmpty(s) ? '\0' : s[0];
                s = new TextRange(_autoCompleteStart, RichTextBox.Selection.End).Text;
                if (input is null)
                {
                    if (s.Length > 1)
                        s = s[..^1];
                    else
                    {
                        AutoCompleteListBox.Visibility = Visibility.Hidden;
                        return;
                    }
                }
                else
                    s += input;

                Dispatcher.InvokeAsync(() => FilterAutoComplete(c, s), DispatcherPriority.Send);
            }
        }

        private void FilterAutoComplete(char c, string s)
        {
            if (s is null)
                AutoCompleteListBox.Items.Filter = null;
            else if (HighLighter.IsDigit(c))
                AutoCompleteListBox.Items.Filter = x => ((string)((ListBoxItem)x).Content).StartsWith(s) && ((ListBoxItem)x).Foreground == Brushes.DarkCyan;
            else
                AutoCompleteListBox.Items.Filter = x => ((string)((ListBoxItem)x).Content).StartsWith(s);

            if (AutoCompleteListBox.HasItems)
            {
                SortAutoComplete();
                AutoCompleteListBox.Visibility = Visibility.Visible;
            }
            else
                AutoCompleteListBox.Visibility = Visibility.Hidden;
        }

        private void SortAutoComplete()
        {
            AutoCompleteListBox.Items.SortDescriptions.Clear();
            if (AutoCompleteListBox.VerticalAlignment == VerticalAlignment.Bottom)
            {
                AutoCompleteListBox.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Descending));
                AutoCompleteListBox.SelectedIndex = AutoCompleteListBox.Items.Count - 1;
            }
            else
            {
                AutoCompleteListBox.Items.SortDescriptions.Add(new SortDescription("Content", ListSortDirection.Ascending));
                AutoCompleteListBox.SelectedIndex = 0;
            }
            AutoCompleteListBox.ScrollIntoView(AutoCompleteListBox.SelectedItem);
        }

        private void EndAutoComplete()
        {
            int i = _currentParagraph.ContentEnd.GetOffsetToPosition(RichTextBox.Selection.End);
            string s = (string)((ListBoxItem)AutoCompleteListBox.SelectedItem).Content;
            new TextRange(_autoCompleteStart, RichTextBox.Selection.End).Text = s;
            AutoCompleteListBox.Visibility = Visibility.Hidden;
            if (s.Length > 0)
            {
                char c = s[^1];
                if (c == ')' || c == '}')
                    --i;
            }
            var tp = _currentParagraph.ContentEnd.GetPositionAtOffset(i);
            if (tp is not null)
                RichTextBox.Selection.Select(tp, tp);
            RichTextBox.Focus();
        }

        private void RichTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsInComment())
            {
                Task.Run(() => Dispatcher.InvokeAsync(() => InitAutoComplete(e.Text) , DispatcherPriority.Send));            
            }
        }

        private void AutoCompleteListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ListBoxItem)  
                EndAutoComplete();
        }

        private void AutoCompleteListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Escape)
                RichTextBox.Focus();
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
    }
}