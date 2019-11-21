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
                    BindingEManifestExcel(workSheet, transactionDetail);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private void BindingEManifestExcel(ExcelWorksheet workSheet, CsTransactionDetailModel transactionDetail)
        {
            List<String> headers = new List<String>()
            {
                "VẬN ĐƠN GOM HÀNG",
                "(House bill of lading)"
            };
            List<String> containerHeaders = new List<String>()
            {
                "Mã hàng \n HS code if avail",
                "Mô tả hàng hóa* \n Description of Goods",
                "Tổng trọng lượng* \n Gross weight",
                "Kích thước/thể tích * \n Demension/tonnage",
                "Số hiệu cont \n Cont. number",
                "Số Seal cont \n Seal number"
            };
            List<EManifestModel> itemInHouses = new List<EManifestModel>()
            {
                new EManifestModel { Title = "Số hồ sơ \nDocument's No", Value = transactionDetail.Hwbno },
                new EManifestModel { Title = "Năm đăng ký hồ sơ \nDocument's Year", Value = "2019" },
                new EManifestModel { Title = "Chức năng của chứng từ \nDocument's function", Value = "CN01" },
                new EManifestModel { Title = "Người gửi hàng* \nShipper", Value = transactionDetail.ShipperDescription },
                new EManifestModel { Title = "Người nhận hàng* \nConsignee", Value = transactionDetail.ConsigneeDescription },
                new EManifestModel { Title = "Người được thông báo 1 \nNotify Party 1", Value = transactionDetail.NotifyPartyDescription },
                new EManifestModel { Title = "Người được thông báo 2 \nNotify Party 2", Value = string.Empty },
                new EManifestModel { Title = "Mã Cảng chuyển tải/quá cảnh \nCode of Port of transhipment/transit", Value = string.Empty },
                new EManifestModel { Title = "Mã Cảng giao hàng/cảng đích \nFinal destination", Value = transactionDetail.PODName },
                new EManifestModel { Title = "Mã Cảng xếp hàng \nCode of Port of Loading", Value = transactionDetail.POLName },
                new EManifestModel { Title = "Mã Cảng dỡ hàng \nPort of unloading/discharging", Value = transactionDetail.PODName },
                new EManifestModel { Title = "Địa điểm giao hàng* \nPlace of Delivery", Value = transactionDetail.FinalDestinationPlace },
                new EManifestModel { Title = "Loại hàng* \nCargo Type/Terms of Shipment", Value = transactionDetail.ServiceType },
                new EManifestModel { Title = "Số vận đơn * \nBill of lading number", Value = transactionDetail.Hwbno },
                new EManifestModel { Title = "Ngày phát hành vận đơn* \nDate of house bill of lading", Value = transactionDetail.Etd.ToString() },
                new EManifestModel { Title = "Số vận đơn gốc* \nMaster bill of lading number", Value = transactionDetail.Mawb },
                new EManifestModel { Title = "Ngày phát hành vận đơn gốc* \nDate of master bill of lading", Value = string.Empty },
                new EManifestModel { Title = "Ngày khởi hành* \nDeparture date", Value = transactionDetail.Eta.ToString() },
                new EManifestModel { Title = "Tổng số kiện* \nNumber of packages", Value = string.Empty },
                new EManifestModel { Title = "Loại kiện* \nKind of packages", Value = string.Empty },
                new EManifestModel { Title = "Ghi chú \nRemark", Value = string.Empty },
            };
            workSheet.Cells[1, 1, 1, 6].Merge = true;
            workSheet.Cells["A1"].Value = headers[0];
            workSheet.Cells["A1"].Style.Font.Bold = true;
            workSheet.Cells["A1"].Style.Font.SetFromFont(new Font("Calibri", 12));
            workSheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A1"].Style.WrapText = true;

            workSheet.Cells[2, 1, 2, 6].Merge = true;
            workSheet.Cells["A2"].Value = headers[1];
            workSheet.Cells["A2"].Style.Font.Bold = false;
            workSheet.Cells["A2"].Style.Font.SetFromFont(new Font("Calibri", 12));
            workSheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            workSheet.Cells["A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            workSheet.Cells["A2"].Style.WrapText = true;

            workSheet.Cells["A1"].AutoFitColumns();
            int addressStartContent = 3;
            for (int i = 0; i < itemInHouses.Count; i++)
            {
                var item = itemInHouses[i];
                workSheet.Cells[i + addressStartContent, 1].Value = item.Title;
                workSheet.Cells[i + addressStartContent, 1].Style.WrapText = true;
                workSheet.Column(1).Width = 32.18;
                workSheet.Cells[i + addressStartContent, 2].Value = item.Value;
                workSheet.Cells[i + addressStartContent, 2].Style.Font.Bold = true;
                workSheet.Cells[i + addressStartContent, 2].Style.WrapText = true;
            }
            for (int i = 0; i < containerHeaders.Count; i++)
            {
                workSheet.Cells[25, i + 1].Value = containerHeaders[i];
                workSheet.Cells[25, i + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[25, i + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[25, i + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                workSheet.Cells[25, i + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                workSheet.Cells[25, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                workSheet.Cells[25, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Column(i + 1).Width = 32.18;
            }
        }
    }
}
