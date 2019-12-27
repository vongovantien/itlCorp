using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
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
        public CsTransactionDetailController(IStringLocalizer<LanguageSub> localizer, ICsTransactionDetailService service, ICurrentUser user , ICsMawbcontainerService mawbcontainerService)
        {
            stringLocalizer = localizer;
            csTransactionDetailService = service;
            currentUser = user;
            containerService = mawbcontainerService;
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
        public IActionResult GetById(Guid Id)
        {
            CsMawbcontainerCriteria criteriaMaw = new CsMawbcontainerCriteria { Hblid = Id };
            var hbl = csTransactionDetailService.GetById(Id);
            var resultMaw = containerService.Query(criteriaMaw).ToList();
            if(resultMaw.Count() > 0) {
                hbl.CsMawbcontainers = resultMaw;
            }
            return Ok(hbl);
        }

        //[HttpGet]
        //[Route("GetHbDetails")]
        //public CsTransactionDetailModel GetHbDetails(Guid JobId,Guid HbId)
        //{
        //    return csTransactionDetailService.GetHbDetails(JobId,HbId);
        //}

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

        private string CheckExist(CsTransactionDetailModel model)
        {
            string message = string.Empty;
            if(model.Id == Guid.Empty)
            { 
                if (csTransactionDetailService.Any(x => x.Hwbno.ToLower() == model.Hwbno.ToLower()))
                {
                    message = "Housebill of Lading No is existed !";
                }
            }
            else
            {
                if (csTransactionDetailService.Any(x => x.Hwbno.ToLower() == model.Hwbno.ToLower() && x.Id != model.Id))
                {
                    message = "Housebill of Lading No is existed !";
                }
            }
            
            return message;
        }

        [HttpPost("QueryData")]
        public IActionResult QueryData(CsTransactionDetailCriteria criteria)
        {
            var data = csTransactionDetailService.Query(criteria);
            return Ok(data);
        }

        [HttpPost]
        [Route("Paging")]
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
        public IActionResult ReviewAirProofOfDelivery(Guid id)
        {
            var result = csTransactionDetailService.PreviewAirProofOfDelivery(id);
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
    }
}
