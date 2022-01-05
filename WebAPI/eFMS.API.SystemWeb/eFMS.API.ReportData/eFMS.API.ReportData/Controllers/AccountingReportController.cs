﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.ReportData.FormatExcel;
using eFMS.API.ReportData.Helpers;
using eFMS.API.ReportData.HttpServices;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Accounting;
using eFMS.API.ReportData.Models.Common.Enums;
using eFMS.API.ReportData.Models.Criteria;
using FMS.API.ReportData.Models.Accounting;
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
        /// Export Settlement List With Charge Detail
        /// </summary>
        /// <param name="settlementPaymentCriteria"></param>
        /// <returns></returns>
        [Route("ExportSettlementPaymentDetailSurCharges")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportSettlementPaymentDetailSurCharges(SettlementPaymentCriteria settlementPaymentCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.PostAPI(settlementPaymentCriteria, aPis.AccountingAPI + Urls.Accounting.SettlementPaymentDetailListUrl, accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AccountingSettlementExportGroup>>();

            var stream = new AccountingHelper().ExportSettlementPaymentDetailSurCharges(dataObjects.Result, "Settlement-Detail Template.xlsx");
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Settlement-Detail Template.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Advance Payment with each Request.
        /// </summary>
        /// <param name="accountingPaymentCriteria"></param>
        /// <returns></returns>
        [Route("ExportAccountingPayment")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportAccountingPayment(AccountingPaymentCriteria accountingPaymentCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var accountingPaymentsAPI = await HttpServiceExtension.PostAPI(accountingPaymentCriteria, aPis.AccountingAPI + Urls.Accounting.InvoicePaymentUrl, accessToken);
            var accountingPayments = accountingPaymentsAPI.Content.ReadAsAsync<List<AccountingPaymentModel>>();
            var stream = accountingPaymentCriteria.PaymentType == PaymentType.Invoice ?
                new AccountingHelper().GenerateInvoicePaymentShipmentExcel(accountingPayments.Result) :
                new AccountingHelper().GenerateOBHPaymentShipmentExcel(accountingPayments.Result);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");
            FileContentResult fileContent = new FileHelper().ExportExcel(stream,
                accountingPaymentCriteria.PaymentType == PaymentType.Invoice ?
                "Invoice Payment List.xlsx":"OBH Payment List.xlsx");
            return fileContent;
        }

        [Route("ExportAccountingCustomerPayment")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportAccountingCustomerPayment(AccountingPaymentCriteria paymentCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.PostAPI(paymentCriteria, aPis.AccountingAPI + Urls.Accounting.CustomerPaymentUrl, accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AccountingCustomerPaymentExport>>();

            var stream = new AccountingHelper().GenerateExportCustomerHistoryPayment(dataObjects.Result, paymentCriteria, "Statement_of_Receivable-Customer.xlsx");
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Statement of Receivable Customer - eFMS.xlsx");

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
        [Authorize]
        public async Task<IActionResult> ExportDetailAdvancePayment(Guid advanceId, string lang)
         {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.DetailAdvancePaymentExportUrl + "?advanceId=" + advanceId + "&&language=" + lang, accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<AdvanceExport>();

            var stream = new AccountingHelper().GenerateDetailAdvancePaymentExcel(dataObjects.Result, lang);
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
        /// <param name="lang">VN (Việt Nam) or ENG (English)</param>
        /// <returns></returns>
        [Route("ExportDetailSettlementPayment")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportDetailSettlementPayment(Guid settlementId, string lang)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.DetailSettlementPaymentExportUrl + "?settlementId=" + settlementId, accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<SettlementExport>();

            var stream = new AccountingHelper().GenerateDetailSettlementPaymentExcel(dataObjects.Result, lang, "");
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Settlement Form - eFMS.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export detail settlement template payment
        /// </summary>
        /// <param name="settlementId">Id of settlement payment</param>
        /// <param name="language">VN (Việt Nam) or ENG (English)</param>
        /// <returns></returns>
        [Route("ExportDetailSettlementPaymentTemplate")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportDetailSettlementPaymentTemplate(Guid settlementId, string lang)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.DetailSettlementPaymentExportUrl + "?settlementId=" + settlementId, accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<SettlementExport>();

            var stream = new AccountingHelper().GenerateDetailSettlementPaymentExcel(dataObjects.Result, lang, "SettlementPaymentTemplate");
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Settlement Template Form - eFMS.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export General Preview in Settlement detail
        /// </summary>
        /// <param name="settlementId"></param>
        /// <returns></returns>
        [Route("ExportGeneralSettlementPayment")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportGeneralSettlementPayment(Guid settlementId)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.GeneralSettlementPaymentExport + "?settlementId=" + settlementId, accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<InfoSettlementExport>();

            var stream = new AccountingHelper().GenerateExportGeneralSettlementPayment(dataObjects.Result, "Settlement-General-Preview.xlsx");
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Settlement General Preview - eFMS.xlsx");

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
            var stream = new AccountingHelper().GenerateSOAAirfreightExcel(dataObjects.Result,null,null);
            if (stream == null)
            {
                return null;
            }
            string fileName = "Export SOA Air Freight " + soaNo + ".xlsx";
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, fileName);
            return fileContent;
        }

        /// Export detail SOA
        /// </summary>
        /// <param name="soaNo">soaNo of SOA</param>
        /// <param name="officeId">officeId of user</param>
        /// <returns></returns>
        [Route("ExportSOAAirfreightWithHBL")]
        [HttpGet]
        public async Task<IActionResult> ExportSOAAirfreightWithHBL(string soaNo, string officeId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.GetDataSOAAirfreightExportUrl + soaNo + "&&officeId=" + officeId);

            var dataObjects = responseFromApi.Content.ReadAsAsync<ExportSOAAirfreightModel>();
            if (dataObjects.Result.HawbAirFrieghts == null)
            {
                return Ok();
            }
            var stream = new AccountingHelper().GenerateSOAAirfreightExcel(dataObjects.Result, "WithHBL", null);
            if (stream == null)
            {
                return null;
            }
            string fileName = "Customer SOA AirFreight  With HBL " + soaNo + ".xlsx";
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, fileName);
            return fileContent;
        }

        /// <summary>
        /// Export detail SOA Supplier
        /// </summary>
        /// <param name="soaNo">SoaNo of SOA</param>
        /// <param name="officeId">OfficeId of User</param>
        /// <returns></returns>
        [Route("ExportSOASupplierAirfreight")]
        [HttpGet]
        public async Task<IActionResult> ExportSOASupplierAirfreight(string soaNo, string officeId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.GetDataSOASupplierAirfreightExportUrl + soaNo + "&&officeId=" + officeId);

            var dataObjects = responseFromApi.Content.ReadAsAsync<ExportSOAAirfreightModel>();
            if (dataObjects.Result.HawbAirFrieghts == null)
            {
                return Ok();
            }
            var stream = new AccountingHelper().GenerateSOASupplierAirfreightExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            string fileName = "Export SOA Supplier Air Freight " + soaNo + ".xlsx";
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

        /// <summary>
        /// Export accounting receivable
        /// </summary>
        /// <param name="criteria">AccountReceivableCriteria</param>
        /// <returns></returns>
        [Route("ExportAccountingReceivableArSumary")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportAccountingReceivableArSumary(AccountReceivableCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.PostAPI(criteria, aPis.AccountingAPI + Urls.Accounting.AccountingGetDataARSumaryExportUrl, accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AccountReceivableResultExport>>();
            if (dataObjects.Result == null || dataObjects.Result.Count == 0) return Ok();

            //var stream = new AccountingHelper().GenerateAccountingReceivableExcel(dataObjects.Result,criteria.ArType);
            var stream = new AccountingHelper().GenerateAccountingReceivableArSumary(dataObjects.Result, "AR_SUMMARY_TEMPLATE.xlsx");

            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Trial" + " - eFMS.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export detail settlement payment
        /// </summary>
        /// <param name="settlementId">Id of settlement payment</param>
        /// <param name="language">VN (Việt Nam) or ENG (English)</param>
        /// <param name="key">String random</param>
        /// <returns></returns>
        [Route("ExportDetailSettlementPreview")]
        [HttpGet]
        //[Authorize]
        public async Task<IActionResult> ExportDetailSettlementPreview(Guid settlementId, string lang,string key)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.DetailSettlementPaymentExportUrl + "?settlementId=" + settlementId, accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<SettlementExport>();

            var stream = new AccountingHelper().GenerateDetailSettlementPaymentExcel(dataObjects.Result, lang, "");
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Settlement Form - eFMS.xlsx");

            return fileContent;
        }

        [Route("ExportAccountingAgencyPayment")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportAccountingAgencyPayment(AccountingPaymentCriteria paymentCriteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.PostAPI(paymentCriteria, aPis.AccountingAPI + Urls.Accounting.AgencyPaymentUrl, accessToken);
             
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AccountingAgencyPaymentExport>>();
            if (dataObjects.Result == null || dataObjects.Result.Count == 0) return Ok();

            var stream = new AccountingHelper().GenerateExportAgencyHistoryPayment(dataObjects.Result, "Statement_of_Receivable-Agency.xlsx",paymentCriteria);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Statement of Receivable Agency - eFMS.xlsx");

            return fileContent;
        }

        [Route("ExportReceiptAdvance")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportReceiptAdvance(AcctReceiptCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.PostAPI(criteria, aPis.AccountingAPI + Urls.Accounting.GetDataExportReceiptAdvance, accessToken);

            var dataObjects = responseFromApi.Content.ReadAsAsync<AcctReceiptAdvanceModelExport>();
            if (dataObjects.Result == null)  return Ok(null);

            var stream = new AccountingHelper().GenerateReceiptAdvance(dataObjects.Result, criteria, out string fileName);
            if (stream == null) return new FileHelper().ExportExcel(new MemoryStream(), "");

            FileContentResult fileContent = new FileHelper().ExportExcel(stream, fileName);

            Response.Headers.Add("efms-file-name", fileName);
            Response.Headers.Add("Access-Control-Expose-Headers", "efms-file-name");

            return fileContent;
        }

        [Route("ExportCombineOps")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportCombineOps(AcctCombineBillingCriteria criteria)
        {
            HttpResponseMessage responseFromApi;
            var accessToken = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(criteria.Currency))
            {
                responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.GetDataCombineOpsUrl + criteria.ReferenceNo[0], accessToken);
            }
            else
            {
                responseFromApi = await HttpServiceExtension.PostAPI(criteria, aPis.AccountingAPI + Urls.Accounting.GetDataCombineOpsByPartnerUrl, accessToken);
            }
            var dataObjects = responseFromApi.Content.ReadAsAsync<CombineOPSModel>();

            var stream = new AccountingHelper().GenerateCombineOPSExcel(dataObjects.Result);

            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "SOA OPS.xlsx");

            return fileContent;
        }
    }
}