using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.FormatExcel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Font = System.Drawing.Font;

namespace eFMS.API.Report.Helpers
{
    public class ReportHelper
    {
        const string numberFormat = "#,##0.00";
        const string numberFormatKgs = "#,##0 \"KGS\"";
        const string numberFormatVND = "_-* #,##0.000_-;-* #,##0.000_-;_-* \"-\"??_-;_-@_-_(_)";
        const string numberFormats = "#,##0";
        const string CURRENCY_LOCAL = "VND";
        const string CURRENCY_USD = "USD";
        const string _formatVNDNew = "_(* #,##0_);_(* (#,##0);_(* \"-\"??_);_(@_)"; // format number VND with brackets if neg
        const string _formatNew = "_(* #,##0.000_);_(* (#,##0.000);_(* \"-\"??_);_(@_)"; // format number diff VND with brackets if neg

        public Stream GenerateAccountingPLSheetExcel(IQueryable<AccountingPlSheetExportResult> listData, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Accounting PL Sheet (" + criteria.Currency + ")");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BindingDataAccountingPLSheetExportExcel(workSheet, listData.ToList(), criteria);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {
                new LogHelper("ExportAccountingPlSheet", "" + ex.ToString());
            }
            return null;
        }

        private void SetWidthColumnExcelAccountingPLSheetExport(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 12; //Cột A
            workSheet.Column(2).Width = 16; //Cột B
            workSheet.Column(3).Width = 16; //Cột C
            workSheet.Column(4).Width = 16; //Cột D
            workSheet.Column(5).Width = 32; //Cột E
            workSheet.Column(6).Width = 16; //Cột F
            workSheet.Column(7).Width = 16; //Cột G
            workSheet.Column(8).Width = 16; //Cột H
            workSheet.Column(9).Width = 15; //Cột I
            workSheet.Column(10).Width = 16; //Cột J
            workSheet.Column(11).Width = 30; //Cột K
            workSheet.Column(12).Width = 15; //Cột L
            workSheet.Column(13).Width = 17; //Cột M
            workSheet.Column(14).Width = 14; //Cột N
            workSheet.Column(15).Width = 17; //Cột O
            workSheet.Column(16).Width = 15; //Cột P
            workSheet.Column(17).Width = 20; //Cột Q
            workSheet.Column(18).Width = 17; //Cột R
            workSheet.Column(19).Width = 17; //Cột S
            workSheet.Column(20).Width = 14; //Cột T
            workSheet.Column(21).Width = 18; //Cột U
            workSheet.Column(22).Width = 17; //Cột V
            workSheet.Column(23).Width = 20; //Cột W
            workSheet.Column(24).Width = 17; //Cột X
            workSheet.Column(25).Width = 15; //Cột Y
            workSheet.Column(26).Width = 22; //Cột Z
            workSheet.Column(27).Width = 17; //Cột AA
            workSheet.Column(28).Width = 18; //Cột AB
            workSheet.Column(29).Width = 17; //Cột AC
            workSheet.Column(30).Width = 19; //Cột AD
            workSheet.Column(31).Width = 19; //Cột AE
            workSheet.Column(32).Width = 24; //Cột AF
            workSheet.Column(33).Width = 25; //Cột AG
            workSheet.Column(34).Width = 24; //Cột AH
            workSheet.Column(35).Width = 24; //Cột AI
            workSheet.Column(36).Width = 24; //Cột AJ
            workSheet.Column(37).Width = 24; //Cột AK
            workSheet.Column(38).Width = 24; //Cột AL
            workSheet.Column(39).Width = 24; //Cột AM
            workSheet.Column(40).Width = 24; //Cột AN
        }

        /// <summary>
        /// Generate file PL sheet report
        /// </summary>
        /// <param name="listData"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public Stream BindingDataAccountingPLSheetExportExcel(IQueryable<AccountingPlSheetExportResult> listData, GeneralReportCriteria criteria)
        {
            try
            {
                FileInfo f = new FileInfo(Path.Combine(ReportConstants.PathOfTemplateExcel, ReportConstants.Accounting_PL_Sheet));
                var path = f.FullName;
                if (!File.Exists(path))
                {
                    return null;
                }
                var excel = new ExcelExport(path);
                excel.Worksheet.Name = "Accounting PL Sheet (" + criteria.Currency + ")";
                // Set logo company
                Image image = Image.FromFile(CrystalEx.GetLogoITL());
                excel.SetPicture(image, "Logo", 1, 1);
                DateTime? _fromDate = criteria.CreatedDateFrom != null ? criteria.CreatedDateFrom : criteria.ServiceDateFrom;
                DateTime? _toDate = criteria.CreatedDateTo != null ? criteria.CreatedDateTo : criteria.ServiceDateTo;
                var listKeyData = new Dictionary<string, object>();
                listKeyData.Add("DateRange", "From: " + _fromDate.Value.ToString("dd MMM, yyyy") + " to: " + _toDate.Value.ToString("dd MMM, yyyy"));
                excel.SetData(listKeyData);
                int rowStart = 9;
                excel.StartDetailTable = rowStart;
                foreach (var item in listData)
                {
                    listKeyData = new Dictionary<string, object>();
                    excel.SetDataTable();
                    listKeyData.Add("Date", item.ServiceDate?.ToString("dd/MM/yyyy"));
                    listKeyData.Add("JobNo", item.JobId);
                    listKeyData.Add("CustomerNo", item.PartnerCode);
                    listKeyData.Add("TaxCode", item.PartnerTaxCode);
                    listKeyData.Add("CustomerName", item.PartnerName);
                    listKeyData.Add("MBLNo", item.Mbl);
                    listKeyData.Add("HBLNo", item.Hbl);
                    listKeyData.Add("CustomNo", item.CustomNo);
                    listKeyData.Add("PaymentTerm", item.PaymentMethodTerm);
                    listKeyData.Add("ChargeCode", item.ChargeCode);
                    listKeyData.Add("ChargeName", item.ChargeName);
                    listKeyData.Add("Quantity", item.Quantity);
                    listKeyData.Add("UnitPrice", item.UnitPrice);
                    excel.Worksheet.Cells[rowStart, 13].Style.Numberformat.Format = criteria.Currency == "VND" ? _formatVNDNew : _formatNew;

                    listKeyData.Add("TaxInvNoRevenue", item.TaxInvNoRevenue);
                    listKeyData.Add("VoucherIdRevenue", item.VoucherIdRevenue);

                    listKeyData.Add("UsdRevenue", (item.UsdRevenue != null && item.UsdRevenue != 0) ? item.UsdRevenue : null);

                    listKeyData.Add("VndRevenue", (item.VndRevenue != null && item.VndRevenue != 0) ? item.VndRevenue : null);

                    listKeyData.Add("TaxOut", (item.TaxOut != null && item.TaxOut != 0) ? item.TaxOut : null);
                    excel.Worksheet.Cells[rowStart, 16].Style.Numberformat.Format = _formatNew;
                    excel.Worksheet.Cells[rowStart, 17, rowStart, 19].Style.Numberformat.Format = criteria.Currency == "VND" ? _formatVNDNew : _formatNew;

                    listKeyData.Add("TotalRevenue", (item.TotalRevenue != null && item.TotalRevenue != 0) ? item.TotalRevenue : null);
                    listKeyData.Add("TaxInvNoCost", item.TaxInvNoCost);
                    listKeyData.Add("VoucherIdCost", item.VoucherIdCost);

                    listKeyData.Add("UsdCost", (item.UsdCost != null && item.UsdCost != 0) ? item.UsdCost : null);

                    listKeyData.Add("VndCost", (item.VndCost != null && item.VndCost != 0) ? item.VndCost : null);

                    listKeyData.Add("TaxIn", (item.TaxIn != null && item.TaxIn != 0) ? item.TaxIn : null);

                    listKeyData.Add("TotalCost", (item.TotalCost != null && item.TotalCost != 0) ? item.TotalCost : null);

                    listKeyData.Add("TotalKickBack", (item.TotalKickBack != null && item.TotalKickBack != 0) ? item.TotalKickBack : null);

                    if (item.ExchangeRate != 0)
                    {
                        listKeyData.Add("ExchangeRate", item.ExchangeRate);
                    }
                    else
                    {
                        listKeyData.Add("ExchangeRate", null);
                    }
                    listKeyData.Add("Balance", (item.Balance != null && item.Balance != 0) ? item.Balance : null);

                    listKeyData.Add("InvNoObh", item.InvNoObh);

                    listKeyData.Add("OBHNetAmount", (item.AmountObh != null && item.AmountObh != 0) ? item.OBHNetAmount : null);
                    listKeyData.Add("AmountObh", (item.AmountObh != null && item.AmountObh != 0) ? item.AmountObh : null);
                    excel.Worksheet.Cells[rowStart, 22].Style.Numberformat.Format = _formatNew;
                    excel.Worksheet.Cells[rowStart, 23, rowStart, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? _formatVNDNew : _formatNew;

                    listKeyData.Add("PaidDate", item.PaidDate?.ToString("dd/MM/yyyy"));
                    listKeyData.Add("AcVoucherNo", item.AcVoucherNo);
                    listKeyData.Add("PmVoucherNo", item.PmVoucherNo);
                    listKeyData.Add("Service", item.Service);
                    listKeyData.Add("CdNote", item.CdNote);
                    listKeyData.Add("Creator", item.Creator);
                    listKeyData.Add("SyncedFrom", item.SyncedFrom);
                    listKeyData.Add("BillNoSynced", item.BillNoSynced);
                    listKeyData.Add("PaySyncedFrom", item.PaySyncedFrom);
                    listKeyData.Add("PayBillNoSynced", item.PayBillNoSynced);
                    listKeyData.Add("VatPartnerName", item.VatPartnerName);
                    excel.SetData(listKeyData);
                    rowStart++;
                }

                listKeyData = new Dictionary<string, object>();
                listKeyData.Add("TotalUsdRevenue", listData.Select(s => s.UsdRevenue).Sum()); // Total USD Revenue           
                listKeyData.Add("TotalVndRevenue", listData.Select(s => s.VndRevenue).Sum()); // Total VND Revenue
                listKeyData.Add("TotalTaxOut", listData.Select(s => s.TaxOut).Sum()); // Total TaxOut
                listKeyData.Add("SumTotalRevenue", listData.Select(s => s.TotalRevenue).Sum()); // Sum Total Revenue
                excel.Worksheet.Cells[rowStart, 16].Style.Numberformat.Format = _formatNew;
                excel.Worksheet.Cells[rowStart, 17, rowStart, 19].Style.Numberformat.Format = criteria.Currency == "VND" ? _formatVNDNew : _formatNew;

                listKeyData.Add("TotalUsdCost", listData.Select(s => s.UsdCost).Sum()); // Total USD Cost
                listKeyData.Add("TotalVndCost", listData.Select(s => s.VndCost).Sum()); // Total VND Cost
                listKeyData.Add("TotalTaxIn", listData.Select(s => s.TaxIn).Sum()); // Total TaxIn
                listKeyData.Add("SumTotalCost", listData.Select(s => s.TotalCost).Sum()); // Sum Total Cost
                excel.Worksheet.Cells[rowStart, 22].Style.Numberformat.Format = _formatNew;
                excel.Worksheet.Cells[rowStart, 23, rowStart, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? _formatVNDNew : _formatNew;

                listKeyData.Add("SumBalance", listData.Select(s => s.Balance).Sum()); // Sum Total Balance
                excel.Worksheet.Cells[rowStart, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? _formatVNDNew : _formatNew;
                listKeyData.Add("SumAmountObh", listData.Select(s => s.AmountObh).Sum()); // Sum Total Amount OBH
                excel.Worksheet.Cells[rowStart, 30].Style.Numberformat.Format = criteria.Currency == "VND" ? _formatVNDNew : _formatNew;

                listKeyData.Add("UserExport", "Print date: " + DateTime.Now.ToString("dd MMM, yyyy HH:ss tt") + ", by: " + listData.FirstOrDefault()?.UserExport);
                excel.SetData(listKeyData);

                return excel.ExcelStream();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void BindingDataAccountingPLSheetExportExcel(ExcelWorksheet workSheet, List<AccountingPlSheetExportResult> listData, GeneralReportCriteria criteria)
        {
            SetWidthColumnExcelAccountingPLSheetExport(workSheet);
            List<string> headers = new List<string> {
               "INDO TRANS LOGISTICS CORPORATION", //0
               "52-54-56 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
               "JOB COSTING SUMMARY REPORT", //2
               "Date", //3
               "Job No.", //4
               "Code Cus", //5
               "Taxcode", //6
               "Customer", //7
               "M-B/L", //8
               "H-B/L", //9
               "Customs No.", //10
               "P/M Term", //11
               "Charge Code", //12
               "Description", //13
               "REVENUE", //14
               "TAX Inv.No",//15
               "USD", //16
               "VND", //17
               "Tax Out", //18
               "Total", //19
               "COST", //20
               "Voucher No.", //21
               "TAX In", //22
               "Com.", //23
               "Ex. Rate", //24
               "Balance", //25
               "Payment on behalf", //26
               "Inv.No", //27
               "Net Amount", //28
               "Amount", //29
               "Paid Date", //30
               "A/C Voucher No.", //31
               "P/M Voucher No.", //32
               "Service" ,//33
               "Cd Note", //34,
               "Creator", //35,
               "Synced", //36,
               "Billing No", //37
               "Pay Synced", //38,
               "Pay Billing No", //39
               "Vat Partner" //40

            };

            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 2, column B
                excelImage.SetPosition(1, 0, 1, 0);
            }

            workSheet.Cells["AA1:AF1"].Merge = true;
            workSheet.Cells["AA1"].Value = headers[0];
            workSheet.Cells["AA1"].Style.Font.Bold = true;
            workSheet.Cells["AA1"].Style.Font.Italic = true;

            workSheet.Row(2).Height = 64.5;
            workSheet.Cells["AA2:AF2"].Merge = true;
            workSheet.Cells["AA2:AF2"].Style.WrapText = true;
            workSheet.Cells["AA2"].Value = headers[1];
            workSheet.Cells["AA2"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            workSheet.Cells["A4:AF4"].Merge = true;
            workSheet.Cells["A4"].Value = headers[2];
            workSheet.Cells["A4"].Style.Font.Bold = true;
            workSheet.Cells["A4"].Style.Font.Size = 13;
            workSheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            DateTime? _fromDate = criteria.CreatedDateFrom != null ? criteria.CreatedDateFrom : criteria.ServiceDateFrom;
            DateTime? _toDate = criteria.CreatedDateTo != null ? criteria.CreatedDateTo : criteria.ServiceDateTo;

            workSheet.Cells["A5:AF5"].Merge = true;
            workSheet.Cells["A5"].Value = "From: " + _fromDate.Value.ToString("dd MMM, yyyy") + " to: " + _toDate.Value.ToString("dd MMM, yyyy");
            workSheet.Cells["A5"].Style.Font.Bold = true;
            workSheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //Header table
            workSheet.Cells["A7:AN8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A7:AN8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A7:AN8"].Style.Font.Bold = true;

            workSheet.Cells["A7:A8"].Merge = true;
            workSheet.Cells["A7"].Value = headers[3]; // Date

            workSheet.Cells["B7:B8"].Merge = true;
            workSheet.Cells["B7"].Value = headers[4]; // Job No.

            workSheet.Cells["C7:C8"].Merge = true;
            workSheet.Cells["C7"].Value = headers[5]; //Code Cus

            workSheet.Cells["D7:D8"].Merge = true;
            workSheet.Cells["D7"].Value = headers[6]; //Taxcode

            workSheet.Cells["E7:E8"].Merge = true;
            workSheet.Cells["E7"].Value = headers[7]; //Customer

            workSheet.Cells["F7:F8"].Merge = true;
            workSheet.Cells["F7"].Value = headers[8]; //MBL

            workSheet.Cells["G7:G8"].Merge = true;
            workSheet.Cells["G7"].Value = headers[9]; //HBL

            workSheet.Cells["H7:H8"].Merge = true;
            workSheet.Cells["H7"].Value = headers[10]; //Customs No.

            workSheet.Cells["I7:I8"].Merge = true;
            workSheet.Cells["I7"].Value = headers[11]; //P/M Term

            workSheet.Cells["J7:J8"].Merge = true;
            workSheet.Cells["J7"].Value = headers[12]; //Charge Code

            workSheet.Cells["K7:K8"].Merge = true;
            workSheet.Cells["K7"].Value = headers[13]; //Description

            workSheet.Cells["L7:P7"].Merge = true;
            workSheet.Cells["L7"].Value = headers[14]; //REVENUE
            workSheet.Cells["L8"].Value = headers[15]; //TAX Inv.No (Revenue)
            workSheet.Cells["M8"].Value = headers[21]; ////Voucher No (Revenue).
            workSheet.Cells["N8"].Value = headers[16]; //USD (Revenue)
            workSheet.Cells["O8"].Value = headers[17]; //VND (Revenue)
            workSheet.Cells["P8"].Value = headers[18]; //TAX Out
            workSheet.Cells["Q8"].Value = headers[19]; //Total (Revenue)

            workSheet.Cells["R7:W7"].Merge = true;
            workSheet.Cells["R7"].Value = headers[20]; //COST
            workSheet.Cells["R8"].Value = headers[15]; //TAX Inv.No (Cost)
            workSheet.Cells["S8"].Value = headers[21]; //Voucher No (Cost).
            workSheet.Cells["T8"].Value = headers[16]; //USD (Cost)
            workSheet.Cells["U8"].Value = headers[17]; //VND (Cost)
            workSheet.Cells["V8"].Value = headers[22]; //TAX In
            workSheet.Cells["W8"].Value = headers[19]; //Total (Cost)

            workSheet.Cells["X7:X8"].Merge = true;
            workSheet.Cells["X7"].Value = headers[23]; //Com.

            workSheet.Cells["Y7:Y8"].Merge = true;
            workSheet.Cells["Y7"].Value = headers[24]; //Ex Rate

            workSheet.Cells["Z7:Z8"].Merge = true;
            workSheet.Cells["Z7"].Value = headers[25]; //Balance

            workSheet.Cells["AA7:AC7"].Merge = true;
            workSheet.Cells["AA7"].Value = headers[26]; //Payment on Behalf
            workSheet.Cells["AA8"].Value = headers[27]; //Inv.No
            workSheet.Cells["AB8"].Value = headers[28]; //Net Amount
            workSheet.Cells["AC8"].Value = headers[29]; //Amount

            workSheet.Cells["AD7:AD8"].Merge = true;
            workSheet.Cells["AD7"].Value = headers[30]; //Paid Date

            workSheet.Cells["AE7:AE8"].Merge = true;
            workSheet.Cells["AE7"].Value = headers[31]; //A/C Voucher No.

            workSheet.Cells["AF7:AF8"].Merge = true;
            workSheet.Cells["AF7"].Value = headers[32]; //P/M Voucher No.

            workSheet.Cells["AG7:AG8"].Merge = true;
            workSheet.Cells["AG7"].Value = headers[33]; //Service

            workSheet.Cells["AH7:AH8"].Merge = true;
            workSheet.Cells["AH7"].Value = headers[34]; //CD NOTE

            workSheet.Cells["AI7:AI8"].Merge = true;
            workSheet.Cells["AI7"].Value = headers[35]; //Creator

            workSheet.Cells["AJ7:AJ8"].Merge = true;
            workSheet.Cells["AJ7"].Value = headers[36]; //Synced

            workSheet.Cells["AK7:AK8"].Merge = true;
            workSheet.Cells["AK7"].Value = headers[37]; //Billing No

            workSheet.Cells["AL7:AL8"].Merge = true;
            workSheet.Cells["AL7"].Value = headers[38]; //Pay Synced

            workSheet.Cells["AM7:AM8"].Merge = true;
            workSheet.Cells["AM7"].Value = headers[39]; //Pay Billing No

            workSheet.Cells["AN7:AN8"].Merge = true;
            workSheet.Cells["AN7"].Value = headers[40]; //Vat Parter
            //Header table

            //Cố định dòng thứ 8 (Freeze Row 8 and no column)
            workSheet.View.FreezePanes(9, 1);

            int rowStart = 9;
            for (int i = 0; i < listData.Count; i++)
            {
                workSheet.Cells[rowStart, 1].Value = listData[i].ServiceDate;
                workSheet.Cells[rowStart, 1].Style.Numberformat.Format = "dd/MM/yyyy";

                workSheet.Cells[rowStart, 2].Value = listData[i].JobId;
                workSheet.Cells[rowStart, 3].Value = listData[i].PartnerCode;
                workSheet.Cells[rowStart, 4].Value = listData[i].PartnerTaxCode;
                workSheet.Cells[rowStart, 5].Value = listData[i].PartnerName;
                workSheet.Cells[rowStart, 6].Value = listData[i].Mbl;
                workSheet.Cells[rowStart, 7].Value = listData[i].Hbl;
                workSheet.Cells[rowStart, 8].Value = listData[i].CustomNo;
                workSheet.Cells[rowStart, 9].Value = listData[i].PaymentMethodTerm;
                workSheet.Cells[rowStart, 10].Value = listData[i].ChargeCode;
                workSheet.Cells[rowStart, 11].Value = listData[i].ChargeName;

                workSheet.Cells[rowStart, 12].Value = listData[i].TaxInvNoRevenue;
                workSheet.Cells[rowStart, 13].Value = listData[i].VoucherIdRevenue;

                if (listData[i].UsdRevenue != null && listData[i].UsdRevenue != 0)
                {
                    workSheet.Cells[rowStart, 14].Value = listData[i].UsdRevenue;
                    workSheet.Cells[rowStart, 14].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                if (listData[i].VndRevenue != null && listData[i].VndRevenue != 0)
                {
                    workSheet.Cells[rowStart, 15].Value = listData[i].VndRevenue;
                    workSheet.Cells[rowStart, 15].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                if (listData[i].TaxOut != null && listData[i].TaxOut != 0)
                {
                    workSheet.Cells[rowStart, 16].Value = listData[i].TaxOut;
                    workSheet.Cells[rowStart, 16].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                if (listData[i].TotalRevenue != null && listData[i].TotalRevenue != 0)
                {
                    workSheet.Cells[rowStart, 17].Value = listData[i].TotalRevenue;
                    workSheet.Cells[rowStart, 17].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                workSheet.Cells[rowStart, 18].Value = listData[i].TaxInvNoCost;
                workSheet.Cells[rowStart, 19].Value = listData[i].VoucherIdCost;

                if (listData[i].UsdCost != null && listData[i].UsdCost != 0)
                {
                    workSheet.Cells[rowStart, 20].Value = listData[i].UsdCost;
                    workSheet.Cells[rowStart, 20].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                if (listData[i].VndCost != null && listData[i].VndCost != 0)
                {
                    workSheet.Cells[rowStart, 21].Value = listData[i].VndCost;
                    workSheet.Cells[rowStart, 21].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                if (listData[i].TaxIn != null && listData[i].TaxIn != 0)
                {
                    workSheet.Cells[rowStart, 22].Value = listData[i].TaxIn;
                    workSheet.Cells[rowStart, 22].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                if (listData[i].TotalCost != null && listData[i].TotalCost != 0)
                {
                    workSheet.Cells[rowStart, 23].Value = listData[i].TotalCost;
                    workSheet.Cells[rowStart, 23].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                if (listData[i].TotalKickBack != null && listData[i].TotalKickBack != 0)
                {
                    workSheet.Cells[rowStart, 24].Value = listData[i].TotalKickBack;
                    workSheet.Cells[rowStart, 24].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                if (listData[i].ExchangeRate != 0)
                {
                    workSheet.Cells[rowStart, 25].Value = listData[i].ExchangeRate;
                    workSheet.Cells[rowStart, 25].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                if (listData[i].Balance != null && listData[i].Balance != 0)
                {
                    workSheet.Cells[rowStart, 26].Value = listData[i].Balance;
                    workSheet.Cells[rowStart, 26].Style.Numberformat.Format = criteria.Currency == "VND" ? _formatVNDNew : _formatNew;
                }

                workSheet.Cells[rowStart, 27].Value = listData[i].InvNoObh;

                if (listData[i].AmountObh != null && listData[i].AmountObh != 0)
                {
                    workSheet.Cells[rowStart, 28].Value = listData[i].OBHNetAmount;
                    workSheet.Cells[rowStart, 29].Value = listData[i].AmountObh;

                    workSheet.Cells[rowStart, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    workSheet.Cells[rowStart, 29].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                workSheet.Cells[rowStart, 30].Value = listData[i].PaidDate;
                workSheet.Cells[rowStart, 31].Value = listData[i].AcVoucherNo;
                workSheet.Cells[rowStart, 32].Value = listData[i].PmVoucherNo;
                workSheet.Cells[rowStart, 33].Value = listData[i].Service;
                workSheet.Cells[rowStart, 34].Value = listData[i].CdNote;
                workSheet.Cells[rowStart, 35].Value = listData[i].Creator;
                workSheet.Cells[rowStart, 36].Value = listData[i].SyncedFrom;
                workSheet.Cells[rowStart, 37].Value = listData[i].BillNoSynced;
                workSheet.Cells[rowStart, 38].Value = listData[i].PaySyncedFrom;
                workSheet.Cells[rowStart, 39].Value = listData[i].PayBillNoSynced;
                workSheet.Cells[rowStart, 40].Value = listData[i].VatPartnerName;
                rowStart += 1;

            }

            workSheet.Cells[rowStart, 1, rowStart, 35].Style.Font.Bold = true;
            workSheet.Cells[rowStart, 1, rowStart, 11].Merge = true;
            workSheet.Cells[rowStart, 1, rowStart, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[rowStart, 1].Value = headers[19];

            workSheet.Cells[rowStart, 14].Value = listData.Select(s => s.UsdRevenue).Sum(); // Total USD Revenue           
            workSheet.Cells[rowStart, 15].Value = listData.Select(s => s.VndRevenue).Sum(); // Total VND Revenue
            workSheet.Cells[rowStart, 16].Value = listData.Select(s => s.TaxOut).Sum(); // Total TaxOut
            workSheet.Cells[rowStart, 17].Value = listData.Select(s => s.TotalRevenue).Sum(); // Sum Total Revenue            
            for (int i = 14; i < 18; i++)
            {
                workSheet.Cells[rowStart, i].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
            }

            workSheet.Cells[rowStart, 20].Value = listData.Select(s => s.UsdCost).Sum(); // Total USD Cost
            workSheet.Cells[rowStart, 21].Value = listData.Select(s => s.VndCost).Sum(); // Total VND Cost
            workSheet.Cells[rowStart, 22].Value = listData.Select(s => s.TaxIn).Sum(); // Total TaxIn
            workSheet.Cells[rowStart, 23].Value = listData.Select(s => s.TotalCost).Sum(); // Sum Total Cost
            for (int i = 20; i < 24; i++)
            {
                workSheet.Cells[rowStart, i].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
            }

            workSheet.Cells[rowStart, 26].Value = listData.Select(s => s.Balance).Sum(); // Sum Total Balance
            workSheet.Cells[rowStart, 26].Style.Numberformat.Format = criteria.Currency == "VND" ? _formatVNDNew : _formatNew;
            workSheet.Cells[rowStart, 28].Value = listData.Select(s => s.AmountObh).Sum(); // Sum Total Amount OBH
            workSheet.Cells[rowStart, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            workSheet.Cells[6, 1, 6, 40].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[7, 1, rowStart, 40].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[7, 1, rowStart, 40].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[rowStart + 2, 1, rowStart + 2, 32].Merge = true;
            workSheet.Cells[rowStart + 2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[rowStart + 2, 1].Style.Font.Italic = true;
            workSheet.Cells[rowStart + 2, 1].Value = "Print date: " + DateTime.Now.ToString("dd MMM, yyyy HH:ss tt") + ", by: " + listData[0].UserExport;
        }

        public Stream GenerateJobProfitAnalysisExportExcel(IQueryable<JobProfitAnalysisExportResult> listData, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Accounting PL Sheet (" + criteria.Currency + ")");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BindingDataAJobProfitAnalysisExportExcel(workSheet, listData.ToList(), criteria);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        private void BindingDataAJobProfitAnalysisExportExcel(ExcelWorksheet workSheet, List<JobProfitAnalysisExportResult> listData, GeneralReportCriteria criteria)
        {
            List<string> headers = new List<string>
            {
               "INDO TRANS LOGISTICS CORPORATION", //0
               "52-54-56 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
               "JOB PROFIT ANALYSIS", //2
               "Job No.", //3
               "Service", //4
               "MBL/MAWB", //5
               "HBL/HAWB", //6
               "ETD", //7
               "ETA", //8
               "Q'TY", //9
               "20'", //10
               "40'", //11
               "40'HC", //12
               "45'", //13
               "Cont", //14
               "GW", //15
               "CW", //16
               "CBM", //17
               "Charge Code", //18
               "Revenue", //19
               "Cost", //20
               "Job Profit", //21
            };

            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 2, column B
                excelImage.SetPosition(1, 0, 1, 0);
            }

            workSheet.Column(1).Width = 14; //Cột A
            workSheet.Column(2).Width = 14; //Cột B
            workSheet.Column(3).Width = 14; //Cột C
            workSheet.Column(4).Width = 15; //Cột D
            workSheet.Column(5).Width = 15; //Cột E
            workSheet.Column(6).Width = 13; //Cột F.
            workSheet.Column(7).Width = 13; //Cột G
            workSheet.Column(16).Width = 13; //Cột P
            workSheet.Column(17).Width = 13; //Cột P
            workSheet.Column(18).Width = 13; //Cột P
            workSheet.Column(19).Width = 13; //Cột P



            workSheet.Cells["M1:S1"].Merge = true;
            workSheet.Cells["M1"].Value = headers[0];
            workSheet.Cells["M1"].Style.Font.Bold = true;
            workSheet.Cells["M1"].Style.Font.Italic = true;

            workSheet.Row(2).Height = 64.5;
            workSheet.Cells["M2:S2"].Merge = true;
            workSheet.Cells["M2:S2"].Style.WrapText = true;
            workSheet.Cells["M2"].Value = headers[1];
            workSheet.Cells["M2"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            workSheet.Cells["A4:S4"].Merge = true;
            workSheet.Cells["A4"].Value = headers[2];
            workSheet.Cells["A4"].Style.Font.Bold = true;
            workSheet.Cells["A4"].Style.Font.Size = 13;
            workSheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            DateTime? _fromDate = criteria.CreatedDateFrom != null ? criteria.CreatedDateFrom : criteria.ServiceDateFrom;
            DateTime? _toDate = criteria.CreatedDateTo != null ? criteria.CreatedDateTo : criteria.ServiceDateTo;

            workSheet.Cells["A5:S5"].Merge = true;
            workSheet.Cells["A5"].Value = "From: " + _fromDate.Value.ToString("dd MMM, yyyy") + " to: " + _toDate.Value.ToString("dd MMM, yyyy");
            workSheet.Cells["A5"].Style.Font.Bold = true;
            workSheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A6:S6"].Style.Font.Bold = true;
            workSheet.Cells["A6:S6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int c = 3; c < 22; c++)
            {
                workSheet.Cells[6, c - 2].Value = headers[c];
            }

            //Cố định dòng thứ 6 (Freeze Row 6 and no column)
            workSheet.View.FreezePanes(7, 1);

            int rowStart = 7;
            for (int i = 0; i < listData.Count; i++)
            {
                workSheet.Cells[rowStart, 1].Value = listData[i].JobNo;
                workSheet.Cells[rowStart, 2].Value = listData[i].Service;
                workSheet.Cells[rowStart, 3].Value = listData[i].Mbl;
                workSheet.Cells[rowStart, 4].Value = listData[i].Hbl;
                workSheet.Cells[rowStart, 5].Value = listData[i].Etd;
                workSheet.Cells[rowStart, 5].Style.Numberformat.Format = "dd/MM/yyyy";
                workSheet.Cells[rowStart, 6].Value = listData[i].Eta;
                workSheet.Cells[rowStart, 6].Style.Numberformat.Format = "dd/MM/yyyy";
                workSheet.Cells[rowStart, 7].Value = listData[i].Quantity;
                workSheet.Cells[rowStart, 8].Value = listData[i].Cont20;
                workSheet.Cells[rowStart, 9].Value = listData[i].Cont40;
                workSheet.Cells[rowStart, 10].Value = listData[i].Cont40HC;
                workSheet.Cells[rowStart, 11].Value = listData[i].Cont45;
                workSheet.Cells[rowStart, 12].Value = listData[i].Cont;
                workSheet.Cells[rowStart, 13].Value = listData[i].GW;
                workSheet.Cells[rowStart, 14].Value = listData[i].CW;
                workSheet.Cells[rowStart, 15].Value = listData[i].CBM;
                workSheet.Cells[rowStart, 16].Value = listData[i].ChargeCode;
                if (listData[i].TotalRevenue != null)
                {
                    workSheet.Cells[rowStart, 17].Value = listData[i].TotalRevenue;
                    workSheet.Cells[rowStart, 17].Style.Numberformat.Format = numberFormat;
                }
                if (listData[i].TotalCost != null)
                {
                    workSheet.Cells[rowStart, 18].Value = listData[i].TotalCost;
                    workSheet.Cells[rowStart, 18].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].JobProfit != null)
                {
                    workSheet.Cells[rowStart, 19].Value = listData[i].JobProfit;
                    workSheet.Cells[rowStart, 19].Style.Numberformat.Format = numberFormat;
                }
                if (listData[i].JobNo == null)
                {
                    workSheet.Cells[rowStart, 17].Style.Font.Bold = true;
                    workSheet.Cells[rowStart, 18].Style.Font.Bold = true;
                    workSheet.Cells[rowStart, 19].Style.Font.Bold = true;
                    workSheet.Cells[rowStart, 1, rowStart, 16].Merge = true;
                    workSheet.Cells[rowStart, 1, rowStart, 19].Style.Border.Bottom.Style = ExcelBorderStyle.None;
                }

                rowStart += 1;
            }
            workSheet.Cells[rowStart, 1, rowStart, 16].Merge = true;
            workSheet.Cells[rowStart, 1, rowStart, 16].Value = "TOTAL";
            workSheet.Cells[rowStart, 1, rowStart, 16].Style.Font.Bold = true;
            workSheet.Cells[rowStart, 1, rowStart, 16].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[rowStart, 17].Value = listData.Where(x => x.JobNo != null).Select(t => t.TotalRevenue).Sum();
            workSheet.Cells[rowStart, 17].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[rowStart, 17].Style.Font.Bold = true;
            workSheet.Cells[rowStart, 18].Value = listData.Where(x => x.JobNo != null).Select(t => t.TotalCost).Sum();
            workSheet.Cells[rowStart, 18].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[rowStart, 18].Style.Font.Bold = true;
            workSheet.Cells[rowStart, 19].Value = listData.Where(x => x.JobNo != null).Select(t => t.JobProfit).Sum();
            workSheet.Cells[rowStart, 19].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[rowStart, 19].Style.Font.Bold = true;


            workSheet.Cells[6, 1, rowStart, 19].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[6, 1, rowStart, 19].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[6, 1, rowStart, 19].Style.Border.Top.Style = ExcelBorderStyle.Thin;

        }

        public Stream GenerateSummaryOfCostsIncurredExcel(IQueryable<SummaryOfCostsIncurredExportResult> lst, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Summary Of Costs");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BinddingDatalSummaryOfCostsIncurred(workSheet, criteria, lst.ToList());
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BinddingDatalSummaryOfCostsIncurred(ExcelWorksheet workSheet, GeneralReportCriteria criteria, List<SummaryOfCostsIncurredExportResult> lst)
        {
            List<string> headerTable = new List<string>()
            {
               "STT/No",
               "Mã Nhà Cung Cấp/Supplier Code",
               "Tên Nhà Cung Cấp/Supplier Name",
               "Tên hàng hóa/Commodity",
               "Cảng/Port",
               "Số hợp đồng/PO No./Contract No",
               "Số Tờ Khai/Customs Declaration No",
               "Số Vận Đơn/Bill of Lading No./AWB No.",
               "Trọng Lượng/VOLUMNE",
               "Phí dịch vụ làm hàng/Customs clearance fee",
               "Phí thu hộ/Authorized fees",
               "Tổng cộng/Total"
            };

            List<string> subheaderTable = new List<string>()
            {
               "KGS",
               "CBM",
               "Container",
               "Chi phí/Fee",
               "VAT",
               "Tổng cộng/Total"
            };

            // Tạo header
            for (int i = 0; i < headerTable.Count; i++)
            {
                //if (i == 7)
                //{
                //    workSheet.Cells[1, i + 3].Value = headerTable[i];
                //}
                if (i < 8)
                {
                    workSheet.Cells[1, i + 1].Value = headerTable[i];
                }
                //if (i > 13)
                //{
                //    workSheet.Cells[1, i + 3].Value = headerTable[i];
                //    workSheet.Cells[1, i + 3].Style.Font.Bold = true;
                //}
                //if (i > 13)
                //{
                //    workSheet.Cells[1, i + 3].Value = headerTable[i];
                //    workSheet.Cells[1, i + 3].Style.Font.Bold = true;
                //}


                workSheet.Cells[1, i + 5].Style.Font.Bold = true;
                workSheet.Cells[1, i + 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, i + 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[1, i + 1].Style.Font.Bold = true;
                workSheet.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            }
            workSheet.Cells["I1"].Value = headerTable[8];
            workSheet.Cells["L1"].Value = headerTable[9];
            workSheet.Cells["O1"].Value = headerTable[10];


            workSheet.Cells["I2"].Value = subheaderTable[0];
            workSheet.Cells["J2"].Value = subheaderTable[1];
            workSheet.Cells["K2"].Value = subheaderTable[2];

            workSheet.Cells["L2"].Value = subheaderTable[3];
            workSheet.Cells["M2"].Value = subheaderTable[4];
            workSheet.Cells["N2"].Value = subheaderTable[5];

            workSheet.Cells["O2"].Value = subheaderTable[3];
            workSheet.Cells["P2"].Value = subheaderTable[4];
            workSheet.Cells["Q2"].Value = subheaderTable[5];
            workSheet.Cells["R1"].Value = subheaderTable[5];
            workSheet.Cells["R1:R2"].Merge = true;

            workSheet.Cells["I2:Q2"].Style.Font.Bold = true;

            workSheet.Cells["I2:Q2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["I2:Q2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["R1"].Style.Font.Bold = true;

            workSheet.Cells["R1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["R1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A1:A2"].Merge = true;
            workSheet.Cells["B1:B2"].Merge = true;
            workSheet.Cells["C1:C2"].Merge = true;
            workSheet.Cells["D1:D2"].Merge = true;
            workSheet.Cells["E1:E2"].Merge = true;
            workSheet.Cells["F1:F2"].Merge = true;
            workSheet.Cells["G1:G2"].Merge = true;
            workSheet.Cells["H1:H2"].Merge = true;
            workSheet.Cells["I1:K1"].Merge = true;
            workSheet.Cells["L1:N1"].Merge = true;
            workSheet.Cells["O1:Q1"].Merge = true;

            //Cố định dòng thứ 2 (Freeze Row 2 and no column)
            workSheet.View.FreezePanes(3, 1);

            int addressStartContent = 3;
            for (int i = 0; i < lst.Count; i++)
            {
                var item = lst[i];
                workSheet.Cells[i + addressStartContent, 1].Value = i + 1;
                workSheet.Cells[i + addressStartContent, 2].Value = item.SupplierCode;
                workSheet.Cells[i + addressStartContent, 3].Value = item.SuplierName;
                workSheet.Cells[i + addressStartContent, 4].Value = item.ChargeName;
                workSheet.Cells[i + addressStartContent, 5].Value = item.POLName;
                workSheet.Cells[i + addressStartContent, 6].Value = item.PurchaseOrderNo;
                workSheet.Cells[i + addressStartContent, 7].Value = item.CustomNo;
                workSheet.Cells[i + addressStartContent, 8].Value = item.HBL;

                workSheet.Cells[i + addressStartContent, 9].Value = item.GrossWeight;
                workSheet.Cells[i + addressStartContent, 9].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[i + addressStartContent, 10].Value = item.CBM;
                workSheet.Cells[i + addressStartContent, 10].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[i + addressStartContent, 11].Value = item.PackageContainer;
                if (!item.Type.Contains("OBH"))
                {
                    workSheet.Cells[i + addressStartContent, 12].Value = item.NetAmount != null ? item.NetAmount : 0M;
                    workSheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    workSheet.Cells[i + addressStartContent, 13].Value = item.VATAmount != null ? item.VATAmount : 0M;
                    workSheet.Cells[i + addressStartContent, 14].Value = item.VATAmount.GetValueOrDefault(0M) + item.NetAmount.GetValueOrDefault(0M);
                    workSheet.Cells[i + addressStartContent, 14].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    workSheet.Cells[i + addressStartContent, 15].Value = 0M;
                    workSheet.Cells[i + addressStartContent, 16].Value = 0M;
                    workSheet.Cells[i + addressStartContent, 17].Value = 0M;
                }
                else
                {
                    workSheet.Cells[i + addressStartContent, 12].Value = 0M;
                    workSheet.Cells[i + addressStartContent, 13].Value = 0M;
                    workSheet.Cells[i + addressStartContent, 14].Value = 0M;


                    workSheet.Cells[i + addressStartContent, 15].Value = item.NetAmount != null ? item.NetAmount : 0M;
                    workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    workSheet.Cells[i + addressStartContent, 16].Value = item.VATAmount != null ? item.VATAmount : 0M;
                    workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    workSheet.Cells[i + addressStartContent, 17].Value = item.VATAmount.GetValueOrDefault(0M) + item.NetAmount.GetValueOrDefault(0M);
                    workSheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                decimal? TotalNormalCharge = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 14].Value);
                decimal? TotalOBHCharge = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 17].Value);

                workSheet.Cells[i + addressStartContent, 18].Value = TotalNormalCharge.GetValueOrDefault(0M) + TotalOBHCharge.GetValueOrDefault(0M);
                workSheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
            }


            addressStartContent = addressStartContent + lst.Count;

            workSheet.Cells[1, 1, addressStartContent, 18].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[1, 1, addressStartContent, 18].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[1, 1, addressStartContent, 18].Style.Border.Top.Style = ExcelBorderStyle.Thin;


            workSheet.Cells[addressStartContent, 1].Value = "Total"; //Total
            string addressTotal = workSheet
             .Cells[addressStartContent, 1]
             .First(c => c.Value.ToString() == "Total")
             .Start
             .Address;
            string addressTotalMerge = workSheet
             .Cells[addressStartContent, 11].Start.Address;
            string addressToMerge = addressTotal + ":" + addressTotalMerge;
            workSheet.Cells[addressToMerge].Merge = true;
            workSheet.Cells[addressToMerge].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[addressToMerge].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            string addressTotalNext = workSheet
           .Cells[addressStartContent, 12].Start.Address;

            decimal? totalNetAmountNormalCharge = 0;
            decimal? totalNetAmountOBHCharge = 0;
            decimal? totalVATNormalCharge = 0;
            decimal? totalVATOBHCharge = 0;
            decimal? totalNormalCharge = 0;
            decimal? totalOBHCharge = 0;
            decimal? totalAll = 0;

            foreach (var item in lst)
            {
                if (!item.Type.Contains("OBH"))
                {
                    totalNetAmountNormalCharge += item.NetAmount.GetValueOrDefault(0M);
                    totalVATNormalCharge += item.VATAmount.GetValueOrDefault(0M);
                    totalNormalCharge = totalNetAmountNormalCharge.GetValueOrDefault(0M) + totalVATNormalCharge.GetValueOrDefault(0M);
                }
                else
                {
                    totalNetAmountOBHCharge += item.NetAmount.GetValueOrDefault(0M);
                    totalVATOBHCharge += item.VATAmount.GetValueOrDefault(0M);
                    totalOBHCharge = totalNetAmountOBHCharge.GetValueOrDefault(0M) + totalVATOBHCharge.GetValueOrDefault(0M);
                }
            }
            totalAll = totalOBHCharge + totalNormalCharge;

            workSheet.Cells[addressTotalNext].Value = totalNetAmountNormalCharge;
            workSheet.Cells[addressTotalNext].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
            string addressTotalVat = workSheet.Cells[addressStartContent, 13].Start.Address;
            workSheet.Cells[addressTotalVat].Value = totalVATNormalCharge;
            workSheet.Cells[addressTotalVat].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalNormalCharge = workSheet.Cells[addressStartContent, 14].Start.Address;
            workSheet.Cells[addressTotalNormalCharge].Value = totalNormalCharge;
            workSheet.Cells[addressTotalNormalCharge].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressNetAmountCharge = workSheet.Cells[addressStartContent, 15].Start.Address;
            workSheet.Cells[addressNetAmountCharge].Value = totalNetAmountOBHCharge;
            workSheet.Cells[addressNetAmountCharge].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressVATChargeNext = workSheet.Cells[addressStartContent, 16].Start.Address;
            workSheet.Cells[addressVATChargeNext].Value = totalVATOBHCharge;
            workSheet.Cells[addressVATChargeNext].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalChargeNext = workSheet.Cells[addressStartContent, 17].Start.Address;
            workSheet.Cells[addressTotalChargeNext].Value = totalOBHCharge;
            workSheet.Cells[addressTotalChargeNext].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalAll = workSheet.Cells[addressStartContent, 18].Start.Address;
            workSheet.Cells[addressTotalAll].Value = totalAll;
            workSheet.Cells[addressTotalAll].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressToBold = addressTotalNext + ":" + addressTotalAll;

            workSheet.Cells[addressToBold].Style.Font.Bold = true;

            workSheet.Column(1).Width = 8; //Cột A
            workSheet.Column(2).Width = 40; //Cột B
            workSheet.Column(3).Width = 20; //Cột C
            workSheet.Column(4).Width = 20; //Cột D
            workSheet.Column(5).Width = 35; //Cột E
            workSheet.Column(6).Width = 30; //Cột F
            workSheet.Column(10).Width = 35;//Cột J
            workSheet.Column(11).Width = 20;//Cột K
            workSheet.Column(12).Width = 20;//Cột L
            workSheet.Column(13).Width = 20;//Cột M
            workSheet.Column(14).Width = 20;//Cột M
            workSheet.Column(15).Width = 20; //Cột N
            workSheet.Column(16).Width = 20;  //Cột O
            workSheet.Column(17).Width = 20;   //Cột P
            workSheet.Cells[addressTotal].Style.Font.Bold = true;
            workSheet.Cells.AutoFitColumns();

        }

        public Stream GenerateSummaryOfRevenueExcel(SummaryOfRevenueModel obj, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Summary Of Revenue");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BinddingDataSummaryOfRevenue(workSheet, criteria, obj);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception)
            {

            }
            return null;
        }

        public void BinddingDataSummaryOfRevenue(ExcelWorksheet workSheet, GeneralReportCriteria criteria, SummaryOfRevenueModel obj)
        {
            List<string> headerTable = new List<string>()
            {
               "STT/No",
               "Mã Nhà Cung Cấp/Supplier Code",
               "Tên Nhà Cung Cấp/Supplier Code",
               "Tên hàng hóa/Commodity",
               "JOB ID",
               "Cảng/Port",
               "Số lượng/Qty",
               "Đơn Vị Tính/Unit",
               "Số Hợp Đồng/PO No./Contract No.",
               "Số Tờ Khai/Customs Declaration No",
               "Số Vận Đơn/HBL No",
               "Trọng Lượng/VOLUMNE",
               "Phí dịch vụ làm hàng/Customs clearance fee",
               "Phí thu hộ/Authorized fees",
               "Tổng cộng/Total",
               "SOA",
               "Số Hóa Đơn/Invoice No",
               "Ngày Hóa Đơn/Invoice Date",
            };

            List<string> subheaderTable = new List<string>()
            {
               "KGS",
               "CBM",
               "Container",
               "Chi phí/Fee",
               "VAT",
               "Tổng cộng/Total"
            };
            // Tạo header
            for (int i = 0; i < headerTable.Count; i++)
            {
                if (i < 12)
                {
                    workSheet.Cells[1, i + 1].Value = headerTable[i];

                }
                workSheet.Cells[1, i + 7].Style.Font.Bold = true;
                workSheet.Cells[1, i + 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, i + 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[1, i + 1].Style.Font.Bold = true;
                workSheet.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[1, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            workSheet.Cells["A1:A2"].Merge = true;
            workSheet.Cells["B1:B2"].Merge = true;
            workSheet.Cells["C1:C2"].Merge = true;
            workSheet.Cells["D1:D2"].Merge = true;
            workSheet.Cells["E1:E2"].Merge = true;
            workSheet.Cells["F1:F2"].Merge = true;
            workSheet.Cells["G1:G2"].Merge = true;
            workSheet.Cells["H1:H2"].Merge = true;
            workSheet.Cells["I1:I2"].Merge = true;
            workSheet.Cells["J1:J2"].Merge = true;
            workSheet.Cells["K1:K2"].Merge = true;
            workSheet.Cells["L1:N1"].Merge = true;
            workSheet.Cells["O1:Q1"].Merge = true;
            workSheet.Cells["O1"].Value = headerTable[12];
            workSheet.Cells["R1:T1"].Merge = true;
            workSheet.Cells["R1"].Value = headerTable[13];
            workSheet.Cells["U1:U2"].Merge = true;
            workSheet.Cells["U1"].Value = headerTable[14];
            workSheet.Cells["V1:V2"].Merge = true;
            workSheet.Cells["V1"].Value = headerTable[15];
            workSheet.Cells["W1:W2"].Merge = true;
            workSheet.Cells["W1"].Value = headerTable[16];
            workSheet.Cells["X1:X2"].Merge = true;
            workSheet.Cells["X1"].Value = headerTable[17];

            workSheet.Cells["L2"].Value = subheaderTable[0];
            workSheet.Cells["M2"].Value = subheaderTable[1];
            workSheet.Cells["N2"].Value = subheaderTable[2];

            workSheet.Cells["O2"].Value = subheaderTable[3];
            workSheet.Cells["P2"].Value = subheaderTable[4];
            workSheet.Cells["Q2"].Value = subheaderTable[5];

            workSheet.Cells["R2"].Value = subheaderTable[3];
            workSheet.Cells["S2"].Value = subheaderTable[4];
            workSheet.Cells["T2"].Value = subheaderTable[5];

            workSheet.Cells["L2:T2"].Style.Font.Bold = true;

            workSheet.Cells["L2:T2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["L2:T2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            decimal? totalALLValue = 0;

            //Cố định dòng thứ 2 (Freeze Row 2 and no column)
            workSheet.View.FreezePanes(3, 1);

            int addressStartContent = 3;
            Color colFromHex = ColorTranslator.FromHtml("#eab286");
            if (obj.summaryOfRevenueExportResults != null && obj.summaryOfRevenueExportResults.Count() > 0)
            {
                for (int i = 0; i < obj.summaryOfRevenueExportResults.Count; i++)
                {

                    var item = obj.summaryOfRevenueExportResults[i];
                    workSheet.Cells[i + addressStartContent, 1].Value = i + 1;
                    workSheet.Cells[i + addressStartContent, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 1].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 2].Value = item.SupplierCode;
                    workSheet.Cells[i + addressStartContent, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    workSheet.Cells[i + addressStartContent, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 2].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 3].Value = item.SuplierName;

                    workSheet.Cells[i + addressStartContent, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 3].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 4].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 4].Value = item.ChargeName;

                    workSheet.Cells[i + addressStartContent, 5].Value = item.SummaryOfCostsIncurredExportResults.Select(t => t.JobId).FirstOrDefault();
                    workSheet.Cells[i + addressStartContent, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 5].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 6].Value = item.POLName;
                    workSheet.Cells[i + addressStartContent, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 6].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 7].Value = item.SummaryOfCostsIncurredExportResults.Select(t => t.Quantity).Sum();
                    workSheet.Cells[i + addressStartContent, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 7].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 8].Value = string.Empty;
                    workSheet.Cells[i + addressStartContent, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 8].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 9].Value = item.PurchaseOrderNo;
                    workSheet.Cells[i + addressStartContent, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 9].Style.Fill.BackgroundColor.SetColor(colFromHex);


                    workSheet.Cells[i + addressStartContent, 10].Value = item.CustomNo;
                    workSheet.Cells[i + addressStartContent, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 10].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 11].Value = item.HBL;
                    workSheet.Cells[i + addressStartContent, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 11].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 12].Value = item.GrossWeight;
                    workSheet.Cells[i + addressStartContent, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 12].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 13].Value = item.CBM;
                    workSheet.Cells[i + addressStartContent, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 13].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 14].Value = item.PackageContainer;
                    workSheet.Cells[i + addressStartContent, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 14].Style.Fill.BackgroundColor.SetColor(colFromHex);


                    workSheet.Cells[i + addressStartContent, 15].Value = item.SummaryOfCostsIncurredExportResults.Where(t => !t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 15].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 15].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                    workSheet.Cells[i + addressStartContent, 16].Value = item.SummaryOfCostsIncurredExportResults.Where(t => !t.Type.Contains("OBH")).Sum(t => t.VATAmount);
                    workSheet.Cells[i + addressStartContent, 16].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 16].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                    workSheet.Cells[i + addressStartContent, 17].Value = item.SummaryOfCostsIncurredExportResults.Where(t => !t.Type.Contains("OBH")).Sum(t => t.VATAmount) + item.SummaryOfCostsIncurredExportResults.Where(t => !t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 17].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;


                    workSheet.Cells[i + addressStartContent, 18].Value = item.SummaryOfCostsIncurredExportResults.Where(t => t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 18].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                    workSheet.Cells[i + addressStartContent, 19].Value = item.SummaryOfCostsIncurredExportResults.Where(t => t.Type.Contains("OBH")).Sum(t => t.VATAmount);
                    workSheet.Cells[i + addressStartContent, 19].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 19].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 19].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                    workSheet.Cells[i + addressStartContent, 20].Value = item.SummaryOfCostsIncurredExportResults.Where(t => t.Type.Contains("OBH")).Sum(t => t.VATAmount) + item.SummaryOfCostsIncurredExportResults.Where(t => t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 20].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 20].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 20].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                    workSheet.Cells[i + addressStartContent, 21].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 21].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 21].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                    workSheet.Cells[i + addressStartContent, 22].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 22].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 23].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 23].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 24].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 24].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 22].Value = item.SummaryOfCostsIncurredExportResults.Select(t => t.SoaNo).FirstOrDefault();
                    workSheet.Cells[i + addressStartContent, 23].Value = item.SummaryOfCostsIncurredExportResults.Select(t => t.InvoiceNo).FirstOrDefault();
                    //workSheet.Cells[i + addressStartContent, 24].Value = item.SummaryOfCostsIncurredExportResults.Select(t => t.InvoiceDate).FirstOrDefault();
                    workSheet.Cells[i + addressStartContent, 24].Value = item.SummaryOfCostsIncurredExportResults.Select(t => t.InvoiceDate).FirstOrDefault().HasValue ? item.SummaryOfCostsIncurredExportResults.Select(t => t.InvoiceDate).FirstOrDefault().Value.ToString("dd/MM/yyyy") : "";

                    decimal? TotalNormalCharge1 = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 17].Value);
                    decimal? TotalOBHCharge1 = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 20].Value);
                    workSheet.Cells[i + addressStartContent, 21].Value = TotalNormalCharge1.GetValueOrDefault(0M) + TotalOBHCharge1.GetValueOrDefault(0M);
                    workSheet.Cells[i + addressStartContent, 21].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    totalALLValue += TotalNormalCharge1.GetValueOrDefault(0M) + TotalOBHCharge1.GetValueOrDefault(0M);

                    for (int j = 0; j < item.SummaryOfCostsIncurredExportResults.Count; j++)
                    {
                        addressStartContent++;
                        var itemCharge = item.SummaryOfCostsIncurredExportResults[j];

                        workSheet.Cells[i + addressStartContent, 2].Value = item.SupplierCode;
                        workSheet.Cells[i + addressStartContent, 3].Value = itemCharge.SuplierName;
                        workSheet.Cells[i + addressStartContent, 4].Value = itemCharge.ChargeName;
                        workSheet.Cells[i + addressStartContent, 5].Value = itemCharge.JobId;
                        workSheet.Cells[i + addressStartContent, 6].Value = item.POLName;
                        workSheet.Cells[i + addressStartContent, 7].Value = itemCharge.Quantity;
                        workSheet.Cells[i + addressStartContent, 7].Style.Numberformat.Format = numberFormatVND;
                        workSheet.Cells[i + addressStartContent, 8].Value = itemCharge.Unit;
                        workSheet.Cells[i + addressStartContent, 9].Value = itemCharge.PurchaseOrderNo;
                        workSheet.Cells[i + addressStartContent, 10].Value = itemCharge.CustomNo;
                        workSheet.Cells[i + addressStartContent, 11].Value = itemCharge.HBL;

                        workSheet.Cells[i + addressStartContent, 12].Value = itemCharge.GrossWeight;
                        workSheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = numberFormat;

                        workSheet.Cells[i + addressStartContent, 13].Value = itemCharge.CBM;
                        workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat;

                        workSheet.Cells[i + addressStartContent, 14].Value = itemCharge.PackageContainer;

                        workSheet.Cells[i + addressStartContent, 15].Value = itemCharge.NetAmount;
                        workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                        string vatAmount = "( " + itemCharge.VATAmount + " )";

                        if (itemCharge.VATAmount < 0)
                        {
                            workSheet.Cells[i + addressStartContent, 16].Value = vatAmount;
                            workSheet.Cells[i + addressStartContent, 19].Value = vatAmount;
                        }
                        else
                        {
                            workSheet.Cells[i + addressStartContent, 16].Value = itemCharge.VATAmount != null ? itemCharge.VATAmount : null;
                            workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                        }
                        workSheet.Cells[i + addressStartContent, 17].Value = itemCharge.VATAmount.GetValueOrDefault(0M) + itemCharge.NetAmount.GetValueOrDefault(0M);
                        workSheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;


                        if (itemCharge.Type.Contains("OBH"))
                        {
                            workSheet.Cells[i + addressStartContent, 18].Value = itemCharge.NetAmount;
                            workSheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                            workSheet.Cells[i + addressStartContent, 20].Value = itemCharge.VATAmount.GetValueOrDefault(0M) + itemCharge.NetAmount.GetValueOrDefault(0M);
                            workSheet.Cells[i + addressStartContent, 20].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                            workSheet.Cells[i + addressStartContent, 19].Value = itemCharge.VATAmount;
                            workSheet.Cells[i + addressStartContent, 19].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                            workSheet.Cells[i + addressStartContent, 15].Value = null;
                            workSheet.Cells[i + addressStartContent, 16].Value = null;
                            workSheet.Cells[i + addressStartContent, 17].Value = null;

                        }

                        workSheet.Cells[i + addressStartContent, 22].Value = itemCharge.SoaNo;
                        workSheet.Cells[i + addressStartContent, 23].Value = itemCharge.InvoiceNo;
                        workSheet.Cells[i + addressStartContent, 24].Value = itemCharge.InvoiceDate;
                        workSheet.Cells[i + addressStartContent, 24].Value = itemCharge.InvoiceDate.HasValue ? itemCharge.InvoiceDate.Value.ToString("dd/MM/yyyy") : "";

                        decimal? TotalNormalCharge = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 17].Value);
                        decimal? TotalOBHCharge = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 20].Value);
                        workSheet.Cells[i + addressStartContent, 21].Value = TotalNormalCharge.GetValueOrDefault(0M) + TotalOBHCharge.GetValueOrDefault(0M);
                        workSheet.Cells[i + addressStartContent, 21].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                        totalALLValue += TotalNormalCharge.GetValueOrDefault(0M) + TotalOBHCharge.GetValueOrDefault(0M);

                        //decimal? TotalOBHCharge = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 15].Value);
                        //workSheet.Cells[i + addressStartContent, 16].Value = TotalNormalCharge.GetValueOrDefault(0M) + TotalOBHCharge.GetValueOrDefault(0M);
                        //workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = numberFormat;

                    }
                }
                addressStartContent = addressStartContent + obj.summaryOfRevenueExportResults.Count;

                workSheet.Cells[1, 1, addressStartContent, 24].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[1, 1, addressStartContent, 24].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[1, 1, addressStartContent, 24].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells.AutoFitColumns();

                workSheet.Cells[addressStartContent, 1].Value = "Total"; //Total
                string addressTotal = workSheet
              .Cells[addressStartContent, 1]
              .First(c => c.Value.ToString() == "Total")
              .Start
              .Address;
                string addressTotalMerge = workSheet
                 .Cells[addressStartContent, 6].Start.Address;
                string addressToMerge = addressTotal + ":" + addressTotalMerge;
                workSheet.Cells[addressToMerge].Merge = true;
                workSheet.Cells[addressToMerge].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[addressToMerge].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[addressStartContent, 1].Style.Font.Bold = true;


                decimal? totalNetAmountNormalCharge = 0;
                decimal? totalNetAmountOBHCharge = 0;
                decimal? totalVATNormalCharge = 0;
                decimal? totalVATOBHCharge = 0;
                decimal? totalNormalCharge = 0;
                decimal? totalOBHCharge = 0;
                decimal? totalAll = 0;
                decimal? totalQuantity = 0;



                foreach (var item in obj.summaryOfRevenueExportResults)
                {
                    foreach (var it in item.SummaryOfCostsIncurredExportResults)
                    {
                        if (!it.Type.Contains("OBH"))
                        {
                            totalNetAmountNormalCharge += it.NetAmount.GetValueOrDefault(0M);
                            totalVATNormalCharge += it.VATAmount.GetValueOrDefault(0M);
                            totalNormalCharge = totalNetAmountNormalCharge.GetValueOrDefault(0M) + totalVATNormalCharge.GetValueOrDefault(0M);
                        }
                        else
                        {
                            totalNetAmountOBHCharge += it.NetAmount.GetValueOrDefault(0M);
                            totalVATOBHCharge += it.VATAmount.GetValueOrDefault(0M);
                            totalOBHCharge = totalNetAmountOBHCharge.GetValueOrDefault(0M) + totalVATOBHCharge.GetValueOrDefault(0M);
                        }

                    }
                    totalQuantity += item.SummaryOfCostsIncurredExportResults.Select(t => t.Quantity).Sum();
                }

                workSheet.Cells[addressStartContent, 7].Value = totalQuantity;
                workSheet.Cells[addressStartContent, 7].Style.Font.Bold = true;
                workSheet.Cells[addressStartContent, 15].Value = totalNetAmountNormalCharge;
                workSheet.Cells[addressStartContent, 15].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                workSheet.Cells[addressStartContent, 15].Style.Font.Bold = true;

                workSheet.Cells[addressStartContent, 16].Value = totalVATNormalCharge != null ? totalVATNormalCharge : null;
                if (totalVATNormalCharge == 0)
                {
                    workSheet.Cells[addressStartContent, 16].Value = null;
                }
                workSheet.Cells[addressStartContent, 16].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                workSheet.Cells[addressStartContent, 16].Style.Font.Bold = true;

                workSheet.Cells[addressStartContent, 17].Value = totalNormalCharge;
                workSheet.Cells[addressStartContent, 17].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                workSheet.Cells[addressStartContent, 17].Style.Font.Bold = true;

                workSheet.Cells[addressStartContent, 18].Value = totalNetAmountOBHCharge;
                workSheet.Cells[addressStartContent, 18].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                workSheet.Cells[addressStartContent, 18].Style.Font.Bold = true;

                workSheet.Cells[addressStartContent, 19].Value = totalVATOBHCharge;
                workSheet.Cells[addressStartContent, 19].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                workSheet.Cells[addressStartContent, 19].Style.Font.Bold = true;

                workSheet.Cells[addressStartContent, 20].Value = totalOBHCharge;
                workSheet.Cells[addressStartContent, 20].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                workSheet.Cells[addressStartContent, 20].Style.Font.Bold = true;

                totalAll = totalOBHCharge + totalNormalCharge;

                workSheet.Cells[addressStartContent, 21].Value = totalAll;
                workSheet.Cells[addressStartContent, 21].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                workSheet.Cells[addressStartContent, 21].Style.Font.Bold = true;


            }
        }

        public Stream GenerateShipmentOverviewExcel(IQueryable<GeneralExportShipmentOverviewResult> overview, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Shipment Overview");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BinddingDataShipmentOverview(workSheet, overview.ToList(), criteria);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception)
            {

            }
            return null;
        }

        public void BinddingDataShipmentOverview(ExcelWorksheet workSheet, List<GeneralExportShipmentOverviewResult> overview, GeneralReportCriteria criteria)
        {
            List<string> headers = new List<string>()
            {
                "INDO TRANS LOGISTICS CORPORATION",
               "52-54-56 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
               "SHIPMENT OVERVIEW"
            };

            List<string> headersTable = new List<string>()
            {
                "No",
                "SERVICE",
                "PRO SERVICE",
                "JOB NO",
                "CUSTOM NO",
                "SERVICE DATE",
                "ETD",
                "ETA",
                "VESSEL/FLIGHT",
                "MBL/MAWB",
                "HBL/HAWB",
                "POL/POD",
                "CARRIER",
                "AGENT",
                "SHIPPER",
                "CONSIGNEE",
                "SHIPMENT TYPE",
                "SALEMAN",
                "NOINATION PARTY",
                "Q'TY",
                "20'",
                "40'",
                "40'HC",
                "45'",
                "G.W",
                "C.W",
                "CBM",
                "REVENUE",
                "COSTING",
                "PROFIT",
                "OBH(P)",
                "OBH(R)",
                "DESTINATION",
                "CUSTOMER ID",
                "CUSTOMER NAME",
                "RELATED HBL/HAWB",
                "RELATED JOB NO",
                "HANDLE OFFICE",
                "SALES OFFICE",
                "CREATOR",
                "P.O/INV#",
                "B.K/REF NO",
                "COMMODITY",
                "REFERENCE NO",
                "P/M TERM",
                "SHIPMENT NOTES",
                "CREATED"
            };

            List<string> subheadersTable = new List<string>()
            {
                "FREIGHT",
                "TRUCKING",
                "HANDLING FEE",
                "CUSTOMS",
                "OTHERS",
                "TOTAL",
                "COM."
            };
            //Title
            workSheet.Cells["A7:BA7"].Merge = true;
            workSheet.Cells["A7"].Style.Font.SetFromFont(new Font("Times New Roman", 15));
            workSheet.Cells["A7"].Value = headers[2];
            workSheet.Cells["A7"].Style.Font.Size = 16;
            workSheet.Cells["A7"].Style.Font.Bold = true;
            workSheet.Cells["A7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A1:E1"].Merge = true;
            workSheet.Cells["A1"].Value = headers[0];
            workSheet.Cells["A1"].Style.Font.Bold = true;
            workSheet.Cells["A2:E6"].Merge = true;
            workSheet.Cells["A2:E6"].Style.WrapText = true;
            workSheet.Cells["A2"].Value = headers[1];
            workSheet.Cells["A2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10));
            workSheet.Cells["A8:BA8"].Merge = true;
            DateTime fromDate = criteria.CreatedDateFrom == null ? criteria.ServiceDateFrom.GetValueOrDefault() : criteria.CreatedDateFrom.GetValueOrDefault();
            DateTime toDate = criteria.CreatedDateTo == null ? criteria.ServiceDateTo.GetValueOrDefault() : criteria.CreatedDateTo.GetValueOrDefault();
            workSheet.Cells["A8"].Value = "From:" + fromDate.ToString("dd/MM/yyyy") + " To:" + toDate.ToString("dd/MM/yyyy");
            workSheet.Cells["A8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Tạo header
            for (int i = 0; i < headersTable.Count; i++)
            {
                if (i == 27)
                {
                    workSheet.Cells[9, i + 6].Value = headersTable[i];
                }
                if (i < 27)
                {
                    workSheet.Cells[9, i + 1].Value = headersTable[i];
                }
                if (i > 27)
                {
                    workSheet.Cells[9, i + 12].Value = headersTable[i];
                    workSheet.Cells[9, i + 12].Style.Font.Bold = true;
                }
                workSheet.Cells[9, i + 1].Style.Font.Bold = true;
                workSheet.Cells[9, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[9, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            }

            for (int i = 1; i < 59; i++)
            {
                if (i <= 27 || i >= 41)
                {
                    workSheet.Cells[9, i, 10, i].Merge = true;
                    workSheet.Cells[9, i, 10, i].Style.WrapText = true;
                }
            }

            int revenueCol = 28;
            for (int i = 0; i <= 5; i++)
            {
                workSheet.Cells[10, revenueCol].Value = subheadersTable[i];
                revenueCol++;
            }
            //workSheet.Cells["AA10"].Value = subheadersTable[0];
            //workSheet.Cells["AB10"].Value = subheadersTable[1];
            //workSheet.Cells["AC10"].Value = subheadersTable[2];
            //workSheet.Cells["AD10"].Value = subheadersTable[3];
            //workSheet.Cells["AE10"].Value = subheadersTable[4];
            //workSheet.Cells["AF10"].Value = subheadersTable[5];

            workSheet.Cells["AU9:BF9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["AU9:BF9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["AB10:AN10"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["AB10:AN10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["AB10:AN10"].Style.Font.Bold = true;

            workSheet.Cells["AH10"].Value = subheadersTable[0];
            workSheet.Cells["AI10"].Value = subheadersTable[1];
            workSheet.Cells["AJ10"].Value = subheadersTable[2];
            workSheet.Cells["AK10"].Value = subheadersTable[3];

            workSheet.Cells["AL10"].Value = subheadersTable[6];
            //workSheet.Cells["AM10"].Value = subheadersTable[6];
            workSheet.Cells["AM10"].Value = subheadersTable[4];
            workSheet.Cells["AN10"].Value = subheadersTable[5];
            workSheet.Cells["AB9:AG9"].Merge = true;
            workSheet.Cells["AH9:AN9"].Merge = true;

            workSheet.Cells["AB9"].Value = headersTable[27];
            workSheet.Cells["AH9"].Value = headersTable[28];

            //Cố định dòng thứ 10 (Freeze Row 10 and no column)
            workSheet.View.FreezePanes(11, 1);

            int addressStartContent = 11;
            int positionStart = 1;
            int j = 0;
            //var array = overview.ToArray();
            foreach (var item in overview)
            {
                workSheet.Cells[j + addressStartContent, 1].Value = j + 1;
                workSheet.Cells[j + addressStartContent, 2].Value = item.ServiceName;
                workSheet.Cells[j + addressStartContent, 3].Value = item.ProductService;
                workSheet.Cells[j + addressStartContent, 4].Value = item.JobNo;
                workSheet.Cells[j + addressStartContent, 5].Value = item.CustomNo;

                workSheet.Cells[j + addressStartContent, 6].Value = item.ServiceDate.HasValue ? item.ServiceDate.Value.ToString("dd/MM/yyyy") : "";
                workSheet.Cells[j + addressStartContent, 7].Value = item.etd.HasValue ? item.etd.Value.ToString("dd/MM/yyyy") : "";
                workSheet.Cells[j + addressStartContent, 8].Value = item.eta.HasValue ? item.eta.Value.ToString("dd/MM/yyyy") : "";
                workSheet.Cells[j + addressStartContent, 9].Value = item.FlightNo;
                workSheet.Cells[j + addressStartContent, 10].Value = item.MblMawb;
                workSheet.Cells[j + addressStartContent, 11].Value = item.HblHawb;
                workSheet.Cells[j + addressStartContent, 12].Value = item.PolPod;
                workSheet.Cells[j + addressStartContent, 13].Value = item.Carrier;
                workSheet.Cells[j + addressStartContent, 14].Value = item.Agent;
                workSheet.Cells[j + addressStartContent, 15].Value = item.Shipper;
                workSheet.Cells[j + addressStartContent, 16].Value = item.Consignee;
                workSheet.Cells[j + addressStartContent, 17].Value = item.ShipmentType;
                workSheet.Cells[j + addressStartContent, 18].Value = item.Salesman;
                workSheet.Cells[j + addressStartContent, 19].Value = item.AgentName;
                workSheet.Cells[j + addressStartContent, 20].Value = item.QTy;
                workSheet.Cells[j + addressStartContent, 21].Value = item.Cont20;
                workSheet.Cells[j + addressStartContent, 22].Value = item.Cont40;
                workSheet.Cells[j + addressStartContent, 23].Value = item.Cont40HC;
                workSheet.Cells[j + addressStartContent, 24].Value = item.Cont45;
                workSheet.Cells[j + addressStartContent, 25].Value = item.GW;
                workSheet.Cells[j + addressStartContent, 26].Value = item.CW;
                workSheet.Cells[j + addressStartContent, 27].Value = item.CBM;
                workSheet.Cells[j + addressStartContent, 28].Value = item.TotalSellFreight;
                workSheet.Cells[j + addressStartContent, 29].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 29].Value = item.TotalSellTrucking;
                workSheet.Cells[j + addressStartContent, 29].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 30].Value = item.TotalSellHandling;
                workSheet.Cells[j + addressStartContent, 30].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 31].Value = item.TotalCustomSell;
                workSheet.Cells[j + addressStartContent, 31].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 32].Value = item.TotalSellOthers;
                workSheet.Cells[j + addressStartContent, 32].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 33].Value = item.TotalSell;
                workSheet.Cells[j + addressStartContent, 33].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 34].Value = item.TotalBuyFreight;
                workSheet.Cells[j + addressStartContent, 34].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 35].Value = item.TotalBuyTrucking;
                workSheet.Cells[j + addressStartContent, 35].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 36].Value = item.TotalBuyHandling;
                workSheet.Cells[j + addressStartContent, 36].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 37].Value = item.TotalCustomBuy;
                workSheet.Cells[j + addressStartContent, 37].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 38].Value = item.TotalBuyKB;
                workSheet.Cells[j + addressStartContent, 38].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 39].Value = item.TotalBuyOthers;
                workSheet.Cells[j + addressStartContent, 39].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 40].Value = item.TotalBuy;
                workSheet.Cells[j + addressStartContent, 40].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 41].Value = item.Profit;
                workSheet.Cells[j + addressStartContent, 41].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 42].Value = item.AmountOBH;
                workSheet.Cells[j + addressStartContent, 42].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 43].Value = item.AmountOBH;
                workSheet.Cells[j + addressStartContent, 43].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 44].Value = item.Destination;
                workSheet.Cells[j + addressStartContent, 45].Value = item.CustomerId;
                workSheet.Cells[j + addressStartContent, 46].Value = item.CustomerName;
                workSheet.Cells[j + addressStartContent, 47].Value = item.RalatedHblHawb;
                workSheet.Cells[j + addressStartContent, 48].Value = item.RalatedJobNo;
                workSheet.Cells[j + addressStartContent, 49].Value = item.HandleOffice;
                workSheet.Cells[j + addressStartContent, 50].Value = item.SalesOffice;
                workSheet.Cells[j + addressStartContent, 51].Value = item.Creator;
                workSheet.Cells[j + addressStartContent, 52].Value = item.POINV;
                workSheet.Cells[j + addressStartContent, 53].Value = item.BKRefNo;
                workSheet.Cells[j + addressStartContent, 54].Value = item.Commodity;
                workSheet.Cells[j + addressStartContent, 55].Value = item.ReferenceNo;
                //workSheet.Cells[i + addressStartContent, 53].Value = item.ShipmentType;
                workSheet.Cells[j + addressStartContent, 56].Value = item.PMTerm;
                workSheet.Cells[j + addressStartContent, 57].Value = item.ShipmentNotes;
                workSheet.Cells[j + addressStartContent, 58].Value = item.Created.HasValue ? item.Created.Value.ToString("dd/MM/yyyy") : "";

                j++;
                positionStart++;
            }
            workSheet.Cells.AutoFitColumns();
            positionStart = positionStart - 2;
            int address = addressStartContent + overview.Count;
            workSheet.Cells[address, 1].Value = "Total"; //Total
            string addressTotal = workSheet
              .Cells[address, 1]
              .First(c => c.Value.ToString() == "Total")
              .Start
              .Address;
            string addressTotalMerge = workSheet
             .Cells[address, 20].Start.Address;
            string addressToMerge = addressTotal + ":" + addressTotalMerge;
            workSheet.Cells[addressToMerge].Merge = true;
            workSheet.Cells[addressToMerge].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[addressToMerge].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[addressToMerge].Style.Font.Bold = true;

            string addressTotalCont20 = workSheet.Cells[address, 21].Start.Address;
            workSheet.Cells[addressTotalCont20].Value = overview.Select(t => t.Cont20).Sum();
            string addressTotalCont40 = workSheet.Cells[address, 22].Start.Address;
            workSheet.Cells[addressTotalCont40].Value = overview.Select(t => t.Cont40).Sum();
            string addressTotalCont40HC = workSheet.Cells[address, 23].Start.Address;
            workSheet.Cells[addressTotalCont40HC].Value = overview.Select(t => t.Cont40HC).Sum();
            string addressTotalCont45 = workSheet.Cells[address, 24].Start.Address;
            workSheet.Cells[addressTotalCont45].Value = overview.Select(t => t.Cont45).Sum();
            string addressTotalGW = workSheet.Cells[address, 25].Start.Address;
            workSheet.Cells[addressTotalGW].Value = overview.Select(t => t.GW).Sum();
            string addressTotalCW = workSheet.Cells[address, 26].Start.Address;
            workSheet.Cells[addressTotalCW].Value = overview.Select(t => t.CW).Sum();
            string addressTotalCBM = workSheet.Cells[address, 27].Start.Address;
            workSheet.Cells[addressTotalCBM].Value = overview.Select(t => t.CBM).Sum();
            string addressTotalSellFreight = workSheet.Cells[address, 28].Start.Address;
            workSheet.Cells[addressTotalSellFreight].Value = overview.Select(t => t.TotalSellFreight).Sum();
            workSheet.Cells[addressTotalSellFreight].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalSellTrucking = workSheet.Cells[address, 29].Start.Address;
            workSheet.Cells[addressTotalSellTrucking].Value = overview.Select(t => t.TotalSellTrucking).Sum();
            workSheet.Cells[addressTotalSellTrucking].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalSellHandling = workSheet.Cells[address, 30].Start.Address;
            workSheet.Cells[addressTotalSellHandling].Value = overview.Select(t => t.TotalSellHandling).Sum();
            workSheet.Cells[addressTotalSellHandling].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalSellCustom = workSheet.Cells[address, 31].Start.Address;
            workSheet.Cells[addressTotalSellCustom].Value = overview.Select(t => t.TotalCustomSell).Sum();
            workSheet.Cells[addressTotalSellCustom].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalSellOther = workSheet.Cells[address, 32].Start.Address;
            workSheet.Cells[addressTotalSellOther].Value = overview.Select(t => t.TotalSellOthers).Sum();
            workSheet.Cells[addressTotalSellOther].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalSell = workSheet.Cells[address, 33].Start.Address;
            workSheet.Cells[addressTotalSell].Value = overview.Select(t => t.TotalSell).Sum();
            workSheet.Cells[addressTotalSell].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyFreight = workSheet.Cells[address, 34].Start.Address;
            workSheet.Cells[addressTotalBuyFreight].Value = overview.Select(t => t.TotalBuyFreight).Sum();
            workSheet.Cells[addressTotalBuyFreight].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyTrucking = workSheet.Cells[address, 35].Start.Address;
            workSheet.Cells[addressTotalBuyTrucking].Value = overview.Select(t => t.TotalBuyTrucking).Sum();
            workSheet.Cells[addressTotalBuyTrucking].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyHandling = workSheet.Cells[address, 36].Start.Address;
            workSheet.Cells[addressTotalBuyHandling].Value = overview.Select(t => t.TotalBuyHandling).Sum();
            workSheet.Cells[addressTotalBuyHandling].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyCustom = workSheet.Cells[address, 37].Start.Address;
            workSheet.Cells[addressTotalBuyCustom].Value = overview.Select(t => t.TotalCustomBuy).Sum();
            workSheet.Cells[addressTotalBuyCustom].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;


            string addressTotalBuyKB = workSheet.Cells[address, 38].Start.Address;
            workSheet.Cells[addressTotalBuyKB].Value = overview.Select(t => t.TotalBuyKB).Sum();
            workSheet.Cells[addressTotalBuyKB].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyOthers = workSheet.Cells[address, 39].Start.Address;
            workSheet.Cells[addressTotalBuyOthers].Value = overview.Select(t => t.TotalBuyOthers).Sum();
            workSheet.Cells[addressTotalBuyOthers].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuy = workSheet.Cells[address, 40].Start.Address;
            workSheet.Cells[addressTotalBuy].Value = overview.Select(t => t.TotalBuy).Sum();
            workSheet.Cells[addressTotalBuy].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressProfit = workSheet.Cells[address, 41].Start.Address;
            workSheet.Cells[addressProfit].Value = overview.Select(t => t.Profit).Sum();
            workSheet.Cells[addressProfit].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressAmountOBH = workSheet.Cells[address, 42].Start.Address;
            workSheet.Cells[addressAmountOBH].Value = overview.Select(t => t.AmountOBH).Sum();
            workSheet.Cells[addressAmountOBH].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressAmountOBHR = workSheet.Cells[address, 43].Start.Address;
            workSheet.Cells[addressAmountOBHR].Value = overview.Select(t => t.AmountOBH).Sum();
            workSheet.Cells[addressAmountOBHR].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressToBold = addressTotalCont20 + ":" + addressAmountOBHR;
            workSheet.Cells[addressToBold].Style.Font.Bold = true;

            //workSheet.Column(36).Hidden = true;
            //workSheet.Column(37).Hidden = true;
            workSheet.Column(2).Width = 20;
            workSheet.Column(3).Width = 20;
            workSheet.Column(4).Width = 20;
            workSheet.Column(5).Width = 15;
            workSheet.Column(9).Width = 20;
            workSheet.Column(17).Width = 20;

            workSheet.Cells[9, 1, addressStartContent + positionStart + 1, 58].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[9, 1, addressStartContent + positionStart + 1, 58].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[9, 1, addressStartContent + positionStart + 1, 58].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        }

        public Stream GenerateShipmentOverviewFCLExcell(IQueryable<GeneralExportShipmentOverviewFCLResult> overviews, GeneralReportCriteria criteria)
        {
            try
            {
                FileInfo f = new FileInfo(Path.Combine(ReportConstants.PathOfTemplateExcel, ReportConstants.Shipment_Overview_FCL));
                var path = f.FullName;
                if (!File.Exists(path))
                {
                    return null;
                }
                var excel = new ExcelExport(path);
                var listKeyData = new Dictionary<string, object>();
                listKeyData.Add("FromDate", "From " + Convert.ToDateTime(criteria.ServiceDateFrom).ToShortDateString() + " To " + Convert.ToDateTime(criteria.ServiceDateTo).ToShortDateString());

                int j = 0;
                int startRow = 11;
                excel.StartDetailTable = startRow;
                listKeyData.Add("sumCont20", overviews.Select(t => t.Cont20).Sum());
                listKeyData.Add("sumCont40", overviews.Select(t => t.Cont40).Sum());
                listKeyData.Add("sumCont40HC", overviews.Select(t => t.Cont40HC).Sum());
                listKeyData.Add("sumcont45", overviews.Select(t => t.Cont45).Sum());
                listKeyData.Add("sumGW", overviews.Select(t => t.GW).Sum());
                listKeyData.Add("sumCW", overviews.Select(t => t.CW).Sum());
                listKeyData.Add("sumCBM", overviews.Select(t => t.CBM).Sum());

                // total revenue
                listKeyData.Add("sumSellFreight", overviews.Select(t => t.TotalSellFreight).Sum());
                listKeyData.Add("sumSellTerminal", overviews.Select(t => t.TotalSellTerminal).Sum());
                listKeyData.Add("sumSellBillFee", overviews.Select(t => t.TotalSellBillFee).Sum());
                listKeyData.Add("sumSellSealFee", overviews.Select(t => t.TotalSellContainerSealFee).Sum());
                listKeyData.Add("sumSellRelease", overviews.Select(t => t.TotalSellTelexRelease).Sum());
                listKeyData.Add("sumSellAutomated", overviews.Select(t => t.TotalSellAutomated).Sum());
                listKeyData.Add("sumSellVGM", overviews.Select(t => t.TotalSellVGM).Sum());
                listKeyData.Add("sumSellBookingFee", overviews.Select(t => t.TotalSellBookingFee).Sum());
                listKeyData.Add("sumSellOther", overviews.Select(t => t.TotalSellOthers).Sum());
                listKeyData.Add("sumSellTotal", overviews.Select(t => t.TotalSell).Sum());

                // total costing
                listKeyData.Add("sumBuyFreight", overviews.Select(t => t.TotalBuyFreight).Sum());
                listKeyData.Add("sumBuyTerminal", overviews.Select(t => t.TotalBuyTerminal).Sum());
                listKeyData.Add("sumBuyBillFee", overviews.Select(t => t.TotalBuyBillFee).Sum());
                listKeyData.Add("sumBuySealFee", overviews.Select(t => t.TotalBuyContainerSealFee).Sum());
                listKeyData.Add("sumBuyRelease", overviews.Select(t => t.TotalBuyTelexRelease).Sum());
                listKeyData.Add("sumBuyAutomated", overviews.Select(t => t.TotalBuyAutomated).Sum());
                listKeyData.Add("sumBuyVGM", overviews.Select(t => t.TotalBuyVGM).Sum());
                listKeyData.Add("sumBuyBookingFee", overviews.Select(t => t.TotalBuyBookingFee).Sum());
                listKeyData.Add("sumBuyOther", overviews.Select(t => t.TotalBuyOthers).Sum());
                listKeyData.Add("sumBuyTotal", overviews.Select(t => t.TotalBuy).Sum());

                listKeyData.Add("sumProfit", overviews.Select(t => t.Profit).Sum());
                listKeyData.Add("sumObhP", overviews.Select(t => t.AmountOBH).Sum());
                listKeyData.Add("sumObhR", overviews.Select(t => t.AmountOBH).Sum());
                excel.SetData(listKeyData);

                foreach (var item in overviews)
                {
                    excel.SetDataTable();
                    listKeyData = new Dictionary<string, object>();
                    listKeyData.Add("no", j + 1);
                    listKeyData.Add("referenceNo", item.BKRefNo);
                    listKeyData.Add("service", item.ServiceName);
                    listKeyData.Add("jobNo", item.JobNo);
                    listKeyData.Add("servicedate", item.ServiceDate?.ToString("dd/MM/yyyy"));
                    listKeyData.Add("etd", item.etd?.ToString("dd/MM/yyyy"));
                    listKeyData.Add("eta", item.eta?.ToString("dd/MM/yyyy"));
                    listKeyData.Add("vessel", item.FlightNo);
                    listKeyData.Add("mbl", item.MblMawb);
                    listKeyData.Add("hbl", item.HblHawb);
                    listKeyData.Add("polpod", item.PolPod);
                    listKeyData.Add("destination", item.FinalDestination);
                    listKeyData.Add("carrier", item.Carrier);
                    listKeyData.Add("agent", item.Agent);
                    listKeyData.Add("shipper", item.Shipper);
                    listKeyData.Add("consignee", item.Consignee);
                    listKeyData.Add("shipmentType", item.ShipmentType);
                    listKeyData.Add("saleman", item.Salesman);
                    listKeyData.Add("noinationparty", item.AgentName);

                    listKeyData.Add("qty", item.QTy);
                    listKeyData.Add("cont20", item.Cont20);
                    listKeyData.Add("cont40", item.Cont40);
                    listKeyData.Add("cont40HC", item.Cont40HC);
                    listKeyData.Add("cont45", item.Cont45);
                    listKeyData.Add("gw", item.GW);
                    listKeyData.Add("cw", item.CW);
                    listKeyData.Add("cbm", item.CBM);
                    listKeyData.Add("sellFreight", item.TotalSellFreight);
                    listKeyData.Add("sellTerminal", item.TotalSellTerminal);
                    listKeyData.Add("sellBillFee", item.TotalSellBillFee);
                    listKeyData.Add("sellSealFee", item.TotalSellContainerSealFee);
                    listKeyData.Add("sellRelease", item.TotalSellTelexRelease);
                    listKeyData.Add("sellAutomated", item.TotalSellAutomated);
                    listKeyData.Add("sellVGM", item.TotalSellVGM);
                    listKeyData.Add("sellBookingFee", item.TotalSellBookingFee);
                    listKeyData.Add("sellOther", item.TotalSellOthers);
                    listKeyData.Add("totalSell", item.TotalSell);

                    listKeyData.Add("buyFreight", item.TotalBuyFreight);
                    listKeyData.Add("buyTerminal", item.TotalBuyTerminal);
                    listKeyData.Add("buyBillFee", item.TotalBuyBillFee);
                    listKeyData.Add("buySealFee", item.TotalBuyContainerSealFee);
                    listKeyData.Add("buyRelease", item.TotalBuyTelexRelease);
                    listKeyData.Add("buyAutomated", item.TotalBuyAutomated);
                    listKeyData.Add("buyVGM", item.TotalBuyVGM);
                    listKeyData.Add("buyBookingFee", item.TotalBuyBookingFee);
                    listKeyData.Add("buyOther", item.TotalBuyOthers);
                    listKeyData.Add("totalBuy", item.TotalBuy);
                    listKeyData.Add("profit", item.Profit);
                    listKeyData.Add("obhp", item.AmountOBH);
                    listKeyData.Add("obhr", item.AmountOBH);
                    excel.SetData(listKeyData);

                    excel.Worksheet.Cells[j + startRow, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 29].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 30].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 31].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 32].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 33].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 34].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 35].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 36].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 37].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 38].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 39].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 40].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 41].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 42].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 43].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 44].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 45].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 46].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 47].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 48].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    excel.Worksheet.Cells[j + startRow, 49].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                    j++;
                }

                return excel.ExcelStream();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Stream BidingGeneralLCLExport(IQueryable<GeneralExportShipmentOverviewFCLResult> data, GeneralReportCriteria criteria, string fileName)
        {
            try
            {
                FileInfo f = new FileInfo(Path.Combine(ReportConstants.PathOfTemplateExcel, fileName));
                var path = f.FullName + ".xlsx";
                if (!File.Exists(path))
                {
                    return null;
                }
                var excel = new ExcelExport(path);
                var listKey = new Dictionary<string, object>();
                var _fromDate = "From: " + criteria.ServiceDateFrom?.ToString("dd/MM/yyyy") + " To: " + criteria.ServiceDateTo?.ToString("dd/MM/yyyy");
                listKey.Add("FromDate", _fromDate);
                excel.SetData(listKey);
                // Get format currency
                var formatCurrency = criteria.Currency == CURRENCY_LOCAL ? _formatVNDNew : _formatNew;
                var formatCell = new List<string>
                {
                    "SellFreight",
                    "SellTerminal",
                    "SellBillFee",
                    "SellTelexRelease",
                    "SellCFSFee",
                    "SellEBSFee",
                    "SellAutomated",
                    "SellVGM",
                    "SellBookingFee",
                    "SellCustomFee",
                    "SellOthers",
                    "TotalSelling",
                    "BuyFreight",
                    "BuyTerminal",
                    "BuyBillFee",
                    "BuyTelexRelease",
                    "BuyCFSFee",
                    "BuyEBSFee",
                    "BuyAutomated",
                    "BuyVGM",
                    "BuyCustomFee",
                    "BuyOthers",
                    "TotalBuying",
                    "Profit",
                    "OBHP",
                    "OBHR",
                };
                excel.SetFormatCell(formatCell, formatCurrency);
                // Start of Table
                var startOfDetail = 11;
                excel.StartDetailTable = startOfDetail;
                var shipments = data.ToList();
                int i = 0;
                while (i < shipments.Count())
                {
                    listKey = new Dictionary<string, object>();
                    var shipment = shipments[i];
                    excel.SetDataTable();
                    listKey.Add("Order", i + 1);
                    listKey.Add("FlexId", shipment.ReferenceNo);
                    listKey.Add("Service", shipment.ServiceName);
                    listKey.Add("JobNo", shipment.JobNo);
                    listKey.Add("servicedate", shipment.ServiceDate?.ToString("dd/MM/yyyy"));
                    listKey.Add("ETD", shipment.etd?.ToString("dd/MM/yyyy"));
                    listKey.Add("ETA", shipment.eta?.ToString("dd/MM/yyyy"));
                    listKey.Add("Vessel", shipment.FlightNo); // VESSEL/FLIGHT
                    listKey.Add("MBL", shipment.MblMawb);
                    listKey.Add("HBL", shipment.HblHawb);
                    listKey.Add("Incoterm", shipment.Incoterm); // Incoterm
                    listKey.Add("Port", shipment.PolPod);
                    listKey.Add("FinalDestination", shipment.FinalDestination);
                    listKey.Add("Carrier", shipment.Carrier); // Coloader
                    listKey.Add("Agent", shipment.AgentName);
                    listKey.Add("Shipper", shipment.Shipper);
                    listKey.Add("Consignee", shipment.Consignee);
                    listKey.Add("ShipmentType", shipment.ShipmentType);
                    listKey.Add("Salesman", shipment.Salesman);
                    listKey.Add("NotifyParty", shipment.NotifyParty);
                    listKey.Add("QTy", shipment.PackageQty);
                    listKey.Add("Cont20", shipment.Cont20);
                    listKey.Add("Cont40", shipment.Cont40);
                    listKey.Add("Cont40HC", shipment.Cont40HC);
                    listKey.Add("Cont45", shipment.Cont45);
                    listKey.Add("GW", shipment.GW);
                    listKey.Add("CW", shipment.CW);
                    listKey.Add("CBM", shipment.CBM);
                    listKey.Add("SellFreight", shipment.TotalSellFreight);
                    listKey.Add("SellTerminal", shipment.TotalSellTerminal);
                    listKey.Add("SellBillFee", shipment.TotalSellBillFee);
                    listKey.Add("SellTelexRelease", shipment.TotalSellTelexRelease);
                    listKey.Add("SellCFSFee", shipment.TotalSellCFSFee);
                    listKey.Add("SellEBSFee", shipment.TotalSellEBSFee);
                    listKey.Add("SellAutomated", shipment.TotalSellAutomated);
                    listKey.Add("SellVGM", shipment.TotalSellVGM);
                    listKey.Add("SellBookingFee", shipment.TotalSellBookingFee);
                    listKey.Add("SellCustomFee", shipment.TotalSellCustomFee);
                    listKey.Add("SellOthers", shipment.TotalSellOthers);
                    listKey.Add("TotalSelling", shipment.TotalSell);
                    listKey.Add("BuyFreight", shipment.TotalBuyFreight);
                    listKey.Add("BuyTerminal", shipment.TotalBuyTerminal);
                    listKey.Add("BuyBillFee", shipment.TotalBuyBillFee);
                    listKey.Add("BuyTelexRelease", shipment.TotalBuyTelexRelease);
                    listKey.Add("BuyCFSFee", shipment.TotalBuyCFSFee);
                    listKey.Add("BuyEBSFee", shipment.TotalBuyEBSFee);
                    listKey.Add("BuyAutomated", shipment.TotalBuyAutomated);
                    listKey.Add("BuyVGM", shipment.TotalBuyVGM);
                    listKey.Add("BuyCustomFee", shipment.TotalBuyCustomFee);
                    listKey.Add("BuyOthers", shipment.TotalBuyOthers);
                    listKey.Add("TotalBuying", shipment.TotalBuy);
                    listKey.Add("Profit", shipment.Profit);
                    listKey.Add("OBHP", shipment.AmountOBH);
                    listKey.Add("OBHR", shipment.AmountOBH);
                    excel.SetData(listKey);
                    i++;
                }

                var lastRow = startOfDetail + shipments.Count();
                var listKeyFormula = new Dictionary<string, string>();
                i = 1;
                while (i <= 31)
                {
                    var totalFormat = string.Format("{0}{1}", "Total", i);
                    var _addressTotal = excel.AddressOfKey(totalFormat);
                    var _statement = string.Format("SUM({0}{1}:{0}{2})", _addressTotal.ColumnLetter, startOfDetail, lastRow - 1);
                    listKeyFormula.Add(totalFormat, _statement);
                    i++;
                }
                excel.SetFormula(listKeyFormula);
                return excel.ExcelStream();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Stream GenerateStandardGeneralReportExcel(IQueryable<GeneralReportResult> listData, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add(string.Format(@"Standard Report ({0})", criteria.Currency));
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BinddingDataStandardGeneralReport(workSheet, listData, criteria);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception)
            {

            }
            return null;
        }

        private void SetWidthColumnExcelStandardGeneralReportExport(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 5.5; //Cột A
            workSheet.Column(2).Width = 14.5; //Cột B
            workSheet.Column(3).Width = 18; //Cột C
            workSheet.Column(4).Width = 18; //Cột D
            workSheet.Column(5).Width = 34; //Cột E
            workSheet.Column(6).Width = 34; //Cột F
            workSheet.Column(7).Width = 34; //Cột G
            workSheet.Column(8).Width = 12.5; //Cột H
            workSheet.Column(9).Width = 13; //Cột I
            workSheet.Column(10).Width = 14; //Cột J
            workSheet.Column(11).Width = 14; //Cột K
            workSheet.Column(12).Width = 14; //Cột L
            workSheet.Column(13).Width = 14; //Cột M
            workSheet.Column(14).Width = 14; //Cột N
            workSheet.Column(15).Width = 20; //Cột O
            workSheet.Column(16).Width = 20; //Cột P
            workSheet.Column(17).Width = 20; //Cột Q 
            workSheet.Column(18).Width = 18; //Cột R
        }
        private void BinddingDataStandardGeneralReport(ExcelWorksheet workSheet, IQueryable<GeneralReportResult> listData, GeneralReportCriteria criteria)
        {
            SetWidthColumnExcelStandardGeneralReportExport(workSheet);
            List<string> headers = new List<string>
            {
                "No.", //0
                "Job ID", //1
                "MBL/MAWB", //2
                "HBL/HAWB", //3
                "Customer", //4
                "Carrier", //5
                "Agent", //6
                "Service Date", //7
                "Vessel/Flight", //8
                "Route", //9
                "Qty", //10
                "CW", //11
                "Revenue", //12
                "Cost", //13
                "Profit", //14
                "OBH", //15
                "P.I.C", //16
                "Salesman", //17
                "Service" //18
            };
            workSheet.Cells["A1:R1"].Style.Font.Bold = true;
            for (int c = 1; c < 19; c++)
            {
                workSheet.Cells[1, c].Value = headers[c - 1];
            }

            //Cố định dòng đầu tiên (Freeze Row 1 and no column)
            workSheet.View.FreezePanes(2, 1);

            int startRow = 2;
            foreach (var item in listData)
            {
                workSheet.Row(startRow).Style.VerticalAlignment = ExcelVerticalAlignment.Top;

                workSheet.Cells[startRow, 1].Value = item.No;
                workSheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                workSheet.Cells[startRow, 2].Value = item.JobId;
                workSheet.Cells[startRow, 3].Value = item.Mawb;
                workSheet.Cells[startRow, 4].Value = item.Hawb;
                workSheet.Cells[startRow, 5].Value = item.CustomerName;
                workSheet.Cells[startRow, 5].Style.WrapText = true;
                workSheet.Cells[startRow, 6].Value = item.CarrierName;
                workSheet.Cells[startRow, 6].Style.WrapText = true;
                workSheet.Cells[startRow, 7].Value = item.AgentName;
                workSheet.Cells[startRow, 7].Style.WrapText = true;
                workSheet.Cells[startRow, 8].Value = item.ServiceDate;
                workSheet.Cells[startRow, 8].Style.Numberformat.Format = "dd/MM/yyyy";
                workSheet.Cells[startRow, 9].Value = item.VesselFlight;
                workSheet.Cells[startRow, 10].Value = item.Route;

                workSheet.Cells[startRow, 11].Value = item.Qty;
                workSheet.Cells[startRow, 12].Value = item.ChargeWeight;
                workSheet.Cells[startRow, 13].Value = item.Revenue;
                workSheet.Cells[startRow, 14].Value = item.Cost;
                workSheet.Cells[startRow, 15].Value = item.Profit;
                workSheet.Cells[startRow, 16].Value = item.Obh;

                workSheet.Cells[startRow, 11].Style.Numberformat.Format = numberFormatVND;

                if (criteria.Currency != "VND")
                {
                    for (int i = 12; i < 17; i++)
                    {
                        workSheet.Cells[startRow, i].Style.Numberformat.Format = numberFormatVND;
                    }
                }
                else
                {
                    for (int i = 12; i < 17; i++)
                    {
                        workSheet.Cells[startRow, i].Style.Numberformat.Format = numberFormats;
                    }
                }

                workSheet.Cells[startRow, 17].Value = item.PersonInCharge;
                workSheet.Cells[startRow, 18].Value = item.Salesman;
                workSheet.Cells[startRow, 19].Value = item.ServiceName;

                startRow += 1;
            }

            //Row total
            workSheet.Cells[startRow, 1].Value = "Total";
            workSheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(startRow).Style.Font.Bold = true;
            workSheet.Cells["A" + startRow + ":I" + startRow].Merge = true;
            workSheet.Cells[startRow, 11].Value = listData.Sum(su => su.Qty); //Total Qty
            workSheet.Cells[startRow, 11].Style.Numberformat.Format = numberFormatVND;
            workSheet.Cells[startRow, 12].Value = listData.Sum(su => su.ChargeWeight); //Total Charge Weight
            workSheet.Cells[startRow, 13].Value = listData.Sum(su => su.Revenue); //Total Revenue
            workSheet.Cells[startRow, 14].Value = listData.Sum(su => su.Cost); //Total Cost
            workSheet.Cells[startRow, 15].Value = listData.Sum(su => su.Profit); //Total Profit
            workSheet.Cells[startRow, 16].Value = listData.Sum(su => su.Obh); //Total OBH
            if (criteria.Currency != "VND")
            {
                for (int i = 12; i < 17; i++)
                {
                    workSheet.Cells[startRow, i].Style.Numberformat.Format = numberFormatVND;
                }
            }
            else
            {
                for (int i = 12; i < 17; i++)
                {
                    workSheet.Cells[startRow, i].Style.Numberformat.Format = numberFormats;
                }
            }

            workSheet.Cells[1, 1, startRow, 19].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[1, 1, startRow, 19].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[1, 1, startRow, 19].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[1, 1, startRow, 19].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
    }
}
