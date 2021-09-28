using OfficeOpenXml;
using System;
using System.Collections.Generic;
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

        #region -- Info of Group
        /// <summary>Number of group to copied</summary>
        public int NumberOfGroup { get; set; }
        /// <summary>Index currenct of group</summary>
        public int IndexOfGroup { get; set; }
        /// <summary>Row of group copied</summary>
        protected List<object> GroupRowCopy { get; set; }
        /// <summary>List values of group copied</summary>
        private List<object> GroupRowValue { get; set; }
        #endregion
        #region -- Infor of row detail
        /// <summary>Row of detail copied</summary>
        protected ExcelRange DetailRowCopy { get; set; }
        /// <summary>Start index of table</summary>
        public int StartDetailTable { get; set; } = 1;
        #endregion
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
                #region Init value
                Worksheet = workBook.Worksheets[0];
                StartRow = Worksheet.Dimension.Start.Row;
                EndRow = Worksheet.Dimension.End.Row;
                StartCol = Worksheet.Dimension.Start.Column;
                EndCol = Worksheet.Dimension.End.Column;
                NumberOfGroup = 1;
                IndexOfGroup = 1;
                #endregion
            }
            catch (Exception ex)
            {
                PackageExcel.Dispose();
            }
        }

        /// <summary>
        /// Set data for cell
        /// </summary>
        /// <param name="nameList">List name of cell to set value</param>
        public void SetData(Dictionary<string, object> nameList)
        {
            foreach (var _name in nameList)
            {
                var name = string.Format("{{{0}}}", _name.Key);
                var result = from cell in Worksheet.Cells[StartRow, StartCol, Worksheet.Dimension.End.Row, EndCol]
                             where cell.Value != null && cell.Value?.ToString().Contains(name) == true
                             select cell;
                if (result.Count() > 0)
                {
                    var address = result.FirstOrDefault().ToString();
                    var value = _name.Value;
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
                                for (int strIndex = 0; strIndex < splitString.Length; strIndex++)
                                {
                                    var addressVal = ExcelCellBase.TranslateFromR1C1(ExcelCellBase.TranslateToR1C1(address, -i, 0), 0, 0);
                                    if (Worksheet.Cells[addressVal].Style.WrapText)
                                    {
                                        Worksheet.Cells[addressVal].Value = string.Join("", splitString.Skip(strIndex).Take(splitString.Length - strIndex));
                                        strIndex = splitString.Length;
                                    }
                                    else
                                    {
                                        Worksheet.Cells[addressVal].Value = splitString[strIndex];
                                    }
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
        }

        /// <summary>
        /// Set string format for cell
        /// </summary>
        /// <param name="nameList"></param>
        /// <param name="numberFormat"></param>
        public void SetFormatCell(List<string> nameList, string numberFormat)
        {
            foreach (var _name in nameList)
            {
                var name = string.Format("{{{0}}}", _name);
                var result = from cell in Worksheet.Cells[StartRow, StartCol, Worksheet.Dimension.End.Row, EndCol]
                             where cell.Value != null && cell.Value?.ToString().Contains(name) == true
                             select cell;
                if (result.Count() > 0)
                {
                    var address = result.FirstOrDefault().ToString();
                    Worksheet.Cells[address].Style.Numberformat.Format = numberFormat;
                }
            }
        }

        /// <summary>
        /// Set formula for one cell
        /// </summary>
        /// <param name="nameList">Licst name of cell to set value</param>
        public void SetFormula(Dictionary<string, string> nameList)
        {
            foreach (var _name in nameList)
            {
                var name = string.Format("{{{0}}}", _name.Key);
                var result = from cell in Worksheet.Cells[StartRow, StartCol, Worksheet.Dimension.End.Row, EndCol]
                             where cell.Value != null && cell.Value?.ToString().Contains(name) == true
                             select cell;
                if (result.Count() > 0)
                {
                    var address = result.FirstOrDefault()?.ToString();
                    if (address != null)
                    {
                        var _formular = _name.Value;
                        Worksheet.Cells[address].Formula = _formular;
                    }
                }
            }
        }

        /// <summary>
        /// Check if exist key on excel
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsExistName(string name)
        {
            name = string.Format("{{{0}}}", name);
            var result = from cell in Worksheet.Cells[StartRow, StartCol, Worksheet.Dimension.End.Row, EndCol]
                         where cell.Value != null && cell.Value?.ToString().Contains(name) == true
                         select cell;
            if (result.Count() > 0)
            {
                var address = result.FirstOrDefault();
                if (address != null)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get address of cell by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public CellAddress AddressOfKey(string name)
        {
            name = string.Format("{{{0}}}", name);
            var result = from cell in Worksheet.Cells[StartRow, StartCol, Worksheet.Dimension.End.Row, EndCol]
                         where cell.Value != null && cell.Value?.ToString().Contains(name) == true
                         select cell;
            if (result.Count() > 0)
            {
                var address = result.FirstOrDefault();
                if (address != null)
                {
                    var _cell = new CellAddress();
                    _cell.Address = address.ToString();
                    _cell.Row = address.Start.Row;
                    _cell.Column = address.Start.Column;
                    _cell.ColumnLetter = ExcelCellAddress.GetColumnLetter(address.Start.Column);
                    return _cell;
                }
            }
            return null;
        }

        /// <summary>
        /// Set Group Headers/Footers Of Table (use if have multigroup)
        /// </summary>
        public void SetGroupsTable()
        {
            if (GroupRowCopy == null)
            {
                GroupRowCopy = new List<object>();
                GroupRowValue = new List<object>();
                for (int i = 0; i < NumberOfGroup; i++)
                {
                    GroupRowCopy.Add(null);
                    GroupRowValue.Add(null);
                }
            }
            if (GroupRowCopy[IndexOfGroup - 1] == null)
            {
                var groupExcel = Worksheet.Cells[StartDetailTable, StartCol, StartDetailTable, EndCol];
                GroupRowCopy[IndexOfGroup - 1] = (groupExcel);
                GroupRowValue[IndexOfGroup - 1] = ((ExcelRange)GroupRowCopy[IndexOfGroup - 1]).Value;
            }
            else
            {
                Worksheet.InsertRow(StartDetailTable, 1);
                ((ExcelRange)GroupRowCopy[IndexOfGroup - 1]).Copy(Worksheet.Cells[StartDetailTable, StartCol, StartDetailTable, EndCol]);
                Worksheet.Cells[StartDetailTable, StartCol, StartDetailTable, EndCol].Value = GroupRowValue[IndexOfGroup - 1];
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
        /// Delete row in excel file
        /// </summary>
        /// <param name="_row">row to delete</param>
        public void DeleteRow(int _row)
        {
            Worksheet.DeleteRow(_row);
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

    /// <summary>
    /// Cell Address Class
    /// </summary>
    public class CellAddress
    {
        /// <summary> Address of cell </summary>
        public string Address { get; set; }
        /// <summary> Row address of cell </summary>
        public int Row { get; set; }
        /// <summary> Column address of cell </summary>
        public int Column { get; set; }
        /// <summary> Column Letter address of cell </summary>
        public string ColumnLetter { get; set; }
    }
}
