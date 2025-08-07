using System;

namespace Calcpad.Core
{
    public partial class ExpressionParser
    {
        private readonly ref struct ReadWriteOptions
        {
            internal readonly ReadOnlySpan<char> Name;
            internal readonly ReadOnlySpan<char> Path;
            internal readonly ReadOnlySpan<char> Ext;
            internal readonly ReadOnlySpan<char> Sheet;
            internal readonly ReadOnlySpan<char> Start;
            internal readonly ReadOnlySpan<char> End;
            internal readonly string FullPath;
            internal readonly char Type = 'R';
            internal readonly char Separator = ',';
            internal readonly bool Append = false;
            internal readonly bool IsExcel;
            internal readonly bool IsHp;

            //#read Name from Path.Ext@Sheet!Start:End Type=T
            //#read M from filename.xlsx@Sheet1!A1:B2 type=[R|C|D|L|U|S]
            //#read M from filename.xlsx@Sheet1 (when range is missing the whole sheet is read)
            //#read M from filename.xlsm!A1:B2 (when sheet is missing the first sheet is read)
            //#read M from filename.xls@Sheet1!A1:B2 (when type is missing R is assumed)
            //#read M from filename.csv@R1C1:R2C2 type=[R|C|D|L|U|S] sep=,
            //#read M from filename.txt@R1C1:R2C2 type=[R|C|D|L|U|S] sep=  

            //#write Name to Path.Ext@Sheet!Start:End Type=T
            //#write M to filename.xlsx@Sheet1!A1:B2 type=[Y|N]
            //#write M to filename.xlsm@Sheet1!A1:B2 (when type is missing N is assumed)
            //#write M to filename.xls@Sheet1!A1:B2

            //#write Name to Path.Ext@Sheet!Start:End Type=T sep=S
            //#write M to filename.csv@R1C1:R2C2 type=[Y|N] sep=,
            //#write M to filename.txt@R1C1:R2C2 type=[Y|N] sep=  

            //#append Name to Path.Ext@Sheet!Start:End Type=T sep=S
            //#append M to filename.csv@R1C1:R2C2 type=[Y|N] sep=,
            //#append M to filename.txt@R1C1:R2C2 type=[Y|N] sep=  

            internal ReadWriteOptions(ReadOnlySpan<char> s, int command)
            {
                if (command > 0)
                    Type = 'N';

                var ts = new TextSpan(s);
                var i0 = 0;
                var len = s.Length;
                Search(s, ' ');
                if (i0 == len)
                    return;

                ts.Reset(i0);
                Search(s, ' ');
                ts.ExpandTo(i0 == len ? i0 : i0 - 1);
                Name = ts.Cut();
                if (i0 == len)
                    return;

                ts.Reset(i0);
                Search(s, ' ');
                ts.ExpandTo(i0 == len ? i0 : i0 - 1);
                var keyword = ts.Cut();
                if (!keyword.Equals(command == 0 ? "from" : "to", StringComparison.OrdinalIgnoreCase))
                    throw Exceptions.InvalidSyntax(keyword.ToString());

                if (i0 == len)
                    throw Exceptions.MissingFileName();

                ts.Reset(i0);
                var i1 = len;
                while (i1 >= 0) { if (s[--i1] == '.') break; }
                ts.ExpandTo(i1);
                Path = ts.Cut();
                ++i1;
                ts.Reset(i1);
                bool HasSheet = false, hasStart = false, hasEnd = false;
                for (int i = i1; i < len; ++i)
                {
                    var c = s[i];
                    switch (c)
                    {
                        case '@':
                            HasSheet = true;
                            Ext = ts.Cut();
                            ts.Reset(i + 1);
                            break;
                        case '!':
                            hasStart = true;
                            if (HasSheet)
                                Sheet = ts.Cut();
                            else
                                Ext = ts.Cut();

                            ts.Reset(i + 1);
                            break;
                        case ':':
                            hasEnd = true;
                            Start = ts.Cut();
                            ts.Reset(i + 1);
                            break;
                        case ' ':
                            break;
                        default:
                            ts.Expand();
                            break;
                    }
                    i0 = i;
                    if (c == ' ')
                        break;
                }
                IsExcel = Ext.StartsWith("xls", StringComparison.OrdinalIgnoreCase);
                if (!IsExcel)
                    hasStart = HasSheet;

                if (hasEnd)
                    End = ts.Cut();
                else if (hasStart)
                    Start = ts.Cut();
                else if (HasSheet)
                    Sheet = ts.Cut();
                else if (Ext.IsEmpty)
                {
                    Ext = ts.Cut();
                    IsExcel = Ext.StartsWith("xls", StringComparison.OrdinalIgnoreCase);
                }
                var path = Environment.ExpandEnvironmentVariables($"{Path}.{Ext}");
                FullPath = System.IO.Path.GetFullPath(path);
                Append = command == 2;
                ++i0;
                for (int i = 0; i < 2; ++i)
                {
                    if (i0 == len)
                        return;
                    ts.Reset(i0);
                    Search(s, '=');
                    ts.ExpandTo(i0 - 1);
                    keyword = ts.Cut();
                    ts.Reset(i0);
                    Search(s, ' ');
                    ts.ExpandTo(i0 == len ? i0 : i0 - 1);
                    var option = ts.Cut();
                    if (keyword.Length > 0)
                    {
                        if (option.IsEmpty)
                            throw Exceptions.InvalidSyntax(keyword.ToString());

                        if (keyword.Equals("type", StringComparison.OrdinalIgnoreCase))
                        {
                            if (option.Length == 4)
                            {
                                if (option.EndsWith("_hp", StringComparison.OrdinalIgnoreCase))
                                    IsHp = true;
                                else
                                    throw Exceptions.InvalidSyntax(option.ToString());
                            }
                            else
                            {
                                IsHp = false;
                                if (option.Length != 1)
                                    throw Exceptions.InvalidSyntax(option.ToString());
                            }
                            Type = option[0];
                        }
                        else if (keyword.Equals("sep", StringComparison.OrdinalIgnoreCase))
                        {
                            if (option.Length != 3 || option[0] != '\'' || option[2] != '\'')
                                throw Exceptions.InvalidSyntax(option.ToString());

                            Separator = option[1];
                        }
                        else
                            throw Exceptions.InvalidSyntax(keyword.ToString());
                    }
                }
                void Search(ReadOnlySpan<char> s, char c)
                {
                    while (i0 < len) { if (s[i0++] == c) break; }
                }
            }
        }
    }
}
