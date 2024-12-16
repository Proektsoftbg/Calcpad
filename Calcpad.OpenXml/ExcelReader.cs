using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Calcpad.OpenXml
{
    public class ExcelReader
    {
        public string FilePath { get; set; }
        public string Reference { get; set; }

        public void GetData()
        {
            try
            {
                // Check if the file exists
                if (!File.Exists(FilePath))
                {
                    throw new FileNotFoundException($"The file at {FilePath} does not exist.");
                }
                string range = "";

                using SpreadsheetDocument document = SpreadsheetDocument.Open(FilePath, false); // Open in read-only mode

                List<string> tableNames = GetTableNames(document);

                if (tableNames.Contains(Reference))
                {
                    range = GetTableRangeByName(document, Reference);
                }
                else if ()

            }
            catch (FileNotFoundException fnfEx)
            {
                Console.WriteLine(fnfEx.Message);
            }
            catch (UnauthorizedAccessException uaeEx)
            {
                Console.WriteLine("Access to the file is denied. " + uaeEx.Message);
            }
            catch (IOException ioEx)
            {
                Console.WriteLine("An error occurred while accessing the file. " + ioEx.Message);
            }
            catch (OpenXmlPackageException openXmlEx)
            {
                Console.WriteLine("The file is not a valid Excel file or it is corrupted. " + openXmlEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An unexpected error occurred: " + ex.Message);
            }
        }

        private static List<string> GetTableNames(SpreadsheetDocument document)
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
        private static string GetTableRangeByName(SpreadsheetDocument document, string tableName)
        {
            // Get the WorkbookPart from the document
            WorkbookPart workbookPart = document.WorkbookPart;
            var table = workbookPart.Workbook.Descendants<Table>()
                .FirstOrDefault(table => table.Name?.Value == tableName);

            if (table != null)
            {
                // Return the reference (range) of the table
                return table.Reference.Value;
            }
            else
            {
                throw new Exception($"Table '{tableName}' not found.");
            }
        }

        // Reads excel data excluding headers and the index 
        public static string[][] ReadExcelData(SpreadsheetDocument document, string sheetName, string range)
        {
            {
                // Check range is in the correct format
                var match = System.Text.RegularExpressions.Regex.Match(range, @"([A-Z]+)(\d+):([A-Z]+)(\d+)");
                if (!match.Success)
                    throw new ArgumentException("Invalid range format");

                // Get the worksheet by name
                WorksheetPart wsPart = document.WorkbookPart.WorksheetParts
                    .FirstOrDefault(ws => ws.Uri.OriginalString.Contains(sheetName)) ?? throw new Exception($"Worksheet '{sheetName}' not found.");

                // Get the sheet data (rows and cells)
                SheetData sheetData = wsPart.Worksheet.Elements<SheetData>().FirstOrDefault() ?? throw new Exception($"No data found in sheet '{sheetName}'.");
                IEnumerable<Row> Rows = sheetData.Elements<Row>();
                IEnumerable<Column> Columns = sheetData.Elements<Column>();
                var RowLength = Rows.Count();
                var ColumnLength = Columns.Count();

                // Initialize the jagged array (double[][]) based on the range dimensions
                string[][] data = new string[RowLength][];
                int start_row = GetRowNumberFromRange(range.Split(":")[0]);
                int start_col = GetColNumberFromRange(range.Split(":")[0]);

                for (int i = 0; i < RowLength; i++)
                {
                    data[i] = new string[ColumnLength];
                }

                for (int i = 0; i < RowLength; i++)
                {
                    for (int j = 0; j < ColumnLength; j++)
                    {
                        string addressName = string.Concat(ExcelColumnNumbertoName(start_col + i), ":", Convert.ToString(start_row + j));
                        data[i][j] = GetCellValue(document, wsPart, addressName);
                    }
                }

                return data;
            }
        }
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
        private static int GetRowNumberFromRange(string cellReference)
        {
            // Extract digits from the cell reference, which represents the row number
            string rowPart = string.Empty;
            foreach (char c in cellReference)
            {
                if (Char.IsDigit(c))
                {
                    rowPart += c;
                }
            }
            return int.Parse(rowPart);
        }

        private static int GetColNumberFromRange(string cellReference)
        {
            // Extract digits from the cell reference, which represents the row number
            string colPart = string.Empty;
            foreach (char c in cellReference)
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
            // Use its Worksheet property to get a reference to the cell 
            // whose address matches the address you supplied.
            Cell cell = wsPart.Worksheet?.Descendants<Cell>()?.Where(c => c.CellReference == addressName).FirstOrDefault();
            // If the cell does not exist, return an empty string.
            if (cell is null || cell.InnerText.Length < 0)
            {
                return string.Empty;
            }
            value = cell.InnerText;
            // If the cell represents an integer number, you are done. 
            // For dates, this code returns the serialized value that 
            // represents the date. The code handles strings and 
            // Booleans individually. For shared strings, the code 
            // looks up the corresponding value in the shared string 
            // table. For Booleans, the code converts the value into 
            // the words TRUE or FALSE.
            if (cell.DataType is not null)
            {
                if (cell.DataType.Value == CellValues.SharedString)
                {
                    // For shared strings, look up the value in the
                    // shared strings table.
                    var stringTable = document.WorkbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                    // If the shared string table is missing, something 
                    // is wrong. Return the index that is in
                    // the cell. Otherwise, look up the correct text in 
                    // the table.
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
    }
}
