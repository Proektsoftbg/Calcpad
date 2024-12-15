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
        public string Reference{ get; set; }

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
        public double[][] ReadExcelData(SpreadsheetDocument document, string sheetName, string range)
        {
            {
                // Get the worksheet by name
                WorksheetPart worksheetPart = document.WorkbookPart.WorksheetParts
                    .FirstOrDefault(ws => ws.Uri.OriginalString.Contains(sheetName));

                if (worksheetPart == null)
                {
                    throw new Exception($"Worksheet '{sheetName}' not found.");
                }

                // Get the sheet data (rows and cells)
                SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
                if (sheetData == null)
                {
                    throw new Exception($"No data found in sheet '{sheetName}'.");
                }

                // Get the range you want (e.g., "A1:C3")
                var cellRange = GetCellRange(range);

                // Initialize the jagged array (double[][]) based on the range dimensions
                int numRows = cellRange.RowEnd - cellRange.RowStart + 1;
                int numCols = cellRange.ColumnEnd - cellRange.ColumnStart + 1;
                double[][] data = new double[numRows][];

                for (int i = 0; i < numRows; i++)
                {
                    data[i] = new double[numCols];
                }

                // Loop through rows and columns in the specified range
                int rowIndex = 0;
                foreach (Row row in sheetData.Elements<Row>())
                {
                    // Only process rows within the specified range
                    if (rowIndex >= cellRange.RowStart && rowIndex <= cellRange.RowEnd)
                    {
                        int colIndex = 0;
                        foreach (Cell cell in row.Elements<Cell>())
                        {
                            // Get the cell column index
                            int cellColumn = GetColumnIndex(cell.CellReference);

                            // Only process cells within the specified column range
                            if (cellColumn >= cellRange.ColumnStart && cellColumn <= cellRange.ColumnEnd)
                            {
                                // Read the value of the cell
                                double cellValue = GetCellValue(cell, worksheetPart);
                                data[rowIndex - cellRange.RowStart][cellColumn - cellRange.ColumnStart] = cellValue;
                            }
                        }
                    }
                    rowIndex++;
                }

                return data;
            }
        }

        private int GetColumnIndex(string cellReference)
        {
            // Extract the column number (e.g., "A" -> 1, "B" -> 2, etc.)
            string columnName = System.Text.RegularExpressions.Regex.Replace(cellReference, @"[\d]", "");
            int columnNumber = 0;
            int power = 1;
            foreach (char c in columnName.Reverse())
            {
                columnNumber += (c - 'A' + 1) * power;
                power *= 26;
            }
            return columnNumber;
        }

        private double GetCellValue(Cell cell, WorksheetPart worksheetPart)
        {
            if (cell.CellValue == null)
                return 0; // Return 0 if the cell is empty

            string value = cell.CellValue.Text;

            // Handle different cell types
            switch (cell.DataType?.Value)
            {
                case CellValues.Number:
                    return double.Parse(value);
                case CellValues.SharedString:
                    var sharedString = worksheetPart.WorkbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>()
                        .ElementAt(int.Parse(value));
                    return double.TryParse(sharedString.Text?.Text, out double result) ? result : 0;
                case null:
                    return double.TryParse(value, out double result) ? result : 0;
                default:
                    return 0;
            }
        }

        private (int RowStart, int RowEnd, int ColumnStart, int ColumnEnd) GetCellRange(string range)
        {
            // Parse the range like "A1:C3"
            var match = System.Text.RegularExpressions.Regex.Match(range, @"([A-Z]+)(\d+):([A-Z]+)(\d+)");
            if (!match.Success)
                throw new ArgumentException("Invalid range format");

            // Get start and end coordinates
            int rowStart = int.Parse(match.Groups[2].Value) - 1; // zero-based index
            int rowEnd = int.Parse(match.Groups[4].Value) - 1;
            int colStart = GetColumnIndex(match.Groups[1].Value) - 1; // zero-based index
            int colEnd = GetColumnIndex(match.Groups[3].Value) - 1;

            return (rowStart, rowEnd, colStart, colEnd);
        }
    }
}
