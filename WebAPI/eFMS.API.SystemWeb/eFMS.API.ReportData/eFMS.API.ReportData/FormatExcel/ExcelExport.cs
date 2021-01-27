using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.FormatExcel
{
    public class ExcelExport
    {
        /// <summary>Excel worksheet</summary>
        public ExcelWorksheet Worksheet { get; set; }
        public ExcelPackage PackageExcel { get; set; }
        /// <summary>Start of row in excel</summary>
        public static int StartRow { get; set; }
        /// <summary>End of row in excel</summary>
        public static int EndRow { get; set; }
        /// <summary>Start of column in excel</summary>
        public static int StartCol { get; set; }
        /// <summary>End of column in excel</summary>
        public static int EndCol { get; set; }

        /// <summary>
        /// Init for excel file to export
        /// </summary>
        /// <param name="fileName"></param>
        public ExcelExport(string fileName)
        {
            Stream stream = new FileStream(fileName, FileMode.Open);
            Stream newStream = new MemoryStream();
            stream.CopyTo(newStream);
            stream.Close();
            try
            { 
                PackageExcel = new ExcelPackage(newStream);
                var workBook = PackageExcel.Workbook;
                Worksheet = workBook.Worksheets[0];
                StartRow = Worksheet.Dimension.Start.Row;
                EndRow = Worksheet.Dimension.End.Row;
                StartCol = Worksheet.Dimension.Start.Column;
                EndCol = Worksheet.Dimension.End.Column;
            }
            catch(Exception ex)
            {

            }
        }

        /// <summary>
        /// Set data for cell
        /// </summary>
        /// <param name="name">name of cell to set value</param>
        /// <param name="value">value set to cell</param>
        /// <param name="isWraptext">use if wraptext for cell</param>
        public void SetData(string name, object value, bool isWraptext = false)
        {
            name = string.Format("{{{0}}}", name);
            var result = from cell in Worksheet.Cells[StartRow, StartCol, EndRow, EndCol]
                       where cell.Value != null && cell.Value?.ToString().Contains(name) == true
                       select cell;
            if(result.Count() > 0)
            {
                var address = result.FirstOrDefault().ToString();
                if (value == null)
                {
                    Worksheet.Cells[address].Value = string.Empty;
                }
                else
                {
                    if (value.ToString().Contains("\n"))
                    {
                        if (isWraptext)
                        {
                            Worksheet.Cells[address].Style.WrapText = isWraptext;
                            Worksheet.Cells[address].Value = value;
                        }
                        else
                        {
                            var splitString = value.ToString().Split('\n');
                            int i = 0;
                            foreach (var text in splitString)
                            {
                                var addressVal = ExcelCellBase.TranslateFromR1C1(ExcelCellBase.TranslateToR1C1(address, -i, 0), 0, 0);
                                Worksheet.Cells[addressVal].Value = text;
                                i++;
                            }
                        }
                    }
                    else
                    {
                        Worksheet.Cells[address].Value = value;
                    }
                }
            }
        }

        /// <summary>
        /// Set data for table
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="rowNumber"></param>
        /// <param name="isWraptext"></param>
        public void SetDataTable(string name, object value, int rowNumber, bool isWraptext = false)
        {
            name = string.Format("{{{0}}}", name);
            var result = from cell in Worksheet.Cells[StartRow, StartCol, EndRow, EndCol]
                         where cell.Value != null && cell.Value?.ToString().Contains(name) == true
                         select cell;
            if (result.Count() > 0)
            {
                var address = ExcelCellBase.TranslateFromR1C1(ExcelCellBase.TranslateToR1C1(result.FirstOrDefault().ToString(), -rowNumber, 0), 0, 0);
                if (value == null)
                {
                    Worksheet.Cells[address].Value = string.Empty;
                }
                else
                {
                    Worksheet.Cells[address].Style.WrapText = isWraptext;
                    Worksheet.Cells[address].Value = value;
                }
            }
        }

        /// <summary>
        /// Save excel and return file stream
        /// </summary>
        /// <returns></returns>
        public Stream ExcelStream()
        {
            PackageExcel.Save();
            return PackageExcel.Stream;
        }
    }
}
