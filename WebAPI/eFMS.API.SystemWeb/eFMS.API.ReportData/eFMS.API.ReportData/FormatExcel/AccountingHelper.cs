using eFMS.API.Common.Globals;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Accounting;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace eFMS.API.ReportData.FormatExcel
{
    public class AccountingHelper
    {
        const double minWidth = 0.00;
        const double maxWidth = 500.00;

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

        public Stream GenerateDetailAdvancePaymentExcel(AdvanceExport advanceExport, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Đề nghị tạm ứng (V)");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataDetailAdvancePaymentExcel(workSheet, advanceExport);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void BindingDataDetailAdvancePaymentExcel(ExcelWorksheet workSheet, AdvanceExport advanceExport)
        {
            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column B
                excelImage.SetPosition(1, 0, 1, 0);
            }

            List<string> headers = new List<string>()
            {
                "INDO TRANS LOGISTICS CORPORATION", //0
                "52-54-65 Truong Son St. Tan Binh Dist. HCM City. Vietnam\nTel: (84-8) 3948 6888  Fax: +84 8 38488 570\nE-mail:\nWebsite: www.itlvn.com",
                "PHIẾU ĐỀ NGHỊ TẠM ỨNG", //2
                "Người yêu cầu", //3
                "Ngày", //4
                "Bộ phận", //5
                "STT", //6
                "Thông tin chung", //7
                "Qty", //8
                "Số tiền tạm ứng (VND)", //9
                "Số cont - Loại cont", //10
                "C.W\n(kgs)", //11
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

            workSheet.Cells["H1:K1"].Merge = true;
            workSheet.Cells["H1"].Value = headers[0];
            workSheet.Cells["H1"].Style.Font.SetFromFont(new Font("Arial Black", 12));
            workSheet.Cells["H2:K2"].Merge = true;
            workSheet.Cells["H2:K2"].Style.WrapText = true;
            workSheet.Cells["H2"].Value = headers[1];
            workSheet.Row(2).Height = 80;

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
            workSheet.Cells["C5"].Value = advanceExport.InfoAdvance.Requester;

            workSheet.Cells["J5"].Value = headers[4]; //Ngày
            workSheet.Cells["K5"].Value = advanceExport.InfoAdvance.RequestDate.Value.ToString("dd/MM/yyyy");

            workSheet.Cells["J6"].Value = headers[5]; //Bộ phận
            workSheet.Cells["K6"].Value = advanceExport.InfoAdvance.Department;

            workSheet.Cells[8, 1, 9, 1].Merge = true;
            workSheet.Cells[8, 1, 9, 1].Value = headers[6];//STT

            workSheet.Cells[8, 2, 9, 3].Merge = true;
            workSheet.Cells[8, 2, 9, 3].Value = headers[7];//Thông tin chung

            workSheet.Cells[8, 4, 8, 7].Merge = true;
            workSheet.Cells[8, 4, 8, 7].Value = headers[8];//Qty

            workSheet.Cells[8, 4, 8, 7].Merge = true;
            workSheet.Cells[8, 4, 8, 7].Value = headers[8];//Qty

            workSheet.Cells["D9"].Value = headers[10]; //Số cont - Loại cont
            workSheet.Cells["E9"].Value = headers[11]; // C.W
            workSheet.Cells["F9"].Value = headers[12]; //Số kiện
            workSheet.Cells["G9"].Value = headers[13]; //số CBM

            workSheet.Cells[8, 8, 8, 11].Merge = true;
            workSheet.Cells[8, 8, 8, 11].Value = headers[9];//Số tiến tạm ứng

            workSheet.Cells["H9"].Value = headers[14]; //Định mức
            workSheet.Cells["I9"].Value = headers[15]; // Chi phí có hóa đơn
            workSheet.Cells["J9"].Value = headers[16]; //Chi phí khác
            workSheet.Cells["K9"].Value = headers[17]; //Tổng cộng

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

                workSheet.Cells[j, 2].Value = headers[24]; //Công ty nhập
                workSheet.Cells[j, 3].Value = advanceExport.ShipmentsAdvance[i].Consignee;
                j = j + 1;

                /////
                workSheet.Cells[p, 1, j - 1, 1].Merge = true;
                workSheet.Cells[p, 1, j - 1, 1].Value = i + 1; //Value STT

                workSheet.Cells[p, 4, j - 1, 4].Merge = true;
                workSheet.Cells[p, 4, j - 1, 4].Value = advanceExport.ShipmentsAdvance[i].Container; //Value Số cont - Loại cont

                workSheet.Cells[p, 5, j - 1, 5].Merge = true;
                workSheet.Cells[p, 5, j - 1, 5].Value = advanceExport.ShipmentsAdvance[i].Cw; //Value C.W

                workSheet.Cells[p, 6, j - 1, 6].Merge = true;
                workSheet.Cells[p, 6, j - 1, 6].Value = advanceExport.ShipmentsAdvance[i].Pcs; //Value Số kiện

                workSheet.Cells[p, 7, j - 1, 7].Merge = true;
                workSheet.Cells[p, 7, j - 1, 7].Value = advanceExport.ShipmentsAdvance[i].Cbm; //Value CBM

                workSheet.Cells[p, 8, j - 1, 8].Merge = true;
                workSheet.Cells[p, 8, j - 1, 8].Value = advanceExport.ShipmentsAdvance[i].NormAmount; //Value định mức

                workSheet.Cells[p, 9, j - 1, 9].Merge = true;
                workSheet.Cells[p, 9, j - 1, 9].Value = advanceExport.ShipmentsAdvance[i].InvoiceAmount; //Value chi phí có hóa đơn

                workSheet.Cells[p, 10, j - 1, 10].Merge = true;
                workSheet.Cells[p, 10, j - 1, 10].Value = advanceExport.ShipmentsAdvance[i].OtherAmount; //Value chi phí khác

                workSheet.Cells[p, 11, j - 1, 11].Merge = true;
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

            ////
            p = p + 2;

            workSheet.Cells[p, 1, p, 2].Merge = true;
            workSheet.Cells[p, 1, p, 2].Value = headers[26]; //Số tiền đề nghị tạm ứng
            workSheet.Cells[p, 3, p, 3].Value = totalAmount;

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
            workSheet.Cells[p, 3, p, 3].Value = advanceExport.InfoAdvance.DealinePayment.Value.ToString("dd/MM/yyyy"); //Thời hạn thanh toán

            p = p + 2;
            workSheet.Cells["B" + p].Value = headers[30]; //Người tạm ứng
            workSheet.Cells["C" + p].Value = headers[31]; //Người chứng từ
            workSheet.Cells["D" + p].Value = headers[32]; //Trưởng bộ phận
            workSheet.Cells[p, 7, p, 7].Merge = true;
            workSheet.Cells[p, 7, p, 7].Value = headers[33]; //Kế toán
            workSheet.Cells[p, 10, p, 10].Merge = true;
            workSheet.Cells[p, 10, p, 10].Value = headers[34]; //Giám đốc

            p = p + 5; //Bỏ trống 5 row
            workSheet.Cells["B" + p].Value = advanceExport.InfoAdvance.Requester; //Value Người tạm ứng
            workSheet.Cells["C" + p].Value = string.Empty; //Value Người chứng từ
            workSheet.Cells["D" + p].Value = advanceExport.InfoAdvance.Manager; //Value Trưởng bộ phận
            workSheet.Cells[p, 7, p, 7].Merge = true;
            workSheet.Cells[p, 7, p, 7].Value = advanceExport.InfoAdvance.Accountant; //Value Kế toán
            workSheet.Cells[p, 10, p, 10].Merge = true;
            workSheet.Cells[p, 10, p, 10].Value = string.Empty; //Giám đốc
        }

    }
}
