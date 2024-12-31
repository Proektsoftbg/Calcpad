#nullable enable

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Table = DocumentFormat.OpenXml.Spreadsheet.Table;

namespace Calcpad.OpenXml
{
    internal partial class ExcelFunctions
    {
        internal static string[,] ReadExcelData(string filepath, string range)
        {
            using SpreadsheetDocument document = SpreadsheetDocument.Open(filepath, false); // Open in read-only mode
            WorkbookPart wbPart = document.WorkbookPart ?? throw new InvalidOperationException("The Excel workbook is missing or not initialized.");
            range = GetExcelRange(wbPart, range);
            Tuple<string, string> rangeObject = GetCellRange(range);
            string sheetName = rangeObject.Item1;
            string cellRange = rangeObject.Item2;

            // Get the worksheet by name
            WorksheetPart? wsPart = null;
            string sheetID = string.Empty;

            // Regex for sheet name being present and correctly formatted (without quotes around sheet name and including a valid multi-cell reference)
            if (ExcelRangeWithSheetName().Match(range).Success)
            {
                Sheet sheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault() ?? throw new InvalidOperationException($"Worksheet '{sheetName}' not found.");
                sheetID = sheet?.Id?.Value ?? string.Empty;
            }
            // Regex for multi-cell range without a sheet name, uses the first sheet
            else if (ExcelRangeWithoutSheetName().Match(range).Success)
            {
                Sheet firstSheet = wbPart.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault() ?? throw new InvalidOperationException("No sheets were found.");
                sheetID = firstSheet?.Id?.Value ?? string.Empty;
            }
            else
            {
                throw new ArgumentException("Invalid range format");
            }
            wsPart = (WorksheetPart)(wbPart.GetPartById(sheetID));

            // Initialize the array based on the range dimensions
            int startRow = GetRangeStart(cellRange).Item1;
            int startCol = GetRangeStart(cellRange).Item2;
            int rowNum = GetRangeSize(cellRange).Item1;
            int colNum = GetRangeSize(cellRange).Item2;
            string[,] readData = new string[rowNum, colNum];

            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {
                    string addressName = string.Concat(ExcelColumnNumbertoName(startCol + j), Convert.ToString(startRow + i));
                    readData[i, j] = GetCellValue(wbPart, wsPart, addressName);
                }
            }
            return readData;
        }

        internal static void WriteExcelData(string filepath, string range, ExternalDataSchema matrixData)
        {
            // Creates a new spreadsheet if it doesn't find one at the filepath given
            if (File.Exists(filepath) == false)
            {
                CreateSpreadsheetWorkbook(filepath);
            }

            // Open the document for editing.
            using SpreadsheetDocument document = SpreadsheetDocument.Open(filepath, true);
            WorkbookPart wbPart = document.WorkbookPart ?? document.AddWorkbookPart();
            Tuple<string, string> rangeObject = GetCellRange(range, false);
            string sheetName = rangeObject.Item1;
            string cellRange = rangeObject.Item2;
            string sheetID = string.Empty;

            if (ExcelCellWithSheetName().Match(range).Success)
            {
                Sheet sheet = wbPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault() ?? throw new InvalidOperationException($"Worksheet '{sheetName}' not found.");
                sheetID = sheet?.Id?.Value ?? string.Empty;
            }
            // Regex for multi-cell range without a sheet name, uses the first sheet
            else if (ExcelCellWithoutSheetName().Match(range).Success)
            {
                Sheet firstSheet = wbPart.Workbook.Sheets?.Elements<Sheet>().FirstOrDefault() ?? throw new InvalidOperationException("No sheets were found.");
                sheetID = firstSheet?.Id?.Value ?? string.Empty;
            }
            else
            {
                // Insert a new worksheet.
                sheetID = InsertWorksheet(wbPart, sheetName);
            }

            WorksheetPart wsPart = (WorksheetPart)(wbPart.GetPartById(sheetID));

            // Initialize the array based on the range dimensions
            int startRow = GetRangeStart(cellRange).Item1;
            int startCol = GetRangeStart(cellRange).Item2;
            int rowNum = matrixData.Values.GetLength(0);
            int colNum = matrixData.Values.GetLength(1);

            if (matrixData.Units is not null)
            {
                // Puts the string representing the units in the first row
                for (int i = 0; i < colNum; i++)
                {
                    string addressName = string.Concat(ExcelColumnNumbertoName(startCol + i), Convert.ToString(startRow));
                    InsertValue(wbPart, wsPart, addressName, matrixData.Units[i]);
                }
                // Data will be entered in the row after the units
                startRow++;
            }

            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {
                    string addressName = string.Concat(ExcelColumnNumbertoName(startCol + j), Convert.ToString(startRow + i));
                    // Known issue - writing to a table or defined name corrupts the file. This will need to be checked for and handled in the future.
                    InsertValue(wbPart, wsPart, addressName, matrixData.Values[i, j]);
                }
            }
            wbPart.Workbook.Save();

            // Dispose the document (closes).
            document.Dispose();
        }

        // -- Read Functions

        // Gets the cell value using OpenXML
        static string GetCellValue(WorkbookPart wbPart, WorksheetPart? wsPart, string addressName)
        {
            string? value = null;
            // Use its Worksheet property to get a reference to the cell whose address matches the address you supplied.

            Cell? cell = wsPart?.Worksheet?.Descendants<Cell>().FirstOrDefault(c => c.CellReference == addressName);
            // If the cell does not exist, return an empty string.
            if (cell is null || cell.InnerText.Length < 0)
            {
                return string.Empty;
            }
            value = cell.InnerText;
            // If the cell represents an integer number, you are done. For dates, this code returns the serialized value that 
            // represents the date. The code handles strings and Booleans individually. For shared strings, the code looks up the corresponding
            // value in the shared string table. For Booleans, the code converts the value into the words TRUE or FALSE.
            if (cell.DataType is not null)
            {
                if (cell.DataType.Value == CellValues.SharedString)
                {
                    // For shared strings, look up the value in the shared strings table.
                    var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    // If the shared string table is missing, something is wrong. Return the index that is in the cell.
                    // Otherwise, look up the correct text in the table.
                    if (stringTable is not null)
                    {
                        value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                    }
                }
                else if (cell.DataType.Value == CellValues.Boolean)
                {
                    value = value switch
                    {
                        "0" => "FALSE",
                        _ => "TRUE",
                    };
                }
            }
            return value;
        }

        static string GetExcelRange(WorkbookPart wbPart, string range)
        {
            Dictionary<string, string> definedNames = GetDefinedNames(wbPart);

            if (definedNames.TryGetValue(range, out string? value))
            {
                range = value;
            }
            return range;
        }

        static Dictionary<string, string> GetDefinedNames(WorkbookPart wbPart)
        {
            // Return a dictionary of defined names and Table names.
            // The pairs include the range name and a string representing the range.
            var returnValue = new Dictionary<string, string>();

            // Retrieve a reference to the defined names collection.
            DefinedNames? definedNames = wbPart.Workbook.DefinedNames;

            // If there are defined names, add them to the dictionary.
            if (definedNames is not null)
            {
                foreach (DefinedName dn in definedNames.Cast<DefinedName>())
                {
                    if (dn?.Name?.Value is not null && dn?.Text is not null)
                    {
                        returnValue.Add(dn.Name.Value, dn.Text.Replace("$", string.Empty));
                    }
                }
            }
            if (wbPart.Workbook.Sheets is not null)
            {
                foreach (Sheet sheet in wbPart.Workbook.Sheets.Elements<Sheet>())
                {
                    string sheetID = sheet?.Id?.Value ?? string.Empty;
                    string sheetName = sheet?.Name?.Value ?? string.Empty;
                    WorksheetPart? wsPart = (WorksheetPart)wbPart.GetPartById(sheetID) ?? throw new Exception("Sheet not found");

                    foreach (TableDefinitionPart tb in wsPart.TableDefinitionParts)
                    {
                        string tableName = tb?.Table?.Name?.Value ?? string.Empty;
                        string cellReference = tb?.Table?.Reference?.Value ?? string.Empty;
                        string tableReference = String.Join('!', [sheetName, cellReference]);
                        returnValue.Add(tableName, tableReference);
                    }
                }
            }
            return returnValue;
        }

        // -- Write Functions

        static void CreateSpreadsheetWorkbook(string filepath)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument document = SpreadsheetDocument.Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart wbPart = document.AddWorkbookPart();
            wbPart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart wsPart = wbPart.AddNewPart<WorksheetPart>();
            wsPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = wbPart.Workbook.AppendChild(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new() { Id = wbPart.GetIdOfPart(wsPart), SheetId = 1, Name = "Sheet1" };
            sheets.Append(sheet);

            wbPart.Workbook.Save();

            // Dispose the document.
            document.Dispose();
        }

        // Given a document name and text, 
        // inserts a new work sheet and writes the text to cell "A1" of the new worksheet.
        static void InsertValue(WorkbookPart wbPart, WorksheetPart? wsPart, string range, object value)
        {
            Cell cell;
            // Insert cell into the worksheet or return the cell at the range
            if (wsPart is not null)
            {
                cell = InsertCellInWorksheet(GetColLetterFromRange(range), (uint)GetRowNumberFromRange(range), wsPart);
            }
            else
            {
                return;
            }

            switch (value)
            {
                case string strValue:
                    // Get the SharedStringTablePart. If it does not exist, create a new one.
                    SharedStringTablePart shareStringPart;
                    if (wbPart.GetPartsOfType<SharedStringTablePart>().Any())
                    {
                        shareStringPart = wbPart.GetPartsOfType<SharedStringTablePart>().First();
                    }
                    else
                    {
                        shareStringPart = wbPart.AddNewPart<SharedStringTablePart>();
                    }

                    // Insert the text into the SharedStringTablePart.
                    int index = InsertSharedStringItem(strValue, shareStringPart);
                    // Set the value of cell A1.
                    cell.CellValue = new CellValue(index.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                    break;
                case double doubleValue:
                    cell.CellValue = new CellValue(doubleValue);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    break;
                default:
                    throw new Exception("Invalid data type for Excel insert value function");
            }

            // Save the new worksheet.
            wsPart.Worksheet.Save();
        }

        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            shareStringPart.SharedStringTable ??= new SharedStringTable();

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }

        // Given a WorkbookPart, inserts a new worksheet.
        static string InsertWorksheet(WorkbookPart wbPart, string sheetName)
        {
            // Add a new worksheet part to the workbook.
            WorksheetPart newWorksheetPart = wbPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new Worksheet(new SheetData());
            newWorksheetPart.Worksheet.Save();

            Sheets sheets = wbPart.Workbook.GetFirstChild<Sheets>() ?? wbPart.Workbook.AppendChild(new Sheets());
            string relationshipId = wbPart.GetIdOfPart(newWorksheetPart);

            // Get a unique ID for the new sheet.
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Any())
            {
                sheetId = sheets.Elements<Sheet>().Select<Sheet, uint>(s =>
                {
                    if (s.SheetId is not null && s.SheetId.HasValue)
                    {
                        return s.SheetId.Value;
                    }

                    return 0;
                }).Max() + 1;
            }

            // Append the new worksheet and associate it with the workbook.
            Sheet sheet = new() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
            sheets.Append(sheet);
            wbPart.Workbook.Save();

            return Convert.ToString(sheetId);
        }

        // Given a column name, a row index, and a WorksheetPart, inserts a cell into the worksheet. 
        // If the cell already exists, returns it. 
        static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData? sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;

            if (sheetData?.Elements<Row>().Where(r => r.RowIndex is not null && r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData!.Elements<Row>().Where(r => r.RowIndex is not null && r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Any(c => c.CellReference is not null && c.CellReference.Value == columnName + rowIndex))
            {
                return row.Elements<Cell>().Where(c => c.CellReference is not null && c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell? refCell = null;

                foreach (Cell cell in row.Elements<Cell>())
                {
                    if (string.Compare(cell.CellReference?.Value, cellReference, true) > 0)
                    {
                        refCell = cell;
                        break;
                    }
                }

                Cell newCell = new() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);

                worksheet.Save();
                return newCell;
            }
        }

        // -- Other Functions

        static Tuple<string, string> GetCellRange(string range, bool read = true)
        {
            string sheetName = "";
            string cellRange;
            // Regex for multi-cell range with a sheet name
            if (ExcelRangeWithSheetName().Match(range).Success & read)
            {
                sheetName = range.Split("!")[0];
                cellRange = range.Split("!").Last();
            }
            // Regex for multi-cell range without a sheet name, uses the first sheet
            else if (ExcelRangeWithoutSheetName().Match(range).Success & read)
            {
                cellRange = range;
            }
            // Regex for single cell range with a sheet name
            else if (ExcelCellWithSheetName().Match(range).Success)
            {
                sheetName = range.Split("!")[0];
                cellRange = range.Split("!").Last();
            }
            // Regex for a single cell range without a sheet name, uses the first sheet
            else if (ExcelCellWithoutSheetName().Match(range).Success)
            {
                cellRange = range;
            }
            else
            {
                throw new ArgumentException("Invalid range format");
            }
            return new Tuple<string, string>(sheetName, cellRange);
        }

        static Tuple<int, int> GetRangeSize(string cellRange)
        {
            return new Tuple<int, int>(
                GetRowNumberFromRange(cellRange.Split(':')[1]) - GetRowNumberFromRange(cellRange.Split(':')[0]) + 1, // number of rows
                ExcelColumnNameToNumber(GetColLetterFromRange(cellRange.Split(':')[1])) - ExcelColumnNameToNumber(GetColLetterFromRange(cellRange.Split(':')[0])) + 1 // number of columns
            );

        }

        static Tuple<int, int> GetRangeStart(string cellRange)
        {
            return new Tuple<int, int>(
                GetRowNumberFromRange(cellRange.Split(':')[0]), // start row
                ExcelColumnNameToNumber(GetColLetterFromRange(cellRange.Split(':')[0])) // start column
            );
        }

        // Converts a column index to a letter for a range
        static string ExcelColumnNumbertoName(int columnNumber)
        {
            string columnName = "";

            while (columnNumber > 0)
            {
                int modulo = (columnNumber - 1) % 26;
                columnName = Convert.ToChar('A' + modulo) + columnName;
                columnNumber = (columnNumber - modulo) / 26;
            }

            return columnName;
        }

        // Converts a column letter to a number index
        static int ExcelColumnNameToNumber(string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException(nameof(columnName));

            columnName = columnName.ToUpperInvariant();

            int sum = 0;

            for (int i = 0; i < columnName.Length; i++)
            {
                sum *= 26;
                sum += (columnName[i] - 'A' + 1);
            }

            return sum;
        }

        // Extract digits from the cell reference, which represents the row number
        static int GetRowNumberFromRange(string cellreference)
        {
            string rowPart = string.Empty;
            foreach (char c in cellreference)
            {
                if (Char.IsDigit(c))
                {
                    rowPart += c;
                }
            }
            return int.Parse(rowPart);
        }

        // Extract letters from the cell reference, which represents the column number
        static string GetColLetterFromRange(string cellreference)
        {
            string colPart = string.Empty;
            foreach (char c in cellreference)
            {
                if (Char.IsLetter(c))
                {
                    colPart += c;
                }
            }
            return colPart;
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"([^'!]+)!([A-Za-z]+[0-9]+):([A-Za-z]+[0-9]+)")]
        private static partial System.Text.RegularExpressions.Regex ExcelRangeWithSheetName();
        [System.Text.RegularExpressions.GeneratedRegex(@"([A-Z]+)(\d+):([A-Z]+)(\d+)")]
        private static partial System.Text.RegularExpressions.Regex ExcelRangeWithoutSheetName();
        [System.Text.RegularExpressions.GeneratedRegex(@"^[A-Za-z0-9 _()&-]+![A-Za-z]+\d+$")]
        private static partial System.Text.RegularExpressions.Regex ExcelCellWithSheetName();
        [System.Text.RegularExpressions.GeneratedRegex(@"^[A-Za-z]+\d+$")]
        private static partial System.Text.RegularExpressions.Regex ExcelCellWithoutSheetName();
    }
}
