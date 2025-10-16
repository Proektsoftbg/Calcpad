using Calcpad.Core;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Calcpad.Wpf
{
    public partial class MainWindow : Window
    {
        //Culture
        private static readonly string _currentCultureName = "en";

        //Static resources
        private static readonly char[] GreekLetters = ['α', 'β', 'χ', 'δ', 'ε', 'φ', 'γ', 'η', 'ι', 'ø', 'κ', 'λ', 'μ', 'ν', 'ο', 'π', 'θ', 'ρ', 'σ', 'τ', 'υ', 'ϑ', 'ω', 'ξ', 'ψ', 'ζ'];
        private static readonly char[] LatinLetters = ['a', 'b', 'g', 'd', 'e', 'z', 'h', 'q', 'i', 'k', 'l', 'm', 'n', 'x', 'o', 'p', 'r', 's', 's', 't', 'u', 'f', 'c', 'y', 'w'];
        private static readonly Regex HtmlAnchorHrefRegex = new(@"(?<=<a\b[^>]*?\bhref\s*=\s*"")(?!#)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HtmlAnchorTargetRegex = new(@"\s+\btarget\b\s*=\s*""\s*_\w+\s*""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HtmlImgPrevRegex = new(@"src\s*=\s*""\s*\.\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HtmlImgCurRegex = new(@"src\s*=\s*""\s*\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HtmlImgAnyRegex = new(@"src\s*=\s*""\s*\.\.?(.+?)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal readonly struct AppInfo
        {
            static AppInfo()
            {
                Path = AppDomain.CurrentDomain.BaseDirectory;
                Name = AppDomain.CurrentDomain.FriendlyName + ".exe";
                FullName = System.IO.Path.Combine(Path, Name);
                Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Title = " Calcpad VM " + Version[0..(Version.LastIndexOf('.'))];
                DocPath = Path + "doc";
                if (!Directory.Exists(DocPath))
                    DocPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Calcpad";
            }
            internal static readonly string Path;
            internal static readonly string Name;
            internal static readonly string FullName;
            internal static readonly string Version;
            internal static readonly string Title;
            internal static readonly string DocPath;
        }
        private const double AutoIndentStep = 28.0;

        //Find and Replace
        private readonly FindReplace _findReplace = new();
        private FindReplaceWindow _findReplaceWindow;

        //Parsers
        private readonly ExpressionParser _parser;
        private readonly MacroParser _macroParser;
        private readonly HighLighter _highlighter;

        //Html strings
        private readonly string _htmlWorksheet;
        private readonly string _htmlParsingPath;
        private readonly string _htmlParsingUrl;
        private readonly string _htmlHelpPath;
        private readonly string _htmlSource;
        private string _htmlUnwarpedCode;

        //RichTextBox Document
        private readonly FlowDocument _document;
        private Paragraph _currentParagraph;
        private Paragraph _lastModifiedParagraph;

        private readonly StringBuilder _stringBuilder = new(10000);
        private readonly UndoManager _undoMan;
        private readonly WebView2Wrapper _wv2Warper;
        private readonly InsertManager _insertManager;
        private readonly AutoCompleteManager _autoCompleteManager;

        private readonly string _readmeFileName;
        private string DocumentPath { get; set; }
        private string _cfn;
        private string _tempDir;
        private string CurrentFileName
        {
            get => _cfn;
            set
            {
                _cfn = value;
                if (string.IsNullOrEmpty(value))
                {
                    _tempDir = Path.GetRandomFileName() + '\\';
                    Title = AppInfo.Title;
                }
                else
                {
                    var path = Path.GetDirectoryName(value);
                    if (string.IsNullOrWhiteSpace(path))
                        _cfn = Path.Combine(DocumentPath, value);
                    else
                        SetCurrentDirectory(path);
                    Title = AppInfo.Title + " - " + Path.GetFileName(value);
                    _tempDir = Path.GetFileNameWithoutExtension(value) + '\\';
                }
            }
        }
        //State variables
        private readonly string _svgTyping;
        private bool _isSaving;
        private bool _isSaved;
        private bool _isParsing;
        private bool _isPasting;
        private bool _isTextChangedEnabled;
        private readonly double _inputHeight;
        private bool _mustPromptUnlock;
        private bool _forceHighlight;
        private int _countKeys = int.MinValue;
        private bool _forceBackSpace;
        private int _pasteOffset;
        private int _currentLineNumber;
        private int _currentOffset;
        private TextPointer _pasteEnd;
        private bool _scrollOutput;
        private double _scrollY;
        private bool _autoRun;
        private readonly double _screenScaleFactor;
        private bool _calculateOnActivate;
        private bool _isWebView2Focused;
        private Brush _borderBrush;

        //Private properites
        private bool IsComplex => _parser.Settings.Math.IsComplex;
        internal bool IsSaved
        {
            get => _isSaved;
            private set
            {
                SaveButton.IsEnabled = !value;
                MenuSave.IsEnabled = !value;
                _isSaved = value;
            }
        }

        private bool IsWebForm
        {
            get => WebFormButton.Tag.ToString() == "T";
            set => SetWebForm(value);
        }
        private string InputText => new TextRange(_document.ContentStart, _document.ContentEnd).Text;
        private int InputTextLength => _document.ContentEnd.GetOffsetToPosition(_document.ContentStart);
        private SpanLineEnumerator InputTextLines => InputText.EnumerateLines();
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

        private bool IsWebView2Focused
        {
            get => _isWebView2Focused;
            set
            {
                if (value == _isWebView2Focused) return;
                _isWebView2Focused = value;
                _findReplace.IsWebView2Focused = value;
                InputFrame.BorderBrush = value ? _borderBrush : SystemColors.ActiveBorderBrush;
                OutputFrame.BorderBrush = value ? SystemColors.ActiveBorderBrush : _borderBrush;
            }
        }
        private bool DisplayUnwarpedCode => CodeCheckBorder.Visibility == Visibility.Visible && CodeCheckBox.IsChecked.Value;
        private bool IsUnwarpedCode => WebViewer.Tag is bool b && b;
        public MainWindow()
        {
            _parser = new();
            _highlighter = new();
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(_currentCultureName);
            InitializeComponent();
            _borderBrush = OutputFrame.BorderBrush;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _inputHeight = InputGrid.RowDefinitions[1].Height.Value;
            ToolTipService.InitialShowDelayProperty.OverrideMetadata(
                typeof(DependencyObject),
                new FrameworkPropertyMetadata(500));
            HighLighter.IncludeClickEventHandler = Include_Click;
            UserDefined.Include = Include;
            LineNumbers.ClipToBounds = true;
            SetCurrentDirectory();
            var docPath = AppInfo.DocPath;
            var docUrl = $"file:///{docPath.Replace("\\", "/")}";
            var htmlExt = AddCultureExt("html");
            _htmlWorksheet = ReadTextFromFile($"{docPath}\\template{htmlExt}").Replace("https://calcpad.local", docUrl);
            _htmlParsingPath = $"{docPath}\\parsing{htmlExt}";
            _htmlParsingUrl = $"{docUrl}/parsing{htmlExt}";
            _htmlHelpPath = GetHelp(MainWindowResources.calcpad_download_help_html);
            _htmlSource = ReadTextFromFile($"{docPath}\\source.html");
            _svgTyping = $"<img style=\"height:1em;\" src=\"{docUrl}/typing.gif\" alt=\"...\">";
            _readmeFileName = $"{docPath}\\readme{htmlExt}";
            InvButton.Tag = false;
            HypButton.Tag = false;
            RichTextBox.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(RichTextBox_Scroll));
            DataObject.AddPastingHandler(RichTextBox, RichTextBox_Paste);
            _document = RichTextBox.Document;
            _currentParagraph = _document.Blocks.FirstBlock as Paragraph;
            _currentLineNumber = 1;
            HighLighter.Clear(_currentParagraph);
            _undoMan = new UndoManager();
            Record();
            _wv2Warper = new WebView2Wrapper(WebViewer, $"{docPath}\\blank.html");
            _macroParser = new MacroParser
            {
                Include = Include
            };
            _insertManager = new(RichTextBox);
            _autoCompleteManager = new(RichTextBox, AutoCompleteListBox, Dispatcher, _insertManager);
            _screenScaleFactor = ScreenMetrics.GetWindowsScreenScalingFactor();
            _cfn = string.Empty;
            _isTextChangedEnabled = false;
            IsSaved = true;
            _findReplace.RichTextBox = RichTextBox;
            _findReplace.WebViewer = WebViewer;
            _findReplace.BeginSearch += FindReplace_BeginSearch;
            _findReplace.EndSearch += FindReplace_EndSearch;
            _findReplace.EndReplace += FindReplace_EndReplace;
            _isTextChangedEnabled = true;
        }

        private static string AddCultureExt(string ext) => string.Equals(_currentCultureName, "en", StringComparison.Ordinal) ?
                $".{ext}" :
                $".{_currentCultureName}.{ext}";

        public bool SaveStateAndRestart(string tempFile)
        {
            var text = InputText;
            Clipboard.SetText(text);
            File.WriteAllText(tempFile, text);
            Properties.Settings.Default.TempFile = tempFile;
            Properties.Settings.Default.FileName = CurrentFileName;
            Properties.Settings.Default.Save();
            _isSaved = true;
            Execute(AppInfo.FullName);
            return true;
        }

        private void TryRestoreState()
        {
            var tempFile = Properties.Settings.Default.TempFile;
            if (string.IsNullOrEmpty(tempFile)) return;
            var fileName = Properties.Settings.Default.FileName;
            Properties.Settings.Default.TempFile = null;
            Properties.Settings.Default.FileName = null;
            Properties.Settings.Default.Save();
            var message = MainWindowResources.TryRestoreState_Recovered_SavePrompt;
            var result = MessageBox.Show(
                message,
                "Calcpad",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                FileOpen(tempFile);
                CurrentFileName = fileName;
            }
            catch (Exception ex)
            {
                ShowErrorMessage(
                    string.Format(MainWindowResources.TryRestoreState_Failed, ex.Message, tempFile)
                );
                IsSaved = true;
                Command_New(this, null);
            }
        }

        private void SetCurrentDirectory(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                DocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Calcpad";
                if (!Directory.Exists(DocumentPath))
                    Directory.CreateDirectory(DocumentPath);
            }
            else
                DocumentPath = path;

            Directory.SetCurrentDirectory(DocumentPath);
        }

        private void ForceHighlight()
        {
            if (_forceHighlight)
            {
                RichTextBox.CaretPosition = _document.ContentStart;
                HighLightAll();
                SetAutoIndent();
                _forceHighlight = false;
            }
        }

        private static void SetButton(Control b, bool on)
        {
            if (on)
            {
                b.Tag = "T";
                b.BorderBrush = Brushes.SteelBlue;
                b.Background = Brushes.LightBlue;
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

        private void SetOutputFrameHeader(bool isWebForm)
        {
            OutputFrame.Header = isWebForm ? MainWindowResources.Input : MainWindowResources.Output;
        }
        private void RichTextBox_Scroll(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 && !_sizeChanged && !IsWebForm)
            {
                _autoCompleteManager.MoveAutoComplete();
                DispatchLineNumbers();
                if (e.VerticalChange > 0 && _lastModifiedParagraph is not null)
                {
                    Rect r = _lastModifiedParagraph.ContentStart.GetCharacterRect(LogicalDirection.Forward);
                    if (r.Top < 0.8 * RichTextBox.ActualHeight)
                        DispatchHighLightFromCurrent();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var tag = element.Tag.ToString();
            var index = tag.IndexOf('␣') + 1;
            if (index > 0)
            {
                if (MarkdownCheckBox.IsChecked.Value == true)
                    tag = tag[index..];
                else
                    tag = tag[..(index - 1)];
            }
            RichTextBox.BeginChange();
            if (tag.Contains('‖'))
            {
                if (tag.StartsWith("‖#"))
                    _insertManager.InsertMarkdownHeading(tag);
                else if (tag.StartsWith("<p>", StringComparison.OrdinalIgnoreCase) ||
                    tag.StartsWith("<h", StringComparison.OrdinalIgnoreCase) &&
                    !tag.Equals("<hr/>‖", StringComparison.OrdinalIgnoreCase))
                    _insertManager.InsertHtmlHeading(tag);
                else if (!_insertManager.InsertInline(tag))
                    Dispatcher.InvokeAsync(() =>
                    MessageBox.Show(
                        MainWindowResources.Inline_Html_elements_must_not_cross_text_lines,
                        "Calcpad", MessageBoxButton.OK, MessageBoxImage.Stop));
            }
            else if (tag.Contains('§'))
                InsertLines(tag, "§", false);
            else switch (tag)
                {
                    case null or "": break;
                    case "AC": RemoveLine(); break;
                    case "C": _insertManager.RemoveChar(); break;
                    case "Enter": _insertManager.InsertLine(); break;
                    default:
                        if (tag[0] == '#' ||
                            tag[0] == '$' && (
                                tag.StartsWith("$plot", StringComparison.OrdinalIgnoreCase) ||
                                tag.StartsWith("$map", StringComparison.OrdinalIgnoreCase)
                            ))
                        {
                            var p = RichTextBox.Selection.End.Paragraph;
                            if (p is not null && p.ContentStart?.GetOffsetToPosition(p.ContentEnd) > 0)
                            {
                                var tp = p.ContentEnd.InsertParagraphBreak();
                                tp.InsertTextInRun(tag);
                                p = tp.Paragraph;
                                var lineNumber = GetLineNumber(p);
                                _highlighter.Parse(p, IsComplex, lineNumber);
                                SetAutoIndent();
                                tp = p.ContentEnd;
                                RichTextBox.Selection.Select(tp, tp);
                            }
                            else
                                _insertManager.InsertText(tag);
                        }
                        else
                            _insertManager.InsertText(tag);
                        break;
                }
            if (tag == "Enter")
                CalculateAsync();

            RichTextBox.EndChange();
            RichTextBox.Focus();
            Keyboard.Focus(RichTextBox);
        }

        private void InsertLines(string tag, string delimiter, bool comment)
        {
            var parts = tag.Split(delimiter);
            var p = RichTextBox.Selection.Start.Paragraph;
            var selLength = RichTextBox.Selection.Text.Length;
            TextPointer tp = selLength > 0 ? p.ContentStart : p.ContentEnd;
            var pararaphLength = new TextRange(p.ContentStart, p.ContentEnd).Text.Length;
            if (pararaphLength > 0)
            {
                tp = tp.InsertParagraphBreak();
                if (selLength > 0)
                    tp = tp.Paragraph.PreviousBlock.ContentEnd;
            }
            p = tp.Paragraph;
            var lineNumber = GetLineNumber(p);
            InsertPart(0);
            if (selLength > 0)
                tp = RichTextBox.Selection.End;

            for (int i = 1, len = parts.Length; i < len; ++i)
            {
                p = tp.Paragraph;
                tp = p.ContentEnd.InsertParagraphBreak();
                ++lineNumber;
                InsertPart(i);
            }
            SetAutoIndent();
            tp = tp.Paragraph.ContentEnd;
            RichTextBox.Selection.Select(tp, tp);

            void InsertPart(int i)
            {
                var s = parts[i];
                if (comment && !s.StartsWith('\''))
                    s = '\'' + s;

                tp.InsertTextInRun(s);
                _highlighter.Defined.Get(s, lineNumber);
                _highlighter.Parse(p, IsComplex, lineNumber);
            }
        }

        private async Task AutoRun(bool syncScroll = false)
        {
            if (_isParsing)
                return;

            IsCalculated = true;
            if (syncScroll)
                _scrollOutput = true;

            _scrollY = await _wv2Warper.GetScrollYAsync();
            CalculateAsync();
        }

        private void RemoveLine()
        {
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            if (_document.Blocks.Count <= 1)
            {
                _currentParagraph = _document.Blocks.FirstBlock as Paragraph;
                _currentParagraph.Inlines.Clear();
            }
            else
            {
                _document.Blocks.Remove(RichTextBox.Selection.Start.Paragraph);
                _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            }
            _currentLineNumber = GetLineNumber(_currentParagraph);
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
            if (IsAutoRun)
                AutoRun();
        }

        private int _scrollOutputToLine;
        private double _scrollOffset;
        private async void LineClicked(string data)
        {
            if (int.TryParse(data, out var line) && line > 0)
            {
                if (_highlighter.Defined.HasMacros && !IsUnwarpedCode)
                {
                    _scrollOffset = await _wv2Warper.GetVerticalPositionAsync(line);
                    _scrollOutputToLine = line;
                    await _wv2Warper.NavigateToStringAsync(_htmlUnwarpedCode);
                    WebViewer.Tag = true;
                    CodeCheckBox.IsChecked = true;
                }
                else if (line <= _document.Blocks.Count)
                {
                    var block = _document.Blocks.ElementAt(line - 1);
                    if (!ReferenceEquals(block, _currentParagraph))
                    {
                        var y = block.ContentEnd.GetCharacterRect(LogicalDirection.Forward).Y -
                            _document.ContentStart.GetCharacterRect(LogicalDirection.Forward).Y -
                            await _wv2Warper.GetVerticalPositionAsync(line) +
                            (RichTextBox.Margin.Top - WebViewer.Margin.Top);
                        RichTextBox.ScrollToVerticalOffset(y);
                        RichTextBox.CaretPosition = block.ContentEnd;
                    }
                }
            }
            RichTextBox.Focus();
            Keyboard.Focus(RichTextBox);
        }

        private void LinkClicked(string data)
        {
            RichTextBox.Selection.Text = string.Empty;
            var lines = data.Split(Environment.NewLine);
            var p = RichTextBox.Selection.Start.Paragraph;
            if (lines.Length == 1)
            {
                if ((data[0] == '#' || data[0] == '$') && !p.ContentEnd.IsAtLineStartPosition)
                {
                    var tp = p.ContentEnd.InsertParagraphBreak();
                    RichTextBox.Selection.Select(tp, tp);
                }
                _insertManager.InsertText(data);
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
                    _highlighter.Parse(p, IsComplex, GetLineNumber(p));
                }
                RichTextBox.Selection.Select(tp, tp);
                RichTextBox.EndChange();
                _isTextChangedEnabled = true;
                DispatchAutoIndent();
                Record();
            }
            RichTextBox.Focus();
            Keyboard.Focus(RichTextBox);
        }
        private void CalcButton_Click(object sender, RoutedEventArgs e) => Command_Calculate(null, null);
        private async void Command_Calculate(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsCalculated)
                _scrollY = await _wv2Warper.GetScrollYAsync();

            Calculate();
            if (IsCalculated)
                await _wv2Warper.SetScrollYAsync(_scrollY);
        }

        private void Calculate()
        {
            if (_parser.IsPaused)
                AutoRun();
            else
            {
                IsCalculated = !IsCalculated;
                if (IsWebForm)
                    CalculateAsync(!IsCalculated);
                else if (IsCalculated)
                    CalculateAsync();
                else
                    ShowHelp();
            }
        }

        private void Command_New(object senter, ExecutedRoutedEventArgs e)
        {
            var r = PromptSave();
            if (r == MessageBoxResult.Cancel)
                return;

            if (_isParsing)
                _parser.Cancel();

            _parser.ShowWarnings = true;
            CurrentFileName = string.Empty;
            _document.Blocks.Clear();
            _highlighter.Defined.Clear(IsComplex);
            RichTextBox.CaretPosition = _document.ContentStart;
            if (IsWebForm)
            {
                _mustPromptUnlock = false;
                IsWebForm = false;
                RichTextBox.Focus();
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
                s = Path.GetExtension(CurrentFileName).ToLowerInvariant();

            var dlg = new OpenFileDialog
            {
                DefaultExt = s,
                FileName = '*' + s,
                InitialDirectory = File.Exists(CurrentFileName) ? Path.GetDirectoryName(CurrentFileName) : DocumentPath,
                CheckFileExists = true,
                Multiselect = false,
                Filter = s == ".txt"
                    ? MainWindowResources.Command_Open_Text_File
                    : MainWindowResources.Command_Open_Calcpad_Worksheet
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
                FileSave(CurrentFileName);
        }

        private void ReadSettings()
        {
            ReadRecentFiles();
            var settings = Properties.Settings.Default;
            Real.IsChecked = settings.Numbers == 'R';
            Complex.IsChecked = settings.Numbers == 'C';
            AutoRunCheckBox.IsChecked = settings.AutoRun;
            Deg.IsChecked = settings.Angles == 'D';
            Rad.IsChecked = settings.Angles == 'R';
            Gra.IsChecked = settings.Angles == 'G';
            UK.IsChecked = settings.Units == 'K';
            US.IsChecked = settings.Units == 'S';
            Professional.IsChecked = settings.Equations == 'P';
            Inline.IsChecked = settings.Equations == 'I';
            DecimalsTextBox.Text = settings.Decimals.ToString();
            SubstituteCheckBox.IsChecked = settings.Substitute;
            AdaptiveCheckBox.IsChecked = settings.Adaptive;
            ShadowsCheckBox.IsChecked = settings.Shadows;
            LightDirectionComboBox.SelectedIndex = settings.Direction;
            ColorScaleComboBox.SelectedIndex = settings.Palette;
            SmoothCheckBox.IsChecked = settings.Smooth;
            ExternalBrowserComboBox.SelectedIndex = settings.Browser;
            ZeroSmallMatrixElementsCheckBox.IsChecked = settings.ZeroSmallMatrixElements;
            MaxOutputCountTextBox.Text = settings.MaxOutputCount.ToString();
            EmbedCheckBox.IsChecked = settings.Embed;
            if (settings.WindowLeft > 0) Left = settings.WindowLeft;
            if (settings.WindowTop > 0) Top = settings.WindowTop;
            if (settings.WindowWidth > 0) Width = settings.WindowWidth;
            if (settings.WindowHeight > 0) Height = settings.WindowHeight;
            this.WindowState = (WindowState)settings.WindowState;

            ExpressionParser.IsUs = US.IsChecked ?? false;
            var math = _parser.Settings.Math;
            math.FormatEquations = Professional.IsChecked ?? false;
            math.IsComplex = Complex.IsChecked ?? false;
            math.Degrees = Deg.IsChecked ?? false ? 0 :
                                            Rad.IsChecked ?? false ? 1 :
                                            2;
            math.Substitute = SubstituteCheckBox.IsChecked ?? false;
            math.ZeroSmallMatrixElements = ZeroSmallMatrixElementsCheckBox.IsChecked ?? false;
            math.MaxOutputCount = int.TryParse(MaxOutputCountTextBox.Text, out int i) ? i : 20;
            var plot = _parser.Settings.Plot;
            plot.ImagePath = string.Empty;
            plot.ImageUri = string.Empty;
            plot.VectorGraphics = false;
            plot.ScreenScaleFactor = _screenScaleFactor;
            plot.IsAdaptive = AdaptiveCheckBox.IsChecked ?? false;
            plot.Shadows = ShadowsCheckBox.IsChecked ?? false;
            plot.SmoothScale = SmoothCheckBox.IsChecked ?? false;
            plot.ColorScale = (PlotSettings.ColorScales)ColorScaleComboBox.SelectedIndex;
            plot.LightDirection = (PlotSettings.LightDirections)LightDirectionComboBox.SelectedIndex;
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
                        Header = GetRecentFileName(fileName),
                    };
                    menu.Click += RecentFileList_Click;
                    MenuRecent.Items.Add(menu);
                }
                if (MenuRecent.Items.Count > 0 && (string.IsNullOrEmpty(CurrentFileName) || !File.Exists(CurrentFileName)))
                {
                    var firstMenu = (MenuItem)MenuRecent.Items[0];
                    var path = Path.GetDirectoryName((string)firstMenu.ToolTip);
                    SetCurrentDirectory(path);
                }
            }
            MenuRecent.IsEnabled = j > 0;
            CloneRecentFilesList();
        }

        private string GetRecentFileName(string fileName) => Path.GetFileName(fileName).Replace("_", "__");

        private void WriteSettings()
        {
            WriteRecentFiles();
            var settings = Properties.Settings.Default;
            settings.Numbers = Real.IsChecked ?? false ? 'R' : 'C';
            settings.AutoRun = AutoRunCheckBox.IsChecked ?? false;
            settings.Angles = Deg.IsChecked ?? false ? 'D' :
                              Rad.IsChecked ?? false ? 'R' : 'G';
            settings.Units = UK.IsChecked ?? false ? 'K' : 'S';
            settings.Equations = Professional.IsChecked ?? false ? 'P' : 'I';
            settings.Decimals = byte.TryParse(DecimalsTextBox.Text, out byte b) ? b : (byte)2;
            settings.Substitute = SubstituteCheckBox.IsChecked ?? false;
            settings.Adaptive = AdaptiveCheckBox.IsChecked ?? false;
            settings.Shadows = ShadowsCheckBox.IsChecked ?? false;
            settings.Direction = (byte)LightDirectionComboBox.SelectedIndex;
            settings.Direction = (byte)LightDirectionComboBox.SelectedIndex;
            settings.Palette = (byte)ColorScaleComboBox.SelectedIndex;
            settings.Smooth = SmoothCheckBox.IsChecked ?? false;
            settings.Browser = (byte)ExternalBrowserComboBox.SelectedIndex;
            settings.ZeroSmallMatrixElements = ZeroSmallMatrixElementsCheckBox.IsChecked ?? false;
            settings.MaxOutputCount = int.TryParse(MaxOutputCountTextBox.Text, out int i) ? i : (int)20;
            settings.Embed = EmbedCheckBox.IsChecked ?? false;  
            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            settings.WindowWidth = Width;
            settings.WindowHeight = Height;
            settings.WindowState = (byte)this.WindowState;
            settings.Save();
        }


        private void WriteRecentFiles()
        {
            var n = MenuRecent.Items.Count;
            if (n == 0)
                return;

            var list =
                Properties.Settings.Default.RecentFileList ??
                [];

            list.Clear();
            for (int i = 0; i < n; ++i)
            {
                var menu = (MenuItem)MenuRecent.Items[i];
                var value = (string)menu.ToolTip;
                list.Add(value);
            }

            Properties.Settings.Default.RecentFileList = list;
        }

        private void AddRecentFile(string fileName)
        {
            if (!File.Exists(fileName))
                return;

            var n = MenuRecent.Items.Count;
            for (int i = 0; i < n; ++i)
            {
                var menu = (MenuItem)MenuRecent.Items[i];
                if (!fileName.Equals((string)menu.ToolTip))
                    continue;

                for (int j = i; j > 0; --j)
                {
                    menu = (MenuItem)MenuRecent.Items[j];
                    var previousMenu = (MenuItem)MenuRecent.Items[j - 1];
                    menu.Header = previousMenu.Header;
                    menu.ToolTip = previousMenu.ToolTip;
                }
                var first = (MenuItem)MenuRecent.Items[0];
                first.Header = GetRecentFileName(fileName);
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
                Header = GetRecentFileName(fileName),
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
            var margin = RecentFilesListContextMenu.Margin;
            margin.Left = RecentFliesListButton.Margin.Left;
            RecentFilesListContextMenu.Margin = margin;
            RecentFilesListContextMenu.StaysOpen = true;
            RecentFilesListContextMenu.IsOpen = true;
        }

        private void RecentFileList_Click(object sender, RoutedEventArgs e)
        {
            RecentFilesListContextMenu.IsOpen = false;
            var r = PromptSave();
            if (r == MessageBoxResult.Cancel)
                return;

            var fileName = (string)((MenuItem)sender).ToolTip;
            if (File.Exists(fileName))
                FileOpen(fileName);
        }

        private void Command_SaveAs(object sender, ExecutedRoutedEventArgs e) => FileSaveAs();

        private bool FileSaveAs()
        {
            string s;
            if (!string.IsNullOrWhiteSpace(CurrentFileName))
                s = Path.GetExtension(CurrentFileName).ToLowerInvariant();
            else
                s = ".cpd";

            var dlg = new SaveFileDialog
            {
                FileName = Path.GetFileName(CurrentFileName),
                InitialDirectory = File.Exists(CurrentFileName) ? Path.GetDirectoryName(CurrentFileName) : DocumentPath,
                DefaultExt = s,
                OverwritePrompt = true,
                Filter = s switch
                {
                    ".txt" => MainWindowResources.Command_Open_Text_File,
                    ".cpdz" => MainWindowResources.FileSaveAs_Calcpad_Compiled,
                    _ => MainWindowResources.Command_Open_Calcpad_Worksheet
                }
            };

            var result = (bool)dlg.ShowDialog();
            if (!result)
                return false;

            var fileName = dlg.FileName;
            if (s == ".cpdz" && !IsCpdz())
                fileName = Path.ChangeExtension(fileName, ".cpdz");

            _parser.ShowWarnings = !IsCpdz();
            CopyLocalImages(fileName);
            FileSave(fileName);
            AddRecentFile(fileName);
            return true;

            bool IsCpdz() => string.Equals(Path.GetExtension(fileName), ".cpdz", StringComparison.OrdinalIgnoreCase);
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
                    var sourceParent = Directory.GetDirectoryRoot(sourcePath);
                    var targetParent = Directory.GetDirectoryRoot(targetPath);
                    if (!string.Equals(sourceParent, sourcePath, StringComparison.OrdinalIgnoreCase))
                        sourceParent = Directory.GetParent(sourcePath).FullName;
                    if (!string.Equals(targetParent, targetPath, StringComparison.OrdinalIgnoreCase))
                        targetParent = Directory.GetParent(targetPath).FullName;
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
                                        ShowErrorMessage(e.Message);
                                        break;
                                    }
                                }
                            }
                        }
                        regexString = @"src\s*=\s*""\s*\./";
                        if (string.Equals(sourceParent, sourcePath, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(targetParent, targetPath, StringComparison.OrdinalIgnoreCase))
                            return;

                        sourceParent = sourcePath;
                        targetParent = targetPath;
                    }
                }
            }
        }

        private async void FileSave(string fileName)
        {
            if (IsWebForm)
                SetAutoIndent();

            _macroParser.Parse(InputText, out var outputText, null, 0, false);
            var hasInputFields = MacroParser.HasInputFields(outputText);
            if (hasInputFields && IsWebForm)
            {
                if (IsCalculated)
                {
                    CalculateAsync(true);
                    IsCalculated = false;
                    _isSaving = true;
                    return;
                }
                if (!await GetAndSetInputFieldsAsync())
                    return;
            }
            var isZip = string.Equals(Path.GetExtension(fileName), ".cpdz", StringComparison.OrdinalIgnoreCase);
            if (isZip)
            {
                if (hasInputFields)
                    _macroParser.Parse(InputText, out outputText, null, 0, false);

                WriteFile(fileName, outputText, true);
                FileOpen(fileName);
            }
            else
            {
                WriteFile(fileName, GetInputText());
                CurrentFileName = fileName;
            }
            SaveButton.Tag = null;
            IsSaved = true;
        }

        private void Command_Help(object sender, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(_readmeFileName))
                Execute(_readmeFileName);
            else
                ShowHelp();
        }

        private void Command_Close(object sender, ExecutedRoutedEventArgs e) => Application.Current.Shutdown();

        private void Command_Copy(object sender, ExecutedRoutedEventArgs e)
        {
            if (_isWebView2Focused)
                WebViewer.ExecuteScriptAsync("document.execCommand('copy');");
            else
                RichTextBox.Copy();
        }

        private void Command_Paste(object sender, ExecutedRoutedEventArgs e)
        {
            if (_isWebView2Focused)
                WebViewer.CoreWebView2.ExecuteScriptAsync($"var input = document.activeElement; input.setRangeText('{Clipboard.GetText()}', input.selectionStart, input.selectionEnd, 'end');");
            else if(InputFrame.Visibility == Visibility.Visible)
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

        private async void Command_Print(object sender, ExecutedRoutedEventArgs e)
        {
            if (!_isParsing)
                _wv2Warper.PrintPreviewAsync();
        }

        private void Command_Find(object sender, ExecutedRoutedEventArgs e) =>
            CommandFindReplace(FindReplace.Modes.Find);

        private void Command_Replace(object sender, ExecutedRoutedEventArgs e) =>
            CommandFindReplace(FindReplace.Modes.Replace);

        private async void CommandFindReplace(FindReplace.Modes mode)
        {
            if (_isWebView2Focused)
                _findReplace.Mode = FindReplace.Modes.Find;
            else
                _findReplace.Mode = mode;

            string s = _isWebView2Focused ?
                await _wv2Warper.GetSelectedTextAsync() :
                RichTextBox.Selection.Text;

            if (!(string.IsNullOrEmpty(s) || s.Contains(Environment.NewLine)))
                _findReplace.SearchString = s;

            if (_findReplaceWindow is null || !_findReplaceWindow.IsVisible)
                _findReplaceWindow = new()
                {
                    Owner = this,
                    FindReplace = _findReplace
                };
            else
                _findReplaceWindow.Hide();

            bool isSelection = s is not null && s.Length > 5;
            _findReplaceWindow.SelectionCheckbox.IsEnabled = isSelection;
            _isTextChangedEnabled = false;
            _findReplaceWindow.Show();
        }

        private void Command_FindNext(object sender, ExecutedRoutedEventArgs e) =>
            _findReplace.Find();

        private void FileOpen(string fileName)
        {
            if (_isParsing)
                _parser.Cancel();

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            CurrentFileName = fileName;

            var hasForm = GetInputTextFromFile();
            _parser.ShowWarnings = ext != ".cpdz";
            if (ext == ".cpdz")
            {
                if (IsWebForm)
                    CalculateAsync(true);
                else
                    RunWebForm();
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
                        RunWebForm();
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
                        DispatchLineNumbers();
                        ForceHighlight();
                    }
                    SaveButton.Tag = null;
                    if (IsAutoRun)
                    {
                        IsCalculated = true;
                        CalculateAsync();
                    }
                    else
                    {
                        IsCalculated = false;
                        ShowHelp();
                    }
                }
            }
            _mustPromptUnlock = IsWebForm;
            if (ext != ".tmp")
            {
                IsSaved = true;
                AddRecentFile(CurrentFileName);
            }
        }

        private MessageBoxResult PromptSave()
        {
            var result = MessageBoxResult.No;
            if (!IsSaved)
                result = MessageBox.Show(MainWindowResources.SavePrompt, "Calcpad", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                if (string.IsNullOrWhiteSpace(CurrentFileName))
                {
                    var success = FileSaveAs();
                    if (!success)
                        return MessageBoxResult.Cancel;
                }
                else
                    FileSave(CurrentFileName);
            }
            return result;
        }

        private void GetMathSettings()
        {
            var mathSettings = _parser.Settings.Math;   
            if (double.TryParse(DecimalsTextBox.Text, out var d))
            {
                var i = (int)Math.Floor(d);
                mathSettings.Decimals = i;
                DecimalsTextBox.Text = mathSettings.Decimals.ToString();
                DecimalsTextBox.Foreground = Brushes.Black;
            }
            else
                DecimalsTextBox.Foreground = Brushes.Red;

            if (double.TryParse(MaxOutputCountTextBox.Text, out var m))
            {
                var i = (int)Math.Floor(m);
                mathSettings.MaxOutputCount = i;
                MaxOutputCountTextBox.Text = mathSettings.MaxOutputCount.ToString();
                MaxOutputCountTextBox.Foreground = Brushes.Black;
            }
            else
                MaxOutputCountTextBox.Foreground = Brushes.Red;

            mathSettings.Substitute = SubstituteCheckBox.IsChecked ?? false;
            mathSettings.ZeroSmallMatrixElements = ZeroSmallMatrixElementsCheckBox.IsChecked ?? false;
        }

        private void GetPlotSettings()
        {
            var plotSettings = _parser.Settings.Plot;
            plotSettings.ColorScale = (PlotSettings.ColorScales)ColorScaleComboBox.SelectedIndex;
            plotSettings.Shadows = ShadowsCheckBox.IsChecked ?? false;
            plotSettings.SmoothScale = SmoothCheckBox.IsChecked ?? false;
            plotSettings.LightDirection = (PlotSettings.LightDirections)LightDirectionComboBox.SelectedIndex;
            if (EmbedCheckBox.IsChecked ?? false)
            {
                plotSettings.ImagePath = string.Empty;
                plotSettings.ImageUri = string.Empty;
            }
            else
            {
                string imagePath;
                if (string.IsNullOrEmpty(_cfn))
                    imagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                else
                    imagePath = Path.GetDirectoryName(_cfn);

                imagePath += "\\Calcpad Plots\\" + _tempDir;
                if (Directory.Exists(imagePath))
                    ClearTempFolder(imagePath);

                plotSettings.ImagePath = imagePath;
                plotSettings.ImageUri = "file:///" + imagePath.Replace('\\', '/');
            }
        }

        private static void ClearTempFolder(string path)
        {
            try
            {
                var dir = new DirectoryInfo(path);
                foreach (var f in dir.GetFiles())
                    f.Delete();
            }
            catch (Exception e)
            {
                ShowErrorMessage(e.Message);
            }
        }

        private async void CalculateAsync(bool toWebForm = false)
        {
            if (_isParsing)
                return;

            GetMathSettings();
            GetPlotSettings();
            if (IsWebForm && !toWebForm && !await GetAndSetInputFieldsAsync())
                return;

            var inputText = SetImageLocalPath(InputText);
            string outputText;
            if (_highlighter.Defined.HasMacros)
            {
                var hasErrors = _macroParser.Parse(inputText, out outputText, null, 0, true);
                _htmlUnwarpedCode = hasErrors || DisplayUnwarpedCode ?
                    CodeToHtml(outputText) :
                    string.Empty;
            }
            else
            {
                outputText = inputText;
                _htmlUnwarpedCode = string.Empty;
            }

            string htmlResult;
            if (!string.IsNullOrEmpty(_htmlUnwarpedCode) && !(IsWebForm || toWebForm))
            {
                WebViewer.Tag = true;
                htmlResult = _htmlUnwarpedCode;
                if (toWebForm)
                    IsWebForm = false;
                OutputFrame.Header = MainWindowResources.Unwarped_code;
                CodeCheckBox.IsChecked = true;
            }
            else
            {
                _parser.Debug = !IsWebForm;
                WebViewer.Tag = false;
                if (toWebForm)
                    _parser.Parse(outputText, false);
                else
                {
                    _isParsing = true;
                    WebFormButton.IsEnabled = false;
                    MenuWebForm.IsEnabled = false;
                    FreezeOutputButtons(true);
                    try
                    {
                        var delayScript = $"setTimeout(function(){{window.location.replace(\"{_htmlParsingUrl}\");}},1000);";
                        await WebViewer.ExecuteScriptAsync(delayScript);
                    }
                    catch
                    {
                        _wv2Warper.Navigate(_htmlParsingPath);
                    }
                    void parse() => _parser.Parse(outputText);
                    await Task.Run(parse);
                    if (!IsWebForm)
                    {
                        MenuWebForm.IsEnabled = true;
                        WebFormButton.IsEnabled = true;
                    }
                    FreezeOutputButtons(false);
                    IsCalculated = !_parser.IsPaused;
                }
                htmlResult = HtmlApplyWorksheet(FixHref(_parser.HtmlResult));
                SetOutputFrameHeader(IsWebForm);
            }
            _autoRun = false;
            try
            {
                if (!string.IsNullOrEmpty(htmlResult))
                    await _wv2Warper.NavigateToStringAsync(htmlResult);
            }
            catch (Exception e)
            {
                ShowErrorMessage(e.Message);
            }
            if (IsWebForm)
                OutputFrame.Header = toWebForm ? MainWindowResources.Input : MainWindowResources.Output;
            if (_highlighter.Defined.HasMacros && string.IsNullOrEmpty(_htmlUnwarpedCode))
                _htmlUnwarpedCode = CodeToHtml(outputText);
        }

        private void FreezeOutputButtons(bool freeze)
        {
            var isEnabled = !freeze;
            MenuOutput.IsEnabled = isEnabled;
            CalcButton.IsEnabled = isEnabled;
            PdfButton.IsEnabled = isEnabled;
            WordButton.IsEnabled = isEnabled;
            CopyOutputButton.IsEnabled = isEnabled;
            SaveOutputButton.IsEnabled = isEnabled;
            PrintButton.IsEnabled = isEnabled;
            if (freeze)
                Cursor = Cursors.Wait;
            else
                Cursor = Cursors.Arrow;
        }

        private static string FixHref(in string text)
        {
            var s = HtmlAnchorHrefRegex.Replace(text, @"#0"" data-text=""");
            s = HtmlAnchorTargetRegex.Replace(s, "");
            return s;
        }

        private string SetImageLocalPath(string s)
        {
            if (string.IsNullOrWhiteSpace(CurrentFileName))
                return s;

            var path = Path.GetDirectoryName(CurrentFileName);
            var s1 = s;
            var parent = Directory.GetDirectoryRoot(path);
            if (!string.Equals(parent, path, StringComparison.OrdinalIgnoreCase))
            {
                parent = Directory.GetParent(path).FullName;
                parent = "file:///" + parent.Replace('\\', '/');
                s1 = HtmlImgPrevRegex.Replace(s, @"src=""" + parent);
            }
            path = "file:///" + path.Replace('\\', '/');
            var s2 = HtmlImgCurRegex.Replace(s1, @"src=""" + path);
            return s2;
        }

        private string CodeToHtml(string code)
        {
            var ErrorString = AppMessages.ErrorString;
            var highlighter = new HighLighter();
            var errors = new Queue<int>();
            _stringBuilder.Clear();
            _stringBuilder.Append(_htmlSource);
            var lines = code.EnumerateLines();
            _stringBuilder.AppendLine("<div class=\"code\">");
            highlighter.Defined.Get(lines, IsComplex);
            var indent = 0.0;
            var lineNumber = 0;
            foreach (var line in lines)
            {
                ++lineNumber;
                var i = line.IndexOf('\v');
                var lineText = i < 0 ? line : line[..i];
                var sourceLine = i < 0 ? lineNumber.ToString() : line[(i + 1)..];
                _stringBuilder.Append($"<p class=\"line-text\" id=\"line-{lineNumber}\"><a class=\"line-num\" href=\"#0\" data-text=\"{sourceLine}\" title=\"Source line {sourceLine}\">{lineNumber}</a>");
                if (line.StartsWith(ErrorString))
                {
                    errors.Enqueue(lineNumber);
                    _stringBuilder.Append($"<span class=\"error\">{lineText}</span>");
                }
                else
                {
                    var p = new Paragraph();
                    highlighter.Parse(p, IsComplex, lineNumber, lineText.ToString());
                    if (!UpdateIndent(p, ref indent))
                        p.TextIndent = indent;

                    var steps = 4 * p.TextIndent / AutoIndentStep;
                    for (int j = 0; j < steps; ++j)
                        _stringBuilder.Append("&nbsp;");

                    foreach (var inline in p.Inlines)
                    {
                        if (inline is not Run r)
                            continue;

                        var cls = HighLighter.GetCSSClassFromColor(r.Foreground);
                        if (r.Background is SolidColorBrush brush && 
                            brush.Color.R > brush.Color.G)
                                cls = "error";

                        var htmlEncodedText = HttpUtility.HtmlEncode(r.Text);
                        if (string.IsNullOrEmpty(cls))
                            _stringBuilder.Append(htmlEncodedText);
                        else
                            _stringBuilder.Append($"<span class=\"{cls}\">{htmlEncodedText}</span>");
                    }
                }
                _stringBuilder.Append("</p>");
            }
            _stringBuilder.Append("</div>");
            if (errors.Count != 0 && lineNumber > 30)
            {
                _stringBuilder.AppendLine(string.Format(MainWindowResources.Found_Errors_In_Modules_And_Macros, errors.Count));
                var count = 0;
                while (errors.Count != 0 && ++count < 20)
                {
                    var line = errors.Dequeue();
                    _stringBuilder.Append($" <span class=\"roundBox\" data-line=\"{line}\">{line}</span>");
                }
                if (errors.Count > 0)
                    _stringBuilder.Append(" ...");

                _stringBuilder.Append("</div>");
                _stringBuilder.AppendLine("<style>body {padding-top:1.1em;}</style>");
            }
            _stringBuilder.Append("</body></html>");
            return _stringBuilder.ToString();
        }

        private static string[] GetLocalImages(string s)
        {
            MatchCollection matches = HtmlImgAnyRegex.Matches(s);
            var n = matches.Count;
            if (n == 0)
                return null;

            string[] images = new string[n];
            for (int i = 0; i < n; ++i)
                images[i] = matches[i].Value;

            return images;
        }

        private string HtmlApplyWorksheet(string s)
        {
            _stringBuilder.Clear();
            var ssf = Math.Round(0.9 * Math.Sqrt(_screenScaleFactor), 2).ToString(CultureInfo.InvariantCulture);
            _stringBuilder.Append(_htmlWorksheet.Replace("var(--screen-scale-factor)", ssf));
            _stringBuilder.Append(s);
            if (_scrollY > 0)
            {
                _stringBuilder.Append($"<script>window.onload = function() {{ window.scrollTo(0, {_scrollY}); }};</script>");
                _scrollY = 0;
            }
            _stringBuilder.Append(" </body></html>");
            return _stringBuilder.ToString();
        }

        private void ShowHelp()
        {
            if (!_isParsing)
                _wv2Warper.Navigate(_htmlHelpPath);
        }

        private static string GetHelp(string helpURL)
        {
            var fileName = $"{AppInfo.DocPath}\\help.{_currentCultureName}.html";
            if (!File.Exists(fileName))
                fileName = $"{AppInfo.DocPath}\\help.html";

            return fileName;
        }

        private static string ReadTextFromFile(string fileName)
        {
            try
            {
                if (string.Equals(Path.GetExtension(fileName), ".cpdz", StringComparison.OrdinalIgnoreCase))
                {
                    if (Zip.IsComposite(fileName))
                        return Zip.DecompressWithImages(fileName);

                    var f = new FileInfo(fileName)
                    {
                        IsReadOnly = false
                    };
                    using var fs = f.OpenRead();
                    return Zip.DecompressToString(fs);
                }
                else
                {
                    using var sr = new StreamReader(fileName);
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
                return string.Empty;
            }
        }

        private static SpanLineEnumerator ReadLines(string fileName)
        {
            var lines = new SpanLineEnumerator();
            try
            {
                if (string.Equals(Path.GetExtension(fileName), ".cpdz", StringComparison.OrdinalIgnoreCase))
                {
                    if (Zip.IsComposite(fileName))
                        lines = Zip.DecompressWithImages(fileName).EnumerateLines();
                    else
                    {
                        var f = new FileInfo(fileName)
                        {
                            IsReadOnly = false
                        };
                        using var fs = f.OpenRead();
                        lines = Zip.Decompress(fs);
                    }
                }
                else
                {
                    return File.ReadAllText(fileName).EnumerateLines();
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
            return lines;
        }

        private static void WriteFile(string fileName, string s, bool zip = false)
        {
            try
            {
                if (zip)
                {
                    var images = GetLocalImages(s);
                    Zip.CompressWithImages(s, images, fileName);
                }
                else
                {
                    using var sw = new StreamWriter(fileName);
                    sw.Write(s);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private bool GetInputTextFromFile()
        {
            var lines = ReadLines(CurrentFileName);
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            _document.Blocks.Clear();
            SetCodeCheckBoxVisibility();
            _highlighter.Defined.Get(lines, IsComplex);
            var hasForm = false;
            foreach (var line in lines)
            {
                ReadOnlySpan<char> s;
                if (line.Contains('\v'))
                {
                    hasForm = true;
                    var n = line.IndexOf('\v');
                    if (n == 0)
                    {
                        SetInputFieldsFromFile(line[1..].EnumerateSplits('\t'));
                        break;
                    }
                    else
                    {
                        SetInputFieldsFromFile(line[(n + 1)..].EnumerateSplits('\t'));
                        s = line[..n];
                    }
                }
                else
                {
                    s = ReplaceCStyleOperators(line.TrimStart('\t'));
                    if (!hasForm)
                        hasForm = MacroParser.HasInputFields(s);
                }
                _document.Blocks.Add(new Paragraph(new Run(s.ToString())));
            }
            if (_document.Blocks.Count == 0)
                _document.Blocks.Add(new Paragraph(new Run()));

            var b = _document.Blocks.LastBlock;
            if (b.ContentStart.GetOffsetToPosition(b.ContentEnd) == 0)
                _document.Blocks.Remove(b);

            _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            _currentLineNumber = GetLineNumber(_currentParagraph);
            _undoMan.Reset();
            Record();
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
            _forceHighlight = true;
            return hasForm;
        }

        private string ReplaceCStyleOperators(ReadOnlySpan<char> s)
        {
            if (s.IsEmpty)
                return string.Empty;

            _stringBuilder.Clear();
            var commentEnumerator = s.EnumerateComments();
            foreach (var item in commentEnumerator)
            {
                if (!item.IsEmpty && item[0] != '"' && item[0] != '\'')
                {
                    foreach (var c in item)
                    {
                        if (c == '=')
                        {
                            var n = _stringBuilder.Length - 1;
                            if (n < 0)
                            {
                                _stringBuilder.Append(c);
                                break;
                            }
                            switch (_stringBuilder[n])
                            {
                                case '=':
                                    _stringBuilder[n] = '≡';
                                    break;
                                case '!':
                                    _stringBuilder[n] = '≠';
                                    break;
                                case '>':
                                    _stringBuilder[n] = '≥';
                                    break;
                                case '<':
                                    _stringBuilder[n] = '≤';
                                    break;
                                default:
                                    _stringBuilder.Append(c);
                                    break;
                            }
                        }
                        else if (c == '%')
                        {
                            var n = _stringBuilder.Length - 1;
                            if (n >= 0 && _stringBuilder[n] == '%')
                                _stringBuilder[n] = '⦼';
                            else
                                _stringBuilder.Append(c);
                        }
                        else if (c == '&')
                        {
                            var n = _stringBuilder.Length - 1;
                            if (n >= 0 && _stringBuilder[n] == '&')
                                _stringBuilder[n] = '∧';
                            else
                                _stringBuilder.Append(c);
                        }
                        else if (c == '|')
                        {
                            var n = _stringBuilder.Length - 1;
                            if (n >= 0 && _stringBuilder[n] == '|')
                                _stringBuilder[n] = '∨';
                            else
                                _stringBuilder.Append(c);
                        }
                        else
                            _stringBuilder.Append(c);
                    }
                }
                else
                    _stringBuilder.Append(item);
            }
            return _stringBuilder.ToString();
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ResetText();
            DispatchLineNumbers();
            if (IsAutoRun)
                AutoRun();
        }

        private void ResetText()
        {
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            _document.Blocks.Clear();
            _currentParagraph = new Paragraph();
            _currentLineNumber = 1;
            _document.Blocks.Add(_currentParagraph);
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        const string Tabs = "\t\t\t\t\t\t\t\t\t\t\t\t";
        private string GetInputText()
        {
            _stringBuilder.Clear();
            var b = _document.Blocks.FirstBlock;
            while (b is not null)
            {
                var n = (int)((b as Paragraph).TextIndent / AutoIndentStep);
                if (n > 12)
                    n = 12;
                var line = new TextRange(b.ContentStart, b.ContentEnd).Text;
                if (n == 0)
                    _stringBuilder.AppendLine(line);
                else
                    _stringBuilder.AppendLine(Tabs[..n] + line);
                b = b.NextBlock;
            }
            _stringBuilder.RemoveLastLineIfEmpty();
            return _stringBuilder.ToString();
        }

        private async void HtmlFileSave()
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
                string html = await _wv2Warper.GetContentsAsync();
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

        private void CopyOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isParsing)
                _wv2Warper.ClipboardCopyAsync();
        }

        private async void WordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isParsing) return;
            var isOutput = IsCalculated || IsWebForm || _parser.IsPaused;
            var isDoc = (Professional.IsChecked ?? false) && isOutput;
            var fileExt = isDoc ? "docx" : "html";
            string fileName;
            if (isOutput)
            {
                if (string.IsNullOrEmpty(CurrentFileName))
                    fileName = Path.GetTempPath() + "Calcpad\\Output." + fileExt;
                else
                    fileName = Path.ChangeExtension(CurrentFileName, fileExt);
            }
            else
            {
                fileName = $"{AppInfo.DocPath}\\help.{_currentCultureName}.docx";
                if (!File.Exists(fileName))
                    fileName = $"{AppInfo.DocPath}\\help.docx";
            }
            try
            {
                if (isOutput)
                {
                    if (isDoc)
                    {
                        fileName = PromtSaveDoc(fileName);
                        var logString = await _wv2Warper.ExportOpenXmlAsync(fileName, _parser.OpenXmlExpressions);
                        if (logString.Length > 0)
                        {
                            string message = MainWindowResources.Error_Exporting_Docx_File;
                            if (MessageBox.Show(message, "Calcpad", MessageBoxButton.YesNo, MessageBoxImage.Error) == MessageBoxResult.Yes)
                            {
                                var logFile = fileName + "_validation.log";
                                WriteFile(logFile, logString);
                                RunExternalApp("NOTEPAD", logFile);
                            }
                        }
                    }
                    else
                    {
                        var html = await _wv2Warper.GetContentsAsync();
                        WriteFile(fileName, html);
                    }
                }
                if (RunExternalApp("WINWORD", fileName) is null)
                    RunExternalApp("SOFFICE", fileName);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private static Process RunExternalApp(string appName, string fileName)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = appName
            };
            if (fileName is not null)
                startInfo.Arguments =
                    fileName.Contains(' ') ?
                    '\"' + fileName + '\"' :
                    fileName;

            startInfo.UseShellExecute = true;
            if (appName != "NOTEPAD")
                startInfo.WindowStyle = ProcessWindowStyle.Maximized;

            try
            {
                return Process.Start(startInfo);
            }
            catch
            {
                return null;
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
            var offset = _undoMan.RestoreOffset;
            var currentLine = _undoMan.RestoreLine;
            var lines = _undoMan.RestoreText.AsSpan().EnumerateLines();
            _highlighter.Defined.Get(lines, IsComplex);
            SetCodeCheckBoxVisibility();
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            var blocks = _document.Blocks;
            int j = 1, n = blocks.Count;
            var indent = 0d;
            var b = blocks.FirstBlock;
            _highlighter.All = true;
            foreach (var line in lines)
            {
                if (j < n)
                {
                    var s = new TextRange(b.ContentStart, b.ContentEnd).Text;
                    if (line.SequenceEqual(s))
                    {
                        if (_currentParagraph == b)
                            _highlighter.Parse(_currentParagraph, IsComplex, j);

                        var bp = b as Paragraph;
                        if (!UpdateIndent(bp, ref indent))
                            bp.TextIndent = indent;

                        b = b.NextBlock;
                        ++j;
                        continue;
                    }
                }
                var p = b is not null ? b as Paragraph : new Paragraph();
                _highlighter.Parse(p, IsComplex, j, line.ToString());
                if (!UpdateIndent(p, ref indent))
                    p.TextIndent = indent;

                if (b is null)
                    blocks.Add(p);
                else
                    b = b.NextBlock;
                ++j;
            }
            _highlighter.All = false;
            blocks.Remove(blocks.LastBlock);
            while (j < n)
            {
                blocks.Remove(blocks.LastBlock);
                --n;
            }
            n = blocks.Count;
            if (currentLine < 1)
                currentLine = 1;
            else if (currentLine > n)
                currentLine = n;
            _currentParagraph = blocks.ElementAt(currentLine - 1) as Paragraph;
            _currentLineNumber = currentLine;
            var pointer = HighLighter.FindPositionAtOffset(_currentParagraph, offset);
            RichTextBox.Selection.Select(pointer, pointer);
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
            DispatchLineNumbers();
            if (IsAutoRun)
                AutoRun();
        }

        private void WebFormButton_Click(object sender, RoutedEventArgs e) => RunWebForm();

        private void Command_WebForm(object sender, ExecutedRoutedEventArgs e)
        {
            if (WebFormButton.IsEnabled)
                RunWebForm();
        }

        private void RunWebForm()
        {
            if (IsWebForm && WebFormButton.Visibility != Visibility.Visible)
                return;

            if (_mustPromptUnlock && IsWebForm)
            {
                string message = MainWindowResources.Are_you_sure_you_want_to_unlock_the_source_code_for_editing;
                if (MessageBox.Show(message, "Calcpad", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;

                _mustPromptUnlock = false;
            }
            IsWebForm = !IsWebForm;
            IsCalculated = false;
            if (IsWebForm)
                CalculateAsync(true);
            else
            {
                //GetAndSetInputFields();
                RichTextBox.Focus();
                if (IsAutoRun)
                {
                    CalculateAsync();
                    IsCalculated = true;
                }
                else
                    ShowHelp();
            }
        }

        private void SetWebForm(bool value)
        {
            SetButton(WebFormButton, value);
            SetUILock(value);
            if (value)
            {
                InputFrame.Visibility = Visibility.Hidden;
                FramesGrid.ColumnDefinitions[0].Width = new GridLength(0);
                FramesGrid.ColumnDefinitions[1].Width = new GridLength(0);
                WebFormButton.ToolTip = MainWindowResources.Open_source_code_for_editing__F4;
                MenuWebForm.Icon = "  ✓";
                AutoRunCheckBox.Visibility = Visibility.Hidden;
                _findReplaceWindow?.Close();
                IsWebView2Focused = true;
            }
            else
            {
                var cursor = WebViewer.Cursor;
                WebViewer.Cursor = Cursors.Wait;
                DispatchLineNumbers();
                ForceHighlight();
                InputFrame.Visibility = Visibility.Visible;
                FramesGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                FramesGrid.ColumnDefinitions[1].Width = new GridLength(5);
                FramesGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                WebFormButton.ToolTip = MainWindowResources.Compile_to_input_form_F4;
                MenuWebForm.Icon = null;
                WebViewer.Cursor = cursor;
                AutoRunCheckBox.Visibility = Visibility.Visible;
                SetOutputFrameHeader(false);
                IsWebView2Focused = false;
            }
        }

        private async Task<bool> GetAndSetInputFieldsAsync()
        {
            if (InputText.Contains("%u", StringComparison.Ordinal))
            {
                try
                {
                    _parser.Settings.Units = await _wv2Warper.GetUnitsAsync();
                }
                catch
                {
                    ShowErrorMessage(MainWindowResources.Error_getting_units);
                }
            }
            else
                _parser.Settings.Units = "m";

            if (!SetInputFields(await _wv2Warper.GetInputFieldsAsync()))
            {
                {
                    ShowErrorMessage(MainWindowResources.Error_Invalid_number_Please_correct_and_then_try_again);
                    IsCalculated = false;
                    WebViewer.Focus();
                    return false;
                }
            }
            return true;
        }

        private void SetUnits()
        {
            if (InputText.Contains("%u", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _wv2Warper.SetUnitsAsync(_parser.Settings.Units);
                }
                catch
                {
                    ShowErrorMessage(MainWindowResources.Error_setting_units);
                }
            }
        }

        private void SubstituteCheckBox_Click(object sender, RoutedEventArgs e) => ClearOutput();
        private void DecimalsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearOutput(false);
            if (IsInitialized && int.TryParse(DecimalsTextBox.Text, out int n))
                DecimalScrollBar.Value = 15 - n;
        }

        private async void ClearOutput(bool focus = true)
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
                        _scrollY = await _wv2Warper.GetScrollYAsync();
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
                InsertImage(dlg.FileName);
        }

        private void InsertImage(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var size = GetImageSize(filePath);
            var fileDir = Path.GetDirectoryName(filePath);
            string src;
            if (!string.IsNullOrEmpty(CurrentFileName) &&
                string.Equals(Path.GetDirectoryName(CurrentFileName), fileDir, StringComparison.OrdinalIgnoreCase))
                src = "./" + fileName;
            else
                src = filePath.Replace('\\', '/');
            var p = new Paragraph();
            p.Inlines.Add(new Run($"'<img style=\"height:{size.Height}pt; width:{size.Width}pt;\" src=\"{src}\" alt=\"{fileName}\">"));
            _highlighter.Parse(p, IsComplex, GetLineNumber(p));
            _document.Blocks.InsertBefore(_currentParagraph ?? _document.Blocks.FirstBlock, p);
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
                InputGrid.RowDefinitions[1].Height = new GridLength(_inputHeight);
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
            _insertManager.InsertText(tb.Text);
        }

        private void EquationRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                var pro = ReferenceEquals(sender, Professional);
                _parser.Settings.Math.FormatEquations = pro;
                Professional.IsChecked = pro;
                Inline.IsChecked = !pro;
            }
            ClearOutput();
        }

        private void AngleRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                var deg = ReferenceEquals(sender, Deg) ? 0 :
                          ReferenceEquals(sender, Rad) ? 1 :
                          2;
                _parser.Settings.Math.Degrees = deg;
                Deg.IsChecked = deg == 0;
                Rad.IsChecked = deg == 1;
                Gra.IsChecked = deg == 2;
            }
            ClearOutput();
        }

        private void ModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                var complex = ReferenceEquals(sender, Complex);
                _parser.Settings.Math.IsComplex = complex;
                Complex.IsChecked = complex;
                Real.IsChecked = !complex;
                _highlighter.Defined.Get(InputText.AsSpan().EnumerateLines(), IsComplex);
                if (!IsWebForm)
                    Task.Run(() => Dispatcher.InvokeAsync(HighLightAll, DispatcherPriority.Send));
            }
            ClearOutput();
        }

        private void SaveOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isParsing)
                HtmlFileSave();
        }

        private void TryOpenOnStartup()
        {
            var args = Environment.GetCommandLineArgs();
            var n = args.Length;
            if (n > 1)
            {
                var s = string.Join(" ", args, 1, n - 1);
                if (File.Exists(s))
                {
                    var ex = Path.GetExtension(s).ToLowerInvariant();
                    if (ex == ".cpd" || ex == ".cpdz")
                    {
                        _parser.ShowWarnings = ex != ".cpdz";
                        CurrentFileName = s;
                        var hasForm = GetInputTextFromFile() || ex == ".cpdz";
                        SetButton(WebFormButton, false);
                        if (hasForm)
                        {
                            RunWebForm();
                            _mustPromptUnlock = true;
                            if (ex == ".cpdz")
                                WebFormButton.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            ForceHighlight();
                            IsCalculated = true;
                            _wv2Warper.NavigateToBlank();
                            Dispatcher.InvokeAsync(() => CalculateAsync(), DispatcherPriority.ApplicationIdle);
                        }
                        AddRecentFile(CurrentFileName);
                        return;
                    }
                }
            }
            ShowHelp();
            DispatchLineNumbers();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var r = PromptSave();
            if (r == MessageBoxResult.Cancel)
                e.Cancel = true;

            WriteSettings();
        }

        private async Task ScrollOutput()
        {   
            var offset = RichTextBox.CaretPosition.GetCharacterRect(LogicalDirection.Forward).Top +
                RichTextBox.Margin.Top - WebViewer.Margin.Top;
            await ScrollOutputToLine(
                _highlighter.Defined.HasMacros
                    ? _macroParser.GetUnwarpedLineNumber(_currentLineNumber)
                    : _currentLineNumber, offset);

            _scrollOutput = false;
        }

        private async Task ScrollOutputToLine(int lineNumber, double offset)
        {
            var tempScrollY = await _wv2Warper.GetScrollYAsync();
            await _wv2Warper.ScrollAsync(lineNumber, offset);
            if (tempScrollY == await _wv2Warper.GetScrollYAsync())
                await _wv2Warper.SetScrollYAsync(_scrollY);
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
                    var pp = p.PreviousBlock as Paragraph;
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
                            _insertManager.InsertText(c.ToString());
                        else
                            sel.Select(cp, cp);
                    }
                }
            }
            else if (e.Key == Key.Back && !_autoCompleteManager.IsInComment())
                Task.Run(() => Dispatcher.InvokeAsync(_autoCompleteManager.RestoreAutoComplete));
        }

        private int GetLineNumber(Block block)
        {
            var blocks = _document.Blocks;
            var i = blocks.Count;
            if (_currentLineNumber > i / 2)
            {
                var b = blocks.LastBlock;
                while (b is not null)
                {
                    if (ReferenceEquals(b, block))
                        return i;
                    --i;
                    b = b.PreviousBlock;
                }
            }
            else
            {
                i = 1;
                var b = blocks.FirstBlock;
                while (b is not null)
                {
                    if (ReferenceEquals(b, block))
                        return i;
                    ++i;
                    b = b.NextBlock;
                }
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
                        if (IsCalculated && len > 2 && !_highlighter.Defined.HasMacros)
                            _wv2Warper.SetContentAsync(_currentLineNumber, _svgTyping);
                    }
                    _autoRun = true;
                }

                if (_isPasting)
                {
                    _highlighter.Defined.Get(InputTextLines, IsComplex);
                    SetCodeCheckBoxVisibility();
                    await Dispatcher.InvokeAsync(HighLightPastedText, DispatcherPriority.Background);
                    SetAutoIndent();
                    var p = RichTextBox.Selection.End.Paragraph;
                    if (p is not null)
                        RichTextBox.CaretPosition = HighLighter.FindPositionAtOffset(p, _pasteOffset);
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
                    _highlighter.Defined.Get(InputTextLines, IsComplex);
                    SetCodeCheckBoxVisibility();
                    await Task.Run(DispatchAutoIndent);
                }
                await Task.Run(DispatchLineNumbers);
                _lastModifiedParagraph = _currentParagraph;
            }
        }

        private async void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var tps = RichTextBox.Selection.Start;
            var tpe = RichTextBox.Selection.End;
            if (tps.Paragraph is null && tpe.Paragraph is null)
                return;

            var p = tps.Paragraph;
            p ??= tpe.Paragraph;

            if (!ReferenceEquals(_currentParagraph, tps.Paragraph) &&
                !ReferenceEquals(_currentParagraph, tpe.Paragraph))
            {
                _isTextChangedEnabled = false;
                RichTextBox.BeginChange();
                _highlighter.Parse(_currentParagraph, IsComplex, _currentLineNumber);
                if (p is not null)
                {
                    _currentParagraph = p;
                    _currentLineNumber = GetLineNumber(_currentParagraph);
                    HighLighter.Clear(_currentParagraph);
                    _autoCompleteManager.FillAutoComplete(_highlighter.Defined, _currentLineNumber);
                }
                e.Handled = true;
                RichTextBox.EndChange();
                _isTextChangedEnabled = true;
                if (_autoRun)
                {
                    var offset = RichTextBox.CaretPosition.GetOffsetToPosition(_document.ContentEnd);
                    await AutoRun(offset <= 2);
                }
                DispatchHighLightFromCurrent();
            }
            if (tps.Paragraph is null)
                return;

            _currentOffset = new TextRange(tps, tps.Paragraph.ContentEnd).Text.Length;
            if (p is not null && tpe.GetOffsetToPosition(tps) == 0)
            {
                _isTextChangedEnabled = false;
                RichTextBox.BeginChange();
                var tr = new TextRange(p.ContentStart, p.ContentEnd);
                tr.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
                tr = new TextRange(p.ContentStart, tpe);
                var len = tr.Text.Length;
                HighLighter.HighlightBrackets(p, len);
                RichTextBox.EndChange();
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
                await _wv2Warper.CheckIsContextMenuAsync())
                return;

            if (_autoRun && IsCalculated)
                AutoRun();
        }

        private void RichTextBox_Paste(object sender, DataObjectPastingEventArgs e)
        {
            var formats = e.DataObject.GetFormats();
            var hasImage = formats.Any(x => x.Contains("Bitmap"));
            if (formats.Contains("UnicodeText") && !hasImage)
            {
                e.FormatToApply = "UnicodeText";
                _isPasting = true;
                GetPasteOffset();
            }
            else
            {
                e.CancelCommand();
                if (hasImage && Clipboard.ContainsImage())
                {
                    string name = null;
                    if (formats.Contains("FileName"))
                    {
                        string[] fn = (string[])e.DataObject.GetData("FileName");
                        name = fn[0];
                        name = Path.GetFileNameWithoutExtension(name) + ".png";
                    }
                    Dispatcher.InvokeAsync(() => PasteImage(name), DispatcherPriority.ApplicationIdle);
                }
            }
        }

        private void PasteImage(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Random rand = new();
                name = $"image_{rand.NextInt64()}";
                InputBox.Show("Calcpad", "Image name:", ref name);
                name += ".png";
            }
            string path;
            if (!string.IsNullOrEmpty(CurrentFileName))
                path = Path.GetDirectoryName(CurrentFileName) + "\\Images\\";
            else
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\Calcpad\\";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path += name;
            try
            {
                BitmapPaster.PasteImageFromClipboard(path);
                InsertImage(path);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        private void GetPasteOffset()
        {
            _pasteEnd = RichTextBox.Selection.End;
            var p = _pasteEnd.Paragraph;
            _pasteOffset = p is not null ? new TextRange(_pasteEnd, p.ContentEnd).Text.Length : 0;
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
            var sz = _document.FontSize - 1;
            var topMax = -sz;
            var tp = RichTextBox.GetPositionFromPoint(new Point(sz, sz), true);
            var b = (Block)tp.Paragraph;
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
            IsWebView2Focused = false;
            var modifiers = e.KeyboardDevice.Modifiers;
            var isCtrl = modifiers == ModifierKeys.Control;
            var isCtrlShift = modifiers == (ModifierKeys.Control | ModifierKeys.Shift);
            if (e.Key == Key.V && isCtrlShift)
            {
                PasteAsCommentMenu_Click(PasteAsCommentMenu, e);
                e.Handled = true;
            }
            if (e.Key == Key.Q && isCtrl)
            {
                CommentUncomment(true);
                e.Handled = true;
            }
            if (e.Key == Key.Q && isCtrlShift)
            {
                CommentUncomment(false);
                e.Handled = true;
            }
            else if ((e.Key == Key.D3 || e.Key == Key.NumPad3) && isCtrl)
            {
                Button_Click(H3Button, e);
                e.Handled = true;
            }
            else if ((e.Key == Key.D4 || e.Key == Key.NumPad4) && isCtrl)
            {
                Button_Click(H4Button, e);
                e.Handled = true;
            }
            else if ((e.Key == Key.D5 || e.Key == Key.NumPad5) && isCtrl)
            {
                Button_Click(H5Button, e);
                e.Handled = true;
            }
            else if ((e.Key == Key.D6 || e.Key == Key.NumPad6) && isCtrl)
            {
                Button_Click(H6Button, e);
                e.Handled = true;
            }
            else if (e.Key == Key.L && isCtrl)
            {
                Button_Click(ParagraphMenu, e);
                e.Handled = true;
            }
            else if (e.Key == Key.R && isCtrl)
            {
                Button_Click(LineBreakMenu, e);
                e.Handled = true;
            }
            else if (e.Key == Key.B && isCtrl)
            {
                Button_Click(BoldButton, e);
                e.Handled = true;
            }
            else if (e.Key == Key.I && isCtrl)
            {
                Button_Click(ItalicButton, e);
                e.Handled = true;
            }
            else if (e.Key == Key.U && isCtrl)
            {
                Button_Click(UnderlineButton, e);
                e.Handled = true;
            }
            else if (e.Key == Key.L && isCtrlShift)
            {
                Button_Click(BulletsMenu, e);
                e.Handled = true;
            }
            else if (e.Key == Key.N && isCtrlShift)
            {
                Button_Click(NumberingMenu, e);
                e.Handled = true;
            }
            else if (e.Key == Key.OemPlus)
            {
                if (isCtrl)
                {
                    Button_Click(SubscriptButton, e);
                    e.Handled = true;
                }
                else if (isCtrlShift)
                {
                    Button_Click(SuperscriptButton, e);
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (isCtrl)
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
                _autoCompleteManager.PreviewKeyDown(e);
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
                p = _document.Blocks.FirstBlock as Paragraph;
            else if (p.PreviousBlock is not null)
                p = p.PreviousBlock as Paragraph;

            if (p is null)
            {
                p = new Paragraph(new Run());
                _document.Blocks.Add(p);
            }
            var indent = 0.0;
            var i = 0;
            var pp = (p.PreviousBlock as Paragraph);
            if (pp is not null)
            {
                indent = pp.TextIndent;
                var s = new TextRange(pp.ContentStart, pp.ContentEnd).Text.Trim().ToLowerInvariant();
                if (s.Length > 3 && s[0] == '#')
                {
                    var span = s.AsSpan(1);
                    if (IsIndentStart(span) || span.StartsWith("else"))
                        indent += AutoIndentStep;
                }
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
                p = p.NextBlock as Paragraph;
            }
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private void SetAutoIndent()
        {
            var indent = 0.0;
            var p = _document.Blocks.FirstBlock as Paragraph;

            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            while (p is not null)
            {
                if (!UpdateIndent(p, ref indent))
                    p.TextIndent = indent;

                p = p.NextBlock as Paragraph;
            }
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private static bool UpdateIndent(Paragraph p, ref double indent)
        {
            var s = new TextRange(p.ContentStart, p.ContentEnd).Text.ToLowerInvariant().Trim();
            if (s.Length > 3 && s[0] == '#')
            {
                var span = s.AsSpan(1);
                if (!IsIndent(span))
                    return false;
                else if (IsIndentStart(span))
                {
                    p.TextIndent = indent;
                    indent += AutoIndentStep;
                }
                else if (IsIndentEnd(span))
                {
                    indent -= AutoIndentStep;
                    if (indent < 0)
                        indent = 0;
                    p.TextIndent = indent;
                }
                else
                    p.TextIndent = Math.Max(indent - AutoIndentStep, 0);

                return true;
            }
            return false;
        }

        private static bool IsIndent(ReadOnlySpan<char> s) =>
            s.StartsWith("if") ||
            s.StartsWith("el") ||
            s.StartsWith("en") ||
            s.StartsWith("re") ||
            s.StartsWith("fo") ||
            s.StartsWith("wh") ||
            s.StartsWith("lo") ||
            s.StartsWith("def") &&
            !s.Contains('=');

        private static bool IsIndentStart(ReadOnlySpan<char> s) =>
            s.StartsWith("if") ||
            s.StartsWith("repeat") ||
            s.StartsWith("for ") ||
            s.StartsWith("while") ||
            s.StartsWith("def") &&
            !s.Contains('=');

        private static bool IsIndentEnd(ReadOnlySpan<char> s) =>
            s.StartsWith("end") || s.StartsWith("loop");

        private void HighLightAll()
        {
            _isTextChangedEnabled = false;
            Cursor = Cursors.Wait;
            RichTextBox.BeginChange();
            _highlighter.Defined.Get(InputTextLines, IsComplex);
            SetCodeCheckBoxVisibility();
            var p = _document.Blocks.FirstBlock as Paragraph;
            var i = 1;
            _highlighter.All = true;
            while (p is not null)
            {
                if (_forceHighlight)
                    _highlighter.Parse(p, IsComplex, i, new TextRange(p.ContentStart, p.ContentEnd).Text.TrimStart('\t'));
                else
                    _highlighter.Parse(p, IsComplex, i);
                p = p.NextBlock as Paragraph;
                ++i;
            }
            _highlighter.All = false;
            _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            _currentLineNumber = GetLineNumber(_currentParagraph);
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            Cursor = Cursors.Arrow;
            _isTextChangedEnabled = true;
        }

        private DispatcherOperation _highLightFromCurrentDispatcherOperation;

        private async void DispatchHighLightFromCurrent()
        {
            _highLightFromCurrentDispatcherOperation?.Abort();
            var currentkeyDownCount = _countKeys;
            await Task.Delay(250).ContinueWith(delegate
            {
                if (currentkeyDownCount == _countKeys &&
                    _highLightFromCurrentDispatcherOperation?.Status != DispatcherOperationStatus.Executing)
                    _highLightFromCurrentDispatcherOperation =
                        Dispatcher.BeginInvoke(HighLightFromCurrent, DispatcherPriority.ApplicationIdle);
            });
        }

        private void HighLightFromCurrent()
        {
            if (_lastModifiedParagraph is null)
                return;

            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            var p = _lastModifiedParagraph.NextBlock as Paragraph;
            var lineNumber = GetLineNumber(p);
            var maxNumber = lineNumber + 35;
            while (p is not null)
            {
                if (!ReferenceEquals(p, _currentParagraph))
                    _highlighter.CheckHighlight(p, lineNumber);
                p = p.NextBlock as Paragraph;
                lineNumber++;
                if (lineNumber >= maxNumber)
                {
                    _lastModifiedParagraph = p;
                    RichTextBox.EndChange();
                    _isTextChangedEnabled = true;
                    return;
                }
            }
            _lastModifiedParagraph = null;
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private void HighLightPastedText()
        {
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            var p = _pasteEnd.Paragraph;
            _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            p ??= _document.Blocks.FirstBlock as Paragraph;

            var lineNumber = GetLineNumber(p);
            _highlighter.All = true;
            while (p != _currentParagraph && p != null)
            {
                _highlighter.Parse(p, IsComplex, lineNumber++);
                p = p.NextBlock as Paragraph;
            }
            _highlighter.All = false;
            _currentLineNumber = GetLineNumber(_currentParagraph);
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
            if (IsControlDown)
            {
                e.Handled = true;
                var d = RichTextBox.FontSize + Math.CopySign(2, e.Delta);
                if (d > 4 && d < 42)
                {
                    RichTextBox.FontSize = d;
                    DispatchLineNumbers();
                }
            }
        }

        private static bool IsControlDown => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        private static bool IsAltDown => (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;

        private void InvHypButton_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            b.Tag = !(bool)b.Tag;
            if ((bool)b.Tag)
                b.Foreground = Brushes.Teal;
            else
                b.Foreground = Brushes.Black;

            bool inv = (bool)InvButton.Tag, hyp = (bool)HypButton.Tag;
            string pref = string.Empty, post = string.Empty;
            if (inv)
                pref = "a";

            if (hyp)
                post = "h";

            double fs = inv && hyp ? 14d : 15d;
            FontFamily ff;
            if (inv || hyp)
                ff = new FontFamily("Arial Nova Cond");
            else
                ff = new FontFamily("Roboto");

            SetTrigButton(SinButton, pref + "sin" + post, fs, ff);
            SetTrigButton(CosButton, pref + "cos" + post, fs, ff);
            SetTrigButton(TanButton, pref + "tan" + post, fs, ff);
            SetTrigButton(CscButton, pref + "csc" + post, fs, ff);
            SetTrigButton(SecButton, pref + "sec" + post, fs, ff);
            SetTrigButton(CotButton, pref + "cot" + post, fs, ff);
            PowButton.Visibility = inv ? Visibility.Hidden : Visibility.Visible;
            SqrButton.Visibility = inv ? Visibility.Hidden : Visibility.Visible;
            CubeButton.Visibility = inv ? Visibility.Hidden : Visibility.Visible;
            ExpButton.Visibility = inv ? Visibility.Hidden : Visibility.Visible;
            RootButton.Visibility = inv ? Visibility.Visible : Visibility.Hidden;
            SqrtButton.Visibility = inv ? Visibility.Visible : Visibility.Hidden;
            CbrtButton.Visibility = inv ? Visibility.Visible : Visibility.Hidden;
            LnButton.Visibility = inv ? Visibility.Visible : Visibility.Hidden;
        }

        private static void SetTrigButton(Button btn, string s, double fontSize, FontFamily fontFamily)
        {
            btn.Content = s;
            btn.Tag = s + '(';
            btn.FontSize = fontSize;
            btn.FontFamily = fontFamily;
            btn.FontStretch = fontFamily.Source.Contains("Cond") ?
                FontStretches.Condensed :
                FontStretches.Normal;

            btn.FontWeight = fontFamily.Source.Contains("Light") ?
                FontWeights.Light :
                FontWeights.Normal;

            btn.ToolTip = s switch
            {
                "sin" => MathResources.Sine,
                "cos" => MathResources.Cosine,
                "tan" => MathResources.Tangent,
                "csc" => MathResources.Cosecant,
                "sec" => MathResources.Secant,
                "cot" => MathResources.Cotangent,

                "asin" => MathResources.InverseSine,
                "acos" => MathResources.InverseCosine,
                "atan" => MathResources.InverseTangent,
                "acsc" => MathResources.InverseCosecant,
                "asec" => MathResources.InverseSecant,
                "acot" => MathResources.InverseCotangent,

                "sinh" => MathResources.HyperbolicSine,
                "cosh" => MathResources.HyperbolicCosine,
                "tanh" => MathResources.HyperbolicTangent,
                "csch" => MathResources.HyperbolicCosecant,
                "sech" => MathResources.HyperbolicSecant,
                "coth" => MathResources.HyperbolicCotangent,

                "asinh" => MathResources.InverseHyperbolicSine,
                "acosh" => MathResources.InverseHyperbolicCosine,
                "atanh" => MathResources.InverseHyperbolicTangent,
                "acsch" => MathResources.InverseHyperbolicCosecant,
                "asech" => MathResources.InverseHyperbolicSecant,
                "acoth" => MathResources.InverseHyperbolicCotangent,
                _ => null
            };
        }

        private void ColorScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearOutput();
        }

        private void LightDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearOutput();
        }


        private void ShadowsCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ClearOutput();
        }

        private void SmoothCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ClearOutput();
        }

        private void EmbedCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ClearOutput();
        }

        private void AdaptiveCheckBox_Click(object sender, RoutedEventArgs e)
        {
            _parser.Settings.Plot.IsAdaptive = AdaptiveCheckBox.IsChecked ?? false;
            ClearOutput();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_isParsing)
                {
                    _autoRun = false;
                    Cancel();
                }
                else if (_parser.IsPaused)
                    Cancel();
            }
            else if (e.Key == Key.Pause || e.Key == Key.P && IsControlDown && IsAltDown)
            {
                if (_isParsing)
                    Pause();
            }
        }

        private void Cancel()
        {
            bool isPaused = _parser.IsPaused;
            _parser.Cancel();
            if (isPaused)
            {
                if (IsWebForm)
                    CalculateAsync(true);
                else
                    ShowHelp();
            }
        }

        private void Pause() => _parser.Pause();

        bool _sizeChanged;
        private void RichTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _sizeChanged = true;
            _autoCompleteManager.MoveAutoComplete();
            _lineNumbersDispatcherOperation?.Abort();
            _lineNumbersDispatcherOperation = Dispatcher.InvokeAsync(DrawLineNumbers, DispatcherPriority.ApplicationIdle);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReadSettings();
            if (Top < 0)
                Top = 0;

            var h = SystemParameters.PrimaryScreenHeight;
            if (Height > h)
                Height = h;
        }

        private void WebViewer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                IsSaved = false;
        }

        private async void Include_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var r = (Run)sender;
                var fileName = r?.Text.Trim();
                if (File.Exists(fileName))
                {
                    Mouse.SetCursor(Cursors.Wait);
                    var tt = (ToolTip)r.ToolTip;
                    if (tt is not null)
                        tt.Visibility = Visibility.Hidden;
                    var ext = Path.GetExtension(fileName).ToLowerInvariant();
                    var path = Path.GetFullPath(fileName);
                    Process process;
                    if (ext == ".txt")
                        process = RunExternalApp("NOTEPAD++", path);
                    else
                    {
                        process = RunExternalApp(AppInfo.FullName, path);
                        process ??= RunExternalApp("NOTEPAD++", path);
                    }
                    process ??= RunExternalApp("NOTEPAD", path);
                    if (tt is not null)
                        tt.Visibility = Visibility.Visible;

                    if (process is not null)
                    {
                        _calculateOnActivate = true;
                        if (tt is not null)
                        {
                            string s = Include(fileName, null);
                            tt.Content = HighLighter.GetPartialSource(s);
                        }
                        if (IsCalculated)
                        {
                            if (IsAutoRun)
                            {
                                _isTextChangedEnabled = false;
                                await AutoRun();
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
            }
        }

        private string Include(string fileName, Queue<string> fields)
        {
            var isLocal = false;
            var s = ReadTextFromFile(fileName);
            var j = s.IndexOf('\v');
            var hasForm = j > 0;
            var lines = (hasForm ? s[..j] : s).EnumerateLines();
            var getLines = new List<string>();
            var sf = hasForm ? s[(j + 1)..] : default;
            Queue<string> getFields = GetFields(sf, fields);
            foreach (var line in lines)
            {
                if (Validator.IsKeyword(line, "#local"))
                    isLocal = true;
                else if (Validator.IsKeyword(line, "#global"))
                    isLocal = false;
                else
                {
                    if (!isLocal)
                    {
                        if (Validator.IsKeyword(line, "#include"))
                        {
                            var includeFileName = UserDefined.GetFileName(line);
                            var includeFilePath = Path.GetFullPath(Environment.ExpandEnvironmentVariables(includeFileName));
                            if (!File.Exists(includeFilePath))
                                throw new FileNotFoundException($"{Core.Messages.File_not_found}: {includeFileName}.");

                            getLines.Add(fields is null
                                    ? Include(includeFilePath, null)
                                    : Include(includeFilePath, new()));
                        }
                        else
                            getLines.Add(line.ToString());
                    }
                }
            }
            if (hasForm && string.IsNullOrWhiteSpace(getLines[^1]))
                getLines.RemoveAt(getLines.Count - 1);

            var len = getLines.Count;
            if (len > 0)
            {
                _stringBuilder.Clear();
                for (int i = 0; i < len; ++i)
                {
                    if (getFields is not null && getFields.Count != 0)
                    {
                        if (MacroParser.SetLineInputFields(getLines[i].TrimEnd(), _stringBuilder, getFields, false))
                            getLines[i] = _stringBuilder.ToString();

                        _stringBuilder.Clear();
                    }
                }
            }
            return string.Join(Environment.NewLine, getLines);
        }

        private static Queue<string> GetFields(ReadOnlySpan<char> s, Queue<string> fields)
        {
            if (fields is null)
                return null;

            if (fields.Count != 0)
            {
                if (!s.IsEmpty)
                {
                    var getFields = MacroParser.GetFields(s, '\t');
                    if (fields.Count < getFields.Count)
                    {
                        for (int i = 0; i < fields.Count; ++i)
                            getFields.Dequeue();

                        while (getFields.Count != 0)
                            fields.Enqueue(getFields.Dequeue());
                    }
                }
                return fields;
            }
            else if (!s.IsEmpty)
                return MacroParser.GetFields(s, '\t');
            else
                return null;
        }

        private bool ValidateInputFields(string[] fields)
        {
            for (int i = 0, len = fields.Length; i < len; ++i)
            {
                var s = fields[i].AsSpan();
                if (s.Length > 0)
                {
                    var j = s.IndexOf(':');
                    if (j > 0)
                        s = s[(j + 1)..];
                }
                if (s.Length == 0 || s[0] == '+' || !double.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var _))
                {
                    _wv2Warper.ReportInputFieldError(i);
                    return false;
                }
            }
            return true;
        }


        private bool SetInputFields(string[] fields)
        {
            if (fields is null ||
                fields.Length == 0 ||
                fields.Length == 1 && string.IsNullOrEmpty(fields[0]))
                return true;

            if (!ValidateInputFields(fields))
                return false;

            var p = _document.Blocks.FirstOrDefault();
            var i = 0;
            var line = 0;
            var fline = 0;
            _stringBuilder.Clear();
            var values = new Queue<string>();
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            while (p is not null && i < fields.Length)
            {
                ++line;
                values.Clear();
                while (i < fields.Length)
                {
                    var s = fields[i].AsSpan();
                    if (s.Length > 0)
                    {
                        var j = s.IndexOf(':');
                        if (j < 0 || !int.TryParse(s[..j], out fline))
                            fline = 0;

                        if (fline > line)
                            break;

                        values.Enqueue(s[(j + 1)..].ToString().Trim());
                    }
                    ++i;
                }
                if (values.Count != 0)
                {
                    var r = new TextRange(p.ContentStart, p.ContentEnd);
                    if (MacroParser.SetLineInputFields(r.Text.TrimEnd(), _stringBuilder, values, true))
                    {
                        if (_forceHighlight)
                            r.Text = _stringBuilder.ToString();
                        else
                            _highlighter.Parse(p as Paragraph, IsComplex, line, _stringBuilder.ToString());
                    }
                    _stringBuilder.Clear();
                }
                if (fline > line)
                {
                    line = fline - 1;
                    p = _document.Blocks.ElementAt(line);
                }
                else
                    p = p.NextBlock;
            }
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
            return true;
        }

        private void SetInputFieldsFromFile(SplitEnumerator fields)
        {
            if (fields.IsEmpty)
                return;

            var p = _document.Blocks.FirstOrDefault();
            _stringBuilder.Clear();
            var values = new Queue<string>();
            foreach (var s in fields)
                values.Enqueue(s.ToString());

            while (p is not null && values.Count != 0)
            {
                var r = new TextRange(p.ContentStart, p.ContentEnd);
                if (MacroParser.SetLineInputFields(r.Text.TrimEnd(), _stringBuilder, values, false))
                    r.Text = _stringBuilder.ToString();

                _stringBuilder.Clear();
                p = p.NextBlock;
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
            if (_isParsing)
                return;

            if (IsCalculated || IsWebForm || _parser.IsPaused)
            {
                var fileName = PromtSavePdf();
                if (fileName is not null)
                    SavePdf(fileName);
            }
            else
            {
                var fileName = _currentCultureName == "en" ?
                    $"{AppInfo.DocPath}\\help.pdf" :
                    $"{AppInfo.DocPath}\\help.{_currentCultureName}.pdf";
                if (!File.Exists(fileName))
                    fileName = $"{AppInfo.DocPath}doc\\help.pdf";

                StartPdf(fileName);
            }
        }

        private string PromtSavePdf()
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
            return result ? dlg.FileName : null;
        }

        private async void SavePdf(string pdfFileName)
        {
            var settings = _wv2Warper.CreatePrintSettings();
            await WebViewer.CoreWebView2.PrintToPdfAsync(pdfFileName, settings);
            StartPdf(pdfFileName);
        }

        private static void StartPdf(string pdfFileName)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo(pdfFileName)
                {
                    UseShellExecute = true
                }
            };
            process.Start();
        }

        private void UnitsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ExpressionParser.IsUs = ReferenceEquals(sender, US);
            ClearOutput();
        }
        private async void WebViewer_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!await _wv2Warper.CheckIsReportAsync())
                return;

            _isParsing = false;
            if (_isSaving)
            {
                var zip = string.Equals(Path.GetExtension(CurrentFileName), ".cpdz", StringComparison.OrdinalIgnoreCase);
                if (zip)
                {
                    _macroParser.Parse(InputText, out var outputText, null, 0, false);
                    WriteFile(CurrentFileName, outputText, true);
                }
                else
                    WriteFile(CurrentFileName, GetInputText());

                _isSaving = false;
                IsSaved = true;
            }
            else if (IsWebForm || IsCalculated || _parser.IsPaused)
            {
                SetUnits();
                if (IsCalculated || _parser.IsPaused)
                {
                    if (_scrollOutput)
                        await ScrollOutput();
                    else if (_scrollY > 0)
                    {
                        await _wv2Warper.SetScrollYAsync(_scrollY);
                        _scrollY = 0;
                    }
                }
            }
            if (_scrollOutputToLine > 0)
            {
                await ScrollOutputToLine(_scrollOutputToLine, _scrollOffset);
                _scrollOutputToLine = 0;
            }
        }

        internal static bool Execute(string fileName, string args = "")
        {
            var proc = new Process();
            var psi = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = fileName,
                Arguments = args
            };
            proc.StartInfo = psi;
            try
            {
                return proc.Start();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
                return false;
            }
        }

        private void DecimalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) =>
            DecimalsTextBox.Text = (15 - e.NewValue).ToString(CultureInfo.InvariantCulture);

        private void Record() =>
            _undoMan.Record(
                InputText,
                _currentLineNumber,
                _currentOffset
            );

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
                    else if (c == '‰')
                        c = '‱';
                    else if (c == '‱')
                        c = '‰';
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
            'ø' => 'j',
            'Ø' => 'J',
            '∡' => 'V',
            '@' => '°',
            '\'' => '′',
            '"' => '″',
            '°' => '@',
            '′' => '\'',
            '″' => '"',
            _ => c
        };

        private async void RichTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (IsCalculated)
            {
                _scrollY = await _wv2Warper.GetScrollYAsync();
                await ScrollOutput();
            }
        }

        private void WebViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                Calculate();
                e.Handled = true;
            }
            else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
            {
                Command_Open(this, null);
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
            IsWebView2Focused = false;
            _isTextChangedEnabled = false;
            RichTextBox.Selection.ApplyPropertyValue(TextElement.BackgroundProperty, null);
            AutoCompleteListBox.Visibility = Visibility.Hidden;
            _isTextChangedEnabled = true;
        }

        private void FindReplace_BeginSearch(object sender, EventArgs e)
        {
            _autoRun = false;
            _isTextChangedEnabled = false;
        }

        private void FindReplace_EndSearch(object sender, EventArgs e)
        {
            _isTextChangedEnabled = true;
        }

        private void FindReplace_EndReplace(object sender, EventArgs e)
        {
            Task.Run(() => Dispatcher.InvokeAsync(
                HighLightAll,
                DispatcherPriority.Send));
            Task.Run(() => Dispatcher.InvokeAsync(SetAutoIndent, DispatcherPriority.Normal));
        }

        private void RichTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_countKeys == int.MaxValue)
                _countKeys = int.MinValue;

            ++_countKeys;
            if (!_autoCompleteManager.IsInComment())
            {
                Task.Run(() => Dispatcher.InvokeAsync(() => _autoCompleteManager.InitAutoComplete(e.Text, _currentParagraph), DispatcherPriority.Send));
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            if (_calculateOnActivate)
            {
                if (IsAutoRun)
                    CalculateAsync();
                else
                    Calculate();
                _calculateOnActivate = false;
            }
        }

        private void CodeCheckBox_Click(object sender, RoutedEventArgs e)
        {
            ClearOutput();
        }

        private void SetCodeCheckBoxVisibility() =>
            CodeCheckBorder.Visibility = _highlighter.Defined.HasMacros ? Visibility.Visible : Visibility.Hidden;


        private static void ShowErrorMessage(string message) =>
            MessageBox.Show(message, "Calcpad", MessageBoxButton.OK, MessageBoxImage.Error);

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            await InitializeWebViewer();
            TryOpenOnStartup();
            TryRestoreState();
            RichTextBox.Focus();
            Keyboard.Focus(RichTextBox);
        }

        private async Task InitializeWebViewer()
        {
            var options = new CoreWebView2EnvironmentOptions("--allow-file-access-from-files");
            var env = await CoreWebView2Environment.CreateAsync(
                null,
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CalcpadWebView2"),
                options
            );
            await WebViewer.EnsureCoreWebView2Async(env);
            RichTextBox.IsEnabled = true;
            WebViewer.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "calcpad.local",
                 AppInfo.DocPath,
                CoreWebView2HostResourceAccessKind.Allow);

            WebViewer.CoreWebView2.Settings.AreDevToolsEnabled = false;
            WebViewer.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            WebViewer.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
        }

        private void MenuCli_Click(object sender, RoutedEventArgs e)
        {
            Execute(AppInfo.Path + "Cli.exe");
        }

        private void ZeroSmallMatrixElementsCheckBox_Click(object sender, RoutedEventArgs e) => ClearOutput();

        private void MaxOutputCountTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ClearOutput(false);
        }

        private void MaxOutputCountTextBox_LostFocus(object sender, RoutedEventArgs e) => ClearOutput(false);

        private void PasteAsCommentMenu_Click(object sender, RoutedEventArgs e)
        {
            RichTextBox.BeginChange();
            RichTextBox.Selection.Text = string.Empty;
            InsertLines(Clipboard.GetText(), Environment.NewLine, true);
            RichTextBox.EndChange();
            RichTextBox.Focus();
        }

        private void CommentUncomment(bool comment)
        {
            var ss = RichTextBox.Selection.Start;
            var ps = ss.Paragraph;
            var se = RichTextBox.Selection.End;
            var pe = se.Paragraph;
            var lineNumber = GetLineNumber(ps);
            bool matches;
            RichTextBox.BeginChange();
            do
            {
                if (ps is null)
                    break;
                var tr = new TextRange(ps.ContentStart, ps.ContentEnd);
                var text = tr.Text;
                var isComment = text.StartsWith('\'') ||
                    text.StartsWith('"');
                if (comment != isComment)
                {
                    if (comment)
                        tr.Text = "\'" + text;
                    else
                        tr.Text = text[1..];
                }
                _highlighter.Defined.Get(tr.Text, lineNumber);
                _highlighter.Parse(ps, IsComplex, lineNumber);
                matches = ReferenceEquals(ps, pe);
                ps = ps.NextBlock as Paragraph;
            } while (!matches);
            _currentParagraph = pe;
            HighLighter.Clear(_currentParagraph);
            SetAutoIndent();
            RichTextBox.Selection.Select(ss, se);
            RichTextBox.EndChange();
            RichTextBox.Focus();
        }

        private void CommentMenu_Click(object sender, RoutedEventArgs e) =>
            CommentUncomment(true);


        private void WebViewer_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var message = e.TryGetWebMessageAsString();
            if (message == "clicked")
                WebViewer_LinkClicked();
            else if (message == "focused")
                IsWebView2Focused = true;
        }

        private async void WebViewer_LinkClicked()
        {
            var s = await _wv2Warper.GetLinkDataAsync();
            if (s is null)
                return;

            if (Uri.IsWellFormedUriString(s, UriKind.Absolute))
                Execute(ExternalBrowserComboBox.Text.ToLower() + ".exe", s);
            else
            {
                var fileName = s.Replace('/', '\\');
                var path = Path.GetFullPath(fileName);
                if (File.Exists(path))
                {
                    fileName = path;
                    var ext = Path.GetExtension(fileName).ToLowerInvariant();
                    if (ext == ".cpd" || ext == ".cpdz" || ext == ".txt")
                    {
                        var r = PromptSave();
                        if (r != MessageBoxResult.Cancel)
                            FileOpen(fileName);
                    }
                    else if (ext == ".htm" ||
                        ext == ".html" ||
                        ext == ".png" ||
                        ext == ".jpg" ||
                        ext == ".jpeg" ||
                        ext == ".gif" ||
                        ext == ".bmp")
                        Execute(ExternalBrowserComboBox.Text.ToLower() + ".exe", s);
                }
                else if (s == "continue")
                    await AutoRun();
                else if (s == "cancel")
                    Cancel();
                else if (IsCalculated || _parser.IsPaused)
                    LineClicked(s);
                else if (!IsWebForm)
                    LinkClicked(s);
            }
        }

        private void MarkdownCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var blocks = _document.Blocks;
            if (MarkdownCheckBox.IsChecked == true)
            {
                var n1 = LastIndexOfParagraphContaining("#md");
                var n2 = LastIndexOfParagraphContaining("#md off");
                var n3 = LastIndexOfParagraphContaining("#md on");
                n1 = Math.Max(n1, n3);
                if (n1 < 0)
                {
                    n2 = n2 < 0 ? 0 : _currentLineNumber;
                    var p = new Paragraph(new Run("#md on") { Foreground = HighLighter.KeywordBrush });
                    var b = blocks.ElementAt(n2);
                    if (b is not null)
                        blocks.InsertBefore(b, p);
                }
                else if (n2 > n1)
                {
                    var p = new Paragraph(new Run("#md on") { Foreground = HighLighter.KeywordBrush });
                    if (_currentParagraph is not null)
                        blocks.InsertBefore(_currentParagraph, p);
                }
            }

            int LastIndexOfParagraphContaining(string s)
            {
                var i = _currentLineNumber;
                var p = _currentParagraph;
                while (p is not null && i >= 0)
                {
                    var text = new TextRange(p.ContentStart, p.ContentEnd).Text;
                    if (text.Trim() == s)
                        return i;
                    --i;
                    p = p.PreviousBlock as Paragraph;
                }
                return -1;
            }
        }

        private void UncommentMenu_Click(object sender, RoutedEventArgs e) =>
            CommentUncomment(false);

        private void RichTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            IsWebView2Focused = false;
        }
    }
}