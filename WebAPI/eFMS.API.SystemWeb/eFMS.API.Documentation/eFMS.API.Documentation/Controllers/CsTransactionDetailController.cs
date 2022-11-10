﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Services;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.ForPartner.DL.Models.Receivable;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsTransactionDetailController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsTransactionDetailService csTransactionDetailService;
        private readonly ICurrentUser currentUser;
        private readonly ICsMawbcontainerService containerService;
        private readonly ICsTransactionService csTransactionService;
        private readonly IAccAccountReceivableService AccAccountReceivableService;
        private readonly IOptions<ApiServiceUrl> apiServiceUrl;
        private readonly ICsStageAssignedService csStageAssignedService;
        private readonly IEDocService edocService;
        public CsTransactionDetailController(IStringLocalizer<LanguageSub> localizer,
            ICsTransactionDetailService service,
            ICurrentUser user,
            ICsMawbcontainerService mawbcontainerService,
            ICsTransactionService csTransaction,
            IAccAccountReceivableService AccAccountReceivable,
            IOptions<ApiServiceUrl> serviceUrl,
            ICsStageAssignedService stageAssignedService,
             IEDocService EDocService
            )
        {
            stringLocalizer = localizer;
            csTransactionDetailService = service;
            currentUser = user;
            containerService = mawbcontainerService;
            csTransactionService = csTransaction;
            AccAccountReceivableService = AccAccountReceivable;
            apiServiceUrl = serviceUrl;
            csStageAssignedService = stageAssignedService;
            edocService = EDocService;
        }

        [HttpGet("CheckPermission/{id}")]
        [Authorize]
        public IActionResult CheckDetailPermission(Guid id)
        {

            var result = csTransactionDetailService.CheckDetailPermission(id);
            if (result == 403) return Forbid();
            return Ok(true);
        }

        [HttpGet]
        [Route("GetByJob")]
        public IActionResult GetByJob(Guid jobId)
        {
            CsTransactionDetailCriteria criteria = new CsTransactionDetailCriteria { JobId = jobId };
            return Ok(csTransactionDetailService.GetByJob(criteria));
        }

        [HttpGet]
        [Route("GetById")]
        [Authorize]
        public IActionResult GetById(Guid Id)
        {
            var statusCode = csTransactionDetailService.CheckDetailPermission(Id);
            if (statusCode == 403) return Forbid();

            CsMawbcontainerCriteria criteriaMaw = new CsMawbcontainerCriteria { Hblid = Id };
            var hbl = csTransactionDetailService.GetDetails(Id);
            var resultMaw = containerService.Query(criteriaMaw).ToList();
            if (resultMaw.Count() > 0)
            {
                hbl.CsMawbcontainers = resultMaw;
            }
            return Ok(hbl);
        }

        [HttpGet]
        [Route("GetSeparateByHblid")]
        public IActionResult GetSeparateByHblid(Guid hbId)
        {
            CsMawbcontainerCriteria criteriaMaw = new CsMawbcontainerCriteria { Hblid = hbId };
            var hbl = csTransactionDetailService.GetSeparateByHblid(hbId);
            var resultMaw = containerService.Query(criteriaMaw).ToList();
            if (resultMaw.Count() > 0)
            {
                hbl.CsMawbcontainers = resultMaw;
            }
            return Ok(hbl);
        }

        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CsTransactionDetailModel model)
        {
            currentUser.Action = "AddNewCSTransactionDetail";
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(model, out int typeExisted, out List<Guid> data);
            if (checkExistMessage.Length > 0)
            {
                if (data.Count > 0)
                {
                    var jobExisted = csTransactionService.Get(x => data.Contains(x.Id)).ToList();
                    return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage + " In " + string.Join(",", jobExisted.Select(x => x.JobNo)) });
                }
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            var hs = csTransactionDetailService.AddTransactionDetail(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model.Id };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {

            var hs = csTransactionDetailService.DeleteTransactionDetail(id, out List<ObjectReceivableModel> modelReceivableList);
            var message = hs.Success == true ? HandleError.GetMessage(hs, Crud.Delete) : hs.Message?.ToString();
            if (hs.Code == 403)
            {
                message = "Do not have permission";
            }

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            else
            {
                Response.OnCompleted(async () =>
                {
                    if (modelReceivableList.Count > 0)
                    {
                        await CalculatorReceivable(modelReceivableList);
                        //del edoc
                        edocService.DeleteEdocByHBLId(id);
                    }
                });
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [Route("Import")]
        public IActionResult Import(CsTransactionDetailModel model)
        {
            currentUser.Action = "ImportCSTransactionDetail";

            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(model, out int typeExisted, out List<Guid> data);
            if (checkExistMessage.Length > 0)
            {
                if (data.Count > 0)
                {
                    var jobExisted = csTransactionService.Get(x => data.Contains(x.Id)).ToList();
                    return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage + " In " + string.Join(",", jobExisted.Select(x => x.JobNo)) });
                }
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            model.UserCreated = currentUser.UserID;
            var result = csTransactionDetailService.ImportCSTransactionDetail(model);
            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(CsTransactionDetailModel model)
        {
            var currentHBL = csTransactionDetailService.First(x => x.Id == model.Id);
            currentUser.Action = "UpdateCSTransactionDetail";
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model, out int typeExisted, out List<Guid> data);
            if (checkExistMessage.Length > 0)
            {
                if (data.Count > 0)
                {
                    var jobExisted = csTransactionService.Get(x => data.Contains(x.Id)).ToList();
                    return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage + " In " + string.Join(",", jobExisted.Select(x => x.JobNo)) });
                }
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }

            string msgCheckUpdateMawb = CheckHasHBLUpdatePermitted(model);
            if (msgCheckUpdateMawb.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = msgCheckUpdateMawb });
            }

            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = csTransactionDetailService.UpdateTransactionDetail(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }

            Response.OnCompleted(async () =>
            {
                if (hs.Success)
                {
                    var handleStage = await csStageAssignedService.SetMutipleStageAssigned(currentHBL, null, model.JobId, model.Id, true);
                }

            });
            return Ok(result);
        }

        [HttpGet("CheckHwbNoExisted")]
        public IActionResult CheckHwbNoExisted(string hwbno, string jobId, string hblId)
        {
            bool existedHwbNo = false;
            var transaction = csTransactionService.Get(x => x.Id == new Guid(jobId))?.FirstOrDefault();
            if (transaction.TransactionType == TermData.AirExport || transaction.TransactionType == TermData.AirImport)
            {
                var data = csTransactionDetailService.GetDataHawbToCheckExisted();
                data = data.Where(x => x.TransactionType == transaction.TransactionType);
                if (hblId == null)
                {
                    if (data.Any(x => hwbno != DocumentConstants.NO_HOUSE && x.Hwbno == hwbno.Trim() && x.Hwbno != null))
                    {
                        existedHwbNo = true;
                    }
                    else
                    {
                        existedHwbNo = false;
                    }
                }
                else
                {
                    var transactionDetail = csTransactionDetailService.Get(x => x.Id.ToString() == hblId).FirstOrDefault();
                    if (transactionDetail.Hwbno == hwbno)
                    {
                        return Ok(false);
                    }
                    if (data.Any(x => hwbno != DocumentConstants.NO_HOUSE && x.Hwbno.Trim() == hwbno.Trim() && x.Id != new Guid(hblId)))
                    {
                        existedHwbNo = true;
                    }
                    else
                    {
                        existedHwbNo = false;
                    }
                }

            }
            return Ok(existedHwbNo);
        }

        [HttpGet("CheckHwbNoExistedAirExport")]
        public IActionResult CheckHwbNoExistedAirExport(string hwbno, string jobId, string hblId)
        {
            var transaction = csTransactionService.Get(x => x.Id == new Guid(jobId))?.FirstOrDefault();
            var data = csTransactionDetailService.GetDataHawbToCheckExisted();
            data = data.Where(x => x.TransactionType == transaction.TransactionType);
            var dataCheck = new List<CsTransactionDetailModel>();
            if (hblId == null)
            {
                if (data.Any(x => hwbno != DocumentConstants.NO_HOUSE && x.Hwbno == hwbno.Trim() && x.Hwbno != null))
                {
                    dataCheck = data.Where(x => hwbno != DocumentConstants.NO_HOUSE && x.Hwbno == hwbno.Trim() && x.Hwbno != null).ToList();
                }
            }
            else
            {
                var transactionDetail = csTransactionDetailService.Get(x => x.Id.ToString() == hblId).FirstOrDefault();
                if (transactionDetail.Hwbno == hwbno)
                {
                    return Ok(false);
                }
                if (data.Any(x => hwbno != DocumentConstants.NO_HOUSE && x.Hwbno.Trim() == hwbno.Trim() && x.Id != new Guid(hblId)))
                {
                    dataCheck = data.Where(x => hwbno != DocumentConstants.NO_HOUSE && x.Hwbno.Trim() == hwbno.Trim() && x.Id != new Guid(hblId)).ToList();
                }
            }
            return Ok(dataCheck.Select(x => x.JobNo).Distinct().ToList());
        }

        [HttpGet("GenerateHBLNo")]
        public IActionResult GenerateHBLNo(TransactionTypeEnum transactionTypeEnum)
        {
            var data = csTransactionDetailService.GenerateHBLNo(transactionTypeEnum);
            return Ok(new { hblNo = data });
        }

        [HttpGet("GenerateHBLSeaExport")]
        public IActionResult GenerateHBLSeaExport(string podCode)
        {
            string hblNo = csTransactionDetailService.GenerateHBLNoSeaExport(podCode);
            return Ok(new { hblNo = hblNo });
        }

        private string CheckExist(CsTransactionDetailModel model, out int existedType, out List<Guid> data)
        {
            string message = string.Empty;
            existedType = 0;
            data = new List<Guid>(); // các ID Job trùng.

            if (model.ParentId == null)
            {
                string shipmentTransactionType = csTransactionService.Get(x => x.Id == model.JobId).FirstOrDefault()?.TransactionType;
                //Chỉ check trùng HBLNo đối với các service khác hàng Air(Import & Export)
                List<Guid> masterBillIds = csTransactionService.Get(x => x.TransactionType.Contains(shipmentTransactionType.Substring(0, 1)))
                                                                .Where(x => x.CurrentStatus != DocumentConstants.CURRENT_STATUS_CANCELED)
                                                                .Select(x => x.Id).ToList();

                IQueryable<CsTransactionDetailModel> houseBills = csTransactionDetailService.Get(x => masterBillIds.Contains(x.JobId)).Where(x => x.ParentId == null);

                if (!string.IsNullOrEmpty(shipmentTransactionType) && shipmentTransactionType != TermData.AirImport && shipmentTransactionType != TermData.AirExport)
                {
                    if (model.Id == Guid.Empty)
                    {
                        if (houseBills.Any(x => x.Hwbno.ToLower() == model.Hwbno.ToLower() && x.OfficeId == currentUser.OfficeID))
                        {
                            message = string.Format(@"Housebill of Lading No is existed", model.Hwbno);

                            existedType = 1;
                            data = houseBills.Where(x => x.Hwbno.ToLower() == model.Hwbno.ToLower() && x.OfficeId == currentUser.OfficeID)
                                             .Select(x => x.JobId)
                                             .Distinct()
                                             .ToList();
                        }
                    }
                    else
                    {
                        if (houseBills.Any(x => x.Hwbno.ToLower() == model.Hwbno.ToLower() && x.OfficeId == currentUser.OfficeID && x.Id != model.Id))
                        {
                            message = string.Format(@"Housebill of Lading No is existed", model.Hwbno);
                            data = houseBills.Where(x => x.Hwbno.ToLower() == model.Hwbno.ToLower() && x.OfficeId == currentUser.OfficeID && x.Id != model.Id)
                                .Select(x => x.JobId)
                                .Distinct()
                                .ToList();
                            existedType = 1;
                        }
                    }
                }

                //Chỉ check trùng MAWB cùng 1 office.
                if (!string.IsNullOrEmpty(shipmentTransactionType) && !string.IsNullOrEmpty(model.Mawb))
                {
                    if (model.Id == Guid.Empty)
                    {

                        if (houseBills.Any(x => x.Mawb.ToLower() == model.Mawb.ToLower() && x.JobId != model.JobId && x.OfficeId == currentUser.OfficeID))
                        {
                            message = stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED, model.Mawb].Value;
                            data = houseBills.Where(x => x.Mawb.ToLower() == model.Mawb.ToLower() && x.JobId != model.JobId && x.OfficeId == currentUser.OfficeID)
                                .Select(x => x.JobId)
                                .Distinct()
                                .ToList();
                            existedType = 2;
                        }
                    }
                    else
                    {
                        if (houseBills.Any(x => x.Mawb.ToLower() == model.Mawb.ToLower() && x.JobId != model.JobId && x.OfficeId == currentUser.OfficeID && x.Id != model.Id))
                        {
                            message = stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED, model.Mawb].Value;
                            data = houseBills.Where(x => x.Mawb.ToLower() == model.Mawb.ToLower() && x.JobId != model.JobId && x.OfficeId == currentUser.OfficeID && x.Id != model.Id)
                              .Select(x => x.JobId)
                              .Distinct()
                              .ToList();
                            existedType = 2;
                        }
                    }
                }
            }
            return message;
        }

        [HttpPost("QueryData")]
        [Authorize]
        public IActionResult QueryData(CsTransactionDetailCriteria criteria)
        {
            var data = csTransactionDetailService.Query(criteria);
            return Ok(data);
        }

        [HttpPost("GetListHouseBillAscHBL")]
        [Authorize]
        public IActionResult GetListHouseBillAscHBL(CsTransactionDetailCriteria criteria)
        {
            var data = csTransactionDetailService.GetListHouseBillAscHBL(criteria);
            return Ok(data);
        }

        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(CsTransactionDetailCriteria criteria, int page, int size)
        {
            var data = csTransactionDetailService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpGet]
        [Route("GetGoodSummaryOfAllHblByJobId")]
        public IActionResult GetGoodSummaryOfAllHBLByJobId(Guid jobId)
        {
            var result = csTransactionDetailService.GetGoodSummaryOfAllHBLByJobId(jobId);
            return Ok(result);
        }
        /// <summary>
        /// preview proof
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PreviewProofOfDelivery")]
        [Authorize]
        public IActionResult ReviewProofOfDelivery(Guid id)
        {
            var result = csTransactionDetailService.PreviewProofOfDelivery(id);
            return Ok(result);
        }
        /// <summary>
        /// preview air proof
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PreviewAirProofOfDelivery")]
        [Authorize]
        public IActionResult ReviewAirProofOfDelivery(Guid id)
        {
            var result = csTransactionDetailService.PreviewAirProofOfDelivery(id);
            return Ok(result);
        }

        /// <summary>
        /// preview air document release
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("PreviewAirDocumentRelease")]
        public IActionResult ReviewAirDocumentRelease(Guid id)
        {
            var result = csTransactionDetailService.PreviewAirDocumentRelease(id);
            return Ok(result);
        }

        /// <summary>
        /// preview sea house bill of lading
        /// </summary>
        /// <param name="hblId">Id of Housebill</param>
        /// <param name="reportType"></param>
        /// <returns></returns>
        [HttpGet("PreviewSeaHBLofLading")]
        public IActionResult PreviewSeaHBLofLading(Guid hblId, string reportType)
        {
            var result = csTransactionDetailService.PreviewSeaHBLofLading(hblId, reportType);
            return Ok(result);
        }

        /// <summary>
        /// preview house airway bill lastest
        /// </summary>
        /// <param name="hblId">Id of Housebill</param>
        /// <param name="reportType"></param>
        /// <returns></returns>
        [HttpGet("PreviewHouseAirwayBillLastest")]
        public IActionResult PreviewHouseAirwayBillLastest(Guid hblId, string reportType)
        {
            var result = csTransactionDetailService.PreviewHouseAirwayBillLastest(hblId, reportType);
            return Ok(result);
        }

        /// <summary>
        /// preview attach list (Air)
        /// </summary>
        /// <param name="hblId">Id of Housebill</param>
        /// <returns></returns>
        [HttpGet("PreviewAirAttachList")]
        public IActionResult PreviewAirAttachList(Guid hblId)
        {
            var result = csTransactionDetailService.PreviewAirAttachList(hblId);
            return Ok(result);
        }

        [HttpGet("PreviewAirImptAuthorisedLetter")]
        public IActionResult PreviewAirImptAuthorisedLetter(Guid housbillId, bool printSign)
        {
            var result = csTransactionDetailService.PreviewAirImptAuthorisedLetter(housbillId, printSign);
            return Ok(result);
        }

        [HttpGet("AirImptAuthorisedLetter_Consign")]
        public IActionResult PreviewAirImptAuthorisedLetterConsign(Guid housbillId, bool printSign)
        {
            var result = csTransactionDetailService.PreviewAirImptAuthorisedLetterConsign(housbillId, printSign);
            return Ok(result);
        }

        /// <summary>
        /// Get data neutral hawb export
        /// </summary>
        /// <param name="housebillId"></param>
        /// <param name="officeId"></param>
        /// <returns></returns>
        [HttpGet("NeutralHawbExport")]
        public IActionResult NeutralHawbExport(Guid housebillId, Guid officeId)
        {
            var result = csTransactionDetailService.NeutralHawbExport(housebillId, officeId);
            return Ok(result);
        }

        /// <summary>
        /// Preview Booking Note
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("PreviewBookingNote")]
        [Authorize]
        public IActionResult PreviewBookingNote(BookingNoteCriteria criteria)
        {
            var result = csTransactionDetailService.PreviewBookingNote(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Update Input Booking Note 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateInputBKNote")]
        [Authorize]
        public IActionResult UpdateInputBKNote(BookingNoteCriteria criteria)
        {
            currentUser.Action = "UpdateInputBKNote";

            var hs = csTransactionDetailService.UpdateInputBKNote(criteria);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get housebill daily export
        /// </summary>
        /// <param name="issuedDate">Issued Date of housebill</param>
        /// <returns></returns>
        [HttpGet("GetHousebillsDailyExport")]
        public IActionResult GetHousebillsDailyExport(DateTime? issuedDate)
        {
            var data = csTransactionDetailService.GetHousebillsDailyExport(issuedDate);
            return Ok(data);
        }

        /// <summary>
        /// Get HAWB List Of Shipment with check Check point
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHAWBListOfShipment")]
        public IActionResult GetHAWBListOfShipment(Guid jobId, Guid? hblId)
        {
            var result = csTransactionDetailService.GetHAWBListOfShipment(jobId, hblId);
            return Ok(result);
        }

        private string CheckHasHBLUpdatePermitted(CsTransactionDetailModel model)
        {
            string errorMsg = string.Empty;
            string hblNo = string.Empty;
            List<string> advs = new List<string>();

            int statusCode = csTransactionDetailService.CheckUpdateHBL(model, out hblNo, out advs);
            if (statusCode == 1)
            {
                errorMsg = String.Format("HBL {0} has Charges List that Synced to accounting system, Please you check Again!", hblNo);
            }

            if (statusCode == 2)
            {
                errorMsg = String.Format("HBL {0} has  Advances {1} that Synced to accounting system, Please you check Again!", hblNo, string.Join(",", advs.ToArray()));
            }

            return errorMsg;
        }

        private async Task<HandleState> CalculatorReceivable(List<ObjectReceivableModel> model)
        {
            Uri urlAccounting = new Uri(apiServiceUrl.Value.ApiUrlAccounting);
            string accessToken = Request.Headers["Authorization"].ToString();

            HttpResponseMessage resquest = await HttpClientService.PutAPI(urlAccounting + "/api/v1/e/AccountReceivable/CalculateDebitAmount", model, accessToken);
            var response = await resquest.Content.ReadAsAsync<HandleState>();
            return response;
        }

        [HttpPut]
        [Route("UpdateFlightInfo")]
        [Authorize]
        public async Task<IActionResult> UpdateGenerateInfo(Guid id)
        {
            var hs = await csTransactionDetailService.UpdateFlightInfo(id);
            if (!hs.Success)
            {
                return BadRequest(hs);
            }
            return Ok(new ResultHandle { Status = hs.Success, Message = "Update Fight Info From Job Success" });
        }
    }
}
