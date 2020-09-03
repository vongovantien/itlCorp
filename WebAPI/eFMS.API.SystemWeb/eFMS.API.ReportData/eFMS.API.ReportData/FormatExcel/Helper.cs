using eFMS.API.ReportData.Consts;
using eFMS.API.ReportData.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace eFMS.API.ReportData
{
    public class Helper
    {
        const double minWidth = 0.00;
        const double maxWidth = 500.00;
        
        public Stream CreateCountryExcelFile(List<CatCountry> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatForCountryExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public Stream GenerateIncotermListExcel(List<CatIncotermModel> listObj, Stream stream = null)
        {
            List<string> headers = new List<string>()
            {

                "Incoterm",
                "Name En",
                "Service",
                "Status",
                "Create Date",
                "Creator",
                
            };
            try
            {
                int addressStartContent = 4;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Incoterm List");
                    var worksheet = excelPackage.Workbook.Worksheets[1];

                    BuildHeader(worksheet, headers, "INCOTERM INFORMATION");
                    for (int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];

                        worksheet.Cells[addressStartContent, 1].Value = item.Code;

                        worksheet.Cells[addressStartContent, 2].Value = item.NameEn;
                        worksheet.Cells[addressStartContent, 3].Value = item.Service;
                        worksheet.Cells[addressStartContent, 4].Value = item.Active.Value ?"Active":"Inactive";
                        worksheet.Cells[addressStartContent, 5].Value = item.DatetimeCreated.HasValue ? item.DatetimeCreated.Value.ToString("dd/MM/yyyy") : "";
                        worksheet.Cells[addressStartContent, 6].Value = item.UserCreatedName;
                        
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

        public void BindingFormatForCountryExcel(ExcelWorksheet worksheet, List<CatCountry> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Country Code";
            worksheet.Cells[1, 2].Value = "English Name";
            worksheet.Cells[1, 3].Value = "Local Name";
            worksheet.Cells[1, 4].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth,maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Code;
                worksheet.Cells[i + 2, 2].Value = item.NameEn;
                worksheet.Cells[i + 2, 3].Value = item.NameVn;
                string inactivechar = "";
                if (item.Active == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 4].Value = inactivechar;
            }
        }

        #region Province
        public Stream CreateProvinceExcelFile(List<CatProvince> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatForProvinceExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingFormatForProvinceExcel(ExcelWorksheet worksheet, List<CatProvince> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Province Code";
            worksheet.Cells[1, 2].Value = "English Name";
            worksheet.Cells[1, 3].Value = "Local Name";
            worksheet.Cells[1, 4].Value = "Country";
            worksheet.Cells[1, 5].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Code;
                worksheet.Cells[i + 2, 2].Value = item.NameEn;
                worksheet.Cells[i + 2, 3].Value = item.NameVn;
                worksheet.Cells[i + 2, 4].Value = item.CountryNameEN;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 5].Value = inactivechar;
            }
        }


        #endregion

        #region TownWard
        public Stream CreateTownWardExcelFile(List<CatTownWard> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatForTownWardExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingFormatForTownWardExcel(ExcelWorksheet worksheet, List<CatTownWard> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Town-Ward Code";
            worksheet.Cells[1, 2].Value = "English Name";
            worksheet.Cells[1, 3].Value = "Local Name";
            worksheet.Cells[1, 4].Value = "District";
            worksheet.Cells[1, 5].Value = "Province";
            worksheet.Cells[1, 6].Value = "Country";
            worksheet.Cells[1, 7].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Code;
                worksheet.Cells[i + 2, 2].Value = item.NameEn;
                worksheet.Cells[i + 2, 3].Value = item.NameVn;
                worksheet.Cells[i + 2, 4].Value = item.DistrictNameEN;
                worksheet.Cells[i + 2, 5].Value = item.ProvinceNameEN;
                worksheet.Cells[i + 2, 6].Value = item.CountryNameEN;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 7].Value = inactivechar;
            }
        }

        #endregion

        #region Charge
        public Stream CreateChargeExcelFile(List<CatCharge> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[3, 1].LoadFromCollection(list, true, TableStyles.None);
                    BindingFormatForCatChargeExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingFormatForCatChargeExcel(ExcelWorksheet worksheet, List<CatCharge> listItems)
        {

            // Tạo header
            List<string> headers = new List<string>
            {
                "No.", "Code", "English Name", "Local Name", "Type", "Inactive"
            };

            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                worksheet.Cells[3, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            worksheet.Cells[1, 1, 1, 6].Merge = true;
            worksheet.Cells["A1"].Value = "CHARGE INFORMATION";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 4, 1].Value = i + 1;
                worksheet.Cells[i + 4, 2].Value = item.Code;
                worksheet.Cells[i + 4, 3].Value = item.ChargeNameEn;
                worksheet.Cells[i + 4, 4].Value = item.ChargeNameVn;
                worksheet.Cells[i + 4, 5].Value = item.Type;
                worksheet.Cells[i + 4, 6].Value = item.Active == true ? "Active" : "Inactive";

                worksheet.Cells[i + 4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 4, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                //Add border left right for cells
                for (int j = 0; j < headers.Count; j++)
                {
                    worksheet.Cells[i + 4, j + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[i + 4, j + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                //Add border bottom for last cells
                if (i == listItems.Count - 1)
                {
                    for (int j = 0; j < headers.Count; j++)
                    {
                        worksheet.Cells[i + 4, j + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                }
            }
        }

        #endregion

        #region Charge
        public Stream CreateCurrencyExcelFile(List<CatCurrency> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatForCatCurrencyExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingFormatForCatCurrencyExcel(ExcelWorksheet worksheet, List<CatCurrency> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Code";
            worksheet.Cells[1, 2].Value = "Currency Name";
            worksheet.Cells[1, 3].Value = "Is Default";
            worksheet.Cells[1, 4].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Id;
                worksheet.Cells[i + 2, 2].Value = item.CurrencyName;
                worksheet.Cells[i + 2, 3].Value = item.IsDefault;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 4].Value = inactivechar;
            }
        }

        #endregion



        #region District
        public Stream CreateDistrictExcelFile(List<CatDistrict> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatForDistrictExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingFormatForDistrictExcel(ExcelWorksheet worksheet, List<CatDistrict> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "District Code";
            worksheet.Cells[1, 2].Value = "English Name";
            worksheet.Cells[1, 3].Value = "Local Name";
            worksheet.Cells[1, 4].Value = "Province";
            worksheet.Cells[1, 5].Value = "Country";
            worksheet.Cells[1, 6].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Code;
                worksheet.Cells[i + 2, 2].Value = item.NameEn;
                worksheet.Cells[i + 2, 3].Value = item.NameVn;
                worksheet.Cells[i + 2, 4].Value = item.ProvinceNameEN;
                worksheet.Cells[i + 2, 5].Value = item.CountryNameEN;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 6].Value = inactivechar;
            }
        }


        #endregion


        #region Commodity List
        public Stream CreateCommoditylistExcelFile(List<CatCommodityModel> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatCommoditylistExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public void BindingFormatCommoditylistExcel(ExcelWorksheet worksheet, List<CatCommodityModel> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Code";
            worksheet.Cells[1, 2].Value = "Name EN";
            worksheet.Cells[1, 3].Value = "Name VN";
            worksheet.Cells[1, 4].Value = "Commodity Group";
            worksheet.Cells[1, 5].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Code;
                worksheet.Cells[i + 2, 2].Value = item.CommodityNameEn;
                worksheet.Cells[i + 2, 3].Value = item.CommodityNameVn;
                worksheet.Cells[i + 2, 4].Value = item.CommodityGroupNameEn;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 5].Value = inactivechar;
            }
        }

        #endregion

        #region Commodity Group
        public Stream CreateCommoditygroupExcelFile(List<CatCommodityGroup> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatCommodityGroupExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public void BindingFormatCommodityGroupExcel(ExcelWorksheet worksheet, List<CatCommodityGroup> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Id";
            worksheet.Cells[1, 2].Value = "Name EN";
            worksheet.Cells[1, 3].Value = "Name VN";
            worksheet.Cells[1, 4].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Id;
                worksheet.Cells[i + 2, 2].Value = item.GroupNameEn;
                worksheet.Cells[i + 2, 3].Value = item.GroupNameVn;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 4].Value = inactivechar;
            }
        }
        #endregion

        #region WareHourse
        public Stream CreateWareHourseExcelFile(List<CatWareHouse> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatForWareHourseExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingFormatForWareHourseExcel(ExcelWorksheet worksheet, List<CatWareHouse> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Country Code";
            worksheet.Cells[1, 2].Value = "English Name";
            worksheet.Cells[1, 3].Value = "Local Name";
            worksheet.Cells[1, 4].Value = "Address";
            worksheet.Cells[1, 5].Value = "District";
            worksheet.Cells[1, 6].Value = "City/Provice";
            worksheet.Cells[1, 7].Value = "Country";
            worksheet.Cells[1, 8].Value = "Status";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Code;
                worksheet.Cells[i + 2, 2].Value = item.NameEn;
                worksheet.Cells[i + 2, 3].Value = item.NameVn;
                worksheet.Cells[i + 2, 4].Value = item.Address;
                worksheet.Cells[i + 2, 5].Value = item.DistrictName;

                worksheet.Cells[i + 2, 6].Value = item.ProvinceName;
                worksheet.Cells[i + 2, 7].Value = item.CountryName;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 8].Value = inactivechar;
            }
        }

        #endregion

        #region PortIndex
        public Stream CreatePortIndexExcelFile(List<CatPortIndex> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatForPortIndexExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public void BindingFormatForPortIndexExcel(ExcelWorksheet worksheet, List<CatPortIndex> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Code";
            worksheet.Cells[1, 2].Value = "Name EN";
            worksheet.Cells[1, 3].Value = "Name VN";
            worksheet.Cells[1, 4].Value = "Country";
            worksheet.Cells[1, 5].Value = "Zone";
            worksheet.Cells[1, 6].Value = "Mode";
            worksheet.Cells[1, 7].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Code;
                worksheet.Cells[i + 2, 2].Value = item.NameEn;
                worksheet.Cells[i + 2, 3].Value = item.NameVn;
                worksheet.Cells[i + 2, 4].Value = item.CountryNameEN;
                worksheet.Cells[i + 2, 5].Value = item.AreaNameEN;
                worksheet.Cells[i + 2, 6].Value = item.ModeOfTransport;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 7].Value = inactivechar;
            }
        }

        #endregion

        #region Partner
        public Stream CreatePartnerExcelFile(List<CatPartner> listObj, string partnerType, string author, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells["A1"].Value = "Partner Data - " + partnerType;
                    workSheet.Cells["A1"].Style.Font.Bold = true;
                    workSheet.Cells[1, 1, 2, 4].Merge = true;
                    workSheet.Cells["A3"].Value = "Export By: " + author;
                    workSheet.Cells["A3"].Style.Font.Bold = true;
                    workSheet.Cells[3, 1, 3, 2].Merge = true;
                    BindingFormatForPartnerExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingFormatForPartnerExcel(ExcelWorksheet worksheet, List<CatPartner> listItems)
        {
            // Tạo header
            worksheet.Cells[5, 1].Value = "No.";
            worksheet.Cells[5, 2].Value = "Partner ID";
            worksheet.Cells[5, 3].Value = "Full Name";
            worksheet.Cells[5, 4].Value = "Short Name";
            worksheet.Cells[5, 5].Value = "Billing Address";
            worksheet.Cells[5, 6].Value = "Tax Code";
            worksheet.Cells[5, 7].Value = "Tel";
            worksheet.Cells[5, 8].Value = "Fax";
            worksheet.Cells[5, 9].Value = "Creator";
            worksheet.Cells[5, 10].Value = "Modify";
            worksheet.Cells[5, 11].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            int indexNo = 0;
            for (int i = 0; i < listItems.Count; i++)
            {
                indexNo = indexNo + 1;
                var item = listItems[i];
                worksheet.Cells[i + 6, 1].Value = indexNo;
                worksheet.Cells[i + 6, 2].Value = item.AccountNo;
                worksheet.Cells[i + 6, 3].Value = item.FullName;
                worksheet.Cells[i + 6, 4].Value = item.ShortName;
                worksheet.Cells[i + 6, 5].Value = item.AddressVN;
                worksheet.Cells[i + 6, 6].Value = item.TaxCode;
                worksheet.Cells[i + 6, 7].Value = item.Tel;
                worksheet.Cells[i + 6, 8].Value = item.Fax;
                worksheet.Cells[i + 6, 9].Value = item.UserCreatedName;
                worksheet.Cells[i + 6, 10].Value = item.DatetimeModified;
                string inactivechar = "Active";
                if (item.Inactive == false)
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 6, 11].Value = inactivechar;
            }
        }
        #endregion

        #region Stage
        public Stream CreateCatStateExcelFile(List<CatStage> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatStageExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingFormatStageExcel(ExcelWorksheet worksheet, List<CatStage> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Department";
            worksheet.Cells[1, 2].Value = "Code";
            worksheet.Cells[1, 3].Value = "Name VN";
            worksheet.Cells[1, 4].Value = "Name EN";
            worksheet.Cells[1, 5].Value = "Description VN";
            worksheet.Cells[1, 6].Value = "Description EN";
            worksheet.Cells[1, 7].Value = "Inactive";

            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.DeptName;
                worksheet.Cells[i + 2, 2].Value = item.Code;
                worksheet.Cells[i + 2, 3].Value = item.StageNameVn;
                worksheet.Cells[i + 2, 4].Value = item.StageNameEn;
                worksheet.Cells[i + 2, 5].Value = item.DescriptionVn;
                worksheet.Cells[i + 2, 6].Value = item.DescriptionEn;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 7].Value = inactivechar;
            }
        }

        #endregion

        #region Unit
        public Stream CreateCatUnitExcelFile(List<CatUnit> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingUnitStageExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingUnitStageExcel(ExcelWorksheet worksheet, List<CatUnit> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Code";
            worksheet.Cells[1, 2].Value = "Name EN";
            worksheet.Cells[1, 3].Value = "Name VN";
            worksheet.Cells[1, 4].Value = "Unit Type";
            worksheet.Cells[1, 5].Value = "Description EN";
            worksheet.Cells[1, 6].Value = "Description VN";
            worksheet.Cells[1, 7].Value = "Inactive";

            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Code;
                worksheet.Cells[i + 2, 2].Value = item.UnitNameEn;
                worksheet.Cells[i + 2, 3].Value = item.UnitNameVn;
                worksheet.Cells[i + 2, 4].Value = item.UnitType;
                worksheet.Cells[i + 2, 5].Value = item.DescriptionEn;
                worksheet.Cells[i + 2, 6].Value = item.DescriptionVn;
                string inactivechar = "";
                if (item.Active == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 7].Value = inactivechar;
            }
        }

        #endregion

        #region Custom Clearence
        public Stream CreateCustomClearanceExcelFile(List<CustomsDeclaration> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatForCustomClearanceExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public void BindingFormatForCustomClearanceExcel(ExcelWorksheet worksheet, List<CustomsDeclaration> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Clearance No";
            worksheet.Cells[1, 2].Value = "Type";
            worksheet.Cells[1, 3].Value = "Clearance Location";
            worksheet.Cells[1, 4].Value = "Partner Name";
            worksheet.Cells[1, 5].Value = "Import Country";
            worksheet.Cells[1, 6].Value = "Export Country";
            worksheet.Cells[1, 7].Value = "JOBID";
            worksheet.Cells[1, 8].Value = "Clearacne Date";
            worksheet.Cells[1, 9].Value = "Status";

            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.ClearanceNo;
                worksheet.Cells[i + 2, 2].Value = item.Type;
                worksheet.Cells[i + 2, 3].Value = item.GatewayName;
                worksheet.Cells[i + 2, 4].Value = item.CustomerName;
                worksheet.Cells[i + 2, 5].Value = item.ImportCountryName;
                worksheet.Cells[i + 2, 6].Value = item.ExportCountryName;
                worksheet.Cells[i + 2, 7].Value = item.JobNo;
                worksheet.Cells[i + 2, 8].Value = item.ClearanceDate;

                string status = "";
                if (item.JobNo != null)
                {
                    status = "Imported";
                }
                else
                {
                    status = "Not Imported";
                }
                worksheet.Cells[i + 2, 9].Value = status;
            }
        }



        #endregion

        #region
        public Stream CreateDepartmentExcelFile(List<CatDepartmentModel> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Sheet1");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[3, 1].LoadFromCollection(list, true, TableStyles.None);
                    BindingFormatForDepartmentExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void BindingFormatForDepartmentExcel(ExcelWorksheet worksheet, List<CatDepartmentModel> listItems)
        {
            // Tạo header
            List<string> headers = new List<string>
            {
                "No.", "Department Code", "Name EN", "Name Local", "Name Abbr", "Office", "Status"
            };

            for(int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[3, i + 1].Value = headers[i];
                worksheet.Cells[3, i + 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells[3, i + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                worksheet.Cells[3, i + 1].Style.Font.Bold = true;
                worksheet.Cells[3, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, i + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            }

            worksheet.Cells[1, 1, 1, 7].Merge = true;
            worksheet.Cells["A1"].Value = "DEPARTMENT INFORMATION";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Cells["A1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 4, 1].Value = i + 1;
                worksheet.Cells[i + 4, 2].Value = item.Code;
                worksheet.Cells[i + 4, 3].Value = item.DeptNameEn;
                worksheet.Cells[i + 4, 4].Value = item.DeptName;
                worksheet.Cells[i + 4, 5].Value = item.DeptNameAbbr;
                worksheet.Cells[i + 4, 6].Value = item.OfficeName;
                worksheet.Cells[i + 4, 7].Value = item.Active == true ? "Active" : "Inactive";

                worksheet.Cells[i + 4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[i + 4, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                //Add border left right for cells
                for (int j = 0; j < headers.Count; j++)
                {
                    worksheet.Cells[i + 4, j + 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[i + 4, j + 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;                    
                }

                //Add border bottom for last cells
                if (i == listItems.Count - 1)
                {
                    for (int j = 0; j < headers.Count; j++)
                    {
                        worksheet.Cells[i + 4, j + 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }                    
                }
            }
        }

        public Stream GenerateCompanyExcel(List<SysCompany> listCompany, Stream stream = null)
        {
            List<String> headers = new List<String>()
            {
                "No",
                "Company Code",
                "Name En",
                "Name Local",
                "Name Abbr",
                "Website",
                "Status"
            };
            try
            {
                int addressStartContent = 4;
                int no = 1;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Sheet1");
                    var worksheet = excelPackage.Workbook.Worksheets[1];

                    BuildHeader(worksheet, headers, "COMPANY INFORMATION");

                    for (int i = 0; i < listCompany.Count; i++)
                    {
                        var item = listCompany[i];
                        worksheet.Cells[i + addressStartContent, 1].Value = no.ToString();
                        worksheet.Cells[i + addressStartContent, 2].Value = item.Code;
                        worksheet.Cells[i + addressStartContent, 3].Value = item.BunameEn;
                        worksheet.Cells[i + addressStartContent, 4].Value = item.BunameVn;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.BunameAbbr;
                        worksheet.Cells[i + addressStartContent, 6].Value = item.Website;
                        string status = "";
                        if (item.Active == true)
                        {
                            status = "Active";
                        }
                        else
                        {
                            status = "Inactive";
                        }
                        worksheet.Cells[i + addressStartContent, 7].Value = status;

                        no++;
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
        #endregion
        public Stream GenerateOfficeExcel(List<SysOfficeModel> listCompany, Stream stream = null)
        {
            List<String> headers = new List<String>()
            {
                "No",
                "Office Code",
                "Name En",
                "Name Local",
                "Name Abbr",
                "Address Local",
                "Tax code",
                "Company",
                "Status"
            };
            try
            {
                int addressStartContent = 4;
                int no = 1;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Sheet1");
                    var worksheet = excelPackage.Workbook.Worksheets[1];

                    BuildHeader(worksheet, headers, "OFFICE INFORMATION");

                    for (int i = 0; i < listCompany.Count; i++)
                    {
                        var item = listCompany[i];
                        worksheet.Cells[i + addressStartContent, 1].Value = no.ToString();
                        worksheet.Cells[i + addressStartContent, 2].Value = item.Code;
                        worksheet.Cells[i + addressStartContent, 3].Value = item.BranchNameEn;
                        worksheet.Cells[i + addressStartContent, 4].Value = item.BranchNameVn;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.ShortName;
                        worksheet.Cells[i + addressStartContent, 6].Value = item.AddressVn;
                        worksheet.Cells[i + addressStartContent, 7].Value = item.Taxcode;
                        worksheet.Cells[i + addressStartContent, 8].Value = item.CompanyName;


                        string status = "";
                        if (item.Active == true)
                        {
                            status = "Active";
                        }
                        else
                        {
                            status = "Inactive";
                        }
                        worksheet.Cells[i + addressStartContent, 9].Value = status;

                        no++;
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

        #region Group
        internal Stream CreateGroupExcelFile(List<SysGroupModel> listObj, Stream stream = null)
        {
            List<String> headers = new List<String>()
            {
                "No.",
                "Group Code",
                "Name EN",
                "Name Local",
                "Name Abbr",
                "Department",
                "Status"
            };
            try
            {
                int addressStartContent = 4;
                int no = 1;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Sheet1");
                    var worksheet = excelPackage.Workbook.Worksheets[1];

                    BuildHeader(worksheet, headers, "GROUP INFORMATION");

                    for (int i = 0; i < listObj.Count; i++)
                    {
                        var item = listObj[i];
                        worksheet.Cells[i + addressStartContent, 1].Value = no;
                        worksheet.Cells[i + addressStartContent, 2].Value = item.Code;
                        worksheet.Cells[i + addressStartContent, 3].Value = item.NameEn;
                        worksheet.Cells[i + addressStartContent, 4].Value = item.NameVn;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.ShortName;
                        worksheet.Cells[i + addressStartContent, 6].Value = item.DepartmentName;
                        worksheet.Cells[i + addressStartContent, 7].Value = item.Active == true ? "Active" : "Inactive";

                        //Add border left right for cells
                        AddBorderLeftRightCell(worksheet, headers, addressStartContent, i);

                        //Add border bottom for last cells
                        AddBorderBottomLastCell(worksheet, headers, addressStartContent, i, listObj.Count);
                        
                        no++;
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
        #endregion
      
        #region User
        public Stream GenerateUserExcel(List<SysUserModel> listCompany, Stream stream = null)
        {
            List<String> headers = new List<String>()
            {
                "No",
                "User Name",
                "Name EN",
                "Full Name",
                "User Type",
                "Status"
            };
            try
            {
                int addressStartContent = 4;
                int no = 1;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("Sheet1");
                    var worksheet = excelPackage.Workbook.Worksheets[1];

                    BuildHeader(worksheet, headers, "USER INFORMATION");

                    for (int i = 0; i < listCompany.Count; i++)
                    {
                        var item = listCompany[i];
                        worksheet.Cells[i + addressStartContent, 1].Value = no.ToString();
                        worksheet.Cells[i + addressStartContent, 2].Value = item.UserName;
                        worksheet.Cells[i + addressStartContent, 3].Value = item.EmployeeNameEN;
                        worksheet.Cells[i + addressStartContent, 4].Value = item.EmployeeNameVN;
                        worksheet.Cells[i + addressStartContent, 5].Value = item.UserType;

                        string status = "";
                        if (item.Active == true)
                        {
                            status = "Active";
                        }
                        else
                        {
                            status = "Inactive";
                        }
                        worksheet.Cells[i + addressStartContent, 6].Value = status;

                        no++;
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
        #endregion

        #region Cat Chart Of Accounts
        public Stream CreateChartOfAccountExcelFile(List<CatChartOfAccounts> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
                    BindingFormatForCatChartOfAccountsExcel(workSheet, list);
                    excelPackage.Save();
                    return excelPackage.Stream;
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public void BindingFormatForCatChartOfAccountsExcel(ExcelWorksheet worksheet, List<CatChartOfAccounts> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Account Code";
            worksheet.Cells[1, 2].Value = "Account Name Local";
            worksheet.Cells[1, 3].Value = "Account Name EN";
            worksheet.Cells[1, 4].Value = "Status";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.AccountCode;
                worksheet.Cells[i + 2, 2].Value = item.AccountNameLocal;
                worksheet.Cells[i + 2, 3].Value = item.AccountNameEn;
                string inactivechar = "";
                if (item.Active == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 4].Value = inactivechar;
            }
        }
        #endregion
    }
}