﻿using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using eFMS.API.ReportData.Models;
using Microsoft.AspNetCore.Mvc;
using eFMS.API.ReportData.HttpServices;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiController]
    public class ReportDataController : ControllerBase
    {
        private readonly APIs aPis;
        public ReportDataController(IOptions<APIs> appSettings)
        {
            this.aPis = appSettings.Value;
        }
        #region catalogue
        /// <summary>
        /// export country
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportCountry")]
        [HttpPost]
        public async Task<IActionResult> ExportCountry(CatCountryCriteria catCountryCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catCountryCriteria, aPis.HostStaging + Urls.Catelogue.CountryUrl);
            var dataObjects =  responseFromApi.Content.ReadAsAsync<List<CatCountry>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateCountryExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
            fileContents: buffer.ToArray(),
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: FilesNames.CountryName
        );
        }
        /// <summary>
        /// export warehouse
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportWareHouse")]
        [HttpPost]
        public async Task<IActionResult> ExportWareHouse(CatPlaceCriteria catPlaceCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catPlaceCriteria, aPis.HostStaging + Urls.Catelogue.CatplaceUrl);
            var dataObjects =  responseFromApi.Content.ReadAsAsync<List<CatWareHouse>>();
            var stream = helper.CreateWareHourseExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
                fileContents: buffer.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: FilesNames.WareHouse
            );
        }
        /// <summary>
        /// export portindex
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportPortIndex")]
        [HttpPost]
        public async Task<IActionResult> ExportPortIndex(CatPlaceCriteria catPlaceCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catPlaceCriteria, aPis.HostStaging + Urls.Catelogue.CatplaceUrl);
            var dataObjects =  responseFromApi.Content.ReadAsAsync<List<CatPortIndex>>();  
            var stream = helper.CreatePortIndexExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
                fileContents: buffer.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: FilesNames.PortIndex
            );
        }
        /// <summary>
        /// export partnerdata
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportPartnerData")]
        [HttpPost]
        public async Task<IActionResult> ExportPartner(CatPartnerCriteria catPartnerCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catPartnerCriteria, aPis.HostStaging + Urls.Catelogue.CatPartnerUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatPartner>>();  
            var stream = helper.CreatePartnerExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
                fileContents: buffer.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: FilesNames.PartnerData
            );
        }
        /// <summary>
        /// export commodity list
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportCommodityList")]
        [HttpPost]
        public async Task<IActionResult> ExportCommodityList(CatCommodityCriteria catCommodityCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catCommodityCriteria, aPis.HostStaging + Urls.Catelogue.CatCommodityUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatCommodityModel>>();  
            var stream = helper.CreateCommoditylistExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
                fileContents: buffer.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: FilesNames.CommodityList
            );
        }
        /// <summary>
        /// export commodity group
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportCommodityGroup")]
        [HttpPost]
        public async Task<IActionResult> ExportCommodityGroup(CatCommodityGroupCriteria catCommodityGroupCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catCommodityGroupCriteria, aPis.HostStaging + Urls.Catelogue.CatCommodityGroupUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatCommodityGroup>>();
            var stream = helper.CreateCommoditygroupExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
                fileContents: buffer.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: FilesNames.CommodityList
            );
        }
        /// <summary>
        /// export stage
        /// </summary>
        /// <returns></returns>
        /// 
        [Route("Catalogue/ExportStage")]
        [HttpPost]
        public async Task<IActionResult> ExportStage(CatStageCriteria catStageCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catStageCriteria, aPis.HostStaging + Urls.Catelogue.CatStageUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatStage>>();
            var stream = helper.CreateCatStateExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
                fileContents: buffer.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: FilesNames.StageList
            );
        }
        /// <summary>
        /// export unit
        /// </summary> 
        /// <returns></returns>
        ///  
        [Route("Catalogue/ExportUnit")]
        [HttpPost]
        public async Task<IActionResult> ExportUnit(CatUnitCriteria catUnitCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catUnitCriteria, aPis.HostStaging + Urls.Catelogue.CatUnitUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatUnit>>();
            var stream = helper.CreateCatUnitExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
                fileContents: buffer.ToArray(),
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: FilesNames.UnitList
            );
        }
        /// <summary>
        /// export province
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportProvince")]
        [HttpPost]
        public async Task<IActionResult> ExportProvince(CatPlaceCriteria catPlaceCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catPlaceCriteria, aPis.HostStaging + Urls.Catelogue.CatplaceUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatProvince>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateProvinceExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
            fileContents: buffer.ToArray(),
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: FilesNames.ProvinceName
        );
        }

        /// <summary>
        /// export district
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportDistrict")]
        [HttpPost]
        public async Task<IActionResult> ExportDistrict(CatPlaceCriteria catPlaceCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catPlaceCriteria, aPis.HostStaging + Urls.Catelogue.CatplaceUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatDistrict>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateDistrictExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
            fileContents: buffer.ToArray(),
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: FilesNames.DistrictName
        );
        }

        /// <summary>
        /// export town-ward
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportTownWard")]
        [HttpPost]
        public async Task<IActionResult> ExportTownWard(CatPlaceCriteria catPlaceCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catPlaceCriteria, aPis.HostStaging + Urls.Catelogue.CatplaceUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatTownWard>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateTownWardExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
            fileContents: buffer.ToArray(),
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: FilesNames.TowardName
        );
        }

        /// <summary>
        /// export charge
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportCharge")]
        [HttpPost]
        public async Task<IActionResult> ExportCharge(CatChargeCriteria catChargeCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catChargeCriteria, aPis.HostStaging + Urls.Catelogue.CatchargeUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatCharge>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateChargeExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
            fileContents: buffer.ToArray(),
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: FilesNames.ChargeName
        );
        }
        /// <summary>
        /// export currency
        /// </summary>
        /// <returns></returns>
        [Route("Catalogue/ExportCurrency")]
        [HttpPost]
        public async Task<IActionResult> ExportCurrency(CatCurrrencyCriteria catCurrrencyCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catCurrrencyCriteria, aPis.HostStaging + Urls.Catelogue.CatCurrencyUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatCurrency>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateCurrencyExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
            fileContents: buffer.ToArray(),
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: FilesNames.CurrencyName
        );
        }


        #endregion
        #region Custom Clearance
        [Route("CustomsDeclaration/ExportCustomClearance")]
        [HttpPost]
        public async Task<IActionResult> ExportCustomClearance(CustomsDeclarationCriteria customsDeclarationCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(customsDeclarationCriteria, aPis.HostStaging + Urls.CustomClearance.CustomClearanceUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CustomsDeclaration>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateCustomClearanceExcelFile(dataObjects.Result);
            var buffer = stream as MemoryStream;
            return this.File(
            fileContents: buffer.ToArray(),
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileDownloadName: FilesNames.CustomClearanceName
        );
        }


        #endregion
    }
}