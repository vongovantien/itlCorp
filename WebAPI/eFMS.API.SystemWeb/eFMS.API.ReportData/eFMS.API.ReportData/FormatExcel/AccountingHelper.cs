using eFMS.API.Common.Globals;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Accounting;
using eFMS.API.ReportData.Models.Common.Enums;
using eFMS.API.ReportData.Models.Criteria;
using FMS.API.ReportData.Models.Accounting;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Controls;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace eFMS.API.ReportData.FormatExcel
{
    /// <summary>
    /// Accountant
    /// </summary>
    public class AccountingHelper
    {
        const double minWidth = 0.00;
        const double maxWidth = 500.00;
        //const string numberFormat = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-_(_)";
        const string numberFormat = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
        const string numberFormat2= "_(* #,##0_);_(* (#,##0);_(* \"-\"??_);_(@_)";
        const string numberFormatUSD = "_-* #,##0.000_-;-* #,##0.000_-;_-* \"-\"??_-;_-@_-_(_)";

        const string numberFormatVND = "_-\"VND\"* #,##0.00_-;-\"VND\"* #,##0.00_-;_-\"VND\"* \"-\"??_-;_-@_-_(_)";

        const string decimalFormat = "#,##0.00";
        const string decimalFormat2 = "#,##0";

        /// <summary>
        /// Get folder contain settlement payment template excel
        /// </summary>
        /// <returns></returns>
        private string GetSettleExcelFolder()
        {
            return Path.Combine(Consts.ResourceConsts.PathOfTemplateExcel, Consts.ResourceConsts.SettlementPath);
        }

        /// <summary>
        /// Get folder contain AR template excel
        /// </summary>
        /// <returns></returns>
        private string GetARExcelFolder()
        {
            return Path.Combine(Consts.ResourceConsts.PathOfTemplateExcel, Consts.ResourceConsts.AccountReceivablePath);
        }

        /// <summary>
        /// Generate advance payment excel
        /// </summary>
        /// <param name="listObj"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateAdvancePaymentExcel(List<AdvancePaymentModel> listObj, Stream stream = null)
        {
            List<string> headers = new List<string>()
            {
                "No",
                "Advance No",
                "Amount",
                "Currency",
                "Requester",
                "Request Date",
                "Deadline Date",
                "Modified Date",
                "Status Approval",
                "Status Payment",
                "Payment Menthod",
                "Description"
            };
            try
            {
                int addressStartContent = 4;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Advance Payment");
                    var worksheet = excelPackage.Workbook.Worksheets.First();

                    BuildHeader(worksheet, headers, "ADVANCE PAYMENT INFORMATION");

                    for (int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];
                        worksheet.Cells[i + addressStartContent, 1].Value = i + 1;
                        worksheet.Cells[i + addressStartContent, 2].Value = item.AdvanceNo;
                        worksheet.Cells[i + addressStartContent, 3].Value = item.Amount;
                        worksheet.Cells[i + addressStartContent, 4].Value = item.AdvanceCurrency;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.RequesterName;
                        worksheet.Cells[i + addressStartContent, 6].Value = item.RequestDate.HasValue ? item.RequestDate.Value.ToString("dd/MM/yyyy") : "";
                        worksheet.Cells[i + addressStartContent, 7].Value = item.DeadlinePayment.HasValue ? item.DatetimeModified.Value.ToString("dd/MM/yyyy") : "";
                        worksheet.Cells[i + addressStartContent, 8].Value = item.DatetimeModified.HasValue ? item.DatetimeModified.Value.ToString("dd/MM/yyyy") : "";
                        worksheet.Cells[i + addressStartContent, 9].Value = item.StatusApprovalName;
                        worksheet.Cells[i + addressStartContent, 10].Value = item.AdvanceStatusPayment.Equals("Settled") ? "Settled" : (item.AdvanceStatusPayment.Equals("NotSettled") ? "Not Settled" : "Partial Settlement");
                        worksheet.Cells[i + addressStartContent, 11].Value = item.PaymentMethodName;
                        worksheet.Cells[i + addressStartContent, 12].Value = item.AdvanceNote;

                        //Add border left right for cells
                        AddBorderLeftRightCell(worksheet, headers, addressStartContent, i);

                        //Add border bottom for last cells
                        AddBorderBottomLastCell(worksheet, headers, addressStartContent, i, listObj.Count);
                    }

                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void SetWidthColumnExcelAdvancePaymentShipment(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 5; //Cột A
            workSheet.Column(2).Width = 12; //Cột B
            workSheet.Column(3).Width = 15; //Cột C
            workSheet.Column(4).Width = 20; //Cột D
            workSheet.Column(5).Width = 20; //Cột E
            workSheet.Column(6).Width = 10; //Cột F
            workSheet.Column(7).Width = 14; //Cột G
            workSheet.Column(8).Width = 23; //Cột H
            workSheet.Column(9).Width = 19; //Cột I
            workSheet.Column(10).Width = 25; //Cột J
            workSheet.Column(11).Width = 20; //Cột K
            workSheet.Column(12).Width = 15; //Cột L
            workSheet.Column(13).Width = 15; //Cột M
            workSheet.Column(14).Width = 25; //Cột N
            workSheet.Column(14).Width = 15; //Cột P
            workSheet.Column(16).Width = 50; //Cột P
        }

        public Stream GenerateAdvancePaymentShipmentExcel(List<AdvancePaymentRequestModel> listObj, Stream stream = null)
        {
            List<string> headers = new List<string>()
            {
                "No",
                "Advance No",
                "Request Date",
                "Requester",
                "Advance Amount",
                "Currency",
                "Payment Method",
                "Deadline Payment",
                "Bank Account No",
                "Bank Account Name",
                "Bank Name",
                "Job ID",
                "MBL",
                "HBL",
                "Custom No",
                "Description",
                "Approve Date",
                "Settle Date",
            };

            try
            {
                int addressStartContent = 4;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Advance Payment");
                    var worksheet = excelPackage.Workbook.Worksheets.First();

                    BuildHeader(worksheet, headers, "Advance Payment");
                    SetWidthColumnExcelAdvancePaymentShipment(worksheet);

                    for (int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];
                        worksheet.Cells[i + addressStartContent, 1].Value = i + 1;
                        worksheet.Cells[i + addressStartContent, 2].Value = item.AdvanceNo;
                        worksheet.Cells[i + addressStartContent, 3].Value = item.RequestDate;
                        worksheet.Cells[i + addressStartContent, 3].Style.Numberformat.Format = "dd/MM/yyyy";
                        worksheet.Cells[i + addressStartContent, 4].Value = item.Requester;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.Amount;
                        if(item.RequestCurrency == "VND"){
                            worksheet.Cells[i + addressStartContent, 5].Style.Numberformat.Format = "#,##0";
                        }
                        else
                        {
                            worksheet.Cells[i + addressStartContent, 5].Style.Numberformat.Format = "#,##0.00";
                        }
                        worksheet.Cells[i + addressStartContent, 6].Value = item.RequestCurrency;
                        //
                        worksheet.Cells[i + addressStartContent, 7].Value = item.PaymentMethod;
                        worksheet.Cells[i + addressStartContent, 8].Value = item?.DeadlinePayment;
                        worksheet.Cells[i + addressStartContent, 8].Style.Numberformat.Format = "dd/MM/yyyy";
                        worksheet.Cells[i + addressStartContent, 9].Value = item.BankAccountNo;
                        worksheet.Cells[i + addressStartContent, 10].Value = item.BankAccountName;
                        worksheet.Cells[i + addressStartContent, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Fill;
                        worksheet.Cells[i + addressStartContent, 11].Value = item.BankName;
                        //
                        worksheet.Cells[i + addressStartContent, 12].Value = item.JobId;
                        worksheet.Cells[i + addressStartContent, 13].Value = item.Mbl;
                        worksheet.Cells[i + addressStartContent, 14].Value = item.Hbl;
                        worksheet.Cells[i + addressStartContent, 15].Value = item.CustomNo;
                        worksheet.Cells[i + addressStartContent, 16].Value = item.Description;
                        //worksheet.Cells[i + addressStartContent, 16].Style.HorizontalAlignment = ExcelHorizontalAlignment.Fill;
                        worksheet.Cells[i + addressStartContent, 17].Value = item.StatusPayment == "New" || item.StatusPayment == "Denied" ? null : item.ApproveDate;
                        worksheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = "dd/MM/yyyy"; //"dd/MM/yyyy  HH:mm:ss AM/PM";
                        worksheet.Cells[i + addressStartContent, 18].Value = item.SettleDate;
                        worksheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = "dd/MM/yyyy";


                        //Add border left right for cells
                        AddBorderLeftRightCell(worksheet, headers, addressStartContent, i);

                        //Add border bottom for last cells
                        AddBorderBottomLastCell(worksheet, headers, addressStartContent, i, listObj.Count);
                    }

                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Generate settlement payment excel
        /// </summary>
        /// <param name="listObj"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateSettlementPaymentExcel(List<SettlementPaymentModel> listObj, Stream stream = null)
        {
            List<string> headers = new List<string>()
            {
                "No",
                "Settlement No",
                "Amount",
                "Currency",
                "Requester",
                "Request Date",
                "Status Approval",
                "Payment Menthod",
                "Description"
            };
            try
            {
                int addressStartContent = 4;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Settlement Payment");
                    var worksheet = excelPackage.Workbook.Worksheets.First();

                    BuildHeader(worksheet, headers, "SETTLEMENT PAYMENT INFORMATION");

                    for (int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];
                        worksheet.Cells[i + addressStartContent, 1].Value = i + 1;
                        worksheet.Cells[i + addressStartContent, 2].Value = item.SettlementNo;
                        worksheet.Cells[i + addressStartContent, 3].Value = item.Amount;
                        worksheet.Cells[i + addressStartContent, 4].Value = item.SettlementCurrency;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.RequesterName;
                        worksheet.Cells[i + addressStartContent, 6].Value = item.RequestDate.HasValue ? item.RequestDate.Value.ToString("dd/MM/yyyy") : "";
                        worksheet.Cells[i + addressStartContent, 7].Value = item.StatusApprovalName;
                        worksheet.Cells[i + addressStartContent, 8].Value = item.PaymentMethodName;
                        worksheet.Cells[i + addressStartContent, 9].Value = item.Note;

                        //Add border left right for cells
                        AddBorderLeftRightCell(worksheet, headers, addressStartContent, i);

                        //Add border bottom for last cells
                        AddBorderBottomLastCell(worksheet, headers, addressStartContent, i, listObj.Count);
                    }

                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Stream GenerateSettlementPaymentShipmentExcel(List<SettlementExportGroupDefault> listObj, Stream stream = null)
        {
            List<string> headers = new List<string>()
            {
                "Job ID",
                "MBL",
                "HBL",
                "Custom No",
                "Settle No",
                "Request Date",
                "Requestor",
                "Settlement Amount",
                "AdvanceNo",
                "Advance Amount",
                "Balance",
                "Currency",
                "ApproveDate",
                "Description"
            };
            try
            {
                int addressStartContent = 4;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Settlement Payment");
                    var worksheet = excelPackage.Workbook.Worksheets.First();

                    BuildHeader(worksheet, headers, "SETTLEMENT PAYMENT INFORMATION");

                    for (int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];

                        worksheet.Cells[addressStartContent, 1].Value = item.JobID;
                        worksheet.Cells[addressStartContent, 2].Value = item.MBL;
                        worksheet.Cells[addressStartContent, 3].Value = item.HBL;
                        worksheet.Cells[addressStartContent, 4].Value = item.CustomNo;
                        worksheet.Cells[addressStartContent, 8].Value = item.SettlementTotalAmount;
                        worksheet.Cells[addressStartContent, 10].Value = item.AdvanceTotalAmount;
                        worksheet.Cells[addressStartContent, 11].Value = item.BalanceTotalAmount;

                        using (var range = worksheet.Cells[addressStartContent, 1, addressStartContent, 14])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                        }

                        for (int j = 0; j < item.requestList.Count; j++)
                        {
                            addressStartContent++;

                            var request = item.requestList[j];

                            worksheet.Cells[addressStartContent, 1].Value = request.JobID;
                            worksheet.Cells[addressStartContent, 2].Value = request.MBL;
                            worksheet.Cells[addressStartContent, 3].Value = request.HBL;
                            worksheet.Cells[addressStartContent, 4].Value = request.CustomNo;
                            worksheet.Cells[addressStartContent, 5].Value = request.SettleNo;
                            worksheet.Cells[addressStartContent, 6].Value = request.RequestDate.HasValue ? request.RequestDate.Value.ToString("dd/MM/yyyy") : "";
                            worksheet.Cells[addressStartContent, 7].Value = request.Requester;
                            worksheet.Cells[addressStartContent, 8].Value = request.SettlementAmount;
                            worksheet.Cells[addressStartContent, 9].Value = request.AdvanceNo;
                            worksheet.Cells[addressStartContent, 10].Value = request.AdvanceAmount;
                            worksheet.Cells[addressStartContent, 11].Value = request.AdvanceAmount - request.SettlementAmount;
                            worksheet.Cells[addressStartContent, 12].Value = request.Currency;
                            worksheet.Cells[addressStartContent, 13].Value = request.ApproveDate;
                            worksheet.Cells[addressStartContent, 13].Style.Numberformat.Format = "dd/MM/yyyy  HH:mm:ss AM/PM";
                            worksheet.Cells[addressStartContent, 14].Value = request.Description;

                        }
                        addressStartContent++;

                        //Add border left right for cells
                        // AddBorderLeftRightCell(worksheet, headers, addressStartContent, i);

                        //Add border bottom for last cells
                        // AddBorderBottomLastCell(worksheet, headers, addressStartContent, i, listObj.Count);
                    }


                    // Total

                    worksheet.Cells[addressStartContent + 3, 2].Value = "VND";
                    worksheet.Cells[addressStartContent + 3, 2].Style.Font.Bold = true;
                    worksheet.Cells[addressStartContent + 3, 2].Style.Font.Bold = true;

                    worksheet.Cells[addressStartContent + 3, 3].Value = "USD";
                    worksheet.Cells[addressStartContent + 3, 3].Style.Font.Bold = true;
                    worksheet.Cells[addressStartContent + 3, 3].Style.Font.Bold = true;

                    worksheet.Cells[addressStartContent + 4, 1].Value = "Total Settlement";
                    worksheet.Cells[addressStartContent + 4, 1].Style.Font.Bold = true;
                    worksheet.Cells[addressStartContent + 4, 1].Style.Font.Bold = true;

                    worksheet.Cells[addressStartContent + 4, 2].Value = listObj.Sum(s => s.SettlementTotalAmountVND);

                    worksheet.Cells[addressStartContent + 5, 1].Value = "Total Advance";
                    worksheet.Cells[addressStartContent + 5, 1].Style.Font.Bold = true;
                    worksheet.Cells[addressStartContent + 5, 2].Value = listObj.Sum(d => d.AdvanceTotalAmountVND);

                    worksheet.Cells[addressStartContent + 4, 3].Value = Math.Round((decimal)listObj.Sum(s => s.SettlementTotalAmountUSD),2);

                    worksheet.Cells[addressStartContent + 5, 3].Value = Math.Round((decimal)listObj.Sum(d => d.AdvanceTotalAmountUSD), 2);

                    worksheet.Cells[addressStartContent + 6, 1].Value = "Total Balance";
                    worksheet.Cells[addressStartContent + 6, 1].Style.Font.Bold = true;
                    worksheet.Cells[addressStartContent + 6, 2].Value = listObj.Sum(d => d.AdvanceTotalAmountVND)- listObj.Sum(s => s.SettlementTotalAmountVND);
                    worksheet.Cells[addressStartContent + 6, 3].Value = Math.Round((decimal)(listObj.Sum(d => d.AdvanceTotalAmountUSD) - listObj.Sum(s => s.SettlementTotalAmountUSD)), 2);

                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Set settlement detail data to excel
        /// </summary>
        /// <param name="settlementList"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Stream ExportSettlementPaymentDetailSurCharges(List<AccountingSettlementExportGroup> settlementList, string fileName)
        {
            try
            {
                var folderOfFile = GetSettleExcelFolder();
                FileInfo f = new FileInfo(Path.Combine(folderOfFile, fileName));
                var path = f.FullName;
                if (!File.Exists(path))
                {
                    return null;
                }
                var excel = new ExcelExport(path);
                var startRow = 4;
                excel.StartDetailTable = startRow;
                excel.NumberOfGroup = 2;

                if (settlementList.Count == 0)
                {
                    settlementList.Add(new AccountingSettlementExportGroup());
                    settlementList.FirstOrDefault().ShipmentDetail.Add(new ShipmentSettlementExportGroup());
                }
                if (settlementList.FirstOrDefault().ShipmentDetail == null || settlementList.Count(x => x.ShipmentDetail.Count() > 0) == 0)
                {
                    excel.DeleteRow(6);
                }
                foreach (var settle in settlementList)
                {
                    excel.IndexOfGroup = 1; // Set index of group 1
                    excel.SetGroupsTable(); // Set group 1
                    var listKeyData = new Dictionary<string, object>()
                    {
                        {"SettlementNo", settle.SettlementNo},
                        {"Requester", settle.Requester},
                        {"RequestDate", settle.RequestDate?.ToString("dd/MM/yyyy")},
                        {"NetAmountG1", settle.TotalNetAmount},
                        {"VatAmountG1", settle.TotalVatAmount},
                        {"TotalAmountG1", settle.TotalAmount},
                        {"TotalAmountVndG1", settle.TotalAmountVnd},
                        {"AdvanceAmountG1", settle.TotalAdvanceAmount},
                        {"BalanceG1", settle.TotalAdvanceAmount - (settle.SettlementAmount ?? 0)},
                        {"ApproveDate", settle.ApproveDate?.ToString("dd/MM/yyyy")},
                        {"PaymentMethod", settle.PaymentMethod},
                        {"DueDate", settle.DueDate?.ToString("dd/MM/yyyy")},
                        {"BankAccountNo", settle.BankAccountNo},
                        {"BankAccountName", settle.BankAccountName},
                        {"BankName", settle.BankName},
                    };
                    excel.SetData(listKeyData);
                    startRow++;
                    foreach (var shipment in settle.ShipmentDetail)
                    {
                        excel.IndexOfGroup = 2; // Set index of group 2
                        excel.SetGroupsTable(); // Set group 2
                        listKeyData = new Dictionary<string, object>()
                        {
                            {"JobIDG2", shipment.JobID},
                            {"MBLG2", shipment.MBL},
                            {"HBLG2", shipment.HBL},
                            {"CustomNoG2", shipment.CustomNo},
                            {"NetAmountG2", shipment.NetAmount},
                            {"VatAmountG2", shipment.VatAmount},
                            {"TotalAmountG2", shipment.TotalAmount},
                            {"TotalAmountVndG2", shipment.TotalAmountVnd},
                            {"AdvanceNo", shipment.AdvanceNo},
                            {"AdvanceAmountG2", shipment.AdvanceAmount ?? 0},
                            {"BalanceG2", (shipment.Balance ?? 0)}
                        };
                        excel.SetData(listKeyData);
                        startRow++;
                        foreach (var charge in shipment.surchargesDetail)
                        {
                            excel.SetDataTable();
                            listKeyData = new Dictionary<string, object>()
                            {
                                {"JobID", shipment.JobID},
                                {"MBL", shipment.MBL},
                                {"HBL", shipment.HBL},
                                {"CustomNo", shipment.CustomNo},
                                {"ChargeCode", charge.ChargeCode},
                                {"ChargeName", charge.ChargeName},
                                {"Quantity", charge.Quantity},
                                {"ChargeUnit", charge.ChargeUnit},
                                {"UnitPrice", charge.UnitPrice},
                                {"Currency", charge.CurrencyId},
                                {"NetAmount", charge.NetAmount},
                                {"Vatrate", charge.Vatrate},
                                {"VatAmount", charge.VatAmount},
                                {"TotalAmount", charge.TotalAmount},
                                {"TotalAmountVnd", charge.TotalAmountVnd},
                                {"Payee", charge.Payee},
                                {"OBHPartnerName", charge.OBHPartnerName},
                                {"InvoiceNo", charge.InvoiceNo},
                                {"SeriesNo", charge.SeriesNo},
                                {"InvoiceDate", charge.InvoiceDate?.ToString("dd/MM/yyyy")},
                                {"VatPartner", charge.VatPartner}
                            };
                            excel.SetData(listKeyData);
                            startRow++;
                        }
                    }

                }
                // Set total
                var listKeyTotal = new Dictionary<string, object>();
                var totalSettleAmount = settlementList.Sum(x => x.SettlementAmount ?? 0);
                var totalAdvAmount = settlementList.Sum(x => x.TotalAdvanceAmount ?? 0);
                listKeyTotal.Add("TotalSettle", totalSettleAmount);
                listKeyTotal.Add("TotalAdv", totalAdvAmount);
                listKeyTotal.Add("TotalBalance", totalAdvAmount - totalSettleAmount);
                excel.SetData(listKeyTotal);
                return excel.ExcelStream();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Stream GenerateInvoicePaymentShipmentExcel(List<AccountingPaymentModel> listObj, Stream stream = null)
        {
            List<string> headers = new List<string>()
            {

                "Reference No",
                "Partner Name",
                "Invoice Amount",
                "Currency",
                "Invoice Date",
                "Serie No",
                "Paid Amount",
                "Unpaid Amount",

                "Due Date",
                "Overdue Days",
                "Payment Status",
                "Extend Days",
                "Extend Note"
            };
            try
            {
                int addressStartContent = 4;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Invoice Payment");
                    var worksheet = excelPackage.Workbook.Worksheets.First();

                    BuildHeader(worksheet, headers,"INVOICE PAYMENT INFORMATION");
                    for(int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];

                        worksheet.Cells[addressStartContent, 1].Value = item.InvoiceNoReal;

                        worksheet.Cells[addressStartContent, 2].Value = item.PartnerName;
                        worksheet.Cells[addressStartContent, 3].Value = item.Amount.HasValue ? item.Amount.Value : 0;
                        worksheet.Cells[addressStartContent, 3].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[addressStartContent, 4].Value = item.Currency;
                        worksheet.Cells[addressStartContent, 5].Value = item.IssuedDate.HasValue ? item.IssuedDate.Value.ToString("dd/MM/yyyy") : "";
                        worksheet.Cells[addressStartContent, 6].Value = item.Serie;

                        worksheet.Cells[addressStartContent, 7].Value = item.PaidAmount.HasValue ? item.PaidAmount.Value : 0 ;
                        worksheet.Cells[addressStartContent, 7].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[addressStartContent, 8].Value = item.UnpaidAmount.HasValue ? item.UnpaidAmount.Value : 0;
                        worksheet.Cells[addressStartContent, 8].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[addressStartContent, 9].Value = item.DueDate.HasValue ? item.DueDate.Value.ToString("dd/MM/yyyy") : "";
                        worksheet.Cells[addressStartContent, 10].Value = item.OverdueDays;
                        worksheet.Cells[addressStartContent, 11].Value = item.Status;
                        worksheet.Cells[addressStartContent, 12].Value = item.ExtendDays;
                        worksheet.Cells[addressStartContent, 13].Value = item.ExtendNote;
                        addressStartContent++;
                    }
                    excelPackage.Save();
                    return excelPackage.Stream;
                }

            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public Stream GenerateAccountingManagementDebCreInvExcel(List<AccountingManagementExport> acctMngts, string typeOfAcctMngt, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Sheet1");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BindingDataAccoutingManagementDebCreInvExcel(workSheet, acctMngts, typeOfAcctMngt);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        /// <summary>
        /// GenerateExportCutomerHistoryPayment
        /// </summary>
        /// <param name="customerPayment"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Stream GenerateExportCustomerHistoryPayment(List<AccountingCustomerPaymentExport> customerPayment, AccountingPaymentCriteria paymentCriteria, string fileName)
        {
            var folderOfFile = GetARExcelFolder();
            FileInfo f = new FileInfo(Path.Combine(folderOfFile, fileName));
            var path = f.FullName;
            if (!File.Exists(path))
            {
                return null;
            }
            var excel = new ExcelExport(path);
            try
            {
                int startRow = 2;
                var listKeyData = new Dictionary<string, object>();
                if (paymentCriteria.IssuedDate != null)
                {
                    listKeyData.Add("RangeDate", string.Format("Từ ngày {0} đến ngày {0}", paymentCriteria.IssuedDate.Value.ToString("dd/MM/yyyy")));
                }
                else if (paymentCriteria.FromUpdatedDate != null)
                {
                    listKeyData.Add("RangeDate", string.Format("Từ ngày {0} đến ngày {1}", paymentCriteria.FromUpdatedDate.Value.ToString("dd/MM/yyyy")
                        , paymentCriteria.ToUpdatedDate.Value.ToString("dd/MM/yyyy")));
                }
                else if (paymentCriteria.DueDate != null)
                {
                    listKeyData.Add("RangeDate", string.Format("Từ ngày {0} đến ngày {0}", paymentCriteria.DueDate.Value.ToString("dd/MM/yyyy")));
                }
                else
                {
                    listKeyData.Add("RangeDate", string.Format("Từ ngày {0} đến ngày {0}", DateTime.Now.ToString("dd/MM/yyyy")));
                }
                excel.SetData(listKeyData);

                startRow = 6;
                excel.StartDetailTable = startRow;
                excel.NumberOfGroup = 2;
                if (customerPayment.Count == 0)
                {
                    customerPayment.Add(new AccountingCustomerPaymentExport());
                }
                var isExistDetail = true;
                var isExistAdvRow = true;
                if (customerPayment.FirstOrDefault().receiptDetail == null || customerPayment.Count(x => x.receiptDetail != null && x.receiptDetail.Count() > 0) == 0 || paymentCriteria.DueDate != null || paymentCriteria.FromUpdatedDate == null)
                {
                    isExistDetail = false;
                }
                if (customerPayment.Where(x => x.BillingRefNo == "ADVANCE AMOUNT").Count() == 0)
                {
                    isExistAdvRow = false;
                }
                if (!isExistDetail)
                {
                    excel.DeleteRow(7);
                    if (!isExistAdvRow)
                    {
                        excel.DeleteRow(7);
                    }
                }
                else if(!isExistAdvRow)
                {
                    excel.DeleteRow(8);
                }

                var sumRemainDb = 0m;
                var sumRemainObh = 0m;
                var sumRemainDbUsd = 0m;
                var sumRemainObhUsd = 0m;
                var sumAdvanceAmountVnd = 0m;
                var sumAdvanceAmountUsd = 0m;
                for (int i = 0; i < customerPayment.Count; i++)
                {
                    var item = customerPayment[i];
                    listKeyData = new Dictionary<string, object>();
                    if (item.BillingRefNo != "ADVANCE AMOUNT")
                    {
                        excel.IndexOfGroup = 1;
                        excel.SetGroupsTable();
                        listKeyData.Add("PartnerCode", item.PartnerCode);
                        listKeyData.Add("ACRefCode", item.ParentCode);
                        listKeyData.Add("PartnerName", item.PartnerName);
                        listKeyData.Add("InvoiceDate", item.InvoiceDate);
                        listKeyData.Add("InvoiceNo", item.InvoiceNo);
                        listKeyData.Add("SoaNo", item.BillingRefNo);
                        //listKeyData.Add("BillingDate", item.BillingDate);
                        listKeyData.Add("UnpaidAmount", item.UnpaidAmountInv);
                        listKeyData.Add("OBHUnpaidAmount", item.UnpaidAmountOBH);
                        listKeyData.Add("PaidAmount", item.PaidAmount);
                        listKeyData.Add("OBHPaidAmount", item.PaidAmountOBH);
                        var remainDb = (item.UnpaidAmountInv ?? 0) - (item.PaidAmount ?? 0);
                        var remainObh = (item.UnpaidAmountOBH ?? 0) - (item.PaidAmountOBH ?? 0);
                        var remainDbUsd = (item.UnpaidAmountInvUsd ?? 0) - (item.PaidAmountUsd ?? 0);
                        var remainObhUsd = (item.UnpaidAmountOBHUsd ?? 0) - (item.PaidAmountOBHUsd ?? 0);
                        remainDb = remainDb < 0 ? 0 : remainDb;
                        remainObh = remainObh < 0 ? 0 : remainObh;
                        remainDbUsd = remainDbUsd < 0 ? 0 : remainDbUsd;
                        remainObhUsd = remainObhUsd < 0 ? 0 : remainObhUsd;
                        // Sum total
                        sumRemainDb += remainDb;
                        sumRemainObh += remainObh;
                        sumRemainDbUsd += remainDbUsd;
                        sumRemainObhUsd += remainObhUsd;
                        listKeyData.Add("RemainDb", remainDb);
                        listKeyData.Add("RemainOBH", remainObh);
                        listKeyData.Add("RemainDbUsd", remainDbUsd);
                        listKeyData.Add("RemainOBHUsd", remainObhUsd);
                        listKeyData.Add("TotalAmount", remainDb + remainObh);
                        listKeyData.Add("TotalAmountUsd", remainDbUsd + remainObhUsd);
                        listKeyData.Add("PaymentTerm", item.PaymentTerm?.ToString("N0"));
                        listKeyData.Add("DueDate", item.DueDate?.ToString("dd/MM/yy"));
                        listKeyData.Add("OverdueDays", item.OverdueDays?.ToString("N0"));
                        listKeyData.Add("JobNo", item.JobNo);
                        listKeyData.Add("MBL", item.MBL);
                        listKeyData.Add("HBL", item.HBL);
                        listKeyData.Add("CustomNo", item.CustomNo);
                        listKeyData.Add("AccountNo", item.AccountNo);
                        listKeyData.Add("Branch", item.BranchName);
                        listKeyData.Add("Salesman", item.Salesman);
                        listKeyData.Add("Creator", item.Creator);
                    }
                    else
                    {
                        excel.IndexOfGroup = 2;
                        excel.SetGroupsTable();
                        sumAdvanceAmountVnd += (item.AdvanceAmountVnd ?? 0);
                        sumAdvanceAmountUsd += (item.AdvanceAmountUsd ?? 0);
                        listKeyData.Add("PartnerCodeAdv", item.PartnerCode);
                        listKeyData.Add("ACRefCodeAdv", item.ParentCode);
                        listKeyData.Add("PartnerNameAdv", item.PartnerName);
                        listKeyData.Add("SoaNoAdv", item.BillingRefNo);
                        listKeyData.Add("AdvanceAmountVnd", item.AdvanceAmountVnd);
                        listKeyData.Add("TotalAmountAdv", 0);
                        listKeyData.Add("AdvanceAmountUsd", item.AdvanceAmountUsd);
                        listKeyData.Add("TotalAmountUsdAdv", 0);
                        listKeyData.Add("BranchAdv", item.BranchName);
                    }
                    excel.SetData(listKeyData);
                    startRow++;
                    if (item.receiptDetail != null && paymentCriteria.DueDate == null && paymentCriteria.FromUpdatedDate != null)
                    {
                        foreach (var detail in item.receiptDetail)
                        {
                            listKeyData = new Dictionary<string, object>();
                            excel.SetDataTable();
                            listKeyData.Add("PartnerCodeDt", item.PartnerCode);
                            listKeyData.Add("ACRefCodeDt", item.ParentCode);
                            listKeyData.Add("PartnerNameDt", item.PartnerName);
                            listKeyData.Add("InvoiceDatedt", item.InvoiceDate);
                            listKeyData.Add("InvoiceNodt", item.InvoiceNo);
                            listKeyData.Add("SoaNodt", item.BillingRefNo);
                            listKeyData.Add("BillingDatedt", item.BillingDate);
                            listKeyData.Add("PaidAmountDt", detail.PaidAmount);
                            listKeyData.Add("PaidAmountOBHDt", detail.PaidAmountOBH);
                            listKeyData.Add("PaidDate", detail.PaymentDate);
                            listKeyData.Add("ReceiptNo", detail.PaymentRefNo);
                            listKeyData.Add("JobNodt", item.JobNo);
                            listKeyData.Add("MBLdt", item.MBL);
                            listKeyData.Add("HBLdt", item.HBL);
                            listKeyData.Add("CustomNodt", item.CustomNo);
                            excel.SetData(listKeyData);
                            startRow++;
                        }
                    }
                }
                var listKeyTotal = new Dictionary<string, object>();
                listKeyTotal.Add("SumUnpaidAmount", customerPayment.Sum(x => (x.UnpaidAmountInv ?? 0)));
                listKeyTotal.Add("SumOBHUnpaidAmount", customerPayment.Sum(x => (x.UnpaidAmountOBH ?? 0)));
                listKeyTotal.Add("SumPaidAmount", customerPayment.Sum(x => (x.PaidAmount ?? 0)));
                listKeyTotal.Add("SumOBHPaidAmount", customerPayment.Sum(x => (x.PaidAmountOBH ?? 0)));
                // Sum total VND
                listKeyTotal.Add("SumRemainDb", sumRemainDb);
                listKeyTotal.Add("SumRemainOBH", sumRemainObh);
                listKeyTotal.Add("SumTotalAmount", sumRemainDb + sumRemainObh);
                // Sum Advance Amount
                listKeyTotal.Add("SumAdvanceAmountVnd", sumAdvanceAmountVnd);
                listKeyTotal.Add("SumAdvanceAmountUsd", sumAdvanceAmountUsd);
                // Sum total USD
                listKeyTotal.Add("SumRemainDbUsd", sumRemainDbUsd);
                listKeyTotal.Add("SumRemainOBHUsd", sumRemainObhUsd);
                listKeyTotal.Add("SumTotalAmountUsd", sumRemainDbUsd + sumRemainObhUsd);
                excel.SetData(listKeyTotal);
                return excel.ExcelStream();
            }
            catch (Exception ex)
            {
                excel.PackageExcel.Dispose();
                return null;
            }
        }

        public Stream GenerateOBHPaymentShipmentExcel(List<AccountingPaymentModel> listObj, Stream stream = null)
        {
            List<string> headers = new List<string>()
            {

                "Reference No",

                "Partner Name",
                "OBH Amount",
                "Currency",
                "Issue Date",

                "Paid Amount",
                "Unpaid Amount",
                "Due Date",
                "Overdue Days",
                "Payment Status",
                "Extend Days",
                "Extend Note"
            };
            try
            {
                int addressStartContent = 4;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("OBH Payment");
                    var worksheet = excelPackage.Workbook.Worksheets.First();

                    BuildHeader(worksheet, headers,"OBH PAYMENT INFORMATION");
                    for (int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];

                        worksheet.Cells[addressStartContent, 1].Value = item.SOANo;

                        worksheet.Cells[addressStartContent, 2].Value = item.PartnerName;
                        worksheet.Cells[addressStartContent, 3].Value = item.Amount.HasValue ? item.Amount.Value : 0;
                        worksheet.Cells[addressStartContent, 3].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[addressStartContent, 4].Value = item.Currency;
                        worksheet.Cells[addressStartContent, 5].Value = item.IssuedDate.HasValue ? item.IssuedDate.Value.ToString("dd/MM/yyyy") : "";

                        worksheet.Cells[addressStartContent, 6].Value = item.PaidAmount.HasValue ? item.PaidAmount.Value : 0;
                        worksheet.Cells[addressStartContent, 6].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[addressStartContent, 7].Value = item.UnpaidAmount.HasValue ? item.UnpaidAmount.Value : 0;
                        worksheet.Cells[addressStartContent, 7].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[addressStartContent, 8].Value = item.DueDate.HasValue ? item.DueDate.Value.ToString("dd/MM/yyyy") : "";
                        worksheet.Cells[addressStartContent, 9].Value = item.OverdueDays;
                        worksheet.Cells[addressStartContent, 10].Value = item.Status;
                        worksheet.Cells[addressStartContent, 11].Value = item.ExtendDays;
                        worksheet.Cells[addressStartContent, 12].Value = item.ExtendNote;
                        addressStartContent++;
                    }
                    excelPackage.Save();
                    return excelPackage.Stream;
                }

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private void AddBorderBottomLastCell(ExcelWorksheet worksheet, List<string> headers, int addressStartContent, int indexDataRow, int totalItem)
        {
            if (indexDataRow == totalItem - 1)
            {
                for (int j = 0; j < headers.Count; j++)
                {
                    worksheet.Cells[indexDataRow + addressStartContent, j + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
            }
        }

        private void AddBorderLeftRightCell(ExcelWorksheet worksheet, List<string> headers, int addressStartContent, int indexDataRow)
        {
            for (int j = 0; j < headers.Count; j++)
            {
                worksheet.Cells[indexDataRow + addressStartContent, j + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[indexDataRow + addressStartContent, j + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }
        }

        /// <summary>
        /// Build header
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="headers"></param>
        /// <param name="title"></param>
        public void BuildHeader(ExcelWorksheet worksheet, List<String> headers, string title)
        {
            worksheet.Cells[1, 1, 1, headers.Count].Merge = true;
            worksheet.Cells["A1"].Value = title;
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            // Tạo header
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];

                //worksheet.Column(i + 1).AutoFit();
                worksheet.Cells[3, i + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Font.Bold = true;

                worksheet.Cells[3, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                worksheet.Column(i + 1).Width = 30;
            }
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        #region --- ADVANCE PAYMENT ---
        /// <summary>
        /// Generate detail advance payment excel
        /// </summary>
        /// <param name="advanceExport"></param>
        /// <param name="language"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateDetailAdvancePaymentExcel(AdvanceExport advanceExport, string language, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    string sheetName = language == "VN" ? "(V)" : "(E)";
                    excelPackage.Workbook.Worksheets.Add("Đề nghị tạm ứng " + sheetName);
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BindingDataDetailAdvancePaymentExcel(workSheet, advanceExport, language);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void SetWidthColumnExcelDetailAdvancePayment(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 8; //Cột A
            workSheet.Column(2).Width = 21; //Cột B
            workSheet.Column(3).Width = 25; //Cột C
            workSheet.Column(4).Width = 20; //Cột D
            workSheet.Column(5).Width = 15; //Cột E
            workSheet.Column(8).Width = 15; //Cột H
            workSheet.Column(9).Width = 20; //Cột I
            workSheet.Column(10).Width = 18; //Cột J
            workSheet.Column(11).Width = 20; //Cột K
            workSheet.Column(12).Width = 20; //Cột L
        }

        private List<string> GetHeaderExcelDetailAdvancePayment(string language)
        {
            List<string> vnHeaders = new List<string>()
            {
                "INDO TRANS LOGISTICS CORPORATION", //0
                "52-54-65 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
                "PHIẾU ĐỀ NGHỊ TẠM ỨNG", //2
                "Người yêu cầu:", //3
                "Ngày:", //4
                "Bộ phận:", //5
                "STT", //6
                "Thông tin chung", //7
                "Qty", //8
                "Số tiền tạm ứng (VND)", //9
                "Số cont - Loại cont", //10
                "C.W (Kgs)", //11
                "Số kiện\n(Pcs)", //12
                "Số CBM\n(CBM)", //13
                "Định mức", //14
                "Chi phí có hóa đơn", //15
                "Chi phí khác", //16
                "Tổng cộng", //17
                "Số chứng từ:", //18
                "Số lô hàng:", //19
                "Số tờ khai:", //20
                "Số HBL/HAWB:", //21
                "Số MBL/MAWB:", //22
                "Khách hàng:", //23
                "Công ty/cá nhân xuất:", //24
                "Công ty/cá nhân nhập:", //25
                "Số tiền đề nghị tạm ứng:", //26
                "Số tiền viết bằng chữ:", //27
                "Lý do tạm ứng:", //28
                "Thời hạn thanh toán:", //29
                "Người tạm ứng\n(Ký, ghi rõ họ tên)", //30
                "Người chứng từ\n(Ký, ghi rõ họ tên)", //31
                "Trưởng bộ phận\n(Ký, ghi rõ họ tên)", //32
                "Kế toán\n(Ký, ghi rõ họ tên)", //33
                "Giám đốc\n(Ký, ghi rõ họ tên)", //34
                "Chứng từ:", //35

                //16/06/2021 - #15834 - Add Field Template Export
                //ADD
                "Chuyển khoản",//36
                "Tên người thụ hưởng",//37
                "Số ngân hàng",//38
                "Tên ngân hàng",//39
                "Mã ngân hàng",//40
                "Tiền mặt",//41
                "Ngày đến hạn",//42
                //END

                //20/09/2021 - #16169 - Add Field Service Date
                //ADD
                "Ngày dịch vụ:",//43
                "Hình thức thanh toán:",//44
                "Đối tượng thanh toán:",//45
                //END
                "Ghi chú", //46
            };

            List<string> engHeaders = new List<string>()
            {
                "INDO TRANS LOGISTICS CORPORATION", //0
                "52-54-65 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
                "ADVANCE REQUEST", //2
                "Employee Name:", //3
                "Date requested:", //4
                "Department:", //5
                "No.", //6
                "Shipment's Information", //7
                "Qty", //8
                "Advance Amount (VND)", //9
                "Con't", //10
                "C.W (Kgs)", //11
                "Packages\n(PCS)", //12
                "CBM", //13
                "Costs according to tariff", //14
                "Costs with reasonable vouchers", //15
                "Others", //16
                "Total", //17
                "Ref No.:", //18
                "Job ID:", //19
                "Customs Clearance No:", //20
                "HBL/HAWB:", //21
                "MBL/MAWB:", //22
                "Customer:", //23
                "Consigner:", //24
                "Consignee:", //25
                "Total advance amount:", //26
                "In words:", //27
                "Advance Reasons:", //28
                "Due date:", //29
                "Employee\n(Name, Signature)", //30
                "Documentation Staff\n(Name, Signature)", //31
                "Head of Department\n(Name, Signature)", //32
                "Chief Accountant\n(Name, Signature)", //33
                "Director\n(Name, Signature)", //34
                "Doc CS:", //35

                 //16/06/2021 - #15834 - Add Field Template Export
                //ADD
                "By Bank transfer",//36
                "Beneficiary",//37
                "Acc No",//38
                "Bank",//39
                "Bank code",//40
                "By cash",//41
                "Due date",//42
                 //END

                //20/09/2021 - #16169 - Add Field Service Date
                //ADD
                "Service date:",//43
                "Payment Method:",//44
                "Supplier Name:",//45
                //END
                 "Note",    //46
            };

            List<string> headers = language == "VN" ? vnHeaders : engHeaders;
            return headers;
        }

        private void BindingDataDetailAdvancePaymentExcel(ExcelWorksheet workSheet, AdvanceExport advanceExport, string language)
        {
            workSheet.Cells.Style.Font.SetFromFont(new Font("Times New Roman", 11));
            workSheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            SetWidthColumnExcelDetailAdvancePayment(workSheet);

            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column B
                excelImage.SetPosition(0, 0, 1, 0);
            }

            List<string> headers = GetHeaderExcelDetailAdvancePayment(language);

            workSheet.Cells["H1:K1"].Merge = true;
            workSheet.Cells["H1"].Value = headers[0];
            workSheet.Cells["H1"].Style.Font.SetFromFont(new Font("Arial Black", 12));
            workSheet.Cells["H1"].Style.Font.Italic = true;
            workSheet.Cells["H2:K2"].Merge = true;
            workSheet.Cells["H2:K2"].Style.WrapText = true;
            workSheet.Cells["H2"].Value = advanceExport.InfoAdvance.ContactOffice;
            workSheet.Cells["H2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10));
            workSheet.Cells["H2"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Row(2).Height = 60;

            //Title
            workSheet.Cells["A4:K4"].Merge = true;
            workSheet.Cells["A4"].Value = headers[2]; //Phiếu đề nghị tạm ứng
            workSheet.Cells["A4"].Style.Font.Size = 18;
            workSheet.Cells["A4"].Style.Font.Bold = true;
            workSheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A5:B5"].Merge = true;
            workSheet.Cells["A5"].Value = headers[3]; //Người yêu cầu
            workSheet.Cells["A5"].Style.Font.SetFromFont(new Font("Times New Roman", 11));
            workSheet.Cells["A5"].Style.Font.Bold = true;
            workSheet.Cells["C5"].Value = advanceExport.InfoAdvance.Requester;

            workSheet.Cells["J5"].Value = headers[4]; //Ngày
            workSheet.Cells["J5"].Style.Font.Bold = true;
            workSheet.Cells["K5"].Value = advanceExport.InfoAdvance.RequestDate;
            workSheet.Cells["K5"].Style.Numberformat.Format = "dd MMM, yyyy";
            workSheet.Cells["K5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            workSheet.Cells["A6"].Value = headers[5]; //Bộ phận
            workSheet.Cells["A6"].Style.Font.Bold = true;
            workSheet.Cells["C6"].Value = advanceExport.InfoAdvance.Department;

            workSheet.Cells["J9"].Value = headers[36];
            workSheet.Cells["J9"].Style.Font.Bold = true;
            workSheet.Cells["K9"].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;

            workSheet.Cells["B10"].Value = headers[41];
            workSheet.Cells["B10"].Style.Font.Bold = true;

            //var check = workSheet.Drawings.AddCheckBoxControl("");

            if (advanceExport.InfoAdvance.PaymentMethod == "Cash")
            {
                //check.SetPosition(7, 15, 2, 0);
                //check.Checked = eCheckState.Checked;
                workSheet.Cells["K9"].Value = "X";
                workSheet.Cells["K9"].Style.Font.Bold = true;
            }
            else if (advanceExport.InfoAdvance.PaymentMethod == "Bank")
            {
                // check.SetPosition(7, 15, 10, 0);
                // check.Checked = eCheckState.Checked;
                workSheet.Cells["K9"].Value = "X";
                workSheet.Cells["K9"].Style.Font.Bold = true;
            }

            workSheet.Cells["J10"].Value = headers[37];
            workSheet.Cells["J10"].Style.Font.Bold = true;
            //workSheet.Cells["C10"].Value = advanceExport.InfoAdvance.BankAccountName;
            workSheet.Cells["K10"].Value = advanceExport.InfoAdvance.BankAccountName;
            workSheet.Cells["K10"].Style.WrapText = true;

            workSheet.Cells["A8:B8"].Merge = true;
            workSheet.Cells["A8:B8"].Value = headers[42];
            workSheet.Cells["A8:B8"].Style.Font.Bold = true;

            workSheet.Cells["A9:B9"].Merge = true;
            workSheet.Cells["A9:B9"].Value = headers[44];
            workSheet.Cells["A9:B9"].Style.Font.Bold = true;

            workSheet.Cells["C8"].Value = advanceExport.InfoAdvance.DeadlinePayment;
            workSheet.Cells["C8"].Style.Numberformat.Format = "dd MMM, yyyy";
            workSheet.Cells["C8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            workSheet.Cells["J11"].Value = headers[38];
            workSheet.Cells["J11"].Style.Font.Bold = true;
            workSheet.Cells["K11"].Value = advanceExport.InfoAdvance.BankAccountNo;

            workSheet.Cells["J12"].Value = headers[39];
            workSheet.Cells["J12"].Style.Font.Bold = true;
            workSheet.Cells["K12"].Value = advanceExport.InfoAdvance.BankName;
            workSheet.Cells["K12"].Style.WrapText = true;

            workSheet.Cells["J13"].Value = headers[40];
            workSheet.Cells["J13"].Style.Font.Bold = true;
            workSheet.Cells["K13"].Value = advanceExport.InfoAdvance.BankCode;


            //Bôi đen header
            workSheet.Cells["A15:K16"].Style.Font.Bold = true;

            workSheet.Cells[15, 1, 16, 1].Merge = true;
            workSheet.Cells[15, 1, 16, 1].Value = headers[6];//STT
            workSheet.Cells[15, 1, 16, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[15, 1, 16, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[15, 2, 16, 3].Merge = true;
            workSheet.Cells[15, 2, 16, 3].Value = headers[7];//Thông tin chung
            workSheet.Cells[15, 2, 16, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[15, 2, 16, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[15, 4, 15, 7].Merge = true;
            workSheet.Cells[15, 4, 15, 7].Value = headers[8];//Qty
            workSheet.Cells[15, 4, 15, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[15, 4, 15, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            for (int x = 4; x < 8; x++)
            {
                workSheet.Cells[16, x].Style.WrapText = true;
                workSheet.Cells[16, x].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[16, x].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            workSheet.Cells[16, 4].Value = headers[10]; //Số cont - Loại cont
            workSheet.Cells[16, 5].Value = headers[11]; // C.W
            workSheet.Cells[16, 6].Value = headers[12]; //Số kiện
            workSheet.Cells[16, 7].Value = headers[13]; //số CBM

            workSheet.Cells[15, 1, 15, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[15, 8, 15, 11].Merge = true;
            workSheet.Cells[15, 8, 15, 11].Value = headers[9];//Số tiến tạm ứng
            workSheet.Cells[15, 8, 15, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[15, 8, 15, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            for (int x = 8; x < 12; x++)
            {
                workSheet.Cells[16, x].Style.WrapText = true;
                workSheet.Cells[16, x].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[16, x].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            workSheet.Cells[16, 8].Value = headers[14]; //Định mức
            workSheet.Cells[16, 9].Value = headers[15]; // Chi phí có hóa đơn
            workSheet.Cells[16, 10].Value = headers[16]; //Chi phí khác
            workSheet.Cells[16, 11].Value = headers[17]; //Tổng cộng

            workSheet.Cells[15, 12, 16, 12].Merge = true;
            workSheet.Cells[15, 12, 16, 12].Value = headers[46];//Note
            workSheet.Cells[15, 12, 16, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[15, 12, 16, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            int p = 17;
            int j = 17;
            for (int i = 0; i < advanceExport.ShipmentsAdvance.Count; i++)
            {
                workSheet.Cells[j, 2].Value = headers[18]; //Số chứng từ
                workSheet.Cells[j, 3].Value = advanceExport.InfoAdvance.AdvanceNo;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[19]; //Số lô hàng
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].JobNo;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[43]; //Ngày dịch vụ
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].ServiceDate;
                workSheet.Cells[j, 3].Style.Numberformat.Format = "dd MMM, yyyy";
                workSheet.Cells[j, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[20]; //Số tờ khai
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].CustomNo;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[21]; //Số HBL
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].HBL;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[22]; //Số MBL
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].MBL;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[23]; //Khách hàng
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].Customer;
                workSheet.Cells[j, 3].Style.WrapText = true;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[24]; //Công ty xuất
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].Shipper;
                workSheet.Cells[j, 3].Style.WrapText = true;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[25]; //Công ty nhập
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].Consignee;
                workSheet.Cells[j, 3].Style.WrapText = true;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[35]; //Chứng từ
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].PersonInCharge;
                workSheet.Cells[j, 3].Style.WrapText = true;
                j = j + 1;
                /////
                workSheet.Cells[p, 1, j - 1, 1].Merge = true;
                workSheet.Cells[p, 1, j - 1, 1].Value = i + 1; //Value STT
                workSheet.Cells[p, 1, j - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 1, j - 1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                workSheet.Cells[p, 4, j - 1, 4].Merge = true;
                workSheet.Cells[p, 4, j - 1, 4].Value = advanceExport.ShipmentsAdvance[i].Container; //Value Số cont - Loại cont
                workSheet.Cells[p, 4, j - 1, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 4, j - 1, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[p, 4, j - 1, 4].Style.WrapText = true;

                for (int x = 5; x < 13; x++)
                {
                    workSheet.Cells[p, x, j - 1, x].Merge = true;
                    workSheet.Cells[p, x, j - 1, x].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    workSheet.Cells[p, x, j - 1, x].Style.Numberformat.Format = numberFormat;
                }
                workSheet.Cells[p, 5, j - 1, 5].Value = advanceExport.ShipmentsAdvance[i].Cw; //Value C.W
                workSheet.Cells[p, 6, j - 1, 6].Value = advanceExport.ShipmentsAdvance[i].Pcs; //Value Số kiện
                workSheet.Cells[p, 7, j - 1, 7].Value = advanceExport.ShipmentsAdvance[i].Cbm; //Value CBM
                workSheet.Cells[p, 8, j - 1, 8].Value = advanceExport.ShipmentsAdvance[i].NormAmount; //Value định mức
                workSheet.Cells[p, 9, j - 1, 9].Value = advanceExport.ShipmentsAdvance[i].InvoiceAmount; //Value chi phí có hóa đơn
                workSheet.Cells[p, 10, j - 1, 10].Value = advanceExport.ShipmentsAdvance[i].OtherAmount; //Value chi phí khác
                workSheet.Cells[p, 11, j - 1, 11].Value = advanceExport.ShipmentsAdvance[i].NormAmount + advanceExport.ShipmentsAdvance[i].InvoiceAmount + advanceExport.ShipmentsAdvance[i].OtherAmount; //Value Tổng cộng
                workSheet.Cells[p, 12, j - 1, 12].Value = advanceExport.ShipmentsAdvance[i].RequestNote; //Request Note

                p = j;
                /////
            }

            ////Tổng cộng
            workSheet.Cells[p, 1, p, 3].Merge = true;
            workSheet.Cells[p, 1, p, 3].Value = headers[17].ToUpper(); //Tổng cộng
            workSheet.Cells[p, 1, p, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 1, p, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[p, 5, p, 5].Value = advanceExport.ShipmentsAdvance.Select(s => s.Cw).Sum(); //Total CW
            workSheet.Cells[p, 6, p, 6].Value = advanceExport.ShipmentsAdvance.Select(s => s.Pcs).Sum(); //Total PCS
            workSheet.Cells[p, 7, p, 7].Value = advanceExport.ShipmentsAdvance.Select(s => s.Cbm).Sum(); //Total CBM
            workSheet.Cells[p, 8, p, 8].Value = advanceExport.ShipmentsAdvance.Select(s => s.NormAmount).Sum(); //Total định mức
            workSheet.Cells[p, 9, p, 9].Value = advanceExport.ShipmentsAdvance.Select(s => s.InvoiceAmount).Sum(); //Total phí có hóa đơn
            workSheet.Cells[p, 10, p, 10].Value = advanceExport.ShipmentsAdvance.Select(s => s.OtherAmount).Sum(); //Total phí khác
            var totalAmount = advanceExport.ShipmentsAdvance.Select(s => s.NormAmount + s.InvoiceAmount + s.OtherAmount).Sum();
            workSheet.Cells[p, 11, p, 11].Value = totalAmount; //Total các phí

            //Bôi đen dòng tổng cộng ở cuối
            workSheet.Cells["A" + p + ":K" + p].Style.Font.Bold = true;
            workSheet.Cells["A" + p + ":K" + p].Style.Numberformat.Format = numberFormat;

            workSheet.Cells[14, 1, 14, 12].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + p + ":K" + p].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + (p + 1) + ":K" + (p + 1)].Style.Border.Top.Style = ExcelBorderStyle.Medium;

            //All border
            workSheet.Cells[15, 1, p, 12].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[15, 1, p, 12].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            for (var i = 15; i < p + 1; i++)
            {
                //In đậm border bên trái của Cột 1
                workSheet.Cells[i, 1].Style.Border.Left.Style = ExcelBorderStyle.Medium;
                //In đậm border Cột 3
                workSheet.Cells[i, 3].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                //In đậm border Cột 7
                workSheet.Cells[i, 7].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                //In đậm border Cột 11
                workSheet.Cells[i, 12].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            }

            //Clear border Shipment
            int r = 17;
            int c = 25;
            for (var i = 0; i < advanceExport.ShipmentsAdvance.Count; i++)
            {
                workSheet.Cells["B" + r + ":C" + c].Style.Border.Bottom.Style = ExcelBorderStyle.None;//Xóa border bottom
                workSheet.Cells["B" + r + ":B" + c].Style.Border.Right.Style = ExcelBorderStyle.None;//Xóa border right
                workSheet.Cells["B" + r + ":B" + (c + 1)].Style.Border.Right.Style = ExcelBorderStyle.None;//Xóa border right (dư)
                r = r + 10;
                c = c + 10;
            }

            ////Bỏ qua 2 dòng
            p = p + 2;

            workSheet.Cells[p, 1, p, 2].Merge = true;
            workSheet.Cells[p, 1, p, 2].Value = headers[26]; //Số tiền đề nghị tạm ứng
            workSheet.Cells[p, 1, p, 2].Style.Font.Bold = true;
            workSheet.Cells[p, 1, p, 2].Style.Font.UnderLine = true;
            workSheet.Cells[p, 3, p, 3].Value = totalAmount;
            workSheet.Cells[p, 3, p, 3].Style.Font.Bold = true;
            workSheet.Cells[p, 3, p, 3].Style.Numberformat.Format = numberFormatVND;

            p = p + 1;
            workSheet.Cells[p, 1, p, 2].Merge = true;
            workSheet.Cells[p, 1, p, 2].Value = headers[27];//Số tiền bằng chữ
            workSheet.Cells[p, 3, p, 3].Value = advanceExport.InfoAdvance.AdvanceAmountWord;

            p = p + 1;
            workSheet.Cells[p, 1, p, 2].Merge = true;
            workSheet.Cells[p, 1, p, 2].Value = headers[28];
            workSheet.Cells[p, 3, p, 3].Value = advanceExport.InfoAdvance.AdvanceReason; //Lý do tạm ứng

            p = p + 1;
            workSheet.Cells[p, 1, p, 2].Merge = true;
            workSheet.Cells[p, 1, p, 2].Value = headers[29];
            workSheet.Cells[p, 3, p, 3].Value = advanceExport.InfoAdvance.DealinePayment; //Thời hạn thanh toán
            workSheet.Cells[p, 3, p, 3].Style.Numberformat.Format = "dd MMM, yyyy";
            workSheet.Cells[p, 3, p, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            p = p + 2;

            for (int x = 2; x < 5; x++)
            {
                workSheet.Cells[p, x].Style.WrapText = true;
                workSheet.Cells[p, x].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, x].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            workSheet.Cells[p, 2].Value = headers[30]; //Người tạm ứng
            workSheet.Cells[p, 3].Value = headers[31]; //Người chứng từ
            workSheet.Cells[p, 4].Value = headers[32]; //Trưởng bộ phận

            workSheet.Cells[p, 7, p, 8].Merge = true;
            workSheet.Cells[p, 7, p, 8].Style.WrapText = true;
            workSheet.Cells[p, 7, p, 8].Value = headers[33]; //Kế toán
            workSheet.Cells[p, 7, p, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 7, p, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[p, 10, p, 11].Merge = true;
            workSheet.Cells[p, 10, p, 11].Value = headers[34]; //Giám đốc
            workSheet.Cells[p, 10, p, 11].Style.WrapText = true;
            workSheet.Cells[p, 10, p, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 10, p, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(p).Height = 50;
            p = p + 1;

            if (advanceExport.InfoAdvance.IsRequesterApproved)
            {
                AddIconTick(workSheet, p, 2); //Tick Requester
            }

            if (advanceExport.InfoAdvance.IsManagerApproved)
            {
                AddIconTick(workSheet, p, 4); //Tick Manager Dept
            }

            if (advanceExport.InfoAdvance.IsAccountantApproved)
            {
                workSheet.Cells[p, 7, p, 8].Merge = true;
                AddIconTick(workSheet, p, 7); //Tick Accountant
            }

            if (advanceExport.InfoAdvance.IsBODApproved)
            {
                workSheet.Cells[p, 10, p, 11].Merge = true;
                AddIconTick(workSheet, p, 10); //Tick BOD
            }
            workSheet.Row(p).Height = 50;
            p = p + 1;

            for (int x = 2; x < 5; x++)
            {
                workSheet.Cells[p, x].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, x].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            workSheet.Cells[p, 2].Value = advanceExport.InfoAdvance.Requester; //Value Người tạm ứng
            workSheet.Cells[p, 3].Value = string.Empty; //Value Người chứng từ
            workSheet.Cells[p, 4].Value = advanceExport.InfoAdvance.Manager; //Value Trưởng bộ phận

            workSheet.Cells[p, 7, p, 8].Merge = true;
            workSheet.Cells[p, 7, p, 8].Value = advanceExport.InfoAdvance.Accountant; //Value Kế toán
            workSheet.Cells[p, 7, p, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 7, p, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[p, 10, p, 11].Merge = true;
            workSheet.Cells[p, 10, p, 11].Value = string.Empty; //Giám đốc
        }
        #endregion --- ADVANCE PAYMENT ---
        #region --- SOA ---
        public Stream GenerateDetailSOAExcel(DetailSOAModel detailSOAModel, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("SOA " + detailSOAModel.SOANo);
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BindingDataDetailSOAExcel(workSheet, detailSOAModel);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingDataDetailSOAExcel(ExcelWorksheet workSheet, DetailSOAModel detailSOAModel)
        {
            List<string> headers = new List<string>()
            {
                "Statement Of Account",
                "SOA No",
                "Customer",
                "Taxcode",
                "Address",
                "Currency",
                "Service Date",
                "Job No.",
                "M-B/L",
                "H-B/L",
                "Customs No.",
                "Code Fee",
                "Description",
                "Invoice No",
                "Total Amount",
                "Currency",
                "Exchange Total Amount",
                "Revenue",
                "Cost",
                "Total",
                "Balance"
            };
            //Title
            workSheet.Cells["A1:M1"].Merge = true;
            workSheet.Cells["I8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["I8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["I8:J8"].Merge = true;
            workSheet.Cells["H8:H9"].Merge = true;
            workSheet.Cells["H8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["H8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["G8:G9"].Merge = true;
            workSheet.Cells["G8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["H8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["L8:M8"].Merge = true;
            workSheet.Cells["L8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["L8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            workSheet.Cells["B8:B9"].Merge = true;
            workSheet.Cells["B8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["B8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["C8:C9"].Merge = true;
            workSheet.Cells["C8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["C8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["D8:D9"].Merge = true;
            workSheet.Cells["D8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["D8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["E8:E9"].Merge = true;
            workSheet.Cells["E8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["E8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["F8:F9"].Merge = true;
            workSheet.Cells["F8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["F8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["K8:K9"].Merge = true;
            workSheet.Cells["K8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["K8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["A8:A9"].Merge = true;
            workSheet.Cells["A8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


            workSheet.Cells["A1"].Value = headers[0]; //Statement Of Account
            workSheet.Cells["A1"].Style.Font.Size = 18;
            workSheet.Cells["A1"].Style.Font.Bold = true;
            workSheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            //workSheet.Cells["A5:B5"].Merge = true;
            workSheet.Cells["F3"].Value = headers[1]; //SOA No
            workSheet.Cells["G3"].Value = detailSOAModel.SOANo;
            workSheet.Cells["F4"].Value = headers[2]; //Customer
            workSheet.Cells["G4"].Value = detailSOAModel.CustomerName;
            workSheet.Cells["F5"].Value = headers[3]; //TaxCode
            workSheet.Cells["G5"].Value = detailSOAModel.TaxCode;
            workSheet.Cells["F6"].Value = headers[4]; //Address
            workSheet.Cells["G6"].Value = detailSOAModel.CustomerAddress;
            workSheet.Cells["F7"].Value = headers[5]; //Currency
            workSheet.Cells["G7"].Value = detailSOAModel.CurrencySOA;

            workSheet.Cells["F3"].Style.Font.Bold = true;
            workSheet.Cells["F4"].Style.Font.Bold = true;
            workSheet.Cells["F5"].Style.Font.Bold = true;
            workSheet.Cells["F6"].Style.Font.Bold = true;
            workSheet.Cells["F7"].Style.Font.Bold = true;
            workSheet.Cells["F8"].Style.Font.Bold = true;
            workSheet.Cells["A8"].Value = headers[6]; //Service Date
            workSheet.Cells["A8"].Style.Font.Bold = true;
            workSheet.Cells["B8"].Value = headers[7]; // Job No.
            workSheet.Cells["B8"].Style.Font.Bold = true;
            workSheet.Cells["C8"].Value = headers[8]; //M-B/L
            workSheet.Cells["C8"].Style.Font.Bold = true;
            workSheet.Cells["D8"].Value = headers[9]; //H-B/L
            workSheet.Cells["D8"].Style.Font.Bold = true;
            workSheet.Cells["E8"].Value = headers[10]; //Customs No.
            workSheet.Cells["E8"].Style.Font.Bold = true;
            workSheet.Cells["F8"].Value = headers[11]; //Code Fee
            workSheet.Cells["F8"].Style.Font.Bold = true;
            workSheet.Cells["G8"].Value = headers[12]; //Description
            workSheet.Cells["G8"].Style.Font.Bold = true;
            workSheet.Cells["G8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["G8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["H8"].Value = headers[13]; //Invoice No
            workSheet.Cells["H8"].Style.Font.Bold = true;
            workSheet.Cells["I8"].Value = headers[14]; //Total Amount
            workSheet.Cells["I8"].Style.Font.Bold = true;
            workSheet.Cells["K8"].Value = headers[15]; //Currency
            workSheet.Cells["K8"].Style.Font.Bold = true;
            workSheet.Cells["L8"].Value = headers[16]; //Exchange Total Amount
            workSheet.Cells["L8"].Style.Font.Bold = true;
            workSheet.Cells["I9"].Value = headers[17]; //Revenue
            workSheet.Cells["I9"].Style.Font.Bold = true;
            workSheet.Cells["I9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["I9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["J9"].Style.Font.Bold = true;
            workSheet.Cells["J9"].Value = headers[18]; //Cost
            workSheet.Cells["J9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["J9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["L9"].Value = headers[17]; //Revenue
            workSheet.Cells["L9"].Style.Font.Bold = true;
            workSheet.Cells["L9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["L9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["M9"].Value = headers[18]; //Cost
            workSheet.Cells["M9"].Style.Font.Bold = true;
            workSheet.Cells["M9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["M9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //workSheet.Cells["J12"].Value = headers[19]; //Total
            //workSheet.Cells["J12"].Style.Font.Bold = true;
            //workSheet.Cells["J13"].Value = headers[20]; //Balance
            //workSheet.Cells["J13"].Style.Font.Bold = true;
            int addressStartContent = 10;
            int row = addressStartContent - 1;
            for (int i = 0; i < detailSOAModel.ListCharges.Count; i++)
            {
                var item = detailSOAModel.ListCharges[i];
                workSheet.Cells[i + addressStartContent, 1].Value = item.ServiceDate;
                workSheet.Cells[i + addressStartContent, 1].Style.Numberformat.Format = "dd/MM/yyyy";
                workSheet.Cells[i + addressStartContent, 2].Value = item.JobId;
                workSheet.Cells[i + addressStartContent, 3].Value = item.MBL;
                workSheet.Cells[i + addressStartContent, 4].Value = item.HBL;
                workSheet.Cells[i + addressStartContent, 5].Value = item.CustomNo;
                workSheet.Cells[i + addressStartContent, 6].Value = item.ChargeCode;
                workSheet.Cells[i + addressStartContent, 7].Value = item.ChargeName;
                workSheet.Cells[i + addressStartContent, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[i + addressStartContent, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[i + addressStartContent, 8].Value = item.CreditDebitNo;
                workSheet.Cells[i + addressStartContent, 9].Value = item.Debit;
                workSheet.Cells[i + addressStartContent, 9].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[i + addressStartContent, 10].Value = item.Credit;
                workSheet.Cells[i + addressStartContent, 10].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[i + addressStartContent, 11].Value = item.CurrencyCharge;
                workSheet.Cells[i + addressStartContent, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[i + addressStartContent, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                workSheet.Cells[i + addressStartContent, 11].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[i + addressStartContent, 12].Value = item.DebitExchange;
                workSheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[i + addressStartContent, 13].Value = item.CreditExchange;
                workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat;
                row++;
            }
            workSheet.Cells[8, 1, row, 13].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[8, 1, row, 13].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[8, 1, row, 13].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row + 1, 10].Value = headers[19]; //Total
            workSheet.Cells[row + 2, 10].Value = headers[20]; //Balance
            workSheet.Cells[row + 1, 11].Merge = true;
            string idT = workSheet.Cells[row + 1, 10].First(c => c.Value.ToString() == "Total").Start.Address;
            string idT1 = workSheet.Cells[row + 1, 11].Start.Address;
            string joinTotal = idT + ":" + idT1;
            workSheet.Cells[joinTotal].Merge = true;

            string idxB = workSheet
                       .Cells[row + 2, 10]
                       .First(c => c.Value.ToString() == "Balance")
                       .Start
                       .Address;

            string idxB1 = workSheet
            .Cells[row + 2, 11]
            .Start
            .Address;

            string joinBalance = idxB + ":" + idxB1;
            workSheet.Cells[joinBalance].Merge = true;

            workSheet.Cells[idxB].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[idxB].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[idT].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[idT].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[idxB].Style.Font.Bold = true;
            workSheet.Cells[idxB].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[idT].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[idT].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[idT].Style.Font.Bold = true;

            workSheet.Cells[idxB].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idxB].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idxB].Style.Border.Top.Style = ExcelBorderStyle.Thin;


            workSheet.Cells[joinTotal].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[joinTotal].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[joinTotal].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[joinTotal].Style.Border.Left.Style = ExcelBorderStyle.Thin;


            workSheet.Cells[joinBalance].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[joinBalance].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[joinBalance].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[joinBalance].Style.Border.Left.Style = ExcelBorderStyle.Thin;

            string idValueTotalRevenue = workSheet
            .Cells[row + 1, 12]
            .Start
            .Address;
            string idValueTotalCost = workSheet
           .Cells[row + 1, 13]
           .Start
           .Address;

            decimal? totalRevenue = 0;
            decimal? totalCost = 0;
            decimal? totalBalance = 0M;
            foreach (var item in detailSOAModel.ListCharges)
            {
                totalRevenue += item.DebitExchange;
                totalCost += item.CreditExchange;
            }
            totalBalance = totalRevenue - totalCost;
            decimal total = totalBalance ?? 0;
            string totalBalanceStr = total.ToString("N2");
            totalBalanceStr = totalBalanceStr.Contains("-") ? totalBalanceStr.Replace("-", "") : totalBalanceStr;
            if (totalBalance < 0)
            {
                totalBalanceStr = "(" + totalBalanceStr + ")";
            }

            workSheet.Cells[idValueTotalRevenue].Value = totalRevenue;
            workSheet.Cells[idValueTotalCost].Value = totalCost;
            workSheet.Cells[idValueTotalCost].Style.Numberformat.Format = numberFormat;

            workSheet.Cells[idValueTotalRevenue].Style.Numberformat.Format = numberFormat;
            string idBalance = workSheet
             .Cells[row + 2, 12]
             .Start
             .Address;
            string idBalance1 = workSheet
              .Cells[row + 2, 13]
              .Start
              .Address;
            string joinTotalBalance = idBalance + ":" + idBalance1;
            workSheet.Cells[joinTotalBalance].Merge = true;
            workSheet.Cells[joinTotalBalance].Value = totalBalanceStr;
            workSheet.Cells[idBalance].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[joinTotalBalance].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[joinTotalBalance].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[joinTotalBalance].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[joinTotalBalance].Style.Border.Left.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[idValueTotalCost].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idValueTotalCost].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idValueTotalCost].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idValueTotalCost].Style.Border.Left.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[idValueTotalRevenue].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idValueTotalRevenue].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idValueTotalRevenue].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idValueTotalRevenue].Style.Border.Left.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[idBalance].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[idBalance].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells.AutoFitColumns();

        }


        public Stream GenerateBravoSOAExcel(List<ExportBravoSOAModel> listObj, Stream stream = null)
        {
            List<string> headers = new List<string>()
            {
                "Ngày chứng từ",
                "Số chứng từ",
                "Mã chứng từ",
                "Diễn giải",
                "Mã khách hàng",
                "TK Nợ",
                "TK Có",
                "Mã loại",
                "Mã tiền tệ",
                "Số tiền",
                "Tỷ giá",
                "Tiền VND",
                "% VAT",
                "TK Nợ VAT",
                "TK Có VAT",
                "Tiền VAT",
                "Tiền VND VAT",
                "Số hóa đơn VAT",
                "Ngày hóa đơn VAT",
                "Số Seri VAT",
                "Mặt hàng VAT",
                "Tên đối tượng VAT",
                "Mã số thuế ĐT VAT",
                "Mã Job",
                "Diễn giải",
                "Đánh dấu",
                "Thời hạn T/T",
                "Mã BP",
                "Số TK",
                "Số H-B/L",
                "ĐVT",
                "Hình thức T/T",
                "Mã ĐT CH",
                "Số Lượng",
                "Mã HĐ",
                "Địa chỉ đối tượng VAT",
                "Số M-B/L",
                "Tình trạng hóa đơn",
                "Email",
                "Ngày phát hành E-Invoice"
            };
            try
            {
                int addressStartContent = 4;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("SOA");
                    var worksheet = excelPackage.Workbook.Worksheets.First();

                    BuildHeader(worksheet, headers, "SOA");

                    for (int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];
                        decimal amount = item.OriginalAmount ?? 0;
                        string amountStr = amount.ToString("N2");
                        amountStr = amountStr.Contains("-") ? amountStr.Replace("-", "") : amountStr;
                        if (item.OriginalAmount < 0)
                        {
                            amountStr = "(" + amountStr + ")";
                        }
                        worksheet.Cells[i + addressStartContent, 1].Value = item.ServiceDate;
                        worksheet.Cells[i + addressStartContent, 1].Style.Numberformat.Format = "dd/mm/yyyy";
                        worksheet.Cells[i + addressStartContent, 2].Value = item.SOANo;
                        worksheet.Cells[i + addressStartContent, 3].Value = string.Empty; // tạm thời để trống
                        worksheet.Cells[i + addressStartContent, 4].Value = item.ChargeName;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.PartnerCode;
                        worksheet.Cells[i + addressStartContent, 6].Value = item.Debit;
                        //worksheet.Cells[i + addressStartContent, 6].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[i + addressStartContent, 7].Value = item.Credit;
                        //worksheet.Cells[i + addressStartContent, 7].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[i + addressStartContent, 8].Value = item.ChargeCode;
                        worksheet.Cells[i + addressStartContent, 9].Value = item.OriginalCurrency;
                        worksheet.Cells[i + addressStartContent, 10].Value = item.OriginalAmount;
                        worksheet.Cells[i + addressStartContent, 10].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[i + addressStartContent, 11].Value = item.CreditExchange;
                        worksheet.Cells[i + addressStartContent, 11].Style.Numberformat.Format = numberFormat;

                        worksheet.Cells[i + addressStartContent, 12].Value = item.AmountVND;
                        worksheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[i + addressStartContent, 13].Value = item.VAT;
                        worksheet.Cells[i + addressStartContent, 14].Value = item.AccountDebitNoVAT;
                        worksheet.Cells[i + addressStartContent, 15].Value = item.AccountCreditNoVAT;
                        worksheet.Cells[i + addressStartContent, 16].Value = item.AmountVAT;
                        worksheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[i + addressStartContent, 17].Value = item.AmountVNDVAT;
                        worksheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[i + addressStartContent, 18].Value = item.InvoiceNo; // invoice no
                        worksheet.Cells[i + addressStartContent, 19].Value = item.InvoiceDate?.ToString("dd/MM/yyyy"); // invoice date
                        worksheet.Cells[i + addressStartContent, 20].Value = item.SeriesNo; // SeriesNo
                        worksheet.Cells[i + addressStartContent, 21].Value = item.Commodity;
                        worksheet.Cells[i + addressStartContent, 22].Value = item.CustomerName;
                        worksheet.Cells[i + addressStartContent, 23].Value = item.TaxCode;
                        worksheet.Cells[i + addressStartContent, 24].Value = item.JobId;
                        worksheet.Cells[i + addressStartContent, 25].Value = item.ChargeName;
                        worksheet.Cells[i + addressStartContent, 26].Value = string.Empty; // tạm thời để trống
                        worksheet.Cells[i + addressStartContent, 27].Value = string.Empty; // tạm thời để trống
                        worksheet.Cells[i + addressStartContent, 28].Value = item.TransationType;
                        worksheet.Cells[i + addressStartContent, 29].Value = item.CustomNo;
                        worksheet.Cells[i + addressStartContent, 30].Value = item.HBL;
                        worksheet.Cells[i + addressStartContent, 31].Value = item.Unit;
                        worksheet.Cells[i + addressStartContent, 32].Value = item.Payment;
                        worksheet.Cells[i + addressStartContent, 33].Value = item.TaxCodeOBH;
                        worksheet.Cells[i + addressStartContent, 34].Value = item.Quantity;
                        worksheet.Cells[i + addressStartContent, 35].Value = string.Empty; // tạm thời để trống;
                        worksheet.Cells[i + addressStartContent, 36].Value = item.CustomerAddress;
                        worksheet.Cells[i + addressStartContent, 37].Value = item.MBL;
                        worksheet.Cells[i + addressStartContent, 38].Value = string.Empty; // tạm thời để trống;
                        worksheet.Cells[i + addressStartContent, 39].Value = item.Email;
                        worksheet.Cells[i + addressStartContent, 40].Value = string.Empty; // tạm thời để trống;

                        //Add border left right for cells
                        AddBorderLeftRightCell(worksheet, headers, addressStartContent, i);

                        //Add border bottom for last cells
                        AddBorderBottomLastCell(worksheet, headers, addressStartContent, i, listObj.Count);
                    }
                    worksheet.Cells.AutoFitColumns();
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Stream GenerateSOAOPSExcel(SOAOPSModel lstSoa, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("SOA OPS");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BinddingDatalSOAOPS(workSheet, lstSoa);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BinddingDatalSOAOPS(ExcelWorksheet workSheet, SOAOPSModel lstSoa)
        {
            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column B
                excelImage.SetPosition(0, 0, 1, 0);
            }

            List<string> headers = new List<string>()
            {
               "INDO TRANS LOGISTICS CORPORATION", //0
               "52-54-56 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
               "BẢNG TỔNG HỢP CHI PHÍ PHÁT SINH/SUMMARY OF COSTS INCURRED", //2
               "Tổng cộng/Total (VND) ", //3
            };

            List<string> headerTable = new List<string>()
            {
               "STT/No",
               "Tên hàng hóa/Commodity",
               "Quantity/Số lượng",
               "Đơn Vị Tính/Đơn vị",
               "Số Lô Hàng/Job No",
               "Số Tờ Khai/Customs Declaration No",
               "Số Vận Đơn/HBL No",
               "Số HĐ/Invoice No",
               "Ngày HĐ/Invoice Date",
               "Trọng Lượng/VOLUMNE",
               "Phí dịch vụ làm hàng/Customs clearance fee",
               "Phí thu hộ/Authorized fees",
               "Tổng cộng/Total",
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

            workSheet.Cells["Q1:V1"].Merge = true;
            workSheet.Cells["Q1"].Value = headers[0];
            workSheet.Cells["Q1"].Style.Font.SetFromFont(new Font("Arial Black", 13));
            workSheet.Cells["Q1"].Style.Font.Italic = true;
            workSheet.Cells["Q2:V2"].Merge = true;
            workSheet.Cells["Q2:V2"].Style.WrapText = true;
            workSheet.Cells["Q2"].Value = headers[1];
            workSheet.Cells["Q2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10));
            workSheet.Row(2).Height = 50;
            //Title
            workSheet.Cells["A4:V4"].Merge = true;
            workSheet.Cells["A4"].Style.Font.SetFromFont(new Font("Times New Roman", 16));
            workSheet.Cells["A4"].Value = headers[2];
            workSheet.Cells["A4"].Style.Font.Size = 16;
            workSheet.Cells["A4"].Style.Font.Bold = true;

            workSheet.Cells["A5:V5"].Merge = true;
            DateTime fromDate = lstSoa.FromDate ?? DateTime.Now;
            DateTime toDate = lstSoa.ToDate ?? DateTime.Now;
            workSheet.Cells["A5"].Style.Font.SetFromFont(new Font("Times New Roman", 14));
            workSheet.Cells["A5"].Value = "Từ Ngày: " + fromDate.ToString("dd/MM/yyyy") + " đến: " + toDate.ToString("dd/MM/yyyy");
            workSheet.Cells["A5"].Style.Font.Bold = true;

            workSheet.Cells["A6:V6"].Merge = true;
            workSheet.Cells["A6"].Value = "SOA No: " + lstSoa.SoaNo?.ToUpper();

            workSheet.Cells["A7:V7"].Merge = true;
            workSheet.Cells["A7"].Value = lstSoa.PartnerNameVN;
            workSheet.Cells["A8:V8"].Merge = true;
            workSheet.Cells["A8"].Value = lstSoa.BillingAddressVN;
            workSheet.Cells["A6:A7"].Style.Font.SetFromFont(new Font("Times New Roman", 15));

            workSheet.Cells[4, 1, 8, 17].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[4, 1, 8, 17].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            decimal? totalALLValue = 0;

            workSheet.Cells["A9:A10"].Merge = true;
            workSheet.Cells["B9:B10"].Merge = true;
            workSheet.Cells["C9:C10"].Merge = true;
            workSheet.Cells["D9:D10"].Merge = true;
            workSheet.Cells["E9:E10"].Merge = true;
            workSheet.Cells["F9:F10"].Merge = true;
            workSheet.Cells["G9:G10"].Merge = true;
            workSheet.Cells["H9:H10"].Merge = true;
            workSheet.Cells["I9:I10"].Merge = true;


            workSheet.Cells["J9:L9"].Merge = true;
            workSheet.Cells["M9:O9"].Merge = true;
            workSheet.Cells["P9:R9"].Merge = true;
            workSheet.Cells["S9:S10"].Merge = true;

            // Tạo header
            for (int i = 0; i < headerTable.Count; i++)
            {
                if (i <= 9)
                {
                    workSheet.Cells[9, i + 1].Value = headerTable[i];
                }

                if (i == 10)
                {
                    workSheet.Cells[9, i + 3].Value = headerTable[i];
                }
                if (i == 11)
                {
                    workSheet.Cells[9, i + 5].Value = headerTable[i];
                }

                if (i == 12)
                {
                    workSheet.Cells[9, i + 7].Value = headerTable[i];
                }
            }

            workSheet.Cells["J10"].Value = subheaderTable[0];
            workSheet.Cells["K10"].Value = subheaderTable[1];
            workSheet.Cells["L10"].Value = subheaderTable[2];

            workSheet.Cells["M10"].Value = subheaderTable[3];
            workSheet.Cells["N10"].Value = subheaderTable[4];
            workSheet.Cells["O10"].Value = subheaderTable[5];

            workSheet.Cells["P10"].Value = subheaderTable[3];
            workSheet.Cells["Q10"].Value = subheaderTable[4];
            workSheet.Cells["R10"].Value = subheaderTable[5];

            workSheet.Cells[9, 1, 10, 18].Style.Font.Bold = true;
            workSheet.Cells[9, 1, 10, 18].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[9, 1, 10, 18].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[9, 1, 10, 18].Style.WrapText = true;

            int addressStartContent = 11;
            Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#eab286");
            if (lstSoa.exportSOAOPs != null && lstSoa.exportSOAOPs.Count() > 0)
            {
                for (int i = 0; i < lstSoa.exportSOAOPs.Count; i++)
                {
                    var item = lstSoa.exportSOAOPs[i];
                    workSheet.Cells[i + addressStartContent, 1].Value = i + 1;
                    workSheet.Cells[i + addressStartContent, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 1].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 2].Value = item.CommodityName;
                    workSheet.Cells[i + addressStartContent, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                    workSheet.Cells[i + addressStartContent, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 2].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 4].Value = string.Empty;
                    workSheet.Cells[i + addressStartContent, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 3].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 4].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 5].Value = item.Charges.Select(t => t.JobId).FirstOrDefault();
                    workSheet.Cells[i + addressStartContent, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 5].Style.Fill.BackgroundColor.SetColor(colFromHex);


                    workSheet.Cells[i + addressStartContent, 6].Value = item.Charges.Select(t => t.CustomNo).FirstOrDefault();
                    workSheet.Cells[i + addressStartContent, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 6].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 7].Value = item.HwbNo;
                    workSheet.Cells[i + addressStartContent, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 7].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 8].Value = string.Join(';', item.Charges.Where(x => !string.IsNullOrEmpty(x.InvoiceNo)).Select(x => x.InvoiceNo));
                    workSheet.Cells[i + addressStartContent, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 8].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 7, i + addressStartContent, 7].Style.WrapText = true;

                    workSheet.Cells[i + addressStartContent, 10].Value = item.GW;
                    workSheet.Cells[i + addressStartContent, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 10].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 11].Value = item.CBM;
                    workSheet.Cells[i + addressStartContent, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 11].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 12].Value = item.PackageContainer;
                    workSheet.Cells[i + addressStartContent, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 12].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 13].Value = item.Charges.Where(t => !t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat2;
                    workSheet.Cells[i + addressStartContent, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 13].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 14].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 15].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 15].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 16].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 16].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 17].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 18].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 19].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 19].Style.Fill.BackgroundColor.SetColor(colFromHex);

                    workSheet.Cells[i + addressStartContent, 14].Value = item.Charges.Where(t => !t.Type.Contains("OBH")).Sum(t => Math.Abs(t.VATAmount ?? 0));
                    workSheet.Cells[i + addressStartContent, 14].Style.Numberformat.Format = numberFormat2;

                    workSheet.Cells[i + addressStartContent, 15].Value = item.Charges.Where(t => !t.Type.Contains("OBH")).Sum(t => Math.Abs(t.VATAmount ?? 0)) + item.Charges.Where(t => !t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = numberFormat2;

                    workSheet.Cells[i + addressStartContent, 19].Value = item.Charges.Sum(t => Math.Abs(t.VATAmount ?? 0)) + item.Charges.Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 19].Style.Numberformat.Format = numberFormat2;

                    workSheet.Cells[i + addressStartContent, 16].Value = item.Charges.Where(t => t.Type.Contains("OBH")).Sum(t => t.NetAmount);

                    workSheet.Cells[i + addressStartContent, 17].Value = item.Charges.Where(t => t.Type.Contains("OBH")).Sum(t => Math.Abs(t.VATAmount ?? 0));

                    workSheet.Cells[i + addressStartContent, 18].Value = item.Charges.Where(t => t.Type.Contains("OBH")).Sum(t => Math.Abs(t.VATAmount ?? 0)) + item.Charges.Where(t => t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = numberFormat2;
                    workSheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = numberFormat2;
                    workSheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = numberFormat2;

                    for (int j = 0; j < item.Charges.Count; j++)
                    {
                        addressStartContent++;
                        var itemCharge = item.Charges[j];

                        workSheet.Cells[i + addressStartContent, 2].Value = itemCharge.ChargeName;
                        workSheet.Cells[i + addressStartContent, 3].Value = itemCharge.Quantity;
                        workSheet.Cells[i + addressStartContent, 4].Value = itemCharge.Unit;
                        workSheet.Cells[i + addressStartContent, 8].Value = itemCharge.InvoiceNo;
                        workSheet.Cells[i + addressStartContent, 9].Value = itemCharge.InvoiceDate?.ToString("dd/MM/yyyy");
                        string vatAmount = "( " + itemCharge.VATAmount + " )";


                        if (itemCharge.Type.Contains("OBH"))
                        {
                            workSheet.Cells[i + addressStartContent, 16].Value = itemCharge.NetAmount;
                            workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = numberFormat2;
                            workSheet.Cells[i + addressStartContent, 18].Value = Math.Abs(itemCharge.VATAmount ?? 0) + itemCharge.NetAmount.GetValueOrDefault(0M);
                            workSheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = numberFormat2;
                            workSheet.Cells[i + addressStartContent, 17].Value = itemCharge.VATAmount;
                            workSheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = numberFormat2;
                            if (itemCharge.VATAmount < 0)
                            {
                                workSheet.Cells[i + addressStartContent, 17].Value = Math.Abs(itemCharge.VATAmount ?? 0);
                            }
                            else
                            {
                                workSheet.Cells[i + addressStartContent, 17].Value = itemCharge.VATAmount;
                                workSheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = numberFormat2;
                            }

                        }
                        else
                        {
                            workSheet.Cells[i + addressStartContent, 13].Value = itemCharge.NetAmount;
                            workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat2;
                            workSheet.Cells[i + addressStartContent, 14].Value = itemCharge.VATAmount;
                            workSheet.Cells[i + addressStartContent, 14].Style.Numberformat.Format = numberFormat2;
                            workSheet.Cells[i + addressStartContent, 15].Value = Math.Abs(itemCharge.VATAmount ?? 0) + itemCharge.NetAmount.GetValueOrDefault(0M);
                            workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = numberFormat2;

                            if (itemCharge.VATAmount < 0)
                            {
                                workSheet.Cells[i + addressStartContent, 14].Value = Math.Abs(itemCharge.VATAmount ?? 0);
                            }
                            else
                            {
                                workSheet.Cells[i + addressStartContent, 14].Value = itemCharge.VATAmount;
                                workSheet.Cells[i + addressStartContent, 14].Style.Numberformat.Format = numberFormat2;
                            }
                        }

                        decimal? TotalNormalCharge = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 15].Value);
                        decimal? TotalOBHCharge = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 18].Value);
                        workSheet.Cells[i + addressStartContent, 19].Value = TotalNormalCharge.GetValueOrDefault(0M) + TotalOBHCharge.GetValueOrDefault(0M);
                        workSheet.Cells[i + addressStartContent, 19].Style.Numberformat.Format = numberFormat2;

                        totalALLValue += TotalNormalCharge.GetValueOrDefault(0M) + TotalOBHCharge.GetValueOrDefault(0M);
                    }

                }

                addressStartContent = addressStartContent + lstSoa.exportSOAOPs.Count;

                workSheet.Cells[9, 1, addressStartContent, 19].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[9, 1, addressStartContent, 19].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[9, 1, addressStartContent, 19].Style.Border.Top.Style = ExcelBorderStyle.Thin;


                workSheet.Cells[addressStartContent, 1].Value = headers[3]; //Total
                string addressTotal = workSheet
                .Cells[addressStartContent, 1]
                .First(c => c.Value.ToString() == headers[3])
                .Start
                .Address;
                string addressTotalMerge = workSheet
                 .Cells[addressStartContent, 12].Start.Address;
                string addressToMerge = addressTotal + ":" + addressTotalMerge;
                workSheet.Cells[addressToMerge].Merge = true;
                workSheet.Cells[addressToMerge].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[addressToMerge].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                string addressTotalNext = workSheet
               .Cells[addressStartContent, 13].Start.Address;

                decimal? totalNetAmountNormalCharge = 0;
                decimal? totalNetAmountOBHCharge = 0;
                decimal? totalVATNormalCharge = 0;
                decimal? totalVATOBHCharge = 0;
                decimal? totalNormalCharge = 0;
                decimal? totalOBHCharge = 0;
                decimal? totalAll = 0;

                foreach (var item in lstSoa.exportSOAOPs)
                {
                    foreach (var it in item.Charges)
                    {
                        if (!it.Type.Contains("OBH"))
                        {
                            totalNetAmountNormalCharge += it.NetAmount.GetValueOrDefault(0M);
                            totalVATNormalCharge += Math.Abs(it.VATAmount ?? 0);
                            totalNormalCharge = totalNetAmountNormalCharge.GetValueOrDefault(0M) + totalVATNormalCharge.GetValueOrDefault(0M);
                        }
                        else
                        {
                            totalNetAmountOBHCharge += it.NetAmount.GetValueOrDefault(0M);
                            totalVATOBHCharge += Math.Abs(it.VATAmount ?? 0);
                            totalOBHCharge = totalNetAmountOBHCharge.GetValueOrDefault(0M) + totalVATOBHCharge.GetValueOrDefault(0M);
                        }

                    }

                }

                totalAll = totalOBHCharge + totalNormalCharge;

                workSheet.Cells[addressTotalNext].Value = totalNetAmountNormalCharge;
                workSheet.Cells[addressTotalNext].Style.Numberformat.Format = numberFormat2;

                string addressTotalVat = workSheet.Cells[addressStartContent, 14].Start.Address;
                workSheet.Cells[addressTotalVat].Value = totalVATNormalCharge;
                workSheet.Cells[addressTotalVat].Style.Numberformat.Format = numberFormat2;

                string addressTotalNormalCharge = workSheet.Cells[addressStartContent, 15].Start.Address;
                workSheet.Cells[addressTotalNormalCharge].Value = totalNormalCharge;
                workSheet.Cells[addressTotalNormalCharge].Style.Numberformat.Format = numberFormat2;

                string addressNetAmountCharge = workSheet.Cells[addressStartContent, 16].Start.Address;
                workSheet.Cells[addressNetAmountCharge].Value = totalNetAmountOBHCharge;
                workSheet.Cells[addressNetAmountCharge].Style.Numberformat.Format = numberFormat2;

                string addressVATChargeNext = workSheet.Cells[addressStartContent, 17].Start.Address;
                workSheet.Cells[addressVATChargeNext].Value = totalVATOBHCharge;
                workSheet.Cells[addressVATChargeNext].Style.Numberformat.Format = numberFormat2;

                string addressTotalChargeNext = workSheet.Cells[addressStartContent, 18].Start.Address;
                workSheet.Cells[addressTotalChargeNext].Value = totalOBHCharge;
                workSheet.Cells[addressTotalChargeNext].Style.Numberformat.Format = numberFormat2;


                string addressTotalAll = workSheet.Cells[addressStartContent, 19].Start.Address;
                workSheet.Cells[addressTotalAll].Value = totalAll;
                workSheet.Cells[addressTotalAll].Style.Numberformat.Format = numberFormat2;


                workSheet.Column(1).Width = 8; //Cột A
                workSheet.Column(2).Width = 40; //Cột B
                workSheet.Column(3).Width = 20; //Cột C
                workSheet.Column(4).Width = 20; //Cột D
                workSheet.Column(5).Width = 20; //Cột E
                workSheet.Column(6).Width = 35; //Cột F
                workSheet.Column(7).Width = 30; //Cột G
                workSheet.Column(8).Width = 20; //Cột H
                workSheet.Column(9).Width = 20; //Cột J
                workSheet.Column(12).Width = 35;//Cột K
                workSheet.Column(13).Width = 20;//Cột L
                workSheet.Column(14).Width = 20;//Cột M
                workSheet.Column(15).Width = 20;//Cột N
                workSheet.Column(16).Width = 20;//Cột N
                workSheet.Column(17).Width = 20; //Cột O
                workSheet.Column(18).Width = 20;  //Cột P
                workSheet.Column(19).Width = 20;   //Cột Q
                workSheet.Cells[addressTotal].Style.Font.Bold = true;
            }
        }

        /// <returns></returns>
        public Stream GenerateSOAAirfreightExcel(ExportSOAAirfreightModel soaAir, string type, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Air freight");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    if (type == "WithHBL")
                    {
                        BinddingDataDetailSOAAirFreightWithHBL(workSheet, soaAir);
                    }
                    else
                    {
                        BinddingDataDetailSOAAirfreight(workSheet, soaAir);
                    }
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        // Generate Soa Supplier
        public Stream GenerateSOASupplierAirfreightExcel(ExportSOAAirfreightModel soaAir, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Air freight");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BinddingDataDetailSOASupplierAirfreight(workSheet, soaAir);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BinddingDataDetailSOAAirFreightWithHBL(ExcelWorksheet workSheet, ExportSOAAirfreightModel airfreightObj)
        {
            {
                using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
                {
                    var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                    //add the image to row 1, column B
                    excelImage.SetPosition(0, 0, 1, 0);
                }

                string monthName = airfreightObj.SoaFromDate.HasValue ? airfreightObj.SoaFromDate.Value.ToString("MMM", CultureInfo.InvariantCulture) : string.Empty;
                List<string> headers = new List<string>()
            {
               "INDO TRANS LOGISTICS CORPORATION", //0
               "52-54-56 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
               "DEBIT NOTE IN " + monthName?.ToUpper() + " " + airfreightObj.SoaFromDate.Value.Year  + " (BẢNG KÊ CƯỚC VCQT)" , //2
               "TOTAL", //3
            };

                List<string> headerTable = new List<string>()
                {
                    "No", //1
                    "Job No", //2
                    "Flight No", //3
                    "ETD", //4
                    "MAWB", //5
                    "HBL", //6
                    "Origin(AOL)", //7
                    "Dest(AOD)", //8
                    //"Service", //9
                    "Air PCS", //10
                    "Gross Weight(KG)", //11
                    "Chargeable Weight(KG)", //12
                    "Rate(USD)", //13
                    "AirFreight(USD)", //14
                    "Fuel Surcharge(USD)", //15
                    "Warisk Surcharge(USD)", //16
                    "Screening Surcharge(USD)", //17
                    "AWB(USD)", //18
                    "AMS(USD)", //19
                    "Dangerous fee(USD)", //20
                    "Other fee(USD)", //21
                    "Handling fee(USD)", //22
                    "Net Amount(USD)", //23
                    "Exchange Rate(VND/USD)", //24
                    "Total Amount(VND)", //25

                };

                workSheet.Cells["H1:U1"].Merge = true;
                workSheet.Cells["H1"].Value = headers[0];
                workSheet.Cells["H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                workSheet.Cells["H1"].Style.Font.SetFromFont(new Font("Arial Black", 13));
                workSheet.Cells["H1"].Style.Font.Italic = true;
                workSheet.Cells["P2:U2"].Merge = true;
                workSheet.Cells["P2:U2"].Style.WrapText = true;
                workSheet.Cells["P2"].Value = headers[1];
                workSheet.Cells["P2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10));
                workSheet.Row(2).Height = 50;
                //Title
                workSheet.Cells["A3:U3"].Merge = true;
                workSheet.Cells["A3"].Style.Font.SetFromFont(new Font("Times New Roman", 16));
                workSheet.Cells["A3"].Value = headers[2];
                workSheet.Cells["A3"].Style.Font.Size = 16;
                workSheet.Cells["A3"].Style.Font.Bold = true;

                workSheet.Cells["A5:J6"].Style.Font.SetFromFont(new Font("Times New Roman", 11));
                workSheet.Cells["A5:B5"].Merge = true;
                workSheet.Cells["A4:U6"].Merge = true;
                workSheet.Cells["A4:U6"].Style.WrapText = true;

                // Tạo header
                for (int i = 0; i < headerTable.Count; i++)
                {
                    //if (i == 5)
                    //{
                    //    workSheet.Cells[8, i + 2].Value = headerTable[i];
                    //}
                    //if (i < 5)
                    //{
                    //    workSheet.Cells[8, i + 1].Value = headerTable[i];
                    //}
                    //if (i > 5)
                    //{
                    //    workSheet.Cells[8, i + 2].Value = headerTable[i];
                    //    workSheet.Cells[8, i + 2].Style.Font.Bold = true;

                    //}
                    workSheet.Cells[8, i + 1].Value = headerTable[i];
                    workSheet.Cells[8, i + 1].Style.Font.Bold = true;
                    workSheet.Cells[8, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Cells[8, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                }

                workSheet.Cells["A8:A9"].Merge = true;
                workSheet.Cells["A8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["B8:B9"].Merge = true;
                workSheet.Cells["B8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["B8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["C8:C9"].Merge = true;
                workSheet.Cells["C8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["C8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["D8:D9"].Merge = true;
                workSheet.Cells["E8:E9"].Merge = true;
                workSheet.Cells["F8:F9"].Merge = true;
                workSheet.Cells["D8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["D8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //workSheet.Cells["E8:E9"].Merge = true;
                workSheet.Cells["E8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["E8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //workSheet.Cells["F8:F9"].Merge = true;
                workSheet.Cells["F8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["F8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                workSheet.Cells["G8:G9"].Merge = true;
                workSheet.Cells["G8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["G8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["H8:H9"].Merge = true;
                workSheet.Cells["H8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["H8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["I8:I9"].Merge = true;
                workSheet.Cells["I8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["I8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["J8:J9"].Merge = true;
                workSheet.Cells["J8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["J8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["J8:J9"].Style.WrapText = true;
                workSheet.Cells["K8:K9"].Merge = true;
                workSheet.Cells["K8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["K8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["K8:K9"].Style.WrapText = true;

                workSheet.Cells["L8:L9"].Merge = true;
                workSheet.Cells["L8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["L8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["M8:M9"].Merge = true;
                workSheet.Cells["M8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["M8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["M8:M9"].Style.WrapText = true;

                workSheet.Cells["N8:N9"].Merge = true;
                workSheet.Cells["N8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["N8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["N8:N9"].Style.WrapText = true;

                workSheet.Cells["O8:O9"].Merge = true;
                workSheet.Cells["O8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["O8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["O8:O9"].Style.WrapText = true;


                workSheet.Cells["P8:P9"].Merge = true;
                workSheet.Cells["P8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["P8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["P8:P9"].Style.WrapText = true;


                workSheet.Cells["Q8:Q9"].Merge = true;
                workSheet.Cells["Q8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["Q8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["Q8:Q9"].Style.WrapText = true;

                workSheet.Cells["R8:R9"].Merge = true;
                workSheet.Cells["R8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["R8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["R8:R9"].Style.WrapText = true;

                workSheet.Cells["S8:S9"].Merge = true;
                workSheet.Cells["S8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["S8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["S8:S9"].Style.WrapText = true;


                workSheet.Cells["T8:T9"].Merge = true;
                workSheet.Cells["T8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["T8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["T8:T9"].Style.WrapText = true;
                workSheet.Cells["U8:U9"].Merge = true;
                workSheet.Cells["U8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["U8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["U8:U9"].Style.WrapText = true;
                workSheet.Cells["V8:V9"].Merge = true;
                workSheet.Cells["V8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["V8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["V8:V9"].Style.WrapText = true;
                workSheet.Cells["W8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["W8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["W8:W9"].Style.WrapText = true;
                workSheet.Cells["X8:X9"].Merge = true;
                workSheet.Cells["X8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["X8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["X8:X9"].Style.WrapText = true;

                //workSheet.Cells["E8:F9"].Merge = true;


                string textHead = string.Empty;
                if (!string.IsNullOrEmpty(airfreightObj.PartnerNameEn))
                {
                    textHead = textHead + airfreightObj.PartnerNameEn + "\n";
                }
                if (!string.IsNullOrEmpty(airfreightObj.PartnerBillingAddress))
                {
                    textHead = textHead + airfreightObj.PartnerBillingAddress + "\n";
                }
                if (!string.IsNullOrEmpty(airfreightObj.PartnerTaxCode))
                {
                    textHead = textHead + "Tax code: " + airfreightObj.PartnerTaxCode;
                }
                workSheet.Cells["A4"].Value = textHead;

                workSheet.Cells["A7:V7"].Merge = true;

                string textSOA = string.Empty;
                if (!string.IsNullOrEmpty(airfreightObj.SoaNo))
                {
                    textSOA = textSOA + "Bảng kê số: " + airfreightObj.SoaNo + " đính kèm hóa đơn số: ";
                }
                if (airfreightObj.DateSOA != null)
                {
                    DateTime dateSOA = airfreightObj.DateSOA ?? DateTime.Now;
                    textSOA = textSOA + dateSOA.ToString("dd/MM/yyyy");
                }
                workSheet.Cells["A7"].Value = textSOA;

                workSheet.Cells["X8:X9"].Merge = true;
                workSheet.Cells["W8:W9"].Merge = true;


                int addressStartContent = 10;
                int row = addressStartContent - 1;
                int row1 = addressStartContent - 1;

                for (int i = 0; i < airfreightObj.HawbAirFrieghts.Count; i++)
                {
                    var item = airfreightObj.HawbAirFrieghts[i];
                    workSheet.Cells[i + addressStartContent, 1].Value = i + 1;
                    workSheet.Cells[i + addressStartContent, 2].Value = item.JobNo;
                    workSheet.Cells[i + addressStartContent, 3].Value = item.FlightNo;
                    workSheet.Cells[i + addressStartContent, 4].Value = item.ShippmentDate;
                    workSheet.Cells[i + addressStartContent, 4].Style.Numberformat.Format = "dd/MM/yyyy";

                    workSheet.Cells[i + addressStartContent, 5].Value = item.Mawb;
                    workSheet.Cells[i + addressStartContent, 6].Value = item.HBLNo;
                    workSheet.Cells[i + addressStartContent, 7].Value = item.AOL;
                    workSheet.Cells[i + addressStartContent, 8].Value = item.AOD;
                    //workSheet.Cells[i + addressStartContent, 8].Value = item.Service;
                    workSheet.Cells[i + addressStartContent, 9].Value = item.Pcs;
                    workSheet.Cells[i + addressStartContent, 9].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartContent, 10].Value = item.GW;
                    workSheet.Cells[i + addressStartContent, 10].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartContent, 11].Value = item.CW;
                    workSheet.Cells[i + addressStartContent, 11].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartContent, 12].Value = item.Rate;
                    workSheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 13].Value = item.AirFreight;
                    workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 14].Value = item.FuelSurcharge;
                    workSheet.Cells[i + addressStartContent, 14].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 15].Value = item.WarriskSurcharge;
                    workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 16].Value = item.ScreeningFee;
                    workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 17].Value = item.AWB;
                    workSheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 18].Value = item.AMS;
                    workSheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 19].Value = item.DAN;
                    workSheet.Cells[i + addressStartContent, 19].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 20].Value = item.OTH;
                    workSheet.Cells[i + addressStartContent, 20].Style.Numberformat.Format = numberFormat;


                    workSheet.Cells[i + addressStartContent, 21].Value = item.HandlingFee;
                    workSheet.Cells[i + addressStartContent, 21].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 22].Value = item.NetAmount;
                    workSheet.Cells[i + addressStartContent, 22].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 23].Value = item.ExchangeRate;
                    workSheet.Cells[i + addressStartContent, 23].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 24].Value = item.TotalAmount;
                    workSheet.Cells[i + addressStartContent, 24].Style.Numberformat.Format = numberFormat;


                    row1++;

                }
                workSheet.Cells[8, 1, row1 + 1, 24].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[8, 1, row1, 24].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[8, 1, row1, 24].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                addressStartContent = addressStartContent + airfreightObj.HawbAirFrieghts.Count;

                workSheet.Cells[addressStartContent, 1].Value = headers[3]; //Total


                string idT = workSheet
                .Cells[addressStartContent, 1]
                .First(c => c.Value.ToString() == "TOTAL")
                .Start
                .Address;

                string idT1 = workSheet
                .Cells[addressStartContent, 8]
                .Start
                .Address;
                string totalStr = idT + ":" + idT1;

                workSheet.Cells[totalStr].Merge = true;
                workSheet.Cells[idT].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[idT].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[idT].Style.Font.Bold = true;

                string idPCS = workSheet
               .Cells[addressStartContent, 9]
               .Start
               .Address;
                workSheet.Cells[idPCS].Value = airfreightObj.HawbAirFrieghts.Select(t => t.Pcs).Sum();
                workSheet.Cells[idPCS].Style.Numberformat.Format = numberFormat;


                string idGW = workSheet
                .Cells[addressStartContent, 10]
                .Start
                .Address;
                workSheet.Cells[idGW].Value = airfreightObj.HawbAirFrieghts.Select(t => t.GW).Sum();
                workSheet.Cells[idGW].Style.Numberformat.Format = numberFormat;


                string idCW = workSheet
                .Cells[addressStartContent, 11]
                .Start
                .Address;
                workSheet.Cells[idCW].Value = airfreightObj.HawbAirFrieghts.Select(t => t.CW).Sum();
                workSheet.Cells[idCW].Style.Numberformat.Format = numberFormat;


                string idAF = workSheet
                  .Cells[addressStartContent, 13]
                  .Start
                  .Address;
                workSheet.Cells[idAF].Value = airfreightObj.HawbAirFrieghts.Select(t => t.AirFreight).Sum();
                workSheet.Cells[idAF].Style.Numberformat.Format = numberFormat;


                string idHF = workSheet
                 .Cells[addressStartContent, 21]
                 .Start
                 .Address;
                workSheet.Cells[idHF].Value = airfreightObj.HawbAirFrieghts.Select(t => t.HandlingFee).Sum();
                workSheet.Cells[idHF].Style.Numberformat.Format = numberFormat;

                string idNA = workSheet
                 .Cells[addressStartContent, 22]
                 .Start
                 .Address;
                workSheet.Cells[idNA].Value = airfreightObj.HawbAirFrieghts.Select(t => t.NetAmount).Sum();
                workSheet.Cells[idNA].Style.Numberformat.Format = numberFormat;

                string idTT = workSheet
                 .Cells[addressStartContent, 24]
                 .Start
                 .Address;
                workSheet.Cells[idTT].Value = airfreightObj.HawbAirFrieghts.Select(t => t.TotalAmount).Sum();
                workSheet.Cells[idTT].Style.Numberformat.Format = numberFormat;

                //format
                workSheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["A3"].Style.Font.Bold = true;
                workSheet.Cells["A4"].Style.Font.Bold = true;
                workSheet.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["A7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["A7"].Style.Font.Bold = true;

                workSheet.Cells[idPCS].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[idPCS].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[idPCS].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[idPCS].Style.Border.Left.Style = ExcelBorderStyle.Thin;

                string idToBoder = workSheet
                 .Cells[addressStartContent, 10]
                 .Start
                 .Address;

                string idToborder1 = workSheet
                .Cells[addressStartContent, 23]
                .Start
                .Address;
                string addressBorderRight = idToBoder + ":" + idToborder1;
                workSheet.Cells[addressBorderRight].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                string addressTextCenter = "A8" + ":" + idToborder1;

                workSheet.Cells[addressTextCenter].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[addressTextCenter].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                workSheet.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["A7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                workSheet.Cells[row1 + 2, 4].Value = "Issued by";
                workSheet.Cells[row1 + 2, 4].Style.Font.SetFromFont(new Font("Times New Roman", 13));

                workSheet.Cells[row1 + 2, 7].Value = "Approved by";
                workSheet.Cells[row1 + 2, 7].Style.Font.SetFromFont(new Font("Times New Roman", 13));

                workSheet.Cells[row1 + 2, 13].Value = "Account";
                workSheet.Cells[row1 + 2, 13].Style.Font.SetFromFont(new Font("Times New Roman", 13));


                //Bôi đen header
                workSheet.Cells["A8:K8"].Style.Font.Bold = true;
                workSheet.Column(1).Width = 8; //Cột A
                workSheet.Column(2).Width = 15; //Cột B
                workSheet.Column(3).Width = 15; //Cột C
                workSheet.Column(4).Width = 15; //Cột D
                workSheet.Column(5).Width = 17; //Cột E
                workSheet.Column(6).Width = 17; //Cột F
                workSheet.Column(10).Width = 15;//Cột J
                workSheet.Column(11).Width = 15;//Cột K
                workSheet.Column(12).Width = 15;//Cột L
                workSheet.Column(13).Width = 15;//Cột M
                workSheet.Column(14).Width = 15;//Cột M
                workSheet.Column(15).Width = 15; //Cột N
                workSheet.Column(16).Width = 15;  //Cột O
                workSheet.Column(17).Width = 15;   //Cột P
                workSheet.Column(18).Width = 15;     //Cột Q
                workSheet.Column(19).Width = 15;  //Cột R
                workSheet.Column(20).Width = 15;   //Cột S
                workSheet.Column(21).Width = 20;   //Cột T
                workSheet.Column(22).Width = 25;   //Cột U

                workSheet.Cells[row1 + 3, 3].Value = "Kindly arrange the payment to: ";
                string textBottom = workSheet.Cells[row1 + 3, 3].Value.ToString();

                if (!string.IsNullOrEmpty(airfreightObj.OfficeEn))
                {
                    textBottom = textBottom + "\n" + airfreightObj.OfficeEn;
                }


                if (!string.IsNullOrEmpty(airfreightObj.BankAccountVND))
                {
                    textBottom = textBottom + "\n" + "A/C: " + (!string.IsNullOrEmpty(airfreightObj.BankAccountUsd) ? (airfreightObj.BankAccountUsd + " - ") : string.Empty) + airfreightObj.BankAccountVND;
                }

                if (!string.IsNullOrEmpty(airfreightObj.BankNameEn))
                {
                    textBottom = textBottom + "\n" + "Via: " + airfreightObj.BankNameEn;
                }

                if (!string.IsNullOrEmpty(airfreightObj.AddressEn))
                {
                    textBottom = textBottom + "\n" + airfreightObj.AddressEn;
                }

                if (!string.IsNullOrEmpty(airfreightObj.SwiftCode))
                {
                    textBottom = textBottom + "\n" + "SWIFT Code: " + airfreightObj.SwiftCode;
                }
                textBottom = textBottom + "\n" + "Thanks for your kind co-operation.";

                string idTexRoot = workSheet
               .Cells[row1 + 3, 3]
               .First(c => c.Value.ToString() == "Kindly arrange the payment to: ")
               .Start
               .Address;

                string idTexBottom = workSheet
                .Cells[row1 + 9, 9]
                .Start
                .Address;

                workSheet.Cells[row1 + 3, 3].Value = textBottom;

                string addressTextBottom = idTexRoot + ":" + idTexBottom;
                workSheet.Cells[idTexRoot].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[idTexRoot].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[addressTextBottom].Merge = true;
                workSheet.Cells[addressTextBottom].Style.WrapText = true;
                workSheet.Column(4).Width = 20; //Cột D
                workSheet.Cells.AutoFitColumns();

            }
        }

        public void BinddingDataDetailSOAAirfreight(ExcelWorksheet workSheet, ExportSOAAirfreightModel airfreightObj)
        {
            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column B
                excelImage.SetPosition(0, 0, 1, 0);
            }

            string monthName = airfreightObj.SoaFromDate.HasValue ? airfreightObj.SoaFromDate.Value.ToString("MMM", CultureInfo.InvariantCulture) : string.Empty;
            List<string> headers = new List<string>()
            {
               "INDO TRANS LOGISTICS CORPORATION", //0
               "52-54-56 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
               "DEBIT NOTE IN " + monthName?.ToUpper() + " " + airfreightObj.SoaFromDate.Value.Year  + " (BẢNG KÊ CƯỚC VCQT)" , //2
               "TOTAL", //3
            };

            List<string> headerTable = new List<string>()
            {
                "No", //1
                "Job No", //2
                "Flight No", //3
                "ETD", //4
                "MAWB", //5
                "Origin(AOL)", //6
                "Dest(AOD)", //7
                //"Service", //8
                "Air PCS", //9
                "Gross Weight(KG)", //10
                "Chargeable Weight(KG)", //11
                "Rate(USD)", //12
                "AirFreight(USD)", //13
                "Fuel Surcharge(USD)", //14
                "Warisk Surcharge(USD)", //15
                "Screening Surcharge(USD)", //16
                "AWB(USD)", //17
                "AMS(USD)", //18
                "Dangerous fee(USD)", //18
                "Other fee(USD)", //19
                "Handling fee(USD)", //20
                "Net Amount(USD)", //21
                "Exchange Rate(VND/USD)", //22
                "Total Amount(VND)", //23
            };

            workSheet.Cells["H1:U1"].Merge = true;
            workSheet.Cells["H1"].Value = headers[0];
            workSheet.Cells["H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells["H1"].Style.Font.SetFromFont(new Font("Arial Black", 13));
            workSheet.Cells["H1"].Style.Font.Italic = true;
            workSheet.Cells["P2:U2"].Merge = true;
            workSheet.Cells["P2:U2"].Style.WrapText = true;
            workSheet.Cells["P2"].Value = headers[1];
            workSheet.Cells["P2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10));
            workSheet.Row(2).Height = 50;
            //Title
            workSheet.Cells["A3:U3"].Merge = true;
            workSheet.Cells["A3"].Style.Font.SetFromFont(new Font("Times New Roman", 16));
            workSheet.Cells["A3"].Value = headers[2];
            workSheet.Cells["A3"].Style.Font.Size = 16;
            workSheet.Cells["A3"].Style.Font.Bold = true;

            workSheet.Cells["A5:J6"].Style.Font.SetFromFont(new Font("Times New Roman", 11));
            workSheet.Cells["A5:B5"].Merge = true;
            workSheet.Cells["A4:U6"].Merge = true;
            workSheet.Cells["A4:U6"].Style.WrapText = true;

            // Tạo header
            for (int i = 0; i < headerTable.Count; i++)
            {
                //if (i == 5)
                //{
                //    workSheet.Cells[8, i + 2].Value = headerTable[i];
                //}
                //if (i < 5)
                //{
                //    workSheet.Cells[8, i + 1].Value = headerTable[i];
                //}
                //if (i > 5)
                //{
                //    workSheet.Cells[8, i + 2].Value = headerTable[i];
                //    workSheet.Cells[8, i + 2].Style.Font.Bold = true;

                //}
                workSheet.Cells[8, i + 1].Value = headerTable[i];
                workSheet.Cells[8, i + 1].Style.Font.Bold = true;
                workSheet.Cells[8, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[8, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            }

                workSheet.Cells["A8:A9"].Merge = true;
                workSheet.Cells["A8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["B8:B9"].Merge = true;
                workSheet.Cells["B8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["B8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["C8:C9"].Merge = true;
                workSheet.Cells["C8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["C8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["D8:D9"].Merge = true;
                workSheet.Cells["E8:E9"].Merge = true;
                workSheet.Cells["F8:F9"].Merge = true;
                workSheet.Cells["D8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["D8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //workSheet.Cells["E8:E9"].Merge = true;
                workSheet.Cells["E8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["E8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //workSheet.Cells["F8:F9"].Merge = true;
                workSheet.Cells["F8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["F8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["G8:G9"].Merge = true;
                workSheet.Cells["G8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["G8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["H8:H9"].Merge = true;
                workSheet.Cells["H8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["H8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["I8:I9"].Merge = true;
                workSheet.Cells["I8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["I8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["J8:J9"].Merge = true;
                workSheet.Cells["J8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["J8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["J8:J9"].Style.WrapText = true;
                workSheet.Cells["K8:K9"].Merge = true;
                workSheet.Cells["K8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["K8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["K8:K9"].Style.WrapText = true;

                workSheet.Cells["L8:L9"].Merge = true;
                workSheet.Cells["L8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["L8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["M8:M9"].Merge = true;
                workSheet.Cells["M8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["M8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["M8:M9"].Style.WrapText = true;

                workSheet.Cells["N8:N9"].Merge = true;
                workSheet.Cells["N8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["N8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["N8:N9"].Style.WrapText = true;

                workSheet.Cells["O8:O9"].Merge = true;
                workSheet.Cells["O8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["O8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["O8:O9"].Style.WrapText = true;


                workSheet.Cells["P8:P9"].Merge = true;
                workSheet.Cells["P8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["P8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["P8:P9"].Style.WrapText = true;


                workSheet.Cells["Q8:Q9"].Merge = true;
                workSheet.Cells["Q8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["Q8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["Q8:Q9"].Style.WrapText = true;

                workSheet.Cells["R8:R9"].Merge = true;
                workSheet.Cells["R8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["R8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["R8:R9"].Style.WrapText = true;

                workSheet.Cells["S8:S9"].Merge = true;
                workSheet.Cells["S8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["S8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["S8:S9"].Style.WrapText = true;


                workSheet.Cells["T8:T9"].Merge = true;
                workSheet.Cells["T8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["T8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["T8:T9"].Style.WrapText = true;
                workSheet.Cells["U8:U9"].Merge = true;
                workSheet.Cells["U8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["U8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["U8:U9"].Style.WrapText = true;
            //workSheet.Cells["E8:F9"].Merge = true;


            string textHead = string.Empty;
            if (!string.IsNullOrEmpty(airfreightObj.PartnerNameEn))
            {
                textHead = textHead + airfreightObj.PartnerNameEn + "\n";
            }
            if (!string.IsNullOrEmpty(airfreightObj.PartnerBillingAddress))
            {
                textHead = textHead + airfreightObj.PartnerBillingAddress + "\n";
            }
            if (!string.IsNullOrEmpty(airfreightObj.PartnerTaxCode))
            {
                textHead = textHead + "Tax code: " + airfreightObj.PartnerTaxCode;
            }
            workSheet.Cells["A4"].Value = textHead;


                workSheet.Cells["A7:U7"].Merge = true;

                string textSOA = string.Empty;
                if (!string.IsNullOrEmpty(airfreightObj.SoaNo))
                {
                    textSOA = textSOA + "Bảng kê số: " + airfreightObj.SoaNo + " đính kèm hóa đơn số: ";
                }
                if (airfreightObj.DateSOA != null)
                {
                    DateTime dateSOA = airfreightObj.DateSOA ?? DateTime.Now;
                    textSOA = textSOA + dateSOA.ToString("dd/MM/yyyy");
                }
                workSheet.Cells["A7"].Value = textSOA;

                workSheet.Cells["X8:X9"].Merge = true;
                workSheet.Cells["W8:W9"].Merge = true;

            int addressStartContent = 10;
            int row = addressStartContent - 1;
            int row1 = addressStartContent - 1;

            for (int i = 0; i < airfreightObj.HawbAirFrieghts.Count; i++)
            {
                var item = airfreightObj.HawbAirFrieghts[i];
                workSheet.Cells[i + addressStartContent, 1].Value = i + 1;
                workSheet.Cells[i + addressStartContent, 2].Value = item.JobNo;
                workSheet.Cells[i + addressStartContent, 3].Value = item.FlightNo;
                workSheet.Cells[i + addressStartContent, 4].Value = item.ShippmentDate;
                workSheet.Cells[i + addressStartContent, 4].Style.Numberformat.Format = "dd/MM/yyyy";

                workSheet.Cells[i + addressStartContent, 5].Value = item.Mawb;

                //workSheet.Cells[i + addressStartContent, 6].Value = item.AOL;
                workSheet.Cells[i + addressStartContent, 7].Value = item.AOD;
                //workSheet.Cells[i + addressStartContent, 8].Value = item.Service;
                workSheet.Cells[i + addressStartContent, 8].Value = item.Pcs;
                workSheet.Cells[i + addressStartContent, 8].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[i + addressStartContent, 9].Value = item.GW;
                workSheet.Cells[i + addressStartContent, 9].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[i + addressStartContent, 10].Value = item.CW;
                workSheet.Cells[i + addressStartContent, 10].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[i + addressStartContent, 11].Value = item.Rate;
                workSheet.Cells[i + addressStartContent, 11].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 12].Value = item.AirFreight;
                workSheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 13].Value = item.FuelSurcharge;
                workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 14].Value = item.WarriskSurcharge;
                workSheet.Cells[i + addressStartContent, 14].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 15].Value = item.ScreeningFee;
                workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 16].Value = item.AWB;
                workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 17].Value = item.AMS;
                workSheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 18].Value = item.DAN;
                workSheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 19].Value = item.OTH;
                workSheet.Cells[i + addressStartContent, 19].Style.Numberformat.Format = numberFormat;


                workSheet.Cells[i + addressStartContent, 20].Value = item.HandlingFee;
                workSheet.Cells[i + addressStartContent, 20].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 21].Value = item.NetAmount;
                workSheet.Cells[i + addressStartContent, 21].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 22].Value = item.ExchangeRate;
                workSheet.Cells[i + addressStartContent, 22].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 23].Value = item.TotalAmount;
                workSheet.Cells[i + addressStartContent, 23].Style.Numberformat.Format = numberFormat;


                row1++;

            }
            workSheet.Cells[8, 1, row1 + 1, 23].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[8, 1, row1, 23].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[8, 1, row1, 23].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            addressStartContent = addressStartContent + airfreightObj.HawbAirFrieghts.Count;

            workSheet.Cells[addressStartContent, 1].Value = headers[3]; //Total


            string idT = workSheet
            .Cells[addressStartContent, 1]
            .First(c => c.Value.ToString() == "TOTAL")
            .Start
            .Address;

            string idT1 = workSheet
            .Cells[addressStartContent, 7]
            .Start
            .Address;
            string totalStr = idT + ":" + idT1;

            workSheet.Cells[totalStr].Merge = true;
            workSheet.Cells[idT].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[idT].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[idT].Style.Font.Bold = true;

            string idPCS = workSheet
           .Cells[addressStartContent, 8]
           .Start
           .Address;
            workSheet.Cells[idPCS].Value = airfreightObj.HawbAirFrieghts.Select(t => t.Pcs).Sum();
            workSheet.Cells[idPCS].Style.Numberformat.Format = numberFormat;


            string idGW = workSheet
            .Cells[addressStartContent, 9]
            .Start
            .Address;
            workSheet.Cells[idGW].Value = airfreightObj.HawbAirFrieghts.Select(t => t.GW).Sum();
            workSheet.Cells[idGW].Style.Numberformat.Format = numberFormat;


            string idCW = workSheet
            .Cells[addressStartContent, 10]
            .Start
            .Address;
            workSheet.Cells[idCW].Value = airfreightObj.HawbAirFrieghts.Select(t => t.CW).Sum();
            workSheet.Cells[idCW].Style.Numberformat.Format = numberFormat;


            string idAF = workSheet
              .Cells[addressStartContent, 12]
              .Start
              .Address;
            workSheet.Cells[idAF].Value = airfreightObj.HawbAirFrieghts.Select(t => t.AirFreight).Sum();
            workSheet.Cells[idAF].Style.Numberformat.Format = numberFormat;


            string idHF = workSheet
             .Cells[addressStartContent, 20]
             .Start
             .Address;
            workSheet.Cells[idHF].Value = airfreightObj.HawbAirFrieghts.Select(t => t.HandlingFee).Sum();
            workSheet.Cells[idHF].Style.Numberformat.Format = numberFormat;

            string idNA = workSheet
             .Cells[addressStartContent, 21]
             .Start
             .Address;
            workSheet.Cells[idNA].Value = airfreightObj.HawbAirFrieghts.Select(t => t.NetAmount).Sum();
            workSheet.Cells[idNA].Style.Numberformat.Format = numberFormat;

            string idTT = workSheet
             .Cells[addressStartContent, 23]
             .Start
             .Address;
            workSheet.Cells[idTT].Value = airfreightObj.HawbAirFrieghts.Select(t => t.TotalAmount).Sum();
            workSheet.Cells[idTT].Style.Numberformat.Format = numberFormat;

            //format
            workSheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A3"].Style.Font.Bold = true;
            workSheet.Cells["A4"].Style.Font.Bold = true;
            workSheet.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A7"].Style.Font.Bold = true;

            workSheet.Cells[idPCS].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idPCS].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idPCS].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[idPCS].Style.Border.Left.Style = ExcelBorderStyle.Thin;

            string idToBoder = workSheet
             .Cells[addressStartContent, 10]
             .Start
             .Address;

            string idToborder1 = workSheet
            .Cells[addressStartContent, 23]
            .Start
            .Address;
            string addressBorderRight = idToBoder + ":" + idToborder1;
            workSheet.Cells[addressBorderRight].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            string addressTextCenter = "A8" + ":" + idToborder1;

            workSheet.Cells[addressTextCenter].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[addressTextCenter].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[row1 + 2, 4].Value = "Issued by";
            workSheet.Cells[row1 + 2, 4].Style.Font.SetFromFont(new Font("Times New Roman", 13));

            workSheet.Cells[row1 + 2, 7].Value = "Approved by";
            workSheet.Cells[row1 + 2, 7].Style.Font.SetFromFont(new Font("Times New Roman", 13));

            workSheet.Cells[row1 + 2, 13].Value = "Account";
            workSheet.Cells[row1 + 2, 13].Style.Font.SetFromFont(new Font("Times New Roman", 13));


            //Bôi đen header
            workSheet.Cells["A8:K8"].Style.Font.Bold = true;
            workSheet.Column(1).Width = 8; //Cột A
            workSheet.Column(2).Width = 15; //Cột B
            workSheet.Column(3).Width = 15; //Cột C
            workSheet.Column(4).Width = 15; //Cột D
            workSheet.Column(5).Width = 17; //Cột E
            workSheet.Column(6).Width = 17; //Cột F
            workSheet.Column(10).Width = 15;//Cột J
            workSheet.Column(11).Width = 15;//Cột K
            workSheet.Column(12).Width = 15;//Cột L
            workSheet.Column(13).Width = 15;//Cột M
            workSheet.Column(14).Width = 15;//Cột M
            workSheet.Column(15).Width = 15; //Cột N
            workSheet.Column(16).Width = 15;  //Cột O
            workSheet.Column(17).Width = 15;   //Cột P
            workSheet.Column(18).Width = 15;     //Cột Q
            workSheet.Column(19).Width = 15;  //Cột R
            workSheet.Column(20).Width = 15;   //Cột S
            workSheet.Column(21).Width = 20;   //Cột T
            workSheet.Column(22).Width = 25;   //Cột U

            workSheet.Cells[row1 + 3, 3].Value = "Kindly arrange the payment to: ";
            string textBottom = workSheet.Cells[row1 + 3, 3].Value.ToString();

            if (!string.IsNullOrEmpty(airfreightObj.OfficeEn))
            {
                textBottom = textBottom + "\n" + airfreightObj.OfficeEn;
            }


            if (!string.IsNullOrEmpty(airfreightObj.BankAccountVND))
            {
                textBottom = textBottom + "\n" + "A/C: " + (!string.IsNullOrEmpty (airfreightObj.BankAccountUsd) ? (airfreightObj.BankAccountUsd +  " - ") : string.Empty)  + airfreightObj.BankAccountVND;
            }

            if (!string.IsNullOrEmpty(airfreightObj.BankNameEn))
            {
                textBottom = textBottom + "\n" + "Via: " + airfreightObj.BankNameEn;
            }

            if (!string.IsNullOrEmpty(airfreightObj.AddressEn))
            {
                textBottom = textBottom + "\n" + airfreightObj.AddressEn;
            }

            if (!string.IsNullOrEmpty(airfreightObj.SwiftCode))
            {
                textBottom = textBottom + "\n" + "SWIFT Code: " + airfreightObj.SwiftCode;
            }
            textBottom = textBottom + "\n" + "Thanks for your kind co-operation.";

             string idTexRoot = workSheet
            .Cells[row1 + 3, 3]
            .First(c => c.Value.ToString() == "Kindly arrange the payment to: ")
            .Start
            .Address;

             string idTexBottom = workSheet
             .Cells[row1+ 9, 9]
             .Start
             .Address;

             workSheet.Cells[row1 + 3, 3].Value = textBottom;

             string addressTextBottom = idTexRoot + ":" + idTexBottom;
             workSheet.Cells[idTexRoot].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
             workSheet.Cells[idTexRoot].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
             workSheet.Cells[addressTextBottom].Merge = true;
             workSheet.Cells[addressTextBottom].Style.WrapText = true;
             workSheet.Column(4).Width = 20; //Cột D
            workSheet.Cells.AutoFitColumns();

        }

        // BinddingDataDetailSOAAirfreightCredit
        public void BinddingDataDetailSOASupplierAirfreight(ExcelWorksheet workSheet, ExportSOAAirfreightModel airfreightObj)
        {
            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column B
                excelImage.SetPosition(0, 0, 1, 0);
            }

            List<string> headers = new List<string>()
            {
               "INDO TRANS LOGISTICS CORPORATION", //0
               "HEAD OFFICE:", //1
               "52 Truong Son St., Tan Binh Dist.\nHo Chi Minh City, Vietnam\nTel: (848) 848 8567 (8 lines)" +
               "\nFax: (848) 848 8593 - 848 8570\nE-mail: indo-trans@itlvn.com\nWebsite: www.itlvn.com", //2
               "CREDIT NOTE IN {0} ( BẢNG KÊ CƯỚC VCQT)", //3
               "TOTAL", //4
            };

            List<string> headerTable = new List<string>()
            {
               "No", //5
               "Job No", //6
               "Flight\nno", //7
               "ETD", //8
               "MAWB", //9
               "Origin\n(AOL)", //10
               "Dest\n(AOD)", //11
               "Gross\nWeight\n(KG)", //12
               "Chargeable\nWeight\n(KG)", //13
               "Rate\n(USD)", //14
               "AirFreight\n(USD)", //15
               "Fuel\nSurcharge\n(USD)", //16
               "War risk\nSurcharge\n(USD)", //17
               "X-ray\nSurcharge\n(USD)", //18
               "AWB\n(USD)", //19
               "AMS\n(USD)", //20
               "Dangerous\nfee\n(USD)", //21
               "Other\nfee\n(USD)", //22
               "Handling fee\n(USD)", //23
               "Net\nAmount\n(USD)", //24
            };

            // Custom With Column
            workSheet.Column(1).Width = 4;  //Cột A
            workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(2).Width = 15; //Cột B
            workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(3).Width = 15; //Cột C
            workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(4).Width = 10; //Cột D
            workSheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(5).Width = 17; //Cột E
            workSheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(6).Width = 10; //Cột F
            workSheet.Column(7).Width = 10; //Cột G
            workSheet.Column(8).Width = 15; //Cột H
            workSheet.Column(9).Width = 15; //Cột I
            workSheet.Column(10).Width = 12; //Cột J
            workSheet.Column(10).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(11).Width = 13; //Cột K
            workSheet.Column(12).Width = 13; //Cột L
            workSheet.Column(13).Width = 13; //Cột M
            workSheet.Column(14).Width = 13; //Cột N
            workSheet.Column(15).Width = 13; //Cột O
            workSheet.Column(16).Width = 13; //Cột P
            workSheet.Column(17).Width = 13; //Cột Q
            workSheet.Column(18).Width = 13; //Cột R
            workSheet.Column(19).Width = 13; //Cột S
            workSheet.Column(20).Width = 13; //Cột T

            // Header0
            workSheet.Cells["O1:T1"].Merge = true;
            workSheet.Cells["O1"].Value = headers[0];
            workSheet.Cells["O1"].Style.Font.SetFromFont(new Font("Arial Black", 13, FontStyle.Bold));
            workSheet.Cells["O1"].Style.Font.Italic = true;
            // Header1
            workSheet.Cells["O2:T2"].Merge = true;
            workSheet.Cells["O2"].Value = headers[1];
            workSheet.Cells["O2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10, FontStyle.Bold));
            // Header2
            workSheet.Cells["O3:T3"].Merge = true;
            workSheet.Cells["O3:T3"].Style.WrapText = true;
            workSheet.Cells["O3"].Value = headers[2];
            workSheet.Cells["O3"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10));
            workSheet.Row(3).Height = 75;

            //Title
            var dateSOA = airfreightObj.DateSOA ?? DateTime.Now;
            workSheet.Cells["A4:T4"].Merge = true;
            workSheet.Cells["A4"].Style.Font.SetFromFont(new Font("Times New Roman", 16, FontStyle.Bold));
            workSheet.Cells["A4"].Value = string.Format(headers[3], dateSOA.ToString("MMM yyyy").ToUpper());
            workSheet.Cells["A4"].Style.Font.Size = 16;
            workSheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["A5:T6"].Merge = true;
            workSheet.Cells["A5:T6"].Style.WrapText = true;
            workSheet.Cells["A5:T6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A5:T6"].Style.Font.SetFromFont(new Font("Times New Roman", 11, FontStyle.Bold));

            string textHead = string.Empty;
            if (!string.IsNullOrEmpty(airfreightObj.PartnerNameEn))
            {
                textHead += airfreightObj.PartnerNameEn + "\n";
            }
            if (!string.IsNullOrEmpty(airfreightObj.PartnerTaxCode))
            {
                textHead += "Tax code: " + airfreightObj.PartnerTaxCode;
            }
            workSheet.Cells["A5"].Value = textHead;

            string textSOA = string.Empty; ;
            if (!string.IsNullOrEmpty(airfreightObj.SoaNo))
            {
                textSOA = "Bảng kê số: " + airfreightObj.SoaNo + " đính kèm hóa đơn số: ";
            }
            workSheet.Cells["A7"].Value = textSOA;
            workSheet.Cells["A7"].Style.Font.Bold = true;
            workSheet.Cells["A7:T7"].Merge = true;
            workSheet.Cells["A7:T7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A7:T7"].Style.Border.Top.Style = ExcelBorderStyle.Thin;

            // Tạo header
            workSheet.Row(8).CustomHeight = true;
            workSheet.Row(8).Height = 60;
            for (int i = 0; i < headerTable.Count; i++)
            {
                workSheet.Cells[8, i + 1].Value = headerTable[i];
                workSheet.Cells[8, i + 1].Style.Font.Bold = true;
                workSheet.Cells[8, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[8, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[8, i + 1].Style.WrapText = true;
            }

            int addressStartContent = 9;
            int row1 = addressStartContent - 1;

            for (int i = 0; i < airfreightObj.HawbAirFrieghts.Count; i++)
            {
                var item = airfreightObj.HawbAirFrieghts[i];
                workSheet.Cells[i + addressStartContent, 1].Value = i + 1; // No
                workSheet.Cells[i + addressStartContent, 2].Value = item.JobNo; // JobNo
                workSheet.Cells[i + addressStartContent, 2].Style.Font.Bold = true;
                workSheet.Cells[i + addressStartContent, 3].Value = item.FlightNo; // FlightNo
                workSheet.Cells[i + addressStartContent, 4].Value = item.ShippmentDate; // ETD
                workSheet.Cells[i + addressStartContent, 4].Style.Numberformat.Format = "dd MMM y";

                workSheet.Cells[i + addressStartContent, 5].Value = item.Mawb; // MAWB
                workSheet.Cells[i + addressStartContent, 6].Value = item.AOL; // AOL
                workSheet.Cells[i + addressStartContent, 7].Value = item.AOD; // AOD
                // Gross Weight
                workSheet.Cells[i + addressStartContent, 8].Value = item.GW;
                workSheet.Cells[i + addressStartContent, 8].Style.Numberformat.Format = numberFormat;
                // Chargeable Weight
                workSheet.Cells[i + addressStartContent, 9].Value = item.CW;
                workSheet.Cells[i + addressStartContent, 9].Style.Numberformat.Format = numberFormat;
                // Rate
                workSheet.Cells[i + addressStartContent, 10].Value = item.Rate;
                workSheet.Cells[i + addressStartContent, 10].Style.Numberformat.Format = numberFormat;
                // AirFreight
                workSheet.Cells[i + addressStartContent, 11].Value = item.AirFreight;
                workSheet.Cells[i + addressStartContent, 11].Style.Numberformat.Format = numberFormat;
                // Fuel Surcharge
                workSheet.Cells[i + addressStartContent, 12].Value = item.FuelSurcharge;
                workSheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = numberFormat;
                // War Risk Surcharge
                workSheet.Cells[i + addressStartContent, 13].Value = item.WarriskSurcharge;
                workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat;
                // X-Ray Surcharge
                workSheet.Cells[i + addressStartContent, 14].Value = item.ScreeningFee;
                workSheet.Cells[i + addressStartContent, 14].Style.Numberformat.Format = numberFormat;
                // AWB
                workSheet.Cells[i + addressStartContent, 15].Value = item.AWB;
                workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = numberFormat;
                // AMS
                workSheet.Cells[i + addressStartContent, 16].Value = item.AMS;
                workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = numberFormat;
                // Dangerous Fee
                workSheet.Cells[i + addressStartContent, 17].Value = item.DAN;
                workSheet.Cells[i + addressStartContent, 17].Style.Numberformat.Format = numberFormat;
                // Other Fee
                workSheet.Cells[i + addressStartContent, 18].Value = item.OTH;
                workSheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = numberFormat;
                // Handling Fee
                workSheet.Cells[i + addressStartContent, 19].Value = item.HandlingFee;
                workSheet.Cells[i + addressStartContent, 19].Style.Numberformat.Format = numberFormat;
                // NetAmount
                workSheet.Cells[i + addressStartContent, 20].Value = item.NetAmount;
                workSheet.Cells[i + addressStartContent, 20].Style.Numberformat.Format = numberFormat;
                row1++;
            }
            workSheet.Cells[8, 1, row1 + 1, 20].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[8, 1, row1 + 1, 20].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row1 + 1, 1, row1 + 1, 20].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            addressStartContent = addressStartContent + airfreightObj.HawbAirFrieghts.Count;

            workSheet.Cells[addressStartContent, 3].Value = headers[4]; //Total
            workSheet.Cells[addressStartContent, 3].Style.Font.Bold = true;
            workSheet.Cells[addressStartContent, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            // Total Gross Weight
            string idGW = workSheet
            .Cells[addressStartContent, 8]
            .Start
            .Address;
            workSheet.Cells[idGW].Value = airfreightObj.HawbAirFrieghts.Select(t => t.GW).Sum();
            workSheet.Cells[idGW].Style.Numberformat.Format = numberFormat;

            // Total Chargeable Weight
            string idCW = workSheet
            .Cells[addressStartContent, 9]
            .Start
            .Address;
            workSheet.Cells[idCW].Value = airfreightObj.HawbAirFrieghts.Select(t => t.CW).Sum();
            workSheet.Cells[idCW].Style.Numberformat.Format = numberFormat;

            // Total Handling Fee
            string idHF = workSheet
             .Cells[addressStartContent, 19]
             .Start
             .Address;
            workSheet.Cells[idHF].Value = airfreightObj.HawbAirFrieghts.Select(t => t.HandlingFee).Sum();
            workSheet.Cells[idHF].Style.Numberformat.Format = numberFormat;

            // Total Net Amount
            string idNA = workSheet
             .Cells[addressStartContent, 20]
             .Start
             .Address;
            workSheet.Cells[idNA].Value = airfreightObj.HawbAirFrieghts.Select(t => t.NetAmount).Sum();
            workSheet.Cells[idNA].Style.Numberformat.Format = numberFormat;

            workSheet.Cells[row1 + 2, 13].Value = "Account";
            workSheet.Cells[row1 + 2, 13].Style.Font.SetFromFont(new Font("Times New Roman", 13));

            // Account info
            string textBottom = "Kindly arrange the payment to: ";
            if (!string.IsNullOrEmpty(airfreightObj.OfficeEn))
            {
                textBottom += "\n" + airfreightObj.OfficeEn;
            }
            if (!string.IsNullOrEmpty(airfreightObj.BankAccountUsd) || !string.IsNullOrEmpty(airfreightObj.BankAccountVND))
            {
                textBottom += "\nA/C: ";
                textBottom += (!string.IsNullOrEmpty(airfreightObj.BankAccountUsd) ? "USD " : "") + airfreightObj.BankAccountUsd;
                textBottom += (!string.IsNullOrEmpty(airfreightObj.BankAccountVND) ? (!string.IsNullOrEmpty(airfreightObj.BankAccountUsd) ? " - " : "") + "VND " : "") + airfreightObj.BankAccountVND;
                textBottom = textBottom.Replace("(USD)", "");
                textBottom = textBottom.Replace("(VND)", "");
            }
            if (!string.IsNullOrEmpty(airfreightObj.BankNameEn))
            {
                textBottom += "\nVia: " + airfreightObj.BankNameEn;
            }
            if (!string.IsNullOrEmpty(airfreightObj.AddressEn))
            {
                textBottom += "\n" + airfreightObj.AddressEn;
            }
            if (!string.IsNullOrEmpty(airfreightObj.SwiftCode))
            {
                textBottom += "\nSWIFT Code: " + airfreightObj.SwiftCode;
            }
            textBottom += "\nThanks for your kind co-operation.";
            var rowNum = textBottom.Split('\n').Length - 1;
            workSheet.Cells[row1 + 3, 3].Value = textBottom;
            workSheet.Cells[row1 + 3, 3, row1 + 3 + rowNum, 10].Merge = true;
            workSheet.Cells[row1 + 3, 3, row1 + 3 + rowNum, 10].Style.WrapText = true;
            workSheet.Cells[row1 + 3, 3, row1 + 3 + rowNum, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }
        #endregion


        #region --- SETTLEMENT PAYMENT ---
        /// <summary>
        /// Generate detail settlement payment excel
        /// </summary>
        /// <param name="settlementExport"></param>
        /// <param name="language"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateDetailSettlementPaymentExcel(SettlementExport settlementExport, string language, string type, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    string sheetName = language == "VN" ? "(V)" : "(E)";
                    excelPackage.Workbook.Worksheets.Add("Đề nghị thanh toán " + sheetName);
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    if (type == "SettlementPaymentTemplate")
                    {
                        BindingDataDetailSettlementPaymentSOAExcel(workSheet, settlementExport, language);
                    }
                    else
                    {
                        BindingDataDetailSettlementPaymentExcel(workSheet, settlementExport, language);
                    }
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void SetWidthColumnExcelDetailSettlementPayment(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 5; //Cột A
            workSheet.Column(2).Width = 21; //Cột B
            workSheet.Column(3).Width = 24; //Cột C
            workSheet.Column(4).Width = 6; //Cột D
            workSheet.Column(5).Width = 30; //Cột E
            workSheet.Column(6).Width = 20; //Cột F
            workSheet.Column(7).Width = 20; //Cột G
            workSheet.Column(8).Width = 20; //Cột H
            workSheet.Column(9).Width = 12; //Cột G
            workSheet.Column(10).Width = 12; //Cột H
            workSheet.Column(11).Width = 20; //Cột I
            workSheet.Column(12).Width = 12; //Cột J
            workSheet.Column(13).Width = 18; //Cột K
        }
        private void SetWidthColumnExcelDetailSettlementPaymentSOA(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 30; //Cột A
            workSheet.Column(2).Width = 20; //Cột B
            workSheet.Column(3).Width = 20; //Cột C
            workSheet.Column(4).Width = 30; //Cột D
            workSheet.Column(5).Width = 30; //Cột E
            workSheet.Column(6).Width = 20; //Cột F
            workSheet.Column(7).Width = 20; //Cột G
        }

        private List<string> GetHeaderExcelDetailSettlementPayment(string language)
        {
            List<string> vnHeaders = new List<string>()
            {
                "INDO TRANS LOGISTICS CORPORATION", //0
                "52-54-65 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
                "ĐỀ NGHỊ THANH TOÁN", //2
                "Người yêu cầu:", //3
                "Ngày:", //4
                "Bộ phận:", //5
                "Số chứng từ:", //6
                "STT", //7
                "Thông tin chung", //8
                "Diễn giải", //9
                "Số tiền", //10
                "Số hóa đơn", //11
                "Ghi chú", //12
                "Số tiền đã tạm ứng", //13
                "Ngày tạm ứng", //14
                "Chênh lệch", //15
                "Số lô hàng:", //16
                "Số tờ khai:", //17
                "Số HBL/HAWB:", //18
                "Số MBL/MAWB:", //19
                "Số cont - Loại cont:", //20
                "C.W (Kgs):", //21
                "Số kiện (Pcs):", //22
                "Số CBM (CBM):", //23
                "Khách hàng:", //24
                "Công ty/cá nhân xuất:", //25
                "Công ty/cá nhân nhập:", //26
                "Chi phí có hóa đơn", //27
                "Chi phí không hóa đơn", //28
                "Chi hộ", //29
                "Tổng cộng", //30
                "TỔNG CỘNG", //31
                "Người tạm ứng\n(Ký, ghi rõ họ tên)", //32
                "Người chứng từ\n(Ký, ghi rõ họ tên)", //33
                "Trưởng bộ phận\n(Ký, ghi rõ họ tên)", //34
                "Kế toán\n(Ký, ghi rõ họ tên)", //35
                "Giám đốc\n(Ký, ghi rõ họ tên)", //36
                "Chứng từ:", //37
                "Đối tượng thanh toán:", // 38
                "Chuyển khoản:", // 39
                "Tên người thụ hưởng:", // 40
                "Số tài khoản:", // 41
                "Tên Ngân hàng:", // 42
                "Mã ngân hàng:", // 43
                "Tiền mặt:", // 44
                "Ngày đến hạn:", // 45
                "Số tiền trước thuế", // 46
                "Tiền thuế", // 47
                "Tổng tiền", // 48
                "Ngày dịch vụ:", //49
                "Hình thức thanh toán:",//50
            };

            List<string> engHeaders = new List<string>()
            {
                "INDO TRANS LOGISTICS CORPORATION", //0
                "52-54-65 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
                "PAYMENT REQUEST", //2
                "Employee Name:", //3
                "Date requested:", //4
                "Department:", //5
                "Ref No.:", //6
                "No.", //7
                "Shipment's Information", //8
                "Description", //9
                "Amount", //10
                "Invoice No", //11
                "Remark", //12
                "Advanced Amount", //13
                "Date Advanced", //14
                "Difference", //15
                "Job ID:", //16
                "Customs Clearance No:", //17
                "HBL/HAWB:", //18
                "MBL/MAWB:", //19
                "Number of cont:", //20
                "C.W (Kgs):", //21
                "Packages (Pcs):", //22
                "Số CBM (CBM):", //23
                "Customer:", //24
                "Consigner:", //25
                "Consignee:", //26
                "Expenses with reasonable vouchers", //27
                "Expenses without reasonable vouchers", //28
                "Payment on behalf", //29
                "Subtotal", //30
                "TOTAL",//31
                "Employee\n(Name, Signature)", //32
                "Documentation Staff\n(Name, Signature)", //33
                "Head of Department\n(Name, Signature)", //34
                "Chief Accountant\n(Name, Signature)", //35
                "Director\n(Name, Signature)", //36
                "Doc CS:", //37
                "Supplier name:", // 38
                "By Bank transfer:", // 39
                "Beneficiary:", // 40
                "Acc No:", // 41
                "Bank:", // 42
                "Bank code:", // 43
                "By cash:", // 44
                "Due date:", // 45
                "Amount befor Tax", // 46
                "Tax", // 47
                "Amount after Tax", // 48
                "Service date:", //49
                "Payment method:"//50
            };

            List<string> headers = language == "VN" ? vnHeaders : engHeaders;
            return headers;
        }
        private List<string> GetHeaderExcelDetailSettlementPaymentSOA(string language)
        {
            List<string> Headers = new List<string>()
            {
                "INDO TRANS LOGISTICS CORPORATION", //0
                "52-54-65 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com", //1
                "PAYMENT REQUEST", //2
                "Requester:", //3
                "Date SOA:", //4
                "SOA No.:", //5
                "Department:", //6
                "Reason for request", //7
                "Job ID:", //8
                "Invoice No", //9
                "Custom No:", //10
                "H-BL No.\n(HAWB):", //11
                "M-BL No.\n(MAWB):", //12
                "Amount(VND)",//13
                "OBH",//14
                "Credit",//15
                "Total Amount",//16
                "Balance", //17
                "Requester\n(Name, Signature)", //18
                "Head of Department\n(Name, Signature)", //19
                "Chief Accountant\n(Name, Signature)", //20
                "Director\n(Name, Signature)", //21
                "Supplier name:", // 22
                "By Bank transfer:", // 23
                "Beneficiary:", // 24
                "Acc No:", // 25
                "Bank:", // 16
                "Bank code:", // 27
                "By cash:", // 28
                "Due date:", // 29
                "Payment method:", //30

            };

            List<string> headers = Headers;
            return headers;
        }

        private void BindingDataDetailSettlementPaymentExcel(ExcelWorksheet workSheet, SettlementExport settlementExport, string language)
        {
            workSheet.Cells.Style.Font.SetFromFont(new Font("Times New Roman", 11));
            workSheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            SetWidthColumnExcelDetailSettlementPayment(workSheet);

            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column A
                excelImage.SetPosition(0, 0, 0, 0);
            }

            List<string> headers = GetHeaderExcelDetailSettlementPayment(language);

            #region #Header
            workSheet.Cells["J1:M1"].Merge = true;
            workSheet.Cells["J1"].Value = headers[0];
            workSheet.Cells["J1"].Style.Font.SetFromFont(new Font("Arial Black", 11));
            workSheet.Cells["J1"].Style.Font.Italic = true;
            workSheet.Cells["J2:M2"].Merge = true;
            workSheet.Cells["J2"].Style.WrapText = true;
            workSheet.Cells["J2"].Value = settlementExport.InfoSettlement.ContactOffice;
            workSheet.Cells["J2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 9));
            workSheet.Cells["J2"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Row(2).Height = 60;

            //Title
            workSheet.Cells["A3:K3"].Merge = true;
            workSheet.Cells["A3"].Style.Font.SetFromFont(new Font("Times New Roman", 18));
            workSheet.Cells["A3"].Value = headers[2]; //Đề nghị thanh toán
            workSheet.Cells["A3"].Style.Font.Bold = true;
            workSheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A5:J6"].Style.Font.SetFromFont(new Font("Times New Roman", 12));

            workSheet.Cells["A5:B5"].Merge = true;
            workSheet.Cells["A5"].Value = headers[3]; //Người yêu cầu
            workSheet.Cells["A5"].Style.Font.Bold = true;
            workSheet.Cells["C5"].Value = settlementExport.InfoSettlement.Requester;

            workSheet.Cells["K5"].Value = headers[4]; //Ngày thanh toán
            workSheet.Cells["K5"].Style.Font.Bold = true;
            workSheet.Cells["L5:M5"].Merge = true;
            workSheet.Cells["L5"].Value = settlementExport.InfoSettlement.RequestDate;
            workSheet.Cells["L5"].Style.Numberformat.Format = "dd MMM, yyyy";
            workSheet.Cells["L5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            workSheet.Cells["A6:B6"].Merge = true;
            workSheet.Cells["A6"].Value = headers[5]; //Bộ phận
            workSheet.Cells["A6"].Style.Font.Bold = true;
            workSheet.Cells["C6"].Value = settlementExport.InfoSettlement.Department;

            workSheet.Cells["K6"].Value = headers[6]; //Số chứng từ
            workSheet.Cells["K6"].Style.Font.Bold = true;
            workSheet.Cells["L6:M6"].Merge = true;
            workSheet.Cells["L6"].Value = settlementExport.InfoSettlement.SettlementNo;

            // Hình thức thanh toán
            workSheet.Cells["A10:B10"].Merge = true;
            workSheet.Cells["A10"].Value = headers[50];
            workSheet.Cells["A10"].Style.Font.Bold = true;

            // Đối tượng thanh toán
            workSheet.Cells["A7:B7"].Merge = true;
            workSheet.Cells["A7"].Value = headers[38];
            workSheet.Cells["A7"].Style.Font.Bold = true;

            // By Bank transfer
            workSheet.Cells["K10"].Value = headers[39];
            workSheet.Cells["K10"].Style.Font.Bold = true;
            workSheet.Cells["L10"].Value = settlementExport.InfoSettlement.PaymentMethod.ToUpper().Contains("BANK") ? "X" : string.Empty;

            // Beneficiary
            workSheet.Cells["K11"].Value = headers[40];
            workSheet.Cells["K11"].Style.Font.Bold = true;
            workSheet.Cells["L11"].Value = settlementExport.InfoSettlement.BankAccountName;

            // Acc No
            workSheet.Cells["K12"].Value = headers[41];
            workSheet.Cells["K12"].Style.Font.Bold = true;
            workSheet.Cells["L12"].Value = settlementExport.InfoSettlement.BankAccountNo;

            // Bank
            workSheet.Cells["K13"].Value = headers[42];
            workSheet.Cells["K13"].Style.Font.Bold = true;
            workSheet.Cells["L13"].Value = settlementExport.InfoSettlement.BankName;

            // Bank Code
            workSheet.Cells["K14"].Value = headers[43];
            workSheet.Cells["K14"].Style.Font.Bold = true;
            workSheet.Cells["L14"].Value = settlementExport.InfoSettlement.BankCode;

            // By Cash
            workSheet.Cells["B11"].Value = headers[44];
            workSheet.Cells["B11"].Style.Font.Bold = true;
            workSheet.Cells["C11"].Value = settlementExport.InfoSettlement.PaymentMethod.ToUpper().Contains("CASH") ? "X" : string.Empty;

            // Due Date
            workSheet.Cells["A9:B9"].Merge = true;
            workSheet.Cells["A9"].Value = headers[45];
            workSheet.Cells["A9"].Style.Font.Bold = true;
            workSheet.Cells["C9"].Value = settlementExport.InfoSettlement.DueDate?.ToString("dd/MM/yyyy");
            #endregion
            //Bôi đen header
            //workSheet.Cells["A15:K15"].Style.Font.Bold = true;

            #region #Format Header Table
            for (var col = 1; col < 14; col++)
            {
                workSheet.Cells[16, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[16, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[16, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[16, col].Style.WrapText = true;
                workSheet.Cells[16, col].Style.Font.Bold = true;
            }
            workSheet.Cells["A16"].Value = headers[7]; // STT

            workSheet.Cells["B16:C16"].Merge = true;
            workSheet.Cells["B16:C16"].Value = headers[8]; // Thông tin chung

            workSheet.Cells["D16:E16"].Merge = true;
            workSheet.Cells["D16:E16"].Value = headers[9]; // Diễn giải

            workSheet.Cells["F16"].Value = headers[46]; // Số tiền trước thuế
            workSheet.Cells["G16"].Value = headers[47]; // Tiền thuế
            workSheet.Cells["H16"].Value = headers[48]; // Tổng tiền
            workSheet.Cells["I16"].Value = headers[11]; // Số hóa đơn
            workSheet.Cells["J16"].Value = headers[12]; // Ghi chú
            workSheet.Cells["K16"].Value = headers[13]; // Số tiền đã tạm ứng
            workSheet.Cells["L16"].Value = headers[14]; // Ngày tạm ứng
            workSheet.Cells["M16"].Value = headers[15]; // Chênh lệch
            workSheet.Row(16).Height = 30;
            workSheet.Cells[16, 1, 16, 13].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            #endregion

            decimal? _sumTotalNetAmount = 0;
            decimal? _sumTotalVatAmount = 0;
            decimal? _sumTotalAmount = 0;
            decimal? _sumTotalAdvancedAmount = 0;
            decimal? _sumTotalDifference = 0;

            int p = 17;
            int j = 17;
            int k = 17;
            for (int i = 0; i < settlementExport.ShipmentsSettlement.Count; i++)
            {
                #region --- Diễn giải ---
                int _no = 1;
                var invoiceCharges = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Where(w => w.ChargeType == "INVOICE");
                //Title Chi phí có hóa đơn
                workSheet.Cells[k, 4].Value = headers[27]; //Chi phí có hóa đơn
                workSheet.Cells[k, 4].Style.Font.Bold = true;
                workSheet.Cells[k, 8].Value = invoiceCharges.Select(s => s.ChargeAmount).Sum(); //Value tổng chi phí có hóa đơn
                workSheet.Cells[k, 8].Style.Font.Bold = true;
                workSheet.Cells[k, 8].Style.Numberformat.Format = numberFormat;
                k += 1;
                foreach (var invoice in invoiceCharges)
                {
                    workSheet.Cells[k, 4].Value = _no;
                    workSheet.Cells[k, 5].Value = invoice.ChargeName;
                    workSheet.Cells[k, 5].Style.WrapText = true;

                    workSheet.Cells[k, 6].Value = invoice.ChargeNetAmount;
                    workSheet.Cells[k, 7].Value = invoice.ChargeVatAmount;
                    workSheet.Cells[k, 8].Value = invoice.ChargeAmount;
                    workSheet.Cells[k, 6, k, 8].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[k, 9].Value = invoice.InvoiceNo;
                    workSheet.Cells[k, 10].Value = invoice.ChargeNote;

                    k += 1;
                    _no += 1;
                }

                var noInvoiceCharges = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Where(w => w.ChargeType == "NO_INVOICE");
                //Title Chi phí không hóa đơn
                workSheet.Cells[k, 4].Value = headers[28]; //Chi phí không hóa đơn
                workSheet.Cells[k, 4].Style.Font.Bold = true;
                workSheet.Cells[k, 8].Value = noInvoiceCharges.Select(s => s.ChargeAmount).Sum(); //Value tổng chi phí không hóa đơn
                workSheet.Cells[k, 8].Style.Font.Bold = true;
                workSheet.Cells[k, 8].Style.Numberformat.Format = numberFormat;
                k += 1;
                foreach (var no_invoice in noInvoiceCharges)
                {
                    workSheet.Cells[k, 4].Value = _no;
                    workSheet.Cells[k, 5].Value = no_invoice.ChargeName;
                    workSheet.Cells[k, 5].Style.WrapText = true;

                    workSheet.Cells[k, 6].Value = no_invoice.ChargeNetAmount;
                    workSheet.Cells[k, 7].Value = no_invoice.ChargeVatAmount;
                    workSheet.Cells[k, 8].Value = no_invoice.ChargeAmount;
                    workSheet.Cells[k, 6, k, 8].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[k, 9].Value = no_invoice.InvoiceNo;
                    workSheet.Cells[k, 10].Value = no_invoice.ChargeNote;

                    k += 1;
                    _no += 1;
                }

                var obhCharges = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Where(w => w.ChargeType == "OBH");
                //Title phí chi hộ
                workSheet.Cells[k, 4].Value = headers[29]; //Chi hộ
                workSheet.Cells[k, 4].Style.Font.Bold = true;
                workSheet.Cells[k, 8].Value = obhCharges.Select(s => s.ChargeAmount).Sum(); //Value tổng phí chi hộ
                workSheet.Cells[k, 8].Style.Font.Bold = true;
                workSheet.Cells[k, 8].Style.Numberformat.Format = numberFormat;
                k += 1;
                foreach (var obh in obhCharges)
                {
                    workSheet.Cells[k, 4].Value = _no;
                    workSheet.Cells[k, 5].Value = obh.ChargeName;
                    workSheet.Cells[k, 5].Style.WrapText = true;

                    workSheet.Cells[k, 6].Value = obh.ChargeNetAmount;
                    workSheet.Cells[k, 7].Value = obh.ChargeVatAmount;
                    workSheet.Cells[k, 8].Value = obh.ChargeAmount;
                    workSheet.Cells[k, 6, k, 8].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[k, 9].Value = obh.InvoiceNo;
                    workSheet.Cells[k, 10].Value = obh.ChargeNote;

                    k += 1;
                    _no += 1;
                }



                #endregion

                #region --- Thông tin chung ---
                workSheet.Cells[j, 2].Value = headers[16]; //Số lô hàng
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].JobNo;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[49]; //Ngày dịch vụ
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].ServiceDate;
                workSheet.Cells[j, 3].Style.Numberformat.Format = "dd MMM, yyyy";
                workSheet.Cells[j, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[17]; //Số tờ khai
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].CustomNo;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[18]; //Số HBL
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].HBL;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[19]; //Số MBL
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].MBL;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[20]; //Số cont - Loại cont
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].Container;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[21]; //C.W
                workSheet.Cells[j, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].Cw;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[22]; //Số kiện
                workSheet.Cells[j, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].Pcs;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[23]; //Số CBM
                workSheet.Cells[j, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].Cbm;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[24]; //Khách hàng
                workSheet.Cells[j, 3].Style.WrapText = true;
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].Customer;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[25]; //Công ty xuất
                workSheet.Cells[j, 3].Style.WrapText = true;
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].Shipper;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[26]; //Công ty nhập
                workSheet.Cells[j, 3].Style.WrapText = true;
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].Consignee;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[37]; //Chứng từ
                workSheet.Cells[j, 3].Style.WrapText = true;
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].PersonInCharge;
                j = j + 1;
                #endregion --- Thông tin chung ---

                j = j + 1;
                k = k + 1;
                if (k > j)
                {
                    j = k;
                }
                else
                {
                    k = j;
                }

                workSheet.Cells[j - 1, 4, j - 1, 13].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[j - 1, 4, j - 1, 13].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[p, 1, p, 13].Style.Border.Top.Style = ExcelBorderStyle.Thin;

                #region --- Change type border diễn giải ---
                for (var f = p; f < j - 2; f++)
                {
                    workSheet.Cells[f, 4, f, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Dotted;
                    workSheet.Cells[f, 4, f, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }
                workSheet.Cells[p, 4, j - 1, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion --- Change type border diễn giải ---

                //Title Subtotal
                workSheet.Cells[j - 1, 4, j - 1, 5].Merge = true;
                workSheet.Cells[j - 1, 4, j - 1, 5].Value = headers[30];
                workSheet.Cells[j - 1, 4, j - 1, 5].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 4, j - 1, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[j - 1, 4, j - 1, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Total net amount
                var _totalNetAmount = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Select(s => s.ChargeNetAmount).Sum();
                workSheet.Cells[j - 1, 6].Value = _totalNetAmount;
                workSheet.Cells[j - 1, 6].Style.Numberformat.Format = decimalFormat2;
                workSheet.Cells[j - 1, 6].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                // Total VAT amount
                var _totalVatAmount = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Select(s => s.ChargeVatAmount).Sum();
                workSheet.Cells[j - 1, 7].Value = _totalVatAmount;
                workSheet.Cells[j - 1, 7].Style.Numberformat.Format = decimalFormat2;
                workSheet.Cells[j - 1, 7].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //Value total amount
                var _totalAmount = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Select(s => s.ChargeAmount).Sum();
                workSheet.Cells[j - 1, 8].Value = _totalAmount;
                workSheet.Cells[j - 1, 8].Style.Numberformat.Format = decimalFormat2;
                workSheet.Cells[j - 1, 8].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //Value total advanced amount (số tiền đã tạm ứng)
                var _advanceAmount = settlementExport.ShipmentsSettlement[i].InfoAdvanceExports.Sum(sum => sum.AdvanceAmount);//settlementExport.ShipmentsSettlement[i].AdvanceAmount ?? 0;
                workSheet.Cells[j - 1, 11].Value = _advanceAmount;
                workSheet.Cells[j - 1, 11].Style.Numberformat.Format = decimalFormat2;
                workSheet.Cells[j - 1, 11].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                workSheet.Cells[j - 1, 12].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //Value chênh lệch
                workSheet.Cells[j - 1, 13].Value = _totalAmount - _advanceAmount;
                workSheet.Cells[j - 1, 13].Style.Numberformat.Format = decimalFormat2;
                workSheet.Cells[j - 1, 13].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 13].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ////

                workSheet.Cells[p, 1, j - 1, 1].Merge = true;
                workSheet.Cells[p, 1, j - 1, 1].Value = i + 1; //Value STT
                workSheet.Cells[p, 1, j - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 1, j - 1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[p, 1, j - 1, 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                workSheet.Cells[p, 11, j - 2, 11].Merge = true;
                workSheet.Cells[p, 11, j - 2, 11].Value = settlementExport.ShipmentsSettlement[i].InfoAdvanceExports.Sum(sum => sum.AdvanceAmount); //Value Số tiền đã tạm ứng
                workSheet.Cells[p, 11, j - 2, 11].Style.Numberformat.Format = decimalFormat2;
                workSheet.Cells[p, 11, j - 2, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 11, j - 2, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[p, 11, j - 2, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                var _requestDateAdvance = settlementExport.ShipmentsSettlement[i].InfoAdvanceExports.Select(s => s.RequestDate).FirstOrDefault();
                var _advanceNo = settlementExport.ShipmentsSettlement[i].InfoAdvanceExports.Select(s => s.AdvanceNo).FirstOrDefault();
                workSheet.Cells[p, 12, j - 2, 12].Merge = true;
                if (!string.IsNullOrEmpty(_advanceNo))
                {
                    workSheet.Cells[p, 12, j - 2, 12].Style.WrapText = true; // Xuống dòng
                    workSheet.Cells[p, 12, j - 2, 12].Value = _requestDateAdvance.HasValue ? _requestDateAdvance.Value.ToString("dd/MM/yyyy") + "\n" + _advanceNo : string.Empty; //Value Ngày tạm ứng + Số tạm ứng
                }
                else
                {
                    workSheet.Cells[p, 12, j - 2, 12].Value = _requestDateAdvance.HasValue ? _requestDateAdvance.Value.ToString("dd/MM/yyyy")  : string.Empty; //Value Ngày tạm ứng
                }
                workSheet.Cells[p, 12, j - 2, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 12, j - 2, 12].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[p, 12, j - 2, 12].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                workSheet.Cells[p, 13, j - 2, 13].Merge = true;
                workSheet.Cells[p, 13, j - 2, 13].Value = string.Empty; //Value Chênh lệch
                workSheet.Cells[p, 13, j - 2, 13].Style.Numberformat.Format = decimalFormat2;
                workSheet.Cells[p, 13, j - 2, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 13, j - 2, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[p, 13, j - 2, 13].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                p = j;
                ////
                _sumTotalNetAmount += (settlementExport.ShipmentsSettlement[i].ShipmentCharges.Select(s => s.ChargeNetAmount).Sum() ?? 0);
                _sumTotalVatAmount += (settlementExport.ShipmentsSettlement[i].ShipmentCharges.Select(s => s.ChargeVatAmount).Sum() ?? 0);
                _sumTotalAmount += (settlementExport.ShipmentsSettlement[i].ShipmentCharges.Select(s => s.ChargeAmount).Sum() ?? 0);
                _sumTotalAdvancedAmount += settlementExport.ShipmentsSettlement[i].InfoAdvanceExports.Sum(sum => sum.AdvanceAmount);//(settlementExport.ShipmentsSettlement[i].AdvanceAmount ?? 0);
                _sumTotalDifference = _sumTotalAmount - _sumTotalAdvancedAmount;
            }

            ////TỔNG CỘNG
            workSheet.Cells[p, 1, p, 5].Merge = true;
            workSheet.Cells[p, 1, p, 5].Value = headers[31]; //Title TỔNG CỘNG
            workSheet.Cells[p, 1, p, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 1, p, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[p, 1, p, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 6].Value = _sumTotalNetAmount; //Value sum total Net amount
            workSheet.Cells[p, 6].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 7].Value = _sumTotalVatAmount; //Value sum total Vat amount
            workSheet.Cells[p, 7].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 8].Value = _sumTotalAmount; //Value sum total amount
            workSheet.Cells[p, 8].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 11].Value = _sumTotalAdvancedAmount; //Value sum total advanced amount
            workSheet.Cells[p, 11].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 12].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 13].Value = _sumTotalDifference; //Value sum total difference
            workSheet.Cells[p, 13].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 13].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //Bôi đen dòng tổng cộng ở cuối
            workSheet.Cells["A" + p + ":M" + p].Style.Font.Bold = true;
            workSheet.Cells["A" + p + ":M" + p].Style.Numberformat.Format = decimalFormat2;

            //In đậm border dòng 15
            workSheet.Cells[15, 1, 15, 13].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + p + ":M" + p].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + (p + 1) + ":M" + (p + 1)].Style.Border.Top.Style = ExcelBorderStyle.Medium;

            for (var i = 16; i < p + 1; i++)
            {
                //In đậm border bên trái của Cột 1
                workSheet.Cells[i, 1].Style.Border.Left.Style = ExcelBorderStyle.Medium;
                //In đậm border Cột 3
                workSheet.Cells[i, 3].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                //In đậm border Cột 10
                workSheet.Cells[i, 10].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                //In đậm border Cột 13
                workSheet.Cells[i, 13].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            }

            p = p + 3;

            workSheet.Cells[p, 2].Style.WrapText = true;
            workSheet.Cells[p, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[p, 2].Value = headers[32]; //Người tạm ứng

            workSheet.Cells[p, 3].Style.WrapText = true;
            workSheet.Cells[p, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[p, 3].Value = headers[33]; //Người chứng từ

            workSheet.Cells[p, 4, p, 5].Merge = true;
            workSheet.Cells[p, 4, p, 5].Style.WrapText = true;
            workSheet.Cells[p, 4, p, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 4, p, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[p, 4, p, 5].Value = headers[34]; //Trưởng bộ phận

            workSheet.Cells[p, 8, p, 10].Merge = true;
            workSheet.Cells[p, 8, p, 10].Style.WrapText = true;
            workSheet.Cells[p, 8, p, 10].Value = headers[35]; //Kế toán
            workSheet.Cells[p, 8, p, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 8, p, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[p, 11, p, 13].Merge = true;
            workSheet.Cells[p, 11, p, 13].Value = headers[36]; //Giám đốc
            workSheet.Cells[p, 11, p, 13].Style.WrapText = true;
            workSheet.Cells[p, 11, p, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 11, p, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(p).Height = 50;

            p = p + 1;

            if (settlementExport.InfoSettlement.IsRequesterApproved)
            {
                AddIconTick(workSheet, p, 2); //Tick Requester
            }

            if (settlementExport.InfoSettlement.IsManagerApproved)
            {
                workSheet.Cells[p, 4, p, 5].Merge = true;
                AddIconTick(workSheet, p, 4); //Tick Manager Dept
            }

            if (settlementExport.InfoSettlement.IsAccountantApproved)
            {
                workSheet.Cells[p, 8, p, 10].Merge = true;
                AddIconTick(workSheet, p, 8); //Tick Accountant
            }

            if (settlementExport.InfoSettlement.IsBODApproved)
            {
                workSheet.Cells[p, 11, p, 13].Merge = true;
                AddIconTick(workSheet, p, 11); //Tick BOD
            }

            workSheet.Row(p).Height = 50;

            p = p + 1;

            workSheet.Cells[p, 2].Style.WrapText = true;
            workSheet.Cells[p, 2].Value = settlementExport.InfoSettlement.Requester; //Value Người tạm ứng
            workSheet.Cells[p, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[p, 3].Style.WrapText = true;
            workSheet.Cells[p, 3].Value = string.Empty; //Value Người chứng từ

            workSheet.Cells[p, 4, p, 5].Merge = true;
            workSheet.Cells[p, 4, p, 5].Style.WrapText = true;
            workSheet.Cells[p, 4, p, 5].Value = settlementExport.InfoSettlement.Manager; //Value Trưởng bộ phận
            workSheet.Cells[p, 4, p, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[p, 8, p, 10].Merge = true;
            workSheet.Cells[p, 8, p, 10].Style.WrapText = true;
            workSheet.Cells[p, 8, p, 10].Value = settlementExport.InfoSettlement.Accountant; //Value Kế toán
            workSheet.Cells[p, 8, p, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[p, 11, p, 13].Merge = true;
            workSheet.Cells[p, 11, p, 13].Value = string.Empty; //Value Giám đốc
        }
        private void BindingDataDetailSettlementPaymentSOAExcel(ExcelWorksheet workSheet, SettlementExport settlementExport, string language)
        {
            workSheet.Cells.Style.Font.SetFromFont(new Font("Times New Roman", 11));
            workSheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Top;

            SetWidthColumnExcelDetailSettlementPaymentSOA(workSheet);

            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column A
                excelImage.SetPosition(0, 0, 0, 0);
            }

            List<string> headers = GetHeaderExcelDetailSettlementPaymentSOA(language);

            #region #Header
            workSheet.Cells["F1:L1"].Merge = true;
            workSheet.Cells["F1"].Value = headers[0];
            workSheet.Cells["F1"].Style.Font.SetFromFont(new Font("Arial Black", 11));
            workSheet.Cells["F1"].Style.Font.Italic = true;
            workSheet.Cells["F2:M2"].Merge = true;
            workSheet.Cells["F2"].Style.WrapText = true;
            workSheet.Cells["F2"].Value = settlementExport.InfoSettlement.ContactOffice;
            workSheet.Cells["F2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 9));
            workSheet.Cells["F2"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Row(2).Height = 60;

            //Title
            workSheet.Cells["A3:H3"].Merge = true;
            workSheet.Cells["A3"].Style.Font.SetFromFont(new Font("Times New Roman", 18));
            workSheet.Cells["A3"].Value = headers[2]; //Đề nghị thanh toán
            workSheet.Cells["A3"].Style.Font.Bold = true;
            workSheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A5:J6"].Style.Font.SetFromFont(new Font("Times New Roman", 12));

            workSheet.Cells["A5:B5"].Merge = true;
            workSheet.Cells["A5"].Value = headers[3]; //Người yêu cầu
            workSheet.Cells["A5"].Style.Font.Bold = true;
            workSheet.Cells["C5"].Value = settlementExport.InfoSettlement.Requester;

            workSheet.Cells["F5"].Value = headers[4]; //Ngày SOA
            workSheet.Cells["F5"].Style.Font.Bold = true;
            workSheet.Cells["G5"].Value = settlementExport.InfoSettlement.SOADate;
            workSheet.Cells["G5"].Style.Numberformat.Format = "dd MMM, yyyy";
            workSheet.Cells["G5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            workSheet.Cells["A6:B6"].Merge = true;
            workSheet.Cells["A6"].Value = headers[6]; //Bộ phận
            workSheet.Cells["A6"].Style.Font.Bold = true;
            workSheet.Cells["C6"].Value = settlementExport.InfoSettlement.Department;

            workSheet.Cells["A7:B7"].Merge = true;
            workSheet.Cells["A7"].Value = headers[22]; //Supplier
            workSheet.Cells["A7"].Style.Font.Bold = true;
            workSheet.Cells["C7"].Value = settlementExport.InfoSettlement.Supplier;

            workSheet.Cells["A8"].Value = headers[7]; //Reson for request
            workSheet.Cells["A8"].Style.Font.Bold = true;
            workSheet.Cells["B8"].Value = settlementExport.InfoSettlement.Note;


            workSheet.Cells["A11"].Value = headers[30]; //Reson for request
            workSheet.Cells["A11"].Style.Font.Bold = true;
            workSheet.Cells["B11"].Value = settlementExport.InfoSettlement.PaymentMethod;

            workSheet.Cells["F6"].Value = headers[5]; //Số SOA
            workSheet.Cells["F6"].Style.Font.Bold = true;
            workSheet.Cells["G6"].Value = settlementExport.InfoSettlement.SOANo;

            // By Bank transfer
            workSheet.Cells["F12"].Value = headers[23];
            workSheet.Cells["F12"].Style.Font.Bold = true;
            workSheet.Cells["G12"].Value = settlementExport.InfoSettlement.PaymentMethod.ToUpper().Contains("BANK") ? "X" : string.Empty;

            // Beneficiary
            workSheet.Cells["F13"].Value = headers[24];
            workSheet.Cells["F13"].Style.Font.Bold = true;
            workSheet.Cells["G13"].Value = settlementExport.InfoSettlement.BankAccountName;

            // Acc No
            workSheet.Cells["F14"].Value = headers[25];
            workSheet.Cells["F14"].Style.Font.Bold = true;
            workSheet.Cells["G14"].Value = settlementExport.InfoSettlement.BankAccountNo;

            // Bank
            workSheet.Cells["F15"].Value = headers[26];
            workSheet.Cells["F15"].Style.Font.Bold = true;
            workSheet.Cells["G15"].Value = settlementExport.InfoSettlement.BankName;

            // Bank Code
            workSheet.Cells["F16"].Value = headers[27];
            workSheet.Cells["F16"].Style.Font.Bold = true;
            workSheet.Cells["G16"].Value = settlementExport.InfoSettlement.BankCode;

            // By Cash
            workSheet.Cells["B12"].Value = headers[28];
            workSheet.Cells["B12"].Style.Font.Bold = true;
            workSheet.Cells["C12"].Value = settlementExport.InfoSettlement.PaymentMethod.ToUpper().Contains("CASH") ? "X" : string.Empty;

            // Due Date
            workSheet.Cells["A10"].Value = headers[29];
            workSheet.Cells["A10"].Style.Font.Bold = true;
            workSheet.Cells["C10"].Value = settlementExport.InfoSettlement.DueDate?.ToString("dd/MM/yyyy");
            #endregion
            //Bôi đen header
            //workSheet.Cells["A15:K15"].Style.Font.Bold = true;

            #region #Format Header Table
            for (var col = 1; col < 8; col++)
            {
                workSheet.Cells[18, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[18, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[18, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[19, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[18, col].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[18, col].Style.WrapText = true;
                workSheet.Cells[18, col].Style.Font.Bold = true;

                workSheet.Cells[19, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[19, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            workSheet.Cells["A18:A19"].Merge = true;
            workSheet.Cells["A18:A19"].Value = headers[8]; // Job ID

            workSheet.Cells["B18:B19"].Merge = true;
            workSheet.Cells["B18:B19"].Value = headers[9]; // Invoice No

            workSheet.Cells["C18:C19"].Merge = true;
            workSheet.Cells["C18:C19"].Value = headers[10]; // Custom No

            workSheet.Cells["D18:D19"].Merge = true;
            workSheet.Cells["D18:D19"].Value = headers[11]; // HBL No

            workSheet.Cells["E18:E19"].Merge = true;
            workSheet.Cells["E18:E19"].Value = headers[12]; // MBL No

            workSheet.Cells["F18:G18"].Merge = true;
            workSheet.Cells["F18:G18"].Value = headers[13]; // Amount
            workSheet.Cells["F18:G18"].Style.Numberformat.Format = numberFormat2;

            workSheet.Cells["F19"].Merge = true;
            workSheet.Cells["F19"].Value = headers[14]; // OBH

            workSheet.Cells["G19"].Merge = true;
            workSheet.Cells["G19"].Value = headers[15]; // Credit

            workSheet.Row(18).Height = 30;
            workSheet.Row(19).Height = 30;
            workSheet.Cells[19, 1, 19, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[17, 1, 17, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            #endregion

            decimal _sumTotalOBH = 0;
            decimal _sumTotalCredit = 0;
            int p = 20;
            int j = 20;
            int k = 20;
            for (int i = 0; i < settlementExport.ShipmentsSettlement.Count; i++)
            {

                var OBHCharges = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Where(w => w.SurType == "OBH");

                workSheet.Cells[k, 6].Value = headers[14];
                workSheet.Cells[k, 6].Style.Font.Bold = true;
                workSheet.Cells[k, 6].Value = OBHCharges.Select(s => s.ChargeAmount).Sum();
                _sumTotalOBH += OBHCharges.Select(s => s.ChargeAmount??0).Sum();
                workSheet.Cells[k, 6].Style.Font.Bold = true;
                workSheet.Cells[k, 6].Style.Numberformat.Format = numberFormat2;
                foreach (var invoice in OBHCharges)
                {
                    if (workSheet.Cells[k, 2].Value == null)
                    {
                        workSheet.Cells[k, 2].Value += invoice.InvoiceNo;
                    }
                    else {
                        workSheet.Cells[k, 2].Value +=";" + invoice.InvoiceNo;
                    }
                }

                var CreditInvoiceCharges = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Where(w => w.SurType == "BUY");

                workSheet.Cells[k, 7].Value = headers[15];
                workSheet.Cells[k, 7].Style.Font.Bold = true;
                workSheet.Cells[k, 7].Value = CreditInvoiceCharges.Select(s => s.ChargeAmount).Sum();
                _sumTotalCredit += CreditInvoiceCharges.Select(s => s.ChargeAmount??0).Sum();
                workSheet.Cells[k, 7].Style.Font.Bold = true;
                workSheet.Cells[k, 7].Style.Numberformat.Format = numberFormat2;
                foreach (var invoice in CreditInvoiceCharges)
                {
                    if (workSheet.Cells[k, 2].Value == null)
                    {
                        workSheet.Cells[k, 2].Value += invoice.InvoiceNo;
                    }
                    else
                    {
                        workSheet.Cells[k, 2].Value += ";" + invoice.InvoiceNo;
                    }
                }
                #endregion

                workSheet.Cells[j, 1].Value = headers[8]; //Số lô hàng
                workSheet.Cells[j, 1].Value = settlementExport.ShipmentsSettlement[i].JobNo;

                workSheet.Cells[j, 3].Value = headers[10]; //Số tờ khai
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].CustomNo;

                workSheet.Cells[j, 4].Value = headers[11]; //Số HBL
                workSheet.Cells[j, 4].Value = settlementExport.ShipmentsSettlement[i].HBL;

                workSheet.Cells[j, 5].Value = headers[12]; //Số MBL
                workSheet.Cells[j, 5].Value = settlementExport.ShipmentsSettlement[i].MBL;


                j = j + 1;
                k = k + 1;
                if (k > j)
                {
                    j = k;
                }
                else
                {
                    k = j;
                }
                workSheet.Cells[j - 1, 3, j - 1, 5].Style.Border.Top.Style = ExcelBorderStyle.Dotted;
                workSheet.Cells[j - 1, 1, j - 1, 2].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[j - 1, 6, j - 1, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[j - 1, 1, j - 1, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                //workSheet.Cells[p, 1, p, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                p = j;
                for (var f = p; f < j - 2; f++)
                {
                    workSheet.Cells[f, 4, f, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Dotted;
                    workSheet.Cells[f, 1, f, 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    workSheet.Cells[f, 6, f, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }
                //workSheet.Cells[p, 1, j - 1, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            }
            #region --TOTAL--
            ////TỔNG CỘNG
            workSheet.Cells[p, 1, p, 5].Merge = true;
            workSheet.Cells[p, 1, p, 5].Value = headers[16]; //Title TỔNG CỘNG
            workSheet.Cells[p, 1, p, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 1, p, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[p, 1, p, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[p, 1, p, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[p, 1, p, 7].Style.Border.BorderAround(ExcelBorderStyle.Medium);

            workSheet.Cells[p, 6].Value = "-"; //Value sum total OBH
            //workSheet.Cells[p, 6].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[p, 1, p, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            workSheet.Cells[p, 7].Value = _sumTotalCredit+_sumTotalOBH; //Value sum total Credit
            //workSheet.Cells[p, 7].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            p=p+2;
            workSheet.Cells[p, 1, p, 6].Merge = true;
            workSheet.Cells[p, 1, p, 6].Value = headers[17]; //Title BALANCE
            workSheet.Cells[p, 1, p, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 1, p, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[p, 1, p, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[p, 1, p, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);

            workSheet.Cells[p, 7].Value =Math.Abs(_sumTotalOBH - _sumTotalCredit); //Value sum total Balance
            //workSheet.Cells[p, 7].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            #endregion--TOTAL--
            //bôi đen dòng tổng cộng ở cuối
            workSheet.Cells["a" + (p-2) + ":g" + (p - 2)].Style.Font.Bold = true;
            workSheet.Cells["a" + (p - 2) + ":g" + (p - 2)].Style.Numberformat.Format = numberFormat2;
            workSheet.Cells["a" + p + ":g" + p].Style.Font.Bold = true;
            workSheet.Cells["a" + p + ":g" + p].Style.Numberformat.Format = numberFormat2;

            //In đậm border dòng 14
            workSheet.Cells[18, 1, 19, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + p + ":G" + p].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + (p + 1) + ":G" + (p + 1)].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            #region --END_FORM--
            for (var i = 18; i < p + 1; i++)
            {
                workSheet.Cells[i, 7].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            }

            p = p + 3;

            workSheet.Cells[p, 1, p, 2].Merge = true;
            workSheet.Cells[p, 1, p, 2].Style.WrapText = true;
            workSheet.Cells[p, 1, p, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 1, p, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[p, 1, p, 2].Value = headers[18]; //Người yêu cầu

            workSheet.Cells[p, 4].Merge = true;
            workSheet.Cells[p, 4].Style.WrapText = true;
            workSheet.Cells[p, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[p, 4].Value = headers[19]; //Trưởng bộ phận

            workSheet.Cells[p, 6].Merge = true;
            workSheet.Cells[p, 6].Style.WrapText = true;
            workSheet.Cells[p, 6].Value = headers[20]; //Kế toán
            workSheet.Cells[p, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[p, 7].Merge = true;
            workSheet.Cells[p, 7].Value = headers[21]; //Giám đốc
            workSheet.Cells[p, 7].Style.WrapText = true;
            workSheet.Cells[p, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Row(p).Height = 50;

            p = p + 1;

            if (settlementExport.InfoSettlement.IsRequesterApproved)
            {
                AddIconTick(workSheet, p, 1); //Tick Requester
            }

            if (settlementExport.InfoSettlement.IsManagerApproved)
            {
                workSheet.Cells[p, 4].Merge = true;
                AddIconTick(workSheet, p, 4); //Tick Manager Dept
            }

            if (settlementExport.InfoSettlement.IsAccountantApproved)
            {
                workSheet.Cells[p, 6].Merge = true;
                AddIconTick(workSheet, p, 6); //Tick Accountant
            }

            if (settlementExport.InfoSettlement.IsBODApproved)
            {
                workSheet.Cells[p, 7].Merge = true;
                AddIconTick(workSheet, p, 7); //Tick BOD
            }

            workSheet.Row(p).Height = 50;

            p = p + 1;

            workSheet.Cells[p, 1].Style.WrapText = true;
            workSheet.Cells[p, 1].Value = settlementExport.InfoSettlement.Requester; //Value Người tạm ứng
            workSheet.Cells[p, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[p, 4].Merge = true;
            workSheet.Cells[p, 4].Style.WrapText = true;
            workSheet.Cells[p, 4].Value = settlementExport.InfoSettlement.Manager; //Value Trưởng bộ phận
            workSheet.Cells[p, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[p, 6].Merge = true;
            workSheet.Cells[p, 6].Style.WrapText = true;
            workSheet.Cells[p, 6].Value = settlementExport.InfoSettlement.Accountant; //Value Kế toán
            workSheet.Cells[p, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[p, 7].Merge = true;
            workSheet.Cells[p, 7].Value = string.Empty; //Value Giám đốc
            #endregion--END_FORM--
        }

        private void AddIconTick(ExcelWorksheet workSheet, int row, int col)
        {
            workSheet.Cells[row, col].Value = char.ConvertFromUtf32(0x0050); //Mã code của Symbol tick
            workSheet.Cells[row, col].Style.Font.Name = "Wingdings 2";
            workSheet.Cells[row, col].Style.Font.Size = 28;
            workSheet.Cells[row, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[row, col].Style.VerticalAlignment = ExcelVerticalAlignment.Bottom;
        }

        /// <summary>
        /// Generate data to Export General Preview in Settlement detail
        /// </summary>
        /// <param name="settlementExport"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Stream GenerateExportGeneralSettlementPayment(InfoSettlementExport settlementExport, string fileName)
        {
            try
            {
                var folderOfFile = GetSettleExcelFolder();
                FileInfo f = new FileInfo(Path.Combine(folderOfFile, fileName));
                var path = f.FullName;
                if (!File.Exists(path))
                {
                    return null;
                }
                var excel = new ExcelExport(path);
                var listKeyData = new Dictionary<string, object>();
                listKeyData.Add("SettlementNo", settlementExport.SettlementNo);
                listKeyData.Add("RequestDate", settlementExport.RequestDate);
                listKeyData.Add("Requester", settlementExport.Requester);
                listKeyData.Add("Department", settlementExport.Department);
                listKeyData.Add("PayeeName", settlementExport.PayeeName);
                listKeyData.Add("PaymentMethod", settlementExport.PaymentMethod);
                listKeyData.Add("BankName", "Bank (Tên Ngân hàng): " + settlementExport.BankName);
                listKeyData.Add("BankAccountNo", "Acc No (số TK): " + settlementExport.BankAccountNo);
                listKeyData.Add("BeneficiaryName", "Beneficiary (Tên người thụ hưởng): " + settlementExport.BankAccountName);
                excel.SetData(listKeyData);
                //Set format amount
                var formatAmountVND = "_([$VND] * #,##0_);_([$VND] * (#,##0);_([$VND] * \"\"??_);_(@_)";
                var formatAmountUSD = "_([$USD] * #,##0.00_);_([$USD] * (#,##0.00);_([$USD] * \"\"??_);_(@_)";
                var listKeyFormat = new List<string>();
                if (settlementExport.SettlementCurrency == "VND")
                {
                    listKeyFormat.Add("SettlementAmount");
                    listKeyFormat.Add("SettlementAmountSum");
                    listKeyFormat.Add("SettlementAmountVND");
                    listKeyFormat.Add("SettlementAmountTotalVND");
                    excel.SetFormatCell(listKeyFormat, formatAmountVND);
                }
                else
                {
                    listKeyFormat.Add("SettlementAmount");
                    listKeyFormat.Add("SettlementAmountSum");
                    listKeyFormat.Add("SettlementAmountUSD");
                    listKeyFormat.Add("SettlementAmountTotalUSD");
                    excel.SetFormatCell(listKeyFormat, formatAmountUSD);
                }
                // footer
                listKeyData = new Dictionary<string, object>();
                listKeyData.Add("SettlementNote", string.IsNullOrEmpty(settlementExport.Note?.Trim()) ? "Thanh toán cước vận chuyển" : settlementExport.Note);
                listKeyData.Add("SettlementAmount", settlementExport.SettlementAmount);
                listKeyData.Add("SettlementAmountVND", settlementExport.SettlementCurrency == "VND" ? settlementExport.SettlementAmount : null);
                listKeyData.Add("SettlementAmountSumVND", settlementExport.SettlementCurrency == "VND" ? settlementExport.SettlementAmount : null);
                listKeyData.Add("SettlementAmountTotalVND", settlementExport.SettlementCurrency == "VND" ? settlementExport.SettlementAmount : null);
                listKeyData.Add("SettlementAmountSum", settlementExport.SettlementAmount);
                listKeyData.Add("SettlementAmountUSD", settlementExport.SettlementCurrency != "VND" ? settlementExport.SettlementAmount : null);
                listKeyData.Add("SettlementAmountSumUSD", settlementExport.SettlementCurrency != "VND" ? settlementExport.SettlementAmount : null);
                listKeyData.Add("SettlementAmountTotalUSD", settlementExport.SettlementCurrency != "VND" ? settlementExport.SettlementAmount : null);

                listKeyData.Add("AmountInWord", "In word (Thành tiền): " + settlementExport.AmountInWords);
                // Họ tên người ký
                listKeyData.Add("Requester", settlementExport.Requester); // Requester
                listKeyData.Add("Manager", settlementExport.Manager); // trưởng bộ phận
                listKeyData.Add("Accountant", settlementExport.Accountant); // kế toàn trưởng
                excel.SetData(listKeyData);

                // Tick Requester
                excel.Worksheet.Cells["B46"].Value = settlementExport.IsRequesterApproved ? char.ConvertFromUtf32(0x0050) : string.Empty; //Mã code của Symbol tick;
                // Tick trưởng bộ phận
                excel.Worksheet.Cells["D46"].Value = settlementExport.IsManagerApproved ? char.ConvertFromUtf32(0x0050) : string.Empty;
                // Tick thủ trưởng
                excel.Worksheet.Cells["F46"].Value = settlementExport.IsBODApproved ? char.ConvertFromUtf32(0x0050) : string.Empty;
                // Tick kế toán trưởng
                excel.Worksheet.Cells["H46"].Value = settlementExport.IsAccountantApproved ? char.ConvertFromUtf32(0x0050) : string.Empty;
                return excel.ExcelStream();
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        #region --- ACCOUNTING MANAGEMENT ---
        public Stream GenerateAccountingManagementExcel(List<AccountingManagementExport> acctMngts, string typeOfAcctMngt, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Sheet1");
                    var workSheet = excelPackage.Workbook.Worksheets.First();
                    BindingDataAccoutingManagementExcel(workSheet, acctMngts, typeOfAcctMngt);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void SetWidthColumnExcelAccoutingManagement(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 14; //Cột A
            workSheet.Column(2).Width = 14; //Cột B
            workSheet.Column(3).Width = 21; //Cột C
            workSheet.Column(4).Width = 30; //Cột D
            workSheet.Column(5).Width = 14; //Cột E
            workSheet.Column(6).Width = 14; //Cột F
            workSheet.Column(7).Width = 14; //Cột G
            workSheet.Column(8).Width = 18; //Cột H
            workSheet.Column(9).Width = 19; //Cột I
            workSheet.Column(10).Width = 15; //Cột J
            workSheet.Column(11).Width = 12; //Cột K
            workSheet.Column(12).Width = 20; //Cột L
            workSheet.Column(13).Width = 10; //Cột M
            workSheet.Column(14).Width = 16; //Cột N
            workSheet.Column(15).Width = 15; //Cột O
            workSheet.Column(16).Width = 15; //Cột P
            workSheet.Column(17).Width = 20; //Cột Q
            workSheet.Column(18).Width = 20; //Cột R
            workSheet.Column(19).Width = 20; //Cột S
            workSheet.Column(20).Width = 20; //Cột T
            workSheet.Column(21).Width = 15; //Cột U
            workSheet.Column(22).Width = 30; //Cột V
            workSheet.Column(23).Width = 16; //Cột W
            workSheet.Column(24).Width = 14; //Cột X
            workSheet.Column(25).Width = 18; //Cột Y
            workSheet.Column(26).Width = 10; //Cột Z
            workSheet.Column(27).Width = 12; //Cột AA
            workSheet.Column(28).Width = 8; //Cột AB
            workSheet.Column(29).Width = 10; //Cột AC
            workSheet.Column(30).Width = 14; //Cột AD
            workSheet.Column(31).Width = 10; //Cột AE
            workSheet.Column(32).Width = 15; //Cột AF
            workSheet.Column(33).Width = 12; //Cột AG
            workSheet.Column(34).Width = 10; //Cột AH
            workSheet.Column(35).Width = 10; //Cột AI
            workSheet.Column(36).Width = 30; //Cột AJ
            workSheet.Column(37).Width = 15; //Cột AK
            workSheet.Column(38).Width = 18; //Cột AL
            workSheet.Column(39).Width = 20; //Cột AM
            workSheet.Column(40).Width = 25; //Cột AN
        }

        private void BindingDataAccoutingManagementExcel(ExcelWorksheet workSheet, List<AccountingManagementExport> acctMngts, string typeOfAcctMngt)
        {
            SetWidthColumnExcelAccoutingManagement(workSheet);
            List<string> headers = new List<string>
            {
                "Ngày chứng từ", //0
                "Số chứng từ", //1
                "Mã chứng từ", //2
                "Diễn giải", //3
                "Mã khách hàng", //4
                "TK Nợ", //5
                "TK Có", //6
                "Mã loại", //7
                "Mã tiền tệ", //8
                "Số tiền", //9
                "Tỷ giá", //10
                "Tiền VND", //11
                "% VAT", //12
                "TK Nợ VAT", //13
                "TK Có VAT", //14
                "Tiền VAT", //15
                "Tiền VND VAT", //16
                "Số hóa đơn VAT", //17
                "Ngày hóa đơn VAT", //18
                "Số seri VAT", //19
                "Mặt hàng VAT", //20
                "Tên đối tượng VAT", //21
                "Mã số thuế ĐT VAT", //22
                "Mã Job", //23
                "Diễn giải", //24
                "Đánh dấu", //25
                "Thời hạn T/T", //26
                "Mã BP", //27
                "Số TK", //28
                "Số H-B/L", //29
                "ĐVT", //30
                "Hình thức T/T", //31
                "Mã ĐT CH", //32
                "Số Lượng", //33
                "Mã HĐ", //34
                "Địa chỉ đối tượng VAT", //35
                "Số M-B/L", //36
                "Tình trạng hóa đơn", //37
                "Email", //38
                "Ngày phát hành E-Invoice" //39
            };
            int rowStart = 1;
            for(int i = 0; i < headers.Count; i++)
            {
                workSheet.Cells[rowStart, i + 1].Value = headers[i];
                workSheet.Cells[rowStart, i + 1].Style.Font.Bold = true;
                workSheet.Cells[rowStart, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            //Cố định dòng đầu tiên (Freeze Row 1 and no column)
            workSheet.View.FreezePanes(2, 1);

            rowStart += 1;
            foreach(var item in acctMngts)
            {
                workSheet.Cells[rowStart, 1].Value = item.Date; //Ngày chứng từ
                workSheet.Cells[rowStart, 1].Style.Numberformat.Format = "dd/MM/yyyy";

                workSheet.Cells[rowStart, 2].Value = item.VoucherId; //Số chứng từ
                workSheet.Cells[rowStart, 3].Value = (typeOfAcctMngt == "Invoice") ? "HD" : item.VoucherId?.Substring(0, 2); //2 ký tự đầu của số chứng từ
                workSheet.Cells[rowStart, 4].Value = item.ChargeName;
                // workSheet.Cells[rowStart, 5].Value = item.VatPartnerCode; //Mã số thuế của partner của charge
                workSheet.Cells[rowStart, 5].Value = item.PartnerId; // Partner ID của đối tượng voucher
                workSheet.Cells[rowStart, 6].Value = item.AccountNo;
                workSheet.Cells[rowStart, 7].Value = item.ContraAccount;
                workSheet.Cells[rowStart, 8].Value = item.ChargeCode;
                workSheet.Cells[rowStart, 9].Value = item.Currency; //Currency của phí

                workSheet.Cells[rowStart, 10].Value = item.OrgAmount;
                workSheet.Cells[rowStart, 11].Value = item.FinalExchangeRate;
                workSheet.Cells[rowStart, 12].Value = item.AmountVnd;
                workSheet.Cells[rowStart, 13].Value = item.Vat;
                for(int i = 10; i < 14; i++)
                {
                    workSheet.Cells[rowStart, i].Style.Numberformat.Format = decimalFormat;
                }

                workSheet.Cells[rowStart, 14].Value = item.TkNoVat;
                workSheet.Cells[rowStart, 15].Value = item.TkCoVat;

                workSheet.Cells[rowStart, 16].Value = item.OrgVatAmount;
                workSheet.Cells[rowStart, 17].Value = item.VatAmountVnd;
                for (int i = 16; i < 18; i++)
                {
                    workSheet.Cells[rowStart, i].Style.Numberformat.Format = decimalFormat;
                }

                workSheet.Cells[rowStart, 18].Value = item.InvoiceNo; //Invoice No của charge

                workSheet.Cells[rowStart, 19].Value = item.InvoiceDate; //Ngày hóa đơn của charge
                workSheet.Cells[rowStart, 19].Style.Numberformat.Format = "dd/MM/yyyy";

                workSheet.Cells[rowStart, 20].Value = item.Serie; //Số serie của charge
                workSheet.Cells[rowStart, 21].Value = string.Empty; //Mặt hàng VAT (Để trống)
                workSheet.Cells[rowStart, 22].Value = item.VatPartnerNameVn; //Partner Name Local của charge
                workSheet.Cells[rowStart, 23].Value = item.VatPartnerCode; //Mã số thuế của partner của charge
                workSheet.Cells[rowStart, 24].Value = item.JobNo;
                workSheet.Cells[rowStart, 25].Value = item.Description;
                workSheet.Cells[rowStart, 26].Value = item.IsTick ? "TRUE" : "FALSE";
                workSheet.Cells[rowStart, 27].Value = item.PaymentTerm; //Thời hạn thanh toán
                workSheet.Cells[rowStart, 28].Value = item.DepartmentCode;
                workSheet.Cells[rowStart, 29].Value = item.CustomNo;
                workSheet.Cells[rowStart, 30].Value = item.Hbl;
                workSheet.Cells[rowStart, 31].Value = item.UnitName;
                workSheet.Cells[rowStart, 32].Value = item.PaymentMethod;
                workSheet.Cells[rowStart, 33].Value = item.ObhPartnerCode; //Taxcode của đối tượng OBH
                workSheet.Cells[rowStart, 34].Value = item.Qty;
                workSheet.Cells[rowStart, 35].Value = string.Empty; //Để trống
                workSheet.Cells[rowStart, 36].Value = item.VatPartnerAddress;
                workSheet.Cells[rowStart, 37].Value = item.Mbl;
                workSheet.Cells[rowStart, 38].Value = item.StatusInvoice; //Tình trạng hóa đơn
                workSheet.Cells[rowStart, 39].Value = item.VatPartnerEmail;
                workSheet.Cells[rowStart, 40].Value = item.ReleaseDateEInvoice;
                rowStart += 1;
            }

            workSheet.Cells["A1:AN" + (rowStart - 1)].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["A1:AN" + (rowStart - 1)].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["A1:AN" + (rowStart - 1)].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["A1:AN" + (rowStart - 1)].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }
        private void BindingDataAccoutingManagementDebCreInvExcel(ExcelWorksheet workSheet, List<AccountingManagementExport> acctMngts, string typeOfAcctMngt)
        {
            SetWidthColumnExcelAccoutingManagement(workSheet);
            workSheet.Column(6).Width = 20; //Cột F
            workSheet.Column(13).Width = 20; //Cột M
            List<string> headers = new List<string>
            {
                "JobNo",//0
                "InvoiceNo",//1
                "MBLNo",//2
                "HBLNo",//3
                "VoucherID",//4
                "Accounting Date",//5
                "CDNote_Code",//6
                "Code_Type",//7
                "ChargeType",//8
                "PayerID",//9
                "Payer_Name",//10
                "PartnerType",//11
                "Curr",//12
                "Amount",//13
                "Issued_by",//14
                "BU",//15
                "Service Date",//16
                "Issue Date",//17
                "Account No"//18
            };
            int rowStart = 1;
            for (int i = 0; i < headers.Count; i++)
            {
                workSheet.Cells[rowStart, i + 1].Value = headers[i];
                workSheet.Cells[rowStart, i + 1].Style.Font.Bold = true;
                workSheet.Cells[rowStart, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            //Cố định dòng đầu tiên (Freeze Row 1 and no column)
            workSheet.View.FreezePanes(2, 1);

            rowStart += 1;
            foreach (var item in acctMngts)
            {
                workSheet.Cells[rowStart, 1].Value = item.JobNo;
                workSheet.Cells[rowStart, 2].Value = item.InvoiceNo;
                workSheet.Cells[rowStart, 3].Value = item.Mbl;
                workSheet.Cells[rowStart, 4].Value = item.Hbl;
                workSheet.Cells[rowStart, 5].Value = item.VoucherId;
                workSheet.Cells[rowStart, 6].Value = item.VoucherIddate;
                workSheet.Cells[rowStart, 6].Style.Numberformat.Format = "dd/MM/yyyy";
                workSheet.Cells[rowStart, 7].Value = item.CdNoteNo;
                workSheet.Cells[rowStart, 8].Value = item.CdNoteType;
                workSheet.Cells[rowStart, 9].Value = item.ChargeType;
                workSheet.Cells[rowStart, 10].Value = item.PayerId;
                workSheet.Cells[rowStart, 11].Value = item.PayerName;
                workSheet.Cells[rowStart, 12].Value = item.PayerType;
                workSheet.Cells[rowStart, 13].Value = item.Currency;

                workSheet.Cells[rowStart, 14].Value = item.Amount;
                workSheet.Cells[rowStart, 14].Style.Numberformat.Format = decimalFormat;

                workSheet.Cells[rowStart, 15].Value = item.IssueBy;
                workSheet.Cells[rowStart, 16].Value = item.Bu;
                workSheet.Cells[rowStart, 17].Value = item.ServiceDate;
                workSheet.Cells[rowStart, 17].Style.Numberformat.Format = "dd/MM/yyyy";
                workSheet.Cells[rowStart, 18].Value = item.IssueDate;
                workSheet.Cells[rowStart, 18].Style.Numberformat.Format = "dd/MM/yyyy";

                workSheet.Cells[rowStart, 19].Value = item.AccountNo;
                rowStart += 1;
            }

            workSheet.Cells["A1:S" + (rowStart - 1)].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["A1:S" + (rowStart - 1)].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["A1:S" + (rowStart - 1)].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["A1:S" + (rowStart - 1)].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        //public Stream GenerateAccountingReceivableExcel(List<AccountReceivableResultExport> acctMngts, ARTypeEnum arType, Stream stream = null)
        //{
        //    try
        //    {
        //        using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
        //        {
        //            excelPackage.Workbook.Worksheets.Add("Sheet1");
        //            var workSheet = excelPackage.Workbook.Worksheets.First();
        //            if (arType == ARTypeEnum.TrialOrOffical)
        //            {
        //                BindingDataAccoutingReceivableListTrialExcel(workSheet, acctMngts);
        //            }else if (arType == ARTypeEnum.Other)
        //            {
        //                BindingDataAccoutingReceivableListOrtherExcel(workSheet, acctMngts);
        //            }
        //            excelPackage.Save();
        //            return excelPackage.Stream;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //    return null;
        //}

        public Stream GenerateAccountingReceivableArSumary(List<AccountReceivableResultExport> result, string fileName)
        {
            try
            {
                var folderOfFile = GetARExcelFolder();
                FileInfo f = new FileInfo(Path.Combine(folderOfFile, fileName));
                var path = f.FullName;
                if (!File.Exists(path))
                    return null;

                var excel = new ExcelExport(path);
                excel.StartDetailTable = 3;

                excel.StartDetailTable = 3;
                //Set format amount
                var formatAmountVND = "_(* #,##0_);_(* (#,##0);_(* \"-\"??_);_(@_)";
                var formatAmountUSD = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";

                var rowStart = 3;
                for (int i = 0; i < result.Count; i++)
                {
                    var item = result[i];
                    var listKeyData = new Dictionary<string, object>();
                    var listKeyFormat = new List<string>();
                    excel.SetDataTable();
                    listKeyData.Add("No", i+1);
                    listKeyData.Add("PartnerId", item.PartnerCode);
                    listKeyData.Add("PartnerName", item.PartnerNameEn);
                    var rate = item.DebitRate?.ToString("0.##");
                    listKeyData.Add("Rate", rate + " %");

                    if (item.AgreementCurrency == "VND")
                    {
                        listKeyData.Add("Billing", item.BillingAmount + item.ObhBillingAmount);
                        excel.Worksheet.Cells[rowStart, 5].Style.Numberformat.Format = formatAmountVND;
                        listKeyData.Add("PaidAPart", item.PaidAmount + item.ObhPaidAmount);
                        excel.Worksheet.Cells[rowStart, 6].Style.Numberformat.Format = formatAmountVND;
                        listKeyData.Add("OutStanding", item.BillingUnpaid + item.ObhUnPaidAmount);
                        excel.Worksheet.Cells[rowStart, 7].Style.Numberformat.Format = formatAmountVND;
                        listKeyData.Add("Over1-15Days", item.Over1To15Day);
                        excel.Worksheet.Cells[rowStart, 8].Style.Numberformat.Format = formatAmountVND;
                        listKeyData.Add("Over16-30Days", item.Over16To30Day);
                        excel.Worksheet.Cells[rowStart, 9].Style.Numberformat.Format = formatAmountVND;
                        listKeyData.Add("Over30Days", item.Over30Day);
                        excel.Worksheet.Cells[rowStart, 10].Style.Numberformat.Format = formatAmountVND;
                        listKeyData.Add("DebitAmount", item.DebitAmount);
                        excel.Worksheet.Cells[rowStart, 12].Style.Numberformat.Format = formatAmountVND;
                        listKeyData.Add("CreditLimited", item.CreditLimited);
                        excel.Worksheet.Cells[rowStart, 13].Style.Numberformat.Format = formatAmountVND;
                        listKeyData.Add("OverCreditAmount", item.DebitAmount - item.CreditLimited);
                        excel.Worksheet.Cells[rowStart, 14].Style.Numberformat.Format = formatAmountVND;
                    }
                    else
                    {
                        listKeyData.Add("Billing", item.BillingAmount + item.ObhBillingAmount);
                        excel.Worksheet.Cells[rowStart, 5].Style.Numberformat.Format = formatAmountUSD;
                        listKeyData.Add("PaidAPart", item.PaidAmount + item.ObhPaidAmount);
                        excel.Worksheet.Cells[rowStart, 6].Style.Numberformat.Format = formatAmountUSD;
                        listKeyData.Add("OutStanding", item.BillingUnpaid + item.ObhUnPaidAmount);
                        excel.Worksheet.Cells[rowStart, 7].Style.Numberformat.Format = formatAmountUSD;
                        listKeyData.Add("Over1-15Days", item.Over1To15Day);
                        excel.Worksheet.Cells[rowStart, 8].Style.Numberformat.Format = formatAmountUSD;
                        listKeyData.Add("Over16-30Days", item.Over16To30Day);
                        excel.Worksheet.Cells[rowStart, 9].Style.Numberformat.Format = formatAmountUSD;
                        listKeyData.Add("Over30Days", item.Over30Day);
                        excel.Worksheet.Cells[rowStart, 10].Style.Numberformat.Format = formatAmountUSD;
                        listKeyData.Add("DebitAmount", item.DebitAmount);
                        excel.Worksheet.Cells[rowStart, 12].Style.Numberformat.Format = formatAmountUSD;
                        listKeyData.Add("CreditLimited", item.CreditLimited);
                        excel.Worksheet.Cells[rowStart, 13].Style.Numberformat.Format = formatAmountUSD;
                        listKeyData.Add("OverCreditAmount", item.DebitAmount - item.CreditLimited);
                        excel.Worksheet.Cells[rowStart, 14].Style.Numberformat.Format = formatAmountUSD;
                    }
              

                    listKeyData.Add("Curr", item.AgreementCurrency);
              

                    listKeyData.Add("Salesman",item.AgreementSalesmanName);
                    listKeyData.Add("ContractType", item.AgreementType);
                    listKeyData.Add("Status", item.AgreementStatus);
                    listKeyData.Add("ContractNo", item.AgreementNo);
                    listKeyData.Add("ExpDate", item.ExpriedDate);
                    listKeyData.Add("ExpDay", item.ExpriedDay);
                    listKeyData.Add("ParentPartner", item.ParentNameAbbr);

                    excel.SetData(listKeyData);
                    rowStart++;
                }
                return excel.ExcelStream();
            }
            catch (Exception ex)
            {
                return null;
            }



        }

        private void BindingDataAccoutingReceivableListOrtherExcel(ExcelWorksheet workSheet, List<AccountReceivableResultExport> acctMngts)
        {
            SetWidthColumnExcelAccoutingManagement(workSheet);

            workSheet.Column(2).Width = 38;
            for (int i = 4; i <= 7; i++)
                workSheet.Column(i).Width = 19;
            workSheet.Column(8).Width = 26;
            workSheet.Column(9).Width = 63;

            List<string> headers = new List<string>
            {
                 "No",//0
                 "Partner ID",//1
                 "Partner Name",//2
                 "Debit Amount",//3
                 "Billing",//4
                 "Paid",//5
                 "OutStanding Balance",//6
                 "Status",//7
                 "Parent Partner"
            };

            int rowStart = 1;
            for (int i = 0; i < headers.Count; i++)
            {
                workSheet.Cells[rowStart, i + 1].Value = headers[i];
                workSheet.Cells[rowStart, i + 1].Style.Font.Bold = true;
                workSheet.Cells[rowStart, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            //Cố định dòng đầu tiên (Freeze Row 1 and no column)
            workSheet.View.FreezePanes(2, 1);

            rowStart += 1;
            foreach (var item in acctMngts)
            {
                workSheet.Cells[rowStart, 1].Value = rowStart-1;
                workSheet.Cells[rowStart, 2].Value = item.PartnerCode;
                workSheet.Cells[rowStart, 3].Value = item.ParentNameAbbr;
                workSheet.Cells[rowStart, 4].Value = item.DebitAmount;
                workSheet.Cells[rowStart, 5].Value = item.BillingAmount;
                workSheet.Cells[rowStart, 6].Value = item.PaidAmount;
                workSheet.Cells[rowStart, 7].Value = item.BillingUnpaid;

                for (int i = 4; i <= 7; i++)
                    workSheet.Cells[rowStart, i].Style.Numberformat.Format = decimalFormat;

                workSheet.Cells[rowStart, 8].Value = item.AgreementStatus;
                workSheet.Cells[rowStart, 9].Value = item.ParentNameAbbr;

                rowStart += 1;
            }

            workSheet.Cells["A1:I" + (rowStart - 1)].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["A1:I" + (rowStart - 1)].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["A1:I" + (rowStart - 1)].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["A1:I" + (rowStart - 1)].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }
        #endregion --- ACCOUTING MANAGEMENT ---

        public Stream GenerateExportAgencyHistoryPayment(List<AccountingAgencyPaymentExport> result, string fileName, AccountingPaymentCriteria paymentCriteria)
        {
            try
            {
                var folderOfFile = GetARExcelFolder();
                var deleteDetailRow = false;
                FileInfo f = new FileInfo(Path.Combine(folderOfFile, fileName));
                var path = f.FullName;
                if (!File.Exists(path))
                {
                    return null;
                }
                var excel = new ExcelExport(path);
                int startRow = 6;
                excel.StartDetailTable = startRow;
                if (result.Count == 0)
                    result.Add(new AccountingAgencyPaymentExport());
                if (paymentCriteria.DueDate != null  || paymentCriteria.FromIssuedDate != null || result.FirstOrDefault().details == null || result.Count(x => x.details != null && x.details.Count() > 0) == 0)
                {
                    excel.DeleteRow(7);
                    deleteDetailRow = true;
                }

                for (int i = 0; i < result.Count; i++)
                {
                    var item = result[i];
                    var listKeyData = new Dictionary<string, object>();
                    excel.SetGroupsTable();
                    listKeyData.Add("AgentParentCode", item.AgentParentCode);
                    listKeyData.Add("AgentPartnerCode", item.AgentPartnerCode);
                    listKeyData.Add("AgentPartnerName", item.AgentPartnerName);
                    listKeyData.Add("InvoiceDate", item.InvoiceDate);
                    listKeyData.Add("InvoiceNo", item.InvoiceNo);
                    listKeyData.Add("DebitNo", item.DebitNo==null?"":item.DebitNo);
                    listKeyData.Add("CreditNo", item.CreditNo == null ? "" : item.CreditNo);
                    listKeyData.Add("JobNo", item.JobNo);
                    listKeyData.Add("MBLNo", item.MBL);
                    listKeyData.Add("HBLNo", item.HBL);

                    var remainDb = (item.UnpaidAmountInv ?? 0) - (item.PaidAmount ?? 0);
                    var remainObh = (item.UnpaidAmountOBH ?? 0) - (item.PaidAmountOBH ?? 0);
                    var remainDbUsd = (item.UnpaidAmountInvUsd ?? 0) - (item.PaidAmountUsd ?? 0);
                    var remainObhUsd = (item.UnpaidAmountOBHUsd ?? 0) - (item.PaidAmountOBHUsd ?? 0);

                    listKeyData.Add("DebitAmountUsd", item.DebitAmountUsd);
                    listKeyData.Add("CreditAmountUsd", item.CreditAmountUsd);

                    listKeyData.Add("Debit", item.DebitUsd);
                    listKeyData.Add("Credit", item.CreditUsd);

                    if (item.DebitAmountUsd != null)
                    {
                        listKeyData.Add("RemainDebitUsd", item.DebitAmountUsd - item.DebitUsd);
                        listKeyData.Add("RemainCreditUsd",0);

                        listKeyData.Add("RemainDebitVnd", item.DebitAmountVnd - item.DebitVnd);
                        listKeyData.Add("RemainCreditVnd", 0);
                    }
                    else if (item.CreditAmountUsd != null)
                    {
                        listKeyData.Add("RemainDebitUsd", 0);
                        listKeyData.Add("RemainCreditUsd", item.CreditAmountUsd - item.CreditUsd);

                        listKeyData.Add("RemainDebitVnd",0);
                        listKeyData.Add("RemainCreditVnd", item.CreditAmountVnd-item.CreditVnd);
                    }

                    listKeyData.Add("ETD", item.EtdDate);
                    listKeyData.Add("ETA", item.EtaDate);

                    listKeyData.Add("CreditTerm",item.CreditTerm);
                    listKeyData.Add("DueDate",item.DueDate);
                    listKeyData.Add("OverDueDays",item.OverDueDays);
                    listKeyData.Add("VoucherNo", item.VoucherNo);

                    listKeyData.Add("Salesman", item.Salesman);
                    listKeyData.Add("Creator", item.Creator);
                    excel.SetData(listKeyData);
                    startRow++;
                    if ( item.details.Count > 0 && deleteDetailRow == false && (paymentCriteria.DueDate == null || paymentCriteria.FromIssuedDate == null))
                    {
                        foreach (var detail in item.details)
                        {
                            listKeyData = new Dictionary<string, object>();
                            excel.SetDataTable();
                            listKeyData.Add("InvoiceDateDt", item.InvoiceDate);
                            listKeyData.Add("DebitNoDt", item.DebitNo);
                            listKeyData.Add("CreditNoDt", item.CreditNo);
                            listKeyData.Add("JobNoDt", item.JobNo);
                            listKeyData.Add("MBLNoDt", item.MBL);
                            listKeyData.Add("HBLNoDt", item.HBL);

                            listKeyData.Add("PaidDate", detail.PaidDate);
                            listKeyData.Add("RefNo", detail.RefNo);

                            if (item.DebitAmountUsd != null)
                            {
                                listKeyData.Add("DebitDt", detail.DebitUsd);
                                listKeyData.Add("CreditDt", 0);
                            }
                            else if (item.CreditAmountUsd != null)
                            {
                                listKeyData.Add("DebitDt", 0 );
                                listKeyData.Add("CreditDt", detail.CreditUsd);
                            }

                            excel.SetData(listKeyData);
                            startRow++;
                        }
                    }
                }
                return excel.ExcelStream();
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public Stream GenerateReceiptAdvance(AcctReceiptAdvanceModelExport result, AcctReceiptCriteria criteria, out string fileName)
        {
            try
            {
                var folderOfFile = GetARExcelFolder();
                FileInfo f = new FileInfo(Path.Combine(folderOfFile, "Receipt_Advance_Report _Teamplate.xlsx"));
                var path = f.FullName;
                fileName = "AdvanceReport_" + result.TaxCode+ ".xlsx";

                if (!File.Exists(path))
                {
                    return null;
                }

                var excel = new ExcelExport(path);

                var map = new Dictionary<string, object>();

                map.Add("taxCode", result.TaxCode);
                map.Add("nameEn", result.PartnerNameEn);
                map.Add("info",  string.Format("{0} at {1}", result.UserExport, DateTime.Now.ToString("dd/MM/yyyy")) );


                excel.SetData(map);

                int startRow = 6;
                excel.StartDetailTable = startRow;
                int _length = result.Details.Count;

                for (int i = 0; i < _length; i++)
                {
                    AcctREceiptAdvanceRow item = result.Details[i];
                    Dictionary<string, object> mappingKeyValue = new Dictionary<string, object>();
                    excel.SetGroupsTable();

                    mappingKeyValue.Add("paidDate",item.PaidDate.ToString("dd/MM/yyyy"));
                    mappingKeyValue.Add("receiptNo",item.ReceiptNo);
                    mappingKeyValue.Add("totalAdvPaymentVnd",item.TotalAdvancePaymentVnd);
                    mappingKeyValue.Add("totalAdvPaymentUsd",item.TotalAdvancePaymentUsd);
                    mappingKeyValue.Add("cusAdvAmountVnd",item.CusAdvanceAmountVnd);
                    mappingKeyValue.Add("cusAdvAmountUsd",item.CusAdvanceAmountUsd);
                    mappingKeyValue.Add("agreementAdvAmountVnd", item.AgreementCusAdvanceVnd);
                    mappingKeyValue.Add("agreementAdvAmountUsd", item.AgreementCusAdvanceUsd);
                    mappingKeyValue.Add("description", item.Description);

                    excel.SetData(mappingKeyValue);
                    startRow++;
                }

                //var listKeyFormula = new Dictionary<string, string>();
                //var _formular = string.Format("SUM({0}{1}:{0}{2})", "C", startRow, _length);
                //listKeyFormula.Add(totalFormat, _formular);

                return excel.ExcelStream();
            }
            catch (Exception ex)
            {
                fileName = "";
                return null;
            }
        }
    }
}
