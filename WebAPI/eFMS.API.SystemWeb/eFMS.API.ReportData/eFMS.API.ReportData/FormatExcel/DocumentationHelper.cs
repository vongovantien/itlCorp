using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Criteria;
using eFMS.API.ReportData.Models.Documentation;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace eFMS.API.ReportData.FormatExcel
{
    public class DocumentationHelper
    {
        const string numberFormat = "#,##0.00";
        const string numberFormatKgs = "#,##0 \"KGS\"";
        const string numberFormatVND = "_-* #,##0.000_-;-* #,##0.000_-;_-* \"-\"??_-;_-@_-_(_)";
        const string numberFormats = "#,##0";
        const string CURRENCY_LOCAL = "VND";
        const string CURRENCY_USD = "USD";

        public Stream CreateEManifestExcelFile(CsTransactionDetailModel transactionDetail, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    GenerateEManifestExcel(workSheet, transactionDetail);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void GenerateEManifestExcel(ExcelWorksheet workSheet, CsTransactionDetailModel transactionDetail)
        {
            List<String> headers = new List<String>()
            {
                "DANH SÁCH VẬN ĐƠN GOM HÀNG",
                "(List of House bill of lading)"
            };
            List<String> containerHeaders = new List<String>()
            {
                "Mã hàng \nHS code if avail",
                "Mô tả hàng hóa* \nDescription of Goods",
                "Tổng trọng lượng* \nGross weight",
                "Kích thước/thể tích * \nDemension/tonnage",
                "Số hiệu cont \nCont. number",
                "Số Seal cont \nSeal number"
            };
            Color colTitleFromHex = System.Drawing.ColorTranslator.FromHtml("#00b0f0");
            workSheet.Cells["A1"].Value = headers[0];
            FormatTitleHeader(workSheet, "A1", "Times New Roman");
            workSheet.Cells[1, 1, 1, 8].Merge = true;
            workSheet.Cells[1, 1, 1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[1, 1, 1, 8].Style.Fill.BackgroundColor.SetColor(colTitleFromHex);
            workSheet.Cells["A1"].AutoFitColumns();

            workSheet.Cells["A2"].Value = headers[1];
            FormatTitleHeader(workSheet, "A2", "Times New Roman");
            workSheet.Cells[2, 1, 2, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[2, 1, 2, 8].Style.Fill.BackgroundColor.SetColor(colTitleFromHex);
            workSheet.Cells[2, 1, 2, 8].Merge = true;
            workSheet.Cells["A2"].Style.WrapText = true;
            workSheet.Cells["A2"].AutoFitColumns();

            List<TitleModel> houseTitles = new List<TitleModel>()
            {
                new TitleModel { VNTitle = "STT", ENTitle = "No"},
                new TitleModel { VNTitle = "Số hồ sơ ", ENTitle =  "Document's No"},
                new TitleModel { VNTitle = "Năm đăng ký hồ sơ ", ENTitle = "Document's Year"},
                new TitleModel { VNTitle = "Chức năng của chứng từ ", ENTitle = "Document's function"},
                new TitleModel { VNTitle = "Người gửi hàng* ", ENTitle = "Shipper"},
                new TitleModel { VNTitle = "Người nhận hàng* ", ENTitle = "Consignee"},
                new TitleModel { VNTitle = "Người được thông báo 1", ENTitle = "Notify Party 1"},
                new TitleModel { VNTitle = "Người được thông báo 2", ENTitle = "Notify Party 2"},
                new TitleModel { VNTitle = "Mã Cảng chuyển tải/ quá cảnh", ENTitle = "Code of Port of transhipment/transit"}
            };
            int addressStartContent = 3;
            for (int i = 0; i < houseTitles.Count; i++)
            {
                workSheet.Cells[addressStartContent, i + 1].Style.WrapText = true;
                workSheet.Cells[addressStartContent, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[addressStartContent, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[addressStartContent, i + 1].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                workSheet.Cells[addressStartContent, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[addressStartContent, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));
                BorderDashItem(workSheet, addressStartContent, i + 1);

                ExcelRichText ert = workSheet.Cells[addressStartContent, i + 1].RichText.Add(houseTitles[i].VNTitle);
                ert.Color = System.Drawing.Color.Red;
                ert = workSheet.Cells[addressStartContent, i + 1].RichText.Add(" \n" + houseTitles[i].ENTitle);
                ert.Color = System.Drawing.Color.Black;
                workSheet.Column(i + 1).Width = 25;
            }

            workSheet.Cells[4, 1, 4, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[4, 1, 4, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));
            workSheet.Cells[4, 1].Value = 01;
            workSheet.Cells[4, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffd966"));
            workSheet.Cells[4, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[4, 3].Value = DateTime.Now.Year;
            workSheet.Cells[4, 4].Value = "CN01";
            workSheet.Cells[4, 5].Value = transactionDetail.ShipperDescription;
            workSheet.Cells[4, 6].Value = transactionDetail.ConsigneeDescription;
            workSheet.Cells[4, 7].Value = transactionDetail.NotifyPartyDescription;
            workSheet.Cells[4, 8].Value = transactionDetail.AlsoNotifyPartyDescription;
            workSheet.Cells[4, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffd966"));
            List<TitleModel> containerTitles = new List<TitleModel>()
            {
                new TitleModel { VNTitle = "Mã hàng", ENTitle = "HS code if avail" },
                new TitleModel { VNTitle = "Mô tả hàng hóa", ENTitle = "Description of Goods" },
                new TitleModel { VNTitle = "Tổng trọng lượng*", ENTitle = "Gross Weight" },
                new TitleModel { VNTitle = "Kích thước/ thể tích*", ENTitle = "Demension/ tonnage" },
                new TitleModel { VNTitle = "Số hiệu cont", ENTitle = "Cont. number"},
                new TitleModel { VNTitle = "Số seal cont", ENTitle = "Seal number"}
            };
            addressStartContent = 6;
            for (int i = 0; i < containerTitles.Count; i++)
            {
                workSheet.Cells[addressStartContent, i + 2].Style.WrapText = true;
                workSheet.Cells[addressStartContent, i + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[addressStartContent, i + 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[addressStartContent, i + 2].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                workSheet.Cells[addressStartContent, i + 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[addressStartContent, i + 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));
                BorderDashItem(workSheet, addressStartContent, i + 2);

                ExcelRichText ert = workSheet.Cells[addressStartContent, i + 2].RichText.Add(containerTitles[i].VNTitle);
                ert.Color = System.Drawing.Color.Red;
                ert = workSheet.Cells[addressStartContent, i + 2].RichText.Add(" \n" + containerTitles[i].ENTitle);
                ert.Color = System.Drawing.Color.Black;
                workSheet.Column(i + 2).Width = 25;
            }
            addressStartContent = 7;
            if (transactionDetail.CsMawbcontainers != null)
            {
                foreach (var item in transactionDetail.CsMawbcontainers)
                {
                    workSheet.Cells[addressStartContent, 2].Value = item.CommodityName;
                    workSheet.Cells[addressStartContent, 3].Value = item.Description;
                    workSheet.Cells[addressStartContent, 4].Value = item.Gw;
                    workSheet.Cells[addressStartContent, 5].Value = item.Cbm;
                    workSheet.Cells[addressStartContent, 6].Value = item.ContainerNo;
                    workSheet.Cells[addressStartContent, 7].Value = item.SealNo;

                    for (int i = 1; i < containerTitles.Count + 1; i++)
                    {
                        BorderThinItem(workSheet, addressStartContent, i + 1);
                        workSheet.Cells[addressStartContent, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        workSheet.Cells[addressStartContent, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[addressStartContent, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));
                    }
                    addressStartContent = addressStartContent + 1;
                }
            }
        }

        internal Stream CreateDangerousGoods(CsTransactionDetailModel transactionDetail, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    GenerateDangerousGoods(workSheet, transactionDetail);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void GenerateDangerousGoods(ExcelWorksheet workSheet, CsTransactionDetailModel transactionDetail)
        {
            List<String> headers = new List<String>()
            {
                "BẢN KHAI HÀNG HÓA NGUY HIỂM",
                "Dangerous goods manifest"
            };
            //Color colTitleFromHex = System.Drawing.ColorTranslator.FromHtml("#00b0f0");
            //Color colSubTitleFromHex = System.Drawing.ColorTranslator.FromHtml("#d9e1f2");
            workSheet.Cells["A1"].Value = headers[0];
            FormatTitleHeader(workSheet, "A1", "Times New Roman");
            workSheet.Cells[1, 1, 1, 13].Merge = true;
            workSheet.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //workSheet.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(colTitleFromHex);
            workSheet.Cells["A1"].AutoFitColumns();

            workSheet.Cells["A2"].Value = headers[1];
            FormatTitleHeader(workSheet, "A2", "Times New Roman");
            workSheet.Cells[2, 1, 2, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
            //workSheet.Cells[2, 1, 2, 13].Style.Fill.BackgroundColor.SetColor(colTitleFromHex);
            workSheet.Cells[2, 1, 2, 13].Merge = true;
            workSheet.Cells["A2"].Style.WrapText = true;
            workSheet.Cells["A1"].Value = headers[0];
            FormatTitleHeader(workSheet, "A1", "Times New Roman");
            workSheet.Cells[1, 1, 1, 16].Merge = true;

            workSheet.Cells["A2"].Value = headers[1];
            FormatTitleHeader(workSheet, "A1", "Times New Roman");
            workSheet.Cells[2, 1, 2, 16].Merge = true;
            workSheet.Cells["A1"].AutoFitColumns();

            List<TitleModel> itemInHouses = new List<TitleModel>()
            {
                new TitleModel { VNTitle = "Số hồ sơ", ENTitle =  "Document's No" },
                new TitleModel { VNTitle = "Năm đăng ký hồ sơ", ENTitle = "Document's Year" },
                new TitleModel { VNTitle = "Chức năng của chứng từ", ENTitle = "Document's function" },
                new TitleModel { VNTitle = "Cảng nhận hàng*", ENTitle = "Port of Loading" },
                new TitleModel { VNTitle = "Cảng trả hàng*", ENTitle = "Port of discharge" },
                new TitleModel { VNTitle = "Thông tin bổ sung", ENTitle = "Additional Remark" },
                new TitleModel { VNTitle = "Nơi ký", ENTitle = "Sign place" },
                new TitleModel { VNTitle = "Ngày ký", ENTitle = "Sign date" },
                new TitleModel { VNTitle = "Người ký", ENTitle = "Master signed"}
            };
            int addressStartContent = 3;
            WriteGeneralDangerousGoodsInfo(workSheet, transactionDetail, addressStartContent);
            List<TitleModel> goodsInfos = new List<TitleModel>() {
                new TitleModel { VNTitle = "Số vận đơn*", ENTitle = "Booking/reference number" },
                new TitleModel { VNTitle = "Kí hiệu container*", ENTitle = "Marks" },
                new TitleModel { VNTitle = "Số bao kiện*", ENTitle = "Number package" },
                new TitleModel { VNTitle = "Loại bao kiện*", ENTitle = "Kind of packages" },
                new TitleModel { VNTitle = "Cty vận chuyển*", ENTitle = "Transporter's name" },
                new TitleModel { VNTitle = "Loại hàng hóa*", ENTitle = "Class"},
                new TitleModel { VNTitle = "Số UN*", ENTitle = "UN number" },
                new TitleModel { VNTitle = "Nhóm hàng*", ENTitle = "Packing group" },
                new TitleModel { VNTitle = "Nhóm phụ số*", ENTitle = "Subsidiary risk(s)" },
                new TitleModel { VNTitle = "Điểm bốc cháy*", ENTitle = "Flash point (In oC, c.c" },
                new TitleModel { VNTitle = "Ô nhiễm biển*", ENTitle = "Marine pollutant" },
                new TitleModel { VNTitle = "Tổng khối lượng*", ENTitle = "Mass (kg) Gross/Net" },
                new TitleModel { VNTitle = "Vị trí xếp hàng*", ENTitle = "Stowage position on board" },
                new TitleModel { VNTitle = "Số hiệu container*", ENTitle = "Container number" },
                new TitleModel { VNTitle = "Số seal container*", ENTitle = "Container seal number" },
                new TitleModel { VNTitle = "Số container*", ENTitle = "Number Container" }
            };
            for (int i = 0; i < goodsInfos.Count; i++)
            {
                workSheet.Cells[13, i + 1].Style.WrapText = true;
                BorderThinItem(workSheet, 13, i + 1);
                workSheet.Cells[13, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                workSheet.Cells[13, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[13, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //workSheet.Cells[13, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#00b0f0"));
                workSheet.Cells[13, i + 1].Style.Font.SetFromFont(new Font("Times New Roman", 12));

                ExcelRichText ert = workSheet.Cells[13, i + 1].RichText.Add(goodsInfos[i].VNTitle);
                ert.Color = System.Drawing.Color.Red;
                ert = workSheet.Cells[13, i + 1].RichText.Add(" \n" + goodsInfos[i].ENTitle);
                ert.Color = System.Drawing.Color.Black;
            }
            addressStartContent = 14;
            if (transactionDetail.CsMawbcontainers.Count > 0)
            {
                foreach (var item in transactionDetail.CsMawbcontainers)
                {
                    workSheet.Cells[addressStartContent, 1].Value = transactionDetail.Hwbno;
                    workSheet.Cells[addressStartContent, 2].Value = item.MarkNo;
                    workSheet.Cells[addressStartContent, 3].Value = item.PackageQuantity;
                    workSheet.Cells[addressStartContent, 4].Value = item.PackageTypeName;
                    workSheet.Cells[addressStartContent, 5].Value = transactionDetail.ShipperDescription;
                    workSheet.Cells[addressStartContent, 6].Value = string.Empty;
                    workSheet.Cells[addressStartContent, 7].Value = string.Empty;
                    workSheet.Cells[addressStartContent, 8].Value = string.Empty;
                    workSheet.Cells[addressStartContent, 9].Value = string.Empty;
                    workSheet.Cells[addressStartContent, 10].Value = string.Empty;
                    workSheet.Cells[addressStartContent, 11].Value = string.Empty;
                    workSheet.Cells[addressStartContent, 12].Value = (item.Gw != null && item.Nw != null) ? (item.Gw / item.Nw) : null;
                    workSheet.Cells[addressStartContent, 13].Value = string.Empty;
                    workSheet.Cells[addressStartContent, 14].Value = item.ContainerNo;
                    workSheet.Cells[addressStartContent, 15].Value = item.SealNo;
                    workSheet.Cells[addressStartContent, 16].Value = item.Quantity;

                    for (int i = 0; i < goodsInfos.Count; i++)
                    {
                        BorderThinItem(workSheet, addressStartContent, i + 1);
                        workSheet.Cells[addressStartContent, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    addressStartContent = addressStartContent + 1;
                }
            }
        }

        internal Stream CreateGoodsDeclare(CsTransactionDetailModel transactionDetail, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    GenerateGoodsDeclare(workSheet, transactionDetail);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void GenerateGoodsDeclare(ExcelWorksheet workSheet, CsTransactionDetailModel transactionDetail)
        {
            List<String> headers = new List<String>()
            {
                "BẢNG KHAI HÀNG HÓA",
                "Goods Declaration"
            };
            Color colTitleFromHex = System.Drawing.ColorTranslator.FromHtml("#00b0f0");
            Color colSubTitleFromHex = System.Drawing.ColorTranslator.FromHtml("#d9e1f2");
            workSheet.Cells["A1"].Value = headers[0];
            FormatTitleHeader(workSheet, "A1", "Times New Roman");
            workSheet.Cells[1, 1, 1, 13].Merge = true;
            workSheet.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(colTitleFromHex);
            workSheet.Cells["A1"].AutoFitColumns();

            workSheet.Cells["A2"].Value = headers[1];
            FormatTitleHeader(workSheet, "A2", "Times New Roman");
            workSheet.Cells[2, 1, 2, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[2, 1, 2, 13].Style.Fill.BackgroundColor.SetColor(colTitleFromHex);
            workSheet.Cells[2, 1, 2, 13].Merge = true;
            workSheet.Cells["A2"].Style.WrapText = true;
            List<TitleModel> itemInHouses = new List<TitleModel>()
            {
                new TitleModel { VNTitle = "Số hồ sơ", ENTitle = "\nDocument's No" },
                new TitleModel { VNTitle = "Năm đăng ký hồ sơ", ENTitle = "Document's Year" },
                new TitleModel { VNTitle = "Chức năng của chứng từ", ENTitle = "Document's function" },
                new TitleModel { VNTitle = "Tổng số kiện*", ENTitle = "Number of packages" },
                new TitleModel { VNTitle = "Loại kiện*", ENTitle = "Kind of packages"}
            };
            int addressStartContent = 3;
            for (int i = 0; i < itemInHouses.Count; i++)
            {
                workSheet.Column(1).Width = 25;
                workSheet.Cells[i + addressStartContent, 1].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                workSheet.Cells[i + addressStartContent, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[i + addressStartContent, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));
                workSheet.Cells[i + addressStartContent, 1].IsRichText = true;
                workSheet.Cells[i + addressStartContent, 1].Style.Font.Color.SetColor(Color.Red);
                ExcelRichText ert = workSheet.Cells[i + addressStartContent, 1].RichText.Add(itemInHouses[i].VNTitle);
                ert.Color = System.Drawing.Color.Red;
                ert = workSheet.Cells[i + addressStartContent, 1].RichText.Add(" \n" + itemInHouses[i].ENTitle);
                ert.Color = System.Drawing.Color.Black;
                workSheet.Cells[i + addressStartContent, 1].Style.WrapText = true;
                BorderThinItem(workSheet, i + addressStartContent, 1);

                workSheet.Cells[i + addressStartContent, 2].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                Color coVlauelFromHex = System.Drawing.ColorTranslator.FromHtml("#ffff00");
                if (i == 2 || i == 4)
                {
                    coVlauelFromHex = System.Drawing.ColorTranslator.FromHtml("#ffd966");
                }
                else
                {
                    if (i == 1)
                    {
                        workSheet.Cells[i + addressStartContent, 2].Value = DateTime.Now.Year;
                    }
                }
                workSheet.Cells[i + addressStartContent, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[i + addressStartContent, 2].Style.Fill.BackgroundColor.SetColor(coVlauelFromHex);
                BorderThinItem(workSheet, i + addressStartContent, 2);
            }
            workSheet.Cells["A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A8"].Value = headers[1];
            FormatTitleHeader(workSheet, "A8", "Times New Roman");
            workSheet.Cells[8, 1, 8, 23].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[8, 1, 8, 23].Style.Fill.BackgroundColor.SetColor(colTitleFromHex);
            workSheet.Cells[8, 1, 8, 23].Merge = true;
            workSheet.Cells["A8"].Value = "THÔNG TIN HÀNG HÓA";
            List<TitleModel> goodsInfos = new List<TitleModel>()
            {
                new TitleModel { VNTitle = "Vận đơn số*", ENTitle = "B/L No" },
                new TitleModel { VNTitle = "Người gửi hàng*", ENTitle = "Shipper/Consignor" },
                new TitleModel { VNTitle = "Người nhận hàng*", ENTitle = "Consignee" },
                new TitleModel { VNTitle = "Người được thông báo*", ENTitle = "Notify Party"},
                new TitleModel { VNTitle = "Người được thông báo 2", ENTitle = "Notify Party 2" },
                new TitleModel { VNTitle = "Số hiệu cont", ENTitle = "Cont's number"},
                new TitleModel { VNTitle = "Số Seal cont", ENTitle = "Seal number"},
                new TitleModel { VNTitle = "Mã hàng (nếu có)", ENTitle = "HS code If avail."},
                new TitleModel { VNTitle = "Tên hàng/mô tả hàng hóa*", ENTitle = "Name, Description of goods"},
                new TitleModel { VNTitle = "Trọng lượng tịnh*", ENTitle = "Net weight"},
                new TitleModel { VNTitle = "Tổng trọng lượng*", ENTitle = "Gross weight"},
                new TitleModel { VNTitle = "Kích thước/thể tích*", ENTitle = "Demension /tonnage"},
                new TitleModel { VNTitle = "Số tham chiếu manifest", ENTitle = "Ref. no manifest"},
                new TitleModel { VNTitle = "Căn cứ hiệu chỉnh", ENTitle = "Ajustment basis"},
                new TitleModel { VNTitle = "Đơn vị tính trọng lượng*", ENTitle = "GrossUnit"},
                new TitleModel { VNTitle = "Cảng dỡ hàng*", ENTitle = "Port Of Discharge"},
                new TitleModel { VNTitle = "Cảng đích*", ENTitle = "Port Of Destination"},
                new TitleModel { VNTitle = "Cảng xếp hàng*", ENTitle =  "Port Of Loading"},
                new TitleModel { VNTitle = "Cảng xếp hàng gốc*", ENTitle = "Port Of OrginalLoading"},
                new TitleModel { VNTitle = "Cảng trung chuyển*", ENTitle = "Port of Transhipment"},
                new TitleModel { VNTitle = "Cảng giao hàng/cảng đích*", ENTitle = "Final Destination"},
                new TitleModel { VNTitle = "Loại cont*", ENTitle = "Cont. type"},
                new TitleModel { VNTitle = "Đơn vị thể tích", ENTitle = "Dimension of unit"}
            };
            addressStartContent = 9;
            int k = 0;
            for (int i = 0; i < goodsInfos.Count; i++)
            {
                workSheet.Cells[addressStartContent, i + 1].Style.WrapText = true;
                workSheet.Cells[addressStartContent, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[addressStartContent, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[addressStartContent, i + 1].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                workSheet.Cells[addressStartContent, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[addressStartContent, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));
                BorderDashItem(workSheet, addressStartContent, i + 1);

                ExcelRichText ert = workSheet.Cells[addressStartContent, i + 1].RichText.Add(goodsInfos[i].VNTitle);
                ert.Color = System.Drawing.Color.Red;
                ert = workSheet.Cells[addressStartContent, i + 1].RichText.Add(goodsInfos[i].ENTitle);
                ert.Color = System.Drawing.Color.Black;
                workSheet.Column(i + 1).Width = 25;
            }
            int totalColums = goodsInfos.Count + 1;
            if (transactionDetail.CsMawbcontainers != null)
            {
                WriteGoodsInfoDataTable(workSheet, transactionDetail, totalColums);
            }
        }

        private void WriteGoodsInfoDataTable(ExcelWorksheet workSheet, CsTransactionDetailModel transactionDetail, int totalColums)
        {
            int addressStartContent = 10;
            short? numberPackage = 0;
            string kindOfPackages = string.Empty;
            foreach (var item in transactionDetail.CsMawbcontainers)
            {
                numberPackage = (short?)(numberPackage + item.PackageQuantity);
                kindOfPackages = item.PackageTypeName != null ? (kindOfPackages + item.PackageTypeName + "; ") : string.Empty;
                workSheet.Cells[addressStartContent, 1].Value = transactionDetail.Hwbno;
                workSheet.Cells[addressStartContent, 2].Value = transactionDetail.ShipperDescription;
                workSheet.Cells[addressStartContent, 3].Value = transactionDetail.ConsigneeDescription;
                workSheet.Cells[addressStartContent, 4].Value = transactionDetail.NotifyPartyDescription;
                workSheet.Cells[addressStartContent, 5].Value = transactionDetail.AlsoNotifyPartyDescription;
                workSheet.Cells[addressStartContent, 6].Value = item.ContainerNo;
                workSheet.Cells[addressStartContent, 7].Value = item.SealNo;
                workSheet.Cells[addressStartContent, 8].Value = item.CommodityName;
                workSheet.Cells[addressStartContent, 9].Value = item.Description;
                workSheet.Cells[addressStartContent, 10].Value = item.Nw;
                workSheet.Cells[addressStartContent, 11].Value = item.Gw;
                workSheet.Cells[addressStartContent, 12].Value = item.Cbm;
                workSheet.Cells[addressStartContent, 13].Value = transactionDetail.ManifestRefNo;
                workSheet.Cells[addressStartContent, 14].Value = string.Empty;
                workSheet.Cells[addressStartContent, 15].Value = item.UnitOfMeasureName;
                workSheet.Cells[addressStartContent, 16].Value = transactionDetail.PODName;
                workSheet.Cells[addressStartContent, 17].Value = transactionDetail.PODName;
                workSheet.Cells[addressStartContent, 18].Value = transactionDetail.POLName;
                workSheet.Cells[addressStartContent, 19].Value = transactionDetail.POLName;
                workSheet.Cells[addressStartContent, 20].Value = string.Empty;
                workSheet.Cells[addressStartContent, 21].Value = transactionDetail.FinalDestinationPlace;
                workSheet.Cells[addressStartContent, 22].Value = item.ContainerTypeName;
                workSheet.Cells[addressStartContent, 23].Value = item.PackageTypeName;
                Color coVlauelFromHex = System.Drawing.ColorTranslator.FromHtml("#ffff00");
                workSheet.Cells[addressStartContent, 1, addressStartContent, 23].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[addressStartContent, 1, addressStartContent, 23].Style.Fill.BackgroundColor.SetColor(coVlauelFromHex);
                addressStartContent = addressStartContent + 1;
            }
            for (int j = 10; j < addressStartContent; j++)
            {
                for (int i = 1; i < (totalColums + 1) / 2; i++)
                {
                    BorderDashItem(workSheet, j, i);
                    workSheet.Cells[j, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
            }
            workSheet.Cells[6, 2].Value = numberPackage;
            workSheet.Cells[7, 2].Value = kindOfPackages;
        }

        private void WriteGeneralDangerousGoodsInfo(ExcelWorksheet workSheet, CsTransactionDetailModel transactionDetail, int addressStartContent)
        {
            List<TitleModel> itemInHouses = new List<TitleModel>()
            {
                new TitleModel { VNTitle = "Số hồ sơ", ENTitle =  "Document's No" },
                new TitleModel { VNTitle = "Năm đăng ký hồ sơ", ENTitle = "Document's Year" },
                new TitleModel { VNTitle = "Chức năng của chứng từ", ENTitle = "Document's function" },
                new TitleModel { VNTitle = "Cảng nhận hàng*", ENTitle = "Port of Loading" },
                new TitleModel { VNTitle = "Cảng trả hàng*", ENTitle = "Port of discharge" },
                new TitleModel { VNTitle = "Thông tin bổ sung", ENTitle = "Additional Remark" },
                new TitleModel { VNTitle = "Nơi ký", ENTitle = "Sign place" },
                new TitleModel { VNTitle = "Ngày ký", ENTitle = "Sign date" },
                new TitleModel { VNTitle = "Người ký", ENTitle = "Master signed"}
            };
            for (int i = 0; i < itemInHouses.Count; i++)
            {
                var item = itemInHouses[i];
                workSheet.Cells[i + addressStartContent, 1].Style.WrapText = true;
                BorderThinItem(workSheet, i + addressStartContent, 1);
                //workSheet.Cells[i + addressStartContent, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //workSheet.Cells[i + addressStartContent, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));
                workSheet.Column(1).Width = 32.18;
                workSheet.Cells[i + addressStartContent, 1].Style.Font.SetFromFont(new Font("Times New Roman", 12));

                ExcelRichText ert = workSheet.Cells[i + addressStartContent, 1].RichText.Add(itemInHouses[i].VNTitle);
                ert.Color = System.Drawing.Color.Red;
                ert = workSheet.Cells[i + addressStartContent, 1].RichText.Add(" \n" + itemInHouses[i].ENTitle);
                ert.Color = System.Drawing.Color.Black;

                workSheet.Cells[i + addressStartContent, 2].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                if (i == 1)
                {
                    workSheet.Cells[i + addressStartContent, 2].Value = DateTime.Now.Year;
                }
                if (i == 3)
                {
                    workSheet.Cells[i + addressStartContent, 2].Value = transactionDetail.POLName;
                }
                if (i == 6)
                {
                    workSheet.Cells[i + addressStartContent, 2].Value = transactionDetail.PODName;
                }
                workSheet.Column(2).Width = 32.18;
                workSheet.Cells[i + addressStartContent, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[i + addressStartContent, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#ffff00"));

                workSheet.Cells[i + addressStartContent, 2].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[i + addressStartContent, 2].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[i + addressStartContent, 2].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[i + addressStartContent, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
        }

        private void BorderThinItem(ExcelWorksheet workSheet, int row, int column)
        {
            workSheet.Cells[row, column].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row, column].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row, column].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row, column].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[row, column].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }
        private void BorderDashItem(ExcelWorksheet workSheet, int row, int column)
        {
            workSheet.Cells[row, column].Style.Border.Top.Style = ExcelBorderStyle.Dashed;
            workSheet.Cells[row, column].Style.Border.Left.Style = ExcelBorderStyle.Dashed;
            workSheet.Cells[row, column].Style.Border.Right.Style = ExcelBorderStyle.Dashed;
            workSheet.Cells[row, column].Style.Border.Bottom.Style = ExcelBorderStyle.Dashed;
        }


        private void FormatTitleHeader(ExcelWorksheet workSheet, string address, string fontFamily)
        {
            workSheet.Cells[address].Style.Font.SetFromFont(new Font(fontFamily, 12));
            workSheet.Cells[address].Style.Font.Bold = true;
            workSheet.Cells[address].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[address].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[address].Style.WrapText = true;
        }

        #region --- MAWB and HAWB Air Export Excel ---
        /// <summary>
        /// Generate MAWB Air Export Excel
        /// </summary>
        /// <param name="airwayBillExport"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateMAWBAirExportExcel(AirwayBillExportResult airwayBillExport, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("MAWB");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataMAWBAirExportExcel(workSheet, airwayBillExport);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void SetWidthColumnExcelMAWBAirExport(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 5.29 + 0.72; //Cột A
            workSheet.Column(2).Width = 6.29 + 0.72; //Cột B
            workSheet.Column(3).Width = 5.43 + 0.72; //Cột C
            workSheet.Column(4).Width = 3.86 + 0.72; //Cột D
            workSheet.Column(5).Width = 8.86 + 0.72; //Cột E
            workSheet.Column(6).Width = 5.57 + 0.72; //Cột F
            workSheet.Column(7).Width = 3.86 + 0.72; //Cột G
            workSheet.Column(8).Width = 6.29 + 0.72; //Cột H
            workSheet.Column(9).Width = 4 + 0.72; //Cột I
            workSheet.Column(10).Width = 9.57 + 0.72; //Cột J
            workSheet.Column(11).Width = 6.71 + 0.72; //Cột K
            workSheet.Column(12).Width = 5.43 + 0.72; //Cột L
            workSheet.Column(13).Width = 15.86 + 0.72; //Cột M
            workSheet.Column(14).Width = 4.14 + 0.72; //Cột N
        }

        private void BindingDataMAWBAirExportExcel(ExcelWorksheet workSheet, AirwayBillExportResult airwayBillExport)
        {
            workSheet.View.ShowGridLines = false;

            SetWidthColumnExcelMAWBAirExport(workSheet);
            workSheet.Cells[1, 1, 100000, 14].Style.Font.SetFromFont(new Font("Arial", 10));

            var _mawb1 = airwayBillExport.MawbNo1?.ToUpper(); //3 ký tự đầu của MAWB
            var _mawb3 = airwayBillExport.MawbNo3?.ToUpper(); //9 ký tự cuối của MAWB (Box 3)
            workSheet.Cells["A1:N1"].Style.Font.SetFromFont(new Font("Arial", 12));
            workSheet.Cells["A1:N1"].Style.Font.Bold = true;
            workSheet.Cells["A1"].Value = _mawb1; //3 ký tự đầu của MAWB
            workSheet.Cells["B1"].Value = airwayBillExport.AolCode?.ToUpper(); //Mã cảng đi
            workSheet.Cells["C1:E1"].Merge = true;
            workSheet.Cells["C1"].Value = _mawb3; //Các ký tự cuối của MAWB
            workSheet.Cells["L1"].Value = _mawb1 + "-" + _mawb3; //3 ký tự đầu của MAWB

            workSheet.Cells["A3:A12"].Style.Font.Color.SetColor(Color.DarkBlue);

            workSheet.Cells["A3:H6"].Style.Font.Bold = true;
            workSheet.Cells["A3:H6"].Merge = true;
            workSheet.Cells["A3:H6"].Style.WrapText = true;
            workSheet.Cells["A3:H6"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["A3"].Value = airwayBillExport.Shipper?.ToUpper(); //Thông tin Shipper

            workSheet.Cells["K4:N5"].Merge = true;
            workSheet.Cells["K4"].Style.Font.SetFromFont(new Font("Arial", 12));
            workSheet.Cells["K4"].Value = airwayBillExport.AirlineNameEn?.ToUpper(); //Airline
            workSheet.Cells["K4"].Style.Font.Bold = true;
            workSheet.Cells["K4:N5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["K4:N5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A9:H12"].Style.Font.Bold = true;
            workSheet.Cells["A9:H12"].Merge = true;
            workSheet.Cells["A9:H12"].Style.WrapText = true;
            workSheet.Cells["A9:H12"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["A9"].Value = airwayBillExport.Consignee?.ToUpper(); //Thông tin consignee            

            workSheet.Cells["J17:M17"].Merge = true;
            workSheet.Cells["J17"].Value = airwayBillExport.AirFrieghtDa?.ToUpper();

            workSheet.Cells["A19"].Style.Font.SetFromFont(new Font("Calibri", 12));
            workSheet.Cells["A19"].Style.Font.Bold = true;
            workSheet.Cells["A19"].Value = "373-0118"; //Default

            workSheet.Cells["A21"].Value = airwayBillExport.DepartureAirport?.ToUpper(); //Tên cảng đi
            workSheet.Cells["A21"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            //workSheet.Row(21).Height = 24.75;

            workSheet.Cells["A22"].Value = airwayBillExport.FirstTo?.ToUpper();
            workSheet.Cells["B22"].Value = airwayBillExport.FirstCarrier?.ToUpper();
            workSheet.Cells["F22:G22"].Merge = true;
            workSheet.Cells["F22"].Value = (airwayBillExport.SecondTo + " " + airwayBillExport.SecondBy)?.ToUpper();
            workSheet.Cells["I22"].Value = airwayBillExport.Currency?.ToUpper();
            workSheet.Cells["L22"].Value = airwayBillExport.Dclrca?.ToUpper();
            workSheet.Cells["M22"].Value = airwayBillExport.Dclrcus?.ToUpper();
            workSheet.Cells["M22"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            workSheet.Cells["A24"].Value = airwayBillExport.DestinationAirport?.ToUpper(); //Tên cảng đến
            workSheet.Cells["E24"].Value = airwayBillExport.FlightNo?.ToUpper(); //Tên chuyến bay
            workSheet.Cells["F24:H24"].Merge = true;
            workSheet.Cells["F24:H24"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["F24"].Value = airwayBillExport.FlightDate; //Ngày bay
            workSheet.Cells["F24"].Style.Numberformat.Format = "dd-MMM-yyyy";
            workSheet.Cells["I24"].Value = airwayBillExport.IssuranceAmount?.ToUpper();

            // workSheet.Cells["A27"].Style.Font.Color.SetColor(Color.Red);
            // workSheet.Cells["A27"].Style.Font.Bold = true;
            // workSheet.Cells["A27"].Value = 1;
            workSheet.Cells["B27"].Value = airwayBillExport.HandingInfo?.ToUpper(); //Handing Info

            workSheet.Cells["A30:I30"].Style.Font.Bold = true;
            workSheet.Cells["A30:E30"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["A30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["A30"].Value = airwayBillExport.Pieces;
            workSheet.Cells["A30"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["B30:C30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["B30:C30"].Merge = true;
            workSheet.Cells["B30"].Value = airwayBillExport.Gw;
            workSheet.Cells["B30"].Style.Numberformat.Format = numberFormatKgs;
            workSheet.Cells["E30:F30"].Merge = true;
            workSheet.Cells["E30"].Value = airwayBillExport.Cw;
            workSheet.Cells["E30"].Style.Numberformat.Format = numberFormatKgs;
            workSheet.Cells["G30:J30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["G30:H30"].Merge = true;
            if (airwayBillExport.RateCharge != null && airwayBillExport.RateCharge != 0)
            {
                workSheet.Cells["G30"].Value = airwayBillExport.RateCharge;
                workSheet.Cells["G30"].Style.Numberformat.Format = numberFormat;
            }
            workSheet.Cells["I30"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["I30:J30"].Merge = true;
            decimal _totalDefault = 0;
            if (decimal.TryParse(airwayBillExport.Total, out _totalDefault))
            {
                workSheet.Cells["I30"].Value = _totalDefault;
                workSheet.Cells["I30"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["I30"].Value = airwayBillExport.Total;
            }

            workSheet.Cells["A31"].Value = "PCS"; //Default
            workSheet.Cells["L31:N39"].Merge = true;
            workSheet.Cells["L31:N39"].Style.WrapText = true;
            workSheet.Cells["L31:N39"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["L31"].Value = (airwayBillExport.DesOfGood + "\r\n" + airwayBillExport.VolumeField)?.ToUpper();

            workSheet.Cells["A40"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["A40"].Value = airwayBillExport.Pieces;
            workSheet.Cells["A40"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["B40"].Value = airwayBillExport.Gw;
            workSheet.Cells["B40"].Style.Numberformat.Format = numberFormatKgs;

            workSheet.Cells["A44:B44"].Merge = true;
            workSheet.Cells["A44:B44"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            decimal _prepaidWtDefault = 0;
            if (decimal.TryParse(airwayBillExport.PrepaidWt, out _prepaidWtDefault))
            {
                workSheet.Cells["A44"].Value = _prepaidWtDefault;
                workSheet.Cells["A44"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["A44"].Value = airwayBillExport.PrepaidWt?.ToUpper();
            }

            decimal _collectWtDefault = 0;
            if (decimal.TryParse(airwayBillExport.CollectWt, out _collectWtDefault))
            {
                workSheet.Cells["D44"].Value = _collectWtDefault;
                workSheet.Cells["D44"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["D44"].Value = airwayBillExport.CollectWt?.ToUpper();
            }

            workSheet.Cells["A46:B46"].Merge = true;
            workSheet.Cells["A46:B46"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            decimal _prepaidValDefault = 0;
            if (decimal.TryParse(airwayBillExport.PrepaidVal, out _prepaidValDefault))
            {
                workSheet.Cells["A46"].Value = _prepaidValDefault;
                workSheet.Cells["A46"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["A46"].Value = airwayBillExport.PrepaidVal?.ToUpper();
            }

            decimal _collectValDefault = 0;
            if (decimal.TryParse(airwayBillExport.CollectVal, out _collectValDefault))
            {
                workSheet.Cells["D46"].Value = _collectValDefault;
                workSheet.Cells["D46"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["D46"].Value = airwayBillExport.CollectVal?.ToUpper();
            }

            workSheet.Cells["A48:B48"].Merge = true;
            workSheet.Cells["A48:B48"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            decimal _prepaidTaxDefault = 0;
            if (decimal.TryParse(airwayBillExport.PrepaidTax, out _prepaidTaxDefault))
            {
                workSheet.Cells["A48"].Value = _prepaidTaxDefault;
                workSheet.Cells["A48"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["A48"].Value = airwayBillExport.PrepaidTax?.ToUpper();
            }
            decimal _collectTaxDefault = 0;
            if (decimal.TryParse(airwayBillExport.CollectTax, out _collectTaxDefault))
            {
                workSheet.Cells["D48"].Value = _collectTaxDefault;
                workSheet.Cells["D48"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["D48"].Value = airwayBillExport.CollectTax?.ToUpper();
            }


            workSheet.Cells["A51:B51"].Merge = true;
            workSheet.Cells["A51:B51"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            decimal _prepaidDueToCarrierDefault = 0;
            if (decimal.TryParse(airwayBillExport.PrepaidDueToCarrier, out _prepaidDueToCarrierDefault))
            {
                workSheet.Cells["A51"].Value = _prepaidDueToCarrierDefault;
                workSheet.Cells["A51"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["A51"].Value = airwayBillExport.PrepaidDueToCarrier?.ToUpper();
            }

            decimal _collectDueToCarrierDefault = 0;
            if (decimal.TryParse(airwayBillExport.CollectDueToCarrier, out _collectDueToCarrierDefault))
            {
                workSheet.Cells["D51"].Value = _collectDueToCarrierDefault;
                workSheet.Cells["D51"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["D51"].Value = airwayBillExport.CollectDueToCarrier?.ToUpper();
            }

            workSheet.Cells["A55:B55"].Merge = true;
            workSheet.Cells["A55"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            decimal _prepaidTotalDefault = 0;
            if (decimal.TryParse(airwayBillExport.PrepaidTotal, out _prepaidTotalDefault))
            {
                workSheet.Cells["A55"].Value = _prepaidTotalDefault;
                workSheet.Cells["A55"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["A55"].Value = airwayBillExport.PrepaidTotal?.ToUpper();
            }

            decimal _collectTotalDefault = 0;
            if (decimal.TryParse(airwayBillExport.CollectTotal, out _collectTotalDefault))
            {
                workSheet.Cells["D55"].Value = _collectTotalDefault;
                workSheet.Cells["D55"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["D55"].Value = airwayBillExport.CollectTotal?.ToUpper();
            }

            //Other Charges
            int k = 44;
            for (var i = 0; i < airwayBillExport.OtherCharges.Count; i++)
            {
                workSheet.Cells["H" + k].Value = airwayBillExport.OtherCharges[i].ChargeName?.ToUpper();
                // workSheet.Cells["J" + k].Value = airwayBillExport.OtherCharges[i].Amount;
                // workSheet.Cells["J" + k].Style.Numberformat.Format = numberFormat;
                k = k + 1;
            }

            if (airwayBillExport.OtherCharges.Count < 12)
            {
                k = 55;
            }

            workSheet.Cells["G" + k + ":L" + k].Merge = true;
            workSheet.Cells["G" + k].Value = airwayBillExport.IssueOn?.ToUpper() + " " + (airwayBillExport.IssueDate.HasValue ? airwayBillExport.IssueDate.Value.ToString("dd MMM yyyy").ToUpper() : string.Empty);
            workSheet.Cells["G" + k + ":L" + k].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //workSheet.Row(57).Height = 42.75;
            workSheet.Cells["L" + (k + 2) + ":M" + (k + 2)].Style.Font.Bold = true;
            workSheet.Cells["L" + (k + 2) + ":M" + (k + 2)].Style.Font.Size = 12;
            workSheet.Cells["L" + (k + 2)].Value = _mawb1 + "-" + _mawb3;
        }

        /// <summary>
        /// Generate HAWBW Air Export Excel
        /// </summary>
        /// <param name="airwayBillExport"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateHAWBAirExportExcel(AirwayBillExportResult airwayBillExport, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("HAWB");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataHAWBAirExportExcel(workSheet, airwayBillExport);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void SetWidthColumnExcelHAWBAirExport(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 4.29 + 0.72; //Cột A
            workSheet.Column(2).Width = 7.29 + 0.72; //Cột B
            workSheet.Column(3).Width = 5.43 + 0.72; //Cột C
            workSheet.Column(4).Width = 3.86 + 0.72; //Cột D
            workSheet.Column(5).Width = 8.86 + 0.72; //Cột E
            workSheet.Column(6).Width = 5.57 + 0.72; //Cột F
            workSheet.Column(7).Width = 3.86 + 0.72; //Cột G
            workSheet.Column(8).Width = 6.29 + 0.72; //Cột H
            workSheet.Column(9).Width = 4 + 0.72; //Cột I
            workSheet.Column(10).Width = 9.57 + 0.72; //Cột J
            workSheet.Column(11).Width = 6.71 + 0.72; //Cột K
            workSheet.Column(12).Width = 5.43 + 0.72; //Cột L
            workSheet.Column(13).Width = 15.86 + 0.72; //Cột M
            workSheet.Column(14).Width = 4.14 + 0.72; //Cột N
        }

        private void BindingDataHAWBAirExportExcel(ExcelWorksheet workSheet, AirwayBillExportResult airwayBillExport)
        {
            workSheet.View.ShowGridLines = false;

            SetWidthColumnExcelHAWBAirExport(workSheet);

            workSheet.Cells[1, 1, 100000, 14].Style.Font.SetFromFont(new Font("Arial", 10));

            var _mawb1 = airwayBillExport.MawbNo1?.Substring(0, 3).ToUpper(); //3 ký tự đầu của MAWB
            var _mawb2 = airwayBillExport.MawbNo3?.ToUpper(); //Các ký tự cuối của MAWB
            var _hawb1 = airwayBillExport.HawbNo.Substring(0, 3).ToUpper(); //3 ký tự đầu của HAWB
            var _hawb2 = airwayBillExport.HawbNo.Substring(3, airwayBillExport.HawbNo.Length - 3).ToUpper(); //Các ký tự cuối của HAWB

            workSheet.Cells["A1:N1"].Style.Font.SetFromFont(new Font("Arial", 12));
            workSheet.Cells["A1:N1"].Style.Font.Bold = true;
            workSheet.Cells["A1"].Value = _mawb1; //3 ký tự đầu của MAWB
            workSheet.Cells["B1"].Value = airwayBillExport.AolCode?.ToUpper(); //Mã cảng đi
            workSheet.Cells["C1:E1"].Merge = true;
            workSheet.Cells["C1"].Value = _mawb2; //Các ký tự cuối của MAWB
            workSheet.Cells["L1"].Value = _hawb1; //3 ký tự đầu của HAWB
            workSheet.Cells["M1"].Value = _hawb2; //Các ký tự cuối của HAWB

            workSheet.Cells["A3:A12"].Style.Font.Color.SetColor(Color.DarkBlue);

            workSheet.Cells["A3:H6"].Style.Font.Bold = true;
            workSheet.Cells["A3:H6"].Merge = true;
            workSheet.Cells["A3:H6"].Style.WrapText = true;
            workSheet.Cells["A3:H6"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["A3"].Value = airwayBillExport.Shipper?.ToUpper(); //Thông tin Shipper

            workSheet.Cells["K4:N5"].Merge = true;
            workSheet.Cells["K4"].Style.Font.SetFromFont(new Font("Arial", 12));
            workSheet.Cells["K4"].Value = airwayBillExport.OfficeUserCurrent?.ToUpper(); //Office của User Current
            workSheet.Cells["K4"].Style.Font.Bold = true;
            workSheet.Cells["K4:N5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["K4:N5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A9:H12"].Style.Font.Bold = true;
            workSheet.Cells["A9:H12"].Merge = true;
            workSheet.Cells["A9:H12"].Style.WrapText = true;
            workSheet.Cells["A9:H12"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["A9"].Value = airwayBillExport.Consignee?.ToUpper(); //Thông tin Consignee;

            workSheet.Cells["J17:M17"].Merge = true;
            workSheet.Cells["J17"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["J17"].Value = airwayBillExport.AirFrieghtDa?.ToUpper();

            workSheet.Cells["A19"].Style.Font.SetFromFont(new Font("Calibri", 12));
            workSheet.Cells["A19"].Style.Font.Bold = true;
            workSheet.Cells["A19"].Value = "373-0118"; //Default

            workSheet.Cells["A21"].Value = airwayBillExport.DepartureAirport?.ToUpper(); //Tên cảng đi
            workSheet.Cells["A21"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            //workSheet.Row(21).Height = 24.75;

            workSheet.Cells["A22"].Value = airwayBillExport.FirstTo?.ToUpper();
            workSheet.Cells["B22"].Value = airwayBillExport.FirstCarrier?.ToUpper();
            workSheet.Cells["F22:G22"].Merge = true;
            workSheet.Cells["F22"].Value = (airwayBillExport.SecondTo + " " + airwayBillExport.SecondBy)?.ToUpper();
            workSheet.Cells["I22"].Value = airwayBillExport.Currency?.ToUpper();
            workSheet.Cells["L22"].Value = airwayBillExport.Dclrca?.ToUpper();
            workSheet.Cells["M22"].Value = airwayBillExport.Dclrcus?.ToUpper();
            workSheet.Cells["M22"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            workSheet.Cells["A24"].Value = airwayBillExport.DestinationAirport?.ToUpper(); //Tên cảng đến
            workSheet.Cells["E24"].Value = airwayBillExport.FlightNo?.ToUpper(); //Tên chuyến bay
            workSheet.Cells["F24:H24"].Merge = true;
            workSheet.Cells["F24:H24"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["F24"].Value = airwayBillExport.FlightDate; //Ngày bay
            workSheet.Cells["F24"].Style.Numberformat.Format = "dd-MMM-yyyy";
            workSheet.Cells["I24"].Value = airwayBillExport.IssuranceAmount?.ToUpper();

            workSheet.Cells["A26"].Value = airwayBillExport.HandingInfo?.ToUpper();

            workSheet.Cells["A30:I30"].Style.Font.Bold = true;
            workSheet.Cells["A30:E30"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["A30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["A30"].Value = airwayBillExport.Pieces;
            workSheet.Cells["B30:C30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["B30:C30"].Merge = true;
            workSheet.Cells["B30"].Value = airwayBillExport.Gw;
            workSheet.Cells["B30"].Style.Numberformat.Format = numberFormatKgs;
            workSheet.Cells["E30:F30"].Merge = true;
            workSheet.Cells["E30"].Value = airwayBillExport.Cw;
            workSheet.Cells["E30"].Style.Numberformat.Format = numberFormatKgs;

            workSheet.Cells["G30:J30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["G30:H30"].Merge = true;
            if (airwayBillExport.RateCharge == null || airwayBillExport.RateCharge == 0)
            {
                workSheet.Cells["G30"].Value = "AS AGREED";
            }
            else
            {
                workSheet.Cells["G30"].Value = airwayBillExport.RateCharge;
                workSheet.Cells["G30"].Style.Numberformat.Format = numberFormat;
            }

            workSheet.Cells["I30"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["I30:J30"].Merge = true;
            decimal _totalDefault = 0;
            if (decimal.TryParse(airwayBillExport.Total, out _totalDefault))
            {
                workSheet.Cells["I30"].Value = _totalDefault;
                workSheet.Cells["I30"].Style.Numberformat.Format = numberFormat;
            }
            else
            {
                workSheet.Cells["I30"].Value = "AS AGREED";
            }

            workSheet.Cells["A31"].Value = "PCS"; //Default
            workSheet.Cells["L31:N39"].Merge = true;
            workSheet.Cells["L31:N39"].Style.WrapText = true;
            workSheet.Cells["L31:N39"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["L31"].Value = airwayBillExport.DesOfGood?.ToUpper() + "\r\n" + airwayBillExport.VolumeField?.ToUpper();

            workSheet.Cells["A40"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["A40"].Value = airwayBillExport.Pieces;
            workSheet.Cells["B40"].Value = airwayBillExport.Gw;
            workSheet.Cells["B40"].Style.Numberformat.Format = numberFormatKgs;

            workSheet.Cells["A44:B44"].Merge = true;
            workSheet.Cells["A44:B44"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A44"].Value = airwayBillExport.PrepaidTotal?.ToUpper(); //Total Prepaid

            //workSheet.Cells["A51:B51"].Merge = true;
            //workSheet.Cells["A51:B51"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            //workSheet.Cells["A51"].Value = string.Empty;

            workSheet.Cells["A55:B55"].Merge = true;
            workSheet.Cells["A55"].Value = airwayBillExport.CollectTotal?.ToUpper(); //Total Collect
            workSheet.Cells["A55"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["G55:L55"].Merge = true;
            workSheet.Cells["G55"].Value = airwayBillExport.IssueOn?.ToUpper() + " " + (airwayBillExport.IssueDate.HasValue ? airwayBillExport.IssueDate.Value.ToString("dd MMM yyyy").ToUpper() : string.Empty);
            workSheet.Cells["G55:L55"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //workSheet.Row(57).Height = 42.75;
            workSheet.Cells["L57:M57"].Style.Font.Bold = true;
            workSheet.Cells["L57:M57"].Style.Font.Size = 12;
            workSheet.Cells["L57"].Value = _hawb1; //3 ký tự đầu của HBL
            workSheet.Cells["M57"].Value = _hawb2; //Các ký tự còn lại của HBL
        }

        #endregion --- MAWB and HAWB Air Export Excel ---

        #region -- Export SCSC & TCS Air Export --
        /// <summary>
        /// Generate SCSC Air Export Excel
        /// </summary>
        /// <param name="airwayBillExport"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateSCSCAirExportExcel(AirwayBillExportResult airwayBillExport, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("SCSC");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataSCSCAirExportExcel(workSheet, airwayBillExport);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void BindingDataSCSCAirExportExcel(ExcelWorksheet workSheet, AirwayBillExportResult airwayBillExport)
        {
            workSheet.Cells[1, 1, 28, 14].Style.Font.Name = "MS Sans Serif";
            workSheet.Cells[1, 1, 28, 14].Style.Font.Size = 10;

            workSheet.Cells["B1:F4"].Merge = true;
            workSheet.Cells["B1:F4"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["B1"].Style.Font.Bold = true;
            workSheet.Cells["B1"].Style.WrapText = true;
            workSheet.Cells["B1"].Value = airwayBillExport.Shipper?.ToUpper();

            workSheet.Cells["B7"].Style.Font.Bold = true;
            workSheet.Cells["B7"].Value = "INDO TRANS LOGISTICS CORPORATION";

            workSheet.Cells["B8"].Value = "52-54-56 TRUONG SON STR., TAN BINH DIST,";
            workSheet.Cells["B9"].Value = "HOCHIMINH CITY, VIETNAM.";

            workSheet.Row(4).Height = 29.25;
            workSheet.Row(4).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["B4"].Value = "84 8 39486888";
            workSheet.Cells["D4"].Value = "84 8 8488593";

            workSheet.Cells["B14:F17"].Merge = true;
            workSheet.Cells["B14:F17"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["B14"].Style.Font.Bold = true;
            workSheet.Cells["B14"].Style.WrapText = true;
            workSheet.Cells["B14"].Value = airwayBillExport.Consignee?.ToUpper();

            workSheet.Cells["A27"].Value = "CONSOLIDATION AS PER ATTACHED MANIFEST. DOCS ATTD";
            workSheet.Cells["A27"].Style.Font.Size = 9;
            workSheet.Cells["A27:F27"].Merge = true;
            workSheet.Cells["A27:F27"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        /// <summary>
        /// Generate TCS Air Export Excel
        /// </summary>
        /// <param name="airwayBillExport"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateTCSAirExportExcel(AirwayBillExportResult airwayBillExport, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("TCS");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataTCSAirExportExcel(workSheet, airwayBillExport);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void BindingDataTCSAirExportExcel(ExcelWorksheet workSheet, AirwayBillExportResult airwayBillExport)
        {
            workSheet.Cells[1, 1, 19, 14].Style.Font.Name = "MS Sans Serif";
            workSheet.Cells[1, 1, 19, 14].Style.Font.Size = 10;
            workSheet.Cells[1, 1, 19, 14].Style.Font.Bold = true;

            workSheet.Cells["B2:F6"].Merge = true;
            workSheet.Cells["B2:F6"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["B2"].Style.WrapText = true;
            workSheet.Cells["B2"].Value = airwayBillExport.Shipper?.ToUpper();

            workSheet.Cells["B9"].Value = "INDO TRANS LOGISTICS (ITL) .MST: 0301909173";
            workSheet.Cells["B10"].Value = "52-54-56 TRUONG SON STR., TAN BINH DIST, HOCHIMINH CITY";
            workSheet.Cells["B11"].Value = "TEL: 84 8 8488567. FAX: 84 8 8488593";

            workSheet.Cells["C15:G19"].Merge = true;
            workSheet.Cells["C15:G19"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["C15"].Style.WrapText = true;
            workSheet.Cells["C15"].Value = airwayBillExport.Consignee?.ToUpper();

            workSheet.Cells["H15"].Value = "CONSOLIDATION AS PER ";
            workSheet.Cells["H16"].Value = "ATTACHED MANIFEST. DOCS ATTD";
            workSheet.Cells["H17"].Value = "MOULD";
            workSheet.Cells["H17"].Style.Font.Color.SetColor(Color.Blue);
        }
        #endregion -- Export SCSC & TCS Air Export --

        #region MONTHY REPORT
        /// <returns></returns>
        public Stream GenerateShipmentOverviewExcel(List<ExportShipmentOverview> overview, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Shipment Overview");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BinddingDataShipmentOverview(workSheet, overview, criteria);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BinddingDataShipmentOverview(ExcelWorksheet workSheet, List<ExportShipmentOverview> overview, GeneralReportCriteria criteria)
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
                "SERVICE MODE",
                "P/M TERM",
                "SHIPMENT NOTES",
                "CREATED"
            };

            List<string> subheadersTable = new List<string>()
            {
                "FREIGHT",
                "TRUCKING",
                "HANDLING FEE",
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
                if (i == 26)
                {
                    workSheet.Cells[9, i + 5].Value = headersTable[i];
                }
                if (i < 26)
                {
                    workSheet.Cells[9, i + 1].Value = headersTable[i];
                }
                if (i > 26)
                {
                    workSheet.Cells[9, i + 10].Value = headersTable[i];
                    workSheet.Cells[9, i + 10].Style.Font.Bold = true;
                }
                workSheet.Cells[9, i + 1].Style.Font.Bold = true;
                workSheet.Cells[9, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[9, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            }

            for (int i = 28; i < headersTable.Count; i++)
            {
                workSheet.Cells[9, i + 10].Value = headersTable[i];
                workSheet.Cells[9, i + 10].Style.Font.Bold = true;
                workSheet.Cells[9, i + 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[9, i + 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            //workSheet.Cells["AD9:AI9"].Merge = true;
            //workSheet.Cells["AA9:AE9"].Merge = true;
            //workSheet.Cells["AA9"].Value = headersTable[24];
            //workSheet.Cells["AD9"].Value = headersTable[27];

            workSheet.Cells["A9:A10"].Merge = true;
            workSheet.Cells["B9:B10"].Merge = true;
            workSheet.Cells["C9:C10"].Merge = true;
            workSheet.Cells["D9:D10"].Merge = true;
            workSheet.Cells["E9:E10"].Merge = true;
            workSheet.Cells["F9:F10"].Merge = true;

            workSheet.Cells["G9:G10"].Merge = true;
            workSheet.Cells["H9:H10"].Merge = true;
            workSheet.Cells["I9:I10"].Merge = true;
            workSheet.Cells["J9:J10"].Merge = true;
            workSheet.Cells["J9:J10"].Merge = true;
            workSheet.Cells["K9:K10"].Merge = true;


            workSheet.Cells["L9:L10"].Merge = true;
            workSheet.Cells["M9:M10"].Merge = true;
            workSheet.Cells["N9:N10"].Merge = true;
            workSheet.Cells["O9:O10"].Merge = true;
            workSheet.Cells["P9:P10"].Merge = true;
            workSheet.Cells["Q9:Q10"].Merge = true;

            workSheet.Cells["R9:R10"].Merge = true;
            workSheet.Cells["S9:S10"].Merge = true;
            workSheet.Cells["T9:T10"].Merge = true;
            workSheet.Cells["U9:U10"].Merge = true;
            workSheet.Cells["V9:V10"].Merge = true;
            workSheet.Cells["W9:W10"].Merge = true;
            workSheet.Cells["X9:X10"].Merge = true;
            workSheet.Cells["Y9:Y10"].Merge = true;
            workSheet.Cells["Z9:Z10"].Merge = true;

            //workSheet.Cells["AJ9:AJ10"].Merge = true;
            //workSheet.Cells["AK9:AK10"].Merge = true;
            //workSheet.Cells["AL9:AL10"].Merge = true;
            //workSheet.Cells["AM9:AM10"].Merge = true;
            //workSheet.Cells["AN9:AN10"].Merge = true;
            //workSheet.Cells["AO9:AO10"].Merge = true;
            //workSheet.Cells["AP9:AP10"].Merge = true;
            //workSheet.Cells["AQ9:AQ10"].Merge = true;
            //workSheet.Cells["AR9:AR10"].Merge = true;
            //workSheet.Cells["AS9:AS10"].Merge = true;
            //workSheet.Cells["AT9:AT10"].Merge = true;
            //workSheet.Cells["AU9:AU10"].Merge = true;
            //workSheet.Cells["AV9:AV10"].Merge = true;
            //workSheet.Cells["AW9:AW10"].Merge = true;
            //workSheet.Cells["AX9:AX10"].Merge = true;
            //workSheet.Cells["AY9:AY10"].Merge = true;
            //workSheet.Cells["AZ9:AZ10"].Merge = true;
            //workSheet.Cells["BA9:BA10"].Merge = true;
            //workSheet.Cells["BB9:BB10"].Merge = true;
            //workSheet.Cells["BC9:BC10"].Merge = true;
            //workSheet.Cells["BD9:BD10"].Merge = true;

            //workSheet.Cells["Y10"].Value = subheadersTable[0];
            //workSheet.Cells["Z10"].Value = subheadersTable[1];
            //workSheet.Cells["AA10"].Value = subheadersTable[2];
            //workSheet.Cells["AB10"].Value = subheadersTable[3];
            //workSheet.Cells["AC10"].Value = subheadersTable[4];

            workSheet.Cells["AA10"].Value = subheadersTable[0];
            workSheet.Cells["AB10"].Value = subheadersTable[1];
            workSheet.Cells["AC10"].Value = subheadersTable[2];
            workSheet.Cells["AD10"].Value = subheadersTable[3];
            workSheet.Cells["AE10"].Value = subheadersTable[4];
            //workSheet.Cells["AI10"].Value = subheadersTable[4];

            //workSheet.Cells["Y10:AI10"].Style.Font.Bold = true;

            workSheet.Cells["AW9:BD9"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["AW9:BD9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["AA10:AK10"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["AA10:AK10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["AA10:AK10"].Style.Font.Bold = true;

            workSheet.Cells["AF10"].Value = subheadersTable[0];
            workSheet.Cells["AG10"].Value = subheadersTable[1];
            workSheet.Cells["AH10"].Value = subheadersTable[2];
            workSheet.Cells["AI10"].Value = subheadersTable[5];
            workSheet.Cells["AJ10"].Value = subheadersTable[3];
            workSheet.Cells["AK10"].Value = subheadersTable[4];

            workSheet.Cells["AA9:AE9"].Merge = true;
            workSheet.Cells["AF9:AK9"].Merge = true;
            workSheet.Cells["AL9:AL10"].Merge = true;
            workSheet.Cells["AM9:AM10"].Merge = true;
            workSheet.Cells["AN9:AN10"].Merge = true;
            workSheet.Cells["AO9:AO10"].Merge = true;
            workSheet.Cells["AP9:AP10"].Merge = true;
            workSheet.Cells["AQ9:AQ10"].Merge = true;
            workSheet.Cells["AR9:AR10"].Merge = true;
            workSheet.Cells["AS9:AS10"].Merge = true;
            workSheet.Cells["AT9:AT10"].Merge = true;
            workSheet.Cells["AU9:AU10"].Merge = true;
            workSheet.Cells["AV9:AV10"].Merge = true;
            workSheet.Cells["AW9:AW10"].Merge = true;
            workSheet.Cells["AX9:AX10"].Merge = true;
            workSheet.Cells["AY9:AY10"].Merge = true;
            workSheet.Cells["AZ9:AZ10"].Merge = true;
            workSheet.Cells["BA9:BA10"].Merge = true;
            workSheet.Cells["BB9:BB10"].Merge = true;
            workSheet.Cells["BC9:BC10"].Merge = true;
            workSheet.Cells["BD9:BD10"].Merge = true;

            workSheet.Cells["AA9"].Value = headersTable[26];
            workSheet.Cells["AF9"].Value = headersTable[27];

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

                workSheet.Cells[j + addressStartContent, 6].Value = item.etd.HasValue ? item.etd.Value.ToString("dd/MM/yyyy") : "";
                workSheet.Cells[j + addressStartContent, 7].Value = item.eta.HasValue ? item.eta.Value.ToString("dd/MM/yyyy") : "";
                workSheet.Cells[j + addressStartContent, 8].Value = item.FlightNo;
                workSheet.Cells[j + addressStartContent, 9].Value = item.MblMawb;
                workSheet.Cells[j + addressStartContent, 10].Value = item.HblHawb;
                workSheet.Cells[j + addressStartContent, 11].Value = item.PolPod;
                workSheet.Cells[j + addressStartContent, 12].Value = item.Carrier;
                workSheet.Cells[j + addressStartContent, 13].Value = item.Agent;
                workSheet.Cells[j + addressStartContent, 14].Value = item.Shipper;
                workSheet.Cells[j + addressStartContent, 15].Value = item.Consignee;
                workSheet.Cells[j + addressStartContent, 16].Value = item.ShipmentType;
                workSheet.Cells[j + addressStartContent, 17].Value = item.Salesman;
                workSheet.Cells[j + addressStartContent, 18].Value = item.AgentName;
                workSheet.Cells[j + addressStartContent, 19].Value = item.QTy;
                workSheet.Cells[j + addressStartContent, 20].Value = item.Cont20;
                workSheet.Cells[j + addressStartContent, 21].Value = item.Cont40;
                workSheet.Cells[j + addressStartContent, 22].Value = item.Cont40HC;
                workSheet.Cells[j + addressStartContent, 23].Value = item.Cont45;
                workSheet.Cells[j + addressStartContent, 24].Value = item.GW;
                workSheet.Cells[j + addressStartContent, 25].Value = item.CW;
                workSheet.Cells[j + addressStartContent, 26].Value = item.CBM;
                workSheet.Cells[j + addressStartContent, 27].Value = item.TotalSellFreight;
                workSheet.Cells[j + addressStartContent, 27].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 28].Value = item.TotalSellTrucking;
                workSheet.Cells[j + addressStartContent, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 29].Value = item.TotalSellHandling;
                workSheet.Cells[j + addressStartContent, 29].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 30].Value = item.TotalSellOthers;
                workSheet.Cells[j + addressStartContent, 30].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 31].Value = item.TotalSell;
                workSheet.Cells[j + addressStartContent, 31].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 32].Value = item.TotalBuyFreight;
                workSheet.Cells[j + addressStartContent, 32].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 33].Value = item.TotalBuyTrucking;
                workSheet.Cells[j + addressStartContent, 33].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 34].Value = item.TotalBuyHandling;
                workSheet.Cells[j + addressStartContent, 34].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 35].Value = item.TotalBuyKB;
                workSheet.Cells[j + addressStartContent, 35].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 36].Value = item.TotalBuyOthers;
                workSheet.Cells[j + addressStartContent, 36].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 37].Value = item.TotalBuy;
                workSheet.Cells[j + addressStartContent, 37].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 38].Value = item.Profit;
                workSheet.Cells[j + addressStartContent, 38].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 39].Value = item.AmountOBH;
                workSheet.Cells[j + addressStartContent, 39].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 40].Value = item.AmountOBH;
                workSheet.Cells[j + addressStartContent, 40].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

                workSheet.Cells[j + addressStartContent, 41].Value = item.Destination;
                workSheet.Cells[j + addressStartContent, 42].Value = item.CustomerId;
                workSheet.Cells[j + addressStartContent, 43].Value = item.CustomerName;
                workSheet.Cells[j + addressStartContent, 44].Value = item.RalatedHblHawb;
                workSheet.Cells[j + addressStartContent, 45].Value = item.RalatedJobNo;
                workSheet.Cells[j + addressStartContent, 46].Value = item.HandleOffice;
                workSheet.Cells[j + addressStartContent, 47].Value = item.SalesOffice;
                workSheet.Cells[j + addressStartContent, 48].Value = item.Creator;
                workSheet.Cells[j + addressStartContent, 49].Value = item.POINV;
                workSheet.Cells[j + addressStartContent, 50].Value = item.BKRefNo;
                workSheet.Cells[j + addressStartContent, 51].Value = item.Commodity;
                workSheet.Cells[j + addressStartContent, 52].Value = item.ServiceMode;
                //workSheet.Cells[i + addressStartContent, 53].Value = item.ShipmentType;
                workSheet.Cells[j + addressStartContent, 53].Value = item.PMTerm;
                workSheet.Cells[j + addressStartContent, 54].Value = item.ShipmentNotes;
                workSheet.Cells[j + addressStartContent, 55].Value = item.Created.HasValue ? item.Created.Value.ToString("dd/MM/yyyy") : "";

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
             .Cells[address, 19].Start.Address;
            string addressToMerge = addressTotal + ":" + addressTotalMerge;
            workSheet.Cells[addressToMerge].Merge = true;
            workSheet.Cells[addressToMerge].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[addressToMerge].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells[addressToMerge].Style.Font.Bold = true;

            string addressTotalCont20 = workSheet.Cells[address, 20].Start.Address;
            workSheet.Cells[addressTotalCont20].Value = overview.Select(t => t.Cont20).Sum();
            string addressTotalCont40 = workSheet.Cells[address, 21].Start.Address;
            workSheet.Cells[addressTotalCont40].Value = overview.Select(t => t.Cont40).Sum();
            string addressTotalCont40HC = workSheet.Cells[address, 22].Start.Address;
            workSheet.Cells[addressTotalCont40HC].Value = overview.Select(t => t.Cont40HC).Sum();
            string addressTotalCont45 = workSheet.Cells[address, 23].Start.Address;
            workSheet.Cells[addressTotalCont45].Value = overview.Select(t => t.Cont45).Sum();
            string addressTotalGW = workSheet.Cells[address, 24].Start.Address;
            workSheet.Cells[addressTotalGW].Value = overview.Select(t => t.GW).Sum();
            string addressTotalCW = workSheet.Cells[address, 25].Start.Address;
            workSheet.Cells[addressTotalCW].Value = overview.Select(t => t.CW).Sum();
            string addressTotalCBM = workSheet.Cells[address, 26].Start.Address;
            workSheet.Cells[addressTotalCBM].Value = overview.Select(t => t.CBM).Sum();
            string addressTotalSellFreight = workSheet.Cells[address, 27].Start.Address;
            workSheet.Cells[addressTotalSellFreight].Value = overview.Select(t => t.TotalSellFreight).Sum();
            workSheet.Cells[addressTotalSellFreight].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalSellTrucking = workSheet.Cells[address, 28].Start.Address;
            workSheet.Cells[addressTotalSellTrucking].Value = overview.Select(t => t.TotalSellTrucking).Sum();
            workSheet.Cells[addressTotalSellTrucking].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalSellHandling = workSheet.Cells[address, 29].Start.Address;
            workSheet.Cells[addressTotalSellHandling].Value = overview.Select(t => t.TotalSellHandling).Sum();
            workSheet.Cells[addressTotalSellHandling].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalSellOther = workSheet.Cells[address, 30].Start.Address;
            workSheet.Cells[addressTotalSellOther].Value = overview.Select(t => t.TotalSellOthers).Sum();
            workSheet.Cells[addressTotalSellOther].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalSell = workSheet.Cells[address, 31].Start.Address;
            workSheet.Cells[addressTotalSell].Value = overview.Select(t => t.TotalSell).Sum();
            workSheet.Cells[addressTotalSell].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyFreight = workSheet.Cells[address, 32].Start.Address;
            workSheet.Cells[addressTotalBuyFreight].Value = overview.Select(t => t.TotalBuyFreight).Sum();
            workSheet.Cells[addressTotalBuyFreight].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyTrucking = workSheet.Cells[address, 33].Start.Address;
            workSheet.Cells[addressTotalBuyTrucking].Value = overview.Select(t => t.TotalBuyTrucking).Sum();
            workSheet.Cells[addressTotalBuyTrucking].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyHandling = workSheet.Cells[address, 34].Start.Address;
            workSheet.Cells[addressTotalBuyHandling].Value = overview.Select(t => t.TotalBuyHandling).Sum();
            workSheet.Cells[addressTotalBuyHandling].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyKB = workSheet.Cells[address, 35].Start.Address;
            workSheet.Cells[addressTotalBuyKB].Value = overview.Select(t => t.TotalBuyKB).Sum();
            workSheet.Cells[addressTotalBuyKB].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuyOthers = workSheet.Cells[address, 36].Start.Address;
            workSheet.Cells[addressTotalBuyOthers].Value = overview.Select(t => t.TotalBuyOthers).Sum();
            workSheet.Cells[addressTotalBuyOthers].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressTotalBuy = workSheet.Cells[address, 37].Start.Address;
            workSheet.Cells[addressTotalBuy].Value = overview.Select(t => t.TotalBuy).Sum();
            workSheet.Cells[addressTotalBuy].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressProfit = workSheet.Cells[address, 38].Start.Address;
            workSheet.Cells[addressProfit].Value = overview.Select(t => t.Profit).Sum();
            workSheet.Cells[addressProfit].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressAmountOBH = workSheet.Cells[address, 39].Start.Address;
            workSheet.Cells[addressAmountOBH].Value = overview.Select(t => t.AmountOBH).Sum();
            workSheet.Cells[addressAmountOBH].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressAmountOBHR = workSheet.Cells[address, 40].Start.Address;
            workSheet.Cells[addressAmountOBHR].Value = overview.Select(t => t.AmountOBH).Sum();
            workSheet.Cells[addressAmountOBHR].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            string addressToBold = addressTotalCont20 + ":" + addressAmountOBHR;
            workSheet.Cells[addressToBold].Style.Font.Bold = true;

            //workSheet.Column(36).Hidden = true;
            //workSheet.Column(37).Hidden = true;
            workSheet.Column(2).Width = 20;
            workSheet.Column(3).Width = 20;
            workSheet.Column(4).Width = 20;
            workSheet.Column(8).Width = 20;
            workSheet.Column(16).Width = 20;

            workSheet.Cells[9, 1, addressStartContent + positionStart + 1, 55].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[9, 1, addressStartContent + positionStart + 1, 55].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[9, 1, addressStartContent + positionStart + 1, 55].Style.Border.Top.Style = ExcelBorderStyle.Thin;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listData"></param>
        /// <param name="criteria"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateAccountingPLSheetExcel(List<AccountingPlSheetExport> listData, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Accounting PL Sheet (" + criteria.Currency + ")");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataAccountingPLSheetExportExcel(workSheet, listData, criteria);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

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
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listData"></param>
        /// <param name="criteria"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateJobProfitAnalysisExportExcel(List<JobProfitAnalysisExport> listData, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Accounting PL Sheet (" + criteria.Currency + ")");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataAJobProfitAnalysisExportExcel(workSheet, listData, criteria);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private void BindingDataAJobProfitAnalysisExportExcel(ExcelWorksheet workSheet, List<JobProfitAnalysisExport> listData, GeneralReportCriteria criteria)
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
        private void BindingDataAccountingPLSheetExportExcel(ExcelWorksheet workSheet, List<AccountingPlSheetExport> listData, GeneralReportCriteria criteria)
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
               "Amount", //28
               "Paid Date", //29
               "A/C Voucher No.", //30
               "P/M Voucher No.", //31
               "Service" //32
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
            workSheet.Cells["A7:AF8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A7:AF8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A7:AF8"].Style.Font.Bold = true;

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

            workSheet.Cells["AA7:AB7"].Merge = true;
            workSheet.Cells["AA7"].Value = headers[26]; //Payment on Behalf
            workSheet.Cells["AA8"].Value = headers[27]; //Inv.No
            workSheet.Cells["AB8"].Value = headers[28]; //Amount

            workSheet.Cells["AC7:AC8"].Merge = true;
            workSheet.Cells["AC7"].Value = headers[29]; //Paid Date

            workSheet.Cells["AD7:AD8"].Merge = true;
            workSheet.Cells["AD7"].Value = headers[30]; //A/C Voucher No.

            workSheet.Cells["AE7:AE8"].Merge = true;
            workSheet.Cells["AE7"].Value = headers[31]; //P/M Voucher No.

            workSheet.Cells["AF7:AF8"].Merge = true;
            workSheet.Cells["AF7"].Value = headers[32]; //Service
            //Header table

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
                    workSheet.Cells[rowStart, 26].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                workSheet.Cells[rowStart, 27].Value = listData[i].InvNoObh;

                if (listData[i].AmountObh != null && listData[i].AmountObh != 0)
                {
                    workSheet.Cells[rowStart, 28].Value = listData[i].AmountObh;
                    workSheet.Cells[rowStart, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
                }

                workSheet.Cells[rowStart, 29].Value = listData[i].PaidDate;
                workSheet.Cells[rowStart, 30].Value = listData[i].AcVoucherNo;
                workSheet.Cells[rowStart, 31].Value = listData[i].PmVoucherNo;
                workSheet.Cells[rowStart, 32].Value = listData[i].Service;

                rowStart += 1;

            }

            workSheet.Cells[rowStart, 1, rowStart, 32].Style.Font.Bold = true;
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
            workSheet.Cells[rowStart, 26].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;
            workSheet.Cells[rowStart, 28].Value = listData.Select(s => s.AmountObh).Sum(); // Sum Total Amount OBH
            workSheet.Cells[rowStart, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormats : numberFormatVND;

            workSheet.Cells[6, 1, 6, 32].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[7, 1, rowStart, 32].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[7, 1, rowStart, 32].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[rowStart + 2, 1, rowStart + 2, 32].Merge = true;
            workSheet.Cells[rowStart + 2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            workSheet.Cells[rowStart + 2, 1].Style.Font.Italic = true;
            workSheet.Cells[rowStart + 2, 1].Value = "Print date: " + DateTime.Now.ToString("dd MMM, yyyy HH:ss tt") + ", by: " + listData[0].UserExport;
        }

        /// <summary>
        /// Generate Standard General Report Excel
        /// </summary>
        /// <param name="listData"></param>
        /// <param name="criteria"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// 
        public Stream GenerateStandardGeneralReportExcel(List<GeneralReportResult> listData, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add(string.Format(@"Standard Report ({0})", criteria.Currency));
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BinddingDataStandardGeneralReport(workSheet, listData, criteria);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
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

        private void BinddingDataStandardGeneralReport(ExcelWorksheet workSheet, List<GeneralReportResult> listData, GeneralReportCriteria criteria)
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
                "Route", //8
                "Qty", //9
                "CW", //10
                "Revenue", //11
                "Cost", //12
                "Profit", //13
                "OBH", //14
                "P.I.C", //15
                "Salesman", //16
                "Service" //17
            };
            workSheet.Cells["A1:R1"].Style.Font.Bold = true;
            for (int c = 1; c < 19; c++)
            {
                workSheet.Cells[1, c].Value = headers[c - 1];
            }

            int startRow = 2;
            foreach (var item in listData)
            {
                workSheet.Cells[startRow, 1].Value = item.No;
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
                workSheet.Cells[startRow, 9].Value = item.Route;

                workSheet.Cells[startRow, 10].Value = item.Qty;
                workSheet.Cells[startRow, 11].Value = item.ChargeWeight;
                workSheet.Cells[startRow, 12].Value = item.Revenue;
                workSheet.Cells[startRow, 13].Value = item.Cost;
                workSheet.Cells[startRow, 14].Value = item.Profit;
                workSheet.Cells[startRow, 15].Value = item.Obh;

                workSheet.Cells[startRow, 10].Style.Numberformat.Format = numberFormatVND;

                if (criteria.Currency != "VND")
                {
                    for (int i = 11; i < 16; i++)
                    {
                        workSheet.Cells[startRow, i].Style.Numberformat.Format = numberFormatVND;
                    }
                }
                else
                {
                    for (int i = 11; i < 16; i++)
                    {
                        workSheet.Cells[startRow, i].Style.Numberformat.Format = numberFormats;
                    }
                }

                workSheet.Cells[startRow, 16].Value = item.PersonInCharge;
                workSheet.Cells[startRow, 17].Value = item.Salesman;
                workSheet.Cells[startRow, 18].Value = item.ServiceName;

                startRow += 1;
            }

            //Row total
            workSheet.Cells[startRow, 1].Value = "Total";
            workSheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(startRow).Style.Font.Bold = true;
            workSheet.Cells["A" + startRow + ":I" + startRow].Merge = true;
            workSheet.Cells[startRow, 10].Value = listData.Sum(su => su.Qty); //Total Qty
            workSheet.Cells[startRow, 10].Style.Numberformat.Format = numberFormatVND;
            workSheet.Cells[startRow, 11].Value = listData.Sum(su => su.ChargeWeight); //Total Charge Weight
            workSheet.Cells[startRow, 12].Value = listData.Sum(su => su.Revenue); //Total Revenue
            workSheet.Cells[startRow, 13].Value = listData.Sum(su => su.Cost); //Total Cost
            workSheet.Cells[startRow, 14].Value = listData.Sum(su => su.Profit); //Total Profit
            workSheet.Cells[startRow, 15].Value = listData.Sum(su => su.Obh); //Total OBH
            if (criteria.Currency != "VND")
            {
                for (int i = 11; i < 16; i++)
                {
                    workSheet.Cells[startRow, i].Style.Numberformat.Format = numberFormatVND;
                }
            }
            else
            {
                for (int i = 11; i < 16; i++)
                {
                    workSheet.Cells[startRow, i].Style.Numberformat.Format = numberFormats;
                }
            }

            workSheet.Cells[1, 1, startRow, 18].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[1, 1, startRow, 18].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[1, 1, startRow, 18].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[1, 1, startRow, 18].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
        #region Summary Cost
        public Stream GenerateSummaryOfCostsIncurredExcel(List<SummaryOfCostsIncurredModel> lst, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Summary Of Costs");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BinddingDatalSummaryOfCostsIncurred(workSheet, criteria, lst);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public void BinddingDatalSummaryOfCostsIncurred(ExcelWorksheet workSheet, GeneralReportCriteria criteria, List<SummaryOfCostsIncurredModel> lst)
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
        #endregion
        #region Export Summary Of Revenue

        public Stream GenerateSummaryOfRevenueExcel(SummaryOfRevenueModel obj, GeneralReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Summary Of Revenue");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BinddingDataSummaryOfRevenue(workSheet, criteria, obj);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
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

            int addressStartContent = 3;
            Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#eab286");
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
            #endregion
        }

        public Stream GenerateHousebillDailyExportExcel(List<HousebillDailyExportResult> housebillDailyExport, DateTime? issuedDate, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add(issuedDate.Value.ToString("dd MMM").ToUpper());
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataHousebillDailyExportExcel(workSheet, housebillDailyExport, issuedDate);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void SetWidthColumnExcelHousebillDailyExport(ExcelWorksheet workSheet)
        {
            workSheet.Column(1).Width = 5; //Cột A
            workSheet.Column(2).Width = 16; //Cột B
            workSheet.Column(3).Width = 16; //Cột C
            workSheet.Column(4).Width = 16; //Cột D
            workSheet.Column(5).Width = 11; //Cột E
            workSheet.Column(6).Width = 40; //Cột F
            workSheet.Column(7).Width = 9; //Cột G
            workSheet.Column(8).Width = 15; //Cột H
            workSheet.Column(9).Width = 30; //Cột I
            workSheet.Column(10).Width = 15; //Cột J
            workSheet.Column(11).Width = 15; //Cột K
        }

        private void BindingDataHousebillDailyExportExcel(ExcelWorksheet workSheet, List<HousebillDailyExportResult> housebillDailyExport, DateTime? issuedDate)
        {
            List<string> headerTable = new List<string>()
            {
                "STT",
                "MAWB",
                "HAWB",
                "FLIGHT",
                "DEST",
                "SHIPPER",
                "CTNS",
                "PO",
                "REMARK",
                "KHO",
                "PIC"
            };

            SetWidthColumnExcelHousebillDailyExport(workSheet);

            workSheet.Cells["B2:G2"].Merge = true;
            workSheet.Cells["B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["B2"].Value = "DALY LIST " + issuedDate.Value.ToString("dd MMM yyyy");
            workSheet.Cells["B2"].Style.Font.Bold = true;

            for (var c = 1; c < 12; c++)
            {
                //Set header
                workSheet.Cells[4, c].Value = headerTable[c - 1];
                //Set Background for header
                workSheet.Cells[4, c].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[4, c].Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                //Set border for row 2,3,4
                BorderThinItem(workSheet, 2, c);
                BorderThinItem(workSheet, 3, c);
                BorderThinItem(workSheet, 4, c);
            }

            workSheet.Cells["A4:K4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A4:K4"].Style.Font.Bold = true;

            int no = 1;
            int rowStart = 5;
            foreach (var item in housebillDailyExport)
            {
                workSheet.Cells[rowStart, 1].Value = no;
                workSheet.Cells[rowStart, 2].Value = item.Mawb;
                workSheet.Cells[rowStart, 3].Value = item.Hawb;
                workSheet.Cells[rowStart, 4].Value = item.FlightNo;
                workSheet.Cells[rowStart, 5].Value = item.PodCode;
                workSheet.Cells[rowStart, 6].Value = item.ShipperName;
                workSheet.Cells[rowStart, 7].Value = item.Pieces;
                workSheet.Cells[rowStart, 8].Value = item.Po;
                workSheet.Cells[rowStart, 9].Value = item.Remark;
                workSheet.Cells[rowStart, 10].Value = item.WarehouseName;
                workSheet.Cells[rowStart, 11].Value = item.PicName;
                for (var i = 1; i < 12; i++)
                {
                    BorderThinItem(workSheet, rowStart, i);
                }
                rowStart += 1;
                no += 1;
            }

            //In đậm & căn giữa value list 
            workSheet.Cells["A5:K" + (5 + housebillDailyExport.Count)].Style.Font.Bold = true;
            workSheet.Cells["A5:K" + (5 + housebillDailyExport.Count)].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        #region Export Credit Debit Note Detail
        // Generate Soa Supplier
        public Stream GenerateCDNoteDetailExcel(AcctCDNoteExportResult cdNoteModel, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Debit Note");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BinddingDataCDNoteDetailExport(workSheet, cdNoteModel);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BinddingDataCDNoteDetailExport(ExcelWorksheet workSheet, AcctCDNoteExportResult cdNoteModel)
        {
            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column B
                excelImage.SetPosition(0, 0, 1, 0);
            }

            List<string> titles = new List<string>()
            {
               "GIẤY ĐỀ NGHỊ THANH TOÁN / DEBIT NOTE", //0
               "Số: ", //1
               "Tên khách hàng", //2
               "Địa chỉ", //3
               "MST:", //4
               "Số tờ khai:", //5
               "Trọng lượng (KGS):", //6
               "CBM:", //7
               "Hợp đồng số:", //8
               "Vận đơn số", //9
               "Cont:", //10
               "Giao hàng về:" //11
            };

            List<string> headerTable = new List<string>()
            {
                "No", //0
                "Chi tiết / Description", //1
                "Hóa đơn/ \nInv No.", //2
                "Số tiền/ Amount (VND)", //3
                "Chi phí/ \nFee", //4
                "Thuế GTGT/ \nVAT", //5
                "Tổng cộng/ \nTotal", //6
                "Ghi chú/ \nNote" //7
            };

            // Custom With Column
            workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(2).Width = 10; //Cột B
            workSheet.Column(2).Width = 15; //Cột C
            workSheet.Column(6).Width = 10; //Cột F
            workSheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(7).Width = 20; //Cột G
            workSheet.Column(8).Width = 20; //Cột H
            workSheet.Column(9).Width = 20; //Cột I
            workSheet.Column(10).Width = 17; //Cột J

            // Title
            workSheet.Cells["A8:J8"].Merge = true;
            workSheet.Cells["A8"].Style.Font.SetFromFont(new Font("Arial", 16, FontStyle.Bold));
            workSheet.Cells["A8"].Value = titles[0];
            workSheet.Cells["A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["A9:J9"].Merge = true;
            workSheet.Cells["A9"].Style.Font.SetFromFont(new Font("Arial", 10));
            workSheet.Cells["A9"].Value = titles[1] + cdNoteModel.CDNo;
            // Tên KH
            workSheet.Cells["A10"].Value = titles[2];
            workSheet.Cells["C10"].Value = cdNoteModel.PartnerNameEn?.ToUpper();
            workSheet.Cells["C10"].Style.Font.Bold = true;
            // Địa chỉ
            workSheet.Cells["A11"].Value = titles[3];
            workSheet.Cells["C11"].Value = cdNoteModel.BillingAddress;
            workSheet.Cells["C11"].Style.Font.Bold = true;
            // MST
            workSheet.Cells["A12"].Value = titles[4];
            workSheet.Cells["C12"].Value = cdNoteModel.Taxcode?.ToUpper();
            // Số tờ khai
            workSheet.Cells["A13"].Value = titles[5];
            workSheet.Cells["C13"].Value = cdNoteModel.ClearanceNo?.ToUpper();
            // Trọng lượng
            workSheet.Cells["A14"].Value = titles[6];
            workSheet.Cells["C14"].Value = cdNoteModel.GW;
            workSheet.Cells["C14"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["C14"].Style.Numberformat.Format = numberFormat;
            // CMB
            workSheet.Cells["A15"].Value = titles[7];
            workSheet.Cells["C15"].Value = cdNoteModel.CBM;
            workSheet.Cells["C15"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["C15"].Style.Numberformat.Format = numberFormat;

            workSheet.Cells["A10:A15"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            // Soo61 HĐ
            workSheet.Cells["G13"].Value = titles[8];
            // Số vận đơn
            workSheet.Cells["G14"].Value = titles[9];
            workSheet.Cells["H14"].Value = cdNoteModel.HBL;
            // Cont
            workSheet.Cells["G15"].Value = titles[10];
            workSheet.Cells["H15"].Value = cdNoteModel.Cont;
            workSheet.Cells["I15"].Value = titles[11];
            workSheet.Cells["J15"].Value = cdNoteModel.WareHouseName?.ToUpper(); // Giao hàng về
            workSheet.Cells["A10:J15"].Style.Font.SetFromFont(new Font("Arial", 10));

            // header0
            workSheet.Cells["A17:A19"].Merge = true;
            workSheet.Cells["A17"].Value = headerTable[0];
            // header1
            workSheet.Cells["B17:E19"].Merge = true;
            workSheet.Cells["B17"].Value = headerTable[1];
            // header2
            workSheet.Cells["F17:F19"].Merge = true;
            workSheet.Cells["F17:F19"].Style.WrapText = true;
            workSheet.Cells["F17"].Value = headerTable[2];
            // header3
            workSheet.Cells["G17:I17"].Merge = true;
            workSheet.Cells["G17"].Value = headerTable[3];
            // header4
            workSheet.Cells["G18:G19"].Merge = true;
            workSheet.Cells["G18:G19"].Style.WrapText = true;
            workSheet.Cells["G18"].Value = headerTable[4];
            // header5
            workSheet.Cells["H18:H19"].Merge = true;
            workSheet.Cells["H18:H19"].Style.WrapText = true;
            workSheet.Cells["H18"].Value = headerTable[5];
            // header6
            workSheet.Cells["I18:I19"].Merge = true;
            workSheet.Cells["I18:I19"].Style.WrapText = true;
            workSheet.Cells["I18"].Value = headerTable[6];
            // header7
            workSheet.Cells["J17:J19"].Merge = true;
            workSheet.Cells["J17:J19"].Style.WrapText = true;
            workSheet.Cells["J17"].Value = headerTable[7];
            workSheet.Cells["A17:J19"].Style.Font.SetFromFont(new Font("Arial", 8, FontStyle.Bold));
            workSheet.Cells["A17:J19"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A17:J19"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["J17:J19"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells["J17:J19"].Style.Border.Top.Style = ExcelBorderStyle.Thin;

            // Detail of table
            // Chi hộ/ Pay on behalf of customer(Type=OBH)
            workSheet.Cells["A20"].Value = "I";
            workSheet.Cells["A20"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["B20:F20"].Merge = true;
            workSheet.Cells["B20"].Value = "Chi hộ/ Pay on behalf of customer";
            workSheet.Cells["A20:J20"].Style.Font.SetFromFont(new Font("Arial", 9, FontStyle.Bold));
            var surchargeOBHLst = cdNoteModel.ListCharges.Where(x => x.Type == "OBH").ToList();
            int addressStartGroup1 = 21;
            int countOBH = surchargeOBHLst.Count();
            int addressLastRow = addressStartGroup1 + countOBH - 1;
            if (countOBH > 0)
            {
                workSheet.Cells[addressStartGroup1, 1, addressStartGroup1 + countOBH, 10].Style.Font.SetFromFont(new Font("Arial", 8));
                workSheet.Cells[addressStartGroup1, 1, addressStartGroup1 + countOBH, 10].Style.WrapText = true;
                workSheet.Cells[addressStartGroup1, 1, addressStartGroup1 + countOBH, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                for (int i = 0; i < countOBH; i++)
                {
                    var item = surchargeOBHLst[i];
                    workSheet.Cells[i + addressStartGroup1, 1].Value = i + 1;
                    workSheet.Cells[i + addressStartGroup1, 2].Value = item.Description;
                    workSheet.Cells[i + addressStartGroup1, 2, i + addressStartGroup1, 5].Merge = true;
                    workSheet.Cells[i + addressStartGroup1, 6].Value = item.VATInvoiceNo;
                    workSheet.Cells[i + addressStartGroup1, 7].Value = item.Amount;
                    workSheet.Cells[i + addressStartGroup1, 7].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartGroup1, 8].Value = item.VATAmount;
                    workSheet.Cells[i + addressStartGroup1, 8].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartGroup1, 9].Value = item.TotalAmount;
                    workSheet.Cells[i + addressStartGroup1, 9].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartGroup1, 10].Value = item.Notes;
                }
                // Border
                workSheet.Cells[21, 1, addressLastRow, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[21, 1, addressLastRow, 10].Style.Border.Top.Style = ExcelBorderStyle.Dotted;
            }
            decimal? sumFee = 0, sumVat = 0;
            decimal? totalFee = surchargeOBHLst.Select(x => x.Amount).Sum();
            decimal? totalVat = surchargeOBHLst.Select(x => x.VATAmount).Sum();
            decimal? total = totalFee + totalVat;
            sumFee += totalFee;
            sumVat += totalVat;
            workSheet.Cells["G20"].Value = totalFee;
            workSheet.Cells["G20"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["H20"].Value = totalVat;
            workSheet.Cells["H20"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["I20"].Value = total;
            workSheet.Cells["I20"].Style.Numberformat.Format = numberFormat;
            // Border
            workSheet.Cells[17, 1, 20, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[17, 1, 20, 10].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[20, 1, 20, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            // Phí dịch vụ & vận chuyển/ Logistics Services(Type!=OBH)
            int addressStartGroup2 = addressStartGroup1 + countOBH;
            workSheet.Cells[addressStartGroup2, 1].Value = "II";
            workSheet.Cells[addressStartGroup2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[addressStartGroup2, 2, addressStartGroup2, 6].Merge = true;
            workSheet.Cells[addressStartGroup2, 2].Value = "Phí dịch vụ & vận chuyển/ Logistics Services";
            workSheet.Cells[addressStartGroup2, 1, addressStartGroup2, 10].Style.Font.SetFromFont(new Font("Arial", 9, FontStyle.Bold));
            var surchargeNotOBHLst = cdNoteModel.ListCharges.Where(x => x.Type != "OBH").ToList();
            totalFee = surchargeNotOBHLst.Select(x => x.Amount).Sum();
            totalVat = surchargeNotOBHLst.Select(x => x.VATAmount).Sum();
            total = totalFee + totalVat;
            sumFee += totalFee;
            sumVat += totalVat;
            workSheet.Cells[addressStartGroup2, 7].Value = totalFee;
            workSheet.Cells[addressStartGroup2, 7].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[addressStartGroup2, 8].Value = totalVat;
            workSheet.Cells[addressStartGroup2, 8].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[addressStartGroup2, 9].Value = total;
            workSheet.Cells[addressStartGroup2, 9].Style.Numberformat.Format = numberFormat;
            // Border
            workSheet.Cells[addressStartGroup2, 1, addressStartGroup2, 10].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[addressStartGroup2, 1, addressStartGroup2, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[addressStartGroup2, 1, addressStartGroup2, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            int countNotOBH = surchargeNotOBHLst.Count();
            addressStartGroup2 += 1;
            if (countNotOBH > 0)
            {
                workSheet.Cells[addressStartGroup2, 1, addressStartGroup2 + countNotOBH, 10].Style.Font.SetFromFont(new Font("Arial", 8));
                workSheet.Cells[addressStartGroup2, 1, addressStartGroup2 + countNotOBH, 10].Style.WrapText = true;
                workSheet.Cells[addressStartGroup2, 1, addressStartGroup2 + countNotOBH, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                addressLastRow = addressStartGroup2 + countNotOBH - 1;
                workSheet.Cells[addressStartGroup2, 1, addressLastRow, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[addressStartGroup2, 1, addressLastRow, 10].Style.Border.Top.Style = ExcelBorderStyle.Dotted;

                for (int i = 0; i < countNotOBH; i++)
                {
                    var item = surchargeNotOBHLst[i];
                    workSheet.Cells[i + addressStartGroup2, 1].Value = i + 1;
                    workSheet.Cells[i + addressStartGroup2, 2].Value = item.Description;
                    workSheet.Cells[i + addressStartGroup2, 2, i + addressStartGroup2, 5].Merge = true;
                    workSheet.Cells[i + addressStartGroup2, 6].Value = item.VATInvoiceNo;
                    workSheet.Cells[i + addressStartGroup2, 7].Value = item.Amount;
                    workSheet.Cells[i + addressStartGroup2, 7].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartGroup2, 8].Value = item.VATAmount;
                    workSheet.Cells[i + addressStartGroup2, 8].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartGroup2, 9].Value = item.TotalAmount;
                    workSheet.Cells[i + addressStartGroup2, 9].Style.Numberformat.Format = numberFormat;
                    workSheet.Cells[i + addressStartGroup2, 10].Value = item.Notes;
                }
            }

            // Tổng chi phí / Total in charge (I) + (II)
            int addressOfTotal = addressStartGroup2 + countNotOBH;
            workSheet.Cells[addressOfTotal, 1, addressOfTotal, 10].Style.Font.SetFromFont(new Font("Arial", 8, FontStyle.Bold));
            workSheet.Cells[addressOfTotal, 2, addressOfTotal, 5].Merge = true;
            workSheet.Cells[addressOfTotal, 2].Value = "Tổng chi phí / Total in charge (I) + (II):";
            workSheet.Cells[addressOfTotal, 7].Value = sumFee;
            workSheet.Cells[addressOfTotal, 7].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[addressOfTotal, 8].Value = sumVat;
            workSheet.Cells[addressOfTotal, 8].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[addressOfTotal, 9].Value = sumFee + sumVat;
            workSheet.Cells[addressOfTotal, 9].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[addressOfTotal, 1, addressOfTotal, 10].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[addressOfTotal, 1, addressOfTotal, 10].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[addressOfTotal, 1, addressOfTotal, 10].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[17, 1, addressOfTotal, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            addressLastRow = addressOfTotal + 1;

            string footer = "Vui lòng chuyển tiền vào tài khoản của chúng tôi như sau/Please transfer funds to our account as follow:";
            workSheet.Cells[addressLastRow, 1, addressLastRow + 12, 1].Style.Font.SetFromFont(new Font("Arial", 10));
            workSheet.Cells[addressLastRow, 1, addressLastRow + 7, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells[addressLastRow, 1].Value = footer;
            if (!string.IsNullOrEmpty(cdNoteModel.BankNameEn))
            {
                addressLastRow += 1;
                footer = "Ngân hàng/Bank: " + cdNoteModel.BankNameEn.ToUpper();
                workSheet.Cells[addressLastRow, 1].Value = footer;
            }
            if (!string.IsNullOrEmpty(cdNoteModel.BankAddressEn))
            {
                addressLastRow += 1;
                footer = cdNoteModel.BankAddressEn.ToUpper();
                workSheet.Cells[addressLastRow, 1].Value = footer;
            }
            if (!string.IsNullOrEmpty(cdNoteModel.BankAccountNameEn))
            {
                addressLastRow += 1;
                footer = "Chủ tài khoản/ower account: " + cdNoteModel.BankAccountNameEn.ToUpper();
                workSheet.Cells[addressLastRow, 1].Value = footer;
            }
            if (!string.IsNullOrEmpty(cdNoteModel.SwiftCode))
            {
                addressLastRow += 1;
                footer = "SWIFT CODE: " + cdNoteModel.SwiftCode.ToUpper();
                workSheet.Cells[addressLastRow, 1].Value = footer;
            }
            if (!string.IsNullOrEmpty(cdNoteModel.BankAccountVND))
            {
                addressLastRow += 1;
                footer = "Tài khoản/Account: " + cdNoteModel.BankAccountVND.ToUpper();
                workSheet.Cells[addressLastRow, 1].Value = footer;
            }
            addressLastRow += 2;
            workSheet.Cells[addressLastRow, 8, addressLastRow + 1, 10].Merge = true;
            workSheet.Cells[addressLastRow, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[addressLastRow, 8].Style.WrapText = true;
            workSheet.Cells[addressLastRow, 8].Value = "For and on behalf of\n" + cdNoteModel.OfficeEn?.ToUpper();
            workSheet.Cells[addressLastRow, 8].Style.Font.SetFromFont(new Font("Calibri", 11));
            workSheet.Cells[addressLastRow + 2, 8].Value = "Date";
            workSheet.Cells[addressLastRow + 3, 8].Value = "Authorizied signature";
        }
        #endregion

        #region Generate Commission-Incentive report
        /// <summary>
        /// Generate Commission Ops Report Excel
        /// </summary>
        /// <param name="listData"></param>
        /// <param name="criteria"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// 
        public Stream GenerateCommissionPROpsReportExcel(CommissionExportResult resultData, CommissionReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Commission PR for OPS");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BinddingDataCommissionPROpsReport(workSheet, resultData, criteria);
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
        /// Generate Commission Services Report Excel
        /// </summary>
        /// <param name="listData"></param>
        /// <param name="criteria"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// 
        public Stream GenerateCommissionPRReportExcel(CommissionExportResult resultData, CommissionReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Commission PR");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BinddingDataCommissionPRReport(workSheet, resultData, criteria);
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
        /// Generate Incentive Services Report Excel
        /// </summary>
        /// <param name="listData"></param>
        /// <param name="criteria"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// 
        public Stream GenerateIncentiveReportExcel(CommissionExportResult resultData, CommissionReportCriteria criteria, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Incentive");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BinddingDataIncentiveReport(workSheet, resultData, criteria);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        #endregion

        #region Bind Commission-Incentive Report
        /// <summary>
        /// Set Image and Company Info to Excel
        /// </summary>
        /// <param name="workSheet">excel sheet</param>
        private void SetImgAndCompanyInfo(ExcelWorksheet workSheet)
        {
            using (Image image = Image.FromFile(CrystalEx.GetLogoITL()))
            {
                var excelImage = workSheet.Drawings.AddPicture("Logo", image);
                //add the image to row 1, column B
                excelImage.SetPosition(0, 0, 1, 0);
            }
            var headers = new List<string>()
            {
               "INDO TRANS LOGISTICS CORPORATION", //0
               "HEAD OFFICE:", //1
               "52 Truong Son St., Tan Binh Dist.\nHo Chi Minh City, Vietnam\nTel: (848) 848 8567 (8 lines)" +
               "\nFax: (848) 848 8593 - 848 8570\nE-mail: indo-trans@itlvn.com\nWebsite: www.itlvn.com", //2
               "COMMISSION PAYMENT REQUEST", //3
               "FOR MONTH: {0}", //4
            };
            // CompanyInfo Header0
            workSheet.Cells["N1:S1"].Merge = true;
            workSheet.Cells["N1"].Value = headers[0];
            workSheet.Cells["N1"].Style.Font.SetFromFont(new Font("Arial Black", 13, FontStyle.Bold));
            workSheet.Cells["N1"].Style.Font.Italic = true;
            // CompanyInfo Header1
            workSheet.Cells["N2:S2"].Merge = true;
            workSheet.Cells["N2"].Value = headers[1];
            workSheet.Cells["N2"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10, FontStyle.Bold));
            // CompanyInfo Header2
            workSheet.Cells["N3:S6"].Merge = true;
            workSheet.Cells["N3:S6"].Style.WrapText = true;
            workSheet.Cells["N3"].Value = headers[2];
            workSheet.Cells["N3"].Style.Font.SetFromFont(new Font("Microsoft Sans Serif", 10));
        }

        /// <summary>
        /// Binding Commission Ops Report
        /// </summary>
        /// <param name="workSheet"></param>
        /// <param name="resultData"></param>
        /// <param name="criteria"></param>
        private void BinddingDataCommissionPROpsReport(ExcelWorksheet workSheet, CommissionExportResult resultData, CommissionReportCriteria criteria)
        {
            SetImgAndCompanyInfo(workSheet);

            var title = new List<string>()
            {
               "COMMISSION PAYMENT REQUEST",    // 0
               "FOR MONTH: {0}",                // 1
            };

            List<string> tableHeaders = new List<string>
            {
                "Flt/shipping month",               // 0
                "Customer",                         // 1
                "Job ID",                           // 2
                "Custom sheet",                     // 3
                "Cont.",                            // 3
                "BUYING\nRATE",                     // 4
                "SELLING\nRATE",                    // 5
                "GROSS FROFIT\nBEFORE\nCOMMISSION", // 6
                "RATE\nOF\nCOM",                    // 7
                "COM\nAMOUNT",                      // 8
                "GROSS FROFIT\nAFTER\nCOMMISSION",  // 10
                "COMMISSION\nCAP",                  // 11
                "% COM ",                           // 12
                "VND",                              // 13
                "COM OVER\nCAP",                    // 14
                "CIT\nCHARGED\nON OVERCAP",         // 15
                "ENTITLED COM",                     // 16
                "PIT (30%)",                        // 17
                "NET DUE TO\nCUSTOMERS"             // 18
            };

            var subTableHeaders = new List<string>
            {
                "[1]",                      // 0
                "[2]",                      // 1
                "[3]",                      // 2
                "[4]",                      // 3
                "[5]",                      // 4
                "[6]",                      // 5
                "[7]=[4]-[6]",              // 6
                "[8]=[7]X40%/60%",          // 7
                "",                         // 8
                "[9]=[6] X 19,000 /19,000", // 9
                "[10]=([6]-[8])X19,000",    // 10
                "[11]=[10]X25%",            // 11
                "[12]=[9]-[11]",            // 12
                "[13]=[12]X30%",            // 13
                "[12]-[13]",                // 14
            };

            // Custom With Column
            workSheet.Column(1).Width = 12;  //Cột A
            workSheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(2).Width = 10; //Cột B
            workSheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(3).Width = 15; //Cột C
            workSheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Column(4).Width = 15; //Cột D
            workSheet.Column(5).Width = 10; //Cột E
            workSheet.Column(6).Width = 10; //Cột F
            workSheet.Column(7).Width = 10; //Cột G
            workSheet.Column(8).Width = 13; //Cột H            
            workSheet.Column(9).Width = 8; //Cột I
            workSheet.Column(10).Width = 12; //Cột J
            workSheet.Column(11).Width = 15; //Cột K
            workSheet.Column(12).Width = 15; //Cột L
            workSheet.Column(13).Width = 8; //Cột M
            workSheet.Column(14).Width = 10; //Cột N
            workSheet.Column(15).Width = 10; //Cột O
            workSheet.Column(16).Width = 12; //Cột P
            workSheet.Column(17).Width = 10; //Cột Q
            workSheet.Column(18).Width = 10; //Cột R
            workSheet.Column(19).Width = 13; //Cột S

            // Header 0
            workSheet.Cells["A8:S8"].Merge = true;
            workSheet.Cells["A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A8"].Value = title[0];
            workSheet.Cells["A8"].Style.Font.SetFromFont(new Font("Calibri", 20, FontStyle.Bold));
            // Header 1
            workSheet.Cells["A9:S9"].Merge = true;
            workSheet.Cells["A9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A9"].Value = string.Format(title[1], resultData.ForMonth);
            workSheet.Cells["A9"].Style.Font.SetFromFont(new Font("Calibri", 11, FontStyle.Bold));

            workSheet.Cells["N10"].Value = "Ex.rate";
            workSheet.Cells["O10"].Value = criteria.ExchangeRate;
            workSheet.Cells["O10"].Style.Numberformat.Format = numberFormats;
            workSheet.Row(11).CustomHeight = true;
            // Set header of table
            for (int cell = 1; cell < 20; cell++)
            {
                workSheet.Cells[11, cell].Value = tableHeaders[cell - 1];
            }
            // Set subheader of table
            for (int cell = 5; cell < 20; cell++)
            {
                workSheet.Cells[12, cell].Value = subTableHeaders[cell - 5];
            }
            workSheet.Cells["A11:S12"].Style.Font.Bold = true;
            workSheet.Cells["A11:S12"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A11:S12"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A11:S12"].Style.WrapText = true;
            workSheet.View.FreezePanes(13, 2);

            int startRow = 13;
            var listDetail = resultData.Details.OrderBy(x => x.ServiceDate).ThenBy(x => x.JobId);
            foreach (var item in listDetail)
            {
                workSheet.Cells[startRow, 1].Value = item.ServiceDate?.ToString("MMM");
                workSheet.Cells[startRow, 2].Value = resultData.CustomerName;
                workSheet.Cells[startRow, 3].Value = item.JobId;
                workSheet.Cells[startRow, 4].Value = item.CustomSheet;
                workSheet.Cells[startRow, 5].Value = "";
                workSheet.Cells[startRow, 6].Value = item.BuyingRate;
                workSheet.Cells[startRow, 7].Value = item.SellingRate;
                // Gross profit before commission
                var _statement = string.Format("G{0}-F{0}", startRow);
                workSheet.Cells[startRow, 8].Formula = _statement;
                // Com Amount
                workSheet.Cells[startRow, 10].Value = item.ComAmount;
                // Gross profit after commission
                _statement = string.Format("H{0}-J{0}", startRow);
                workSheet.Cells[startRow, 11].Formula = _statement;
                // Commission cap
                _statement = string.Format("K{0}*(40%/60%)", startRow);
                workSheet.Cells[startRow, 12].Formula = _statement;
                // %Com
                _statement = string.Format("IF(K{0}=0,0,J{0}/(K{0}/60%))", startRow);
                workSheet.Cells[startRow, 13].Formula = _statement;
                // VND
                _statement = string.Format("J{0}*O10/O10", startRow);
                workSheet.Cells[startRow, 14].Formula = _statement;
                // Com over cap
                _statement = string.Format("IF(J{0}-L{0}<0,0,1)", startRow);
                workSheet.Cells[startRow, 15].Formula = _statement;
                // CIT charge on overcap
                _statement = string.Format("O{0}*25%", startRow);
                workSheet.Cells[startRow, 16].Formula = _statement;
                // Entitled COM
                _statement = string.Format("ROUND(N{0}-P{0},0)", startRow);
                workSheet.Cells[startRow, 17].Formula = _statement;
                // PIT (30%)
                _statement = string.Format("Q{0}*30%", startRow);
                workSheet.Cells[startRow, 18].Formula = _statement;
                // Net due to customers
                _statement = string.Format("Q{0}-R{0}", startRow);
                workSheet.Cells[startRow, 19].Formula = _statement;
                startRow += 1;
            }
            workSheet.Cells[12, 1, startRow, 19].Style.Font.Size = 10;
            // Row of total
            workSheet.Cells[startRow, 1].Value = "TOTAL";
            workSheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(startRow).Style.Font.Bold = true;
            workSheet.Cells[startRow, 1, startRow, 7].Merge = true;
            var amountOfCus = string.Empty;
            if (resultData.Details.Count > 0)
            {
                int rowEndSum = startRow - 1;
                char startCol = 'F';
                for (int i = 6; i < 20; i++)
                {
                    var _statement = string.Format("SUM({0}13:{0}{1})", startCol, rowEndSum);
                    workSheet.Cells[startRow, i].Formula = _statement;
                    if (i == 19)
                    {
                        amountOfCus = workSheet.Cells[startRow, i].Address;
                    }
                    startCol++;
                }
            }
            string formatNumber = "_(* #,##0_);_(* (#,##0);_(* \" - \"??_);_(@_)";
            workSheet.Cells[13, 6, startRow, 12].Style.Numberformat.Format = formatNumber;
            workSheet.Cells[13, 13, startRow, 13].Style.Numberformat.Format = "0%";
            workSheet.Cells[13, 14, startRow, 19].Style.Numberformat.Format = formatNumber;

            // Border
            workSheet.Cells[11, 1, startRow, 19].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[11, 1, startRow, 19].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[11, 1, startRow, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[startRow, 1, startRow, 19].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            // Footer
            startRow += 2;
            workSheet.Cells[startRow, 1, startRow + 13, 19].Style.Font.SetFromFont(new Font("Calibri", 12));
            workSheet.Cells[startRow, 1].Value = "The payment should be transfer to:";
            workSheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells[startRow + 1, 1].Value = "Beneficiary:";
            workSheet.Cells[startRow + 1, 4].Value = resultData.BeneficiaryName;
            workSheet.Cells[startRow + 2, 1].Value = "Amount:";
            workSheet.Cells[startRow + 2, 4].Formula = amountOfCus;
            workSheet.Cells[startRow + 2, 4].Style.Numberformat.Format = formatNumber;
            workSheet.Cells[startRow + 2, 5].Value = "VND";
            workSheet.Cells[startRow + 3, 1].Value = "A/C:";
            workSheet.Cells[startRow + 3, 4].Value = resultData.BankAccountNo;
            workSheet.Cells[startRow + 4, 1].Value = "Via:";
            workSheet.Cells[startRow + 4, 4].Value = resultData.BankName;
            workSheet.Cells[startRow + 5, 1].Value = "ID code:";
            workSheet.Cells[startRow + 5, 4].Value = resultData.TaxCode;
            workSheet.Cells[startRow + 6, 1].Value = "Tax code:";
            // Prepared by
            workSheet.Cells[startRow + 8, 5].Value = "Prepared by";
            workSheet.Cells[startRow + 13, 5].Value = resultData.PreparedBy;
            // Verified by
            workSheet.Cells[startRow + 8, 8].Value = "Verified by";
            workSheet.Cells[startRow + 13, 8].Value = resultData.VerifiedBy;
            // Approved by
            workSheet.Cells[startRow + 8, 12].Value = "Approved by";
            workSheet.Cells[startRow + 9, 12].Value = "(only in exception case)";
            workSheet.Cells[startRow + 9, 12].Style.Font.Italic = true;
            // Cross-checked by
            workSheet.Cells[startRow + 8, 17].Value = "Cross-checked by";
            workSheet.Row(startRow + 8).Style.Font.Bold = true;
        }

        /// <summary>
        /// Bind data to Commission Report
        /// </summary>
        /// <param name="workSheet"></param>
        /// <param name="resultData"></param>
        /// <param name="criteria"></param>
        private void BinddingDataCommissionPRReport(ExcelWorksheet workSheet, CommissionExportResult resultData, CommissionReportCriteria criteria)
        {
            SetImgAndCompanyInfo(workSheet);

            var title = new List<string>()
            {
               "COMMISSION PAYMENT REQUEST",    // 0
               "FOR MONTH: {0}",                // 1
            };

            List<string> tableHeaders = new List<string>
            {
                "Flt/shipping date",              // 0
                "Bill No.",                       // 1
                "CW",                             // 2
                "DEST/ ORIGIN",                   // 3
                "BUYING RATE",                    // 3
                "SELLING RATE",                   // 4
                "GROSS FROFIT BEFORE COMMISSION", // 6
                "RATE OF COM",                    // 7
                "COM AMOUNT",                     // 8
                "GROSS FROFIT AFTER COMMISSION",  // 10
                "COMMISSION CAP",                 // 11
                "% COM ",                         // 12
                "VND",                            // 13
                "COM OVER CAP",                   // 14
                "CIT CHARGED ON OVERCAP",         // 15
                "ENTITLED COM",                   // 16
                "PIT (30%)",                      // 17
                "NET DUE TO CUSTOMERS"            // 18
            };

            var subTableHeaders = new List<string>
            {
                "[1]",                      // 0
                "",                         // 1
                "[2]",                      // 2
                "[3]",                      // 3
                "[4]=[3]-[2]",              // 4
                "[5]",                      // 5
                "[6]=[1]x[5]",              // 6
                "[7]=[4]-[6]",              // 7
                "[8]=[7]X40%/60%",          // 8
                "",                         // 9
                "[9]=[6] X 20,000",         // 10
                "[10]=([6]-[8])X20,000",    // 11
                "[11]=[10]X25%",            // 12
                "[12]=[9]-[11]",            // 13
                "[13]= [12]X10% ",          // 14
                "[12]-[13]",                // 15
            };

            // Custom With Column
            workSheet.Column(1).Width = 12;  //Cột A
            workSheet.Column(2).Width = 10; //Cột B
            workSheet.Column(3).Width = 15; //Cột C
            workSheet.Column(4).Width = 15; //Cột D
            workSheet.Column(5).Width = 10; //Cột E
            workSheet.Column(6).Width = 10; //Cột F
            workSheet.Column(7).Width = 13; //Cột G
            workSheet.Column(8).Width = 13; //Cột H            
            workSheet.Column(9).Width = 11; //Cột I
            workSheet.Column(10).Width = 13; //Cột J
            workSheet.Column(11).Width = 15; //Cột K
            workSheet.Column(12).Width = 15; //Cột L
            workSheet.Column(13).Width = 8; //Cột M
            workSheet.Column(14).Width = 10; //Cột N
            workSheet.Column(15).Width = 10; //Cột O
            workSheet.Column(16).Width = 12; //Cột P
            workSheet.Column(17).Width = 10; //Cột Q
            workSheet.Column(18).Width = 13; //Cột R

            // Header 0
            workSheet.Cells["A8:R8"].Merge = true;
            workSheet.Cells["A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A8"].Value = title[0];
            workSheet.Cells["A8"].Style.Font.SetFromFont(new Font("Calibri", 20, FontStyle.Bold));
            // Header 1
            workSheet.Cells["A9:R9"].Merge = true;
            workSheet.Cells["A9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A9"].Value = string.Format(title[1], resultData.ForMonth.ToUpper());
            workSheet.Cells["A9"].Style.Font.SetFromFont(new Font("Calibri", 11, FontStyle.Bold));

            // Customer name
            workSheet.Cells["A10"].Value = "CUSTOMER";
            workSheet.Cells["B10"].Value = resultData.CustomerName.ToUpper();
            workSheet.Cells[10, 1, 10, 2].Style.Font.Bold = true;
            // Ex.rate
            workSheet.Cells["M10"].Value = "Ex.rate";
            workSheet.Cells["N10"].Value = criteria.ExchangeRate;
            workSheet.Cells["N10"].Style.Numberformat.Format = numberFormats;
            workSheet.Row(9).CustomHeight = true;
            // Set header of table
            for (int cell = 1; cell < 19; cell++)
            {
                workSheet.Cells[11, cell].Value = tableHeaders[cell - 1];
            }
            // Set subheader of table
            for (int cell = 3; cell < 19; cell++)
            {
                workSheet.Cells[12, cell].Value = subTableHeaders[cell - 3];
            }
            workSheet.Cells["A11:S12"].Style.Font.Bold = true;
            workSheet.Cells["A11:S12"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A11:S12"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A11:S12"].Style.WrapText = true;
            workSheet.View.FreezePanes(13, 2);

            int startRow = 13;
            var listDetail = resultData.Details.OrderBy(x => x.ServiceDate).ThenBy(x => x.HBLNo);
            foreach (var item in listDetail)
            {
                workSheet.Cells[startRow, 1].Value = item.ServiceDate?.ToString("dd-MMM");
                workSheet.Cells[startRow, 2].Value = item.HBLNo;
                workSheet.Cells[startRow, 3].Value = item.ChargeWeight;
                workSheet.Cells[startRow, 4].Value = item.PortCode;
                workSheet.Cells[startRow, 5].Value = item.BuyingRate;
                workSheet.Cells[startRow, 6].Value = item.SellingRate;
                // Gross profit before commission
                var _statement = string.Format("F{0}-E{0}", startRow);
                workSheet.Cells[startRow, 7].Formula = _statement;
                // Rate of com
                _statement = string.Format("IF(C{0}=0,0,I{0}/C{0})", startRow);
                workSheet.Cells[startRow, 8].Formula = _statement;
                // Com Amount
                workSheet.Cells[startRow, 9].Value = item.ComAmount;
                // Gross profit after commission
                _statement = string.Format("G{0}-I{0}", startRow);
                workSheet.Cells[startRow, 10].Formula = _statement;
                // Commission cap
                _statement = string.Format("J{0}*(40%/60%)", startRow);
                workSheet.Cells[startRow, 11].Formula = _statement;
                // %Com
                _statement = string.Format("IF(J{0}=0,0,I{0}/(J{0}/60%))", startRow);
                workSheet.Cells[startRow, 12].Formula = _statement;
                // VND
                _statement = string.Format("I{0}*N10", startRow);
                workSheet.Cells[startRow, 13].Formula = _statement;
                // Com over cap
                _statement = string.Format("IF(I{0}-K{0}<0,0,(I{0}-K{0})*N10)", startRow);
                workSheet.Cells[startRow, 14].Formula = _statement;
                // CIT charge on overcap
                _statement = string.Format("N{0}*25%", startRow);
                workSheet.Cells[startRow, 15].Formula = _statement;
                // Entitled COM
                _statement = string.Format("ROUND(M{0}-O{0},0)", startRow);
                workSheet.Cells[startRow, 16].Formula = _statement;
                // PIT (30%)
                _statement = string.Format("P{0}*10%", startRow);
                workSheet.Cells[startRow, 17].Formula = _statement;
                // Net due to customers
                _statement = string.Format("P{0}-Q{0}", startRow);
                workSheet.Cells[startRow, 18].Formula = _statement;
                startRow += 1;
            }
            workSheet.Cells[12, 1, startRow, 18].Style.Font.Size = 10;
            // Row of total
            workSheet.Cells[startRow, 1].Value = "TOTAL";
            workSheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Row(startRow).Style.Font.Bold = true;
            var amountOfCus = string.Empty;
            if (resultData.Details.Count > 0)
            {
                int rowEndSum = startRow - 1;
                char startCol = 'C';
                for (int i = 3; i < 19; i++)
                {
                    if (i == 4) // except column DEST/ ORIGIN
                    {
                        startCol++;
                        continue;
                    }
                    var _statement = string.Format("SUM({0}13:{0}{1})", startCol, rowEndSum);
                    workSheet.Cells[startRow, i].Formula = _statement;
                    if (i == 18)
                    {
                        amountOfCus = workSheet.Cells[startRow, i].Address;
                    }
                    startCol++;
                }
            }
            string formatNumber = "_(* #,##0.00_);_(* (#,##0.00);_(* \" - \"??_);_(@_)";
            workSheet.Cells[13, 3, startRow, 3].Style.Numberformat.Format = formatNumber;
            workSheet.Cells[13, 12, startRow - 1, 12].Style.Numberformat.Format = "0%";
            workSheet.Cells[13, 5, startRow - 1, 11].Style.Numberformat.Format = formatNumber;
            workSheet.Cells[13, 13, startRow - 1, 18].Style.Numberformat.Format = formatNumber;
            workSheet.Cells[startRow, 3, startRow, 18].Style.Numberformat.Format = formatNumber;

            // Border
            workSheet.Cells[11, 1, startRow, 18].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[11, 1, startRow, 18].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[11, 1, startRow, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[startRow, 1, startRow, 18].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            // Footer
            startRow += 2;
            workSheet.Cells[startRow, 1, startRow + 13, 18].Style.Font.SetFromFont(new Font("Calibri", 12));
            workSheet.Cells[startRow, 1].Value = "The payment should be transfer to:";
            workSheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells[startRow + 1, 1].Value = "Beneficiary:";
            workSheet.Cells[startRow + 1, 4].Value = resultData.BeneficiaryName;
            workSheet.Cells[startRow + 2, 1].Value = "Amount:";
            workSheet.Cells[startRow + 2, 4].Formula = amountOfCus;
            workSheet.Cells[startRow + 2, 4].Style.Numberformat.Format = "#,### \"VND\"";
            workSheet.Cells[startRow + 3, 1].Value = "A/C:";
            workSheet.Cells[startRow + 3, 4].Value = resultData.BankAccountNo;
            workSheet.Cells[startRow + 4, 1].Value = "Via:";
            workSheet.Cells[startRow + 4, 4].Value = resultData.BankName;
            workSheet.Cells[startRow + 5, 1].Value = "ID code:";
            workSheet.Cells[startRow + 5, 4].Value = resultData.TaxCode;
            workSheet.Cells[startRow + 6, 1].Value = "Tax code:";
            // Prepared by
            workSheet.Cells[startRow + 8, 3].Value = "Prepared by";
            workSheet.Cells[startRow + 13, 3].Value = resultData.PreparedBy;
            // Verified by
            workSheet.Cells[startRow + 8, 7].Value = "Verified by";
            workSheet.Cells[startRow + 13, 7].Value = resultData.VerifiedBy;
            // Approved by
            workSheet.Cells[startRow + 8, 11].Value = "Approved by";
            workSheet.Cells[startRow + 13, 11].Value = resultData.ApprovedBy;
            // Cross-checked by
            workSheet.Cells[startRow + 8, 16].Value = "Cross-checked by";
            workSheet.Cells[startRow + 13, 16].Value = resultData.CrossCheckedBy;
            workSheet.Row(startRow + 8).Style.Font.Bold = true;
        }

        /// <summary>
        /// Bind data to Incentive Report
        /// </summary>
        /// <param name="workSheet"></param>
        /// <param name="resultData"></param>
        /// <param name="criteria"></param>
        private void BinddingDataIncentiveReport(ExcelWorksheet workSheet, CommissionExportResult resultData, CommissionReportCriteria criteria)
        {
            List<string> tableHeaders = new List<string>
            {
                "CLIENT",
                "MBL",
                "HBL",
                "PROFIT ($)"
            };
            // Custom With Column
            workSheet.Column(1).Width = 17;  //Cột A
            workSheet.Column(2).Width = 40; //Cột B
            workSheet.Column(3).Width = 17; //Cột C
            workSheet.Column(4).Width = 17; //Cột D
            workSheet.Column(5).Width = 15; //Cột E

            workSheet.Cells["A1:E1"].Merge = true;
            workSheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A1"].Value = "DE NGHI THANH TOAN COMMISSION";
            workSheet.Cells["A1"].Style.Font.SetFromFont(new Font("VNI-Times", 14, FontStyle.Bold));
            workSheet.Cells["A2:E2"].Merge = true;
            workSheet.Cells["A2"].Value = resultData.ForMonth.ToUpper();
            workSheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A2"].Style.Font.SetFromFont(new Font("VNI-Times", 14, FontStyle.Bold));

            for (int i = 2; i < 6; i++)
            {
                workSheet.Cells[3, i].Value = tableHeaders[i - 2];
            }
            workSheet.Row(3).Style.Font.Bold = true;
            workSheet.Row(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var monthGrp = resultData.Details.GroupBy(x => x.ServiceDate?.Month).OrderBy(x => x.Key).Select(x => x.Key);
            var startRow = 4;
            foreach (var mon in monthGrp)
            {
                var shipmentGrp = resultData.Details.Where(x => x.ServiceDate?.Month == mon).OrderBy(x => x.JobId);
                if (shipmentGrp.Count() > 0)
                {
                    var month = shipmentGrp.FirstOrDefault().ServiceDate?.ToString("MMM");
                    workSheet.Cells[startRow, 1].Value = month;
                    workSheet.Cells[startRow, 1, startRow, 5].Merge = true;
                    startRow++;
                }
                else
                {
                    continue;
                }
                var startGrp = startRow;
                foreach (var shipment in shipmentGrp)
                {
                    workSheet.Cells[startRow, 1].Value = shipment.JobId;
                    workSheet.Cells[startRow, 2].Value = resultData.CustomerName;
                    workSheet.Cells[startRow, 3].Value = shipment.MBLNo;
                    workSheet.Cells[startRow, 4].Value = shipment.HBLNo;
                    workSheet.Cells[startRow, 5].Value = shipment.BuyingRate - shipment.SellingRate;
                    startRow++;
                }
                workSheet.Cells[startGrp, 1, startRow - 1, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }
            workSheet.Cells[5, 5, startRow - 1, 5].Style.Numberformat.Format = numberFormats;
            workSheet.Cells[startRow, 1].Value = "TOTAL(USD)";
            workSheet.Cells[startRow, 1, startRow, 3].Merge = true;
            var _statement = string.Format("SUM(E5:E{0})", startRow - 1);
            workSheet.Cells[startRow, 5].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[startRow, 5].Formula = _statement;
            workSheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            startRow++;
            workSheet.Cells[startRow, 1].Value = string.Format("De nghi thanh toan VND: (USDx10% x {0})", resultData.ExchangeRate.ToString("#,##0"));
            workSheet.Cells[startRow, 1, startRow, 3].Merge = true;
            workSheet.Cells[startRow, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            _statement = string.Format("E{0}*0.1*{1}", startRow - 1, resultData.ExchangeRate);
            workSheet.Cells[startRow, 5].Formula = _statement;
            workSheet.Cells[startRow, 5].Style.Numberformat.Format = numberFormat;
            // Border
            workSheet.Cells[3, 1, startRow, 5].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[3, 1, startRow, 5].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[3, 1, startRow, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[startRow, 1, startRow, 5].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            startRow += 3;
            // Issued by
            workSheet.Cells[startRow, 2].Value = "Issued by";
            workSheet.Cells[startRow + 5, 2].Value = resultData.PreparedBy;
            // Manager
            workSheet.Cells[startRow, 3].Value = "Manager";
            workSheet.Cells[startRow + 5, 3].Value = resultData.VerifiedBy;
            // Chief of Accounting
            workSheet.Cells[startRow, 4].Value = "Chief of Accounting";
            // Approved by
            workSheet.Cells[startRow, 5].Value = "Approved by";
            workSheet.Row(startRow).Style.Font.Bold = true;
            workSheet.Row(startRow + 5).Style.Font.Bold = true;
        }
        #endregion
    }
}
