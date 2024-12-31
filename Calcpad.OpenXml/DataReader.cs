#nullable enable

using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Calcpad.OpenXml
{
    public class ExternalData
    {
        public static ExternalDataSchema GetData(string Reference)
        {
            return DataReader.GetData(Reference);
        }
    }
    public class ExternalDataSchema(double[,] values, string[]? units)
    {
        public double[,] Values {get; set;} = values;
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
            string range = "";
            string[,] textData;

            string filepath;
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

            if (filepath.Split('.').Last() == "csv")
            {
                textData = ReadCsvData(filepath);
            } 
            else if (filepath.Split('.').Last() == "xls" || filepath.Split('.').Last() == "xlsx")
            {
                if (range != "")
                {
                    textData = ExcelFunctions.ReadExcelData(filepath, range);
                }
                else
                {
                    throw new ArgumentException("Missing range reference for Excel file.");
                }
            } 
            else
            {
                throw new Exception($"'.{filepath.Split(".").Last()}' is an unsupported file extension.");
            }
            return ConvertData(textData);
        }

        // Reads a csv file, it must have rows of equal length
        internal static string[,] ReadCsvData(string filepath)
        {
            using TextFieldParser csvReader = new(filepath);
            csvReader.SetDelimiters([","]);
            csvReader.HasFieldsEnclosedInQuotes = false;
            int csvRows = File.ReadAllLines(filepath).Length;
            int csvColumns = File.ReadAllLines(filepath).First().Split(',').Length;

            string[,] data = new string[csvRows,csvColumns];
            int i = 0;
            while (!csvReader.EndOfData)
            {
                string[] fieldData = csvReader.ReadFields() ?? throw new Exception("CSV file is not correctly formatted");
                if (fieldData != null)
                {
                    for (int j = 0; j < csvColumns; j++)
                    {
                        data[i, j] = fieldData[j];
                    }
                }
                i++;
            }
            return data;
        }
        
        internal static ExternalDataSchema ConvertData(string[,] data)
        {
            bool header = false;
            string[] units = new string[data.GetLength(1)];

            for (int i = 0; i < data.GetLength(1); i++)
            {
                if (!Double.TryParse(data[0, i], out double _))
                {
                    header = true;
                    break;
                }
            }

            // If any cells in the first row contains non-numeric values, it assumes the first row is a header.
            // Further data validation is required to check if each unit string matches a valid unit. If not, it should assume it is unitless.
            
            if (header)
            {
                string match = "";
                for (int i = 0; i < data.GetLength(1); i++)
                {
                    // Checks if header cell has any text in parenthesis. If so, it will take the first match as the unit instead of the entire string.

                    match = UnitInParenthesis().Match(data[0, i]).Groups[1].Value;

                    if (match == "")
                    {
                        units[i] = data[0, i];
                    }
                    else
                    {
                        units[i] = match;
                    }
                }
            }

            int startRow = 0;
            if (header) startRow++;
            double[,] convertedData = new double[data.GetLength(0) - startRow, data.GetLength(1)];

            for (int i = 0; i < data.GetLength(0) - startRow; i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    if (double.TryParse(data[i + startRow, j], out double _))
                    {
                        convertedData[i, j] = double.Parse(data[i + startRow, j]);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid Data. Ensure all data values outside of the header are numbers.");
                    }
                }
            }
            return new ExternalDataSchema(convertedData, units);
        }

        [GeneratedRegex(@"\(([^)]*)\)")]
        private static partial Regex UnitInParenthesis();
    }
}
