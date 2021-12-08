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
                Version = string.Join(".", FileVersionInfo.GetVersionInfo(FullName).ProductVersion.Split('.').Take(2));
                Title = "Calcpad " + Version;
            }
            internal static readonly string Path;
            internal static readonly string Name;
            internal static readonly string FullName;
            internal static readonly string Version;
            internal static readonly string Title;
        }

        private string _cfn;
        private readonly ExpressionParser _parser;
        private readonly string _htmlWorksheet;
        private readonly string _htmlParsing;
        private readonly string _htmlHelp;
        private readonly StringBuilder _htmlBuilder = new();
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
                    WebFormButton.ToolTip = "Редактирай програмния код";
#else
                    OutputFrame.Header = "Input";
                    WebFormButton.ToolTip = "Open source code for editing";
#endif
                    MenuWebForm.Icon = "  ✓";
                }
                else
                {
                    var cursor = WebBrowser.Cursor;
                    WebBrowser.Cursor = Cursors.Wait;
                    ForceHighlight();
                    DispatchLineNumbers();
                    SaveInputToCode();
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
            _undoMan.Record(InputText, 0, null);
            _wbWarper = new WebBrowserWrapper(WebBrowser);
            _parser.Settings.Plot.ImagePath = string.Empty; //tmpDir;
            _parser.Settings.Plot.ImageUri = string.Empty; //tmpDir;
            _parser.Settings.Plot.VectorGraphics = false;
            _parser.Settings.Plot.ScreenScaleFactor = ScreenMetrics.GetWindowsScreenScalingFactor();
            _cfn = string.Empty;
            TryOpenOnStartup();
            _isTextChangedEnabled = false;
            IsSaved = true;
            RichTextBox.Focus();
        }

        private void ForceHighlight()
        {
            if (_forceHighlight)
            {
                HighLightAll();
                RichTextBox.CaretPosition = _document.ContentStart;
                Dispatcher.InvokeAsync(SetAutoIndent, DispatcherPriority.ApplicationIdle);
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
        }

        private void RichTextBox_Scroll(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0 && !_sizeChanged && !IsWebForm)
                DispatchLineNumbers();
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
            if (tag.Contains("|"))
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
            else if (tag.Contains("§"))
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
                HighLighter.Parse(tp.Paragraph, IsComplex);
                if (selectionLength > 0)
                    tp = RichTextBox.Selection.End;

                for (var i = 1; i < parts.Length; i++)
                {
                    tp = tp.Paragraph.ContentEnd.InsertParagraphBreak();
                    tp.InsertTextInRun(parts[i]);
                    HighLighter.Parse(tp.Paragraph, IsComplex);
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
                                HighLighter.Parse(tp.Paragraph, IsComplex);
                                SetAutoIndent();
                                tp = tp.Paragraph.ContentEnd;
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
                    n++;
            return n;
        }

        private void RemoveLine() { _document.Blocks.Remove(RichTextBox.Selection.Start.Paragraph); }
        private void RemoveChar() { RichTextBox.Selection.Start.DeleteTextInRun(-1); }
        private void InsertLine() { RichTextBox.CaretPosition = RichTextBox.Selection.Start.InsertParagraphBreak(); }

        private void InsertText(string text)
        {
            RichTextBox.Selection.Text = string.Empty;
            RichTextBox.Selection.End.InsertTextInRun(text);
            RichTextBox.Selection.End.GetPositionAtOffset(text.Length);
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
                tp.GetPositionAtOffset(lines[0].Length);
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
                    HighLighter.Parse(p, IsComplex);
                }
                RichTextBox.Selection.Select(tp, tp);
                RichTextBox.EndChange();
                _isTextChangedEnabled = true;
                DispatchAutoIndent();
            }
            RichTextBox.Focus();
        }
        private void CalcButton_Click(object sender, RoutedEventArgs e)
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
        }

        private void Command_Open(object sender, ExecutedRoutedEventArgs e)
        {
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
            if ((string)SaveButton.Tag == "S" || CurrentFileName.Length == 0)
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

                    j++;
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
                    var firstMenu = (MenuItem) MenuRecent.Items[0];
                    DocumentPath = Path.GetDirectoryName((string) firstMenu.ToolTip);
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
            for (var i = 0; i < n; i++)
            {
                var menu = (MenuItem)MenuRecent.Items[i];
                var value = (string) menu.ToolTip;
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
            for (var i = 0; i < n; i++)
            {
                var menu = (MenuItem)MenuRecent.Items[i];
                if (!string.Equals(menu.ToolTip, fileName))
                    continue;

                for (var j = i; j > 0; j--)
                {
                    menu = (MenuItem) MenuRecent.Items[j];
                    menu.Header = ((MenuItem) MenuRecent.Items[j - 1]).Header;
                    menu.ToolTip = ((MenuItem) MenuRecent.Items[j - 1]).ToolTip;
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
                n--;
            }
            var newMenu = new MenuItem()
            {
                ToolTip = fileName,
                Icon = "   1",
                Header = Path.GetFileName(fileName),
            };
            newMenu.Click += RecentFileList_Click;
            MenuRecent.Items.Insert(0, newMenu);
            for (var i = 1; i <= n; i++)
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

        private void Command_SaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            FileSaveAs();
        }

        private void FileSaveAs()
        {
            string s;
            if (string.IsNullOrWhiteSpace(CurrentFileName))
                s = Path.GetExtension(CurrentFileName);
            else if (InputText.Contains("?"))
                s = ".cpd";
            else
                s = ".txt";

            var dlg = new SaveFileDialog
            {
                FileName = Path.GetFileName(CurrentFileName),
                InitialDirectory = File.Exists(CurrentFileName) ? Path.GetDirectoryName(CurrentFileName) : DocumentPath,
                DefaultExt = s,
                OverwritePrompt = true
            };
            dlg.Filter = s switch
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
            };

            var result = (bool)dlg.ShowDialog();
            if (!result)
                return;

            CurrentFileName = dlg.FileName;
            if (s == ".cpdz" && Path.GetExtension(CurrentFileName) != ".cpdz")
                CurrentFileName = Path.ChangeExtension(CurrentFileName, ".cpdz");

            FileSave();
            AddRecentFile(CurrentFileName);
        }

        private void FileSave()
        {
            var text = GetInputText();
            if (text.Contains("?"))
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

        private void Command_Close(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Command_Copy(object sender, ExecutedRoutedEventArgs e)
        {
            RichTextBox.Copy();
        }

        private void Command_Paste(object sender, ExecutedRoutedEventArgs e)
        {
            if (InputFrame.Visibility == Visibility.Visible)
            {
                RichTextBox.Paste();
                RichTextBox.Focus();
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

        private void Command_Print(object sender, ExecutedRoutedEventArgs e)
        {
            _wbWarper.PrintPreview();
        }

        private string _searchString = string.Empty;
        private void Command_Find(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsWebForm)
                return;

            string s = RichTextBox.Selection.Text;
            if (!string.IsNullOrWhiteSpace(s))
                _searchString = s;
#if BG
            if (InputBox.Show("Търсене", "Въведете текст да търсене:", ref _searchString))
#else
            if (InputBox.Show("Find", "Enter text to search for:", ref _searchString))
#endif
            {
                _searchString = _searchString.ToLowerInvariant();
                FindNext();
            }
        }

        private void Command_FindNext(object sender, ExecutedRoutedEventArgs e)
        {
            FindNext();
        }

        private void FindNext()
        {
            var isStart = false;
            var p = (Paragraph)RichTextBox.Selection.End.Paragraph.NextBlock;
            if (p == null)
            {
                p = (Paragraph)_document.Blocks.FirstBlock;
                isStart = true;
            }
            var length = _searchString.Length;
            while (p != null)
            {
                var s = new TextRange(p.ContentStart, p.ContentEnd).Text.ToLowerInvariant();
                var i = s.IndexOf(_searchString, StringComparison.InvariantCulture) + 1;
                if (i > 0)
                {
                    RichTextBox.Selection.Select(p.ContentStart, p.ContentStart);
                    RichTextBox.Selection.Select(p.ContentStart.GetPositionAtOffset(i), p.ContentStart.GetPositionAtOffset(i + length));
                    return;
                }
                p = (Paragraph)p.NextBlock;
                if (p is null)
                {
#if BG
                    if (isStart)
                    {
                        MessageBox.Show("Текстът не е намерен.");
                        return;
                    }
                    else if (MessageBox.Show(
                        "Достигнат е краят на текста. Искате ли да продължа търсенето от началото?",
                        "Търсене",
                        MessageBoxButton.YesNo) 
                        == MessageBoxResult.No)
                        return;

#else
                    if (isStart)
                    {
                        MessageBox.Show("Text not found.");
                        return;
                    }
                    else if (MessageBox.Show(
                        "End of text reached. Would you like to search from the beginning?", 
                        "Find",
                        MessageBoxButton.YesNo
                        ) == MessageBoxResult.No)
                            return;
#endif
                    isStart = true;
                    p = (Paragraph)_document.Blocks.FirstBlock;
                }
            }
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
                        DispatchLineNumbers();
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
                FileSaveAs();

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

            _parser.Settings.Math.Substitute = SubstituteCheckBox.IsChecked != null && (bool)SubstituteCheckBox.IsChecked;
            if (IsWebForm)
            {
                if (!toWebForm)
                    GetAndSetInputFields();
            }
            else
                ReadAndSetInputFields();

            var text = InputText;
            if (toWebForm)
                _parser.Parse(text, false);
            else
            {
                _isParsing = true;
                WebFormButton.IsEnabled = false;
                MenuWebForm.IsEnabled = false;
                CalcButton.IsEnabled = false;
                MenuCalculate.IsEnabled = false;
                WebBrowser.InvokeScript($"delayedLoad", _htmlParsing);
                _parseTask = Task.Run(() =>
                {
                    _parser.Parse(text);
                });
                await _parseTask;
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
                WebBrowser.NavigateToString($"<p>Неочаквана грешка: {e.Message}</p>");
#else
                WebBrowser.NavigateToString($"<p>Unexpected error: {e.Message}</p>");
#endif
            }
            if (IsWebForm)
#if BG
                OutputFrame.Header = toWebForm ? "Входни данни" : "Резултати";
#else
                OutputFrame.Header = toWebForm ? "Input" : "Output";
#endif
            if (!toWebForm)
                GC.Collect();
        }

        private string HtmlApplyWorksheet(string s)
        {
            _htmlBuilder.Clear();
            _htmlBuilder.Append(_htmlWorksheet);
            _htmlBuilder.Append(s);
            _htmlBuilder.Append("</body></html>");
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
            foreach (var line in lines)
            {
                string s;
                if (line.Contains("\v"))
                {
                    hasForm = true;
                    var n = line.IndexOf('\v');
                    s = line[(n + 1)..];
                    var inputFields = s.Split('\t');
                    _parser.ClearInputFields();
                    SetInputFields(inputFields);
                    s = line.Substring(0, n);
                }
                else if (highLight)
                    s = line.TrimStart('\t');
                else
                    s = line;

                var p = new Paragraph();
                p.Inlines.Add(new Run(s));
                if (highLight)
                    HighLighter.Parse(p, IsComplex);

                _document.Blocks.Add(p);
            }
            _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            if (highLight)
            {
                HighLighter.Clear(_currentParagraph);
                Dispatcher.InvokeAsync(SetAutoIndent, DispatcherPriority.ApplicationIdle);
            }
            _undoMan.Reset();
            _isTextChangedEnabled = true;
            _forceHighlight = !highLight;
            return hasForm;
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _document.Blocks.Clear();
        }

        private string GetInputText()
        {
            const double step = 20.0;
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
            while (b != null)
            {
                var n = (int)(((Paragraph)b).TextIndent / step);
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

        private void CopyOutputButton_Click(object sender, RoutedEventArgs e)
        {
            _wbWarper.ClipboardCopy();
        }

        private void WordButton_Click(object sender, RoutedEventArgs e)
        {
            var isDoc = (bool)Professional.IsChecked && (IsCalculated || IsWebForm);
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
            if (fileName != null)
                startInfo.Arguments = fileName.Contains(" ") ? '\"' + fileName + '\"' : fileName;

            startInfo.UseShellExecute = true;
            startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            try
            {
                return Process.Start(startInfo) != null;
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
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            double indent = 0;
            var values = _undoMan.RestoreValues;
            _document.Blocks.Clear();
            int i = 0, n = 0;
            if (values != null)
                n = values.Length;

            using (var sr = new StringReader(_undoMan.RestoreText))
            {
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line is null)
                        break;
                    var p = new Paragraph();
                    p.Inlines.Add(new Run(line));
                    HighLighter.Parse(p, IsComplex);
                    if (!UpdateIndent(p, ref indent))
                        p.TextIndent = indent;

                    foreach (var inline in p.Inlines)
                        if (i < n && inline.ToolTip is ToolTip tt)
                            tt.Content = values[i++];

                    _document.Blocks.Add(p);
                }
            }
            var pointer = _document.ContentStart;
            pointer = pointer.GetPositionAtOffset(_undoMan.RestorePointer);
            if (pointer != null)
            {
                var p = pointer.Paragraph;
                if (p != null)
                    RichTextBox.CaretPosition = p.ContentEnd;
            }
            _currentParagraph = RichTextBox.CaretPosition.Paragraph;
            HighLighter.Clear(_currentParagraph);
            DispatchLineNumbers();
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private void WebFormButton_Click(object sender, RoutedEventArgs e)
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
            HighlightCurrentLine();
            var values = ReadInputFromCode();
            ClearCurrentLine();
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
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] is null || s[i].Length == 0)
                    _parser.SetInputField("0");
                else
                    _parser.SetInputField(s[i].Replace(',', '.'));
            }
        }

        private void SubstituteCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            ClearOutput();
        }

        private void DecimalsTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ClearOutput();
            if (IsInitialized && int.TryParse(DecimalsTextBox.Text, out int n))
                DecimalScrollBar.Value = 15 - n;
        }

        private void ClearOutput()
        {
            if (IsInitialized && IsCalculated)
            {
                IsCalculated = false;
                if (IsWebForm)
                    CalculateAsync(true);

                else
                    ShowHelp(); //WebView.NavigateToString(" ");
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
                HighLighter.Parse(p, IsComplex);
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
                    HighLightAll();
            }
        }

        private void SaveOutputButton_Click(object sender, RoutedEventArgs e)
        {
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
            DispatchLineNumbers();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!IsSaved)
                if (PromptSave() == MessageBoxResult.Cancel)
                    e.Cancel = true;

            ClearTempFolder();
            WriteRecentFiles();
        }

        private void RichTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _document.Blocks.Count < 51)
            {
                IsCalculated = true;
                CalculateAsync();
            }
            else if (_forceBackSpace && RichTextBox.CaretPosition.IsAtLineStartPosition)
            {
                _forceBackSpace = false;
                var p = RichTextBox.CaretPosition.Paragraph;
                if (p != null)
                {
                    var pp = (Paragraph)p.PreviousBlock;
                    if (pp != null)
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
        }

        private int _textLength;
        private bool _forceUndo;
        private bool _forceBackSpace;
        private bool _isPasting;
        private int _pasteOffset;
        private TextPointer _pasteStart;
        private void RichTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_isTextChangedEnabled)
            {
                if (_isPasting)
                {
                    HighLightPastedText();
                    SetAutoIndent();
                    var p = RichTextBox.Selection.End.Paragraph;
                    if (p != null)
                        RichTextBox.CaretPosition = p.ContentEnd.GetPositionAtOffset(_pasteOffset);
                    _isPasting = false;
                    _forceUndo = true;
                }
                var txtLen = InputTextLength;
                if (_forceUndo || Math.Abs(_textLength - txtLen) > 1)
                {
                    var tp = RichTextBox.Selection.Start;
                    var values = ReadInputFromCode();
                    _undoMan.Record(InputText, _document.ContentStart.GetOffsetToPosition(tp), values);
                    _forceUndo = false;
                }
                _textLength = txtLen;
                IsSaved = false;
                if (IsCalculated)
                {
                    ShowHelp(); //WebView.NavigateToString(" ");
                    IsCalculated = false;
                }
                if (!_isPasting)
                    DispatchAutoIndent();

                DispatchLineNumbers();
            }
        }

        private void RichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var tps = RichTextBox.Selection.Start;
            var tpe = RichTextBox.Selection.End;
            if (ReferenceEquals(_currentParagraph, tps.Paragraph) ||
                ReferenceEquals(_currentParagraph, tpe.Paragraph)) 
                return;

            _isTextChangedEnabled = false;
            HighLighter.Parse(_currentParagraph, IsComplex);
            _currentParagraph = tps.Paragraph;
            HighLighter.Clear(_currentParagraph);
            RichTextBox.Selection.Select(tps, tpe);
            _isTextChangedEnabled = true;
        }

        private void RichTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            _isTextChangedEnabled = false;
            HighLighter.Clear(_currentParagraph);
            _isTextChangedEnabled = true;
        }

        private void RichTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            _isTextChangedEnabled = false;
            HighLighter.Parse(_currentParagraph, IsComplex);
            _isTextChangedEnabled = true;
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
            _pasteStart = RichTextBox.Selection.End;
            _pasteOffset = _pasteStart.Paragraph?.ContentEnd.GetOffsetToPosition(_pasteStart) ?? 0;
        }

        private DispatcherOperation _lineNumbersDispatcherOperation;
        private void DispatchLineNumbers()
        {
            _lineNumbersDispatcherOperation?.Abort();
            _lineNumbersDispatcherOperation = Dispatcher.InvokeAsync(DrawLineNumbers, DispatcherPriority.Render);
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
                i++;
                if (ReferenceEquals(block, b))
                    break;
            }
            while (b != null)
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
                    j++;
                }
                b = b.NextBlock;
                i++;
            }
            if (j < n)
                LineNumbers.Children.RemoveRange(j, n - j);
            _sizeChanged = false;
        }

        private Key _oldKey = Key.Enter;
        private void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != _oldKey)
            {
                _oldKey = e.Key;
                _forceUndo = _oldKey == Key.Space ||
                             _oldKey == Key.Enter ||
                             _oldKey == Key.Delete ||
                             _oldKey == Key.Back;
            }

            if (_oldKey == Key.Enter)
                RichTextBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
            if (_oldKey == Key.Back)
            {
                var tp = RichTextBox.Selection.Start;
                _forceBackSpace = tp.IsAtLineStartPosition && tp.Paragraph.TextIndent > 0;
            }
            else
                _forceBackSpace = false;
        }

        private DispatcherOperation _autoIndentDispatcherOperation;
        private void DispatchAutoIndent()
        {
            if (_autoIndentDispatcherOperation?.Status != DispatcherOperationStatus.Executing)
                _autoIndentDispatcherOperation = Dispatcher.InvokeAsync(AutoIndent, DispatcherPriority.ApplicationIdle);
        }

        private void AutoIndent()
        {
            var p = RichTextBox.Selection.End.Paragraph;
            if (p is null)
                p = (Paragraph)_document.Blocks.FirstBlock;
            else if (p.PreviousBlock != null)
                p = (Paragraph)p.PreviousBlock;

            if (p is null)
            {
                p = new Paragraph(new Run());
                _document.Blocks.Add(p);
            }
            const double step = 28;
            var indent = 0.0;
            var i = 0;
            var pp = ((Paragraph)p.PreviousBlock);
            if (pp != null)
            {
                indent = pp.TextIndent;
                var s = new TextRange(pp.ContentStart, pp.ContentEnd).Text.ToLowerInvariant();
                if (s.Length > 0 && s[0] == '#' && (s.StartsWith("#if") || s.StartsWith("#else") || s.StartsWith("#repeat")))
                    indent += step;
            }

            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            while (p != null)
            {
                if (!UpdateIndent(p, ref indent))
                {
                    if (p.TextIndent == indent)
                    {
                        i++;
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
            while (p != null)
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
            const double step = 28.0;
            var s = new TextRange(p.ContentStart, p.ContentEnd).Text.ToLowerInvariant();
            if (s.Length > 0 && s[0] == '#')
            {
                if (!(s.StartsWith("#if") ||
                    s.StartsWith("#el") ||
                    s.StartsWith("#en") ||
                    s.StartsWith("#re") ||
                    s.StartsWith("#lo")))
                    return false;
                else if (s.StartsWith("#if") || s.StartsWith("#repeat"))
                {
                    p.TextIndent = indent;
                    indent += step;
                }
                else if (indent > 0 && (s.StartsWith("#end if") || s.StartsWith("#loop")))
                {
                    indent -= step;
                    p.TextIndent = indent;
                }
                else
                    p.TextIndent = Math.Max(indent - step, 0);

                return true;
            }
            return false;
        }

        private void SaveInputToCode()
        {
            var p = (Paragraph)_document.Blocks.FirstBlock;
            var values = _wbWarper.GetInputVaues();
            int i = 0, n = values.Length;
            while (p != null)
            {
                if (p.Tag is Queue<string> cache)
                    for (var j = 0; j < cache.Count; j++)
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
            while (p != null)
            {
                if (p.Tag is Queue<string> cache)
                    for (var i = 0; i < cache.Count; i++)
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
            var p = (Paragraph)_document.Blocks.FirstBlock;
            while (p != null)
            {
                if (_forceHighlight)
                {
                    var t = new TextRange(p.ContentStart, p.ContentEnd);
                    t.Text = t.Text.TrimStart('\t');
                }
                if (p.Tag == null)
                    HighLighter.Parse(p, IsComplex);

                p = (Paragraph)p.NextBlock;
            }
            _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private void HighLightPastedText()
        {
            _isTextChangedEnabled = false;
            RichTextBox.BeginChange();
            var p = (Paragraph)_pasteStart.Paragraph;
            _currentParagraph = RichTextBox.Selection.Start.Paragraph;
            if (p is null)
                p = (Paragraph)_document.Blocks.FirstBlock;
            while (p != _currentParagraph)
            {
                HighLighter.Parse(p, IsComplex);
                p = (Paragraph)p.NextBlock;
            }
            HighLighter.Clear(_currentParagraph);
            RichTextBox.EndChange();
            _isTextChangedEnabled = true;
        }

        private void HighlightCurrentLine()
        {
            if (_currentParagraph != null)
            {
                _isTextChangedEnabled = false;
                RichTextBox.BeginChange();
                HighLighter.Parse(_currentParagraph, IsComplex);
                RichTextBox.EndChange();
                _isTextChangedEnabled = true;
            }
        }

        private void ClearCurrentLine()
        {
            if (_currentParagraph != null)
            {
                _isTextChangedEnabled = false;
                RichTextBox.BeginChange();
                HighLighter.Clear(_currentParagraph);
                RichTextBox.EndChange();
                _isTextChangedEnabled = true;
            }
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
                var d = RichTextBox.FontSize + 2 * Math.Sign(e.Delta);
                if (d > 4 && d < 42)
                {
                    RichTextBox.FontSize = d;
                    DispatchLineNumbers();
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
            _parser.Settings.Plot.Shadows = ShadowsCheckBox.IsChecked != null && (bool)ShadowsCheckBox.IsChecked;
            ClearOutput();
        }

        private void LightDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _parser.Settings.Plot.LightDirection = (PlotSettings.LightDirections)LightDirectionComboBox.SelectedIndex;
            ClearOutput();
        }


        private void ShadowsCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            _parser.Settings.Plot.Shadows = ShadowsCheckBox.IsChecked != null && (bool)ShadowsCheckBox.IsChecked;
            ClearOutput();
        }

        private void SmoothCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            _parser.Settings.Plot.SmoothScale = SmoothCheckBox.IsChecked != null && (bool)SmoothCheckBox.IsChecked;
            ClearOutput();
        }

        private void AdaptiveCheckBox_Clicked(object sender, RoutedEventArgs e)
        {
            _parser.Settings.Plot.IsAdaptive = AdaptiveCheckBox.IsChecked != null && (bool)AdaptiveCheckBox.IsChecked;
            ClearOutput();
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _isParsing)
                _parser.Cancel();
        }

        bool _sizeChanged = false;
        private void RichTextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _sizeChanged = true;
            _lineNumbersDispatcherOperation?.Abort();
            _lineNumbersDispatcherOperation = Dispatcher.InvokeAsync(DrawLineNumbers, DispatcherPriority.Background);
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

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
                    ShowHelp(); //WebView.NavigateToString(" ");
                    IsCalculated = false;
                }
                var tp = RichTextBox.Selection.Start;
                var values = ReadInputFromCode();
                _undoMan.Record(InputText, _document.ContentStart.GetOffsetToPosition(tp), values);
                e.Handled = true;
            }
        }

        private void Logo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var info = new ProcessStartInfo
            {
                FileName = "https://proektsoft.bg",
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
            if (htmlFileName.Contains(" "))
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
            if (WebBrowser.Source != null && WebBrowser.Source.Fragment == "#0")
            {
                var s = _wbWarper.GetLinkData();
                if (!string.IsNullOrEmpty(s))
                    LinkClicked(s);
            }
            else if (_isSaving)
            {
                var text = GetInputTextAndValues();
                WriteFile(CurrentFileName, text);
                _isSaving = false;
                IsSaved = true;
            }
            else if (IsWebForm || IsCalculated)
                SetUnits();
        }

        private void DecimalScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DecimalsTextBox.Text = (15 - e.NewValue).ToString();
        }
    }
}