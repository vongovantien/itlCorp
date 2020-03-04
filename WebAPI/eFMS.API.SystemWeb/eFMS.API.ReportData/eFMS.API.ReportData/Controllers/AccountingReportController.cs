using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.ReportData.FormatExcel;
using eFMS.API.ReportData.Helpers;
using eFMS.API.ReportData.HttpServices;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Accounting;
using eFMS.API.ReportData.Models.Criteria;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace eFMS.API.ReportData.Controllers
{
    /// <summary>
    /// Accounting Report Controller
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiController]
    public class AccountingReportController : ControllerBase
    {
        private readonly APIs aPis;

        /// <summary>
        /// Contructor controller Accounting Report
        /// </summary>
        /// <param name="appSettings"></param>
        public AccountingReportController(IOptions<APIs> appSettings)
        {
            this.aPis = appSettings.Value;
        }

        /// <summary>
        /// Export Advance Payment
        /// </summary>
        /// <param name="advancePaymentCriteria"></param>
        /// <returns></returns>
        [Route("ExportAdvancePayment")]
        [HttpPost]
        public async Task<IActionResult> ExportAdvancePayment(AdvancePaymentCriteria advancePaymentCriteria)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(advancePaymentCriteria, aPis.HostStaging + Urls.Accounting.AdvancePaymentUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AdvancePaymentModel>>();

            var stream = new AccountingHelper().GenerateAdvancePaymentExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Advance Payment List.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Settlement Payment
        /// </summary>
        /// <param name="settlementPaymentCriteria"></param>
        /// <returns></returns>
        [Route("ExportSettlementPayment")]
        [HttpPost]
        public async Task<IActionResult> ExportSettlementPayment(SettlementPaymentCriteria settlementPaymentCriteria)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(settlementPaymentCriteria, aPis.HostStaging + Urls.Accounting.SettlementPaymentUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SettlementPaymentModel>>();

            var stream = new AccountingHelper().GenerateSettlementPaymentExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Settlement Payment List.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export detail advance payment
        /// </summary>
        /// <param name="advanceId">Id of advance payment</param>
        /// <param name="language">VN (Việt Nam) or ENG (English)</param>
        /// <returns></returns>
        [Route("ExportDetailAdvancePayment")]
        [HttpGet]
        public async Task<IActionResult> ExportDetailAdvancePayment(Guid advanceId, string language)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Accounting.DetailAdvancePaymentExportUrl + "?advanceId=" + advanceId + "&&language=" + language);

            var dataObjects = responseFromApi.Content.ReadAsAsync<AdvanceExport>();

            var stream = new AccountingHelper().GenerateDetailAdvancePaymentExcel(dataObjects.Result, language);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Advance Form - eFMS.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Bravo SOA
        /// </summary>
        /// <param name="soaNo"></param>
        /// <returns></returns>
        [Route("ExportBravoSOA")]
        [HttpGet]
        public async Task<IActionResult> ExportBravoSOA(string soaNo)
        {
            var responseFromApi = await HttpServiceExtension.GetApi( aPis.HostStaging + Urls.Accounting.GetDataBravoSOAUrl + soaNo) ;

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<ExportBravoSOAModel>>();

            var stream = new AccountingHelper().GenerateBravoSOAExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "SOA Bravo List.xlsx");

            return fileContent;
        }
    }
}