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

            List<string> houseTitles = new List<string>()
            {
                "STT",
                "No",
                "Số hồ sơ ",
                "Document's No",
                "Năm đăng ký hồ sơ ",
                "Document's Year",
                "Chức năng của chứng từ ",
                "Document's function",
                "Người gửi hàng* ",
                "Shipper",
                "Người nhận hàng* ",
                "Consignee",
                "Người được thông báo 1",
                "Notify Party 1",
                "Người được thông báo 2",
                "Notify Party 2",
                "Mã Cảng chuyển tải/ quá cảnh"
            };
            int addressStartContent = 3;
            int k = 0;
            for (int i = 0; i < houseTitles.Count - 1; i = i + 2)
            {
                int indexColAddress = 0;
                if (i == 0)
                {
                    indexColAddress = 1;
                }
                if (i > 0)
                {
                    indexColAddress = i - k;
                    k = k + 1;
                }
                workSheet.Cells[addressStartContent, indexColAddress].Style.WrapText = true;
                workSheet.Cells[addressStartContent, indexColAddress].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[addressStartContent, indexColAddress].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[addressStartContent, indexColAddress].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                workSheet.Cells[addressStartContent, indexColAddress].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[addressStartContent, indexColAddress].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));
                BorderDashItem(workSheet, addressStartContent, indexColAddress);

                ExcelRichText ert = workSheet.Cells[addressStartContent, indexColAddress].RichText.Add(houseTitles[i]);
                ert.Color = System.Drawing.Color.Red;
                ert = workSheet.Cells[addressStartContent, indexColAddress].RichText.Add(" \n" + houseTitles[i + 1]);
                ert.Color = System.Drawing.Color.Black;
                workSheet.Column(indexColAddress).Width = 25;
            }

            workSheet.Cells[4, 1].Value = 01;
            workSheet.Cells[4, 3].Value = DateTime.Now.Year;
            workSheet.Cells[4, 4].Value = "CN01";
            workSheet.Cells[4, 5].Value = transactionDetail.ShipperDescription;
            workSheet.Cells[4, 6].Value = transactionDetail.ConsigneeDescription;
            workSheet.Cells[4, 7].Value = transactionDetail.NotifyPartyDescription;
            workSheet.Cells[4, 8].Value = transactionDetail.AlsoNotifyPartyDescription;
            List<TitleModel> containerTitles = new List<TitleModel>()
            {
                new TitleModel { VNTitle = "Mã hàng", ENTitle = "HS code if avail" },
                new TitleModel { VNTitle = "Mô tả hàng hóa", ENTitle = "Description of Goods" },
                new TitleModel { VNTitle = "Tổng trọng lượng*", ENTitle = "Gross Weight" },
                new TitleModel { VNTitle = "Kích thước/ thể tích*", ENTitle = "Demension/ tonnage" },
                new TitleModel { VNTitle = "Số hiệu cont", ENTitle = "Cont. number"},
                new TitleModel { VNTitle = "Số seal cont", ENTitle = "Seal number"}
            };
            //for (int i = 0; i < containerHeaders.Count; i++)
            //{
            //    workSheet.Cells[25, i + 1].Value = containerHeaders[i];
            //    workSheet.Cells[25, i + 1].Style.WrapText = true;
            //    BorderThinItem(workSheet, 25, i + 1);

            //    workSheet.Cells[25, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //    workSheet.Cells[25, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //    workSheet.Column(i + 1).Width = 32.18;
            //}
            //short? numberPackage = 0;
            //string kindOfPackages = string.Empty;

            //int addressStartContent = 26;
            //if (transactionDetail.CsMawbcontainers.Count > 0)
            //{
            //    foreach (var item in transactionDetail.CsMawbcontainers)
            //    {
            //        numberPackage = (short?)(numberPackage + item.PackageQuantity);
            //        kindOfPackages = item.PackageTypeName != null ? (kindOfPackages + item.PackageTypeName + "; ") : string.Empty;
            //        workSheet.Cells[addressStartContent, 1].Value = item.CommodityName;
            //        workSheet.Cells[addressStartContent, 2].Value = item.Description;
            //        workSheet.Cells[addressStartContent, 3].Value = item.Gw;
            //        workSheet.Cells[addressStartContent, 4].Value = item.Gw;
            //        workSheet.Cells[addressStartContent, 5].Value = item.ContainerNo;
            //        workSheet.Cells[addressStartContent, 6].Value = item.SealNo;
            //        for (int i = 0; i < containerHeaders.Count; i++)
            //        {
            //            BorderThinItem(workSheet, addressStartContent, i + 1);
            //            workSheet.Cells[addressStartContent, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            //            if (i == 2 || i == 3)
            //            {
            //                workSheet.Cells[addressStartContent, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            //            }
            //            else
            //            {
            //                workSheet.Cells[addressStartContent, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            //            }
            //        }
            //        addressStartContent = addressStartContent + 1;
            //    }
            //}
            //List<ManifestModel> itemInHouses = new List<ManifestModel>()
            //{
            //    new ManifestModel { Title = "Số hồ sơ \nDocument's No", Value = string.Empty },
            //    new ManifestModel { Title = "Năm đăng ký hồ sơ \nDocument's Year", Value = "2019" },
            //    new ManifestModel { Title = "Chức năng của chứng từ \nDocument's function", Value = "CN01" },
            //    new ManifestModel { Title = "Người gửi hàng* \nShipper", Value = transactionDetail.ShipperDescription },
            //    new ManifestModel { Title = "Người nhận hàng* \nConsignee", Value = transactionDetail.ConsigneeDescription },
            //    new ManifestModel { Title = "Người được thông báo 1 \nNotify Party 1", Value = transactionDetail.NotifyPartyDescription },
            //    new ManifestModel { Title = "Người được thông báo 2 \nNotify Party 2", Value = string.Empty },
            //    new ManifestModel { Title = "Mã Cảng chuyển tải/quá cảnh \nCode of Port of transhipment/transit", Value = string.Empty },
            //    new ManifestModel { Title = "Mã Cảng giao hàng/cảng đích \nFinal destination", Value = transactionDetail.PODName },
            //    new ManifestModel { Title = "Mã Cảng xếp hàng \nCode of Port of Loading", Value = transactionDetail.POLName },
            //    new ManifestModel { Title = "Mã Cảng dỡ hàng \nPort of unloading/discharging", Value = transactionDetail.PODName },
            //    new ManifestModel { Title = "Địa điểm giao hàng* \nPlace of Delivery", Value = transactionDetail.FinalDestinationPlace },
            //    new ManifestModel { Title = "Loại hàng* \nCargo Type/Terms of Shipment", Value = transactionDetail.ServiceType },
            //    new ManifestModel { Title = "Số vận đơn * \nBill of lading number", Value = transactionDetail.Hwbno },
            //    new ManifestModel { Title = "Ngày phát hành vận đơn* \nDate of house bill of lading", Value = transactionDetail.Eta?.ToShortDateString() },
            //    new ManifestModel { Title = "Số vận đơn gốc* \nMaster bill of lading number", Value = transactionDetail.Mawb },
            //    new ManifestModel { Title = "Ngày phát hành vận đơn gốc* \nDate of master bill of lading", Value = transactionDetail.ShipmentEta?.ToShortDateString() },
            //    new ManifestModel { Title = "Ngày khởi hành* \nDeparture date", Value = transactionDetail.Etd?.ToShortDateString() },
            //    new ManifestModel { Title = "Tổng số kiện* \nNumber of packages", Value = numberPackage },
            //    new ManifestModel { Title = "Loại kiện* \nKind of packages", Value = kindOfPackages.Length>0? kindOfPackages.Substring(0, kindOfPackages.Length -2): string.Empty },
            //    new ManifestModel { Title = "Ghi chú \nRemark", Value = transactionDetail.Remark },
            //};

            //addressStartContent = 3;
            //WriteGeneralManifestInfo(workSheet, itemInHouses, addressStartContent);
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

            workSheet.Cells["A1"].Value = headers[0];
            FormatTitleHeader(workSheet, "A1", "Calibri");
            workSheet.Cells[1, 1, 1, 16].Merge = true;

            workSheet.Cells["A2"].Value = headers[1];
            FormatTitleHeader(workSheet, "A2", "Calibri");
            workSheet.Cells[2, 1, 2, 16].Merge = true;
            workSheet.Cells["A1"].AutoFitColumns();

            List<ManifestModel> itemInHouses = new List<ManifestModel>()
            {
                new ManifestModel { Title = "Số hồ sơ \nDocument's No", Value =  string.Empty },
                new ManifestModel { Title = "Năm đăng ký hồ sơ \nDocument's Year", Value = "2019" },
                new ManifestModel { Title = "Chức năng của chứng từ \nDocument's function", Value = "CN01" },
                new ManifestModel { Title = "Cảng nhận hàng* \nPort of Loading", Value = transactionDetail.POLName },
                new ManifestModel { Title = "Cảng trả hàng* \nPort of discharge", Value = transactionDetail.PODName },
                new ManifestModel { Title = "Thông tin bổ sung \nAdditional Remark", Value = string.Empty },
                new ManifestModel { Title = "Nơi ký \nSign place", Value = string.Empty },
                new ManifestModel { Title = "Ngày ký \nSign date", Value = string.Empty },
                new ManifestModel { Title = "Người ký \nMaster signed", Value = string.Empty}
            };
            int addressStartContent = 3;
            WriteGeneralManifestInfo(workSheet, itemInHouses, addressStartContent);
            List<String> goodsInfos = new List<String>() {
                "Số vận đơn* \nBooking/reference number",
                "Kí hiệu container* \nMarks",
                "Số bao kiện* \nNumber package",
                "Loại bao kiện * \nKind of packages",
                "Cty vận chuyển* \nTransporter's name",
                "Loại hàng hóa* \nClass",
                "Số UN* \nUN number",
                "Nhóm hàng* \nPacking group",
                "Nhóm phụ số* \nSubsidiary risk(s)",
                "Điểm bốc cháy* \nFlash point (In oC, c.c)",
                "Ô nhiễm biển* \nMarine pollutant",
                "Tổng khối lượng* \nMass (kg) Gross/Net",
                "Vị trí xếp hàng* \nStowage position on board",
                "Số hiệu container* \nContainer number",
                "Số seal container* \nContainer seal number",
                "Số container* \nNumber Container"
            };
            for (int i = 0; i < goodsInfos.Count; i++)
            {
                workSheet.Cells[14, i + 1].Value = goodsInfos[i];
                workSheet.Cells[14, i + 1].Style.WrapText = true;
                BorderThinItem(workSheet, 14, i + 1);

                workSheet.Cells[14, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                workSheet.Cells[14, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Column(i + 1).Width = 32.18;
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
            List<ManifestModel> itemInHouses = new List<ManifestModel>()
            {
                new ManifestModel { Title = "Số hồ sơ "},
                new ManifestModel { Title = "\nDocument's No"},
                new ManifestModel { Title = "Năm đăng ký hồ sơ "},
                new ManifestModel { Title = "\nDocument's Year"},
                new ManifestModel { Title = "Chức năng của chứng từ "},
                new ManifestModel { Title = "\nDocument's function"},
                new ManifestModel { Title = "Tổng số kiện* "},
                new ManifestModel { Title = "\nNumber of packages"},
                new ManifestModel { Title = "Loại kiện* "},
                new ManifestModel { Title = "\nKind of packages"}
            };
            int addressStartContent = 3;
            for (int i = 0; i < itemInHouses.Count -1; i = i +2)
            {
                int indexAddress = 0;
                if(i == 0)
                {
                    indexAddress = i + addressStartContent;
                }
                else
                {
                    indexAddress = i - (i / 2) + addressStartContent;
                }
                workSheet.Column(1).Width = 25;
                workSheet.Cells[indexAddress, 1].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                workSheet.Cells[indexAddress, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[indexAddress, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));
                workSheet.Cells[indexAddress, 1].IsRichText = true;
                workSheet.Cells[indexAddress, 1].Style.Font.Color.SetColor(Color.Red);
                ExcelRichText ert = workSheet.Cells[indexAddress, 1].RichText.Add(itemInHouses[i].Title);
                ert.Color = System.Drawing.Color.Red;
                ert = workSheet.Cells[indexAddress, 1].RichText.Add(itemInHouses[i + 1].Title);
                ert.Color = System.Drawing.Color.Black;
                workSheet.Cells[indexAddress, 1].Style.WrapText = true;
                BorderThinItem(workSheet, indexAddress, 1);

                workSheet.Cells[indexAddress, 2].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                Color coVlauelFromHex = System.Drawing.ColorTranslator.FromHtml("#ffff00");
                if (i == 4 || i == 8)
                {
                    coVlauelFromHex = System.Drawing.ColorTranslator.FromHtml("#ffd966");
                }
                else
                {
                    if(i == 2)
                    {
                        workSheet.Cells[indexAddress, 2].Value = DateTime.Now.Year;
                    }
                }
                workSheet.Cells[indexAddress, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[indexAddress, 2].Style.Fill.BackgroundColor.SetColor(coVlauelFromHex);
                BorderThinItem(workSheet, indexAddress, 2);
            }
            workSheet.Cells["A8"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A8"].Value = headers[1];
            FormatTitleHeader(workSheet, "A8", "Times New Roman");
            workSheet.Cells[8, 1, 8, 23].Style.Fill.PatternType = ExcelFillStyle.Solid;
            workSheet.Cells[8, 1, 8, 23].Style.Fill.BackgroundColor.SetColor(colTitleFromHex);
            workSheet.Cells[8, 1, 8, 23].Merge = true;
            workSheet.Cells["A8"].Value = "THÔNG TIN HÀNG HÓA";
            List<String> goodsInfos = new List<String>()
            {
                "Vận đơn số* ",
                "\nB/L No",
                "Người gửi hàng* ",
                "\nShipper/Consignor",
                "Người nhận hàng* ",
                "\nConsignee",
                "Người được thông báo* ",
                "\nNotify Party",
                "Người được thông báo 2 ",
                "\nNotify Party 2",
                "Số hiệu cont ",
                "\nCont's number",
                "Số Seal cont ",
                "\nSeal number",
                "Mã hàng (nếu có) ",
                "\nHS code If avail.",
                "Tên hàng/mô tả hàng hóa* ",
                "\nName, Description of goods",
                "Trọng lượng tịnh* ",
                "\nNet weight",
                "Tổng trọng lượng* ",
                "\nGross weight",
                "Kích thước/thể tích* ",
                "\nDemension /tonnage",
                "Số tham chiếu manifest ",
                "\nRef. no manifest",
                "Căn cứ hiệu chỉnh ",
                "\nAjustment basis",
                "Đơn vị tính trọng lượng* ",
                "\nGrossUnit",
                "Cảng dỡ hàng* ",
                "\nPort Of Discharge",
                "Cảng đích* ",
                "\nPort Of Destination",
                "Cảng xếp hàng* ",
                "\nPort Of Loading",
                "Cảng xếp hàng gốc* ",
                "\nPort Of OrginalLoading",
                "Cảng trung chuyển* ",
                "\nPort of Transhipment",
                "Cảng giao hàng/cảng đích* ",
                "\nFinal Destination",
                "Loại cont* ",
                "\nCont. type",
                "Đơn vị thể tích ",
                "\nDimension of unit"
            };
            addressStartContent = 9;
            int k = 0;
            for (int i = 0; i < goodsInfos.Count-1; i = i+2)
            {
                int indexColAddress = 0;
                if(i == 0)
                {
                    indexColAddress = 1;
                }
                if (i> 0)
                {
                    indexColAddress = i - k;
                    k = k + 1;
                }
                workSheet.Cells[addressStartContent, indexColAddress].Style.WrapText = true;
                workSheet.Cells[addressStartContent, indexColAddress].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells[addressStartContent, indexColAddress].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells[addressStartContent, indexColAddress].Style.Font.SetFromFont(new Font("Times New Roman", 12));
                workSheet.Cells[addressStartContent, indexColAddress].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[addressStartContent, indexColAddress].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));
                BorderDashItem(workSheet, addressStartContent, indexColAddress);

                ExcelRichText ert = workSheet.Cells[addressStartContent, indexColAddress].RichText.Add(goodsInfos[i]);
                ert.Color = System.Drawing.Color.Red;
                ert = workSheet.Cells[addressStartContent, indexColAddress].RichText.Add(goodsInfos[i + 1]);
                ert.Color = System.Drawing.Color.Black;
                workSheet.Column(indexColAddress).Width = 25;
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

        private void WriteGeneralManifestInfo(ExcelWorksheet workSheet, List<ManifestModel> itemInHouses, int addressStartContent)
        {
            for (int i = 0; i < itemInHouses.Count; i++)
            {
                var item = itemInHouses[i];
                workSheet.Cells[i + addressStartContent, 1].Value = item.Title;
                workSheet.Cells[i + addressStartContent, 1].Style.WrapText = true;
                BorderThinItem(workSheet, i + addressStartContent, 1);
                workSheet.Cells[i + addressStartContent, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[i + addressStartContent, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#d9e1f2"));
                workSheet.Column(1).Width = 32.18;
                workSheet.Cells[i + addressStartContent, 1].Style.Font.SetFromFont(new Font("Times New Roman", 12));

                workSheet.Cells[i + addressStartContent, 2].Value = item.Value;
                workSheet.Cells[i + addressStartContent, 2].Style.Font.Bold = true;
                workSheet.Cells[i + addressStartContent, 2].Style.WrapText = true;
                
                if(i == 0 || i == 1 || i == 3)
                {
                    Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#ffff00");
                    workSheet.Cells[i + addressStartContent, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 2].Style.Fill.BackgroundColor.SetColor(colFromHex);
                }
                else
                {
                    Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#ffd966");
                    workSheet.Cells[i + addressStartContent, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    workSheet.Cells[i + addressStartContent, 2].Style.Fill.BackgroundColor.SetColor(colFromHex);
                }
                BorderThinItem(workSheet, i + addressStartContent, 2);
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
