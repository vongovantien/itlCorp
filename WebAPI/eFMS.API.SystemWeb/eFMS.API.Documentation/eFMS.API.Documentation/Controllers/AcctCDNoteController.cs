﻿using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemManagementAPI.Infrastructure.Middlewares;


namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AcctCDNoteController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAcctCDNoteServices cdNoteServices;
        private readonly ICurrentUser currentUser;
        private readonly ICheckPointService checkPointService;

        public AcctCDNoteController(IStringLocalizer<LanguageSub> localizer, IAcctCDNoteServices service, ICurrentUser user, ICheckPointService checkPoint)
        {
            stringLocalizer = localizer;
            cdNoteServices = service;
            currentUser = user;
            checkPointService = checkPoint;
        }

        /// <summary>
        /// Add New CD Note
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddNew(AcctCdnoteModel model)
        {
            currentUser.Action = "AddNewCDNote";
            if (!ModelState.IsValid) return BadRequest();

            HandleState hs = cdNoteServices.AddNewCDNote(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update CD Note
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(AcctCdnoteModel model)
        {
            currentUser.Action = "UpdateCDNote";

            if (!ModelState.IsValid) return BadRequest();
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            var hs = cdNoteServices.UpdateCDNote(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Delete CD Note
        /// </summary>
        /// <param name="cdNoteId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid cdNoteId)
        {
            currentUser.Action = "DeleteCDNote";

            var hs = cdNoteServices.DeleteCDNote(cdNoteId);
            ResultHandle result = new ResultHandle();
            if (!hs.Success)
            {
                result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
                return BadRequest(result);
            }
            else
            {
                var message = HandleError.GetMessage(hs, Crud.Delete);
                result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                return Ok(result);
            }
        }

        [HttpGet]
        [Route("Get")]
        [Authorize]
        public List<object> Get(Guid Id, bool IsShipmentOperation)
        {
            return cdNoteServices.GroupCDNoteByPartner(Id, IsShipmentOperation);
        }

        /// <summary>
        /// Get CDNote With Hbl Id
        /// </summary>
        /// <param name="hblId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCDNoteWithHbl")]
        [Authorize]
        public List<AcctCdnoteModel> GetCDNoteWithHbl(Guid? hblId, Guid? jobId)
        {
            return cdNoteServices.GetCDNoteWithHbl(hblId, jobId);
        }

        [HttpGet]
        [Route("GetDetails")]
        public AcctCDNoteDetailsModel Get(Guid jobId, string cdNo)
        {
            return cdNoteServices.GetCDNoteDetails(jobId, cdNo);
        }

        /// <summary>
        /// Preview CD Note (OPS)
        /// </summary>
        /// <param name="model">AcctCDNoteDetailsModel</param>
        /// <param name="isOrigin">Is Preview with Original</param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewOpsCdNote")]
        public IActionResult PreviewOpsCdNote(AcctCDNoteDetailsModel model, bool isOrigin)
        {
            var result = cdNoteServices.Preview(model, isOrigin);
            return Ok(result);
        }

        /// <summary>
        /// Preview CD Note (OPS) Combine
        /// </summary>
        /// <param name="model">AcctCDNoteDetailsModel List</param>
        /// <param name="isOrigin">Is Preview with Original</param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewOpsCdNoteList")]
        public IActionResult PreviewOpsCdNoteList(object model, bool isOrigin)
        {
            var data = JsonConvert.SerializeObject(model, Formatting.Indented);
            List<AcctCdnoteModel> acctCdNoteList = JsonConvert.DeserializeObject<List<AcctCdnoteModel>>(data);
            var cdNoteModel = cdNoteServices.GetDataPreviewCDNotes(acctCdNoteList);
            var result = cdNoteServices.Preview(cdNoteModel, isOrigin);
            return Ok(result);
        }

        /// <summary>
        /// Data Export Details CD Note
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="cdNo"></param>
        /// <param name="officeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExportOpsCdNote")]
        public IActionResult ExportOpsCdNote(Guid jobId, string cdNo, Guid officeId)
        {
            var cdNoteDetail = cdNoteServices.GetCDNoteDetails(jobId, cdNo);
            var data = cdNoteServices.GetDataExportOpsCDNote(cdNoteDetail, officeId);
            return Ok(data);
        }

        /// <summary>
        /// Export Excel Template of OPS CD Note Combine
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ExportOpsCdNoteCombine")]
        public IActionResult ExportOpsCdNoteCombine(object model)
        {
            var cdData = JsonConvert.SerializeObject(model, Formatting.Indented);
            List<AcctCdnoteModel> acctCdNoteList = JsonConvert.DeserializeObject<List<AcctCdnoteModel>>(cdData);
            var cdNoteDetail = cdNoteServices.GetDataPreviewCDNotes(acctCdNoteList);
            var data = cdNoteServices.GetDataExportOpsCDNote(cdNoteDetail, (Guid)cdNoteDetail.CDNote.OfficeId);
            return Ok(data);
        }

        /// <summary>
        /// Preview CD Note Local and USD currency
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewOPSCDNoteWithCurrency")]
        public IActionResult PreviewOPSCDNoteWithCurrency(PreviewCdNoteCriteria criteria)
        {
            var result = cdNoteServices.PreviewOPSCDNoteWithCurrency(criteria);
            return Ok(result);
        }

        /// <summary>
        /// check allow delete an existed item
        /// </summary>
        /// <param name="cdNoteId"></param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{cdNoteId}")]
        public IActionResult CheckAllowDelete(Guid cdNoteId)
        {
            return Ok(cdNoteServices.CheckAllowDelete(cdNoteId));
        }

        /// <summary>
        /// Preview CD Note (Sea)
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewSIFCdNote")]
        public IActionResult PreviewSIFCdNote(PreviewCdNoteCriteria criteria)
        {
            var data = cdNoteServices.GetCDNoteDetails(criteria.JobId, criteria.CreditDebitNo);
            var result = cdNoteServices.PreviewSIF(data, criteria.Currency, criteria.ExportFormatType);
            return Ok(result);
        }

        /// <summary>
        /// Preview CD Note (AIR)
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewAirCdNote")]
        public IActionResult PreviewAirCdNote(PreviewCdNoteCriteria criteria)
        {
            var data = cdNoteServices.GetCDNoteDetails(criteria.JobId, criteria.CreditDebitNo);
            var result = cdNoteServices.PreviewAir(data, criteria.Currency, criteria.ExportFormatType);
            return Ok(result);
        }

        /// <summary>
        /// Preview CD Note (OPS) Combine
        /// </summary>
        /// <param name="model">AcctCDNoteDetailsModel List</param>
        /// <param name="currency"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewASCdNoteList")]
        public IActionResult PreviewASCdNoteList(object model, string currency, string service)
        {
            var data = JsonConvert.SerializeObject(model, Formatting.Indented);
            List<AcctCdnoteModel> acctCdNoteList = JsonConvert.DeserializeObject<List<AcctCdnoteModel>>(data);
            var jobId = acctCdNoteList.FirstOrDefault() == null ? Guid.Empty : acctCdNoteList.FirstOrDefault().JobId;
            var cdNoteModel = cdNoteServices.GetCDNoteDetails(jobId, string.Empty, acctCdNoteList);
            if (service == DocumentConstants.AI_SHIPMENT)
            {
                var result = cdNoteServices.PreviewAir(cdNoteModel, currency);
                return Ok(result);
            }
            else
            {
                var result = cdNoteServices.PreviewSIF(cdNoteModel, currency);
                return Ok(result);
            }
            
        }

        /// <summary>
        /// get invoice - cd note
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(CDNoteCriteria criteria, int page, int size)
        {
            var data = cdNoteServices.PagingInvoiceList(criteria, page, size, out int rowsCount);
            var result = new { data, totalItems = rowsCount, page, size };
            return Ok(result);
        }

        /// <summary>
        /// Reject credit note
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("RejectCreditNote")]
        [Authorize]
        public IActionResult RejectCreditNote(RejectCreditNoteModel model)
        {
            currentUser.Action = "RejectCreditNote";

            var reject = cdNoteServices.RejectCreditNote(model);
            if (!reject.Success)
            {
                var result = new ResultHandle { Status = reject.Success, Message = string.Format("{0}. Reject credit note fail.", reject.Message.ToString()), Data = model };
                return BadRequest(result);
            }
            return Ok(new ResultHandle { Status = reject.Success, Message = "Reject credit note successful.", Data = model });
        }

        /// <summary>
        /// Preview Combine Billing CD Note
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("PreviewCombineBilling")]
        [Authorize]
        public IActionResult PreviewCombineBilling([FromBody]List<CombineBillingCriteria> criteria)
        {
            var result = cdNoteServices.PreviewCombineBilling(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Query CD Note by criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("Query")]
        [Authorize]
        public async Task<IActionResult> QueryAsync(CDNoteCriteria criteria)
        {
            var result = await cdNoteServices.QueryAsync(criteria);
            if (result == null)
            {
                // Handle empty result set
                return NotFound(); // or any other appropriate response indicating no data found
            }
            else
            {
                // Handle non-empty result set
                return Ok(result);
            }
        }

        /// <summary>
        /// get invoice - cd note
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetDataAcctMngtDebCretInvExport")]
        [Authorize]
        public IActionResult GetDataAcctMngtDebCretInvExport(CDNoteCriteria criteria)
        {
            var result = cdNoteServices.GetDataAcctMngtDebCretInvExport(criteria);
            return Ok(result);
        }

        /// <summary>
        /// get invoice - cd note by agency template
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetDataAcctMngtAgencyExport")]
        [Authorize]
        public IActionResult GetDataAcctMngtAgencyExport(CDNoteCriteria criteria)
        {
            var result = cdNoteServices.GetDataAcctMngtAgencyExport(criteria);
            if (result == null || result.Count() == 0) { return BadRequest(); };
            return Ok(result);
        }
    }
}