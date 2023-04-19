using Calcpad.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Calcpad.Cli
{
    class Program
    {
        const string Prompt = " |> ";
        internal static readonly string AppPath = AppContext.BaseDirectory;
        struct Line
        {
            public string Input, Output;
            public Line(string Input)
            {
                this.Input = Input;
                Output = string.Empty;
            }
        }

        static void Main()
        {
            Settings settings = GetSettings();
            if (TryConvertOnStartup(settings))
                return;

            MathParser mp = new(settings.Math);
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WindowWidth = 85;
            List<Line> Lines = new();
            var Title = TryOpenOnStartup(Lines);
            Header(Title, settings.Math.Degrees);
            if (Title.Length > 0)
                Render(mp, Lines, true);

            while (true)
            {
                var LineNo = (Lines.Count + 1).ToString().PadLeft(3) + Prompt;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(LineNo);
                Console.ResetColor();
                var s = Console.ReadLine();
                if (s.Length == 0)
                {
                    Header(Title, settings.Math.Degrees);
                    Render(mp, Lines, true);
                }
                else
                {
                    string sCaps = s.ToUpper().Trim();
                    switch (sCaps)
                    {
                        case "NEW":
                            Title = string.Empty;
                            mp = new(settings.Math);
                            Lines.Clear();
                            Header(Title, settings.Math.Degrees);
                            break;
                        case "OPEN":
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            var t = Open(Title, LineNo, Lines);
                            if (!string.IsNullOrEmpty(t))
                            {
                                Title = t;
                                mp = new(settings.Math);
                                Header(Title, settings.Math.Degrees);
                                Render(mp, Lines, true);
                            }
                            break;
                        case "SAVE":
                            Title = Save(Title, LineNo, Lines);
                            Header(Title, settings.Math.Degrees);
                            Render(mp, Lines, false);
                            break;
                        case "EXIT":
                            return;
                        case "CLS":
                        case "DEL":
                        case "RESET":
                            Header(Title, settings.Math.Degrees);
                            if (sCaps == "DEL" && Lines.Count > 0)
                                Lines.RemoveAt(Lines.Count - 1);

                            if (sCaps != "CLS")
                                Render(mp, Lines, sCaps == "RESET");

                            break;
                        case "LIST":
                            List(LineNo);
                            break;
                        case "DEG":
                        case "RAD":
                        case "GRA":
                            settings.Math.Degrees = sCaps == "DEG" ? 0: sCaps == "RAD" ? 1 : 2;
                            mp.Degrees = settings.Math.Degrees;
                            Header(Title, settings.Math.Degrees);
                            Render(mp, Lines, true);
                            break;
                        case "SETTINGS":
                        case "OPTIONS":
                            if (Execute("NOTEPAD", AppPath + "Settings.xml"))
                            {
                                settings = GetSettings();
                                mp = new(settings.Math);
                                Header(Title, settings.Math.Degrees);
                                Render(mp, Lines, true);
                            }
                            break;
                        case "LICENSE":
                        case "HELP": 
                            RenderFile(sCaps);
                            break;
                        default:
                            Console.SetCursorPosition(0, Console.CursorTop - 1);
                            Line L = new(s);
                            if (Calculate(mp, LineNo, ref L))
                                Lines.Add(L);

                            break;
                    }
                }
            }
        }

        static Settings GetSettings()
        {
            Settings settings = new(); 
            settings.Math.Decimals = 6;
            XmlSerializer writer = new(settings.GetType());
            var path = AppPath + "Settings.xml";
            FileStream file = null; ;
            try
            {
                if (Path.Exists(path))
                {
                    file = File.OpenRead(path);
                    settings = (Settings)writer.Deserialize(file);
                }
                else
                {
                    file = File.Create(path);
                    writer.Serialize(file, settings);
                }
            }
            catch (Exception ex)
            {
                file?.Close();
                var key = WriteErrorAndWait(ex.Message, "Would you like to restore the previous settings (y/n)?");
                if (key.Key == ConsoleKey.Y)
                    TryRestoreSettings(settings, writer, path);
            }
            finally
            {
                file?.Close();
            }
            return settings;
        }

        private static void TryRestoreSettings(Settings settings, XmlSerializer writer, string path)
        {
            try
            {
                if (Path.Exists(path))
                {
                    FileStream file = File.OpenWrite(path);
                    writer.Serialize(file, settings);
                    file.Close();
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                WriteErrorAndWait(ex.Message);
            }
        }

        static void RenderFile(string fileName)
        {
            var path = AppPath + fileName + ".txt";
            try
            {
                Console.Write(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);    
            }
            Console.WriteLine();
        }

        static bool TryConvertOnStartup(Settings settings)
        {
            var args = Environment.GetCommandLineArgs();
            var n = args.Length;
            if (n == 0)
                return false;

            var fileName = string.Join(" ", args, 1, n - 1).ToLower();
            var i = fileName.IndexOf(".cpd");
            if (i < 0)
                return false;
            i += 4;
            var outFile = fileName[i..].Trim();
            fileName = fileName[..i].Trim();

            if (string.IsNullOrWhiteSpace(outFile))
                outFile = Path.ChangeExtension(fileName, ".html");
            else if (Directory.Exists(outFile))
                outFile += Path.GetFileNameWithoutExtension(fileName) + ".html";
            else if (string.Equals(outFile, "html") ||
                     string.Equals(outFile, "htm") ||
                     string.Equals(outFile, "docx") ||
                     string.Equals(outFile, "pdf"))
                outFile = Path.ChangeExtension(fileName, "." + outFile);

            var ext = Path.GetExtension(outFile);
            var code = File.ReadAllText(fileName);
            ExpressionParser parser = new()
            {
                Settings = settings
            };
            try 
            { 
                parser.Parse(code, true);
                Converter converter = new();
                if (ext == ".html" || ext == ".htm")
                    converter.ToHtml(parser.HtmlResult, outFile);
                else if (ext == ".docx")
                    converter.ToOpenXml(parser.HtmlResult, outFile);
                else if (ext == ".pdf")
                    converter.ToPdf(parser.HtmlResult, outFile);
                else
                    return false;
                return true;
            }
            catch (Exception ex) 
            {
                WriteErrorAndWait(ex.Message);
                return false;
            }
        }

        private static ConsoleKeyInfo WriteErrorAndWait(string message, string prompt = null)
        {
            WriteError(message, true);
            prompt ??= "Press any key to continue.";
            Console.WriteLine(prompt);
            return Console.ReadKey();
        }

        static string TryOpenOnStartup(List<Line> Lines)
        {
            var args = Environment.GetCommandLineArgs();
            var n = args.Length;
            if (n > 1)
            {
                var fileName = string.Join(" ", args, 1, n - 1).ToLower();
                if (File.Exists(fileName))
                {
                    if (Path.GetExtension(fileName) == ".cpc")
                    {
                        Lines.Clear();
                        using (StreamReader sr = new(fileName))
                            while (!sr.EndOfStream)
                                Lines.Add(new Line(sr.ReadLine()));

                        return Path.GetFileNameWithoutExtension(fileName);
                    }
                }
            }
            return string.Empty;
        }

        static void Header(string Title, int drg)
        {
            Console.Clear();
            var n = Math.Min(Console.WindowWidth, Console.BufferWidth);
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            Console.WriteLine(new string('—', n));
            Console.WriteLine($" Welcome to Calcpad command line interpreter v.{ver.Major}.{ver.Minor}.{ver.Build}!");
            Console.WriteLine(" Copyright: © 2023 by Proektsoft EOOD");
            Console.Write("\r\n Commands: NEW OPEN SAVE LIST EXIT RESET CLS DEL ");
            switch (drg)
            {
                case 0:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("DEG ");
                    Console.ResetColor();
                    Console.Write("RAD ");
                    Console.Write("GRA ");
                    break;
                case 1:
                    Console.Write("DEG ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("RAD ");
                    Console.ResetColor();
                    Console.Write("GRA ");
                    break;
                default:
                    Console.Write("DEG ");
                    Console.Write("RAD ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("GRA ");
                    Console.ResetColor();
                    break;
            }
            Console.Write("SETTINGS LICENSE HELP\r\n");
            Console.WriteLine(new string('—', n));
            if (Title.Length > 0)
                Console.WriteLine(" " + Title + ":\n");
            else
                Console.WriteLine(" Enter math expressions or commands (or type HELP for further instructions):\n");
        }

        static bool Calculate(MathParser mp, string Prompt, ref Line L)
        {
            try
            {
                var Buffer = GetVariables(Prompt, L.Input);
                var Tokens = Buffer.Split('\'');
                L.Output = string.Empty;
                for (int i = 0; i < Tokens.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        if (Tokens[i].Length > 0)
                        {
                            var s = Tokens[i]
                                .Replace(" ", "")
                                .Replace("==", "≡")
                                .Replace("!=", "≠")
                                .Replace("<=", "≤")
                                .Replace(">=", "≥")
                                .Replace("%%", "⦼");
                            mp.Parse(s);
                            mp.Calculate();
                            L.Output += mp.ToString().Trim() + ' ';
                        }
                    }
                    else
                        L.Output += Tokens[i].Trim() + ' ';
                }
                var Output = Prompt + L.Output.PadRight(Buffer.Length + 1);
                Console.WriteLine(Output);
                mp.SaveAnswer();
                return true;
            }
            catch (Exception ex)
            {
                WriteError(Prompt + L.Input + " Error: " + ex.Message, true);
                return false;
            }
        }

        static void Render(MathParser mp, List<Line> Lines, bool Reset)
        {
            if (Reset)
                mp.ClearCustomUnits();

            for (int i = 0; i < Lines.Count; i++)
            {
                var LineNo = (i + 1).ToString().PadLeft(3) + Prompt;
                if (Reset)
                {
                    Line L = Lines[i];
                    Calculate(mp, LineNo, ref L);
                    Lines[i] = L;
                }
                else
                    Console.WriteLine(LineNo + Lines[i].Output);

            }
        }

        static string GetVariables(string Prompt, string Input)
        {
            var i = 0;
            while (i >= 0)
            {
                i = Input.IndexOf('?');
                if (i >= 0)
                {
                    Console.Write(Prompt + Input[..i].Replace("\'", string.Empty));
                    var Variable = Console.ReadLine();
                    Input = Input[..i] + Variable + Input[(i + 1)..];
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                }
            }
            return Input;
        }

        static string Open(string Title, string Prompt, List<Line> Lines)
        {
            var FilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cpc";
            if (!Directory.Exists(FilePath))
            {
                WriteError(Prompt + "OPEN There are no saved problems.\r\n", false);
                return Title;
            }
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(Prompt + "OPEN Problem title: ");
            var NewTitle = Console.ReadLine();
            var FileName = FilePath + "\\" + NewTitle + ".cpc";
            if (File.Exists(FileName))
            {
                Lines.Clear();
                using StreamReader sr = new(FileName);
                while (!sr.EndOfStream)
                    Lines.Add(new Line(sr.ReadLine()));

                return NewTitle;
            }
            else
            {
                WriteError($"{Prompt}Problem \"{Title}\" does not exits.", true);
                return Title;
            }
        }

        static string Save(string Title, string Prompt, List<Line> Lines)
        {
            var FilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cpc";
            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Prompt += "SAVE Problem title";
            if (Title.Length > 0 )
                Prompt += $" ({Title}): ";
            else
                Prompt += ": ";
            Console.Write(Prompt);
            var NewTitle = Console.ReadLine();
            if (NewTitle.Length == 0)
                NewTitle = Title;

            if (NewTitle.Length > 0)
            {
                var FileName = FilePath + "\\" + NewTitle + ".cpc";
                using StreamWriter sw = new(FileName);
                foreach (Line L in Lines)
                    sw.WriteLine(L.Input);
            }
            return NewTitle;
        }

        static void List(string Prompt)
        {
            string FilePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\cpc";
            if (!Directory.Exists(FilePath))
            {
                WriteError(Prompt + "There are no saved problems.", true);
                return;
            }
            List<string> Lines = Directory.EnumerateFiles(FilePath).ToList();
            foreach (string s in Lines)
                Console.WriteLine(Path.GetFileNameWithoutExtension(s));

            Console.WriteLine();
        }

        private static void WriteError(string message, bool line)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (line)
                Console.WriteLine(message);
            else
                Console.Write(message);

            Console.ResetColor();
        }
        private static bool Execute(string fileName, string args = "")
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
                Console.WriteLine("Loading the settings file...");
                var result = proc.Start();
                proc.WaitForExit();
                return result;
            }
            catch (Exception Ex)
            {
                WriteError(Ex.Message, true);
                return false;
            }
        }
    }
}
