using System;
using System.Globalization;
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
        public CsTransactionDetailController(IStringLocalizer<LanguageSub> localizer, ICsTransactionDetailService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            csTransactionDetailService = service;
            currentUser = user;
        }

        [HttpGet]
        [Route("GetByJob")]
        public IActionResult GetByJob(Guid jobId)
        {
            CsTransactionDetailCriteria criteria = new CsTransactionDetailCriteria { JobId = jobId };
            return Ok(csTransactionDetailService.GetByJob(criteria));
        }

        [HttpGet]
        [Route("GetHbDetails")]
        public CsTransactionDetailModel GetHbDetails(Guid JobId,Guid HbId)
        {
            return csTransactionDetailService.GetHbDetails(JobId,HbId);
        }

        [HttpPost]
        [Route("addNew")]
        [Authorize]
        public IActionResult Add(CsTransactionDetailModel model)
        {
            //ChangeTrackerHelper.currentUser = currentUser.UserID;
            if (!ModelState.IsValid) return BadRequest();
            var checkExistMessage = CheckExist(model);
            if (checkExistMessage.Length > 0)
            {
                return BadRequest(new ResultHandle { Status = false, Message = checkExistMessage });
            }
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            model.UserCreated = currentUser.UserID;
            model.DatetimeCreated = DateTime.Now;
            var hs = csTransactionDetailService.AddTransactionDetail(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpDelete]
        [Route("delete")]
        [Authorize]
        public IActionResult Delete(Guid hblId)
        {
            var hs = csTransactionDetailService.DeleteTransactionDetail(hblId);
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
        [Route("update")]
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
                    message = stringLocalizer[LanguageSub.MSG_CODE_EXISTED].Value;
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

    //    [HttpGet("GetReport")]
    //    public CsTransactionDetailReport GetReport(Guid jobId)
    //    {
    //        var result = csTransactionDetailService.GetReportBy(jobId);
    //        return result;
    //}

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

        [HttpGet("GetGoodSummaryOfAllHBL")]
        public IActionResult GetGoodSummaryOfAllHBL(Guid HblId)
        {
            var result = csTransactionDetailService.GetGoodSummaryOfAllHBL(HblId);
            return Ok(result);
        }
    }
}
