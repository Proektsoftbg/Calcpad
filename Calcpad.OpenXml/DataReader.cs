#nullable enable

using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Table = DocumentFormat.OpenXml.Spreadsheet.Table;

namespace Calcpad.OpenXml
{
    public class ExternalData(string Reference)
    {
        public ExternalDataSchema Data {get; set;} = DataReader.GetData(Reference);
    }
    public class ExternalDataSchema(double[,]? values, string[]? units)
    {
        public double[,]? Values {get; set;} = values;
        public string[]? Units {get; set;} = units;
    }
    internal partial class DataReader
    {
        // Reads both Excel and csv files, reference matches this syntax:
        // csv files: <filepath>, ex. data.csv or folder/data.csv
        // Excel files at first sheet <filepath>@<cell range>, ex. data.xlsx@A1:B3
        // Excel files using sheet name: <filepath>@<sheetname>!<cell range>, ex. data.xls@Sheet2!A1:B3
        // Excel files using table name or named range: <filepath>@<Named Range or Table>, ex. data.xlsx@Table1
        internal static ExternalDataSchema GetData(string reference)
        {
            // Return Values and Units as null if an error is encountered
            ExternalDataSchema externalData = new(null, null);
            string filepath = "";
            string range = "";
            string[,]? textData = null;

            // Get filepath from reference
            if (reference.Contains('@'))
            {
                filepath = reference.Split('@')[0];
                range = reference.Split('@').Last();
            } 
            else
            {
                filepath = reference;
            }

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"The file at {filepath} does not exist.");
            }

            if (filepath.Split('.').Last() == "csv")
            {
                textData = ReadCsvData(filepath);
            } 
            else if (filepath.Split('.').Last() == "xls" || filepath.Split('.').Last() == "xlsx")
            {
                if (reference is not null)
                {
                    textData = ReadExcelData(filepath, range);
                }
                else
                {
                    throw new ArgumentException("Missing range reference for Excel file.");
                }
            } 
            else
            {
                throw new FileFormatException($"'.{filepath.Split(".").Last()}' is an unsupported file extension.");
            }
            if (textData is not null)
            {
                externalData = ConvertData(textData);
            }
            return externalData;
        }
        internal static string[,] ReadCsvData(string filepath)
        {
            // !!! Working
            return new string[10, 10]; 
        }

        internal static string[,] ReadExcelData(string filepath, string range)
        {
            using SpreadsheetDocument document = SpreadsheetDocument.Open(filepath, false); // Open in read-only mode
            WorkbookPart workbookPart = document.WorkbookPart ?? throw new InvalidOperationException("The Excel workbook is missing or not initialized.");

            string sheetName = "";
            string cellRange = "";
            // Get the worksheet by name
            WorksheetPart? wsPart = null;

            // Regex for sheet name being present and correctly formatted (without quotes around sheet name and including a valid multi-cell reference)
            if (ExcelRangeWithSheetName().Match(range).Success)
            {
                sheetName = range.Split("!")[0];
                wsPart = document.WorkbookPart.WorksheetParts.FirstOrDefault(ws => ws.Uri.OriginalString.Contains(sheetName)) ?? throw new InvalidOperationException($"Worksheet '{sheetName}' not found.");
                cellRange = range.Split("!").Last();
            }
            // Regex for multi-cell range without a sheet name, uses the first sheet
            else if (ExcelRangeWithoutSheetName().Match(range).Success)
            {
                wsPart = workbookPart.WorksheetParts.FirstOrDefault() ?? throw new InvalidOperationException($"No worksheets found.");
                cellRange = range;
            }
            else
            {
                throw new ArgumentException("Invalid range format");
            }

            // Get the sheet data (rows and cells)
            SheetData sheetData = wsPart.Worksheet.Elements<SheetData>().FirstOrDefault() ?? throw new InvalidOperationException($"No data found in sheet '{sheetName}'.");
            IEnumerable<Row> Rows = sheetData.Elements<Row>();
            IEnumerable<Column> Columns = sheetData.Elements<Column>();
            int RowLength = Rows.Count();
            int ColumnLength = Columns.Count();

            // Initialize the array based on the range dimensions
            string[,] data = new string[RowLength, ColumnLength];
            int start_row = GetRowNumberFromRange(cellRange.Split(':')[0]);
            int start_col = GetColNumberFromRange(cellRange.Split(':')[0]);

            for (int i = 0; i < RowLength; i++)
            {
                for (int j = 0; j < ColumnLength; j++)
                {
                    string addressName = string.Concat(ExcelColumnNumbertoName(start_col + i), ":", Convert.ToString(start_row + j));
                    data[i, j] = GetCellValue(document, wsPart, addressName);
                }
            }
            return data;
        }

        internal static string GetExcelRange(SpreadsheetDocument document, string reference)
        {
            List<string> tableNames = GetTableNames(document);
            string range = "";

            if (tableNames.Contains(reference))
            {
                range = GetTableRangeByName(document, reference);
            }
            else
            {

            }
            return range;
        }

        internal static List<string> GetTableNames(SpreadsheetDocument document)
        {
            // Get the WorkbookPart from the document
            WorkbookPart workbookPart = document.WorkbookPart;
            var tables = workbookPart.Workbook.Descendants<Table>();

            // Extract and return the names of all the tables
            List<string> tableNames = tables
                .Select(table => table.Name?.Value)
                .ToList();

            return tableNames;
        }

        // Method to get the range of a table by name
        internal static string? GetTableRangeByName(WorkbookPart workbookPart, string tableName)
        {
            // Get the WorkbookPart from the document
            var table = workbookPart.Workbook.Descendants<Table>()
                .FirstOrDefault(table => table.Name?.Value == tableName);

            if (table != null)
            {
                // Return the reference (range) of the table
                return table.reference.Value;
            }
            else
            {
                return null;
            }
        }

        // Reads excel data excluding headers and the index 
        
        private static string ExcelColumnNumbertoName(int columnNumber)
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

        private static int ExcelColumnNameToNumber(string columnName)
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
        internal static int GetRowNumberFromRange(string cellreference)
        {
            // Extract digits from the cell reference, which represents the row number
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

        internal static int GetColNumberFromRange(string cellreference)
        {
            // Extract digits from the cell reference, which represents the row number
            string colPart = string.Empty;
            foreach (char c in cellreference)
            {
                if (Char.IsLetter(c))
                {
                    colPart += c;
                }
            }
            return ExcelColumnNameToNumber(colPart);
        }

        static string GetCellValue(SpreadsheetDocument document, WorksheetPart wsPart, string addressName)
        {
            string? value = null;
            // Use its Worksheet property to get a reference to the cell whose address matches the address you supplied.
            Cell cell = wsPart.Worksheet?.Descendants<Cell>()?.Where(c => c.Cellreference == addressName).FirstOrDefault();
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
                    var stringTable = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
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
        internal static ExternalDataSchema ConvertData(string[,] data)
        {
            bool validData = true;
            bool header = false;
            ExternalDataSchema externalData = new(null, null);

            if (validData is false)
            {
                throw new ArgumentException("Invalid Data. Ensure all data values outside of the header are numbers.");
            }
            return externalData;
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"([^'!]+)!([A-Za-z]+[0-9]+):([A-Za-z]+[0-9]+)")]
        private static partial System.Text.RegularExpressions.Regex ExcelRangeWithSheetName();
        [System.Text.RegularExpressions.GeneratedRegex(@"([A-Z]+)(\d+):([A-Z]+)(\d+)")]
        private static partial System.Text.RegularExpressions.Regex ExcelRangeWithoutSheetName();
    }
}
