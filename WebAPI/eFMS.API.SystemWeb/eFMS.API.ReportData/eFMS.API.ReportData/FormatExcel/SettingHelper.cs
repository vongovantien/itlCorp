using eFMS.API.ReportData.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace eFMS.API.ReportData.FormatExcel
{
    public class SettingHelper
    {
        public Stream GenerateUnlockRequestExcel(List<UnlockRequestExport> unlockRequests, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Unlock Request");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BiddingDataUnlockRequest(workSheet, unlockRequests);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void SetWidthColumnExcelUnlockRequestExport(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 8; //Cột A
            workSheet.Column(2).Width = 18; //Cột B
            workSheet.Column(3).Width = 25; //Cột C
            workSheet.Column(4).Width = 16; //Cột D
            workSheet.Column(5).Width = 16; //Cột E
            workSheet.Column(6).Width = 19; //Cột F
            workSheet.Column(7).Width = 18; //Cột G
            workSheet.Column(8).Width = 18; //Cột H
            workSheet.Column(9).Width = 15; //Cột I
            workSheet.Column(10).Width = 30; //Cột J
            workSheet.Column(11).Width = 30; //Cột K
        }

        private void BorderThinItem(ExcelWorksheet workSheet, int row, int column)
        {
            workSheet.Cells[row, column].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row, column].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row, column].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row, column].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row, column].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private void BiddingDataUnlockRequest(ExcelWorksheet workSheet, List<UnlockRequestExport> unlockRequests)
        {
            List<string> headers = new List<string>()
            {
                "No",
                "Object",
                "Description",
                "Reference No",
                "Request Type",
                "Change Service Date",
                "Request Date",
                "Unlock Date",
                "Requester",
                "Reason",
                "General Reason"
            };
            SetWidthColumnExcelUnlockRequestExport(workSheet);

            workSheet.Row(1).Style.Font.Bold = true;
            workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //Cố định dòng đầu tiên (Freeze Row 1 and no column)
            workSheet.View.FreezePanes(2, 1);

            for (int i = 0; i < headers.Count; i++)
            {
                workSheet.Cells[1, i + 1].Value = headers[i];
                BorderThinItem(workSheet, 1, i + 1);
            }

            int no = 1;
            int rowStart = 2;
            foreach (var item in unlockRequests)
            {
                workSheet.Row(rowStart).Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                workSheet.Cells[rowStart, 1].Value = no;
                workSheet.Cells[rowStart, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                workSheet.Cells[rowStart, 2].Value = item.SubjectUnlock;
                workSheet.Cells[rowStart, 3].Value = item.DescriptionUnlock;
                workSheet.Cells[rowStart, 4].Value = item.ReferenceNo;
                workSheet.Cells[rowStart, 5].Value = item.UnlockType;
                for(var j = 2; j < 6; j++)
                {
                    workSheet.Cells[rowStart, j].Style.WrapText = true;
                }

                workSheet.Cells[rowStart, 6].Value = item.ChangeServiceDate;
                workSheet.Cells[rowStart, 7].Value = item.RequestDate;
                workSheet.Cells[rowStart, 8].Value = item.UnlockDate;                
                for(var f = 6; f < 9; f++)
                {
                    workSheet.Cells[rowStart, f].Style.Numberformat.Format = "dd/MM/yyyy HH:mm";
                }

                workSheet.Cells[rowStart, 9].Value = item.Requester;
                workSheet.Cells[rowStart, 10].Value = item.ReasonDetail;
                workSheet.Cells[rowStart, 10].Style.WrapText = true;

                workSheet.Cells[rowStart, 11].Value = item.GeneralReason;

                no += 1;
                rowStart += 1;
            }
        }
    }
}
