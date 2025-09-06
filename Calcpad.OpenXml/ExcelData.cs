using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Calcpad.OpenXml
{
    public static class ExcelData
    {
        private const string defaultSheet = "Sheet1";
        private struct CellRef
        {
            internal int Row;
            internal int Col;
            internal CellRef(ReadOnlySpan<char> cellRefString)
            {
                if (!cellRefString.IsEmpty)
                {
                    var index = GetColRowSepIndex(cellRefString);
                    if (index < cellRefString.Length)
                        Row = int.Parse(cellRefString[index..]);

                    if (index > 0)
                        Col = GetColNumber(cellRefString[..index]);
                }
            }

            private static int GetColNumber(ReadOnlySpan<char> colName)
            {
                int sum = 0;
                for (int i = 0; i < colName.Length; i++)
                {
                    sum *= 26;
                    sum += char.ToUpperInvariant(colName[i]) - 'A' + 1;
                }
                return sum;
            }

            public override string ToString() =>
                string.Concat(GetColName(Col), Row.ToString());

            private static string GetColName(int colNumber)
            {
                var colName = string.Empty;
                int A = 'A';
                while (colNumber > 0)
                {
                    int modulo = (colNumber - 1) % 26;
                    colName = Convert.ToChar(A + modulo) + colName;
                    colNumber = (colNumber - modulo) / 26;
                }
                return colName;
            }

            private static int GetColRowSepIndex(ReadOnlySpan<char> cellRef)
            {
                int index = 0, len = cellRef.Length;
                while (index < len && !char.IsDigit(cellRef[index]))
                    ++index;

                return index;
            }

            internal static int GetColNumberFromCellRef(ReadOnlySpan<char> cellRef) =>
                cellRef.IsEmpty ? 0 : GetColNumber(cellRef[..GetColRowSepIndex(cellRef)]);
        }

        private struct CellRange
        {
            internal CellRef Start;
            internal CellRef End;
            internal int RowCount => End.Row - Start.Row + 1;
            internal int ColCount => End.Col - Start.Col + 1;

            internal CellRange(ReadOnlySpan<char> start, ReadOnlySpan<char> end)
            {
                Start = new(start);
                End = new(end);
            }

            public override string ToString() => $"{Start}:{End}";

            internal void Normalize(int minRow, int minCol, int maxRow, int maxCol)
            {
                Start.Row = Math.Clamp(Start.Row, minRow, maxRow);
                Start.Col = Math.Clamp(Start.Col, minCol, maxCol);
                if (End.Row == 0)
                    End.Row = maxRow;
                else
                    End.Row = Math.Clamp(End.Row, minRow, maxRow);

                if (End.Col == 0)
                    End.Col = maxCol;
                else
                    End.Col = Math.Clamp(End.Col, minCol, maxCol);

                if (Start.Row > End.Row)
                    (Start.Row, End.Row) = (End.Row, Start.Row);

                if (Start.Col > End.Col)
                    (Start.Col, End.Col) = (End.Col, Start.Col);
            }

        }

        public static string[][] Read(string filepath, string sheetName, string rangeStart, string rangeEnd)
        {
            using SpreadsheetDocument document = SpreadsheetDocument.Open(filepath, false); // Open in read-only mode
            WorkbookPart wbPart = document.WorkbookPart ?? 
                throw new InvalidOperationException("The Excel workbook is missing or not initialized.");
            
            WorksheetPart wsPart = null;
            Sheet sheet = null;
            if (!string.IsNullOrEmpty(sheetName))
                sheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == sheetName) ??
                    throw new InvalidOperationException($"Worksheet \"{sheetName}\" not found.");
            else
                sheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault() ?? 
                    throw new InvalidOperationException("This Excel workbook doesn not contain worksheets.");

            var sheetID = sheet?.Id?.Value ?? string.Empty;
            wsPart = (WorksheetPart)wbPart.GetPartById(sheetID);
            var range = new CellRange(rangeStart, rangeEnd);
            var maxRow = (int)wsPart.Worksheet.Descendants<Row>().LastOrDefault()?.RowIndex.Value;
            var maxCol = wsPart.Worksheet.Descendants<Cell>().Max(c => CellRef.GetColNumberFromCellRef(c.CellReference?.Value));
            range.Normalize(1, 1, maxRow, maxCol);
            var rowCount = range.RowCount;
            var colCount = range.ColCount;
            if (rowCount < 0 || colCount < 0)
                return null;

            string[][] data = new string[rowCount][];
            var current = range.Start;
            for (int i = 0; i < rowCount; ++i)
            {
                var rowData = new string[colCount];
                current.Col = range.Start.Col;
                for (int j = 0; j < colCount; ++j)
                {
                    rowData[j] = GetCellValue(wbPart, wsPart, current.ToString());
                    ++current.Col;
                }
                data[i] = rowData;
                ++current.Row;
            }
            return data;
        }

        private static string GetCellValue(WorkbookPart wbPart, WorksheetPart wsPart, string cellRef)
        {
            Cell cell = wsPart.Worksheet.Descendants<Cell>().FirstOrDefault(c => c.CellReference == cellRef);
            if (cell is null)
                return string.Empty;

            string value = cell.InnerText;
            if (cell.DataType?.Value == CellValues.SharedString)
            {
                var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                if (stringTable is not null)
                    return stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
            }
            else if(cell.CellFormula is not null)
            {
                value = cell.CellValue?.InnerText;
                if (!string.IsNullOrEmpty(value))
                    return value;
            }
            return value;
        }

        public static void Write(string filepath, string sheetName, string rangeStart, string rangeEnd, string[][] data, bool append)
        {
            if (!File.Exists(filepath) || !append)
                CreateSpreadsheetWorkbook(filepath, sheetName);

            using SpreadsheetDocument document = SpreadsheetDocument.Open(filepath, true);
            WorkbookPart wbPart = document.WorkbookPart ?? document.AddWorkbookPart();
            Sheet sheet = null;
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                sheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault();
                sheetName = defaultSheet;
            }
            else
                sheet = wbPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => s.Name == sheetName);

            sheet ??= InsertWorksheet(wbPart, sheetName);
            var sheetID = sheet?.Id?.Value ?? string.Empty;
            WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(sheetID));
            var range = new CellRange(rangeStart, rangeEnd);
            var maxRow = Math.Max(range.Start.Row, 1) + data.Length - 1;
            var maxCol = Math.Max(range.Start.Col, 1) + data.Max(r => r.Length) - 1;
            range.Normalize(1, 1, maxRow, maxCol);
            var rowCount = range.RowCount;
            var colCount = range.ColCount;
            var current = range.Start;
            for (int i = 0; i < rowCount; i++)
            {
                Row row = InsertRow(wsPart.Worksheet, (uint)current.Row);
                var rowData = data[i];
                var len = Math.Min(rowData.Length, colCount);
                current.Col = range.Start.Col;
                for (int j = 0; j < len; j++)
                {
                    InsertValue(row, current.ToString(), rowData[j]);
                    ++current.Col;
                }
                ++current.Row;
            }
            wsPart.Worksheet.Save();
            wbPart.Workbook.Save();
        }

        private static void CreateSpreadsheetWorkbook(string filePath, string sheetName)
        {
            using SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
            WorkbookPart wbPart = document.AddWorkbookPart();
            wbPart.Workbook = new();
            WorksheetPart wsPart = wbPart.AddNewPart<WorksheetPart>();
            wsPart.Worksheet = new(new SheetData());
            Sheets sheets = wbPart.Workbook.AppendChild(new Sheets());
            Sheet sheet = new() { 
                Id = wbPart.GetIdOfPart(wsPart), 
                SheetId = 1, 
                Name = string.IsNullOrWhiteSpace(sheetName) ? defaultSheet : sheetName,
            };
            sheets.Append(sheet);
            wbPart.Workbook.Save();
        }

        private static void InsertValue(Row row, string cellRef, string value)
        {
            Cell cell = InsertCell(row, cellRef);
            if (double.TryParse(value, CultureInfo.CurrentCulture.NumberFormat, out var d))
            {
                cell.CellValue = new CellValue(d);
                cell.DataType = CellValues.Number;
            }
            else
            {
                cell.InlineString = new InlineString { Text = new Text(value) };
                cell.DataType = CellValues.InlineString;
            }
        }
 
        static Sheet InsertWorksheet(WorkbookPart wbPart, string sheetName)
        {
            WorksheetPart wsPart = wbPart.AddNewPart<WorksheetPart>();
            wsPart.Worksheet = new(new SheetData());
            Sheets sheets = wbPart.Workbook.GetFirstChild<Sheets>() ?? 
                wbPart.Workbook.AppendChild(new Sheets());
            string relationshipId = wbPart.GetIdOfPart(wsPart);
            uint sheetId = sheets.Elements<Sheet>().Max(s => s.SheetId?.Value ?? 0) + 1;
            Sheet sheet = new() { 
                Id = relationshipId, 
                SheetId = sheetId, 
                Name = sheetName 
            };
            sheets.Append(sheet);
            return sheet;
        }

        private static Row InsertRow(Worksheet worksheet, uint rowIndex)
        {
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            Row row = sheetData.Elements<Row>().FirstOrDefault(r => r.RowIndex == rowIndex);
            if (row is null)
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }
            return row;
        }

        private static Cell InsertCell(Row row, string cellRef)
        {
            var cells = row.Elements<Cell>();
            var cell = cells.FirstOrDefault(c => c.CellReference?.Value == cellRef);
            if (cell is null)
            {
                cell = new() { CellReference = cellRef };
                var refCell = cells.FirstOrDefault(c => string.Compare(c.CellReference?.Value, cellRef, true) > 0);
                if (refCell is null)
                    row.Append(cell);
                else
                    row.InsertBefore(cell, refCell);
            }
            return cell;
        }
    }
}
