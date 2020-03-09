using eFMS.API.ReportData.Models.Documentation;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.FormatExcel
{
    public class DocumentationHelper
    {
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
                    //BindingDataDetailSettlementPaymentExcel(workSheet, settlementExport, language);
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
            workSheet.Column(1); //Cột A
            workSheet.Column(2); //Cột B
            workSheet.Column(3); //Cột C
            workSheet.Column(4); //Cột D
            workSheet.Column(5); //Cột E
            workSheet.Column(6); //Cột F
            workSheet.Column(7); //Cột G
            workSheet.Column(8); //Cột H
            workSheet.Column(9); //Cột I
            workSheet.Column(10); //Cột J
            workSheet.Column(11); //Cột K
            workSheet.Column(12); //Cột L
            workSheet.Column(13); //Cột M
            workSheet.Column(14); //Cột N
        }

        private void BindingDataMAWBAirExportExcel(ExcelWorksheet workSheet)
        {
            SetWidthColumnExcelMAWBAirExport(workSheet);
        }

        /// <summary>
        /// Generate HAWBW Air Export Excel
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public Stream GenerateHAWBAirExportExcel(Stream stream = null)
        {
            try
            {
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("HAWB");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    //BindingDataDetailSettlementPaymentExcel(workSheet, settlementExport, language);
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

        }

        private void BindingDataHAWBAirExportExcel(ExcelWorksheet workSheet)
        {
            SetWidthColumnExcelHAWBAirExport(workSheet);
        }
        #endregion --- MAWB and HAWB Air Export Excel ---

    }
}
