using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calcpad.OpenXml
{
    internal class DataWriter
    {
        // Write to both Excel and csv files, reference matches this syntax:
        // csv files: <filepath>, ex. data.csv or folder/data.csv
        // Excel files at first sheet <filepath>@<cell range>, ex. data.xlsx@A1:B3
        // Excel files using sheet name: <filepath>@<sheetname>!<cell range>, ex. data.xls@Sheet2!A1:B3
        // No support for writing to a table or named range at this time. This could be added but array size match validation would be required.
        public static void WriteData(string reference, ExternalDataSchema data)
        {
            string range = "";
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
                WriteCsv(filepath, data);
            }
            else if (filepath.Split('.').Last() == "xls" || filepath.Split('.').Last() == "xlsx")
            {
                if (range != "")
                {
                    ExcelFunctions.WriteExcelData(filepath, range, data);
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
        }
        static void WriteCsv(string filepath, ExternalDataSchema data)
        {
            using StreamWriter writer = new(filepath);
            // Write units to the CSV file
            if (data.Units is not null)
            {
                writer.WriteLine(string.Join(",", data.Units));
            }

            int rowNum = data.Values.GetLength(0);
            int colNum = data.Values.GetLength(1);

            // Iterate through the 2D array and write each row to the file
            for (int i = 0; i < rowNum; i++)
            {
                // Create a string to represent the current row
                string[] row = new string[colNum];

                for (int j = 0; j < colNum; j++)
                {
                    // Convert each value to a string and store it in the row
                    row[j] = Convert.ToString(data.Values[i, j]); // Format as needed (e.g., 2 decimal places)
                }

                // Write the row to the CSV file
                writer.WriteLine(string.Join(",", row));
            }
        }
    }
}
