using eFMS.API.ReportData.Models.Documentation;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace eFMS.API.ReportData.FormatExcel
{
    public class DocumentationHelper
    {
        const string numberFormat = "#,##0.00";
        const string numberFormatKgs = "#,##0 \"KGS\"";

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

        private void BindingDataMAWBAirExportExcel(ExcelWorksheet workSheet, AirwayBillExportResult airwayBillExport)
        {
            workSheet.View.ShowGridLines = false;

            SetWidthColumnExcelMAWBAirExport(workSheet);
            workSheet.Cells[1, 1, 100000, 14].Style.Font.SetFromFont(new Font("Arial", 10));

            var _mawb1 = airwayBillExport.MawbNo.Substring(0, 3).ToUpper(); //3 ký tự đầu của MAWB
            var _mawb2 = airwayBillExport.MawbNo.Substring(3, airwayBillExport.MawbNo.Length - 3).ToUpper(); //Các ký tự cuối của MAWB

            workSheet.Cells["A1:N1"].Style.Font.SetFromFont(new Font("Arial", 12));
            workSheet.Cells["A1:N1"].Style.Font.Bold = true;
            workSheet.Cells["A1"].Value = _mawb1; //3 ký tự đầu của MAWB
            workSheet.Cells["B1"].Value = airwayBillExport.AolCode?.ToUpper(); //Mã cảng đi
            workSheet.Cells["C1:E1"].Merge = true;
            workSheet.Cells["C1"].Value = _mawb2; //Các ký tự cuối của MAWB
            workSheet.Cells["L1"].Value = _mawb1 + "-"; //3 ký tự đầu của MAWB
            workSheet.Cells["M1"].Value = _mawb2; //Các ký tự cuối của MAWB

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
            workSheet.Cells["E24"].Value = airwayBillExport.FlightNo.ToUpper() ?? string.Empty; //Tên chuyến bay
            workSheet.Cells["F24:H24"].Merge = true;
            workSheet.Cells["F24:H24"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["F24"].Value = airwayBillExport.FlightDate; //Ngày bay
            workSheet.Cells["F24"].Style.Numberformat.Format = "dd-MMM-yyyy";
            workSheet.Cells["I24"].Value = airwayBillExport.IssuranceAmount?.ToUpper();

            workSheet.Cells["A27"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["A27"].Style.Font.Bold = true;
            workSheet.Cells["A27"].Value = 1;
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
                workSheet.Cells["H" + (k + i)].Value = airwayBillExport.OtherCharges[i].ChargeName?.ToUpper();
                workSheet.Cells["J" + (k + i)].Value = airwayBillExport.OtherCharges[i].Amount;
                workSheet.Cells["J" + (k + i)].Style.Numberformat.Format = numberFormat;
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
            workSheet.Cells["L" + (k + 2)].Value = _mawb1 + "-";
            workSheet.Cells["M" + (k + 2)].Value = _mawb2;
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

            var _mawb1 = airwayBillExport.MawbNo.Substring(0, 3).ToUpper(); //3 ký tự đầu của MAWB
            var _mawb2 = airwayBillExport.MawbNo.Substring(3, airwayBillExport.MawbNo.Length - 3).ToUpper(); //Các ký tự cuối của MAWB
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
            workSheet.Cells["L31"].Value = airwayBillExport.DesOfGood?.ToUpper() + "\r\n" + airwayBillExport.VolumeField;

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

    }
}
