using eFMS.API.Common.Globals;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Accounting;
using eFMS.API.ReportData.Models.Common.Enums;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        const string numberFormat = "_-* #,##0.00_-;-* #,##0.00_-;_-* \"-\"??_-;_-@_-_(_)";
        const string numberFormatUSD = "_-* #,##0.000_-;-* #,##0.000_-;_-* \"-\"??_-;_-@_-_(_)";

        const string numberFormatVND = "_-\"VND\"* #,##0.00_-;-\"VND\"* #,##0.00_-;_-\"VND\"* \"-\"??_-;_-@_-_(_)";

        const string decimalFormat = "#,##0.00";
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
                    var worksheet = excelPackage.Workbook.Worksheets[1];

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
                    var worksheet = excelPackage.Workbook.Worksheets[1];

                    BuildHeader(worksheet, headers, "Advance Payment");


                    for (int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];
                        worksheet.Cells[i + addressStartContent, 1].Value = i + 1;
                        worksheet.Cells[i + addressStartContent, 2].Value = item.AdvanceNo;
                        worksheet.Cells[i + addressStartContent, 3].Value = item.RequestDate;
                        worksheet.Cells[i + addressStartContent, 3].Style.Numberformat.Format = "dd/MM/yyyy";
                        worksheet.Cells[i + addressStartContent, 4].Value = item.Requester;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.Amount;
                        worksheet.Cells[i + addressStartContent, 6].Value = item.RequestCurrency;
                        worksheet.Cells[i + addressStartContent, 7].Value = item.JobId;
                        worksheet.Cells[i + addressStartContent, 8].Value = item.Mbl;
                        worksheet.Cells[i + addressStartContent, 9].Value = item.Hbl;
                        worksheet.Cells[i + addressStartContent, 10].Value = item.CustomNo;
                        worksheet.Cells[i + addressStartContent, 11].Value = item.Description;
                        worksheet.Cells[i + addressStartContent, 12].Value = item.ApproveDate;
                        worksheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = "dd/MM/yyyy  HH:mm:ss AM/PM";
                        worksheet.Cells[i + addressStartContent, 13].Value = item.SettleDate;
                        worksheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = "dd/MM/yyyy";


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
                    var worksheet = excelPackage.Workbook.Worksheets[1];

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
                    var worksheet = excelPackage.Workbook.Worksheets[1];

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
                            worksheet.Cells[addressStartContent, 11].Value = request.SettlementAmount - request.AdvanceAmount;
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
                    worksheet.Cells[addressStartContent + 4, 1].Value = "Total Settlement";
                    worksheet.Cells[addressStartContent + 4, 1].Style.Font.Bold = true;
                    worksheet.Cells[addressStartContent + 4, 1].Style.Font.Bold = true;

                    worksheet.Cells[addressStartContent + 4, 2].Value = listObj.Sum(s => s.SettlementTotalAmount);

                    worksheet.Cells[addressStartContent + 5, 1].Value = "Total Advance";
                    worksheet.Cells[addressStartContent + 5, 1].Style.Font.Bold = true;
                    worksheet.Cells[addressStartContent + 5, 2].Value = listObj.Sum(d => d.AdvanceTotalAmount);

                    worksheet.Cells[addressStartContent + 6, 1].Value = "Total Balance";
                    worksheet.Cells[addressStartContent + 6, 1].Style.Font.Bold = true;
                    worksheet.Cells[addressStartContent + 6, 2].Value = listObj.Sum(b => b.BalanceTotalAmount);


                    excelPackage.Save();
                    return excelPackage.Stream;
                }
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
                    var worksheet = excelPackage.Workbook.Worksheets[1];

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
                    var worksheet = excelPackage.Workbook.Worksheets[1];

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
                    var workSheet = excelPackage.Workbook.Worksheets[1];
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
            workSheet.Column(2).Width = 20; //Cột B
            workSheet.Column(3).Width = 30; //Cột C
            workSheet.Column(4).Width = 20; //Cột D
            workSheet.Column(5).Width = 15; //Cột E
            workSheet.Column(8).Width = 15; //Cột H
            workSheet.Column(9).Width = 20; //Cột I
            workSheet.Column(10).Width = 18; //Cột J
            workSheet.Column(11).Width = 20; //Cột K
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
                "Giám đốc\n(Ký, ghi rõ họ tên)" //34
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
                "Director\n(Name, Signature)" //34
            };

            List<string> headers = language == "VN" ? vnHeaders : engHeaders;
            return headers;
        }

        private void BindingDataDetailAdvancePaymentExcel(ExcelWorksheet workSheet, AdvanceExport advanceExport, string language)
        {
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
            workSheet.Cells["H2"].Value = headers[1];
            workSheet.Cells["H2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10));
            workSheet.Cells["H2"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Row(2).Height = 60;

            workSheet.Cells[4, 1, 100000, 11].Style.Font.Name = "Times New Roman";

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

            workSheet.Cells["J6"].Value = headers[5]; //Bộ phận
            workSheet.Cells["J6"].Style.Font.Bold = true;
            workSheet.Cells["K6"].Value = advanceExport.InfoAdvance.Department;

            //Bôi đen header
            workSheet.Cells["A8:K9"].Style.Font.Bold = true;

            workSheet.Cells[8, 1, 9, 1].Merge = true;
            workSheet.Cells[8, 1, 9, 1].Value = headers[6];//STT
            workSheet.Cells[8, 1, 9, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[8, 1, 9, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[8, 2, 9, 3].Merge = true;
            workSheet.Cells[8, 2, 9, 3].Value = headers[7];//Thông tin chung
            workSheet.Cells[8, 2, 9, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[8, 2, 9, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[8, 4, 8, 7].Merge = true;
            workSheet.Cells[8, 4, 8, 7].Value = headers[8];//Qty
            workSheet.Cells[8, 4, 8, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[8, 4, 8, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            for (int x = 4; x < 8; x++)
            {
                workSheet.Cells[9, x].Style.WrapText = true;
                workSheet.Cells[9, x].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[9, x].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            workSheet.Cells[9, 4].Value = headers[10]; //Số cont - Loại cont           
            workSheet.Cells[9, 5].Value = headers[11]; // C.W           
            workSheet.Cells[9, 6].Value = headers[12]; //Số kiện            
            workSheet.Cells[9, 7].Value = headers[13]; //số CBM

            workSheet.Cells[8, 8, 8, 11].Merge = true;
            workSheet.Cells[8, 8, 8, 11].Value = headers[9];//Số tiến tạm ứng           
            workSheet.Cells[8, 8, 8, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[8, 8, 8, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            for (int x = 8; x < 12; x++)
            {
                workSheet.Cells[9, x].Style.WrapText = true;
                workSheet.Cells[9, x].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[9, x].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            workSheet.Cells[9, 8].Value = headers[14]; //Định mức           
            workSheet.Cells[9, 9].Value = headers[15]; // Chi phí có hóa đơn           
            workSheet.Cells[9, 10].Value = headers[16]; //Chi phí khác            
            workSheet.Cells[9, 11].Value = headers[17]; //Tổng cộng

            int p = 10;
            int j = 10;
            for (int i = 0; i < advanceExport.ShipmentsAdvance.Count; i++)
            {
                workSheet.Cells[j, 2].Value = headers[18]; //Số chứng từ
                workSheet.Cells[j, 3].Value = advanceExport.InfoAdvance.AdvanceNo;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[19]; //Số lô hàng
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].JobNo;
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
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[24]; //Công ty xuất
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].Shipper;
                j = j + 1;

                workSheet.Cells[j, 2].Value = headers[25]; //Công ty nhập
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].Consignee;
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

                for (int x = 5; x < 12; x++)
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

            workSheet.Cells[7, 1, 7, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + p + ":K" + p].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + (p + 1) + ":K" + (p + 1)].Style.Border.Top.Style = ExcelBorderStyle.Medium;

            //All border
            workSheet.Cells[8, 1, p, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[8, 1, p, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //In đậm border Cột 1
            for (var i = 8; i < p + 1; i++)
            {
                workSheet.Cells[i, 3].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            }

            //In đậm border Cột 2
            for (var i = 8; i < p + 1; i++)
            {
                workSheet.Cells[i, 7].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            }

            //In đậm border Cột 3
            for (var i = 8; i < p + 1; i++)
            {
                workSheet.Cells[i, 11].Style.Border.Right.Style = ExcelBorderStyle.Medium;
            }

            //Clear border Shipment
            int r = 10;
            int c = 16;
            for (var i = 0; i < advanceExport.ShipmentsAdvance.Count; i++)
            {
                workSheet.Cells["B" + r + ":C" + c].Style.Border.Bottom.Style = ExcelBorderStyle.None;//Xóa border bottom
                workSheet.Cells["B" + r + ":B" + c].Style.Border.Right.Style = ExcelBorderStyle.None;//Xóa border right
                workSheet.Cells["B" + r + ":B" + (c + 1)].Style.Border.Right.Style = ExcelBorderStyle.None;//Xóa border right (dư)
                r = r + 8;
                c = c + 8;
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

            p = p + 5; //Bỏ trống 5 row


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

            //Set font size từ header table trở xuống
            workSheet.Cells[8, 1, p, 11].Style.Font.Size = 10;
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
                    var workSheet = excelPackage.Workbook.Worksheets[1];
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
                    var worksheet = excelPackage.Workbook.Worksheets[1];

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
                        worksheet.Cells[i + addressStartContent, 4].Value = item.Service;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.PartnerCode;
                        worksheet.Cells[i + addressStartContent, 6].Value = item.Debit;
                        //worksheet.Cells[i + addressStartContent, 6].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[i + addressStartContent, 7].Value = item.Credit;
                        //worksheet.Cells[i + addressStartContent, 7].Style.Numberformat.Format = numberFormat;
                        worksheet.Cells[i + addressStartContent, 8].Value = item.ChargeCode;
                        worksheet.Cells[i + addressStartContent, 9].Value = item.OriginalCurrency;
                        worksheet.Cells[i + addressStartContent, 10].Value = amountStr;
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
                        worksheet.Cells[i + addressStartContent, 18].Value = string.Empty; // tạm thời để trống
                        worksheet.Cells[i + addressStartContent, 19].Value = string.Empty; // tạm thời để trống
                        worksheet.Cells[i + addressStartContent, 20].Value = string.Empty; // tạm thời để trống
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
                    var workSheet = excelPackage.Workbook.Worksheets[1];
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
               "Số Tờ Khai/Customs Declaration No",
               "Số Vận Đơn/HBL No",
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

            workSheet.Cells["P1:U1"].Merge = true;
            workSheet.Cells["P1"].Value = headers[0];
            workSheet.Cells["P1"].Style.Font.SetFromFont(new Font("Arial Black", 13));
            workSheet.Cells["P1"].Style.Font.Italic = true;
            workSheet.Cells["P2:U2"].Merge = true;
            workSheet.Cells["P2:U2"].Style.WrapText = true;
            workSheet.Cells["P2"].Value = headers[1];
            workSheet.Cells["P2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10));
            workSheet.Row(2).Height = 50;
            //Title
            workSheet.Cells["A4:U4"].Merge = true;
            workSheet.Cells["A4"].Style.Font.SetFromFont(new Font("Times New Roman", 16));
            workSheet.Cells["A4"].Value = headers[2];
            workSheet.Cells["A4"].Style.Font.Size = 16;
            workSheet.Cells["A4"].Style.Font.Bold = true;
            workSheet.Cells["A4"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["A5:U5"].Merge = true;
            DateTime fromDate = lstSoa.FromDate ?? DateTime.Now;
            DateTime toDate = lstSoa.ToDate ?? DateTime.Now;
            workSheet.Cells["A5"].Style.Font.SetFromFont(new Font("Times New Roman", 14));
            workSheet.Cells["A5"].Value = "Từ Ngày: " + fromDate.ToString("dd/MM/yyyy") + " đến: " + toDate.ToString("dd/MM/yyyy");
            workSheet.Cells["A5"].Style.Font.Bold = true;
            workSheet.Cells["A5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["A6:U6"].Merge = true;
            workSheet.Cells["A6"].Value = lstSoa.PartnerNameVN;
            workSheet.Cells["A7:U7"].Merge = true;
            workSheet.Cells["A7"].Value = lstSoa.BillingAddressVN;

            workSheet.Cells["A6:A7"].Style.Font.SetFromFont(new Font("Times New Roman", 15));

            workSheet.Cells["A6"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A6"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A7"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            decimal? totalALLValue = 0;

            // Tạo header
            for (int i = 0; i < headerTable.Count; i++)
            {
                if (i == 7)
                {
                    workSheet.Cells[8, i + 3].Value = headerTable[i];
                }
                if (i < 7)
                {
                    workSheet.Cells[8, i + 1].Value = headerTable[i];
                }
                if (i > 7)
                {
                    workSheet.Cells[8, i + 3].Value = headerTable[i];
                    workSheet.Cells[8, i + 3].Style.Font.Bold = true;
                }
                if (i > 8)
                {
                    workSheet.Cells[8, i + 3].Value = headerTable[i];
                    workSheet.Cells[8, i + 3].Style.Font.Bold = true;
                }

                if (i == 8)
                {
                    workSheet.Cells[8, i + 3].Value = headerTable[i];
                }


                workSheet.Cells[8, i + 1].Style.Font.Bold = true;
                workSheet.Cells[8, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[8, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            }
            workSheet.Cells["A8:A9"].Merge = true;
            workSheet.Cells["B8:B9"].Merge = true;
            workSheet.Cells["C8:C9"].Merge = true;
            workSheet.Cells["D8:D9"].Merge = true;
            workSheet.Cells["E8:E9"].Merge = true;
            workSheet.Cells["F8:F9"].Merge = true;

            workSheet.Cells["G8:I8"].Merge = true;
            workSheet.Cells["J8:L8"].Merge = true;

            workSheet.Cells["M8:O8"].Merge = true;

            workSheet.Cells["M8"].Value = headerTable[8];
            workSheet.Cells["P8:P9"].Merge = true;
            workSheet.Cells["P8"].Value = headerTable[9];
            workSheet.Cells["M8:P8"].Style.Font.Bold = true;

            workSheet.Cells["M8:P8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["M8:P8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["G9"].Value = subheaderTable[0];
            workSheet.Cells["H9"].Value = subheaderTable[1];
            workSheet.Cells["I9"].Value = subheaderTable[2];

            workSheet.Cells["J9"].Value = subheaderTable[3];
            workSheet.Cells["K9"].Value = subheaderTable[4];
            workSheet.Cells["L9"].Value = subheaderTable[5];

            workSheet.Cells["M9"].Value = subheaderTable[3];
            workSheet.Cells["N9"].Value = subheaderTable[4];
            workSheet.Cells["O9"].Value = subheaderTable[5];

            workSheet.Cells["G9:O9"].Style.Font.Bold = true;
            workSheet.Cells["G9:O9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["G9:O9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            int addressStartContent = 10;
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
                    workSheet.Cells[i + addressStartContent, 5].Value = item.Charges.Select(t => t.CustomNo).FirstOrDefault();
                    workSheet.Cells[i + addressStartContent, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 5].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 6].Value = item.HwbNo;
                    workSheet.Cells[i + addressStartContent, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 6].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 7].Value = item.GW;
                    workSheet.Cells[i + addressStartContent, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 7].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 8].Value = item.CBM;
                    workSheet.Cells[i + addressStartContent, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 8].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 9].Value = item.PackageContainer;
                    workSheet.Cells[i + addressStartContent, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 9].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 10].Value = item.Charges.Where(t => !t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 10].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartContent, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 10].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 11].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 11].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 12].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 13].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 14].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 15].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 15].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 16].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 16].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    workSheet.Cells[i + addressStartContent, 11].Value = item.Charges.Where(t => !t.Type.Contains("OBH")).Sum(t => t.VATAmount);
                    workSheet.Cells[i + addressStartContent, 11].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartContent, 12].Value = item.Charges.Where(t => !t.Type.Contains("OBH")).Sum(t => t.VATAmount) + item.Charges.Where(t => !t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[i + addressStartContent, 16].Value = item.Charges.Sum(t => t.VATAmount) + item.Charges.Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartContent, 13].Value = item.Charges.Where(t => t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 14].Value = item.Charges.Where(t => t.Type.Contains("OBH")).Sum(t => t.VATAmount);
                    workSheet.Cells[i + addressStartContent, 15].Value = item.Charges.Where(t => t.Type.Contains("OBH")).Sum(t => t.VATAmount) + item.Charges.Where(t => t.Type.Contains("OBH")).Sum(t => t.NetAmount);
                    workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartContent, 14].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = numberFormat;

                    for (int j = 0; j < item.Charges.Count; j++)
                    {
                        addressStartContent++;
                        var itemCharge = item.Charges[j];

                        workSheet.Cells[i + addressStartContent, 2].Value = itemCharge.ChargeName;
                        workSheet.Cells[i + addressStartContent, 3].Value = itemCharge.Quantity;
                        workSheet.Cells[i + addressStartContent, 4].Value = itemCharge.Unit;
                        workSheet.Cells[i + addressStartContent, 10].Value = itemCharge.NetAmount;
                        workSheet.Cells[i + addressStartContent, 10].Style.Numberformat.Format = numberFormat;
                        string vatAmount = "( " + itemCharge.VATAmount + " )";


                        if (itemCharge.VATAmount < 0)
                        {
                            workSheet.Cells[i + addressStartContent, 11].Value = vatAmount;
                            workSheet.Cells[i + addressStartContent, 14].Value = vatAmount;


                        }
                        else
                        {
                            workSheet.Cells[i + addressStartContent, 11].Value = itemCharge.VATAmount;
                            workSheet.Cells[i + addressStartContent, 11].Style.Numberformat.Format = numberFormatUSD;
                        }
                        workSheet.Cells[i + addressStartContent, 12].Value = itemCharge.VATAmount.GetValueOrDefault(0M) + itemCharge.NetAmount.GetValueOrDefault(0M);
                        workSheet.Cells[i + addressStartContent, 12].Style.Numberformat.Format = numberFormat;
                        decimal? TotalNormalCharge = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 12].Value);


                        if (itemCharge.Type.Contains("OBH"))
                        {
                            workSheet.Cells[i + addressStartContent, 13].Value = itemCharge.NetAmount;
                            workSheet.Cells[i + addressStartContent, 13].Style.Numberformat.Format = numberFormat;
                            workSheet.Cells[i + addressStartContent, 15].Value = itemCharge.VATAmount.GetValueOrDefault(0M) + itemCharge.NetAmount.GetValueOrDefault(0M);
                            workSheet.Cells[i + addressStartContent, 15].Style.Numberformat.Format = numberFormat;
                            workSheet.Cells[i + addressStartContent, 14].Value = itemCharge.VATAmount;
                            workSheet.Cells[i + addressStartContent, 14].Style.Numberformat.Format = numberFormatUSD;
                            workSheet.Cells[i + addressStartContent, 10].Value = null;
                            workSheet.Cells[i + addressStartContent, 11].Value = null;
                            workSheet.Cells[i + addressStartContent, 12].Value = null;

                        }

                        decimal? TotalOBHCharge = Convert.ToDecimal(workSheet.Cells[i + addressStartContent, 15].Value);
                        workSheet.Cells[i + addressStartContent, 16].Value = TotalNormalCharge.GetValueOrDefault(0M) + TotalOBHCharge.GetValueOrDefault(0M);
                        workSheet.Cells[i + addressStartContent, 16].Style.Numberformat.Format = numberFormat;

                        totalALLValue += TotalNormalCharge.GetValueOrDefault(0M) + TotalOBHCharge.GetValueOrDefault(0M);
                    }

                }

                addressStartContent = addressStartContent + lstSoa.exportSOAOPs.Count;

                workSheet.Cells[8, 1, addressStartContent, 16].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[8, 1, addressStartContent, 16].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[8, 1, addressStartContent, 16].Style.Border.Top.Style = ExcelBorderStyle.Thin;


                workSheet.Cells[addressStartContent, 1].Value = headers[3]; //Total
                string addressTotal = workSheet
                .Cells[addressStartContent, 1]
                .First(c => c.Value.ToString() == headers[3])
                .Start
                .Address;
                string addressTotalMerge = workSheet
                 .Cells[addressStartContent, 9].Start.Address;
                string addressToMerge = addressTotal + ":" + addressTotalMerge;
                workSheet.Cells[addressToMerge].Merge = true;
                workSheet.Cells[addressToMerge].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[addressToMerge].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                string addressTotalNext = workSheet
               .Cells[addressStartContent, 10].Start.Address;

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

                }

                totalAll = totalOBHCharge + totalNormalCharge;

                workSheet.Cells[addressTotalNext].Value = totalNetAmountNormalCharge;
                workSheet.Cells[addressTotalNext].Style.Numberformat.Format = numberFormat;

                string addressTotalVat = workSheet.Cells[addressStartContent, 11].Start.Address;
                workSheet.Cells[addressTotalVat].Value = totalVATNormalCharge;
                workSheet.Cells[addressTotalVat].Style.Numberformat.Format = numberFormat;

                string addressTotalNormalCharge = workSheet.Cells[addressStartContent, 12].Start.Address;
                workSheet.Cells[addressTotalNormalCharge].Value = totalNormalCharge;
                workSheet.Cells[addressTotalNormalCharge].Style.Numberformat.Format = numberFormat;

                string addressNetAmountCharge = workSheet.Cells[addressStartContent, 13].Start.Address;
                workSheet.Cells[addressNetAmountCharge].Value = totalNetAmountOBHCharge;
                workSheet.Cells[addressNetAmountCharge].Style.Numberformat.Format = numberFormat;

                string addressVATChargeNext = workSheet.Cells[addressStartContent, 14].Start.Address;
                workSheet.Cells[addressVATChargeNext].Value = totalVATOBHCharge;
                workSheet.Cells[addressVATChargeNext].Style.Numberformat.Format = numberFormat;

                string addressTotalChargeNext = workSheet.Cells[addressStartContent, 15].Start.Address;
                workSheet.Cells[addressTotalChargeNext].Value = totalOBHCharge;
                workSheet.Cells[addressTotalChargeNext].Style.Numberformat.Format = numberFormat;


                string addressTotalAll = workSheet.Cells[addressStartContent, 16].Start.Address;
                workSheet.Cells[addressTotalAll].Value = totalAll;
                workSheet.Cells[addressTotalAll].Style.Numberformat.Format = numberFormat;


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
                //workSheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                //workSheet.Cells.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
        }

        /// <returns></returns>
        public Stream GenerateSOAAirfreightExcel(ExportSOAAirfreightModel soaAir, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Air freight");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BinddingDataDetailSOAAirfreight(workSheet, soaAir);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        public void BinddingDataDetailSOAAirfreight(ExcelWorksheet workSheet, ExportSOAAirfreightModel airfreightObj)
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
               "DEBIT NOTE IN OCT 2019 ( BẢNG KÊ CƯỚC VCQT)", //2
               "TOTAL", //3
            };

            List<string> headerTable = new List<string>()
            {
                "No", //3
               "Job No", //4
               "Flight No", //5
               "Shippment Date", //6
               "MAWB#", //7
               "Dest", //8
               "Service", //9
               "Air PCS", //10
               "Gross Weight(KG)", //11
               "Chargeable Weight(KG)", //12
               "Rate(USD)", //13
               "AirFreight(USD)", //14
               "Fuel Surcharge(USD)", //15
               "Warisk Surcharge(USD)", //16
               "Screening Surcharge(USD)", //17
               "AWB(USD)", //18
               "Handling fee(USD)", //19
               "Net Amount(USD)", //20
               "Exchange Rate(VND/USD)", //21
               "Total Amount(VND)", //21
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
                if (i == 5)
                {
                    workSheet.Cells[8, i + 2].Value = headerTable[i];
                }
                if (i < 5)
                {
                    workSheet.Cells[8, i + 1].Value = headerTable[i];
                }
                if (i > 5)
                {
                    workSheet.Cells[8, i + 2].Value = headerTable[i];
                    workSheet.Cells[8, i + 2].Style.Font.Bold = true;

                }

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

            workSheet.Cells["E8:F9"].Merge = true;


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

                workSheet.Cells[i + addressStartContent, 5].Value = item.AOL;
                workSheet.Cells[i + addressStartContent, 6].Value = item.Mawb;
                workSheet.Cells[i + addressStartContent, 7].Value = item.AOD;
                workSheet.Cells[i + addressStartContent, 8].Value = item.Service;
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

                workSheet.Cells[i + addressStartContent, 18].Value = item.HandlingFee;
                workSheet.Cells[i + addressStartContent, 18].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 19].Value = item.NetAmount;
                workSheet.Cells[i + addressStartContent, 19].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 20].Value = item.ExchangeRate;
                workSheet.Cells[i + addressStartContent, 20].Style.Numberformat.Format = numberFormat;

                workSheet.Cells[i + addressStartContent, 21].Value = item.TotalAmount;
                workSheet.Cells[i + addressStartContent, 21].Style.Numberformat.Format = numberFormat;


                row1++;

            }
            workSheet.Cells[8, 1, row1 + 1, 21].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[8, 1, row1, 21].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[8, 1, row1, 21].Style.Border.Top.Style = ExcelBorderStyle.Thin;
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
             .Cells[addressStartContent, 18]
             .Start
             .Address;
            workSheet.Cells[idHF].Value = airfreightObj.HawbAirFrieghts.Select(t => t.HandlingFee).Sum();
            workSheet.Cells[idHF].Style.Numberformat.Format = numberFormat;

            string idNA = workSheet
             .Cells[addressStartContent, 19]
             .Start
             .Address;
            workSheet.Cells[idNA].Value = airfreightObj.HawbAirFrieghts.Select(t => t.NetAmount).Sum();
            workSheet.Cells[idNA].Style.Numberformat.Format = numberFormat;

            string idTT = workSheet
             .Cells[addressStartContent, 21]
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
            .Cells[addressStartContent, 21]
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
                textBottom = textBottom + "\n" + "A/C: " + airfreightObj.BankAccountVND;
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
        #endregion


        #region --- SETTLEMENT PAYMENT ---
        /// <summary>
        /// Generate detail settlement payment excel
        /// </summary>
        /// <param name="settlementExport"></param>
        /// <param name="language"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateDetailSettlementPaymentExcel(SettlementExport settlementExport, string language, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    string sheetName = language == "VN" ? "(V)" : "(E)";
                    excelPackage.Workbook.Worksheets.Add("Đề nghị thanh toán " + sheetName);
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataDetailSettlementPaymentExcel(workSheet, settlementExport, language);
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
            workSheet.Column(1).Width = 8; //Cột A
            workSheet.Column(2).Width = 20; //Cột B
            workSheet.Column(3).Width = 30; //Cột C
            workSheet.Column(4).Width = 6; //Cột D
            workSheet.Column(5).Width = 30; //Cột E
            workSheet.Column(6).Width = 20; //Cột F
            workSheet.Column(7).Width = 15; //Cột G
            workSheet.Column(8).Width = 15; //Cột H
            workSheet.Column(9).Width = 20; //Cột I
            workSheet.Column(10).Width = 15; //Cột J
            workSheet.Column(11).Width = 18; //Cột K
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
                "Số chứng tứ:", //6
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
                "Giám đốc\n(Ký, ghi rõ họ tên)" //36
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
                "Director\n(Name, Signature)" //36
            };

            List<string> headers = language == "VN" ? vnHeaders : engHeaders;
            return headers;
        }

        private void BindingDataDetailSettlementPaymentExcel(ExcelWorksheet workSheet, SettlementExport settlementExport, string language)
        {
            SetWidthColumnExcelDetailSettlementPayment(workSheet);

            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column A
                excelImage.SetPosition(0, 0, 0, 0);
            }

            List<string> headers = GetHeaderExcelDetailSettlementPayment(language);

            workSheet.Cells["H1:K1"].Merge = true;
            workSheet.Cells["H1"].Value = headers[0];
            workSheet.Cells["H1"].Style.Font.SetFromFont(new Font("Arial Black", 10));
            workSheet.Cells["H1"].Style.Font.Italic = true;
            workSheet.Cells["H2:K2"].Merge = true;
            workSheet.Cells["H2:K2"].Style.WrapText = true;
            workSheet.Cells["H2"].Value = headers[1];
            workSheet.Cells["H2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 8));
            workSheet.Cells["H2"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Row(2).Height = 50;

            //Title
            workSheet.Cells["A3:K3"].Merge = true;
            workSheet.Cells["A3"].Style.Font.SetFromFont(new Font("Times New Roman", 16));
            workSheet.Cells["A3"].Value = headers[2]; //Đề nghị thanh toán
            workSheet.Cells["A3"].Style.Font.Size = 16;
            workSheet.Cells["A3"].Style.Font.Bold = true;
            workSheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A3"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A5:J6"].Style.Font.SetFromFont(new Font("Times New Roman", 11));

            workSheet.Cells["A5:B5"].Merge = true;
            workSheet.Cells["A5"].Value = headers[3]; //Người yêu cầu
            workSheet.Cells["A5"].Style.Font.Bold = true;
            workSheet.Cells["C5"].Value = settlementExport.InfoSettlement.Requester;

            workSheet.Cells["I5"].Value = headers[4]; //Ngày thanh toán
            workSheet.Cells["I5"].Style.Font.Bold = true;
            workSheet.Cells["J5"].Value = settlementExport.InfoSettlement.RequestDate;
            workSheet.Cells["J5"].Style.Numberformat.Format = "dd MMM, yyyy";
            workSheet.Cells["J5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            workSheet.Cells["A6:B6"].Merge = true;
            workSheet.Cells["A6"].Value = headers[5]; //Bộ phận
            workSheet.Cells["A6"].Style.Font.Bold = true;
            workSheet.Cells["C6"].Value = settlementExport.InfoSettlement.Department;

            workSheet.Cells["I6"].Value = headers[6]; //Số chứng từ
            workSheet.Cells["I6"].Style.Font.Bold = true;
            workSheet.Cells["J6"].Value = settlementExport.InfoSettlement.SettlementNo;

            workSheet.Cells[8, 1, 100000, 11].Style.Font.SetFromFont(new Font("Times New Roman", 10));

            //Bôi đen header
            workSheet.Cells["A8:K8"].Style.Font.Bold = true;

            for (var col = 1; col < 12; col++)
            {
                workSheet.Cells[8, col].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[8, col].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[8, col].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }
            workSheet.Cells["A8"].Value = headers[7];//STT

            workSheet.Cells["B8:C8"].Merge = true;
            workSheet.Cells["B8:C8"].Value = headers[8];//Thông tin chung

            workSheet.Cells["D8:E8"].Merge = true;
            workSheet.Cells["D8:E8"].Value = headers[9];//Diễn giải

            workSheet.Cells["F8"].Value = headers[10];//Số tiền
            workSheet.Cells["G8"].Value = headers[11];//Số hóa đơn
            workSheet.Cells["H8"].Value = headers[12];//Ghi chú
            workSheet.Cells["I8"].Value = headers[13];//Số tiền đã tạm ứng
            workSheet.Cells["J8"].Value = headers[14];//Ngày tạm ứng
            workSheet.Cells["K8"].Value = headers[15];//Chênh lệch
            workSheet.Row(8).Height = 30;

            workSheet.Cells[8, 1, 8, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            decimal? _sumTotalAmount = 0;
            decimal? _sumTotalAdvancedAmount = 0;
            decimal? _sumTotalDifference = 0;

            int p = 9;
            int j = 9;
            int k = 9;
            for (int i = 0; i < settlementExport.ShipmentsSettlement.Count; i++)
            {
                #region --- Diễn giải ---
                int _no = 1;
                var invoiceCharges = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Where(w => w.ChargeType == "INVOICE");
                //Title Chi phí có hóa đơn
                workSheet.Cells[k, 4].Value = headers[27]; //Chi phí có hóa đơn
                workSheet.Cells[k, 4].Style.Font.Bold = true;
                workSheet.Cells[k, 6].Value = invoiceCharges.Select(s => s.ChargeAmount).Sum(); //Value tổng chi phí có hóa đơn
                workSheet.Cells[k, 6].Style.Font.Bold = true;
                workSheet.Cells[k, 6].Style.Numberformat.Format = numberFormat;
                k += 1;
                foreach (var invoice in invoiceCharges)
                {
                    workSheet.Cells[k, 4].Value = _no;
                    workSheet.Cells[k, 5].Value = invoice.ChargeName;

                    workSheet.Cells[k, 6].Value = invoice.ChargeAmount;
                    workSheet.Cells[k, 6].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[k, 7].Value = invoice.InvoiceNo;
                    workSheet.Cells[k, 8].Value = invoice.ChargeNote;

                    k += 1;
                    _no += 1;
                }

                var noInvoiceCharges = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Where(w => w.ChargeType == "NO_INVOICE");
                //Title Chi phí không hóa đơn
                workSheet.Cells[k, 4].Value = headers[28]; //Chi phí không hóa đơn
                workSheet.Cells[k, 4].Style.Font.Bold = true;
                workSheet.Cells[k, 6].Value = noInvoiceCharges.Select(s => s.ChargeAmount).Sum(); //Value tổng chi phí không hóa đơn
                workSheet.Cells[k, 6].Style.Font.Bold = true;
                workSheet.Cells[k, 6].Style.Numberformat.Format = numberFormat;
                k += 1;
                foreach (var no_invoice in noInvoiceCharges)
                {
                    workSheet.Cells[k, 4].Value = _no;
                    workSheet.Cells[k, 5].Value = no_invoice.ChargeName;

                    workSheet.Cells[k, 6].Value = no_invoice.ChargeAmount;
                    workSheet.Cells[k, 6].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[k, 7].Value = no_invoice.InvoiceNo;
                    workSheet.Cells[k, 8].Value = no_invoice.ChargeNote;

                    k += 1;
                    _no += 1;
                }

                var obhCharges = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Where(w => w.ChargeType == "OBH");
                //Title phí chi hộ
                workSheet.Cells[k, 4].Value = headers[29]; //Chi hộ
                workSheet.Cells[k, 4].Style.Font.Bold = true;
                workSheet.Cells[k, 6].Value = obhCharges.Select(s => s.ChargeAmount).Sum(); //Value tổng phí chi hộ
                workSheet.Cells[k, 6].Style.Font.Bold = true;
                workSheet.Cells[k, 6].Style.Numberformat.Format = numberFormat;
                k += 1;
                foreach (var obh in obhCharges)
                {
                    workSheet.Cells[k, 4].Value = _no;
                    workSheet.Cells[k, 5].Value = obh.ChargeName;

                    workSheet.Cells[k, 6].Value = obh.ChargeAmount;
                    workSheet.Cells[k, 6].Style.Numberformat.Format = numberFormat;

                    workSheet.Cells[k, 7].Value = obh.InvoiceNo;
                    workSheet.Cells[k, 8].Value = obh.ChargeNote;

                    k += 1;
                    _no += 1;
                }



                #endregion

                #region --- Thông tin chung ---
                workSheet.Cells[j, 2].Value = headers[16]; //Số lô hàng
                workSheet.Cells[j, 3].Value = settlementExport.ShipmentsSettlement[i].JobNo;
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

                workSheet.Cells[j - 1, 4, j - 1, 11].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[j - 1, 4, j - 1, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[p, 1, p, 11].Style.Border.Top.Style = ExcelBorderStyle.Thin;

                #region --- Change type border diễn giải ---
                for (var f = p; f < j - 2; f++)
                {
                    workSheet.Cells[f, 4, f, 8].Style.Border.Bottom.Style = ExcelBorderStyle.Dotted;
                    workSheet.Cells[f, 4, f, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }
                workSheet.Cells[p, 4, j - 1, 8].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                #endregion --- Change type border diễn giải ---

                //Title Subtotal
                workSheet.Cells[j - 1, 4, j - 1, 5].Merge = true;
                workSheet.Cells[j - 1, 4, j - 1, 5].Value = headers[30];
                workSheet.Cells[j - 1, 4, j - 1, 5].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 4, j - 1, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[j - 1, 4, j - 1, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //Value total amount
                var _totalAmount = settlementExport.ShipmentsSettlement[i].ShipmentCharges.Select(s => s.ChargeAmount).Sum();
                workSheet.Cells[j - 1, 6].Value = _totalAmount;
                workSheet.Cells[j - 1, 6].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[j - 1, 6].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //Value total advanced amount (số tiền đã tạm ứng)
                var _advanceAmount = settlementExport.ShipmentsSettlement[i].AdvanceAmount ?? 0;
                workSheet.Cells[j - 1, 9].Value = _advanceAmount;
                workSheet.Cells[j - 1, 9].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[j - 1, 9].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                workSheet.Cells[j - 1, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //Value chênh lệch
                workSheet.Cells[j - 1, 11].Value = _totalAmount - _advanceAmount;
                workSheet.Cells[j - 1, 11].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[j - 1, 11].Style.Font.Bold = true;
                workSheet.Cells[j - 1, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ////

                workSheet.Cells[p, 1, j - 1, 1].Merge = true;
                workSheet.Cells[p, 1, j - 1, 1].Value = i + 1; //Value STT
                workSheet.Cells[p, 1, j - 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 1, j - 1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[p, 1, j - 1, 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                workSheet.Cells[p, 9, j - 2, 9].Merge = true;
                workSheet.Cells[p, 9, j - 2, 9].Value = settlementExport.ShipmentsSettlement[i].AdvanceAmount; //Value Số tiền đã tạm ứng
                workSheet.Cells[p, 9, j - 2, 9].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[p, 9, j - 2, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 9, j - 2, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[p, 9, j - 2, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                workSheet.Cells[p, 10, j - 2, 10].Merge = true;
                workSheet.Cells[p, 10, j - 2, 10].Value = settlementExport.ShipmentsSettlement[i].AdvanceRequestDate.HasValue ? settlementExport.ShipmentsSettlement[i].AdvanceRequestDate.Value.ToString("dd/MM/yyyy") : string.Empty; //Value Ngày tạm ứng
                workSheet.Cells[p, 10, j - 2, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 10, j - 2, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[p, 10, j - 2, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                workSheet.Cells[p, 11, j - 2, 11].Merge = true;
                workSheet.Cells[p, 11, j - 2, 11].Value = string.Empty; //Value Chênh lệch
                workSheet.Cells[p, 11, j - 2, 11].Style.Numberformat.Format = numberFormat;
                workSheet.Cells[p, 11, j - 2, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[p, 11, j - 2, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[p, 11, j - 2, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                p = j;
                ////

                _sumTotalAmount += (settlementExport.ShipmentsSettlement[i].ShipmentCharges.Select(s => s.ChargeAmount).Sum() ?? 0);
                _sumTotalAdvancedAmount += (settlementExport.ShipmentsSettlement[i].AdvanceAmount ?? 0);
                _sumTotalDifference = _sumTotalAmount - _sumTotalAdvancedAmount;
            }

            ////TỖNG CỘNG
            workSheet.Cells[p, 1, p, 5].Merge = true;
            workSheet.Cells[p, 1, p, 5].Value = headers[31]; //Title TỔNG CỘNG
            workSheet.Cells[p, 1, p, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 1, p, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[p, 1, p, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 6].Value = _sumTotalAmount; //Value sum total amount
            workSheet.Cells[p, 6].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 9].Value = _sumTotalAdvancedAmount; //Value sum total advanced amount
            workSheet.Cells[p, 9].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 9].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[p, 11].Value = _sumTotalDifference; //Value sum total difference
            workSheet.Cells[p, 11].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[p, 11].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //Bôi đen dòng tổng cộng ở cuối
            workSheet.Cells["A" + p + ":K" + p].Style.Font.Bold = true;
            workSheet.Cells["A" + p + ":K" + p].Style.Numberformat.Format = numberFormat;

            //In đậm border dòng 7
            workSheet.Cells[7, 1, 7, 11].Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + p + ":K" + p].Style.Border.Top.Style = ExcelBorderStyle.Medium;
            workSheet.Cells["A" + (p + 1) + ":K" + (p + 1)].Style.Border.Top.Style = ExcelBorderStyle.Medium;

            for (var i = 8; i < p + 1; i++)
            {
                //In đậm border Cột 3
                workSheet.Cells[i, 3].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                //In đậm border Cột 8
                workSheet.Cells[i, 8].Style.Border.Right.Style = ExcelBorderStyle.Medium;
                //In đậm border Cột 11
                workSheet.Cells[i, 11].Style.Border.Right.Style = ExcelBorderStyle.Medium;
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

            workSheet.Cells[p, 6, p, 8].Merge = true;
            workSheet.Cells[p, 6, p, 8].Style.WrapText = true;
            workSheet.Cells[p, 6, p, 8].Value = headers[35]; //Kế toán
            workSheet.Cells[p, 6, p, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 6, p, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells[p, 9, p, 11].Merge = true;
            workSheet.Cells[p, 9, p, 11].Value = headers[36]; //Giám đốc
            workSheet.Cells[p, 9, p, 11].Style.WrapText = true;
            workSheet.Cells[p, 9, p, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[p, 9, p, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            p = p + 5;

            workSheet.Cells[p, 2].Style.WrapText = true;
            workSheet.Cells[p, 2].Value = settlementExport.InfoSettlement.Requester; //Value Người tạm ứng    
            workSheet.Cells[p, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[p, 3].Style.WrapText = true;
            workSheet.Cells[p, 3].Value = string.Empty; //Value Người chứng từ 

            workSheet.Cells[p, 4, p, 5].Merge = true;
            workSheet.Cells[p, 4, p, 5].Style.WrapText = true;
            workSheet.Cells[p, 4, p, 5].Value = settlementExport.InfoSettlement.Manager; //Value Trưởng bộ phận
            workSheet.Cells[p, 4, p, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[p, 6, p, 8].Merge = true;
            workSheet.Cells[p, 6, p, 8].Style.WrapText = true;
            workSheet.Cells[p, 6, p, 8].Value = settlementExport.InfoSettlement.Accountant; //Value Kế toán
            workSheet.Cells[p, 6, p, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells[p, 9, p, 11].Merge = true;
            workSheet.Cells[p, 9, p, 11].Value = string.Empty; //Value Giám đốc
        }

        #endregion --- SETTLEMENT PAYMENT ---

        #region --- ACCOUNTING MANAGEMENT ---
        public Stream GenerateAccountingManagementExcel(List<AccountingManagementExport> acctMngts, string typeOfAcctMngt, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Sheet1");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
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
                     
            rowStart += 1;
            foreach(var item in acctMngts)
            {               
                workSheet.Cells[rowStart, 1].Value = item.Date; //Ngày chứng từ
                workSheet.Cells[rowStart, 1].Style.Numberformat.Format = "dd/MM/yyyy";

                workSheet.Cells[rowStart, 2].Value = item.VoucherId; //Số chứng từ 
                workSheet.Cells[rowStart, 3].Value = (typeOfAcctMngt == "Invoice") ? "HD" : item.VoucherId?.Substring(0, 2); //2 ký tự đầu của số chứng từ
                workSheet.Cells[rowStart, 4].Value = item.ChargeName;
                workSheet.Cells[rowStart, 5].Value = item.VatPartnerCode; //Mã số thuế của partner của charge
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
        #endregion --- ACCOUTING MANAGEMENT ---
    }
}
