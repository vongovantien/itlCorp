using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
        ICsMawbcontainerService containerService;
        ICsTransactionService csTransactionService;
        public CsTransactionDetailController(IStringLocalizer<LanguageSub> localizer, ICsTransactionDetailService service, ICurrentUser user , ICsMawbcontainerService mawbcontainerService, ICsTransactionService csTransaction)
        {
            stringLocalizer = localizer;
            csTransactionDetailService = service;
            currentUser = user;
            containerService = mawbcontainerService;
            csTransactionService = csTransaction;
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
            if(resultMaw.Count() > 0) {
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
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model);
            if (checkExistMessage.Length > 0)
            {
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
            var hs = csTransactionDetailService.DeleteTransactionDetail(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        [Route("Import")]
        public IActionResult Import(CsTransactionDetailModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            string checkExistMessage = CheckExist(model);
            if (checkExistMessage.Length > 0)
            {
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
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var hs = csTransactionDetailService.UpdateTransactionDetail(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("CheckHwbNoExisted")]
        public IActionResult CheckHwbNoExisted(string hwbno,string jobId, string hblId )
        {
            bool existedHwbNo = false;
            var transaction = csTransactionService.Get(x => x.Id == new Guid(jobId))?.FirstOrDefault();
            var data = csTransactionDetailService.GetDataHawbToCheckExisted();
            data = data.Where(x => x.TransactionType == transaction.TransactionType);
            if (transaction.TransactionType == TermData.AirExport || transaction.TransactionType == TermData.AirImport)
            {
                if(hblId == null)
                {
                    if (data.Any(x => x.Hwbno == hwbno && x.Hwbno != null))
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
                    var transactionDetail = csTransactionDetailService.Get(x => x.Id.ToString() == hblId ).FirstOrDefault();
                    if (transactionDetail.Hwbno == hwbno)
                    {
                        return Ok(false);
                    }
                    if (data.Any(x => x.Hwbno.Trim() == hwbno.Trim() && x.Id != new Guid(hblId)) )
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

        private string CheckExist(CsTransactionDetailModel model)
        {
            string message = string.Empty;
            if (model.ParentId == null)
            {
                var shipmentTransactionType = csTransactionService.Get(x => x.Id == model.JobId).FirstOrDefault()?.TransactionType;
                //Chỉ check trùng HBLNo đối với các service khác hàng Air(Import & Export)
                var masterBillIds = csTransactionService.Get(x=>x.TransactionType.Contains(shipmentTransactionType.Substring(0, 1))).Where(x=>x.CurrentStatus != "Canceled").Select(x=>x.Id).ToList();
                var houseBills = csTransactionDetailService.Get(x => masterBillIds.Contains(x.JobId)).Where(x=> x.ParentId != null);
                if (!string.IsNullOrEmpty(shipmentTransactionType) && shipmentTransactionType != TermData.AirImport && shipmentTransactionType != TermData.AirExport)
                {
                    if (model.Id == Guid.Empty)
                    {
                        if (houseBills.Any(x => x.Hwbno.ToLower() == model.Hwbno.ToLower()))
                        {
                            message = "Housebill of Lading No is existed !";
                        }
                    }
                    else
                    {
                        if (houseBills.Any(x => x.Hwbno.ToLower() == model.Hwbno.ToLower() && x.Id != model.Id))
                        {
                            message = "Housebill of Lading No is existed !";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(shipmentTransactionType) && !string.IsNullOrEmpty(model.Mawb))
                {
                    if(model.Id == Guid.Empty)
                    {

                        if (houseBills.Any(x => x.Mawb.ToLower() == model.Mawb.ToLower() && x.JobId != model.JobId ))
                        {
                            message = stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED].Value;
                        }
                    }
                    else
                    {
                        if (houseBills.Any(x => x.Mawb.ToLower() == model.Mawb.ToLower() && x.JobId != model.JobId && x.Id != model.Id ))
                        {
                            message = stringLocalizer[DocumentationLanguageSub.MSG_MAWB_EXISTED].Value;
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

        [HttpPost]
        [Route("PreviewSeaHBofLading")]
        public IActionResult PreviewSeaHBofLading(CsTransactionDetailModel model)
        {
            var result = csTransactionDetailService.Preview(model);
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
        public IActionResult PreviewAirImptAuthorisedLetter(Guid housbillId)
        {
            var result = csTransactionDetailService.PreviewAirImptAuthorisedLetter(housbillId);
            return Ok(result);
        }

        [HttpGet("AirImptAuthorisedLetter_Consign")]
        public IActionResult PreviewAirImptAuthorisedLetterConsign(Guid housbillId)
        {
            var result = csTransactionDetailService.PreviewAirImptAuthorisedLetterConsign(housbillId);
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
    }
}
