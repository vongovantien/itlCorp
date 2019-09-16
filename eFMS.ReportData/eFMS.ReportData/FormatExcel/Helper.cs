using eFMS.ReportData.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.IO;

namespace eFMS.ReportData
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

        public Stream CreateWareHourseExcelFile(List<WareHouse> listObj, Stream stream = null)
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

        public void BindingFormatForWareHourseExcel(ExcelWorksheet worksheet, List<WareHouse> listItems)
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
            worksheet.Cells.AutoFitColumns(minWidth,maxWidth);
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

        public void BindingFormatCommoditylistExcel(ExcelWorksheet worksheet, List<CatCommodityModel> listItems)
        {
            // Tạo header
            worksheet.Cells[1, 1].Value = "Code";
            worksheet.Cells[1, 2].Value = "Name EN";
            worksheet.Cells[1, 3].Value = "Name VN";
            worksheet.Cells[1, 4].Value = "Commodity Group";
            worksheet.Cells[1, 5].Value = "Inactive";
            worksheet.Cells.AutoFitColumns(minWidth,maxWidth);
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

    }
}