using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.ReportData.FormatExcel;
using eFMS.API.ReportData.Helpers;
using eFMS.API.ReportData.HttpServices;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Accounting;
using eFMS.API.ReportData.Models.Criteria;
using Microsoft.AspNetCore.Authorization;
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
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(advancePaymentCriteria, aPis.AccountingAPI + Urls.Accounting.AdvancePaymentUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AdvancePaymentModel>>();

            var stream = new AccountingHelper().GenerateAdvancePaymentExcel(dataObjects.Result);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Advance Payment List.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Advance Payment with each Request.
        /// </summary>
        /// <param name="advancePaymentCriteria"></param>
        /// <returns></returns>
        [Route("ExportAdvancePaymentShipment")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportAdvancePaymentShipment(AdvancePaymentCriteria advancePaymentCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var advancePaymentsAPI = await HttpServiceExtension.PostAPI(advancePaymentCriteria, aPis.AccountingAPI + Urls.Accounting.AdvancePaymentUrl, accessToken);

            var advancePayments = advancePaymentsAPI.Content.ReadAsAsync<List<AdvancePaymentModel>>();
            List<string> codes = new List<string>();
            foreach (var item in advancePayments.Result)
            {
                codes.Add(item.AdvanceNo);
            }
            var responseFromApi = await HttpServiceExtension.PostAPI(codes, aPis.AccountingAPI + Urls.Accounting.GetGroupRequestsByAdvanceNoList, accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AdvancePaymentRequestModel>>();

            var stream = new AccountingHelper().GenerateAdvancePaymentShipmentExcel(dataObjects.Result);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Advance Payment List Shipment.xlsx");

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
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(settlementPaymentCriteria, aPis.AccountingAPI + Urls.Accounting.SettlementPaymentUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SettlementPaymentModel>>();

            var stream = new AccountingHelper().GenerateSettlementPaymentExcel(dataObjects.Result);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Settlement Payment List.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Settlement Payment
        /// </summary>
        /// <param name="settlementPaymentCriteria"></param>
        /// <returns></returns>
        [Route("ExportSettlementPaymentShipment")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportSettlementPaymentShipment(SettlementPaymentCriteria settlementPaymentCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var settlementsAPI = await HttpServiceExtension.PostAPI(settlementPaymentCriteria, aPis.AccountingAPI + Urls.Accounting.SettlementPaymentUrl, accessToken);

            var settlementPayments = settlementsAPI.Content.ReadAsAsync<List<SettlementPaymentModel>>();
            List<string> codes = new List<string>();
            foreach (var item in settlementPayments.Result)
            {
                codes.Add(item.SettlementNo);
            }
            var responseFromApi = await HttpServiceExtension.PostAPI(codes, aPis.AccountingAPI + Urls.Accounting.QueryDataSettlementExport, accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SettlementExportGroupDefault>>();

            var stream = new AccountingHelper().GenerateSettlementPaymentShipmentExcel(dataObjects.Result);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

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
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.DetailAdvancePaymentExportUrl + "?advanceId=" + advanceId + "&&language=" + language);

            var dataObjects = responseFromApi.Content.ReadAsAsync<AdvanceExport>();

            var stream = new AccountingHelper().GenerateDetailAdvancePaymentExcel(dataObjects.Result, language);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Advance Form - eFMS.xlsx");

            return fileContent;
        }

        /// Export detail SOA
        /// </summary>
        /// <param name="soaNo">soaNo of SOA</param>
        /// <param name="currency">currency of SOA</param>
        /// <returns></returns>
        [Route("ExportDetailSOA")]
        [HttpGet]
        public async Task<IActionResult> ExportDetailSOA(string soaNo, string currency)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.DetailSOAExportUrl + soaNo + "&&currencyLocal="  + currency);

            var dataObjects = responseFromApi.Content.ReadAsAsync<DetailSOAModel>();

            var stream = new AccountingHelper().GenerateDetailSOAExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            string fileName = "Export SOA " + soaNo + ".xlsx";
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, fileName);
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
            var responseFromApi = await HttpServiceExtension.GetApi( aPis.AccountingAPI + Urls.Accounting.GetDataBravoSOAUrl + soaNo) ;

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<ExportBravoSOAModel>>();

            var stream = new AccountingHelper().GenerateBravoSOAExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "SOA Bravo List.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export SOA OPS
        /// </summary>
        /// <param name="soaNo"></param>
        /// <returns></returns>
        [Route("ExportSOAOPS")]
        [HttpGet]
        public async Task<IActionResult> ExportSOAOPS(string soaNo)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.GetDataSOAOPSUrl + soaNo);

            var dataObjects = responseFromApi.Content.ReadAsAsync<SOAOPSModel>();

            var stream = new AccountingHelper().GenerateSOAOPSExcel(dataObjects.Result);
            //if (dataObjects.Result.exportSOAOPs.Count == 0)
            //{
            //    return Ok();
            //}
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "SOA OPS.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export detail settlement payment
        /// </summary>
        /// <param name="settlementId">Id of settlement payment</param>
        /// <param name="language">VN (Việt Nam) or ENG (English)</param>
        /// <returns></returns>
        [Route("ExportDetailSettlementPayment")]
        [HttpGet]
        public async Task<IActionResult> ExportDetailSettlementPayment(Guid settlementId, string language)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.DetailSettlementPaymentExportUrl + "?settlementId=" + settlementId);

            var dataObjects = responseFromApi.Content.ReadAsAsync<SettlementExport>();

            var stream = new AccountingHelper().GenerateDetailSettlementPaymentExcel(dataObjects.Result, language);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Settlement Form - eFMS.xlsx");

            return fileContent;
        }

        /// Export detail SOA
        /// </summary>
        /// <param name="soaNo">soaNo of SOA</param>
        /// <param name="officeId">officeId of user</param>
        /// <returns></returns>
        [Route("ExportSOAAirfreight")]
        [HttpGet]
        public async Task<IActionResult> ExportSOAAirfreight(string soaNo, string officeId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.GetDataSOAAirfreightExportUrl + soaNo + "&&officeId=" + officeId);

            var dataObjects = responseFromApi.Content.ReadAsAsync<ExportSOAAirfreightModel>();
            if(dataObjects.Result.HawbAirFrieghts == null)
            {
                return Ok();
            }
            var stream = new AccountingHelper().GenerateSOAAirfreightExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            string fileName = "Export SOA Air Freight " + soaNo + ".xlsx";
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, fileName);
            return fileContent;
        }

        /// <summary>
        /// Export accounting management
        /// </summary>
        /// <param name="criteria">List Voucher or Invoices</param>
        /// <returns></returns>
        [Route("ExportAccountingManagement")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportAccountingManagement(AccAccountingManagementCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.PostAPI(criteria, aPis.AccountingAPI + Urls.Accounting.AccountingManagementExportUrl, accessToken);            

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AccountingManagementExport>>();
            if (dataObjects.Result == null || dataObjects.Result.Count == 0) return Ok();

            var stream = new AccountingHelper().GenerateAccountingManagementExcel(dataObjects.Result, criteria.TypeOfAcctManagement);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, (criteria.TypeOfAcctManagement == "Invoice" ? "VAT INVOICE" : "VOUCHER") + " - eFMS.xlsx");
            return fileContent;
        }

    }
}