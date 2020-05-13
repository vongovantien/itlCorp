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
                ert = workSheet.Cells[addressStartContent, i+1].RichText.Add(" \n" + houseTitles[i].ENTitle);
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
            for (int i = 0; i< containerTitles.Count; i++)
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
            if(transactionDetail.CsMawbcontainers != null)
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
                workSheet.Cells[13, i+1].Style.WrapText = true;
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
                    if(i == 1)
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
                workSheet.Cells[addressStartContent, i+1].Style.WrapText = true;
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
                for (int i = 1; i < (totalColums +1) / 2; i++)
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
            workSheet.Cells["I30"].Value = airwayBillExport.Total;
            workSheet.Cells["I30"].Style.Numberformat.Format = numberFormat;

            workSheet.Cells["A31"].Value = "PCS"; //Default
            workSheet.Cells["L31:N39"].Merge = true;
            workSheet.Cells["L31:N39"].Style.WrapText = true;
            workSheet.Cells["L31:N39"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["L31"].Value = (airwayBillExport.DesOfGood + "\r\n" + airwayBillExport.VolumeField)?.ToUpper();

            workSheet.Cells["A40"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["A40"].Value = airwayBillExport.Pieces;
            workSheet.Cells["B40"].Value = airwayBillExport.Gw;
            workSheet.Cells["B40"].Style.Numberformat.Format = numberFormatKgs;

            workSheet.Cells["A44:B44"].Merge = true;
            workSheet.Cells["A44:B44"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A44"].Value = airwayBillExport.PrepaidWt?.ToUpper();
            //workSheet.Cells["A44"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["D44"].Value = airwayBillExport.CollectWt?.ToUpper();

            workSheet.Cells["A46:B46"].Merge = true;
            workSheet.Cells["A46:B46"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A46"].Value = airwayBillExport.PrepaidVal?.ToUpper();
            workSheet.Cells["D46"].Value = airwayBillExport.CollectVal?.ToUpper();

            workSheet.Cells["A48:B48"].Merge = true;
            workSheet.Cells["A48:B48"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A48"].Value = airwayBillExport.PrepaidTax?.ToUpper();
            workSheet.Cells["D48"].Value = airwayBillExport.CollectTax?.ToUpper();

            workSheet.Cells["A51:B51"].Merge = true;
            workSheet.Cells["A51:B51"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A51"].Value = airwayBillExport.PrepaidDueToCarrier?.ToUpper();
            workSheet.Cells["D51"].Value = airwayBillExport.CollectDueToCarrier?.ToUpper();

            workSheet.Cells["A55:B55"].Merge = true;
            workSheet.Cells["A55"].Value = airwayBillExport.PrepaidTotal?.ToUpper();
            workSheet.Cells["A55"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["D55"].Value = airwayBillExport.CollectTotal?.ToUpper();

            //Other Charges
            int k = 44;
            for (var i = 0; i < airwayBillExport.OtherCharges.Count; i++)
            {
                workSheet.Cells["H" + k].Value = airwayBillExport.OtherCharges[i].ChargeName?.ToUpper();
                workSheet.Cells["J" + k].Value = airwayBillExport.OtherCharges[i].Amount;
                workSheet.Cells["J" + k].Style.Numberformat.Format = numberFormat;
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
            if (airwayBillExport.Total == null || airwayBillExport.Total == 0)
            {
                workSheet.Cells["I30"].Value = "AS AGREED";
            }
            else
            {
                workSheet.Cells["I30"].Value = airwayBillExport.Total;
                workSheet.Cells["I30"].Style.Numberformat.Format = numberFormat;
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

            workSheet.Cells["B1"].Style.Font.Bold = true;
            workSheet.Cells["B1"].Value = "INDO TRANS LOGISTICS CORPORATION";

            workSheet.Cells["B2"].Value = "52-54-56 TRUONG SON STR., TAN BINH DIST,";
            workSheet.Cells["B3"].Value = "HOCHIMINH CITY, VIETNAM.";

            workSheet.Row(4).Height = 29.25;
            workSheet.Row(4).Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["B4"].Value = "84 8 39486888";
            workSheet.Cells["D4"].Value = "84 8 8488593";

            workSheet.Cells["B7:F10"].Merge = true;
            workSheet.Cells["B7:F10"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["B7"].Style.Font.Bold = true;
            workSheet.Cells["B7"].Style.WrapText = true;
            workSheet.Cells["B7"].Value = airwayBillExport.Shipper?.ToUpper();

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

            workSheet.Cells["B2"].Value = "INDO TRANS LOGISTICS (ITL) .MST: 0301909173";
            workSheet.Cells["B3"].Value = "52-54-56 TRUONG SON STR., TAN BINH DIST, HOCHIMINH CITY";
            workSheet.Cells["B4"].Value = "TEL: 84 8 8488567. FAX: 84 8 8488593";
            
            workSheet.Cells["B8:F12"].Merge = true;
            workSheet.Cells["B8:F12"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["B8"].Style.WrapText = true;
            workSheet.Cells["B8"].Value = airwayBillExport.Shipper?.ToUpper();

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
               "MONTHLY SALES REPORT"
            };

            List<string> headersTable = new List<string>()
            {
                "No",
                "SERVICE",
                "JOB NO",
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
                if (i == 24)
                {
                    workSheet.Cells[9, i + 5].Value = headersTable[i];
                }
                if (i < 24)
                {
                    workSheet.Cells[9, i + 1].Value = headersTable[i];
                }
                if (i > 24 )
                {
                    workSheet.Cells[9, i + 5].Value = headersTable[i];
                    workSheet.Cells[9, i + 5].Style.Font.Bold = true;
                }
                workSheet.Cells[9, i + 1].Style.Font.Bold = true;
                workSheet.Cells[9, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[9, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            }

            for (int i = 26; i < headersTable.Count; i++)
            {
                workSheet.Cells[9, i + 10].Value = headersTable[i];
                workSheet.Cells[9, i + 10].Style.Font.Bold = true;
                workSheet.Cells[9, i + 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[9, i + 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }
            workSheet.Cells["AD9:AI9"].Merge = true;
            workSheet.Cells["Y9:AC9"].Merge = true;
            workSheet.Cells["Y9"].Value = headersTable[24];

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

            workSheet.Cells["AJ9:AJ10"].Merge = true;
            workSheet.Cells["AK9:AK10"].Merge = true;
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

            workSheet.Cells["Y10"].Value = subheadersTable[0];
            workSheet.Cells["Z10"].Value = subheadersTable[1];
            workSheet.Cells["AA10"].Value = subheadersTable[2];
            workSheet.Cells["AB10"].Value = subheadersTable[3];
            workSheet.Cells["AC10"].Value = subheadersTable[4];

            workSheet.Cells["AD10"].Value = subheadersTable[0];
            workSheet.Cells["AE10"].Value = subheadersTable[1];
            workSheet.Cells["AF10"].Value = subheadersTable[2];
            workSheet.Cells["AG10"].Value = subheadersTable[5];
            workSheet.Cells["AH10"].Value = subheadersTable[3];
            workSheet.Cells["AI10"].Value = subheadersTable[4];

            workSheet.Cells["Y10:AI10"].Style.Font.Bold = true;

            workSheet.Cells["Y10:AI10"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["Y10:AI10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            int addressStartContent = 11;
            int positionStart = 1;
            for (int i = 0; i < overview.Count; i++)
            {
                var item = overview[i];
                workSheet.Cells[i + addressStartContent, 1].Value = i + 1;
                workSheet.Cells[i + addressStartContent, 2].Value = item.ServiceName;
                workSheet.Cells[i + addressStartContent, 3].Value = item.JobNo;
                workSheet.Cells[i + addressStartContent, 4].Value = item.etd.HasValue ? item.etd.Value.ToString("dd/MM/yyyy") : "";
                workSheet.Cells[i + addressStartContent, 5].Value = item.eta.HasValue ? item.eta.Value.ToString("dd/MM/yyyy") : "";
                workSheet.Cells[i + addressStartContent, 6].Value = item.FlightNo;
                workSheet.Cells[i + addressStartContent, 7].Value = item.MblMawb;
                workSheet.Cells[i + addressStartContent, 8].Value = item.HblHawb;
                workSheet.Cells[i + addressStartContent, 9].Value = item.PolPod;
                workSheet.Cells[i + addressStartContent, 10].Value = item.Carrier;
                workSheet.Cells[i + addressStartContent, 11].Value = item.Agent;
                workSheet.Cells[i + addressStartContent, 12].Value = item.Shipper;
                workSheet.Cells[i + addressStartContent, 13].Value = item.Consignee;
                workSheet.Cells[i + addressStartContent, 14].Value = item.ShipmentType;
                workSheet.Cells[i + addressStartContent, 15].Value = item.Salesman;
                workSheet.Cells[i + addressStartContent, 16].Value = item.AgentName;
                workSheet.Cells[i + addressStartContent, 17].Value = item.QTy;
                workSheet.Cells[i + addressStartContent, 18].Value = item.Cont20;
                workSheet.Cells[i + addressStartContent, 19].Value = item.Cont40;
                workSheet.Cells[i + addressStartContent, 20].Value = item.Cont40HC;
                workSheet.Cells[i + addressStartContent, 21].Value = item.Cont45;
                workSheet.Cells[i + addressStartContent, 22].Value = item.GW;
                workSheet.Cells[i + addressStartContent, 23].Value = item.CW;
                workSheet.Cells[i + addressStartContent, 24].Value = item.CBM;
                workSheet.Cells[i + addressStartContent, 25].Value = item.TotalSellFreight;
                workSheet.Cells[i + addressStartContent, 25].Style.Numberformat.Format = criteria.Currency =="VND"  ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 26].Value = item.TotalSellTrucking ;
                workSheet.Cells[i + addressStartContent, 26].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 27].Value = item.TotalSellHandling;
                workSheet.Cells[i + addressStartContent, 27].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 28].Value = item.TotalSellOthers;
                workSheet.Cells[i + addressStartContent, 28].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 29].Value = item.TotalSell;
                workSheet.Cells[i + addressStartContent, 29].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 30].Value = item.TotalBuyFreight;
                workSheet.Cells[i + addressStartContent, 30].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 31].Value = item.TotalBuyTrucking;
                workSheet.Cells[i + addressStartContent, 31].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 32].Value = item.TotalBuyHandling;
                workSheet.Cells[i + addressStartContent, 32].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 33].Value = item.TotalBuyKB;
                workSheet.Cells[i + addressStartContent, 33].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 34].Value = item.TotalBuyOthers;
                workSheet.Cells[i + addressStartContent, 34].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 35].Value = item.TotalBuy;
                workSheet.Cells[i + addressStartContent, 35].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 36].Value = item.Profit;
                workSheet.Cells[i + addressStartContent, 36].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 37].Value = item.AmountOBH;
                workSheet.Cells[i + addressStartContent, 37].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 38].Value = item.TotalBuyKB;
                workSheet.Cells[i + addressStartContent, 38].Style.Numberformat.Format = criteria.Currency == "VND" ? numberFormatVND : numberFormat;
                workSheet.Cells[i + addressStartContent, 39].Value = item.Destination;
                workSheet.Cells[i + addressStartContent, 40].Value = item.CustomerId;
                workSheet.Cells[i + addressStartContent, 41].Value = item.CustomerName;
                workSheet.Cells[i + addressStartContent, 42].Value = item.RalatedHblHawb ;
                workSheet.Cells[i + addressStartContent, 43].Value = item.RalatedJobNo ;
                workSheet.Cells[i + addressStartContent, 44].Value = item.HandleOffice;
                workSheet.Cells[i + addressStartContent, 45].Value = item.SalesOffice;
                workSheet.Cells[i + addressStartContent, 46].Value = item.Creator;
                workSheet.Cells[i + addressStartContent, 47].Value = item.POINV;
                workSheet.Cells[i + addressStartContent, 48].Value = item.BKRefNo;
                workSheet.Cells[i + addressStartContent, 49].Value = item.Commodity;
                workSheet.Cells[i + addressStartContent, 50].Value = item.ServiceMode;
                workSheet.Cells[i + addressStartContent, 51].Value = item.PMTerm;
                workSheet.Cells[i + addressStartContent, 52].Value = item.ShipmentNotes;
                workSheet.Cells[i + addressStartContent, 53].Value = item.Created.HasValue ? item.Created.Value.ToString("dd/MM/yyyy") : "";
                workSheet.Cells.AutoFitColumns();
                positionStart++;
            }
            positionStart = positionStart - 2;
            workSheet.Cells[9, 1, addressStartContent + positionStart, 53].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[9, 1, addressStartContent + positionStart, 53].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[9, 1, addressStartContent + positionStart, 53].Style.Border.Top.Style = ExcelBorderStyle.Thin;
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
            workSheet.Column(13).Width = 14; //Cột M
            workSheet.Column(14).Width = 17; //Cột N
            workSheet.Column(15).Width = 15; //Cột O
            workSheet.Column(16).Width = 20; //Cột P
            workSheet.Column(17).Width = 17; //Cột Q
            workSheet.Column(18).Width = 17; //Cột R
            workSheet.Column(19).Width = 14; //Cột S
            workSheet.Column(20).Width = 18; //Cột T
            workSheet.Column(21).Width = 17; //Cột U
            workSheet.Column(22).Width = 20; //Cột V
            workSheet.Column(23).Width = 17; //Cột W
            workSheet.Column(24).Width = 15; //Cột X
            workSheet.Column(25).Width = 22; //Cột Y
            workSheet.Column(26).Width = 17; //Cột Z
            workSheet.Column(27).Width = 18; //Cột AA
            workSheet.Column(28).Width = 17; //Cột AB
            workSheet.Column(29).Width = 19; //Cột AC
            workSheet.Column(30).Width = 19; //Cột AD
            workSheet.Column(31).Width = 24; //Cột AE
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

            workSheet.Cells["Z1:AE1"].Merge = true;
            workSheet.Cells["Z1"].Value = headers[0];
            workSheet.Cells["Z1"].Style.Font.Bold = true;
            workSheet.Cells["Z1"].Style.Font.Italic = true;

            workSheet.Row(2).Height = 64.5;
            workSheet.Cells["Z2:AE2"].Merge = true;
            workSheet.Cells["Z2:AE2"].Style.WrapText = true;
            workSheet.Cells["Z2"].Value = headers[1];
            workSheet.Cells["Z2"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            
            workSheet.Cells["A4:AE4"].Merge = true;
            workSheet.Cells["A4"].Value = headers[2];
            workSheet.Cells["A4"].Style.Font.Bold = true;
            workSheet.Cells["A4"].Style.Font.Size = 13;
            workSheet.Cells["A4"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            DateTime? _fromDate = criteria.CreatedDateFrom != null ? criteria.CreatedDateFrom : criteria.ServiceDateFrom;
            DateTime? _toDate = criteria.CreatedDateTo != null ? criteria.CreatedDateTo : criteria.ServiceDateTo;

            workSheet.Cells["A5:AE5"].Merge = true;
            workSheet.Cells["A5"].Value = "From: " + _fromDate.Value.ToString("dd MMM, yyyy") + " to: " + _toDate.Value.ToString("dd MMM, yyyy");
            workSheet.Cells["A5"].Style.Font.Bold = true;
            workSheet.Cells["A5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            //Header table
            workSheet.Cells["A7:AE8"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A7:AE8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A7:AE8"].Style.Font.Bold = true;

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
            workSheet.Cells["M8"].Value = headers[16]; //USD (Revenue)
            workSheet.Cells["N8"].Value = headers[17]; //VND (Revenue)
            workSheet.Cells["O8"].Value = headers[18]; //TAX Out
            workSheet.Cells["P8"].Value = headers[19]; //Total (Revenue)

            workSheet.Cells["Q7:V7"].Merge = true;
            workSheet.Cells["Q7"].Value = headers[20]; //COST
            workSheet.Cells["Q8"].Value = headers[15]; //TAX Inv.No (Cost)
            workSheet.Cells["R8"].Value = headers[21]; //Voucher No.
            workSheet.Cells["S8"].Value = headers[16]; //USD (Cost)
            workSheet.Cells["T8"].Value = headers[17]; //VND (Cost)
            workSheet.Cells["U8"].Value = headers[22]; //TAX In
            workSheet.Cells["V8"].Value = headers[19]; //Total (Cost)

            workSheet.Cells["W7:W8"].Merge = true;
            workSheet.Cells["W7"].Value = headers[23]; //Com.

            workSheet.Cells["X7:X8"].Merge = true;
            workSheet.Cells["X7"].Value = headers[24]; //Ex Rate

            workSheet.Cells["Y7:Y8"].Merge = true;
            workSheet.Cells["Y7"].Value = headers[25]; //Balance

            workSheet.Cells["Z7:AA7"].Merge = true;
            workSheet.Cells["Z7"].Value = headers[26]; //Payment on Behalf
            workSheet.Cells["Z8"].Value = headers[27]; //Inv.No
            workSheet.Cells["AA8"].Value = headers[28]; //Amount

            workSheet.Cells["AB7:AB8"].Merge = true;
            workSheet.Cells["AB7"].Value = headers[29]; //Paid Date

            workSheet.Cells["AC7:AC8"].Merge = true;
            workSheet.Cells["AC7"].Value = headers[30]; //A/C Voucher No.

            workSheet.Cells["AD7:AD8"].Merge = true;
            workSheet.Cells["AD7"].Value = headers[31]; //P/M Voucher No.

            workSheet.Cells["AE7:AE8"].Merge = true;
            workSheet.Cells["AE7"].Value = headers[32]; //Service
            //Header table

            int rowStart = 9;
            for(int i = 0; i < listData.Count; i++)
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

                if (listData[i].UsdRevenue != null && listData[i].UsdRevenue != 0)
                {
                    workSheet.Cells[rowStart, 13].Value = listData[i].UsdRevenue;
                    workSheet.Cells[rowStart, 13].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].VndRevenue != null && listData[i].VndRevenue != 0)
                {
                    workSheet.Cells[rowStart, 14].Value = listData[i].VndRevenue;
                    workSheet.Cells[rowStart, 14].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].TaxOut != null && listData[i].TaxOut != 0)
                {
                    workSheet.Cells[rowStart, 15].Value = listData[i].TaxOut;
                    workSheet.Cells[rowStart, 15].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].TotalRevenue != null && listData[i].TotalRevenue != 0)
                {
                    workSheet.Cells[rowStart, 16].Value = listData[i].TotalRevenue;
                    workSheet.Cells[rowStart, 16].Style.Numberformat.Format = numberFormat;
                }

                workSheet.Cells[rowStart, 17].Value = listData[i].TaxInvNoCost;
                workSheet.Cells[rowStart, 18].Value = listData[i].VoucherId;

                if (listData[i].UsdCost != null && listData[i].UsdCost != 0)
                {
                    workSheet.Cells[rowStart, 19].Value = listData[i].UsdCost;
                    workSheet.Cells[rowStart, 19].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].VndCost != null && listData[i].VndCost != 0)
                {
                    workSheet.Cells[rowStart, 20].Value = listData[i].VndCost;
                    workSheet.Cells[rowStart, 20].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].TaxIn != null && listData[i].TaxIn != 0)
                {
                    workSheet.Cells[rowStart, 21].Value = listData[i].TaxIn;
                    workSheet.Cells[rowStart, 21].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].TotalCost != null && listData[i].TotalCost != 0)
                {
                    workSheet.Cells[rowStart, 22].Value = listData[i].TotalCost;
                    workSheet.Cells[rowStart, 22].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].TotalKickBack != null && listData[i].TotalKickBack != 0)
                {
                    workSheet.Cells[rowStart, 23].Value = listData[i].TotalKickBack;
                    workSheet.Cells[rowStart, 23].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].ExchangeRate != 0)
                {
                    workSheet.Cells[rowStart, 24].Value = listData[i].ExchangeRate;
                    workSheet.Cells[rowStart, 24].Style.Numberformat.Format = numberFormat;
                }

                if (listData[i].Balance != null && listData[i].Balance != 0)
                {
                    workSheet.Cells[rowStart, 25].Value = listData[i].Balance;
                    workSheet.Cells[rowStart, 25].Style.Numberformat.Format = numberFormat;
                }

                workSheet.Cells[rowStart, 26].Value = listData[i].InvNoObh;

                if (listData[i].AmountObh != null && listData[i].AmountObh != 0)
                {
                    workSheet.Cells[rowStart, 27].Value = listData[i].AmountObh;
                    workSheet.Cells[rowStart, 27].Style.Numberformat.Format = numberFormat;
                }

                workSheet.Cells[rowStart, 28].Value = listData[i].PaidDate;
                workSheet.Cells[rowStart, 29].Value = listData[i].AcVoucherNo;
                workSheet.Cells[rowStart, 30].Value = listData[i].PmVoucherNo;
                workSheet.Cells[rowStart, 31].Value = listData[i].Service;

                rowStart += 1;

            }

            workSheet.Cells[rowStart, 1, rowStart, 31].Style.Font.Bold = true;
            workSheet.Cells[rowStart, 1, rowStart, 11].Merge = true;
            workSheet.Cells[rowStart, 1, rowStart, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[rowStart, 1].Value = headers[19];

            workSheet.Cells[rowStart, 13].Value = listData.Select(s => s.UsdRevenue).Sum(); // Total USD Revenue           
            workSheet.Cells[rowStart, 14].Value = listData.Select(s => s.VndRevenue).Sum(); // Total VND Revenue
            workSheet.Cells[rowStart, 15].Value = listData.Select(s => s.TaxOut).Sum(); // Total TaxOut
            workSheet.Cells[rowStart, 16].Value = listData.Select(s => s.TotalRevenue).Sum(); // Sum Total Revenue            
            for(int i = 13; i < 17; i++)
            {
                workSheet.Cells[rowStart, i].Style.Numberformat.Format = numberFormat;
            }

            workSheet.Cells[rowStart, 19].Value = listData.Select(s => s.UsdCost).Sum(); // Total USD Cost
            workSheet.Cells[rowStart, 20].Value = listData.Select(s => s.VndCost).Sum(); // Total VND Cost
            workSheet.Cells[rowStart, 21].Value = listData.Select(s => s.TaxIn).Sum(); // Total TaxIn
            workSheet.Cells[rowStart, 22].Value = listData.Select(s => s.TotalCost).Sum(); // Sum Total Cost
            for (int i = 19; i < 23; i++)
            {
                workSheet.Cells[rowStart, i].Style.Numberformat.Format = numberFormat;
            }

            workSheet.Cells[rowStart, 25].Value = listData.Select(s => s.Balance).Sum(); // Sum Total Balance
            workSheet.Cells[rowStart, 25].Style.Numberformat.Format = numberFormat;
            workSheet.Cells[rowStart, 27].Value = listData.Select(s => s.AmountObh).Sum(); // Sum Total Amount OBH
            workSheet.Cells[rowStart, 27].Style.Numberformat.Format = numberFormat;

            workSheet.Cells[6, 1, 6, 31].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[7, 1, rowStart, 31].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[7, 1, rowStart, 31].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            workSheet.Cells[rowStart + 2, 1, rowStart + 2, 31].Merge = true;
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
            workSheet.Column(10).Width = 9; //Cột J
            workSheet.Column(11).Width = 14; //Cột K
            workSheet.Column(12).Width = 14; //Cột L
            workSheet.Column(13).Width = 14; //Cột M
            workSheet.Column(14).Width = 14; //Cột N
            workSheet.Column(15).Width = 20; //Cột O
            workSheet.Column(16).Width = 20; //Cột P
            workSheet.Column(17).Width = 15.5; //Cột Q            
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
                "Revenue", //10
                "Cost", //11
                "Profit", //12
                "OBH", //13
                "P.I.C", //14
                "Salesman", //15
                "Service" //16
            };
            workSheet.Cells["A1:Q1"].Style.Font.Bold = true;
            for(int c = 1; c < 18; c++)
            {
                workSheet.Cells[1, c].Value = headers[c - 1];
            }

            int startRow = 2;
            foreach(var item in listData)
            {
                workSheet.Cells[startRow, 1].Value = item.No;
                workSheet.Cells[startRow, 2].Value = item.JobId;
                workSheet.Cells[startRow, 3].Value = item.Mawb;
                workSheet.Cells[startRow, 4].Value = item.Hawb;
                workSheet.Cells[startRow, 5].Value = item.CustomerName;
                workSheet.Cells[startRow, 6].Value = item.CarrierName;
                workSheet.Cells[startRow, 7].Value = item.AgentName;
                workSheet.Cells[startRow, 8].Value = item.ServiceDate;
                workSheet.Cells[startRow, 8].Style.Numberformat.Format = "dd/MM/yyyy";
                workSheet.Cells[startRow, 9].Value = item.Route;

                workSheet.Cells[startRow, 10].Value = item.Qty;
                workSheet.Cells[startRow, 11].Value = item.Revenue;
                workSheet.Cells[startRow, 12].Value = item.Cost;
                workSheet.Cells[startRow, 13].Value = item.Profit;
                workSheet.Cells[startRow, 14].Value = item.Obh;
                for(int i = 10; i < 15; i++)
                {
                    workSheet.Cells[startRow, i].Style.Numberformat.Format = numberFormat;
                }

                workSheet.Cells[startRow, 15].Value = item.PersonInCharge;
                workSheet.Cells[startRow, 16].Value = item.Salesman;
                workSheet.Cells[startRow, 17].Value = item.ServiceName;

                startRow += 1;
            }

            workSheet.Cells[1, 1, startRow - 1, 17].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            workSheet.Cells[1, 1, startRow - 1, 17].Style.Border.Right.Style = ExcelBorderStyle.Thin;
        }
    }
}
