using eFMS.API.ReportData.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.IO;

namespace eFMS.API.ReportData
{
    public class Helper
    {
        const double minWidth = 0.00;
        const double maxWidth = 500.00;
        #region country
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
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
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
            worksheet.Cells[1, 1].Value = "Code";
            worksheet.Cells[1, 2].Value = "English Name";
            worksheet.Cells[1, 3].Value = "Local Name";
            worksheet.Cells[1, 4].Value = "Type";
            worksheet.Cells[1, 5].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells["A1:Z1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Code;
                worksheet.Cells[i + 2, 2].Value = item.ChargeNameEn;
                worksheet.Cells[i + 2, 3].Value = item.ChargeNameVn;
                worksheet.Cells[i + 2, 4].Value = item.Type;
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
        public Stream CreatePartnerExcelFile(List<CatPartner> listObj, Stream stream = null)
        {
            try
            {
                var list = listObj;
                using (var excelPackage = new ExcelPackage(stream ?? new MemoryStream()))
                {
                    excelPackage.Workbook.Worksheets.Add("First Sheet");
                    var workSheet = excelPackage.Workbook.Worksheets[1];
                    workSheet.Cells[1, 1].LoadFromCollection(list, true, TableStyles.Dark9);
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
            worksheet.Cells[1, 1].Value = "Partner ID";
            worksheet.Cells[1, 2].Value = "Full Name";
            worksheet.Cells[1, 3].Value = "Short Name";
            worksheet.Cells[1, 4].Value = "Billing Address";
            worksheet.Cells[1, 5].Value = "Tax Code";
            worksheet.Cells[1, 6].Value = "Tel";
            worksheet.Cells[1, 7].Value = "Fax";
            worksheet.Cells[1, 8].Value = "Creator";
            worksheet.Cells[1, 9].Value = "Modify";
            worksheet.Cells[1, 10].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth, maxWidth);
            worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            for (int i = 0; i < listItems.Count; i++)
            {
                var item = listItems[i];
                worksheet.Cells[i + 2, 1].Value = item.Id;
                worksheet.Cells[i + 2, 2].Value = item.FullName;
                worksheet.Cells[i + 2, 3].Value = item.ShortName;
                worksheet.Cells[i + 2, 4].Value = item.AddressVN;
                worksheet.Cells[i + 2, 5].Value = item.TaxCode;
                worksheet.Cells[i + 2, 6].Value = item.Tel;
                worksheet.Cells[i + 2, 7].Value = item.Fax;
                worksheet.Cells[i + 2, 8].Value = item.UserCreatedName;
                worksheet.Cells[i + 2, 9].Value = item.DatetimeModified;
                string inactivechar = "";
                if (item.Inactive == true)
                {
                    inactivechar = "Active";
                }
                else
                {
                    inactivechar = "Inactive";
                }
                worksheet.Cells[i + 2, 10].Value = inactivechar;
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

        #region Custom Clearence

        #endregion




    }
}