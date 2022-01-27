﻿using System.Collections.Generic;
using System.Net.Http;
using eFMS.API.ReportData.Models;
using Microsoft.AspNetCore.Mvc;
using eFMS.API.ReportData.HttpServices;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using eFMS.API.ReportData.Helpers;
using Microsoft.AspNetCore.Authorization;
using eFMS.API.ReportData.Models.Criteria;

namespace eFMS.API.ReportData.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiController]
    public class CatalogueController : ControllerBase
    {
        private readonly APIs aPis;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="appSettings"></param>
        public CatalogueController(IOptions<APIs> appSettings)
        {
            this.aPis = appSettings.Value;
        }
        #region catalogue
        /// <summary>
        /// export country
        /// </summary>
        /// <returns></returns>
        [Route("ExportCountry")]
        [HttpPost]
        public async Task<IActionResult> ExportCountry(CatCountryCriteria catCountryCriteria)
        {
            Helper helper = new Helper();

            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catCountryCriteria, aPis.CatalogueAPI + Urls.Catelogue.CountryUrl);
            var dataObjects =  responseFromApi.Content.ReadAsAsync<List<CatCountry>>();

            var stream = helper.CreateCountryExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.CountryName);
        }
        /// <summary>
        /// export warehouse
        /// </summary>
        /// <returns></returns>
        [Route("ExportWareHouse")]
        [HttpPost]
        [Authorize]

        public async Task<IActionResult> ExportWareHouse(CatPlaceCriteria catPlaceCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            Helper helper = new Helper();

            var responseFromApi = await HttpServiceExtension.PostAPI(catPlaceCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatplaceUrl, accessToken);
            var dataObjects =  responseFromApi.Content.ReadAsAsync<List<CatWareHouse>>();

            var stream = helper.CreateWareHourseExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.WareHouse);
        }
        /// <summary>
        /// export portindex
        /// </summary>
        /// <returns></returns>
        [Route("ExportPortIndex")]
        [HttpPost]
        [Authorize]

        public async Task<IActionResult> ExportPortIndex(CatPlaceCriteria catPlaceCriteria)
        {

            var accessToken = Request.Headers["Authorization"].ToString();
            Helper helper = new Helper();

            var responseFromApi = await HttpServiceExtension.PostAPI(catPlaceCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatplaceUrl, accessToken);
            var dataObjects =  responseFromApi.Content.ReadAsAsync<List<CatPortIndex>>();  

            var stream = helper.CreatePortIndexExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.PortIndex);
        }
        /// <summary>
        /// export partnerdata
        /// </summary>
        /// <returns></returns>
        [Route("ExportPartnerData")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportPartner(CatPartnerCriteria catPartnerCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.PostAPI(catPartnerCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatPartnerUrl,accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatPartner>>();  

            var stream = helper.CreatePartnerExcelFile(dataObjects.Result, catPartnerCriteria.PartnerType, catPartnerCriteria.Author);
            return new FileHelper().ExportExcel(null,stream, FilesNames.PartnerData);
        }

        /// <summary>
        /// export partnerdata
        /// </summary>
        /// <returns></returns>
        [Route("ExportAgreementInfo")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportAgreementInfo(CatPartnerCriteria catPartnerCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.PostAPI(catPartnerCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatAgreementUrl, accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AgreementInfo>>();

            var stream = helper.GenerateAgreementExcel(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.PartnerData);
        }

        /// <summary>
        /// export commodity list
        /// </summary>
        /// <returns></returns>
        [Route("ExportCommodityList")]
        [HttpPost]
        public async Task<IActionResult> ExportCommodityList(CatCommodityCriteria catCommodityCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catCommodityCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatCommodityUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatCommodityModel>>();  
            var stream = helper.CreateCommoditylistExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.CommodityList);
        }
        /// <summary>
        /// export commodity group
        /// </summary>
        /// <returns></returns>
        [Route("ExportCommodityGroup")]
        [HttpPost]
        public async Task<IActionResult> ExportCommodityGroup(CatCommodityGroupCriteria catCommodityGroupCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catCommodityGroupCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatCommodityGroupUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatCommodityGroup>>();
            var stream = helper.CreateCommoditygroupExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.CommodityGroupList);
        }
        /// <summary>
        /// export stage
        /// </summary>
        /// <returns></returns>
        /// 
        [Route("ExportStage")]
        [HttpPost]
        public async Task<IActionResult> ExportStage(CatStageCriteria catStageCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catStageCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatStageUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatStage>>();

            var stream = helper.CreateCatStateExcelFile(dataObjects.Result);

            return new FileHelper().ExportExcel(null,stream, FilesNames.StageList);
        }
        /// <summary>
        /// export unit
        /// </summary> 
        /// <returns></returns>
        ///  
        [Route("ExportUnit")]
        [HttpPost]
        public async Task<IActionResult> ExportUnit(CatUnitCriteria catUnitCriteria)
        {
            Helper helper = new Helper();

            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catUnitCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatUnitUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatUnit>>();

            var stream = helper.CreateCatUnitExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.UnitList);
        }

        /// <summary>
        /// export province
        /// </summary>
        /// <returns></returns>
        [Route("ExportProvince")]
        [HttpPost]
        public async Task<IActionResult> ExportProvince(CatPlaceCriteria catPlaceCriteria)
        {
            Helper helper = new Helper();

            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catPlaceCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatplaceUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatProvince>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll

            var stream = helper.CreateProvinceExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.ProvinceName);
        }

        /// <summary>
        /// export district
        /// </summary>
        /// <returns></returns>
        [Route("ExportDistrict")]
        [HttpPost]
        public async Task<IActionResult> ExportDistrict(CatPlaceCriteria catPlaceCriteria)
        {
            Helper helper = new Helper();

            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catPlaceCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatplaceUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatDistrict>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll

            var stream = helper.CreateDistrictExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.DistrictName);
        }

        /// <summary>
        /// export town-ward
        /// </summary>
        /// <returns></returns>
        [Route("ExportTownWard")]
        [HttpPost]
        public async Task<IActionResult> ExportTownWard(CatPlaceCriteria catPlaceCriteria)
        {
            Helper helper = new Helper();

            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catPlaceCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatplaceUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatTownWard>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll

            var stream = helper.CreateTownWardExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.TowardName);
        }

        /// <summary>
        /// export charge
        /// </summary>
        /// <returns></returns>
        [Route("ExportCharge")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportCharge(CatChargeCriteria catChargeCriteria)
        {
            Helper helper = new Helper();
            var accessToken = Request.Headers["Authorization"].ToString();

            var responseFromApi = await HttpServiceExtension.PostAPI(catChargeCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatchargeUrl, accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatCharge>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll

            var stream = helper.CreateChargeExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.ChargeName);
        }

        /// <summary>
        /// export currency
        /// </summary>
        /// <returns></returns>
        [Route("ExportCurrency")]
        [HttpPost]
        public async Task<IActionResult> ExportCurrency(CatCurrrencyCriteria catCurrrencyCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catCurrrencyCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatCurrencyUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatCurrency>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll

            var stream = helper.CreateCurrencyExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.CurrencyName);
        }

        /// <summary>
        /// Export Chart Of Accounts
        /// </summary>
        /// <returns></returns>
        [Route("ExportChartOfAccounts")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportChartOfAccounts(CatChartOfAccountsCriteria catChartOfAccountsCriteria)
        {
            Helper helper = new Helper();
            var accessToken = Request.Headers["Authorization"].ToString();

            var responseFromApi = await HttpServiceExtension.PostAPI(catChartOfAccountsCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatChartOfAccountsUrl,accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatChartOfAccounts>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll

            var stream = helper.CreateChartOfAccountExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.CurrencyName);
        }
        [Route("ExportIncotermList")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportIncotermList(CatIncotermCriteria catIncotermCriteria)
        {
            Helper helper = new Helper();
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.PostAPI(catIncotermCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatIncotermListUrl, accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatIncotermModel>>();
            var stream = helper.GenerateIncotermListExcel(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.IncotermList);
            
        }

        [Route("ExportPotentialCustomerList")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportPotentialCustomerList(CatPotentialCriteria catPotentialCriteria)
        {
            Helper helper = new Helper();
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.PostAPI(catPotentialCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatPotentialListUrl, accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatPotentialModel>>();
            var stream = helper.GeneratePotentialListExcel(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.PotentialList);

        }

        /// <summary>
        /// export Custom Clearance
        /// </summary>
        /// <returns></returns>
        /// 
        #endregion
        #region Custom Clearance
        [Route("CustomsDeclaration/ExportCustomClearance")]
        [HttpPost]
        public async Task<IActionResult> ExportCustomClearance(CustomsDeclarationCriteria customsDeclarationCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(customsDeclarationCriteria, aPis.HostStaging + Urls.CustomClearance.CustomClearanceUrl, accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CustomsDeclaration>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll

            var stream = helper.CreateCustomClearanceExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.CustomClearanceName);
        }

        [Route("ExportBank")]
        [HttpPost]
        public async Task<IActionResult> ExportBank(CatBankCriteria catBankCriteria)
        {
            Helper helper = new Helper();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(catBankCriteria, aPis.CatalogueAPI + Urls.Catelogue.CatBankUrl);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<CatBank>>();  //Make sure to add a reference to System.Net.Http.Formatting.dll

            var stream = helper.CreateBankExcelFile(dataObjects.Result);
            return new FileHelper().ExportExcel(null,stream, FilesNames.BankName);
        }

        #endregion
    }
}