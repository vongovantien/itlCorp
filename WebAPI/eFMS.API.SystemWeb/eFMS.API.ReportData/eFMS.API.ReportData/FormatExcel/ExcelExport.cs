using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;

namespace eFMS.API.ReportData.FormatExcel
{
    public class ExcelExport
    {
        /// <summary>Excel Worksheet</summary>
        public ExcelWorksheet Worksheet { get; set; }
        /// <summary>Excel Package</summary>
        public ExcelPackage PackageExcel { get; set; }

        /// <summary>Start of row in excel</summary>
        private static int StartRow { get; set; }
        /// <summary>End of row in excel</summary>
        private static int EndRow { get; set; }
        /// <summary>Start of column in excel</summary>
        private static int StartCol { get; set; }
        /// <summary>End of column in excel</summary>
        private static int EndCol { get; set; }
        /// <summary>Row of group copied</summary>
        protected ExcelRange GroupRowCopy { get; set; }
        /// <summary>Row of detail copied</summary>
        protected ExcelRange DetailRowCopy { get; set; }

        /// <summary>Start index of table</summary>
        public int StartDetailTable { get; set; }
        /// <summary>Values of group copied</summary>
        private object _groupRowValue;
        /// <summary>Values of detail copied</summary>
        private object _detailRowValue;

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
                PackageExcel.Dispose();
            }
        }

        /// <summary>
        /// Set data for cell
        /// </summary>
        /// <param name="name">name of cell to set value</param>
        /// <param name="value">value set to cell</param>
        public void SetData(string name, object value, string numberFormat = null)
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
                        if (Worksheet.Cells[address].Style.WrapText)
                        {
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
                    if (!string.IsNullOrEmpty(numberFormat))
                    {
                        Worksheet.Cells[address].Style.Numberformat.Format = numberFormat;
                    }
                }
            }
        }

        /// <summary>
        /// Set Group Headers/Footers Of Table (use if have multigroup)
        /// </summary>
        public void SetGroupsTable()
        {
            if (GroupRowCopy == null)
            {
                _groupRowValue = new object();
                GroupRowCopy = Worksheet.Cells[StartDetailTable, StartCol, StartDetailTable, EndCol];
                _groupRowValue = GroupRowCopy.Value;
            }
            else
            {
                Worksheet.InsertRow(StartDetailTable, 1);
                GroupRowCopy.Copy(Worksheet.Cells[StartDetailTable, StartCol, StartDetailTable, EndCol]);
                Worksheet.Cells[StartDetailTable, StartCol, StartDetailTable, EndCol].Value = _groupRowValue;
            }
            StartRow = StartDetailTable;
            StartDetailTable++;
        }

        /// <summary>
        /// Set detail data for table
        /// </summary>
        public void SetDataTable()
        {
            if (DetailRowCopy == null)
            {
                _detailRowValue = new object();
                DetailRowCopy = Worksheet.Cells[StartDetailTable, StartCol, StartDetailTable, EndCol];
                _detailRowValue = DetailRowCopy.Value;
            }
            else
            {
                Worksheet.InsertRow(StartDetailTable, 1);
                DetailRowCopy.Copy(Worksheet.Cells[StartDetailTable, StartCol, StartDetailTable, EndCol]);
                Worksheet.Cells[StartDetailTable, StartCol, StartDetailTable, EndCol].Value = _detailRowValue;
            }
            StartRow = StartDetailTable;
            StartDetailTable++;
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
