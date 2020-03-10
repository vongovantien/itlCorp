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
                "VẬN ĐƠN GOM HÀNG",
                "(House bill of lading)"
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
            workSheet.Cells["A1"].Value = headers[0];
            FormatTitleHeader(workSheet, "A1", "Calibri");
            workSheet.Cells[1, 1, 1, 6].Merge = true;

            workSheet.Cells[2, 1, 2, 6].Merge = true;
            FormatTitleHeader(workSheet, "A2", "Calibri");
            workSheet.Cells["A2"].Value = headers[1];

            workSheet.Cells["A1"].AutoFitColumns();
            for (int i = 0; i < containerHeaders.Count; i++)
            {
                workSheet.Cells[25, i + 1].Value = containerHeaders[i];
                workSheet.Cells[25, i + 1].Style.WrapText = true;
                BorderThinItem(workSheet, 25, i + 1);

                workSheet.Cells[25, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                workSheet.Cells[25, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Column(i + 1).Width = 32.18;
            }
            short? numberPackage = 0;
            string kindOfPackages = string.Empty;

            int addressStartContent = 26;
            if (transactionDetail.CsMawbcontainers.Count > 0)
            {
                foreach (var item in transactionDetail.CsMawbcontainers)
                {
                    numberPackage = (short?)(numberPackage + item.PackageQuantity);
                    kindOfPackages = item.PackageTypeName != null?(kindOfPackages + item.PackageTypeName + "; "): string.Empty;
                    workSheet.Cells[addressStartContent, 1].Value = item.CommodityName;
                    workSheet.Cells[addressStartContent, 2].Value = item.Description;
                    workSheet.Cells[addressStartContent, 3].Value = item.Gw;
                    workSheet.Cells[addressStartContent, 4].Value = item.Gw;
                    workSheet.Cells[addressStartContent, 5].Value = item.ContainerNo;
                    workSheet.Cells[addressStartContent, 6].Value = item.SealNo;
                    for(int i=0; i<containerHeaders.Count; i++)
                    {
                        BorderThinItem(workSheet, addressStartContent, i + 1);
                        workSheet.Cells[addressStartContent, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if(i==2|| i == 3)
                        {
                            workSheet.Cells[addressStartContent, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        }
                        else
                        {
                            workSheet.Cells[addressStartContent, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                    }
                    addressStartContent = addressStartContent + 1;
                }
            }
            List<ManifestModel> itemInHouses = new List<ManifestModel>()
            {
                new ManifestModel { Title = "Số hồ sơ \nDocument's No", Value = string.Empty },
                new ManifestModel { Title = "Năm đăng ký hồ sơ \nDocument's Year", Value = "2019" },
                new ManifestModel { Title = "Chức năng của chứng từ \nDocument's function", Value = "CN01" },
                new ManifestModel { Title = "Người gửi hàng* \nShipper", Value = transactionDetail.ShipperDescription },
                new ManifestModel { Title = "Người nhận hàng* \nConsignee", Value = transactionDetail.ConsigneeDescription },
                new ManifestModel { Title = "Người được thông báo 1 \nNotify Party 1", Value = transactionDetail.NotifyPartyDescription },
                new ManifestModel { Title = "Người được thông báo 2 \nNotify Party 2", Value = string.Empty },
                new ManifestModel { Title = "Mã Cảng chuyển tải/quá cảnh \nCode of Port of transhipment/transit", Value = string.Empty },
                new ManifestModel { Title = "Mã Cảng giao hàng/cảng đích \nFinal destination", Value = transactionDetail.PODName },
                new ManifestModel { Title = "Mã Cảng xếp hàng \nCode of Port of Loading", Value = transactionDetail.POLName },
                new ManifestModel { Title = "Mã Cảng dỡ hàng \nPort of unloading/discharging", Value = transactionDetail.PODName },
                new ManifestModel { Title = "Địa điểm giao hàng* \nPlace of Delivery", Value = transactionDetail.FinalDestinationPlace },
                new ManifestModel { Title = "Loại hàng* \nCargo Type/Terms of Shipment", Value = transactionDetail.ServiceType },
                new ManifestModel { Title = "Số vận đơn * \nBill of lading number", Value = transactionDetail.Hwbno },
                new ManifestModel { Title = "Ngày phát hành vận đơn* \nDate of house bill of lading", Value = transactionDetail.Eta?.ToShortDateString() },
                new ManifestModel { Title = "Số vận đơn gốc* \nMaster bill of lading number", Value = transactionDetail.Mawb },
                new ManifestModel { Title = "Ngày phát hành vận đơn gốc* \nDate of master bill of lading", Value = transactionDetail.ShipmentEta?.ToShortDateString() },
                new ManifestModel { Title = "Ngày khởi hành* \nDeparture date", Value = transactionDetail.Etd?.ToShortDateString() },
                new ManifestModel { Title = "Tổng số kiện* \nNumber of packages", Value = numberPackage },
                new ManifestModel { Title = "Loại kiện* \nKind of packages", Value = kindOfPackages.Length>0? kindOfPackages.Substring(0, kindOfPackages.Length -2): string.Empty },
                new ManifestModel { Title = "Ghi chú \nRemark", Value = transactionDetail.Remark },
            };

            addressStartContent = 3;
            WriteGeneralManifestInfo(workSheet, itemInHouses, addressStartContent);
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
                workSheet.Cells[13, i + 1].Value = goodsInfos[i];
                workSheet.Cells[13, i + 1].Style.WrapText = true;
                BorderThinItem(workSheet, 13, i + 1);

                workSheet.Cells[13, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                workSheet.Cells[13, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
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
                    workSheet.Cells[addressStartContent, 12].Value = (item.Gw != null && item.Nw!= null)? (item.Gw/item.Nw): null; 
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

            workSheet.Cells["A1"].Value = headers[0];
            FormatTitleHeader(workSheet, "A1", "Calibri");
            workSheet.Cells[1, 1, 1, 22].Merge = true;

            workSheet.Cells["A2"].Value = headers[1];
            FormatTitleHeader(workSheet, "A2", "Calibri");
            workSheet.Cells[2, 1, 2, 22].Merge = true;
            workSheet.Cells["A1"].AutoFitColumns();

            List<String> goodsInfos = new List<String>()
            {
                "Vận đơn số* \nB/L No",
                "Người gửi hàng* \nShipper/Consignor",
                "Người nhận hàng* \nConsignee",
                "Người được thông báo* \nNotify Party",
                "Người được thông báo 2 \nNotify Party 2",
                "Số hiệu cont \nCont's number",
                "Số Seal cont \nSeal number",
                "Mã hàng (nếu có) \nHS code If avail.",
                "Tên hàng/mô tả hàng hóa* \nName, Description of goods",
                "Trọng lượng tịnh* \nNet weight",
                "Tổng trọng lượng* \nGross weight",
                "Kích thước/thể tích* \nDemension /tonnage",
                "Số tham chiếu manifest \nRef. no manifest",
                "Căn cứ hiệu chỉnh \nAjustment basis",
                "Đơn vị tính trọng lượng* \nGrossUnit",
                "Cảng dỡ hàng* \nPort Of Discharge",
                "Cảng đích* \nPort Of Destination",
                "Cảng xếp hàng* \nPort Of Loading",
                "Cảng xếp hàng gốc* \nPort Of OrginalLoading",
                "Cảng trung chuyển* \nPort of Transhipment",
                "Cảng giao hàng/cảng đích* \nFinal Destination",
                "Loại cont* \nCont. type"
            };
            for (int i = 0; i < goodsInfos.Count; i++)
            {
                workSheet.Cells[8, i + 1].Value = goodsInfos[i];
                workSheet.Cells[8, i + 1].Style.WrapText = true;
                BorderThinItem(workSheet, 8, i + 1);

                workSheet.Cells[8, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                workSheet.Cells[8, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Column(i + 1).Width = 32.18;
            }

            int addressStartContent = 9;
            short? numberPackage = 0;
            string kindOfPackages = string.Empty;
            workSheet.Cells["A7"].Value = "THÔNG TIN HÀNG HÓA";
            workSheet.Cells["A7"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells[7, 1, 7, 22].Merge = true;
            if (transactionDetail.CsMawbcontainers.Count > 0)
            {
                foreach (var item in transactionDetail.CsMawbcontainers)
                {
                    numberPackage = (short?)(numberPackage + item.PackageQuantity);
                    kindOfPackages = item.PackageTypeName != null ? (kindOfPackages + item.PackageTypeName + "; ") : string.Empty;
                    workSheet.Cells[addressStartContent, 1].Value = transactionDetail.Hwbno;
                    workSheet.Cells[addressStartContent, 2].Value = transactionDetail.ShipperDescription;
                    workSheet.Cells[addressStartContent, 3].Value = transactionDetail.ConsigneeDescription;
                    workSheet.Cells[addressStartContent, 4].Value = transactionDetail.NotifyPartyDescription;
                    workSheet.Cells[addressStartContent, 5].Value = string.Empty;
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
                    for (int i = 0; i < goodsInfos.Count; i++)
                    {
                        BorderThinItem(workSheet, addressStartContent, i + 1);
                        workSheet.Cells[addressStartContent, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    addressStartContent = addressStartContent + 1;
                }
            }

            List<ManifestModel> itemInHouses = new List<ManifestModel>()
            {
                new ManifestModel { Title = "Số hồ sơ \nDocument's No", Value =  string.Empty },
                new ManifestModel { Title = "Năm đăng ký hồ sơ \nDocument's Year", Value = "2019" },
                new ManifestModel { Title = "Chức năng của chứng từ \nDocument's function", Value = "CN01" },
                new ManifestModel { Title = "Tổng số kiện và loại kiện* \nNumber of packages and Kind of packages", Value = kindOfPackages.Length>0?(numberPackage + ", " + kindOfPackages.Substring(0, kindOfPackages.Length -2)): string.Empty }
            };
            addressStartContent = 3;
            WriteGeneralManifestInfo(workSheet, itemInHouses, addressStartContent);
        }
        
        private void WriteGeneralManifestInfo(ExcelWorksheet workSheet, List<ManifestModel> itemInHouses, int addressStartContent)
        {
            for (int i = 0; i < itemInHouses.Count; i++)
            {
                var item = itemInHouses[i];
                workSheet.Cells[i + addressStartContent, 1].Value = item.Title;
                workSheet.Cells[i + addressStartContent, 1].Style.WrapText = true;
                BorderThinItem(workSheet, i + addressStartContent, 1);
                workSheet.Column(1).Width = 32.18;

                workSheet.Cells[i + addressStartContent, 2].Value = item.Value;
                workSheet.Cells[i + addressStartContent, 2].Style.Font.Bold = true;
                workSheet.Cells[i + addressStartContent, 2].Style.WrapText = true;
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
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateMAWBAirExportExcel(Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("MAWB");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataMAWBAirExportExcel(workSheet);
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

        private void BindingDataMAWBAirExportExcel(ExcelWorksheet workSheet)
        {
            workSheet.View.ShowGridLines = false;

            SetWidthColumnExcelMAWBAirExport(workSheet);
            workSheet.Cells[1, 1, 100000, 14].Style.Font.SetFromFont(new Font("Arial", 10));

            workSheet.Cells["A1:N1"].Style.Font.SetFromFont(new Font("Arial", 12));
            workSheet.Cells["A1:N1"].Style.Font.Bold = true;
            workSheet.Cells["A1"].Value = "157";
            workSheet.Cells["B1"].Value = "SGN";
            workSheet.Cells["C1:E1"].Merge = true;
            workSheet.Cells["C1"].Value = "4591 2506";
            workSheet.Cells["L1"].Value = "157-";
            workSheet.Cells["M1"].Value = "4591 2506";

            workSheet.Cells["A3:A12"].Style.Font.Color.SetColor(Color.DarkBlue);

            workSheet.Cells["A3:A5"].Style.Font.Bold = true;            
            workSheet.Cells["A3"].Value = "INDO TRANS LOGISTICS CORPORATION";
            workSheet.Cells["A4"].Value = "52-54-56 TRUONG SON STR., TAN BINH DIST,";
            workSheet.Cells["A5"].Value = "HOCHIMINH CITY, VIETNAM.";
            workSheet.Cells["A6"].Value = "TEL: +84 8 39486888 FAX: +84 8 8488593";

            workSheet.Cells["K4:N5"].Merge = true;            
            workSheet.Cells["K4"].Style.Font.SetFromFont(new Font("Arial", 12));
            workSheet.Cells["K4"].Value = "QATAR AIRWAYS CARGO";
            workSheet.Cells["K4"].Style.Font.Bold = true;
            workSheet.Cells["K4:N5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["K4:N5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A9:A12"].Style.Font.Bold = true;
            workSheet.Cells["A9"].Value = "BENDIX TRANSPORT DANMARK A/S";
            workSheet.Cells["A10"].Value = "39 JERNHOLMEN, DK-2650 HVIDOVRE, DENMARK";
            workSheet.Cells["A11"].Value = "TEL: +45 36772244";
            workSheet.Cells["A12"].Value = "ATTN: MICHAEL BOMARK";

            workSheet.Cells["J17:M17"].Merge = true;
            workSheet.Cells["J17"].Value = "PP IN SGN";

            workSheet.Cells["A19"].Style.Font.SetFromFont(new Font("Calibri", 12));
            workSheet.Cells["A19"].Style.Font.Bold = true;
            workSheet.Cells["A19"].Value = "373-0118";

            workSheet.Cells["A21"].Value = "HOCHIMINH AIRPORT";
            workSheet.Cells["A21"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Row(21).Height = 24.75;

            workSheet.Cells["A22"].Value = "DOH";
            workSheet.Cells["B22"].Value = "QR";
            workSheet.Cells["F22:G22"].Merge = true;
            workSheet.Cells["F22"].Value = "CPH QR";
            workSheet.Cells["I22"].Value = "USD";
            workSheet.Cells["L22"].Value = "NVD";
            workSheet.Cells["M22"].Value = "NVC";
            workSheet.Cells["M22"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            workSheet.Cells["A24"].Value = "COPENHAGEN";
            workSheet.Cells["E24"].Value = "QR0971";
            workSheet.Cells["F24:H24"].Merge = true;
            workSheet.Cells["F24"].Value = "23/01/2020";
            workSheet.Cells["I24"].Value = "NIL";

            workSheet.Cells["A27"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["A27"].Style.Font.Bold = true;
            workSheet.Cells["A27"].Value = 1;
            workSheet.Cells["B27"].Value = "HAWB";

            workSheet.Cells["A30:I30"].Style.Font.Bold = true;
            workSheet.Cells["A30:E30"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["A30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["A30"].Value = 1;
            workSheet.Cells["B30:C30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["B30:C30"].Merge = true;
            workSheet.Cells["B30"].Value = 118;
            workSheet.Cells["B30"].Style.Numberformat.Format = numberFormatKgs;
            workSheet.Cells["E30:F30"].Merge = true;
            workSheet.Cells["E30"].Value = 209;
            workSheet.Cells["E30"].Style.Numberformat.Format = numberFormatKgs;
            workSheet.Cells["G30:J30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["G30:H30"].Merge = true;
            workSheet.Cells["G30"].Value = 9.85;
            workSheet.Cells["G30"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["I30"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["I30:J30"].Merge = true;
            workSheet.Cells["I30"].Value = 2058.65;
            workSheet.Cells["I30"].Style.Numberformat.Format = numberFormat;

            workSheet.Cells["A31"].Value = "PCS";
            workSheet.Cells["L31:N39"].Merge = true;
            workSheet.Cells["L31:N39"].Style.WrapText = true;
            workSheet.Cells["L31:N39"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["L31"].Value = "CONSOL CARGO AND DOCS ATT´D";

            workSheet.Cells["A40"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["A40"].Value = 1;
            workSheet.Cells["B40"].Value = 118;

            workSheet.Cells["A44:B44"].Merge = true;
            workSheet.Cells["A44:B44"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A44"].Value = 2058.65;
            workSheet.Cells["A44"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["H44"].Value = "AWB";
            workSheet.Cells["J44"].Value = 5.00;
            workSheet.Cells["J44"].Style.Numberformat.Format = numberFormat;

            workSheet.Cells["H45"].Value = "MCC";
            workSheet.Cells["J45"].Value = 2.01;
            workSheet.Cells["J45"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["H46"].Value = "CGC";
            workSheet.Cells["J46"].Value = 5.00;
            workSheet.Cells["J46"].Style.Numberformat.Format = numberFormat;

            workSheet.Cells["A51:B51"].Merge = true;
            workSheet.Cells["A51:B51"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A51"].Value = 12.01;
            workSheet.Cells["A51"].Style.Numberformat.Format = numberFormat;

            workSheet.Cells["A55:B55"].Merge = true;
            workSheet.Cells["A55"].Value = 2070.66;
            workSheet.Cells["A55"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["A55"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["G55:L55"].Merge = true;
            workSheet.Cells["G55"].Value = "HOCHIMINH CITY 21 JAN 2020";
            workSheet.Cells["G55:L55"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Row(57).Height = 42.75;
            workSheet.Cells["L57:M57"].Style.Font.Bold = true;
            workSheet.Cells["L57:M57"].Style.Font.Size = 12;
            workSheet.Cells["L57"].Value = "157-";
            workSheet.Cells["M57"].Value = "4591 2506";
        }

        /// <summary>
        /// Generate HAWBW Air Export Excel
        /// </summary>
        /// <param name="transactionDetail"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateHAWBAirExportExcel(CsTransactionDetailModel transactionDetail, Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("HAWB");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    BindingDataHAWBAirExportExcel(workSheet, transactionDetail);
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

        private void BindingDataHAWBAirExportExcel(ExcelWorksheet workSheet, CsTransactionDetailModel transactionDetail)
        {
            workSheet.View.ShowGridLines = false;

            SetWidthColumnExcelHAWBAirExport(workSheet);

            workSheet.Cells[1, 1, 100000, 14].Style.Font.SetFromFont(new Font("Arial", 10));

            workSheet.Cells["A1:N1"].Style.Font.SetFromFont(new Font("Arial", 12));
            workSheet.Cells["A1:N1"].Style.Font.Bold = true;
            workSheet.Cells["A1"].Value = "157";
            workSheet.Cells["B1"].Value = "SGN";
            workSheet.Cells["C1:E1"].Merge = true;
            workSheet.Cells["C1"].Value = "4591 2506";
            workSheet.Cells["L1"].Value = "ITL";
            workSheet.Cells["M1"].Value = "79398049";

            workSheet.Cells["A3:A12"].Style.Font.Color.SetColor(Color.DarkBlue);

            workSheet.Cells["A3:A5"].Style.Font.Bold = true;
            workSheet.Cells["A3"].Value = "SHIPPER";
            workSheet.Cells["A4"].Value = "52-54-56 TRUONG SON STR., TAN BINH DIST,";
            workSheet.Cells["A5"].Value = "HOCHIMINH CITY, VIETNAM.";
            workSheet.Cells["A6"].Value = "TEL: +84 8 39486888 FAX: +84 8 8488593";

            workSheet.Cells["K4:N5"].Merge = true;
            workSheet.Cells["K4"].Style.Font.SetFromFont(new Font("Arial", 12));
            workSheet.Cells["K4"].Value = "IN DO TRANS LOGISTICS";
            workSheet.Cells["K4"].Style.Font.Bold = true;
            workSheet.Cells["K4:N5"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["K4:N5"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            workSheet.Cells["A9:A12"].Style.Font.Bold = true;
            workSheet.Cells["A9"].Value = "CNEE";
            workSheet.Cells["A10"].Value = "39 JERNHOLMEN, DK-2650 HVIDOVRE, DENMARK";
            workSheet.Cells["A11"].Value = "TEL: +45 36772244";
            workSheet.Cells["A12"].Value = "ATTN: MICHAEL BOMARK";

            workSheet.Cells["J17:M17"].Merge = true;
            workSheet.Cells["J17"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["J17"].Value = "PP IN SGN";

            workSheet.Cells["A19"].Style.Font.SetFromFont(new Font("Calibri", 12));
            workSheet.Cells["A19"].Style.Font.Bold = true;
            workSheet.Cells["A19"].Value = "373-0118";

            workSheet.Cells["A21"].Value = "HOCHIMINH AIRPORT";
            workSheet.Cells["A21"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Row(21).Height = 24.75;

            workSheet.Cells["A22"].Value = "DOH";
            workSheet.Cells["B22"].Value = "QR";
            workSheet.Cells["F22:G22"].Merge = true;
            workSheet.Cells["F22"].Value = "CPH QR";
            workSheet.Cells["I22"].Value = "USD";
            workSheet.Cells["L22"].Value = "NVD";
            workSheet.Cells["M22"].Value = "NVC";
            workSheet.Cells["M22"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

            workSheet.Cells["A24"].Value = "COPENHAGEN";
            workSheet.Cells["E24"].Value = "QR0971";
            workSheet.Cells["F24:H24"].Merge = true;
            workSheet.Cells["F24"].Value = "23/01/2020";
            workSheet.Cells["I24"].Value = "NIL";
            
            workSheet.Cells["A30:I30"].Style.Font.Bold = true;
            workSheet.Cells["A30:E30"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["A30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["A30"].Value = 1;
            workSheet.Cells["B30:C30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["B30:C30"].Merge = true;
            workSheet.Cells["B30"].Value = 118;
            workSheet.Cells["B30"].Style.Numberformat.Format = numberFormatKgs;
            workSheet.Cells["E30:F30"].Merge = true;
            workSheet.Cells["E30"].Value = 209;
            workSheet.Cells["E30"].Style.Numberformat.Format = numberFormatKgs;
            workSheet.Cells["G30:J30"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["G30:H30"].Merge = true;
            workSheet.Cells["G30"].Value = "AS AGREED";
            workSheet.Cells["G30"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["I30"].Style.Font.Color.SetColor(Color.Red);
            workSheet.Cells["I30:J30"].Merge = true;
            workSheet.Cells["I30"].Value = "AS AGREED";
            workSheet.Cells["I30"].Style.Numberformat.Format = numberFormat;

            workSheet.Cells["A31"].Value = "PCS";
            workSheet.Cells["L31:N39"].Merge = true;
            workSheet.Cells["L31:N39"].Style.WrapText = true;
            workSheet.Cells["L31:N39"].Style.VerticalAlignment = ExcelVerticalAlignment.Top;
            workSheet.Cells["L31"].Value = "INJECTION MOULD";

            workSheet.Cells["A40"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            workSheet.Cells["A40"].Value = 1;
            workSheet.Cells["B40"].Value = 118;

            workSheet.Cells["A44:B44"].Merge = true;
            workSheet.Cells["A44:B44"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A44"].Value = "AS AGREED";
            workSheet.Cells["A44"].Style.Numberformat.Format = numberFormat;            

            workSheet.Cells["A51:B51"].Merge = true;
            workSheet.Cells["A51:B51"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A51"].Value = 0;

            workSheet.Cells["A55:B55"].Merge = true;
            workSheet.Cells["A55"].Value = "AS AGREED";
            workSheet.Cells["A55"].Style.Numberformat.Format = numberFormat;
            workSheet.Cells["A55"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Cells["G55:L55"].Merge = true;
            workSheet.Cells["G55"].Value = "HOCHIMINH CITY 21 JAN 2020";
            workSheet.Cells["G55:L55"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            workSheet.Row(57).Height = 42.75;
            workSheet.Cells["L57:M57"].Style.Font.Bold = true;
            workSheet.Cells["L57:M57"].Style.Font.Size = 12;
            workSheet.Cells["L57"].Value = "ITL";
            workSheet.Cells["M57"].Value = "79398049";
        }
        #endregion --- MAWB and HAWB Air Export Excel ---

    }
}
